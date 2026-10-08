# Generic Constraints

[Reference index](README.md) · [Summary of this topic](../ConversionStrategies.md#generic-constraints)
A Go generic constraint becomes a C# `where` clause. Most type-set constraints lift to the matching golib/.NET interface — a `[]T` element constraint to `ISlice<T>`, `[N]E` array-core to `IArray<E>`, `map[K]V` to `IMap<K,V>`, `chan T` to `IChannel<T>` — plus, for operator-bearing type sets, the `System.Numerics` operator interfaces (`IAdditionOperators`, `IComparisonOperators`, …) so the body's `+`/`<`/`==` on the type parameter compile. The Go built-in `comparable` maps to golib's [CRTP](../Glossary.md#crtp) `comparable<T>`.

## An array-core constraint `~[N]E` lifts to `IArray<E>`

A type-set constraint whose core is an ARRAY — `func polyAdd[T ~[256]fieldElement](a, b T) T` (ML-KEM's
`ringElement`/`nttElement` share the core `[256]fieldElement`) — must map to `where T : IArray<E>`, NOT
to the operator interfaces the general type-set path would produce. An array is a *comparable* type in
Go, so the operator-set resolver put `Array` in the comparable set and lifted `IEqualityOperators<T, T,
bool>`; the named-array generated wrapper (which the converter emits for `ringElement` etc.) does not
implement that interface, so every instantiation failed CS0315, and the interface exposes no array
surface, so the body's `t[i]` (CS0021), `for i := range t` (CS8130 on the index deconstruction), and
`for _, x := range t` (CS1579/CS8183) had nothing to bind against. The fix (`getArrayConstraintElem` in
`constraintOperations.go`, a new branch in `getGenericDefinition`) detects a single-array-core type set,
extracts the element type, and emits `where T : /* ~[N]E */ IArray<E>, new()`. The array wrapper already
declares `IArray<E>, ISupportMake<wrapper>` (the go2cs-gen `Array` inherited-type template), whose
`ref E this[nint]` indexer and `IEnumerable<(nint, E)>` enumeration supply exactly the indexing/ranging
surface the body needs, and the `new()` (appended by the same path) covers `var f T`/`T{}` construction.
Greened `crypto/internal/mlkem768` (census 254 → 255); the reconvert A/B changed only `mlkem768.cs`'s
four constraint lines. (Guarded by `GenericArrayConstraint` — two array-wrapper types over a shared
`~[4]fieldElement` core through a generic function that indexes, index-ranges, value-ranges, and
constructs the type parameter, values vs Go.)

## A type set of COMPOSITE terms lifts nothing — the union survives only as a comment

The array-core entry above fixed one SHAPE of a wider defect, and the rest of it surfaced on
`runtime/pprof`'s `testProfileRecordNullPadding[T runtime.StackRecord | runtime.MemProfileRecord |
runtime.BlockProfileRecord]` — a union whose terms are all plain structs, which was the whole of that
package's build wall (five call sites, `error CS0315` on each).

The root is what `IEqualityOperators<T, T, bool>` MEANS on each side. Go's `==` works on any comparable
type, so the operator-set resolver listed `Struct`, `Array`, `Pointer` and `Channel` in
`comparableOperatorTypes` and lifted that interface for them. But a C# `where` clause is a claim about
the type ARGUMENT implementing a BCL interface, not about an operator being available, and nothing on
the Go side of the corpus implements it: a converted struct, `array<T>`, `ж<T>` and `channel<T>` all
compare through `Equals`/`AreEqual`. The lifted clause was therefore unsatisfiable by construction, and
the diagnostic named the concrete struct rather than the constraint that could not admit it.

Two changes, both in `constraintOperations.go`. The composite kinds leave `comparableOperatorTypes`, so
the rule is stated once where the operator sets are defined instead of per constraint shape (the
array-core branch's hand-written `suppressLiftedConstraints` was the same rule applied to one shape; a
union of *named array types* took no such branch). That alone exposed the fall-through underneath: with
no operator lift and no interface to name, `getGenericDefinition`'s generic tail emitted the Go union
text VERBATIM as a C# constraint list — `where T : runtime.StackRecord | runtime.MemProfileRecord | …`,
`error CS1003` ×4, a syntax error rather than a type error. So `constraintTypeSetIsInexpressible` closes
it, asked LAST after every shape with a real emission has been tried: a non-empty type set whose
operator set is empty emits the union as a breadcrumb comment plus the one constraint C# can still
express —

```csharp
internal static T testProfileRecordNullPadding<T>(ж<testing.T> Ꮡt, @string name, Func<slice<T>, (nint, bool)> fn)
    where T : /* runtime.StackRecord | runtime.MemProfileRecord | runtime.BlockProfileRecord */ new()
```

— the same answer, for the same reason, the built-in [`comparable`](#the-comparable-constraint) arm
reaches: Go's own checker validated every instantiation before conversion, so the C# clause has nothing
left to enforce. `new()` is kept (unlike that arm) because a composite type set admits no pointer type
argument. The corpus footprint is one line: censused at the fix, this was the ONLY converter-emitted
`IEqualityOperators` clause in the whole corpus that is not on a numeric or ordered union — which is the
shape's own signature, since a composite type set is a subset of no other operator set and so lifts the
comparable operators ALONE, with no arithmetic siblings. (Guarded by `Constraints` — a struct-only
`recordA | recordB | recordC` union through a generic function instantiated at each term; without the
fix it reproduces CS0315 at all three sites.)

## A single-term pointer constraint `[P *T]` erases the parameter to `ж<T>`

A type parameter constrained to a single, non-tilde pointer term — go/types' flat-copy helper
`func clone[P *T, T any](p P) P { c := *p; return &c }` (predicates.go) — cannot be modeled as a C#
type parameter: no C# constraint fixes a parameter to a *specific constructed type*, and `ж<T>`
implements no interface through which `*p`/`&c` could be expressed generically. The operator-lift
fallback emitted `where P : /* *T */ IEqualityOperators<P, P, bool>, new()` with the deref dropped
(`c = p` — CS0029 P→T) and the box mismatched (`return Ꮡc` — CS0029 ж<T>→P), and the call site's
synthesized `clone<ж<ΔSignature>, ΔSignature>(asig)` failed CS0311 (ж<> implements no
IEqualityOperators).

The Go spec makes the faithful lowering an *identity*, not an approximation: a non-tilde term's type
set is a **singleton**, so `P`'s only permissible type argument is `*T` itself. The converter therefore
**erases** such a parameter (`pointerCoreConstraint` in `constraintOperations.go`): it leaves the
emitted `<…>` list and `where` clauses (a breadcrumb comment preserves the Go constraint), renders as
`ж<T>` everywhere it appears (a `getAliasQualifiedTypeName` arm beside the `*types.Pointer` arm), and the parameter
classification treats a `p P` exactly like a `p *T` (`paramPointerType` — deref alias, `Ꮡ` box naming),
so the entire existing pointer machinery applies unchanged:

```csharp
internal static ж<T> clone<T>(ж<T> Ꮡp)
    /* where P : *T (erased: P renders as ж<T>) */
{
    ref var p = ref Ꮡp.Value;

    ref var c = ref heap<T>(out var Ꮡc);
    c = p;
    return Ꮡc;
}
```

Call sites drop the erased position from any synthesized explicit type-argument list
(`renderedTypeArgs`, applied at convCallExpr's two synthesis blocks and convSelectorExpr's
method-group form): `clone(asig)` emits `clone<ΔSignature>(asig)`, and a callee whose remaining
parameters make C# inference sufficient stays bare (`setThrough(Ꮡn, 55)`). An EXPLICITLY written Go
instantiation equally drops erased positions — full (`setThrough[*int, int](…)` →
`setThrough<nint>(…)`), partial (`clone[*thing](…)` → bare `clone(…)`, the rest inferring), and the
function-VALUE form (`fv := clone[*thing, thing]` → `var fv = clone<thing>;`) — via
`explicitTypeArgsAfterErasure` in convIndexExpr/convIndexListExpr. A C# consumer calls the emitted
method naturally — `T` sits in a real parameter position, so inference works without spelling the
phantom `P`.

The pointer classification flips at every use shape, not just the deref/address pair: returning the
parameter WHOLE yields its box (`return a` → `return Ꮡa;`), passing it onward to another erased
callee — including self-recursion — supplies the box (`cloneChain<T>(clone<T>(Ꮡp), …)`; the
interface-shaped argument arm carves out erased params exactly like instantiated pointers), copying
it into a local is a Go pointer copy (`q := p` → `var q = Ꮡp;`, writes through `q` land in the
caller's referent), and a nil comparison takes the box form over the nil-deferring entry alias
(`if p == nil` → `ref var p = ref Ꮡp.DerefOrNull(); … if (Ꮡp == nil)` — a nil argument reaches the
guard instead of throwing at entry, e.g. `orZero[*int, int](nil)`). The NAMED constraint-interface
spellings — `[P PtrOf[T]]` and the embedded `[P interface{ PtrOf[T] }]`, where
`type PtrOf[T any] interface{ *T }` — resolve to the identical singleton type set and erase
identically. The constraint interface's own DECLARATION follows the existing constraint-interface
convention (`partial interface PtrOf<T> { /* Type constraints: *T */ }`): a pointer term is
a type-set term, not an embeddable interface (previously it emitted an interface inheriting the
struct `ж<T>` — CS0527), and a GENERIC constraint interface carries its own `<T>` list, so the
arity-0 `<ΔT>` marker list and its generated operator machinery are both suppressed for it.

Erasure is deliberately gated to the identity case: **function** type parameters whose constraint
type-set is a single non-tilde pointer term. Declined shapes warn instead of silently mis-emitting —
an approximate `~*T` admits *named* pointer types, which emit as `/*ж<E>*/` wrapper classes
(not identity with `ж<E>`); pointer unions have no single identity; and erasing a generic *named
type's* parameter would change its emitted arity at every use. None occur anywhere in the converted
stdlib (exhaustive GOROOT census: go/types' `clone` is the only compiled occurrence of the pattern;
see `DESIGN-pointer-core-typeparam.md` on the fix branch for the full study). (Guarded by
`PointerCoreConstraints` — clone/read/write/round-trip through `[P *T]` and the swapped-order
`[T any, P *T]`, flat-copy independence verified, values vs Go.)

## An integer named-numeric wrapper implements the integer operator interfaces

A `[GoType num:]` wrapper (`type stringID uint64`) already declared the *common* numeric operator
interfaces so it could serve a `cmp.Ordered`-shaped constraint (`IAddition`/`ISubtraction`/
`IMultiply`/`IDivision`/`IEquality`/`IComparison`/`IIncrement`/`IDecrementOperators`), but the
*integer-only* three — `IModulusOperators`, `IBitwiseOperators`, `IShiftOperators<T, int, T>` —
were deliberately left off because their operators (`%`, `&|^~`, `<<`, `>>`) are kind-gated. That
left a named integer type unable to satisfy a converter-emitted `~integer` operator constraint:
internal/trace's `type dataTable[EI ~uint64, E any]` instantiated with `type stringID uint64` was
CS0315 ×48 on exactly those three interfaces. The `NumericTypeTemplate` operators already exist
(same kind-gate), so `InheritedTypeTemplate` now also *declares* the three integer interfaces for an
integer underlying (float/complex keep only the common set). `IShiftOperators` additionally requires
`operator >>>` (unsigned right shift) — added to the integer operator block; Go emits no `>>>`, but
the member is needed to satisfy the interface. Cleared internal/trace's 48 CS0315 (49→1, the residual
being the unrelated ΔLabel CS0542). Guarded by `NamedNumericOperatorConstraint` (a generic
`mix[K ~uint64 | ~int32]` applying modulus/bitwise/both-shifts on the type parameter, instantiated
with a named `uint64` and a named `int32`, values vs Go). Corpus-verified against math/big (Word),
archive/tar, and time (Duration).

## A named-numeric wrapper is `IComparable<T>` as well as ordered by operators

Ordering has two surfaces in .NET and the wrapper only carried one. `IComparisonOperators<T,T,bool>`
(above) serves a constraint lifted from `cmp.Ordered`; `IComparable<T>` is what the BCL's own
ordering binds — `Array`/`List.Sort`, `SortedSet<T>`, `Comparer<T>.Default` — and, decisively for
converted code, what golib's N-argument `min`/`max` are constrained on. (The two-argument forms take
`IComparisonOperators`, because a *type parameter* constrained by `cmp.Ordered` has no
`IComparable<T>` conversion; the `params ReadOnlySpan<T>` forms cannot, since a span element must
compare through a member, not an operator.) So a named numeric bound `min(a, b)` and failed
`min(a, b, c, d)`: `min(a-got, got-a, a-got+q, got-a+q)` over `crypto/internal/mlkem768`'s
`type fieldElement uint16` was CS0315, "no boxing conversion from `fieldElement` to
`System.IComparable<fieldElement>`". `InheritedTypeTemplate` now declares `IComparable<T>` on the
**same kind-gate** as `IComparisonOperators` — every numeric kind except complex, which Go orders no
more than C# does — and `NumericTypeTemplate` emits its single member inside the same gated block:

```csharp
public int CompareTo(fieldElement other) => m_value.CompareTo(other.m_value);
```

Forwarding to the *underlying* value's `CompareTo`, rather than writing the comparison out of the
wrapper's own `<`/`>`, is what keeps a named float on the BCL total order (NaN below everything) —
which is what makes `min` yield NaN when any argument is NaN, as Go's does. Every underlying a
`[GoType num:]` wrapper can name satisfies it: the aliases are BCL primitives, `uintptr` is a golib
struct that declares `IComparable<uintptr>` itself, and a wrapper over another wrapper picks up the
member this template gives it. The wrapper was already `IEquatable<T>`; this makes it ordered too,
matching the golib `uintptr` and `@string` structs, which are both. (Guarded by extensions to the
`MinMaxBuiltin` behavioral test — `min`/`max` at two and four arguments over named unsigned,
floating and signed underlyings, values vs Go; the pre-fix generator is CS0315 ×10 across the three
kinds.)

## An untyped constant argument to `min`/`max` takes the call's type

Go converts an untyped constant argument of `min`/`max` to the call's type: the type of its typed
arguments, or the constant's default type when every argument is constant. golib's `min`/`max` are
generic, so C# infers `T` from the arguments as emitted, and a bare integer literal is a C# `int`. The
converter therefore casts each untyped-constant argument to the call's type wherever the bare form
would infer a different `T`:

<!-- source: src/tests/Behavioral/MinMaxBuiltin/main.go:143-144 -->
```go
x := max(u8, 1)
x += 100
```
<!-- source: src/tests/Behavioral/MinMaxBuiltin/main.cs.target:92-93 -->
```csharp
var x = max(u8, (uint8)(1));
x += 100;
```

Left bare, `max(u8, 1)` infers `T = int`: `%T` prints `int32`, the value boxes as the wrong type in an
interface, and `x += 100` gives 300 where Go's `uint8` wraps to 44. With `uint32`, `uint64`, `uint` or
`uintptr` there is no implicit conversion from `int` at all, so the bare call does not compile, and
`min(s, "a")` over strings binds neither `@string` nor the literal's UTF-8 span. The cast is emitted
for every sized or unsigned integer type, `uintptr`, `string` (`(@string)("a"u8)`), every named type
(`(fieldElement)(1)`), and an all-constant `int` call, whose literals would infer `int32` where Go's
default type is `int`: `max((nint)(1), (nint)(2))`.

The bare form is kept wherever it already infers the call's type, so that output reads as before:
the float types (the literal carries the context's `F` or `D` suffix), `int32` and `rune` (a C#
integer literal is an `int`), and `int` or `int64` beside a typed argument when every constant fits in
an `int`, which then widens to `nint` or `long`. The same arm casts a NAMED untyped constant argument,
which renders as its `UntypedInt` static (see
[Named Numeric Types and Constant Contexts](named-numeric-types.md)).

Guarded by: `MinMaxBuiltin` (each shape prints `%T` and `%v` against Go: int8 through uint64,
uintptr, int and uint, float32 and float64, a mixed float/int constant, strings, the three named
kinds, all-constant calls, and a later wrap in `uint8`).

## Lifted shift constraint uses the BCL shape `IShiftOperators<T, int, T>`

The lifted Integer operator set constrains shifts as `IShiftOperators<T, int, T>` — the shift **count** is `int`, not the type parameter. Every [BCL](../Glossary.md#bcl) binary integer implements exactly that shape (`IShiftOperators<TSelf, int, TSelf>`); only C# `int` itself happens to also satisfy the self-typed form, so the self-typed constraint made every non-`int` instantiation fail (CS0315 — strconv's `bsearch[S ~[]E, E ~uint16 | ~uint32]` on `ushort`/`uint`). The shape is also exactly what emitted bodies need: the converter coerces every shift count to `int` (`x << (int)(k)`), so a generic body can only ever perform `T << int`. The generated named-constraint interface template (`Integer` in go2cs-gen) and its dynamic-conversion placeholder shift operators use the same `int`-count shape, keeping the two emitters consistent. (Guarded by the `GenericTypeInference` extensions `bsearchLike`/`halve` — `~uint16 | ~uint32` instantiations with a shift on the type parameter, values vs Go.)

## Builtins over constrained slice type parameters

golib's builtins carry **interface-typed overloads** so a value held as a constrained type parameter (`S ~[]E`, boxed to its `ISlice<E>` constraint) binds directly: `copy(ISlice<T1> dst, ISlice<T2> src)` (plus an `ISlice<byte>`/`@string` form), `clear(ISlice<T> s)`, and two-argument `min`/`max` constrained on `IComparisonOperators` (Go's `cmp.Ordered` lifts to operator interfaces; a constrained `E` has no `IComparable<E>` conversion). **All four `min`/`max` overloads PROPAGATE NaN**, which is Go's own spec rule — if any argument is a NaN the result is a NaN — and which neither natural C# spelling gives for free: the operator form `x < y ? x : y` answers the NON-NaN side whenever the NaN sits on the left, because every C# comparison involving a NaN is false; and the params form's `IComparable<T>` total order sorts NaN BELOW everything, which happens to be Go's answer for `min` and is the OPPOSITE of Go's answer for `max`. Both test explicitly now, through one shared per-`T` fact (`builtin.OrderedFacts<T>`) that classifies the floating kinds — the two BCL primitives, and the generated single-field `/*num:floatNN*/` wrapper a NAMED Go float becomes, recognized by walking that one field so a wrapper-over-a-wrapper resolves for free. The gate is a `static readonly` per closed `T`, so it folds at JIT time and no integer instantiation pays for it, and the fact carries a same-width reinterpret rather than an operator because `CompareTo`/`Equals` cannot see a NaN at all (`double.NaN.CompareTo(double.NaN)` is 0 and `double.NaN.Equals(double.NaN)` is true, both by BCL design). Measured by `slices`' `TestMinMaxNaNs`, which replaces each element of a float64 slice with NaN in turn and requires `slices.Min` AND `slices.Max` to propagate it; guarded by `GolibTests.OrderedMinMaxNaNTests`, which holds all four overloads including a named-float wrapper. The box wraps the same backing array, so interface writes land in the caller's storage — `copy`/`clear` into an `S` are true write-throughs (span windows, memmove semantics for overlap). Overload resolution keeps concrete calls on the exact `slice<T>` overloads (an exact parameter beats a boxing conversion), so nothing outside generic bodies changes. Cleared ~37 of the slices package's constraint seams. (Guarded by the `GenericTypeInference` extension `CopyClearMinMax` — copy into and clear through constrained values, write-through verified by value vs Go.)

**S-preserving sub-slice and append.** Go's sub-slice of a named slice type yields the *same named type sharing the same backing* — pdqsort's recursion depends on it (`pdqsort(s[:mid])` with `s S`). The `ISliceWrap<TSelf, T>` static-abstract factory (`TSelf Wrap(in slice<T> source)`) supplies the non-copying reconstruction: `slice<T>` implements it as identity, every generated named-slice wrapper wraps the window in its own type, and the `~[]E` where-clause carries it (`ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>`). A sub-slice of a constrained type parameter emits golib's `subslice<S, E>(s, lo, hi)` (type arguments explicit — `E` is constraint-only) which routes `S.Wrap(window.Reslice(…))`; the new `slice<T>(ISlice<T> view)` constructor SHARES storage (unboxes a `slice<T>`, reconstructs any other implementer from its source array and window). `append` on a constrained value binds golib's `append<S, T>(S, params ReadOnlySpan<T>)` (S from the first argument, T from the span — fully inferrable) and wraps the result back to S; its body routes to the core `slice<T>.Append` directly, since a recursive `append(…)` call would resolve back to itself (`slice<T>` satisfies the constraints). The same change fixed the named-slice WRAPPER template's sub-slice members, which routed through `ToSpan()` — *detached copies*, a silent write-through divergence for named slice types generally; they now route through the wrapped `m_value` (sharing). (Guarded by the `GenericTypeInference` extensions `SumHalves` — recursion over sub-slices of S with a write through the deepest view, verified against the caller's array — and `AppendKeep`.)
**No bound of a constrained sub-slice is a SENTINEL** (2026-08-26). The omitted-high form used to travel as `high = -1` through the three-argument method and the omitted-low form as `low = -1`, so `s[i:]` and `s[i:-1]` were the identical call — and the low convention was worse than an ambiguity, because the method clamped EVERY negative low to 0 rather than only the sentinel. Go panics for a negative index, so `slices.Insert(s, -1, …)` and `slices.Replace(s, -1, 2, …)` — whose bodies OPEN with `_ = s[i:]` and `_ = s[i:j]` as their bounds check, the expressions existing for no other purpose — silently succeeded. The remedy removes both sentinels rather than moving them: an omitted LOW is emitted as the `0` it means (Go's `s[:h]` *is* `s[0:h]`, so no overload is needed), and an omitted HIGH selects a two-argument `subslice<S, E>(s, low)` overload. `subslice3` needs no companion — Go's grammar requires the high bound in a full slice expression — and all three now route `slice<T>.Reslice` directly rather than the `slice()` extension, whose own `-1` defaulting convention would have re-opened the collision one layer down. A golib-only remedy was impossible and the reason is worth stating: with one signature and the converter passing `-1` for "omitted", the two calls are byte-identical at the boundary, so no amount of golib logic can separate them — the honest layer is the emission. The corpus footprint is `core/slices/{slices,iter}.cs` and two behavioral goldens, since `subslice` is emitted only for type-parameter receivers. **Residual, recorded not fixed:** the ORDINARY (non-type-parameter) path emits `s[Low..]`, whose `int`→`Index` conversion throws `ArgumentOutOfRangeException` for a negative bound — a .NET exception, not a Go panic, so it is neither `recover`-able nor contained the way `RuntimeErrorPanic.SliceBoundsOutOfRange` is; and `SliceExtensions.slice`'s `-1` default still collides at exactly `-1` for the three-index form. Neither is reachable from a banked row today. (Measured by `slices`' `TestInsertPanics` and `TestReplacePanics`; guarded by `GolibTests.ConstrainedSubsliceBoundsTests`, which holds the negative, out-of-range, valid and backing-shared cases together.)

Every generated named-slice wrapper also implements the non-generic `IArray` surface explicitly. The public typed `Source` remains `T[]` for the concrete wrapper, but the interface member is emitted as `Array IArray.Source => ((IArray)m_value).Source!;`, matching golib's `IArray.Source` contract and keeping `len(IArray)`, element-address helpers, and interface-typed builtins bound to the wrapper. Pointer elements use the same form, e.g. `type queue []*item` emits `ISlice<ж<item>>` plus the explicit `Array IArray.Source` member. (Guarded by `NamedSlicePointerElements`.)

**S where `[]E` is expected.** Go assignability lets a named-slice-typed value pass where the unnamed `[]E` is expected (`rotateRight(s[m:i], …)`, `pdqsortOrdered(x, …)`); the converter materializes such an argument through the SHARING `slice<T>(ISlice<T>)` constructor — `pdqsortOrdered(new slice<E>(x), …)` — a cast cannot apply (interface-constrained source; C# forbids user conversions from interfaces). The constructor unboxes a boxed `slice<T>` directly and otherwise takes the implementer's full-window interface sub-slice, which every golib implementer returns as a boxed shared `slice<T>` — NOT `Source`, which materializes a detached copy (caught by the write-through gate: the helper's write must land in the caller's array). The 3-index form on a constrained value emits `subslice3<S, E>`, and a constrained spread (`append(s, v.ꓸꓸꓸ)` — a `Span<E>`) binds an exact `params Span<T>` twin of the constrained append (betterness otherwise picked the legacy `params T[]` candidate with `T = Span<E>`, a ref struct as type argument — CS9244). (Guarded by the `GenericTypeInference` extension `PassSlice` — S passed to a concrete `[]E` helper, write-through verified by value vs Go — and by the `ConstrainedSliceParamInPlace` behavioral test, which drives a *full in-place mutation* through the materialized `slice<E>` — an element reversal and a real insertion sort mirroring `slices.Sort`/`SortStableFunc` and `internal/fmtsort`'s make+append-built `SortedMap` — over plain, named, and `[]string` sequences, asserting the caller observes the reordering. A detached copy would leave the caller's slice untouched.)

**Explicit `[]E(x)` conversion of a `~[]E` type parameter.** The *explicit* twin of the assignability case above — Go that spells out the slice conversion (`reverse([]E(x))`, `x` of type `S ~[]E`) rather than relying on assignability — took a different converter path and was broken (CS1503). `isTypeConversion`'s `*ast.ArrayType` arm rejects it (a type parameter's `Underlying()` is its constraint *interface*, not `[]E`, so the identical-underlyings gate fails), so it fell through to the general call assembly and rendered `slice<E>(x)` — the golib *array-only* builtin `slice<T>(T[])`, which the `ISlice<E>`-typed source `S` cannot satisfy. The converter now intercepts this shape in the general call path (mirroring the sibling `string|[]byte`-union `[]byte(x)` special case at `convCallExpr.go`): when the conversion target is a slice-type literal and the sole argument is a `*types.TypeParam` with a `~[]E` slice core (`typeParamSliceCore` — the *same* recognizer the implicit path uses), it emits the SHARING `new slice<E>(x)` constructor, so explicit and implicit `~[]E`→`[]E` conversions land identically on the sharing ctor and preserve Go's slice-conversion aliasing. Genuine conversions are untouched: named-slice casts (`[]CaseRange(special)`) take the `isTypeConversion` cast path; string/nil sources are not type parameters; the `string|[]byte` union is handled by the block just above (`typeParamSliceCore` is nil for it). Proven output-neutral (all 1696 stdlib `.cs` byte-identical across an old-vs-fixed reconvert; the behavioral corpus unchanged). (Guarded by the `ConstrainedSliceParamInPlace` behavioral test's `explicitReverseSeq` case — `reverse([]E(x))` over plain and named `~[]E` sources, which did not compile before the fix.)

**An untyped-int literal in a `~[]E`-locked type-parameter slot is cast to the resolved element type.** A bare untyped-integer literal passed where the parameter is the **element type parameter `E` of a sibling `~[]E`-constrained type parameter** — `Index[S ~[]E, E comparable](s S, v E)` called `Index(s, 2)`, or the variadic element of `Insert[S ~[]E, E any](s S, i int, v ...E)` called `Insert(b, len(b)-1, 0)` (the `slices` shape) — drives C# generic inference from the *literal's own C# type*. Go infers `E` from `S`'s core type (`[]int` → `E` = Go `int` → `nint`), but C# has **no analogue for `~[]E` core-type inference**: the emitted `where S : ISlice<E>` does not flow `S`'s concrete element to `E`, so C# infers `E` SOLELY from the value literal. A bare C# int literal is `System.Int32`, so `Index(s, 2)` with `s []int` made C# infer `E=int`, and `slice<nint>` then failed the `~[]int` constraint — `CS0315` (no boxing conversion `slice<nint>`→`ISlice<int>`), `CS0411` (inference failed), or `CS1503` (arg conversion). go/types has already resolved the literal to `E`'s instantiation (`Info.Types[lit].Type` — `int` for the `[]int` caller, `byte` for a `[]byte` caller, `int64`→`long`, `uint`→`nuint`, …), so `convCallExpr` emits the literal AT that C# type via the shared `castArgToType` plumbing: `Index(s, (nint)(2))`, `Insert(b, len(b) - 1, (byte)(0))`. The **sibling-lock gate** (`typeParamIsSliceElementOfSibling`) is what keeps the footprint minimal and correct: the cast fires ONLY when the parameter's type parameter is the slice-element of another type parameter's `~[]E` constraint — the one shape C# cannot infer. Everything C# already infers correctly is left with its bare literal: a **freely-inferred** type parameter (`First[T any](v ...T)` — `T=int32` satisfies `any`, and the value is identical), one **determined directly by another argument** (`setThrough[P *T, T any](p P, v T)` — C# infers `T` from the pointer), and an **explicitly-instantiated** call (`NewOption[nint](42)` — the type argument is already pinned). A resolved **`int32`/`rune`** kind is skipped even inside the gate (a bare int literal already IS `System.Int32` — the `[]int32` element case stays a plain literal), and a value `convBasicLit` already casts (`(nint)…L` for an out-of-int32 constant) is not double-wrapped (the `wholeExprIsCastOfType` skip in `convExprList`). The literal-constant test reuses `isUntypedNumericConstArg`, the same recognizer the `append`-element and narrow-int casts key off, so a *tightened* local const — already declared at its concrete type — is excluded. This unblocks the whole `slices`-package `Index`/`Insert`/`Replace`/`Contains`-family value-argument seam (cleared the entire CS0315 cluster in the `slices` Phase-4 test host — 53→40 residual errors, the remainder unrelated classes). Proven zero-drift on the behavioral corpus. (Guarded by the `GenericUntypedIntArg` behavioral test — `Index`/`appendAll` over `[]int`, a named `numbers []int`, `[]byte`, and `[]int32` element types with bare int-literal args, which did not compile before the fix; the `[]int32` case proves the no-cast arm.)

**Range-over-func on named/generic Seq types.** Go 1.23's `for v := range seq` (and the two-value `for k, v := range seq2`) on an `iter.Seq[E]`-shaped value emits through golib's yield-adapting `range()` overloads. Three pieces make the named/generic form work: detection unwraps the type's `Underlying()` (a defined or instantiated func type is a `Named`, not a bare `Signature`); a NAMED func type renders as a C# *delegate*, which has no conversion to the overloads' `Action<Func<…>>` parameter — its method GROUP does, so the emission appends `.Invoke`; and because C# cannot infer a type parameter from a method group's parameters, the element types are spelled out from the yield signature: `foreach (var v in range<nint>(countdown(5).Invoke))`. `break` inside the body ends the foreach, which cancels the adapter's producer — the yield function receives `false`, matching Go's semantics; a two-value `range<K, V>` overload adapts pair-yields onto the tuple machinery. One adjacent gate was refined en route: a call's result being a generic instantiation adds explicit type arguments only for conversions and GENERIC callees (`NewOption<nint>(42)` — an untyped-const arg would infer C# `int` where Go infers `nint`), never for a plain function returning a generic named type (`countdown<nint>(5)` was CS0308). (Guarded by the `GenericTypeInference` extensions — a generic `Seq[V]` ranged with `break` and a two-value `KVSeq[K, V]`, values vs Go.)

**An EXPLICITLY-instantiated generic function through a package selector renders its type arguments once.** Go's `pkg.Func[T](…)` is an `IndexExpr` (or `IndexListExpr` for `pkg.Func[K, V]`) whose base `X` is the selector `pkg.Func`. `convIndexExpr`/`convIndexListExpr` renders the `[T]` as `<T>` itself. But the base is *also* a generic-function value, so `convSelectorExpr` — which spells a generic function's inferred type arguments when it appears as a method-group **value** (the `slices.SortFunc(all, slices.Compare)` path, needed because C# can't infer a method group's type parameters) — appended `<T>` a second time, producing `pkg.Func<T><T>()`. Depending on context this surfaced as CS1525 (`reflect.TypeFor[X]()` → invalid expression term), CS0119 (a plain-return generic like `saferio.SliceCap[T]`), or CS8124 (`<T>()` parsed as a one-element tuple) — ~67 errors across encoding/gob, xml, asn1, json, text/template, database/sql/driver, debug/macho·pe·elf, and unique. The index expression now converts its base with a `suppressGenericTypeArgs` context flag, so `convSelectorExpr` skips the value-path append when it is the base of an explicit instantiation (the standalone method-group-value case is unchanged — no flag, still appends). A *local* generic function (`Func[T]()`, base is an `Ident` not a selector) never hit this, since only `convSelectorExpr` appends. (Guarded by the `CrossPkgUser` extension — `CrossPkgLib.Wrap[int](5)` (IndexExpr) and `CrossPkgLib.Pair[string, int](…)` (IndexListExpr), both rendering single type-argument lists, output vs Go.)

## Integer type-parameter conversions route through golib (the `E(100)` family)

C# has no cast to or from a type parameter, so the Go conversions in `rand.N[Int intType]` — `Int(x)`, `uint64(n)` — and an untyped constant compared against the parameter (`n <= 0`, which Go types AS `Int` but C# leaves as `int`, unacceptable to the lifted `IComparisonOperators<Int, Int, bool>`) all failed (CS0030/CS0019). Three coordinated pieces, gated on a constraint whose every type-set term has an **integer underlying** (`typeParamIsInteger`): a conversion **to** the parameter emits golib's runtime-typed `ConvertToType<Int>(…)` (typeof-dispatch that JIT-folds to a single branch per instantiation; signed kinds sign-extend, unsigned zero-extend — Go's exact conversion semantics; a `/*num:**/` wrapper instantiation falls back to a reflection-cached bridge over its `Value` property/ctor); a conversion **from** the parameter to a basic integer emits `ConvertToUInt64<Int>(n)` (plus a plain numeric cast when the target is not `uint64`); and a **constant operand** of a binary op against the parameter materializes via `ConvertToType<Int>(0)` — except a SHIFT count, which Go types independently and the emission already coerces to `int`. Result: `if (n <= ConvertToType<Int>(0)) … return ConvertToType<Int>(ConvertToUInt64<Int>(n) / 2);`. (Guarded by the `GenericTypeInference` extension `halveN` — `~int32 | ~int64` with the compare, both conversions, and a negative value proving sign-extension, values vs Go; clears math/rand/v2's `N`.)

## A named-wide-integer or type-parameter slice index casts to `nint`

Go permits any integer type as a slice/array index, converting it to `int` for the access. The C#
`slice<E>`/`array<E>` indexer takes `nint`, and the existing wide-*basic* index cast already routed
`uint`/`uint32`/`uint64`/`uintptr`/`int64` through `(nint)`. Two more index kinds need it, both from
internal/trace's `dataTable`:
- a **named type over signed `int64`** — `type ProcID int64` indexing `spans[procID]` (CS1503).
  There is no `this[long]` overload, `int64→nint` does not narrow implicitly, and `int64→ulong`
  (which would bind `this[ulong]`) is a signed→unsigned conversion — so it has no bare path.
  `(nint)(procID)` composes as one *user* conversion (named→long) plus one *built-in* (long→nint).
  Every other kind is deliberately **excluded** — no churn, and casting some would even break: an
  *unsigned* named type (`type kindT uint`/uint32/uint64) binds the golib `this[ulong]` overload
  bare, and a nuint-backed wrapper (`uint`/`uintptr`) is *CS0030* under a `(nint)` cast; an
  `int`/`int32`/`nint` underlying narrows implicitly (`type rank int` stays bare). So only signed
  `int64` both *needs* and *accepts* the cast;
- a numeric **type parameter** — `dataTable[EI ~uint64]` doing `d.dense[id]`. A constrained type
  parameter has no C# cast at all, so it routes through golib's `ConvertToUInt64<K>` bridge (the
  `E(100)` integer-type-param family above) and then narrows: `d.dense[(nint)(ConvertToUInt64<EI>(id))]`
  (an arithmetic index `id/8` is still `EI`, so it wraps the same way).

Cleared ~11 of internal/trace's index CS1503/CS0030 (17→6). The companion **shift-*count*** case —
`1 << (id % 8)` where the count is a numeric type parameter, coerced to `int` by `intCastOperand`
(the same coercion the shift-width machinery uses) — routes through the same bridge:
`(uint8)1 << (int)(ConvertToUInt64<EI>(id % 8))` (a bare `(int)(EI)` is CS0030). Cleared internal/trace's
last two shift-count CS0030 (6→4). Guarded by `NamedNumericSliceIndex` (a generic `lookup[K ~uint64]`
indexing by the type parameter and an arithmetic result, a `pick` indexing by a named `int64`, a
`num:nint` `rank` index that must stay bare, and a `bitset[K ~uint64]` shifting by the type parameter,
values vs Go).

## Method-set interface constraints bind the interface directly; pointer instantiations project through the adapter

A type parameter constrained by a **regular method-set interface** (a pure method set, no type-term
unions — go/ast's `walkList[N Node](v Visitor, list []N)`) emits `where N : Node` against the
arity-0 emitted interface. Only union+method **constraint interfaces** take the generic CRTP form
(`ConstraintTest1<ΔT>`); the method-set arm previously emitted the phantom `Node<N>, new()`, which
was doubly wrong: `Node` is emitted arity-0 (CS0308), and the instantiation may itself be an
*interface* (walkList takes `N=Stmt/Expr/Spec/Decl`), which can never satisfy `new()`. The
interface-typed and interface-inheriting instantiations then satisfy the constraint natively
(`Stmt : Node` is emitted inheritance).

A **pointer** instantiation (`walkList(v, n.Names)` with `N=*Ident`) cannot: the `ж<Ident>` box
does not implement the interface — its generated pointer adapter does. The call site projects the
slice element-wise through the adapter, instantiating `N` as the interface itself:

```csharp
internal static void walkList<N>(Visitor v, slice<N> list)
    where N : Node
{
    foreach (var (_, node) in list) {
        Walk(v, node);
    }
}
// call site, N=*Ident:
walkList(v, widen<ж<Ident>, Node>((~n).Names, elemᴛ1 => new IdentжNode(elemᴛ1)));
```

golib's `widen<T, TWide>(slice<T>, Func<T, TWide>)` copies the slice HEADER only — elements alias
the original objects through the shared boxes, so method calls through the projected slice mutate
the real objects. A callee that reassigned `list[i]` itself would not write back; the projection
targets the read/widen shape (Go itself performs the same per-element interface widening inside
the loop). `convertToInterfaceType` supplies the adapter reference and the `GoImplement` recording,
exactly as at scalar `*T→iface` call sites. (Guarded by `GenericInterfaceConstraint` — pointer,
interface, and embedded-interface instantiations of a method-set-constrained generic, calling a
constraint method on the parameter and widening walkList-style, values vs Go; clears go/ast's
CS0308, the go/* toolchain gate.)

## A SELF-REFERENTIAL generic method-set constraint uses a box-wrapping proxy as the type argument

The widen-to-the-interface escape above works only when the constraint interface is **non-generic**
(`N=Node`, so `N` can *be* `Node`). crypto/elliptic's `nistCurve[Point nistPoint[Point]]` — where
`nistPoint[T]` is a **generic, self-referential** method-set interface (`Add(T,T) T`,
`SetBytes([]byte) (T,error)`, …) — cannot: you can't substitute `Point = nistPoint<Point>` (infinite
regress), and the golib box `ж<P224Point>` cannot *nominally* implement `nistPoint<ж<P224Point>>`
(it is a sealed golib type in another assembly, and Go's structural satisfaction has no C# analog).
Four coordinated pieces make it convert **and dispatch**:

1. **The constraint interface is emitted GENERIC.** A method-set interface whose own Go type
   parameter is used in its member signatures carries its `<T>` (and constraints) in C#, exactly like
   a generic struct — `partial interface nistPoint<T> { … T Add(T, T); (T, error) SetBytes(slice<byte> _); }`.
   Without it the declaration is arity-0 yet the constraint that references it spells the arity-1
   `where Point : nistPoint<Point>` (CS0308) and every bare `T` is undefined (CS0246). (Go's
   operator-only constraint interfaces are arity-0 *in Go*, so this is disjoint from the `<ΔT>`
   operator machinery.)

2. **One GENERIC adapter class** implements the outer interface: `nistCurveжCurve<Point> : Curve,
   IжAdapter where Point : nistPoint<Point>` wrapping `ж<nistCurve<Point>>` — NOT a class per
   instantiation. The converter's `GoImplement` records are per-instantiation but all resolve to the
   open form here, so `ImplementGenerator` de-dups on the open `(struct, interface)` pair and forwards
   its type parameters and the struct's own constraint (`GetGenericConstraintClause`). The converter
   composes the reference name+args separately (`nistCurveжCurve<…>`, base+`ж`+iface, then the closed
   args) so the type arguments do not bake into the identifier (the old `nistCurve<…>жCurve` was CS1526).

3. **A self-referential PROXY stands in for the type argument.** For each concrete pointer type used
   to instantiate the generic (`nistCurve[*P224Point]`), the converter renders the type argument as a
   generated proxy `P224PointжnistPoint` (element-simple+`ж`+iface-simple) instead of the box `ж<P224Point>`,
   and records `[assembly: GoImplement<P224Point, nistPoint<P224Point>>(ConstraintProxy = true)]` (the
   interface's own argument is a placeholder). `ImplementGenerator.EmitConstraintProxy` emits:

   ```csharp
   internal sealed class P224PointжnistPoint : nistPoint<P224PointжnistPoint>, IжAdapter {
       private readonly ж<P224Point> m_box;
       public P224PointжnistPoint(ж<P224Point> box) => m_box = box;
       public static implicit operator P224PointжnistPoint(ж<P224Point> box) => new(box);
       public static implicit operator ж<P224Point>(P224PointжnistPoint proxy) => proxy.m_box;
       // T rewritten to the proxy itself; the implicit conversions marshal every T-boundary:
       P224PointжnistPoint nistPoint<P224PointжnistPoint>.Add(P224PointжnistPoint a, P224PointжnistPoint b) => m_box.Add(a, b);
       (P224PointжnistPoint, error) nistPoint<P224PointжnistPoint>.SetBytes(slice<byte> b) => m_box.SetBytes(b);
       // …
   }
   ```

   The proxy implements the interface **over itself**, so `Point = P224PointжnistPoint` satisfies
   `where Point : nistPoint<Point>` (CS0311 otherwise) *and* resolves every `p.Add(…)`/`newPoint().SetBytes(…)`
   call inside `nistCurve`'s body. The implicit `ж<P224Point>`↔proxy conversions do all the T-boundary
   marshalling automatically: each forwarder is a bare `m_box.M(args)` (arguments unwrap to the box on
   the way in, results rewrap to the proxy on the way out — including element-wise inside a
   `(T, error)` tuple), and a value flowing into a `Point`-typed position (`base: Ꮡ(new P224Point(…))`)
   converts implicitly at the site. The proxy forwards to the element's **exported** `ж`-extensions even
   cross-assembly (`m_box.SetBytes` binds nistec's extension from crypto/elliptic).

4. **A `func()`-typed field's method-group initializer is re-wrapped as a lambda.** `nistCurve`'s
   `newPoint func() Point` becomes `Func<P224PointжnistPoint>`, but a method group (`newPoint: nistec.NewP224Point`,
   returning `ж<P224Point>`) cannot convert to it — a C# method-group conversion does not apply the
   user-defined implicit operator (CS0407). Inside a constraint-proxy composite the converter re-wraps
   such an initializer as a lambda, `newPoint: () => nistec.NewP224Point()`, whose *return* position
   does apply the conversion.

5. **The proxy forwards the WHOLE method set, embedded interfaces included.** A Go constraint
   interface may embed others, and Go embedding emits as C# interface inheritance — so the members a
   proxy must forward are not the ones the constraint DECLARES but the ones its method set CONTAINS.
   The emitter walked `GetMembers()` only, which is the declared half, and any member reached through
   an embed was simply absent: the proxy did not implement its own interface (CS0535, one per
   inherited member). It surfaced at full size in `net/http`, whose `clientserver_test.go` declares

   ```go
   type TBRun[T any] interface {
       testing.TB
       Run(string, func(T)) bool
   }
   ```

   giving proxies for `*testing.T` and `*testing.B` that forwarded `Run` and were each missing all 18
   members of the embedded `testing.TB` — 36 diagnostics, and the last wall but one in front of a
   1,352-verdict suite. `AllInterfaces` is already transitive, so a two-level embed needs no recursion
   of the emitter's own.

   Each member is qualified by **its own declaring interface**, not by the constraint: C# explicit
   interface implementation must name the interface that declares the member, so `void Derived.M()` is
   CS0539 when `M` comes from `Base`. A generic embed is closed over the proxy exactly as the
   constraint itself is (`Bar<T>` embedded in `Foo<T>` forwards as `Bar<proxy>.M`), reusing the same
   `RenderWithProxy` substitution. The constraint's OWN members are emitted first in declaration order,
   so a proxy that embeds nothing is byte-identical to what the emitter always produced; the embedded
   sets follow, ordered by rendered reference so the emission does not depend on the order
   `AllInterfaces` happens to report. This also brings the constraint-proxy path into line with the
   interface-ADAPTER path beside it, which had walked base interfaces from the start.

(Guarded by `GenericPointerInterfaceImpl` — a self-referential `curve[Point point[Point]]` implementing
`Curve` via pointer receiver, instantiated two ways, with a `newPoint func() Point` field and a
`(T, error)`-returning constraint method, values vs Go — and by `ConstraintProxyEmbeddedInterface` for
the method-set walk: a constraint embedding `Middle` embedding `Base`, so one member arrives one level
deep and another two, recorded from two instantiation sites to exercise the per-pair de-duplication,
output-compared vs `go run`. Reverting the walk reproduces CS0535 on `Middle.Size()` and `Base.Name()`
— both levels. Embedding the constrained generic and greening the whole crypto-curve family is the
next subsection.)

### A func LITERAL at a proxied delegate position declares its parameters AT the proxy

Item 4 above moves a METHOD GROUP's T-boundary into a lambda body, because a method-group conversion
will not apply the user-defined conversion. A func literal meets the same wall one position further
in and cannot take that remedy — it already *is* the lambda, and it renders its own parameter list
from the Go signature. At `T = ImplжConstrained` a `func(t T, mode int)` argument emits
`(ж<Impl> t, nint mode) => …` against a delegate requiring `Action<ImplжConstrained, nint>`, and
**C# applies no user-defined conversion at a parameter DECLARATION**: CS1678 + CS1661, one pair per
call site. net/http's suite is written on this shape throughout —
`run[T TBRun[T]](t T, f func(t T, mode testMode), opts ...any)` — and it was 48 of its 81 body
diagnostics.

The remedy is the same principle: **move the conversion to a position C# performs it.** The parameter
is declared at the proxy under a synthesized name and the body opens with the natural-typed alias:

```csharp
run<TжTBRun>(Ꮡt, (TжTBRun tΔ1Δp, testMode mode) => {
    var tΔ1 = (ж<testing.T>)tΔ1Δp;      // the proxy's own implicit operator
    …                                    // body unchanged, byte for byte
});
```

Everything after that line is the body exactly as it would have been emitted, so no member access,
capture, or nested literal inside it renders differently. Declaring the parameter at the proxy and
letting the body use it *directly* would not work: the forwarders are explicit interface
implementations, reachable through a type parameter's constraint but not by member lookup on the
concrete proxy type.

Restricted to a parameter whose type **is** the proxied type parameter exactly. One that merely
MENTIONS it (`[]T`, `map[K]T`, `func(T)`) is excluded — `slice<ImplжConstrained>` and
`slice<ж<Impl>>` are distinct instantiations with no conversion between them, so no single assignment
could stand in the prologue, and guessing would trade a clear diagnostic for a wrong one.

⚠ **Both halves key on the RENDERED name.** A literal's signature is generated from synthesized vars
carrying the shadow-renamed name, so keying the proxy map on the Go name misses every renamed
parameter — and misses it *asymmetrically*, because the body prologue reads the same map from the
AST, where the Go name is present. The first cut did exactly that: it emitted the prologue while
leaving the declaration at its natural type, producing a local with the same name as the parameter
beside it. net/http is entirely the renamed case (its inner `t` shadows the outer), so the guard
carries both spellings and the un-renamed one alone would have passed over the real defect.

### The anchored adapter REFERENCE keeps the shadow marker

The `-tests` metadata-anchored resolution composes the adapter class reference a cast site will use
(`anchoredAdapterMemberName`) while go2cs-gen composes the class it emits. The two must agree
character for character, and they disagreed on the shadow marker: the generator names a local adapter
from `adapterBaseName` — the C# type name verbatim, `Δhandler` — and a foreign one from
`GetSimpleName(structName)`, neither of which strips it, while the reference side stripped it and
named a class that is never emitted. net/http's internal test variant declares
`type handler struct{ i int }` (server_test.go), shadow-renamed to `Δhandler`, so the generator minted
`ΔhandlerжΔHandler` and every cast site referenced `handlerжΔHandler` — CS0426 ×9.

The rule the strip violated: **the marker belongs to the C# IDENTITY of the type, not to a rendering
convention.** `adapterStructKey` strips it for GROUPING, which is right and unchanged — a collision
group must not depend on which side got renamed — but that key must not double as the emitted name.
Only the `-tests` anchored path was affected: a production conversion resolves through
`adapterResolvedName`, which never stripped, so the corpus could not move (and CNR confirms it did
not). The measured shape here also **corrects a plausible-looking diagnosis** worth recording: the
symptom reads as an adapter minted for one test variant and referenced from the other, and it is not
— the record is correctly bridge-anchored in `package_info_internal_test.cs` and the class is minted
in `http_internal_test_package`, exactly where the reference looks for it. Only the NAME differed.

⚠ One adjacent surface is deliberately NOT addressed and is worth naming, since a reader meeting it
will otherwise read it as this defect: a `T` RETURNED out of a constrained generic into concrete code
arrives as the proxy TYPE, whose forwarders are explicit interface implementations and so are
unreachable by member lookup there (`r := second(p); r.Name()` — CS1929, resolving instead to the
element's own extension whose receiver it cannot satisfy). The proxy carries an implicit conversion
back to `ж<element>`, but C# does not apply a user conversion during member lookup. Nothing in the
corpus or in `net/http` reaches it; the guard's `second` builds its result inside the generic context
on purpose.

## A struct embedding the constrained generic promotes its members — three residual crypto-curve fixes

crypto/elliptic's `p256Curve struct { nistCurve[*nistec.P256Point] }` — a **non-generic** struct
embedding a **concrete instantiation** of the self-referential-constrained generic above — must PROMOTE
`nistCurve`'s internal fields (`newPoint`, `params`) and methods (`Add`/`Double`/`Params`/`ScalarMult`/…)
onto `p256Curve`, exactly as an embed of a plain struct does, so `p256.params = …` binds and the generated
`p256Curve→Curve` interface adapter can forward `curve.Add(…)` to the promoted shim. Because the type
argument is the box-wrapping proxy of the previous subsection, its rendered name **embeds the marker glyph
`ж`** (`nistCurve<P256PointжnistPoint>`) — the thread that runs through all three fixes that made the whole
crypto-curve family (elliptic, ecdh, nistec) COMPILE (+3 packages):

1. **The proxy marker glyph `ж` is not a pointer prefix.** The generator's simple-name / underlying-name
   helpers (`GetSimpleName`, `GetUnderlyingTypeName`) detected a pointer type `ж<T>` by scanning for a
   *bare* `ж` and slicing from it. The proxy's own name embeds that glyph mid-identifier
   (`P256Point`**`ж`**`nistPoint`), so an embed typed `nistCurve<P256PointжnistPoint>` was mis-sliced into
   garbage (its simple name became `oint.Value`, its underlying name an unresolvable string) and the embed
   promoted **nothing** (CS1061 on `params`, CS1929/CS1501 on every forwarded method). Both helpers now
   match the pointer prefix as the two-character `ж<`, so a marker embedded in an identifier is left intact.

2. **A generic-instantiation embed resolves to its declaration and substitutes its type arguments.** An
   embed of a generic INSTANTIATION (`nistCurve<P256PointжnistPoint>`) resolves to the generic DECLARATION
   (`nistCurve<Point>`) by base-name + arity (`FindStructDeclaration` — an instantiation can never
   string-match a declaration that carries its type PARAMETERS), and a generic struct's extension methods
   now match on the type-parameter-bearing receiver (`nistCurve<Point>`, not the bare `nistCurve`). The
   promoted field and method signatures are harvested from the declaration, so they carry its type
   PARAMETER (`Func<Point>`, `pointFromAffine` returning `(Point, error)`); the template rewrites each to
   the instantiation's type ARGUMENT before emission —

   ```csharp
   internal ref global::System.Func<P256PointжnistPoint> newPoint => ref nistCurve.newPoint;
   internal static (P256PointжnistPoint p, error err) pointFromAffine(this ref p256Curve target, ж<bigꓸInt> Ꮡx, ж<bigꓸInt> Ꮡy)
       => target.nistCurve.pointFromAffine(Ꮡx, Ꮡy);
   ```

   — so no promoted member references the out-of-scope `Point`. (The member ACCESS hop keeps the bare
   property name `nistCurve`; only the emitted TYPE is substituted.) When the ENCLOSING struct is itself
   GENERIC — `wrapped<T>` embedding `tag<T>` (the `GenericStructFields` guard) — the promoted method is
   a GENERIC extension method carrying the struct's own type parameters (`static T show<T>(this wrapped<T>
   target) => target.tag.show();`, the substitution then an identity `T`→`T`), else the `T` in the
   receiver and return is an undefined type name (CS0246).

3. **The constraint proxy imports its element's package namespace.** The proxy forwards each interface
   method to the boxed element's box extension methods (`m_box.Bytes()`), which live in the element type's
   PACKAGE class (`nistec_package`, namespace `go.crypto.@internal`). The `[assembly: GoImplement<…>(ConstraintProxy = true)]`
   attribute driving the proxy sits in `package_info.cs`, whose usings never cover a FOREIGN element, so the
   forwarders bound nothing (`ж<P224Point>` "has no `Bytes`", CS1929/CS1501). `EmitConstraintProxy` now emits
   `using <element-namespace>;` for the box element's namespace.

4. **An open-generic interface cast is CONVERTED but not RECORDED.** Inside a generic method the receiver
   itself is cast to the interface — crypto/ecdh's `return newBoringPrivateKey(c, …)` with `c *nistCurve[Point]`.
   The converter must still WRAP it in the generic adapter (`new nistCurveжΔCurve<Point>(Ꮡc)` — the adapter
   the CLOSED per-instantiation records already generate), but must NOT RECORD it as an implementation: a
   record emits `[assembly: GoImplement<nistCurve<Point>, ΔCurve>]`, whose type-PARAMETER argument `Point`
   is out of scope in an assembly attribute (CS0246). `convertToInterfaceType` now skips the record for an
   open-generic target while still firing the adapter-wrapping conversion.

(Fix 2 is guarded by the `GenericEmbedPromotion` behavioral test — a non-generic struct embedding a concrete
`curve[*p224]` over a self-referential proxy: reading a promoted internal field, calling a promoted method
whose parameter is the type argument (passed the promoted proxy-typed field), and reaching the promoted
methods through a non-generic interface adapter, values vs Go. Fixes 3 and 4 need a cross-package element /
a generic-method interface cast the single-package baseline cannot express; they are validated by the census
— elliptic, ecdh, and nistec now emit their DLLs, 254 → 257 packages.)

## Constraint-only type parameters need explicit type arguments

Go infers a type parameter that appears only in *constraints* through core types — `func Twice[S ~[]E, E Integer](s S)` infers `E` from `S`'s underlying element; the `slices` package's whole `Sort[S ~[]E, E cmp.Ordered] → pdqsortOrdered` chain relies on this. C# never infers a type parameter that does not appear in the parameter list (CS0411 — at *every* call site, concrete instantiations included). When the callee declares such a constraint-only type parameter, the converter renders the call's type arguments explicitly from the instantiation `go/types` already resolved (`info.Instances`): `Twice<Point, int32>(p, 2)` at a concrete site, `Scale<S, E>(s, c)` inside a generic body. Calls to generics whose every type parameter is argument-visible keep their bare Go-shaped form — C# infers them as Go does, no churn. (Guarded by the `GenericTypeInference` extension — a constrained `S`/`E` pass-through chain plus a concrete call to a constraint-only-param generic, values vs Go; clears the 14 CS0411s in the slices/maps wave.)

The same explicit-type-argument rule applies to a generic function referenced as a **method-group value**, not just a call. `slices.SortFunc(all, slices.Compare)` (runtime/pprof) passes `slices.Compare[S ~[]E, E cmp.Ordered]` as `SortFunc`'s comparison delegate; C# cannot infer a generic method group's constraint-only `E` when converting it to `Func<…>` (CS0411). `convSelectorExpr` now spells the arguments on the selector — `slices.Compare<slice<uintptr>, uintptr>` — when the selector is NOT the callee of a call (`!context.isCallExpr`, so convCallExpr's own type-arg site still owns the call form) and `info.Uses[Sel]` is a generic function with an `info.Instances` instantiation. Byte-identical across the behavioral corpus and across an A/B of pprof+slices+sort+maps+cmp+net+go/types (a single line moves — the `slices.Compare` argument; every `Compare(...)` **call** stays bare). GUARD OWED — the shape needs a cross-package generic function with a constraint-only type parameter passed as a method-group value, which the single-package baseline cannot express.

**The bare-IDENT variant is no longer latent** (2026-08-26, the generic-inference arc that unblocked `slices`). `convIdent` now carries the same call-vs-value flag `convSelectorExpr` always had — `IdentContext.suppressGenericTypeArgs`, set by convCallExpr for a call's CALLEE and by convIndexExpr/convIndexListExpr for the base of an explicit instantiation — so a same-package generic function passed as a method group (`apply(s1, Reverse)` against `Reverse[S ~[]E, E any]`, slices' `TestInference`) spells its arguments out (`reverse<slice<nint>, nint>`) exactly as the qualified form does, and the two spellings of one Go reference cannot disagree. Without the flag there was no way to add the append without also writing every direct call in the corpus out longhand; with it, the value form is the only one that moves. The append rides on whichever spelling the ident's own tail arms produce (a `-tests` Δ-rename, a white-box bridge qualification), so a renamed generic function passed as a method group is covered too.

Three further shapes in the same family were fixed with it, all first measured as compile errors in `slices`' converted test suite (16 errors across roughly six exported generics, every one CS0411/CS0305 or a CS1503 cascade behind one):

- **An explicitly-instantiated generic function argument is still a method group.** `EqualFunc(s1, s2, equal[int])`, `CompareFunc(s1, s2, cmp.Compare[int])`, `CompactFunc(s, equal[int])`, `equalToCmp(equal[int])` — writing the type arguments fixes the group's *shape*, not its C#-inference status, so the enclosing generic call still needs its own arguments spelled. `exprIsMethodGroup` met an `*ast.IndexExpr`, matched neither of its two cases, and answered "not a method group"; it now peels `ParenExpr`/`IndexExpr`/`IndexListExpr` (indexing a function *value* is not legal Go, so peeling can never reclassify an ordinary map/slice index). Eight of the sixteen errors.
- **A type parameter reachable only through an UNSUPPLIED parameter position.** `Insert[S ~[]E, E any](s S, i int, v ...E)` called as `Insert(s, 0)` hands C# an empty `params Span<E>`, and `E` is inferable from nothing — while `calleeHasConstraintOnlyTypeParam` cannot see it, because `E` *does* appear in a parameter type. `calleeTypeParamUnsuppliedByCall` asks the question C# inference actually asks — which parameter positions did this call supply — and since every non-variadic parameter is always supplied in a well-typed Go call, it can fire on nothing but an empty variadic. `Insert(s, 0, 7, 8)` keeps its bare form.
- **A PARTIAL explicit instantiation.** Go allows a written prefix and infers the rest through core types (`Equal[Slice]` against `Equal[S ~[]E, E comparable]`, slices' own `iter_test`); C# has no partial instantiation, so the prefix alone is CS0305, "requires 2 type arguments". `completedInstantiationTypeArgs` replaces the written list with the resolved one *only* when the resolved list is longer — a complete instantiation, which is nearly all of them, renders verbatim and byte-identically. The comparison is made after erasure filtering on both sides, so an erased pointer-core position cannot make a complete instantiation look partial.

Every one of the four is an ADDITION to the existing trigger set rather than a replacement, and each is gated on a property that is arithmetic rather than heuristic (is this argument a function reference; did this position receive an argument; is the written list shorter than the resolved one). That is what keeps the footprint at exactly the shapes that were failing: **CNR at 645 behavioral packages moves only the guard project itself, and a seeded reconvert of the whole converted standard library re-emits 4,173 artifacts byte for byte** (0 changed, 0 new; marker gate 0 violations across 78 marked files) — `slices` and `maps` included, whose own production code leans hardest on the inference this arc is about. Guarded by the `MethodGroupGenericArg` extension, which fails on the pre-change converter with exactly the `slices` error set — CS0411 ×4 (instantiated method-group argument, empty variadic, bare-ident generic value ×2) plus CS0305 ×1 (partial instantiation) — and passes after.

### A NAMED composite argument at an UNNAMED composite parameter

`keys[V any](m map[int]V)` called `keys(names)` with `type Names map[int]string`: Go infers `V` through the named
type's underlying type. The named type emits as a wrapper struct (`partial struct Names`) that reaches
`map<nint, V>` only by a user-defined implicit conversion, and C# type inference never looks through a user-defined
conversion, so the bare call is CS0411 (the hashset module's test, 2026-10-03). `callPassesNamedCompositeToCompositeParam`
adds the shape to the trigger set: an argument whose type is a NAMED map, slice, channel or array, at a parameter
whose declared type is an UNNAMED map, slice, channel or array mentioning a type parameter. The call then spells
the resolved arguments, `keys<@string>(names)`, `total<float64>(scores)`, `drain<nint>(feed)`, `first<nint>(grid)`,
and the conversion applies once the parameter type is closed.

Three neighbours keep their bare form. An unnamed argument (`keys(map[int]string{...})`) already has the
parameter's shape. A named argument at a bare type-parameter position (`ident[T any](v T)`) infers the wrapper
itself. A named FUNC type emits as a delegate that the parameter's delegate type infers from, measured compiling
before the fix. The type-aware census of the shape read 0 sites in the converted standard library (production on
three targets, tests on two) and 0 across the behavioral corpus, so the footprint is the guard alone. Guarded by
`namedCompositeGenericArg_test.go` and the `NamedCompositeGenericArg` behavioral test, which fails on the pre-change
converter with CS0411 x4, one per composite kind.

## The `comparable` constraint

Go's built-in `comparable` admits every `==`-able Go type — numerics, strings, pointers, channels, and comparable structs/arrays/interfaces. No C# constraint can express that set: golib's old `comparable<T>` CRTP interface was implemented by *nothing* (every real instantiation failed — `maps.Keys[M ~map[K]V, K comparable]` could not be used at all), and lifting `IEqualityOperators` would reject structs, which Go admits. A `comparable` type parameter therefore emits **no C# constraint at all** — no `where` clause — relying on the two facts that make it sound: Go's checker already validated every instantiation, and emitted equality on type parameters routes through `AreEqual`, never operator `==`.

Until the B1 per-kind box split (2026-08-26) this arm emitted `where K : /* comparable */ new()`. The `new()` was a holdover nothing needed — golib's `@new<T>` constructs through the runtime, and no comparable-constrained body in the corpus constructs its parameter — and the split turned it from dead weight into a defect: Go pointers are comparable, a Go pointer type argument instantiates at the abstract `ж<T>`, and no abstract class satisfies a constructor constraint. `unique`'s `HashTrieMap[*abi.Type, any]` was the corpus witness (CS0310); `slices`/`maps`/`cmp` instantiations at pointer element types were the latent class behind it. Guarded by the corpus compile plus the `Constraints`-family behavioral goldens, which now pin the clause-free form.

An interface that **embeds** `comparable` inherits that fact whole, and both sides of the emission
have to say so. `type netipTypeCmp interface { comparable; netipType }` (net/netip's `fuzz_test.go`)
disagreed with itself: the DECLARATION appended a bare `comparable` to the C# base list — that
unimplementable generic named with no type argument, CS0305 — while the CONSTRAINT decided the
interface was not a method set and took the generic CRTP form `netipTypeCmp<P>` against a declaration
emitted arity-0, CS0308. `comparable` contributes no methods, so it is dropped from the base list
(leaving the interface's C# surface exact) and DISCOUNTED when deciding whether an interface is a
pure method set — which puts the constraint in the arity-0 `where P : netipTypeCmp` form the
declaration actually emits. An interface mixing `comparable` with a real type-term union still
restricts its type set and keeps the generic treatment.

`AreEqual` itself is not a performance tax on that path: a generic overload `AreEqual<T>(T, T)` — automatically preferred by overload resolution exactly where both operands share the type parameter — takes `EqualityComparer<T>.Default.Equals` for value-type arguments, which the JIT specializes per type and devirtualizes to the type's own `IEquatable<T>` (operator-comparable speed, no reflection or boxing; golib wrappers emit `operator ==` and `Equals` as consistent pairs, so semantics match). Reference/interface type arguments delegate to the reflective `AreEqual(object, object)` overload, preserving its typed-null and runtime-type semantics. (A constraint-differentiated overload pair is not expressible — C# treats `where` clauses as outside the signature, CS0111 — and a source-generated `==` twin is unnecessary given the `EqualityComparer<T>.Default` JIT intrinsic.) (The behavioral `GenericVariadicFunc` golden captures the erased form with unchanged output.)

**Floating-point equality follows Go's IEEE-754 `==`, not `Equals`.** The `Equals`-based fast path
above is wrong for exactly one family: `double`/`float` report `NaN.Equals(NaN)` as *true* (and
`Complex`/golib `complex64` inherit that componentwise), while Go's `==` — the operation `AreEqual`
stands in for — is IEEE: NaN compares unequal to everything, itself included. That inverted every
generic NaN probe of the `x != x` form: `cmp.isNaN` emits `!AreEqual(x, x)`, so `cmp.Less` lost its
NaN-first ordering (sort's `TestFloat64s` — `Float64s` → `slices.Sort` produced a NaN-scrambled
order) *and* `cmp.Compare` reported NaN equal to everything, which let the mis-sort slip past
`TestSortFloat64sCompareSlicesSort`'s own equality check. The generic overload now special-cases
`double`, `float`, `complex128`, and `complex64` to the operator compare (JIT-constant `typeof(T)`
guards, box-cast elided). The reflective object overload (boxed/interface comparisons) was already
IEEE-correct and stays untouched: on .NET 7+ the primitives DECLARE `op_Equality` (the
`IEqualityOperators` implementation), so its cached operator lookup finds the real operator rather
than falling to `Equals`. Concrete (non-generic) float comparisons were always correct — `f != f`
emits the C# operator directly. (Guarded by the `ReverseSortNaNOrder`
behavioral test — generic `isNaN`/`less`/`eq` legs over `float64`/`float32`/`complex128`/
`complex64`, boxed-`any` NaN equality, and a NaN-aware interface sort, values vs Go.)

## Generic struct equality is decided per FIELD, not per type parameter
A generic converted struct's synthesized `Equals` (see [Struct Types](struct-types.md#struct-types)) was gated on
the struct's TYPE PARAMETERS: unless every parameter carried an `IEqualityOperators`-implementing
constraint (and, stricter still, every *constraint* of every parameter implemented it), the whole
struct's `Equals` body was the constant **`false /* missing equality constraints */`**. Since a
`comparable` parameter deliberately emits no C# constraint (previous subsection),
essentially every generic struct in the corpus — all 22 generic converted type declarations at the time
of the fix — carried a constant-false `Equals`, breaking equality that never depended on the
parameter at all. `unique.Handle[T]`'s only field is `*T` (`ж<T>`), whose pointer-identity `==` is
valid for **every** T, yet no two handles ever compared equal — directly contradicting the type's
documented contract ("two handles compare equal exactly if the values used to create them would");
`internal/weak.Pointer[T]`'s only field does not mention T at all. Even
`internal/trace.dataTable[EI, E]` — whose `EI` explicitly lists `IEqualityOperators` — failed the
every-constraint quantifier because `IAdditionOperators` and its siblings do not *themselves*
implement the equality interface.

The gate now decides **per member** (`GetEqualityFallbackMembers` in
`StructDeclarationSyntaxExtensions`): a member whose type supports `==` independent of the
unconstrained parameters keeps the same `this.f == other.f` compare a non-generic struct emits, and
only a member whose type IS an unconstrained type parameter falls back to golib's
**`AreEqual`** — the identical routing the converter emits for Go `==` on any type-parameter
operand, giving `EqualityComparer<T>.Default` speed on value types while preserving IEEE float
semantics (raw `EqualityComparer` reports NaN equal to itself, inverting Go — see the
floating-point note above) and typed-null/runtime-type semantics for reference and interface
instantiations. Real emissions (from the converted stdlib's generated sources):

```csharp
// unique.Handle<T> — ж<T> has pointer-identity == for every T:
public bool Equals(Handle<T> other) =>
    this.value == other.value;

// database/sql.Null<T> — mixed: the T field routes through AreEqual, the rest keep ==:
public bool Equals(Null<T> other) =>
    global::go.builtin.AreEqual(this.V, other.V) &&
    this.Valid == other.Valid;

// net/http.mapping<K, V> — golib slice/map fields carry their own ==, so no member falls back:
public bool Equals(mapping<K, V> other) =>
    this.s == other.s &&
    this.m == other.m;
```

The member classifier asks only "does `==` COMPILE for this member type": a type parameter
qualifies through ANY `IEqualityOperators`-implementing constraint (matching C# operator
resolution, not the whole-struct gate's every-constraint test); reference types (classes incl.
`ж<T>` and `unsafe.Pointer`, interfaces, arrays, delegates), enums, pointers, and built-in value
types always qualify; a converted struct qualifies by its **attribute** — both struct templates
emit a same-type `operator ==` unconditionally, and for a struct of the *same compilation* the
attribute is the only visible evidence, because that operator does not exist yet while the
generator runs; any other value type qualifies only by actually declaring a same-type
`op_Equality`. Structs that passed the old whole-struct gate, and every non-generic struct, emit
byte-identical bodies to before — the fallback set is computed only when the gate fails.
`GetHashCode` needs no matching change (`golib.HashCode.Combine` always compiled and hashes
consistently with both compare forms). (Guarded by the `GenericStructEquality` behavioral test —
the `Handle` pointer-identity shape, the plain-T fallback shape, a T-independent-field struct, the
`Null`-shaped mix, a nested generic struct field, and a generic struct as a map key, all
output-compared vs Go.)

## A generic struct implementing an interface BY VALUE partials at its OPEN definition
A Go method on a generic type is declared for every instantiation, so `func (g G[T]) M()` makes
`G[int]`, `G[string]` and `G[G[int]]` all satisfy an interface with `M`. The converter records a
`[assembly: GoImplement<…>]` per instantiation it sees, and `ImplementGenerator`'s value-form arm
wrote one `partial struct` per record, spelled with the record's TYPE ARGUMENTS:
`partial struct G<IntPtr> : I`. C# reads that argument list as a **type-parameter list**, so the
declaration disagrees with the converter's own `partial struct G<T>` (CS0264) and the mismatched
parts stop merging — every member the template writes then lands in the containing **static**
package class instead (CS0715 on the operators, CS0708 on `Equals`/`GetHashCode`/`ToString`,
CS0563 and CS0540 in the cascade). The arm now emits ONE partial against the open definition, keyed
by `(OriginalDefinition, interface)` so all instantiations of a pair fold into it; the member and
value-pair dedupe indexes key on the same open form, since two interfaces over one open generic
share a single partial. Constraints are deliberately omitted — a partial declaration may leave them
off and they merge from the converter's declaration, so omission can never raise CS0265. The
pointer-adapter arm had always done this (`emittedGenericPointerAdapters`, crypto/elliptic's
`nistCurve[Point]`); this is its value-form sibling.

Behind it sat a second, independent defect in the shared `GetSimpleName` helper, and it is the one
that explains why the two packages holding this class both name their generic with a **single
letter**. Asked to drop a type-argument list, the helper tested `typeName.IndexOf('<') > 1` — so
`G<T>`, whose `<` sits at index 1, kept its arguments. `StructTypeTemplate` derives the constructor
name from that call, and emitted `public G<T>(NilType _)`, which is not a constructor to C#: the
`partial struct G<T>` scope never opens and the same spill follows. Every multi-character generic
in the corpus (`meta<T>`, `nistCurve<Point>`, `Handle<T>`) cleared the guard, which is why this
survived to the first single-letter one. The guard is now `> 0` and indexes the *simple* name
rather than the full one — the latter also closes a latent, currently unreached miscut on a dotted
generic (`a.Map<K, V>` indexed at 5 into an 8-character `Map<K, V>`, yielding `Map<K`).

Measured on `internal/reflectlite` (`type B[T any] struct{}`) and `runtime/debug`
(`type G[T any] struct{}` with `var dummy I = G[int]{}` and `var dummy2 I = G[G[int]]{}`), the two
packages the board recorded behind one CS0715 root. `runtime/debug` moves from build-blocked to a
measured **2 of 9**; `internal/reflectlite` clears this root and stops on five unrelated ones.
Guarded by the `GenericValueInterfaceImpl` behavioral test — a single-letter generic held as an
interface at three instantiations, a sibling type named exactly like the type parameter (the
`runtime/debug` shape that made the spilled members render as `debug_test_package.T`), struct
equality, struct-versus-interface comparison, and interface dispatch over a mixed slice, all
output-compared against `go run`.

## The `string | []byte` union
C# generic constraints are conjunctive ("and"), so they cannot express Go's `string | []byte` union directly. The two members share no operators (the union is neither comparable nor additive), so a conforming body may only use the read operations common to both — indexing, `len`, and sub-slicing. These are captured by the golib read-only byte-sequence interface [`IByteSeq`](https://github.com/ritchiecarroll/go2cs/blob/master/src/core/golib/IByteSeq.cs), which both `@string` and `slice<T>` implement; the converter emits it for the union and suppresses the (spurious) lifted operator constraints:
```go
func HashStr[T string | []byte](sep T) uint32 { /* uses sep[i], len(sep) */ }
```
```csharp
public static uint32 HashStr<T>(T sep)
    where T : /* string | []byte */ IByteSeq<T, byte>, new()
{ /* … */ }
```
The constraint is **self-referential** — the C# rendering of the CRTP shape, `IByteSeq<TSelf, T> : IByteSeq<T> where TSelf : IByteSeq<TSelf, T>`, instantiated at the type parameter itself. `@string` implements `IByteSeq<@string, byte>` and `slice<T>` implements `IByteSeq<slice<T>, T>`, so the sub-slice indexer returns the **concrete** sequence type rather than the interface. That extra type parameter exists purely to keep the union allocation-free (next subsection).

`len` resolves through a `len<TSeq>(TSeq) where TSeq : IByteSeq` overload that is dispreferred for concrete `slice<byte>`/`@string` arguments, so existing call sites keep their specific overloads (no ambiguity). Go's `[]byte(s)` and `string(s)` over a constrained value render as the golib extensions `s.ToSlice()` and `s.ToGoString()`, each generic over the caller's concrete type; both preserve Go's per-instantiation semantics exactly (`[]byte([]byte)` shares its backing, `[]byte(string)` copies, `string(string)` is free, `string([]byte)` copies). The behavioral test `StringByteUnionConstraint` exercises both the `string` and `[]byte` instantiations across all of these.

### Allocation-free union-constrained bodies
The interface was originally single-parameter (`IByteSeq<T>`) with an `IByteSeq<T> this[Range]` sub-slice indexer. Every member whose signature named the sequence type therefore forced a **box**, and Go bodies over this union sub-slice constantly:

| Emitted shape | Boxed | Why |
|:--|--:|:--|
| `((bytes)(s[a..b]))` | 48 B (`slice<byte>`) / 24 B (`@string`) | the range indexer returned the INTERFACE, so the struct result was boxed — and the cast then unboxed it |
| `len(s)` | one box per call | the overload took `IByteSeq<T>`, an interface parameter |
| `new slice<byte>(s)` — Go's `[]byte(s)` | one box per call | the constructor took `IByteSeq<T>` |
| `new @string(s)` — Go's `string(s)` | one box per call | the constructor took `IByteSeq<byte>` |

A `parseRFC3339`-shaped body (seven sub-slices, eight `len` calls, six `[]byte(s)` conversions per parse) measured **720 B/parse** on the `slice<byte>` instantiation and **776 B/parse** on `@string` — for Go code that allocates nothing. The remedy is one idea applied four times: *never name the sequence type as an interface in a signature a generic body reaches*.

- **Sub-slicing** moves to the self-referential `TSelf this[Range]`. Both implementers' public range indexers already return their own type, so this is satisfied implicitly, and the call becomes a `constrained.` direct dispatch on the value type. The result IS the type parameter, so the converter emits the range expression bare (above).
- **`len`** takes the constrained type parameter (`len<TSeq>(TSeq) where TSeq : IByteSeq`) instead of the interface.
- **`[]byte(s)` / `string(s)`** become the `ToSlice`/`ToGoString` **extension methods** rather than constructors. C# has no generic constructor, so a constructor can only accept the interface; and a static factory cannot even be *named* here, because converted code carries `using static go.builtin`, which shadows the `slice` and `@string` type names with the builtin conversion methods (`slice<byte>.From(s)` is CS0119 — "is a method, which is not valid in the given context"). An extension call is member access on the receiver, so it sidesteps both problems. Each folds `typeof(TSeq) == typeof(…)` to a per-instantiation constant, so the sharing case reduces to a field copy.

Measured after: **0 B/parse** on `slice<byte>`, and 776 → 416 B/parse on `@string` (what remains there is the `byte[]` each `[]byte(string)` conversion must materialize — Go copies too). Guarded by GolibTests `ByteSeqAllocationTests`, which asserts the measured bytes and carries a deliberately-boxed control that must still report the old 720 B/parse. That control pins its box behind a `[MethodImpl(NoInlining)]` boundary on purpose: the JIT elides a box/unbox pair written adjacently in one method, so an inline control would report 384 B/parse and understate what the redesign is worth. The real interface indexer boxed inside a callee and returned, which is why the cost was really paid.

## Explicit type arguments come from the callee's instantiation
A generic function's explicit type arguments are read from the CALLEE's resolved instantiation (`info.Instances`), not from the RESULT type's arguments -- the two lists differ whenever the callee has more type parameters than the result names. reflect's `rangeNum[T, N](num N) iter.Seq[T]` called `rangeNum[int8](v)`: the result `Seq[T]` carries ONE argument where the method needs TWO (CS0305). The result's own arguments still gate *whether* to emit, so a generic callee returning a plain named type keeps C# inference:
```csharp
return rangeNum<int8, int64>(v.Int());
```
Guarded by `GenericTypeInference` (`seqOf[T ~int64, N ~int32 | ~int64](n N) Seq[T]`).

## A CONSTANT argument that fixes a type parameter is retyped to the instantiation
Go infers a type parameter from an untyped constant's default type; go2cs then maps that Go type to C#. The two do not meet: an untyped int defaults to Go `int`, which is go2cs `nint`, but the constant emits as a bare C# literal whose own type is `int` (System.Int32). C# infers the type argument from the ARGUMENT, so `wantValue(0)` makes C# choose `T = int` where Go chose `nint`.

Most such calls are fine and deliberately stay bare, because C# repairs the mismatch wherever an implicit conversion bridges it -- `int` -> `nint` is implicit, so a result that IS the bare type parameter converts at the use site. What cannot be repaired is an **invariant** position: C# generics have no variance for these, so `Action<int, bool>` is not `Action<nint, bool>` and `slice<int>` is not `slice<nint>`. Wherever the type parameter reaches a CONSTRUCTED type, the wrong instantiation is terminal (CS1503/CS0315/CS0411).

Two gates therefore retype the constant to the C# spelling of the type Go resolved (`untypedIntGenericArgCastType`, applied through the per-argument `castArgToType` plumbing):

1. **The sibling `~[]E` lock.** `Index[S ~[]E, E comparable](s S, v E)` (slices): Go fixes `E` from `S`'s core type, but `where S : ISlice<E>` carries no such flow, so C# infers `E` from the value alone and `slice<nint>` then fails the `~[]int` constraint.
2. **An invariant RESULT position** (`typeParamReachesInvariantResult`): the parameter appears inside a func, slice, array, map, chan or pointer result. `internal/concurrent`'s own test suite ships the control pair that isolates this exactly -- `expectMissing[K, V comparable](t, key K, want V) func(got V, ok bool)` called `expectMissing(t, s, 0)` mis-inferred `V` and its returned delegate then rejected the map's `nint` (CS1503 x16), while `expectDeleted(..., 15) func(deleted bool)` -- the same untyped literal, `V` absent from the result -- compiled untouched.

```csharp
wantValue((nint)(0))(i, false);      // V reaches func(V, bool) -- retyped
wantPresent(15)(true);               // V absent from the result -- bare
bareResult(42);                      // result IS V; implicit conversion repairs it -- bare
wantValue<nint>(0)(i, false);        // Go wrote the type argument -- bare
sliceOf((nint)(9));                  // []V is invariant -- retyped
wantInt64((int64)(1234567890123L));  // the cast follows the RESOLVED width, not always nint
```

The cast type comes from the resolved type, so it is `nint`, `long`, `byte`, `nuint` and so on as the instantiation requires; a resolved `int32`/`rune` is skipped because a bare C# literal already IS System.Int32. A generic NAMED result is skipped too -- invariant likewise, but the explicit type-argument rule above already pins that instantiation, and retyping the argument as well would be redundant. Non-constant arguments never qualify: their emitted C# already carries the mapped Go type. Folded constant EXPRESSIONS do qualify (`3 + 4`, `1 << 10`), since they emit as bare C# arithmetic in exactly the same way.

Guarded by `GenericUntypedIntArg` (the `~[]E` lock) and `GenericUntypedConstInfer` (the invariant-result gate, its control shapes, and the nint/long/byte widths), both output-compared vs `go run`.

## Increment/decrement on a type parameter
`i++` / `i--` on a constrained type parameter binds `IIncrementOperators<T>` / `IDecrementOperators<T>`, which the lifted **Arithmetic** operator set now includes (reflect `rangeNum`'s loop, CS0023). They live in the numeric-only Arithmetic set -- never the string-including Sum set, since `@string` implements neither. The list is emitted in two places that must stay in sync: the converter's `getLiftedConstraints` (`constraintOperations.go`) and the go2cs-gen `InterfaceTypeTemplate`.

## Unary negation on a type parameter
`-x` on a constrained type parameter binds `IUnaryNegationOperators<T, T>`, which the lifted
**Arithmetic** operator set now includes (math/rand/v2's
`func keep[T int | uint | int32 | uint32 | int64 | uint64](x T) T { return -x }`, CS0023). Like
increment/decrement it is numeric-only — `@string` has no negation — so it never joins the Sum set.

Satisfying it needed a matching change on the generator side, because a NAMED Go numeric type is
instantiated through its go2cs-gen wrapper. Go defines `-x` on EVERY numeric type, unsigned
included, as the wrap-around `0 - x`; C# has no unary minus for `ulong` at all and widens
`uint`/`ushort`/`byte` to a signed type, which is why `NumericTypeTemplate` previously emitted the
operator only for signed types. It now emits the unsigned form as that subtraction under
`unchecked` — exactly Go's semantics — so a generic over `~uint64` instantiated with a named
unsigned type (internal/trace's `dataTable[EI ~uint64, E]` over `type stringID uint64`) satisfies
the constraint instead of failing CS0315:

```csharp
// generated for `type counter uint64`
public static counter operator -(counter value) => (counter)unchecked((uint64)((uint64)0 - value.m_value));
```

The list is emitted in THREE places that must stay in sync: the converter's `getLiftedConstraints`
(`constraintOperations.go`), the go2cs-gen `InterfaceTypeTemplate` "Arithmetic" constraint list, and
`InheritedTypeTemplate`'s `NumericInterfaces` declaration list (whose operator bodies come from
`NumericTypeTemplate`). Guarded by `GenericNegation`, which negates across the primitive widths and
through named types over both a signed (`~int32`) and an unsigned (`~uint64`) underlying type,
output-compared against Go so the unsigned wrap is verified rather than merely compiled.

## `uintptr` as a generic numeric type argument
The golib `uintptr` struct declares the full generic-math interface set the lifted numeric constraints demand (`IAdditionOperators` through `IComparisonOperators`, `IShiftOperators<uintptr, int, uintptr>` with a `>>>` operator, `IIncrementOperators`/`IDecrementOperators`) -- matching operators alone never satisfy a C# where-clause (CS0315 at reflect's `rangeNum<uintptr, uint64>`). At runtime, `ConvertToType`/`ConvertToUInt64` have `uintptr` fast paths, and the reflection-cached `TypeParamCaster` probes a public `Value` FIELD as well as the generated wrappers' `Value` property (hand-written wrappers keep a field for `Interlocked`/`Volatile` `ref x.Value` seams). Guarded by `GenericTypeInference` (`growShrink[U ~uint32 | ~uintptr]`).

> **Latent gap ([banked](../Glossary.md#banked)):** generated `/*num:**/` wrapper structs do NOT yet declare the generic-math interfaces -- a NAMED numeric wrapper used as a union-generic type argument would CS0315. No corpus site hits this yet.

## Union-constrained sub-slices ARE the type parameter
A sub-slice of a `string | []byte` union-constrained value is typed by Go as the type parameter again, so it assigns back to, passes as, and returns as the parameter (time format_rfc3339). The emission is the bare C# range expression:
```csharp
return parse(s[0..2]) + parse(s[3..5]);
```
```csharp
s = s[19..];                        // Go: s = s[19:]
```
Nothing converts it, because nothing needs to: the constraint is the **self-referential** `IByteSeq<T, byte>` (above), whose `TSelf this[Range]` indexer returns `T` itself, so the range expression already has the type parameter's type. All four bound shapes take this route — `s[..hi]`, `s[lo..hi]`, `s[lo..]` and `s[..]` — and a three-index slice cannot occur on a string-including union (Go forbids it on strings), so `Slice3` never reaches it. Downstream members bind through the constraint on the resulting value directly: `s[i..j].ToGoString()` for Go's `string(s[i:j])` (bytealg's Rabin-Karp), `src[lo..hi].ꓸꓸꓸ` for the variadic spread (below).

Func-literal parameters typed as the union type parameter render as the parameter itself (the enclosing method's type parameter is in scope inside a lambda), matching the Go:
```csharp
var parse = (T part) => {
```

> **History.** When `IByteSeq` was single-parameter, its Range indexer returned the **interface**, and the emission wrapped every sub-slice in `((T)(…))` to recover the type Go gives the expression (CS0266/CS0310/CS0029 without it — a runtime-checked unbox of a struct the indexer had just boxed). The self-referential constraint retired the box and left the cast an identity conversion emitting no IL; the cast was then retired in turn, so the rendering matches the Go instead of narrating a conversion that no longer happens.

Guarded by `StringByteUnionConstraint` — `trimHead`/`headSum` (assigned back, passed on, returned), `digitSum` (both bounds, through a func-literal parameter), `prefixMatch` (low omitted) and `wholeSpan` (both omitted). Its golden is the A/B: the cast's removal moves those lines and nothing else, while the stdout comparison against `go run` stays byte-identical.

## Spreading a union-constrained value
A union-constrained value may also be **spread** into a variadic — encoding/json's `appendString[Bytes []byte | string]` does `append(dst, src[lo:hi]...)` (and the open-ended `append(dst, src[lo:]...)`). The sub-slice is typed as the type parameter again (above), so the spread renders as `src[lo..hi].ꓸꓸꓸ`. A bare type-parameter value has no members of its own, so the spread `ꓸꓸꓸ` (which yields the `Span<byte>` the `append<T>(slice<T>, params Span<T>)` overload binds) must be declared on the **constraint interface** — a member access on a constrained type-parameter value resolves through its constraint. `IByteSeq<T>` therefore exposes `Span<T> ꓸꓸꓸ { get; }`; both implementers already satisfy it (`slice<T>` as `Span<T>`, `@string` as `Span<byte>`), so the interface member is implicit and adds no cast (CS1061 otherwise — the type parameter `Bytes` had no `ꓸꓸꓸ`). (Guarded by the `StringByteUnionConstraint` extension `appendRun` — a bounded and an open-ended sub-slice of the union value spread into `append`, both instantiations value-compared vs Go.)

---

[← Maps and Channels](maps-and-channels.md) · [Index](README.md) · [Type Aliasing →](type-aliasing.md)
