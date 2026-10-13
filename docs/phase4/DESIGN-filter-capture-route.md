# The filter-capture route — sizing a panic unwind that no frame catches and re-raises

> **STATUS: PROPOSED 2026-10-12 (G, ordered by COORD). Nothing is cut.** This is the design pass COORD ordered
> after parking Fix B as specified. Its four questions are answered with measurements. **The emitted shape
> CHANGES on every route that works**, so by COORD's rule the adoption decision goes to **the owner**. The
> recommendation (§6) is to **park the route**. The motivating case, go-cmp's cyclic subtests, does not move.
> Deep panic recursion does move, about 5x, but no validated row needs that. The price is a corpus-wide emission
> change plus a re-derivation of `runtime.Callers`' panic splice.
>
> Every number below was read on windows, Release, tiered JIT off unless a row says otherwise, against one golib:
> master `226afb810a` plus F2 fix A (`claude/g-golib-creator-o1` `1a00003c9b`) plus the scratch prototype (§1),
> which was never pushed. Each prediction was stamped in its script before the run and is scored where it is
> used.

## 0. The answer in five lines

| COORD's question | Answer | Where |
|---|---|---|
| 1. What today's filter-then-catch order guarantees, and how the route keeps it | Chaining, recover-once, one panic per fault, defer order, named results and re-panic all read identical to Go and to today under the route. **`runtime.Callers` from a deferred call changes**: the frames the panic runs through are LIVE under a filter, so today's splice doubles them where it fires, and the filter funclet appears where Go shows `runtime.gopanic` | §3 |
| 2. The .NET semantics | Filters run on the first pass, before any inner `finally`. A filter runs again only for a NEW dispatch, never twice for one. A throwing filter counts as false and its exception is lost. A nested dispatch inside a filter completes. A stack walk inside a filter sees the throw path. **JIT and Native AOT read identical** | §2 |
| 3. Prototype numbers | deeppanic, 65,536 levels: **287–303 ms → 53–57 ms** wall. go-cmp's cyclic pair: **20 s → 17–19 s**, verdicts unchanged | §4 |
| 4. Footprint | **Not zero, and cannot be.** Measured at the tree: **3,182 emitted catch lines in 893 files** each change on their one line (the filter and the catch body). 339 open-coded-defer frames must keep today's shape | §5 |
| Ruling | Emitted code changes, so it goes to the owner. G recommends PARK | §6 |

## 1. The two routes, and why COORD's wording needs the second

**F-adopt (COORD's wording):** "the emitted filter captures the panic and returns false, so no frame catches and
rethrows at every level." The prototype is `GoFrame.CaptureInFilter(ex)`, which adopts the panic and answers
false. Timing it is easy, and it is the arm labelled *adopt-only* in §4. As a route it is incomplete, and it
cannot be completed:

- **Recovery needs a catch, and the catch is decided on the first pass.** A frame whose deferred call recovers
  must stop the unwind there, so its filter must answer TRUE. A `finally` cannot stop an exception: the second
  pass continues past it (filtersem S1).
- **The emitted filter cannot see its frame.** It is the static `GoFrame.IsPanic(ᒐex, out ᒐp)`. The frame's
  deferred calls live in the `ᒐ` local, a `ref struct`, which nothing static can reach. A thread-side registry
  cannot pair a filter with its frame either. A frame that has not registered a defer yet is absent from the
  registry, yet its filter still runs, so a registry would hand it an outer frame's defers.
- **Even with the frame, "will a deferred call recover?" cannot be answered before running the call.**
  - A marker on functions that call `recover()` directly would change behavior. Today a `recover()` in a HELPER
    the deferred function calls also recovers; that is the accepted divergence in `builtin.recover`'s remarks.
  - Fix B's census found 442 of std's 2,432 deferring frames defer an interface method or a func value. Those
    are unknowable statically. go-cmp's `compareAny` is one of them: `defer r.PopStep()` on an interface.

**F-run (the route sized here):** the filter RUNS the frame's deferred calls on the first pass. That is Go's own
model: Go runs defers on the panicking stack before anything unwinds. The filter answers TRUE only when one of
those calls recovered the panic or replaced it. Recovery is decided by running the defers, not by predicting
them, and no marker is needed. The prototype adds about 60 lines to golib:

- `GoFrame.RunInFilter(Exception)` adopts the panic, arms the claim, and runs `Run()` in a mode that reports an
  unrecovered tail instead of throwing it.
- `GoFrame.AfterFilter()` is the catch body. It raises a replacing panic.
- A thread-side (exception → panic) pair keeps ONE panic for a mapped fault (§3, row 5).

The emitted catch becomes:

```csharp
catch (Exception ᒐex) when (ᒐ.RunInFilter(ᒐex)) { GoFrame.AfterFilter(); }   // today: when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
finally { ᒐ.Run(); }                                                         // unchanged; on a panic path the defers already ran
```

A foreign .NET exception and a `runtime.Goexit` keep today's path. `RunInFilter` answers false without running
anything, so the `finally` runs the defers as it does now. Mixing shapes is safe in both directions:
- An old-shape frame inside an F-run frame catches on the first pass and re-raises, and the F-run frame's filter
  then runs on the new dispatch.
- An F-run frame inside an old-shape frame runs its defers in its filter, and the old frame then catches.

The go-cmp arm ran exactly this mix: its swapped frames ran against stdlib frames of today's shape (§4).

## 2. The .NET semantics, measured on the real CLR (item 2)

A standalone probe (`filtersem`) logs the ORDER of filter evaluations, `finally` blocks and catches, and which
exception each filter saw. It was published twice from one source, JIT and Native AOT. **All eight scenarios
print byte-identical lines on both.**

| # | Scenario | Observed order (both runtimes) | Property it proves |
|---|---|---|---|
| S1 | two filter-false frames over a thrower, the root catches | filter middle, filter outer, filter root, finally middle, finally outer, root caught | **P1:** every filter runs on the FIRST pass, before any inner `finally` |
| S2 | a dispatch thrown and caught inside a `finally` during the second pass | the outer dispatch's filters, then `finally middle` runs the inner dispatch to completion, then the outer second pass resumes | an inner dispatch is separate and does not disturb the outer one |
| S3 | a `finally` throws a NEW exception that escapes | filters of e1 (middle, outer, root), `finally middle` throws e3, filter outer(e3), filter root(e3), … root caught **e3** | **P2:** a filter runs again only for a NEW exception: the replacement re-runs the OUTER filters |
| S4 | a filter that throws | filter middle throws, filter outer(e1), … root caught **e1** | **P3:** a throwing filter counts as false, and its exception is swallowed |
| S5 | `throw;` from a catch | middle catches and rethrows, then filter outer(e1), filter root(e1) | P2: a rethrow is a new dispatch through the outer filters |
| S6 | `ExceptionDispatchInfo.Throw` after the frame's `finally` | the same as S5 | P2, by the re-raise golib uses for foreign exceptions |
| S7 | a stack walk taken INSIDE a filter | `Walk > S7Outer > Throw > S7Middle > S7Outer > Root > Main` | **P5:** a filter runs ON TOP of the throw path. The frames being unwound are live, and the filtering method appears again above them (its funclet) |
| S8 | a dispatch thrown and caught INSIDE a filter | filter middle begins, inner filter, caught e2, finally inside filter, filter middle ends, then filter outer(e1) and on | **P4:** a nested dispatch completes inside a filter, and the outer first pass continues unchanged |

**How F-run stays correct across the passes:**

- **P1 is the route itself.** All the frames' defers run innermost-first on the first pass, which is Go's order.
- **P2: "a filter may run more than once"** happens only for a NEW dispatch (S3, S5, S6). The route is idempotent
  by construction. `Run` consumes the frame's slots, so a re-run of one frame's filter finds nothing to run: it
  re-arms, claims, finds the panic unrecovered, and answers false. No deferred call can run twice.
- **P3 binds the design.** Nothing may escape `RunInFilter`. The prototype turns an unrecovered tail into a report,
  and a REPLACING panic (a deferred call panicked) answers true so the catch body can raise it. That costs one
  extra dispatch, and only when a deferred call panics.
  - **Open, designed but not prototyped:** a FOREIGN exception or a `GoexitException` raised by a deferred call
    inside the filter. The prototype would lose it to P3, along with the frame's remaining defers. The route
    must catch everything in `RunInFilter` and raise it from the catch body by `ExceptionDispatchInfo`.
- **P4** is what lets a deferred call panic and recover inside the filter, which is Run's own catch.
- **P5** is the one place the route changes what a deferred call can observe (§3, rows 1 and 2).

**Timing from the same probe:** N frames, each with a `finally`.
- Catch plus `ExceptionDispatchInfo` rethrow per frame grows QUADRATICALLY: JIT 10.4 / 208 / 2,882 µs per frame at
  1,024 / 8,192 / 32,768 frames; AOT 5.2 / 97 / 1,770 µs per frame.
- One filter-false dispatch costs 0.28–1.0 µs per frame, flat.
- golib's own re-raise is `throw owned`, not EDI, so its real cost is deeppanic's (§4), not this probe's.

## 3. What today's order guarantees, and what the route does to each (item 1)

A Go probe (`fcsem`, nine rows) prints what deferred calls observe while a panic runs. It was read three times:
under Go (the pin), converted by master's emitter and built against the golib above (*today*), and with every
one of its 15 emitted catches swapped to F-run (*F-run*).

| Row | What it reads | Go | today | F-run |
|---|---|---|---|---|
| 1 | `runtime.Callers` from a recovering deferred call, with the panic two deferring frames below | `a.func1 > runtime.gopanic > c > b > a > main` | `a.func1 > a > main` | `a.func1 > a > c > b > a > main` |
| 2 | `Callers` from a NON-recovering deferred call in the middle frame, mid-panic | `b3.func1 > runtime.gopanic > c3 > b3 > a3 > main` | **= Go** | `b3.func1 > runtime.gopanic > c3 > b3 > c3 > b3 > a3 > main` (**doubled**) |
| 3 | a panic during a panic: the second replaces the first | `recovered=second` | = Go | = Go |
| 4 | `recover()` answers exactly once | `first=v then=<nil>` | = Go | = Go |
| 5 | a nil dereference through two non-recovering deferring frames: their defers run, one panic | seen `[deref-defer mid-defer]` | = Go (seen) | = Go (seen) |
| 6 | `debug.Stack` from a recovering deferred call | shows c2, b2 and `panic(` | c2, b2, **no `panic(`** | the same as today |
| 7 | defer order across three frames, LIFO within each | `l3b,l3a,l2,l1` | = Go | = Go |
| 8 | a recovered frame returns its named result | `42` | = Go | = Go |
| 9 | a re-panic of `recover()`'s value, caught one frame out | `re:inner` | = Go | = Go |

Rows 5 and 6 also show two older divergences, identical in both arms, so they are not the route's:
- **Row 5:** `errors.As(err, &runtime.Error)` reads false. The recovered value of a nil dereference is a `string`,
  already recorded under `TestRuntimePanicWithRuntimeError` in the runtime divergence census.
- **Row 6:** `debug.Stack` has no `gopanic` frame, part of the frame-attribution class.

**Prediction, scored:**
- **Hit:** F-run "differs on 1 and 2". Row 2 doubled exactly as predicted.
- **Missed:** "row 6 differs" was wrong. Rows 5 and 6 do not differ between the arms, and today does not match
  Go on row 5's `runtime.Error` either.

**The behaviours, one by one:**

- **Two-exception chaining (rows 3, 9).** Today, Run's catch adopts a deferred call's panic, sets `Beneath`, and the
  sequence continues; the tail throws the replacement from the `finally`. Under F-run the same catch runs inside the
  filter (P4), and the replacement leaves through `AfterFilter`. Measured identical.
- **recover() exactly once (row 4).** Unchanged: `Recovered` is set once, and a recovered panic never re-enters a
  dispatch, because its frame's filter answers true and the frame returns.
- **One panic for a mapped fault (row 5).** This is NEW work the route needs.
  - `RuntimeErrorPanic.TryAsPanic` builds a NEW `PanicException` each time a `NullReferenceException` or
    `DivideByZeroException` is adopted.
  - Today only the first catch ever sees the raw fault, because every outer frame sees the re-raised panic.
  - Under any filter route the raw fault stays in flight, so every frame would adopt its own panic. The
    prototype keeps an (exception → panic) pair per thread.
- **Site ownership and the Callers sequences (rows 1, 2): the route's real cost.**
  - Today's model assumes the CLR has unwound the panicking frames before any deferred call runs, so
    `runtime.captureCallers` splices them back from the panic's `SiteTrace` when the sequence's activation
    owns the site (DESIGN-panic-stack-frames §3.B).
  - Under F-run those frames are LIVE (P5). Where today splices, the frames appear twice (row 2). Where today
    does not splice, the live frames show, but the funclet appears as the deferring method where Go shows
    `runtime.gopanic` (row 1).
  - So the route needs a captureCallers rule: a sequence run in a filter splices nothing, and the funclet frame
    renders as `runtime.gopanic`.
  - With that rule, row 1 would match Go exactly, which today does not. The rule is a re-derivation of the
    splice, and the runtime package's `TestCallers*` family is its gate.
- **An ordering hazard that does not exist today.**
  - F-run's deferred calls run BEFORE the `finally` blocks of the frames being unwound that are NOT Go
    frames: C# `lock` and `using` in golib and hand-owned code, and a range-over-func loop's `Dispose`.
  - Converted Go code has none, but golib does. `channel.DrainBuffer` raises its panic inside
    `lock (SyncRoot)`, while the hot channel sites release the lock before raising.
  - Under F-run, an outer deferred call that waits on another goroutine needing that lock would deadlock.
  - **A cut's first gate would be a census of every raise under a held lock** in golib and the hand-owns.
- **Open-coded defers.**
  - A deferred receiver-field call can be lowered into the frame's `finally`: 339 such `finally` lines at
    master (ConversionStrategies-Reference, *A deferred RECEIVER-FIELD call…*).
  - Under F-run they would run AFTER the frame's registered defers, which breaks LIFO.
  - The rule is that those frames keep today's shape, which mixing makes safe (§1).
- **Stack depth.**
  - Deferred calls now run on top of the full panicking stack, as in Go, but a .NET stack does not grow.
  - A recursion that panics near its thread's reserve could overflow where today it unwinds first, and a
    stack overflow is fatal in .NET.
  - deeppanic at 65,536 levels and go-cmp's cyclic pair both ran clean.
- **Goexit and foreign exceptions.** Unchanged: they keep today's `finally` path (§1).

## 4. Prototype numbers (item 3)

**deeppanic.** A Go program recurses N levels; each level defers a pop and the bottom panics. A top frame recovers
and reports. Three runs per arm. Under F-run the program's own timer runs inside the recovering deferred call, and
that call now runs BEFORE the second pass. So every arm also prints a WALL clock taken around the panicking call,
and that wall clock is the comparison.

| Arm | 1,024 levels | 8,192 levels | 65,536 levels | no-panic path, 65,536 |
|---|---|---|---|---|
| today (wall; 6 runs over two scripts) | 4.5–4.8 ms | 21.1–23.0 ms | **286.8–303.2 ms** | 79.3–83.4 ms |
| adopt-only, the hot frame only (its recovering frame keeps today's catch, so its timer is the wall) | 2.8–2.9 ms | 5.7–6.0 ms | **49.5–50.3 ms** | 60.5–62.8 ms |
| F-run, every frame (wall) | 3.1–3.2 ms | 6.0–6.5 ms | **53.3–56.7 ms** | 60.0–62.2 ms |

- **Prediction, scored: HIT** ("~300 ms falls to tens of ms; F-run ≈ adopt-only, since the defers' own work only
  moves").
- The no-panic path also reads about 20 ms faster in both prototype arms at 65,536 levels. That is not explained
  here; the plausible cause, the frame's `out` local and catch body, was not isolated.
- **Native AOT.** The shape is measured on AOT by `filtersem` (§2): catch-and-rethrow per frame is quadratic,
  5.2 / 97 / 1,770 µs per frame at 1,024 / 8,192 / 32,768 frames, while a filter-false dispatch is flat. The two
  deeppanic arms published under Native AOT (the default trim, not full) were still compiling when this record
  was posted, after more than 37 minutes of `ilc`. Their numbers land as a dated block here, as a commit on top.

  **2026-10-13 (G), the Native AOT arms.** The base arm (today's emitted shape, against the same golib) published and
  ran: 65,536-level wall unwind **115–227 ms**, against the JIT's 287–303 ms; 8,192 levels 9.6–11.2 ms; 1,024 levels
  1.1–1.4 ms (three runs each). The F-run arm was NOT measured under AOT. Its publish had spent about seven hours in
  `ilc` when it was stopped, to give TRAIN T4's linux legs a quiet box. The route is PARKED, so no re-run is owed.
  If it is ever unparked, its AOT arm is the first reading to take.

**go-cmp's cyclic subtests** (`TestDiff/Transformer/Cyclic`, both in one run, the published test host):

| Arm | Cyclic pair, wall | Whole `cmp` package, alternated runs | Verdicts |
|---|---|---|---|
| today | 20 s (4 runs) | 21.2 s, 21.0 s | 288 PASS / 3 FAIL |
| adopt-only, `compareAny` only | 18–19 s | not run | both subtests PASS |
| F-run, all 7 non-test catches in 3 files | 17–19 s | 19.7 s, 18.5 s | 288 PASS / 3 FAIL, **fail set identical** |

The three FAILs on both sides are the two pointer-keyed map rows disclosed 2026-10-05 plus their parent
`TestDiff`, compared as sets.

**Prediction, scored: HIT** ("stays 18–20 s"). A first whole-package read of today at 44 s against F-run's 18 s
was a first-run artifact. The alternated rerun was ordered to settle it, and its prediction ("both within a few
seconds") also hit.

**So the motivating case does not move.** Panic unwind is at most about 10% of go-cmp's 20 s floor. The BOARD's
note (2026-10-12) lists two other candidates: one OS thread per goroutine, and `reflect.Call` boxing and copies.
Neither is measured. A `dotnet-stack` sampler returned nothing at that stack depth, so this record does not say
where the 18 s goes.

## 5. Footprint (item 4)

**Zero emitted lines is impossible, and that is measured, not assumed.**
- Every emitted catch names the static `GoFrame.IsPanic`, and a route that decides per frame needs the frame
  (§1). So the filter's text must name `ᒐ`.
- Counted at master `226afb810a` (`git grep` on the tree, comment lines excluded). The doc-comment example
  inside golib is the one `///` hit, and it is not counted.

| Tree | Emitted catch lines (change) | Files | of which `… return default!; }` | Open-coded-defer `finally` lines (keep today's shape) | Files |
|---|---|---|---|---|---|
| `src/core` | 2,579 | 620 | 602 | 317 | 93 |
| `src/tests` | 603 | 273 | 41 | 22 | 8 |
| **both** | **3,182** | **893** | **643** | **339** | **101** |

- The `return default!;` variant is a catch body for a function with results. Its body keeps the `return` behind
  `GoFrame.AfterFilter();`.
- **The converter side** is one template, `goFrameTail`, plus the open-coded-defer exclusion.
- **The golib side** is:
  - `RunInFilter` and `AfterFilter`;
  - the foreign and Goexit handling inside the filter (§2, P3);
  - the mapped-fault identity (§3, row 5);
  - the captureCallers rule (§3, rows 1-2);
  - the raise-under-lock census (§3).
- A cut would still owe the standard footprint reading: the two-seeded `-stdlib` reconvert on three targets, plus
  the `-tests` footprint. The tree count above is what is committed today, not that reading.

## 6. Recommendation and the asks

**G recommends PARK.**
- The gain is real only for deep panic recursion. deeppanic moves about 5x; go-cmp moves at most about 10%; and no
  validated row is held on panic-unwind cost.
- The price:
  - a one-line change to every one of 3,182 emitted catches;
  - the captureCallers re-derivation, the riskiest part, gated by `TestCallers*`;
  - a lock-ordering hazard class that does not exist today.
- If the owner wants the route anyway, F-run is the version to build, not F-adopt. The order of work:
  1. the raise-under-lock census;
  2. golib `RunInFilter` with full P3 handling and the mapped-fault identity, behind a GolibTests contract that
     reproduces §2 S1-S8 against `GoFrame` itself;
  3. the captureCallers rule, read against the runtime `TestCallers*` rows on all three OSes;
  4. the emitter (`goFrameTail`, open-coded frames excluded);
  5. the corpus footprint and the battery.

**Asks.**
- **COORD:** the emitted shape changes, so this goes to the owner as the decision COORD's rule names.
- **The owner:** adopt F-run (in the order above) or park it. The H4a slot COORD is holding can be released either
  way, because nothing here blocks it.

## Evidence

All of it is scratch, unpushed, and kept on the box that read it:
- the `filtersem` probe (source, JIT and AOT outputs);
- the `fcsem` probe (Go source, both converted arms, three outputs);
- the deeppanic and go-cmp arm scripts, each with its prediction stamped before the run;
- the prototype's golib diff (`GoFrame.cs`, one file).

The F-run arm's catch swap is a literal one-line substitution, and each arm's log proves the swap by counting the
lines before and after (deeppanic 2→2, go-cmp 7→7 in 3 files, fcsem 15→15, none of the old shape left).

— G
