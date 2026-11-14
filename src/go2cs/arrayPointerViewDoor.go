// arrayPointerViewDoor.go - Gbtc
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
	"strings"
)

// THE VIEW DOORS (docs/phase4/DESIGN-array-pointer-doors.md, door A).
//
// A slice of a pointer-to-array conversion whose operand is an unsafe.Pointer, `(*A)(p)[lo:hi]`, was emitted as
// `new slice<E>(new ReadOnlySpan<E>((E*)(uintptr)p, n))`. The span constructor copies, so a write through the
// slice never reached the memory; the capacity was the length; the length was narrowed to int; and the open form
// of a named array (`(*RawMem)(p)[:]`, modernc.org/libc) took the box route and read the array as a value.
//
// The forms are emitted as golib doors that receive the pointer itself and the array's length N as a constant:
//
//	(*A)(p)[lo:hi]              @unsafe.Pointer.View<E>(p, lo, hi, N)
//	(*A)(p)[lo:hi:max]          @unsafe.Pointer.View<E>(p, lo, hi, max, N)
//	(*A)(p)[lo:]                @unsafe.Pointer.View<E>(p, lo, N)              A a NAMED array
//	copy((*A)(p)[lo:], src)     @unsafe.Pointer.CopyInto<E>(p, lo, N, src)     A a NAMED array
//	copy(dst, (*A)(p)[lo:])     @unsafe.Pointer.CopyFrom<E>(dst, p, lo, N)     A a NAMED array
//
// What a view is (a window on a Go array, a window on native memory, or a copy of another Go value) is decided at
// run time by what the pointer names; see the doors in src/core/unsafe/unsafe.cs.
//
// Four forms keep what they had. An element that holds references is refused by name (managedSliceViewRefusal).
// The same-element alias conversion of an UNNAMED array, `(*[N]T)(unsafe.Pointer(p))` with p a `*T`, keeps
// `array<T>.AliasPointer`, which already windows p's own storage (arrayPointerAliasEmission).
// The open form of an UNNAMED array (`(*[4]byte)(unsafe.Pointer(&x))[:]`) keeps the box route: it runs on banked
// rows, and its own defect is a separate row (the design's decision D4). And an operand that holds a `FromRef`
// mint keeps the span: that number is an address read under `fixed` for one statement, and a window that
// aliases it would outlive the statement that made the address true (design 4.6).

// arrayPointerView is what a door emission needs of `(*A)(p)`.
type arrayPointerView struct {
	operand ast.Expr // p
	elem    string   // E, as C# spells it
	length  int64    // N
	named   bool     // whether A is a named array type
	elemGo  types.Type
}

// arrayPointerViewOf reads sliceExpr.X as `(*A)(p)` (extra parentheses and an explicit dereference included), p an
// unsafe.Pointer and A an array whose element holds no references. False for every other expression.
func (v *Visitor) arrayPointerViewOf(sliceExpr *ast.SliceExpr) (arrayPointerView, bool) {
	conversion := v.pointerArrayViewConversion(sliceExpr.X)

	if conversion == nil {
		return arrayPointerView{}, false
	}

	// The same-element alias conversion, `(*[N]T)(unsafe.Pointer(p))` with p a `*T`, is already a window on p's own
	// storage (arrayPointerAliasEmission), and banked rows run on it. Sliced, it keeps that route.
	if _, _, alias := v.arrayPointerAliasSource(conversion, conversion.Args[0]); alias {
		return arrayPointerView{}, false
	}

	operandType := v.info.TypeOf(conversion.Args[0])

	if operandType == nil {
		return arrayPointerView{}, false
	}

	// The doors take golib's unsafe.Pointer. A named type over it is a wrapper struct, and a typed pointer operand
	// is the alias conversion, which has its own door.
	if basic, ok := types.Unalias(operandType).(*types.Basic); !ok || basic.Kind() != types.UnsafePointer {
		return arrayPointerView{}, false
	}

	pointer := types.Unalias(v.info.TypeOf(conversion)).Underlying().(*types.Pointer)
	array := types.Unalias(pointer.Elem()).Underlying().(*types.Array)

	if csManagedField(array.Elem(), 0) {
		return arrayPointerView{}, false
	}

	_, named := types.Unalias(pointer.Elem()).(*types.Named)

	return arrayPointerView{
		operand: conversion.Args[0],
		elem:    convertToCSTypeName(v.getAliasQualifiedTypeName(array.Elem(), false)),
		length:  array.Len(),
		named:   named,
		elemGo:  array.Elem(),
	}, true
}

// arrayPointerViewOperand renders p, or reports false where the operand holds a FromRef mint and the site keeps
// the span emission and its warning.
func (v *Visitor) arrayPointerViewOperand(view arrayPointerView) (string, bool) {
	operand := v.convExpr(view.operand, nil)

	if strings.Contains(operand, "FromRef(") {
		return "", false
	}

	return operand, true
}

// arrayPointerViewLow renders the low bound at its full value; Go's omitted low is 0.
func (v *Visitor) arrayPointerViewLow(sliceExpr *ast.SliceExpr) string {
	if sliceExpr.Low == nil {
		return "0"
	}

	return v.castWideIntegerToNint(sliceExpr.Low)
}

// arrayPointerViewDoor renders a slice of `(*A)(p)` as the view door. The bounds travel at their full value: the
// door checks them against N with Go's panics, and refuses by name a length no span can hold.
func (v *Visitor) arrayPointerViewDoor(sliceExpr *ast.SliceExpr) (string, bool) {
	view, ok := v.arrayPointerViewOf(sliceExpr)

	if !ok || sliceExpr.High == nil && !view.named {
		return "", false
	}

	operand, ok := v.arrayPointerViewOperand(view)

	if !ok {
		return "", false
	}

	low := v.arrayPointerViewLow(sliceExpr)

	switch {
	case sliceExpr.Slice3:
		return fmt.Sprintf("@unsafe.Pointer.View<%s>(%s, %s, %s, %s, %d)", view.elem, operand, low,
			v.castWideIntegerToNint(sliceExpr.High), v.castWideIntegerToNint(sliceExpr.Max), view.length), true
	case sliceExpr.High != nil:
		return fmt.Sprintf("@unsafe.Pointer.View<%s>(%s, %s, %s, %d)", view.elem, operand, low,
			v.castWideIntegerToNint(sliceExpr.High), view.length), true
	}

	// The open form away from copy is the whole array from lo. N is a constant, so an array no slice can hold is
	// known here: the door refuses it by name at run time, and this says so at conversion time.
	if view.length > maxManagedSliceLength {
		v.showWarning("the open slice %s of an array of %d elements has no slice this runtime can hold: it is refused by name at run time (it is aliased only as a direct argument of copy)",
			v.getPrintedNode(sliceExpr), view.length)
	}

	return fmt.Sprintf("@unsafe.Pointer.View<%s>(%s, %s, %d)", view.elem, operand, low, view.length), true
}

// maxManagedSliceLength is .NET's Array.MaxLength, the longest span: what golib's GoSliceLimit refuses past.
const maxManagedSliceLength = 0x7FFFFFC7

// arrayPointerCopyDoor renders a builtin copy whose destination or source is the OPEN form of a named array's
// view, `(*A)(p)[lo:]`, as the copy doors. Go copies min(N-lo, len(other)) elements, so the door cuts a view of
// exactly that many, whatever N is: `(*RawMem)(p)[:]` has a length of 1<<50 - 1 that nothing can span, and every
// real use of it is this one. The arguments keep Go's order of evaluation.
//
// The other operand is a slice of the array's element type, or a string for a byte destination; any other pairing
// keeps the general emission, where the open form is the view door's.
func (v *Visitor) arrayPointerCopyDoor(callExpr *ast.CallExpr) (string, bool) {
	ident, ok := ast.Unparen(callExpr.Fun).(*ast.Ident)

	if !ok || len(callExpr.Args) != 2 {
		return "", false
	}

	if builtin, ok := v.info.Uses[ident].(*types.Builtin); !ok || builtin.Name() != "copy" {
		return "", false
	}

	open := func(arg ast.Expr) (*ast.SliceExpr, arrayPointerView, bool) {
		sliceExpr, ok := ast.Unparen(arg).(*ast.SliceExpr)

		if !ok || sliceExpr.High != nil || sliceExpr.Slice3 {
			return nil, arrayPointerView{}, false
		}

		view, ok := v.arrayPointerViewOf(sliceExpr)

		return sliceExpr, view, ok && view.named
	}

	if sliceExpr, view, ok := open(callExpr.Args[0]); ok {
		source := v.info.TypeOf(callExpr.Args[1])
		sliceSource := arrayPointerCopyPeer(source, view.elemGo)

		if !sliceSource && !arrayPointerCopyStringSource(source, view.elemGo) {
			return "", false
		}

		operand, ok := v.arrayPointerViewOperand(view)

		if !ok {
			return "", false
		}

		low := v.arrayPointerViewLow(sliceExpr)
		src := v.convExpr(callExpr.Args[1], nil)

		if sliceSource {
			return fmt.Sprintf("@unsafe.Pointer.CopyInto<%s>(%s, %s, %d, %s)", view.elem, operand, low, view.length, src), true
		}

		return fmt.Sprintf("@unsafe.Pointer.CopyInto(%s, %s, %d, %s)", operand, low, view.length, src), true
	}

	if sliceExpr, view, ok := open(callExpr.Args[1]); ok {
		if !arrayPointerCopyPeer(v.info.TypeOf(callExpr.Args[0]), view.elemGo) {
			return "", false
		}

		dst := v.convExpr(callExpr.Args[0], nil)
		operand, ok := v.arrayPointerViewOperand(view)

		if !ok {
			return "", false
		}

		return fmt.Sprintf("@unsafe.Pointer.CopyFrom<%s>(%s, %s, %s, %d)", view.elem, dst, operand, v.arrayPointerViewLow(sliceExpr), view.length), true
	}

	return "", false
}

// arrayPointerCopyPeer reports whether t is an unnamed slice of elem: the operand the generic copy doors take.
func arrayPointerCopyPeer(t, elem types.Type) bool {
	if t == nil {
		return false
	}

	slice, ok := types.Unalias(t).(*types.Slice)

	return ok && types.Identical(slice.Elem(), elem)
}

// arrayPointerCopyStringSource reports whether t is a string copied into bytes, Go's one mixed copy.
func arrayPointerCopyStringSource(t, elem types.Type) bool {
	if t == nil {
		return false
	}

	source, ok := types.Unalias(t).(*types.Basic)

	if !ok || source.Info()&types.IsString == 0 {
		return false
	}

	target, ok := types.Unalias(elem).(*types.Basic)

	return ok && target.Kind() == types.Uint8
}
