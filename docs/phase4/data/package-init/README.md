# Data for SCOUT-package-init.md

Measured 2026-11-14 by P5 on one cloud linux/amd64 VM (4 vCPU, 15 GB, .NET SDK 10.0.400), Release, at master
`a6cefba6ee`. The program is the design's `hello` (`docs/phase4/data/startup-method-sets/programs/hello.go` on
`claude/p5-startup-method-sets`). Nothing here ships.

| file | what it is |
|:--|:--|
| `hook/` | the start-up hook (`DOTNET_STARTUP_HOOKS`): runs each assembly's module initializer in dependency order and records wall, JIT time, methods, IL bytes and allocation for each; `F1_MODE=jit` adds per-method JIT durations. The measured copy of the program has `System.StartupHookProvider.IsSupported` set to true in its runtime configuration |
| `init-build.tsv`, `init-publish.tsv` | one run of the hook for each shape |
| `packages-build.txt`, `packages-publish.txt` | medians over 12 and 10 runs, every package (`agg.py`) |
| `jit-build.tsv`, `jit-publish.tsv` | per-method JIT rows of one run for each shape |
| `kinds-build.txt`, `kinds-publish.txt` | those rows classed by kind on the start-up thread (`kinds.py`) |
| `trace-publish-startup-thread.txt` | `dotnet-trace` sampled stacks of the published program's start-up thread |
| `multi.py` | interleaved wall and CPU time of several arms: `multi.py ROUNDS -- label=ENV=V,ENV=V:command …` |
