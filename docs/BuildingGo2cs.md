# Building go2cs from source

This page is for developers who want to build the converter themselves, check its results, or work on
it: how to build it, how to run its tests, how to validate a converted package against `go test`, what
its error codes mean, and where things live in the repository.

## Build the converter

You need the Go release and the .NET SDK named in the README's [Requirements](README.md#requirements),
with your Go environment set up (`GOROOT`/`GOPATH`). Converted projects target the framework named by
[`src/Directory.Build.props`](https://github.com/ritchiecarroll/go2cs/blob/master/src/Directory.Build.props).
The converter's module path is plain `go2cs`, so it cannot be installed by URL: clone the repository and
install from the clone, as the README's [Installing the converter](README.md#installing-the-converter)
shows. Go produces a self-contained native binary; to target another
platform, use Go's standard cross-compilation (`GOOS`/`GOARCH`). Rebuild after every pull, with the
same Go release you convert with.

`go2cs -version` prints which release the binary belongs to and which Go toolchain built it, one
`key=value` line per key ([Command-line options](CommandLine.md#options)). Paste it into a bug report.

## Run the converter's own tests

```shell
cd go2cs/src/go2cs
go test ./...
```

The per-feature Go-versus-C# tests live under `src/tests/Behavioral`: each one is transpiled, compiled
and compared against a committed expected output, and each runnable one is executed with its output
compared against Go's. [`CONTRIBUTING.md`](https://github.com/ritchiecarroll/go2cs/blob/master/CONTRIBUTING.md)
says how to run them before you send a change.

Three focused checks run by hand when you change what they guard:

- `src/check-warning-entries.sh`: the per-file warning entries the converter writes match the compiler's
  warnings, in both directions.
- `src/tests/pack-paths-selftest.ps1`: the pack's path guard (`src/check-pack-paths.ps1`) names exactly
  the paths planted in fabricated packages.
- `src/tools/nugetgo/Test-NugetgoSelfDescription.ps1`: the module pack's self-description, on a
  fabricated conversion output.

## Validate a converted test suite

Every validated package ships its **converted C# test sources** next to the production code under
[`src/core`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core) (for example,
[`unicode/utf8/utf8_test.cs`](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/unicode/utf8/utf8_test.cs)),
so you can read the exact C# that runs — and re-run the validation yourself. You need the Go
release and the .NET SDK in the README's [Requirements](README.md#requirements) (Go for the
reference `go test` run), and `go2cs` on your `PATH` (see [Build the converter](#build-the-converter)).

Run it from the root of your clone. The first argument is the package's Go source, and the second is
the package's home in the converted tree; the converter locates the runtime and the package's
standard-library dependencies from there, so no other flags or environment setup are needed. The ten
minutes are the deadline for running the tests and comparing them with `go test`; building the test host
has its own, longer allowance, because the first build compiles the converted runtime and the package's
standard-library dependencies.

PowerShell, on Windows:

```powershell
go2cs -tests -test-action all -test-timeout 10m "$(go env GOROOT)\src\unicode\utf8" src/core/unicode/utf8
```

bash, on Linux and macOS:

```bash
go2cs -tests -test-action all -test-timeout 10m "$(go env GOROOT)/src/unicode/utf8" src/core/unicode/utf8
```

Expected final line:

```text
Validated 14 tests against go test (0 skipped identically on both sides, 1 disclosed-divergent (deferred), 49 disclosed-unsupported declarations excluded).
```
<!-- Run cold on linux/amd64 (4 vCPU, .NET SDK 10.0.112, empty NuGet and Go build caches) at ac1454281f by C2,
     2026-10-26, the bash form above: this line, rc 0, in 347 s. The 49 are go1.26.10's unicode/utf8 Benchmark,
     Example and Fuzz declarations: 16 in example_test.go and 33 in utf8_test.go. -->

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

<a id="diagnostics"></a>

The `GO2CS` error codes a conversion reports are listed in
[Command-line options, Diagnostics](CommandLine.md#diagnostics).

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

To build the whole converted standard library in Visual Studio, open the
[Go Standard Library Solution](https://github.com/ritchiecarroll/go2cs/blob/master/src/go2cs-stdlib.slnx).

## Contributing

Contributors: start with [`CONTRIBUTING.md`](https://github.com/ritchiecarroll/go2cs/blob/master/CONTRIBUTING.md),
then see [`Architecture.md`](Architecture.md), [`ConversionStrategies.md`](ConversionStrategies.md) and
[`Roadmap.md`](Roadmap.md) for details. There's plenty of low-hanging fruit here; jump in if you'd like to help.
