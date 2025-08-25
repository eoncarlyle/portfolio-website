---
title: August Event Stream Notes
date: 2025.08.24
---
# August Event Stream Notes

## Kafka Consumer Group Offset Durability
A few weeks back I read Taylor Troesh's "How/Why to Sweep Async Tasks Under a Postgres Table". Not only does it show off how elegant the Postgres NPM package is, more importantly it shows some good patterns for using PostgreSQL in place of an event stream or message queue. Because I'm sympathetic to [arguments](https://vicki.substack.com/p/you-dont-need-kafka) that Kafka isn't a good application everywhere it is used, I was receptive to the message. In Troesh's post, he wrote "In my experience, transaction guarantees supersede everything else", which reminded me of my least favourite aspect of Kafka consumer groups.

For the uninitiated, Kafka consumer groups are best explained by an example. The event stream analogues to tables and shards in a relational database are topics and partitions respectively. Kafka consumer groups allow for multiple consumers to coordinate the consumption of events from a given topic [^multi-topic].  Let's say that a payment processor is placing all transaction attempts into a `card_tx_attempts` topic with four partitions. There may be many different services consuming from `card_tx_attempts`, including a service that records possibly fraudulent transactions for further investigation. If every fraud analysis service was consuming from `card_tx_attempts` as part of a `fraud_service` consumer group, the Kafka broker will guarantee two things:

1) Every partition in the `fraud_service` consumer group will have one and only one consumer
2) As many consumers will be active as possible

For example, if only a single consumer is initially in `fraud_service`, then the single consumer will be assigned all four `card_tx_attempts` partitions. If an additional consumer is added to `fraud_service` then the partition assignments will change as part of a partition balance, such that each consumer will be assigned two partitions for faster consumption. If an additional two consumers are added each `card_tx_attempts` partition will have one dedicated consumer, but any additional consumers will be idle given that each partition can only be assigned one partition. After a consumer in a consumer group fetches and processes a batch of records, the progress of the consumer groups is committed and recorded in the `__consumer_offsets` topic, so when consumer groups are restarted they can pick up at the record offset where they left off.

However, the durability of consumer group offsets is disappointing, which prompted me to email Troesh with a subject line of 'Validating your Kafka scepticism' earlier last month. If a new consumer joins a running consumer group and triggers a partition rebalance, the default Kafka behaviour does absolutely nothing to save progress inside of an event poll. If the consumer is polling 1000 events at a time and a rebalance occurs while it's processing the 999th event, you have a problem. As far as the broker is concerned none of those events were actually consumed by that consumer group; the consumer couldn't commit its progress before losing access to the partition. There is a `onPartitionsRevoked` in `ConsumerRebalanceListener` that can define a callback that runs before the consumer is dropped from a partition, but this requires you to _manually keep track of the events that you have processed_. While Kafka producers support transactions, `ConsumerConfig` provides no such configuration because Kafka transactions are not designed for consumers. From the Apache Kafka documentation: "Kafka transactions are a bit different from transactions in other messaging systems. In Kafka, the consumer and producer are separate, and it is only the producer which is transactional. It is however able to make transactional updates to the consumer's position (confusingly called the 'committed offset'), and it is this which gives the overall exactly-once behavior."

This may not even have been that bad of an oversight for the original use case at LinkedIn, but the great irony is that append-only write-ahead logs are the exact structure that relational databases use to make performant transactional guarantees. There doesn't seem to be a good way to get transactional guarantees from consumer groups; while I don't think it would be impossible to do I don't see how anyone can look at this behaviour and assume that Kafka was designed with all of this in mind.

## Further Thoughts on 'Kafka in One File'
In the last couple of weeks I've talked through some details about the idea from 'Kafka in One File' with a couple of people. The first thing I came to realise was that consumer groups shouldn't be necessary. It is easy to think of situations where it is worthwhile to split the consumption of a single topic across multiple threads and to make sure each event is only processed once. But with the producer, event stream, and consumer all on one host, it is more appropriate to push this responsibility to the consuming process. This would mean one consuming thread passing events to worker threads. The consumer would also need to store their equivalent of `__consumer_offsets`. This could either be stored on the stream itself or read from a key-value store.

The other thing I came to a conclusion on was how to best carry out the concurrency control side of things. It turns out that full-file locking is relatively portable between operating systems and runtimes: the Unix `fcntl` system call and the Win32 equivalent [^win32-locking] are relatively equivalent and are how file locking is accomplished in Java's `FileChannel#lock`, .NET `FileStream` constructors, and `FileExt::lock_exclusive` in the Rust `fs2` crate. My original idea was to follow the pattern from SQLite: write the clients in Rust and call them from other languages using foreign function interface, but this portable file locking would make it more viable to start prototyping in Kotlin first. This is especially true if I take pains to use Arrow result types and other Rust-like idioms.[^zero-allocation] If I ever saw a need for using the stream in F# I'd likely go down the Rust FFI path, but it sounds like an F# client calling `FileStream` in the right way would cooperate with a Kotlin process via `fcntl` and `LockFileEx` on Unix and Win32 respectively.

I'm not sure if I'll actually give this a shot, but it would be a nice reprieve from yet another side project that is some flavour of a REST API. If this thing is horribly non-performant, my guess would be that forgoing index files would be the issue, but it would be nice to keep this to one file.


### References

1) [Apache Kafka Documentations, Section 4.7](https://kafka.apache.org/documentation/#usingtransactions)
2) [Java Apache Kafka Client ConsumerConfig Documentation](https://kafka.apache.org/31/javadoc/org/apache/kafka/clients/consumer/ConsumerConfig.html)
3) [Java Apache Kafka Client ConsumerRebalanceListener Documentation](https://kafka.apache.org/28/javadoc/org/apache/kafka/clients/consumer/ConsumerRebalanceListener.html)
4) [Java Apache Kafka Client ProducerConfig Documentation](https://kafka.apache.org/23/javadoc/org/apache/kafka/clients/producer/ProducerConfig.html)
5) [Java NIO FileChannel Documentation](https://docs.oracle.com/javase/8/docs/api/java/nio/channels/FileChannel.html#lock--)
6) [Microsoft Learn Win32 LockFileEx Documentation](https://learn.microsoft.com/en-us/windows/win32/api/fileapi/nf-fileapi-lockfileex)
7) [Microsoft Learn .NET FileStream Constructors Documentation](https://learn.microsoft.com/en-us/dotnet/api/system.io.filestream.-ctor?view=net-9.0)
8) [NPM Postgres Package](https://www.npmjs.com/package/postgres)
9) [Rust fs2 Crate Documentation: FileExt Trait Documentation](https://docs.rs/fs2/latest/fs2/trait.FileExt.html#tymethod.lock_exclusive)
10) Taylor Troesh. 2024. "How/Why to Sweep Async Tasks Under a Postgres Table". [https://taylor.town/pg-task](https://taylor.town/pg-task)
11) William R. Stevens and Stephen A. Rago. 2013. Advanced I/O. In Advanced Programming in the UNIX Environment (3rd ed.). Addison-Wesley Professional, Upper Saddle River, NJ, USA.


[^zero-allocation]: Besides, this would give me an excuse to try writing Kotlin in a way that avoids heap allocation using object pools and other techniques, but I don't know exactly how well that would play with trying to make it as functional as possible.
[^win32-locking]: The Win32 equivalent is `LockFileEx` in `fileapi.h` but as best as I can tell this isn't a system call
[^multi-topic]: A consumer group can also be assigned multiple topics
