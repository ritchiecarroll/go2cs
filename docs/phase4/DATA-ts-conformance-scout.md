# DATA — TypeScript's compiler/conformance suite on the converted TypeScript Go port (scout)

> **State: MEASURED (2026-11-20 to 2026-11-21), linux/amd64, one 4-core cloud box with 15 GB RAM.** Measurement
> only: no converter, golib or pipeline change rides with this note. Every number below is a **scout instrument
> reading (by-hand publish + P5 diff), not the `-tests` compare**, and every converted number carries the
> labelled `TSC_REPO_ROOT` harness step of section 3.3. Sections are **appended and superseded, never overwritten.**
> The official run is a later `-tests` run at train 1's landed tip. Brief: COORD 2026-11-20 210156Z, 230710Z,
> 231237Z; 2026-11-21 003739Z, 005347Z. No TypeScript source and no converted C# is reproduced here.

## 1. Why this was measured

The TypeScript Go port carries the TypeScript compiler's own baseline suite: every case under
`testdata/tests/cases/{compiler,conformance}` is compiled and its error, type, symbol, output and sourcemap
baselines are compared byte for byte with the committed references. It is a large, real behavioural test of a
converted Go program. The scout asked three things: does the converted runner start and compare honestly, how
many verdicts match Go's, and what a full run costs on this box.

## 2. Host, toolchain, method

| | |
|:--|:--|
| box | 4 logical processors, 15 GB RAM, no swap, linux/amd64 |
| toolchains | go1.26.11, .NET SDK 10.0.x (runtime 10.0.11) |
| source | `microsoft/TypeScript` at `ba29a56299`, the `tsc/` tree (module `github.com/microsoft/TypeScript/tsc`). The separate `typescript-go` repository is closed (2026-08-20); the port lives in `tsc/`. |
| runner | `tsc/internal/testrunner`, `TestLocal` (two baseline runners, compiler and conformance) |
| converter | P5's local tree `de3a62fad8` (C1 union + wall C on a master-era base), not current master |
| converted host GC | the published test host's runtimeconfig has no GC keys: workstation GC, concurrent on |
| native GC | Go defaults |

- **Native**: `go test` (and, for sampled runs, the built test binary run directly with `-test.v=test2json`,
  decoded with `go tool test2json -t`).
- **Converted**: `go2cs -tests -recurse -test-action convert`, then the harness's own publish command run by
  hand (section 4.2 says why), then the published host run directly with `-test.v=test2json` and the same
  `-test.run` filter as the native side. Verdicts compared by a per-test JSON diff at two levels:
  configuration (`TestLocal/<file>[_<option=value>]`) and baseline subtest (`.../error`, `.../type`, ...).
- **RSS**: `VmHWM`/`VmRSS` from `/proc/<pid>/status` of the host process itself, every 5 s.
- **Heap**: `dotnet-gcdump collect` (non-elevated) and `dotnet-gcdump report`; retention paths by a small reader
  over the same tool's graph assemblies (breadth-first from the roots, so a path is the first one found, not
  the owner).

## 3. Recipe

### 3.1 Native needs `GOWORK=off`
`tsc/go.work` references `../tools`, which a sparse `tsc/` checkout does not carry. With `GOWORK=off` the module
builds and tests on its own.

### 3.2 The module copy the converter sees
`go list -deps -test ./internal/testrunner` gives 89 non-std packages: 63 TypeScript packages and 26 third-party
(go-json-experiment/json, go-cmp, xxh3, cpuid, patience, x/sync, x/sys/unix, x/text, gotest.tools). All 89 are
inside the 95 of P1's four TypeScript test packages; the runner adds none. The copy:
1. Keep the 63 TypeScript packages of the closure; drop the `.go` files of every other package directory (41).
2. Remove every `_test.go` except in `internal/testrunner`.
3. `replace github.com/zeebo/xxh3 => <go2cs tree>/src/forks/github.com/zeebo/xxh3@v1.1.0` (this drops cpuid,
   so the converted closure is 88).
4. Control: the native suite on the copy is identical to the native suite on the sparse tree (85 s, rc 0).

### 3.3 Harness step for W1 (labelled in every converted number)
`internal/repo.RootPath` finds `testdata` by walking up from `runtime.Caller(0)`'s file name and panics
("repo root cannot be found when built with -trimpath") on a module-path-form name. The converted
`runtime.Caller` answers the position map's name, `github.com/microsoft/TypeScript/tsc@/internal/repo/paths.go`,
which is the shape a Go `-trimpath` build reports. The step: in the module copy's `internal/repo/paths.go`, inside
the `github.com/` prefix branch and before the panic, return `os.Getenv("TSC_REPO_ROOT")` when it is set; convert
that one package again in single-package mode; run the converted host with `TSC_REPO_ROOT` pointing at the copy.
The native side runs on the same patched copy with the variable unset, so it takes its usual branch.

## 4. Readings

### 4.1 Native, full suite (GOMAXPROCS=4)
12,728 case files enumerated, 42 skipped by name, 12,686 run. They expand to **15,164 configurations (13,444 pass +
1,720 skip)** and **119,720 baseline subtests (119,712 pass + 8 skip)**, **0 fail**. **85 s** of test time (208 s
with the first compile). The suite runs its configurations with `t.Parallel`.

### 4.2 Conversion and build
- Convert: **88/88** packages in 22 min 13 s, 0 GO2CS2005, no refusal. The Go build cache grew to 12 GB (F3).
- Build: under `-recurse`, `-test-action build` runs phase-A conversion again before it builds (the log repeats
  the 88-package conversion), so the Go cache grew again and the 4 GB disk floor stopped the run. With
  `-recurse`, "build" is not build-only (COORD's BOARD row (zz)).
- The harness's publish command run by hand on the converted test host
  (`dotnet publish <testrunner>.tests.csproj -c Release -p:go2csPath=<tree>/src/ -p:GoTargetOS=linux -o <pkg>/bin/tests/publish`):
  **0 CS/MSB/NETSDK errors**, 48 min on 4 cores, out root 6.9 GB, publish directory 251 MB.

### 4.3 Instrument checks
- **Bundled-libs falsifier cleared.** `TestLocal` skips when the bundled libs are not embedded; on the converted
  host it runs (one-case probe `2dArrays.ts`: 9 baseline subtests pass, 13.2 s, 14.5 s process).
- **Red control.** One character changed in the committed `2dArrays.types` reference: the converted run fails
  exactly `TestLocal/2dArrays.ts/type` ("the baseline file ... does not match"); restored sha-identical. A
  converted pass is a real baseline comparison.

### 4.4 Slice, parallel (default workers)
Every 32nd file of the sorted compiler+conformance list minus the 42 named skips: **397 files**.

| | configurations | baseline subtests | fail | TestLocal | process |
|:--|:--|:--|:--|:--|:--|
| native | 454 (409 pass + 45 skip) | 3,644 pass | 0 | 45.1 s | 55 s |
| converted | 454 (409 pass + 45 skip) | 3,644 pass | 0 | 63.7 s | 65 s |

**0 differences** at both levels; the skip set is identical. Ratio 1.41x on TestLocal, 1.18x on process wall.

### 4.5 Full suite, converted, by hand (default workers, `-test.timeout 85m`)
- **Did not finish.** The timeout fired at 5,156 s; the host's timeout reporter then died of
  `OutOfMemoryException` (exit 2).
- Finished: **11,739 of 15,164 configurations = 10,361 pass + 1,370 skip + 8 fail**; 92,498 baseline subtests
  (92,492 pass + 6 skip). 3,425 configurations never finished.
- **0 wrong outputs**: every finished configuration that did not fail has the native verdict.
- The 8 fails are all `OutOfMemoryException` (20 infrastructure-error events), not wrong output, in 8 files:
  `unionSubtypeReductionErrors.ts`, `largeControlFlowGraph.ts`, `varianceProblingAndZeroOrderIndexSignatureRelationsAlign2.ts`,
  `asyncArrowFunction4_es6.ts`, `crossProductUnionIntersectionRepeatedPropDeclarationsNoOOM1.ts`,
  `jsxJsxsCjsTransformChildren.tsx`, `tsxReactEmitSpreadAttribute.ts`, `typeGuardsInClassAccessors.ts`. Native
  passes all 8.
- Throughput fell with run length: about 2.3 configurations/s averaged over 85 min, against 454 in 64 s for the
  slice. RSS 2.9 GB at minute 4.

### 4.6 Repetition, converted slice, `-test.count=4` in one process
TestLocal per repetition 69.5 / 42.9 / 39.7 / 39.0 s (flat after JIT warm-up), same verdicts each time. RSS 1.65 →
1.83 → 1.96 → 2.08 GB over repetitions 2-4 (about +0.15 GB per repetition).

### 4.7 Serial slice (arm 2): `-test.parallel 1`, `GOMAXPROCS=1`, both sides

| | TestLocal | process | peak RSS |
|:--|:--|:--|:--|
| native | 41.15 s | 65 s | 239 MB |
| converted | 98.5 s | 105 s | 1.15 GB |

0 differences. **2.39x** on TestLocal, 1.62x on process wall, 4.8x on peak RSS.

### 4.8 The 8 out-of-memory files alone, serial (12 configurations)

| | rc | wall | verdicts | peak RSS |
|:--|:--|:--|:--|:--|
| native | 0 | 40 s | 11 pass + 1 skip | 1.52 GB |
| converted | 0 | 640 s | 11 pass + 1 skip (same skip), 0 subtest fails | 4.95 GB |

Verdicts identical; none runs out of memory alone. `crossProductUnionIntersectionRepeatedPropDeclarationsNoOOM1.ts`
takes 31.6 s native and 557 s converted (**17.6x**, including a 12 s snapshot pause). A snapshot inside that
case's type baseline (RSS 3.9 GB) read 965 MB of live heap after the dump's collection.

### 4.9 Heap snapshots, converted slice, `-test.count=4`, after repetitions 1 and 3
TestLocal 63.7 / 44.5 / 38.4 / 40.7 s, 0 fails. Dumps fired within 2 s of the repetition's end. The repetition-3
dump (and the 4.8 dump) reached the tool's 10,000,000-node cap under lossy buffering on .NET 10, so their counts
are lower bounds; repetition 1 is complete (6.75 M nodes).

| | rep 1 | rep 3 |
|:--|:--|:--|
| GC heap | 474 MB | ≥ 672 MB |
| RSS at dump | 1.30 GB | 1.49 GB |
| `go.ViewEntry` | 95.5 MB / 2,387,689 | 147.3 MB / 3,681,704 |
| `FieldRefBox<ast.Node>` (and `NodeBase`, `NodeDefault`, the same count) | 28.7 MB / 359,304 each | 44.8 MB / ~559,380 each |
| `FieldRefBox` of the expression-base chain (6 types) | ~10.5 MB / ~130k each | ~16 MB / ~200k each |
| reflection (`RuntimeMethodInfo`, `RuntimeParameterInfo`, `Signature`) | ~34 MB, ~125k each | same, flat |
| `SemaBucket` | 16,253 | ≥ 31,658 |

- **`SemaBucket`** (P3's prediction for i9's seat): each bucket comes with exactly one `os.file` box,
  `FieldRefBox<poll.FD>`, `FieldRefBox<fdMutex>`, `Queue<SemaWaiter>` and dictionary node. Retention: the static
  `semaTable` (`ConcurrentDictionary<ж<uint32>, SemaBucket>`) keyed by the fdMutex semaphore's field box, which
  holds fdMutex → FD → `os.file`, so every file ever opened stays. About 704 B per file, about 7.7k files per
  repetition (17 per configuration): ~5.4 MB per repetition, ~5% of the growth. Real, but not the long-run cause.
- **The largest growing family** is AST-shaped: `go.ViewEntry` plus the node field boxes. Root-side first paths
  for `ViewEntry`, rep 1 → rep 3: the static `sync.Pool` `emitContextPool` (8 pooled `printer.EmitContext`)
  659,661 → 1,498,112; `ConditionalWeakTable` view-table values (dependent handles) 922,811 → 1,468,494; the
  harness's static `sourceFileCache` 713,917 → 460,567. In the 4.8 dump all field/element ref boxes and view
  entries were 459 MB of the 1.01 GB live heap.
- First reading, refuted in 4.10: the converted `sync.Pool` ages its caches only in the converted runtime's GC
  cycle (`runtime.GC()` or the 2-minute periodic tick), where Go clears pools at the start of every GC cycle, so
  the pooled contexts would pin recent ASTs.

### 4.10 Discriminator: pool cleanup at every gen-2 collection (scratch startup hook)
A .NET startup hook (no tree touched, host not rebuilt) with a self-re-registering finalizer sentinel runs the
converted runtime's registered pool cleanup once after every gen-2 collection, under the runtime's own GC-cycle
lock. It fired 14 times, once per gen-2 collection of the run. Same `-test.count=4` slice and dumps.

| | unhooked | hooked |
|:--|:--|:--|
| GC heap rep 1 → rep 3 | 474 → 672 MB (+198) | 458 → 682 MB (+225) |
| `go.ViewEntry` rep 1 → rep 3 | 2,387,689 → 3,681,704 | 2,273,024 → 3,629,727 |
| top root path in rep 3 | `emitContextPool` | `emitContextPool` |
| TestLocal per rep | 63.7 / 44.5 / 38.4 / 40.7 s | 65.8 / 42.9 / 38.1 / 40.5 s |

The growth is unchanged (COORD's falsifier, "growth within 20% of today's", fired), so pool aging is not the
cause. The pooled contexts are always recent (Get/Put churn refills the pool after every cleanup), so that root
path means "reaches the most recent ASTs" under first-path attribution, not ownership.

### 4.11 What does and does not grow
- **Native does not grow**: the native `-test.count=4` slice under `GODEBUG=gctrace=1` holds a live heap after GC
  of median 45 MB in each of repetitions 2-4 (minimum 35-36 MB); TestLocal 42.1 / 37.7 / 37.6 / 34.7 s. The
  converted live heap is about 10x native at repetition 1 and grows about 100 MB per repetition.
- `ast.SourceFile` boxes: exactly 675 in all four converted snapshots. The number of ASTs held does not grow.
- Field accessor tokens (`FieldRefFunc`/`FieldPtrFunc`): 4,228 in repetitions 1 and 3, so view-cache keys are
  stable. `FieldRefBox` count = `ViewEntry` count + 190 in every snapshot: each field box is one cached view.
- `ElemRefBox` 466 k → 692 k (+48%). No slab array has more element boxes than its length (at most 264-295 per
  slab), so there is no duplicate per-access element box. Slab arrays held grow 5-9% (`Identifier[]` 2,377 →
  2,582) while the boxed elements in them grow about 45%.
- Golib's own retention rule (`ж.Views.cs` header): a parent keeps one view per distinct field asked of it, for
  its lifetime.

### 4.12 The curve: `-test.count=8`, live heap after each gen-2 collection
The scratch hook in log-only mode (no pool cleanup) writes `GC.GetGCMemoryInfo` after every gen-2 collection; no
dumps. Same slice, one process: rc 0, 308 s, 0 fails, same verdicts; TestLocal 58.5 / 38.8 / 34.4 / 33.2 / 32.8 /
35.2 / 36.1 / 35.6 s.

| repetitions finished | 1 | 2 | 3 | 4 | 5 | 6 | 7 |
|:--|:--|:--|:--|:--|:--|:--|:--|
| survivors of gen 2 (`PromotedBytes`) | 455 MB | 637 MB | 785 MB | 927 MB | 1,103 MB | 1,267-1,296 MB | 1,526-1,568 MB |

**Linear, about 140-160 MB per repetition of 454 configurations (about 0.33 MB per configuration), with no sign
of levelling off.** Over 15,164 configurations that is about 5 GB retained, which fits the full run's collapse
and its out-of-memory failures with four configurations in flight on 15 GB (4.5). This growth is the long-run
cause; the semaphore table (4.9) and the long-tail case (4.8) are not. Which code path keeps adding new
(node, field) views on the constant set of ASTs is not found here.

## 5. Findings for routing

1. **W1** (COORD's BOARD row (aaa)): converted `runtime.Caller` reports the module-path (trimpath-form) file
   name, so any Go program that locates files from its own source path cannot run converted without a remedy
   (a run-time source-root mapping in golib, or a documented limitation).
2. **(zz)**: under `-recurse`, `-test-action build` re-runs phase-A conversion.
3. **Instrument note**: the converted testing host's elapsed time for a `t.Parallel` subtest includes its paused
   time (89-93 s reported for 0.02-0.07 s cases), where Go restarts the clock when the subtest resumes.
   Per-case ratios from converted test2json are not usable; totals are.
4. **semaTable** keeps every opened file (4.9).
5. **Converted memory grows linearly across repetitions of identical work; native does not** (4.11, 4.12).
   Pool aging is ruled out (4.10); the growth is element boxes and field views accumulating on a constant set
   of ASTs.
6. **Memory**: converted peak RSS is 3-5x native on these cases; four concurrent large cases exceed 15 GB.
   The serial OOM-8 run passes in a fresh process, so the full-run OOMs read as the per-run growth of 4.12 plus
   four configurations in flight, not a per-case defect (not measured: the 8 files at 4 workers alone).
7. **Long tail**: one case is 17.6x native (4.8), against 1.4-2.4x for the slice.

## 6. Predictions, scored as worded

| prediction (post) | result |
|:--|:--|
| native: 12,686 cases run, 0 fail (210655Z) | MET |
| native wall 15-40 min | MISSED (85 s; the suite is parallel) |
| convert 89/89 (211649Z) | 88/88; the xxh3 fork drops cpuid |
| converted host 0 CS errors | MET |
| bundled libs embed, TestLocal does not skip | MET (falsifier cleared) |
| slice native ~475 configurations, 0 fail, under 10 s (230039Z) | 454 MET as "about"; 0 fail MET; 10 s MISSED (45 s) |
| slice converted: no hang, ≥95% match, 2-10 min | no hang MET; 100% match MET; wall MISSED (65 s) |
| full converted: all 15,164 configurations, 0 differences, 1.2-1.6x, no OOM (230828Z) | MISSED; falsifier fired (timeout, 8 OOM) |
| count=4: time and RSS both grow, or both flat (003623Z) | neither: time flat, RSS grows slowly |
| arm 2 ratio under 2x (P3, and P5) | MISSED on TestLocal (2.39x); held on process wall (1.62x) |
| SemaBucket grows with cases run (P3) | MET in shape; ~5% of growth |
| pool cleanup at gen 2 cuts growth to <= 50 MB/rep, pool root leaves the top (010537Z) | MISSED; falsifier fired (4.10) |
| count=8: bounded warm-up, rep 5→7 < half of rep 1→3 (011507Z) | MISSED; falsifier fired (+423 vs +330 MB, linear) |

## 7. Not measured here
- Any run through the `-tests` pipeline, and any run at current master.
- The 8 OOM files at 4 workers alone.
- Windows and darwin.

## 8. Amendment, 2026-11-21 (after `a68adf08de`): the owner and the root cause

Same instruments and labels as above. COORD gave P5 the root-cause seat (2026-11-21 012155Z).

### 8.1 Fill and reachability (the existing count=4 dumps)
- Fill = element boxes / total length of the slab arrays they point into: 84.4% at repetition 1, 83.8% at
  repetition 3. Flat, so the growth is new slabs (capacity 552 k → 799 k), not more boxes on old ones. P5's
  prediction (55-75%, rising) MISSED.
- Split by reachability from the harness `sourceFileCache`, the `emitContextPool` static and the
  dependent-handle roots: about all of the repetition 1 → 3 growth sits in slabs not reachable from
  `sourceFileCache` (prediction ≥ 70%: MET). Symbols do not grow; syntax nodes do, type nodes most
  (`KeywordTypeNode` +64%, `TypeReferenceNode` +53%).

### 8.2 Owner by dominator tree (clean dumps after repetitions 1 and 2, both under the node cap)
`-test.count=3`, 6.76 M and 9.28 M nodes, heap 476 → 641 MB. Exclusive retention (dominator subtree) of the items
that grow:

| owner | rep 1 | rep 2 |
|:--|:--|:--|
| static `sync.Pool` `emitContextPool` | 80.9 MB | 199.1 MB |
| an `EmitContext` box held from a running goroutine | 45.9 MB | 90.3 MB |
| `semaTable` (4.9) | 17.3 MB | 33.6 MB |
| harness `sourceFileCache`, extension-method reflection scan, others | flat | flat |

Each pooled `printer.EmitContext` dominates 24-59 MB, all through its `printer.NodeFactory`. The factory
references only its arenas' current slabs (49 references), yet dominates, for example, 70 `Identifier[]`, 44
`NodeList[]` and 27 `TypeReferenceNode[]` slabs with their element boxes and field views. TS's `core.Arena` keeps
filling a partly used slab across uses, and `EmitContext.Reset` keeps the factory, so the current slab holds the
latest emit's nodes and the tail of the one before; those nodes point into older slabs that hold an earlier
emit's tail, and so on through every emit the pooled factory did.

### 8.3 Pool cleanup at every .NET GC (scratch startup hook)
The same hook as 4.10, firing after every GC of any generation: 720 cleanups in 752 collections (33 gen 2).
`-test.count=6`: rc 0, 244 s, 0 fails, same verdicts; TestLocal 61.2 / 39.6 / 34.9 / 34.3 / 34.8 / 36.3 s.

| repetitions finished | 1 | 2 | 3 | 4 | 5 |
|:--|:--|:--|:--|:--|:--|
| gen-2 survivors, per-GC cleanup | 382-436 MB | 371-504 MB | 409-531 MB | 481-534 MB | 442-594 MB |
| gen-2 survivors, no cleanup (4.12) | 455 MB | 637 MB | 785 MB | 927 MB | 1,103 MB |

About +20 MB per repetition on the gen-2 medians, of which the semaphore table is about 16 MB. Peak RSS 1.62 GB at
the end of repetition 6. Prediction (≤ 30 MB per repetition after repetition 2): MET; falsifier (≥ 75 MB) did not
fire.

### 8.4 Root cause
The converted `sync.Pool` ages its caches only in the converted runtime's GC cycle (`runtime.GC()` or the 2-minute
periodic tick), where Go runs `clearpools` at the start of every GC cycle (the native run did 12,782 collections
in 188 s). A pooled object that keeps references is therefore never dropped while it stays in use, and TS's pooled
emit contexts keep every node graph they built. Gen-2 cadence is not enough (4.10): a busy pooled object survives
each cleanup through the victim cache. Per-GC cadence is. The element boxes and field views are retained only
because their slabs are; this is not a `ж.Views` or `ElemRefBox` policy defect. The fix is routed by COORD.

## 9. Amendment 2026-11-21: the fix arm (claude/p5-pool-cleanup-per-gc)

The seat's runtime change (sync.Pool aged at every CLR collection from a fresh gen-0 finalizer sentinel; commit
efa75d1c8c, local at the time of these runs) applied as a scratch change to the TS tree's runtime, the test host
republished by hand. Same instrument as above: by-hand publish + P5 diff, not the `-tests` compare, TSC_REPO_ROOT
harness step, linux/amd64 Release. The startup hook ran in log-only mode (no cleanup by the hook).

### 9.1 Slice, `-test.count=8`
rc 0, 338 s, 8 of 8 repetitions pass, 0 fails.

| arm | rep 2 | last rep | growth after rep 2 | peak RSS | s per rep (2 to last) |
|:--|:--|:--|:--|:--|:--|
| no cleanup (4.12) | 455-529 MB | 1,526-1,568 MB | ~150-175 MB/rep | 3.61 GB | 35.3 |
| per-GC hook, count=6 (8.3) | 405-513 MB | 442-594 MB | ~20 MB/rep | 1.62 GB | 35.8 |
| the fix | 391-432 MB | 479-593 MB | ~23 MB/rep (means 414 → 550) | 1.72 GB | 38.4 |

Gen-2 survivors are `PromotedBytes` at each gen-2 collection, grouped by the repetition in flight. One run per
arm, so the per-rep times are not a controlled time measurement. The residual slope is about the semaphore
table's share (4.9).

Predictions (021139Z), scored as worded: 0 fails MET; "350-650 MB at every rep" upper bound MET (max 593), lower
bound MISSED in rep 1 (279 and 303 MB at 20-26 s, before the first rep finished); "rep 8 within 120 MB of rep 2"
MISSED (+136 on means); falsifier (any rep over 800 MB, or over 30 MB/rep) did not fire; time within 10% of the
hook arm MET (+7%).

### 9.2 Full TestLocal, all 15,164 configurations
Same command as 4.5 (`-test.run '^TestLocal$'`, default parallelism, 85-minute timeout). rc 0, 2,644 s.

| | converted, with the fix | native |
|:--|:--|:--|
| configurations | 15,164: 13,444 pass, 1,720 skip | 15,164: 13,444 pass, 1,720 skip |
| baseline subtests | 119,720: 119,712 pass, 8 skip | 119,720: 119,712 pass, 8 skip |
| verdict differences | 0 at both levels | |
| OutOfMemoryException | 0 | |

Against 4.5: that run timed out at 5,156 s with 11,739 configurations finished, 8 of them failing on
OutOfMemoryException. Host RSS in the second half of this run was 6.5-8.6 GB with peaks to 10.9 GB (VmHWM); it
was not sampled for the first 1,465 s (the sampler read the timeout wrapper's PID), so the peak is the process
high-water mark, not a sampled curve.

Prediction (021139Z), scored as worded: finishes MET; 0 differences MET; no OOM MET; peak RSS under 7.5 GB
MISSED (10.9 GB).

Not measured here: the same runs through the `-tests` pipeline, and the converted run's wall time against native
(85 s, parallel) beyond the totals above.
