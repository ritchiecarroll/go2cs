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

**The exit hold** (COORD, 2026-11-01, ruled into this seat). Go's `exit` stops every thread at once; the CLR keeps
background threads running through `ProcessExit` and its own shutdown, and in that window a goroutine can still reach
the OS. Once the exit is decided (`OnProcessExit` is done, or `MainExit.Exit` for `os.Exit` and the fatal paths),
`Goroutine.EnterBlocked` holds a counted goroutine at its next system call for the hang guard. That is in the slow path
only counted goroutines reach, so an uncounted system call pays nothing. Its conditions:

- The thread that decided the exit is exempt: it runs the handlers, an `os.Exit` from a goroutine included.
- No lock a held goroutine can own is taken after the decision. The five `ProcessExit` subscriptions in `src/core`
  (MainExit, the testing host's flush, Q44's dump, IGoEquatable's census write, runtime's stop flag) write through
  .NET's `File` and `Console`, never through a Go fd or mutex.
- Anything outside that census is bounded by the guard. A held goroutine that owns a lock the exiting thread needs
  delays the exit by at most the guard (the GolibTests plant reads 308 ms against a 300 ms guard), never deadlocks it.
- Residual windows: golib's direct console writes (the `print` builtin) and a C# caller's own I/O pass no mark.

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

To be recorded as the i9, R-WSL and C1 legs report.
