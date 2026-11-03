# Main-return wait: the linux and macOS leg briefs

COORD dispatches these two legs from this file, after the seat lands. The record is
[`DESIGN-main-return-wait.md`](../../DESIGN-main-return-wait.md). The windows legs ran on the i9, and their results
are in its section 7.

## The briefs, as posted with the predictions (2026-10-31)

> R-WSL (yours to dispatch, after my DONE): at my tip, linux-x64 JIT+AOT: GolibTests (MainExit\*, ThreadCpuTime\*); the
> amplifier and GoroutineRecoveringWhenMainReturns, N=100 idle and under taskset -c 0-3 with 32 busy loops; R0 at base.
>
> C1 macOS runner: as R-WSL, on darwin JIT+AOT, N=50 per arm, plus the live ThreadCpuTime test; R0 at base first.

## What the briefs mean now

- **Where to run.** "My tip" is now **the batch's merged master tip**, the merge that carries this seat. R0's "base"
  is that merge's first parent, master just before this seat.
- **The amplifier** is `GoroutinePanicAfterDeferredDone`, built once and run directly N times under the load. A run
  loses when it exits 0 with an empty stderr. The test sets `runtime.GOMAXPROCS(1)`, so Go's side exits 2 every time.
- **The loads.** linux: `taskset -c 0-3` for the program and for 32 busy loops. macOS has no affinity call, so C1 runs
  its 32 busy loops unpinned and states that.
- **The amendment (2026-11-01).** Under load, `GoroutineRecoveringWhenMainReturns` is a **WATCH reading**, not a pass
  criterion. Report late "recovered" over N per arm. On windows it read 0-11 per 100 at n32 and 2-45 at hi, depending
  on runtime and session. Idle stays a pass criterion: 0 late prints.

## What each leg reports

| Arm | Pass |
|:--|:--|
| GolibTests `MainExit*` and `ThreadCpuTime*` (C1 also runs the live `ThreadCpuTime` test) | all pass |
| R0, the amplifier at base, idle and loaded | a reading: losses over N (windows lost only under load, and only JIT) |
| R1, the amplifier at the tip, idle and loaded, JIT and AOT | 0 losses; every run exits 2 with the panic report |
| `GoroutineRecoveringWhenMainReturns` at the tip, idle | 0 late "recovered" |
| `GoroutineRecoveringWhenMainReturns` at the tip, loaded | WATCH: late over N |

For each arm, report the runtime and RID, N, the load, and whether the spinners were alive for every run. One post per
leg, at most 40 lines. Push the text logs to a branch of your own under `docs/phase4/data/main-return-wait/`.
