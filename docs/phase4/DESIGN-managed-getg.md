# DESIGN — a managed `getg()` over the goroutine registry (Q40)

**Design only. This document SIZES and does not cut**; the cut is a separate queue item once this is
ruled. Dated 2026-09-04, written at master `8f82b3f63` against the registry as widened by SUB-Q27
(`claude/sub-q27` @ `d1e1300a4`, train 25); amended by dated blocks, never rewritten.

## 0. The one-paragraph version

`getg()` is the one runtime symbol with no body on any flavour that stands in front of every measured
runtime death the fleet has recorded this week. A census of its readers — two derivations, reconciled
to the site — says the argument for leaving it a stub (`proflabel_impl.cs`: *574 sites; a body converts
loud throws into quiet partial behaviour*) has the population right and the mechanism wrong: **202 of
280 sites read `gp.m`**, and at 100 of the 102 reachable ones `m` is the FIRST thing read, so a `g`
with a nil `m` does not go quiet, it throws a `NullReferenceException` one frame later — loud, foreign,
non-recoverable, and ANONYMOUS, which is worse than the stub that names itself. The honest shape is
therefore not "a `g`" but **"a `g` and the `m` that IS its thread"**: in golib every goroutine owns a
dedicated OS thread, so one `m` per goroutine with `curg` set and no P is a description of the managed
scheduler, not a modelling choice, and under it every measured death moves to the NEXT genuine wall —
`asmcgocall`, `getcallerpc`, `mcall`, `setitimer` — still named by the stub generator. The quiet class
(a plausible zero read from the replaced representation) is **24 sites, of which 3 to 5 are reachable,
each named below**. What the design buys is named doors and an honest `runtime.Getg()`; what it does
NOT buy is a verdict: **none of the three acceptance rows is predicted to pass**, and the falsifier the
item asked for is `gp.m.p` — 133 reads at reachable sites, 37 (windows) / 38 (linux, darwin) of them
dereferences no honest zero can satisfy, which is where the scheduler's replaced representation
actually begins.

## 1. Where `getg` stands — and one correction to the item's framing

- Declared `internal static partial ж<g> getg();` in the flat `runtime/stubs.cs:31`; bodyless on
  windows, linux and darwin; the `PartialStubGenerator` throwing stub on all three.
- **Windows bill** (`CENSUS-runtime-semantic-bill.md`, C1's linux census `5a15d08b4`):
  `TestAddrRangesAdd` — `NewAddrRanges → addrRanges.init → persistentalloc → systemstack → persistentalloc1
  → acquirem → getg`.
- **Linux death neighbourhood** (C1, `5a15d08b4`): `TestDebugCall`'s `debugCallWorker` —
  `LockOSThread` (displaced, a no-op by construction) → `runtime.Getg()` (`export_test.go:573`) → `getg`,
  a goroutine fault at the same moment as `TestCrashWhileTracing`'s log-after-completion fault, the
  host dead at position 57 of 436.
- **CPU-profile class** (SUB-Q27's ungated read, mailbox): `StartCPUProfile → SetCPUProfileRate →
  setcpuprofilerate → getg` (`TestAtomicLoadStore64` first).
- **Darwin `libcCall`** — the item names its first statement as the keystone precondition. That was true
  when `DESIGN-darwin-run-layer-2.md` §2.2 was written and is **no longer true at master**: increment 2
  (`88f01638c`) DISPLACED `libcCall` onto `runtime/darwin/libccall_impl.cs` through
  `manualConversionFuncs`, so the converted body that opens with `getg()` is never executed. The census
  below confirms it — the darwin flavour's only corpus path into `libcCall` (`fcntl > libcCall`) ends in
  the displaced body and reaches no `getg` site. The darwin acceptance row is therefore discharged
  BEFORE this design, not by it (§8.4).
- The corpus has NO other consumer: the whole banked roster is green, a reached `getg` is a foreign
  exception `recover()` cannot adopt (`GoFrame.IsPanic` adopts `PanicException` only), so a reached site
  is a red row — and there is none.

## 2. The reader census — two derivations, reconciled to the site

### 2.1 Derivation 1 — typed `go/ast` over the pinned GOROOT, per GOOS

Instrument: `go/packages` (`x/tools v0.36.0`, the converter's own pin) loads `runtime` with
`Tests: true` for one `GOOS`/`amd64`, `CGO_ENABLED=0` (the corpus's emission state), toolchain
`go1.23.12` proven by bare `go version`. The `runtime [runtime.test]` variant is walked — production
files plus the internal `_test.go` files — and every `getg()` call (resolved through `TypesInfo.Uses`,
never by spelling) is followed to the fields its result flows into: direct selectors
(`getg().m.locks++`), aliases tracked by `types.Object` identity (`gp := getg()` … `gp.preempt`),
second-level aliases (`mp := gp.m` … `mp.p.ptr()`), and the non-field uses (passed to a callee,
compared, returned, stored). A field path is recorded with its mode (read / write / addr).

| GOOS | files (prod + internal test) | production `getg()` sites | internal-test sites |
|:--|--:|--:|--:|
| windows | 150 + 8 | **280** | 18 |
| linux | 165 + 13 | **269** | 23 |
| darwin | 158 + 11 | **266** | 19 |

### 2.2 Derivation 2 — a regex over the EMITTED `src/core/runtime`, per flavour

Instrument: every `getg()` occurrence in the flat folder plus the flavour's per-GOOS folder (comment
tails stripped; the declaration excluded), attributed to its enclosing column-0 method, its fields read
through the emitted forms — `(~gp).f`, `gp.Value.f`, `getg().Value.f`, `(~getg()).f` — and given an
ordinal within the method so it can be joined to derivation 1 on `(file, function, ordinal)`.

| flavour | emitted sites | files | matched to derivation 1 | field sets agree | differ | only in Go | only in C# |
|:--|--:|--:|--:|--:|--:|--:|--:|
| windows (flat + `windows/`) | **266** | 52 | 264 | 249 | 15 | 16 | 2 |
| linux (flat + `linux/`) | **255** | 51 | 253 | 237 | 16 | 16 | 2 |
| darwin (flat + `darwin/`) | **251** | 50 | 249 | 234 | 15 | 17 | 2 |

**The site counts reconcile exactly.** Windows: 280 − 13 sites inside bodies the registry DISPLACES
(`lock2`, `unlock2`, `notesleep`, `notetsleep_internal`, `notetsleepg`, `getgcmask`, `LockOSThread`,
`UnlockOSThread`, `lockOSThread`, `unlockOSThread`, `Stack` ×2, `callers` — the emission holds a
placeholder there, no call) − 1 site inside the bootstrap `init` the converter marks *not run*
(`exithook.Goid = func() uint64 { return getg().goid }`, `proc.go:315`) = **266**, the two "only in C#"
entries being Go's `main` emitted as `Main` (its two sites are the "only in Go" `main` pair). Linux
269 − 13 − 1 = 255; darwin 266 − 14 (`libcCall` joins the displaced set) − 1 = 251. **Every one of the
15–16 field-set differences is the C# regex's**, not the Go derivation's: an address-taken field is
emitted through `gp.of(g.Ꮡfield)` and the regex reads none (`parkingOnChan`, `selectDone`, `_defer`,
`gcAssistBytes`, `schedlink`, `lockedm`); `OrTypedNil` is a golib helper the regex mistook for a field;
one function (`unlockAndRun`) reuses the alias name `gp` across three sites, which the regex merges.
Derivation 1's field sets are the ones counted below; derivation 2 confirmed them site by site.

### 2.3 The three earlier counts, reconciled

`proflabel_impl.cs`'s header says **574 sites across 92 files**; a mailbox scouting note says **560**;
a raw token grep at master says **582 lines across 93 files**. All three are grep counts of the TOKEN
over all three flavours' folders at once — comments, the declaration and the per-GOOS triplication
included (`proc.cs` alone contributes 71 × 3). The call-line count over the same three folders is
**517**; the per-flavour figure a body would actually face is **266 / 255 / 251**; the Go-side figure
with the displaced bodies restored is **280 / 269 / 266**. None of the three headline numbers was wrong
as a token count; none of them is the number of readers.

## 3. Buckets — what each reader needs, and whether the managed runtime can say it truthfully

Classes, per the item, by the FIRST field on the path from the `getg()` result:

- **H — honest today (or with SUB-Q27's widening at train 25).** `goid` ← `Goroutine.Id` (the number
  `Stack(all)` already prints, `managed_impl.cs`), `parentGoid` ← `ParentId`, `gopc` ←
  `GoSyntheticPC.Of(Creator)`, `startpc` ← `GoSyntheticPC.Of(Entry)` (Q27), `labels` ← Q27's per-goroutine
  mirror (`GetProfileLabels()`), and for the CALLING goroutine — the only one `getg()` ever returns —
  `atomicstatus = _Grunning` and `waitreason = waitReasonZero` are true by construction: a goroutine
  executing `getg()` is running. (The registry's parked state and wait reason matter only for a `g`
  reached through `allgs`/`sudog.g`, which this design does not populate — §11.)
- **P — honest by persistence.** Plain state whose only writers are converted code on the same `g`
  (`_panic`, `_defer`, `param`, `paniconfault`, `throwsplit`, `sig*`, `writebuf`, `racectx`, `waiting`,
  `timer`, `sleepWhen`, `selectDone`, `coroarg`, `parkingOnChan`, `activeStackChans`, `nocgocallback`,
  `raceignore`, `ancestors`, `goroutineProfiled`, `trace`, the tracking fields), or a scheduler/signal
  path the managed model does not run, where ZERO is the true answer (`preempt`, `preemptStop`,
  `preemptShrink`, `asyncSafePoint`: no preemption is ever requested of a managed goroutine).
- **C — a cheap registry widening would answer.** `waitsince` (a park timestamp the registry does not
  keep). One site reads it; it is a `write` on the syscall path (§4). Nothing else lands here.
- **R — the replaced representation.** `m` and everything under it; `stack`, `stackguard0/1`, `sched`,
  `syscallsp/pc/bp`, `stktopsp`, `schedlink`, `lockedm`, `gcAssistBytes`, `gcscandone`, `inMarkAssist`,
  `stackLock`. Split in two because they FAIL differently: **R-m** (the site reads `m`/`lockedm`: a nil
  `m` throws at the first dereference) and **R-zero** (the site reads a scalar of the replaced
  representation and gets a plausible zero — the only class that can go QUIET).

A site's bucket is the worst of its fields (`R-zero` > `R-m` > `C` > `H/P`), and **N** is a site that
reads no field at all — identity only: `getg() == gp.m.curg`, `ready <- getg()`, `return getg()`.

| bucket | windows | linux | darwin | reads |
|:--|--:|--:|--:|:--|
| R-m | **189** | 178 | 176 | `m` at 202 / 191 / 189 sites (72 % of production sites) |
| R-zero | 24 | 24 | 23 | `stack` 12, `stackguard0` 11, `syscallsp` 5, `sched` 4, `syscallpc/bp` 3 each, `stackguard1` 3, `schedlink` 2, `gcAssistBytes` 1 |
| C | 0 | 0 | 0 | `waitsince`'s one site is bucketed R-zero by its other fields |
| H/P | 35 | 35 | 35 | `preempt` 6, `racectx` 6, `param` 6, `_defer` 5, `goid` 5, `coroarg` 4, `labels` 4, `_panic` 3, `throwsplit` 3, `waiting` 3, `parkingOnChan` 3, `activeStackChans` 3, … |
| N | 32 | 32 | 32 | identity only |
| **total** | **280** | **269** | **266** | |

Writes to an H field by converted code — the case where the registry and the `g` could disagree —
number **exactly one** on every flavour: `runtime_setProfLabel`'s `gp.labels = labels`
(`proflabel.go:43`), which `proflabel_impl.cs` already displaces onto the mirror. No reachable site
writes `goid`, `startpc`, `gopc` or `parentGoid`.

## 4. Reachability — what a body could actually touch

Bucket totals say what the readers WANT; they say nothing about which readers a program reaches. Two
root sets, one typed intra-package call graph (methods resolved by receiver type through
`TypesInfo.Selections`, never by name; `systemstack(fn)` follows `fn`; a call to a THROWING stub — a
Go-bodyless declaration with no hand-own body, `getg` itself excluded since this models the corpus
after the cut — makes the straight-line statements after it dead; a displaced or whole-file-hand-owned
body is never entered):

- **Corpus roots** — the non-displaced runtime functions the converted corpus REFERENCES, derived by a
  grep over every `.cs` outside runtime's own assembly with comment tails and string literals stripped
  (a frame name in a test string is not a call): **25 functions** — `BlockProfile`, `CPUProfile`,
  `Caller`, `CallersFrames`, `GOROOT`, `MemProfile`, `MutexProfile`, `NumCPU`, `ReadTrace`,
  `SetCPUProfileRate`, `SetMutexProfileFraction`, `ThreadCreateProfile`, `Version`, `readMetricNames`,
  the four `reflect_*Off`, `signalWaitUntilIdle`, the five `signal_*`, `windows_GetSystemDirectory`.
  Exactly **two** of them reach a `getg` site: **`SetCPUProfileRate`** (the measured CPU-profile door)
  and **`ReadTrace`** — referenced from `runtime/trace`'s converted `trace.cs`, whose only caller
  `trace.Start` is a hand-own that refuses (`trace_impl.cs`), so this root is referenced and not live.
- **Suite roots** — every runtime function the external `runtime_test` package references, read off
  its type info: **193 / 197 / 195** functions. This is the runtime row's own exposure.

| reachable production sites | from corpus roots | + suite roots | first field read is `m` (R-m) |
|:--|--:|--:|--:|
| windows: R-m / R-zero / H-P / N | 55 / 1 / 4 / 4 = **64** | 102 / 3 / 11 / 17 = **133** | 100 of 102 |
| linux | 48 / 1 / 4 / 4 = **57** | 88 / 5 / 11 / 17 = **121** | 86 of 88 |
| darwin | 48 / 3 / 4 / 4 = **59** | 88 / 5 / 11 / 17 = **121** | 86 of 88 |

Read against the totals: **a body can touch at most 133 of 280 sites on windows; 64 of those from the
corpus's own roots — 42 behind `SetCPUProfileRate`, 22 behind `ReadTrace`, whose caller refuses — so
the live corpus exposure is one root**; the static set is an UPPER bound in two known ways (858 interface
method calls resolve to no receiver and are dropped; the throwing-stub cut applies at top-level
statements only, so `setProcessCPUProfiler`'s `stdcall3` inside an `if` does not cut the `newm` path
the tool then walks — §8.3 traces that path by hand) and a LOWER bound in none that matters here (a
function value stored and called later is followed only in the "upper" variant, which adds 4–14 sites).

**The R-zero population — the only readers that can go quiet — reachable by name:**

| site | flavour | reads | root | what the zero does |
|:--|:--|:--|:--|:--|
| `releasem` (`runtime1.go:611`) | all | `preempt`, `stackguard0` | corpus (`ReadTrace → gopark`) | `gp.preempt` false → the `stackguard0 = stackPreempt` restore is skipped: the honest no-preemption branch |
| `deductAssistCredit` (`malloc.go:1337`) | all | `gcAssistBytes` under `m.curg` | suite (`GostringW → mallocgc`) | reads `m` first (§5) |
| `gcTestPointerClass` (`mgc.go:1953`) | all | `stack.lo/hi` | suite (`GCTestPointerClass`) | a stack pointer classified as not-stack: a WRONG answer in a test whose subject is the memory model — the E3 shape |
| `entersyscallblock` (`proc.go:4599`) | linux, darwin | `sched.*`, `syscall*`, `stack.*`, `stackguard0`, `throwsplit` | suite (`WaitForSigusr1`) on linux; corpus (`signal_recv`) on darwin | reads `m.locks` first, then writes the replaced context with zeros and `casgstatus`es: the quiet class proper |
| `exitsyscall` (`proc.go:4676`) | linux, darwin | the same set plus `waitsince` | as above | reads `m.locks` first, then `m.oldp.ptr()` — a P dereference (§10) |

That is the whole quiet-partial population a body can reach: **five sites on the unix flavours, three on
windows, one of them on a live corpus path**, and that one (`releasem`) takes the honest branch.

## 5. The measured doors, traced statement by statement

Two shapes are traced, because the trace is what decides between them:

- **(A) a `g` alone** — `m` nil, every other R field zero.
- **(B) a `g` and its `m`** — one `m` per goroutine, `curg` set, no P, no g0, no gsignal (§6).

| row | today | under (A) | under (B) |
|:--|:--|:--|:--|
| Windows `TestAddrRangesAdd` | `NotImplementedException: getg` in `acquirem` (via `persistentalloc1`) | `gp.Value.m.Value.locks++` → **`NullReferenceException` in `acquirem`**, one frame later, foreign and non-recoverable, naming nothing | `acquirem`/`releasem` proceed on `m.locks`; `mp.p == 0` takes the global-allocator branch Go itself has for an M without a P; `lock(&globalAlloc.mutex)` is the displaced managed lock; `persistent.base == nil` → `sysAlloc` → `sysAllocOS` → `stdcall4(_VirtualAlloc)` → `stdcall` writes `mp.libcall` → **`NotImplementedException: asmcgocall`**, named |
| Linux `TestDebugCall` family | goroutine fault on `getg` in `debugCallWorker`, host dead at position 57 | `runtime.Getg()` returns the honest `g`; `InjectDebugCall` reads `gp.lockedm == 0` → `plainError("goroutine not locked to thread")` → `t.Fatal` → the deferred `after()` stops the worker cleanly | identical — `lockedm` is never set because `LockOSThread` is a no-op by construction, and setting it would only reach `tkill(tid, SIGTRAP)` against a managed thread |
| CPU-profile class (`TestAtomicLoadStore64` …) | `NotImplementedException: getg` in `setcpuprofilerate` | `gp.m.locks++` → **`NullReferenceException`**, anonymous | `m.locks` proceeds; `setThreadCPUProfiler(0)` writes `m.profilehz`; the `prof.signalLock` CAS succeeds; `setProcessCPUProfiler(hz)` → windows `stdcall3(_CreateWaitableTimerA)` → `stdcall` → **`asmcgocall`**; linux `setThreadCPUProfiler(0)` returns before any stub (no timer exists yet), then `setProcessCPUProfiler(hz)` → `setProcessCPUProfilerTimer` → **`NotImplementedException: setitimer`** — named either way |
| Darwin `libcCall` | displaced (`88f01638c`); `getg` not executed | no change | no change |

Three facts the table rests on, each read from the source rather than assumed: `systemstack` is hand-owned
as `fn()` (`stubs_impl.cs`), so `persistentalloc` reaches `persistentalloc1`; `print(...)` in the converted
runtime binds golib's builtin, not `printlock`, so a runtime `throw(msg)` already prints its `fatal
error:` line today and dies at `fatalthrow`'s `getcallerpc()` BEFORE its own `getg()` — a `throw` is not
a door this design moves; and `~` on a null `ж<T>` is a CLR `NullReferenceException` on the operator's
null operand, not golib's nil-pointer panic — `GoFrame.IsPanic` adopts only `PanicException`, so under
(A) the death is as unrecoverable as today's and less informative.

### 5a. AMENDMENT 2026-09-08 (C1) — the SECOND of those three facts expires with this commit's own train, and the conclusion it supports gets STRONGER

The sentence above — *"a runtime `throw(msg)` … dies at `fatalthrow`'s `getcallerpc()` BEFORE its own
`getg()` — a `throw` is not a door this design moves"* — was TRUE when it was written and is TRUE at
master. ⚠ **Line numbers below are named with their TREE, because this amendment's own commit moves
them** — the same layer discipline the two-pin citation rule asks for, one axis over. **At
`origin/master`**: `runtime/panic.cs:1090` `@throw` calls `fatalthrow(throwTypeRuntime)` at `:1098`,
`fatal` calls `fatalthrow(throwTypeUser)` at `:1118`, and `fatalthrow` (`:1265`) opens with
`getcallerpc()`, a bodyless partial the generator fills with a throw. **It stops being true in the train
that carries this amendment**, and the sharpest evidence is on this very branch: `panic.cs` no longer
declares `@throw` or `fatal` at all, while `fatalthrow` survives at `:1214` and `fatalpanic` at `:1245`
— **the declarations stand and their callers are gone**, which is precisely what "unreached rather than
unimplemented" means. The fatal-path chain (`claude/c1-fatal-path-body` `8fdbd4704`, train 46)
displaces `throw` and `fatal` through `manualConversionFuncs` onto `runtime/panic_impl.cs:67` and `:76`,
whose bodies do nothing but forward to golib's `FatalReport.Fatal`, which formats Go's report and calls
`Environment.Exit(2)` (`golib/runtime/FatalReport.cs:141`). `fatalthrow` is never called, so
`getcallerpc()` is never reached. That commit's own header records the census the displacement rests on:
`fatalthrow` had exactly two callers and `fatalpanic` one (already dead at its own `getcallerpc()`), so
**`fatalthrow`, `fatalpanic`, `getcallerpc` and `getcallersp` all become UNREACHED rather than
unimplemented.**

**The MECHANISM becomes false; the CONCLUSION stands and is strengthened.** A `throw` is still not a door
this design moves — after the chain it is a *deterministic exit at the primitive* rather than a death at a
different stub one frame earlier, which is a cleaner reason for the same claim. Nothing in §5's table
moves: no row's "today", "(A)" or "(B)" column mentions `throw`.

⚠ **§8.2 does not rest on this fact.** The `TestDebugCall` family reaches `getg` through
`runtime.Getg()` (`export_test.go:573`) from `debugCallWorker`, and has nothing to do with `throw`; its
prediction is unaffected. A census over every record in `docs/` found this premise load-bearing in
**exactly one place** — here — outside the fatal arc's own documents.

**Why the amendment rides the chain rather than landing earlier or later.** Committed on its own it would
be FALSE on master; left to a landing-day fixup it would depend on an owner this record does not name.
Appended here, on the branch that carries the change, it is true on every tree it can reach. Per the
record-amendment rule it is a dated block: the original sentence above is left standing exactly as
written, because a premise that expired is evidence about when it was true, not an error to erase.
(COORD ruling, mailbox `f916391`.)

## 6. The shape — (B), and why the `m` is not the replaced representation

**What is minted.** On the first `getg()` from a thread: one heap box of `g` and one heap box of `m`,
linked both ways (`gp.m = mp`, `mp.curg = gp`), cached in a `[ThreadStatic]` slot of `runtime_package`
and returned by every later call from that thread. golib gives every goroutine its own dedicated
thread for its whole life (`Goroutine.cs`, the executor's stated policy), so a thread-static IS
goroutine identity here — the same fact golib's own `t_current` rests on — and an `m` that names that
thread, with `curg` the goroutine it runs, is a **true statement about the managed scheduler**, not a
modelling choice: there is one M per goroutine and there are no Ps. R's tracer design labelled
"one P per OS thread" a modelling choice; this is the opposite case, because golib literally
constructs it.

**What is populated, and from where** (nothing else):

| field | source | when |
|:--|:--|:--|
| `g.goid`, `g.parentGoid` | `Goroutine.Current.Id`, `.ParentId` (internal, visible to `runtime` through golib's `InternalsVisibleTo("runtime")`) | at mint |
| `g.gopc`, `g.startpc` | `GoSyntheticPC.Of(Creator)`, `GoSyntheticPC.Of(Entry)` — Q27's PC space, train 25 | at mint |
| `g.atomicstatus`, `g.waitreason` | `_Grunning`, `waitReasonZero` — true of the caller | at mint |
| `g.labels` | Q27's mirror, `Goroutine.GetProfileLabels()` (`object?`, the `unsafe.Pointer` `runtime_setProfLabel` stored) | **on every call** — it is the one H field programs mutate |
| `g.m` / `m.curg` | each other | at mint |
| everything else on `g` and `m` | its zero value, under one comment block naming the classes: stack bounds and scheduling context (replaced); P, g0, gsignal linkage (absent by construction); counters and bookkeeping (`locks`, `printlock`, `mallocing`, `throwing`, `dying`, `preemptoff`, `lockedExt/Int`, `libcall*`, `profilehz`: honest by persistence — the converted code that increments them is the code that reads them) | never written by the mint |

**A thread with no goroutine** (`Goroutine.Current` is null — a host thread that never ran Go code)
mints the same pair with `goid = 0`, which is precisely the id `runtime.Stack` already prints for such
a thread (`appendGoroutineHeader`: *id 0, which is not a goid Go's allocator ever mints*). No new
convention is introduced.

**The g0 assertions fire as Go's own throws.** Inside `systemstack(fn)` the managed `fn()` runs on the
caller's goroutine, and (B) never pretends otherwise: a reader asserting `gp == gp.m.g0` sees false and
`throw`s its own message ("not on g0"), which golib's `print` writes before `fatalthrow` dies at
`getcallerpc`. That is the honest outcome — the code has named what it needed.

**Displacement mechanics.** `getg` is a BODYLESS partial, so writing a body displaces the stub by
construction (`PartialStubGenerator`'s predicate is `IsPartialDefinition && PartialImplementationPart
is null`): no `manualConversionFuncs` entry, no converter change, no two-seeded diff, no corpus
footprint. The body belongs in the flat `runtime/stubs_impl.cs`, whose header currently records the
decision to leave `getg` throwing "while no reachable path needs it" — that paragraph is rewritten in
the same commit (the hand-own's own scope header is corrected with the scope), and
`proflabel_impl.cs`'s *WHY THIS DOES NOT IMPLEMENT getg* paragraph is amended to point here, its
labels storage untouched (the mirror stays the source of truth; the `g` reads from it).

**The alternative cache, deliberately not taken now.** An opaque `object?` slot on `Goroutine`
(golib) would let a registry consumer reach a goroutine's `g` from another thread. No consumer needs
it: Q27's profile and Q28's tracer read the registry entry, not a `g`, and the converted readers that
walk other goroutines (`allgs`, `forEachG`, `sudog.g`) are not populated by this design (§11). It
would also add +8 B to every `Goroutine` corpus-wide and re-open GolibTests and the route-#7 compile
for a field nothing reads. It is the C-bucket widening if a consumer names itself.

## 7. Cost

- **Per goroutine that ever calls `getg`**: one `g` box and one `m` box, lazily. **PROVISIONAL sizes,
  from the emitted field lists, not measured**: `g` has 56 fields ≈ 430 bytes of fields (a `gobuf` is 7
  words, a `stack` 2), so ≈ 0.5 KB boxed; `m` has 72 fields including two arrays its struct initializer
  allocates eagerly (`tls` 6 words, `createstack` 32 words) and the `pcvalueCache`/`chacha8` state, so
  ≈ 1.5–2.5 KB. The cut measures both with `Unsafe.SizeOf` and an allocated-bytes probe in GolibTests
  and replaces these figures; they are not to be carried.
- **Who pays: nobody on the banked roster.** A reached `getg` is a foreign exception no `recover()`
  adopts, so a banked row that reached it would be red, and the roster is green. The reached set today
  is `runtime` (unbanked) and `runtime/pprof`'s CPU-profile class (unbanked). Corpus-wide the cost is
  ZERO by construction, and the doctrine's per-box byte-cost rule is not engaged: no field is added to
  `ж<T>`, and under the thread-static form none to `Goroutine`.
- **Per call**: one thread-static read plus one `AsyncLocal` read for the labels refresh — on the
  reached paths only, none of which is hot in the managed corpus.
- **Gates the cut owes** (stated now so the cut inherits them): no emission change, so CNR is not owed
  and `git status` proves it; `go2cs-stdlib.slnx` on all three targets (`stubs_impl.cs` is flat);
  no golib change, so GolibTests is unchanged and route #7's behavioral compile is not engaged; the four
  rows of §8 run gated, Release + tiering off, records preserved before any restore; and a filtered
  sweep of the banked rows that reference runtime roots (`runtime/debug`, `runtime/metrics`, `time`,
  `sync`, `os/signal`, `log/slog`) as the non-movement control — none reaches `getg` today, so the
  prediction is byte-identical verdicts.

## 8. Acceptance — each row stated as the door it moves to, never as a pass

Every prediction below is for shape (B); shape (A)'s predictions are the middle column of §5.

### 8.1 `TestAddrRangesAdd` (runtime, Windows) — predicted: `stub` → `stub`, one wall further
`NotImplementedException: getg` becomes `NotImplementedException: asmcgocall` from `stdcall` under
`sysAllocOS`, after `acquirem`, the global-allocator branch and the displaced lock have all proceeded.
The row's real wall is the persistent allocator's backing store — `sysAlloc` over `VirtualAlloc` and raw
chunk-list writes through `unsafe.Pointer` — which is a native-allocator hand-own, a separate sizing
(§12). **Falsifier for this row**: any exception other than `asmcgocall`'s between `acquirem` and
`stdcall` means the trace in §5 missed a reader.

### 8.2 `TestDebugCall` family (runtime, Linux) — predicted: goroutine fault → `divergence`
`debugCallWorker` no longer faults; the test fails on `InjectDebugCall`'s own
`goroutine not locked to thread` (Go = pass, C# = fail). Its subject — injecting a call into a
goroutine's registers through `SIGTRAP` on a thread the scheduler locked — is the replaced
representation itself; whether that is E3 is the OWNER's call under the ruling that a coordinator
cannot mint an exclusion class, and this design only names the shape. **The host death at position 57
is predicted to PERSIST**: C1's tail records a second, independent fault (`TestCrashWhileTracing`'s
goroutine logging after completion) at the same moment, and this design does not touch it. If the host
instead proceeds past 57, that two-cause reading was wrong and the 378 shadowed rows come into view —
a falsifiable statement either way, and the cheaper of the two to be wrong about.

### 8.3 The CPU-profile class (runtime/pprof, `TestAtomicLoadStore64` first) — predicted: `stub` → `stub`, named
Windows: `asmcgocall` from `stdcall3(_CreateWaitableTimerA)` inside `setProcessCPUProfiler`, before
`newm` (the static graph's 42-site `newm` tail on windows is the top-level-only cut's over-approximation;
the hand trace stops at the `stdcall`). Linux: `setitimer` from `setProcessCPUProfilerTimer` under
`setProcessCPUProfiler` (`setThreadCPUProfiler(0)` returns before its `timer_*` stubs while no timer
exists).
The class's disposition is unchanged from SUB-Q27's reading — SIGPROF sampling has no managed analogue —
and this design does not claim otherwise; it moves the wall from a symbol the runtime could have
answered to the one it cannot.

### 8.4 Darwin — predicted: no door moves
`libcCall` is displaced; no darwin-flavour reader is reachable from a corpus root that is live; the
darwin census differs from linux's only by `libcCall` joining the displaced set and by
`entersyscallblock`/`exitsyscall` being reachable through `signal_recv`. The first mac dispatch's next
death is not predicted here, because nothing in this design is on its path.

### 8.5 Non-movement, the gate rather than the hope
`runtime.NumGoroutine`, `runtime.Stack`, the goroutine profile and every banked row: byte-identical.
`runtime.Getg()` and `Goid()` (export_test) answer the registry's id — the first time a runtime test can
read the caller's identity — and `Goid()` is predicted to agree with the `goroutine N` header
`Stack` prints on the same goroutine, which is a one-line guard the cut adds.

## 9. The reply to `proflabel_impl.cs`, on the census's numbers

The header's argument: *"referenced at 574 sites across 92 files … a body there converts 574 LOUD
THROWS into quiet partial behaviour over a `g` that models a fraction of what Go's carries — the
false-green shape this corpus treats as worse than the throw."*

1. **The count is a token count** (§2.3): 582 token lines over three flavours' folders including
   comments and the declaration; 266 emitted call sites per flavour; 280 readers in Go with the
   displaced bodies restored. The header was right that the population is large; it is a third the
   size it reads as, and only 133 of it is reachable by the runtime suite, 64 by the corpus.
2. **The mechanism is inverted for 72 % of the readers.** 202 of 280 sites read `m`; at 100 of the 102
   reachable ones it is the FIRST field read. A `g` alone therefore does not "operate on a fabricated
   descriptor" — it throws, one frame later, a `NullReferenceException` that `recover()` cannot adopt.
   That is the header's own outcome (loud) minus its one virtue (the name). The shape that would be
   quiet — the `g` alone — is the shape this design rejects, on the header's own reasoning.
3. **The quiet class is 24 sites, not 559**, and the reachable ones are five, named in §4 with what
   their zero does; one is on a live corpus path and it takes Go's own no-preemption branch.
4. **"A fraction of what Go's carries" is right about the stack and wrong about the identity.** The
   registry already states `goid`, `parentGoid`, the creator, the entry function, the labels and the
   parked state in `Stack(all)` and in the goroutine profile; the `g` restates them. What it cannot
   state — stack bounds, the `sched` context, P linkage — stays zero, and §10 names where that zero is
   reached.
5. The header's decision was the right one for the two functions it was written for, and it stays: the
   labels remain in the mirror, `runtime_getProfLabel` keeps its body, and the `g` reads from them.

## 10. The falsifier

The item asked for *a reader that needs a field only the real scheduler could fill and that no honest
zero can satisfy*. The census names it: **`gp.m.p`** — 133 reads at reachable sites, **37 (windows) /
38 (linux, darwin) of them dereferences** (`mp.p.ptr().palloc`, `mp.p.ptr().mcache`,
`mp.p.ptr().syscalltick`, and the `g0`/`gsignal` stacks beside them) with no nil test, which under (B)
fault as nil-linkage `NullReferenceException`s — the same anonymous class (A) would have produced one
frame earlier for every site — while the other **68 / 55 / 55** reachable R sites proceed on `m` state
that is honest by persistence (`curg` 84 reads, `libcall` 19, `locks` 15, `preemptoff` 8, `printlock` 4,
`mallocing` 3, `throwing` 2, `profilehz` 2 on windows). The scheduler's replaced representation begins
at the P, not at the `g`, and no honest value exists for it: there are no Ps.

Where that leaves the rows:

- None of the three measured rows reaches a P dereference; each dies earlier at a named wall (§8). Their
  dispositions are unchanged by this design: `TestAddrRangesAdd` **unimplemented** behind a native
  allocator; the CPU-profile class **unimplemented** behind SIGPROF (Q27's reading); the DebugCall family
  **E3-shaped**, the question the owner's.
- The rows that WOULD reach a P dereference are the runtime suite's scheduler, GC, stack and arena tests
  behind the suite roots `CountPagesInUse → stopTheWorld`, `NewUserArena → gcStart`,
  `ShrinkStackAndVerifyFramePointers`, `Scavenger.Start`, `GostringW → mallocgc` (the sites that
  proceed on honest `m` state end at one of the dereferences). Their subject is the memory model and the
  scheduler; this design claims none of them, and the E3-or-unimplemented reading of each class is per
  class and per the owner's bar, from the record of a run, not from this sentence.
- **If the cut's gated runs show a row moving to a plausible WRONG answer rather than to a named wall** —
  a `stack.lo/hi` zero read as a classification, a `sched` write taken as state — that row joins the §4
  table and the design's quiet-class count is falsified upward; the response is a per-site disclosure,
  never a fabricated value.

## 11. What this design does NOT buy

- **No verdict.** No acceptance row is predicted to pass; every door moves to a named wall.
- **No other goroutine's `g`.** `allgs`, `forEachG`, `sudog.g` and `m.curg` for a goroutine that is not
  the caller stay unpopulated; `tracebackothers` and the profile's per-goroutine stack walk are the
  `ForeignStackPlaceholder`'s territory, unchanged.
- **No P, no g0, no gsignal**, and no stack bounds or scheduling context — zeros, named as such.
- **No change to the labels' storage** (the mirror stays), to `Stack`, `NumGoroutine`, `gcount` or the
  goroutine profile.
- **No `runtime.Getg()`-based scheduler tests**: `RunGetgThreadSwitchTest` (cgo callbacks) and
  `G0StackOverflow` read what this design leaves zero and stay where they are.

## 12. SUGGEST items to COORD, outside this item's scope

1. **Windows `sysAlloc`/`sysAllocOS` over `VirtualAlloc`, and `persistentalloc` over native memory** — the
   real wall behind `TestAddrRangesAdd` (§8.1), a sizing of its own (the chunk list writes raw words
   through `unsafe.Pointer`; the native-backed-slice/array-view designs are the neighbourhood).
2. **`TestCrashWhileTracing`'s goroutine fault** (the host's log-after-completion guard) — the Linux
   runtime row's OTHER host-killer at position 57, independent of `getg`; until it is gated the 378
   shadowed rows stay unmeasured whatever this design does.
3. **`printlock`/`printunlock`, `acquirem`/`releasem`, `setcpuprofilerate`'s `m.locks` pair** — the
   mailbox's earlier "narrow hand-own" proposal is SUBSUMED by (B): with an `m` per goroutine these
   counters are honest by persistence and need no displacement; the proposal should be closed against
   this design rather than cut separately.
4. **`allgs` over `Goroutine.Snapshot()`** — the C-bucket widening that would let `tracebackothers`-style
   readers enumerate goroutines; not proposed until a consumer names itself.

## Appendix A — the instruments, reproducibly

- **Derivation 1**: `go/packages` load of `runtime` (`Tests: true`, `NeedSyntax|NeedTypes|NeedTypesInfo|
  NeedImports`, env `GOOS=<goos> GOARCH=amd64 CGO_ENABLED=0` and the pinned GOROOT); sites are
  `CallExpr`s whose `Fun` resolves through `TypesInfo.Uses` to `runtime.getg`; uses are classified by
  walking the parent chain (selector → path; `AssignStmt` → alias by `types.Object`, depth ≤ 3;
  `IncDecStmt`/LHS → write; `&` → addr; call argument → passed; `BinaryExpr` → compared; `ReturnStmt` →
  returned). Bucket map: §3. Call graph: `CallExpr` → `TypesInfo.Selections` / `Uses`; `systemstack(x)`
  → `x`; opaque = Go-bodyless without a hand-own body (`systemstack`, `procyield`, `nanotime1`,
  `cputicks` have one), the `manualConversionFuncs["runtime"]` set at master, the whole-file hand-owns
  `mfinal.go`/`runtime2.go`, and `mcall`; the throwing-stub cut at top-level statements. Roots: §4.
- **Derivation 2**: per flavour, flat + `<goos>/` `.cs`, comment tails stripped, `getg()` occurrences
  attributed to the nearest column-0 method head and given an ordinal; fields through the four emitted
  forms; joined to derivation 1 on `(file, function, ordinal)` with `initΔN → init` and `Main ↔ main`.
- Both ran at master `8f82b3f63`, GOROOT `go1.23.12` proven by bare `go version`, corpus emission state
  `CGO_ENABLED=0`; the tools live in the lane's scratch and are attachable on request. A re-derivation
  at a later tip must reproduce the reconciliation of §2.2 (280 − 13 − 1 = 266 on windows) before any
  number here is quoted from it — a bucket count is re-derived at the tip, never carried.
