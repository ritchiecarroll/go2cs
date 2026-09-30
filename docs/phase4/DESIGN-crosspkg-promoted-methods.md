# TRAIN M -- a method promoted through another package's embed, in the RUNTIME method set

**Status:** DESIGN RECORD, no code. It is written for G's second read and COORD's ruling before any cut.
**Lane:** i9
**Date:** 2026-09-30
**Asked for by:** COORD's TRAIN M ruling on i9's sizing post (the go2cs-gen interface-satisfaction item), items 1-10,
plus two later rulings: the field-shadow rule is SCOPED to cross-package promotions, and the same-package forwarders
it would otherwise change are a named residual.
**Tree read:** `master` @ `f819887fa3`; toolchain go1.24.13 / .NET SDK 10.0.401. Every file:line below is at that tree.
**Companion seat:** `claude/i9-crosspkg-value-promotion` @ `8213221317` (TRAIN L), the converter's CALL-SITE half of the
same defect class. This document is the METHOD-SET half.

---

## 0. Headline

> **A Go struct that embeds a struct from ANOTHER package gets none of that embed's methods in its run-time method set,
> so `fmt`, type assertions, type switches and `reflect` answer MISS where Go answers HIT.** `time.Time` inside
> `Stamp{time.Time}` does not make `Stamp` a `fmt.Stringer`. The cause is in the generator, where it is deliberate:
> the metadata harvest mints no forwarder across a Go package boundary. The fix is to lift that gate, but lifting it
> alone is WRONG in three measured places. Go deliberately DELETES some promotions: through an embedded interface
> (io's own test `Buffer`), and through a shallower field (reflect's `ptrType.Elem`). And a minted forwarder shadows
> a dot-imported function (io_test's bare `WriteString(...)`, net/http_test's `Error(...)`). The change therefore
> spans the CONVERTER, which must mark an interface embed so the generator can count it, as well as the GENERATOR,
> which needs four rule changes.
>
> Predicted footprint, from a generator-emission simulator reconciled against a go/types census in both
> directions:
> - **+770 / +767 / +767 forwarder names** (windows / linux / darwin) on the 56 / 55 / 55 std types the sizing named;
> - **0 same-package forwarder changes** in std x3 and in all 734 behavioral modules;
> - **0 over-claims introduced**, and 4 name clashes, all of which the enclosing-scope guard removes.
>
> The marker itself changes converter output: 54 production interface embeds in 35 std files, and 24 in 15
> behavioral modules. It also makes `reflect.StructField.Anonymous` read TRUE for them, which is Go-correct.

---

## 1. The mechanism (confirmed; COORD's G1-G8)

- **The gate.** `StructTypeTemplate.GetMetadataPromotedMethods` (`src/gen/go2cs-gen/Templates/StructType/StructTypeTemplate.cs:1144`)
  returns empty when the embed's `[GoPackage]` differs from the enclosing struct's (`:1154-1157`). Its own comment
  says "a metadata embed promotes FIELDS only". In an MSBuild build every cross-package embed is a METADATA embed
  (a `<ProjectReference>` arrives as metadata), so no cross-package method is ever forwarded.
- **The one method-set source.** golib's `TypeExtensions.GetGoMethodSetCandidates`
  (`src/core/golib/runtime/TypeExtensions.GoMethodSets.cs:302-331`) reads ONLY emitted extension methods. The
  structural probe, the shell binder (`AdapterBinder`), `reflect`'s method table and `builtin`'s assert cache all read
  that one list. An unemitted forwarder is an ABSENT Go method, for T and for *T alike.
- **Seen at the generated source** (the TRAIN L red, `CrossPackagePromotedValueMethod`): `Stamp{time.Time}`,
  `PtrStamp{*time.Time}` and `Outer{inner{time.Time}}` carry an EMPTY "Promoted Struct Receivers" block. The
  same-package control `Wrap{localBase}` gets `Twice(this Wrap)` and `Twice(this ж<Wrap>)`. Go prints the promoted
  `String()` for `fmt.Println(s)`, and `any(s).(fmt.Stringer)` is true; C# is false for both the value and the
  pointer.
- **The pointer method set.** *T adds the embed's pointer-receiver methods. Through a DIRECT value embed that is the
  existing box shim (`IsValueEmbedBoxRecv`: `Ꮡtarget.of(T.Ꮡ<embed>).M()`, `:997-1018`). Through a pointer embed it
  is the box hop (`:1025-1028`). Through a VALUE embed at depth >= 2 there is no shim for same-package embeds either,
  a residual this item leaves alone (§5.2).
- **Transitive promotion needs nothing new.** A foreign embed's own forwarders are public extension methods on ITS
  package class with receiver T, and the harvest (`:1182-1263`) already reads every such method.
- **The interface-implementation bridge is not disturbed.** `GoImplement<rtype, ΔType>`-style adapters resolve an
  interface member through the embed hop by reading SYNTAX
  (`StructDeclarationSyntaxExtensions.GetEmbeddedPointerHopNames`, `:573`). The TypeGenerator's generated forwarders
  are not syntax that generator can see, so the adapter's choice of member is unchanged.

---

## 2. The change

### 2.1 Converter: mark an interface embed (COORD item 1)

The converter emits an embedded INTERFACE as a plain field, identical to a same-named named field:

```csharp
// src/core/io/io_test.cs:22-24 (Go: type Buffer struct { bytes.Buffer; ReaderFrom; WriterTo })
public partial ref bytes_package.Buffer ΔBuffer { get; }
public io_package.ReaderFrom ReaderFrom; // conflicts with and hides bytes.Buffer's ReaderFrom.
public io_package.WriterTo WriterTo;     // conflicts with and hides bytes.Buffer's WriterTo.
```

The generator deliberately treats a field named like its type as NOT an embed (`StructTypeTemplate.cs:851-856`,
dnsmessage's `RCode RCode`), so it cannot tell `ReaderFrom` the embed from `ReaderFrom ReaderFrom` the field.

**Proposal: stamp `[GoEmbedded]` on an interface-embed field.** The attribute already exists
(`src/core/golib/GoEmbeddedAttribute.cs`), and its contract is exactly this case: "a struct field the converter
emitted as a PLAIN field for a Go EMBEDDED field". Today it is applied only to predeclared-type embeds
(`src/go2cs/visitStructType.go:845, :851`). Extending it to interface embeds:

- gives the generator its ambiguity input (§2.2 R3), read off the field symbol. That works from SYNTAX and from
  METADATA, so a foreign embed's own interface embeds are visible too;
- **also changes reflection:** `GoReflect.FieldAccess.cs:419` projects `embedded:` from this attribute, so
  `StructField.Anonymous` turns TRUE for every interface embed. Go says true; today C# says false. The sweep measures
  this. Go's `encoding/json` treats an anonymous INTERFACE field as a named field, so json output does not move.
- **converter footprint (census `ifaceembed`, go/types, deduplicated by position):**
  - std windows / linux / darwin: 54 production interface embeds in 35 files, all three targets; test 136 / 140 / 139
    in 57 / 59 / 58 files;
  - behavioral: 24 embeds in 15 of 734 modules.
  - The -stdlib two-seeded footprint must match the 35 production files exactly, and CNR must move exactly the 15
    goldens.

The alternative, a new generator-only attribute, would keep reflection byte-identical, but it would duplicate a
contract that already exists and leave `Anonymous` wrong. Recommended: `[GoEmbedded]`. **COORD to rule.**

### 2.2 Generator: four rule changes

- **R1. Lift the gate for EXPORTED methods only (COORD item 4 corrects the sizing's G7).**
  - A cross-package metadata embed contributes its PUBLIC extension methods whose Go name is exported, on the
    `PromotesFromMetadata` precedent (`:1120-1127`).
  - The sizing claimed unexported methods would be emitted `internal`. They would NOT: the plain forwarder pair takes
    the STRUCT's scope (`:886`), and the harvest's `IsSymbolAccessibleWithin` (`:1193`) admits friend-granted
    internals. The filter must therefore be explicit.
  - Go promotes an unexported method only toward interfaces in the METHOD's package, and golib's
    `UnexportedMethodPackageMatches` (`GoMethodSets.cs:239-255`) already refuses that for a wrapper. Dropping them
    loses nothing Go could observe.
- **R2. Scope the function guard to the ENCLOSING scope (COORD item 2).**
  - The existing guard (`:1159-1185`) skips a forwarder named like a function in the EMBED's package. That is right
    for the same-Go-package `-tests` split, where that package arrives via `using static`.
  - It is wrong across packages. A foreign package's functions are alias-qualified (`time.Unix(...)`, as in the
    converted jwt `types.cs`), so the guard would silently drop real promotions: `Time.Unix`, `Time.After`.
  - For a cross-package embed, the guard's set is instead: (a) the enclosing package class's own functions (same
    static class, same overload set), and (b) every function of every package the enclosing package DOT-imports
    (`using static`). A same-named class member shadows the import for every bare call, which is the net
    `lookupCustomResolver` CS1501 class.
  - Measured (simulator clash arm, planted and seen to fire): 4 rows per target, all dot-import:
    - `io_test.Buffer`, `dataAndErrorBuffer` and `writeToChecker` would get a `WriteString` forwarder that shadows
      `io.WriteString` (`src/core/io/multi_test.cs:10`, bare calls at `:131`, `:157`);
    - `net/http_test.cleanupT` (embeds `*testing.T`) would get an `Error` forwarder that shadows `http.Error`.
    - Own-package clashes: 0 in std x3 and behavioral.
  - Each such name is SKIPPED, with the same explanatory comment line the other skips emit. Go still promotes it, so
    this is a stated residual (§5.4): an assert against `io.StringWriter` on those three test types still misses.
- **R3. Count embedded-interface providers for ambiguity.**
  - `countPromotedMethods` (`:669-748`) walks struct embeds only. With R1 alone, io_test's `Buffer` would gain
    `ReadFrom`/`WriteTo` forwarders that Go deliberately deletes (a same-depth collision with the `ReaderFrom` /
    `WriterTo` interface embeds).
  - Then `io.Copy` takes a fast path Go keeps off, and **`TestCopyLargeWriter` (`io_test.go:466`) goes PASS -> PANIC**:
    `bytes.Buffer.WriteTo` panics "invalid Write count". This is JOB-010 Shape C again.
  - With the §2.1 marker, each interface embed's methods count at its depth. They never MINT a forwarder: the
    converter's promoted-interface records own those.
- **R4. Field shadowing, CROSS-PACKAGE ONLY (COORD's ruling).**
  - A promoted method is dropped when a FIELD of T at a shallower-or-equal Go depth has its name (Go's selector
    rule). The generator checks only DECLARED method names (`structMethodNames`, `:623-634`).
  - Measured: 8 reflect drop rows, all over internal/abi:
    - `interfaceType`: `InterfaceType`
    - `ptrType`: `Elem`
    - `sliceType`: `Elem`
    - `structType`: `StructType`
    - `structTypeUncommon`: `StructType`
  - Predicted by reading, and settled by the red: without R4 the forwarder body `target.PtrType.Elem()` also fails to
    compile, because C# member lookup finds the non-invocable FIELD `Elem` and never falls back to the extension.
  - Applied only to names harvested from a cross-package embed, so same-package output stays byte-identical. The same
    rule applied globally would change 10 pre-existing same-package forwarders. Those are a separate residual
    (§5.1).

### 2.3 Items 5-7, 9, 10 and 8

- **Item 5, the harvest flattens depth.**
  - `GetMetadataPromotedMethods` takes every extension on the embed's package class with receiver T, and the
    generated forwarders live in that same class. So a foreign embed's OWN promotions count at the embed's depth.
  - That can annihilate a local method Go promotes from deeper, or beat one it should lose to. The plant `Five{
    bufio.ReadWriter; loc }` (loc declares `ReadString`) reproduces it: Go promotes `loc.ReadString` (depth 1)
    over ReadWriter's (depth 2), while the flattened count calls it ambiguous and drops it.
  - **Population: 0** in std x3 and in all 734 behavioral modules. Metadata cannot tell a declared method from a
    generated forwarder without a new stamp on every forwarder, which would churn every same-package generated file.
  - Proposed: keep the flattening, gate it with the plant as a documented known-divergence arm, and name it (§5.3).
    **COORD to rule** whether a forwarder depth stamp is wanted instead.
- **Item 6, generics.**
  - `GetEmbedTypeArgumentMap` (`:1330-1350`) resolves type parameters from SYNTAX only, so a METADATA generic embed
    would keep its declaration's parameter names (CS0246).
  - Scope: **exclude generic metadata embeds** (an embed type carrying type arguments) from R1, as a named residual
    (§5.5).
  - The census's only generic crossing type, `unique.uniqueMap` over `HashTrieMap`, gains nothing even without the
    exclusion: all its methods are pointer receivers, and the box shim skips generic enclosing structs (`:999`).
- **Item 7, same-package byte identity.**
  - Feeding metadata methods into `promotedMethodCounts` can newly mark a same-package forwarder ambiguous (plant
    `Seven{ sync.Mutex; local }` where local declares `Lock`: Go drops `Lock`, and today the generator emits it).
  - **Population: 0** in std x3 and behavioral (the simulator's "removed" set is empty on every target). Any
    same-package change in the generated-source diff is a finding.
- **Item 9, reflectlite.** rtype's hand-owned methods are seen by `structMethodNames`:
  - `GetExtensionMethods` scans every syntax tree of the compilation (`StructDeclarationSyntaxExtensions.cs:502-527`).
  - `Elem`, `String` and `PkgPath` are value-receiver extensions in `src/core/internal/reflectlite/type_impl.cs:32, :274, :252`.
  - `NumMethod`, `Key` and `Len` are converted normally (`type.cs:237, :292, :300`).
  - So none gets a duplicate forwarder. `Size`/`Kind` etc. from `*abi.Type` are new forwarders; the `GoImplement<rtype,
    ΔType>` bridge is unaffected (§1). Gate: reflectlite and reflect BUILD.
- **Item 10, the two arms agree.**
  - The SYNTAX arm (`:730-746`, `:817-862`: an embed resolved through a `CompilationReference`, which is how an IDE
    build sees a project reference) has no `[GoPackage]` gate today, so IDE and MSBuild output already differ.
  - After the cut both arms apply one rule set, R1-R4. The Go package of a syntax-resolved embed is read from its
    containing package class's `[GoPackage]` through that compilation's semantic model, the identity the metadata arm
    already uses.
- **Item 8, build safety.**
  - On TRAIN K's union (`4016a2269c`), CS8032/CS8034/CS8784/CS8785 are ERRORS (`src/Directory.Build.props:58`), and
    `src/go2cs/internal/repoguard/generatorLoadGuard_test.go:34` pins them. The cut lands on K, so a generator
    exception is a hard build error.
  - Every new path is null-safe: `FindUnderlyingStructSymbol` returning null, a constructed generic name, a missing
    `[GoPackage]`, or an unresolvable dot-import all yield "no promotion", never a throw.
  - GenTests adds one arm per shape with an unresolvable input.

---

## 3. Populations (read-only; the tools and outputs are kept with the lane's logs)

- **Method-set census** (go/types, T and *T, concrete promoted methods with a package-crossing hop; production and
  test variants; planted on 4 arms): std windows / linux / darwin, 923 / 926 / 922 packages, 0 load errors.
  - 56 / 55 / 55 types; 954 / 951 / 951 methods, of which 784 / 781 / 781 are exported.
  - 82 well-known-interface rows, identical on all targets.
  - Behavioral: 6 of 734 modules.
  - (Type list, interface rows and corpus rows: the sizing post.)
- **Generator-emission simulator** (models today vs the cut vs Go; planted on 7 arms, each seen to fire):
  - "added": 770 / 767 / 767. This reconciles EXACTLY with the census's exported 784 / 781 / 781, minus
    `unique.uniqueMap` 11 (§5.5) and `net/http_test.breakableConn` 3 (§5.2).
  - "removed" (same-package changes): 0 / 0 / 0; behavioral 0.
  - "over-claims": 10 per target, all pre-existing and all §5.1; behavioral 9 (IotaEnum 2, StdLibInternalAbi 7, the
    same class).
  - "under-claims": 312 rows per target. 188 (type, name) pairs are pre-existing same-package residuals the cut does
    not touch, plus breakableConn's 3.
  - Clashes: 4, all handled by R2.
- **Interface-embed census** (the §2.1 converter footprint): as stated in §2.1.

---

## 4. Gates for the cut

1. **Red first.** Extend `CrossPackagePromotedValueMethod` or add a sibling behavioral. Each arm is red at the base
   and green at the cut:
   - `fmt.Println(s)`, `fmt.Println(&s)` and `any(s).(fmt.Stringer)`: today C# is false/false, Go is true/true;
   - the io_test `Buffer` shape (interface-embed ambiguity, R3);
   - a reflect-style field-shadow shape (R4);
   - a dot-import clash shape (R2).
   Plus the item-5 plant as a stated known-divergence arm.
2. **io's `TestCopyLargeWriter` must STAY PASS**, and io and io_test BUILD (R2, R3).
3. **Behavioral `EmbeddedInterfaceWitness` stays green**, its `conflicted` arm included (Reader: no, NumMethod 0).
   That is R3's shape.
4. **GenTests `PromotedMetadataEmbedTests.CrossPackageEmbedStaysPublicFieldsOnlyAndMintsNoForwarders` (`:251-267`)
   flips ON PURPOSE:** `ServePublic` gains a forwarder, and the friend-granted internal `serveInternal` is STILL refused
   (R1).
5. **reflect and internal/reflectlite BUILD** (items 9, R4).
6. **The generated-source diff is the footprint.** The converter's emission changes only by the §2.1 marker, and
   go2cs-gen changes at BUILD time. So:
   - full `go2cs-stdlib.slnx` build on windows, base vs cut;
   - diff every `Generated/` tree, census'd BOTH ways against the 56 types: every added forwarder on a listed type,
     and every listed type gains its predicted names less R2/R4 skips;
   - **same-package changes: predicted 0**, and any change is listed and explained.
7. The **-stdlib two-seeded footprint** equals the marker's 35 production files (windows), and **CNR** moves exactly
   the 15 behavioral goldens (or 0, if COORD rules the generator-only attribute).
8. **Full validated sweep on windows plus a linux leg.** crypto/tls and net rows go to a lane that may run them.
   GolibTests x2 with GOROOT pinned, and `go test ./...`.

---

## 5. Named residuals (not in TRAIN M)

1. **Same-package field-shadowed forwarders, Go-INCORRECT today (COORD: a separate follow-up with its own census).**
   The generator emits a forwarder for a method Go hides behind a shallower FIELD of the same name:

   | forwarder today | the field that shadows it | package |
   |---|---|---|
   | `ArrayType.Elem`, `ArrayType.Len` (from `*Type.Elem`, `*Type.Len`) | `Elem *Type`, `Len uintptr` | internal/abi |
   | `ChanType.Elem` | `Elem *Type` | internal/abi |
   | `PtrType.Elem` | `Elem *Type` | internal/abi |
   | `SliceType.Elem` | `Elem *Type` | internal/abi |
   | `OldMapType.Elem`, `OldMapType.Key` | `Elem *Type`, `Key *Type` | internal/abi |
   | `SwissMapType.Elem`, `SwissMapType.Key` | `Elem *Type`, `Key *Type` | internal/abi |
   | `timeTimer.init` (from `(*timer).init`) | `init bool` | runtime |

   - Behavioral carries the same class: IotaEnum 2, StdLibInternalAbi 7.
   - Reach: internal/abi and runtime are in every row's import closure. The divergence is observable only where a
     value of one of these types is asserted to an interface carrying the name. The follow-up's census counts those
     sites; none is known today.
2. **A pointer-receiver method through a VALUE embed at depth >= 2** (no box shim, for same-package embeds too).
   - Census: 154 methods per target, for example `net/http_test.breakableConn` ← `*brokenState` ← `sync.Mutex`
     (`Lock`/`TryLock`/`Unlock`, the only crossing under-claim).
   - Unchanged by M.
3. **Item 5's depth flattening**, population 0, kept as a stated known-divergence arm.
4. **R2 skips.** `io_test`'s three `WriteString` and `net/http_test.cleanupT.Error` stay absent from the method set.
   Go keeps them, and minting them would break the build.
5. **Generic metadata embeds** (item 6): not harvested. Census population is `unique.uniqueMap`, which gains nothing
   either way.
6. **Unexported cross-package methods** (R1): 170 per target on windows. They are unobservable, per
   `UnexportedMethodPackageMatches`.

---

## 6. What COORD rules

- The §2.1 marker: `[GoEmbedded]` (recommended; reflection moves in Go's direction) or a new generator-only attribute
  (reflection byte-identical, CNR 0).
- Item 5: keep the flattening as a known-divergence arm (recommended; population 0), or stamp forwarders with their
  depth (a same-package generated-code churn).
- The size: TRAIN M as ruled. It is one converter change (the marker), four generator rules, and the gates above.
