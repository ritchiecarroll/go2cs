# Converted Test Suites

[Reference index](README.md) · [Summary of this topic](../ConversionStrategies.md#converted-tests)

This page covers how `-tests` converts a package's Go test files: the test-project models and the white-box bridge, how test-variant names stay coherent with production, and which references and source files a test project compiles.

## Test-variant names

### Test-variant name coherence: production names are pinned, test-side method declarators Δ-rename

The `-tests` pipeline re-analyzes the package over the **whole variant universe** (production files + `_test.go` files) but only **emits** the test files — the production `.cs` on disk were converted from the production-only universe and recompile into the test assembly as-is. Production symbol names are therefore **immutable** in a test-variant analysis: any collision a test file introduces must resolve by `Δ`-renaming the **test-side declarator**, never the production element. Two shapes (strings/sort blockers B2/B9), both resolved in `performNameCollisionAnalysis`:

- **A test-file METHOD over a production element's name (B2).** strings' export_test.go declares `func (r *Replacer) Replacer() any` — the ordinary type-vs-method resolution (above) would Δ-rename the *type*, but the production `replace.cs` on disk keeps `Replacer`, so the assembly split into two disagreeing halves (CS0102 `strings_package` already contains `Replacer` + CS0246 `ΔReplacer`). When the colliding method declarators are **all** test-declared and the element is production-declared, the element keeps its bare name (and no exported alias is registered) and the **method** Δ-renames instead; a FuncDecl colliding with a same-package element is necessarily a method (Go keeps method names in a separate namespace — any other same-scope reuse is a Go compile error). When a *production* method also carries the name, the production universe had the same collision and already renamed the element on disk, so the normal path stays consistent.
- **A test-file METHOD shadowing a dot-imported function the variant calls unqualified (B9).** Go keeps method names and dot-imported function names in separate namespaces, but both land in the C# package class's member-lookup scope, and an enclosing class's method group always wins over `using static` imports — sort_test.go's dot-imported `Sort(data)` bound example_keys_test.go's `By.Sort` extension (CS1501 ×14, plus 5 downstream method-group CS1503s the wave-2 probe had attributed to B10). A test-declared method whose name matches a foreign function the variant references **unqualified** (only unqualified sites conflict — SelectorExpr Sels are excluded, so a qualified `sort.Sort(ps)` never triggers; an unqualified foreign-function reference can only come from a dot-import) Δ-renames, and the dot-imported call keeps its bare emission, now binding through `using static`.

- **A test-file FREE FUNCTION whose emitted signature matches a production METHOD's receiver (2026-07-20).** A method emits as a C# extension method, so its receiver becomes the leading `this` parameter — and `this` does **not** participate in C# signature identity. math/big's `func (z nat) norm() nat` (nat.go) and `func norm(x nat) nat` (int_test.go) are legal Go in separate namespaces, but both emit as `norm(nat)` in `big_package`: CS0111. `resolveReceiverParameterCollisions` compares each same-named pair's *emitted* parameter list — the method's receiver type followed by its parameters, against the free function's parameters — and Δ-renames the test-side declarator. Discrimination is exact: an extra parameter (`func trim(x nat, n int)`) or a different first parameter (`func keep(n int)`) emits distinctly and keeps its plain name, as do generic declarations (type parameters keep the C# signatures distinct) and a variadic/non-variadic mismatch. Two methods can never collide this way (Go forbids redeclaring one method on one type) and two free functions cannot share a package scope, so a method/free-function pair is the only shape. When *both* sides are test-declared the FREE FUNCTION is the one renamed, so the outcome does not depend on declaration order and two colliding declarators never both become `Δ`-prefixed; a collision between two **production** declarators is deliberately left alone, since it would equally break the production-only conversion and is a different fix than test-variant coherence (the 302-package corpus compiles clean, so no instance exists).

The rename registry is **object-keyed** (`testMethodRenames map[types.Object]bool`) — the same-named production type/function keeps its plain emission at every other site — and **session-scoped**, initialized once per `-tests` conversion rather than per variant: both variants come from one `go/packages` load, so the external variant's references to an internal-variant method (the export_test pattern) resolve by object identity to entries registered during the internal pass. The declaration renames in `visitFuncDecl`, and every reference follows through `convIdent` — a METHOD name through its isMethod arm (all selector emissions funnel there), and a package-level **free function** referenced as a call target or function value through the trailing identifier path, which the receiver/first-parameter case above made reachable (without it the declarator renamed while its call sites still emitted the bare name: CS0103). The go2cs-gen `RecvGenerator` reads the emitted name, so generated `ж`-receiver overloads follow automatically. Real emissions from the probes:

```csharp
public static any ΔReplacer(this ж<Replacer> Ꮡr) { … }   // strings export_test.go `func (r *Replacer) Replacer() any` — type stays bare
@string got = fmt.Sprintf("%T"u8, tc.r.ΔReplacer());     // EXTERNAL-variant call site (replace_test.cs) follows the internal rename

public static void ΔSort(this By by, slice<Planet> planets) { … } // sort example_keys_test.go `func (by By) Sort(planets []Planet)`
new By(mass).ΔSort(planets);                                       // test-method call sites follow
Sort(data);                                                        // dot-imported production call keeps its bare emission
```

Production conversions have no `_test.go` files in their universe, so the analysis is inert there ([CNR](../Glossary.md#cnr) byte-identical ×402). (Guarded by `TestTestVariantPinsProductionTypeAgainstTestMethodCollision` — declaration, internal call site, external call site, and the pinned type — and `TestTestVariantRenamesTestMethodShadowingDotImportedFunction` — rename + bare dot-imported call, with never-referenced and qualified-only same-named methods as discrimination controls.)

### A name both `-tests` variant classes declare is qualified with the FILE's anchor class
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

### A box accessor's qualifier names the class that DECLARES the type

**A box accessor's qualifier names the class that DECLARES the type, which under `-tests` can be the bridge.** The accessor `Type.Ꮡfield` in `receiver.of(Type.Ꮡfield)` package-qualifies whenever a bare name could be shadowed (see *Field address of a collision-renamed heap-boxed local*), and the qualifier used to be the production `<pkg>_package` unconditionally. For a type an internal `_test.go` declares that class is the wrong one — the white-box emission unit is the bridge (`<pkg>_internal_test_package`), so the reference resolves to nothing (CS0117). `database/sql`'s `fakedb_test.go` is the corpus instance and it is forced into the qualifying path: `type table struct { mu sync.Mutex; … }` sits beside `func (db *fakeDB) table(string) (*table, bool)`, so the *type* is Δ-renamed, and Δ-renamed always qualifies — every one of the six `t.mu.Lock()`/`Unlock()` sites emitted `sql_package.Δtable.Ꮡmu`. The qualifier now resolves through `packageScopeClassName`, the same helper that already draws the production/bridge line for package-level *value* references, so both halves stay addressable from the one bridge file. Production conversions are unaffected by construction — outside `-tests` there is no class override and the helper returns the production class. (Guarded by the `TestTestVariantBoxAccessorNamesBridgeDeclaringClass` converter test, which asserts both directions: the bridge class appears and the production class does not.)

### A NESTED package's production `GoImplement` record anchors to the production metadata file

An EXTERNAL test variant's collected `GoImplement` records are split across **two** anchor files, because the go2cs-gen `ImplementGenerator` hosts its output in the **first class** of the attribute-bearing file: a record whose generated adapter must be a member of the test package class goes to `package_info_external_test.cs` (whose first class is `<pkg>_test_package`), while a record that generates a partial/adapter on the **production** class stays with the production-anchored `package_test_info.cs`. `isTestAnchoredImplementRecord` decides which, by testing the implementer name against the production class's qualifier.

The anchor is a **file-level** property — `GetFirstClassName` reads the first `ClassDeclarationSyntax` in the compilation unit — so two required anchors mean two files; this is a workaround for the generator's positional contract, not an intrinsic need. The external unit is written only when the variant actually records test-anchored attributes, so utf8-class packages keep their single-file shape. It carries **no** `[GoPackage]` (the attribute-bearing partial stays in `package_test_info.cs`, CS0579) and **no** `global using` aliases (they must be declared once per compilation, CS1537).

⚠ **The `_test.cs` suffix on `package_info_external_test.cs` is load-bearing.** It is what excludes the file from the *production* project, via the shared `csproj-template.xml` `<Compile Remove="*_test.cs;…">` glob and `productionCSFiles`. `package_test_info.cs` does *not* match that glob, which is why it needed its own explicit entry in the template — and why the external unit was originally named `package_info_test.cs` (2026-07-18) to ride the glob for free. That name was a near-anagram of `package_test_info.cs`, and the two sorted adjacent to `package_info.cs` in every converted package directory; it was renamed to `package_info_external_test.cs` on 2026-07-21 ("external test package" is Go's own term for `package <name>_test`). Any future rename must keep the `_test.cs` suffix, or add an entry to the shared template — which re-emits, and so churns, every behavioral `.csproj`.

The live records qualify the implementer **namespace-relative — WITHOUT the `go.` root**: math/rand's external `rand_test` casting `*rand.Rand` to `io.Reader` records `ж<math.rand_package.Rand>`. The test formerly compared only the BARE (`rand_package.`) and fully-rooted (`go.math.rand_package.`) forms, so a **top-level** package matched by accident — its relative qualifier *is* the bare form (`sort_package.IntSlice`) — while every **nested** package (`math/*`, `text/*`, `net/*`, `encoding/*`, `container/*`, `hash/*`, `crypto/*`) matched neither and mis-anchored to the test file. The generator then emitted a SHORT `StructName` (non-foreign structs are emitted unqualified) inside `rand_test_package`, producing `ж<Rand>` where no `Rand` is in scope — while the converter's own cast site already assumed production anchoring (`new rand.RandжReader(r)` against `using rand = go.math.rand_package;`), so the pair failed as two CS0246s in the generated `.g.cs`. The relative qualifier is now recognized alongside the other two, matching the function's stated contract.

⚠ **`package_info_external_test.cs` and `package_test_info.cs` are MERGE-PRESERVING.** After any change to this routing, DELETE both files *and* the package's `Generated/` directory before re-running the pipeline — otherwise a stale record persists in **both** anchors and masks the result.

## Test-project models

### Test suites REFERENCE the production project instead of recompiling it

The original `-tests` model — recompile the production `.cs` into the test assembly — duplicates the production types. That is harmless until another referenced assembly surfaces one of those types in its API: `strings.ToLowerSpecial(unicode.SpecialCase, …)` and `hash.Hash : io.Writer` name the type in the production assembly, while the test source binds a distinct recompiled copy. No compile-set adjustment repairs that identity split.

`-tests` therefore selects among **three test-project models** (`selectTestProjectModel`, recorded as `testProjectModel` in the manifest):

- **`reference`** — a black-box-only suite references the colocated production project and emits only the external test package into the test assembly.
- **`whitebox-reference`** — a suite with an internal variant still references production, but emits the internal `_test.go` declarations into a separate friend-assembly bridge. This keeps the production assembly as the sole identity while preserving access to Go-unexported members.
- **`recompile`** — the original same-assembly shape, retained only as the deterministic fallback when test-contributed metadata genuinely has to mutate a closed production type.

Both reference models bind the package under test as an ordinary imported package: exported aliases and implementation metadata load from the colocated `package_info.cs`, its types render package-qualified, and `isSameAssemblyPkg` is false. Their `package_test_info.cs` is a **test-class-only metadata anchor**; it never declares a local production partial.

The white-box extension has five coupled parts:

1. The normal production scan already reads build-selected same-package `_test.go` files to stabilize alias-shadow spelling. That same cheap scan now reports whether an internal test file exists. Only then does the production `.csproj` emit `<InternalsVisibleTo Include="$(AssemblyName).tests" />`; packages with no internal tests remain byte-stable.
2. Internal test files emit into `<name>_internal_test_package`, with `using static <namespace>.<name>_package`. Production declarations remain untouched. Test-host registrations retain the Go package name in the manifest but target this bridge class.
3. `go/packages` loads production, internal and external test variants together. An external test reference is routed to the bridge only when its `go/types.Object` belongs to the production import path **and** its declaration position is in `_test.go`; production objects and same-spelled unrelated declarations keep their ordinary route. This is how `io_test` reaches `ErrInvalidWrite` from `export_test.go` without source rewriting or a generated alias contract.
4. Test-contributed implementation adapters are owned by test metadata anchors — a MIXED suite has **two**. The generators host output in the FIRST class of the attribute-bearing file, and a mixed white-box assembly has two classes generated code must merge into, so the B4/B5 two-file split returns in mirror image: records whose generated partial must merge with a **bridge-declared** type (a BARE record name in the internal variant's declared-name set — `splitWhiteboxVariantRecords`) anchor in **`package_info_internal_test.cs`**, whose first — and only — class is the bridge (also the bridge's single `static` declaration and its `[GoPackage]` carrier); every other record — production-qualified, foreign, or external-declared — stays in `package_test_info.cs` under the external test class. Anchoring a bridge implementer in the external class would generate a phantom empty type there instead of merging with the real declaration. Deferred adapter markers are redirected only when the exact `(struct, interface)` pair appears in a test anchor (`emittedAdapterPair`); the anchored reference is composed `<anchor>.<member>` where the member comes from the **record's** spelling (`anchoredAdapterMemberName` — `adapterStructKey` normalizes a qualified production struct to the generator's foreign `<pkg>_<Simple>` form and leaves a variant-local name bare, exactly the generator's local-vs-foreign naming split; composing from the cast site's spelling instead emitted `ParseErrorжerror` where the generator wrote `csv_ParseErrorжerror`), and each pair remembers **which** anchor file recorded it. Imported production adapters keep pointing to their defining assembly. The generator also recognizes a collision-renamed embedded value property (`ΔBuffer` for embedded `bytes.Buffer`) as the same promotion hop, and scans the current compilation for friend-bridge box-receiver extensions by **simple name** when — and only when — the struct has no local declaration (the bridge spells its box parameter through the imported alias, `this ж<Replacer>`, and the metadata-only case is precisely the one whose discovery compilation is null).
5. The metadata seed imports the production, bridge and external-test classes as needed, but its first and only declaration remains the selected test anchor. An internal-only suite's bridge is both the test class and the bridge, so the seed imports it exactly once (a second, global import of the same class is CS8933). This keeps go2cs-gen's positional anchor contract deterministic for mixed and internal-only suites.
   **A MIXED suite's `package_info_internal_test.cs` is therefore written UNCONDITIONALLY, records or not (2026-08-14).** The file is not only a metadata anchor: it is the bridge class's ONLY `public static partial` declaration. Every converted SOURCE file opens its package class bare — `partial class registry_internal_test_package {` — exactly as production and external-test sources do, with the modifier living in the metadata file; see [`package_info.cs`'s `TypeAccessibility` section](source-generators.md#package_infocss-typeaccessibility-section-pins-each-types-accessibility-in-source) for the same division of labour applied to types. Writing the unit only when the variant contributed bridge-anchored `GoImplement`/`GoImplicitConv` records therefore left a record-less bridge with no `static` declaration anywhere, and an internal test file declaring a method on a production type — which converts to an EXTENSION method — is then **CS1106**. `internal/syscall/windows/registry`'s whole 6-verdict suite sat behind one such line, `func (k Key) SetValue(name string, valtype uint32, data []byte) error` in its `export_test.go`. Mixed suites that appear to escape it do so *incidentally*: `sort`, `bytes` and `strings` each happen to have a go2cs-gen `RecvGenerator` file that re-declares the class `public static partial` — a generator supplying a modifier the emitter owes. A record-less bridge writes an anchor whose sections are all empty, which is what the production and external-test seeds already do in the same situation. Measured: registry moves from build-blocked to **4 of 6** (residuals `TestValues`, a raw-address array reinterpret materializing a zero-length `array<T>`, and `TestGetMUIStringValue`); guarded by `TestWhiteboxBridgeUnitIsWrittenWithoutBridgeRecords`.
6. The friend grant is **inserted after template rendering**, never as a template verb: a user-supplied `-csproj` template keeps its historical verb count and renders exactly as before (`insertFriendAssemblyAccess`, anchored on the first closing `PropertyGroup`). And the reference models' anchored metadata writes treat the anchor class as the local type scope (`metadataAnchorLocalTypes`), while the recompile model's anchored writes keep the historical production-local qualification — there the production class genuinely is local to the assembly.

**A production ALIAS whose right-hand side is ANONYMOUS is carried across with its `global using` (2026-08-14).** Go's `type CorpusEntry = struct{Parent string; Path string; Data []byte; …}` (internal/fuzz's `fuzz.go`) has no C# spelling of its own, so the production conversion LIFTS the anonymous struct to a real nested type and reaches it through a compilation-scoped alias — `global using CorpusEntry = go.@internal.fuzz_package.CorpusEntryᴛ1;` at the top of `fuzz.cs`. `global using` is scoped to ONE compilation, and a reference-model test project is a second one that does not recompile the production sources, so neither half crossed: nothing visited the declaration, nothing claimed the lift, and every test-side reference fell through to `t.String()` and emitted **raw Go syntax into a C# file** —

```csharp
internal Func<struct{Parent string; Path string; Data []byte; Values []any; Generation int; IsSeed bool}, error> fn;
```

— CS1031/CS1525/CS1003 cascades in `minimize_test.cs` and `worker_test.cs`, with all 52 of the package's verdicts behind them. `seedProductionAliasLifts` now reads the production package's own `package_info.cs` (which the test conversion already opens for its `GoImplement` pairs) and seeds **both halves together**: the alias into `importedTypeAliases`, so the test metadata file re-emits the `global using`, and the anonymous TYPE into `productionAliasLiftedTypes`, so every renderer spells `CorpusEntry` (`liftedNameFor`, consulted wherever `liftedTypeMap` was). Keying by go/types identity is exact here — production and test variants are type-checked in one `go/packages` load, so the alias's right-hand side and every test-side reference are the same `*types.Struct`.

Narrow on both axes, deliberately — but on the SECOND axis only. Publication is the standing precondition: only an alias the production `package_info.cs` **publishes** is seeded, so a type is never rendered under a name the test compilation cannot resolve, and an unexported alias to an anonymous struct publishes nothing and keeps the pre-existing route.

**The anonymous-RHS restriction was WRONG and is retired (2026-08-19).** Its reasoning — "a named RHS already renders through its own qualified name" — does not hold wherever the production conversion declares the alias as a compilation-scoped `global using` in the FILE that declares it, because the renderer then spells the BARE alias name in production and test alike. `html/template`'s `type FuncMap = template.FuncMap` is exactly that shape:

```csharp
// template.cs, line 4 — the production conversion's own declaration
global using FuncMap = go.text.template_package.FuncMap;
```

A reference-model test project compiles `*_test.cs` ONLY, so `template.cs` is not in that compilation and the bare name resolves nowhere — `new FuncMap(new map<@string, any>{…})` in `clone_test.cs`, `escape_test.cs` and `exec_test.cs` is **CS0246 ×6**, with all 243 of the package's verdicts behind it. The test metadata file did already declare the CROSS-package two-hop spelling (`global using templateꓸFuncMap = go.text.template_package.FuncMap;`), which is what makes this a name-resolution gap rather than a missing import.

So a NAMED right-hand side is seeded too — and only the **name** half of it. The type half stays anonymous-RHS-only, because a named RHS has its own qualified spelling and is already rendered through it, so recording it in `productionAliasLiftedTypes` would re-spell references that already compile. The two kinds therefore need different halves of "reachable", and `recordType` in `seedProductionAliasLifts` is that distinction. The set this widens to is small and exact: across Go 1.23's converted standard library, an EXPORTED `type X = <named>` exists in four packages only — `html/template` (`FuncMap`), `os` (`DirEntry`/`PathError`/`FileInfo`/`FileMode`), `internal/reflectlite` (`Kind`) and `debug/buildinfo` (`BuildInfo`) — and a `_test.go` cannot redeclare such a name in the package's own scope, which is where the recorded collision concern was. Guarded by `TestSeedProductionAliasLiftsCarriesLiftAndAliasTogether`, which now pins BOTH halves of the named case (the alias seeded, the type not) alongside the anonymous case and the unpublished-alias control.

**Fallback is based on mutation, not merely on a production-qualified record.** Pointer/value adapters and the shared `T → ж<T>` boxing route are relocatable. A structural conversion involving a production type, or a numeric conversion whose two operands are both production types, would require an operator on a closed referenced type; `recordsRequireProductionMutation` returns `errProductionAnchoredRecords`, and the already-loaded variants are re-emitted once under `recompile`. The older black-box `recordsRequireProductionAnchor` gate remains conservative for the ordinary reference model.

The model is abstract, not an `io` patch. `io` is the first mixed-suite proof: its project now builds with one `io_package.Writer` identity, `export_test.go` lives in `io_internal_test_package`, external references bind the bridge by Go object identity, and test-owned adapters coexist with imported `io` adapters. Focused guards cover model selection, conditional internal-test detection, host targeting, mutation fallback, emitted-pair adapter ownership, alias-shadow stability, and collision-renamed embedded-value promotion.
**What the bridge owes the compiler once production is a REFERENCE.** A 62-package regeneration sweep found five defects that share one cause: the bridge is the same GO package as production, so every same-package test in the converter reads a production declaration as LOCAL — while its C# now lives in a closed referenced assembly. Each was fixed at the layer that made the wrong assumption.

- **A bridge member HIDES the production `using static`.** The bridge binds production through `using static <pkg>_package`, and C# member lookup stops at the first enclosing type carrying the name — so any member the bridge declares hides *every* same-named production member, overload resolution included. container/heap's `func (h *myHeap) Pop() any` hid `heap_package.Pop(Interface)`, and the suite's own `Pop(h)`/`Push(h, i)` bound the extension by value (CS1620 ×8). Such a reference is emitted production-class-qualified (`go.container.heap_package.Pop(…)`), the remedy `packageBuiltinShadows` already applies to a shadowed `using static go.builtin`. The shadow set is the internal variant's package-level declarations **plus its methods** — those emit as static extension members of the same class — keyed on raw Go names, and consulted only for a production-declared package-level object.
- **A production type is FOREIGN for a VALUE implement.** "Local" is what selects go2cs-gen's partial-struct realization, which folds the interface into the type's own declaration; a closed referenced type instead gets a per-interface `ᴠ` VALUE ADAPTER. Both consequences matter and only together: the cast site must CONSTRUCT the adapter, and the record must be EXEMPT from the interface-inheritance prune (sound only for the one-type-one-interface-list partial shape). encoding/binary's `TestByteOrder` casts `BigEndian` to a function-local `byteOrder` that EMBEDS the production `ByteOrder`, so the subsumed `bigEndian → ByteOrder` pair was pruned as covered — true while a merged partial carried it, false against a referenced assembly, and every `Read(r, BigEndian, data)` was CS1503. The arm mirrors the existing both-foreign one, `importedValueImplements` check included, so a pair production already records stays a bare implicit conversion.
- **A value adapter must name the anchor its record lands in.** A mixed white-box suite has two metadata anchors, and the value-adapter reference qualified through the external test class unconditionally — right for a production↔production pair, wrong for one whose BRIDGE-declared interface anchors it at the bridge (CS0426). The reference now applies `splitWhiteboxVariantRecords`' own bridge-declared-name predicate to the record's two participants, folding in the live LIFTED claims while the bridge is the variant under conversion. (The pointer form already reached the same answer through its deferred marker's `emittedAdapterPairAnchors`; a value adapter's name is composed inline and has no marker to resolve.)
- **A bridge-declared LIFTED name must reach the record split.** That split keys on the record's EMITTED name against a set built from go/types `TypeName` defs, which carry the GO-SOURCE name — so encoding/hex's `type r struct{ io.Reader }` inside `TestEncoderDecoder` was collected as `r` while its record named `TestEncoderDecoder_r`. The record anchored in the external class and the generator declared a PHANTOM empty type there; every symptom followed from that one type being empty (CS0103 on the promoted embed it does not declare, CS0034 because a phantom carries no `[GoType]` and so no TypeGenerator `(T,T)` `==` to bind exactly, CS1503 at the cast site). The internal variant's live lift claims are unioned in right after it converts, while they still stand.
- **A BARE record name resolves in the variant that RECORDED it, never across variants.** The bridge's declared-name set is a set of SIMPLE names, and the two `-tests` variants are separate Go packages, free to declare the same one: encoding/gob declares `Point` in `codec_test.go` (`package gob`) and again in `example_interface_test.go` (`package gob_test` — the one whose `Hypotenuse` implements `Pythagoras`). Each variant's records are split as that variant converts, and every cross-variant reference is routed by `go/types.Object` identity to a CLASS-QUALIFIED spelling — `whiteboxBridgeNamedType` renders an internal-test type the external suite names as `global::<ns>.<pkg>_internal_test_package.T`, and `whiteboxProductionObject` does the mirror while the bridge converts — so a bare name recorded by the external suite is external-declared *by construction*, whatever the bridge spells the same way. Matching it against the bridge's set regardless anchored the EXTERNAL pair `Point → Pythagoras` in `package_info_internal_test.cs`, where `Pythagoras` is not in scope: `CS0246`, no test host built, and all 106 of gob's verdicts read empty — a missing host masquerading as mass runtime failure. The set is therefore consulted only while splitting the BRIDGE variant's own records (`splitWhiteboxVariantRecords`' `bridgeVariant`), and the emission mirror that names an adapter through the anchor its record will land in carries the identical gate (`whiteboxBridgeDeclaredType`), so the two cannot disagree about a pair. Write-time qualification is not a substitute: `qualifyAmbiguousTestTypeRefs` roots an ambiguous bare name at the file it is ALREADY being written into, so a mis-anchored record comes out merely qualified to the wrong variant's class (`gob_internal_test_package.Point`). Guarded by `TestSplitWhiteboxVariantRecordsResolvesBareNamesInTheRecordingVariant`, over a fixture module that declares `Point` in both variants and asserts the collision itself through the real `go/types` scan (`collectWhiteboxBridgeTypeNames`) before exercising either split.
- **The friend bridge's box receivers survive into the pointer adapter.** sync's `export_test.go` declares `PushHead`/`PopTail` on the production `*poolDequeue`/`*poolChain`; the converter emits them as direct-ж primaries, so `poolDequeueжPoolDequeue` must forward `m_box.PushHead(val)`. Two generator defects stacked: the bridge scan matched the receiver's parameter TEXT against one composed spelling (`ж<poolDequeue>`) when the bridge qualifies it however its own file needs — `this ж<global::go.sync_package.poolChain>` once a `go/*` package in the closure shadows the root namespace — and the foreign-struct arm then re-derived every member's receiver from the referenced assembly's METADATA, where a bridge-contributed method does not exist, clobbering the binding back to `m_box.Value` (CS1929 ×5). The scan is now on the ж argument's last dotted segment (well-defined because it runs only where the struct has no local declaration), and a bridge-bound member is left alone.

**Reference closure (the declaration-edge rule).** The test project's references are the direct-import set, plus the alias scan (B2c), plus — because binding *any* referenced type in C# requires the assemblies that type's **own declaration** names — the **declaration closure** of that set (`declarationClosureImports`, both project models). Two declaration edges carry it: an interface's **base interfaces**, and a struct's **field types**. *Interface bases:* Go interfaces satisfy structurally *and* compose by embedding; C# interfaces are nominal, so the converter carries both shapes as C# inheritance at each interface declaration (`getStructuralInterfaceBases`): `hash.Hash` **embeds** io.Writer, io/fs's `fs.File` lists `Read`/`Close` explicitly and does *not* embed io, and both emit a converted declaration that NAMES an io base. Such a base edge belongs to the **declaring package's** import graph, so it appears in no test import and no alias `using`; `DisableTransitiveProjectReferences` (B2b) then hides the declaring assembly's own io reference, and every site that names the interface fails CS0012:

- the emitted **conversion record** — hash/maphash's `[assembly: GoImplement<Hash, hash_package.Hash64>(Pointer = true)]` and crypto/hmac's `GoImplement<hmac, hash_package.Hash>`, whose closures reach `hash` but never `io` (`'io_package.Writer' is defined in an assembly that is not referenced`);
- the **go2cs-gen adapter** realizing that record, whose class declaration lists the interface; and
- every converted production/test **source** that names it (`(fs.File, error) Open(...)`, hmac's `justHash`).

*Struct fields at a composite literal:* the converter renders a Go composite literal as `new T(Field: …)` — a call to the **fieldwise constructor** go2cs-gen generates for a `[GoType]` struct, whose parameter list spells out **every** field's type — so binding that call needs every field type's assembly. `testing/quick`'s `Config` holds a `Rand *rand.Rand`, so `image/draw`'s `quick.CheckEqual(orig, sqDiff, &quick.Config{MaxCountScale: 10})` fails `CS0012: The type 'rand_package.Rand' is defined in an assembly that is not referenced` *at the `new quick.Config(…)` expression*, with `math/rand` in no import list on either side. No interface closure can ever reach it — `Rand` is a **struct**, so the shape is invisible to a base-interface walk; it is the same missing-declaration-edge defect one type-kind over.

The alias scan cannot reach either class: the named type *does* bind by name — its own package **is** referenced — and what is missing is a package named inside that type's own C# declaration. Three gates keep the closure minimal, because over-inclusion is its own defect — every extra reference is churn across the banked corpus plus a chance at a duplicate-type conflict:

- **Seed only from the files the test assembly COMPILES** (`referencedTypeSeeds` walks the retained syntax rather than iterating the `TypesInfo` maps, which also makes seed order deterministic). A Phase-4D compile-excluded Example/Benchmark-only file is analyzed — so its declarations still reach the manifest — but no C# is emitted for it, so it names nothing the compilation must bind. Seeding from one handed `compress/gzip` five references (`context`, `crypto/tls`, `mime/multipart`, `net/http`, `net/url`) reached through `http.Request`'s fields, from an `example_test.go` that is not compiled at all, and handed `go/token` a `go/ast` reference the same way.
- **Start the interface walk from named types, never from whole packages,** and follow bases **transitively** (`b.B : a.A : io.Writer` needs both `a` and `io`, because a base's own declaration must bind in turn). C# needs a base's assembly only when the derived interface is BOUND, and walking every exported interface of every referenced package would hand `io` to nearly the whole corpus through `fmt.State`'s structural io.Writer base — a reference no project that merely calls `fmt.Sprintf` requires.
- **Fire the struct-field edge only where a composite literal CONSTRUCTS the struct** — that, not mere value use, is what demands the field types, and one level suffices (the generated constructor's parameters default unless supplied, and a *nested* literal is itself a seed). Measured against the corpus: eleven banked packages hold `sync.Once`, `sync.Map` and `reflect.Value` **values** (strconv's package-level `atofOnce`, encoding/binary's `reflect.ValueOf`) and compile clean today with no reference to `sync/atomic` or `internal/abi` — so a "named by value" rule would add eleven references that nothing needs. `os.File`'s single `*file` field likewise never drags internal/poll, syscall and the rest of os's private graph in. An **EMPTY** literal carries the edge only for a struct declared in a **ROOT** package, and the boundary is ACCESSIBILITY rather than a package list: `T{}` renders `new T(nil)` — go2cs-gen's dedicated nil constructor, which names no field — but the FIELDWISE overload stays a resolution candidate wherever it is visible, and binding a candidate means binding its parameter types. That constructor is `internal` for any struct with an unexported field, so outside its assembly and friends it is not a candidate at all (which is exactly why mime's `once = sync.Once{}` and testing/quick's `return reflect.Value{}, false` need nothing), while a root package's struct IS visible that way — recompiled into the test assembly, or reached through the white-box `InternalsVisibleTo` grant. math/rand/v2's `*p = ChaCha8{}` failed `CS0012 … 'chacha8rand_package.State' … assembly that is not referenced` at the `new ChaCha8(nil)` expression, with internal/chacha8rand named in no import list on either side.

A **root's own package is never an addition**, and the external variant makes that load-bearing rather than theoretical: `go/packages` names it `<pkg>_test`, which resolves to no importable package at all, so a `bytes_test` struct literal whose field type is declared beside it fails the whole conversion with F14b's loud `resolve test project dependency "bytes_test": package bytes_test is not in std`. Every root's `PkgPath` therefore seeds the already-referenced set (its types compile into the test assembly, or bind through the production project reference the template already carries).

`testing` is skipped as a walk **source** (`closureWalkable`): it binds to the hand-owned `core/testing` shim per F15b, whose C# declarations are authored by hand and share only *names* with Go's — Go's `testing.T` embeds a `common` holding `io.Writer`, `time.Time`, `sync.RWMutex` and a dozen more, none of which the shim's two-field `T` declares, so inferring C# edges from the Go declaration there is simply invalid. Every `-tests` compilation names `testing.T`, making this the widest over-inclusion the struct rule could possibly cause; nothing is lost, since the shim's reference is fixed in the project template.

Each interface step runs the same `types.Implements` candidate match the converter uses at the declaration site (identical exported / non-alias / non-generic / method-set / strictly-fewer-methods gates) — deliberately taken *before* that function's covered-by-embed skip and minimal-covering-set prune, so the result is a superset of the emitted base list and no emitted base's assembly can be missing. Only the declaring package's own imports are scanned, so a same-package base needs no separate visit (an interface implements its base's bases too, so those candidates are found directly), and the output is a sorted set, so the map-ordered walk stays deterministic. It is a pure project-reference concern (the manifest dependency list stays import-derived), empty for a package whose named types carry no foreign declaration edge (unicode/utf8/path/cmp/itoa → unchanged), and `{io}` for io/fs, hash/maphash and crypto/hmac. **Measured minimality:** regenerating every banked package's `.tests.csproj` and diffing is the instrument, and it is what rejected each looser rule in turn — an un-file-scoped seed drifted `compress/gzip` (context, crypto/tls, mime/multipart, net/http, net/url) and `go/token` (go/ast); a "named by value" struct edge drifted eleven more (encoding/binary, errors, hash/crc64, internal/fmtsort, math/rand, math/rand/v2, mime, os/signal, strconv, strings, testing/quick); an "any composite literal" edge — the empty form *unscoped* — still drifted three (encoding/binary, mime, testing/quick). Every one of those gates remains a **zero-drift** rule. The **root-scoped** empty-literal edge is measured the same way, and it is the one edge that is deliberately not zero: at the 63-package roster (2026-07-31), converting every package twice on one binary with that edge neutered and restored, it changes exactly **one** project — `math/rand/v2`, by exactly **one** line, `<ProjectReference Include="$(go2csPath)core/internal/chacha8rand/internal.chacha8rand.csproj" />`. That is the root set itself and nothing else: the three foreign-struct negatives (`mime`, `testing/quick`, `encoding/binary`) that rejected the unscoped form stay byte-identical, as does every other banked package. (Run that probe with the converter's exit status checked — 63/63 conversions must succeed on **both** sides: a conversion that *fails* writes no csproj, so an ignored failure reads exactly like "no drift", and that false-clean is how the `<pkg>_test` self-reference below first hid.) Guarded by the `TestSelectTestProjectModel`, `TestRecordsRequireProductionAnchorGatesReferenceModel`, `TestWriteTestProjectReferenceModelBindsProductionProject`, `TestReferenceModelSeedAnchorsTestClassOnly`, and `TestDeclarationClosureImportsSurfacesForeignDeclarationEdges` (foreign-package interface base, own-package structural base, transitive `b → a → io`, the `fmt.State` narrowing with its positive control, the empty-closure case, the `*rand.Rand` struct field at a composite literal **with its by-value-only negative**, a func-typed field's signature, the one-level field boundary, the compile-excluded-file negative with its own positive control, the external-variant self-reference negative, and the `testing`-is-never-walked negative) converter unit tests; unicode (28 tests, incl. `TestSpecialCaseNoMapping` and the `testing.Benchmark`-driven `TestCalibrate`) is the first package it validated under the reference model, and crypto/hmac (172 tests) the first the generalized foreign-package rule unblocked. (io/fs compiles clean under this closure but does not yet *validate*: its `TestCVE202230630` globs a 10 001-separator pattern whose faithful `globWithLimit` recursion overflows .NET's fixed thread stack before Go's `depth > 10000` guard fires — a Go-growable-stack vs .NET-fixed-stack divergence in the test host, orthogonal to the reference closure.)

**The third closure edge — a MEMBER ACCESS (2026-08-03, r38-gob).** The two edges above are edges of a named
TYPE's declaration; the third is the edge of an **access**: resolving `x.M` requires BINDING `x`'s type, and
when `x` is declared in another package that type is spelled nowhere in this compilation — not in an import,
not in an alias `using`. `unique` is the witness: `handle.go` declares `var cleanupMu sync.Mutex`, the
white-box suite calls `cleanupMu.Lock()`, and the test project referenced no `sync` — `handle_test.cs` failed
`CS0012 … 'sync_package.Mutex'` twice, no host linked, and the package's suite had never been measured. Adding
the production package's whole import list instead is the looser rule the reference model exists to avoid (it
would have added `internal/stringslite` and `unsafe` here, and far more elsewhere). The seed is every
`*ast.SelectorExpr`'s BASE type, `namedTypesIn`-expanded, then `reach()`ed and `enqueue()`d like a field edge;
a package-QUALIFIED selector (`sync.Mutex`, `lib.F`) is not this shape at all — its base is a `PkgName`, which
has no type — and the import that spells it already carries the reference.

**Measured minimality — two restrictions, each of which the roster REJECTED a looser form of.** The probe is
the one this section prescribes (regenerate every banked package's `.tests.csproj` and diff, with the
converter's exit status checked on both sides: 73/73 conversions must succeed, since a failed conversion
writes no csproj and reads exactly like "no drift"). (1) *The receiver, not every named declaration.* "The
type of every var/const/func the compilation NAMES" is equally true of C#'s binding rules in the abstract, and
drifts **23 of 73** — `bufio` into compress/bzip2 and image/gif, `internal/abi` + `internal/reflectlite` into
errors, three references into hash/crc32 — all of which compile clean today with none of it. Naming a
declaration does not force its signature to be materialized; accessing a member of it forces the receiver's.
(2) *`_test.go` files only, scoped per FILE.* Under the reference model the production sources are not in this
compilation — they are in the referenced assembly, which carries its own references — and seeding from them
too still drifts **13** (crc32's `castagnoliOnce.Do`, math's `cpu.X86`, …). Per-file rather than per-package
because go/packages loads the INTERNAL test variant with the production files alongside its own, so a
package-level gate lets every production receiver back in. With both restrictions the roster is **zero-drift**:
unique's single `<ProjectReference Include="$(go2csPath)core/sync/sync.csproj" />` is the only line that
changes across all 73. Under the **recompile** model the edge is a no-op by construction — that model adds
`production.Imports` wholesale, so a production receiver can never be an addition. Guarded by
`TestDeclarationClosureImportsSurfacesMemberAccessEdges` (the unique shape through real test variants, plus
the named-but-not-accessed, production-source-only and package-qualified negatives). One more rule fell out
of the family's own `testing`-never-walked negative: `testing` must never be an ADDITION either, for the same
reason it is never a walk SOURCE — its reference is fixed in the project template, which is why the caller
strips `"testing"` from the import-derived set rather than passing it through as already-referenced, so
`reach()` now honours `closureWalkable` too.

**The fourth and fifth closure edges — a ZERO-VALUE DECLARATION and a RECORDED INTERFACE BASE (2026-08-07, r43f).**
`log` and `go/scanner` sat on the build-blocked list with one CS0012 family each, and each named a demand the
first three edges structurally cannot see.

*`log` — the zero-value declaration.* `TestNonNewLogger` writes `var l Logger`. There is no composite literal
anywhere in it, so the struct-field edge's `*ast.CompositeLit` seed never fires — but Go's zero value of a
struct is not `default(T)` in the emission. The converter renders the declaration as a **constructor call**:
`ref var l = ref heap(new Logger(), out var Ꮡl)` for the address-taken shape (`escapeAnalysisOperations`),
`new Logger()` otherwise (`visitValueSpec`). C# overload resolution must materialize every **accessible**
constructor's signature before it can choose one, and under the white-box `InternalsVisibleTo` grant the
package-under-test's `internal` fieldwise overload is accessible — so `log_test.cs` failed
`CS0012 … 'atomic_package.Pointer<>' … assembly 'sync.atomic'` at `new Logger()`, `Logger`'s three `atomic.*`
fields being spelled in no import list on either side. This is the **empty-literal edge's exact demand by
another route**, so it feeds the same seed (`constructedEmpty`) under the same ROOT/accessibility gate, scoped
to `_test.go` files for the member-access edge's reason. `math/rand/v2`'s `*p = ChaCha8{}` and log's
`var l Logger` are now one rule.

*`go/scanner` — the recorded interface base.* A converted **concrete** type names no interface in its own
emitted declaration — `[GoType("[]ж<ΔError>")] partial struct ErrorList;` names none at all. Its bases arrive
as the **VALUE-form `[assembly: GoImplement<T, I>]` records** its package emits, which the go2cs-gen
`ImplementGenerator` realizes as `partial struct ErrorList : global::go.sort_package.Interface` **inside the
declaring assembly**. The metadata type therefore *declares* that base, and binding **any** member on it makes
the compiler resolve the base list: `list.Sort()`, `len(list)`, `Ꮡlist.RemoveMultiples()` and the generated
`ErrorList`→`error` value adapter's own `m_value.Equals(…)` all failed
`CS0012 … 'sort_package.Interface'`, thirteen times across the suite, with `sort` in no test import and no
alias `using`. The edge hangs off the **member-access seed** the third edge already computes — it is what
binding that receiver additionally costs — and interfaces are excluded from it because the base walk covers
them already.

**Measured minimality — and here the instrument rejected the rule C#'s binding rules appear to justify.**
"The interfaces the receiver's type implements, taken from the declaring package's imports" is the natural
`go/types` statement of the edge, mirroring `interfaceBaseCandidates` one type-kind over. It drifts **16 of
the 96** banked projects. The reason is that a `GoImplement` record exists only where the converter converted
a **cast**, so Go satisfaction wildly over-approximates the emitted base list: `os.File` satisfies
`syscall.Conn` and handed `syscall` to thirteen projects (compress/{flate,gzip,lzw,zlib}, image and four
image codecs, io, math/rand/v2, regexp, strconv) — while `os` records `File` only against `io/fs.File` and
`io.Writer`, and both **POINTER-form**, which generates an adapter *class* (`FileжWriter`) rather than a base
on the type and so demands nothing of a member binding; `bytes.Buffer` satisfies most of `io` and handed `io`
to `sort` and `unicode/utf8` though `bytes` emits **no records at all**; `internal/buildcfg`'s `Stringer`
handed it `fmt` from an equally empty set. All sixteen compile clean today with none of it. So the **records
are the gate** and satisfaction merely supplies the candidate universe: `packageImplementBases` parses the
package's own freshly-written `package_info.cs` (value form only — `parseExportedValueImplementLines` already
drops the pointer form), keys the interfaces by the implementing type, and the walk fires only where the
receiver's type *and* the candidate's package both match a record. Keyed **per type**, because os's one
genuine `syscall` record is for `rawConn`, not `File`. With that gate the roster is **zero-drift**: converting
all 96 banked packages on both binaries changes not one line of one `.tests.csproj`. The lookup is scoped to
the package under test, whose `package_info.cs` this run has just written; a foreign type's base list is its
own package's record set in its own `package_info.cs`, and no measured case has ever demanded one — widening
is the same lookup pointed at that file.

*`go/types` — the widening, measured.* That last sentence held until `go/types`' own suite, whose host build
produced exactly one error: `check_test.cs(78,66): CS0012 … 'sort_package.Interface'`, at `len(list)` in
`if list, _ := err.(scanner.ErrorList); len(list) > 0`. The type is `go/scanner`'s `ErrorList` — the very
type the same-package edge was built for, one package over. `go/scanner` **is** referenced (the suite imports
it) so `ErrorList` binds; what does not resolve is the base list `go/scanner`'s **own** record set realizes on
it, and `sort` sits in `go/types`' PRODUCTION project references (the package imports it) and in no test
import. So the records are now read **per declaring package**: a root type's from the `package_info.cs` this
run just wrote, a foreign type's from that package's, resolved through the same `getImportPackageInfo` route
the `<ImportedTypeAliases>` block already reads a dependency's metadata by (layout L3's per-GOOS placement
included) and memoized per import path. An unreadable or absent file yields no edge, exactly as it does for
the package under test. The gate itself is untouched — same value-form records, same per-type key, same
candidate match — which is why the widening does not reopen the 16-of-96 question: it points one lookup at a
different file.

*The other half: the member bindings that spell no selector.* The same measurement moved the edge's SEED as
well. `check_test.go` never writes `list.Sort()` — it binds `ErrorList` only through `len(list)` and
`range list` — so the member-access seed (`x.M`, the third edge's) never saw it. Each of those lowers to a
member on the value's type all the same: golib's generic `len`, the emitted enumeration, an indexer. The seed
therefore also carries the types a compiled `_test.go` binds a member on through a **builtin call, a range, or
an index/slice** (`memberBound`), and that set feeds the implemented-interface edge **only** — never
`reach`/`enqueue`, because the value is spelled by a package the suite already imports. The boundary the
member-access edge measured stays exactly where it was, and is pinned by the same negatives: a test that
merely NAMES a value of such a type, or PASSES IT ALONG to a function with an exact parameter type
(`var r Rows; Order(r)`), binds nothing on it and surfaces nothing. Seeding this edge from every named type
instead would cross that line — the existing negative fails immediately — which is why the shape is the
member binding rather than the mention. `go/types`' reference set gains exactly one line, `core/sort`, and the
host builds clean. (Guarded by `TestDeclarationClosureImportsSurfacesForeignImplementedBases` — the foreign
positive plus four negatives (no resolver, empty record set, a record keyed to another type, and the
name-and-pass boundary) and the range/index forms — and by
`TestForeignImplementBasesResolverReadsDeclaringPackageInfo`, which pins the value-form-only parse, the
memoization, and the silent-nothing on an absent file. Both fail against the pre-fix converter, and each half
of the fix fails it independently.)

Guarded by the `TestDeclarationClosureImportsSurfacesZeroValueVarDeclarations` (the log shape through real
test variants, plus the foreign-struct accessibility negative and the production-source scoping negative) and
`TestDeclarationClosureImportsSurfacesImplementedInterfaceBases` (the scanner shape, plus the three negatives
that pin the gate: **no records ⇒ no edge** even where `types.Implements` says yes, a record keyed to another
type, and the receiver restriction) converter unit tests. `go/scanner` **validates 11/11** on the fix.
`log` builds and runs for the first time — seven of its nine test functions agree with `go test` — but does
**not** bank: two roots stand behind the closure one, `runtime.Caller`'s unimplemented `getcallersp` stub
(`TestAll`, the same row `testing/slogtest` carries) and `TestDiscard`'s exact allocation-count assert
(the established `alloc-profile` class). ⚠ The `Caller` root closed on 2026-08-07 and `log` **still** does
not bank — `TestAll` asserts the GO source file's own extension and line numbers, which the converted
program does not carry; see *`runtime.Caller` works by severing the FUNNEL* below.

### Under the RECOMPILE model the test half CONTINUES the production emission (the `productionSeed`)

The `recompile` model is the only one where the converted `_test.go` files land in the **same C# class**
as production sources this run does not rewrite. Everything the converter numbers or claims per package
is therefore a shared, immutable name supply, and every counter that restarts for the test emission pass
re-mints a name already on disk. Two such supplies were already pinned — lifted type names
(`productionLiftedTypeNames`) and hoisted big-constant ordinals (`productionHoistedConstOrdinals`) — and
the same rule turned out to be owed by three more. All five now travel in one `productionSeed` struct,
captured in `convertTestVariants` from the production run's live state (it ran moments earlier in the
same process) **before** the first variant's `resetPackageState`, and installed by `convertTestVariant`
for the **INTERNAL** variant under the recompile model alone:

| Supply | Emitted name | The collision |
|:--|:--|:--|
| lifted type names | `Δtypeᴛ1` | two lifts of differently-shaped anonymous structs (encoding/gob) |
| hoisted big-constant ordinals | `maskᶜ1` | two `const mask = <big>` hoists |
| import force hooks | `initᴛᴛimportꓸcryptoꓸsha256` | production and test files repeating one import |
| the blank-identifier counter | `_ᴛ1ʗ` | a blank package-level `_` in each half |
| `func init()` ordinals | `init` / `initΔ1` | production and test files each declaring `func init()` |

The last three are `crypto/x509`'s, and they are ordinary Go, not exotica. `x509.go` and `x509_test.go`
both `import _ "crypto/sha256"` and `_ "crypto/sha512"` — a test repeating a production blank import is
what a test that exercises those registrations does — and each half emitted the same
`[GoInit] internal static void initᴛᴛimportꓸcryptoꓸsha256()` into `x509_package`: CS0111. The
`x509_package` class likewise already held `_ᴛ1ʗ` for `pem_decrypt.go`'s blank const heading an iota
block when `oid_test.go`'s `var _ encoding.BinaryMarshaler = OID{}` re-minted it (CS0102), and
`root_windows.go`'s `func init()` when `x509_test.go`'s own `init` claimed the bare name again (CS0111).

The import hook is the one whose OWNERSHIP is worth stating rather than merely its uniqueness:
exactly one hook per (assembly, imported package) — Go initializes an imported package once per program
and a .NET module constructor runs once per assembly — and the **production** half owns it whenever its
file is in the compilation, because that file is the one a `-tests` run cannot rewrite. The seed is
skipped for the EXTERNAL variant and for both reference models for one reason, stated once: there the
names land in a different class (`<pkg>_test_package`, the friend bridge) or a different assembly
(production, referenced), so they may be reused freely and seeding would only churn banked emissions.

Guarded by `TestTestVariantPinsProductionBlankImportForces`,
`TestTestVariantContinuesProductionBlankIdentifierCounter` and
`TestTestVariantContinuesProductionInitOrdinals`, each pinning both directions — unseeded the test half
legitimately takes the first name, seeded it must step past the production one.

### A recompile-model test project compiles the production sources — so it owes their references and their per-GOOS half

Two more `crypto/x509` roots, both of the same shape: the recompile model makes the production `.cs`
compile items of the test project (`writeTestProject`), and two places that enumerate or probe those
files did not describe what actually compiles.

**The B2c alias scan read only the test-emitted files.** The tests csproj sets
`DisableTransitiveProjectReferences`, so every assembly the compilation names must be a DIRECT reference,
and the alias scan is what finds the ones no import list mentions (see the reference-closure rule above).
Under the recompile model a production file's `using` aliases are references the TEST project owns — and
they were never scanned. The omission hides in the ordinary case, because a production file's aliases are
usually its own package's direct imports, which the import-derived set already carries; it bites where the
alias names a package reached only transitively. `x509.cs` and `pem_decrypt.cs` emit
`using hash = hash_package;` because `crypto.Hash.New()` RETURNS `hash.Hash` — `hash` is in no import list
of `crypto/x509` and in no reference of its own production csproj, which compiles anyway precisely because
it does *not* disable transitive references. The test build failed **CS0246 ×2 inside the production
files**. `testProjectAliasScanFiles` now names the scan set as "what the test project compiles", with the
production half included under the recompile model and excluded under the reference models (there those
sources compile in their own project and their aliases are that project's concern). Guarded by
`TestAliasScanCoversRecompiledProductionSources`, which pins the model gate as well as the find.

**The enumeration and the static-ctor probe were both flat-only, and layout L3 is not flat.** An L3
package keeps its platform-varying sources in `<goos>/` and its production csproj compiles one folder via
`$(GoTargetOS)/*.cs`; a test project lists compile items explicitly, so the same selection has to be made
when enumerating them (`productionCSFiles`) and when asking whether a production `package_init.cs` exists
(`platformLayoutPath`, the probe that decides whether the test side implements the erasable
`initᴛᴛtests()` hook or declares a static constructor of its own). `crypto/x509` is the corpus's only L3
package on the recompile model — every other L3 suite takes a reference model, where the production
ASSEMBLY carries its per-GOOS half — so neither gap had ever been exercised. Together they cost 187
errors reported against the TEST files rather than the missing folder (`Verify`, `VerifyOptions`' fields,
`loadSystemRoots`, `domainToReverseLabels`, every error type's `Error()`), plus a second
`static x509_package()` beside the real one. Guarded by
`TestProductionCSFilesTakeTheTargetPlatformFolder` (target folder taken, non-target folder not, per-GOOS
`package_init.cs` included, flat package unchanged) and `TestProductionInitProbeFollowsPlatformLayout`.

## References and the compile set

### A package-qualifier `using` in a converted TEST SOURCE contributes a project reference

Test projects set `DisableTransitiveProjectReferences=true`, so an assembly the package reaches only *transitively* is invisible to the test compile (CS0234). `aliasReferenceImports` covers this by scanning `using` ALIASES for namespace tokens of packages in the transitive import closure and adding a direct project reference for each. It formerly scanned only the two **metadata** files, despite its contract covering a file-local package-qualifier `using`; it now scans the converted `*_test.cs` **outputs** as well.

The shape this misses otherwise has no textual import at all: math/rand's `default_test.go` does not import `os/exec`, but `testenv.Command(…)` **returns** `*exec.Cmd`, so the emitted `default_test.cs` binds `cmd.Value.Env` and `cmd.CombinedOutput()` through the os/exec assembly while `exec.` never appears in any import list. Scanning the emitted source finds the qualifier `using` and emits `$(go2csPath)core/os/exec/os.exec.csproj`. The manifest's dependency list stays import-derived — alias targets are purely a project-reference concern — and the scan is additive, so a package whose sources introduce no new qualifier is unchanged.

### A test project's references cover UNROOTED alias targets (single- AND multi-segment)
A `-tests` project sets `DisableTransitiveProjectReferences`, so its references are the
direct-import closure plus whatever `aliasReferenceImports` recovers by scanning the emitted `using`
aliases for namespace tokens. The scan matched only the ROOTED token (`go.hash_package`), but a
SINGLE-SEGMENT package emits its alias UNROOTED — `using hash = hash_package;` inside
`namespace go.math.rand`, where C#'s outward lookup finds the class in the enclosing root namespace
with no qualifier. math/rand/v2's `chacha8_test.cs` needs `hash` purely because `sha256.New()`
RETURNS `hash.Hash`, so the package appears in no import list and only this scan could have found
it: the reference went missing and the build failed CS0246 on `hash_package`. The scan now also
carries a bare token per single-segment package, matched on a SEGMENT boundary (`target == token` or
`target` starts with `token + "."`) — a substring test would let `hash_package` match
`go.hash.maphash_package` and pull in a package nothing references. Guarded by
`TestAliasReferenceImportsMatchesUnrootedSingleSegmentAlias` and
`TestAliasReferenceImportsDoesNotMatchAcrossSegmentBoundaries`.

A **MULTI-segment** package hits the identical gap when the test's enclosing namespace SHADOWS the
root `go`. From `namespace go.math` the alias for os/exec is emitted ROOTED — `using exec =
go.os.exec_package;` (math/rand's `default_test.cs`, caught by the `HasSuffix(target, token)` arm) —
but from a namespace whose first segment re-binds `go`, the alias is emitted UNROOTED and relies on
C# outward lookup: go/doc/comment's `std_test.cs` (in `namespace go.go.doc`) and internal/abi's
`abi_test.cs` (in `namespace go.@internal`) both emit `using exec = os.exec_package;`, again purely
because `testenv.Command(…)` RETURNS `*exec.Cmd` so os/exec appears in no import list. The rooted
token (`go.os.exec_package`) is now ALSO matched when it ends with the unrooted target after a
segment boundary — `HasSuffix(token, "." + target)`, so `os.exec_package` matches
`go.os.exec_package` while the leading `.` anchor keeps `os.exec_package` from matching an unrelated
`go.notos.exec_package`. This was the single shared root cause blocking both internal/abi and
go/doc/comment (CS0246 on the `os` namespace). Guarded by
`TestAliasReferenceImportsMatchesUnrootedMultiSegmentAlias` and
`TestAliasReferenceImportsUnrootedTailAnchoredOnSegmentBoundary`.

**An emitted CONVERSION RECORD names packages no import list and no alias mentions.** A `using` alias
is not the only line in the test metadata that must BIND: go2cs-gen realizes every
`[assembly: GoImplement<…>]` / `[assembly: GoImplicitConv<…>]` record into a generated adapter,
partial or operator, so both generic arguments have to resolve at the attribute itself. The converter
records an interface pair from a type's *use*, and that use can be entirely implicit —
os/signal's test does `cmd.Stdout = &buf`, whose os/exec field type is `io.Writer`, so
`package_test_info.cs` carries

```csharp
[assembly: GoImplement<strings_package.Builder, io_package.Writer>(Pointer = true)]
```

while `io` appears in no import list of the production package or its tests, and in no alias. Under
`DisableTransitiveProjectReferences` that is CS0246 on `io_package` at the attribute line, plus a
cascading go2cs-gen **CS8785** (`ImplementGenerator failed … second generic type argument must be an
interface`) once the unbound interface degrades to an error type — the generator then contributes
nothing and the whole package's adapters vanish. The scan therefore also reads the record lines,
extracting each type reference's **package-class qualifier** — everything up to and including the
first segment ending in `_package` (`io_package`, `go.io.fs_package`, `go.@internal.abi_package`).
That is deliberately the qualifier, not the whole type reference: it has exactly the shape a `using`
alias TARGET has, so the *same* three token-match arms above decide both, with no second matcher to
keep in sync. Only the record's generic argument list is scanned (first `<` to last `>`, so a nested
`ж<…>` argument is covered whole) — an attribute's `(Pointer = true)` / `(ValueType = "…")` payload is
metadata, and the `ValueType` is a string, not a reference. Additive as before, and the manifest's
dependency list stays import-derived. This is what lets **os/signal** validate (its `TestCtrlBreak`,
1/1 vs `go test`). Guarded by `TestAliasReferenceImportsMatchesConversionRecordPackages`,
`TestAliasReferenceImportsMatchesConversionRecordQualifierShapes` and
`TestAliasReferenceImportsIgnoresConversionRecordAttributePayload`.

**Referencing a `go/*`-package TYPE loses a root segment because the path's own `go` collides with the root namespace.** A `go/ast` type reference renders correctly as `go.go.ast_package.X` (root `go` + the path's `go.ast` → namespace `go.go`, class `ast_package`), but `convertToCSTypeName` then strips the *leading* `go.` as a redundant root (bodies live inside `namespace go`), leaving `go.ast_package.X` — namespace `go`, which has no `ast_package` (CS0234/CS0426 in the go/* consumers go/doc, go/printer, go/internal/typeparams, whose GoImplement attributes and `using` aliases both carry the stripped form). The two rooting helpers now recognise this: `isStrippedGoPathPackageRef` splits the ref at its first `_package` class segment and tests the *namespace* portion against `packageChildNamespaces` (the current package's rooted import-closure namespaces): the ref is stripped iff that namespace is NOT already a real rooted namespace but *becomes* one when the root `go.` is prepended. This is a **membership** test, not a string-shape test, so it recognises a stripped go/*-package ref at any depth — `go.ast_package` (ns `go`✗ → `go.go`✓), `go.build.constraint_package` (ns `go.build`✗ → `go.go.build`✓, three-segment `go/build/constraint`), `go.doc.comment_package` (ns `go.doc`✗ → `go.go.doc`✓) — while leaving a genuinely-rooted ref alone (`go.io.fs_package` — ns `go.io` is already real). (The earlier two-segment string heuristic — "the class segment sits immediately after `go.`" — recognised only the depth-one `go.ast_package` shape and silently missed the three-segment `go/build/constraint` and `go/doc/comment` sub-package refs, which are string-indistinguishable from a correctly-rooted `go.io.fs_package`; the membership test is what disambiguates them.) `rootQualifySubNamespaceTypeRefs` (the assembly-scope GoImplement/GoImplicitConv attributes) re-roots the stripped form to a bare `go.go.ast_package`; `rootQualifyIfAmbiguous` (the in-namespace `using` aliases) re-roots to `global::go.go.ast_package` — always `global::`, because a bare `go.go.<pkg>_package` re-binds its leading `go` to the nearest enclosing `go` from *any* importer (a go/*-package's own `go.go.*` namespace, and equally `internal/pkgbits` at `go.internal.pkgbits` resolving the second `go` inside `go.go`, CS0234). This un-blocks the whole go/* chain at the rooting level (go/doc's own-errors 17 → 1); each go/* package still needs its remaining per-package residuals (e.g. a methodless-func-type's `[GoTypeAlias]` still names an inline-rendered `ΔFilter`) to fully compile. The depth-one shape is now guarded by `GoNamespaceShadow` (its `go/nsshadow` nested module's import renders through `isStrippedGoPathPackageRef` → `using nsshadow = global::go.go.nsshadow_package;`); the multi-segment sub-package depth (`go/build/constraint`) remains census-verified only — the A/B reconvert-diff showed only the four `go/build/constraint`- and `go/doc/comment`-importing packages, go/build, go/doc, go/parser, go/printer, gaining the corrected double-`go` rooting, with the depth-one `go.go.ast_package` refs unchanged and zero collateral.

### An Example/Benchmark-ONLY test file is dropped from the compile set (Phase-4D file exclusion)

`Example` and `Benchmark` declarations are uniformly **Phase-4D-deferred** — `discoverTestDeclarations` records them in the manifest with status `unsupported` ("… execution is deferred to Phase 4D") and the differential oracle filters them from both sides (`eligibleTerminalTestResults` admits only `included` `test`-kind declarations). The **option-a ruling** (2026-07-24) extends that deferral from the *declaration* to the *file*: a `_test.go` file is dropped from the `-tests` conversion/compile set (`selectCompileExcludedTestFiles`) iff **both**

1. **every RUNNABLE declaration it contributes is a Phase-4D-deferred `func Example*`/`func Benchmark*`** — imports do not count as declarations, and (since 2026-08-15) neither do pure `type` declarations and methods; any top-level `var`/`const`, or any other plain func (a `Test`/`TestMain`/`Fuzz` func, an `init`, or a *mis-signatured* Example/Benchmark), disqualifies the whole file (conservative by design; `TestMain`/`Fuzz` are deliberately out of scope). The Example/Benchmark classification is the **exact** `isPhase4DExcludedTestFunc` predicate `discoverTestDeclarations` uses (no receiver, no results, no type params, and either a zero-parameter `Example*` or a single-`*testing.B`-parameter `Benchmark*`), so a file qualifies only when it truly contributes nothing to the run registry; **and**
2. **no RETAINED test file references any object the file declares**, resolved by go/types **object identity** across the loaded variant set (never filename or text) — a promotion **fixpoint** over both variants, so an Example a retained test takes by value (`[]func(){ExampleWired}`) keeps its file compiled, and a candidate promoted back to retained can in turn pull further candidates in.

The predicate is pure `go/ast`+`go/types`: go/token's `example_test.go` declares **only** `func Example_retrievePositionInfo()` at top level (its `type p = token.Pos` / `const bad` / `func ok` live inside a raw-string literal fed to `parser.ParseFile`), so a text scan would wrongly disqualify it while the AST predicate correctly qualifies it. Excluding it is the **demonstrated consumer**: that external `package token_test` file, recompiled into go/token's mixed whitebox+blackbox test assembly, names `token.FileSet`/`ΔPos`/`Token` — types the referenced `go.parser`/`go.ast` assemblies surface from the *production* `go.token` assembly while the recompile makes a *second*, local copy — CS0012. With the file excluded, go/token compiles.

**Metadata representation — discovery stays intact; only emission + compile-membership are dropped.** The excluded file's Example/Benchmark declarations still appear in the manifest under their existing disclosed-unsupported status, and its source is listed in `testSources` with a distinct `example-benchmark-only` status. This is *required* for oracle exactness, not cosmetic: `go test` **runs** an Example that carries an `// Output:` comment (go/token's does), so it appears in the raw `go test -json` stream, and the **F6 census gate** (`manifestCensusGaps`, computed over the *unfiltered* Go results) fails the comparison for any run declaration the manifest does not account for. Dropping the declaration would trip the census; keeping discovery — and dropping only the file's `.cs` emission and its `.tests.csproj` compile item — keeps every already-filtered Example filtered and the differential oracle exact. The predicate lives in `convertTestVariants` (both project models honor it): discovery runs over every test file, emission over the non-excluded subset.

**Blast radius (banked packages).** Because the policy changes which files a *banked* package compiles, a GOROOT scan of every committed test suite (with go/token as the positive control) found **18** packages with a qualifying file — each re-validated with identical `Test` counts and rebanked to remove the excluded `*_test.cs`, its `.tests.csproj` compile item, the project references + `using` aliases the excluded file **exclusively** pulled in (verified by import analysis, e.g. math/cmplx's fmt, encoding/hex's os/io.fs/log), and — where the excluded external file was the sole contributor to the external anchor — the orphaned `package_info_external_test.cs`. One subtlety is guarded operationally: the metadata writer **merges** with the committed anchor, so a `GoImplement`/`GoImplicitConv` record contributed *only* by the excluded file (math/rand's `GoImplement<text.tabwriter.Writer, io.Writer>`, from an Example that casts a `*tabwriter.Writer`) survives the merge as a stale record referencing a now-unreferenced assembly (CS0234); regenerating the anchor from a clean state (as a whole-corpus reconvert would) drops it. The 17 packages with no qualifying file stay byte-identical.

**Condition (1) admits pure TYPE declarations and METHODS** (2026-08-15, the `crypto/tls` lane). The original wording — every top-level declaration is an Example/Benchmark — is the shape go/token's `example_test.go` happens to have, and `crypto/tls`'s is the same file in every way that matters: the package's ONLY black-box file, every runnable thing in it an Example. It differs in one respect — its Examples need an `io.Reader` to hand `Config.Rand`, so it declares `type zeroSource struct{}` and one `Read` method — and that single helper kept the whole file compiled, producing precisely the failure this ruling exists to prevent: `http.Transport`'s `TLSClientConfig` field names `tls_package.Config` in the PRODUCTION assembly while the recompile makes a second local copy, so the field is unnameable — **CS0012 ×3** at `example_test.cs` 88/99/198, three of `crypto/tls`'s four build errors. Adding the production reference cannot fix that (the two `Config`s stay distinct types and CS0012 merely becomes CS0029); the file must not be compiled. A type declaration and its methods are admissible because they have no RUN-TIME behavior of their own — nothing executes at package init — and any use by a retained file is a reference condition (2) already resolves. That last clause is load-bearing and is why the type and method objects are now recorded in `declared`: widening condition (1) without it would have silently disarmed condition (2) for exactly the declarations it just admitted. Everything else stays disqualifying, deliberately: a `var`/`const` initializer can carry side effects and a plain helper func can be `init()`, neither of which any reference edge would reveal.

Guarded by the `TestSelectCompileExcludedTestFilesDropsExampleAndBenchmarkOnly` (positive: external Example-only + internal Benchmark-only), `TestSelectCompileExcludedTestFilesDropsExampleWithHelperType` (the crypto/tls widened-arm positive), `TestSelectCompileExcludedTestFilesKeepsHelperTypeUsedByRetainedTest` (condition 2 over the widened arm — the disarm this change had to avoid), `TestSelectCompileExcludedTestFilesKeepsExampleWithTopLevelVar` (condition 1 negative), `TestSelectCompileExcludedTestFilesKeepsReferencedExample` (condition 2 fixpoint), and `TestSelectCompileExcludedTestFilesKeepsTestMainAndFuzzOnly` converter unit tests.

---

[Index](README.md)
