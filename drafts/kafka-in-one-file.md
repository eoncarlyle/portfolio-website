# Kafka in one file

When Redis's original BSD open-source liscences was revoked, Machine Learning Engineer Vicki Boykis mourned the occasion with "I love Redis with a loyalty that I reserve for close friends and family and the first true day of spring, because Redis is software made for me, the developer." While I have certainly worked with Redis far less than Boykis, this is a good description for how I feel about SQLite. There may be more impactful software projects, but there is no software that I love so unreservedly as SQLite.

Most 'how to get started with relational databases' blogs and books don't use SQLite, case in point When I was learning the basics of SQL I had to download a .pkg MySQL with this clunky editor, and it's probably still downloaded on my Mac, untouched since that holiday break I was using it.[^1] If you're setting up the database for a side project is more cumbersome, because you have to provision compute, make sure it is accessible over the network, and secure it accordingly. But with SQLite, one file. That's it. No daemon, no extra compute, no managed service. The daemonless, single file setup means you can check your database into version control or swap out between test and production with a single-line change. While it isn't as performant as PostgreSQL, the performance ceiling may be higher than you realise, emphasis mine:

> The SQLite website (https://sqlite.org/) uses SQLite itself, of course, and as of this writing (2015) it handles about *400K to 500K HTTP requests per day, about 15-20% of which are dynamic pages touching the database. Dynamic content uses about 200 SQL statements per webpage*. This setup runs on a single VM that shares a physical server with 23 others and yet still keeps the load average below 0.1 most of the time.

Not only can you check your database into version control
- Can commit database into source control


SQLite doesn't require a seperate daemon and exists as a single file. I would love the event stream equivalent of this.
- https://sqlite.org/wal.html
- All Kafka is doing is handling byte arrays and making consumers handle the rest
- Consumer groups would be a real pain, I would probably literally make this a SQLite problem
  - After all, SQLite WAL requires a seperate file anyway!
- Note that tasks that would be background tasks in other databases are inline with requests in SQLite
- Length prefixing: probably model off of the Kafka file format?
- https://web.stanford.edu/~ouster/cs111-spring23/assign_logfs/
- https://users.cms.caltech.edu/~donnie/cs122/projects/Assignment6.pdf


> Q: The consumer groups database doesn't need to be a full database it really just needs to be a key/value store, but it does need to be persistent. And I don't want to gate every single consumer group write to a SQLite update if that makes sense. It seems rather hubristic to try to write a key/value store with SQLite level gaurentees
> A: You're absolutely right! You don't need SQLite-level guarantees for consumer offsets - you just need "don't lose my place when I crash" reliability. And you definitely don't want to bottleneck your high-throughput event writes on SQLite transactions. For a simple persistent key/value store, you have some nice middle-ground options: Append-only offset log: Just append consumer_group:stream_id:offset entries to a simple file. On startup, replay the log to build an in-memory map. Periodically compact it. This is what many streaming systems do internally.

## Need to have
- Topics
- Message keys

## Don't _need_ to have
- Deletion/Compaction: will be a real pain
- Consumer groups
  - Persistence is a pain and should almost certainly done in SQLite if I opted for this, doesn't break the spirit of 'Kafka in one file'
  - Unless the stream itself would be used, which could be tricky
- Partitions: only reason these would be useful is in partition-level consumers


## References
https://vickiboykis.com/2024/04/16/redis-is-forked/

[^1]: The reasons for not using SQLite to teach relationship databases aren't terrible in that you want to teach people the platforms they will be using at larger enterprises, but early on those benefits are, I think, swamped by how lightweight SQLite is
