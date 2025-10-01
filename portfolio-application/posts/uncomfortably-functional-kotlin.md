---
title: Uncomfortably Functional Kotlin
date: 2025.09.29
---

# Uncomfortably Functional Kotlin

SPS hosts an informal, internal technology conference every year. This was where I  [presented](https://iainschmitt.com/pdf/2025-TechJam-BearTerritory.pdf) work on a stock exchange simulation project a few weeks back. The project was mainly an excuse for learning a couple of technologies that I thought would be
fun to use. One of said technologies was Kotlin, which was my server-side language of choice.

## Kotlin: better in every way

Kotlin is a JVM language that was first released 16 years after Java. That is a lot of time to make something better,
but Kotlin was worth the wait. The following are just a few reasons why it is a joy to work with:
- Completely interoperable with Java: you don't need to leave behind three decades of dependable Java libraries
- Nullable types: the compiler enforces null-safety
- Abbreviated class syntax: you can describe an entire constructor in a class signature, cutting down on boilerplate
- Flow control expressions: both `if` and `when` statements are expressions that evaluate to their results
- Top-level functions: very nice to have these, there is a reason they have been in C# for awhile now

As compared to the Java I write every day at work, Kotlin is better in every way.
My functional programming bias certainly comes into play here, but any programming language that is well-used today
but didn't exist in the 90s must offer something meaningful to displace alternatives. Kotlin is no exception. What is rather surprising is how far you can take the Java interop: using the 'Convert Java File to Kotlin File'
command in IntelliJ I converted one of my team's controller classes to Kotlin in about two minutes during a demo of the
language. I assumed you couldn't run Java and Kotlin side-by-side in the same Maven module, but I didn't see any issues
in doing so; the Kotlinised endpoints worked without issue. There is a learning curve coming from Java because Kotlin
has more syntax. But this is made worthwhile because the additional syntax allows you to be more concise.

While there are many functional programming features in Kotlin, there isn't language-level support of `Either`
and `Option`. This isn't that much a surprise given the Java interop and nullable types, but I was impressed with the
[Arrow](https://github.com/arrow-kt/arrow) functional programming library's implementation of `Either` and `Option`. In the
exchange simulation I used these extensively given how familiar I am working with them in F#, and the library has
something equivalent to computation expressions to work with these types.
F# [computation expression](https://learn.microsoft.com/en-us/dotnet/fsharp/language-reference/computation-expressions)
`let!` assignments are evaluated by calling the `bind` function of the expressions's monad type, and Arrow has used Kotlin's [type-safe builders](https://kotlinlang.org/docs/type-safe-builders.html) to accomplish the same thing.

In the snippet below, the `either` expression will short circuit during the `val y` assignment because `maybeY` is
a `Left` type representing a failure rather than the intended `Int` type. Otherwise, if `y` was a `Right`, `a` would have been a `Right` type wrapping the sum of `x` and `y`.

```kotlin
fun arrowEitherDemonstration() {
    val maybeX: Either<Nothing, Int> = 1.right()
    val maybeY: Either<String, Int> = Either.Left("left failed")

    val a = either {
        val x = maybeX.bind()
        val y = maybeY.bind()
        x + y
    }
    a.fold({println("fold failed")}, { println(it) })
}
```

It isn't terribly clear to me how the Arrow authors enable this short-circuit behavior; the library seems to
be using every Kotlin trick in the book to make this syntax work.  F#'s computation expression syntax isn't quite as
elegant (especially for defining new computation expressions), but it is more straightforward and whenever you see a
`let!`, `do!`, or similar you know exactly what that means in F#. But all-in all, Arrow  brings a lot of what
makes F# fun into Kotlin, and I don't really miss F#'s partial application and function signature type inference when
working in Kotlin.

But as far as Arrow can take you, there are still real and frustrating language-level limitations to going down the
functional programming rabbit hole in Kotlin.

## Hitting the Language Wall

In Haskell, every single side effect producing function must be monadically abstracted. If you try to log to standard
output or read a file in an `Int` returning function, your program will not build: logging and I/O are side effects
rather than pure functions. To log or to carry out I/O the function will need to return a `Writer` or `IO` monad type
instead. This takes getting used to but it allows you to read a Haskell function type signature and immediately tell if
the function is pure.

There's a Reddit [post](https://www.reddit.com/r/fsharp/comments/60ic2f/is_it_worth_using_the_io_monad_in_f/) from
r/fsharp titled "Is it worth using the IO monad in F#?" that I'm reminded of whenever I try to crowbar this behaviour
into another language. The top comment says:

> I'd strongly advise against trying to write Haskell in F#. It's not idiomatic, it's slow and people do not expect it.

This is, unfortunately, quite defensible in F# and even more so in Kotlin. I also refuse to accept it: bringing the best
aspects of Haskell into other languages that I know and like is too appealing. Luckily, Vermeulen, Bjarnason, and
Chiusano's 2021 Book [_Functional Programming in Kotlin_](https://www.manning.com/books/functional-programming-in-kotlin) was written with exactly this idea in mind. Chapter 13, titled "External effects and I/O" isn't an easy read but is rather thought-provoking. That chapter alone makes it
worth buying the book, and it starts off with a naive `IO` monad implementation, similar to the following:

```kotlin
interface IO<A> {
    companion object {
        fun <A> unit(a: () -> A) = object : IO<A> {
            override fun run(): A = a()
        }
        operator fun <A> invoke(a: () -> A) = unit(a)
    }

    fun run(): A

    fun <B> map(f: (A) -> B): IO<B> =
        object : IO<B> {
            override fun run(): B = f(this@IO.run())
        }

    fun <B> flatMap(f: (A) -> IO<B>): IO<B> =
        object : IO<B> {
            override fun run(): B = f(this@IO.run()).run()
        }
}
```

This `IO` implementation would _probably_ work for most use cases, but `flatMap` ends up nesting `IO#run` calls in a way
that will force a stack overflow if called enough times. This can be fixed by replacing stack frames with objects on
the heap, which was done in the book by baking the control flow into a sealed class hierarchy:

```kotlin
sealed class IO<A> {
    companion object {
        fun <A> unit(a: A): IO<A> = LiftF { a }
    }

    fun <B> bind(f: (A) -> IO<B>): IO<B> = Bind(this, f)
    fun <B> map(f: (A) -> B): IO<B> = bind { a -> Pure(f(a)) }
    fun <B, C> map2(ma: IO<A>, mb: IO<B>, f: (A, B) -> C): IO<C> =
        ma.bind { a -> mb.bind { b -> LiftF { f(a, b) } } }
}

data class Pure<A>(val a: A) : IO<A>()
data class LiftF<A>(val thunk: () -> A) : IO<A>()
data class Bind<A, B>(
    val m: IO<A>,
    val continuation: (A) -> IO<B>
) : IO<B>()
```

The next step is a tail-recursive call that operates over the `Pure`, `LiftF`, and `Bind`. Working around JVM type erasure makes this a little awkward, but it works[^0]:

```kotlin
@Suppress("UNCHECKED_CAST")
tailrec fun <A> run(io: IO<A>): A =
    when (io) {
        is Pure -> io.a
        is LiftF -> io.thunk()
        is Bind<*, *> -> {
            val outerM = io.m as IO<A>
            val outerContinuation = io.continuation as (A) -> IO<A>
            val nextIO = when (outerM) {
                is Pure -> outerContinuation(outerM.a)
                is LiftF -> outerContinuation(outerM.thunk())
                is Bind<*, *> -> {
                    val innerContinuation = outerM.continuation as (A) -> IO<A>
                    val innerM = outerM.m as IO<A>
                    innerM.bind { a: A -> innerContinuation(a).bind(outerContinuation) }
                }
            }
            run(nextIO)
        }
    }
```

This is a trampoline[^1], and after it is introduced in chapter 13 the authors point out that the trampoline can be
adapted to create an `Async` monad. They then show that if you define the trampoline for an abstract type constructor,
you end up defining the very useful `Free` monad. But I am relatively sure that this requires higher-kinded type support
in Arrow that was removed from the library since publication of the book. Arrow used to have its own `IO` monad implementation as well as `Semigroup` and `Monoid` interfaces, but the libraryr has since walked back from functional
maximalism. One reason for this may be that you hit something of a wall if you want to add anything on top of the `IO`
monad.

One way to show this is to walk through an incredibly basic Haskell application that both carries out IO and
writes logs. The snippet below serves a single `GET` endpoint which returns a random number and logs to standard output.
The `Writer` monad uses the `tell` function to add accumulated logs, and `runWriterT` will return both the `IO Text`
result of `businessLogic` alongside the `[String]` logs created in the process. These are assigned to `result` and
`logs` in the endpoint respectively.

```haskell
businessLogic :: WriterT [String] IO Text
businessLogic = do
    tell ["processing"]
    randomNum <- liftIO $ randomRIO (1, 100 :: Int)
    tell ["generated random number: " ++ show randomNum]
    tell ["done"]
    return "Hello World"

main :: IO ()

main = scotty 3000 $ do
    get "/" $ do
        (result, logs) <- liftIO $ runWriterT businessLogic
        liftIO $ print logs
        text (TL.fromStrict result)
```

This is possible because `WriterT` is a monad transformer, which allows for layering multiple monads together. In this case
the `WriterT [String] IO Text` is a combination of the `IO` monad and the `Writer` monad. Monad transformers are also
made possible by higher-kinded types that are supported by Haskell and Scala, but not Kotlin. I mention this to
demonstrate that even in this very simple application, `IO` is not enough. Many applications will also require `State`
and `Reader` and while it may be possible to define some `IOWriter` in Kotlin, it increasingly feels like you're hitting
a wall. Kotlin simply wasn't meant to do this.

We use much more Kotlin than Scala at SPS and I have only good things to say about the language, so I don't regret
picking it up. But it seems like you can only get about 85% the way to 'full monad', which is a disappointment.

## References
1) [Patrick McKenzie. 2025. Developing In Stockfighter With No Trading Experience.](https://www.kalzumeus.com/2015/10/30/developing-in-stockfighter-with-no-trading-experience/)
2) [Brian Nigito. 2017. How to Build an Exchange](https://www.youtube.com/watch?v=b1e4t2k2KJY)
3) Rachel Wonnacott. 2025. How to Build an Exchange. At Manifest 2025. Berkeley, CA.
4) [The Arrow Authors. 2017-2025. Arrow. GitHub repository.](https://github.com/arrow-kt/arrow)
5) [Microsoft. 2023. F# Language Reference: Computation Expression](https://learn.microsoft.com/en-us/dotnet/fsharp/language-reference/computation-expressions#built-in-computation-expressions)
6) [Kotlin Foundation. 2025. Type-safe Builders.](https://kotlinlang.org/docs/type-safe-builders.html)
7) [Reddit. 2017 r/fsharp: Is it worth using the IO monad in F#?](https://www.reddit.com/r/fsharp/comments/60ic2f/is_it_worth_using_the_io_monad_in_f/)
8) Marco Vermeulen, Rúnar Bjarnason, and Paul Chiusano. 2021. Functional Programming in Kotlin. Manning Publications, USA. ISBN: 9781617297168.
9) [Marco Vermeulen, Rúnar Bjarnason, and Paul Chiusano. 2011-2025. Functional Programming in Kotlin. GitHub repository](https://github.com/fpinkotlin/fpinkotlin/tree/master)
10) [Rúnar Bjarnason. 2012. Stackless Scala With Free Monads](https://days2012.scala-lang.org/sites/days2012/files/bjarnason_trampolines.pdf)
11) [Andy Gill. 2001. MTL Library: Control.Monad.Writer.CPS](https://hackage.haskell.org/package/mtl-2.3.1/docs/Control-Monad-Writer-CPS.html)

[^0]: It may be possible that the eager function call in `innerM.bind` could force a stack overflow but I haven't proven this
[^1]: I'd recommend Rúnar Bjarnason [Scala paper](https://days2012.scala-lang.org/sites/days2012/files/bjarnason_trampolines.pdf)
which looks like something of a precursor to Chapter 13.
