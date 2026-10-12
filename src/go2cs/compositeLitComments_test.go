// compositeLitComments_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards where `-comments` output writes the comments inside a composite literal (class F of
// docs/PLAN-comment-and-spacing-parity.md) and between the arguments of a call that spans lines.
//
// Go labels the rows of a table with a comment at the end of each row, and heads a run of rows with a
// comment on its own line. The converted literal keeps each one where Go has it: a row's comment ends
// that row, with no blank line after it; a heading stays on its own line, inside the literal, above
// its row; a comment after the last element stays above the closing brace. Before, a package-level
// literal's comments were all written above the declaration, and a function-level literal's row
// comments were each followed by a blank line.

package main

import (
	"strings"
	"testing"
)

const compositeLitCommentsSource = `package main

import "fmt"

// table is a package-level literal with a heading and row labels.
var table = [...]int{
	// heading
	1, 2, // first row
	3, 4, // second row
	// before the last row
	5, 6, // last row
}

// steps is a package-level table of one-line funcs, each labeled after its closing brace.
var steps = []func(int) int{
	func(x int) int { return x + 1 }, // add one
	func(x int) int { return x * 2 }, // double
}

type pair struct{ a, b int }

func main() {
	local := []int{
		10, // ten
		20, // twenty
		// before thirty
		30,
		// after the last element
	}
	p := pair{
		a: 1, // field a
		b: 2, // field b
	}
	fmt.Println(table, local, p, steps[0](1),
		len(local), // the length
		cap(local))
}
`

func TestCompositeLitCommentsKeepGoPlacement(t *testing.T) {
	lines := convertWithComments(t, compositeLitCommentsSource)

	// Each row label ends its own row.
	for _, c := range [][2]string{
		{"// first row", "1, 2,"},
		{"// second row", "3, 4,"},
		{"// last row", "5, 6"},
		{"// ten", "10,"},
		{"// twenty", "20,"},
		{"// field a", "a: 1,"},
		{"// field b", "b: 2"},
		{"// the length", "len(local),"},
	} {
		line := strings.TrimSpace(lines[lineContaining(t, lines, c[0])])

		if !strings.HasPrefix(line, c[1]) {
			t.Errorf("%q does not end its row %q: the line reads %q", c[0], c[1], line)
		}

		if blankBelow(lines, c[0]) {
			t.Errorf("a blank line follows %q; Go has none", c[0])
		}
	}

	// A label after a one-line func's closing brace trails the element, not the func's own statement:
	// the func keeps its expression body.
	for _, c := range [][2]string{{"// add one", "x + 1,"}, {"// double", "x * 2 //"}} {
		line := strings.TrimSpace(lines[lineContaining(t, lines, c[0])])

		if !strings.Contains(line, "=> "+c[1]) {
			t.Errorf("%q does not trail its func element `=> %s`: the line reads %q", c[0], c[1], line)
		}
	}

	// A heading stays on its own line, inside the literal, directly above its row.
	for _, c := range [][2]string{
		{"// heading", "1, 2,"},
		{"// before the last row", "5, 6"},
		{"// before thirty", "30"},
	} {
		at := lineContaining(t, lines, c[0])

		if strings.TrimSpace(lines[at]) != c[0] {
			t.Errorf("%q is not on a line of its own: %q", c[0], strings.TrimSpace(lines[at]))
		}

		if next := strings.TrimSpace(lines[at+1]); !strings.HasPrefix(next, c[1]) {
			t.Errorf("%q is not directly above %q: the next line reads %q", c[0], c[1], next)
		}
	}

	// The package-level literal's comments are inside it, below its doc comment.
	if lineContaining(t, lines, "// heading") < lineContaining(t, lines, "table is a package-level literal") {
		t.Errorf("the literal's heading was written above the declaration's doc comment")
	}

	// A comment after the last element stays inside the literal, above its closing brace, and the
	// brace is not commented out.
	at := lineContaining(t, lines, "// after the last element")

	if next := strings.TrimSpace(lines[at+1]); !strings.HasPrefix(next, "}") {
		t.Errorf("the comment after the last element is not directly above the closing brace: the next line reads %q", next)
	}

	if t.Failed() {
		t.Logf("emitted:\n%s", strings.Join(lines, "\n"))
	}
}

// blankBelow reports whether the line after the one containing text is blank.
func blankBelow(lines []string, text string) bool {
	for i, line := range lines {
		if strings.Contains(line, text) {
			return i+1 < len(lines) && strings.TrimSpace(lines[i+1]) == ""
		}
	}

	return false
}
