# Main-return wait, R1 to R3 and P6 on the i9

- Tip: claude/g-main-return-wait @ e592e782ca (three commits on 2e619b9966). OLD shape of the recovering test: 971f018d7e, whose `src/core` and `src/go2cs` equal the tip's.
- Tests: GoroutinePanicAfterDeferredDone (`pad`) and GoroutineRecoveringWhenMainReturns (`rec`).
- Box: i9 (13th Gen Intel Core i9-13900K), win-x64. .NET SDK 10.0.403; JIT on Microsoft.NETCore.App 10.0.14; Native AOT with ILCompiler 10.0.14; go1.26.10.
- WHEA 0 from 2026-10-31 20:04 to 2026-11-01 01:52 (local).

## Harness

harness/race3.sh is R0's race2.sh with two changes:
- The spinner runs 5400 s, longer than any arm, and is still stopped at the arm's end. R0's 600 s spinner ran out before some arms did; see the i9-r0 README's correction.
- Every row gets a 7th column, `spinner_alive`: 1 when the spinner was alive at the run's end (always 1 idle).

Everything else is as in R0:
- Each run goes through `start /affinity 0xF` with GOMAXPROCS=4 and a 60 s cap.
- Loads, all pinned to the same 4 CPUs: `load` n8 = 8 spinners, `load` n32 = 32 spinners, `loadhi` n8 = 8 spinners at above-normal priority.

R2 runs its six programs in one load session per load. Each round runs, in order: old JIT, old Go, new JIT, new Go, then old AOT and new AOT in the first 50 rounds only.

## Builds (build.txt)

- JIT and Go: the behavioral runner's Release build, on detached worktrees of the tip and of 971f018d7e. The runner passes each. The exe folders were copied out before any red patch.
- AOT: one `dotnet publish` per (tree, test) with the seat #53 P1 kit's aot.targets (harness/r1-aot.cmd).
- The old and tip golib.dll differ in 146 bytes, all from the build path and the PE stamp: same length, same source.

## Files

- tally.tsv: per-arm counts, with `spinner_alive` beside N.
- runs.tsv: every run of R1-R3 (3,340 rows).
- run.txt, r3.txt, build.txt: the drivers' logs.
- affinity*.tsv: 2 s samples (time, name, pid, mask, priority, exe folder).
  - Test processes: every readable sample reads 0xF Normal; 0x0 rows had already exited when read.
  - Spinners: every sample reads 0xF at the priority their load sets.
- p6/: the microbench (below).

## R3's first attempt (r3-abort.txt, affinity-r3-abort.tsv)

The first R3 rebuilt on the tip worktree after that tree's win-x64 Native AOT publish. That plain build produced a C# exe that cannot start ("You must install or update .NET", exit 150).
- Its 26 rows are relabeled `r3abort-` and counted nowhere.
- R3 then ran on a fresh tip worktree with no publish (harness/r3-run.sh). It first builds the unpatched tree (the runner PASSes), and that build's golib.dll is the same-path reference.
- Each red golib.dll differs from that reference, each red exe starts (one idle run exits 0 or 2), and each restore gives back the committed MainExit.cs blob.

## P6 (p6/)

The tip's kit/syscallbench, a loop of SyscallN(GetCurrentProcessId) that prints `syscall_ns`.
- Built with the seat #53 P1 kit's convert-and-build.sh (claude/g-seat53-r2r-guard @ 8d45d108ab) and the tip's converter, built from a never-built tip worktree.
- Arms:
  - base: a 2e619b9966 worktree's src.
  - aa: the same root built again.
  - tip: the never-built tip worktree's src.
- hashes.tsv: base and aa share every library assembly; the tip's syscall, golib and runtime assemblies differ.
- drive.py, 15 rounds per runtime, under affinity 0xF. The JIT arms set DOTNET_TC_CallCountingDelayMs=0.
- drive-jit.txt and drive-aot.txt are its readouts; p6-*.tsv hold the per-round values.
