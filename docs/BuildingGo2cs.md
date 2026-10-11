# Building go2cs from source

This page is for developers who want to build the converter themselves, check its results, or work on
it: how to build it, how to run its tests, how to validate a converted package against `go test`, what
its error codes mean, and where things live in the repository.

## Build the converter

You need the Go release and the .NET SDK named in the README's [Requirements](README.md#requirements),
with your Go environment set up (`GOROOT`/`GOPATH`). Converted projects target the framework named by
[`src/Directory.Build.props`](https://github.com/ritchiecarroll/go2cs/blob/master/src/Directory.Build.props).
The converter's module path is plain `go2cs`, so it cannot be installed by URL: clone the repository,
then install from the clone.

```shell
git clone https://github.com/ritchiecarroll/go2cs.git
cd go2cs/src/go2cs
go install .
```

`go install` compiles the converter and puts the binary in `GOBIN` (or `GOPATH/bin`), which a standard
Go setup already has on its `PATH`. Go produces a self-contained native binary; to target another
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

## Validate a converted test suite

Every validated package ships its **converted C# test sources** next to the production code under
[`src/core`](https://github.com/ritchiecarroll/go2cs/tree/master/src/core) (for example,
[`unicode/utf8/utf8_test.cs`](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/unicode/utf8/utf8_test.cs)),
so you can read the exact C# that runs — and re-run the validation yourself. You need
**[Go 1.24.13](https://go.dev/dl/)** (for the reference `go test` run), the
**[.NET 10 SDK](https://dotnet.microsoft.com/download)**, and `go2cs` on your `PATH` (see
[Build the converter](#build-the-converter)).

Run it from the root of your clone. The first argument is the package's Go source, and the second is
the package's home in the converted tree; the converter locates the runtime and the package's
standard-library dependencies from there, so no other flags or environment setup are needed. The first
run builds the converted runtime and its dependencies, so it allows ten minutes.

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

## Diagnostics

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
