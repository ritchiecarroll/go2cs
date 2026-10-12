// golex.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package commentcensus

import (
	"go/ast"
	"go/parser"
	"go/scanner"
	"go/token"
	"os"
	"strings"
)

type goGroup struct {
	class    string   // see classify
	declKind string   // for decl-doc: func, method, const, var, type-struct, type-interface, type-alias, type-other
	startLn  int      // first line of the group
	endLn    int      // last line
	lines    []string // normalized text lines (empty lines dropped)
	lineNos  []int    // source line of each text line
	trailing bool     // shares its first line with code before it
	cols     []int    // visual column (tab 8) of each text line's comment start, for // comments
}

type goFile struct {
	path     string
	src      []string // lines
	fset     *token.FileSet
	file     *ast.File
	groups   []*goGroup
	tokLine  map[int][]token.Token // non-comment tokens per line
	idents   map[int][]string      // identifiers per line
	firstTok map[int]token.Token   // first non-comment token on the line
	blank    map[int]bool
}

func parseGo(path string) (*goFile, error) {
	data, err := os.ReadFile(path)
	if err != nil {
		return nil, err
	}
	fset := token.NewFileSet()
	f, err := parser.ParseFile(fset, path, data, parser.ParseComments|parser.SkipObjectResolution)
	if err != nil {
		return nil, err
	}
	g := &goFile{path: path, fset: fset, file: f, tokLine: map[int][]token.Token{}, idents: map[int][]string{}, firstTok: map[int]token.Token{}, blank: map[int]bool{}}
	g.src = strings.Split(strings.ReplaceAll(string(data), "\r\n", "\n"), "\n")
	for i, l := range g.src {
		if strings.TrimSpace(l) == "" {
			g.blank[i+1] = true
		}
	}
	// token scan
	var s scanner.Scanner
	tf := token.NewFileSet().AddFile(path, -1, len(data))
	s.Init(tf, data, nil, 0)
	for {
		pos, tok, lit := s.Scan()
		if tok == token.EOF {
			break
		}
		if tok == token.SEMICOLON && lit == "\n" {
			continue
		}
		ln := tf.Line(pos)
		if _, ok := g.firstTok[ln]; !ok {
			g.firstTok[ln] = tok
		}
		g.tokLine[ln] = append(g.tokLine[ln], tok)
		if tok == token.IDENT {
			g.idents[ln] = append(g.idents[ln], lit)
		}
	}
	g.classify()
	return g, nil
}

func isDirective(text string) bool {
	return strings.HasPrefix(text, "//go:") || strings.HasPrefix(text, "//line ") || strings.HasPrefix(text, "//export ") || strings.HasPrefix(text, "//extern ") || strings.HasPrefix(text, "//sys") || strings.HasPrefix(text, "// +build")
}

func (g *goFile) classify() {
	f := g.file
	kind := map[*ast.CommentGroup]string{}
	dk := map[*ast.CommentGroup]string{}
	if f.Doc != nil {
		kind[f.Doc] = "package-doc"
	}
	ast.Inspect(f, func(n ast.Node) bool {
		switch x := n.(type) {
		case *ast.FuncDecl:
			if x.Doc != nil {
				kind[x.Doc] = "decl-doc"
				if x.Recv != nil {
					dk[x.Doc] = "method"
				} else {
					dk[x.Doc] = "func"
				}
			}
		case *ast.GenDecl:
			if x.Doc != nil {
				kind[x.Doc] = "decl-doc"
				dk[x.Doc] = genKind(x)
			}
			grouped := x.Lparen.IsValid()
			for _, sp := range x.Specs {
				var doc, com *ast.CommentGroup
				switch s := sp.(type) {
				case *ast.ValueSpec:
					doc, com = s.Doc, s.Comment
				case *ast.TypeSpec:
					doc, com = s.Doc, s.Comment
				case *ast.ImportSpec:
					doc, com = s.Doc, s.Comment
				}
				if doc != nil {
					if grouped {
						kind[doc] = "spec-doc-in-group"
					} else {
						kind[doc] = "decl-doc"
					}
					dk[doc] = specKind(x.Tok, sp)
				}
				if com != nil {
					kind[com] = "trailing-spec"
				}
			}
		case *ast.Field:
			if x.Doc != nil {
				kind[x.Doc] = "field-doc"
			}
			if x.Comment != nil {
				kind[x.Comment] = "trailing-field"
			}
		}
		return true
	})
	// enclosing-node lookup for free comments
	type span struct {
		pos, end token.Pos
		n        ast.Node
	}
	var spans []span
	ast.Inspect(f, func(n ast.Node) bool {
		if n == nil {
			return false
		}
		switch n.(type) {
		case *ast.CompositeLit, *ast.CallExpr, *ast.BinaryExpr, *ast.FuncType, *ast.IndexExpr, *ast.KeyValueExpr, *ast.ParenExpr, *ast.FuncLit, *ast.BlockStmt, *ast.StructType, *ast.InterfaceType:
			spans = append(spans, span{n.Pos(), n.End(), n})
		}
		return true
	})
	for _, cg := range f.Comments {
		gg := &goGroup{}
		startLn := g.fset.Position(cg.Pos()).Line
		gg.startLn = startLn
		gg.endLn = g.fset.Position(cg.End()).Line
		allDirective := true
		for _, c := range cg.List {
			ln := g.fset.Position(c.Pos()).Line
			col := g.fset.Position(c.Pos()).Column
			if !isDirective(c.Text) {
				allDirective = false
			}
			if strings.HasPrefix(c.Text, "//") {
				t := normText(c.Text[2:])
				t = strings.TrimPrefix(t, "/")
				t = strings.TrimSpace(t)
				if t != "" {
					gg.lines = append(gg.lines, t)
					gg.lineNos = append(gg.lineNos, ln)
					gg.cols = append(gg.cols, visualCol(g.src[ln-1], col-1, 8))
				}
			} else {
				body := strings.TrimSuffix(strings.TrimPrefix(c.Text, "/*"), "*/")
				for k, bl := range strings.Split(body, "\n") {
					t := normText(bl)
					t = strings.TrimPrefix(t, "* ")
					if t == "*" {
						t = ""
					}
					if t != "" {
						gg.lines = append(gg.lines, t)
						gg.lineNos = append(gg.lineNos, ln+k)
						gg.cols = append(gg.cols, -1)
					}
				}
			}
		}
		// trailing: a code token earlier on the group's first line
		toks := g.tokLine[startLn]
		if len(toks) > 0 {
			// tokens on that line that precede the comment: compare columns via source text
			line := g.src[startLn-1]
			cpos := g.fset.Position(cg.Pos()).Column - 1
			if strings.TrimSpace(line[:cpos]) != "" {
				gg.trailing = true
			}
		}
		if allDirective {
			gg.class = "directive"
		} else if k, ok := kind[cg]; ok {
			gg.class = k
			gg.declKind = dk[cg]
		} else if cg.End() < f.Package {
			gg.class = "file-header"
		} else {
			// innermost enclosing span
			var inner ast.Node
			var innerSize token.Pos = 1 << 60
			for _, s := range spans {
				if s.pos < cg.Pos() && cg.End() <= s.end && s.end-s.pos < innerSize {
					inner, innerSize = s.n, s.end-s.pos
				}
			}
			nextTok := token.ILLEGAL
			for ln := gg.endLn + 1; ln <= len(g.src); ln++ {
				if t, ok := g.firstTok[ln]; ok {
					nextTok = t
					break
				}
			}
			switch inner.(type) {
			case *ast.CompositeLit:
				gg.class = "in-composite-literal"
			case *ast.CallExpr, *ast.BinaryExpr, *ast.FuncType, *ast.IndexExpr, *ast.KeyValueExpr, *ast.ParenExpr:
				gg.class = "in-expression"
			case *ast.StructType, *ast.InterfaceType:
				gg.class = "free-in-type"
			default:
				switch {
				case gg.trailing:
					before := strings.TrimSpace(g.src[startLn-1][:g.fset.Position(cg.Pos()).Column-1])
					switch {
					case strings.HasSuffix(before, "{"):
						gg.class = "after-open-brace"
					case strings.HasSuffix(before, ":") && (g.firstTok[startLn] == token.CASE || g.firstTok[startLn] == token.DEFAULT):
						gg.class = "after-case-label"
					default:
						gg.class = "trailing-stmt"
					}
				case nextTok == token.ELSE:
					gg.class = "before-else"
				case nextTok == token.CASE || nextTok == token.DEFAULT:
					gg.class = "before-case"
				case nextTok == token.RBRACE:
					gg.class = "before-close-brace"
				case inner == nil:
					gg.class = "free-top-level"
				default:
					gg.class = "free-stmt"
				}
			}
		}
		g.groups = append(g.groups, gg)
	}
}

func genKind(x *ast.GenDecl) string {
	if len(x.Specs) == 0 {
		return x.Tok.String()
	}
	return specKind(x.Tok, x.Specs[0])
}

func specKind(tok token.Token, sp ast.Spec) string {
	if ts, ok := sp.(*ast.TypeSpec); ok {
		switch {
		case ts.Assign.IsValid():
			return "type-alias"
		}
		switch ts.Type.(type) {
		case *ast.StructType:
			return "type-struct"
		case *ast.InterfaceType:
			return "type-interface"
		}
		return "type-other"
	}
	return tok.String()
}
