# Row 7 piece 1: the host-not-partial census (record)

**G, 2026-11-22.** COORD's ruling (G inbox, 2026-11-22: row 7 piece 1 GO NOW) asks for a census of the hand-owned hosts
the twin generator refuses as `host-not-partial`, on all three targets, read at the tree, with the population predicate
and a must-be-zero control stated before the count prints. Branch `claude/g-mn-twins-p1` off train 4's tip afb01dfb6c.
Piece 1 itself (making those hosts `partial`) is DESIGN-mn-stdlib-twins.md section 1, row 1; that file is on the seated
row-7 branch, not yet on this base, so this record holds the census and a pointer is added there once train 5 merges.

## Population predicate (stated before the run)

The generator's own rule, read syntactically over each package's Compile items as MSBuild evaluates them per target
(`dotnet msbuild <pkg>.csproj -getItem:Compile -p:GoTargetOS=<os>`), for every `src/core` package project except golib
and the `*.tests` projects. A **method** counts when, in the generator's order (TwinGenerator.cs AddCandidate, then
PreRefusal):

1. it is a candidate: it has a body (or is `extern`), sits directly in a type declaration, is not `async`, and its
   name does not end in the twin suffix;
2. it is not refused first as `native` (extern, DllImport or LibraryImport, a `fixed` statement or an `unsafe` block)
   nor as `has-twin` (a sibling `<name>ꞏtwin` with the same parameter count);
3. some enclosing type declaration lacks `partial`.

An **interface member** counts when it is bodyless and the interface or an enclosing type lacks `partial`
(TwinGenerator.Iface.cs CollectIface). A **host** is the type chain such a member sits in.

Instrument: `C:\go2cs\scratch\mn2\proto\row7\hostcensus` (Roslyn 4.14, the generator's version, parse only) driven by
`hosts-lists.sh`. Syntax, not symbols: `has-twin` checks one declaration, which is exact for a non-partial host (it has
one declaration).

## Controls (stated before the run)

- **Must be zero:** `host-not-partial` methods and interface members in CONVERTED files (the generator's
  IsConvertedSourceFile: not `_impl.cs`, not `[module: GoManualConversion]`, not a package metadata file). The converter
  writes every type `partial`, so any count there is an instrument fault, not a host.
- **Must be present (known positives):** runtime's `Sysmon` host (named in TwinGenerator.cs as the example) and
  runtime's `IGoCpuSampler` interface (named in TwinGenerator.Iface.cs).
- **Evaluation:** every project evaluates on every target (EVAL-FAIL = 0), or the missing package is named.

## Predictions (before the count prints)

- H1: the converted-file count is 0 on every target, and both known positives appear.
- H2: windows reads at least stage (c)'s closure count (94 methods, 2 interface members): the whole stdlib is a
  superset of one closure's packages. Upper bound: under 600 methods on any target.
- H3: the hosts are few relative to the methods: under 80 type chains on any target, concentrated in runtime.
- H4: the three targets differ by under 15% in method count (the hand-owned files are mostly target-neutral).

## Reading (2026-11-22, tree 79e753266f)

Kit: `hosts-lists.sh` (Compile items per target, then hostcensus), `hosts-analyzers.sh` (which projects reference
go2cs-gen), `hosts-xcheck.sh` and `hosts-xcheck2.sh` (each named package built with twins on, the generator's own
report read). 362 projects evaluated on each target, EVAL-FAIL 0.

**The must-be-zero control fired first, and named an instrument fault.** The first count read 20 methods and 1
interface member in CONVERTED files on every target. All 21 sit in two projects that are not Go packages and do not
reference go2cs-gen: `go.arrays` (InlineArrays.cs) and `go2cs.CpuProfiler` (EventPipeSampler.cs, DarwinThreadCpu.cs).
The TwinGenerator never runs there. The predicate is amended to "projects that reference go2cs-gen.csproj": 360 of 362
on every target, the same two out. With it, the converted-file count is **0** on all three targets, and those two
projects contributed no hand-owned rows. Both known positives are present (runtime `Sysmon`, `IGoCpuSampler`).

**Instrument against the generator** (windows, every package with a row, twins on, the report's own refusal line):

| package | census | generator `host-not-partial` | of which park (`blocked`) | interface members refused |
|---|---|---|---|---|
| testing | 251 | 251 | 11 | 23 (`TB`) |
| runtime | 56 | 56 | 2 | 2 (`IGoCpuSampler`) |
| unsafe | 51 | 51 | 0 | 0 |
| syscall (windows only) | 16 | 16 | 0 | 0 |
| sync | 9 | 9 | 0 | 0 |
| reflect | 6 | 6 | 0 | 1 (`ISpanEquality`) |
| weak, internal/sync, internal/godebug | 2, 2, 1 | 2, 2, 1 | 0, 1, 0 | 0 |
| **total, windows** | **394** | **394** | **14** | **26** |

Linux and darwin: 378 methods (syscall's 16 are windows files), 26 interface members. Hosts (type chains): windows 54,
linux and darwin 47; by package testing 19, runtime 14, syscall 7 (windows), reflect 5, unsafe 4, sync 2, and one each
in weak, internal/sync and internal/godebug. The parking count was read on windows only.

The 14 that park, by host: testing `TestExecution` (RunSynctest, ExecuteSynctest, Context, Execute, RunCleanupsLoop),
`TestRunner` (Printf, Write, WaitForSerialBoundary, WaitBoundedAtSerialBoundary), `TestRunner.ChattyPrinter`
(Updatef), `TestOptions` (Matches); runtime `PoolCleanupCadence.AgeForCollection`, `Sysmon.Tick`; internal/sync
`hookedHash.GetHashCode`. Several park only through a call through a func value (the generator's conservative rule;
GO2CS0014 names Context, Execute, RunCleanupsLoop, Matches and Updatef), and `GetHashCode` is reached through
`IEqualityComparer`, so its twin would never be called.

**Scored:**
- H1 **met after the amendment**: the control fired at 21 in every target, as an instrument fault (two non-package
  projects in the population), not as a host. With the predicate gated on go2cs-gen, 0, and both positives present.
- H2 **met**: 394 (windows) and 378 (linux, darwin), above the closure's 94 and under 600.
- H3 **half met**: 54 and 47 type chains, under 80, but concentrated in **testing** (251 methods, 19 hosts), not
  runtime.
- H4 **met**: windows is 4% over linux and darwin; the difference is syscall's windows files.

## What piece 1 buys, and the plan for pieces 1 and 4-6 (for COORD's ruling)

Hand-owned methods are never in the forwarder population (InPopulation requires a converted file), and a method gets
a twin only when it parks. So making a host partial changes the build only where a member parks: 14 methods in 7
type chains (testing 4, runtime 2, internal/sync 1), plus whichever interfaces get twin slots. Making all 54 hosts
partial writes the same twins, and edits 47 more hand-owned type declarations for nothing measurable today.

| piece | shape | size (lane-days) |
|---|---|---|
| 1 | (B, recommended) the 7 parking hosts made `partial`, plus a guard: a PARKING member of a non-partial host becomes a named diagnostic, so a future parker cannot stay plain silently. Gates: the census re-read (parkers 0 refused), GolibTests R/D, a twinned `-tests` row on a package whose tests use `t.Run` (testing's own twins run there). (A), all 54 hosts: 1, and no guard needed for today's hosts, but still needed for new ones | 0.5-1 |
| 4 | R5's hand-owned population re-read at the tree first (the 127 was dated), then a forwarder per public Go function from a per-package list | 1-1.5 |
| 5 | `-stdlib` packs every converted package with `GoTwins=true` | 0.5 |
| 6 | GO2CS0017 = 0 and GO2CS0018 = 0 over the whole stdlib build, on all three targets; CNR both ways; the 4 windows `LockOSThread` users under the pin | 1-2 |

Interfaces: `TB` (23 members) is the big one. Under the hidden all-members policy, making it partial gives every member
a twin slot. That is a cost with no parker to justify it today, so interfaces stay as they are unless a member parks.

## Piece 1 seated as ruled (B) (2026-11-22, G)

COORD's ruling: (B). The 7 hosts with a parking member are made `partial`, with all 14 members, internal/sync's
`hookedHash` included. The guard is **GO2CS0021**, an error: a member that parks in a host (or an enclosing type)
that is not `partial`, in any assembly built with twins, hand-owned or not. Its predicate is exactly that, with no
exception list. GenTests' `AMethodOfANonPartialHostIsRefusedByNameAndCalledPlain` now also asserts that GO2CS0021
names `Loop` and its host `Ticker`, once, and does not name the partial host beside it. docs/DiagnosticCodes.md lists
the code. (GO2CS0019 is held by the N1 design and GO2CS0020 by the func-value seat, so this takes 0021.)

**What partial bought: 4 twins, not 14.** With each host `partial`, a later refusal in the generator's order applies:
a twin is written only for a static method, and 10 of the 14 parkers are instance methods (`instance`, outside R1:
hand-written C# only). Twinned now: testing `TestOptions.Matches` and `TestRunner.WaitForSerialBoundary` (both
static), runtime `PoolCleanupCadence.AgeForCollection` and `Sysmon.Tick` (and `Sysmon.Loop`, which calls Tick).
Refused `instance`, still plain: testing's other 9 parkers (in `TestExecution`, `TestRunner`, `ChattyPrinter`), plus
`hookedHash.GetHashCode`. The guard stays satisfied either way: once a host is partial, its member is no longer
refused as `host-not-partial`. The record's earlier "14 methods" was the number the guard governs, not a twin count.

**Correction to the plan row above.** The piece 1 gate row proposed "a twinned `-tests` row on a package whose tests
use `t.Run`". It would prove nothing. A converted test host's entry is plain (go2cs_test_host.cs registers plain
delegates and calls `TestHost.Run`), so no `-tests` program reaches testing's twins. They are reached only from a
twinned caller. The gate is replaced by the twinned builds below.

**Gates** (kit `C:\go2cs\scratch\mn2\proto\row7`: p1-gates.sh, p1-ident.sh, p1-mdident.sh, mddump):
- Twinned builds of every census package on windows, linux and darwin (27 builds): all rc=0, GO2CS0021 = 0, and the
  report's `host-not-partial` blocked = 0 on every one. Refusals now: testing 133, runtime 49 (were 251 and 56).
- Made to fail, each restored byte-identical. N1 (COORD's): runtime's `Sysmon` without `partial` fails the twinned
  runtime build: "'go.runtime_package.Sysmon.Tick()' parks but gets no twin: its host 'Sysmon' is not partial".
  N2: the guard's report removed reds exactly that GenTests row.
- Plain build (no twins): runtime, testing and internal/sync built with the edits and with the 7 words reverted. Every
  type, flag, type attribute, field and method IL hash is identical (39,931, 1,247 and 536 dump lines). Only the
  module identity moves, as with any source edit (MVID and the PDB id: the PDB records the source's checksums). The
  dump's own negative: sync plain against sync twinned differs on 1,293 lines.
- GenTests 764/764; GolibTests Release 3,111 passed (53 skipped), Debug 3,085 passed (79 skipped); the Q1 kit 22
  builds, 14 twin runs, 0 differing, GO2CS0021 = 0; repoguard rc=0.
