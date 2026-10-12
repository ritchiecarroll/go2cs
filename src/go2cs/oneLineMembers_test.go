// oneLineMembers_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards adjacent one-line functions (class K of docs/PLAN-comment-and-spacing-parity.md; the owner's decision 5):
// functions Go writes on one line, one after another, become one-line C# members written one after another — an
// expression body when the C# body is one expression, `{ }` when it is empty. A body C# needs more lines for keeps
// them, and a one-line function Go writes on its own is unchanged. The rule holds in every conversion.

package main

import (
	"strings"
	"testing"
)

const oneLineMembersSource = `package main

import "fmt"

type T struct{ n int }

func (t T) Len() int     { return t.n }
func (t *T) Add(k int)   { t.n += k }
func (T) marker()        {}
// Twice doubles the count.
func (t T) Twice() int   { return 2 * t.n }
func (t T) Show() string { return fmt.Sprint(t.n) }

func alone() int { return 1 }

func multi() int { a := alone(); return a + 1 }
func three() int { return 3 }

func main() {
	t := &T{n: 1}
	t.Add(2)
	t.marker()
	fmt.Println(t.Len(), t.Twice(), t.Show(), alone(), multi(), three())
}
`

func checkOneLineMembers(t *testing.T, lines []string) {
	t.Helper()

	for _, c := range []struct{ name, line string }{
		{"Len(", "=> t.n;"},
		{"Add(", "=> t.n += k;"},
		{"marker(", "{ }"},
		{"Twice(", "=> 2 * t.n;"},
		{"Show(", "=> fmt.Sprint(t.n);"},
		{"nint three()", "=> 3;"},
	} {
		at := lineContaining(t, lines, " "+c.name)

		if !strings.HasSuffix(strings.TrimSpace(lines[at]), c.line) {
			t.Errorf("%s is not one line ending %q: %q", c.name, c.line, lines[at])
		}
	}

	// Adjacent in Go, adjacent in C#; a doc comment stays directly above its member.
	for _, name := range []string{" Add(", " marker(", "// Twice doubles the count."} {
		if blankAbove(t, lines, name) {
			t.Errorf("a blank line above %q, which Go writes adjacently", name)
		}
	}

	if at := lineContaining(t, lines, " Twice("); !strings.Contains(lines[at-1], "// Twice doubles the count.") {
		t.Errorf("the doc comment is not directly above Twice: %q", lines[at-1])
	}

	// A one-line function Go writes on its own keeps its three lines.
	if at := lineContaining(t, lines, "nint alone()"); !strings.HasSuffix(lines[at], "{") || !blankAbove(t, lines, "nint alone()") {
		t.Errorf("alone() changed: %q", lines[at])
	}

	// A body C# needs more lines for keeps them, and the blank lines either side of it.
	if at := lineContaining(t, lines, "nint multi()"); !strings.HasSuffix(lines[at], "{") {
		t.Errorf("multi() collapsed: %q", lines[at])
	}

	if !blankAbove(t, lines, "nint three()") {
		t.Errorf("no blank line between multi()'s body and three()")
	}
}

func TestAdjacentOneLineMembersUnderComments(t *testing.T) {
	checkOneLineMembers(t, convertWithComments(t, oneLineMembersSource))
}

func TestAdjacentOneLineMembersWithoutComments(t *testing.T) {
	lines := convertWithoutComments(t, oneLineMembersSource)

	for _, c := range []struct{ name, line string }{
		{" Len(", "=> t.n;"},
		{" marker(", "{ }"},
		{" Twice(", "=> 2 * t.n;"},
	} {
		if at := lineContaining(t, lines, c.name); !strings.HasSuffix(strings.TrimSpace(lines[at]), c.line) {
			t.Errorf("%s is not one line ending %q without -comments: %q", c.name, c.line, lines[at])
		}
	}

	if blankAbove(t, lines, " Add(") {
		t.Errorf("a blank line above Add() without -comments")
	}
}
