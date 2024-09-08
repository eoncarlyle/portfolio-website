
# Review of _Domain Modeling Made Functional_

_Domain Modeling Made Functional: Tackle Software Complexity with Domain-Driven Design and F\#_ is a 2018 book by Scott Wlaschin [^wlaschin]. As it says on the tin, the book's goal is to show the reader how to implement domain modeling using functional programming. The phrase "domain-driven design" itself was coined by Eric Evans' 2003 book of the same name where all code examples are written in Java [^evans], and most other books and talks about DDD use object-oriented languages. While code examples for this book are provided in F\#, no prior knowledge of the language is assumed. The first section of the book is a brief overview of domain-driven design terms, and section two walks through how the type system and chained function calls can be used for 'modeling in the small'. Part 3 wraps up the book by fully implementing the e-commerce bounded context introduced in Part 1.

The book's discussion of domain modeling topics agnostic to programming paradigm were good but could have gone into more detail. It covers the basics of entities, value objects, aggregates, and bounded contexts well if not as in-depth as _Domain Driven Design_. While the Evans book suffers from a low ratio between prose and code segments, _Domain Modeling Made Functional_ never goes more than a few paragraphs without showing some code, making Wlaschin's arguments more concrete and the book more readable. One limitation of his approach is his reliance on a single, relatively straightforward business domain throughout the book while Evans offers more varied modeling challenges across multiple domains in a single chapter. Wlaschin's e-commerce business domain includes order validation, pricing, and an acknowledgement email [^dmmfrepo], and this business logic isn't as complicated compared to the syndicated loan system in chapters 8 and 10 of _Domain Driven Design_ where the solution must keep track of a lender's share of an incoming loan payment. The book would be improved by a chapter walking the reader through multiple thorny domain modeling cases.

What makes the book shine is its progressive disclosure of language features to an F\# beginner in a way that really sells the language. Domain modeling doesn't require too many language features so Wlaschin spends as little time as possible explaining F\# syntax. By introducing the type system and pattern matching early on the book can quickly explain the advantages that F\# brings over imperative languages. An example of baking the domain rules into the type system is shown below, where the `String50` type and module are set up to make null, empty, or string larger than 50 characters unrepresentable.


```fsharp
type String50 = private String50 of string

module String50 =
    let value (String50 str) = str

    let create fieldName str : Result<String50, string> =
        if String.IsNullOrEmpty str then
            Error(fieldName + " must be non-empty")
        elif str.Length > 50 then
            Error(fieldName + " must be less than 50 chars")
        else
            Ok(String50 str)
```

Concerning pattern matching, the following example was used in the book with the `ShoppingCart` discriminated union.

```fsharp
type ShoppingCart = 
    | EmptyCart
    | ActiveCart
    | PaidCart

let addItem cart item =
    match cart with
    | EmptyCart -> ActiveCart { UnpaidItems = [ item ] }
    | ActiveCart { UnpaidItems = existingItems } 
        -> ActiveCart { UnpaidItems = item :: existingItems }
    | PaidCart _ -> cart
```


Wlaschin also gives an effective explanation of the Either monad to bring error handling into the primary control flow rather than the 'hidden' control flow of exception handling. Rather than explain that the `Result` type is an implementation of the Either monad - or even explain what monads are - he explains the type before using it in the rest of the domain for error handling. Aiding his explanation are diagrams similar to those from his 'Railway Oriented Programming' talk shown below [^slides].

!["Railway Oriented Programming" Diagram](https://iainschmitt.com/images/RailwayOrientedProgrammingDiagram.png)

For readers coming to functional programming for the first time, this is an appropriately gentle introduction to monads, and he doesn't even use the 'm-word' until he's fully explained `Result`: "The m-word has a reputation for being scary, but in fact we’ve already created and used one in this very chapter!". I'm almost embarrassed to admit that this was how I learned that the Java `Optional` class was itself a monad. Little did I know that nearly every day I was using an FP concept that had confused me for years! Something similar is done for Michał Płachta's _Groking Functional Programming_, where the Optional, Either, and I/O monads are demonstrated without using the word 'monad' anywhere in the book [^plachta].  

This leaves a lot about F\# that a novice to the language will have to pick up - there isn't a discussion of the built-in .NET types, a dedicated chapter to working with collections, and other such topics. For questions like 'how do I do a map over an array' the excellent F\# Language Reference [^fsharplangref] and F\# Library Reference [^fsharplibref] provide quick answers.  _Domain Modeling Made Functional_ isn't sold as a way to learn F\#, but while I didn't intend to at the outset I ended up reading the entire book cover-to-cover and it greatly motivated me to write F\# in the process. Reading the book, using the language documentation, and doing a few old Advent of Code problems was a faster and more painless way to learn the basics of a new language than what I had done in the past by reading a language-learning book first. This has made me realise that I would much rather learn a new language by diving into a problem area that it is well equipped to work in: rather than just learn Rust, I'd rather do a deep dive in concurrency and learn Rust in the process. Rather than just learn C, I'd rather write a ray tracer, something I've tried unsuccessfully a few times since first seeing the spectacular ray tracer in 99 lines of C++ [^beason]. In the future, I'll be on the lookout for 'Learn concept X through language Y' books.

One criticism of the book is that it stresses practicality and making functional programming look normal to a fault. While some material in the persistence chapter helped tie database options to `Result` types, much of the content on incorporating relational and document databases was unnecessary; this type of detail was left out of the Evans book for a reason. Wlaschin's chosen dependency injection method was by passing dependencies as function parameters, explaining that the Reader and Free monads would be omitted given the introductory nature of the book. The Reader monad was something that I immediately read up on after finishing the book, as I'm sceptical of passing every dependency as function parameters for large applications. Given how good his Either monad explanation was, I'm sure he would have hit the mark for Reader and Free as well.

The second to last sentence in _Domain Modeling Made Functional_ is "In this book I aimed to convince you that functional programming and domain modeling are a great match", and the book does accomplish this. Wlaschin is one of the best technical authors I've read, making the 310 pages in the book fly by. While It is missing some advanced DDD and FP concepts it ended up being an excellent introduction to F\# that I would recommend to anyone interested in learning the language.

<br>

[^jsparty]: [JS Party Episode #263](https://changelog.com/jsparty/263)

[^talkpython]: [Talk Python To Me Episode #420](https://talkpython.fm/episodes/show/420/database-consistency-isolation-for-python-devs)

[^boringtech]: [McKinley "Choose Boring Technology" Blog Post](https://mcfunley.com/choose-boring-technology)

[^su-muratori]: [Software Unscripted Episode #78](https://shows.acast.com/software-unscripted/episodes/664fde448c77cc0013b33390)

[^su-wlaschin]: [Software Unscripted Episode #48](https://shows.acast.com/software-unscripted/episodes/664fde448c77cc0013b333ae )

[^dotnetexplain]:  [What is .NET? What's C# and F\#? What's the .NET Ecosystem?](https://www.youtube.com/watch?v=bEfBfBQq7EE)

[^wikipedia]: [C# redirect Wikipedia](https://en.wikipedia.org/wiki/C)

[^msstyle]: [.NET Microsoft Style Guide Entry](https://learn.microsoft.com/en-us/style-guide/a-z-word-list-term-collections/n/net)

[^firebasedotnet]: [Repository for firebase-admin-dotnet](https://github.com/firebase/firebase-admin-dotnet)

[^firebasehaskell]: [Hackage firebase query](https://hackage.haskell.org/packages/search?terms=firebase)

[^couchdbdotnet]: [NuGet page for CouchDB.NET](https://www.nuget.org/packages/CouchDB.NET)

[^couchdbhaskell]: [Hackage page for CouchDB](https://hackage.haskell.org/package/CouchDB)

[^wlaschin]: [_Domain Modeling Made Functional_](https://pragprog.com/titles/swdddf/domain-modeling-made-functional/)

[^evans]: [_Domain-Driven Design_](https://learning.oreilly.com/library/view/domain-driven-design-tackling/0321125215/)

[^dmmfrepo]: [`placeOrder` function in  _Domain Modeling Made Functional_ repository](https://github.com/swlaschin/DomainModelingMadeFunctional)

[^fsharplangref]: [F\# Language Reference](https://learn.microsoft.com/en-us/dotnet/fsharp/language-reference/)

[^fsharplibref]: [F\# Library Reference](https://fsharp.github.io/fsharp-core-docs/)

[^francher]: [_The Book of F\#_](https://nostarch.com/fsharp)

[^abraham]: [_F\# in Action_](https://www.manning.com/books/f-sharp-in-action)

[^kleppman]: [_Designing Data-Intensive Applications_](https://dataintensive.net/)

[^petrov]: [_Database Internals_](https://www.databass.dev/book)

[^java]: [_Learning Java, 5th Edition_](https://learning.oreilly.com/library/view/learning-java-5th/9781492056263/)

[^plachta]: [_Grokking Functional Programming_](https://www.manning.com/books/grokking-functional-programming)

[^slides]: ["Railway Oriented Programming"](https://www.slideshare.net/slideshow/railway-oriented-programming/32242318#90)

[^beason]: [Kevin Beason's "Global Illumination in 99 lines of C++"](https://www.kevinbeason.com/smallpt/)


