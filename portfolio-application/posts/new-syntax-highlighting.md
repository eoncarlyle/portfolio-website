---
title: This blog's new syntax highlighting
date: 2025.03.02
---
# This blog's new syntax highlighting

This blog is written from scratch with my [custom static site generator](https://github.com/eoncarlyle/portfolio-website), and up until a few days ago I was using Prism for client-side syntax highlighting. [^prismjs] This added about 34.3 kB of JavaScript to support F#, TypeScript, and JavaScript syntax highlighting. In the grand scheme of things this is a small bundle, but this always bothered me because syntax highlighting is the only part of the website that needed _any_ JavaScript. Using client-side syntax highlighting required a manual change whenever a new language is used in a code segment: the syntax highlighting rules were limited to only those that I was actively using for the smallest possible JavaScript bundle. That's not something I would want to slow me down as I'm learning and writing more about Kotlin and C this year, and that's not even counting the XML later on in this post. The advantage of server-side syntax highlighting is that you can support a comical number of languages without any impact on bundle size.

I started by trying to do a port of Prism to F# and then run it in the same way that Prism can be used server-side. But Prism is a pretty old piece of JavaScript first made public in 2012 so it relies on dynamic typing exactly as much as you'd expect. [^introducing-prism] The `matchGrammar` function below takes a syntax highlighting grammar - a collection of regular expressions to match language features - and applies it to the text to be highlighted. Server side Prism is called like `const html = Prism.highlight('const code = "var data = 1"', Prism.languages.javascript, "javascript")`, meaning that the `tokenize` function is called such that `tokenList` originally has a single element that matches `text`. Type modeling of `grammar` isn't impossible, but it is a pretty permissive type that includes circular references and properties that can be either arrays or strings.[^prismjs-types-note] The whole idea behind adapting Prism was that the small size of the library would make a re-write relatively short and still allow for using the existing language grammars, but that wasn't happening. For a little while I even tried to run JavaScript from .NET inside of the application; this is supposed to be possible but it's a pretty janky setup and I wasn't able to get it to work. [^js-in-dotnet]


```javascript
/**
	 * @param {string} text
	 * @param {LinkedList<string | Token>} tokenList
	 * @param {any} grammar
	 * @param {LinkedListNode<string | Token>} startNode
	 * @param {number} startPos
	 * @param {RematchOptions} [rematch]
	 * @returns {void}
	 * @private
	 *
	 * @typedef RematchOptions
	 * @property {string} cause
	 * @property {number} reach
	 */
	function matchGrammar(text, tokenList, grammar, startNode, startPos, rematch) {
	 //...
	}
```

I then had the realisation that I didn't need to run the syntax highlighting at runtime. The way that I am making static pages is by taking a markdown page at application startup and using Markdig to parse it to HTML. [^markdig] On page load, Prism would run in a script tag to apply the highlighting. However, placing the HTML from a syntax highlighter directly into the pages before application startup would allow me to avoid any changes on the .NET side. A Node script running at build time would load the posts, apply syntax highlighting, and then place them in to the application's `WebRoot` static file directory. I needed to place the syntax highlighted files into the correct `WebRoot` for both debug and release builds and wait for those directories to exist before running the syntax highlighter.
This was accomplished with a couple of MSBuild commands in the `portfolio-website.fsproj` file as shown below. It took me longer than it should have to get the output directories correct, but I don't really want to invest that much time in understanding MSBuild.

```xml
  <PropertyGroup>
    <SyntaxHighlighterOutputDir Condition="'$(Configuration)' == 'Release'">out/WebRoot/markdown</SyntaxHighlighterOutputDir>
    <SyntaxHighlighterOutputDir Condition="'$(Configuration)' != 'Release'">$(MSBuildProjectDirectory)/bin/$(Configuration)/$(TargetFramework)/WebRoot/markdown</SyntaxHighlighterOutputDir>
  </PropertyGroup>

  <Target Name="InstallNodePackages" BeforeTargets="PrepareForBuild">
    <Message Text="[MSBuild] Installing Syntax Highlighter Node Packages" Importance="high" />
    <Exec Command="npm --prefix $(ProjectDir)syntax-highlighting install" />
  </Target>

  <Target Name="EnsureOutputDirectoryExists" BeforeTargets="RunSyntaxHighlighter">
    <MakeDir Directories="$(SyntaxHighlighterOutputDir)" />
    <Message Text="[MSBuild] Ensuring output directory exists: $(SyntaxHighlighterOutputDir)" Importance="high" />
  </Target>

  <Target Name="RunSyntaxHighlighter" DependsOnTargets="InstallNodePackages;EnsureOutputDirectoryExists" BeforeTargets="PrepareForBuild">
    <Message Text="[MSBuild] Running Syntax Highlighter, Migrating Posts" Importance="high" />
    <Exec Command="node $(ProjectDir)syntax-highlighting/index.js posts $(SyntaxHighlighterOutputDir)" />
  </Target>
```

The new syntax highlighting script isn't what anyone would call elegant. Client-side Prism would find all `<code>` elements and apply the appropriate highlighting, but here I have to use some regular expressions to find the code blocks and language definitions. It's pretty fragile; I faced some initial issues with the triple backtick counting in the code block below. The string `replace()` to modify the style should absolutely replaced with proper HTML parsing, but that's a problem for later on. While I was going to all of this effort, I decided to switch out the syntax highlighting library to Shiki, which uses inline HTML styling rather than a stylesheet, cutting out a stylesheet that I had to send. [^shiki]

```javascript
  const sliceIncludingLang = currentFileContents.slice(backticks[index], backticks[index + 1]);
  const fistNewlineIndex = sliceIncludingLang.indexOf("\n");

  if (fistNewlineIndex === -1) throw Error("[Syntax Highlighting]: Newline not found when expected");

  const language = sliceIncludingLang.match(/\`\`\`(\w+)\n/)[1];
  const sliceWithoutLang = sliceIncludingLang.slice(fistNewlineIndex + 1, backticks[index + 1]);

  const defaultSyntaxHighlighting = await codeToHtml(sliceWithoutLang, { lang: language, theme: "catppuccin-mocha" });
  const adjustedSyntaxHighlighting = defaultSyntaxHighlighting.replace(
    "background-color:#1e1e2e;color:#cdd6f4",
    "background-color:#181825;color:#cdd6f4;padding:1em;border-radius:0.3em;overflow:auto",
  );
  const sliceIncludingLangAndClosingBackticks = currentFileContents.slice(backticks[index], backticks[index + 1] + 3);
  replacementPair.push([sliceIncludingLangAndClosingBackticks, adjustedSyntaxHighlighting]);
```

One thing I noticed when working on the syntax highlighting was that my means to bypass connecting to Apache ZooKeeper during development wasn't working. My production containers connect to Apache ZooKeeper as part of service discovery for my reverse proxy, but I don't want to handle this during development. [^zk-reverse-proxy]

```fsharp

module AppZooKeeper
//...
let configureZookeeper (zkConnectString: string) (hostAddress: string) (hostPort: string) =
    task {
        match zkConnectString with
        | "-1" -> ()
        | _ ->
            let zooKeeper = getZooKeeper zkConnectString
            let! targetListStat = zooKeeper.existsAsync TARGETS_ZNODE_PATH
            let currentTargetZnodePath = getCurrentTargetZnodePath hostAddress hostPort

            if (isNull targetListStat) then zooKeeper.createAsync (TARGETS_ZNODE_PATH, null, ZooDefs.Ids.OPEN_ACL_UNSAFE, CreateMode.PERSISTENT)
                    |> ignore
            //...
```

However, whenever I use `"-1"` as a ZooKeeper connect string during development, the following logs are created in directory where I run the application.

```text
[2025-03-02 03:47:14.695 GMT 	ERROR 	DynamicHostProvider 	Failed resolving Host=-1]
Exc level 0: System.Net.Sockets.SocketException: nodename nor servname provided, or not known
   at System.Net.Dns.GetHostEntryOrAddressesCore(String hostName, Boolean justAddresses, AddressFamily addressFamily, Nullable`1 startingTimestamp)
   at System.Net.Dns.<>c.<GetHostEntryOrAddressesCoreAsync>b__33_0(Object s, Int64 startingTimestamp)
   at System.Net.Dns.<>c__DisplayClass39_0`1.<RunAsync>b__0(Task <p0>, Object <p1>)
   at System.Threading.Tasks.ContinuationResultTaskFromTask`1.InnerInvoke()
   at System.Threading.ExecutionContext.RunFromThreadPoolDispatchLoop(Thread threadPoolThread, ExecutionContext executionContext, ContextCallback callback, Object state)
```

[^prismjs]: [Prism NPM Package](https://www.npmjs.com/package/prismjs)
[^introducing-prism]: [Lea Verou's "Introducing Prism"](https://lea.verou.me/blog/2012/07/introducing-prism-an-awesome-new-syntax-highlighter)
[^js-in-dotnet]: ["Running JavaScript inside a .NET app with JavaScriptEngineSwitcher
"](https://andrewlock.net/running-javascript-in-a-dotnet-app-with-javascriptengineswitcher/)
[^prismjs-types-note]: I didn't even realise that a [@types/prismjs](https://www.npmjs.com/package/@types/prismjs) package existed until writing this post, which certainly would have helped
[^markdig]: [Markdig NuGet Package](https://www.nuget.org/packages/Markdig)
[^shiki]: [Shiki NPM Package](https://www.npmjs.com/package/shiki)
[^zk-reverse-proxy]: [Prior post "My needlessly complicated ZooKeeper-enabled reverse proxy"](https://iainschmitt.com/post/my-needlessly-complicated-reverse-proxy)
