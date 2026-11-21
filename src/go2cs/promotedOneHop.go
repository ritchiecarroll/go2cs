// promotedOneHop.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/ast"
	"go/types"
	"slices"
	"strings"
)

// promotedOneHopView renders the receiver view of a pointer-receiver method promoted through two or more VALUE embeds in
// ONE hop (TS perf item 1b, COORD 2026-11-21 06:58Z). The chain form takes a view per level, `x.of(A.ᏑB).of(B.ᏑC).of(C.ᏑD)`,
// and golib mints and keeps each one per box: TypeScript's checkdemo kept six per AST node in
// LeftHandSideExpressionBase.AsNode. Go reaches the method's receiver in one step, and so can C#: go2cs-gen declares the
// OUTER type's accessor for every promoted embed it can name (`A.ᏑD`, a ref to `x.B.C.D`), and golib resolves that
// accessor's Go offset by name through the embeds, so the one view is managed-exact and is the same Go pointer as the
// chain's last view and as `&x.D`.
//
// fieldAddr is the first hop as the &-machinery rendered it. The one hop is taken only when that hop is a real view
// over a box (`<box>.of(<Outer>.Ꮡ<first embed>)`; a copy box or a ref receiver keeps its own arm), when the Go selector
// x.<last embed> names exactly the embed on the method's path (so the accessor exists: no shallower field shadows it and
// no other embed at its depth makes it ambiguous), and when the outer type is declared in this package (its accessor may
// be internal). Otherwise the chain is kept.
func (v *Visitor) promotedOneHopView(selectorExpr *ast.SelectorExpr, fieldAddr string, hopFields []*types.Var) (string, bool) {
	if len(hopFields) < 2 {
		return "", false
	}

	sel := v.info.Selections[selectorExpr]

	if sel == nil {
		return "", false
	}

	outer := promotionNamed(sel.Recv())

	if outer == nil || outer.Obj().Pkg() != v.pkg {
		return "", false
	}

	first := v.structFieldBoxName(&ast.Ident{Name: hopFields[0].Name()}, selectorExpr.X)
	ofIndex := strings.LastIndex(fieldAddr, ".of(")

	if ofIndex < 0 || !strings.HasSuffix(fieldAddr, ")") {
		return "", false
	}

	accessor := fieldAddr[ofIndex+len(".of(") : len(fieldAddr)-1]
	firstSuffix := "." + AddressPrefix + first

	if !strings.HasSuffix(accessor, firstSuffix) {
		return "", false
	}

	last := hopFields[len(hopFields)-1]
	obj, index, _ := types.LookupFieldOrMethod(outer, false, v.pkg, last.Name())

	if obj != last || !slices.Equal(index, sel.Index()[:len(sel.Index())-1]) {
		return "", false
	}

	owner := accessor[:len(accessor)-len(firstSuffix)]
	lastName := v.structFieldBoxName(&ast.Ident{Name: last.Name()}, selectorExpr.X)

	return fieldAddr[:ofIndex] + ".of(" + owner + "." + AddressPrefix + lastName + ")", true
}
