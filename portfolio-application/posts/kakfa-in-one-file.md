---
title: Kafka in One File
date: 2025.07.12
---

# Kafka in One File

On a piece of software whose lack of existence confuses me.

## A Love Letter to SQLite

When Redis's original BSD open-source licence was changed, Machine Learning Engineer Vicki Boykis mourned the occasion with "I love Redis with a loyalty that I reserve for close friends and family and the first true day of spring, because Redis is software made for me, the developer." While I have certainly worked with Redis far less than Boykis, this is exactly how I feel about SQLite. There may be more impactful software projects, but there is no software that I love so unreservedly as SQLite.

Despite how objectively great it is, most 'how to get started with relational databases' blogs and books don't use SQLite: when I was learning the basics of SQL I had to download a `.pkg` with MySQL with this clunky editor; it's probably still downloaded on my Mac, untouched since that holiday break I was using it.[^1] If you're setting up a new database for a project you'll have to provision compute, make sure it is available over the network, and secure it accordingly.

But with SQLite, you create a file and then run `sqlite3 myNewDatabase.sqlite` to set up your new tables. That's it. There's no daemon, no extra compute, no managed service. The daemonless, single file setup means you can check your database into version control or swap out between test and production with a single-line change. While it isn't as performant as PostgreSQL, the performance ceiling may be higher than you realise, emphasis mine:

> The SQLite website ([https://sqlite.org/](https://sqlite.org/)) uses SQLite itself, of course, and as of this writing (2015) it handles about *400K to 500K HTTP requests per day, about 15-20% of which are dynamic pages touching the database. Dynamic content uses about 200 SQL statements per webpage*. This setup runs on a single VM that shares a physical server with 23 others and yet still keeps the load average below 0.1 most of the time.

## Event Stream Woes & Motivation

A few weeks back I had the idea of standing up Apache Kafka behind a reverse proxy for use in a [side project](https://github.com/eoncarlyle/januaryplayground). I have plenty of compute in my home server rack, but it is all hidden behind a reverse proxy on a Digital Ocean VPS to avoid exposing my private IP address. The idea was to open the broker up to any IP address and secure it with mTLS. At first Nginx streams didn't work on the VPS, so I moved over to HAProxy, but I had this annoying issue where the certificate presented by the Kafka domain name was for an unrelated application on Nginx on the same DMZ server. Given that the reverse proxy went through a port-forwarding rule straight to port 9093 where Kafka was listening, I don't know what this was happening. I am a half decent Linux system administrator and this was a big part of my last job, but in the end I decided to give up and started running the broker directly on the VPS. I have considerably less CPU, RAM, and disk to work with, but at least mTLS works. It all reminded me of what it is like to set up a database from scratch and made me grateful that we are using a Kafka managed service at work.

It all made me think that surely _someone_ has created a high-quality, open-source "SQLite of Kafka" because that is exactly what I want. Given that SQLite fit my needs well as a database we're not talking about all that much data. But I find event streams interesting to work with given that they have useful qualities of a database, a write-ahead log, and a message queue. Much to my surprise, I haven't really found _anything_ that fits this bill, let alone some actively maintained, well-used open-source project.

This breaks my mental model for open source software and the types of projects that get built. I can't be the first person to want this, and there are a lot of software engineers out there who are more experienced and talented than I. There are no barriers to entry to making this, and there are non-zero rewards to reputational capital (and more importantly intrinsic rewards) for those who make a robust, respected solution. My best explanation for why this doesn't already exist is that there aren't many situations where a problem needs more than an append-only log, but an event stream over the network isn't appropriate. It is, frankly, a somewhat contrived problem. But it is certainly a simpler problem than a full relational database; chapter 3 of Travis Jeffery's excellent 'Distributed Services with Go' walks through writing a memory-mapped append-only log and based off of reading the chapter it seems a decent append-only log can be written in an afternoon. All in all, it is somewhat tempting to give this a whirl.

## How This Might Work

Any project worthy of the name must satisfy the following

- Like SQLite, constitute just a file format and a binary for interacting with the file format
- Event byte arrays for message keys and values, with consumers responsible for deserialisation
- Consumer message handlers run after events are fetched
- Support for multiple topics
- Support for consumer groups

A minimal log message would have to include keys, values, and the actual message payloads themselves. From what I have read, Kafka stores these as length prefixed values, so something like

```text
[4-byte total length] [2-byte key length] [key] [2-byte partition length] [partition] [value]
```

With the value size calculated from the total length left over after the key, partition, and their respective length bytes. However, because all messages are going onto one file, the topic would also need to be included. And just as Kafka stores consumer group offsets in `__consumer_offsets`, something similar could be done for consumer group offset persistence here. I don't see multi-partition support as a hard requirement; they could be used by different consumers in the same consumer group, but this would only be useful when the event production rate is faster than the single-partition event consumption rate.

Kafka's daemon allows for it to address obsolete event deletion, topic compaction, and many other background tasks even without active consumers or producers active, but 'Kafka in one file' would have to intersperse all of these background tasks during normal operation just as SQLite does. I have three big outstanding questions I have about what this would look like

1) Kafka uses periodic heartbeats from consumers to determine if they are still alive. While offset commits prove consumer health when there are new events to be consumed, but otherwise how can consumer liveness be proven?
2) Can obsolete record deletion be carried out without locking all other producers and consumers? It looks like that POSIX OSes may handle interleaved file appends, but that should be simpler than the deletes themselves
3) Kafka works by separate index and store files. While events are stored in the store file, the index file maps event offsets to their location in the store file. The index file is memory mapped, and is this

Just as most uses of SQLite call the C libraries through FFI, it would seem appropriate to do the same here with a similarly capable system programming language, and it seems like this would introduce more than a few interesting concurrency challenges.


### References

1) SQLite. Appropriate Uses For SQLite. Retrieved from [https://sqlite.org/whentouse.html](https://sqlite.org/whentouse.html)
2) Travis Jeffery. 2016. How Kafka's Storage Internals Work. Retrieved from [https://medium.com/the-hoard/how-kafkas-storage-internals-work-3a29b02e026](https://medium.com/the-hoard/how-kafkas-storage-internals-work-3a29b02e026)
3) Travis Jeffery. 2021.  _Distributed Services with Go_. Pragmatic Bookshelf, Raleigh, NC.
4) Vicki Boykis. 2024. Redis is forked. Retrieved from [https://vickiboykis.com/2024/04/16/redis-is-forked/](https://vickiboykis.com/2024/04/16/redis-is-forked/)


[^1]: The reasons for not using SQLite to teach relationship databases aren't terrible in that you want to teach people the platforms they will be using at larger enterprises, but early on those benefits are, I think, swamped by how lightweight SQLite is

[^2]: A record schema version would also make sense but could be omitted just to get the proof-of-concept going.
