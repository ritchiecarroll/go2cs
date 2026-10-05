# Converted Go Test Infrastructure Requirements

## Status

The design behind the converted-test pipeline. Phases 4A to 4C are implemented as the `-tests`
pipeline; Phase 4D is planned ([Roadmap](Roadmap.md#phase-4d-examples-fuzz-seed-corpora-and-benchmarks)).

## 1. Purpose

The infrastructure shall convert and execute Go package tests in C# so that a converted package can
be validated against the same tests used by its Go implementation. Its immediate purpose is to make
the later replacement of assembly/cgo stubs with C# implementations evidence-driven: an implementation
is not considered working merely because it compiles; it should pass the applicable converted upstream
tests.

The key success statement is:

> For a selected Go package and target platform, the same eligible Go tests pass in Go and in the
> converted C# package, with failures reported under the same Go test names.

This supplements rather than replaces the existing behavioral suite. The behavioral suite protects
individual converter and runtime semantics; converted upstream tests validate whole-package behavior.

## 2. Design principles

1. **Preserve Go's test compilation model.** A same-package test is compiled with the package sources,
   not against a previously compiled package binary.
2. **Keep tests colocated.** Converted `*_test.go` files, the library project, and the test project live
   in the converted package directory.
3. **Keep production artifacts clean.** The library project never compiles test sources, test metadata,
   or the generated test host.
4. **Use Go metadata, not filename guessing.** Package loading, build constraints, internal versus
   external test packages, imports, and test discovery come from `go/packages` and the typed AST.
5. **Model Go semantics in a thin compatibility runtime.** Do not rewrite assertions into a C# test
   framework and do not depend on the full converted `testing` package for the first implementation.
6. **Run each package in an isolated process.** This preserves package globals, `init`, `TestMain`,
   working-directory behavior, environment changes, and `os.Exit` without endangering an orchestrator.
7. **Report exclusions honestly.** Unsupported, filtered, or platform-ineligible tests must be visible;
   they must never silently count as passing.
8. **Make generated output deterministic.** Equivalent inputs, Go version, converter version, and target
   platform produce byte-stable C# sources, project files, manifests, and discovery order.

## 3. Required package layout

For a package such as `unicode/utf8`, conversion shall produce a layout equivalent to:

```text
unicode/utf8/
  utf8.cs
  utf8_test.cs
  example_test.cs
  package_info.cs
  package_test_info.cs
  go2cs_test_host.cs
  unicode.utf8.csproj
  unicode.utf8.tests.csproj
  testdata/
```

The names are normative:

- `x_test.go` converts to `x_test.cs`.
- The production project remains `<package>.csproj`.
- The test project is `<package>.tests.csproj`.
- Test-only package metadata is `package_test_info.cs`.
- The generated entry point/registry is `go2cs_test_host.cs`.

The production project shall include production converted files and hand-owned companions such as
`doc_impl.cs`, while explicitly excluding `*_test.cs`, `package_test_info.cs`, and
`go2cs_test_host.cs`. The existing `*._test.cs` exclusion is not sufficient for the converter's actual
`x_test.go` to `x_test.cs` naming and must not be relied upon.

The test project shall compile the production source files again together with the eligible converted
test files and test-only generated files. It shall **not** reference the package's production project.
This is intentional: same-package Go tests must share the package's partial C# class and must be able to
use unexported declarations. Both projects shall reference the same `golib`, source generators, package
dependencies, and hand-written implementation companions.

Each project shall use explicit include/exclude rules with default SDK compile inclusion disabled or
equivalently precise item rules. A stale test conversion must not enter either compilation accidentally.

## 4. Conversion and package loading requirements

### 4.1 Opt-in mode

Test conversion shall be opt-in through a dedicated converter mode. Normal conversion shall retain its
current production-only behavior and performance. The command surface shall support:

- converting production plus tests for one package;
- converting production plus tests for a filtered standard-library package set;
- selecting GOOS/GOARCH consistently for production and tests;
- converting without executing; and
- building/running already-converted tests without reconverting.

Exact flag names are an implementation decision, but `go2cs test` or a `-tests` option should be the
user-facing shape rather than a separate ad hoc utility.

### 4.2 Go package variants

The loader shall use `packages.Config.Tests = true` with sufficient modes for syntax, types, imports,
compiled files, and module/package identity. It shall distinguish:

- production package `p`;
- the augmented same-package test variant containing `package p` tests; and
- external tests declared as `package p_test`.

One C# test executable shall contain both same-package and external-package tests for the Go package.
The converter shall consolidate their test-only aliases, implementation attributes, and imports without
emitting conflicting assembly metadata or duplicate package declarations. An external test's import of
the package under test shall bind to the production sources in the same test compilation, not add a
self-referencing `ProjectReference`.

### 4.3 File eligibility

Test file selection shall honor the same build tags, filename GOOS/GOARCH suffixes, cgo policy, and Go
version as the corresponding `go test` invocation. Discovery shall use the loaded package's compiled
file lists; scanning every `*_test.go` in the directory is insufficient.

The conversion manifest shall record every discovered test source as included, platform-excluded,
unsupported, or failed-to-convert, with a reason.

### 4.4 Test declaration discovery

The typed AST shall discover valid Go declarations according to Go's rules:

- `func TestX(*testing.T)`;
- `func TestMain(*testing.M)`; and
- subtests created dynamically through `T.Run`.

Discovery must not be based only on the `Test` prefix. Invalid signatures remain ordinary functions
and shall not be registered.

Benchmarks, fuzz targets, and executable examples are staged work described in section 11 and shall be
reported as unsupported until their stage is implemented.

## 5. Test execution architecture

### 5.1 Go-native host, not assertion rewriting

The generated test project shall be an executable package test host. Its generated registry maps stable
Go test names to converted delegates. A hand-written compatibility assembly shall provide the
`go.testing` API used by converted tests and the host scheduler.

The primary execution path shall not translate `t.Errorf` into MSTest/xUnit assertions. That approach
cannot faithfully model `FailNow`, subtests, `Parallel`, `Cleanup`, or `TestMain`. The host may later
publish results through a .NET test adapter, but Go test semantics remain owned by the compatibility
runtime.

The minimum direct command shall be equivalent to:

```text
dotnet run --project unicode.utf8.tests.csproj -- --json
```

A repository-level orchestrator shall select packages, build test projects, execute one child process
per package, enforce timeouts, aggregate results, and return nonzero if any required package fails or
has an unexpected exclusion. It shall be able to emit a machine-readable result file and a common CI
format such as JUnit XML or TRX.

The host's command line is the converted program's `os.Args`, so the pipeline launches it with the
arguments `go test -json` gives a Go test binary, and nothing else:

```text
<host> -test.v=test2json -test.timeout=20m0s [-test.run=<filter>] [-test.skip=<pattern>]
```

`-test.v=test2json` is the host's JSON mode (`--json` remains an accepted spelling). A package that
reads its own arguments then sees what Go's binary sees: cobra's `Execute` with no `SetArgs` parses
`os.Args[1:]`, and pflag skips every `-test.` flag. The pipeline's result and JUnit paths have no Go
flag, so they arrive in the environment as `GO2CS_TEST_RESULT` and `GO2CS_TEST_JUNIT`. The host reads
them once and removes them, so a test that re-executes the host does not write into the parent's
files. An explicit `--result` or `--junit` argument still wins, which is how the in-process test tier
passes them. `os.Args[0]` stays the real executable path, because tests re-execute themselves through it.

### 5.2 Isolation and fixtures

Each package host shall run in its own process and an isolated working directory. The working tree shall
preserve the relative layout expected by Go tests, including `testdata`, fixtures, and any source files
that the test intentionally reads. The original GOROOT or repository shall not be used as a writable
test directory.

**Shared (ancestor) fixtures.** "Relative layout" is not limited to the package's own `testdata/`. Go
shares large fixtures between sibling packages rather than duplicating them: `compress/{flate,zlib,lzw}`
all read `../testdata/{e,pi,gettysburg}.txt`, and `flate` also reads
`../../testdata/Isaac.Newton-Opticks.txt`. Eleven converted packages depend on such a path —
`compress/{flate,lzw,zlib}`, `image/{draw,gif,jpeg,png}`, `index/suffixarray`, `internal/zstd`, `net`.
Three parts make it work, and all three are required:

1. **Discovery** — the converter scans each package's `_test.go` sources for `"../"`-prefixed literals
   with a `testdata` path segment whose target exists, and stages them like any other fixture. They join
   the same list that feeds the input digest, so editing a shared fixture invalidates staleness normally.
2. **Build output** — such a path cannot keep its shape under the build output (MSBuild links a
   `..`-relative item by its BARE FILE NAME, which both flattens the two `testdata` trees together and
   loses the relative shape), so the emitted `csproj` gives it an explicit `Link` under a staging root,
   keyed by how many levels it ascends. Two ancestors may hold a same-named file, so the level count is
   part of the key.
3. **Sandbox shape** — the isolated working directory mirrors the package's WHOLE import path
   (`<runRoot>/compress/flate`), not just its last segment. That is what makes `../testdata/e.txt`
   resolve to a real location *inside* the sandbox, and it is Go's own layout, so the depth is always
   sufficient rather than guessed. The containment check is therefore against the run root, not the
   working directory — a shared fixture legitimately lands beside the package directory. Mirroring also
   preserves the property the flat form was chosen for (the working directory's parent holds nothing but
   the package, which `io/fs`'s `TestGlob` relies on) because each mirrored level holds only the next.

Staging only the package's own `testdata/` left these opens failing, which is what held `compress/flate`
at 61 of 64 tests and blocked `compress/zlib` and `compress/lzw` from validating at all.

**Nested (descendant) fixtures.** The same principle reaches DOWNWARD, and for the same reason: `go test`
runs a package in its real source directory, where the sibling packages nested under it are present with
their own `testdata` beside them. `internal/trace`'s `TestOldtrace` globs
`./internal/oldtrace/testdata/*_good` and drives one subtest per trace it finds. The staging created
`internal/` as an empty NAME (the sibling-shape pass, which reproduces the package directory's shape one
level deep), the glob matched nothing, and the parent failed on
`didn't see expected test case user_task_region_1_21_good` — 13 verdicts, the parent plus twelve subtests
that were never created. The rule is therefore: **every `testdata` directory below the package is staged
whole**, keeping its relative shape, with no reference analysis — the same rule the package's own
`testdata/` already gets, applied to the rest of the tree the package occupies on disk. Only `testdata` is
staged, never a nested package's sources: a sibling package is compiled and referenced as its own
assembly, and its own conversion run stages its files. Go's convention of keeping fixtures under
`testdata` is what bounds the cost — measured over all of `$GOROOT/src`, sixteen packages gain anything at
all, a few hundred files in total.

The orchestrator shall pass a controlled environment and record relevant values in the result manifest:
GOOS, GOARCH, Go version, .NET runtime, culture, timezone, test seed, package source revision, and
converter revision. Default culture shall be invariant unless the matched Go test explicitly requires
another locale.

Package and test timeouts shall terminate the child process and produce a timeout result, never hang the
full run.

## 6. `testing` compatibility requirements

Phase 4 shall add a hand-owned compatibility implementation for imports of `testing` from converted test
projects. Test projects shall reference this implementation instead of the full auto-converted
`core/testing` project. Production projects are unaffected. Testing the Go `testing` package
itself is deferred until the bootstrap dependency is resolved.

### 6.1 Required first-tier API

The first tier shall implement behaviorally compatible forms of:

- `T.Error`, `Errorf`, `Fail`, `FailNow`, `Failed`, `Fatal`, and `Fatalf`;
- `T.Log`, `Logf`, `Helper`, `Name`, `Cleanup`, and `Run`;
- `T.Skip`, `Skipf`, `SkipNow`, and `Skipped`;
- `T.TempDir` and `T.Setenv`;
- `T.Parallel` with Go's parent/subtest barrier behavior;
- `M.Run` and one optional valid `TestMain` per package; and
- test filtering, deterministic shuffle seed, count, verbosity, and timeout options needed for
  reproducible diagnosis.

`FailNow`, `SkipNow`, and test completion shall unwind converted defers before the host records the
result. They shall affect only the calling test goroutine. Calls from an invalid goroutine shall produce
a clear infrastructure failure rather than silently terminating unrelated work.

Cleanup functions shall run in last-in-first-out order after the test and all of its subtests complete,
including failure, skip, panic, and fail-now paths. Panics not recovered by converted Go code shall fail
the current test and retain the converted stack and Go source identity where available.

### 6.2 Additional API and capability reporting

`Deadline`, context support, `Chdir`, private/internal testing hooks, and less common APIs may be added as
real packages require them. Missing members must fail conversion or compilation with a categorized
capability diagnostic. They must not be emitted as throwing stubs that allow the package to be labeled
validated.

The runner shall publish a versioned capability list so package status can distinguish a product defect
from a test-runtime feature not yet implemented.

## 7. Result and differential-oracle requirements

The Go baseline shall be captured with a clean, uncached invocation equivalent to
`go test -json -count=1` using the same package, source revision, build tags, GOOS, and GOARCH. The C# host
shall emit normalized events containing at least:

- package and full hierarchical test name;
- start, pass, fail, skip, timeout, or infrastructure-error action;
- elapsed time;
- failure/log text;
- source file and line when known; and
- exclusion or unsupported-capability reason.

The comparison gate shall compare the eligible test set and terminal status by full Go test name.
Timing, stack formatting, temporary paths, and unordered log interleaving shall not be byte-compared.
The converted test's own assertions are the behavioral oracle; matching console output is only required
for Go examples with declared `Output:`/`Unordered output:` expectations when example support is added.

A package status shall be one of:

- **validated**: production and tests convert and compile; all eligible tests pass in Go and C#; no
  required test is silently omitted;
- **failing**: at least one eligible converted test fails or times out;
- **conversion-blocked**: production or test source does not convert/compile;
- **infrastructure-blocked**: a declared testing capability or fixture requirement is unsupported; or
- **not-applicable**: the package has no eligible tests for the target.

Only **validated** satisfies the Phase 5 behavioral gate.

## 8. Generated manifest and reproducibility

Every test conversion shall produce a deterministic, machine-readable manifest containing:

- package import path and C# project/assembly names;
- source Go version and target platform;
- production, internal-test, external-test, fixture, and hand-owned companion files;
- discovered top-level tests and `TestMain`;
- project dependencies, including test-only imports;
- test-runtime capability requirements;
- exclusions with reasons; and
- converter version/revision and a source-input digest.

The orchestrator shall reject a stale manifest when its input digest does not match the current sources,
options, converter, or hand-owned companions. Re-running conversion with unchanged inputs shall leave the
working tree unchanged.

## 9. Interaction with assembly-backed implementations

Phase 5 shall consume Phase 4 results package by package. A hand-written implementation such as
`doc_impl.cs` remains in the normal production file set and is therefore compiled identically by the
library and test projects.

An assembly-backed function or type implementation may be marked complete only when:

1. its production package compiles without a generated throwing partial stub for the implemented member;
2. the package's eligible converted tests pass;
3. the same upstream tests pass in Go for the matched platform; and
4. the result manifest shows no silent test omissions relevant to that implementation.

If upstream tests do not exercise the implementation, a focused Go test shall be added as a separate
go2cs behavioral fixture and run through the same converted-test host. A waiver for platform-specific or
unobservable behavior must be explicit, reviewed, and recorded in the package manifest/status; compilation
alone is never an implicit waiver.

## 10. Validation of the infrastructure itself

Before upstream standard-library tests are used as evidence, the harness shall have converter behavioral
fixtures covering at least:

- a passing test and each nonfatal/fatal failure path;
- panic, recover, defer execution, skip, and timeout;
- nested subtests, duplicate subtest names, cleanup order, and parallel barriers;
- same-package access to an unexported declaration;
- an external `package p_test` test importing the package under test;
- production and test-only imports/project references;
- `TestMain`, including nonzero return and `os.Exit`;
- `TempDir`, `Setenv`, relative files, and `testdata` isolation;
- build tags and GOOS/GOARCH filename selection;
- deterministic registration and manifests; and
- a deliberately unsupported API reported as infrastructure-blocked.

These fixtures belong in the existing behavioral test system where practical, with end-to-end host
fixtures added for behavior that cannot be proven by generated-code goldens alone.

## 11. Delivery stages

### Phase 4A — project model and serial test core

- Load test package variants and emit colocated test sources, manifest, and `.tests.csproj`.
- Implement the process-isolated host, registry, core failure/skip/log/cleanup semantics, fixtures, and
  structured results.
- Support same-package tests, external tests, and `TestMain` serially.

Exit criterion: all Phase 4A harness fixtures pass and at least one small, already-compiling standard-
library package is validated end to end.

### Phase 4B — subtests, parallelism, and package fixtures

- Implement `T.Run`, hierarchical filtering, Go-compatible cleanup and `T.Parallel` barriers.
- Add isolated working directories, `testdata`, environment control, timeouts, and Go/C# result
  comparison.

Exit criterion: representative leaf packages using subtests, parallel tests, and file fixtures are
validated reproducibly.

### Phase 4C — coverage expansion

- Expand the compatibility API from real failure data.
- Add package batching, status dashboards/manifests, CI sharding, retry diagnostics for suspected flakes,
  and common CI result output.
- Work bottom-up through compiling standard-library packages.

Exit criterion: every compiling package is classified; none is reported validated with an undisclosed
test exclusion.

### Phase 4D — later test kinds

- Convert runnable `Example` functions and verify declared output.
- Add fuzz seed-corpus execution as deterministic tests before considering active fuzzing.
- Add benchmark compilation/execution for compatibility, while keeping performance equivalence outside
  the correctness gate.

Benchmarks, active fuzzing, race-detector equivalence, coverage-percentage equivalence, and native C/C++
test harnesses are not prerequisites for beginning Phase 5.

## 12. Acceptance criteria for Phase 4

Phase 4 is complete enough to gate Phase 5 when all of the following are true:

1. A single documented command can convert, build, and run a selected package's tests in an isolated
   child process.
2. Production and test projects coexist in the package directory and compile mutually exclusive intended
   file sets; the production artifact contains no test code.
3. Same-package and external-package tests work, including unexported access and self-import handling.
4. Core `testing.T`, subtest, cleanup, parallel, skip, panic, timeout, and `TestMain` semantics are guarded
   by end-to-end fixtures.
5. Test-only dependencies, source generators, `golib`, and hand-written `*_impl.cs` companions resolve in
   the test project exactly as required.
6. Go and C# runs produce comparable structured results by full test name and expose every exclusion.
7. Repeated conversion is deterministic and stale outputs/manifests are detected.
8. At least three representative leaf standard-library packages are **validated**, including one with an
   external test package and one with subtests or parallel tests.
9. CI can aggregate package results and fails on test failure, timeout, conversion failure, stale input,
   or unexpected exclusion.
10. The Roadmap records Phase 4 package status and Phase 5 uses **validated**, not merely **compiles**, as
    its default completion gate.

## 13. Explicit non-goals for the initial implementation

- Replacing the current behavioral converter suite.
- Requiring byte-identical Go and C# logs, stack traces, or timings.
- Running the entire standard library in one process or one monolithic test project.
- Making the auto-converted implementation of the `testing` package the bootstrap test runtime.
- Claiming semantic equivalence for skipped, unsupported, or non-converted tests.
- Treating benchmark speed, fuzz discovery, race-detector output, or coverage percentage as Phase 4A/4B
  correctness gates.
- Solving platform-specific native integration that is unrelated to the selected package's managed C#
  behavior.

## 14. Recommended roadmap boundary

Roadmap Phase 4 should be named **Convert and run Go package tests** and should end at the acceptance
criteria above. Roadmap Phase 5 should be named **Implement assembly-backed declarations in C#** and use
the Phase 4 package-validation status as its evidence trail. This ordering turns the standard library's
own tests into the proving ground for each assembly conversion rather than a retrospective check after
many implementations have accumulated.
