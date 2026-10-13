// trailingCommentAlignment_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the alignment of trailing comments (class J of docs/PLAN-comment-and-spacing-parity.md; the owner's
// decision 4): the comments Go aligned are aligned in C# with gofmt's rule, one column after the block's longest code,
// and no other comment moves; a lone trailing comment keeps one space.

package main

import (
	"strings"
	"testing"
	"unicode/utf8"
)

const trailingAlignmentSource = `package main

import "fmt"

const (
	small  = 1      // one
	medium = 1 << 8 // two
	big    = 'x'    // three
)

type pair struct {
	a  int     // first
	bb []byte  // second
	c  float64 // third
}

var lone = 4 // only

func main() {
	x := 1   // set x
	x += 2   // add
	y := "s" // a string
	fmt.Println(x, y, small, medium, big, lone, pair{})
}
`

// commentColumn answers where the trailing comment holding text starts (in runes) and the width of the code before it.
func commentColumn(t *testing.T, lines []string, text string) (int, int) {
	t.Helper()

	line := lines[lineContaining(t, lines, text)]
	at := strings.Index(line, "//")

	return utf8.RuneCountInString(line[:at]), utf8.RuneCountInString(strings.TrimRight(line[:at], " "))
}

func TestGoAlignedTrailingCommentsAlign(t *testing.T) {
	lines := convertWithComments(t, trailingAlignmentSource)

	for _, run := range [][]string{
		{"// one", "// two", "// three"},
		{"// first", "// second", "// third"},
		{"// set x", "// add", "// a string"},
	} {
		longest, columns := 0, map[int]bool{}

		for _, text := range run {
			column, width := commentColumn(t, lines, text)
			columns[column] = true
			longest = max(longest, width)
		}

		if len(columns) != 1 {
			for _, text := range run {
				t.Logf("%q", lines[lineContaining(t, lines, text)])
			}
			t.Errorf("run %v: comments at %d columns, want one", run, len(columns))
			continue
		}

		for column := range columns {
			if column != longest+1 {
				t.Errorf("run %v: comments at column %d, want the longest code + 1 = %d", run, column, longest+1)
			}
		}
	}

	if column, width := commentColumn(t, lines, "// only"); column != width+1 {
		t.Errorf("a lone trailing comment has %d spaces before it, want 1", column-width)
	}
}
