![go2cs](images/go2cs-small.png)

# go2cs — Go to C# Converter

[![golib NuGet package](https://img.shields.io/nuget/dt/go.lib?label=go.lib%20NuGet%20package)
](https://www.nuget.org/packages/go.lib)

Browse all: [Go Standard Library NuGet packages](https://www.nuget.org/packages?q=go2cs%20ritchiecarroll)

---

## 📰 NEWS — Every implementable standard-library package validates

**All 225 implementable packages of Go 1.24.13's standard library pass their own test suites in
C#**: 225 of the 230 testable (97.8%), at 69,777 matching verdicts against `go test -json`, with 373
divergences disclosed by exact failure signature. They include `runtime` itself, `reflect`,
`net/http` and `crypto/tls`, and on Linux 223 of the 223 applicable rows validate. Each row of the
[validated roster](ValidatedTestPackages.md) links a proof page that lists Go's verdict beside
go2cs's, test by test, and the five packages outside the 225 are
[listed with their reasons](ValidatedTestPackages.md#excluded-packages). The converted library
ships as **NuGet 1.24.13.3**, targeting .NET 10. The
[full announcement](NEWS.md#october-1-2026--every-implementable-standard-library-package-validates)
has the details.

**➡ All announcements can be found in the [go2cs News Archive](NEWS.md).**

## go2cs Purpose

Convert source code written in the [Go programming language](https://golang.org/ref/spec) into
[C#](https://learn.microsoft.com/dotnet/csharp/). The generated C# is designed to be both *behaviorally*
and *visually* similar to the original Go — so a Go developer can read the converted code and follow it
easily, and a .NET developer can use Go code directly within the .NET ecosystem.

* Browse transpiled code: [Converted Go Standard Library](https://github.com/ritchiecarroll/go2cs/tree/master/src/core)
* Explore Go and generated C# side by side: [Tour of go2cs](https://github.com/ritchiecarroll/go2cs/blob/master/src/tour/README.md)
* Learn how it works: [Go to C# Conversion Strategies](ConversionStrategies.md)
* Walk through an example: [Converting a real-world module](#converting-a-real-world-module)
* Compile in Visual Studio: [Go Standard Library Solution](https://github.com/ritchiecarroll/go2cs/blob/master/src/go2cs-stdlib.slnx)
* Run converted Go test validation: [Try it yourself](#try-it-yourself--validate-a-converted-test-suite)
* Track which stdlib test suites pass in C#: [Validated Test Packages](ValidatedTestPackages.md)
* Find converted Go modules as NuGet packages: [nugetgo.net](https://nugetgo.net)
* Call converted Go from your C# code: [Consuming converted Go from C#](ConsumingGoFromCSharp.md)
* View example converted test: [`utf8_test.cs`](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/unicode/utf8/utf8_test.cs)
* See current project [status](#status), [milestones](#milestones), and [known issues](KnownIssues.md)

[![Tour of go2cs showing Go and generated C# side by side](images/tour-of-go2cs.png)](images/tour-of-go2cs.png)

### Frequently asked questions

* Why is a Go to C# transpiler needed? _[Integration opportunities](Background.md#background)._
* Won't converted C# code be slower? _[Usually — but not always](#performance)._
* OK, I want to try this — where will it fail or fight me? _[The honest limitations](Limitations.md)._

## Transpiler Goals

Go leans on its compiler and runtime for slices, maps, channels, goroutines, `defer`/`panic`/`recover`,
multiple returns, struct embedding and interface duck-typing. go2cs maps each onto idiomatic C#, keeping
the machinery out of sight — in a small runtime library and compile-time source generators — so the
converted code stays close to the original Go.

- **Reads like Go.** Receiver methods become extension methods, multiple returns become tuples, struct
  embedding becomes promoted fields — the shape of the code is preserved.
- **Runs like Go.** Conversions prioritize behavioral equivalence first (e.g. a `goroutine` runs on its
  own thread rather than being rewritten into `async`).
- **Managed first.** Output targets portable managed C#; native interop is a last resort, not the default.

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

### Real standard-library conversions, side by side

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

## Features

go2cs converts the full Go language surface except a generic type alias whose target is an anonymous struct or interface type (`type A[T any] = struct{…}`), which it reports and does not yet convert — the same converter that emits the packages above:

**Types & values**

- Slices and arrays (backing-array aliasing preserved, as in Go), maps, and UTF-8-backed `@string`
- `int` / `uint` as platform-width native integers; named numeric types and untyped-constant semantics
- Constants and `iota`, preserving Go's numeric literal formatting (hex, binary, underscores, exponents)
- Pointers with automatic heap-boxing driven by escape analysis; `nil`; `unsafe.Pointer`
- Type definitions and aliases — including exported aliases that resolve across assembly boundaries, and Go 1.24 generic aliases, which render as their target type (except one whose target is an anonymous struct or interface)

**Functions & methods**

- Multiple return values and named results, as tuples
- Receiver methods as extension methods, with distinct pointer- and value-receiver overloads
- Function values, closures (honoring Go's shared-storage capture semantics), and variadic functions
- `defer` / `panic` / `recover`, including named results observed and mutated by deferred closures

**Concurrency**

- Goroutines, each on its own thread (behavioral equivalence first — not rewritten into `async`)
- Channels with channel-operator (`<-`) lowering, and `select`-statement lowering

**Composition & polymorphism**

- Struct embedding with promoted fields and methods (multi-hop and cross-package)
- Interfaces, satisfied structurally in Go and realized as nominal C# glue via Roslyn source generators
- Type assertions and type switches
- Generics: type parameters and constraints (unions, `comparable`, `~`-underlying, method sets)

**Control flow & packaging**

- `for` / `range`, labeled `break` / `continue`, expression and type switches, Go 1.22 loop variables
- The built-ins: `append`, `len`, `cap`, `make`, `new`, `copy`, `delete`, `close`, …
- Packages mapped to namespaces; cross-package imports compiled as separate, referenced assemblies
- Build-tag and `GOOS` / `GOARCH` platform file selection, and deterministic, byte-stable output

See [`ConversionStrategies.md`](ConversionStrategies.md) for an example-driven tour of how each construct
maps to C# (with [`ConversionStrategies-Reference/`](ConversionStrategies-Reference/README.md) for the full detail).

![GopherDotNetBotFrisbee](images/GopherDotNetBotFrisbee.png)

## Requirements

- **[.NET 10.0 SDK](https://dotnet.microsoft.com/download)** — to build and run the converted C#. Converted
  projects target `net10.0`, the framework named by
  [`src/Directory.Build.props`](https://github.com/ritchiecarroll/go2cs/blob/master/src/Directory.Build.props).
- **[Go 1.24.13](https://go.dev/dl/)** — the converter is a Go program, and it uses the Go toolchain to load
  and type-check the source being converted. Make sure your Go environment is set up (`GOROOT`/`GOPATH`)
  and the source you want to convert already builds with `go build`.

## Installing the converter

Build the `go2cs` executable from source and place it on your `PATH`. The simplest way is `go install`,
which compiles it and drops the binary into `%GOBIN%` (or `%GOPATH%\bin`) — already on your `PATH` in a
standard Go setup — in one step:

```shell
cd src/go2cs
go install .
```

Go produces a self-contained native binary. To target another platform, use Go's standard cross-compilation
(`GOOS`/`GOARCH`).

## Usage

```shell
go2cs [options] <input_dir> [output_dir]
```

Examples:

```shell
go2cs example.go                       # convert a single file
go2cs package_dir                      # convert a package
go2cs -indent 2 -var=false example.go conv/example.cs
go2cs -stdlib                          # convert the entire Go standard library
go2cs -stdlib fmt strings io           # convert specific standard library packages
go2cs -recurse=nuget module_dir out    # convert a module + its third-party deps, stdlib from NuGet
go2cs -recurse module_dir              # same, referencing a locally-staged standard library
go2cs -recurse module_dir output_root  # ...with the generated app/dep trees under output_root
go2cs -recurse=module module_dir out   # convert only the module's own packages (deps referenced, not converted)
go2cs -tests package_dir               # convert a package plus its Go test suite
go2cs -tests -test-action all goroot_pkg_dir converted_pkg_dir   # ...and build, run, and diff vs go test
go2cs -tests -recurse -test-action all module_dir out_root      # validate a whole module against its own tests
```

### Common options

| Option | Description |
|:--|:--|
| `-stdlib` | Convert the Go standard library (optionally followed by specific package names). |
| `-recurse` | Convert a downloaded module **and its third-party dependencies** in dependency order, referencing (not reconverting) the pre-converted standard library. An optional second positional sets the output root for the generated `src\` (app) and `pkg\` (dependency) trees. A package that fails to convert is reported and skipped. The module is converted, and validated, as Go builds it with the `safe` build tag (see [Build tags](#build-tags-for-a-module)). See [Converting a real-world module](#converting-a-real-world-module). |
| `-recurse=module` | Same, but convert only the module's own packages: third-party packages are referenced into `pkg\` but not converted, and are listed at the end of the run. See [converting the module only](#optional-convert-the-module-only-and-deal-with-its-dependencies-later). |
| `-recurse=nuget` | Same, but the standard library, the `golib` runtime and the analyzer come from NuGet ([`go.<pkg>`](https://www.nuget.org/packages?q=go2cs%20ritchiecarroll), [`go.lib`](https://www.nuget.org/packages/go.lib), [`go.gen`](https://www.nuget.org/packages/go.gen)), so nothing is staged locally. Third-party modules that the [nugetgo.net](https://nugetgo.net) registry maps are referenced as published packages when a qualifying version exists, and converted locally otherwise (see [Mapping modules to NuGet packages](#mapping-modules-to-nuget-packages)). Scope and reference style combine: `-recurse=module,nuget`. The published packages match the Go release go2cs is built with; go2cs checks the module against that release first, and refuses a mismatch, naming both versions. |
| `-module-safe-tag=false` | With `-recurse`: convert and validate the module without the `safe` build tag, using the files a plain `go build` selects. On by default; see [Build tags](#build-tags-for-a-module). |
| `-nuget-map <source>` | With `-recurse=nuget`: add a mapping source, a local file or an `https://` URL. Repeatable; the first source that names a module wins, and nugetgo.net answers the rest. `-nuget-map off` turns mapping off. See [Mapping modules to NuGet packages](#mapping-modules-to-nuget-packages). |
| `-nuget-map-only` | With `-nuget-map`: use **only** the listed sources — the nugetgo.net fallback is dropped, so a module your sources do not name stays local, and nugetgo.net is never fetched. |
| `-nuget-map-exclude <module-path>` | With `-recurse=nuget`: never map this module, whatever a source says. Repeatable. |
| `-nuget-map-refresh` | With `-recurse=nuget`: fetch every URL source unconditionally (bypassing the local cache) and re-resolve, adopting what the sources say now instead of what `go2cs.nuget.lock` pinned. |
| `-nuget-map-canonical-only` | With `-recurse=nuget`: apply only **canonical** mappings (the module owner's own conversion); a community mapping is treated as unmapped. |
| `-nuget-map-feed <feed>` | With `-recurse=nuget`: read mapped packages' versions from this feed **instead of** nuget.org: an `https://` NuGet v3 service index (a rehearsal gallery such as int.nugettest.org) or a local folder of `.nupkg` files. A rehearsal version (`nugetgo-pack.ps1 -RehearsalSuffix`) is admitted only from it. The restore reads your own package sources, so add the feed there too. |
| `-tests` | Also convert the package's `_test.go` suite and emit a runnable C# test-host project (default off). Forces `-comments` on and works from a bare clone with no flags or environment setup. With plain `-recurse` it validates a whole module against its own tests: `go2cs -tests -recurse module_dir out_root`, with the output root outside the module's source tree; a module that needs a newer Go than the converted standard library is refused. See [Try it yourself](#try-it-yourself--validate-a-converted-test-suite). |
| `-test-action <action>` | With `-tests`: one of `convert` (default), `build`, `run`, `compare`, or `all`. `convert` and `all` convert the package and its tests; `build` / `run` / `compare` act on the **existing** converted artifacts — validated against the test manifest's recorded input digest — without reconverting. `compare` (and `all`) runs both `go test -json -count=1` and the converted C# test host and diffs the terminal results by test name. |
| `-test-timeout <duration>` | Package deadline for a converted-test action, in Go duration syntax (default `2m`); `run` and `compare` give it to both `go test` and the converted host. The host's `dotnet publish` always gets at least `30m`, because the first publish on a fresh tree builds the whole standard-library closure. A suite that runs long in C# needs a larger value: `hash/maphash` is validated with `-test-timeout 30m`. |
| `-module-disclosures <dir>` | With `-tests` on a third-party module: the committed tree of module rows' disclosure manifests, laid out like the module cache (`<dir>/<module path>@<version>/<package dir>/go2cs_test_disclosures.json`; the repository keeps it at `src/tests/ModuleDisclosures`). A manifest there is read only when the package's output directory holds none, and only for the module version this run resolved; a manifest kept for another version is not applied, and the run prints one line naming both. |
| `-go2cspath <dir>` | Runtime/stdlib root (env `GO2CSPATH`; default `~/go2cs`) used by generated `$(go2csPath)…` references, and the output root for `-stdlib`. For a single-package/file conversion, C# output goes to optional `[output_dir]` (in place by default). |
| `-goroot` / `-gopath` | Override the detected Go root / path. |
| `-platforms <os/arch>` | Target platform for build-tagged files (defaults to the host). A comma-separated **list** (`windows/amd64,linux/amd64,darwin/amd64`) is accepted and today requires `-platform-census`: a conversion still emits for exactly one target, so a list without the census flag is rejected rather than silently converting the first. |
| `-platform-census <dir>` | With `-stdlib` and two or more `-platforms` targets: convert once per target into an isolated, seeded staging root under `<dir>`, compare what each run actually emitted, and write `<dir>\platform-manifest.json` classifying every artifact as shared, variant, partial or platform-exclusive. Produces **no** converted output of its own — `-go2cspath` is read as the seed and never written to. |
| `-tags <list>` | Build tags applied when loading packages. `-stdlib` and `-tests` apply `purego` by default, and `-recurse` adds `safe`. An explicit `-tags` replaces every default. |
| `-indent <n>` | Spaces per indent level (default 4). |
| `-var` | Prefer `var` declarations where the type is obvious (default on). |
| `-uco` | Emit channel operators instead of method calls (default on). |
| `-comments` | Carry source comments into the output (best effort, see [go/ast comment status](https://github.com/golang/go/issues/20744)). |
| `-csproj <file>` | Generate project files from a custom `.csproj` template instead of the embedded one. |
| `-tree` | Print each file's Go parse tree (`go/ast`) to stdout during conversion — a diagnostic aid. |
| `-debug` | Disable the converter's per-file panic recovery, so a conversion failure crashes with a full stack trace instead of being reported as a warning. |
| `-version` | Print the converter's release tuple and exit 0, converting nothing. One `key=value` line per key, always in this order, every key present (`unknown` when a value cannot be resolved), so a bug report or a build script can paste or parse it: `go2cs.release` (the go2cs corpus release this binary belongs to, e.g. `1.24.13.3`), `go2cs.converter` (this binary's identity, `exe-` plus the first 16 hex digits of its SHA-256 — the value test manifests record), `go.packages` (the `go.*` NuGet version `-recurse=nuget` defaults `$(GoStdLibVersion)` to, e.g. `1.24.13.*`), `go.toolchain` (the active Go toolchain, `go env GOVERSION`), `go.build` (the Go toolchain that built this binary), `vcs.revision` and `vcs.modified` (the commit a binary built from a clone was built from, and whether its tree had local changes; Go 1.24 does not stamp a binary built from a `git worktree` checkout, so that reads `unknown`). New keys are only ever appended. |
| ~~`-cgo`~~ | ~~Also convert cgo-targeted files.~~ Not yet functional, but planned: the [cgo interop plan](PLAN-cgo-interop.md) lays out a P/Invoke-backed bridge for `import "C"`, and this flag comes alive with it. |

All converted C# code references a hand-written runtime library (`golib`, published as the [`go.lib`](https://www.nuget.org/packages/go.lib)
NuGet package) plus a set of Roslyn source generators that supply Go semantics at compile time (published as
[`go.gen`](https://www.nuget.org/packages/go.gen)). A `-recurse=nuget` conversion wires both up for you.

### Diagnostics

A failure in the Go input itself is printed on stderr in MSBuild's canonical error format, one line per error, so CI logs
and editor problem matchers (for example VS Code's `$msCompile`) pick it up:

```
/src/app/main.go(3,16): error GO2CS1003: cannot use 1 (untyped int constant) as string value in variable declaration
```

| Code | Meaning |
|:--|:--|
| `GO2CS1000` | The package did not load cleanly (an error `go/packages` does not classify). The package still converts best-effort. |
| `GO2CS1001` | The package did not load cleanly: a `go list` error, such as a missing module or no Go files for this build. |
| `GO2CS1002` | The package did not load cleanly: a Go parse error. The package still converts best-effort. |
| `GO2CS1003` | The package did not fully type-check. It still converts best-effort, but code that depends on the failing expression is emitted untyped and will not compile. |
| `GO2CS2001` | Refused: a cgo source (`import "C"`) is selected for this build, and cgo has no C# conversion yet. The run exits non-zero. Convert with `CGO_ENABLED=0` or exclude the file. |

A diagnostic with no source position names `go2cs` as its origin. The `WARNING:` / `Refusing to convert:` summary line
printed before these lines is unchanged.

### Converting a real-world module

The `-recurse` option converts a **whole downloaded application together with every third-party dependency
package** in its transitive import closure — in dependency order (least-dependencies-first) — while
**referencing** (not reconverting) the pre-converted standard library. The result is a C# solution you can
open and build. With `-recurse=nuget` the standard library, the `golib` runtime and the `go2cs-gen`
analyzer come from [nuget.org](https://www.nuget.org/packages?q=go2cs%20ritchiecarroll), so nothing has to
be staged on the machine beforehand. (Prefer the standard library as local C# source? See
[building against a local standard library](#optional-build-against-a-local-standard-library) below.)

#### Build tags for a module

go2cs converts a module, and validates it against its own tests, as Go builds it with the `safe` build tag.
Some libraries reach into Go's runtime internals with unsafe pointer arithmetic, which converted code cannot
do, and ship a fallback for builds tagged `safe`; with the tag, that fallback is the code you get. The visible
difference is small: testify's assertion messages, for example, do not print the unexported fields of a struct.
When a module is validated, Go's own `go test` run uses the same tag, so both sides are compared on the same
code. To convert the files a plain `go build` selects instead, pass `-module-safe-tag=false`.

Wondering which real-world Go packages make good conversions? The
**[go2cs Target Atlas](https://go2cs.net/TargetAtlas.html)** ranks the most-depended-on Go modules and
grades how hard each one is to convert.

Here is the full round-trip for a small CLI that uses [`github.com/fatih/color`](https://github.com/fatih/color),
which itself pulls in `github.com/mattn/go-colorable`, `github.com/mattn/go-isatty`, and `golang.org/x/sys` —
a genuine dependency graph:

> **NOTE:** _the commands below run verbatim in both PowerShell and a POSIX shell (bash/zsh), on Windows and
> on Linux. Where the two genuinely differ, both forms are shown and the block is labelled with its shell._

**1 — Go: get the app and confirm it builds as Go.**

```shell
mkdir colordemo
cd colordemo
go mod init example.com/colordemo
```

Create `main.go` (`go mod tidy` needs a real source file — with none present it reports
`warning: "all" matched no packages`):

```go
package main

import "github.com/fatih/color"

func main() {
	color.New(color.FgGreen, color.Bold).Println("hello from fatih/color")
}
```

Next, pin the app's dependencies to releases that go2cs's Go version can read, and confirm it builds as Go.

> **NOTE:** _go2cs's type-checker reads only modules whose `go` directive (and their dependencies') is no newer than the Go release go2cs is built with (see [Requirements](#requirements)). Newer `fatih/color` and `golang.org/x/sys` releases need a newer Go, which would fail step 2 with_ `package requires newer Go version`_; pin as shown._
>
> _The `GOTOOLCHAIN=local` below is what makes that error appear at all. Left unset, Go **silently downloads and re-execs** whichever newer toolchain a `go`/`toolchain` directive asks for, so the build succeeds against a standard library go2cs has no published packages for. go2cs detects the switch and says so, but pinning the toolchain keeps the whole round-trip on one Go release, which is what you want._

First pin the toolchain, so Go uses the release you have instead of fetching the newer one a dependency asks
for. This is the one command whose syntax is shell-specific:

```powershell
$env:GOTOOLCHAIN = 'local'   # PowerShell
```

```bash
export GOTOOLCHAIN=local     # bash / zsh
```

Then, in either shell:

```shell
go get github.com/fatih/color@v1.18.0   # a Go 1.24-compatible release (v1.19+ requires Go 1.25)
go mod tidy                             # download color + its dependencies
go build ./...                          # baseline: confirm it compiles as Go first
```

**2 — go2cs: recurse-convert the app.** `go2cs` is the converter you put on your `PATH` in *Installing the
converter* above, so it runs from anywhere. Point it at the **app** directory, and give it an output root
to write the generated C# into:

```shell
cd path/to/colordemo
go2cs -recurse=nuget . csharp
```

`go2cs` discovers the imports and converts each package, least-dependencies-first
(`go-isatty` and `x/sys` → `go-colorable` → `color` → the app), into a parallel tree under `csharp/`,
leaving your original Go source untouched. The converted app lands under `csharp/src/<import-path>`, and
every third-party library under `csharp/pkg/<import-path>`. The standard library is referenced as
`go.<pkg>` packages, and the generated `csharp/Directory.Build.props` supplies the version they resolve —
so the projects restore and build with no further configuration. A per-project `.slnx` sits next to every
generated `.csproj`, each with that project plus its converted dependencies. By default a `-recurse=nuget` run
also asks the [nugetgo.net](https://nugetgo.net) registry whether a third-party module has a published
conversion, and references one that does as that NuGet package instead of converting it (see
[Mapping modules to NuGet packages](#mapping-modules-to-nuget-packages); `-nuget-map off` opts out).
Where Go's own semantics draw a C# warning in one file (a local whose every use folded to a constant, a
package variable nothing writes), an `.editorconfig` beside that `.csproj` turns it off for that file only
([Conversion Strategies](ConversionStrategies.md#package-conversion)).

_Code converted from `main.go` should look like the following in `main.cs`:_
```c#
namespace go.example.com;

using color = github.com.fatih.color_package;
using github.com.fatih;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object helloFromFatihColorˢ = (@string)"hello from fatih/color"u8;

internal static void Main() {
    color.New(color.FgGreen, color.Bold).Println(helloFromFatihColorˢ);
}

} // end main_package
```

> **NOTE — platforms:** _all four steps run on **Windows** (`windows/amd64`) and **Linux** (`linux/amd64`).
> The conversion records the platform it targets, and the go2cs packages compile and run against that
> platform's flavor: `win-x64` for a Windows conversion, `linux-x64` for a Linux one. Use the current go2cs
> packages, with a converter built from a checkout at or after the commit that published them. The output
> matches `go run`, including `fatih/color`'s colors in an interactive terminal, on both platforms.
> macOS (Intel and Apple silicon): packages ship for both chips, the behavioral suite passes on both, and the README
> walkthrough runs. The standard library is not yet validated package by package on macOS, so don't expect it to be
> fully operational there yet. Other platforms and architectures are tracked in the Roadmap's
> [Platforms section](Roadmap.md#platforms)._

**3 — C#: build the generated solution.** The app's per-project `.slnx` builds the app and its whole
converted dependency tree, restoring the go2cs packages on the way; opening it in Visual Studio makes the
app the startup project (F5 runs it):

```shell
cd csharp/src/example.com/colordemo
dotnet build example.com.colordemo.slnx -c Debug
```

**4 — C#: run the converted app.** From the same folder:
```shell
dotnet run -c Debug
```

The build output lands under `csharp/.artifacts/bin/`, where a native launcher is built beside the
assembly — `colordemo.exe` on Windows, `./colordemo` on Linux — and runs the same program.

To ship the app, `dotnet publish -c Release -r <runtime>` produces a self-contained, ReadyToRun-compiled
program. A converted project trims in partial mode: the .NET framework is trimmed, while golib, the
converted packages and the app itself are kept whole, so the members they reach by reflection survive the
publish. A project may set its own `TrimMode`.

_Expected output:_

![colorapp-output](images/colorapp-output.png)

> **NOTE:** this `fatih/color` example **compiles clean** — app plus all four dependency projects — **and runs**. The standard library it references is validated package by package against Go's own tests; see [Validated Test Packages](ValidatedTestPackages.md).

#### Optional: convert the module only, and deal with its dependencies later

A dependency closure is not always convertible today — a large third-party SDK can hit a converter defect,
or pull in a package go2cs cannot yet handle — and under plain `-recurse` that blocks the packages you
actually came for. `-recurse=module` narrows the **scope** to the input module's own packages:

```shell
cd path/to/colordemo
go2cs -recurse=module . csharp
```

Every package under the module's own `go.mod` converts, in dependency order, exactly as it would under the
full `-recurse`; every third-party package is *referenced* — into `csharp/pkg/<import-path>`, the same place
the full run would have converted it — but never converted, so nothing about it can fail the run. The
converter prints the referenced-but-unconverted list when it finishes:

```text
Closure: 73 packages discovered — converting 1 app, referencing 4 third-party + 67 stdlib (1 skipped)
...
Third-party packages referenced but NOT converted (-recurse=module): 4
  github.com/fatih/color
  github.com/mattn/go-colorable
  github.com/mattn/go-isatty
  golang.org/x/sys/windows
```

Those references are unresolved until something is written at those paths, so the generated solution does
**not** build yet — the mode's deliberate trade. Re-running the same conversion **without** `=module` (once
the dependencies convert) writes them at exactly those paths and resolves the references; the app's own
converted `.cs` and `.csproj` come out byte-identical either way, so nothing you have done to them is lost.

#### Optional: build against a local standard library

Some work wants the standard library on disk as C# source instead — to step into it in the debugger, or to
change it and rebuild. `deploy-core.ps1` is a PowerShell script in the go2cs repo's **`src/`** folder (it is
*not* on your `PATH`), so run it from there, from a PowerShell prompt — `powershell` or `pwsh` on Windows,
`pwsh` on Linux and macOS. It stages the standard library, runtime and analyzer at `<gopath>/src/go2cs` —
the "deploy root" a converted project resolves through `$(go2csPath)`, where `<gopath>` is the directory
`go env GOPATH` prints:

```powershell
cd path/to/go2cs/src
./deploy-core.ps1
```

Staging is **per-machine**, not per-app; redo it when you pull a new go2cs version. Then convert with plain
`-recurse`, pointing at the deploy root:

```shell
cd path/to/colordemo
go2cs -recurse . -go2cspath <gopath>/src/go2cs
```

The converted app lands under `<gopath>/src/go2cs/src/<import-path>` and its converted third-party
dependencies under `<gopath>/src/go2cs/pkg/<import-path>`, with the standard library referenced at
`<gopath>/src/go2cs/core/`; build and run it exactly as in steps 3 and 4 from there. The converted C# is
the same either way — only the reference style in the generated projects differs.

The output root does not have to be the deploy root: `go2cs -recurse -go2cspath <gopath>/src/go2cs . csharp`
keeps the generated tree in `csharp/` while still referencing the deploy root. The conversion records that
root as the `$(go2csPath)` default in the output root's generated `Directory.Build.props`, so the generated
solution builds from anywhere with no extra configuration — override with a `go2csPath` environment variable
or a `-p:go2csPath` build global if the runtime root later moves.

#### Mapping modules to NuGet packages

With `-recurse=nuget`, go2cs asks the [nugetgo.net](https://nugetgo.net) registry whether someone has already
converted and published each third-party module. A mapped module is **referenced, not converted**: one
exact-pinned `PackageReference` per module. A mapping applies only when a published version describes itself (in
`go2cs/source-metadata.txt`) as that module, at exactly the version your build selects, and was built for this
go2cs release; the highest such revision wins. Anything else converts locally, and the end-of-run table says why.
A module supplied by a `replace` directive is never mapped. A mapped package that itself depends on another
third-party package is not yet tested end to end.

The choices are pinned, with each package's SHA-512, in `go2cs.nuget.lock` in the output root. A later run reuses
the pin offline, warns if a source now disagrees, and refuses a package whose bytes no longer match.

- `-nuget-map <source>` adds your own mapping source: a local file or an `https://` URL in the registry's format.
  It is repeatable, and the first source that names a module answers for it. nugetgo.net answers every module your
  sources do not name, so a short override file is enough.
- `-nuget-map off` turns mapping off and makes no request.
- `-nuget-map-only`, `-nuget-map-exclude`, `-nuget-map-refresh`, `-nuget-map-canonical-only` and `-nuget-map-feed` are listed under
  [Common options](#common-options). Every `-nuget-map*` flag needs `-recurse=nuget`.

## Project layout

| Path | Contents |
|:--|:--|
| `src/go2cs/` | The converter (written in Go, using `go/ast` + `go/types`). |
| `src/core/` | The converted Go standard library — every package, with `unsafe` and `testing` hand-written rather than converted. Everything (tests, tour, NuGet) builds against this one tree. |
| `src/core/golib/` | The C# runtime library (`slice`, `map`, `channel`, `@string`, built-ins, type aliases). |
| `src/core/go2cs/` | Shared `Symbols` project — the canonical marker glyphs used by the runtime and the generators. |
| `src/gen/go2cs-gen/` | Roslyn source generators (interface implementation, receiver overloads, struct embedding). |
| `src/tour/` | [Tour of go2cs](https://github.com/ritchiecarroll/go2cs/blob/master/src/tour/README.md) — the Tour of Go beside a live Go-to-C# workspace. |
| `src/tests/Behavioral/` | Per-feature Go↔C# equivalence tests (transpile, compile, run-and-compare). |
| `src/tests/Performance/` | Go vs transpiled C# runtime benchmarks (JIT and Native AOT) — see the [performance comparison](Performance.md) for current numbers. |

Contributors: start with [`CONTRIBUTING.md`](https://github.com/ritchiecarroll/go2cs/blob/master/CONTRIBUTING.md),
then see [`Architecture.md`](Architecture.md), [`ConversionStrategies.md`](ConversionStrategies.md) and
[`Roadmap.md`](Roadmap.md) for details. There's plenty of low-hanging fruit here; jump in if you'd like to help.

## Status

The converter builds idiomatic C# for the full range of Go language features, guarded by hundreds of
Go-vs-C# behavioral test projects. Each one is transpiled, compiled and compared against a committed golden,
and each runnable one is executed with its output compared against Go's. The entire Go standard library
compiles cleanly as .NET assemblies.
<!-- "hundreds" (697 at the 2026-09-26 count) counted at master db1bd885a2: 697 directories under src/tests/Behavioral
     carry a committed .cs.target golden; the two harness directories (BehavioralRunner, BehavioralTests)
     carry none and are not counted. -->

The converted standard library reproduces **Go built with `-tags purego`** — a managed runtime cannot
execute Go's hand-written `.s` assembly, so the portable pure-Go variants of the asm-backed crypto and hash
functions are the faithful target (`-stdlib` and `-tests` apply the tag by default; see
[Conversion Strategies](ConversionStrategies.md#the-standard-library-reproduces-go--tags-purego)).

Compiling is not the same as running correctly, so the library is also validated **operationally**. Each
package's own `_test.go` suite is converted to C#, built against the converted standard library, and run
under a Go-semantics test host. Its results are compared verdict for verdict against a clean `go test -json`
run, and every difference is disclosed by exact failure signature; a test withdrawn is withdrawn from both
sides, by name. Every implementable package validates this way, on Windows and on Linux where it applies,
and the few packages outside that set are each listed with the reason they cannot be validated.
[Validated Test Packages](ValidatedTestPackages.md) carries the current counts, and the results are
reproducible via [Try it yourself](#try-it-yourself--validate-a-converted-test-suite). Real third-party Go
modules convert too, and `go2cs -tests -recurse` validates a module against its own test suites the same way.
go2cs moves to newer Go and .NET releases one at a time, and each move
re-validates every package against that release's own tests; see the [Roadmap](Roadmap.md).
<!-- The prose no longer quotes the counts. They were 225 of 225 and 225 of 230 on 2026-10-01 at master
     c2591d5b95, read from the Phase 4 progress header of docs/ValidatedTestPackages.md, which carries
     the current figures. -->

### Try it yourself — validate a converted test suite

Every validated package ships its **converted C# test sources** next to the production code under
[`src/core`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core) (for example,
[`unicode/utf8/utf8_test.cs`](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/unicode/utf8/utf8_test.cs)),
so you can read the exact C# that runs — and re-run the validation yourself. You need
**[Go 1.24.13](https://go.dev/dl/)** (for the reference `go test` run), the
**[.NET 10 SDK](https://dotnet.microsoft.com/download)**, and `go2cs` on your `PATH` (see
[installing the converter](#installing-the-converter)):

```sh
# 1. Convert unicode/utf8's test suite, build + run the C# host, and diff it against `go test`.
#    The second argument is the package's home in the converted tree; the converter locates the
#    runtime and its stdlib dependencies from there — no flags or environment setup required.
#    (On Windows, Go's source lives under "C:\Program Files\Go\src"; elsewhere use "$(go env GOROOT)/src".)
#    Off Windows the converter binary is `go2cs`, not `go2cs.exe`.
#    The first run builds the converted runtime and its dependencies, so it allows 10 minutes.
go2cs.exe -tests -test-action all -test-timeout 10m \
    "C:\Program Files\Go\src\unicode\utf8" \
    src/core/unicode/utf8
```

Expected final line:

```text
Validated 14 tests against go test (0 skipped identically on both sides, 1 disclosed-divergent (deferred), 47 disclosed-unsupported declarations excluded).
```

The command converts the `_test.go` files to C#, generates a test host, builds it against the converted
standard library, runs it in an isolated process, captures a clean `go test -json -count=1` baseline, and
compares terminal results by full Go test name — reporting `validated` only when every test agrees on both
sides and every unsupported declaration (benchmarks, examples) is accounted for. It regenerates the local
converted `.cs` in place; the Go source copies and run manifests it stages are git-ignored. The same
command validates every other package on the table — substitute its GOROOT source path and its
`src/core/<pkg>` path in the two arguments.

The test host is published in the Release configuration and run with the CLR's tiered JIT off
(`DOTNET_TieredCompilation=0`), so every method is compiled once at full optimization and a verdict
cannot depend on when the runtime promotes a method; `-test-tiered` turns tiering back on, and
`-test-config Debug` publishes a Debug host. ReadyToRun is opt-in — set `PublishReadyToRun=true` in the
environment to publish a precompiled host — because it buys a slower steady state on heavy suites, a
larger host and a longer first publish, and because precompiled code does not keep the JIT's stack
frames, so a test that asserts on frames can answer differently.

A few packages carry a **disclosed divergence**: a Go test asserting something the converted runtime does
not satisfy — an allocation count Go meets through compiler escape analysis, or a collectibility check Go
answers from per-safepoint liveness maps. Some of these a managed runtime provably cannot satisfy; others
— marked `deferred`, such as `unicode/utf8`'s own zero-allocation `TestRuneCountNonASCIIAllocation` — it
can, and each of those is pinned against the named plan that will retire it. Rather than skip those
tests, each affected package pins the divergence in a hand-owned, committed
[`go2cs_test_disclosures.json`](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/bytes/go2cs_test_disclosures.json)
that the differential oracle matches by *exact failure signature* — any other failure is still a hard
mismatch — and reports as **disclosed-divergent** in the summary. Packages without a manifest compare
strictly.

### Performance

_Everyone asks:_ how fast is the transpiled C# compared to the original Go — including startup time,
memory, and Native AOT builds? See the [performance comparison](Performance.md) — **`TL;DR`**: _usually
slower than native Go, [but not always](Background.md#how-fast-is-converted-code)_: maps and the optimized
[stack string](ConversionStrategies.md#strings-string-and-sstring) path run at **parity with Go or
faster in both C# variants**. Most compute-shaped code — channels included — sits within a small multiple
of Go, with runtime structural-interface satisfaction the honest outlier. Some optimization is already in,
such as the [ref struct](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/ref-struct)
based stack strings and [stack slices](ConversionStrategies.md#slices-and-arrays); broader optimization is
planned (see the [Roadmap](Roadmap.md#performance)), and the parity rows show the ceiling, not the finish line.

## Milestones

High level timeline of the project's major turning points; the tags carry the details.

| Date | Milestone | Commit / Tag | Notes |
|:--|:--|:--|:--|
| 2018-05-21 | Project inception | `929d1457f` | A C#/.NET converter built on an ANTLR4 Go grammar. |
| 2022-03-13 | [`v0.1.2` release](NEWS.md#march-13-2022--v012-release) | [`v0.1.2`](https://github.com/ritchiecarroll/go2cs/releases/tag/v0.1.2) | The mature ANTLR4-era converter, tagged. |
| 2025-01-12 | [The converter is rewritten in Go](NEWS.md#january-12-2025--the-converter-is-rewritten-in-go-go2cs-version-2) | `87465f5f5` | Rebuilt on `go/ast` + `go/types`, with the `golib` runtime library and Roslyn source generators supplying Go's semantics. |
| 2025-05-05 | [First full standard-library auto-conversion](NEWS.md#may-5-2025--first-full-standard-library-auto-conversion) | `6ca1c45b7` · [`full-conversion-2025-05`](https://github.com/ritchiecarroll/go2cs/releases/tag/full-conversion-2025-05) (`cc14584c7`) | Every Go file gets a C# file; compiling comes later. |
| 2026-07-10 | [**First clean full-standard-library compile**](NEWS.md#july-10-2026--the-entire-go-standard-library-compiles-in-net) | `51ba5d9cf` · [`stdlib-green-2026-07-10`](https://github.com/ritchiecarroll/go2cs/releases/tag/stdlib-green-2026-07-10) | Every package compiles with zero errors, `runtime`, `reflect` and `net/http` included ([details](StdLibCompileMilestone.md)). |
| 2026-07-14 | [Standard library on NuGet](NEWS.md#july-14-2026--the-converted-go-standard-library-is-on-nuget) | `2363af0e6` · `dd821a556` · [`nuget-stdlib-2026-07-14`](https://github.com/ritchiecarroll/go2cs/releases/tag/nuget-stdlib-2026-07-14) | A converted app references the standard library from nuget.org, with no local go2cs checkout. |
| 2026-07-17 | [**First Go test suite passing in C#**](NEWS.md#july-17-2026--gos-own-tests-now-pass-in-c) | `337a928df` · [`utf8-tests-green-2026-07-17`](https://github.com/ritchiecarroll/go2cs/releases/tag/utf8-tests-green-2026-07-17) | `unicode/utf8` matches `go test`, test for test. |
| 2026-08-08 | [**Go programs run on Linux**](NEWS.md#august-8-2026--go-programs-run-on-linux) | [`linux-first-run-2026-08-08`](https://github.com/ritchiecarroll/go2cs/releases/tag/linux-first-run-2026-08-08) | Converted programs, the `fatih/color` walkthrough included, match `go run` on Linux. |
| 2026-10-01 | [**Every implementable standard-library package validates**](NEWS.md#october-1-2026--every-implementable-standard-library-package-validates) | `133ca704e` · `nuget-1.24.13.3` | Every implementable package passes its own Go tests in C#, `runtime` included, on Windows and Linux. |

## C# to Go?

A full code-based conversion from C# to Go is not offered (it would require so many restrictions as to be
impractical). To call compiled .NET code *from* Go instead, see
[go-dotnet](https://github.com/matiasinsaurralde/go-dotnet) (CLR hosting for .NET Core) or
[embedding Mono via cgo](https://www.mono-project.com/docs/advanced/embedding/) for traditional .NET.

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
