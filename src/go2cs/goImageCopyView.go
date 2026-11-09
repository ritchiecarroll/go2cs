// goImageCopyView.go - Gbtc
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

// THE GO-IMAGE COPY VIEW (docs/phase4/DESIGN-reinterpret-seam.md, COORD ruling (A))
//
// Go's record-decoding idiom views a struct's bytes and copies through the view:
//
//	copy((*[unsafe.Sizeof(d)]byte)(unsafe.Pointer(&d))[:], data)
//
// When the struct is REFERENCE-BEARING (a fixed array inside it is a managed array<T>, or it holds a reference),
// its box has no address, the uintptr route hands out an order token, and the first dereference of the view
// panics (arm 2a). golib's GoImage<S> copies through the struct's Go-layout image instead, so a slice of such a
// view that is a DIRECT argument of copy emits as
//
//	copy(@unsafe.GoImage(Ꮡd).slice(), data)
//
// keeping copy at the site. The shape, kept to what the census found (one site, syscall's darwin
// getdirentries_test.go:67):
//   - the slice's operand is `(*[N]E)(unsafe.Pointer(P))`, with E byte and N equal to unsafe.Sizeof(*P);
//   - *P is a struct that is reference-bearing in go2cs's model;
//   - the slice expression is an argument of the builtin copy itself.
// Every other use of such a view (stored in a variable, indexed, another element type or length) keeps the route
// it had, whose dereference refuses by name.

// markGoImageCopyArgs records the arguments of a builtin copy call that take the Go-image route, before the call's
// arguments are converted.
func (v *Visitor) markGoImageCopyArgs(callExpr *ast.CallExpr) {
	ident, ok := ast.Unparen(callExpr.Fun).(*ast.Ident)

	if !ok || len(callExpr.Args) != 2 {
		return
	}

	if builtin, ok := v.info.Uses[ident].(*types.Builtin); !ok || builtin.Name() != "copy" {
		return
	}

	for _, arg := range callExpr.Args {
		sliceExpr, ok := ast.Unparen(arg).(*ast.SliceExpr)

		if !ok {
			continue
		}

		if _, ok := v.goImageViewOperand(sliceExpr); ok {
			if v.goImageCopySlices == nil {
				v.goImageCopySlices = map[*ast.SliceExpr]bool{}
			}

			v.goImageCopySlices[sliceExpr] = true
		}
	}
}

// goImageViewOperand reports P when the slice's operand is the Go-image view shape `(*[N]byte)(unsafe.Pointer(P))`.
func (v *Visitor) goImageViewOperand(sliceExpr *ast.SliceExpr) (ast.Expr, bool) {
	conversion, ok := ast.Unparen(sliceExpr.X).(*ast.CallExpr)

	if !ok || len(conversion.Args) != 1 {
		return nil, false
	}

	p, srcPtr, targetPtr := v.pointerConversionSource(conversion, conversion.Args[0])

	// Only through an unsafe.Pointer conversion: a direct (*U)(p) is a same-underlying-type conversion, never a view.
	if p == nil || p == conversion.Args[0] {
		return nil, false
	}

	targetArray, ok := types.Unalias(targetPtr.Elem()).Underlying().(*types.Array)

	if !ok {
		return nil, false
	}

	if basic, ok := targetArray.Elem().Underlying().(*types.Basic); !ok || basic.Kind() != types.Uint8 {
		return nil, false
	}

	pointee := srcPtr.Elem()

	if _, ok := pointee.Underlying().(*types.Struct); !ok || !goImageReferenceBearing(pointee, map[types.Type]bool{}) {
		return nil, false
	}

	sizes := types.SizesFor("gc", platformArch(v.options.targetPlatform))

	if sizes == nil {
		sizes = types.SizesFor("gc", "amd64")
	}

	if targetArray.Len() != sizes.Sizeof(pointee) {
		return nil, false
	}

	return p, true
}

// goImageReferenceBearing reports whether a Go type holds a managed reference in go2cs's model: a fixed array (an
// array<T> holds its T[]), a string, or any pointer, slice, map, channel, func, interface or unsafe.Pointer.
func goImageReferenceBearing(t types.Type, seen map[types.Type]bool) bool {
	if seen[t] {
		return false
	}

	seen[t] = true

	switch u := t.Underlying().(type) {
	case *types.Basic:
		return u.Kind() == types.String || u.Kind() == types.UnsafePointer
	case *types.Array:
		return true
	case *types.Struct:
		for i := 0; i < u.NumFields(); i++ {
			if goImageReferenceBearing(u.Field(i).Type(), seen) {
				return true
			}
		}

		return false
	default:
		return true
	}
}

// goImageCopySlice emits a marked slice as a window of the Go-image view, or reports false for any other slice.
func (v *Visitor) goImageCopySlice(sliceExpr *ast.SliceExpr) (string, bool) {
	if !v.goImageCopySlices[sliceExpr] {
		return "", false
	}

	p, _ := v.goImageViewOperand(sliceExpr)
	view := fmt.Sprintf("@unsafe.GoImage(%s)", v.convExpr(p, nil))
	bound := func(expr ast.Expr) string {
		text := v.convExpr(expr, nil)

		if tv, ok := v.info.Types[expr]; ok && tv.Value == nil {
			if basic, ok := tv.Type.Underlying().(*types.Basic); !ok || basic.Kind() != types.Int {
				return "(nint)(" + text + ")"
			}
		}

		return text
	}

	low := "0"

	if sliceExpr.Low != nil {
		low = bound(sliceExpr.Low)
	}

	switch {
	case sliceExpr.Slice3:
		return fmt.Sprintf("%s.slice(%s, %s, %s)", view, low, bound(sliceExpr.High), bound(sliceExpr.Max)), true
	case sliceExpr.High != nil:
		return fmt.Sprintf("%s.slice(%s, %s)", view, low, bound(sliceExpr.High)), true
	case sliceExpr.Low != nil:
		return fmt.Sprintf("%s.slice(%s)", view, low), true
	default:
		return view + ".slice()", true
	}
}
