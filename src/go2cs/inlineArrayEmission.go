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
	"go/types"
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
//   - an array literal that initializes the field: new array<N><T>(e0, e1, ...) (convCompositeLit.go).

// isInlineArrayExpr reports whether expr denotes an array held inline: an array field of an admitted struct, or an array
// element of one (an admitted closure holds every array inline, nested ones included).
func (v *Visitor) isInlineArrayExpr(expr ast.Expr) bool {
	if packageInlineAdmission == nil {
		return false
	}

	switch e := ast.Unparen(expr).(type) {
	case *ast.SelectorExpr:
		owner, _, ok := arrayFieldOwner(e, v.info)
		return ok && packageInlineAdmission.admits(owner)
	case *ast.IndexExpr:
		if _, isArray := types.Unalias(v.info.TypeOf(e)).Underlying().(*types.Array); isArray {
			return v.isInlineArrayExpr(e.X)
		}
	}

	return false
}

// markInlineArrayLiterals marks, in a struct literal of an admitted type, each array literal that initializes one of its
// nonzero array fields (keyed by field name or by position).
func (v *Visitor) markInlineArrayLiterals(lit *ast.CompositeLit) {
	if packageInlineAdmission == nil || !packageInlineAdmission.admits(v.info.TypeOf(lit)) {
		return
	}

	st, ok := types.Unalias(v.info.TypeOf(lit)).Underlying().(*types.Struct)

	if !ok {
		return
	}

	for i, elt := range lit.Elts {
		field, value := -1, elt

		if kv, ok := elt.(*ast.KeyValueExpr); ok {
			value = kv.Value

			if id, ok := kv.Key.(*ast.Ident); ok {
				for j := range st.NumFields() {
					if st.Field(j).Name() == id.Name {
						field = j
					}
				}
			}
		} else {
			field = i
		}

		if field < 0 || field >= st.NumFields() {
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

// inlineArrayLiteral renders a marked array literal as the go.arrays member's constructor over its elements, Go's zero
// filling the rest. Only a positional literal is marked into this form; a keyed one keeps the array<T> rendering, which
// does not convert to the member, so it is a C# compile error rather than a silent copy (S4).
func (v *Visitor) inlineArrayLiteral(lit *ast.CompositeLit) (string, bool) {
	array, ok := v.inlineArrayLiterals[lit]

	if !ok || compositeLitIsKeyed(lit.Elts) {
		return "", false
	}

	v.addRequiredUsing("go.arrays")

	// A nested array literal is itself an inline array (every array in an admitted closure is).
	if inner, isArray := array.Elem().Underlying().(*types.Array); isArray && inner.Len() > 0 {
		for _, elt := range lit.Elts {
			if elemLit, isLit := ast.Unparen(elt).(*ast.CompositeLit); isLit {
				v.inlineArrayLiterals[elemLit] = inner
			}
		}
	}

	if len(lit.Elts) == 0 {
		return fmt.Sprintf("new %s()", v.inlineArrayTypeName(array)), true
	}

	return fmt.Sprintf("new %s(%s)", v.inlineArrayTypeName(array), v.convExprList(lit.Elts, lit.Lbrace, v.withValueCloneArgs(lit.Elts, v.elidedElemContext(array.Elem(), lit.Elts)))), true
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
