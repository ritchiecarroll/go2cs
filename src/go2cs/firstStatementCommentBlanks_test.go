// firstStatementCommentBlanks_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards Go's blank lines among the comments that lead a body's FIRST statement in `-comments` output.
// Between later statements writeStatementGap keeps them (bodyBlankLines_test.go); the first statement's
// comments are flushed apart from it, by visitBlockStmt and, for a case body, writeLeadingBodyComments,
// and both used to write the groups back to back: Go's blank line after the last group and Go's blank
// line between two groups were dropped, in function bodies, nested blocks, function literals and case
// bodies alike. The comment census reads each such site as body:go-blank-cs-none.

package main

import "testing"

const firstStatementCommentBlanksSource = `package main

func opens() int {
	// opening comment

	x := 1
	return x
}

func opensGroups() int {
	// first opening group

	// second opening group
	x2 := 2
	return x2
}

func adjacent() int {
	// no blank line after
	w := 5
	return w
}

func nested(c bool) int {
	if c {
		// nested comment

		y := 3
		return y
	}
	return 0
}

func caseBody(n int) int {
	switch n {
	case 1:
		// first case group

		// second case group

		z := 4
		return z
	}
	return 0
}

func lit() func() int {
	return func() int {
		// literal comment

		v := 6
		return v
	}
}

func main() {
	println(opens(), opensGroups(), adjacent(), nested(true), caseBody(1), lit()())
}
`

func TestFirstStatementCommentBlanksKept(t *testing.T) {
	isolateConverterPathGlobals(t)
	lines := convertWithComments(t, firstStatementCommentBlanksSource)

	for _, c := range []struct {
		line  string
		blank bool
		why   string
	}{
		{"// opening comment", false, "no blank line directly after the opening brace"},
		{"nint x = 1;", true, "a blank line between a function's opening comment and its first statement"},
		{"// first opening group", false, "no blank line directly after the opening brace"},
		{"// second opening group", true, "a blank line between two opening comment groups"},
		{"nint x2 = 2;", false, "a comment directly above the first statement stays on it"},
		{"nint w = 5;", false, "no blank line in Go"},
		{"nint y = 3;", true, "a blank line between a nested block's opening comment and its first statement"},
		{"// second case group", true, "a blank line between two comment groups leading a case body"},
		{"nint z = 4;", true, "a blank line between a case body's comments and its first statement"},
		{"nint v = 6;", true, "a blank line between a function literal's opening comment and its first statement"},
	} {
		if got := blankAbove(t, lines, c.line); got != c.blank {
			t.Errorf("%s: blank line above %q = %v, want %v", c.why, c.line, got, c.blank)
		}
	}
}
