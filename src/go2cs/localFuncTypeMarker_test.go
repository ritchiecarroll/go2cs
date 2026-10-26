// localFuncTypeMarker_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the layout of the marker comment a methodless func type declared in a function body emits in place
// of the declaration (visitFuncType): it is laid out as the statement it replaces. It starts its own line,
// never the end of the line before it; Go's blank line before it (and before its doc comment) and after it is
// kept; and it adds none where Go has none. As between statements, in every conversion.

package main

import (
	"strings"
	"testing"
)

const localFuncTypeMarkerSource = `package main

func grouped() int {
	// leads the body.

	// aFunc is documented.
	type aFunc func(x int)

	// bFunc is documented.
	type bFunc func() int

	// after the types
	x := 1
	return x
}

func between() int {
	x := 2
	type cFunc func()
	y := x
	return y
}

func first() int {
	type dFunc func()

	return 3
}

func main() {
	println(grouped(), between(), first())
}
`

const localFuncTypeMarker = " is a methodless func type"

// ownLine reports whether the line holding text starts with it.
func ownLine(t *testing.T, lines []string, text string) bool {
	t.Helper()

	return strings.HasPrefix(strings.TrimSpace(lines[lineContaining(t, lines, text)]), text)
}

func TestLocalFuncTypeMarkerLaidOutAsStatementUnderComments(t *testing.T) {
	isolateConverterPathGlobals(t)
	lines := convertWithComments(t, localFuncTypeMarkerSource)

	for _, name := range []string{"aFunc", "bFunc", "cFunc", "dFunc"} {
		if marker := "// type " + name + localFuncTypeMarker; !ownLine(t, lines, marker) {
			t.Errorf("the marker for %s does not start its own line: %q", name, lines[lineContaining(t, lines, marker)])
		}
	}

	for _, c := range []struct {
		line  string
		blank bool
		why   string
	}{
		{"// leads the body.", false, "no blank line after the opening brace"},
		{"// aFunc is documented.", true, "a blank line between a comment and a local type's doc comment"},
		{"// type aFunc" + localFuncTypeMarker, false, "the doc comment stays on its type"},
		{"// bFunc is documented.", true, "a blank line between two local types"},
		{"// after the types", true, "a blank line after a local type"},
		{"// type cFunc" + localFuncTypeMarker, false, "no blank line in Go"},
		{"nint y = x;", false, "no blank line in Go after a local type"},
		{"// type dFunc" + localFuncTypeMarker, false, "no blank line after the opening brace"},
		{"return 3;", true, "a blank line after a local type"},
	} {
		if got := blankAbove(t, lines, c.line); got != c.blank {
			t.Errorf("%s: blank line above %q = %v, want %v", c.why, c.line, got, c.blank)
		}
	}
}

func TestLocalFuncTypeMarkerLaidOutAsStatementWithoutComments(t *testing.T) {
	isolateConverterPathGlobals(t)
	lines := convertWithoutComments(t, localFuncTypeMarkerSource)

	for _, name := range []string{"aFunc", "bFunc", "cFunc", "dFunc"} {
		if marker := "// type " + name + localFuncTypeMarker; !ownLine(t, lines, marker) {
			t.Errorf("the marker for %s does not start its own line: %q", name, lines[lineContaining(t, lines, marker)])
		}
	}

	for _, c := range []struct {
		line  string
		blank bool
		why   string
	}{
		{"// type aFunc" + localFuncTypeMarker, false, "no blank line after the opening brace"},
		{"// type bFunc" + localFuncTypeMarker, true, "a blank line between two local types"},
		{"nint x = 1;", true, "a blank line after a local type"},
		{"// type cFunc" + localFuncTypeMarker, false, "no blank line in Go"},
		{"nint y = x;", false, "no blank line in Go after a local type"},
		{"return 3;", true, "a blank line after a local type"},
	} {
		if got := blankAbove(t, lines, c.line); got != c.blank {
			t.Errorf("%s: blank line above %q = %v, want %v", c.why, c.line, got, c.blank)
		}
	}
}
