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

## Named func types implementing interfaces

- **Named func types implementing interfaces** (flag's `funcValue`): a delegate cannot be a
  partial struct — the generator routes Delegate records to the VALUE adapter
  (`new funcValueᴠValue(v)`), whose Go methods are package extensions binding on the wrapped
  copy; non-struct record kinds SKIP rather than throw (a throw kills the package's entire
  generator run). Guarded by `FirstClassFunctions` (`handler.tag`).

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

## A white-box PRODUCTION↔PRODUCTION pointer pair is already implemented — do not record it again

Under the `whitebox-reference` test model the internal bridge is the SAME Go package as production, so
a `*prodT → prodIface` cast inside an internal `_test.go` reads as local and records its own
`[assembly: GoImplement<T, Iface>(Pointer = true)]`. Production, though, is a **referenced assembly**
that already generated that adapter from its own record — and `InternalsVisibleTo <assembly>.tests`
makes even an unexported adapter class reachable. The duplicate record makes go2cs-gen emit a SECOND
adapter under a test anchor, and that copy resolves its forwarding members in the TEST class's scope:
`context`'s `contains(pc.children, cc)` converts `*cancelCtx`/`*timerCtx` to `canceler` for a map key,
and the duplicate bound `Done` to the unrelated `afterFuncContext.Done` extension (CS1929) while
emitting `cancel` with an **empty body** — a silently degraded override, not merely a build error.

The fix suppresses only the RECORD, which is what makes it small: `resolveAdapterNameMarkers` resolves
a pair that reached no record to the unqualified name it would have had, and that name is production's
own adapter, so the cast site repoints with no other change.

```csharp
!contains((~pc).children, new global::go.context_package.cancelCtxжcanceler(cc))   // production's, not a copy
```

It is gated on production ACTUALLY carrying the pair — its `package_info.cs` is loaded by
`convertTestVariant` into `importedPointerImplements` — never assumed: a pair only the test converts
still needs its local record. Reaching that set also required `canonicalRecordIfaceName` to strip a
leading `global::`, which names no package and never appears in a parsed record; the deliberate
non-collapse it documents is untouched, since a genuinely foreign pair still keys as
`net.http_package.ΔHandler` against the record's `http_package.ΔHandler`. This is the POINTER twin of
the value arm's `whiteboxProductionTarget` carve-out — note the pointer target arrives as a
`*types.Pointer`, so the shared `whiteboxProductionTarget` flag (computed from the unwrapped VALUE
form) is structurally false there and the check must unwrap and ask directly.

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

A struct that satisfies an interface member by **promotion** gets an `internal static class
<pkg>ᴛ<struct>ᴛ<iface>ᴛpromoted` of extension twins, because go2cs's runtime method set is built from
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

## A GoImplement record is gated on the method set actually satisfying the interface
Every `[assembly: GoImplement<T, Iface>]` record makes the `ImplementGenerator` emit implementation glue whose members forward to T's like-named methods — so a record whose Go method set does NOT satisfy the interface generates a forwarder to a method that does not exist. The corpus case: net/http's `err = http2GoAwayError{LastStreamID: …, ErrCode: cc.goAway.ErrCode, …}` — the keyed composite's sparse-array `ident` context leaks the `error`-typed LHS onto each FIELD value, and the `ErrCode` field's value recorded `GoImplement<http2ErrCode, error>` even though `http2ErrCode` has only `String()`/`stringToken()` (its generated `Error() => this.Error()` was CS1929). `convertToInterfaceType` now folds a `types.Implements` check over the recorded form's method set (T for a value record, `*T` for a `ж<T>` record) into `recordableBase`, which gates both the record and the matching adapter-wrapping emissions. A conversion the Go checker admitted always passes the check, so the gate can only drop pairs a caller composed from mismatched types; a type-param-carrying target skips the check (`types.Implements` is undefined for uninstantiated generics, and the open-generic conversion emission must stay). The full-stdlib A/B for this change is exactly one removed line — the false `http2ErrCode` record. (Guarded by the NEGATIVE `KeyedLiteralIfaceAssign` behavioral test: a keyed literal assigned to an `error` variable whose field-value type has `String()` but no `Error()` — a reintroduced record fails the compile phase.)

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

## A colliding pointer-adapter name qualifies its FOREIGN interface side

A pointer-interface adapter class is named `[<pkg>_]<structSimple>ж<ifaceSimple>`. The STRUCT side is
package-qualified when foreign (`bytes_ReaderжReader`), which keeps two same-named foreign structs
adapting to one interface apart. The INTERFACE side had no such treatment — it composed from its bare
last-dot segment — so the mirror-image case collided: ONE struct cast to TWO interfaces whose simple
names match composes one class name twice (`CS0102`, `CS0111` per member, `CS8646`).

`compress/flate` is the case that surfaced it. It declares its own `Reader` (`io.Reader` +
`io.ByteReader`), and its tests hand a `*bufio.Reader` and a `*bytes.Reader` to `NewReader`, which
casts to both that and `io.Reader` — so `bufio_ReaderжReader` and `bytes_ReaderжReader` were each
emitted twice and the package could not build its test host at all. The rule is
**collision-conditional**: only within a group of records composing the same name does the interface
side take a package qualifier (`bufio_Readerжio_Reader`), and the LOCAL member of a group keeps the
bare name (at most one member can be local, so that stays unambiguous and preserves the Go-like short
form). Qualifying unconditionally was measured and rejected — 644 distinct adapter names across 3,688
construction sites would churn. The entire 302-package production corpus contains **no** collisions,
so the rule is byte-neutral there by construction; it takes a test closure's extra casts to make one.

Grouping keys on the whole composed name, struct side included. `compress/gzip` records both
`<Reader, io.Reader>` and `<bufio.Reader, flate.Reader>` — two records whose interfaces share a simple
name but whose struct sides differ, composing `ReaderжReader` and `bufio_ReaderжReader`. Keying on the
interface name alone would call that a collision and rename a validated package's adapters for nothing.

The converter and the generator must agree on every name, and neither may guess, so both read the same
authority: the final `[assembly: GoImplement<…>(Pointer = true)]` lines. That set is not known while
cast sites are being rendered — it is settled only after the whole package is visited and
`writePackageInfoFile` has applied its alias-covered skip and its interface-inheritance prune — so a
cast emits a deferred marker (mirroring the DYNTYPE marker of the anonymous-struct barrier) that
`resolveAdapterNameMarkers` rewrites once the records are final. The marker's payload is hex-encoded
for the same reason DYNTYPE's is: a rendered type name passes through string transformation passes
before reaching the file. Only the INTERFACE side is ever rewritten — the struct side is emitted
verbatim, because it is the reference's *path*, not just a name fragment: rewriting it turned
`new os.FileжWriter(f)` (namespace `os`, adapter class `FileжWriter`, generated in os's own assembly)
into a bare `FileжWriter` that resolves nowhere. (Guarded by `AdapterNameInterfaceCollision` — a local
`Reader` and `io.Reader` reached from one `*src`, verified by reverting the fix: `CS0102` + `CS8646`
on `srcжReader`. Unblocked `compress/flate`'s Phase-4 test host.)

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

## Moved sections

- <a id="an-exported-func-type-publicizes-the-unexported-types-in-its-signature"></a>Moved to [An exported func type publicizes the unexported types in its signature](type-accessibility.md#an-exported-func-type-publicizes-the-unexported-types-in-its-signature).
- <a id="a-test-file-exported-helper-over-an-unexported-production-type-is-emitted-internal-the-mirror"></a>Moved to [A test-file exported helper over an unexported PRODUCTION type is emitted `internal` (the MIRROR)](type-accessibility.md#a-test-file-exported-helper-over-an-unexported-production-type-is-emitted-internal-the-mirror).
- <a id="a-function-local-type-is-emitted-internal--its-go-names-case-carries-no-export-meaning"></a>Moved to [A FUNCTION-LOCAL type is emitted `internal` — its Go name's case carries no export meaning](type-accessibility.md#a-function-local-type-is-emitted-internal--its-go-names-case-carries-no-export-meaning).
- <a id="a-publicized-unexported-interface-is-emitted-public"></a>Moved to [A publicized unexported interface is emitted `public`](type-accessibility.md#a-publicized-unexported-interface-is-emitted-public).
- <a id="publicized-unexported-types-make-their-exported-methods-public"></a>Moved to [Publicized unexported types make their exported methods public](type-accessibility.md#publicized-unexported-types-make-their-exported-methods-public).
- <a id="a-forwarded-multi-value-call-deconstructs-when-tuple-elements-need-interface-conversion"></a>Moved to [A forwarded multi-value call deconstructs when tuple elements need interface conversion](multi-result-and-comma-ok.md#a-forwarded-multi-value-call-deconstructs-when-tuple-elements-need-interface-conversion).
- <a id="a-multi-value-call-spread-into-a-calls-parameters-in-an-assignment-hoists-into-temps"></a>Moved to [A multi-value call spread into a call's parameters in an assignment hoists into temps](multi-result-and-comma-ok.md#a-multi-value-call-spread-into-a-calls-parameters-in-an-assignment-hoists-into-temps).
- <a id="an-initialized-var-lifts-its-explicit-anonymous-declared-type-too--and-a-blank-name-lifts-from-the-go-identifier"></a>Moved to [An INITIALIZED var lifts its explicit anonymous declared type too — and a blank name lifts from the GO identifier](struct-types.md#an-initialized-var-lifts-its-explicit-anonymous-declared-type-too--and-a-blank-name-lifts-from-the-go-identifier).
- <a id="every-type-name-render-resolves-a-lifted-anonymous-struct-cross-file"></a>Moved to [Every type-name render resolves a lifted anonymous struct cross-file](struct-types.md#every-type-name-render-resolves-a-lifted-anonymous-struct-cross-file).
- <a id="a-lifted-type-name-is-unique-across-the-package-and-the--tests-variant-inherits-productions"></a>Moved to [A lifted type name is unique across the PACKAGE, and the `-tests` variant inherits production's](struct-types.md#a-lifted-type-name-is-unique-across-the-package-and-the--tests-variant-inherits-productions).
- <a id="a-promoted-box-receiver-method-through-an-unexported-value-embed-is-called-cross-package-via-a-public-forwarder"></a>Moved to [A promoted box-receiver method through an UNEXPORTED value embed is called cross-package via a public forwarder](struct-embedding.md#a-promoted-box-receiver-method-through-an-unexported-value-embed-is-called-cross-package-via-a-public-forwarder).
- <a id="a-value-embed-promotes-its-pointer-receiver-methods-into-the-outer-pointer-method-set"></a>Moved to [A value embed promotes its pointer-receiver methods into the outer POINTER method set](struct-embedding.md#a-value-embed-promotes-its-pointer-receiver-methods-into-the-outer-pointer-method-set).
- <a id="embedded-pointer-hop-receivers-split-per-method"></a>Moved to [Embedded-pointer hop receivers split per method](struct-embedding.md#embedded-pointer-hop-receivers-split-per-method).
- <a id="with-several-embedded-pointers-the-hop-is-chosen-per-member-not-per-struct"></a>Moved to [With SEVERAL embedded pointers the hop is chosen per member, not per struct](struct-embedding.md#with-several-embedded-pointers-the-hop-is-chosen-per-member-not-per-struct).
- <a id="a-goimplicitconv-record-needs-at-least-one-local-operand"></a>Moved to [A GoImplicitConv record needs at least one LOCAL operand](source-generators.md#a-goimplicitconv-record-needs-at-least-one-local-operand).
- <a id="but-the-pointer-boxing-route-needs-none-and-a-whitebox-production-operand-still-counts"></a>Moved to [...but the POINTER-BOXING route needs none, and a whitebox-production operand still counts](source-generators.md#but-the-pointer-boxing-route-needs-none-and-a-whitebox-production-operand-still-counts).
- <a id="system-colliding-local-type-names-are-root-qualified-in-assembly-attributes"></a>Moved to [System-colliding local type names are root-qualified in assembly attributes](package-conversion.md#system-colliding-local-type-names-are-root-qualified-in-assembly-attributes).

---

[← Struct Type Embedding](struct-embedding.md) · [Index](README.md) · [Pointers →](pointers.md)
<!-- {% endraw %} -->
