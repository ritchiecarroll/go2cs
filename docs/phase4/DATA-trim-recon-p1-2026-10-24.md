# DATA — trim warnings and the trimmed traceback, reconciled (P1, 2026-10-24)

> **A point-in-time record.** COORD asked P1 to reconcile two of C2's measurements against P1's IL2036 seat post:
> 110 ILLink trim-analysis warnings where that post said the consumer's trim step emits none, and a trimmed binary that
> printed `panic: message` with no traceback. Measured at hop tip `a8821c781ae0277091912ebe6a75c7990ef3bafa`
> (batch 33) on linux-x64, with go1.26.10 and .NET SDK 10.0.112. **No fix is proposed here; each finding is for COORD
> to route.**

## 1. Why the IL2036 post said "none" — it was wrong

- **The counts in that post were all IL2xxx in the log.** The trimmed publish of PerfStartup logged 14 IL warnings:
  IL2026 8, IL2091 1 and IL2111 5. All 14 came from golib.csproj's compile-time analyzer, and the ILLink step itself
  logged none.
- **ILLink logged none because the harness suppresses them.** `src/tests/Performance/Directory.Build.props` sets
  `SuppressTrimAnalysisWarnings` to true for every benchmark project, PerfStartup included. That switch does not
  cover IL2036, which is why the IL2036 errors still showed and the fix could be proven there.
- **What was wrong:** the post generalized from that run to "a package consumer ... gets a warning-free trimmed
  publish". A converted program outside the benchmark tree does not carry the suppression. The control in section 2
  (the same program with `-p:SuppressTrimAnalysisWarnings=true`) reads 0, which is the shape the post measured.

## 2. A project-reference consumer (C2's measurement, reproduced)

**The program:** `fmt.Println("hello")`, then a function that calls `panic("message")`. It was converted with
`G -recurse -platforms linux/amd64` against the tip's `src`, then published with
`dotnet publish -c Release -r linux-x64`. That publish uses the converted csproj as emitted: `PublishTrimmed`, partial
`TrimMode` and ReadyToRun.

| Arm | Exit | IL warnings logged |
|:--|:--:|--:|
| default | 0 | **110**, every one an ILLink "Trim analysis warning" |
| `-p:SuppressTrimAnalysisWarnings=true` | 0 | 0 |
| `-p:PublishTrimmed=false` | 0 | 0 |

By code, the 110 are:

| Code | Count |
|:--|--:|
| IL2026 | 38 |
| IL2070 | 23 |
| IL2075 | 15 |
| IL2111 | 9 |
| IL2091 | 8 |
| IL2072 | 5 |
| IL2080 | 4 |
| IL2071, IL2067, IL2060, IL2055 | 2 each |

By assembly, read from each warning's source path, the 110 are:

| Assembly | Count |
|:--|--:|
| golib | 41 |
| runtime | 40 |
| reflect | 14 |
| time | 6 |
| unsafe | 5 |
| errors | 2 |
| internal/sync | 1 |
| internal/reflectlite | 1 |

This is C2's split exactly. The largest single group is `runtime_package.splicePanic`'s nine uses of
`StackFrame.GetMethod()`. A program with no imports logs 86.

## 3. A NuGet-shaped consumer

- **Feed:** the program's go.* closure, which is 62 projects plus go.gen, packed from the tip with
  `dotnet pack -c Release -p:GoTargetOS=linux -p:PackageVersion=1.26.10.0-local.1` into a local folder feed (63 nupkgs).
- **Deviation from a release:** a release merges three flavors, so its `lib/` asset is the Windows build and
  `runtimes/linux-x64` carries the linux build. These packages carry only the linux build. ILLink reads the runtime
  asset for the publish RID, and that asset is the same linux build in both shapes.
- **Consumer:** the same program, converted with `G -recurse=nuget`. A `nuget.config` maps `go.*` to the local feed only,
  `NUGET_PACKAGES` is a fresh directory, and the consumer was published with
  `-p:GoStdLibVersion=1.26.10.0-local.1 -r linux-x64`.

| Arm | Exit | IL warnings logged |
|:--|:--:|--:|
| default | 0 | **8**, all IL2104 ("Assembly '…' produced trim warnings"), one each for errors, internal.reflectlite, internal.sync, golib, reflect, runtime, time and unsafe |
| `-p:TrimmerSingleWarn=false` | 0 | **110**, with the same codes as section 2 |

- **What a real consumer sees:** by default, eight warnings, each naming an assembly. With `TrimmerSingleWarn=false`,
  they see the full 110.
- **An instrument trap:** ILLink is incremental and does not re-run for a changed `TrimmerSingleWarn`. A second publish
  into the same `obj` read 0. The 110 needed the intermediate output cleared first.
- **Runs:** both shapes run, and the trimmed output equals the untrimmed output.

## 4. The trimmed traceback: not reproduced, and the real dependency

**Not reproduced for a program that imports fmt.** In every arm below, the trimmed binary's stderr equals the
untrimmed build's, byte for byte:

- The section 2 program, in seven publishes:
  - the default (trimmed, ReadyToRun);
  - suppressed warnings;
  - untrimmed (ReadyToRun only);
  - single-file trimmed;
  - single-file untrimmed;
  - framework-dependent trimmed;
  - the NuGet consumer.

  The single-file arms ran without ReadyToRun; see finding 3.
- Seven panic shapes, each untrimmed and trimmed: a direct panic, an index out of range, a nil-map write, an error
  value, a recover-and-repanic, a goroutine panic, and a panic in main.
- A program whose only fmt call sits on a path that never runs, in three publishes: the default, trimmed without
  ReadyToRun, and ReadyToRun without trimming.

The one publish whose stderr differs is `-p:DebugType=none`; see finding 2.

**The real dependency.** go.runtime's `[ModuleInitializer]` registers the traceback renderer
(`managed_impl.cs`, `ᴛRegisterCrashTraceback`). A module initializer runs only when its assembly is first touched.
When nothing touches runtime before the panic, `CrashReport.Format` finds no renderer, and the report is
`panic: <value>` alone. This happens with or without trimming:

| Program | Untrimmed | Trimmed |
|:--|:--|:--|
| `func main() { panic("message") }`, no imports | no traceback | no traceback |
| The same, after one `runtime.NumGoroutine()` call | traceback | traceback |

This is C2's no-imports case and R's fix (1) on `claude/r-crash-traceback`. Trimming does not change it. C2's trimmed
fmt probe was deleted after its post, so its exact program cannot be re-read. If it still reproduces at `017dafb1f2`,
P1 asks for the program.

## 5. Findings outside the trim question

1. **A goroutine that panics after its deferred `wg.Done()` lets main exit 0 silently.** Go exits 2 with the report.
   Repro, in main: `var wg sync.WaitGroup; wg.Add(1); go func() { defer wg.Done(); panic("in goroutine") }(); wg.Wait()`.
   - Go read 3/3 with exit 2. The converted program read 5/5 with exit 0 and empty stderr, the same untrimmed and
     trimmed.
   - The same panic without the WaitGroup (main sleeps) reports correctly.
   - Go guarantees this order: `runtime.main` waits for a goroutine that is still running panic defers or printing a
     panic before it exits (`runtime/proc.go:307-321`, "let the other goroutine finish printing the panic trace";
     issues 3934 and 20018).
   - In the conversion, the deferred `Done` wakes main, and main's return ends the process before the goroutine's
     unhandled-exception report is written.
2. **Positions come from the PDB.** With `-p:DebugType=none` the traceback keeps its frames but loses every
   `file:line` line.
3. **A single-file ReadyToRun publish exhausts memory.** `-p:PublishSingleFile=true` with the csproj's ReadyToRun on
   ran crossgen2 to 10 GB RSS and failed with "Insufficient memory". MSBuild then hung, and it was stopped by PID. With
   ReadyToRun off, the single-file publish succeeded.
