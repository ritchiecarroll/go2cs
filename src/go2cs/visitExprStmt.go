// visitExprStmt.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/ast"
	"strings"
)

func (v *Visitor) visitExprStmt(exprStmt *ast.ExprStmt, format FormattingContext) {
	if exprStmt.X == nil {
		return
	}

	// A func literal passed as a call argument (`systemstack(func(){ …&m… })`, pervasive in
	// runtime) captures variables whose snapshot declarations (`var mʗ1 = m;`) are statements —
	// invalid inside an argument list. For a standalone statement, collect them in a buffer and
	// write them before the statement. A for-loop init/post clause (useNewLine == false) is not a
	// standalone statement slot, so it does not hoist. Save/restore guards nesting.
	savedHoist := v.hoistedDecls
	var hoistBuf *strings.Builder

	if format.useNewLine {
		hoistBuf = &strings.Builder{}
		v.hoistedDecls = hoistBuf
	}

	defer func() { v.hoistedDecls = savedHoist }()

	// This statement's own expression is emitted with its result DISCARDED. C# admits a call in a
	// statement slot but not a cast, so a conversion applied purely to type the result has nothing
	// left to serve here and is a syntax error (CS0201). Publish the node so convCallExpr can tell
	// THIS call from a nested one whose value is still consumed. Save/restore guards nesting.
	savedDiscard := v.resultDiscardedExpr
	v.resultDiscardedExpr = exprStmt.X

	defer func() { v.resultDiscardedExpr = savedDiscard }()

	expr := v.convExpr(exprStmt.X, nil)

	if hoistBuf != nil && hoistBuf.Len() > 0 {
		// The hoisted decls carry their own leading newline + per-line indentation.
		v.outputBuilder.WriteString(hoistBuf.String())
	} else if format.useNewLine {
		v.outputBuilder.WriteString(v.newline)
	}

	if format.useIndent {
		v.outputBuilder.WriteString(v.indent(v.indentLevel))
	}

	v.outputBuilder.WriteString(expr)

	// A for-loop init/post clause is `;`-free (the for-syntax supplies the separators); a standalone
	// expression statement is terminated with a semicolon.
	if format.includeSemiColon {
		v.outputBuilder.WriteString(";")
	}
}
