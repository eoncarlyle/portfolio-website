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
