# DESIGN — M:N N5: the policy for blocking the scheduler cannot see

**G, 2026-11-23.** Row 12 of the M:N .5 plan (G's PLAN, inbox/COORD 20261123T153840Z-G, accepted by COORD the same
day). This is a design: it changes no code. Row 13 codes it, and COORD rules on row 13's owner when this posts. Every
prediction and stop line in section 6 is written before any code. Section 8 holds the questions that need a ruling.

N5 was named by the second spike (DATA-mn-spike-2.md, N5): "a stated policy that cannot hang: seam coverage as a
contract (N6), and for what stays invisible an injection rule that slows down instead of stopping at a cap."

## 1. What the scheduler sees today, and what it does not

Read at the train 6 tip, 27eabd6619 (src/core/golib/runtime/GoScheduler.cs, Goroutine.Multiplexed.cs, GoNativeCall.cs).

| a carrier's thread is stuck in... | the scheduler sees it by | and answers with |
|---|---|---|
| a golib plain park (channel, select, a plain Mutex.Lock, WaitGroup.Wait, ...) | `HandOffBeforeBlocking` -> `GoScheduler.EnterBlocked` | the P is handed off at once (`handoffs`) |
| a native call through the seam (`GoNativeCall.Enter`, the runtime's entersyscall family) | the goroutine's transition depth is odd | the monitor retakes the P after 2 ticks (about 4 ms) while work waits (`retakes`) |
| the same, entered as `entersyscallblock` (`NativeCallHint.Blocks`) | as above: **the hint is read by nothing** (GoNativeCall.cs remarks) | the same 2-tick retake, where Go hands the P off at once |
| a managed wait outside golib: `lock`/`Monitor`, `System.Threading.Lock`, `SemaphoreSlim`, `ManualResetEvent(Slim)`, `Task.Wait`, `Thread.Sleep` in hand-written or consumer C# | **nothing** | injection (below) |
| a native call that does not enter the seam (a consumer's P/Invoke; any BCL path that blocks in native code without passing the runtime's entersyscall family) | **nothing** | injection |
| a long step that never parks (a CPU loop, a pure spin; Gosched is a park since the Gosched seat) | the step's age | injection |

**Injection** (`Monitor`, GoScheduler.cs:722): a step that has run past `GO2CS_MN_PREEMPT_MS` (10 ms, Go's
forcePreemptNS), net of GC pauses, while work is queued (or a carrier waits for a P), gets one extra P. It is
spent **once per step** (`carrier.Injected`), and at most `GO2CS_MN_MAXINJECT` (default: the P count) injected Ps are
live at once. A released P beyond the budget retires.

**Where that fails, measured:**
- Hang: 40 natively blocked goroutines on 12 Ps, with the retake off (spike 2, 5.5).
- Hang: P5's S2 arm 1, `MN_SPIN_PURE=1` (spinners with no Gosched past the P count), 5 of 5 at 20 s (P-S1-13). The
  spinners' steps never end, each spends its one injection, and the budget is gone.
- No bound on wake latency under a saturated P set: an unlocked wake every 25 ms against 12 CPU steps of 200 ms reads
  p50 15.4 ms, p99 57.8 ms (pin record, M7's fourth run). With 4 s steps and 12 pinned parked, wakes read p50
  1.0-2.0 s, p99 3.0 s (pin record, 11.1's shape). COORD moved P10's absolute bound here.

Go has none of these. Every Go syscall enters `entersyscall`, so nothing blocks invisibly. Its Ms are unbounded up
to `debug.SetMaxThreads` (10,000, fatal past it), and a long step is preempted asynchronously. go2cs cannot preempt a
running .NET thread. So the policy has two halves: make every block it can visible, and for what stays invisible,
add parallelism without a hard stop.

## 2. A fact the policy rests on, measured first (the probe)

**Question.** Can the monitor tell, from its own thread and cheaply, that a carrier's thread is blocked in a managed
wait?

**Probe.** src/tools/mnspike2/MnWaitSense (this seat). A thread is blocked in each case below, and another thread
reads its `Thread.ThreadState` at +50 ms and +200 ms. Then 2,000,000 cross-thread reads of a spinning thread's state
are timed.

| case | windows (.NET 10.0.400) | linux, WSL Ubuntu 22.04 (.NET 10.0.403) |
|---|---|---|
| Thread.Sleep; Monitor.Enter contended; Monitor.Wait; System.Threading.Lock contended | WaitSleepJoin | WaitSleepJoin |
| SemaphoreSlim.Wait; ManualResetEventSlim.Wait; ManualResetEvent.WaitOne; Task.Wait | WaitSleepJoin | WaitSleepJoin |
| an anonymous pipe's Stream.Read; a native sleep P/Invoke; libc read(2) on an empty pipe | running (Background only) | running |
| a spin | running | running |
| one cross-thread `ThreadState` read | 11.9 ns | 10.8 ns |

The same eight managed waits read WaitSleepJoin on both OSes, at both instants. A native block and a spin read as
running. So managed waits CAN be sensed, and native ones cannot. At 500 monitor ticks a second and P = 12 carriers,
the reads cost about 70 µs per second (0.007% of a core). osx-arm64 is not read. The coder reads it on the hosted
mac leg first, before relying on it there (prediction I0).

## 3. The policy

Four parts. (a) and (b) make blocks visible; (c) bounds what stays invisible; (d) shrinks what stays invisible.

### 3a. Sense managed waits (a retake, not an injection)

The monitor already walks the carriers each tick, for the retake. It adds one test beside the native-depth test. A
carrier is **sensed** when all of these hold:
- it is in a step (`RunningSince != 0`) with a goroutine mounted;
- it holds its P (not `Retaken`, not counted blocked by a golib park);
- its thread reads `ThreadState & WaitSleepJoin` on two ticks in a row (`NativeTicks`' twin counter).

A sensed carrier is retaken exactly as a native call is, through the same code: `Retaken`, `s_blocked`, the pinned
flag (DESIGN-mn-pin.md 15.2), `ReleaseP`, `EnsureCarrier`. It is outside the injection budget and settles when the step
ends or parks. It runs only while work waits, as the retake does. A new counter, `sensed`, joins the READING line.

The carrier keeps its `Thread` (one field). The exclusions matter:
- an idle carrier waits on its own semaphore, but it is not in a step;
- a carrier back from a block waits for a P (`s_pWaiters`), but it holds no P;
- a pinned carrier waiting for its goroutine is not in a step.

**The cost of a false positive** is one extra carrier: a short wait over 2 ticks (about 4 ms) is retaken and then
returns. This is the retake's existing cost, and it is bounded the same way (the over-budget P retires at release).

### 3b. Honor the Blocks hint (N6's first request)

`GoNativeCall.Enter(NativeCallHint.Blocks)` on a multiplexed goroutine hands its P off at once (`GoScheduler.EnterBlocked`),
and the matching `Exit` takes one back (`ExitBlocked`), as Go's `entersyscallblock` does. `MayBlock` keeps the
2-tick retake. Callers today: the runtime's `entersyscallblock` (darwin's os_darwin.cs:166, and the two public
funnels in syscall_managed_impl.cs). With the scheduler off this is inert: there is no P to hand off.

### 3c. Injection that re-arms, under a thread ceiling

Two changes to `Monitor`'s injection loop:
1. **Re-arm per threshold, not once per step.** A step earns an injection each time it passes another
   `GO2CS_MN_PREEMPT_MS` while work waits (`carrier.InjectedAt` replaces `carrier.Injected`). A tick injects at most
   as many Ps as there are waiting units, as pin 11.2's rule already limits for P waiters, now with the run queue's
   length counted too. So injection follows demand: one wake against P long steps gets one P within about one
   threshold. The P retires when its step ends, as today.
2. **The live-injection cap becomes a thread ceiling.** `s_maxInject` (= P) goes. Injection and carrier addition stop
   only at `GO2CS_MN_MAXTHREADS` carriers in total (default 10,000, Go's SetMaxThreads default). At the ceiling the
   monitor stops injecting and counts `ceiling-hits` in READING. It does not crash. That differs from Go's fatal
   "thread exhaustion", and it needs a ruling (Q1).

What this buys: a pure spin past the P count completes (it degrades to a thread per spinner, which is what the
default mode does); a wake under saturation waits about one threshold, not a step; the 40-native-blocked hang cannot
happen below the ceiling. What it costs: oversubscription in the cases that used to hang or crawl, which is the
default mode's shape. Storm-shaped programs park within a threshold and earn nothing new (prediction I7).

`GO2CS_MN_MAXINJECT` is kept as an arm only (the old cap, for the made-to-fail rows), refused nowhere: like the other
GO2CS_MN_* arms it is an environment variable, never a build property.

### 3d. The seam as a contract: a census, then brackets

The seam is entered today by the runtime's entersyscall family: linux's `syscall.Syscall` (syscall_linux_impl.cs),
windows' trampoline, and darwin's `entersyscallblock`. The spike read `os.File.Read` on linux and darwin as
unbracketed at its base. Whether that still holds at this tip is **not read here**: it is the census's first row.

**Census instrument** (row 13, step 1). A fixture runs each stdlib call that can block, with K = 4P goroutines blocked
in it at once, under GO2CS_MN=1 with READING. It runs with sensing off and then on, and reads `handoffs`, `retakes`,
`sensed` and `injections` per call. A call that moves only `injections` is invisible. The candidate list: file read
on a pipe, net Accept and Read, os/exec Wait, signal.Notify's receive, time.Sleep's plain path, os.Stdin read,
and DNS lookup. Each invisible call gets a named owner: a bracket seat for its funnel, or a disclosure.
Brackets are separate seats, not this one.

**Consumer C#.** A consumer's managed wait is sensed (3a). A consumer's own blocking P/Invoke is invisible unless it
brackets the call with `GoNativeCall.Enter` / `Exit`. That type is public but hidden from IntelliSense, and it is NOT
in the [mn-frozen] set. Documenting it as the consumer contract is a surface decision (Q2).

## 4. What does not change

- No preemption. Gosched stays a park (the Gosched seat). The pin (DESIGN-mn-pin.md) is unchanged; a sensed pinned
  carrier takes the retake's pinned path.
- The scheduler stays OFF by default. The monitor starts with the first multiplexed goroutine (`GoScheduler.Ready`),
  so with GO2CS_MN unset it never starts: 3a and 3c cost nothing, and 3b's hand-off is skipped (no P).
- golib's visible parks and the native retake keep their thresholds.
- N1 (does-not-park set): a cut site calls the plain body, whose park is a golib park, so it is visible (handed off).
  N10 (thread-statics to the goroutine): 3a reads a `Thread` object and touches no thread-static.

## 5. The fixture: MnInvisible

src/tests/Behavioral/MnInvisible, Go source plus a hand_impl.cs, as MnForeignThread does: under Go the hooks are nil
and the program falls back to Go-visible blocking. Under go2cs, hand_impl.cs sets them before main. Rows (selected by
`MN_INV_ONLY`):
- **V1, managed wait.** K goroutines block in `ManualResetEventSlim.Wait` on an event that a goroutine queued AFTER
  them sets. Today this hangs once K exceeds P + MAXINJECT (the setter never gets a P).
- **V2, native wait outside the seam.** The same, blocked in a native sleep-poll loop (P/Invoke, 1 ms polls) on a
  flag that the late goroutine sets. Invisible to 3a, so it is answered by 3c alone.
- **V3, pure spin.** P5's MnSpinCap `MN_SPIN_PURE=1`, by reference: the row is P5's, read again here.
- **V4, wake latency.** P CPU twins in 200 ms steps (and a 2 s variant), plus one goroutine woken by a timer every
  25 ms for 5 s: wake-to-run p50 and p99. This is M7's shape without the pin.
Output (V1-V3): Go's lines, plus `done` per row. V4 prints only whether the wake count is complete; its latencies go
to stderr, with no Output phase.

## 6. Predictions and stop lines (before code)

The base is the row-13 tip with 3a-3c switched off (`GO2CS_MN_SENSE=0`, `GO2CS_MN_REARM=0`; arms only). P = 4
(hosted linux) and P = 12 (this box), unless stated.

| # | prediction | stop line |
|---|---|---|
| I0 | osx-arm64 reads WaitSleepJoin for the 8 managed waits, as windows and linux do (the probe, hosted) | any managed wait reads running: 3a is windows+linux only, and we post before building it |
| I1 | V1 at K = 4P: base HANGS (20 s timeout, 3 of 3); with 3a, completes 3 of 3 with `sensed` >= K - P - MAXINJECT, `injections` near 0 | the cure hangs once in 10 |
| I2 | V2 at K = 4P: base HANGS 3 of 3; with 3c, completes 3 of 3, peak carriers <= K + P + 2 | a hang; or peak carriers over 2K + P |
| I3 | V3 (P-S1-13): HANG at base 5 of 5, completes 5 of 5 with 3c | a hang |
| I4 | V4, 200 ms steps: p99 <= 20 ms with 3c (base 57.8 ms); 2 s steps: p99 <= 25 ms (base 3.0 s, pinned shape) | p99 over 25 ms, or over 2x the threshold + 2 ticks |
| I5 | 3b: a GolibTests row, `Enter(Blocks)` under MN hands off before the next monitor tick (`handoffs` +1, `retakes` +0). It is red with 3b removed | - |
| I6 | an idle program, and MnMixedStorm n=1,000: monitor CPU unchanged within the A/A band (3a's reads are about 70 µs/s) | a measurable cost beyond A/A |
| I7 | no regression: battery `--mn` (87) PASS; the Q1 kit 0 differing; MnContended, MnSpinCap, MnForeignThread PASS; pin M3b guard; storm gate 50 x n=100,000 twinned, 0 hangs, peak OS threads within +10% of row 7's (windows 42-58, linux 37-44) | any red; storm threads over +25% |
| I8 | MnContended's `sensed` stays under 1% of its parks (short Monitor waits rarely last 2 ticks) | over 5%: the 2-tick test is too eager; post first |
| I9 | `GO2CS_MN_MAXTHREADS=16` on V2 with K = 64: completes slower or stalls, `ceiling-hits` > 0, and no crash | a crash |

Each of 3a, 3b and 3c is made to fail: remove it, and its own rows (I1, I5, I2/I3/I4) go red by name. Then restore
it and check byte-identical. The census of 3d is a reading with no stop line; its table goes in the row-13 record.

## 7. Order and size for row 13

1. MnInvisible plus the census instrument, red at the base (I1-I3 hang): 0.5-1 d.
2. 3b, the hint (golib, a few lines, GolibTests row I5): 0.25 d.
3. 3a, sensing (Carrier.Thread, a second counter, the retake path, READING `sensed`): 0.5 d.
4. 3c, re-arm and ceiling (`InjectedAt`, a demand-limited tick, `GO2CS_MN_MAXTHREADS`, `ceiling-hits`): 0.5-1 d.
5. Gates I0-I9, made-to-fail, the census table: 0.5-1 d.

Total 2.25-3.75 lane-days (the PLAN said 2-3). Steps 2-4 touch GoScheduler.cs, GoNativeCall.cs and
Goroutine.Multiplexed.cs, which are N10's files too, so N10 and row 13 run serially (PLAN row 13's note).
S2 arm 2's second re-read (P5, PLAN row 10) follows row 13.

## 8. Questions for a ruling

- **Q1, the ceiling.** At `GO2CS_MN_MAXTHREADS` (default 10,000), stop injecting and report (proposed), or fail fatally
  as Go does at SetMaxThreads? Stopping can still hang past 10,000 concurrent invisible blocks; a fatal error names
  the cause. `runtime/debug.SetMaxThreads` could later set it; that is not proposed here.
- **Q2, the consumer contract.** Document `GoNativeCall.Enter`/`Exit` as how consumer C# makes a native block
  visible (it becomes supported surface, and joins the [mn-frozen] set), or keep it internal-in-spirit and rely on
  3c for consumer native blocks?
- **Q3, the census's brackets.** Each invisible stdlib call the census names gets a bracket seat. Owners are routed
  by COORD when the table posts. None is in this design's size.

## 9. COORD's rulings, and what they change (dated block, 2026-11-23, G)

COORD ruled on the design as posted (87a2754a16). Sections 1-8 above are kept as written; this block amends them.

- **Q1, the ceiling: FATAL, as Go.** At `GO2CS_MN_MAXTHREADS` carriers the runtime dies with Go's own text,
  `runtime: program exceeds N-thread limit` and then `fatal error: thread exhaustion`. `runtime/debug.SetMaxThreads`
  sets the same ceiling and returns the old value. Parity outranks graceful degradation (the owner's standing
  preference): a silent stop is a hang the user cannot see, which is what N5 exists to remove. KnownIssues discloses
  the ceiling beside its default. This replaces 3c's "stops injecting and counts `ceiling-hits`".
  **I9 is restated:** `GO2CS_MN_MAXTHREADS=16` on V2 with K = 64 dies with Go's two lines on stderr and Go's fatal exit
  code (2). A second row sets the ceiling through `debug.SetMaxThreads(16)`, and its return reads the old value
  (10,000). Stop line: a hang, a different text, or a crash with no text.
- **Q2, `GoNativeCall.Enter`/`Exit`: YES.** It becomes a documented consumer contract, in the ADVANCED tier of the
  public-surface doctrine, marked and warned like the other advanced members, and it joins the [mn-frozen] set.
  Row 13 owes one user-doc paragraph and one behavioral project that calls it from consumer C#. That project is a
  new prediction, **I10**: its blocked native call is handed off or retaken (`retakes` or `handoffs` moves,
  `injections` does not), under GO2CS_MN=1, with output equal to Go's.
- **Q3, bracket owners: routed when the census table posts.** By default a stdlib site goes to the lane on row 13,
  and a disclosure goes to P4's docs row (PLAN row 18).
- **I0 runs before row 13 starts,** on the hosted mac. If WaitSleepJoin does not read there, stop and post: the
  policy changes shape. The instrument is GolibTests `WaitSenseTests` (this block's seat), so every OS's golib-tests
  leg keeps the premise checked after row 13 lands.
- **Row 13 is R's,** after N10 (serial, the same files). H stays stood down for account A's runway.

Section 7's size grows by the debug.SetMaxThreads binding, the doc paragraph and the I10 project: about +0.5 day,
2.75-4.25 lane-days in all.
