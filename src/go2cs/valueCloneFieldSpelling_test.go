// valueCloneFieldSpelling_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/token"
	"go/types"
	"strings"
	"testing"
)

// The [GoValueClone(…)] stamp NAMES MEMBERS, and go2cs-gen resolves every name against the emitted
// struct's declarations to generate Clone(). So the stamp's spelling has exactly one correct value:
// whatever visitStructType DECLARED. These guards pin the two transforms that decide it, one in each
// direction, because each was invisible until something reached it:
//
//	(a) a field whose name is in nameCollisions must NOT be Δ-prefixed. That collision branch exists
//	    for a package-level TYPE colliding with a METHOD; a field is MEMBER scope. Stamping it
//	    Δ-prefixed while the declaration emitted the bare name is CS1061 at the generated Clone()
//	    (measured on 1.24 runtime2.cs: three `trace` fields, stamp `Δtrace`, declaration `trace`).
//	(b) a field whose name equals its enclosing type IS renamed in the declaration (CS0542), so a
//	    stamp spelling it bare names a member that does not exist. Untriggered in today's corpus —
//	    the mirror of (a), and the reason both are pinned here rather than only the one that fired.
//
// Both arms compare against the DECLARATION's own helpers rather than against a literal, so a future
// change to how a field is declared moves the expectation with it instead of leaving a stale pin.
func TestValueCloneStampSpellsFieldsAsDeclared(t *testing.T) {
	arrayField := func(name string) *types.Var {
		return types.NewField(token.NoPos, nil, name, types.NewArray(types.Typ[types.Byte], 4), false)
	}

	t.Run("package-level collision does not reach a member name", func(t *testing.T) {
		prev := nameCollisions
		nameCollisions = map[string]bool{"trace": true}
		defer func() { nameCollisions = prev }()

		// Guard the guard: if the collision branch stopped Δ-prefixing at all, this test would pass
		// for the wrong reason and pin nothing. Assert the branch is still live before relying on it.
		if getSanitizedIdentifier("trace") == getCoreSanitizedIdentifier("trace") {
			t.Fatal("control failed: getSanitizedIdentifier no longer renames a collision, so this guard proves nothing")
		}

		got := structValueCloneFields(types.NewStruct([]*types.Var{arrayField("trace")}, nil), "g")
		want := getCoreSanitizedIdentifier("trace")

		if len(got) != 1 || got[0] != want {
			t.Fatalf("stamp spelled %v, declaration spells %q -- go2cs-gen resolves the stamp against the declaration", got, want)
		}
	})

	t.Run("field named like its enclosing type is renamed as the declaration renames it", func(t *testing.T) {
		const structTypeName = "holder"

		got := structValueCloneFields(types.NewStruct([]*types.Var{arrayField(structTypeName)}, nil), structTypeName)
		want := typeCollidingFieldName(getCoreSanitizedIdentifier(structTypeName))

		if len(got) != 1 || got[0] != want {
			t.Fatalf("stamp spelled %v, declaration spells %q (CS0542 rename)", got, want)
		}

		// The rename must actually have changed something, or the arm is vacuous.
		if want == getCoreSanitizedIdentifier(structTypeName) {
			t.Fatalf("control failed: typeCollidingFieldName is a no-op for %q, so this arm pins nothing", structTypeName)
		}
	})

	t.Run("an ordinary field is untouched by either transform", func(t *testing.T) {
		got := structValueCloneFields(types.NewStruct([]*types.Var{arrayField("buf")}, nil), "holder")

		if len(got) != 1 || got[0] != "buf" {
			t.Fatalf("stamp spelled %v, want [buf] -- neither transform applies here", got)
		}

		if strings.Contains(got[0], ShadowVarMarker) {
			t.Fatalf("stamp %q carries the disambiguation marker on a name that needs none", got[0])
		}
	})
}
