// elementAddressCompare.go - Gbtc
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
	"strings"
)

// An ELEMENT-ADDRESS COMPARE is a `==` or `!=` whose operands are both element address-ofs: `&x[i] == &y[j]`
// (math/big's alias() and addVW, the crypto alias packages' InexactOverlap, regexp, cryptobyte, syscall).
// Go's `&x[i]` is a plain interior pointer and costs nothing; golib's `Ꮡ(x, i)` mints an element box, and a
// named slice or array header is boxed as well to reach it. Comparing only needs the element's identity,
// the backing storage and the absolute index, so each operand renders as golib's `Ꮡelem`, which returns
// that identity as a value (ElementAddress<T>) and allocates nothing:
//
//	Ꮡ(z, 0) != Ꮡ(x, 0)                  ->  Ꮡelem(z, 0) != Ꮡelem(x, 0)
//	Ꮡ(x.slice(0, cap(x)), cap(x) - 1)    ->  Ꮡelem<Word>(x.slice(0, cap(x)), cap(x) - 1)   (a named slice)
//	p.at<uint16>(0)                      ->  p.Ꮡelem<uint16>(0)                            (a pointer to an array)
//
// The rewrite covers only the element arms of convUnaryExpr named below, and only when BOTH operands take
// one: a compare is rewritten whole or not at all, because ElementAddress<T> has no conversion to or from
// ж<T>. Every other arm (a named array behind a box, whose backing the box publishes lazily; an array field
// reached through a box; a nested element chain; the receiver copy form; a captured box inside a lambda)
// keeps today's boxed compare. Only parentheses are removed to find the operands, so `any(&x[0])`, a
// pointer conversion and a switch tag stay boxed. (1.26.10.2 seat #10, COORD ruling 2026-10-27.)

// elementAddressTakeKind is the golib form an element address-of takes in a compare.
type elementAddressTakeKind int

const (
	// Ꮡelem(x, i) over a slice<T> or array<T> header; `<E>` is written for a named slice type, whose
	// wrapper C# reaches the slice<T> overload through only with the element type given.
	elementTakeHeader elementAddressTakeKind = iota + 1

	// base.Ꮡelem<E>(i) through a ж<array<E>> box: a pointer to an array, or an addressed array local.
	elementTakeBox
)

type elementAddressTake struct {
	kind        elementAddressTakeKind
	parens      int
	indexExpr   *ast.IndexExpr
	typeArg     string // the explicit element type for a named slice header, or the box take's
	projection  string // `.Value` on a named array header (lazyArrayBackingProjection)
	unsignedArg bool   // the box arm's index operand (convArrayIndex), else fullValueIndexOperand
}

// elementAddressCompare renders the two operands of an element-address compare, left then right, or reports
// false when either operand is not a covered element address-of; the caller then renders the compare as before.
func (v *Visitor) elementAddressCompare(binaryExpr *ast.BinaryExpr, context PatternMatchExprContext) (string, string, bool) {
	if (binaryExpr.Op != token.EQL && binaryExpr.Op != token.NEQ) || context.usePattenMatch {
		return "", "", false
	}

	left, ok := v.elementAddressTakeOf(binaryExpr.X)

	if !ok {
		return "", "", false
	}

	right, ok := v.elementAddressTakeOf(binaryExpr.Y)

	if !ok {
		return "", "", false
	}

	return v.renderElementAddressTake(left), v.renderElementAddressTake(right), true
}

// elementAddressTakeOf classifies one operand without rendering it, so a compare whose other operand is not
// covered renders nothing twice.
func (v *Visitor) elementAddressTakeOf(operand ast.Expr) (elementAddressTake, bool) {
	take := elementAddressTake{}

	for {
		paren, ok := operand.(*ast.ParenExpr)

		if !ok {
			break
		}

		take.parens++
		operand = paren.X
	}

	unaryExpr, ok := operand.(*ast.UnaryExpr)

	if !ok || unaryExpr.Op != token.AND {
		return take, false
	}

	indexExpr, ok := unaryExpr.X.(*ast.IndexExpr)

	if !ok {
		return take, false
	}

	// inside a lambda, a captured box renders through lambdaBoxRefAddressForm (convUnaryExprCore's first arm)
	if _, ok := v.lambdaBoxRefAddressForm(unaryExpr); ok {
		return take, false
	}

	take.indexExpr = indexExpr

	// the kind tests read the UNDERLYING type, as convUnaryExprCore's element arms do; whether the base is a
	// NAMED slice or array is a question of its declared type
	declaredType := v.getType(indexExpr.X, false)
	exprType := v.getType(indexExpr.X, true)

	if declaredType == nil || exprType == nil {
		return take, false
	}

	if _, isTypeParam := types.Unalias(declaredType).(*types.TypeParam); isTypeParam {
		return take, false
	}

	_, baseIsNamed := types.Unalias(declaredType).(*types.Named)

	recvName, isRecvPointer := v.pointerReceiverName()
	baseIdent, baseIsIdent := indexExpr.X.(*ast.Ident)
	baseIsRecvIdent := baseIsIdent && isRecvPointer && v.identResolvesToReceiver(baseIdent, recvName)

	// refRecv as convUnaryExprCore computes it: the ROOT identifier of the base is the pointer receiver
	refRecv := false

	if root := getIdentifier(indexExpr.X); root != nil && isRecvPointer && v.identResolvesToReceiver(root, recvName) {
		refRecv = true
	}

	if ptr, isPtr := exprType.(*types.Pointer); isPtr {
		arrayType, isArray := ptr.Elem().Underlying().(*types.Array)

		if !isArray {
			return take, false
		}

		// the receiver itself: `Ꮡ(recv.Value, i)` today, a header take
		if baseIsRecvIdent {
			take.kind = elementTakeHeader
			take.projection = v.lazyArrayBackingProjection(ptr.Elem())
			return take, true
		}

		// a NAMED array behind the box is published lazily through it (ж.arrayView), so it keeps `.at<E>(i)`
		if _, isNamed := types.Unalias(ptr.Elem()).(*types.Named); isNamed {
			return take, false
		}

		take.kind = elementTakeBox
		take.typeArg = convertToCSTypeName(v.getScopeCheckedTypeName(arrayType.Elem()))
		take.unsignedArg = true
		return take, true
	}

	if sliceType, isSlice := exprType.Underlying().(*types.Slice); isSlice {
		// the receiver copy form `Ꮡ(recv[i])` boxes a copy; it is left as it is
		if refRecv && baseIsIdent {
			return take, false
		}

		take.kind = elementTakeHeader

		if baseIsNamed {
			take.typeArg = convertToCSTypeName(v.getScopeCheckedTypeName(sliceType.Elem()))
		}

		return take, true
	}

	arrayType, isArray := exprType.Underlying().(*types.Array)

	if !isArray || !strings.HasPrefix(v.getAliasQualifiedTypeName(exprType, false), "[") {
		return take, false
	}

	if refRecv {
		// an array FIELD of the receiver: `Ꮡ(recv.f, i)` today, with `.Value` on a named array field
		if baseIsIdent {
			return take, false
		}

		take.kind = elementTakeHeader
		take.projection = v.lazyArrayBackingProjection(v.getType(indexExpr.X, false))
		return take, true
	}

	// a heap-escaped array LOCAL owns a ж<array<E>> box: `ᏑX.at<E>(i)` today
	if baseIsIdent {
		if obj := v.info.ObjectOf(baseIdent); obj != nil && v.identEscapesHeap[obj] {
			// a NAMED array behind the box is published lazily through it, as for a pointer to one
			if baseIsNamed {
				return take, false
			}

			take.kind = elementTakeBox
			take.typeArg = convertToCSTypeName(v.getScopeCheckedTypeName(arrayType.Elem()))
			take.unsignedArg = true
			return take, true
		}
	}

	// every other array base (a field reached through a box, a nested element, a parameter's copy box,
	// the naive fallback) keeps today's form
	return take, false
}

func (v *Visitor) renderElementAddressTake(take elementAddressTake) string {
	var rendered string

	switch take.kind {
	case elementTakeHeader:
		typeArg := ""

		if take.typeArg != "" {
			typeArg = "<" + take.typeArg + ">"
		}

		rendered = fmt.Sprintf("Ꮡelem%s(%s%s, %s)", typeArg, v.convExpr(take.indexExpr.X, nil), take.projection, v.fullValueIndexOperand(take.indexExpr.Index))
	case elementTakeBox:
		// the base in POINTER context, so it yields the box (`Ꮡtab`, or the box-valued local), as the `.at<E>(i)` arm does
		boxIdentContext := DefaultIdentContext()
		boxIdentContext.isPointer = true
		base := v.parenthesizeConversionBase(take.indexExpr.X, v.convExpr(take.indexExpr.X, []ExprContext{boxIdentContext}))

		rendered = fmt.Sprintf("%s.Ꮡelem<%s>(%s)", base, take.typeArg, v.convArrayIndex(take.indexExpr.Index))
	}

	for range take.parens {
		rendered = "(" + rendered + ")"
	}

	return rendered
}

// pointerReceiverName reports the current function's receiver name when that receiver is a pointer, the
// receiver context convUnaryExprCore reads before its element arms.
func (v *Visitor) pointerReceiverName() (string, bool) {
	if v.currentFuncSignature == nil {
		return "", false
	}

	recv := v.currentFuncSignature.Recv()

	if recv == nil {
		return "", false
	}

	if _, ok := recv.Type().(*types.Pointer); !ok {
		return "", false
	}

	return recv.Name(), true
}
