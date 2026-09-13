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
	// A non-generic methodless named func type is rendered AS its base C# delegate everywhere it
	// is referenced (see methodlessNamedFuncSignature), matching Go's free named↔underlying func
	// interconversion — so it emits NO distinct delegate declaration here. Only a marker comment,
	// mirroring the hand-converted-type case; every reference resolves to `Action`/`Func<…>`.
	if _, ok := methodlessNamedFuncSignature(identType); ok {
		// Record the name so its exported-type-alias is NOT emitted — there is no named
		// `<pkg>_package.<Δname>` type to alias to (a consumer's `[GoTypeAlias]` global-using would
		// name a nonexistent type; go/doc's `ast.Filter` → `go.go.ast_package.ΔFilter`, CS0426).
		packageLock.Lock()
		packageInlineFuncTypeNames[name] = true
		packageInlineFuncTypeNames[getCoreSanitizedIdentifier(name)] = true
		packageLock.Unlock()

		if !v.inFunction {
			v.outputBuilder.WriteString(v.newline)
		}

		v.writeOutput("// type %s is a methodless func type — rendered inline as its base delegate", name)
		v.outputBuilder.WriteString(v.newline)
		return
	}

	var resultsSignature, parameterSignature string

	resultsSignature, parameterSignature = v.convFuncType(funcType)

	// The type parameters of a generic defined function type — `type Seq[V any] func(yield
	// func(V) bool)` (Go 1.23 iter) — live on the NAMED type, not the *ast.FuncType's signature,
	// so the generic definition must come from identType (as the struct/array paths do). Deriving
	// it from the signature emitted a NON-generic delegate (`delegate void Seq(Func<V, bool>…)`)
	// whose V was undefined (CS0246) while uses correctly said `Seq<V>` (CS0308).
	typeParams, constraints := v.getGenericDefinition(identType)

	if len(constraints) > 0 {
		constraints = fmt.Sprintf("%s%s%s", constraints, v.newline, v.indent(v.indentLevel))
	}

	v.outputBuilder.WriteString(v.newline)
	v.writeOutputLn("%s delegate %s %s%s(%s)%s;", getAccess(name), resultsSignature, name, typeParams, parameterSignature, constraints)
}
