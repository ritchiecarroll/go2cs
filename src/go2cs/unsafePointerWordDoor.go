// unsafePointerWordDoor.go - Gbtc
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

// THE unsafe.Pointer WORD DOOR (W1 and W2; docs/phase4/DESIGN-cgo-phase3-marshaling.md 18.3 to 18.7, cure S-B1).
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
//
// A PROVABLY MANAGED SLOT keeps its emission too (unsafePointerWordSlotIsManaged): a slot the converter itself allocated
// in managed memory, which no native box can be. Over it the doors would take their `.Value` arm anyway, so the call
// buys no behavior and costs the Go-like shape. Two shapes, and only these:
//
//	(a) the reinterpret CARRYING arm, `(*unsafe.Pointer)(unsafe.Pointer(p))` of a Go pointer p whose pointee is not
//	    unsafe.Pointer (and its deref at more than one level), which reinterpretManagedEmission renders as a fresh
//	    `Ꮡ(<rvalue>)` box: `new Pointer(..)`, `OfFunc(f)`, `FromPinnedBox(..)`;
//	(b) `&x`, through the identity round trip `(*unsafe.Pointer)(unsafe.Pointer(&x))`, where x is a LOCAL variable or a
//	    PARAMETER: its box is the converter's `heap(..)`.
//
// A package variable, a field, an element, a call result, a *unsafe.Pointer variable or parameter and every pointer
// derived from a number stay on the doors. A mistake in this predicate fails LOUD: W4 refuses a `.Value` over a native
// box by name, so a slot wrongly called managed is a refusal and never a silent wrong read.
//
// ONE SILENT CASE, and it is master's behavior rather than this predicate's: a STORE through shape (a) writes into the
// fresh box, which nothing else reads, so the write is lost (reinterpretManagedEmission's own comment names it). The
// corpus has it at runtime's debugcall.cs (debugCallWrap2's dispatchF) and the per-OS cgocall.cs (cgocallbackg1's cb),
// both a pointer word stored into a func variable: the (ff) LOST WRITE finding of 2026-11-20 (9cec06ef4c).

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

	if v.unsafePointerWordSlotIsManaged(star.X) {
		return nil, false
	}

	return star.X, true
}

// unsafePointerWordSlotIsManaged reports whether a *unsafe.Pointer operand provably names a slot the converter allocated
// in managed memory: shapes (a) and (b) of the door's header comment.
func (v *Visitor) unsafePointerWordSlotIsManaged(operand ast.Expr) bool {
	return v.unsafePointerManagedSlotShape(operand) != 0
}

// unsafePointerShapeBStoreAccessor answers the accessor of a STORE `*p = v` through shape (b), `&x` of a local or a
// parameter: `.ValueSlot`, the converter's own convention for writing a held reference (`pp.ValueSlot = Ꮡx`). `.Value`
// would read the held value first and panic on a nil unsafe.Pointer, where Go stores (design decision on the review of
// cure S-B1, 2026-11-20; d02c2dda3b). Loads through (b) already read the slot (`~Ꮡx`), and shape (a)'s store keeps its emission:
// its fresh box is the (ff) lost write, not this. Every other operand keeps accessor.
func (v *Visitor) unsafePointerShapeBStoreAccessor(operand ast.Expr, accessor string) string {
	if isUnsafePointerPointerType(v.info.TypeOf(operand)) && v.unsafePointerManagedSlotShape(operand) == 'b' {
		return ".ValueSlot"
	}

	return accessor
}

// unsafePointerManagedSlotShape answers which provably managed shape a *unsafe.Pointer operand names, 'a' or 'b' of the
// door's header comment, and 0 for every other operand.
func (v *Visitor) unsafePointerManagedSlotShape(operand ast.Expr) byte {
	expr := ast.Unparen(operand)
	stripped := false

	// (a) at any level: the deref of a carrying conversion's fresh box is itself a fresh box.
	for {
		star, isStar := expr.(*ast.StarExpr)

		if !isStar {
			break
		}

		expr = ast.Unparen(star.X)
		stripped = true
	}

	call, isCall := expr.(*ast.CallExpr)

	if isCall && len(call.Args) == 1 {
		// The carrying arm's own test, asked of the function that makes it (never re-derived here), so the two cannot
		// drift: a source it declines is rendered by another route, which is not a fresh box.
		if _, isWordTarget := unsafePointerTargetLevels(v.info.TypeOf(call)); isWordTarget {
			if src, _, _ := v.pointerReinterpretManagedSource(call, call.Args[0]); src != nil {
				return 'a'
			}
		}

		// (b)'s identity round trip: (*unsafe.Pointer)(unsafe.Pointer(x)), x itself a *unsafe.Pointer.
		if inner, isRoundTrip := v.unsafePointerIdentityRoundTrip(call); isRoundTrip {
			expr = ast.Unparen(inner)
		}
	}

	// (b) &x, x a local variable or a parameter, and only as the operand itself: a stripped star means the slot is read
	// out of x (`**&x`, x a local *unsafe.Pointer), and what x holds may be a native box.
	unary, isAddr := expr.(*ast.UnaryExpr)

	if stripped || !isAddr || unary.Op != token.AND {
		return 0
	}

	ident, isIdent := ast.Unparen(unary.X).(*ast.Ident)

	if !isIdent {
		return 0
	}

	variable, isVar := v.info.ObjectOf(ident).(*types.Var)

	if isVar && !variable.IsField() && variable.Pkg() != nil && variable.Parent() != nil && variable.Parent() != variable.Pkg().Scope() {
		return 'b'
	}

	return 0
}

// unsafePointerIdentityRoundTrip answers x for the conversion `(*unsafe.Pointer)(unsafe.Pointer(x))` where x is itself a
// *unsafe.Pointer, and false for every other call.
func (v *Visitor) unsafePointerIdentityRoundTrip(call *ast.CallExpr) (ast.Expr, bool) {
	if tv, isType := v.info.Types[call.Fun]; !isType || !tv.IsType() || !isUnsafePointerPointerType(v.info.TypeOf(call)) {
		return nil, false
	}

	inner, isCall := ast.Unparen(call.Args[0]).(*ast.CallExpr)

	if !isCall || len(inner.Args) != 1 {
		return nil, false
	}

	if tv, isType := v.info.Types[inner.Fun]; !isType || !tv.IsType() {
		return nil, false
	}

	if basic, isBasic := types.Unalias(v.info.TypeOf(inner)).(*types.Basic); !isBasic || basic.Kind() != types.UnsafePointer {
		return nil, false
	}

	if !isUnsafePointerPointerType(v.info.TypeOf(inner.Args[0])) {
		return nil, false
	}

	return inner.Args[0], true
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
