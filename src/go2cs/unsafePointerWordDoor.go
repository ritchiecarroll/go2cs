// unsafePointerWordDoor.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

package main

import (
	"fmt"
	"go/ast"
	"go/token"
	"go/types"
)

// THE unsafe.Pointer WORD DOOR (W1 and W2; docs/phase4/DESIGN-cgo-phase3-marshaling.md 18.3 to 18.7, seat S-B1).
//
// `*p` and `*p = v`, where p's static type is *unsafe.Pointer, are emitted as calls:
//
//	*p        @unsafe.Pointer.LoadWord(p)
//	*p = v    @unsafe.Pointer.StoreWord(p, v);
//
// golib's unsafe.Pointer is a CLR class, and `p.Value` is a `ref` to a Pointer slot. Over a box that holds a CLR Pointer
// slot (a variable, a field or an element typed unsafe.Pointer) that is the value. Over a NATIVE box, the reinterpret
// `(*unsafe.Pointer)(unsafe.Pointer(&w))` of a Go word or a word of C memory, the slot is a WORD, and converting between
// a word and a CLR object cannot sit behind a `ref`: a write through the ref has no way to tell the box what to publish.
// So the load and the store are calls, and the calls decide by the box (unsafe.Pointer.LoadWord / StoreWord).
//
// SCOPE: exactly the dereference of an expression whose type is *unsafe.Pointer. A named pointer type, a pointer to a
// named type whose underlying type is unsafe.Pointer, and every other pointee keep their emission. A tuple assignment
// target (`*p, x = a, b`) keeps `.Value`: the door is a statement, and a tuple element is not one.

// isUnsafePointerPointerType reports whether t is exactly *unsafe.Pointer.
func isUnsafePointerPointerType(t types.Type) bool {
	if t == nil {
		return false
	}

	pointer, isPointer := types.Unalias(t).(*types.Pointer)

	if !isPointer {
		return false
	}

	basic, isBasic := types.Unalias(pointer.Elem()).(*types.Basic)
	return isBasic && basic.Kind() == types.UnsafePointer
}

// unsafePointerWordOperand answers the operand of `*x` when x is a *unsafe.Pointer, and false otherwise.
func (v *Visitor) unsafePointerWordOperand(expr ast.Expr) (ast.Expr, bool) {
	star, isStar := ast.Unparen(expr).(*ast.StarExpr)

	if !isStar {
		return nil, false
	}

	// A TYPE `*unsafe.Pointer` (a conversion's target) is not a dereference.
	if tv, isType := v.info.Types[star.X]; isType && tv.IsType() {
		return nil, false
	}

	if !isUnsafePointerPointerType(v.info.TypeOf(star.X)) {
		return nil, false
	}

	// A REF-LOWERED parameter (refLoweringEmissionOperations.go) is a C# `ref` to a CLR Pointer slot: every caller passes
	// the address of an unsafe.Pointer variable, so it names a slot and never a word, and it has no box to hand a door.
	if ident, isIdent := ast.Unparen(star.X).(*ast.Ident); isIdent && v.paramIsRefLoweredObj(v.info.ObjectOf(ident)) {
		return nil, false
	}

	return star.X, true
}

// unsafePointerWordBox renders the BOX a *unsafe.Pointer operand names: never a parameter's deref alias.
func (v *Visitor) unsafePointerWordBox(operand ast.Expr) string {
	if ident, isIdent := ast.Unparen(operand).(*ast.Ident); isIdent && v.isDerefdPointerParamIdent(ident) {
		return AddressPrefix + v.getIdentName(ident)
	}

	return v.convExpr(operand, nil)
}

// unsafePointerWordLoad renders W1 for `*x`, x a *unsafe.Pointer, reporting false for every other expression.
func (v *Visitor) unsafePointerWordLoad(starExpr *ast.StarExpr, context StarExprContext) (string, bool) {
	if context.inLhsAssign {
		return "", false
	}

	operand, isWord := v.unsafePointerWordOperand(starExpr)

	if !isWord {
		return "", false
	}

	return fmt.Sprintf("@unsafe.Pointer.LoadWord(%s)", v.unsafePointerWordBox(operand)), true
}

// unsafePointerWordStore renders W2 for the single assignment `*x = v`, x a *unsafe.Pointer, as a statement.
func (v *Visitor) unsafePointerWordStore(assign *ast.AssignStmt) (string, bool) {
	// A func-word store (funcWordOperations.go) keeps its own door.
	if assign.Tok != token.ASSIGN || len(assign.Lhs) != 1 || len(assign.Rhs) != 1 || packageFuncWords[assign] != nil {
		return "", false
	}

	operand, isWord := v.unsafePointerWordOperand(assign.Lhs[0])

	if !isWord {
		return "", false
	}

	return fmt.Sprintf("@unsafe.Pointer.StoreWord(%s, %s);", v.unsafePointerWordBox(operand), v.convExpr(assign.Rhs[0], nil)), true
}
