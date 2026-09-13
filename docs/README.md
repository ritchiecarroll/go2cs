![go2cs](images/go2cs-small.png)

# go2cs — Go to C# Converter

[![golib NuGet package](https://img.shields.io/nuget/dt/go.lib?label=go.lib%20NuGet%20package)
](https://www.nuget.org/packages/go.lib)

Browse all: [Go Standard Library NuGet packages](https://www.nuget.org/packages?q=go2cs%20ritchiecarroll)

---

## 📰 NEWS — The Go 1.23.12 record closes at 97.6% of the implementable set; the corpus hops to Go 1.24

**204 of the 215 testable standard-library packages pass their own Go test suites in C#** — 28,459
matching verdicts against `go test -json`, compared verdict for verdict, with 167 divergences
disclosed by exact failure signature and nothing else waived. Six of those 215 cannot be validated
at all — no eligible tests on this platform, a broken upstream oracle, a suite whose whole subject
is the raw memory layout a managed runtime deliberately does not have, or a comparison that runs
cleanly and validates nothing — so the honest denominator is **209, putting the roster at 97.6%**.
Each of the six is listed with its class, mechanism and evidence in the
[exclusion ledger](ValidatedTestPackages.md#excluded-packages), and any one of them rejoins the
count the day its evidence changes. On Linux, 198 of the 202 applicable rows validate at their own
Linux counts. A package appears on the [roster](ValidatedTestPackages.md) only when *every* eligible
test agrees, and every row links a [proof page](validation/index.md) listing Go's verdict beside
go2cs's, test by test.

Those figures are the **Go 1.23.12 anchor** — a closed record rather than a running total. The
corpus now moves to **Go 1.24.13**, and a version hop re-derives every roster row from the new
release's own test sources, so the five packages still unbanked here re-validate there on exactly
the footing of the 204 that banked. The 1.23.12 corpus ships one final NuGet release first, freezing
its roster, its proof pages and every package README at the record above — see
[the announcement](NEWS.md#september-7-2026--the-go-12312-record-closes-at-its-anchor-the-corpus-hops-to-go-124).

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
* View example converted test: [`utf8_test.cs`](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/unicode/utf8/utf8_test.cs)
* See current project [status](#status) and [milestones](#milestones)

[![Tour of go2cs showing Go and generated C# side by side](images/tour-of-go2cs.png)](images/tour-of-go2cs.png)

### Frequently asked questions

* Why is a Go to C# transpiler needed? _[Integration opportunities](Background.md#background)._
* Won't converted C# code be slower? _[Usually — but not always](#performance)._

## Transpiler Goals

Go leans on its compiler and runtime for slices, maps, channels, goroutines, `defer`/`panic`/`recover`,
multiple returns, struct embedding and interface duck-typing. go2cs maps each onto idiomatic C#, keeping
the machinery out of sight — in a small runtime library and compile-time source generators — so the
converted code stays close to the original Go.

- **Reads like Go.** Receiver methods become extension methods, multiple returns become tuples, struct
  embedding becomes promoted fields — the shape of the code is preserved.
- **Runs like Go.** Conversions prioritize behavioral equivalence first (e.g. a `goroutine` runs on the
  thread pool rather than being rewritten into `async`).
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

```csharp
[GoType] partial struct Person {
    internal @string name;
    internal int32 age;
}

public static bool IsAdult(this Person p) {
    return p.age >= 18;
}
```

### Real standard-library conversions, side by side

The goal — *reads like Go* — is easiest to judge on real code. Below are converted standard-library files
next to their original **Go 1.23.12** source, in order of increasing richness:

| Package | Go 1.23.12 source | Converted C# | What it shows |
|:--|:--|:--|:--|
| `errors` | [errors.go](https://github.com/golang/go/blob/go1.23.12/src/errors/errors.go) | [errors.cs](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/errors/errors.cs) | Error values and an unexported type satisfying the `error` interface. |
| `cmp` | [cmp.go](https://github.com/golang/go/blob/go1.23.12/src/cmp/cmp.go) | [cmp.cs](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/cmp/cmp.cs) | Generics with an ordered-type constraint. |
| `unicode/utf8` | [utf8.go](https://github.com/golang/go/blob/go1.23.12/src/unicode/utf8/utf8.go) | [utf8.cs](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/unicode/utf8/utf8.cs) | Constants keeping Go's hex/binary literal formatting; arrays and structs. |
| `sort` | [search.go](https://github.com/golang/go/blob/go1.23.12/src/sort/search.go) | [search.cs](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/sort/search.cs) | Binary search driven by a `func(int) bool` closure. |
| `strings` | [reader.go](https://github.com/golang/go/blob/go1.23.12/src/strings/reader.go) | [reader.cs](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/strings/reader.cs) | A struct with receiver methods, tuple returns, and interface implementation. |
| `container/list` | [list.go](https://github.com/golang/go/blob/go1.23.12/src/container/list/list.go) | [list.cs](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/container/list/list.cs) | A doubly-linked list — pointers and receiver methods. |

Browse the whole set under [`src/core`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core).

## Features

go2cs converts the full Go language surface — the same converter that emits the 302 packages above:

**Types & values**

- Slices and arrays (backing-array aliasing preserved, as in Go), maps, and UTF-8-backed `@string`
- `int` / `uint` as platform-width native integers; named numeric types and untyped-constant semantics
- Constants and `iota`, preserving Go's numeric literal formatting (hex, binary, underscores, exponents)
- Pointers with automatic heap-boxing driven by escape analysis; `nil`; `unsafe.Pointer`
- Type definitions and aliases — including exported aliases that resolve across assembly boundaries

**Functions & methods**

- Multiple return values and named results, as tuples
- Receiver methods as extension methods, with distinct pointer- and value-receiver overloads
- Function values, closures (honoring Go's shared-storage capture semantics), and variadic functions
- `defer` / `panic` / `recover`, including named results observed and mutated by deferred closures

**Concurrency**

- Goroutines, run on the thread pool (behavioral equivalence first — not rewritten into `async`)
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
maps to C# (with [`ConversionStrategies-Reference.md`](ConversionStrategies-Reference.md) for the full detail).

![GopherDotNetBotFrisbee](images/GopherDotNetBotFrisbee.png)

## Requirements

- **[.NET 10.0 SDK](https://dotnet.microsoft.com/download)** — to build and run the converted C#. Converted
  projects target `net10.0`, the framework named by
  [`src/Directory.Build.props`](https://github.com/ritchiecarroll/go2cs/blob/master/src/Directory.Build.props).
- **[Go 1.23+](https://go.dev/dl/)** — the converter is a Go program, and it uses the Go toolchain to load
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
```

### Common options

| Option | Description |
|:--|:--|
| `-stdlib` | Convert the Go standard library (optionally followed by specific package names). |
| `-recurse` | Recursively convert a downloaded module **and its third-party dependencies** in dependency order, referencing (not reconverting) the pre-converted standard library through `$(go2csPath)`. An optional second positional output root isolates the generated `src\` app and `pkg\` dependency trees; references inside that graph are relative. A package that fails to load or convert is reported and skipped, and the run continues with the rest. See [Converting a real-world module](#converting-a-real-world-module). |
| `-recurse=module` | Same recursion, narrower **scope**: convert the input module's own packages (every package under its `go.mod`, in dependency order) and **stop there** — each third-party package is still referenced into the `pkg\` tree, but none of them is converted, so a dependency closure that go2cs cannot yet convert can no longer hold up the module's own code. The referenced-but-unconverted packages are listed at the end of the run; converting them into the same output root later resolves those references. See [Converting a real-world module](#converting-a-real-world-module). |
| `-recurse=nuget` | Same, but the standard library, the `golib` runtime and the analyzer come from NuGet — [`go.<pkg>`](https://www.nuget.org/packages?q=go2cs%20ritchiecarroll) + [`go.lib`](https://www.nuget.org/packages/go.lib) + [`go.gen`](https://www.nuget.org/packages/go.gen) — so nothing is staged locally. The app's own and third-party converted packages stay project references. A reference style and a scope are independent, so the values combine: `-recurse=module,nuget`. **The published packages exist for one Go release** — the release go2cs itself is built with — so the module being converted has to resolve to that release; go2cs checks before writing anything and refuses, naming both versions, rather than emitting a project whose restore fails on whichever packages that release added or moved. |
| `-tests` | Also convert the package's eligible `_test.go` suite and emit a runnable C# test-host project (default off; cannot be combined with `-recurse`). Forces `-comments` on and self-locates `$(go2csPath)` by walking up from the output directory, so the two-argument form works from a bare clone with no flags or environment setup. See [Try it yourself](#try-it-yourself--validate-a-converted-test-suite). |
| `-test-action <action>` | With `-tests`: one of `convert` (default), `build`, `run`, `compare`, or `all`. `convert` and `all` convert the package and its tests; `build` / `run` / `compare` act on the **existing** converted artifacts — validated against the test manifest's recorded input digest — without reconverting. `compare` (and `all`) runs both `go test -json -count=1` and the converted C# test host and diffs the terminal results by test name. |
| `-test-timeout <duration>` | Package deadline for a converted-test action, in Go duration syntax (default `2m`). For `run`/`compare` it is handed to **both** sides — `go test -timeout` and the converted host's own `-timeout` — so they agree. A suite that legitimately runs long needs a value above both defaults: `hash/maphash` takes ~15 minutes in C# where Go's takes 7.6 seconds, so it is validated with `-test-timeout 30m`. |
| `-go2cspath <dir>` | Runtime/stdlib root (env `GO2CSPATH`; default `~/go2cs`) used by generated `$(go2csPath)…` references, and the output root for `-stdlib`. For a single-package/file conversion, C# output goes to optional `[output_dir]` (in place by default). |
| `-goroot` / `-gopath` | Override the detected Go root / path. |
| `-platforms <os/arch>` | Target platform for build-tagged files (defaults to the host). A comma-separated **list** (`windows/amd64,linux/amd64,darwin/amd64`) is accepted and today requires `-platform-census`: a conversion still emits for exactly one target, so a list without the census flag is rejected rather than silently converting the first. |
| `-platform-census <dir>` | With `-stdlib` and two or more `-platforms` targets: convert once per target into an isolated, seeded staging root under `<dir>`, compare what each run actually emitted, and write `<dir>\platform-manifest.json` classifying every artifact as shared, variant, partial or platform-exclusive. Produces **no** converted output of its own — `-go2cspath` is read as the seed and never written to. |
| `-tags <list>` | Build tags applied when loading packages. `-stdlib` and `-tests` apply `purego` by default; an explicit value replaces it. |
| `-indent <n>` | Spaces per indent level (default 4). |
| `-var` | Prefer `var` declarations where the type is obvious (default on). |
| `-uco` | Emit channel operators instead of method calls (default on). |
| `-comments` | Carry source comments into the output (best effort, see [go/ast comment status](https://github.com/golang/go/issues/20744)). |
| `-csproj <file>` | Generate project files from a custom `.csproj` template instead of the embedded one. |
| `-tree` | Print each file's Go parse tree (`go/ast`) to stdout during conversion — a diagnostic aid. |
| `-debug` | Disable the converter's per-file panic recovery, so a conversion failure crashes with a full stack trace instead of being reported as a warning. |
| ~~`-cgo`~~ | ~~Also convert cgo-targeted files.~~ Not yet functional — but planned, not abandoned: the ratified [cgo interop plan](PLAN-cgo-interop.md) lays out the `import "C"` ladder (P/Invoke-backed, staged after the current validation campaign), and this flag comes alive with it. |

All converted C# code references a hand-written runtime library (`golib`, published as the [`go.lib`](https://www.nuget.org/packages/go.lib)
NuGet package) plus a set of Roslyn source generators that supply Go semantics at compile time (published as
[`go.gen`](https://www.nuget.org/packages/go.gen)). A `-recurse=nuget` conversion wires both up for you.

### Converting a real-world module

The `-recurse` option converts a **whole downloaded application together with every third-party dependency
package** in its transitive import closure — in dependency order (least-dependencies-first) — while
**referencing** (not reconverting) the pre-converted standard library. The result is a C# solution you can
open and build. With `-recurse=nuget` the standard library, the `golib` runtime and the `go2cs-gen`
analyzer come from [nuget.org](https://www.nuget.org/packages?q=go2cs%20ritchiecarroll), so nothing has to
be staged on the machine beforehand. (Prefer the standard library as local C# source? See
[building against a local standard library](#optional-build-against-a-local-standard-library) below.)

Wondering which real-world Go packages make good conversions? The
**[go2cs Target Atlas](https://go2cs.net/TargetAtlas.html)** surveys the Go ecosystem's best
candidates — the study behind the first operational package conversions now being planned.

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

Next, pin the app to a **Go 1.23-compatible** dependency set and confirm it builds as Go.

> **NOTE:** _go2cs is built with **Go 1.23**, so its type-checker only reads modules whose `go` directive — and their dependencies' — is **≤ 1.23**. `fatih/color` v1.19+ and current `golang.org/x/sys` require Go 1.25, which would fail step 2 with_ `package requires newer Go version go1.25`_; pin as shown._
>
> _The `GOTOOLCHAIN=local` below is what makes that error appear at all. Left unset, Go **silently downloads and re-execs** whichever newer toolchain a `go`/`toolchain` directive asks for, so the build succeeds against a standard library go2cs has no published packages for. go2cs detects the switch and says so, but pinning the toolchain keeps the whole round-trip on one Go release, which is what you want._

First pin the toolchain, so Go uses the 1.23 you have instead of fetching the newer one a dependency asks
for — the one command whose syntax is shell-specific:

```powershell
$env:GOTOOLCHAIN = 'local'   # PowerShell
```

```bash
export GOTOOLCHAIN=local     # bash / zsh
```

Then, in either shell:

```shell
go get github.com/fatih/color@v1.18.0   # a Go 1.23-era release (v1.19+ requires Go 1.25)
go mod tidy                             # download color + its (Go 1.23-era) dependencies
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
generated `.csproj`, each with that project plus its converted dependencies.

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

> **NOTE — platforms:** _requires go2cs packages **1.23.1.5 or later**, the first release carrying Linux
> binaries alongside Windows. Steps 1–2 run on both platforms; steps 3–4 complete on **Windows** today, and
> on Linux for programs whose import closure stays within the `fmt`/`os`/`time` class. A closure reaching
> platform-divergent `syscall` surface (as this example's `x/sys` dependency does) still builds only on
> Windows — the remaining piece is the Linux side of that `syscall` surface, tracked in the Roadmap's
> [Platforms section](Roadmap.md#platforms--linux-and-the-multi-target-corpus-in-progress) with the
> operational detail in [PLAN-linux-operation.md](PLAN-linux-operation.md)._

**3 — C#: build the generated solution.** The app's per-project `.slnx` builds the app and its whole
converted dependency tree, restoring the go2cs packages on the way; opening it in Visual Studio makes the
app the startup project (F5 runs it):

```shell
cd csharp/src/example.com/colordemo
dotnet build example.com.colordemo.slnx -c Debug
```

**4 — C#: run the converted app.** Navigate into the default debug build folder — named for the target
framework — and run demo:
```shell
cd bin/Debug/net10.0
dotnet colordemo.dll
```

A native launcher is built beside it — `colordemo.exe` on Windows, `./colordemo` on Linux — and runs the
same program.

_Expected output:_

![colorapp-output](images/colorapp-output.png)

> **NOTE:** this `fatih/color` example **compiles clean** — app plus all four dependency projects — **and runs**. Bigger programs are a deeper milestone: the referenced standard library compiles in full, and making it **operational** package by package is the [Phase-4](Roadmap.md#phase-4--convert-and-run-go-package-tests) work that [Validated Test Packages](ValidatedTestPackages.md) tracks.

#### Optional: convert the module only, and deal with its dependencies later

A dependency closure is not always convertible today — a large third-party SDK can hit a converter defect,
or pull in a package go2cs cannot yet handle — and under plain `-recurse` that blocks the packages you
actually came for. `-recurse=module` narrows the **scope** to the input module's own packages:

```shell
cd path/to/myapp
go2cs -recurse=module . csharp
```

Every package under the module's own `go.mod` converts, in dependency order, exactly as it would under the
full `-recurse`; every third-party package is *referenced* — into `csharp/pkg/<import-path>`, the same place
the full run would have converted it — but never converted, so nothing about it can fail the run. The
converter prints the referenced-but-unconverted list when it finishes:

```text
Closure: 214 packages discovered — converting 9 app, referencing 118 third-party + 87 stdlib (0 skipped)
...
Third-party packages referenced but NOT converted (-recurse=module): 118
  google.golang.org/api/googleapi
  ...
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

## Project layout

| Path | Contents |
|:--|:--|
| `src/go2cs/` | The converter (written in Go, using `go/ast` + `go/types`). |
| `src/core/` | The converted Go standard library — 302 packages, with `unsafe` and `testing` hand-written rather than converted. Everything (tests, tour, NuGet) builds against this one tree. |
| `src/core/golib/` | The C# runtime library (`slice`, `map`, `channel`, `@string`, built-ins, type aliases). |
| `src/core/go2cs/` | Shared `Symbols` project — the canonical marker glyphs used by the runtime and the generators. |
| `src/gen/go2cs-gen/` | Roslyn source generators (interface implementation, receiver overloads, struct embedding). |
| `src/tour/` | [Tour of go2cs](https://github.com/ritchiecarroll/go2cs/blob/master/src/tour/README.md) — the Tour of Go beside a live Go-to-C# workspace. |
| `src/tests/Behavioral/` | Per-feature Go↔C# equivalence tests (transpile, compile, run-and-compare). |
| `src/tests/Performance/` | Go vs transpiled C# runtime benchmarks (JIT and Native AOT) — see the [performance comparison](Performance.md) for current numbers. |

Contributors: see [`CLAUDE.md`](../CLAUDE.md) for an architecture overview and
[`Architecture.md`](Architecture.md), [`ConversionStrategies.md`](ConversionStrategies.md), and
[`Roadmap.md`](Roadmap.md) for details. There's lots of low hanging fruit to be had here, jump in if you'd like to help...

## Status

The converter builds idiomatic C# for the full range of Go language features, gated by 519 Go-vs-C#
behavioral regression projects — each transpiled, compiled, byte-compared against a committed golden and,
where it is a runnable program, executed with its stdout compared against the Go original's. The entire Go
standard library (302 packages, Go 1.23.12) compiles cleanly as .NET assemblies.

The converted standard library reproduces **Go built with `-tags purego`** — a managed runtime cannot
execute Go's hand-written `.s` assembly, so the portable pure-Go variants of the asm-backed crypto and hash
functions are the faithful target (`-stdlib` and `-tests` apply the tag by default; see
[Conversion Strategies](ConversionStrategies.md#the-standard-library-reproduces-go--tags-purego)).

Compiling is not runtime parity. Making the library **operational** is the ongoing
[Phase 4](Roadmap.md#phase-4--convert-and-run-go-package-tests) work: each package's own `_test.go` suite
is converted to C#, built against the converted standard library, run under a Go-semantics test host, and
compared verdict-for-verdict against a clean `go test -json` baseline.
[Validated Test Packages](ValidatedTestPackages.md) tracks the set — reproducible via
[Try it yourself](#try-it-yourself--validate-a-converted-test-suite).

### Try it yourself — validate a converted test suite

Every validated package ships its **converted C# test sources** next to the production code under
[`src/core`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core) (for example,
[`unicode/utf8/utf8_test.cs`](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/unicode/utf8/utf8_test.cs)),
so you can read the exact C# that runs — and re-run the validation yourself. You need
**[Go 1.23.12](https://go.dev/dl/)** (for the reference `go test` run), the
**[.NET 10 SDK](https://dotnet.microsoft.com/download)**, and `go2cs` on your `PATH` (see
[installing the converter](#installing-the-converter)):

```sh
# 1. Convert unicode/utf8's test suite, build + run the C# host, and diff it against `go test`.
#    The second argument is the package's home in the converted tree; the converter locates the
#    runtime and its stdlib dependencies from there — no flags or environment setup required.
#    (On Windows, Go's source lives under "C:\Program Files\Go\src"; elsewhere use "$(go env GOROOT)/src".)
go2cs.exe -tests -test-action all \
    "C:\Program Files\Go\src\unicode\utf8" \
    src/core/unicode/utf8
```

Expected final line:

```text
Validated 14 tests against go test (0 skipped identically on both sides, 37 disclosed-unsupported declarations excluded).
```

The command converts the `_test.go` files to C#, generates a test host, builds it against the converted
standard library, runs it in an isolated process, captures a clean `go test -json -count=1` baseline, and
compares terminal results by full Go test name — reporting `validated` only when every test agrees on both
sides and every unsupported declaration (benchmarks, examples) is accounted for. It regenerates the local
converted `.cs` in place; the Go source copies and run manifests it stages are git-ignored. The same
command validates every other package on the table — substitute its GOROOT source path and its
`src/core/<pkg>` path in the two arguments.

A few packages carry a **disclosed divergence**: a Go test asserting something a managed runtime provably
cannot satisfy — an exact allocation count (Go's `testing.AllocsPerRun`, reached through compiler escape
analysis), or a collectibility check Go answers from per-safepoint liveness maps. Rather than skip those
tests, each affected package pins the divergence in a hand-owned, committed
[`go2cs_test_disclosures.json`](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/bytes/go2cs_test_disclosures.json)
that the differential oracle matches by *exact failure signature* — any other failure is still a hard
mismatch — and reports as **disclosed-divergent** in the summary. Packages without a manifest compare
strictly.

### Performance

_Everyone asks:_ how fast is the transpiled C# compared to the original Go — including startup time,
memory, and Native AOT builds? See the [performance comparison](Performance.md) — **`TL;DR`**: _usually
slower than native Go, [but not always](Background.md#why-convert-go-to-c)_: maps and the optimized
[stack string](ConversionStrategies.md#strings-string-and-sstring) path run at **parity with Go or
faster in both C# variants**. Most compute-shaped code — channels included — sits within a small
multiple of Go, with runtime structural-interface satisfaction the honest outlier. Save for
the [ref struct](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/ref-struct)
based stack string and [stack slice](ConversionStrategies.md#slices-and-arrays) work already landed, broad
optimization is targeted for _after_ Phase 4 — the parity rows show the ceiling, not the finish line.

Newer Go and .NET versions are planned; a validated baseline comes first.

## Milestones

High level timeline of the project's major turning points.

| Date | Milestone | Commit / Tag | Notes |
|:--|:--|:--|:--|
| 2018-05-21 | Project inception | `929d1457f` | A C#/.NET converter built on an ANTLR4 Go grammar with T4 templates. |
| 2020-07-09 | Runtime library + hand-converted stub | `9792eeea2` | The `golib` Go-semantics runtime and a curated hand-finished stdlib stub. |
| 2022-03-13 | [`v0.1.2` release](NEWS.md#march-13-2022--v012-release) | [`v0.1.2`](https://github.com/ritchiecarroll/go2cs/releases/tag/v0.1.2) | Tagged release of the mature ANTLR4-era converter. |
| 2025-01-12 | [Rewrite as "go2cs2" — Go-based converter](NEWS.md#january-12-2025--the-converter-is-rewritten-in-go-go2cs-version-2) | `87465f5f5` | Converter re-implemented in Go on `go/ast` + `go/types`; Roslyn source generators supply ancillary Go semantics. |
| 2025-05-05 | [First full standard-library auto-conversion](NEWS.md#may-5-2025--first-full-standard-library-auto-conversion) | `6ca1c45b7` · [`full-conversion-2025-05`](https://github.com/ritchiecarroll/go2cs/releases/tag/full-conversion-2025-05) (`cc14584c7`) | Every Go file got a C# file — the transpiler did not crash; it did not mean the emitted C# compiled. |
| 2026-06-25 | Baseline ↔ full-conversion separation | `3c8b3a848` | A compiling curated baseline and the WIP full conversion split apart, restoring a green build and the converter-improvement loop. |
| 2026-06-26 | First full-conversion package promoted | `05a53e8c0` | `sync/atomic` migrated into the baseline (`atomic.Pointer[T]` backed by a managed slot). |
| 2026-06-27 | [`math` package compiles clean](NEWS.md#june-27-2026--the-math-package-compiles-clean) | [`math-green-2026-06-27`](https://github.com/ritchiecarroll/go2cs/releases/tag/math-green-2026-06-27) (`914d4bd72`) | Nine packages greened via 19 behaviorally-tested converter fixes, including widely-imported `math`. |
| 2026-07-10 | [**First clean full-standard-library compile**](NEWS.md#july-10-2026--the-entire-go-standard-library-compiles-in-net) | `51ba5d9cf` · [`stdlib-green-2026-07-10`](https://github.com/ritchiecarroll/go2cs/releases/tag/stdlib-green-2026-07-10) | All **302** packages (Go 1.23.1) compile with zero errors — `runtime`, `reflect`, `net/http`, `go/types`, `crypto/tls` included ([details](StdLibCompileMilestone.md)). |
| 2026-07-14 | [Standard library on NuGet + NuGet-referencing conversion](NEWS.md#july-14-2026--the-converted-go-standard-library-is-on-nuget) | `2363af0e6` · `dd821a556` · [`nuget-stdlib-2026-07-14`](https://github.com/ritchiecarroll/go2cs/releases/tag/nuget-stdlib-2026-07-14) | `go.<pkg>` / `go.lib` / `go.gen` published to nuget.org; `-recurse=nuget` emits matching `<PackageReference>` entries, so a converted app needs no local go2cs checkout. |
| 2026-07-17 | [**First Go standard-library test suite passing in C#**](NEWS.md#july-17-2026--gos-own-tests-now-pass-in-c) | `337a928df` · [`utf8-tests-green-2026-07-17`](https://github.com/ritchiecarroll/go2cs/releases/tag/utf8-tests-green-2026-07-17) | `unicode/utf8` validates **14/14 against `go test -json`** under the hand-owned `go.testing` host; the differential pipeline goes live end to end. |
| 2026-07-18 | [**Phase-4 test suites expand — disclosed-divergence mechanism**](NEWS.md#july-18-2026--bytes-and-strings-tests-pass-with-disclosed-divergence) | `40f39d2be` · [`bytes-strings-tests-green-2026-07-18`](https://github.com/ritchiecarroll/go2cs/releases/tag/bytes-strings-tests-green-2026-07-18) · [`sort`](https://github.com/ritchiecarroll/go2cs/releases/tag/sort-tests-green-2026-07-18) · [`utf16`](https://github.com/ritchiecarroll/go2cs/releases/tag/utf16-tests-green-2026-07-18) | `bytes` (81), `strings` (68), `sort` (63) and `unicode/utf16` (8) validate, introducing the committed per-package disclosure manifest matched by exact failure signature. |
| 2026-07-26 | [**More than a quarter of the standard library's test suites pass in C#**](NEWS.md#july-26-2026--more-than-a-quarter-of-the-standard-librarys-test-suites-pass-in-c) | `44fcc4f04` | The validated set moves past leaf packages into `sync`, `regexp`/`regexp/syntax`, `strconv`, `bufio`, `compress/gzip`, the `crypto/sha*` family and the reflection-driven `errors` / `encoding/binary` / `go/token`. |
| 2026-08-01 | One tree — the converted standard library comes home to `src/core` | `2e8066da6` | Baseline and full conversion consolidate into a single tree with one `$(go2csPath)core/<pkg>` path scheme; no reference rewriting anywhere. |
| 2026-08-08 | [**Over half the stdlib's test suites validate in C#**](NEWS.md#august-8-2026--over-half-the-standard-library-validates-defers-reach-zero-allocation) | [`stdlib-half-validated-2026-08-08`](https://github.com/ritchiecarroll/go2cs/releases/tag/stdlib-half-validated-2026-08-08) | **110/215** packages, 13,628 matching verdicts; zero-allocation `defer` frames; Docs+Tests badge trust chain in every package. |
| 2026-08-08 | [**Go programs run on Linux**](NEWS.md#august-8-2026--go-programs-run-on-linux) | [`linux-first-run-2026-08-08`](https://github.com/ritchiecarroll/go2cs/releases/tag/linux-first-run-2026-08-08) | `hello, 世界`, an `os`/`time` program, and the real-world walkthrough (`fatih/color`, true ANSI colour) all byte-identical to `go run`; one L3 tree compiles windows+linux+darwin; one nupkg per package; one measured `libc syscall(2)` keystone. |
| 2026-08-22 | [**Over 75% of the standard library's test suites pass in C#**](NEWS.md#august-22-2026--over-75-of-the-standard-librarys-test-suites-pass-in-c) | [`stdlib-tests-75pct-2026-08-22`](https://github.com/ritchiecarroll/go2cs/releases/tag/stdlib-tests-75pct-2026-08-22) | **162/215** packages, 18,569 matching verdicts, 85 disclosed; converted frames report Go file:line positions; Go 1.23.1's terminal validation marker, with `release/go1.23` cut. |
| 2026-08-25 | [**Both runtime pins move: .NET 10 + Go 1.23.12**](NEWS.md#august-25-2026--both-runtime-pins-move-net-10-go-12312--and-the-whole-roster-re-proves-itself) | `925e48067` · `a2e079259` | 955 project files to `net10.0` with zero emission drift, three OS flavors green; the full roster re-derives from 1.23.12's own test sources — **162/162, 18,598** matching verdicts (+29, exactly the four re-derived rows). |
| 2026-08-29 | [**Over 90% of the standard library's test suites pass in C#**](NEWS.md#august-29-2026--over-90-of-the-standard-librarys-test-suites-pass-in-c) | `773afa2c2` · `d2da277f5` · `nuget-1.23.12.2` | **189/215** packages, 26,043 matching verdicts, 148 disclosed — **189/208 = 90.9%** against the implementable set; `net` aboard at 472 verdicts, `reflect` executing for the first time; 189 proof pages frozen for the 1.23.12.2 release. |
| 2026-09-07 | [**Go 1.23.12's record closes at its anchor; the corpus hops to Go 1.24**](NEWS.md#september-7-2026--the-go-12312-record-closes-at-its-anchor-the-corpus-hops-to-go-124) | `95daed007` | **204/215** packages, 28,459 matching verdicts, 167 disclosed — **204/209 = 97.6%** against the implementable set, frozen as the Go 1.23.12 anchor; the five rows still unbanked re-validate under Go 1.24.13, where a hop re-derives every row from scratch. |

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
commercial license, sponsorship of the remaining validation work, or an outright
acquisition that keeps the tool free for the community are all on the table. The
contact address is in [AUTHORS](https://github.com/ritchiecarroll/go2cs/blob/master/AUTHORS).
Contributions to the converter carry a Developer Certificate of Origin sign-off and a
relicensing grant, see [CONTRIBUTING.md](https://github.com/ritchiecarroll/go2cs/blob/master/CONTRIBUTING.md),
which is what keeps those options open. The go2cs name and logo identify this
project; a fork is welcome under the AGPL but should carry its own name.

See [LICENSING.md](https://github.com/ritchiecarroll/go2cs/blob/master/LICENSING.md) for the component matrix, exceptions and output
policy, and [NOTICE](https://github.com/ritchiecarroll/go2cs/blob/master/NOTICE) for attribution.
