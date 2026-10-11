# Draft: the reply bank

> DRAFT for the owner's voice. One short answer for each objection that can be predicted. Each grants
> what is true, says one thing, and links the page with the detail. Edit before use; never paste the
> same text twice in one thread. **Acknowledge and link; never argue.**

## "Why not just use cgo, or build a native library?"

You can, and if you need Go's own speed that is the better choice. The cost is a native binary for every
platform, a C toolchain, two runtimes and two garbage collectors in one process, and marshalling at the
boundary. Converted code is plain managed code: one runtime, `dotnet build`, and you can step into it.
It is slower. That is the trade. https://go2cs.net/FAQ.html#why-not-call-go-through-a-native-library-instead

## "Isn't it slow?"

Slower than Go, yes: usually a small multiple, worse for string-heavy code, about even for maps. It also
starts slower and uses more memory. I publish the numbers, including the bad rows:
https://go2cs.net/Performance.html

## "It's on Go 1.24? That's out of support."

*(Soft launch.)* It is, and I say so on the front page. The converted library carries none of Go's
security fixes since. The move to Go 1.26 is the next piece of work:
https://go2cs.net/Roadmap.html#go-releases

*(Wide launch.)* It tracks Go 1.26 now, and follows Go one release at a time, proving every package
again at each move: https://go2cs.net/Roadmap.html#go-releases

## "AGPL? I can't use that."

The AGPL covers the converter only. It has a written exception for its output: code go2cs generates
stays under the license of the Go it came from. The runtime library is MIT, and the converted standard
library is BSD like Go's. Running the converter in your build creates no obligation.
https://github.com/ritchiecarroll/go2cs/blob/master/LICENSING.md

## "The generated code is ugly." / "What is `ж`?"

It is shaped like the Go on purpose, so a Go reader can follow it, and a few Go ideas with no C# spelling
got their own letters. `ж<T>` is a pointer. I know it is strange at first sight. The table is here:
https://go2cs.net/ConversionStrategies.html#reading-converted-code-names-and-glyphs
*(If ASCII spellings are planned by then, say so in one line.)*

## "Why would anyone want this?"

Most people will not. It is for a .NET team that needs a library that exists only in Go and does not
want a sidecar process or a native dependency. If that is not you, it is fair to shrug.
https://go2cs.net/Background.html#why-convert-go-to-c

## "Was this written by AI?"

[The owner's words: decision D6. A possible shape:] I use AI assistants heavily, and the commit history
says so. That is why the project does not ask to be trusted on authorship. Every package is held to Go's
own test suite, and the results are published per package, reproducible with one command:
https://go2cs.net/ValidatedTestPackages.html

## "Goroutines as threads? That defeats the point of Go."

For a server holding a million connections, yes, and I would not convert that. Each goroutine is an OS
thread, so the practical ceiling is in the thousands. For a library that uses a handful of goroutines it
behaves the same. I chose matching Go's behavior over rewriting everything into `async`.
https://go2cs.net/Limitations.html#what-converts-but-doesnt-behave-exactly-like-go

## "Is it production-ready?"

I would not make that claim for you. What I can say is what is measured: which packages pass Go's own
tests, on which platforms, and what is disclosed as different. Read the limits, and try it on your code
before you decide. https://go2cs.net/Limitations.html

## "One maintainer. What happens if you stop?"

A fair worry. Everything is open, the converted code is ordinary C# that you own, and the validation is
reproducible without me. Replies are best-effort because it is one person.

## "Does it handle generics / reflection / unsafe / my favorite feature?"

Generics, closures, embedding, `defer`/`panic`/`recover`, channels, `select` and `reflect`: yes. `unsafe`
works over plain numbers and stops at anything the garbage collector tracks. cgo: no. The list of what
does not make the trip is short and specific: https://go2cs.net/Limitations.html#what-wont-make-the-trip

## "Why not compile Go to WebAssembly and host that?"

That works, and it keeps Go's own runtime. What you get is a sandboxed module you call across a boundary.
What go2cs gives is C# types and methods you call directly and can debug. Different tools for different
jobs.

## "I hit a bug."

Thank you. Could you open an issue with the output of `go2cs -version`, your OS, the error, and the
smallest Go that shows it? https://github.com/ritchiecarroll/go2cs/issues
*(Then: label it within the hour, and reply in the thread with the issue link.)*

## "Does it work on macOS?"

Partly. The packages are there for Intel and Apple silicon and the walkthrough runs, but validation is
still in progress, package by package. Windows and Linux x64 are the finished platforms.
https://go2cs.net/Roadmap.html#platforms

## "Can it go the other way, C# to Go?"

No. C# would need so many restrictions that it would not be practical.
https://go2cs.net/FAQ.html#can-it-convert-c-to-go
