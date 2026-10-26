// parityOperatorEmission.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// This file holds the parity stage's OPERATOR rules (the PA seat, ruled 2026-10-19): a shift or bitwise expression
// spells only the parentheses C# needs to group it as Go does, a bitwise result that C# already types as Go's
// result keeps no re-cast, and a literal shift count is written as Go writes it.
//
// C# ranks two Go operator families differently. Go puts `<<` `>>` `&` `&^` beside `*`, and `|` `^` beside `+`, all
// above the comparisons. C# puts the shifts BELOW `+` and `-`, and `&` `^` `|` BELOW the comparisons, each on its own
// level. So `x | y << 8` needs no parentheses in either language, while Go's `a&m == 0` is C#'s `(a & m) == 0`.
//
// A shift or bitwise expression still RENDERS parenthesized, which is correct in every position, and every cast an
// emission arm prefixes to an operand relies on it. The ENCLOSING operator then drops the parentheses of an operand
// at the moment it composes its own text, and only when (a) the operand is still exactly one parenthesized
// expression (a cast prefixed to it makes it something else, so a cast's grouping is never touched) and (b) C#'s
// precedence groups the bare operand as Go's tree does.

package main

import (
	"go/ast"
	"go/constant"
	"go/token"
	"go/types"
	"math"
	"regexp"
	"strconv"
	"strings"
)

// bitwiseResultCastRedundant reports whether a bitwise expression's `(T)(…)` result cast can be dropped because C#
// already types the result as T. That holds for a NON-constant expression whose Go type is a plain basic integer C#
// does not promote (int32, uint32, int64, uint64, int, uint) when every operand renders as T (bitwiseOperandIsT).
// A sub-int result keeps it (C# computes `byte | byte` in `int`), and so do a named numeric, `uintptr` (a golib
// struct with its own operators), and a constant expression (the constant folds keep their own casts).
func (v *Visitor) bitwiseResultCastRedundant(binaryExpr *ast.BinaryExpr, csType, left, right string) bool {
	tv, ok := v.info.Types[binaryExpr]

	if !ok || tv.Value != nil || tv.Type == nil {
		return false
	}

	basic, ok := tv.Type.(*types.Basic)

	if !ok {
		return false
	}

	switch basic.Kind() {
	case types.Int32, types.Uint32, types.Int64, types.Uint64, types.Int, types.Uint:
	default:
		return false
	}

	// The right operand of `&^` renders complemented, `~y`: a constant there complements as C# `int`.
	if binaryExpr.Op == token.AND_NOT && !v.bitwiseOperandIsT(binaryExpr.Y, basic, csType, right, false) {
		return false
	}

	return v.bitwiseOperandIsT(binaryExpr.X, basic, csType, left, true) && v.bitwiseOperandIsT(binaryExpr.Y, basic, csType, right, true)
}

// castPrimary matches what may follow a prefixed `(T)` cast for the cast to cover the whole operand: a name, a
// member path or an indexed one, with no operator at the top level.
var castPrimary = regexp.MustCompile(`^[\w@.\x{13D1}\x{0394}]+(\[[^\[\]]*\])*$`)

// bitwiseOperandIsT reports whether an operand of a bitwise expression of type `basic` renders as the C# type
// `csType`: a non-constant operand of the identical Go type; an operand an emission arm already cast to `csType`; a
// reference to a typed constant of the identical type; or, where `literals` allows, an integer literal C# converts to
// `csType` without widening the result.
func (v *Visitor) bitwiseOperandIsT(operand ast.Expr, basic *types.Basic, csType, rendered string, literals bool) bool {
	if prefix := "(" + csType + ")"; strings.HasPrefix(rendered, prefix) {
		rest := rendered[len(prefix):]

		if wrappedWhole(rest) || castPrimary.MatchString(rest) {
			return true
		}
	}

	tv, ok := v.info.Types[operand]

	if !ok || tv.Type == nil {
		return false
	}

	if tv.Value == nil {
		return types.Identical(tv.Type, basic)
	}

	// A typed constant referenced by name renders as its C# constant, of the same type.
	switch ref := ast.Unparen(operand).(type) {
	case *ast.Ident, *ast.SelectorExpr:
		var name *ast.Ident

		if ident, ok := ref.(*ast.Ident); ok {
			name = ident
		} else {
			name = ref.(*ast.SelectorExpr).Sel
		}

		if c, ok := v.info.ObjectOf(name).(*types.Const); ok && types.Identical(c.Type(), basic) {
			return true
		}

		return false
	case *ast.BasicLit:
		if !literals || ref.Kind != token.INT {
			return false
		}

		value, exact := constant.Uint64Val(constant.ToInt(tv.Value))

		if !exact {
			return false
		}

		// C# types a literal as the first of int, uint, long, ulong that holds it; each limit keeps the operation in
		// csType (an `int` result with a `uint` literal would compute in `long`).
		switch basic.Kind() {
		case types.Int32, types.Int:
			return value <= math.MaxInt32
		case types.Uint32, types.Uint:
			return value <= math.MaxUint32
		case types.Int64:
			return value <= math.MaxInt64
		case types.Uint64:
			return true
		}
	}

	return false
}

// unsignedLiteral matches a C# numeric or character literal with no sign: `500`, `0x1F`, `1.5e3`, `8UL`, `'a'`.
// C# reads `(T)` followed by a literal as a cast for every type T, so such an operand needs no parentheses.
var unsignedLiteral = regexp.MustCompile(`^(?:(?:0[xX][0-9A-Fa-f_]+|0[bB][01_]+|\d[\d_]*(?:\.\d[\d_]*)?(?:[eE][+-]?\d+)?)[uUlLfFdDmM]{0,2}|'(?:[^'\\]|\\.[^']*)')$`)

// castOf renders the cast of a rendered expression to csType: `(T)500` for an unsigned literal, as Go writes
// `T(500)`; `(T)(expr)` for an expression already rendered as ONE parenthesized expression, which keeps its own
// pair rather than taking a second (`(int)(c[3] & 63)` for Go's `u >> (c[3] & 63)`, where the pair is Go's own); and
// `(T)(expr)` around anything else. C# reads `(T)` before a parenthesized expression as a cast for every type T.
func castOf(csType, rendered string) string {
	if unsignedLiteral.MatchString(rendered) || wrappedWhole(rendered) {
		return "(" + csType + ")" + rendered
	}

	return "(" + csType + ")(" + rendered + ")"
}

// shiftCountIsIntLiteral reports whether a shift count is an integer literal C# already reads as `int` (it fits
// int32), so the count needs no `(int)` cast: Go's `x << 8` stays `x << 8`.
func shiftCountIsIntLiteral(count ast.Expr) bool {
	literal, ok := ast.Unparen(count).(*ast.BasicLit)

	if !ok || literal.Kind != token.INT {
		return false
	}

	value, err := strconv.ParseInt(literal.Value, 0, 64)

	return err == nil && value >= 0 && value <= 1<<31-1
}

// csOperatorPrecedence is C#'s binding strength for a Go binary operator as the converter renders it (`&^` renders
// `& ~`), higher binding tighter; 0 for anything else.
func csOperatorPrecedence(op token.Token) int {
	switch op {
	case token.MUL, token.QUO, token.REM:
		return 10
	case token.ADD, token.SUB:
		return 9
	case token.SHL, token.SHR:
		return 8
	case token.LSS, token.LEQ, token.GTR, token.GEQ:
		return 7
	case token.EQL, token.NEQ:
		return 6
	case token.AND, token.AND_NOT:
		return 5
	case token.XOR:
		return 4
	case token.OR:
		return 3
	case token.LAND:
		return 2
	case token.LOR:
		return 1
	}

	return 0
}

// operandParensNeeded reports whether an operand that is a binary expression of operator `child` must stay
// parenthesized as the `right` (or left) operand of `parent`. Go's tree already reflects Go's precedence, so the
// operand sits under its parent exactly as Go groups it. C# keeps that grouping without parentheses when the operand
// binds tighter than its parent, or binds equally and is the LEFT operand (every binary operator involved associates
// left in both languages).
func operandParensNeeded(child, parent token.Token, right bool) bool {
	own, enclosing := csOperatorPrecedence(child), csOperatorPrecedence(parent)

	if own == 0 || enclosing == 0 {
		return true
	}

	return own < enclosing || own == enclosing && right
}

// unparenthesizedOperand drops the parentheses of a rendered operand of `parent` where C# does not need them: the
// operand's Go expression is a shift or bitwise expression (written without Go parentheses, which stay as written),
// its rendering is still exactly one parenthesized expression, and C# groups the bare operand as Go does.
func unparenthesizedOperand(rendered string, operand ast.Expr, parent token.Token, right bool) string {
	binary, ok := operand.(*ast.BinaryExpr)

	if !ok || !isShiftOrBitwise(binary.Op) || !wrappedWhole(rendered) || operandParensNeeded(binary.Op, parent, right) {
		return rendered
	}

	return rendered[1 : len(rendered)-1]
}

// unparenthesizedWholeExpression drops the outer parentheses of a rendering that stands as a WHOLE expression (a call
// argument, an assigned value, a returned one), where nothing outside can bind into it. Only two shapes take it,
// both written without Go parentheses: a type conversion (`((slice<T>)x)` -> `(slice<T>)x`) and a shift or bitwise
// expression. Neither leaves a `<` or `>` at its own top level (a cast's type sits inside the cast's parentheses,
// and `<<` is one token), so dropping the pair can never re-read an argument list as a generic one.
func (v *Visitor) unparenthesizedWholeExpression(expr ast.Expr, rendered string) string {
	if !wrappedWhole(rendered) {
		return rendered
	}

	switch e := expr.(type) {
	case *ast.BinaryExpr:
		if !isShiftOrBitwise(e.Op) {
			return rendered
		}
	case *ast.CallExpr:
		if !v.callExprIsTypeConversion(e) {
			return rendered
		}
	default:
		return rendered
	}

	return rendered[1 : len(rendered)-1]
}

func isShiftOrBitwise(op token.Token) bool {
	switch op {
	case token.SHL, token.SHR, token.AND, token.OR, token.XOR, token.AND_NOT:
		return true
	}

	return false
}

// wrappedWhole reports whether `expr` is ONE parenthesized expression: its first character opens a parenthesis
// that closes at its last character, and nothing at the top level inside is a comma (a tuple is not a
// parenthesized expression). String, character and comment text is skipped, so a parenthesis inside a literal
// never counts.
func wrappedWhole(expr string) bool {
	if len(expr) < 2 || expr[0] != '(' || expr[len(expr)-1] != ')' {
		return false
	}

	depth := 0

	for i := 0; i < len(expr); i++ {
		switch expr[i] {
		case '"':
			i = skipQuoted(expr, i)
		case '\'':
			i = skipCharLiteral(expr, i)
		case '/':
			if i+1 < len(expr) && expr[i+1] == '*' {
				end := indexFrom(expr, "*/", i+2)

				if end < 0 {
					return false
				}

				i = end + 1
			}
		case '(':
			depth++
		case ')':
			depth--

			if depth == 0 && i != len(expr)-1 {
				return false
			}
		case ',':
			if depth == 1 {
				return false
			}
		}

		if i >= len(expr) {
			return false
		}
	}

	return depth == 0
}

// skipQuoted returns the index of the closing quote of the string literal opening at `start`: a regular literal
// (backslash escapes), a verbatim one (`@"…"`, doubled quotes), or a raw one (`"""…"""`).
func skipQuoted(expr string, start int) int {
	if start+2 < len(expr) && expr[start+1] == '"' && expr[start+2] == '"' {
		end := indexFrom(expr, `"""`, start+3)

		if end < 0 {
			return len(expr)
		}

		return end + 2
	}

	verbatim := start > 0 && expr[start-1] == '@'

	for i := start + 1; i < len(expr); i++ {
		switch expr[i] {
		case '\\':
			if !verbatim {
				i++
			}
		case '"':
			if verbatim && i+1 < len(expr) && expr[i+1] == '"' {
				i++
				continue
			}

			return i
		}
	}

	return len(expr)
}

// skipCharLiteral returns the index of the closing quote of the character literal opening at `start`.
func skipCharLiteral(expr string, start int) int {
	for i := start + 1; i < len(expr); i++ {
		switch expr[i] {
		case '\\':
			i++
		case '\'':
			return i
		}
	}

	return len(expr)
}

func indexFrom(s, sub string, from int) int {
	if from > len(s) {
		return -1
	}

	for i := from; i+len(sub) <= len(s); i++ {
		if s[i:i+len(sub)] == sub {
			return i
		}
	}

	return -1
}

// unparenthesizedAssignedValue is unparenthesizedWholeExpression for an assigned value. Only a plain `=` or `:=`
// qualifies: a compound assignment can render an operator ahead of the value (`&^=` is `&= ~`), which binds to the
// value's first token.
func (v *Visitor) unparenthesizedAssignedValue(assignStmt *ast.AssignStmt, rhs ast.Expr, rendered string) string {
	if assignStmt.Tok != token.ASSIGN && assignStmt.Tok != token.DEFINE {
		return rendered
	}

	return v.unparenthesizedWholeExpression(rhs, rendered)
}
