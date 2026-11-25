// operandOrderCovers_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the operand-order walk's reach into `&` and map literals, and the fail-safe behind it
// (operandOrder.go). A call the walk does not record stays in place while a later call moves ahead
// of it, which runs two calls in the opposite order to Go; the fail-safe keeps C#'s order for any
// statement holding such a call before its last hoist.

package main

import (
	"go/ast"
	"testing"
)

// operandOrderCoversFixture type-checks one function whose statements are the shapes under test and
// returns a Visitor over it and the call of each expression statement, in source order.
func operandOrderCoversFixture(t *testing.T) (*Visitor, []*ast.CallExpr) {
	t.Helper()

	dir := t.TempDir()
	writeModuleFiles(t, dir, map[string]string{
		"go.mod": "module example/operandorder\n\ngo 1.23\n",
		"prod.go": "package operandorder\n" +
			"import \"unsafe\"\n" +
			"type T struct{ x int }\n" +
			"var g int\n" +
			"var arr [4]int\n" +
			"func f(tag string) int { g++; return g }\n" +
			"func h3(a int, t *T, b int) {}\n" +
			"func he(a int, e *int, b int) {}\n" +
			"func hm(a, b int) {}\n" +
			"func use() {\n" +
			"\th3(g, &T{x: f(\"lit\")}, f(\"last\"))\n" +
			"\the(g, &arr[f(\"idx\")%4], f(\"last\"))\n" +
			"\thm(g+map[string]int{\"k\": f(\"m1\")}[\"k\"], f(\"m2\"))\n" +
			"\thm(g, map[string]int{\"k\": g}[\"k\"]+f(\"r\"))\n" +
			"\th3(0, &T{x: g}, f(\"a\"))\n" +
			"\th3(0, &T{x: f(\"only\")}, 0)\n" +
			"\thm(g+len(\"abc\")+len(arr)+int(unsafe.Sizeof(g)), f(\"c\"))\n" +
			"}\n",
	})

	production := loadProductionForDir(t, dir)

	var calls []*ast.CallExpr

	for _, file := range production.Syntax {
		for _, decl := range file.Decls {
			if fn, ok := decl.(*ast.FuncDecl); ok && fn.Name.Name == "use" {
				for _, stmt := range fn.Body.List {
					calls = append(calls, stmt.(*ast.ExprStmt).X.(*ast.CallExpr))
				}
			}
		}
	}

	if len(calls) != 7 {
		t.Fatalf("the fixture's seven statements were not found: %d", len(calls))
	}

	return &Visitor{info: production.TypesInfo}, calls
}

// hoistTags names each hoist by its call's string argument, or by its node kind.
func hoistTags(hoists []ast.Expr) []string {
	var tags []string

	for _, hoist := range hoists {
		switch h := hoist.(type) {
		case *ast.CallExpr:
			if len(h.Args) == 1 {
				if lit, ok := h.Args[0].(*ast.BasicLit); ok {
					tags = append(tags, lit.Value)
					continue
				}
			}

			tags = append(tags, "call")
		case *ast.CompositeLit:
			tags = append(tags, "map")
		default:
			tags = append(tags, "?")
		}
	}

	return tags
}

func TestOperandOrderWalksAddressAndMapOperands(t *testing.T) {
	visitor, calls := operandOrderCoversFixture(t)

	want := [][]string{
		{`"lit"`, `"last"`}, // a call in &T{...} runs before the later call
		{`"idx"`, `"last"`}, // a call locating &arr[...] runs before the later call
		{"map", `"m2"`},     // a map literal holding a call is one ordered unit
		{"map", `"r"`},      // a map literal reading g in place moves whole, as an early form
		{`"a"`},             // g inside &T{...} is read after f, as gc reads it
		nil,                 // control: no call follows a read, nothing moves
		{`"c"`},             // a constant call (len of a string or an array, unsafe.Sizeof) runs nothing and moves nothing
	}

	for i, call := range calls {
		got := hoistTags(visitor.operandOrderHoists(nil, false, []ast.Expr{call}))

		if len(got) != len(want[i]) {
			t.Fatalf("statement %d: hoists %v, want %v", i+1, got, want[i])
		}

		for j := range got {
			if got[j] != want[i][j] {
				t.Fatalf("statement %d: hoists %v, want %v", i+1, got, want[i])
			}
		}
	}
}

func TestOperandOrderFailSafeKeepsCSharpOrder(t *testing.T) {
	visitor, calls := operandOrderCoversFixture(t)

	// h3(g, &T{x: f("lit")}, f("last")), with an event list that has not seen f("lit").
	statement := calls[0]
	lit := statement.Args[1].(*ast.UnaryExpr).X.(*ast.CompositeLit).Elts[0].(*ast.KeyValueExpr).Value
	last := statement.Args[2]

	// The walk records the enclosing call too, after its operands; it is never hoisted.
	outer := operandOrderEvent{node: statement, kind: operandOrderCall, ancestors: map[ast.Expr]bool{}}
	events := []operandOrderEvent{{node: last, kind: operandOrderCall, ancestors: map[ast.Expr]bool{}}, outer}

	if operandOrderCovers(visitor, events, []ast.Expr{last}, nil, []ast.Expr{statement}) {
		t.Fatal("a call before the last hoist that the walk did not record must keep C#'s order")
	}

	events = append([]operandOrderEvent{{node: lit, kind: operandOrderCall, ancestors: map[ast.Expr]bool{}}}, events...)

	if !operandOrderCovers(visitor, events, []ast.Expr{lit, last}, nil, []ast.Expr{statement}) {
		t.Fatal("a statement whose every earlier call is an event must keep its hoists")
	}

	// A call inside a hoisted unit is covered by the unit, recorded or not.
	unit := []operandOrderEvent{{node: statement, kind: operandOrderCall, ancestors: map[ast.Expr]bool{}}}

	if !operandOrderCovers(visitor, unit, []ast.Expr{statement}, nil, []ast.Expr{statement}) {
		t.Fatal("a call inside a hoisted unit must count as covered")
	}

	// A call after the last hoist cannot be overtaken, so it never trips the fail-safe.
	if !operandOrderCovers(visitor, []operandOrderEvent{events[0], outer}, []ast.Expr{lit}, nil, []ast.Expr{statement}) {
		t.Fatal("a call after the last hoist must not trip the fail-safe")
	}

}
