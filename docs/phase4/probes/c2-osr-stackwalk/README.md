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
