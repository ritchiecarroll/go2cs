# DESIGN -- a returning main waits on whether a panicking goroutine can run, not on the clock

> Drafted 2026-10-31 by lane G from branch `claude/i9-panic-after-done-oracle` at `2e619b9966`, the i9's oracle fix on
> master `2334e8949c`. COORD ruled the seat in-hop: golib-only, no regen rung, landing before the re-bank dispatch.

## 1. The defect

`GoroutinePanicAfterDeferredDone` (with the i9's `runtime.GOMAXPROCS(1)` oracle fix) exits 2 with Go's report in 80
of 80 Go runs across idle and three loads on the i9. The converted program exited 0 with an empty stderr in 1 of 20
runs under 32 pinned spinners and 2 of 20 at above-normal priority. A crash read as a success.

The cause is golib's `MainExit.AwaitPanickingGoroutines`. When `main` returns while a goroutine is running a panic's
deferred calls, golib waited at most 20 ms by the wall clock. Under load the goroutine's thread waited for a CPU for
longer than that, and `main` exited first.

## 2. Go's rule

`runtime.main` (runtime/proc.go:310 at go1.26.10) calls `Gosched` up to 1000 times while `runningPanicDefers` is not 0.
Under one P each call hands the panicking goroutine the CPU, so a goroutine that can run always gets its turns, and a
goroutine that is blocked (parked, or in a system call) gets none, and the loop runs out in microseconds. The bound
asks one question, "can it run?", and is never a clock.

Golib cannot hand over the CPU: a goroutine is an OS thread scheduled beside `main`'s, and golib does not model
`GOMAXPROCS(1)`. It can ask the question.

## 3. Candidates

| | Signal | Ruling |
|:--|:--|:--|
| A | The panicker's CPU time alone | Rejected: a blocked panicker uses none, so only a wall cap ends the wait, which breaks the no-late-output parity of `GoroutineRecoveringWhenMainReturns`. |
| B | Progress marks on the panic path | Rejected: the CLR's first pass (1.2 to 1.8 ms) cannot be marked, and marks cannot tell starved from blocked. |
| C | `main`'s own CPU time or turns | Rejected: on a loaded box a turn costs a quantum rotation, and a few turns fit in `main`'s own quantum. |
| D | The OS's thread state, plus the panicker's CPU time | Superseded by E: Windows would need `NtQuerySystemInformation`, and the OS calls a thread blocked in a CLR lock "waiting" where Go calls it running. |
| E | golib's own scheduler marks, plus the panicker's CPU time | **Chosen** (COORD, 2026-10-31): it reads the scheduler's fact, as Go's loop does. |

## 4. The design

**The marks** (`Goroutine`, the main-return marks). A counted goroutine is blocked when it is parked and not readied
(`Goroutine.Park`, the existing gopark accounting) or blocked outside the park accounting (`Goroutine.EnterBlocked`):

- a system call: the runtime's `reentersyscall`/`exitsyscall` (linux, darwin), and the windows trampoline
  `dll_windows.cs` `callNative` (every `SyscallN` arity and the deep-copy path), which before this seat never marked
  a call at all;
- the execution tracer's reader and stop waits, which cannot `Park` because `Park`'s block event takes the lock they
  hold.

The marks are written only while the goroutine is counted in `runningPanicDefers`, so an uncounted system call pays one
thread-static read and a branch. A per-goroutine block sequence names each counted block.

**The decision** (`MainExit.Holds`, a pure function over samples). `main` waits while a counted goroutine:

- is not blocked, or has not stayed in the same block for `BlockedConfirm` (1 ms): Go's loop runs out at once for a
  blocked goroutine, and the 1 ms stands for sysmon's retake of a short system call;
- and has used less than `PanicDefersCpuBudget` (20 ms) of CPU since the wait began: the role of Go's 1000 turns, which
  bounds a goroutine running user code in a deferred call.

**The CPU seam** (`ThreadCpuTime`), documented per-thread APIs, read only during the wait: windows `OpenThread` and
`GetThreadTimes` (tick-coarse; the handle is held for the wait and closed on every path), linux `clock_gettime` on the
thread's CPU clock id, darwin `thread_info`. Where the CPU cannot be read (no seam, or the thread exited), the bound is
the wall-clock 20 ms it was before.

**What stays wall-clock.** A hang guard of 10 s for a goroutine that can run but is given no CPU: a visible divergence,
disclosed in `MainExit`'s remarks, reachable only by threads pinned above it for longer than the OS's starvation boost,
or by a block golib does not mark. `ReportWait` (60 s) is unchanged.

**Not marked, deliberately.** `Coro`'s start handshake: Go's `newcoro` never parks its creator, and the wait is on a
runnable peer (COORD, 2026-10-31).

## 5. The census

`docs/phase4/data/main-return-wait/census.sh` reads every hand-owned `src/core` file (the only route from converted Go
to .NET), finds each unbounded wait primitive, and requires its method to carry a mark or its site to have a row in
`classified.tsv`. It also checks both syscall funnels. At this seat's tip it reads 400 files and 77 wait sites: 29
marked in their method, 48 classified (12 marked deeper, 8 service threads, 2 fatal paths, 19 runtime test helpers, 3
runnable in Go too, 2 idle system goroutines, 1 runtime-internal, 1 transient handshake), 0 unclassified, funnels
marked. At base it names the 3 tracer waits and all 3 funnel marks, and exits 1. Re-run it after any golib or
hand-owned change.

## 6. Predictions and results

The predictions (P1 to P8) and the red-first arms (R0 to R6) were posted before code, and are scored in section 7 as
the batteries report.

## 7. Results

Windows, on the i9 (i9-13900K, .NET 10.0.14, go1.26.10). Every run went through `start /affinity 0xF`, with the
loads pinned to the same 4 CPUs:
- n8: 8 spinners;
- n32: 32 spinners;
- hi: 8 spinners at above-normal priority.

Raw logs are on the i9's branches `claude/i9-mrw-r0` to `-r4`. Counts are of loaded runs only. In R0 the first
spinner ran out before some arms did, and the i9 corrected the denominators. The linux and macOS legs report after this
record.

**GoroutinePanicAfterDeferredDone: runs that LOST the panic report (exit 0, empty stderr).**

| | idle | n8 | n32 | hi |
|:--|:--|:--|:--|:--|
| R0, base, JIT | 0/100 | 1/100 | 2/211 | 2/45 |
| R0, base, AOT | 0/50 | 0/400 | 0/219 | 0/121 |
| R1, tip, JIT | 0/100 | 0/100 | 0/400 | 0/100 |
| R1, tip, AOT | 0/100 | 0/100 | 0/100 | 0/100 |

AOT never lost. Its panic path is precompiled and reaches the report in about a millisecond, so even the old 20 ms
wall bound held. The JIT panicker compiles the path of its own first throw, and that is the arm this change fixed.

**GoroutineRecoveringWhenMainReturns: runs that printed "recovered" after main returned** (every run exits 0).

| shape | arm | n32 | hi |
|:--|:--|:--|:--|
| old (sleep 500 ms from wg.Done) | Go | 5/100 | 2/100 |
| old | JIT | 3/100 | 7/100 |
| old | AOT | 3/50 | 27/50 |
| new (AfterFunc at main's return) | Go | 3/100 | 1/100 |
| new | JIT | 0/100 | 5/100 |
| new | AOT | 2/50 | 13/50 |
| new + GOMAXPROCS(1) | Go | 0/400 | 0/100 |
| new + GOMAXPROCS(1) | JIT | 0/100 | 3/100 |
| new + GOMAXPROCS(1) | AOT | 5/50 | 16/50 |

Idle and n8 read 0 in every arm. GOMAXPROCS(1) makes Go's side deterministic: 0 of 700 loaded runs. golib's
GOMAXPROCS only records the setting, so the converted side does not move. The trace arm, below, says why it prints
late.

**The controls (R3).**
- "Always runnable, no budget" printed "recovered" in 20/20 runs idle and 20/20 at n32 (held to the guard). The
  regression shows.
- "Always blocked" lost the report in 4/100 runs at hi, against the tip's 0/100, so it regressed there. At n32 it lost
  0/100, then 1/400 in the re-read. Above-normal priority is where this control has power on that box.

**P6, the windows trampoline's mark.** A SyscallN(GetCurrentProcessId) loop, 15 interleaved rounds, against the A/A
band:
- JIT: base 8.66 ns, tip 8.49 ns, ratio 0.992, inside the band;
- AOT: base 9.41 ns, tip 9.66 ns, ratio 1.034, inside the band.

**The exit hold: tried, measured, reverted** (2026-11-01). Go's `exit` stops every thread at once, while the CLR keeps
goroutine threads running through `ProcessExit` and its own shutdown. The hold (`438c43dbab`) held a counted
goroutine at its first system call after the exit was decided, for the hang guard, so that a recovering deferred
call's late print could not reach the OS. Its landing gate was the recovering test's late prints under Native AOT:
5/100 or fewer at hi. On the i9 (`claude/i9-mrw-r3`, `560e4aa82f`), the hold and its base ran interleaved, 100 runs
per arm:

| AOT, late "recovered" | n32 | hi |
|:--|:--|:--|
| base `5d017630ae` | 11/100 | 28/100 |
| with the hold | 10/100 | 25/100 |
| Go | 0/100 | 0/100 |

The hold did not move the rate, so the late print does not leave through the post-exit system-call path. The hold was
reverted on top (`12d5e3eb91`). It did not touch reporting: `GoroutinePanicAfterDeferredDone` exited 2 with the report
in 100/100 runs at n32 and at hi.

**The trace arm: which release lets the print through** (`claude/i9-mrw-r4`, `51067a72d4`). The build was
`kit/make-trace.sh` on a checkout at `8914486591`, one Native AOT publish. Each load ran 100 traced and 100 untraced
runs of that one binary, alternated.

| AOT, late "recovered" | traced | untraced |
|:--|:--|:--|
| n32 | 11/100 | 11/100 |
| hi | 45/100 | 40/100 |

The two halves agree, so tracing does not move the race. The hi session ran hotter than r3's base of 28/100: hi has
read between 25 and 45 per 100 across sessions on that box. The traces show:

- **Main is starved for whole seconds at every step of its way out, late or not.** The goroutine parks a median 1.1 to
  1.3 s before main's exit handler starts (0.3 ms in the i9's idle smoke run). In the three on-time runs shown, the
  wait's 1 ms confirmation stretched to between 120 ms and 1.4 s.
- **In 14 of the 45 late hi runs, the wait released as designed before the print, and main lost the CPU afterwards.**
  Each of these releases names `blocked=True seq=1`: the original park. A counted system call moves the sequence
  (`Goroutine.EnterCountedBlock`), so these samples were taken before the print. After the release, main's few
  microseconds of work took seconds. In one run, from the handler's start: release at +180 ms, print at +1.26 s, exit
  decision at +3.0 s. The i9 tables split these 14 runs by where the print falls against the release's write: 7
  before the wait's derived start, 4 inside it and 3 after it. That start is derived from the write, so it is late by
  however long main waited to write.
- **In the other 38 late runs (27 at hi, all 11 at n32), the wait was still running when the goroutine printed and
  ended.** By then the timer had readied it. Holding main for a runnable goroutine is Go's own rule.
- At hi, 2 runs printed before the exit handler began, and 2 printed after it began but before the wait looked.

**No release rule tightens this.** The release is not slow; main's thread is. Go avoids the race because, under one
P, the timer cannot run while main holds the P. golib's goroutines and timers are OS threads, and its GOMAXPROCS is
recorded, not modeled. An early release for a parked goroutine would not help either: the 14 releases above were
already early, and main still waited seconds for the CPU after each. Of the late runs, such a rule could reach only
the 38 wait-exit runs, and only those whose first sample saw the park; the trace does not log that sample. It would
also drop a goroutine that a runnable peer is about to wake, which Go's 1000 turns still honor. The late print is
therefore a watched divergence of the load, not a defect of the wait. The wait answers "can it run?" correctly in
every traced run.

**The predictions, scored** (posted before code; the windows legs):
- P1, R0 at base: exit 0 in 2 to 10 per 100 runs under n32 and hi, 0 idle. Partly hit. JIT read about 1 per 100 at
  n32 and 4 at hi. AOT read 0 everywhere, a miss: its precompiled panic path beats even the old bound.
- P2, R1 at the tip: 0 losses in every arm. Hit on windows, JIT and AOT. linux and macOS are pending.
- P3, R2: the recovering test never prints "recovered". Hit idle. Missed under load: Go needed GOMAXPROCS(1) to read
  0, and the converted side prints late (the trace arm).
- P4, the R3 controls. "Always runnable" hit, 20/20. "Always blocked" hit at hi (4/100 against R0's 2/45), and had no
  power at n32.
- P5, GolibTests: hit, except the green twin's first call in a JIT process, at 21 ms against < 20 ms. Later calls take
  1.2 ms.
- P6: the tip's delta is inside the A/A band, a hit. The base level of 25 to 60 ns missed: it is about 9 ns.
- P7, the census: hit, 0 unclassified.
- P8, the gates: hit.
- The trace arm (B at least 70% of late runs, A the rest, C about 0): the counts hit, at 89% at hi, 100% at n32 and C
  0. The mechanism missed. The prediction said the wait would be starved partway through its confirmation and then
  hold for the woken goroutine. In 14 of the 45 hi runs the wait had already released correctly, and main was starved
  after the release. 3 runs printed between the release and the exit decision, a window the prediction had not named.

**G-LAPTOP gates, on the seat branch before its merge of master.** At `0e521b4758`, golib, the generator, the
converter and GolibTests are identical to `ba6550b337` and `971f018d7e`. The batch's merge battery covers the merged
tree.
- GolibTests Debug and Release: rc 0 at `ba6550b337`. GenTests: rc 0.
- `go test ./...`: green at the tip.
- Both solutions build at `971f018d7e`.
- CNR: 0 tracked changes at `971f018d7e`.
- BehavioralRunner (full): rc 0 at `971f018d7e`, and again at `438c43dbab` with the new test shape. The two changed
  tests pass at the tip.
- The first use of the wait in a JIT process costs about 21 ms, the JIT of its slow path. Later uses take 1.2 ms.

**The linux and macOS legs (2026-11-01).** Both ran the briefs in `data/main-return-wait/leg-briefs.md`, at tip
`bbf1725aa7` and base `c5a4637bba`.

Linux, on R-WSL:
- Box: linux-x64 under WSL 2, 10 vCPUs, .NET 10.0.11, go1.26.10; Native AOT linked by clang 14.
- Every build targets the linux flavor of the corpus (`-p:GoTargetOS=linux`).
- Every run goes through `taskset -c 0-3` with GOMAXPROCS=4. The load is 32 busy loops pinned to the same CPUs, and
  the spinner was alive at the end of every loaded run.
- Logs: `data/main-return-wait/linux-r/`.

| linux: runs that lost the panic report | idle | loaded |
|:--|:--|:--|
| R0, base, JIT | 0/100 | 1/400 |
| R0, base, AOT | 0/100 | 0/100 |
| R1, tip, JIT | 0/100 | 0/400 |
| R1, tip, AOT | 0/100 | 0/100 |

- The recovering test printed late in 0/100 runs in every arm (JIT, AOT and Go, idle and loaded).
- GolibTests `MainExit*` all passed. The two `ThreadCpuTime` failures were a race in the test itself, fixed in
  `d3a05a0162`.
- R0's one loss came from a stated extension to N=400. The first 100 loaded runs read 0, so at this load the base
  defect shows, but at about a quarter of the i9's rate.

macOS, on C1's GitHub runners:
- Run 41351205394, recorded on `claude/c1-mrw-macos` at `9e703e720b` under `data/main-return-wait/c1-macos/`.
- osx-arm64 (Apple M1, 3 CPUs) and osx-x64 (Intel i7-8700B, 4 CPUs), .NET 10.0.14, N=50 per arm. The load is 32
  unpinned busy loops, because macOS has no affinity call.

| macOS JIT: runs that lost the panic report | arm64 idle | arm64 loaded | x64 idle | x64 loaded |
|:--|:--|:--|:--|:--|
| R0, base | 0/50 | 0/50 | 0/50 | 0/50 |
| R1, tip | 0/50 | 0/50 | 0/50 | 0/50 |

- The recovering test printed late in 0/50 runs in every macOS arm.
- **The darwin Native AOT arms are OPEN.** They wait on G's frozen-object holder seat (ledger `0c2c770db6`). The
  macOS leg found that ILC's frozen-object symbol names collide for darwin `syscall`'s `IfmaMsghdr` and `IfmaMsghdr2`,
  so every darwin Native AOT publish that reaches `syscall` fails.

P2 on linux: hit, JIT and AOT, idle and loaded. On macOS: hit for JIT; AOT pending.

**The darwin Native AOT arms (2026-11-02): CLOSED.** G's frozen-object holder seat landed at batch 129
(`56039a371d`). Each struct's field accessor tokens now live in a nested holder class, so ILC's frozen-object names
can no longer collide. C1 then ran the open arms on the same two macOS runners in run 41420753631, recorded under
`data/main-return-wait/c1-macos-aot/` (fix commit `7f4279bd15`, .NET 10.0.14, N=50 per arm, 32 unpinned busy loops).

| macOS Native AOT | arm64 | x64 |
|:--|:--|:--|
| control at `64d145b2c4`: ILC fails on the `IfmaMsghdr23` key | fails | fails |
| fix: the amplifier and the recovering test both publish | rc 0 | rc 0 |
| R1, tip, idle: runs that lost the panic report | 0/50 | 0/50 |
| R1, tip, loaded: runs that lost the panic report | 0/50 | 0/50 |
| recovering test printed late, idle and loaded | 0/50 | 0/50 |

- Every amplifier run exited 2 with the report, and every recovering run exited 0. No run was invalid, and all 32
  spinners were alive at the end of every loaded run. The counts are recomputed from the per-run TSVs.

P2 on macOS: hit for JIT and AOT, idle and loaded. The macOS leg is complete.
