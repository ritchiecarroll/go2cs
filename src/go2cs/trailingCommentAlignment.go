// trailingCommentAlignment.go - Gbtc
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
	"os"
	"path/filepath"
	"sort"
	"strconv"
	"strings"
	"sync"
	"unicode/utf8"
)

// END-OF-LINE COMMENT ALIGNMENT (docs/PLAN-comment-and-spacing-parity.md, class J).
//
// gofmt aligns the trailing comments of consecutive lines into one column, and a Go reader expects the converted C#
// to read the same way. The runs are read from GO, not from C#: the Go source is gofmt's output, so a run of
// consecutive lines whose trailing comments sit in one column is exactly a run gofmt aligned, and every rule gofmt
// applied to decide it (a blank line, a change of indentation, a section break after a statement spanning lines,
// the key-size ratio of a composite literal) is already in the columns. Nothing of gofmt is re-derived on C#.
//
// Three steps:
//
//  1. trailingCommentRuns gives each Go trailing `//` comment of the file the run it belongs to.
//  2. Wherever the converter writes such a comment, the text carries an invisible run sentinel right after its `//`
//     (markTrailingComment): `//` SENTINEL <decimal run> SENTINEL. Every writer takes the comment's text from
//     standAloneComments, writeCommentString or a split string constant's operands (convBasicLit), so marking the
//     text there marks it in every placement, and the comment keeps its `//` prefix for the writers that test it.
//  3. Once the file's text is finished, alignTrailingComments pads every C# line of one run to one column, the
//     longest code plus one space counted in runes (as text/tabwriter counts), and strips the sentinels. A run the
//     converter splits with lines of its own (a func literal lowered to a local function) still pads to one column.
//     A comment alone in its run gets one space.
//
// Only Go's comments carry a sentinel, so the converter's own (`// end X_package`, the deferred-call note) and every
// /* fact */ block comment, which sits in the CODE part of a line, are never moved. Padding changes the spaces
// between a line's code and its comment and nothing else: no line is added or removed, so the position map's C# line
// numbers hold, and no code column moves.
//
// The deferred-marker passes resolve type names AFTER a file is written, which changes the width of the code on a
// marked line. The pass records each padded run against the file it was written to (registerAlignedRuns), and
// rewriteDeferredMarkers re-pads the runs on a file it rewrote (repadAlignedRuns).

// alignSentinelOpen and alignSentinelClose delimit the run sentinel after a marked comment's `//`. Neither occurs in
// emitted C# outside a sentinel: Go source text cannot hold them unescaped except inside a comment, and a comment that
// starts with one right after its `//` is not a thing Go code carries.
const (
	alignSentinelOpen  = "\x01"
	alignSentinelClose = "\x02"
	alignSentinelMark  = "//" + alignSentinelOpen
)

// trailingCommentRuns returns the run of every trailing `//` comment in file: the Go line of the run's first comment,
// keyed by the comment's position. A comment is trailing when code precedes it on its line. A run is a sequence of
// trailing comments on consecutive lines with the same indentation and the same visual column (tabs at 8, counted in
// runes), the shape gofmt's alignment leaves. The columns are read from the file's source; a file whose source cannot
// be read has no runs, and its comments keep the placement they had without this pass.
func trailingCommentRuns(fset *token.FileSet, file *ast.File) map[token.Pos]int {
	tokenFile := fset.File(file.Pos())

	if tokenFile == nil {
		return nil
	}

	source, err := os.ReadFile(tokenFile.Name())

	if err != nil {
		return nil
	}

	lines := strings.Split(string(source), "\n")

	type trailing struct {
		pos          token.Pos
		line, column int
		indent       string
	}

	var comments []trailing

	for _, group := range file.Comments {
		for _, comment := range group.List {
			if !strings.HasPrefix(comment.Text, "//") {
				continue
			}

			position := fset.Position(comment.Slash)

			if position.Line < 1 || position.Line > len(lines) || position.Column-1 > len(lines[position.Line-1]) {
				continue
			}

			prefix := lines[position.Line-1][:position.Column-1]

			if strings.TrimSpace(prefix) == "" {
				continue
			}

			comments = append(comments, trailing{
				pos:    comment.Slash,
				line:   position.Line,
				column: visualColumn(prefix),
				indent: prefix[:len(prefix)-len(strings.TrimLeft(prefix, " \t"))],
			})
		}
	}

	sort.Slice(comments, func(i, j int) bool { return comments[i].pos < comments[j].pos })

	runs := make(map[token.Pos]int, len(comments))

	for i, comment := range comments {
		run := comment.line

		if i > 0 {
			previous := comments[i-1]

			if previous.line == comment.line-1 && previous.column == comment.column && previous.indent == comment.indent {
				run = runs[previous.pos]
			}
		}

		runs[comment.pos] = run
	}

	return runs
}

// visualColumn is the column text ends at, with tabs at 8 and every other rune one column.
func visualColumn(text string) int {
	column := 0

	for _, r := range text {
		if r == '\t' {
			column += 8 - column%8
		} else {
			column++
		}
	}

	return column
}

// markTrailingComment returns a comment's text carrying its run sentinel when it is a Go trailing `//` comment, and
// the text unchanged otherwise.
func (v *Visitor) markTrailingComment(pos token.Pos, text string) string {
	run, found := v.trailingRuns[pos]

	if !found || !strings.HasPrefix(text, "//") {
		return text
	}

	return alignSentinelMark + strconv.Itoa(run) + alignSentinelClose + text[2:]
}

// alignedRun is one padded run: the 0-based lines of the text it was padded in, and the comment that ends each line.
type alignedRun struct {
	lines    []int
	comments []string
}

// alignTrailingComments pads every line of each marked run in text to one column and strips every sentinel. It
// returns the text and the runs it padded, in order of their first line. The number of lines never changes.
func alignTrailingComments(text string) (string, []alignedRun) {
	if !strings.Contains(text, alignSentinelMark) {
		return text, nil
	}

	lines := strings.Split(text, "\n")
	byRun := map[string]*alignedRun{}
	var order []string

	for i, line := range lines {
		at := strings.Index(line, alignSentinelMark)

		if at < 0 {
			continue
		}

		closing := strings.Index(line[at+len(alignSentinelMark):], alignSentinelClose)

		if closing < 0 {
			lines[i] = stripAlignSentinels(line)
			continue
		}

		run := line[at+len(alignSentinelMark) : at+len(alignSentinelMark)+closing]
		code := line[:at]
		comment := "//" + stripAlignSentinels(line[at+len(alignSentinelMark)+closing+len(alignSentinelClose):])

		// A comment the converter placed on a line of its own keeps that line's indentation; it has no code to
		// align against.
		if strings.TrimSpace(code) == "" {
			lines[i] = code + comment
			continue
		}

		entry, found := byRun[run]

		if !found {
			entry = &alignedRun{}
			byRun[run] = entry
			order = append(order, run)
		}

		entry.lines = append(entry.lines, i)
		entry.comments = append(entry.comments, strings.TrimSuffix(comment, "\r"))
		lines[i] = code + comment
	}

	runs := make([]alignedRun, 0, len(order))

	for _, run := range order {
		runs = append(runs, *byRun[run])
	}

	padAlignedRuns(lines, runs)

	return strings.Join(lines, "\n"), runs
}

// padAlignedRuns pads each run's lines to the run's column: the longest code, trailing spaces removed, plus one
// space, in runes. Each line must end with its run's comment (a carriage return aside); a run with a line that does
// not is left as it is.
func padAlignedRuns(lines []string, runs []alignedRun) {
	for _, run := range runs {
		codes := make([]string, len(run.lines))
		column := 0
		intact := true

		for k, index := range run.lines {
			if index < 0 || index >= len(lines) {
				intact = false
				break
			}

			line := strings.TrimSuffix(lines[index], "\r")

			if !strings.HasSuffix(line, run.comments[k]) {
				intact = false
				break
			}

			codes[k] = strings.TrimRight(strings.TrimSuffix(line, run.comments[k]), " ")
			column = max(column, utf8.RuneCountInString(codes[k])+1)
		}

		if !intact {
			continue
		}

		for k, index := range run.lines {
			ending := ""

			if strings.HasSuffix(lines[index], "\r") {
				ending = "\r"
			}

			lines[index] = codes[k] + strings.Repeat(" ", column-utf8.RuneCountInString(codes[k])) + run.comments[k] + ending
		}
	}
}

// stripAlignSentinels removes every run sentinel from text.
func stripAlignSentinels(text string) string {
	for {
		at := strings.Index(text, alignSentinelMark)

		if at < 0 {
			return text
		}

		closing := strings.Index(text[at+len(alignSentinelMark):], alignSentinelClose)

		if closing < 0 {
			return text[:at+2] + text[at+len(alignSentinelMark):]
		}

		text = text[:at+2] + text[at+len(alignSentinelMark)+closing+len(alignSentinelClose):]
	}
}

// alignEmittedTrailingComments aligns the finished output text and keeps the runs it padded for the writer to
// register. It runs after finalizePositionMap, whose sentinels would otherwise count as code width.
func (v *Visitor) alignEmittedTrailingComments() {
	text, runs := alignTrailingComments(v.outputBuilder.String())
	v.alignedRuns = runs

	if runs == nil && !strings.Contains(text, alignSentinelMark) {
		return
	}

	v.outputBuilder.Reset()
	v.outputBuilder.WriteString(text)
}

var (
	alignedRunsLock   sync.Mutex
	alignedRunsByFile = map[string][]alignedRun{}
)

// registerAlignedRuns records the runs padded in a file's text, which starts lineOffset lines into the written file,
// for repadAlignedRuns. A file written again replaces its record.
func registerAlignedRuns(fileName string, runs []alignedRun, lineOffset int) {
	shifted := make([]alignedRun, 0, len(runs))

	for _, run := range runs {
		if len(run.lines) < 2 {
			continue
		}

		lines := make([]int, len(run.lines))

		for k, index := range run.lines {
			lines[k] = index + lineOffset
		}

		shifted = append(shifted, alignedRun{lines: lines, comments: run.comments})
	}

	alignedRunsLock.Lock()
	defer alignedRunsLock.Unlock()

	key := filepath.Clean(fileName)

	if len(shifted) == 0 {
		delete(alignedRunsByFile, key)
		return
	}

	alignedRunsByFile[key] = shifted
}

// repadAlignedRuns pads the runs recorded for fileName again in content, the file's text after a deferred-marker pass
// changed the width of some of its lines. A file with no record is returned as it is.
func repadAlignedRuns(fileName, content string) string {
	alignedRunsLock.Lock()
	runs := alignedRunsByFile[filepath.Clean(fileName)]
	alignedRunsLock.Unlock()

	if len(runs) == 0 {
		return content
	}

	lines := strings.Split(content, "\n")
	padAlignedRuns(lines, runs)

	return strings.Join(lines, "\n")
}
