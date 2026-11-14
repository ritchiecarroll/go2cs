// pointerArithmetic.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// This file owns two emissions that keep a derived pointer a POINTER until golib has it
// (docs/phase4/DESIGN-interior-pointer-root.md 4.2 and 4.3, with 10.2 and 10.3):
//
//   - THE ARITHMETIC FORM. Go's `unsafe.Pointer(uintptr(P) + n)`, written in one expression, is the valid
//     form of pointer arithmetic (package unsafe, rule 3) and the form `go vet`'s unsafeptr check admits. It is
//     emitted `@unsafe.Add(P', n)`: golib moves the pointer, and the moved pointer keeps what P carried. The
//     number form it replaces, `(@unsafe.Pointer)((uintptr)P + n)`, made a pointer from a number, and a number
//     carries nothing.
//
//   - THE RECEIVER AT A PROVEN USE. `unsafe.Pointer(recv)` inside a method renders the receiver's address
//     from its value alias, `(uintptr)@unsafe.Pointer.FromRef(ref recv)`: a number, which keeps nothing. Where
//     the receiver's box is in scope and the USE of the conversion is known at the node, the box is emitted.
//     Three uses are known at the node: the first argument of unsafe.Add, the P of an arithmetic form, and the
//     operand of Go's own `uintptr(...)`. Every other use (a call argument, a return, a store) keeps FromRef,
//     because what the callee or the reader does with the number is not known here; the mint guard in
//     internal/repoguard names each kept site.

package main

import (
	"fmt"
	"go/ast"
	"go/token"
	"go/types"
	"strings"
)

// pointerArithmeticSentinel stands for the `uintptr(P)` leaf while the rest of an arithmetic form is rendered.
// The sum is rendered once, whole, by the ordinary binary-expression path, so every term keeps the casts and
// parentheses it has always had; the sentinel is then cut from the front of the text.
const pointerArithmeticSentinel = "ᐧpointerArithmeticBaseᐧ"

// pointerArithmeticLeaf returns the `uintptr(P)` conversion of an arithmetic form, when callExpr is one: a
// conversion to unsafe.Pointer whose operand is a chain of `+` and `-` whose LEFTMOST operand is a conversion to
// uintptr of an unsafe.Pointer value. The leftmost position is go vet's rule (unsafeptr's isSafeArith): in
// `off + uintptr(p)` the pointer is not the base of the sum, and the form stays a number. So does a rounded
// pointer (`uintptr(p) &^ m`), which vet admits: what it subtracts is read from p's own number, so it is not an
// offset unsafe.Add can be handed.
func (v *Visitor) pointerArithmeticLeaf(callExpr *ast.CallExpr) *ast.CallExpr {
	if len(callExpr.Args) != 1 || !v.callExprIsTypeConversion(callExpr) {
		return nil
	}

	if !isUnsafePointerBasic(v.info.TypeOf(callExpr)) {
		return nil
	}

	node := ast.Unparen(callExpr.Args[0])

	if _, isBinary := node.(*ast.BinaryExpr); !isBinary {
		return nil
	}

	for {
		binary, isBinary := node.(*ast.BinaryExpr)

		if !isBinary {
			break
		}

		if binary.Op != token.ADD && binary.Op != token.SUB {
			return nil
		}

		node = binary.X
	}

	leaf, isCall := node.(*ast.CallExpr)

	if !isCall || len(leaf.Args) != 1 || !v.callExprIsTypeConversion(leaf) {
		return nil
	}

	if target, isBasic := types.Unalias(v.info.TypeOf(leaf)).(*types.Basic); !isBasic || target.Kind() != types.Uintptr {
		return nil
	}

	if !isUnsafePointerBasic(v.info.TypeOf(leaf.Args[0])) {
		return nil
	}

	return leaf
}

// isUnsafePointerBasic reports whether t is unsafe.Pointer itself, or an alias of it. A DEFINED type over
// unsafe.Pointer is a different C# type and takes no part in these emissions.
func isUnsafePointerBasic(t types.Type) bool {
	if t == nil {
		return false
	}

	basic, isBasic := types.Unalias(t).(*types.Basic)

	return isBasic && basic.Kind() == types.UnsafePointer
}

// pointerArithmeticEmission renders an arithmetic form as `@unsafe.Add(P', n)`. A difference keeps Go's
// wrapping arithmetic: `uintptr(P) - n` is `P + (0 - n)` modulo the word, so the offset is `(uintptr)0 - n`.
// A sum whose rendering does not begin with the leaf is left to the number form, and the site is named.
func (v *Visitor) pointerArithmeticEmission(callExpr *ast.CallExpr) (string, bool) {
	leaf := v.pointerArithmeticLeaf(callExpr)

	if leaf == nil {
		return "", false
	}

	saved := v.pointerArithmeticBase
	v.pointerArithmeticBase = leaf
	sum := v.convExpr(ast.Unparen(callExpr.Args[0]), nil)
	v.pointerArithmeticBase = saved

	var offset string

	switch {
	case strings.HasPrefix(sum, pointerArithmeticSentinel+" + "):
		offset = sum[len(pointerArithmeticSentinel)+3:]
	case strings.HasPrefix(sum, pointerArithmeticSentinel+" - "):
		offset = "(uintptr)0" + sum[len(pointerArithmeticSentinel):]
	}

	if offset == "" || strings.Contains(offset, pointerArithmeticSentinel) {
		v.showWarning("@pointerArithmeticEmission - line %d: `unsafe.Pointer(uintptr(P) + n)` did not render with P first (got %q); the pointer is made from the number and keeps nothing", v.fset.Position(callExpr.Pos()).Line, sum)
		return "", false
	}

	// P is consumed as a pointer: a receiver conversion there renders its box.
	base := leaf.Args[0]
	v.markCarriedPointerConversion(base)

	return fmt.Sprintf("@unsafe.Add(%s, %s)", v.convExpr(base, nil), offset), true
}

// markCarriedPointerArgument marks the first argument of unsafe.Add: golib receives it as a pointer.
func (v *Visitor) markCarriedPointerArgument(callExpr *ast.CallExpr) {
	if len(callExpr.Args) != 2 {
		return
	}

	selectorExpr, isSelector := callExpr.Fun.(*ast.SelectorExpr)

	if !isSelector {
		return
	}

	if builtin, isBuiltin := v.info.Uses[selectorExpr.Sel].(*types.Builtin); !isBuiltin || builtin.Pkg() != types.Unsafe || builtin.Name() != "Add" {
		return
	}

	v.markCarriedPointerConversion(callExpr.Args[0])
}

// markCarriedPointerConversion records that expr, when it is an `unsafe.Pointer(x)` conversion, is handed to
// golib as a pointer. Keyed on the conversion's node, so it can only apply to the conversion this use wraps.
func (v *Visitor) markCarriedPointerConversion(expr ast.Expr) {
	conversion, isCall := ast.Unparen(expr).(*ast.CallExpr)

	if !isCall || len(conversion.Args) != 1 || !v.callExprIsTypeConversion(conversion) || !isUnsafePointerBasic(v.info.TypeOf(conversion)) {
		return
	}

	if v.carriedPointerCasts == nil {
		v.carriedPointerCasts = map[*ast.CallExpr]bool{}
	}

	v.carriedPointerCasts[conversion] = true
}

// receiverPointerAtProvenUse renders `unsafe.Pointer(recv)` from the receiver's BOX at the three uses known at
// the node, and reports false everywhere else (the caller keeps FromRef). As a pointer handed to golib it is
// `@unsafe.Pointer.FromPinnedBox(Ꮡrecv)`, with no cast: a cast would turn it back into a number before
// unsafe.Add saw it (10.3). Under Go's own `uintptr(...)` it is the bare box, and the enclosing conversion's
// `(uintptr)` cast reads its number, as it does for any other pointer operand (markDeadUnsafePointerBox).
func (v *Visitor) receiverPointerAtProvenUse(callExpr *ast.CallExpr) (string, bool) {
	carried := v.carriedPointerCasts[callExpr]

	if !carried && !v.deadUnsafePointerBoxes[callExpr] {
		return "", false
	}

	arg := callExpr.Args[0]

	if !v.exprHasReceiverBoxInScope(arg) {
		return "", false
	}

	identContext := DefaultIdentContext()
	identContext.isPointer = true
	box := v.convExpr(arg, []ExprContext{identContext})

	if carried {
		return fmt.Sprintf("@unsafe.Pointer.FromPinnedBox(%s)", box), true
	}

	return box, true
}
