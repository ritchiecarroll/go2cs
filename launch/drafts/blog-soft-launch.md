# Draft: the soft-launch post for go2cs.net

> DRAFT for the owner's voice. It becomes a NEWS entry with its own page. Figures in [square brackets]
> are read from the roster on the day. First person, because it is one person's project.

**Title:** go2cs: use Go code from .NET, and check it against Go's own tests

---

go2cs converts Go source code into C#. The C# reads like the Go it came from and is meant to behave the
same. I have been building it for a long time, and it has reached the point where I would like a few
more people to try it and tell me where it breaks.

**The quickest way to see it** needs no converter at all. Three Go modules are already converted and
published on NuGet. In any C# project:

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

**To convert your own Go**, you need Go, the .NET SDK and the converter, as the
[README](https://go2cs.net/#requirements) describes. Then:

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
proof page for each one, and one command reproduces any row on your own machine.

## What it does not do

I would rather you hear these from me:

- **It tracks Go 1.24**, which Go no longer patches. The move to Go 1.26 is the next big piece of work.
- **It is usually slower than Go**, starts slower and uses more memory. The
  [measurements](https://go2cs.net/Performance.html) are published.
- **A goroutine is a real thread.** Thousands are fine. Hundreds of thousands are not.
- **cgo does not convert**, and hand-written assembly becomes a stub. Many libraries have a pure-Go
  fallback, and that is what converts.
- **macOS is not finished.** Windows and Linux on x64 are the validated platforms.
- **The converted code uses a few unusual letters**, such as `ж` for a pointer. There is a
  [table](https://go2cs.net/ConversionStrategies.html#reading-converted-code-names-and-glyphs).
- **You install it by cloning and building.** A simpler install is coming.

[Limitations](https://go2cs.net/Limitations.html) has the full list, and
[Known issues](https://go2cs.net/KnownIssues.html) has the bugs that would bite you.

## What I am asking for

An hour of your time, if the idea is useful to you. Try a package you know. Tell me the first thing that
confused you or failed: [open an issue](https://github.com/ritchiecarroll/go2cs/issues) or start a
[discussion](https://github.com/ritchiecarroll/go2cs/discussions). This is a personal project, so replies
are best-effort, and I read everything.

[One or two sentences, in the owner's words, on how the project is built: decision D6.]
