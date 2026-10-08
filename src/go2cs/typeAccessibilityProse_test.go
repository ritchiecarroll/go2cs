// typeAccessibilityProse_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"strings"
	"testing"
)

// A persisted package_info.cs whose TypeAccessibility prose opens with the CURRENT first line but carries an
// earlier wording below it (the face lift's "`[GoType]` declarations" -> "Go type declarations") converges on
// the current prose; a block already current is left byte-identical. Without the second anchor the section's
// prose migrated only from its ORIGINAL first line, so a reconversion left every existing file's wording stale.
func TestTypeAccessibilityProseConvergesOnTheCurrentWording(t *testing.T) {
	prose := typeAccessibilityProseLines()
	open := typeAccessibilityIndent + "// <" + TypeAccessibilitySection + ">"
	close := typeAccessibilityIndent + "// </" + TypeAccessibilitySection + ">"

	stale := append([]string{}, prose...)
	stale[1] = typeAccessibilityIndent + "// `[GoType]` declarations in this package's converted sources are deliberately"

	file := func(block []string) []string {
		lines := []string{"namespace go;", "", "public static partial class p_package", "{"}
		lines = append(lines, block...)
		lines = append(lines, "", open, typeAccessibilityIndent+"internal partial struct t {}", close, "}")
		return lines
	}

	migrated := ensureTypeAccessibilitySection(file(stale))

	if got, want := strings.Join(migrated, "\n"), strings.Join(file(prose), "\n"); got != want {
		t.Errorf("stale prose did not converge on the current wording:\n%s\nwant:\n%s", got, want)
	}

	current := file(prose)

	if got := ensureTypeAccessibilitySection(append([]string{}, current...)); strings.Join(got, "\n") != strings.Join(current, "\n") {
		t.Errorf("current prose was not left byte-identical:\n%s", strings.Join(got, "\n"))
	}
}
