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
