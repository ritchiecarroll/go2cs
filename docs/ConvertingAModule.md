# Converting a real-world Go module

The `-recurse` option converts a **whole downloaded application together with every third-party dependency
package** in its transitive import closure — in dependency order (least-dependencies-first) — while
**referencing** (not reconverting) the pre-converted standard library. The result is a C# solution you can
open and build. With `-recurse=nuget` the standard library, the `golib` runtime and the `go2cs-gen`
analyzer come from [nuget.org](https://www.nuget.org/packages?q=go2cs%20ritchiecarroll), so nothing has to
be staged on the machine beforehand. (Prefer the standard library as local C# source? See
[building against a local standard library](#optional-build-against-a-local-standard-library) below.)

Wondering which real-world Go packages make good conversions? The
**[go2cs Target Atlas](https://go2cs.net/TargetAtlas.html)** ranks the most-depended-on Go modules and
grades how hard each one is to convert.

## Step by step

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

> **NOTE:** _go2cs's type-checker reads only modules whose `go` directive (and their dependencies') is no newer than the Go release go2cs is built with (see [Requirements](README.md#requirements)). Newer `fatih/color` and `golang.org/x/sys` releases need a newer Go, which would fail step 2 with_ `package requires newer Go version`_; pin as shown._
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

**2 — go2cs: recurse-convert the app.** `go2cs` is the converter you put on your `PATH` when you
[installed it](README.md#installing-the-converter), so it runs from anywhere. Point it at the **app** directory, and give it an output root
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
> macOS (Intel and Apple silicon): packages ship for both chips, the behavioral suite passes on both, and this
> walkthrough runs. The standard library is being validated package by package on macOS; a
> [roster](ValidatedTestPackages.md) row shows a macOS count once its package validates there, so don't expect a
> package without one to be fully operational on macOS yet. Other platforms and architectures are tracked in the Roadmap's
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

## Build tags for a module

go2cs converts a module, and validates it against its own tests, as Go builds it with the `safe` build tag.
Some libraries reach into Go's runtime internals with unsafe pointer arithmetic, which converted code cannot
do, and ship a fallback for builds tagged `safe`; with the tag, that fallback is the code you get. The visible
difference is small: testify's assertion messages, for example, do not print the unexported fields of a struct.
When a module is validated, Go's own `go test` run uses the same tag, so both sides are compared on the same
code. To convert the files a plain `go build` selects instead, pass `-module-safe-tag=false`.

## Optional: convert the module only, and deal with its dependencies later

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

## Optional: build against a local standard library

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

## Mapping modules to NuGet packages

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
- `-nuget-map-only`, `-nuget-map-exclude`, `-nuget-map-refresh` and `-nuget-map-canonical-only` are listed under
  [Command-line options](CommandLine.md#options). Every `-nuget-map*` flag needs `-recurse=nuget`.
