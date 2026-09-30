# c2-osr-stackwalk: a .NET 10 stack-walk fault under on-stack replacement

A reproduction kept for an upstream report. It sits on `claude/c2-osr-repro`, whose base is A8's first cut
(`c42929f370`). In that cut, golib's six `panicCheck1` factories in `RuntimeErrorPanic` walked the live stack
(`new StackTrace(1, false)`) each time they raised a bounds panic. The A8 re-cut removed that walk, so this
reproduction needs that base.

## What it does

`Program.cs` builds a golib `PanicException` 200,000 times per arm, at a recursion depth of 5 and then 50.
Each arm first builds it without throwing, then throws and catches it:

- `Baseline`: a plain `PanicException`, with no walk.
- `Factory`: `RuntimeErrorPanic.IndexOutOfRange`, which walks the stack in this base. With `A8_NOWALK=1` it
  calls `Baseline` instead.

`Time`'s loop is hot and contains a `try`/`catch`, so under tiered compilation it is compiled through
on-stack replacement (OSR).

## Run

```
dotnet build -c Release
DOTNET_TieredCompilation=1 ./bin/Release/net10.0/osr-stackwalk
```

## Measured (linux x64, .NET 10.0.1 runtime `10.0.1226.42308`, SDK 10.0.112, 4 cores; 2026-09-29)

| Configuration | Runs that died with `Fatal error. Internal CLR error. (0x80131506)` |
|---|---|
| `DOTNET_TieredCompilation=1` | 19 of 24 |
| `DOTNET_TieredCompilation=1`, `A8_NOWALK=1` | 2 of 24 |
| `DOTNET_TieredCompilation=1`, `DOTNET_TC_OnStackReplacement=0` | 0 of 24 |
| `DOTNET_TieredCompilation=0` | 0 of 24 |

A crash report (`DOTNET_DbgEnableMiniDump=1 DOTNET_EnableCrashReport=1`) records a
`System.ExecutionEngineException` and no managed frames. The fault needs OSR. The extra `StackTrace` walk
per exception raises its rate about tenfold, but the plain throw and catch reaches it too. A bare `StackTrace`
walk in a program without golib did not fail in 8 runs, so something golib's `PanicException` does is also
involved; this reproduction does not isolate what that is.

A converted Go program that recovers 400,000 index panics through `defer`/`recover` 50 frames deep ran clean in
24 of 24 runs on the same base, so no converted-code instance is known.

## Amendment 2026-09-30: the runtime BUILD decides it (Canonical's fails, Microsoft's does not)

**Correction to the table above.** Its runtime is 10.0.12 (file version `10.0.1226.42308`), not "10.0.1". Its
counts are exit codes only: those loops sent stdout to a file and kept no per-run record of the depth lines,
and the reading below shows why that undercounts.

**Provenance of the box** (read-only):
- Ubuntu 24.04. `dotnet --info` RID `ubuntu.24.04-x64`: Canonical's build, from the packages
  `dotnet-runtime-10.0 10.0.12-0ubuntu1~24.04.1` and `dotnet-sdk-10.0 10.0.112-0ubuntu1~24.04.1`.
- The installed `libcoreclr.so` links the system unwinder: `libunwind.so.8` and `libunwind-x86_64.so.8`, from
  `libunwind8 1.6.2-3build1.1`. Its Build ID is `edc8e045d194821a314d9424319b8e77a9bc2943`, and its `.version`
  commit is `95017c711e6a`.
- Microsoft's `microsoft.netcore.app.runtime.linux-x64` 10.0.12 is the same commit and version string, but a
  different binary: `libcoreclr.so` sha256 `df5cfe7b...` against the installed `b5845ca1...`, Build ID `79945f51fb26...`.
  It links NO libunwind.

**The A/B.** The probe was built twice:
- framework-dependent, on the installed Canonical runtime;
- self-contained (`-r linux-x64`), with only the core runtime pinned through a targets file passed as
  `-p:CustomAfterMicrosoftCommonTargets`:
  `<KnownFrameworkReference Update="Microsoft.NETCore.App" LatestRuntimeFrameworkVersion="10.0.12" />`.
  The published `libcoreclr.so` is byte-identical to the NuGet cache's pack.

Every run used `DOTNET_TieredCompilation=1`. The four arms ran interleaved round-robin, and each run's
stdout and stderr went to a file. A run is BAD if it exits non-zero or ends without both depth lines.

| Runtime | Arm | Runs | Fatal (`0x80131506`, rc 134) | Silent early exit (rc 0, no `depth 50` line) | Bad |
|---|---|---:|---:|---:|---:|
| Canonical 10.0.12 | walk | 24 | 19 | 0 | **19** |
| Canonical 10.0.12 | `A8_NOWALK=1` | 48 | 3 | 28 | **31** |
| Microsoft 10.0.12 | walk | 24 | 0 | 0 | **0** |
| Microsoft 10.0.12 | `A8_NOWALK=1` | 48 | 0 | 0 | **0** |

**The silent early exit.** The process prints the `depth 5` line, writes one NUL byte, and exits 0 with no
`depth 50` line and no fatal text. An exit-code count reads it as a pass. It is the same fault's other face on
the no-walk arm, where it outnumbers the fatal exits 28 to 3.

**The bare-walk control** (`bare-walk/`, this commit; no golib: the same OSR loop shape over `System.Exception`,
with and without a `new StackTrace(1, false)` walk) was run 24 times per arm on each runtime: 0 bad in 96.
So the fault needs Canonical's build AND something golib's `PanicException` path does. The walk only raises
the rate.

**The crashing thread's native frames** (Canonical, walk arm; crash report, frames only):
- `libc.so.6 wait4`, which is the crash-report writer;
- then `libcoreclr.so` at `+0x6577a1 +0x658cb3 +0x656261 +0x65618d +0x498ee7 +0x41fdff +0x4c18a2 +0x616829`;
- exception `System.ExecutionEngineException`, and no managed frames.

These are UNSYMBOLIZED. Canonical's `libcoreclr.so` is stripped, and `dotnet-runtime-dbg-10.0` carries only
managed PDBs. The offsets resolve against Build ID `edc8e045...` with Canonical's native dbgsym.

**Reading.** This matches the upstream reports of Canonical's build faulting where Microsoft's does not
(dotnet/runtime #130577, #134044), which name system libunwind 1.8.x. This box has 1.6.2, so either the
version bound in those reports is too narrow, or the cause is another difference between the two builds.
This probe does not separate those two readings.
