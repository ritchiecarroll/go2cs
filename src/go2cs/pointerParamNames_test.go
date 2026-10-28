// pointerParamNames_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import "testing"

// A pointer parameter with no deref alias keeps its Go name (owner ruling 2026-10-27, R5): once the
// function's text is complete, renameBoxToken renames its box token there. These pin what the rename
// takes and what it must leave: a whole token only, never a field accessor after a '.', never a longer
// identifier, and the target name as given (a keyword stays escaped).
func TestRenameBoxTokenTakesWholeTokensOnly(t *testing.T) {
	tests := []struct {
		name, text, box, to, want string
	}{
		{"signature and uses", "f(ж<T> Ꮡp) {\n    return Ꮡp == nil || g(Ꮡp);\n}", "Ꮡp", "p", "f(ж<T> p) {\n    return p == nil || g(p);\n}"},
		{"member access on the box", "Ꮡp.Inc(); defer(Ꮡp.Inc, ref ᒐ);", "Ꮡp", "p", "p.Inc(); defer(p.Inc, ref ᒐ);"},
		{"a field accessor named like the parameter", "x.of(T.Ꮡp).Value = Ꮡp;", "Ꮡp", "p", "x.of(T.Ꮡp).Value = p;"},
		{"a longer identifier is another token", "var Ꮡpʗ1 = Ꮡp; ᏑpΔ1 = Ꮡpp;", "Ꮡp", "p", "var Ꮡpʗ1 = p; ᏑpΔ1 = Ꮡpp;"},
		{"a named-argument label is the field's, not the box", "new T(p: Ꮡp)", "Ꮡp", "p", "new T(p: p)"},
		{"a keyword-named parameter stays escaped", "f(ж<T> Ꮡnew) { return Ꮡnew; }", "Ꮡnew", "@new", "f(ж<T> @new) { return @new; }"},
		{"at the ends of the text", "Ꮡp", "Ꮡp", "p", "p"},
	}

	for _, test := range tests {
		t.Run(test.name, func(t *testing.T) {
			if got := renameBoxToken(test.text, test.box, test.to); got != test.want {
				t.Fatalf("renameBoxToken(%q, %q, %q)\n got %q\nwant %q", test.text, test.box, test.to, got, test.want)
			}
		})
	}
}
