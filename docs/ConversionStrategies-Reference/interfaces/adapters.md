# Interfaces: Adapters
<!-- {% raw %} — Jekyll/Liquid guard: this page contains Go composite-literal and template syntax ({{ … }}) that Liquid would otherwise parse; the HTML comment hides the tag on GitHub. -->

[Reference index](../README.md) · [Interfaces](../interfaces.md) · [Summary of this topic](../../ConversionStrategies.md#interfaces)

This page covers the adapter classes that carry a value into an interface: the pointer `ж` adapter, the value adapter and the `ᴠ` interface-to-interface adapter, and how each adapter class is named.

## Pointer, value and interface-to-interface adapters

### Pointer-sourced interface values use a generated ADAPTER, not the value-boxing partial struct

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

### Non-empty interface-to-interface conversions use a forwarding adapter

**Non-empty interface-to-interface conversions use a forwarding adapter.** A Go interface value may be assigned or passed to another non-empty interface when the source interface method set satisfies the target (`var local localLabel = foreign`, where `foreign` is `CrossPkgLib.Labeled`). C# has no structural conversion between unrelated interfaces, so the converter records the interface pair as `[assembly: GoImplement<CrossPkgLib_package.Labeled, localLabel>]` and emits the cast site as a generated adapter:

```csharp
CrossPkgLib.Labeled foreign = new CrossPkgLib.Sensor(Name: "adapter"u8, Temp: 21);
localLabel local = new CrossPkgLib_LabeledᴠlocalLabel(foreign);
fmt.Println(labelOf(new CrossPkgLib_LabeledᴠlocalLabel(foreign)));
```

`ImplementGenerator` emits a sealed adapter implementing the target interface and `IInterfaceAdapter`, stores the source interface value, and forwards each target member to that value. The golib assertion/equality helpers unwrap `IInterfaceAdapter.Value` before type assertions, `Implements<TInterface>`, and `AreEqual`, so the wrapper behaves as an interface view over the original Go interface value rather than a new concrete payload. Guarded by `InterfaceToInterfaceAdapter`, which imports `CrossPkgLib.Labeled`, assigns it to a local compatible interface, passes it as a parameter, and output-compares the forwarded calls.

### Cross-package pointer-to-interface conversions use the foreign adapter
A pointer-sourced cast to an interface implemented by a FOREIGN type references the foreign assembly's PUBLIC adapter class - os's `err = &PathError{...}` emits `new fs.PathErrorжerror(Ꮡ(new PathError(...)))`, io/fs having generated the adapter from its own `GoImplement<PathError, error>(Pointer = true)` record. The record's existence is read from the imported package's package_info (`parseExportedPointerImplements`, the same imported-records pattern as GoTypeAlias). The existence key is the **shared canonical spelling** both sides compose through `implementRecordKey` — `<declaring package>|<C# simple type>|<pkg>_package.<Iface>` — exactly as the value-implement records do; keeping the package CLASS on the interface side is what stops image's `Paletted→image.Image` record from satisfying a `Paletted→draw.Image` cast and referencing the adapter that implements the WRONG interface (CS1503). The reference goes through the file-local package ALIAS (`fs.PathErrorжerror`, user-ruled style) via getAliasQualifiedTypeName, which also registers the using — except when that yields a whole-TYPE alias for a collision-renamed foreign type (`imageꓸRGBA`), which is an identifier and not a path, so the base is rebuilt as the package qualifier plus the type's EMITTED simple name (`image.ΔRGBAжImage`). Guarded by `CrossPkgUser` (`rep = mtr` -> `new CrossPkgLib.MeterжReporter(mtr)`; `&CrossPkgLib.Alarm{}` -> error; and the same-simple-name LOCAL `Labeled` — `var localLb Labeled = sp2` takes the LOCAL `CrossPkgLib_SensorжLabeled`, never the lib's exported `SensorжLabeled`).

An **EXPLICIT pointer-to-interface conversion** — Go's `image.Image(dst)` with `dst *image.RGBA` (image/draw) — is the same interface cast in conversion clothing and routes through the same machinery: `isTypeConversion` probes the ORIGINAL pointer type against an interface target (the value type alone does not implement it — the elem-only probe misread the conversion as a constructor call, `new image.Image(dst)`, CS0144), and the emission re-renders the argument in its BOX form and CASTS the adapter to the interface — `((image.Image)new image_ΔRGBAжImage(Ꮡdst))` — because the adapter implements its members explicitly, and a chained member access on the conversion result (`CrossPkgLib.Labeled(sp).Label()`) cannot bind on the adapter class itself (CS1929). (Guarded by `CrossPkgUser`'s `CrossPkgLib.Labeled(sp2).Label()` / `LabeledOf(sp2)` pair, output-compared vs Go.)

### Cross-package value-to-interface conversions use the local VALUE adapter
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

### Named func types implementing interfaces

- **Named func types implementing interfaces** (flag's `funcValue`): a delegate cannot be a
  partial struct — the generator routes Delegate records to the VALUE adapter
  (`new funcValueᴠValue(v)`), whose Go methods are package extensions binding on the wrapped
  copy; non-struct record kinds SKIP rather than throw (a throw kills the package's entire
  generator run). Guarded by `FirstClassFunctions` (`handler.tag`).

## Adapter naming and accessibility

### A collision-renamed type's pointer adapter composes on the package qualifier, never the whole-type alias

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

### A colliding pointer-adapter name qualifies its FOREIGN interface side

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

### Under `-tests`, a white-box PRODUCTION type is FOREIGN to the generator — so the name carries the prefix
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

### Anonymous interfaces used as an adapter target are lifted package-wide

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

### Adapter accessibility: symbol-OR-name on both sides
The adapter class scope cannot be derived from Go name casing alone (`error` is lowercase yet the golib interface is public METADATA - the name rule made io/fs's PathErrorжerror internal, CS0122 x40) nor from symbols alone (sibling generators' `public partial` modifiers are invisible to a single-pass generator - the symbol rule broke same-assembly interfaces like `CrossPkgLib.Reporter`). The ImplementGenerator takes symbol-OR-name on the struct AND the interface.

### The anchored adapter REFERENCE keeps the shadow marker

The `-tests` metadata-anchored resolution composes the adapter class reference a cast site will use
(`anchoredAdapterMemberName`) while go2cs-gen composes the class it emits. The two must agree
character for character, and they disagreed on the shadow marker: the generator names a local adapter
from `adapterBaseName` — the C# type name verbatim, `Δhandler` — and a foreign one from
`GetSimpleName(structName)`, neither of which strips it, while the reference side stripped it and
named a class that is never emitted. net/http's internal test variant declares
`type handler struct{ i int }` (server_test.go), shadow-renamed to `Δhandler`, so the generator minted
`ΔhandlerжΔHandler` and every cast site referenced `handlerжΔHandler` — CS0426 ×9.

The rule the strip violated: **the marker belongs to the C# IDENTITY of the type, not to a rendering
convention.** `adapterStructKey` strips it for GROUPING, which is right and unchanged — a collision
group must not depend on which side got renamed — but that key must not double as the emitted name.
Only the `-tests` anchored path was affected: a production conversion resolves through
`adapterResolvedName`, which never stripped, so the corpus could not move (and CNR confirms it did
not). The measured shape here also **corrects a plausible-looking diagnosis** worth recording: the
symptom reads as an adapter minted for one test variant and referenced from the other, and it is not
— the record is correctly bridge-anchored in `package_info_internal_test.cs` and the class is minted
in `http_internal_test_package`, exactly where the reference looks for it. Only the NAME differed.

⚠ One adjacent surface is deliberately NOT addressed and is worth naming, since a reader meeting it
will otherwise read it as this defect: a `T` RETURNED out of a constrained generic into concrete code
arrives as the proxy TYPE, whose forwarders are explicit interface implementations and so are
unreachable by member lookup there (`r := second(p); r.Name()` — CS1929, resolving instead to the
element's own extension whose receiver it cannot satisfy). The proxy carries an implicit conversion
back to `ж<element>`, but C# does not apply a user conversion during member lookup. Nothing in the
corpus or in `net/http` reaches it; the guard's `second` builds its result inside the generic context
on purpose.

## Keyword names

### A dynamic interface's runtime conversion class re-escapes a keyword method name
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

### A keyword-named type's interface adapters escape declarations and compose class names unescaped
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

### An interface member's keyword-named PARAMETERS escape in every generated implementation

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

## Composite and field sites

### A struct-literal interface field takes a pointer element's adapter
A composite struct literal whose field is an INTERFACE type, initialized with a POINTER element whose pointer-receiver method set satisfies that interface, must record and route the same `*T`→interface adapter a call argument does — `&handlerWriter{l.Handler(), &logLoggerLevel, capturePC}` (log/slog SetDefault), where field `level` is `Leveler` and `*LevelVar` implements Leveler via a pointer-receiver `Level()`. The struct-field interface routing (`checkStructFields`) recorded/routed a NAMED VALUE element that satisfies the field (`DecodingError{InvalidIndexError(idx)}`) but matched only a `*types.Named` element, so a POINTER element fell through: no `GoImplement<LevelVar, Leveler>(Pointer = true)` was recorded, and the box `ᏑlogLoggerLevel` was passed bare to the interface-typed constructor parameter (CS1503). The detection now takes the concrete satisfying type from the element OR the pointee of a POINTER element (`types.Implements` tested on the element's own pointer method set, the non-interface guard tested on the pointee), so a pointer element records and routes exactly like the value case:
```csharp
new handlerWriter(l.Handler(), new LevelVarжLeveler(ᏑlogLoggerLevel), capturePC)
// [assembly: GoImplement<LevelVar, Leveler>(Pointer = true)]  -- in package_info.cs
```
The record flows through the existing pointer-target arm of `convertToInterfaceType` (the `ж<T>`-wrapped name unwraps to `GoImplement<T, Iface>(Pointer = true)`, and the render wraps the box in the generated `TжIface` adapter), so a same-package local (`streamWriter`→`io.Closer` in net/http/fcgi) and a foreign pointee (`*ast.SelectorExpr`→`ast.Expr`, `*Basic`→`Type`, `*Func`→`Object` in go/types) route through their local or foreign adapters uniformly. Positional and keyed literals both resolve their field (a keyed element renders `d: new SettingжDescriber(Ꮡs)`); an already-interface element and a value element are unchanged. (Guarded by the `PointerInterfaceStructField` behavioral test — a pointer-receiver-only implementer placed in an interface-typed struct field, positional via an addressed global and keyed via an addressed local, output-compared vs Go.)

### The struct-field interface routing also fires on an ELIDED element composite
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

### A keyed element's interface target is the composite's own SLOT, never the LHS variable's type
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

### Pointer-to-interface assignment through selector fields
A selector assignment whose LHS field is an interface (`h.d = s`) uses the type of the **whole selector expression**, not just the selected identifier name, when deciding whether to wrap the RHS in an interface adapter. If the RHS is a pointer-typed identifier, the adapter receives the pointer box so a dereferenced value alias is not copied into a pointer-only implementation. The generated form matches other pointer-to-interface conversion sites:

```go
func assignDescriber(h *holder, s *Setting) {
    h.d = s
}
```
```csharp
internal static void assignDescriber(ж<holder> Ꮡh, ж<Setting> Ꮡs) {
    ref var h = ref Ꮡh.Value;
    ref var s = ref Ꮡs.Value;

    h.d = new SettingжDescriber(Ꮡs);
}
```

This is intentionally keyed on selector/index expression type instead of the root identifier, so struct fields such as `go/types`' `operand.expr ast.Expr` and ordinary behavioral fields both take the same path. Guarded by `PointerInterfaceStructField`, including the assignment case after the struct-literal cases.

---

[← Interfaces](../interfaces.md) · [Index](../README.md)
<!-- {% endraw %} -->
