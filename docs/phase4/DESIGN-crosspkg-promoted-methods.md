# TRAIN M -- a method promoted through another package's embed, in the RUNTIME method set

**Status:** DESIGN RECORD **v3**, no code in this file. It revises v2 (`6fc6c2b26d`) under COORD's second CONDITIONS
ruling (G's re-read and COORD's three-lens review), for G and COORD to re-read before any cut. The census tools this
record cites are committed beside it, in `src/tools/crosspkg-census` (M6).
**Lane:** i9
**Date:** 2026-09-30
**Tree read:** `master` @ `f819887fa3`; every file:line is at that tree. Two files moved on TRAIN K's union
`4016a2269c`, and both lines are cited (m6): `reflect/value_impl.cs` Anonymous `:2763` -> `:2778`, identity `:3184` ->
`:3199` (`:833` is unchanged). Item 8's two K-only files are cited on K. Toolchain go1.24.13 / .NET SDK 10.0.401.
**Sequencing:** M cuts on L's LANDED master. The red `CrossPackagePromotedValueMethod` lives on L's `8213221317`.
**Companion seat:** `claude/i9-crosspkg-value-promotion` (TRAIN L) is the converter's CALL-SITE half of the same defect
class. This document is the METHOD-SET half.

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
> Footprint, re-derived from the committed tools: **+704 / +701 / +701** forwarded rows (windows / linux / darwin), 0
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
  - **(c) The literal and refined rules differ in one std case.** At the Go level the census counts every declaration,
    so `testing.T`'s `Chdir`/`Setenv` shadowing its embedded `common`'s are 2 occurrences. That is `k = 2`
    (`net/http_test.cleanupT`). Under (b) a package-class name that mirrors SAME-package providers below counts 1, so
    the prototype would mint those 2, which is Go-correct. The gate measures which.
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
- **m4, frame names.** `GoSyntheticPC` derives a frame's package from a `<pkg>_package` class (`:185-191`). The probe
  calls a `runtime.Caller(1)` method directly, through an assert on `Direct`, and through one on the transitive
  `Outer`. It reports the calling closure exactly as Go does: no `ᴛxpkg` frame surfaces. The only difference is the
  pre-existing `main.Main` vs `main.main` naming, which the forwarder-free direct call shows too.
- **m4, twins.** A (struct, method) pair holding both a `ᴛpromoted` twin and a `ᴛxpkg` forwarder is **0 by
  construction**: an interface provider counts under (i'), so a name it provides is never unique beside a cross one.

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
| **= forwarded** | **704** | **701** |

- **Independent re-derivation.** `cmd/sim` models the generator with v3's rules: master's counter over same-package
  occurrences only, cross names minted by (i'), C4's invisible same-package forwarders, and `[GoRecv]` methods at any
  depth. Its `added` is **704 / 701 / 701**. The added SET equals the census's expected set row for row (704 = 704,
  nothing on either side).
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

### 5.2 Floor-13 plants (each rule OFF in turn, everything else on; the baseline is 0 diff lines)

| rule off | what flips (diagnostic and site, or the line) |
|---|---|
| (i') filter | **CS0121** at ProtoB `Mid2.g.cs:109`: `ProtoB_package.Tok(Mid2)` and `ProtoBᴛMid2ᴛxpkg.Tok(Mid2)` are ambiguous |
| R5 | the first `report(Direct)` DIES: `NotImplementedException` at `GoReflect.MakeDelegateType` (`GoReflect.MethodSets.cs:620`) |
| B1 | `report(PtrDirect)` DIES at the same site |
| the marker | `HideC ptr 23` vs Go's 22: `WriteTo` is minted |
| M4 (guard scoped) | `Stamp` / `PtrStamp` lose `After`, `Date`, `Unix`, `UnixMicro`, `UnixMilli` (38 / 42 vs 43 / 47) |

The remaining plants are GenTests arms of the cut itself: the C14 string test, a broken parity arm, and R4 keyed on the
field-shadow arm. R4 cannot be switched off apart from (i'), which subsumes it.

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
3. **(i') collisions where Go's winner is a METADATA method** keep master's MISS: `k` = 2 per target at the Go level
   (`net/http_test.cleanupT.Chdir/Setenv`), 0 in behavioral. Under refinement (b) the prototype rule may mint them,
   Go-correctly.
4. **C4, through a pre-existing same-package INTERNAL forwarder:** 67 exported rows per target (reflect's abi wrappers,
   and net/http_test's `testMockTCPConn` and `cleanupT`). The fix is the name-heuristic scope, which is same-package
   churn. Accepted.
5. **Generic metadata embeds:** 11 rows (`unique.uniqueMap`), and the generic-enclosing box-shim gap.
6. **The same-package by-ref class (B1 §5.6): a CRASH residual unless COORD folds it into M.**
   - Census (`cmd/r5` `same-ptrhop`): **490 / 498 / 494 rows in 37 / 38 / 38 std types**. Among them:
     - `os.fileWithoutWriteTo`/`fileWithoutReadFrom` (38 each);
     - `net.lookupCustomResolver` (38);
     - `runtime.sweepLocked` (35);
     - `bufio.ReadWriter` (25).
   - Its value-hop twin (`same`, the over-claim) is 1013-1017 rows.
   - At run time (§5.3) a reflect method walk, `MethodByName` or `Method(i)` call kills the process, and an assert
     answers a wrong MISS.
   - **Reachability from banked rows: 0 by construction for the crash** (a banked row passes at master, where these
     forwarders exist; a reached crash kills the row). The silent MISS cannot be ruled out that way.
   - The B1 remedy (by value through a pointer hop) is safe for callers: a by-value receiver serves lvalues and
     rvalues, and the mutation goes through the pointer. Applied same-package, it would change those generated files.
     **COORD rules whether it joins M.**
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
6. **The generated-source diff is the footprint.**
   - Base and cut run in SEPARATE fresh worktrees, every `Generated/` purged first.
   - Generated output is captured PER PROJECT (`-p:CompilerGeneratedFilesOutputPath=Generated-<proj>`; `io.csproj:106`
     and `io.tests.csproj:80` otherwise collide).
   - The diff unit is (project, hint name, declaration), and declarations AND comments are measured.
   - Production and test predictions are separate. The tests.csproj of every package holding a census `_test` type is
     built (io, encoding/json, net/http, net/http/cgi, math/big, go/types, runtime, sync, image/draw, net/rpc/jsonrpc;
     crypto/tls on its lane).
   - The diff is census'd BOTH ways against `cmd/sim`'s added set: **704** on windows; the linux reconciliation 701;
     darwin stated unmeasured.
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
   - a broken parity arm.
9. **Full validated sweep on windows plus a linux leg.** crypto/tls and net go to their lane. Must-not-move rows:
   reflect, internal/reflectlite, encoding/json, encoding/xml, fmt, archive/tar, io, net, os. Plus GolibTests x2 with
   GOROOT pinned, and `go test ./...`.

---

## 8. For G and COORD to read

- **§6.6, the same-package by-ref crash class:** fold the by-value emission into M (it changes same-package generated
  files: 490 rows in 37 types), or keep it a named crash residual with its guard.
- **(i') refinement (b)'s package-class mirror:** it mints `cleanupT.Chdir/Setenv` Go-correctly where the literal rule
  refuses (`k` 2 vs 0). Which rule is the cut's?
- **The name** `{pkg}ᴛ{Struct}ᴛxpkg`, as specified in M2.
