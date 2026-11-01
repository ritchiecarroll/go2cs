# Main-return wait, the re-read on the i9 at 5d017630ae

- Tip: claude/g-main-return-wait @ 5d017630ae. Its 4th commit adds `runtime.GOMAXPROCS(1)` to GoroutineRecoveringWhenMainReturns (`rec`). golib's GOMAXPROCS only records the setting, so only Go's side should move.
- Box: i9 (13th Gen Intel Core i9-13900K), win-x64. .NET SDK 10.0.403; JIT on Microsoft.NETCore.App 10.0.14; Native AOT with ILCompiler 10.0.14; go1.26.10.
- WHEA 0 from 2026-11-01 01:12 to 03:20 (local, CST). Daylight saving time ended during the night, so these local times come after i9-r1's 01:52 CDT.
- Harness: i9-r1's race3.sh, unchanged. The spinner runs 5400 s and is stopped at the arm's end; `spinner_alive` is recorded per run, and every loaded run here had it alive. Loads are as in R0 and R1, all on the same 4 CPUs.
- Driver: harness/r2-run.sh. It uses two fresh worktrees at the tip:
  - `a` holds the behavioral runner's JIT and Go builds of `rec`, and the blocked red;
  - `b` holds only the Native AOT publish of `rec`, so no plain build follows a publish in one tree.

## Legs

- A and B share one load session per load. Each round runs, in order, Go, JIT, AOT, each up to its own N:
  - idle: Go N=100;
  - n8: Go N=100;
  - n32: Go N=400, JIT N=100, AOT N=50;
  - loadhi: Go N=100, JIT N=100, AOT N=50.
- C, the blocked control: kit/make-red.sh `blocked` on worktree `a`, then a runner rebuild of GoroutinePanicAfterDeferredDone (`pad`), then JIT at n32, N=400.
  - Before the runs: the red golib.dll differs from the unpatched same-path build, and the red exe starts.
  - Before any run: the restore gives back the committed MainExit.cs blob (218301c588).

## Files

- tally.tsv: per-arm counts, with `spinner_alive` beside N.
- runs.tsv: every run (1,400 rows).
- r2.txt: the driver's log.
- affinity.tsv: 2 s samples. Every readable test-process sample reads 0xF Normal; spinners read 0xF at their load's priority. 0x0 rows had already exited when read.
