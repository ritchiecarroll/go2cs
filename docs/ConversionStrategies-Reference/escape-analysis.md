# Escape Analysis and Heap Boxing
<!-- {% raw %} — Jekyll/Liquid guard: this page contains Go composite-literal and template syntax ({{ … }}) that Liquid would otherwise parse; the HTML comment hides the tag on GitHub. -->

[Reference index](README.md) · [Summary of this topic](../ConversionStrategies.md#compiled-library-versus-source-code)

This page covers when a converted local, parameter, named result or receiver lives in a heap box because Go code takes its address, and the field views a heap box hands out.

## Heap-boxed locals, parameters and results

### An address-taken reference-typed local heap-boxes too — `Ꮡ(value)` copies are only for reads
An INHERENTLY heap-allocated local (interface/pointer/slice/map/chan/func) is already a
reference, so escape analysis blanket-marks it and the box machinery historically skipped it —
`&local` fell back to the `Ꮡ(value)` **copy** constructor. That is only sound when nothing
writes through the pointer: dwarf's `zeroArray(&typ)` (with `typ Type`, an interface local)
writes `*t = &tt` in the callee, and the copy-box silently dropped the write (C# printed the
un-replaced value — a behavioral divergence, not a compile error). The box predicate
(`identHasHeapBox`) now boxes such a local when its address is **genuinely taken** — by a
capturing closure (the pre-existing box-ref-var case) or anywhere in the current function
(memoized `&ident` scan) — so `&swapped` references a real aliasing box:

```go
var swapped Animal = Dog{}
replaceAnimal(&swapped)      // callee: *a = &Cat{}
```
```csharp
ref var swapped = ref heap<Animal>(out var Ꮡswapped);
swapped = new Dog(nil);
replaceAnimal(Ꮡswapped);     // callee writes through the SAME box — "Meow!"
```

Details: the box declaration always uses the parameterless `heap<T>(out …)` form for these
(`new Animal()` on an interface is CS0144, and the reference-like zero value is exactly what
the box provides); a `[]T` slice local routes to `heap<slice<T>>` (the array-branch prefix
test mistook `[]` for an array and emitted a mismatching `heap<array<T>>`); and the
pointer-form ident render in convIdent deliberately keeps the PLAIN value render for these
locals (`new Middle(Inner: inner)` wants the held pointer; only an explicit `&inner` wants
the `ж<ж<T>>` box, via convUnaryExpr). Non-escaping and never-addressed reference locals are
unchanged (no churn). (Guarded by `InterfaceCasting`'s `replaceAnimal` — the swap is visible
through the original variable; the churned goldens `PointerToPointer`,
`UnsafePointerReinterpret`, `DerefPointerToField`, `PointerCastSliceRange`,
`EscapedLoopVarSiblingIndex` all re-verified against Go.)

### An address-taken NAMED RESULT heap-boxes too
A named result is declared in the function signature, not by any body statement, so the escape
analysis' define-walk never reached it — it was analyzed only when it also happened to sit on a
`:=` LHS somewhere. A named result whose address is taken but which is never `:=`-reassigned
(text/tabwriter's `func (b *Writer) flush() (err error) { defer b.handlePanic(&err, "Flush"); … }`,
where the deferred handler writes `*err = nerr.err`) was therefore left unboxed: `&err` fell back to
the `Ꮡ(err)` **copy** box, so the handler wrote a copy while `return err` read the original —
silently dropping the error (Go promotes an address-taken named result to the heap). The escape
analysis now walks every named result and marks it escaping when its address is genuinely taken
(`&err`, `&err.field`, `&err[i]`), so the existing heap-box machinery boxes it at entry — the box
`Ꮡerr`, the deferred handler's write through the pointer, and the final `return err` all reference
one slot:
```go
func (b *Writer) flush() (err error) { defer b.handlePanic(&err, "Flush"); … }   // handlePanic: *err = e
```
```csharp
internal static error /*err*/ flush(this ж<Writer> Ꮡb) {
    heap<error>(out var Ꮡerr);                            // box declared before the try
    GoFrame ᒐ = default;
    try {
        ref var b = ref Ꮡb.DerefOrNull();

        ref var err = ref Ꮡerr.ValueSlot;                 // value alias inside
        defer(Ꮡb.handlePanic, Ꮡerr, flushˢ, ref ᒐ);       // the BOX is passed, not Ꮡ(copy)
        b.flushNoDefers();
        err = default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return Ꮡerr.ValueSlot;                                // reads the SAME slot the handler wrote
}
```
The trigger is address-taken **specifically** — a named result merely referenced or written inside a
closure is not boxed (a C# closure already captures the outer local by reference), so a common
`defer func(){ err = wrap(err) }()` result keeps its plain declaration (no churn). The verdict is
only ever SET true, so a result whose address is never taken is byte-unchanged. Function literals
take the same treatment. (Guarded by `NamedResultAddressEscape` — an error result written through
`&err` by a deferred handler and a value `int` result mutated through `&n`, output-compared vs Go;
`PointerToInterfaceParamDeref` re-baselined to the box form, its output unchanged since its handler
only *reads* `*err`.)

### An address-taken VALUE PARAMETER heap-boxes too
A value parameter, like a named result, is declared in the **signature** — not by any body statement —
so the escape analysis' define-walk never reached it either. Parameters were additionally kept out of
the full escape analysis on purpose, which left exactly **one** parameter trigger in the pass: the
capture-mode-method check (`markCaptureModeBoxedParams`, *A capture-mode method called on a value
PARAMETER* below). A plain `&param` was not a trigger, so `&r` fell back to the call-site `Ꮡ(r)`
**copy** box — it compiles, and silently drops every write the callee makes through the pointer:

```go
func DrawMask(dst Image, r image.Rectangle, src Image, sp image.Point, …) {   // image/draw
	clip(dst, &r, src, &sp, mask, &mp)   // clip narrows r (and sp/mp) IN PLACE
	if r.Empty() { return }              // …but the narrowed r was never seen
```

The escape pass now marks a value parameter whose address is genuinely taken — `&r`, `&r.field…`
(a value-field chain rooted at the parameter), or `&r[i]` — via **`objectAddressTaken`**, the same
generic per-object scan the named-result arm above uses (renamed from `namedResultAddressTaken` now
that it serves both signature-declared categories). The existing entry-time box machinery then boxes
the parameter at entry, exactly as the capture-mode trigger does:

```go
func clipParam(r Rect) Rect { clip(&r, 5, 5); return r }
```
```csharp
internal static Rect clipParam(Rect rʗp) {
    ref var r = ref heap(rʗp, out var Ꮡr);   // ENTRY-time box, never a call-site copy
    clip(Ꮡr, 5, 5);                          // the callee writes THIS storage…
    return r;                                // …and the body reads it back
}
```

Entry-time boxing is what preserves Go's semantics on both sides: the callee's writes are visible to
the rest of the body, and the **caller's** argument stays untouched (the parameter is still by-value —
the box is initialized from the incoming `ʗp` copy). The `&param.field` form renders through the box
accessor (`Ꮡb.of(Box.ᏑR).of(Rect.ᏑMin)`) and `&param[i]` through `Ꮡa.at<E>(i)` — the latter now
superseding, at its parameter sites, the array-parameter fallback that copy-boxed the `array<T>`
*wrapper* (`Ꮡ(value).at<byte>(0)`, kept for any array base that still owns no box) and was correct
only because the wrapper shares its `T[]`. An ARRAY param folds its Go by-value clone into the box
init: `ref var a = ref heap(aʗp.Clone(), out var Ꮡa);`.

**An INHERENTLY-HEAP parameter takes only the BARE `&p` form** (`paramAddressTakenNeedsBox`). A
slice/map/chan/interface/func — and a type parameter, whose underlying is its constraint interface —
is already a reference, so only the address of the reference *variable itself* needs a box; `&p[i]`
addresses the **shared backing array**, which the emitted element form `Ꮡ(p, i)` already aliases
correctly, and Go likewise does not heap-promote a slice header for an element address. This mirrors
`identHasHeapBox`'s own box gate, which is what makes the restriction load-bearing rather than an
optimization: marking a verdict that gate then refuses would leave `identEscapesHeap` set with **no**
box, and that map is read raw by the capture analysis and several emitters. Measured on the first cut
(which did not restrict), 48 of 149 newly-boxed parameters were `&s[i]`-only slices — including
`unicode.is16`/`is32`, `slices.Equal`/`Index`, `subtle.XORBytes`, `crypto/internal/alias`, and the
Windows syscall buffer paths — every one allocating a box per call for no semantic gain.

**The analysis trigger and the emission gate must move together.** `markCaptureModeBoxedParams`
records the reason and `paramBoxReasonHolds` (read by visitFuncDecl's parameter preamble, its
`processPotentialCapture` box-ref arm, and convFuncLit's literal prologue) re-verifies it against the
declaring ident; a reason recorded by analysis but missing from the gate leaves body uses referencing
a box that was never declared (CS0103), and the reverse declares a box nothing references. Both gained
the address-taken trigger in the same change. The gate stays *narrow* in the other direction: a param
that leaks into `identEscapesHeap` some other way — a mixed `data, pc, line := …` define re-uses the
param object, so the define walker escape-analyzes it (debug/gosym's `slice`) — still keeps its
historical unboxed emission. Function-literal params take the identical treatment
(`funcLitHeapBoxParamIdents`), and a boxed param referenced from a nested closure is box-ref'd rather
than snapshot-copied (see *CaptureModeParamClosure* below), so the closure, the body, and the callee
all share the one parameter variable Go gives them.

This was the **fifth** path in the `Ꮡ(value)` copy-box family, after the pointer-to-array element,
the slice/array field of a receiver, the field-addressed value local, and the named result — and the
**value RECEIVER**, below, is the sixth and last. The corpus consumers the parameter arm corrects are
all silent-wrong-answer bugs, not compile errors:

| Package | Site | Before → after |
|---|---|---|
| `crypto/x509` `parser.cs` | `parseValidity(der cryptobyte.String)` calls `parseTime(&der)` **twice** | two independent `Ꮡ(der)` copies → one `Ꮡder`: the DER cursor now advances, so `notAfter` is parsed from the bytes after `notBefore` instead of re-parsing the same ones. `parseName`, `forEachSAN`, `parseBasicConstraintsExtension`, `parseExtKeyUsageExtension` and `parseCertificatePoliciesExtension` have the same non-advancing-cursor shape (`der.ReadASN1(&der, …)`). |
| `crypto/x509` `verify.cs` | `Verify(opts VerifyOptions)` → `systemVerify(&opts)`, `isValid(…, &opts)`, `buildChains(…, &opts)` | three separate copies → one shared `Ꮡopts`. |
| `net/http` `server.cs` | `Serve(l net.Listener)` registers `trackListener(&l, true)` and defers `trackListener(&l, false)` | the register and deregister boxed **different** copies, so the deregister's `delete(srv.listeners, ln)` could never match the registered key — every served listener leaked. Now both pass `Ꮡl`. |
| `database/sql` `sql.cs` | `(*Rows).close(err error)` → `fn(rs, &err)` plus a `withLock` closure that assigns `err` | the hook's `*err = …` wrote a copy while `return err` read the original. Now the closure (`Ꮡerr.ValueSlot = …`), the hook, and the return share one slot. |
| `testing/slogtest` `slogtest.cs` | `wrapper.Handle(…, r slog.Record)` → `h.mod(&r)` then `h.Handler.Handle(ctx, r)` | `mod` mutated a copy, so the wrapped handler received the **unmodified** record — the whole wrapper mechanism was a no-op. |

(Guarded by the `AddressOfParamWrite` behavioral test — a value parameter clipped in place through
`&r` by a callee that writes, a `&param.field` bump, an `&param[i]` bump on an array parameter, and
three controls that must not change: a read-only `&param`, an address-taken *local*, and a parameter
whose address is never taken; the caller's own argument is printed after the call to prove it stayed
untouched. Output-compared vs `go run` — under the pre-fix emission the three write cases all printed
the unmodified input.)

**The value RECEIVER is the same category — and it closes the family.** A method's receiver is the
*third* thing declared in a signature rather than by a body statement, so it failed for exactly the
reason the named result and the value parameter did: it arrives on `funcDecl.Recv`, which neither the
define-walk nor `markCaptureModeBoxedParams` (which walks `funcType.Params`) ever reaches. Two
distinct symptoms, both closed by **`markAddressTakenBoxedReceiver`**:

| Go (receiver starts `Box{Rect{0,16}, 6}` / `Trio{7,8,9}`) | Pre-fix C# | Go says | Pre-fix C# said |
|---|---|---|---|
| `func (b Box) Bumped() int { bumpBox(&b); return b.Tag }` | `bumpBox(Ꮡ(b));` | `16` | `6` |
| `func (b Box) Clipped() … { clip(&b.R, 5, 5) … }` | `clip(Ꮡ(b).of(Box.ᏑR), 5, 5);` | `5 5` | `0 16` |
| `func (a Trio) Elem() … { bump(&a[1]) … }` (array receiver) | `bump(Ꮡa.at<nint>(1));` | `7 18 9` | **CS0103** — `Ꮡa` never declared |

The array-receiver row is not a silent wrong answer but a **hard compile error**: `convUnaryExpr`'s
array-base copy-box fallback (`Ꮡ(value).at<E>(i)`, kept for an array base that owns no box) is keyed on
`identIsParameter`, and the receiver is deliberately *not* a parameter in that model — so the naive
identity-box form was emitted for a box nothing declared. Giving the receiver a real box fixes both
symptoms with one mechanism.

The convention is the parameter's, reused verbatim rather than invented: the incoming value takes the
`ʗp` name and the entry preamble re-declares the Go name as the boxed ref alias, with an ARRAY
receiver folding its Go by-value clone into the box init exactly as an array parameter does.

```go
func (b Box) Bumped() int { bumpBox(&b); return b.Tag }
func (a Trio) Elem() int  { bump(&a[1]); return a[1] }
```
```csharp
public static nint Bumped(this Box bʗp) {
    ref var b = ref heap(bʗp, out var Ꮡb);
    bumpBox(Ꮡb);
    return b.Tag;
}
public static nint Elem(this Trio aʗp) {
    ref var a = ref heap(aʗp.Clone(), out var Ꮡa);   // the by-value clone folds into the box init
    bump(Ꮡa.at<nint>(1));
    return a[1];
}
```

**The public surface is unchanged**, which is what makes the rename safe: only the receiver's *name*
moves, never its C# type, so the method stays the value-receiver extension Go's method set requires —
still callable on a value, still callable through a pointer (which copies into the receiver, so the
caller's own variable is untouched, matching Go), and still satisfying an interface it implements by
value. `RecvGenerator` is unaffected because it is gated on `IsRefRecv` (`this ref T`), and a value
receiver never carries `ref`; `[GoRecv]` is likewise emitted only for a `this ref ` signature. The box
is an implementation detail of the body, exactly as the parameter `ʗp` + heap preamble is.

Analysis and emission move together here too — `markAddressTakenBoxedReceiver` records and
**`recvBoxReasonHolds`** (read by `paramNeedsHeapBox`, which now consults `funcDecl.Recv` before the
params walk) re-verifies. The receiver's reason set is deliberately **narrower** than a parameter's:
just the address-taken predicate. A capture-mode (direct-ж) receiver is already served by
`packageDirectBoxReceiverMethods`, which emits the box *as* the receiver instead of renaming it, and a
receiver the capture analysis routed to box-ref storage must never take the `ʗp` form at all — so
neither `bodyCallsCaptureModeMethodOn` nor `isLambdaBoxRefVar` joins the receiver gate.

The receiver reuses `paramAddressTakenNeedsBox`, so an inherently-heap receiver still boxes only for
the bare `&r`. **Measured, that restriction rejects zero corpus sites today** — unlike the parameter
arm's 48 of 149 — and the reason is worth recording: the parameter over-boxings came from the first
cut *also* recording `packageCaptureModeBoxIdents`, which forces `identHasHeapBox` to grant a box. The
receiver arm never records it, so for a `&r[i]`-only slice receiver `identHasHeapBox`'s own gate
refuses the box independently and the emission is byte-identical either way. The restriction is kept
because it keeps the analysis verdict and that gate in **agreement**: marking a verdict the gate then
refuses leaves `identEscapesHeap` set with no box, and that map is read raw by the capture analysis and
several emitters.

Corpus footprint of the fix, from a two-seeded-root A/B (master converter vs fixed, both reconverting
all 305 projects into their own temp root): **3 receiver sites across 2 files** — `encoding/base64`'s
`WithPadding` and `Strict` and `encoding/base32`'s `WithPadding`, each `func (enc Encoding) … *Encoding`
returning `&enc`. All three are *correct-by-luck* under the old emission: `&enc` is the last operation,
after every mutation, so the `Ꮡ(enc)` copy carried the mutated value out. They are now one storage
identity rather than two, at the same single allocation. That there is no live victim is the point —
this path was closed at its root rather than after a sixth package was found broken by it.

**The receiver decision is MODE-STABLE, structurally** — `-stdlib` and `-tests` cannot disagree about
it, and that is worth stating because the sibling category *can*. A package-level var's address may be
taken by the package's own `_test.go`, which a production `go/packages` load never sees, so the storage
shape has to be reconciled deliberately (*A global addressed only by the package's own `_test.go` is
still heap-boxed*, below). A **receiver is function-scoped**: its address can only be taken inside its
own method body, and a production method's body is production source that no `_test.go` can add a
statement to. `markAddressTakenBoxedReceiver` reads `funcDecl.Recv` against `funcDecl.Body` and nothing
else, so admitting test files to the analysis universe cannot change its answer. Measured against the
claim rather than assumed: a whole-stdlib `-stdlib` reconvert and the `-tests` pipeline's regenerated
production `.cs` for `encoding/base32` and `encoding/base64` are byte-identical. (A board row read the
opposite from a `go2cs.exe` built *before* this fix, and filed the resulting sweep drift as an open
mode-instability that must never be banked; the retraction — and the bank — are in
[`phase4/BOARD-next-validation-candidates.md`](../phase4/BOARD-next-validation-candidates.md).)

(Guarded by the same `AddressOfParamWrite` behavioral test, extended: a value receiver bumped in place
through `&b`, a `&recv.field` clip, a `&recv[i]` bump on an ARRAY receiver, and four controls that must
not change — a `&recv[i]` on an inherently-heap **slice** receiver, a read-only `&recv`, a pointer
receiver, and a receiver whose address is never taken — plus the two public-surface cases, the boxed
value-receiver method called through a pointer and through an interface. Output-compared vs `go run`.)

### A field-addressed value local heap-boxes — `Ꮡ(x).of(…)` copy-boxes orphan writes
Escape analysis's address-of walk marked `&x` (direct) and `&x[k]` (element) but had **no
selector arm**, so a value-struct local whose FIELD address was taken in plain assignment
(or composite-literal / return) position stayed unboxed, and convUnaryExpr fell back to the
`Ꮡ(x).of(T.Ꮡval)` **copy**-box — writes through the pointer landed in the copy and were
silently lost (Go reads the write back through `x`; C# printed the original value — a
behavioral divergence, not a compile error). The walk now peels a value-field selector
chain (`x.f1.…fn`, every hop a direct `FieldVal` selection with no pointer indirection) to
its root ident and marks the root escaping, so the emission routes through the identity box:

```go
x := Thing{val: 7}
p := &x.val
*p = 99
return x.val                       // Go: 99
```
```csharp
ref var x = ref heap<Thing>(out var Ꮡx);
x = new Thing(val: 7);
var p = Ꮡx.of(Thing.Ꮡval);
p.Value = 99;
return x.val;                      // 99 — the pointer aliases x's box
```

Multi-hop chains chain the accessors (`&w.inner.val` → `Ꮡw.of(Wrap.Ꮡinner).of(Thing.Ꮡval)`),
and a field promoted through a VALUE embed roots at the local too (`&o.ev` →
`Ꮡo.of(Outer.Ꮡev)`). A hop that crosses a POINTER — an explicit `w.ptr.val` deref or a field
promoted through an embedded pointer (both are `Selection.Indirect()`) — aliases the
POINTEE's storage instead, so the root deliberately stays unboxed: `w.ptr.of(Thing.Ꮡval)`
already writes through the held box. (Guarded by `LocalStructFieldAddr` — plain, nested,
method-body, value-embed-promoted, composite-literal, and return positions plus the
pointer-hop negative control, all output-compared vs Go; the one churned golden
`UnsafePointerParamPin` — `&h.v` under `unsafe.Pointer` — re-verified.)

### A TYPE-SWITCH BINDING is escape-analyzed like any other local
Every rule above reached a variable through `info.Defs` — and a type-switch guard has no object
there (go/types: *"symbolic variables t in t := x.(type) … the corresponding objects are nil"*;
the real binding is one implicit `*types.Var` PER CASE CLAUSE, in `info.Implicits`). So no
address form on a type-switch binding was ever seen: `d.translate(&t1.Name, true)` handed a
ж<Name> **method** parameter (a position Phase A never ref-lowers — §10.1 of
`phase4/DESIGN-zh-box-reduction.md`) the `Ꮡ(t1).of(StartElement.ᏑName)` copy-box, and
encoding/xml's Token() namespace translation wrote into a heap copy that `t = t1` then discarded
— a shipped lost write, invisible to every compile gate because the copy READS correctly.

The per-case objects now join both analyses: `performEscapeAnalysisForObject` runs the standard
walk for each case clause's implicit var (body uses resolve to it through `info.Uses`, so every
arm matches by object identity), and the ref-lowering locals census tracks the same category, so
a binding whose every address-connected use feeds a lowered position still REVERTS to a plain
stack local — the fixture shapes that already aliased correctly through a lowered `ref Name`
parameter emit byte-identically. The analysis is deliberately narrowed to non-inherently-heap
bound types: a binding bound at an interface (multi-type and `default` arms always are) is
already a reference, and its no-entry state is load-bearing for the capture analysis.

On the emission side a C# pattern variable cannot be a ref local, so an escaping binding binds
the pattern to a uniquely-numbered temp and opens the clause with the entry-time box pattern
proven by the escaping-parameter preamble and the select comm-clause binding:

```go
switch t1 := tok.(type) {
case StartElement:
    d.translate(&t1.Name, true)    // write must land in t1
    t = t1                          // …because Go reads it back out
```
```csharp
case StartElement t1ᴛ1: {
    ref var t1 = ref heap(t1ᴛ1, out var Ꮡt1);
    d.translate(Ꮡt1.of(StartElement.ᏑName), true);
    t = t1;
```

The gate is `identHasHeapBox` — the exact predicate the body's `&name` emission consults — so the
box is declared iff it is referenced, and a binding with no escaping use keeps today's direct
pattern binding byte for byte. (Guarded by `TypeSwitchBindingAddress` — the xml shape through a
ж-parameter method, a held `p := &t1.n` pointer, the direct `&t1` form, and the already-correct
slice-element control `&t1.attr[i]`, all output-compared vs Go.)

### A PACKAGE-LEVEL function literal's own locals are analyzed too
Every heap-box rule above is decided by the escape-analysis pass, and that pass reached a variable
only through its declaring **function declaration**: the driver walked `*ast.FuncDecl` bodies and ran
the define-walk (`:=`, `var`, range/for/if/switch/type-switch init defines) against each body. A
function literal that is not inside any declaration — a **package-level `var` initializer** — hit a
separate arm that marked its parameters and named results but never ran that walk, so **none of its
own locals were ever analyzed** and every one of them stayed unboxed, whatever the body did with it.

That is the shape of every Go test table (`var tests = []struct{ name string; f func() }{{"…",
func(){ … }}}`) and of the `sync.OnceFunc`/`OnceValue` package-level initializers, and it produced
both failure modes at once:

```go
var InitWSA = sync.OnceFunc(func() {          // internal/poll fd_windows.go
    var d syscall.WSAData
    e := syscall.WSAStartup(uint32(0x202), &d)   // fills d
    …
})
```
```csharp
// before — Ꮡ(d) COPY-boxes: WSAStartup filled a copy and the Winsock data was lost (silent)
Δsyscall.WSAData d = new();
var e = Δsyscall.WSAStartup((uint32)0x202, Ꮡ(d));
// after — the identity box, exactly as an in-declaration local has always emitted
ref var d = ref heap(new Δsyscall.WSAData(), out var Ꮡd);
var e = Δsyscall.WSAStartup((uint32)0x202, Ꮡd);
```

The compile-visible half is a capture-mode receiver: sync `mutex_test.go`'s `misuseTests` table does
`var mu sync.Mutex; mu.Unlock()` inside such a literal, and an unboxed `mu` emits the VALUE receiver
form, which binds no `ж<Mutex>` extension overload at all (CS1929 ×16). The define-walk is now a
shared helper both arms call, so a package-level literal gets byte-identical treatment to a
declaration body. Literals **nested inside** a declaration were already walked against the enclosing
body (a superset), and the analysis short-circuits per object, so the overlap is a no-op — the
behavioral corpus is byte-identical and the full-stdlib footprint is exactly the three package-level
initializers that needed it (`internal/poll`, `internal/syscall/windows`, `internal/sysinfo`).
(Guarded by `PkgLevelFuncLitLocals` — every declaration form inside a package-level literal (`var`,
`:=`, for/if/switch init, range value, a nested closure) plus a standalone package-level literal and
an in-declaration control, output-compared vs Go.)

## Method values

### A pointer-receiver METHOD VALUE heap-boxes its receiver — the implicit `(&x).M`
Go's spec makes `c.split` shorthand for `(&c).split` when `c` is addressable and `split` has a
pointer receiver: the method **value** binds a pointer into `c`'s own storage, so every write it
makes through its receiver is visible in `c` afterwards. That is the same escape condition as an
explicit `&c` — just written without the `&`, which is precisely why the address-of walk above could
not see it. The local stayed unpromoted and emission fell back to the copy box `Ꮡ(c).split`: it
compiled and ran, but the method mutated a **copy** and the caller's writes were silently dropped.
bufio's `s.Split(c.split)` is the real site — the scan counter never decremented, so the reader
"stopped with 10000 left to process".

Escape analysis now recognizes a method value that selects a pointer-receiver method on the local's
own storage — the bare ident, or a non-indirect value-field chain rooted at it, reusing the same
root walk the explicit-`&` arm uses — and marks it escaping, so the emission becomes the aliasing
box:

```go
c := counter{n: 100}
sum := applyInt(c.dec, 5, 7)       // (&c).dec — Go: c.n is 88 afterwards
```
```csharp
ref var c = ref heap<counter>(out var Ꮡc);
c = new counter(n: 100);
nint sum = applyInt(Ꮡc.dec, 5, 7);   // Ꮡc aliases c — was Ꮡ(c), a copy
```

The rule is position-general (argument, assignment, composite element, return) and covers value
**parameters** and named **results** as well as locals: a parameter takes the entry-time box
(`ref var c = ref heap(cʗp, out var Ꮡc);`) rather than a call-site copy, exactly as the capture-mode
call form already did — the three emitters that materialize a parameter box now share one predicate
(`paramBoxReasonHolds`), so an analysis reason can no longer be recorded without its box being
declared (CS0103). An **inherently-heap** named slice/map/chan receiver takes the box too (`Ꮡ(l)`
would clone the slice header, orphaning `*l = append(*l, v)`).

Three boundaries stay deliberately outside the rule. A direct **call** `c.dec()` is not a method
value: it binds C#'s `this ref counter c` extension receiver against the variable and is already
correct, and promoting for it would heap-box every local that calls a pointer-receiver method. A
**pointer-typed base** (`p.M` where `p` is `*counter`) passes the pointer value and takes no address
of `p`. A **value-receiver** method value (`c.peek`) copies the receiver at evaluation time in Go —
which is what the existing lambda-snapshot emission (`var cʗ1 = c; … cʗ1.peek()`) already does.
(Guarded by `MethodValueReceiverEscape` — local, value parameter, named result, value-field chain,
named-slice, and closure-formed method values, plus all three negative controls, output-compared vs
Go. Note the still-open residue: a method value bound to a *variable* — `f := c.dec` — takes the
lambda-snapshot path, which copies the receiver even when the local is promoted.)

A **blank-identifier element** in a split multi-assign is a C# discard, never a declaration. Go's `_, _, _, _ = a, b, c, d` (a common "mark these used" idiom) is emitted as one bare discard per element with **no** `var` — the per-element discard test keys off each LHS ident, not just the single-LHS case, so every blank stays a discard:

```go
_, _, _, _ = fi, fn, gi, gn
```
```csharp
_ = fi;
_ = fn;
_ = gi;
_ = gn;
```
A blanket `var _` on each would declare `_` once and then collide on every later element (CS0128 "a local named `_` is already defined"). (Guarded by the `BlankIdentifierCollision` behavioral test; runtime hits it in `softfloat64`'s `fdiv64`.)

## The field-view cache

### `Ꮡx.of(T.Ꮡf)` returns ONE view per (box, field) — the field-view cache

Every `recv.field.Method()` whose callee takes a `ж<FieldT>` receiver is emitted as
`Ꮡrecv.of(T.Ꮡfield).Method()`, and until 2026-09-05 `of()` minted a fresh `FieldRefBox` on EVERY call:
64 B and one counted object per call, plus the accessor-wrapper weak-table lookup the typed overload
paid. The seg-3 sizing censused 965 receiver-base and 218 parameter-base call sites in 52 stdlib
packages paying that box after the ref-primary machinery had freed 1,023 of the 2,036 shape sites
(`os`'s want-zero row carried exactly one per op).

Since the field-view cache (`golib/ж.Views.cs`, design `docs/phase4/DESIGN-field-view-cache.md`),
`of()` returns the SAME `FieldRefBox` for the same (box, accessor token) for the box's life — minted
on the first call, reused afterwards. Nothing in the emission changes; what changes is the identity
contract at run time, and it changes in the STRONGER direction: `FieldRefBox.Equals` was already
(source, token) — `&x.f == &x.f` is Go pointer identity, and the address-keyed semaphores in the
hand-owned `sync` / `internal/poll` implementations depend on it — and two calls now return the same
object rather than two equal ones. A chain (`Ꮡc.of(Conn.Ꮡin).of(halfConn.ᏑMutex)`) caches its inner hop
on the outer view. The nil box never caches (it is a shared static per `T` with no field to alias).

Where the cache lives is a type gate: a `SlottedStandardBox<T>` carrying one slot (+8 B) is minted by
`Ꮡ<T>()` / `@new<T>()` once `BoxShape<T>.Slotted` is set — by a `[GoBoxViews]` attribute on `T`, or
flipped lazily by the first `of()` asked of any `ж<T>` — and every other box kind (a `StandardBox`
minted before its type flipped, `ElemRefBox`, `NativeBox`) falls back to a per-`T`
`ConditionalWeakTable`. The lazy flip's non-determinism is deliberate and named at the site:
correctness is identical on both paths (the same view comes back), only the COST placement varies
across a process's early life. Guarded by `GolibTests/FieldViewCacheTests.cs` (identity, the byte split
by type, the chain hop, the fallback, the nil box, the negative arm, concurrent first calls — the last
of which caught a publish race before the cut was announced).

---

[Index](README.md)
<!-- {% endraw %} -->
