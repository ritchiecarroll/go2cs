// linknameCycleGuard_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"path/filepath"
	"runtime"
	"testing"

	"golang.org/x/tools/go/packages"
)

// W1's guards. `linknamePullWouldCycle` decides whether a two-argument `//go:linkname` var pull is
// emitted as a cross-assembly forwarding property (which adds a ProjectReference) or keeps its plain
// field. It used to answer that question from the CONVERT-SET dependency graph alone, and to answer
// "no cycle" whenever there was no graph — which is every single-package and every `-tests`
// conversion. That shortcut cost W1: converting `runtime` under `-stdlib` suppressed the
// `internal/syscall/windows.CanUseLongPaths` pull, converting the SAME package under `-tests` emitted
// it, and the resulting `runtime -> internal/syscall/windows` reference closes a project cycle
// through Go's own `internal/syscall/windows -> syscall -> runtime`. One variable, two answers, no
// diagnostic. See docs/phase4/DESIGN-linkname-push-cycles.md.
//
// These tests pin the four properties the armed guard must have, and each is red-provable by an
// isolated edit named in its own comment.

// linknameCycleTestState installs the package-scoped state linknamePullWouldCycle reads, and restores
// it. The memo is cleared BOTH ways: a cached answer from another test would make these assertions
// measure the cache instead of the resolver, which is the mirror of the stale-binary trap.
func linknameCycleTestState(t *testing.T, graph *DependencyGraph, pkgPath, sourceDir string, imports map[string]*packages.Package) {
	t.Helper()

	savedGraph := conversionGraph
	savedPath := currentPackagePath
	savedDir := packageSourceDir
	savedImports := importedPackages
	savedClosures := linknameTargetClosures

	conversionGraph = graph
	currentPackagePath = pkgPath
	packageSourceDir = sourceDir
	importedPackages = imports
	linknameTargetClosures = map[string]linknameCycleAnswer{}

	t.Cleanup(func() {
		conversionGraph = savedGraph
		currentPackagePath = savedPath
		packageSourceDir = savedDir
		importedPackages = savedImports
		linknameTargetClosures = savedClosures
	})
}

// linknameCycleTestOptions is the loader context a single-package/-tests conversion supplies: the
// target platform the pull is being converted for. Pinned to windows because W1's pair only exists
// there — `internal/syscall/windows` has no non-Windows build — so a host-platform default would
// make the assertion vacuous off Windows rather than merely unmeasured.
func linknameCycleTestOptions() Options {
	return Options{targetPlatform: "windows/" + runtime.GOARCH}
}

// TestLinknamePullCycleGuardAnswersWithoutTheConvertSetGraph is the M1 assertion, and it is W1
// itself: with NO convert-set graph — the state every `-tests` and every single-package conversion is
// in — an UPWARD pull must still be refused.
//
// RED PROOF: restore the shortcut this arc removed, i.e. put
//
//	if conversionGraph == nil {
//		return false
//	}
//
// back at the top of linknamePullWouldCycle. This test then reports that the pull is allowed, which
// is exactly the emission that produced the six project cycles.
func TestLinknamePullCycleGuardAnswersWithoutTheConvertSetGraph(t *testing.T) {
	goRoot := testGoRoot(t)

	// currentPackagePath and packageSourceDir are what resetPackageState sets from the loaded
	// package; here they name `runtime`, whose os_windows.go carries the two-argument directive.
	linknameCycleTestState(t, nil, "runtime", filepath.Join(goRoot, "src", "runtime"), map[string]*packages.Package{})

	const target = "internal/syscall/windows"

	if !linknamePullWouldCycle(target, linknameCycleTestOptions()) {
		t.Fatalf("linknamePullWouldCycle(%q) = false while converting \"runtime\" with no convert-set graph.\n"+
			"That emits a forwarding property and queues a ProjectReference runtime -> %s, which Go's own\n"+
			"%s -> syscall -> runtime closes into a project cycle (MSB4006). No conversion ORDER can undo it:\n"+
			"a project-reference graph is a static property of the emitted files.", target, target, target)
	}
}

// TestLinknamePullCycleGuardKeepsDownwardPullsWithoutTheGraph is the other half of the same decision,
// and the reason M2 (refuse every cross-package pull when there is no graph) was rejected as the
// PRIMARY answer: it would trade W1's rare wrong answer for a common one.
//
// Both live pulls in the corpus point DOWNWARD and must survive: math/bits pulls runtime.overflowError
// and runtime.divideError (math/bits/bits_errors.cs), and time's sleep_test pulls
// runtime.haveHighResSleep. Neither target reaches back — `go list -deps runtime` contains neither
// math/bits nor time — so both keep their forwarding property.
//
// RED PROOF: make the M2 fallback the primary, i.e. `return true` immediately after the
// conversionGraph nil check. This test reports the downward pulls suppressed; the corpus loses
// three forwarding properties and math/bits's error values become null fields again.
func TestLinknamePullCycleGuardKeepsDownwardPullsWithoutTheGraph(t *testing.T) {
	goRoot := testGoRoot(t)

	for _, puller := range []string{"math/bits", "time"} {
		t.Run(puller, func(t *testing.T) {
			linknameCycleTestState(t, nil, puller, filepath.Join(goRoot, "src", filepath.FromSlash(puller)), map[string]*packages.Package{})

			if linknamePullWouldCycle("runtime", linknameCycleTestOptions()) {
				t.Fatalf("linknamePullWouldCycle(\"runtime\") = true while converting %q with no convert-set graph.\n"+
					"runtime does not import %s, so this pull points DOWNWARD and is safe; refusing it drops the\n"+
					"forwarding property and restores the null-field bug the feature exists to prevent.", puller, puller)
			}
		})
	}
}

// TestLinknamePullCycleGuardAgreesWithTheConvertSetGraph is the derivation guard: the graph-less
// resolver is not allowed to be a SECOND OPINION. W1 was born from two code paths answering one
// question differently, so the property that matters is not "the new path is right" but "the two
// paths agree" — for both directions of the same pair, which is the only pair the corpus exposes.
//
// The graph here is built the way StdLibConverter builds one (AddPackage, then addImportEdges over
// each node's raw imports), from the real transitive closures, so both oracles are reading the same
// world through different mechanisms.
//
// RED PROOF: change either arm in isolation — e.g. make the graph-less path test
// `closure.Contains(targetPath)` instead of `closure.Contains(currentPackagePath)`, inverting the
// direction — and the two answers stop matching on the upward case.
func TestLinknamePullCycleGuardAgreesWithTheConvertSetGraph(t *testing.T) {
	goRoot := testGoRoot(t)
	options := linknameCycleTestOptions()

	const isw = "internal/syscall/windows"

	// The convert-set: the pair plus everything either of them reaches, so no edge that matters is
	// filtered out by the convert-set edge predicate. Loaded through stdLibLoadConfig — the
	// configuration StdLibConverter itself builds its graph with — rather than a transcription of it.
	loaded, err := packages.Load(
		stdLibLoadConfig(options, packages.NeedName|packages.NeedImports|packages.NeedDeps, filepath.Join(goRoot, "src")),
		isw, "runtime")

	if err != nil {
		t.Skipf("could not load the W1 pair for %s: %v", options.targetPlatform, err)
	}

	graph := NewDependencyGraph()
	closure := map[string]*packages.Package{}

	var collect func(pkg *packages.Package)

	collect = func(pkg *packages.Package) {
		if _, seen := closure[pkg.PkgPath]; seen {
			return
		}

		closure[pkg.PkgPath] = pkg
		graph.AddPackage(pkg.PkgPath, pkg.Dir)

		for _, imported := range pkg.Imports {
			collect(imported)
		}
	}

	for _, pkg := range loaded {
		collect(pkg)
	}

	if !graph.Contains(isw) || !graph.Contains("runtime") {
		t.Skipf("the loaded convert-set is missing one half of the W1 pair (%s / runtime) on %s", isw, options.targetPlatform)
	}

	for path, pkg := range closure {
		imports := make([]string, 0, len(pkg.Imports))

		for importPath := range pkg.Imports {
			imports = append(imports, importPath)
		}

		graph.addImportEdges(path, imports)
	}

	// Both directions of the one pair the corpus exposes: upward (the cycle) and downward (safe).
	for _, probe := range []struct {
		puller string
		target string
	}{
		{puller: "runtime", target: isw},
		{puller: isw, target: "runtime"},
	} {
		linknameCycleTestState(t, graph, probe.puller, filepath.Join(goRoot, "src", "runtime"), map[string]*packages.Package{})
		withGraph := linknamePullWouldCycle(probe.target, options)

		linknameCycleTestState(t, nil, probe.puller, filepath.Join(goRoot, "src", "runtime"), map[string]*packages.Package{})
		withoutGraph := linknamePullWouldCycle(probe.target, options)

		if withGraph != withoutGraph {
			t.Fatalf("the two oracles disagree for %q pulling from %q: convert-set graph says cycle=%v, graph-less resolver says cycle=%v.\n"+
				"That disagreement IS W1 — one question, two answers, and the emission differs by which driver ran.",
				probe.puller, probe.target, withGraph, withoutGraph)
		}
	}
}

// TestLinknamePullCycleGuardRefusesAnUnanswerableTarget pins M2 in the role the design gives it —
// M1's FALLBACK, not its replacement. A target that will not load leaves the cycle question
// unanswered, and an unanswered cycle question must not be answered "no": answering "no" emits a
// reference that may not compile at all, while answering "yes" emits the plain field this converter
// emitted for every such pull before the forwarding feature existed.
//
// RED PROOF: return false instead of true from the `!resolved` arm of linknamePullWouldCycle.
func TestLinknamePullCycleGuardRefusesAnUnanswerableTarget(t *testing.T) {
	goRoot := testGoRoot(t)

	linknameCycleTestState(t, nil, "runtime", filepath.Join(goRoot, "src", "runtime"), map[string]*packages.Package{})

	const target = "go2cs.invalid/no/such/package"

	if !linknamePullWouldCycle(target, linknameCycleTestOptions()) {
		t.Fatalf("linknamePullWouldCycle(%q) = false for a target that cannot be loaded.\n"+
			"An unanswerable cycle question must fail CLOSED: \"no\" emits a cross-assembly reference on\n"+
			"nothing but the absence of evidence.", target)
	}
}

// TestLinknamePullCycleGuardTrustsTheCurrentPackageClosure pins the free arm. If the package under
// conversion already reaches the pull target through its own imports, the target cannot reach back —
// Go's import graph is acyclic by construction, which is the same fact the whole design rests on —
// so the pull is downward and no load is needed.
//
// This is the arm the behavioral corpus rides: LinknameVarPull blank-imports LinknameVarPullLib and
// pulls its `secret`, so CNR's re-transpile of that package answers here rather than through a
// go/packages load of a `replace`d module.
//
// RED PROOF: delete the importedPackages arm. This test still passes through the load path when a
// loader is available, so it asserts the SHORTCUT specifically — a target that could never load.
func TestLinknamePullCycleGuardTrustsTheCurrentPackageClosure(t *testing.T) {
	const target = "go2cs.invalid/provider"

	linknameCycleTestState(t, nil, "go2cs.invalid/consumer", "", map[string]*packages.Package{
		target: {PkgPath: target},
	})

	if linknamePullWouldCycle(target, linknameCycleTestOptions()) {
		t.Fatalf("linknamePullWouldCycle(%q) = true even though the package under conversion already imports it.\n"+
			"An importer cannot be imported back — Go's import graph is acyclic — so this pull is downward\n"+
			"and safe, and refusing it would drop the forwarding property that the LinknameVarPull\n"+
			"behavioral test's golden pins.", target)
	}
}
