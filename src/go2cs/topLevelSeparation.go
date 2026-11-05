// topLevelSeparation.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Class K of docs/PLAN-comment-and-spacing-parity.md, as the owner ruled it on 2026-11-05
// (docs/phase4/CENSUS-comment-parity-K-scout.md), in every conversion:
//
//   - K2 (a): a function Go writes on one line is written on one line, as a block body: Go's
//     `func (x *ParenExpr) Pos() token.Pos { return x.Lparen }` is
//     `public static tokenꓸPos Pos(this ref ParenExpr x) { return x.Lparen; }`, and an empty body is `{ }`.
//     One whose C# body needs more than one statement (a receiver deref, a heap box, two statements on
//     Go's one line, a deferred call's frame) stays as the converter writes it.
//   - K1 (b): two top-level declarations are separated by a blank line only where Go has one, except
//     that a function Go writes on one line and C# cannot keeps a blank line on each side, so a
//     several-line member does not sit directly against its neighbours.

package main

import (
	"go/ast"
	"go/token"
	"strings"
)

// declShape is what a top-level declaration's emission looks like, for the separator after it.
type declShape int

const (
	declShapeOther        declShape = iota // any declaration K does not single out
	declShapeOneLine                       // a function Go writes on one line, written on one line
	declShapeOneLineSplit                  // a function Go writes on one line that C# writes on several
)

// goOneLineFunc reports whether Go writes the function, from `func` to its closing brace, on one line.
func (v *Visitor) goOneLineFunc(funcDecl *ast.FuncDecl) bool {
	return funcDecl.Body != nil && v.fset.Position(funcDecl.Pos()).Line == v.fset.Position(funcDecl.End()).Line
}

// collapseOneLineFunc writes the function emitted from start on one line when Go writes it on one line
// and its C# body is at most one statement, and returns its shape for the separator.
func (v *Visitor) collapseOneLineFunc(funcDecl *ast.FuncDecl, start int) declShape {
	if !v.goOneLineFunc(funcDecl) {
		return declShapeOther
	}

	output := v.outputBuilder.String()

	if start < 0 || start > len(output) {
		return declShapeOneLineSplit
	}

	collapsed, ok := collapseOneLineBody(output[start:], v.newline, v.indent(1))

	if !ok {
		return declShapeOneLineSplit
	}

	v.outputBuilder.Reset()
	v.outputBuilder.WriteString(output[:start])
	v.outputBuilder.WriteString(collapsed)

	return declShapeOneLine
}

// collapseOneLineBody rewrites a function's emitted text whose last lines are its header, at most one
// statement indented one level, and its closing brace at column 0, so that header, statement and brace
// share the header's line. A line holding only position sentinels or emission markers is not a
// statement: what it holds moves onto the one line, where the position map still finds it. It returns
// false when the body has more than one statement, when the statement carries a `//` comment (which
// would swallow the brace), or when the text is not that shape.
func collapseOneLineBody(text, newline, bodyIndent string) (string, bool) {
	trailing := ""

	if strings.HasSuffix(text, newline) {
		trailing = newline
		text = strings.TrimSuffix(text, newline)
	}

	lines := strings.Split(text, newline)
	content := func(line string) string {
		return strings.TrimSpace(stripEmissionMarkers(stripPositionSentinels(line)))
	}

	closing := len(lines) - 1

	if closing < 1 || !strings.HasPrefix(lines[closing], "}") {
		return "", false
	}

	if rest := strings.TrimSpace(lines[closing][1:]); rest != "" && !strings.HasPrefix(rest, "//") {
		return "", false
	}

	var carried []string // sentinels and markers on lines with no statement
	statement, comment := "", ""
	header := -1

	for i := closing - 1; i >= 0; i-- {
		line := lines[i]
		text := content(line)

		if text == "" {
			if raw := strings.TrimSpace(line); raw != "" {
				carried = append([]string{raw}, carried...)
			}

			continue
		}

		if strings.HasSuffix(text, "{") && (statement != "" || !strings.HasPrefix(line, bodyIndent)) {
			header = i
			break
		}

		if statement != "" || !strings.HasPrefix(line, bodyIndent) || strings.HasPrefix(line, bodyIndent+" ") ||
			strings.HasSuffix(text, "{") || strings.HasPrefix(text, "}") {
			return "", false
		}

		statement = strings.TrimSpace(line)
		code, lineComment, ok := splitLineComment(statement)

		if !ok || (lineComment != "" && strings.TrimSpace(lines[closing][1:]) != "") {
			return "", false
		}

		statement, comment = code, lineComment
	}

	if header < 0 {
		return "", false
	}

	one := strings.TrimRight(lines[header], " \t")

	if body := strings.TrimSpace(strings.Join(append(carried, statement), "")); body != "" {
		one += " " + body
	}

	one += " " + strings.TrimSpace(lines[closing])

	if comment != "" {
		one += " " + comment
	}

	return strings.Join(append(lines[:header:header], one), newline) + trailing, true
}

// splitLineComment splits a C# statement line into its code and a trailing `//` comment, which moves after
// the closing brace when the function is put on one line. It reports false for a line it cannot scan with
// certainty: a raw or interpolated string literal, or an unterminated literal.
func splitLineComment(line string) (code, comment string, ok bool) {
	for i := 0; i < len(line); i++ {
		switch c := line[i]; {
		case c == '/' && i+1 < len(line) && line[i+1] == '/':
			return strings.TrimRight(line[:i], " \t"), line[i:], true
		case c == '/' && i+1 < len(line) && line[i+1] == '*':
			end := strings.Index(line[i+2:], "*/")

			if end < 0 {
				return "", "", false
			}

			i += end + 3
		case c == '$' || strings.HasPrefix(line[i:], `"""`):
			return "", "", false
		case c == '@' && i+1 < len(line) && line[i+1] == '"':
			i += 2

			for ; i < len(line); i++ {
				if line[i] == '"' {
					if i+1 < len(line) && line[i+1] == '"' {
						i++
						continue
					}

					break
				}
			}

			if i >= len(line) {
				return "", "", false
			}
		case c == '"' || c == '\'':
			quote := c
			i++

			for ; i < len(line) && line[i] != quote; i++ {
				if line[i] == '\\' {
					i++
				}
			}

			if i >= len(line) {
				return "", "", false
			}
		}
	}

	return line, "", true
}

// declStart is where a top-level declaration begins in the Go source: its doc comment, if it has one.
func declStart(decl ast.Decl) token.Pos {
	switch d := decl.(type) {
	case *ast.FuncDecl:
		if d.Doc != nil {
			return d.Doc.Pos()
		}
	case *ast.GenDecl:
		if d.Doc != nil {
			return d.Doc.Pos()
		}
	}

	return decl.Pos()
}

// separateTopLevelDecl decides the separator ahead of decl, emitted from mark, after prev with prevShape:
// the blank line every declaration's emission opens with is removed when Go writes decl directly under
// prev and neither is a one-line Go function C# writes on several lines (K1 (b)).
func (v *Visitor) separateTopLevelDecl(prev ast.Decl, prevShape declShape, decl ast.Decl, shape declShape, mark int) {
	if prev == nil || prevShape == declShapeOneLineSplit || shape == declShapeOneLineSplit {
		return
	}

	if v.file == nil {
		v.file = v.fset.File(decl.Pos())
	}

	if v.goBlankLineBetween(prev.End(), declStart(decl)) {
		return
	}

	v.joinAt(mark)
}

// joinAt reduces the line breaks around mark, between the previous declaration's last line and the next
// one's first, to a single line break, so no blank line separates them.
func (v *Visitor) joinAt(mark int) {
	output := v.outputBuilder.String()

	if mark <= 0 || mark > len(output) {
		return
	}

	before, after := output[:mark], output[mark:]
	trimmedBefore := strings.TrimRight(before, "\r\n")
	trimmedAfter := strings.TrimLeft(after, "\r\n")
	breaks := strings.Count(before[len(trimmedBefore):], "\n") + strings.Count(after[:len(after)-len(trimmedAfter)], "\n")

	if breaks < 2 || trimmedBefore == "" || trimmedAfter == "" {
		return
	}

	v.outputBuilder.Reset()
	v.outputBuilder.WriteString(trimmedBefore)
	v.outputBuilder.WriteString(v.newline)
	v.outputBuilder.WriteString(trimmedAfter)
}

// emittedSince reports whether anything but whitespace, position sentinels and emission markers was
// written since mark.
func (v *Visitor) emittedSince(mark int) bool {
	output := v.outputBuilder.String()

	if mark < 0 || mark > len(output) {
		return false
	}

	return strings.TrimSpace(stripEmissionMarkers(stripPositionSentinels(output[mark:]))) != ""
}
