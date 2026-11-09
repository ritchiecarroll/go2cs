// coreTypeParamAssign.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"fmt"
	"go/types"
)

// A TYPE PARAMETER ASSIGNED TO ITS CORE TYPE (TS readiness C24)
//
// Go lets a value whose type is a type parameter with a slice or map core type be assigned to that core type
// (Assignability: "V is a type parameter and T is not a named type"):
//
//	func CopyMapInto[M1 ~map[K]V, M2 ~map[K]V, K comparable, V any](dst M1, src M2) map[K]V { ...; return dst }
//
// tsc's internal/core/core.go:805. The emitted parameter is interface-constrained (`where M1 : IMap<K, V>`), and C#
// has no conversion from it to golib's concrete struct (CS0029, CS1503 at an argument). Go's two values SHARE the
// backing, so the materialization shares too:
//   - a slice core goes through golib's sharing slice<T>(ISlice<T>) constructor: `new slice<E>(s)`;
//   - a map core reads the shared map through IMap's AsMap(): `dst.AsMap()`.
// The positions are the ones chanDirNarrowedValue hooks: return, typed var, assignment and argument. The slice
// ARGUMENT keeps its older route, convCallExpr's wrapArgWithNew, which already emits the same constructor; this
// helper's argument arm takes only the map suffix.
//
// A chan-cored type parameter is not here: its constraint emits no IChannel<E> at all, a wider gap filed on its own.

// coreTypeParamValue returns rendered materialized as dst, or "" when the value is not a slice- or map-cored type
// parameter or dst is not its core type. dst is the static target type, src the value's own type.
func (v *Visitor) coreTypeParamValue(dst, src types.Type, rendered string) string {
	if dst == nil || src == nil || rendered == "" {
		return ""
	}

	tp, isTypeParam := types.Unalias(src).(*types.TypeParam)

	if !isTypeParam {
		return ""
	}

	// Go admits only an UNNAMED target here; a named slice or map, an interface or another type parameter is a
	// different conversion (or none).
	switch target := types.Unalias(dst).(type) {
	case *types.Slice:
		if typeParamSliceCore(tp) == nil {
			return ""
		}

		return fmt.Sprintf("new slice<%s>(%s)", convertToCSTypeName(v.getAliasQualifiedTypeName(target.Elem(), false)), rendered)
	case *types.Map:
		if typeParamMapCore(tp) == nil {
			return ""
		}

		return memberAccessOperand(rendered) + ".AsMap()"
	}

	return ""
}

// coreTypeParamArgSuffix returns the member call that materializes a MAP-cored type parameter argument passed where
// its core map type is expected, or "". The argument channel appends suffixes to the rendered argument (convExprList).
func (v *Visitor) coreTypeParamArgSuffix(dst, src types.Type) string {
	if _, isMap := types.Unalias(dst).(*types.Map); isMap && v.coreTypeParamValue(dst, src, "x") != "" {
		return ".AsMap()"
	}

	return ""
}

// memberAccessOperand parenthesizes a rendered expression unless a member access already binds to all of it: an
// identifier chain, or a primary expression ending in a call or index (`maps.Clone<M2, K, V>(src)`, `ms[i]`) that does
// not open with a cast or a parenthesis and holds no space outside brackets (`new T(x)` is the one spaced primary).
func memberAccessOperand(rendered string) string {
	if isIdentifierChain(rendered) {
		return rendered
	}

	last := rendered[len(rendered)-1]

	if rendered[0] != '(' && (last == ')' || last == ']') {
		depth := 0
		spaced := false

		for i, r := range rendered {
			switch r {
			case '(', '[', '<':
				depth++
			case ')', ']', '>':
				depth--
			case ' ':
				if depth == 0 && !(i == 3 && rendered[:3] == "new") {
					spaced = true
				}
			}
		}

		if !spaced {
			return rendered
		}
	}

	return "(" + rendered + ")"
}
