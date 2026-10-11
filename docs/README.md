![go2cs](images/go2cs-small.png)

# go2cs — Go to C# Converter

[![golib NuGet package](https://img.shields.io/nuget/dt/go.lib?label=go.lib%20NuGet%20package)
](https://www.nuget.org/packages/go.lib)

Browse all: [Go Standard Library NuGet packages](https://www.nuget.org/packages?q=go2cs%20ritchiecarroll)

---

**go2cs converts Go source code into C# that reads like the Go it came from and behaves the same.**
A Go developer can read the converted code and follow it, and a .NET developer can use a Go library as
ordinary managed code. Go's slices, maps, channels, goroutines, `defer`, multiple returns, struct
embedding and interfaces come from a small runtime library and compile-time source generators, so that
machinery stays out of the converted files. Behavior comes first: a goroutine runs on its own thread
and is not rewritten into `async`. The converted Go standard library passes Go's own tests, package by
package, on Windows and Linux, and the same validation is in progress on macOS.

## 📰 NEWS — Converted code reads like Go, and Native AOT runs on Windows

**October 9, 2026, NuGet 1.24.13.5.** Converted C# drops the attributes that repeated Go's own syntax,
and a C# project that references the `go.*` packages runs under Native AOT on Windows; a Native AOT
publish is still slow and large
([Known issues](KnownIssues.md#a-native-aot-publish-of-a-program-that-references-the-packages-is-slow-and-large)).
[Full announcement](NEWS.md#october-9-2026--converted-code-reads-like-go-and-native-aot-runs-on-windows).

**➡ All announcements can be found in the [go2cs News Archive](NEWS.md).**

## Example

Given this Go:

```go
type Person struct {
    name string
    age  int32
}

func (p Person) IsAdult() bool {
    return p.age >= 18
}
```

go2cs produces this C#:

<!-- source: src/tests/Behavioral/StructPromotion/StructPromotion.cs.target:8-11 and :17-19 -->
```csharp
partial struct Person {
    internal @string name;
    internal int32 age;
}

public static bool IsAdult(this Person p) {
    return p.age >= 18;
}
```

## Real standard-library conversions, side by side

The goal — *reads like Go* — is easiest to judge on real code. Below are converted standard-library files
next to their original **Go 1.24.13** source, in order of increasing richness:

| Package | Go 1.24.13 source | Converted C# | What it shows |
|:--|:--|:--|:--|
| `errors` | [errors.go](https://github.com/golang/go/blob/go1.24.13/src/errors/errors.go) | [errors.cs](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/errors/errors.cs) | Error values and an unexported type satisfying the `error` interface. |
| `cmp` | [cmp.go](https://github.com/golang/go/blob/go1.24.13/src/cmp/cmp.go) | [cmp.cs](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/cmp/cmp.cs) | Generics with an ordered-type constraint. |
| `unicode/utf8` | [utf8.go](https://github.com/golang/go/blob/go1.24.13/src/unicode/utf8/utf8.go) | [utf8.cs](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/unicode/utf8/utf8.cs) | Constants keeping Go's hex/binary literal formatting; arrays and structs. |
| `sort` | [search.go](https://github.com/golang/go/blob/go1.24.13/src/sort/search.go) | [search.cs](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/sort/search.cs) | Binary search driven by a `func(int) bool` closure. |
| `strings` | [reader.go](https://github.com/golang/go/blob/go1.24.13/src/strings/reader.go) | [reader.cs](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/strings/reader.cs) | A struct with receiver methods, tuple returns, and interface implementation. |
| `container/list` | [list.go](https://github.com/golang/go/blob/go1.24.13/src/container/list/list.go) | [list.cs](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/container/list/list.cs) | A doubly-linked list — pointers and receiver methods. |

Browse the whole set under [`src/core`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core).

## Explore

* Go and C# side by side: [Tour of go2cs](https://github.com/ritchiecarroll/go2cs/blob/master/src/tour/README.md)
* Real converted code: [the Go standard library](https://github.com/ritchiecarroll/go2cs/tree/master/src/core)
* How each Go construct maps to C#: [Conversion Strategies](ConversionStrategies.md)
* What the converter does: [Features](Features.md)
* Call converted Go from C#: [Consuming converted Go](ConsumingGoFromCSharp.md)
* Go modules on NuGet: [nugetgo.net](https://nugetgo.net)
* Packages passing Go's own tests: [Validated Test Packages](ValidatedTestPackages.md)
* Before you start: [Limitations](Limitations.md) and [Known issues](KnownIssues.md)
* Where it stands: [Status](#status) and the [Roadmap](Roadmap.md)
* Build it yourself: [Building go2cs from source](BuildingGo2cs.md)

[![Tour of go2cs showing Go and generated C# side by side](images/tour-of-go2cs.png)](images/tour-of-go2cs.png)

## Requirements

- **[.NET 10.0 SDK](https://dotnet.microsoft.com/download)**, to build and run the converted C#.
- **[Go 1.24.13](https://go.dev/dl/)**, exactly that release. The converter is a Go program and uses
  the Go toolchain to load and type-check the source it converts, so that source must already build
  with `go build`. Go 1.24 is out of upstream support, so the converted standard library carries none
  of the Go security fixes issued since; the [Roadmap](Roadmap.md#go-releases) says which Go releases
  come next.
- **Windows or Linux on x64**, where the converted standard library is validated against Go's own
  tests. macOS, on Intel and Apple silicon, has packages and is being validated package by package
  ([Platforms](Roadmap.md#platforms)).

## Installing the converter

Clone the repository and install the converter from the clone. `go install` puts the `go2cs` binary in
`GOBIN` (or `GOPATH/bin`), which a standard Go setup already has on its `PATH`:

```shell
git clone https://github.com/ritchiecarroll/go2cs.git
cd go2cs/src/go2cs
go install .
```

[Building go2cs from source](BuildingGo2cs.md) has the rest: cross-compiling, running the tests and the
layout of the repository.

## Usage

```shell
go2cs [options] <input_dir> [output_dir]
```

```shell
go2cs -recurse=nuget module_dir out      # a module and its dependencies; the standard library comes from NuGet
go2cs -recurse=module module_dir out     # only the module's own packages
go2cs package_dir out                    # one package
go2cs example.go out/example.cs          # one file
go2cs -platforms linux/amd64 -recurse=nuget module_dir out-linux    # convert for another operating system
go2cs -tests -recurse -test-action all module_dir out               # convert a module, run its tests, compare with go test
go2cs -version                           # which release this converter belongs to
```

Give the output directory when you convert a single file or package: without it, the C# is written
beside the Go input.

| Option | What it does |
|:--|:--|
| `-recurse=nuget` | Convert a module and its third-party dependencies, in dependency order. The standard library, the runtime and the source generators are NuGet package references, so nothing is staged on your machine. Start here. |
| `-recurse=module` | Convert only the module's own packages. Use it when a dependency does not convert yet. |
| `-tests` | Also convert the Go test suite. With `-test-action all`, build it, run it, and compare its results with `go test`. |
| `-platforms <os/arch>` | The platform to convert for. It defaults to the machine you are on. |
| `-tags <list>` | The build tags to select files with, as in `go build -tags`. |
| `-comments` | Carry the Go comments into the C#. |
| `-version` | Print the release this converter belongs to, and convert nothing. |

[Command-line options](CommandLine.md) lists every option and more examples.

## Converting a real-world module

Four steps take a Go program with third-party dependencies to a running .NET program:

1. **Start from Go that builds.** Set `GOTOOLCHAIN=local`, so Go uses the release you installed, and
   run `go build ./...` in your module. A dependency that needs a newer Go has to be pinned to an older
   release first.
2. **Convert it.** In the module's directory, `go2cs -recurse=nuget . csharp` writes the C# for your
   packages and for every dependency under `csharp/`, and leaves the Go untouched.
3. **Build it.** Each converted project has a solution beside it. In `csharp/src/<import-path>`, run
   `dotnet build` on the `.slnx` file there, or open it in Visual Studio.
4. **Run it.** `dotnet run`, from the same folder.

**[Converting a real-world Go module](ConvertingAModule.md)** does all four with a real program and
shows the output. It also covers build tags, converting your own packages before their dependencies,
building against a local copy of the standard library, and using modules that are already published as
NuGet packages. Wondering which Go modules make good conversions? The
[go2cs Target Atlas](https://go2cs.net/TargetAtlas.html) ranks the most-depended-on ones.

## Frequently asked questions

* **Why convert Go to C#?** [To use Go code inside .NET](FAQ.md#why-convert-go-to-c).
* **Is it slower than Go?** [Usually, and sometimes not](FAQ.md#is-converted-code-slower-than-go).
* **Does it behave like Go?** [Measured against Go's own tests](FAQ.md#does-converted-code-really-behave-like-go).
* **What does not convert?** [cgo, assembly, newer Go](FAQ.md#what-does-not-convert).
* **What does the C# look like?** [Like the Go](FAQ.md#what-does-the-generated-c-look-like). [See examples](#real-standard-library-conversions-side-by-side).
* **How do I call it from C#?** [Like any other library](FAQ.md#how-do-i-call-a-converted-go-package-from-c).
* **Why not a native library?** [You can; here is the trade](FAQ.md#why-not-call-go-through-a-native-library-instead).
* **Third-party dependencies?** [Yes, not all convert cleanly yet](FAQ.md#can-it-convert-a-module-with-third-party-dependencies).
* **Does it work with Native AOT?** [Yes, at a cost in time and size](FAQ.md#does-it-work-with-native-aot).
* **What license covers the output?** [Not the converter's](FAQ.md#what-license-covers-the-code-go2cs-generates).

[More questions](FAQ.md): versions, platforms, dependencies, C# to Go.

![GopherDotNetBotFrisbee](images/GopherDotNetBotFrisbee.png)

## Status

The whole Go standard library converts and compiles as .NET assemblies, and every package that can be
validated passes its own Go test suite in C#, on Windows and Linux.
[Validated Test Packages](ValidatedTestPackages.md) carries the current counts, package by package, and
the [Roadmap](Roadmap.md) says where the project stands and what comes next.

### Try it yourself — validate a converted test suite

You do not have to take the roster's word for it. One command converts a package's Go tests, runs them
in C#, runs `go test`, and compares the two:
[Validate a converted test suite](BuildingGo2cs.md#validate-a-converted-test-suite).

## Contributing

Start with [`CONTRIBUTING.md`](https://github.com/ritchiecarroll/go2cs/blob/master/CONTRIBUTING.md), then
[Building go2cs from source](BuildingGo2cs.md) and [`Architecture.md`](Architecture.md). There is plenty
of low-hanging fruit here; jump in if you'd like to help.

## License

go2cs contains components under several licenses. The converter (`src/go2cs`) is
licensed under **AGPL-3.0-only** with the [go2cs Converter Output Exception](https://github.com/ritchiecarroll/go2cs/blob/master/src/go2cs/LICENSE-EXCEPTION),
an additional permission under AGPL section 7. Alternative commercial licensing for
the converter is available from the copyright holder listed in
[AUTHORS](https://github.com/ritchiecarroll/go2cs/blob/master/AUTHORS), at the contact
address given there. Original runtime/support libraries and Roslyn
generators use **MIT**. Standard-library packages, including project-owned
handwritten additions, use **BSD-3-Clause**, with upstream notices preserved.

Code generated by go2cs is not subject to the converter's AGPL license merely because
it was produced by go2cs, and the output exception says so as a binding term: rights
in output remain governed by the input source and any separately licensed components
incorporated into or referenced by the output, and the project scaffolding the
converter emits into your output is MIT. Running the converter, locally or in your
own build, triggers no AGPL obligation; only distributing the converter, or offering
a modified converter to others over a network, does.

**Commercial licensing, sponsorship and acquisition.** go2cs is developed by a single
maintainer and will stay free for its users. A company that wants to ship the
converter inside a proprietary product, offer it as a service without publishing its
changes, or take on the project's long-term development is welcome to talk: a
commercial license, sponsorship of ongoing work, or an outright
acquisition that keeps the tool free for the community are all on the table. The
contact address is in [AUTHORS](https://github.com/ritchiecarroll/go2cs/blob/master/AUTHORS).
Contributions to the converter carry a Developer Certificate of Origin sign-off and a
relicensing grant, see [CONTRIBUTING.md](https://github.com/ritchiecarroll/go2cs/blob/master/CONTRIBUTING.md),
which is what keeps those options open. The go2cs name and logo identify this
project; a fork is welcome under the AGPL but should carry its own name.

See [LICENSING.md](https://github.com/ritchiecarroll/go2cs/blob/master/LICENSING.md) for the component matrix, exceptions and output
policy, and [NOTICE](https://github.com/ritchiecarroll/go2cs/blob/master/NOTICE) for attribution.
