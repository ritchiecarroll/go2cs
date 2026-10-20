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
	"slices"
	"sort"
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

	if v.options.includeComments {
		limit := blockStmt.Rbrace

		if len(blockStmt.List) > 0 {
			limit = blockStmt.List[0].Pos()
		}

		v.writeLineTrailingComments(blockStmt.Lbrace, limit)
	}

	for _, stmt := range blockStmt.List {
		gap := -1

		if lastStmt != nil {
			gap = v.writeStatementGap(lastStmt, stmt)
		} else if v.options.includeComments {
			gap = v.writeLeadingComments(blockStmt.Lbrace, stmt, prefix)
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

	if lastStmt != nil {
		v.writeClosingComments(lastStmt.End(), blockStmt.Rbrace, 0)
	} else {
		v.writeClosingComments(blockStmt.Lbrace, blockStmt.Rbrace, 0)
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
// Go's blank lines between them as a block body does (writeStatementGap). colon is the clause's `:`.
func (v *Visitor) visitStatementList(stmts []ast.Stmt, colon token.Pos) {
	if len(stmts) == 0 {
		v.writeLeadingBodyComments(colon, nil)
		return
	}

	for i, stmt := range stmts {
		gap := -1

		if i == 0 {
			gap = v.writeLeadingBodyComments(colon, stmt)
		} else {
			gap = v.writeStatementGap(stmts[i-1], stmt)
		}

		v.visitListStmt(stmt)
		v.dropEmptyGap(gap)
	}

	if span, spanned := v.clauseSpans[colon]; spanned {
		v.writeClosingComments(stmts[len(stmts)-1].End(), span.before, span.caseColumn)
	}
}

// writeAboveLabelComments writes the comments Go has on their own lines above a clause's `case` or
// `default` label, above the clause's header in C# (class H), at the header's indentation. A clause's
// held comments (holdClauseComments) are released first. Left to writeLeadingBodyComments, they were
// written below the header, at the top of the body.
func (v *Visitor) writeAboveLabelComments(colon token.Pos) {
	if !v.options.includeComments {
		return
	}

	v.releaseClauseComments(colon)

	span, spanned := v.clauseSpans[colon]

	if !spanned {
		return
	}

	if v.file == nil {
		v.file = v.fset.File(colon)
	}

	afterLine := v.file.Line(span.after)

	var above []token.Pos

	for _, pos := range v.sortedCommentPos {
		if pos >= span.caseStart {
			break
		}

		if _, found := v.standAloneComments[pos]; found && pos > span.after && v.file.Line(pos) > afterLine {
			above = append(above, pos)
		}
	}

	for _, pos := range above {
		v.writeOutput("%s", strings.ReplaceAll(v.standAloneComments[pos], "\n", v.newline))
		v.outputBuilder.WriteString(v.newline)

		delete(v.standAloneComments, pos)
		v.sortedCommentPos = removeCommentPos(v.sortedCommentPos, pos)
	}
}

// clauseSpan is a clause's source: after the previous clause (or the body's `{`) and before the next
// clause (or the body's `}`). caseStart is its `case` or `default` keyword, end the end of its last
// statement (or its colon), and caseColumn the keyword's column.
type clauseSpan struct {
	after, caseStart, before, end token.Pos
	caseColumn                    int
}

// noteClauseSpans records the clauseSpan of each clause of a switch, type switch or select body, in
// source order: holdClauseComments holds a moved clause's comments by it, and writeLeadingBodyComments
// writes an empty clause's comments up to its end.
func (v *Visitor) noteClauseSpans(body *ast.BlockStmt) {
	if !v.options.includeComments || body == nil {
		return
	}

	if v.clauseSpans == nil {
		v.clauseSpans = map[token.Pos]clauseSpan{}
	}

	after := body.Lbrace

	for i, stmt := range body.List {
		before := body.Rbrace

		if i+1 < len(body.List) {
			before = body.List[i+1].Pos()
		}

		switch clause := stmt.(type) {
		case *ast.CaseClause:
			v.clauseSpans[clause.Colon] = clauseSpan{after, clause.Case, before, clause.End(), v.fset.Position(clause.Case).Column}
		case *ast.CommClause:
			v.clauseSpans[clause.Colon] = clauseSpan{after, clause.Case, before, clause.End(), v.fset.Position(clause.Case).Column}
		}

		after = stmt.End()
	}
}

// holdClauseComments takes the pending comments of the clause at colon out of the flush index, so a
// flush while clauses emitted ahead of it are converted cannot write them there: the if/else lowering
// and select move a leading `default:` clause to the end, and every flush writes all pending comments
// ahead of its target, in an argument list as readily as above a statement. releaseClauseComments puts
// them back when the clause's body starts, and the switch or select releases them again when it ends,
// so none is lost when the body is not converted through visitStatementList.
func (v *Visitor) holdClauseComments(colon token.Pos) {
	span, spanned := v.clauseSpans[colon]

	if !spanned {
		return
	}

	var held, kept []token.Pos

	// After the clause's last statement, a comment at the next label's column or left of it documents
	// the next clause (gofmt indents a body's comments deeper), so it is not held.
	for _, pos := range v.sortedCommentPos {
		if pos > span.after && pos < span.before && (pos < span.end || v.fset.Position(pos).Column > span.caseColumn) {
			held = append(held, pos)
		} else {
			kept = append(kept, pos)
		}
	}

	if len(held) == 0 {
		return
	}

	if v.heldComments == nil {
		v.heldComments = map[token.Pos][]token.Pos{}
	}

	v.heldComments[colon] = held
	v.sortedCommentPos = kept
}

// releaseClauseComments returns the comments holdClauseComments held for the clause at colon; a second
// release finds nothing.
func (v *Visitor) releaseClauseComments(colon token.Pos) {
	held, found := v.heldComments[colon]

	if !found {
		return
	}

	delete(v.heldComments, colon)
	v.sortedCommentPos = append(v.sortedCommentPos, held...)
	slices.Sort(v.sortedCommentPos)
}

// writeLeadingBodyComments writes, above a clause's first statement, the comments still pending ahead
// of it, as visitBlockStmt does for a block's first statement: those between the `:` line and the first
// statement, those above the `case` label, and one trailing the label. With no first statement (first
// is nil) it writes those of the empty body, up to the next clause. Left pending, they were written by
// the next flush, after the first statement or inside an argument list. A clause whose comments were
// held (holdClauseComments) gets them back first.
//
// Each comment is written on its own, with the prefix this body's statements take, and with Go's blank
// line between two groups below the `:` line and between the last of them and first, as visitBlockStmt
// writes them for a block (writeLeadingComments). It returns the output length before the blank line
// it wrote ahead of first, or -1 when it wrote none, for dropEmptyGap.
func (v *Visitor) writeLeadingBodyComments(colon token.Pos, first ast.Stmt) int {
	if !v.options.includeComments || !colon.IsValid() {
		return -1
	}

	v.releaseClauseComments(colon)

	span, spanned := v.clauseSpans[colon]

	if first == nil && !spanned {
		return -1
	}

	if v.file == nil {
		v.file = v.fset.File(colon)
	}

	before := span.before

	if first != nil {
		before = first.Pos()
	}

	prefix := v.newline + v.indent(v.indentLevel)

	var leading []token.Pos

	for _, pos := range v.sortedCommentPos {
		if pos >= before {
			break
		}

		if _, found := v.standAloneComments[pos]; found {
			leading = append(leading, pos)
		}
	}

	// A one-line comment on the `:` line trails the clause's opening line, as it trails `case X:` in Go.
	// It is written first, while the output's current line is still that header; the clause's other
	// leading comments follow on their own lines.
	colonLine := v.file.Line(colon)
	header := v.currentLine()
	trailHeader := header != "" && !strings.HasPrefix(header, "//") && !strings.HasPrefix(header, "/*")

	var onLabel, ownLine []token.Pos

	for _, pos := range leading {
		if trailHeader && pos > colon && v.file.Line(pos) == colonLine && !strings.Contains(v.standAloneComments[pos], "\n") {
			onLabel = append(onLabel, pos)
		} else {
			ownLine = append(ownLine, pos)
		}
	}

	for _, pos := range onLabel {
		v.outputBuilder.WriteString(" ")
		v.outputBuilder.WriteString(v.standAloneComments[pos])

		delete(v.standAloneComments, pos)
		v.sortedCommentPos = removeCommentPos(v.sortedCommentPos, pos)
	}

	// lastLine is the last line written below the `:` line; comments above the label do not move it, as no
	// blank line is kept among them.
	lastLine := colonLine

	for _, pos := range ownLine {
		line := v.file.Line(pos)

		if line > colonLine && lastLine > colonLine && line-lastLine > 1 {
			v.writeGapLine()
		}

		v.outputBuilder.WriteString(prefix)
		v.outputBuilder.WriteString(strings.ReplaceAll(v.standAloneComments[pos], "\n", v.newline))

		if line > colonLine {
			lastLine = max(lastLine, line+strings.Count(v.standAloneComments[pos], "\n"))
		}

		delete(v.standAloneComments, pos)
		v.sortedCommentPos = removeCommentPos(v.sortedCommentPos, pos)
	}

	if first != nil && lastLine > colonLine && v.goBlankLineBetween(v.file.LineStart(lastLine), first.Pos()) {
		mark := v.outputBuilder.Len()

		if v.writeGapLine() {
			return mark
		}
	}

	return -1
}

// writeLeadingComments writes the comments still pending ahead of a block's first statement, each group on
// its own lines, with Go's blank line between two groups and between the last group and first, as
// writeStatementGap writes them between two statements. No blank line is written directly after the `{`
// (writeGapLine refuses it, and gofmt keeps none there). It returns the output length before the blank
// line it wrote ahead of first, or -1 when it wrote none, for dropEmptyGap.
func (v *Visitor) writeLeadingComments(lbrace token.Pos, first ast.Stmt, prefix string) int {
	if v.file == nil {
		v.file = v.fset.File(first.Pos())
	}

	var pending []token.Pos

	for _, pos := range v.sortedCommentPos {
		if pos >= first.Pos() {
			break
		}

		if _, found := v.standAloneComments[pos]; found {
			pending = append(pending, pos)
		}
	}

	lastLine := v.file.Line(lbrace)
	wrote := false

	for _, pos := range pending {
		line := v.file.Line(pos)

		if wrote && line-lastLine > 1 {
			v.writeGapLine()
		}

		lines := strings.Count(v.standAloneComments[pos], "\n")
		v.writeStandAloneCommentString(v.outputBuilder, pos, nil, prefix)
		lastLine = max(lastLine, line+lines)
		wrote = true
	}

	if wrote && v.goBlankLineBetween(v.file.LineStart(lastLine), first.Pos()) {
		mark := v.outputBuilder.Len()

		if v.writeGapLine() {
			return mark
		}
	}

	return -1
}

// writeLineTrailingComments writes a one-line comment Go has after a block's `{`, on the same line and
// before limit, at the end of the `{` line, as Go has it (class H). It does nothing unless the output's
// current line ends with that `{`: a block whose opening line is followed by converter text (a
// function body's prelude placeholder) keeps the comment on its own line below, as before.
func (v *Visitor) writeLineTrailingComments(lbrace, limit token.Pos) {
	if !v.options.includeComments || !lbrace.IsValid() {
		return
	}

	if v.file == nil {
		v.file = v.fset.File(lbrace)
	}

	output := v.outputBuilder.String()
	raw := strings.TrimRight(stripPositionSentinels(output[strings.LastIndex(output, "\n")+1:]), " \t\r")

	if !strings.HasSuffix(raw, "{") {
		return
	}

	braceLine := v.file.Line(lbrace)

	var trailing []token.Pos

	for _, pos := range v.sortedCommentPos {
		if pos >= limit || v.file.Line(pos) > braceLine {
			break
		}

		if comment, found := v.standAloneComments[pos]; found && pos > lbrace && !strings.Contains(comment, "\n") {
			trailing = append(trailing, pos)
		}
	}

	for _, pos := range trailing {
		v.outputBuilder.WriteString(" ")
		v.outputBuilder.WriteString(v.standAloneComments[pos])

		delete(v.standAloneComments, pos)
		v.sortedCommentPos = removeCommentPos(v.sortedCommentPos, pos)
	}
}

// writeClosingComments writes the comments still pending before limit (a block's `}`, or the end of a
// case body) on their own lines at the end of the body, with Go's blank line before a comment where it
// has one (class H). after is the body's last statement end, or its `{` when the body is empty; with
// minColumn above 0, only comments indented deeper than that column are taken (a case body's, not the
// next label's). Left pending, the next flush wrote them after the block, in what follows it.
func (v *Visitor) writeClosingComments(after, limit token.Pos, minColumn int) {
	if !v.options.includeComments || !limit.IsValid() || !after.IsValid() {
		return
	}

	if v.file == nil {
		v.file = v.fset.File(limit)
	}

	prefix := v.newline + v.indent(v.indentLevel)
	lastLine := v.file.Line(after)

	var pending []token.Pos

	for _, pos := range v.sortedCommentPos {
		if pos >= limit {
			break
		}

		if _, found := v.standAloneComments[pos]; found && (minColumn == 0 || pos > after && v.fset.Position(pos).Column > minColumn) {
			pending = append(pending, pos)
		}
	}

	for _, pos := range pending {
		line := v.file.Line(pos)

		if line-lastLine > 1 {
			v.writeGapLine()
		}

		v.writeStandAloneCommentString(v.outputBuilder, pos, nil, prefix)
		lastLine = max(lastLine, line+strings.Count(v.standAloneComments[pos], "\n"))
	}
}

// writeStatementGap writes what lies between two statements of one list in the Go source: the
// standalone comments still pending before next, and a blank line wherever Go has one, before a
// comment group, between two groups, and before next itself. gofmt keeps at most one blank line in a
// row, so a gap of any size writes one. Comments before prev's last line (inside prev, or trailing it
// and not yet claimed) are flushed in order with no blank line, as before.
//
// In every conversion (docs/PLAN-comment-and-spacing-parity.md, class A; the owner's decision 1).
// Without `-comments` no comment is written, so the gap is one blank line when Go has any blank line in
// it (goBlankLineBetween).
//
// It returns the output length before the blank line it wrote ahead of next, or -1 when it wrote none,
// for dropEmptyGap.
func (v *Visitor) writeStatementGap(prev, next ast.Stmt) int {
	if v.file == nil {
		v.file = v.fset.File(next.Pos())
	}

	if !v.options.includeComments {
		if v.goBlankLineBetween(prev.End(), next.Pos()) {
			mark := v.outputBuilder.Len()

			if v.writeGapLine() {
				return mark
			}
		}

		return -1
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

	// Comment-aware, as goBlankLineBetween is, not a raw line difference: a comment group that is next's own
	// doc (a var or const statement's) is written with next rather than flushed above, so its lines are not
	// blank ones.
	if v.goBlankLineBetween(v.file.LineStart(lastLine), nextPos) {
		mark := v.outputBuilder.Len()

		if v.writeGapLine() {
			return mark
		}
	}

	return -1
}

// goBlankLineBetween reports whether a line between from's line and to's line is blank in the Go source:
// not on either end's line, and covered by no comment group.
func (v *Visitor) goBlankLineBetween(from, to token.Pos) bool {
	last := v.file.Line(from)
	first := sort.Search(len(v.commentGroups), func(i int) bool { return v.commentGroups[i].Pos() > from })

	for _, group := range v.commentGroups[first:] {
		if group.Pos() >= to {
			break
		}

		if v.file.Line(group.Pos())-last > 1 {
			return true
		}

		last = max(last, v.file.Line(group.End()))
	}

	return v.file.Line(to)-last > 1
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
	line := v.currentLine()

	// An empty current line is a break already written, or nothing emitted yet in this block's own
	// builder (a function body's brace can sit in the enclosing builder).
	if line == "" || strings.HasSuffix(line, "{") {
		return false
	}

	v.outputBuilder.WriteString(v.newline)

	return true
}

// trimTrailingLineBreak removes the line break a converted switch ends with. Every statement opens with
// its own line break, as does the enclosing block's `}`, so the switch's own made a blank line Go never
// has: after a switch Go writes adjacently to the next statement, and before the `}` of a block that ends
// with one. Where Go has a blank line after the switch, writeStatementGap writes it.
//
// Under `-comments` only (docs/PLAN-comment-and-spacing-parity.md, class E1; the owner's decision 1 took
// only class A into every conversion): the behavioral goldens, captured without it, keep the line break,
// and writeGapLine then writes no second one.
func (v *Visitor) trimTrailingLineBreak() {
	if !v.options.includeComments {
		return
	}

	output := v.outputBuilder.String()

	if !strings.HasSuffix(output, v.newline) {
		return
	}

	v.outputBuilder.Reset()
	v.outputBuilder.WriteString(strings.TrimSuffix(output, v.newline))
}

// currentLine returns the line the output builder ends with, as it will be emitted: without position
// sentinels, `>>MARKER:…<<` placeholders and trailing whitespace. It is empty after a line break.
func (v *Visitor) currentLine() string {
	output := v.outputBuilder.String()
	lineStart := strings.LastIndex(output, "\n") + 1

	return strings.TrimRight(stripEmissionMarkers(stripPositionSentinels(output[lineStart:])), " \t\r")
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
//
// Without `-comments` it also removes the blank line ahead of a statement whose own first line is blank
// or holds only indentation (one Go leads with a comment that is not written, a variable initialized by
// a func literal): that line already sits where Go's blank line is, and a second would make two in a
// row, which gofmt never writes.
func (v *Visitor) dropEmptyGap(mark int) {
	if mark < 0 {
		return
	}

	output := v.outputBuilder.String()
	end := mark + len(v.newline)

	if end > len(output) || output[mark:end] != v.newline {
		return
	}

	if after := stripPositionSentinels(output[end:]); strings.TrimSpace(after) != "" && (v.options.includeComments || !opensWithBlankLine(after, v.newline)) {
		return
	}

	v.outputBuilder.Reset()
	v.outputBuilder.WriteString(output[:mark])
	v.outputBuilder.WriteString(output[end:])
}

// opensWithBlankLine reports whether text, a statement's emission, begins with a line break and then a
// line that is empty or holds only indentation.
func opensWithBlankLine(text, newline string) bool {
	rest, found := strings.CutPrefix(text, newline)

	if !found {
		return false
	}

	line, _, complete := strings.Cut(rest, newline)

	return complete && strings.TrimSpace(line) == ""
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
