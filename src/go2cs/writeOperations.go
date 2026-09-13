// writeOperations.go - Gbtc
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
	"go/token"
	"sort"
	"strings"
)

// The converter emits CRLF UNCONDITIONALLY — every writer in this tree terminates its lines with
// "\r\n" so the output is byte-deterministic on every host, and .gitattributes pins the emitted
// artifact types to eol=crlf so every checkout materializes those same bytes.
//
// The files it READS BACK are the other half of that contract. package_info.cs and
// package_test_info.cs are committed artifacts that the converter parses on the next run, so their
// endings are whatever the CHECKOUT produced. A tree materialized before the pin — or one cloned
// with core.autocrlf=false, git's default everywhere but Windows — hands the reader LF content that
// no CRLF-shaped scan can match, and the failure is not a diff but a hard stop: the marker sections
// are never found and the conversion log.Fatals (F3 in docs/PLAN-linux-operation.md).
//
// These helpers are that seam. They belong to the READ path only; nothing here changes what is
// written, which is why applying them cannot move emitted bytes on any platform.

// normalizeToLF collapses CRLF to bare LF so read-back content can be scanned or split into lines
// independently of how it was checked out.
func normalizeToLF(content string) string {
	return strings.ReplaceAll(content, "\r\n", "\n")
}

// normalizeToCRLF rewrites read-back content into the converter's own uniform CRLF form, so
// CRLF-shaped searches and inserts behave identically on every checkout. Idempotent, and an exact
// no-op on content that is already uniformly CRLF — which is what lets a caller keep an
// unchanged-content early-out and stay byte-identical on a Windows tree.
func normalizeToCRLF(content string) string {
	return strings.ReplaceAll(normalizeToLF(content), "\n", "\r\n")
}

// splitLines splits read-back content into lines EOL-agnostically. Note it treats a BARE LF as a
// line break too, matching writePackageInfoFile's re-emission (which terminates every element with
// "\r\n"): a package_info.cs is uniformly CRLF by construction, so the two agree by design.
func splitLines(content string) []string {
	return strings.Split(normalizeToLF(content), "\n")
}

func (v *Visitor) indent(indentLevel int) string {
	return strings.Repeat(" ", v.options.indentSpaces*indentLevel)
}

func (v *Visitor) isLineFeedBetween(prevEndPos, currPos token.Pos) bool {
	prevLine := v.fset.Position(prevEndPos).Line
	currLine := v.fset.Position(currPos).Line
	return currLine > prevLine
}

func (v *Visitor) removeLastLineFeed(builder *strings.Builder) {
	currentResult := builder.String()
	currentResult = strings.TrimSuffix(currentResult, v.newline)
	builder.Reset()
	builder.WriteString(currentResult)
}

func (v *Visitor) writeString(builder *strings.Builder, format string, a ...interface{}) {
	if v.indentLevel > 0 {
		builder.WriteString(v.indent(v.indentLevel))
	}

	builder.WriteString(fmt.Sprintf(format, a...))
}

func (v *Visitor) writeStringLn(builder *strings.Builder, format string, a ...interface{}) {
	v.writeString(builder, format, a...)
	builder.WriteString(v.newline)
}

// TODO: Come back to this later, you could spend a lot of time here - there is an ongoing effort to improve AST for
// parsing free floating comments: https://github.com/golang/go/issues/20744 - plus a DST package as a fallback
func (v *Visitor) writeStandAloneCommentString(builder *strings.Builder, targetPos token.Pos, doc *ast.CommentGroup, prefix string) (bool, int) {
	wroteStandAloneComment := false
	lines := 0

	if !v.options.includeComments {
		return wroteStandAloneComment, lines
	}

	// Handle standalone comments that may precede the target position
	if targetPos != token.NoPos {
		if v.file == nil {
			v.file = v.fset.File(targetPos)
		}

		handledPos := []token.Pos{}

		for _, pos := range v.sortedCommentPos {
			if pos > targetPos {
				break
			}

			comment, found := v.standAloneComments[pos]

			if !found {
				continue
			}

			comment = strings.ReplaceAll(comment, "\n", v.newline)

			if !strings.Contains(prefix, "\n") && !strings.HasSuffix(comment, v.newline) {
				comment += v.newline
			}

			lines += strings.Count(comment, "\n")

			builder.WriteString(prefix)
			builder.WriteString(comment)

			delete(v.standAloneComments, pos)
			handledPos = append(handledPos, pos)
			wroteStandAloneComment = true
		}

		if len(handledPos) > 0 {
			lastCommentPos := handledPos[len(handledPos)-1]

			// Add line breaks if there is a gap between the last comment and the target position
			if lastCommentPos > token.NoPos && doc != nil {
				docPos := doc.Pos()
				targetLine := v.file.Line(lastCommentPos)
				nodeLine := v.file.Line(docPos)

				if int(nodeLine-targetLine)-1 > 0 {
					builder.WriteString(v.newline)
				}
			}

			// Remove handled positions from sorted list
			for _, pos := range handledPos {
				v.sortedCommentPos = removeCommentPos(v.sortedCommentPos, pos)
			}
		}
	}

	return wroteStandAloneComment, lines
}

// removeCommentPos drops one position from the sorted standalone-comment index, which is how a
// comment is retired once it has been written somewhere.
func removeCommentPos(slice []token.Pos, pos token.Pos) []token.Pos {
	for i, p := range slice {
		if p == pos {
			return append(slice[:i], slice[i+1:]...)
		}
	}

	return slice
}

// discardStandAloneComments retires — WITHOUT emitting — every free-floating comment positioned
// inside [fromPos, toPos].
//
// Every other retirement in this file happens because a comment was written somewhere. This one
// exists for the single shape that has no writer at all: a declaration DISPLACED to a hand-own —
// visitFuncDecl's manualConversionFuncs placeholder, and visitTypeSpec's manualConversionTypes twin
// — is never visited, so the comments inside its span never reach an emission point. Leaving them
// in the sink does not merely delay them, it MISPLACES them — the drain is positional, so the NEXT
// declaration's writeDoc flushes everything standing before it and a displaced declaration's
// commentary resurfaces as that declaration's doc comment (syscall_linux's recvmsgRaw comment
// reading as sendmsgN's) or, when the displaced declaration was the file's last, as a block after
// the class's closing brace (runtime mbitmap's getgcmask, 32 lines). Discarding is the faithful
// answer rather than a lossy one: those comments document a declaration this file does not contain,
// and the hand-own that does contain it carries its own.
//
// Only the sink is touched, so a comment the AST attached to a node (a Doc group) is unaffected —
// visitFile already removed those, and they travel with the node they belong to.
func (v *Visitor) discardStandAloneComments(fromPos, toPos token.Pos) {
	if !v.options.includeComments || fromPos == token.NoPos || toPos == token.NoPos {
		return
	}

	// The index is sorted and a FileSet lays every position of one file ahead of the next, so the
	// span is one contiguous run — seek to its start rather than rescanning the file's comments.
	first := sort.Search(len(v.sortedCommentPos), func(i int) bool { return v.sortedCommentPos[i] >= fromPos })
	discarded := 0

	for _, pos := range v.sortedCommentPos[first:] {
		if pos > toPos {
			break
		}

		delete(v.standAloneComments, pos)
		discarded++
	}

	if discarded > 0 {
		v.sortedCommentPos = append(v.sortedCommentPos[:first], v.sortedCommentPos[first+discarded:]...)
	}
}

// writeTrailingComment appends the free-floating comment(s) sitting on the SAME source line as the
// just-emitted statement's end, so a Go end-of-line comment stays an end-of-line comment.
//
// Go's AST attaches comments to declarations, fields and specs but NEVER to statements, so a
// statement's trailing comment is "standalone" (visitFile) and — with nothing claiming it where it
// belongs — was flushed at the next emission point instead: on its own line AFTER the statement, and,
// when the statement was the LAST of its block, after that block's closing brace, escaping the
// construct it documented entirely (`pow[i] = 1 << uint(i) // == 2**i` losing its comment out of the
// enclosing loop; a func body's final comment landing past the method's `}`).
//
// Only the last emitted line is a candidate: a Go statement can lower to several C# lines, and the
// comment belongs at the end of that whole emission, which is exactly where the output builder
// stands when this is called. Callers are the statement-LIST slots (visitListStmt) — the one place a
// statement's text is known to end its line.
//
// Returns whether anything was written.
func (v *Visitor) writeTrailingComment(stmtEnd token.Pos) bool {
	if !v.options.includeComments || stmtEnd == token.NoPos || len(v.sortedCommentPos) == 0 {
		return false
	}

	endPos := v.fset.Position(stmtEnd)
	handledPos := []token.Pos{}
	trailing := strings.Builder{}

	// A comment INSIDE the statement's own span (`x := /* why */ 1`) is not a trailing one, and one
	// before it was consumed long ago; the index is sorted, so seek past both rather than rescanning
	// the file's comments once per statement.
	first := sort.Search(len(v.sortedCommentPos), func(i int) bool { return v.sortedCommentPos[i] > stmtEnd })

	for _, pos := range v.sortedCommentPos[first:] {
		commentPos := v.fset.Position(pos)

		// sortedCommentPos is ordered and a FileSet lays every position of one file ahead of the
		// next, so the first comment past this line — or past this file — ends the run.
		if commentPos.Filename != endPos.Filename || commentPos.Line != endPos.Line {
			break
		}

		comment, found := v.standAloneComments[pos]

		if !found {
			continue
		}

		// A block comment that opens on this line but spans further cannot be tucked onto it; leave
		// it to the standalone path, which writes it with the indentation its own lines need.
		if strings.ContainsAny(comment, "\r\n") {
			break
		}

		trailing.WriteString(" ")
		trailing.WriteString(comment)

		handledPos = append(handledPos, pos)
	}

	if len(handledPos) == 0 {
		return false
	}

	// Most statements leave the builder mid-line (each writes its own LEADING newline), but a few
	// close their own line — visitSwitchStmt ends its emission with one. Tuck the comment in AHEAD of
	// that terminator, or it would open the next line at column zero instead of trailing the
	// statement.
	current := v.outputBuilder.String()
	lineTerminator := ""

	if strings.HasSuffix(current, v.newline) {
		lineTerminator = v.newline
		v.outputBuilder.Reset()
		v.outputBuilder.WriteString(strings.TrimSuffix(current, v.newline))
	}

	v.outputBuilder.WriteString(trailing.String())
	v.outputBuilder.WriteString(lineTerminator)

	for _, pos := range handledPos {
		delete(v.standAloneComments, pos)
		v.sortedCommentPos = removeCommentPos(v.sortedCommentPos, pos)
	}

	return true
}

func (v *Visitor) writeDocString(builder *strings.Builder, doc *ast.CommentGroup, targetPos token.Pos) {
	if !v.options.includeComments {
		return
	}

	/*wroteStandAloneComment_, _ :=*/
	v.writeStandAloneCommentString(builder, targetPos, doc, "")

	if doc == nil {
		return
	}

	// Handle doc comments
	v.writeString(builder, "") // Write indent
	v.writeCommentString(builder, doc, token.NoPos)
	builder.WriteString(v.newline)
}

func (v *Visitor) writeCommentString(builder *strings.Builder, comment *ast.CommentGroup, targetPos token.Pos) {
	if comment == nil || !v.options.includeComments {
		return
	}

	if !v.processedComments.Add(comment.Pos()) {
		return
	}

	for index, comment := range comment.List {
		if index > 0 {
			builder.WriteString(v.newline)
			v.writeString(builder, "") // Write indent
		}

		if targetPos > token.NoPos {
			padding := int(comment.Slash - targetPos)

			if padding < 1 {
				padding = 1
			}

			builder.WriteString(strings.Repeat(" ", padding))
		}

		builder.WriteString(comment.Text)
	}
}

func (v *Visitor) replaceMarkerString(builder *strings.Builder, marker string, replacement string) {
	builderString := strings.ReplaceAll(builder.String(), marker, replacement)
	builder.Reset()
	builder.WriteString(builderString)
}

// The five functions below are the ones nearly every visit*/conv* file calls. Each is the
// "current destination" form of a *String sibling above: it writes to v.outputBuilder, the
// in-memory buffer holding the C# emitted for the file so far.
//
// Which builder that IS changes during a walk — visitBlockStmt swaps in a fresh one so a nested
// block accumulates separately (see pushBlock) — so these deliberately read the field at call
// time rather than capturing it.
//
// Note the one name that does NOT belong to this family: writeOutputFile (projectFileWriter.go)
// is the function that actually touches disk, writing the finished builder contents out.

// writeOutput appends formatted text to the current output builder.
func (v *Visitor) writeOutput(format string, a ...interface{}) {
	v.writeString(v.outputBuilder, format, a...)
}

// writeOutputLn appends formatted text plus a newline to the current output builder.
func (v *Visitor) writeOutputLn(format string, a ...interface{}) {
	v.writeStringLn(v.outputBuilder, format, a...)
}

// writeDoc appends a Go doc comment to the current output builder, converted to C# doc form.
func (v *Visitor) writeDoc(doc *ast.CommentGroup, targetPos token.Pos) {
	v.writeDocString(v.outputBuilder, doc, targetPos)
}

// writeComment appends an ordinary Go comment to the current output builder.
func (v *Visitor) writeComment(comment *ast.CommentGroup, targetPos token.Pos) {
	v.writeCommentString(v.outputBuilder, comment, targetPos)
}

// replaceMarker substitutes a deferred placeholder already written into the current output
// builder — used when the final text only becomes known after more of the file has been visited.
func (v *Visitor) replaceMarker(marker string, replacement string) {
	v.replaceMarkerString(v.outputBuilder, marker, replacement)
}

// spliceOutput inserts text at an earlier byte offset of the current output builder — the
// positional twin of replaceMarker, for a caller that recorded where its statement's own output
// begins and only afterwards learns it must emit something ahead of it (visitRangeStmt's
// func-literal capture snapshots). The offset must have been taken on THIS builder; a nested block
// swaps the builder and restores it, so an offset recorded before such a call is still valid.
func (v *Visitor) spliceOutput(offset int, text string) {
	if text == "" {
		return
	}

	current := v.outputBuilder.String()

	if offset < 0 || offset > len(current) {
		return
	}

	v.outputBuilder.Reset()
	v.outputBuilder.WriteString(current[:offset])
	v.outputBuilder.WriteString(text)
	v.outputBuilder.WriteString(current[offset:])
}
