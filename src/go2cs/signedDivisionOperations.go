// signedDivisionOperations.go - Gbtc
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
	"go/constant"
	"go/types"
)

// Go's spec wraps the one overflowing signed quotient, the most negative value divided by -1, to that
// same value, and makes any value modulo -1 zero, with no panic. .NET throws OverflowException for
// both at 32 and 64 bits, and that is not a Go panic: a deferred recover() cannot see it, so the
// program dies. A narrower signed type needs nothing here, because C# promotes it to int (the
// narrow-arithmetic rule then wraps the result), and a NAMED integer type carries the -1 arm inside
// its generated wrapper operators (go2cs-gen's NumericTypeTemplate), so its `a / b` keeps Go's
// spelling. What remains is an UNNAMED int, int32 or int64 (rune and nint included):
//   - a divisor that is a constant -1 folds at conversion: `a / -1` is `unchecked(-a)` and `a % -1`
//     is zero;
//   - any other constant divisor, and a len/cap divisor (never negative), keeps the plain operator;
//   - every other divisor goes through golib's quo(a, b) / rem(a, b), which carry the -1 arm.

// signedDivisionGuardKind reports whether a division or remainder of Go type t is one this rule
// governs: an unnamed int, int32 or int64 (rune and nint are the same kinds).
func signedDivisionGuardKind(t types.Type) bool {
	if t == nil {
		return false
	}

	basic, ok := types.Unalias(t).(*types.Basic)

	if !ok {
		return false
	}

	switch basic.Kind() {
	case types.Int, types.Int32, types.Int64:
		return true
	}

	return false
}

// signedDivisorClass classifies a divisor: guard (quo/rem), minusOne (fold), or neither (plain).
func (v *Visitor) signedDivisorClass(divisor ast.Expr) (guard bool, minusOne bool) {
	if tv, ok := v.info.Types[divisor]; ok && tv.Value != nil {
		if value, exact := constant.Int64Val(constant.ToInt(tv.Value)); exact && value == -1 {
			return false, true
		}

		return false, false
	}

	if call, ok := ast.Unparen(divisor).(*ast.CallExpr); ok {
		if ident, ok := ast.Unparen(call.Fun).(*ast.Ident); ok && (ident.Name == "len" || ident.Name == "cap") {
			if _, isBuiltin := v.info.Uses[ident].(*types.Builtin); isBuiltin {
				return false, false
			}
		}
	}

	return true, false
}

// signedDivisionHelperName spells golib's quo or rem, qualified as `builtin.<name>` wherever a Go
// name would bind first in C#: a package-level declaration of that name (go/constant declares its own
// `quo`), or a variable of that name anywhere in the enclosing function — a C# local is in scope in
// its whole block, including its own initializer, so `if rem := a % b; …` needs the qualification
// even though Go's scope for rem starts after the statement.
func (v *Visitor) signedDivisionHelperName(name string) string {
	shadowed := packageFuncMethodNames != nil && packageFuncMethodNames[name] ||
		v.funcScopeVarNames.Contains(name) ||
		v.pkg != nil && v.pkg.Scope().Lookup(name) != nil

	if shadowed {
		return "builtin." + name
	}

	return name
}

// signedDivisionExpr renders a governed division (quotient true) or remainder of left by right, or
// returns false when the operation keeps the plain C# operator. dividend is the Go dividend, whose
// side effects a folded remainder must not drop.
func (v *Visitor) signedDivisionExpr(quotient bool, resultType types.Type, dividend, divisor ast.Expr, left, right string) (string, bool) {
	if !signedDivisionGuardKind(resultType) {
		return "", false
	}

	guard, minusOne := v.signedDivisorClass(divisor)

	switch {
	case minusOne && quotient:
		return fmt.Sprintf("unchecked(-%s)", v.parenthesizedOperand(dividend, left)), true
	case minusOne && isSideEffectFreeOperand(dividend):
		return fmt.Sprintf("(%s)0", convertToCSTypeName(v.getAliasQualifiedTypeName(resultType, false))), true
	case minusOne || guard:
		name := "rem"

		if quotient {
			name = "quo"
		}

		return fmt.Sprintf("%s(%s, %s)", v.signedDivisionHelperName(name), left, right), true
	}

	return "", false
}

// parenthesizedOperand wraps a rendered operand in parentheses unless it is a single primary term.
func (v *Visitor) parenthesizedOperand(expr ast.Expr, rendered string) string {
	switch ast.Unparen(expr).(type) {
	case *ast.Ident, *ast.SelectorExpr, *ast.IndexExpr, *ast.CallExpr, *ast.BasicLit:
		return rendered
	}

	if isFullyParenthesized(rendered) {
		return rendered
	}

	return "(" + rendered + ")"
}

// isSideEffectFreeOperand reports whether evaluating expr can have no effect beyond its value, so a
// folded `x % -1` may drop it: an identifier, a field chain of identifiers or a literal.
func isSideEffectFreeOperand(expr ast.Expr) bool {
	switch e := ast.Unparen(expr).(type) {
	case *ast.Ident, *ast.BasicLit:
		return true
	case *ast.SelectorExpr:
		return isSideEffectFreeOperand(e.X)
	}

	return false
}

// isSideEffectFreeLvalue reports whether an assignment target can be read twice without effect, so a
// compound `x /= b` may become `x = quo(x, b)`: an identifier, a field chain of identifiers, or an
// index of such a base by such an index.
func isSideEffectFreeLvalue(expr ast.Expr) bool {
	switch e := ast.Unparen(expr).(type) {
	case *ast.Ident:
		return true
	case *ast.SelectorExpr:
		return isSideEffectFreeLvalue(e.X)
	case *ast.IndexExpr:
		return isSideEffectFreeLvalue(e.X) && isSideEffectFreeOperand(e.Index)
	}

	return false
}
