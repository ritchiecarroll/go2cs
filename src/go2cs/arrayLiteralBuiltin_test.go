// arrayLiteralBuiltin_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"strings"
	"testing"
)

// An unkeyed array literal of an unnamed (or alias) array type renders through golib's array builtin over a C#
// collection expression, the way a slice literal does (Tour visual-parity item 4); every other shape keeps its form.
const arrayLiteralBuiltinSource = `package main

import "fmt"

type digits [3]int

type triple = [3]int

type cell struct {
	marks [2]bool
}

var primes = [6]int{2, 3, 5, 7, 11, 13}

func main() {
	short := [8]byte{1, 2}
	dots := [...]string{"a", "b"}
	var t triple = triple{4, 5, 6}
	d := digits{7, 8, 9}
	keyed := [5]int{1: 10, 3: 30}
	empty := [4]int{}
	grid := [2][3]int{{1, 2, 3}, {4}}
	ptrs := []*[4]byte{{1}}
	cells := [3]cell{{marks: [2]bool{true, false}}}
	fmt.Println(primes, short, dots, t, d, keyed, empty, grid, ptrs, cells)
}
`

func TestArrayLiteralTakesTheArrayBuiltin(t *testing.T) {
	isolateConverterPathGlobals(t)
	emitted := strings.Join(convertWithComments(t, arrayLiteralBuiltinSource), "\n")

	for _, want := range []string{
		"primes = array<nint>([2, 3, 5, 7, 11, 13]);",                            // full length
		"var @short = array<byte>(8, [1, 2]);",                                   // short: the declared length
		`var dots = array<@string>(["a", "b"]);`,                                 // an ellipsis literal
		"triple t = array<nint>([4, 5, 6]);",                                     // an alias
		"var d = new digits(new nint[]{7, 8, 9}.array());",                       // a named array keeps its constructor
		"var keyed = new array<nint>(5){[1] = 10, [3] = 30};",                    // keyed
		"var empty = new nint[]{}.array(4);",                                     // empty
		"new array<nint>[]{array<nint>([1, 2, 3]), array<nint>(3, [4])}.array()", // outer kept, inner converted
		"[Ꮡ(array<byte>(4, [1]))]",                                               // elided behind a pointer
		"array<bool>([true, false])",                                             // a field's literal
		"}.array(3, () => new());",                                               // a factory element keeps its form
	} {
		if !strings.Contains(emitted, want) {
			t.Errorf("missing %q", want)
		}
	}

	if t.Failed() {
		t.Logf("emitted:\n%s", emitted)
	}
}
