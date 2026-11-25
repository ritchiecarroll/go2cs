# go2cs command-line options

Every option the converter accepts. The [common options](#options) are the few a first conversion needs, and
the [option reference](#option-reference) at the end of this page explains every option, common, advanced and
experimental. `go2cs -help` prints the common options, `go2cs -help all` lists every option, and
`go2cs -help <option>` prints one option's reference entry. The help and this page are written from one table,
so they say the same thing. [Converting a real-world Go module](ConvertingAModule.md) walks through a whole
conversion.

## Usage

```shell
go2cs [options] <input_dir> [output_dir]
```

Give the output directory whenever you convert a single package: without it the C# is written
beside the Go input.

Examples:

```shell
go2cs package_dir out                  # convert a package
go2cs -indent 2 -var=false package_dir out   # same, with 2-space indents and explicit types
go2cs -stdlib                          # convert the entire Go standard library
go2cs -stdlib fmt strings io           # convert specific standard library packages
go2cs -recurse=nuget module_dir out    # convert a module + its third-party deps, stdlib from NuGet
go2cs -recurse -go2cspath <gopath>/src/go2cs module_dir out   # same, against a standard library staged by deploy-core.ps1
go2cs -recurse=module,nuget module_dir out   # convert only the module's own packages (deps referenced, not converted)
go2cs -tests goroot_pkg_dir converted_pkg_dir   # convert a package plus its Go test suite
go2cs -tests -test-action all goroot_pkg_dir converted_pkg_dir   # ...and build, run, and diff vs go test
go2cs -tests -recurse -test-action all -go2cspath <go2cs_clone>/src module_dir out_root   # validate a whole module against its own tests
go2cs toolchain env                    # the Go toolchain go2cs converts with (see below)
```

## Options

The common options, with a short description each. Each name links to its full entry in the
[option reference](#option-reference).

<!-- BEGIN GENERATED common-options: written from src/go2cs/internal/clihelp/options.go by `go generate` in src/go2cs. Edit options.go, not this region; TestCommandLinePageInSync fails on a hand edit. -->
| Option | Description |
|:--|:--|
| [`-recurse[=module,nuget]`](#option-recurse) | Convert a module and its third-party dependencies (=module: the module only; =nuget: standard library from NuGet). |
| [`-go2cspath <dir>`](#option-go2cspath) | Where the go2cs standard library and runtime are when not from NuGet, and where -stdlib writes. |
| [`-platforms <os/arch>`](#option-platforms) | The target platform, os/arch (default: this machine's). |
| [`-tags <list>`](#option-tags) | Build tags applied when loading packages, comma-separated. |
| [`-comments`](#option-comments) | Carry the Go source comments into the C#. |
| [`-stdlib`](#option-stdlib) | For go2cs development: convert the Go standard library into the -go2cspath root. |
| [`-cgo`](#option-cgo) | Convert a cgo package whose C use go2cs can bind (needs a C compiler). |
| [`-tests`](#option-tests) | Also convert the package's Go tests and emit a runnable C# test host. |
| [`-test-action <action>`](#option-test-action) | With -tests: convert, build, run, compare (with go test) or all. |
| [`-version`](#option-version) | Print the converter's release tuple and exit. |
<!-- END GENERATED common-options -->

All converted C# code references a hand-written runtime library (`golib`, published as the [`go.lib`](https://www.nuget.org/packages/go.lib)
NuGet package) plus a set of Roslyn source generators that supply Go semantics at compile time (published as
[`go.gen`](https://www.nuget.org/packages/go.gen)). A `-recurse=nuget` conversion wires both up for you.

## The Go toolchain

The converter loads and type-checks Go with the Go toolchain. A release build, such as the
[go2cs dotnet tool](DotnetTool.md), downloads the Go release it converts with on first use when no Go of that
release is set up, and `go2cs toolchain` manages that copy:

```shell
go2cs toolchain install             # download it now
go2cs toolchain install -from FILE  # install it from a Go release archive or toolchain module zip
go2cs toolchain env                 # print GOROOT, GOVERSION and GO, the go command go2cs uses
go2cs toolchain prune [-all]        # remove copies of other Go releases (-all: this one too)
```

`GO2CS_GOROOT` names an existing install of that release to use instead. [The go2cs dotnet tool](DotnetTool.md)
has the whole order go2cs chooses a Go in.

## Diagnostics

A failure in the Go input itself is printed on stderr in MSBuild's canonical error format, one line per error, so CI logs
and editor problem matchers (for example VS Code's `$msCompile`) pick it up:

```
/src/app/main.go(3,16): error GO2CS1003: cannot use 1 (untyped int constant) as string value in variable declaration
```

Every code the converter can print, with what to do about it, is in the one table of
[diagnostic codes](DiagnosticCodes.md), beside the `GO2CS0xxx` codes `go2cs-gen` reports in the C# build.

A diagnostic with no source position names `go2cs` as its origin. A `WARNING:` or `Refusing to convert:` summary line
is printed before these lines.

## Option reference

Every option, with its full description: the common options first, then the advanced and experimental ones,
each class in the same group order `go2cs -help all` uses.

<!-- BEGIN GENERATED option-reference: written from src/go2cs/internal/clihelp/options.go by `go generate` in src/go2cs. Edit options.go, not this region; TestCommandLinePageInSync fails on a hand edit. -->
### Common options

What a first conversion needs: convert and build a program or module, or check a conversion against the package's Go tests. `go2cs -help` lists these.

#### <a id="option-recurse"></a>`-recurse[=module,nuget]`

Common option, converting.

Convert a downloaded module **and its third-party dependencies** in dependency order, referencing (not reconverting) the pre-converted standard library. **Plain `-recurse` references a standard library staged on this machine** by `deploy-core.ps1`, at `<gopath>/src/go2cs`, so pass `-go2cspath <gopath>/src/go2cs` with it; most conversions want `-recurse=nuget`, which needs nothing staged. An optional second positional sets the output root for the generated `src\` (app) and `pkg\` (dependency) trees. A package that fails to convert (it does not load, or the converter panics) is reported and skipped, and so is a package the converter **refuses** (any refusal error, listed in [Diagnostic codes](DiagnosticCodes.md)): its diagnostics print as errors, it gets no project, every other package still converts, and the run ends with exit 1, naming each failed and refused package and each converted package that imports a refused one (which will not build until the refused package converts). A code downgraded by `-warn` is not a refusal. A single-package conversion still stops at a refusal and writes nothing. `-tests -recurse` skips a refused package too and tests each package as `go test ./...` does: a tested package that is refused, or whose test closure imports a refused package, is named with that reason and gets no test project, the others are converted and tested, and the run ends with exit 1. Its conversion step also goes on past a failed package as before. The module is converted, and validated, as Go builds it with the build tags `safe,purego,noasm`, which the run prints at its start (see [Build tags](ConvertingAModule.md#build-tags-for-a-module)). See [Converting a real-world Go module](ConvertingAModule.md).

`-recurse=module`: Same, but convert only the module's own packages: third-party packages are referenced into `pkg\` but not converted, and are listed at the end of the run. It changes the scope only: alone it keeps plain `-recurse`'s local references, and `-recurse=module,nuget` gives NuGet ones. See [converting the module only](ConvertingAModule.md#optional-convert-the-module-only-and-deal-with-its-dependencies-later).

`-recurse=nuget`: Same, but the standard library, the `golib` runtime and the analyzer come from NuGet ([`go.<pkg>`](https://www.nuget.org/packages?q=go2cs%20ritchiecarroll), [`go.lib`](https://www.nuget.org/packages/go.lib), [`go.gen`](https://www.nuget.org/packages/go.gen)), so nothing is staged locally. Third-party modules that the [nugetgo.net](https://nugetgo.net) registry maps are referenced as published packages when a qualifying version exists, and converted locally otherwise (see [Mapping modules to NuGet packages](ConvertingAModule.md#mapping-modules-to-nuget-packages)). Scope and reference style combine: `-recurse=module,nuget`. The published packages match the Go release go2cs is built with; go2cs checks the module against that release first, and refuses a mismatch, naming both versions.

#### <a id="option-go2cspath"></a>`-go2cspath <dir>`

Common option, converting.

Runtime/stdlib root (env `GO2CSPATH`; default `~/go2cs`) used by generated `$(go2csPath)…` references, and the output root for `-stdlib`. `deploy-core.ps1` stages at `<gopath>/src/go2cs`, not at the default, so a conversion against its stage names it here. For a single-package conversion, C# output goes to optional `[output_dir]` (in place by default).

#### <a id="option-platforms"></a>`-platforms <os/arch>`

Common option, converting.

Target platform for build-tagged files (defaults to the host; a `-tests` run of a standard-library package defaults to the converted standard library's flavour for the host: the host's own platform when the library is converted for it (`windows/amd64`, `linux/amd64`, `linux/arm64`, `darwin/amd64`), otherwise the host's OS on `amd64`, so an Apple silicon host converts at `darwin/amd64`). A comma-separated **list** (`windows/amd64,linux/amd64,darwin/amd64,linux/arm64`) is accepted only with `-stdlib`, which then converts once per target and writes the multi-platform standard library, one flavour per target. Any other conversion emits for exactly one target, so a list there is rejected rather than silently converting the first.

#### <a id="option-tags"></a>`-tags <list>`

Common option, converting.

Build tags applied when loading packages. `-stdlib` and `-tests` apply `purego` and `math_big_pure_go` by default, and `-recurse` applies `safe`, `purego` and `noasm` (with `-tests`, `purego,math_big_pure_go,safe,noasm`). A `-stdlib` or `-recurse` run prints the tags it applies at its start. An explicit `-tags` replaces every default.

#### <a id="option-comments"></a>`-comments`

Common option, converting.

Carry source comments into the output (best effort, see [go/ast comment status](https://github.com/golang/go/issues/20744)).

#### <a id="option-stdlib"></a>`-stdlib`

Common option, converting.

For go2cs development: convert the Go standard library, or only the packages named after it (`go2cs -stdlib fmt strings`), into the `-go2cspath` root, as `core/<package>`. Converting a program does not need it: `-recurse=nuget` takes the standard library from NuGet. Convert into a root copied from a go2cs clone's `src/core`: in any other root the hand-written files are replaced by automatic conversions that compile and do not work. With no `-tags` it applies the build tags `purego,math_big_pure_go`, which select the portable Go implementations over the assembly ones; an explicit `-tags` replaces them, and `-tags=` applies none.

#### <a id="option-cgo"></a>`-cgo`

Common option, converting.

Converts a single cgo package. It loads the package with cgo on (`CGO_ENABLED=1`, which needs a C compiler) and converts it when every use of C it makes is one go2cs binds: calls of C functions a shared library defines ([Limitations](Limitations.md#cgo)). Any other use is refused by name (`GO2CS2001`, see [Diagnostic codes](DiagnosticCodes.md)), with its reason, before anything is written.<!-- 2026-11-23 .5 docs pass (P1), UX-10 follow-on: no cgo refusal text names a phase (src/go2cs/cgoRoute.go). --> It converts for the machine's own operating system and architecture, or for another `-platforms` target when `CC` names that target's C compiler (for example `zig cc -target <triple>`); without one, any other `-platforms` is refused, for a package with no C in it too. Not combinable with `-stdlib`. With `-tests`, it converts ONE package with its tests into a scratch root seeded from `src/core`, which the seed step marks with a `.go2cs-cgo-scratch-root` file at the root: a root without the marker, and an output inside a repository's `src/core` (symlinks resolved), are refused by name. The package's own test loads and the `go test` oracle run with cgo on, and every other package keeps its build without cgo. With `-recurse`, every package of the module outside the standard library loads with cgo on, and each cgo package converts this way. A package that imports `runtime/cgo` or `plugin` is refused by name (`GO2CS2001`); one that imports `net` or `os/user` uses their standard-library builds without cgo, as Go does with `CGO_ENABLED=0` ([Limitations](Limitations.md#cgo)). More of cgo is on the [Roadmap](Roadmap.md#cgo).<!-- the [cgo interop plan](PLAN-cgo-interop.md) lays out the bridge (internal record, unlinked 2026-10-26, OQ-1) -->

#### <a id="option-tests"></a>`-tests`

Common option, testing.

Also convert the package's `_test.go` suite and emit a runnable C# test-host project (default off). Forces `-comments` on. Like `-stdlib`, with no `-tags` it applies the build tags `purego,math_big_pure_go`. For one package it finds the go2cs tree above its output directory, so it works from a bare clone with no flags or environment setup. With plain `-recurse` it validates a whole module against its own tests: `go2cs -tests -recurse -go2cspath <go2cs_clone>/src module_dir out_root`, with the output root outside the module's source tree. The module form does not look for the go2cs tree, so name it with `-go2cspath`, as an absolute path (or `GO2CSPATH`). It takes plain `-recurse` only: `-tests` with `=module` or `=nuget` is refused; a module that needs a newer Go than the converted standard library is refused. See [Validate a converted test suite](BuildingGo2cs.md#validate-a-converted-test-suite).

#### <a id="option-test-action"></a>`-test-action <action>`

Common option, testing.

With `-tests`: one of `convert` (default), `build`, `run`, `compare`, or `all`. `convert` and `all` convert the package and its tests; `build` / `run` / `compare` act on the **existing** converted artifacts — validated against the test manifest's recorded input digest — without reconverting. `compare` (and `all`) runs both `go test -json -count=1` and the converted C# test host and diffs the terminal results by test name. On unix a `-tests` run handles SIGINT and SIGQUIT as `go test` does. Every child it starts, the converted host included, begins with both at the default disposition, even when the run was launched with them ignored (an async `cmd &`). A terminal Ctrl-C or Ctrl-\ stops the run: go2cs kills each running child's process group and exits 130.

#### <a id="option-version"></a>`-version`

Common option, information.

Print the converter's release tuple and exit 0, converting nothing. One `key=value` line per key, always in this order, every key present (`unknown` when a value cannot be resolved), so a bug report or a build script can paste or parse it: `go2cs.release` (the go2cs corpus release this binary belongs to, four numbers: the Go release, then the go2cs revision), `go2cs.converter` (this binary's identity, `exe-` plus the first 16 hex digits of its SHA-256 — the value test manifests record), `go.packages` (the `go.*` NuGet version `-recurse=nuget` defaults `$(GoStdLibVersion)` to: the Go minor with a float, for example `1.26.*`), `go.toolchain` (the active Go toolchain, `go env GOVERSION`), `go.build` (the Go toolchain that built this binary), `vcs.revision` and `vcs.modified` (the commit a binary built from a clone was built from, and whether its tree had local changes; Go does not stamp a binary built from a `git worktree` checkout, whose `.git` is a file rather than a directory, so that reads `unknown`). New keys are only ever appended.

### Advanced options

Options that change a default, tune a run, or drive a maintainer's instrument. `go2cs -help all` lists them with the common ones.

| Option | Group | Description |
|:--|:--|:--|
| [`-goroot <dir>`](#option-goroot) | Converting | Override the detected Go root. |
| [`-gopath <dir>`](#option-gopath) | Converting | Override the detected Go path. |
| [`-module-safe-tag`](#option-module-safe-tag) | Converting | With -recurse: apply the safe, purego and noasm build tags (default on). |
| [`-convert-timeout <duration>`](#option-convert-timeout) | Converting | With -stdlib: the cap on one package's conversion. |
| [`-allow-stale-converter`](#option-allow-stale-converter) | Converting | With -stdlib or -tests: run even when this binary is older than its sources. |
| [`-stack-append <mode>`](#option-stack-append) | Converting | Emit Go's stack-append buffer: on, off or auto. |
| [`-probe-cache <dir>`](#option-probe-cache) | Converting | The Go build cache the escape probe uses (env GO2CS_PROBE_CACHE). |
| [`-test-timeout <duration>`](#option-test-timeout) | Testing | With -tests: the package deadline for build, run and compare. |
| [`-test-deadline <duration>`](#option-test-deadline) | Testing | With -tests: the per-test deadline for the converted host (0 disables it). |
| [`-test-filter <regex>`](#option-test-filter) | Testing | With -tests: a regular expression both sides run, as go test -run. |
| [`-test-config <config>`](#option-test-config) | Testing | With -tests: build and run the test host as Release (default) or Debug. |
| [`-test-tiered`](#option-test-tiered) | Testing | With -test-config Release: turn the CLR's tiered JIT back on. |
| [`-test-build-maxcpu <n>`](#option-test-build-maxcpu) | Testing | With -tests: cap the test host's dotnet publish at n MSBuild nodes. |
| [`-test-publish-binlog`](#option-test-publish-binlog) | Testing | With -tests: keep an MSBuild binary log of a failed test-host publish. |
| [`-test-oracle-vet-off`](#option-test-oracle-vet-off) | Testing | With -tests: run go test with -vet=off. |
| [`-test-oracle-goarch <arch>`](#option-test-oracle-goarch) | Testing | With -tests: the GOARCH go test runs at, when it must differ from the target. |
| [`-cgo-removals-dry-run`](#option-cgo-removals-dry-run) | Testing | With -cgo -tests: list the cgo-off files a cgo-on conversion would remove, and remove none. |
| [`-test-allow-handown`](#option-test-allow-handown) | Testing | With -tests: convert a package -stdlib skips, into a bare scratch root. |
| [`-module-disclosures <dir>`](#option-module-disclosures) | Testing | With -tests on a module: the tree of committed disclosure manifests. |
| [`-module-version <version>`](#option-module-version) | Testing | With -module-disclosures: the version a copied module source is. |
| [`-nuget-map <source>`](#option-nuget-map) | NuGet mapping | With -recurse=nuget: add a module-to-package mapping source (repeatable; off disables mapping). |
| [`-nuget-map-only`](#option-nuget-map-only) | NuGet mapping | With -nuget-map: use only the listed sources, not nugetgo.net. |
| [`-nuget-map-exclude <module-path>`](#option-nuget-map-exclude) | NuGet mapping | With -recurse=nuget: never map this module (repeatable). |
| [`-nuget-map-refresh`](#option-nuget-map-refresh) | NuGet mapping | With -recurse=nuget: fetch every source again and re-resolve every module. |
| [`-nuget-map-canonical-only`](#option-nuget-map-canonical-only) | NuGet mapping | With -recurse=nuget: apply only the module owners' own mappings. |
| [`-nuget-map-feed <feed>`](#option-nuget-map-feed) | NuGet mapping | With -recurse=nuget: read mapped package versions from this feed, not nuget.org. |
| [`-indent <n>`](#option-indent) | Output | Spaces per indent level (default 4). |
| [`-var`](#option-var) | Output | Prefer var declarations where the type is obvious (default on). |
| [`-uco`](#option-uco) | Output | Emit channel operators instead of method calls (default on). |
| [`-hoist-literals`](#option-hoist-literals) | Output | Hoist string literals to named static fields, as the standard library does. |
| [`-csproj <file>`](#option-csproj) | Output | Generate project files from this .csproj template. |
| [`-license <expression>`](#option-license) | Output | Set the generated projects' NuGet license to an SPDX expression. |
| [`-provenance`](#option-provenance) | Output | Add a notice to each converted file naming its Go source and module. |
| [`-trimpath`](#option-trimpath) | Output | Record main-module sources as go build -trimpath does (default on). |
| [`-inline-arrays <pattern>[,<pattern>...]`](#option-inline-arrays) | Output | Override which packages' structs hold their arrays inline. |
| [`-warn <code>[,<code>...]`](#option-warn) | Output | Downgrade the listed converter refusal codes to warnings. |
| [`-platform-census <dir>`](#option-platform-census) | Multi-platform and census | With -stdlib and several -platforms: write a per-target emission census here. |
| [`-platform-stage <dir>`](#option-platform-stage) | Multi-platform and census | With -stdlib and several -platforms: stage the per-target conversions here. |
| [`-ref-census <file>`](#option-ref-census) | Multi-platform and census | With -stdlib: write the box-to-reference lowering census to this file. |
| [`-tree`](#option-tree) | Diagnostics | Print each file's Go parse tree during conversion. |
| [`-debug`](#option-debug) | Diagnostics | Let a conversion failure crash with a full stack trace. |

#### <a id="option-goroot"></a>`-goroot <dir>`

Advanced option, converting.

Override the detected Go root.

#### <a id="option-gopath"></a>`-gopath <dir>`

Advanced option, converting.

Override the detected Go path (GOPATH).

#### <a id="option-module-safe-tag"></a>`-module-safe-tag`

Advanced option, converting.

With `-recurse`: convert the module, and with `-tests` run its `go test` baseline, with the build tags `safe`, `purego` and `noasm`, so a library that ships a fallback for something converted code cannot do uses it: `safe` for unsafe pointer arithmetic into Go runtime internals, `purego` and `noasm` for hand-written assembly. On by default. `-module-safe-tag=false` converts and validates the module without them, using the files a plain `go build` selects; an explicit `-tags` overrides both. See [Build tags](ConvertingAModule.md#build-tags-for-a-module).

#### <a id="option-convert-timeout"></a>`-convert-timeout <duration>`

Advanced option, converting.

With `-stdlib`: the cap on one package's conversion, in Go duration syntax (default `10m`; it must be greater than zero). A package that runs past it stops the run: the command exits 1, and `failed_packages.txt` lists that package and every package not yet converted, so `-stdlib` with that list resumes. It guards against a hung conversion: a slow or busy machine can need a larger value.

#### <a id="option-allow-stale-converter"></a>`-allow-stale-converter`

Advanced option, converting.

With `-stdlib` or `-tests`: proceed even when this binary is older than the converter sources beside it. Both refuse otherwise, because a stale binary emits the previous converter's C# and still exits 0. It is meant for one deliberate case, a comparison against a preserved binary.

#### <a id="option-stack-append"></a>`-stack-append <mode>`

Advanced option, converting.

`on`, `off` or `auto`: whether the conversion reproduces Go's stack-append backing store. Since Go 1.25 an `append` to a slice that does not escape first fills a 32-byte buffer, which `cap()` after the first appends and `testing.AllocsPerRun` can observe. `on` emits that buffer (`var xᴷ = new stackAppend<T>();` and `x = append(xᴷ, x, v);`), reading which append sites qualify from `go build -gcflags=-m` once per package and target (see `-probe-cache`). `off` emits plain `x = append(x, v)` and runs no probe build. `auto`, the default, is `on` for `-stdlib` and `-tests`, whose converted tests compare against Go's own, and `off` for a plain or `-recurse` conversion, a single standard-library package converted without `-stdlib` included (use `-stdlib <package>` for the standard library's emission). The buffer is a heap array in golib, so `on` changes what a program can observe, not its speed or its allocations. See [the stack-append buffer](ConversionStrategies-Reference/slices-and-arrays.md#a-non-escaping-append-can-take-gos-32-byte-stack-buffer).

#### <a id="option-probe-cache"></a>`-probe-cache <dir>`

Advanced option, converting.

The Go build cache (`GOCACHE`) of the `go build` and `go test -c` runs that read the Go compiler's escape decisions for each converted package. They run only when `-stack-append` resolves `on`: for `-stdlib` and `-tests` by default, and for any other conversion only with `-stack-append=on`; with it off the cache is never created. Default: `go2cs/probe-cache` under the user cache directory (`~/.cache` on Linux, `%LocalAppData%` on Windows, `~/Library/Caches` on macOS), or the `GO2CS_PROBE_CACHE` environment variable. It is kept apart from your own `GOCACHE` because those runs build the standard library for four targets with flags no ordinary build uses. Go removes entries unused for five days, and deleting the directory is always safe: the next conversion rebuilds what it needs. See [Known issues](KnownIssues.md#a-conversion-keeps-a-go-build-cache-of-its-own).

#### <a id="option-test-timeout"></a>`-test-timeout <duration>`

Advanced option, testing.

Package deadline for a converted-test action, in Go duration syntax (default `2m`); `run` and `compare` give it to both `go test` and the converted host. The host's `dotnet publish` always gets at least `30m`, because the first publish on a fresh tree builds the whole standard-library closure. A suite that runs long in C# needs a larger value: `hash/maphash` is validated with `-test-timeout 60m`.

#### <a id="option-test-deadline"></a>`-test-deadline <duration>`

Advanced option, testing.

Per-test deadline for the CONVERTED host under `run` and `compare`, in Go duration syntax (default `15m`; `0` turns it off). A top-level test still running this long after it started is reported `fail`, marked abandoned, and the host goes on to the next test, so one hung test does not hold the host to `-test-timeout` and leave every later test unmeasured. The comparison labels such a row "abandoned: did not finish within the per-test deadline", and every later event names the abandoned test (`afterAbandoned`), since its goroutines keep running. The Go side is unbounded, as `go test` is. It reaches the host as `GO2CS_TEST_DEADLINE`, never on argv. A package whose own test runs longer than the default needs a larger value; the repository's validation sweep (`src/run-validated-sweep.ps1`) sets one per package.

#### <a id="option-test-filter"></a>`-test-filter <regex>`

Advanced option, testing.

With `-tests`: a regular expression passed unchanged to `go test -run` and to the converted host under `run` and `compare`, so both sides run the same tests. A filtered run is for diagnosis: it writes no proof pages, and a validated result comes from an unfiltered run.

#### <a id="option-test-config"></a>`-test-config <config>`

Advanced option, testing.

With `-tests`: the configuration the converted test host is published and run in, `Release` (the default) or `Debug`. It is recorded on every proof page and in the comparison record.

#### <a id="option-test-tiered"></a>`-test-tiered`

Advanced option, testing.

With `-test-config Release`: turn the CLR's tiered JIT back on. `Release` runs the host with it off by default, so a verdict does not depend on when the JIT promotes a method. It has no effect with `Debug`, and it changes what the host's JIT does, not what the converter emits. A program you ship keeps tiering on: see [Tuning converted code](Tuning.md#net-runtime-settings).

#### <a id="option-test-build-maxcpu"></a>`-test-build-maxcpu <n>`

Advanced option, testing.

With `-tests`: cap the MSBuild nodes of the converted test host's `dotnet publish` by passing it `-m:<n>`. That publish is the only `dotnet` command a `-tests` run starts, so this caps every build the pipeline does. `0` (the default) passes no `-m`, and MSBuild uses one node per logical processor. Only an explicit `-m` caps it: a `Directory.Build.rsp` in the tree or `MSBUILDNODECOUNT` does not. The validation sweep (`src/run-validated-sweep.ps1`) passes it to every package's conversion with `-TestBuildMaxCpu <n>` (default `0`, which passes nothing).

#### <a id="option-test-publish-binlog"></a>`-test-publish-binlog`

Advanced option, testing.

With `-tests`: have the test host's `dotnet publish` write an MSBuild binary log to `<output>/bin/tests/publish.binlog`, kept only when the publish fails. It slows the publish; use it to chase a build failure.

#### <a id="option-test-oracle-vet-off"></a>`-test-oracle-vet-off`

Advanced option, testing.

With `-tests`: run `go test` with `-vet=off`, for a package whose own `go test` fails at the vet checks it runs before any test, rather than at a test. For example, `github.com/pmezard/go-difflib` v1.0.0 has two `Example` functions that vet's `tests` check rejects. Off by default, and set per run, so per module. The run first reads what those checks report with `go vet -json`, and refuses the option for a package they pass. Each finding is stated, by check, on the package's proof page and in the module's `MODULE.md`. The converted tests are still compared one by one with the `-vet=off` run.

#### <a id="option-test-oracle-goarch"></a>`-test-oracle-goarch <arch>`

Advanced option, testing.

With `-tests`: the `GOARCH` that `go test` runs at, when it must differ from the conversion's target. For example, an Apple silicon host validating the `darwin/amd64` conversion sets `arm64`, so Go does not run under Rosetta. Empty by default, which is the target's own; when set, it is recorded in the comparison record.

#### <a id="option-cgo-removals-dry-run"></a>`-cgo-removals-dry-run`

Advanced option, testing.

With `-cgo -tests`, lists the files a cgo-on conversion would remove from its output, and removes none. An output seeded from the corpus holds the package's build without cgo, and where the cgo-on build swaps a file for another (`net`'s `cgo_stub.go` for `cgo_unix.go`), both would compile. Without this flag each such file is removed and printed with its reason, but only when it carries go2cs's own emission header for its source; any other file there, and a hand-owned one, is refused by name.

#### <a id="option-test-allow-handown"></a>`-test-allow-handown`

Advanced option, testing.

With `-tests`: convert a package that `-stdlib` deliberately skips (`testing`, `unsafe`, `builtin`, `cmd/...`) anyway. Refused by default, because the natural output path for `testing` is the hand-written test host itself, and a mistyped command overwrites it. Use it only with an empty scratch output root: the output is for a measurement, not for use.

#### <a id="option-module-disclosures"></a>`-module-disclosures <dir>`

Advanced option, testing.

With `-tests` on a third-party module: the committed tree of disclosure manifests (the tests a module's own validation excludes, each with its reason), laid out like the module cache (`<dir>/<module path>@<version>/<package dir>/go2cs_test_disclosures.json`; the repository keeps it at `src/tests/ModuleDisclosures`). A manifest there is read only when the package's output directory holds none, and only for the module version this run resolved; a manifest kept for another version is not applied, and the run prints one line naming both. The version comes from the module-cache directory the module was converted from, or from `-module-version`; when it can be found from neither, the run is refused before any conversion. Takes effect only with `-tests -recurse`, and is refused otherwise.

#### <a id="option-module-version"></a>`-module-version <version>`

Advanced option, testing.

With `-tests -module-disclosures` on a module source that is not a module-cache directory, such as a copy of the module: the version the copy is (e.g. `v1.62.0`). A copy carries no version of its own. Refused when it is not a version of the module, when it contradicts the module-cache directory's own version, when the module cache holds that version and its `go.mod` differs from the copy's, when the disclosure tree keeps the module under other versions only, and without `-module-disclosures`. A version taken from this flag is CLAIMED, not resolved: the run's line, its comparison record (`moduleVersionClaimed`) and its proof page say so.

#### <a id="option-nuget-map"></a>`-nuget-map <source>`

Advanced option, nuget mapping.

With `-recurse=nuget`: add a mapping source, a local file or an `https://` URL. Repeatable; the first source that names a module wins, and nugetgo.net answers the rest. `-nuget-map off` turns mapping off. See [Mapping modules to NuGet packages](ConvertingAModule.md#mapping-modules-to-nuget-packages).

#### <a id="option-nuget-map-only"></a>`-nuget-map-only`

Advanced option, nuget mapping.

With `-nuget-map`: use **only** the listed sources — the nugetgo.net fallback is dropped, so a module your sources do not name stays local, and nugetgo.net is never fetched.

#### <a id="option-nuget-map-exclude"></a>`-nuget-map-exclude <module-path>`

Advanced option, nuget mapping.

With `-recurse=nuget`: never map this module, whatever a source says. Repeatable.

#### <a id="option-nuget-map-refresh"></a>`-nuget-map-refresh`

Advanced option, nuget mapping.

With `-recurse=nuget`: fetch every URL source unconditionally (bypassing the local cache) and re-resolve, adopting what the sources say now instead of what `go2cs.nuget.lock` pinned.

#### <a id="option-nuget-map-canonical-only"></a>`-nuget-map-canonical-only`

Advanced option, nuget mapping.

With `-recurse=nuget`: apply only **canonical** mappings (the module owner's own conversion); a community mapping is treated as unmapped.

#### <a id="option-nuget-map-feed"></a>`-nuget-map-feed <feed>`

Advanced option, nuget mapping.

With `-recurse=nuget`: read mapped packages' versions from this feed **instead of** nuget.org: an `https://` NuGet v3 service index (a test gallery such as int.nugettest.org, for trying packages before they are published) or a local folder of `.nupkg` files. A rehearsal version (`nugetgo-pack.ps1 -RehearsalSuffix`) is admitted only from it. The restore reads your own package sources, so add the feed there too.

#### <a id="option-indent"></a>`-indent <n>`

Advanced option, output.

Spaces per indent level (default 4).

#### <a id="option-var"></a>`-var`

Advanced option, output.

Prefer `var` declarations where the type is obvious (default on).

#### <a id="option-uco"></a>`-uco`

Advanced option, output.

Emit channel operators instead of method calls (default on).

#### <a id="option-hoist-literals"></a>`-hoist-literals`

Advanced option, output.

Hoist mode, the standard library's: a string literal in a value slot whose words give it a meaningful name becomes one `static readonly` field (`F(helloˢ)`, and pre-boxed in an `any` slot); every other site keeps the default plain form (`F("hello")`, converted once and shared by golib; in an `any` slot, boxed by a go2cs-gen interceptor), so no literal is copied on each evaluation. The standard library always hoists; this opts any other package in. It is recommended for performance-critical code: a hoisted field is a plain static read, where a plain literal makes a cache lookup on every evaluation. It is also recommended for a large project that is IDE-heavy, because go2cs-gen re-binds every `any`-slot literal call on each edit. See [plain literals](ConversionStrategies-Reference/strings.md#a-literal-in-a-string-slot-is-a-plain-c-literal) and [their design-time cost](ConversionStrategies-Reference/strings.md#a-literal-in-an-any-slot-is-a-plain-c-literal-too-and-go2cs-gen-boxes-it-as-a-go-string). The measured run-time difference, and when it matters: [Tuning converted code](Tuning.md#string-literals).

#### <a id="option-csproj"></a>`-csproj <file>`

Advanced option, output.

Generate project files from a custom `.csproj` template instead of the embedded one.

#### <a id="option-license"></a>`-license <expression>`

Advanced option, output.

Set the generated projects' NuGet license metadata to an SPDX expression, for example `MIT` or `BSD-3-Clause OR MIT`. It does not relicense the input code. See [LICENSING.md](https://github.com/ritchiecarroll/go2cs/blob/master/LICENSING.md).

#### <a id="option-provenance"></a>`-provenance`

Advanced option, output.

Add to each converted file a two-line notice naming its Go source file and module and stating that go2cs changed it: `// Converted from Go source: "doc/md_docs.go" in module github.com/spf13/cobra@v1.10.2.`, then `// Modified from the original by conversion to C# with go2cs <release>.` It goes after the file's upstream copyright and license comments, before the package doc comment. The notice carries no timestamp, so the output stays the same from run to run. A module whose license asks modified files to say so (Apache-2.0) is packed with it ([LICENSING.md](../LICENSING.md)).

#### <a id="option-trimpath"></a>`-trimpath`

Advanced option, output.

On by default: a converted position map records a main-module source the way `go build -trimpath` bakes it, as `<module path>/<path in module>`, so a converted stack trace names the file in that form and the output names no directory of this machine. `-trimpath=false` records the absolute source path, as an untrimmed `go build` does. A standard-library or module-cache source is recorded the same way either way.

#### <a id="option-inline-arrays"></a>`-inline-arrays <pattern>[,<pattern>...]`

Advanced option, output.

Override which struct types hold their Go arrays INLINE (the `go.arrays` family, so a struct read from native memory or reinterpreted through `unsafe.Pointer` is Go's bytes) rather than as golib's `array<T>`. With no override the rule is automatic: a struct is admitted when its own package's non-test files convert it whole through `unsafe.Pointer`, together with every struct it holds by value, each within the caps (4096 bytes and 4096 elements per array, 4096 bytes per struct), unless those files use one of its arrays in a way an inline array cannot express (sliced, its address or an address inside it taken, or used as a whole value other than as an interface argument). A pattern (an import path, or a `/...` prefix) forces its packages ON with every struct they declare or spell that holds an array; `-<pattern>` turns the rule OFF for them. An entry `<import path>#<TypeName>` seeds ONE type instead: that package admits the named type or alias as if its own files converted it whole through `unsafe.Pointer`, under the automatic rule (the same caps, the same use check, the same `GO2CS2009`), and nothing else in the package changes. Use it for a type that another package converts whole and passes back by value. A `-recurse` conversion of `modernc.org/sqlite` for Windows derives the seed `modernc.org/libc#largeInteger` itself; converting `modernc.org/libc` on its own for Windows needs `-inline-arrays=modernc.org/libc#largeInteger` ([known issue](KnownIssues.md#converting-modernc-sqlite-for-windows-needs-a-seed-for-libcs-largeinteger)).<!-- 2026-11-23 .5 docs pass (P1): the derived seed landed in merge 00b0dc37dd (KnownIssues.md, same pass); 1.26.11.1 still needs the flag for the -recurse conversion too, as that entry says. --> A name the package does not declare selects nothing, so one value serves every target. A `-recurse` run also derives seeds itself: an import is seeded with each alias whose identical anonymous struct an importer admits ([how](ConversionStrategies-Reference/slices-and-arrays.md#a-struct-read-from-native-memory-holds-its-arrays-inline-and-seeds-make-two-packages-agree)); a seed named here and derived there is one seed, and `-<pattern>` beats both. The standard library is never admitted. A selected type a cap keeps out converts as before with warning `GO2CS2009`. A reinterpret chain whose two sides disagree is made to agree under the automatic rule, by keeping the admitted side out with its partner (also `GO2CS2009`), and is refused with `GO2CS2010` when the package is forced or the admitted side belongs to another package. The decision is recorded in the package's `package_info.cs`: a `-tests` conversion applies the recorded rule to the package's non-test files, so the option need not be repeated there, and refuses a record those files do not reproduce (the named types, a count and digest of every admitted type, anonymous ones included, a digest of each admitted anonymous type, and the seeds). A `-tests` conversion of a package that has no record while `-inline-arrays` names it is refused too: the rule its production output was converted under cannot be known. See [diagnostic codes](DiagnosticCodes.md).

#### <a id="option-warn"></a>`-warn <code>[,<code>...]`

Advanced option, output.

Downgrade the listed converter refusals to warnings, so a package that holds a refused site still converts and the rest of it runs. Opt-in per code, never a default. A code is allowed only when every site it covers fails LOUD when reached: the converted program panics there with the refusal text, and never runs on with a silently wrong result. Today that is `GO2CS2002` alone (a self-referential constraint proxy method that cannot forward its storage-sharing value). Each downgraded site is printed as `warning GO2CS2002: ...` (exit 0) and recorded in package_info.cs, and every C# build of the project then reports it as `GO2CS0011`, so a project nobody converts again still shows it carries a known-broken path. Any other code, an unknown one or a refusal that cannot fail loud, is an error that names it and the reason. See [diagnostic codes](DiagnosticCodes.md).

#### <a id="option-platform-census"></a>`-platform-census <dir>`

Advanced option, multi-platform and census.

With `-stdlib` and two or more `-platforms` targets: convert once per target into an isolated, seeded staging root under `<dir>`, compare what each run actually emitted, and write `<dir>\platform-manifest.json` classifying every artifact as shared, variant, partial or platform-exclusive. Produces **no** converted output of its own — `-go2cspath` is read as the seed and never written to.

#### <a id="option-platform-stage"></a>`-platform-stage <dir>`

Advanced option, multi-platform and census.

With `-stdlib` and two or more `-platforms` targets: the directory the per-target conversions are staged in, kept afterwards for inspection. Without it, the run stages in `.go2cs-platform-stage` under the output root and removes it at the end.

#### <a id="option-ref-census"></a>`-ref-census <file>`

Advanced option, multi-platform and census.

With `-stdlib`: run an analysis-only census of how the converter would lower heap boxes to references, over the standard library and once per `-platforms` target, and write its JSON report to this file. It emits no C#.

#### <a id="option-tree"></a>`-tree`

Advanced option, diagnostics.

Print each file's Go parse tree (`go/ast`) to stdout during conversion — a diagnostic aid.

#### <a id="option-debug"></a>`-debug`

Advanced option, diagnostics.

Disable the converter's per-file panic recovery, so a conversion failure crashes with a full stack trace, instead of being recovered, reported and skipped (the run then ends with `GO2CS2008`, naming the file).

### Experimental options

Options that make output that is not validated.

| Option | Group | Description |
|:--|:--|:--|
| [`-dual-recv`](#option-dual-recv) | Development | Also emit by-reference forms of eligible pointer-receiver methods. |
| [`-dual-recv-params`](#option-dual-recv-params) | Development | With -dual-recv: those forms also lower their pointer parameters. |

#### <a id="option-dual-recv"></a>`-dual-recv`

Experimental option, development.

Experimental, for converter development: eligible pointer-receiver methods also emit a form that takes the receiver by reference, beside the default form. Off by default; output made with it is not validated.

#### <a id="option-dual-recv-params"></a>`-dual-recv-params`

Experimental option, development.

Experimental, requires `-dual-recv`: those forms also lower their pointer parameters, and call sites choose between the two forms. Off by default.

<!-- END GENERATED option-reference -->
