# TRAIN M -- a method promoted through another package's embed, in the RUNTIME method set

**Status:** DESIGN RECORD **v2**, no code. It revises v1 (`93e4e5d7e6`) under COORD's CONDITIONS ruling (G's second
read C1-C5 and COORD's three-lens review), for G and COORD to re-read before any cut.
**Lane:** i9
**Date:** 2026-09-30
**Tree read:** `master` @ `f819887fa3`. Every file:line below is at that tree and, per COORD, identical on TRAIN K's
union `4016a2269c`, except item 8's two K-only files, which are cited on K. Toolchain go1.24.13 / .NET SDK 10.0.401.
**Sequencing:** M cuts on L's LANDED master. The red `CrossPackagePromotedValueMethod` lives on L's `8213221317`.
**Companion seat:** `claude/i9-crosspkg-value-promotion` @ `8213221317` (TRAIN L), the converter's CALL-SITE half of the
same defect class. This document is the METHOD-SET half.

## What v2 changes

- **(a) marker, ruled `[GoEmbedded]`:**
  - v1's premise was wrong. A generator-only attribute would need the SAME converter stamp, so it is the same 35 std
    files and 15 behavioral modules either way.
  - Added: the golib `Value.Elem` read-only fix, the full consumer list, R3's `TypeKind.Interface` gate, and positional
    hunk gates (§2.1).
- **(b) item 5, ruled a named residual:** re-attributed to the depth-blind cross-embed counter. The real generator
  shows it is a REGRESSION, not only a residual (§2.3, §4).
- **New rule R5**, a blocker, confirmed through the real generator (§2.2).
- **R2 reversed:**
  - the full guard set (every member name plus every `using static`) finds 26 clashes per target, not 4;
  - so cross-package forwarders go into a SIBLING static class, which gives 0 skips instead of 26 (§2.2).
- **R3 widened** to inherited interface members (20 composite interface embeds per target).
- **New population and rule items:** C3 signature accessibility (census 0), C15 name normalization, C4 transitive
  (real generator, census 67), and C14 generics.
- **Item 10:** a parity arm, and `[GoPackage]` read from symbol attributes.
- **Gates rewritten.** Populations are now stated term by term, with units defined.
- **Real-generator experiment added (§4).** Three Go modules, converted and built with an R1-only prototype against
  master, run against Go's output. It confirms C4, item 5 (as a regression), R5 and R2.

---

## 0. Headline

> **A Go struct that embeds a struct from ANOTHER package gets none of that embed's methods in its run-time method set.**
> `fmt`, assertions, type switches and `reflect` answer MISS where Go answers HIT. On master the experiment's
> `Direct{ProtoA.Inner}` has NumMethod 0/0 (value/pointer) where Go says 4/5.
>
> The cause is a deliberate generator gate. Lifting it (R1) is necessary, and the real generator shows it is **unsafe
> alone in five measured ways:**
> - **R5, value-set over-claim.** Pointer-receiver forwarders are emitted without `[GoRecv]`, so the value set
>   over-claims them: NumMethod 5 vs Go's 4, and `MethodByName("Set")` CRASHES.
> - **R2, name clashes.** A forwarder landing in the package class collides with package members: CS0102 against a
>   package var. A full-guard census finds 26 such names per target, reflect's `Kind`/`ChanDir` among them.
> - **R3, interface providers.** An interface embed is invisible to the generator's ambiguity count. io's
>   `TestCopyLargeWriter` would PANIC.
> - **R4, field shadowing.** A shallower field shadows the method in Go; without the rule reflect fails to build.
> - **Item 5, depth flattening.** It REGRESSES a correct answer: `Five has Tok` goes true → false.
>
> The design is one converter change (`[GoEmbedded]` on interface embeds), five generator rules (R1-R5), cross-package
> forwarders in a SIBLING static class, and one golib fix (`Value.Elem`'s read-only flag).
>
> **Predicted footprint** (windows / linux / darwin, rows as defined in §3):
> - **+706 / +703 / +703** forwarded method rows on the 56 / 55 / 55 std types;
> - **0 same-package forwarder changes**;
> - 0 R2 skips;
> - three named residuals:
>   - **C4 transitive, 67 rows:** promotions that pass through a PRE-EXISTING same-package internal forwarder;
>   - **generics, 11 rows:** `unique.uniqueMap`;
>   - **item 5, population 0.**
> - **Converter footprint of the marker:** 54 production interface embeds in 35 std files, 136-140 test embeds in
>   57-59 files, and 24 embeds in 15 behavioral modules.

---

## 1. The mechanism (confirmed; COORD's G1-G8)

- **The gate.** `StructTypeTemplate.GetMetadataPromotedMethods` (`src/gen/go2cs-gen/Templates/StructType/StructTypeTemplate.cs:1144`)
  returns empty when the embed's `[GoPackage]` differs from the enclosing struct's (`:1154-1157`). In an MSBuild build
  every cross-package embed is a METADATA embed.
- **The one method-set source.** golib's `GetGoMethodSetCandidates` (`src/core/golib/runtime/TypeExtensions.GoMethodSets.cs:302-331`)
  reads ONLY emitted extension methods. The probe, `AdapterBinder`, `reflect`'s method table and the assert cache all
  read that list. Its filter (`:312-321`) admits a `this ref T` receiver to the VALUE set unless it carries `[GoRecv]`,
  which is R5's hinge.
- **At the generated source**, the TRAIN L red's `Stamp{time.Time}`, `PtrStamp{*time.Time}` and
  `Outer{inner{time.Time}}` carry an EMPTY "Promoted Struct Receivers" block. The same-package control
  `Wrap{localBase}` gets its `Twice` pair.
- **The pointer method set.**
  - A direct VALUE embed gets the box shim (`IsValueEmbedBoxRecv`, `:997-1018`) for a BOX-primary pointer method
    (`this ж<E>`).
  - A `[GoRecv] this ref E` pointer method (the common form: `time.Time.UnmarshalJSON`, `sync.Mutex.Lock`) is harvested
    as a value-shaped method (`IsRefRecv`). It is forwarded as `this ref T` at any depth, WITHOUT `[GoRecv]`
    (`:1026-1030`; the template emits no `[GoRecv]`). That is R5.
- **Transitive promotion** reads a foreign embed's OWN forwarders, which are extension methods with receiver E. That is
  also the source of item 5 (depth flattening) and C4 (a forwarder the name heuristic made `internal` is invisible).
- **The interface-implementation bridge is undisturbed.** `GoImplement` adapters resolve members through the embed hop
  from SYNTAX (`StructDeclarationSyntaxExtensions.GetEmbeddedPointerHopNames`, `:573`), and the TypeGenerator's output
  is not syntax that generator can see.

---

## 2. The change

### 2.1 Converter: `[GoEmbedded]` on an interface embed (ruled), and what it touches

- **The gap.** An embedded INTERFACE is emitted as a plain field, identical to a same-named named field
  (`src/core/io/io_test.cs:22-24`: `public io_package.ReaderFrom ReaderFrom;`). The generator deliberately treats a
  field named like its type as NOT an embed (`StructTypeTemplate.cs:851-856`).
- **The stamp.** The converter stamps `[GoEmbedded]` on the field, extending `src/go2cs/visitStructType.go:845, :851`
  from predeclared embeds to interface embeds. The docs that say "predeclared only" are amended:
  `src/core/golib/GoEmbeddedAttribute.cs:11-17` and `src/core/golib/GoReflect.FieldAccess.cs:417-418`.
- **R3's input.** A `[GoEmbedded]` field counts as an interface provider ONLY when its type symbol is
  `TypeKind.Interface`, since the attribute already stamps predeclared embeds.
- **Every consumer of the Anonymous / Embedded bit.** Each turns Go-correct for interface embeds:
  - `GoReflect.FieldAccess.cs:419`: the projection itself.
  - `:451`: the embedded-field reorder.
  - `GoReflect.ValueMarshalling.cs:464-466`: struct convertibility demands equal `Embedded`, so this is a NEW refusal.
    Go's compiler rejects a static conversion between `struct{ io.Reader }` and `struct{ Reader io.Reader }` (the types
    are not identical), so the refusal is reachable only through `reflect` (`ConvertibleTo`/`Convert`). The static
    census is 0 by construction; the run-time arms below gate it.
  - `GoStructSynthesis.cs:310` (the synthesized-type identity key) and `:363` (the embed list).
  - `reflect/value_impl.cs:1111` (the `flagEmbedRO` choice), `:2763` (`StructField.Anonymous`) and `:3184`
    (type-identity field compare).
  - `internal/reflectlite/type_impl.cs:212` (the same compare).
  - `encoding/xml`'s typeinfo, where Go newly admits an unexported ANONYMOUS field it walks into.
  - `reflect.VisibleFields`.
  - `encoding/json`: Go treats an anonymous INTERFACE field as named, so json output does not move. An arm pins it.
- **The golib fix in the same seat.**
  - `reflect/value_impl.cs:833` `elem.flag |= (flag)(v.flag & flagRO);` becomes Go's `elem.flag |= v.flag.ro();`
    (Go `value.go:1232`, `:89-94`).
  - Why: with the marker, an unexported interface embed gets `flagEmbedRO`, and `Elem` would carry it verbatim. The
    next `Field` would clear it, so `CanInterface()` would read TRUE where Go says false.
  - Red first: `reflect.ValueOf(w{E{}}).Field(0).Elem().Field(0).CanInterface() == false` for
    `type w struct{ error }`, and `%#v` of `w{E}` with `E.GoString`. Both are red with the marker alone and green with
    both changes.
- **Behavioral arms.** Anonymous is TRUE for an exported and for an unexported interface embed (PkgPath `main` for the
  latter). The others:
  - json with a named key, and with omit;
  - xml over an unexported interface embed;
  - `ConvertibleTo` against a `StructOf` twin and against a named-field twin.
- **Converter footprint** (census `ifaceembed`, go/types, deduplicated by position, split by production and test
  file):

  | target | interface embeds | composite | production | test |
  |---|---|---|---|---|
  | windows | 190 | 20 | 54 in 35 files | 136 in 57 files |
  | linux | 194 | 20 | 54 in 35 files | 140 in 59 files |
  | darwin | 193 | 20 | 54 in 35 files | 139 in 58 files |

  - Behavioral: 24 embeds in 15 of 734 modules.
  - Crossed against the 157 whole-file `[module: go.GoManualConversion]` hand-owned `.cs` files: **0 hits**. The
    intersection's positive control (a planted hand-owned path) is seen to hit. No hand-stamp is needed.
- **Positional gate.** Every CNR, `-stdlib` and `-tests` hunk from the marker is exactly a `[GoEmbedded] ` prefix on
  ONE interface-embed field line. The hunks match the census position for position; the goldens are predicted from
  it.

### 2.2 Generator: five rules, and where the forwarders live

- **R1. Harvest a cross-package metadata embed's EXPORTED methods.**
  - **Accessibility is explicit.** The plain forwarder pair takes the STRUCT's scope (`:886`), and the harvest's
    `IsSymbolAccessibleWithin` (`:1193`) admits friend-granted internals. So the filter is explicit, on the
    `PromotesFromMetadata` precedent (`:1120-1127`): public, and a Go-exported NAME.
  - **Name normalization (C15).** The exported test reads the Go name: strip the `Δ` `ShadowVarMarker` and an `@`
    escape (`src/gen/go2cs-gen/Common.cs:557-558`), then apply Go's first-rune rule. `pointerEmbedTypeNames` is keyed
    by simple name at EVERY depth (`:645-647`, `:721`, `:805`). It is gated on `typeName == promotedStructType`, so a
    deeper same-named embed cannot pass as a pointer embed.
  - **Signature accessibility (C3).** A method whose parameters or result name a type the enclosing assembly cannot
    access is SKIPPED with a comment line. That covers an unexported or friend-internal type, which would give
    CS0122/CS0050/CS0051. Census predicate: an exported method whose signature names a foreign unexported type. It is
    **0** in std x3, and its plant (`L.Leak() hidden`) is seen to fire.
  - **Generics (C14).**
    - The exclusion tests the SYMBOL: `TypeArguments.Length > 0` after unwrapping `ж<>`. The string test
      `Contains("<")` is true for `ж<time_package.Time>`.
    - A generic metadata embed is excluded, a named residual (§5.5).
    - A GENERIC ENCLOSING struct over a non-generic foreign embed has population 0 in std x3 and 1 in behavioral
      (`CrossPkgUser.holder`). Its forwarders carry the enclosing type parameters, as the same-package path already
      does (`:959-960`), with a GenTests arm.
    - The generic-enclosing BOX-SHIM gap (`:999`: no shim for a generic enclosing struct) is named, not fixed.
- **R2. Where the forwarders live: a SIBLING static class, chosen on the numbers.**
  - **The guard route.** The guard set COORD specified is every member name of the enclosing package class (types,
    fields, consts, properties, methods), plus the static members of every `using static` in scope:
    - the global `<Using Include="go.builtin" Static="True" />` (74 public static names, read by reflection from
      golib.dll);
    - the test bridges (`io/export_test.cs:6` `using static go.io_package`, and `io_test.cs:16`
      `using static go.io_internal_test_package`);
    - the dot-imports.
  - The simulator's clash arm on that set is planted on 5 arms (a function, a var, a builtin name, a dot-import, a
    bridge), each seen to fire. It finds **26 clashes on each of the three targets** (v1 counted 4, functions only):
    - reflect `Kind` and `ChanDir` on six types: reflect declares TYPES `Kind`/`ChanDir`, so this is CS0102;
    - internal/reflectlite `rtype.Kind`;
    - net `addrPortUDPAddr.Addr`;
    - math/big_test `Bits` x4 (bridge);
    - runtime_test `PaddedRWMutex.Lock`/`Unlock` (dot + bridge);
    - net/http_test `testMockTCPConn.File` and `cleanupT.Error`;
    - io_test `WriteString` x3;
    - go/types_test `gen.String`.
  - **The two routes compared:**
    - The guard route SKIPS all 26: method-set holes in must-not-move rows (reflect, reflectlite, io, net).
    - The SIBLING route places cross-package forwarders in a top-level static class beside the package class,
      `<Struct>ᴛxpkg` in namespace `go`, on the Shape-B precedent (`src/gen/go2cs-gen/Templates/InterfaceImpl/InterfaceImplTemplate.cs:102-113`).
      That class is invisible to bare-name lookup, so nothing is shadowed, and CS0102 cannot arise. The registry already
      "scans every non-nested sealed static class in the assembly" (ibid.; `TypeExtensions.ExtensionMethodRegistry.cs:169-181`).
      **0 skips.**
  - **Chosen: sibling.** The harvest (`GetMetadataPromotedMethods`) is extended to scan a foreign embed's `ᴛxpkg`
    class beside its package class. Same-package forwarders stay where they are, so they remain byte-identical.
- **R3. Count interface providers for ambiguity, INHERITED members included (G C2).**
  - `countPromotedMethods` (`:669-748`) walks struct embeds only. A `[GoEmbedded]` interface field (§2.1) contributes
    its methods at its depth: `AllInterfaces` plus the Δ-projection. A composite interface is emitted as C# inheritance
    with an empty body (`src/core/io/io.cs:152`, `ReadWriteCloser`), so its own members list is empty.
  - Population: 20 composite interface embeds per target, 3 of them production (net/http `transport.go:754` and `:2564`,
    net/smtp `smtp.go:278`).
  - Interface providers COUNT and never MINT; the converter's promoted-interface records own those.
  - Scope: R3 reads the ENCLOSING compilation's interface fields and those of source embeds. It does NOT count a
    METADATA embed's own interface fields; that embed's own generator already dropped what they annihilate.
  - Without R3: `struct{ bytes.Buffer; io.ReaderFrom; io.WriterTo }` (io_test's `Buffer`) gains `ReadFrom`/`WriteTo`,
    which Go deletes. `io.Copy` then takes a fast path Go keeps OFF, and `TestCopyLargeWriter` (`io_test.go:466`) goes
    PASS -> PANIC ("invalid Write count"). The hiding is what keeps io.Copy's fast paths OFF there; it does not make
    them work.
- **R4. Field shadowing, CROSS-PACKAGE names only (ruled).**
  - A harvested cross-package name is dropped when `LookupFieldOrMethod` finds a FIELD first. The census finds 8 drop
    rows over internal/abi in reflect (`Elem`, `StructType`, `InterfaceType`), identical on all targets.
  - Name comparison uses R1's normalization.
- **R5. `[GoRecv]` on a cross-package forwarder of a POINTER-receiver method (blocker, C1).**
  - **The rule.** When the harvested method is a pointer receiver (`IsRefRecv`, or `[GoRecv]` read from METADATA
    rather than inferred from `RefKind`) and the hop path holds NO pointer embed, the forwarder carries `[GoRecv]`.
    Through a pointer embed it carries none, because Go's value set includes it there.
  - **Confirmed** (§4): the prototype's `Set(this ref Direct target, nint v)` has no `[GoRecv]`. It gives value
    NumMethod 5 (Go 4), and `reflect.TypeOf(Direct{}).MethodByName("Set")` throws `NotImplementedException` ("(Direct&,
    IntPtr) -> Void has no Func<>/Action<> delegate form"). The anonymous assert path happened to answer false, which
    MASKS the over-claim there.
  - **Census `r5`** (go/types; pointer-receiver promotions whose path is all VALUE hops, assuming the common
    `[GoRecv] this ref` form):

    | target | cross-package | same-package residual |
    |---|---|---|
    | windows | 360 rows in 33 types | 1017 rows in 125 types |
    | linux | 360 rows in 33 types | 1013 rows in 126 types |
    | darwin | 360 rows in 33 types | 1013 rows in 126 types |

    - The same-package residual is an upper bound, since it assumes the `[GoRecv]` form.
    - False VALUE-T interface satisfactions in golib's view: 83 on windows, 55 of them on cross-package types, which
      R5 prevents. Examples: `io_test.Buffer` as an `io.Writer`, and `sync.Locker` on 14 types.
    - Controls: `Stamp`/`inner`/`Outer` fire; `PtrStamp` (pointer hop) does not.
  - Scope: cross-package. The existing same-package over-claim (for example `PromotedEmbedAnonIfaceWitness.cs:16`'s
    `CommonType.String`) is a named residual (§5.6) with that census.
  - Red first: `any(Stamp{}).(json.Unmarshaler) == false`; NumMethod equal to Go's for T and *T; `MethodByName` does
    not throw; and a `PtrStamp{*time.Time}` positive.

### 2.3 Items 5-10 and C4

- **Item 5, ruled a named residual. Re-attributed, and it is a REGRESSION.**
  - The cause is the DEPTH-BLIND cross-embed counter (`:697`, `:879-883`). A metadata embed's harvested forwarders
    count at the embed's depth, so a depth stamp alone would not fix it.
  - The real generator (§4): `Five{ ProtoB.Mid; loc }`, where `loc` declares `Tok` at depth 1 and Mid promotes
    Inner's `Tok` from depth 2. Go answers `Five has Tok: true` (N 7). Master answers true, because nothing is
    promoted. **R1 answers false.**
  - Population 0 in std x3 and in all 734 behavioral modules (simulator).
  - Held by:
    - a GenTests pin asserting the DIVERGENT generated text, so a fix flips it on purpose (the behavioral harness
      cannot hold an expected divergence: `OutputComparisonTests.cs:2189`);
    - a converter `showWarning` driven by go/types selection depth when the shape appears (the
      `visitStructType.go:805` precedent);
    - the predicate joining the release-hop census.
  - The flattened-R4 over-claim shape is listed in §5.3.
- **C4, transitive through an INTERNAL forwarder (review C4).**
  - A forwarder's scope is decided by a NAME heuristic on its return type (`:903-916`). A builtin (`@string`, `nint`,
    `error`...), a tuple or an unnamed type reads `internal`. A foreign harvest then skips it (`:1193`), so the
    promotion vanishes one assembly further on.
  - Real generator: ProtoB's `Mid` gets `internal static @string Name(this Mid)` and
    `internal static (nint, error) Pair(this Mid)`, so `Outer{ProtoB.Mid}` has no `Name`/`Pair`/`String` (Go: all
    three).
  - Census (`internalfwd`): **67 exported rows per target** (78 with unexported). They are on reflect's
    `interfaceType`, `ptrType`, `sliceType`, `structType` and `structTypeUncommon` (10-11 each), and net/http_test's
    `testMockTCPConn` (9) and `cleanupT` (4).
  - `multicross` = **0** on all three targets and in behavioral: every hiding forwarder is a PRE-EXISTING same-package
    one (internal/abi's). None is a forwarder the cut mints. ProtoC's positive control counts 6.
  - Fixing the scope heuristic would change same-package forwarders' scope, which is same-package churn. So C4 is a
    named residual (§5.4), with this census.
  - The cut's OWN new forwarders (`ᴛxpkg`) take the ACCURATE semantic scope (`method.ReturnTypeIsPublic` /
    `ParametersArePublic`, already used at `:911-912` and `:939-940`). So M creates no new vanishing chain.
- **Item 6, generics:** §2.2 R1.
- **Item 7, same-package byte identity.**
  - Feeding metadata into `promotedMethodCounts` can newly mark a same-package forwarder ambiguous (the plant
    `Seven{ sync.Mutex; local }`: Go drops `Lock`, and today it is emitted). **Population 0**: the simulator's
    "removed" set is empty on every target and in behavioral.
  - Any same-package change in the generated-source diff is listed and explained.
- **Item 8, build safety.**
  - On K, CS8032/CS8034/CS8784/CS8785 are ERRORS (`src/Directory.Build.props:58`, pinned by
    `src/go2cs/internal/repoguard/generatorLoadGuard_test.go:34`).
  - Every new path is null-safe, and each of these yields "no promotion", never a throw:
    - `FindUnderlyingStructSymbol` returning null, or given a constructed generic name;
    - a missing `[GoPackage]`;
    - an absent `ᴛxpkg` class;
    - an unresolvable dot-import;
    - item 10's attribute read.
  - GenTests asserts `GeneratorRunResult.Exception` null and its diagnostics empty. The harness ignores both today
    (`src/tests/GenTests/PromotedMetadataEmbedTests.cs:172-177`).
- **Item 9, reflectlite.** rtype's hand-owned methods are in `structMethodNames`:
  - `GetExtensionMethods` scans every syntax tree (`StructDeclarationSyntaxExtensions.cs:502-527`).
  - `Elem`, `String` and `PkgPath` are value-receiver extensions in
    `src/core/internal/reflectlite/type_impl.cs:32, :274, :252`.
  - `NumMethod`, `Key` and `Len` are converted normally (`type.cs:237, :292, :300`).
  - `Kind` is also R2's clash; the sibling class removes it. Gate: reflectlite and reflect BUILD.
- **Item 10, the two arms agree.**
  - The SYNTAX arm (`:730-746`, `:817-862`: a `CompilationReference`, the IDE's view) has no `[GoPackage]` gate today.
    After the cut both arms apply R1-R5.
  - The Go package is read from the embed symbol's containing class's `[GoPackage]` ATTRIBUTE (the metadata arm's
    `GetGoPackageName`, `:1103-1110`). It is never read through a cross-compilation `GetSemanticModel`, which throws
    on a foreign tree.
  - A GenTests PARITY arm runs one shape through `ToMetadataReference()` and through the emitted image, asserting
    identical generated text.

---

## 3. Populations, term by term

**Units.**
- A **row** is (the enclosing type's declaration position, method).
- A **pair** is (package, type NAME, method). Pairs collapse same-named LOCAL types: internal/abi declares a local `u`
  in many functions, 143 under-claim rows alone.
- **T / *T** is the value / pointer method set. "Exported" is the Go name.

**Forwarded rows (windows; linux/darwin in brackets):**

| term | windows | linux / darwin |
|---|---|---|
| exported crossing rows (census) | 784 | 781 |
| − generic metadata embed (`unique.uniqueMap`, §5.5) | 11 | 11 |
| − C4, through a pre-existing internal forwarder (§5.4) | 67 | 67 |
| − R2 skips (sibling class) | 0 | 0 |
| − C3 signature skips | 0 | 0 |
| **= forwarded** | **706** | **703** |

- R3 and R4 drops are NOT inside 784. The census counts Go's own method set, which already excludes what Go deletes.
  They are the 12 drop rows the generator must not add.
- v1's "770 = 784 − 11 − 3" was wrong on two counts:
  - The simulator counted C4 names as visible.
  - It excluded `breakableConn`'s 3 as a box-shim residual. But `sync.Mutex.Lock`/`Unlock` are `[GoRecv] this ref`
    (`src/core/sync/mutex.cs:137, :250`), which promote at any depth; only BOX primaries need the shim.
  - Its 4 R2 rows were inside the 784 as well.
- **Under-claims (simulator):** 312 rows = 309 same-package pre-existing + 3 crossing (breakableConn, the modelling
  error above). As pairs: 191 = 188 + 3.
- **Over-claims:** 10 per target, all pre-existing same-package field shadows (§5.1). The cut adds 0.
- **Same-package forwarder changes:** 0.
- **Behavioral:** 6 crossing modules. `removed` is 0 everywhere. Over-claims are only the same field-shadow class
  (IotaEnum 2, StdLibInternalAbi 7).

---

## 4. The real-generator experiment (scratch, never committed)

Three Go modules, each converted by its tree's own converter and built with its own go2cs-gen (Release, `-m:4`):
- **ProtoA:** `Inner` with `Tok() Token`, `Name() string`, `Pair() (int, error)`, `(*Inner) Set(int)` and
  `String() string`.
- **ProtoB:** `Mid{ProtoA.Inner}`.
- **ProtoC:** `Direct{ProtoA.Inner}`, `Outer{ProtoB.Mid}`, `Five{ProtoB.Mid; loc}`.

`base` is master `f819887fa3`. `proto` is master plus the R1 prototype only: the gate lifted for PUBLIC methods, with
the embed-package function guard skipped across packages.

| probe | Go | base | proto (R1) |
|---|---|---|---|
| `fmt.Println(Direct{}, Outer{})` | `Inner! Inner!` | `{Inner!}`, then Outer as the same struct form nested one level deeper | `Inner! {Inner!}` (C4) |
| value `Direct` has `Set` (anonymous assert) | false | false | false (masks R5) |
| `*Direct` has `Set` | true | false | true |
| `Outer` has `Name` / `Pair` | true / true | false / false | false / false (C4) |
| `Outer` has `Tok` (public named return) | true | false | true |
| `Five` has `Tok` | true | true | **false** (item 5) |
| NumMethod value / pointer of `Direct` | 4 / 5 | 0 / 0 | **5** / 5 (R5) |
| value `MethodByName("Set")` | false | false | **throws** (R5) |
| package var `Name` beside the promoted `Name` | builds | builds | **CS0102** (R2) |

---

## 5. Named residuals (not in TRAIN M)

1. **Same-package field-shadowed forwarders, Go-INCORRECT today** (ruled a follow-up with its own census).

   | forwarder today | the field that shadows it | package |
   |---|---|---|
   | `ArrayType.Elem`, `ArrayType.Len` | `Elem *Type`, `Len uintptr` | internal/abi |
   | `ChanType.Elem` | `Elem *Type` | internal/abi |
   | `PtrType.Elem` | `Elem *Type` | internal/abi |
   | `SliceType.Elem` | `Elem *Type` | internal/abi |
   | `OldMapType.Elem`, `OldMapType.Key` | `Elem *Type`, `Key *Type` | internal/abi |
   | `SwissMapType.Elem`, `SwissMapType.Key` | `Elem *Type`, `Key *Type` | internal/abi |
   | `timeTimer.init` | `init bool` | runtime |

   - Behavioral: IotaEnum 2, StdLibInternalAbi 7.
   - Reach: both packages are in every row's closure. The divergence is observable only where such a value is
     asserted to an interface carrying the name.
2. **Box-primary pointer methods behind a depth >= 2 value hop** (no shim, for same-package embeds too). The census's
   `shim` flag (154 per target) is an UPPER bound: most such methods are `[GoRecv] this ref` and promote. Unchanged by
   M.
3. **Item 5's depth flattening.** Population 0. Pinned by GenTests, warned by the converter, and in the release-hop
   census.
   - Its R4 twin: `Outer{F; G}`, where F promotes M from depth 2 and G has a FIELD M at depth 2. The flattened count
     puts F's M at depth 1, where G's field cannot shadow it, so the forwarder over-claims.
4. **C4: promotions through a pre-existing same-package INTERNAL forwarder.** 67 exported rows per target (reflect's
   abi wrappers, and net/http_test's `testMockTCPConn`/`cleanupT`). The fix is the scope heuristic, which is
   same-package churn.
5. **Generic metadata embeds:** 11 rows (`unique.uniqueMap`, which gains nothing either way: pointer receivers only,
   and no shim for a generic enclosing struct). **The generic-enclosing box-shim gap** (`:999`).
6. **Same-package `[GoRecv]` over-claim** (R5 scoped to cross-package): up to 1013-1017 rows in 125-126 std types.
   For example `PromotedEmbedAnonIfaceWitness`'s `CommonType.String`, with 4 false interface satisfactions there.
7. **Unexported cross-package methods** (R1): 170 per target on windows. They are unobservable, per
   `UnexportedMethodPackageMatches` (`GoMethodSets.cs:239-255`).

---

## 6. Gates for the cut

1. **Reds first**, each red at the base and green at the cut:
   - the Stringer/value/pointer arms (`fmt.Println(s)`, `&s`, `any(s).(fmt.Stringer)`);
   - R5's arms;
   - the io_test `Buffer` shape plus a COMPOSITE-interface arm (`struct{ bytes.Buffer; io.ReadWriteCloser }`), for R3;
   - a field-shadow shape (R4);
   - a var clash and a dot-import clash beside a forwarder (R2, sibling);
   - C3's `Leak() hidden`;
   - §2.1's reflect and json/xml/ConvertibleTo arms;
   - the golib `Value.Elem` red.
   Item 5 is pinned as a DIVERGENCE in GenTests.
2. **The `-tests` EMISSION footprint gate, BEFORE everything else.** The cut REGENERATES committed `*_test.cs`. Today
   `io_test.cs:22-25`'s `ReaderFrom`/`WriterTo` are unmarked, and with the new generator unmarked means minted
   `ReadFrom`/`WriteTo` and a PANIC in `TestCopyLargeWriter`. So:
   - -tests re-emission touches 57 / 59 / 58 files per target;
   - marker-only;
   - positional against the census.
3. `TestCopyLargeWriter` stays PASS; `EmbeddedInterfaceWitness` stays green, its `conflicted` arm included
   (`Reader: no`, `NumMethod 0`); io and io_test BUILD.
4. **GenTests:**
   - `CrossPackageEmbedStaysPublicFieldsOnlyAndMintsNoForwarders` (`:251-267`) flips ON PURPOSE: `ServePublic` gains a
     forwarder, while the friend-granted internal `serveInternal` is STILL refused.
   - Plus the item 10 parity arm, the composite-interface arm and the item 5 divergence pin.
   - Every harness run asserts no generator exception and no diagnostics.
5. reflect, internal/reflectlite, encoding/json, encoding/xml and fmt BUILD.
6. **The generated-source diff is the footprint:**
   - Base and cut run in SEPARATE fresh worktrees, every `Generated/` purged first. `io.csproj` and `io.tests.csproj`
     share one Generated folder (last build wins), so the two are never mixed.
   - Production and test predictions are separate. `go2cs-stdlib.slnx` builds no tests.csproj, so the tests.csproj of
     every package holding a census `_test` type is built as well (io, encoding/json, net/http, net/http/cgi, math/big,
     go/types, runtime, sync, image/draw, net/rpc/jsonrpc, crypto/tls on its lane).
   - The diff is census'd BOTH ways against the §3 rows: 706 windows; the linux reconciliation 703; darwin stated
     unmeasured.
   - Same-package changes: predicted 0, each one listed.
   - Planted control: drop one predicted forwarder; the both-ways census must name it; restore byte-identical.
7. **Behavioral:**
   - the `Generated/` diff shows additions only in the 6 crossing modules and 0 same-package changes;
   - the FULL behavioral run suite, not CNR alone;
   - CNR moves exactly the 15 marker modules' goldens, positionally;
   - `go2cs.slnx` built once.
8. **FLOOR 13, planted runs with each rule OFF,** each restored byte-identical with SHAs and logs cited:
   - R3 off: `TestCopyLargeWriter` PANIC and the Witness reads `Reader: yes`;
   - R4 off: the reflect build fails;
   - R5 off: its arms go red;
   - R2 sibling off, i.e. into the package class: CS0102 in reflect.
9. **Full validated sweep on windows, plus a linux leg.** crypto/tls and net go to their lane. The sweep names the
   must-not-move rows: reflect, internal/reflectlite, encoding/json, encoding/xml, fmt, archive/tar, io, net, os.
   GolibTests x2 with GOROOT pinned; `go test ./...`.

---

## 7. For G and COORD to read

- **R2's sibling class** reverses v1: 0 skips against 26. The name `<Struct>ᴛxpkg` is proposed.
- **R5's scope** is cross-package only, with the same-package over-claim a residual (§5.6). Its census is an upper
  bound.
- **C4** is a residual because its fix is same-package churn. The cut's own forwarders take the accurate scope.
- **The corrected accounting:** 706/703, not v1's 770.
