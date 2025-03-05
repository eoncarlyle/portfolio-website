---
title: Understanding monads?
date: 2024.12.26
---
# Understanding monads?

The Node reverse proxy explained in my <a href="/post/my-needlessly-complicated-reverse-proxy">last post</a> sprinkled in a little bit of functional TypeScript through Ramda. [^ramda] This naturally gave me an excuse to talk about the project at the functional programming group at work, but some of the feedback that I got was that Ramda takes advantage of the permissive nature of JavaScript types, so `fp-ts` is a better functional library for TypeScript.[^fpts] _Professor Frisby's Mostly Adequate Guide to Functional Programming_ is linked by the `fp-ts` docs in lieu of a more comprehensive tutorial for the library, and it is a wonderful resource for functional programming in JavaScript. [^mostly-adequate] Chapters 8 and 9 explain functors and monads respectively, and after reading them I told a developer friend of mine 'I recently made a breakthrough in understanding monads' before sending him some notes explaining why. Said notes are available [here](https://gist.github.com/eoncarlyle/309b7da602df8ea6d4ab7a01fc83bcbb), but the next morning I stared at the relevant type signatures long enough to understand that nearly everything about my explanation was wrong. This post is an effort to get it right.

Some say that once you understand monads you lose the ability to explain them. [^paradox] This can't bode well for this explanation, but let's give it a shot. A better question than 'what are monads and why would you use them' is 'what are the practical differences between monads and functors'. Functors by themselves are pretty powerful: the _Mostly Adequate_ guide explains them well, the gist being 'A Functor is a type that implements `map` and obeys some laws', with said laws shown below:

```javascript
// identity
map(id) === id;

// composition
compose(map(f), map(g)) === map(compose(f, g));

const compLaw1 = compose(map(append(" romanus ")), map(append(" sum")));
const compLaw2 = map(compose(append(" romanus "), append(" sum")));
compLaw1(Container.of("civis")); // Container("civis romanus sum")
compLaw2(Container.of("civis")); // Container("civis romanus sum")
```

The rest of this post will assume familiarity with the `Either`, `Option`, and `IO` types, the latter of which is implemented in `fp-ts/IO`. Note that all three of these types are both monads and functors, as all monads are functors but not vice versa. The characteristic function of functors is `map`, which calls a function on the value contained within the functor. The IO type specific `map` function has been imported as `mapIO` below:

```typescript
import { pipe } from "fp-ts/function";
import { IO, chain as chainIO, map as mapIO, of as ofIO } from "fp-ts/IO";

const effect: IO<string> = ofIO("myString");

const singleMap: (fa: IO<string>) => IO<string> = mapIO((input: string) =>
  input.toUpperCase(),
);
const singleMapFunction: IO<string> = singleMap(effect);
const singleMapApplied: string = singleMapFunction(); // = "MYSTRING"
```

In the segment above, the argument to `mapIO` is a function that accepts a string and returns another string. The return value is itself a function, and it accepts an `IO<string>` and returns another `IO<string>`. This can be useful to sequentially apply multiple successive functions on the functor's value. While this can look messy in languages without a pipeline operator, the `fp-ts` pipe function can clean this up making `doubleMapMessy` and `doubleMapPipe` equivalent in the following.

```typescript
const doubleMapMessy: IO<string> = mapIO((input: string) =>
  input.toUpperCase(),
)(mapIO((input: string) => input.repeat(2))(effect));

const doubleMapPipe: IO<string> = pipe(
  effect,
  mapIO((input: string) => input.toUpperCase()),
  mapIO((input: string) => input.repeat(2)),
);
const doubleMapPipeApplied: string = doubleMapPipe()// = "MYSTRINGMYSTRING"
```

While all the functions provided to `mapIO` thus far have had types `IO<string> => IO<string>`, note the type signature of `mapIO`:

```typescript
export declare const map: <A, B>(f: (a: A) => B) => (fa: IO<A>) => IO<B>
```

This means that we can use `map` with `string => void` functions, which would be appropriate for logging to standard output. To make things clearer, I've added some type information in the segment below.

```typescript
const mapWithSideEffect: IO<void> = pipe(
  effect,
  // IO<string>
  mapIO((input: string) => input.toUpperCase()),
  //(f: (a: A) => B) => (fa: IO<A>) => IO<B>; A: string, B: string
  mapIO((input: string) => input.repeat(2)),
  //(f: (a: A) => B) => (fa: IO<A>) => IO<B>; A: string, B: string
  mapIO((input: string) => console.log(input)),
  //(f: (a: A) => B) => (fa: IO<A>) => IO<B>; A: string, B: void
);

//Logs "MYSTRINGMYSTRING"
mapWithSideEffect();
```

Given all of this, why bother with monads? While we've only seen examples from the IO monad, everything shown here would allow you to convert `Option<string>` to `Option<number>` while deserialising a property that may not exist, or from `Result<UserValidationError, User>`  to `Result<BalanceValidationError, Balance>`.

A problem arises when we need to combine different `IO` operations. Let's say we have a `logFilePath` in `myConfig.json`. It is entirely reasonable for an application to read the log file path from the configuration file before writing to the log, but map functions aren't meant to handle this. Strangely, the segment below will compile even though the type hint for `pureMapWriteToConfig` is incorrect: it should be `IO<() => void>` instead as shown by the related comment. Because of this, `mapWriteToConfig()` doesn't actually log anything, which shouldn't have surprised us in the first place.

```typescript
import { pipe } from "fp-ts/function";
import { IO, chain as chainIO, map as mapIO, of as ofIO } from "fp-ts/IO";

import { readFileSync, writeFileSync } from "node:fs";
interface Config {
  logFilePath: string;
}

const getFileJson =
  (fileName: string): IO<Config> =>
  () =>
    JSON.parse(readFileSync(fileName, "utf-8")) as Config;

const pureMapWriteToConfig = (configFileName: string, log: string): IO<void> =>
  pipe(
    getFileJson(configFileName),
    mapIO((config: Config) => {
      console.log(`Map config: ${JSON.stringify(config)}`);
      return config;
    }),
    mapIO((config: Config) => () => writeFileSync(config.logFilePath, log)),
    // mapIO<Config, () => void>(f: (a: Config) => () => void): (fa: IO<Config>) => IO<() => void>
  );

const mapWriteToConfig = pureMapWriteToConfig("mapConfig.json", "myMapLog");
mapWriteToConfig();

/*
$ wc -l mapLog.txt
  0 mapLog.txt
*/
```

Once we have the config file represented as an `IO<Config>` instance, we need to read it and do an IO operation for the log file. That means we need some function that can accept a `(config: Config) => IO<void>` as well as the incoming `IO<Config>`. This is known by a few different names, including `bind` and `flatMap`, but `fp-ts` calls this `chain`. This is the characteristic function that separates monads from non-monad functors:

```typescript
export declare const chain: <A, B>(f: (a: A) => IO<B>) => (ma: IO<A>) => IO<B>
```

The `chain` function is imported as `chainIO`, so `chainWriteToConfig` logs successfully when called.

```typescript
const pureChainWriteToConfig = (
  configFileName: string,
  log: string,
): IO<void> =>
  pipe(
    getFileJson(configFileName),
    mapIO((config: Config) => {
      console.log(`Chain config: ${JSON.stringify(config)}`);
      return config;
    }),
    chainIO((config: Config) => () => writeFileSync(config.logFilePath, log)),
    //chainIO<Config, void>(f: (a: Config) => IO<void>): (ma: IO<Config>) => IO<void>
  );

const chainWriteToConfig = pureChainWriteToConfig(
  "chainConfig.json",
  "myChainLog",
);
chainWriteToConfig();

/*
$ cat chainLog.txt
myChainLog
*/

```

Chapter 9 of the _Mostly Adequate_ guide explains monads somewhat differently. They introduce `join` as the flattening of `Monad<Mondad<T>>` into `Monad<T>` making `chain` equivalent to a `map` followed by a `join`. The examples they provided use function composition rather than pipes, but this doesn't change much.

A few takeaways
1) The `pureMapWriteToConfig` type inference failure is unsettling and has made me lose some confidence in how the TypeScript compiler handles `fp-ts`.
2) It is now more obvious to me why Haskell's higher-order types matter. While one has to import type-specific `map` and `chain` functions in `fp-ts`, but it is my understanding that this isn't necessary in Haskell, allowing for `$` and `>==` operators to map and bind respectively.
3) It is now more obvious to me why you can get so far without understanding monads in impure functional programming languages. I'd argue that functors are more generally applicable than monads, but operations with multiple IO calls are more common than operations with multiple `Either` or `Option` instances, so in pure languages you're forced to recon with this sooner.
4) The only references to functional purity in this post were the function names `pureMapWriteToConfig` and `pureChainWriteToConfig`. While many discussions of monads will reference carrying out IO or mutable state without breaking functional purity, this isn't all that helpful in explaining what monads do given a) functors enable similar behaviour and b) `chain`/`bind` can be useful in situations where no side effects are carried out.

[^ramda]: [Ramda NPM Package](https://www.npmjs.com/package/ramda)
[^fpts]: [`fp-ts` NPM Package](https://www.npmjs.com/package/fp-ts). Note: `fp-ts` will appear in code segments in this article, consistent with the package documentation
[^mostly-adequate]: [Mostly Adequate Guide to Functional Programming](https://github.com/MostlyAdequate/mostly-adequate-guide). Note: it appears that [Brian Lonsdorf](https://www.linkedin.com/in/drboolean/) started the project, but there are many other contributors to the current repository
[^paradox]: Unfortunately I have forgotten where I first read this, it isn't my original quip
