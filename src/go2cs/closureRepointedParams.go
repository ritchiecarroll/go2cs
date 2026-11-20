// closureRepointedParams.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/ast"
	"go/token"
	"go/types"
)

// closureRepointedPointerParams returns the pointer parameters (and a direct-ж pointer receiver) of
// funcDecl that a func literal anywhere in its body ASSIGNS — `n = k` inside a closure, a deferred
// literal or a go-statement literal.
//
// Such a parameter cannot take the usual entry alias `ref var n = ref Ꮡn.DerefOrNull();`. Inside the
// literal the assignment repoints the box (`Ꮡn = k;`), because a C# closure cannot capture the ref
// local and so cannot re-alias it (CS8175); every later read of the alias in the enclosing body then
// still reads the OLD target. That is a silent wrong read, and an infinite loop when the body walks a
// structure until the closure moves it: `for len(n.kids) > 0 { n.each(func(k *node) bool { n = k;
// return true }) }`. Go has one variable, so the parameter keeps its box and every value read goes
// through it, in the body and in the literal alike: the box is the variable. With no alias the box
// takes the Go name, as for a parameter whose alias is dead (visitFuncDecl), so the emission reads
// `ж<node> n`, `n.DerefOrNull().kids` and `n = k`.
//
// A rebinding in the body itself is not in this set. The body's own assignment re-aliases in the
// same statement (`Ꮡn = n.kids[0]; n = ref Ꮡn.DerefOrNull();`, see visitAssignStmt), which keeps the
// alias current at every point the body can observe it.
//
// A ref-LOWERED parameter never reaches this question: any reference from inside a closure vetoes
// the lowering (refVetoX2Capture), and so does any assignment to it (refVetoX4Repoint).
func closureRepointedPointerParams(funcDecl *ast.FuncDecl, info *types.Info) map[types.Object]bool {
	if funcDecl == nil || funcDecl.Body == nil || info == nil {
		return nil
	}

	candidates := map[types.Object]bool{}

	addFields := func(fields *ast.FieldList) {
		if fields == nil {
			return
		}

		for _, field := range fields.List {
			for _, name := range field.Names {
				if name.Name == "" || name.Name == "_" {
					continue
				}

				if obj := info.Defs[name]; obj != nil {
					if _, isPtr := obj.Type().Underlying().(*types.Pointer); isPtr {
						candidates[obj] = true
					}
				}
			}
		}
	}

	addFields(funcDecl.Recv)
	addFields(funcDecl.Type.Params)

	if len(candidates) == 0 {
		return nil
	}

	var result map[types.Object]bool

	mark := func(expr ast.Expr) {
		ident, ok := ast.Unparen(expr).(*ast.Ident)

		if !ok {
			return
		}

		if obj := info.Uses[ident]; obj != nil && candidates[obj] {
			if result == nil {
				result = map[types.Object]bool{}
			}

			result[obj] = true
		}
	}

	ast.Inspect(funcDecl.Body, func(node ast.Node) bool {
		funcLit, isLit := node.(*ast.FuncLit)

		if !isLit {
			return true
		}

		// Every assignment anywhere inside the literal, nested literals included.
		ast.Inspect(funcLit.Body, func(inner ast.Node) bool {
			switch stmt := inner.(type) {
			case *ast.AssignStmt:
				if stmt.Tok != token.DEFINE {
					for _, lhs := range stmt.Lhs {
						mark(lhs)
					}
				}
			case *ast.RangeStmt:
				if stmt.Tok == token.ASSIGN {
					if stmt.Key != nil {
						mark(stmt.Key)
					}

					if stmt.Value != nil {
						mark(stmt.Value)
					}
				}
			}

			return true
		})

		return false
	})

	return result
}

// paramReadsThroughBox reports whether obj is a pointer parameter of the current declaration that a
// func literal repoints (see closureRepointedPointerParams): it has no entry alias, and every value
// read renders through its box (`Ꮡn.DerefOrNull()`, Go-named `n.DerefOrNull()` once the function is
// complete).
func (v *Visitor) paramReadsThroughBox(obj types.Object) bool {
	return obj != nil && v.closureRepointedParams[obj]
}

// boxNameReadsThroughBox is paramReadsThroughBox keyed by the rendered box name (`Ꮡn`), for the
// assignment emitter, which holds only the rendered left-hand side. The name is unique only because
// two parameters of one declaration cannot share a name: any other box spelled the same within the
// declaration would also lose its re-alias here.
func (v *Visitor) boxNameReadsThroughBox(boxName string) bool {
	for obj := range v.closureRepointedParams {
		if AddressPrefix+obj.Name() == boxName {
			return true
		}
	}

	return false
}
