// dependencyGraph.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// This file holds the shared package dependency graph + topological sort used to order
// conversion "least dependencies first." It is the common core of the two conversion
// drivers: StdLibConverter (convert-set = the Go standard library) and, from P2 on,
// ModuleConverter (convert-set = an end-user app + its third-party libraries). Each driver
// discovers its own node set and each node's raw imports; the graph filters edges to the
// convert-set, sorts adjacency deterministically, and produces the ordered conversion queue.
package main

import (
	"log"
	"sort"
)

// Package represents a Go package as a node in a DependencyGraph, with the edges to the other
// convert-set packages it depends on (and, inversely, that depend on it).
type Package struct {
	Path         string   // Import path
	Dir          string   // Directory path
	Dependencies []string // Dependencies (import paths) that are themselves in the convert-set
	Dependents   []string // Convert-set packages that depend on this one
	Processed    bool     // Whether package has been processed
}

// DependencyGraph holds a convert-set of packages and their intra-set dependency edges, and
// produces a topologically sorted conversion order (least dependencies first). The convert-set
// IS the edge predicate: addImportEdges only records an edge to a dependency that is itself an
// added node, so a dependency outside the set (e.g. the standard library, when converting a
// user module that references a pre-converted stdlib) never constrains the order.
//
// Conversion order matters for byte-reproducible output: a converted importer must observe its
// dependency's finished package_info.cs (the source of imported collision-rename aliases like
// `bidiꓸClass`) before it is converted, so leaves must precede the packages that import them.
type DependencyGraph struct {
	packages    map[string]*Package // Map of package paths to Package objects (the convert-set)
	sortedQueue []string            // Topologically sorted queue (populated by topologicalSort)
}

// NewDependencyGraph creates an empty dependency graph.
func NewDependencyGraph() *DependencyGraph {
	return &DependencyGraph{packages: make(map[string]*Package)}
}

// AddPackage registers (or replaces) a convert-set node for the given import path and source
// directory. Only added packages participate in edges — see the DependencyGraph doc.
func (g *DependencyGraph) AddPackage(path, dir string) {
	g.packages[path] = &Package{
		Path:         path,
		Dir:          dir,
		Dependencies: make([]string, 0),
		Dependents:   make([]string, 0),
		Processed:    false,
	}
}

// Contains reports whether an import path is a node in the convert-set.
func (g *DependencyGraph) Contains(path string) bool {
	_, exists := g.packages[path]
	return exists
}

// DependsOn reports whether package `from` transitively depends on package `to` within the
// convert-set (following the intra-set Dependencies edges). Used to detect a linkname pull that
// would add a CYCLIC C# project reference — `from` is the linkname target, `to` the pulling package;
// if the target already depends on the puller, referencing it back cannot compile (a project-ref
// cycle), so the pull keeps its null-field form instead of a forwarding property.
func (g *DependencyGraph) DependsOn(from, to string) bool {
	seen := map[string]bool{}
	stack := []string{from}

	for len(stack) > 0 {
		path := stack[len(stack)-1]
		stack = stack[:len(stack)-1]

		if seen[path] {
			continue
		}

		seen[path] = true

		node, ok := g.packages[path]

		if !ok {
			continue
		}

		for _, dep := range node.Dependencies {
			if dep == to {
				return true
			}

			stack = append(stack, dep)
		}
	}

	return false
}

// addImportEdges wires intra-convert-set dependency edges for one package: for each of pkgPath's
// raw import paths it resolves a GOROOT-vendored key, then records an edge ONLY when the resolved
// dependency is itself a node in the graph. Dependencies/Dependents are deduped here and sorted
// later by sortAdjacency, so the order the imports arrive in is immaterial.
func (g *DependencyGraph) addImportEdges(pkgPath string, imports []string) {
	pkg, ok := g.packages[pkgPath]

	if !ok {
		return
	}

	for _, importPath := range imports {
		// A GOROOT-vendored dependency is imported by its source path (golang.org/x/…) but keyed
		// in the graph by its on-disk path (vendor/golang.org/x/…). Resolve to the vendored key —
		// otherwise the edge is silently dropped (the source path is not a node) and the
		// topological order can convert an importer BEFORE its dependency, whose package_info.cs
		// (the source of imported collision-rename aliases like `bidiꓸClass`) does not exist yet at
		// that point, so the importer emits the un-renamed form.
		if _, exists := g.packages[importPath]; !exists {
			if _, vendored := g.packages["vendor/"+importPath]; vendored {
				importPath = "vendor/" + importPath
			}
		}

		// Only include dependencies that are part of this conversion set.
		if depPkg, isConversionTarget := g.packages[importPath]; isConversionTarget {
			// Add dependency if not already added
			if !containsString(pkg.Dependencies, importPath) {
				pkg.Dependencies = append(pkg.Dependencies, importPath)
			}

			// Add this package as a dependent to the dependency
			if !containsString(depPkg.Dependents, pkgPath) {
				depPkg.Dependents = append(depPkg.Dependents, pkgPath)
			}
		}
	}
}

// sortAdjacency sorts every node's Dependencies and Dependents for deterministic behavior, so
// the topological order and any dependency-derived reporting are stable run-to-run.
func (g *DependencyGraph) sortAdjacency() {
	for _, pkg := range g.packages {
		sort.Strings(pkg.Dependencies)
		sort.Strings(pkg.Dependents)
	}
}

// topologicalSort populates sortedQueue with the convert-set ordered least dependencies first.
// It is deterministic: no-dependency roots go first (sorted), then a depth-first visit from the
// remaining roots in sorted order — map-iteration roots would flip the queue, and thus any
// order-sensitive output, run-to-run. Go's import cycles are tolerated with a warning.
func (g *DependencyGraph) topologicalSort() {
	// Create a copy of the package map for sorting
	unprocessed := make(map[string]*Package)

	for path, pkg := range g.packages {
		unprocessed[path] = &Package{
			Path:         pkg.Path,
			Dependencies: pkg.Dependencies,
			Processed:    false,
		}
	}

	// Create the sorted queue
	g.sortedQueue = make([]string, 0, len(g.packages))

	// First handle packages with no dependencies
	var noDeps []string

	for path, pkg := range unprocessed {
		if len(pkg.Dependencies) == 0 && !pkg.Processed {
			noDeps = append(noDeps, path)
		}
	}

	// Sort for deterministic order
	sort.Strings(noDeps)

	// Process packages with no dependencies first
	for _, path := range noDeps {
		if !unprocessed[path].Processed {
			unprocessed[path].Processed = true
			g.sortedQueue = append(g.sortedQueue, path)
		}
	}

	// Keep track of packages being processed (to detect cycles)
	processing := make(map[string]bool)

	// Then recursively process remaining packages — from SORTED roots, so packages not ordered
	// relative to each other by a dependency edge still land in the same queue position every run
	// (map-iteration roots made the queue, and thus any order-sensitive output, flip run-to-run).
	remaining := make([]string, 0, len(unprocessed))

	for path := range unprocessed {
		remaining = append(remaining, path)
	}

	sort.Strings(remaining)

	for _, path := range remaining {
		if !unprocessed[path].Processed {
			g.visitPackage(path, unprocessed, processing)
		}
	}
}

// visitPackage visits a package during topological sort, emitting its dependencies before it.
func (g *DependencyGraph) visitPackage(path string, unprocessed map[string]*Package, processing map[string]bool) {
	// Check for cycles
	if processing[path] {
		// We found a cycle, but it might be okay for Go packages. Just log it and continue.
		log.Printf("WARNING: Cycle detected involving package %s", path)
		return
	}

	// Mark as being processed
	processing[path] = true

	// Process dependencies first
	for _, depPath := range unprocessed[path].Dependencies {
		if dep, exists := unprocessed[depPath]; exists && !dep.Processed {
			g.visitPackage(depPath, unprocessed, processing)
		}
	}

	// Mark as processed and add to the queue
	unprocessed[path].Processed = true
	g.sortedQueue = append(g.sortedQueue, path)

	// Unmark as being processed
	processing[path] = false
}

// containsString checks if a string slice contains a specific string.
func containsString(slice []string, str string) bool {
	for _, s := range slice {
		if s == str {
			return true
		}
	}

	return false
}
