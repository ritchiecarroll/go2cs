# Labeled Control Flow and Loop Variables

[Reference index](README.md) · [Summary of this topic](../ConversionStrategies.md#labeled-control-flow-and-loop-variables)
Go restricts a label to immediately precede the enclosing statement (e.g. a `for`). Equivalent behavior is produced with a placed label and a `goto`:

## Break Label
```go
OuterLoop:
    for i = 0; i < n; i++ {
        for j = 0; j < m; j++ {
            switch a[i][j] {
            case nil:
                state = Error
                break OuterLoop
            }
        }
    }
```
becomes (the label is emitted as `break_OuterLoop:`):
```csharp
    for (i = 0; i < n; i++) {
        for (j = 0; j < m; j++) {
            switch (a[i][j].type()) {
            case nil:
                state = Error;
                goto break_OuterLoop;
            }
        }
    }
break_OuterLoop:;
```

## Continue Label
```go
RowLoop:
    for y, row := range rows {
        for x, data := range row {
            if data == endOfRow {
                continue RowLoop
            }
            row[x] = data + bias(x, y)
        }
    }
```
becomes (`continue_RowLoop:` placed at the end of the *labeled* loop's body — so `goto continue_RowLoop` from the inner loop lands there and the **outer** loop proceeds to its next iteration; `break_RowLoop:` would go after the outer loop):
```csharp
    foreach (var (y, row) in rows) {
        foreach (var (x, data) in row) {
            if (data == endOfRow) {
                goto continue_RowLoop;
            }
            row[x] = data + bias(x, y);
        }
continue_RowLoop:;
    }
```
Both the `break_<label>`/`continue_<label>` labels are emitted for a labeled `for` **and** a labeled `range`/`foreach` loop (the label target is placed regardless of loop kind; a missing one is CS0159 "no such label"). Guarded by the `ForVariants` behavioral test (labeled `range` with nested `continue`/`break`).

## A user label on an empty statement emits an explicit empty statement

A Go label can attach to an **empty statement** — `keep:` as the last line of a block, a
`goto`/`break`/`continue` target with nothing between the label and the closing brace
(internal/trace gc.go's `goto keep` target at the tail of a for-loop body). A C# label must
precede a statement, so a bare `keep:` before `}` is CS1525/CS1002. `visitLabeledStmt` detects
an `*ast.EmptyStmt` target and emits the explicit empty statement `keep:;` — the same shape the
`break_<label>:;`/`continue_<label>:;` synthesis already uses. A label on a non-empty statement
is unchanged (`big:` followed by its statement stays bare). Guarded by `LabeledEmptyStmt` (a
`goto` to an end-of-loop-body label, a `goto` to an end-of-function label, and a `goto` to an
end-of-inner-block label — values vs Go).

## Reassigned or ref-bound range variable
A C# `foreach` iteration variable is **read-only**, but Go lets a `range` key/value variable be reassigned inside the body (it is a per-iteration copy). When the converter detects such a reassignment (`=`, `+=`, `-=`, `++`, …) of a newly-`:=`-defined range variable — or a **pointer-receiver method selected on the value-typed range var** (`q.GoString()`, whose emitted `[GoRecv]` form takes `this ref T`; a foreach var cannot bind `ref` — CS1657, dnsmessage's four `Message.GoString` loops) — it iterates a temp and declares the variable as a mutable local copy in the body, rather than binding it directly. The per-iteration `var q = vᴛ1;` copy preserves Go's semantics exactly: the pointer-receiver mutates the copy, as Go's implicit `(&q)` on the range copy does. A pointer-*typed* range var is excluded (it dereferences; no ref bind), as are value-receiver and interface-method selections. The machinery covers string AND slice/array/map ranges:

```go
for _, r := range s {  // r is a rune
    if r >= 0x10000 {
        r -= 0x10000   // reassigns the range variable — CS1656 on a foreach var
        …
    }
}
```
```csharp
foreach (var (_, rᴛ1) in s) {
    var r = rᴛ1;       // mutable local copy
    if (r >= 65536) {
        r -= 65536;
        …
    }
}
```
A range variable that is only *read* keeps binding directly to the `foreach` tuple (no temp, no churn). This reuses the same temp-var/`innerPrefix` machinery as the `for k, v = range` (re-assign-into-existing-vars) form. (Guarded by the `RangeVarReassign` behavioral test; runtime hits this in `os_windows`'s UTF-16 surrogate-pair encoder.)

**Addressability flows from the path ROOT, not the immediate operand.** Each trigger above is
matched against the identifier at the root of a field/element access path (`rangeVarRootIdent`),
because Go's addressability propagates through every value hop: `test.x.String()` with a
pointer-receiver `String` is legal in Go (it auto-takes `&test.x`), and C# names the same rule in
its diagnostics — CS1654/CS1655 speak of *"fields of"* the iteration variable. Matching only the
immediate operand missed the whole class, which matters far beyond one construct because this is
the **table-driven test** idiom that dominates the standard library's own test suites:

```go
for _, test := range tests {
    fmt.Println(test.x.String())   // pointer receiver on a FIELD of the range var — CS1655
}
```
```csharp
foreach (var (_, vᴛ1) in tests) {
    var test = vᴛ1;                // mutable, addressable per-iteration copy
    fmt.Println(test.x.String());
}
```

The same root walk covers a write through a nested path (`test.x.n = 99`, CS1654) and an address
taken of one (`&test.x`). The walk **stops at the first pointer hop**: past a pointer the access
goes through the heap box (`ж<T>.Value`), an independent mutable lvalue the iteration variable's
read-only-ness never reaches — so `test.ptr.Bump()` keeps the direct `foreach` binding with no copy,
and (matching Go) its write *is* observed by the source. Indexing likewise only continues through an
**array**, whose storage is in-place; a slice or map index reaches a separate backing store and is
its own addressability root. The copy is per-iteration, matching Go 1.22+ loop-variable semantics
(see the for-clause section below).

Scoping the trigger to the root this way *removes* work as well as adding it: the previous
field-write arm never checked whether the base was a POINTER, so a write like `s.Name = x` through a
pointer-typed range variable emitted a needless per-iteration copy. Across the 302-package
converted-stdlib corpus the net effect is **54 spurious copies eliminated and none added** — the
addressability additions show up in *test* code (the table-driven idiom), not in production sources.

## For-clause variables are per-iteration (Go 1.22 loop-variable semantics)

Go 1.22 gives each iteration of a `for i := …; cond; post` loop its **own copy** of the clause-declared variables, initialized from the previous iteration's final value. A C# for-clause variable is ONE variable shared by every iteration, so a closure captured in the body would observe the shared post-mutated final value (`3 3 3` instead of Go's `0 1 2`), and a stored `&i` would alias one shared box — compiling code that is silently wrong. When a clause-declared variable is **captured by a func literal in the body** or is **heap-boxed**, the converter rewrites the clause to drive a renamed *carrier* (`iᴛ1`) and re-declares the real variable fresh from the carrier at the top of the body; when the body can write the variable, its value is copied back to the carrier at every transfer to the post clause — the end of the body, before an unlabeled `continue` (a C# `continue` skips the end of the body), and after the `continue_<label>:` target (which the copy-backs deliberately follow, so a `goto continue_<label>` flows through them):

```go
var fs []func() int
for i := 0; i < 3; i++ {
	fs = append(fs, func() int { return i })
}
fmt.Println(fs[0](), fs[1](), fs[2]())   // Go 1.22+: 0 1 2
```
```csharp
for (nint iᴛ1 = 0; iᴛ1 < 3; iᴛ1++) {
    var i = iᴛ1;               // fresh per-iteration variable — each closure captures its own
    fs = append(fs, () => i);
}
```

A variable the body (or a closure in it) *writes* adds the copy-backs:

```csharp
for (nint iᴛ1 = 0; iᴛ1 < 6; iᴛ1++) {
    var i = iᴛ1;
    if (i % 2 == 0) {
        iᴛ1 = i;               // an unlabeled continue copies back at its own site
        continue;
    }
    i++;
    fs = append(fs, () => i);
    iᴛ1 = i;                   // end-of-body copy-back feeds the post clause
}
```

A **heap-boxed** clause variable allocates a *fresh box* each pass — the same rule as the [per-iteration range-variable box](pointers.md#pointers): a stored `&i` must be a distinct pointer per iteration — and always copies back (writes through the pointer are not syntactically detectable):

```csharp
for (nint iᴛ1 = 0; iᴛ1 < 3; iᴛ1++) {
    ref var i = ref heap<nint>(out var Ꮡi);
    i = iᴛ1;
    ps = append(ps, Ꮡi);       // three DISTINCT pointers: 0 1 2
    iᴛ1 = i;
}
```

Loops whose clause variables are neither captured nor boxed emit exactly as before — the shared clause variable is then unobservable, so there is no churn. A read-only captured variable skips the copy-backs. Every clause reference (init/cond/post) renders the carrier, so the body keeps the variable's Go name — nested same-name loops compose with shadow renames (`iΔ1` gets carrier `iΔ1ᴛ1`), and a multi-variable clause transforms only the variables that need it. One legacy fallback: a heap-boxed variable that a **clause** func literal references keeps the old hoisted whole-loop box, since the body-scoped box would not be in scope at the clause. (Guarded by the `ForLoopPerIterationVars` behavioral test — read-only capture, body write + unlabeled `continue`, stored `&i` distinctness, boxed + captured closures, labeled continue with a write, nested same-name loops, a multi-variable clause, a struct-typed clause variable, and an immediately-invoked writing closure — values vs Go. `EscapedLoopVarSiblingIndex`, `ForVariants`, and `RingPointerMethods` re-baselined to the per-iteration shape.)


### The capture that has no func literal: `defer`/`go` on an indexed receiver

The trigger above asks whether a clause variable is heap-boxed or referenced inside a **func literal
in the Go source**. A `defer`/`go` statement can capture one with no func literal present at all:
`visitDeferStmt`/`visitGoStmt` SYNTHESIZE a lambda (`() => recv.M()`) for every callee the
method-group form cannot express -- a **result-returning** method, a named func type
(`context.CancelFunc`), a variadic callee -- and that synthesis happens long after the analysis has
run. The clause variable therefore stayed SHARED and the synthesized lambda read its post-loop value:

```go
var c [3]closer                     // Conn-shaped: Close() returns error
for i := 0; i < 3; i++ {
    c[i] = dial(i)
    defer c[i].Close()              // Go: closes 2, 1, 0
}
```

```csharp
for (nint i = 0; i < 3; i++) {      // BEFORE: shared i
    var cʗ1 = c;                    // the capture snapshot hoists the ARRAY...
    defer(() => cʗ1[i].Close(), ref ᒐ);  // ...but the SUBSCRIPT is read when the defer fires: c[3]
}
```

net's `TestConcurrentSetDeadline` is the witness -- `defer c[i].Close()` over a `[10]Conn` indexed
`c[10]` and panicked with index out of range, a hard failure rather than a wrong answer.
`deferOrGoCalleeReferences` joins the trigger so the transform fires, and the emission takes the
ordinary per-iteration shape above. Only the **callee** is consulted: a deferred call's ARGUMENTS
are already evaluated at the defer statement exactly as Go requires
(`defer((ᴛ1, ᴛ2) => f(ᴛ1, ᴛ2), a, i, ref ᒐ)`), so a clause variable appearing only in an
argument needs no per-iteration copy -- and minting one would move emission at every such site for
no behavioral gain. That restriction is why the corpus census measured **zero** moved files across
307 projects: the sole production site with the shape (`net/http/h2_bundle.go`'s
`defer http2bufPools[index].Put(bp)`) indexes a plain local outside any loop. The shape occurs in
`net/dial_test.go` and `net/timeout_test.go`, so the fix's reach is TEST conversions -- which is
where the defect was found. (Guarded by the `DeferLoopCapture` behavioral test: the defer witness
and the `go` equivalent as red shapes, plus four controls -- a receiver reassigned after the defer,
3-clause and range closure capture, and deferred plain arguments -- that were already correct and
must stay green.)

## A range-over-int index is `nint` (golib's `range` helper yields Go's `int`)
Go 1.22's `for i := range n` (range over an integer) produces `i` of type `int`, which go2cs maps to `nint`. The converter lowers it to a `foreach` over golib's `range` helper, and — with `-var` (the default) — leaves the index as the idiomatic `var`:
```go
size := 5
var s []int
for i := range size {
    s = append(s, i)
}
```
```csharp
nint size = 5;
slice<nint> s = default!;
foreach (var i in range(size)) {
    s = append(s, i);
}
```
For `var i` to infer `nint` — matching Go's index type — golib's `range(nint)` must *yield* `nint`, not a C# `int`. It originally returned `Enumerable.Range(0, (int)n)` (element type `int`), so `var i` inferred `int`. That is invisible until the index feeds a generic builtin: `append(s, i)` with `s` a `slice<nint>` and `i` an `int` matches **two** `builtin.append` overloads with **different** inferred `T` — `append<T>(slice<T>, params Span<T>)` infers `T=nint` (from `s`; the `int→nint` element conversion is implicit), while `append<T>(ISlice, params T[])` infers `T=int` (from `i`). Neither wins the argument-by-argument betterness tie (each is better on one argument), so the call is ambiguous — **CS0121**. An explicit `nint i` always resolved (both overloads then infer `T=nint`, and `slice<T>` beats `ISlice` on the first argument), which is the tell that the defect was the *element type* the index inferred, not the converter's `var` (correct and idiomatic) nor the `append` overload set (unambiguous for a correctly-typed `nint`). The root fix is therefore in golib: `range(nint)` yields `nint`. As a hand-written iterator (`for (nint i = 0; i < n; i++) yield return i;`) it also matches Go's integer-range semantics for `n <= 0` exactly (zero iterations), where the old `Enumerable.Range` threw on a negative count. Because the converter emission is unchanged, this is a pure golib change — byte-identical `check-no-regression` — that silently corrects the index type for *every* range-over-int loop in the corpus, and unblocked the `maps` and `slices` Phase-4 test suites (whose `want = append(want, i)` over a `range(size)` index hit exactly this CS0121). Guarded by the `RangeIntIndexAppend` behavioral test (the minimal `append(s, i)`-over-`range(size)` shape, output-compared vs Go; neutering golib's `range` back to `IEnumerable<int>` reproduces the CS0121).

## Range-over-integer covers EVERY integer type, and the iteration variable keeps the operand's width
Go 1.22's range-over-integer accepts **any** integer type, not just `int`, and gives the iteration variable *that* type. The converter recognized only `types.Int` and untyped-int, so every other integer kind — `uintptr`, `int64`, `uint8`, `rune`, a named integer type — fell through `visitRangeStmt`'s `unexpected 'ast.RangeStmt' expression` arm, which prints the statement as a **C# comment**. The loop did not fail to convert loudly; it silently *vanished*, and the program quietly computed a different answer.

The corpus site that proved it is `internal/abi`'s `for range atyp.Len` inside `unique.buildArrayCloneSeq` (`unique/clone.go`), where `atyp.Len` is a `uintptr`:
```go
for range atyp.Len {
	switch etyp.Kind() {
	case abi.String:
		seq.stringOffsets = append(seq.stringOffsets, offset)
	…
	}
	offset += etyp.Size()
}
```
```csharp
foreach (var _ᴛ1 in range<uintptr>((~atyp).Len)) {
    var exprᴛ1 = etyp.Kind();
    if (exprᴛ1 == abi.ΔString) {
        seq.stringOffsets = append(seq.stringOffsets, offset);
    }
    …
    offset += etyp.Size();
}
```
The whole body had been a `/* … */` block, so `unique`'s `cloneSeq` for any array-of-string type came back **empty** instead of carrying one offset per element. It was the only such comment in the entire converted standard library — a one-site defect precisely because non-`int` range operands are rare, and invisible for exactly the same reason.

Two pieces carry the fix. golib gains a generic `range<T>(T n)`, constrained to the **operator pair the loop actually uses** (`IComparisonOperators<T,T,bool>` + `IIncrementOperators<T>`, with `default(T)` for the zero) rather than `IBinaryInteger<T>` — golib's own `uintptr` is a hand-written struct that implements the generic-math *operator* interfaces but not the `INumberBase` hierarchy, and `uintptr` is exactly the type this site ranges over. The converter then names `T` **explicitly** at every non-`int` site (`range<uintptr>(…)`, `range<rune>(…)`), because Go's `rune` and C#'s `int` are one CLR type and an inferred call could not tell `for i := range r` (yields `rune`) from `for i := range 3` (yields Go's `int`); a NAMED integer type additionally has its operand cast down to the underlying width, since a converted `[GoType("num:…")]` struct satisfies no generic-math interface at all.

The `int` case is emitted byte-identically — bare `range(expr)` — but it needed one guard of its own. A literal operand hands the overload set a C# `int`, where the new generic is an **identity** match and `range(nint)` needs an implicit numeric conversion; the generic therefore wins outright and C#'s "a non-generic method is better than a generic method" tie-break never applies (measured, not reasoned — the first attempt did regress `range(3)` to `System.Int32`). golib carries a third overload, `range(int n) → IEnumerable<nint>`, meaning "a C# `int` operand is Go's `int`": an `int`-typed operand in emitted code can only be an untyped Go constant, since a Go `int` expression already renders as `nint` and every other width is emitted with an explicit type argument. With it, `for i := range 3` stays on `nint` and the CS0121 above cannot come back.

Guarded by the `RangeOverIntegerTypes` behavioral test (blank-key `uintptr`, `uint8`/`int64`/`uint64`/`rune`, a named `uintptr` type, non-positive operands, and the unchanged `int`/untyped cases, output-compared vs Go — reverting the `isInt` widening reproduces `FAIL [Target,Output]`) and by `GolibTests.GoStructLayoutTests` (the `range<T>` widths and the literal-operand overload binding). Behavioral `check-no-regression` is byte-identical across all 570 packages apart from the new project, which is the expected shape: the corpus contained exactly one non-`int` range operand.

## Range-over-func on named/generic Seq types

**Range-over-func on named/generic Seq types.** Go 1.23's `for v := range seq` (and the two-value `for k, v := range seq2`) on an `iter.Seq[E]`-shaped value emits through golib's yield-adapting `range()` overloads. Three pieces make the named/generic form work: detection unwraps the type's `Underlying()` (a defined or instantiated func type is a `Named`, not a bare `Signature`); a NAMED func type renders as a C# *delegate*, which has no conversion to the overloads' `Action<Func<…>>` parameter — its method GROUP does, so the emission appends `.Invoke`; and because C# cannot infer a type parameter from a method group's parameters, the element types are spelled out from the yield signature: `foreach (var v in range<nint>(countdown(5).Invoke))`. `break` inside the body ends the foreach, which cancels the adapter's producer — the yield function receives `false`, matching Go's semantics; a two-value `range<K, V>` overload adapts pair-yields onto the tuple machinery. One adjacent gate was refined en route: a call's result being a generic instantiation adds explicit type arguments only for conversions and GENERIC callees (`NewOption<nint>(42)` — an untyped-const arg would infer C# `int` where Go infers `nint`), never for a plain function returning a generic named type (`countdown<nint>(5)` was CS0308). (Guarded by the `GenericTypeInference` extensions — a generic `Seq[V]` ranged with `break` and a two-value `KVSeq[K, V]`, values vs Go.)

## A blank scalar range variable never emits as `_`
A `range` with no iteration variable (or an explicit blank) over a **scalar-yield** source — a channel, an integer (Go 1.22 `for range n`), or a single-value yield function — needs a C# `foreach` iteration variable, and that variable must **not** be named `_`: in a scalar `foreach` position C# declares a genuine read-only variable *named* `_` (only tuple-deconstruction `_` is a discard), which shadows the discard idiom for the entire loop body. Any Go blank assignment inside the body (`_ = f(x)` — evaluate and discard) then resolves to that variable and becomes an illegal write to a `foreach` iteration variable (**CS1656**; first hit: `encoding/binary`'s `BenchmarkSize`, `for range b.N { _ = Size(data) }`). The converter emits a marked temp instead:
```go
for range b.N {
    _ = Size(data)
}
```
```csharp
foreach (var _ᴛ1 in range((~bΔ1).N)) {
    _ = Size(dataʗ1);
}
```
Tuple positions are unaffected — `foreach (var (_, data) in …)` keeps the true C# discard. Guarded by the `RangeStatements` behavioral test (blank int-range and blank channel-range, each with a body blank assignment; the compile phase is the guard — the old emission is CS1656).

## `for range` over a slice allocates NOTHING — `slice<T>.GetEnumerator()` returns a struct

`for i, v := range s` emits `foreach (var (i, v) in s)`, and Go's range over a slice allocates nothing at all. C# matches that only if the enumerator stays off the heap, which is entirely a question of what `GetEnumerator` **returns**: `foreach` binds `GetEnumerator` by **pattern** — the concrete return type, ahead of and independently of any interface — so a struct return is enumerated in place, while an interface return is a heap object per loop *entry*.

`slice<T>.GetEnumerator()` returned `IEnumerator<(nint, T)>` from an ITERATOR method (`yield return`), which is the worst of both: the compiler-generated state machine is one allocation and the inner `SliceEnumerator` class it drove is a second. Measured at **136 bytes per loop entry**, corpus-wide — every ranged loop in every converted package, paid whether the loop body allocated or not. It is invisible in output and in timings at small scale, and unmissable in a Go test that asserts an allocation count: `time.TestUnmarshalTextAllocations` runs `parseRFC3339`, whose `parseUint` closure ranges its argument once per field.

The return type is now the concrete nested `slice<T>.Enumerator` struct (the shape `List<T>.Enumerator` uses, and the one golib's own `sslice<T>` already had). Two contracts had to move with it:

* `slice<T>` reaches `IEnumerable<(nint, T)>` through `ISlice<T>` → `IArray<T>`, which the old public method satisfied implicitly. The interface member is now an **explicit** implementation returning the same struct boxed — so LINQ, an interface-typed local, and anything holding the slice as `IEnumerable<(nint, T)>` behave exactly as before, at exactly the cost they already paid. Only the pattern path is free.
* go2cs-gen's `ISliceTypeTemplate` (every `type S []E` named-slice wrapper) forwarded the interface. It now forwards `global::go.slice<E>.Enumerator` and carries the same explicit interface member, so a named slice type ranges as cheaply as the `slice<E>` it wraps — otherwise every `for range` over a named slice would have kept the box.

`array<T>.GetEnumerator()` was the identical shape and was deliberately left alone here, because the copy Go's array range takes had to be placed first; it is settled in the section below.

Guarded by `SliceRangeAllocationTests` in `GolibTests`, which asserts **zero** bytes via `GC.GetAllocatedBytesForCurrentThread` across 1,000 loops (whole slice, sub-window with window-relative indices, and the nil slice), plus the interface-path equivalence. It is a measured guard on purpose: restoring the interface return type still compiles and still produces correct output — it just allocates again — so only bytes can catch the regression. Neutering to the interface return reports 48 B/loop; restoring the original iterator body reports exactly 136 B/loop.

## `range` over an ARRAY VALUE iterates a COPY — the snapshot is the range EXPRESSION's `.Clone()`

Go evaluates a range expression **once** before the loop, so `for i, v := range a` over an array VALUE iterates a copy: a write to the container inside the body is invisible to every later iteration. The emitted `array<T>` (and the generated named-array wrapper) is a struct over a shared `T[]` backing, so the plain operand ALIASES the container — the emission read the writes back, diverging from `go run` on every such loop:

```go
a := [4]int{1, 2, 3, 4}
for i, v := range a {
    if i == 0 { a[1], a[2], a[3] = 91, 92, 93 }
    fmt.Println(i, v)                 // Go: 1 2 3 4        emitted (before): 1 91 92 93
}
```

The range expression is simply the array value-copy site nobody had emitted (see *Array VALUE-COPY at every transfer site* above — `range` was listed there for the iteration VARIABLE, never for the operand). It now takes a copy of its own:

```csharp
foreach (var (i, v) in a.ΔRangeSnapshot()) { … }        // array value  — snapshot, Go's copy
foreach (var (i, v) in h.arr.ΔRangeSnapshot()) { … }    // struct field — likewise a value
foreach (var (i, v) in r.ΔRangeSnapshot()) { … }        // named array  — the wrapper forwards it
```

**Why it is not the `.Clone()` every other transfer site takes, and this is the load-bearing part.**
Semantically it could be, and it was first. But a Go array copy lives INLINE — on the stack when the
destination is a local — so Go charges it **zero mallocs and zero `TotalAlloc`**, and a range snapshot
is the one array copy that provably cannot outlive its statement. `Clone()` mints a counted managed
array through `AllocationCounter`, which is right for a copy that DOES outlive the statement (an
assignment, a return, a field, a channel send) and wrong for one that cannot: golib's counter is
documented as the structural mirror of `runtime.MemStats.Mallocs`, so charging what Go does not
makes the mirror wrong by construction, and every `testing.AllocsPerRun` assertion around a range
over an array value would disagree with Go's own number. The byte meter is stricter still —
`runtime.ReadMemStats`'s `TotalAlloc` maps to `GC.GetTotalAllocatedBytes`, which no counter can hide
from — so the copy has to genuinely not allocate. `array<T>.ΔRangeSnapshot()` returns a `RangeSnapshot`
struct whose enumerator rents from `ArrayPool<T>.Shared` and returns the buffer in `Dispose`, which
C#'s `foreach` calls in a `finally`; steady state is zero managed allocations on both meters. Three
residuals are named rather than hidden: the first rent of a size class allocates once per process, an
array beyond the pool's largest bucket allocates per rent (as it would in Go, which also moves an
array that size off the stack), and an element type needing a DEEP copy still allocates per element,
because a nested `array<T>`'s backing is a heap object in this model.

**A SLICE element is never re-copied, and getting that wrong was a real crash.** `array<T>.Clone()`
re-clones elements that are themselves array wrappers, and `ISlice<T>` derives from `IArray<T>` — so a
named-slice element passed that test, while the generated wrapper's `Clone()` forwards to the
underlying `slice<T>`'s `ICloneable.Clone()`, which hands back a boxed `slice<T>`: the element cast is
then `slice<int>` → `ΔBits` and throws `InvalidCastException`. Latent until something first cloned an
array whose element is a slice; the range snapshot was that first caller, and math/big's
`bitsList` (`[...]Bits`, `type Bits []int`) is the corpus site — `TestFloatAdd` and `TestFloatMul` died
on it. Semantically the exclusion is required anyway: Go's array-of-slices copy copies HEADERS and
shares every backing store, which the shallow element copy already did. The `ArrayRangeSnapshot`
guard pins both halves — replacing a whole element through the original is invisible to the loop
(a fresh header), while a write THROUGH a shared backing is visible.

Scoped exactly as gc's own rule is (`cmd/compile/internal/walk/order.go`'s `rangeStmt`), so three shapes stay UNCOPIED because Go copies nothing there either — and each is a control in the guard:

* **No value iteration variable.** With at most one iteration variable and a constant length, Go does not evaluate the range expression at all, so `for i := range a` reads the LIVE array through the index. A blank value (`for i, _ := range a`) is the same case.
* **A POINTER to an array.** `for i, v := range p` shares the pointee; the emission keeps the bare `p.Value`.
* **A slice.** Its copy is the header, which the struct assignment already is.

`exprReadsValueNeedingClone` narrows the rest: only a read out of EXISTING storage (ident, selector, index, deref) can alias — a composite literal, call result, or conversion is freshly constructed and reachable by no other name, and a return already clones on its own way out.

With the operand snapshotted, `array<T>`'s enumerator reads LIVE storage and needs no capture point of its own, which is what finally let it shed the iterator method: `GetEnumerator()` returns the nested `array<T>.Enumerator` STRUCT, and go2cs-gen's `IArrayTypeTemplate` / `IArrayViewTypeTemplate` forward that struct (with the explicit `IEnumerable<(nint, T)>` member beside it) so named array types range as cheaply. Measured with `GC.GetAllocatedBytesForCurrentThread` over 1,000 loops: **72 B/loop → 0 B/loop** on the pattern path, and 103 → 79 B/loop on the boxing interface path, which now boxes a struct instead of driving a state machine. Snapshotting inside the enumerator instead would have been wrong in both directions — it would allocate on every loop AND copy for the two shapes above where Go shares.

Guarded two ways: `ArrayRangeSnapshot` (behavioral, output-compared against `go run`) mutates the container mid-loop across the array value, named-array, struct-field, nested-array, array-of-named-slices, `=`-form, mutable-range-var and aliased-element shapes, with the pointer, slice and index-only arms as controls that must NOT copy; `ArrayRangeAllocationTests` (`GolibTests`) asserts the enumerator's zero bytes with the boxing interface path as its nonzero control, the snapshot's zero on BOTH meters (object count and CLR bytes) with `Clone()` as the counted control that makes those zeros mean something, and the array-of-slices copy that must share its backing rather than throw.

---

[← Implicit Pointer Dereferencing](implicit-dereferencing.md) · [Index](README.md) · [The `go.golib` support namespace →](golib-namespace.md)
