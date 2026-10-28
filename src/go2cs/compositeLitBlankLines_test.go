// compositeLitBlankLines_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards Go's blank lines between the elements of a multi-line composite literal (and of a multi-line call's
// arguments, which share the element list). The converted literal keeps each one where Go has it: between two
// elements, inside a nested literal, and after a comment that leads an element. It adds none: no blank line where
// Go has none, none after the opening brace. As between statements, the blank lines are kept in every conversion;
// without `-comments` a line a comment covers is not blank, so a blank above an omitted comment is still kept. A
// comment trailing a nested literal's last element is written by the enclosing list, and adds no blank line there.

package main

import "testing"

const compositeLitBlankLinesSource = `package main

type pair struct {
	a, b int
}

type outer struct {
	name  string
	inner pair
	list  []int
}

func flat() []int {
	return []int{
		11,
		22,

		33,
		44,
	}
}

func keyed() outer {
	return outer{
		name: "x",

		inner: pair{
			a: 1,

			b: 2,
		},

		// the list
		list: []int{5, 6},
	}
}

func commented() map[string]int {
	return map[string]int{
		"one": 1,
		// two follows

		"two":   2,
		"three": 3,
	}
}

func trailing() [][]int {
	return [][]int{
		{
			7,
			8, // ends the first row
		},
		{9},
	}
}

func main() {
	println(len(flat()), keyed().name, len(commented()), len(trailing()))
}
`

func TestCompositeLitBlankLinesKeptUnderComments(t *testing.T) {
	isolateConverterPathGlobals(t)
	lines := convertWithComments(t, compositeLitBlankLinesSource)

	for _, c := range []struct {
		line  string
		blank bool
		why   string
	}{
		{"11,", false, "no blank line after the opening brace"},
		{"22,", false, "no blank line in Go"},
		{"33,", true, "a blank line between two elements"},
		{"44", false, "no blank line in Go"},
		{`name: "x"`, false, "no blank line after the opening parenthesis"},
		{"inner: new pair(", true, "a blank line between two keyed fields"},
		{"a: 1,", false, "no blank line after a nested literal's opening parenthesis"},
		{"b: 2", true, "a blank line between two fields of a nested literal"},
		{"// the list", true, "a blank line before a comment that leads a field"},
		{"list: slice<nint>([5, 6])", false, "a leading comment stays on its field"},
		{`["one"] = 1,`, false, "no blank line after the opening brace"},
		{"// two follows", false, "no blank line in Go"},
		{`["two"] = 2,`, true, "a blank line between a comment and the element after it"},
		{`["three"] = 3`, false, "no blank line in Go"},
		{"// ends the first row", false, "no blank line in Go"},
		{"slice<nint>([9])", false, "a comment trailing a nested literal's last element adds no blank line"},
	} {
		if got := blankAbove(t, lines, c.line); got != c.blank {
			t.Errorf("%s: blank line above %q = %v, want %v", c.why, c.line, got, c.blank)
		}
	}
}

func TestCompositeLitBlankLinesKeptWithoutComments(t *testing.T) {
	isolateConverterPathGlobals(t)
	lines := convertWithoutComments(t, compositeLitBlankLinesSource)

	for _, c := range []struct {
		line  string
		blank bool
		why   string
	}{
		{"33,", true, "a blank line between two elements"},
		{"44", false, "no blank line in Go"},
		{"inner: new pair(", true, "a blank line between two keyed fields"},
		{"b: 2", true, "a blank line between two fields of a nested literal"},
		{"list: slice<nint>([5, 6])", true, "a blank line above an omitted comment is still a blank line"},
		{`["two"] = 2,`, true, "a blank line after an omitted comment"},
		{`["three"] = 3`, false, "no blank line in Go"},
		{"slice<nint>([9])", false, "no blank line in Go"},
	} {
		if got := blankAbove(t, lines, c.line); got != c.blank {
			t.Errorf("%s: blank line above %q = %v, want %v", c.why, c.line, got, c.blank)
		}
	}
}
