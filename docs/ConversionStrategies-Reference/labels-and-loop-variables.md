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
becomes (with `a` a `[][]*int`; the Go label stays in place, and the `break` target is emitted as `break_OuterLoop:;` after the loop):
```csharp
OuterLoop:
    for (i = 0; i < n; i++) {
        for (j = 0; j < m; j++) {
            var exprᴛ1 = a[i][j];
            if (exprᴛ1 == default!) {
                state = Error;
                goto break_OuterLoop;
            }

        }
continue_OuterLoop:;
    }
break_OuterLoop:;
```
<!-- probe 2026-10-22 at 1b101a3def: the one-case switch on a pointer lowers to the if-chain form (see expression-switch.md); both synthesized targets are emitted even when only one is used. -->


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
becomes (`continue_RowLoop:` placed at the end of the *labeled* loop's body — so `goto continue_RowLoop` from the inner loop lands there and the **outer** loop proceeds to its next iteration; `break_RowLoop:;` follows the outer loop):
```csharp
RowLoop:
    foreach (var (y, row) in rows) {
        foreach (var (x, data) in row) {
            if (data == endOfRow) {
                goto continue_RowLoop;
            }
            row[x] = data + bias(x, y);
        }
continue_RowLoop:;
    }
break_RowLoop:;
```
Both the `break_<label>`/`continue_<label>` labels are emitted for a labeled `for` **and** a labeled `range`/`foreach` loop (the label target is placed regardless of loop kind; a missing one is CS0159 "no such label"). Guarded by the `ForVariants` behavioral test (labeled `range` with nested `continue`/`break`).

## A user label on an empty statement emits an explicit empty statement

A Go label can attach to an **empty statement** — `keep:` as the last line of a block, a
`goto`/`break`/`continue` target with nothing between the label and the closing brace
(internal/trace gc.go's `goto keep` target at the tail of a for-loop body). A C# label must
precede a statement, so a bare `keep:` before `}` is CS1525/CS1002. `visitLabeledStmt` detects
an `*ast.EmptyStmt` target and emits the explicit empty statement `keep:;` — the same shape the
`break_<label>:;`/`continue_<label>:;` synthesis already uses. A label on a non-empty statement
keeps the plain form (`big:` followed by its statement stays bare). Guarded by `LabeledEmptyStmt` (a
`goto` to an end-of-loop-body label, a `goto` to an end-of-function label, and a `goto` to an
end-of-inner-block label — values vs Go).

## Reassigned or ref-bound range variable
A C# `foreach` iteration variable is **read-only**, but Go lets a `range` key/value variable be reassigned inside the body (it is a per-iteration copy). When the converter detects such a reassignment (`=`, `+=`, `-=`, `++`, …) of a newly-`:=`-defined range variable — or a **pointer-receiver method selected on the value-typed range var** (`q.GoString()`, whose emitted form takes `this ref T`; a foreach var cannot bind `ref` — CS1657, dnsmessage's four `Message.GoString` loops) — it iterates a temp and declares the variable as a mutable local copy in the body, rather than binding it directly. The per-iteration `var q = vᴛ1;` copy preserves Go's semantics exactly: the pointer-receiver mutates the copy, as Go's implicit `(&q)` on the range copy does. A pointer-*typed* range var is excluded (it dereferences; no ref bind), as are value-receiver and interface-method selections. The machinery covers string AND slice/array/map ranges:

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

    if (r >= 0x10000) {
        r -= 0x10000;
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
(see [For-clause variables are per-iteration](#for-clause-variables-are-per-iteration-go-122-loop-variable-semantics)).

Scoping the trigger to the root also keeps copies out where none is needed: a write like `s.Name = x`
through a pointer-typed range variable stops at the pointer hop and binds the `foreach` variable
directly. In the converted standard library the root walk's copies appear in *test* code (the
table-driven idiom), not in production sources.
<!-- history: the field-write arm before the root walk never checked whether the base was a POINTER, so a write
through a pointer-typed range variable emitted a needless per-iteration copy; the walk's net effect was spurious
copies eliminated and none added.
measured when the root walk landed: 302-package corpus, 54 copies eliminated, none added; not re-measured by any
guard. -->

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
    fs = append(fsᴷ, fs, () => i);
}
```

(`fsᴷ` is the function's stack buffer for the non-escaping `fs`, declared at its entry; see [A non-escaping append can take Go's 32-byte stack buffer](slices-and-arrays.md#a-non-escaping-append-can-take-gos-32-byte-stack-buffer).)

A variable the body (or a closure in it) *writes* adds the copy-backs:

```csharp
for (nint iᴛ1 = 0; iᴛ1 < 6; iᴛ1++) {
    var i = iᴛ1;
    if (i % 2 == 0) {
        iᴛ1 = i;               // an unlabeled continue copies back at its own site
        continue;
    }
    i++;
    fs = append(fsᴷ, fs, () => i);
    iᴛ1 = i;                   // end-of-body copy-back feeds the post clause
}
```

A **heap-boxed** clause variable allocates a *fresh box* each pass — the same rule as the [per-iteration range-variable box](pointers.md#pointers): a stored `&i` must be a distinct pointer per iteration — and always copies back (writes through the pointer are not syntactically detectable):

```csharp
for (nint iᴛ1 = 0; iᴛ1 < 3; iᴛ1++) {
    ref var i = ref heap<nint>(out var Ꮡi);
    i = iᴛ1;
    ps = append(psᴷ, ps, Ꮡi);  // three DISTINCT pointers: 0 1 2
    iᴛ1 = i;
}
```

Loops whose clause variables are neither captured nor boxed keep the plain C# clause — the shared clause variable is then unobservable, so there is no churn. A read-only captured variable skips the copy-backs. Every clause reference (init/cond/post) renders the carrier, so the body keeps the variable's Go name — nested same-name loops compose with shadow renames (`iΔ1` gets carrier `iΔ1ᴛ1`), and a multi-variable clause transforms only the variables that need it. One fallback: a heap-boxed variable that a **clause** func literal references keeps a single box hoisted over the whole loop, since the body-scoped box would not be in scope at the clause. (Guarded by the `ForLoopPerIterationVars` behavioral test — read-only capture, body write + unlabeled `continue`, stored `&i` distinctness, boxed + captured closures, labeled continue with a write, nested same-name loops, a multi-variable clause, a struct-typed clause variable, and an immediately-invoked writing closure — values vs Go. The `EscapedLoopVarSiblingIndex` and `RingPointerMethods` goldens also carry the per-iteration shape.)
<!-- history: EscapedLoopVarSiblingIndex, ForVariants and RingPointerMethods were re-baselined to the per-iteration
shape when it landed; at 3e3f18925a ForVariants' golden has no carrier (its goǃ passes i as an argument). -->


### The capture that has no func literal: `defer`/`go` on an indexed receiver

The trigger above asks whether a clause variable is heap-boxed or referenced inside a **func literal
in the Go source**. A `defer`/`go` statement can capture one with no func literal present at all:
`visitDeferStmt`/`visitGoStmt` SYNTHESIZE a lambda (`() => recv.M()`) for every callee the
method-group form cannot express -- a **result-returning** method, a named func type
(`context.CancelFunc`), a variadic callee -- and that synthesis happens long after the analysis has
run. Left SHARED, the clause variable would be read by the synthesized lambda at its post-loop value:
the capture snapshot (`cʗ1`) hoists the ARRAY, but the SUBSCRIPT is read when the defer fires, `c[3]`.
`deferOrGoCalleeReferences` therefore joins the trigger, and the emission takes the ordinary
per-iteration shape above:

```go
var c [3]closer                     // Conn-shaped: Close() returns error
for i := 0; i < 3; i++ {
    c[i] = dial(i)
    defer c[i].Close()              // Go: closes 2, 1, 0
}
```

```csharp
for (nint iᴛ1 = 0; iᴛ1 < 3; iᴛ1++) {
    var i = iᴛ1;                    // this iteration's own i
    c[i] = dial(i);
    var cʗ1 = c;                    // the capture snapshot of the array
    defer(() => cʗ1[i].Close(), ref ᒐ);  // the subscript reads this iteration's i
}
```
<!-- shape: src/tests/Behavioral/DeferLoopCapture/main.cs.target:56 `defer(() => cʗ1[i].Close(), ref ᒐ);`
(deferIndexedReceiverInLoop, :52 `for (nint iᴛ1 = 0; iᴛ1 < 3; iᴛ1++) {`), the fixture's conn construction written
as dial(i). -->

net's `TestConcurrentSetDeadline` is the witness: with the variable shared, `defer c[i].Close()` over a
`[10]Conn` indexes `c[10]` and panics with index out of range, a hard failure rather than a wrong answer.
Only the **callee** is consulted: a deferred call's ARGUMENTS
are already evaluated at the defer statement exactly as Go requires
(`defer((ᴛ1, ᴛ2) => f(ᴛ1, ᴛ2), a, i, ref ᒐ)`), so a clause variable appearing only in an
argument needs no per-iteration copy -- and minting one would move emission at every such site for
no behavioral gain. That restriction keeps the converted standard library's production code
unchanged: its only sites with the shape (`net/http/h2_bundle.go`'s two
`defer http2bufPools[index].Put(…)` calls) index a plain local outside any loop. The shape occurs in
`net/dial_test.go` and `net/timeout_test.go`, so the rule reaches TEST conversions. (Guarded by the
`DeferLoopCapture` behavioral test: the defer witness and the `go` equivalent, plus four controls -- a
receiver reassigned after the defer, 3-clause and range closure capture, and deferred plain arguments --
whose emission the trigger must not move.)
<!-- census at landing: zero moved files across 307 projects; not re-measured by a guard. -->

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
For `var i` to infer `nint` — matching Go's index type — golib's `range(nint)` must *yield* `nint`, not a C# `int`. An enumerator yielding `int` (such as `Enumerable.Range(0, (int)n)`) would make `var i` infer `int`, which is invisible until the index feeds a generic builtin: `append(s, i)` with `s` a `slice<nint>` and `i` an `int` matches **two** `builtin.append` overloads with **different** inferred `T` — `append<T>(slice<T>, params Span<T>)` infers `T=nint` (from `s`; the `int→nint` element conversion is implicit), while `append<T>(ISlice, params T[])` infers `T=int` (from `i`). Neither wins the argument-by-argument betterness tie (each is better on one argument), so the call is ambiguous — **CS0121**. An explicit `nint i` always resolved (both overloads then infer `T=nint`, and `slice<T>` beats `ISlice` on the first argument), which is the tell that the problem lies in the *element type* the index infers, not the converter's `var` (correct and idiomatic) nor the `append` overload set (unambiguous for a correctly-typed `nint`). The answer is therefore in golib: `range(nint)` yields `nint`. It returns golib's `RangeOverInt<nint>` struct, whose struct enumerator yields `0..n-1` without allocating, and it matches Go's integer-range semantics for `n <= 0` exactly (zero iterations), where `Enumerable.Range` throws on a negative count. The `maps` and `slices` test suites (`want = append(want, i)` over a `range(size)` index) depend on exactly this. Guarded by the `RangeIntIndexAppend` behavioral test (the minimal `append(s, i)`-over-`range(size)` shape, output-compared vs Go; neutering golib's `range` to `IEnumerable<int>` reproduces the CS0121).
<!-- history: golib's range(nint) originally returned Enumerable.Range(0, (int)n); the switch to RangeOverInt<nint>
was a
pure golib change (byte-identical check-no-regression) that corrected every range-over-int index in the corpus and
unblocked the maps and slices Phase-4 test suites. -->

## Range-over-integer covers EVERY integer type, and the iteration variable keeps the operand's width
Go 1.22's range-over-integer accepts **any** integer type, not just `int`, and gives the iteration variable *that* type. The converter therefore treats every integer kind — `uintptr`, `int64`, `uint8`, `rune`, a named integer type — as a range-over-int operand. The stakes are silent: a range operand the converter does not recognize falls through `visitRangeStmt`'s `unexpected 'ast.RangeStmt' expression` arm, which prints the statement as a **C# comment**, so the loop does not fail to convert loudly, it *vanishes*, and the program quietly computes a different answer.

A corpus site is `unique.buildArrayCloneSeq`'s `for range atyp.Len` (`unique/clone.go`), where `atyp.Len`, an
`internal/abi` field, is a `uintptr`:
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
Commented out, the body would leave `unique`'s `cloneSeq` for any array-of-string type **empty** instead of carrying one offset per element. Non-`int` range operands are rare, which makes such a loss hard to notice.
<!-- history: before every integer kind was recognized, this loop body was a /* … */ block, the only such comment in the
whole converted standard library. -->

Two pieces carry the rule. golib has a generic `range<T>(T n)`, constrained to the **operator pair the loop actually uses** (`IComparisonOperators<T,T,bool>` + `IIncrementOperators<T>`, with `default(T)` for the zero) rather than `IBinaryInteger<T>` — golib's own `uintptr` is a hand-written struct that implements the generic-math *operator* interfaces but not the `INumberBase` hierarchy, and `uintptr` is exactly the type this site ranges over. The converter then names `T` **explicitly** at every non-`int` site (`range<uintptr>(…)`, `range<rune>(…)`), because Go's `rune` and C#'s `int` are one CLR type and an inferred call could not tell `for i := range r` (yields `rune`) from `for i := range 3` (yields Go's `int`); a NAMED integer type is its own type argument with the operand passed through unchanged (`range<myLen>(((myLen)3))`), so the loop variable has the named type as in Go, and the converted `/*num:…*/` struct binds because it declares the same operator interfaces.

The `int` case is emitted as bare `range(expr)`, and it needs one guard of its own. A literal operand hands the overload set a C# `int`, where the generic is an **identity** match and `range(nint)` needs an implicit numeric conversion; the generic therefore wins outright and C#'s "a non-generic method is better than a generic method" tie-break never applies (measured, not reasoned: without the guard `range(3)` yields `System.Int32`). golib carries a third overload, `range(int n) → RangeOverInt<nint>`, meaning "a C# `int` operand is Go's `int`": an `int`-typed operand in emitted code can only be an untyped Go constant, since a Go `int` expression already renders as `nint` and every other width is emitted with an explicit type argument. With it, `for i := range 3` stays on `nint` and the CS0121 above cannot come back.

Guarded by the `RangeOverIntegerTypes` behavioral test (blank-key `uintptr`, `uint8`/`int64`/`uint64`/`rune`, a named `uintptr` type, non-positive operands, and the unchanged `int`/untyped cases, output-compared vs Go — reverting the `isInt` widening reproduces `FAIL [Target,Output]`) and by `GolibTests.GoStructLayoutTests` (the `range<T>` widths and the literal-operand overload binding).
<!-- history: behavioral check-no-regression was byte-identical apart from the new project when this landed.
at landing: 570 behavioral packages, and exactly one non-`int` range operand in the converted stdlib; neither figure
is re-measured by a guard. -->

## An index-only range over a slice or array reads no element
<!-- src/go2cs/visitRangeStmt.go:744 `} else if (isSlice || isArray) && valExpr == "_" {`;
src/core/golib/RangeIndexOnly.cs:38 `range<T>(slice<T> s) => range(s.Length)`, :50 `range<T>(sslice<T> s)`,
:58 `range<T>(array<T> a)`, :72 `range<TArray>(ж<TArray> p)`, :82 `range<TArray>(ж<TArray> p, nint length)`,
:97 `range<TSeq>(in TSeq x) where TSeq : IArray`. -->
Go's index-only range (`for i := range s`, `for i, _ := range s`, or `for range s`) reads the length once and
loads no element. The converter emits golib's index enumerator for it, `foreach (var i in range(s))`, when the
operand is a slice, an array, a pointer to an array, a named slice or array type, a type parameter whose core type
is one, or a variadic parameter that the converter emits as golib's stack-only `sslice<T>`:

```go
for i := range s { … }
for range s { … }
```
```csharp
foreach (var i in range(s)) { … }
foreach (var _ᴛ1 in range(s)) { … }
```

Each `range` overload in golib's `RangeIndexOnly.cs` reads the operand's length once and returns
`RangeOverInt<nint>`, the allocation-free enumerator of range-over-int, so `i` is Go's `int` and the loop yields
`0` to `len(s) - 1`. A slice, an `sslice<T>`, an array and a pointer to an array bind their own overloads. A named
sequence or a type parameter binds the constrained `range<TSeq>(in TSeq)`, which reads a struct operand without
boxing it. The `sslice<T>` overload has to exist because `sslice<T>` is a ref struct: without it C# reaches the
integer `range<T>(T)` and refuses the ref struct as its type argument (CS9244). A pointer to an array is passed as
the pointer, with no `.Value` spelled, beside its type's length: `range(p, N)`.

The (index, value) enumerator would not do here even with the value discarded: it reads every element into a stack
temp. An element the body waits to see collected stays reachable from the loop's own frame
(crypto/internal/fips140cache's TestCache waits for exactly that and would never finish), and an element of a
native-backed slice over memory that refuses a read panics where Go touches nothing.

The scalar escapes of range-over-int apply unchanged: a blank or absent key iterates a marked temp (see
[A blank scalar range variable never emits as `_`](#a-blank-scalar-range-variable-never-emits-as-_)), an assigned key (`for i = range s`) iterates a temp and copies it into the existing variable, a key the
body writes iterates a temp and declares the variable from it, and an address-taken key gets a fresh box per
iteration.

What keeps the pair form:
- a **string**, `foreach (var (i, _) in s)`, because its indexes are the byte offsets where each rune starts, which
  its length alone cannot produce;
- a **map**, `foreach (var (k, _) in m)`;
- a range with a **value variable**, `foreach (var (i, v) in s)`.

Go's length of a `*[N]T` is the constant `N`, so Go ranges a nil array pointer `N` times without dereferencing it.
golib's array carries its length in the value, not the type, so only the converter knows `N`, and it passes it:
`range(p, N)` reads neither the pointer nor an element, and a nil `p` ranges `N` times. The pointer is still
evaluated once, as Go evaluates a range operand that holds a call.

A pointer parameter is already the dereferenced array, either its ref-lowered form or its `DerefOrNull` entry alias.
A pointer receiver is the value it addresses (`this ref T b`). Either would range a `ref`, which is null for a nil
pointer and faults where Go ranges `N` times. Neither needs evaluating, because with at most one iteration variable and
a constant length Go does not evaluate the range operand at all. So each ranges the count itself, `range(N)`, a
pointer to a named array type included; the box overload, which cannot bind a `ref` (CS0411), is never asked to.
<!-- src/go2cs/visitRangeStmt.go:801-802 `case isArrayPointee && rangedThroughPointer && derefdValue:` then
`foreach (%s%s in range(%d))`, and :804 `foreach (%s%s in range(%s, %d))` for the box form; Go spec, For statements
with range clause: "if at most one iteration variable is present and len(x) is constant, the range expression is
not evaluated". Ruled 2026-10-25 (COORD, H's diagnosis of C2's H12 finding): before it a nil pointer parameter or
receiver faulted at `range(p)`; runtime's pageBits.setAll and image/jpeg's differ are the corpus sites. src/core/golib/RangeIndexOnly.cs:82
`range<TArray>(ж<TArray> p, nint length) where TArray : IArray => range(length)`; its one-argument sibling, kept for
hand-written callers, :72 `range(p.Value.Length)`. -->

Guarded by the converter's `TestIndexOnlyRangeEmitsTheIndexEnumerator`, which pins every operand kind above, the
scalar escapes and the string, map and value-variable controls, and a pointer to an array, a nil one, a pointer
parameter and a pointer receiver, by `TestRangeOverPointerArrayValueIsCountRange`, which pins the count range for a
lowered parameter, an aliased one, a named array and a receiver beside the box, value and slice controls; and by the
behavioral goldens `ArrayRangeSnapshot`, `SlicePointerIdentity`, `VariadicPackPassThrough`, `PointerArraySlice`, whose
`rangeIndexOnly` ranges a nil `*[3]int` and a nil `*grid` and prints Go's counts, and whose `fill` ranges its pointer
receiver, and `PointerArrayRange`, which ranges a nil lowered parameter, a nil aliased one and a nil receiver.
<!-- src/go2cs/rangeIndexOnly_test.go:219 `func TestIndexOnlyRangeEmitsTheIndexEnumerator(t *testing.T) {`;
:258 `expect("overArray", "foreach (var i in range(a))", "foreach (var i in range(p, 3))")`;
:260 `expect("overNilPointer", "foreach (var i in range(nilp, 4))")`; :266 `expect("overPointerParam", "foreach (var i in range(2))")`;
:268 `expect("setAll", "foreach (var i in range(4))")`;
:280 `expect("overString", "foreach (var (i, _) in str)")`; :281 `expect("overMap", "foreach (var (k, _) in m)")`.
src/go2cs/rangeNilPointerArray_test.go `func TestRangeOverPointerArrayValueIsCountRange(t *testing.T) {`.
src/tests/Behavioral/PointerArraySlice/main.go:63 `func rangeIndexOnly() {`, :90 `func (b *grid) fill() {`.
src/tests/Behavioral/ArrayRangeSnapshot/ArrayRangeSnapshot.go:113 `for i := range a {` -> .cs.target:106 `foreach (var i in range(a)) {`;
src/tests/Behavioral/SlicePointerIdentity/main.go:16 `for i := range s {` -> main.cs.target:13 `foreach (var i in range(s)) {`;
src/tests/Behavioral/VariadicPackPassThrough/VariadicPackPassThrough.go:11 `for i := range xs {`
-> .cs.target:12 `foreach (var i in range(xs)) {`. -->

## A blank scalar range variable never emits as `_`
A `range` with no iteration variable (or an explicit blank) over a **scalar-yield** source — a channel, an integer (Go 1.22 `for range n`), a single-value yield function, or a slice or array (whose index-only range also lowers to `range(x)`) — needs a C# `foreach` iteration variable, and that variable must **not** be named `_`: in a scalar `foreach` position C# declares a genuine read-only variable *named* `_` (only tuple-deconstruction `_` is a discard), which shadows the discard idiom for the entire loop body. Any Go blank assignment inside the body (`_ = f(x)` — evaluate and discard) then resolves to that variable and becomes an illegal write to a `foreach` iteration variable (**CS1656**; first hit: `encoding/binary`'s `BenchmarkSize`, `for range b.N { _ = Size(data) }`). The converter emits a marked temp instead:
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
Tuple positions are unaffected — `foreach (var (_, data) in …)` keeps the true C# discard. Guarded by the `RangeStatements` behavioral test (blank int-range and blank channel-range, each with a body blank assignment; the compile phase is the guard — a `_`-named iteration variable is CS1656).

## `for range` over a slice allocates NOTHING — `slice<T>.GetEnumerator()` returns a struct

`for i, v := range s` emits `foreach (var (i, v) in s)`, and Go's range over a slice allocates nothing at all. C# matches that only if the enumerator stays off the heap, which is entirely a question of what `GetEnumerator` **returns**: `foreach` binds `GetEnumerator` by **pattern** — the concrete return type, ahead of and independently of any interface — so a struct return is enumerated in place, while an interface return is a heap object per loop *entry*.

An interface return from an ITERATOR method (`yield return`) would be the worst of both: the compiler-generated state machine is one allocation and any inner enumerator class it drives is a second, two heap objects per loop entry in every ranged loop of every converted package, paid whether the loop body allocates or not. That is invisible in output and in timings at small scale, and unmissable in a Go test that asserts an allocation count: `time.TestUnmarshalTextAllocations` runs `parseRFC3339`, whose `parseUint` closure ranges its argument once per field.

`slice<T>.GetEnumerator()` therefore returns the concrete nested `slice<T>.Enumerator` struct (the shape
`List<T>.Enumerator` uses, and the one golib's own `sslice<T>` has). Two contracts go with it:

* `slice<T>` reaches its `(index, value)` sequence through `ISlice<T>` → `IArray<T>`, whose own `GetEnumerator()` returns `IEnumerator<(nint, T)>`. That interface member is an **explicit** implementation returning the same struct boxed, so an interface-typed local ranges pairs at the cost of one box. Only the pattern path is free. The pairs are `IArray<T>`'s own member rather than an `IEnumerable<(nint, T)>` base: beside the `IEnumerable<T>` that `IArray<T>` gets from `IList<T>`, a second generic sequence interface would leave C# unable to infer LINQ's element type, so no LINQ method would bind on a slice (CS1061). LINQ sees the values: `IEnumerable<T>` is the only generic sequence interface.
* go2cs-gen's `ISliceTypeTemplate` (every `type S []E` named-slice wrapper) forwards `global::go.slice<E>.Enumerator` and carries the same explicit interface member, so a named slice type ranges as cheaply as the `slice<E>` it wraps; forwarding the interface would keep the box on every `for range` over a named slice.

`array<T>.GetEnumerator()` returns a struct enumerator too, which works only because the copy Go's array range takes is placed on the range expression; see [the next section](#range-over-an-array-value-iterates-a-copy--the-snapshot-is-the-range-expressions-clone).
<!-- history: slice<T>.GetEnumerator() originally returned IEnumerator<(nint, T)> from an iterator method driving an
inner
SliceEnumerator class (two heap objects per loop entry); the struct return replaced it. IArray<T> used to derive from
IEnumerable<(nint, T)>; the pairs became IArray<T>'s own member on 2026-10-16. array<T>.GetEnumerator() kept the
iterator shape until the range snapshot landed. -->

Guarded by `SliceRangeAllocationTests` in `GolibTests`, which asserts **zero** bytes via `GC.GetAllocatedBytesForCurrentThread` across 1,000 loops (whole slice, sub-window with window-relative indices, and the nil slice), plus the interface-path equivalence. It is a measured guard on purpose: an interface return type still compiles and still produces correct output — it just allocates — so only bytes can catch the regression. Neutering to the interface return, or to an iterator body, makes it report a nonzero per-loop byte count.
<!-- measured when the guard landed (platform not recorded): 48 B/loop for the interface return, 136 B/loop for the original iterator body; SliceRangeAllocationTests.cs still cites ~136 B. -->

<a id="range-over-an-array-value-iterates-a-copy--the-snapshot-is-the-range-expressions-clone"></a>
## `range` over an ARRAY VALUE iterates a COPY — the snapshot is the range EXPRESSION's `ΔRangeSnapshot()`

Go evaluates a range expression **once** before the loop, so `for i, v := range a` over an array VALUE iterates a copy: a write to the container inside the body is invisible to every later iteration. The emitted `array<T>` (and the generated named-array wrapper) is a struct over a shared `T[]` backing, so a plain operand would ALIAS the container and read the writes back, diverging from `go run` on every such loop:

```go
a := [4]int{1, 2, 3, 4}
for i, v := range a {
    if i == 0 { a[1], a[2], a[3] = 91, 92, 93 }
    fmt.Println(i, v)                 // Go: 1 2 3 4        an aliased operand: 1 91 92 93
}
```

The range expression is therefore an array value-copy site of its own (see [Array VALUE-COPY at every transfer site](slices-and-arrays.md#array-value-copy-at-every-transfer-site-range-composite-return-send-append--deep-for-nested-arrays), whose `range` entry covers the iteration VARIABLE, not the operand):

```csharp
foreach (var (i, v) in a.ΔRangeSnapshot()) { … }        // array value  — snapshot, Go's copy
foreach (var (i, v) in h.arr.ΔRangeSnapshot()) { … }    // struct field — likewise a value
foreach (var (i, v) in r.ΔRangeSnapshot()) { … }        // named array  — the wrapper forwards it
```

**Why it is not the `.Clone()` every other transfer site takes, and this is the load-bearing part.**
Semantically it could be. But a Go array copy lives INLINE — on the stack when the
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

**A SLICE element is never re-copied, and re-copying one crashes.** `array<T>.Clone()`
re-clones elements that are themselves array wrappers, and `ISlice<T>` derives from `IArray<T>` — so a
named-slice element passed that test, while the generated wrapper's `Clone()` forwards to the
underlying `slice<T>`'s `ICloneable.Clone()`, which hands back a boxed `slice<T>`: the element cast is
then `slice<int>` → `ΔBits` and throws `InvalidCastException`. The range snapshot is a caller that
clones an array whose element is a slice: math/big's `bitsList` (`[...]Bits`, `type Bits []int`) is the
corpus site, which `TestFloatAdd` and `TestFloatMul` exercise.<!-- history: the slice-element crash was latent until
the range snapshot first cloned an array of slices; math/big's TestFloatAdd and TestFloatMul died on it before the
exclusion. The snapshot was first written as .Clone(). --> Semantically the exclusion is required anyway: Go's
array-of-slices copy copies HEADERS and
shares every backing store, which the shallow element copy does. The `ArrayRangeSnapshot`
guard pins both halves — replacing a whole element through the original is invisible to the loop
(a fresh header), while a write THROUGH a shared backing is visible.

Scoped exactly as gc's own rule is (`cmd/compile/internal/walk/order.go`'s `rangeStmt`), so three shapes stay UNCOPIED because Go copies nothing there either — and each is a control in the guard:

* **No value iteration variable.** With at most one iteration variable and a constant length, Go does not evaluate the range expression at all, so `for i := range a` reads the LIVE array through the index. It emits `foreach (var i in range(a))`, whose golib overload reads `len(a)` once and never an element. A blank value (`for i, _ := range a`) is the same case.
* **A POINTER to an array.** `for i, v := range p` shares the pointee; the emission keeps the bare `p.Value`.
* **A slice.** Its copy is the header, which the struct assignment already is.

`exprReadsValueNeedingClone` narrows the rest: only a read out of EXISTING storage (ident, selector, index, deref) can alias — a composite literal, call result, or conversion is freshly constructed and reachable by no other name, and a return already clones on its own way out.

With the operand snapshotted, `array<T>`'s enumerator reads LIVE storage and needs no capture point of its own, so it needs no iterator method: `GetEnumerator()` returns the nested `array<T>.Enumerator` STRUCT, and go2cs-gen's `IArrayTypeTemplate` / `IArrayViewTypeTemplate` forward that struct (with the explicit `IArray<T>.GetEnumerator()` member beside it) so named array types range as cheaply. Measured with `GC.GetAllocatedBytesForCurrentThread` over 1,000 loops, the pattern path costs **zero** bytes, and the boxing interface path costs one boxed struct rather than an iterator state machine. Snapshotting inside the enumerator instead would be wrong in both directions — it would allocate on every loop AND copy for the two shapes above where Go shares.
<!-- measured at landing (platform not recorded): pattern path 72 -> 0 B/loop, interface path 103 -> 79 B/loop. -->

Guarded two ways: `ArrayRangeSnapshot` (behavioral, output-compared against `go run`) mutates the container mid-loop across the array value, named-array, struct-field, nested-array, array-of-named-slices, `=`-form, mutable-range-var and aliased-element shapes, with the pointer, slice and index-only arms as controls that must NOT copy; `ArrayRangeAllocationTests` (`GolibTests`) asserts the enumerator's zero bytes with the boxing interface path as its nonzero control, the snapshot's zero on BOTH meters (object count and CLR bytes) with `Clone()` as the counted control that makes those zeros mean something, and the array-of-slices copy that must share its backing rather than throw.

---

[← Implicit Pointer Dereferencing](implicit-dereferencing.md) · [Index](README.md) · [The `go.golib` support namespace →](golib-namespace.md)
