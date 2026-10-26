// rangeNilPointerArray_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the index-only range over a pointer to an array whose operand is the DEREFERENCED value -- a pointer
// parameter (its ref-lowered form or its DerefOrNull entry alias) or a pointer receiver: `for i := range p`,
// `for i, _ := range p` and `for range p` emit the count range `range(N)` and never read p, so a NIL pointer ranges
// N times as Go's spec has it (with at most one iteration variable and a constant len, the operand is not
// evaluated). A pointer to a named array type takes the same form. The controls keep their emission: a box-form
// local ranges `range(lp, N)`, a range that names a value reads the pointee, and a slice parameter ranges itself.

package main

import (
	"path/filepath"
	"strings"
	"testing"
)

func TestRangeOverPointerArrayValueIsCountRange(t *testing.T) {
	isolateConverterPathGlobals(t)

	root := t.TempDir()

	writeModuleFile(t, filepath.Join(root, "app", "go.mod"), "module example.com/fieldaccess\n\ngo 1.24\n")
	writeModuleFile(t, filepath.Join(root, "app", "main.go"), `package main

import "fmt"

type bits [4]uint64

type arr [5]byte

// lowered: every use of p is a range, so the parameter lowers to a ref.
func lowered(p *[3]int) (n int) {
	for i := range p {
		n += i
	}
	return
}

// aliased: p also escapes to a call, so it keeps its box and an entry alias.
func aliased(p *[6]int) (n int) {
	keep(p)
	for i, _ := range p {
		n += i
	}
	return
}

func keep(p *[6]int) { fmt.Println(p == nil) }

func named(b *bits) (n int) {
	for range b {
		n++
	}
	return
}

func (p *arr) count() (n int) {
	for i := range p {
		n += i
	}
	return
}

func controls(s []int) (n int) {
	var lp *[7]int
	for i := range lp {
		n += i
	}
	q := new([2]int)
	for i, v := range q {
		n += i + v
	}
	for i := range s {
		n += i
	}
	return
}

func main() {
	var a *arr
	fmt.Println(lowered(nil), aliased(nil), named(nil), a.count(), controls(nil))
}
`)

	cs := readGenerated(t, filepath.Join(convertFieldAccessModule(t, root), "main.cs"))

	for _, want := range []struct{ fn, form string }{
		{"lowered(", "in range(3))"},
		{"aliased(", "in range(6))"},
		{"named(", "in range(4))"},
		{"count(", "in range(5))"},
	} {
		if body := funcBody(t, cs, want.fn); !strings.Contains(body, want.form) {
			t.Errorf("an index-only range over a dereferenced *[N]T in %s does not take the count range (want %q):\n%s", want.fn, want.form, body)
		}
	}

	body := funcBody(t, cs, "controls(")

	for _, kept := range []string{"in range(lp, 7))", "in q.Value)", "in range(s))"} {
		if !strings.Contains(body, kept) {
			t.Errorf("a control left its emission (want %q):\n%s", kept, body)
		}
	}
}
