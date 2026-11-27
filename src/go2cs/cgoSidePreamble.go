// cgoSidePreamble.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).
//
// This file is derived from Go, whose notice it carries (license of record: LICENSES/BSD-3-Clause.txt):
//
// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//
// PROVENANCE:
// - cgoSidePreamble: DERIVED FROM src/cmd/cgo/ast.go, Go 1.26.11, func (f *File) ParseGo (the preamble read) and func
//   commentText; and src/cmd/cgo/gcc.go, Go 1.26.11, func (f *File) ProcessCgoDirectives (the #cgo lines blanked).

package main

import (
	"fmt"
	"go/ast"
	"go/parser"
	"go/token"
	"strings"
	"unicode"
)

// cgo Phase 4 (docs/phase4/DESIGN-cgo-phase4-sidebuild.md §2.2): the preamble a side translation unit compiles is the
// one cmd/cgo compiled, read the way cmd/cgo reads it.

// cgoSidePreamble reads a Go source's import "C" preamble as cmd/cgo reads it (src/cmd/cgo/ast.go, ParseGo and
// commentText; src/cmd/cgo/gcc.go, ProcessCgoDirectives; Go 1.26.11): each import "C" spec's doc comment, or its
// declaration's when the declaration holds that one spec, under a #line naming the file lineName and the comment's line,
// with every #cgo line blanked. cmd/cgo names the file by its absolute path, which cmd/go's -trimpath map then trims;
// the caller passes that trimmed name, or the base name where nothing maps it (cgoSideTrim.lineName).
func cgoSidePreamble(path, lineName string) (string, error) {
	fset := token.NewFileSet()
	file, err := parser.ParseFile(fset, path, nil, parser.ImportsOnly|parser.ParseComments)

	if err != nil {
		return "", err
	}

	var preamble strings.Builder

	for _, decl := range file.Decls {
		genDecl, ok := decl.(*ast.GenDecl)

		if !ok || genDecl.Tok != token.IMPORT {
			continue
		}

		for _, spec := range genDecl.Specs {
			importSpec, ok := spec.(*ast.ImportSpec)

			if !ok || importSpec.Path.Value != `"C"` {
				continue
			}

			group := importSpec.Doc

			if group == nil && len(genDecl.Specs) == 1 {
				group = genDecl.Doc
			}

			if group == nil {
				continue
			}

			fmt.Fprintf(&preamble, "#line %d %q\n", fset.Position(group.Pos()).Line, lineName)

			for _, comment := range group.List {
				text := comment.Text

				if text[1] == '/' {
					text = text[2:] + "\n"
				} else {
					text = text[2 : len(text)-2]
				}

				preamble.WriteString(text)
			}

			preamble.WriteString("\n")
		}
	}

	lines := strings.Split(strings.ReplaceAll(preamble.String(), "\r\n", "\n"), "\n")

	for i, line := range lines {
		if trimmed := strings.TrimSpace(line); len(trimmed) >= 5 && trimmed[:4] == "#cgo" && unicode.IsSpace(rune(trimmed[4])) {
			lines[i] = ""
		}
	}

	return strings.Join(lines, "\n"), nil
}
