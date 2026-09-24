# B′ — method dual emission: the `ref`-receiver primary beside the ж method

> **STATUS: RATIFIED (coordinator, 2026-08-21) — all seven §9 open questions are ruled AS
> RECOMMENDED**, with ONE binding addition on §4.2's no-silent-wrong-selection claim: S0 must
> include a mechanical guard proving it — a compile-probe matrix over every must-not-select
> receiver shape §4.2 names, asserting each either fails to compile (CS1510) or demonstrably
> binds the twin. The claim is the design's boldest and it must be enforced by construction,
> not carried by argument. Sequencing per OQ-6 as ruled: S0/S1 flag-gated and corpus-inert in
> the terminal era; S2 rides the 1.23.12 regen. The design increment
> [`DESIGN-zh-box-reduction.md`](DESIGN-zh-box-reduction.md) §3.7 explicitly excluded from its
> sign-off ("dual emission doubles surface — it needs its own design increment and its own
> measurement"), commissioned by the 2026-08-20 ruling for the **1.23.12 era**. This document is
> DESIGN-ONLY: zero converter changes, zero golib changes, zero corpus changes ride this branch.
> Pattern per the ratified [`DESIGN-readmemstats-surface.md`](DESIGN-readmemstats-surface.md):
> the measured bill first, an adversarial self-review, and an open-questions list with
> recommendations — nothing lands until the coordinator rules the OQs.

---

## 1. The bill — measured before anything is proposed

Every number in this section was measured on the pinned toolchain (**go1.23.1**, windows/amd64,
this machine, 2026-08-21) or is quoted from a board entry that names its own measurement. Nothing
is carried forward from the §3.7-era sketch without re-derivation.

### 1.1 The acceptance case: `crypto/internal/edwards25519`'s want-zero row, decomposed

The zh-box-reduction-impl lane measured the row at **54 of 55, `TestAllocations` reading 98
objects/run against `want 0`** — stable to the object across sessions — and decomposed every
allocation site on the executed path from the committed `.cs` (board entry 2026-08-20). Three
classes, ONE of them B′'s:

| Class | Sites on the path | Whose |
|:--|:--|:--|
| **Field-ref boxes at method-ARGUMENT positions** (`Ꮡx.Multiply(Ꮡv.of(Point.Ꮡx), ᏑzInv)` — the params are boxes, so every caller mints) | `projP1xP1.Add` 16, `Element.Invert` 9, `Point.bytes` 6, `projCached.FromP3` 4, `Point.fromP1xP1` 4, `Scalar.bytes` 2 | **B′ (the ref-parameter half)** |
| **`heap()` locals that exist to be RECEIVERS** (`ref var z2 = ref heap(new Element(), out var Ꮡz2)` … `Ꮡz2.Square(Ꮡz)`) | ~15 in `Element.Invert` alone | **B′ (the ref-receiver half)** |
| `@new<T>()` — class 3b (`NewIdentityPoint`, `NewScalar`, `projCached`/`projP1xP1` temporaries: five per run) plus `checkInitialized`'s params-array and `Bytes`' backing array | 5 + residue | **Phase C — not B′'s, and this design claims none of it** |

The same entry recorded the structural fact B′ exists to fix: Phase A's lowering **landed
underneath** these methods (`feMul(ж<Element> Ꮡv, ref Element x, ref Element y)` — 14 lowered
signatures in the package), and **a lowered leaf saves nothing at the caller of the boxed method
that wraps it**. nistec fell −96.5 % because its bill is fiat leaf *functions*; edwards25519's
bill is point-arithmetic *methods*.

### 1.2 The corpus-wide constituency, re-derived on the pinned toolchain

`-ref-census` (the A1 instrument; loads `std` once, emits nothing) re-run for this design —
`go1.23.1`, windows/amd64, 304 packages:

```
pointer params 2,749: lowered(A) 528, lowered(A′) 590; method ptr-params (B′ context) 1,609
address-taken locals 2,799: revert 231 (8.3%)
kept-local reasons: … ptr-receiver=1,016 …
```

Two axes matter to B′ and the census now prices both:

**The demand axis — receiver traffic.** 1,016 address-taken locals corpus-wide are kept by Phase A
*solely because their address serves as a method receiver* (`ptr-receiver`, the largest single
kept-reason after `unlowered-position`). Per package, descending:

| Package | ptr-receiver kept locals | Package | ptr-receiver kept locals |
|:--|--:|:--|--:|
| `runtime` | **190** | `net` | 21 |
| `crypto/tls` | 72 | `internal/trace` | 19 |
| `net/http` | 56 | `vendor…/norm` | 19 |
| **`crypto/internal/edwards25519`** | **49** | `archive/zip` | 17 |
| `go/types` | 47 | `archive/tar` | 16 |
| `crypto/x509` | 46 | `time` | 16 |
| `vendor…/dnsmessage` | 40 | `debug/dwarf` | 15 |
| **`math/big`** | **33** | | |

This ranking independently confirms the commissioning ruling's constituency (§3.7's
receiver-chain of() sites: `runtime/proc` 387 × 3 GOOS, `h2_bundle` 212, `database/sql` 154 — the
netpoll, http and database arcs the campaign continues into after the terminal) *and* places the
two acceptance rows on it: edwards25519 at #4, math/big at #8.

**The supply axis — the surface that would dual-emit.** Counting emitted `this ж<T>` receiver
methods in the committed corpus (non-Generated, non-test): **3,762 methods across the corpus**,
top: `net/http` 302, `go/types` 280, `runtime` 231, `math/big` 138, `database/sql` 129,
`crypto/tls` 115, `go/parser` 87, `sync` 73, `crypto/internal/edwards25519` 44. This is the
doubling bill's denominator (§6).

**Method pointer-params (the ref-parameter half's denominator): 1,609** corpus-wide.

### 1.3 The S0/S1 discriminator input — what share B′ claims of `math/big`

The ReadMemStats S0/S1 stage measured the §5.4 discriminator: `math/big`'s converted path
allocates at **50.9× Go** (T = P to within 40 bytes across six windows; the process-wide-counter
hypothesis is dead), and the coordinator rerouted the row to "zh-box/B′ constituency — the
1.23.12-era arc, exactly where edwards25519 already routes." Clearing the 10× bound needs ~80 %
of the converted path's allocation removed.

**This design deliberately does NOT claim a number for B′'s share of that 80 %.** math/big's bill
is a mixture: 33 ptr-receiver locals and 138 ж-receiver methods (B′-shaped) *plus* `nat`
slice-header traffic and `@new<Int>` constructor flow (class 3b / Phase C-shaped). The honest
instrument is the S0 prototype measurement (§7): run `math/big`'s TotalAlloc probe under the S0
flag and read the split off the counter, exactly as the discriminator itself was measured rather
than argued. What can be said structurally: B′'s share is the receiver/argument **mints**, Phase
C's is the **object count of the values themselves** — and on edwards25519's decomposition those
are separable classes with no overlap, so the same separation is expected to hold here.

---

## 2. The load-bearing observation: the dual-emission machinery already exists, in production, banked

B′ was sketched in §3.7 as if it required new machinery. It does not. The corpus already contains
the exact shape, authored by hand, proven end to end:

**`sync/atomic`'s hand-owned scalar types** (`type.cs`, since L11) declare every method in the
`[GoRecv] (this ref T x)` form — the **ref-receiver primary** — and **`RecvGenerator` mints the
ж-receiver twin**:

```csharp
// hand-authored primary (type.cs)
[GoRecv] public static int32 /*new*/ Add(this ref Int32 x, int32 delta) {
    return Interlocked.Add(ref x.v, delta);
}

// generated twin (Generated/go2cs-gen/go2cs.RecvGenerator/….Add.….Int32.g.cs)
public static int Add(this ж<Int32> Ꮡx, int delta) {
    ref var x = ref Ꮡx.DerefOrNull();
    return x.Add(delta);
}
```

Both call shapes are zero-allocation (`type.cs`'s own header, written against the measured
net/textproto want-zero assert), and the composition is *banked*, not merely believed:

- **`sync/atomic` is roster row #159 at 108/108 + 0** — including `TestNilDeref`, which proves the
  twin's nil semantics: `DerefOrNull` hands `Unsafe.NullRef<T>()` for a nil box, so the
  nil-receiver call ENTERS and faults on first touch inside the body — Go's exact contract (the
  call succeeds, the deref panics).
- **`sync.Mutex` implements `sync.Locker` through this shape** — `Lock`/`Unlock` are `[GoRecv]`
  primaries, ImplementGenerator's adapter binds the generated ж twin, and `sync` is banked at
  44 rows. Interface dispatch through the pair is production fact.
- **The reflection bridge already enumerates types whose method sets carry the pair** (`sync`,
  `sync/atomic`, `internal/runtime/atomic` — 52 ж-receiver methods emitted there today), with
  `fmt` (%v over such values) and the method-name projection guards green.

**B′ is therefore a *selection* change, not a mechanism**: teach the converter to EMIT eligible
pointer-receiver methods in the `[GoRecv]` ref-primary form (RecvGenerator does the rest), extend
the primary's *parameters* with Phase A's existing D/X classification, and teach call sites to
bind the direct form where the receiver is statically known. Every existing consumer — interface
adapters, method values, generic constraints, the reflect bridge — keeps binding the ж twin it
binds today.

---

## 3. The emission shape

### 3.1 Declaration side

For an eligible method (eligibility: §4), the converter emits the **primary** where it today emits
the ж form:

```csharp
[GoRecv] internal static ж<Element> Multiply(this ref Element v, ref Element x, ref Element y)
```

- **Receiver**: `this ref T` — always, for an eligible method (that is what eligibility means).
- **Parameters**: each pointer parameter independently classified by the SAME two-sided D/X fixed
  point Phase A runs on package-level functions (`refLoweringAnalysisOperations.go`), extended to
  method scope. A parameter that fails classification stays `ж` in the primary — the two halves
  are independent (§8 OQ-2 recommends receiver-only for S0 precisely so the halves are measured
  separately).
- **Return**: unchanged. Go's fluent-chain convention (`return v`) returns the receiver pointer;
  the primary cannot return `ref` through the existing `ж<Element>` signature consumers, so the
  primary returns what the ж form returns today. **Chained calls stay on the ж form** (§4.2 —
  a chain's intermediate receiver is a ж expression, which is a must-not-select case anyway), so
  the primary's return value is dead at direct call sites in the dominant patterns
  (`Ꮡz2.Square(Ꮡz)` discards it). ⚠ One shape needs care and is priced in §9 (OQ-7): a direct
  call whose RESULT is used (`p := v.Add(x, y)` where `p` re-aliases `v`) must yield a ж box for
  the receiver — the primary cannot mint one without allocating the very box the call exists to
  avoid. Recommendation there: such sites simply do not select the primary.

- **The twin**: `RecvGenerator` mints it from the `[GoRecv]` attribute exactly as it does for
  hand-owns today, extended to forward ref-parameters:

```csharp
public static ж<Element> Multiply(this ж<Element> Ꮡv, ж<Element> Ꮡx, ж<Element> Ꮡy) {
    ref var v = ref Ꮡv.DerefOrNull();
    return v.Multiply(ref Ꮡx.DerefOrNull(), ref Ꮡy.DerefOrNull());
}
```

  ⚠ The parameter forwarding is where the twin's nil semantics need one design decision (§9
  OQ-3): `DerefOrNull` on a nil ARGUMENT hands a null ref that faults on first touch **inside the
  callee** — which is Go's contract for a parameter the callee unconditionally dereferences, and
  the D-classification only lowers parameters it PROVES unconditionally dereferenced (Phase A's
  §10.4 nil doctrine, unchanged). A parameter the callee nil-TESTS is X-vetoed and stays ж in the
  primary, so the twin forwards the box untouched. The doctrine transfers whole; no new nil rule
  is invented for methods.

### 3.2 Overload coexistence is clean by construction

The pair cannot collide or ambiguate:

- **Call-site resolution separates on the receiver's FORM.** `this ref T` binds only a
  ref-addressable variable of type `T`; `this ж<T>` binds only a `ж<T>` expression. No expression
  is both. (Verified against C#'s extension-method rules; also the reason the twin can share the
  primary's name, as `sync/atomic` already demonstrates.)
- **Method values and delegate conversions** target a delegate type whose parameter list names
  `ж<T>` — only the twin converts. `convSelectorExpr`'s method-value emission is untouched.
- **Generic constraints** (`nistPoint[P]` at `*P224Point`) dispatch through the constraint-proxy /
  adapter machinery, which binds the ж twin — the same route it binds today.

---

## 4. The selection rule — where the direct form binds, and where it must not

### 4.1 Declaration eligibility (which methods dual-emit)

A pointer-receiver method of a package-declared struct type is eligible unless:

| Veto | Reason |
|:--|:--|
| **XM-1: hand-owned declaration** | the file is `[module: GoManualConversion]`-marked or the method lives in an `*_impl.cs` companion — the converter does not re-emit these, and hand-owns already choose their own form (A1's X5 hand-own arm, reused verbatim) |
| **XM-2: value receiver** | not in scope — value receivers already emit `this T`/`this ref T` shapes without boxes |
| **XM-3: interface-typed or type-parameter receiver base** | no struct storage to `ref` |
| **XM-4: linkname push/pull participant** | the registries' existing X5 arm, reused |
| **XM-5: `unsafe.Pointer`-adjacent** (`ж<T>`-subclass receivers, reflect-bridge special types) | the boundary types whose representation IS the box |

Everything else dual-emits **unconditionally at the declaration** — selection pressure lives at
call sites, not declarations, because the twin must exist for every method regardless (interface
dispatch, method values, cross-package ж callers), so declaring the primary conditionally would
save nothing and would make eligibility a whole-program question. This is the same
"classification reads only the declaring package" stability property A′ was priced on.

### 4.2 Call-site selection (where the primary binds)

The direct form is selected when the receiver expression is **statically ref-addressable storage
of the receiver type**, and the census's §3.3 shape rows already name these:

| Receiver shape at the call site | Selects | Why |
|:--|:--:|:--|
| local declared via `heap()` ref-alias (`ref var z2 = ref heap(…)` … `z2.Square(…)`) | **primary** | the 1,016-local class — the mint the call exists to avoid |
| plain local / parameter of type `T` whose address Go takes only for the call | **primary** | the receiver-position `of()`-chain class after A's reversion |
| field lvalue reachable without a box hop (`s.f.M(…)` where `s` is direct) | **primary** | `ref s.f` is legal and free |
| deref-in-hand (`(~Ꮡp).M(…)`) | **primary** (as `p.M(…)` via the deref's ref) | the ж is already held; `Ꮡp.Value` is a ref |
| **ж-typed expression** (row-3 pointer var: `Ꮡv.M(…)`) | **twin** | the box exists; forwarding through `DerefOrNull` at the twin costs nothing new |
| **interface-typed receiver** | twin (via adapter) | dispatch surface |
| **generic-constrained receiver** | twin (via proxy/adapter) | nominal constraint surface |
| **method value / delegate** | twin | delegate parameter types |
| **`defer`/`go` argument position** | twin | the boxed carve-out, mirrored from §3.3's defer-go row — the frame outlives the statement |
| **non-ref-addressable receiver** (map index, property-shaped accessor, conditional expression) | twin | `this ref T` cannot bind; C# enforces it (CS1510/CS1657), so mis-selection FAILS THE BUILD rather than corrupting |
| **result-used direct call** (`p := v.M(…)` capturing the returned receiver pointer) | twin | §9 OQ-7 — the primary cannot yield the receiver's box without minting it |

The last column's parenthetical is a property worth stating as a design invariant: **every
must-not-select case either fails to compile on the primary or resolves to the twin by C#'s own
overload rules.** There is no silent-wrong-selection class; the selection rule's failure mode is
a build error, which CNR and the behavioral suite catch as loudly as anything can be caught.

> **MEASURED 2026-08-26 (S0's binding guard) — the invariant HOLDS, and two details in the sentence
> above are corrected by measurement.** The ratification's compile-probe matrix is live at
> `src/tests/GolibTests/ZhBoxSelectionProbeTests.cs`, run against the **real** production pair
> (`sync/atomic`'s `[GoRecv] this ref Int32` primary + `RecvGenerator`'s `this ж<Int32>` twin) with
> Roslyn's own `SemanticModel` and diagnostic bag as the verdict. 15/15 green, and proven
> non-vacuous: injecting a by-value `Add` overload — a faithful simulation of the hazard — turns
> 10 of the 15 red, including every must-not-select row, and the matrix returns to green when it is
> removed. Corrections:
>
> 1. **The cited codes are incomplete, and the one named most is not the one that fires most.** The
>    measured refusals are **CS0206** ("a property or indexer may not be passed as an out or ref
>    parameter") for the map-index and property-shaped arms, and **CS1510** for the conditional
>    expression. **CS1657 does not fire anywhere in the matrix.** The codes are now pinned by a test
>    rather than cited from memory, so a compiler change that moves one — or worse, stops refusing —
>    is caught here.
> 2. **"Resolves to the twin" overstates what the refusal arm does.** On the non-ref-addressable
>    shapes, overload resolution still *selects the primary* — it is the only applicable overload for
>    a receiver of type `Int32` — and the refusal happens later, at argument conversion. Roslyn
>    therefore reports the primary as the site's symbol **on a build that does not compile**. The
>    invariant is unaffected, because it is a disjunction about the BUILD OUTCOME, not about which
>    symbol the compiler names; but a guard that asserts on the symbol instead of the outcome reads
>    a satisfied invariant as a violation. It did, on the first run here, and the note is left
>    standing so S1 does not re-learn it.

> **MEASURED 2026-08-26 (S0b, call-site selection) — the receiver already renders directly; what
> did NOT fall out of A2 is the REVERSION, and §1.2's "1,016-local class" is 55 % smaller than this
> design assumed.** The stage's framing question was whether the receiver renders directly
> (`z2.Square(…)`) or as its box (`Ꮡz2.Square(…)`), and whether that falls out of A2's local
> reversion or needs its own rule. Both halves were driven with probes against the real machinery
> before anything was cut, and they answer differently:
>
> 1. **Rendering needs no new rule, and did not before this stage either.** A probe over all
>    eleven §4.2 receiver shapes plus ten adjacent ones (promotion, named non-struct receivers,
>    array/slice elements, embedded chains) shows `convSelectorExpr`/`convCallExpr` already
>    rendering an addressable receiver directly for every `[GoRecv] ref` method and boxing it for
>    every direct-ж one. The discriminator is `selectorCallsDirectBoxMethod`, keyed on
>    `packageDirectBoxReceiverMethods` — the emitted receiver FORM — and the §4.2 table reproduces
>    its behavior row for row. B′'s primary changes which methods are ref-flavored; it does not
>    need the selection machinery rewritten.
> 2. **The reversion did NOT fall out of A2 — A2 actively blocked it.** `classifyLocalUse` returned
>    a blanket `"ptr-receiver"` kept-reason for *any* pointer-receiver call on a value chain, so a
>    local whose other address use fed a lowered position kept a box the emitted body never
>    referenced. The instrument and the emitter therefore disagreed, and the census was the wrong
>    one: `bLoweredPlusRefMethod` emitted `ref var z = ref heap(new T(), out var Ꮡz)` with `Ꮡz`
>    occurring **nowhere else in the function**, while the shape one line shorter reverted. The
>    unit fixture pinned the disagreement (`keptMethod` asserted a kept box for a `[GoRecv] ref`
>    receiver the emitter had already left on the stack).
>
> **The rule** (`receiverUseKeptReason`) keeps the box only where a probe showed one consumed:
> a **direct-ж callee** (`ptr-receiver-box`), a **method value** rather than a call
> (`ptr-receiver-value`), or a **receiver under defer/go** (`ptr-receiver-defer-go`). Everything
> else imposes no reason. The first two are load-bearing beyond their rows: `performEscapeAnalysis`
> documents that a reverting verdict is mutually exclusive with captureMode, and captureMode is
> exactly `bodyCallsCaptureModeMethodOn || pointerMethodValueAddressTaken` — those same two shapes
> — so the invariant now holds by construction rather than by the old reason's breadth
> (`collectCaptureModeMethods` writes both method sets under one condition, so they cannot diverge).
>
> **The split, re-derived corpus-wide on the pinned toolchain** (windows/amd64, go1.23.12). These
> are CENSUS verdicts — read the emission measurement below before pricing anything off them:
>
> | | before | after |
> |:--|--:|--:|
> | `ptr-receiver` reason occurrences | **1,016** | — |
> | → `ptr-receiver-box` (the real B′ target) | | **448** |
> | → `ptr-receiver-defer-go` / `ptr-receiver-value` | | **5** / **3** |
> | → imposes no reason (was pure conservatism) | | **560** |
> | locals reverting corpus-wide | **231** | **621** |
> | locals kept for the receiver reason ALONE | **693** | **0** |
>
> **⚠ THE EMISSION DELTA IS 5 BOXES, NOT 560 — and that gap is this stage's most useful finding.**
> A seeded A/B reconvert settles it: the **baseline** converter reproduces the committed corpus
> **byte-identically** (3,361 `.cs`, same hash — so the control holds and B − A is exactly this
> change), and B differs in **5 files**: three sources shedding **5 `heap()` mints**
> (`archive/tar/reader.cs` −2, `runtime/windows/proc.cs` −2, `runtime/windows/tracetime.cs` −1)
> plus two `package_info.cs` position maps. Every other box-mint spelling (`new ж<`, `Ꮡ(`) is
> unchanged to the occurrence.
>
> **Why the census moved 390 locals and the emission moved 5.** The census marks a local
> address-taken on the IMPLICIT receiver take, but the EMITTER never boxed a receiver-only local in
> the first place — `eRefMethodOnly` was already a plain local before this change. A box is actually
> minted only where the local ALSO carries a real box-forcing address use that lowers, and
> corpus-wide that co-occurrence is five sites. So the 560 were not boxes removed; they were census
> verdicts that emission had always ignored. What this change buys at flag-off is therefore **an
> instrument that describes the emission** — plus five genuinely redundant mints.
>
> **The consequence for S1's pricing, stated plainly: census local-counts are NOT emission counts.**
> §1.2's 1,016 is a census figure, and so is the 448 below; pricing B′ off either would overstate
> the win by orders of magnitude, exactly as it would have here. **S1 must be priced by EMISSION**,
> using the instrument this stage established and controlled: seeded A/B reconvert, baseline
> reproducing the committed corpus byte-identically, then a box-mint diff. The census figure is an
> upper bound on the constituency, never a prediction of the saving.
>
> **The arc's named instrument agrees, and says why.** `os.File.WriteString` measures **17.00
> obj/op — UNMOVED** by this stage (three runs, golib's own `AllocationCounter`, the same counter
> §7's targeted probes use; the byte figure is not comparable to the 2,368 stamp because the probe's
> string and buffer shape is its own, and the object count is the shape-invariant half). That is the
> correct result rather than a disappointing one: `WriteString` is emitted `this ж<File>` — a
> **direct-ж** method, so it sits in the 448 that a ref-receiver primary would convert, not in what
> S0b recovers. The instrument moving here would have meant the selection rule had reached a site it
> should not have.
>
> That bound is still worth having, and it is genuinely smaller than §1.2 assumed: **448**, not
> 1,016, of the receiver-position uses carry a box-consuming receiver at all. And B′'s own prospects
> are NOT bounded by this stage's five: a direct-ж receiver IS really boxed by the emitter
> (`Ꮡz.FieldAddr()` mints), which is precisely why the ref-flavored case had so little left to
> recover and why the primary's case has to be measured rather than inferred from either number.
>
> **S1's constituency, located.** Of the 448, `ptr-receiver-box` is the **SOLE** kept-reason for
> **295** — those revert the moment the callee gains a primary, with no other analysis owed. The
> remaining 153 carry a second reason (`unlowered-position`, `non-candidate-callee`, …) and need
> Phase-A work as well, so they are not B′'s to claim alone. Where they live:
>
> | package | locals | package | locals |
> |:--|--:|:--|--:|
> | `runtime` | 83 | `crypto/tls` | 22 |
> | `crypto/internal/edwards25519` | 46 | `crypto/internal/edwards25519/field` | 13 |
> | `net/http` | 37 | `sync` | 13 |
> | `math/big` | 32 | `strings` | 8 |
> | `go/types` | 29 | `crypto/ecdh` | 7 |
>
> **§7's S0 acceptance target totals 59 locals** — `crypto/internal/edwards25519` (46) plus its
> `field` subpackage (13) — which is the concrete size of what S0's two-package prototype is
> reaching for; and §1.3's deferred "B′ share of `math/big`" now has its receiver-side term
> measured at **32** (its 33 receiver locals, one of which reverted here), leaving the `nat`-flow
> share to Phase C exactly as §1.3 refused to guess.
>
> Two mechanical notes for S1. **The census had to be taught the same fact**: `runRefLoweringCensus`
> called `analyzeRefLowering` without `collectCaptureModeMethods`, so it could not see the receiver
> FORM at all; it now mirrors the conversion driver's ordering against fresh per-package maps and
> restores the drivers' state, keeping its standing "touch nothing" property. The unit harness owed
> the same mirror, or every fixture would have taken the nil-map fallback and the rule would have
> been untested. **And the guard was proven in both directions** before it was believed: restoring
> the blanket behavior reds exactly the revert assertion; dropping every carve-out reds exactly the
> four keep assertions; both restores byte-identical.

### 4.3 The retroactive widening of Phase A — a measured positive interaction

A1 recorded that `edwards25519/field`'s `feMulGeneric`/`feSquareGeneric` are **X3-vetoed on their
trailing `v.carryPropagate()` method call**, stripping `feMul`/`feSquare` through the fixed point
— the field family's hot path keeps the boxed convention *because a method call appears in the
body*. Once B′'s primary exists, a method call on a `ref`-held receiver is no longer a
representation change, and the X3 arm can stop vetoing bodies whose only offense is a
direct-selectable method call. **The Phase-A fixed point must re-run as part of B′'s S1**, and
the A1 instrument already measures the delta (`forward-unlowered=495` corpus-wide is the ceiling
on what could unstrip). This is B′ paying Phase A back, and it falls out of re-running an
existing instrument rather than new analysis.

---

## 5. Interactions with the settled machinery — each one named, each with its production precedent

| Surface | Interaction | Precedent that pins it |
|:--|:--|:--|
| **RecvGenerator** | gains ref-parameter forwarding in the twin (today it forwards value/box params verbatim); authorship direction is UNCHANGED — `[GoRecv]` primary in, ж twin out | `sync/atomic` ×~40 methods, `sync.Mutex`, `internal/runtime/atomic` |
| **ImplementGenerator / interface dispatch** | adapters keep binding the ж twin; nothing about adapter minting changes | `Mutex → Locker`, banked `sync` 44 |
| **Constraint proxies / generic dispatch** | bind the twin, as today | `nistPoint[P]` machinery, banked nistec rows |
| **Reflect bridge** | method discovery, `%v`, method-name projection all already operate over packages carrying `[GoRecv]` pairs; the twin remains the canonical bridge surface | banked `sync`, `sync/atomic`, `fmt`'s 63 rows |
| **Promotion (embedded methods)** | TypeGenerator's promoted accessors forward through the same member lookup that resolves hand-own pairs today | `noCopy`/embed patterns in the banked corpus |
| **Nil semantics** | receiver: `DerefOrNull` → `NullRef` → fault-on-touch (Go's contract), **banked as `TestNilDeref`**; parameters: only provably-dereferenced params lower (§10.4 doctrine transfers whole) | row #159's 108/108 |
| **The ж-box design's Phase B (golib byte-slimming)** | independent — B′ changes emission, B changes box representation; they compose without ordering constraints beyond §10.5's A-before-B, already satisfied | — |
| **FINDING-managed-box-uintptr-lifetime** | untouched — B′ never converts a pointer to a scalar or back | — |

---

## 6. The doubling bill — sized from the census, priced for measurement

**Static surface**: +3,762 `[GoRecv]` primaries corpus-wide at full rollout, each with a
RecvGenerator twin file (the generator emits one file per method×type — `sync/atomic` shows the
shape). Generated-file count and IL size both roughly double *for the method surface*; the
per-package worst cases are the constituency's own heads (`net/http` +302, `go/types` +280,
`runtime` +231 ×3 GOOS under L3).

**Compile-time**: the honest answer is that no estimate from first principles is worth writing
down when S0 can measure it. The current baselines to measure against: full
`go2cs-stdlib.slnx` ~92–188 s warm (i9-class) / 516 s `--no-incremental` (i7-5820K);
`go2cs.slnx` ~87 s `--no-incremental`. **S0's deliverable includes the per-package compile-time
delta on the prototype packages**, and S2 gates on the corpus-wide build staying inside the
budget table's existing ceilings (a doubling that blows the 600/900 s rows is a finding, not a
tax to absorb silently).

**Emission churn**: at full rollout every package with pointer-receiver methods rewrites — this
is why the commissioning ruling scheduled B′ for the 1.23.12 era, "on the corpus where every row
re-derives anyway (the H10 economics)." The design honors that: **S2 lands with the 1.23.12
regen**, never as a standalone corpus rewrite.

---

## 7. Staging and acceptance — measured gates, in the ReadMemStats pattern

**S0 — prototype, flag-gated, two packages.** Emit the dual form (receiver-only primaries; §8
OQ-2) for `crypto/internal/edwards25519` + `edwards25519/field` behind a converter flag; regen
those two packages into a scratch root (never the corpus). Measure:

1. `TestAllocations`: 98 → the class-3b floor. From §1.1's decomposition the B′-attributable
   classes (receiver `heap()` locals + method-argument field-ref boxes at *selected* sites) go to
   zero; the floor is the five `@new<T>()` per run + params-array + `Bytes` backing — predicted
   **≤ 10 objects/run**, measured not assumed. A measured floor materially above the prediction
   means the §4.2 selection table is leaving traffic on the twin — a per-site census answers
   which row, before any rule is widened.
2. `math/big` TotalAlloc probe under the same flag (its 33 receiver locals + 138 methods
   dual-emitted): the **B′-vs-Phase-C share** of the 50.9×, read off the counter (§1.3).
3. Compile-time and generated-surface deltas for both packages.
4. **The nistec control: must not regress.** nistec's 0-of-32 lowered params and its −96.5 %
   Phase-A result are receiver-free facts; its `TestAllocations` row and the four `Perf*`
   pointer-family benchmarks (`PerfRefLower`, `PerfIfaceCall`, `PerfIface`, `PerfIfaceShell`)
   must hold within noise. A dual emission that costs the JIT inlining budget shows up here
   first.

**S1 — the parameter half + selection breadth + the Phase-A re-run.** Extend primaries with
lowered parameters (the twin gains ref-forwarding); land the §4.2 call-site table corpus-wide
behind the same flag; re-run the Phase-A fixed point with the X3 method-call arm relaxed (§4.3)
and census the unstrip delta. Gate: A1's instrument re-run shows `other-veto` still **zero**
(the completeness property survives the wider world), CNR against the flag-off world byte-identical.

**S2 — rollout with the 1.23.12 regen.** Flag becomes default inside the hop's corpus regen;
acceptance is the full combined gate plus: `edwards25519` re-measured (and banked, per the
commissioning ruling, once Phase C covers the floor), `math/big` re-measured against its 10×
bound, the six reflect-consumer canaries derived at gate time, and the budget table's build rows
re-measured and updated rather than silently exceeded.

**What B′ does NOT claim, restated from §1**: the five `@new<T>()` per run (class 3b), math/big's
constructor/`nat`-flow share, and any want-zero row whose floor is class 3b. `edwards25519` banks
when B′ **and** Phase C land — the ruling's sequencing, unchanged here.

---

## 8. Adversarial self-review (charter §7)

1. **"The primary's `ref` receiver changes observable identity"** — a direct call passes the
   receiver by ref; the ж twin derefs a box to the same storage. Go pointer identity is only
   observable through pointers, and every pointer-producing path stays on the twin (§4.2). The
   one leak would be a primary body taking `&x` of its own receiver — but the body is CONVERTED
   code whose Go source took `&v` of the *pointer receiver's pointee*, i.e. the pointer itself,
   which the selection rule keeps on the twin (result-used and repoint shapes — OQ-7, X4). Held.
2. **"DerefOrNull-forwarded ref params could silently no-op on nil"** — no: `NullRef` faults on
   touch, and only provably-touched params lower (§3.1). The param that is nil-TESTED stays ж.
   The residual risk is a classifier bug, which is Phase A's existing correctness surface, not a
   new one — and its guard corpus (A1's synthetic strips) transfers.
3. **"Doubling the method surface doubles JIT/AOT work and could cost more wall-clock than the
   allocations save"** — the twins are one-line forwarders the JIT inlines (sync/atomic measured
   both shapes zero-allocation, and the perf table's `Startup` row watches AOT size), but the
   honest answer is S0's item 3/4: measure, with nistec + the Perf family as the regression
   tripwire. The §4 B1 microbench lesson (a time regression must not land formally unmeasured)
   is inherited as a gate, not a footnote.
4. **"The selection table will be wrong somewhere"** — its failure mode is deliberately a BUILD
   error (§4.2's invariant): a mis-selected primary on a non-addressable receiver is CS1510, not
   a corruption. The corrupting direction (twin selected where primary was intended) is merely
   the status quo — allocation, not wrongness.
5. **"RecvGenerator twins for 3,762 methods explode the Generated tree"** — sized in §6, measured
   at S0, and bounded by the same L3 routing that already carries runtime ×3 GOOS. If the file
   count is the problem, the generator can emit per-TYPE twin files instead of per-method — an
   engineering choice deferred to S0's numbers, noted so it is not re-discovered.
6. **"B′ should wait for Phase B (box slimming) or fight it"** — they are orthogonal by
   construction (§5): B′ removes mints, B slims what still mints. The one ordering fact (§10.5's
   A-before-B) is already satisfied. No new ordering constraint is introduced.
7. **"The math/big share claim smuggles a number"** — it doesn't: §1.3 refuses the number and
   routes it to S0's counter, the same discipline the discriminator itself used.

---

## 9. Open questions for the coordinator — each with a recommendation

- **OQ-1 — Rollout shape: blanket dual-emission at eligible declarations, or demand-driven (only
  methods with measured receiver traffic)?** *Recommendation: blanket per §4.1.* The twin must
  exist regardless, call sites carry the selection pressure, and demand-driven eligibility would
  make declarations depend on whole-program facts — the instability A′ was explicitly priced to
  avoid. The bill difference is generated-surface only, and §6 prices it.
- **OQ-2 — S0 scope: receiver-only primaries first, or receiver+parameters together?**
  *Recommendation: receiver-only at S0, parameters at S1.* The receiver half is the measured
  dominant (1,016 locals; the Invert chains), it needs no RecvGenerator change (the twin's
  receiver forwarding exists today), and separating the halves makes S0's edwards25519 delta
  attributable instead of blended.
- **OQ-3 — The twin's nil contract for ref-forwarded parameters: `DerefOrNull` (fault on touch)
  as recommended, or an explicit entry null-check?** *Recommendation: `DerefOrNull`.* It is Go's
  own semantics for a dereferenced parameter, it is the banked receiver precedent
  (`TestNilDeref`), and an entry check would panic EARLIER than Go does for a param whose deref
  is conditional — precisely the class the classifier keeps ж anyway.
- **OQ-4 — Marker: reuse `[GoRecv]` verbatim, or mint `[GoDual]`?** *Recommendation: `[GoRecv]`
  verbatim.* The generator, the reflect bridge, and the promotion machinery already key on it;
  a second marker would be a second thing to keep in sync with zero added information.
- **OQ-5 — Does S1's Phase-A re-run (the X3 relaxation, §4.3) need its own ruling, or does this
  design's ratification cover it?** *Recommendation: covered here, gated by A1's instrument
  showing `other-veto = 0` post-relaxation.* It changes no doctrine — it re-runs a ratified
  fixed point in a world where one veto's premise no longer holds.
- **OQ-6 — Sequencing: confirm S2 rides the 1.23.12 regen and S0/S1 may land flag-gated before
  it.** *Recommendation: yes to both.* S0/S1 are corpus-inert by construction (flag-off default,
  scratch-root regens), so they can be built and measured in the terminal era without touching
  the H10 economics; S2 is the hop's passenger, per the commissioning ruling.
- **OQ-7 — Result-used direct calls (`p := v.Add(x, y)`): keep them on the twin (recommended), or
  teach the primary a ж-yielding variant?** *Recommendation: twin, permanently.* The returned
  pointer IS the receiver's box; a call site that wants the box must have the box, and minting
  one inside the primary would relocate the allocation, not remove it. The census's row-3
  dominance (806 corpus sites carry existing boxes) says this class was never B′'s constituency.

---

*Inputs: `DESIGN-zh-box-reduction.md` §3.2/§3.3/§3.7/§4/§10; `CENSUS-zh-box-a1.md`; the
edwards25519 board entry (2026-08-20, lane `claude/zh-box-reduction-impl`); the B′ commissioning
ruling (2026-08-20); the S0/S1 discriminator harvest (2026-08-21); `DESIGN-readmemstats-surface.md`
(pattern); fresh `-ref-census` and corpus-surface measurements, this machine, go1.23.1, 2026-08-21.*

---

## 10. AMENDMENT 2026-09-02 — the fluent-body gap, measured and RULED (R3)

> Added, not rewritten (the doc-type rule). Found at the S0 emitter's first `return` — before any
> emitter line existed — and ruled the same day (coordinator, mailbox `01c110efb`).

**The gap.** §3.1 fixes the primary's return type as the ж form's and its twin snippet DELEGATES
the return; OQ-7 rules the primary cannot yield the receiver's box. Both cannot hold for a body
whose Go text is `return v`: with only `ref Element v` in hand, the primary must either MINT a box
(relocating the exact allocation B′ removes, and breaking ж-path pointer identity through the
delegating twin — Go's `p := Ꮡv.Multiply(…); p == Ꮡv` is true) or fail to compile. The panel did
not catch it; writing the first return did.

**The census** (go/types walk of the two S0 packages at the pin, GOROOT-guarded instrument):
**RECV-ONLY 38 / NO-RECV 32 / MIXED 5** — the MIXED five are `field.SetBytes`, `field.SqrtRatio`,
`SetBytes`, `SetUniformBytes`, `SetCanonicalBytes` (the `return v, nil` / `return nil, err` family).

**The ruling (R3):**

- **(a) RECV-ONLY — the primary returns `ref T`** (the receiver itself: free, allocation-less, and
  chain-capable at direct sites), its receiver-returns emitted `return ref v;`. **The twin
  delegates and then returns its own `Ꮡv`** — Go's own semantics, since the method returns the
  receiver pointer and the twin holds it; identity is a GUARD row, not an argument
  (`ZhBoxFluentPrimaryTests.TwinReturnsItsOwnBox_TheIdentityGuard`, proven able to go red by the
  fresh-box neuter). §3.1's "return unchanged" rationale binds the TWIN, which keeps the ж
  signature for every existing consumer; the primary's consumers are only the new selection sites.
- **(b) NO-RECV — the §3 shapes verbatim.** Nothing new.
- **(c) MIXED — a new declaration veto, `XM-6-receiver-escapes`:** a body that conditionally hands
  out its receiver pointer NEEDS the box; these five stay twin-only. Their traffic is the SetBytes
  family whose keeps A3's floor already prices on the boxed path, so the ≤10-floor prediction is
  untouched.

**The matrix rows landed FIRST, per the ruling's order** — four compile probes (plain-local
selects the ref-return primary; ж-typed keeps the twin; ref-capture of the chain compiles;
result-used-by-value COMPILES and binds the primary, deliberately inverted to document that OQ-7's
result-used row is enforced at CONVERTER EMISSION, not by C#) and two execution guards (twin
identity + chain; primary mutating the caller's storage with no box in existence). 21/21 with the
original 15, the identity neuter reds exactly one row, restore verified.

## 11. 2026-09-24 -- the context TestAllocs B′-admission read (G, post-hop)

The disclosure's plan in `src/core/context/go2cs_test_disclosures.json` cites this record for three
field-ref boxes in `WithDeadlineCause` (`context/context.cs:700`, `:709` and `:711` at `ffa5c1015a`).
**B′ admits none of the three.** The per-site reasons follow:

- **Measured, B′ changes nothing here.** With a converter built at `ffa5c1015a`, a single-package
  conversion flag-on (`-dual-recv -dual-recv-params`) and one flag-off are BYTE-IDENTICAL for
  `context` and `internal/sync`. The only `ref` primaries in `context` are the ones the flag-off
  emission already carries (the `String`, `Deadline` and `Value` methods).
  `sync` refuses to convert into an unseeded root: `refPrimaryHandOwns` registers `sync.Mutex.Lock`, and
  the refusal names the missing hand-own. That is the registry's positive control and safety-floor
  rule 2 working as intended, not a B′ reading.
- **`:700` `c.of(timerCtx.ᏑcancelCtx).propagateCancel(parent, …)` -- irreducible by any ref form.**
  Go's own escape analysis reads `context.go:466:7: leaking param: c` (go1.24.13 `-gcflags=-m`): the
  receiver is retained. A retained interior pointer is free in Go and is a heap object in golib, so
  this view has no Go counterpart. It is the leading candidate for the WithTimeout(5ms) leg's +1
  (9 against want 8), but that attribution is UNMEASURED.
- **`:709` `c.of(timerCtx.Ꮡmu).Lock()` and `:711` `defer(cʗ2.of(timerCtx.Ꮡmu).Unlock, …)` -- already
  one object, and not B′'s to remove.** `cʗ2` is `c`, so both ask the same (box, accessor) pair, and
  the field-view cache (`ж.Views.cs`) returns one view per call: the plan's "may already collapse"
  now reads DOES. `sync.Mutex.Lock` and `Unlock` ARE ref primaries, the hand-owned registrations of
  I3 (`refVerdictPublication.go`). The reason they do not bind here is I3's call-site base test
  (`convSelectorExpr.go`, the I3 arm), which deliberately admits only a ref-lvalue base (the current
  deref-aliased receiver or a deref-aliased pointer parameter) and NOT a bare pointer ident such as
  `c`, whose rendering is the box. Removing the view needs TWO changes, and either one alone removes
  zero counted objects, because the other site still mints the same view:
  (a) I3's base widened to a pointer-ident base rendered through `.Value` (`c.Value.mu.Lock()`; `c`'s
  nil panic is kept, because `.Value` on the nil box panics); and
  (b) the `defer` at `:711` lowered without a method-group delegate. A delegate over a `this ref`
  extension is CS1113. The precedent is the finally-flag form this package already emits for a
  ref receiver (`finally { if (ᒐd1) Ꮡc.DerefOrNull().mu.Unlock(); … }`, the `ᒐd1` flag in the
  cancelCtx `Done` body).

**Routing.** The (a)+(b) pair is I3's territory (the os arc's increments), not B′'s. Its reach is every
`p.mu.Lock()` / `defer p.mu.Unlock()` pair on a box-local base (the same file also shows `cc` at
`:310`/`:312` and `p` at `:434` and `:534`), and it is sized before anything is cut. The disclosure's
plan should re-point from B′ to that pair plus the `:700` attribution at its next re-sign.
