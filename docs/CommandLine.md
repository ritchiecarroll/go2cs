# go2cs command-line options

Every option the converter accepts. The [README](README.md#usage) shows the few that most conversions
need, and [Converting a real-world Go module](ConvertingAModule.md) walks through a whole conversion.

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
```

## Options

| Option | Description |
|:--|:--|
| `-stdlib` | Convert the Go standard library (optionally followed by specific package names). |
| `-convert-timeout <duration>` | With `-stdlib`: the cap on one package's conversion, in Go duration syntax (default `10m`; it must be greater than zero). A package that runs past it is reported as a failed package. It guards against a hung conversion: a slow or busy machine can need a larger value. |
| `-recurse` | Convert a downloaded module **and its third-party dependencies** in dependency order, referencing (not reconverting) the pre-converted standard library. **Plain `-recurse` references a standard library staged on this machine** by `deploy-core.ps1`, at `<gopath>/src/go2cs`, so pass `-go2cspath <gopath>/src/go2cs` with it; most conversions want `-recurse=nuget`, which needs nothing staged. An optional second positional sets the output root for the generated `src\` (app) and `pkg\` (dependency) trees. A package that fails to convert is reported and skipped. The module is converted, and validated, as Go builds it with the `safe` build tag (see [Build tags](ConvertingAModule.md#build-tags-for-a-module)). See [Converting a real-world Go module](ConvertingAModule.md). |
| `-recurse=module` | Same, but convert only the module's own packages: third-party packages are referenced into `pkg\` but not converted, and are listed at the end of the run. It changes the scope only: alone it keeps plain `-recurse`'s local references, and `-recurse=module,nuget` gives NuGet ones. See [converting the module only](ConvertingAModule.md#optional-convert-the-module-only-and-deal-with-its-dependencies-later). |
| `-recurse=nuget` | Same, but the standard library, the `golib` runtime and the analyzer come from NuGet ([`go.<pkg>`](https://www.nuget.org/packages?q=go2cs%20ritchiecarroll), [`go.lib`](https://www.nuget.org/packages/go.lib), [`go.gen`](https://www.nuget.org/packages/go.gen)), so nothing is staged locally. Third-party modules that the [nugetgo.net](https://nugetgo.net) registry maps are referenced as published packages when a qualifying version exists, and converted locally otherwise (see [Mapping modules to NuGet packages](ConvertingAModule.md#mapping-modules-to-nuget-packages)). Scope and reference style combine: `-recurse=module,nuget`. The published packages match the Go release go2cs is built with; go2cs checks the module against that release first, and refuses a mismatch, naming both versions. |
| `-module-safe-tag=false` | With `-recurse`: convert and validate the module without the `safe` build tag, using the files a plain `go build` selects. On by default; see [Build tags](ConvertingAModule.md#build-tags-for-a-module). |
| `-nuget-map <source>` | With `-recurse=nuget`: add a mapping source, a local file or an `https://` URL. Repeatable; the first source that names a module wins, and nugetgo.net answers the rest. `-nuget-map off` turns mapping off. See [Mapping modules to NuGet packages](ConvertingAModule.md#mapping-modules-to-nuget-packages). |
| `-nuget-map-only` | With `-nuget-map`: use **only** the listed sources — the nugetgo.net fallback is dropped, so a module your sources do not name stays local, and nugetgo.net is never fetched. |
| `-nuget-map-exclude <module-path>` | With `-recurse=nuget`: never map this module, whatever a source says. Repeatable. |
| `-nuget-map-refresh` | With `-recurse=nuget`: fetch every URL source unconditionally (bypassing the local cache) and re-resolve, adopting what the sources say now instead of what `go2cs.nuget.lock` pinned. |
| `-nuget-map-canonical-only` | With `-recurse=nuget`: apply only **canonical** mappings (the module owner's own conversion); a community mapping is treated as unmapped. |
| `-nuget-map-feed <feed>` | With `-recurse=nuget`: read mapped packages' versions from this feed **instead of** nuget.org: an `https://` NuGet v3 service index (a rehearsal gallery such as int.nugettest.org) or a local folder of `.nupkg` files. A rehearsal version (`nugetgo-pack.ps1 -RehearsalSuffix`) is admitted only from it. The restore reads your own package sources, so add the feed there too. |
| `-tests` | Also convert the package's `_test.go` suite and emit a runnable C# test-host project (default off). Forces `-comments` on. For one package it finds the go2cs tree above its output directory, so it works from a bare clone with no flags or environment setup. With plain `-recurse` it validates a whole module against its own tests: `go2cs -tests -recurse -go2cspath <go2cs_clone>/src module_dir out_root`, with the output root outside the module's source tree. The module form does not look for the go2cs tree, so name it with `-go2cspath`, as an absolute path (or `GO2CSPATH`). It takes plain `-recurse` only: `-tests` with `=module` or `=nuget` is refused; a module that needs a newer Go than the converted standard library is refused. See [Validate a converted test suite](BuildingGo2cs.md#validate-a-converted-test-suite). |
| `-test-action <action>` | With `-tests`: one of `convert` (default), `build`, `run`, `compare`, or `all`. `convert` and `all` convert the package and its tests; `build` / `run` / `compare` act on the **existing** converted artifacts — validated against the test manifest's recorded input digest — without reconverting. `compare` (and `all`) runs both `go test -json -count=1` and the converted C# test host and diffs the terminal results by test name. |
| `-test-timeout <duration>` | Package deadline for a converted-test action, in Go duration syntax (default `2m`); `run` and `compare` give it to both `go test` and the converted host. The host's `dotnet publish` always gets at least `30m`, because the first publish on a fresh tree builds the whole standard-library closure. A suite that runs long in C# needs a larger value: `hash/maphash` is validated with `-test-timeout 60m`. |
| `-test-deadline <duration>` | Per-test deadline for the CONVERTED host under `run` and `compare`, in Go duration syntax (default `15m`; `0` turns it off). A top-level test still running this long after it started is reported `fail`, marked abandoned, and the host goes on to the next test, so one hung test does not hold the host to `-test-timeout` and leave every later test unmeasured. The comparison labels such a row "abandoned: did not finish within the per-test deadline", and every later event names the abandoned test (`afterAbandoned`), since its goroutines keep running. The Go side is unbounded, as `go test` is. It reaches the host as `GO2CS_TEST_DEADLINE`, never on argv. A package whose own test runs longer than the default needs a larger value; the repository's validation sweep (`src/run-validated-sweep.ps1`) sets one per package. |
| `-test-config <config>` | With `-tests`: the configuration the converted test host is published and run in, `Release` (the default) or `Debug`. It is recorded on every proof page and in the comparison record. |
| `-test-tiered` | With `-test-config Release`: turn the CLR's tiered JIT back on. `Release` runs the host with it off by default, so a verdict does not depend on when the JIT promotes a method. It has no effect with `Debug`, and it changes what the host's JIT does, not what the converter emits. A program you ship keeps tiering on: see [Tuning converted code](Tuning.md#net-runtime-settings). |
| `-test-filter <regex>` | With `-tests`: a regular expression passed unchanged to `go test -run` and to the converted host under `run` and `compare`, so both sides run the same tests. A filtered run is for diagnosis: it writes no proof pages, and a validated result comes from an unfiltered run. |
| `-test-build-maxcpu <n>` | With `-tests`: cap the MSBuild nodes of the converted test host's `dotnet publish` by passing it `-m:<n>`. That publish is the only `dotnet` command a `-tests` run starts, so this caps every build the pipeline does. `0` (the default) passes no `-m`, and MSBuild uses one node per logical processor. Only an explicit `-m` caps it: a `Directory.Build.rsp` in the tree or `MSBUILDNODECOUNT` does not. The validation sweep (`src/run-validated-sweep.ps1`) passes it to every package's conversion with `-TestBuildMaxCpu <n>` (default `0`, which passes nothing). |
| `-module-disclosures <dir>` | With `-tests` on a third-party module: the committed tree of module rows' disclosure manifests, laid out like the module cache (`<dir>/<module path>@<version>/<package dir>/go2cs_test_disclosures.json`; the repository keeps it at `src/tests/ModuleDisclosures`). A manifest there is read only when the package's output directory holds none, and only for the module version this run resolved; a manifest kept for another version is not applied, and the run prints one line naming both. |
| `-go2cspath <dir>` | Runtime/stdlib root (env `GO2CSPATH`; default `~/go2cs`) used by generated `$(go2csPath)…` references, and the output root for `-stdlib`. `deploy-core.ps1` stages at `<gopath>/src/go2cs`, not at the default, so a conversion against its stage names it here. For a single-package conversion, C# output goes to optional `[output_dir]` (in place by default). |
| `-goroot` / `-gopath` | Override the detected Go root / path. |
| `-platforms <os/arch>` | Target platform for build-tagged files (defaults to the host; a `-tests` run of a standard-library package defaults to the host's OS on `amd64`, the architecture of the converted standard library). A comma-separated **list** (`windows/amd64,linux/amd64,darwin/amd64`) is accepted only with `-stdlib`, which then converts once per target and writes the multi-platform standard library, one GOOS per target. Any other conversion emits for exactly one target, so a list there is rejected rather than silently converting the first. |
| `-platform-census <dir>` | With `-stdlib` and two or more `-platforms` targets: convert once per target into an isolated, seeded staging root under `<dir>`, compare what each run actually emitted, and write `<dir>\platform-manifest.json` classifying every artifact as shared, variant, partial or platform-exclusive. Produces **no** converted output of its own — `-go2cspath` is read as the seed and never written to. |
| `-tags <list>` | Build tags applied when loading packages. `-stdlib` and `-tests` apply `purego` and `math_big_pure_go` by default, and `-recurse` adds `safe`. An explicit `-tags` replaces every default. |
| `-indent <n>` | Spaces per indent level (default 4). |
| `-var` | Prefer `var` declarations where the type is obvious (default on). |
| `-hoist-literals` | Hoist each string literal in a value slot to one `static readonly` field (`F(helloˢ)`, and pre-boxed in an `any` slot), the standard library's form, instead of the default plain C# literal (`F("hello")`, converted once and shared by golib; in an `any` slot, boxed by a go2cs-gen interceptor). The standard library always hoists; this opts any other package in. It is recommended for performance-critical code: a hoisted field is a plain static read, where a plain literal makes a cache lookup on every evaluation. It is also recommended for a large project that is IDE-heavy, because go2cs-gen re-binds every `any`-slot literal call on each edit. See [plain literals](ConversionStrategies-Reference/strings.md#a-literal-in-a-string-slot-is-a-plain-c-literal) and [their design-time cost](ConversionStrategies-Reference/strings.md#a-literal-in-an-any-slot-is-a-plain-c-literal-too-and-go2cs-gen-boxes-it-as-a-go-string). What it costs at run time: [Tuning converted code](Tuning.md). |
| `-uco` | Emit channel operators instead of method calls (default on). |
| `-comments` | Carry source comments into the output (best effort, see [go/ast comment status](https://github.com/golang/go/issues/20744)). |
| `-csproj <file>` | Generate project files from a custom `.csproj` template instead of the embedded one. |
| `-license <expression>` | Set the generated projects' NuGet license metadata to an SPDX expression, for example `MIT` or `BSD-3-Clause OR MIT`. It does not relicense the input code. See [LICENSING.md](https://github.com/ritchiecarroll/go2cs/blob/master/LICENSING.md). |
| `-provenance` | Add to each converted file a comment naming its Go source file (`// Converted from Go source: "…"`). The comment carries no timestamp, so the output stays the same from run to run. |
| `-tree` | Print each file's Go parse tree (`go/ast`) to stdout during conversion — a diagnostic aid. |
| `-debug` | Disable the converter's per-file panic recovery, so a conversion failure crashes with a full stack trace instead of being reported as a warning. |
| `-version` | Print the converter's release tuple and exit 0, converting nothing. One `key=value` line per key, always in this order, every key present (`unknown` when a value cannot be resolved), so a bug report or a build script can paste or parse it: `go2cs.release` (the go2cs corpus release this binary belongs to, four numbers: the Go release, then the go2cs revision), `go2cs.converter` (this binary's identity, `exe-` plus the first 16 hex digits of its SHA-256 — the value test manifests record), `go.packages` (the `go.*` NuGet version `-recurse=nuget` defaults `$(GoStdLibVersion)` to: the Go minor with a float, for example `1.26.*`), `go.toolchain` (the active Go toolchain, `go env GOVERSION`), `go.build` (the Go toolchain that built this binary), `vcs.revision` and `vcs.modified` (the commit a binary built from a clone was built from, and whether its tree had local changes; Go does not stamp a binary built from a `git worktree` checkout, whose `.git` is a file rather than a directory, so that reads `unknown`). New keys are only ever appended. |
| ~~`-cgo`~~ | ~~Also convert cgo-targeted files.~~ Not yet functional, but planned: a P/Invoke-backed bridge for `import "C"` is on the [Roadmap](Roadmap.md#cgo), and this flag comes alive with it.<!-- the [cgo interop plan](PLAN-cgo-interop.md) lays out the bridge (internal record, unlinked 2026-10-26, OQ-1) --> |

All converted C# code references a hand-written runtime library (`golib`, published as the [`go.lib`](https://www.nuget.org/packages/go.lib)
NuGet package) plus a set of Roslyn source generators that supply Go semantics at compile time (published as
[`go.gen`](https://www.nuget.org/packages/go.gen)). A `-recurse=nuget` conversion wires both up for you.

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
