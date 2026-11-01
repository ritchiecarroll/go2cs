# Main-return wait, R0 on the i9 (the BEFORE arm)

- Base: claude/i9-panic-after-done-oracle @ 2e619b9966 (detached worktree). Tests: GoroutinePanicAfterDeferredDone (`pad`) and GoroutineRecoveringWhenMainReturns (`rec`).
- Box: i9 (13th Gen Intel Core i9-13900K). RID win-x64. .NET SDK 10.0.403. JIT runs on Microsoft.NETCore.App 10.0.14 (the app targets 10.0.0 and rolls forward). Native AOT uses ILCompiler 10.0.14. go1.26.10.
- JIT = the behavioral runner's Release build (it passes the runner). AOT = one `dotnet publish` per test with the seat #53 P1 kit's aot.targets (harness/r0-aot.cmd), reused across loads.
- Harness: the pado harness, unchanged (harness/race2.sh).
  - Each run goes through `start /affinity 0xF` with GOMAXPROCS=4 and a 60 s cap.
  - Loads are pinned to the same 4 CPUs: `load` n8 = 8 spinners, `load` n32 = 32 spinners, `loadhi` n8 = 8 spinners at above-normal priority (harness/spin-main.go).
- N: JIT 100 and AOT 50 per load. A loaded `pad` arm with no exit 0 was raised to N=400 in total (its `-ext` rows).
- Files:
  - tally.tsv: per-arm counts.
  - runs.tsv: every run, with its exit code, stderr bytes, whether stderr holds "panic: panic after deferred Done", whether stdout holds "recovered", and the first lines.
  - run.txt and publish.txt: the drivers' logs.
  - affinity.tsv: 2 s samples of the spinners and the test processes (time, name, pid, mask, priority).
- Box notes:
  - WHEA 0 from 16:08 to 18:22 (2026-10-31 local); the last WHEA event was 2026-10-15.
  - One test-process sample in 2,746 read mask 0xFFFFFF (pid 25260 at 17:41:51, during the AOT `pad` loadhi extension). Every other readable sample of the test processes and spinners read 0xF. The 0x0 rows are processes that had exited by the time they were read.
- Added control, outside the brief: rows `r0ctl-go-rec-*` run the runner's own Go binary of `rec` under loadhi n8 and load n32 (50 runs each). Go itself printed "recovered" in 1 of 50 under loadhi.

## Correction, 2026-10-31: the loaded arms' tails ran without load

The spinner runs a fixed 600 s (`spin.exe N 600`, harness/race2.sh), and six loaded arms outlasted it, so their last
runs had no load. runs.tsv, tally.tsv and the text above are unedited; this block corrects what they measured.

How it was counted (harness/r0-tail-check.py, output in r0-tail-check.tsv):
- The spinner's last live moment in an arm is its last affinity.tsv sample (2 s cadence) inside the arm's window from
  run.txt.
- A run is LOADED if its stdout file was last written at or before that sample plus 2 s.

Loaded denominators, where they differ from N:

| Arm | N | Loaded | Marked runs, all loaded |
|:--|--:|--:|:--|
| JIT `pad` load n32 (with `-ext`) | 400 | 211 | exit 0: 2 |
| JIT `pad` loadhi n8 | 100 | 45 | exit 0: 2 |
| AOT `pad` load n32 (with `-ext`) | 400 | 219 | exit 0: none |
| AOT `pad` loadhi n8 (with `-ext`) | 400 | 121 | exit 0: none |
| JIT `rec` load n32 | 100 | 99 | "recovered": 1 |
| JIT `rec` loadhi n8 | 100 | 42 | "recovered": 3 |

Every other R0 arm was loaded for all of its runs. The two `r0ctl-go-rec-*` controls ran outside the sampler's window, but
lasted 93 s and 205 s from first run to last, inside one spinner's 600 s. Every exit-0 run of `pad`
and every "recovered" run of `rec` happened while the spinner was alive, so each reproduction stands. What changes is
the loaded N behind the zeros: AOT `pad` is 0/219 at n32 and 0/121 at loadhi (not 0/400), and JIT `pad` at loadhi is
2/45 (not 2/100). From R1 on, the spinner outlasts each arm and every run's row records whether it was alive.
