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
