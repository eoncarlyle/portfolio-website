---
title: Understanding monads?
date: 2024.12.26
---
# Understanding monads?

The Node reverse proxy explained in my <a href="/post/my-needlessly-complicated-reverse-proxy">last post</a> sprinkled in a little bit of functional TypeScript through Ramda. [^ramda] This naturally gave me an excuse to talk about the project at the functional programming group at work, but some of the feedback that I got was that Ramda takes advantage of the permissive nature of JavaScript types, so `fp-ts` is a better functional library for TypeScript.[^fpts] _Professor Frisby's Mostly Adequate Guide to Functional Programming_ is linked by the `fp-ts` docs in lieu of a more comprehensive tutorial for the library, and it is a wonderful resource for functional programming in JavaScript. [^mostly-adequate] Chapters 8 and 9 explain functors and monads respectively, and after reading them I told a developer friend of mine 'I recently made a breakthrough in understanding monads' before sending him some notes explaining why. Said notes are available [here](https://gist.github.com/eoncarlyle/309b7da602df8ea6d4ab7a01fc83bcbb), but the next morning I stared at the relevant type signatures long enough to understand that nearly everything about my explanation was wrong. This post is an effort to get it right.

Some say that once you understand monads you lose the ability to explain them. [^paradox] This can't bode well for this explanation, but let's give it a shot. A better question than 'what are monads and why would you use them' is 'what are the practical differences between monads and functors'. Functors by themselves are pretty powerful: the _Mostly Adequate_ guide explains them well, the gist being 'A Functor is a type that implements `map` and obeys some laws', with said laws shown below:

<pre class="language-javascript tabindex="0">
<code class="language-javascript>
<span class="token comment">// identity</span>
<span class="token function">map</span><span class="token punctuation">(</span>id<span class="token punctuation">)</span> <span class="token operator">===</span> id<span class="token punctuation">;</span>

<span class="token comment">// composition</span>
<span class="token function">compose</span><span class="token punctuation">(</span><span class="token function">map</span><span class="token punctuation">(</span>f<span class="token punctuation">)</span><span class="token punctuation">,</span> <span class="token function">map</span><span class="token punctuation">(</span>g<span class="token punctuation">)</span><span class="token punctuation">)</span> <span class="token operator">===</span> <span class="token function">map</span><span class="token punctuation">(</span><span class="token function">compose</span><span class="token punctuation">(</span>f<span class="token punctuation">,</span> g<span class="token punctuation">)</span><span class="token punctuation">)</span><span class="token punctuation">;</span>

<span class="token keyword">const</span> compLaw1 <span class="token operator">=</span> <span class="token function">compose</span><span class="token punctuation">(</span><span class="token function">map</span><span class="token punctuation">(</span><span class="token function">append</span><span class="token punctuation">(</span><span class="token string">" romanus "</span><span class="token punctuation">)</span><span class="token punctuation">)</span><span class="token punctuation">,</span> <span class="token function">map</span><span class="token punctuation">(</span><span class="token function">append</span><span class="token punctuation">(</span><span class="token string">" sum"</span><span class="token punctuation">)</span><span class="token punctuation">)</span><span class="token punctuation">)</span><span class="token punctuation">;</span>
<span class="token keyword">const</span> compLaw2 <span class="token operator">=</span> <span class="token function">map</span><span class="token punctuation">(</span><span class="token function">compose</span><span class="token punctuation">(</span><span class="token function">append</span><span class="token punctuation">(</span><span class="token string">" romanus "</span><span class="token punctuation">)</span><span class="token punctuation">,</span> <span class="token function">append</span><span class="token punctuation">(</span><span class="token string">" sum"</span><span class="token punctuation">)</span><span class="token punctuation">)</span><span class="token punctuation">)</span><span class="token punctuation">;</span>
<span class="token function">compLaw1</span><span class="token punctuation">(</span><span class="token maybe-class-name">Container</span><span class="token punctuation">.</span><span class="token method function property-access">of</span><span class="token punctuation">(</span><span class="token string">"civis"</span><span class="token punctuation">)</span><span class="token punctuation">)</span><span class="token punctuation">;</span> <span class="token comment">// Container("civis romanus sum")</span>
<span class="token function">compLaw2</span><span class="token punctuation">(</span><span class="token maybe-class-name">Container</span><span class="token punctuation">.</span><span class="token method function property-access">of</span><span class="token punctuation">(</span><span class="token string">"civis"</span><span class="token punctuation">)</span><span class="token punctuation">)</span><span class="token punctuation">;</span> <span class="token comment">// Container("civis romanus sum")</span>
</code>
</pre>

The rest of this post will assume familiarity with the `Either`, `Option`, and `IO` types, the latter of which is implemented in `fp-ts/IO`. Note that all three of these types are both monads and functors, as all monads are functors but not vice versa. The characteristic function of functors is `map`, which calls a function on the value contained within the functor. The IO type specific `map` function has been imported as `mapIO` below:

<pre class="language-typescript tabindex="0">
<code class="language-typescript>
<span class="token keyword">import</span> <span class="token punctuation">{</span> pipe <span class="token punctuation">}</span> <span class="token keyword">from</span> <span class="token string">"fp-ts/function"</span><span class="token punctuation">;</span>
<span class="token keyword">import</span> <span class="token punctuation">{</span> <span class="token constant">IO</span><span class="token punctuation">,</span> chain <span class="token keyword">as</span> chainIO<span class="token punctuation">,</span> map <span class="token keyword">as</span> mapIO<span class="token punctuation">,</span> <span class="token keyword">of</span> <span class="token keyword">as</span> ofIO <span class="token punctuation">}</span> <span class="token keyword">from</span> <span class="token string">"fp-ts/IO"</span><span class="token punctuation">;</span>

<span class="token keyword">const</span> effect<span class="token operator">:</span> <span class="token constant">IO</span><span class="token operator">&lt;</span><span class="token builtin">string</span><span class="token operator">></span> <span class="token operator">=</span> <span class="token function">ofIO</span><span class="token punctuation">(</span><span class="token string">"myString"</span><span class="token punctuation">)</span><span class="token punctuation">;</span>

<span class="token keyword">const</span> <span class="token function-variable function">singleMap</span><span class="token operator">:</span> <span class="token punctuation">(</span>fa<span class="token operator">:</span> <span class="token constant">IO</span><span class="token operator">&lt;</span><span class="token builtin">string</span><span class="token operator">></span><span class="token punctuation">)</span> <span class="token operator">=></span> <span class="token constant">IO</span><span class="token operator">&lt;</span><span class="token builtin">string</span><span class="token operator">></span> <span class="token operator">=</span> <span class="token function">mapIO</span><span class="token punctuation">(</span><span class="token punctuation">(</span>input<span class="token operator">:</span> <span class="token builtin">string</span><span class="token punctuation">)</span> <span class="token operator">=></span>
  input<span class="token punctuation">.</span><span class="token function">toUpperCase</span><span class="token punctuation">(</span><span class="token punctuation">)</span><span class="token punctuation">,</span>
<span class="token punctuation">)</span><span class="token punctuation">;</span>
<span class="token keyword">const</span> singleMapFunction<span class="token operator">:</span> <span class="token constant">IO</span><span class="token operator">&lt;</span><span class="token builtin">string</span><span class="token operator">></span> <span class="token operator">=</span> <span class="token function">singleMap</span><span class="token punctuation">(</span>effect<span class="token punctuation">)</span><span class="token punctuation">;</span>
<span class="token keyword">const</span> singleMapApplied<span class="token operator">:</span> <span class="token builtin">string</span> <span class="token operator">=</span> <span class="token function">singleMapFunction</span><span class="token punctuation">(</span><span class="token punctuation">)</span><span class="token punctuation">;</span> <span class="token comment">// = "MYSTRING"</span>
</code>
</pre>

In the segment above, the argument to `mapIO` is a function that accepts a string and returns another string. The return value is itself a function, and it accepts an `IO<string>` and returns another `IO<string>`. This can be useful to sequentially apply multiple successive functions on the functor's value. While this can look messy in languages without a pipeline operator, the `fp-ts` pipe function can clean this up making `doubleMapMessy` and `doubleMapPipe` equivalent in the following.

<pre class="language-typescript tabindex="0">
<code class="language-typescript>
<span class="token keyword">const</span> doubleMapMessy<span class="token operator">:</span> <span class="token constant">IO</span><span class="token operator">&lt;</span><span class="token builtin">string</span><span class="token operator">></span> <span class="token operator">=</span> <span class="token function">mapIO</span><span class="token punctuation">(</span><span class="token punctuation">(</span>input<span class="token operator">:</span> <span class="token builtin">string</span><span class="token punctuation">)</span> <span class="token operator">=></span>
  input<span class="token punctuation">.</span><span class="token function">toUpperCase</span><span class="token punctuation">(</span><span class="token punctuation">)</span><span class="token punctuation">,</span>
<span class="token punctuation">)</span><span class="token punctuation">(</span><span class="token function">mapIO</span><span class="token punctuation">(</span><span class="token punctuation">(</span>input<span class="token operator">:</span> <span class="token builtin">string</span><span class="token punctuation">)</span> <span class="token operator">=></span> input<span class="token punctuation">.</span><span class="token function">repeat</span><span class="token punctuation">(</span><span class="token number">2</span><span class="token punctuation">)</span><span class="token punctuation">)</span><span class="token punctuation">(</span>effect<span class="token punctuation">)</span><span class="token punctuation">)</span><span class="token punctuation">;</span>

<span class="token keyword">const</span> doubleMapPipe<span class="token operator">:</span> <span class="token constant">IO</span><span class="token operator">&lt;</span><span class="token builtin">string</span><span class="token operator">></span> <span class="token operator">=</span> <span class="token function">pipe</span><span class="token punctuation">(</span>
  effect<span class="token punctuation">,</span>
  <span class="token function">mapIO</span><span class="token punctuation">(</span><span class="token punctuation">(</span>input<span class="token operator">:</span> <span class="token builtin">string</span><span class="token punctuation">)</span> <span class="token operator">=></span> input<span class="token punctuation">.</span><span class="token function">toUpperCase</span><span class="token punctuation">(</span><span class="token punctuation">)</span><span class="token punctuation">)</span><span class="token punctuation">,</span>
  <span class="token function">mapIO</span><span class="token punctuation">(</span><span class="token punctuation">(</span>input<span class="token operator">:</span> <span class="token builtin">string</span><span class="token punctuation">)</span> <span class="token operator">=></span> input<span class="token punctuation">.</span><span class="token function">repeat</span><span class="token punctuation">(</span><span class="token number">2</span><span class="token punctuation">)</span><span class="token punctuation">)</span><span class="token punctuation">,</span>
<span class="token punctuation">)</span><span class="token punctuation">;</span>
<span class="token keyword">const</span> doubleMapPipeApplied<span class="token operator">:</span> <span class="token builtin">string</span> <span class="token operator">=</span> <span class="token function">doubleMapPipe</span><span class="token punctuation">(</span><span class="token punctuation">)</span><span class="token comment">// = "MYSTRINGMYSTRING"</span>
</code>
</pre>

While all the functions provided to `mapIO` thus far have had types `IO<string> => IO<string>`, note the type signature of `mapIO`:

<pre class="language-typescript tabindex="0">
<code class="language-typescript>
<span class="token keyword">export</span> <span class="token keyword">declare</span> <span class="token keyword">const</span> map<span class="token operator">:</span> <span class="token operator">&lt;</span><span class="token constant">A</span><span class="token punctuation">,</span> <span class="token constant">B</span><span class="token operator">></span><span class="token punctuation">(</span><span class="token function-variable function">f</span><span class="token operator">:</span> <span class="token punctuation">(</span>a<span class="token operator">:</span> <span class="token constant">A</span><span class="token punctuation">)</span> <span class="token operator">=></span> <span class="token constant">B</span><span class="token punctuation">)</span> <span class="token operator">=></span> <span class="token punctuation">(</span>fa<span class="token operator">:</span> <span class="token constant">IO</span><span class="token operator">&lt;</span><span class="token constant">A</span><span class="token operator">></span><span class="token punctuation">)</span> <span class="token operator">=></span> <span class="token constant">IO</span><span class="token operator">&lt;</span><span class="token constant">B</span><span class="token operator">></span>
</code>
</pre>

This means that we can use `map` with `string => void` functions, which would be appropriate for logging to standard output. To make things clearer, I've added some type information in the segment below.

<pre class="language-typescript tabindex="0">
<code class="language-typescript>
<span class="token keyword">const</span> mapWithSideEffect<span class="token operator">:</span> <span class="token constant">IO</span><span class="token operator">&lt;</span><span class="token keyword">void</span><span class="token operator">></span> <span class="token operator">=</span> <span class="token function">pipe</span><span class="token punctuation">(</span>
  effect<span class="token punctuation">,</span>
  <span class="token comment">// IO&lt;string></span>
  <span class="token function">mapIO</span><span class="token punctuation">(</span><span class="token punctuation">(</span>input<span class="token operator">:</span> <span class="token builtin">string</span><span class="token punctuation">)</span> <span class="token operator">=></span> input<span class="token punctuation">.</span><span class="token function">toUpperCase</span><span class="token punctuation">(</span><span class="token punctuation">)</span><span class="token punctuation">)</span><span class="token punctuation">,</span>
  <span class="token comment">//(f: (a: A) => B) => (fa: IO&lt;A>) => IO&lt;B>; A: string, B: string</span>
  <span class="token function">mapIO</span><span class="token punctuation">(</span><span class="token punctuation">(</span>input<span class="token operator">:</span> <span class="token builtin">string</span><span class="token punctuation">)</span> <span class="token operator">=></span> input<span class="token punctuation">.</span><span class="token function">repeat</span><span class="token punctuation">(</span><span class="token number">2</span><span class="token punctuation">)</span><span class="token punctuation">)</span><span class="token punctuation">,</span>
  <span class="token comment">//(f: (a: A) => B) => (fa: IO&lt;A>) => IO&lt;B>; A: string, B: string</span>
  <span class="token function">mapIO</span><span class="token punctuation">(</span><span class="token punctuation">(</span>input<span class="token operator">:</span> <span class="token builtin">string</span><span class="token punctuation">)</span> <span class="token operator">=></span> <span class="token builtin">console</span><span class="token punctuation">.</span><span class="token function">log</span><span class="token punctuation">(</span>input<span class="token punctuation">)</span><span class="token punctuation">)</span><span class="token punctuation">,</span>
  <span class="token comment">//(f: (a: A) => B) => (fa: IO&lt;A>) => IO&lt;B>; A: string, B: void</span>
<span class="token punctuation">)</span><span class="token punctuation">;</span>

<span class="token comment">//Logs "MYSTRINGMYSTRING"</span>
<span class="token function">mapWithSideEffect</span><span class="token punctuation">(</span><span class="token punctuation">)</span><span class="token punctuation">;</span>
</code>
</pre>

Given all of this, why bother with monads? While we've only seen examples from the IO monad, everything shown here would allow you to convert `Option<string>` to `Option<number>` while deserialising a property that may not exist, or from `Result<UserValidationError, User>`  to `Result<BalanceValidationError, Balance>`.

A problem arises when we need to combine different `IO` operations. Let's say we have a `logFilePath` in `myConfig.json`. It is entirely reasonable for an application to read the log file path from the configuration file before writing to the log, but map functions aren't meant to handle this. Strangely, the segment below will compile even though the type hint for `pureMapWriteToConfig` is incorrect: it should be `IO<() => void>` instead as shown by the related comment. Because of this, `mapWriteToConfig()` doesn't actually log anything, which shouldn't have surprised us in the first place.

<pre class="language-typescript tabindex="0">
<code class="language-typescript>
<span class="token keyword">import</span> <span class="token punctuation">{</span> pipe <span class="token punctuation">}</span> <span class="token keyword">from</span> <span class="token string">"fp-ts/function"</span><span class="token punctuation">;</span>
<span class="token keyword">import</span> <span class="token punctuation">{</span> <span class="token constant">IO</span><span class="token punctuation">,</span> chain <span class="token keyword">as</span> chainIO<span class="token punctuation">,</span> map <span class="token keyword">as</span> mapIO<span class="token punctuation">,</span> <span class="token keyword">of</span> <span class="token keyword">as</span> ofIO <span class="token punctuation">}</span> <span class="token keyword">from</span> <span class="token string">"fp-ts/IO"</span><span class="token punctuation">;</span>

<span class="token keyword">import</span> <span class="token punctuation">{</span> readFileSync<span class="token punctuation">,</span> writeFileSync <span class="token punctuation">}</span> <span class="token keyword">from</span> <span class="token string">"node:fs"</span><span class="token punctuation">;</span>
<span class="token keyword">interface</span> <span class="token class-name">Config</span> <span class="token punctuation">{</span>
  logFilePath<span class="token operator">:</span> <span class="token builtin">string</span><span class="token punctuation">;</span>
<span class="token punctuation">}</span>

<span class="token keyword">const</span> getFileJson <span class="token operator">=</span>
  <span class="token punctuation">(</span>fileName<span class="token operator">:</span> <span class="token builtin">string</span><span class="token punctuation">)</span><span class="token operator">:</span> <span class="token constant">IO</span><span class="token operator">&lt;</span>Config<span class="token operator">></span> <span class="token operator">=></span>
  <span class="token punctuation">(</span><span class="token punctuation">)</span> <span class="token operator">=></span>
    <span class="token constant">JSON</span><span class="token punctuation">.</span><span class="token function">parse</span><span class="token punctuation">(</span><span class="token function">readFileSync</span><span class="token punctuation">(</span>fileName<span class="token punctuation">,</span> <span class="token string">"utf-8"</span><span class="token punctuation">)</span><span class="token punctuation">)</span> <span class="token keyword">as</span> Config<span class="token punctuation">;</span>

<span class="token keyword">const</span> pureMapWriteToConfig <span class="token operator">=</span> <span class="token punctuation">(</span>configFileName<span class="token operator">:</span> <span class="token builtin">string</span><span class="token punctuation">,</span> log<span class="token operator">:</span> <span class="token builtin">string</span><span class="token punctuation">)</span><span class="token operator">:</span> <span class="token constant">IO</span><span class="token operator">&lt;</span><span class="token keyword">void</span><span class="token operator">></span> <span class="token operator">=></span>
  <span class="token function">pipe</span><span class="token punctuation">(</span>
    <span class="token function">getFileJson</span><span class="token punctuation">(</span>configFileName<span class="token punctuation">)</span><span class="token punctuation">,</span>
    <span class="token function">mapIO</span><span class="token punctuation">(</span><span class="token punctuation">(</span>config<span class="token operator">:</span> Config<span class="token punctuation">)</span> <span class="token operator">=></span> <span class="token punctuation">{</span>
      <span class="token builtin">console</span><span class="token punctuation">.</span><span class="token function">log</span><span class="token punctuation">(</span><span class="token template-string"><span class="token template-punctuation string">`</span><span class="token string">Map config: </span><span class="token interpolation"><span class="token interpolation-punctuation punctuation">${</span><span class="token constant">JSON</span><span class="token punctuation">.</span><span class="token function">stringify</span><span class="token punctuation">(</span>config<span class="token punctuation">)</span><span class="token interpolation-punctuation punctuation">}</span></span><span class="token template-punctuation string">`</span></span><span class="token punctuation">)</span><span class="token punctuation">;</span>
      <span class="token keyword">return</span> config<span class="token punctuation">;</span>
    <span class="token punctuation">}</span><span class="token punctuation">)</span><span class="token punctuation">,</span>
    <span class="token function">mapIO</span><span class="token punctuation">(</span><span class="token punctuation">(</span>config<span class="token operator">:</span> Config<span class="token punctuation">)</span> <span class="token operator">=></span> <span class="token punctuation">(</span><span class="token punctuation">)</span> <span class="token operator">=></span> <span class="token function">writeFileSync</span><span class="token punctuation">(</span>config<span class="token punctuation">.</span>logFilePath<span class="token punctuation">,</span> log<span class="token punctuation">)</span><span class="token punctuation">)</span><span class="token punctuation">,</span>
    <span class="token comment">// mapIO&lt;Config, () => void>(f: (a: Config) => () => void): (fa: IO&lt;Config>) => IO&lt;() => void></span>
  <span class="token punctuation">)</span><span class="token punctuation">;</span>

<span class="token keyword">const</span> mapWriteToConfig <span class="token operator">=</span> <span class="token function">pureMapWriteToConfig</span><span class="token punctuation">(</span><span class="token string">"mapConfig.json"</span><span class="token punctuation">,</span> <span class="token string">"myMapLog"</span><span class="token punctuation">)</span><span class="token punctuation">;</span>
<span class="token function">mapWriteToConfig</span><span class="token punctuation">(</span><span class="token punctuation">)</span><span class="token punctuation">;</span>

<span class="token comment">/*
$ wc -l mapLog.txt
  0 mapLog.txt
*/</span>
</code>
</pre>

Once we have the config file represented as an `IO<Config>` instance, we need to read it and do an IO operation for the log file. That means we need some function that can accept a `(config: Config) => IO<void>` as well as the incoming `IO<Config>`. This is known by a few different names, including `bind` and `flatMap`, but `fp-ts` calls this `chain`. This is the characteristic function that separates monads from non-monad functors:

<pre class="language-typescript tabindex="0">
<code class="language-typescript>
<span class="token keyword">export</span> <span class="token keyword">declare</span> <span class="token keyword">const</span> chain<span class="token operator">:</span> <span class="token operator">&lt;</span><span class="token constant">A</span><span class="token punctuation">,</span> <span class="token constant">B</span><span class="token operator">></span><span class="token punctuation">(</span><span class="token function-variable function">f</span><span class="token operator">:</span> <span class="token punctuation">(</span>a<span class="token operator">:</span> <span class="token constant">A</span><span class="token punctuation">)</span> <span class="token operator">=></span> <span class="token constant">IO</span><span class="token operator">&lt;</span><span class="token constant">B</span><span class="token operator">></span><span class="token punctuation">)</span> <span class="token operator">=></span> <span class="token punctuation">(</span>ma<span class="token operator">:</span> <span class="token constant">IO</span><span class="token operator">&lt;</span><span class="token constant">A</span><span class="token operator">></span><span class="token punctuation">)</span> <span class="token operator">=></span> <span class="token constant">IO</span><span class="token operator">&lt;</span><span class="token constant">B</span><span class="token operator">></span>
</code>
</pre>

The `chain` function is imported as `chainIO`, so `chainWriteToConfig` logs successfully when called.

<pre class="language-typescript tabindex="0">
<code class="language-typescript>
<span class="token keyword">const</span> pureChainWriteToConfig <span class="token operator">=</span> <span class="token punctuation">(</span>
  configFileName<span class="token operator">:</span> <span class="token builtin">string</span><span class="token punctuation">,</span>
  log<span class="token operator">:</span> <span class="token builtin">string</span><span class="token punctuation">,</span>
<span class="token punctuation">)</span><span class="token operator">:</span> <span class="token constant">IO</span><span class="token operator">&lt;</span><span class="token keyword">void</span><span class="token operator">></span> <span class="token operator">=></span>
  <span class="token function">pipe</span><span class="token punctuation">(</span>
    <span class="token function">getFileJson</span><span class="token punctuation">(</span>configFileName<span class="token punctuation">)</span><span class="token punctuation">,</span>
    <span class="token function">mapIO</span><span class="token punctuation">(</span><span class="token punctuation">(</span>config<span class="token operator">:</span> Config<span class="token punctuation">)</span> <span class="token operator">=></span> <span class="token punctuation">{</span>
      <span class="token builtin">console</span><span class="token punctuation">.</span><span class="token function">log</span><span class="token punctuation">(</span><span class="token template-string"><span class="token template-punctuation string">`</span><span class="token string">Chain config: </span><span class="token interpolation"><span class="token interpolation-punctuation punctuation">${</span><span class="token constant">JSON</span><span class="token punctuation">.</span><span class="token function">stringify</span><span class="token punctuation">(</span>config<span class="token punctuation">)</span><span class="token interpolation-punctuation punctuation">}</span></span><span class="token template-punctuation string">`</span></span><span class="token punctuation">)</span><span class="token punctuation">;</span>
      <span class="token keyword">return</span> config<span class="token punctuation">;</span>
    <span class="token punctuation">}</span><span class="token punctuation">)</span><span class="token punctuation">,</span>
    <span class="token function">chainIO</span><span class="token punctuation">(</span><span class="token punctuation">(</span>config<span class="token operator">:</span> Config<span class="token punctuation">)</span> <span class="token operator">=></span> <span class="token punctuation">(</span><span class="token punctuation">)</span> <span class="token operator">=></span> <span class="token function">writeFileSync</span><span class="token punctuation">(</span>config<span class="token punctuation">.</span>logFilePath<span class="token punctuation">,</span> log<span class="token punctuation">)</span><span class="token punctuation">)</span><span class="token punctuation">,</span>
    <span class="token comment">//chainIO&lt;Config, void>(f: (a: Config) => IO&lt;void>): (ma: IO&lt;Config>) => IO&lt;void></span>
  <span class="token punctuation">)</span><span class="token punctuation">;</span>

<span class="token keyword">const</span> chainWriteToConfig <span class="token operator">=</span> <span class="token function">pureChainWriteToConfig</span><span class="token punctuation">(</span>
  <span class="token string">"chainConfig.json"</span><span class="token punctuation">,</span>
  <span class="token string">"myChainLog"</span><span class="token punctuation">,</span>
<span class="token punctuation">)</span><span class="token punctuation">;</span>
<span class="token function">chainWriteToConfig</span><span class="token punctuation">(</span><span class="token punctuation">)</span><span class="token punctuation">;</span>

<span class="token comment">/*
$ cat chainLog.txt
myChainLog
*/</span>

</code>
</pre>

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
