---
title: Vibe code at your peril
date: 2025.12.19
---

# Vibe code at your peril

fdsafsdafAt every software company that I have worked at, if something is going wrong the customer has a named person 
who is 
responsible for getting the problem fixed. If a piece of the company's software could be to blame, there is a living,
breathing developer on-call 24 hours a day, seven days a week. This on-call developer is responsible for answering
questions like 'why is this service not working?' and 'have you made any changes that would explain what we're seeing
here?'. Being on-call doesn't require terminal expertise in every facet of the software, but if they are doing their job
correctly the engineer can give plausible answers to these questions.

If you are on-call, this inevitably requires you to _understand the code that you are responsible for_.

No, the on-call engineer has not written every line of the software they are being asked about. Maybe their _team_
hasn't even written all of it. But they need to have a good mental model of the types of things their services are and
are not supposed to do, and why.

If no one actually expects you to understand or take responsibility for the software you develop, then by all means
don't waste your time reading what your large language model of choice plops onto the screen. But as Patrick McKenzie
[explains](https://www.kalzumeus.com/2011/10/28/dont-call-yourself-a-programmer/), "Most software is boring one-off
applications in corporations, under-girding every imaginable facet of the global economy". This is software worth paying
for because nearly no one should have to think about if an invoice was stored correctly in a database, if a Slack
message will actually reach its intended target, or if the airline you're flying on has double-booked your seat in their
reservation system.

For all of the oxygen that "agentic development" has sucked out of the room, developers cannot take responsibility for
that which they do not understand. The act of writing code by hand forces the developer to think through what could go
wrong with the changes they are putting in place and what other alternative options exist to solve the problem at hand.
A lot of writing code is really an internal monologue to convince yourself that what you have written will work! This
monologue isn't something you experience when reviewing someone else's code, and for this reason reviewing code can be
harder than writing it yourself. If you're reviewing a person's work you should be sure that they have walked through
this process before asking for review; chances are they've thought to themselves 'if this code triggers a 3:00 AM page
to a teammate, will they be able to understand what is going on?'. What falls out of this is that if you want a living,
breathing person to be responsible for a piece of software then its rate of change is gated by human understanding.

Don't get me wrong, Claude is a great resource and allows me to ask an experienced engineer all manner of questions at a
fraction of the opportunity and interpersonal cost. But writing code by hand helps the developer immensely in
understanding the problems they are trusted to solve. It takes longer to write that new feature by hand, but this saves
precious minutes when tracking down customer-facing bugs, or precious months by preventing wild goose chases down
development dead ends. Ultimately if you are responsible for a piece of software that is load-bearing to your customers,
you should act like it.
