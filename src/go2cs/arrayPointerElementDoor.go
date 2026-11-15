// arrayPointerElementDoor.go - Gbtc
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

// THE ELEMENT DOORS (docs/phase4/DESIGN-array-pointer-doors.md, door B).
//
// An index or a dereference applied directly to a pointer-to-array conversion whose operand is an unsafe.Pointer
// was emitted through a box of the WHOLE array (`@unsafe.Pointer.To<array<E>>(p)` and its `.Value`). Memory that
// holds no Go array has no `array<E>` to answer with, so over native memory every one of these forms panicked by
// name, and over a Go value of another type the box answered a copy.
//
// The forms are emitted as golib doors that receive the pointer itself and the array's length N as a constant:
//
//	(*A)(p)[i]          @unsafe.Pointer.ElementRef<E>(p, i, N)      a `ref E`: read, store, compound, ++
//	&(*A)(p)[i]         @unsafe.Pointer.Element<E>(p, i, N)         the element's pointer
//	*(*A)(p)            @unsafe.Pointer.LoadArray<E>(p, N)          read for its value
//	*(*A)(p) = v        @unsafe.Pointer.StoreArray<E>(p, N, v)
//
// A NAMED array's value is its generated wrapper over `array<E>`, which converts to and from it: the load is
// cast to the named type, `(A)@unsafe.Pointer.LoadArray<E>(p, N)`, and the store takes the wrapper as it is.
//
// No box of the whole array exists at any point. What the element is (an element of a Go array, a location in
// native memory, bytes of another Go value) is decided at run time by what the pointer names; see the doors in
// src/core/unsafe/unsafe.cs.
//
// THE SCOPE IS THE VIEW DOORS' SCOPE (arrayPointerViewDoor.go), read from the same predicates: an element that
// holds references takes no door, a conversion that another route renders whole keeps that route, and an operand
// that holds a `FromRef` mint keeps its emission.
// A dereference is a load only where it is used for its value (collectArrayPointerLoads); every other use of it
// (an address, a method call) keeps the box route.
//
// THE LENGTH IS THE TYPE'S. Go answers `len` and `cap` of an array, and the count of a range over one, from the
// array's TYPE, and where the operand holds no call and no receive it does not evaluate the operand at all. A box of
// the whole array answers the length of whatever Go array it found at the address, or panics over memory that
// holds none. So `len(*(*A)(p))`, `cap(*(*A)(p))`, the same two over `(*A)(p)`, and a range over `*(*A)(p)` for its
// index alone emit the constant N and name no memory (arrayPointerConversionLength). Where Go does evaluate the
// operand (a range for its values, or a call inside the operand), the dereference is a load.
// The conversion itself ranged for its values, `for _, x := range (*A)(p)`, reads N elements through the pointer
// and copies nothing: it ranges the view door's window of the whole array (arrayPointerRangeWindow).

// packageArrayPointerLoads marks each dereference that is used for its value and nothing else. It is rebuilt by
// collectArrayPointerLoads per package, beside the value puns' maps and for the same reason: where a dereference
// stands is a fact of its parent, which the emission visitor does not see.
var packageArrayPointerLoads map[*ast.StarExpr]bool

// collectArrayPointerLoads is the package pre-pass. It is self-resetting. It records positions only; whether a
// recorded dereference is a door's is decided at emission, by arrayPointerLoadDoor.
func collectArrayPointerLoads(files []FileEntry, info *types.Info) {
	packageArrayPointerLoads = map[*ast.StarExpr]bool{}

	for _, fileEntry := range files {
		var stack []ast.Node

		ast.Inspect(fileEntry.file, func(n ast.Node) bool {
			if n == nil {
				stack = stack[:len(stack)-1]
				return true
			}

			var parent ast.Node

			if len(stack) > 0 {
				parent = stack[len(stack)-1]
			}

			stack = append(stack, n)

			if star, ok := n.(*ast.StarExpr); ok && starExprIsValueOperand(star, parent, info) {
				packageArrayPointerLoads[star] = true
			}

			return true
		})
	}
}

// starExprIsValueOperand reports whether a dereference stands where Go copies its value: the right side of an
// assignment or a declaration, a returned value, a call argument, an operand of == and !=, or the operand of a range
// that Go evaluates. The list is the positions themselves, so a position it does not name keeps the general emission.
func starExprIsValueOperand(star *ast.StarExpr, parent ast.Node, info *types.Info) bool {
	among := func(exprs []ast.Expr) bool {
		for _, expr := range exprs {
			if expr == star {
				return true
			}
		}

		return false
	}

	switch p := parent.(type) {
	case *ast.AssignStmt:
		return among(p.Rhs)
	case *ast.ValueSpec:
		return among(p.Values)
	case *ast.ReturnStmt:
		return among(p.Results)
	case *ast.CallExpr:
		// A call Go folded to a constant evaluated no argument: `len(*(*A)(p))` reads no memory.
		if tv, ok := info.Types[p]; ok && tv.Value != nil {
			return false
		}

		return among(p.Args)
	case *ast.BinaryExpr:
		return p.Op == token.EQL || p.Op == token.NEQ
	case *ast.RangeStmt:
		// Go copies a ranged array when the loop reads its values. For its index alone it ranges the type's length
		// and evaluates the operand only for a call or a receive inside it.
		return p.X == star && (rangeReadsValues(p) || exprHoldsCallOrReceive(star, info))
	}

	return false
}

// rangeReadsValues reports whether a range statement has a value variable that is not blank.
func rangeReadsValues(rangeStmt *ast.RangeStmt) bool {
	if rangeStmt.Value == nil {
		return false
	}

	ident, isIdent := rangeStmt.Value.(*ast.Ident)

	return !isIdent || ident.Name != "_"
}

// exprHoldsCallOrReceive reports whether expr holds a function call or a channel receive, which is the spec's test
// for whether `len`, `cap` and an index-only range evaluate an array operand. A conversion is no call, and neither is
// a call Go folded to a constant (unsafe.Sizeof).
func exprHoldsCallOrReceive(expr ast.Expr, info *types.Info) bool {
	found := false

	ast.Inspect(expr, func(n ast.Node) bool {
		if found {
			return false
		}

		switch node := n.(type) {
		case *ast.CallExpr:
			if tv, ok := info.Types[node.Fun]; ok && tv.IsType() {
				return true
			}

			if tv, ok := info.Types[node]; ok && tv.Value != nil {
				return false
			}

			found = true

			return false
		case *ast.UnaryExpr:
			if node.Op == token.ARROW {
				found = true

				return false
			}
		}

		return true
	})

	return found
}

// arrayPointerConversionLength reads expr as `*(*A)(x)` or `(*A)(x)`, A an array type of any name and element, with
// no call and no receive inside it, and answers A's length. Go answers the length of both forms from the type and
// evaluates neither, so the answer needs no memory and holds for every operand, a nil one included.
func (v *Visitor) arrayPointerConversionLength(expr ast.Expr) (int64, bool) {
	expr = ast.Unparen(expr)

	if star, isStar := expr.(*ast.StarExpr); isStar {
		expr = ast.Unparen(star.X)
	}

	call, isCall := expr.(*ast.CallExpr)

	if !isCall || len(call.Args) != 1 {
		return 0, false
	}

	if tv, ok := v.info.Types[call.Fun]; !ok || !tv.IsType() {
		return 0, false
	}

	ptr, isPointer := types.Unalias(v.info.TypeOf(call)).Underlying().(*types.Pointer)

	if !isPointer {
		return 0, false
	}

	arr, isArray := types.Unalias(ptr.Elem()).Underlying().(*types.Array)

	if !isArray || exprHoldsCallOrReceive(call, v.info) {
		return 0, false
	}

	return arr.Len(), true
}

// arrayPointerConversionIsUnevaluated reports whether expr is a pointer-to-array conversion, dereferenced or not,
// that Go measures without evaluating.
func (v *Visitor) arrayPointerConversionIsUnevaluated(expr ast.Expr) bool {
	_, ok := v.arrayPointerConversionLength(expr)

	return ok
}

// arrayPointerRangeWindow renders the operand of `for _, x := range (*A)(p)`, the conversion itself ranged for its
// values, and answers the element type the loop ranges. Go reads each of the N elements through the pointer as
// the loop reaches it and copies nothing, which is the view door's window of the whole array, `(*A)(p)[0:N]`. The
// box it replaces ranged the length of whatever Go array the address held. It declines where the view doors do.
func (v *Visitor) arrayPointerRangeWindow(rangeStmt *ast.RangeStmt) (string, types.Type, bool) {
	if !rangeReadsValues(rangeStmt) {
		return "", nil, false
	}

	if _, isStar := ast.Unparen(rangeStmt.X).(*ast.StarExpr); isStar {
		return "", nil, false
	}

	view, ok := v.arrayPointerViewOfBase(rangeStmt.X)

	if !ok {
		return "", nil, false
	}

	operand, ok := v.arrayPointerViewOperand(view)

	if !ok {
		return "", nil, false
	}

	return fmt.Sprintf("@unsafe.Pointer.View<%s>(%s, 0, %d, %d)", view.elem, operand, view.length, view.length), view.elemGo, true
}

// arrayPointerElementRefDoor renders `(*A)(p)[i]` as the location door. The index travels at its full value: the
// door checks it against N with Go's index panic.
func (v *Visitor) arrayPointerElementRefDoor(indexExpr *ast.IndexExpr) (string, bool) {
	return v.arrayPointerIndexDoor(indexExpr, "ElementRef")
}

// arrayPointerElementAddressDoor renders `&(*A)(p)[i]` as the element's pointer.
func (v *Visitor) arrayPointerElementAddressDoor(unaryExpr *ast.UnaryExpr) (string, bool) {
	indexExpr, ok := ast.Unparen(unaryExpr.X).(*ast.IndexExpr)

	if !ok || unaryExpr.Op != token.AND {
		return "", false
	}

	return v.arrayPointerIndexDoor(indexExpr, "Element")
}

func (v *Visitor) arrayPointerIndexDoor(indexExpr *ast.IndexExpr, door string) (string, bool) {
	view, ok := v.arrayPointerViewOfBase(indexExpr.X)

	if !ok {
		return "", false
	}

	operand, ok := v.arrayPointerViewOperand(view)

	if !ok {
		return "", false
	}

	return fmt.Sprintf("@unsafe.Pointer.%s<%s>(%s, %s, %d)", door, view.elem, operand, v.castWideIntegerToNint(indexExpr.Index), view.length), true
}

// arrayPointerLoadOf reads star as `*(*A)(p)` used for its value.
func (v *Visitor) arrayPointerLoadOf(star *ast.StarExpr) (arrayPointerView, bool) {
	if !packageArrayPointerLoads[star] {
		return arrayPointerView{}, false
	}

	return v.arrayPointerWholeOf(star)
}

// arrayPointerWholeOf reads star as `*(*A)(p)`.
func (v *Visitor) arrayPointerWholeOf(star *ast.StarExpr) (arrayPointerView, bool) {
	// The conversion is the dereference's own operand; a second dereference inside it is another expression.
	if _, nested := ast.Unparen(star.X).(*ast.StarExpr); nested {
		return arrayPointerView{}, false
	}

	return v.arrayPointerViewOfBase(star.X)
}

// arrayPointerLoadDoor renders a dereference read for its value as the array load: a new Go array value, copied
// element by element from what the pointer names.
func (v *Visitor) arrayPointerLoadDoor(star *ast.StarExpr) (string, bool) {
	view, ok := v.arrayPointerLoadOf(star)

	if !ok {
		return "", false
	}

	operand, ok := v.arrayPointerViewOperand(view)

	if !ok {
		return "", false
	}

	load := fmt.Sprintf("@unsafe.Pointer.LoadArray<%s>(%s, %d)", view.elem, operand, view.length)

	if view.named {
		load = fmt.Sprintf("(%s)%s", view.array, load)
	}

	return load, true
}

// arrayPointerLoadIsOwnCopy reports whether expr is a dereference the load door renders. The door's answer is
// already a copy, so the value-copy clone has nothing to add to it.
func (v *Visitor) arrayPointerLoadIsOwnCopy(expr ast.Expr) bool {
	star, ok := ast.Unparen(expr).(*ast.StarExpr)

	if !ok {
		return false
	}

	_, isDoor := v.arrayPointerLoadDoor(star)

	return isDoor
}

// arrayPointerStoreDoor renders the statement `*(*A)(p) = v` as the array store. Go evaluates the pointer
// before the value, and the arguments keep that order.
func (v *Visitor) arrayPointerStoreDoor(lhs ast.Expr, rhs ast.Expr) (string, bool) {
	star, ok := ast.Unparen(lhs).(*ast.StarExpr)

	if !ok {
		return "", false
	}

	view, ok := v.arrayPointerWholeOf(star)

	if !ok {
		return "", false
	}

	operand, ok := v.arrayPointerViewOperand(view)

	if !ok {
		return "", false
	}

	return fmt.Sprintf("@unsafe.Pointer.StoreArray<%s>(%s, %d, %s);", view.elem, operand, view.length, v.convExpr(rhs, nil)), true
}
