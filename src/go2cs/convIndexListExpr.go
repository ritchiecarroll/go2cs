// convIndexListExpr.go - Gbtc
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
	"strings"
)

func (v *Visitor) convIndexListExpr(indexListExpr *ast.IndexListExpr) string {
	callExprContext := DefaultCallExprContext()
	callExprContext.sourceIsTypeParams = true

	// The base (X) renders WITHOUT its own generic type arguments — this appends the explicit
	// `<Indices>` here, so convSelectorExpr must not also append the inferred instance args
	// (`concurrent.NewHashTrieMap<K, V><K, V>` — unique, CS1525 cascade). See convIndexExpr.
	xContext := DefaultLambdaContext()
	xContext.suppressGenericTypeArgs = true

	// The BARE-IDENT form of the same suppression — see convIndexExpr.
	xIdentContext := DefaultIdentContext()
	xIdentContext.suppressGenericTypeArgs = true
	xContexts := []ExprContext{xContext, xIdentContext}

	// An explicitly written instantiation of a pointer-core generic drops its ERASED positions
	// (`clone[*thing, thing]` → `clone<thing>`, see explicitTypeArgsAfterErasure); a list that
	// erases to empty leaves the base bare (C# infers from the remaining value arguments).
	indices := indexListExpr.Indices

	if kept, erased := v.explicitTypeArgsAfterErasure(indexListExpr.X, indices); erased {
		if len(kept) == 0 {
			return v.convExpr(indexListExpr.X, xContexts)
		}

		indices = kept
	}

	// A PARTIAL Go instantiation of a generic FUNCTION completes from the resolved instance —
	// C# has no partial instantiation (CS0305). See completedInstantiationTypeArgs.
	if completed := v.completedInstantiationTypeArgs(indexListExpr.X, len(indices)); completed != nil {
		return fmt.Sprintf("%s<%s>", v.convExpr(indexListExpr.X, xContexts), strings.Join(completed, ", "))
	}

	return fmt.Sprintf("%s<%s>", v.convExpr(indexListExpr.X, xContexts), v.convExprList(indices, indexListExpr.Lbrack, callExprContext))
}
