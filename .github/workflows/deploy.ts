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

type Application = "zk" | "portfolio-application" | "zk-reverse-proxy";
type Enviornment = "prod" | "test";
type Colour = "blue" | "green";
type ReplicaNumber = "1" | "2" | "3";

const ADDRESS_BASE = "192.168.100";

const getCurrentColor = () => {};

const replaceColor = (
  application: Application,
  enviornment: Enviornment,
  newColor: Colour,
  replicaNumber: ReplicaNumber,
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

  return `${ADDRESS_BASE}${
    getApplicationOffset(application) +
    getEnviornmentOffset(enviornment) +
    getColourOffset(colour) +
    getReplicaOffset(replicaNumber)
  }`;
};

const getName = (
  application: Application,
  enviornment: Enviornment,
  colour: Colour,
  replicaNumber: ReplicaNumber,
) => {
  return application + enviornment + colour + replicaNumber;
};
