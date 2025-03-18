---
title: First Thoughts <em>The Little Book of Semaphores</em> and Rust
date: 2025.03.17
---

# First Thoughts on _The Little Book of Semaphores_ and Rust

In my <a href="/post/ddmf-review">review</a> of _Domain Modeling Made Functional_, I made the following comment about
Rust:

> This has made me realise that I would much rather learn a new language by diving into a problem area that it is well equipped to work in: rather than just learn Rust, I'd rather do a deep dive in concurrency and learn Rust in the process.

This was what I briefly thought about as a principal developer at work talked me out of learning C. He argued that if I wanted to learn C for systems programming experience I'd be better served by Rust, but if C interoperability and experience with manual memory management was a bigger priority then Zig would be the right choice. While I was ultimately convinced, this was disappointing. I haven't finished all of either Stevens & Rago's _Advanced Programming in the UNIX Environment_ or _Operating Systems: Three Easy Pieces by Arpaci-Dusseau & Arpaci-Dusseau_ but especially for someone who never took an Operating Systems class both works motivate the reader to pickup C programming.[^hyphenation] It's fun to see a function signature and then look in man pages to answer questions not provided in the text. If I remember correctly, I told the engineer I was talking to 'I want to spend more time in man pages and less time building data-transfer-objects in yet another CRUD application'. _The Little Book of Semaphores_ came from a list of Dan Luu's recommended programming books. [^luu] This wasn't my first time trying to learn concurrency; I got a decent amount out of the first six chapters of Goetz et al. _Java Concurrency in Practice_ if my O'Reily online reading history is to be trusted. [^goetz] But the Goetz book can be dry and naturally is focused on Java, while Downey's book is both language-agnostic and available for free. I figured it was as good as any to start the ball rolling with learning more about concurrency using Rust.

But Allen Downey's _The Little Book of Semaphores_ is different from most concurrency books in that it spends all of ten pages on the background of semaphores before getting into synchronisation problems. Initially I thought there was a typo in PDF when there was an entire blank page after the first problem statement; both problems and solutions are written in a Python-like pseudocode. Downey doesn't explain much about the format of the desired solution. My solution to the ballroom dance queue problem in 3.8 initially used a literal queue data structure guarded by a mutex, but figuring this out forced me to more explicitly learn a one-thread-per-actor model for concurrent problems. The first three chapters have been a joy to go through and my ['Learning Rust'](https://github.com/eoncarlyle/learning-rust) repository is where I've put my solutions. The problems are rewarding to go through, and you spend far more time writing solutions than reading text. Given that all content is language agnostic and requires very few special language features, pretty much any language that supports shared-memory concurrency at runtime would be appropriate to write solutions for. _The Little Book of Semaphores_ is one of few technical books I've read that comes across as a better self-teaching aide than a textbook. How relatively vague the problems are set up makes me unsure of how well it is to lecture off of, but the relatively vague problems and a 'LLM-as-code-reviewer' made it great for my purposes.

As far as the language is concerned, it's a pretty good one. It has a mix of language features that I really like including `Option<T>` in lieu of null pointers, ML style pattern matching, and language-level support for `Result<T, E>`. The semaphore exercises haven't required any crazy lifetime annotations, and compiler errors strictly about variable RAII patterns are pretty clear. It's something of a miracle to me that the RAII memory management works as well as it does: so far I haven't faced a situation where I was confused about why the compiler thought a piece of dynamic allocated memory wasn't available. It's something of a cliché to say 'the Rust compiler is so nice to work with' but it's cliché for a reason. But for the times that I didn't understand the compiler message, I tried to explain my problem or misunderstanding as clearly as possible to Claude with the explicit instructions to 'Provide minimal code examples: I want to understand this concept, don't hesitate to ask me questions or probe to build my understanding'. The README of my 'Learning Rust' repository has a log of these questions and answers like the following:

> ### Question
> When using `handles.iter().for_each(|handle| handle.join().unwrap());` in place of the for loop, the build error
> `rustc: cannot move out of *handle which is behind a shared reference`
> was provided. Why is the iterator different than the for loop? I would have thought the ownership was clear?
>
> ### Answer
> The problem is that `handles.iter()` provides shared `&JoinHandle<()>` references to the handles but does not grant ownership of them.

It is easy to see the value of LLMs as search engines that can interpolate between queries, but this is pretty good evidence in favour of Bryne Hobart's argument that 'AI Ruins Education the way Pulleys Ruin Powerlifting'. [^hobart] Being as specific as you possibly can in writing about a topic is a great way to push your understanding; you're better off learning from an engaging professor, but LLMs can sometimes give you something close.

Because the same semaphore needs to be shared across threads I ended up using Rust's atomic reference counting pointer, `Arc<T>`, in every solution so far. McNamara's _Rust in Action_ describes this smart pointer as "Rust's ambassador. It can share values across threads, guaranteeing that these will not interfere with each other". While the semaphore is acquired and released by different threads, the semaphore state is handled by concurrency primitives within the semaphore struct. I expected a little more of a fight from the Rust compiler, but the same ceremony is required for `Semaphore.acquire()` and adding an element to a collection contained in a mutex. Speaking of the semaphores themselves, I was a little surprised to learn that Rust doesn't have them in the standard library so I just used Sean Chen's implementation of them. [^chen]


The Rust learning curve is made more tolerable because the things that are hard have a good reason for being so. But for one problem, I had a tough enough time figuring out how to modify a collection in a way that Rust's compiler would tolerate that I started writing the solution in F#. This was the ballroom dancer queue matching problem between leaders and followers, where I didn't use a one-thread-per-actor model. Both threads in my solution were started by `problem_3_8_thread`, with leaders and followers having a dedicated queue, the rest of the solution is in the F# part of the [repository](https://github.com/eoncarlyle/learning-rust/blob/master/f-sharp/Program.fs#L221). Using a mutable collection this way isn't very good F# style, one could argue that an explicit `ref` cells would be less bad than using `Queue<T>` this way to at least make the mutability more explicit.

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

While it is a much, much better idea to write a solution with an implicit queue formed by a single thread for each dancer, I also knew that the same solution had to be possible in Rust. I eventually [wrote the following](https://github.com/eoncarlyle/learning-rust/blob/master/src/safety_example.rs). One of the issues I faced was that method calls on a mutex-guarded item are handled differently than things like incrementing an integer, but that didn't bother me as much as how mutexes are released in Rust. The `Arc<T>` usage in `dancer_list` is to allow sharing across threads, and `Mutex<T>` allows for mutability - using a `LinkedList<String>` directly as was done in the F# example wouldn't satisfy Rust's safety guarantees, nor should it. I wanted to be able to add entries to the `dancer_list` from the main thread after a dancer thread was initialised, so I wasn't surprised by needing `Arc<Mutex<T>>`. I was surprised that `std::sync::Mutex` didn't provide a function to unlock a mutex. Rather than unlocking a mutex you're supposed to let it be dropped when it falls out of scope as shown below. This is the first time that I've had to use scopes in this way - I'm sure that there is a good reason that there isn't such a function on `Mutex<T>`, either because this prevents bugs or because it's better to use RAII rather than work around it, but aesthetically I absolutely despise this. It looks like the `parking_lot` crate provides an unlockable mutex, but I don't remember getting very far with the crate and decided to stick in the standard library.[^parking_lot]

```rust
fn problem_3_8_thread(
    internal_turnstile: Arc<Semaphore>,
    external_turnstile: Arc<Semaphore>,
    dancer_list: Arc<Mutex<LinkedList<String>>>,
    label: String,
) -> JoinHandle<()> {
    return thread::spawn(move || {
        loop {
            {
                let dancer_list_data = dancer_list.lock().unwrap();
                if dancer_list_data.is_empty() {
                    break;
                }
            }
            println!("{label} thread waiting");
            internal_turnstile.release();
            external_turnstile.acquire();

            {
                let mut dancer_list_data = dancer_list.lock().unwrap();
                let maybe_dancer = dancer_list_data.pop_front();
                maybe_dancer.map(|dancer| println!("{dancer} danced"));
            }
        }
    });
}
```

These are relatively small issues in the grand scheme of things, and jumping straight into concurrency with Rust means dealing with the language's most distinctive features right-off-the-bat. The tooling situation is very good, which is what you should expect from a post-2000 language that wasn't built for interop with anything else. While the Rust standard library reference [^rustlibref] is better than it's F\# equivalent, I haven't found something like the language reference. [^fsharplangref] Having the Rust book available for free as a GitBook is an acceptable substitute. [^rustbook]

[^hyphenation]: I really thought about providing Remzi H. Arpaci-Dusseau and Andrea C. Arpaci-Dusseau's names as
'Professors Arpaci-Dusseu' as is done for the plural 'attorneys general'; this would have been more fun but less clear

[^luu]: [Luu, D. 2016. Programming book recommendations and anti-recommendations.](https://danluu.com/programming-books/)

[^goetz]: Goetz, B. et al 2006. _Java Concurrency in Practice_. Addison-Wesley Professional, Upper Saddle River, NJ.

[^downey]: Downey, A. B. 2016. The Little Book of Semaphores (2nd ed.). Green Tea Press.

[^mcnamara]: McNamara, T. 2021. Rust in Action. Manning Publications Co., Shelter Island, NY.

[^chen]: [Chen S. 2020. Implementing Synchronization Primitives in Rust: Semaphores.](https://seanchen1991.github.io/posts/sync-primitives-semaphores/)

[^hobart]: [Hobart B. 2024. AI Ruins Education the way Pulleys Ruin Powerlifting.](https://www.thediff.co/archive/ai-ruins-education-the-way-pulleys-ruin-powerlifting?ref=iainschmitt.com)

[^parking_lot]: [Docs.rs: parking_lot Crate](https://docs.rs/parking_lot/latest/parking_lot/)

[^fsharplangref]: [F\# Language Reference](https://learn.microsoft.com/en-us/dotnet/fsharp/language-reference/)

[^fsharplibref]: [F\# Library Reference](https://fsharp.github.io/fsharp-core-docs/)

[^rustlibref]: [Rust Library Reference](https://doc.rust-lang.org/std/index.html)

[^rustbook]: [Steve Klabnik and Carol Nichols. 2022. The Rust Programming Language. No Starch Press, San Francisco, CA, USA.](https://doc.rust-lang.org/stable/book/title-page.html)
