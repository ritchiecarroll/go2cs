# Draft: the soft-stage post for go2cs.net

> DRAFT for the owner's voice. It becomes a NEWS entry with its own page, published the morning of the
> soft stage, **after** go.* 1.26 and the nugetgo wave are on NuGet. Figures in [square brackets] are
> read on the day: the roster for counts, the README's Requirements for the exact Go release. First
> person, because it is one person's project. Before posting, check every "new in 1.26" line against the
> 1.26 NEWS entry and every link against the live site.

**Title:** go2cs: use Go code from .NET, checked against Go's own tests

---

go2cs converts Go source code into C#. The C# reads like the Go it came from and is meant to behave the
same. I have been building it for a long time, and with the move to Go 1.26 it has reached the point
where I would like more people to try it and tell me where it breaks.

**The quickest way to see it** needs no converter at all. Go modules are already converted and published
on NuGet. In any C# project:

```shell
dotnet add package nugetgo.github.com.google.uuid
```

```csharp
uuid_package.UUID id = uuid_package.New();
string s = id.String();

var (parsed, err) = uuid_package.Parse("6ba7b810-9dad-11d1-80b4-00c04fd430c8");
// err == nil
```

That is Google's Go `uuid` package, running as managed code. There is no native library and no second
process. [nugetgo.net](https://nugetgo.net) lists the published modules, and
[Consuming converted Go from C#](https://go2cs.net/ConsumingGoFromCSharp.html) shows what Go's strings,
slices, errors and multiple results look like from the C# side.

**To convert your own Go**, you need Go [the release in Requirements], the .NET 10 SDK and the
converter. The converter is a download for Windows, Linux and macOS on the
[releases page](https://github.com/ritchiecarroll/go2cs/releases), or you build it from a clone, as the
[README](https://go2cs.net/#installing-the-converter) describes. Then:

```shell
go2cs -recurse=nuget . csharp
```

converts your module and its dependencies into a solution that builds with `dotnet build`. The
[walkthrough](https://go2cs.net/ConvertingAModule.html) does it start to finish with a real program.

## How I know it works

I do not ask you to take my word for it. The converted Go standard library is checked against **Go's own
tests**. Each package's test suite is converted to C#, run, and compared with `go test`, verdict for
verdict. Today [N of N] packages that can be validated pass on Windows and Linux, and every difference is
written down by name with its reason. [The roster](https://go2cs.net/ValidatedTestPackages.html) has a
proof page for each one, and one command reproduces any row on your own machine. Converted third-party
modules are held to their own tests the same way before they are published.

## What is new with Go 1.26

- **The converted standard library is on Go 1.26**, a release Go still supports. Every package was
  validated again at the new release.
- **Your converted code reads more like your Go.** A string literal is a plain C# string literal where you
  wrote it, shifts and casts are spelled the way Go spells them, and comments and blank lines land where
  Go put them.
- **Slices grow the way Go's do**, so `cap()` gives the numbers Go gives.
- **Converted tests print Go's own output**: `=== RUN`, `--- PASS`, and the file and line of each log
  call.

The [release notes](https://go2cs.net/NEWS.html) have the full list.

## What it does not do

I would rather you hear these from me:

- **It is usually slower than Go**, starts slower and uses more memory. The
  [measurements](https://go2cs.net/Performance.html) are published, bad rows included.
- **A goroutine is a real thread.** Thousands are fine. Hundreds of thousands are not.
- **cgo does not convert**, and hand-written assembly becomes a stub. Many libraries have a pure-Go
  fallback, and that is what converts.
- **macOS is not finished.** Windows and Linux on x64 are the validated platforms; macOS validation is
  in progress, package by package.
- **The converted code uses a few unusual letters**, such as `ж` for a pointer. There is a
  [table](https://go2cs.net/ConversionStrategies.html#reading-converted-code-names-and-glyphs).
- **Native AOT works, and costs you**: the publish is slow and the executable is large.

[Limitations](https://go2cs.net/Limitations.html) has the full list, and
[Known issues](https://go2cs.net/KnownIssues.html) has the bugs that would bite you.

## What I am asking for

An hour of your time, if the idea is useful to you. Try a package you know. Tell me the first thing that
confused you or failed: [open an issue](https://github.com/ritchiecarroll/go2cs/issues) or start a
[discussion](https://github.com/ritchiecarroll/go2cs/discussions). This is a personal project, so replies
are best-effort, and I read everything.

[One or two sentences, in the owner's words, on how the project is built: decision D6.]
