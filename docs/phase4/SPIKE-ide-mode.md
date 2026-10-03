# SPIKE — go2cs IDE mode, Phase 0 feasibility (cloud half)

> **Record, not a plan.** The Phase 0 spike of [PLAN-ide-mode.md](../PLAN-ide-mode.md), cloud half, run by lane C2 on a
> Linux cloud container (linux/amd64, Go 1.24.13, Microsoft .NET SDK 10.0.112). One section per experiment, each green or
> red with its numbers. Predictions are written before the measurement they predict and are never edited afterwards.

## Summary

| Experiment | Verdict | One line |
|:--|:--|:--|
| E1 latency | GREEN | hello-world 3.1 s; 10-package module 10.8 s whole, 3.0–3.4 s changed package. Conversion, not compilation, dominates. |
| E2 netcoredbg | NOT RUN | The debugger cannot be downloaded here. The PDB half is answered by E6, E7 and E9. |
| E5 Hot Reload | AMBER | Blocked by default (startup hooks off). With hooks on, body edits apply live in 2.2 s; literal and line-count edits restart in 4.4 s. |
| E6 stepping density | GREEN for (b) hidden | 18/18 statement lines, 33 points, 9 repeats, against hand-placed 38 and 10. |
| E6b line model | GREEN, one RED | A collapsed literal whose body is on its own Go line cannot bind; a converter fix. |
| E7 `runtime.Caller` | positions GREEN, names RED | Function-literal names fall back to Roslyn ordinals under `#line`. |
| E8 build shape, CLI | GREEN for build and run; `dotnet watch` RED as configured | One project builds and runs either shape. Under `dotnet watch`, a `.go` edit to a running consumer is lost with Hot Reload on, and loops forever with it off. Run outside the build, the conversion is applied live. |
| E9 `#line` source | **(b), hidden mode** | Decodes the whole corpus; `strconv` binds 1,567 of 1,730 statement lines against the runtime map's 1,571. |

## E1 — latency

### Prediction (written 2026-10-03T21:58Z, before any E1 measurement)

Warm loop = edit a `.go` file, convert, build (Debug), launch until the program's first output. A debugger's launch
and attach cost is not in these numbers (no debugger here yet; see E2).

| Sample | Path | Convert | Build | Launch | Total |
|:--|:--|--:|--:|--:|--:|
| hello-world (`fmt` only) | whole module | 1.5 s | 3 s | 0.3 s | **≈ 5 s** (at the plan's 5 s bar) |
| 10-package module | whole module, every `.cs` rewritten | 3 s | 12 s | 0.3 s | **≈ 15 s** (at the plan's 15 s bar) |
| 10-package module | one changed package, with the write fix | 2 s | 5 s | 0.3 s | **≈ 7 s** |

Reasoning: MSBuild's fixed cost per `dotnet build` invocation dominates small projects (about 2–3 s warm); every
reconverted project recompiles today because the converter rewrites every `.cs`, so the whole-module build scales
with the number of projects; the write fix limits recompilation to the changed project and its dependents. The cold
first restore of the go.* closure is predicted at 20–60 s, network-bound, and is reported separately.

### Setup

- **Converter arm:** master `8f46a9adae`'s converter, `go install`ed, against the **published** go.* 1.24.13.3 from
  nuget.org (`-recurse=nuget`). TRAIN N's converter is not usable against the published packages until 1.24.13.4: it
  emits `unsafe.ArrayPointer`, which 1.24.13.3 lacks (ruled 2026-10-03; the window is accepted).
- **Isolation:** fresh GOPATH, GOMODCACHE, GOCACHE and NUGET_PACKAGES; `GOTOOLCHAIN=local`; MSBuild terminal logger
  off; the Roslyn compiler server left at its default (on), as an IDE build has it.
- **Samples:** *hello-world* (`package main` printing a package-level string through `fmt`); *10-package* (`main` →
  `p1` → … → `p9`, a chain, each package using `fmt`). Both build and run as Go first.
- **One iteration** = change a version string in one `.go` file, convert, `dotnet build <app>.slnx -c Debug
  --no-restore`, run the native launcher until it exits. An iteration counts only if the launcher's output carries the
  new string (so a stale build cannot pass as a fast one). Three timed iterations per row, after one discarded warm-up;
  the table gives medians. Every iteration passed.
- **Paths.** *Whole module:* `go2cs -recurse=nuget . out`, which rewrites every `.cs`. *Changed package:* `go2cs
  ./<pkg> <staging>` for the edited package only, copied into the tree; its `.cs` is byte-identical to the whole-module
  emission (checked with `cmp` for `main` and `p9`). *Write fix* (simulated): convert into a staging copy and copy into
  the tree only the `.cs` files whose bytes changed.

### Results

| Sample | Path | Convert | Build | Launch | Total |
|:--|:--|--:|--:|--:|--:|
| hello-world | whole module | 1.36 s | 1.21 s | 0.50 s | **3.07 s** |
| hello-world | whole module, write fix | 1.34 s | 1.15 s | 0.46 s | 2.97 s |
| 10-package, edit in `main` | whole module | 7.23 s | 2.68 s | 0.48 s | **10.80 s** |
| 10-package, edit in `main` | whole module, write fix | 6.83 s | 1.82 s | 0.47 s | 9.12 s |
| 10-package, edit in `main` | changed package | 0.79 s | 1.74 s | 0.46 s | 3.04 s |
| 10-package, edit in `main` | changed package, write fix | 0.83 s | 1.77 s | 0.45 s | **3.05 s** |
| 10-package, edit in leaf `p9` | whole module | 6.50 s | 2.11 s | 0.46 s | 9.03 s |
| 10-package, edit in leaf `p9` | whole module, write fix | 6.65 s | 2.25 s | 0.45 s | 9.37 s |
| 10-package, edit in leaf `p9` | changed package | 0.67 s | 2.20 s | 0.46 s | 3.33 s |
| 10-package, edit in leaf `p9` | changed package, write fix | 0.71 s | 2.25 s | 0.49 s | 3.42 s |

**Cold, reported separately:** the first `dotnet restore` of hello-world's go.* closure took **20.4 s** (61 go.*
packages, 120 MB, from nuget.org into an empty NUGET_PACKAGES); the 10-package module then restored in 2.5 s from the
populated folder. First conversions: 1.1 s (hello-world) and 7.5 s (10-package); first builds about 5 s each.

**Recompilation, counted:** after a whole-module conversion, a build recompiled **10 of 10** assemblies; with the
write fix it recompiled **1** (assemblies under `.artifacts/obj` newer than the build's start).

### Scored against the prediction

| Row | Predicted total | Measured | Components |
|:--|--:|--:|:--|
| hello-world | ≈ 5 s | 3.07 s | build 2.5× faster than predicted (1.2 s against 3 s); launch 1.7× slower (0.5 s against 0.3 s) |
| 10-package, whole module | ≈ 15 s | 10.80 s | **inverted**: convert 2.4× slower (7.2 s against 3 s), build 4.5× faster (2.7 s against 12 s) |
| 10-package, changed package + fix | ≈ 7 s | 3.05–3.42 s | build 2.8× faster |

The totals held; the model behind them did not. The whole-module loop is dominated by **conversion** (the converter
loads and type-checks the module and its stdlib closure on every run), not by compilation. Recompiling ten small
projects costs well under a second more than recompiling one, so the `.cs` write fix saves at most 0.9 s here. It
would save more on larger packages, but the lever that matters is converting **only the changed package**: about 0.7 s
against 6.5–7.2 s.

### Verdict: GREEN

Both bars hold with margin: hello-world 3.1 s against 5 s, and the 10-package module 10.8 s (whole module) or 3.1–3.4 s
(changed package) against 15 s. No row is near twice its bar, so the F5-on-Go positioning is **not** dropped on E1.

**What these numbers leave out:** the debugger. They stop at the launched program's exit, so a debugger's own launch
and attach, and the time to the first breakpoint, are not included (no debugger is available here; see E2). The loop
also assumes the build package converts only changed packages, which today's converter can do per package
(`go2cs ./<pkg> <out>`) but nothing yet orchestrates.

## E7 — `runtime.Caller` under `#line`

**Sample.** `where()` returns `runtime.Caller(1)`'s file, line and `FuncForPC(pc).Name()`; `main` calls it directly,
through a closure inside a closure (`outer` → `inner` → `where`), through an immediately invoked literal, and after a
multi-line call. Converted by master's converter into an `-recurse=nuget` tree; built and run three ways: the default
emission, and two `#line` copies of `main.cs` written by the option-(b) prototype (see E9), *re-anchor* and *hidden*.

| Call site (Go line) | `go run -gcflags=all=-l` | default emission | `#line`, either mode |
|:--|:--|:--|:--|
| direct (15) | `main.go:15 main.main` | `main.go:15 main.Main` | `main.go:15 main.Main` |
| closure in a closure (19) | `main.go:19 main.main.func1.1` | `main.go:18 main.Main.func1.1` | `main.go:18 main.Main.func2` |
| immediately invoked literal (26) | `main.go:26 main.main.func2` | `main.go:26 main.Main.func2` | `main.go:26 main.Main.func1` |
| after a multi-line call (33) | `main.go:33 main.main` | `main.go:33 main.Main` | `main.go:33 main.Main` |

(Every file column is the same absolute path to `main.go`; with inlining on, Go names the closure-in-a-closure
`main.main.main.func1.func3`, so the uninlined build is the baseline.)

**Position half: GREEN.** Under `#line` every frame reports the absolute `.go` path and the Go line, exactly as the
default emission does. There is no double mapping: the position-map lookup, keyed by the `.cs` file name, misses, and
the runtime reports the PDB's position, which is now Go's. The path matches Go's untrimmed build.

**Name half: RED, as the plan predicted.** Under `#line` the function-literal names fall back to Roslyn's ordinals:
the closure in a closure becomes `func2` (was `func1.1`, which matches Go) and the immediately invoked literal becomes
`func1` (was `func2`). Nesting is lost and the ordinals swap. The fix is the plan's Phase 2 item: a function-literal
identity record keyed by the Go file, not the `.cs` file.

**Two findings that are not about `#line`:**
- The entry point reports `main.Main` where Go reports `main.main`, in every mode. That is a naming difference in the
  default emission, independent of IDE mode.
- The closure in a closure reports line 18 where Go reports 19, in every mode. The converter collapses
  `inner := func() string { return where() }` onto one C# line (an expression-bodied local function), so Go line 19
  has no C# line of its own: the position map maps C# line 18 to Go line 18, and the PDB has no sequence point on Go
  line 19 in any variant. A breakpoint on that line cannot bind. See E6b.

**PDB evidence** (portable PDB read with `System.Reflection.Metadata`): under `#line` the PDB gains a document for
`main.go` whose SHA-256 checksum equals the file's (`78933d1a…`), so `#pragma checksum` round-trips.

## E6 and E6b — stepping density and the line model (PDB half)

**What is measured, and what is not.** A breakpoint binds, and a step can stop, only where the portable PDB has a
sequence point. These results read the sequence points of each build directly (`System.Reflection.Metadata`), so
they show where a debugger *can* bind and stop. They do not show how a given debugger merges consecutive sequence
points on one line when stepping; that is the debugger half (E2, and the desktop checklist).

**Sample** (39 Go lines): a two-result function; a named-result function with a `defer` literal and a `for`/`if` loop;
`main` with a tuple assignment, a `sync.WaitGroup`, a `for range` loop spawning goroutines whose literal `defer`s
`wg.Done()` (the converter hoists a capture copy, `var resultsʗ1 = results;`, ahead of `goǃ`), a collapsed
expression-bodied literal (`double := func(x int) int { return x * 2 }`), and a call split over four lines. The
reference is the start line of every Go statement, read from the Go AST: 18 lines.

Five builds of the same conversion, each output-identical to `go run`:
- **default:** the converter's emission, with the `.cs` sequence points translated through the position map (what
  the runtime answers);
- **(b) re-anchor:** every C# line after a position-map record re-anchored (`#line G`) to that record's Go line;
- **(b) hidden:** a record's own line mapped and the lines after it `#line hidden` until the next record;
- **hand:** directives I inserted line by line: statement lines mapped, scaffolding and a statement's second C# line
  hidden, and the closing braces of methods and literals mapped to Go's closing-brace line (which needs
  `go/token` End positions, so only option (a) could do it automatically);
- **naive:** a directive before each record only, the plan's counter-example.

| Build | Sequence points | Statement lines bindable | Same-line repeats | Lines that are not statements |
|:--|--:|--:|--:|:--|
| default | 61 | 18/18 | 35 | 10, 22 (func lines), 31 (`}()`) |
| (b) re-anchor | 61 | 18/18 | 35 | the same |
| **(b) hidden** | **33** | **18/18** | **9** | the same |
| hand | 38 | 18/18 | 10 | the same, plus closing braces 13, 20, 39 |
| naive | — | — | — | wrong lines: `pair`'s `}` on the blank line 9; `work`'s `catch`/`finally` on 20–23, past the method; a span to line 40 in a 39-line file |

**E6: GREEN for (b) hidden.** It binds every statement line and stops almost exactly as often as the hand-inserted
directives (33 against 38 points; 9 against 10 same-line repeats). The only thing it cannot do is stop on a Go
closing brace, which the hand version deliberately adds. Re-anchoring is worse: every scaffolding and closing-brace
line becomes another stop on the previous statement's Go line (35 same-line repeats), so a user would press Step
several times on one line. The naive dump is wrong, as the plan predicted.

**E6b, the line model:**
- **Multi-line call** (Go 35–38, C# 50–52): one sequence point on Go 35 in every mapped build; the continuation lines
  carry none. GREEN.
- **Hoisted temp** (`var resultsʗ1 = results;` before `goǃ`): carries Go 28, the `go` statement's line, so a
  breakpoint on line 28 binds before the goroutine is spawned. GREEN.
- **Collapsed expression-bodied literal on one Go line** (`double`, Go 34): binds on 34. GREEN.
- **Collapsed literal whose body is on its OWN Go line** (E7's `inner := func() string {⏎ return where() ⏎}`): RED,
  in every build. The converter collapses it to one C# line, so the body's Go line (19) has no C# line, no position-map
  record and no sequence point; a breakpoint there cannot bind. No `#line` writer can fix that: the converter would
  have to keep a block body when the literal's body starts on a later Go line than the literal.

## E5 — Hot Reload under `dotnet watch`

**Setup.** A ticker that prints its process id, a counter and `message()` every 0.5 s for two minutes, converted
by master's converter. `dotnet watch --verbose run -c Debug` (SDK 10.0.112, Microsoft runtime) runs in the converted
project directory. Each edit is made to `main.go`, reconverted into a staging directory, and only the `.cs` files
whose bytes changed are copied over the watched project. A control is a plain `dotnet new console` app with the
same loop.

**The default emission blocks Hot Reload entirely.** The control hot-reloads (same process id, new text). The
converted app never connects to `dotnet watch`'s agent pipe: no update is sent and the output never changes. The
cause is in the emitted csproj: an executable project sets `<PublishTrimmed>True</PublishTrimmed>`, the SDK then
defaults `StartupHookSupport` to `false` for every configuration, Debug included, and the runtimeconfig carries
`"System.StartupHookProvider.IsSupported": false`. `dotnet watch` injects its delta applier as a startup hook, so
the hook is never loaded. With `StartupHookSupport=true` (set here from the environment) the runtimeconfig reads
`true` and the app connects. An IDE mode needs `StartupHookSupport` true in Debug, or `PublishTrimmed` set only
when publishing.

**With startup hooks enabled:**

| Edit | Result | Time |
|:--|:--|:--|
| change an integer argument inside a method body (`fmt.Sprint("tick ", 1)` → `2`) | **applied live**, same process id | file change seen to first new line 2.2 s (`dotnet watch` reports 1.94 s) |
| change a hoisted string literal (`return "tick v1"` → `"tick v2"`) | **restart**: `ENC0020 Renaming field 'tickV1ˢ'` | edit to first line of the new process 4.4 s |
| add a statement (`suffix := "!"`) | **restart**: `ENC0003 Updating 'attribute'` on `package_info.cs`'s `[assembly: GoPositionMap(…)]`, plus `ENC0033` deleting the old literal field | not timed |
| add an import (`strings`) | **build fails** until the csproj is regenerated: `CS0246 'strings_package'` | — |

`dotnet watch` restarts the process on its own after a rude edit (its non-interactive mode); the timed restart took
4.4 s. Only edits that change neither a hoisted string literal nor any line count were applied without a restart.

**Why the emission causes restarts, and what would change it:**
- **String literals.** The converter hoists a string literal that stands alone as a value (`return "tick v1"`, or
  an argument to `fmt.Sprint`, where it is hoisted already boxed as `object`) into a static field named from its
  content (`tickV1ˢ`), so changing the text renames a field. A literal inside a concatenation stays inline, and an
  edit to it applies live (E8). A name from the content is what the emission uses today; a name
  that stays the same would turn the rename into a change to a static initializer, which `ENC0118` says may have no
  effect until restart. Not tested further: either way a changed literal does not take effect live.
- **The position map.** `GoPositionMap` is an assembly-level attribute holding the whole file's line map, so any edit
  that moves a line changes it, and the runtime cannot update an assembly attribute in place. Under `#line` the line
  information also goes into the PDB, which Hot Reload updates, and E7 shows the runtime no longer uses the map then
  (its lookup by `.cs` name misses). So an option-(b) task could leave the attribute out of its transient copy of
  `package_info.cs` and remove this restart cause. That is inferred from E7 and not tested under Hot Reload.
- **Imports.** A new import is a new project reference. `dotnet watch` treats a csproj change as a full rebuild, and
  the reconversion has to write the csproj too. See E8.

**Verdict: AMBER.** Hot Reload works for body-only edits once startup hooks are on (2.2 s, and the program keeps its
state). Every other edit restarts the program, which is the E1 changed-package loop (3.0–3.4 s) plus `dotnet watch`'s
own detection and shutdown: 4.4 s measured. Applying ordinary Go edits (changed text, added lines) live needs the two
emission changes above.

## E8 — the build shape (CLI part)

**Question.** Can one project restore, build and run the multi-project output, in the Go-`main` shape and the
C#-consumer shape, and does `dotnet watch` follow `.go` edits? The Visual Studio and Rider half (F5, the fast
up-to-date check, conversion on project load) is on the desktop checklist.

**Setup.** Master's converter with `-recurse=nuget`, SDK 10.0.112, packages already in the local NuGet folder. Both
host projects convert into `obj/go2cs/<Configuration>/` under themselves, which keeps Debug and Release apart. Each
declares `**/*.go` and `go.mod` as `GoSource`, `Watch` and `UpToDateCheckInput` items, converts only when a source is
newer than a stamp file, and skips conversion when `DesignTimeBuild` is `true`. Both projects are about 40 lines of
MSBuild and need no converter change.

### Go-`main` shape: GREEN for build and run

The 10-package module from E1, hosted by one `Microsoft.Build.NoTargets` project:
1. `GoConvert` (before `Build`) runs `go2cs -recurse=nuget . obj/go2cs/<Config>`.
2. `GoRestore` restores the generated main project through the `MSBuild` task, with a unique global property so
   the restore gets its own evaluation. It runs only when a generated `.csproj` or `Directory.Build.*` file is newer
   than the last restore. The converter skips writing an unchanged project file, so a body edit does not restore.
3. `GoBuild` builds the generated main project and writes its `TargetPath` to a file.
4. The host sets `RunCommand`/`RunArguments` at evaluation from that file, so `dotnet run` launches the generated
   assembly. Computing them in a target hooked to `ComputeRunArguments` gave the right values under
   `dotnet msbuild`, but `dotnet run` still refused the project as not runnable.

| Run | Time | Output |
|:--|--:|:--|
| cold: no `obj`, packages cached | 14.2 s | identical to `go run` |
| nothing changed | 4.0–4.6 s (6.7 s on the first run after the host project itself changed) | no conversion, no restore |
| `main.go` edited | 11.5–13.1 s | new text |

The edited run profiles at 11.9 s: conversion 6.7 s, nested build 3.8 s, the host's own restore and evaluation the
rest. That is E1's whole-module loop plus about 1 s for the host project. The whole module is converted, so all 10
assemblies recompile. Converting only the changed package (E1: about 0.7 s) is the lever, and nothing in MSBuild
yet decides which package changed.

**`dotnet watch` on the host:**
- `dotnet watch` lists the `.go` files outside the project directory as watched, and its `GenerateWatchList`
  evaluation does not trigger a conversion.
- With Hot Reload on (the default) and the default emission, the first launch never connects to the agent pipe (the
  E5 cause), and `.go` edits are never acted on.
- With startup hooks enabled, or with `--no-hot-reload`, every `.go` edit rebuilt and restarted the program. Edit to
  new output: 13.0–13.7 s for `main.go`, 11.7 s for a leaf package; 13.6–14.7 s with `--no-hot-reload`. **This sample
  exits at once**, and `dotnet watch` restarts an exited program on any watched change. With Hot Reload on and the
  program still running, a `.go` edit is not C#, so it would most likely be dropped as in the consumer below. That
  case was not run in this shape. With `--no-hot-reload`, `dotnet watch` restarts a running program on any change.
- No hot reload of converted code is possible in this shape: the generated projects are not in the host's
  project graph, so `dotnet watch` has no Roslyn workspace for them.

### C#-consumer shape: GREEN for build and run, RED for `dotnet watch` as configured

A C# console app references the generated project of a two-package Go library module (`example.com/shared`, which
imports `example.com/shared/words`) with an ordinary `ProjectReference`. The generated project does not exist
until conversion runs, so the conversion has to run before NuGet walks the restore graph:
- hooked `BeforeTargets="_GenerateRestoreGraph"`, it ran too late. The walk had already skipped the missing project,
  and the build failed with `NETSDK1004` (no `project.assets.json` for the generated project);
- hooked `BeforeTargets="_LoadRestoreGraphEntryPoints"` (and `BeforeBuild`), a cold `dotnet run` converts, restores
  both generated projects with the app and runs: 7.6 s. The C# code calls `shared_package.Greet("dotnet")` with a
  plain `string` argument.

| Run | Time |
|:--|--:|
| cold: no `obj`, packages cached | 7.6 s |
| nothing changed | 3.1–3.9 s |
| `words.go` edited | 4.4–4.7 s |

**`dotnet watch` on the consumer:**
- **Hot Reload on, conversion in the build:** a `.go` edit is seen, `dotnet watch` reports "No C# changes to apply",
  and nothing is converted or rebuilt. The edit is lost until a manual restart.
- **Hot Reload on, conversion run outside the build** (the same `go2cs` command started by hand after each edit, as a
  `go2cs`-side file watcher would): `dotnet watch` sees the regenerated `.cs` files, because the generated projects
  are in the app's project graph, and **applies both edits live in the same process**. From the `.go` save to the
  first changed output line: 2.9 s for an integer change in `Count()` (conversion 1.4 s, `dotnet watch` reports the
  update handled in 2.0 s) and 1.5 s for a string change in `Hello()`. That string is concatenated, not hoisted to a
  field (contrast E5).
- **`--no-hot-reload`, conversion in the build: an endless restart loop.** The build's conversion rewrites the
  generated `.cs` files, which `dotnet watch` also watches. It kills the build (SIGKILL) before the target writes its
  stamp, so the next build converts again, which rewrites the files again. It cycled every 0.5–1.5 s (23 conversions
  and 47 kills in 47 s) until the watcher was stopped. The output tree was intact afterwards and built and ran
  correctly, but a conversion killed part-way through writing is the "raced conversion" case of the safety floor.

**What a Phase 3 design needs, from E8:**
1. Generated `.cs` files must not be `Watch` items when the conversion runs inside the build (for example `Watch="false"`
   in the generated `Directory.Build.targets`), and they must be when it runs outside (the live-apply path).
2. Hot reload of converted code needs the generated projects in the watched project graph (the consumer shape)
   and a conversion that runs on `.go` change outside the build. The traversal host (Go-`main` shape) can only restart.
3. The `.cs` write-skip (Phase 2) stops an unchanged file from looking changed. That matters more here than for build
   time: every rewritten file is a reload or a restart.
4. The conversion target has to run before `_LoadRestoreGraphEntryPoints` in a project that references generated
   projects, or the generated projects never restore.
5. Startup hooks have to be on in Debug (E5) for any Hot Reload path.

## E9 — where the `#line` text comes from

**Prototype.** Option (b) is prototyped as a script that reads one package's `package_info.cs`, decodes each
`GoPositionMap` record and writes a transient copy of each `.cs` file with `#pragma checksum` and `#line` directives.
An 8-line MSBuild targets file (passed as `CustomAfterMicrosoftCommonTargets`) swaps the copies in for the committed
files, which is the shape the build package's task would take. Mode `hidden` (from E6) maps a record's own line and
hides the lines after it. One improvement came out of this experiment: a record whose C# line holds only a comment is
moved to the next line with code (`--skip-comment-records`; see below).

**At corpus scale (the committed tree at the spike's base):**
- **Decode.** Every record in the corpus decodes: 373 packages, 2,165 files, 280,188 records, 0.5 s in one Python
  process. In every file the C# lines ascend and stay inside the file, and every Go line stays inside its Go file. Two
  layout facts a task has to handle: a standard-library record names its Go file relative to `GOROOT/src`
  (`fmt/errors.go`), and a record in a per-GOOS `package_info.cs` names a `.cs` file in the flat package folder above
  it (or the reverse, for `go/build`'s `build.cs`).
- **Compile.** `strconv` (13 converted files, 1,730 Go statement lines) built from the transient copies with 0
  warnings and 0 errors, and the assembly is the same size as the default build's (433,152 bytes). Its PDB names the 13
  `.go` files with their SHA-256 checksums.

**Statement coverage on `strconv`** (a Go statement line counts as bound when at least one sequence point maps to it;
the default build's `.cs` points are translated through the position map, which is what the runtime reports and
what option (b)'s re-anchor mode would give):

| Build | Statement lines bound (of 1,730) | Mapped sequence points | Same-line repeats |
|:--|--:|--:|--:|
| default, translated | 1,571 | 2,913 | 1,148 |
| (b) hidden | 1,555 | 2,064 | 396 |
| **(b) hidden, comment records moved** | **1,567** | 2,154 | **396** |

- **Missed by every build: 159 lines.**
  - 124 are Go `case` clauses. A C# `case` label has no IL of its own, so no option can bind one.
  - 17 are `} else if cond {` lines. The converter splits them into `} else` and `if (cond) {` on two C# lines and
    the map records neither, so the condition's line answers the previous record: in `atof.go`, Go 116's condition
    reports Go 115. This is a defect in the runtime's positions too: a panic in such a condition reports the wrong
    line. It is a converter fix, and it benefits every option.
  - 18 others (13 assignments, 2 declarations, 1 each of `for`, `++` and a combined line) are not root-caused. One of
    them, `decimal.go:48`, also shows a comment-placement defect, though it is not shown to be why the line is
    missed: the whole-line comment above `w += copy(buf[w:], a.d[0:a.dp])` is emitted inside the call's argument
    list, splitting the statement over two C# lines.
- **Missed by (b) only: 16 lines, then 4.** Twelve were declarations such as `var d decimal`, whose record the converter
  puts on the C# line of the comment above the statement (`// Slow fallback.`). In hidden mode the statement's own line
  is hidden. Moving a comment-only record to the next code line recovers 12 of the 16; the other 4 are not
  root-caused.
- **Stepping.** Hidden mode removes two thirds of the same-line repeats (1,148 to 396), which agrees with E6's sample
  (35 to 9).

**Option (c) was not prototyped.** It reads the same table as (b), so its line fidelity is the same. What separates it:
- It leaves the `.cs` and the IL untouched, so compile errors stay on the `.cs` (in (b) an error in a mapped line is
  reported on the `.go` line).
- It has to rewrite the PDB after every build. Hot Reload produces its own PDB deltas from source, so methods updated
  by Hot Reload would revert to `.cs` lines (inferred, not tested).
- It is the only option that could reach the published go.* packages. Their nupkgs carry no PDB. nuget.org reports a
  symbol package for go.fmt 1.24.13.3, but its download host is blocked in this environment, so its contents were not
  checked.

### Recommendation: (b), hidden mode, comment records moved

Evidence for (b) over (a):
- (b)-hidden binds every statement line in the E6 sample (18/18), and on `strconv` it binds 4 fewer lines than the
  runtime's own map (1,567 against 1,571).
- (b)-hidden steps almost exactly like hand-placed directives (E6: 33 points and 9 repeats, against 38 and 10).
- What (a) adds over (b) is a stop on closing braces (E6), and later columns (2b). The `else if` and comment-line
  cases above are defects in the table, which a converter fix removes for (b) as well.
- (b) runs on today's committed trees and any locally converted tree, needs no converter-source freeze, and leaves
  Q2 moot.

Re-anchor mode is ruled out by the repeat count, and the naive mode is wrong (E6). Choose (c) instead only if
stepping into the published go.* packages becomes a requirement and their symbol packages turn out to carry PDBs.

**Converter items every option needs,** each small and justified independently of IDE mode:
1. **Startup hooks in Debug** (E5). An executable project's emitted `PublishTrimmed` disables startup hooks in every
   configuration, which blocks Hot Reload.
2. **`else if` conditions get a position-map record** (E9). The runtime reports the wrong line today.
3. **A literal whose body starts on a later Go line keeps its block body** (E6b, E7).
4. **Function-literal names under `#line`** (E7: `func1.1` reported as `func2`), which the plan's Phase 2
   identity record covers, and the entry point's name (`main.Main` against Go's `main.main`) in every mode.
5. **Leading-comment placement**: a record on the comment's line (worked around in (b)), and a whole-line comment
   emitted inside an expression (`decimal.go`).

No item is proposed for hoisted string literals (E5): with a content-derived name a changed literal restarts, and with
a stable name `ENC0118` says the change may have no effect until restart.

## E2 — netcoredbg over DAP: NOT RUN

netcoredbg is distributed as GitHub release assets, and this environment's network policy denies github.com and its
release-download host, so the debugger could not be installed. vsdbg is not an option here: its licence limits it to
Microsoft's own products (the plan's licence row). The PDB half of E2's question is answered above: under `#line` the PDB carries `.go` documents with
matching SHA-256 checksums (E7, E9) and a sequence point on every mapped statement line (E6, E9). Whether a debugger
binds a `.go` breakpoint to those points, shows `.go` frames and accepts the checksum under `requireExactSource` is
still open. It can be run here once the two hosts are allowed, or moved to the desktop half below.

## The desktop half: what is left, and what each item needs

Each item is answered green or red, with a screenshot.

| Item | Question | Inputs from this record |
|:--|:--|:--|
| E2, vsdbg | In Microsoft VS Code with the C# extension: does a breakpoint set in a `.go` file bind and stop, does the call stack show `.go:line`, and does `requireExactSource` accept the file? Do golib values (`slice<T>`, `@string`, `map<K,V>`) display, raw? | A converted sample built from (b)-hidden copies (E9); `launch.json` of type `coreclr` with `program` set from `dotnet msbuild -getProperty:TargetPath` |
| E2, netcoredbg | The same questions over DAP, if this environment cannot get netcoredbg. | The same build |
| E3 | Visual Studio: does F9 in a `.go` file set and bind a breakpoint? | The same build, opened as the generated `.slnx` |
| E4 | Rider: do `.go` breakpoints bind without a plugin? | The same build |
| E8, IDE | Does Visual Studio or Rider load the two E8 host projects without running a conversion (`DesignTimeBuild` skip)? Does F5 run them? Does Visual Studio's fast up-to-date check rebuild after a `.go` edit, given the `UpToDateCheckInput` items? | The two host projects of E8 |

The swap that compiles the (b) copies in place of the committed files, used for every E9 build:

```xml
<Project>
  <ItemGroup Condition="'$(MSBuildProjectName)' == '$(GoLineProject)' and '$(GoLineDir)' != ''">
    <_GoLineCopy Include="$(GoLineDir)*.cs" />
    <Compile Remove="@(_GoLineCopy->'$(MSBuildProjectDirectory)/%(Filename)%(Extension)')" />
    <Compile Include="@(_GoLineCopy)" />
  </ItemGroup>
</Project>
```

passed as `-p:CustomAfterMicrosoftCommonTargets=<file> -p:GoLineProject=<project name> -p:GoLineDir=<folder of copies>/`.
