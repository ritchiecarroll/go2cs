# Draft: r/dotnet

> DRAFT for the owner's voice, for the wide stage (estimated Thu Nov 19, 09:00 US Eastern), on go.* 1.26
> and the nugetgo wave. Read the subreddit's self-promotion rules on the day. A .NET audience asks "what
> can I do with this today", so the post leads with a NuGet package and working code, and only then says
> what the project is. The r/csharp variant is retired (venue dropped); it is kept in
> [`retired/reddit-csharp.md`](retired/reddit-csharp.md).

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
slices, maps, pointers and channels, with the rough edges. Other converted modules, such as
`golang-jwt/jwt` and `spf13/cobra`, are listed at https://nugetgo.net [confirm the list on the day].

**Why trust converted code?** Because it is held to the original's tests. The Go module's own test suite
is converted, run in .NET, and compared with `go test`, test by test. The whole Go standard library, at
Go 1.26, is checked the same way: https://go2cs.net/ValidatedTestPackages.html

**What it costs:**

- It is slower than the Go original, and usually slower than hand-written C# would be. If you need Go's
  speed, a native library is still the right answer.
- The API looks like Go, not like .NET: Go's names, tuples for results, `error` values.
- It targets .NET 10. Windows and Linux x64 are validated; macOS is in progress.
- Native AOT works, but the publish is slow and the executable is large today.

The converter is on GitHub, with downloads for Windows, Linux and macOS, and you can convert your own Go
module: https://github.com/ritchiecarroll/go2cs.

[One or two sentences, in the owner's words, on how the project is built: decision D6.]

It is a personal project. I would like to know which Go library you would want on NuGet, and what
breaks when you try one.
