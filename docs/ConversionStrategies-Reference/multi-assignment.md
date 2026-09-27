# Multi-Assignment and Evaluation Order
<!-- {% raw %} — Jekyll/Liquid guard: this page contains Go composite-literal and template syntax ({{ … }}) that Liquid would otherwise parse; the HTML comment hides the tag on GitHub. -->

[Reference index](README.md) · [Summary of this topic](../ConversionStrategies.md#multi-assignment-and-evaluation-order)
All right-hand operands in assignment expressions in Go are evaluated before assignment to the left-hand operands. C# can operate equivalently using tuple deconstruction (_thanks to Eugene Bekker for the [suggestion](https://github.com/ritchiecarroll/go2cs/issues/6)_). For the following Go code:

```go
x, y = y, x+y
```
the equivalent C# code operates as follows:
```csharp
(x, y) = (y, x + y);
```

The simultaneous deconstruction is **mandatory** whenever the targets alias — a swap `s[i], s[j] = s[j], s[i]` shattered into `s[i] = s[j]; s[j] = s[i];` loses the first target's original value (the second read sees the already-overwritten slot). The converter routes a multi-target assignment to the deconstruction form when every target is a *reassignment to existing storage*, counted per element; an index, star-deref, or selector LHS is always such a write. This recognition keyed off `getIdentifier`, which resolves a target's root identifier by unwrapping index/star/selector/chan/array/map nodes but **not** `ParenExpr` — so an index whose base is a *parenthesized* pointer deref, `(*p)[i]` (the shape a pointer-receiver method uses to write its own named-slice element, e.g. a heap's `func (h *myHeap) Swap(i, j int) { (*h)[i], (*h)[j] = (*h)[j], (*h)[i] }`), resolved to a nil root and was **not** counted as a reassignment. The parallel assignment then fell through to sequential statements and the swap silently corrupted the slice — one element lost, the other duplicated:

```go
func (h *myHeap) Swap(i, j int) { (*h)[i], (*h)[j] = (*h)[j], (*h)[i] }
```
```csharp
// before: two sequential stores drop the temporary — (h)[i] and (h)[j] both end up as the old (h)[j]
[GoRecv] internal static void Swap(this ref myHeap h, nint i, nint j) {
    ((h)[i], (h)[j]) = ((h)[j], (h)[i]);   // simultaneous deconstruction, correct swap
}
```

Such a paren-deref index LHS is now counted as a reassignment directly (a single-element `(*h)[i] = v` write emits identically on either path, so nothing else drifts). The bug was invisible to compilation — the broken form compiled cleanly — and only surfaced when a converted test *ran*: it silently miscompiled `container/heap`'s test heap and the `internal/trace/internal/oldtrace` order heap (three swap sites). (Guarded by the `PointerReceiverSliceSwap` behavioral test — a pointer-receiver `swap` and a full slice reversal by repeated swaps, output-compared vs `go run`; the pre-fix converter loses elements and diverges. It is also what makes `container/heap`'s Go test suite validate — see [Phase 4](../Roadmap.md#phase-4--convert-and-run-go-package-tests).)

The swap recognition above routes a *pure*-reassignment parallel assignment (every target already exists) to the deconstruction form. A **mixed** parallel `:=` — some targets reassigned, some newly declared, with `rhsLen == lhsLen` — was not covered: it satisfies neither `lhsLen == reassignedCount` nor the all-declared arm, and (unlike the call-deconstruction mixed cases below) has no single-call RHS to trigger `tupleResult`, so it fell through to **sequential** statements. When a reassigned target is *read by a later right-hand expression*, that read must see the target's ORIGINAL value (Go evaluates every RHS before any store); sequential emission reads the already-updated value. strconv's Ryū shortest-float rounding does exactly this — `dc, fracc := dc>>extra, dc&extraMask`, where `fracc` must read the pre-shift `dc`:

```go
dc, fracc := dc>>extra, dc&extraMask   // fracc must read the ORIGINAL dc
```
```csharp
(dc, var fracc) = (dc.Rsh(extra), (uint64)(dc & extraMask));   // whole tuple evaluated, then deconstructed
```

The converter now detects this read-after-write **hazard** (`lhsReusedInLaterRhs`: a written target's `types.Object` appears in a strictly *later* RHS element) and routes the mixed assignment through the same deconstruction path, where C# evaluates the entire right-hand tuple before deconstructing. It is scoped to the actual hazard, so a hazard-free mixed `:=` (`m, n := m+1, 100`) keeps its minimal sequential form. A newly-declared **int/uint** element takes its *explicit* type rather than `var` — `(a, b, nint c) = (b, a, a + b)` — because `var` would infer C# 32-bit `int` from a literal/int32 RHS instead of the Go-`int`-target `nint`; `string` (an `@string`'s u8 span cannot sit on a value-tuple LHS) and `unsafe.Pointer` hazards are excluded and keep the sequential form, a documented limitation with no stdlib occurrence. Like the swap bug this was invisible to compilation and surfaced only at runtime: it made strconv's shortest-float formatting round down, failing `math`'s `TestFloatMinMax` (`4e-324` vs Go's `5e-324`). (Guarded by the `ParallelAssignmentHazard` behavioral test — reassigned + newly-declared parallel forms whose later RHS re-reads a written target, output-compared vs `go run`; the pre-fix converter diverges. It is also what makes `math`'s Go test suite validate — see [Phase 4](../Roadmap.md#phase-4--convert-and-run-go-package-tests).)

Go's **partial redeclaration** — `a, b := f()` where `a` already exists in the same scope — reuses `a` (assigns it) and declares only the new names. A blanket `var (a, b)` would re-declare the reused variable, so the converter emits `var` per *newly-declared* element only:

```go
frac, e := normalize(frac)   // frac is the existing parameter; e is new
```
```csharp
(frac, var e) = normalize(frac);
```

The same per-element mechanism handles a destructured element whose **address is taken** (`list, delta := netpoll(0); injectglist(&list)`). Such a local must be heap-boxed so its `Ꮡlist` companion exists, but the combined `var (list, delta) = …` deconstruction cannot declare it as a `ref var … = ref heap(…)`. The converter emits the escaping element's heap declaration first, then a mixed deconstruction-assignment in which the escaping element is the pre-declared box ref-local and the rest declare with `var`:

```go
list, delta := netpoll(0)
injectglist(&list)
```
```csharp
ref var list = ref heap<gList>(out var Ꮡlist);
(list, var delta) = netpoll(0);     // list is the box ref-local; delta is newly declared
injectglist(Ꮡlist);                 // Ꮡlist now exists
```

Without this, `&list` emits `Ꮡlist` with no box (CS0103), and the `Ꮡ(value)` copy fallback would silently lose writes made through the pointer. (Guarded by the `TupleDestructureEscapingLocal` behavioral test — a mutate-through-pointer proves the real local is updated; runtime exercises it in the `netpoll` poll loops.)

A subtler case: a newly-declared tuple element can be flagged *escaping* by analysis yet need **no** heap box — typically an already-pointer local that is merely returned (`pp, now := pidleget(now)`, where `pp` is a `*p` that the function returns). The heap-decl path above only owns elements that produce an actual `ref var … = ref heap(…)`; an escaping element with no such declaration must still be counted as newly-declared so it receives its `var`, or the deconstruction emits `(pp, now) = …` with `pp` declared nowhere (CS0103). Both the mixed (`(var pp, now) = …`, reusing the value parameter `now`) and the all-shadowing (`var (ppΔ1, gpΔ1) = …`) forms are handled. (Guarded by the `TupleMixedDeclareReassign` behavioral test; runtime hits it in `pidlegetSpinning` and `findRunnable`.)

## A multi-value RETURN reads its plain operands AFTER its calls

The read-after-write hazard above has a **return-statement** sibling, and it arrives from the opposite direction: there Go's ordering is fixed and C#'s sequential emission breaks it; here Go's ordering is *free* and C#'s tuple literal fixes it the other way.

Go's spec orders only a return statement's **calls** — "all function calls, method calls, receive operations, and binary logical operations are evaluated in lexical left-to-right order" — and leaves its plain operands deliberately unordered against them. gc resolves that freedom the same way every time, because its order pass rewrites the statement: each call is spilled to a temporary **first**, and the result list is then assembled from those temporaries and whatever plain operands remain. So under gc every plain operand is read *after* every call. A C# tuple literal has no such freedom — it evaluates strictly left to right — so a plain operand written before a mutating call is copied **before** the mutation:

```go
func ParseOID(oid string) (OID, error) {
    var o OID
    return o, o.unmarshalOIDText(oid)   // gc: the call runs, THEN o is read
}
```
```csharp
public static (OID, error) ParseOID(@string oid) {
    OID o = default!;
    var ᴛ1 = o.unmarshalOIDText(oid);   // gc's own rewrite, emitted
    return (o, ᴛ1);
}
```

Without the spill this emits `return (o, o.unmarshalOIDText(oid));`, which copies the **empty** `o` and only then fills the original — so `crypto/x509`'s `ParseOID` returned a zero-length OID beside a nil error, and every parse "succeeded" with no bytes. Nothing about it is visible at compile time: both sides compile, both return two values, and only the *content* of the first differs.

The spill fires only where the ordering is **observable** — where a later operand's call receives an earlier operand's storage *by address*, the only way it can write what that operand reads. Three shapes hand a call that address:

| Shape | Example | Storage the call can write |
|---|---|---|
| pointer-receiver method on a value | `return o, o.fill()` | `o` itself — the call takes `&o` |
| explicit address argument | `return n, raise(&n)` | `n` itself |
| a pointer operand handed over | `return c.n, c.bump()`, `c` a `*counter` | `*c`, so a read *through* `c` observes it |

Whether the read and the write actually *meet* is decided by comparing **access paths** — a root variable (`types.Object`, exactly as in `lhsReusedInLaterRhs`, so a same-named but distinct variable is never a false positive) plus the hops from it to the storage in question, where a field or element hop stays *inside* the previous location and a deref *leaves* it. Two locations conflict when they share a root, agree on every hop they both have, and the longer path's extra hops contain no deref.

A root plus a single "indirect" flag is not enough, and the corpus says so. `net/http`'s `return n.handler, n.pattern.String(), n.pattern, matches` reads storage inside `*n` while the call writes `*(n.pattern)`; the flag model reads both as one location — "something behind `n`" — and spills a call that provably cannot touch what the first operand reads. The paths diverge at `.handler` versus `.pattern`, so they do not conflict and nothing spills. The model holds the case one hop over just as firmly: `return nd.pat.n, nd.pat.bump()` reads *through* the very pointer the call writes through, the write path is a prefix of the read path with no deref between them, and it spills. Where a hop cannot be spelled exactly — a field promoted through embedding — the path is **truncated** rather than abandoned, naming the larger enclosing location, so imprecision can only ever over-report a conflict and never miss one.

Every call-bearing operand at or **below** the hazardous index spills, not merely the hazardous one: the spec *does* fix the calls' order among themselves, so spilling `b.bump()` out of `return b.n, side(), b.bump()` while leaving `side()` in the tuple would run bump first. A channel **receive** spills with them, since the spec's sentence orders receives alongside calls. A type conversion and a pure builtin (`len(o.der)`) do not: both are calls syntactically and reads semantically, gc spills neither, and leaving them in the tuple is what puts their reads after the spilled calls — which is exactly where gc puts them. The temporaries are numbered from the file-monotonic `tupleTempIndex` the converter's other multi-value expansions already share, so two spills in one scope cannot collide.

The scope is as much of the rule as the spill is: a rule that spilled every call-bearing operand would also be correct, and would rewrite most of the corpus for no correctness gain. Four controls therefore keep the call *in* the tuple — an operand that **is** the pointer (`return c, c.bump()`; both orders yield the same pointer, and the emitted pointer is the box rather than a copy), a call on unrelated storage (`return a.n, b.bump()`), a **value**-receiver method (`return c.n, c.peek()`; the receiver is a copy, so the caller can observe nothing), and the net/http pointer-field shape above. Deliberately *not* covered, each because deciding it needs more than the statement itself: an operand that contains a call of its own (`return o.f + g(), o.mutate()` — gc spills `g()` too and reads `o.f` last, so spilling the whole operand would re-create the problem rather than fix it; no corpus site), a pointer whose **pointee no path can name** (`f(getPtr())` — the interprocedural question one step removed), aliasing through a slice or map's **backing store** (`return s[0], fill(s)`, where no address is taken at the call site at all), and a call that reaches the operand through a package-level or captured variable, which is interprocedural outright.

**Corpus footprint: 2 production files** (A/B of two seeded whole-stdlib reconverts, control binary versus fixed, 10,260 files emitted per side) — `crypto/x509/oid.cs`, where the spill is the bug fix, and `runtime/symtabinl.cs`'s `return u, u.resolveInternal(pc)`, where it is emission-only (the method reads its receiver and writes nothing, so gc's order and C#'s agree on the value; the spill simply stops relying on that). Their two `package_info.cs` position maps move with them, and nothing else in the corpus does.

(Guarded by the `MultiValueReturnOrder` behavioral test — all five hazard shapes, a three-operand call-order case, and the four controls, output-compared vs `go run`; before the fix every hazard reports the pre-mutation value. `returnOperandOrder.go` owns the analysis and `returnOperandOrder_test.go` pins the emission against three neuters: the rule forced off, the rule forced on everywhere, and `pathsConflict` reduced to the root-plus-indirect model — the last reporting exactly the two controls the access path exists to separate.)

## A SELECTOR left-hand side counts as a reassignment — a field swap must stay simultaneous

The paren-deref fix above closed the *index* form of the target-classification gap; the **selector** form (`x.f, y.g = …`) had the same hole. The classifier deliberately drops a selector LHS's root identifier (`getIdentifier` would return the *base* — a package name, or a struct local — which, not itself being reassigned, would wrongly be counted as a new declaration and take a `var` prefix), and then counted it as **neither** reassigned nor declared. A parallel assignment whose targets are *all* selectors therefore satisfied no tuple-path gate — not `lhsLen == reassignedCount`, not `lhsLen == declaredCount`, and (with no single-call RHS) not `tupleResult` — and shattered into sequential stores, losing the swap's implicit temporary:

```go
// regexp/onepass.go — makeOnePass: put the empty-match leg in inst.Out
inst.Out, inst.Arg = inst.Arg, inst.Out
```
```csharp
// before — both fields end up holding the ORIGINAL Arg
inst.Value.Out = inst.Value.Arg;
inst.Value.Arg = inst.Value.Out;

// after — the simultaneous deconstruction C# gives for free
(inst.Value.Out, inst.Value.Arg) = (inst.Value.Arg, inst.Value.Out);
```

Note the tell in the "before": the very next statement of the same Go function swaps two plain *locals* (`matchOut, matchArg = matchArg, matchOut`) and was already emitted correctly as `(matchOut, matchArg) = (matchArg, matchOut);` — the divergence was purely the target *shape*. The consequence was silent: every `InstAlt` whose empty-match leg needed swapping got a corrupted dispatch, so `regexp` quietly lost its one-pass engine for `^[a-c]*$`, `^(?:a*)$`, `^.bc(d|e)*$` and friends, and `^[a-c]+$` stopped matching `"abc"` at all. A field write is a write to existing storage exactly as the index and star-deref forms are, so it is now counted as reassigned like them — which also aligns single-selector assignments (`h.flags &= ^writing`) with the narrowing-cast rendering a plain-ident target already got. (Guarded by the `ParallelAssignmentHazard` extension — a pointer-receiver field swap, a cross-struct rotate reading pre-assignment values, and a package-var swap; and by the `RangeVarReassign` / `AndNotAssignNarrow` goldens, whose re-baselines are this routing change.)

## A STAR-DEREF of a CALL result counts as a reassignment — the last shape of the classification gap

The paren-deref *index* form and the *selector* form above each closed one hole in the target
classifier. The third and last is a **star-deref whose operand has no ident root at all**: `getIdentifier`
unwraps index/star/selector/chan/array/map nodes but has no `CallExpr` arm, so `*l.Ptr(i)` — the deref of
a method that returns a pointer *into* a backing store — resolved to a nil root. It then reached neither
the selector arm nor the index arm of the `ident == nil` branch, and the plain-ident star arm
(`*v = …`) lives in the `ident != nil` branch it never entered. The target was counted as **neither**
reassigned nor declared, no tuple-path gate was satisfied, and the parallel assignment shattered:

```go
// internal/trace/internal/oldtrace/parser.go — Events.Swap, the only mutator sort.Stable calls
func (l *Events) Swap(i, j int) { *l.Ptr(i), *l.Ptr(j) = *l.Ptr(j), *l.Ptr(i) }
```
```csharp
// before — the second store re-reads the slot the first just overwrote, so the
// swap is a NO-OP that duplicates element j over element i
l.Ptr(i).Value = l.Ptr(j).Value.ΔClone();
l.Ptr(j).Value = l.Ptr(i).Value.ΔClone();

// after — the simultaneous deconstruction
(l.Ptr(i).Value, l.Ptr(j).Value) = (l.Ptr(j).Value.ΔClone(), l.Ptr(i).Value.ΔClone());
```

This is the same package the paren-deref fix repaired one layer down (that one fixed the oldtrace *order*
heap; this is the event list the parser sorts at the end of `parse`), and it was the last converted-code
divergence in `internal/trace`. The failure mode is worth noting because it is **not** a sorting complaint:
a corrupted `Swap` makes `sort.Stable` duplicate and lose events, and the damage is reported much later by
the old-trace parser's own post-pass consistency checks — `p 3 is running before start`, `previous sweeping
is not ended before a new one` — which read like trace-semantics bugs and point nowhere near the assignment.
It is also why the divergence hid for so long: `sort.Stable` performs no swaps on an already-ordered input,
and the event stream only acquires inversions from the `EvGoSysExit` timestamp rewrite that runs immediately
before the sort. Only traces with enough syscall traffic to reorder anything ever exercised the broken
`Swap`, so 10 of the 12 old-trace fixtures passed and the two `stress` fixtures failed.

Counting such a deref as a reassignment is scoped to **multi-target** assignments: simultaneity is the only
property at stake, and a single-element `*(*T)(p) = v` has no hazard to fix. That single-element form is also
the overwhelmingly common one — 213 sites in the converted corpus at Go 1.23.12, 60 in `reflect/value.go`
alone, nearly all the `*(*T)(p) = v` unsafe-write idiom — so leaving them on their existing path holds the
change's emission footprint to the one site in the Go tree that is genuinely a parallel deref assignment.
(The single-element form was measured to emit identically on either path, so the scoping buys footprint,
not correctness.)
(Guarded by the `PointerReceiverSliceSwap` extension — a `cell`/`cells` pair mirroring oldtrace's
`Event`/`Events`, exercised by disjoint swaps, a full reversal, and a selection sort driven entirely by the
call-deref swap, output-compared vs `go run`; the pre-fix converter leaves all three visibly uncorrected.)

<a id="an-address-taken-reference-typed-local-heap-boxes-too--ꮡvalue-copies-are-only-for-reads"></a>Moved to [An address-taken reference-typed local heap-boxes too — `Ꮡ(value)` copies are only for reads](escape-analysis.md#an-address-taken-reference-typed-local-heap-boxes-too--ꮡvalue-copies-are-only-for-reads).

<a id="an-address-taken-named-result-heap-boxes-too"></a>Moved to [An address-taken NAMED RESULT heap-boxes too](escape-analysis.md#an-address-taken-named-result-heap-boxes-too).

<a id="an-address-taken-value-parameter-heap-boxes-too"></a>Moved to [An address-taken VALUE PARAMETER heap-boxes too](escape-analysis.md#an-address-taken-value-parameter-heap-boxes-too).

<a id="a-field-addressed-value-local-heap-boxes--ꮡxof-copy-boxes-orphan-writes"></a>Moved to [A field-addressed value local heap-boxes — `Ꮡ(x).of(…)` copy-boxes orphan writes](escape-analysis.md#a-field-addressed-value-local-heap-boxes--ꮡxof-copy-boxes-orphan-writes).

<a id="a-capture-mode-method-called-on-a-field-chain-of-a-local-is-an-address-of-too"></a>Moved to [A capture-mode method called on a FIELD CHAIN of a local is an address-of too](methods-and-receivers.md#a-capture-mode-method-called-on-a-field-chain-of-a-local-is-an-address-of-too).

<a id="the-same-chain-one-level-up-recvf1f2-on-a-pointer-receiver"></a>Moved to [The same chain one level up: `&recv.f1.f2` on a POINTER RECEIVER](methods-and-receivers.md#the-same-chain-one-level-up-recvf1f2-on-a-pointer-receiver).

<a id="ꮡxoftꮡf-returns-one-view-per-box-field--the-field-view-cache"></a>Moved to [`Ꮡx.of(T.Ꮡf)` returns ONE view per (box, field) — the field-view cache](escape-analysis.md#ꮡxoftꮡf-returns-one-view-per-box-field--the-field-view-cache).

<a id="a-type-switch-binding-is-escape-analyzed-like-any-other-local"></a>Moved to [A TYPE-SWITCH BINDING is escape-analyzed like any other local](escape-analysis.md#a-type-switch-binding-is-escape-analyzed-like-any-other-local).

<a id="a-package-level-function-literals-own-locals-are-analyzed-too"></a>Moved to [A PACKAGE-LEVEL function literal's own locals are analyzed too](escape-analysis.md#a-package-level-function-literals-own-locals-are-analyzed-too).

<a id="a-pointer-receiver-method-value-heap-boxes-its-receiver--the-implicit-xm"></a>Moved to [A pointer-receiver METHOD VALUE heap-boxes its receiver — the implicit `(&x).M`](escape-analysis.md#a-pointer-receiver-method-value-heap-boxes-its-receiver--the-implicit-xm).

---

[← Empty Interface (`any`)](empty-interface.md) · [Index](README.md) · [Short Variable Redeclaration (Shadowing) →](shadowing.md)
<!-- {% endraw %} -->
