# Main-return wait, the linux leg on R-WSL

- Brief of record: `../leg-briefs.md` at `bbf1725aa7`. Tip `bbf1725aa7` (batch 120, the merge carrying this seat). Base
  `c5a4637bba` (its first parent, R0).
- Box: R-WSL, Ubuntu 22.04 under WSL 2, 10 vCPUs, linux-x64. go1.26.10. .NET SDK 10.0.400, runtime 10.0.11. Native
  AOT with ILCompiler 10.0.11 and clang 14 (Ubuntu's, installed for this leg with the owner's approval).
- Every build passes `-p:GoTargetOS=linux` (the linux flavor of the converted standard library).
  - The first attempt did not. Its binaries ran the windows flavor and died in module init on `kernel32.dll`.
  - Four idle arms ran before that was caught (`winflavor-aborted.txt`). They are counted nowhere.
  - Every binary now passes a smoke gate before any arm: no flavor warning, Go's exit code, and the panic line.

## Harness (`harness/`)

- `mrw-build.sh`: two detached worktrees, the JIT Release builds with the tree's go2csPath, the Go binaries, the
  smoke gates, and GolibTests `MainExit*` and `ThreadCpuTime*` at the tip.
- `mrw-aot.sh`: one `dotnet publish -r linux-x64 -p:GoAot=true` per (tree, test), with the i9's seat #53 P1 kit
  `aot.targets`.
- `mrw-run.sh`: the brief's arms.
  - Every run is `GOMAXPROCS=4 timeout 60 taskset -c 0-3 <exe>`.
  - `load` is 32 busy loops (`spin.go`, GOMAXPROCS=32) pinned to CPUs 0-3, started 3 s before the arm and stopped at
    its end.
  - `alive` records the spinner at each run's end. N=100, one arm at a time.
- `mrw-ext.sh`: a stated extension.
  - R0 and R1 JIT loaded, 300 more runs each (mode `load2`, folded into `load` in the tally, so N=400).
  - A `hi` load with the spinners at nice -10. Stopped after 21 runs: all 21 timed out at 60 s (exit 124). Under
    CFS, nice -10 spinners starve the test outright, so `hi` measured starvation, not the race.
  - The `hi` rows are in `rows-hi-excluded.tsv` and counted nowhere.

## Files

- `tally.tsv`: per (arm, mode) counts of N, exit 0/2/other, empty stderr, exit 2 with the panic line, stdout
  "recovered", and spinner alive.
- `runs.tsv`: every counted run (2,200 rows).
- `build.txt`, `run.txt`: the drivers' logs.
- `golib-tests.txt`: the GolibTests result section.
- `threadcputime-rep10.txt`: ten repeats of `ThreadCpuTimeTests` on the windows-flavor build, recording the known
  test race.

## Readings

| Arm | idle | load (32 pinned loops) |
|:--|:--|:--|
| R0, base, JIT | 0/100 lost | 1/400 lost (run 220 of `load2`: exit 0, empty stderr) |
| R0, base, AOT | 0/100 | 0/100 |
| R1, tip, JIT | 0/100 | 0/400 |
| R1, tip, AOT | 0/100 | 0/100 |
| recovering, late "recovered", JIT / AOT / Go | 0/100 each | 0/100 each |
| Go amplifier control | exit 2 with the report, 100/100 | 100/100 |

The spinner was alive at the end of every counted loaded run.

GolibTests: 21 tests, 19 pass, all `MainExit*` included. Two fail, `ASpinningThreadsCpuTimeAdvancesWithItsWallTime`
and `ASleepingThreadsCpuTimeStaysFlat`, both at "the second read". That is the known `Measure()` test race (the worker
exits before the second `TryRead`; `clock_gettime` on a dead tid fails). On the earlier build it read 4 of 10 repeats.
