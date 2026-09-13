// corpusReferenceClosure_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"os"
	"path/filepath"
	"strings"
	"testing"

	"go/types"
)

// The child-namespace set that decides an import-alias rename must describe the assembly's
// REFERENCE closure. computeImportAliasRenames builds it from the GO LOADER's import closure, which
// equals that set only while the loader's release and the corpus's release agree — and a toolchain
// hop is exactly when they do not (see corpusReferenceClosure's header).
//
// Every closure here is SYNTHETIC: a hand-built types.Package and, for the corpus arms, a fixture
// tree under t.TempDir(). Nothing reads GOROOT or src/core, so these arms cannot rot when either
// release moves — which matters because the defect they pin is itself a release-skew defect and a
// guard that read the live GOROOT would silently change what it tests at the next hop.
func TestImportAliasRenameReadsBothClosures(t *testing.T) {
	// A package importing `runtime` directly, plus whatever else the arm needs in the loader closure.
	packageImporting := func(direct ...*types.Package) *types.Package {
		pkg := types.NewPackage("iter", "iter")
		pkg.SetImports(direct)
		return pkg
	}

	runtimePkg := func() *types.Package { return types.NewPackage("runtime", "runtime") }

	// withLoaderChild returns `runtime` carrying ONE transitive import, which is what contributes the
	// `go.runtime` child namespace through the loader half.
	withLoaderChild := func(childPath string) *types.Package {
		rt := runtimePkg()
		child := types.NewPackage(childPath, filepath.Base(childPath))
		rt.SetImports([]*types.Package{child})
		return rt
	}

	// writeFixtureCorpus lays down the minimum a corpus needs for corpusDirectReferences to read it:
	// core/<pkg>/<name>.csproj with ProjectReferences in the emitted spelling, and — when
	// testReferences is non-empty — the `.tests.csproj` SIBLING that a package gains once its tests
	// have been emitted. The sibling exists in the real corpus and must be IGNORED here; see the arm
	// below for what reading it costs.
	writeFixtureCorpus := func(t *testing.T, pkgPath string, references []string, testReferences []string) string {
		t.Helper()

		root := t.TempDir()
		dir := filepath.Join(root, "core", filepath.FromSlash(pkgPath))

		if err := os.MkdirAll(dir, 0o755); err != nil {
			t.Fatalf("fixture corpus: %v", err)
		}

		project := func(refs []string) string {
			content := "<Project Sdk=\"Microsoft.NET.Sdk\">\n  <ItemGroup>\n"

			for _, ref := range refs {
				// The emitted spelling parseCoreProjectRefs matches, dotted file name and all.
				dotted := strings.ReplaceAll(ref, "/", ".")
				content += "    <ProjectReference Include=\"$(go2csPath)core/" + ref + "/" + dotted + ".csproj\" />\n"
			}

			return content + "  </ItemGroup>\n</Project>\n"
		}

		if err := os.WriteFile(filepath.Join(dir, "fixture.csproj"), []byte(project(references)), 0o644); err != nil {
			t.Fatalf("fixture csproj: %v", err)
		}

		if len(testReferences) > 0 {
			if err := os.WriteFile(filepath.Join(dir, "fixture.tests.csproj"), []byte(project(testReferences)), 0o644); err != nil {
				t.Fatalf("fixture tests csproj: %v", err)
			}
		}

		return root
	}

	// writeGOOSConditionedCorpus lays down a csproj whose reference sits inside a
	// <ItemGroup Condition="'$(GoTargetOS)'=='<goos>'"> block — the shape a real corpus csproj uses
	// for its per-GOOS references, and the one that produced the defect the arm below pins. The
	// SELF-CLOSING empty group in front of it is deliberate: it has no body and must not open a
	// region, or the scan would swallow the unconditional group that follows and lose a reference.
	writeGOOSConditionedCorpus := func(t *testing.T, pkgPath string, conditionGOOS string, conditioned string, unconditional string) string {
		t.Helper()

		root := t.TempDir()
		dir := filepath.Join(root, "core", filepath.FromSlash(pkgPath))

		if err := os.MkdirAll(dir, 0o755); err != nil {
			t.Fatalf("fixture corpus: %v", err)
		}

		ref := func(path string) string {
			dotted := strings.ReplaceAll(path, "/", ".")
			return "    <ProjectReference Include=\"$(go2csPath)core/" + path + "/" + dotted + ".csproj\" />\n"
		}

		content := "<Project Sdk=\"Microsoft.NET.Sdk\">\n" +
			"  <ItemGroup Condition=\"'$(GoTargetOS)'=='" + conditionGOOS + "'\" />\n" +
			"  <ItemGroup Condition=\"'$(GoTargetOS)'=='" + conditionGOOS + "'\">\n" + ref(conditioned) + "  </ItemGroup>\n" +
			"  <ItemGroup>\n" + ref(unconditional) + "  </ItemGroup>\n" +
			"</Project>\n"

		if err := os.WriteFile(filepath.Join(dir, "fixture.csproj"), []byte(content), 0o644); err != nil {
			t.Fatalf("fixture csproj: %v", err)
		}

		return root
	}

	var renameForGOOS func(*testing.T, *types.Package, string, string, string) (string, bool)

	// renameFor runs the real pre-pass over synthetic state and returns the alias recorded for
	// `qualifier`, plus whether one was recorded at all. The arms that predate per-GOOS conditioning
	// build UNCONDITIONAL fixtures, whose references every target sees, so the GOOS they run under
	// cannot change their answer; they delegate here rather than each naming one.
	renameFor := func(t *testing.T, pkg *types.Package, corpusRoot string, qualifier string) (string, bool) {
		t.Helper()
		return renameForGOOS(t, pkg, corpusRoot, qualifier, "windows")
	}

	renameForGOOS = func(t *testing.T, pkg *types.Package, corpusRoot string, qualifier string, goos string) (string, bool) {
		t.Helper()

		previousRenames, previousSegments := packageImportAliasRenames, packageImportLeadingSegments
		previousQualified, previousSibling := packageQualifiedNamespaces, siblingClosureImportPaths

		t.Cleanup(func() {
			packageImportAliasRenames, packageImportLeadingSegments = previousRenames, previousSegments
			packageQualifiedNamespaces, siblingClosureImportPaths = previousQualified, previousSibling
		})

		// Both caches are per-run memoization; a stale entry from a sibling arm would answer for this
		// one and the arms would stop being independent.
		corpusReferenceClosureCache = nil
		corpusCsprojDirectRefs = nil

		packageImportAliasRenames = map[string]string{}
		packageImportLeadingSegments = map[string]bool{}
		packageQualifiedNamespaces = map[string]bool{}
		siblingClosureImportPaths = nil

		setShadowState(t, RootNamespace, nil)

		computeImportAliasRenames(nil, pkg, RootNamespace, corpusRoot, goos)

		alias, renamed := packageImportAliasRenames[qualifier]

		return alias, renamed
	}

	t.Run("loader closure alone still decides when it can", func(t *testing.T) {
		// The pre-hop state: `runtime` transitively imports runtime/internal/sys, so `go.runtime`
		// exists through the LOADER half and the alias must be renamed with no corpus at all.
		pkg := packageImporting(withLoaderChild("runtime/internal/sys"))

		alias, renamed := renameFor(t, pkg, "", "runtime")

		if !renamed || alias != ShadowVarMarker+"runtime" {
			t.Fatalf("loader closure holds runtime/internal/sys: want %q, got %q (renamed=%v)",
				ShadowVarMarker+"runtime", alias, renamed)
		}
	})

	t.Run("no child in either closure leaves the alias bare", func(t *testing.T) {
		// The post-hop loader state (1.24 `runtime` imports internal/runtime/*, which contributes
		// `go.internal`, NOT `go.runtime`) with NO corpus to consult.
		pkg := packageImporting(withLoaderChild("internal/runtime/sys"))

		alias, renamed := renameFor(t, pkg, "", "runtime")

		if renamed {
			t.Fatalf("neither closure holds a runtime/* child: want no rename, got %q", alias)
		}
	})

	t.Run("CORPUS closure renames what the loader closure no longer sees", func(t *testing.T) {
		// THE LOAD-BEARING ARM, and the one that is RED on the pre-fix predicate. Loader half is the
		// post-hop shape (no `go.runtime`); the corpus's runtime.csproj still references
		// runtime/internal/sys, so `go.runtime` exists in the assembly's reference closure and the
		// alias is CS0576 at every use unless it is renamed.
		pkg := packageImporting(withLoaderChild("internal/runtime/sys"))
		corpus := writeFixtureCorpus(t, "runtime", []string{"runtime/internal/sys"}, nil)

		alias, renamed := renameFor(t, pkg, corpus, "runtime")

		if !renamed || alias != ShadowVarMarker+"runtime" {
			t.Fatalf("corpus references runtime/internal/sys: want %q, got %q (renamed=%v)",
				ShadowVarMarker+"runtime", alias, renamed)
		}
	})

	t.Run("a corpus that references no runtime child does not invent a rename", func(t *testing.T) {
		// The over-approximation bound: reading the reference GRAPH must not become "rename whenever
		// core/runtime/<sub> exists on disk". A corpus whose csproj references something else leaves
		// the alias bare.
		pkg := packageImporting(withLoaderChild("internal/runtime/sys"))
		corpus := writeFixtureCorpus(t, "runtime", []string{"internal/abi"}, nil)

		alias, renamed := renameFor(t, pkg, corpus, "runtime")

		if renamed {
			t.Fatalf("corpus references no runtime/* child: want no rename, got %q", alias)
		}
	})

	t.Run("the .tests.csproj sibling contributes NOTHING", func(t *testing.T) {
		// A package directory holds TWO csprojs once its tests are emitted, and only the production
		// one is in a production assembly's reference closure. Reading both was measured to rename
		// aliases across 670 behavioral projects — `time` → Δtime through time/tzdata and `os` → Δos
		// through os/exec, neither reachable from the production csproj — while the FOUR arms above
		// all stayed green, because none of them put a .tests.csproj on disk. The test half has its
		// own contributor under -tests (siblingClosureImportPaths) and must not be duplicated here.
		pkg := packageImporting(withLoaderChild("internal/runtime/sys"))
		corpus := writeFixtureCorpus(t, "runtime",
			[]string{"internal/abi"},         // production: no runtime/* child
			[]string{"runtime/internal/sys"}) // tests-only: a runtime/* child that must NOT count

		alias, renamed := renameFor(t, pkg, corpus, "runtime")

		if renamed {
			t.Fatalf("only the .tests.csproj holds a runtime/* child: want no rename, got %q", alias)
		}
	})

	// A GOOS-CONDITIONED reference belongs to the target it names and to no other. This is the arm
	// the first version of this guard did not have, and the defect it now pins SHIPPED because of
	// that: syscall.csproj references internal/syscall/windows/sysdll inside a windows-only group,
	// an unconditioned read put `go.internal.syscall` in internal/sysinfo's DARWIN closure, and the
	// darwin emission renamed a `syscall` alias on a target where nothing declares that namespace.
	//
	// BOTH DIRECTIONS, because either alone is green on a broken predicate: a fold that dropped every
	// conditioned reference passes the darwin half, and a fold that kept every one passes the windows
	// half. Only the pair distinguishes conditioning from either blanket answer.
	t.Run("a GOOS-conditioned reference is visible ONLY to its own target", func(t *testing.T) {
		// The conditioned reference contributes `go.runtime`; the unconditional one contributes
		// `go.encoding`, and is here so the arm can tell "conditioning works" from "the fold stopped
		// reading this csproj at all" — a distinction the rename alone cannot make.
		corpusRoot := writeGOOSConditionedCorpus(t, "runtime", "windows", "runtime/internal/sys", "encoding/json")

		pkg := packageImporting(runtimePkg())

		if alias, renamed := renameForGOOS(t, pkg, corpusRoot, "runtime", "windows"); !renamed || alias != ShadowVarMarker+"runtime" {
			t.Fatalf("windows target sees its own conditioned reference: want %q, got %q (renamed=%v)",
				ShadowVarMarker+"runtime", alias, renamed)
		}

		if alias, renamed := renameForGOOS(t, pkg, corpusRoot, "runtime", "darwin"); renamed {
			t.Fatalf("a windows-conditioned reference is not in the DARWIN closure: want no rename, got %q", alias)
		}

		// VACUITY CONTROL. The darwin no-rename above is only evidence of CONDITIONING if the fold
		// still read that csproj on darwin at all — a fold that failed to open the file, or that
		// dropped every conditioned group AND its unconditional siblings, would produce the same
		// no-rename. The unconditional reference must therefore still land: a package importing both
		// `runtime` and `encoding` gets `encoding` renamed on darwin, from the group with no condition.
		bothImporter := types.NewPackage("iter", "iter")
		bothImporter.SetImports([]*types.Package{types.NewPackage("encoding", "encoding"), runtimePkg()})

		if alias, renamed := renameForGOOS(t, bothImporter, corpusRoot, "encoding", "darwin"); !renamed || alias != ShadowVarMarker+"encoding" {
			t.Fatalf("the UNCONDITIONAL reference must still be read on darwin (else the arm above is vacuous): want %q, got %q (renamed=%v)",
				ShadowVarMarker+"encoding", alias, renamed)
		}
	})
}

// The two-sided control: one package, two direct imports, and only the one whose child namespace
// exists may be renamed. A predicate that renamed everything, or nothing, passes each single-sided
// arm above; it cannot pass this one.
func TestImportAliasRenameIsTwoSided(t *testing.T) {
	previousRenames, previousSegments := packageImportAliasRenames, packageImportLeadingSegments
	previousQualified, previousSibling := packageQualifiedNamespaces, siblingClosureImportPaths

	t.Cleanup(func() {
		packageImportAliasRenames, packageImportLeadingSegments = previousRenames, previousSegments
		packageQualifiedNamespaces, siblingClosureImportPaths = previousQualified, previousSibling
	})

	corpusReferenceClosureCache = nil
	corpusCsprojDirectRefs = nil

	packageImportAliasRenames = map[string]string{}
	packageImportLeadingSegments = map[string]bool{}
	packageQualifiedNamespaces = map[string]bool{}
	siblingClosureImportPaths = nil

	encoding := types.NewPackage("encoding", "encoding")
	encoding.SetImports([]*types.Package{types.NewPackage("encoding/json", "json")})

	runtime := types.NewPackage("runtime", "runtime")
	runtime.SetImports([]*types.Package{types.NewPackage("internal/runtime/sys", "sys")})

	pkg := types.NewPackage("iter", "iter")
	pkg.SetImports([]*types.Package{encoding, runtime})

	setShadowState(t, RootNamespace, nil)
	computeImportAliasRenames(nil, pkg, RootNamespace, "", "")

	if got, ok := packageImportAliasRenames["encoding"]; !ok || got != ShadowVarMarker+"encoding" {
		t.Fatalf("encoding/json contributes go.encoding: want %q, got %q (renamed=%v)",
			ShadowVarMarker+"encoding", got, ok)
	}

	if got, ok := packageImportAliasRenames["runtime"]; ok {
		t.Fatalf("internal/runtime/sys contributes go.internal, not go.runtime: want no rename, got %q", got)
	}
}
