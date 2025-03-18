---
title: First Thoughts <em>The Little Book of Semaphores</em> and Rust
date: 2025.03.17
---

# First Thoughts on _The Little Book of Semaphores_ and Rust

In a my <a href="/post/ddmf-review">review</a> of _Domain Modeling Made Functional_, I made the following comment about
Rust:

> This has made me realise that I would much rather learn a new language by diving into a problem area that it is well equipped to work in: rather than just learn Rust, I'd rather do a deep dive in concurrency and learn Rust in the process.

This was what I briefly thought about a principal developer at work was in the process of talking me out of learning C. He argued that if I wanted to learn C for systems programming experience I'd be better served by Rust, but if C interoperability and experience with manual memory management was a bigger priority then Zig would be the right choice. While I was ultimately convinced, this was disappointing. I haven't finished all of either Stevens & Rago's _Advanced Programming in the UNIX Environment_ or _Operating Systems: Three Easy Pieces by Arpaci-Dusseau & Arpaci-Dusseau_ but especially for someone who never took an Operating Systems class both works motivate the readerto pickup C programming.[^hyphenation] It's fun to see a function signature and then look in man pages to answer questions non provided in the text. If I remember correctly, I told the engineer I was talking to 'I want to spend more time in man pages and less time building data-transfer-objects in yet another CRUD application'. _The Little Book of Semaphores_ came from a list of Dan Luu's recommended programming books. [^luu] This wasn't my first time trying to learn concurrency; I got a decent amount out of the first six chapters of Goetz et al's _Java Concurrency in Practice_ if my O'Reily online reading history is to be trusted. [^goetz] But the Goetz book can be dry and naturally is focused on Java, while Downey's book is both language-agnostic and available for free. I figured it was as good as any to start the ball rolling with learning more about concurrency using Rust.

But Allen Downey's _The Little Book of Semaphores_ is different from most concurrency books in that it spends all of ten pages on the background of sempahores before getting into synchronisation problems. Initially I thought there was a typo in PDF when there was an entire blank page after the first problem statement; both problems and solutions are written in a Python-like pseudocode. Downey doesn't explain much about the format of the desired solution. My solution to the ballroom dance queue problem in 3.8 initially used a literal queue data structure gaurded by a mutex, but figuring this out forced me to more explicitly learn a 'one thread per actor' model for concurrent problems. The first three chapters have been a joy to go through and my ['Learning Rust'](https://github.com/eoncarlyle/learning-rust) repository is where I've put my solutions. The problems are rewarding to go through and you spend far more time writing solutions than reading text. Given that all content is langauge agnostic and requires very few special langague features, pretty much any langauge that supports shared-memory concurrency at runtime would be appropriate to write solutions for. _The Little Book of Semaphores_ is one of few technical books I've read that comes across as a better self-teaching aide than a textbook. How relatively vauge the problems are set up makes me unsure of how well it is to lecture off of, but the relatively vauge problems and a 'LLM-as-code-reviewer' made it great for my purposes.

As far as the langauge is concerned, it's a pretty good one. It has a mix of langague features that I really like including `Option<T>` in leiu of null poiters, ML style pattern matching, and language-level support for `Result<T, E>`. The semaphore exercises haven't required any crazy lifetime annotations, and compiler errors strictly about lifetimes are pretty clear. Ultimately the program is allocating and freeing the heap _somehow_ and at least so far I haven't faced a situation where I was confused about why the compiler thought a piece of dynamic allocated memory wasn't available. Every time I didn't understand the compiler message I tried to explain my problem or misunderstanding as best as I could to Claude, with the explicit instructions to 'Provide minimal code examples: I want to understand this concept, don't hesitate to ask me questions or probe to build my understanding'. The README of my 'Learning Rust' repository has a log of these questions and answers such as the following:

> ### Question
> When using `handles.iter().for_each(|handle| handle.join().unwrap());` in place of the for loop, the build error
> `rustc: cannot move out of *handle which is behind a shared reference`
> was provided. Why is the iterator different than the for loop? I would have thought the ownership was clear?
>
> ### Answer
> The problem is that `handles.iter()` provides shared `&JoinHandle<()>` references to the handles but does not grant ownership of them.

It is easy to see the value of LLMs as search engines that can interpolate between queries, but this is pretty good evidence in favour of Bryne Hobart's argument that 'AI Ruins Education the way Pulleys Ruin Powerlifting'. [^hobart]

Because the same semaphore needs to be shared across threads I ended up using Rusts' atomic reference counting pointer, `Arc<T>`, in every solution so far. McNamara's _Rust in Action_ describes this smart pointer as "Rust's ambassador. it can share values across threads, gaurenteeing that these will not interfere with each other". While the semaphore is acquired and released by different threads, the semaphore state is handled by concurrency primitives within the semaphore struct. I expected a little more of a fight from the Rust compiler, but the same ceramony is required for `Semaphore.acquire()` and adding an element to a collection contained in a mutex. Speaking of the semaphores themselves, I was a little surprised to learn that Rust doesn't have them in the standard library so I just used Sean Chen's implementation of them. [^chen]

```fsharp
let problem_3_8_thread
    (internal_sem: Semaphore)
    (external_sem: Semaphore)
    (dancer_list: Queue<String>)
    (label: String)
    =
    Thread(fun () ->
        while true do
            if dancer_list.Count <> 0 then
                Console.WriteLine $"{label} thread waiting"
                toggleSem internal_sem Release
                toggleSem external_sem Wait
                Console.WriteLine $"Dancer: {dancer_list.Dequeue()}")
```


[^hyphenation]: I really thought about providing Remzi H. Arpaci-Dusseau and Andrea C. Arpaci-Dusseau's names as
'Professors Arpaci-Dusseu' as is done for the plural 'attorneys general'; this would have been more fun but less clear

[^luu]: [Luu, D. 2016. Programming book recommendations and anti-recommendations.](https://danluu.com/programming-books/)

[^goetz]: Goetz, B. et al 2006. _Java Concurrency in Practice_. Addison-Wesley Professional, Upper Saddle River, NJ.

[^downey]: Downey, A. B. 2016. The Little Book of Semaphores (2nd ed.). Green Tea Press.

[^mcnamara]: McNamara, T. 2021. Rust in Action. Manning Publications Co., Shelter Island, NY.

[^chen]: [Chen S. 2020. Implementing Synchronization Primitives in Rust: Semaphores.](https://seanchen1991.github.io/posts/sync-primitives-semaphores/)

[^hobart]: [Hobart B. 2024. AI Ruins Education the way Pulleys Ruin Powerlifting.](https://www.thediff.co/archive/ai-ruins-education-the-way-pulleys-ruin-powerlifting?ref=iainschmitt.com)
