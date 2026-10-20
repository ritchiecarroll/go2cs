// preludeSeparator_test.go - Gbtc
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

// A function's converter prelude (named-result locals, variadic slices, array clones, pointer derefs, heap
// boxes for escaping parameters) is set apart from the Go body by exactly one blank line, and has none inside
// it, however many of those parts the function needs (docs/PLAN-comment-and-spacing-parity.md, class E2; the
// owner's decision 2).
const preludeSeparatorSource = `package main

import "fmt"

type T struct{ v int }

func one(xs ...int) (n int) {
	n = len(xs)
	return
}

func sum(a [4]int, xs ...int) (n int) {
	// first body comment
	for _, x := range xs {
		n += x
	}
	return n + a[0]
}

func esc(a [2]int, x int) (p *int, n int) {
	n = a[1]
	p = &x
	return
}

func (t T) val(x int) *int {
	return &x
}

func (t *T) get(a [2]int) (n int) {
	defer func() { n++ }()
	n = t.v + a[0]
	return
}

func rec(a [2]int, x int) (p *int, err error) {
	defer func() {
		if r := recover(); r != nil {
			err = fmt.Errorf("%v", r)
		}
	}()
	p = &x
	return
}

func main() {
	t := &T{1}
	fmt.Println(one(1), sum([4]int{1}, 1, 2), t.get([2]int{}))
	p, n := esc([2]int{}, 3)
	q, _ := rec([2]int{}, 4)
	fmt.Println(*p, n, *q, *t.val(2))
}
`

func TestPreludeSetApartByOneBlankLine(t *testing.T) {
	isolateConverterPathGlobals(t)
	lines := convertWithComments(t, preludeSeparatorSource)

	for _, c := range []struct {
		header  string // the method's signature line
		open    string // the line the prelude follows: the signature itself, or the frame's `try {`
		prelude int    // non-blank prelude lines
		first   string // the first line of the Go body
	}{
		{"/*n*/ one(", "/*n*/ one(", 2, "n = len(xs);"},
		{"/*n*/ sum(", "/*n*/ sum(", 3, "// first body comment"},
		{" esc(/*[2]*/", " esc(/*[2]*/", 4, "n = a[1];"},
		{" val(this ", " val(this ", 1, "return Ꮡx;"},
		{" get(this ", "try {", 2, "defer(() => {"},
		{" rec(/*[2]*/", "try {", 2, "defer(() => {"},
	} {
		at := lineContaining(t, lines, c.header)

		for !strings.Contains(lines[at], c.open) {
			at++
		}

		first := at + 1

		for first < len(lines) && strings.TrimSpace(lines[first]) != c.first {
			first++
		}

		if first == len(lines) {
			t.Fatalf("%s: the body's first line %q is not emitted", c.header, c.first)
		}

		var blanks []int

		for i := at + 1; i < first; i++ {
			if strings.TrimSpace(lines[i]) == "" {
				blanks = append(blanks, i-at)
			}
		}

		body := strings.Join(lines[at:first+1], "\n")

		if len(blanks) != 1 || blanks[0] != first-at-1 {
			t.Errorf("%s: want one blank line, directly above the body's first line; blank lines at %v of\n%s", c.header, blanks, body)
		}

		if got := first - at - 1 - len(blanks); got != c.prelude {
			t.Errorf("%s: %d prelude lines, want %d:\n%s", c.header, got, c.prelude, body)
		}
	}
}
