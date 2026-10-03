// copyingSliceViewNote.go - Gbtc
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

// THE COPYING SLICE VIEW, MADE AUDIBLE.
//
// convSliceExpr lowers a pointer-to-array cast that is then sliced -- `(*[N]T)(ptr)[lo:hi]`, where the
// element type differs from the source's or the source is a raw pointer -- to
// `new slice<T>(new ReadOnlySpan<T>((T*)…, n))`, and golib's span constructor COPIES. A site that only
// reads the slice reads exactly what Go reads. A site that writes through it writes the copy, and Go's
// store never reaches the memory the view was taken over. The emission is unchanged (no aliasing door
// exists for a managed element over arbitrary words); what this adds is a conversion-time note naming
// both lines, so the lost write is said at conversion time instead of discovered at run time.
//
// Census at the TRAIN M union (BOARD, 2026-10-02, "the copying slice view"): 14 emissions in src/core,
// 3 writers -- runtime/iface.go's itabInit, runtime/heapdump.go's makeheapobjbv and
// runtime/mbitmap.go's progToPointerMask -- none reachable from executed code today.

// noteCopyingSliceViewWrite reports when the slice the copying branch produces for sliceExpr is
// written through in the same function: an element store or op-assign, an increment, or the
// destination of the copy or clear builtins -- through the slice itself, a reslice of it, or the
// variable it is assigned to.
func (v *Visitor) noteCopyingSliceViewWrite(sliceExpr *ast.SliceExpr) {
	if v.currentFuncDecl == nil || v.currentFuncDecl.Body == nil {
		return
	}

	body := v.currentFuncDecl.Body
	var view types.Object

	// The variable the view lands in, when it lands in one: `x := view`, `x = view`, `var x = view`.
	ast.Inspect(body, func(node ast.Node) bool {
		if view != nil {
			return false
		}

		switch node := node.(type) {
		case *ast.AssignStmt:
			for i, rhs := range node.Rhs {
				if unparenthesize(rhs) == sliceExpr && i < len(node.Lhs) {
					if ident, ok := unparenthesize(node.Lhs[i]).(*ast.Ident); ok {
						view = v.info.ObjectOf(ident)
					}
				}
			}
		case *ast.ValueSpec:
			for i, value := range node.Values {
				if unparenthesize(value) == sliceExpr && i < len(node.Names) {
					view = v.info.ObjectOf(node.Names[i])
				}
			}
		}

		return true
	})

	// Whether expr is the view: the slice expression itself, a reslice or parenthesization of it, or
	// the variable it was assigned to.
	isView := func(expr ast.Expr) bool {
		for {
			switch e := expr.(type) {
			case *ast.ParenExpr:
				expr = e.X
			case *ast.SliceExpr:
				if e == sliceExpr {
					return true
				}

				expr = e.X
			case *ast.Ident:
				return view != nil && v.info.ObjectOf(e) == view
			default:
				return false
			}
		}
	}

	storesThrough := func(target ast.Expr) bool {
		index, ok := unparenthesize(target).(*ast.IndexExpr)
		return ok && isView(index.X)
	}

	var write token.Pos

	ast.Inspect(body, func(node ast.Node) bool {
		if write.IsValid() {
			return false
		}

		switch node := node.(type) {
		case *ast.AssignStmt:
			if node.Tok == token.DEFINE {
				return true
			}

			for _, lhs := range node.Lhs {
				if storesThrough(lhs) {
					write = node.Pos()
				}
			}
		case *ast.IncDecStmt:
			if storesThrough(node.X) {
				write = node.Pos()
			}
		case *ast.CallExpr:
			ident, ok := unparenthesize(node.Fun).(*ast.Ident)

			if !ok || len(node.Args) == 0 {
				return true
			}

			if builtin, ok := v.info.Uses[ident].(*types.Builtin); ok && (builtin.Name() == "copy" || builtin.Name() == "clear") && isView(node.Args[0]) {
				write = node.Pos()
			}
		}

		return true
	})

	if !write.IsValid() {
		return
	}

	v.showWarning("a slice of a pointer-to-array cast at line %d COPIES the memory it views, and it is written through at line %d: that write does not reach the original",
		v.fset.Position(sliceExpr.Pos()).Line, v.fset.Position(write).Line)
}
