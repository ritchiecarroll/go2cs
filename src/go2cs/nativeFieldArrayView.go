// nativeFieldArrayView.go - Gbtc
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

// THE NATIVE FIELD-VIEW DOOR (docs/phase4/DESIGN-native-array-view.md, the LookupServicePort door)
//
// Go's `(*[N]T)(unsafe.Pointer(&p.f))`, where f's type is not T, views the bytes of ONE field. When p
// points at NATIVE memory — net's darwin cgoLookupServicePort reads the port of a sockaddr libc
// returned — the raw-address route fails twice: the floor refuses an array view of native memory by
// name, and the address it would view is the field's CLR slot, base plus the field's CLR OFFSET. A
// converted struct carrying an array<> field holds managed references, so the CLR lays it out
// automatically and that offset is not Go's (measured: Port at CLR offset 0, Go offset 2), which turns
// a view-only fix into a silent wrong port.
//
// So the conversion emits golib's door ahead of the raw route, handing it the field reference itself —
// the same `Ꮡp.of(S.Ꮡf)` the raw route pins:
//
//	(NativeFieldArrayPointer<T>(Ꮡp.of(S.Ꮡf), N) ?? <the raw route>)
//
// golib resolves f's GO offset from the struct's Go layout at run time. It is NOT folded here: a Go offset
// is a property of the target, and a literal makes one source file emit three ways — runtime's
// m.cheaprand sits at 1648, 1688 and 1752 across linux, windows and darwin, which split the shared
// runtime/rand.cs into three per-flavour copies when this door first folded it. The door answers only for
// a native root; a managed or nil root answers null and keeps today's route byte for byte. And because a native array pointer has no array<T> for Value to return, a local bound
// by the door and never re-pointed reads `p[i]` through ElementRef, which indexes a native array in
// place and falls back to `ref p.Value[i]` for every other box.
//
// SCOPE, kept to the one site class the census found (12 sites across 5 converted files): ONE field
// hop off a pointer-typed identifier, a numeric element type, and a different element type than the
// field's (the same-type view stays array<T>.AliasPointer). Anything else keeps the raw route.

// nativeFieldArrayView is the one shape the door serves.
type nativeFieldArrayView struct {
	field  ast.Expr   // the field address, `&p.f`
	elem   types.Type // the array's element type
	length int64      // the array's N
}

// nativeFieldArrayViewOf reports whether a `(*[N]T)(…)` conversion is the native field-view shape.
func (v *Visitor) nativeFieldArrayViewOf(callExpr *ast.CallExpr, arg ast.Expr) (nativeFieldArrayView, bool) {
	p, srcPtr, targetPtr := v.pointerConversionSource(callExpr, arg)

	if p == nil {
		return nativeFieldArrayView{}, false
	}

	targetArr, ok := types.Unalias(targetPtr.Elem()).(*types.Array)

	if !ok || types.Identical(srcPtr.Elem(), targetArr.Elem()) {
		return nativeFieldArrayView{}, false
	}

	if basic, ok := targetArr.Elem().Underlying().(*types.Basic); !ok || basic.Info()&types.IsNumeric == 0 {
		return nativeFieldArrayView{}, false
	}

	unary, ok := ast.Unparen(p).(*ast.UnaryExpr)

	if !ok || unary.Op != token.AND {
		return nativeFieldArrayView{}, false
	}

	sel, ok := ast.Unparen(unary.X).(*ast.SelectorExpr)

	if !ok {
		return nativeFieldArrayView{}, false
	}

	selection := v.info.Selections[sel]

	if selection == nil || selection.Kind() != types.FieldVal || len(selection.Index()) != 1 {
		return nativeFieldArrayView{}, false
	}

	root, ok := ast.Unparen(sel.X).(*ast.Ident)

	if !ok {
		return nativeFieldArrayView{}, false
	}

	if _, ok := v.info.TypeOf(root).Underlying().(*types.Pointer); !ok {
		return nativeFieldArrayView{}, false
	}

	return nativeFieldArrayView{field: unary, elem: targetArr.Elem(), length: targetArr.Len()}, true
}

// nativeFieldArrayViewEmission renders the door ahead of the raw route it falls back to.
func (v *Visitor) nativeFieldArrayViewEmission(view nativeFieldArrayView, rawRoute string) string {
	elemName := convertToCSTypeName(v.getAliasQualifiedTypeName(view.elem, false))

	return fmt.Sprintf("(NativeFieldArrayPointer<%s>(%s, %s) ?? %s)",
		elemName, v.convExpr(view.field, nil), csNintLiteral(view.length), rawRoute)
}

// isNativeFieldArrayViewLocal reports whether an index base is a local bound by the door and never
// re-pointed, so `p[i]` may read through ElementRef. A local that is re-pointed or whose address is
// taken may later name a managed array, which `p.Value[i]` already serves, so it keeps that form.
func (v *Visitor) isNativeFieldArrayViewLocal(x ast.Expr) bool {
	ident, ok := ast.Unparen(x).(*ast.Ident)

	if !ok || v.currentFuncDecl == nil || v.currentFuncDecl.Body == nil {
		return false
	}

	obj, ok := v.info.Uses[ident].(*types.Var)

	if !ok {
		return false
	}

	if bound, found := v.nativeFieldViewLocalCache[obj]; found {
		return bound
	}

	bound := v.nativeFieldArrayViewBinds(obj) && !v.refRootIsReassigned(obj) && !v.identAddressTaken(obj)

	if v.nativeFieldViewLocalCache == nil {
		v.nativeFieldViewLocalCache = make(map[types.Object]bool)
	}

	v.nativeFieldViewLocalCache[obj] = bound
	return bound
}

// nativeFieldArrayViewBinds reports whether obj's declaration in the current function binds it to the
// native field-view shape — `p := (*[N]T)(unsafe.Pointer(&x.f))` or `var p = …`.
func (v *Visitor) nativeFieldArrayViewBinds(obj types.Object) bool {
	binds := false

	isView := func(expr ast.Expr) bool {
		call, ok := ast.Unparen(expr).(*ast.CallExpr)

		if !ok || len(call.Args) != 1 {
			return false
		}

		_, ok = v.nativeFieldArrayViewOf(call, call.Args[0])
		return ok
	}

	ast.Inspect(v.currentFuncDecl.Body, func(n ast.Node) bool {
		if binds {
			return false
		}

		switch node := n.(type) {
		case *ast.AssignStmt:
			if node.Tok != token.DEFINE || len(node.Lhs) != len(node.Rhs) {
				return true
			}

			for i, lhs := range node.Lhs {
				if ident, ok := lhs.(*ast.Ident); ok && v.info.Defs[ident] == obj && isView(node.Rhs[i]) {
					binds = true
				}
			}
		case *ast.ValueSpec:
			if len(node.Names) != len(node.Values) {
				return true
			}

			for i, name := range node.Names {
				if v.info.Defs[name] == obj && isView(node.Values[i]) {
					binds = true
				}
			}
		}

		return true
	})

	return binds
}
