// shadowedImportAlias_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/types"
	"testing"
)

// A package's OWN class is a member of its own namespace in every compilation, so it shadows a
// same-named import target exactly as a class from the import closure does. github.com/pkg/errors
// (package errors, namespace go.github.com.pkg) imports the standard library's errors as stderrors:
// its production go113.cs emitted `using stderrors = errors_package;`, which C# binds to the
// package's OWN errors_package, so Is called itself (a hang in the Release test host, a stack
// overflow in the Behavioral repro ShadowedStdlibImportAlias). The test files of the same package
// were right only because its external test imports the package, putting the own class in the
// closure by accident.
func TestImportAliasTargetShadowedByOwnClassIsRooted(t *testing.T) {
	stdErrors := types.NewPackage("errors", "errors")

	alias := func(pkg *types.Package, packageNS string, includeSiblingClosure bool) string {
		t.Helper()
		withCleanAliasRenameState(t)
		setShadowState(t, packageNS, nil)
		computeImportAliasRenames(nil, pkg, packageNS, "", "", includeSiblingClosure)
		return rootQualifyIfAmbiguous("errors_package")
	}

	// THE DEFECT: the package is named like the standard-library package it imports.
	self := types.NewPackage("github.com/pkg/errors", "errors")
	self.SetImports([]*types.Package{stdErrors})

	for _, unit := range []bool{false, true} {
		if got := alias(self, "go.github.com.pkg", unit); got != "go.errors_package" {
			t.Fatalf("own class errors_package shadows the import (test unit %v): want %q, got %q", unit, "go.errors_package", got)
		}
	}

	// A SIBLING package of the same module, reached through another sibling: already rooted by the
	// closure walk (measured 2026-10-02); kept here so the two shapes stay one rule.
	siblingErrors := types.NewPackage("example.com/m/errors", "errors")
	util := types.NewPackage("example.com/m/util", "util")
	util.SetImports([]*types.Package{siblingErrors})
	app := types.NewPackage("example.com/m/app", "app")
	app.SetImports([]*types.Package{stdErrors, util})

	if got := alias(app, "go.example.com.m", false); got != "go.errors_package" {
		t.Fatalf("a sibling errors_package in the closure shadows the import: want %q, got %q", "go.errors_package", got)
	}

	// No churn: a package whose own class does not collide keeps the bare target, and so does a
	// root-namespace package (namespace go IS where errors_package lives).
	plain := types.NewPackage("github.com/pkg/wrap", "wrap")
	plain.SetImports([]*types.Package{stdErrors})

	if got := alias(plain, "go.github.com.pkg", false); got != "errors_package" {
		t.Fatalf("no class named errors_package outside the root: want the bare %q, got %q", "errors_package", got)
	}

	rootLog := types.NewPackage("log", "log")
	rootLog.SetImports([]*types.Package{stdErrors})

	if got := alias(rootLog, RootNamespace, false); got != "errors_package" {
		t.Fatalf("a root-namespace package: want the bare %q, got %q", "errors_package", got)
	}
}
