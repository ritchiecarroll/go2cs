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

## `iter.Pull`'s coro — a symmetric handoff between two threads, and the goroutine count that had never been wired

**The narrowest cut yet through a scheduler primitive, and the one where the converted code stayed the specification.** `iter.Pull`/`Pull2` are built on two `//go:linkname` entries into the runtime, `newcoro` and `coroswitch` (`iter/iter.go:213–217`). Go describes a coro as *"a special channel that always has a goroutine blocked on it"*: `coroswitch(c)` makes the caller the blocked party and starts the party that was blocked, so control alternates and the two contexts are never runnable at once. `coroswitch_m` implements that by swapping stacks and ending in `gogo`.

**Everything else in `iter` converts faithfully, and that is why the hand-own is four methods rather than a package.** The yield closure, the `yieldNext` handshake, the `done` latch, and the deferred `recover` that turns a panic *or* a `runtime.Goexit` inside the sequence function into a `panicValue` the pulling side re-raises are ordinary Go, and `iter.cs` reproduces them line for line — panic texts (`iter.Pull: next called again before yield`) included. Only the control TRANSFER has no managed counterpart, so the seam is drawn exactly there. The converter already emits the two linkname declarations as bodyless `partial`s, so **no `manualConversionFuncs` entry is needed for them** — there is no Go body to displace — and `core/iter/iter_impl.cs` simply supplies the implementing halves. (Without it the `PartialStubGenerator` fills both with throwing stubs, which is why `iter.Pull` raised `NotImplementedException` on first use.)

The transfer itself is golib's (`go.golib.Coro`), not `iter`'s, because it is a runtime capability: Go declares it in `runtime/coro.go`, and the corpus carries the mechanically converted, permanently dead counterpart at `runtime/coro.cs`, whose body bottoms out in the `mcall`/`getg`/`newproc1`/`gogo` stubs. Four decisions are worth cribbing:

- **A thread and two one-permit semaphores, because the callee is arbitrary converted Go.** The sequence function needs a real stack, which under the CLR means a real thread — the same conclusion `Goroutine` reaches for goroutines. Capacity **one** is the point: a permit is a TURN, not a count, so a second release before the peer consumes the first would mean both sides were runnable, and `SemaphoreSlim` throws rather than letting that pass. The alternatives (an iterator state machine, a `Task`) both require the CALLEE to be written for them.
- **The coro goroutine is a real goroutine, registered before `newcoro` returns.** Go's `newcoro` creates the g synchronously and `NumGoroutine` counts it from that moment — `iter`'s own tests assert one extra goroutine on the statement *after* `Pull` returns. A thread left to register on its own time makes that count race, so `Coro.Start` waits for the handshake. **The exit side is the mirror and matters as much**: the identity is retired BEFORE the caller is released, so a puller can never observe a count that still includes a coro which has finished.
- **Panics and Goexit cross by not being special.** The body runs under `Goroutine.Run` — the same root every `go` statement uses — so a `GoexitException` ends the coro goroutine after its defers have run, a host containment policy still contains an infrastructure failure to one test, and an unrecovered panic keeps Go's fatal path. Nothing is re-implemented. What the caller sees is whatever the body recorded in the closure both sides share, which is Go's own mechanism rather than an emulation of one. The release sits in a `finally` so an escaping panic **crashes rather than hangs**: Go's outcome is process death either way, but a peer parked on a permit nobody will release wedges the run instead of reporting.
- **The token is keyed, not widened.** `iter` declares `type coro struct{}` — an empty struct whose only job is to be a token the two functions agree on. There is nowhere in it to put a rendezvous, and widening it would both diverge from Go's field set and change how `GoZeroSizeFacts` classifies it. A `ConditionalWeakTable` keyed on the `ж<coro>` box leaves the converted type exactly as Go declares it (`new(coro)` mints a fresh `StandardBox` per call, and boxes compare by IDENTITY — the property `sync`'s semaphore table already relies on), and the entry cannot outlive the token.

**`runtime.NumGoroutine` was wired in the same change, and it had never answered truthfully.** Its Go body is `gcount()`, which derives the live count by SUBTRACTION over scheduler state the managed model never populates — `allglen`, less `sched.gFree.n`, less `sched.ngsys`, less each P's `gFree.n`. Every term was zero, and `gcount`'s own `if n < 1 { n = 1 }` floor then turned the nonsense into a plausible-looking constant: **`NumGoroutine()` returned 1 for every program, forever** — exactly the shape of wrong that survives unnoticed, because a single-goroutine program's answer really is 1. golib's `Goroutine` registry had maintained the true count all along (it is what the SIGQUIT dump already printed), so this is a WIRING rather than an approximation: a `manualConversionFuncs` entry displaces the auto body and `managed_impl.cs` returns `Goroutine.Count`. Go's staleness caveat carries over unchanged and for the same reason; `gcount`'s floor does not, since the registry cannot report fewer than the caller's own goroutine.

Guarded on both sides: `iter`'s converted suite validates **28/28** against `go test` (goroutine accounting, double-next/double-yield, panic-through on `next` and on `stop`, Goexit across the boundary, immediate stop), and the `IterPullRendezvous` behavioral test pins the rendezvous semantics against `go run` in the corpus gate — deliberately printing no goroutine counts, since a count is stable in Go only under the stabilization loop that suite uses, and a flaky stdout comparison would fire across the whole corpus rather than in one package.

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

<a id="for-range-over-a-slice-allocates-nothing--slicetgetenumerator-returns-a-struct"></a>Moved to [`for range` over a slice allocates NOTHING — `slice<T>.GetEnumerator()` returns a struct](slices-and-arrays.md#for-range-over-a-slice-allocates-nothing--slicetgetenumerator-returns-a-struct).

<a id="range-over-an-array-value-iterates-a-copy--the-snapshot-is-the-range-expressions-clone"></a>Moved to [`range` over an ARRAY VALUE iterates a COPY — the snapshot is the range EXPRESSION's `.Clone()`](slices-and-arrays.md#range-over-an-array-value-iterates-a-copy--the-snapshot-is-the-range-expressions-clone).

---

[← Implicit Pointer Dereferencing](implicit-dereferencing.md) · [Index](README.md) · [The `go.golib` support namespace →](golib-namespace.md)
