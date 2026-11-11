// collectionExpressionTargets.go - Gbtc
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
	"go/types"
)

// markTargetedSliceLiterals marks the slice literals that render as a bare C# collection expression, `[1, 2, 3]`,
// instead of golib's builtin over one, `slice<nint>([1, 2, 3])` (docs/PLAN-collection-expressions.md, ruled
// 2026-11-05, option 4.2 (a)). C# 14 gives a collection expression no natural type, so the bare form compiles only
// where C# already knows the target is the literal's own slice<T>: slice<T>'s [CollectionBuilder] then builds it.
//
// A literal is marked when its position gives C# that target and the target's Go type is IDENTICAL to the literal's
// (an `any` slot or a named slice type takes no collection expression):
//
//   - the right side of a single `x = []T{…}`;
//   - a package-level var's initializer, which is a typed C# field;
//   - the only result of a `return` in a function declaration with one result;
//   - an element, field value or map value of another composite literal;
//   - an argument to a non-generic function's non-variadic parameter, outside `defer` and `go`, whose arguments
//     are evaluated into locals.
//
// Every other position keeps the builtin: `x := []T{…}` (Go names the type on the right, and `var` has no target), a
// range operand, a conversion, a builtin's argument, an interface or type-parameter argument, and a func literal's
// return, whose lambda may take its type from its body.
//
// A literal whose element holds a call keeps the builtin too: a collection expression's builder takes a span, and C#
// fills a span target in place, where builtin.slice<T>(T[]) receives an array, so only the array keeps the call's
// own Go line in an optimized build (seat #54, see sliceLiteralTakesBuiltin).
//
// The marks are read from the file's own syntax before anything converts, so a literal that a later step moves
// (a mixed literal normalized to keyed elements) keeps the position Go gave it. The emission still asks
// sliceLiteralTakesBuiltin and its own conditions; a mark only chooses between the two spellings of the builtin form.
func (v *Visitor) markTargetedSliceLiterals(file *ast.File) {
	var stack []ast.Node

	ast.Inspect(file, func(n ast.Node) bool {
		if n == nil {
			stack = stack[:len(stack)-1]
			return true
		}

		if lit, ok := n.(*ast.CompositeLit); ok && v.sliceLiteralIsTargeted(lit, stack) {
			if v.targetedSliceLiterals == nil {
				v.targetedSliceLiterals = map[*ast.CompositeLit]bool{}
			}

			v.targetedSliceLiterals[lit] = true
		}

		stack = append(stack, n)
		return true
	})
}

func (v *Visitor) sliceLiteralIsTargeted(lit *ast.CompositeLit, stack []ast.Node) bool {
	litType := v.info.TypeOf(lit)

	if litType == nil || !sliceLiteralTakesBuiltin(litType) || compositeLitIsKeyed(lit.Elts) {
		return false
	}

	for _, elt := range lit.Elts {
		if v.exprHoldsCall(elt) {
			return false
		}
	}

	// The literal itself, or the parenthesized expression that stands for it in its parent.
	var child ast.Node = lit
	i := len(stack) - 1

	for i >= 0 {
		if _, isParen := stack[i].(*ast.ParenExpr); !isParen {
			break
		}

		child = stack[i]
		i--
	}

	if i < 0 {
		return false
	}

	targets := func(t types.Type) bool {
		return t != nil && types.Identical(t, litType)
	}

	switch parent := stack[i].(type) {
	case *ast.AssignStmt:
		if parent.Tok != token.ASSIGN || len(parent.Lhs) != 1 || len(parent.Rhs) != 1 || parent.Rhs[0] != child {
			return false
		}

		if ident, ok := parent.Lhs[0].(*ast.Ident); ok && ident.Name == "_" {
			return false
		}

		return targets(v.info.TypeOf(parent.Lhs[0]))
	case *ast.ValueSpec:
		for _, ancestor := range stack[:i] {
			switch ancestor.(type) {
			case *ast.FuncDecl, *ast.FuncLit:
				return false
			}
		}

		for j, value := range parent.Values {
			if value == child && j < len(parent.Names) && parent.Names[j].Name != "_" {
				return targets(v.info.TypeOf(parent.Names[j]))
			}
		}

		return false
	case *ast.ReturnStmt:
		if len(parent.Results) != 1 || parent.Results[0] != child {
			return false
		}

		for j := i - 1; j >= 0; j-- {
			switch fn := stack[j].(type) {
			case *ast.FuncLit:
				return false
			case *ast.FuncDecl:
				obj, ok := v.info.Defs[fn.Name].(*types.Func)

				if !ok {
					return false
				}

				results := obj.Type().(*types.Signature).Results()

				return results.Len() == 1 && targets(results.At(0).Type())
			}
		}

		return false
	case *ast.CompositeLit:
		return targets(compositeElementType(v.info.TypeOf(parent), parent, child, nil))
	case *ast.KeyValueExpr:
		if parent.Value != child || i < 1 {
			return false
		}

		if outer, ok := stack[i-1].(*ast.CompositeLit); ok {
			return targets(compositeElementType(v.info.TypeOf(outer), outer, child, parent))
		}

		return false
	case *ast.CallExpr:
		return v.callArgumentIsTargeted(parent, child, stack[:i], targets)
	}

	return false
}

func (v *Visitor) callArgumentIsTargeted(call *ast.CallExpr, arg ast.Node, ancestors []ast.Node, targets func(types.Type) bool) bool {
	if call.Fun == arg || call.Ellipsis.IsValid() {
		return false
	}

	// A deferred or `go` call's arguments are evaluated into locals before the call.
	if len(ancestors) > 0 {
		switch ancestors[len(ancestors)-1].(type) {
		case *ast.DeferStmt, *ast.GoStmt:
			return false
		}
	}

	if tv, ok := v.info.Types[call.Fun]; ok && tv.IsType() {
		return false
	}

	fun := ast.Unparen(call.Fun)

	switch f := fun.(type) {
	case *ast.Ident:
		if _, isBuiltin := v.info.Uses[f].(*types.Builtin); isBuiltin {
			return false
		}
	case *ast.SelectorExpr:
		// unsafe.SliceData and its siblings: go/types records each builtin call's own signature, which would
		// read as an ordinary parameter, and golib's counterparts are generic.
		if _, isBuiltin := v.info.Uses[f.Sel].(*types.Builtin); isBuiltin {
			return false
		}
	case *ast.IndexExpr, *ast.IndexListExpr:
		// An explicitly instantiated generic function.
		return false
	}

	sig, ok := v.info.TypeOf(call.Fun).(*types.Signature)

	if !ok || sigIsGeneric(v.info, fun) {
		return false
	}

	index := -1

	for j, a := range call.Args {
		if a == arg {
			index = j
		}
	}

	params := sig.Params()
	fixed := params.Len()

	if sig.Variadic() {
		fixed--
	}

	if index < 0 || index >= fixed {
		return false
	}

	return targets(params.At(index).Type())
}

// sigIsGeneric reports whether a call's callee is a generic function or a method of a generic type: C# would infer its
// type arguments, and an untyped constant element infers System.Int32 where Go's int is nint.
func sigIsGeneric(info *types.Info, fun ast.Expr) bool {
	var obj types.Object

	switch f := fun.(type) {
	case *ast.Ident:
		obj = info.Uses[f]
	case *ast.SelectorExpr:
		obj = info.Uses[f.Sel]
	}

	fn, ok := obj.(*types.Func)

	if !ok {
		return false
	}

	sig := fn.Origin().Type().(*types.Signature)

	if sig.TypeParams().Len() > 0 {
		return true
	}

	if recv := sig.Recv(); recv != nil {
		t := recv.Type()

		if ptr, ok := types.Unalias(t).(*types.Pointer); ok {
			t = ptr.Elem()
		}

		if named, ok := types.Unalias(t).(*types.Named); ok && named.TypeParams().Len() > 0 {
			return true
		}
	}

	return false
}

// exprHoldsCall reports whether evaluating expr calls a function: any call that is not a conversion, outside a func
// literal's body, which the literal does not run.
func (v *Visitor) exprHoldsCall(expr ast.Expr) bool {
	found := false

	ast.Inspect(expr, func(n ast.Node) bool {
		if found {
			return false
		}

		switch node := n.(type) {
		case *ast.FuncLit:
			return false
		case *ast.CallExpr:
			if tv, ok := v.info.Types[node.Fun]; ok && tv.IsType() {
				return true
			}

			found = true
			return false
		}

		return true
	})

	return found
}
