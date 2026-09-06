---
title: A Wasted Weekend on Kafka
date: 2026.08.31
---

# A Wasted Weekend on Kafka
Back when I was at SPS I wanted to get a better handle on Apache Kafka, so I spun up a single-broker cluster on my Digital Ocean VPS. As I wrote in <a href="/post/february-2026-puro-notes">_Kafka in One File_</a>, I originally wanted to run Kafka out of my homelab DMZ VLAN in a way that was still accessible from the wider internet with mTLS authentication. It is a bad idea to have internet accessible Kafka for true production infrastructure, but while working the <a href="/pdf/2025-TechJam-BearTerritory.pdf">Bear Territory</a> stock exchange model I wanted Kafka access even if I wasn't in my home network.

Unfortunately I could never get the reverse proxies to work, so as a temporary measure I ran Kafka directly on my VPS with its measly single gigabyte of RAM. It worked fine last year for Bear Territory, but given that I had gone to the trouble of setting up a cluster I decided to start writing my Nginx logs to a new topic. Admittedly most of the traffic to my public facing domains is from bots (some benign, others not), but I still wanted to know what traffic I was getting on which articles.

However, there is an obvious problem with running Kafka with less than 1Gb of memory. Multiple times a month Kafka would take down my `new-york-0` VPS, and this is what I would see in Grafana:

<img src="/images/kafka-vps-woes.png" alt="Prometheus chart showing Node exporter CPU usage reaching 100% before reporting stopped altogether on the host" style="max-width: 75%;" />

Traffic on this broker wasn't crazy, with maybe 10 records that averaged ~850 bytes in size. It isn't entirely clear to me what is happening here: I'd have to re-read the internals chapter of the [O'Reily Kakfa Book](https://learning.oreilly.com/library/view/kafka-the-definitive/9781492043072/) to jog my memory but my best guess is that older segments are being compressed and written to disk. At some point I should try to recreate this on a virtual machine and grab some Java Flight Recordings to know for sure. But the gap in the `new-york-0` metrics shows that the VPs was so RAM constrained that it couldn't serve Prometheus metrics, let alone anything else I run on that VPS including this website.

When my wife was away at a bachlorette party in early August I had my opportunity to setup a proper broker that could be reached securely outside my network. I don't have anything going directly to my homelab without being reached by a VPS, and this would be no exception. I also wanted the VPS _itself_ to access the broker, because that is where the producer for my Nginx log topics lives. 

Kafka clients, both inside and outside the network, refer to the cluster with `broker-0.santa-cruz-kafka-1.iainschmitt.com:10000` and `broker-1.santa-cruz-kafka-1.iainschmitt.com:10001`. Following along with the table below, these A records resolve to my VPS, but those same records resolve to my homelab IP _on_ the VPS using `/etc/hosts`. 

| Host                       | `broker-0` resolves to | `broker-1` resolves to | Commentary                                             |
|----------------------------|------------------------|------------------------|--------------------------------------------------------|
| Public DNS                 | `161.35.106.161`       | `161.35.106.161`       | VPS's public IP, different ports for different brokers |
| VPS `/etc/hosts`           | `198.51.100.25`[^ip]   | `198.51.100.25`        | Homelab WAN IP                                         |
| santa-cruz (broker-0 host) | `192.168.6.14`         | `192.168.6.17`         | DMZ LAN IP                                             |
| san-diego (broker-1 host)  | `192.168.6.14`         | `192.168.6.17`         | DMZ LAN IP                                             |

DNS is only part of the picture, because the traffic needs to go from the VPS to the two brokers in my homelab. The first hop is taken care of by port forwarding rules that direct all traffic on ports 10000 and 10001 to my homelab WAN IP.

```
$ sudo firewall-cmd --zone=public --list-forward-ports
port=10000:proto=tcp:toport=10000:toaddr=198.51.100.25
port=10001:proto=tcp:toport=10001:toaddr=198.51.100.25
```

My homelab IP is assigned to my pfSense router, so one more set of port forwarding rules is needed. Requests to port 10000 are forwarded to port 9093 for the host with the first broker, with port 10001 request forwarded to the same destination port on the second broker's host.

```console
$ sudo pfctl -s nat | grep 9093
rdr on mvneta0 inet proto tcp from <PublicReverseProxies> to 198.51.100.25 port = 10000 -> 192.168.6.14 port 9093
rdr on mvneta0 inet proto tcp from <PublicReverseProxies> to 198.51.100.25 port = 10001 -> 192.168.6.17 port 9093
```

Each broker a Podman container running Kafka; below is a part of the `docker-compose.yml` file for the first broker, on `192.168.6.14`. The `KAFKA_LISTENERS` and `KAFKA_ADVERTISED_LISTENERS` were a little finicky.[^broker-1] The `KAFKA_LISTENERS` line lists all the URIs the broker will listen on, and is pretty normal with the exception that port 9093 uses SSL. `KAFKA_ADVERTISED_LISTENERS` lists the URIs that Kafka will advertise to other clients and brokers. If a request comes from a URI that isn't spelled out on this list, the broker will ignore it. The `192.168.6.14:9092` and `192.168.6.14:9095` are only ever used by the second broker at `192.168.6.17`; it would be silly to route inter-broker traffic through the entire internet, so as implied by those addresses this traffic doesn't leave the DMZ. But note that the advertised URI for clients is `broker-0.santa-cruz-kafka-1.iainschmitt.com` on port 10000 rather than 9093. Original requests from clients, both on the VPC and elsewhere, use `broker-0.santa-cruz-kafka-1.iainschmitt.com:10000` and `broker-1.santa-cruz-kafka-1.iainschmitt.com:10001`, and those original URIs need to match an advertised address rather than the `:9093` used internally in the port forwarding chain.

```yaml
services:
  kafka:
    image: docker.io/confluentinc/cp-kafka:8.2.2
    container_name: kafka-broker-0
    ports:
      - "9092:9092"
      - "9093:9093"
      - "9094:9094"
      - "9095:9095"
    environment:
      KAFKA_NODE_ID: 0
      KAFKA_PROCESS_ROLES: 'broker,controller'
      KAFKA_CONTROLLER_QUORUM_VOTERS: '0@192.168.6.14:9094,1@192.168.6.17:9094'
      KAFKA_CONTROLLER_LISTENER_NAMES: 'CONTROLLER'
      KAFKA_LISTENERS: 'PLAINTEXT://0.0.0.0:9092,SSL://0.0.0.0:9093,CONTROLLER://0.0.0.0:9094,REPLICATION://0.0.0.0:9095'
      KAFKA_ADVERTISED_LISTENERS: 'PLAINTEXT://192.168.6.14:9092,SSL://broker-0.santa-cruz-kafka-1.iainschmitt.com:10000,REPLICATION://192.168.6.14:9095'
      KAFKA_LISTENER_SECURITY_PROTOCOL_MAP: 'PLAINTEXT:PLAINTEXT,SSL:SSL,CONTROLLER:PLAINTEXT,REPLICATION:SSL'
```
This took an embarrassing amount to get working, so much so that I would have probably done something else that weekend (computing or otherwise) if I had known what a hassle it was. This was something of a 'play stupid games, win stupid prizes' because the network requirements are a little bespoke. My job hunt earlier this year put [Puro](https://github.com/eoncarlyle/puro) development into something of a hiatus, but this is another example of a place where it would be nice to have an embedded event stream.

[^ip]: I used an RFC 5737 IP address in lieu of my actual IP address for reasons obvious enough not to explain
[^broker-1]: The second broker at `92.168.6.17` had an equivalent compose file. `container_name` and `KAFKA_NODE_ID` were `kafka-broker-1` and `1` respectively. `KAFKA_ADVERTISED_LISTENERS` URIs used `192.168.6.17:909x` and `broker-1.santa-cruz-kafka-1.iainschmitt.com:10001`