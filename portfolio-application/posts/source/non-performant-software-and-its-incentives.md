---
title: Non-performant software and its incentives
date: 2026.07.12
---

# Non-performant software and its incentives

I end up re-listening to [Episode #78](https://shows.acast.com/software-unscripted/episodes/664fde448c77cc0013b33390) of _Software Unscripted_ at least once a year. As I've <a href="/post/november-2024-grab-bag">previously written</a>, this podcast is hosted by high-profile Elm programmer and the creator of the Roc functional programming language. Episode #78 is Feldman's conversation with game developer Casey Muratori. Feldman correctly decries how unseriously backend and frontend programmers take performance as compared to programmers pushing the limits of what hardware is capable of. Feldman brought Muratori on the show to have a conversation about what software performance lessons web programmers can learn from game programmers.

Muratori says a lot of insightful things in the episode, but there's a section where his reasoning is off:

> People intuitively understand great performance when you give it to them. There's a tremendous market opportunity for anyone who wants to make a move into a space and deliver a truly great experience, of which performance is usually critical. I think that's something that could also help change the culture, because once you have strong competitors who are really strong on performance, they're very hard to unseat, because you have to come out with a very well performing product to do so.

Muratori continues, emphasis mine:

> There's nothing magic about games. What happens with games is ***players won't play something that runs at 10 frames a second***. It's a market force. They've already played games at 60 FPS. They won't go back... if you are the kind of person who wanted to get some people together and write some performant software, I feel like the world is kind of your oyster because literally anywhere you look, the software is garbage. About the only place you couldn't assail that way is games. Games and maybe like backend search at Google might be really super tightly optimized... Just don't go after those. Everybody else, ***they have no ability to compete with you. You could just absolutely dominate.***

To be clear, he's completely correct that modern software performance is woeful. Discord uses at least 400 MB of RAM on my 2024 MacBook Pro when _idle_. This is more RAM than what most consumer desktops had installed in 2000, and that's without actually making voice or video calls wit the application. There are many reasons why Discord wastes a pathological amount of memory, but much of modern software is built atop a tower of dependencies that often aren't written in a performance-aware way. Each dependency is a layer of scar tissue slowing the program down.

But Muratori is wrong about these competitive forces outside of games, and he is wrong for a depressing reason. While individual games may be differentiated by gameplay, narrative, or artistic style, the relevant axis of competition is not 'multi-player real-time strategy games set in space' and rather 'interactive entertainment in general', so a game with unplayably bad performance can be replaced by something else in the player's library because both titles can scratch a similar itch.

Most of the software that people like Muratori rightfully complain about can't be swapped with an equivalent substitute nearly as well because of high barriers for possible competitors to enter.
- For operating systems there is the obviously daunting scale of the work to be done as well as network effects to contend with
- Enterprise software: sales and marketing as moats, people making the decisions are not users, many more dimensions of comeptition that are not just performance
