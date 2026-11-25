// inlineArrayEmission.go - Gbtc
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
	"go/constant"
	"go/token"
	"go/types"
	"path/filepath"
	"strings"
)

// INLINE-ARRAY EMISSION (I0 part D3; docs/phase4/DESIGN-array-value-storage.md, 7.7.4)
//
// An array field of a struct the admission admits (inlineArrayAdmission.go) is a go.arrays member, array<N><T>, which
// exposes an element indexer, its Length, a constructor from an element list (a Go array literal) and the copy a range
// iterates (ΔRangeSnapshot), and nothing else. The declaration is the layout arc's inline mode (zeroSizeFieldLayout.go);
// the uses the admission's face rule admits are spelled here and where each one is emitted:
//
//   - len and cap: the constant (convCallExpr.go), as golib's len cannot see the family;
//   - an index-only range: range(N) (visitRangeStmt.go); a value range keeps range(x.ΔRangeSnapshot());
//   - an element: the indexer, unchanged;
//   - an array literal that initializes the field: new array<N><T>(e0, e1, ...) (convCompositeLit.go);
//   - the whole array boxed into an interface parameter: its deep array<T> copy (inlineArrayBoxCopy).

// isInlineArrayExpr reports whether expr denotes an array held inline: an array field of an admitted struct (this
// package's, or one another package records as admitted: C1's review, m3), or an array element of one (an admitted
// closure holds every array inline, nested ones included).
func (v *Visitor) isInlineArrayExpr(expr ast.Expr) bool {
	return isInlineArrayExprIn(expr, v.info)
}

// isInlineArrayExprIn is isInlineArrayExpr for a pass that holds the package's types.Info but no visitor (the
// ref-lowering analysis).
func isInlineArrayExprIn(expr ast.Expr, info *types.Info) bool {
	if packageInlineAdmission == nil {
		return false
	}

	switch e := ast.Unparen(expr).(type) {
	case *ast.SelectorExpr:
		owner, _, ok := arrayFieldOwner(e, info)
		return ok && inlineArrayHeld(owner)
	case *ast.IndexExpr:
		if _, isArray := types.Unalias(info.TypeOf(e)).Underlying().(*types.Array); isArray {
			return isInlineArrayExprIn(e.X, info)
		}
	}

	return false
}

// markInlineArrayLiterals marks, in a struct literal of an admitted type (this package's or another's), each array
// literal that initializes one of its nonzero array fields (keyed by field name or by position).
func (v *Visitor) markInlineArrayLiterals(lit *ast.CompositeLit) {
	if !inlineArrayHeld(v.info.TypeOf(lit)) {
		return
	}

	st, ok := types.Unalias(v.info.TypeOf(lit)).Underlying().(*types.Struct)

	if !ok {
		return
	}

	for i, elt := range lit.Elts {
		field, value := structLitField(st, i, elt)

		if field < 0 {
			continue
		}

		array, isArray := st.Field(field).Type().Underlying().(*types.Array)
		valueLit, isLit := ast.Unparen(value).(*ast.CompositeLit)

		if isArray && array.Len() > 0 && isLit {
			if v.inlineArrayLiterals == nil {
				v.inlineArrayLiterals = map[*ast.CompositeLit]*types.Array{}
			}

			v.inlineArrayLiterals[valueLit] = array
		}
	}
}

// structLitField maps the i-th element of a struct literal of type st to the field it initializes (keyed by field name
// or by position) and the value it stores there, or -1 when the element names no field of st. The emitter and the
// admission's face rule both read a literal through it, so they agree on which field an element initializes.
func structLitField(st *types.Struct, i int, elt ast.Expr) (int, ast.Expr) {
	if kv, ok := elt.(*ast.KeyValueExpr); ok {
		if id, ok := kv.Key.(*ast.Ident); ok {
			for j := range st.NumFields() {
				if st.Field(j).Name() == id.Name {
					return j, kv.Value
				}
			}
		}

		return -1, kv.Value
	}

	if i >= st.NumFields() {
		return -1, elt
	}

	return i, elt
}

// inlineArrayLiteral renders a marked array literal as the go.arrays member's constructor over its elements, Go's zero
// filling the rest. Only a positional literal is marked into this form; a keyed one keeps the array<T> rendering, which
// does not convert to the member, so it is a C# compile error rather than a silent copy (S4).
func (v *Visitor) inlineArrayLiteral(lit *ast.CompositeLit) (string, bool) {
	array, ok := v.inlineArrayLiterals[lit]

	if !ok {
		return "", false
	}

	v.addRequiredUsing("go.arrays")

	// A nested array literal is itself an inline array (every array in an admitted closure is).
	if inner, isArray := array.Elem().Underlying().(*types.Array); isArray && inner.Len() > 0 {
		for _, elt := range lit.Elts {
			if kv, isKeyed := elt.(*ast.KeyValueExpr); isKeyed {
				elt = kv.Value
			}

			if elemLit, isLit := ast.Unparen(elt).(*ast.CompositeLit); isLit {
				v.inlineArrayLiterals[elemLit] = inner
			}
		}
	}

	if len(lit.Elts) == 0 {
		return fmt.Sprintf("new %s()", v.inlineArrayTypeName(array)), true
	}

	if !compositeLitIsKeyed(lit.Elts) {
		return fmt.Sprintf("new %s(%s)", v.inlineArrayTypeName(array), v.convExprList(lit.Elts, lit.Lbrace, v.withValueCloneArgs(lit.Elts, v.elidedElemContext(array.Elem(), lit.Elts)))), true
	}

	// A KEYED literal (C1's review, M3): Go's keys are constants, so each element lands at its position, a positional
	// element after a key takes the next one, the gaps are the element's zero value, and the constructor zero-fills past
	// the last element.
	slots := []ast.Expr{}
	index := int64(0)

	for _, elt := range lit.Elts {
		value := elt

		if kv, isKeyed := elt.(*ast.KeyValueExpr); isKeyed {
			key, exact := constant.Int64Val(constant.ToInt(v.info.Types[kv.Key].Value))

			if !exact {
				return "", false
			}

			index, value = key, kv.Value
		}

		for int64(len(slots)) <= index {
			slots = append(slots, nil)
		}

		slots[index] = value
		index++
	}

	elements := make([]string, len(slots))

	for i, value := range slots {
		if value == nil {
			elements[i] = "default"
			continue
		}

		single := []ast.Expr{value}
		elements[i] = v.convExprList(single, lit.Lbrace, v.withValueCloneArgs(single, v.elidedElemContext(array.Elem(), single)))
	}

	return fmt.Sprintf("new %s(%s)", v.inlineArrayTypeName(array), strings.Join(elements, ", ")), true
}

// inlineArrayBoxCopy renders an inline array crossing into an interface as its array<T> copy, deep for a nested array
// (C1's review, M1): x.f.ΔCopy(), x.m.ΔSelect(static Δe1 => Δe1.ΔCopy()), and so on down. Interface space then holds
// the one representation every other Go [N]T has there, so a type switch, interface ==, a map key, reflect and fmt see
// no second form, and the copy aliases nothing the struct holds (Go boxes a copy).
func inlineArrayBoxCopy(rendered string, array *types.Array) string {
	if strings.HasPrefix(rendered, PointerDerefOp) {
		rendered = "(" + rendered + ")"
	}

	return rendered + "." + inlineArrayDeepCopy(array, 1)
}

// inlineArrayDeepCopy is the member call that copies an inline array of type array out, one ΔSelect per nested level.
func inlineArrayDeepCopy(array *types.Array, depth int) string {
	if inner, isArray := array.Elem().Underlying().(*types.Array); isArray && inner.Len() > 0 {
		element := fmt.Sprintf("Δe%d", depth)
		return fmt.Sprintf("ΔSelect(static %s => %s.%s)", element, element, inlineArrayDeepCopy(inner, depth+1))
	}

	return "ΔCopy()"
}

// inlineArrayHeld reports whether t is a struct whose arrays are held inline: one this package admits, or a named struct
// another converted package records as admitted (inlineArrayStamp.go). Such a struct is copied whole by a plain C# struct
// copy, so it takes no value clone.
func inlineArrayHeld(t types.Type) bool {
	a := packageInlineAdmission

	if a == nil || t == nil {
		return false
	}

	if a.admits(t) {
		return true
	}

	if named, ok := types.Unalias(t).(*types.Named); ok && named.Obj().Pkg() != nil && named.Obj().Pkg() != a.pkg {
		if _, isStruct := named.Underlying().(*types.Struct); isStruct {
			in, known := a.foreignAdmits(t)
			return known && in
		}
	}

	return false
}

// inlineArraySliceBase renders the base of a slice of an inline array, `x.f[lo:hi]` (go-winio's pkg/guid slicing
// windows.GUID's Data4, COORD 2026-11-15, Part 2 ruled (a)): the whole array as a slice that ALIASES the struct's own
// storage, `Ꮡx.of(X.Ꮡf).ΔInlineSlice<E>()`, to which the caller applies the expression's bounds. Go's x.f[lo:hi] is
// (&x.f)[lo:hi], so the base is the field's address, and x is boxed by the address-taken path (escape analysis reads the
// slice as an address of x, inlineArraySliceRoots). golib's ΔInlineSlice pins the box and keeps it for as long as any
// slice cut from it lives (ж.InlineSlice.cs). A value receiver that is sliced is boxed as a COPY, so its slice never
// reaches the caller's struct, as in Go.
//
// Reached only where the admission did not keep the type out: a field another package holds inline, and a local
// defined type over one (inlineArrayAdmission.go's reconcileDefinedTypes), plus forced mode. An element whose C# form
// holds a managed reference cannot be pinned, so it is not spelled (ok is false) and stays a C# compile error, as
// every unspelled face is.
func (v *Visitor) inlineArraySliceBase(array ast.Expr) (string, bool) {
	arrayType, ok := types.Unalias(v.info.TypeOf(array)).Underlying().(*types.Array)

	if !ok || arrayType.Len() == 0 || !inlineSliceElementIsPlain(arrayType.Elem()) {
		return "", false
	}

	address := v.convUnaryExpr(&ast.UnaryExpr{OpPos: array.Pos(), Op: token.AND, X: array}, DefaultUnaryExprContext())

	// P3's review, ruling 4 (COORD 2026-11-15): a slice cut from golib's one-argument Ꮡ(value) would alias a COPY of the
	// operand, so every write through it would be lost with nothing said. That spelling means an address walk missed
	// the variable that holds the array (escape analysis, capture mode, ref lowering or the package-variable scan), which
	// is a converter defect, so the file is refused here, naming the site: the run fails (GO2CS2008) instead of the
	// program printing a wrong answer.
	if addressIsCopyBox(address) {
		position := v.fset.Position(array.Pos())
		panic(fmt.Sprintf("a slice of the inline array %s at %s:%d would be cut from a copy (%s), so its writes would be lost: no address walk boxed the variable that holds it",
			types.ExprString(array), filepath.Base(position.Filename), position.Line, address))
	}

	if strings.HasPrefix(address, "(") && !isFullyParenthesized(address) || strings.HasPrefix(address, "~") {
		address = "(" + address + ")"
	}

	return fmt.Sprintf("%s.ΔInlineSlice<%s>()", address, convertToCSTypeName(v.getAliasQualifiedTypeName(arrayType.Elem(), false))), true
}

// inlineSliceElementIsPlain reports whether an inline array of elem can be sliced as a native window: a numeric or
// boolean element, whose C# form holds no managed reference.
func inlineSliceElementIsPlain(elem types.Type) bool {
	basic, ok := elem.Underlying().(*types.Basic)
	return ok && basic.Info()&(types.IsNumeric|types.IsBoolean) != 0
}

// addressIsCopyBox reports whether address, convUnaryExpr's spelling of &x, boxes a COPY of x: golib's one-argument
// Ꮡ(value) (builtin.cs), alone or followed by a field or element step. Every other Ꮡ overload takes a container and an
// index and aliases the element. The test counts brackets over the spelled text, so three spellings inside the one
// argument can read as a second argument and make a copy read as an alias: a comma in a generic argument list at the
// call's own depth, and a comma or a bracket inside a string or a rune literal. Each misses toward "not refused"; the
// test never refuses a spelling that aliases (P3's re-read, note 3, 2026-11-15).
func addressIsCopyBox(address string) bool {
	if !strings.HasPrefix(address, AddressPrefix+"(") {
		return false
	}

	depth := 0

	for _, r := range address[len(AddressPrefix):] {
		switch r {
		case '(', '[', '{':
			depth++
		case ')', ']', '}':
			depth--

			if depth == 0 {
				return true
			}
		case ',':
			if depth == 1 {
				return false
			}
		}
	}

	return false
}

// inlineSliceStorageRoot walks the operand of a slice of an inline array (x.f, x.a.f, x.arr[i].f, x.m[i], p.f, (*p).f)
// down its storage chain to the variable whose storage holds it, and reports whether the chain reaches that variable
// through a pointer (the variable is a pointer and its pointee holds the array). It is THE predicate for the four address
// walks (escape analysis, objectAddressTaken, capture mode and the package-variable scan), so each boxes exactly the
// roots inlineArraySliceBase spells (P3's review, ruling 2, COORD 2026-11-15).
//
// Only hops that stay inside one variable's storage are walked: a field of a struct value and an element of an array
// value. A pointer is admitted only at the root (p.f with p a pointer, (*p).f); a slice element, a map element, a call,
// or a pointer reached deeper in names storage that already has a box of its own, and answers false. A package-qualified
// variable (pkg.V.f) roots at V.
//
// A bare variable (x[:], pkg.V[:]: no hop) answers false, and no caller reaches it: inlineArraySliceSpelled admits only a
// field selection or an element of one, since an array is held inline only as a field of an admitted struct. A field
// promoted through an EMBEDDED POINTER of a pointer root (p.f, with f in *E embedded in *p) answers (p, indirect) though
// its storage is E's; the only effect is one receiver box capture mode did not need (P3's re-read, notes 1 and 2).
func inlineSliceStorageRoot(expr ast.Expr, info *types.Info) (root *ast.Ident, indirect bool, ok bool) {
	for hops := 0; ; hops++ {
		switch e := ast.Unparen(expr).(type) {
		case *ast.SelectorExpr:
			selection, found := info.Selections[e]

			if !found {
				// A package-qualified variable: pkg.V.
				if pkgIdent, isIdent := ast.Unparen(e.X).(*ast.Ident); isIdent && hops > 0 {
					if _, isPkg := info.Uses[pkgIdent].(*types.PkgName); isPkg {
						return e.Sel, false, true
					}
				}

				return nil, false, false
			}

			if selection.Kind() != types.FieldVal {
				return nil, false, false
			}

			if selection.Indirect() {
				ident, isIdent := ast.Unparen(e.X).(*ast.Ident)

				if !isIdent {
					return nil, false, false
				}

				if _, isPointer := types.Unalias(info.TypeOf(ident)).Underlying().(*types.Pointer); !isPointer {
					return nil, false, false // a promoted field through an embedded pointer: the pointee's storage
				}

				return ident, true, true
			}

			expr = e.X
		case *ast.IndexExpr:
			containerType := info.TypeOf(e.X)

			if containerType == nil {
				return nil, false, false
			}

			if _, isArray := containerType.Underlying().(*types.Array); !isArray {
				return nil, false, false
			}

			expr = e.X
		case *ast.StarExpr:
			ident, isIdent := ast.Unparen(e.X).(*ast.Ident)

			if !isIdent || hops == 0 {
				return nil, false, false
			}

			return ident, true, true
		case *ast.Ident:
			if hops == 0 {
				return nil, false, false
			}

			return e, false, true
		default:
			return nil, false, false
		}
	}
}

// inlineArraySliceRootsAt reports whether slice cuts an inline array that inlineArraySliceBase spells and that lies in
// the storage of identObj itself (no pointer on the way), so escape analysis boxes identObj.
func (v *Visitor) inlineArraySliceRootsAt(slice *ast.SliceExpr, identObj types.Object) bool {
	root, indirect, ok := inlineSliceStorageRoot(slice.X, v.info)
	return ok && !indirect && v.info.ObjectOf(root) == identObj && inlineArraySliceSpelled(slice, v.info)
}

// inlineArraySliceSpelled reports whether slice cuts an inline array that inlineArraySliceBase spells, over the
// array's address.
func inlineArraySliceSpelled(slice *ast.SliceExpr, info *types.Info) bool {
	if !isInlineArrayExprIn(slice.X, info) {
		return false
	}

	arrayType, ok := types.Unalias(info.TypeOf(slice.X)).Underlying().(*types.Array)

	return ok && arrayType.Len() > 0 && inlineSliceElementIsPlain(arrayType.Elem())
}

// inlineArrayCopyOperand reports whether call is Go's copy with exactly one operand a two-index slice of an inline array
// that inlineArraySliceBase spells, and the other operand a slice (COORD 2026-11-15, Part 2 ruling (b)): that slice and
// its position (0, the destination; 1, the source). Such a copy is spelled over a reference to the inline storage for
// the copy's duration (inlineArrayCopyEmission), so the slice takes no pin and no box: Go's own copy does not let it
// escape. A string operand, a three-index slice, and two inline operands keep the slice's own spelling.
//
// The test is structural (the call, its operands and their types), so every pass that asks it, in the declaring
// package or in a consumer reading an imported method's body, gets the same answer.
func inlineArrayCopyOperand(call *ast.CallExpr, info *types.Info) (*ast.SliceExpr, int, bool) {
	if len(call.Args) != 2 || call.Ellipsis.IsValid() {
		return nil, -1, false
	}

	if builtin := calledBuiltin(call.Fun, info); builtin == nil || builtin.Name() != "copy" {
		return nil, -1, false
	}

	var found *ast.SliceExpr
	position := -1

	for i, arg := range call.Args {
		slice, ok := ast.Unparen(arg).(*ast.SliceExpr)

		if !ok || slice.Slice3 || !inlineArraySliceSpelled(slice, info) {
			continue
		}

		if found != nil {
			return nil, -1, false // both operands inline
		}

		found, position = slice, i
	}

	if found == nil {
		return nil, -1, false
	}

	if _, isSlice := types.Unalias(info.TypeOf(call.Args[1-position])).Underlying().(*types.Slice); !isSlice {
		return nil, -1, false
	}

	// Go checks a slice's bounds when the operand is evaluated, so a DESTINATION's bad bound panics before the source
	// runs (`copy(x.f[lo:hi], next())` never calls next); the reference spelling checks them inside copy, after it. A
	// constant bound cannot fail (Go rejects one outside the array at compile time), so the destination is spelled over
	// a reference only when its bounds are constant or absent; any other keeps the slice's own spelling, which is
	// evaluated first (P3's review, NIT 6, COORD 2026-11-15). A source is evaluated last either way.
	if position == 0 {
		for _, bound := range []ast.Expr{found.Low, found.High} {
			if bound == nil {
				continue
			}

			if typeAndValue, ok := info.Types[bound]; !ok || typeAndValue.Value == nil {
				return nil, -1, false
			}
		}
	}

	return found, position, true
}

// pinFreeInlineCopySlices collects, in body, every slice inlineArrayCopyOperand spells without a pin, for the address
// walks to pass over.
func pinFreeInlineCopySlices(body ast.Node, info *types.Info) map[*ast.SliceExpr]bool {
	if body == nil || packageInlineAdmission == nil {
		return nil
	}

	var slices map[*ast.SliceExpr]bool

	ast.Inspect(body, func(n ast.Node) bool {
		if call, ok := n.(*ast.CallExpr); ok {
			if slice, _, ok := inlineArrayCopyOperand(call, info); ok {
				if slices == nil {
					slices = map[*ast.SliceExpr]bool{}
				}

				slices[slice] = true
			}
		}

		return true
	})

	return slices
}

// inlineArrayCopyEmission renders a copy inlineArrayCopyOperand spells: copy(ref x.f, lo, hi, src) for a destination
// slice and copy(dst, ref x.f, lo, hi) for a source slice, over golib's two overloads (builtin.InlineArrayCopy.cs).
func (v *Visitor) inlineArrayCopyEmission(call *ast.CallExpr) (string, bool) {
	slice, position, ok := inlineArrayCopyOperand(call, v.info)

	if !ok {
		return "", false
	}

	arrayType := types.Unalias(v.info.TypeOf(slice.X)).Underlying().(*types.Array)
	low, high := "0", fmt.Sprintf("%d", arrayType.Len())

	if slice.Low != nil {
		low = v.castWideIntegerToNint(slice.Low)
	}

	if slice.High != nil {
		high = v.castWideIntegerToNint(slice.High)
	}

	window := fmt.Sprintf("ref %s, %s, %s", v.convExpr(slice.X, nil), low, high)
	other := v.convExpr(call.Args[1-position], nil)

	if position == 0 {
		return fmt.Sprintf("%s(%s, %s)", v.golibCall("copy"), window, other), true
	}

	return fmt.Sprintf("%s(%s, %s)", v.golibCall("copy"), other, window), true
}
