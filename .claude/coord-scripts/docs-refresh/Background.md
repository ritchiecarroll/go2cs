# Background

> "I've been programming in C# for many years, even so, I try to keep an eye on new tech being developed, regardless of language -- one common choice for new applications these days is Go. Often the easiest way for me to experiment with new technology is to integrate it with some of my existing C# code, hence the desire for this project -- besides, I figured I might learn something about building apps in Go in the process."

That's how this project [started](README.md#milestones): a personal itch to learn Go by turning it into something I already knew. The itch never quite went away, but the reasons for scratching it have gotten a lot better than "I was curious" -- and most of them are now things I can *show* you instead of argue about. 😄

It took two tries to get right. The first converter was written in C# and read Go through an ANTLR4 grammar. It got surprisingly far, but a grammar only knows what Go code *looks like*, and a faithful conversion needs to know what it *means*: every type, every method set, every interface a value satisfies. So I did what every developer dreads and started over. Today's converter is written in Go and reads code with the same packages Go's own tools use (`go/ast` and `go/types`). Turns out nobody understands Go quite like Go does.

## Why convert Go to C#?

**To use Go code from inside .NET.** Plenty of shops are all-in on .NET. When the library they need is written in Go, the usual options are a separate process, a native wrapper, or a hand rewrite kept in sync forever. go2cs adds a fourth: convert it and reference it like any other .NET assembly. The converted standard library ships on [NuGet](https://www.nuget.org/packages?q=go2cs%20ritchiecarroll), and one command ([`-recurse=nuget`](README.md#converting-a-real-world-module)) turns a whole Go app, dependencies and all, into a solution you can build and run. The README walks through one, and its output matches `go run`.

**To migrate with your eyes open.** Compiling is not the same as working, and I try never to blur the two. So the yardstick here is **Go's own tests**: a package's real `_test.go` suite is converted to C#, run, and compared verdict for verdict against `go test`. You can [reproduce that from a clone](README.md#try-it-yourself--validate-a-converted-test-suite) with one command. That turns migration from a leap of faith into a plan: move a package at a time, and keep the receipts. 🧾

**To give Go code another place to run.** Converted code is ordinary .NET, so it deploys like any other .NET app, and published with [Native AOT](https://learn.microsoft.com/dotnet/core/deploying/native-aot/) it becomes one self-contained executable, just like a Go binary. Converting is fast and cheap, so you can even keep writing Go and convert again: *code in Go, run in .NET*.

**To get C# you can read.** Receiver methods become extension methods, multiple returns become tuples, embedded structs become promoted fields, and the machinery behind Go's semantics stays out of sight. Judge for yourself with the README's [side-by-side table](README.md#real-standard-library-conversions-side-by-side), or the construct-by-construct map in [Conversion Strategies](ConversionStrategies.md). Readable code means a team can *maintain* it -- fix a bug in the C# on a Tuesday afternoon -- instead of owning machine output nobody can review. Conversion isn't a one-way door.

**To learn.** Because the output mirrors the Go, it doubles as a phrasebook between the two languages. The [Tour of go2cs](https://github.com/ritchiecarroll/go2cs/blob/master/src/tour/README.md) puts the Tour of Go on one side of the window and the generated C# on the other, live. That's the tool I wanted for myself when I started.

## Where it stands

**What works today:**

* The whole Go standard library converts and compiles as .NET assemblies.
* Every standard-library package that *can* be validated passes its own Go test suite in C#, on Windows and on Linux. 🎉 Each one has a [proof page](ValidatedTestPackages.md), Go's verdict beside go2cs's, test by test, and the few that can't be validated are [listed with their reasons](ValidatedTestPackages.md#excluded-packages).
* Underneath, hundreds of small Go programs check the language features one by one, comparing the C# output against Go's.
* Real third-party Go modules convert too, and are judged the same way: by their own test suites. [nugetgo.net](https://nugetgo.net) is the community registry for converted modules.

Of course, even roses have thorns 🌹. A tool like this is worth exactly as much as its worst *undisclosed* defect, so here are mine:

* **It's usually slower than Go.** Go wins most benchmarks, starts faster and travels lighter on memory. [More below](#how-fast-is-converted-code).
* **Some Go tests ask for the impossible, or the not-yet.** The impossible ones check things no managed runtime can promise, like the inner workings of Go's own runtime, or whether an object a test just stopped using can already be collected. The not-yet ones count allocations that Go's compiler avoids and go2cs doesn't *yet*, and each of those carries a plan to retire it. None are quietly skipped: each one is pinned by its exact failure signature in a [committed file](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/bytes/go2cs_test_disclosures.json), so any *other* failure of that test still counts.
* **Hand-written assembly doesn't make the trip.** .NET can't run Go's assembly code, so the converted library reproduces Go built with its portable fallbacks (`-tags purego`).
* **macOS is still catching up.** The library compiles there, and the work to run and validate it is under way, tracked on the [Roadmap](Roadmap.md#platforms).

Bugs that would bite you are written up in the open, in [Known issues](KnownIssues.md). Nothing here is graded on a curve.

## How fast is converted code?

**`TL;DR`**: Don't expect converted C# to run as fast as the original Go -- but don't expect it to crawl, either.

Most compute lands within a small multiple of Go's time. String-heavy work takes several times longer, and the one thing C# has no native answer for -- checking at run time whether a value satisfies an interface -- is the honest outlier. And it isn't always slower: maps and the optimized stack-string path run at parity with Go, or faster.

Why the gap? Go's strings, slices and interfaces don't map one-to-one onto .NET's, so a runtime library emulates them, and the converter aims for C# a Go programmer can read and follow, not hyper-tuned C#. Startup is the other cost: Go compiles straight to machine code, while .NET compiles as it runs ([JIT](https://en.wikipedia.org/wiki/Just-in-time_compilation)) unless you publish with Native AOT. You don't have to take my word for any of this: the [performance comparison](Performance.md) measures Go, C# on the JIT, and C# with Native AOT, methodology and all.

Need Go's own speed? You can [export Go functions](https://pkg.go.dev/cmd/cgo#hdr-C_references_to_Go) as C, build them into a native library, and [import them](https://learn.microsoft.com/dotnet/api/system.runtime.interopservices.dllimportattribute) in C#. go2cs is for when you want the code itself.

If you code in Go and ever need to build a C# app, I hope this gives you a head start: bring your existing code along, and keep using the standard library you already know.
