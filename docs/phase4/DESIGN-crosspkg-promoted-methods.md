# TRAIN M -- a method promoted through another package's embed, in the RUNTIME method set

**Status:** DESIGN RECORD **v3.1**: v3 (`4deec86a26`, ACCEPTED WITH CONDITIONS) plus the AMENDMENT below, for G and
COORD to spot-read by its diff before the CROSS-PACKAGE seat is cut (TRAIN M, after L). There is no code in this file.
The census tools it cites are committed beside it, in `src/tools/crosspkg-census` (M6).
**Lane:** i9
**Date:** 2026-09-30
**Tree read:** `master` @ `f819887fa3`; every file:line is at that tree. Two files moved on TRAIN K's union
`4016a2269c`, and both lines are cited (m6): `reflect/value_impl.cs` Anonymous `:2763` -> `:2778`, identity `:3184` ->
`:3199` (`:833` is unchanged). Item 8's two K-only files are cited on K. Toolchain go1.24.13 / .NET SDK 10.0.401.
**Sequencing:** M cuts on L's LANDED master. The red `CrossPackagePromotedValueMethod` lives on L's `8213221317`.
**Companion seat:** `claude/i9-crosspkg-value-promotion` (TRAIN L) is the converter's CALL-SITE half of the same defect
class. This document is the METHOD-SET half.

## What v3.1 amends (the conditions of the v3 ruling)

Every figure below is from the committed tools, run PINNED (go1.24.13; `toolchain@` absent from the output), or from
the scratch prototype (§5).

1. **The §5.6 crash class is a SEPARATE seat, not this one (§6.6 rewritten).** v3's 490 / 498 / 494 counts every row
   Go SEES, not the by-ref, non-`[GoRecv]` forwarders the generator EMITS (`os.fileWithoutWriteTo`: 4 emitted vs 38
   counted). It is one class with two halves: SameP (pointer hop) and SameV (value hop, the 1013-1017 over-claim
   rows). The seat's first deliverable is the census of what is emitted.
2. **Refinement (b) is the cut's rule; its premise is now stated (§2.2b).** Measuring it found a defect, now fixed:
   FIELDS must count separately from the method surface. Otherwise (b) mints `reflect.ptrType.Elem` and
   `sliceType.Elem` through internal/abi's PRE-EXISTING field-shadowed forwarder. The fixed (b) adds exactly
   `net/http_test.cleanupT.Chdir/Setenv` over literal (i').
   - **Gate 6 is re-predicted: 706 / 703 / 703.** The row-for-row join is exact in both modes: literal 704 / 701 / 701,
     and (b) 706 / 703 / 703, with 0 rows on either side on every target.
   - The plant `bfield` (fields merged into the surface) makes `OuterFS` inherit the shadowed `Tok`.
3. **The M2 name is defined (§2.2).**
   - `{pkg}` is the EMITTED package-class stem, so `http` and `http_test` differ.
   - `{Struct}` is the EMITTED type name, lifted locals included, so it MAY contain `ᴛ` (`sizeTestsᴛ1`).
   - Uniqueness rests on package stems NEVER containing `ᴛ`, which makes the first `ᴛ` the stem boundary. Census:
     **0** std `.go` files contain `ᴛ`, and the behavioral corpus has **0** package or type declarations with it (27
     files use it, all in comments).
4. **Completeness:**
   - S1, S3, S4 and S5 are real-generator arms. S1/S3/S4 are NAMED DIVERGENCES exactly as the ruling predicted (§6.3);
     S5 is Go-equal.
   - Also Go-equal: a depth >= 1 FIELD inside a metadata subtree (`OuterFD`), the field-shadow propagation arm
     (`OuterFS`), and the hand-owned arm (`struct{ *testing.T }`, value `Error` true).
   - The transit figure is given (§6.4). m3 is widened (`xtransit`: **0**).
   - The shared collector is answered (§2.2), and so is the GolibTests guard's scope with its plant (§7.5).
   - Every plant names its line or site (§5.2).
   - The join is committed (`scripts/reconcile.sh`; a CROSS-CHECK, since the two tools share `treeOccurrences`).
   - m4's twin count is an actual census (**0**). v2's depth-flattening obligations are RETIRED (§3). m7 and m8 are
     quoted (§3).

## What v3 changes

The core stands as ruled: lift R1's gate, the `[GoEmbedded]` marker with the `Value.Elem` read-only fix, and a SIBLING
class. Everything below was built and measured in a scratch PROTOTYPE of the whole design: the real converter and the
real generator, run against Go's own output (§5).

- **Item 5 is now G's uniqueness rule (i')**, and it is not a residual: the prototype is Go-equal on G's
  counterexample, S2, S6, the within-embed variant and the field shadow. (i') needed two refinements, both measured
  (§2.2): it walks a METADATA embed's whole subtree through its inline `ʗ` fields, and it splits that embed's surface
  between the sibling class and the package class. The forwarder count is **704 / 701 / 701 = 706 / 703 / 703 − k,
  with k = 2** (§4).
- **B1, pointer-hop forwarders.** A cross-package pointer-hop forwarder of a ref method is emitted BY VALUE. The
  real-generator probes are Go-equal for PtrStamp, PtrDirect and `struct{*bytes.Buffer}`.
- **The §5.6 same-package class, MEASURED:**
  - 490 / 498 / 494 rows in 37 / 38 / 38 std types;
  - at run time, a `reflect` method walk, `MethodByName` or `Method(i).Call` KILLS THE PROCESS (the exception escapes
    Go's `recover`);
  - a dynamic assert answers a wrong MISS.
  It is not reachable from any banked row by construction. COORD rules whether it joins M (§6.6).
- **B2, R5's input.**
  - `MethodInfo.IsGoRecv` is read from attributes; a direct foreign method is a pointer receiver if it is ref-kind or
    `[GoRecv]`; a harvested sibling forwarder is read by its own `[GoRecv]` stamp.
  - `PathHasPointer` is threaded through collect and count.
  - v2's C15 sentence ("gated on `typeName == promotedStructType`") was FALSE at the tree, and is corrected (§2.2).
- **M2, the sibling class:** `public static class {pkg}ᴛ{Struct}ᴛxpkg`, with qualified receivers. It is harvested from
  the EMBED's own assembly (`ContainingNamespace.GetTypeMembers`), not by `GetTypeByMetadataName`.
- **M3:** a mixed path (a same-package hop, then a cross one) goes to the sibling class. It is in the prototype.
- **M4:** v1's scoping of the embed-package function guard is RESTORED (same-package only). The plant that turns it
  off loses `Time.Unix/After/Date/UnixMicro/UnixMilli`.
- **M5, gates:** rewritten (§7). All floor-13 plants were RUN on the prototype; each names its diagnostic and site or
  its flipped line (§5.2).
- **M6:** the census tools are committed, path-free. Every population was re-derived from the committed copies, and
  the simulator reconciles with the census SET-for-SET (§4).
- **M7:** CompilationReference transitive promotion is a named residual with a GenTests pin (§6.8).
- **m1:** `CrossPkgUser.holder` is a generic METADATA embed, so the generic-enclosing population is **0 / 0**.
- **m3:** census 0 (§3).
- **m4:** both censuses (§3). The frame probe is Go-equal: no `ᴛxpkg` frame surfaces.
- **m5:** `Generated` is captured per project (§7).
- **m7 / m8:** v2's §4 instance was measured in the package-class layout, and all of it is re-measured here.

---

## 0. Headline

> **A Go struct that embeds a struct from ANOTHER package gets none of that embed's methods in its run-time method
> set.** On master, `Direct{ProtoA.Inner}` has NumMethod 0 / 0 (value / pointer) where Go says 4 / 5.
>
> The design is one converter change (`[GoEmbedded]` on an interface embed), one golib fix (`Value.Elem`'s read-only
> flag), and five generator rules:
> - **R1:** exported-only, with the function guard scoped;
> - **(i'):** uniqueness across the whole tree;
> - **R5:** `[GoRecv]` on value-hop pointer methods;
> - **B1:** by-value pointer-hop forwarders;
> - **C3/C4:** semantic scope.
> Cross-package forwarders go into a public sibling class.
>
> The scratch prototype of exactly that is **IDENTICAL TO GO**, byte for byte, on 14 shapes and every probe in §5.
> Every floor-13 plant flips something named.
>
> Footprint, re-derived from the committed tools: **+706 / +703 / +703** forwarded rows under refinement (b) (v3.1;
> literal (i') gives 704 / 701 / 701) (windows / linux / darwin), 0
> same-package forwarder changes, and 0 over-claims added. The converter's marker changes 54 production interface
> embeds in 35 std files, 136-140 test embeds in 57-59 files, and 24 embeds in 15 behavioral modules.

---

## 1. The mechanism (confirmed)

- **The gate.** `StructTypeTemplate.GetMetadataPromotedMethods` (`src/gen/go2cs-gen/Templates/StructType/StructTypeTemplate.cs:1144`)
  returns empty for a foreign `[GoPackage]` (`:1154-1157`). Every cross-package embed is a metadata embed in an MSBuild
  build.
- **The one method-set source.** golib's `GetGoMethodSetCandidates` (`src/core/golib/runtime/TypeExtensions.GoMethodSets.cs:302-331`)
  reads only emitted extension methods. The registry finds every sealed, non-nested, non-generic static class with no
  name filter (`TypeExtensions.ExtensionMethodRegistry.cs:113-114`, `:169-181`), so a sibling class is found.
- **The two hinges R5 and B1 turn on.**
  - The value-set filter (`GoMethodSets.cs:312-321`) admits a `this ref T` receiver unless it carries `[GoRecv]`. Its
    comment, that such a receiver "does not exist in emitted code", is false: the template emits exactly that
    (`StructTypeTemplate.cs:947`, `:1026-1030`; there is no `[GoRecv]` emission).
  - golib's binder skips every by-ref receiver (`AdapterBinder.cs:203-204`), and a reflect method value of one THROWS
    (`GoReflect.MethodSets.cs:512-517`, via `MakeDelegateType` `:620`).
- **The generated source today.** The TRAIN L red's `Stamp`, `PtrStamp` and `Outer` carry an empty
  "Promoted Struct Receivers" block; the same-package control `Wrap` gets its `Twice` pair.

---

## 2. The change

### 2.1 Converter: `[GoEmbedded]` on an interface embed (ruled), the golib fix, and the consumers

- **The stamp.** `src/go2cs/visitStructType.go` stamps `[GoEmbedded]` on an embedded-INTERFACE field, as it already
  does for a predeclared embed (`:845`, `:851`). Two docs that say "predeclared only" are amended:
  `src/core/golib/GoEmbeddedAttribute.cs:11-17` and `GoReflect.FieldAccess.cs:417-418`.
- **What the generator reads.** It treats a `[GoEmbedded]` field as an interface PROVIDER only when its type symbol is
  `TypeKind.Interface`; predeclared embeds carry the same attribute.
- **The golib fix, same seat.** `reflect/value_impl.cs:833` `elem.flag |= (flag)(v.flag & flagRO);` becomes Go's
  `elem.flag |= v.flag.ro();` (Go `value.go:1232`, `:89-94`). With the marker, an unexported interface embed gets
  `flagEmbedRO`, and `Elem` would otherwise carry it verbatim, so `CanInterface()` would read true where Go says false.
  Red: `reflect.ValueOf(w{E{}}).Field(0).Elem().Field(0).CanInterface() == false` for `type w struct{ error }`, plus
  `%#v` of `w{E}` with `E.GoString`.
- **Every Anonymous / Embedded consumer**, each turning Go-correct:
  - `GoReflect.FieldAccess.cs:419` (the projection) and `:451` (the reorder);
  - `GoReflect.ValueMarshalling.cs:464-466` (convertibility demands equal `Embedded`). This is a NEW refusal, but it is
    reachable only through `reflect`: Go's compiler already rejects the static conversion. Static census 0; run-time
    arms gate it.
  - `GoStructSynthesis.cs:310` and `:363`;
  - `reflect/value_impl.cs:1111`, `:2763` (K `:2778`) and `:3184` (K `:3199`);
  - `internal/reflectlite/type_impl.cs:212`;
  - `encoding/xml`'s typeinfo (Go newly admits an unexported anonymous field);
  - `reflect.VisibleFields`;
  - `encoding/json`, which does not move: an anonymous INTERFACE field is a named field to it.
- **Behavioral arms.**
  - Anonymous is true for an exported and for an unexported interface embed (PkgPath `main`).
  - json with a named key, and with omit.
  - xml over an unexported interface embed.
  - `ConvertibleTo` against a `StructOf` twin and against a named-field twin.
- **The marker's footprint** (`cmd/ifaceembed`, re-derived; production / test by file name):

  | target | interface embeds | composite | production | test |
  |---|---|---|---|---|
  | windows | 190 | 20 | 54 in 35 files | 136 in 57 files |
  | linux | 194 | 20 | 54 in 35 files | 140 in 59 files |
  | darwin | 193 | 20 | 54 in 35 files | 139 in 58 files |

  - Behavioral: 24 embeds in 15 of 734 modules.
  - Crossed against the 157 whole-file `[module: go.GoManualConversion]` files: **0** hits (the planted control hits).
  - Every CNR, `-stdlib` and `-tests` hunk is exactly a `[GoEmbedded] ` prefix on ONE interface-embed line, matched
    position for position to this census.

### 2.2 Generator

- **Placement (M2).**
  - **v3.1, the name's definitions (ruled).** `{pkg}` is the EMITTED package-class stem, what precedes `_package`, so
    an internal-test `http` and an external-test `http_test` differ (the dumpConn CS0101 case). `{Struct}` is the
    EMITTED type name, lifted locals included (internal/abi's `u`; a lifted `sizeTestsᴛ1`), so it may contain `ᴛ`.
    Uniqueness rests on package stems NEVER containing `ᴛ`, which makes the first `ᴛ` the stem boundary. Census: 0 std
    `.go` files contain `ᴛ`; the behavioral corpus has 0 package or type declarations with it (its 27 uses are all in
    comments about emitted C#).
  - Cross-package forwarders go into `public static class {pkg}ᴛ{Struct}ᴛxpkg` under the package namespace. `{Struct}`
    is the name without generic arity or `@`; receivers are qualified `global::{ns}.{pkg}_package.{Struct}`.
  - The class is PUBLIC, because the harvest skips inaccessible methods (`:1193`). Each forwarder takes the struct's
    scope.
  - The harvest reads a foreign embed's sibling class from the EMBED's own assembly
    (`packageClass.ContainingNamespace.GetTypeMembers(...)`), keeping the receiver-equality check (`:1198-1202`). A
    name found there is a cross-package promotion whatever the embed's own package is.
  - Same-package forwarders stay in the package class, byte-identical. Suppression comments for cross names (`:869`,
    `:875`, `:881`) are OMITTED.
- **R1, exported only, with the guard scoped (M4).**
  - A cross-package method is harvested when it is public AND its Go name is exported. The Go name is the emitted name
    with the `Δ` marker and an `@` escape stripped (C15, `src/gen/go2cs-gen/Common.cs:557-558`).
  - The embed-package function guard (`:1170-1185`) applies ONLY when the embed's `[GoPackage]` equals the enclosing
    struct's. Across packages it would drop `Time.Unix/After/Date/UnixMicro/UnixMilli`, which the plant shows (§5.2).
  - Guard-skips term: 0.
- **C15, corrected.** `pointerEmbedTypeNames` is consulted UNGATED at `:721` and `:805` (keyed by simple name, `:645-647`).
  v2 said otherwise, and that was false. The prototype leaves those same-package sites as they are (byte identity) and
  decides cross-package box promotion from the threaded `PathHasPointer` instead (B2).
- **(i') G's uniqueness rule.** A name reached through a cross-package path mints ONLY when it is unique across the
  enclosing struct's WHOLE tree: methods at any depth (syntax and metadata), interface providers (the marker plus
  `AllInterfaces`), and fields at any depth. Otherwise it neither mints nor COUNTS, so master's counter sees exactly
  master's population. Measured refinements:
  - **(a) The metadata subtree.** A metadata embed is walked through its INLINE `ʗ<Embed>` fields. An UNEXPORTED embed
    has no accessor property at all: ProtoB's `Mid2` metadata shows `ʗloc2` and no `loc2`. Walking accessors missed it.
  - **(b) The surface split.** A foreign assembly's INTERNAL methods are never imported (`loc2.Tok`, emitted
    `internal static`, is invisible), so a package-class surface name cannot be told from a same-package forwarder of
    an invisible provider. The sibling class settles it:
    - a `ᴛxpkg` name counts 1, since it was already proven unique where it was minted;
    - a package-class name counts 1 plus every CROSS-package or interface provider below it (master's counter never
      saw those).
    Without (b), `Within{ProtoB.Mid2}` inherited ProtoB's PRE-EXISTING Go-incorrect `Mid2.Tok` as a new cross addition.
    The prototype showed exactly that before (b).
    - **v3.1, PREMISE (ruled).** (b) is regression-free RELATIVE TO the foreign generator's surface names being
      Go-correct IN NAME within the embed. Literal (i') could not claim more, because foreign INTERNAL methods are never
      imported.
    - **v3.1, FIELDS count separately (a defect found by measuring (b), now fixed in the prototype and in `cmd/sim -b`).**
      A field is its own occurrence and never joins the method surface. Go forbids a field and a method of one name at
      one level, so a surface method beside a same-named field can only be a forwarder of a SHADOWED method: internal/abi
      `PtrType.Elem`, the §6.1 pre-existing over-claim, which violates the premise. With the two merged, (b) minted
      `reflect.ptrType.Elem` and `sliceType.Elem`, which Go drops. Separated, they count 2.
      - The arms that prove (b) does not propagate the Seven-type or the field-shadow surface are `Within` (Seven-type)
        and `OuterFS` (`PS{inner3; Tok int}`, whose master forwarder `PS.Tok` is field-shadowed). Both are Go-equal.
      - The plant `bfield` (fields merged back) makes `OuterFS` inherit `Tok`.
      - The cut carries both as GenTests arms.
    - **v3.1, re-prove trigger.** If §6.2's same-package counter ever changes from DROP to MINT, (b) must be re-proved:
      its package-class mirror assumes master DROPS a same-depth same-package ambiguity.
  - **(c) The literal and refined rules differ in one std case.** At the Go level the census counts every declaration,
    so `testing.T`'s `Chdir`/`Setenv` shadowing its embedded `common`'s are 2 occurrences. That is `k = 2`
    (`net/http_test.cleanupT`). Under (b) a package-class name that mirrors SAME-package providers below counts 1, so it
    mints those 2, which is Go-correct.
    - **v3.1:** (b) is the cut's rule, so the forwarded count is **706 / 703 / 703**, not 704 / 701 / 701.
      `cmd/sim -b` adds exactly those 2 rows over the literal run, and the row join is exact in both modes (§4).
  - **v3.1, ONE collector?** No. The uniqueness walk (`BuildTreeNameCounts`) is separate from the collector
    (`collectPromotedMethods`) and the counter (`countPromotedMethods`). Their disagreement cannot over-claim, unlike
    the rejected (i):
    - the counter never sees a cross name, so master's same-package answer is untouched;
    - a cross name mints only if the collector offered it AND the walk counts it exactly once;
    - the walk's view contains the collector's (it visits every embed the collector visits, plus fields and interface
      providers);
    - so any disagreement can only REFUSE a name: master's MISS.
  - (i') subsumes R3 (interface providers), R4 (field shadow) and v2's section-5.3 twin. It is a harvest-side filter:
    ~20-40 lines in the cut, about 150 in the prototype with its walkers. No golib change.
- **R5 and B2.**
  - `MethodInfo` gains `IsGoRecv`, set from `GetAttributes()` (symbol) and from the attribute lists (syntax). A direct
    foreign method is a POINTER receiver when `RefKind == Ref` OR it is `[GoRecv]`; a hand-owned `this ref T` without
    the attribute (`testing.cs:231`) is covered by the ref-kind arm.
  - `PathHasPointer` is threaded through collect and count, so a forwarder knows whether any hop on its path is a
    pointer.
  - A cross-package pointer-receiver method whose path holds NO pointer is forwarded `[GoRecv] this ref T`, which keeps
    it out of the value set.
- **B1, by value through a pointer hop.** When a pointer hop is on the path, the method IS in Go's value set. The
  forwarder is emitted by value: `this T target => target.X.Value.M(...)`, with the mutation going through the pointer.
  It binds in `AdapterBinder` and as a reflect method value. No forwarder of a cross-package promotion is by-ref and
  unmarked, and a GolibTests guard asserts no by-ref, non-`[GoRecv]` method reaches `GetGoMethodSetEntries`.
- **C3 / C4 scope.** A cross forwarder takes the SEMANTIC scope (`ReturnTypeIsPublic && ParametersArePublic`). An
  inaccessible signature is SKIPPED: the C3 census is 0. So the cut creates no new vanishing chain; `multicross` is 0
  in std x3 and behavioral, and its positive control counts.
- **M3, the mixed path.** `Outer{localMid{foreign.Inner}}`: any crossing hop sends the forwarder to `ᴛxpkg` (`IsCrossPackage`).
  The prototype's `Mixed` is Go-equal, and a package var `Name` beside it no longer collides (R2, via the sibling class).
- **Generics (C14, m1).** The exclusion tests the SYMBOL: `TypeArguments.Length > 0` after unwrapping `ж<>`. The
  generic-ENCLOSING-over-non-generic-foreign-embed population is **0 / 0**. A GenTests fixture with one unconstrained
  and one constrained `T` copies the where-clauses onto the forwarders (`:959-960` emits none today). The
  generic-enclosing box-shim gap (`:999`) is named.

---

## 3. Items 6-10 and the m-items

- **Item 7, same-package byte identity:** the simulator's `removed` is **0** on every target and in behavioral. Any
  same-package change in the generated diff is a STOP.
- **Item 8, build safety.**
  - On K, CS8032/CS8034/CS8784/CS8785 are errors (`src/Directory.Build.props:58`;
    `src/go2cs/internal/repoguard/generatorLoadGuard_test.go:34`).
  - Every new path yields "no promotion" rather than a throw on any of: a null symbol, a constructed generic name, a
    missing `[GoPackage]`, an absent sibling class, a missing `ʗ` field type, item 10's attribute read.
  - GenTests asserts `GeneratorRunResult.Exception` null and its diagnostics empty, which the harness ignores today
    (`PromotedMetadataEmbedTests.cs:172-177`).
- **Item 9, reflectlite.** rtype's hand-owned `Elem`, `String` and `PkgPath` (`type_impl.cs:32`, `:274`, `:252`) and its
  converted `NumMethod`, `Key` and `Len` (`type.cs:237`, `:292`, `:300`) are in `structMethodNames`. The `Kind` clash
  is gone with the sibling class. reflect and reflectlite BUILD is a gate.
- **Item 10 and M7.**
  - `[GoPackage]` is read from the symbol's attributes (`GetGoPackageName`, `:1103-1110`), never through a
    cross-compilation `GetSemanticModel`.
  - A GenTests PARITY arm (one shape through `ToMetadataReference()` and through the emitted image) must produce
    identical text.
  - The syntax arm matches receivers by exact text (`StructDeclarationSyntaxExtensions.cs:621-634`), which a QUALIFIED
    sibling receiver never matches. So TRANSITIVE promotion through a CompilationReference (the IDE's view) is a named
    residual (§6.8), with a GenTests pin of the divergent text.
- **m3.** The adapter's single-value-embed static hop (`ImplementGenerator.cs:1423`) calls the embed's PACKAGE class
  and cannot reach a `ᴛxpkg` forwarder. Its population is the rows reaching a method THROUGH an embed's own cross
  forwarder: `multicross` = **0** (std x3, behavioral).
  - **v3.1, widened (ruled).** `multicross` counted only inside the non-public-return branch, while the cut mints
    exactly the PUBLIC-shape intermediates into `ᴛxpkg`. The census's new `xtransit` counts every exported row whose
    path crosses packages at two or more hops, whatever the return shape: **0** on all three targets. ProtoC is the
    positive control, at 16.
- **m4, frame names.** `GoSyntheticPC` derives a frame's package from a `<pkg>_package` class (`:185-191`). The probe
  calls a `runtime.Caller(1)` method directly, through an assert on `Direct`, and through one on the transitive
  `Outer`. It reports the calling closure exactly as Go does: no `ᴛxpkg` frame surfaces. The only difference is the
  pre-existing `main.Main` vs `main.main` naming, which the forwarder-free direct call shows too.
- **m4, twins.** A (struct, method) pair holding both a `ᴛpromoted` twin and a `ᴛxpkg` forwarder is **0 by
  construction**: an interface provider counts under (i'), so a name it provides is never unique beside a cross one.
  - **v3.1, as an actual census (`twin`):** **0** on all three targets.
  - The arm cannot fire in Go's own semantics, and that is stated rather than planted. A direct interface embed
    providing a crossing row's name either TIES the cross method at depth 1 (ambiguous, so no row) or BEATS a deeper
    one (so not a crossing row).
- **v3.1, depth flattening RETIRED.** Under (i') the item-5 shape no longer regresses. v2's depth-flattening
  obligations are retired: the converter `showWarning`, the release-hop census predicate, and the GenTests divergence
  pin.
- **v3.1, m7 (quoted: "State that section 4's instance was measured in the package-class layout").** v2's §4
  real-generator instance ran the R1-only prototype with the forwarders in the PACKAGE class. Every v3 / v3.1 reading
  (§5) is the sibling-class layout.
- **v3.1, m8 (quoted: "The item-5 census predicate becomes (i')'s collision set, split Go-correct vs Go-incorrect").**
  - Go-INCORRECT refusals: the `collide` rows Go promotes but literal (i') refuses. There are 2 per target
    (`cleanupT.Chdir/Setenv`), which refinement (b) mints, so 0 under the cut's rule.
  - Go-CORRECT refusals: names Go itself drops by ambiguity or shadowing. Those never become census rows; they are the
    drop rows and the Go-equal arms of §5.

---

## 4. Populations, term by term (re-derived from the committed tools, M6)

**Units.** A row is (the enclosing type's declaration position, method). T / *T is the value / pointer method set.
"Exported" is the Go name.

| term | windows | linux / darwin |
|---|---|---|
| exported crossing rows (`cmd/census`) | 784 | 781 |
| − generic metadata embed (`unique.uniqueMap`) | 11 | 11 |
| − C4, through a pre-existing internal same-package forwarder | 67 | 67 |
| − R2 skips (sibling class) | 0 | 0 |
| − C3 signature skips | 0 | 0 |
| − M4 guard skips | 0 | 0 |
| **= 706 / 703 before (i')** | | |
| − k, (i') collisions at the Go level (`collide`) | 2 | 2 |
| **= forwarded under literal (i')** | **704** | **701** |
| **v3.1: = forwarded under refinement (b), the cut's rule (k minted, Go-correctly)** | **706** | **703** |

- **The re-derivation (a CROSS-CHECK: the two tools share `treeOccurrences` and go/types).** `cmd/sim` models the
  generator with v3's rules: master's counter over same-package occurrences only, cross names minted by (i'), C4's
  invisible same-package forwarders, and `[GoRecv]` methods at any depth. `cmd/sim -b` adds refinement (b).
  - The committed join `scripts/reconcile.sh` labels both sides (package, type, declaration file:line, method).
  - Literal mode against `sim`: **704 / 701 / 701 = census**. (b) mode against `sim -b`: **706 / 703 / 703 = census**.
    Every target has 0 rows on either side.
  - Both readings are pinned. An unpinned run read a cached go1.24.0 std and shifted two types' lines by 13; the README
    now requires the pin assertion.
- `removed` is **0** on all targets and behavioral. Behavioral `k` = 0 in all 6 crossing modules.
- **Over-claims** (all PRE-EXISTING same-package, none added by the cut): 21 per target under the any-depth `[GoRecv]`
  model, 10 under v2's depth-1 model. All are internal/abi field shadows (the local type `u` among them) plus
  `runtime.timeTimer.init` (§6.1).
- **Under-claims:** 167 rows per target. All are pre-existing same-package rows except the residuals named in §6.
- **v2 corrections:**
  - v2's "770" counted C4 names as visible.
  - v2's "breakableConn 3" box-shim exclusion was wrong: `sync.Mutex.Lock/Unlock` are `[GoRecv] this ref`
    (`src/core/sync/mutex.cs:137`, `:250`) and promote, which the prototype's `Broken` confirms.
  - The 4 clash rows of v2 sat inside the 784. With the sibling class no clash skips anything; the full guard-set
    census finds 25, after C4 removed `testMockTCPConn.File` from the added set.

---

## 5. The real-generator prototype (scratch, never committed)

Three Go modules, each converted by the tree's own converter and built with its own go2cs-gen:
- **ProtoA:** `Inner` with `Tok`, `Name`, `Pair`, `(*Inner) Set`, `String` and `Where`.
- **ProtoB:**
  - `Mid{ProtoA.Inner}`;
  - `Mid2{ProtoA.Inner; loc2}`, with `Tok` ambiguous inside it;
  - `SameP{*r1}` and `SameV{r1}`, the same-package `(*r1).Ping`.
- **ProtoC:** the shapes below.

The build runs Release, `-m:4`, `-p:UseSharedCompilation=false`. The Roslyn compiler server otherwise keeps a STALE
go2cs-gen loaded across rebuilds; that was caught by a marker the new generator had to emit. The prototype is master
`f819887fa3` plus the whole design.

### 5.1 Go-equal shapes (each prints value and pointer NumMethod and the sorted method names)

| shape | what it exercises | result |
|---|---|---|
| `Direct{ProtoA.Inner}` | R1, R5 (value set excludes `Set`) | = Go |
| `PtrDirect{*ProtoA.Inner}` | B1 (value set INCLUDES `Set`, `MethodByName` does not throw) | = Go |
| `Outer{ProtoB.Mid}` | C4 transitive (`Name`/`Pair` through `ProtoBᴛMidᴛxpkg`) | = Go |
| `Five{ProtoB.Mid; loc}` | S2: `loc.Tok` (depth 1) beats `Mid.Inner.Tok` (depth 2); `Tok` N = 7 | = Go |
| `GCounter{local{ProtoA.Inner}; ProtoB.Mid}` | G's counterexample: everything ambiguous (0 / 0) | = Go |
| `S6{loc; local2{ProtoA.Inner}}` | no cross forwarder on the losing path; `Tok` N = 7 | = Go |
| `Within{ProtoB.Mid2}` | the within-embed variant (`Tok` absent) | = Go, after (i') refinement (b) |
| `Shadow{ProtoA.Inner; Tok int}` | field shadow (R4 subsumed) | = Go |
| `Mixed{localMid{ProtoA.Inner}}` plus a package var `Name` | M3, R2 (no CS0102) | = Go |
| `Broken{io.Reader; *brokenState{sync.Mutex}}` | breakableConn's shape; value is a `sync.Locker` | = Go |
| `Stamp{time.Time}` / `PtrStamp{*time.Time}` | `json.Unmarshaler` false / true / true; 43 / 47 / 47 / 47 methods | = Go |
| `BufP{*bytes.Buffer}` | B1 over std `[GoRecv]` methods (23 / 23) | = Go |
| `HideC{bytes.Buffer; io.WriterTo}` | R3 via the marker (`WriteTo` dropped: 0 / 22) | = Go |
| `Where` via direct call, via assert on `Direct`, via assert on `Outer` | m4 frame names | = Go (`main.Main` naming aside) |
| **v3.1** `OuterS5{ProtoB.Mid3; localTok}` | S5: Inner's `Tok` at depth 3 vs `deepTok.Tok` at depth 2; set AND dispatch (N 5) | = Go |
| **v3.1** `OuterFD{ProtoB.MidF}` | a FIELD at depth >= 1 inside a metadata subtree (`MidF{Inner; HasTok{Tok int}}`) | = Go |
| **v3.1** `OuterFS{ProtoB.PS}` | (b) does not propagate a field-shadowed same-package forwarder (`PS{inner3; Tok int}`) | = Go |
| **v3.1** `TT{*testing.T}` | hand-owned `this ref T` methods across packages (cleanupT's shape); 23 / 23, value `Error` true | = Go |

**v3.1, the named divergences** (S1 / S3 / S4, exactly as the ruling predicted; §6.3):
- `OuterS1{ProtoA.Foreign; localM}`: Go 0 / 0, C# 1 [M] / 1 [M]. Master's `localM.M` over-claim stays.
- `OuterS4{ProtoB.MidM; localDeep}`: Go 0 / 0, C# 1 [M] / 1 [M]. The over-claim stays.
- `OuterS3{ProtoA.Foreign; localDeep}`: membership equal (1 [M]), but DISPATCH through an interface is Go `foreign`
  vs C# `deep`. Wrong dispatch; a direct call stays right through the converter's explicit hop.

A VALUE embed `struct{ testing.T }` fails the struct template's constructor/`==` emission (CS1729, CS0019, in
`TT.g.cs`). That is pre-existing at master, not a cut effect, so the arm uses `*testing.T`.

### 5.2 Floor-13 plants (each rule OFF in turn, everything else on; the baseline is 0 diff lines)

| rule off | what flips (diagnostic and site, or the line) |
|---|---|
| (i') filter | **CS0121** at ProtoB `Mid2.g.cs:109`: `ProtoB_package.Tok(Mid2)` and `ProtoBᴛMid2ᴛxpkg.Tok(Mid2)` are ambiguous |
| R5 | the first `report(Direct)` DIES: `NotImplementedException` at `GoReflect.MakeDelegateType` (`GoReflect.MethodSets.cs:620`) |
| B1 | `report(PtrDirect)` DIES at the same site |
| the marker | `HideC ptr 23` vs Go's 22: `WriteTo` is minted |
| M4 (guard scoped) | `Stamp` / `PtrStamp` lose `After`, `Date`, `Unix`, `UnixMicro`, `UnixMilli` (38 / 42 vs 43 / 47) |
| **v3.1** C4-transitive (the cross forwarder takes the NAME-heuristic scope) | `Outer value 5 [Name Pair String Tok Where]` -> `1 [Tok]` (and `Five` the same): ProtoB's `Mid` forwarders go internal and vanish one assembly on |
| **v3.1** C14 (the generic test by STRING, `Contains("<")`) | every pointer-embed shape loses its cross forwarders: `PtrDirect 6 / 6` -> `0 / 0`; `Broken 4 [Lock Read TryLock Unlock]` -> `1 [Read]` (also PtrStamp, BufP, TT) |
| **v3.1** R4 (fields not counted in the (i') walk) | `Shadow value 4` -> `5 [... Tok ...]`: the field-shadowed `Tok` is minted; `OuterFD` and `OuterFS` move too |
| **v3.1** (b)'s field separation (`bfield`: fields merged into the surface) | `OuterFS 0 / 0` -> `1 [Tok] / 1 [Tok]`: the field-shadowed same-package forwarder propagates |

**v3.1:** the baseline (every rule on) differs from Go only by the 8 lines of the named S1/S4 divergences. Each plant
is measured against that baseline. The broken-PARITY plant is a GenTests arm of the cut: its diagnostic is the parity
test's `Assert.AreEqual` failing on the generated text, at the arm's own line. The MSBuild-only prototype has no
CompilationReference route.

### 5.3 The §5.6 same-package class, at run time (ProtoB's own forwarders, unchanged by the cut)

| probe | Go | C# |
|---|---|---|
| `SameP` (pointer hop) value: reflect method walk / `MethodByName("Ping")` / `Method(0).Call` | works | **process dies** (`NotImplementedException`, not catchable by `recover`) |
| `SameP` value: assert to `interface{ Ping() int }` and call | 1 | **"not satisfied"** (the binder skips by-ref) |
| `SameP` value NumMethod | 1 | 1 |
| `SameV` (value hop) value NumMethod | 0 | **1** (over-claim) |
| `SameV` value method walk | works | **process dies** |
| `*SameV` assert and call | 1 | 1 |

---

## 6. Named residuals (not in TRAIN M unless COORD rules otherwise)

1. **Same-package field-shadowed forwarders, Go-incorrect today** (ruled a separate follow-up): internal/abi
   `ArrayType.Elem/Len`, `ChanType.Elem`, `PtrType.Elem`, `SliceType.Elem`, `OldMapType.Elem/Key`,
   `SwissMapType.Elem/Key`, the local type `u` and `structTypeUncommon`, and `runtime.timeTimer.init`. That is 10-21
   rows per target, depending on the pointer-method emission form. Behavioral: IotaEnum 2, StdLibInternalAbi 7.
2. **The pre-existing same-package depth-blind METHOD counter.** Fields were made depth-aware (`:290-321`); methods
   never were (`:697`, `:879-883`). It is its own follow-up.
3. **(i') collisions where Go picks a METADATA method or calls the name ambiguous** keep master's answer.
   - At the Go level `k` = 2 per target (`net/http_test.cleanupT.Chdir/Setenv`), which refinement (b) mints
     Go-correctly, so 0 under the cut's rule. 0 in behavioral.
   - **v3.1, the residual's three shapes, measured on the real generator (each pinned by a GenTests arm asserting the
     generated text keeps master's forwarder, plus a recorded known-divergence behavioral line):**
     - **S1, a same-depth collision** `Outer{ProtoA.Foreign; local}` (both `M` at depth 1). Go: ambiguous, so absent.
       C#: master's `local.M` OVER-CLAIM stays (1 [M] / 1 [M]).
     - **S3, the foreign method shallower** `Outer{ProtoA.Foreign; local{deep}}`. Go: `Foreign.M`. C#: membership
       right, DISPATCH wrong (`deep`) through an interface or reflect.
     - **S4, equal depth 2** `Outer{ProtoB.Mid{Foreign}; local{deep}}`. Go: ambiguous, so absent. C#: over-claim stays.
     - S5 (a local shallower than a deep foreign forwarder) is Go-equal (§5.1), not a residual.
4. **C4, through a pre-existing same-package INTERNAL forwarder:** 67 exported rows per target (reflect's abi wrappers,
   and net/http_test's `testMockTCPConn` and `cleanupT`). The fix is the name-heuristic scope, which is same-package
   churn. Accepted.
   - **v3.1, the TRANSIT figure.** `transit` counts **132** exported rows per target: rows reached THROUGH a foreign
     type's own forwarder. **65** are forwarded, because that forwarder is public. **67** are this C4 residual.
     `multicross` / `xtransit` is **0**: no transit row passes through a forwarder the cut itself mints.
5. **Generic metadata embeds:** 11 rows (`unique.uniqueMap`), and the generic-enclosing box-shim gap.
6. **The same-package by-ref class (B1 §5.6): v3.1, RULED A SEPARATE SEAT (the CRASH-CLASS seat), not this one.**
   - **Its design, as ruled:**
     - golib-side COPY-binding: `AdapterBinder.cs:203-204` and `CompileBoundFactory` (`GoReflect.MethodSets.cs:507-526`)
       bind a by-ref receiver WITHOUT `[GoRecv]` through a copy of the receiver. That is Go's value-method semantics,
       with no generator churn and no per-call copy on static paths.
     - In the SAME seat and ATOMIC with it: same-package `[GoRecv]` stamping of the value-hop pointer-method forwarders
       (SameV). It is an attribute only, and the diff is attribute lines predicted from a base build's `Generated/`.
     - A CLASSIFICATION census first: every by-ref, non-`[GoRecv]` extension receiver across the corpus (converted,
       hand-owned such as `testing.cs:231`, and generated forwarders from a base `Generated/`), each classified as a Go
       VALUE or POINTER method. The pointer ones are stamped.
     - A GolibTests reflection guard pinned to a REVIEWED ALLOWLIST.
     - G's recoverable-panic hardening: `TryAsPanic` adopts only Panic/DivideByZero/NullReference (`:490-520`).
     - The ruled before/after: the SameP value assert flips MISS -> HIT, NumMethod is unchanged, and the reflect walks
       stop dying.
   - **v3.1 correction:** the figures below count every row GO SEES, not the forwarders the generator EMITS
     (`os.fileWithoutWriteTo`: 4 emitted vs 38 counted; `bufio.ReadWriter` agrees at 25). The seat's first
     deliverable is the emitted census.
   - Census (`cmd/r5` `same-ptrhop`, Go-seen): **490 / 498 / 494 rows in 37 / 38 / 38 std types**. Among them:
     - `os.fileWithoutWriteTo`/`fileWithoutReadFrom` (38 each);
     - `net.lookupCustomResolver` (38);
     - `runtime.sweepLocked` (35);
     - `bufio.ReadWriter` (25).
   - Its value-hop twin (`same`, the over-claim) is 1013-1017 rows.
   - At run time (§5.3) a reflect method walk, `MethodByName` or `Method(i)` call kills the process, and an assert
     answers a wrong MISS.
   - **Reachability from banked rows: 0 by construction for the crash** (a banked row passes at master, where these
     forwarders exist; a reached crash kills the row). The silent MISS cannot be ruled out that way.
   - The generator by-value route is WITHDRAWN for same-package work (G). It is the fallback only if the golib route
     fails somewhere, and then with a measured go/types A/B, because it copies the outer struct per static call.
7. **Unexported cross-package methods** (R1): unobservable, per `UnexportedMethodPackageMatches` (`GoMethodSets.cs:239-255`).
8. **CompilationReference transitive promotion (M7).** The syntax arm's exact-text receiver match never sees a
   qualified `ᴛxpkg` receiver. Pinned by GenTests; MSBuild (metadata) is unaffected.

---

## 7. Gates for the cut

1. **The `-tests` EMISSION gate first.** The cut regenerates committed `*_test.cs`, and an unmarked io_test `Buffer`
   (`io_test.cs:22-25`) would mint `ReadFrom`/`WriteTo` and PANIC `TestCopyLargeWriter`. So: 57 / 59 / 58 files per
   target, marker-only, positional against `cmd/ifaceembed`.
2. **Reds first**, each red at the base and green at the cut:
   - Stringer, value and pointer;
   - **R5, split as M5 asks:**
     - red-at-base: NumMethod of T and *T;
     - red-under-R5-off: value NumMethod and `MethodByName`. The assert arm is green at the base and masked by the
       binder, so it is not a red.
   - B1: PtrStamp value NumMethod equal to Go's, and `MethodByName` not throwing;
   - a composite-interface arm (`struct{ bytes.Buffer; io.ReadWriteCloser }`);
   - the field-shadow arm;
   - a var clash and a dot-import clash beside a forwarder;
   - C3's `Leak() hidden`;
   - the C4 transitive arm `Outer{B.Mid{A.Inner}}` (`Name`, `Pair` true), with its plant;
   - the §2.1 reflect / json / xml / ConvertibleTo arms;
   - the `Value.Elem` red.
3. `TestCopyLargeWriter` stays PASS; `EmbeddedInterfaceWitness` stays green, its `conflicted` arm included; io and
   io_test BUILD.
4. **GenTests:**
   - `CrossPackageEmbedStaysPublicFieldsOnlyAndMintsNoForwarders` (`:251-267`) flips on purpose, with
     `serveInternal` still refused;
   - the transitive parity arm, or the M7 pin;
   - the composite-interface arm;
   - the breakableConn shape (`sync.Mutex` `Lock/TryLock/Unlock` through a pointer hop, emitted BY VALUE, landing in
     `ᴛxpkg`);
   - a mixed-path arm with a clashing package var;
   - the M4 guard arm and its plant;
   - the generic fixture with where-clauses;
   - every run asserts no generator exception and no diagnostics.
5. **The per-row `[GoRecv]` gate.** Each of the 360 `cmd/r5` cross rows (value-hop, pointer-receiver) is emitted
   `[GoRecv] this ref`, and each of the 387 / 384 / 384 `cross-ptrhop` rows is emitted by value. A GolibTests guard
   asserts that no by-ref, non-`[GoRecv]` method reaches `GetGoMethodSetEntries` for a cross forwarder.
   - **v3.1, the guard's SCOPE.** It is CROSS-only here: its predicate is "a method of a `ᴛxpkg` class". The
     same-package class EXISTS today (§6.6), so a corpus-wide guard would fail at master. The CRASH-CLASS seat widens it
     into the allowlist guard.
   - **The plant proving it fails:** the predicate (a `ᴛxpkg` forwarder that is `this ref` with no `[GoRecv]`), read
     over the prototype's `Generated/`, counts **0** at the baseline, **45** with R5 off and **53** with B1 off.
6. **The generated-source diff is the footprint.**
   - Base and cut run in SEPARATE fresh worktrees, every `Generated/` purged first.
   - Generated output is captured PER PROJECT (`-p:CompilerGeneratedFilesOutputPath=Generated-<proj>`; `io.csproj:106`
     and `io.tests.csproj:80` otherwise collide).
   - The diff unit is (project, hint name, declaration), and declarations AND comments are measured.
   - Production and test predictions are separate. The tests.csproj of every package holding a census `_test` type is
     built (io, encoding/json, net/http, net/http/cgi, math/big, go/types, runtime, sync, image/draw, net/rpc/jsonrpc;
     crypto/tls on its lane).
   - The diff is census'd BOTH ways against `cmd/sim -b`'s added set (v3.1, re-predicted under refinement (b)):
     **706** on windows; the linux reconciliation 703; darwin stated unmeasured.
   - **A mismatch is a STOP, not a reconciliation.** Same-package changes: 0.
   - Planted control: drop one predicted forwarder; the both-ways census names it; restore byte-identical.
7. **Behavioral:**
   - the `Generated/` diff shows additions only in the 6 crossing modules;
   - the FULL behavioral run suite;
   - CNR moves exactly the 15 marker modules, positionally;
   - `go2cs.slnx` built once.
8. **FLOOR 13:** the §5.2 plants re-run on the cut itself, each restored byte-identical with SHAs and logs cited:
   - R3/marker off: `TestCopyLargeWriter` PANIC with (i') on, and the Witness `Reader: yes`;
   - R5 off;
   - B1 off;
   - (i') off: CS0121;
   - M4 off;
   - the C14 string test;
   - a broken parity arm;
   - v3.1: C4-transitive (the name-heuristic scope), R4 (fields not counted) and (b)'s field separation, each keyed
     on the §5.2 line it flips.
9. **Full validated sweep on windows plus a linux leg.** crypto/tls and net go to their lane. Must-not-move rows:
   reflect, internal/reflectlite, encoding/json, encoding/xml, fmt, archive/tar, io, net, os. Plus GolibTests x2 with
   GOROOT pinned, and `go test ./...`.

---

## 8. For G and COORD to read

v3's three open points are RULED (v3.1):
- §6.6 is a SEPARATE CRASH-CLASS seat, designed as in §6.6. It is sized after this record, with its emitted
  classification census as the first deliverable.
- Refinement (b) is the cut's rule, with its premise, field separation and re-prove trigger (§2.2b). Gate 6 reads
  706 / 703.
- The name `{pkg}ᴛ{Struct}ᴛxpkg` is SOUND, with its definitions (§2.2).

For the spot-read: this record's v3.1 diff only. The cross-package seat is then ready to cut (TRAIN M, after L).
