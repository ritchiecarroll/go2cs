# RETIRED draft: r/csharp

> **Retired 2026-10-20.** r/csharp was dropped from the launch (PLAN-launch.md section 5): a second .NET
> subreddit in the same week doubles the watching for an overlapping audience, and two self-promotional
> posts close together read as spam. This is the r/csharp variant as it stood in `reddit-dotnet.md` at
> `da487c7eea`, kept verbatim in case the venue comes back. It was written for the 1.24 releases; refresh
> it against `reddit-dotnet.md` before any use.

## r/csharp variant

**Title:** What Go's features look like when they are translated into C#

The r/csharp reader is more interested in the language than in the package. Same honesty, different
lead: open with the side-by-side example from the README (a Go struct and method beside its C#), then
three mappings that are worth a look:

- a Go method as a C# extension method, with value and pointer receivers as separate overloads;
- Go's structural interfaces implemented by a Roslyn source generator;
- `defer`, `panic` and `recover` on top of C# `try`/`finally`.

Link [Conversion Strategies](https://go2cs.net/ConversionStrategies.html) for the full mapping, then the
same "what it costs" list and the same closing question. Keep it shorter than the r/dotnet post.
