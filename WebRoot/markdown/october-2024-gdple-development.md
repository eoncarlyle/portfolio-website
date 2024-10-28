
# October 2024 GDPLE Development

GDPLE is my [Wordle-inspired US state economy guessing game](https://gdple.iainschmitt.com/) that I wrote last year, but this October I did some work on both the frontend and backend that I thought warranted a blog post.

## Re-writing the GDPLE Backend in F\#

After <a href="/post/ddmf-review">reading <em>Domain Modeling Made Functional</em></a> I decided to re-write the GDPLE backend in F\#. The player is shown a breakdown of a US state's GDP by sector and has five attempts to guess the state correctly. Initially, the backend was written in TypeScript and Node using NestJS, which is based off of Express but includes some useful features for dependency injection and decorators to mark endpoints. While it strikes a good balance between something like Spring and vanilla Express, NestJS was overkill for the GDPLE backend, and you can end up writing [lots of boilerplate](https://github.com/eoncarlyle/state-economy-game/blob/a495631fe4455d88435f2bc4c8b1ac60b52b3c5d/state-economy-game-backend/src/app.module.ts) when using the library.

The prior NestJS controller class is shown below, and the new F# backend has the same endpoints. The frontend hits `POST /puzzle_session` to get a UUID for the player's attempt on that day's puzzle that is written to local storage. That UUID is included in `POST /guess` to submit a guess and `GET /answer/:id` if the player has exhausted their guesses before reaching the correct answer. The `GET /economy` provides the GDP breakdown of the mystery US state for the frontend to create a treemap visualisation of the mystery state economy.

```typescript
@Controller()
export class AppController {
  constructor(private readonly appService: AppService) {}

  @Get("/economy")
  async getTargetStateEconomy(): Promise<StateEconomy> { ... }

  @Post("/puzzle_session")
  async postPuzzleSession(): Promise<IPuzzleSession> { ... }

  @Post("/guess")
  async ostGuess(@Body() body: GuessSubmissionRequest): 
      Promise<GuessSubmissionResponse> { ... }
  
  @Get("/answer/:id")
  async getPuzzleAnswer(@Param() params: PuzzleAnswerRequest):
      Promise<PuzzleAnswerResponse> { ... }
}
```

F\# is a joy to write in, but because I was pretty directly re-writing TypeScript into F#, there are a couple of TypeScript language features that I missed. The US state economy data was represented in a JSON file, and being able to directly import the file with `import stateRecordList from "./UsStates"` was convenient. TypeScript union types made working with hierarchical economic data very succinct as shown by the `getTotalGdp` function below.

```typescript
export interface NonLeafEconomyNode {
  gdpCategory: string; 
  children: Array<NonLeafEconomyNode | LeafEconomyNode>;
}

export interface LeafEconomyNode {
  gdpCategory: string;
  gdp: number;
}

getTotalGdp(economy: NonLeafEconomyNode | LeafEconomyNode): number {
  if ("children" in economy) {
    return economy.children
        .map((node) => this.getTotalGdp(node))
        .reduce((prev, cur) => prev + cur, 0);
  } else {
    return economy.gdp;
  }
}
```

The function takes advantage of TypeScript's permissiveness and `if ("children" in economy)` isn't the best way to distinguish between the two node types. It is possible to do something equivalent with more type safety in F\# by replacing the direct JSON import with a JSON type provider class and using discriminated unions as shown below, but it ended up being unwieldy by introducing four separate types for the economy data. Because only the leaf nodes of the economy tree structure have GDP data, calculating total GDP means distinguishing between a leaf node that must return a GDP value and a non-leaf node where the GDP sum needs to be recursively called over all child nodes. I am sure there is a way to use option types more creatively and coerce the type provider into using them, but at first pass I wasn't able to make it work, and I wanted to get something up-and-running relatively quickly. Type providers are such a great F\# language feature, so I hope I'll soon find the time to get these working properly.

```fsharp
let getTotalGdp (economyNode: StateEconomies.Root) =
    let rec loop (node: Node) =
        match mode with
        | Leaf leaf -> leaf.Gdp
        | StateEconomy se -> (se.Children) |> Array.map loop |> array.sum
        | OuterChild oc -> oc.Children |> Array.map loop |> array.sum
        | MiddleChild mc -> mc.Children |> Array.map loop |> array.sum

    loop economyNode.StateEconomy |> Math.Round |> Convert.ToInt64
```

Another aspect of the backend that I want to return to pertains to the database. While I ended up using F\# Dapper, I originally wanted to use either an SQL type provider or SqlHydra for better database related type checking while editing, but these required referencing an OS-specific `.dll` and I didn't want the hassle while moving between MacOS and Linux while writing the new backend. When I tried to decrement the `id` column of every row in the `target_states` table (represented as `puzzleAnswerTable` in Dapper), I faced a compiler error as shown below because I wasn't able to use the `puzzleAnswer` field in such a self-referential way inside the `update` statement. This meant I had to use the raw query instead, with even less type safety than what F# Dapper provides:

```fsharp
let deleteObsoletePuzzleAnswers (dbConnection: DbConnection) =

    // More elegant, but wouldn't compile
    let updatePuzzleAnswer (puzzleAnswer: PuzzleAnswer) =
        {id=puzzleAnswer.id-obsoletePuzzleAnswerCount; name=puzzleAnswer.name; gdp=puzzleAnswer.gdp}
    // error FS0039: The value or constructor 'puzzleAnswer' is not defined.
    update {
        for puzzleAnswer in puzzleAnswerTable do
            set (updatePuzzleAnswer puzzleAnswer)
        } |> ignore

    // Cruder, but compiled
    dbConnection.Execute(
    $"UPDATE target_states SET id = id - {obsoletePuzzleAnswerCount}, updatedAt = CURRENT_TIMESTAMP"
    )
    |> ignore
```


## CI and Frontend Woes

While working on the F\# re-write I found out that GitHub allows for self-hosted CI/CD runners, and I wish I had found out about these sooner. All the CI/CD I had done in the past had been on GitLab CI/CD runners or Azure DevOps pipelines and I mistakenly thought that GitHub's equivalent - GitHub Actions - was a paid offering. This means that in the past, deploying one of my personal projects onto my VPS (virtual private server) meant either using `scp` or `git pull` before manually restarting some `systemd` services. Every once in a while I'd forget to restart NGINX, or I'd mess up file permissions by accidentally deploying as `root` and while this was a great way to stay on top of my system administration skills, it could get annoying and introduced more friction while trying to push out updates. My GitHub actions for the backend were straightforward to implement but not very sophisticated: I'm running the actions directly on the VPS hosting the backend. But the CI/CD Actions made my life much easier while addressing the bugs on the F\# backend. Instead of having to SSH onto the VPS for every fix, I could push out small fixes over lunch.

The CI story was more interesting for the frontend: I kept [running out of memory](https://github.com/eoncarlyle/state-economy-game/actions/runs/11154820214/job/31004718166#step:3:6196) while running `vite build`, which wasn't terribly surprising given the 1 GB of memory on the VPS:

```text
<--- Last few GCs --->

[1044134:0x67be140] 48423 ms: Scavenge (reduce) 380.7 (391.3) -> 380.0 (391.6) MB, 1.79 / 0.00 ms (average mu = 0.210, current mu = 0.079) allocation failure;

[1044134:0x67be140] 49129 ms: Mark-Compact (reduce) 381.1 (391.6) -> 379.2 (391.6) MB, 697.59 / 0.05 ms (average mu = 0.262, current mu = 0.313) allocation failure; scavenge might not succeed

<--- JS stacktrace --->

FATAL ERROR: Ineffective mark-compacts near heap limit Allocation failed - JavaScript heap out of memory
```

The JavaScript bundle was relatively small at 445.79 kB/145.92 kB when gzip compressed. Luckily I was using Preact as a drop-in-replacement for React, which gzips down to 9.96 kB but my problems seemed to be with bundling Material UI. There were over 10,000 'module level directive' warnings associated with the `@mui` NPM package: 

```bash
$ npx vite build &> /tmp/build
$  cat /tmp/build | grep -c 'node_modules/@mui.*Module level directives cause errors when bundled'
10924
```

There has to be some way to reduce Node's memory footprint or work around these Material UI issues, but I wasn't able to do so and never liked the component library that much anyway. When an actual professional frontend engineer recommended I look into [React Aria](https://www.npmjs.com/package/react-aria-components) I had an excuse to replace the library entirely. It's a very simple frontend and I only ended up using the `Button`, `Modal`, and `Dialog` React Aria components. The most complicated part of the frontend is the autocomplete text input for submitting a US State name as a guess, and if I was using React then the `ComboBox` element would have worked great for this input. But I ended up using an `Autocomplete` element from the [Mantine](https://mantine.dev/) component library because of a possible bug in Preact.


## Possible Preact Bug

I set up [another repository](https://github.com/eoncarlyle/react-aira-issue-on-preact) with a side-by-side comparison between Preact and React to troubleshoot the bug. Both applications render only a `ComboBox` with a single `ListBoxItem` underneath it as a selectable option. When the Preact application is run, the following error appears in the developer console:

```text
Uncaught TypeError: Cannot set property previousSibling of #<Node> which has only a getter
    at $681cc3c98f569e39$export$b34a105447964f9f.appendChild (Document.ts:119:13)
    at Object.insertBefore (portals.js:49:22)
```

The relevant section of `Document.ts` is shown below, this is part of React Aria used to build a variety of different components.

```typescript
export class BaseNode<T> {
  //...
  appendChild(child: ElementNode<T>) {
    //...
    if (this.lastChild) {
      this.lastChild.nextSibling = child;
      child.index = this.lastChild.index + 1;
      child.previousSibling = this.lastChild;
    } else {
      child.previousSibling = null; // Line 119 of Document.ts: error thrown here
      child.index = 0;
    }
    //...
  }
  //...
}
```

The React application in that repository has no issue with rendering `ComboBox`. No Preact errors are thrown if `ListItemBox` elements are left out, which of course defeats the purpose of having the `ComboBox` in the first place. This means that the problem is with Preact and `ListBoxItem` in particular. With respect to the error itself, in the code segment above the `child.constructor.name` is equal to `"HTMLUnkownElement"` for Preact. The rest of this section is more speculative - I know very little about Preact internals. However, in comparing two stack frames up from `BaseNode#appendChild`, Preact is calling the method on a lower level of the virtual DOM hierarchy than React, and I don't think that this lower level exists.

The code segment below is taken two stack frames above `BaseNode#appendChild`. For Preact, `parentVNode.type` and `parentVNode._dom` have values of `"item"` and `undefined` respectively, and the `_dom` property of a Preact `VNode` is 'The \[first (for Fragments)\] DOM child of a VNode' [^vnode_dom]. While the function call looks a little different for React, `child.constructor` and `child.node.type` are `[[FunctionLocation]] Document.ts:227` and `"item"`.  While React is inserting an `"item"` virtual DOM element into the DOM, Preact looks to be inserting the first _child_ of an `"item"` virtual DOM element, which is `undefined`. 

Because the text inside of the `ListBoxItem` is the lowest level in the component hierarchy, I assume is that the `"item"` DOM element is the content between the `ListBoxItem` tags, which is 'Aardvark' in my bug demonstration repository.  Line 119 of `Document.ts` is only reached once in both the Preact and React applications, so this isn't a matter of React not yet reaching the lowest level of the component tree,  and the `ListBoxItem` definition is `const ListBoxItem = createLeafComponent('item', function (props, forwardedRef, item) {...})` which likely explains the `"item"` in both function calls. 


```javascript
// preact: children.js:343
function insert(parentVNode, oldDom, parentDom) {
    //...
    parentDom.insertBefore(parentVNode._dom, oldDom || null);
    //...
}

// react: react-dom-development.js:11069
function appendChildToContainer(container, child) {
    //...
    parentNode.insertBefore(child, container);
    //...
}
```

There's a good chance that this isn't a true bug and is rather a tradeoff in `Portal` components that the Preact maintainers had to make, but in the coming days I'll post an issue in the GitHub repository for the project. If it's not a bug then I'll be curious to see what I got wrong here.

## Frontend Re-write Impact

I would have preferred to use React Aria for the autocomplete over importing a second component library, but my VPS can build and deploy the frontend with GitHub actions without running out of memory. To my surprise, this frontend rewrite barely impacted the JavaScript bundle size. In comparing the repository before the F# rewrite at `ee865c8` with the current most recent commit `fa98105` as of writing, the bundle size shrank by less than 5% down to 423 kB. When building on an Apple Silicon M1 processor, the peak memory during the build also went down a modest amount from 47.56 MB to 45.04 MB. However, the build time went from 9.62 to 3.42 seconds, and the number of modules transformed from 12,473 to 2,716 [^perfmeasure].

Instead of rewriting the frontend to build on one of the cheapest servers offered by Digital Ocean, I could have instead moved the action runners to a homelab server and done any number of things to deploy it onto the VPS - something I plan to do anyway. But what's the fun in that?


[^vnode_dom]: https://github.com/preactjs/preact/blob/main/src/internal.d.ts#L150
[^perfmeasure]: Values taken using the MacOS `time` command, averaged over three measurements
