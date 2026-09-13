// dependencyGraph_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"reflect"
	"strings"
	"testing"
)

// TestBuildDependencyGraphAbortsOnUnloadablePackage pins the abort, because the alternative is
// invisible by construction. Skipping an unloadable package drops only its dependency EDGES: it
// stays in the convert set, sorts as if it had no prerequisites, and converts BEFORE the packages
// it imports — reading type-alias metadata that has not been written yet. The output can still
// compile and the summary still reads "100.0%", so nothing but this abort distinguishes that run
// from a good one.
//
// A healthy corpus never reaches it: a full `-stdlib` run reports zero load failures, as does a
// cross-platform one. The error must NAME the package, since a run that aborts without saying which
// package failed is barely better than the warning it replaced.
func TestBuildDependencyGraphAbortsOnUnloadablePackage(t *testing.T) {
	converter := NewStdLibConverter(Options{targetPlatform: "windows/amd64"})
	converter.graph.AddPackage("go2cs.test/no/such/package", t.TempDir())

	err := converter.buildDependencyGraph()

	if err == nil {
		t.Fatal("a package that cannot be loaded was skipped instead of aborting the run")
	}

	if !strings.Contains(err.Error(), "go2cs.test/no/such/package") {
		t.Fatalf("the abort does not name the failed package: %v", err)
	}
}

// buildTestGraph constructs a DependencyGraph from a node set and an import map, mirroring the
// real driver flow (AddPackage for every node, addImportEdges per package, sortAdjacency), so the
// tests exercise exactly the path both converters use to order conversion.
func buildTestGraph(nodes map[string]string, imports map[string][]string) *DependencyGraph {
	g := NewDependencyGraph()

	for path, dir := range nodes {
		g.AddPackage(path, dir)
	}

	for path := range nodes {
		g.addImportEdges(path, imports[path])
	}

	g.sortAdjacency()

	return g
}

// TestTopologicalSortLeavesFirst locks the extracted topological order: dependency-free roots
// first (sorted), then a depth-first visit from the remaining roots in sorted order — a
// deterministic queue regardless of Go's randomized map iteration. This is the property the
// -stdlib driver relies on for byte-reproducible conversion, so the exact order is asserted.
func TestTopologicalSortLeavesFirst(t *testing.T) {
	nodes := map[string]string{
		"app":  "/src/app",
		"lib1": "/src/lib1",
		"lib2": "/src/lib2",
		"base": "/src/base",
		"solo": "/src/solo",
	}

	// Imports deliberately unsorted (app lists lib2 before lib1) to prove sortAdjacency
	// normalizes adjacency into a deterministic visit order.
	imports := map[string][]string{
		"app":  {"lib2", "lib1"},
		"lib1": {"base"},
		"lib2": {"base"},
	}

	g := buildTestGraph(nodes, imports)
	g.topologicalSort()

	want := []string{"base", "solo", "lib1", "lib2", "app"}

	if !reflect.DeepEqual(g.sortedQueue, want) {
		t.Fatalf("topological order = %v, want %v", g.sortedQueue, want)
	}

	// Independent of the exact order, every dependency must precede its dependent in the queue.
	pos := make(map[string]int, len(g.sortedQueue))

	for i, p := range g.sortedQueue {
		pos[p] = i
	}

	for path, pkg := range g.packages {
		for _, dep := range pkg.Dependencies {
			if pos[dep] >= pos[path] {
				t.Errorf("dependency %q (pos %d) does not precede dependent %q (pos %d)", dep, pos[dep], path, pos[path])
			}
		}
	}
}

// TestEdgePredicateIgnoresNonConvertSet verifies the convert-set edge predicate: an import to a
// package that is NOT an added node (e.g. the standard library, when converting a user module
// against a pre-converted stdlib) creates no edge, so it neither appears in the graph nor
// constrains the order.
func TestEdgePredicateIgnoresNonConvertSet(t *testing.T) {
	nodes := map[string]string{
		"app": "/src/app",
		"lib": "/src/lib",
	}
	imports := map[string][]string{
		// "fmt" and "strings" are not in the convert-set — they must be ignored.
		"app": {"lib", "fmt", "strings"},
	}

	g := buildTestGraph(nodes, imports)

	appDeps := g.packages["app"].Dependencies

	if !reflect.DeepEqual(appDeps, []string{"lib"}) {
		t.Fatalf("app dependencies = %v, want [lib] (non-convert-set imports must be dropped)", appDeps)
	}

	if g.Contains("fmt") {
		t.Errorf("fmt must not be a graph node — it was only an (unconverted) import target")
	}

	g.topologicalSort()

	if !reflect.DeepEqual(g.sortedQueue, []string{"lib", "app"}) {
		t.Errorf("order = %v, want [lib app]", g.sortedQueue)
	}
}

// TestVendoredKeyResolution verifies a GOROOT-vendored dependency imported by its source path
// (golang.org/x/…) resolves to its on-disk vendored node key (vendor/golang.org/x/…), so the edge
// is recorded instead of silently dropped (which would let an importer convert before its dep).
func TestVendoredKeyResolution(t *testing.T) {
	nodes := map[string]string{
		"net":                                    "/goroot/src/net",
		"vendor/golang.org/x/net/dns/dnsmessage": "/goroot/src/vendor/golang.org/x/net/dns/dnsmessage",
	}
	imports := map[string][]string{
		// net imports the SOURCE path; the node is keyed by the VENDORED path.
		"net": {"golang.org/x/net/dns/dnsmessage"},
	}

	g := buildTestGraph(nodes, imports)

	netDeps := g.packages["net"].Dependencies
	want := []string{"vendor/golang.org/x/net/dns/dnsmessage"}

	if !reflect.DeepEqual(netDeps, want) {
		t.Fatalf("net dependencies = %v, want %v (vendored source path must resolve to the vendored key)", netDeps, want)
	}
}

// TestCycleTolerated confirms an import cycle neither hangs nor drops nodes: both packages still
// land in the queue (Go permits cycles the converter tolerates with a warning).
func TestCycleTolerated(t *testing.T) {
	nodes := map[string]string{
		"x": "/src/x",
		"y": "/src/y",
	}
	imports := map[string][]string{
		"x": {"y"},
		"y": {"x"},
	}

	g := buildTestGraph(nodes, imports)
	g.topologicalSort()

	if len(g.sortedQueue) != 2 {
		t.Fatalf("cycle queue = %v, want both x and y present", g.sortedQueue)
	}
}
