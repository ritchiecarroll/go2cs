// aliasDocComments_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the doc of a type that C# declares as a `global using` line (class C of
// docs/PLAN-comment-and-spacing-parity.md, the owner's decision 6): a Go alias, and a defined type over an
// interface, which takes the same form. The using line is the only place C# allows the declaration, so the doc
// goes directly above it, under `-comments`.

package main

import (
	"strings"
	"testing"
)

const aliasDocSource = `package main

import "fmt"

// Celsius is an alias for a float, kept for the old API.
type Celsius = float64

// Shape is anything, held as an interface.
//
// It is a defined type over the empty interface.
type Shape any

func main() {
	var c Celsius = 21.5
	var s Shape = c
	fmt.Println(c, s)
}
`

func TestAliasDocAboveItsGlobalUsing(t *testing.T) {
	isolateConverterPathGlobals(t)
	lines := convertWithComments(t, aliasDocSource)

	for _, c := range []struct {
		using string
		doc   []string
	}{
		{"global using Celsius =", []string{"// Celsius is an alias for a float, kept for the old API."}},
		{"global using Shape =", []string{"// Shape is anything, held as an interface.", "//", "// It is a defined type over the empty interface."}},
	} {
		at := lineContaining(t, lines, c.using)

		if at < len(c.doc) {
			t.Fatalf("%q is too close to the top of the file for its doc", c.using)
		}

		for i, want := range c.doc {
			if got := strings.TrimSpace(lines[at-len(c.doc)+i]); got != want {
				t.Errorf("above %q, line %d of its doc reads %q, want %q", c.using, i+1, got, want)
			}
		}
	}

	// With nothing above the using block, the file does not open with a blank line.
	if strings.TrimSpace(lines[0]) == "" {
		t.Errorf("the file opens with a blank line")
	}

	if t.Failed() {
		t.Logf("emitted:\n%s", strings.Join(lines, "\n"))
	}
}

const aliasDocUnderHeaderSource = `// Copyright 2026 The Example Authors.
// The header's last line.

package main

import "fmt"

// Celsius is an alias for a float.
type Celsius = float64

func main() {
	var c Celsius = 21.5
	fmt.Println(c)
}
`

// The using block sits directly under the file's leading comments; an alias's doc is set apart from them by the
// blank line Go has above the declaration, so it does not read as the end of the file's header.
func TestAliasDocSetApartFromTheFileHeader(t *testing.T) {
	isolateConverterPathGlobals(t)
	lines := convertWithComments(t, aliasDocUnderHeaderSource)

	if !blankAbove(t, lines, "// Celsius is an alias for a float.") {
		t.Errorf("no blank line between the file's header and the alias's doc")
		t.Logf("emitted:\n%s", strings.Join(lines, "\n"))
	}
}

const aliasOfStructDocSource = `// Copyright 2026 The Example Authors.

// Package main is a sample.
package main

import "fmt"

// Point is an alias of an anonymous struct, whose doc the lifted struct takes.
type Point = struct{ X, Y int }

func main() {
	fmt.Println(Point{1, 2})
}
`

// An alias of an anonymous struct lifts the struct, which takes the doc; the using line then has no doc of its own
// to write and must not leave a bare blank line where it would have gone.
func TestAliasOfStructLeavesNoBareBlankLine(t *testing.T) {
	isolateConverterPathGlobals(t)
	lines := convertWithComments(t, aliasOfStructDocSource)

	for i := 1; i < len(lines); i++ {
		if strings.TrimSpace(lines[i]) == "" && strings.TrimSpace(lines[i-1]) == "" {
			t.Errorf("two blank lines in a row at line %d", i+1)
			t.Logf("emitted:\n%s", strings.Join(lines, "\n"))
			break
		}
	}

	if got := strings.Count(strings.Join(lines, "\n"), "Point is an alias of an anonymous struct"); got != 1 {
		t.Errorf("the alias's doc is written %d times, want 1", got)
	}
}
