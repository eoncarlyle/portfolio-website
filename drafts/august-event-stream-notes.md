---
title: August Event Stream Notes
date: 2025.08.12
---
# August Event Stream Notes

## Further Thoughts on 'Kafka in One File'
In the last couple of weeks I've talked through some details about the idea from 'Kafka in One File' with a couple of people. The first thing I came to realise was that consumer groups shouldn't be neccesary. It is easy to think of situations where it is worthwhile to split the consumption of a single topic across mulitple threads and to make sure each event is only processed once. But with the producer, event stream, and consumer all on one host, it is more appropriate to push this responsibility to the consuming process. This would mean one consuming thread passing events to worker threads. The consumer would also need to store their equivalent of `__consumer_offsets`. This could either be stored on the stream itself or read from a key-value store.

The other thing I came to a conclusion on was how to best carry out the concurrency control side of things. It turns out that full-file locking is relatively portable between operating systems and runtimes: the Unix `fcntl` system call and the Win32 equivalent [^win32-locking] are relatively equivalent and are how file locking is accomplished in Java's `FileChannel#lock`, .NET `FileStream` constructors, and `FileExt::lock_exclusive` in the Rust `fs2` crate. My original idea was to follow the pattern from SQLite: write the clients in Rust and call them from other languages using foreign funciton interface, but this portable file locking would make it more viable to start prototyping in Kotlin first. This is especially true if I take pains to use Arrow result types and other Rust-like idioms.[^zero-allocation] If I ever saw a need for using the stream in F# I'd likely go down the Rust FFI path, but it sounds like an F# client calling `FileStream` in the right way would cooperate with a Kotlin process via `fcntl` and `LockFileEx` on Unix and Win32 respectively.

I'm not sure if I'll actually give this a shot but it would be a nice reprieve from yet another side project that is some flavour of a REST API. If this thing is horribly non-peformant, my guess would be that forgoing index files would be the issue, but it would be nice to keep this to one file.

## Kafka Consumer Group Progress Durability
A few weeks back I read Taylor Troesh's "How/Why to Sweep Async Tasks Under a Postgres Table". Not only does it show off how elegant the Postgres NPM package is, more importantly it shows some good patterns for using Postgres as a message queue. Because I'm sympathetic to [arguments](https://vicki.substack.com/p/you-dont-need-kafka) that Kafka isn't a good application everywhere it is used, I was receptive to the message. In Troesh's post, he wrote "In my experience, transaction guarantees supersede everything else", which immediately reminded me of my least favourite aspect of Kafka consumer groups.

- Kafka stream split into topics, and topics split into partitions.
- Consumer gorups allow for splitting work across multiple consuming processes while keeping process of consumed events
- The way that this works is that for each topic consumed by consumer group, each partition is assigned one and only one consumer
- For example, if a payment processor split customer transaction attempts into four partitions of a single topic, a set of stream consumers for detecting possibly fraudulent transactions could be tied to a dedicated consumer group
- By doing this, all consumers in the fraud process will be automatically assigned partitions to consume from by the Kafka broker
- For example, if only a single consumer is in the fraud consumer group at first, then it will be assigned all 4 partitions, but if an additional consumer in the same consumer group comes online then the partition assignments will change such that each consumer will be assigned two partitions for faster consumption. If an additional two consumers are added to the group then each partition will have one dedicated consumer, but subsequent consumers will be idle given that each partition can only have one consumer at a time

consumer group progress updates have poor transactional gaurentees.

### References

[Java Kafka Client ConsumerConfig Documentation](https://kafka.apache.org/31/javadoc/org/apache/kafka/clients/consumer/ConsumerConfig.html)

[Java Kafka Client ProducerConfig Documentation](https://kafka.apache.org/23/javadoc/org/apache/kafka/clients/producer/ProducerConfig.html)

[Java NIO FileChannel Documentation](https://docs.oracle.com/javase/8/docs/api/java/nio/channels/FileChannel.html#lock--)

[Microsoft Learn Win32 LockFileEx Documentation](https://learn.microsoft.com/en-us/windows/win32/api/fileapi/nf-fileapi-lockfileex)

[Microsoft Learn .NET FileStream Constructors Documentation](https://learn.microsoft.com/en-us/dotnet/api/system.io.filestream.-ctor?view=net-9.0)

[Rust fs2 Crate Documentation: FileExt Trait Documentation](https://docs.rs/fs2/latest/fs2/trait.FileExt.html#tymethod.lock_exclusive)

W. Richard Stevens and Stephen A. Rago. 2013. Advanced I/O. In Advanced Programming in the UNIX Environment (3rd ed.). Addison-Wesley Professional, Upper Saddle River, NJ, USA.

Taylor Troesh. 2024. "How/Why to Sweep Async Tasks Under a Postgres Table". [https://taylor.town/pg-task](https://taylor.town/pg-task)

[NPM Postgres Package](https://www.npmjs.com/package/postgres)

[^zero-allocation]: Besides, this would give me an excuse to try writing Kotlin in a way that avoids heap allocation using object pools and other techniques, but I don't know exactly how well that would play with trying to make it as functional as possible.
[^win32-locking]: The Win32 equivalent is `LockFileEx` in `fileapi.h` but as best as I can tell this isn't a system call
