// addressOfParenOperand_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"bytes"
	"go/ast"
	"go/printer"
	"go/token"
	"testing"
)

// The loader's ParseFile removes the parentheses an address-of operand carries, and the parentheses around
// each base on the operand's selector and index path, so every address-of rule and every address-taken
// analysis sees the one form Go means. A conversion or a dereference keeps its parentheses: there they are
// part of the expression, not around it. The behavioral project AddressOfParenthesizedOperand proves the
// converted program behaves as Go does; this holds the parse-time form.
func TestAddressOfOperandParenthesesAreRemovedAtParse(t *testing.T) {
	const source = `package p

type mutex struct{ owner int32 }
type point struct{ X, Y int }

func f(p *mutex, pt point, pp *point, arr [3]int, s []point, n int) {
	_ = &((*mutex)(p).owner)
	_ = &(((*mutex)(p)).owner)
	_ = &((pt).X)
	_ = &(pt.Y)
	_ = &((arr[1]))
	_ = &((arr)[2])
	_ = &(n)
	_ = &((*pp).X)
	_ = &(((s)[0]).Y)
	_ = &(pp.Y)
}
`
	want := []string{
		"&(*mutex)(p).owner",
		"&((*mutex)(p)).owner",
		"&pt.X",
		"&pt.Y",
		"&arr[1]",
		"&arr[2]",
		"&n",
		"&(*pp).X",
		"&s[0].Y",
		"&pp.Y",
	}

	fset := token.NewFileSet()
	file, err := parseFileUnparenAddressOperands(fset, "p.go", []byte(source))

	if err != nil {
		t.Fatal(err)
	}

	var got []string

	ast.Inspect(file, func(n ast.Node) bool {
		if unary, ok := n.(*ast.UnaryExpr); ok && unary.Op == token.AND {
			var text bytes.Buffer

			if err := printer.Fprint(&text, fset, unary); err != nil {
				t.Fatal(err)
			}

			got = append(got, text.String())
		}

		return true
	})

	if len(got) != len(want) {
		t.Fatalf("got %d address-of operands, want %d: %q", len(got), len(want), got)
	}

	for i := range want {
		if got[i] != want[i] {
			t.Errorf("operand %d: got %q, want %q", i, got[i], want[i])
		}
	}
}
