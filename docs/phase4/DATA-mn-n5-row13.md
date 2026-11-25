# DATA — M:N row 13 (N5): blocking the scheduler cannot see, as built and measured

**R, 2026-11-25.** The record of row 13: docs/phase4/DESIGN-mn-n5-invisible-blocking.md, sections 3, 5, 6 and 7 as
amended by section 9 (COORD's rulings). Branch claude/r-mn-n5-invisible, base master cb6def82a2 with row 4 (37fede0ea6)
and N10 (d4600c7264) merged no-ff, as COORD ruled (inbox/R/20261125T000600Z-COORD.md). A point-in-time record: amended
with dated blocks, never rewritten. Every prediction was written before its run; the appendix holds them as written,
each followed by its score.

## 1. What was built

| part | where | what it does |
|---|---|---|
| fixture | src/tests/Behavioral/MnInvisible | V1 managed wait, V2 native wait outside the seam, V4 wake latency, V5/V6 the GoNativeCall contract (Blocks / MayBlock), V7 SetMaxThreads past the ceiling, census rows C-* |
| 3b | GoNativeCall.Enter / Exit | Enter(Blocks) on a multiplexed goroutine hands its P off at once; Exit takes one back |
| 3a | GoScheduler's monitor | a carrier in a step, holding its P, whose thread reads WaitSleepJoin on two ticks in a row is retaken (`sensed`) |
| 3c | GoScheduler's monitor | a step earns its (n+1)th injection at n+1 thresholds; a tick injects no more than the work waiting; no live cap |
| Q1 | GoScheduler.AddCarrier, SetMaxThreads | GO2CS_MN_MAXTHREADS carriers (default 10,000): past it, Go's two lines and exit 2; runtime/debug.SetMaxThreads sets it |
| Q2 | PublicSurface.golib.txt [mn-frozen] | GoNativeCall and NativeCallHint frozen; KnownIssues.md documents the consumer contract |
| I4 gate | .github/workflows/os-matrix.yml | behavioral-smoke 'MnInvisible;mn=1' runs V4 directly and fails on a twinned p99 over 25 ms |

**Additions to the design** (COORD accepted both, 2026-11-25): the injection walk skips a carrier the monitor has seen
inside a native call or a sensed wait (on R-LAPTOP the monitor's 2 ms sleep can last a 15.6 ms timer quantum, and a
MayBlock call was injected before its second retake tick); I5 and I10 are MnInvisible rows (V5, V6) from the fixture's
consumer C#, because GolibTests cannot switch the scheduler on in process. **Fixes found on the way:**
GoScheduler.Reading() reads the block counters before its thread sample (on linux the sample, which reads /proc for every
thread, ran a measuring step past the threshold and counted its own injection); runtime's SetMaxThreadsManaged sits at
the end of runtime_package so no cited line above it moves.

## 2. Gates I0-I10

| # | result | reading |
|---|---|---|
| I0 | MET (G, before the row) | osx-arm64 and osx-x64 read WaitSleepJoin for the eight managed waits (run 45214659319) |
| I1 | MET | V1 hangs 3/3 at the base and with GO2CS_MN_SENSE=0; with 3a true 3/3, sensed 49-64, Windows and linux |
| I2 | MET | V2 hangs 3/3 at the base and with GO2CS_MN_REARM=0; with 3c true 3/3, peak carriers 65 (bar 82) |
| I3 | MET | MnSpinCap PURE with 32 extra spinners hangs 3/3 at the base, completes 3/3 (the default 4 extra did not hang the base: scored MISSED) |
| I4 | MET at the gate of record | hosted linux, 4 cores (run 45546636858): twinned p99 12.0-12.1 ms (200 ms steps), 6.0-7.0 ms (2 s); floor 3.0-4.0. R-LAPTOP: p99 79-117 at P=16 against a floor of 17-44 (the host-only tail, BOARD 2026-11-25) |
| I5 | MET | V5: a Blocks call hands off (handoffs rose, retakes did not) 3/3 Windows and linux; false 3/3 with 3b removed |
| I6 | no measurable cost | process CPU, MnMixedStorm n=1000: tip 1766 / 1812, SENSE=0 1875 ms (medians); C-sleep 2000 / 1781 / 1781 |
| I7 | MET | battery at 1f3210616b: repoguard 0; GolibTests 3197/0; --mn subset 91, Output 88/0/3 NOT COMPARED; default subset 3/3 |
| I8 | MET | MnContended sensed 36-42 of 86,631-87,888 parks (0.04-0.05%) |
| I9 | MET | V2 with GO2CS_MN_MAXTHREADS=16 and V7 (SetMaxThreads(16) returns 10000) each die with "runtime: program exceeds 16-thread limit" / "fatal error: thread exhaustion", exit 2, 3/3 |
| I10 | MET | V6: a MayBlock call is retaken with fewer than k/3 injections in its window, 3/3 Windows and linux; false 3/3 with the injection skip removed |

**Made to fail**, each restored byte-identical: 3b removed (V5 false), 3a removed (V1 false), 3c removed (V2 and V3x hang),
the ceiling check removed (I9's run survives), the injection skip removed (V6 false). The I4 step itself fired under
GO2CS_MN_REARM=0 in WSL (2 s-step arm p99 1705-1707 ms, three "::error::N5 I4" lines, exit 1).

## 3. The seam census (3d)

K = 4P goroutines blocked in one call at once, GO2CS_MN=1, one run each. "visible" = handed off or retaken; injections
alone = invisible. Windows: R-LAPTOP, P = 16, K = 64. Linux: WSL Ubuntu 22.04, P = 10, K = 40.

| call | Windows, sensing off (base) | Windows, sensing on (tip) | linux, sensing off | linux, sensing on | reading |
|---|---|---|---|---|---|
| pipe read | handoffs 63, inj 0 | 63, 0 | 40, 6 | 40, 13 | visible |
| net Accept | 195, 0 | 193, 2 | 115, 13 | 117, 9 | visible |
| net Read | 197, 0 | 200, 1 | 113, 1 | 117, 2 | visible |
| os/exec Wait | 834 + retakes 54, inj 41 | 836 + 49, inj 9 | NOT MEASURED | NOT MEASURED | mostly visible on Windows; linux: os.Executable is dotnet here, the child exits at once |
| time.Sleep | 0, 0 | 0, 0 | 0, inj 31 | 0, inj 29 | Windows a park; linux INVISIBLE (injections only) |
| channel | 64, 0 | 64, 0 | 40, 17 | 40, 5 | visible (a golib park) |
| os.Stdin read | 63, 0 | 63, 0 | 39, 23 | 39, 23 | visible |
| DNS lookup | 127, inj 16 | 127, inj 49 | 328, inj 46 | 324, inj 51 | PARTLY INVISIBLE on both |

Linux's small injection counts beside a full handoff count are main's own spawning step (traced for V1/V6: goroutine 1,
20-60 ms). Owners per Q3: time.Sleep on linux and DNS lookup on both are bracket-seat candidates (default: the lane on
row 13); os/exec Wait on linux needs a reading with an apphost (hosted runner) before it is routed.

## Appendix: the predictions as written, with their scores

### row13/PREDICTIONS-base.md

```text
Row 13 step 1, red at the base (written before the runs, 2026-11-24). Tree: claude/r-mn-n5-invisible at 8bd8ed0e61 plus
the uncommitted MnInvisible fixture (no N5 code yet, so this IS the base 3a-3c-off arm). R-LAPTOP, P = 16, K = 4P = 64.
MnInvisible built twinned (runner --mn), each row run directly with GO2CS_MN=1 GO2CS_MN_READING=1 and a 20 s timeout:
- V1 (managed wait): HANG 3 of 3 (main never gets a P back after its sleep: 16 carriers + up to 16 injected, 64 blocked).
- V2 (native wait outside the seam): HANG 3 of 3, the same arithmetic.
- V4 (200 ms steps, 200 wakes of 25 ms): completes 3 of 3 ("V4 every wake ran: true"); p99 wake latency well over
  20 ms (the pin record read 57.8 ms at P = 12).
- MnSpinCap MN_SPIN_PURE=1 (V3, P5's row by reference): HANG at 20 s, 3 of 3.
The runner's own --mn run of MnInvisible: Output NOT MEASURED (run timeout) or FAIL, not PASS.
SCORED (base rows): V1 HANG 3/3 MET; V2 HANG 3/3 MET; V4 completes 3/3, p99 87.7/90.3/94.3 ms (over 20 ms) MET.
V3 MISSED: MnSpinCap MN_SPIN_PURE=1 COMPLETED 3/3 (injections 15-16). Its default MN_SPIN_EXTRA=4 spinners past P fit in
the 16-P injection budget; P5's hang (P-S1-13) needs more spinners past P than the budget. Re-run written before it:
MN_SPIN_EXTRA=32 (2P past P) -> HANG 3/3 at the base.
The runner's --mn run: MnInvisible Output NOT MEASURED (run timeout) MET; MnSpinCap (PURE off) PASS.
SCORED V3x (MN_SPIN_EXTRA=32, written before it): HANG 3/3 MET (PowerShell deadline kit; Git Bash's timeout once failed to
kill a starving MnSpinCap, stopped by PID).
Census at the base (Windows, one run each, sensing does not exist yet): handed off: pipe read 63, Accept 195, net Read 197,
stdin 63; C-exec handoffs 834 + retakes 54 + injections 41 (Wait partly invisible); C-dns handoffs 127 + injections 16;
C-sleep 0 (a timer park, not a block); C-chan 64.

STEP 2 (3b), written before the runs: V5 (Blocks bracket) and V6 (MayBlock bracket) on the 3b build, 3 runs each:
V5 prints true 3/3 (handoffs >= 64, retakes +0); V6 prints true 3/3 (retakes > 0, injections +0).
Plant (3b's branch removed): V5 prints false 3/3 (no hand-off; retaken instead); V6 unchanged true.
SCORED step 2 on b3-inv: V5 true 3/3 (handoffs 64, retakes 0) MET. V6 MISSED: false 3/3, retakes 47-48 AND injections
63-65. Read: the monitor sleeps Thread.Sleep(2), which on this host's timer can last a 15.6 ms quantum, so a native step
outlives the 10 ms threshold before its second retake tick and is injected first. Fix (an addition to the design): the
injection walk skips a carrier the retake walk has seen inside a native call (NativeTicks > 0).
Re-run written before it: V6 true 3/3 with injections 0 or near 0 (<= 2), retakes > 0; V5 unchanged true 3/3.
SCORED V6 re-run (injection skip): still false 3/3 with injections 1 process-wide, retakes 63 -- the strict "injections
+0 inside the row" read main's own first, JIT-heavy step that starts 64 goroutines. Fixture change: a warm-up blockAll
over a channel before the counters are read. Then V5 true 3/3, V6 true 3/3 (retakes 64 each, injections inside the
window 0). The 3b plant (V5 must turn false) runs in the step-5 plant battery.

STEP 3 (3a), written before the runs, on the 3a build: V1 completes 3/3 with sensed >= K - P (48) and injections near 0;
V1 with GO2CS_MN_SENSE=0 HANGS 3/3 (the arm is the base for 3a). V2 still HANGS (native, 3c not built yet).
V5/V6 unchanged true 3/3.
SCORED step 3: V1 true 3/3, sensed 64, injections 0 MET; V1 with GO2CS_MN_SENSE=0 HANG 3/3 MET; V5, V6 true 3/3 MET
(sensed 0 in both). V2 not re-run here (3c still to build).

STEP 4 (3c), written before the runs, on the 3c build (3a and 3b on), 3 runs each:
- V2 completes 3/3 ("true"), peak carriers <= K + P + 2 (82); injections > 0. V2 with GO2CS_MN_REARM=0 HANGS 3/3.
- V3x (MN_SPIN_EXTRA=32, PURE) completes 3/3.
- V4 (200 ms steps) p99 <= 20 ms; V4b (2 s steps) p99 <= 25 ms.
- I9: V2 with GO2CS_MN_MAXTHREADS=16: exit 2 3/3, stderr carries "runtime: program exceeds 16-thread limit" then
  "fatal error: thread exhaustion". V7 (debug.SetMaxThreads(16)): prints "...old ceiling: 10000", then the same death.
- V1, V5, V6 unchanged true 3/3.
SCORED step 4: V2 true 3/3, peak carriers 65 MET; V2 GO2CS_MN_REARM=0 HANG 3/3 MET; V3x completes 3/3 MET;
I9 V2 + MAXTHREADS=16 exit 2 3/3 with Go's two lines MET; V7 prints 10000 then exit 2 with the two lines MET;
V1/V5/V6 true 3/3 MET. V4 MISSED: p99 79.1/93.0/101.1 ms (<= 20 predicted; base 87.7-94.3); V4b MISSED: p99 63.4-78.4
(<= 25 predicted). Probe written before it (one axis each, 2 runs): (a) V4 in default mode (OS threads, no scheduler) --
if its p99 is also 60-100 ms, the tail is the OS (16 CPU threads on 16 hardware threads, any 17th runnable thread waits
a Windows quantum), not injection; (b) V4 under GO2CS_MN with GO2CS_MN_PROCS=8 -- p99 <= 25 ms if the OS tail is the cause.
PROBE READ: (a) default mode p50 9.8/9.2, p99 44.2/35.7 ms: this host's floor with no scheduler is already over I4's
25 ms. (b) GO2CS_MN_PROCS=8: p50 12.6/11.9, p99 58.0/60.3, max 201.4/88.9 ms -- one wake waited a whole 200 ms step, a
scheduler miss beyond the OS floor. I4's stop line fired; posted to COORD before going on.

I8, written before the run: MnContended (twinned, GO2CS_MN=1, READING) 3 runs at 6447728b21: output equal to its
golden's expected lines, and sensed < 1% of parks in each run (stop line: over 5%).

FIXTURE v7 (counters read while the K goroutines are still blocked, just before set), written before the runs:
Windows tip: V1, V5, V6 true 3/3. Plants (rebuilt for the new fixture): P3a V1 false 3/3; P3b V5 false 3/3; Pskip V6
false 3/3. Linux tip (WSL, nproc 10): V1, V5, V6 true 2/2 each (the release-phase injections now fall outside the window).
SCORED v7 Windows: tip V1/V5/V6 true 3/3 MET; P3a V1 false 3/3, P3b V5 false 3/3, Pskip V6 false 3/3 MET (all restored
byte-identical). v7 linux: V1 true 3/3, V5 true 3/3 MET; V6 MISSED, false 2/3 (injections 4-6 in the window). Read: the
window's END snapshot is GoScheduler.Reading(), which samples every thread (/proc on linux) BEFORE it reads the counters,
so main's measuring step runs past the threshold and its own injection is counted. Fix (golib): Reading() reads the block
counters before the thread sample. Re-run written before it: linux V6 true 3/3; Windows V1/V5/V6 unchanged true.
SCORED v8 linux (Reading fix): V5 true 3/3; V1 false 1/3, V6 false 1/3 -- MISSED again. TRACE (temporary, uncommitted
build): the injected step is goroutine 1, main, 20-60 ms old while it starts the k goroutines and they queue (plus a few
warm-up blockers); the ~20 sensed are the warm-up's release (woken blockers contend on done's lock). The policy is right
to inject for main's long step. Fixture: V1 and V6 allow fewer than k/3 injections in the window (Windows 21, linux 13;
traced normal <= 9; Pskip read 33-49 on Windows, P3a reads sensed 0).
Re-runs written before them: linux V1/V5/V6 true 3/3; Windows tip true 3/3; Pskip V6 false 3/3, P3a V1 false 3/3.
SCORED v9 (43f158a3c9): Windows tip V1/V5/V6 true 3/3; P3a V1 false 3/3; Pskip V6 false 3/3; linux V1/V5/V6 true 3/3:
all MET.

201 ms ROOT, arms on V4 at GO2CS_MN_PROCS=8 (tip build), 5 runs each, max and p99 read (written before the runs):
A as built; B GO2CS_MN_RUNNEXT=0; C GO2CS_MN_REARM=0; D GO2CS_MN_INJECT_WAITERS=0. Prediction: A shows a max >= 150 ms
in at least 2 of 5 (it did in 3 of 5 before); an arm that removes the miss names the mechanism. I expect C to be WORSE
(once per step), and B or D to move it if the run-next slot or the waiter count is where the wake waits.
SCORED root arms (P=8, 5 runs): A max 186-224 in 4/5 (MET, it reproduces); B (RUNNEXT=0) max 183-191 in 5/5 -- run-next
is NOT the mechanism; C (REARM=0) and D (INJECT_WAITERS=0) pass the 20 s deadline 5/5 (wakes wait whole steps; the
re-arm and the waiter count are load-bearing).
TRACE (temporary Windows build): in run 3's 165 ms wait the monitor ticked ONCE in 130 ms (15.6 ms ticks would give ~8),
with over=8 injected Ps, 16 carriers in step on 16 hardware threads. The monitor thread is starved by the carriers it
supervises, so nothing injects for the waiting wake. ROOT: sysmon starvation under oversubscription.
Fix tried, written before the run: the monitor thread runs at ThreadPriority.AboveNormal. Prediction at P=8, 5 runs:
max <= 100 ms in 5/5 and p99 <= floor + 25 ms (floor read in the same session, default mode, 3 runs).
SCORED priority fix: MISSED. P=8 max 184.5/186.6/181.6/82.1/129.9; P=16 p99 79.7-110.1; floor (GO2CS_MN unset, same
session) p50 9.1, p99 26.0-31.3, max 35.1-131.7. Reverted (never committed). Next: a trace of tick gaps and of the
monitor's s_carriersLock waits, to tell starvation from a blocked monitor.
GAP TRACE (temporary Windows build, 4 runs at P=8): every long wake wait (106-183 ms) coincides with ONE monitor tick whose
WORK (not its sleep) took 136-200 ms. Suspect: the monitor starts carrier threads itself (injection's AddCarrier, the
retake's EnsureCarrier), and Thread.Start waits for the new thread to run, which under full CPU takes a quantum or more.
Fix, written before the run: the monitor registers the carrier at once and a starter thread starts it. Prediction at P=8,
5 runs: max <= 100 ms in 5/5 (the floor's own max reached 132 ms once, so one run over is not a refutation by itself);
p99 <= floor + 25 ms. P=16 V4 and V4b: p99 <= floor + 25 ms (floor p99 26-31 this session).
SCORED deferred-start fix: MISSED. P=8 max 129.8/165.4/182.7/176.0/50.7; P=16 p99 79-117; floor this session p99
17.1-40.4, max 27-105. The thread start is not the slow work. Reverted (never committed). Next: time each part of the
monitor tick (steal walk, retake walk and its release loop, injection walk, injection loop).
TRACE 5 (injection loop split): AddCarrier took 27-64 ms each, several per tick (Thread.Start under full CPU); s_p.Release
and s_carriersLock never reached 20 ms. With the start deferred the queued wake waits for its carrier instead. Churn:
an idle surplus carrier retires after 100 ms, so each burst creates threads again; Go keeps idle Ms parked (mput/mget).
Arm, written before it: idle carriers never retire (temporary build). Prediction at P=8, 5 runs: max <= 100 ms in at
least 4 of 5, p99 <= floor + 25 ms; P=16 V4/V4b p99 <= floor + 25 ms.
SCORED no-retire arm: MISSED (P=8 max 116-183; P=16 p99 95-137; peak carriers 17, so no re-creation happened). Not the
root. Before more Windows arms, the linux reading (COORD: host-only -> BOARD; reproduces hosted -> stop line). Written
before it: WSL (10 cores) V4 at GO2CS_MN_PROCS=4 and =8, 3 runs each, plus the default-mode floor: if the max stays
<= floor max + 25 ms the miss is this Windows host's.
SCORED linux V4 (WSL 10 cores, build 43f158a3c9): floor p99 5.9-8.7 (max 5.9-39.0); P=4 p99 10.9-15.0, max 11.0-16.4;
P=8 p99 11.1-11.7, max 11.6-15.5; P=4 2 s steps p99 10.3-15.0, max 12.5-23.0. MET (<= 25 ms, and <= floor + 25).
The 150-200 ms miss is this Windows host's: BOARD it with the readings (COORD's ruling). The hosted linux P=4 run stays
the gate of record.

I4 GATE OF RECORD (hosted linux, os-matrix run 45546636858 at ed2ca0c280, 4 cores, the runner's own P): MET.
floor p99 3.5/3.0/4.0 ms; twinned 200 ms steps p99 12.0/12.0/12.1 (max 12.1-14.0); twinned 2 s steps p99 7.0/7.0/6.0
(max 6.9-8.5). behavioral-smoke --mn MnInvisible PASS (Output equal to Go's). R-LAPTOP relative bar (evidence): not met
at P=16 (p99 79-117 against a floor of 17-44 + 25): the BOARDed host-only tail.
Made-to-fail for the step, written before it: the step's own script run in WSL against the linux build with
GO2CS_MN_REARM=0 exported prints "::error::N5 I4" for the twinned arms and exits 1.
SCORED step plant (WSL, GO2CS_MN_REARM=0, the step's own script): the step FIRED and exited 1 -- the 2 s-step twinned arm
read p99 1705-1707 ms, three "::error::N5 I4" lines. The 200 ms twinned arm did NOT fire (p99 11-15 ms: with 10 cores the
old rule still keeps up at 200 ms steps), so "the twinned arms" in the wording held for one of the two arms.

I6, written before it: process CPU (TotalProcessorTime) at 1f3210616b, 5 runs each, Windows:
MnMixedStorm n=1000 twinned and MnInvisible C-sleep (64 goroutines asleep 300 ms: near idle), each as tip, tip again (A/A),
and GO2CS_MN_SENSE=0. Prediction: the SENSE=0 arm's median within the A/A band (3a's reads are ~70 us/s).
SCORED I6: storm medians tip 1766 / tip2 1812 / SENSE=0 1875 ms (runs 1672-2047); C-sleep tip 2000 / tip2 1781 /
SENSE=0 1781. The storm SENSE=0 median sits outside the A/A medians [1766, 1812] by 63 ms, inside the A/A runs' range
[1672, 1922], and in the direction of MORE cost without sensing: as worded, MISSED on the storm (outside the band), MET on
C-sleep; read, no measurable cost of 3a in either.
```

### plants/PREDICTIONS.md

```text
Row 13 made-to-fail, at 79dbcb6555, written before the runs. Each plant is a source edit, built twinned (MnInvisible,
MnSpinCap), its rows run 3 times with the PowerShell deadline kit, then the file restored and checked byte-identical.
- P3b (GoNativeCall.Enter's Blocks branch removed): V5 prints false 3/3 (no hand-off; retaken instead). V6 still true.
- P3a (the monitor's `waiting` test forced false): V1 HANGS 3/3.
- P3c (s_rearm forced false: once per step under the P-sized cap): V2 HANGS 3/3, V3x HANGS 3/3.
- Pceil (AddCarrier's ceiling check removed): V2 with GO2CS_MN_MAXTHREADS=16 COMPLETES 3/3 ("true", exit 0).
- Pskip (the injection walk's NativeTicks/SensedTicks skip removed): V6 prints false 3/3 (MayBlock steps injected).
SCORED (row runs re-done against each plant's kept build; the battery's own row calls lost their path to a quoting slip):
P3b V5 false 3/3, V6 true MET; P3c V2 HANG 3/3, V3x HANG 3/3 MET; Pceil V2+MAXTHREADS=16 completes exit 0 3/3 MET;
Pskip V6 false 3/3 (injections 33) MET. All five restored byte-identical (RESTORED-BYTE-IDENTICAL, dirty=0 at END).
P3a MISSED: V1 still completes 3/3, with injections 49 and sensed 0 -- 3c's re-armed injection now reaches a managed
wait too, and V1 only checked completion. Fixture fix: V1 checks the counters (sensed rose by at least K - P, and no
injection inside its window, after a warm-up). Re-run written before it: V1 true 3/3 at the tip, false 3/3 under P3a.
SCORED re-run: V1 true 3/3 at the tip (sensed 49/55/49, injections 0), false 3/3 under P3a (sensed 0, injections 49):
MET. Then the threshold was loosened from "sensed >= K - P" (49 cleared 48 by one) to "sensed rose, no injection in the
window"; the P3a arm read sensed 0 with 49 injections, so the loosening cannot turn it green. Final fixture: V1, V2, V5,
V6 true 3/3 each.
```

### battery/PREDICTIONS.md

```text
Row 13 battery at 1f3210616b (golib class + behavioral fixture + CI step), written before it runs:
- repoguard rc 0.
- GolibTests Release: Passed 3197, Failed 0 (N10's 3183 at its tip on master cb6def82a2, plus row 4's 12
  CarrierStackFallbackTests rows, plus SchedulerCeilingTests' 2). Skipped read, not predicted.
- --mn subset ('Mn|Select|Channel|GoCall|Defer|GoroutineStackReserve|MainDeepRecursion'): 91 pass transpile/compile/
  target (N10's 88 + GoroutineStackReserve + MainDeepRecursion + MnInvisible); Output 88 compared, 0 failed, 3 NOT COMPARED.
- default-mode subset ('MnInvisible|MainDeepRecursion|GoroutineStackReserve'): 3 pass all four phases.
SCORED (04:28-05:22Z at 1f3210616b, dirty=0 at both ends), every item MET: repoguard rc 0; GolibTests 3197/0 (53 skipped,
3250 total); --mn subset 91 pass, Output 88 compared, 0 failed, 3 NOT COMPARED; default subset 3/3 all phases.
```

