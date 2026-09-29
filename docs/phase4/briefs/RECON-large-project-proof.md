# RECON — the large-project proof: TypeScript 7.0's Go port

**Dated record, 2026-09-29. Lane R (S5), read-only, on `claude/coord-handover` only.** Low profile per the owner: no
master-facing text names this target. Web pages only (GitHub HTML, raw files and the API); nothing was cloned.
Disclosure: to grep the target, the research agent saved local copies of 649 of its `.go` files, each read as a raw
web page. That is effectively a partial source copy, which the brief asks to be cleared first. The copy was deleted
unused beyond the figures cited here (675 files, 52 MB).

Every figure below is at **microsoft/TypeScript `release-7.0` @ `abd7447ed5`** (2026-09-17, "Update release-7.0 for TS7
tagged releases"). Latest tag `v7.0.2`; `Herebyfile.mjs` pins `nativePreviewReleaseVersion = "7.0.3"`. The standalone
`microsoft/typescript-go` was **archived on 2026-09-01**; the port now lives in the TypeScript repo, and typescript-go
never had a `release-7.0` branch (it had `ts7-release` @ `9dd50136`).

## 1. Shape

- **Workspace:** `go.work` (`go 1.26`) over `./tsc` (the product) and `./tools` (lint and signing, a separate module).
- **Module** `github.com/microsoft/TypeScript/tsc`, `go 1.26`, **no `toolchain`, no `godebug` line.**
- **Size:** 96 package directories under `tsc/internal` plus `cmd/tsc`.
  - 4,932 `.go` files, of which 4,437 are `_test.go`.
  - 4,290 of those test files are in ONE package, `internal/fourslash/tests`.
  - About **296K non-test lines, ~46K of them generated**; about 54K test lines outside fourslash.
  - Measured by the research read; no source states a count.
- **Generated code** (26 files):
  - `ast_generated.go` (10K lines);
  - `lsp/lsproto/lsp_generated.go` (17K);
  - `diagnostics_generated.go` (8.6K, ~900 KB);
  - Unicode case/identifier tables;
  - the `api/encoder` pair;
  - `bundled/embed_generated.go`;
  - 10 stringer files and 3 moq mocks.
- **No cgo.** Assembly and `//go:linkname` appear only in `internal/fswatch`, darwin-only: FSEvents via
  `syscall.syscall6` and `cgo_import_dynamic`. Neither reaches the windows or linux targets.
- **Dependencies:**
  - direct (11): `golang.org/x/{sync,sys,term,text}`, `Microsoft/go-winio`, `go-json-experiment/json` (with `jsontext`),
    `google/go-cmp`, `mackerelio/go-osstat`, `peter-evans/patience`, `zeebo/xxh3`, `gotest.tools/v3`;
  - indirect: `klauspost/cpuid/v2`, `x/mod`, `x/tools`, `go-difflib`, `moq`;
  - `tool`: `moq` and `stringer` (build-time only).

## 2. Go features in use (the release it needs)

- **Go 1.26, used, not just declared:**
  - `new(expr)` (`fourslash/baselineutil.go:752`);
  - `errors.AsType` (`lsp/server.go:670`).
- **Go 1.25:**
  - `sync.WaitGroup.Go` (`core/workgroup.go:39`);
  - `testing/synctest` (`project/checkerpool_test.go`, `lsp/progress_test.go`).
- **Go 1.23 and 1.24:**
  - `iter.Seq`/yield (26 files) and range over maps/slices iterators (~39);
  - `strings.SplitSeq`/`Lines`;
  - `sync.OnceValue` (20 files) and `OnceFunc`;
  - `b.Loop()` (14 tests), `t.Context()` (13), `t.Chdir`;
  - `reflect.TypeFor`, range-over-int, `min`/`max`/`clear`, `unsafe.String`/`StringData`.
- **Not used:**
  - `weak`, `unique`, `os.Root`, `runtime.AddCleanup`/`SetFinalizer`, generic type aliases;
  - stdlib `encoding/json/v2`: it uses the EXTERNAL `go-json-experiment/json`, so no GOEXPERIMENT is needed.

**Consequence:** the target cannot be converted before our corpus reaches **Go 1.26, i.e. after hop D** (1.25 is hop C).
Even at 1.25 the two 1.26 sites would not compile.

## 3. Its imports against our corpus (banked roster at master `a1f133c3a9`, 222 rows)

- **Non-test std imports: 54, of which 48 are banked rows.**
  - `embed`, `encoding` and `unsafe` have no Go tests to bank, but they convert.
  - `runtime`, `runtime/debug` and `runtime/pprof` are testable and **not banked**. They convert and compile, but the
    runtime row is the corpus's open row, and `runtime/pprof`/`runtime/metrics` are measurement surfaces with
    disclosed divergences.
  - The rest are banked: `bufio`, `bytes`, `context`, `encoding/*`, `fmt`, `go/format`, `go/token`, `hash/fnv`, `io/fs`,
    `iter`, `math/big`, `math/rand/v2`, `net`, `net/url`, `os`, `os/exec`, `os/signal`, `path/filepath`, `reflect`,
    `regexp`, `slices`, `sort`, `strconv`, `sync`, `sync/atomic`, `syscall`, `testing`, `testing/fstest`, `time`,
    `unicode/*`, and others.
- **Test-only adds:** `math/rand` (banked) and **`testing/synctest`**. At 1.24.13 our `internal/synctest` is entirely
  stubbed, so the two synctest test files are blocked until a synctest body exists (a 1.25-era item).
- **Patterns that stress known gaps:**
  - **Goroutine = OS thread.** golib runs each goroutine on a dedicated thread with a 256 MB stack reservation, bounded
    near 10⁴ live goroutines (`golib/runtime/Goroutine.cs`). The checker pool defaults to 4 (cap 256), but parsing and
    loading fan out through `core.WorkGroup`/`ThrottleGroup` (`compiler/filesparser.go`, `fileloader.go`). How many
    goroutines a large program's parse spawns at once is the first thing to measure.
  - **Parallel tests:** the compiler runner's ~13K cases run with `t.Parallel()`.
  - **Scale of sync use:** `sync.Pool` (9 files, the parser and binder hot paths), atomics (37 files), mutexes (59),
    channels (27), `select` (18).
  - **Reflection:** `reflect` plus `go-json-experiment/json` (reflection-driven), and `go-cmp` in tests.
  - **`unsafe.String`/`StringData`** in the scanner/string paths.
- **Third-party dependencies are themselves conversions,** and several are not trivial:
  - `x/sys` (windows) and `go-winio` (named pipes, the LSP transport) lean on our syscall door;
  - `zeebo/xxh3` and `klauspost/cpuid` carry assembly with pure-Go fallbacks, so they must convert under the pure tags;
  - `go-json-experiment/json` is a large reflection library in its own right.

## 4. Its conformance suites (the proof of operation is its OWN tests passing)

- **Cases:** `tsc/testdata/tests/cases` holds 13,168 entries: **6,833 compiler, 5,927 conformance**, 22 transpile. They
  were PROMOTED from the upstream TypeScript repo (no submodule any more; `promotedTestCollisions.txt`).
- **Baselines:** `tsc/testdata/baselines/reference/` holds ~22.5K compiler and ~22K conformance files (`.js`, `.types`,
  `.symbols`, `.errors.txt`), 1,696 fourslash files, and config/tsc/tsbuild/watch trees.
- **Harness:** `internal/testrunner`, entry `TestLocal` (`compiler_runner_test.go`). Parallel cases, and **it skips when
  built `noembed`**, so the embedded lib `.d.ts` set is required.
  - Baselines are written to `baselines/local`, compared to `reference`, and diffed with `patience`.
  - `npx hereby test` is `go test ./...` / gotestsum under `tsc/`, with `GODEBUG=tracebackancestors=10` (diagnostic
    only) and `-count=1` on Windows.
- **Needs Node.js:** `test:api`, the `jstest`-backed tests (`jsnum`, `astnav`), `ls/lsconv/converters_test.go` (checks
  `LookPath` first), `vfs/osvfs/realpath_test.go` and `lsp/replay_test.go` (npm). These are disclosure candidates, not
  proof material.
- **fourslash** (4,290 LSP tests) is the heaviest single package and a later stage.
- **CI:** linux, windows and macos, plus race and noembed variants.

## 5. What it needs from us that does not exist yet

Most items are already on the roadmap's "Real-world Go modules" list, stepping stones first.
1. **Go 1.26 corpus: hop D.** Hard gate; see section 2.
2. **The multi-package module flow**, from the roadmap:
   - `-tests` combined with `-recurse` over one module's packages;
   - a layout for dependencies taken from the module cache;
   - a packaging decision for a module with several packages.
   At 96 packages plus ~16 dependency modules, this is the scale test of all three. `-recurse=module` exists today;
   the test flow across packages does not.
3. **Fixture staging at scale:** tens of thousands of case and baseline files under `testdata/`. The junction staging
   exists; `baselines/local` WRITES must land somewhere the host allows.
4. **`DefaultGODEBUG`:** the module's `go 1.26` line must select Go 1.26's GODEBUG defaults. It carries no `godebug`
   overrides of its own.
5. **`//go:embed` at size:** ~3.8 MB of lib `.d.ts` (108 files; `lib.dom.d.ts` alone is 2.3 MB), plus possibly ~1.1 MB of
   gzipped diagnostic locales. The mechanism exists (`EmbeddedResource`, ruled 2026-09-22); the size is new.
6. **Build size and time:** ~296K Go lines across ~96 projects plus dependencies is several times any stdlib package.
   Assembly count, compile time and a single-file publish are unmeasured.
7. **`testing/synctest`** (2 test files) and **Node.js-gated tests** need disclosures or bodies.

## 6. Seat ladder (rough sizes; everything converter-side can start before hop D, the target itself cannot)

| Step | Scope | Rough size | Gate |
|:--|:--|:--|:--|
| 0 | Stepping stones on the roadmap: uuid, then gojq (module-cache dependency), then jwt (tests across packages, `DefaultGODEBUG`, fixtures outside `testdata`) | as roadmapped | now to Q4 |
| 1 | The dependency slice: `x/sync`, `x/text` (the subset used), `go-json-experiment/json`, `patience`, `xxh3` (pure fallback), `cpuid` (fallback). Windows-only: `x/sys/windows`, `go-winio` | ~6-8 modules; json is the large one | hop D, per dependency |
| 2 | **Smallest compiling slice:** the compiler core closure without LSP/API/fourslash. `core`, `collections`, `tspath`, `scanner`, `parser`, `ast` (incl. generated), `diagnostics`, `binder`, `checker`, `compiler`, `printer`/emitter, `bundled` (embed), `vfs`, `cmd/tsc`. Proof: `tsc --noEmit` on a trivial program. | a majority of the 296K lines; the checker alone is the largest single package | hop D; steps 1 and 0's flow items |
| 3 | **First conformance subset:** `TestLocal` filtered to a few hundred `conformance/` cases, with the reference baselines staged and compared | fixture staging of a subset; parallel-test thread budget measured | step 2 |
| 4 | The full compiler + conformance runner (~12.8K cases) | host time and memory unmeasured | step 3 |
| 5 | LSP and fourslash (4,290 tests; `go-winio` pipes on windows) | the heaviest single package | step 4 |

**Hop dependency, stated plainly:** steps 2-5 need **Go 1.26 = after hop D**. Before that, the useful work is step 0 and
the converter capabilities in section 5, all shared with the roadmap's modules.

## Unconfirmed (carried from the read)

- Whether the gzipped diagnostic locales are embedded (inferred from `loc_generated.go`, not read).
- How the 4,290 fourslash test files are produced (no `Code generated` header on them).
- Whether tag `v7.0.2` points at a commit on `release-7.0`.
- The full `require` list comes from one summarising read; the `go`/`toolchain`/`godebug` lines were checked exactly.
- Every size in section 6 is a rough, unmeasured estimate. None is a prediction until a slice is actually converted.
