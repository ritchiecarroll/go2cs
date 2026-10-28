// runeLiteralForms.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/ast"
	"go/constant"
	"go/token"
	"go/types"
)

// runeLiteralForm is how a Go rune literal renders. Go's `'x'` is an untyped rune constant that takes
// its type from where it is used; C#'s `'x'` is a char, which converts implicitly to int, nint, long,
// ulong, uint, ushort, float and double but not to byte, sbyte or short, and which a `var`, a boxing
// or a generic call would keep as char. So the cast is dropped only where the slot fixes the type and
// C# accepts the char there (seat #23, COORD's ruling of 2026-10-28):
//
//	runeLiteralKeep   `(rune)'x'`  inference, boxing, builtins, generics, named types, uintptr,
//	                               constant-only expressions, and runes beyond the BMP
//	runeLiteralBare   `'x'`        comparisons and arithmetic against a non-constant operand, typed
//	                               slots of an int32/int/int64/uint/uint32/uint64/uint16/float type,
//	                               and the operand of a conversion to an unnamed basic numeric type
//	runeLiteralTarget `(byte)'x'`  typed slots of a byte, int8 or int16 type, cast to that type
type runeLiteralForm int

const (
	runeLiteralKeep runeLiteralForm = iota
	runeLiteralBare
	runeLiteralTarget
)

// markRuneLiteralForms decides the form of every rune literal in the file from the type go/types
// records for it and its slot. convBasicLit consults the mark; an unmarked literal keeps `(rune)'x'`.
func (v *Visitor) markRuneLiteralForms(file *ast.File) {
	var stack []ast.Node

	ast.Inspect(file, func(n ast.Node) bool {
		if n == nil {
			stack = stack[:len(stack)-1]
			return true
		}

		if lit, ok := n.(*ast.BasicLit); ok && lit.Kind == token.CHAR {
			if form := v.runeLiteralFormOf(lit, stack); form != runeLiteralKeep {
				if v.runeLiteralForms == nil {
					v.runeLiteralForms = map[*ast.BasicLit]runeLiteralForm{}
				}

				v.runeLiteralForms[lit] = form
			}
		}

		stack = append(stack, n)
		return true
	})
}

func (v *Visitor) runeLiteralFormOf(lit *ast.BasicLit, stack []ast.Node) runeLiteralForm {
	tv, ok := v.info.Types[lit]

	if !ok || tv.Type == nil || tv.Value == nil {
		return runeLiteralKeep
	}

	// A rune beyond the BMP is no C# char; it keeps `(rune)0x1D504`.
	if codePoint, exact := constant.Int64Val(constant.ToInt(tv.Value)); !exact || codePoint > 0xFFFF {
		return runeLiteralKeep
	}

	// A named type (no conversion from char) or a type parameter keeps the cast.
	basic, isBasic := types.Unalias(tv.Type).(*types.Basic)

	if !isBasic {
		return runeLiteralKeep
	}

	implicit, narrow := charImplicitKind(basic.Kind()), charNarrowKind(basic.Kind())

	if !implicit && !narrow {
		return runeLiteralKeep
	}

	// The form for a slot that fixes the literal's type: bare where C# converts the char implicitly,
	// cast to the target where it does not.
	typedSlot := runeLiteralBare

	if narrow {
		typedSlot = runeLiteralTarget
	}

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
		return runeLiteralKeep
	}

	switch parent := stack[i].(type) {
	case *ast.BinaryExpr:
		// Against a non-constant operand both sides promote to int (or wider), so the char reads the
		// same as `(rune)'x'` for every basic integer and float operand.
		other := parent.X

		if other == child {
			other = parent.Y
		}

		// The other operand must be a variable of a basic type: an interface operand would compare
		// against a boxed char.
		if otherTV, ok := v.info.Types[other]; !ok || otherTV.Value != nil || otherTV.Type == nil {
			return runeLiteralKeep
		} else if _, isBasic := types.Unalias(otherTV.Type).(*types.Basic); !isBasic {
			return runeLiteralKeep
		}

		switch parent.Op {
		case token.LAND, token.LOR:
			return runeLiteralKeep
		}

		return runeLiteralBare

	case *ast.AssignStmt:
		if parent.Tok == token.DEFINE || !exprInList(child, parent.Rhs) {
			return runeLiteralKeep
		}

	case *ast.ValueSpec:
		if parent.Type == nil || !exprInList(child, parent.Values) || inConstDecl(stack[:i]) {
			return runeLiteralKeep
		}

	case *ast.ReturnStmt, *ast.CompositeLit, *ast.SendStmt:

	case *ast.KeyValueExpr:
		if i == 0 {
			return runeLiteralKeep
		}

		composite, inLiteral := stack[i-1].(*ast.CompositeLit)

		if !inLiteral {
			return runeLiteralKeep
		}

		// An array or slice literal's index key is an int; C# indexes with the char.
		if child == parent.Key {
			switch v.info.TypeOf(composite).Underlying().(type) {
			case *types.Array, *types.Slice:
				return typedSlot
			}
		}

	case *ast.CaseClause:
		// The case label of an expression switch takes the tag's type.
		if !exprInList(child, parent.List) || i < 2 {
			return runeLiteralKeep
		}

		if switchStmt, ok := stack[i-2].(*ast.SwitchStmt); ok && switchStmt.Tag != nil && types.Identical(v.info.TypeOf(switchStmt.Tag), tv.Type) {
			return typedSlot
		}

		return runeLiteralKeep

	case *ast.CallExpr:
		return v.runeLiteralCallForm(parent, child, typedSlot)

	default:
		return runeLiteralKeep
	}

	// A typed slot keeps the cast unless its destination IS the literal's type: an interface
	// destination records the literal at its default type, rune, and C# would box the char.
	if destination := v.narrowDestinationType(lit, stack); destination == nil || !types.Identical(destination, tv.Type) {
		return runeLiteralKeep
	}

	return typedSlot
}

// runeLiteralCallForm is the form of a rune literal that is a call's argument: bare as the operand
// of a conversion to an unnamed basic numeric type (the conversion supplies the one cast), the
// typed-slot form for append's byte elements and for a parameter of a non-generic function, and kept
// for every other builtin, a generic call, a variadic argument and an interface parameter.
func (v *Visitor) runeLiteralCallForm(call *ast.CallExpr, child ast.Node, typedSlot runeLiteralForm) runeLiteralForm {
	funTV := v.info.Types[call.Fun]

	if funTV.IsType() {
		if basic, ok := types.Unalias(funTV.Type).(*types.Basic); ok && (charImplicitKind(basic.Kind()) || charNarrowKind(basic.Kind())) {
			return runeLiteralBare
		}

		return runeLiteralKeep
	}

	if ident, ok := ast.Unparen(call.Fun).(*ast.Ident); ok {
		if _, isBuiltin := v.info.Uses[ident].(*types.Builtin); isBuiltin {
			if ident.Name == "append" && typedSlot == runeLiteralTarget && !call.Ellipsis.IsValid() {
				return runeLiteralTarget
			}

			return runeLiteralKeep
		}
	}

	// A generic call keeps the cast whether its type arguments are written or inferred: the call's own type
	// is the instantiated signature, and C# would infer char from a bare literal.
	switch fun := ast.Unparen(call.Fun).(type) {
	case *ast.IndexExpr, *ast.IndexListExpr:
		return runeLiteralKeep
	case *ast.Ident:
		if _, generic := v.info.Instances[fun]; generic {
			return runeLiteralKeep
		}
	case *ast.SelectorExpr:
		if _, generic := v.info.Instances[fun.Sel]; generic {
			return runeLiteralKeep
		}
	}

	signature, ok := funTV.Type.Underlying().(*types.Signature)

	if !ok || signature.TypeParams().Len() > 0 {
		return runeLiteralKeep
	}

	params := signature.Params()

	for index, arg := range call.Args {
		if arg != child {
			continue
		}

		if index >= params.Len() || signature.Variadic() && index >= params.Len()-1 {
			return runeLiteralKeep
		}

		if _, isBasic := types.Unalias(params.At(index).Type()).(*types.Basic); !isBasic {
			return runeLiteralKeep
		}

		return typedSlot
	}

	return runeLiteralKeep
}

// charImplicitKind reports whether C# converts a char to the basic kind implicitly.
func charImplicitKind(kind types.BasicKind) bool {
	switch kind {
	case types.Int32, types.Int, types.Int64, types.Uint, types.Uint32, types.Uint64, types.Uint16, types.Float32, types.Float64:
		return true
	}

	return false
}

// charNarrowKind reports whether the basic kind is one C# reaches from a char only by a cast.
func charNarrowKind(kind types.BasicKind) bool {
	switch kind {
	case types.Uint8, types.Int8, types.Int16:
		return true
	}

	return false
}

func exprInList(node ast.Node, list []ast.Expr) bool {
	for _, expr := range list {
		if expr == node {
			return true
		}
	}

	return false
}

func inConstDecl(stack []ast.Node) bool {
	for i := len(stack) - 1; i >= 0; i-- {
		if decl, ok := stack[i].(*ast.GenDecl); ok {
			return decl.Tok == token.CONST
		}
	}

	return false
}
