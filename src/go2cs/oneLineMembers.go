// oneLineMembers.go - Gbtc
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

// Adjacent one-line functions (class K of docs/PLAN-comment-and-spacing-parity.md; the owner's decision 5). Go writes
// runs of short functions one per line with no blank line between them:
//
//	func (x *StarExpr) Pos() token.Pos  { return x.Star }
//	func (x *UnaryExpr) Pos() token.Pos { return x.OpPos }
//
// The converter writes every function as a body of three lines with a blank line around it. For a function in such a
// run, C# writes the member on one line instead: `sig => E;` when its body is one expression, `sig { }` when it is
// empty, and no blank line between it and the previous member of the run. A body that C# needs more lines for (a
// receiver deref, named-result locals, a frame, a hoisted temporary) keeps them and the blank lines around it, and a
// one-line function Go writes on its own is unchanged.
//
// The collapse rewrites the member's finished text, so it keeps every position sentinel the text carries: the
// statement's sentinel moves ahead of the member's body, onto the member's line, which is the Go line both were
// written for.

// oneLineRunMap maps each function of a file that Go writes on one line, directly next to another such function, to
// the one-line function directly above it (nil for the first of a run).
type oneLineRunMap = map[*ast.FuncDecl]*ast.FuncDecl

// adjacentOneLineFuncs answers the oneLineRunMap of file.
func adjacentOneLineFuncs(fset *token.FileSet, file *ast.File) oneLineRunMap {
	line := func(pos token.Pos) int { return fset.Position(pos).Line }

	oneLine := func(decl ast.Decl) *ast.FuncDecl {
		if funcDecl, ok := decl.(*ast.FuncDecl); ok && funcDecl.Body != nil && line(funcDecl.Pos()) == line(funcDecl.Body.Rbrace) {
			return funcDecl
		}

		return nil
	}

	start := func(funcDecl *ast.FuncDecl) int {
		if funcDecl.Doc != nil {
			return line(funcDecl.Doc.Pos())
		}

		return line(funcDecl.Pos())
	}

	runs := oneLineRunMap{}

	for i := 1; i < len(file.Decls); i++ {
		above, below := oneLine(file.Decls[i-1]), oneLine(file.Decls[i])

		if above == nil || below == nil || start(below) != line(above.End())+1 {
			continue
		}

		if _, ok := runs[above]; !ok {
			runs[above] = nil
		}

		runs[below] = above
	}

	return runs
}

// oneLineBodyKind answers how a one-line function's body can be written as a one-line member: "empty", "return" (one
// return with results), "statement" (one expression statement, assignment or increment, each an expression in C#),
// or "" when it cannot.
func oneLineBodyKind(funcDecl *ast.FuncDecl) string {
	switch len(funcDecl.Body.List) {
	case 0:
		return "empty"
	case 1:
	default:
		return ""
	}

	switch stmt := funcDecl.Body.List[0].(type) {
	case *ast.ReturnStmt:
		if len(stmt.Results) > 0 {
			return "return"
		}
	case *ast.ExprStmt, *ast.IncDecStmt:
		return "statement"
	case *ast.AssignStmt:
		if stmt.Tok != token.DEFINE {
			return "statement"
		}
	}

	return ""
}

// statementKeywords open a C# statement that is not an expression, which an expression body cannot hold.
var statementKeywords = []string{"var ", "ref ", "if ", "if(", "for ", "for(", "foreach", "while", "do ", "switch", "using ",
	"lock", "goto ", "return", "break", "continue", "try", "checked", "unchecked", "fixed", "unsafe", "yield", "const ", "{"}

// collapseOneLineMember rewrites the member written from memberStart to the end of the output as a one-line member when
// funcDecl is in a run of adjacent one-line functions and its emitted body allows it. separable reports whether the
// member's text opens with the converter's blank line and nothing lifted ahead of it, so the blank line can go.
func (v *Visitor) collapseOneLineMember(funcDecl *ast.FuncDecl, memberStart int, separable bool) {
	above, inRun := v.oneLineRuns[funcDecl]

	if !inRun {
		return
	}

	kind := oneLineBodyKind(funcDecl)

	if kind == "" {
		return
	}

	text := v.outputBuilder.String()

	if memberStart < 0 || memberStart > len(text) {
		return
	}

	lines := strings.Split(text[memberStart:], v.newline)

	closing := len(lines) - 1

	for closing >= 0 && strings.TrimSpace(stripPositionSentinels(lines[closing])) == "" {
		closing--
	}

	header := closing - 2

	if kind == "empty" {
		header = closing - 1
	}

	if header < 0 {
		return
	}

	closingText := stripPositionSentinels(lines[closing])
	indent := closingText[:len(closingText)-len(strings.TrimLeft(closingText, " "))]
	headerText := stripPositionSentinels(lines[header])

	if strings.TrimSpace(closingText) != "}" || !strings.HasSuffix(headerText, " {") ||
		!strings.HasPrefix(headerText, indent) || strings.HasPrefix(headerText[len(indent):], " ") {
		return
	}

	// The body's statement sentinel is written after the header's `{`, where a sentinel with nothing after it marks
	// the NEXT line. Collapsed, that line is the next member's, so the sentinel moves ahead of the member's body and
	// marks the member's own line, the Go line it was written for.
	brace := strings.LastIndex(lines[header], " {")
	signature, sentinels := lines[header][:brace], lines[header][brace+len(" {"):]

	if stripPositionSentinels(sentinels) != "" {
		return
	}

	member := signature + " " + sentinels + "{ }"

	if kind != "empty" {
		bodySentinels, statement := splitLeadingSentinels(lines[closing-1])

		if kind == "return" {
			if !strings.HasPrefix(statement, "return ") {
				return
			}

			statement = statement[len("return "):]
		} else {
			for _, keyword := range statementKeywords {
				if strings.HasPrefix(statement, keyword) {
					return
				}
			}
		}

		if !strings.Contains(statement, ";") {
			return
		}

		member = signature + " => " + sentinels + bodySentinels + statement
	}

	collapsed := append(append(lines[:header:header], member), lines[closing+1:]...)
	result := strings.Join(collapsed, v.newline)

	// Adjacent in Go, adjacent in C#: the blank line the converter writes ahead of every member goes when the member
	// directly above in Go was collapsed and nothing was written between the two.
	if above != nil && above == v.lastCollapsedFunc && v.lastCollapsedEnd == memberStart && separable {
		result = strings.TrimPrefix(result, v.newline)
	}

	v.outputBuilder.Reset()
	v.outputBuilder.WriteString(text[:memberStart])
	v.outputBuilder.WriteString(result)

	v.lastCollapsedFunc = funcDecl
	v.lastCollapsedEnd = v.outputBuilder.Len()
}

// splitLeadingSentinels separates the position sentinels at the start of an emitted statement line from the
// statement, dropping the line's indentation.
func splitLeadingSentinels(line string) (sentinels, statement string) {
	rest := strings.TrimLeft(line, " ")

	for strings.HasPrefix(rest, PositionSentinel) {
		end := strings.Index(rest[1:], PositionSentinel)

		if end < 0 {
			break
		}

		sentinels += rest[:end+2]
		rest = strings.TrimLeft(rest[end+2:], " ")
	}

	return sentinels, rest
}
