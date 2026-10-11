# Draft: r/dotnet, with the r/csharp variant

> DRAFT for the owner's voice, for the wide launch. Read each subreddit's self-promotion rules on the
> day. A .NET audience asks "what can I do with this today", so the post leads with a NuGet package and
> working code, and only then says what the project is. Post the two a day apart, not the same text.

## r/dotnet

**Title:** Go libraries as NuGet packages: calling Google's Go `uuid` from C#, with no native code

---

There is a lot of good Go code that has no .NET equivalent, or none as well tested. The usual ways to
reach it from .NET are a separate process, or a native library behind P/Invoke.

I have been working on a third way: convert the Go source to C#. The result is an ordinary managed
assembly. This works today:

```shell
dotnet add package nugetgo.github.com.google.uuid
```

```csharp
uuid_package.UUID id = uuid_package.New();
string s = id.String();

var (parsed, err) = uuid_package.Parse("6ba7b810-9dad-11d1-80b4-00c04fd430c8");
if (err != nil) { /* Go errors are values, not exceptions */ }
```

That is Google's Go `uuid` module, converted by a tool called go2cs. A Go function with several results
returns a tuple, a Go `error` is a value you compare with `nil`, and a Go string converts to and from
`System.String`. [Consuming converted Go from C#](https://go2cs.net/ConsumingGoFromCSharp.html) covers
slices, maps, pointers and channels, with the rough edges.

**Why trust converted code?** Because it is held to the original's tests. The Go module's own test suite
is converted, run in .NET, and compared with `go test`, test by test. The whole Go standard library is
checked the same way: https://go2cs.net/ValidatedTestPackages.html

**What it costs:**

- It is slower than the Go original, and usually slower than hand-written C# would be. If you need Go's
  speed, a native library is still the right answer.
- The API looks like Go, not like .NET: Go's names, tuples for results, `error` values.
- It targets .NET 10, on Windows and Linux x64. macOS is in progress.
- Native AOT works but the publish is slow and large today.

The converter is on GitHub, and you can convert your own Go module:
https://github.com/ritchiecarroll/go2cs. The published modules are listed at https://nugetgo.net.

[One or two sentences, in the owner's words, on how the project is built: decision D6.]

It is a personal project. I would like to know which Go library you would want on NuGet, and what
breaks when you try one.

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
