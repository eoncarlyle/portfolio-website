/*

A few changes
- Proper health checks in NGINX
- Dedicated blue-green color swapping for both application and in the reverse proxy
  - Sometimes running deployment will be green, sometimes running deployment will be blue
  - May have some ambiguities if both are running, but I suppose that this can be handled through Nginx
  - As soon as new colour is healthy and passes health check, drop it's counterpart number from NGINX
- Use Node scripts to handle the deployment
  - Declare constants, functions to get a given IP range for enviornment/application/colour/number
  - Use command-line-args and some strict types

- This is going to require briefly taking all of the services down
  - Must fix to the 'blue' color
  - Need to match the naming scheme

-

```
upstream backend {
    server 192.168.100.31:1080 max_fails=1 fail_timeout=10s;
    server 192.168.100.32:1080 max_fails=1 fail_timeout=10s;
    check interval=3000 rise=2 fall=5 timeout=1000 type=http;
    check_http_send "HEAD / HTTP/1.0\r\n\r\n";
    check_http_expect_alive http_2xx http_3xx;
}
```

*/
import { readFile, writeFile } from "fs/promises";
import { promisify } from "util";
import child_process from "child_process";
import { exit } from "process";
const exec = promisify(child_process.exec);

const applications = [
  "zk",
  "portfolio-application",
  "zk-reverse-proxy",
] as const;
const environments = ["prod", "test"] as const;
const colour = ["blue", "green"] as const;
const replicaNumber = ["1", "2", "3"] as const;

type Application = (typeof applications)[number];
type Enviornment = (typeof environments)[number];
type Colour = (typeof colour)[number];
type ReplicaNumber = (typeof replicaNumber)[number];

const ADDRESS_BASE = "192.168.100";
const PORTFOLIO_APP_PORT = 1080;
const REVERSE_PROXY_PORT = 1080;

// Query the Nginx conf files too?
const getCurrentColor = async (enviornment: Enviornment): Promise<Colour> => {
  const { stdout, stderr } = await exec("sudo podman ps --format '{{.Names}}'");

  const blueApplicationCount = applications
    .map((application) => `${application}-${enviornment}-"blue"`)
    .map((slug) => stdout.includes(slug)).length;

  const greenApplicationCount = applications
    .map((application) => `${application}-${enviornment}-"green"`)
    .map((slug) => stdout.includes(slug)).length;

  if (
    blueApplicationCount === applications.length &&
    greenApplicationCount === 0
  ) {
    return "blue";
  } else if (
    greenApplicationCount === applications.length &&
    blueApplicationCount === 0
  ) {
    return "green";
  } else {
    console.error(
      `Illegal applications state with ${blueApplicationCount} blue applications and ${greenApplicationCount} green applications`,
    );
    console.error(stdout);
    exit(1);
  }
};

// c.match(/{([^}]+)}/g

// This should be staged: first add the new
const replaceReverseProxyNginx = async (
  enviornment: Enviornment,
  newColor: Colour,
  replicaNumber: ReplicaNumber,
) => {
  const nginxConfName =
    enviornment === "prod" ? "iainschmitt.conf" : "iainschmitt-test.conf";
  const nginxConfPath = `/etc/nginx/conf.d/${nginxConfName}`;
  const nginxConfFile = await readFile(nginxConfPath, "utf8");

  const oldAddress = getAddress(
    "zk-reverse-proxy",
    enviornment,
    getReverseColour(newColor),
    replicaNumber,
  );
  const newAddress = getAddress(
    "zk-reverse-proxy",
    enviornment,
    newColor,
    replicaNumber,
  );

  await writeFile(nginxConfPath, nginxConfFile.replace(oldAddress, newAddress));
};

// Use the templates, use conf names, use health checks, need to get creative about existing IPs
const addReverseProxyNginx = async (
  enviornment: Enviornment,
  incomingColour: Colour,
) => {};

// Use the templates, use conf names, use health checks, need to get creative about existing IPs
const removeReverseProxyNginx = async (
  enviornment: Enviornment,
  outgoinColour: Colour,
) => {};

const getAddress = (
  application: Application,
  enviornment: Enviornment,
  colour: Colour,
  replicaNumber: ReplicaNumber,
) => {
  const getApplicationOffset = (application: Application) => {
    if (application === "zk") return 0;
    else if (application === "portfolio-application") return 10;
    else return 20;
  };

  const getEnviornmentOffset = (enviornment: Enviornment) => {
    if (enviornment === "prod") return 20;
    else return 50;
  };

  const getColourOffset = (color: Colour) => {
    if (color === "blue") return 0;
    else return 4;
  };

  const getReplicaOffset = (replicaNumber: ReplicaNumber) => {
    return parseInt(replicaNumber);
  };

  return `${ADDRESS_BASE}.${
    getApplicationOffset(application) +
    getEnviornmentOffset(enviornment) +
    getColourOffset(colour) +
    getReplicaOffset(replicaNumber)
  }`;
};

const getContainerName = (
  application: Application,
  enviornment: Enviornment,
  colour: Colour,
  replicaNumber: ReplicaNumber,
) => {
  return `${application}-${enviornment}-${colour}-${replicaNumber}`;
};

const getReverseColour = (color: Colour): Colour =>
  color === "green" ? "blue" : "green";

const startZooKeeperContainer = async (
  enviornment: Enviornment,
  colour: Colour,
  replicaNumber: ReplicaNumber,
) => {
  const zkServers = `server.1=${getAddress("zk", enviornment, colour, "1")}:2888:3888;2181 \
                    server.2=${getAddress("zk", enviornment, colour, "2")}:2888:3888;2181 \
                    server.3=${getAddress("zk", enviornment, colour, "3")}:2888:3888;2181`;

  const cmd = `sudo podman run -d \
                --name ${getContainerName("zk", enviornment, colour, replicaNumber)} \
                --replace \
                --network podman_bridge_rootfull \
                --ip ${getAddress("zk", enviornment, colour, replicaNumber)} \
                -e ZOO_MY_ID=${replicaNumber} \
                -e ZOO_SERVERS="${zkServers}" \
                docker.io/library/zookeeper:3.8`;

  return await exec(cmd);
};

const getZkConnectString = (enviornment: Enviornment, colour: Colour) => {
  const zk1 = getAddress("zk", enviornment, colour, "1");
  const zk2 = getAddress("zk", enviornment, colour, "2");
  const zk3 = getAddress("zk", enviornment, colour, "3");
  return `${zk1}:2181,${zk2}:2181,${zk3}:2181`;
};

const startReverseProxies = async (
  enviornment: Enviornment,
  colour: Colour,
  replicaNumber: ReplicaNumber,
) => {
  const cmd = `sudo podman run -d \
                --name ${getContainerName("zk-reverse-proxy", enviornment, colour, replicaNumber)} \
                --replace \
                --network podman_bridge_rootfull \
                --ip ${getAddress("zk-reverse-proxy", enviornment, colour, replicaNumber)} \
                --tls-verify=false \
                localhost/zk-reverse-proxy:latest \
                "${getZkConnectString(enviornment, colour)}" ${REVERSE_PROXY_PORT}`;

  return await exec(cmd);
};

const startPortfolioApplication = async (
  enviornment: Enviornment,
  colour: Colour,
  replicaNumber: ReplicaNumber,
) => {
  const cmd = `sudo podman run -d \
                --name ${getContainerName("portfolio-application", enviornment, colour, replicaNumber)} \
                --replace \
                --network podman_bridge_rootfull \
                --ip ${getAddress("portfolio-application", enviornment, colour, replicaNumber)} \
                --tls-verify=false \
                localhost/portfolio-application:latest \
                "${getZkConnectString(enviornment, colour)}" \
                ${getAddress("portfolio-application", enviornment, colour, replicaNumber)} ${PORTFOLIO_APP_PORT}`;

  return await exec(cmd);
};

const stopContainer = async (
  application: Application,
  enviornment: Enviornment,
  colour: Colour,
  replicaNumber: ReplicaNumber,
) => {
  await exec(
    `sudo podman stop ${getContainerName(application, enviornment, colour, replicaNumber)}`,
  );
};

const restartNginx = async () => {
  try {
    await exec(`sudo systemctl restart nginx`);

    const status = await exec(`systemctl is-active nginx`);

    if (status.stdout.trim() !== "active") {
      throw new Error(`Service 'nginx' failed to start properly`);
    }
  } catch (error) {
    console.error(`Failed to restart 'nginx'`, error.message);
    process.exit(1);
  }
};
