// visitMapType.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/ast"
	"go/types"
)

// Handles map types in context of a TypeSpec
func (v *Visitor) visitMapType(mapType *ast.MapType, identType types.Type, name string) {
	// A defined map type — `type Grades map[string]int` — emits the `[GoType("map[K, V]")]
	// partial struct` forward declaration whose Map template go2cs-gen implements (the
	// generator's attribute format is comma-separated `map[K, V]`, not Go's `map[K]V`). The
	// old stub emitted only a comment, leaving the type undeclared (CS0246).
	keyType := convertToCSTypeName(v.getAliasQualifiedTypeName(v.info.TypeOf(mapType.Key), false))
	valueType := convertToCSTypeName(v.getAliasQualifiedTypeName(v.info.TypeOf(mapType.Value), false))
	access := v.consumePendingTypeAccess()

	// A map type declared inside a function body (`type M map[int]bool`, maps_test.go) cannot be a
	// method-body statement in C#; hoist it to member level (see liftLocalTypeDecl). A package-level
	// declaration is unaffected — target is v.outputBuilder and finish() is a no-op.
	preLiftName := name
	name, target, finish := v.liftLocalTypeDecl(name, identType)

	// Key/value were resolved ABOVE, i.e. before this declaration's own hoist registered its
	// lifted name — so a SELF-referential local type (`type recursiveMap map[string]recursiveMap`,
	// gob's encoder_test.go) rendered the source name, which no longer exists at member level
	// (CS0246 in the generated map partial). Re-resolve through liftedTypeMap whenever a hoist
	// actually renamed the declaration; a package-level declaration never renames, so its
	// emission is unchanged.
	if name != preLiftName {
		if keyGoType := v.info.TypeOf(mapType.Key); keyGoType != nil {
			if lifted, ok := v.liftedTypeMap[keyGoType]; ok {
				keyType = lifted
			}
		}

		if valueGoType := v.info.TypeOf(mapType.Value); valueGoType != nil {
			if lifted, ok := v.liftedTypeMap[valueGoType]; ok {
				valueType = lifted
			}
		}
	}

	if !v.inFunction {
		target.WriteString(v.newline)
	}

	v.recordTypeAccessibility("struct", getSanitizedIdentifier(name), "", access, "")
	v.writeStringLn(target, "%s[GoType(\"map[%s, %s]\")] %spartial struct %s;", v.localNameAttrFor(identType), keyType, valueType, access, getSanitizedIdentifier(name))
	finish()
}
