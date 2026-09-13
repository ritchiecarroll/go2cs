// convStructType.go - Gbtc
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

func (v *Visitor) convStructType(structType *ast.StructType, context IdentContext) string {
	var name string
	var identType types.Type

	// An empty struct (`struct{}`) is never lifted to a dynamic named type — it maps to the
	// shared golib `EmptyStruct`. Lifting it would also be unsound here: when this StructType is
	// the value of a `struct{}{}` composite assigned to a map element (`seen[tp] = struct{}{}`),
	// the enclosing assignment passes the LHS ident (`seen`) as context, so visitStructType would
	// both name the lift after the variable (`<func>_seen`) AND register the variable's own type
	// (the *map*) in liftedTypeMap — poisoning every later reference to that map type (its
	// parameter signature, indexers, comma-ok). extractStructType already excludes empty structs
	// from lifting everywhere else; mirror that here.
	if isEmptyStruct(structType) {
		return convertToCSTypeName("struct{}")
	}

	t := v.getType(structType, false)

	if liftedName, ok := v.liftedNameFor(t); ok {
		return liftedName
	}

	if context.ident == nil {
		name = "type"
	} else {
		if len(context.ident.Name) > 0 {
			name = context.ident.Name
		}

		identType = v.getIdentType(context.ident)
	}

	return v.visitStructType(structType, identType, name, nil, true, nil)
}
