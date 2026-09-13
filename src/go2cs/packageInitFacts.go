// packageInitFacts.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/ast"

	"golang.org/x/tools/go/packages"
)

// packageInitFacts memoizes, per Go import path, whether that package initializes anything at run
// time TRANSITIVELY — the fact writeImportInit's trigger is decided by.
//
// It is reset per conversion (resetPackageState) rather than kept for the whole run, because the
// handles it is computed from are: each converted package is loaded on its own, so a dependency
// appears as a DIFFERENT *packages.Package in every conversion that reaches it, and a multi-target
// `-platforms` emission re-loads the whole graph under different build constraints — where the
// answer can legitimately differ, since which files a package is built from is what decides
// whether it has an `init` at all. Within one conversion the memo is what keeps the walk linear:
// each package in the import closure is examined once however many importers reach it.
var packageInitFacts map[string]bool

// packageInitializes reports whether the Go package p performs any initialization of its own at
// run time. Go's own two sources, and only those:
//
//  1. a `func init()` declaration — any number per package, and any one of them is enough; and
//  2. a package-level variable whose initialization expression must be EVALUATED, which go/types
//     already answers as Info.InitOrder (its dependency-sorted list of package-level initializers).
//
// A constant right-hand side (`var maxLen = 1 << 20`) is excluded: it converts to an ordinary C#
// static field initializer, which runs with the TYPE rather than with the module, so no module
// constructor could force it and forcing one would say nothing about it. Everything else in
// InitOrder — a call, a composite literal, a reference to another package's variable — is
// initialization in exactly the sense Go's ordering rule is about.
//
// The UNKNOWN case — no handle, or a handle carrying no syntax — never reaches here: the caller
// answers it, and answers it the other way.
func packageInitializes(p *packages.Package) bool {
	for _, file := range p.Syntax {
		for _, decl := range file.Decls {
			funcDecl, isFunc := decl.(*ast.FuncDecl)

			// A Go `init` is a function, never a method, and is the one name a package may declare
			// repeatedly. A bodyless declaration is an assembly/cgo implementation, which the
			// converted assembly cannot run either way.
			if isFunc && funcDecl.Recv == nil && funcDecl.Name != nil && funcDecl.Name.Name == "init" && funcDecl.Body != nil {
				return true
			}
		}
	}

	if p.TypesInfo == nil {
		return false
	}

	for _, initializer := range p.TypesInfo.InitOrder {
		if typeAndValue, known := p.TypesInfo.Types[initializer.Rhs]; !known || typeAndValue.Value == nil {
			return true
		}
	}

	return false
}

// packageInitializesTransitively reports whether the package at importPath, or ANY package in its
// import closure, initializes something at run time.
//
// Transitivity is what makes the fact usable as an emission trigger. Forcing an importer's module
// constructor runs the forcing hooks that constructor itself carries, so the closure is walked by
// the RUNTIME one link at a time; the converter only needs to know whether walking it would reach
// anything. A package that initializes nothing itself but imports one that does must therefore
// still be forced — it is the path to the initialization, and skipping it would strand the whole
// subtree.
//
// An UNKNOWN package — one this conversion's loader handed no handle for — answers TRUE. Forcing a
// module constructor that turns out to be empty is a guaranteed no-op; skipping one that is not
// loses Go's ordering silently, and silently is how this defect lived in the corpus for months. So
// the trigger fails toward fidelity, never toward the smaller emission.
func packageInitializesTransitively(importPath string) bool {
	packageLock.Lock()
	defer packageLock.Unlock()

	return resolvePackageInitializes(importPath, map[string]bool{})
}

// resolvePackageInitializes is packageInitializesTransitively's memoized recursion. Callers hold
// packageLock.
func resolvePackageInitializes(importPath string, visiting map[string]bool) bool {
	if answer, memoized := packageInitFacts[importPath]; memoized {
		return answer
	}

	// The pseudo-packages the language gives no initialization at all. Answering here rather than
	// leaving it to the handle lookup keeps `unsafe` — which loads with type information but no
	// syntax — from being read as a package whose initialization is merely unknown.
	if noInitPseudoPackages.Contains(importPath) {
		return false
	}

	pkg, loaded := importedPackages[importPath]

	// No handle at all, or a handle with no SYNTAX — which under the LoadAllSyntax mode every
	// conversion path uses can only mean the loader gave this run nothing to read (the discovery
	// mode `-recurse=module` uses for its closure scan is the shape to watch: it deliberately omits
	// syntax and types, and a predicate that read `false` off such a handle would silently
	// under-force every dependency in the graph). Answer yes rather than guess.
	if !loaded || pkg == nil || len(pkg.Syntax) == 0 {
		return true
	}

	if visiting[importPath] {
		// Go forbids an import cycle, so a well-formed graph never reaches this; a malformed one
		// must not spin the converter. Reporting false here cannot lose an initialization: the
		// package is already on the stack, and whichever frame opened it answers for the cycle.
		return false
	}

	visiting[importPath] = true
	initializes := packageInitializes(pkg)

	if !initializes {
		for dependencyPath := range pkg.Imports {
			if resolvePackageInitializes(dependencyPath, visiting) {
				initializes = true
				break
			}
		}
	}

	delete(visiting, importPath)
	packageInitFacts[importPath] = initializes

	return initializes
}
