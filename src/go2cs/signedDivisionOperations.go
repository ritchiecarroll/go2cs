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
	"go/token"
	"go/types"
	"math"
)

// Go's spec wraps the one overflowing signed quotient, the most negative value divided by -1, to that
// same value, and makes any value modulo -1 zero, with no panic. .NET throws OverflowException for
// both at 32 and 64 bits, and that is not a Go panic: a deferred recover() cannot see it, so the
// program dies. A narrower signed type needs nothing here, because C# promotes it to int (the
// narrow-arithmetic rule then wraps the result), and a NAMED integer type carries the -1 arm inside
// its generated wrapper operators (go2cs-gen's NumericTypeTemplate), so its `a / b` keeps Go's
// spelling. What remains is an UNNAMED int, int32 or int64 (rune and nint included), and a type
// parameter whose type set holds one:
//   - a divisor that is a constant -1 folds at conversion: `a / -1` is `unchecked(-a)`, and `a % -1`
//     is zero, spelled `(T)0` when reading a is free of effect and `a * 0` when it is not (a call, an
//     index, a pointer hop), so a's own panic survives;
//   - any other constant divisor, and a len/cap divisor (never negative), keeps the plain operator;
//   - so does a dividend that cannot be the most negative value: a constant other than it, or len/cap;
//   - everything else goes through golib's quo(a, b) / rem(a, b), which carry the -1 arm.
// A compound `x /= b` or `x %= b` follows the same classes (see signedCompoundAssign).

// signedDivisionClass is how a division's type takes this rule: not at all, as a basic int, int32
// or int64, or as a type parameter whose type set holds one.
type signedDivisionClass int

const (
	signedDivisionNone signedDivisionClass = iota
	signedDivisionBasic
	signedDivisionTypeParam
)

// signedDivisionGuardKind classifies a division or remainder of Go type t.
func signedDivisionGuardKind(t types.Type) signedDivisionClass {
	if t == nil {
		return signedDivisionNone
	}

	switch typ := types.Unalias(t).(type) {
	case *types.Basic:
		if isGovernedSignedKind(typ.Kind()) {
			return signedDivisionBasic
		}
	case *types.TypeParam:
		if typeSetHoldsGovernedSigned(typ.Constraint(), map[types.Type]bool{}) {
			return signedDivisionTypeParam
		}
	}

	return signedDivisionNone
}

func isGovernedSignedKind(kind types.BasicKind) bool {
	return kind == types.Int || kind == types.Int32 || kind == types.Int64
}

// typeSetHoldsGovernedSigned reports whether a constraint's type set includes int, int32 or int64 --
// as a term (`~int64`, `int | uint8`), through an embedded interface (constraints.Integer), or as
// the constraint itself.
func typeSetHoldsGovernedSigned(t types.Type, seen map[types.Type]bool) bool {
	if t == nil || seen[t] {
		return false
	}

	seen[t] = true

	switch typ := types.Unalias(t).(type) {
	case *types.Basic:
		return isGovernedSignedKind(typ.Kind())
	case *types.Union:
		for i := 0; i < typ.Len(); i++ {
			if typeSetHoldsGovernedSigned(typ.Term(i).Type(), seen) {
				return true
			}
		}
	case *types.Named:
		if iface, ok := typ.Underlying().(*types.Interface); ok {
			return typeSetHoldsGovernedSigned(iface, seen)
		}
	case *types.Interface:
		for i := 0; i < typ.NumEmbeddeds(); i++ {
			if typeSetHoldsGovernedSigned(typ.EmbeddedType(i), seen) {
				return true
			}
		}
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

	if isBuiltinLenOrCap(v.info, divisor) {
		return false, false
	}

	return true, false
}

// isBuiltinLenOrCap reports whether expr is a call of the builtin len or cap, never negative.
func isBuiltinLenOrCap(info *types.Info, expr ast.Expr) bool {
	call, ok := ast.Unparen(expr).(*ast.CallExpr)

	if !ok {
		return false
	}

	ident, ok := ast.Unparen(call.Fun).(*ast.Ident)

	if !ok || (ident.Name != "len" && ident.Name != "cap") {
		return false
	}

	_, isBuiltin := info.Uses[ident].(*types.Builtin)

	return isBuiltin
}

// signedDividendCannotOverflow reports whether a dividend can never be the most negative value, so
// the division keeps the plain operator whatever its divisor: a constant other than that value, or
// len/cap. An int is nint, so both widths' minimum counts for it.
func (v *Visitor) signedDividendCannotOverflow(dividend ast.Expr) bool {
	if isBuiltinLenOrCap(v.info, dividend) {
		return true
	}

	tv, ok := v.info.Types[dividend]

	if !ok || tv.Value == nil {
		return false
	}

	value := constant.ToInt(tv.Value)

	for _, minimum := range []int64{math.MinInt64, math.MinInt32} {
		if constant.Compare(value, token.EQL, constant.MakeInt64(minimum)) {
			return false
		}
	}

	return true
}

// signedDivisionHelperName spells golib's quo or rem, qualified as `builtin.<name>` wherever a Go
// name would bind first in C#: a package-level declaration of that name (go/constant declares its own
// `quo`), or any object of that name declared anywhere in the enclosing function -- a variable, a
// constant, a type, or a type-switch binding (whose per-clause objects live in info.Implicits). A C#
// local is in scope in its whole block, including before its declaration, so `r := a % b; const rem =
// 10` needs the qualification even though Go's scope for rem starts after it.
func (v *Visitor) signedDivisionHelperName(name string) string {
	shadowed := packageFuncMethodNames != nil && packageFuncMethodNames[name] ||
		v.funcScopeVarNames.Contains(name) ||
		v.pkg != nil && v.pkg.Scope().Lookup(name) != nil ||
		v.funcDeclaresName(name)

	if shadowed {
		return "builtin." + name
	}

	return name
}

// funcDeclaresName reports whether the function being converted declares an object named name at
// any depth, of any kind.
func (v *Visitor) funcDeclaresName(name string) bool {
	if v.currentFuncDecl == nil {
		return false
	}

	found := false

	ast.Inspect(v.currentFuncDecl, func(n ast.Node) bool {
		if found {
			return false
		}

		switch node := n.(type) {
		case *ast.Ident:
			if obj := v.info.Defs[node]; obj != nil && obj.Name() == name {
				found = true
			}
		case *ast.CaseClause:
			if obj := v.info.Implicits[node]; obj != nil && obj.Name() == name {
				found = true
			}
		}

		return !found
	})

	return found
}

// signedDivisionExpr renders a governed division (quotient true) or remainder of left by right, or
// returns false when the operation keeps the plain C# operator. dividend is the Go dividend, whose
// evaluation a folded remainder must keep when it can panic or has an effect.
func (v *Visitor) signedDivisionExpr(quotient bool, resultType types.Type, dividend, divisor ast.Expr, left, right string) (string, bool) {
	class := signedDivisionGuardKind(resultType)

	if class == signedDivisionNone {
		return "", false
	}

	guard, minusOne := v.signedDivisorClass(divisor)

	switch {
	case minusOne && quotient:
		return fmt.Sprintf("unchecked(-%s)", v.parenthesizedOperand(dividend, left)), true
	case minusOne && class == signedDivisionTypeParam:
		// A type parameter has no literal zero to fold to; the helper evaluates the dividend once.
		return fmt.Sprintf("%s(%s, %s)", v.signedDivisionHelperName("rem"), left, right), true
	case minusOne && v.isFoldableDividend(dividend):
		return fmt.Sprintf("(%s)0", convertToCSTypeName(v.getAliasQualifiedTypeName(resultType, false))), true
	case minusOne:
		// Evaluated for its effect and its panic, then zero: `f() % -1` still calls f once, and
		// `sl[i] % -1` / `p.x % -1` still check the index or the pointer.
		return fmt.Sprintf("%s * 0", v.parenthesizedOperand(dividend, left)), true
	case !guard || v.signedDividendCannotOverflow(dividend):
		return "", false
	}

	name := "rem"

	if quotient {
		name = "quo"
	}

	return fmt.Sprintf("%s(%s, %s)", v.signedDivisionHelperName(name), left, right), true
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

// isFoldableDividend reports whether a folded `x % -1` may drop x's evaluation: reading x can neither
// panic nor have an effect. That is an identifier, a literal, a package-qualified name, or a field
// selector chain over such a base that never goes through a pointer (each pointer hop, explicit or
// promoted through an embedded pointer, is a nil check).
func (v *Visitor) isFoldableDividend(expr ast.Expr) bool {
	switch e := ast.Unparen(expr).(type) {
	case *ast.Ident, *ast.BasicLit:
		return true
	case *ast.SelectorExpr:
		if ident, ok := e.X.(*ast.Ident); ok {
			if _, isPkg := v.info.Uses[ident].(*types.PkgName); isPkg {
				return true
			}
		}

		selection := v.info.Selections[e]

		if selection == nil || selection.Kind() != types.FieldVal || selection.Indirect() {
			return false
		}

		return v.isFoldableDividend(e.X)
	}

	return false
}

// isSideEffectFreeOperand reports whether evaluating expr can have no effect beyond its value, so an
// index built from it may be read twice: an identifier, a field chain of identifiers or a literal.
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

// signedCompoundForm is how a governed compound `x /= b` or `x %= b` renders: the text written in
// place of the target (empty keeps the converted target), the operator written after it, whether
// the right operand is left out (a fold whose operator already says everything), and whether a `)`
// closes the statement.
type signedCompoundForm struct {
	target      string
	operator    string
	omitOperand bool
	close       bool
}

// signedCompoundAssign renders a compound `x /= b` or `x %= b` on a governed type so it never reaches
// C#'s throwing operator:
//   - a constant -1 divisor folds: `x /= -1` is `x *= -1` (C#'s unchecked multiply wraps MinInt to
//     itself, as Go's quotient does) and `x %= -1` is `x = 0`, the target evaluated once either way;
//   - a target that can be read twice without effect takes `x = quo(x, b)`;
//   - any other target takes golib's `ref this` twin, `x.QuoAssign(b)`, so it is evaluated once; a map
//     element has no ref, so it takes the map and its key, `m.QuoAssign(key, b)`.
//
// A type parameter's target folds or takes `x = quo(x, b)` when it can be read twice; otherwise the
// plain operator stays, since a `ref this` receiver of a type parameter needs a struct constraint
// that a Go constraint never carries.
func (v *Visitor) signedCompoundAssign(assignStmt *ast.AssignStmt) (signedCompoundForm, bool) {
	if len(assignStmt.Lhs) != 1 || len(assignStmt.Rhs) != 1 {
		return signedCompoundForm{}, false
	}

	lhs := assignStmt.Lhs[0]
	class := signedDivisionGuardKind(v.info.TypeOf(lhs))

	if class == signedDivisionNone {
		return signedCompoundForm{}, false
	}

	quotient := assignStmt.Tok == token.QUO_ASSIGN
	guard, minusOne := v.signedDivisorClass(assignStmt.Rhs[0])
	readTwice := isSideEffectFreeLvalue(lhs)

	name, assignName := "rem", "RemAssign"

	if quotient {
		name, assignName = "quo", "QuoAssign"
	}

	switch {
	case minusOne && class == signedDivisionTypeParam && quotient && readTwice:
		return signedCompoundForm{operator: fmt.Sprintf(" = unchecked(-%s)", v.convExpr(lhs, nil)), omitOperand: true}, true
	case minusOne && class == signedDivisionTypeParam && !quotient:
		return signedCompoundForm{operator: " = default!", omitOperand: true}, true
	case minusOne && class == signedDivisionBasic && quotient:
		return signedCompoundForm{operator: " *= "}, true
	case minusOne && class == signedDivisionBasic:
		return signedCompoundForm{operator: " = 0", omitOperand: true}, true
	case !guard && !minusOne:
		return signedCompoundForm{}, false
	case readTwice:
		return signedCompoundForm{operator: fmt.Sprintf(" = %s(%s, ", v.signedDivisionHelperName(name), v.convExpr(lhs, nil)), close: true}, true
	case class == signedDivisionTypeParam:
		return signedCompoundForm{}, false
	}

	if index, ok := ast.Unparen(lhs).(*ast.IndexExpr); ok {
		if containerType := v.info.TypeOf(index.X); containerType != nil {
			if _, isMap := containerType.Underlying().(*types.Map); isMap {
				return signedCompoundForm{
					target:   v.convExpr(index.X, nil),
					operator: fmt.Sprintf(".%s(%s, ", assignName, v.convExpr(index.Index, nil)),
					close:    true,
				}, true
			}
		}
	}

	return signedCompoundForm{operator: "." + assignName + "(", close: true}, true
}
