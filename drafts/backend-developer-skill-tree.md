---
title: A Rough Backend Developer Skill Tree
date: 2025.10.06
---

# A Rough Backend Developer Skill Tree

Last week an associate software engineer was asking me for advice on the types of side projects that would build
relevant skills in his backend role. In the process I talked through some of the books that I found the most helpful in
getting me to where I am now, and it forms something of a backend 'skill tree'. By 'backend' I mean server side software
engineering that writes to some persistent data store. As Patrick McKenzie wrote in his seminal essay
["Don't Call Yourself A Programmer, And Other Career Advice"](https://www.kalzumeus.com/2011/10/28/dont-call-yourself-a-programmer/),
an awful lot of software engineering jobs.

## Building Blocks

Regardless of if you're starting inside or outside of a computer science program, the first few steps are basically the
following

1) Learn the basics of eiter Python or Type/JavaScript
2) Get comfortable with Git, Bash, and SQL [^boykis]
3) Get a foundation in data structures and algorithms
   With the possible exception of SQL this true pretty much across all of software engineering; if someone is dead set
   on
   working on embedded systems maybe they'd go straight to C, but this is how I'd reccommend anyone get their start in
   software engineering.

For those starting out on their own it makes more sense to pick one of the big interpreted languages and try to learn
it well before moving on to other things. Most university computer science programs will expose the learner to a
greater variety of langagues in the first two years, but in a classroom setting this is good if for no other reason
than after the third langauge you start to gain an intuition for the difference between meaningful and less meaningful
difference between languages. [^parallel] As far as picking betwen Python and Type/JavaScript, both have thier flaws
but are good first langauges. JavaScript certainly has it's quirks, but you often can't get away from it when working on
backend systems because a lot of server-side programs are communicating with _some_ user interface and that user
interface is often some web application. TypeScript taken the langauge much further, but until type-stripping is more
mature in Node it  may be too much to ask a complete beginner to handle the 'well actually all these types the compiler 
is telling you about actually don't exist at runtime' for someone who is just grasping what 'runtime' means. Maybe the 
choice should be Type/JavaScript if you want to pickup anything on the client side, and Python more interested in data 
& analytics. I started with Python because my first big exposure to programming was scientific computing, but 
TypeScript's curly braces and static types appeal to me more.

What is true regardless of picking this up inside or outside a computer science program is the importance of
Git, SQL, and working on the command line.

# Books and Skills

- CompTIA Network+: https://www.amazon.com/CompTIA-Network-Study-2025-2026-Bundle/dp/B0FQKJM6FG
- REST application basic
    - Flask Tutorial https://blog.miguelgrinberg.com/post/the-flask-mega-tutorial-part-i-hello-world
    - Web Development with Node and
      Express https://learning.oreilly.com/library/view/web-development-with/9781492053507/
- Domain modelling made functional https://pragprog.com/titles/swdddf/domain-modeling-made-functional/
- SQL Anti-patterns: https://pragprog.com/titles/bksqla/sql-antipatterns/
- Data Intensive Applications: (Skip chapters
  2-4) - https://learning.oreilly.com/library/view/designing-data-intensive-applications/9781491903063/
- Database Internals: https://learning.oreilly.com/library/view/database-internals/9781492040330/
- Little Book of Semaphores: https://greenteapress.com/semaphores/LittleBookOfSemaphores.pdf
- Operating Systems: https://pages.cs.wisc.edu/~remzi/OSTEP/

# Other Notes

- Tough call between picking up Python and JavaScript first (TypeScript, packaging considerations)
- Probably cannot get away from some frontend
- Easy to learn too many languages and frameworks
    - Learn one of Python or Type/JavaScript
    - One statically typed garbage collected langauge is enough: if you pick up any of Java, C#, and Go then the others
    - Learn on functional langauge (F# is my first recommendation)
    - Learn a langauge that requires manual memory management (or Rust)
      aren't the most worthwhile to learn, a lot of line of business applications
- Patio11: https://www.kalzumeus.com/2011/10/28/dont-call-yourself-a-programmer/

[^pedantic]: CSCI 1113 taken in place but, well, technicalities
[^parallel]: This has parallels for other technologies: Different relational databases may have small syntactic
differences (`LIMIT` in PostgreSQL vs. `FETCH FIRST` in Oracle SQL) that aren't crazy meaningful, but SQLite doesn't
have dedicated schema in the same way. The
[^boykis]: This isn't an original observation; I got that trio from a
[Vicki Boykis](https://vickiboykis.com/2022/01/09/git-sql-cli/) blog post. As Boykis points out you don't need to
reach terminal expertise in all three, but they are crucially important in any backend job. While jobs that don't
involve database work won't require much SQL, I can't think of a single software development job that wouldn't require
use of Git and command-line tools.
