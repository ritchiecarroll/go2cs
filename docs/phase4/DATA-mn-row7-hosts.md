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
