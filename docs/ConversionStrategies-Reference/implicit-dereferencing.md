# Implicit Pointer Dereferencing

[Reference index](README.md) · [Summary of this topic](../ConversionStrategies.md#implicit-pointer-dereferencing)
**Deciding whether a selector base is *already* dereferenced.** A field selector on a pointer-valued base auto-derefs in Go, so the converter must reach the field through the box — the generated field property `x.field` where it serves (see the pointer-local rule below), the deref `(~x).field` / `x.Value.field` otherwise — *unless* the base is itself an explicit dereference (`(*p).field`) or a pointer conversion whose dedicated branch appends its own `.Value`. That "is the base already deref'd" test inspects only the base's own outermost shape (unwrapping parens; a pointer-conversion base still routes to the conversion branch), never the whole subtree: a scan for **any** `StarExpr` would mistake a conversion star buried in a call **argument** for a dereferenced base — `stringStructOf((*string)(unsafe.Pointer(p))).n` (runtime `arena.go`): the `(*string)` star belongs to the argument's conversion, the *call result* (`ж<stringStruct>`) is not deref'd, and skipping the auto-deref would leave `.n` on the box (CS1061). The conversion-branch dispatch also unwraps **enclosing parens**, so an extra-paren conversion base — `((*specialWeakHandle)(unsafe.Pointer(…))).handle` (runtime `mheap.go`) — reaches it. <!-- history: the test was once a whole-subtree StarExpr scan; the paren gap was the same extra-paren blind spot the reinterpret routing had. --> A conversion base renders through golib's `Reinterpret` (`(~viewOf(p.Reinterpret<view, header>())).n`), which aliases the source's storage wherever the managed model can represent the pair — see [A reinterpret of a MANAGED pointer aliases the box](pointers.md#a-reinterpret-of-a-managed-pointer-aliases-the-box--it-never-round-trips-through-the-address); an unrepresentable pair keeps the address route. (Guarded by the `PointerSelectorDeref` behavioral test — both shapes, read values vs Go.) <!-- Landing record: the corpus was byte-identical across all behavioral projects after the change (only previously-non-compiling shapes gained emissions); cleared 3 runtime CS1061, 74 → 71. The sentence this replaced said a WRITE through a conversion base hits a copy box; golib's Reinterpret has since aliased storage (src/core/golib/ж.PointerExtensions.cs, Reinterpret<T, TDst>), and the guard's golden shows the Reinterpret form (src/tests/Behavioral/PointerSelectorDeref/main.cs.target:24,32 (`p.Reinterpret<view, header>() … Ꮡg.Reinterpret<header, view>()`)). -->

In Go, pointer types automatically dereference; these `age` assignments are equivalent:
```go
var s struct{ age int }
var ps = &s
(*ps).age = 20
ps.age = 20
```
This also applies to receiver methods — a value-receiver method works on the type *and* on a pointer to it. In practice, the converter handles implicit dereferencing of a pointer parameter by binding a `ref` local to the box's value (through the nil-deferring `DerefOrNull()`, so a nil argument faults only where the body dereferences it — see [Pointers](pointers.md#a-pointer-parameter-is-nil-deferring-for-exactly-the-reason-a-receiver-is)). For example:
<!-- source: src/tests/Behavioral/PointerToPointer/PointerToPointer.go:71-74 -->
```go
func PrintValPtr(ptr *int) {
	fmt.Printf("Value available at *ptr = %d\n", *ptr)
	*ptr++
}
```
becomes:
<!-- source: src/tests/Behavioral/PointerToPointer/PointerToPointer.cs.target:70-75 -->
```csharp
public static void PrintValPtr(ж<nint> Ꮡptr) {
    ref var ptr = ref Ꮡptr.DerefOrNull();

    fmt.Printf("Value available at *ptr = %d\n"u8, ptr);
    ptr++;
}
```

A pointer **local** that holds a `ж<T>` box (e.g. `x := list.head`, where `head` is a `*node`) reaches a field of a named struct as Go spells it, `x.field`, read or written (parity seat #31). The site binds a C# 14 `ref` extension property that go2cs-gen emits on `ж<T>` for each struct the package's `package_info.cs` records (`[assembly: GoFieldAccess(typeof(T))]`, one per struct type). Its body is `ref ʗp is NativeBox<T> ? ref ʗp.field(T.Ꮡf) : ref ʗp.Value.f`: on a managed box the field in place, with `Value`'s own nil check; on a box over native memory the field at its Go offset. Being a `ref`, one property serves a read, a store, a compound assignment, `++`/`--`, a nested field (`o.stack.hi = …`), an index store (`req.Header[k] = vv`) and a chain through further pointers (`e.prev.next.x = …`). A read takes no copy of the whole struct. (Guarded by the `PointerFieldSelectors` behavioral test and `TestFieldPropertyReadsAsGoSpellsIt`.) The field's ADDRESS reads the same way, `&x.field` as `x.Ꮡfield`, through the `ж<F>` property go2cs-gen emits beside the field's (parity seat #32; [the rule](pointers.md#a-field-reached-through-a-pointer-reinterpreted-from-unsafepointer-reads-at-its-go-offset--the-field-property); guarded by `FieldAddressProperties` and `TestFieldAddressReadsAsAProperty`). <!-- the rule: src/go2cs/fieldAccess.go `// P1: GO'S `p.f` THROUGH ANY POINTER`, fieldPropertySelector; the body: src/gen/go2cs-gen/FieldAccessGenerator.cs Specialized. -->

These keep the deref, because no property serves them as Go spells them:
- a field named like a member of `ж<T>` (`Value`, `of`, `field`, …; `src/go2cs/fieldAccessCollisions.txt`), or one that shadows a method an embedded type promotes (`abi.ArrayType`'s `Len`);
- a **promoted** field (`x.next` where `next` comes from an embedded header): the properties are the struct's own fields, so the site would have to name the embed. The converter checks field membership recursively through embeds, so a promoted-field access still dereferences rather than leaving a bare `x.next` on the box (CS1061), as the Go runtime's `scanstack` needs for `x := state.head; … x.nobj`;
- a struct a hand-written file declares, a struct of a package the converter never emits (`testing`'s `T` and `B`), a struct the package defines over another named type (its fields are generated), a struct shaped as a slice or string header, and an unnamed struct.

The deref keeps two rules of its own. When the field access is the **LHS of an assignment** and the chain is *nested*, every dereference in the base takes the assignable `.Value` form, not `~`: `(~o).stack` yields a value (an rvalue), so assigning to a field through it is not a variable/property (CS0131). The converter propagates the assignment context down the selector chain, emitting `o.Value.stack.hi = …` where the property does not serve. **`++`/`--`** reads *and* writes its operand, so `(~mp).ncgocall++` (a field of an rvalue) is CS1059 and the converter emits the assignable `mp.Value.ncgocall++` there too.

**An INDEX-expression assignment target takes the same `.Value` write path where the deref remains.** The
field property serves `req.Header[k] = vv` directly; this rule governs the shapes above that keep the deref. When the assignment LHS
is an index over a field reached through a pointer local — net/http client.go's redirect loop
`req.Header[k] = vv`, `Header` a named-map wrapper field — the read form `(~req).Header[k] = vv`
indexer-sets a wrapper-STRUCT field of an rvalue copy (CS0131; httptest's recorder hit the same shape).
The assignment threads a dedicated `IndexExprContext.isAssignmentTarget` flag (distinct from the
general assignment context, which also rides along RHS conversions where an index READ must keep the
deref form), and `convIndexExpr` converts its BASE in assignment context: `req.Value.Header[k] = vv;`.
Compound assignment (`m[k] += 2`), `++`/`--` on an index operand (`m[k]++` via `visitIncDecStmt`), and
tuple-deconstruction elements (`(config.Value.Certificates[0], err) = …`, net/http server.cs) take the
same path. The write form is emitted for EVERY boxed index-assignment base <!-- 75 stdlib files at landing; not
re-measured --> — for
reference-backed fields (plain maps, slices, golib arrays) both forms compile and write through the
same backing store, so this is churn-free semantically; the named-wrapper fields are the shapes that
did not compile. (Guarded by `BoxedMapFieldWrite` — named-map-wrapper writes, plain-map writes,
compound/inc-dec writes, and an array-field element write, all through a pointer local, values
output-compared vs Go.)

**Dereferencing a pointer FIELD reached through a parameter — `*p.field`.** A `*p` where `p` is a pointer parameter is emitted as the value alias `p` itself (the `ref var p = ref Ꮡp.DerefOrNull()` local already denotes the pointed-to value), so the converter has a parameter-deref shortcut. That shortcut must fire only when the operand *is* the parameter (`*p`, or `**p`): for `*p.field` — a deref of a pointer *field* reached through `p` (`*gp.ancestors`, where `ancestors` is a `*[]ancestorInfo`) — the operand `p.field` is a distinct lvalue that still needs its own dereference. Keyed off the *root* identifier (`getIdentifier` digs through the selector to `p`), it would drop the field deref, emitting `gp.ancestors` (the `ж<…>` pointer) instead of `gp.ancestors.Value` — a pointer where the pointed-to value is expected: `for _, a := range *gp.ancestors` would range the box (CS8130, since a `ж<slice<…>>` is not enumerable as tuples), and `x := *p.cnt` would type a pointer as a value (CS0029). So the shortcut excludes a **selector** operand, so `*p.field` falls through to the selector-deref path and renders the field's own dereference — `h.cnt.Value` for a value read, `h.xs.ValueSlot` as a range operand. (Guarded by the `DerefPointerToField` behavioral test — a `for _, x := range *h.xs` over a deref'd pointer-to-slice field and a `*h.cnt` value read, both through a pointer parameter; runtime hit this on `traceback.go`'s `range *gp.ancestors`.) An **index** operand rooted at a parameter (`*temps[depth]`, math/big's slice-of-pointers element deref) is excluded the same way.

The **receiver** flavor of the same shortcut (`*u` inside `func (u *unifier)` → the deref-aliased `u`) has the identical hazard: keyed off the root identifier, `*u.handles[x]` — a deref of a **pointer-valued map element** reached through the receiver (go/types unify.go's `return *u.handles[x]` and `*u.handles[x] = t`, `handles` a `map[*TypeParam]*Type`) — would drop the element deref entirely, returning/assigning the raw `ж<ΔType>` (CS0266 in both directions). So the receiver shortcut is gated on the operand *being* the receiver ident (object identity, like every other receiver-specific render), and the non-direct operand falls through to the tail deref: `u.handles[Ꮡx].ValueSlot` (`ValueSlot` because the element's pointee is an interface — reference-like reads and writes both persist through the real slot). (Guarded by the `RecvMapElementDeref` behavioral test — element deref read and write through the receiver, the write observed through the shared pointer, alongside the genuine `return *r` receiver copy, output-compared vs Go.)

---

[← Pointers](pointers.md) · [Index](README.md) · [Labeled Control Flow and Loop Variables →](labels-and-loop-variables.md)
