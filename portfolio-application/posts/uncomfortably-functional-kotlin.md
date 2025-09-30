---
title: Uncomfortably Functional Kotlin
date: 2025.09.29
---

# Uncomfortably Functional Kotlin

SPS has an informal internal technology conference, and this year I [presented](https://iainschmitt.com/pdf/2025-TechJam-BearTerritory.pdf) work from a stock exchange simulation that I have been
working on this year. I didn't actually set to build out an exchange at first: there were a handful of technologies
that I wanted more exposure to, but when I read about how [Patrick McKenzie did this](https://www.kalzumeus.com/2015/10/30/developing-in-stockfighter-with-no-trading-experience/) a few years back,
I thought it would be fun to take a crack at.

While there is a lot that I took away from the project, the language I wrote it in ended up being the most fun lesson I learned.

## Kotlin: Better in every way

Kotlin is a JVM langague that was first released 16 years after Java. That is a lot of time to make something better, but Kotlin was worth the wait. The following are just a few reasons why it is a joy to work with:
- Completely interoperable with Java: you don't need to leave behind three decades of dependable Java libraries
- Nullable types: the compiler forces the author to check null-saftey; the safe call and Elvis operators (`?.` and `?:`)
make nullable types ergonomic to work with
- Abreviated class syntax: you can describe a constructor in a class signature, so classes need not so much boilerplate
- Flow control expressions: both `if` and `when` statements are expressions that can evaluate to a given value
- Top-level funtions: very nice to have these, there is a reason they have been in C# for awhile now

I almost hesitate to write this, but as compared to the Java I write every day at work, Kotlin is better in every way.
My functional prrogramming bias certainly comes into play here, but any programming langauge of prominence that wasn't
around in 2000 must offer something rather special to displace alternatives and Kotlin is no exception. What I was
rather suprised by is how far you can take the Java interop. Using the 'Convert Java File to Kotlin File'
command in IntelliJ I converted one of our controller classes to Kotlin in about two minutes during a demo of the
language a few days back, so there isn't any issue runnign Java and Kotlin side-by-side within the same Maven module.

As much as I would love to have langauge-level support of `Either` and `Option` in Kotlin, I was impressed with the
[Arrow](https://github.com/arrow-kt/arrow) functional programming library's implementation of these types. In the
exchange simulation I used these extensively given how familiar I am working with them in F#, and there is even some
syntax equivalent to computation expressions to work with these types. Assignments using `let!` within an F#
[computation expression](https://learn.microsoft.com/en-us/dotnet/fsharp/language-reference/computation-expressions)
are evaluated by calling the `bind` function of a monad, and Arrow has used Kotlin's [type-safe builders](https://kotlinlang.org/docs/type-safe-builders.html) to accomplish the same thing.

In the snippet below, the `either` expression will short circut during the `val y` assignment because `maybeY` is
a `Left` type representing a failure rather than an intended `Int` type. Otherwise if `y` was a `Right`, `a` would have been a `Right` type wrapping the sum of `x` and `y`.

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

It isn't terribly clear to me exactly how the Arrow authors enable this short-circut behaviour; the library seems to
be using every Kotlin trick in the book to make this syntax work. F#'s computation expression syntax isn't quite as
elegant (especially for defining new computation expressions), but it is a lot more straightforward and every time you
see `let!` you know what you are getting while the Kotlin `bind` method has no such gaurentees.

But all-in all, I like working with Arrow and it brings a lot of what makes F# fun into Kotlin. But as far as Arrow
can take you, there are still real langauge-level limitations.

## Hitting the Langauge Wall

In Haskell, every single side effect producing function must be monadically abstracted. If you try to log to standard
output or read a file in an `Int` returning function, your program will not build: logging and I/O are side effects
rather than pure functions. To log or to carry out I/O the function will need to return a `MonadWriter` or `IO` type instead. This has a learning curve, but it allows you to read a Haskell function type signature and immediately tell if the function you're looking at is a pure function.

The Reddit [post](https://www.reddit.com/r/fsharp/comments/60ic2f/is_it_worth_using_the_io_monad_in_f/)) I think of whenever I try to crowbar this behaviour into another langague comes from r/fsharp, and is
titled 'Is it worth using the IO monad in F#?'. The comment is the following:

> I'd strongly advise against trying to write Haskell in F#. It's not idiomatic, it's slow and people do not expect it.

This is, unfortunately, quite defensible even as applied to Kotlin. I also refuse to accept it, the of bringing the
best aspect of Haskell into other languages is too aluring.

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

```haskell
tell :: w -> m ()
runWriter :: Monoid w => Writer w a -> (a, w)
```

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

## References
1) [Patrick McKenzie. 2025. Developing In Stockfighter With No Trading Experience.](https://www.kalzumeus.com/2015/10/30/developing-in-stockfighter-with-no-trading-experience/)
2) [Brian Nigito. 2017. How to Build an Exchange](https://www.youtube.com/watch?v=b1e4t2k2KJY)
3) Rachel Wonnacott. 2025. How to Build an Exchange. At Manifest 2025. Berkeley, CA.
4) [The Arrow Authors. 2017-2025. Arrow. GitHub repository.](https://github.com/arrow-kt/arrow)
5) [Microsoft. 2023. F# Langauge Reference: Computation Expression](https://learn.microsoft.com/en-us/dotnet/fsharp/language-reference/computation-expressions#built-in-computation-expressions)
6) [Kotlin Foundation. 2025. Type-safe Builders.](https://kotlinlang.org/docs/type-safe-builders.html)
7) [Reddit. 2017 r/fsharp: Is it worth using the IO monad in F#?](https://www.reddit.com/r/fsharp/comments/60ic2f/is_it_worth_using_the_io_monad_in_f/)
