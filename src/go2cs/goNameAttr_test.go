// goNameAttr_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import "testing"

// The [GoName] stamp carries a declaration's Go name where golib's projection, which removes one leading Δ from
// the emitted name, would not give it: a Go name that starts with Δ, or a rename that adds more than one marker.
// Every other name, renamed or not, carries no stamp (D4: only those names change emission).
func TestGoNameAttrStampsOnlyWhatTheTrimMisnames(t *testing.T) {
	for _, row := range []struct {
		goName, emitted, want string
	}{
		{"t", "t", ""},
		{"Δt", "Δt", `[GoName("Δt")] `},
		{"Δu", "ΔΔu", `[GoName("Δu")] `},           // a Δ-led field named like its Δ-led struct
		{"B", "ΔB", ""},                            // an ordinary rename: the trim gives B
		{"slice", "Δslice", ""},                    // a golib reserved name
		{"Main", "ΔMain", ""},                      // the entry-point rename
		{"file", "@file", ""},                      // a keyword escape is not a rename
		{"file", "ΔΔfile", `[GoName("file")] `},    // typeCollidingFieldName's keyword family (net's file)
		{"union", "ΔΔunion", `[GoName("union")] `}, // a field named like its renamed struct
		{"slice", "Δsliceᴛ", `[GoName("slice")] `}, // getCollisionAvoidanceIdentifier's reserved branch
		{"slice", "ΔΔslice", `[GoName("slice")] `}, // a reserved field named like its struct
	} {
		if got := goNameAttr(row.goName, row.emitted); got != row.want {
			t.Errorf("goNameAttr(%q, %q) = %q, want %q", row.goName, row.emitted, got, row.want)
		}
	}
}
