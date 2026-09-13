// siblingAddressedGlobals_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/types"
	"runtime"
	"slices"
	"testing"
)

// TestSiblingTestAddressedGlobalsAreCollected pins the scan that lets a production emission see the
// address-taking its own in-package `_test.go` half performs (path/filepath's `var LstatP = &lstat`).
// Every negative here is a name the scan must NOT report, because reporting it would heap-box a
// global no pointer aliases.
func TestSiblingTestAddressedGlobalsAreCollected(t *testing.T) {
	dir := t.TempDir()
	writeModuleFiles(t, dir, map[string]string{
		"go.mod": "module example/addressed\n\ngo 1.23\n",
		"prod.go": "package addressed\n" +
			"type box struct{ n int }\n" +
			"var lstat = func(string) int { return 0 }\n" +
			"var held box\n" +
			"var table [4]int\n" +
			"var untouched int\n" +
			"var shadowed int\n" +
			"func Stat(s string) int { return lstat(s) }\n",
		// The export_test.go pattern: package-level address-of, no local bindings in sight.
		"export_test.go": "package addressed\n" +
			"var LstatP = &lstat\n" +
			"var HeldFieldP = &held.n\n" +
			"var TableElemP = &table[2]\n" +
			"var OwnP = &own\n" +
			"var own int\n",
		// Negatives: a same-named LOCAL, a composite literal, and an imported qualifier.
		"local_test.go": "package addressed\n" +
			"import \"testing\"\n" +
			"func TestLocal(t *testing.T) {\n" +
			"\tshadowed := 1\n" +
			"\tp := &shadowed\n" +
			"\tq := &box{n: 2}\n" +
			"\t_, _ = p, q\n" +
			"}\n",
		// Build-excluded: never scanned, so its address-of must not surface.
		"excluded_test.go": "//go:build go2cs_never\n\npackage addressed\nvar UntouchedP = &untouched\n",
		// External tests emit into a different class and reach only the exported surface.
		"outside_test.go": "package addressed_test\nvar external = 1\nvar ExternalP = &external\n",
	})

	signals := collectSiblingTestSignals(dir, "addressed", Options{
		targetPlatform: runtime.GOOS + "/" + runtime.GOARCH,
	})

	if !signals.hasInternalTests {
		t.Fatal("expected build-selected internal test detection")
	}

	// `own` is declared by the test file itself, so it is excluded at the scan; `untouched` sits in a
	// build-excluded file; `shadowed` is a local; `box` is a composite-literal type, not an operand.
	if want := []string{"held", "lstat", "table"}; !slices.Equal(signals.addressedGlobalNames, want) {
		t.Fatalf("addressed sibling globals = %v, want %v", signals.addressedGlobalNames, want)
	}

	// The scan is name-based; the production pass is what resolves the names against the real scope.
	production := loadProductionForDir(t, dir)

	previousGlobals := packageAddressedGlobals
	previousSiblings := siblingTestAddressedGlobalNames
	t.Cleanup(func() {
		packageAddressedGlobals = previousGlobals
		siblingTestAddressedGlobalNames = previousSiblings
	})

	packageAddressedGlobals = map[types.Object]bool{}
	siblingTestAddressedGlobalNames = signals.addressedGlobalNames
	collectAddressedGlobals(nil, production.Types, production.TypesInfo)

	scope := production.Types.Scope()

	for _, name := range []string{"lstat", "held", "table"} {
		varObj, ok := scope.Lookup(name).(*types.Var)
		if !ok {
			t.Fatalf("%s is not a package-level var in the production scope", name)
		}
		if !packageAddressedGlobals[varObj] {
			t.Fatalf("%s must be heap-boxed: its address is taken by the package's own tests", name)
		}
	}

	for _, name := range []string{"untouched", "shadowed"} {
		if varObj, ok := scope.Lookup(name).(*types.Var); ok && packageAddressedGlobals[varObj] {
			t.Fatalf("%s must not be heap-boxed: no pointer aliases it", name)
		}
	}
}

// TestSiblingTestAddressedGlobalsDropUnresolvedNames confirms the seed never invents a global: a
// candidate that resolves to something other than a package-level var of the package under
// conversion (a func, a type, an import qualifier) is discarded rather than marked.
func TestSiblingTestAddressedGlobalsDropUnresolvedNames(t *testing.T) {
	dir := t.TempDir()
	writeModuleFiles(t, dir, map[string]string{
		"go.mod":  "module example/unresolved\n\ngo 1.23\n",
		"prod.go": "package unresolved\ntype T struct{ n int }\nfunc F() int { return 0 }\nvar real int\n",
	})

	production := loadProductionForDir(t, dir)

	previousGlobals := packageAddressedGlobals
	previousSiblings := siblingTestAddressedGlobalNames
	t.Cleanup(func() {
		packageAddressedGlobals = previousGlobals
		siblingTestAddressedGlobalNames = previousSiblings
	})

	packageAddressedGlobals = map[types.Object]bool{}
	siblingTestAddressedGlobalNames = []string{"T", "F", "testing", "nothingAtAll", "real"}
	collectAddressedGlobals(nil, production.Types, production.TypesInfo)

	if len(packageAddressedGlobals) != 1 {
		t.Fatalf("only the package-level var may be marked, got %d entries", len(packageAddressedGlobals))
	}

	varObj, ok := production.Types.Scope().Lookup("real").(*types.Var)
	if !ok || !packageAddressedGlobals[varObj] {
		t.Fatal("the one real package-level var in the candidate list must be marked")
	}
}
