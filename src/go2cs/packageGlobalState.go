// packageGlobalState.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// This file owns the converter's PACKAGE-WIDE state: the registries every file of the package
// being converted reads from and writes to, plus the markers and limits that describe the
// conversion itself.
//
// Why globals at all: the converter visits a package's files CONCURRENTLY, and a decision made
// while emitting one file (a lifted type name, an interface implementation record, a collision
// rename) frequently has to be visible to the others. These maps and sets are that shared
// blackboard. Access is serialized through packageLock, declared below with the state it guards.
//
// Their LIFECYCLE lives next door in packageStateOperations.go: resetPackageState clears every
// one of them between packages, which is what makes converting 300 packages in one process
// behave like converting them one at a time.
//
// Per-FILE state — anything that belongs to a single Visitor — lives in visitorState.go instead.

package main

import (
	"go/types"
	"sync"

	"golang.org/x/tools/go/packages"
)

// Converter-internal template sentinels and limits. The cross-language symbol constants
// (RootNamespace, PackageSuffix, the marker glyphs, ...) live in symbols.go, generated
// from the canonical symbol table src/core/go2cs/symbols.json - edit THAT file and run
// `go generate .` (or src/check-symbol-sync.ps1) to change them.
const OutputTypeMarker = ">>MARKER:OUTPUT_TYPE<<"

const UnsafeMarker = ">>MARKER:UNSAFE<<"

const ProjectReferenceMarker = ">>MARKER:PROJECT_REFERENCE<<"

// HandOwnReferencesLabel marks the one ItemGroup in a csproj that the re-mint PRESERVES rather than
// derives. A reference only HAND-WRITTEN C# needs — a whole-file hand-own or an `_impl.cs` companion
// binding a package the Go source does not import and the converted emission never spells — is
// invisible to every source-side and emission-side derivation, so it is DECLARED here once and
// carried across every future mint. See preserveHandOwnReferences (projectFileWriter.go).
const HandOwnReferencesLabel = `<ItemGroup Label="GoHandOwnReferences">`

const DynamicCastArgMarker = ">>MARKER:DYNAMIC_CAST_ARG<<"

// ValidationPackMarker occupies one whole line of csproj-template.xml and is substituted AFTER the
// template's printf verbs — like the friend-assembly grant, and for the same reason: a user-supplied
// `-csproj` template cannot know about the slot, and a 7th verb would append a `%!s(EXTRA ...)`
// diagnostic into every generated project. A stdlib conversion expands it into the block that packs
// the package's versioned validation proof sheet as VALIDATION.md; every other conversion replaces it
// with nothing, collapsing the line to the blank line the template has always had there.
const ValidationPackMarker = ">>MARKER:VALIDATION_PACK<<"

// Define package level variables
var packageName string

var packageNamespace string

var projectImports HashSet[string]

var exportedTypeAliases map[string]string

var importedTypeAliases map[string]string

// importedTypeAliasSourceDirs records which foreign dependency's SourceDir published each key in
// importedTypeAliases. go2cs's alias keys are short-name-only (`<rootPackageName>.<member>`), which
// cannot distinguish two DIFFERENT import paths that happen to declare the same package name —
// runtime/trace and internal/trace are both literally `package trace`. A reference that resolves
// (via go/types, which IS path-correct) to a package that never published anything under a given
// key must not adopt a DIFFERENT package's entry for it just because the short name matches; see
// aliasResolvedSelector's consultation of this map.
var importedTypeAliasSourceDirs map[string]string

// packageInlineFuncTypeNames records the names of this package's NON-GENERIC METHODLESS named func
// types — the ones visitFuncType renders inline as their base delegate and whose named declaration
// is skipped (there is no `<name>_package.<Δname>` type). Their exported-type-alias must NOT be
// emitted: a `[GoTypeAlias("Filter", "ΔFilter")]` makes consumers reference a nonexistent
// `go.go.ast_package.ΔFilter` (go/doc's `ast.Filter`, CS0426). Keyed by both the raw Go name and its
// core-sanitized form to match either exportedTypeAliases population site.
var packageInlineFuncTypeNames map[string]bool

// importedPointerImplements records `[assembly: GoImplement<T, Iface>(Pointer = true)]` lines
// parsed from IMPORTED packages' package_info files, keyed "pkgName|T|ifaceSimple" - the
// existence proof that the foreign assembly generated the public TжIface adapter class
// (io/fs's PathErrorжerror), so a cross-package pointer-to-interface conversion can
// reference it (os's `err = &PathError{...}`, CS0029 x38).
var importedPointerImplements HashSet[string]

// importedValueImplements records VALUE-form `[assembly: GoImplement<T, Iface>]` lines (plain or
// Promoted) parsed from IMPORTED packages' package_info files, keyed "pkgName|T|ifaceSimple" -
// the existence proof that the foreign assembly itself implements the interface on the value
// type, so a both-foreign value cast here converts implicitly and skips the local adapter.
var importedValueImplements HashSet[string]

// importPackageDirs maps a REACHABLE imported package's import path (the transitive closure, not
// just direct imports) to its on-disk source directory and Go package name, captured from the
// MODULE-AWARE go/packages graph at load time. It is the fallback resolver for cross-package
// references to a LOCAL/USER module (a `replace`d or co-located module), which the legacy
// go/build (GOPATH-only) resolver in getImportPackageInfo cannot find — including a type reached
// only through ANOTHER package's signature (aliasedElementTypeName's on-demand alias load).
// Reset and repopulated per package.
var importPackageDirs map[string]importedPackageMeta

type importedPackageMeta struct {
	Dir  string // package source directory (also the in-place converted-output directory)
	Name string // Go package name (the identifier used to qualify references in code)
}

// importedPackageSources maps a REACHABLE imported package's cleaned source DIRECTORY to its
// loaded go/packages entry (types + syntax), so a dependency's converted-name metadata can be
// derived from the dependency's OWN declarations when it has no package_info.cs in the output root
// — never converted, or converted into a different root (see foreignNameCollisions.go). Keyed by
// directory because that is the one identifier the go/packages graph and go/build's PackageInfo
// resolution always agree on: a GOROOT-vendored import is reached under a rewritten path. Reset and
// repopulated per package alongside importPackageDirs.
var importedPackageSources map[string]*packages.Package

// importedPackages maps a REACHABLE imported package's IMPORT PATH to the same loaded go/packages
// entry importedPackageSources holds by directory. The two exist for opposite lookups:
// importedPackageSources answers "what did the loader see at this directory", while this answers
// "what did the loader see for this import path" — the question visitImportSpec has in hand, and
// the one packageInitializesTransitively is asked.
//
// A GOROOT-vendored package is registered under BOTH spellings — the path the type graph reports
// (`golang.org/x/text/transform`) and the on-disk path the converter rewrites it to
// (`vendor/golang.org/x/text/transform`, see resolveGorootVendoredPath) — because the emission side
// has already rewritten by the time it asks. A single-spelling map would simply miss those 19
// packages, and, the lookup failing open, would force every one of them unconditionally: correct
// output, arrived at by accident. Reset and repopulated per package alongside importPackageDirs.
var importedPackages map[string]*packages.Package

// currentPackageSource is the loaded go/packages entry for the package being converted — the one
// handle importedPackages deliberately does NOT hold, since that map is the package's IMPORTS. The
// descriptor-carrier predicate needs syntax for the DECLARING package of a type it meets, and that
// package is as often the current one as an imported one. Reset alongside importedPackages.
var currentPackageSource *packages.Package

var constImportedTypeAliases HashSet[string]

// derivedTypeAliases marks the importedTypeAliases keys that were DERIVED from a dependency's own
// declarations (foreignNameCollisions.go) rather than parsed from its emitted package_info.cs, and
// usedDerivedTypeAliases the subset an emitted reference actually resolved through. A derived
// entry's `global using` is emitted into this package's package_info.cs only when it was USED: the
// parsed set describes an assembly that provably declares every alias target, while a derived set
// describes what go2cs WOULD emit for that dependency's Go source — true of any real conversion,
// but not of a hand-written proxy such as the baseline `core/time` stub (which declares no
// `ΔLocation`/`ΔMonth`/`ΔWeekday` at all, so an unused alias to one is CS0426 in every behavioral
// test that imports time). Gating on use keeps the derived metadata's blast radius to the code that
// actually references the renamed member — where the rename is required for the reference to bind
// at all. Reset per package.
var derivedTypeAliases HashSet[string]

var usedDerivedTypeAliases HashSet[string]

var parsedPackageInfoFiles HashSet[string]

var interfaceImplementations map[string]HashSet[string]

var promotedInterfaceImplementations map[string]HashSet[string]

// constraintProxies collects the SELF-REFERENTIAL constraint proxies this package needs — a
// generic type instantiated with a pointer type whose type parameter carries a self-referential
// generic method-set interface constraint (nistCurve's `Point nistPoint[Point]` at `*P224Point`).
// The box ж<P224Point> can't nominally implement nistPoint<…>, so a proxy stands in as the type
// argument. Keyed "elementFullName|interfaceFullName" → {elementFullName, interfaceFullName}; each
// becomes a `[assembly: GoImplement<element, iface<element>>(ConstraintProxy = true)]` record that
// drives ImplementGenerator's EmitConstraintProxy. See constraintProxyArg.
var constraintProxies map[string][2]string

// nominalProductionConstraints collects the (production type argument, test-declared constraint
// interface) pairs a WHITE-BOX REFERENCE test conversion has emitted a nominal C# `where` clause
// for. C# enforces `where P : I` on the type ARGUMENT itself, and go2cs-gen can only make a type
// satisfy an interface nominally by declaring a partial that adds it to the base list — which a
// production type closed inside a referenced assembly forbids, so the generator emits an ADAPTER
// class instead and the instantiation is CS0315. The relocatable-records claim behind the reference
// model therefore has exactly this exception: an adapter serves interface BOXING, never a nominal
// constraint. Entries are `typeArg|interface` for diagnostics; the model-selection gate reads only
// whether the set is non-empty. See recordNominalProductionConstraint.
var nominalProductionConstraints HashSet[string]

var interfaceInheritances map[string]HashSet[string]

// adapterClassImplementations marks recorded "iface|impl" GoImplement pairs whose implementation
// is a DISTINCT value-form adapter CLASS (`<src>ᴠ<iface>` — an interface-sourced conversion or a
// foreign-struct value conversion) rather than an interface entry folded into the implementing
// type's own partial-struct base list. Cast sites reference the adapter for the EXACT interface
// they target, so the interface-inheritance prune must not drop these pairs (mirrors the ж<T>
// pointer-form exemption there): pruning GoImplement<net.Conn, io.Writer> under
// GoImplement<net.Conn, io.ReadWriteCloser> leaves every `new net_ConnᴠWriter(…)` use site
// referencing a class the generator never emits (net/http, CS0246 ×17). Guarded by packageLock.
var adapterClassImplementations HashSet[string]

var implicitConversions map[string]HashSet[string]

var invertedImplicitConversions map[string]HashSet[string]

var indirectImplicitConversions map[string]HashSet[string]

// conversionPackageUsings maps a cross-package import alias (e.g. "abi") to its C# namespace (e.g.
// "@internal.abi_package") for every package referenced by a recorded implicit conversion. The
// `[assembly: GoImplicitConv<abi.Type, ж<abi.Type>>]` lines in package_info.cs use these aliases, but
// that file has no file-local `using abi = …`, so the aliases are emitted there as `global using`
// directives. Keyed by alias; guarded by packageLock. See recordConversionPackageUsing.
var conversionPackageUsings map[string]string

var numericConversions map[string]map[string]string

var indirectNumericConversions map[string]map[string]string

var nameCollisions map[string]bool

// testMethodRenames holds the `-tests` TEST-file declarators that must emit Δ-renamed to keep
// production symbol names IMMUTABLE in a test-variant emission (blockers B2/B9, and the
// receiver-vs-first-parameter collision): the variant universe mixes production files with
// `_test.go` files, but only the test files are emitted — the production .cs on disk keep their
// production-only-universe names, so a collision introduced by a test file resolves by renaming
// the TEST-side declarator (and every reference site, via convIdent's isMethod arm for a method
// name and its trailing identifier path for a free function), never the production element. Object-keyed so the same-named
// production/dot-imported symbols keep their plain emission, and SESSION-scoped (initialized once
// per -tests conversion in processTestConversion, NOT reset per variant): both variants share one
// go/packages load, so the external variant's references to an internal-variant method (the
// export_test pattern) resolve to the very object registered during the internal pass. Nil outside
// -tests conversions. See performNameCollisionAnalysis / registerTestMethodRenames.
var testMethodRenames map[types.Object]bool

// testTypeRenames holds package-level TYPE declarations that were Δ-renamed by an ordinary
// same-variant collision (getCollisionAvoidanceIdentifier via visitTypeSpec) — the "expose a field
// via a method sharing the type's name" idiom (export_test.go's `type PallocBits pallocBits` next
// to `func (*PallocData) PallocBits() *PallocBits`), which Δ-prefixes the TYPE and leaves the
// METHOD bare (getCollisionAvoidanceIdentifier's own doc comment). That rename is captured
// correctly within the variant that declares it, but nameCollisions is reset per variant
// (resetPackageState) while a white-box EXTERNAL-variant reference to the same declaration
// (whiteboxBridgeNamedType) renders through its OWN, unrelated collision set — which has no entry
// for a name the external variant's files never declare — so the bridge reference emitted the bare
// name against a Δ-renamed declaration. Object-keyed and SESSION-scoped for the identical reason
// testMethodRenames is: both variants share one go/packages load, so the external variant's
// reference resolves by object identity to the very declaration registered during the internal
// pass. Nil outside -tests conversions.
//
// It carries VARS and CONSTS as well as types — same mechanism, same cross-variant blindness (the
// export_test.go `var Lock = lock` beside `RWMutex.Lock`, read back by whiteboxBridgeMember). Each
// kind registers at ITS OWN rename site, and they are not the same file: a TYPE at visitTypeSpec, a
// CONST at visitValueSpec's CONST arm, and a package-level VAR at performGlobalVariableAnalysis —
// which renames the var declarator BEFORE any visitor runs, so by visitValueSpec the name is
// already `ΔLock` and nameCollisions no longer answers for it. Guarded by
// TestTestVariantBridgeFollowsRenamedPackageVar.
var testTypeRenames map[types.Object]bool

// packageBuiltinShadows holds Go built-in names (`clear`, `len`, …) that the current package ALSO
// declares as a method or function. In Go a method `func (x T) clear()` and the universe `clear`
// built-in coexist (the method is only reached as `x.clear()`), but in C# the method is emitted as a
// `clear(this ref T)` extension on the package's static class, which SHADOWS the using-static
// `go.builtin.clear` for an unqualified free `clear(s)` call (C# member lookup stops at the class).
// A built-in call whose name is in this set is therefore emitted qualified as `builtin.<name>(…)`.
var packageBuiltinShadows map[string]bool

// packageFuncMethodNames holds every method/function name declared in the current package —
// a name matching an imported package's using-alias shadows it inside the package class
// (compress/flate's byLiteral.sort vs `import "sort"`, CS0119); the package-ident emission
// qualifies through the _package class instead.
var packageFuncMethodNames map[string]bool

// siblingTestFuncMethodNames holds the build-selected package-level declarator names contributed by
// the IN-PACKAGE `_test.go` half. Those declarations compile into the very same C# package class as
// the production sources (TestingInfrastructureRequirements §2.1/§4.2), so one of them shadows a
// PRODUCTION file's import using-alias exactly as a production declarator would — but the
// production package loaded by go/packages does not include test files and cannot see it:
// hash/maphash's smhasher_test.go declares `func (k *bytesKey) bits() int` while maphash_purego.go
// imports "math/bits", so `bits.Mul64(a, b)` would bind the method group (CS0119, plus CS8130 on
// both deconstructed results). Folded into packageFuncMethodNames by performNameCollisionAnalysis,
// which qualifies such a package ident through its _package class — a REFERENCE spelling only,
// never a symbol rename (production names stay pinned; see testMethodRenames). Populated
// independently for every production package immediately before its collision analysis, in
// ordinary and -tests conversion alike, so production source spelling is mode-stable.
var siblingTestFuncMethodNames []string

var hasSiblingInternalTestFiles bool

// siblingTestAddressedGlobalNames holds the identifier names the build-selected IN-PACKAGE
// `_test.go` half takes the address of. A Go pointer to a package-level var must alias the var's
// real storage, which in C# requires the global to be backed by a heap box (see
// packageAddressedGlobals) — and the box is declared by the PRODUCTION emission, which go/packages
// cannot see the need for because it excludes `_test.go`: path/filepath's export_test.go declares
// `var LstatP = &lstat` over path.go's `var lstat = os.Lstat`, and the test variant's `Ꮡlstat`
// named a box the production class never declared (CS0103). Folded into packageAddressedGlobals by
// collectAddressedGlobals, which resolves each name against the real package scope and drops any
// that is not a package-level var. Populated for every production package immediately before its
// analysis, in ordinary and -tests conversion alike, so production storage shape is mode-stable —
// exactly as siblingTestFuncMethodNames pins reference spelling.
var siblingTestAddressedGlobalNames []string

// packageTestAliasShadows is the subset of packageFuncMethodNames contributed ONLY by the
// same-package test half. It does not affect qualification itself; statement emission uses it to
// explain an otherwise surprising fully-qualified production reference when the reader is not
// looking at the package's tests.
var packageTestAliasShadows map[string]bool

// goBuiltinNames is the set of Go universe built-in function names that golib implements as static
// methods on `go.builtin`. Used to detect a package method/function that shadows one of them.
var goBuiltinNames = map[string]bool{
	"append": true, "cap": true, "clear": true, "close": true, "complex": true,
	"copy": true, "delete": true, "imag": true, "len": true, "make": true,
	"max": true, "min": true, "new": true, "panic": true, "print": true,
	"println": true, "real": true, "recover": true,
}

var globalTempVarCount map[string]int

// packageHoistedConstOrdinals counts, per Go const name, the hoisted big-constant fields this
// package's conversion has claimed so far (visitValueSpec's writeUntypedConst — a function-local
// int-kind GoBigConst hoists its BigInteger.Parse to a `static readonly` field named
// `<name>ᶜ[ordinal]`). Package-scoped because every hoisted field lands in the ONE
// `<pkg>_package` partial class: two functions (or two files) each declaring `const mask = <big>`
// must claim distinct field names or the class declares the field twice (CS0102). Deterministic
// because files convert sequentially in sorted order. Reset per package/variant by
// resetPackageState; claimed under packageLock.
var packageHoistedConstOrdinals map[string]int

// productionHoistedConstOrdinals pins the hoisted big-constant ordinals the PRODUCTION conversion
// of this package claimed, for the `-tests` INTERNAL variant only — the same production-pinned
// seeding productionLiftedTypeNames applies to lifted type names, and for the same reason: that
// variant's test files emit into the production `<pkg>_package` class whose on-disk `.cs` are not
// regenerated, so a test-side hoist reusing a production field name is CS0102. Nil for a
// production conversion and for the EXTERNAL variant (its `<pkg>_test_package` class is a separate
// scope). Installed by convertTestVariant from the seed its caller captured before the first
// variant's resetPackageState.
var productionHoistedConstOrdinals map[string]int

var initFuncCounter int

var usesUnsafeCode bool

// packageImportForces holds the import paths this package (this ASSEMBLY, i.e. the current
// conversion pass) has already emitted a blank-import force hook for. Go initializes an imported
// package exactly once per program regardless of how many files import it, and a .NET module
// constructor likewise runs once per assembly — so a package blank-imported from several files
// needs exactly ONE hook, and it belongs to the first file that names it. Reset per package/variant
// by resetPackageState; written under packageLock.
var packageImportForces HashSet[string]

// packageImportInits holds each force hook's FORCING TARGET — the C# package class the emitted
// `typeof(...)` names, already `global::`-qualified where the leading segment would be occluded —
// keyed by the import path it forces. The visitor decides the target because only it can see the
// scope; the metadata writer composes the block, because only it knows the indentation.
// The hooks used to be spliced into the importing FILE's class body; since the relocation they are
// collected here and written once into the emission unit's metadata file (package_info.cs, or
// package_test_info.cs for a -tests variant), which is what takes a machinery block out of every one
// of the 684 production files that carries one. Keyed rather than ordered because the emission sorts
// by import path: hook order is not correctness-bearing — each hook forces ONE assembly, whose own
// module constructor runs that assembly's hooks first, so Go's transitive ordering is reproduced by
// the forcing itself rather than by the order of the calls — and a map plus a sort is deterministic
// where visit order is merely reproducible.
//
// Drained by writePackageInfoFile, which is called once per emitted metadata file (production, then
// each -tests variant), so a hook lands in the file belonging to the unit that emitted it. Reset per
// package/variant by resetPackageState; written under packageLock.
var packageImportInits map[string]string

// packageDoc holds the current package's Go doc comment as godoc-markup TEXT, for the NuGet README.
// It is rendered to Markdown at emission time by renderPackageDoc, which is where the source
// directory and options a doc link needs to resolve to a fully-qualified URL are in scope.
var packageDoc string

// packageSourceDir holds the directory the current package's Go sources were loaded from — the
// GOROOT package directory under -stdlib. The README's validation badge needs it to answer the one
// question the converted output cannot: does this package's Go source define `Test` functions at
// all? That is what separates an honest "not yet validated" badge from a "none to validate" one.
var packageSourceDir string

var packageLock = sync.Mutex{}

// packageDynamicTypeNames maps a lifted (anonymous struct/interface) type's
// structural signature (`types.Type.String()`) to its generated C# type name,
// shared across all files in a package. Per-file visitors lift these types into
// their own `liftedTypeMap`, but cross-file references (e.g. taking the address
// of a field of a package-global anonymous-struct var declared in another file)
// can't see that per-file map. This package-level registry, resolved after the
// concurrent file-visit barrier, bridges that gap. Guarded by packageLock.
var packageDynamicTypeNames map[string]string

// packageLiftedTypeNames holds every lifted type name (an anonymous struct/interface, or a
// function-local declaration hoisted out of its body) CLAIMED so far by this package's conversion.
// Every lifted type is emitted as a NESTED type of the single `<pkg>_package` partial class, so the
// uniquing scope must be the PACKAGE, not the file: two files that each lift `struct{…}` under the
// fallback name `type` both emitted `Δtype`, and the class then declares it twice — CS0579 on the
// duplicated `[GoType]` attribute plus CS0111/CS0557 on every member go2cs-gen generates for the
// doubled definition (encoding/gob's type.cs vs encoder_test.cs, 32 errors). Reset per
// package/variant by resetPackageState; written under packageLock.
//
// A `[module: GoManualConversion]` file does NOT claim here — its emission is redirected to a
// non-compiled `.cs.auto` review sibling, so a claim would push a REAL file's type name to a higher
// ordinal for a declaration that is never compiled. Those visitors keep the per-file set alone.
var packageLiftedTypeNames HashSet[string]

// productionLiftedTypeNames pins the lifted type names the PRODUCTION conversion of this package
// already claimed, for the `-tests` INTERNAL variant only: that variant emits its `_test.go` files
// into the SAME `<pkg>_package` class while the production `.cs` on disk are NOT regenerated, so
// those names are immutable and a test-side lift must step around them. Same production-pinned rule
// testMethodRenames applies to declarators and the Tier-C hoist seed applies to literal fields. Nil
// for a production conversion and for the EXTERNAL variant, whose `<pkg>_test_package` class is a
// separate scope that may reuse the names freely. Installed by convertTestVariant from the seed its
// caller captured before the first variant's resetPackageState.
var productionLiftedTypeNames HashSet[string]

// productionAliasLiftedTypes maps an anonymous struct/interface TYPE to the package-scope ALIAS
// name the PRODUCTION conversion lifted it under (`type CorpusEntry = struct{…}` → "CorpusEntry").
// Populated for a `-tests` conversion only, by seedProductionAliasLifts, and only for aliases the
// production package_info.cs actually PUBLISHES — the map and the `global using` that makes the
// name resolvable in the test compilation are seeded together, so a name is never rendered without
// its alias. Keyed by go/types identity, which is exact here: production and test variants are
// type-checked in ONE `go/packages` load, so the alias's right-hand side and every test-side
// reference to it are the same *types.Struct. Nil for a production conversion. Guarded by
// packageLock; reset per package/variant by resetPackageState.
var productionAliasLiftedTypes map[types.Type]string

// productionDynamicTypeNames pins the PRODUCTION conversion's OWN packageDynamicTypeNames (a
// purely-anonymous struct/interface's structural signature -> its converter-synthesized lift name,
// e.g. `interface{F()}` -> "ifaceHash_i") for the `-tests` INTERNAL variant, the sibling of
// productionAliasLiftedTypes for the case that registry does not cover: a type with no Go-level
// `type X = ...` alias at all, lifted only because deferredDynamicTypeName/dynamicStructTypeName
// (dynamicTypeOperations.go) minted it a name during production's own conversion. Without this, the
// SAME anonymous type reached from an internal `_test.go` file (export_test.go's whole reason to
// exist) finds no per-file claim and no package-registry entry — packageDynamicTypeNames was reset
// to a fresh map for the test variant's own pass — and falls to a deferred marker that the
// post-barrier resolution ALSO cannot resolve, since production's registrations live only in the
// map resetPackageState already replaced. The result is the raw Go type signature emitted as C#
// (unbuildable) and, on the `-tests` path, the W2b gate failing the whole conversion.
//
// Keyed by structural SIGNATURE, not go/types identity: unlike productionAliasLiftedTypes' exact
// *types.Type keys (safe because production and the test variant share one type-check pass), the
// signature string is what dynamicTypeMarker/lookupDynamicTypeName already key on everywhere else,
// so this registry composes with them without a second identity question. Nil for a production
// conversion and for the EXTERNAL variant (its own separate `<pkg>_test_package` scope has no
// production entries to inherit — an anonymous type it needs was either already lifted under the
// production import's own qualified name or must lift fresh in its own scope). Installed by
// convertTestVariant from the seed its caller captured before the first variant's
// resetPackageState; guarded by packageLock; reset per package/variant by resetPackageState.
var productionDynamicTypeNames map[string]string

// testAmbiguousLocalTypeNames holds the SIMPLE type names declared by BOTH `-tests` variant
// classes — the package under test (`<pkg>_package`, production files plus its internal `_test.go`
// files) and the external suite (`<pkg>_test_package`). The merged test metadata files carry a
// `using static` for BOTH classes, so at the file scope where the `[assembly: GoImplement<…>]`
// attributes sit a bare reference to such a name is CS0104-ambiguous (encoding/gob declares Point
// and Vector in codec_test.go AND in example_encdec_test.go / example_interface_test.go). Those
// references are emitted class-qualified instead — see qualifyAmbiguousTestTypeRefs. Session-scoped
// to one `-tests` conversion (computed in convertTestVariants from the two loaded variants, before
// either is converted) and, like testMethodRenames, deliberately NOT cleared by resetPackageState;
// nil for every other conversion, so no other emission changes.
var testAmbiguousLocalTypeNames HashSet[string]

// whiteboxInternalTestObjects is the object-identity set contributed by the internal `_test.go`
// half of the one go/packages load. External-variant selector and type rendering consult it when
// export data carries no usable declaration position.
var whiteboxInternalTestObjects map[types.Object]bool

// whiteboxBridgeTypeNames holds the simple TYPE names the white-box bridge declares — the
// declared-name set splitWhiteboxVariantRecords partitions records by. Session-scoped to one
// `-tests` conversion and consulted during emission too, so a generated adapter's reference names
// the anchor class its record will actually land in (whiteboxBridgeDeclaredType); the LIFTED half
// of the set is only claimed as the bridge converts, so that predicate folds in the live lift
// claims while the bridge is the variant under conversion.
var whiteboxBridgeTypeNames HashSet[string]

// whiteboxBridgeDeclaredNames holds the GO names the white-box bridge class itself declares — its
// internal `_test.go` package-level funcs/vars/consts/types plus its methods, which emit as static
// extension members of the same class. The bridge binds production through
// `using static <pkg>_package`, and C# member lookup stops at the first enclosing type that has
// the name: a bridge member HIDES every same-named production member, `using static` included.
// container/heap's `[GoRecv] Pop(this ref myHeap)` hid `heap_package.Pop(Interface)` outright, so
// the test's own `Pop(h)` bound the extension and failed CS1620 (ref argument). A production
// reference whose name is in this set is emitted production-class-qualified instead — the same
// remedy packageBuiltinShadows applies to a shadowed `using static go.builtin`. Raw Go names,
// because both sides pass through the same sanitizers; session-scoped like testMethodRenames and
// nil for every other conversion.
var whiteboxBridgeDeclaredNames HashSet[string]

// metadataAnchorLocalTypes reports whether the anchored metadata file being written treats its
// anchor class as the LOCAL type scope — true for the reference test models (the production class
// is a referenced assembly there), false for the recompile model's anchored writes (the production
// class is compiled into the same assembly and keeps the historical local qualification).
var metadataAnchorLocalTypes bool

// metadataAnchorClassPrefix is the fully-qualified class the metadata file currently being WRITTEN
// anchors to — `go.encoding.gob_package` for package_test_info.cs, `go.encoding.gob_test_package`
// for package_info_external_test.cs. It is a property of the FILE, not of the variant doing the
// writing: the external variant merges into package_test_info.cs too, and a bare local reference
// there still means the production class (the same invariant the B4/B5 record split is built on).
// Empty outside those writes, where the current package's own class is the anchor.
var metadataAnchorClassPrefix string

// packageManualTypeNames records the CONVERTED names of this package's manually-converted
// types (see manualTypeOperations.go), collected as visitTypeSpec skips their declarations.
// Consumed by the GoImplicitConv attribute emission, which must not reference the skipped
// auto forms (the *_impl.cs declares any conversion operators the call sites need). Guarded
// by packageLock.
var packageManualTypeNames map[string]bool
