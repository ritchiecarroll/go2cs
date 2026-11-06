// golauncher_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the count a [GoBoundedGoroutine] ordinal indexes: every goǃ call of the file, as go2cs-gen counts them, an
// invocation whose callee is the bare identifier goǃ, written @goǃ or not. A goǃ inside a comment, a string literal of
// any form or a character literal is not one, nor is a member access x.goǃ(…), a generic goǃ<T>(…) or a method group;
// columns count UTF-16 units.

package golauncher

import (
	"strings"
	"testing"
)

func TestPositionsCountOnlyBareGoLauncherCalls(t *testing.T) {
	text := strings.Join([]string{
		`goǃ(a);`,                                      // 1:1  #0
		`// goǃ(comment);`,                             // a line comment
		`/* goǃ(block); */ goǃ(b);`,                    // 3:19 #1, after a block comment
		`var s = "goǃ(\"quoted\")"; goǃ(c);`,           // 4:28 #2, after a regular string with escapes
		`var v = @"goǃ("""") \"; goǃ(d);`,              // 5:25 #3, after a verbatim string ("" is a quote)
		`var r = """goǃ("x") ""inner"" """;`,           // a raw string holding quotes
		`var i = $"{(x ? "goǃ" : "y")} {z}";`,          // an interpolated string whose hole holds a string
		`var c = '"'; var q = '\''; goǃ(e);`,           // 8:28 #4, after character literals that are quotes
		`@goǃ(f); x.goǃ(g); var m = goǃ; goǃ<int>(k);`, // 9:1 #5, a verbatim identifier (its token starts at @); the rest are not calls of goǃ
		`var w = "𝒳"; goǃ(h);`,                         // 10:15 #6, after a character outside the BMP (two UTF-16 units)
		`var e = $$"""{{goǃ}} {not}""";`,               // a raw interpolated string with a hole
		`// a comment that ends a sentence.`,
		`goǃ(i);`, // 13:1 #7, after a comment ending in a period, which is no member access
	}, "\n")

	got := Positions(text)
	want := []Site{
		{Line: 1, Column: 1, Ordinal: 0},
		{Line: 3, Column: 19, Ordinal: 1},
		{Line: 4, Column: 28, Ordinal: 2},
		{Line: 5, Column: 25, Ordinal: 3},
		{Line: 8, Column: 28, Ordinal: 4},
		{Line: 9, Column: 1, Ordinal: 5},
		{Line: 10, Column: 15, Ordinal: 6},
		{Line: 13, Column: 1, Ordinal: 7},
	}

	if len(got) != len(want) {
		t.Fatalf("Positions found %d goǃ calls, want %d: %v", len(got), len(want), got)
	}

	for i := range want {
		if got[i] != want[i] {
			t.Errorf("launcher %d: got %+v, want %+v", i, got[i], want[i])
		}
	}
}
