// visitBlockStmt.go - Gbtc
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

func (v *Visitor) visitBlockStmt(blockStmt *ast.BlockStmt, context BlockStmtContext) {
	v.pushBlock()

	if len(context.outerPrefix) > 0 {
		v.outputBuilder.WriteString(context.outerPrefix)
	}

	if context.format.useNewLine {
		v.outputBuilder.WriteString(v.newline)
		v.outputBuilder.WriteString(v.indent(v.indentLevel))
		v.outputBuilder.WriteRune('{')
	} else {
		v.outputBuilder.WriteString(" {")
	}

	if len(context.innerPrefix) > 0 {
		v.outputBuilder.WriteString(context.innerPrefix)
	}

	v.firstStatementIsReturn = false

	if context.format.useIndent {
		v.indentLevel++
	}

	var lastStmt ast.Stmt
	prefix := v.newline + v.indent(v.indentLevel)

	if len(blockStmt.List) > 0 && v.options.includeComments {
		v.writeStandAloneCommentString(v.outputBuilder, blockStmt.List[0].Pos(), nil, prefix)
	}

	for _, stmt := range blockStmt.List {
		if v.options.includeComments {
			if lastStmt != nil {
				if v.file == nil {
					v.file = v.fset.File(stmt.Pos())
				}

				currentLine := v.file.Line(stmt.Pos())
				lastLine := v.file.Line(lastStmt.End())

				comments := &strings.Builder{}
				var wrote bool

				if wrote, lines := v.writeStandAloneCommentString(comments, stmt.Pos(), nil, prefix); wrote {
					lastLine -= lines - 1
				}

				if wrote && currentLine-lastLine > 1 {
					v.outputBuilder.WriteString(strings.Repeat(v.newline, currentLine-lastLine-1))
				}

				if comments.Len() > 0 {
					v.outputBuilder.WriteString(comments.String())
				}
			}
		}

		// Inject any hoisted stack-string temp declarations whose anchor is this statement, so a
		// repeated `string(x)` conversion is materialized once here and each use references the temp
		// (see planSStringHoists). The anchor is always a top-level function-body statement, so this
		// fires exactly once, at body scope, before the first use.
		for _, hoist := range v.sstringHoistsByStmt[stmt] {
			v.emitSStringHoist(hoist)
		}

		v.visitListStmt(stmt)

		lastStmt = stmt
	}

	if context.format.useIndent {
		v.indentLevel--
	}

	statementList := blockStmt.List

	// Check if the first statement is a return statement
	if len(statementList) > 0 {
		_, ok := statementList[0].(*ast.ReturnStmt)
		v.firstStatementIsReturn = ok
	}

	if len(context.innerSuffix) > 0 {
		v.outputBuilder.WriteString(context.innerSuffix)
	}

	v.outputBuilder.WriteString(v.newline)
	v.writeOutput("}")

	if len(context.outerSuffix) > 0 {
		v.outputBuilder.WriteString(context.outerSuffix)
	}

	v.popBlock()
}

// pushBlock redirects emission into a FRESH output builder, so whatever is written next accumulates
// on its own instead of landing in the enclosing block's text. The displaced builder is remembered
// on the block stack and restored by popBlock.
//
// This is what lets the converter emit a nested block, then decide what to do with it — keep it,
// indent it, wrap it, or discard it — after seeing all of it.
func (v *Visitor) pushBlock() {
	v.blocks.Push(v.outputBuilder)
	v.outputBuilder = &strings.Builder{}
}

// popBlock restores the enclosing output builder and APPENDS the block just emitted to it — the
// normal case, where a nested block simply becomes part of its parent. It returns the block's text
// as well, for a caller that also wants to inspect it.
func (v *Visitor) popBlock() string {
	return v.popBlockAppend(true)
}

// popBlockAppend restores the enclosing output builder and returns the text emitted since the
// matching pushBlock.
//
// Pass appendToPrevious=false to CAPTURE the block instead of emitting it: the enclosing builder is
// left untouched and the caller receives the text to place itself. That is how a block gets
// rewritten before it lands — hoisted into a lambda, wrapped in `do { … } while (false)`, or
// reordered — rather than having to be un-emitted afterwards.
func (v *Visitor) popBlockAppend(appendToPrevious bool) string {
	enclosing := v.blocks.Pop()
	block := v.outputBuilder.String()

	if appendToPrevious {
		enclosing.WriteString(block)
	}

	v.outputBuilder = enclosing

	return block
}
