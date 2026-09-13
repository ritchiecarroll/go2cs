// visitIfStmt.go - Gbtc
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

func (v *Visitor) visitIfStmt(ifStmt *ast.IfStmt) {
	// A func literal passed as a call argument in the condition (`if (underIs(t, func(u){…}))`,
	// pervasive in go/types) emits its captured-variable snapshot declarations (`var tʗ1 = t;`) —
	// statements, invalid inside the condition expression. Convert the condition into a hoist buffer
	// so those decls can be written on their own lines before the `if` (mirrors visitExprStmt).
	// Save/restore guards nesting.
	savedHoist := v.hoistedDecls
	hoistBuf := &strings.Builder{}
	var cond string

	if ifStmt.Init == nil {
		// Convert the condition first: on a hoist the buffer supplies the leading newline + the decls.
		v.hoistedDecls = hoistBuf
		cond = v.convExpr(ifStmt.Cond, nil)
		v.hoistedDecls = savedHoist

		if hoistBuf.Len() > 0 {
			// The buffer carries its own leading newline+indent per decl and a trailing newline (the
			// per-decl trailing indent is trimmed by convFuncLit); the `if`'s own indent follows.
			v.outputBuilder.WriteString(hoistBuf.String())
		} else {
			v.outputBuilder.WriteString(v.newline)
		}

		v.writeOutput("")
	} else {
		// Any declared variable will be scoped to if statement, so create a sub-block for it
		v.outputBuilder.WriteString(v.newline)
		v.writeOutput("{")
		v.indentLevel++

		v.visitStmt(ifStmt.Init, []StmtContext{})

		// Convert the condition AFTER the init (preserving capture-counter ordering); hoist any
		// snapshot decls onto their own lines between the init and the `if`.
		v.hoistedDecls = hoistBuf
		cond = v.convExpr(ifStmt.Cond, nil)
		v.hoistedDecls = savedHoist

		if hoistBuf.Len() > 0 {
			v.outputBuilder.WriteString(hoistBuf.String())
			v.writeOutput("")
		} else {
			v.outputBuilder.WriteRune(' ')
		}
	}

	context := DefaultBlockStmtContext()
	context.format.useNewLine = false

	v.outputBuilder.WriteString("if (")
	v.outputBuilder.WriteString(cond)
	v.outputBuilder.WriteRune(')')

	v.pushBlock()
	v.visitBlockStmt(ifStmt.Body, context)
	body := v.popBlockAppend(false)

	if ifStmt.Else == nil {
		v.outputBuilder.WriteString(body)
	} else {
		v.outputBuilder.WriteString(strings.TrimSpace(body))
		v.outputBuilder.WriteString(" else")

		switch elseStmt := ifStmt.Else.(type) {
		case *ast.IfStmt:
			v.outputBuilder.WriteRune(' ')
			v.visitIfStmt(elseStmt)
		case *ast.BlockStmt:
			v.visitBlockStmt(elseStmt, context)
		default:
			panic("@visitIfStmt - Unexpected `ast.IfStmt` else type: " + v.getPrintedNode(elseStmt))
		}
	}

	if ifStmt.Init != nil {
		v.indentLevel--
		v.outputBuilder.WriteString(v.newline)
		v.writeOutput("}")
	}
}
