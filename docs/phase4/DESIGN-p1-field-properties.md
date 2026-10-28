# DESIGN — P1: `p.f` through generated field properties (1.26.10.2 seat #31)

> **STATUS: DRAFT (G, 2026-10-28), for COORD's ruling before any code.** Document type: a design record for one seat
> of [the parity train](../PLAN-parity-train.md) (§1, seat #31), and for the one #32 carry-over below. It answers
> that seat's five design questions. Q1 was measured and ruled on 2026-10-27; Q2-Q5 and the carry-over are this
> note's proposals. It lands before the seat and is amended, with dated blocks, from what the seat measures.

## What changes

Today a Go selector through a pointer to a struct converts to one of two spellings:
- a read is `(~p).f`, which copies the whole struct;
- a write is `p.Value.f`.

P1 makes both `p.f`, as Go writes it. go2cs-gen already emits `ref`-returning C# 14 extension properties on `ж<T>` for
the struct types a file's `[GoFieldAccess(typeof(T), …)]` record lists (`FieldAccessGenerator`, today used for the
`unsafe.Pointer` class only). P1 widens that mechanism to every Go selector through a pointer to a struct: about
27.6K sites across production and tests, about 220 goldens, and one record line per affected file (about 900 files).

## Q1 — the property body and its cost (MEASURED; RULED by COORD 2026-10-27)

The micro times one read-modify-write of two fields of a 4-word struct, 20 M iterations, median of 7, every arm's
checksum equal (win-x64, G-LAPTOP, golib at master 5931bad29b). Figures are ns per iteration:

| receiver, mode | `p.Value.f` | `(~p).f` | today's body | specialized body | hoisted `ref var` |
|:--|--:|--:|--:|--:|--:|
| heap box, Native AOT | 11.4 | 11.0 | 30.1 | 12.7 | 0.7 |
| heap box, JIT TieredCompilation=0 | 11.4 | 7.2 | 19.2 | 12.2 | 0.7 |
| heap box, JIT default (PGO) | 1.0 | 3.7 | 12.4 | 6.9 | 0.7 |
| field view, Native AOT | 25.6 | 31.5 | 41.0 | 26.1 | 0.7 |

Rulings:
- **The specialized body is required.** Today's body, `ref ʗp.field(T.Ꮡf)`, is a type test plus an accessor-delegate
  call: 1.6-2.7x `p.Value.f` on a heap box. P1 emits instead
  `ref ʗp is NativeBox<T> ? ref ʗp.field(T.Ꮡf) : ref ʗp.Value.f`.
- **The native arm goes through the public `ʗp.field(T.Ꮡf)`.** `NativeBox<T>.FieldRef` stays internal.
- **The gate reads real programs, not the micro alone:** RefLower, FieldView and Iface, under the JIT's default tiering
  and under Native AOT. Under PGO the JIT can devirtualize `p.Value` in a plain loop, and the same expression inside an
  inlined extension getter loses that guard (5.8 against 1.0 in the micro). The micro moves into the seat as its perf
  guard.
- **The `ж<T>.Value` fast path is its own seat, #35, landing first.** StandardBox's virtual `IsNull` was half the
  per-access cost under AOT and TC0; #35 reads it as fields (heap box under AOT: 11.3 -> 7.3 ns). #31 measures on top.

## Q2 — semantics (PROPOSAL)

- **A managed box:** the property is `ref ʗp.Value.f`, the same storage `p.Value.f` reaches today. A read through it
  equals today's `(~p).f` without copying the whole struct. A nil pointer panics at the same site with the same text:
  the nil check is `Value`'s, unchanged.
- **A native box:** the property answers the field at its GO offset (`NativeBox.FieldRef`), where today's `p.Value.f`
  answers the CLR slot. For a struct whose CLR layout is not Go's (a managed reference or a value embed inside), the
  CLR slot holds the wrong bytes, and `Value` refuses the whole-value read outright. So this is a correctness gain,
  stated in the seat and guarded by `PointerFieldSelectors`' native-backed read.
- **Evaluation order is unchanged.** A `ref` read happens at the operand's own evaluation, as the copy did. Parallel
  assignment keeps the converter's existing temporaries (ParallelAssignmentHazard is a red-first arm).
- **Copy semantics for a struct-typed field:** `x := p.f` copies (`var x = p.f` copies a `ref` property's value), and
  `&p.f` stays `p.of(T.Ꮡf)` until #32.

## Q3 — the record's visibility (PROPOSAL: per FILE, as today)

**Keep one visible `[GoFieldAccess(…)]` line per file**, the mechanism go2cs-gen, GenTests and the converter share
today. The reasons:
- **A test assembly that recompiles production files sees each file's record with it.** 3 of 242 converted test
  projects recompile production files today (`crypto/sha3`: sha3.cs; `net/netip`: netip.cs, uint128.cs;
  `text/tabwriter`: tabwriter.cs). A per-package record in `package_info.cs` would not reach them, since a test
  project compiles its own package-info files. Their properties would vanish and those builds fail (CS1061).
- **The record keys the gen output per file**, so a file the end regen has not yet re-emitted carries no record and
  gets no property: the OLD corpus compiles unchanged through the deferred-corpus window (§3.1).
- **Each rung's footprint stays per file and attributable** (§3.2): a file's record changes with that file.

The cost is about 900 visible attribute lines, one per affected file, beside the existing `using` block. A per-package
record would need the record mirrored into each test variant's package-info, and that is new machinery for a
cosmetic gain. Owner FYI: the line is visible in each converted file.

## Q4 — collisions, shadowing and frames (PROPOSAL)

- **The collision list stands** (`fieldAccessCollisions.txt`: 32 names, every public `ж<T>` member and golib extension
  member on `ж<…>`). A Go field with one of those names keeps the explicit spelling, and GenTests'
  `FieldAccessCollisionGuardTests` fails when `ж<T>` gains a member the list lacks.
- **Promoted-method shadowing:** go2cs-gen emits no property for a field whose name is also an extension member on
  `ж<T>` visible in the using compilation (a converted pointer-receiver method or a promoted forwarder). The converter
  keeps today's spelling for such a field. The guard is `PointerFieldSelectors`' `Len` arm.
- **Hand-owned structs keep today's spelling.** A `[module: GoManualConversion]` file's types are not listed in any
  record.
- **Frames.** The getters are `[AggressiveInlining]`, so at Release no getter frame exists, and `runtime.Caller` and
  tracebacks see the user's frame. Where a getter is NOT inlined (a Debug build, or a refused inline), its frame has no
  `GoPositionMap` and would print with its C# position in a nil-pointer traceback. Proposal: go2cs-gen marks the
  `GoFieldAccess` class's getters as go2cs machinery (the `GoTwinForwarder` treatment: no Go frame), so the frame walk
  skips them whatever the JIT decides. The red-first arm is a nil-pointer panic through `p.f` in a Debug build, with
  its traceback compared to Go's.

## Q5 — build time (PROPOSAL: measured in the seat)

Each using compilation gets one extension block per recorded type (about 900 files' worth). The seat measures a clean
`dotnet build` of the three largest consumers by site count (from the seat's census) and of the full stdlib solution,
base against the rung, on G-LAPTOP, one at a time. Gate: under +5% wall on the stdlib solution, otherwise COORD
rules. The gen's own time is read from `-p:ReportAnalyzer=true` on the largest package.

## #32's carry-over: one accessor identity per field (PROPOSAL)

R's FieldView root cause (2026-10-27): golib's view cache (`ж.Views.cs` `sameAccessor`) reads `Delegate.Method` on every
mismatch since 4ad8e8df66, the token comparison of [the view cache's](DESIGN-field-view-cache.md) 2026-10-21 amendment.
It needs that comparison because every `&p.f` site converts the accessor method group to its own cached delegate, so
one field reaches a box's view list as several delegate instances. Under Native AOT `Delegate.Method` is an uncached
reflection lookup of about 0.9 µs; reverting only that compare takes AOT FieldView from 95 s to 4.8 s. R's seat #36
fixes it tactically in golib (batch 74).
- **The structural fix rides #32.** #32 respells every `&p.f` (`p.of(T.Ꮡf)`) as the generated `p.Ꮡf` property. That
  property passes ONE delegate instance per field: a `static readonly FieldRefFunc<T, F>` that go2cs-gen emits on `T`
  itself, in `T`'s own assembly, beside the `Ꮡf` accessor method. Every site in every assembly then hands the view cache
  the same object, and each cache compare is `ReferenceEquals` on both runtimes.
- **The identity must live on `T`, not in the using compilation's `GoFieldAccess` class.** That class is internal and
  per assembly, so two packages would hold two delegates for one field.
- **#36 remains the fallback** for any site the token does not reach: hand-owned code, reflect's field boxes and a
  method-group conversion the converter emits elsewhere.
- **Gate:** PerfFieldView, JIT and Native AOT, no worse than #36's tip, plus a GolibTests arm that two packages'
  `&s.f` views of one struct share one cache entry.

## Gates and order

- **Order:** #25 (parameter names) -> #35 (the `Value` fast path) -> #31 -> #32 (`b.Ꮡoff`, the same extension block).
- **Red-first:**
  - behavioral `PointerFieldSelectors` (new in this seat; the Q2 and Q4 guards above are its arms): a field named
    `Value`, the `Len` shadow, a generic struct, the chain `e.prev.next.x = …`, a parallel swap and a native-backed
    read, all in the Output phase;
  - GenTests arms for the record forms and the shadow rule;
  - the `FieldAccessReinterpret` golden unchanged;
  - the Debug-build traceback arm (Q4).
- **Perf:** the micro as a guard; RefLower, FieldView and Iface under JIT default tiering and Native AOT (the i9's AOT
  leg), no worse than the base beyond the null.
- **Footprint:** the seat's own two-seeded three-target `-stdlib` measurement plus its `-tests` leg, attributed per
  file (§3.2).
