# Source Generators

[Reference index](README.md) · [Summary of this topic](../ConversionStrategies.md#source-generators)
Several Go semantics cannot be written directly in C#, so the converter emits compact, attributed partial declarations and lets a set of Roslyn source generators (`src/gen/go2cs-gen/`, referenced as an analyzer by every converted project) synthesize the rest at compile time. This keeps the visible converted code close to the Go original. The principal generators and attributes:

* **`TypeGenerator`** — driven by `[GoType]`. Emits the body of a converted type: a struct's members and equality, a named numeric/slice/array/map/channel type's wrapper and operators (see [Named Numeric Types and Constant Contexts](named-numeric-types.md#named-numeric-types-and-constant-contexts) and [Slices and Arrays](slices-and-arrays.md#slices-and-arrays)), and struct-embedding field/method promotion.
* **`ImplementGenerator`** — wires up Go's duck-typed [interfaces](interfaces.md#interfaces): finds the concrete types that satisfy each `[GoType] partial interface` and emits the implementation glue and implicit conversions.
* **`RecvGenerator`** — emits pointer-receiver overloads for receiver methods (`[GoRecv]`), so a method written against a value (`this ref T`) is also callable through the pointer/box form. A **variadic** method keeps its `params` in the generated overload: cryptobyte's `func (b *Builder) add(bytes ...byte)` emits the value form `add(this ref Builder b, params Span<byte> bytesʗp)`, but the `ж<Builder>` overload had dropped `params` (a bare `Span<byte>`), so a call passing individual elements through a box (`c.add(0xff)`, `c` a `ж<Builder>` closure parameter) could not bind it and fell back to the ref-receiver value method — CS1929. `GetMethodInfo` now preserves the `params` modifier (the Go variadic is always the last, non-receiver parameter, so it never lands on the `this ж<T>` receiver). Guarded by `VariadicBoxReceiver` (a `*sink` with `add(bytes ...byte)` called on a box — via a closure and directly — with zero, one, several, and spread arguments, values vs Go).
* **`ImplicitConvGenerator`** — emits the implicit conversion operators that let a [named type](type-aliasing.md#type-definitions) and its underlying types be used interchangeably.
* **`StrGenerator`** — driven by `[GoStr]`. For an [sstring twin](strings.md#an-sstring-twin-a-registered-function-gains-an-sstring-overload-that-calls-bind), emits the `@string` overload that forwards to the `[GoStr]` member under `[OverloadResolutionPriority(-1)]`, and for a package-level function the canonical value delegate `<Name>ᶠ`. Types are rendered fully qualified from the symbols, because the converted file's `using` aliases are not in scope in a generated file.
* **`PartialStubGenerator`** — emits a throwing `partial` implementation for any bodyless `partial` method that has no other implementing part (e.g. assembly/cgo functions with no convertible body), while leaving real hand-written companion implementations untouched.

Common attributes the converter emits for the generators (and tooling) to consume: `[GoType]` (type bodies), `[GoRecv]` (receiver methods), `[GoStr]` (sstring twins), `[GoTag]` (struct field tags), `[GoPackage]` (package info), and the test-only `[GoTestMatchingConsoleOutput]`. The full vocabulary — every stamp, where it lands, who reads it, and which of them are kept off the visible declaration — is classified in [Extended attributes: what stays on the declaration and what moves](#extended-attributes-what-stays-on-the-declaration-and-what-moves).

**A generator's view of ACCESSIBILITY is provisional — its own output is what supplies the access modifier (2026-07-25).** The converter emits a Go type as a bare `[GoType] partial interface X` (or `partial struct X`) nested in the package class and leaves the access modifier to `TypeGenerator`, which derives it from the Go export convention (`GetScope` — `public` for an exported name, `internal` otherwise; an explicit modifier on the converter's part wins). A C# nested type with no modifier is **private**, so until that generated partial exists the declaration is private — accessible from inside its own package class and *inaccessible from any other class in the assembly*. A generator cannot see its own output, so a semantic query that crosses package classes sees the provisional accessibility, not the real one.

That bit io's external test package, and it is the first shape in the corpus that can hit it: `io_test` and `io` compile into ONE assembly (the recompile test-project model), as two classes. `io_test.closer : io.Closer` and `io_test.testMultiWriter_sink : io.Writer` bound their base to an `IErrorTypeSymbol` (`CandidateReason.Inaccessible`, candidate `go.io_package.Closer/Interface/Private`) with **zero members**, so every method the base contributes silently vanished from the generated interface shell and adapters — while the FINAL compilation, which does have the generated `public partial interface Closer`, still demanded them: four distinct `CS0535` (six sites) across `Δcloser<T>`, `ΔcloserᴛObj`, `PipeReaderжcloser`, `PipeWriterжcloser`, `ΔtestMultiWriter_sink<T>` and `bytes_BufferжtestMultiWriter_sink`. The same `io.Closer` resolved normally as a base of `io.ReadCloser` *inside* `io_package`, and `fmt.Stringer` resolved normally from a referenced assembly (already `public` in metadata) — which is what made the failure look spelling-related. It is not: writing the base `global::go.io_package.Closer` fails identically.

`Common.GetAllBaseInterfaces` replaces the raw `AllInterfaces` walk in `InterfaceDeclarationSyntaxExtensions.GetInterfaceMethods` and in both of `ImplementGenerator`'s method-collection paths. It recovers an `Inaccessible` base from the error symbol's `CandidateSymbols` when the candidate is an interface declared in **this compilation's assembly**, and folds in that recovered base's own transitive bases (`AllInterfaces` cannot traverse through an error symbol). The recovery is sound rather than a bypass: the generator is about to declare that very type `public` or `internal`, both reachable from anywhere in the assembly. A genuinely inaccessible *foreign* type keeps its error symbol.

The deeper alternative — having the converter emit the access modifier on its own partial, so the pre-generation source stops understating it — would remove the whole class of provisional-accessibility blind spots, but writing that modifier onto the *inline* `[GoType]` declaration re-baselines every converted `.cs` in the corpus and in ~490 behavioral goldens, and coarsens the Go-shaped declaration the converter works to keep readable. It was left on the table here and taken the following day in the form below. (No behavioral guard is possible for this row: one behavioral project is one Go package is one C# assembly, so two package classes never share an assembly there. The reproducer is the `-tests` pipeline on `io`, whose six `CS0535` clear; gated by the full behavioral suite 490/490, the 302-package corpus build, and the bytes/strings/encoding/binary/strconv pipeline canaries all at banked counts.)

## `package_info.cs`'s `TypeAccessibility` section pins each type's accessibility IN SOURCE

**Resolved (2026-07-25).** The blind spot above is closed at the root, without touching a single converted `.cs`: `package_info.cs` gains a **`TypeAccessibility`** section, emitted **inside the package class body** (its entries are type declarations, and the types they name are nested in that class), carrying one condensed single-line partial declaration per converter-emitted `[GoType]` type:

```csharp
[GoPackage("io")]
public static partial class io_package
{
    // <TypeAccessibility>
    internal partial struct discard {}
    internal partial struct nopCloser {}
    public partial interface Closer {}
    public partial interface Reader {}
    public partial struct LimitedReader {}
    // </TypeAccessibility>
}
```

C# lets a partial type carry its access modifier on any **one** of its parts, so the inline `[GoType] partial interface Closer` stays bare and Go-shaped while this part fixes the accessibility — and it is fixed in **source**, so a generator's semantic query across package classes sees `public`/`internal` instead of the provisional `private`. The section carries its own explanatory prose in the style of the file's other sections; a package info file written before the section existed has the prose and markers inserted on the next conversion (`ensureTypeAccessibilitySection`), so no migration step is needed.

Details that make it a pure relocation of the modifier rather than a change of it:

* **The rule is `TypeGenerator`'s, mirrored exactly.** `recordTypeAccessibility` uses the explicit modifier the converter emitted inline (the publicization pre-pass's `public`) when there is one, and otherwise `generatedTypeScope`, a Go-side mirror of `Common.GetScope`. Deliberately **not** `getAccess`: `GetScope` reads the C# identifier verbatim, so a `Δ` collision-rename (a Greek capital) reads as exported where `getAccess` strips the prefix first. Mirroring the generator is what keeps the corpus's effective accessibility byte-for-byte unchanged and avoids `CS0262` between the two parts.
* **Generic types repeat the type-parameter list, never the constraints** (`internal partial struct entry<K, V> {}`) — a partial declaration may leave constraints to another part. An arity-0 *constraint* interface repeats the CRTP `<ΔT>` marker list the inline declaration carries.
* **Every partial-emitting kind is covered:** struct (defined struct, map, channel, slice/array, defined-over-selector), class (named pointer types), interface. A named **func** type is emitted as a C# `delegate`, which is not partial and already carries its own modifier, so it has no entry.
* **Hand-owned files are deliberately excluded.** A file whose destination `.cs` carries `[module: go.GoManualConversion]` has its converter output redirected to the non-compiled `.cs.auto` sibling, so the declarations that actually compile are the author's — their kind, name and modifier are the author's to choose, and a generated entry could contradict them (`CS0261`/`CS0262`) or conjure a phantom empty type. Across the 302-package corpus that leaves 19 marked files, of which 12 declare `[GoType]` types (~48 declarations, mostly `runtime/runtime2.cs`) — and exactly **one** interface among them, `sync.Locker` in the hand-rewritten `sync/mutex.cs`. That single declaration is why **`Common.GetAllBaseInterfaces`'s `Inaccessible` recovery is RETAINED** rather than retired: it is now a narrow backstop for hand-owned declarations instead of the primary mechanism. Writing the modifier explicitly on a hand-owned declaration removes the need for it there.
* **Test variants get the same treatment, split by owning class.** Under the recompile test model the production entries reach `package_test_info.cs` through the verbatim seed from `package_info.cs`, the internal variant's test-local types merge into that same (production-class) section, and the **external** variant's types — which live in `<pkg>_test_package` — go to `package_info_external_test.cs`, whose first and only class is the test class. `writeExternalVariantMetadata` clears the accessibility set before writing `package_test_info.cs` so a test-class type can never land in the production class, and the unit is now written whenever the external variant declares types, not only when it records test-anchored `GoImplement`/`GoImplicitConv` attributes. Under ordinary reference the test class is the anchor; under white-box reference the external test class is preferred, with the internal bridge used when no external variant exists.
* **The generator still writes the modifier on its own part** — the section is a third part with the *same* modifier, which C# accepts. Removing it is not safe while the section deliberately does not cover hand-owned declarations: those parts would become `private`.
* **Generator-side consequence:** a converted type now has two converter-written parts, so any lookup that resolves a type by **name** through the syntax trees can land on either. `Compilation.FindStructDeclaration` and `ImplicitConvGenerator.GetStructDeclaration` — both of which go on to read members or the `[GoType]` definition token — now prefer the part carrying `[GoType]` (`Common.IsGoTypeDefinition`), falling back to first-match. Without that, syntax-tree order (i.e. compile-item order, where `package_info.cs` sorts ahead of the package sources in roughly half the corpus) would silently decide.

Measured, with a positive control (the `-tests` pipeline on `io`, whose `CS0535` cluster is the reproducer): recovery **on** + section on → 0 `CS0535`; recovery **neutered** + section on → 0; recovery neutered + section **off** → the cluster returns. Gates: full behavioral suite 490/490 across all four phases (460 output-compared, 30 skipped) with **every** main `.cs` golden byte-identical — the churn is 490/490 `package_info.cs`, additions only; seeded 305-package reconvert (14/14 `.cs.auto`, no marked file clobbered) + overlay + full corpus build 0 errors; converter `go test` and `GenTests` green; pipeline canaries at banked counts (errors 61, encoding/csv 71, io/fs 18, bytes 81 with 7 disclosed).

## Extended attributes: what stays on the declaration and what moves

The `[GoType]` declaration is the line a reader of converted code actually reads, so every *other* attribute stamped on it is machinery competing with the Go original for that reader's attention. `package_info.cs` already exists to hold per-type records out of view, and the `TypeAccessibility` section above already moved the access modifier there. **A stamp can follow it whenever its consumer reads the attribute off the TYPE rather than off a particular declaration** — C# unions the attributes of every part of a partial type, so which part carries one is invisible to runtime reflection and to any generator that resolves the symbol.

That single criterion classifies the whole surface. The converter stamps nothing from the BCL — every `[StructLayout]`, `[MethodImpl]` or `[LibraryImport]` in the corpus is in `golib` or in a hand-owned file — so the vocabulary is exactly this:

| Stamp | Lands on | Consumer | Verdict |
|---|---|---|---|
| `[GoType]`, `[GoType("dyn")]`, `[GoType("num:…")]`, … | struct / class / interface | `TypeGenerator`'s syntax receiver keys on it; also read semantically and at runtime | **Must stay** — it is the declaration's identity, and the receiver has no type to resolve until it matches |
| `[GoValueClone("f1", "f2")]` | struct | `TypeGenerator`, reading field names to emit `Clone()` | **Moved** |
| `[GoLocalName("Point")]` | struct (lifted function-local named type) | golib's reflection bridge, `GoReflect.TypeNaming` | **Moved** |
| `[GoTag("json:\"x\"")]` | **field** | golib reflection, via the `DescriptionAttribute` alias | **Must stay** — field-level. A `<TypeAccessibility>` record is an empty `{}` body; C# has no way for a second part to re-declare a field and attach an attribute to it |
| `[GoRecv]` | **method** | `RecvGenerator` syntactically, plus runtime | **Must stay** — same reason, one level up: a method exists on the part that defines its body |
| `[GoStr]` | **method** | `StrGenerator` syntactically | **Must stay** — as `[GoRecv]`: it marks the member whose signature the generator reads |
| `[GoArrayDims(4, 8)]` | **parameter** | golib reflection — `GoReflect.FuncParamDims` off the delegate instance's `Method.GetParameters()`, or `MethodParamDims` off the method table's `ParameterInfo`s | **Must stay** — the sharpest case in the set: the datum is not type-keyed at all. It distinguishes two funcs that share one emitted delegate type (`func([32]byte) bool` and `func([64]byte) bool` are both `Func<array<byte>, bool>`), so a record keyed by type has nothing to key on, and the consumer reads the parameter's own metadata |
| `[GoInit]` | **method** | the **C# compiler** — it is a `using` alias for `ModuleInitializerAttribute` | **Must stay** — the compiler requires it on the method it initializes with |
| `[GoPackage]`, `[GoImplement<T,I>]`, `[GoImplicitConv<S,T>]`, `[GoTypeAlias]`, `[GoSStringTwin]` | package class / assembly | generators, runtime, and the converter's own next run | **Already there** — these are emitted into `package_info.cs` and never touched a mainline declaration |
| `[GoManualConversion]`, `[GoRequiresUnsafe]` | module | the converter | **Already off** — hand-written, module-scoped |
| `[GoInterfaceShell]`, `[GoTwinForwarder]`, `[GoReflectCompanion]` | interface / lambda / field | golib | **Not converter-emitted** — the first two written by the generators (`[GoTwinForwarder]` by `StrGenerator`, on a twin delegate's lambda), the last by hand |

So the movable set is `[GoValueClone]` and `[GoLocalName]`, and both moved. `[GoType] [GoValueClone("intbuf")] partial struct pp {` reads `[GoType] partial struct pp {`, with the record in `package_info.cs` carrying the rest:

```csharp
    // <TypeAccessibility>
    [GoValueClone("grid")] internal partial struct holder {}
    [GoValueClone("b")] internal partial struct inner {}
    internal partial struct row {}
    // </TypeAccessibility>
```

Mechanics worth knowing:

* **The attributes and the access modifier travel together, by construction.** `recordTypeAccessibility` takes the stamps as an argument and returns what the caller must still write inline — empty when the record absorbed them. So the two paths that write **no** record keep the stamps on the declaration and cannot lose them: a hand-owned file (whose emission goes to the non-compiled `.cs.auto` review sibling) and a `-tests` bridge unit (whose accessibility is inline because its metadata anchor can be a different test class). A hand-written conversion therefore still stamps `[GoValueClone]` inline, and it still works.
* **`TypeGenerator` reads the stamp across every partial declaration**, starting with the `[GoType]` one its receiver matched and continuing through the symbol's other `DeclaringSyntaxReferences`. That is what makes both placements equivalent rather than one replacing the other. The match itself stays syntactic, as it is for every attribute this generator reads.
* **The section sorts on the DECLARATION, not the line.** `typeAccessibilityKey` strips the attribute prefix before comparing, so a stamped entry keeps the place its accessibility/kind/name earns instead of being pulled into a leading block by its `[`. Sorting the raw line is legal but scrambles a section whose whole value is being readable at a glance.
* **Not a semantic change anywhere.** The relocation moves *where the attribute is written*, exactly as the `TypeAccessibility` section moved where the modifier is written. Nothing observes a difference: the generated `Clone()` is identical, and `GoReflect`'s `%T` output is identical.

## BCL names in generator templates are global::-qualified too

**BCL names in generator templates are global::-qualified too — a Go type can shadow any bare BCL
name.** The generated partials sit inside the package class, where every Go type in the package is
a sibling member that wins name lookup over `System.*`: internal/trace/traceviewer declares
`type Range struct`, so the named-string wrapper's sub-slice indexer `this[Range range]` bound the
Go `Range` instead of `System.Range` (CS1503 inside its own `ViewType.g.cs`). This is a *class* of
collisions, not one bug — any package declaring a type named `Range`, `Index`, `Type`, `Span`, … is
exposed — so the audit qualified every BCL reference the TypeGenerator templates emit:
`global::System.Range` (string/slice/array indexers), `global::System.Span<T>`/`ReadOnlySpan<byte>`,
the `IEnumerator`/`IEnumerable` members, `ICloneable`, `IEquatable` and the `System.Numerics`
operator interfaces on numeric wrappers, `System.Type`/`Reflection.MethodInfo`/`Activator`/
`NotImplementedException`/`[DebuggerNonUserCode]` in the dynamic-interface machinery, and the
`GeneratedCode` attribute stamped on every generated declaration (`Common.cs`, shared by all
generators). golib names (`slice<T>`, `NilType`, `IChannel`, …) stay bare — they live in the `go`
namespace the generated code owns. Converter-emitted visible code is not part of this rule (it
renders BCL names by the file-scoped conventions above). (Guarded by `BclTypeNameShadow` —
a package declaring `type Range struct` alongside a named string type and a named slice type, both
sub-sliced with the Go `Range`'s fields as bounds, output vs Go.)

## Two `[GoType]` payload conventions coexist

Two `[GoType]` payload conventions coexist, and the generator's alias substitution must tell them
apart. The map/channel emitters write dotted types in **source-alias form** (`CrossPkgLib.Ticks`,
via `getAliasQualifiedTypeName`), which the substitution above resolves; the slice/array element and
defined-over-selector emitters write the **namespace-qualified form** (`io.fs_package.FileInfo`,
via `getFullyQualifiedTypeName`), which roots through the `go` namespace and must pass through untouched.
The telltale is the segment after the leading identifier: a real alias maps to a package *class*,
so its next segment is a type name — a `_package`-suffixed next segment means the leading
identifier is a namespace segment that merely *collides* with a file alias. net/http's fs.go
aliases `io` while declaring `type fileInfoDirs []fs.FileInfo` → `[]io.fs_package.FileInfo`;
substituting the `io.` produced the nonexistent `go.io_package.fs_package.FileInfo` (CS0426 ×48).
The substitution skips exactly those occurrences (a negative lookahead on `_package.`). On the
converter side, the namespace-qualified form must lead with the **canonical** qualifier, never a
file-local Δ collision-rename: a consumer whose own namespace has a same-named child imports under
`using ΔIoLike = IoLike_package;`, but `[]ΔIoLike.FsLike_package.Info` resolves nowhere in the
alias-free `.g.cs` — `canonicalizeQualifierRename` reverts a leading import-rename segment
(mirroring the visitTypeSpec global-using-target rule). (Guarded by `NamedSliceChildPkg` — a
nested-namespace consumer package importing both `IoLike` and `IoLike/FsLike`, with a named slice
of the subpackage's type used across the assembly boundary.)

## ImplicitConvGenerator

### A GoImplicitConv record needs at least one LOCAL operand
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

#### ...but the POINTER-BOXING route needs none, and a whitebox-production operand still counts
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

### Generated conversion operators between named numerics of different assemblies

**Generated conversion operators between named numerics of *different* assemblies.** The two paragraphs above are the *converter's inline* casts. Separately, when the converter sees a conversion *between two named numeric types* it records a `[assembly: GoImplicitConv<…>]` and the `ImplicitConvGenerator` emits a user-defined `implicit operator` for it. The emitted body constructs one named type from the other's underlying value: `new Target((ValueType)src.Value)`.

**`ValueType` is a CAST TARGET, and it names the constructed type's BACKING PRIMITIVE (corrected
2026-08-08).** The template applies it to `src.Value` and feeds the result to the constructed type's
constructor, and that constructor takes the primitive — so `ValueType` must be `uint32`, `nint`,
`int64`, not the wrapper. It named the **constructed type itself** until this was rooted, making the
body a round-trip through that type's own conversion operators — `new WaitStatus((WaitStatus)src.Value)`
— which compiles only while a standard EXPLICIT conversion exists between the two primitives, because
a user-defined conversion admits just one standard conversion on its input. syscall's unix flavors are
where it finally bit: `WaitStatus` is backed by `uint32` and `Signal` by `int` (`nint`), and
`uint32`→`nint` is not a standard IMPLICIT conversion (a 32-bit unsigned value does not fit a 32-bit
native int), so its reverse is not a standard explicit one, the operator is not applicable, and the
cast is **CS0030**. Windows declares `WaitStatus` a struct, so the pair is never registered there and
no corpus build reached it. All 49 of the corpus's `ValueType` records carried the constructed type's
own name, so the form was never right — only never yet fatal, because the two compensating generator
overrides below cover most of the gap. One consumer had to move with it: the `uintptr` hop read
`ValueType` as the type to CONSTRUCT (`new {valueType}(…)`) and now constructs the LH type and casts
to `ValueType`, exactly like the default body. (Guarded by `implicitConvValueType_test.go` — an
end-to-end conversion of two named numerics with different underlyings, plus a unit sweep over every
basic kind a named numeric can carry.)

When both named types live in the **same** assembly the default body is fine (e.g. runtime's
`muintptr ↔ Δhex`), but when the operator must **construct a *foreign* named numeric** — one declared
in another C# assembly — two problems appear that only manifest cross-assembly:

* A direct cast to the foreign named type has no route. `(NameOff)src.Value` where `src.Value` is `ulong` and `NameOff` (`internal/abi`) is a *different assembly* is **CS0030** — C# does not select the foreign type's `int32`-based user conversion for a `ulong` source across the assembly boundary (the same cast to a *local* named type compiles). It must go **through the foreign type's underlying basic**: `new …NameOff((int)src.Value)`.
* The default host can be a phantom. The operator is hosted in `partial struct {sourceType}`; if that source is the *foreign* type (reached here via a local alias, e.g. runtime's `global using nameOff = abi.NameOff`, so the cross-package dot is hidden and the conversion records as `Inverted`), the `partial struct NameOff` declares a new *empty local* type rather than extending the foreign one — **CS1729** (no constructor). The operator is relocated into the **local** type instead.

So for a foreign *constructed* type the generator emits, fully-qualified and hosted in the local type:

```csharp
// runtime, dur↔hex style: foreign abi.NameOff constructed from local Δhex
partial struct Δhex {
    public static implicit operator global::go.@internal.abi_package.NameOff(global::go.runtime_package.Δhex src)
        => new global::go.@internal.abi_package.NameOff((int)src.Value); // through the underlying int32
}
```

The override fires **only** when the `new`-constructed side (the LH type: the *source* when the conversion is `Inverted`, else the *target*) is foreign; same-assembly operators are emitted byte-identically as before (no churn). Because the trigger is inherently cross-assembly, the behavioral-test harness (single-assembly, and unable to import a foreign named numeric — `internal/*` types are un-importable from a test module and the baseline stubs expose none) cannot host it; the guard is the **`core/runtime` build**, where `NameOff`/`TypeOff`/`TextOff` ↔ `Δhex` naturally occur (this fix cleared 3×CS0030 + 3×CS1729 there).

A **same-assembly** pair also needs the through-underlying routing when the two named numerics have **incompatible underlyings** — internal/trace's public `type Time int64` ↔ unexported `type timestamp uint64`, converted both ways (`Time(ev.Ts)` / `timestamp(ts)`). The default `new Time((ΔTime)src.Value)` casts `src.Value` (a `ulong`, since `timestamp` is `uint64`-backed) straight to the wrapper, which routes through the wrapper's `long`-based user conversion — but `ulong`→`long` is not an implicit C# conversion, so the cast is **CS0030**. (This is the *mixed-accessibility* case: `Time` is exported and `timestamp` is not, so the operator is already relocated into the less-accessible `timestamp` struct — orthogonal to the underlying.) The generator now, for a **local** numeric pair, casts through the constructed type's underlying C# keyword when the source underlying does **not** implicitly convert to it: `new Time((long)src.Value)`, `new timestamp((ulong)src.Value)`. The source/constructed underlyings are read from each side's `[GoType("num:X")]` tag (a sibling generator cannot see the generated `Value` property), and the implicit-convertibility test is the fixed C# numeric-conversion table over the fixed-width integer/float basics. Crucially this fires **only** on pairs the default cast could not compile (the default `(Wrapper)src.Value` succeeds *iff* that same source→underlying conversion is implicit), so every already-compiling conversion stays byte-identical — the full behavioral suite's [goldens](../Glossary.md#golden) are unchanged. `uintptr`-backed pairs keep the existing `nuint`-hop override; `int`/`uint` native-width wrappers are deliberately left to the default (their classification is version-sensitive and the failing corpus cases are fixed-width). (Guarded by the `NamedIntSignednessConv` behavioral test — a public `int64` ↔ unexported `uint64` named pair converted both ways, including a `^uint64(0)`→`int64` case whose `-1` result verifies the cast preserves the bit pattern exactly, output-compared vs Go; internal/trace's `timestamp`→`Time` inverse operator relies on it.)

A **cross-assembly** mixed-accessibility pair has no legal form at all, and is skipped. The relocation
above is the only remedy for a mixed pair — a C# user-defined conversion operator is necessarily
`public` **and** must be declared in one of its two operand types — and a *foreign* type cannot host
anything. Hosting in the local, more accessible side then exposes a type less accessible than the
operator: **CS0056** when the foreign side is the return type, **CS0057** when it is the parameter, so
neither direction is expressible. The shape is reachable only under the `-tests` white-box model, where
a package's own `_test.go` declares an EXPORTED defined type over an UNEXPORTED production one — `time`'s
`export_test.go` has `type RuleKind int` beside `zoneinfo.go`'s `type ruleKind int`, which become
`public RuleKind` in the test assembly and `internal ruleKind` in the referenced production assembly.
Nothing is lost by skipping: the converter renders such a conversion site as an explicit
through-underlying cast (`(RuleKind)(nint)r.kind`), which needs no operator at all. The local side's
accessibility comes from the GO export rule, as in the relocation above (at analysis time the `[GoType]`
partials are modifier-less); the FOREIGN side is read from metadata, where it is already final.

The recorded `GoImplicitConv` must also be able to **name the foreign type**. The recorded type name carries the foreign package's import qualifier — the DOT form `driver.IsolationLevel` for an unrenamed type, or a `ꓸ` global-using alias (`CrossPkgLibꓸGrade`) for a `Δ`-renamed one — but the attribute sits in `package_info.cs` at file scope and the generated operator lands in a `.g.cs`, neither of which carries the body files' import `using`s. A `Δ`-renamed foreign numeric resolves through its own `ꓸ` global using, but the **dot form needs a resolving `using driver = go.database.sql.driver_package;`** in `package_info.cs`'s `ImportedTypeAliases` block. The STRUCT-conversion branch of `checkForImplicitConversion` already drives that using by calling `recordConversionPackageUsing(argType)`/`(funcType)`, but the **aliased-NUMERIC branch omitted it** — so a cross-package named-numeric conversion (database/sql's `driver.IsolationLevel(opts.Isolation)`, where `sql.IsolationLevel` and `driver.IsolationLevel` are distinct named ints) left `driver` unresolved in both the attribute and the generated operator (CS0246). The numeric branch now records the same package usings. (Guarded by an extension to the `CrossPkgUser` cross-assembly test — a local `float64`-based named numeric converted to the *unrenamed* `CrossPkgLib.Celsius`, which renders in dot form and so needs the registered using; a `Δ`-renamed target like `CrossPkgLib.Grade` would have resolved via its alias and would not have caught the gap.)

### A pointer conversion bridging structurally-identical structs records a boxing implicit conversion

When a pointer conversion `(*Target)(srcPtr)` bridges two structurally-identical structs, the converter records an **indirect** (boxing) implicit conversion `Source → ж<Target>` and `ImplicitConvGenerator` emits `implicit operator ж<Target>(Source src) => Ꮡ(new Target(<members>))`. For a **self-boxing** conversion — `Source` and `Target` are the *same* struct (`mspan → ж<mspan>`), which arises from a self-referential struct's recursive sub-struct conversions — that member-by-member reconstruction is both unnecessary and wrong: a pointer field whose target ctor parameter is itself a `ж<…>` was deref'd (`src.f?.Value ?? default!`), and a value cannot bind a pointer parameter (CS1503). The generator detects self-boxing (the boxed element type equals the source) and emits `Ꮡ(src)` instead — boxing a copy of the whole struct directly, identical in effect for a pointer-free struct and correct for one with pointer fields. (Validated by the green baseline build, which regenerates every `.g.cs`, plus the `TypeConversion` behavioral test for the non-self-boxing form; runtime exercised self-boxing for `mspan`, `g`, `stackScanState`, `hmap`, etc.)

---

[← The `go.golib` support namespace](golib-namespace.md) · [Index](README.md) · [The standard-library conversion applies `-tags purego` →](purego.md)
