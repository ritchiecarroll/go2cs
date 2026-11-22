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
