# November Grab Bag

There are a couple of topics that were cut from drafts of the previous two posts. I liked the sections too much to shelve, so I'm posting them as a grab-bag here

## _Software Unscripted_ and Enterprise Haskell

There aren't all that many good software engineering podcasts because the medium isn't a great match for the message: source code in plain text is ultimately, well, text, and being able to re-read sentences that you didn't understand the first time helps a lot when learning a concept in computing. But podcasts move at the pace of their presenters, and along with the general expectation that podcast episodes are self-contained, it's hard to get a lot of depth on a topic. This means most quality software engineering podcasts are valuable primarily for their topic curation: a _JS Party_ podcast taught me about Preact [^jsparty] and _Talk Python to Me_ episode led me to MIT's 6.824 and Klepmman's _Designing Data-Intensive Applications_ [^talkpython]. This is still valuable, as it can help prioritise what topics are worth learning and which written resources to reference as you're learning them.

One software engineering podcast that stands head-and-shoulders above the rest is _Software Unscripted_ by Richard Feldman, a high-profile Elm programmer and the creator of the Roc functional programming language. Richard and his guests get into a level of detail that I haven't seen in podcast form. One of the things that enables this is that Richard expects a lot from his listeners: he would rather have you stop the podcast to refresh your memory on automatic reference counting or effect systems than break up the conversation with an explanation. Not every episode is tied to progress on the Roc language, but a lot of the episodes seem to be Richard recording a conversation with someone he needed to talk to anyway to make progress on the Roc language.

Something else that sets the tone for many of the conversations on Software Unscripted is that Feldman used to work for a company that used Haskell as its primary backend language. It's a powerful tool in the right hands, but Haskell is a gutsy choice to make in enterprise settings. If you have the right developer talent then the steep language learning curve can be overcome, but the dearth of third-party libraries and tooling that come with a smaller language are considerable disadvantages. The Java and Microsoft.NET platforms have libraries for seemingly everything under the sun, and while there's a lot to be said for stick-and-rudder programming with nothing but Vim and your terminal emulator - the way that UNIX was written, after all - having an out-of-the-box debugger sure helps solve bugs on a late Friday afternoon. Languages can and should exist for non-enterprise reasons, but someone who is a good enough engineer to use Haskell is also a good enough engineer to write your CRUD app in Java. This isn't to say that there aren't any advantages to writing production programs in Haskell, but seldom will using the language be the best use of anyone's 'innovation tokens' [^boringtech].

## F\# and .NET Naming Conventions

Until listening to "F\# in Production with Scott Wlaschin" [^wlaschin], I was skeptical of production use of Haskell in particular and functional programming in general. The episode didn't go in as much depth as some [^muratori], but Wlaschin positioned F\# as a better fit for enterprise software engineering over Haskell:

> “If you look at the Haskell books, like printing ‘Hello World’ is like in Chapter 7 of these books … you can’t do it in Haskell until you understand I/O, and you can’t understand I/O until you understand monads”

> “And so \[F\#\] is nice because it’s not as pure as something like Haskell but it’s very programmatic, and so you can piggyback on the massive .NET libraries”

> “And I have seen … one person re-writes a whole chunk of code in Haskell or Clojure or whatever and then they go away and nobody has really bought into it”

Wlaschin's pitch for F\# was so good that I read his 2018 book _Domain Modeling Made Functional_ and wrote a short review on this website [^dmmf].

F\# is an ML-family functional language that compiles to Microsoft's Common Intermediate Language (CIL), the bytecode for the Common Language Runtime (CLR) virtual machine. C\# is far and away the post commonly used CIL language, but the .NET runtime APIs are shared between both languages: for example, the `System.IO` namespace contains I/O related .NET libraries included in the runtime. Interop between the languages is supported, allowing the F\# programmer to put to use a whole wealth of packages already written for C\#. In my experience working with the language, I've had to use several packages written entirely in C\# and while you have to call the functions slightly differently, things have almost always just worked.

My biggest knock on the .NET Platform is the naming scheme, and complaining about this is too much fun to pass up. My favourite example of this is that “What is .NET? What's C# and F\#? What's the .NET Ecosystem?” is an eighteen-minute video by Microsoft VP Scott Hanselman. At one point he says "it's not the best name, but it's the name that we have" [^dotnetexplain]. After all, ".NET" is indistinguishable from a common top-level domain when spoken, and starting a sentence with ".NET" makes it look like you've broken punctuation rules. The Microsoft Writing Style Guide allows for starting a sentence with the platform name, but notes that "Microsoft.NET" is an acceptable alternative. Before seeing it in the style guide I had literally never seen this longer form, but the full "Microsoft.NET" apparently should always be used on the first mention of the platform, as is done in this post [^msstyle]. This goes without saying but "Common Intermediate Language" and "Common Language Runtime" could not sound more generic without serious effort. Luckily the second characters in "C\#" and "F\#" are not actually musical sharp symbols, which would give the languages the unfortunate honour of having a non-ASCII character in their name. But not to let them off the hook, searching for "C#" or "F\#" on Wikipedia redirects the reader to the pages for their respective Latin letters: 'for technical reasons, "C#" and ":C" redirect here' [^wikipedia]. More than a few times I've had to spell out "sharp" in place of the number sign when referencing either language.

## Moving this Website to F\#

Most advice about developer blogs is to use a static site generator by the likes of Hugo, Gatsby, or 11ty rather than trying to create something yourself. While it isn't bad advice, I've never been very good at following it. Last year I picked Ethan Brown's book on Node and Express - one of the more engaging language/framework learning books I've come across - and I realised that I had some gaps in my knowledge of how bog-standard MVC applications worked [^brown]. At the time this website was some not-very-well-written React application serving static text, so I re-wrote it to be a Node MVC application. Then, as is still the case, the website was little more than a group of routes to serve Markdown files rendered as HTML, with a little bit of templating and CSS to cut down on boilerplate and to make things look nice. When doing that first migration to Express, I thought that I would be writing more, so I added a `/post/:markdownFileName` route to serve arbitrary Markdown files. While I didn't end up writing much of anything other than the landing and resume pages on the old website, I kept using it as a testbed for various things like Nest or different logging libraries.

A little before picking up F\# I finally took the [very](https://yieldcode.blog/post/why-engineers-should-write/) [common](https://rmoff.net/2023/07/19/blog-writing-for-developers/) [advice](https://guzey.com/personal/why-have-a-blog/) for software engineers to do public-facing writing, so I wanted to preserve the ability to serve arbitrary Markdown now that I was finally putting it to use. The main server-side framework I used was Giraffe, which is mostly set of functional bindings on top of C\#'s ASP.NET [^giraffe]. The Node website used Handlebars templates, but I moved these over to Razor pages without much issue when I couldn't find a .NET Handlebars engine that worked well. As is called out by the Giraffe docs, F\#'s eager evaluation of functions means that routes are only evaluated the first time that they are requested, where the same initial result will be used for future requests. Giraffe's `warbler` functions need to be used for accessing dynamic resources, but eager evaluation for static content probably has some performance benefits by forgoing unnecessary re-renders from Markdown to HTML. However, as shown below I was following the lessons from Wlaschin's _Domain Modelling Made Functional_ by using the type system only build `/post/:markdownPath` paths for existing Markdown files. This meant that I'd need to restart the application whenever I'd write a new post.

```fsharp
module AppHandlers

//...
module MarkdownPath =
    let create path =
        match path with
        | path when (File.Exists path) && (Path.GetExtension path = ".md") -> Some(MarkdownPath path)
        | _ -> None

    let toString (MarkdownPath path) = path
//...

let createRouteHandler markdownPath =
    match MarkdownPath.toString markdownPath |> Path.GetFileName with
    | "landing.md" -> route "/" >=> markdownFileHandler LeftHeaderMarkdown markdownPath "Iain Schmitt"
    | "resume.md" ->
        route "/resume"
        >=> markdownFileHandler LeftHeaderMarkdown markdownPath "Iain Schmitt's Resume"
    | _ ->
        route $"/post/{Path.GetFileNameWithoutExtension(MarkdownPath.toString markdownPath)}"
        >=> markdownFileHandler PostMarkdown markdownPath "Iain Schmitt"

let appRoutes: list<HttpHandler> =
    Directory.GetFiles markdownRoot
    |> Array.choose MarkdownPath.create
    |> Array.map createRouteHandler
    |> Array.toList
```

While I'm sure I could get some Headless CMS working to my liking, Markdown is just plaintext, so I decided to essentially use GitHub CI/CD Actions as a CMS; the deploy pipeline would build and deploy the application with all Markdown files in `WebRoot/markdown/` after merge commits. GitHub actions worked well in another F\# project[^previouspost], but I decided to make things more interesting by self-hosting the website. While the domain records for this website are served on a Digital Ocean VM, Nginx on said VM runs a reverse proxy to my home router. A port forwarding rule that only applies to the Digital Ocean IP address forwards traffic to a server on a DMZ VLAN, which hosts the application. One of these days I'll implement some caching on the Digital Ocean Nginx and set up another reverse proxy VM in a different geography, but that's almost entirely for show given what little bandwidth this website needs. At this point I don't have any website analytics outside of Nginx logs, but some minor F\# middleware would fix that to give me some visibility on page views.

As far as the actual content goes, I was surprised that Markdown syntax includes footnotes, but they work pretty well in both the browser and in the Obsidian Markdown editor that I use to write posts. One fun initial footnote issue was '↩' rendering as an emoji on iOS until I used the correct [escape characters](https://github.com/eoncarlyle/portfolio-website/commit/abe663e5af09363ce855d654946dc8ac95124f77#diff-7153e8b42576b13b681b05a7d90a3720247c7eae140aec8c60eea18b0475972fL63), and if I use a sufficiently long URL sans any link text then the page will grow larger than the mobile viewport. I'd be surprised if there wasn't some CSS fix for breaking those URLs, but it's a good idea to have link text anyway. And speaking of CSS, my biggest current annoyance is that I'm still doing code syntax highlighting on the client using Prism [^prismjs]. As Josh Comeau wrote in his excellent post about building his new blog, there's a tradeoff between bundle size and supporting more languages in a client-side syntax highlighter [^comeau]. Given that the only thing on this website that needs JavaScript is the syntax highlighting, I would much rather do this entirely on the server. However, I haven't found a good replacement syntax highlighter for use with my Markdown processor, Markdig; while I've seen the `Markdig.Prism` package, it requires serving the Prism script on the pages themselves thus defeating the purpose of migrating entirely [^markdig.prism]. At the very least Prism is less than 2000 lines long, so maybe it's time for a full rewrite in C# or F#.



[^jsparty]: [JS Party Episode #263](https://changelog.com/jsparty/263)

[^talkpython]: [Talk Python To Me Episode #420](https://talkpython.fm/episodes/show/420/database-consistency-isolation-for-python-devs)

[^boringtech]: [McKinley "Choose Boring Technology" Blog Post](https://mcfunley.com/choose-boring-technology)

[^muratori]: [Software Unscripted Episode #78](https://shows.acast.com/software-unscripted/episodes/664fde448c77cc0013b33390)

[^wlaschin]: [Software Unscripted Episode #48](https://shows.acast.com/software-unscripted/episodes/664fde448c77cc0013b333ae )

[^dotnetexplain]:  [What is .NET? What's C# and F\#? What's the .NET Ecosystem?](https://www.youtube.com/watch?v=bEfBfBQq7EE)

[^wikipedia]: [C# redirect Wikipedia](https://en.wikipedia.org/wiki/C)

[^msstyle]: [.NET Microsoft Style Guide Entry](https://learn.microsoft.com/en-us/style-guide/a-z-word-list-term-collections/n/net)

[^dmmf]: [_Domain Modelling Made Functional_](https://pragprog.com/titles/swdddf/domain-modeling-made-functional/)

[^dmmfrepo]: [Code sample repository for _Domain Modelling Made Functional_](https://github.com/swlaschin/DomainModelingMadeFunctional)

[^fsharplangref]: [F\# Language Reference](https://learn.microsoft.com/en-us/dotnet/fsharp/language-reference/)

[^fsharplibref]: [F\# Library Reference](https://fsharp.github.io/fsharp-core-docs/)

[^francher]: [_The Book of F\#_](https://nostarch.com/fsharp)

[^abraham]: [_F\# in Action_](https://www.manning.com/books/f-sharp-in-action)

[^kleppman]: [_Designing Data-Intensive Applications_](https://dataintensive.net/)

[^previouspost]: [Previous post: October 2024 GDPLE Development](https://iainschmitt.com/post/october-2024-gdple-development)

[^brown]: [Web Development with Node and Express, 2nd Edition](https://learning.oreilly.com/library/view/web-development-with/9781492053507/)

[^giraffe]: [Giraffe Framework](https://giraffe.wiki/)

[^prismjs]: [Prism syntax highlighter](https://prismjs.com/)

[^comeau]: [Josh Comeau blog post](https://www.joshwcomeau.com/blog/how-i-built-my-blog-v2/#the-magic-of-static-5)

[^markdig.prism]: [`Markdig.Prism` package](https://github.com/ilich/Markdig.Prism)

[^kleppman]: [_Designing Data-Intensive Applications_](https://dataintensive.net/)
