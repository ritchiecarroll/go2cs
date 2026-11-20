# tsbench kit: the published checkdemo row (TypeScript table, typescript-go-cs, 2026-11-20)

Scratch, for R's TypeScript profiles (P3's perf scout). Not for master. Everything here is what produced the published
`check-101` row on the perf box (G-LAPTOP, Windows x64, .NET 10 SDK 10.0.400, runtime 10.0.11).

## Contents

- `tsbench/` -- the harness (Program.cs, tsbench.csproj). Build: `dotnet build tsbench/tsbench.csproj -c Release`.
- `arms-check101.tsv` -- the three arms of the row, as run: program, arm, exe, working directory, env, args.
- `units-101.args` -- the 101 inputs, one path per line (tsbench's `@file` form); `units-101.ids` -- their ids.
- `corpus-units-101/` -- those 101 files; `manifest-101.tsv` -- their manifest rows (id, ext, flags, SHA-256 of the
  original, source path in the TypeScript fixtures).
- `SHA256SUMS` -- every file in this kit.

The args file names `C:/go2cs/ts/corpus/units/<id>.txt`. Put the files there, or rewrite the prefix:
`sed 's|C:/go2cs/ts/corpus/units/|<your dir>/|' units-101.args > units-101.local.args`, then point the arms at it.

## The programs

- C#: `checkdemo.exe` from typescript-go-cs at `6ba3f6b` ("The converted code ... at go2cs 1.26.11.1"), built Release
  (JIT), the artifact directory `.artifacts/bin/538abe619327/Release/net10.0/`.
- Go: `checkdemo-go.exe`, the same demo built natively from the TypeScript Go sources the C# was converted from.

## The GC settings (the published `jit` arm)

From checkdemo's own `checkdemo.runtimeconfig.json`: `"System.GC.Server": true` and
`"System.GC.DynamicAdaptationMode": 0` (server GC, DATAS off). No environment override. The `jit-wks` arm is the same exe with
`DOTNET_gcServer=0` (workstation GC), as published beside it.

## The exact command line

Environment: `DOTNET_ROOT` = the .NET 10 SDK folder, on PATH first; `DOTNET_ROLL_FORWARD`, `GOFLAGS` and
`DOTNET_gcServer` unset.

```
tsbench.exe <path>\arms-ts.tsv 5 <path>\ts.tsv
```

5 rounds. Per program, tsbench verifies that every arm's stdout is equal (SHA-256) and exits 0, runs one discarded
warmup per arm, then the rounds, with the arm order alternating forward and reverse. It records wall time, CPU time (user + kernel) and peak
working set. The published table used the median wall of the 5 timed runs (one process per run).
