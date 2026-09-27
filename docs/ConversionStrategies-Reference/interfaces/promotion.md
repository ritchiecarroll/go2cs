# Interfaces: Promotion Through Embeds

[Reference index](../README.md) · [Interfaces](../interfaces.md) · [Summary of this topic](../../ConversionStrategies.md#interfaces)

This page covers how an adapter forwards an interface member that a struct promotes through an embedded interface field or a value embed.

## Embedded interface fields

### An embedded INTERFACE FIELD forwards the members it declares

**An embedded INTERFACE FIELD forwards the members it declares in the pointer adapter.** zip's `type nopCloser struct { io.Writer }` satisfies `io.WriteCloser` with `Write` living on the embedded interface VALUE (Go promotes the field's method set) and `Close` on the struct. The `IжAdapter` forwards still-unbound members that the field's interface declares through the field itself — `m_box.Value.Writer.Write(…)` (CS1929 with no forward). Detection is semantic (a non-static field whose name equals its interface type's simple name — the converter emits embeds as real fields), gated to a SINGLE embedded interface field, and filtered to members the field's interface (including its inherited interfaces) actually declares. (Guarded by `InterfaceCasting`'s `wrapSink{Animal}` cast by pointer to the wider `speakShutter` — both the promoted and the own member called through the interface, runtime-verified vs Go.)

**Promoted forwarders through a Δ-renamed embedded interface use the markerless FIELD name.** The converter names an embedded field after the **Go embed name**, so a struct value-embedding an interface whose C# TYPE was collision-renamed (see [Type-vs-Method Name Collisions](../shadowing.md#type-vs-method-name-collisions)) declares `public log.slog_package.ΔHandler Handler;` — the marker lives on the type only. testing/slogtest's `type wrapper struct { slog.Handler; mod func(*slog.Record) }` (slog has both a `Handler` type and a `Logger.Handler()` method, so the type is `ΔHandler`) broke in BOTH generated wrapper forms because the `ImplementGenerator` derived the promoted-forwarder field name from the interface TYPE's simple name: the value partial struct emitted bare `ΔHandler.Enabled(…)` (CS0103 cross-package, CS0120 same-package where the bare name binds the nested interface type), and the pointer adapter emitted `m_box.Value.ΔHandler.Enabled(…)` (CS1061). The field name is now the `Δ`-stripped simple name (`GetSimpleName(…, dropCollisionPrefix: true)`, the same derivation `StructTypeTemplate` already used for embedded-field accessors) in all three places: the value template's promoted arm, the pointer arm's promoted fallback, and the pointer arm's semantic embedded-interface-field detection (which compares field name to type name and otherwise never matches `Handler` vs `ΔHandler`):

```csharp
// value partial struct — promoted members forward through the field:
public bool Enabled(nint level) => Handler.Enabled(level);
// pointer adapter — through the box value's field:
bool global::go.main_package.ΔHandler.Enabled(nint level) => m_box.Value.Handler.Enabled(level);
```

An overridden member is untouched (it forwards to the struct's own method, `this.Handle(…)` / `m_box.Value.Handle(…)`), and a non-renamed interface's simple name has no marker to strip, so every other promotion emits byte-identical code. (Guarded by `ShadowedInterfaceEmbed` — a `Handler` interface Δ-renamed by a `Logger.Handler()` method collision, value-embedded in a `wrapper` struct that overrides one of its three methods, cast to the interface by BOTH value and pointer, promoted and overridden members runtime-verified vs Go; cleared testing/slogtest's 6 errors.)

**A FOREIGN struct's adapter forwards a PROMOTED interface method through the box value, not a phantom static.** When the pointer adapter is generated in an assembly OTHER than the struct's — the struct's package class lives in a different namespace segment, so its extension methods are invisible to extension-method lookup — the `ImplementGenerator` forwards each interface member through a package-class STATIC call (`xcoff_package.ReadAt(m_box, …)`). But this only works for a method the struct **declares directly** (whose `RecvGenerator` ж/ref static exists). A **promoted** interface method — debug/buildinfo's `*xcoff.Section` → `io.ReaderAt`, where `Section` embeds the `io.ReaderAt` interface so `ReadAt` is promoted, not declared — has no such static, so the forward targets a nonexistent overload (CS1501, "no overload takes 3 arguments"). The static forward is now gated on a real box/ref-bound static existing; when absent, the adapter forwards through the box VALUE — `m_box.Value.ReadAt(…)` — invoking the struct's own PUBLIC promoted method (the same promotion `File.ReadAt` etc. rely on). A directly-declared method keeps the static forward unchanged (no churn). (Validated by the `core/debug/buildinfo` build — its `*os.File → io.ReaderAt` [direct `ReadAt`, static] and `*xcoff.Section → io.ReaderAt` [promoted, box-value] adapters — plus the full behavioral suite + tar/math-big/net corpus; a single-assembly behavioral guard cannot host it — the shape needs a struct embedding a THIRD package's interface, cast cross-assembly, so **GUARD OWED**.)

### A named field whose name equals its interface type is NOT an embedded interface
`ImplementGenerator` forwards an interface member it cannot bind directly through an **embedded interface
field** (zip's `type nopCloser struct{ io.Writer }` → `m_box.Value.Writer.Write(…)`), and detects one by
NAME: the field's name equals its interface type's simple name, modulo the `Δ` collision marker (the
converter names the field after the Go embed, so a Δ-renamed interface TYPE keeps a markerless FIELD —
slogtest's `wrapper` embeds `slog.Handler` as `Handler`).

That test cannot, by itself, be right. Go emits an **ordinary named field** `Type Type` and an **embedded**
`Type` to the same C# field declaration, and only the first promotes nothing. `debug/dwarf` carries both
shapes in ONE struct:

```go
type PtrType struct {
	CommonType        // a real embed — promotes Common()
	Type       Type   // an ordinary field — promotes nothing
}
```

`Common()` was therefore forwarded through the FIELD, returning the **referenced** type's `CommonType`
instead of the receiver's own — a silent wrong answer whenever `Type` was non-nil, and a null dereference
when it was not. Five dwarf structs carry that field (`QualType`, `ArrayType`, `PtrType`, `StructField`,
`TypedefType`).

Resolved by **precedence**, not by a new signal — none is available, since the two emissions are identical
by construction. Promotion through a marker-backed **depth-1** value embed (`public partial ref CommonType
CommonType { get; }` — a hard converter marker the name heuristic is not) now resolves BEFORE the
interface-field arm:

```csharp
ж<CommonType> ΔType.Common() => m_box.of(PtrType.ᏑCommonType).Common();   // not m_box.Value.Type.Common()
```

Legal Go guarantees the two can never both be correct at depth 1: promoting one member from two depth-1
embeds is an **ambiguity the Go compiler rejects**, so a struct where both arms answer is a struct whose
"interface embed" is really a plain field. Deeper embed levels stay BELOW the interface arm, matching Go's
shallower-embed-wins rule. Implemented by running the existing `.of(…)` descent in two passes (`maxDepth`
1, then 4) so the "what can bind at this hop" logic is not duplicated and cannot drift from itself.

(Both subsections guarded by `PromotedEmbedAnonIfaceWitness`, which pins the anonymous-interface assert
from a named-interface-held value, the two-concrete-type spread, the type-switch form, the ALIASING write
back through the promoted pointer, the VALUE-must-miss narrowing, and the `Ptr{CommonType; Node Node}`
shape checking the VALUE `Common()` returns rather than merely that nothing crashed. `debug/dwarf`
30 → 40 of 40.)

## Promoted methods in adapters

### A foreign struct's promoted method forwards through its value embed
When the adapter's struct is FOREIGN (defined in another assembly) it binds forwarding from METADATA
(the boxBound / refBound scan above); a member neither on its box nor a ref-static falls to
`m_box.Value.M()`. But an interface member the foreign struct PROMOTES through a VALUE-embedded field
has no extension on the struct's OWN package class, so `m_box.Value.M()` is CS1929 — `text/template`
casting `*parse.RangeNode` to `parse.Node`, where `RangeNode` embeds `BranchNode` and the exported
`String` lives on `BranchNode`, not `RangeNode`. The generator now discovers the foreign struct's
value embeds from metadata (`GetForeignValueEmbeds`: a member whose name equals its type's simple
name) and, for a still-unbound member the embed's package class declares as a public value/ref-receiver
extension (`GetForeignValueReceiverMethods`), forwards through the embed's package-class STATIC:
```csharp
global::go.@string global::go.…parse_package.Stringer.String() =>
    global::go.…parse_package.String(ref m_box.Value.BranchNode);
```
The static form is required because the embed's namespace is not imported in the adapter file (only
`using go;`), so an instance-form `m_box.Value.BranchNode.String()` cannot resolve the extension —
exactly as the foreign struct's own extensions route through `staticClass`. The receiver argument
carries the extension's ref-kind (`ref`/`in`/value). Rerouting is gated to a genuinely promoted member
(the struct binds it neither directly nor via a box/ref hop), so a struct that declares the method
itself is unaffected. This clears `text/template`'s last CS1929. (Guarded by `CrossPkgLib`/`CrossPkgUser`:
`*Branch`, which promotes the exported `Emit` through its `EmitBase` value embed, cast to `Emitter` —
CS1929 without the reroute.)

---

[← Interfaces](../interfaces.md) · [Index](../README.md)
