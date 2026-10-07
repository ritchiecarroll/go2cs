# Known issues

This page lists specific bugs, each with a workaround. For the limits that come with running Go on .NET, and
the rough spots real code hits today, see [Limitations](Limitations.md).

## Ubuntu's packaged .NET 10 can end a converted program early

On Ubuntu's own build of .NET 10 (the `dotnet` installed with `apt`, runtime identifier `ubuntu.*-x64`), a
converted program that raises and recovers Go panics inside a hot loop can stop in one of two ways under the
default tiered compilation:
- a fatal `Internal CLR error. (0x80131506)`;
- more often, a SILENT exit with code 0 and truncated output.

The fault needs the JIT's on-stack replacement. Microsoft's build of the same .NET version does not show it, and
neither does Windows. No converted Go program has reproduced it so far, only a synthetic reproduction.

**To avoid it**, run on Microsoft's .NET build, or turn off quick JIT for loops, which removes on-stack
replacement. A build property does it without touching the converted project:

```shell
dotnet build -p:TieredCompilationQuickJitForLoops=false
```

The built program's `runtimeconfig.json` then carries
`"System.Runtime.TieredCompilation.QuickJitForLoops": false`. Or set it in the converted executable's project
file, though the next conversion rewrites that file:

```xml
<TieredCompilationQuickJitForLoops>false</TieredCompilationQuickJitForLoops>
```

Converted executable projects carry this line, commented out. It costs about 90 ms (16%) more startup time,
and steady-state loop speed stays within measurement noise. It becomes the default if a converted Go program
ever reproduces the fault.

## Publishing a converted program twice loses file and line numbers in stack traces

<!-- Measured 2026-10-06 on the go.* 1.24.13.4 packages and on master: the i9's readings (mailbox 48323a941d,
     40afec31fd: a first publish keeps every symbol file, the same publish again keeps only the program's own;
     removing the executable or the folder restores them for one publish) and CI on linux-x64, win-x64, osx-x64
     and osx-arm64 (run 37428852458). The fix is the seat claude/i9-publish-symbols-loose; delete this section
     when a release carries it. -->

A converted executable project carries publish profiles (`win-x64`, `linux-x64`, `osx-arm64` and the rest).
Each one publishes a self-contained, single-file program into a fixed folder. When the same publish runs a
second time with nothing changed, the .NET SDK skips rebuilding the single file and then removes, as
leftovers, the symbol files (`.pdb`) of every library the program references as a project. Only the program's
own `.pdb` stays. It happens on Windows, Linux and macOS.

The program still runs correctly. What changes is its diagnostics. The converted Go runtime reads those symbol
files to answer `runtime.Caller`, `debug.Stack` and a panic's traceback, so frames inside those libraries print
with no file and line, and `runtime.Caller` reports line 0 for them. Frames in the program's own code keep
theirs.

Converting again does not bring the files back when the conversion changes nothing: an unchanged file keeps its
timestamp, so the single file is still not rebuilt.

**To avoid it**, delete the published executable, or the whole publish folder
(`bin/Release/net10.0/publish/<profile>/`), before each publish. The next publish rebuilds the single file
and copies every symbol file back:

```shell
dotnet publish -p:PublishProfile=win-x64
```

The step is needed before every publish, because the next unchanged publish removes the files again. A
publish after a source change rebuilds the single file as well.

## A hand-written project that references the packages fails at startup when published with Native AOT

<!-- Measured 2026-10-06 against go.* 1.24.13.4 from nuget.org by C1 (mailbox bdfb01d50d, d543355364,
     3ae3f16305): a C# consumer of go.lib, go.runtime and go.sort, 31 go.* assemblies in its closure, linux-x64.
     With no trim mode of its own and PublishAot: exit 2 at startup, TypeInitializationException over an
     InvalidOperationException from internal/cpu's initialiser ("... occupies 32 bytes and reports no instance
     fields -- its field metadata was removed, most likely by trimming"). The same with TrimMode=full. With the
     trim mode set to partial in its own project file: the publish ended after 2 h 24 min on a 4-core, 15 GB
     machine (about 70 min inside a hosted CI leg, run 37495989079), the executable ran (exit 0) and was 249 MB.
     WINDOWS (C1, mailbox 079b80b37b; hosted aot-smoke run 37523439849, win-x64): with go.lib's packed default
     of partial the publish succeeded (71 min) and the executable exited 2 at startup with
     "System.InvalidOperationException: There is no metadata token available for the given member"; linux-x64 in
     the same run passed (56 min). golib reads FieldInfo.MetadataToken at four sites to order a struct's fields
     (the two header/slice boxes and GoLibcCall); which one windows reaches is not yet read from a stack.
     A trimmed non-AOT publish (PublishTrimmed, self-contained, no trim mode) ran. A converted project is not
     affected: src/go2cs/csproj-template.xml sets the mode. The fix for the failure is the seat
     claude/c1-golib-trim-default (go.lib's packed targets set the template's default when the consumer set
     none); delete the failure half of this section when a release carries it. The compile time stays until the
     runtime library's reflection is annotated for a full trim (the IL trim warning rows on the BOARD).
     2026-10-07 (COORD): WINDOWS read from a stack by the i9 (ledger 2026-10-06 20:40): the failing site is the
     converted runtime's internCallerFrame, which keyed a call site by its method's metadata token, reached at the
     first runtime.Caller; a converted program that never asks for its caller ran under Native AOT on windows with
     output identical to Go. The fix is the i9's seat claude/i9-aot-metadata-token for the train after TRAIN Q.
     macOS: C1's hosted aot-smoke, the same consumer with the partial trim mode, osx-x64 PASS three times (runs
     37523439849, 37559598601, 37564030683; publish 256 to 293 min); osx-arm64 cancelled twice by the six-hour
     job limit inside the publish (37523439849, 37559598601). -->

A project that go2cs converts sets `TrimMode` to `partial` in its generated project file, so the trimming
failure described here does not occur for it. This is about a C# project you write yourself that references
the `go.*` packages from NuGet and publishes with Native AOT (`PublishAot`).

Such a project gets the .NET SDK's own default for Native AOT, a full trim. A full trim removes field
metadata that the converted Go runtime reads by reflection, and the published executable stops at startup
with a `TypeInitializationException`. Its inner message says that a type "reports no instance fields" and
that its field metadata was removed by trimming. `dotnet run`, a framework-dependent publish, a
self-contained publish and a trimmed publish without Native AOT (`PublishTrimmed`) all run correctly.

**To avoid it**, set the trim mode in your own project file:

```xml
<PropertyGroup>
  <TrimMode>partial</TrimMode>
</PropertyGroup>
```

**This is enough on Linux and on macOS on Intel, and not on Windows.** With the setting, a Native AOT publish
on Linux or on an Intel Mac produces a program that runs. On Windows the publish succeeds, and the program stops
the first time it asks for a caller's frame (`runtime.Caller`; the measured program does so at startup) with a
different error, `There is no metadata token available for the given member`: the runtime library asks
reflection for a value that Native AOT does not provide. A program that never asks for a caller's frame runs.
No setting avoids that error; on Windows, publish without Native AOT. On a Mac with Apple silicon the publish
has not been measured: it does not finish within the six hours a hosted CI job is allowed.

**What that costs.** With `partial`, Native AOT compiles every `go.*` assembly the program references in
full, not only the parts the program uses. For a small program that references about thirty of them, the
publish took a little over an hour on a CI runner and more than two hours on a four-core machine, and the
executable was about 250 MB. A converted project pays the same cost, because it uses the same setting. A
build, `dotnet run` and a publish without Native AOT are not affected.
