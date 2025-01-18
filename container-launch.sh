# Create network
#sudo podman network create \
#  --driver bridge \
#  --subnet 192.168.100.0/24 \
#  --gateway 192.168.100.1 \
#  podman_bridge_rootfull

# Deploy ZooKeeper Ensemble
# Common settings for all ZK nodes
ZK_SERVERS="server.1=192.168.100.21:2888:3888;2181 \
server.2=192.168.100.22:2888:3888;2181 \
server.3=192.168.100.23:2888:3888;2181"

# ZK Node 1
sudo podman run -d \
  --name zk1 \
  --replace \
  --network podman_bridge_rootfull \
  --ip 192.168.100.21 \
  -e ZOO_MY_ID=1 \
  -e ZOO_SERVERS="$ZK_SERVERS" \
  docker.io/library/zookeeper:3.8

# ZK Node 2 
sudo podman run -d \
  --name zk2 \
  --replace \
  --network podman_bridge_rootfull \
  --ip 192.168.100.22 \
  -e ZOO_MY_ID=2 \
  -e ZOO_SERVERS="$ZK_SERVERS" \
  docker.io/library/zookeeper:3.8

# ZK Node 3
sudo podman run -d \
  --name zk3 \
  --replace \
  --network podman_bridge_rootfull \
  --ip 192.168.100.23 \
  -e ZOO_MY_ID=3 \
  -e ZOO_SERVERS="$ZK_SERVERS" \
  docker.io/library/zookeeper:3.8

# Check status of each node
for i in {1..3}; do
  sudo podman exec -it zk$i zkServer.sh status
done

# Deploy Reverse Proxies
for i in {1..2}; do
  sudo podman run -d \
    --name zk-reverse-proxy-$i \
    --replace \
    --network podman_bridge_rootfull \
    --ip 192.168.100.3$i \
    --tls-verify=false \
    localhost/zk-reverse-proxy:latest \
    "192.168.100.21:2181,192.168.100.22:2181,192.168.100.23:2181" 1080
done

# Deploy Portfolio Applications
for i in {1..2}; do
  sudo podman run -d \
    --name portfolio-application-$i \
    --replace \
    --network podman_bridge_rootfull \
    --ip 192.168.100.4$i \
    --tls-verify=false \
    localhost/portfolio-application:latest \
    "192.168.100.21:2181,192.168.100.22:2181,192.168.100.23:2181" \
    192.168.100.4$i 1080
done
