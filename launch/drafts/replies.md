# Draft: the reply bank

> DRAFT for the owner's voice, refreshed 2026-10-20 for the Go 1.26 release. One short answer for each
> objection that can be predicted. Each grants what is true, says one thing, and links the page with the
> detail. Edit before use; never paste the same text twice in one thread. **Acknowledge and link; never
> argue.**
>
> **How the watchers use this file.** The watcher agents ([`../AGENTS-venue-watch.md`](../AGENTS-venue-watch.md))
> and the issues agent ([`../AGENTS-issues-discussions.md`](../AGENTS-issues-discussions.md)) draft their
> suggested replies from these entries. Each entry's *Matches* line lists what it answers. An entry marked
> **Owner only** is never drafted as a ready reply: the agent flags the comment for the owner and quotes
> the entry as background.
>
> Before the wide stage, check every link and every figure against the live site at the 1.26 release.
> Figures in [square brackets] are read on the day.

## Why not just use Go?

*Matches:* why would anyone, just use Go, what is the point, rewrite it in C#.

If you can, do. Go is the better home for Go code, and go2cs gives a Go developer nothing Go does not
already do better. It is for the place where the two meet: a .NET application that needs a library that
exists only in Go, without a sidecar process or a native dependency. If that is not you, it is fair to
shrug. https://go2cs.net/FAQ.html#why-convert-go-to-c

## Why not cgo, or a native library?

*Matches:* cgo, c-shared, P/Invoke, native library, FFI, sidecar, gRPC.

You can, and if you need Go's own speed that is the better choice. The cost is a native binary for every
platform, a C toolchain, two runtimes and two garbage collectors in one process, and marshalling at the
boundary. Converted code is plain managed code: one runtime, `dotnet build`, and you can step into it in
the debugger. It is slower. That is the trade.
https://go2cs.net/FAQ.html#why-not-call-go-through-a-native-library-instead

## Why not compile Go to WebAssembly and host that?

*Matches:* wasm, WebAssembly, wazero, wasmtime.

That works, and it keeps Go's own runtime. What you get is a sandboxed module you call across a boundary.
What go2cs gives is C# types and methods you call directly and can debug. Different tools for different
jobs.

## Isn't it slow?

*Matches:* performance, slow, benchmark, overhead, memory, startup.

Slower than Go, yes: usually a small multiple, worse for string-heavy code, about even or better for
maps. It also starts slower and uses more memory, because every converted package it references comes
along. I publish the numbers, including the bad rows, and say which machine produced them:
https://go2cs.net/Performance.html
*(If asked about a specific row: grant it, and say whether it is on the Roadmap's performance list.
Quote no number that is not on the page on the day.)*

## Does it work with Native AOT? Is AOT faster?

*Matches:* Native AOT, AOT, trimming, ILC, single binary, startup time.

It works, and I would rather be precise about what it costs. A Native AOT publish starts several times
faster than the JIT, but it still runs every converted package's initialization, so it does not start as
fast as Go. Once running it is often slower than the JIT on tight loops, because the JIT's dynamic
optimizations are not there. The publish itself is slow and the executable is large, because the
converted packages it references are compiled in full. Printing and reflection behave as they do under
the JIT; that took real work, and the size went up for it on purpose. On Apple silicon the publish is not
measured yet.
https://go2cs.net/KnownIssues.html#a-native-aot-publish-of-a-program-that-references-the-packages-is-slow-and-large

## How do you know it behaves like Go? How are the tests validated?

*Matches:* tests, validated, correctness, how do you know, proof, test suite, disclosed.

Each standard-library package's own `_test.go` suite is converted to C#, built against the converted
library, run, and compared with `go test -json` from the same Go release, test by test. A package counts
only when every test either matches Go's verdict or is disclosed by name, with its exact failure and the
reason (a goroutine is a thread, an allocation count Go's escape analysis beats, a test that reads Go's
runtime internals). The counts are per platform, and one command reproduces any row on your own machine.
At Go 1.26 that is [N of N] packages on Windows and Linux; macOS is in progress.
https://go2cs.net/ValidatedTestPackages.html
What it does not prove is written down too:
https://go2cs.net/Limitations.html#what-validated-means-and-what-it-doesnt

## Which Go version? Why not Go 1.27?

*Matches:* Go 1.26, Go 1.27, out of date, behind, version, cadence, keep up.

It tracks Go 1.26 [the patch release in Requirements], which Go supports until Go 1.28 ships. Go 1.27 is
the next hop and starts now that 1.26 is out. The plan is to follow every Go release, one at a time,
re-validating every package at each move, and to take Go's patch releases as they come, the security
ones first. https://go2cs.net/Roadmap.html#go-releases

## Will it keep up? Isn't this a huge maintenance burden?

*Matches:* maintenance, burden, abandoned, bus factor, sustainable, how do you keep up.

It is real work, and I do not pretend otherwise. Each Go release is a planned migration with the same
steps every time: convert at the new release, review the hand-written parts against what changed
upstream, then re-run every package's tests until each one matches Go again. That process is written
down and repeatable, which is what makes it possible for one person. The converted code itself is
ordinary C# that you own, so a conversion you have keeps working whatever happens to the project.

## One maintainer. What happens if you stop?

*Matches:* one person, single maintainer, solo, what if you stop.

A fair worry. Everything is open, the converted code is ordinary C# that you own, and the validation is
reproducible without me. Replies are best-effort because it is one person.

## "It's AGPL? I can't use that." / "What license is the output?"

*Matches:* AGPL, GPL, license, copyleft, can I use it at work, output license.

The AGPL covers the converter only, and it has a written exception for its output: code go2cs generates
stays under the license of the Go it came from. The runtime library and the source generators are MIT,
and the converted standard library is BSD like Go's. Running the converter in your build creates no
obligation. https://github.com/ritchiecarroll/go2cs/blob/master/LICENSING.md

## "Wasn't this MIT before?" / "Why the CLA?" / "Is there a commercial license?"

*Matches:* relicense, used to be MIT, license change, CLA, contributor agreement, commercial license.

**Owner only.** Answer only if asked, in one or two sentences, and do not expand on it. The facts:

- Releases before September 2026 were MIT, and stay MIT for anyone who has them.
- Since then the converter alone is AGPL-3.0-only with the output exception; the runtime, the generators
  and everything they emit are MIT, and the converted standard library is BSD.
- A commercial license for the converter is available from the copyright holder, and the relicensing
  grant in CONTRIBUTING.md (converter contributions only) is what keeps that possible. Contributions to
  the MIT and BSD parts need only a sign-off.

Link LICENSING.md and CONTRIBUTING.md section 2; do not argue the choice.

## "The generated code is ugly." / "What is `ж`?"

*Matches:* ugly, unreadable, glyphs, ж, ᴷ, Unicode identifiers, weird names, naming conventions.

It is shaped like the Go on purpose, so a Go reader can follow it, and a few Go ideas with no C# spelling
got their own letters: `ж<T>` is a pointer, and `xᴷ` is the buffer that lets a slice grow the way Go's
does. I know they are strange at first sight. With Go 1.26 the rest reads closer to Go than before:
string literals where you wrote them, Go's comments and blank lines, shifts and casts as Go spells them.
The table is here: https://go2cs.net/ConversionStrategies.html#reading-converted-code-names-and-glyphs

## "Was this written by AI?"

*Matches:* AI, LLM, Claude, Copilot, vibe coded, generated by AI, co-authored.

**Owner only** (decision D6; the words are the owner's). A possible shape: I use AI assistants heavily,
and the commit history says so. That is why the project does not ask to be trusted on authorship. Every
package is held to Go's own test suite, and the results are published per package, reproducible with one
command: https://go2cs.net/ValidatedTestPackages.html

## "Goroutines as threads? That defeats the point of Go."

*Matches:* goroutines, threads, scheduler, M:N, million goroutines, async, runtime async, .NET 11.

For a server holding a million connections, yes, and I would not convert that. Each running goroutine owns
an OS thread (threads are reused once a goroutine ends), so a program with tens of thousands of goroutines
parked at once is where it stops being practical. For a library that uses a handful of goroutines it
behaves the same. I chose matching Go's behavior over rewriting everything into `async`.
https://go2cs.net/Limitations.html#what-converts-but-doesnt-behave-exactly-like-go

*If they want to go further (what would real M:N take?):* there is a design note that measures the gap
against Go and walks through what M:N would need on .NET, including a possible spike on .NET 11's opt-in
runtime async. It's a thought exercise for discussion, not a plan or a schedule.
[https://github.com/ritchiecarroll/go2cs/blob/master/docs/DESIGN-mn-scheduler-sizing.md: check the link is live
on master at the 1.26 release]

## "Is it production-ready?"

*Matches:* production, stable, ready, enterprise, would you use it.

I would not make that claim for you. What I can say is what is measured: which packages pass Go's own
tests, on which platforms, and what is disclosed as different. Read the limits, and try it on your code
before you decide. https://go2cs.net/Limitations.html

## "Does it handle generics / reflection / unsafe / my favorite feature?"

*Matches:* generics, reflection, reflect, unsafe, iterators, range over func, embedding, select.

Generics, closures, embedding, iterators, `defer`/`panic`/`recover`, channels, `select` and `reflect`:
yes. `unsafe` works over plain numbers and stops at anything the garbage collector tracks. cgo: no. The
list of what does not make the trip is short and specific:
https://go2cs.net/Limitations.html#what-wont-make-the-trip

## "Does it work on macOS?"

*Matches:* macOS, Mac, Apple silicon, M1, M2, Darwin.

Partly. The packages are there for Intel and Apple silicon, the converter has a macOS download, and HTTPS
works, but validation is still in progress, package by package. Windows and Linux x64 are the finished
platforms. https://go2cs.net/Roadmap.html#platforms

## "How do I install it?"

*Matches:* install, download, go install, dotnet tool, brew, winget.

Download the converter for your platform from the releases page, or clone and `go install` it. You also
need Go [the release in Requirements] and the .NET 10 SDK, because the converter uses Go's own toolchain
to read your code. There is no `dotnet tool` or package-manager install yet.
https://go2cs.net/#installing-the-converter

## "I hit a bug."

*Matches:* bug, crash, error, doesn't compile, failed, exception.

Thank you. Could you open an issue with the output of `go2cs -version`, your OS, the error, and the
smallest Go that shows it? The form asks for exactly that: https://github.com/ritchiecarroll/go2cs/issues/new/choose
*(Then: the issues agent labels it; reply in the thread with the issue link.)*

## "Can it go the other way, C# to Go?"

*Matches:* C# to Go, reverse, other direction.

No. C# would need so many restrictions that it would not be practical.
https://go2cs.net/FAQ.html#can-it-convert-c-to-go

## Always the owner's, never drafted as a reply

The watchers flag these for the owner and draft nothing beyond a one-line summary:

- a security report, or anything that reads like one (point the person to the private reporting route:
  https://github.com/ritchiecarroll/go2cs/security/policy, and say no more in public);
- legal claims, trademark or license accusations, takedown language;
- anything about the owner personally, or his work outside the project;
- hostility, harassment, or a pile-on;
- a question the reply bank does not cover and the docs do not answer plainly.
