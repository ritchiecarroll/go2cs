# Interfaces
<!-- {% raw %} — Jekyll/Liquid guard: this page contains Go composite-literal and template syntax ({{ … }}) that Liquid would otherwise parse; the HTML comment hides the tag on GitHub. -->

[Reference index](README.md) · [Summary of this topic](../ConversionStrategies.md#interfaces)
Go interfaces are duck-typed: a type implements an interface simply by having the methods. The converter emits each **user-defined** interface as a partial interface with a `[GoType]` attribute, and the **`ImplementGenerator`** source generator discovers which concrete types satisfy it and emits the implementing glue plus the implicit conversions. As a result, assigning a concrete value to an interface variable is direct — no reflection lookup or `.As(...)` call is needed:

```go
type Stringer interface {
    String() string
}

type point struct{ x, y int }

func (p point) String() string {
    return fmt.Sprintf("(%d, %d)", p.x, p.y)
}

func describe() Stringer {
    return point{1, 2}    // point implements Stringer -> assignable directly
}
```
```csharp
[GoType] partial interface Stringer {
    @string String();
}

[GoType] partial struct point {
    internal nint x, y;
}

[GoRecv] internal static @string String(this ref point p) {
    return fmt.Sprintf("(%d, %d)"u8, p.x, p.y);
}

internal static Stringer describe() {
    return new point(1, 2);   // implicit conversion emitted by ImplementGenerator
}
```

The well-known built-in interfaces (`error`, `fmt.Stringer`, etc.) are hand-written in `golib`/the baseline rather than `[GoType]`-generated, but concrete types implement them the same duck-typed way. (Earlier strategies used a generic `As`/reflection mechanism; that has been superseded by the compile-time source generators.)

Each discovered "concrete type implements interface" pairing is recorded as an assembly-level attribute in the package's `package_info.cs`, e.g. `[assembly: GoImplement<point, Stringer>]`, which `ImplementGenerator` consumes.

Two refinements to the recording pipeline (io's `NopCloser`/`eofReader`, 2026-07-03): **(a) the interface-inheritance prune drops only COMMON implementations, and only from the LOWER interface.** When an interface embeds others (`ReadCloser` = `Reader` + `Closer`), a type recorded on both the derived and an embedded interface needs only the derived pair — C# interface inheritance covers the embedded one. The prune intersects the two sets and removes the overlap from the *embedded* interface's set; it previously intersected **in place on the derived set** (the HashSet mutates its receiver), which *emptied* the derived interface's recordings whenever the overlap was empty — `GoImplement<nopCloser, ReadCloser>` vanished and the `return nopCloser{r}` failed CS0029. **(b) An INDEX-expression assignment target records against its ELEMENT type.** `mr.readers[0] = eofReader{}` assigns to a `[]Reader` element; the interface-detection previously tested the container's root identifier (`mr` — never an interface; Go forbids indexing one), so no pair was recorded and the concrete literal emitted bare. The check now types the whole index expression, and the conversion-recording path keeps the element type rather than redirecting to the container. (Guarded by the `InterfaceCasting` extensions — `rdCloser`, an inheriting interface returned concretely while the embedded `rdr` has its own recording, plus an interface-slice element assignment.)

The prune matches a FOREIGN base by its **canonical name**: the inheritance tracking stores both the alias render the declaration emits (`fs.FileInfo`) and the `getFullyQualifiedTypeName` render (`go.io.fs_package.FileInfo`) — the implementation-map keys are canonical, so the alias form alone never matched a foreign embed and both the derived and base impls emitted the same explicit members (zip's `headerFileInfo : fileInfoDirEntry` + `fs.FileInfo`, CS8646 ×6/CS0111 ×2). Structural bases track their canonical names the same way. (Guarded by `CrossPkgUser`'s `stamped` — a local interface embedding the foreign `CrossPkgLib.Labeled`, with `seal` recorded against both; only the derived record survives.)

**An embedded INTERFACE FIELD forwards the members it declares in the pointer adapter.** zip's `type nopCloser struct { io.Writer }` satisfies `io.WriteCloser` with `Write` living on the embedded interface VALUE (Go promotes the field's method set) and `Close` on the struct. The `IжAdapter` forwards still-unbound members that the field's interface declares through the field itself — `m_box.Value.Writer.Write(…)` (CS1929 with no forward). Detection is semantic (a non-static field whose name equals its interface type's simple name — the converter emits embeds as real fields), gated to a SINGLE embedded interface field, and filtered to members the field's interface (including its inherited interfaces) actually declares. (Guarded by `InterfaceCasting`'s `wrapSink{Animal}` cast by pointer to the wider `speakShutter` — both the promoted and the own member called through the interface, runtime-verified vs Go.)

**Promoted forwarders through a Δ-renamed embedded interface use the markerless FIELD name.** The converter names an embedded field after the **Go embed name**, so a struct value-embedding an interface whose C# TYPE was collision-renamed (see [Type-vs-Method Name Collisions](shadowing.md#type-vs-method-name-collisions)) declares `public log.slog_package.ΔHandler Handler;` — the marker lives on the type only. testing/slogtest's `type wrapper struct { slog.Handler; mod func(*slog.Record) }` (slog has both a `Handler` type and a `Logger.Handler()` method, so the type is `ΔHandler`) broke in BOTH generated wrapper forms because the `ImplementGenerator` derived the promoted-forwarder field name from the interface TYPE's simple name: the value partial struct emitted bare `ΔHandler.Enabled(…)` (CS0103 cross-package, CS0120 same-package where the bare name binds the nested interface type), and the pointer adapter emitted `m_box.Value.ΔHandler.Enabled(…)` (CS1061). The field name is now the `Δ`-stripped simple name (`GetSimpleName(…, dropCollisionPrefix: true)`, the same derivation `StructTypeTemplate` already used for embedded-field accessors) in all three places: the value template's promoted arm, the pointer arm's promoted fallback, and the pointer arm's semantic embedded-interface-field detection (which compares field name to type name and otherwise never matches `Handler` vs `ΔHandler`):

```csharp
// value partial struct — promoted members forward through the field:
public bool Enabled(nint level) => Handler.Enabled(level);
// pointer adapter — through the box value's field:
bool global::go.main_package.ΔHandler.Enabled(nint level) => m_box.Value.Handler.Enabled(level);
```

An overridden member is untouched (it forwards to the struct's own method, `this.Handle(…)` / `m_box.Value.Handle(…)`), and a non-renamed interface's simple name has no marker to strip, so every other promotion emits byte-identical code. (Guarded by `ShadowedInterfaceEmbed` — a `Handler` interface Δ-renamed by a `Logger.Handler()` method collision, value-embedded in a `wrapper` struct that overrides one of its three methods, cast to the interface by BOTH value and pointer, promoted and overridden members runtime-verified vs Go; cleared testing/slogtest's 6 errors.)

**A FOREIGN struct's adapter forwards a PROMOTED interface method through the box value, not a phantom static.** When the pointer adapter is generated in an assembly OTHER than the struct's — the struct's package class lives in a different namespace segment, so its extension methods are invisible to extension-method lookup — the `ImplementGenerator` forwards each interface member through a package-class STATIC call (`xcoff_package.ReadAt(m_box, …)`). But this only works for a method the struct **declares directly** (whose `RecvGenerator` ж/ref static exists). A **promoted** interface method — debug/buildinfo's `*xcoff.Section` → `io.ReaderAt`, where `Section` embeds the `io.ReaderAt` interface so `ReadAt` is promoted, not declared — has no such static, so the forward targets a nonexistent overload (CS1501, "no overload takes 3 arguments"). The static forward is now gated on a real box/ref-bound static existing; when absent, the adapter forwards through the box VALUE — `m_box.Value.ReadAt(…)` — invoking the struct's own PUBLIC promoted method (the same promotion `File.ReadAt` etc. rely on). A directly-declared method keeps the static forward unchanged (no churn). (Validated by the `core/debug/buildinfo` build — its `*os.File → io.ReaderAt` [direct `ReadAt`, static] and `*xcoff.Section → io.ReaderAt` [promoted, box-value] adapters — plus the full behavioral suite + tar/math-big/net corpus; a single-assembly behavioral guard cannot host it — the shape needs a struct embedding a THIRD package's interface, cast cross-assembly, so **GUARD OWED**.)

**Pointer-sourced interface values use a generated ADAPTER, not the value-boxing partial struct (2026-07-03).** A Go interface value created from a pointer — `var s Iface = &t`, `New(new(lockedSource))`, `Rand{src: &runtimeSource{}}` — holds the **pointer**: every call through the interface mutates the original object, `s.(*T)` recovers that same pointer, and interface equality is pointer identity. The old emission deref'd the box into the C# interface (`~box`, boxing a **copy**) — aliasing divergence — and could not serve **direct-ж** receiver methods at all (a method that takes the address of a receiver field is emitted with the box AS its receiver, `this ж<T>`, which a struct's `this` can never bind — math/rand `lockedSource` CS1929). The converter now records such casts as `[assembly: GoImplement<T, Iface>(Pointer = true)]`, and `ImplementGenerator` emits a sealed **adapter class** instead:

```csharp
internal sealed class runtimeSourceжSource : go.math.rand_package.Source, IжAdapter
{
    private readonly ж<runtimeSource> m_box;
    public runtimeSourceжSource(ж<runtimeSource> box) => m_box = box;
    public object? Box => m_box;
    long go.math.rand_package.Source.Int63() => m_box.Int63();  // direct-ж / ж-twin binds the box
    // Equals/GetHashCode delegate to box identity (Go pointer-interface equality)
}
```

Cast sites emit the adapter around the box (`Incrementer inc = new CounterжIncrementer(c);`, `src: new runtimeSourceжSource(Ꮡ(new runtimeSource()))`), covering call arguments, keyed composite-literal fields, and `var` declarations; a pointer-typed operand in these positions renders as the box (isPointer ident context), not the deref'd receiver ref-local. Member forwarding picks the receiver form per method: direct-ж and `[GoRecv]` ref-extensions (whose `RecvGenerator` ж-twin exists) forward to `m_box.M(...)`; plain value-receiver methods forward to `m_box.Value.M(...)` (Go copies the value at the call). The golib type-assert machinery (`_<T>()`) unwraps `IжAdapter.Box` so `s.(*T)` yields the original `ж<T>`, and `AreEqual` unwraps both operands so interface-vs-interface and interface-vs-pointer comparisons are box identity (`ж<T>.Equals` is already identity-based); `iface == ptr`/`iface != ptr` comparisons emit `AreEqual(...)` with the pointer operand kept as the box (the old `iface == ~p` deref form compared a copy). Because each adapter is a distinct class, the interface-inheritance de-duplication (dropping `GoImplement<T, Source>` when `GoImplement<T, Source64>` exists and `Source64` embeds `Source`) exempts pointer-form pairs — a `Source`-targeted cast site references `runtimeSourceжSource` even though `runtimeSourceжSource64` also implements `Source`. VALUE-sourced casts (`var s Iface = t`) keep the partial-struct implementation — Go copies the value into the interface there, which is exactly C#'s struct-boxing semantic. Known limits (documented, not yet needed by the corpus): a cross-package pointer cast keeps the old deref-copy form (the adapter class only exists in the impl type's assembly — `isLocalImplType` gate), and asserting an adapter-held interface to a *different* interface (`s.(Source64)` on a `Source`-created value) is not yet unwrapped. (Guarded by the `InterfaceCasting` extension — pointer-receiver `Counter` with a direct-ж member cast to an interface, mutations verified through BOTH the interface and the original pointer, assert-back recovering the same box, and `back == c` pointer equality, run-verified vs Go; and by `InterfaceImplementation`'s output comparison — `zoo[0] == f` interface-vs-pointer identity.)


**Non-empty interface-to-interface conversions use a forwarding adapter.** A Go interface value may be assigned or passed to another non-empty interface when the source interface method set satisfies the target (`var local localLabel = foreign`, where `foreign` is `CrossPkgLib.Labeled`). C# has no structural conversion between unrelated interfaces, so the converter records the interface pair as `[assembly: GoImplement<CrossPkgLib_package.Labeled, localLabel>]` and emits the cast site as a generated adapter:

```csharp
CrossPkgLib.Labeled foreign = new CrossPkgLib.Sensor(Name: "adapter"u8, Temp: 21);
localLabel local = new CrossPkgLib_LabeledᴠlocalLabel(foreign);
fmt.Println(labelOf(new CrossPkgLib_LabeledᴠlocalLabel(foreign)));
```

`ImplementGenerator` emits a sealed adapter implementing the target interface and `IInterfaceAdapter`, stores the source interface value, and forwards each target member to that value. The golib assertion/equality helpers unwrap `IInterfaceAdapter.Value` before type assertions, `Implements<TInterface>`, and `AreEqual`, so the wrapper behaves as an interface view over the original Go interface value rather than a new concrete payload. Guarded by `InterfaceToInterfaceAdapter`, which imports `CrossPkgLib.Labeled`, assigns it to a local compatible interface, passes it as a parameter, and output-compares the forwarded calls.

Two rules govern how concrete implementation records are emitted:

* **Only impl types declared in the *current* package are recorded.** `ImplementGenerator` realizes the attribute by emitting a `partial struct <Impl> : <Interface>` into the **current package's** namespace and class — so it can only add an interface to a type defined in the *same assembly*. A pairing whose impl type is *imported* from another package (e.g. `image/color/palette` building `[]color.Color{ color.RGBA{…} }`) is therefore **not** re-emitted in the consumer: that relationship is already established in the impl type's own package (`image/color` records `[assembly: GoImplement<ΔRGBA, Color>]`). Re-emitting it in a consumer would generate a broken cross-assembly partial (a fresh empty `palette_package.ΔRGBA` rather than the real `color_package.ΔRGBA`), so the converter skips any pairing whose impl type is not local.
* **Multi-segment interface references are root-qualified.** The `GoImplement` attributes are emitted before the file's `namespace` with only `using go;` in scope; that directive imports the *types* of namespace `go` (so a top-level `io_package.Writer` resolves unqualified) but **not** its nested namespaces. A multi-segment package class such as `container.heap_package.Interface` is therefore root-qualified to `go.container.heap_package.Interface` so it resolves; single-segment refs (`io_package`, `sort_package`) are left unchanged.

## Every eligible interface carries runtime duck-typing shells — the sole resolver of a structural assert

The `GoImplement` STRUCTURAL recorders were a compile-time *approximation* of Go's structural satisfaction, incomplete **by construction** — which is why, once these shells existed, they were retired outright (2026-07-25; see the RETIRED note under *Multi-Result Values and Comma-Ok Forms*). A dynamic type may live in a package converted **after** the interface's own, and then no record can exist: a dynamic type may live in a package converted **after** the interface's own, and then no record can exist. `io/fs` is converted before `os`, so `fs/package_info.cs` records only `subFS→ReadDirFS` — `os.dirFS` is unreachable — and every `fsys.(ReadDirFS)` against an `os.DirFS(…)` value silently missed. golib's structural probe answered the *question* correctly (`StructurallyImplements`) but had nothing to *construct*, so the assertion still failed. That is what kept io/fs at 16/18 (`Glob` returning nothing, `WalkDir` seeing only the root).

With no dynamic code generation available (Native AOT), a **per-interface compile-time artifact is the irreducible minimum**, and it must live in the **interface's own package class** — the only placement guaranteed loaded at every asserting site, and the only one that yields a single cross-assembly identity. `TypeGenerator` therefore emits, for every non-generic, non-constraint, non-empty `[GoType]` interface, **two sibling shells**, discovered through a new `[GoInterfaceShell]` stamp on the interface itself. No static member is added to the interface — that shape would be inherited by every embedding interface, which is both a large CS0108 hiding class and a method-set corruption (no Go type can implement a static helper), and it makes the shell NAMES non-contractual so the generator may disambiguate freely:

```csharp
[global::go.GoInterfaceShell(typeof(ΔSpeaker<>), typeof(ΔSpeakerᴛObj), "Speak")]
public partial interface Speaker
{
}
```

**Tier 1 — `ΔI<ᴛTTarget>`, delegate-bound, for a REFERENCE-typed dynamic value** (every `ж<X>` receiver box, i.e. every pointer-sourced Go interface value — the dominant case). Each interface method is a *pre-bound delegate*, so a forwarded call costs a delegate invocation, not a reflective one; that matters because a wrapper is obtained once and then called possibly millions of times (a duck-typed `io.Reader` inside `io.Copy`, a wrapped `sort.Interface`'s `Less`/`Swap`). It closes over the **element** type — the pointee — because that is what makes *both* receiver forms bindable, and the dispatch encodes Go's `*T` method-set rule directly:

```csharp
internal sealed class ΔSpeaker<ΔTTarget> : Speaker, IInterfaceAdapter
{
    private delegate global::go.@string SpeakByPtr(ж<ΔTTarget> targetʗ);
    private delegate global::go.@string SpeakByVal(ΔTTarget targetʗ);

    private static readonly SpeakByPtr? s_SpeakByPtr;
    private static readonly SpeakByVal? s_SpeakByVal;

    public global::go.@string Speak()
    {
        if (m_targetᴛ_is_ptr && s_SpeakByPtr is not null)
            return s_SpeakByPtr(m_targetᴛ_ptr!);
        else
            return s_SpeakByVal!(m_targetᴛ_is_ptr ? m_targetᴛ_ptr!.Value : m_targetᴛ);
    }

    static ΔSpeaker()
    {
        global::go.AdapterBinder.ResolveReceiverMethods(typeof(ΔTTarget), "Speak", out byPtrᴛ, out byValᴛ);
        s_SpeakByPtr = byPtrᴛ is null ? null : byPtrᴛ.CreateStaticDelegate(typeof(SpeakByPtr)) as SpeakByPtr;
        s_SpeakByVal = byValᴛ is null ? null : byValᴛ.CreateStaticDelegate(typeof(SpeakByVal)) as SpeakByVal;
        …
        ᴛBoundByPtr = boundByPtrᴛ;      // read by the binder BEFORE the shell is handed out
        ᴛBoundByVal = boundByValᴛ;
    }
}
```

**Tier 2 — `ΔIᴛObj`, reflective, for a VALUE-typed dynamic value** (the forcing case: `os.dirFS` is `[GoType("@string")] partial struct dirFS`, a value type held in an `fs.FS`). It holds the value as `object` and forwards through `MethodInvoker`s resolved once per (dynamic type, interface) pair, so it needs **no generic instantiation at all**. That is not a stylistic choice: under Native AOT `ilc` roots exactly the instantiations visible in source, and a `MakeGenericType` driven by a run-time `GetType()` over a value type is never one of them, so this is the tier that is unconditionally available. It is emitted only when every member survives the `object` round-trip (a Go variadic tail lowers to `params Span<T>`, a ref-struct that cannot be boxed):

**The forwarder dispatches on ARITY (2026-07-26).** `GoShellBinding.Invoke` used to build a fresh `object?[args.Length + 1]` on every forwarded call, purely to prepend the receiver — 32 B allocated and zeroed per call even for a Go method with no parameters at all, which is the common case (`Len`, `Error`, `String`, `Less`). The bound members are static extension methods, so `MethodInvoker`'s `obj` is always `null` and the receiver occupies the first *argument* slot; the BCL's fixed-arity overloads take up to four arguments, so Go arities **0–3** now forward with no array and arity 4+ keeps the `Span` path. Measured on `PerfIfaceShell` (one object-tier call per iteration, provisional): JIT **633.7 → 588.0 ms**, Native AOT **760.1 → 727.8 ms** — the AOT column matters more in principle, because the binder's belt degrades *both* shell tiers to this one there, so the cost is paid twice per iteration rather than once. The boxed *return* is not fixable this way: `MethodInvoker` returns `object?` and the shell unboxes, and removing that needs a non-reflective forwarder, which needs a generic instantiation — exactly what this tier exists to avoid. (Guarded by the `ShellForwardArity` behavioral test: one anonymous interface spanning arities 0–5 plus an int-returning and a mixed-parameter shape, every method folding its arguments into the printed result so a dropped, duplicated or reordered argument diverges from `go run` instead of passing silently. An instrumented run confirms all four fixed arms *and* the `Span` fallback are reached.)

```csharp
internal sealed class ΔSpeakerᴛObj : Speaker, IInterfaceAdapter
{
    object? IInterfaceAdapter.Value => m_targetᴛ;

    public global::go.@string Speak() => (global::go.@string)m_bindingᴛ.Invoke(0, m_targetᴛ)!;
}
```

The tier choice branches on `Type.IsValueType`, and **either tier belts to the other** when construction fails — AOT rooting is source-shape sensitive, so "we never reach that instantiation" cannot be asserted, only constructed. (Honest limit: because tier 1 closes over the *pointee*, and a pointee is usually a struct, the pointer tier is AOT-**graceful** rather than AOT-guaranteed — an unavailable instantiation degrades to the object shell, not to a miss.)

**One binder owns the method-set discipline.** golib's `AdapterBinder` resolves every binding through `TypeExtensions.GetGoMethodSetCandidates(element, isPointer)` — the *same* receiver rule `StructurallyImplements` applies, factored out for exactly this reason. So a value-sourced shell binds only value-receiver methods and can never widen a Go method set: `PtrOnly{}` (whose `Speak` has a pointer receiver) MISSES, `&PtrOnly{}` matches, and a pointer source calling a value-receiver method dereferences the box **per call**, matching Go's copy-at-the-call. This also fixes a latent over-broad lookup: `GetExtensionMethod` collapses a closed `ж<X>` to the open `ж<>` definition (correct for single-dispatch precedence, wrong for a method-set query), so a name shared across types could bind the wrong receiver; the binder matches on element identity instead.

**A candidate's EMITTED name is not always its GO name — the method set must be read in Go names (2026-08-01).** A Go method set is a Go-level fact reconstructed at run time from emitted C#, and the emitted name can carry the converter's collision-avoidance marker instead of the Go name. A `-tests` variant Δ-renames a test-file method declarator whose bare name would hijack a same-named dot-imported function at every unqualified call site — B9 in `performNameCollisionAnalysis`, and a real compile fix, since C# resolves the enclosing class's method group ahead of any `using static` import. `io`'s `multi_test.go` is exactly that shape: `func (c *writeStringChecker) WriteString(string)` against the dot-imported `io.WriteString`, so the method emits as `ΔWriteString` while `io.StringWriter`'s member keeps the bare `WriteString`. `StructurallyImplements` compared the two emitted names, answered MISS, no shell was built, and `MultiWriter`'s `w.(StringWriter)` fell through to the `Write` leg — which returns the same `(n, err)`, so the divergence was **silent**: only Go's own `TestMultiWriter_StringCheckCall`, which asserts that `WriteString` was *called*, could see it. `TypeExtensions.GoMethodNameMatches` now projects a candidate's leading `ShadowVarMarker` away, the same way `GoReflect`'s type naming (`ΔHandle` → `Handle`) and struct-field projection already recover a Go name from an emitted one. It runs as a **second pass**, after an exact-name pass satisfies nothing, so a Δ-renamed candidate can never displace a plainly-named one; `AdapterBinder.ResolveReceiverMethods` applies the identical two-pass rule, because a binder that bound a method the probe would not have counted is precisely the disagreement `GetGoMethodSetCandidates` exists to prevent. Measured: `io` 47 → 48 of 54.

**The COMPILE-TIME adapter reads the same rule — an interface member is IMPLEMENTED under the interface's name and FORWARDS under the emitted one (2026-08-19).** The run-time half above kept binder and probe in step; the third consumer, `go2cs-gen`'s `ImplementGenerator`, was left re-deriving nothing at all — it spelled the interface member's Go name at BOTH positions. The two positions are not the same name. The member being implemented must always carry the interface's name (an explicit implementation of `Stringer.String` is spelled `String`, and renaming it would implement nothing), while the implementation it forwards TO carries whatever the converter emitted — and those part company at exactly the Δ-rename above. `flag_test.go`'s five `flag.Value` types each declare `String`/`Set` against the production `flag` package the test variant dot-imports, so all ten emit `ΔString`/`ΔSet`; the adapter forwarded `m_box.String()`, which binds nothing on the box, and C# reported the nearest candidate it could see — `bytes_package.String(ж<bytes_package.Buffer>)`, an unrelated extension. Ten CS1929, and the whole 24-verdict suite sat behind them.

The resolution is a LOOKUP, not a re-derivation: `ImplementGenerator` already builds `localImplNames`, every method the struct declares in either receiver form, so the emitted name is a fact in hand. `Common.ResolveForwardMemberName` matches the interface member against that set — exact name FIRST, the `ShadowVarMarker` projection only as a second pass — the identical two-pass shape `GoMethodNameMatches` runs at run time, so adapter, binder and probe now agree by construction rather than by coincidence. Exact-first is load-bearing rather than stylistic: `Δ` is a Unicode letter and therefore a legal Go identifier character, so a genuinely `ΔX`-named Go method must never be displaced by a projection of `X`. The resolved name drives the forwarding RECEIVER as well as the call target, because `ForwardReceivers` is keyed by declared names too — `URLValue`'s value-receiver `ΔString(this URLValue)` needs `m_box.Value.ΔString()`, and a Go-name miss fell through to the `m_box` default and stranded the call at CS1929 even once the target was right. `ForwardName` is null for every member the collision pass left alone, which is the whole production corpus: measured byte-identical across all 627 behavioral packages, and 0 errors on both solutions. Guard: `src/tests/GenTests/CollisionRenamedForwardTests.cs`. Measured: `flag` 0 (build-blocked) → **23 of 24**.

**`TryCreate` is FAIL-SOFT.** The structural probe is deliberately NAME-ONLY for an open-generic receiver method — a Go `gbox[int]` whose `Get() T` returns an `int` matches `interface{ Get() string }` under that weaker rule (measured). Go answers `ok=false` there, so a shell that cannot be built answers `false` rather than escaping as an exception; a false-positive match must reproduce today's harmless miss, never a crash.

**Tier order in `builtin.TryTypeAssert` is unchanged, and the shells fire only where it previously answered MISS**: nominal `case T` → `IжAdapter` unwrap → `AdapterRegistry` (compile-time adapters) → **shell memo** → `Implements<T>` gate → **shell construction**. That is the regression floor — no assertion that already resolved can take a different path. Construction costs on the order of a microsecond and `fmt` probes three interfaces per formatted value, so the memo ships in the same change: `AdapterRegistry` caches the (dynamic type, interface) decision, **including the negative**. It is deliberately **not** cleared on `AssemblyLoad` — Go fixes a type's method set at compile time, and the extension methods carrying it live in that type's own assembly, which is loaded by definition when we are holding an instance of it. (Extension-method *discovery* caches are still invalidated on load; only this decision cache is not.)

**The two memoized tiers were folded into ONE per-interface itab cache (2026-07-26).** The ladder above consulted the nominal `AdapterRegistry` (a `(Type, Type)` tuple hash) and then a separate per-interface shell memo, in sequence, on *every* assert — and for a shell-resolved pair the first of those could never hit, by construction: a hit would have returned before the shell tier was ever reached. Together with a `target.GetType()` whose result was dead for an interface target (its only consumers sat behind a `typeof(T).IsValueType &&` that short-circuits false), a memoized assert cost **three `GetType()` calls, two dictionary lookups and two `RuntimeType` property calls** before it did any work. `builtin.Itab<TInterface>` replaces both reads with one: a single entry per (dynamic type, interface), holding the resolver **whichever tier produced it** — a registered nominal adapter factory, a runtime shell factory, or `null` for a decided MISS — exactly as Go keeps one `*itab` per pair. `AdapterRegistry` is unchanged as the authoritative durable record; the itab is a projection formed by reading it back, never by forming a second decision. `IsInterface`/`IsValueType` are hoisted to per-closed-generic statics, and a **monomorphic slot** sits in front of the dictionary (an assert site is overwhelmingly single-typed, the locality assumption Go's per-site checks rely on), so the steady-state read is a static field load, an int compare and a reference compare. Tier precedence is unchanged — nominal still wins, shells still fire only where it answers MISS.

Two correctness points are load-bearing. **The entry is one immutable object**, never two static fields: a torn `(type, resolver)` pair would silently construct the *wrong* implementation. And unifying the caches reintroduces the hazard the two separate dictionaries avoided by construction — a shell (or a decided miss) memoized *before* a lazily-loaded assembly's module initializer registers the nominal adapter for that same pair — so `AdapterRegistry.Register` bumps an **epoch** (only on a `TryAdd` that actually adds; the registry is first-wins, so a duplicate can invalidate nothing) and every itab entry carries the epoch it was decided under. An entry from an older epoch is simply never matched and is overwritten when its pair is next formed, which needs no clearing step that could race a concurrent fill. Registration is startup-time and append-only, so the steady state never re-forms anything. A side effect of recording the miss as `null` rather than reading a factory back is that a **miss is now stable**: previously a pair whose factory exists but throws its binding failure out of construction answered `false` once and then re-threw on the next assert, because the projection had published that factory.

Measured on `PerfIfaceShell` (5M iterations, each two asserts plus two forwarded calls; provisional, taken on a contended machine): JIT **789.8 → 683.1 ms** for the unified cache alone, **→ 633.7 ms** with the monomorphic slot — 158.0 → 136.6 → 126.7 ns per iteration. Native AOT **976.5 → 866.2 → 760.1 ms**, a larger relative win because under AOT both shell tiers are reflective and dictionary work is a bigger share. (Guarded by the `ItabLateRegistration` behavioral test — shells decided first, a real module initializer registering real generated adapters afterwards, every pair re-formed, misses still misses, two interfaces over the same types kept separate — and by `tests/GolibTests/ItabEpochTests`, which pins what Go cannot express: tier precedence is deliberately *invisible* from Go, since a shell and an adapter forward to the same receiver methods, so the epoch's effect is only observable at the golib level, where a memoized MISS must start succeeding once an adapter is registered for it.)

Guarded by three behavioral projects, each pairing a `main` package with a sibling library so the interface's package genuinely cannot see the concrete types (the io/fs shape): **`NamedInterfacePointerMethodSet`** (the X3 negative — a value of a pointer-receiver-only type must MISS — plus the positives, a wrong-signature miss and the fail-soft generic-receiver miss), **`NamedInterfaceLateAssert`** (a value-typed defined string satisfying a THREE-DEEP embedded interface chain cross-assembly, an unexported interface, and a partial implementer that must not satisfy the derived interface), and **`NamedInterfaceAdapterIdentity`** (`%T`, re-assert, type switch and interface equality through a shell). Counter-proven: with shell emission disabled, all nine `NamedInterfaceLateAssert` assertions revert to MISS.

**The ladder's CONCRETE-target miss read a custom attribute per call (fixed 2026-07-26) — this was the whole of the `Iface` benchmark row.** Everything above tunes the INTERFACE-target tiers. The concrete-target tiers below them ended in one that answers Go's rule that only dynamic (anonymous) struct types convert to each other, and it asked that question with `Type.IsDynamicType()` — whose body is an uncached `GetCustomAttribute<GoTypeAttribute>()` that materializes a fresh attribute instance on every call. Measured against live golib: **785.92 ns and 368 bytes per call on the JIT, 3,826.27 ns and 2,017 bytes under Native AOT** (ILC parses the attribute blob out of the compiled image's metadata each time and has no equivalent of the JIT's attribute caching). Every `v, ok := x.(SomeStruct)` that does *not* match reached it, so the ordinary named-struct assert — the most common assert in Go code — paid it corpus-wide, and it is what made `PerfIface` the worst row in the performance table at **158× Go on the JIT and 660× under AOT**, a benchmark whose asserts miss on 4 of every 6 iterations. Two layers, one root cause: `IsDynamicType` now **memoizes per type**, beside `GetStructFieldNames`, which memoizes for exactly this reason (deliberately *not* added to `ClearTypeCaches` — like the field-name cache, an assembly load cannot change a type's own attributes); and `AssertFacts<T>` gains **`IsDynamic`**, joining the `IsInterface`/`IsValueType` per-closed-generic facts already hoisted there, so a named-struct miss short-circuits before `GetType()` is even called. Reordering pure predicates in the `&&` chain preserves semantics exactly, and `AnonymousStructs` pins both directions — an anonymous struct asserted against an identically-shaped anonymous struct must HIT, and against a named struct with the same fields must MISS.

**One marker probe answers "is this a wrapper?" for both adapter tiers.** With the attribute lookup gone, what remained on the common path was **four failing interface type tests per type-switch-plus-assert iteration**: `builtin.type()` probes `IInterfaceAdapter` then `IжAdapter`, and `TryTypeAssert` probes the same two. That is not free, and the asymmetry is the runtime's, not the emission's — measured on a boxed value implementing neither, **one failing interface test costs ~2.9 ns (JIT) where one failing sealed-CLASS test is below measurement noise**, because a failing interface `isinst` walks the type's interface map while a failing class test is a short parent-chain compare the JIT inlines. Both consumers ask the same question first — *is this object standing in for another value?* — and for an ordinary Go value the answer is no, so it is worth exactly one test. `IжAdapter` and `IInterfaceAdapter` now share an empty base marker, **`IGoAdapter`**, and both call sites probe it once to gate their two tiers. Generated adapters implement the base transitively, so **`go2cs-gen` is unchanged and no emitted C# moves**. Ordering is preserved where it is load-bearing: in `TryTypeAssert` the `IжAdapter` match leaves the `switch` but stays BELOW the `string` and `case T` arms (an adapter that is itself a `T` still resolves as that `T`), and the gate flag is re-read after each unwrap since what an adapter yields need not be one itself; in `builtin.type()` the `string` arm moves after the `IжAdapter` arm, which cannot change an answer because `string` is sealed and implements neither marker.

Measured end-to-end on `PerfIface` (20M iterations of one slice-of-interface read, two interface dispatches, one concrete comma-ok assert and a three-case type switch). The per-stage A/B is a `--filter PerfIface` run: JIT **10,117.8 ms (158.24×) → 458.1 ms (7.20×)** for the attribute fix, **→ 379.7 ms (5.95×)** with the marker probe. The **published figure is the full-table quiet-machine run, median of 5: JIT 370.1 ms (5.86×) and Native AOT 262.3 ms (4.15×)**, against the pre-fix 10,117.8 ms (158.24×) and 42,228.2 ms (660.42×) — a 27× and 161× improvement respectively. Peak working set fell 41.3 → 23.1 MB (JIT) and 29.6 → 11.1 MB (AOT) as ~4.9 GB of per-assert attribute garbage stopped being allocated. A decomposition micro-benchmark against live golib (best-of-5, `DOTNET_TieredCompilation=0`) attributes the whole of it, per iteration of the emitted loop: slice element read 1.8 ns, two interface dispatches 3.9 ns, comma-ok assert **579.0 → 12.1 → 8.1 ns**, type switch **16.0 → 11.0 → 4.4 ns** — against a floor of 1.2 ns for a plain `s is Circle c` and 1.4 ns for a bare C# pattern ladder, both indistinguishable from the slice read alone. `IfaceShell` moved 44.58× → 44.13× → 40.58× across the same two changes — flat for the first (it is the oracle, and does not touch the miss tier) and improving on the second, since the shell tier enters through the same assert. **AOT now beats the JIT on this row**, reversing the pre-fix order, because ILC's failing interface type tests are markedly cheaper (3.7 ns for two against 9.2 ns on the JIT).

**ANONYMOUS (`dyn`) interfaces use the SAME shells — the second renderer is gone.** An interface literal was the *original* duck-typing case, and it had its own machinery long before named interfaces got any: `TypeGenerator` stamped two static `ᴛAs<ᴛTTarget>` conversion methods plus a `ᴛAs(object)` overload onto every `[GoType("dyn")]` interface, emitted a `Δ<Iface><ᴛTTarget>` wrapper next to it (with a full operator/nil block it never needed), and `builtin.TryTypeAssert` reached that wrapper by *reflecting for the method by name* and closing it with `MakeGenericMethod`. Two renderers of one idea, and the older one carried three real defects the shells do not:

* **It was the last unbelted `MakeGenericMethod` in the assert path.** `MakeGenericMethod` over a run-time type is dynamic code: under Native AOT it succeeds only for an instantiation `ilc` already rooted, and there was no fallback tier — an unavailable instantiation was an unrecoverable MISS, silently wrong rather than degraded. The shells' `IsValueType` branch answers the same case with a shell that needs *no* instantiation, and belts the other way when it does.
* **Its static members were on the interface.** A converted interface's statics are inherited by every interface that EMBEDS it, so `interface{ error; Temporary() bool }` forwarded `ᴛAs` overloads into its own wrapper (CS0102 ×6) and demanded a static helper from the dynamic value's Go method set — both had to be filtered out downstream. Attribute discovery removes the shape rather than the symptom.
* **Its binding used the by-name extension lookup.** `GetExtensionMethod` collapses a closed `ж<X>` to the open `ж<>` definition — right for single-dispatch precedence, wrong for a method-set query — so a method name shared across types could bind another type's receiver. `AdapterBinder` matches on element identity.

Nothing about the *emitted* dyn interface changes except the disappearance of `ᴛAs`: an interface literal is still a `[GoType("dyn")] partial interface`, still resolved structurally at run time, still fail-soft. The `dyn` key is no longer read by `TypeGenerator` at all — its only remaining reader is the runtime's `Type.IsDynamicType`, used for Go's anonymous-struct-to-anonymous-struct conversion, which reads the `[GoType]` attribute directly. Guarded by the existing dyn corpus (`AnonymousInterfaces`, `DynIfaceParamNameCollision`, `DynamicInterfaceKeywordMethod`, `AnonIfaceMethodSetWidening`, `AnonIfaceThroughPointerAdapter`, `AnonInterfaceCrossFile`, `AnonInterfaceSignatureAssert`, `DerivedInterfaceStructuralProbe`, `StructuralAssertFailSoftMiss`), which is the dyn contract and stayed green through the migration unchanged, plus the `PerfIfaceShell` performance benchmark, which executes both tiers under a Native AOT publish.

**golib's three hand-written interfaces joined the same mechanism.** `error` (golib), `fmt.Stringer` and `io.Reader` (the baseline stubs) predate the marker and expose plain `As<T>` helpers; `TryTypeAssert` found *those* by the same reflective probe and closed them the same way, so deleting the probe would have taken their duck-typing with it. Each is now stamped `[GoInterfaceShell(typeof(<I><>), null, "<M>")]` — their existing `<I><T>` carrier class **is** the delegate-bound generic shell, and always was — and each carrier's `(in T)` constructor became `(T)`, because `AdapterBinder` locates a shell's constructor by exact parameter type and an `in` parameter is `T&` in metadata. `null` for the object shell is deliberate rather than a gap: a reflective tier would have to reproduce these carriers' `%v`/`%T` formatting contract (`error<T>.ToString(format, provider)`), so a value-typed error still binds through the generic shell — AOT-graceful, exactly as before. Because a hand-written shell has no `ᴛBoundByPtr`/`ᴛBoundByVal` flags, the binder treats those as optional and forces the type initializer explicitly (`RuntimeHelpers.RunClassConstructor`), so an unbindable pair is still decided — and memoized — at factory-build time rather than rediscovered per construction.

## Cross-package pointer-to-interface conversions use the foreign adapter
A pointer-sourced cast to an interface implemented by a FOREIGN type references the foreign assembly's PUBLIC adapter class - os's `err = &PathError{...}` emits `new fs.PathErrorжerror(Ꮡ(new PathError(...)))`, io/fs having generated the adapter from its own `GoImplement<PathError, error>(Pointer = true)` record. The record's existence is read from the imported package's package_info (`parseExportedPointerImplements`, the same imported-records pattern as GoTypeAlias). The existence key is the **shared canonical spelling** both sides compose through `implementRecordKey` — `<declaring package>|<C# simple type>|<pkg>_package.<Iface>` — exactly as the value-implement records do; keeping the package CLASS on the interface side is what stops image's `Paletted→image.Image` record from satisfying a `Paletted→draw.Image` cast and referencing the adapter that implements the WRONG interface (CS1503). The reference goes through the file-local package ALIAS (`fs.PathErrorжerror`, user-ruled style) via getAliasQualifiedTypeName, which also registers the using — except when that yields a whole-TYPE alias for a collision-renamed foreign type (`imageꓸRGBA`), which is an identifier and not a path, so the base is rebuilt as the package qualifier plus the type's EMITTED simple name (`image.ΔRGBAжImage`). Guarded by `CrossPkgUser` (`rep = mtr` -> `new CrossPkgLib.MeterжReporter(mtr)`; `&CrossPkgLib.Alarm{}` -> error; and the same-simple-name LOCAL `Labeled` — `var localLb Labeled = sp2` takes the LOCAL `CrossPkgLib_SensorжLabeled`, never the lib's exported `SensorжLabeled`).

An **EXPLICIT pointer-to-interface conversion** — Go's `image.Image(dst)` with `dst *image.RGBA` (image/draw) — is the same interface cast in conversion clothing and routes through the same machinery: `isTypeConversion` probes the ORIGINAL pointer type against an interface target (the value type alone does not implement it — the elem-only probe misread the conversion as a constructor call, `new image.Image(dst)`, CS0144), and the emission re-renders the argument in its BOX form and CASTS the adapter to the interface — `((image.Image)new image_ΔRGBAжImage(Ꮡdst))` — because the adapter implements its members explicitly, and a chained member access on the conversion result (`CrossPkgLib.Labeled(sp).Label()`) cannot bind on the adapter class itself (CS1929). (Guarded by `CrossPkgUser`'s `CrossPkgLib.Labeled(sp2).Label()` / `LabeledOf(sp2)` pair, output-compared vs Go.)

## `%T` (and type-name rendering generally) unwraps generated adapters and pointer boxes

Go's `%T` prints the interface value's **dynamic Go type** — `*strings.byteReplacer`, never an implementation artifact. The managed model interposes artifacts a name renderer must see through (strings' `TestPickAlgorithm`, which `%T`s each replacer algorithm, printed `strings.byteReplacerжreplacer`):

* a **pointer-sourced ж adapter** (`byteReplacerжreplacer : IжAdapter`) stands in for the `*T` it wraps → renders `*strings.byteReplacer`;
* a **value-sourced foreign ᴠ adapter** (`typelib_Markᴠstamper`) wraps a struct copy → renders the struct type, `typelib.Mark`;
* an **interface-to-interface adapter** (`IInterfaceAdapter`) forwards to its wrapped value's dynamic type;
* a **raw receiver box** `ж<T>` (a pointer held in an `any` with no adapter in its history) renders `*main.loud`;
* a **converted named type** package-qualifies from its `<pkg>_package` declaring class: `go.main_package+soft` → `main.soft`.

The unwrap lives at the shared choke points, so every formatting path agrees: `GoReflect.GoTypeName` (which `reflect.Type.String()` serves the converted `fmt`'s `%T` from, via the Phase-1 reflection bridge) gains a `TryAdapterWrappedType` arm, and golib's `builtin.GetGoTypeName` (the stub `fmt` `%T`, the testing shim's `TestFormat`, and interface-conversion panic texts) unwraps `IжAdapter.Box`/`IInterfaceAdapter.Value` at the value level and routes ж/adapter/named types through `GoReflect.GoTypeName`. Adapter detection is structural, never name-parsed for the wrapped type: `IжAdapter` (or the `ᴠ` infix per `Symbols.ValueAdapterInfix` for value adapters) identifies the adapter, and the wrapped type is read from the adapter's single one-parameter constructor (`ж<T>` → pointer-sourced; the struct type → value-sourced). `reflect.Kind()`/`Elem()` of an adapter type still report the adapter class (a reflection-bridge follow-up owned with R5's DeepEqual work), but `%T`/`String()` — the only surface the corpus exercises — are Go-exact. (Guarded by `FormatTypeAdapters` — a two-project behavioral test whose `typelib` sub-package supplies the foreign value implementer, output-comparing `%T` over the ж adapter, the local value implementer, the raw box, a plain named struct, the ᴠ adapter, and a nil interface vs `go run`; without the unwrap it prints `main_package+loudжgreeter` / ``ж`1[[go.main_package+loud, …]]`` / `main_package+typelib_Markᴠstamper`.)

The **VALUE mirror of the explicit conversion** — `crypto.SignerOpts(sigHash)` with `sigHash crypto.Hash` (crypto/tls, CS0030 ×4) — routes a **FOREIGN named non-interface VALUE source** through the same `convertToInterfaceType` machinery, keeping the outer interface cast: `((crypto.SignerOpts)new crypto_HashᴠSignerOpts(sigHash))` plus the local value-form `GoImplement<crypto_package.Hash, crypto_package.SignerOpts>` record. A plain cast cannot bind here: a foreign value type implements its interfaces via **extension methods** (never structurally), and the converting assembly cannot `partial` a foreign type — the same reason the implicit both-foreign value arm exists (`syscall.Signal`→`os.Signal`). When the defining assembly already implements the pair (its package_info carries the value-form record), `convertToInterfaceType` falls through and the emission stays the plain cast spelling. LOCAL value sources originally kept the plain-cast route on the reasoning that a local type can be partial'd to declare the interface; that reasoning was half-right and the route now covers them too (next paragraph). The outer cast is load-bearing exactly as in the pointer arm — and additionally because `var signOpts = …` must type as the INTERFACE: each tls site reassigns `signOpts` to a different adapter two lines later (CS0029 hazard if the var typed as the adapter class). (Guarded by the `CrossPkgLib`/`CrossPkgUser` extension — `Verdict` implements `Scored` via a value receiver with deliberately NO witness in the lib, `CrossPkgLib.Scored(CrossPkgLib.Verdict(4))` converts explicitly in the user package and the same var is then reassigned a local `*tallies` implementation, output-compared vs Go; whole-stdlib reconvert diff: exactly the four crypto/tls sites plus its package_info record.)

The **LOCAL named VALUE source** of the same explicit conversion — `crypto.Signer(private)` with `type PrivateKey []byte` (crypto/ed25519, CS0030 ×2) and `pinUnexpMeth(EmbedWithUnexpMeth{})` (internal/reflectlite) — routes through `convertToInterfaceType` as well (2026-08-18). The original “no churn on local sources” boundary reasoned that a local type can be `partial`'d to declare the interface, which is true and is exactly why the route matters: the partial is go2cs-gen's, minted from an `[assembly: GoImplement<T, Iface>]` record, and a plain cast records **nothing** — so the cast had nothing to bind to whenever no other site recorded the pair. “No other site” is precisely the two shapes the speculative recorder (`recordSamePackageImplements`) declines: an interface declared in ANOTHER assembly (it pairs two locals only) and an UNEXPORTED local interface (its exported gate — a record is a cross-assembly contract). Framed by SYNTAX the rule is: Go's `Iface(x)` and `var i Iface = x` are the same conversion, and the emission must not depend on which spelling the source used — the assignment form has always routed through `convertToInterfaceType`. For a local non-func value source the route is **record-only** (the expression text is unchanged, preserving the original boundary's intent: a seeded whole-stdlib reconvert after the change is 1,668 emitted / 0 real differences / 0 new); a local named FUNC source is the one emission that moves, correctly, onto its generated `ᴠ` value adapter (a C# delegate cannot be a partial struct). An INTERFACE source still takes the plain cast — that position belongs to the `recordableInterface` class and is measured broken in call syntax on its own terms (`valued(d)` with `d` an interface throws `InvalidCastException` where assignment syntax builds the adapter), recorded on the phase-4 board as its own arc rather than folded in here. (Guarded by `LocalValueIfaceCallConversion` — slice-underlying and struct locals cast call-syntax to `fmt.Stringer`, the reflectlite shape verbatim at package scope, a no-churn local-exported-interface control, `meter`/`gauge` against one interface, identity/assert/map-key semantics output-compared vs Go — and by the converter-level `TestLocalValueIfaceCallConversion`, which pins the records that must appear, the emissions that must not move, and the interface-source position left untouched.)

**No exported adapter — the LOCAL adapter for a foreign pair.** When the defining package never
converts the pair itself (os never casts `*File` to `io.Reader`, so no record exists to
reference), the converting package records `GoImplement<os_package.File, io_package.Reader>(Pointer
= true)` **locally** and the generator emits a **local adapter class** for the foreign struct
(`internal sealed class os_FileжReader`; the `m_box` field is fully qualified). The class name is
**package-qualified** (`{pkg}_{Struct}ж{Iface}`): two same-named foreign structs adapting to one
interface otherwise compose a single colliding class — math/big records both `bytes.Reader` and
`strings.Reader` against `io.ByteScanner` (CS0102/CS0111/CS8646 ×8). The local VALUE adapters for
foreign structs qualify the same way (`syscall_ΔSignalᴠΔSignal`); a LOCAL delegate's value adapter
stays bare (`funcValueᴠValue`). Forwarding decisions come from **metadata** — the compiled foreign
assembly exposes every converter and sibling-generator form as real symbols, so an extension on
`ж<T>` binds the box (`m_box.Read(p)`) and everything else binds the deref'd value
(`m_box.Value.M()`, ref extensions bind through the ref-returning `Value`). This replaces the old
deref-COPY fallback, so aliasing is faithful: fmt's `Fscan(os.Stdin, …)` emits
`Fscan(new os_FileжReader(os.Stdin), …)` (CS1503 ×3, the last fmt family). Guarded by
`CrossPkgUser` (`*Probe → Sampler` via `CrossPkgLib_ProbeжSampler`, mutation read back through the
original pointer).

## A cross-package interface's unexported sealing marker is stubbed
Go seals an interface to its defining package with an **unexported marker method** — `ast.Expr`'s
`exprNode()`, `ast.Stmt`'s `stmtNode()`, `ast.Decl`'s `declNode()`, `text/template/parse.Node`'s
`tree()`/`writeTo()`. The method's C# implementation is an **internal** extension in the interface's
own assembly (`internal static void exprNode(this ref IndexExpr _)`), so an adapter generated where
the interface is CONSUMED — `go/internal/typeparams` casting go/ast's `*IndexExpr` to `ast.Expr`, or
`text/template` casting `*parse.RangeNode` to `parse.Node` — cannot see it: forwarding
`m_box.Value.exprNode()` is CS1061. The C# interface member itself is public (unexported Go methods
render without a modifier), so it is still *required* — dropping it is CS0535, and an *internal*
interface member cannot be implemented cross-assembly at all. Because Go never lets a sealing marker
be called from outside its package, the adapter satisfies the member with a **no-op / `default!`
stub** instead of forwarding:
```csharp
void global::go.go.ast_package.Expr.exprNode() { }                       // void marker
global::go.…parse_package.Tree global::go.…parse_package.Node.tree() => default!;   // non-void marker
```
The `ImplementGenerator` flags a method as an inaccessible marker when its Go name is unexported
(`GetScope == "internal"`), its declaring assembly differs from the one the adapter is generated
into, **and the struct declares no method of that name in the current compilation**
(`MethodInfo.IsInaccessibleMarker`); a SAME-assembly impl keeps forwarding (the internal extension is
accessible there). Both the pointer (`AdapterImplTemplate`) and value (`ValueAdapterImplTemplate`)
adapters emit the stub. This greens `go/internal/typeparams` (whose only errors were the two
`exprNode` forwards) and is a prerequisite for `text/template`/`go/doc`. (Guarded by
`CrossPkgLib`/`CrossPkgUser`: the sealed `Emitter` interface with an unexported `emitNode()`, a
`*Leaf` implementing it, cast to `Emitter` in the consumer assembly — CS1061 without the stub.)

That third clause is the correction the white-box test model forced (2026-08-09, `internal/profile`).
The assembly comparison is a *proxy* for "there is nothing to forward to", and it answers wrongly for
the one shape where a single Go package spans two C# assemblies: an **INTERNAL (white-box) test
package**. `internal/profile`'s `proto_test.go` is `package profile` — it declares `packedInts` and
its `encode`/`decoder` methods for the production package's own unexported `message` interface. Same
Go package, different C# assembly, and genuinely reachable, because the test model mints an
`InternalsVisibleTo` grant for exactly this. Stubbing there is worse than a compile error: the
adapter COMPILES and *silently does nothing*, so `marshal(source)` returned an empty buffer and
`unmarshal` decoded nothing, with no diagnostic at any layer. Requiring the absence of a local
implementation — the struct's own value/ref extensions plus its direct-`ж` primaries — leaves every
genuine marker stubbed unchanged, because a FOREIGN struct never declares the sealing method (Go
forbids implementing another package's unexported method at all, so a `[GoImplement]` record naming
an unexported interface method can only come from a struct in that same Go package). The guard is
`internal/profile`'s own banked suite: the shape needs a white-box test package, which the behavioral
corpus has no way to express.

The white-box model then forced a SECOND correction to the same clause (2026-08-19, the crypto/tls
regression). `internal/profile`'s shape declares the STRUCT in the test compilation, so the
local-implementation evidence — gathered from the struct's own declaration syntax — was findable
there. crypto/tls's `TestMarshalUnmarshal` is the mirror shape: the struct is a PRODUCTION type
(`*SessionState`, metadata-only in the test compilation) and only its METHODS are test-declared
(handshake_messages_test.go's `marshal`/`unmarshal`, the sole source of its `handshakeMessage`
satisfaction — Go lets a package's test files add methods to its production types). With no local
declaration syntax the evidence set was EMPTY, both members classified as markers, and the identical
silent-stub failure recurred: marshal answered an empty buffer with nil error and the test reported
"failed to unmarshal" with no diagnostic. The evidence now also covers the friend bridge's
extensions by receiver simple name, in BOTH receiver forms — direct-`ж`, and `[GoRecv] ref` (which
forwards through its RecvGenerator `ж`-twin, the same routing `IsRefRecv` applies to a local
declaration). Genuine markers still stub: a foreign struct with no bridge has no such extensions
anywhere in the compilation. (Guarded by `GenTests.WhiteboxBridgeAdapterTests`, which runs the real
generator over a two-assembly model of the shape, and the ref-scan rows in
`GenTests.FriendBridgeBoxReceiverTests` — the behavioral corpus still cannot express a white-box
test package.)


### …so the DECLARING package must own the adapter, and its speculative record carves out for it

The stub above rests on one sentence — *"Go never lets a sealing marker be called from outside its
package"* — which is true and is not the whole rule. The marker cannot be called from outside; it is
called **inside**, on a value the consumer boxed, which is the entire reason a sealing interface has
unexported members in the first place. `text/template/parse.ErrorContext(n Node)` opens with
`tree := n.tree()`; `html/template` boxes an `&n.BranchNode` into that `Node`. Nothing in
html/template can call `tree()` — and nothing has to, because `parse` does it for them. A stubbed
`tree()` answers `default!` there, `ErrorContext` then substitutes its own receiver, and at
html/template's `(*parse.Tree)(nil).ErrorContext(e.Node)` call site that receiver is also nil, so
`~tree` nil-dereferences. `TestErrors` was the symptom, three packages from the record that was never
written (2026-08-20).

So the stub is a **last resort, not a design**, and it must be unreachable for any pair that can be
realized properly. It can always be realized in ONE assembly: Go scopes an interface with an
unexported method to its declaring package, so every type that will ever implement it is declared
there, and the declaring assembly's own adapter forwards the marker natively (its extension is
`internal`, and that is the assembly it is internal to). A consumer then references the exported
`pkg.TжIface` through the existing foreign-adapter-exists arm and mints nothing. That is what
[`recordSamePackageImplements`](package-conversion.md#a-package-records-the-pairs-it-satisfies-not-only-the-ones-it-witnesses) already
does for the pairs a package satisfies but never witnesses — it simply withheld this one.

The gate it withheld on is `generatorCanForwardPointerMethodSet`, which demands every interface method
resolve DIRECTLY on the type (no promotion at all), on the stated reasoning that *"withholding a
speculative record is always safe, because the consumer keeps the local adapter it had before"*. That
sentence is true of an all-exported interface and **false of a sealed one**, where the local adapter
is not a fallback but an adapter that cannot work. `*parse.BranchNode` is exactly the shape: it
implements `Node`, but `Type()` and `Position()` are promoted from its embedded `NodeType`/`Pos`, so
the strict gate refused — and `parse` never casts a `*BranchNode` itself (it casts the
`If`/`Range`/`With` wrappers), so nothing demanded the record either.

The carve-out is therefore keyed on the interface, not on the type: **when the interface carries an
unexported method, the pointer record falls back to the VALUE form's depth-2 bound instead of being
withheld.** It stays bounded — a promotion deeper than one embed hop is still refused, exactly as
before — and the shape the strict gate was written for (a speculative record whose promoted member
resolves through the wrong embedded POINTER hop, `StructPointerPromotionWithInterface`'s
`MyCustomError`) is an all-exported interface that never reaches the arm. Corpus footprint at
`text/template/parse`: one line, `[assembly: GoImplement<BranchNode, Node>(Pointer = true)]`.

What remains stubbed is what should be: a pair the declaring package genuinely cannot realize
(promotion deeper than one hop, a generic, an unexported target) would still mint a consumer-local
adapter whose marker is a stub, silently. **Censused after the fix, the standard library holds no such
pair**: of the **1,307** `ImplementGenerator` adapters a whole-stdlib reconvert generates, **zero**
carry a `=> default!` or empty-body member. The shape stays reachable — it is what to suspect when a
sealed interface's member answers a plausible zero — but it has no instance today.

Guarded by `CrossPkgLib`/`CrossPkgUser`, extended for this: `Emitter` gains a VALUE-returning sealed
member `nodeTag() string` and the lib gains `DescribeEmitter(e Emitter)`, the `ErrorContext` shape —
a declaring-package reader of the sealed member, on a value the consumer boxed. `*Leaf` (whole method
set declared directly, so its record was never withheld) is the control and reads `leaf/lf` either
way; `*Branch` (Emit promoted through its `EmitBase` embed) read **`branch/`** before the carve-out
and reads `branch/brn` after — proven by neutering the carve-out and running the pair.

## A dynamic interface's runtime conversion class re-escapes a keyword method name
An anonymous or type-asserted interface is lifted to a `[GoType("dyn")]` partial interface (see
[Anonymous interfaces used as an adapter target](#anonymous-interfaces-used-as-an-adapter-target-are-lifted-package-wide)),
and for the dynamic form `go2cs-gen`'s `InterfaceTypeTemplate` additionally emits a **runtime
conversion class** — `ΔI<ᴛTTarget> : I` — that duck-types a target at run time by reflection-binding
each interface method to the target's extension methods (the fallback for a duck-typed assertion the
compile-time `ImplementGenerator` could not resolve). When such a dynamic interface **embeds** an
interface carrying an unexported *sealing* method whose name is a C# reserved keyword — internal/testenv's
`interface{ testing.TB; Deadline() (time.Time, bool) }`, where `testing.TB` has `private()` — that name
must be `@`-escaped in the generated class. The converter already escapes it in the interface itself
(`void @private();`), but the sealing method reaches the conversion class through the base-interface walk
(`interfaceSymbol.AllInterfaces`), and a symbol name read from Roslyn (`IMethodSymbol.Name`) arrives
**UNescaped** — unlike a syntax `Identifier.Text`. Emitting it raw yields `void private()` and
`nameof(private)`; that syntax error corrupts the class body, and because the conversion class is nested
inside the `public static partial class …_package` container the parse recovery ejects every subsequent
operator into the *static* container — **CS0715** ("static classes cannot contain user-defined operators")
×25 plus a **CS0246** cascade (~54): 84 errors from one keyword method. (The nesting itself is legal — a
non-static class nested in a static class holds instance members and operators fine, as every non-keyword
dynamic interface proves; only the broken body triggers the eject.)

The fix re-escapes the name only where it is emitted as its own identifier token — the method declaration
(`MethodInfo.GetSignature`) and each `nameof(...)` in the reflection-binding static constructor. The
compound delegate/field names (`{Name}ByPtr`, `s_{Name}ByPtr`) stay on the raw name: a keyword + suffix
is never itself a keyword, and `@` cannot appear mid-token. `EscapeCsKeyword` is a no-op for every
non-keyword method, so all other dynamic-interface output is byte-identical. Emitted form:
```csharp
internal class ΔcommandContext_type<ΔTTarget> : commandContext_type
{
    private delegate void privateByPtr(ж<ΔTTarget> targetʗ);              // compound name — raw
    public void @private() { … }                                         // declaration — escaped
    // static constructor:
    extensionMethod = targetType.GetExtensionMethod(nameof(@private));   // nameof — escaped
}
```
Greens internal/testenv (its only errors were this one method's cascade). Guarded by the
`DynamicInterfaceKeywordMethod` behavioral test — a named `TB` interface with a `private()` sealing
method, embedded in a type-assertion's anonymous interface so the lifted `[GoType("dyn")]` target's
conversion class must implement the escaped `@private()`; it does not compile without the fix.

## A keyword-named type's interface adapters escape declarations and compose class names unescaped
A Go **type** whose name is a C# reserved keyword (`type fixed struct{…}`, `type lock interface{…}`) is
`@`-escaped by the converter everywhere it stands as its own identifier token (`[GoType] partial struct
@fixed`, `ж<@fixed>`, `@lock l = f`). Two other name paths mishandled such types:

1. **`ImplementGenerator`'s emitted type positions.** A LOCAL struct's name reaches the generator as a
   bare Roslyn SYMBOL name — UNescaped, unlike display strings (`ToDisplayString()` uses
   `CSharpErrorMessageFormat`, which escapes, so `go.main_package.@lock` arrives correct). Emitting the
   raw name produced `partial struct fixed : sizer` — which the C# parser reads as a *fixed-size-buffer*
   declaration, ejecting mangled members into the static `…_package` container (**CS0708**
   `'main_package.'` "cannot declare instance members in a static class" plus a CS1642/CS1663/CS7092
   buffer cascade) — and the same raw name inside the pointer adapter's `ж<fixed>`. The generator now
   applies `EscapeCsKeyword` at those emission sites (`InterfaceImplTemplate.StructName`, the pointer
   adapter's wrapped `StructName`, and the value-embed hop's class qualifier); it is a no-op for every
   non-keyword name.

2. **Adapter class-name composition, BOTH sides.** `@` is only legal at the START of a C# identifier
   token, so a keyword part cannot carry its marker into a composed adapter name: the converter emitted
   `new @fixedж@lock(Ꮡf)`, which lexes as TWO tokens (`@fixedж` + `@lock` — CS1526). Both composers now
   build from UNESCAPED simple names — the converter's `adapterTypeRef`/`valueAdapterTypeRef` via
   `stripSanitizationMarkers` (which also clears a pre-qualified `os_@fixed`-style interior marker), and
   the generator's `AdapterName` compositions via `GetUnsanitizedIdentifier` — producing
   `fixedжlock`/`fixedᴠlock`. The composed name always contains the `ж`/`ᴠ` infix or a package prefix,
   so it is never itself a keyword and needs no marker (the same rule the keyword-method compound names
   above rely on: a keyword + suffix is never a keyword).

Emitted form (from the `KeywordNamedTypes` goldens and its generated adapters):
```csharp
sizer p = new fixedжsizer(Ꮡf);                          // converter cast site — composed, no marker
@lock lp = new fixedжlock(Ꮡf);

partial struct @fixed : global::go.main_package.@lock   // generator value-form — escaped declaration

internal sealed class fixedжlock : global::go.main_package.@lock, IжAdapter
{
    private readonly ж<@fixed> m_box;                   // escaped type reference
```
`TypeGenerator` and `RecvGenerator` were already correct — they read syntax `Identifier.Text`, which
keeps the `@fixed` spelling. Guarded by the `KeywordNamedTypes` behavioral test: struct `fixed` value-
and pointer-implementing `sizer` plus a keyword-named interface `lock`, with a pointer-receiver `grow`
exercising the RecvGenerator ж-twin on the keyword-named receiver.

## An interface member's keyword-named PARAMETERS escape in every generated implementation

The same symbol-vs-syntax asymmetry reaches parameter NAMES. `sync.Map`'s

```go
func (m *Map) CompareAndSwap(key, old, new any) (swapped bool)
```

is emitted by the converter with the keyword escaped (`any @new`), but `ImplementGenerator` re-reads
the members off the interface SYMBOL when it realizes a `[GoImplement]` record, and
`IParameterSymbol.Name` — like `IMethodSymbol.Name` above — arrives with the escape stripped. Every
template renders that name straight into a declaration and a forwarding call, so the generated
explicit implementation emitted `object new`, which does not lex as a parameter; Roslyn's recovery
reported **CS0501** ("must declare a body because it is not marked abstract, extern, or partial") on
the enclosing member — three of them in sync's converted test build, one per type implementing the
test's `mapInterface`.

The three symbol→`MethodInfo` projections (the interface-adapter path, the struct-adapter/explicit-impl
path, and `MethodInfo`'s own `IMethodSymbol` overload) duplicated the same tuple construction, so the
escape lands once in a shared `ToParameterInfos` extension that all three now call; it also carries the
`in`/`ref`/`out` prefix the two adapter paths need (an explicit implementation must reproduce the
ref-kind or it matches no member, CS0539). `EscapeCsKeyword` is a no-op for every non-keyword and for
an already-escaped name, so all other generated output is byte-identical:

```csharp
bool global::go.sync_test_package.mapInterface.CompareAndSwap(object key, object old, object @new)
    => m_box.CompareAndSwap(key, old, @new);
```

Guarded by the `SymbolParameterInfoTests` GenTests cases (name escaping, ref-kind composition, and a
parse assertion on the rendered declaration + forwarding call — the failure is a *parse* failure, so
the string compare alone would not pin it) and by the `InterfaceKeywordParamNames` behavioral test,
which drives BOTH realization shapes — a pointer-receiver implementation (the `ж<T>` adapter) and a
value-receiver one — through an interface whose members declare `new`, `lock`, `base` and `event`
parameters, values vs Go.

## A keyword-named addressed global's heap-box field strips the escape after the Ꮡ prefix
An address-taken package-level var is backed by a heap-box FIELD plus a ref-returning property
(`writeAddressedGlobalDecl`). A keyword-named such global (`var null = json.RawMessage([]byte("null"))`,
net/rpc/jsonrpc) arrives keyword-escaped (`@null`), and composing the box as `Ꮡ` + `@null` places the
escape INTERIOR to the identifier token — `Ꮡ@null` lexes as two tokens (a whole-file syntax cascade).
The `Ꮡ` prefix already de-keywords the composed name (the keyword + affix rule the adapter compositions
above rely on), so the field declaration strips the escape — matching every `&null` use site, which
already composed `Ꮡnull` through `boxBaseName`:
```csharp
internal static ж<slice<byte>> Ꮡnull = new(slice<byte>((@string)"null"));
internal static ref slice<byte> @null => ref Ꮡnull.ValueSlot;   // the var itself keeps its escape

var p = Ꮡnull;                                                  // use site, unchanged
```
Guarded by `HeapKeywordVar` (a package-level `var null` written through its pointer and read back both
ways), alongside its existing keyword-named LOCAL coverage.

## A foreign struct's promoted method forwards through its value embed
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

## A named field whose name equals its interface type is NOT an embedded interface
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

## Cross-package value-to-interface conversions use the local VALUE adapter
A VALUE conversion of a FOREIGN named type to a LOCAL interface (os's `Signal` interface is
DOWNSTREAM of `syscall.Signal` — neither assembly can partial the other) records
`GoImplement<foreign, localIface>` locally; the `ImplementGenerator` detects the foreign struct
(different containing assembly, no local declaration) and emits a **value adapter class**
`{pkg}_{Struct}ᴠ{Iface}` (composed with `Symbols.ValueAdapterInfix`; package-qualified for a
FOREIGN struct — see the pointer-adapter collision note above) wrapping a **COPY** of the struct
— exactly as a Go interface holds a value — with value equality. The conversion site emits
`new syscall_ΔSignalᴠΔSignal(sig)`. The adapter's struct field is **fully qualified**
(`GetFullTypeName(true)`): the bare name resolved to the LOCAL same-named type when os's
`ΔSignal` interface shadowed syscall's `ΔSignal` struct.

Method forwarding uses the **container-qualified static form** —
`global::go.encoding.binary_package.Uint32(m_value, b)` rather than `m_value.Uint32(b)`:
converted Go methods are extension methods on the package class the struct nests in, and the
instance form only resolves when the generated file has a `using` for that namespace (`using go;`
covers root-namespace packages like io/os, but a sub-namespace package like `encoding/binary`
never resolved — debug/plan9obj CS1061 ×6). The static form is exactly equivalent and needs no
using at all.

**BOTH-FOREIGN value pairs take the same route.** When the interface is foreign too
(debug/plan9obj passes `binary.BigEndian`, an `encoding/binary` value, as `binary.ByteOrder`),
the converter first consults the imported package_info records (`parseExportedValueImplements`,
plain or `Promoted` `GoImplement` forms): if the defining assembly already implements the pair,
the bare value converts implicitly and nothing is recorded. Otherwise the pair is recorded
locally and the conversion site wraps in the locally generated value adapter
(`new binary_bigEndianᴠByteOrder(binary.BigEndian)`) — the value sibling of the both-foreign pointer
adapter above.

**A value adapter declares `IValueAdapter`, so the runtime sees the Go DYNAMIC TYPE through it.**
The adapter is a C# wrapper class, but Go's dynamic type of the interface value it carries is the
wrapped struct. golib settles that question at three places — `AreEqual` (Go `==`), `TryTypeAssert`
(`x.(T)` and every type-switch case guard), and `type()` (the type-switch operand) — and each one
unwraps the OTHER two adapter kinds through their marker (`IжAdapter.Box`, `IInterfaceAdapter.Value`)
while the value adapter carried no marker at all. So all three answered against the **wrapper class**:

```go
got := dst.At(0, 10)                 // image: color.Color carrying a color.NRGBA
got == color.NRGBA{…}                // Go true  -> C# false (AreEqual: type mismatch, bails
                                     //            before the adapter's own Equals ever runs)
v, ok := got.(color.NRGBA)           // Go ok=true -> C# ok=false
switch got.(type) { case color.NRGBA: … }   // Go matches -> C# fell to default
```

The panic text made the shape unmistakable: `interface conversion: interface {} is
colorlike.NRGBA, not colorlike.NRGBA` — `%T` already unwrapped (through `TryAdapterWrappedType`)
while the assert did not. The fix is the missing third marker, mirroring the other two exactly:

```csharp
public interface IValueAdapter : IGoAdapter { object? Value { get; } }
```

`ImplementGenerator`'s `ValueAdapterImplTemplate` now emits it on every `ᴠ` adapter —
**explicitly** implemented, so it can never collide with a forwarded Go method named `Value` (a
promoted adapter binds its members by bare name) — and the three sites unwrap it beside the pointer
kind through one shared `UnwrapAdapter` helper, still behind the single `IGoAdapter` probe that
keeps an ordinary Go value at one failing interface test. `GetGoTypeName`, `GoDynamicTypeOf` and the
reflect-bridge assignability check gained the same arm, and `GoReflect.TryAdapterWrappedType`
switched from a NAME probe for the `ᴠ` infix plus a package-class nesting check to the exact marker
test. One marker fixes equality, both assert forms, the type switch and `%T` together — an
`Equals`-only fallback would have fixed equality alone and left the dynamic-type rule weaker.

Unwrapping in the assert's **interface** tier matters independently: the adapter class carries only
the ONE interface it was generated for, so probing it can never resolve the wrapped struct's other
interfaces — `image/gif`'s `m.ColorModel().(color.Palette)` and `c.(color.RGBA)` are exactly that
shape.

Only the CROSS-ASSEMBLY shape reproduces any of this: a same-package value conversion implements the
interface on the struct directly (the value-boxing partial-struct implementation), so there is no
wrapper and never was a problem — which is why the defect stayed invisible until packages like
`image`/`image/draw` converted a foreign struct to a foreign interface. (Guarded by the
`ValueAdapterDynamicType` behavioral test: a sibling `colorlike` package declares the interface and
two value-receiver implementers, `main` does the conversion, and the test exercises `==` both
operand orders, interface-vs-interface equality, both type-assert forms plus a miss, a type switch
over both implementers, `%T`, method dispatch and an interface-keyed map — output-compared vs
`go run`.)

## Under `-tests`, a white-box PRODUCTION type is FOREIGN to the generator — so the name carries the prefix
The two sides of an adapter name must compose it identically, and they answer *"is the source type
foreign?"* by different means: the generator tests the **containing assembly**, the converter tests the
**Go package**. Under the white-box reference model those two disagree about exactly one set of types —
the package under test's own. `go/packages` merges the production files into the INTERNAL test variant's
Go package, so `pkg == v.pkg` reads `net.Conn` as local; its C# lives in the *referenced* production
assembly, which the generator reads as foreign and therefore prefixes.

The value arm already carried that carve-out (`whiteboxProductionTarget`, added for `encoding/binary`).
The **interface**-sourced arm did not, so every cast site named a type that does not exist — 26 CS0426
across seven of net's internal test files, 55% of everything left after the r27 syntax cascade closed:

```csharp
new net_test_package.ConnᴠReader(c)        // referenced (arm composed the name unprefixed)
public sealed class net_ConnᴠReader : …     // generated (prefixed, in the SAME anchor class)
```

The anchor was never in doubt — the record lands in `package_test_info.cs` and the class is generated
into the test metadata class both sides name. Only the *simple name* disagreed. `isSameAssemblyPkg`
is deliberately left alone: it already answers correctly (both reference models clear
`testPackagePath`), and it remains the RECOMPILE fallback's answer, where production sources really do
compile into the test assembly. Behavioral CNR is byte-identical — the shape exists only under
`-tests`.

## An exported func type publicizes the unexported types in its signature
An EXPORTED named func type becomes a `public` C# delegate; an unexported type in its signature —
x/text/unicode/bidi's `type Option func(*options)`, where `options` is package-private — is then
less accessible than the delegate (CS0059, "inconsistent accessibility"). The type-accessibility
pass, which already publicizes the unexported types exposed by an exported struct field / package
var / method signature, also walks an exported named type whose underlying is a `*types.Signature`
and publicizes the unexported named types in its parameters and results:

```go
type options struct{ … }        // unexported
type Option func(*options)       // exported -> public delegate
```
```csharp
[GoType] public partial struct options { … }   // publicized to match the delegate
public delegate void Option(ж<options> _);
```

Only a package with an exported func type over an unexported type is affected (no golden churn). (Guarded by the `PublicizedFuncTypeParam` behavioral test.)

**A func-TYPED exported field or var publicizes the unexported types in the func signature.** The
accessibility walk that publicizes an unexported type exposed by an exported field / package var
(`collectUnexportedNamedTypes`, CS0052) peels `pointer`/`slice`/`array`/`map`/`chan` wrappers to reach
the element type — but stopped at a `*types.Signature`, so an unexported type reachable ONLY through a
func-typed field's signature was left `internal`. crypto/internal/hpke's

```go
type hkdfKDF struct{ … }                          // unexported
var SupportedKDFs = map[uint16]func() *hkdfKDF{…}  // exported var -> public field
```

emits `public static map<uint16, Func<ж<hkdfKDF>>> SupportedKDFs`, whose type embeds `hkdfKDF` through
the func RESULT — but `[GoType] partial struct hkdfKDF` defaulted to `internal`, less accessible than
the public field (CS0052). `collectUnexportedNamedTypes` now has a `*types.Signature` case that recurses
into the signature's PARAMS and RESULTS through the same named-only walk (which handles a nested func
result in turn), so `hkdfKDF` is publicized to `[GoType] public partial struct hkdfKDF` (and its exported
methods go public via the receiver-access cascade). Both sides of the signature are covered — a func
PARAMETER exposes an unexported type just as a func RESULT does (`var Appliers = []func(*cfg)` →
`public static slice<Action<ж<cfg>>> Appliers`, publicizing `cfg`). This routes through the named-only
`collectUnexportedNamedTypes`, NOT the signature-context `collectSignatureTypes`: a lifted anonymous
struct/interface written in the func signature stays the CS0050/CS0051 signature domain, so only genuinely
func-reachable NAMED types publicize here. (Guarded by the `FuncFieldUnexportedType` behavioral test — a
public `map[uint16]func() *hkdfState` var whose func result exposes an unexported type, plus a
`[]func(*cfg)` var whose func parameter exposes another, output-compared vs Go; both fail CS0052 without
the publicize.)

**A publicized wrapper reaches through an UNNAMED composite RHS to its element type.** A defined type whose `[GoType]` wrapper is emitted `public` (exported, or unexported-but-publicized) exposes its written RHS through the wrapper's `Value`/ctor/indexer/operators, so an unexported RHS type must be publicized too. This holds not just for a NAMED RHS (`type EncoderBuffer encoder`) but for an UNNAMED composite RHS whose ELEMENT is an unexported named type: `type ringElement [256]fieldElement` exposes `fieldElement` through the array-wrapper's indexer/`Value`/`ToSpan`, so `fieldElement` must be publicized (crypto/internal/mlkem768, CS0050/CS0051/CS0053/CS0054/CS0056/CS0057). `collectPublicizedWrapperRHS` therefore feeds the RHS unconditionally to the pointer/slice/array/map/chan-peeling walk (`collectUnexportedNamedTypes`) rather than gating on a named RHS. The walk has no `*types.Struct` case, so a struct RHS stays a no-op — an exported field of an unexported struct-field type is the CS0052 domain and is intentionally left internal. (Guarded by the `NamedArrayWrapper` extension — an exported `Grid [3]unit` over an unexported `unit`, output vs Go.)

**A package-level literal struct reached only as a VALUE publicizes its exported fields' types.** The
walks above read TYPES, so an anonymous struct built inside a package-level var initializer and held only
behind an interface is reachable through none of them — yaml.v3's decode_test.go:

```go
var unmarshalTests = []struct{ data string; value interface{} }{
	{"a: 1\nb: 2\nc: 3\n", &struct {
		A int
		C inlineB `yaml:",inline"`
	}{1, inlineB{2, inlineC{3}}}},
}
```

The struct lifts under the placeholder name `Δtype`, which is public (`generatedTypeScope`: an anonymous
lift carries no export status, and a lifted interface must stay public as a public interface's base,
CS0061), so its exported field `C` held the internal `inlineB` — CS0052, with CS0050/CS0051 on the
members go2cs-gen builds from it. `collectPackageLevelLiteralStructFieldTypes` walks every package-level
var initializer's composite literals before the type walks run, and publicizes the unexported named types
of each anonymous struct's EXPORTED fields: the rule `collectPublicizedLiftedType` already applies to a
publicized lift's fields, applied to the lifts that are public by name. Func-literal bodies are skipped
(their lifts are pinned `internal`), and an unexported field keeps an internal type. A type declared in
PRODUCTION code and reached this way only from a `_test.go` literal is not covered: the `-tests` variant
does not regenerate production output. (Guarded by `packageLevelLiteralLiftAccess_test.go`; an AST census
finds no such field in GOROOT production code, so the standard library does not move.)

## A test-file exported helper over an unexported PRODUCTION type is emitted `internal` (the MIRROR)
The publicization passes above run over a single `*types.Package` and raise a production type's
accessibility to match the exported production surface that exposes it. A `_test.go`-declared helper
is the **mirror** case and needs the *opposite* resolution. In the `-tests` pipeline the production
sources are converted first and independently (no test files), so an unexported production type is
already emitted `internal` on disk; the test files are converted afterward. Go's `strconv/internal_test.go`
declares an EXPORTED helper returning that internal production type:

```go
// internal_test.go (package strconv) — exports access to strconv internals for tests
func NewDecimal(i uint64) *decimal { … }   // decimal is package-private, emitted `internal`
```

Emitting `NewDecimal` `public` (its capitalized name) makes it a public method whose result is the
less-accessible internal `decimal` — CS0050. Publicizing `decimal` is **not** the fix here: production
was already emitted (and is not re-emitted in the test pass), so the map entry would be inert, and a
public API surface for a test-only helper is semantically wrong. In the recompile test model the test
assembly is self-contained (production + internal-`package strconv` + external-`package strconv_test`
files all compile into ONE assembly, with no cross-assembly consumer of a test symbol), so the correct
and sufficient resolution is to downgrade the helper to `internal`:

```csharp
internal static ж<@decimal> NewDecimal(uint64 i) { … }   // was public → CS0050; internal ≤ any prod access
```

`visitFuncDecl` downgrades an exported free function (`Recv == nil`) declared in a `_test.go` file when
its signature references, in any param/result position (peeling pointer/slice/array/map/chan),
an unexported same-package named type **declared in a production file**
(`signatureReferencesUnexportedProductionType`). The production-file restriction is essential and is
what distinguishes this from `sort`'s `example_multi_test.go`, whose exported `OrderedBy(...) *multiSorter`
returns an unexported type declared **in a test file**: that type is publicized AND re-emitted `public`
within the same test pass (the framework above), so its referrer compiles as public and must **not**
be flipped. Only a production-declared unexported type stays `internal`-on-disk and forces the
downgrade. The change fires solely inside `_test.go` conversion, so normal-path output is byte-identical
(check-no-regression clean across the behavioral corpus) and no already-validated package drifts
(`sort` re-validates 63/63, `SetOptimize(bool) bool` in the same file stays `public`). This is the
blocker that lets `strconv`'s test host reach compilation of its file-reading suites (`TestFp`/`TestAtof`
read `testdata/testfp.txt` via `os.Open` + `bufio.Scanner`). No behavioral guard is expressible — the
`-tests` recompile model has no normal-path analogue — so the guard is the `strconv` pipeline (its
`internal_test.cs` emits `NewDecimal` `internal`; the CS0050 no longer blocks).

**The same downgrade applies to a package-level VAR or CONST — the CS0052 half of the rule.** A
white-box test file's exported *value* faces the identical arithmetic on a *field* rather than a
method: `internal/cpu`'s `export_test.go` declares `var Options = options` over the production
`type option struct{…}`, and a `public` field of type `slice<option>` is **CS0052 — inconsistent
accessibility**, the field's type being less accessible than the field. `visitValueSpec` runs every
package-level var/const access through `testDeclaredValueAccess`, which applies exactly the
predicate the func rule uses (`typeReferencesUnexportedProductionNamed`, peeling
pointer/slice/array/map/chan) and downgrades to `internal` on a hit; the production-file restriction
and the self-contained-assembly reasoning carry over unchanged.

This half only became reachable when the white-box **bridge class** started carrying an access
modifier. Before
[the unconditional bridge metadata unit](shadowing.md#test-suites-reference-the-production-project-instead-of-recompiling-it), an internal
test file's `partial class cpu_internal_test_package {` was the class's ONLY declaration, and a
top-level C# class with no modifier is `internal` — so its `public` members were internal *in
effect* and the inconsistency never arose. Making the bridge `public static partial` (which a
record-less mixed suite needs, or an extension method in an internal test file is CS1106) exposed
every such field at once. `internal/cpu`'s whole 8-verdict suite sat behind the one line.
(Guarded by `TestExportedTestFileVarOverProductionTypeIsDowngraded`, with three negative controls:
an exported production element type, a test-file-declared element type — which the publicize pass
re-emits `public` in this same pass — and a production-declared exported var over the same
unexported type, which stays `public` because the gate is the declaring FILE, not the type.)

## A FUNCTION-LOCAL type is emitted `internal` — its Go name's case carries no export meaning
Both rules above read an access modifier out of an identifier's first rune, which is exactly what Go's
export convention licenses — **for a package-level identifier**. A type declared *inside a function
body* is a different animal: it is unreachable from outside that function by construction, so Go
draws no visibility distinction between `S8` and `embed2` there. Neither is exported; neither can be.

go2cs hoists such a type to package scope under a `<Func>_<name>` identifier (see the lift sections),
and that hoist is where the meaning gets invented. Go's `encoding/json` `decode_test.go` is the
witness — one function, two local types, one a field of the other:

```go
func TestUnmarshalEmbeddedUnexported(t *testing.T) {
	type embed2 struct{ Q int }
	type S8 struct {
		embed2
		R int
	}
	…
}
```

The white-box bridge arm asked `generatedTypeScope` for the **local** name, so the two siblings landed
on opposite sides:

```csharp
[GoType("dyn")] [GoLocalName("embed2")] internal partial struct TestUnmarshalEmbeddedUnexported_embed2 { … }
[GoType("dyn")] [GoLocalName("S8")]     public   partial struct TestUnmarshalEmbeddedUnexported_S8 {
    public TestUnmarshalEmbeddedUnexported_embed2 embed2;   // CS0053 — less accessible than the property
}
```

and a lifted **anonymous** struct, which carries no modifier at all, was scoped by go2cs-gen's own
rule from the **hoisted** name — inheriting the case of the enclosing function, so
`TestEncoderSetEscapeHTML_type` came out `public` and its exported fields over the package-level
unexported `strMarshaler` were CS0052.

`localTypeAccess` (`typeAccessibilityOperations.go`) resolves both by emitting a function-local type
`internal`, consumed at the three points that finalize a modifier — `visitTypeSpec`'s bridge arm, and
the lift defaults in `visitStructType` and `visitInterfaceType`. `internal` is faithful (no Go
consumer outside the function can name the type) and sufficient (every emitted C# consumer — the
hoisted siblings and the converted function body — compiles into the same test assembly). Writing it
**inline** is load-bearing: go2cs-gen reproduces a modifier the declaration already carries and falls
back to its name rule only for a bare declaration, so pinning it inline is what stops the generator
from re-deriving `public` and colliding (CS0262). This was the entire compile wall of `encoding/json`'s
suite — **76 errors across CS0050/CS0051/CS0052/CS0053, four codes, one cause**.

The rule is scoped to the bridge arm. On the production path the modifier is left empty and
`recordTypeAccessibility` pins `generatedTypeScope` of the **mangled** name, which gives every local
type of one function the same modifier — uniform, and therefore consistent, though for a reason
nobody chose. The same latent mixture is expressible there (a function-local struct with an exported
field of a package-level unexported type); no corpus package presents it, and flipping production
local types would move a public value adapter's operand out from under it, so it is recorded rather
than pre-emptively changed. Guarded by `TestFunctionLocalTypesShareOneAccessibility`, which pins all
three shapes — the uppercase local, the lowercase local, and the anonymous lift reaching a
package-level unexported production type — and fails without the fix.

## A publicized unexported interface is emitted `public`
The accessibility pass records an unexported **interface** used in an exported surface exactly like a
struct or func type — testing's `type testDeps interface { … }` reached through `func MainStart(deps
testDeps, …) *M` is interned into `packagePublicizedTypes`, and `visitTypeSpec` sets
`pendingTypeAccess = "public "`. But on the EMISSION side, every top-level type-kind emitter consumes
`v.pendingTypeAccess` (struct, array, map, ident, the inline selector/star cases) *except*
`visitInterfaceType`, which dropped it — so the interface always emitted `[GoType] partial interface
testDeps`, defaulting to C# `internal`, less accessible than the `public` member that references it
(CS0051). `visitInterfaceType` now reads-and-clears `pendingTypeAccess` at entry (so the lifted/anonymous
interfaces it visits recursively see an empty value) and folds the modifier into the post-attribute slot,
emitting `[GoType] public partial interface testDeps`. Non-publicized interfaces are unchanged (no churn).
(Guarded by the `PublicizedInterfaceParam` behavioral test — an exported function taking an unexported
interface whose method returns a built-in type, output-compared vs Go.) The **transitive** cascade also
walks a publicized interface's method signatures: the `collectMethodSignatureUnexportedTypes` fixpoint step
walked a type's `named.NumMethods()` (declared receiver methods) but that is **0 for a defined interface**
— an interface's methods live on its underlying `*types.Interface`. It now also iterates
`iface.NumMethods()` for a publicized interface, so an unexported NAMED type in a public interface member's
parameter/result signature is publicized in turn (CS0051/CS0050).

**…and an interface member is public whether or not the GO method is exported (2026-08-08).** That
walk still ran each method through a gate that returned early on `!method.Exported()`. The gate is
right for a CONCRETE method — an unexported one emits `internal static … sockaddr(this
ж<SockaddrInet4> …)` and exposes nothing — and wrong for an interface member, which
`visitInterfaceType` emits with **no access modifier** and which C# therefore makes implicitly
**public**. Go's case convention simply does not survive into the emitted surface, so it is the
EMITTED C# accessibility, not the Go exportedness, that decides what must be lifted; the gate now
takes an explicit flag, set only on the interface arm. syscall's `Sockaddr` is the archetype and the
idiom is deliberate Go: `sockaddr() (unsafe.Pointer, _Socklen, error)` is unexported precisely so
that only the package can implement the interface — a SEALED interface — yet the emitted member
returns the unexported `_Socklen` from a public interface (**CS0050** on every unix flavor; Windows
spells the same method with `int32`, which is why the corpus never saw it). `go/types` is the other
reached case, and a subtler one: its exported `Object` interface has `color() color` and
`setColor(color)`, and the wrapper only ever compiled because the type's `Δ` collision-rename made
`TypeGenerator`'s name-based scope rule read the leading Greek capital as exported and emit it
`public` by accident. It is now publicized on purpose. (Guarded by
`typeAccessibilityInterface_test.go`, whose negative controls fail if the gate is dropped outright
rather than narrowed — a concrete unexported method must still publicize nothing.)

A public callable's signature can also reference a **lifted anonymous** type, which the NAMED-only cascade
above cannot reach — testing's `testDeps.CoordinateFuzzing(… corpusEntry …)` / `RunFuzzWorker` / `ReadCorpus`,
where `type corpusEntry = struct{…}` is an ALIAS to an anonymous struct. The signature type is not a
`*types.Named` but a lift (`corpusEntryᴛ1`), a synthesized name over a raw `types.Type` with no
`*types.Object`, so `packagePublicizedTypes` (keyed by object) cannot hold it. A parallel set
`packagePublicizedLiftedTypes` (keyed by the alias-stripped anonymous `types.Type`) fills the gap: a
SIGNATURE-context walker `collectSignatureTypes` — used by the exported-func, exported named-func-type, and
method/interface-method signature paths — records any lifted anonymous struct/interface it reaches, and the
lift emission in `visitStructType` consults `isPublicizedLiftedType` and emits `public`. This is deliberately
**signature-scoped** and does *not* fold into the shared named-only `collectUnexportedNamedTypes`: an exported
**field/var** of an anonymous struct is the CS0052 domain (a public struct/var over an internal anon field
type is legal while its own enclosing type is internal), so only signature positions lift — keeping golden
churn to the one genuinely-affected shape. (Guarded by the `PublicizedInterfaceAnonAlias` behavioral test — an
unexported interface publicized through an exported function, whose method both takes and returns a
`type = struct{…}` alias, output-compared vs Go; it fails to compile with CS0050/CS0051 without the lift
publicize.)

The walker records lifted anonymous INTERFACES as well as structs, but only the struct emitter consulted the set,
so an exported func taking an anonymous interface emitted its lift `internal` beside the `public` method: go-cmp's
`func Reporter(r interface{ PushStep(PathStep); Report(Result); PopStep() }) Option` was CS0051 in production.
`visitInterfaceType` now consults `isPublicizedLiftedType` too, ahead of the function-local `internal` default:

<!-- source: src/tests/Behavioral/AnonInterfaceParamPublic/AnonInterfaceParamPublic.cs.target:19 -->
```csharp
public partial interface Report_r /*dyn*/ {
```

An unexported func's anonymous interface keeps `internal`. The census of the shape (a non-empty anonymous
interface reached by the walker) read 0 in the converted standard library on three targets and 0 across 757
behavioral modules. (Guarded by `anonInterfaceParamPublic_test.go` and the `AnonInterfaceParamPublic` behavioral
test, which fails to compile with CS0051 on the pre-change converter.)

## Publicized unexported types make their exported methods public
An unexported Go type reachable through an exported surface (an exported var — `var BigEndian
bigEndian` — an exported field, or an exported function's signature) is emitted `public`
(`packagePublicizedTypes`, CS0052/CS0050). Its **exported methods** must then be public too —
Go callers hold such values through the exported var and call the methods cross-package, but the
receiver-based access rule alone rendered them `internal` (extension methods invisible outside
the assembly: `binary.BigEndian.Uint32(...)` CS1061). The receiver-access checks in
`visitFuncDecl` treat a publicized receiver as public, and `collectPublicizedTypes` **cascades**
through the publicized types' exported method signatures to a fixpoint (a newly public method's
unexported parameter/result types get publicized in turn, or the public method would be CS0050).
Unexported methods stay `internal` regardless.

## Structural interface satisfaction emits C# interface inheritance
Go converts `fs.File` to `io.Reader` implicitly because the method set suffices; C# interfaces
are nominal. When a declared interface's method set **strictly contains** an EXPORTED method
interface from a **directly imported** package (checked with `types.Implements`), the converter
emits real C# inheritance at the declaration and **skips re-declaring the covered members**
(redeclaring would HIDE the base member — implementers would need both):

```csharp
[GoType] partial interface File :
    io_package.ReadCloser
{
    (FileInfo, error) Stat();
}
```

Every downstream interface-to-interface conversion then becomes an implicit reference
conversion — identity-preserving (the dynamic value flows through type asserts, unlike an
adapter wrapper) and zero-cost (os's `CopyFS` passes an `fs.File` to `io.Copy`, CS1503).
Details: only the **minimal covering set** is listed (`ReadCloser` subsumes `Reader`/`Closer`);
the strict-subset guard rules out inheritance cycles (equal method sets never inherit);
candidates covered by a declared **embed** are skipped (the embed emission handles those);
bases reference the **file-local package alias** (`io.ReadCloser`, user-ruled style) via
getAliasQualifiedTypeName, which also registers the using — needed because the declaring Go file may not
import the candidate's package (`fs.go` declares `File` without importing `io`); lifted/dyn
and constraint interfaces are excluded. **Multiple non-subsuming bases sharing a method**
(`CrossPkgLib.Sealed` and `.Rated` both carry `Label`): both are inherited, and the shared
member is **re-declared** — a member covered by exactly one listed base is inherited/skipped,
but one covered by two or more is re-declared so it hides both inherited slots and member
lookup through the derived interface stays unambiguous (CS0121). Go needs only one method to
satisfy all; the C# implementers satisfy every slot with the same public method. Consequently the converter **never records an interface-to-interface
`GoImplement`** — the generator's impl types are structs, and an interface-typed record kills
its whole run. Bounds (banked): candidates come from direct imports only — same-package
structural pairs, the universe `error`, and non-imported-package pairs would still surface as
compile errors and would need the adapter complement. Guarded by `CrossPkgUser` (`namedLabel :
CrossPkgLib_package.Labeled`, passed to `CrossPkgLib.Describe`).

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

## A forwarded multi-value call deconstructs when tuple elements need interface conversion
`return newRawConn(f)` forwards a `(*rawConn, error)` tuple into a `(syscall.RawConn, error)`
result list — C# tuple conversions do not consult user conversions element-wise (CS0266). The
converter deconstructs into temps and converts each element through the usual interface
machinery (which also records the `GoImplement` pairing):

```csharp
var (ᴛ1, ᴛ2) = makeRelay();
return (new relayжReporter(ᴛ1), ᴛ2);
```

Elements whose actual type is itself an interface are left alone (structural inheritance
covers those). Guarded by `CrossPkgUser` (`getReporter` forwarding `makeRelay`).

## A multi-value call spread into a call's parameters in an assignment hoists into temps
Go lets a MULTI-VALUE call fill the parameters of an enclosing call — `r := t.newRange(t.parseControl("range"))`,
where `parseControl` returns five values feeding `newRange`'s five parameters. C# has no splat, so the inner
call is deconstructed into markers and passed expanded:

```csharp
var (ᴛ6, ᴛ7, ᴛ8, ᴛ9, ᴛ10) = Ꮡt.parseControl("range"u8);
var r = Ꮡt.newRange(ᴛ6, ᴛ7, ᴛ8, ᴛ9, ᴛ10);
```

`convExprList` already performs this expansion, but only when the call's `deferredDecls` hoist target is
non-nil — passing the whole tuple as one argument is otherwise CS7036 (text/template/parse's `rangeControl`).
The return-form threads that target (visitReturnStmt); the assignment forms do too, on BOTH lowering
branches: the single-declare block and the mixed/escaping block (a pointer-result local that is heap-boxed is
not counted in `declaredCount`, so it takes the latter — the `newRange` case above). A **statement-level**
`f(g())` (a bare expression statement, not an assignment) carries no `deferredDecls` of its own, so the
expansion now falls back to the enclosing `ExprStmt`'s `v.hoistedDecls` buffer — testing's
`registerCover2(deps.InitRuntimeCoverage())`, where `InitRuntimeCoverage` returns three values:

```csharp
var (ᴛ1, ᴛ2, ᴛ3) = deps.InitRuntimeCoverage();
registerCover2(ᴛ1, ᴛ2, ᴛ3);
```

The hoisted `var (…) = …;` lands in the statement's existing hoist buffer, emitted before the statement.
Byte-identical corpus-wide except where the pattern occurs (and a harmless renumber of any later temps, since
the per-file marker index is monotonic). Guarded by `TupleSpreadIntoCall` (a value result, an escaping pointer
result, and a statement-level spread).

A **PACKAGE-LEVEL var initializer** has no statement sink at all — `var debug = template.Must(
template.New("RPC debug").Parse(debugText))` (net/rpc debug.go; also internal/trace/traceviewer) passed the
whole `(ж<Template>, error)` tuple as `Must`'s one argument (CS7036). There the spill becomes a hidden
once-evaluated static tuple FIELD (`v.globalDeclHoist`, flushed by visitValueSpec before the var's own
field — C# static field initializers run in textual order, the same holder shape `visitPackageTupleVarSpec`
emits for `var a, b = f()`), and the arguments read its components:

```csharp
internal static (nint, nint) tupleᴛ1ʗ = parts();
internal static nint g = combine(tupleᴛ1ʗ.Item1, tupleᴛ1ʗ.Item2);
```

Guarded by the `TupleSpreadIntoCall` extension (a package-level `var` spreading a two-value call into a
wrapping call, value read back in main).

## A range over a pointer-typed type conversion parenthesizes before the deref
Ranging over a pointer to an array implicitly dereferences it — the converter appends `.Value` to the
range expression. When the range expression is itself a pointer-typed TYPE CONVERSION it renders as a C#
cast (`(ж<array<byte>>)(uintptr)(p)`, crypto/internal/nistec's p256 init over
`(*[43*32*2*4][8]byte)(*p256PrecomputedPtr)`). A cast binds LOWER than member access, so a bare append
`(ж<…>)(p).Value` parses as `(ж<…>)((p).Value)` — the deref lands on the operand, not the cast result
(CS1579 "no GetEnumerator" on the box type, CS8130). `visitRangeStmt` now wraps the range expression in
parentheses — `((ж<…>)(p)).Value` — whenever the pointer-unwrap deref is active and `rangeStmt.X` is a
`*ast.CallExpr` whose `Fun` is a type expression (`info.Types[Fun].IsType()`, which catches the
unsafe.Pointer conversions `isTypeConversion` deliberately excludes). Byte-identical corpus-wide (the
pattern only occurs on a pointer-producing conversion in range position, which never compiled before).
Guarded by `RangePointerArrayConversion` (transpile+compile+target only — the exact cast shape needs an
`unsafe.Pointer` source, whose runtime round-trip golib does not reproduce, so it is not output-compared).

## Adapter accessibility: symbol-OR-name on both sides
The adapter class scope cannot be derived from Go name casing alone (`error` is lowercase yet the golib interface is public METADATA - the name rule made io/fs's PathErrorжerror internal, CS0122 x40) nor from symbols alone (sibling generators' `public partial` modifiers are invisible to a single-pass generator - the symbol rule broke same-assembly interfaces like `CrossPkgLib.Reporter`). The ImplementGenerator takes symbol-OR-name on the struct AND the interface.

## GoImplement records de-duplicate at attribute emission
os converts dirEntry to fs.DirEntry both through its own alias (`type DirEntry = fs.DirEntry`) and through the io/fs name - two records for ONE interface made the generator emit the explicit implementation twice (CS8646/CS0111). The de-duplication happens at ATTRIBUTE EMISSION with the ALIASED record winning (its simple name resolves via the package usings); normalizing the RECORD KEY instead was twice wrong - qualified attr names break generator name resolution and flip the alias-locality gate. **Measurement lesson:** those declaration-phase errors had SUPPRESSED all of os's method-body diagnostics (Roslyn phase gating) - a package is not truly measured until its declaration errors are zero.

**The comparison must run on the EMITTED spelling, not the raw registry key (2026-08-08).** The
covered set is built from `exportedTypeAliases`, whose values `visitTypeSpec` has already
canonicalized — it reverts a file-local import rename before recording the alias target — while the
registry key keeps whatever rendering the cast site produced. os aliases its `io` import to `Δio`
(io is shadowed once io/fs is in the reference closure), so on the **unix** flavors
`unixDirent`→`fs.DirEntry` registers as `DirEntry` *and* as `Δio.fs_package.DirEntry`, and **neither**
compares equal to the canonical `io.fs_package.DirEntry` the covered set holds. Both records were
emitted for the one pair and `unixDirentжDirEntry` was composed twice (CS0102, CS0111 ×9, CS8646 ×4).

That also produced a **third** adapter spelling, worth recording because it looks like a separate
defect and is not: two records composing one adapter name is exactly what `adapterNameCollisionSet`
exists to detect, so it saw a FALSE collision, applied its collision-conditional rule and qualified
the foreign side of one cast site to `unixDirentжfs_DirEntry` — a name neither record produces
(CS0246). Removing the duplicate removes the collision, and all three call sites in `file_unix.cs`
converge. The key is now built with `qualifyLocalTypeRef`, the same canonicalization the emission
applies, so the two sides are comparable by construction. Windows is unaffected, and that was
measured rather than argued: its `os` registers the renamed-canonical spelling ALONE, with no
alias-keyed record, so nothing is covered and the qualified record is still the only record.
(Guarded by `implementRecordAliasCanonicalization_test.go`, whose controls pin both the Windows shape
and a genuinely distinct second implementation that must NOT collapse.)

**The interface-inheritance PRUNE exempts pairs that generate their own adapter CLASS.** The same
attribute-emission stage also drops a "lower" GoImplement record when the SAME implementing type is
recorded against a derived interface that C#-inherits it (elf's errorReader against both io.ReadSeeker
and io.Reader — the two value-form partial-struct implementations would implement `Read` twice,
CS0111/CS8646). That prune is only valid for the value-boxing PARTIAL-STRUCT form (one type, one
interface list). A pair whose implementation is a DISTINCT generated adapter class must survive, since
each cast site references the adapter for the EXACT interface it targets — the ж<T> pointer form was
already exempt, and the same now holds for the value-form adapter classes (`<src>ᴠ<iface>`): an
**interface-sourced** conversion (net/http wraps `net.Conn` values as `io.Reader`/`io.Writer` — the
prune dropped both pairs under the also-recorded `Conn→ReadWriteCloser`, so every
`new net_ConnᴠWriter(…)` referenced a class the generator never emitted, CS0246 ×17 in net/http and
recurring in net/rpc and httputil) and a **foreign-struct value** conversion
(`<pkg>_<T>ᴠ<iface>`) are marked at recording time (`adapterClassImplementations` in
`convertToInterfaceType`) and skipped by the prune. (Guarded by `IfaceToIfaceNarrow` — one source
interface converted to a full-surface embedded-interface target AND to its narrower bases at
argument, assignment, and return positions, dispatch output-compared vs Go.)

### A `global::` root escape is a THIRD spelling of one type, and the record sets dedupe on text

The de-duplication above compares rendered attribute lines, so every distinct spelling of one type is
a distinct record. The alias case is the one that section documents; the **root escape** is the same
defect reached by a different route, and it is invisible in a production conversion. `-tests` alone
mints `global::` — `testAliasShadowOperations` / `convSelectorExpr` escape to the root whenever a test
package's own class shadows the leading segment of a qualified reference — so one test package can
register the same (impl, interface) pair from an escaped site and a bare site and emit both records.
go2cs-gen resolves both to the SAME symbol and mints the adapter twice: `net/http`'s
`http_HandlerFuncᴠΔHandler` came out as both `-val.g.cs` and `-val.1.g.cs`, giving CS0102 + CS0111 ×5
+ CS8646 ×2 against a test suite that had never run.

`dedupeRootEscapedRecords` collapses lines that differ only by root escapes, as a shared pass over
**both** record sections — `GoImplement` and `GoImplicitConv` are built by the same
`qualifyLocalTypeRef` rendering over the same registries, so they carry the same exposure and a fix in
one alone would only wait for the other. It runs BEFORE `recordEmittedPointerAdapterPairs`, so the
adapter-naming authority sees the deduplicated set and cannot manufacture the false collision that the
alias case's third spelling came from.

The **escaped** spelling wins a collapse: it is shadow-proof by construction, which is why the
machinery minted it, and keeping the bare form could reintroduce the shadow the escape exists to
defeat. That is the opposite preference from the alias case — there the ALIASED (simple) form wins
because the qualified form breaks generator name resolution — and the two are consistent once stated
as one rule: **keep the spelling that resolves under the most conditions.** A root escape adds
resolution guarantees; a package qualifier removed one. Ties fall to the lexicographically smaller
line so the output stays deterministic. The pass is an exact identity on any record set without an
escape, which is what makes the whole production corpus provably unaffected. (Guarded by
`rootEscapedRecordDedupe_test.go`, whose four cases pin the collapse, the escape-count preference
independent of sort order, a negative control that distinct records all survive, and the
production-inertness identity.)

### The promoted-method twins class is named for the PACKAGE and the pair, not the pair alone

A struct that satisfies an interface member by **promotion** gets an
`internal static class <pkg>ᴛ<struct>ᴛ<iface>ᴛpromoted` of extension twins, because go2cs's runtime method set is built from
extension methods and a promoted method is the one kind of Go method that never became one. The class
sits at NAMESPACE scope — deliberately a sibling of the package class, so its twins cannot intercept a
bare-name call — and it was named for the (struct, interface) pair alone.

That name is unique per package class, and a namespace holds several of them. Go's **internal**
(`package http`) and **external** (`package http_test`) test variants are distinct packages that may
each declare a type of the same name; they emit correctly into distinct classes
(`http_internal_test_package`, `http_test_package`) and share one namespace. `net/http` declares
`dumpConn` in both `requestwrite_test.go` and `transport_test.go`, each promoting `io.Reader` and
`io.Writer`, so `dumpConnᴛReaderᴛpromoted` and `dumpConnᴛWriterᴛpromoted` were each declared twice in
`go.net` — CS0101 ×2. Nothing about the type model is wrong there; only this cargo name was
under-qualified, and adding the owning package makes "two generated files can never collide" true
rather than merely intended.

Qualified **unconditionally**, which is the opposite of the collision-conditional rule the pointer
ADAPTER names follow, and the difference is blast radius rather than principle: an adapter name
appears at thousands of construction sites, whereas no source-level call ever binds a twin (the struct
MEMBER always wins) and the method-set registry discovers them by scanning every non-nested static
class, never by name. The rename is invisible everywhere except in its own declaration.

## Anonymous interfaces used as an adapter target are lifted package-wide

An inline anonymous interface used as a `GoImplement` target — internal/trace's `readBatch(r
interface{io.Reader; io.ByteReader})`, whose concrete `*bufio.Reader` argument is cast to the
inline interface — must resolve to a NAMED C# type on every side, or the raw Go structural
literal is emitted into the `package_info.cs` assembly attribute and into the adapter class
name (`bufio_ReaderжByteReader}` — the stray `}` breaks the C# parse and cascades ~75 syntax
errors across the file). `visitInterfaceType` already lifts the inline interface to a named
type (`readBatch_r`) in the visitor's per-file `liftedTypeMap`, but a cast at a DIFFERENT
file's call site (generation.go) has its own visitor and its own map, so `convertToInterfaceType`
saw only the raw `*types.Interface` and emitted the literal.

The lift is now also recorded in the package-level `packageDynamicTypeNames` registry — for
FUNCTION-scoped lifts too, since a function-parameter anon interface hoists to file level and is
referenced cross-file — exactly as anonymous structs already register (`visitStructType`).
`convertToInterfaceType` resolves an anonymous `*types.Interface` through the same three steps
`dynamicStructTypeName` uses: this file's `liftedTypeMap`, then the shared registry, then a
deferred `«DYNTYPE:…»` marker resolved after the file-visit barrier. The marker survives the
adapter-name composition (`adapterTypeRef`/`valueAdapterTypeRef` skip the simple-name strip when
it is present — the marker resolves as one unit to the already-simple lifted name), and the
`GoImplement` attribute writer resolves or drops it (mirroring the implicit-conversion writer).
`registerDynamicTypeName` keeps the lexically smallest name for a signature so the winner is
well-defined even when several files lift the same shape. Emitted form:
```csharp
// batch.cs (declaring file):
[GoType("dyn")] partial interface readBatch_r : /* io.Reader */ … { … }
// generation.cs (cross-file cast site):
(b, gen, var err) = readBatch(new bufio_ReaderжreadBatch_r(Ꮡr));
// package_info.cs:
[assembly: GoImplement<bufio_package.Reader, readBatch_r>(Pointer = true)]
```
Clears internal/trace's 75-error syntax cascade (the residual CS0315 — a named-numeric wrapper
not satisfying a lifted operator constraint — is a distinct, deeper root). Guarded by
`AnonInterfaceCrossFile` (a two-file package: file A declares `describe(thing interface{ Sizer;
Namer })`, file B casts a concrete `*box` to it — the lifted name must flow into the attribute,
the adapter, and the signature).

## An INITIALIZED var lifts its explicit anonymous declared type too — and a blank name lifts from the GO identifier

`visitValueSpec` lifts a var whose DECLARED type is an anonymous struct/interface literal, but
until 2026-08-09 only on the BODYLESS arm (`var x struct{…}`). Give the same var an
**initializer** and nothing lifted it, so the raw Go text landed in both the declaration type
and the value adapter's class name — and its braces close the C# member, making every following
declaration in the file read as a namespace-level one:

```go
// crypto/ecdh's test half opens with the documented-interface witness idiom:
var _ interface{ Equal(x crypto.PublicKey) bool } = &ecdh.PublicKey{}
```
```csharp
// before — CS1519/CS1002 at the site, then CS0106 on every remaining member, CS1022 at EOF:
internal static interface{Equal(x crypto.PublicKey) bool} _ᴛ1ʗ =
    new ecdhꓸPublicKeyжinterface{Equal(x crypto.PublicKey) bool}(Ꮡ(new ecdhꓸPublicKey(nil)));
// after:
[GoType("dyn")] partial interface _ᴛ1 { bool Equal(cryptoꓸPublicKey x); }
internal static _ᴛ1 _ᴛ1ʗ = new ecdh.ΔPublicKeyж_ᴛ1(Ꮡ(new ecdhꓸPublicKey(nil)));
```

The initialized arm now performs the bodyless arm's lift (both the struct and the interface
twin). Ordering is not a constraint: the adapter name is minted EARLIER in the same iteration by
`convertToInterfaceType`, but as a deferred `«DYNTYPE:…»` marker, so a lift registered afterwards
still resolves it at the file-visit barrier.

The lift is named from the **GO** identifier, not from `csIDName`. For an ordinary name the two
agree (`csIDName` is that name sanitized, and `getUniqueLiftedTypeName` re-sanitizes its
argument), but a BLANK `_` var's `csIDName` is a synthesized temp (`_ᴛ1ʗ`) that exists in no Go
scope — so `getUniqueLiftedTypeName`'s `typeExists` check cannot see it and hands the type the
field's own name back, giving one class a nested type and a field both called `_ᴛ1ʗ` (CS0102).
Passing `_` finds the blank var among the package's defs and bumps the type to `_ᴛ1`, distinct
by construction. (Guarded by the `AnonInterfaceVarWitness` behavioral test — two blank witnesses
over different anonymous interfaces, a NAMED anonymous-interface var that is then called through
its adapter, an anonymous-struct declared type, and a local interface value of the witness type,
output-compared vs Go; and by `crypto/ecdh`'s banked 47-verdict suite, which is where it was
found.)

## A collision-renamed type's pointer adapter composes on the package qualifier, never the whole-type alias

A COLLISION-RENAMED type resolves through a whole-type `global using` alias — `imageꓸRGBA` =
`go.image_package.ΔRGBA`, `ecdhꓸPublicKey` = `go.crypto.ecdh_package.ΔPublicKey` — which is a
single IDENTIFIER, not a qualified path. The adapter is a MEMBER of the declaring package's
class, so composing the adapter infix onto the alias names nothing: `imageꓸRGBAжImage`,
`ecdhꓸPublicKeyж_ᴛ1`, CS0246. The base is rebuilt as the file's package qualifier plus the
type's EMITTED simple name — what the declaring generator composed the class from — giving
`image.ΔRGBAжImage` / `ecdh.ΔPublicKeyж_ᴛ1`.

The FOREIGN-adapter arm carried this rebuild from the start; the SAME-ASSEMBLY arm (the `-tests`
production-under-test package, which compiles into the test assembly) did not, and the
asymmetry was invisible because it only bites a type that is BOTH collision-renamed and
adapted. crypto/ecdh shows both halves side by side: `PrivateKey` is not renamed, renders
`ecdh.PrivateKey`, and composed correctly all along, while `PublicKey` is renamed and did not.
Both arms now share `wholeTypeAliasAdapterBase`, which returns any render that already carries a
qualifier untouched — so it is a no-op for every un-renamed type. (The same-assembly arm is a
`-tests`-only shape, so its guard is `crypto/ecdh`'s banked suite rather than a behavioral
project; the foreign arm's twin is guarded by `CrossPkgUser`.)

## Every type-name render resolves a lifted anonymous struct cross-file

The registry/marker resolution above initially covered only two dedicated call sites
(`dynamicStructTypeName`'s `ж.of(…)` address-of-field form and `convertToInterfaceType`), while
the GENERAL type-name renderers — `getAliasQualifiedTypeName`/`getFullyQualifiedTypeName`, which every other emission
path reaches (heap-box declarations, casts, generic arguments…) — still fell through to raw
`t.String()` Go text on a `liftedTypeMap` miss. So ranging over a package-level anonymous-struct
slice declared in a SIBLING file, with the loop variable escaping to a heap box, stringified the
element type into the box declaration: bytes' `compareTests` (`[]struct{a, b []byte; i int}`,
declared in compare_test.go, ranged from the earlier-sorted bytes_test.go) emitted
`ref var tt = ref heap(new struct{a <>byte; b <>byte; i int}(), …)` — CS1526 plus a ~170-error
parser cascade that blocked all of bytes (Phase-4 blocker B8).

Both renderers now resolve a NON-EMPTY anonymous struct/interface through
`deferredDynamicTypeName` before the `t.String()` fall-through: the shared
`packageDynamicTypeNames` registry (the declaring file may already have been visited — file
visits run in deterministic sorted-file order), else the deferred `«DYNTYPE:…»` marker. The
empty `struct{}`/`interface{}` are excluded — their raw signatures intentionally map to
`EmptyStruct`/`any` downstream. The marker payload is now the HEX-ENCODED signature rather than
the raw text: these general render paths flow through string transformation passes
(`convertToCSTypeName` rewrites every `[`/`]` to `<`/`>`, alias handling splits on `.`) that
would corrupt an embedded raw signature before the post-barrier resolution could match it back
to the registry; hex digits pass through every transform untouched, and the encoding is a pure
function of the signature so equal signatures still render the identical (comparable) string.
Emitted form:

```csharp
// zvars.cs (declaring file, visited AFTER the reference):
[GoType("dyn")] partial struct compareTestsᴛ1 { … }
internal static slice<compareTestsᴛ1> compareTests = …;
// main.cs (cross-file range + heap box):
foreach (var (_, vᴛ1) in compareTests) {
    ref var tt = ref heap(new compareTestsᴛ1(), out var Ꮡtt);
    …
}
```

Guarded by `AnonStructCrossFile` (`zvars.go` declares `compareTests` and sorts after `main.go`,
forcing the marker path; `avars.go` declares `sizeTests` and sorts before it, taking the direct
registry hit — main.go ranges over both with `&tt`/`&st` forcing the heap box, output-compared vs
Go).

## A lifted type name is unique across the PACKAGE, and the `-tests` variant inherits production's

Resolution (above) is one half; **naming** is the other. Every lifted type — an anonymous
struct/interface, or a function-local declaration hoisted out of its body — is emitted as a
**nested type of the single `<pkg>_package` partial class**, so its name has to be unique across
the whole package. The uniquing set was per-FILE, which is a scope narrower than the emission
target: two sibling files whose lifts reach for the same generated name each believed the name
free and both declared it.

Both spellings a lift can start from are exposed to this. An anonymous type with no name of its
own falls back to the generic `type` (rendered `Δtype`, then `Δtypeᴛ1`, `Δtypeᴛ2`, … per
collision), and a function-local declaration is prefixed with the **method name only** — which
sibling files legitimately share, since Go allows one `probe` method per receiver type.
encoding/gob hit both at once: production `type.cs` and the internal-variant `encoder_test.cs`
each lifted a differently-shaped `struct{…}` to `Δtype`/`Δtypeᴛ1`, and the class then carried two
definitions of each — CS0579 on the doubled `[GoType]` attribute plus CS0111/CS0557 on every
member `go2cs-gen`'s `TypeGenerator` emitted for the duplicate (32 errors, the whole package
blocked). Note the failure is **not** avoided when the two anonymous structs happen to be
structurally identical: the second `[GoType("dyn")]` is still a duplicate attribute.

The claim set is therefore package-scoped (`packageLiftedTypeNames`, reset per package/variant),
with two deliberate exemptions:

- A `[module: GoManualConversion]` file **does not claim**. Its emission is redirected to a
  non-compiled `.cs.auto` review sibling, so a claim there would push a real file's type name to a
  higher ordinal for a declaration that never compiles. Those visitors keep the per-file set alone.
- The `-tests` **INTERNAL** variant is pre-seeded with the names the production conversion claimed
  (`productionLiftedTypeNames`). That variant emits its `_test.go` files into the production
  package class while the production `.cs` on disk are **not** regenerated, so those names are
  immutable and the test-side lift is the side that moves — the same production-pinned rule
  `testMethodRenames` applies to declarators and the Tier-C hoist seed applies to literal fields.
  The seed is the production run's live claim set, captured in `convertTestVariants` before the
  first variant's `resetPackageState` (production conversion runs moments earlier in the same
  process). The **EXTERNAL** variant is not seeded: its `<pkg>_test_package` is a separate class
  and may reuse every production name freely.

```csharp
// type.cs (production, pinned):        encoder_test.cs (internal variant, steps around):
[GoType("dyn")] partial struct Δtype {  [GoType("dyn")] partial struct Δtypeᴛ7 {
    internal nint r7;                       internal nint A;
}                                       }
```

Residual: two package-level anonymous structs that are structurally IDENTICAL but declared in
different files still lift to two distinct C# types (one Go type split in two) rather than
sharing one. That combination cannot compile today either — it is the CS0579 case above — so
nothing regressed; unifying them needs the second declaration's *emission* suppressed, not just
its name reused.

Guarded by `AnonStructCrossFile`'s `bvars.go`/`yvars.go` (both manifestations, straddling
`main.go` so file order is exercised in both directions) and, for the `-tests` seed,
`TestTestVariantPinsProductionLiftedTypeNames`.

## Function-literal parameters share the body scope
Go declares parameters in the function block, so a body-level `fpath, err := ...` REUSES a literal's `err` parameter. The variable analysis gives literals ONE merged scope (params + body declarations) mirroring real function declarations; a separate param scope had made the `:=` a shadow declaration beside later reuses (CS0841/CS0128, os CopyFS's WalkDir literal). Guarded by `LambdaFunctions` (`probe`).

## System-colliding local type names are root-qualified in assembly attributes
A Go package can name one of its own exported types after a top-level C# `System` type — internal/profile's `ValueType`, go/ast's `Object`, bytes' `Buffer`. The `GoImplement`/`GoImplicitConv` assembly attributes generated in `package_info.cs` sit at **file scope**, before the `namespace` line, where both `using System;` (a csproj global using) and `using static go.<pkg>_package;` are active — so a bare `ValueType` is ambiguous between `System.ValueType` and the package type (CS0104). The emitter root-qualifies any bare, dotless type name matching a curated set of `System` top-level names at the package class:

```csharp
[assembly: GoImplement<go.@internal.profile_package.ValueType, message>]
[assembly: GoImplicitConv<go.@internal.profile_package.ValueType, ж<go.@internal.profile_package.ValueType>>(Indirect = true)]
```

Foreign types are always package-qualified already (dotted) and are left untouched; no non-colliding name changes, so every non-colliding attribute emits byte-identically. (Guarded by the `SystemCollidingTypeName` behavioral test.)

## A name both `-tests` variant classes declare is qualified with the FILE's anchor class
The same file-scope ambiguity has a second source under `-tests`. The merged test metadata carries a
`using static` for the package under test (`<pkg>_package`) **and** for the external suite
(`<pkg>_test_package`) — the second one added so an attribute argument can name a type the external
test files declare (B3). A Go package is free to declare the same simple type name on both sides,
and encoding/gob does: `Point` and `Vector` are declared by `codec_test.go` (package `gob`) and
again by `example_encdec_test.go` / `example_interface_test.go` (package `gob_test`). The bare
reference then binds to neither — CS0104 ×3, which blocked the whole package build.

Such a name is emitted **class-qualified**, with the class the metadata FILE anchors to — its first
class, which is also what the `go2cs-gen` generators host output in:

```csharp
// package_test_info.cs — anchored at the production class:
[assembly: GoImplement<go.encoding.gob_package.Point, Squarer>]
[assembly: GoImplement<go.encoding.gob_package.Vector, Squarer>]
// package_info_external_test.cs — anchored at the external test class:
[assembly: GoImplement<go.encoding.gob_test_package.Point, Pythagoras>]
```

The anchor is a property of the file, **not** of the variant writing it: the external variant also
merges its production-anchored partition into `package_test_info.cs`, and a bare local reference
there still means the production class — the very invariant the B4/B5 record split already relies on
(`isTestAnchoredImplementRecord`: "a BARE impl name is a type declared in the external test package
itself"). Making the reference explicit states that invariant instead of assuming it. Because the
qualification runs LAST in the name pipeline — after `stripLocalTypeQualifier` — both arrival forms
(bare from the declaring variant, class-qualified then stripped from the other) converge on ONE
canonical spelling, so the merge HashSet still dedupes them to a single record.

The name set is computed once per `-tests` conversion from the two loaded variants and is empty
otherwise, so nothing outside `-tests` changes (check-no-regression: byte-identical across all 495
behavioral projects). Guarded by `TestAmbiguousVariantTypeNamesAreClassQualified`.

## A struct-literal interface field takes a pointer element's adapter
A composite struct literal whose field is an INTERFACE type, initialized with a POINTER element whose pointer-receiver method set satisfies that interface, must record and route the same `*T`→interface adapter a call argument does — `&handlerWriter{l.Handler(), &logLoggerLevel, capturePC}` (log/slog SetDefault), where field `level` is `Leveler` and `*LevelVar` implements Leveler via a pointer-receiver `Level()`. The struct-field interface routing (`checkStructFields`) recorded/routed a NAMED VALUE element that satisfies the field (`DecodingError{InvalidIndexError(idx)}`) but matched only a `*types.Named` element, so a POINTER element fell through: no `GoImplement<LevelVar, Leveler>(Pointer = true)` was recorded, and the box `ᏑlogLoggerLevel` was passed bare to the interface-typed constructor parameter (CS1503). The detection now takes the concrete satisfying type from the element OR the pointee of a POINTER element (`types.Implements` tested on the element's own pointer method set, the non-interface guard tested on the pointee), so a pointer element records and routes exactly like the value case:
```csharp
new handlerWriter(l.Handler(), new LevelVarжLeveler(ᏑlogLoggerLevel), capturePC)
// [assembly: GoImplement<LevelVar, Leveler>(Pointer = true)]  -- in package_info.cs
```
The record flows through the existing pointer-target arm of `convertToInterfaceType` (the `ж<T>`-wrapped name unwraps to `GoImplement<T, Iface>(Pointer = true)`, and the render wraps the box in the generated `TжIface` adapter), so a same-package local (`streamWriter`→`io.Closer` in net/http/fcgi) and a foreign pointee (`*ast.SelectorExpr`→`ast.Expr`, `*Basic`→`Type`, `*Func`→`Object` in go/types) route through their local or foreign adapters uniformly. Positional and keyed literals both resolve their field (a keyed element renders `d: new SettingжDescriber(Ꮡs)`); an already-interface element and a value element are unchanged. (Guarded by the `PointerInterfaceStructField` behavioral test — a pointer-receiver-only implementer placed in an interface-typed struct field, positional via an addressed global and keyed via an addressed local, output-compared vs Go.)

## The struct-field interface routing also fires on an ELIDED element composite
The routing above lived only on the TYPED composite path (`checkStructFields`, reached from
`convCompositeLit`'s `*types.Named`/`*types.Struct` arms). An **elided** element composite — the inner
`{v0, v1, …}` of a `[]struct{…}{…}` / `map[K]struct{…}{…}` / `[N]struct{…}{…}`, which drops the repeated
struct type and resolves it by inference (`compositeLit.Type == nil`) — took the separate target-typed
`new(…)` constructor branch, which emitted its element values through `convExprList` with **no** interface
recording or routing at all. So a struct field of interface type in such a literal was passed bare: a
POINTER form lost its `new TжIface(…)` adapter wrap, and a VALUE form whose concrete was used *only* in the
elided literal (never converted to the interface anywhere else) was never `GoImplement`-recorded, so no
`partial struct T : Iface` was generated for it. Both compile to **CS1503**. This is exactly errors'
`wrap_test`, whose `[]struct{ err error; … }{ {&poser{…}, …}, {errorUncomparable{}, …} }` produced 17
`cannot convert from 'ж<poser>' / 'errorUncomparable' to 'error'` at the `new(…)` sites while the sibling
`multiErr{poser}` slice-element cast (a *different* path) wrapped its `poser` correctly.

The interface-field record+route loop was extracted from `checkStructFields` into a shared
`recordStructFieldInterfaceCasts(compositeLit, structType, callContext)` and is now called from **both** the
typed path and the elided path (against the inferred `*types.Struct`), so an elided struct composite routes
its interface fields identically:
```csharp
new(new poserжerror(poser), err1, true)                      // *poser  → error  (Pointer = true)
new(new errorUncomparableжerror(Ꮡ(new errorUncomparable(nil))), …)  // *errorUncomparable → error
new(new errorUncomparable(nil), …)                            // value form: partial struct : error boxes
// [assembly: GoImplement<poser, error>(Pointer = true)] + <errorUncomparable, error>[(Pointer = true)]
```
The extracted logic is byte-for-byte the proven typed-path logic (same keyed-vs-positional field resolution,
same value/pointer method-set satisfaction test), so it inherits every guard the typed path already carried
(the gif keyed-field bogus-record avoidance, the `types.Implements` pointee test). An isolated A/B
full-reconvert of a production cross-section (fmt, errors, net/http, encoding/json, flag, go/types, os, time,
text/template — 172 `.cs`) shows **zero** production emission change: the pattern is overwhelmingly a
test-code shape, so the fix is inert for ordinary packages and only realizes the previously-uncompilable test
literals. (Guarded by the `ElidedStructInterfaceField` behavioral test — a pointer-receiver `*pointerErr`
and a value-receiver `valueErr`, each used *only* in an elided `[]struct{ err error; … }{…}`, output-compared
vs Go; the pre-fix converter emits the bare box / bare value and fails CS1503 on both.)

**And on the elided POINTER element composite (2026-07-31).** There are *three* composite paths, not
two: `[]*struct{…}{{…}, …}` — Go's shorthand where the `&` is implied — has its own arm in
`convCompositeLit`, reached before the elided-struct arm above and emitting `Ꮡ(new T(…))` rather than
the target-typed `new(…)`. That arm marked `any` field literals but never called
`recordStructFieldInterfaceCasts`, so a concrete element in an interface slot again reached the
generated constructor bare. net `ip_test`'s `[]*struct{ in IP; str string; byt []byte; error }` — an
**embedded** `error` field — handed a `ж<AddrError>` to the `error` parameter with no
`AddrErrorжerror` wrap (CS1503). The arm now makes the same record+route call its two siblings do:

```csharp
Ꮡ(new ipStringTestsᴛ1(new IP(…), "?0123456789abcdef"u8, default!,
    new net_test_package.net_AddrErrorжerror(Ꮡ(new AddrError(Err: …, Addr: …)))))
```

The tell is worth carrying forward: each of the three paths grew its own field-marking sequence
(`markStringFieldLits` / `markAnyFieldLits` / `recordStructFieldInterfaceCasts`) independently, and the
one that fell behind is the one nobody had a failing case for — the same shape-versus-its-pointer-
composition asymmetry as *An anonymous struct lifts from ANY depth of its declared type*. Behavioral
CNR is byte-identical across the whole corpus: like its sibling, this is a test-code shape. (Guarded
by the `ElidedStructInterfaceField` extension — a `[]*struct{ want string; error }` whose interface
field is **embedded**, carrying both a pointer-receiver and a value-receiver implementer.)

## A keyed element's interface target is the composite's own SLOT, never the LHS variable's type
A composite literal assigned to an **interface-typed** variable converts to that interface as a
WHOLE — `visitAssignStmt`'s `convertExprToInterfaceType` (and `visitValueSpec`'s
`convInterfaceDeclValue` for a declaration) wraps the finished literal in its adapter. `convKeyValueExpr`
*also* consulted the LHS variable's type (`context.ident`) for each keyed element, so the interface was
applied a SECOND time, to values whose real slot is not an interface at all. On a map whose element type
is a POINTER that is silently destructive: the element renders correctly as `Ꮡ(new T(…))`, the spurious
`*T → Iface` conversion adds the deref prefix, and `convertToInterfaceType`'s
"`~` of an immediate `Ꮡ(…)`" collapse then hands back the bare struct — a `map[K]*T` slot holding a
VALUE (CS0029).

os's `TestCopyFS` is the reached case: `fsys` is an `fs.FS` and the test *reassigns* it

```go
fsys = fstest.MapFS{"william": {Data: []byte("Shakespeare\n")}}   // map[string]*MapFile
```

which emitted `["william"u8] = new fstest.MapFile(Data: …)` instead of `Ꮡ(new fstest.MapFile(…))`, ×5.
The same literal in a `var` declaration, as a call argument, or assigned to its own concrete type was
always correct — only the reassignment path carried the LHS type down into the elements, which is the
tell that the LHS was never the right source of truth.

The element's target is now the composite's own value slot (`valueSlotType`, already computed for the
`MapSource`/`StructSource` untyped-constant boxing just above), with the LHS ident kept only as the
FALLBACK for a composite that does not state its slot type here — the sparse-array shape it was
originally added for. A struct FIELD of interface type keeps its single conversion through
`structFieldIfaceType`. An interface-VALUED container (`map[K]Iface{k: v}`) still converts every element,
now through the slot rather than the variable, so the two agree by construction. Behavioral CNR is
byte-identical across the corpus — the shape needs a *named* container of pointers reassigned to an
interface variable, which the behavioral corpus did not contain. (Guarded by the
`ElidedPtrElemIfaceAssign` behavioral test: a `map[string]*Item` and a `[]*Item`, each declared into,
reassigned into, and passed into an interface, with a write through a stored element pointer proving the
map holds the same object; plus an interface-VALUED map as the live control for the preserved
conversion.)

## A GoImplicitConv record needs at least one LOCAL operand
`ImplicitConvGenerator` realizes a recorded conversion as a `partial struct <name>` inside THIS package's
class, so the record has to name a type this package declares. The generator already relocates the host
when exactly ONE side is foreign (its "foreign SOURCE via a local alias" / "foreign TARGET via a qualified
reference" arms), and the converter's aliased-numeric arm swaps target and argument for the same reason —
to anchor the record on the local operand. With NEITHER operand local the swap merely picks the other
foreign one and the generator has nothing to extend: it declares `partial struct <simple name>` locally, a
PHANTOM type of that name, and the operator body's `src.Value` does not exist (CS1061).

os reaches it from `os_windows_test.go`'s privilege helper, `syscall.CloseHandle(syscall.Handle(t))` over a
`syscall.Token` — both operands in `syscall`. Both the struct-conversion and the aliased-numeric arms of
`checkForImplicitConversion` now require `conversionRecordHasLocalOperand`, stated once as the property
rather than per-arm. Declining costs nothing: the call site already emits the explicit
`((syscallꓸHandle)(uintptr)t)` cast chain, which needs no generated operator, and an operator between two
foreign types could not be hosted in either of their assemblies from here in any case. Behavioral CNR is
byte-identical. (Guarded by the `ForeignPairNumericConv` behavioral test — a sibling library declaring two
named numerics and never converting between them, converted across in `main`, with the
foreign→local and local→foreign directions as the live controls for the records that are still needed.)

### ...but the POINTER-BOXING route needs none, and a whitebox-production operand still counts
The rule above is about HOSTING, so it stops where hosting does. A record of the form `T` → `ж<T>` —
the shared Go pointer-boxing route, and the corpus's dominant record family at **193 of the 268**
`GoImplicitConv` records across the emitted `package_info.cs` files — hosts nothing at all:
`ж<T>` is golib's generic box, no converted package declares it, and `ImplicitConvGenerator` looks the
target up by struct declaration and `continue`s when it finds none. No host is ever chosen, so no phantom
can be minted and no closed assembly can be mutated. `recordsRequireProductionMutation` already stated
exactly this when deciding whether a white-box test project can keep the reference model; the predicate is
now written once (`pointerBoxConversionRecord`) and both readers share it.

That matters because of the second refinement. On the internal `-tests` variant go/packages merges the
production files into the test package, so a production type's `obj.Pkg()` IS the converted package while
its C# lives in the CLOSED referenced production assembly — which is why `typeDeclaredInConvertedPackage`
subtracts such a declaration (`whiteboxProductionObject`; internal/reflectlite's `flag(typ.Kind())` minted
a phantom `partial struct flag` in the test class, CS1061). Subtracting it for the pointer-boxing route as
well was one notch too far: it silently shrank every white-box package's committed `package_test_info.cs`
on regen. `crypto/rc4` lost its `Cipher` → `ж<Cipher>` record **and** the
`using testing = go.testing_package;` qualifier alias that the same record site registers;
`go/types` lost three (`Basic`, `Interface`, `Tuple`). Nothing catches it: CNR never runs
`-tests`, and the records are inert in the generator, so the only symptom is a `-tests` regen that no
longer reproduces committed bytes.

`conversionRecordHasLocalOperand` therefore takes the record shape as an argument and readmits a
WHITEBOX-PRODUCTION operand — and only that — when the record is the pointer-boxing route. A
BOTH-FOREIGN pair stays declined exactly as the section above describes, which is what keeps the change a
restoration rather than a widening: `go/types`' test conversion also reaches `types.Basic` → `ж<types.Basic>`
and `ast.FuncType` → `ж<ast.FuncType>`, and those must not start recording. (Guarded by
`TestWhiteboxProductionPointerBoxConvStillRecorded`, whose both-foreign arm is the boundary, and
`TestPointerBoxConversionRecordShape` for the shared predicate; the numeric phantom keeps its own guard,
`TestWhiteboxProductionNumericConvNotRecorded`.)

---

[← Struct Type Embedding](struct-embedding.md) · [Index](README.md) · [Pointers →](pointers.md)
<!-- {% endraw %} -->
