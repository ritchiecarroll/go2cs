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

	for _, spec := range genDecl.Specs {
		switch specType := spec.(type) {
		case *ast.ImportSpec:
			v.visitImportSpec(specType, genDecl.Doc)
		case *ast.ValueSpec:
			v.visitValueSpec(specType, genDecl.Doc, genDecl.Tok)
			hasValueSpec = true
		case *ast.TypeSpec:
			v.visitTypeSpec(specType, genDecl.Doc)
		default:
			panic(fmt.Sprintf("@visitGenDecl - unexpected GenDecl Spec: %#v", v.getPrintedNode(specType)))
		}
	}

	if !v.inFunction && hasValueSpec {
		v.outputBuilder.WriteString(v.newline)
	}
}
