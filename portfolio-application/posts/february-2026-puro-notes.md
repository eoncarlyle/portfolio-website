---
title: February 2026 Puro Notes
date: 2026.02.27
---

# February 2026 Puro Notes

Last July I wrote the following in <a href="/post/kafka-in-one-file">_Kafka in One File_</a>:

> It all made me think that surely _someone_ has created a high-quality, open-source "SQLite of Kafka" because that is
> exactly what I want. Given that SQLite fit my needs well as a database we're not talking about all that much data. But
> I find event streams interesting to work with given that they have useful qualities of a database, a write-ahead log,
> and a message queue. Much to my surprise, I haven't really found _anything_ that fits this bill, let alone some
> actively maintained, well-used open-source project.

I've been sporadically working on [Puro](https://github.com/eoncarlyle/puro), my attempt at an 'SQLite of Kafka'. Puro
is a Kotlin program with event-stream like semantics stored on a local filesystem, rather than being distributed like
Kafka, Kinesis, or equivalents. A Kafka broker stores each partition of each topic that it serves as a set of segment
files, with one active segment file receiving new records from producers. In contrast, Puro has no daemon or broker;
consumers and producers use file locking to control access to the active segment. Readers acquire shared locks to read
existing records on the active segment; producers acquire an exclusive lock to the region of the file after the end of
the existing bytes before writing new records. There aren't partitions in Puro, and all topics are placed onto the same
segment. By running on a single filesystem there aren't benefits to consumer groups that can't be reproduced by a
consumer thread handing off messages to specific worker threads. Log compaction isn't very practical without a daemon,
but iterating over stale segments would allow something roughly equivalent.

There is still a lot of work to do, I haven't completed the work on repairing failed writes or started the work on
rollover to a new active segment. Once everything is 'working' there will be no shortage of performance fixes. Despite
being a JVM program, I'm trying to allocate as little as I can on the heap; I've internalised the arguments by Casey
Muratori and the TigerBeetle team to this end.

Working with binary serialisation is the main challenge of the program, and one that I don't have a ton of experience
with. When each record is just a series of bytes, detection and recovery of bad writes is a challenge. And if a producer
writes to a segment immediately after an incomplete write, consumers won't be able to tell where the bad write stops and
the good write begins. At first, I had some convoluted logic for consumers to detect write failures and zero out their
corrupted records. Consumers are using filesystem APIs to listen to reads; by tying the deserialied messages to certain
offsets the consumer can piece together when bad writes started. But a consumer that started consuming after the bad
write wouldn't have the required history to piece this together. A consumer consuming from the beginning of the segment
wouldn't have an issue here, but I wanted consumers to be able to start from the latest record. There is an arguably
greater challenge of consumers not being able to tell the difference between a healthy write and an unhealthy write in
the absence of any producer locks.

The solution came from a principal engineer at work, who suggested the last action a producer makes during a write is
flipping a signal bit. The way I implemented this was with special write-block start and write-block end messages; a
consumer that encounters a low signal bit will relinquish the read lock for a delay, and will wait until the bit is
high. The write-block end message shows the length of a successful write, allowing producers to check signal bits. If a
producer encounters a low signal bit, it will zero out the corrupted message before writing its own messages. Given that
producers are responsible for detecting bad writes they will have to iterate down the entire length of the active
segment to make sure the existing segment is sound before producing messages themselves, but it feels more appropriate
to make the producers responsible for this.

The most _annoying_ part of the project is working with the Java `ByteBuffer` class, which is hard to avoid when working
with the Java NIO APIS. A `ByteBuffer` instance stores the position of the next byte to read or write to, so just
reading an instance and iterating through the contents mutates state. My most common bug is forgetting to rewind a
buffer, leading the program to think I have fewer bytes than I actually do in a data structure. I almost want some means
to automatically rewind any buffer once it leaves the scope of a function, but I'm not sure if it could be done cleanly
in Kotlin. It was a really confusing day when I assumed that `public ByteBuffer put(int index, byte[] src)` advanced the
buffer position just like `public abstract ByteBuffer put(byte b)` did before seeing 'The position of this buffer is
unchanged' in the relevant JavaDocs. I suppose you don't need the buffer to keep track of its own position if you know
the length of `src` at the outset. That one really threw me for a loop.

## References

1. [Apache Kafka 4.2.X Documentation: Implementation](https://kafka.apache.org/42/implementation/)
2. Gwen Shapira, Todd Palino, Rajini Sivaram, and Krit Petty. 2021. Kafka Internals. In Kafka: The Definitive Guide (2nd
   ed.). O'Reilly Media, Sebastopol, CA. ISBN 978-1-492-04307-2. I/O. In Advanced Programming in the UNIX Environment
   (3rd ed.). Addison-Wesley Professional, Upper Saddle River, NJ, USA.
3. [Software Unscripted Episode #78](https://shows.acast.com/software-unscripted/episodes/664fde448c77cc0013b33390)
4. [TigerStyle: TigerBeetle Style Guide](https://github.com/tigerbeetle/tigerbeetle/blob/main/docs/TIGER_STYLE.md)
5. William R. Stevens and Stephen A. Rago. 2013. Advanced I/O. In Advanced Programming in the UNIX Environment (3rd
   ed.). Addison-Wesley Professional, Upper Saddle River, NJ, USA.
