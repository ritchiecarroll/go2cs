// visitImportSpec.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"fmt"
	"go/ast"
	"go/build"
	"go/types"
	"log"
	"os"
	"path/filepath"
	"regexp"
	"strings"
)

// packageQualifiedNameRegex matches a dotted qualified identifier. Segments may contain Unicode
// letters/digits the converter uses in generated names (e.g. Δ, ꓸ, ᴛ) and may carry the C#
// keyword escape (`@internal`) — the `@` must be INSIDE the match, or the root-qualifier below
// splices the prefix between the escape and its segment (`@internal.bisect_package.Writer`
// became `@go.internal.…` — a parse error in internal/godebug's GoImplement attribute).
var packageQualifiedNameRegex = regexp.MustCompile(`@?[\p{L}_][\p{L}\p{N}_]*(?:\.@?[\p{L}_][\p{L}\p{N}_]*)*`)

// rootQualifyScanRegex is packageQualifiedNameRegex with the C# root escape allowed IN FRONT of the
// match, used only by rootQualifySubNamespaceTypeRefs. The base pattern cannot match `::`, so an
// escaped reference reaches a plain scan as its bare inner path and is indistinguishable from an
// unrooted one — which is exactly how a `global::go.…` reference got rooted a second time. Carrying
// the prefix into the match is what lets that arm recognize and skip it; every other consumer of the
// base pattern is unaffected.
var rootQualifyScanRegex = regexp.MustCompile(`(?:global::)?@?[\p{L}_][\p{L}\p{N}_]*(?:\.@?[\p{L}_][\p{L}\p{N}_]*)*`)

// systemCollidingTypeNames are top-level types of C# namespace `System` whose names a Go package can
// legitimately reuse for one of its own exported types (e.g. `internal/profile.ValueType`,
// `go/ast.Object`, `bytes.Buffer`). Assembly-level GoImplement/GoImplicitConv attributes are emitted
// at file scope where BOTH `using System;` and `using static go.<pkg>_package;` are in scope, so a
// BARE reference to such a local type is ambiguous between the System type and the package type
// (CS0104). qualifySystemCollidingLocalTypeRefs roots those bare references at the package class so
// they resolve unambiguously to the local type. Only names in this curated set are touched, so
// attributes whose type names never collide with System (every behavioral-test case) emit
// byte-identically (no golden churn).
var systemCollidingTypeNames = NewHashSet([]string{
	"Action", "Activator", "Array", "Attribute", "Boolean", "Buffer", "Byte", "Char", "Comparison",
	"Console", "Convert", "DateTime", "Decimal", "Delegate", "Double", "Enum", "Environment", "Exception",
	"Func", "Guid", "Half", "Index", "Int128", "Lazy", "Math", "Memory", "Nullable", "Object", "Predicate",
	"Progress", "Random", "Range", "SByte", "Single", "Span", "String", "TimeSpan", "TimeZone", "TimeZoneInfo",
	"Tuple", "Type", "UInt128", "Uri", "ValueType", "Version",
})

// rootQualifySubNamespaceTypeRefs prefixes the root namespace ("go.") onto package-qualified type
// references that live in a sub-namespace (e.g. image.color_package.ΔRGBA -> go.image.color_package.ΔRGBA).
// Assembly-level GoImplement attributes are emitted before the file's namespace with only `using go;`
// in scope; that directive imports the TYPES of namespace `go` (so a top-level `io_package.Writer`
// resolves unqualified) but NOT its nested namespaces, so a multi-segment package class such as
// `image.color_package` cannot be found and yields CS0246. References whose `_package` class is the
// first segment (single-segment packages such as io/fmt/sort) and references already rooted at "go."
// are returned unchanged, so single-segment GoImplements — every behavioral-test case — emit
// byte-identically (no golden churn).
func rootQualifySubNamespaceTypeRefs(name string) string {
	return rootQualifyScanRegex.ReplaceAllStringFunc(name, func(match string) string {
		// A `global::`-ESCAPED reference is already absolutely qualified — that is the entire point
		// of the escape — so nothing here may re-root it. The scan regex carries the prefix only so
		// this arm can SEE it: packageQualifiedNameRegex cannot match `::`, so an escaped reference
		// used to arrive here as its bare inner path, indistinguishable from an unrooted one.
		//
		// runtime's `-tests` metadata is the measured case. The white-box bridge class arrives as
		// `global::go.runtime_internal_test_package.TestingT`; the inner `go.runtime_internal_test_package`
		// matched, tested as `go.`-prefixed, and asked isStrippedGoPathPackageRef — which walks to its
		// LAST line and answers `packageChildNamespaces["go.go"]`. That is TRUE for runtime's test
		// closure, because it imports `go/token`, so a class that is not a `go/*` package at all was
		// re-rooted to `global::go.go.runtime_internal_test_package.TestingT` and CS0234'd against the
		// real `go.go` namespace those `go/*` imports create. The same file spells the same class
		// correctly one line above, in a `global using static` the escape reaches unmangled.
		//
		// Skipping the escape is the general statement of that, and it is safe in the other direction
		// too: an escaped reference that genuinely names a `go/*` package already carries its full
		// `global::go.go.…` path, so there is nothing left to add.
		if strings.HasPrefix(match, rootEscapePrefix) {
			return match
		}

		if strings.HasPrefix(match, RootNamespace+".") {
			// A go/*-package ref whose root the strip removed (`go.ast_package` for go/ast, whose
			// real namespace is `go.go`) is re-rooted to `go.go.ast_package`; the GoImplement/
			// GoImplicitConv attributes emit at assembly scope, so a bare root prefix resolves (no
			// global:: needed, unlike the in-namespace using aliases). A genuinely-rooted ref is
			// left unchanged. See isStrippedGoPathPackageRef.
			if isStrippedGoPathPackageRef(match) {
				return RootNamespace + "." + match
			}

			return match
		}

		// A leading segment that is an IMPORT ALIAS rather than a package CLASS: the converted
		// sources reach `unsafe.Pointer` through a file-local `using @unsafe = unsafe_package;`, so
		// the rendered reference is `@unsafe.Pointer` — no `_package` segment for the scan below to
		// key on, and no alias in scope at assembly-attribute level, where the record actually
		// lands. It emitted verbatim and CS0246'd on the namespace `unsafe`.
		//
		// Same FILE-LOCAL-ALIAS class the Δ-shadow arm below already handles (`Δio.fs_package` →
		// `go.io.fs_package`), reached through the other spelling: Δ marks an alias renamed around a
		// child-namespace collision, `@` marks one escaped around a C# keyword. Both are devices of
		// the file that declared them and neither survives into an assembly attribute, so both owe
		// the REAL class path.
		//
		// Gated on the package class being one this conversion actually imported, so the rewrite can
		// only ever name a class that exists — an escaped segment that is not an imported package
		// falls through untouched.
		if first, rest, hasRest := strings.Cut(match, "."); hasRest && strings.HasPrefix(first, "@") &&
			!strings.Contains(match, PackageSuffix) {
			if class := strings.TrimPrefix(first, "@") + PackageSuffix; packageQualifiedNamespaces[RootNamespace+"."+class] {
				return RootNamespace + "." + class + "." + rest
			}
		}

		for i, seg := range strings.Split(match, ".") {
			if strings.HasSuffix(seg, PackageSuffix) {
				// Only a sub-namespace package class (not the leading segment) needs rooting.
				if i > 0 {
					// A Δ-renamed import alias (io -> Δio, which collides with the `io` CHILD
					// namespace) is a FILE-LOCAL device; the rooted `go.` path needs the REAL
					// namespace segment — `go.io.fs_package.DirEntry`, not `go.Δio.fs_package…`
					// (CS0234, embed's GoImplement<@file, io/fs.DirEntry> lines). Strip the
					// collision marker from the leading segment.
					if raw, wasShadow := strings.CutPrefix(match, ShadowVarMarker); wasShadow {
						return RootNamespace + "." + raw
					}

					return RootNamespace + "." + match
				}

				break
			}
		}

		return match
	})
}

// stripLocalTypeQualifier rewrites a type reference that names a type compiled into THIS assembly
// through a fully-qualified package class (`go.math.rand.rand_package.PCG`) to the bare local form
// (`PCG`) the owning package's own emission always uses — the assembly-attribute file carries
// `using static <ns>.<pkg>_package;`, so both spellings resolve to the SAME type and the bare one
// is canonical.
//
// The GoImplement/GoImplicitConv record sets are keyed by RENDERED name, so two spellings of one
// resolved pair are two records: under -tests the package-under-test's records arrive BOTH short
// (seeded verbatim from the production package_info.cs by the test-metadata anchor) and fully
// qualified (re-discovered while converting the _test.go variants, where the production package is
// reached through its import path). go2cs-gen then generated the adapter TWICE — GetUniqueHintName
// silently uniquified the second FILE name, so the duplicate TYPE reached the compiler
// (math/rand/v2: CS0102 + CS0111 x5 + CS8646 on rand_package.PCGжSource). Normalizing here makes
// the two records textually identical, so the emitting HashSet collapses them to one. math/rand
// escaped only by luck — its one self-qualified record targets a different interface than any
// short record.
//
// The prefixes considered are the CURRENT package's own class plus testLocalTypePrefixes (the
// package under test, populated only by a -tests conversion). A genuinely FOREIGN reference is
// never stripped: matching a foreign package's record would suppress the LOCAL record its consumer
// needs, which is the mistake documented on implementRecordKey.
func stripLocalTypeQualifier(name string, localTypePrefix string) string {
	prefixes := make([]string, 0, 1+len(testLocalTypePrefixes))

	if localTypePrefix != "" {
		prefixes = append(prefixes, localTypePrefix)
	}

	// Under -tests the package under test compiles into this same assembly, so its class prefix is
	// local too even though the external variant reached it through an import path.
	prefixes = append(prefixes, testLocalTypePrefixes...)

	matched := false

	for _, prefix := range prefixes {
		if strings.Contains(name, prefix+".") {
			matched = true
			break
		}
	}

	if !matched {
		return name
	}

	return packageQualifiedNameRegex.ReplaceAllStringFunc(name, func(match string) string {
		for _, prefix := range prefixes {
			trimmed, ok := strings.CutPrefix(match, prefix+".")

			// Only a DIRECT member of the local package class is the bare local type; a longer
			// dotted tail is a nested reference this normalization has no business rewriting.
			if ok && !strings.Contains(trimmed, ".") {
				return trimmed
			}
		}

		return match
	})
}

// qualifySystemCollidingLocalTypeRefs roots any BARE (single-segment) type reference whose name is a
// System type (systemCollidingTypeNames) at packagePrefix (e.g. `go.@internal.profile_package`), so it
// resolves to the LOCAL package type rather than being ambiguous with the `using System;`-imported type
// (CS0104) at the file scope where GoImplement/GoImplicitConv attributes are emitted. Dotted references
// (foreign `pkg_package.Type`, already-rooted `go.…`) are left untouched: a bare System-colliding name
// in these attributes can only be a local package type (foreign types are always package-qualified).
func qualifySystemCollidingLocalTypeRefs(name string, packagePrefix string) string {
	return qualifyBareTypeReferences(name, systemCollidingTypeNames, packagePrefix)
}

// qualifyBareTypeReferences roots every BARE (dotless) type reference inside name that appears in
// ambiguousNames at qualifier, turning `Buffer` into `io_test_package.Buffer`.
//
// This is the shared body of the two passes above. Both solve the same C# problem from different
// directions: a bare name at the file scope where GoImplement/GoImplicitConv attributes are emitted
// can be ambiguous (CS0104) between the local package type and something a `using` brought in, and
// the cure is the same either way — say which one you mean. They differ only in WHICH names are
// ambiguous and WHAT to qualify them with, so those are the parameters.
//
// A reference that already contains a dot is left alone: it has said where it comes from, and
// re-qualifying it would produce a name that resolves nowhere.
func qualifyBareTypeReferences(name string, ambiguousNames HashSet[string], qualifier string) string {
	return packageQualifiedNameRegex.ReplaceAllStringFunc(name, func(match string) string {
		if strings.Contains(match, ".") {
			return match
		}

		if ambiguousNames.Contains(match) {
			return qualifier + "." + match
		}

		return match
	})
}

// qualifyAmbiguousTestTypeRefs roots any BARE type reference whose simple name is declared by BOTH
// `-tests` variant classes (testAmbiguousLocalTypeNames) at anchorPrefix — the class the metadata
// file being written anchors to. The merged test metadata carries a `using static` for the package
// under test AND for the external `<pkg>_test` class, so at the file scope where the
// GoImplement/GoImplicitConv attributes sit the bare name is ambiguous between them (CS0104 — gob's
// Point/Vector, declared in codec_test.go and again in example_encdec_test.go /
// example_interface_test.go). Making the reference explicit also states the invariant the B4/B5
// record split already relies on: a bare local name means the file's ANCHOR class.
//
// A no-op outside a `-tests` conversion (the name set is nil), so no other emission changes.
func qualifyAmbiguousTestTypeRefs(name string, anchorPrefix string) string {
	// Bail out before scanning when there is nothing this pass could do. The empty-set case is
	// merely the fast path for every non-`-tests` conversion, but the empty-prefix case is
	// REQUIRED: qualifying with "" would emit a leading dot (".Point") that resolves nowhere.
	if len(testAmbiguousLocalTypeNames) == 0 || anchorPrefix == "" {
		return name
	}

	return qualifyBareTypeReferences(name, testAmbiguousLocalTypeNames, anchorPrefix)
}

func (v *Visitor) visitImportSpec(importSpec *ast.ImportSpec, doc *ast.CommentGroup) {
	v.currentImportPath = strings.Trim(importSpec.Path.Value, "\"")

	if !v.options.parseCgoTargets && v.currentImportPath == "C" {
		log.Fatalf("cgo target parsing is not supported: file \"%s\"", v.fset.Position(importSpec.Pos()).Filename)
	}

	// Resolve a GOROOT-vendored import to its on-disk path (see resolveGorootVendoredPath) so
	// the import queue and the imported-alias loader look at the real output location. Gated on
	// the importing FILE living under GOROOT so a user module's own golang.org/x dependency is
	// untouched.
	if goroot := filepath.Clean(build.Default.GOROOT); strings.HasPrefix(filepath.Clean(v.fset.Position(importSpec.Pos()).Filename), goroot+string(filepath.Separator)) {
		v.currentImportPath = resolveGorootVendoredPath(v.currentImportPath)
	}

	v.importQueue.Add(v.currentImportPath)

	// -tests: an EXTERNAL package test (package <name>_test) imports the package under test, but
	// its converted C# compiles INTO the test assembly that RECOMPILES the production sources
	// (TestingInfrastructureRequirements §2.1/§4.2) — the import must bind to that local partial
	// class, never back at the production project. Skip the imported-alias load (the package's
	// types are local here, and its exported metadata is already seeded into package_test_info.cs)
	// and rebind the emitted using target to the local class. The substitution PRECEDES the alias
	// branches below so every form binds identically — including the dot-import form
	// (`. "unicode/utf8"` → `using static <ns>.<name>_package;`) the first-proof package requires.
	isPackageUnderTest := v.options.testPackagePath != "" && v.currentImportPath == v.options.testPackagePath

	if !isPackageUnderTest {
		v.loadImportedTypeAliases(v.currentImportPath)
	}

	importPath := rootQualifyIfAmbiguous(convertImportPathToNamespace(v.currentImportPath, PackageSuffix))

	if isPackageUnderTest {
		// Composed from packageNamespace directly rather than routed through
		// rootQualifyIfAmbiguous, so force the root shadow qualifier explicitly.
		importPath = globalQualifyRooted(fmt.Sprintf("%s.%s", packageNamespace, getSanitizedImport(v.options.testPackageName+PackageSuffix)))
	}

	// The canonical C# alias for this package — what an unaliased import emits and what getAliasQualifiedTypeName's
	// short-form type references (`pkg.Type`) resolve through. Record the import path when THIS import
	// actually emits that canonical alias (an unaliased import, or one explicitly aliased to the same
	// name), so visitFile does not re-emit (and duplicate) it; a blank/dot/renamed import does not emit
	// it, so a foreign type reference from this file still gets the alias supplied (see collectTypePackages).
	canonicalAlias, _ := packageUsingAlias(v.currentImportPath)

	v.writeDocString(v.packageImports, doc, importSpec.Pos())

	if importSpec.Name != nil {
		alias := importSpec.Name.Name

		if alias == "." {
			v.packageImports.WriteString(fmt.Sprintf("using static %s;", importPath))
		} else if alias == "_" {
			// A BLANK import (`import _ "unsafe"`) is side-effects-only: Go forbids referencing
			// the package through it, so the alias is never legitimately used — but emitting
			// `using _ = <ns>;` HIJACKS C#'s `_` DISCARD for the whole file: any deconstruction
			// discard (`(w, _) = w.ensure(…)`, runtime tracetime.go) then binds the namespace
			// alias instead (CS0118 + CS0029). Record the import as a comment only; the package's
			// exported aliases still load (loadImportedTypeAliases above) and a genuine type
			// reference gets its canonical `using` from visitFile's collectTypePackages machinery.
			v.packageImports.WriteString(fmt.Sprintf("// blank import: %s (side effects only; no using emitted — a `using _` alias hijacks C# discards)", importPath))
		} else {
			if getSanitizedImport(alias) == canonicalAlias {
				v.canonicalAliasImported.Add(v.currentImportPath)
			}

			emittedAlias := getSanitizedImport(importQualifier(alias))
			v.importAliasesEmitted.Add(emittedAlias)
			v.importAliasTargets[emittedAlias] = importPath
			v.importPathAliases[v.currentImportPath] = emittedAlias
			v.packageImports.WriteString(fmt.Sprintf("using %s = %s;", emittedAlias, importPath))
		}
	} else {
		v.canonicalAliasImported.Add(v.currentImportPath)

		// Get package name from the import path, last name after last "."
		importName := importPath
		lastDotIndex := strings.LastIndex(importPath, ".")

		if lastDotIndex != -1 {
			importName = importPath[lastDotIndex+1:]

			namespace := importPath[:lastDotIndex]

			if len(namespace) > 0 && packageNamespace != fmt.Sprintf("%s.%s", RootNamespace, namespace) {
				v.requiredUsings.Add(namespace)
			}
		}

		emittedAlias := getSanitizedImport(importQualifier(strings.TrimSuffix(importName, PackageSuffix)))
		v.importAliasesEmitted.Add(emittedAlias)
		v.importAliasTargets[emittedAlias] = importPath
		v.packageImports.WriteString(fmt.Sprintf("using %s = %s;", emittedAlias, importPath))
	}

	// Go initializes an imported package before the importing package — for EVERY form of import,
	// not only the blank one. The hook that reproduces that is the same in all cases, so it is
	// emitted here, once, rather than in the alias branches (see writeImportInit).
	//
	// The package under test is the one exception, and it is not an exception to Go's rule but to
	// the assembly model: under -tests the external variant's import of the package under test
	// binds to a class compiled into THIS assembly, so there is no separate module constructor to
	// force and `initPackage` would be asked to force the module it is already running inside.
	if !isPackageUnderTest {
		v.writeImportInit(importPath)
	}

	v.writeCommentString(v.packageImports, importSpec.Comment, importSpec.End())
	v.packageImports.WriteString(v.newline)
}

// noInitPseudoPackages are the Go pseudo-packages the language gives no initialization at all:
// `unsafe` and `builtin` are compiler-provided and have no source to run, and `C` is cgo. An import
// of one (`import _ "unsafe"`, required by every `//go:linkname` file — 67 files of the converted
// standard library) exists to satisfy the Go compiler, never to run an `init`, so forcing its
// converted assembly's module constructor would be a guaranteed no-op. See writeImportInit.
var noInitPseudoPackages = NewHashSet([]string{"unsafe", "builtin", "C"})

// writeImportInit records the module-initializer hook that forces an IMPORTED package's `init`
// functions to run before the importing package's own.
//
// Go initializes an imported package before the importing package whether or not anything in it is
// ever referenced. A converted Go `init` becomes `[GoInit]`, which csproj-template.xml aliases to
// .NET's [ModuleInitializer] — the right shape and a WEAKER guarantee: a module constructor runs at
// first access to something in ITS OWN module, so an assembly nothing in the program has touched
// yet has not initialized. golib's `initPackage` closes the gap with
// RuntimeHelpers.RunModuleConstructor, which the runtime guarantees runs a module constructor at
// most once.
//
// The hook was emitted for BLANK imports only until 2026-08-26, on the reading that a blank import
// is the case that references nothing and so exists SOLELY for the initialization (`image/png`'s
// `init` calls `image.RegisterFormat`, a `database/sql` driver's calls `sql.Register`,
// `net/http/pprof`'s installs its handlers). That is true of blank imports and says nothing about
// the others: a NAMED import whose package is referenced only from a function body is equally
// untouched at module-initialization time. `log/slog` is the case that made the difference
// observable — its `init` captures `log/internal.DefaultOutput`, which `log`'s `init` installs, and
// with `log` unforced the capture took nil, so the default handler dereferenced nil and killed the
// package's test host outright. The guard is tests/Behavioral/NamedImportInitOrder.
//
// The TRIGGER is "the imported package initializes something transitively"
// (packageInitializesTransitively), which is observationally the same rule as "force every import"
// — running an empty module constructor is a guaranteed no-op, the same reasoning
// noInitPseudoPackages already applies to `unsafe`/`builtin`/`C` — at a fraction of the emission.
//
// ⚠ It is deliberately NOT a read-set heuristic ("force the imports this package's own `init`
// references"). That narrower rule MISSES the very case that motivated the change: slog's `init`
// reads `log/internal.DefaultOutput`, but the package whose `init` WRITES it is `log`. The
// dependency that must be forced is not the one the init statement names.
//
// The hook is emitted at the TOP of the importing file's class body, so it precedes that file's own
// `init` functions — Go's "imported package first" ordering, to the extent one assembly's module
// initializers are ordered at all (Roslyn emits them in compilation file order, then declaration
// order within a file; ACROSS files of one package that order is not something the converter
// states, so an `init` in file B that depends on an import only file A names is still ordered by
// the compiler rather than by us). Exactly ONE hook is emitted per (assembly, imported package): Go
// initializes a package once per program however many files import it, and a .NET module
// constructor likewise runs once per assembly.
//
// csNamespace is the already-root-qualified C# package class the import resolved to — the same
// target an unaliased import's `using` binds — so the emitted `typeof` needs no alias of its own
// (which is what lets the blank form, whose alias Go forbids using, share this emission).
func (v *Visitor) writeImportInit(csNamespace string) {
	if noInitPseudoPackages.Contains(v.currentImportPath) {
		return
	}

	if !packageInitializesTransitively(v.currentImportPath) {
		return
	}

	forcingTarget := csNamespace

	if v.forcingTargetShadowed(csNamespace) {
		forcingTarget = globalQualifyForcingTarget(csNamespace)
	}

	// The hand-own FENCE that stood here until the relocation, and why it is gone rather than moved.
	// While a hook lived in the importing FILE's class body, a hand-owned file's emission landed in
	// the non-compiled `.cs.auto` review sibling — so it had to SHOW the hook without CLAIMING the
	// package-wide slot, or the slot would be taken by a file that compiles nothing and the import
	// would go unforced. The cost of that fence was the mirror case: a package whose ONLY importer of
	// X is hand-owned had no file left to claim the slot, so X was forced NOWHERE. Censused at the
	// relocation: 14 such hooks across 8 packages, plus two that exist only because somebody wrote
	// them by hand (crypto/internal/boring/bcache, runtime/metrics). With the hook in the metadata
	// file the question does not arise — there is no file to claim, the package's import set is the
	// union of every file's including the hand-owned ones, and every one of those imports is already
	// a ProjectReference, so nothing here can move the project graph.
	packageLock.Lock()
	defer packageLock.Unlock()

	if packageImportForces.Contains(v.currentImportPath) {
		return
	}

	packageImportForces.Add(v.currentImportPath)
	packageImportInits[v.currentImportPath] = forcingTarget
}

// forcingTargetShadowed reports whether the emitted `typeof(<ns>)` of a forcing hook would bind its
// LEADING segment to a nested type of the class the hook is written into, rather than to the
// namespace it names.
//
// This is the one emission that puts a raw namespace-qualified path inside a CLASS BODY. Every
// other cross-package reference the converter emits goes through a file-scoped `using` alias, and a
// using DIRECTIVE is resolved at namespace scope, where class members are not in play — which is
// why `using palette = image.color.palette_package;` resolves while `typeof(image.color.palette_package)`
// five lines below it does NOT. C# resolves the leading identifier of a namespace-or-type-name by
// searching the enclosing type declarations OUTWARD FIRST, so a nested type named `image` occludes
// the `go.image` namespace for the whole class body.
//
// The sighting: Go's own image_test.go declares a test-local `type image interface{…}`, emitted as a
// nested `[GoType] internal partial interface image` of `image_internal_test_package`; the same
// class's forcing hooks for `image/color` and `image/color/palette` then failed to compile with
// CS0426 ("the type name 'color' does not exist in the type 'image_internal_test_package.image'").
// A Go package may legally declare a type whose name matches the first path segment of a package it
// imports, so the PRODUCTION emission has the same exposure — `type sync struct{}` alongside
// `import "sync/atomic"` breaks identically, which is what tests/Behavioral/ImportSegmentTypeShadow
// pins. Nothing in the converted standard library declares such a type today (it compiles, and this
// collision is a hard error), so the guard costs the corpus nothing and covers the class rather than
// the instance.
//
// Only a TYPE occludes: `typeof(a.b)` is a namespace-or-type-name, and that lookup considers types
// and namespaces alone, so a same-named func/var/const is irrelevant. And only a type emitted into
// THIS class occludes — packageScopeClassName answers which class declares a package-level object,
// so a production type does not qualify a hook written into the test-variant class (and vice
// versa), keeping the `-stdlib` and `-tests` emissions of the production files identical.
//
// SINCE THE RELOCATION the hook is written into the emission unit's metadata file, not into the
// file whose import spec produced it, so `emittedClassName` answers for the hook's HOME class only
// where the two coincide. They coincide for the production unit (every production file emits into
// `<pkg>_package`, which is the class package_info.cs declares) and for the external test variant
// (`<pkg>_test_package`, which is the class package_test_info.cs declares). They do NOT coincide for
// the INTERNAL test variant, whose files emit into the production class while its hooks land in the
// test class — so a test-only import shadowed by a TEST-declared type of the same leading segment
// would be under-qualified there. That case has no instance in the corpus (nothing in the converted
// standard library declares such a type at all, which is why this guard covers the class rather than
// an instance), it is guarded by tests/Behavioral/ImportSegmentTypeShadow, and it fails LOUDLY as
// CS0426 rather than silently if it ever arises. Stated rather than papered over with a conservative
// qualification, which would make the production files' `-stdlib` and `-tests` emissions differ —
// the property this check exists to preserve.
func (v *Visitor) forcingTargetShadowed(csNamespace string) bool {
	if strings.HasPrefix(csNamespace, "global::") || v.pkg == nil || v.emittedClassName == "" {
		return false
	}

	leadSegment := csNamespace

	if dot := strings.Index(csNamespace, "."); dot != -1 {
		leadSegment = csNamespace[:dot]
	}

	// The emitted C# name is the sanitized form of the Go identifier (`internal` -> `@internal`), so
	// compare against the sanitized spelling of each package-level type name rather than trying to
	// unmake the escape.
	for _, name := range v.pkg.Scope().Names() {
		object := v.pkg.Scope().Lookup(name)

		if _, isType := object.(*types.TypeName); !isType {
			continue
		}

		if getSanitizedIdentifier(name) != leadSegment {
			continue
		}

		return v.packageScopeClassName(object) == v.emittedClassName
	}

	return false
}

// globalQualifyForcingTarget makes a forcing hook's `typeof` target absolute, so no name declared in
// the enclosing class can occlude it. `global::` is what makes it collision-PROOF rather than merely
// collision-avoiding: it restarts lookup at the global namespace, skipping the enclosing types and
// namespaces entirely.
//
// The rooting itself mirrors rootQualifyIfAmbiguous's own first branch — a go/*-package reference
// whose root the redundant-root strip removed (`go.ast_package`, whose real namespace is `go.go`)
// re-roots to `go.go.ast_package`, while a genuinely rooted path is prefixed as it stands — with the
// ambiguity GATE removed, since the caller has already decided the reference is occluded.
func globalQualifyForcingTarget(ns string) string {
	if strings.HasPrefix(ns, "global::") {
		return ns
	}

	if strings.HasPrefix(ns, RootNamespace+".") {
		if isStrippedGoPathPackageRef(ns) {
			return "global::" + RootNamespace + "." + ns
		}

		return "global::" + ns
	}

	return "global::" + RootNamespace + "." + ns
}

// importInitName builds the generated hook method's name from the Go IMPORT PATH — unique by
// construction within the class (one hook per imported package), stable across runs, and readable
// as what it is: `image/png` -> `initᴛᴛimportꓸimageꓸpng`. The doubled temp marker keeps the name
// clear of the relocated-package-var method space (`init<ᴛ><varname>`, initOrderOperations.go) and
// the `import` word clear of PackageTestInitHookMethod (`initᴛᴛtests`). Path segments are reduced
// to C# identifier characters, so a module path's dots and hyphens
// (`github.com/mattn/go-isatty`) cannot break the identifier.
func importInitName(importPath string) string {
	name := strings.Builder{}
	name.WriteString("init" + TempVarMarker + TempVarMarker + "import")

	for _, segment := range strings.Split(importPath, "/") {
		name.WriteString(TypeAliasDot)

		for _, r := range segment {
			// Anything C# will not accept inside an identifier becomes an underscore, so a module
			// path's dots and hyphens survive as a legal (if lossy) name.
			if isIdentifierRune(r) {
				name.WriteRune(r)
			} else {
				name.WriteRune('_')
			}
		}
	}

	return name.String()
}

// rootQualified prefixes ns with the root namespace, using `global::go.` instead of a bare `go.`
// whenever a `go.go` namespace shadows the root. That happens two ways: the CURRENT package is
// itself a `go/*` stdlib package (go/token, go/ast, go/doc, go/build, … land in
// `namespace go.go.<pkg>`), or ANY `go/*` package appears in the transitive import CLOSURE — its
// referenced assembly makes `go.go` a member of namespace `go`, and C#'s inner-to-outer lookup
// binds the bare leading `go` of a using target to that member from every namespace nested under
// the root, resolving e.g. `go.math` to the nonexistent `go.go.math` (internal/fuzz importing
// go/ast: `using bits = go.math.bits_package;` inside `namespace go.@internal`, CS0234).
// `global::` forces resolution from the global namespace. A package with no `go/*` anywhere in
// its closure keeps the bare `go.` prefix — `global::` there would be needless golden churn.
func rootQualified(ns string) string {
	if rootNamespaceShadowed() {
		return "global::" + RootNamespace + "." + ns
	}

	return RootNamespace + "." + ns
}

// rootNamespaceShadowed reports whether a `go.go` namespace shadows the root in the compilation
// the current file is emitted into — see rootQualified for the two ways that happens.
func rootNamespaceShadowed() bool {
	segs := strings.Split(packageNamespace, ".")

	if len(segs) >= 2 && segs[0] == RootNamespace && segs[1] == RootNamespace {
		return true
	}

	// packageChildNamespaces mirrors the transitive import closure's namespace chains (populated
	// by computeImportAliasRenames' pre-pass); a go/* import path contributes the `go.go` key.
	return packageChildNamespaces[RootNamespace+"."+RootNamespace]
}

// globalQualifyRooted forces `global::` onto an ALREADY-root-qualified namespace path (`go.…`)
// when a `go.go` namespace shadows the root. rootQualified serves the callers that BUILD a rooted
// path from a bare one; the using-directive targets composed directly from packageNamespace — the
// test project's `using static <ns>.<class>;` anchors and the test host's `using go.testing_runtime;`
// — are already rooted and so bypassed the gate entirely, re-binding their leading `go` to `go.go`
// (math/rand/v2 whose regress_test.go imports go/format: CS0234 on the production class anchor and
// on the host's runtime import). Idempotent, and a no-op with no shadow, so unshadowed packages
// (every behavioral-test case) emit byte-identically.
func globalQualifyRooted(rooted string) string {
	if strings.HasPrefix(rooted, "global::") || !rootNamespaceShadowed() {
		return rooted
	}

	return "global::" + rooted
}

// rootQualifyIfAmbiguous prefixes an imported namespace with the root namespace ("go.") when its
// leading segment also appears in the current package's namespace path. Without this, C# relative
// name resolution binds the leading segment to the closer (current-namespace) match instead of the
// intended root-level one — e.g. a package in `go.runtime.@internal` importing `internal/goarch`
// (namespace `@internal.goarch_package`) resolves `@internal` to `go.runtime.@internal`, not
// `go.@internal` → CS0234. Non-colliding imports (the common case) are returned unchanged, so this
// adds no churn for packages whose namespace does not nest under a colliding segment.
// isStrippedGoPathPackageRef reports whether a "go."-prefixed rendered name is actually a
// go/*-PACKAGE reference whose root was stripped — the leading "go" is the package import path's
// own first segment (go/ast, go/token, go/types → namespace go.go.ast), not the root namespace.
// Such a ref has a "_package" CLASS as the segment RIGHT AFTER "go.". A genuinely root-qualified
// ref (go.go.ast_package, go.io.fs_package) has a non-"_package" segment there, and a LOCAL package
// never renders its own class as "go.<class>_package" (after the root strip it is the bare
// "<class>_package"), so only go/*-package refs match. Used to re-root the go/* refs that
// convertToCSTypeName's redundant-root strip mangled to `go.ast_package` (CS0234/CS0426).
//
// The decision is made against packageChildNamespaces — the CURRENT package's rooted import-closure
// namespaces — not a purely-textual test, because the shapes are AMBIGUOUS: a stripped
// `go.build.constraint_package` (go/build/constraint, whose real namespace is `go.go.build`) looks
// exactly like a correctly-rooted `go.io.fs_package` (io/fs, whose real namespace IS `go.io`). A
// stripped go/* ref's namespace is NOT a real child namespace but becomes one with the root
// prepended (`go.build` ✗ → `go.go.build` ✓; `go` ✗ → `go.go` ✓); a genuinely-rooted ref's namespace
// is already real (`go.io`, `go.go.build`) and is left alone. This handles every go/* nesting depth
// (go/ast, go/build/constraint, …), superseding the earlier "_package immediately after go." shape
// test that only caught the two-segment case.
func isStrippedGoPathPackageRef(goPrefixed string) bool {
	segs := strings.Split(goPrefixed, ".")

	// Namespace = everything up to the first `_package` CLASS segment.
	nsEnd := -1

	for i, seg := range segs {
		if strings.HasSuffix(seg, PackageSuffix) {
			nsEnd = i
			break
		}
	}

	if nsEnd <= 0 {
		return false
	}

	pkgRef := strings.Join(segs[:nsEnd+1], ".")

	if packageQualifiedNamespaces[pkgRef] {
		return false
	}

	if packageQualifiedNamespaces[RootNamespace+"."+pkgRef] {
		return true
	}

	ns := strings.Join(segs[:nsEnd], ".")

	if packageChildNamespaces[ns] {
		return false
	}

	return packageChildNamespaces[RootNamespace+"."+ns]
}

func rootQualifyIfAmbiguous(ns string) string {
	if ns == "" {
		return ns
	}

	if strings.HasPrefix(ns, RootNamespace+".") {
		// A go/*-package namespace whose root the strip removed (`go.token_package` for go/token,
		// whose real namespace is `go.go`) is re-rooted to `go.go.token_package`, and ALWAYS with
		// `global::`: a bare `go.go.<pkg>_package` re-binds its leading `go` to the nearest enclosing
		// `go`, which mis-resolves from a go/*-package's own `go.go.*` namespace AND from any other
		// package under the root `go` (internal/pkgbits' `go.internal.pkgbits` resolved
		// `go.go.constant_package`'s second `go` inside `go.go`, CS0234). rootQualified only forces
		// `global::` when the IMPORTER itself nests `go.go`, so a non-go/* importer of a go/* package
		// was left bare — force it here. A genuinely-rooted ref is returned unchanged.
		if isStrippedGoPathPackageRef(ns) {
			return "global::" + RootNamespace + "." + ns
		}

		return ns
	}

	firstSeg := ns

	if dot := strings.Index(ns, "."); dot != -1 {
		firstSeg = ns[:dot]
	}

	// A relative target ALSO mis-binds when its leading segment is bound as a using-alias by a
	// same-package import — a sub-package import (`io/fs` → `io.fs_package`) whose parent (`io`) is
	// also imported (`using io = io_package;`) would otherwise bind `io` to that TYPE alias and
	// resolve `io.fs_package` to the nonexistent nested type `io_package.fs_package` (CS0426);
	// `go.io.fs_package` makes `io` resolve as the child namespace it was meant to be. A single-segment
	// namespace (`io_package`) has no leading qualifier to shadow, so this applies only to multi-segment
	// (sub-package) targets.
	if firstSeg != ns && packageImportLeadingSegments[firstSeg] {
		return rootQualified(ns)
	}

	for _, seg := range strings.Split(packageNamespace, ".") {
		if seg != RootNamespace && seg == firstSeg {
			return rootQualified(ns)
		}
	}

	// A relative alias target ALSO mis-binds when its first segment names a CHILD namespace of
	// the current namespace — contributed by the transitive reference closure, not the current
	// namespace's own path: runtime/metrics (namespace go.runtime) importing internal/godebugs
	// emitted `using godebugs = @internal.godebugs_package;`, but runtime.csproj's own
	// runtime/internal/* references put go.runtime.@internal in the compilation, so C#'s
	// inner-to-outer lookup binds `@internal` there (CS0234). packageChildNamespaces (the
	// CS0576 Δ-alias machinery) already mirrors that closure; walk every enclosing-namespace
	// prefix above the root, since any level can shadow the intended go.<firstSeg>.
	// ...and it mis-binds the same way when the nearer thing is a CLASS rather than a namespace,
	// which is the SINGLE-SEGMENT case the leading-segment rule above explicitly excludes. That
	// exclusion is right about what it says — a one-segment target has no leading qualifier to
	// shadow — and it does not cover the target CLASS ITSELF being shadowed by a nearer class of the
	// same simple name.
	//
	// internal/singleflight (namespace go.@internal) emits `using sync = sync_package;` for the ROOT
	// sync. At go1.23.12 that is the only class of that name and it binds correctly; go1.24 adds
	// internal/sync, so `go.@internal.sync_package` enters the closure, C#'s inner-to-outer lookup
	// finds it FIRST, and the alias silently rebinds to a class with no Once and no WaitGroup. The
	// same shape reaches go.@internal.syscall and go.@internal.trace.
	//
	// packageQualifiedNamespaces is the CLASS half of the map pair packageChildNamespaces is the
	// NAMESPACE half of — both are built from the same closure, one segment apart — so the two
	// questions are asked together here rather than in two places that could answer differently.
	// It fires only when a nearer class of that exact name is really in the closure, so a corpus
	// with one class of the name (every 1.23.12 package) emits exactly as before.
	prefix := packageNamespace

	for prefix != "" && prefix != RootNamespace {
		if packageChildNamespaces[prefix+"."+firstSeg] || packageQualifiedNamespaces[prefix+"."+firstSeg] {
			return rootQualified(ns)
		}

		if dot := strings.LastIndex(prefix, "."); dot != -1 {
			prefix = prefix[:dot]
		} else {
			break
		}
	}

	return ns
}

// packageUsingAlias returns the canonical C# using alias and target namespace for a Go import path,
// matching visitImportSpec's unaliased-import emission (`using <alias> = <namespace>;`). Used both to
// decide whether an import already emitted the canonical alias and to synthesize it in visitFile for a
// foreign type referenced without a canonical import.
func packageUsingAlias(importPath string) (alias string, namespace string) {
	namespace = rootQualifyIfAmbiguous(convertImportPathToNamespace(importPath, PackageSuffix))

	name := namespace

	if lastDot := strings.LastIndex(namespace, "."); lastDot != -1 {
		name = namespace[lastDot+1:]
	}

	alias = getSanitizedImport(strings.TrimSuffix(name, PackageSuffix))

	return alias, namespace
}

// resolveGorootVendoredPath maps a GOROOT-vendored import path to its ON-DISK form: a stdlib
// package imports `golang.org/x/text/transform` (the type info also carries the unprefixed
// path), but the converted package - its namespace, csproj, and output directory - lives at
// `vendor/golang.org/x/text/transform`, the key the stdlib dependency graph uses
// (stdLibConverter). Every namespace-text derivation must agree, or consumers emit
// `go.golang.org...` refs that exist nowhere (bidirule's 25 CS0234). The dotted-domain first
// segment is the cheap pre-filter (no plain stdlib path contains a dot). CAVEAT: a USER module
// that depends on the same golang.org/x package would false-positive here (its copy is not
// GOROOT-vendored); revisit with module-conversion support - the behavioral corpus has no such
// dependency today.
func resolveGorootVendoredPath(importPath string) string {
	firstSegment := importPath

	if idx := strings.Index(firstSegment, "/"); idx >= 0 {
		firstSegment = firstSegment[:idx]
	}

	if !strings.Contains(firstSegment, ".") || strings.HasPrefix(importPath, "vendor/") {
		return importPath
	}

	if _, err := os.Stat(filepath.Join(build.Default.GOROOT, "src", "vendor", filepath.FromSlash(importPath))); err == nil {
		return "vendor/" + importPath
	}

	return importPath
}

// majorVersionSegmentRegex matches a Go module major-version path segment (v2, v3, …).
var majorVersionSegmentRegex = regexp.MustCompile(`^v[0-9]+$`)

func convertImportPathToNamespace(importPath string, packageSuffix string) string {
	importPath = resolveGorootVendoredPath(importPath)

	// The import GRAPH is keyed by the path as written; only the emitted NAMESPACE elides the
	// repository's own `go2cs/` module marker, and it must elide it here for the same reason
	// getProjectName does on the declaration side — see trimGo2CSModulePrefix.
	graphKey := importPath

	importPath = trimGo2CSModulePrefix(importPath)

	// Split import path by "/"
	importPathParts := strings.Split(importPath, "/")

	// The emitted class is <packageName>_package, and a Go package name can differ from its
	// import-path's last segment — github.com/mattn/go-isatty is `package isatty`, and a hyphen in
	// the segment (`go-isatty`) is not even a valid C# identifier. Use the actual package name
	// (from the module-aware import graph, which is populated with valid Go identifiers) for the
	// last (class) segment whenever the graph knows it.
	//
	// This used to exclude the standard library, on the reasoning that a stdlib package is named
	// for its directory so its references would stay byte-identical. The premise holds for every
	// stdlib package but one, and the exception was invisible until darwin was built at all:
	// crypto/x509/internal/macos is `package macOS`, so the DECLARATION side emitted macOS_package
	// while every importer emitted macos_package, and C# is case-sensitive — CS0234 on
	// go.crypto.x509.@internal.macos_package, from crypto/x509's own root_darwin.cs. Censused
	// across all three targets, the stdlib paths whose name differs from their tail are exactly
	// four: crypto/x509/internal/macos (darwin only), math/rand/v2 and
	// internal/trace/internal/testgen/go122 and runtime/internal/wasitest (every target). The
	// second reaches the identical answer through the /vN branch below, and nothing in the corpus
	// imports the last two — so trusting the graph everywhere keeps the promise the exclusion was
	// making, instead of asserting it.
	if meta, ok := importPackageDirs[graphKey]; ok && meta.Name != "" && len(importPathParts) > 0 {
		importPathParts[len(importPathParts)-1] = meta.Name
	} else if len(importPathParts) > 1 {
		// A MAJOR-VERSION directory (`math/rand/v2`): the Go package is named for the PARENT
		// segment (`rand`), and the emitted class follows the package NAME — namespace
		// go.math.rand + class rand_package. The path-derived v2_package exists nowhere
		// (CS0234, internal/concurrent importing math/rand/v2). Convention-based: a /vN dir
		// hosts the parent-named package (true stdlib-wide; a package literally named vN
		// would need the type-graph name instead).
		if last := importPathParts[len(importPathParts)-1]; majorVersionSegmentRegex.MatchString(last) {
			importPathParts[len(importPathParts)-1] = importPathParts[len(importPathParts)-2]
		}
	}

	// Update all import path parts to sanitized identifiers. getSanitizedImport maps a hyphen/tilde in
	// a segment to an underscore (github.com/mattn/go-isatty) and splits the segment on its dots so an
	// embedded C# keyword is escaped per namespace level (the `in` of gopkg.in → `gopkg.@in`), which is
	// exactly what the DECLARATION side (getProjectName → getCoreSanitizedIdentifier) has always done —
	// the two agreeing is what makes a `gopkg.in/*` dependency referenceable from its importers.
	for i, part := range importPathParts {
		if i == len(importPathParts)-1 {
			part = part + packageSuffix
		}

		importPathParts[i] = getSanitizedImport(part)
	}

	return strings.Join(importPathParts, ".")
}

// packageClassPath returns pkgPath with its FINAL segment replaced by the Go package NAME — the
// segment the package's emitted C# class (`<name>_package`) is named for. The two agree for nearly
// every package (a Go package is named for its directory), so this normally returns pkgPath
// unchanged; they differ when the path tail is a major-version directory (math/rand/v2 is
// `package rand`: class rand_package under namespace go.math.rand, so a string-path renderer that
// composes a `<path>_package.<Type>` intermediate from the raw path targets the nonexistent
// `v2_package` — CS0426/CS0234, sort's test suite importing math/rand/v2) or a module directory
// named differently from its package (github.com/mattn/go-isatty is `package isatty`). Unlike the
// convention-based /vN branch above, this works from the type graph's authoritative package name.
func packageClassPath(pkgPath string, pkgName string) string {
	if idx := strings.LastIndex(pkgPath, "/"); idx != -1 && pkgPath[idx+1:] != pkgName {
		return pkgPath[:idx+1] + pkgName
	}

	return pkgPath
}
