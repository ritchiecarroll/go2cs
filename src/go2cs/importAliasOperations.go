// importAliasOperations.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/ast"
	"go/build"
	"go/types"
	"os"
	"path/filepath"
	"regexp"
	"sort"
	"strings"
)

// packageImportAliasRenames maps a package-local import qualifier (the identifier Go code
// qualifies with — the package's own name for an unaliased import, or the explicit alias) to
// its collision-renamed C# using alias. A C# using alias declared inside a namespace CONFLICTS
// with a same-named CHILD namespace visible from any (transitively) referenced assembly
// (CS0576 at every use): `using runtime = runtime_package;` inside `namespace go` collides
// with `go.runtime` the moment anything in the reference closure contains a runtime/*
// subpackage (runtime.csproj itself references runtime/internal/math|sys) — surfaced by
// iter/internal/weak in the first full-solution wave. Such an alias is Δ-renamed
// (`using Δruntime = runtime_package;` / `Δruntime.Goexit()`), the same marker every other
// collision rename uses. Populated by a synchronous pre-pass; read-only during concurrent
// file visiting.
var packageImportAliasRenames map[string]string

// packageChildNamespaces holds every namespace CHAIN contributed by the package's transitive
// import closure (package path a/b/c contributes namespaces go.a and go.a.b — the class c_package
// is not a namespace). Mirrors MSBuild's transitive ProjectReference visibility.
var packageChildNamespaces map[string]bool

// packageQualifiedNamespaces holds the rooted namespace-plus-package-class name for every package in
// the transitive import closure (package path a/b/c contributes go.a.b.c_package). This lets
// assembly-scope qualification distinguish a real root package from a stripped go/* package whose
// first path segment is also named go.
var packageQualifiedNamespaces map[string]bool

// siblingClosureImportPaths holds import paths contributed by a SIBLING compilation unit that
// shares the emitted assembly with the package being converted. Under -tests the package's
// production sources are RECOMPILED into the test assembly (TestingInfrastructureRequirements
// §2.1/§4.2), so that assembly's reference closure is the UNION of the production and _test.go
// closures — but the production conversion pass runs against the production package alone and so
// computed its namespace maps from the production half only. Every consumer of those maps
// (rootQualified's `go.go` shadow gate, rootQualifyIfAmbiguous, isStrippedGoPathPackageRef) then
// under-qualified the production usings for a shadow the test half introduces: math/rand/v2's
// regress_test.go imports go/format, making `go.go` a member of namespace `go` in the test
// assembly, so the production `using bits = go.math.bits_package;` re-bound its leading `go` to
// `go.go` (CS0234 x13). Populated once per -tests run before the production conversion (see
// collectSiblingTestClosure); empty for every non--tests conversion, so no other output moves.
var siblingClosureImportPaths []string

// testLocalTypePrefixes holds fully-qualified package-class prefixes OTHER than the current
// package's own whose members nonetheless compile into the SAME assembly, so a record naming one of
// their types must render in the bare local form rather than qualified. Only a `-tests` conversion
// populates it: the package under test is recompiled into the test assembly, but an EXTERNAL
// (`package <name>_test`) variant reaches it through its IMPORT PATH and so renders its types fully
// qualified — while the seeded production metadata carries the same pairs short. See
// stripLocalTypeQualifier. Empty for every other conversion.
var testLocalTypePrefixes []string

// packageImportLeadingSegments holds the C# using-alias identifier bound by every DIRECT import in
// the current package (an unaliased import's canonical name, or an explicit alias). A sub-package
// import path (`io/fs`) emits a RELATIVE namespace target (`io.fs_package`); if the leading segment
// (`io`) is also imported (`using io = io_package;`), C# binds that segment to the TYPE alias, so
// `io.fs_package` resolves to the nonexistent nested type `io_package.fs_package` (CS0426). Recording
// the bound leading identifiers lets rootQualifyIfAmbiguous prefix "go." onto such a target
// (`go.io.fs_package`) so the segment resolves as the child NAMESPACE it was meant to be. Populated by
// computeImportAliasRenames' synchronous pre-pass; read-only during concurrent file visiting.
var packageImportLeadingSegments map[string]bool

// computeImportAliasRenames populates the two maps above for the package being converted.
// packageNS is the emission namespace of the current package (e.g. "go", "go.@internal").
// includeSiblingClosure selects the COMPILATION UNIT the maps describe. The production `.cs` of a
// package compiles into the production assembly, whose reference closure is the package's own; the
// converted `_test.go` variants compile into the test assembly, whose closure is the UNION with the
// test half. One map set cannot serve both: computed with the union and applied to the production
// files, it Δ-renames a production alias against a collision only the test assembly has (measured
// 2026-09-22 on `regexp`: `using io` -> `using Δio` with `io/fs` absent from `regexp.csproj`'s
// closure and present in `regexp.tests.csproj`'s), which compiles but is not what the production
// unit means and rewrites tracked corpus bytes on every -tests run.
//
// The one case where the production files DO belong to the test assembly is testProjectRecompile,
// which makes them compile items of it; there the union IS the production unit's closure and the
// caller passes true.
func computeImportAliasRenames(files []FileEntry, pkg *types.Package, packageNS string, corpusRoot string, goos string, includeSiblingClosure bool) {
	closure := make(map[string]bool)

	// The loader's closure packages themselves, not only their paths: a package this one reaches
	// only through a TYPE is qualified by its NAME, so the rename below must be able to test it.
	closurePackages := make(map[string]*types.Package)

	var walk func(p *types.Package)

	walk = func(p *types.Package) {
		for _, imp := range p.Imports() {
			if !closure[imp.Path()] {
				closure[imp.Path()] = true
				closurePackages[imp.Path()] = imp
				walk(imp)
			}
		}
	}

	walk(pkg)

	// Fold in the sibling compilation unit's closure (the _test.go half under -tests) so the
	// namespace maps describe the ASSEMBLY the emitted C# compiles into, not just this package.
	// Gated on the unit: see includeSiblingClosure on the signature above.
	if includeSiblingClosure {
		for _, path := range siblingClosureImportPaths {
			closure[path] = true
		}
	}

	// Fold in the CORPUS reference closure of every package this one IMPORTS, for the same reason
	// and in the same shape: the maps must describe the assembly's REFERENCE closure, and the loop
	// above walks the GO loader's imports, which is that set only while the loader's release and the
	// corpus's release agree. They do not during a toolchain hop (see corpusReferenceClosure).
	//
	// Keyed on the IMPORTS, not on pkg itself. The emitted assembly references `core/<import>` for
	// each import, and MSBuild makes those references' own references visible transitively — so it
	// is `core/runtime.csproj` referencing runtime/internal/{math,sys} that puts `go.runtime` in
	// scope for a package importing `runtime`. Keying on pkg.Path() reads the converted package's
	// OWN corpus csproj, which for a behavioral project or any non-corpus module does not exist at
	// all: measured, that lookup found nothing and every alias stayed bare.
	//
	// Contributes nothing when no corpus csproj exists, so a standalone or -recurse=nuget conversion
	// is unchanged.
	for _, imp := range pkg.Imports() {
		for _, path := range corpusReferenceClosure(corpusRoot, imp.Path(), goos) {
			closure[path] = true
		}
	}

	// A GOROOT package's `golang.org/x/…` imports are GOROOT-VENDORED — visitImportSpec resolves them
	// to their `vendor/…` on-disk path (and namespace) when the importing file lives under GOROOT. The
	// child-namespace map must use the SAME resolved form or a vendored sub-namespace (e.g.
	// `go.vendor.golang.org.x.text.unicode`) is absent, so rootQualifyIfAmbiguous cannot see that a
	// stdlib alias's leading segment (`unicode` of `unicode/utf8`) collides with it — bidirule emitted
	// `using utf8 = unicode.utf8_package;` binding `unicode` to the vendored namespace (CS0234). Gated
	// on the package living under GOROOT so a user module's own golang.org/x dependency is untouched.
	isGorootPackage := false

	if len(files) > 0 {
		goroot := filepath.Clean(build.Default.GOROOT)
		isGorootPackage = strings.HasPrefix(filepath.Clean(files[0].filePath), goroot+string(filepath.Separator))
	}

	for path := range closure {
		importPath := path

		if isGorootPackage {
			path = resolveGorootVendoredPath(path)
		}

		packageQualifiedNamespaces[RootNamespace+"."+convertImportPathToNamespace(path, PackageSuffix)] = true

		parts := strings.Split(path, "/")
		ns := RootNamespace

		for _, part := range parts[:len(parts)-1] {
			ns += "." + getSanitizedImport(part)
			packageChildNamespaces[ns] = true
		}

		// A package that keeps its directory segment (packageKeepsDirectorySegment) declares one level
		// deeper: teststructs/foo1, `package foo`, is namespace <parent>.teststructs.foo1, so an alias
		// `foo1` inside <parent>.teststructs collides with it (CS0576) like any other child namespace.
		if tail := parts[len(parts)-1]; packageKeepsDirectorySegment(tail, closurePackageName(importPath, closurePackages)) {
			packageChildNamespaces[ns+"."+getSanitizedImport(tail)] = true
		}
	}

	// The package's OWN class is a member of its own namespace in every compilation, so it shadows
	// a same-named import target exactly as a closure class does: github.com/pkg/errors (namespace
	// go.github.com.pkg) emitted `using stderrors = errors_package;` for the standard library's
	// errors, which bound to ITSELF (Is called Is). Only the class half: the package's namespace
	// chain is its own namespace and adding it would only re-qualify what already binds.
	if pkg != nil {
		ownPath := pkg.Path()

		if isGorootPackage {
			ownPath = resolveGorootVendoredPath(ownPath)
		}

		packageQualifiedNamespaces[RootNamespace+"."+convertImportPathToNamespace(ownPath, PackageSuffix)] = true
	}

	collides := func(qualifier string) bool {
		return packageChildNamespaces[packageNS+"."+getSanitizedImport(qualifier)]
	}

	// Canonical (unaliased) import names across the package.
	for _, imp := range pkg.Imports() {
		name := imp.Name()

		// Every canonically-imported package binds its name as a using-alias — record the SANITIZED
		// form (matching how namespace segments are rendered) so a sub-package's relative namespace
		// whose leading segment equals it gets root-qualified.
		packageImportLeadingSegments[getSanitizedImport(name)] = true

		if collides(name) {
			packageImportAliasRenames[name] = ShadowVarMarker + name
		}
	}

	// Explicitly aliased imports (`import foo "runtime"`) qualify by the alias instead.
	for _, fileEntry := range files {
		for _, importSpec := range fileEntry.file.Imports {
			if importSpec.Name == nil {
				continue
			}

			alias := importSpec.Name.Name

			if alias == "." || alias == "_" {
				continue
			}

			// An explicit alias binds THAT (sanitized) identifier as the file-local using-alias.
			packageImportLeadingSegments[getSanitizedImport(alias)] = true

			if collides(alias) {
				packageImportAliasRenames[alias] = ShadowVarMarker + alias
			}
		}
	}

	// Packages the package reaches only TRANSITIVELY are qualified by their name too: a type from one
	// (an inferred type argument, a func result) is recorded by collectTypePackages, visitFile supplies
	// its canonical using, and both that using and every type reference read importQualifier. Testing
	// only the two populations above left such a name bare where it collides — crypto/internal/hpke
	// never imports crypto/internal/fips140, yet names fips140.Hash, and `using fips140 = …` inside
	// go.crypto.@internal collides with the go.crypto.@internal.fips140 child its fips140/hkdf import
	// makes visible (CS0576). Keyed by name exactly as an import is, so an import and a type reach of
	// the same name cannot disagree. Leading segments are NOT recorded: a transitive package binds no
	// using unless a file references it, and rootQualifyIfAmbiguous must not change for the ones it
	// never does.
	for _, closurePackage := range closurePackages {
		if name := closurePackage.Name(); collides(name) {
			packageImportAliasRenames[name] = ShadowVarMarker + name
		}
	}
}

// closurePackageName is a closure package's Go name: from the loader when it holds the package, else
// from the import graph. Empty when neither knows it, which keeps the path-derived namespace.
func closurePackageName(importPath string, closurePackages map[string]*types.Package) string {
	if pkg, ok := closurePackages[importPath]; ok {
		return pkg.Name()
	}

	return importPackageDirs[importPath].Name
}

// importQualifier returns the (possibly collision-renamed) C# alias for a package qualifier.
func importQualifier(name string) string {
	if packageImportAliasRenames != nil {
		if renamed, ok := packageImportAliasRenames[name]; ok {
			return renamed
		}
	}

	return name
}

// corpusCsprojDirectRefs memoizes ONE csproj's direct ProjectReference import paths, keyed by the
// target GOOS AND the referring import path. Read once per conversion run: a -stdlib pass asks for
// hundreds of closures and they overlap heavily, so without this the same csproj is parsed once per
// dependent. The GOOS is part of the key because the answer DEPENDS on it (see corpusRefsForGOOS)
// and platformEmit converts every target in ONE process — keyed by import path alone, the second
// target would be served the first target's references.
var corpusCsprojDirectRefs map[string][]string

// corpusReferenceClosure memoizes the full transitive answer, keyed the same way and for the same
// reason.
var corpusReferenceClosureCache map[string][]string

// goosConditionRE captures the GOOS an <ItemGroup> gates on. The corpus's condition set is CLOSED,
// and it was censused before this was written rather than assumed: every <ProjectReference> in every
// production csproj sits either in an UNCONDITIONAL ItemGroup (2651 of them) or under exactly one
// `'$(GoTargetOS)'=='<goos>'` equality (linux 56, darwin 45, windows 23). There is no `!=`, no
// AND/OR, and no `$(OutputType)` group encloses a ProjectReference at all.
var goosConditionRE = regexp.MustCompile(`Condition\s*=\s*"'\$\(GoTargetOS\)'\s*==\s*'([^']*)'"`)

// corpusRefsForGOOS returns the ProjectReference import paths a csproj contributes WHEN BUILT FOR
// goos. A corpus csproj carries its per-GOOS references in <ItemGroup Condition="'$(GoTargetOS)'==
// '<goos>'"> blocks — the same conditioning that decides the COMPILE item set under layout L3 — so a
// reader that takes every reference in the file describes an assembly no build ever produces.
//
// That is not a hypothetical: it is the defect this function fixes. `syscall.csproj` references
// `internal/syscall/windows/sysdll` inside a windows-only group, so an unconditioned read put
// `go.internal.syscall` in `internal/sysinfo`'s DARWIN closure and renamed its `syscall` alias on a
// target where nothing declares that namespace. Measured two ways: the reference conditions read at
// the source, and `internal.sysinfo` compiled at master under GoTargetOS=darwin — rc 0, CS0576 zero.
//
// Narrowing to the target's own groups does NOT weaken the never-under-approximate rule this file's
// header states. That rule guards against PROXIES for the reference graph; this reads the same
// property MSBuild itself evaluates, so excluding another target's references is exact rather than
// optimistic — those assemblies are not in the build being emitted.
//
// The scan is line-structured because the emission is: an <ItemGroup> opens and closes on its own
// line. A SELF-CLOSING group (`<ItemGroup Condition=… />`, 20 of them) has no body, so it must not
// open a region — treating it as one would swallow every line up to the next `</ItemGroup>`,
// removing UNCONDITIONAL references that sit in between. A condition outside the censused form
// leaves its group's references INCLUDED, which is the pre-existing behaviour rather than a new hole.
func corpusRefsForGOOS(csprojContent string, goos string) []string {
	var admissible strings.Builder

	inGroup := false
	groupGOOS := ""

	for _, line := range strings.Split(csprojContent, "\n") {
		trimmed := strings.TrimSpace(line)

		switch {
		case strings.HasPrefix(trimmed, "<ItemGroup"):
			if !strings.HasSuffix(trimmed, "/>") {
				inGroup = true
				groupGOOS = ""

				if m := goosConditionRE.FindStringSubmatch(trimmed); m != nil {
					groupGOOS = m[1]
				}
			}

			continue
		case strings.HasPrefix(trimmed, "</ItemGroup>"):
			inGroup = false
			groupGOOS = ""

			continue
		}

		if !inGroup || groupGOOS == "" || groupGOOS == goos {
			admissible.WriteString(line)
			admissible.WriteString("\n")
		}
	}

	return parseCoreProjectRefs(admissible.String())
}

// corpusReferenceClosure returns every Go import path reachable from importPath through the
// CORPUS's csproj ProjectReference graph rooted at options.go2csPath. Empty when no corpus is
// configured or the package has no emitted csproj there.
//
// WHY IT EXISTS. packageChildNamespaces above is built from the GO LOADER's import closure, which
// is a PROXY for the population that actually decides the collision: the C# namespaces visible
// through the emitted assembly's transitive ProjectReferences. This file's own header already
// names that population — "runtime.csproj itself references runtime/internal/math|sys" — so the
// proxy was never the rule, only a stand-in that is exact while the loader's release and the
// corpus's release agree. Train 43 made them disagree: the converter builds under go1.24.13 while
// the corpus is still the 1.23.12 emission, and 1.24's `runtime` imports internal/runtime/* with
// no `runtime/...` child at all. The loader closure therefore yields a BARE `using runtime = …`
// while the corpus's runtime.csproj still references runtime/internal/{math,sys}, so `go.runtime`
// exists in the reference closure and the alias is CS0576 at every use.
//
// The asymmetry decides the shape: a SPURIOUS rename is an alias spelling and compiles, a MISSING
// rename is fatal. So this may over-approximate and must never under-approximate — but not by
// directory existence, which would mint Δruntime forever on every tree that has core/runtime/debug
// on disk. Reading the REFERENCE GRAPH reads the population that decides the collision, and it is
// self-correcting across the hop: at H5 the corpus references internal/runtime/* too, `go.runtime`
// stops existing, and the alias drops by itself with no further converter change.
//
// Absent a corpus csproj (-recurse=nuget, a standalone conversion) this contributes NOTHING and the
// loader closure decides alone, exactly as before.
func corpusReferenceClosure(corpusRoot string, importPath string, goos string) []string {
	if importPath == "" || corpusRoot == "" {
		return nil
	}

	if cached, ok := corpusReferenceClosureCache[goos+"|"+importPath]; ok {
		return cached
	}

	coreRoot := filepath.Join(corpusRoot, "core")
	seen := make(map[string]bool)

	var walk func(path string)

	walk = func(path string) {
		if path == "" || seen[path] {
			return
		}

		seen[path] = true

		for _, ref := range corpusDirectReferences(coreRoot, path, goos) {
			walk(ref)
		}
	}

	walk(importPath)

	// A package is not part of its own child-namespace contribution: the collision is with a
	// namespace a REFERENCED assembly declares.
	delete(seen, importPath)

	closure := make([]string, 0, len(seen))

	for path := range seen {
		closure = append(closure, path)
	}

	// Deterministic order so a caller folding this into a map cannot vary with map iteration.
	sort.Strings(closure)

	if corpusReferenceClosureCache == nil {
		corpusReferenceClosureCache = make(map[string][]string)
	}

	corpusReferenceClosureCache[goos+"|"+importPath] = closure

	return closure
}

// corpusDirectReferences reads one emitted csproj and returns the import paths it references
// directly.
//
// The extraction is solutionGenerator's, not a second implementation of it: parseCoreProjectRefs
// already matches `Include="$(go2csPath)core/…csproj"` for both separators and normalizes with
// normalizeEmittedPath rather than filepath.ToSlash — which its own comment explains is the
// difference between reading a backslashed reference on Linux and silently missing every one — and
// importPathOf already maps `core/internal/abi/internal.abi.csproj` back to `internal/abi`. Reusing
// them keeps ONE definition of how an emitted reference is read; a private copy here would be a
// second derivation to drift.
//
// `core/golib` maps to the single segment `golib`, which contributes no namespace PREFIX (a path
// with no separator has no non-final part), so the hand-written runtime needs no special case.
func corpusDirectReferences(coreRoot string, importPath string, goos string) []string {
	if cached, ok := corpusCsprojDirectRefs[goos+"|"+importPath]; ok {
		return cached
	}

	// A Glob keeps this independent of the dotted-name spelling — but a package directory holds TWO
	// csprojs once its tests have been emitted, and the `.tests.csproj` is NOT part of a production
	// assembly's reference closure. Reading it over-approximates enormously and wrongly: measured,
	// folding the test closure in renamed aliases across 670 behavioral projects (`time` → `Δtime`
	// via time/tzdata, `os` → `Δos` via os/exec — both reachable only from the test csproj). The
	// test half has its own contributor for the conversion that needs it: siblingClosureImportPaths,
	// which -tests populates and this must not duplicate.
	matches, err := filepath.Glob(filepath.Join(coreRoot, filepath.FromSlash(importPath), "*.csproj"))
	refs := []string{}

	if err == nil {
		for _, csproj := range matches {
			if strings.HasSuffix(filepath.Base(csproj), ".tests.csproj") {
				continue
			}

			content, readErr := os.ReadFile(csproj)

			if readErr != nil {
				continue
			}

			for _, coreProject := range corpusRefsForGOOS(string(content), goos) {
				if referenced := importPathOf(coreProject); referenced != "" {
					refs = append(refs, referenced)
				}
			}
		}
	}

	if corpusCsprojDirectRefs == nil {
		corpusCsprojDirectRefs = make(map[string][]string)
	}

	corpusCsprojDirectRefs[goos+"|"+importPath] = refs

	return refs
}

// identIsRenamedImport reports whether ident is a package qualifier with a renamed alias.
func (v *Visitor) identifierIsPackageName(ident *ast.Ident) (string, bool) {
	if obj := v.info.ObjectOf(ident); obj != nil {
		if pkgName, ok := obj.(*types.PkgName); ok {
			return pkgName.Name(), true
		}
	}

	return "", false
}
