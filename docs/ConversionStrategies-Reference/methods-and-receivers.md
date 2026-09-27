<!-- {% raw %} — Jekyll/Liquid guard: this page contains Go composite-literal and template syntax ({{ … }}) that Liquid would otherwise parse; the HTML comment hides the tag on GitHub. -->
# Functions, Methods and Receivers

[Reference index](README.md) · [Summary of this topic](../ConversionStrategies.md#functions-and-methods)

This page covers how a Go method's receiver is emitted: when a pointer receiver is the heap box itself rather than a `ref` to the value, how a receiver or pointer parameter is repointed, and how capture-mode methods reach a field's real storage.

## Receivers

### A pointer local captured by a closure that takes its address

A **pointer (or other inherently-heap) local** captured by a closure that takes its address needs the box too, but reaches it by a different route. A local of an *inherently heap-allocated* type — a pointer, slice, map, channel, interface, or func — is already a reference, so it normally gets **no** heap box (the `convertToHeapTypeDecl` path returns nothing for such types). But when one is captured by a closure that takes its address (`mToFlush := &node{…}; run(func(){ prev := &mToFlush; … *prev = mToFlush.next })`), the closure needs a *shared* box so writes through `&mToFlush` inside it reach the outer function's storage. The converter detects this as the same *box-ref* mark used above (an inherently-heap local whose address is taken inside a lambda), and for a box-ref local it now emits the heap box even though the type is inherently heap — `ref var mToFlush = ref heap<ж<node>>(out var ᏑmToFlush)` — so the box `ᏑmToFlush` (a `ж<ж<node>>`, i.e. a `**node`) exists for the closure to reference. Without it the closure emitted `ᏑmToFlush` for `&mToFlush` against a never-declared box (CS0103); a same-function `&ptr` with **no** closure still takes the `Ꮡ(ptr)` copy form (a copy is fine there — no shared storage is needed), so that case is unchanged.

Reading such a box needs care, because for a box-of-pointer the held value can legitimately be nil while the box itself is a real allocation. `Ꮡm` here is a `ж<ж<node>>` (a `**node`), so `Ꮡm.Value` reads the *held pointer value* — not a dereference of `Ꮡm` — and in Go reading `*(&p)` when `p` is a nil `*T`/slice/map yields the nil value, with no dereference and no panic. The strict `ж<T>.Value` getter (which panics on a null stored value by design, so a genuine `*p` on a nil pointer still throws) would wrongly panic on that read. So the converter emits the golib `ж<T>.ValueSlot` accessor for these box-of-pointer reads — identical to `.Value` but without the nil-pointer-dereference check, returning the *real* slot so reads and writes both persist (and unlike the retired `DerefOrNil`, which yielded a throwaway slot for a genuinely-nil box). `ValueSlot` is selected here for a box-ref **local** of inherently-heap type; a deref'd pointer *parameter* reaches the same slot through `DerefOrNull()`, whose non-nil path IS `ValueSlot` — see *The THREE deref accessors of `ж<T>`*. The `heap(out …)` / `heap(target, out …)` helpers likewise return `ref pointer.ValueSlot`: a freshly allocated box is structurally non-nil, so the getter's nil check there is always spurious (identical to `.Value` for a value-type box; it just avoids a spurious panic when establishing the `ref var mToFlush = ref heap<ж<node>>(out var ᏑmToFlush)` alias). A genuine dereference of the held pointer (the second `.Value` in `ᏑmToFlush.ValueSlot.Value.v`) stays strict and still panics on nil — preserving Go's "panic ⇒ panic" semantics, and complementing the deliberate strict-`.Value` design at every genuine USE site. (Guarded by the `ClosureCapturedPointerAddress` behavioral test — a closure that takes the address of a captured pointer local, walks a linked list by reassigning *through* that address and mutating each node, with the outer function observing both the reassignment-to-nil and the persisted mutations, proving the box is shared rather than copied. Mirrors runtime's `trace.go` `mToFlush := allm; systemstack(func(){ prev := &mToFlush; … mToFlush = mToFlush.next })`, ~4 CS0103.)

A pointer-receiver method called **through a FIELD of such a boxed pointer local**, inside the closure, field-refs through the **held pointer**, not the box. The receiver of `c.flushGen.Store(…)` (runtime `mcache.go`'s `allocmcache`, inside `systemstack`) is taken via the &-machinery, and inside a lambda the box-ref address form substitutes the capturable box for the uncapturable ref-local alias. For a *value*-struct local (box `ж<T>`) and a deref'd pointer *parameter* (box `ж<T>` — the Go pointer itself) the bare box is the correct `.of()` receiver — but a boxed pointer LOCAL's box is `ж<ж<T>>`, one level above the `ж<T>` the field accessor projects from, and feeding it to `.of` fails inference (CS0411 — the one error that skip-cascaded ~237 packages behind `runtime`). Such a base declines the bare-box form and falls through to the pointer-variable field arm, whose ident render reads the box the same way every other in-lambda value use does: `Ꮡc.ValueSlot.of(mcache.ᏑflushGen).Store(…)` — `.ValueSlot` because reading the held pointer out of the box must not nil-check (the dereference happens in `.of`, preserving panic semantics), and because that slot IS what the enclosing `ref var c = ref heap<ж<mcache>>(out var Ꮡc)` alias reads. When such a local is **named after its own type** (`gauge := newGauge()`), the accessor's owning-type name additionally qualifies with the package class (`Ꮡgauge.ValueSlot.of(main_package.gauge.Ꮡv)`): the enclosing `ж<gauge>`-declared local stays visible inside the lambda, so the bare type name binds the uncapturable ref-local (CS8175) with no identical-simple-name fallback — the declared type differs from the type name. (Guarded by the `ClosurePtrLocalFieldMethod` behavioral test — the `allocmcache` shape: a pointer local written inside a closure and immediately method-called through a value field, read back after the closure, plus the named-after-type variant; output-compared vs Go, proving the write-through and the field-method call both bind the one shared box.)

A **deref'd pointer parameter or pointer receiver** captured by a closure is box-ref'd the same way, even when only its *value* is used inside the closure (not its address). Such a parameter is emitted as the box `ж<T> Ꮡp` with `ref var p = ref Ꮡp.DerefOrNull()`, and the `ref`-local alias cannot be captured (CS8175). Inside the closure a value use becomes `Ꮡp.Value.field` and an address use `Ꮡp`, so the closure captures the box by reference — matching Go capturing the pointer. (Guarded by the behavioral test `PointerParamCapturedInClosure`; the runtime captures `*maptype` / `*m` parameters this way pervasively.)

A pointer **receiver** captured by a closure needs an extra step the parameter case does not: the box `Ꮡp` only exists if the method is emitted **direct-ж** (the box passed *as* the receiver, `this ж<T> Ꮡp`). A normal pointer-receiver method is `[GoRecv] this ref T p` (a value-ref receiver, with the `ж<T>` companion generated separately), which has no box for the closure to reference. So "the receiver is referenced inside a function literal" is a **direct-ж trigger** — a fourth one alongside taking a field's address (`&p.field`), returning the receiver (`return p`), and using the receiver as a bare pointer value (`p.next = p`, `p != q`). Mirrors runtime's `func (p *_panic) nextFrame() { systemstack(func(){ … p.lr … }) }`. A closure parameter that shadows the receiver name resolves to a distinct object, so it does not falsely trigger the promotion. (Guarded by the `ReceiverCapturedInClosure` behavioral test — receiver captured by an immediately-invoked closure that reads/writes through it, by one that takes a field's address, and by one that is *returned* so the box must outlive the call.)

Once a method is direct-ж, its receiver is the box `Ꮡc`, but the deref'd value alias `ref var c = ref Ꮡc.DerefOrNull()` is what most uses see. When such a receiver is passed **whole** as a pointer argument — `stackcache_clear(c)` in `func (c *mcache) prepareForSweep()` — the argument must be the box `Ꮡc`, not the value alias `c` (a value cannot bind a `ж<mcache>` parameter → CS1503). A deref-aliased pointer *parameter* is already handled (it is an `identIsParameter`), but a direct-ж *receiver* is not a parameter, so the call-argument conversion recognizes it explicitly and emits the box. (Guarded by the `DirectBoxReceiverPassedWhole` behavioral test.)

The receiver placed whole into a **composite-literal element** whose field is a pointer — `func (f *_func) funcInfo() funcInfo { …; return funcInfo{f, mod} }` (runtime `symtab.go`; `funcInfo`'s first field is the embedded `*_func`) — needs the same box, and is itself a **direct-ж promotion trigger** (`bodyUsesReceiverAsPointerValue`'s composite arm): a boxless `[GoRecv] ref` receiver has no `Ꮡf` to place in the field (CS1503). Once promoted, the composite renders the box through the existing pointer-field element machinery: `new ΔfuncInfo(Ꮡf, mod)`. Both positional and keyed elements trigger, gated on the **field's declared type being a Go pointer** (resolved positionally or by key from the composite's struct type — the element expression's own type is always `*T` for a pointer receiver): a receiver placed into an *interface*-typed field also typechecks in Go, but that emission compiles today, and promoting for it would re-route every such method stdlib-wide (the field gate trims the first-cut 73-file audit to 68 — the shape is genuinely pervasive: go/types' Checker methods, net/textproto's dotReader{r: r}, zstd readers — every audited site the same signature+box re-routing) — its pointer-identity semantics are logged as a separate question. (Guarded by the `DirectBoxReceiverPassedWhole` extension — positional + keyed composites, identity verified by writing through the wrapped pointer and reading the original.)

The same composite arm also fires when the receiver is stored **as an element of a SLICE or ARRAY literal whose element type is a pointer** — `func (s *UserTaskSummary) Descendents() []*UserTaskSummary { descendents := []*UserTaskSummary{s}; … }` (internal/trace `summary.go`). Without promotion the boxless `[GoRecv] ref` receiver renders the value alias `s` into a `ж<T>[]` slot (CS0029); once promoted direct-ж, the element renders the box: `new ж<UserTaskSummary>[]{Ꮡs}.slice()` (and `[2]*T{s, other}` → `new ж<T>[]{Ꮡs, Ꮡother}.array()`). Gated on the **slice/array element type being a pointer** (the `*types.Slice`/`*types.Array` arms of `bodyUsesReceiverAsPointerValue`), mirroring the struct-field pointer gate. (Guarded by the `ReceiverPointerValue` extension — the receiver stored into a `[]*ring` and a `[2]*ring` literal, `chain[0]` identity verified by mutating through the stored pointer and reading back through the receiver.)

The same pointer-element boxing must also fire for an **ELIDED (type-inferred) nested composite** — the inner `{c}` of `[][]*Certificate{{c}}` (crypto/x509 `Verify`). The inner literal has no `Type` node; its inferred element type is `*Certificate`, and its sole element `c` is the deref-aliased `*Certificate` receiver. The typed composite path boxes a bare pointer-typed ident element (`argTypeIsPtr`), but the untyped-elided slice/array path rendered its elements with a **nil** context, so that treatment never ran and `c` emitted the value alias into a `ж<Certificate>[]` array (CS0029). The elided path now supplies a context that boxes a bare pointer-typed ident when the element type is a pointer — `new ж<Certificate>[]{Ꮡc}.slice()` — returning nil (unchanged nil-context rendering) when the element type is not a pointer or no element is a bare pointer ident, so non-pointer elided literals stay byte-identical. (Guarded by the `ElidedNestedPtrComposite` behavioral test — `[][]*Node{{n}}` where `n` is a pointer receiver.)

A **MAP** composite literal whose value or key type is a pointer boxes its element the same way — but through `convKeyValueExpr` (the `[key] = value` form), not the slice/array element loop above. `map[K]*T{k: c}` where `c` is a deref'd pointer parameter renders the value alias `c` into a `ж<T>` map slot (CS0029); the map-source branch of `convKeyValueExpr` now sets the `isPointer` ident context for the VALUE when the map's declared element type is a pointer, so a bare-ident pointer value emits its box `Ꮡc` — `new map<@string, ж<node>>{["a"u8] = Ꮡa}`. A pointer-KEY map (`map[*T]V{c: 1}`) boxes the key the same way (`new map<ж<node>, nint>{[Ꮡa] = 1}` — the `ж<T>` dictionary key matches by box identity). Gated on the map's declared **element/key type being a pointer** (not an interface — an interface-valued map still routes through the interface conversion) *and* the element expr's own type being a pointer, so a value already rendered as a box (`&x`, a pointer local) is unaffected. (Guarded by the `MapPointerElementLiteral` behavioral test — a pointer-value map and a pointer-key map built from `*node` parameters, aliasing verified by mutating through a stored value and looking up by pointer-key identity.)

### The deferring-receiver rule picks the direct-ж receiver

The **deferring-receiver rule** is a
sibling of these box-form decisions: a method that defers or recovers at FUNCTION level and also
references its receiver takes the direct-ж receiver (`this ж<T> Ꮡx`) rather than `this ref T`,
whose deref alias then emits inside the frame's `try` (`bodyWrappedInDeferContext`; fmt `ss.Token`,
guarded by `DeferCallOrder` `acc.add`). The direct-ж form is the alloc-free, race-free one, and it
is also what a deferred closure needs, since a lambda cannot capture a `ref` local.

## Repointing a receiver or parameter

### Reassigning a pointer parameter to a new pointer

**Reassigning a pointer parameter to a new pointer.** A `*T` parameter that walks memory by reassignment — `bits = addb(bits, n)` (a `*byte` step in the runtime's bitmap scanners) or `p = p.next` (a list walk) — cannot write through its value alias: `ref var bits = ref Ꮡbits.Value` makes `bits` the pointed-to *value*, and a pointer RHS (`ж<byte>`) does not fit it (CS0266/CS0029). The reassignment instead repoints the **box** and re-aliases the value var — `Ꮡbits = addb(Ꮡbits, n); bits = ref Ꮡbits.Value;` — reusing the same box-reassignment path that handles a direct-ж receiver's `r = r.prev` (the RHS already emits the box form). (Guarded by the `PointerParamWalk` behavioral test, a circular-list walk that reassigns the parameter and reads the pointed-to value each step.) Reassigning a *pointer local* (not a parameter) is unaffected — a local already holds the box.

### Reassigning the RECEIVER is itself a direct-ж trigger

**Reassigning the RECEIVER is itself a direct-ж trigger.** Go's pointer receiver is an ordinary local, so a method may repoint it to walk a structure — `func (s *fakeStmt) QueryContext(…) { for { …; if s.next == nil { break }; s = s.next } }` (database/sql's `fakedb_test.go`) — and the rebind is local to the callee. The emission is the receiver twin of the parameter case directly above (`Ꮡs = s.next; s = ref Ꮡs.DerefOrNull();`), and `visitAssignStmt`'s arm for it has existed as long as the parameter's — but it is reachable only through the box `Ꮡs`, which exists only when the method is emitted **direct-ж**. That made the repoint a trigger the pre-pass did not have: a method whose receiver is repointed but which returns nothing, compares nothing to nil and takes no field address matched none of the other nine predicates, kept the boxless `[GoRecv] this ref T s`, and the assignment put a `ж<T>` into a `ref T` (CS0029). `bodyReassignsReceiver` closes it, matching by **object identity** so an inner `:=` that shadows the receiver's name — a different variable — does not promote the method. The trigger is invisible on the production corpus by construction: every stdlib method that repoints its receiver is *also* carried by a neighbouring predicate (`container/ring`'s `Move` and `go/types`' `LookupParent` return it; `math/big`'s `fmtX`, `net/http`'s `addBytes` and `time`'s two `(*Location)` lookups likewise already emit `this ж<T> Ꮡx`), which is why the gap first surfaced in a test file — and why adding the trigger moves no production emission. (Guarded by the `PointerReceiverRepoint` behavioral test — three methods that repoint the receiver to walk a list, one summing, one advancing a bounded number of steps and reading a field afterward, one *writing* through the receiver at every step so a stale alias would scale the head repeatedly; all deliberately free of every other trigger, with the caller's head pointer proven unmoved by the callee's rebind, output-compared vs Go.)

### Reassigning a captured pointer parameter inside a closure

**Reassigning a captured pointer parameter inside a closure.** The repoint-and-re-alias above (`Ꮡp = …; p = ref Ꮡp.Value;`) rebinds a `ref`-local. Inside a CLOSURE that captured the parameter that is illegal: the re-aliased value var is an ENCLOSING `ref`-local, and C# forbids referencing an outer `ref` local inside a lambda (CS8175 — crypto/x509 `buildChains`'s `considerCandidate` closure does `if sigChecks == nil { sigChecks = new(int) }` on the captured `*int` parameter). The box reassignment `Ꮡp = …` is legal (it writes the captured box field, hoisted to a closure field), so only the ref-local refresh is dropped inside a lambda:
```csharp
if (ᏑsigChecks == nil) {
    ᏑsigChecks = @new<nint>();          // was: … ; sigChecks = ref ᏑsigChecks.DerefOrNull();  (CS8175)
}
ᏑsigChecks.Value++;
```
Every in-lambda and post-lambda dereference of a repointed captured pointer routes through the box `Ꮡp.Value`, so the now-stale value alias is never read — an accepted modeling gap (like the nil-terminated walk's), not a miscompile. The suppression is sound because no LEGITIMATE re-alias ever occurs inside a lambda: a lambda's OWN pointer parameter is passed as the box `ж<T>` (never deref-aliased), and a heap-boxed value local is written THROUGH its box (`Ꮡb.Value = …`, never box-repointed). Guarded by `ClosureReassignsPtrParam` (a closure that reassigns a captured `*int` parameter; a non-nil runtime argument keeps the reassignment branch unreached so output stays deterministic).

The same repoint-and-re-alias applies when the parameter is reassigned **from a tuple** — `(left, x, idx) = binarySearchTree(x, idx, n/2)` (runtime `mgcstack.go`) or `pp, _ = pidleget(0)` (`proc.go`). The box-reassignment triggers matched the RHS **element-wise**, so a tuple *deconstruction* (one call RHS, several LHS) never fired them — the ж<T> tuple component was assigned into the deref'd value alias (CS0029) — and element 0's raw expression type is the whole `*types.Tuple` (never a pointer), so even a first-position pointer element missed. The per-element RHS type now comes from the call's result tuple, and the emitted form is the single-assign form verbatim: `(left, Ꮡx, idx) = binarySearchTree(Ꮡx, idx, n / 2); x = ref Ꮡx.DerefOrNull();` — the same nil-deferring re-alias every repoint takes (`(Ꮡpp, _) = pidleget(0); pp = ref Ꮡpp.DerefOrNull();`). The triggers are gated to a **reassigned** element: a `:=`-declared pointer element binds the tuple's ж<T> component into a fresh pointer local — which *is* the box — directly, and an inner `:=` local shadowing a parameter's name must not repoint the parameter's box (crypto/x509's `c, _, err := …cert(i)`). (Guarded by the `PointerParamNilWalk` extension — a nil-compared tuple-reassign walk plus a reassign-then-mutate-through probe, values vs Go.)

### Assigning `nil` to the parameter itself is a box repoint

**Assigning `nil` to the parameter itself is a box repoint, not a write to the pointee.** Both triggers above gate on the RHS being *pointer-typed*, and the untyped `nil` literal has no type of its own — so `p = nil` missed them, rendered against the deref'd **value** alias, and emitted `p = default!`, which **zeroes the pointed-to struct** while leaving the box `Ꮡp` non-nil. The caller's `!= nil` then still passed and it walked a wiped-out object. This is regexp's `makeOnePass`, whose `p = nil` (the "not one-pass after all" bail-out) handed `compileOnePass` an `onePassProg` with an **emptied `Inst` slice** instead of a nil pointer — an index-out-of-range in `cleanupOnePass` on every pattern the one-pass analysis rejected, which is most of them. A nil RHS is now treated as pointer-valued whenever the corresponding target is pointer-typed, so it takes the ordinary repoint-and-re-alias form (nil-DEFERRING, as every repoint is):

```go
// regexp/onepass.go — makeOnePass
if !check(pc, m) { p = nil; break }
…
if p != nil { for i := range p.Inst { p.Inst[i].Rune = onePassRunes[i] } }
```
```csharp
if (!check(pc, m)) {
    Ꮡp = default!; p = ref Ꮡp.DerefOrNull();  // the POINTER goes nil; the pointee is untouched
    break;
}
…
if (Ꮡp != nil) { foreach (var (i, _) in p.Inst) { p.Inst[i].Rune = onePassRunes[i]; } }
```

A pointer **local** assigned nil is unaffected — a local already *is* the box, so `p = default!` is correct there. (Guarded by the `PointerParamNilWalk` extension `dropIfShort` — nils the parameter, returns it, and the caller then proves the original node's value survived; the pre-fix converter compiles it and reports a non-nil result with a zeroed pointee.)

### Nil-terminated walk

**Nil-terminated walk.** A pointer-parameter walk that stops at a nil terminator — `func sumList(p *node) int { for p != nil { total += p.val; p = p.next } }` — needs two extra pieces, *modeled together*:

1. **Compare the box, not the value alias.** The loop guard `p != nil` must emit `Ꮡp != nil` (the box). Each binary operand's pointer context is otherwise taken from the *other* operand's pointer-ness, and `nil` is not a pointer type — so the param would convert in value form (`p != nil`, comparing a `node` struct value, the wrong thing). The converter forces the box form for a deref'd pointer *parameter* in a `==`/`!=` comparison. This is safe only for a parameter: a pointer *local* is already the box, and forcing it would emit a non-existent `Ꮡlocal`.
2. **Nil-deferring re-alias.** On the final step `p.next` is nil, so `Ꮡp = p.next` repoints the box to nil; re-aliasing through the plain `Ꮡp.Value` getter would then throw a nil-pointer dereference before the guard is re-checked. The deref/re-alias instead routes through the golib `ж<T>` extension `Ꮡp.DerefOrNull()`, which binds `Unsafe.NullRef<T>` when the box is nil — legal to HOLD, faulting only on USE — rather than throwing at the bind. The entry alias uses it too, so an empty-list call (`sumList(nil)`) binds without faulting at entry.

```csharp
internal static nint sumList(ж<node> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();
    nint total = 0;
    while (Ꮡp != nil) {
        total += p.val;
        Ꮡp = p.next; p = ref Ꮡp.DerefOrNull();
    }
    return total;
}
```

`DerefOrNull()` is **not** a substitute for a genuine dereference: reading or writing `*p` on a nil pointer (`~Ꮡp` / `Ꮡp.Value`) still panics, preserving Go semantics — and so does a read THROUGH the bound null ref, which is the whole point. Neither piece needs a predicate any more: piece 1 (the box-form comparison) is a property of the expression, and piece 2 is what EVERY pointer entry alias and repoint now emits, because a repoint is not a dereference in Go and a nil argument is not an error in Go (see *A pointer PARAMETER is nil-deferring for exactly the reason a receiver is*). Historically both were gated on the body nil-COMPARING the parameter, which covered the reassigned walk above and a nil-testing body invoked with a literal-nil argument (`defer closeIt(nil, 3)` → `p == nil`) — at the cost of a shared `default(T)` slot that let an *unguarded* deref of an actually-nil argument read a silent zero where Go panics. The unconditional accessor keeps the walk working and drops the trade. (Guarded by the `PointerParamNilWalk` behavioral test — a nil-terminated sum, a mutate-through-the-parameter pass, and an empty-list call — plus `DeferTypelessReturns`' deferred nil-argument call. `PointerParamWalk` covers the never-nil circular walk.)

## Capture-mode methods

### Capture-mode methods called through a value field of the receiver
A pointer-receiver method that takes the address of one of its own fields (`func (c *Counter) Add(d int32) int32 { return bump(&c.n, d) }`) is *capture-mode*: it is emitted with the heap box **as** its receiver (`this ж<Counter> Ꮡc`) so `&c.n` can field-reference the real storage as `Ꮡc.of(Counter.Ꮡn)`. When another struct embeds such a type as a **value field** and drives it through that field — `func (f *Flag) Incr() int32 { return f.c.Add(1) }` — the call needs a `ж<Counter>` aliasing the real `f.c`. The enclosing method is therefore itself promoted to capture-mode (direct-ж), and `f.c.Add(1)` is emitted as `(&f.c).Add(1)`:
```csharp
public static int32 Incr(this ж<Flag> Ꮡf) {
    ref var f = ref Ꮡf.Value;
    return Ꮡf.of(Flag.Ꮡc).Add(1);   // f.c.Add(1) — nested field-address box
}
```
The nested `Ꮡf.of(Flag.Ꮡc).of(Counter.Ꮡn)` chain resolves each level through `ж<T>.Value` (which honors a parent that is itself a field/array reference), so writes land on the real embedded field rather than a copy. A plain (non-capture) value method called through the same field — `f.c.Get()` — is left as a normal `f.c.Get()` value call.

This field-address routing applies only to **value** fields. When the field is itself a **pointer** — e.g. cpuProfile's `log *profBuf`, accessed as `cpuprof.log` where `cpuprof` is a heap-boxed global — its C# value is *already* a `ж<profBuf>` box, so a direct-ж method binds to it directly: `cpuprof.log.close()`. Taking the field's address (`Ꮡcpuprof.of(cpuProfile.Ꮡlog)`) would double-box to `ж<ж<profBuf>>` (CS1929). The heap-boxed-receiver routing recognizes that a field selector or indexed element whose own type is a Go pointer is already a box and skips the `&`-machinery for it. This discriminates a pointer *field* of a boxed global (already a box) from a deref'd pointer *parameter* (`s` in `s.Prev()`, a value alias whose box is `Ꮡs`): the latter is a bare identifier, not a selector/index, so it is correctly still routed through `Ꮡs`. The same exclusion applies when the pointer field is reached through a pointer **local** rather than a boxed global — `s := sl.mspan; s.gcmarkBits.bytep(…)` where `s` is a `*mspan` local — which otherwise routed through the pointer-local-field address path (`s.of(mspan.ᏑgcmarkBits)`); the field value `(~s).gcmarkBits` is already the `ж<gcBits>`. (Guarded by the `PointerFieldOfBoxedGlobal` behavioral test, covering both the boxed-global `cpuprof.log.write`/`.close` form and the pointer-local `s.log.push` form; runtime exercises both pervasively, e.g. `mspan.sweep`.)

The same applies when the value field belongs to a **package global** rather than a receiver — `ctrl.total.Add(5)` where `var ctrl controller` and `total` is an atomic field. The method's box address goes through the field-address machinery, `Ꮡctrl.of(controller.Ꮡtotal).Add(5)`, not a bare `Ꮡ` prefix on `ctrl.total` (which would bind to the box variable `Ꮡctrl`, whose value type has no `total` member → CS1061). This is the form runtime uses pervasively for `gcController`, `sched`, `memstats`, etc. The **method call itself** triggers heap-boxing the global: when a pointer-receiver method is called on a (possibly nested) value field of a package value global, the escape pass marks that global address-taken so its box exists — the call site needs the box even when the global is never explicitly `&`-addressed elsewhere. This is gated on the method being ж-only (a pointer receiver): a same-package method known to be capture-mode, **or** any pointer-receiver method whose package's capture-mode set is not locally available — the latter covers cross-package atomic methods (`func (x *Uint32) Store`), which are likewise emitted with only a box receiver, so a plain value/ref of the field cannot bind them (CS1929). The walk to the global root bails at any pointer hop (a field reached through a pointer already has a real address and is handled by the pointer-local / receiver paths), so a receiver/parameter field such as `f.c` is never disturbed. (Guarded by the `AtomicValues` behavioral test's global-atomic-field case; runtime exercises this for `prof.signalLock`, `trace.seqlock`, `scavenge.gcPercentGoal`, etc.)

It also applies when the receiver is an **indexed element** of such a field — `trace.stackTab[i].dump()` (boxed global) — where the element's address goes through the box-field accessor: `Ꮡ(trace.stackTab, i).dump()` for a slice field, or `Ꮡtrace.of(T.ᏑstackTab).at<E>(i).dump()` for an array field. The same routing covers an indexed element of an array/slice reached through a **pointer** — `bh.Value[i].Load()`, where `bh` is a pointer and the element is an atomic value — emitted `bh.of(T.Ꮡval).at<E>(i).Load()`. This is gated on the called method being **direct-ж** (a box receiver): an ordinary `[GoRecv] ref` method binds to an addressable element directly, so it is left as `container[i].method()` and only a direct-ж method (which truly needs the box) is routed — avoiding needless churn on the common case. (Guarded by the `IndexedElementDirectBoxMethod` behavioral test — a direct-ж method on an array-element-through-a-pointer-parameter, with mutation persistence verified; runtime hits this on `mprof`'s `bh.Value[i].Load()`/`.StoreNoWB()`.)

A capture-mode method called on a **value local of an inherently-heap type** — a named *slice*/*map*/*chan* — also forces the box, which `identHasHeapBox` otherwise refuses. An inherently-heap type is already a reference, so a var of it is normally *not* boxed even when it "escapes" (the escape pass marks every inherently-heap local escaping and returns early). But a capture-mode pointer-receiver method — internal/trace/internal/oldtrace's `orderEventList` (a named `[]orderEvent`) with heap.Interface `Push`/`Pop` that forward the receiver to `heapUp(h, …)`/`heapDown(h, …)` — is emitted with a `ж<orderEventList>` receiver, so a plain value cannot bind it (CS1929 — `var frontier orderEventList; frontier.Push(…)`). The escape pass therefore records the capture-mode reason **in that inherently-heap early-return branch** (the only place these vars are seen, before the general address-of scan), and `identHasHeapBox` honors it — emitting `ref var frontier = ref heap<orderEventList>(out var Ꮡfrontier)` so the calls route `Ꮡfrontier.Push(…)`/`Ꮡfrontier.Pop()` through the box. A named slice/map/chan with no capture-mode method called on it stays unboxed (already a reference — no churn). (Guarded by the `NamedSliceCaptureMethod` behavioral test — a named-slice value local with `*stack` `push`/`pop` that forward the receiver to helpers, mutated and read through the same box, output-compared vs Go.)

A capture-mode method called on a **value PARAMETER** boxes the parameter **at entry** — go/format's `format(…, cfg printer.Config)` calling `cfg.Fprint(&buf, fset, file)`, where `(*printer.Config).Fprint` is transitively direct-ж (its body calls the defer/recover-wrapped `fprint` on its own receiver), so its only emitted receiver form is the box `ж<Config>` and the raw value parameter cannot bind it (CS1929 ×2). Parameters are deliberately **never** fed through the full escape analysis, so the escape pass runs only narrow, named parameter checks (`markCaptureModeBoxedParams`) rather than the general escape walk — this one being `bodyCallsCaptureModeMethodOn`, the same predicate the local-var arms use. (The companion check, `objectAddressTaken`, was added later — see *An address-taken VALUE PARAMETER heap-boxes too* above; before it, a plain `&param` did use the `Ꮡ(value)` copy-box.) For a marked param the **signature renames the incoming value to the `ʗp` form** (the variadic-prologue rename convention) and the parameter preamble declares the boxed alias:

```csharp
internal static (slice<byte>, error) format(…, printer.Config cfgʗp) {
    ref var cfg = ref heap(cfgʗp, out var Ꮡcfg);
    …
    cfg.Indent = indent + indentAdj;        // body writes hit the boxed storage…
    var err = Ꮡcfg.Fprint(…);               // …the same storage the callee mutates through the receiver
```

Entry-time boxing is the load-bearing choice: Go auto-addresses the parameter (`cfg.Fprint(…)` ≡ `(&cfg).Fprint(…)`), so a body write **before** the call (`cfg.Indent = …`) must be seen by the callee, and the callee's writes through the receiver pointer must be seen by the rest of the body — while the **caller's** argument stays untouched (by-value parameter). A call-site `Ꮡ(cfg)` copy-box compiles but silently drops the callee's writes for the rest of the function. An ARRAY param folds its Go by-value clone into the box init (`ref var b = ref heap(bʗp.Clone(), out var Ꮡb);` — the plain `b = b.Clone();` preamble line is skipped), and an inherently-heap-typed param records the capture-mode box reason exactly like the value-local arm above. Beyond this trigger and the address-taken one, a param that leaks into `identEscapesHeap` some other way — a mixed `data, pc, line := …` define re-uses the param object, so the define walker escape-analyzes it (debug/gosym's `slice`) — keeps its historical unboxed emission (`paramNeedsHeapBox` re-verifies the predicate against the declaring ident). Whole-stdlib reconvert diff: exactly go/format's `internal.cs` changed, nothing else. (Guarded by the `CaptureModeValueParam` behavioral test — a defer-promoted direct-ж method plus a transitively-promoted one called on a value parameter, with a pre-call write observed by the callee, callee writes read back after, and the caller's copy proven untouched, output-compared vs Go — and by the `CaptureModeValueParamLib`/`CaptureModeValueParamUser` cross-package pair mirroring the format→printer shape: a foreign `Config` value param, `Fprint` → defer/recover `fprint` transitive promotion, trace accumulation across two calls proving write-visibility through the foreign `ж<Config>` extension.)

When the same function **also contains a func literal or defer that references the boxed parameter**, the in-lambda references must route **through the box** — the capture analysis marks such a param box-ref (the same arm family as a deref'd pointer parameter, whose `ref var p = ref Ꮡp.Value` alias shares the exact shape). The boxed param's Go name is a `ref`-local alias, which a C# lambda cannot capture (CS8175), and the general capture-snapshot fallback (`var tʗ1 = t;` before the lambda) compiles but **divorces the closure from the boxed storage** Go shares between the closure and the direct-ж callee: a closure read misses the callee's writes through the receiver pointer, a closure write is invisible to the callee, and a deferred closure observes entry-time values instead of return-time state. With the box-ref mark, a closure read emits `var get = () => Ꮡt.Value.total;`, a closure write `Ꮡt.Value.total += 100;`, and a deferred observer `defer(() => { (result, log) = (Ꮡt.Value.total, Ꮡt.Value.log); }, ref ᒐ);` — the box `Ꮡt` is a plain `ж<T>` local, captured by reference, so every reference (body, closure, callee) hits the one boxed storage, matching Go's one-parameter-variable semantics. A **deferred direct-ж method value on the param itself** (`defer t.Add(n)`) needed no change — it already routes through the box (`defer(Ꮡt.Add, n, ref ᒐ)`), binding the receiver address at defer time exactly like Go. Whole-stdlib reconvert diff: **zero files** — no stdlib function composes a capture-mode-boxed param with a closure today, so the composition is user-code-facing and was guard-discovered. (Guarded by the `CaptureModeParamClosure` behavioral test — four compositions with write-visibility checks in both directions: a closure read that must see the callee's later write, a closure write the callee must observe (and vice versa), a deferred closure reading return-time state, and a deferred method value whose writes a sibling deferred observer reads; each output-compared vs Go, with the caller's copy proven untouched. Under the pre-fix snapshot emission all four compiled and produced wrong values.)

Entry-time boxing extends to a **function literal's own value parameter** — the original coverage walked only `*ast.FuncDecl` params, so `f := func(t Tally, m int) {…; t.Add(m); …}` rendered the raw `Tally` value against `Add`'s only `ж<Tally>` receiver form (CS1929). The escape pass marks literal params with the same one-narrow-predicate check as declaration params (walking `FuncLit` nodes **before** the define walk, so a mixed `t, y := …` re-use cannot pre-empt the verdict; a leaked-but-not-capture-mode param keeps its historical unboxed emission via the same declaring-ident re-verification). The literal's signature takes the incoming value under the `ʗp` name and its **first block statement** is the boxed re-declaration — the exact preamble form, injected before the single-return collapse (which it thereby suppresses, correctly keeping the body a block):
```csharp
var f = (Tally tʗp, nint m) => {
    ref var t = ref heap(tʗp, out var Ꮡt);
    t.total++;                    // body writes hit the boxed storage…
    Ꮡt.Add(m);                    // …the same storage the callee mutates
    return (t.total, t.log);
};
```
This applies uniformly to every literal form: an assigned literal, a call argument, a `defer func(t Tally) {…}(x)` / `go …` argument-passing target (each deferred/goroutine run boxes its own copy at entry), and — unlike the variadic prologue, which excludes them — an **IIFE**, whose names-only parameter list emits the `ʗp` name so the rebinding composes with the delegate cast. A literal with both a variadic tail and a boxed param stacks the two `ʗp` prologues (variadic slice first, matching the declaration preamble order). A **nested closure** over the literal's boxed param takes the box-ref route (never a value snapshot, which compiled but orphaned the callee's writes — `var tʗ1 = t; tʗ1.Add(9)` lost both directions of write-visibility), while the literal's **own body** keeps the plain ref-alias renders above (`t.total++`, not `Ꮡt.Value.total++`): a box-ref var whose declaring literal is the lambda currently being converted renders plain, since its box and alias are locals of that very lambda — only genuinely nested lambdas read through the box. Whole-stdlib reconvert diff: **zero files** — no stdlib literal calls a capture-mode method on its own value param today, so this is user-code-facing and guard-discovered. (Guarded by the `CaptureModeFuncLitParam` behavioral test — assigned, IIFE, deferred-argument, nested-closure, and variadic-composition shapes, each with write-visibility checked in both directions and the caller's copy proven untouched, output-compared vs Go.)

And it applies when the field belongs to a **pointer local** — `h.s.inc()` where `h` is a `*holder` local and `inc` has a pointer receiver. A pointer local holds the box `ж<holder>` directly, so the value `~` dereference of the field (`(~h).s`) is an rvalue; the `[GoRecv]` method needs an addressable receiver (CS1510 on the generated `ref`). The field's box address is taken instead — `h.of(holder.Ꮡs).inc()` — binding the `ж` overload. (A pointer *parameter* is deref-aliased to a value, so `p.s.inc()` already works without this and is left alone. This is the form runtime uses for `(*c).gp.set(…)` / `.cas(…)` in coro.)

Finally, the same rvalue problem occurs when the field belongs to a pointer reached through *another field* — `o.h.wait.add(…)` where `o.h` is a `*holder` field and `wait` is a value (atomic) field. `o.h` dereferences to an rvalue, so `(~o.h).wait` is not addressable. The receiver is routed through the box-field accessor `o.h.of(holder.Ꮡwait)`, which aliases the **real** field storage — *not* a `Ꮡ(value)` copy, which compiles but silently boxes a copy so the atomic write is lost (a behavioral bug, not a compile error). Both the explicit address form (`&o.h.wait`) and a pointer-receiver method call on the field are routed this way. This is deliberately scoped to a base that is itself a *field selector*: a bare-ident base is the method's own receiver or a deref'd pointer *parameter* (both emitted as an addressable `ref`, so `f.c.Get()` binds directly — routing them through `&` would emit `Ꮡf.of(…)` but a value-ref receiver has no `Ꮡf` box) or a pointer *local* (handled above). (Guarded by the `AtomicFieldThroughPointer` behavioral test — a mutate-then-read proves the real field is updated, not a copy; runtime exercises this for atomic fields reached through pointer chains such as `sgp.g.selectDone.CompareAndSwap` and `gp.m.mLockProfile.recordLock`.)

The base may also be a pointer **rvalue** — a pointer-returning **call** (`getg().schedlink.set(…)`, `q.tail.ptr().schedlink.set(…)`, `Δp.chunkOf(ci).scavenged.setRange(…)`, `getg().m.p.ptr().wbBuf.get2()`) or a pointer **element index** (`batch[i].schedlink.set(…)`). Go auto-derefs the pointer to reach the value field, so the converter renders the read as `(~rvalue).field`; the `~` deref is an rvalue, so a pointer-receiver method on it cannot bind (`CS1510` on the generated `ref`). Unlike a deref-aliased *parameter* (whose box is `Ꮡp`) or a *field* deref (handled above), the call/index value **already is** the `ж<T>` box, so the receiver is materialized straight through it via the box-field accessor — `getg().of(g.Ꮡschedlink).set(…)`, `batch[i].of(g.Ꮡschedlink).set(…)` — never a `Ꮡ(value)` copy (which would lose the write). The routing is scoped to a base that is **not** an ident and **not** a field selector (those are the param/receiver/local/field cases above) and is **not a type conversion**: a conversion `(*T)(p)` renders as a C# *cast* (`(ж<T>)(uintptr)(…)`), a low-precedence form on which a trailing `.of(…)` would mis-bind to the inner operand, so a pointer-reinterpret keeps its existing `Ꮡ(…)` form (the runtime-unsafe S1 territory). (Guarded by the `PointerRvalueFieldReceiver` behavioral test — a pointer-receiver method on a value field reached through a returning call, a method-call chain, and a pointer-element index, each with write-through verified; runtime exercises this for `guintptr.set` via `getg()`/`batch[i]`/`q.tail.ptr()`, `pallocData.setRange` via `chunkOf`, and `wbBuf.get2`/`discard` via `getg().m.p.ptr()`.)

**A TYPE-ASSERTION base is a pointer rvalue too (2026-07-31).** The shape list that admits a base into
that box-field routing enumerates ident / selector / call / index / star, and a type assertion is none
of them — so `&c.(*UDPConn).conn` (net `udpsock_test`, reaching the promoted `conn.Write` through an
asserted `PacketConn`) dropped to the `Ꮡ(value)` copy-box fallback and named a `.conn` member that
`ж<UDPConn>` does not have (**CS1061**; had it bound, it would have written into a copy). The list is
about C# **precedence**, not about which node kinds have happened to come up: a base whose rendering is
postfix chains `.of(…)` cleanly, and a type assertion always renders as the postfix `c._<ж<UDPConn>>()`,
which *is* the box. Only the type-CONVERSION `CallExpr` stays excluded, for the cast-precedence reason
stated above. `exprIsValueFieldOfPointerRvalue`, the sibling predicate that decides the *routing*,
already accepted a type assertion through its default arm — so the two now agree rather than one
routing a shape the other could not render. This is the address-of copy-boxing family's next uncovered
**base** shape; the pattern of that family is that each fix covers one base shape, so the next
uncovered one is worth looking for rather than waiting for. (Guarded by the
`PointerRvalueFieldReceiver` extension — `iface.(*node).s.set(55)`, with the write read back through
the original pointer.)

It has a **second consumer, in an already-banked package**, found by the validated sweep rather than
predicted: `compress/flate`'s `flate_test.go` reaches `dict.availWrite()` through an asserted
`*decompressor`, and the old emission copy-boxed it —

```csharp
Ꮡ((~r._<ж<…decompressor>>()).dict).availWrite()                      // copy
r._<ж<…decompressor>>().of(…decompressor.Ꮡdict).availWrite()         // the real field
```

— which `flate` survived only because `availWrite` is a **read**. That is this family's signature
exactly: the copy gives the right answer until someone writes through it, and the documented
"faithful for reads" caveat is a latent wrong answer with a timer on it. `compress/flate` re-validates
at its banked 64/64 with the corrected emission.

The bare-ident-base exclusion above holds **only for `[GoRecv] ref` methods** (which bind on the addressable value alias directly). A **direct-ж** (box-receiver) method — `func (s *scavengeIndex) find(…)` and the like, emitted with a `ж<T>` receiver — needs the *box*, so calling it on a value field-chain rooted at a deref-aliased pointer **parameter or (direct-ж) receiver** is `CS1929`: `Δp.scav.index.find(force)` (root `p`, a `*pageAlloc` receiver), `mp.trace.seqlock.Load()` (root `mp`, a `*m` parameter), `h.userArena.readyList.remove(s)`. These are routed through the box-field accessor too — `Ꮡp.of(pageAlloc.Ꮡscav).of(pageAlloc_scav.Ꮡindex).find(force)` — never a `Ꮡ(value)` copy (which would lose an atomic write). The `&`-machinery recurses through the value field-chain to the param/receiver box: `&Δp.scav.index` builds `Ꮡp.of(…).of(…)`, where the box base is the **raw** parameter name (`Ꮡp`, not the shadow-renamed `ᏑΔp` — a deref param `p`→`Δp` is `ref var Δp = ref Ꮡp.Value`, box `Ꮡp`). The routing is gated to direct-ж so a `[GoRecv] ref` method on the same chain keeps binding directly (no churn); a receiver root additionally requires the *enclosing* method to be direct-ж (only then does its receiver box `Ꮡrecv` exist). (Guarded by the `FieldChainBoxReceiver` behavioral test — a direct-ж method on a value field-chain rooted at a pointer parameter and at a direct-ж receiver, both with write-through verified; runtime exercises this pervasively for `scavengeIndex`/`mSpanList`/`timers` methods and `m.trace` atomic fields.)

For the **receiver-root** case, the enclosing method only *becomes* direct-ж through the capture-mode pre-pass's transitive fixpoint: a pointer-receiver method that calls a direct-ж method on a value field-chain of its own receiver — `func (p *pageAlloc) free(…) { … p.scav.index.free(…) }` — is promoted to direct-ж so its receiver box `Ꮡp` exists for the routing above. This detection walks the **full** value field-chain `recvName.f1.…fn.method` (every hop a value, non-pointer field), not just one level: `p.scav.index.free(…)` roots `free` at the receiver `p` through two value fields (`scav`→`index`). A one-level chain (`b.u.Load()` on an embedded atomic) was already detected; the multi-level walk generalizes it. A pointer field anywhere in the chain stops the walk — that subexpression is already a box and roots the call elsewhere (the pointer-field paths above), so it must not trigger promotion. The promotion is transitive: once `pageAlloc.free` is direct-ж, its caller `func (h *mheap) freeSpanLocked(…) { … h.pages.free(…) }` is in turn promoted (now calling a direct-ж method on `h.pages`), and so on up the call graph until a root holding the value through a real box/pointer. (The multi-level receiver-root promotion is covered by the `FieldChainBoxReceiver` test's `deep.bumpDeep` case — `d.mid.c.inc()`, a direct-ж `inc` on a two-level value field-chain of a receiver with no other direct-ж trigger, write-through verified; runtime exercises it on `pageAlloc.free`/`freeSpanLocked`.)

### A direct-ж method on a value field-chain boxes through the &-machinery
A direct-ж (box-receiver) method called on a field of a plain VALUE param — netip's
`ip.addr.halves()`, where Go auto-addresses `&ip.addr` — routes the receiver through the
&-machinery: `Ꮡ(ip).of(ΔAddr.Ꮡaddr).halves()`. This boxes a COPY, which is faithful because
the enclosing Go value param is itself a copy: writes through the method could only ever reach
the local copy in Go too. (Pointer-rooted chains and indexed elements take their own
long-standing arms; this is the remaining value-rooted case.) Guarded by
`StructPointerPromotionWithInterface` (`rig`/`probeRig`).

### Field address of a collision-renamed heap-boxed local uses the raw box name
A heap box always keeps the RAW Go identifier (`ref var Δslice = ref heap<T>(out var <box>slice)`), so taking the address of a FIELD of a collision-renamed boxed local routes through `boxBaseName` -- the raw-name box, never the Δ-renamed alias (CS0103; reflect `SliceOf`'s `&slice.Type`). This matches the whole-value `&p` form and the renamed receiver/parameter boxes:
```csharp
internal static void bump(ж<nint> Ꮡnp) {
```
Guarded by `CollisionRenamedLocalBox` (`bump(&p.n)` on the renamed local `p`).

### A capture-mode method on a shadow-renamed heap-boxed local uses the rendered box name
A capture-mode method — one that escapes its receiver's address, e.g. `cryptobyte.Builder.AddASN1`, which hands `&b` to a callback — called on a heap-boxed VALUE local routes through the receiver box: `var b Builder; b.AddASN1(…)` → `Ꮡb.AddASN1(…)`. Unlike a deref-aliased pointer *parameter* (whose box keeps the RAW name, `Ꮡp`), a heap-boxed value LOCAL keeps its box under the RENDERED name — an escaping local is `ref var b = ref heap(new T(), out var Ꮡb)`, so when the local is SHADOW-renamed its box takes the renamed name. crypto/x509 `marshalCertificate`'s inner `serialiseConstraints` closure declares `var b cryptobyte.Builder`, renamed `bΔ1` to dodge the enclosing method's own `var b` declared LATER (a C# lambda cannot re-declare an enclosing-scope local, CS0136); its box is `ᏑbΔ1`. Emitting the raw-name box `Ꮡb` there both mis-references the outer `b`'s box (declared later in the method → CS0841/CS0103) and, where a same-named outer box does resolve, calls the method on the wrong operand — go/types `conversions.go` called `x.convertibleTo` on the receiver box `Ꮡx` instead of the inner operand box `ᏑxΔ2`:
```csharp
ref var bΔ1 = ref heap(new cryptobyte.Builder(), out var ᏑbΔ1);
…
ᏑbΔ1.AddASN1(cryptobyte_asn1.SEQUENCE, (ж<cryptobyte.Builder> bΔ2) => { … });   // was Ꮡb (CS0841)
```
The receiver-box render resolves the box base through `boxBaseName` with the lambda capture-remap DISABLED, so it yields: the shadow-rendered *declaring* name (`bΔ1`) for an escaping local; the raw name (`Ꮡp`) for a pointer parameter; and — critically — the *declaring* name for a variable CAPTURED by the closure, not its value-snapshot capture name. A heap-boxed local captured by a closure has its box captured directly (`Ꮡonce` in sync `OnceFunc`'s returned closure), so the capture-remapped `Ꮡonceʗ1` (a non-existent box) must not appear. Guarded by `ShadowedHeapBoxReceiver` (an inner closure's `var b` capture-mode method, shadow-renamed against an outer same-named `var b` declared later).

### A capture-mode method called on a FIELD CHAIN of a local is an address-of too
The selector arm above sees the explicit `&x.field`; Go also takes that address **implicitly**
when a pointer-receiver method is called on the field — `x.i.Add(delta)` is `(&x.i).Add(delta)`.
For a method whose receiver binds `this ref T` the emission needs nothing: C# binds the extension
on `x.i` itself, a genuine ref into the local. A **capture-mode** (direct-ж) method takes `ж<T>`
instead, so the call site must materialize a real pointer — and the escape trigger that boxes the
local recognized only the method called on the var ITSELF (`i.Store(10)`), never on a field chain
rooted at it. Unboxed, emission fell to the `Ꮡ(x).of(…)` copy-box: every atomic write landed in a
fresh copy per occurrence, and every read minted another (sync/atomic's entire 43-divergence
Phase-4 residual — `x.i.Add(delta)` returned the right value while `x.i` read back zero).
`bodyCallsCaptureModeMethodOnObject` now accepts a value-field chain rooted at the target
(`selectorChainRootsAtIdent`, the same root walk the explicit-`&` arm uses, whose
`Selection.Indirect()` gate keeps a pointer-crossing chain excluded), so the local heap-boxes and
the call routes through its identity box:

```go
var x struct{ i atomic.Int32 }
v := x.i.Add(5)                    // Go: v=5, and x.i reads 5
```
```csharp
ref var x = ref heap(new struct_x(), out var Ꮡx);
var v = Ꮡx.of(struct_x.Ꮡi).Add(5); // aliases x's box — x.i reads the write back
```

The analysis trigger and the emission-side re-verification (`paramBoxReasonHolds`) read the SAME
predicate, so value parameters take the widening in the same motion (`func f(x holder)` calling
`x.i.Store(3)` boxes `x` at entry, `ref var x = ref heap(xʗp, out var Ꮡx)`). A pointer-receiver
method that is NOT capture-mode stays untouched — `w.c.inc()` binds `ref w.c` in place, and
promoting for it would heap-box every local that calls any pointer-receiver method. (Guarded by
`CaptureModeFieldAddress` — local, value parameter, type-switch binding and lifted anonymous
struct, plus the non-capture-mode control, all output-compared vs Go.)

### The same chain one level up: `&recv.f1.f2` on a POINTER RECEIVER
The two sections above fix the LOCAL. The identical shape rooted at a **pointer receiver** went
unfixed until 2026-08-23, and it is the more dangerous of the two because the receiver is already a
pointer in Go, so the address is unambiguously real and a lost write is unambiguously a bug.

The converter recognised only the ONE-hop form `&recv.field`, which emits the field box
`Ꮡrecv.of(T.Ꮡfield)` and marks the method direct-ж so that box exists. A DEEPER chain matched no arm
and fell through to `Ꮡ(recv.f1).of(T.Ꮡf2)` — the `Ꮡ(value)` **copy**-box — so every write through the
returned pointer went into a temporary. It compiled, it ran, and it printed wrong numbers.

```go
func (b *Builder) incrementSectionCount() error {          // vendor/golang.org/x/net/dns/dnsmessage
	var count *uint16
	switch b.section {
	case sectionQuestions:
		count = &b.header.questions                        // header is a VALUE struct field
	...
	}
	*count++                                               // ... and this increment was LOST
}
```

```csharp
// before — count points into a heap copy of b.header; b.header.questions never moves
count = Ꮡ(b.header).of(dnsmessage_package.Δheader.Ꮡquestions);

// after — chained from the receiver box, so the write lands in the real field
count = Ꮡb.of(Builder.Ꮡheader).of(dnsmessage_package.Δheader.Ꮡquestions);
```

Consequence in the corpus: the DNS message Builder's header counts stayed at zero, so every message
it produced carried a question section with `QDCOUNT=0`. Any conformant parser answers
`ErrSectionDone` to that, which is why the symptom surfaced three levels away as an unexplained
resolver timeout rather than as anything resembling a lost write. The census over all 5,565
address-of sites found the hazard at exactly **four write-context sites, all in that one function**.

Both halves moved together, and that is the part worth remembering:

- `convUnaryExpr`'s receiver arm walks the chain (`receiverValueFieldChain`) and folds one `.of(…)`
  per hop. A single-hop chain reproduces the previous string byte for byte, so no existing site moved.
- `bodyTakesReceiverFieldAddress` — the scan that MARKS a method direct-ж — walks the same chain, so
  the box the emission reaches for actually exists.

**Every intermediate hop must be a VALUE struct field**, and the walk is type-aware to enforce it. A
pointer-typed hop is already its own box and the pointer-variable arm field-refs through it
correctly (`o.ptr.of(inner.Ꮡb)`); routing it through the receiver would address the pointer's own
storage instead of the pointee's field — the mirror-image defect.

**A deep chain additionally requires the enclosing method to actually BE direct-ж, and skipping that
test is a compile error waiting to happen.** Marking is driven by scanning for an *explicit*
`&recv.f1.f2`; an **implicit** address is invisible to that scan, because there is no `ast.UnaryExpr`
in the tree at all:

```go
func (h *MAC) Sum(b []byte) []byte {   // vendor/golang.org/x/crypto/internal/poly1305
	h.mac.Sum(&mac)                    // Sum is promoted from an embedded field:
}                                      // Go takes &h.mac.macGeneric implicitly
```

Emitting the box form there names a receiver the method does not have — `CS0103: The name 'Ꮡh' does
not exist in the current context`, measured on the full-corpus build. Those sites decline and keep
their value-chain form, which is already correct for them: the receiver binds `this ref T`, so
`h.mac.macGeneric` reaches the real storage and the write lands. Guarded by
`ReceiverNestedFieldAddress` (one hop, two hops, switch-selected pointer written after the switch,
read-back, and the pointer-hop negative control, all output-compared vs Go). Against the un-fixed
converter that guard compiles clean and prints `0` for every value-chain write while the pointer-hop
control still prints `3` — the defect's exact scope, and the reason a guard here had to be behavioral
rather than a golden.

---

[Index](README.md)
<!-- {% endraw %} -->
