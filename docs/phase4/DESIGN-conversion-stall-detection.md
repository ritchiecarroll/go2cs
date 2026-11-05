# DESIGN — conversion stall detection

> **Status: PROPOSED, awaiting review.** Nothing here is implemented. Written 2026-08-07 out of the
> issue-#33 arc; the open questions at the end are the ones that need a ruling before it is built.
> Companion work already landed: the `GO2CS_PPROF` endpoint and the per-run `Slowest N of M packages`
> summary (see the *UP NEXT* entry in
> [`BOARD-next-validation-candidates.md`](BOARD-next-validation-candidates.md)).
>
> **STATUS UPDATE, 2026-11-05 (P2): BUILT** on `claude/p2-stall-warning`, after COORD ruled the five open questions
> below (all five of P2's recommendations accepted; none was a reads-like or runs-like-Go point, so none went to
> the owner). The rulings: (1) warn at 2m, repeat at 4m, 8m and 16m and then every 16m; dump once at 5m;
> `GO2CS_STALL_WARN` / `GO2CS_STALL_DUMP` take Go durations, `0` turns each off, no new flag. (2) The dump is on by
> default and goes to a temp-dir file (`go2cs-stall-*.txt`) that one stderr line names. (3) Watched: `-recurse`'s
> `convertAll`, `-stdlib`'s `convertAllPackages`, and the conversion step of `-tests -recurse` phase B (not its test
> action); a single-package conversion is not. (4) Warning only, permanently; `-stdlib`'s `-convert-timeout` cap
> stays as it is. (5) `-stdlib` is included, through the one helper.
>
> What had moved since this was written, read at master `1e99f5bb44`: `-stdlib` gained a per-package kill cap on
> 2026-09-02 (`-convert-timeout`, default 10m), so "not a timeout" holds for the watch, not for that driver; phase B
> of `-tests -recurse` is a third per-package loop; and `-stdlib` measured 358 packages in 9m29s (linux, 4 cores,
> host target, seeded root): mean 1.59s, median 0.91s, slowest `runtime` 24.2s, so the defaults table below is
> superseded by the 2m warning. The helper is `withStallWatch` in `src/go2cs/diagnosticProfiling.go`; its tests are
> `src/go2cs/stallWatch_test.go`. The warning text differs from the sample below in one place: it names
> `GO2CS_PPROF=:6060` and the endpoint that run prints, rather than a fixed URL.
>
> A finding from the re-check is its own seat (COORD, 2026-11-05): when `-convert-timeout` fires, the timed-out
> conversion's goroutine is abandoned, not stopped, and keeps running on package-global state while the driver
> converts the next package.

## The problem, in the reporter's own words

Issue #33 arrived twice, and both times the converter told the user nothing useful about a failure that
was entirely inside the converter.

> "It's definitely a lot better now, but it appears to be **hanging indefinitely** on
> `[1440/1726] Converting go.mongodb.org/mongo-driver/bson/bsoncodec`. **Either that or there's
> something very odd about this package that's making it legitimately take more than half an hour to
> convert?**"

That sentence is the whole case. A user watching a `[1440/1726]` line sit still **cannot distinguish a
bug from slow work**, because the converter never states what normal looks like. They were right to
report it and right to hedge — and the hedge cost a full round trip, because the report could not carry
the one thing that would have closed it immediately: a stack.

This is not a one-off. Both defects in issue #33 reached us stack-free:

| what the user saw | what we needed | round trips |
|---|---|---|
| `panic: … nil pointer dereference` mid-run, ~1,000 packages discarded | which package, and its stack | 1 (they pasted a trace — this one worked) |
| `[1440/1726]` not advancing | which package, and its stack | 2 — reproduce locally, then profile |

The second row is what this design removes. The converter knows it has been inside one package for
half an hour; it simply never says so.

## Goals

1. **Tell the user their run is abnormal, while it is still running.** Not after; the run may never end.
2. **Make the first bug report actionable** — name the package, the elapsed time, and how to capture
   more.
3. **Never change the outcome of a conversion.** A slow package must still be allowed to finish.

## Non-goals

- **Not a timeout.** Nothing is aborted, skipped, or failed on the strength of a clock. A legitimately
  slow package on a slow machine must complete normally. (See *Open question 4* — this is the one I
  would most like ruled explicitly, because "warn only" is a deliberate limitation, not an oversight.)
- **Not a profiler.** `GO2CS_PPROF` already covers deliberate investigation. This is for the user who
  was not expecting to investigate anything.
- **Not per-file or per-phase granularity.** Per package is the unit the progress output already uses
  and the unit a bug report can name.

## Proposed behavior

Two thresholds, both per package, both warning-only.

**At `warnAfter` (proposed default 60s)** — one line to stderr:

```
WARNING: go.mongodb.org/mongo-driver/bson/bsoncodec has been converting for 60s.
         Typical is well under a second per package (a few seconds under -recurse), so this is
         unusual and worth reporting: https://github.com/ritchiecarroll/go2cs/issues
         The conversion has NOT been interrupted and will continue.
         For a diagnosis, re-run with GO2CS_PPROF=:6060 and capture:
             go tool pprof -top http://<loopback>:6060/debug/pprof/profile?seconds=20
```
<!-- identifier scrubbed 2026-09-26 by security order: `:6060` is the loopback default (a bare port reads as localhost); the URL host is a placeholder -->

Repeat at a widening interval (60s, 2m, 4m, 8m…, capped) so a genuinely long conversion does not
produce a wall of text, but a truly stuck one keeps saying so.

**At `dumpAfter` (proposed default 5m)** — the above, plus every goroutine's stack via
`runtime.Stack(buf, true)`, once. That is the artifact that would have made issue #33's follow-up a
zero-round-trip fix: the ~40-deep `convCallExpr → convExpr → convSelectorExpr → convExpr` cycle is
legible on sight, and its *stability across two dumps* is what identified re-walking rather than
runaway recursion.

### Why this would have worked on the actual bug

The reporter would have seen the warning at 60s instead of wondering at 30 minutes, and their first
message would have carried the stack. The diagnosis took me ~20 minutes *once I had that stack*; the
expensive part was getting to it.

## Where it hooks in

A single helper, used by both batch drivers, so the behavior cannot drift apart:

```go
// in diagnosticProfiling.go, beside the pprof endpoint
func withStallWatch(label string, convert func() error) error
```

- `ModuleConverter.convertAll` — wraps the existing per-package closure (`moduleConverter.go:401`).
- `StdLibConverter.convertPackage` — the same, for `-stdlib`.
- **Not** bare single-package conversion: there is only one package, so "which one" is not a question,
  and the user is watching a command they just typed.

Implementation is a `time.AfterFunc` armed before the call and `Stop()`ed after — no polling goroutine,
no channel, and it costs one timer per package.

### Things the implementation must get right

- **`Stop()` in a `defer`**, so a package that panics still disarms its timer. The per-package
  `recover()` in both drivers is *inside* the closure; the watch must wrap *outside* it.
- **Serialize the output.** The timer fires on its own goroutine while the main one is printing
  progress lines. Take a mutex around the warning, or the two interleave mid-line.
- **`runtime.Stack(buf, true)` stops the world** for its duration. At a 5-minute threshold on a run
  that is already pathological this is free, but it is a real reason not to lower `dumpAfter` much.
- **Size the stack buffer generously and honestly** — the escape analysis runs a goroutine per file, so
  a large package has hundreds of stacks. Grow until it fits rather than silently truncating.
- **Say "not interrupted" explicitly.** A user who sees a warning about a hang will otherwise assume
  the tool gave up.

## Risks

| risk | mitigation |
|---|---|
| False alarms on slow machines annoy users into ignoring warnings | Threshold set from measured data (below), tunable, and worded as "unusual, worth reporting" rather than "error" |
| A `-stdlib` run over 304 packages emits noise if the threshold is too low | Measured average is ~0.6s/package; 60s is ~100× that |
| Stack dump leaks paths/source into a pasted report | Already true of any panic trace; no new class of disclosure |
| Timer goroutine outlives the run | `defer Stop()`, and the process exits anyway |

## Choosing the defaults — the data we have

| workload | measured | source |
|---|---|---|
| `-stdlib` full | ~195s / 304 packages ≈ **0.6s** per package | CLAUDE.md timing table |
| `-recurse`, warm | 12.6s / 7 packages ≈ **1.8s** per package | mongo repro, 2026-08-07 |
| `-recurse`, cold module cache | 36.7s / 7 packages ≈ **5.2s** per package | mongo repro, first run |
| the pathological case | unbounded (3⁴² walks) | issue #33 |

A 60s warning threshold is ~12× the slowest legitimate per-package average measured, on a laptop. It is
comfortably clear of normal and still catches a stall long before a user gives up. **These numbers are
laptop and desktop mixed** — worth one more measurement on the desktop before the default is fixed.

## Test plan

- A unit test on `withStallWatch` with a millisecond threshold and a deliberately slow `convert`,
  asserting the warning fired, that the return value is passed through untouched, and that a fast call
  fires nothing.
- A test that a panicking `convert` still disarms the timer and still propagates the panic to the
  caller's `recover` (this is the containment issue-#33 already had to fix once, in
  `performEscapeAnalysis` — the watch must not reintroduce it).
- No behavioral-corpus impact expected: the corpus converts single packages, and nothing here touches
  emission. CNR should be byte-identical.

## Open questions — these need your ruling

1. **Thresholds.** 60s warn / 5m dump, or different? Should they be tunable by env
   (`GO2CS_STALL_WARN=2m`), by flag, or fixed?
2. **Is the goroutine dump on by default,** or opt-in? On by default is what makes a first report
   actionable; against that, it dumps a wall of text into a user's terminal unannounced.
3. **Scope.** Both batch drivers as proposed, or `-recurse` only (which is where third-party code — the
   unbounded input — actually enters)?
4. **Warning-only, permanently?** I recommend never aborting. But if you would ever want
   `-package-timeout` to *skip* a package and continue, the design changes: the skip has to be recorded
   in the failed list and reported at the end, and that is a real behavior change worth deciding now
   rather than bolting on.
5. **Does this belong in `-stdlib` at all?** Its input is fixed and known-good; the argument for
   including it is uniformity, and the argument against is that it can only ever fire on our own
   regression — which CNR and the suite already catch.
