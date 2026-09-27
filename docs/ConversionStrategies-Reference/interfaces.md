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

Two rules govern how concrete implementation records are emitted:

* **Only impl types declared in the *current* package are recorded.** `ImplementGenerator` realizes the attribute by emitting a `partial struct <Impl> : <Interface>` into the **current package's** namespace and class — so it can only add an interface to a type defined in the *same assembly*. A pairing whose impl type is *imported* from another package (e.g. `image/color/palette` building `[]color.Color{ color.RGBA{…} }`) is therefore **not** re-emitted in the consumer: that relationship is already established in the impl type's own package (`image/color` records `[assembly: GoImplement<ΔRGBA, Color>]`). Re-emitting it in a consumer would generate a broken cross-assembly partial (a fresh empty `palette_package.ΔRGBA` rather than the real `color_package.ΔRGBA`), so the converter skips any pairing whose impl type is not local.
* **Multi-segment interface references are root-qualified.** The `GoImplement` attributes are emitted before the file's `namespace` with only `using go;` in scope; that directive imports the *types* of namespace `go` (so a top-level `io_package.Writer` resolves unqualified) but **not** its nested namespaces. A multi-segment package class such as `container.heap_package.Interface` is therefore root-qualified to `go.container.heap_package.Interface` so it resolves; single-segment refs (`io_package`, `sort_package`) are left unchanged.

## Equality

### Comparing two interfaces of one UNCOMPARABLE dynamic type panics, as Go does

Go decides an interface `==` in three steps, and only the third can panic:

1. a **nil** operand makes it a nil test — `m == nil` is false for an interface holding a map, and
   never panics;
2. a **dynamic-type mismatch** answers false — `m == 5` never panics either;
3. only once both operands carry the **same** dynamic type does the runtime run that type's equal
   algorithm — and an uncomparable type has none, so it panics
   `runtime error: comparing uncomparable type T`.

`builtin.AreEqual(object?, object?)` implemented the first two and answered step 3 quietly with a
`bool`, for every shape but one: a **map**, **slice** or **func** held in an interface, and a
**struct or array that transitively contains one**. The single covered shape — two adapters over a
nil named-func delegate — was a special case of exactly this rule, and now routes through the same
mint (`RuntimeErrorPanic.ComparingUncomparableType`) rather than restating the message.

The gate sits **after** both nil legs and the dynamic-type check, which is what makes it safe against
the ~1,300 emitted `AreEqual` call sites: it is reached only where Go itself would have run the equal
algorithm and found none, and the converter emits `AreEqual` only for a comparison Go's own type
checker admitted. The panic is a recoverable runtime error, so `recover()` observes it exactly as in
Go.

The message spells the dynamic type as Go spells it, which takes two things the managed type alone
cannot supply:

| Go value in an `any` | reported type |
|---|---|
| `map[string]int{}` | `map[string]int` |
| `[]int{1}` | `[]int` |
| `func(){}` | `func()` |
| `withSlice{1, []int{2}}` | `main.withSlice` — the STRUCT, not its field |
| `myMap{}` (`type myMap map[string]int`) | `main.myMap` — its OWN name |
| `[1][]int{{1}}` | `[1][]int` — the LENGTH is part of the type |

The length comes off the live operand (`GoReflect.ArrayDimsOfValue`), because a managed `array<T>`
does not carry it in its `Type` — only the value knows. Comparability itself is **not** restated
here: it delegates to `GoReflect.IsComparable`, already the signal the reflection bridge populates
`abi.Type.Equal` from, so `==` and `reflect.Type.Comparable` answer from one definition. The verdict
is immutable per type and cached in a `ConcurrentDictionary<Type, bool>`, since `==` is a hot path
(roughly every `err == io.EOF` in the corpus) and an uncached walk would re-reflect over a struct's
fields on every comparison.

**A struct's interface-typed FIELD recurses correctly** — `TypeGenerator` already compares such
fields through `AreEqual` (see *the interface field an emitted `Equals` must route through
`AreEqual`*), so `struct{ V any }` holding a map panics naming `map[string]int`, the inner type, just
as Go does.

**Stated residual — an ARRAY that reaches an uncomparable value through an interface.** Measured
against go1.23.12: `[1]any{map…}`, `[1]any{[1]any{map…}}` and `[1]withAny{…}` (a struct with an `any`
field) all panic in Go and all answer quietly here. Two distinct mechanisms sit behind that, and
neither is the gate above:

- a SELF-comparison never reaches an element at all — `array<T>`'s structural equality
  short-circuits on backing-store reference identity, so `a == a` is true before any element is
  examined. This is visible in the `[1]withAny{…}` row, whose per-element comparer *would* have
  panicked (the struct routes its `any` field through `AreEqual`), and did not;
- for genuinely distinct arrays, `array<T>` compares elements with `EqualityComparer<T>.Default`
  rather than Go's relation, so an `[N]any` never consults `AreEqual` for its elements.

Closing either means changing `array<T>`'s equality, which also decides `GetHashCode`, array-typed
map keys and `DeepEqual` — and the element-comparer half is the same hole
`GoEqualityComparer.ForKeys<T>()` closed for map keys, whose doc records a deliberate decision to
leave float-containing arrays on the BCL rule. Left as a measured residual with no known consumer
rather than covered speculatively (the r39d rule).

(Guarded by the `UncomparableEquality` behavioral test — every panicking shape with its message
asserted verbatim, every nil and type-mismatch non-trigger, and the comparable positive controls,
output-compared vs `go run`.)

## Sub-pages

| Page | Covers |
|:--|:--|
| [Adapters](interfaces/adapters.md) | the pointer adapter, value and interface-to-interface adapters, cross-package adapters, adapter naming and accessibility, keyword names, adapters at composite and field sites |
| [GoImplement records](interfaces/records.md) | recording a pair, C# interface inheritance, method-set gating, the white-box pair, record de-duplication, sealing-marker stubs |
| [Run-time assertions](interfaces/runtime-asserts.md) | the duck-typing shells that resolve a structural assert, type names rendered through adapters |
| [Promotion through embeds](interfaces/promotion.md) | embedded interface fields in adapters, promoted methods of a foreign struct |

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
- <a id="system-colliding-local-type-names-are-root-qualified-in-assembly-attributes"></a>Moved to [System-colliding local type names are root-qualified in assembly attributes](package-conversion/imports-and-build-constraints.md#system-colliding-local-type-names-are-root-qualified-in-assembly-attributes).
- <a id="cross-package-pointer-to-interface-conversions-use-the-foreign-adapter"></a>Moved to [Cross-package pointer-to-interface conversions use the foreign adapter](interfaces/adapters.md#cross-package-pointer-to-interface-conversions-use-the-foreign-adapter).
- <a id="cross-package-value-to-interface-conversions-use-the-local-value-adapter"></a>Moved to [Cross-package value-to-interface conversions use the local VALUE adapter](interfaces/adapters.md#cross-package-value-to-interface-conversions-use-the-local-value-adapter).
- <a id="a-collision-renamed-types-pointer-adapter-composes-on-the-package-qualifier-never-the-whole-type-alias"></a>Moved to [A collision-renamed type's pointer adapter composes on the package qualifier, never the whole-type alias](interfaces/adapters.md#a-collision-renamed-types-pointer-adapter-composes-on-the-package-qualifier-never-the-whole-type-alias).
- <a id="under--tests-a-white-box-production-type-is-foreign-to-the-generator--so-the-name-carries-the-prefix"></a>Moved to [Under `-tests`, a white-box PRODUCTION type is FOREIGN to the generator — so the name carries the prefix](interfaces/adapters.md#under--tests-a-white-box-production-type-is-foreign-to-the-generator--so-the-name-carries-the-prefix).
- <a id="anonymous-interfaces-used-as-an-adapter-target-are-lifted-package-wide"></a>Moved to [Anonymous interfaces used as an adapter target are lifted package-wide](interfaces/adapters.md#anonymous-interfaces-used-as-an-adapter-target-are-lifted-package-wide).
- <a id="adapter-accessibility-symbol-or-name-on-both-sides"></a>Moved to [Adapter accessibility: symbol-OR-name on both sides](interfaces/adapters.md#adapter-accessibility-symbol-or-name-on-both-sides).
- <a id="a-dynamic-interfaces-runtime-conversion-class-re-escapes-a-keyword-method-name"></a>Moved to [A dynamic interface's runtime conversion class re-escapes a keyword method name](interfaces/adapters.md#a-dynamic-interfaces-runtime-conversion-class-re-escapes-a-keyword-method-name).
- <a id="a-keyword-named-types-interface-adapters-escape-declarations-and-compose-class-names-unescaped"></a>Moved to [A keyword-named type's interface adapters escape declarations and compose class names unescaped](interfaces/adapters.md#a-keyword-named-types-interface-adapters-escape-declarations-and-compose-class-names-unescaped).
- <a id="an-interface-members-keyword-named-parameters-escape-in-every-generated-implementation"></a>Moved to [An interface member's keyword-named PARAMETERS escape in every generated implementation](interfaces/adapters.md#an-interface-members-keyword-named-parameters-escape-in-every-generated-implementation).
- <a id="a-struct-literal-interface-field-takes-a-pointer-elements-adapter"></a>Moved to [A struct-literal interface field takes a pointer element's adapter](interfaces/adapters.md#a-struct-literal-interface-field-takes-a-pointer-elements-adapter).
- <a id="the-struct-field-interface-routing-also-fires-on-an-elided-element-composite"></a>Moved to [The struct-field interface routing also fires on an ELIDED element composite](interfaces/adapters.md#the-struct-field-interface-routing-also-fires-on-an-elided-element-composite).
- <a id="a-keyed-elements-interface-target-is-the-composites-own-slot-never-the-lhs-variables-type"></a>Moved to [A keyed element's interface target is the composite's own SLOT, never the LHS variable's type](interfaces/adapters.md#a-keyed-elements-interface-target-is-the-composites-own-slot-never-the-lhs-variables-type).
- <a id="structural-interface-satisfaction-emits-c-interface-inheritance"></a>Moved to [Structural interface satisfaction emits C# interface inheritance](interfaces/records.md#structural-interface-satisfaction-emits-c-interface-inheritance).
- <a id="goimplement-records-de-duplicate-at-attribute-emission"></a>Moved to [GoImplement records de-duplicate at attribute emission](interfaces/records.md#goimplement-records-de-duplicate-at-attribute-emission).
- <a id="a-global-root-escape-is-a-third-spelling-of-one-type-and-the-record-sets-dedupe-on-text"></a>Moved to [A `global::` root escape is a THIRD spelling of one type, and the record sets dedupe on text](interfaces/records.md#a-global-root-escape-is-a-third-spelling-of-one-type-and-the-record-sets-dedupe-on-text).
- <a id="the-promoted-method-twins-class-is-named-for-the-package-and-the-pair-not-the-pair-alone"></a>Moved to [The promoted-method twins class is named for the PACKAGE and the pair, not the pair alone](interfaces/records.md#the-promoted-method-twins-class-is-named-for-the-package-and-the-pair-not-the-pair-alone).
- <a id="a-cross-package-interfaces-unexported-sealing-marker-is-stubbed"></a>Moved to [A cross-package interface's unexported sealing marker is stubbed](interfaces/records.md#a-cross-package-interfaces-unexported-sealing-marker-is-stubbed).
- <a id="so-the-declaring-package-must-own-the-adapter-and-its-speculative-record-carves-out-for-it"></a>Moved to […so the DECLARING package must own the adapter, and its speculative record carves out for it](interfaces/records.md#so-the-declaring-package-must-own-the-adapter-and-its-speculative-record-carves-out-for-it).
- <a id="every-eligible-interface-carries-runtime-duck-typing-shells--the-sole-resolver-of-a-structural-assert"></a>Moved to [Every eligible interface carries runtime duck-typing shells — the sole resolver of a structural assert](interfaces/runtime-asserts.md#every-eligible-interface-carries-runtime-duck-typing-shells--the-sole-resolver-of-a-structural-assert).
- <a id="t-and-type-name-rendering-generally-unwraps-generated-adapters-and-pointer-boxes"></a>Moved to [`%T` (and type-name rendering generally) unwraps generated adapters and pointer boxes](interfaces/runtime-asserts.md#t-and-type-name-rendering-generally-unwraps-generated-adapters-and-pointer-boxes).
- <a id="a-named-field-whose-name-equals-its-interface-type-is-not-an-embedded-interface"></a>Moved to [A named field whose name equals its interface type is NOT an embedded interface](interfaces/promotion.md#a-named-field-whose-name-equals-its-interface-type-is-not-an-embedded-interface).
- <a id="a-foreign-structs-promoted-method-forwards-through-its-value-embed"></a>Moved to [A foreign struct's promoted method forwards through its value embed](interfaces/promotion.md#a-foreign-structs-promoted-method-forwards-through-its-value-embed).
- <a id="a-name-both--tests-variant-classes-declare-is-qualified-with-the-files-anchor-class"></a>Moved to [A name both `-tests` variant classes declare is qualified with the FILE's anchor class](test-conversion.md#a-name-both--tests-variant-classes-declare-is-qualified-with-the-files-anchor-class).
- <a id="a-keyword-named-addressed-globals-heap-box-field-strips-the-escape-after-the-ꮡ-prefix"></a>Moved to [A keyword-named addressed global's heap-box field strips the escape after the Ꮡ prefix](naming.md#a-keyword-named-addressed-globals-heap-box-field-strips-the-escape-after-the-ꮡ-prefix).

---

[← Struct Type Embedding](struct-embedding.md) · [Index](README.md) · [Pointers →](pointers.md)
<!-- {% endraw %} -->
