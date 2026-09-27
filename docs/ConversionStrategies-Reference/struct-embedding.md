# Struct Type Embedding
<!-- {% raw %} — Jekyll/Liquid guard: this page contains Go composite-literal and template syntax ({{ … }}) that Liquid would otherwise parse; the HTML comment hides the tag on GitHub. -->

[Reference index](README.md) · [Summary of this topic](../ConversionStrategies.md#struct-type-embedding)
Go structs use "[type embedding](https://go101.org/article/type-embedding.html)" instead of inheritance. Since converted structs are C# `struct`s (no inheritance), the `TypeGenerator` manages the equivalent: it adds a field for the embedded type and promotes the embedded type's fields and methods (selection shorthand). Both field and method promotion are **transitive through every embedding level**: when `top` embeds `mid` which embeds `inner`, `top` gets an accessor for `inner`'s field `n` (`top.n => ref mid.n`) and a forwarding receiver for `inner`'s method `describe` (`top.describe() => target.mid.describe()`), each resolving through `mid`'s own one-level promotion. The generator collects an embedded struct's members and methods recursively (following each field whose name equals its type's simple name — Go's embedding marker), with the closest declaration of a name winning, matching Go's promotion rules. **Pointer embeds promote too.** Go also embeds by pointer (`*traceBuf`), whose C# field type is `ж<traceBuf>`; its methods and fields are promoted exactly like a value embed (the field's ref-property is dereferenced — `target.traceBuf.Value.method()` — which binds the pointer-receiver method via the `[GoRecv]` `ж<T>` overload). The embedding-marker comparison dereferences the field type first, because a pointer field's simple name carries a `.Value` suffix (`traceBuf.Value`) that would never match the bare embed field name. This matters most *transitively*: `traceExpWriter` embeds `traceWriter` (value) which embeds `*traceBuf` (pointer), and `traceBuf`'s `varint`/`byte` must promote all the way up — without the deref-aware marker the nested pointer embed is skipped and the upper struct silently loses the method (CS1929). (Guarded by the `NestedEmbeddingPromotion` behavioral test for value embeds and the `PointerEmbeddingPromotion` test for one-level and two-level-transitive pointer embeds; runtime relies on the field case for `stackWorkBuf` → `stackWorkBufHdr` → `workbufhdr.nobj` and the pointer case for the trace writers.) Because the promotion is performed at conversion time by the generator, methods added later in hand-written C# are not automatically promoted; keeping the source in Go and re-converting (or using explicit interfaces) is the maintainable path.

**Zero values of promoted-embed structs construct through a generated constructor — never `default`.** An embed is an inline field, so its own `default` is a usable Go zero value; what still is not is an embedded type that itself needs construction (a fixed-size array field, or a nested embed of one), whose `default` leaves a null backing. The generator therefore keeps allocating every embed in the type's constructors, and the converter keeps rendering an uninitialized declaration of such a struct through them. Both halves close this: the **converter** renders every *uninitialized* declaration of such a struct through the NilType constructor instead of `default!` — `var s shadowed` emits `shadowed s = new(nil);`, an uninitialized package-level `var g shadowed` emits `internal static shadowed g = new(nil);` (the addressed-global box wraps the same, `new(new shadowed(nil))`), and a named result `(r shadowed)` declares `shadowed r = new(nil);` — while the **generator** allocates the boxes in the *parameterless* constructor too, so the `new S()` zero values materialized by `heap(new S(), out var Ꮡs)` (an address-taken local) and golib's `@new<T>()` (`p := new(shadowed)`, which constructs via `Activator.CreateInstance<T>()`) are equally usable. The detection (`structHasPromotedEmbeds`, `visitStructType.go`) mirrors the embedded-field emission: an embed takes the promoted-box path unless it is a same-package interface, a builtin non-named embed (`int`), or a pointer to a non-named type; a cross-package embed (selector type) always promotes. Residual gap: an instance materialized as `default(T)` *outside* a declaration — a missing-key map read, a freshly `make`d slice's elements — still has null boxes; golib cannot run a constructor generically there. (Guarded by the `NamedTypeOverStruct` behavioral test — `var s shadowed` with explicit `s.ctxt.fn` and promoted `s.fn` access, plus `new(shadowed)`, vs Go.)

**A C#-keyword-named embed composes generated names from the unescaped member name.** A Go struct named for a C# keyword (`type base struct{…}`) is emitted with the `@` escape (`@base`), and embedding it makes `@base` the member name. Standalone identifier positions keep the escape (the `partial ref @base @base` accessor, the constructor parameter, member accesses like `instance.@base.id`), but every *composed* generated name must strip it, because `@` is only valid leading an identifier: the promoted-struct inline field and its constructor assignments emit `ʗbase` (`ʗ@base` is CS1002), matching the already-stripped `Ꮡ`-prefixed field-reference statics and the converter's `structFieldBoxName`. (Guarded by the `NamedTypeOverStruct` extension — a keyword-named embed with promoted field/method access, a keyword-keyed composite literal, and a write through `&p.id` promoted through the embed, all vs Go.)

**Cross-package embeds resolve through the semantic model.** The member-collection above resolves the embedded struct's *syntax* (`GetStructDeclaration`) — same-package or via `CompilationReference`s. In a real [MSBuild](../Glossary.md#msbuild) build, project references arrive as **metadata** references (never `CompilationReference`), so a cross-package embed — `type rtype struct { *abi.Type }` (runtime `type.go`) or a user package embedding a library struct — silently promoted **nothing**: the generated "Promoted Struct Field Accessors" section was empty and every `t.TFlag`/`t.Str`/`t.Kind_` was CS1061. The field collection now falls back to the **type's metadata symbol** (`GetTypeByMetadataName` on the normalized nested name, e.g. `go.internal.abi_package+Type`) and enumerates its public instance fields; the emitted accessors are unchanged in form — true refs through the embed (`public ref abi.TFlag TFlag => ref Type.Value.TFlag;` for a pointer embed), so writes through a promoted name reach the embedded target. Transitive promotion through a *metadata* type's own embeds is not chased (no corpus site needs it). **Promoted POINTER-RECEIVER method calls through a cross-package *pointer* embed are routed at the call site**: the generator emits no method forwarder for a metadata embed (method promotion is syntax-resolved), so `t.Uncommon()` on `Δrtype` (embeds `*abi.Type`, runtime `type.go`) was CS1929; the converter now emits the explicit hop through the embed field's box — `t.Type.Value.Uncommon()` — where the deref'd `.Value` is a ref return, binding the `[GoRecv] ref` extension addressably. A *same-package* pointer embed keeps its generated forwarder (no churn), and a promoted **value-receiver** method call (`p.Hot()`) remains a documented open gap — call through the embed explicitly. (Guarded by the `CrossPkgUser` Phase-4b extension — a promoted pointer-receiver `Calibrate` through the cross-assembly pointer embed, write-through observed via the target.) (Guarded by the `CrossPkgUser` Phase-4 extension — pointer-embed and value-embed field promotion across the assembly boundary, write-through observed via the embedded target, vs Go; cleared runtime `type.go`'s 4 CS1061, 68 → 64.)

Two refinements complete the cross-package pointer-embed story (2026-07-03, internal/reflectlite's last 4): **(a) the hop names the FIELD, which is struct-scoped** — an embed field named like a Δ-renamed package type (rtype's embedded `Type` vs reflectlite's `Type` interface, Δ-renamed `ΔType` by its type-vs-method collision) is *declared* unrenamed, so the hop emission must not apply the package-level rename (`t.ΔType.Value.Uncommon()` was CS1061); both hop arms now route through `structFieldBoxName`, the same struct-scoped naming the box accessors use. **(b) A generated interface implementation forwards through the hop too**: when an interface member has NO direct struct method and is satisfied purely by Go promotion through a single embedded-pointer field (`GoImplement<rtype, ΔType>` — `Size`/`Kind` live on `*abi.Type`), the `InterfaceImplTemplate` emits `this.Type.Value.Size()` instead of the unbindable `this.Size()` (CS1929); the `IжAdapter` template forwards the same members `m_box.Value.Type.Value.M()`. Detection is syntax-level — the converter's embed marker is the `public partial ref ж<X> F {{ get; }}` property (`GetEmbeddedPointerHopNames`) — and originally gated to a SINGLE hop, on the reasoning that multi-embed interface satisfaction was rare. The corpus surfaced one (jsonrpc's `pipe`), and the gate is gone: several embeds now route each member to the unique embed declaring it (see [*With SEVERAL embedded pointers the hop is chosen per member, not per struct*](#with-several-embedded-pointers-the-hop-is-chosen-per-member-not-per-struct)). (Guarded by the `CrossPkgUser` Phase-5 extension — a local Δ-renamed `Meter` interface colliding with the embed field name, satisfied purely by promotion through `*CrossPkgLib.Meter`, with all bump paths aliasing one shared object, vs Go.)

**A pointer-receiver method promoted through a VALUE embed is routed at the call site, not by a generator forwarder.** When `timeTimer` embeds `timer` *by value* and `timer` has a pointer-receiver method (`func (t *timer) modify(…)`), the generator emits **no** `modify` forwarder on `timeTimer` (a `target.timer.modify(…)` forwarder body would copy the value field, losing the write, and would not bind the `ж<timer>` overload) — so a promoted call `t.modify(…)` on a `*timeTimer` would leave the receiver as the whole `ж<timeTimer>` box, which the promoted method's ж/`[GoRecv]`-ref overload cannot bind (CS1929). The converter instead routes the promoted call through the embedded field's box, exactly as the *explicit* `t.timer.modify(…)` already renders: `t.of(timeTimer.Ꮡtimer).modify(…)` for a pointer local, `Ꮡt.of(timeTimer.Ꮡtimer).modify(…)` for a deref'd pointer parameter (the `&receiver.field` &-machinery supplies the correct box per receiver form). Because it field-refs the real embedded storage — never a `Ꮡ(copy)` — the mutation writes through. This is detected via the method's `types.Selection.Index()` having a single embedded-field hop (`[embeddedField, method]`); it is gated to a **value** embed (a *pointer* embed already yields the box as its field value and is left to the generated forwarder — taking its address would double-box to `ж<ж<T>>`), and to a single hop (deeper chains fall through).

**The pointer-interface ADAPTER projects through VALUE embeds the same way — chained.** A `GoImplement<T, Iface>(Pointer = true)` whose interface members are satisfied only by promotion through value embed(s) — dwarf's `type UintType struct { BasicType }`, `type BasicType struct { CommonType }`, `func (c *CommonType) Common()` — cannot forward `m_box.M()` (nothing binds on `ж<UintType>`, CS1929 ×18). The `ImplementGenerator` resolves each unbound interface member by walking the single-value-embed chain (syntax marker: the `public partial ref X X {{ get; }}` property whose name equals its type's simple name — `GetEmbeddedValueHopNames`; bounded to 4 hops), composing the box projection hop by hop via the TypeGenerator's static ref accessors: `m_box.of(UintType.ᏑBasicType).of(BasicType.ᏑCommonType).Common()`. At each level a direct-ж method binds on the projected box and anything else binds through its deref'd `.Value` (ref extensions bind on the ref-returning `Value`) — the same dichotomy as the pointer-embed hop. Mutations write through (the projection field-refs the real embedded storage in the receiver box). (Guarded by `StructPointerPromotionWithInterface`'s `counterKind → kindBase → meta` chain: `st.Stamp()` twice through the interface, `Hits()` reading the count mutated through the same boxes, vs Go.)

**A FOREIGN value embed's direct-ж method binds through METADATA.** When the embedded type lives in another assembly — database/sql's `driverConn` value-embeds `sync.Mutex`, cast `*driverConn → sync.Locker` — its direct-ж method (`Lock`/`Unlock`, emitted by the converter as `this ж<Mutex>` extensions) is visible only in the compiled sync assembly's METADATA, never this compilation's syntax trees. The syntax-based box scan (`GetBoxReceiverMethodNames`) therefore misses it and the chain-walk fell through to the unbindable `m_box.Lock()` (CS1929 ×2). The walk now also resolves the embed field's TYPE SYMBOL and probes its containing package class's static `this ж<T>` members via metadata (`GetForeignBoxReceiverMethodNames`, mirroring the foreignStruct arm's boxBound scan — only a PUBLIC ж-extension binds cross-assembly, since unexported `RecvGenerator` twins are internal); when found it forwards the box hop `m_box.of(driverConn.ᏑMutex).Lock()`, exactly the converter's own call-site form (`Ꮡdc.of(driverConn.ᏑMutex).Lock()`). The `.Lock()` resolves in the generated adapter because `sync_package` sits in the enclosing `go` namespace — the same reason the converter's own call sites bind without a `using static`. (No single-baseline behavioral guard expresses this — it needs a foreign package's ж-method type value-embedded AND implementing that package's interface, the `sync.Mutex`+`sync.Locker` shape — so **GUARD OWED**; verified by a minimal two-assembly reproduction of that exact shape, 2×CS1929 → 0.)

The **exception is the enclosing method's own `[GoRecv] ref` receiver**: a non-direct-ж pointer-receiver method renders `this ref T recv` with **no box** (`Ꮡrecv` exists only for direct-ж), so the box descent referenced a nonexistent name (CS0103 — runtime `mgcscavenge.go`, `(*scavChunkData).alloc/free` calling the promoted `sc.setEmpty()`/`setNonEmpty()` from the embedded `scavChunkFlags`). No box is needed either: the embedded field of a `ref` receiver is *addressable*, so the promoted method's `[GoRecv] ref` overload binds on the **explicit field call** — `sc.scavChunkFlags.setEmpty()` — with faithful write-through. (A *direct-ж* target on the bare receiver would have promoted the enclosing method via the capture-mode fixpoint, so this arm's target always has the `ref` overload.) The receiver name-match is guarded **rendered==raw**: an inner binding that shadows the receiver name is Δ-renamed by the shadow pass, declines the arm, and keeps the descent — the same hardening applied in `convUnaryExpr`'s `&recv.field` branch, where a pointer *local* shadowing the receiver name previously took the receiver arm and emitted `Ꮡ`+raw (a nonexistent box) instead of falling to the pointer-variable arm (`cΔ1.of(chunk.Ꮡflags)`). The fix also pre-cleared the same latent shape in `archive/zip` (`f.FileHeader.hasDataDescriptor()`), `go/internal/gcimporter`, `go/types`, and `image` (whole-stdlib reconvert diff: exactly those sites changed, nothing else). (Guarded by the `EmbeddedValuePointerMethod` behavioral test — value embed + mutating pointer-receiver methods called via a pointer local, a deref'd param, AND the enclosing `[GoRecv] ref` receiver, plus a shadowing-pointer-local control, all with write-through verified against Go; runtime relies on it for `timeTimer`'s `modify`/`stop`/`reset` and `scavChunkData`'s `setEmpty`/`setNonEmpty`.)

**A POINTER embed's BOX-receiver primary promotes through the box hop, not the deref'd value.** The promoted-receiver harvest (`GetExtensionMethods` → `IsExtensionMethodForStruct`) matched only VALUE-receiver forms (`T`/`ref T`/…), so a **direct-ж** primary (`this ж<T>`, emitted when a method takes the address of a receiver field) on an embedded type had no promoted forwarder — sha3's `cshakeState` embeds `*state`, whose `Write` is `this ж<state>`, so `Ꮡc.Write(…)` was CS1929. Such a method IS promotable through a **pointer** embed: the converter renders the hop `target.<embed>` as a `ж<T>`, so the forwarder `target.<embed>.Write(…)` binds the box receiver directly (no box construction). The `TypeGenerator` now collects those box primaries separately (`GetBoxReceiverExtensionMethods`, keyed off `GetEmbeddedPointerHopNames` so it fires ONLY for pointer embeds — a value embed's `target.<embed>` is a value that cannot bind a ж-receiver, which would need the box-hop form the sibling `GoImplement` adapter uses above) and marks each `MethodInfo.IsBoxRecv`, so the emission drops the `.Value` a value-receiver forwarder appends (`target.<embed>.M(…)` for a box primary vs `target.<embed>.Value.M(…)` for a value method). The pointer-receiver forwarder delegates to the value form unchanged, and the shared box means write-through reaches the real embedded storage. (Guarded by the `PointerEmbedBoxReceiver` behavioral test — `Outer` embedding `*Inner` whose `Add` takes `&n.total` (a box primary), the promoted `o.Add(…)` mutating through the shared box, output-compared vs Go. Full behavioral suite green; a whole-corpus confirmation on the real sha3 is deferred to the next census, as with the sibling foreign-embed fix.)

**Same-Go-package promotion survives the `-tests` reference model's assembly seam.** The metadata
fallbacks above implement Go's CROSS-package rule — public members only — which was also their
accessibility filter. But the `-tests` reference model splits ONE Go package across two assemblies
(the test project references the production project instead of recompiling its sources), so a
white-box test struct embedding a production type by pointer — net's `resolvConfTest` over
`*resolverConfig` (`dnsclient_unix_test.go`) — is a *same-package* embed whose type is nonetheless
METADATA in the test compilation: Go promotes its unexported fields (`initOnce`, `dnsConfig`,
`lastChecked`) and methods (`init`, `tryAcquireSema`, `releaseSema`), the field scan's public-only
filter dropped every one, and the method harvest had no metadata path at all — promotion did not
happen and all eight of net's cgo-off Linux test-build errors were promoted selections on that one
type (CS0117/CS1061/CS1929). The membership rule is now Go's own, projected through what the
compiler already knows: a metadata member promotes when it is **accessible to this compilation**
(`IsSymbolAccessibleWithin`, which folds in the `InternalsVisibleTo` friend grant the test model
mints) **and** either **public** (what Go promotes across packages) or a member of the **same Go
package** as the embedding struct — decided by comparing the `[GoPackage]` identities of the two
containing package classes, the identity that survives the assembly split (`net_package` and
`net_internal_test_package` both carry `[GoPackage("net")]`; the external-test class carries
`[GoPackage("net_test")]`, a genuinely different Go package that keeps public-only promotion exactly
as Go does, friend grant notwithstanding). Methods take a new metadata harvest
(`GetMetadataPromotedMethods`): a converted Go method is a static extension on the type's containing
package class, so that class's metadata carries full signatures; receivers split as the syntax side's
do (`this T`/`this ref T` value forms vs the direct-ж box primary), `IsExtensionMethod` keeps
package-level *functions* out, and the harvest is **same-Go-package only** — a genuine cross-package
embed still yields no forwarders and keeps the converter's explicit-hop call emission above, so
nothing changes corpus-wide. One name class is deliberately NOT minted: a promoted method whose name
a package-level **function** also carries (Go scopes them apart — `LookupHost` and
`(*Resolver).LookupHost` — but the emission folds both into one static package class). A forwarder
by that name lives in the *test* class, and C# member lookup finds class methods before `using
static` imports, so it would shadow every bare function call — net's `lookupCustomResolver` embeds
`*Resolver`, and unsuppressed `Lookup*` forwarders cost 54 CS1501s on plain `LookupHost(host)`
calls. The bare function call has no other spelling the converter emits, while a promoted-method
call always has the explicit hop, so the function wins (residual, unmeasured: a Go promoted call of
such a colliding method through the embedding struct would need the converter's explicit hop). No record schema moved: there IS no promotion witness in any
`package_info` file — an embed's promotion has always been resolved at generation time (syntax
same-assembly, metadata otherwise), and the fix completes the metadata half for the one seam where
"same package" and "same assembly" part company. (Guarded by `GenTests/PromotedMetadataEmbedTests` —
the real `TypeGenerator` over the two-assembly friend shape, promoted internal fields and methods
asserted, the collision suppression asserted, plus the cross-package control pinning
public-fields-only and no method forwarders; verified end-to-end by net's linux-target
`net.tests.csproj` building clean — the 8 promotion errors and the 46-site shadowing class both
closed.)

## An embedded struct is an INLINE field, so a value copy copies it

Go gives an embedded field no special storage: it is a field like any other, and a struct value copy
copies it inline. The `TypeGenerator` originally held a promoted embed in a `private readonly ж<T>`
**box** — a heap allocation the constructors made and the `partial ref` accessor resolved through —
which gave the embed *reference* semantics that a plain C# struct assignment then shared:

```go
type inner struct{ v int }
type outer struct { inner; tag string }

a := outer{inner: inner{v: 1}, tag: "a"}
b := a          // Go: b.inner is a COPY
b.v = 2
// Go prints 1 2; the boxed emission printed 2 2 — and `tag`, an ordinary field, printed a b.
```

Every by-value transfer inherited it — assignment, `c := *p`, a value parameter, a returned value, an
element read out of a slice — so the copy and its source shared one embedded storage while the
enclosing struct's own fields copied correctly.

**What it cost.** This is the root of go/types' *type parameter judged not identical to itself* wall
(gcimporter's 108 `TestImportTypeparamTests` mismatches, go/types' own 33 failures, and the
`validType0` stack overflow at `TestFixedbugs/issue48951.go`). `go/types.Var` embeds `object`, which
carries the field's `typ`, and substitution copies a `*Var` to retype it:

```go
func substVar(v *Var, typ Type) *Var {
	copy := *v            // C#: `copy = v` shared the ж<object> box
	copy.typ = typ        // …so this wrote the ORIGIN's typ
	copy.origin = v.Origin()
	return &copy
}
```

So instantiating `S[T]` for the first method of a generic type rewrote the ORIGIN's underlying
`struct{V T₁}` to `struct{V T₂}` in place. The second method then substituted `{T₁ → T₃}` over a
struct that no longer mentioned `T₁`, kept `T₂`, and `Identical(T₂, T₃)` correctly answered false —
`a.V` was judged not assignable to the method's own `T`. `Identical` was never the defect, and
neither were the instance caches (both were instrumented and behave exactly as Go's do).

**The emission.** The embed is an inline field, and the accessor is the same `partial ref` property
it always was, made legal by `[UnscopedRef]` — a struct member returning a ref to its own instance
state is CS8170 by default, because the receiver could be a temporary; the attribute states the
ref's lifetime is the receiver's, which is exactly the guarantee Go gives (the selection *is* the
enclosing value's storage) and moves the burden to the call site, where C#'s ref-safety rules then
reject precisely the cases Go also rejects. It is the same technique the `InheritedTypeTemplate`
already used to forward a defined-type-over-struct's fields.

```csharp
public partial struct Var
{
    private @object ʗobject;                                       // was: private readonly ж<@object> Ꮡʗobject;

    [UnscopedRef] internal partial ref @object @object => ref ʗobject;
    [UnscopedRef] internal ref ΔType typ => ref @object.typ;       // promotion chains the same way

    internal static ref @object Ꮡobject(ref Var instance) => ref instance.@object;   // unchanged
}
```

Everything downstream is unchanged in form: `&v.embed` still goes through the static `Ꮡ`-accessor
(`Ꮡv.of(Var.Ꮡobject)`), which builds a struct-field-reference box rooted at the *enclosing* box, so
pointer identity is still the enclosing allocation's; a POINTER embed's slot still holds a possibly
null `ж<T>` that reads and assigns without dereferencing; promoted methods, adapters and the
interface hops all still descend `<embed>` / `<embed>.Value`. One thing improves for free: a
`default(T)` reached where no constructor runs — a missing-key map read, a freshly `make`d element —
no longer has a null embed box, so the previously documented residual gap narrows to embedded types
that need construction in their own right (a fixed array at some depth).

**The one residue, named.** A fixed-size ARRAY reached only *through* an embed is still shared after
a copy: `array<T>` is a struct over a shared `T[]`, and the converter's clone walk
(`typeNeedsValueClone`) skips embedded fields when deciding whether a struct needs a
`[GoValueClone]` stamp. That is unchanged by this fix — it was shared before and is shared now, by a
different mechanism — and widening the walk is now *sound* (the generated
`copy.<member> = <member>.ΔClone()` lands in the copy's own inline storage rather than corrupting
the source), but it moves converter emission corpus-wide and belongs to a change that owns that
footprint.

Guarded by the **`EmbeddedStructValueCopy`** behavioral test: assignment, by-value parameter, a
two-level `c := *p`, a slice-element read, plus a pointer embed proving both halves of Go's rule —
reassigning the copy's embedded pointer leaves the source's alone, while the pointee stays shared
when it is not reassigned.

## The address of a FIELD of a slice or array element aliases the element

Go's `&s[i].f` is a pointer *into* the backing storage: a write through it changes `s[i]`. The
`&`-machinery builds such an address in two steps — the element's address, then a field reference on
it — and the first step has to be the **element-aliasing** form the index branch already renders for
`&s[i]` itself (`Ꮡ(s, i)` for a slice, `Ꮡarr.at<E>(i)` / `p.at<E>(i)` for an array or a
pointer-to-array). The arm's last-resort fallback instead renders `Ꮡ(<value>)`, a box over a **copy**
of the element, and a field ref rooted there aliases the copy: every write through the pointer is
dropped while every read still looks right, so the container simply never changes.

```go
p_A_Other := &p.Inst[pc].Out        // regexp/onepass.go, onePassCopy
*p_B_Alt = *p_A_Other               // patches the compiled program in place
```

```csharp
var p_A_Other = Ꮡ((~p).Inst, pc).of(onePassInst.ᏑOut);          // aliases the element
// NOT: Ꮡ((~p).Inst[pc]).of(onePassInst.ᏑOut)                   // a box over a COPY — write lost
```

This is the same write-dropping class the slice, array and pointer-to-array index branches each call
out by name (`text/tabwriter`'s empty lines, `compress/flate` emitting literals only at levels 2–9,
`hash/crc32`'s all-zero slicing tables), reached through a **field of the element** rather than
through the element itself. The predicate is `exprIsIndexableElement`: slice, array, or
pointer-to-array only. A map is excluded because Go does not permit `&m[k]` at all, so an index over
one can never legitimately reach the `&`-machinery, and admitting it would mask a front-end error as
a plausible emission; a generic instantiation shares `*ast.IndexExpr`'s shape but types as a
signature or a named type and falls out without a special case. The recursion is ordered *before* the
heap-boxed branch, which already recursed identically for an `IndexExpr` base, so a boxed base
reaches the same emission either way and no existing site moves.

**Why it surfaced when it did.** The PROMOTED case was masked for as long as `go2cs-gen` held an
embed in a shared `ж<T>` box (see *An embedded struct is an INLINE field, so a value copy copies it*
above): the embed's reference semantics meant a copied element still pointed at the origin's embedded
storage, so `Ꮡ(elem).of(T.ᏑPromoted)` reached the real element **by accident**. Making the embed an
inline field was correct and removed that accident, which is what exposed this — `regexp`'s
`onePassCopy` stopped patching, and `TestCompileOnePass` reported `isOnePass=false` for
`^(?:(?:a+)*)$` and `^(?:(?:(?:a*)+))$`. That commit fixed the sibling arm (a promoted
pointer-receiver **call** descending a copy box); this is the address-of-**field** arm of the same
defect. An ordinary, non-embedded field of an element was never masked and was broken all along.

**The base the recursion newly exposed: a pointer RECEIVER over a named array.** `&t[i].field` where
`t` is `*semTable` (`type semTable [4]struct{…}`, runtime's `semtable.rootFor`) now reaches the
index arm's pointer-to-array branch, which renders `t.at<E>(i)` on the assumption that a
pointer-to-array base yields a `ж<[N]E>` box. A Go pointer receiver does not: it renders as
`this ref T recv`, which has no box companion, so `recv.at<E>(i)` names a member the value does not
have (CS1061). It needs none — a named fixed-array type is generated as `IArray<E>` over a shared
backing `E[]`, so the two-arg element-aliasing overload aliases on the wrapper itself. **But that
wrapper's backing is allocated LAZILY, and the two-arg overload takes its target BY VALUE, so on a
still-virgin wrapper the backing materialized on the call site's boxing temp and the receiver's own
storage was never written** — see *The element address of a VIRGIN named array must materialize
through the receiver* below, which is why the emission carries `.Value`:

```csharp
[GoRecv] internal static ж<semaRoot> rootFor(this ref semTable t, nint i) {
    return Ꮡ(t.Value, i).of(semTableᴛ1.Ꮡroot);     // was: Ꮡ(t.Value[i]).of(…) — a COPY
}
```

That is exactly the treatment the receiver's array FIELD already gets in the same arm (see *Element
address of an ARRAY FIELD of the receiver* under Slices and Arrays), for the same reason. A
deref-aliased pointer PARAMETER and a box-valued LOCAL both DO have a box and keep `.at<E>(i)`.

The base has to be the receiver **identifier itself**, not merely rooted at it — the same
object-identity-versus-root-identifier rule the slice and array branches state, inverted.
`getIdentifier` walks a selector chain to its root, so `&p.chunks[l1][l2]` (runtime's
`pageAlloc.chunkOf`) and `&u.inlTree[uf.index]` (`symtabinl`) both report the receiver as their root
while their actual base is a pointer-to-array FIELD — a genuine `ж<[N]E>` rvalue that does have a box
and must keep `.at<E>(i)`. Routing those through the two-arg overload hands it a `ж<array<E>>` where
it wants an `IArray<E>`, which does not bind. Neither shape has a behavioral test, and the corpus is
what caught them: a `-stdlib` reconvert of the affected packages moved both files, and reverting them
is what the identifier restriction does.

Guarded by the **`SliceElementFieldAddress`** behavioral test — the deliberate mirror of
`SliceFieldElementAddress` (that one is `&(slice field)[i]`, this one is `&(slice[i]).field`) —
covering an ordinary field of a slice local and of an array local, a promoted field of a slice field
reached through a pointer, and `onePassCopy`'s own idioms: two pointers into one element swapped and
then written through, and a cross-element `*dst = *src`. The pointer-receiver-over-named-array
sub-case above is guarded by **`NamedArrayAnonElement`**'s Compile and golden phases, and — since
the lazy-backing gap below was closed — behaviorally by **`NamedArrayWrapper`**'s element-address
probe on a virgin wrapper. (`NamedArrayAnonElement`'s own `main` still deliberately never indexes
the array; that note said zero-valuing a named fixed-size array "does not yet materialize its
backing on the value itself", which is exactly the gap the next section closes.)

## The array-backing publish is atomic per box

`ж<T>.at<Telem>(i)` has to reach a go2cs-gen named fixed-size array wrapper's LAZY backing, and
`ж<T>` is deliberately **unconstrained** in `T`, so golib cannot call an interface member on
`ref Value` without boxing a copy. The sequence it used was box the wrapper, touch `Source` so the
backing materializes on that copy, copy the whole wrapper back over the real storage — correct
single-threaded (that copy-back IS `47ddd5a50`'s fix for the same lost write) and **lossy with two
threads**, because it is an unsynchronized read-modify-write of shared state. Two threads reaching a
still-lazy wrapper each allocated their own backing; the second copy-back discarded the first along
with every element already written into it, and the element pointers already handed out kept naming
the orphan. Because the wrapper is several words wide, the half-done copy-back could also be
*observed*, surfacing as a spurious `IndexOutOfRangeException` out of `at`'s bounds check rather
than as a lost write.

`crypto/internal/boring/bcache`'s concurrent section is the measured victim: entries lost in ~28% of
runs, and always in the first ~15 of 102,100 — the fingerprint of a bounded start-up window rather
than of a broken CAS or a GC interaction (both A/B-eliminated).

The publish is now gated per **box**, which is the only durable unit available: the by-value copy
cannot be one, and constraining `T` is not on the table — the constrained-CALL route was torn out in
`d5c0c9c10` for killing every Native AOT binary at type-init. `m_publishedArrayBacking` serves two
jobs at once: `null` is the once-only gate (every thread serializes through `lock (this)`, which is
exactly the cold-start window the race lives in), and a *different* backing is the reassignment
detector, so no stale ready-flag can hand out a pointer into a private copy after `*p` is assigned a
fresh zero wrapper. The fast path is lock-free — one acquire read, one type test, one reference
compare.

The publish path is also narrowed to the shapes that actually *are* lazy, which fixed a second,
separate defect the old unconditional probe carried. golib's own `array<T>`/`slice<T>` and every
named-slice wrapper hold their backing in a field, so there is nothing to publish — and
**`slice<T>.Source` is defined to return a DETACHED COPY**, so the old code allocated and threw away
a full copy of the backing on *every* element take through a `ж<slice<T>>`. Measured (isolated
processes, median of three): slice `.at()` **215.09 → 29.00 ns/op**, array `23.66 → 21.84`, named
wrapper `28.51 → 26.22`. Every shape got faster; the fix removes an allocation from the hot path
rather than adding a lock to it.

> **Doctrine: a lazy-initialization fix is not finished until the publish is atomic.** `47ddd5a50`
> correctly diagnosed "the allocation landed on the copy and the real storage stayed virgin" and
> added the copy-back. The single-threaded repair of a lost-write defect is exactly the shape that
> leaves a concurrency residue behind.

**Not closed by this**, because no golib-side gate can be: the generated `Value => m_value ??= …`
getter is itself a read-modify-write, so two threads first-touching the *same struct instance* by
ref still race (`ref semTable semtable => ref Ꮡsemtable.Value; semtable[i] = x`). Closing that needs
an atomic publish inside the generated getter (go2cs-gen). Measured unchanged at ~95% of trials
(ElemAliasProbe `arm7`) — closed separately, see *The named-array wrapper publishes its lazy backing
atomically* below.

## The element address of a VIRGIN named array must materialize through the receiver

The arm above hands `&t[i]` to golib's by-value `Ꮡ<T>(IArray<T> target, int index)`, which was
reasoned sound because "a named fixed-array type is generated as `IArray<E>` over a shared backing
`E[]`". That is true of golib's own `array<E>` — an eagerly-allocated readonly struct, where a copy
shares the storage — and **false of the go2cs-gen wrapper**, whose backing is allocated on first
touch:

```csharp
private array<E>? m_value;
public  array<E>  Value => m_value ??= new array<E>(N);
```

The overload takes its target by value, so the CALL SITE boxes the wrapper and golib only ever sees
that private copy. Over a still-zero wrapper the `??=` therefore ran on the boxing temp, the
receiver's storage stayed virgin, and **every element pointer named a fresh throwaway array — every
write through it silently lost, single-threaded, no concurrency required.** `runtime`'s `rootFor` is
the only access path to `semtable`, so nothing ever materialized the shared table: each call handed
back a pointer into its own private 251-entry array of zero `semaRoot`s. (Latent only because
`sync`'s Mutex/RWMutex/WaitGroup are hand-owned on `SemaphoreSlim` and never reach
`runtime.semacquire`.)

The emission projects through the wrapper's own `Value` getter first:

```csharp
Ꮡ(t.Value, i)      // was: Ꮡ(t, i)
```

`Value` is a **mutating struct member**, so invoking it on the `ref` receiver (or on a field of one)
runs the `??=` against the REAL storage, and the `array<E>` it returns shares that backing — so the
element box aliases the receiver. Both wrapper flavors carry it: a direct-array RHS
(`type Mont [4]uint64`) exposes `Value : array<E>`, and a named RHS (`type pallocBits pageBits`)
yields the view wrapper whose `Value` is that named type, itself an `IArray<E>` over the same
storage. An UNNAMED `[N]E` base renders as golib `array<E>`, has no `Value` member and needs none,
so the projection is gated on the base being a named type over an array
(`lazyArrayBackingProjection`, `convUnaryExpr.go`) and every other site is unchanged — a seeded
whole-corpus reconvert, diffed emission-against-emission, moves **exactly one file**:
`runtime/sema.cs`.

The `.at<E>(i)` route would also be correct (it publishes through the box — see golib's
`arrayView`/`publishArrayBacking`), but it is unavailable here for the same reason this arm exists
at all: a `[GoRecv] ref` receiver has no `ж<>` box.

Guarded by **`NamedArrayWrapper`**'s `slots`/`slot` probe — a pointer-receiver method returning
`&s[i]` on a virgin wrapper, written through and read back. Verified as a real gate rather than a
green that cannot go red: at the previous emission it reports `stdout mismatch C# vs Go`.

**A different door, measured and NOT closed by this.** `runtime/mpallocbits.cs`'s
`Ꮡ((pageBits)(b))` binds golib's standard-box `Ꮡ<T>(in T)` over a value produced by the generated
by-value conversion operator (`implicit operator pageBits(pallocBits value) => value.view`), which
materializes on the operator's own parameter copy. First-touch writes through it are lost; once
anything else materializes `b`, every copy shares the backing and writes land (measured both ways —
ElemAliasProbe `arm8`). It needs its own increment.

## The named-array wrapper publishes its lazy backing atomically

The third door of the same family, and the one neither of the others can reach: the generated
wrapper's **own** `Value` getter, reached by a plain `ref` with no golib on the path at all —
`internal static ref semTable semtable => ref Ꮡsemtable.Value;` and then `semtable[i] = x`. A per-box
publish gate in `ж<T>.at()` never sees it (there is no `at()` call), and the receiver projection above
never sees it either (there is no `Ꮡ`). What it meets is `m_value ??= new array<E>(N)`, a
read-modify-write of shared mutable state: two threads that first-touch the same zero-valued wrapper
each allocate a backing and the second store **orphans the first**, together with every element
pointer already derived from it. Silent — no fault, no exception — and confined to a start-up window
measured in microseconds. Measured at **872 of 900** concurrent first-touch trials (ElemAliasProbe
`arm7`, 24 threads × 300 trials × 3 batches).

The publish becomes an interlocked CAS. Every racing thread allocates, exactly one wins the slot, and
the losers discard their allocation *before* anything can derive an element address from it — which is
what makes it correct rather than merely narrower:

```csharp
private global::System.Runtime.CompilerServices.StrongBox<array<uint64>>? m_value;   // was: array<uint64>?

public array<uint64> Value
{
    get
    {
        global::System.Runtime.CompilerServices.StrongBox<array<uint64>>? value = m_value;

        if (value is null)
        {
            var created = new global::System.Runtime.CompilerServices.StrongBox<array<uint64>>(new array<uint64>(256));
            value = global::System.Threading.Interlocked.CompareExchange(ref m_value, created, null) ?? created;
        }

        return value.Value;
    }
}
```

**Why the slot had to change shape at all.** An interlocked publish needs ONE machine word. `array<E>`
is a 3-field readonly struct (backing plus the `Alias` window's low/length), so `array<E>?` is 24
bytes — it can neither be CAS'd nor even *read* without tearing while another thread writes it. The
narrower one-word alternative, holding the bare `E[]`, does not preserve the value: a
constructor-supplied array may be an alias **window** (`array<E>.Alias`, Go's `(*[N]E)(s)`) whose
`Source` is wider than the array, and flattening it to its backing would silently widen the named
array and shift its origin. The holder carries the whole `array<E>`, so nothing is lost. It is
`StrongBox<array<E>>` and not plain `object` because an `object` slot makes every warm read an
`unbox.any` — a type-check helper *call* in the hot loop; measured on the element-address path over 64
cold tables, `1.97 → 4.13 ns/op` for `object` against `1.97 → 2.45` for the typed holder.

The residual cost is one dependent load and the probe reports it honestly (`arm9`, both emissions in
one process): the raw `Value` getter gets **faster** (`1.03 → 0.87 ns/op` — the wrapper struct shrank
from 24 bytes to 8, so every Go by-value array copy moved with it), the element path over ONE
long-lived table — what the corpus's named arrays actually are, and where the JIT hoists the
loop-invariant getter — sits between `−1%` and `+12%` run to run, and the pathological shape of 64
separate non-resident tables costs `+17…25%`.

**The consequence that had to be measured, not reasoned.** A Go fixed-size array is COMPARABLE and
legal as a map key. With no overrides a C# struct inherits `ValueType.Equals`/`GetHashCode`, and both
read the single `m_value` field — now a *reference*. So two distinct wrappers over equal content began
comparing unequal and hashing differently, missing each other in a map and in `reflect.DeepEqual`:
precisely the silent wrong answer this door exists to remove, traded for a different one. The `==`
operator hid it completely, because `EqualityExpression` binds the wrapper's own
`Equals(IArray<E>)` at COMPILE time and that was structural all along. The Array kind therefore emits
both overrides, delegating to `array<E>`'s element-wise pair so neither depends on the slot's shape
any more:

```csharp
public override bool Equals(object? obj) => obj is Table other && Value.Equals(other.Value);

public override int GetHashCode() => Value.GetHashCode();
```

One golib companion follows for the same reason: `GoReflect.TryUnwrapWrapperValue` reads `m_value` by
reflection to hand callers the wrapper's underlying value, so it unwraps the holder's extra level (no
converted or golib type is ever an `IStrongBox`).

Guarded by **`NamedArrayWrapper`**'s map-key probe — two separately built equal keys, a re-store
through the second, a third distinct key, and the same for the VIRGIN zero array whose backing neither
side has materialized. Verified as a real gate rather than a green that cannot go red: with the two
overrides suppressed and nothing else changed, it reports `stdout mismatch C# vs Go`.

**Still not closed, by construction:** a materialization that happens on a by-value COPY of the
wrapper publishes to the copy's field, so the `arm8` `Ꮡ((pageBits)(b))` door above is untouched. The
emission-vs-emission blast radius is nil — a generator change alters no committed `.cs`, and the
suite's Transpile and Target phases stay byte-identical across it.

## A promoted field whose name equals the enclosing type is Δ-renamed

Go lets an embedded struct carry a field whose name equals the type doing the embedding —
debug/gosym's `type Func struct{ *Sym }` where `type Sym struct{ Func *Func; … }`, so `Sym.Func`
promotes onto `Func`. The generator's promoted-field accessor would then emit a `Func` member
inside struct `Func`, which C# rejects (CS0542 — a member cannot share its enclosing type's name).
The `TypeGenerator` now Δ-prefixes just that accessor's NAME when its simple name equals the
`NonGenericStructName` (the field ACCESS on the right keeps the original name), matching the
`ΔGoType`/`Δslice` collision-rename precedent:
```csharp
public ref ж<Func> ΔFunc => ref Sym.Value.Func;   // was: `Func => …`, CS0542
```
The promoted field is read on the embedded struct directly (`sym.Func = fn`), never via the outer
value, so no converter reference to the renamed accessor needs coordinating; a package that *did*
read `outerFunc.Func` would surface CS1061 in the gate (none does). Cleared debug/gosym's lone
CS0542. Guarded by `PromotedFieldNameIsType` (a `Node` embedding a `*sym` whose `Node` field
collides — accessed through the explicit embedded path, values vs Go).

## An embedded field is named by GO, not by the C# rendering of its type

Go names an embedded field after the **unqualified type name** — the field of `struct{ *myInt }` is `myInt`, the field of `struct{ io.Writer }` is `Writer`. The converter used to derive that member name from the *rendered C# type*, stripping the package qualifier and any type arguments back to something that looked like the Go name. For every ordinary embed the two strings are identical, which is why the derivation served for years; they part company the moment the converter **renames the type**.

A function-local type is hoisted to package scope under a mangled name (`type myInt int` inside `TestAnonymousFields` becomes `TestAnonymousFields_myIntᴛ1`). Naming the member after *that* left the declaration — and go2cs-gen's generated constructor and promotion accessor, which are both read off it — spelling one thing while every use site kept spelling the Go field name:

```
encode_test.cs(604): CS1061 'TestAnonymousFields_Sᴛ4' does not contain a definition for 'myInt'
decode_test.cs(2532): CS1739 the best overload for 'TestUnmarshalEmbeddedUnexported_S3' does not have a parameter named 'embed1'
```

It also silently flipped the field's **exportedness**, because the derived name begins with the enclosing function's capital: `embed1`, unexported in Go, was emitted `public`. That is not cosmetic — `encoding/json` reads exportedness off the field, and `TestUnmarshalEmbeddedUnexported` asserts precisely that such a field is *not* settable.

The member name is therefore taken from the Go **object** the embed resolves to: a same-package embed resolves to the field itself (`*types.Var`, whose name IS the Go field name by definition), and a selector embed to the embedded type's own `TypeName`. Both are already unqualified and free of type arguments, so this replaces the stripping rather than adding to it; a `*types.PkgName` (an unresolved selector) is deliberately not claimed, since it would name the member after the package.

```go
type (
    myInt int
    MyInt int
    holder struct{ myInt; MyInt }
)
```
```csharp
[GoType("dyn")] partial struct embeddedLocalTypes_holder {
    internal partial ref embeddedLocalTypes_myInt myInt { get; }
    public partial ref embeddedLocalTypes_MyInt MyInt { get; }
}
```

**The generator follows.** `StructTypeTemplate`'s promoted-struct accessor derived its access modifier from the same rendered type name; C# requires both halves of a partial member to agree, so the corrected declaration met a generator that still said the opposite (`CS8799`). The accessor now scopes by the **member** name, which is what every sibling accessor in that template already did. (Guarded by the `LiftedLocalTypes` behavioral test — value and pointer embeds of function-local named and struct types, positional and keyed construction, writes through an embedded field, and promotion through the embedded struct, all output-compared vs Go.)

⚠ Related but distinct, and still open: `%T` of a lifted function-local **non-struct** named type still prints the hoisted identifier, because only lifted STRUCT types carry the `[GoLocalName]` stamp that the reflection bridge reads.

## An embedded field's NAME is the UNQUALIFIED type name (dot-imported embeds)

An embedded struct field's name is, per the Go spec, the *unqualified* type name. A cross-package
embed written as a selector (`struct{ io.Writer }`) already stripped its qualifier for the field
name; a **dot-imported** embed (`import . "io"` then embedded `ReaderFrom`) reaches the emitter as a
bare `*ast.Ident`, yet `getAliasQualifiedTypeName` still renders it package-qualified — and, once the package is a
collision-rename, as `Δio.ReaderFrom`. Gating the qualifier-strip on the *selector* form left that
qualifier in the field name (`internal io_package.ReaderFrom Δio.ReaderFrom;`), whose embedded dot is
a C# syntax error (`CS1003 '(' expected` / `CS1026 ') expected'` — the reported io `io_test`
defect). `visitStructType` now strips to the last segment whenever the resolved embedded-type name
carries a qualifier (covering both the selector and dot-imported-ident forms; a same-package embed
has no dot, so it is a byte-identical no-op), yielding the correct `public io_package.ReaderFrom
ReaderFrom;`. (This is one root among several in the io test suite, which remains blocked by separate
`import . "io"` using-alias resolution issues — the `Δio` namespace is emitted but never aliased.)

## An embedded PREDECLARED type is a plain field stamped `[GoEmbedded]` (2026-09-05)

Go lets a struct embed a predeclared type — `struct{ int }`, `struct{ *int }` — and the embed has
nothing to promote: no methods, no fields, only a name (`int`) and the **Anonymous** bit
`reflect.StructField` reports for it. The converter had always emitted such an embed as a PLAIN
field named after its type, which is the right managed shape (there is no `partial ref` accessor to
generate for a type with no members) and the wrong reflection surface: on the managed side a field
named `int` of type `nint` is indistinguishable from a field somebody *declared* as `int nint`, so
the projection keyed embeddedness on the promotion shape and answered `Anonymous == false` — the
other half of reflect's `TestFieldPkgPath`, red since increment E2 landed the first half.

The datum is stamped where it is lost, exactly as `[GoArrayDims]` stamps a parameter's array length
and `[GoMapKeyDims]` a map field's key dims: the two `handled` branches of `visitStructType` that
render an embedded predeclared type (and an embedded POINTER to one) emit the field with golib's
`[GoEmbedded]` marker, and `GoReflect.GoFields` reads it into `GoFieldInfo.Embedded` for the
plain-field arm. Nothing else moves — every embed of a NAMED type keeps its generated
promotion shape, which already reports Anonymous:

```go
func test() struct { string; *int; P; M } {   // TypeConversionReturnType: P = *bool, M = map[int]int
```
```csharp
[GoType("dyn")] internal partial struct test_R0 {
    [GoEmbedded] internal @string @string;
    [GoEmbedded] internal ж<nint> @int;
    [GoEmbedded] public P P;
    [GoEmbedded] public M M;
}
```

The stamp's predicate is the converter's, not "predeclared": the two branches fire for an embed
written as an identifier whose object's type is not a `*types.Named` — a basic type, or an ALIAS of
a non-defined type (`P`, `M` above; an alias is not `Named` at those branches) — and for a `*T` embed
whose pointee is not `Named`. Go reports every one of them `Anonymous`, so the stamp is right for the
whole set; the census that sized the cut is keyed on exactly that predicate (positive control:
`TypeConversionReturnType` reads 8 sites, four per textual occurrence of its struct).

Blast radius, measured before the cut: **zero** embedded-builtin fields in production std (positive
control: 269 embedded struct fields, not counted), 39 sites in six TEST packages (`reflect_test` 22,
`internal/reflectlite_test` 7, `text/template/parse` 4, `sync/atomic_test` 3, `math/big_test` 2,
`runtime_test` 1) — so the `-stdlib` two-seeded diff is empty on all three targets, the four banked
rows' test sources take their attribute hunks with the cut, and CNR moves exactly one behavioral
golden (`TypeConversionReturnType`, whose structs embed `string` and `*int` — four sites). Guarded by
`ReflectFieldMetadata`'s root-4 rows: `struct{ int }` and `struct{ *int }` report Go's `Name`,
`Anonymous` and `PkgPath`, beside a plain field of a named-int type (must NOT read embedded) and an
embedded named int (Anonymous through the promotion shape already).

## An EMBEDDED field whose derived name equals the enclosing type is Δ-renamed

The sibling of the case above, one layer earlier: an **embedded** field's member name is the
*unqualified type name* (Go spec), so it can equal the enclosing struct's own name outright.
io's `io_test.go` declares exactly that — a `bytes.Buffer` embedded in a struct called `Buffer`:

```go
// A version of bytes.Buffer without ReadFrom and WriteTo
type Buffer struct {
	bytes.Buffer
	ReaderFrom // conflicts with and hides bytes.Buffer's ReaderFrom.
	WriterTo   // conflicts with and hides bytes.Buffer's WriterTo.
}
```

The NAMED-field path in `visitStructType` had renamed such a field since net's `type file struct{
file *os.File }` (`typeCollidingFieldName`, the `Δ`/`ΔΔ` rules described under
[Type-vs-Method Name Collisions](shadowing.md#type-vs-method-name-collisions)), and every ACCESS
site already emitted the renamed form — `fieldCollidesWithType` compares the selector against its
enclosing named type without caring whether the field is embedded, and `structFieldBoxName` runs the
same rename for the box accessor. Only the EMBEDDED-field DECLARATION path was out of step: it
emitted `getCoreSanitizedIdentifier(goTypeName)` raw, so the declaration and its accesses disagreed
and the struct itself was CS0542:

```csharp
[GoType] partial struct Buffer {
    public partial ref bytes_package.Buffer ΔBuffer { get; }   // was: `Buffer { get; }`, CS0542
    public io_package.ReaderFrom ReaderFrom;
    public io_package.WriterTo WriterTo;
}
```

The rename is applied once, before the four embed emission forms (interface embed, the two plain-field
arms, and the `partial ref` promotion), using the same raw compare the named-field path uses (escape
and `Δ` markers stripped on both sides), so the marker doubling for a keyword-family or already-Δ-
renamed enclosing type carries over unchanged. Promotion is unaffected — the `TypeGenerator` derives
the backing box and every promoted forwarder from the DECLARED member name, so `Ꮡ` + the renamed
member is what both the generator and the converter's call sites spell (`rb.of(Buffer.ᏑΔBuffer)`).
The collision is only expressible across packages (one package cannot declare two types with the same
name), so it is absent from the single-package behavioral corpus and [CNR](../Glossary.md#cnr) is
byte-identical apart from the new guard. Cleared io's test-host CS0542. Guarded by
`EmbeddedTypeNameCollision` (a `main.Buffer` embedding `inner.Buffer`, exercising BOTH halves the
rename must keep consistent — the explicit field selector `b.Buffer.Data` incl. a write-through, and
the promoted fields plus value- and pointer-receiver methods reached through it — with a composite
literal keyed by the embedded field and a `new(T)` zero value, values vs Go).

## Promoted pointer methods descend multi-hop value-embed chains
A pointer-receiver method promoted through two or more embedded VALUE structs descends hop by hop: the first hop through the `&`-machinery (box-vs-parameter distinction), then one `.of(<Owner>.<field-box>)` view per additional hop -- the `ж<T>` field views compose onto the method's receiver box (reflect's `sliceType` embeds `abi.SliceType` embeds `abi.Type`, whose `Common()` extension binds `ж<abi.Type>` -- CS1929):
```csharp
Ꮡ(rg).of(rig.ᏑDevice).of(CrossPkgLib.Device.ᏑSensor).Calibrate(3);
```
The own-receiver bare form joins the hop path (`recv.E1.E2.method(...)`); a chain broken by a pointer embed falls through unchanged. Guarded by `CrossPkgUser`.

## A promoted box-receiver method through an UNEXPORTED value embed is called cross-package via a public forwarder
An EXPORTED, pointer-receiver method that takes the address of a receiver field is emitted as a
**direct-ж (box-receiver) primary** `M(this ж<T> …)`. When such a method is *promoted* through an
**unexported** VALUE embed — `testing.T.Errorf`, promoted from the embedded `common` (`type T struct{
common; … }`), or go/types' `TypeName`/`Var`/`Func`, which embed `object` — an IN-PACKAGE caller
renders the descent through the embed's box-field accessor (see *Promoted pointer methods descend
multi-hop value-embed chains* above):
```csharp
Ꮡt.of(testing.T.Ꮡcommon).Errorf("…"u8, …);   // in-package
```
`Ꮡcommon` is the `TypeGenerator`'s `FieldReferences` box accessor for the embed, and — matching the
embed's unexportedness — it is `internal`. So a caller in **another package/assembly** (crypto/internal/
cryptotest, testing/slogtest, x/net/nettest, go/internal/gcimporter) cannot see it: a cross-assembly
reference to an `internal` member reads as **CS0117** ("`testing_package.T` does not contain a definition
for `Ꮡcommon`"), not CS0122. Every path through the unexported embed (`common`, `Ꮡcommon`, its promoted
members) is `internal`, so no converter-only descent can reach it. The fix is two-sided:

- **`go2cs-gen` (`StructTypeTemplate`)** — for a direct, non-generic **VALUE embed**, harvest
  the embed's box-receiver primaries (`GetBoxReceiverExtensionMethods`, previously collected only for
  POINTER embeds) and, for each **exported** one, emit a single box-only shim (`IsValueEmbedBoxRecv`) that
  performs the descent internally, where the `internal` accessor is reachable:
  ```csharp
  public static void Errorf(this ж<T> Ꮡtarget, @string format, params Span<object> argsʗp)
      => Ꮡtarget.of(T.Ꮡcommon).Errorf(format, argsʗp);
  ```
  No `this ref T` overload (a box receiver cannot bind on a value). The shim scope is the shared
  `methodScope` — the STRUCT's exportedness, downgraded for a non-public return type — so it is `public`
  only for an exported method on an EXPORTED struct returning void/public (the genuinely reachable case),
  and `internal` on an UNEXPORTED enclosing struct (context's `afterFuncCtx`, reflect's
  `structTypeUncommon`), whose `ж<T>` receiver is itself internal — a `public` shim there is CS0051. It is
  gated to an exported promoted method (an unexported one is never reachable across packages, so it needs
  no shim; its in-package callers keep the inline descent). The value embed is discriminated by
  `!promotedStructType.Contains("<")` (a plain value embed's type name never carries `<`, whereas the
  pointer-box form `ж<…>` and generic embeds do) — a more robust test than the `@`-keyword-escaped
  `pointerEmbedTypeNames` membership, whose `ж<@file>`-shaped names mismatch and mis-fired the shim onto
  os.File's `*file` POINTER embed (a stray `File.Ꮡfile.Value`, CS0119). The embed's own **exportedness is
  NOT part of that discrimination** — it was, until r56g, and the restriction was never a Go rule; see
  *A value embed promotes its pointer-receiver methods into the outer POINTER method set* below for why
  the narrower gate silently truncated a Go method set rather than merely skipping an unreachable shim.
- **the converter (`convSelectorExpr`)** — when the promoted-method descent is reached through an
  unexported embed of a FOREIGN package (single hop), it drops the inaccessible `.of(…)` view and calls
  the promoted method DIRECTLY on the receiver box, binding the public shim:
  ```csharp
  Ꮡt.Errorf("…"u8, …);   // cross-package (Ꮡt for a deref'd param, tΔ1 for a lambda box param)
  ```
  The box is recovered from the first-hop `&embed`-address the `&`-machinery already computes (the text
  before its last `.of(`), so it is correct for every receiver kind without re-deriving it.

(Guarded by `PromotedEmbedLib`/`PromotedEmbedUser`: `Counter` value-embeds an unexported `common` whose
exported `Add`/`Report` take `&c.sum` (box-receiver); the user package calls them on a `*Counter` local
and through a parameter, plus reads the exported `Label` field for contrast — output-compared vs Go.)

**Plain-return-type addendum — a PLAIN (non-box) promoted method returning a public builtin.** The
box-shim above covers a method emitted as a `ж<T>` primary (it takes `&receiver.field`). A method that
merely READS a field — `testing.common.Name()` (`func (c *common) Name() string { return c.name }`) — is
emitted as an ordinary `Name(this ref common)` extension, so the promotion machinery emits the usual
value + box forwarders `Name(this ref T)` / `Name(this ж<T>)` with body `target.common.Name()`. But their
scope was downgraded by the RETURN type: `@string` (and `error`, `bool`, `nint`, … — every golib builtin)
is a PUBLIC C# type whose Go-lowercase name the name-based `GetScope` heuristic reads as unexported, so
the forwarder was emitted `internal` and thus invisible cross-assembly. Cross-package the converter emits
the same bare `Ꮡt.Name()` (the foreign-unexported-value-embed arm fires for EVERY promoted
pointer-receiver method, not just box ones), which then bound a same-named FOREIGN extension —
`x/net/nettest`'s `timeoutWrapper` reads `t.Name() == "…"`, and the only visible `Name` was
`flag.Name(ref flag.FlagSet)` (flag is imported by testing) → **CS1929**. The fix keeps the forwarder
public when its return type is GENUINELY accessible: `go2cs-gen` captures the return type's ACTUAL C#
accessibility (`MethodInfo.ReturnTypeIsPublic`, computed by `IsEffectivelyPublicType` — the type and
every type argument / tuple element / array-or-pointer element is `public`, treating builtin special
types and use-site-bound type parameters as public) and, for the direct-unexported-value-embed case,
trusts it over the lowercase name (`directEmbedIsUnexportedValue && method.ReturnTypeIsPublic`):
```csharp
public static @string Name(this ж<T> Ꮡtarget) { ref var target = ref Ꮡtarget.Value; return target.common.Name(); }
```
Every OTHER promotion keeps the conservative name heuristic (so no golden/compile churn), and an
UNEXPORTED enclosing struct still yields an internal forwarder (its `ж<T>` receiver is internal — a public
forwarder there is CS0051, and my change only prevents a downgrade below the struct's own scope). This
greens `x/net/nettest` (census 271 → 272/302, zero regressions). (Guarded by `PromotedValueEmbedLib`/
`PromotedValueEmbedUser`: `Widget` value-embeds an unexported `common` whose plain `Name() string` is read
in an expression cross-package, alongside an unrelated `Gadget.Name()` — the foreign same-named extension
— output-compared vs Go; CS1929 without the fix.)

**Pointer-expression-receiver addendum.** The converter arm above recovers the box from the `.of(…)`
strip of the first-hop `&embed` address — which assumes the receiver has an addressable base (an
ident: a raw-box local, a deref'd param's `Ꮡx`). A pointer receiver **expression** — a type-assert or
call chain like go/internal/gcimporter's `pkg.Scope().Lookup(name).(*types.TypeName).Type()` — has no
such base: the `&`-machinery boxes a COPY (`Ꮡ(x.@object)`, no `.of(` anywhere), so the arm silently
fell through to the spelled embed hop, `internal` cross-assembly (**CS1061**). A follow-up sub-arm
recognizes a pointer-typed receiver expression that renders as the raw box (pointer-typed and not
deref-aliased) and calls the promoted member straight on it — the box IS the receiver:
```csharp
pkg.Scope().Lookup(name)._<ж<types.TypeName>>().Type();   // binds the public Type(this ж<TypeName>)
```
(Guarded by `PromotedValueEmbedExprRecv`: the promoted `Name()` called on a `map[string]any`
assert-chain receiver and on a constructor-call receiver, output-compared vs Go; CS1061 without the
fix.)

## A value embed promotes its pointer-receiver methods into the outer POINTER method set
Go's rule has no exportedness clause: for `type S struct{ E; … }` embedding `E` **by value**, the method
set of `*S` contains every **pointer**-receiver method of `E`, because `&s.E` is addressable. (The method
set of a plain `S` does **not** — that half is the narrowing this subsection also has to preserve.)

The shim above emitted exactly that promotion, but only for an **unexported** embed. That gate arrived
with the cross-package-reachability problem it solves (`testing.T.Errorf`, whose `Ꮡcommon` accessor is
`internal`) and reads as a scoping decision, which is why it looked harmless: for an EXPORTED embed the
accessor is public, so the converter's own call sites descend inline and never need a shim.

They are not the only reader. **golib reconstructs a Go method set at RUN TIME by scanning the EMITTED
extension methods** (`TypeExtensions.GetGoMethodSetCandidates`, shared by the `StructurallyImplements`
probe and `AdapterBinder`'s shell binder — see *Every eligible interface carries runtime duck-typing
shells*). So an un-emitted promotion is not a missing convenience, it is an **ABSENT Go method**: the type
stops satisfying interfaces Go says it satisfies, at every site the compile-time recorders cannot reach.

`debug/dwarf` is the reached case. Its `readType` asserts to an **anonymous** interface —

```go
typ.(interface{ Basic() *BasicType }).Basic()
```

— which the converter lifts to a package-local `[GoType("dyn")] partial interface readType_type`. The
concrete types (`*IntType`, `*UintType`, `*CharType`, `*UcharType`, `*FloatType`, …) satisfy it **only**
through `func (b *BasicType) Basic() *BasicType` promoted from their exported `BasicType` value embed, and
the value is held as a *different* named interface (`Type`) at the assertion site — so no compile-time
witness can exist for the pair and the run-time tier is the only thing that can answer. It answered MISS:

```
panic: interface conversion: interface {} is *dwarf.UintType, not dwarf.readType_type
```

The gate is now the Go rule — any direct, non-generic VALUE embed promotes its box-receiver primaries —
and the shim keeps its `ж<S>`-only receiver, which is what preserves the narrowing half (a `Uint` VALUE
must still miss the same anonymous interface):

```csharp
public static ж<BasicType> Basic(this ж<UintType> Ꮡtarget) => Ꮡtarget.of(UintType.ᏑBasicType).Basic();
```

The `of(…)` view is load-bearing rather than incidental: it **aliases** the embedded storage, so dwarf's
caller writing `t.Name`/`t.BitSize` through the returned `*BasicType` reaches the real field. A
copy-returning promotion would have compiled, run, and printed plausible zeros.

**Reachability addendum — the shim was emitted, and emitted unreachable.** Widening the collection gate
made the promotion EXIST; it did not by itself make it bindable. The shim's scope is the shared
`methodScope`, whose return-type downgrade runs the name heuristic — and `GetSimpleName` reduces a type to
its last dotted segment, which for a Go MULTI-RETURN is `error)`. Lowercase. So *every tuple-returning*
promoted method read as unexported and was emitted `internal`. `archive/zip` is the reached case: `Open`,
promoted from `ReadCloser`'s exported `Reader` embed, returns `(io.fs.File, error)`, so the package's own
test assembly could not bind the shim its `ReadCloser`→`fs.FS` adapter needed (**CS1929**, the whole
package build-blocked behind it). The accurate test (`method.ReturnTypeIsPublic`, from
`IsEffectivelyPublicType`, which walks tuple elements) already existed for the plain-return case; it now
also applies to `IsValueEmbedBoxRecv`, which is the *stronger* case for it — that shim exists precisely to
be reachable across assemblies, since it performs a descent the caller cannot spell, so emitting it
`internal` defeats its own purpose. Every other promotion keeps the conservative heuristic.

Only the **collection** gate widened. The return-type relaxation's OTHER arm
(`directEmbedIsUnexportedValue && method.ReturnTypeIsPublic`, in the plain-return addendum above) stays on
the narrow condition, because it answers a different question — a cross-package call the converter emits
as a bare `Ꮡt.M()` — which remains the unexported-embed case alone.

**Third gate, same reasoning — the promoted METHOD's own exportedness (2026-08-29).** The shim above kept
one more `GetScope(…) == "public"` test, this one on the **method name**, on the argument that "an
unexported method is never reachable across packages, so it needs no shim and its in-package callers keep
the inline descent". That is true of the **call sites** and false of the **method set** — the identical
distinction the embed-exportedness paragraph draws, one gate over. `net`'s vectored write is the reached
case, and it is a **silent behavioral divergence rather than any diagnostic**:

```go
type buffersWriter interface{ writeBuffers(*Buffers) (int64, error) }   // UNEXPORTED, package net

func (v *Buffers) WriteTo(w io.Writer) (int64, error) {
	if wv, ok := w.(buffersWriter); ok {      // the ONLY thing that ever asks
		return wv.writeBuffers(v)             // writev fast path
	}
	…                                          // per-chunk fallback
}
```

`*net.TCPConn` satisfies it **only** by promoting the unexported `writeBuffers` from its embedded
unexported `conn`, and `conn`'s methods are direct-ж primaries (`ok` compares the receiver against `nil`),
so the promotion is exactly the box shim this subsection emits. With the shim withheld, the emitted method
set had no `writeBuffers`, `StructurallyImplements` answered **False**, the assert MISSED, and the fallback
ran — the program is correct, just not vectored, which surfaces only as `TestBuffers_WriteTo`'s
`write calls = 0; want 1` (nine verdicts, `writev_test.go:91`). Measured on the built `net` assembly, before
and after:

```
method-set entry : writeBuffers ABSENT from *TCPConn's Go method set   |  net_package.writeBuffers(this ж`1 …) [internal]
structural probe : False                                              |  True
type assert      : False -> <miss>                                    |  True -> ΔbuffersWriter`1
```

The shim for an unexported method is emitted **`internal`**, not at `methodScope`: that keeps an unexported
Go method off the assembly's public surface while leaving it inside the set the run-time probe reads, since
`GetGoMethodSetCandidates` resolves extension methods through `NonPublic` binding flags exactly as it does
the converter's own `internal static M(this ж<T> …)` primaries. Guarded by the `UnexportedIfaceDynamicAssert`
behavioral test, whose `conn` (method declared directly) and `ValueSink` (value method set) rows are the
controls that hold while the promoted `TCPConn` row diverges.

**No `GoImplement` record is involved, and none would have helped.** The pair is same-package and the
interface is unexported, so `recordSamePackageImplements`' exported-interface gate declines it by design —
correctly, since a record is a **cross-assembly** contract and no other assembly can name `buffersWriter`.
The resolution path for such a pair is the run-time tier alone: `TryTypeAssert` unwraps the `io.Writer`
adapter to the `ж<TCPConn>` box, misses the nominal `AdapterRegistry`, and binds `AdapterBinder`'s
generated `ΔbuffersWriter<>` delegate shell — which needs no record and no `AdapterRegistry` change, only a
complete method set. That is why the fix belongs to promotion emission and not to record emission: the
record layer was never the variable.

## Promoted methods through a FOREIGN pointer embed forward via the embedded box
A struct embedding a FOREIGN pointer (`net/http`'s `http2timeTimer struct { *time.Timer }`, `net/http/internal`'s `FlushAfterChunkWriter struct { *bufio.Writer }`, `bufio.ReadWriter { *Reader; *Writer }`) promotes the embed's pointer-receiver methods, but those land as ж-extensions visible to the consuming assembly only through METADATA — direct-ж primaries (`Reset(this ж<Timer> …)`) and the public `RecvGenerator` twins of `[GoRecv]` methods (`Write(this ж<Writer> …)`). The `ImplementGenerator`'s syntax-tree hop scan saw none of them, so the value-form partial deref'd to the value (`this.Timer.Value.Reset(d)` — CS1929, the extension receiver strands) and the FOREIGN-struct pointer adapter fell back to the struct itself (`m_box.Value.Write(p)` — CS1061, `ReadWriter` declares nothing). Both arms now probe the embed's type SYMBOL: the single-hop paths union the foreign element's metadata box methods into the hop's box-method set (`this.Timer.Reset(d)`; `m_box.Value.Writer` in the local pointer arm), and the foreign-struct pointer arm routes each member still on the plain fallback through the UNIQUE pointer embed whose metadata box methods declare it (`m_box.Value.Writer.Write(p)`; Go's depth-one promotion ambiguity rules make the unique-embed requirement faithful). An embed package class outside extension-lookup reach (not the emitting namespace, the shared root `go`, or an enclosing segment) forwards through its package-class static with the box as the receiver argument, mirroring the foreign-extension arm. (Guarded by the `ForeignPtrEmbedIfaceLib`/`ForeignPtrEmbedIfaceUser` pair — a local struct embedding a foreign pointer adapted by value AND by pointer, plus a foreign two-pointer-embed struct adapted by pointer, output-compared vs Go.)

## Embedded-pointer hop receivers split per method
An interface member satisfied by promotion through an embedded POINTER field forwards through
the hop — but the receiver form depends on the target method: a `[GoRecv]` ref extension (or
struct method) binds the deref'd value (`this.File.Value.Name()`), while a **direct-ж primary**
(an extension on `ж<X>` emitted when the receiver escapes — os's `File.Read`/`Write`) binds the
box FIELD itself (`this.File.Read(p)`; deref'ing first strands the receiver, CS1929). The
generator discriminates by scanning the compilation for `this ж<X>` extensions — only
converter-emitted primaries are visible to the single-pass scan (sibling-generator ж-twins are
not), which is exactly the needed split. Applied to both the value-form partial and the pointer
adapter's hop arm. Guarded by `StructPointerPromotionWithInterface` (`Describer` over
`deviceHandle{*Device}`).

## With SEVERAL embedded pointers the hop is chosen per member, not per struct
The hop forwarding above was gated to a struct with exactly ONE embedded pointer, on the
reasoning that multi-embed interface satisfaction was rare. It is not: `net/rpc/jsonrpc`'s
`type pipe struct { *io.PipeReader; *io.PipeWriter }` (`all_test.go:310`) gets `Read` and
`Write` entirely by promotion from two different embeds. With the gate closed, every promoted
member fell through to the templates' bare receiver — `m_box.Read(p)` / `this.Read(p)` — which
binds nothing on the struct, so C# overload resolution reached the nearest same-named extension
anywhere in scope and reported **CS1929 naming a type the package never mentions**
(`io_package.Read(ref io_package.LimitedReader, slice<byte>)` from a jsonrpc test; likewise
`os_package.WriteString(ж<os_package.File>, …)`). That misdirection is the signature of this
defect — it reads as a missing reference and is not one.

The generator now indexes the hop path **per member** (`GetMultiEmbedHopPaths`), routing each
still-unbound interface member to the UNIQUE embed declaring it — which is precisely Go's
depth-1 promotion rule. A name TWO embeds declare is dropped rather than guessed (Go promotes
neither, so only a method the struct declares itself can satisfy the member — `*pipe.Close`
over the `Close` both `*io.PipeReader` and `*io.PipeWriter` declare). Each embed's method set
is read from local **syntax** where its type is declared in this compilation and from
**metadata** where it is not — a referenced assembly exposes both the converter's direct-ж
primaries and the public `RecvGenerator` ж-twins as ordinary symbols, which is the whole
jsonrpc case. The receiver form keeps the per-method split of the section above: a direct-ж
primary binds the embed's ж field itself (`m_box.Value.PipeReader.Read(p)`), anything else its
deref'd value (`m_box.Value.writer.Value.Write(p)`). Both emission paths are covered — the
pointer adapter and the value-form partial, since a pointer embed's method set is in the
STRUCT's method set too, so `var rw ReadWriter = p` (no `&`) records the pair as well. A member
neither resolution places is left unbound and keeps the old fallback, i.e. a loud CS1929 naming
it, never a silent wrong receiver.

Note this is *not* the same machinery as the `TypeGenerator`'s promoted-method forwarders,
which mint `M(this ж<Outer> …)` on the struct itself: those bail out on an embed with no local
declaration (`GetStructDeclaration` returns null for a metadata-only type), which is why a
struct with two LOCAL pointer embeds compiled all along and jsonrpc's two FOREIGN ones did not.
Guarded by `MultiPointerEmbedPromotion` (local embeds in both receiver forms, foreign embeds
`*strings.Reader`/`*strings.Builder` resolved from metadata, an overridden `Close` both embeds
declare, and pointer- and value-sourced casts of each, with write-through observed via the
original embedded objects, vs Go).

## Promoted methods through embedded INTERFACE fields route per-member to the declaring field
A struct whose embeds are INTERFACE fields (httputil's `dumpConn struct { io.Writer; io.Reader }` adapted to `net.Conn`) satisfies interface members through the fields' method sets. The pointer adapter's embedded-interface-field arm was gated to a SINGLE field, so a multi-field struct got no forwarding at all (`m_box.Read(…)` — CS1061). The arm now resolves per member: each still-unbound interface member forwards through the UNIQUE embedded field whose interface declares it (`m_box.Value.Reader.Read(p)` / `m_box.Value.Writer.Write(p)`); a member declared by several fields is left unbound (Go's promotion ambiguity rules reject it unless the struct overrides, and a struct override is already resolved earlier). The single-field behavior is unchanged (zip's `nopCloser`, slogtest's Δ-renamed `Handler` field). (Guarded by the `IfaceFieldEmbedAdapter` behavioral test — a two-interface-field struct adapted by pointer to a third interface needing members from both fields plus one declared on the struct, output-compared vs Go.)

## A nil embedded pointer is holdable and assignable — only its dereference panics

Go permits an embedded pointer to *be* nil: constructing `&Setting{name: name}` with the embedded
`*setting` unset, comparing it (`s.setting == nil`), and assigning it after construction
(`s.setting = lookup(…)` — internal/godebug's `New` → `once.Do` population shape) are all legal;
only *dereferencing through* the nil embed panics. The generator's promoted-pointer machinery
(a `ж<ж<T>>` box behind a ref accessor) previously conflated the two: the accessor resolved the box
with `.Value`, which treats a null held value as a nil-pointer dereference — so the *first touch* of
an unpopulated embed (even the legal `== nil` comparison, or the assignment that would populate it)
panicked from the accessor. Two generator changes separate holding from dereferencing
(`StructTypeTemplate`):

1. **The promoted-struct accessor resolves through `ValueSlot`** — golib's nil-check-free real slot.
   The `Ꮡʗ` box is a real constructor allocation, so resolving the accessor is a read of the *held*
   value, never a dereference *of* the box; reads and writes of the held `ж<T>` must not panic.
2. **The parameterized constructors box a nil pointer for an omitted POINTER embed** (`arg ?? new
   ж<T>(nil)`), matching what the NilType/parameterless constructors already emitted. The held value
   is then a *nil box* rather than a raw null, so a genuine deref of the nil embed panics with Go's
   `runtime error: invalid memory address or nil pointer dereference` (recoverable) instead of
   surfacing an unrecoverable `NullReferenceException`, and a nil embed compares equal regardless of
   which constructor produced it (`ж.Equals`: nil == nil). Value embeds are untouched (their member
   type is a value type — never null).

```csharp
// Promoted Struct Accessors
internal partial ref ж<setting> setting => ref Ꮡʗsetting.ValueSlot;   // was .Value — panicked on first touch

// internal ctor: an omitted pointer embed holds a nil BOX, not a raw null
internal Setting(@string tag = default!, ж<setting> setting = default!)
{
    this.tag = tag;
    Ꮡʗsetting = new ж<ж<setting>>(setting ?? new ж<setting>(nil));
}
```

Go's nil-deref panic is preserved *downstream*, where the actual dereference happens: the promoted
field/method accessors descend `setting.Value.name` / `target.setting.Value.bump()`, and that inner
`.Value` — on the held nil `ж<T>` itself — still routes through the strict panic path. (Guarded by
the `EmbeddedPointerNilAssign` behavioral test — compare-nil on a fresh instance, a recovered deref
panic through the nil embed, assignment of a nil pointer variable, post-construction population, and
aliasing through the populated embed, vs Go; each half discriminates independently — without (1) the
nil-variable assignment leg panics, without (2) the recover leg crashes with an unrecoverable NRE.)

## A field promoted through an embedded POINTER is rooted at the POINTED-TO allocation

A Go pointer's identity is the storage it names, and `f.pfd` for `type File struct{ *file }` is by
definition `f.file.pfd` — one address, whichever spelling reaches it. go2cs encodes a field reference
as **(containing allocation, field token)**, so that encoding is right only while the accessor stays
inside the allocation it was handed. A promotion that crosses a pointer does not: the generated
accessor deref'd on the right,

```csharp
// was — reaches the right storage, but describes the WRONG allocation
internal static ref FD Ꮡpfd(ref File instance) => ref instance.@file.Value.pfd;
```

so `of()` rooted the resulting pointer at the outer `ж<File>` box. Reads and writes still landed in
the real `file.pfd` (the `ref` is correct), which is why nothing looked wrong; only the *identity*
was, and it was wrong in the one way that cannot be seen locally — `&f.pfd` taken through `*File` and
`&file.pfd` taken through `*file` stopped being the same pointer.

The accessor now takes the hop **before** the reference is built, handing the inner type's own
accessor to the inner box:

```csharp
// now — the pointer is rooted where Go roots it
internal static ж<FD> Ꮡpfd(ref File instance) => instance.@file.of(global::go.os_package.file.Ꮡpfd);
```

Call sites are untouched (`Ꮡf.of(File.Ꮡpfd)` still): golib gains a `FieldPtrFunc<T, TElem>` delegate
plus matching `of`/`at` overloads, and C# picks the overload by the accessor's return type. The form
composes for a multi-level embed (`fileWithoutReadFrom` → `*File` → `*file`) because the inner
accessor may itself be this shape. **Value embeds keep the plain `ref` form** — their promoted fields
live in the enclosing allocation, so the existing rooting is already right — and so does a
**cross-package** embed, whose declaration syntax is unavailable to the generator: there the member
list comes from metadata and can surface public fields the inner declaration never had (the reflect
bridge's hand-added `abi.Type.sysType`/`arrayDims`, promoted into `runtime.rtype`), for which no inner
accessor exists to name. That fallback is fail-loud, not silent — naming a missing accessor is CS0117
at the corpus build.

Two identity fixes in `golib` sit underneath it, both in `ж<T>`. A field reference's SOURCE is now
compared by **pointer identity**, not object reference, because an `of()` chain mints a fresh
intermediate box on every access: `Ꮡo.of(Outer.Ꮡin).of(Inner.Ꮡv)` allocates a new `ж<Inner>` each time
it is evaluated, so `&o.in.v == &o.in.v` was **false at depth two** while correctly true at depth one.
`Equals`, `GetHashCode` and `PointerOrderToken` resolve the source through the chain now, the way
`ReferentObject` already did for lifetime questions.

Both defects surfaced as one symptom, and it is worth recording because nothing about that symptom
points at pointer identity: `internal/poll`'s `FD.Close` hung forever on a file whose `Read` was still
in flight. `Close` parks on `runtime_Semacquire(&fd.csema)` until the reader's `readUnlock` →
`destroy` → `runtime_Semrelease(&fd.csema)` wakes it, and those semaphores are keyed by pointer
identity. The two spellings of `&fd.csema` — `os.close`'s, reached through `ж<file>`, and `os.read`'s,
reached through `ж<File>` — landed in **different buckets**, so the release never reached the acquire.
Everything else on the path was already faithful: `syscall.CancelIoEx` really did abort the blocking
`ReadFile`, and the read really did return Go's `file already closed`. Guarded by
`PipeCloseUnblocksRead` (a goroutine blocked on a pipe read, a closer, output-compared against
`go run`) and `EmbeddedPointerFieldIdentity` (depth-2 chain equality, `map[*T]V` keying, and both
spellings of a field promoted through an embedded pointer).

## Generic embedded fields
A GENERIC embed (`entry[K,V]` embedding `node[K,V]`, internal/concurrent) arrives in the AST as
an `IndexExpr`/`IndexListExpr` over the base type; the anonymous-field walk unwraps it (plain,
pointer, and selector forms) and the member emits under the **base name** with type arguments
stripped **before** the selector dot-strip — the arguments may contain qualified types whose
dots otherwise win the LastIndex (`*concurrent.HashTrieMap[T, weak.Pointer[T]]` misnamed its
member `Pointer` instead of `HashTrieMap`). The TypeGenerator's promoted accessors carry the
type parameters on the instance param (`ref Δentry<K, V> instance`) and strip them from the
member access (`instance.node.isEntry`). A promoted method call through a raw ж **box local**
hops `X.Value` ahead of the cross-package pointer-embed hop
(`m.Value.HashTrieMap.Value.Load(value)`, unique). BANKED: unqualified promoted METHOD calls
through a generic embed (`w.show()`) — receiver wrappers resolve the embedded type by exact
name; qualified calls work. Guarded by `GenericStructFields` (`wrapped[T]`/`tag[T]`) and
`CrossPkgUser` (`holder[T]` embedding `*CrossPkgLib.Cache[T]`).

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

---

[← Struct Types](struct-types.md) · [Index](README.md) · [Interfaces →](interfaces.md)
<!-- {% endraw %} -->
