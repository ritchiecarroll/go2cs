// options.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package clihelp

// options is the table: every command-line option go2cs accepts, sorted by class, then by group, then as listed.
// `go2cs -help` and docs/CommandLine.md are both written from it, so a change to an option's text is made here, and
// `go generate` in src/go2cs rewrites the page. Summary is the one line the help lists; Details is the full
// explanation, in Markdown, that `go2cs -help <option>` prints and the page's option reference carries.
var options = []Option{
	{
		// The flag's usage text before this table (main.go, master cb6def82a2), kept as history:
		//   Recursively convert an end-user module and its third-party dependencies (references the pre-converted
		//   standard library); use -recurse=module to convert only the module's own packages, leaving the third-party
		//   closure referenced but unconverted, and -recurse=nuget to reference the published go2cs NuGet packages
		//   (go.<pkg>/go.lib/go.gen) instead of local project references (values combine: -recurse=module,nuget)
		Name: "recurse", Value: "[=module,nuget]", Class: Common, Group: "Converting",
		Summary: "Convert a module and its third-party dependencies (=module: the module only; =nuget: standard library from NuGet)",
		Details: "" +
			"Convert a downloaded module **and its third-party dependencies** in dependency order, referencing (not " +
			"reconverting) the pre-converted standard library. **Plain `-recurse` references a standard library " +
			"staged on this machine** by `deploy-core.ps1`, at `<gopath>/src/go2cs`, so pass `-go2cspath " +
			"<gopath>/src/go2cs` with it; most conversions want `-recurse=nuget`, which needs nothing staged. An " +
			"optional second positional sets the output root for the generated `src\\` (app) and `pkg\\` (dependency) " +
			"trees. A package that fails to convert (it does not load, or the converter panics) is reported and " +
			"skipped, and so is a package the converter **refuses** (any refusal error, listed in [Diagnostic " +
			"codes](DiagnosticCodes.md)): its diagnostics print as errors, it gets " +
			"no project, every other package still converts, and the run ends with exit 1, naming each failed and " +
			"refused package and each converted package that imports a refused one (which will not build until the " +
			"refused package converts). A code downgraded by `-warn` is not a refusal. A single-package conversion " +
			"still stops at a refusal and writes nothing. `-tests -recurse` skips a refused package too and tests " +
			"each package as `go test ./...` does: a tested package that is refused, or whose test closure imports a " +
			"refused package, is named with that reason and gets no test project, the others are converted and " +
			"tested, and the run ends with exit 1. Its conversion step also goes on past a failed package as before. " +
			"The module is converted, and validated, as Go builds it with the build tags `safe,purego,noasm`, which " +
			"the run prints at its start (see [Build tags](ConvertingAModule.md#build-tags-for-a-module)). See " +
			"[Converting a real-world Go module](ConvertingAModule.md).\n\n" +
			"`-recurse=module`: Same, but convert only the module's own packages: third-party packages are " +
			"referenced into `pkg\\` but not converted, and are listed at the end of the run. It changes the scope " +
			"only: alone it keeps plain `-recurse`'s local references, and `-recurse=module,nuget` gives NuGet ones. " +
			"See [converting the module " +
			"only](ConvertingAModule.md#optional-convert-the-module-only-and-deal-with-its-dependencies-later).\n\n" +
			"`-recurse=nuget`: Same, but the standard library, the `golib` runtime and the analyzer come from NuGet " +
			"([`go.<pkg>`](https://www.nuget.org/packages?q=go2cs%20ritchiecarroll), " +
			"[`go.lib`](https://www.nuget.org/packages/go.lib), [`go.gen`](https://www.nuget.org/packages/go.gen)), " +
			"so nothing is staged locally. Third-party modules that the [nugetgo.net](https://nugetgo.net) registry " +
			"maps are referenced as published packages when a qualifying version exists, and converted locally " +
			"otherwise (see [Mapping modules to NuGet " +
			"packages](ConvertingAModule.md#mapping-modules-to-nuget-packages)). Scope and reference style combine: " +
			"`-recurse=module,nuget`. The published packages match the Go release go2cs is built with; go2cs checks " +
			"the module against that release first, and refuses a mismatch, naming both versions.",
	},
	{
		// The flag's usage text before this table (main.go, master cb6def82a2), kept as history:
		//   Path to C# converted code
		Name: "go2cspath", Value: "<dir>", Class: Common, Group: "Converting",
		Summary: "Where the go2cs standard library and runtime are when not from NuGet, and where -stdlib writes",
		Details: "" +
			"Runtime/stdlib root (env `GO2CSPATH`; default `~/go2cs`) used by generated `$(go2csPath)…` references, " +
			"and the output root for `-stdlib`. `deploy-core.ps1` stages at `<gopath>/src/go2cs`, not at the " +
			"default, so a conversion against its stage names it here. For a single-package conversion, C# output " +
			"goes to optional `[output_dir]` (in place by default).",
	},
	{
		// The flag's usage text before this table (main.go, master cb6def82a2), kept as history:
		//   Target platform(s) for conversion, format: os/arch; comma-separated for a list
		//   (windows/amd64,linux/amd64,darwin/amd64,linux/arm64), which with -stdlib emits the multi-platform (layout
		//   L3) corpus — one flavour per target
		Name: "platforms", Value: "<os/arch>", Class: Common, Group: "Converting",
		Summary: "The target platform, os/arch (default: this machine's)",
		Details: "" +
			"Target platform for build-tagged files (defaults to the host; a `-tests` run of a standard-library " +
			"package defaults to the converted standard library's flavour for the host: the host's own platform when " +
			"the library is converted for it (`windows/amd64`, `linux/amd64`, `linux/arm64`, `darwin/amd64`), " +
			"otherwise the host's OS on `amd64`, so an Apple silicon host converts at `darwin/amd64`). A " +
			"comma-separated **list** (`windows/amd64,linux/amd64,darwin/amd64,linux/arm64`) is accepted only with " +
			"`-stdlib`, which then converts once per target and writes the multi-platform standard library, one " +
			"flavour per target. Any other conversion emits for exactly one target, so a list there is rejected " +
			"rather than silently converting the first.",
	},
	{
		// The flag's usage text before this table (main.go, master cb6def82a2), kept as history:
		//   Comma-separated build tags applied when loading packages, e.g. -tags purego to select the portable Go
		//   implementations over assembly ones (with -stdlib or -tests, "+defaultStdLibTagList+" is applied by default,
		//   and with -recurse "+strings.Join(defaultModuleBuildTags, ",")+"; any explicit -tags value replaces the
		//   default entirely)
		Name: "tags", Value: "<list>", Class: Common, Group: "Converting",
		Summary: "Build tags applied when loading packages, comma-separated",
		Details: "" +
			"Build tags applied when loading packages. `-stdlib` and `-tests` apply `purego` and `math_big_pure_go` " +
			"by default, and `-recurse` applies `safe`, `purego` and `noasm` (with `-tests`, " +
			"`purego,math_big_pure_go,safe,noasm`). A `-stdlib` or `-recurse` run prints the tags it applies at its " +
			"start. An explicit `-tags` replaces every default.",
	},
	{
		// The flag's usage text before this table (main.go, master cb6def82a2), kept as history:
		//   Include comments in output
		Name: "comments", Value: "", Class: Common, Group: "Converting",
		Summary: "Carry the Go source comments into the C#",
		Details: "" +
			"Carry source comments into the output (best effort, see [go/ast comment " +
			"status](https://github.com/golang/go/issues/20744)).",
	},
	{
		// The flag's usage text before this table (main.go, master cb6def82a2), kept as history:
		//   Convert Go standard library (implies -tags "+defaultStdLibTagList+" by default; pass an explicit -tags to
		//   override)
		Name: "stdlib", Value: "", Class: Common, Group: "Converting",
		Summary: "For go2cs development: convert the Go standard library into the -go2cspath root",
		Details: "" +
			"For go2cs development: convert the Go standard library, or only the packages named after it (`go2cs " +
			"-stdlib fmt strings`), into the `-go2cspath` root, as `core/<package>`. Converting a program does not need " +
			"it: `-recurse=nuget` takes the standard library from NuGet. Convert into a root copied from a go2cs " +
			"clone's `src/core`: in any other root the hand-written files are replaced by automatic conversions that " +
			"compile and do not work. With " +
			"no `-tags` it applies the build tags `purego,math_big_pure_go`, which select the portable Go " +
			"implementations over the assembly ones; an explicit `-tags` replaces them, and `-tags=` applies none.",
	},
	{
		// The flag's usage text before this table (main.go, master cb6def82a2), kept as history:
		//   Convert a single cgo package: load it cgo-on (CGO_ENABLED=1; needs a C compiler) and convert it when every
		//   C use it makes is one cgo support binds (calls of C functions a shared library defines;
		//   docs/Limitations.md). Any other use is refused by name, with the phase that owns it, before anything is
		//   written. Converts for the host's own target, or for another -platforms target when CC names that target's C
		//   compiler (for example the pinned zig, CC="zig cc -target <triple>"); without one another target is refused,
		//   for a package with no C in it too. Not combinable with -stdlib or -tests. With -recurse, every
		//   non-standard-library package of the module loads cgo-on and each cgo package converts this way; a package
		//   that imports runtime/cgo or plugin, whose cgo-off builds are not working builds of their cgo API, is
		//   refused by name, and one that imports net or os/user binds their cgo-off builds, as Go does with
		//   CGO_ENABLED=0
		Name: "cgo", Value: "", Class: Common, Group: "Converting",
		Summary: "Convert a cgo package whose C use go2cs can bind (needs a C compiler)",
		Details: "" +
			"Converts a single cgo package. It loads the package with cgo on (`CGO_ENABLED=1`, which needs a C " +
			"compiler) and converts it when every use of C it makes is one go2cs binds: calls of C functions a " +
			"shared library defines ([Limitations](Limitations.md#cgo)). Any other use is refused by name " +
			"(`GO2CS2001`, see [Diagnostic codes](DiagnosticCodes.md)), with its reason, before anything is " +
			"written.<!-- 2026-11-23 .5 docs pass (P1), UX-10 follow-on: no cgo refusal text names a phase " +
			"(src/go2cs/cgoRoute.go). --> It converts for the machine's own operating system and architecture, or " +
			"for another `-platforms` target when `CC` names that target's C compiler (for example `zig cc -target " +
			"<triple>`); without one, any other `-platforms` is refused, for a package with no C in it too. Not " +
			"combinable with `-stdlib`. With `-tests`, it converts ONE package with its tests into a scratch root " +
			"seeded from `src/core`, which the seed step marks with a `.go2cs-cgo-scratch-root` file at the root: a " +
			"root without the marker, and an output inside a repository's `src/core` (symlinks resolved), are " +
			"refused by name. The package's own test loads and the `go test` oracle run with cgo on, and every " +
			"other package keeps its build without cgo. With `-recurse`, every package of the module outside the " +
			"standard library loads with cgo on, and each cgo package converts this way. A package that imports " +
			"`runtime/cgo` or `plugin` is refused by name (`GO2CS2001`); one that imports `net` or `os/user` uses " +
			"their standard-library builds without cgo, as Go does with `CGO_ENABLED=0` " +
			"([Limitations](Limitations.md#cgo)). More of cgo is on the [Roadmap](Roadmap.md#cgo).<!-- the [cgo " +
			"interop plan](PLAN-cgo-interop.md) lays out the bridge (internal record, unlinked 2026-10-26, OQ-1) -->",
	},
	{
		// Added by cgo 3.4 seat 4c (claude/h-cgo-res-4c-r10) after this table existed; its usage text before the table:
		//   With -cgo -tests, list the C# files of the cgo-off selection that the cgo-on load deselects, and remove none.
		//   Without it, each one that carries go2cs's emission header for its source is removed, and printed with its
		//   reason (a file in an output seeded from the corpus, which would otherwise compile beside its cgo-on
		//   replacement); any other file in that set is refused by name
		Name: "cgo-removals-dry-run", Value: "", Class: Advanced, Group: "Testing",
		Summary: "With -cgo -tests: list the cgo-off files a cgo-on conversion would remove, and remove none",
		Details: "" +
			"With `-cgo -tests`, lists the files a cgo-on conversion would remove from its output, and removes none. " +
			"An output seeded from the corpus holds the package's build without cgo, and where the cgo-on build " +
			"swaps a file for another (`net`'s `cgo_stub.go` for `cgo_unix.go`), both would compile. Without this " +
			"flag each such file is removed and printed with its reason, but only when it carries go2cs's own " +
			"emission header for its source; any other file there, and a hand-owned one, is refused by name.",
	},
	{
		// The flag's usage text before this table (main.go, master cb6def82a2), kept as history:
		//   Convert eligible Go package tests and emit a runnable test host project (implies -tags
		//   "+defaultStdLibTagList+" by default, as -stdlib does)
		Name: "tests", Value: "", Class: Common, Group: "Testing",
		Summary: "Also convert the package's Go tests and emit a runnable C# test host",
		Details: "" +
			"Also convert the package's `_test.go` suite and emit a runnable C# test-host project (default off). " +
			"Forces `-comments` on. Like `-stdlib`, with no `-tags` it applies the build tags " +
			"`purego,math_big_pure_go`. For one package it finds the go2cs tree above its output directory, so it works " +
			"from a bare clone with no flags or environment setup. With plain `-recurse` it validates a whole module " +
			"against its own tests: `go2cs -tests -recurse -go2cspath <go2cs_clone>/src module_dir out_root`, with " +
			"the output root outside the module's source tree. The module form does not look for the go2cs tree, so " +
			"name it with `-go2cspath`, as an absolute path (or `GO2CSPATH`). It takes plain `-recurse` only: " +
			"`-tests` with `=module` or `=nuget` is refused; a module that needs a newer Go than the converted " +
			"standard library is refused. See [Validate a converted test " +
			"suite](BuildingGo2cs.md#validate-a-converted-test-suite).",
	},
	{
		// The flag's usage text before this table (main.go, master cb6def82a2), kept as history:
		//   Converted-test action: convert, build, run, compare, or all
		Name: "test-action", Value: "<action>", Class: Common, Group: "Testing",
		Summary: "With -tests: convert, build, run, compare (with go test) or all",
		Details: "" +
			"With `-tests`: one of `convert` (default), `build`, `run`, `compare`, or `all`. `convert` and `all` " +
			"convert the package and its tests; `build` / `run` / `compare` act on the **existing** converted " +
			"artifacts — validated against the test manifest's recorded input digest — without reconverting. " +
			"`compare` (and `all`) runs both `go test -json -count=1` and the converted C# test host and diffs the " +
			"terminal results by test name. On unix a `-tests` run handles SIGINT and SIGQUIT as `go test` does. " +
			"Every child it starts, the converted host included, begins with both at the default disposition, even " +
			"when the run was launched with them ignored (an async `cmd &`). A terminal Ctrl-C or Ctrl-\\ stops the " +
			"run: go2cs kills each running child's process group and exits 130.",
	},
	{
		// The flag's usage text before this table (main.go, master cb6def82a2), kept as history:
		//   Print the converter's release tuple (go2cs release, converter revision, go.* package version, Go
		//   toolchains, VCS revision) as key=value lines and exit
		Name: "version", Value: "", Class: Common, Group: "Information",
		Summary: "Print the converter's release tuple and exit",
		Details: "" +
			"Print the converter's release tuple and exit 0, converting nothing. One `key=value` line per key, " +
			"always in this order, every key present (`unknown` when a value cannot be resolved), so a bug report or " +
			"a build script can paste or parse it: `go2cs.release` (the go2cs corpus release this binary belongs to, " +
			"four numbers: the Go release, then the go2cs revision), `go2cs.converter` (this binary's identity, " +
			"`exe-` plus the first 16 hex digits of its SHA-256 — the value test manifests record), `go.packages` " +
			"(the `go.*` NuGet version `-recurse=nuget` defaults `$(GoStdLibVersion)` to: the Go minor with a float, " +
			"for example `1.26.*`), `go.toolchain` (the active Go toolchain, `go env GOVERSION`), `go.build` (the Go " +
			"toolchain that built this binary), `vcs.revision` and `vcs.modified` (the commit a binary built from a " +
			"clone was built from, and whether its tree had local changes; Go does not stamp a binary built from a " +
			"`git worktree` checkout, whose `.git` is a file rather than a directory, so that reads `unknown`). New " +
			"keys are only ever appended.",
	},
	{
		// The flag's usage text before this table (main.go, master cb6def82a2), kept as history:
		//   Path to Go root directory
		Name: "goroot", Value: "<dir>", Class: Advanced, Group: "Converting",
		Summary: "Override the detected Go root",
		Details: "" +
			"Override the detected Go root.",
	},
	{
		// The flag's usage text before this table (main.go, master cb6def82a2), kept as history:
		//   Path to Go path directory
		Name: "gopath", Value: "<dir>", Class: Advanced, Group: "Converting",
		Summary: "Override the detected Go path",
		Details: "" +
			"Override the detected Go path (GOPATH).",
	},
	{
		// The flag's usage text before this table (main.go, master cb6def82a2), kept as history:
		//   With -recurse: convert the module, and with -tests run its go test baseline, with the build tags
		//   "+strings.Join(defaultModuleBuildTags, ",")+", so a library that ships a fallback for something converted
		//   code cannot do uses it: `safe` for unsafe pointer arithmetic into Go runtime internals (testify's
		//   internal/spew), `purego` and `noasm` for hand-written assembly (klauspost/cpuid/v2); -module-safe-tag=false
		//   drops all three and builds the module's default file set (an explicit -tags overrides both)
		Name: "module-safe-tag", Value: "", Class: Advanced, Group: "Converting",
		Summary: "With -recurse: apply the safe, purego and noasm build tags (default on)",
		Details: "" +
			"With `-recurse`: convert the module, and with `-tests` run its `go test` baseline, with the build tags " +
			"`safe`, `purego` and `noasm`, so a library that ships a fallback for something converted code cannot do " +
			"uses it: `safe` for unsafe pointer arithmetic into Go runtime internals, `purego` and `noasm` for " +
			"hand-written assembly. On by default. `-module-safe-tag=false` converts and validates the module without " +
			"them, using the files a plain `go build` selects; an explicit `-tags` overrides both. See [Build " +
			"tags](ConvertingAModule.md#build-tags-for-a-module).",
	},
	{
		// The flag's usage text before this table (main.go, master cb6def82a2), kept as history:
		//   Per-package cap the -stdlib driver applies to ONE package's conversion; Go duration syntax, must be greater
		//   than zero. It is a safety net against a hung conversion, never a performance assumption -- a slow host
		//   under concurrent load can legitimately need far more than the default, and a package that hits it stops the
		//   run (exit 1), and failed_packages.txt lists it with every package not yet converted, for a resume
		Name: "convert-timeout", Value: "<duration>", Class: Advanced, Group: "Converting",
		Summary: "With -stdlib: the cap on one package's conversion",
		Details: "" +
			"With `-stdlib`: the cap on one package's conversion, in Go duration syntax (default `10m`; it must be " +
			"greater than zero). A package that runs past it stops the run: the command exits 1, and " +
			"`failed_packages.txt` lists that package and every package not yet converted, so `-stdlib` with that " +
			"list resumes. It guards against a hung conversion: a slow or busy machine can need a larger value.",
	},
	{
		// The flag's usage text before this table (main.go, master cb6def82a2), kept as history:
		//   Proceed with a -stdlib or -tests run even when this binary is OLDER than the converter sources beside it.
		//   Those two drivers otherwise refuse, because their output is banked or measured and a stale run emits the
		//   previous converter's C# while exiting 0. Pass this for the one deliberate case: an A/B against a PRESERVED
		//   binary, where the pinned binary IS the measurement (see converterStaleness.go)
		Name: "allow-stale-converter", Value: "", Class: Advanced, Group: "Converting",
		Summary: "With -stdlib or -tests: run even when this binary is older than its sources",
		Details: "" +
			"With `-stdlib` or `-tests`: proceed even when this binary is older than the converter sources beside " +
			"it. Both refuse otherwise, because a stale binary emits the previous converter's C# and still exits 0. " +
			"It is meant for one deliberate case, a comparison against a preserved binary.",
	},
	{
		Name: "stack-append", Value: "<mode>", Class: Advanced, Group: "Converting",
		Summary: "Emit Go's stack-append buffer: on, off or auto",
		Details: "" +
			"`on`, `off` or `auto`: whether the conversion reproduces Go's stack-append backing store. Since Go 1.25 " +
			"an `append` to a slice that does not escape first fills a 32-byte buffer, which `cap()` after the " +
			"first appends and `testing.AllocsPerRun` can observe. `on` emits that buffer " +
			"(`var xᴷ = new stackAppend<T>();` and `x = append(xᴷ, x, v);`), reading which append sites qualify " +
			"from `go build -gcflags=-m` once per package and target (see `-probe-cache`). `off` emits plain " +
			"`x = append(x, v)` and runs no probe build. `auto`, the default, is `on` for `-stdlib` and `-tests`, " +
			"whose converted tests compare against Go's own, and `off` for a plain or `-recurse` conversion, a " +
			"single standard-library package converted without `-stdlib` included (use `-stdlib <package>` for the " +
			"standard library's emission). The " +
			"buffer is a heap array in golib, so `on` changes what a program can observe, not its speed or its " +
			"allocations. See [the stack-append buffer]" +
			"(ConversionStrategies-Reference/slices-and-arrays.md#a-non-escaping-append-can-take-gos-32-byte-stack-buffer).",
	},
	{
		// The flag's usage text before this table (main.go, master cb6def82a2), kept as history:
		//   Go build cache (GOCACHE) for the escape probe's go build/go test runs, kept apart from your own GOCACHE;
		//   default go2cs/probe-cache under the user cache directory, or $"+probeCacheEnv+". Go trims it by its own
		//   rule, and deleting it is always safe
		Name: "probe-cache", Value: "<dir>", Class: Advanced, Group: "Converting",
		Summary: "The Go build cache the escape probe uses (env GO2CS_PROBE_CACHE)",
		Details: "" +
			"The Go build cache (`GOCACHE`) of the `go build` and `go test -c` runs that read the Go compiler's " +
			"escape decisions for each converted package. They run only when `-stack-append` resolves `on`: for " +
			"`-stdlib` and `-tests` by default, and for any other conversion only with `-stack-append=on`; with it " +
			"off the cache is never created. Default: `go2cs/probe-cache` under the user cache " +
			"directory (`~/.cache` on Linux, `%LocalAppData%` on Windows, `~/Library/Caches` on macOS), or the " +
			"`GO2CS_PROBE_CACHE` environment variable. It is kept apart from your own `GOCACHE` because those runs " +
			"build the standard library for four targets with flags no ordinary build uses. Go removes entries " +
			"unused for five days, and deleting the directory is always safe: the next conversion rebuilds what it " +
			"needs. See [Known issues](KnownIssues.md#a-conversion-keeps-a-go-build-cache-of-its-own).",
	},
	{
		// The flag's usage text before this table (main.go, master cb6def82a2), kept as history:
		//   Timeout for each converted-test child process (build/run/compare); the test host's dotnet publish gets at
		//   least 30m
		Name: "test-timeout", Value: "<duration>", Class: Advanced, Group: "Testing",
		Summary: "With -tests: the package deadline for build, run and compare",
		Details: "" +
			"Package deadline for a converted-test action, in Go duration syntax (default `2m`); `run` and `compare` " +
			"give it to both `go test` and the converted host. The host's `dotnet publish` always gets at least " +
			"`30m`, because the first publish on a fresh tree builds the whole standard-library closure. A suite " +
			"that runs long in C# needs a larger value: `hash/maphash` is validated with `-test-timeout 60m`.",
	},
	{
		// The flag's usage text before this table (main.go, master cb6def82a2), kept as history:
		//   Per-test deadline for the CONVERTED host under -test-action run/compare/all: a top-level test still running
		//   this long after it started is reported failed and ABANDONED (labelled so in the comparison), and the run
		//   goes on to the next test. Without it a hung test held the host to -test-timeout and every later test went
		//   unmeasured. 0 disables it. The Go side is unbounded, as `go test` is. A row whose own legitimate test
		//   outlasts the default takes a per-row floor (run-validated-sweep.ps1 $testDeadlineFloors)
		Name: "test-deadline", Value: "<duration>", Class: Advanced, Group: "Testing",
		Summary: "With -tests: the per-test deadline for the converted host (0 disables it)",
		Details: "" +
			"Per-test deadline for the CONVERTED host under `run` and `compare`, in Go duration syntax (default " +
			"`15m`; `0` turns it off). A top-level test still running this long after it started is reported `fail`, " +
			"marked abandoned, and the host goes on to the next test, so one hung test does not hold the host to " +
			"`-test-timeout` and leave every later test unmeasured. The comparison labels such a row \"abandoned: did " +
			"not finish within the per-test deadline\", and every later event names the abandoned test " +
			"(`afterAbandoned`), since its goroutines keep running. The Go side is unbounded, as `go test` is. It " +
			"reaches the host as `GO2CS_TEST_DEADLINE`, never on argv. A package whose own test runs longer than the " +
			"default needs a larger value; the repository's validation sweep (`src/run-validated-sweep.ps1`) sets " +
			"one per package.",
	},
	{
		// The flag's usage text before this table (main.go, master cb6def82a2), kept as history:
		//   Regex handed VERBATIM to BOTH sides of a -test-action compare (go test -run and the converted host --run),
		//   so the two runs filter identically. Intended for the block-gated census: exclude a test that BLOCKS the
		//   suite by passing an anchored alternation of the parents to keep. A gated census is DIAGNOSTIC ONLY and must
		//   never bank a row -- the row banks from an ungated run, after the block is rooted or the divergence disclosed
		Name: "test-filter", Value: "<regex>", Class: Advanced, Group: "Testing",
		Summary: "With -tests: a regular expression both sides run, as go test -run",
		Details: "" +
			"With `-tests`: a regular expression passed unchanged to `go test -run` and to the converted host under " +
			"`run` and `compare`, so both sides run the same tests. A filtered run is for diagnosis: it writes no " +
			"proof pages, and a validated result comes from an unfiltered run.",
	},
	{
		// The flag's usage text before this table (main.go, master cb6def82a2), kept as history:
		//   Publish/run configuration for the converted test host: Release (default since 2026-09-02 -- the validation
		//   configuration of RECORD, with an explicit -p:go2csPath replacing the Debug-conditional csproj-template
		//   default, and the CLR's tiered JIT disabled by default, see -test-tiered) or Debug (the pre-2026-09-02
		//   default, still fully supported by flag). Recorded on every proof page and in the comparison record so a
		//   verdict carries the level it was measured at. The default moved on the deployment owner's ruling, gated on
		//   the Release census (docs/phase4/CENSUS-release-tc0-delta.md), not on this flag's own authority
		Name: "test-config", Value: "<config>", Class: Advanced, Group: "Testing",
		Summary: "With -tests: build and run the test host as Release (default) or Debug",
		Details: "" +
			"With `-tests`: the configuration the converted test host is published and run in, `Release` (the " +
			"default) or `Debug`. It is recorded on every proof page and in the comparison record.",
	},
	{
		// The flag's usage text before this table (main.go, master cb6def82a2), kept as history:
		//   With -test-config Release, opt back IN to the CLR's default tiered JIT (Release's own default is
		//   DOTNET_TieredCompilation=0, since a verdict that depends on JIT promotion timing is not reproducible run to
		//   run). Meaningless with -test-config Debug. It changes what the C# host's JIT does, not what the converter
		//   emits
		Name: "test-tiered", Value: "", Class: Advanced, Group: "Testing",
		Summary: "With -test-config Release: turn the CLR's tiered JIT back on",
		Details: "" +
			"With `-test-config Release`: turn the CLR's tiered JIT back on. `Release` runs the host with it off by " +
			"default, so a verdict does not depend on when the JIT promotes a method. It has no effect with `Debug`, " +
			"and it changes what the host's JIT does, not what the converter emits. A program you ship keeps tiering " +
			"on: see [Tuning converted code](Tuning.md#net-runtime-settings).",
	},
	{
		// The flag's usage text before this table (main.go, master cb6def82a2), kept as history:
		//   Cap the MSBuild nodes of the converted test host's dotnet publish, the only dotnet command a -tests run
		//   starts, by passing it -m:N. 0 (the default) passes no -m, and MSBuild uses one node per logical processor.
		//   Only an explicit -m on the command line caps it: a Directory.Build.rsp or MSBUILDNODECOUNT in the tree did
		//   not (measured on the i9, 2026-10-18)
		Name: "test-build-maxcpu", Value: "<n>", Class: Advanced, Group: "Testing",
		Summary: "With -tests: cap the test host's dotnet publish at n MSBuild nodes",
		Details: "" +
			"With `-tests`: cap the MSBuild nodes of the converted test host's `dotnet publish` by passing it " +
			"`-m:<n>`. That publish is the only `dotnet` command a `-tests` run starts, so this caps every build the " +
			"pipeline does. `0` (the default) passes no `-m`, and MSBuild uses one node per logical processor. Only " +
			"an explicit `-m` caps it: a `Directory.Build.rsp` in the tree or `MSBUILDNODECOUNT` does not. The " +
			"validation sweep (`src/run-validated-sweep.ps1`) passes it to every package's conversion with " +
			"`-TestBuildMaxCpu <n>` (default `0`, which passes nothing).",
	},
	{
		// The flag's usage text before this table (main.go, master cb6def82a2), kept as history:
		//   Have the converted test host's dotnet publish write an MSBuild binary log to
		//   <output>/bin/tests/publish.binlog, from the first attempt, KEPT only when the publish fails and deleted
		//   when it succeeds. For census and battery sweeps chasing a build failure that passes on a re-run; costs ~35%
		//   of a warm publish
		Name: "test-publish-binlog", Value: "", Class: Advanced, Group: "Testing",
		Summary: "With -tests: keep an MSBuild binary log of a failed test-host publish",
		Details: "" +
			"With `-tests`: have the test host's `dotnet publish` write an MSBuild binary log to " +
			"`<output>/bin/tests/publish.binlog`, kept only when the publish fails. It slows the publish; use it to " +
			"chase a build failure.",
	},
	{
		// The flag's usage text before this table (main.go, master cb6def82a2), kept as history:
		//   Run the -tests compare's ORACLE (`go test`) with -vet=off, for a package whose own `go test` fails at go
		//   test's vet checks rather than at a test (github.com/pmezard/go-difflib v1.0.0: two Example names its vet
		//   rejects). Off by default, and set per run, so per module. The run first reads go test's vet subset with `go
		//   vet -json`, refuses the option for a package that vet passes, and states every finding, by analyzer, on the
		//   package's proof page and in MODULE.md. The converted side is compared test by test against the -vet=off run
		Name: "test-oracle-vet-off", Value: "", Class: Advanced, Group: "Testing",
		Summary: "With -tests: run go test with -vet=off",
		Details: "" +
			"With `-tests`: run `go test` with `-vet=off`, for a package whose own `go test` fails at the vet checks " +
			"it runs before any test, rather than at a test. For example, `github.com/pmezard/go-difflib` v1.0.0 has " +
			"two `Example` functions that vet's `tests` check rejects. Off by default, and set per run, so per " +
			"module. The run first reads what those checks report with `go vet -json`, and refuses the option for a " +
			"package they pass. Each finding is stated, by check, on the package's proof page and in the module's " +
			"`MODULE.md`. The converted tests are still compared one by one with the `-vet=off` run.",
	},
	{
		// The flag's usage text before this table (main.go, master cb6def82a2), kept as history:
		//   The GOARCH the -tests compare's ORACLE (`go test`) runs at, when it must differ from the target's: the arch
		//   the converted host process runs at. Empty (the default) is the target's own. The conversion, the converted
		//   host and the manifest keep the target. For a host whose native arch differs from the corpus target's,
		//   osx-arm64 running the darwin/amd64 corpus, where the default ran the oracle under Rosetta and read
		//   Rosetta's own behaviour as the package's. Recorded as oracleGoarch in the comparison record when it differs
		Name: "test-oracle-goarch", Value: "<arch>", Class: Advanced, Group: "Testing",
		Summary: "With -tests: the GOARCH go test runs at, when it must differ from the target",
		Details: "" +
			"With `-tests`: the `GOARCH` that `go test` runs at, when it must differ from the conversion's target. " +
			"For example, an Apple silicon host validating the `darwin/amd64` conversion sets `arm64`, so Go does " +
			"not run under Rosetta. Empty by default, which is the target's own; when set, it is recorded in the " +
			"comparison record.",
	},
	{
		// The flag's usage text before this table (main.go, master cb6def82a2), kept as history:
		//   Convert a package the -stdlib queue deliberately skips (testing, unsafe, builtin, cmd/...) as a -tests
		//   target anyway. Refused by default: `testing` IS the hand-owned Phase-4 test host, and the pipeline's
		//   natural output path is the very directory the host lives in, so a mistyped command overwrites it (measured
		//   2026-09-03 -- the run replaced src/core/testing/testing.cs with Go's converted testing.go). Pass this ONLY
		//   with a BARE scratch output root, for a deliberate measurement whose emission is thrown away; it is not a
		//   route to banking such a package, which would still hit the F15b 'ONE testing package, period' collision.
		//   Consulted AFTER the hand-own host path since 2026-09-20, so what this flag does depends on what is AT the
		//   output root: on a BARE root it keeps the full production census measured 2026-09-03 (no csproj there, so
		//   the host path cannot open), while at a hand-owned counterpart's own directory -- or at a root SEEDED from
		//   src/core, which carries that counterpart's csproj and markers and so is NOT bare -- the host path wins
		//   instead, the run converts TESTS ONLY, and no production file is emitted over a hand-own
		Name: "test-allow-handown", Value: "", Class: Advanced, Group: "Testing",
		Summary: "With -tests: convert a package -stdlib skips, into a bare scratch root",
		Details: "" +
			"With `-tests`: convert a package that `-stdlib` deliberately skips (`testing`, `unsafe`, `builtin`, " +
			"`cmd/...`) anyway. Refused by default, because the natural output path for `testing` is the " +
			"hand-written test host itself, and a mistyped command overwrites it. Use it only with an empty scratch " +
			"output root: the output is for a measurement, not for use.",
	},
	{
		// The flag's usage text before this table (main.go, master cb6def82a2), kept as history:
		//   With -tests on a third-party module: the committed tree of module rows' disclosure manifests
		//   (<root>/<module>@<version>/<package dir>/go2cs_test_disclosures.json, e.g. src/tests/ModuleDisclosures).
		//   Read only when the package's output directory holds no manifest, and only for the module version this run
		//   resolved
		Name: "module-disclosures", Value: "<dir>", Class: Advanced, Group: "Testing",
		Summary: "With -tests on a module: the tree of committed disclosure manifests",
		Details: "" +
			"With `-tests` on a third-party module: the committed tree of disclosure manifests (the tests a module's own validation excludes, each with its reason), laid " +
			"out like the module cache (`<dir>/<module path>@<version>/<package dir>/go2cs_test_disclosures.json`; " +
			"the repository keeps it at `src/tests/ModuleDisclosures`). A manifest there is read only when the " +
			"package's output directory holds none, and only for the module version this run resolved; a manifest " +
			"kept for another version is not applied, and the run prints one line naming both. The version comes " +
			"from the module-cache directory the module was converted from, or from `-module-version`; when it can " +
			"be found from neither, the run is refused before any conversion. Takes effect only with `-tests " +
			"-recurse`, and is refused otherwise.",
	},
	{
		// The flag's usage text before this table (main.go, master cb6def82a2), kept as history:
		//   With -tests -module-disclosures on a module source that is not a module-cache directory (a copy of the
		//   module): the version the copy is, e.g. v1.62.0. A copy carries no version of its own. Refused when it
		//   contradicts the module-cache directory's version; without it, -module-disclosures on a copy is refused
		//   before any conversion
		Name: "module-version", Value: "<version>", Class: Advanced, Group: "Testing",
		Summary: "With -module-disclosures: the version a copied module source is",
		Details: "" +
			"With `-tests -module-disclosures` on a module source that is not a module-cache directory, such as a " +
			"copy of the module: the version the copy is (e.g. `v1.62.0`). A copy carries no version of its own. " +
			"Refused when it is not a version of the module, when it contradicts the module-cache directory's own " +
			"version, when the module cache holds that version and its `go.mod` differs from the copy's, when the " +
			"disclosure tree keeps the module under other versions only, and without `-module-disclosures`. A " +
			"version taken from this flag is CLAIMED, not resolved: the run's line, its comparison record " +
			"(`moduleVersionClaimed`) and its proof page say so.",
	},
	{
		// The flag's usage text before this table (main.go, master cb6def82a2), kept as history:
		//   With -recurse=nuget (where mapping is ON by default): a Go-module -> NuGet-package mapping source, a local
		//   file or an https:// URL in nugetgo.net's schema v1; REPEATABLE, and the listed order is the precedence (the
		//   first source naming a module answers it). The nugetgo.net registry (https://nugetgo.net/v1/mappings.txt)
		//   answers every module the listed sources do not name, unless -nuget-map-only. A mapped module is referenced
		//   as its published package (one exact-pinned PackageReference, locked in go2cs.nuget.lock) when a version
		//   built for this go2cs release describes it exactly; otherwise it converts locally and the report says why.
		//   'off' disables mapping entirely
		Name: "nuget-map", Value: "<source>", Class: Advanced, Group: "NuGet mapping",
		Summary: "With -recurse=nuget: add a module-to-package mapping source (repeatable; off disables mapping)",
		Details: "" +
			"With `-recurse=nuget`: add a mapping source, a local file or an `https://` URL. Repeatable; the first " +
			"source that names a module wins, and nugetgo.net answers the rest. `-nuget-map off` turns mapping off. " +
			"See [Mapping modules to NuGet packages](ConvertingAModule.md#mapping-modules-to-nuget-packages).",
	},
	{
		// The flag's usage text before this table (main.go, master cb6def82a2), kept as history:
		//   With -nuget-map: use only the listed sources, dropping the nugetgo.net fallback, so a module they do not
		//   name stays local and the registry is never fetched
		Name: "nuget-map-only", Value: "", Class: Advanced, Group: "NuGet mapping",
		Summary: "With -nuget-map: use only the listed sources, not nugetgo.net",
		Details: "" +
			"With `-nuget-map`: use **only** the listed sources — the nugetgo.net fallback is dropped, so a module " +
			"your sources do not name stays local, and nugetgo.net is never fetched.",
	},
	{
		// The flag's usage text before this table (main.go, master cb6def82a2), kept as history:
		//   With -recurse=nuget: a module path that is never mapped, whatever a source says; repeatable
		Name: "nuget-map-exclude", Value: "<module-path>", Class: Advanced, Group: "NuGet mapping",
		Summary: "With -recurse=nuget: never map this module (repeatable)",
		Details: "" +
			"With `-recurse=nuget`: never map this module, whatever a source says. Repeatable.",
	},
	{
		// The flag's usage text before this table (main.go, master cb6def82a2), kept as history:
		//   With -recurse=nuget: fetch every URL source unconditionally (bypassing the cache) and re-resolve every
		//   module, adopting what the sources now say instead of what go2cs.nuget.lock pinned
		Name: "nuget-map-refresh", Value: "", Class: Advanced, Group: "NuGet mapping",
		Summary: "With -recurse=nuget: fetch every source again and re-resolve every module",
		Details: "" +
			"With `-recurse=nuget`: fetch every URL source unconditionally (bypassing the local cache) and " +
			"re-resolve, adopting what the sources say now instead of what `go2cs.nuget.lock` pinned.",
	},
	{
		// The flag's usage text before this table (main.go, master cb6def82a2), kept as history:
		//   With -recurse=nuget: apply canonical mappings only (the module owner's own conversion); a community mapping
		//   is treated as unmapped
		Name: "nuget-map-canonical-only", Value: "", Class: Advanced, Group: "NuGet mapping",
		Summary: "With -recurse=nuget: apply only the module owners' own mappings",
		Details: "" +
			"With `-recurse=nuget`: apply only **canonical** mappings (the module owner's own conversion); a " +
			"community mapping is treated as unmapped.",
	},
	{
		// The flag's usage text before this table (main.go, master cb6def82a2), kept as history:
		//   With -recurse=nuget: read mapped packages' versions from this feed INSTEAD of nuget.org -- an https://
		//   NuGet v3 service index (a rehearsal gallery such as int.nugettest.org) or a local folder of nupkgs; a
		//   rehearsal version (nugetgo-pack.ps1 -RehearsalSuffix) is admitted only from it. The restore of the emitted
		//   projects reads your own package sources, so name the feed there too
		Name: "nuget-map-feed", Value: "<feed>", Class: Advanced, Group: "NuGet mapping",
		Summary: "With -recurse=nuget: read mapped package versions from this feed, not nuget.org",
		Details: "" +
			"With `-recurse=nuget`: read mapped packages' versions from this feed **instead of** nuget.org: an " +
			"`https://` NuGet v3 service index (a test gallery such as int.nugettest.org, for trying packages before they are published) or a local folder of " +
			"`.nupkg` files. A rehearsal version (`nugetgo-pack.ps1 -RehearsalSuffix`) is admitted only from it. The " +
			"restore reads your own package sources, so add the feed there too.",
	},
	{
		// The flag's usage text before this table (main.go, master cb6def82a2), kept as history:
		//   Number of spaces for indentation
		Name: "indent", Value: "<n>", Class: Advanced, Group: "Output",
		Summary: "Spaces per indent level (default 4)",
		Details: "" +
			"Spaces per indent level (default 4).",
	},
	{
		// The flag's usage text before this table (main.go, master cb6def82a2), kept as history:
		//   Prefer "var" declarations
		Name: "var", Value: "", Class: Advanced, Group: "Output",
		Summary: "Prefer var declarations where the type is obvious (default on)",
		Details: "" +
			"Prefer `var` declarations where the type is obvious (default on).",
	},
	{
		Name: "uco", Value: "", Class: Advanced, Group: "Output",
		Summary: "Emit channel operators instead of method calls (default on)",
		Details: "" +
			"Emit channel operators instead of method calls (default on).",
	},
	{
		// The flag's usage text before this table (main.go, master cb6def82a2), kept as history:
		//   Hoist mode, the standard library's: each string literal in a value slot with a readable name becomes one
		//   `static readonly` field (`F(helloˢ)`), and every other literal takes the default plain mode's cached form
		//   (`F("hello")`, converted once and shared by golib). The standard library always hoists; this opts any other
		//   package in
		Name: "hoist-literals", Value: "", Class: Advanced, Group: "Output",
		Summary: "Hoist string literals to named static fields, as the standard library does",
		Details: "" +
			"Hoist mode, the standard library's: a string literal in a value slot whose words give it a meaningful " +
			"name becomes one `static readonly` field (`F(helloˢ)`, and pre-boxed in an `any` slot); every other " +
			"site keeps the default plain form (`F(\"hello\")`, converted once and shared by golib; in an `any` slot, " +
			"boxed by a go2cs-gen interceptor), so no literal is copied on each evaluation. The standard library " +
			"always hoists; this opts any other package in. It is recommended for performance-critical code: a " +
			"hoisted field is a plain static read, where a plain literal makes a cache lookup on every evaluation. " +
			"It is also recommended for a large project that is IDE-heavy, because go2cs-gen re-binds every " +
			"`any`-slot literal call on each edit. See [plain " +
			"literals](ConversionStrategies-Reference/strings.md#a-literal-in-a-string-slot-is-a-plain-c-literal) " +
			"and [their design-time " +
			"cost](ConversionStrategies-Reference/strings.md#a-literal-in-an-any-slot-is-a-plain-c-literal-too-and-go2cs-gen-boxes-it-as-a-go-string). " +
			"The measured run-time difference, and when it matters: [Tuning converted " +
			"code](Tuning.md#string-literals).",
	},
	{
		// The flag's usage text before this table (main.go, master cb6def82a2), kept as history:
		//   Path to custom .csproj template file
		Name: "csproj", Value: "<file>", Class: Advanced, Group: "Output",
		Summary: "Generate project files from this .csproj template",
		Details: "" +
			"Generate project files from a custom `.csproj` template instead of the embedded one.",
	},
	{
		// The flag's usage text before this table (main.go, master cb6def82a2), kept as history:
		//   Override NuGet license metadata with an SPDX expression (for example MIT or BSD-3-Clause); does not
		//   relicense input code
		Name: "license", Value: "<expression>", Class: Advanced, Group: "Output",
		Summary: "Set the generated projects' NuGet license to an SPDX expression",
		Details: "" +
			"Set the generated projects' NuGet license metadata to an SPDX expression, for example `MIT` or " +
			"`BSD-3-Clause OR MIT`. It does not relicense the input code. See " +
			"[LICENSING.md](https://github.com/ritchiecarroll/go2cs/blob/master/LICENSING.md).",
	},
	{
		// The flag's usage text before this table (main.go, master cb6def82a2), kept as history:
		//   Include in each converted file a notice naming its Go source and module and that go2cs modified it by
		//   conversion (no timestamp)
		Name: "provenance", Value: "", Class: Advanced, Group: "Output",
		Summary: "Add a notice to each converted file naming its Go source and module",
		Details: "" +
			"Add to each converted file a two-line notice naming its Go source file and module and stating that " +
			"go2cs changed it: `// Converted from Go source: \"doc/md_docs.go\" in module " +
			"github.com/spf13/cobra@v1.10.2.`, then `// Modified from the original by conversion to C# with go2cs " +
			"<release>.` It goes after the file's upstream copyright and license comments, before the package doc " +
			"comment. The notice carries no timestamp, so the output stays the same from run to run. A module whose " +
			"license asks modified files to say so (Apache-2.0) is packed with it ([LICENSING.md](../LICENSING.md)).",
	},
	{
		// The flag's usage text before this table (main.go, master cb6def82a2), kept as history:
		//   Record a main-module source in the converted position maps the way `go build -trimpath` bakes it, <module
		//   path>/<path in module>, so the output names no directory of this machine; -trimpath=false records the
		//   absolute source path, as an untrimmed `go build` does
		Name: "trimpath", Value: "", Class: Advanced, Group: "Output",
		Summary: "Record main-module sources as go build -trimpath does (default on)",
		Details: "" +
			"On by default: a converted position map records a main-module source the way `go build -trimpath` bakes " +
			"it, as `<module path>/<path in module>`, so a converted stack trace names the file in that form and the " +
			"output names no directory of this machine. `-trimpath=false` records the absolute source path, as an " +
			"untrimmed `go build` does. A standard-library or module-cache source is recorded the same way either " +
			"way.",
	},
	{
		// The flag's usage text before this table (main.go, master cb6def82a2), kept as history:
		//   Override inline-array admission per package (comma-separated import-path patterns, each an exact path or a
		//   /... prefix). A pattern forces its packages ON with the declaration-local population: every struct the
		//   package declares or spells that holds an array and passes the caps holds its arrays inline (go.arrays). A
		//   -<pattern> turns admission OFF. With no override, a struct is admitted when its own package converts it
		//   whole through unsafe.Pointer (the auto rule); the standard library is never admitted
		Name: "inline-arrays", Value: "<pattern>[,<pattern>...]", Class: Advanced, Group: "Output",
		Summary: "Override which packages' structs hold their arrays inline",
		Details: "" +
			"Override which struct types hold their Go arrays INLINE (the `go.arrays` family, so a struct read from " +
			"native memory or reinterpreted through `unsafe.Pointer` is Go's bytes) rather than as golib's " +
			"`array<T>`. With no override the rule is automatic: a struct is admitted when its own package's " +
			"non-test files convert it whole through `unsafe.Pointer`, together with every struct it holds by value, " +
			"each within the caps (4096 bytes and 4096 elements per array, 4096 bytes per struct), unless those " +
			"files use one of its arrays in a way an inline array cannot express (sliced, its address or an address " +
			"inside it taken, or used as a whole value other than as an interface argument). A pattern (an import " +
			"path, or a `/...` prefix) forces its packages ON with every struct they declare or spell that holds an " +
			"array; `-<pattern>` turns the rule OFF for them. An entry `<import path>#<TypeName>` seeds ONE type " +
			"instead: that package admits the named type or alias as if its own files converted it whole through " +
			"`unsafe.Pointer`, under the automatic rule (the same caps, the same use check, the same `GO2CS2009`), " +
			"and nothing else in the package changes. Use it for a type that another package converts whole and " +
			"passes back by value. A `-recurse` conversion of `modernc.org/sqlite` for Windows derives the seed " +
			"`modernc.org/libc#largeInteger` itself; converting `modernc.org/libc` on its own for Windows needs " +
			"`-inline-arrays=modernc.org/libc#largeInteger` ([known " +
			"issue](KnownIssues.md#converting-modernc-sqlite-for-windows-needs-a-seed-for-libcs-largeinteger)).<!-- " +
			"2026-11-23 .5 docs pass (P1): the derived seed landed in merge 00b0dc37dd (KnownIssues.md, same pass); " +
			"1.26.11.1 still needs the flag for the -recurse conversion too, as that entry says. --> A name the " +
			"package does not declare selects nothing, so one value serves every target. A `-recurse` run also " +
			"derives seeds itself: an import is seeded with each alias whose identical anonymous struct an importer " +
			"admits " +
			"([how](ConversionStrategies-Reference/slices-and-arrays.md#a-struct-read-from-native-memory-holds-its-arrays-inline-and-seeds-make-two-packages-agree)); " +
			"a seed named here and derived there is one seed, and `-<pattern>` beats both. The standard library is " +
			"never admitted. A selected type a cap keeps out converts as before with warning `GO2CS2009`. A " +
			"reinterpret chain whose two sides disagree is made to agree under the automatic rule, by keeping the " +
			"admitted side out with its partner (also `GO2CS2009`), and is refused with `GO2CS2010` when the package " +
			"is forced or the admitted side belongs to another package. The decision is recorded in the package's " +
			"`package_info.cs`: a `-tests` conversion applies the recorded rule to the package's non-test files, so " +
			"the option need not be repeated there, and refuses a record those files do not reproduce (the named " +
			"types, a count and digest of every admitted type, anonymous ones included, a digest of each admitted " +
			"anonymous type, and the seeds). A `-tests` conversion of a package that has no record while " +
			"`-inline-arrays` names it is refused too: the rule its production output was converted under cannot be " +
			"known. See [diagnostic codes](DiagnosticCodes.md).",
	},
	{
		// The flag's usage text before this table (main.go, master cb6def82a2), kept as history:
		//   Downgrade the listed converter refusal codes to warnings (comma-separated), so a package holding a refused
		//   site still converts; each downgraded site fails loud, a panic carrying the refusal text, when reached, and
		//   the build reports it (GO2CS0011). Opt-in per code, never a default; only GO2CS2002 is allowed (an unknown
		//   or disallowed code is an error that names it)
		Name: "warn", Value: "<code>[,<code>...]", Class: Advanced, Group: "Output",
		Summary: "Downgrade the listed converter refusal codes to warnings",
		Details: "" +
			"Downgrade the listed converter refusals to warnings, so a package that holds a refused site still " +
			"converts and the rest of it runs. Opt-in per code, never a default. A code is allowed only when every " +
			"site it covers fails LOUD when reached: the converted program panics there with the refusal text, and " +
			"never runs on with a silently wrong result. Today that is `GO2CS2002` alone (a self-referential " +
			"constraint proxy method that cannot forward its storage-sharing value). Each downgraded site is printed " +
			"as `warning GO2CS2002: ...` (exit 0) and recorded in package_info.cs, and every C# build of the project " +
			"then reports it as `GO2CS0011`, so a project nobody converts again still shows it carries a " +
			"known-broken path. Any other code, an unknown one or a refusal that cannot fail loud, is an error " +
			"that names it and the reason. See [diagnostic codes](DiagnosticCodes.md).",
	},
	{
		// The flag's usage text before this table (main.go, master cb6def82a2), kept as history:
		//   With -stdlib and two or more -platforms targets: convert once per target into an isolated seeded staging
		//   root under this directory, classify the emissions (shared/variant/partial/exclusive) and write
		//   platform-manifest.json there. Emits NO corpus output
		Name: "platform-census", Value: "<dir>", Class: Advanced, Group: "Multi-platform and census",
		Summary: "With -stdlib and several -platforms: write a per-target emission census here",
		Details: "" +
			"With `-stdlib` and two or more `-platforms` targets: convert once per target into an isolated, seeded " +
			"staging root under `<dir>`, compare what each run actually emitted, and write " +
			"`<dir>\\platform-manifest.json` classifying every artifact as shared, variant, partial or " +
			"platform-exclusive. Produces **no** converted output of its own — `-go2cspath` is read as the seed and " +
			"never written to.",
	},
	{
		// The flag's usage text before this table (main.go, master cb6def82a2), kept as history:
		//   Directory a multi-platform -stdlib EMISSION stages its per-target conversions in (kept afterwards for
		//   inspection); when omitted, the run stages in .go2cs-platform-stage under the output root, reclaims anything
		//   an earlier run left there, and removes it at the end
		Name: "platform-stage", Value: "<dir>", Class: Advanced, Group: "Multi-platform and census",
		Summary: "With -stdlib and several -platforms: stage the per-target conversions here",
		Details: "" +
			"With `-stdlib` and two or more `-platforms` targets: the directory the per-target conversions are " +
			"staged in, kept afterwards for inspection. Without it, the run stages in `.go2cs-platform-stage` under " +
			"the output root and removes it at the end.",
	},
	{
		// The flag's usage text before this table (main.go, master cb6def82a2), kept as history:
		//   With -stdlib: run the ж-box A1 ref-lowering classification census (analysis only, never emits) over the
		//   standard library — once per -platforms target — and write the JSON report to this path. See
		//   docs/phase4/DESIGN-zh-box-reduction.md §9 stage A1
		Name: "ref-census", Value: "<file>", Class: Advanced, Group: "Multi-platform and census",
		Summary: "With -stdlib: write the box-to-reference lowering census to this file",
		Details: "" +
			"With `-stdlib`: run an analysis-only census of how the converter would lower heap boxes to references, " +
			"over the standard library and once per `-platforms` target, and write its JSON report to this file. It " +
			"emits no C#.",
	},
	{
		// The flag's usage text before this table (main.go, master cb6def82a2), kept as history:
		//   Show parse tree
		Name: "tree", Value: "", Class: Advanced, Group: "Diagnostics",
		Summary: "Print each file's Go parse tree during conversion",
		Details: "" +
			"Print each file's Go parse tree (`go/ast`) to stdout during conversion — a diagnostic aid.",
	},
	{
		// The flag's usage text before this table (main.go, master cb6def82a2), kept as history:
		//   Enable debug mode
		Name: "debug", Value: "", Class: Advanced, Group: "Diagnostics",
		Summary: "Let a conversion failure crash with a full stack trace",
		Details: "" +
			"Disable the converter's per-file panic recovery, so a conversion failure crashes with a full stack " +
			"trace, instead of being recovered, reported and skipped (the run then ends with `GO2CS2008`, naming the " +
			"file).",
	},
	{
		// The flag's usage text before this table (main.go, master cb6def82a2), kept as history:
		//   Experimental, for converter development: eligible pointer-receiver methods also emit a form that takes the
		//   receiver by reference, beside the default form. Off by default; output made with it is not validated
		Name: "dual-recv", Value: "", Class: Experimental, Group: "Development",
		Summary: "Also emit by-reference forms of eligible pointer-receiver methods",
		Details: "" +
			"Experimental, for converter development: eligible pointer-receiver methods also emit a form that takes " +
			"the receiver by reference, beside the default form. Off by default; output made with it is not " +
			"validated.",
	},
	{
		// The flag's usage text before this table (main.go, master cb6def82a2), kept as history:
		//   Experimental, requires -dual-recv: those forms also lower their pointer parameters, and call sites choose
		//   between the two forms. Off by default
		Name: "dual-recv-params", Value: "", Class: Experimental, Group: "Development",
		Summary: "With -dual-recv: those forms also lower their pointer parameters",
		Details: "" +
			"Experimental, requires `-dual-recv`: those forms also lower their pointer parameters, and call sites " +
			"choose between the two forms. Off by default.",
	},
}
