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
