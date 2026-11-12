# DATA: the cost of `==` on two pointers to struct fields (C51, P2, 2026-11-11)

> **A point-in-time record.** C51 started from a TypeScript compiler test that did not finish within 59 times native's
> 20 s. A trace of the converted run put the checking thread entirely in `checker.getBaseTypes` and the `hasBaseType`
> closure, whose `target == checkBase` compares two `*Type`. This record measures that compare, names its cause, and
> scores the fix the design post predicted. Measured on linux-x64 (4 vCPU, Xeon at 2.1 GHz), go1.26.10, .NET SDK
> 10.0.403, runtime 10.0.14. The base is `claude/c2-c49-one-nil` at `ff19c516af`, C49's one-nil rule, accepted for
> batch 286.

## 1. Why a `*Type` compare is costly

- **A TypeScript `*Type` is a field pointer.** Every type's data struct embeds `TypeBase`, which embeds `Type`, and
  `AsType()` returns `&t.Type`. A `TypeReference` reaches `Type` through five embedded structs (`ObjectType`,
  `StructuredType`, `ConstrainedType`, `TypeBase`, `Type`). go2cs converts that to a chain of `FieldRefBox` views, one
  per hop, over a `StandardBox` root.
- **The repro has the same shape.** [`tsshape.go`](DATA-c51-field-view-equality-data/tsshape.go.txt) declares those
  structs and the `AsType` method. A probe of its converted `*Type` read the chain hop by hop: `FieldRefBox` over `Type`,
  `TypeBase`, `ConstrainedType`, `StructuredType` and `ObjectType`, then `StandardBox<TypeReference>` as the root. Every
  hop's order token is exact. Those are the trace's `FieldRefBox<T>` leaves.
- **An equal compare is cheap.** `AsType()` returns the cached view, so the same pointer is the same box, and
  `FieldRefBox.Equals` answers it with `ReferenceEquals`.
- **An unequal compare is quadratic in depth.** At each hop, `Equals` ran three steps:
  - the native arm, which walks to the root through interface tests;
  - `SameSource`, which calls the two sources' `Equals` one hop down, so the walk recurses;
  - `SameGoAddress`, which walks the chain again: `OrderTokenIsExact` takes one `GoFieldOffsetOf` weak-table lookup
    per hop, and `AllocationIdOf` walks once more.

  The trace's leaves are this work: `IsInstanceOfInterface` about 21%, `GoFieldOffsetOf` about 35%,
  `FieldRefBox.Equals` about 15%, `IsNativeRooted`, `AllocationIdOf` and `OrderTokenIsExact` about 4%.

## 2. Hang or constant factor (the TS session's count, relayed by COORD, unit 05056)

The search is the same; the calls are slower. The two counts:

- **Native:** 1,552,219,830 `getBaseTypes` calls over the full unit in about 21 s, about 74M calls per second. The
  counter was a `go build -overlay`, and the output stayed byte-identical to the plain build's.
- **Converted:** the same counter in the emitted `getBaseTypes` read a steady 674k calls per second, about 50M after
  150 s.

The per-call cost is about 110 times native's. C51 is a performance row, not a memoization divergence.

## 3. The microbench

**Method.** Native timings come from `go test -bench` of
[`tsshape_test.go`](DATA-c51-field-view-equality-data/tsshape_test.go.txt). The converted timings come from the same
package converted with this tree's go2cs and driven by
[`bench-Program.cs`](DATA-c51-field-view-equality-data/bench-Program.cs.txt) through the converted `CountEq`. Both
sides run one loop, `a[i] == b[(i+shift)%n]` over 64 pointers. Each figure is in nanoseconds per compare, the loop
included.

**Paired runs.** The base and fix binaries differ only in `golib.dll`. They ran alternately in three processes each,
and each process reported the minimum of 5 reps. The table gives the minimum over the three processes.

| Pair | Go | Base | Fix |
|---|---|---|---|
| int compare (the loop alone) | 0.69 | 3.16 | 3.06 |
| two heap boxes, unequal | 3.45 | 6.48 | 6.13 |
| the same heap box | n/a | 6.66 | 6.12 |
| field view, unequal, 1 hop | 3.45 | 28.73 | 5.94 |
| field view, unequal, 5 hops (the TS shape) | 3.42 | 716.52 | 6.03 |
| the same 5-hop view | 3.39 | 5.83 | 5.65 |
| 5-hop views of two allocations | n/a | 664.26 | 5.67 |
| 5-hop view against a heap box | 3.37 | 12.80 | 5.93 |
| heap box against a 5-hop view | n/a | 6.98 | 7.09 |

At the base, five hops cost 24 times one hop for five times the depth. Every untouched arm reads within the
spread of the base's own processes (two heap boxes, unequal, read 6.48 to 11.29 across them).

**Allocation.** Building the pointers allocated 224 B per one-hop element and 792 B per five-hop element, the same at
base and fix. A compare allocates nothing on either.

## 4. The fix and its predictions

**The fix, in `FieldRefBox`.** Each view caches one byte (`m_identityFacts`, in `ж<T>`'s existing padding beside
`m_id`). The byte is computed once from the view's immutable source and field. Its bits:

- native-rooted;
- order token exact;
- **MANAGED-EXACT**: every hop down to the root is a field view with a resolved Go offset (a reinterpret hop counts as
  0), and the root is a non-nil `StandardBox` that does not name the zerobase.

Two MANAGED-EXACT views are equal exactly when their cached order tokens match (root id over Go offset) and their
`ReferentObject` is the same root. A MANAGED-EXACT view is unequal to every other kind. Any other pair keeps every
existing arm. Those arms now read the cached bits, so they are linear in depth.

**What stays exact.** `operator==` and C49's one-nil rule are unchanged; a field view is never nil.

**The one intended difference.** Allocation ids are 29 bits and wrap. Before the fix, two views of distinct roots
whose ids collided compared equal through `SameGoAddress`. The root check now keeps them distinct, as Go does.

**Predictions** (posted before the cut; [scored](#5-scores-and-gates) below):

- the field view, unequal, at 1 and 5 hops, at most 12 ns;
- the 5-hop view against a heap box, at most 10 ns;
- every heap-box and same-box arm within 20% of the base;
- the box size +0 B;
- the unit about 4 times faster, from removing the trace's 75% identity share.

## 5. Scores and gates

**Scores.**

| Prediction | Result | Score |
|---|---|---|
| Field view, unequal, at most 12 ns | 5.94 at 1 hop, 6.03 at 5 hops | **MET** |
| 5-hop view against a heap box, at most 10 ns | 5.93 | **MET** |
| Heap-box and same-box arms within 20% | -5% to +2% | **MET** |
| Box size +0 B | 224 B and 792 B per element, unchanged | **MET** |
| Unit about 4x faster | not yet measured | **OPEN**, for the TS lane on this branch |

**New tests:** [`FieldViewIdentityTests`](../../src/tests/GolibTests/FieldViewIdentityTests.cs), nine arms. Against
the base golib, 8 pass and `FieldsOfDistinctRootsSharingAnIdAreDistinct` fails. Against the fix, all 9 pass.

**GolibTests, `-p:GoTargetOS=linux`:**

- Release with tiering off: 2635 total, 1 failed. The failure was `TheBoxGainsNoInstanceState`, the guard asking for
  the new byte's cost to be stated. This commit updates it, and its file re-reads 19/0 in Release.
- Debug: 2568 passed, 0 failed, 67 skipped.
- The skip sets, compared by name: none is Release-only. The 26 Debug-only skips are the optimizer-shape gates (the
  JIT-optimizer and optimizing-compiler `Inconclusive` lines).

**Other gates.**

- `run-behavioral --filter NilPtrIdentity`: 2 projects pass, outputs equal Go.
- ApiCompat strict mode, base golib against fix golib, both directions: no difference. As a control, comparing golib
  against an unrelated assembly does report.
- The full behavioral suite runs on CI, because this box is under the runner's 25 GB disk floor.
