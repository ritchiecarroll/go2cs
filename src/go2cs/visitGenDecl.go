// visitGenDecl.go - Gbtc
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
)

func (v *Visitor) visitGenDecl(genDecl *ast.GenDecl) {
	var hasValueSpec bool

	for index, spec := range genDecl.Specs {
		switch specType := spec.(type) {
		case *ast.ImportSpec:
			v.visitImportSpec(specType, genDecl.Doc)
		case *ast.ValueSpec:
			v.visitValueSpec(specType, specDoc(genDecl, index, specType.Doc), genDecl.Tok)
			hasValueSpec = true
		case *ast.TypeSpec:
			v.visitTypeSpec(specType, specDoc(genDecl, index, specType.Doc))
		default:
			panic(fmt.Sprintf("@visitGenDecl - unexpected GenDecl Spec: %#v", v.getPrintedNode(specType)))
		}
	}

	if !v.inFunction && hasValueSpec {
		v.outputBuilder.WriteString(v.newline)
	}
}

// specDoc answers the doc comment the spec at index of genDecl is emitted with. go/parser attaches a
// doc to the GenDecl when it precedes the declaration, and to the SPEC when it precedes a spec inside
// a grouped `( … )`, so an ungrouped declaration's doc is the GenDecl's and a grouped spec's doc is
// its own. A grouped declaration's doc documents the group: it is written once, above the first spec,
// together with that spec's own doc when it has one.
func specDoc(genDecl *ast.GenDecl, index int, doc *ast.CommentGroup) *ast.CommentGroup {
	if !genDecl.Lparen.IsValid() {
		return genDecl.Doc
	}

	if index > 0 || genDecl.Doc == nil {
		return doc
	}

	if doc == nil {
		return genDecl.Doc
	}

	return &ast.CommentGroup{List: append(append([]*ast.Comment{}, genDecl.Doc.List...), doc.List...)}
}
