---
title: My Rough and Incomplete Backend Developer Skill Tree
date: 2025.10.06
---

# My Rough and Incomplete Backend Developer Skill Tree

A few weeks back an associate software engineer was asking me for advice on the types of side projects that would build
relevant skills in his backend role. In the process I talked through some of the books that I found the most helpful in
getting me to where I am now, and it forms something of a backend 'skill tree'. By 'backend' I mean server side software
engineering that writes to some persistent data store. As Patrick McKenzie wrote in his seminal essay
["Don't Call Yourself A Programmer, And Other Career Advice"](https://www.kalzumeus.com/2011/10/28/dont-call-yourself-a-programmer/),
an awful lot of software engineering jobs.

# The Books

Regardless of if you're starting inside or outside of a computer science program, the first few steps are basically the
following.

1. Learn the basics of eiter Python or Type/JavaScript
2. Get comfortable with Git, Bash, and SQL [^boykis]
3. Get a foundation in data structures and algorithms

With the possible exception of SQL this true pretty much across all of software engineering; if someone is dead set on
working on embedded systems maybe they'd go straight to C, but this is how I'd recommend anyone get their start in
software engineering. After taking care of these building blocks, these are the books that I would read

1. [_Web Development with Node and Express_](https://learning.oreilly.com/library/view/web-development-with/9781492053507/)
   in JavaScript or
   [_Flask Web Development_](https://learning.oreilly.com/library/view/flask-web-development/9781491991725/) for Python.
   Both of these books teach how to build server side web applications that handle HTTP requests, template HTML, and
   write to a database. The Express and Flask frameworks are relatively simple, allowing the reader to focus on things
   that will be transferable to other systems. It can be much more intimidating to pickup something like Java's Spring
   framework right out of the gate even if you already know the language. Spring or .NET's ASP.NET core are very
   powerful and have a lot of features that make enterprise development easier, but they aren't the first server side
   framework one should learn. At the end of reading either book the reader should be able to standup a simple web
   application, be it a personal website or something that makes REST calls with a client for interactivity.

2. [_CompTIA Network+ Certification Exam Guide_](https://a.co/d/7ZiCa1J) chapters 1 and 6-12. While it may not come up
   every day, if you write server-side software you need to have a good mental model for exactly what happens when you
   type 'www.google.com' into a browser address bar. Otherwise, you will be at the mercy of what you do not understand.
   This is a book meant for the CompTIA Network+ certification exam, the likes of which are much more important in
   network engineering as compared to software engineering. While more advanced cert exams will be more specific to
   given network vendor equipment, this book is a great overview breaking down the OSI model, TCP/IP, routing, DNS, and
   other important network basics.

3. [_SQL Anti-patterns_](https://pragprog.com/titles/bksqla/sql-antipatterns/). Not nearly enough books on computing
   walk through an example of an understandable mistake before explaining the right way to do things - but that is _all_
   that this book does. This book has 25 short, pretty self-contained chapters. As it says on the tin, each of these
   works through a common relational database mistake. After getting one's feet wet with SQL in a project or two this
   could both correct some bad habits and help the reader recognise bad SQL when the see it down the line.

4. [_Domain Modeling Made Functional_](https://pragprog.com/titles/swdddf/domain-modeling-made-functional/). Domain
   modeling is the process of turning the capabilities and requirements of a system into a tractable model, often
   something like a UML diagram. The model is meant to be understandable to domain experts such that someone in
   operations could look at a domain model for their company and say 'this looks right, but it's missing the part with
   volume-based shipping discounts'. As I wrote in a [review](https://iainschmitt.com/post/ddmf-review), the book isn't
   as detailed as other domain modelling books that I've read, but it makes up for it by being far more readable. The
   'functional' in _Domain Modeling Made Functional_ makes this book somewhat unique, as the language used in the book
   is F#. But no prior knowledge of the language is used and much of the book transfers over well to other languages.

5. [_Data Intensive
   Applications_](https://learning.oreilly.com/library/view/designing-data-intensive-applications/9781491903063/. This
   book is about the general problems that you face in applications where I/O is a more meaningful bottleneck than CPU
   performance. Many server-side applications now have more persistence than a relational database, be they search
   engines, event streams, or dedicated caches. _Data Intensive Applications_ does an incredible job at teaching the
   details of on-disk storage, distributed persistence challenges, and batch vs. stream processing that are broadly
   applicable across a variety of data persistence technologies. Chapters 5 through 9 are the best explanation of
   distributed systems that I've ever read. It can be dense material, but I've never read better prose from a technical
   book.

6. [Database Internals](https://learning.oreilly.com/library/view/database-internals/9781492040330/). The _Data
   Intensive Applications_ book gave an introduction to database internals that left me more curious. I haven't made it
   all the way through the book, but it is a good read after working with relational databases for a couple of years
   especially because I never had the chance to take a database class at school. This was where I finally understood how
   write-ahead logs were used to make transactions more durable without sacrificing performance.

7. [Little Book of Semaphores](https://greenteapress.com/semaphores/LittleBookOfSemaphores.pdf). Semaphores are a way to
   coordinate concurrent threads or processes, and while concurrency isn't something that comes up every day it is
   important to understand how these problems are solved. The book is also generally fun to work through, which to be
   honest is the real reason I have it on this list. As I wrote in a
   [post](https://www.iainschmitt.com/post/first-thoughts-on-lbs-and-rust), the book probably works better in classroom
   settings as it can sometimes be hard to tell if the reader's solutions to the problems are equivalent to the solution
   manual. But I've had good results with asking Claude 'I am trying to learn this in greater detail, please ask me
   questions to probe my understanding rather than just telling me if my solution is equivalent'. More than any of these
   other books, the reader has to do the exercises to get much out of this book.

8. [Operating Systems: Three Easy Pieces](https://pages.cs.wisc.edu/~remzi/OSTEP/). I only made it through about 1/3 of
   Tanenbaum's 'Modern Operating Systems' and while I got a lot out of it, _Three Easy Pieces_ is a more appropriate
   first book on operating systems. Modern cloud infrastructure does a lot to try to abstract away the responsibilities
   of the OS, but as implied by
   ['The Cloud Is Just Someone Else’s Computer'](https://blog.codinghorror.com/the-cloud-is-just-someone-elses-computer/),
   _some_ OS _somewhere_ is still doing roughly the same thing to serve your production applications as what takes place
   when running the same application locally. As for network engineering, you don't want to be at the mercy of what you
   don't understand about operating systems.

# Aside on Languages

It is easy to learn too many languages and frameworks, which wastes valuable time re-learning how to do something you
already know how to do when you could have learned something new. meaningfully different. I'm not 100% sure on this, but
there is a case to be made that you only need to pickup four languages:

1. One of the aforementioned big interpreted languages: Python or Type/JavaScript
2. A statically typed, garbage-collected language: Java, C#, or Go
3. A language where you have to manually manage memory: Probably C. Maybe you can include Rust in this category, but Zig
   would be a decent choice after it the 1.0 release
4. A functional language. I am biased to F#. Haskell is a great langauge with a steep learning curve, and Scala can
   sometime face mixed OO/FP paradigm issues.

C# and Go are great languages, but they are ultimately too similar to Java to justify me learning them. Someone who
learned C# first should say the equivalent. The wrinkle in this list is for the interpreted languages. Almost all web
applications are in Java/TypeScript and the language can still be used server side, but JS has it's quirks and Python's
plotting, analytics, and ML libraries make the language worth learning. Maybe you just can't get away without learning
both.

[^boykis]:
    This isn't an original observation; I got that trio from a
    [Vicki Boykis](https://vickiboykis.com/2022/01/09/git-sql-cli/) blog post. As Boykis points out you don't need to
    reach absolute expertise in all three, but they are crucially important in any backend job.
