# Did AI write this?

**Short answer: much of the recent code, yes, under my direction, and none of it is taken on trust.** Here is the long answer.

## 8 + 5: eight years, then five months

go2cs is a personal project. One developer, nights and weekends, since the [first commit on May 21, 2018](RoadmapHistory.md#milestones). It started as a way to bring Go into my C# world, and the [Background](Background.md) page tells that story.

For most of those years this was an experiment:

* **2018 to 2022.** The first converter was written in C# and read Go through an ANTLR grammar. It got surprisingly far, and I tagged it as `v0.1.2` in March 2022. But a grammar only knows what Go code *looks like*, and a faithful conversion has to know what it *means*.
* **January 2025.** So, I started over, in Go, on the same `go/ast` and `go/types` packages Go's own tools use. That rewrite set the shape go2cs still has today: the converter, the `golib` runtime library, the behavioral tests and the Roslyn source generators.
* **May 2025.** The new converter first ran over the whole standard library, but the output did not compile yet.

Every line of that was written by hand, about 900 commits over seven years.

In June 2026 I began using chat-based AI assistance on this project for the sticky issues, such as particular AST and type-checking cases in Go and escape-analysis techniques. It was very useful, much quicker than an Internet search, but still very manual: still me, coding by hand. Around the same time I started exploring AI coding agents, which had gotten good enough to work on a codebase like this one. The first commit an agent co-authored here is dated June 25, 2026. Fifteen days later the entire standard library compiled as .NET assemblies. Less than five months after that, it is validated against Go's own tests, and real-world Go converts and runs. Agents have since co-authored about four in five of all the commits in the project's history. It's been a fast ride and I'll say this: without that help, getting to where the project is now would have taken me another eight years, if I was lucky.

## What the agents did, and what I did

The agents wrote most of the code from June 2026 on: converter fixes, runtime work, tests, and much of the documentation. They ran around the clock, as several sessions on several machines, each on its own piece of the work.

For this part of the ride, I steered. I decided what go2cs should be and how converted code should read, and I ruled on the trade-offs:

* behavior that matches Go comes before speed or size;
* the C# stays as visually close to the Go as it can;
* breaking changes are made now, before people depend on the code, not later.

I set the quality bar. Every release, and every claim these pages make, is mine to answer for. The goal was (and still is) a tool you can use and count on: honest about what it can and can't do, and something you can trust to behave like the original code.

The work also went slower in some places than an AI demo would suggest. Agents make mistakes, often confident ones. A good share of the effort went into better _process_, i.e., building the checks that catch those mistakes.

### The AI reality check

As an aside, I can't pass this point without telling you how strange this journey has been: being the captain of the ship, not the engineer wearing every hat.

I've identified as being a computer programmer for basically my entire career. Over that time, I have come to consider coding to be an art form, at least from the perspective of when something was architected well, with clean, supporting code -- it becomes a thing of beauty. I still feel that way. When you find code that reads well and has a logical structure, you discover its nuanced, inviting and appealing features as you explore it, not unlike standing in front of a painting. So, when I found these Claude agents could code as well as me (if not better now), matching my style and structure, there was a moment of existential crisis. What followed was a few of the common stages of grief, and most certainly the final one: _acceptance_.

One thing remains clear: I am still architecting, guiding and directing, many of the "hats" I wore before. I am just not the one _coding_. I am conducting the choir, as it were, just not the one singing (not exclusively, anyway). The acceptance is this: I am not giving up control, I am handing the more tedious and time-consuming parts of the development to capable agents, to reach the end goal _much faster_ and, importantly, without losing the beauty (and when it slips, I intervene). So, I step up a level, I take off one of my hats, and I focus on steering the ship.

The question that then remains is this: how do you trust this?

## Why you don't have to trust the AI, or me

The yardstick is Go itself. A converted package is called validated only when its real `_test.go` suite has been converted to C#, run, and compared verdict for verdict against `go test` on the same machine. Every such package has a proof page, Go's verdict beside go2cs's, test by test.

That bar stays the same whoever wrote the code: when a change breaks something, Go's own tests say so. That is how any defects agents introduced were caught, before any release.

What doesn't work yet is written down in the open, in [Limitations](Limitations.md) and [Known issues](KnownIssues.md).

## The receipts

This way of working has produced checkable results:

* [Validated test packages](ValidatedTestPackages.md): the Go standard library, package by package, validated against its own test suites on Windows, Linux (x64 and Arm64) and macOS (Intel and Apple silicon), each with its proof page.
* [Validated modules](ValidatedModules.md): third-party Go modules converted and validated the same way, published as NuGet packages through the [nugetgo.net](https://nugetgo.net) registry.
* [typescript-go-cs](https://github.com/ritchiecarroll/typescript-go-cs): Microsoft's TypeScript compiler, written in Go, converted to C#. It converts, compiles and type-checks.
* [sqlite-cs](https://github.com/ritchiecarroll/sqlite-cs): modernc.org/sqlite, a Go translation of SQLite's C code, converted to C#. A program that writes and reads a database file prints exactly what native Go prints, on Windows and on Linux.
* The [Tour of go2cs](https://github.com/ritchiecarroll/go2cs/blob/master/src/tour/README.md): the Tour of Go beside the generated C#, live.

## From toy to tool

Eight years made the idea and the foundation. Five months with Claude turned my pet project from a toy into a tool. I'm proud of both, and I wouldn't be where I am with this project without either.
