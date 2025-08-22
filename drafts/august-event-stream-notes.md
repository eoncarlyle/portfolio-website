---
title: August Event Stream Thoughts
date: 2025.08.12
---

# Kafka in One File: Further Thoughts
- Consumer groups are unneccessary: it is not hard to imagine keeping a group of consumer threads consistent while distributing work between them, but that should be handled by the consuming thread
- Some `.lock` file for locking on readers, writers
  - https://docs.rs/fs2/latest/fs2/trait.FileExt.html#tymethod.lock_exclusive
  - https://learn.microsoft.com/en-us/dotnet/api/system.io.filestream.-ctor?view=net-9.0#system-io-filestream-ctor(system-string-system-io-filemode-system-io-fileaccess-system-io-fileshare-system-int32-system-boolean)
  - Unix Syscall: `fcntl`, Win32: https://learn.microsoft.com/en-us/windows/win32/api/fileapi/nf-fileapi-lockfileex
  -

# Works Cited
W. Richard Stevens and Stephen A. Rago. 2013. Advanced I/O. In Advanced Programming in the UNIX Environment (3rd ed.). Addison-Wesley Professional, Upper Saddle River, NJ, USA.

[Microsoft Learn Win32 LockFileEx Documentation](https://learn.microsoft.com/en-us/windows/win32/api/fileapi/nf-fileapi-lockfileex)

[Microsoft Learn .NET FileStream Constructors Documentation](https://learn.microsoft.com/en-us/dotnet/api/system.io.filestream.-ctor?view=net-9.0#system-io-filestream-ctor(system-string-system-io-filemode-system-io-fileaccess-system-io-fileshare-system-int32-system-boolean))

[Rust fs2 Crate Documentation: FileExt Trait Documentation](https://docs.rs/fs2/latest/fs2/trait.FileExt.html#tymethod.lock_exclusive)

[Java FileChannel NIO Documentation](https://docs.oracle.com/javase/8/docs/api/java/nio/channels/FileChannel.html#lock--)
