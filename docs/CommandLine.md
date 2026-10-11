# go2cs command-line options

Every option the converter accepts. The [README](README.md#usage) shows the few that most conversions
need, and [Converting a real-world Go module](ConvertingAModule.md) walks through a whole conversion.

## Usage

```shell
go2cs [options] <input_dir> [output_dir]
```

Give the output directory whenever you convert a single file or package: without it the C# is written
beside the Go input.

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

## Options

| Option | Description |
|:--|:--|
| `-stdlib` | Convert the Go standard library (optionally followed by specific package names). |
| `-recurse` | Convert a downloaded module **and its third-party dependencies** in dependency order, referencing (not reconverting) the pre-converted standard library. An optional second positional sets the output root for the generated `src\` (app) and `pkg\` (dependency) trees. A package that fails to convert is reported and skipped. The module is converted, and validated, as Go builds it with the `safe` build tag (see [Build tags](ConvertingAModule.md#build-tags-for-a-module)). See [Converting a real-world Go module](ConvertingAModule.md). |
| `-recurse=module` | Same, but convert only the module's own packages: third-party packages are referenced into `pkg\` but not converted, and are listed at the end of the run. See [converting the module only](ConvertingAModule.md#optional-convert-the-module-only-and-deal-with-its-dependencies-later). |
| `-recurse=nuget` | Same, but the standard library, the `golib` runtime and the analyzer come from NuGet ([`go.<pkg>`](https://www.nuget.org/packages?q=go2cs%20ritchiecarroll), [`go.lib`](https://www.nuget.org/packages/go.lib), [`go.gen`](https://www.nuget.org/packages/go.gen)), so nothing is staged locally. Third-party modules that the [nugetgo.net](https://nugetgo.net) registry maps are referenced as published packages when a qualifying version exists, and converted locally otherwise (see [Mapping modules to NuGet packages](ConvertingAModule.md#mapping-modules-to-nuget-packages)). Scope and reference style combine: `-recurse=module,nuget`. The published packages match the Go release go2cs is built with; go2cs checks the module against that release first, and refuses a mismatch, naming both versions. |
| `-module-safe-tag=false` | With `-recurse`: convert and validate the module without the `safe` build tag, using the files a plain `go build` selects. On by default; see [Build tags](ConvertingAModule.md#build-tags-for-a-module). |
| `-nuget-map <source>` | With `-recurse=nuget`: add a mapping source, a local file or an `https://` URL. Repeatable; the first source that names a module wins, and nugetgo.net answers the rest. `-nuget-map off` turns mapping off. See [Mapping modules to NuGet packages](ConvertingAModule.md#mapping-modules-to-nuget-packages). |
| `-nuget-map-only` | With `-nuget-map`: use **only** the listed sources — the nugetgo.net fallback is dropped, so a module your sources do not name stays local, and nugetgo.net is never fetched. |
| `-nuget-map-exclude <module-path>` | With `-recurse=nuget`: never map this module, whatever a source says. Repeatable. |
| `-nuget-map-refresh` | With `-recurse=nuget`: fetch every URL source unconditionally (bypassing the local cache) and re-resolve, adopting what the sources say now instead of what `go2cs.nuget.lock` pinned. |
| `-nuget-map-canonical-only` | With `-recurse=nuget`: apply only **canonical** mappings (the module owner's own conversion); a community mapping is treated as unmapped. |
| `-tests` | Also convert the package's `_test.go` suite and emit a runnable C# test-host project (default off). Forces `-comments` on and works from a bare clone with no flags or environment setup. With plain `-recurse` it validates a whole module against its own tests: `go2cs -tests -recurse module_dir out_root`, with the output root outside the module's source tree; a module that needs a newer Go than the converted standard library is refused. See [Validate a converted test suite](BuildingGo2cs.md#validate-a-converted-test-suite). |
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
| `-version` | Print the converter's release tuple and exit 0, converting nothing. One `key=value` line per key, always in this order, every key present (`unknown` when a value cannot be resolved), so a bug report or a build script can paste or parse it: `go2cs.release` (the go2cs corpus release this binary belongs to, e.g. `1.24.13.5`), `go2cs.converter` (this binary's identity, `exe-` plus the first 16 hex digits of its SHA-256 — the value test manifests record), `go.packages` (the `go.*` NuGet version `-recurse=nuget` defaults `$(GoStdLibVersion)` to, e.g. `1.24.13.*`), `go.toolchain` (the active Go toolchain, `go env GOVERSION`), `go.build` (the Go toolchain that built this binary), `vcs.revision` and `vcs.modified` (the commit a binary built from a clone was built from, and whether its tree had local changes; Go 1.24 does not stamp a binary built from a `git worktree` checkout, so that reads `unknown`). New keys are only ever appended. |
| ~~`-cgo`~~ | ~~Also convert cgo-targeted files.~~ Not yet functional, but planned: the [cgo interop plan](PLAN-cgo-interop.md) lays out a P/Invoke-backed bridge for `import "C"`, and this flag comes alive with it. |

All converted C# code references a hand-written runtime library (`golib`, published as the [`go.lib`](https://www.nuget.org/packages/go.lib)
NuGet package) plus a set of Roslyn source generators that supply Go semantics at compile time (published as
[`go.gen`](https://www.nuget.org/packages/go.gen)). A `-recurse=nuget` conversion wires both up for you.

An error in the Go input is reported with a `GO2CS` code, one line per error:
see [Diagnostics](BuildingGo2cs.md#diagnostics).
