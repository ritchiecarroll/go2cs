// trailingCommentAlignment.go - Gbtc
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
	"math"
	"os"
	"regexp"
	"strconv"
	"strings"
	"sync"
	"unicode/utf8"
)

// Trailing-comment alignment (class J of docs/PLAN-comment-and-spacing-parity.md; the owner's decision 4): the
// comments Go aligned are aligned in C# with gofmt's rules, and no other comment moves.
//
// gofmt aligns a run of trailing comments with text/tabwriter: consecutive lines at one indentation that each end in a
// comment form a block, and every comment of the block starts one column after the block's longest code, counted in
// runes. go/printer's exprList also starts a new section inside an element list when an element's size jumps against
// the running geometric mean (ratio 2.5). The converter cannot copy Go's columns, since C# code has other widths, so it
// repeats the rule on the C# text: a comment from a Go run that gofmt aligned (two or more consecutive Go lines at one
// indentation whose trailing comments share a column) is written followed by an invisible marker naming its run, and
// once the file's text is final one pass pads each block of consecutive marked lines of one run and strips the
// markers. Every other trailing comment keeps the single space it is written with. The pass changes only spaces, never
// a line count, so the position map is unaffected.
//
// The marker follows the comment rather than preceding it, so code that inspects a comment's text before it is written
// (its `//` or `/*` opening) reads it unchanged. It marks a comment only where it ends its line: a `//` comment always
// does, and a single-line `/* */` comment does as a spec's or field's trailing comment (writeCommentString), so a free
// block comment, which can sit inside an expression, is never marked.

// AlignMarker delimits a trailing-comment alignment marker: AlignMarker <run> : <comment length in bytes>
// AlignMarker, written directly after the comment. Like the position sentinel it is a byte that cannot occur in
// emitted C#.
const AlignMarker = "\x01"

// goAlignedRuns answers, for a file's single-line trailing comments that sit in a run gofmt aligned, the run each
// belongs to: two or more consecutive Go lines at one indentation, each ending in a comment, all at one visual column
// (tabs at 8). A stretch whose columns differ holds such runs where neighbouring lines share a column: a struct's
// fields with and without a tag put their comments in different cells, and those in one cell are aligned.
func goAlignedRuns(fset *token.FileSet, file *ast.File) map[token.Pos]int {
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
		line, column int
		indent       string
		slash        token.Pos
	}

	var entries []trailing

	for _, group := range file.Comments {
		start := fset.Position(group.Pos())

		if start.Line < 1 || start.Line > len(lines) || fset.Position(group.End()).Line != start.Line {
			continue
		}

		text := lines[start.Line-1]

		if start.Column-1 > len(text) || strings.TrimSpace(text[:start.Column-1]) == "" {
			continue // own-line comment
		}

		first := group.List[0].Text

		if strings.HasPrefix(first, "//go:") || strings.HasPrefix(first, "//line ") {
			continue
		}

		column := 0

		for _, r := range text[:start.Column-1] {
			if r == '\t' {
				column = (column/8 + 1) * 8
			} else {
				column++
			}
		}

		slash := token.NoPos

		if len(group.List) == 1 {
			slash = group.List[0].Slash
		}

		entries = append(entries, trailing{start.Line, column, text[:len(text)-len(strings.TrimLeft(text, " \t"))], slash})
	}

	runs := map[token.Pos]int{}
	run := 0

	for i := 0; i < len(entries); {
		j := i + 1

		for j < len(entries) && entries[j].line == entries[j-1].line+1 && entries[j].indent == entries[i].indent &&
			entries[j].column == entries[i].column {
			j++
		}

		if j-i >= 2 {
			run++

			for k := i; k < j; k++ {
				if entries[k].slash.IsValid() {
					runs[entries[k].slash] = run
				}
			}
		}

		i = j
	}

	return runs
}

// alignMarkerFor answers the marker to write after comment, or "" when it is not in a run gofmt aligned. A block
// comment is marked only when endsLine says the caller writes it at the end of its line.
func (v *Visitor) alignMarkerFor(comment *ast.Comment, endsLine bool) string {
	run, ok := v.alignedRuns[comment.Slash]

	if !ok || !endsLine && !strings.HasPrefix(comment.Text, "//") {
		return ""
	}

	return fmt.Sprintf("%s%d:%d%s", AlignMarker, run, len(carriedComment(comment.Text)), AlignMarker)
}

// alignedLine is one line of an aligned block: its index in the file's lines, and its comment.
type alignedLine struct {
	index   int
	comment string
}

// alignedFile is what the alignment pass recorded for a written file, so that a later rewrite of the file (the
// deferred marker passes, which replace type names and so change code widths) can align the same blocks again.
type alignedFile struct {
	lines  int
	blocks [][]alignedLine
}

var (
	alignedFiles     = map[string]alignedFile{}
	alignedFilesLock sync.Mutex
)

// finalizeCommentAlignment runs the alignment pass on the visitor's finished text for outputFileName and records the
// blocks it aligned (finalizePositionMap calls it, on every path that writes a file).
func (v *Visitor) finalizeCommentAlignment(outputFileName string) {
	text := v.outputBuilder.String()
	aligned, record := alignTrailingComments(text)

	alignedFilesLock.Lock()

	if len(record.blocks) > 0 {
		alignedFiles[outputFileName] = record
	} else {
		delete(alignedFiles, outputFileName)
	}

	alignedFilesLock.Unlock()

	if aligned != text {
		v.outputBuilder.Reset()
		v.outputBuilder.WriteString(aligned)
	}
}

// alignTrailingComments pads each block of consecutive lines whose trailing comments carry one run's marker, strips
// every marker, and answers the blocks it aligned.
func alignTrailingComments(text string) (string, alignedFile) {
	if !strings.Contains(text, AlignMarker) {
		return text, alignedFile{}
	}

	lines := strings.Split(text, "\n")

	type marked struct {
		index, run    int
		code, comment string
	}

	var entries []marked

	for index, line := range lines {
		if !strings.Contains(line, AlignMarker) {
			continue
		}

		body, cr := strings.CutSuffix(line, "\r")
		var rebuilt strings.Builder
		run, length, markerEnd := 0, 0, -1

		for {
			open := strings.Index(body, AlignMarker)

			if open < 0 {
				rebuilt.WriteString(body)
				break
			}

			end := strings.Index(body[open+1:], AlignMarker)

			if end < 0 {
				rebuilt.WriteString(body)
				break
			}

			end += open + 1
			rebuilt.WriteString(body[:open])
			runText, lengthText, _ := strings.Cut(body[open+1:end], ":")
			run, _ = strconv.Atoi(runText)
			length, _ = strconv.Atoi(lengthText)
			markerEnd = rebuilt.Len()
			body = body[end+1:]
		}

		stripped := rebuilt.String()

		if cr {
			lines[index] = stripped + "\r"
		} else {
			lines[index] = stripped
		}

		// Only a marker that ends its line, directly after a comment of the length it records, starts a comment
		// that can be aligned.
		if markerEnd != len(stripped) || run == 0 || length < 2 || length > markerEnd {
			continue
		}

		start := markerEnd - length
		code := strings.TrimRight(stripped[:start], " ")

		if opening := stripped[start:]; !strings.HasPrefix(opening, "//") && !strings.HasPrefix(opening, "/*") || strings.TrimSpace(code) == "" {
			continue
		}

		entries = append(entries, marked{index, run, code, stripped[start:]})
	}

	var record alignedFile

	for i := 0; i < len(entries); {
		j := i + 1

		for j < len(entries) && entries[j].run == entries[i].run && entries[j].index == entries[j-1].index+1 {
			j++
		}

		if j-i >= 2 {
			block := make([]alignedLine, 0, j-i)
			codes := make([]string, 0, j-i)
			comments := make([]string, 0, j-i)

			for _, entry := range entries[i:j] {
				block = append(block, alignedLine{entry.index, entry.comment})
				codes = append(codes, entry.code)
				comments = append(comments, entry.comment)
			}

			for k, line := range alignBlock(codes, comments) {
				lines[block[k].index] = withCR(line, lines[block[k].index])
			}

			record.blocks = append(record.blocks, block)
		}

		i = j
	}

	record.lines = len(lines)

	return strings.Join(lines, "\n"), record
}

// realignTrailingComments aligns again the blocks recorded for fileName in content, a rewrite of the file that kept
// its line count (the deferred marker passes). A file written with a banner ahead of the converted text (a `.cs.auto`
// sibling) is offset by the banner's lines.
func realignTrailingComments(fileName string, content string) string {
	alignedFilesLock.Lock()
	record, ok := alignedFiles[fileName]
	alignedFilesLock.Unlock()

	if !ok {
		return content
	}

	lines := strings.Split(content, "\n")
	offset := len(lines) - record.lines

	if offset < 0 {
		return content
	}

	for _, block := range record.blocks {
		codes := make([]string, 0, len(block))
		comments := make([]string, 0, len(block))

		for _, line := range block {
			body := strings.TrimSuffix(lines[line.index+offset], "\r")

			if !strings.HasSuffix(body, line.comment) {
				codes = nil
				break
			}

			codes = append(codes, strings.TrimRight(body[:len(body)-len(line.comment)], " "))
			comments = append(comments, line.comment)
		}

		if codes == nil {
			continue
		}

		for k, line := range alignBlock(codes, comments) {
			lines[block[k].index+offset] = withCR(line, lines[block[k].index+offset])
		}
	}

	return strings.Join(lines, "\n")
}

func withCR(line, original string) string {
	if strings.HasSuffix(original, "\r") {
		return line + "\r"
	}

	return line
}

// alignBlock writes each line of a block with its comment one column after the longest code of its section (runes).
// A block is one section, except an element list (every line's code but the last ends in `,`: C# writes no comma after
// a literal's last element), which go/printer's exprList splits where an element's size jumps against the running
// geometric mean (alignmentSections).
func alignBlock(codes, comments []string) []string {
	sections := [][]int{make([]int, len(codes))}

	for k := range codes {
		sections[0][k] = k
	}

	elementList := true

	for _, code := range codes[:len(codes)-1] {
		if !strings.HasSuffix(code, ",") {
			elementList = false
			break
		}
	}

	if elementList {
		sizes := make([]int, len(codes))

		for k, code := range codes {
			sizes[k] = elementSize(strings.TrimSuffix(strings.TrimSpace(code), ","))
		}

		sections = alignmentSections(sizes)
	}

	result := make([]string, len(codes))

	for _, section := range sections {
		longest := 0

		for _, k := range section {
			longest = max(longest, utf8.RuneCountInString(codes[k]))
		}

		for _, k := range section {
			padding := 1

			if len(section) > 1 {
				padding = longest + 1 - utf8.RuneCountInString(codes[k])
			}

			result[k] = codes[k] + strings.Repeat(" ", padding) + comments[k]
		}
	}

	return result
}

// C# spellings of a key:value element: `[key] = value`, `name: value`, `Name = value`.
var (
	indexedElementKey = regexp.MustCompile(`^(\[.*?\])\s*=[^=]`)
	namedElementKey   = regexp.MustCompile(`^([\pL_@][\pL\pN_]*)\s*(:|=[^=])`)
)

// elementSize is an element's size as go/printer's exprList measures it: the key's for a key:value element, else the
// element's, in runes.
func elementSize(element string) int {
	if m := indexedElementKey.FindStringSubmatch(element); m != nil {
		return utf8.RuneCountInString(m[1])
	}

	if m := namedElementKey.FindStringSubmatch(element); m != nil {
		return utf8.RuneCountInString(m[1])
	}

	return utf8.RuneCountInString(element)
}

// alignmentSections splits an element list where go/printer's exprList would start a new section: between two
// elements unless both sizes are at most 40, or the element's size against the geometric mean of its section's sizes
// so far is within (0.4, 2.5) (go/printer nodes.go, exprList).
func alignmentSections(sizes []int) [][]int {
	var sections [][]int
	current := []int{0}
	lnsum, count := 0.0, 0

	if len(sizes) > 0 && sizes[0] > 0 {
		lnsum, count = math.Log(float64(sizes[0])), 1
	}

	for k := 1; k < len(sizes); k++ {
		previous, size := sizes[k-1], sizes[k]
		formfeed := true

		if previous > 0 && size > 0 {
			const smallSize = 40

			if count == 0 || previous <= smallSize && size <= smallSize {
				formfeed = false
			} else {
				const r = 2.5
				ratio := float64(size) / math.Exp(lnsum/float64(count))
				formfeed = r*ratio <= 1 || r <= ratio
			}
		}

		if formfeed {
			sections = append(sections, current)
			current = nil
			lnsum, count = 0, 0
		}

		current = append(current, k)

		if size > 0 {
			lnsum += math.Log(float64(size))
			count++
		}
	}

	return append(sections, current)
}
