# FINDING — the first real third-party modules through the `-tests` pipeline: x/sync and x/mod

**Record type:** FINDING (point-in-time). Amend with dated blocks; do not rewrite.
**Lane:** P1. **Asked by:** COORD, 2026-10-02. **Read at:** the L union `02a0b44467` (every run below), go1.24.13 as
`GOROOT` with `GOTOOLCHAIN=local`, Microsoft .NET 10.0.12, linux/amd64, Release, `GoTargetOS` unset in the driver.
**Sources:** P1's posts to COORD, named by their mailbox file stamp, and the run logs named per section. Each number is
as the run printed it. Nothing here is a roster row: a third-party proof page is written beside the conversion, not under
`docs/validation/current`.

The runs read two real modules, both from the local module cache with `GOPROXY=off` and no download, copied to a
writable directory outside any clone, with a fresh output root outside it.

## 1. Commands

| Run | Command (from a built converter; `<tree>` is a checkout at `02a0b44467`) |
|:--|:--|
| Whole module | `go2cs -tests -recurse -test-action all -test-config Release -test-timeout 20m -go2cspath <tree>/src <moduleCopy> <outRoot>` |
| Single package (x/sync only) | `go2cs -tests -test-action all -test-config Release -test-timeout 10m -go2cspath <tree>/src <package dir> <outRoot>` |

`-test-timeout 20m` is not optional on a fresh tree: see F4.

## 2. golang.org/x/sync@v0.19.0 (whole module, post `20261002T011150Z`)

- **Closure:** 64 packages discovered, converting 4 app + 0 third-party, referencing 59 stdlib (1 skipped). 4 of 4
  convert (2.88 s on the run that counts).
- **MODULE.md:** one row, `errgroup` 5 matched, 0 disclosed. "Total: 5 matched · 0 disclosed across 1 package(s)". The three
  failed packages are absent from it. The run exits 1, "phase B: 3 of 4 package(s) failed".
- **Module directory:** sha256 of all 19 files identical before and after; no file newer than the snapshot.

| Package | Result |
|:--|:--|
| `errgroup` | **Validated 5 tests.** The same count as the single-package run in section 4. |
| `semaphore` | Go/C# mismatch on one test: `TestWeightedAcquire` Go=pass C#=fail. 7 of the 8 tests the run lists pass. Timing class, row T1. |
| `singleflight` | `dotnet publish` fails: `singleflight_test.cs(281,29)` CS1503. F1. |
| `syncmap` | `dotnet publish` fails: `map_test.cs(99,27)` and `map_bench_test.cs(22,161)` CS0246. F3. |

## 3. golang.org/x/mod@v0.33.0 (whole module, post `20261002T013852Z`)

- **Literal run refused before any conversion:** `go mod download preflight failed ... golang.org/x/tools@v0.41.0: module
  lookup disabled by GOPROXY=off`. The module's `go.mod` requires x/tools v0.41.0 (only `zip/zip_test.go` imports it); the
  cache holds v0.42.0. No download was attempted.
- **Stated deviation for the run that counts:** the working copy was pinned to the cached v0.42.0: `go.mod` one line
  (`require golang.org/x/tools v0.42.0`), `go.sum` two added lines. Nothing else changed: 0 files newer than the `go.sum`
  edit after the run; the module's own sources are byte-identical to the cache copy.
- **Closure:** 191 packages discovered, converting 11 app + 0 third-party, referencing 179 stdlib (1 skipped). 12 of 12
  convert (10.2 s; `golang.org/x/tools/txtar` is one of the 12).
- **MODULE.md:** `module` 16, `semver` 9, `sumdb/note` 7, `sumdb/storage` 1. "Total: 33 matched · 0 disclosed across 4
  package(s)". The run exits 1, "phase B: 5 of 9 package(s) failed".
- **Excluded declarations:** `semver` 1 (`BenchmarkCompare`), `sumdb/note` 4. The proof pages read "0 disclosed" for them;
  the log's "disclosed-unsupported declarations excluded" counts the same declarations.

| Package | Result |
|:--|:--|
| `module` | Validated 16 |
| `semver` | Validated 9 |
| `sumdb/note` | Validated 7 |
| `sumdb/storage` | Validated 1 |
| `modfile` | `rule.cs(397,20)`, `(613,20)`, `(736,20)` CS0103. F5. |
| `sumdb/dirhash` | `hash.cs(87,24)` CS1503. F6. |
| `sumdb/tlog` | `tlog_test.cs(285,9)` CS0034. F7. |
| `sumdb` | `client_test.cs` lines 81, 88, 101, 138, 187, 203, 209, 313: CS0012 then CS8130/CS8183. F8. |
| `zip` | Blocked, not its own defect: `zip.go` imports `modfile`, so its build stops on F5's errors. Its own test code is unread. |

## 4. Findings F1 to F8

All eight are converter or harness defects found by reading real code. Each has a RED guard on
`claude/p1-m-repros` (first commit `9702981c33`, second `6f2c0cc460`), proven FROM THE COMMITTED BYTES at `02a0b44467`.
Status for all eight: **queued for TRAIN M** (COORD, `20261002T011332Z` and `20261002T020717Z`).

| Id | Package | Error | The Go construct | The emitted C# shape | Class | Repro project |
|:--|:--|:--|:--|:--|:--|:--|
| F1 | x/sync `singleflight` | CS1503 `singleflight_test.cs(281,29)`: Action to `Func<(object, go.error)>` | `singleflight_test.go:222-224`, `TestPanicDo`: `fn := func() (interface{}, error) { panic(...) }` then `g.Do("key", fn)` | `var fn = () => { throw panic(...); };`, which C# types as Action | converter emission | `PanicOnlyFuncLiteralVar` (`main.cs(30,25)`) |
| F2 | x/sync `semaphore` | CS0234 `errgroup_package` missing | the test imports the sibling `errgroup` | the sibling was never converted | **closed, not a defect at L**: the single-package `-tests` flow does not convert siblings; M3's `-tests -recurse` does, and semaphore's import resolved there | none |
| F3 | x/sync `syncmap` | CS0246 `sync_syncmapꓸMapжmapInterface` | `syncmap/map.go` declares `type Map = sync.Map`; the tests pass a `*syncmap.Map` to their own interface `mapInterface` (`map_test.go:90` `applyCalls(new(syncmap.Map), calls)`, `map_bench_test.go:22` `&syncmap.Map{}`; the interface is `map_reference_test.go:15`) | `new sync_syncmapꓸMapжmapInterface(@new<sync.Map>())`, a pointer-to-struct-to-interface adapter that nothing generates | converter emission | `AliasStructToInterface` + `AliasStructToInterfaceLib` (`main.cs(14,16)`). Control: `&sync.Map{}` directly emits `sync_MapжmapInterface` and builds. The real defect is in an external `_test` package, which Behavioral cannot express; same error, same name shape; the fix seat confirms against syncmap |
| F4 | all | `dotnet timed out after 2m0s` on every package | none: a fresh worktree | the per-child default is 2 m and the first `dotnet publish` builds the stdlib closure | harness default | none; `tL-modules-legs.sh` passes `-test-timeout 20m` |
| F5 | x/mod `modfile` | CS0103 `argsʗp`, `argsΔ1` | `rule.go:371`, `:577`, `:688`: `errorf := func(format string, args ...any) {...}` inside `func (f *File) add(..., args []string, ...)` | `void errorf(@string format, params ꓸꓸꓸany argsΔ1ʗp) { var args = argsʗp.sslice(); ...argsΔ1.ꓸꓸꓸ }`: renamed parameter, body reads the old names | converter emission | `VariadicClosureShadowParam` (`main.cs(10,20)`, `(11,41)`) |
| F6 | x/mod `sumdb/dirhash` | CS1503 `hash.cs(87,24)` | `hash.go:75-78`: `osOpen := func(name string) (io.ReadCloser, error) { return os.Open(...) }` then `hash(files, osOpen)` | the literal's natural type is `Func<@string,(os_FileжReadCloser, error)>`, not the declared result list | converter emission; **shares F1's root** (a `:=`-bound function literal takes its natural type from its body) | `FuncLiteralDeclaredResultIface` (`main.cs(22,9)`) |
| F7 | x/mod `sumdb/tlog` | CS0034 `tlog_test.cs(285,9)` | `tlog_test.go:285`: `if h != sha256.Sum256(nil)` with `type Hash [HashSize]byte` | `h != sha256.Sum256(...)` is ambiguous between `tlog_package.Hash` and `array<byte>` | converter emission | `NamedArrayVsUnnamedCompare` (`main.cs(23,9)`); a local function returning `[4]byte` stands in for `sha256.Sum256` |
| F8 | x/mod `sumdb` | CS0012 `module_package.Version` not referenced, then CS8130/CS8183 | `sumdb/test.go` `TestServer.Lookup(ctx, m module.Version)` beside `Client.Lookup(path, vers string)`; the test calls `client.Lookup(a, b)` | both are `Lookup` extension methods of the package class; the same arity makes both candidates, ranking the TestServer one needs module's assembly, and the test project's references (note, tlog, sumdb) omit it under `DisableTransitiveProjectReferences` | harness / project wiring | `TestNeedsTransitiveApiRef`, a module directory (it needs a `_test` file); command in the commit message; `b_test.cs(15,24)` |

**F8 mechanism, as bisected:** on a reduced x/mod copy, deleting `sumdb/test.go` removes every CS0012, and the reduced
`sumdb` then validates 4 tests (the count is from the reduced copy, not the real package). Two shapes that compile CLEAN
and are NOT the defect: an overload of a different arity, and an interface naming the type.

## 5. Rows that are not defects

| Id | What | Reading |
|:--|:--|:--|
| T1 | `semaphore` `TestWeightedAcquire`: gives each `Acquire` a 10 ms context timeout | Fails only on the first, cold call at `TieredCompilation=0` (the Release default), because JIT of that path exceeds 10 ms. On the published host: 3 of 3 cold runs FAIL at TC0; `-test.count=3` at TC0 FAILS iteration 1 and PASSES 2 and 3; with tiering on it PASSES cold. COORD ruled it the TC0 first-call-latency class (log/slog's `TestSetDefault` is the same): a design item for user modules, not a defect. |
| H1 | F4 above | A default that cannot finish a first build on a fresh checkout reads as N package failures. Queued with the fix for TRAIN M. |

## 6. The non-root red/green for G's module-cache fix (post `20261002T005145Z`)

**Verdict rule, written before any run:** the fix is proven non-root on linux if RED fails IN STAGING with a permission
error and FIX, same uid and cache, gets past staging.

- **Setup:** a throwaway non-root user (uid 1001, gid 1002; a root run masks this defect). Its `GOMODCACHE` held a copy of
  `golang.org/x/sync@v0.19.0` owned by that user, files 0444 (19), directories 0555 (5), `GOPROXY=off`; modes read back
  identical before RED, after RED and after FIX. A user-owned copy of the go1.24.13 toolchain, of the Microsoft .NET
  10.0.12 root and of the NuGet cache; each commit as its own `git archive` tree with its own converter built as that user.
- **Package:** `errgroup`. `singleflight` stops at F1 and `semaphore` at F2 (single-package flow) before any test host runs,
  so errgroup was the first of the three to reach the host.
- **RED CONTROL `f8f911df00`:** rc 1. The test host died in staging: `System.UnauthorizedAccessException: Access to the path
  .../go2cs-tests/golang.org_x_sync_errgroup/<hash>/src/golang.org/x/sync/errgroup/errgroup.go is denied`, inner
  `IOException: Permission denied`, at `FileSystem.CopyFile` from `TestHost.CopyFixtures` (`src/core/testing/TestHost.cs:1131`)
  from `TestHost.Run` (`:254`). The record carried `infrastructure-error` and the 5 tests Go=pass, C#=empty.
- **FIX `02a0b44467`:** rc 0. "Validated 5 tests against go test (0 skipped identically on both sides, 4 disclosed-unsupported
  declarations excluded)." The record: status validated, matched true, `targetGOOS` linux, Release, tiered false, oracle go1.24.13.
- **Sandbox:** removed by the host's own cleanup in both arms, including the failed RED arm: the per-run hash directory was
  gone and only the empty package directory remained.
- **The two trees differ only** in `src/core/testing/PackageAncestry.cs`, `src/tests/GolibTests/ModuleAncestryTests.cs` and
  `docs/README.md`, so the change from red to green is the `CopyWritable` fix.
- **Verdict:** rule met.

## 7. Corrections made on the way

1. P1's first x/sync post named `singleflight_test.go:278` (`_, err, _ = g.Do("key", fn)`) as F1's Go line. It is
   `TestPanicDo`, lines 222-224; the tuple-assign goroutine converts clean. Corrected in `20261002T012518Z`.
2. P1's x/mod post said the sumdb `Client` API takes `module.Version`. It does not; the mechanism is row F8.
   Corrected in `20261002T020631Z`.
3. P1's x/sync post said "11 of 12 pass" for `semaphore`. The run lists 8 tests, 7 pass and 1 fails. Corrected here.
4. The same post's F3 line named `&sync.Map{}` as the converted expression; the source is `new(syncmap.Map)` (`map_test.go:90`).
5. The same post gave the x/sync conversion time as 3.05 s. That was the first run, which timed out in the test phase;
   the run that counts converted in 2.88 s.

## 8. Not measured

windows and darwin for every row; a warm-tree timing; `zip`'s own test code (blocked by F5); whether F7 reproduces with
`sha256.Sum256` itself rather than a local function (expected, not built); the behavioral runner on the eight guards
(unregistered, no goldens, as ruled).
