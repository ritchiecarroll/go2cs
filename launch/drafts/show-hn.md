# Draft: Show HN

> DRAFT for the owner's voice, for the wide launch, after the Go 1.26 release. Figures in [square
> brackets] are read from the roster on the day. Hacker News rewards a plain title and an author who
> stays to answer; it punishes superlatives.

## Title (80 characters at most)

Preferred:

    Show HN: go2cs – convert Go to C#, validated against Go's own test suite

Alternatives:

    Show HN: go2cs – Go's standard library, converted to C#, passing Go's tests
    Show HN: go2cs – use Go libraries from .NET as managed code

## URL

The repository, https://github.com/ritchiecarroll/go2cs. The text below goes in the first comment, not in
the submission, because a Show HN with a URL shows no text.

## First comment (the author's)

Hi HN, I'm the author. go2cs converts Go source into C# that reads like the Go and is meant to behave the
same: slices, maps, channels, goroutines, defer/panic/recover, struct embedding and interfaces all come
across, backed by a small runtime library and Roslyn source generators.

The part I think is interesting is how it is checked. Compiling is not correctness, so the converted Go
standard library is run against Go's own tests. Each package's `_test.go` suite is converted to C#,
executed, and compared with `go test`, verdict for verdict. [N of N] packages that can be validated pass
on Windows and Linux, including `runtime`, `reflect`, `net/http` and `crypto/tls`. Every difference is
disclosed by name with its reason, and each package has a proof page:
https://go2cs.net/ValidatedTestPackages.html

To try it without installing anything Go-related, three Go modules are on NuGet already:
`dotnet add package nugetgo.github.com.google.uuid`. To convert your own module:
https://go2cs.net/ConvertingAModule.html

What it does not do, so you do not have to find out:

- It is usually slower than Go, starts slower and uses more memory. Numbers:
  https://go2cs.net/Performance.html
- A goroutine is an OS thread. Thousands are fine; a million is not.
- cgo is refused, and hand-written assembly becomes a stub (pure-Go fallbacks convert).
- macOS is still being validated. Windows and Linux x64 are done.
- The output is readable but it is generated code, with a few unusual identifiers.

The converter is AGPL with an explicit exception for its output: code it generates is not covered by the
converter's license. The runtime is MIT, and the converted standard library is BSD like Go's.

[One or two sentences, in the owner's words, on how the project is built: decision D6.]

It is a personal project and I will be here for the next few hours. The limits are written up at
https://go2cs.net/Limitations.html, and I would much rather hear where it breaks than that it is neat.

## While the post is up

- Stay two to three hours. Answer every substantive comment once, briefly.
- Grant what is true in an objection before answering it.
- Link the page; do not paste it.
- Do not ask anyone to vote, and do not post it twice.
- The prepared answers are in [`replies.md`](replies.md).
