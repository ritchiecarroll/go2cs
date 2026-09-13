// addressTakenPredicates_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/ast"
	"go/types"
	"testing"
)

// The escape analysis asks "is this thing's address taken?" in two different senses, and the
// difference is load-bearing rather than accidental:
//
//	ANY form   — `&x`, `&x[i]`, `&x.f` — hands out a pointer INTO x's own storage. An ordinary
//	             value type must be heap-boxed for all three, so the box, the callee's write
//	             through the pointer, and every later read share one slot.
//	DIRECT only — just `&x`. This is what an INHERENTLY-heap type (slice/map/chan/interface/func)
//	             needs, because it is already a reference: `&s[i]` addresses the shared backing
//	             array, which aliases correctly with no box at all.
//
// Reading the direct-only cases as "any" would be invisible — the output still compiles and still
// behaves correctly — while boxing a slice header on every call of the hot helpers that take an
// element address (unicode's is16/is32, slices.Equal, subtle.XORBytes, and 40-odd more). That is a
// silent per-call allocation regression across the corpus, which no compile check would catch, so
// it is pinned here instead.
//
// identAddressTaken is the direct-only sense scoped to the current function declaration, and this
// asserts that equivalence explicitly: it is what licenses the two predicates sharing one walk.

// addressTakenFixture type-checks a function exercising every address-of shape and returns a
// Visitor positioned on it, plus its locals by name.
func addressTakenFixture(t *testing.T) (*Visitor, *ast.FuncDecl, map[string]types.Object) {
	t.Helper()

	dir := t.TempDir()
	writeModuleFiles(t, dir, map[string]string{
		"go.mod": "module example/addresstaken\n\ngo 1.23\n",
		"prod.go": "package addresstaken\n" +
			"type holder struct{ n int }\n" +
			"func f() {\n" +
			"\tvar direct []int\n" +
			"\tvar elem []int\n" +
			"\tvar field holder\n" +
			"\tvar both []int\n" +
			"\tvar none []int\n" +
			"\tvar nested []int\n" +
			"\t_ = &direct\n" +
			"\t_ = &elem[0]\n" +
			"\t_ = &field.n\n" +
			"\t_ = &both\n" +
			"\t_ = &both[0]\n" +
			"\t_ = none\n" +
			"\tgo func() { _ = &nested }()\n" +
			"}\n",
	})

	production := loadProductionForDir(t, dir)

	var funcDecl *ast.FuncDecl

	for _, file := range production.Syntax {
		for _, decl := range file.Decls {
			if fn, ok := decl.(*ast.FuncDecl); ok && fn.Name.Name == "f" {
				funcDecl = fn
			}
		}
	}

	if funcDecl == nil {
		t.Fatal("the fixture function was not found in the loaded syntax")
	}

	locals := map[string]types.Object{}

	ast.Inspect(funcDecl, func(n ast.Node) bool {
		if ident, ok := n.(*ast.Ident); ok {
			if obj, defined := production.TypesInfo.Defs[ident]; defined && obj != nil {
				locals[ident.Name] = obj
			}
		}

		return true
	})

	visitor := &Visitor{info: production.TypesInfo, currentFuncDecl: funcDecl}

	return visitor, funcDecl, locals
}

func TestAddressTakenSensesDivergeOnElementAndFieldForms(t *testing.T) {
	visitor, funcDecl, locals := addressTakenFixture(t)

	cases := []struct {
		local      string
		anyForm    bool // objectAddressTaken with directOnly = false
		directForm bool // objectAddressTaken with directOnly = true, and identAddressTaken
	}{
		{"direct", true, true},
		{"elem", true, false},  // &elem[0] — the divergence
		{"field", true, false}, // &field.n — the divergence
		{"both", true, true},   // the direct form wins even alongside an element form
		{"none", false, false}, // never addressed
		{"nested", true, true}, // inside a function literal: both walks must descend
	}

	for _, tc := range cases {
		obj := locals[tc.local]

		if obj == nil {
			t.Fatalf("fixture local %q was not resolved", tc.local)
		}

		if got := visitor.objectAddressTaken(obj, funcDecl, false); got != tc.anyForm {
			t.Errorf("objectAddressTaken(%s, directOnly=false) = %v, want %v", tc.local, got, tc.anyForm)
		}

		if got := visitor.objectAddressTaken(obj, funcDecl, true); got != tc.directForm {
			t.Errorf("objectAddressTaken(%s, directOnly=true) = %v, want %v", tc.local, got, tc.directForm)
		}

		// The equivalence claim. identAddressTaken is consulted only for inherently-heap types,
		// where directOnly is exactly the right sense, so it must agree with the direct column for
		// every shape — including disagreeing with the "any" column on elem and field.
		if got := visitor.identAddressTaken(obj); got != tc.directForm {
			t.Errorf("identAddressTaken(%s) = %v, want %v (the directOnly sense)", tc.local, got, tc.directForm)
		}
	}
}

// identAddressTaken memoizes by *types.Object and is never reset between functions. That is safe
// only because an object belongs to exactly one declaration, so a stale entry can never be
// consulted while converting a later function — and it must keep answering from the cache rather
// than re-walking, since it runs per identifier occurrence during emission.
func TestIdentAddressTakenMemoizesPerObject(t *testing.T) {
	visitor, _, locals := addressTakenFixture(t)
	obj := locals["direct"]

	if obj == nil {
		t.Fatal("fixture local \"direct\" was not resolved")
	}

	if !visitor.identAddressTaken(obj) {
		t.Fatal("&direct must count as directly addressed")
	}

	cached, found := visitor.identAddressTakenCache[obj]

	if !found || !cached {
		t.Fatal("the answer was not memoized")
	}

	// With no current function declaration a fresh call cannot walk anything, so a second true
	// answer can only have come from the cache.
	visitor.currentFuncDecl = nil

	if visitor.identAddressTaken(obj) {
		t.Fatal("identAddressTaken must return false with no current function, cache or not")
	}
}
