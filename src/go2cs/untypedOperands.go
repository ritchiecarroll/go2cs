// untypedOperands.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/ast"
	"go/types"

	"golang.org/x/tools/go/packages"
)

// recordUntypedOperands closes the untyped regions of a package that did not fully type-check, before
// anything reads its types. Such a package converts best-effort (issue #33), but go/types records NO
// type for an operand that went invalid (Checker.record returns early for `mode == invalid`), so
// types.Info.TypeOf returns nil there -- and the converter reads TypeOf results at hundreds of sites
// and calls methods on them. Guarding those one fault at a time is how issue #33 began, and the shapes
// in untypedRegionShapes_test.go outran it.
//
// So the invariant is established once, here: every expression go/types records when it is valid gets,
// when it is not, the type go/types itself gives an invalid object -- types.Typ[types.Invalid], a
// *types.Basic whose Underlying is itself. Every type predicate then answers "not that kind", the
// answer an unrecognized type already gets, and the caller's fallback path runs. Only a package that
// loaded with errors is touched, so a clean package's emission cannot change. Returns how many
// expressions were recorded.
func recordUntypedOperands(pkg *packages.Package) int {
	info := pkg.TypesInfo

	if info == nil || info.Types == nil {
		return 0
	}

	recorded := 0

	for _, file := range pkg.Syntax {
		ast.Inspect(file, func(node ast.Node) bool {
			switch node := node.(type) {
			case *ast.ImportSpec:
				// An import path is a string literal go/types never records.
				return false
			case *ast.Field:
				// Nor a struct tag: walk the field's names and type, not its tag. (Its names are
				// definitions, which recordedWhenValid already declines.)
				for _, name := range node.Names {
					recorded += recordUntypedOperand(name, info)
				}

				if node.Type != nil {
					ast.Inspect(node.Type, func(inner ast.Node) bool {
						recorded += recordUntypedOperand(inner, info)
						return true
					})
				}

				return false
			}

			recorded += recordUntypedOperand(node, info)
			return true
		})
	}

	return recorded
}

// recordUntypedOperand records types.Typ[types.Invalid] for node when it is an expression go/types
// would have recorded had it been valid, and has no type.
func recordUntypedOperand(node ast.Node, info *types.Info) int {
	expr, ok := node.(ast.Expr)

	if !ok || !recordedWhenValid(expr, info) {
		return 0
	}

	if tv, ok := info.Types[expr]; ok && tv.Type != nil {
		return 0
	}

	info.Types[expr] = types.TypeAndValue{Type: types.Typ[types.Invalid]}

	return 1
}

// recordedWhenValid reports whether go/types records expr in Info.Types when it type-checks. An
// identifier is the exception that needs care: one that declares (Defs, including the package clause
// and a type switch's symbolic variable, both mapped to nil) or resolves (Uses) is answered through its
// object, so only an identifier in neither -- an undefined name -- is untyped.
func recordedWhenValid(expr ast.Expr, info *types.Info) bool {
	switch expr := expr.(type) {
	case *ast.Ident:
		if expr.Name == "_" {
			return false
		}

		_, defined := info.Defs[expr]
		_, used := info.Uses[expr]

		return !defined && !used
	case *ast.TypeAssertExpr:
		// A type switch guard's `x.(type)` is never recorded.
		return expr.Type != nil
	case *ast.BinaryExpr, *ast.UnaryExpr, *ast.CallExpr, *ast.IndexExpr, *ast.IndexListExpr, *ast.SliceExpr,
		*ast.StarExpr, *ast.ParenExpr, *ast.SelectorExpr, *ast.BasicLit, *ast.CompositeLit, *ast.FuncLit:
		return true
	}

	return false
}
