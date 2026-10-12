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

	if t.Failed() {
		t.Logf("emitted:\n%s", strings.Join(lines, "\n"))
	}
}
