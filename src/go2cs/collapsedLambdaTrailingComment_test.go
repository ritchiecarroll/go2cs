// collapsedLambdaTrailingComment_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards a single-return func literal whose return carries a TRAILING comment under `-comments` (go-cmp's
// cmp/options.go, `return t.AssignableTo(errorIface) // Never true`, COORD 2026-10-03). The literal collapsed to
// an expression body with the comment still attached, so the comment ran to the end of the line and commented out
// the rest of the enclosing call (`wrap((nint n) => n < limit; // below the limit, "x"u8);`, CS1026). Such a
// literal keeps its block body; the control is a comment-free literal that still collapses.

package main

import (
	"strings"
	"testing"
)

func TestASingleReturnLiteralWithATrailingCommentKeepsItsBlockBody(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	lines := convertWithComments(t, `package main

import "fmt"

func wrap(f func(int) bool, tag string) func(int) bool { return f }

func main() {
	limit := 3
	f := wrap(func(n int) bool {
		return n < limit // below the limit
	}, "x")
	h := wrap(func(n int) bool { return n == limit }, "z")
	fmt.Println(f(1), h(3))
}
`)

	emitted := strings.Join(lines, "\n")

	// The comment ends its own line inside the kept block, and the call's remaining argument follows the block.
	for _, want := range []string{
		"var f = wrap((nint n) => {\n",
		"        return n < limit; // below the limit\n",
		"    }, \"x\"u8);",
	} {
		if !strings.Contains(emitted, want) {
			t.Errorf("want %q: a trailing comment must not end inside the enclosing call:\n%s", want, emitted)
		}
	}

	// CONTROL: a literal with no comment still collapses to an expression body.
	if want := `var h = wrap((nint n) => n == limit, "z"u8);`; !strings.Contains(emitted, want) {
		t.Errorf("control: want %q unchanged:\n%s", want, emitted)
	}
}
