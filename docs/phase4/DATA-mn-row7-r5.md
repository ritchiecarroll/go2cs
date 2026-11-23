# Row 7 pieces 4 and 5: R5's hand-owned population and the stdlib packed with twins (record)

**G, 2026-11-22.** COORD's ruling (G inbox, 2026-11-22): pieces 4 and 5 go as one seat, gated by piece 6
(GO2CS0017 = 0 and GO2CS0018 = 0 on all three targets, CNR both ways, the windows `LockOSThread` users), off piece 1's
tip 80d31623ae, with R5 re-read at the tree first, not taken from the list. Branch `claude/g-mn-twins-p45`. Piece 1's
census and seat are DATA-mn-row7-hosts.md.

## R5, as defined (DESIGN-mn-twin-set.md R5, 12.1; P3's finding 6 in 12.6)

A public Go function whose body is hand-written is a bodyless `partial` method declaration in a CONVERTED file. Its
implementing part sits in an `_impl.cs` companion. The generator sees neither part as a member of the population:
the declaration is not a candidate (no body), and the implementing part is not in a converted file. So neither gets a
twin, a forwarder or a GO2CS0017 row. P3 counted 166 public declarations on proto-b1: 127 distinct Go functions
(one per name and arity across the OS folders), in 15 packages, none generic.

## Re-read predicate (stated before the run)

Over each package's Compile items as MSBuild evaluates them per target (windows, linux, darwin), in the projects that
reference go2cs-gen and are not `[assembly: GoHandOwnedPackage]`. A row is a method declaration that:

1. sits in a converted file (the generator's IsConvertedSourceFile);
2. carries `partial` and has no body;
3. is `public`, in types that are public in at least one of their parts (accessibility read across all parts);
4. is not generic, nor in a generic type (E1, counted apart).

Distinct = one per (package, host chain, name, arity). Instrument: `C:\go2cs\scratch\mn2\proto\row7\r5census`
(Roslyn 4.14, parse only). It reuses piece 1's Compile-item lists (hosts-files.txt and hosts-gen.txt, evaluated at
79e753266f). Piece 1 changed file contents only, never the item sets, and the contents are read fresh at this tip.

## Controls (stated before the run)

- **Must be present:** P3's three examples: syscall `Getpagesize` and `Exit`, runtime/debug `WriteHeapDump`.
- **Must be absent:** syscall `runtime_BeforeFork` (not exported).
- **Must equal the row count:** every bodyless `partial` declaration has its implementing part in the package
  (`impl-found` = rows; C# refuses a public partial method without one, CS8795), so a gap is an instrument fault.

## Predictions (before the count prints)

- R5-a: windows reads 127 distinct within 10 (the tree moved since proto-b1: trains 3 and 4), in 15 packages within 2.
- R5-b: internal/runtime/atomic and sync/atomic stay the two largest; together more than half the distinct count.
- R5-c: 0 generic.
- R5-d: linux and darwin differ from windows only in OS-folder packages (syscall, internal/syscall/unix), by under 15.

## R5 reading (2026-11-22, tip ea68ced798)

Per target: windows 109 declarations in 13 packages, linux 118 in 14, darwin 121 in 13; each declaration is distinct
by (package, host, name, arity) within its target. Union across the three targets (P3's definition): **124 distinct
in 15 packages**. internal/runtime/atomic 42, sync/atomic 39, syscall 8, internal/syscall/unix 8, internal/bytealg 8,
then internal/synctest, internal/runtime/sys and crypto/internal/boring/sig 3 each, runtime/debug,
internal/runtime/gc/scan and internal/abi 2 each, and time, internal/runtime/syscall/windows,
internal/runtime/syscall/linux and internal/runtime/startlinetest 1 each. 0 generic. 2 hand-owned packages skipped
(testing, unsafe).

Controls: P3's three examples present (windows: syscall.cs Getpagesize and Exit, garbage.cs WriteHeapDump);
`runtime_BeforeFork` absent. **The implementing-part control fired** on 4 distinct functions (10 rows over the
targets): internal/runtime/gc/scan `ExpandAVX512` and `FilterNilAVX512`, internal/runtime/sys `GetClosurePtr`, and
internal/runtime/syscall/windows `StdCall`. Their implementing parts are not hand-written. They are
`PartialStubGenerator` output (a generated stub for an assembly-only body), so they exist only in a generator's
output. That is a fourth body class, named here: **stub-bodied**. The tool is not at fault.

Scored: R5-a **met** on P3's definition (124 against 127, 15 packages); per target windows reads 109. R5-b **met**
(the two atomic packages are 81 of 124). R5-c **met** (0). R5-d **met**: linux is 9 and darwin 12 over windows, all in
OS-folder packages (syscall, internal/syscall/unix, internal/runtime/syscall/*).

`time.Sleep` is R5 and already has its author-written twin (row 7 piece 3), so R5's own rule covers it: an
author-written twin replaces the forwarder.

## Piece 4: the shape (stated before code)

No per-package list. The declaration itself names the Go function. The generator already normalizes a partial
method to its definition part (TwinGenerator Normalize), so the implementing part's candidate carries the
declaration's symbol. Three changes:

1. **InPopulation reads the declaration's file.** For a partial method, the converted-file test reads the DEFINITION
   part's tree, not the implementing part's. R5 then joins the population, and the existing machinery decides
   every row: a state-machine twin where the hand body parks, else a forwarder (R3), else GO2CS0017.
2. **Stub-bodied declarations become candidates.** A bodyless `partial` definition whose implementation is not in
   the compilation's own trees (a generator's stub) is admitted as a candidate, refused `bodyless` (never twinned:
   there is no body to rewrite), and so gets a forwarder.
3. **An author-written twin satisfies the guard.** A population member refused `has-twin` classifies as
   `author-twin`, not GUARD (R5: the author's twin replaces the forwarder).

GenTests rows, each red first: a hand body that does not park gets a marked forwarder; a hand body that parks gets a
state-machine twin and no GO2CS0017; a stub-bodied declaration (PartialStubGenerator in the driver) gets a forwarder;
an author-written twin gets neither a forwarder nor GO2CS0017.

Predictions (windows, each R5 package built with twins on and GoTwinsGuardWarn=true, so every GUARD row prints):
- P4-a: of windows' 109 declarations, at most 6 classify `twin` (hand bodies that park: internal/synctest's Run and
  Wait among them); 1 `author-twin` (time.Sleep); the rest are forwarders.
- P4-b: GO2CS0017 = 0 over the R5 packages on all three targets, with no new exclusion.
- P4-c: the 4 stub-bodied functions get forwarders on every target where they compile.
- P4-d: no R5 package's plain build moves. Without GoTwins the generator writes nothing, so types, fields and IL are
  identical, read with piece 1's metadata dump.

## Piece 5: COORD's ruling (a) (2026-11-22)

`GoStdlibTwins`, a property in src/core/Directory.Build.props, **default off**. When it is set, every converted
package builds with `GoTwins=true`; golib, hand-owned packages and the `*.tests` projects are excluded. Piece 6's
gates and row 10 build with it on. The default flips, and the pack carries twins, **only on row 10's T1-T5 ruling**
(the owner's 2026-11-21 ruling: stdlib twins are gated on T1-T5). That follows O3: the first twinned pack is
permanent, so nothing twinned ships before the window passes. Option (b), on by default now, was refused. The seat
owes a guard that fails when the property's default reads anything but off, and one DESIGN line naming the flip as
row 10's ruling. That line goes into DESIGN-mn-stdlib-twins.md with the pointer once train 5 has merged it onto this
seat's base.

## Piece 6: the twinned standard library did not compile (2026-11-22)

The whole standard library built with `-p:GoStdlibTwins=true` (go2cs-stdlib.slnx, Release) failed on windows, linux
and darwin alike, with GO2CS0017 = 0, GO2CS0018 = 0 and GO2CS0021 = 0: 5 compile errors in twins, the same 5 on every
target. None of those packages was in stage (c)'s closure. They surface only when every package is twinned, because
only then do their callees park.

**MISDIAGNOSIS, as first posted (kept, so the correction reads against it).** I posted the four CS9217 errors as "a
foreach over `range(...)` (ref iteration) whose body now parks, so the twin awaits inside a ref-bound loop", and
proposed iterating by value. That was wrong. Nothing iterates by ref; the range value is already a copy.

**Correction, from the emitted twins.** Three distinct shapes, each a twin-rewriter gap:

1. **Go's two-variable range is a DECONSTRUCTING foreach** (`foreach (var (k, v) in range(…))`), which Roslyn parses
   as `ForEachVariableStatementSyntax`, not `ForEachStatementSyntax`. Rule H (a body entered after its header
   awaited starts with the live ref locals derived again) existed for the one-variable form only, so the
   deconstructing body read a stale ref: internal/diff `Diff` (`@out`), net/http `send` (`req`), text/template
   `walkRange`'s Seq2 loop (`r`; its one-variable loop beside it was served). CS9217 x3.
2. **An await in a for statement's iterator** was parenthesized, `(await Ꮡp.nextꞏtwin())`, which is not a statement
   expression (go/parser, CS0201), and the condition read the ref local `p` after it.
3. **An `else if` whose condition awaits and then reads a ref local in the same condition**: archive/zip
   `checksumReader.Read` (`(await …Sum32ꞏcall(r.hash)) != r.f.CRC32`), and go/types' conversion
   (`… || !isTypes2 && x.isNil()`), which compiled only once go/parser did. RefReadAfter counted only reads whose
   nearest statement is the statement in hand, and an `else if` is a nested IfStatement. So the read was missed and
   the call was awaited in place: CS9217. My first post grouped archive/zip under shape 1; it is shape 3.

**Cures (COORD accepted 1 and 2; 3 found on the re-build):**

1. `CommonForEachStatementSyntax` is treated alike in the three places that named only the one-variable form: a
   `VisitForEachVariableStatement` with Rule H, RefAliases' loop list (liveness), and the sequence-point mapping.
2. An await that IS a for statement's iterator stays bare, and the iterator list continues with the live ref locals
   derived again: `for (; p.tok != token.EOF; await Ꮡp.nextꞏtwin(), p = ref Ꮡp.DerefOrNull())`. The iterator stays
   the loop's own, so `continue` and a labeled `goto continue_x` run it as in the plain method. Nothing is lowered.
3. RefReadAfter also counts a read in an `else if` of the chain when that else-if's own condition holds both the
   call and the read. The call then takes the existing path for a statement that cannot spill: it stays a plain,
   blocking call (`ref-blocked`, stage (c)'s stated cost; that path hands its P off as a thread park does). The
   standard library already carries 295 ref-blocked call sites in 55 packages (windows). A non-blocking form, the
   read re-derived inline through its box, would unblock all of them. It is a behavior change of its own, so it is
   proposed as a follow-up, not folded in.

GenTests `TwinLoopTests`, 5 rows, red first (the base compiled none of them: CS9217 x5, CS0201 x2). Each EXECUTES the
twin and compares it with the plain method. (1) A two-variable range whose header awaits writes through the
re-derived ref, and the write is read back after the loop, with a one-variable control beside it. (2a) A parking
iterator whose condition reads the ref after the park, with a `continue`. (2b) A labeled continue from an inner loop,
as the converter spells it. (3) An else-if whose condition reads the ref after a parking call. A stale ref cannot
"lose a write silently" in C#: the compiler refuses a ref local held across an await (CS9217). The compile is that
guard, and the executed rows show the semantics equal the plain method's.

## Piece 6 gates, after the cures (2026-11-22, tree 4959435e78 plus the seat's edits)

Kit `C:\go2cs\scratch\mn2\proto\row7`: p6-gates.sh, p6-census.sh, p6-supp.sh, p6-supp2.sh, p6-supp3.sh.

**The whole standard library twinned** (go2cs-stdlib.slnx, Release, `-p:GoStdlibTwins=true`): rc=0 with 0 errors and
GO2CS0017 = 0, GO2CS0018 = 0, GO2CS0021 = 0 on windows (506 s), linux (554 s) and darwin (548 s). Every project
compiled, net/http's dependents included, so the zeros are a reading, not a floor. 358 twins reports from 364
projects; the 6 without one are named: go2cs-gen itself, golib, go.arrays and go2cs.CpuProfiler (no twin generator),
and testing and unsafe (hand-owned, skipped).

**The parker census as a fixed point** (COORD's (3)). A full build compiles every package against its dependencies'
final twinned assemblies, in dependency order with no cycles, so one build is the fixed point. A second windows build
read 0 of 358 reports differing, so nothing new appears. Against piece 1 and piece 4:
- host-not-partial refusals that park, in packages that are not hand-owned: **0** on all three targets.
- piece 1's hosts: runtime's parkers in managed_impl.cs are the same set as the piece 1 seat read (PoolCleanupCadence,
  Sysmon and the rest twinned; gcCycle refused await-in-lock); internal/sync's hookedHash.GetHashCode still refused
  `instance`.
- R5: windows 108 forwarders + 1 author-twin (109), linux 117 + 1 (118), darwin 120 + 1 (121). **0 changed** against
  piece 4's read over plain dependencies.
- The one delta is hand-owned. Under the twinned standard library, testing's `TestLogBridge` (a non-partial static
  class) parks in 5 methods: Open, Close, WriteLocked, FlushLocked and ExitIfFailed. Its callees are twinned now.
  Read with the hand-owned skip removed, over testing's whole twinned closure: GO2CS0021 names exactly those 5; with
  the skip, testing builds clean. **Disclosed (COORD's (4)):** testing is excluded from twinned standard-library
  builds by the skip, and a test host enters plain (TestHost.Run), so these methods run plain and block their
  carrier wherever a twin reaches them. Today nothing does. Making TestLogBridge partial is **not owed now**: its
  twins could not be reached. It becomes owed if testing is ever built with twins, and GO2CS0021 then names it
  without anyone having to remember. (Two earlier reads of this were void, and are recorded as such. One used a stale
  analyzer, through --no-dependencies. The other ran over dependencies that the plain-identity step had rebuilt plain.)

**The windows LockOSThread users.** Under the twinned standard library three of the four are state-machine twins:
internal/syscall/windows `withPrivilege` (7 sites), net `lookupProtocol` (8) and os/user `runAsProcessOwner` (2).
internal/syscall/windows/registry `ReadSubKeyNames` does not park and stays plain. A scratch probe (ResolveIPAddr
over "ip4:ggp", which takes lookupProtocol's locked goroutine, and user.Current), twinned against the twinned
standard library, runs EQUAL to Go under GO2CS_MN=1 with off-carrier-locks=0 and pins=0. That goroutine never parks
while locked: its getprotobyname is a plain syscall, and its send is buffered. So the pin is not stressed by the
standard library's own users. MnLockPin is, twinned against the twinned standard library: output identical to its
baseline, pins=10, off-carrier-locks=0, over 206,037 parks.

**CNR both ways.** Seven programs, each in three builds: plain against the plain standard library (the baseline);
plain against the twinned standard library; and twinned (GoTwinsFor) against it, run with GO2CS_MN unset and set.
Every run is identical to its baseline, and EQUAL to Go where a Go source exists. The programs: MnSpinCap, MnContended,
MnForeignThread, MnMixedStorm (n=1,000), MnLockPin (no Go source: compared with its baseline), MnMutexTwin and
MnBlockedIO. MnLockSleep and MnLockFair are not on this base (they came with row 7's train-5 branch); their first rows
were void and are not counted.

**Plain build unchanged** (P4-d **met**): the 13 windows R5 packages built plain with the seat's generator and props,
and with those files at 80d31623ae. Every type, flag, attribute, field and method IL hash is identical; only the module
identity moves.

**Made to fail**, each restored byte-identical:
- piece 4: N4a, N4b and N4c each red their TwinR5Tests rows;
- piece 5: N5a (default true) and N5b (a tracked script turns it on) red TestStdlibTwinsDefaultOff; N5c is the
  TestLogBridge read above;
- piece 6, each with its own error code: N6a (no Rule H for the deconstructing form) CS9217; N6b (the iterator await
  parenthesized) CS0201; N6c (no re-derivation in the iterator list) CS9217; N6d (no else-if chain) CS9217.

**Suites:** GenTests 774/774; GolibTests Release 3,111 passed (53 skipped), Debug 3,085 passed (79 skipped); the Q1
kit 22 builds, 14 twin runs, 0 differing; repoguard rc=0.

**Scored:** P4-a met (0 twins among R5: every hand body that is not author-twinned forwards); P4-b met, and over the
whole standard library on all three targets; P4-c met; P4-d met.

**Proposed follow-up (not in this seat):** re-derive a ref read inline through its box where a statement cannot spill.
That would unblock the 295 ref-blocked call sites in 55 packages (windows) that today stay plain, shape 3's two among
them.
