// cgoPointerDoor.go - Gbtc
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
	"go/token"
	"go/types"
)

// THE **C.T DOOR (cgo Phase 3, increment 3.2, seat 1c; docs/phase4/DESIGN-cgo-phase3-marshaling.md 18.11).
//
// On the cgo route, `*x` and `*x = v`, where x's static type is `**U` and U is a type cmd/cgo declares, are emitted as
// calls:
//
//	*x        global::go.golib.GoCgoArgs.LoadPointer(x)
//	*x = v    global::go.golib.GoCgoArgs.StorePointer(x, v);
//
// `&l.next` over a C struct in C memory is a native box at the field's Go offset, which names a WORD of C memory. `.val`
// is a `ref ж<U>`, and a word cannot sit behind a ref: the reference floor refuses that read by name. So the load and
// the store are calls, and the calls decide by the box, as the *unsafe.Pointer word door does (unsafePointerWordDoor.go).
// A managed box takes `.Value` there, so a slot Go allocated reads and writes exactly as before.
//
// SCOPE: the package takes the cgo route (cgoRouteActive), and U is named by cmd/cgo's TYPE prefixes (classifyCgoName:
// a struct, a union, an enum or `_Ctype_x`, so `**C.char` is on the door), the same name test 1a's marker reads. A
// DEFINED pointer typedef as the pointee (`*_Ctype_xptr`) is a named type, not `*U`, and keeps its emission. So do a
// tuple assignment target (the door is a statement), a ref-lowered parameter (a C# ref to a managed slot, with no box),
// a named pointer type for x, and every package off the route: the stdlib and the behavioral corpus move 0 lines.
// Whatever the door does not reach still meets the reference floor over a native box, which refuses by name.

// isCgoPointerPointerType reports whether t is exactly `**U`, U a type cmd/cgo declares.
func isCgoPointerPointerType(t types.Type) bool {
	if t == nil {
		return false
	}

	outer, isPointer := types.Unalias(t).(*types.Pointer)

	if !isPointer {
		return false
	}

	inner, isPointer := types.Unalias(outer.Elem()).(*types.Pointer)

	if !isPointer {
		return false
	}

	named, isNamed := types.Unalias(inner.Elem()).(*types.Named)

	if !isNamed {
		return false
	}

	declaration, ok := classifyCgoName(named.Obj().Name())

	if !ok {
		return false
	}

	switch declaration.kind {
	case cgoStruct, cgoUnion, cgoEnum, cgoType:
		return true
	}

	return false
}

// cgoPointerDoorOperand answers the operand of `*x` when the dereference takes the door, and false otherwise.
func (v *Visitor) cgoPointerDoorOperand(expr ast.Expr) (ast.Expr, bool) {
	if !cgoRouteActive {
		return nil, false
	}

	star, isStar := ast.Unparen(expr).(*ast.StarExpr)

	if !isStar {
		return nil, false
	}

	// A TYPE `**C.T` (a conversion's target) is not a dereference.
	if tv, isType := v.info.Types[star.X]; isType && tv.IsType() {
		return nil, false
	}

	if !isCgoPointerPointerType(v.info.TypeOf(star.X)) {
		return nil, false
	}

	// A REF-LOWERED parameter (refLoweringEmissionOperations.go) is a C# `ref` to a managed slot and has no box.
	if ident, isIdent := ast.Unparen(star.X).(*ast.Ident); isIdent && v.paramIsRefLoweredObj(v.info.ObjectOf(ident)) {
		return nil, false
	}

	return star.X, true
}

// cgoPointerDoorLoad renders the door's load for `*x`, reporting false for every other expression.
func (v *Visitor) cgoPointerDoorLoad(starExpr *ast.StarExpr, context StarExprContext) (string, bool) {
	if context.inLhsAssign {
		return "", false
	}

	operand, isDoor := v.cgoPointerDoorOperand(starExpr)

	if !isDoor {
		return "", false
	}

	return fmt.Sprintf("global::go.golib.GoCgoArgs.LoadPointer(%s)", v.unsafePointerWordBox(operand)), true
}

// cgoPointerDoorStore renders the door's store for the single assignment `*x = v` as a statement.
func (v *Visitor) cgoPointerDoorStore(assign *ast.AssignStmt) (string, bool) {
	if assign.Tok != token.ASSIGN || len(assign.Lhs) != 1 || len(assign.Rhs) != 1 {
		return "", false
	}

	operand, isDoor := v.cgoPointerDoorOperand(assign.Lhs[0])

	if !isDoor {
		return "", false
	}

	// The stored value is a POINTER: a deref-aliased pointer parameter renders its box (`Ꮡv`), never its value alias.
	pointerContext := DefaultIdentContext()
	pointerContext.isPointer = true

	return fmt.Sprintf("global::go.golib.GoCgoArgs.StorePointer(%s, %s);", v.unsafePointerWordBox(operand), v.convExpr(assign.Rhs[0], []ExprContext{pointerContext})), true
}

// pointerWordDoorLoad is the load of either word door: W1 for a *unsafe.Pointer, else the **C.T door.
func (v *Visitor) pointerWordDoorLoad(starExpr *ast.StarExpr, context StarExprContext) (string, bool) {
	if load, ok := v.unsafePointerWordLoad(starExpr, context); ok {
		return load, true
	}

	return v.cgoPointerDoorLoad(starExpr, context)
}

// pointerWordDoorStore is the store of either word door: W2 for a *unsafe.Pointer, else the **C.T door.
func (v *Visitor) pointerWordDoorStore(assign *ast.AssignStmt) (string, bool) {
	if store, ok := v.unsafePointerWordStore(assign); ok {
		return store, true
	}

	return v.cgoPointerDoorStore(assign)
}
