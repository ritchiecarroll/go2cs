# Runtime Introspection

[Reference index](README.md) · [Summary of this topic](../ConversionStrategies.md#defer--panic--recover)

This page covers how a converted program reads its own stack: `runtime.Stack`, the panic traceback, and `runtime.Callers` with its frames, each rendered in Go's shape from the managed stack.

## Tracebacks

### `runtime.Stack` renders a GO-shaped traceback, and recovers the panic site

A traceback is **observable output**: Go programs print it on a crash, and Go's own tests grep it by
package-qualified function name (`sync`'s `TestOnceFuncPanicTraceback` looks for
`sync_test.onceFuncPanic`). Two things made the CLR trace unusable for that.

**Frame names.** A converted package's frames live on a `<pkg>_package` class inside namespace `go`,
so the CLR renders `at go.sync_test_package.onceFuncPanic() in …oncefunc_test.cs:line 191` — which
contains `sync_test_package.onceFuncPanic`, never `sync_test.onceFuncPanic`. `Stack` now formats
frames itself, in Go's shape (`<pkg>.<Func>()` then a tab-indented `<file>:<line>`), mapping
`go.<a>.<b>_package` → `<a>/<b>` (Go names a package by its import path, which the namespace mirrors)
and a closure's `<Outer>b__N` on its display class → `Outer.funcN`, Go's own spelling for a function
literal. A frame that is **not** converted Go code — golib, the BCL, the test host — keeps its .NET
name rather than being given an invented Go one, and Go's `+0x<offset>` PC deltas are omitted.

**Frames that already unwound.** Go keeps a panicking goroutine's frames physically on the stack until
the panic completes, so a `debug.Stack()` inside a deferred function shows the panic site; a CLR
exception has unwound them before the `finally`-based defer runs. Worse, *both* ways a panic travels
destroy the trace: re-raising the same instance (`throw ex`) resets `Exception.StackTrace` to the
re-raise point, and Go's re-panic idiom — `defer func(){ p := recover(); panic(p) }()`, which is
precisely how `sync.OnceFunc` replays a panic on every call — creates a brand-new panic in the
deferred frame. So golib snapshots the origin **once**, at the first (innermost, deepest) catch, into
`PanicException.PanicTrace`, and a panic raised while handling another *inherits* it. `GoFrame.Run()`
tracks which panic a deferred sequence is handling in a strictly save/restore-scoped thread-local
(`HandledPanic`, surfaced as `GoFuncRoot.InFlightPanic`) — `recover()` clears `CapturedPanic`, but
Go's traceback keeps showing the panicking frames for the rest of the sequence, and the strict scoping
is what stops the value from ever going stale. `Stack` appends those frames *below* the live ones,
which is where Go's traceback puts them too, since the deferred call runs on top of `gopanic`. Cost on
the non-panicking path is zero: the CLR fills the trace at throw time anyway, and nothing is
snapshotted unless a panic is actually caught.

### `runtime.Stack(all: true)` enumerates every goroutine, and each one names the wait it is parked on

The `all` flag was **read and ignored**: the header was a literal `goroutine 1 [running]:` and the dump
carried the calling thread alone. Both halves were untrue for any program with more than one
goroutine, and the second was a *contract* violation rather than a cosmetic one — the bracketed word
in a header is Go's **wait reason**, the runtime publishes it at every `gopark`, and Go's own tests
grep it (`runtime/pprof`'s `awaitBlockedGoroutine` builds a regex around
`^goroutine \d+ \[sync\.Mutex\.Lock\]:`; `runtime`'s `TestNumGoroutine` counts headers and requires the
total to equal `NumGoroutine()`).

**Park accounting** is what makes the word answerable. golib's `Goroutine` carries ONE field — a
`WaitReason`, whose `Zero` value *is* "running", so the state and the reason cannot disagree — and
every blocking primitive names its wait around the wait it already performs
(`docs/phase4/DESIGN-cooperative-scheduler.md` §5.3):

```csharp
using (Goroutine.Park(WaitReason.ChanReceive))
    parked.Park.Wait();                       // the EXISTING primitive, untouched
```

The scope is **accounting only**. It relocates no wait, re-opens no park/claim protocol, and adds no
`goready` side: the waker already signals the primitive and the woken thread un-marks itself when the
scope disposes. Cost is one volatile store to park and one to unpark, no allocation (`ParkScope` is a
`readonly struct`), and nothing per-`ж` or per-object — so there is no corpus-wide byte cost. A thread
with no goroutine identity (a BCL callback, `time`'s timer service thread, the finalizer) gets an
inert scope and writes nothing; a nested park restores the enclosing reason rather than clearing it.

The reasons are Go's own strings, copied verbatim from `runtime2.go`'s `waitReasonStrings`, and the
enum carries only the ones a go2cs park site actually sets — a reason nothing can set is a word no
dump can print. Adopted sites and the reason each names:

| Wait | Reason | Go's counterpart |
|---|---|---|
| channel send / receive | `chan send`, `chan receive` | `gopark(chanparkcommit)` |
| blocked `select` | `select` | `gopark(selparkcommit)` |
| nil-channel op, `select{}` | `chan send (nil chan)`, `chan receive (nil chan)`, `select (no cases)` | same reasons |
| `sync.Mutex.Lock` | `sync.Mutex.Lock` | `sync_runtime_SemacquireMutex` |
| `sync.RWMutex.RLock` / `.Lock` | `sync.RWMutex.RLock`, `sync.RWMutex.Lock` | `sync_runtime_SemacquireRWMutexR` / `RWMutex` |
| `sync.WaitGroup.Wait` | `semacquire` | `sync_runtime_Semacquire` — Go has no `sync.WaitGroup.Wait` reason |
| `sync.Cond.Wait` | `sync.Cond.Wait` | `notifyListWait` → `goparkunlock` |
| `internal/poll` fdMutex | `semacquire` | `poll_runtime_Semacquire` |
| `internal/poll` netpoller | `IO wait` | `netpollblock` |
| `time.Sleep` | `sleep` | `gopark(resetForSleep)` |

**What the dump renders.** The calling goroutine first, with real frames, exactly as before; then one
blank-line-separated block per other live goroutine, in goid order, each with a truthful header — the
id golib's registry minted and the reason its park recorded. Where Go prints the other goroutine's
frames, go2cs prints ONE line, `[stack unavailable: go2cs does not capture another goroutine's
frames]`, deliberately shaped so nothing could mistake it for a frame (no tab-indented position line,
no package-qualified name). **The CLR has no supported cross-thread stack walk** — `Thread.Suspend` is
gone, and ClrMD or EventPipe would mean a process snapshot per call inside what is typically a spin
loop — so the honest answer is a sentence, and fabricating plausible frames for a stack that was never
walked is the one thing a traceback must never do. Capturing a goroutine's OWN stack at park time is
Stage B of the scheduler design, held behind the synthetic-PC registry that would symbolize it.

Three further limits are stated rather than approximated: `running` covers Go's `_Grunnable` as well
as `_Grunning` (there is no P and no run queue, so a thread waiting for a core is indistinguishable
from one on it); Go's ` (scan)`, `, N minutes` and `, locked to thread` header decorations are omitted
(no GC of ours, no park timestamp, and `LockOSThread` is a no-op here because every goroutine already
owns its thread); and `NumGoroutine()` and the enumeration are two reads of one registry, so they agree
at rest and can differ while a goroutine is registering or retiring — the same direction as the count's
already-documented early-climb/late-decay. (Guarded by the `GoroutineWaitState` behavioral test — four
goroutines parked on a mutex, a channel receive, a select and a WaitGroup, read back through a
*normalized* reading compared against `go run`, with a negative arm for a reason no goroutine has and a
release arm proving every reason clears; plus `GolibTests`'
`GoroutineParkAccountingTests` for the strings, the nesting, the inert host-thread park, and the
header-count-versus-`NumGoroutine` agreement.)

### EVERY reader of a panic's trace gets the origin — not just `runtime.Stack`

The snapshot above served exactly one consumer. Every *other* reader — the Phase-4 test host, an
unhandled-exception dump, a debugger — asked `Exception.StackTrace` and got the wreckage the section
above describes, because a panic reaches its reporter by being re-raised from `GoFrame.Run()`
(`throw CapturedPanic.Value`, once the deferred sequence declined to recover it) and re-raising a
stored instance resets the trace to the re-raise point. So **every panic in the corpus reported the
same deepest frame — the defer drain — regardless of where it actually faulted.** That reads as
a defect in the defer machinery, and it hid the real one: nine of `time`'s failures were filed as a
shared "nil pointer dereference in the defer drain" when they were one nil-receiver deref in
`Location.lookup` (see the normalization idiom above), invisible until the trace was honest.

Two changes, both at the layer that owns the fact:

1. **`PanicException.StackTrace` reports the panic SITE first, then the frames it unwound through** —
   the closest a CLR exception can come to Go's single uninterrupted traceback. `PanicTrace` already
   held the origin; overriding the property is what lets consumers see it without any of them knowing
   the panic machinery exists. With no origin recorded the base trace is returned unchanged, so the
   override only ever *adds*.
2. **The origin is snapshotted at the ADOPTION POINT, `RuntimeErrorPanic.TryAsPanic`** — the one place
   a .NET exception becomes a Go panic. A mapped runtime error (nil deref, divide by zero) is
   synthesized there and was never thrown at the fault, so only the incoming exception carries those
   frames; once `TryAsPanic` returns, they are gone. Doing it in each *adopter* covered only panics
   that passed through a frame — a function that never defers gets no frame, so its fault
   travelled raw to the reporter, which synthesized a brand-new panic with **no trace at all**. That
   was five of the nine `time` rows: `panic: …` and nothing else.

(Guarded by `GolibTests.PanicTracebackTests`: a synthesized runtime-error panic escaping a `GoFrame`, an
explicit `panic()` surviving the re-raise, the same runtime-error panic adopted with **no** frame
anywhere between fault and reporter, a recovered panic that must not be reported at all, and a panic
with no origin snapshot falling back to its intact base trace. Not a behavioral test: no converted Go
program reads a CLR stack trace, so there is nothing to output-compare — the Go-observable half is
`runtime.Stack`, covered above.)

## Callers and frames

### `runtime.Callers` / `Frames.Next` walk the managed stack projected to GO-LOGICAL frames

Go's programmatic traceback — `pc := make([]uintptr, N); n := runtime.Callers(skip, pc)` expanded by
`runtime.CallersFrames(pc[:n])` — bottoms out in machinery with **no managed form**: `Callers` →
`callers()` opens with `getcallersp()`/`getcallerpc()` (assembly intrinsics) and hands the raw stack
to the runtime `unwinder`, and `(*Frames).Next` resolves each PC through `findfunc`'s linker-built
`funcInfo` tables. Converted faithfully they compile, then die on the first step — `io`'s
`TestMultiReaderFlatten`/`TestMultiWriterSingleChainFlatten` (relative stack-depth asserts, the
flatten optimization's only observable) threw `getcallersp: … not implemented` from inside
`runtime.Callers` itself.

The fork is the PROCESS-CONTROL one (section above): both API **contracts** — "record the calling
goroutine's frames as opaque PCs" and "expand PCs to function/file/line" — the CLR answers natively,
so `Callers` and `Frames.Next` are hand-owned in `runtime`'s `managed_impl.cs`
(`manualConversionFuncs["runtime"]`) over `System.Diagnostics.StackTrace`, while **`getcallersp`
stays an honest `NotImplementedException` stub**: a caller's stack pointer has no managed answer, so
the chain is severed at the API boundary that does — the same severing rule the reflection bridge
applied at `methodName`. `CallersFrames` itself is pure Go (slice bookkeeping) and stays auto.

What makes the projection *semantically* faithful is the **Go-frame filter**: a managed frame counts
only when it is a function the **Go source declares** — a free function or `[GoRecv]` receiver on a
`go.*` `<pkg>_package` class, a method on a struct nested in one, or a function literal (its compiler
display class nests in the same scope). go2cs **dispatch machinery is invisible**, exactly as Go's
interface dispatch adds no frame: generated adapter shells (any `IGoAdapter`) and go2cs-gen's
synthesized forwarders (`[GeneratedCode("go2cs-gen", …)]`, e.g. RecvGenerator's `ж`-overloads) are
dropped, as is everything outside a package class (golib, the BCL, the test host). Without the
filter, one Go-level interface call contributes 2–3 CLR frames and every relative-depth assert is
off; with it, **depth deltas between two `Callers` calls on one goroutine match Go's logical model**
— `io`'s `readDepth == myDepth+2` holds bit-exactly. Absolute depth still reflects the managed
host's own frames below `main`/the test runner, which cancel in any same-goroutine delta.

PC values are **opaque interned tokens** (index+1 into a process-lifetime record table keyed on
module × method-token × IL offset; 0 stays the invalid sentinel), never addresses — stable across
recurrences so pc-equality behaves as in Go, resolvable by any later `CallersFrames` walk, like Go's
permanent PC space. `Frame.Function` uses the Go spelling (`goFrameName`: `io_test.TestCopy`,
`runtime/debug.Stack`, `Outer.funcN` for literals); `Frame.File`/`Line` name the **converted `.cs`**
source — the source that honestly exists; `Frame.Func` stays nil (contract-permitted) and
`FuncForPC` remains a stub, since a `*runtime.Func` has no managed referent. A PC the runtime never
minted resolves like Go's `!funcInfo.valid()` — skipped, not fatal — and `more` is computed
precisely over the remaining *resolvable* tokens, mirroring Go's two-frame prefetch. Known fidelity
edge, recorded: Go's `<autogenerated>` promoted-method wrappers can occupy PC slots in a raw Go
`Callers` capture, while here **all** synthesized wrappers (Go-side and go2cs-side alike) are
uniformly invisible — refine only if a consumer differential ever lands on it.

**The two test-variant suffixes are NOT symmetric — `_internal_test` is stripped, `_test` is kept
(2026-08-26).** `goFrameName` derives the reported import path from the emitted namespace plus
package-class name, and the `-tests` pipeline emits a package's two test variants as two separate
classes: `<pkg>_test_package` for an external suite (`package slog_test`) and
`<pkg>_internal_test_package` for an in-package one (`package slog`). Go treats those two cases
differently, measured against the go1.23.12 toolchain rather than reasoned about:

| the test file declares | Go names the frame | why |
|:--|:--|:--|
| `package callerprobe` (internal) | `callerprobe.TestInternalCallerName` | the file is compiled INTO the package under test, so there is no separate package to name |
| `package callerprobe_test` (external) | `callerprobe_test.TestExternalCallerName` | a genuinely separate Go package, and Go spells it |

So `_internal_test` is a go2cs emission detail that must be stripped back off, while `_test` is a
fact about the Go build that must survive. The tempting generalization — strip any trailing `_test`
— is wrong in a way a banked row already measures: `runtime/debug`'s own `TestStack` greps a
rendered traceback for `runtime/debug_test.(*T).ptrmethod`.

[`DESIGN-position-map.md`](../phase4/DESIGN-position-map.md) §8 records that these two suffix rules
retire from the FILE half (which is RECORDED, so nothing is derived from a namespace any more) but
"remain necessary and unchanged for the FUNCTION half"; the internal-test half of that rule was
never actually landed there, and the leak was **systemic to the `-tests` pipeline rather than
package-specific** — every converted suite that inspects caller info saw it. `log/slog`'s
`logger_test.go` asserts `wantFunc = "log/slog.TestCallDepth"` and got
`log/slog_internal_test.TestCallDepth`; the fix moves that row from 190 to 194 matching verdicts
(`TestCallDepth`, `TestJSONAndTextHandlers` and its `/Source` + `/Source/json` subtests, plus
`TestRecordSource`'s depth-1 case). (Guarded by `GolibTests/CallerFrameTestVariantNamingTests`,
which pins all THREE emitted shapes — production, internal-test, external-test — in one file, so
neither rule can be "fixed" into the other; measured failing-first, the guard reports exactly the
internal-test shape and leaves the other two green.)

**A frame set read at Release + `DOTNET_TieredCompilation=0` is the set of frames the JIT chose to
KEEP — literal-frame naming is inlining-dependent, and at the validation configuration of record the
frame can simply be gone (2026-09-04).** `Frame.Function` can only name a function literal while
that literal's own frame is on the stack, and the CLR's `StackTrace` does not report inlined frames
— the same fact `captureCallers`'s `[MethodImpl(MethodImplOptions.NoInlining)]` pins one layer down.
Tiering is what had been hiding it: a test method runs ONCE, so under default tiering it is jitted at
tier 0, where nothing is inlined and the literal frame is always there. The validation configuration
of record is **Release with tiering off**, i.e. full optimization from the first call — and there a
one-expression lambda is inlined into its enclosing method, leaving the `.funcN` suffix nothing to
attach to. Measured on `CallerFrameTestVariantNamingTests`, one build, both configurations: all
three literal shapes answered the ENCLOSING frame at TC0
(`litguard/probe.recordedOuterLiteralFrame` for a guard wanting `…recordedOuterLiteralFrame.func2`)
and were green under default tiering. The guard now carries
`[MethodImpl(MethodImplOptions.NoInlining)]` on the **lambdas** as well as on the enclosing methods
— an attribute on a lambda expression reaches its synthesized backing method (verified in an
isolated probe: `implFlags=NoInlining`) — so what it measures is the naming rule rather than an
inlining budget.

The naming rule itself is unchanged; what is recorded here is the **reading**. Anything that
consults a frame set at Release+TC0 — a relative-depth assert, a count of host frames, a traceback
grepped for a literal's name — is reading an inlining decision unless every frame it depends on is
pinned, and a frame inlined away is indistinguishable from one the Go-frame filter above declined to
report. Same family as reflect's `valueMethodName`, whose faithfully transcribed stack climb was
correct in Debug and could not work in Release, and which was retired for a `[CallerMemberName]`
thread precisely because a compile-time constant is the one answer no tiering or inlining decision
can move ([`DESIGN-reflection-bridge.md`](../phase4/DESIGN-reflection-bridge.md)); and the sibling of
the tier-0 liveness rule recorded further down this section, where the same "test methods run once,
so they are jitted at tier 0" fact makes a GC-lifetime local look permanently live. (The literal's
COUNTER is recorded rather than derived —
[`DESIGN-position-map.md`](../phase4/DESIGN-position-map.md) §8 — which is orthogonal to whether the
frame exists to be named at all.)

**`runtime.Caller` works by severing the FUNNEL, not by hand-owning another public API
(2026-08-07).** The 2026-07-31 landing above hand-owned the exported `Callers`, which left
`runtime.Caller` — the far more widely used of the pair — still dead: its auto body calls the
*lower-case* `callers`, and that is the declaration whose first statement is `getcallersp()`. Four
call sites go through it (`Caller`, `mprof`'s profile recorders, `proc`'s `createstack`,
`tracestack`), so `callers` is the single funnel and hand-owning **it** is one entry on
`manualConversionFuncs["runtime"]` that fixes all four — and leaves `Caller` auto-converted and
Go-shaped, which hand-owning `Caller` would not. Go's own comment on the declaration says the same
thing from the other side: "almost identical to `Callers`", linkname'd by the ecosystem, signature
frozen — an API boundary with a managed answer.

The two differ in exactly one way that the managed walk has to encode: `Callers(0, pc)` names *its
own* frame, while `callers(0, pcbuf)` names *its caller's* (Go's body starts the unwinder at
`getcallerpc()`/`getcallersp()`, one frame up). The walk therefore lives in a private
`captureCallers`, and each entry point passes its own frame budget — `Callers` → `skip + 1`,
`callers` → `skip + 2` — because **every hop is itself a Go-source frame by the filter above**: they
are all declared on `runtime_package`, so the walker cannot exclude them structurally. All three
carry `[MethodImpl(MethodImplOptions.NoInlining)]`: the CLR's `StackTrace` does not report inlined
frames where Go's unwinder does (through the compiler's inline trees), so an inlined hop would
silently shift every answer by one. Guarded by the `RuntimeCallerFrames` behavioral test, which
asserts *relations* between frames rather than absolute positions — chiefly that `callerLine()` and
`wrapGrand()` invoked **on one source line** must report the same line, an equality that breaks
under an off-by-one in either direction (positive control: `skip + 1` in `callers` flips three of
its eleven output lines). The suite builds Debug, where nothing is inlined, so the guard was also
run against a **Release** build of the same program — identical output, so the pins hold under the
optimizing JIT and not merely under the one that could not have broken them. `io`'s banked flatten
asserts re-validate at 59/59 across the refactor.

**What `Caller` can and cannot honor.** `pc` is the same opaque interned token `Callers` mints, and
`ok` is exact — including the past-the-stack case, which returns Go's zero values with `ok == false`
rather than inventing a frame. Measured against a Go control on the two things a caller actually
does with a `pc`: `runtime.CallersFrames([]uintptr{pc}).Next()` — the branch `log.Output` takes when
a `pc` is supplied — **resolves** to the same file/line `Caller` returned, while
`runtime.FuncForPC(pc)` returns **nil**, which is Go's own `!funcInfo.valid()` answer rather than an
error or a fabricated `*Func` (the inverse-atomic rule: a `*runtime.Func` has no managed referent, so
none is minted). One fidelity edge in the same probe: `Frame.Function` renders the *emitted* method
name through `goFrameName`, so the program's own top frame reads `main.Main`, not Go's `main.main` —
the same "a candidate's EMITTED name is not always its GO name" class recorded elsewhere.
`file`/`line` name the **converted `.cs`** position, consistent with
`Frame.File`/`Line` above and with the honest-answer rule: the running program's source *is* C#, and
no Go-position map is emitted today. That fully serves a caller asking "where am I" — `log`'s
`Lshortfile`/`Llongfile` prefix produces `log_test.cs:67: hello 23 world` where Go produces
`log_test.go:63: …`, and `testing/slogtest`'s `withSource` labels its cases with a real file:line
instead of panicking in a package initializer. It does **not** serve a test that asserts the *Go*
file's own geometry: `log`'s `TestAll` pins `` `.*/[A-Za-z0-9_\-]+\.go:(63|65):` `` — the `.go`
extension and the exact line numbers of the `Printf`/`Println` calls inside `log_test.go`. That is
**not** a disclosure candidate: unlike `alloc-profile` or `codegen-liveness` it is not unsatisfiable
at any layer go2cs owns — a Go-source position map (emitted `#line` directives, or a side-car map
consulted by `internCallerFrame`) would satisfy it exactly. It is a deferred capability, and `log`
stays unbanked until one lands rather than being disclosed around.

**The path is SPELLED Go's way even while it names the `.cs`.** Which source a frame's path points
at and how that path is written are separate questions, and only the first is deferred. Go records
source paths with **forward slashes on every platform**: measured on Windows,
`runtime.Caller`/`CallersFrames` there answer `C:/Program Files/Go/src/runtime/proc.go`, never the
host's native separator. The CLR hands back whatever the PDB holds, which on Windows is
backslash-separated, so until 2026-08-19 every converted program answered `C:\…\log_test.cs` where
Go would have answered `C:/…/log_test.go` — a divergence in the *spelling* on top of the deferred
one in the *target*. `goSourcePath` (`runtime/managed_impl.cs`) now applies Go's rule at the two
places a frame's file reaches a program — `internCallerFrame`, which every `Caller`/`Callers`/
`CallersFrames` answer is interned through, and `appendGoFrames`, which renders `runtime.Stack`'s
traceback. This is a fidelity fix, not a cosmetic one: the string is observable, Go's own suites
match patterns against it (`flag`'s `TestDefineAfterSet` asserts `` `.*/flag_test.go:.*` ``), and
converted `path/filepath` accepts either separator on Windows exactly as Go's does, so no consumer
pays for the normalization. It also makes `log`'s `Lshortfile` trim work, since that trim looks for
the last `/`. Guarded by `RuntimeCallerFrames` (five assertions across `Caller`, a
`Callers`/`CallersFrames` walk, and a rendered traceback; trivially true on a forward-slash host, so
the guard bites on Windows, where the two spellings differ).

Worth carrying into whichever lane takes the position-map arc: **`#line` by itself does not close it.**
Measured — a `#line 852 "C:/Program Files/Go/src/flag/flag_test.go"` region does make
`StackFrame.GetFileName()`/`GetFileLineNumber()` answer with that file and line, so the PDB route
needs no golib change; but Roslyn **resolves and normalizes the directive's path**, and no spelling
of it survives with forward slashes on Windows (absolute, unix-rooted and relative forms all came
back backslash-separated). `#line` therefore supplies the `.go` extension and the Go line, and the
separator normalization above supplies the rest — the two halves compose, and `#line` does not close
`.*/flag_test.go:.*` on its own. One qualifier measured in the same probe, so the claim is about the
directive and not about Roslyn generally: the **`PathMap` compiler option** *does* emit a
forward-slash path (`-p:PathMap=C:\Program Files\Go\src\=/goroot/` yielded
`/goroot/flag/flag_test.go`), so a `#line` emission could in principle carry the separator itself.
It is the worse instrument for this even so — it is a whole-compilation option that rewrites every
source path in the PDB, the emitted `.cs` positions included; it needs the GOROOT prefix as a
per-package build property; and it does nothing for `runtime.Stack` or for any frame reached without
a directive. `goSourcePath` covers all of those with no build-time configuration, which is why the
separator half is settled at the runtime rather than at the compiler.

Two further costs the arc should be priced with, both measured rather than argued: a CS diagnostic
inside a `#line` region reports its position in the **Go** file
(`C:\…\flag_test.go(854,17): error CS0029`), which relocates every compile diagnostic in the corpus
away from the emitted C# the census workflow reads; and a per-statement emission adds roughly
**28–47%** more lines to a converted file (measured on `flag/flag.cs`, `strings/strings.cs` and
`edwards25519/edwards25519.cs`), interleaved between every statement, against the project's
reads-like-Go goal. The side-car alternative pays neither of those but adds a file and a csproj item
per package.

---

[Index](README.md)
