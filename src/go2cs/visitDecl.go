// visitDecl.go - Gbtc
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

func (v *Visitor) visitDecl(decl ast.Decl) {
	switch declType := decl.(type) {
	case *ast.GenDecl:
		v.visitGenDecl(declType)
	case *ast.FuncDecl:
		v.visitFuncDecl(declType)
	case *ast.BadDecl:
		v.showWarning("@visitDecl - BadDecl encountered: %#v", declType)
	default:
		panic(fmt.Sprintf("@visitDecl - Unexpected Decl type: %#v", v.getPrintedNode(declType)))
	}
}
