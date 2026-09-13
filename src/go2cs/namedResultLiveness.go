// namedResultLiveness.go - Gbtc
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

// namedResultNeedsDeclaration reports whether a named RESULT still needs its zero-value local
// declared at function entry (`rune r1 = default!;`).
//
// The converter used to declare every named result unconditionally, because Go's named results ARE
// ordinary addressable locals. But most Go functions name their results for DOCUMENTATION and then
// return explicit values — `func EncodeRune(r rune) (r1, r2 rune)` never mentions r1/r2 again — so
// the declaration is dead on arrival. That one shape was 1,218 of the corpus's 1,219 CS0219
// warnings. Nothing is lost by omitting it: the names survive where a reader actually reads them,
// in the C# tuple return type (`(rune r1, rune r2) EncodeRune(rune r)`), so the emission moves
// CLOSER to the Go source rather than further from it. Omitting the dead ones also PRESERVES
// CS0219 as a live signal — a genuinely dropped assignment to a named result still surfaces, which
// suppressing the code corpus-wide would have destroyed.
//
// `body` is the function's own body; `param` the result's object. The caller passes a nil body when
// liveness must not be consulted at all — the lowerings where GENERATED code, not the Go body,
// reads the locals:
//
//   - namedReturnDeferMode: the declarations sit outside the func() wrapper precisely so deferred
//     closures can mutate them, and the read happens after the wrapper returns.
//   - A GoFrame: its named exit emits a trailing `return <names>;` after the try, reached by
//     `goto ᒐdone` from every return inside.
//
// Everything else is decided here, conservatively — keeping a dead declaration costs one line of
// noise, dropping a live one is CS0103.
func (v *Visitor) namedResultNeedsDeclaration(body *ast.BlockStmt, param *types.Var) bool {
	if body == nil || param == nil {
		return true
	}

	// A heap-box-backed result's box IS the storage its render sites reference; that emission is
	// chosen upstream and liveness never applies to it.
	if v.identHasHeapBox(param, param.Type()) {
		return true
	}

	// Referenced anywhere in the body — read, assigned, address-taken, or captured by a closure.
	// The walk descends into nested function literals deliberately: a capture is a use.
	referenced := false

	ast.Inspect(body, func(n ast.Node) bool {
		if referenced {
			return false
		}

		if ident, ok := n.(*ast.Ident); ok {
			if v.info.Uses[ident] == types.Object(param) || v.info.Defs[ident] == types.Object(param) {
				referenced = true
				return false
			}
		}

		return true
	})

	if referenced {
		return true
	}

	// A NAKED return reads every named result by definition. This walk stops at a nested function
	// literal — a naked return in there returns from the LITERAL, not from this function.
	return bodyHasNakedReturn(body)
}

// bodyHasNakedReturn reports whether the body contains a bare `return` belonging to THIS function.
// Returns inside a nested function literal belong to that literal, so the walk does not enter one.
func bodyHasNakedReturn(body *ast.BlockStmt) bool {
	if body == nil {
		return false
	}

	found := false

	ast.Inspect(body, func(n ast.Node) bool {
		if found {
			return false
		}

		switch stmt := n.(type) {
		case *ast.FuncLit:
			return false
		case *ast.ReturnStmt:
			if len(stmt.Results) == 0 {
				found = true
				return false
			}
		}

		return true
	})

	return found
}
