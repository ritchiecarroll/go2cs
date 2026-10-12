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
	"go/token"
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
		gap := -1

		if lastStmt != nil {
			gap = v.writeStatementGap(lastStmt, stmt)
		}

		// Inject any hoisted stack-string temp declarations whose anchor is this statement, so a
		// repeated `string(x)` conversion is materialized once here and each use references the temp
		// (see planSStringHoists). The anchor is always a top-level function-body statement, so this
		// fires exactly once, at body scope, before the first use.
		for _, hoist := range v.sstringHoistsByStmt[stmt] {
			v.emitSStringHoist(hoist)
		}

		v.visitListStmt(stmt)
		v.dropEmptyGap(gap)

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

// visitStatementList converts the statements of a case, type-switch case or select case body, keeping
// Go's blank lines between them as a block body does (writeStatementGap).
func (v *Visitor) visitStatementList(stmts []ast.Stmt) {
	for i, stmt := range stmts {
		gap := -1

		if i > 0 {
			gap = v.writeStatementGap(stmts[i-1], stmt)
		}

		v.visitListStmt(stmt)
		v.dropEmptyGap(gap)
	}
}

// writeStatementGap writes what lies between two statements of one list in the Go source: the
// standalone comments still pending before next, and a blank line wherever Go has one, before a
// comment group, between two groups, and before next itself. gofmt keeps at most one blank line in a
// row, so a gap of any size writes one. Comments before prev's last line (inside prev, or trailing it
// and not yet claimed) are flushed in order with no blank line, as before.
//
// Under `-comments` only (docs/PLAN-comment-and-spacing-parity.md, class A and its decision 1): without
// it nothing is written, so the emission, and every behavioral golden, is unchanged.
//
// It returns the output length before the blank line it wrote ahead of next, or -1 when it wrote none,
// for dropEmptyGap.
func (v *Visitor) writeStatementGap(prev, next ast.Stmt) int {
	if !v.options.includeComments {
		return -1
	}

	if v.file == nil {
		v.file = v.fset.File(next.Pos())
	}

	prefix := v.newline + v.indent(v.indentLevel)
	lastLine := v.file.Line(prev.End())
	nextPos := next.Pos()

	// Take the pending positions first: writing a comment removes it from sortedCommentPos.
	var pending []token.Pos

	for _, pos := range v.sortedCommentPos {
		if pos >= nextPos {
			break
		}

		if _, found := v.standAloneComments[pos]; found {
			pending = append(pending, pos)
		}
	}

	for _, pos := range pending {
		line := v.file.Line(pos)
		endLine := line + strings.Count(v.standAloneComments[pos], "\n")

		if line-lastLine > 1 {
			v.writeGapLine()
		}

		v.writeStandAloneCommentString(v.outputBuilder, pos, nil, prefix)

		lastLine = max(lastLine, endLine)
	}

	if v.file.Line(nextPos)-lastLine > 1 {
		mark := v.outputBuilder.Len()

		if v.writeGapLine() {
			return mark
		}
	}

	return -1
}

// writeGapLine writes the line break that makes Go's blank line, and reports whether it did. It writes
// none when the output already ends with a line break (a lowered switch ends with its own, and the next
// line's leading break then makes the blank line by itself; a second would leave two blank lines in a
// row, which gofmt never does), or directly after an opening brace (the statements before the gap
// emitted nothing here, such as a local type declaration lifted out of the body).
//
// Both tests read the current line as it will be emitted: without position sentinels (every statement
// writes one, so a statement that emits no C# still leaves one behind; positionMapOperations.go) and
// without `>>MARKER:…<<` placeholders (a function body opens with its block-prefix marker).
func (v *Visitor) writeGapLine() bool {
	output := v.outputBuilder.String()
	lineStart := strings.LastIndex(output, "\n") + 1
	line := strings.TrimRight(stripEmissionMarkers(stripPositionSentinels(output[lineStart:])), " \t\r")

	// An empty current line is a break already written, or nothing emitted yet in this block's own
	// builder (a function body's brace can sit in the enclosing builder).
	if line == "" || strings.HasSuffix(line, "{") {
		return false
	}

	v.outputBuilder.WriteString(v.newline)

	return true
}

// stripEmissionMarkers removes the `>>MARKER:…<<` placeholders the converter resolves after emission.
func stripEmissionMarkers(text string) string {
	const prefix, suffix = ">>MARKER:", "<<"

	for {
		start := strings.Index(text, prefix)

		if start < 0 {
			return text
		}

		end := strings.Index(text[start+len(prefix):], suffix)

		if end < 0 {
			return text
		}

		text = text[:start] + text[start+len(prefix)+end+len(suffix):]
	}
}

// dropEmptyGap removes the blank line writeStatementGap wrote ahead of a statement that then emitted
// nothing but whitespace and its position sentinel (a bare `return` the deferred-call lowering carries
// past its `finally`, for one), so the blank line does not end up alone before a closing brace. Only the
// line break at mark is removed; the statement's sentinel stays, so its position-map entry does too.
// mark is writeStatementGap's result.
func (v *Visitor) dropEmptyGap(mark int) {
	if mark < 0 {
		return
	}

	output := v.outputBuilder.String()
	end := mark + len(v.newline)

	if end > len(output) || output[mark:end] != v.newline || strings.TrimSpace(stripPositionSentinels(output[end:])) != "" {
		return
	}

	v.outputBuilder.Reset()
	v.outputBuilder.WriteString(output[:mark])
	v.outputBuilder.WriteString(output[end:])
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
