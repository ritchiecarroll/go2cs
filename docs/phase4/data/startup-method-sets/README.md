# Data for DESIGN-startup-method-sets.md

Measured 2026-11-13 by P5 on one cloud linux/amd64 VM (4 vCPU, 15 GB, .NET SDK 10.0.400, go1.26.10), Release
builds under the JIT, `GoTargetOS=linux`, at master `651c4ad147` unless a file says otherwise. Nothing here ships.

| file | what it is |
|:--|:--|
| `programs/hello.go`, `stringer.go`, `wide.go` | the three Go programs measured |
| `t.py` | wall, CPU and peak RSS of a command: `t.py RUNS WARMUPS -- cmd…`, medians |
| `b3-probe.patch` | the measurement probe, a patch to golib. Counts and times the method-set path when `GO2CS_B3_PROBE` is set (`2` adds the keys). **Never merged.** |
| `probe-hello.txt`, `probe-stringer.txt`, `probe-wide.txt` | the probe's output for one run of each program |
| `trace-hello-startup-thread.txt` | `dotnet-trace` sampled stacks of hello's start-up thread, inclusive time, read with P4's `stacks.py` |
| `trace-wide-main-goroutine.txt` | the same for the main goroutine of `wide` |
| `alt-harness/` | a console program that prices three sources of "the extension methods whose receiver is type E" over a built program's assemblies: `scan` (reflection over every host of the asked homes, as golib reads today), `holder` (only the hosts whose receiver is an asked type, plus unmarked hosts), `meta` (a per-method index from metadata, methods resolved by token), `names` (a name list resolved with `GetMember`) |
