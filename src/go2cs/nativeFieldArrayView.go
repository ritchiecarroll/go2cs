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
// So the conversion emits the unsafe package's helper over the field reference, naming the field ONCE as
// the Go line does:
//
//	@unsafe.ArrayPointer<T>.Of(Ꮡp.of(S.Ꮡf), N)
//
// It carries both routes. A native root takes golib's door, which resolves f's GO offset from the struct's
// Go layout at run time; every other root takes the raw-address route this conversion emitted before, byte
// for byte. The offset is NOT folded here: a Go offset is a property of the target, and a literal makes
// one source file emit three ways — runtime's m.cheaprand sits at 1648, 1688 and 1752 across linux,
// windows and darwin, which split the shared runtime/rand.cs into three per-flavour copies when this door
// first folded it. The helper lives in the unsafe package because golib cannot name Pointer.FromPinnedBox,
// the raw route's retaining pin; an earlier form spelled both routes at the call site and so named the
// field twice. And because a native array pointer has no array<T> for Value to return, a local bound by
// the door and never re-pointed reads `p[i]` through ElementRef, which indexes a native array in place and
// falls back to `ref p.Value[i]` for every other box. The view indexed where it is written,
// `(*[N]T)(unsafe.Pointer(&p.f))[i]`, reads through ElementRef too (convIndexExpr.go): it went through Value,
// which the view refuses on every root (docs/phase4/DESIGN-array-pointer-doors.md, section 18).
//
// SCOPE: the address of a field reached by one or more field selections off a variable (`&p.f`,
// `&sa.raw.Port`, `&s.f`, `&global.f`), a numeric element type, and a different element type than the
// field's (the same-type view stays array<T>.AliasPointer). Anything else keeps the raw route. It was ONE
// field hop off a pointer-typed identifier, the one site class the first census found (12 sites across 5
// converted files), until docs/phase4/DESIGN-array-pointer-doors.md, section 19: golang.org/x/sys writes
// a port through `&sa.raw.Port`, two selections deep, and the raw route's pointer over such a field has no
// array behind its Value on any root.

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

	if !v.isFieldSelectionChainOffVariable(unary.X) {
		return nativeFieldArrayView{}, false
	}

	return nativeFieldArrayView{field: unary, elem: targetArr.Elem(), length: targetArr.Len()}, true
}

// isFieldSelectionChainOffVariable reports whether expr is one or more field selections, each of a field
// declared in the type it is selected from, off a variable: `p.f`, `sa.raw.Port`, `s.f` for a struct variable,
// `global.f`. That is every shape whose address the converter already emits as the field's own pointer
// (`p.Ꮡf`, `Ꮡsa.Ꮡraw.ᏑPort`, `Ꮡs.Ꮡf`), which is what the helper takes. A promoted field (a selection that
// passes through an embedded field), a root that is not an identifier (an index, a call, a dereference
// written out) and a package-qualified name keep the raw route.
func (v *Visitor) isFieldSelectionChainOffVariable(expr ast.Expr) bool {
	sel, ok := ast.Unparen(expr).(*ast.SelectorExpr)

	if !ok {
		return false
	}

	for {
		selection := v.info.Selections[sel]

		if selection == nil || selection.Kind() != types.FieldVal || len(selection.Index()) != 1 {
			return false
		}

		inner, ok := ast.Unparen(sel.X).(*ast.SelectorExpr)

		if !ok {
			break
		}

		sel = inner
	}

	root, ok := ast.Unparen(sel.X).(*ast.Ident)

	if !ok {
		return false
	}

	_, ok = v.info.Uses[root].(*types.Var)
	return ok
}

// nativeFieldArrayViewEmission renders the unsafe package's helper, which carries the door and the raw
// route it falls back to.
func (v *Visitor) nativeFieldArrayViewEmission(view nativeFieldArrayView) string {
	elemName := convertToCSTypeName(v.getAliasQualifiedTypeName(view.elem, false))

	return fmt.Sprintf("@unsafe.ArrayPointer<%s>.Of(%s, %s)", elemName, v.convExpr(view.field, nil), csNintLiteral(view.length))
}

// isNativeFieldArrayViewConversion reports whether expr, parentheses aside, is the field-view conversion
// itself, `(*[N]T)(unsafe.Pointer(&p.f))`.
func (v *Visitor) isNativeFieldArrayViewConversion(expr ast.Expr) bool {
	call, ok := ast.Unparen(expr).(*ast.CallExpr)

	if !ok || len(call.Args) != 1 {
		return false
	}

	_, ok = v.nativeFieldArrayViewOf(call, call.Args[0])
	return ok
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
	isView := v.isNativeFieldArrayViewConversion

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

// THE WHOLE ARRAY THROUGH THE VIEW (docs/phase4/DESIGN-array-pointer-doors.md, section 18, part C)
//
// The view has no array<T> for Value to return, so a form that needs all N elements goes through the elements:
//
//	v := *p             @unsafe.ArrayPointer<T>.Load(p)        read for its value: a copy
//	*p = v              @unsafe.ArrayPointer<T>.Store(p, v)
//	for _, x := range p     range(p.slice())                   each element read when its round comes
//	(*p)[i]             p.ElementRef(i)                        no copy: the view's own element
//
//	(*p)[lo:hi]         p.slice(lo, hi)                        the view's own window (convSliceExpr.go)
//	len(*p), range *p for its index alone                      N: Go reads no memory for either
//
// p is the conversion where it is written or a local bound by it and never re-pointed. The value positions are
// packageFieldViewLoads'. A dereference anywhere else keeps `p.Value`, which the view refuses by name: the left
// side of a tuple assignment is the one such position a probe found.

// packageFieldViewLoads marks each dereference that stands where Go copies its value, for the field view. Its
// list is packageArrayPointerLoads' and three positions more: an element of a composite literal (a struct
// literal's field value is how x/sys/windows writes the form), a sent value, and any of them inside parentheses.
// The view has no general emission to fall back on, so a value position the list misses is a refusal at run
// time. It is rebuilt with packageArrayPointerLoads, in the same walk.
var packageFieldViewLoads map[*ast.StarExpr]bool

// starExprIsFieldViewLoad reports whether a dereference stands in one of packageFieldViewLoads' positions.
// ancestors is the walk's stack above the dereference, nearest last.
func starExprIsFieldViewLoad(star *ast.StarExpr, ancestors []ast.Node, info *types.Info) bool {
	var operand ast.Expr = star
	at := len(ancestors) - 1

	for ; at >= 0; at-- {
		paren, ok := ancestors[at].(*ast.ParenExpr)

		if !ok {
			break
		}

		operand = paren
	}

	if at < 0 {
		return false
	}

	switch parent := ancestors[at].(type) {
	case *ast.CompositeLit:
		for _, elt := range parent.Elts {
			if elt == operand {
				return true
			}
		}

		return false
	case *ast.KeyValueExpr:
		return parent.Key == operand || parent.Value == operand
	case *ast.SendStmt:
		return parent.Value == operand
	}

	return starExprIsValueOperand(operand, ancestors[at], info)
}

// nativeFieldArrayViewDerefIsUnevaluated reports whether expr, parentheses aside, is the dereference of a field
// view with no call or receive inside it: `len` of it and a range for its index alone are the array type's
// length, and Go evaluates nothing.
func (v *Visitor) nativeFieldArrayViewDerefIsUnevaluated(expr ast.Expr) bool {
	star, ok := ast.Unparen(expr).(*ast.StarExpr)

	if !ok || v.exprHasCallOrReceive(expr) {
		return false
	}

	_, isView := v.nativeFieldArrayViewPointer(star.X)

	return isView
}

// nativeFieldArrayViewPointer reports whether x, parentheses aside, is a pointer the field view answered: the
// conversion itself, or a local bound by it and never re-pointed. It answers the C# name of the element type.
func (v *Visitor) nativeFieldArrayViewPointer(x ast.Expr) (string, bool) {
	if !v.isNativeFieldArrayViewConversion(x) && !v.isNativeFieldArrayViewLocal(x) {
		return "", false
	}

	ptr, ok := v.info.TypeOf(x).Underlying().(*types.Pointer)

	if !ok {
		return "", false
	}

	arr, ok := types.Unalias(ptr.Elem()).(*types.Array)

	if !ok {
		return "", false
	}

	return convertToCSTypeName(v.getAliasQualifiedTypeName(arr.Elem(), false)), true
}

// nativeFieldArrayViewLoad renders `*p` read for its value, p a field view, as the load helper: a new Go array
// value, copied element by element from the view.
func (v *Visitor) nativeFieldArrayViewLoad(star *ast.StarExpr) (string, bool) {
	if !packageFieldViewLoads[star] {
		return "", false
	}

	elem, ok := v.nativeFieldArrayViewPointer(star.X)

	if !ok {
		return "", false
	}

	return fmt.Sprintf("@unsafe.ArrayPointer<%s>.Load(%s)", elem, v.convExpr(star.X, nil)), true
}

// nativeFieldArrayViewLoadIsOwnCopy reports whether expr is a dereference the load helper renders. Its answer
// is already a copy, so the value-copy clone has nothing to add to it.
func (v *Visitor) nativeFieldArrayViewLoadIsOwnCopy(expr ast.Expr) bool {
	star, ok := ast.Unparen(expr).(*ast.StarExpr)

	if !ok || !packageFieldViewLoads[star] {
		return false
	}

	_, isView := v.nativeFieldArrayViewPointer(star.X)

	return isView
}

// nativeFieldArrayViewStore renders the statement `*p = v`, p a field view, as the store helper. Go evaluates
// the pointer before the value, and the arguments keep that order.
func (v *Visitor) nativeFieldArrayViewStore(lhs ast.Expr, rhs ast.Expr) (string, bool) {
	star, ok := ast.Unparen(lhs).(*ast.StarExpr)

	if !ok {
		return "", false
	}

	elem, ok := v.nativeFieldArrayViewPointer(star.X)

	if !ok {
		return "", false
	}

	return fmt.Sprintf("@unsafe.ArrayPointer<%s>.Store(%s, %s);", elem, v.convExpr(star.X, nil), v.convExpr(rhs, nil)), true
}

// nativeFieldArrayViewRangeWindow renders the operand of a range that reads the values of a field view, the
// pointer itself and not its dereference, as the view's window. Go reads each element when its round comes, and
// so does a range over the window; a range over the dereference copies first, and takes the load helper.
func (v *Visitor) nativeFieldArrayViewRangeWindow(rangeStmt *ast.RangeStmt) (string, types.Type, bool) {
	if !rangeReadsValues(rangeStmt) {
		return "", nil, false
	}

	if _, ok := v.nativeFieldArrayViewPointer(rangeStmt.X); !ok {
		return "", nil, false
	}

	arr := types.Unalias(v.info.TypeOf(rangeStmt.X).Underlying().(*types.Pointer).Elem()).(*types.Array)

	return v.convExpr(rangeStmt.X, nil) + ".slice()", arr.Elem(), true
}
