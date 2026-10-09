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

## A Native AOT publish of a program that references the packages is slow and large

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
     job limit inside the publish (37523439849, 37559598601).
     2026-10-09 (R, for the 1.24.13.5 release): the FAILURE half of this entry is deleted. Both fixes are in
     master 56f0f1f254: the trim default is ab6aa8f443 (go.lib's packed targets default TrimMode=partial; TRAIN Q,
     merge be7b209245), and the Windows failure is a0cafdb588 (no metadata token is read on any path a Native AOT
     program takes; TRAIN FL row 11, merge 3db77316b1). Hosted aot-smoke 37870614219 at 56f0f1f254: linux-x64,
     win-x64 and osx-x64 pass, osx-arm64 not measured. The full-trim reading is G's, from its logs: win-x64,
     Windows 11, .NET SDK 10.0.400, one development machine, exit 0 with 0 refused types (ledger 2026-10-08
     19:36). The section "Publishing a converted program twice loses file and line numbers in stack traces" was
     deleted in the same commit: its fix is a395442f7b (dependency symbol files stay beside a single-file host
     on every publish), in master through the merge 4d9046eb70. -->

A program that references the `go.*` packages and publishes with Native AOT (`PublishAot`) runs. The check
that gates each release publishes a C# consumer of the packages and runs it on `linux-x64`, `win-x64` and
`osx-x64`. A project that sets no trim mode of its own gets the packages' default, `partial`, which is the
setting a converted project carries in its generated project file. On a Mac with Apple silicon the publish has
not been measured: it does not finish within the six hours a hosted CI job is allowed.

A full trim (`TrimMode` set to `full`) produces a program that runs as well. That is measured on `win-x64`, on
one development machine; the hosted check measures the default.

**What it costs.** With `partial`, Native AOT compiles every `go.*` assembly the program references in
full, not only the parts the program uses. For a small program that references about thirty of them, the
publish took a little over an hour on a CI runner for Linux and for Windows, several hours for an Intel Mac,
and more than two hours on a four-core machine, and the
executable was about 250 MB. A converted project pays the same cost, because it uses the same setting. A
build, `dotnet run` and a publish without Native AOT are not affected.
