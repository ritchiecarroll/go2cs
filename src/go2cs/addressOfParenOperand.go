// addressOfParenOperand.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/ast"
	"go/parser"
	"go/token"
)

// parseFileUnparenAddressOperands is the loader's ParseFile for every load whose syntax is converted. It parses
// exactly as the loader's default does, then removes the parentheses around the operand of every address-of
// operator: Go's `&(x)` is `&x`, `&((*T)(p).f)` is `&(*T)(p).f`.
//
// The converter's address-of rules match the operand's SHAPE (an identifier, a selector, an index, a field
// reached through a pointer conversion), and so does every analysis that decides a variable is
// address-taken and boxes it. A parenthesized operand matched none of them, fell to the last form, `Ꮡ(x)`,
// which boxes a COPY, and every write through the address was lost: libc's pthread mutex owner,
// `atomic.CompareAndSwapInt32(&((*pthreadMutex)(unsafe.Pointer(m)).owner), 0, tls.ID)`, was never stored, and
// sqlite's recursive init mutex deadlocked on re-entry. Removing the parentheses where the file is parsed,
// before go/types runs, means every rule and every analysis sees the one form, with no rule to remember.
func parseFileUnparenAddressOperands(fset *token.FileSet, filename string, src []byte) (*ast.File, error) {
	// The loader's default mode (golang.org/x/tools/go/packages, loader.parseFile).
	const mode = parser.AllErrors | parser.ParseComments

	file, err := parser.ParseFile(fset, filename, src, mode)

	if file != nil {
		unparenAddressOperands(file)
	}

	return file, err
}

// unparenAddressOperands replaces the operand of each `&` in node with the operand inside its parentheses, and
// does the same down the operand's path to its variable: `&((pt).X)` is `&pt.X`, `&((arr)[1])` is `&arr[1]`. On
// the path, the parentheses go only where the parser writes the same base bare (an identifier, a selector, an
// index); `(*p).f` and `((*T)(p)).f` keep theirs, since the bare form would read as another expression.
func unparenAddressOperands(node ast.Node) {
	ast.Inspect(node, func(n ast.Node) bool {
		if unary, ok := n.(*ast.UnaryExpr); ok && unary.Op == token.AND {
			unary.X = ast.Unparen(unary.X)

			for operand := unary.X; operand != nil; {
				switch path := operand.(type) {
				case *ast.SelectorExpr:
					path.X = unparenPathBase(path.X)
					operand = path.X
				case *ast.IndexExpr:
					path.X = unparenPathBase(path.X)
					operand = path.X
				case *ast.IndexListExpr:
					path.X = unparenPathBase(path.X)
					operand = path.X
				default:
					operand = nil
				}
			}
		}

		return true
	})
}

// unparenPathBase removes the parentheses around a selector's or an index's base where its bare form is the same
// expression: an identifier, a selector or an index.
func unparenPathBase(base ast.Expr) ast.Expr {
	switch ast.Unparen(base).(type) {
	case *ast.Ident, *ast.SelectorExpr, *ast.IndexExpr, *ast.IndexListExpr:
		return ast.Unparen(base)
	}

	return base
}
