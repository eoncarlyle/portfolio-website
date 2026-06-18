---
title: A few thoughts from JavaOne 2026
date: 2026.04.12
---

# A few thoughts from JavaOne 2026

Last month SPS sent me and a staff engineer to [JavaOne](https://www.oracle.com/javaone/), the main Java language
conference held at the Oracle campus in Redwood City, California. It was a great experience and I got a lot out of it.
It is worth asking what the value of conferences is when there is no shortage of high-quality content online about
advancements in the language, JVM, and important libraries. But as Byrne Hobart
[points out](https://www.thediff.co/archive/conferences-as-industry-cluster-economics-squared/), by clustering a lot of
people in a small location for a couple of days you end up creating a miniature industry-specialised city with all the
implied nonlinear benefits. And while most (or at least many) of the JavaOne talks will end up on YouTube, the hallway
chatter and side conversations certainly won't be.

There is also something to be said for the curation value of conferences. Regardless of quality, anyone can publish a
blog post or video about some OpenJDK 26 performance improvement. But because there are only so many slots to present at
conferences like JavaOne, each talk had to have an answer for "why _this_ presentation and not another one?". This sorts
the wheat from the chaff.

<img src="/images/st-patricks-day-duke.jpg"
alt="Duke, the Java mascot, with the author on St. Patrick's Day"
style="max-width: 50%;" />

Not in any order in particular, here are a few takeaways from the conference:

1. Ron Pressler's "Principles of Memory Management" was excellent. The SPS staff engineer I went with explained that a
   lot of the talk was a rebuttal to JVM memory management criticism from various camps. One of the arguments made about
   generational GC was that CPU cost of garbage collection is proportional the product of the live set size and the
   allocation rate. The young generation in generational GC has a high allocation rate but a small live set and vice
   versa for the old generation, so GC is less CPU expensive by using these offsetting factors across each generation.
   Pressler also pointed out that if a program uses 100% of CPU it doesn't really matter how much memory it is using
   because no other programs can get scheduled by the OS. So if you can pay a price in memory to shorten the amount of
   time a program is hogging CPU, it is often a good trade.

2. John Rose's "How the JVM Optimizes Generic Code" is the best technical talk that I have ever seen in person. It isn't
   surprising that a JVM Senior Architect had something interesting to say about the JVM, but I was surprised by how
   good Rose's stage presence was - he is engaging and funny. His slides are available
   [here](https://cr.openjdk.org/~jrose/pres/202603-SpecializedGeneric.pdf), and his presentation used Quick Sort to
   measure how polymorphism decreases performance on code that uses generics. In C++ the compiler prepares specialised,
   static implementations for each required type while Java generics are more dynamic. While Java is roughly as
   performant as C++ on `int[]` Quick Sort, using reflection to support both `Integer[]` and `int[]` introduces only a
   slight performance penalty. But when standard generics are used on both `Long[]` and `Integer[]` there is a
   substantial performance penalty and an even higher one if the reflective version is used, as the number and cost of
   code paths increase substantially. Once three types are handled at runtime then performance falls off a cliff. I'm
   probably not doing this talk justice, but it was fantastic.

3. [Project Leyden](https://openjdk.org/projects/leyden/) is an OpenJDK project to "improve the startup time, time to
   peak performance, and footprint of Java programs". While true ahead-of-time (AOT) compilation is still in progress,
   the finished Leyden JEPs allow for capturing an AOT cache on a running application in order to do some class loading
   & linking ahead of time and capture profiles that can help the JIT compiler optimise faster. Netflix is using this in
   production for services with long startup times. Leyden requires the JVM you capture the cache from to be running on
   the same hardware and be on the same minor version as the JVM where the cache is used. To set the JIT up for success,
   you want the source JVM to operate under similar conditions as the target JVM. So to avoid a circular dependency,
   Netflix captures AOT caches from canary production deployments.

4. I talked with several presenters from Oracle and Netflix between talks; everyone was friendly, open to sharing
   expertise, and willing to answer questions. I asked a couple of people 'how do I better understand JVM internals' and
   the answers I got were reading the [_Garbage Collection Handbook_](https://gchandbook.org/), chapters 4 and 5 of the
   runtime spec, running the bytecode interpreter under a debugger, and reading everything that Aleksey Shipilëv has
   ever written about the JVM.

5. [Project Babylon](https://openjdk.org/projects/babylon/) is an OpenJDK project to run Java in more exotic places than
   a JVM running on a CPU, such as on a GPU or an FPGA. With respect to GPUs you _can_ inline CUDA C in existing Java
   programs, but it looks and sounds like a painful mess. In order to represent Java programs in a more target-agnostic
   way you need something easier to manipulate than an AST but something less tied to the JVM than compiled bytecode,
   and that is where Babylon code models come in; these roughly analogise to MLIR from the LLVM project. "Reflecting on
   HAT: A Project Babylon Case Study" included a neat demo of using Project Babylon to run a Conway's Game of Life
   implementation in Java on a GPU. I will be jealous of the first 'I used Babylon to program an FPGA in Java' Hacker
   News article, à la this
   [Haskeller who solved Advent of Code problems on an FPGA](https://midirus.com/blog/advent-of-fpga), that post won't
   come from me because I wouldn't even know how to begin with all that.

6. Taking conference notes in notebooks rather than on a laptop seems like an anachronism but because you can only write
   so fast you don't fall into 'transcription mode' when listening to a presenter, and you're also less distractable.
   Frontier models are good at OCR, so converting them to text really isn't much effort. And high quality materials 
   make a world of difference, I exclusively wrote in Maruman Mnemosyne steno notebooks with a Morning Glory 
   Pro Mach rollerball pen.
