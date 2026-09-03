---
title: A Wasted Weekend on Kafka
date: 2026.08.31
---

# A Wasted Weekend on Kafka
Back when I was at SPS I wanted to get a better handle on Apache Kafka, so I spun up a single-broker cluster on my Digital Ocean VPS. As I wrote in <a href="/post/february-2026-puro-notes">_Kafka in One File_</a>, I originally wanted to run Kafka out of my homelab DMZ VLAN in a way that was still accessible from the wider internet with mTLS authentication. It is a bad idea to have internet accessible Kafka for true production infrastructure, but while working the <a href="/pdf/2025-TechJam-BearTerritory.pdf">Bear Territory</a> stock exchange model I wanted Kafka access even if I wasn't in my home network.

Unfortunately I could never get the reverse proxies to work, so as a temporary measure I ran Kafka directly on my VPS with its measly single gigabyte of RAM. It worked fine last year for Bear Territory, but given that I had gone to the trouble of setting up a cluster I decided to start writing my Nginx logs to a new topic. Admittedly most of the traffic to my public facing domains is from bots (some benign, others not), but I still wanted to know what traffic I was getting on which articles.

However, there is an obvious problem with running Kafka with less than 1Gb of memory. Multiple times a month Kafka would take down my `new-york-0` VPS, and this is what I would see in Grafana:

<img src="/images/kafka-vpc-woes.png"
alt="Prometheus chart showing Node exporter CPU usage reaching 100% before reporting stopped altogether on the host"
style="max-width: 75%;" />

Traffic on this broker wasn't crazy, with maybe 10 records that averaged ~850 bytes in size. It isn't entirely clear to me what is happening here: I'd have to re-read the internals chapter of the [O'Reily Kakfa Book](https://learning.oreilly.com/library/view/kafka-the-definitive/9781492043072/) to jog my memory but my best guess is that older segments are being compressed and written to disk. At some point I should try to recreate this on a virtual machine and grab some Java Flight Recordings to know for sure. But the gap in the `new-york-0` metrics shows that the VPC was so RAM constrained that it couldn't serve Prometheus metrics, let alone anything else I run on that VPC including this website.

When my wife was away at a bachlorette party in early August I had my opportunity to setup a proper broker that could be reached securely outside my network. I don't have anything going directly to my homelab without being reached by a VPC, and this would be no exception. I also wanted the VPC _itself_ to access the broker, because that is where the producer for my Nginx log topics lives. This took a thoroughly embarrassing amount of time, so much so that I would have probably done something else that weekend (computing or otherwise) if I had known what a hassle it was. This was something of a 'play stupid games, win stupid prizes' because the network requirements are a little bespoke.

Kafka clients, both inside and outside the network, refer to the cluster with `broker-0.santa-cruz-kafka-1.iainschmitt.com:10000` and `broker-1.santa-cruz-kafka-1.iainschmitt.com:10001`. Following along with the table below, these A records resolve to my VPC, but those same records resolve to my homelab IP _on_ the VPC using `/etc/hosts`. 

| Host                       | `broker-0` resolves to | `broker-1` resolves to | Commentary                                             |
|----------------------------|------------------------|------------------------|--------------------------------------------------------|
| Public DNS                 | `161.35.106.161`       | `161.35.106.161`       | VPC's public IP, different ports for different brokers |
| VPC `/etc/hosts`           | `198.51.100.25`[^ip]   | `198.51.100.25`        | Homelab WAN IP                                         |
| santa-cruz (broker-0 host) | `192.168.6.14`         | `192.168.6.17`         | DMZ LAN IP                                             |
| san-diego (broker-1 host)  | `192.168.6.14`         | `192.168.6.17`         | DMZ LAN IP                                             |

No matter where the request is coming from it needs to actually reach the broker on my homelab, which is taken care of with a port forwarding rule for each broker.

```
$ sudo firewall-cmd --zone=public --list-forward-ports
port=10000:proto=tcp:toport=10000:toaddr=198.51.100.25
port=10001:proto=tcp:toport=10001:toaddr=198.51.100.25
```

The homelab IP itself is assigned to my pfSense router, so one more set of port forwarding rules are needed to get the requests to my DMZ hosts.

```console
$ sudo pfctl -s nat | grep 9093
rdr on mvneta0 inet proto tcp from <PublicReverseProxies> to 198.51.100.25 port = 10000 -> 192.168.6.14 port 9093
rdr on mvneta0 inet proto tcp from <PublicReverseProxies> to 198.51.100.25 port = 10001 -> 192.168.6.17 port 9093
```


> Two structural decisions worth calling out explicitly since they're the "why," not just the "what": splitting `SSL` vs `REPLICATION` listeners (so inter-broker traffic never leaves the LAN even though client traffic legitimately needs to), and `advertised.listeners` being an unverified assertion the broker hands to clients rather than something derived from the container's actual bind config.

[^ip]: I used an RFC 5737 IP address in lieu of my actual IP address for reasons obvious enough not to explain