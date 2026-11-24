// visitFuncType.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"fmt"
	"go/ast"
	"go/types"
)

// Handles function types in context of a TypeSpec
func (v *Visitor) visitFuncType(funcType *ast.FuncType, identType types.Type, name string) {

	var resultsSignature, parameterSignature string

	resultsSignature, parameterSignature = v.convFuncType(funcType)

	// The type parameters of a generic defined function type — `type Seq[V any] func(yield
	// func(V) bool)` (Go 1.23 iter) — live on the NAMED type, not the *ast.FuncType's signature,
	// so the generic definition must come from identType (as the struct/array paths do). Deriving
	// it from the signature emitted a NON-generic delegate (`delegate void Seq(Func<V, bool>…)`)
	// whose V was undefined (CS0246) while uses correctly said `Seq<V>` (CS0308).
	typeParams, constraints := v.getGenericDefinition(identType)

	// A func type declared inside a function body is lifted to member level, as every other local type kind is
	// (liftLocalTypeDecl): C# declares no delegate in a method body. Its Go name rides [GoLocalName], so reflection
	// prints `main.aFunc`, not the lifted `main.grouped_aFunc`. A package-level declaration is unchanged.
	localName := v.localNameAttrFor(identType)
	name, target, finish := v.liftLocalTypeDecl(name, identType)

	if len(constraints) > 0 {
		constraints = fmt.Sprintf("%s%s%s", constraints, v.newline, v.indent(v.indentLevel))
	}

	if !v.inFunction {
		target.WriteString(v.newline)
	}

	v.writePendingTypeDoc(target)
	v.writeString(target, "%s%s delegate %s %s%s(%s)%s;", localName, getAccess(name), resultsSignature, name, typeParams, parameterSignature, constraints)
	target.WriteString(v.newline)
	finish()
}
