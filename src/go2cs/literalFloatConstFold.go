// literalFloatConstFold.go - Gbtc
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
	"strings"
)

// foldedInexactLiteralFloatConst folds a float constant expression built ONLY from literals
// (`1 - .999`, `0.1 + 0.2`) when C# would compute a different value than Go, and returns "" otherwise.
//
// Go evaluates a constant expression exactly and rounds once, to the expression's type. The converter
// keeps a pure-literal expression in its operator form (`1D - .999D`), which C# evaluates one operation
// at a time, rounding after each step. For most expressions the two agree (`1.5 * 2.0`), and the
// operator form is kept, because it reads like the Go. Where they differ, a decimal with no exact
// binary form makes the step-by-step result land off Go's value: `1 - .999` is 0.001 in Go and
// 0.0010000000000000009 in C# (internal/trace/traceviewer's quantiles). Only then is the expression
// folded, in the same `/* <go> */ <value>` form foldedNamedFloatConstLiteral uses, so the Go text stays
// beside the value. An expression with a named constant is foldedNamedFloatConstLiteral's case.
func (v *Visitor) foldedInexactLiteralFloatConst(expr *ast.BinaryExpr) string {
	tv, ok := v.info.Types[expr]

	if !ok || tv.Value == nil || tv.Value.Kind() != constant.Float || !isLiteralConstArithmetic(expr) || v.keptLiteralFloatConsts[expr] {
		return ""
	}

	targetCSType := v.concreteNumericCSType(tv.Type)
	isFloat32 := targetCSType == "float32"

	if targetCSType != "float64" && !isFloat32 {
		return ""
	}

	// The decision is made once, for the outermost literal expression: emission reaches it before its
	// operands. When it stays in operator form its nested literal operations stay too, so the value C#
	// computes is exactly the stepped value checked here.
	stepped, ok := v.steppedLiteralOperation(expr, isFloat32)

	if !ok || stepped == roundedConstFloat(tv.Value, isFloat32) {
		v.keepNestedLiteralFloatConsts(expr)
		return ""
	}

	suffix := "D"

	if isFloat32 {
		suffix = "F"
	}

	return fmt.Sprintf("/* %s */ %s%s", strings.TrimSpace(v.getPrintedNode(expr)), exactFloatText(tv.Value, "", isFloat32), suffix)
}

// keepNestedLiteralFloatConsts marks every literal operation nested inside expr as kept in operator form.
func (v *Visitor) keepNestedLiteralFloatConsts(expr *ast.BinaryExpr) {
	ast.Inspect(expr, func(n ast.Node) bool {
		if nested, ok := n.(*ast.BinaryExpr); ok && nested != expr {
			if v.keptLiteralFloatConsts == nil {
				v.keptLiteralFloatConsts = map[*ast.BinaryExpr]bool{}
			}

			v.keptLiteralFloatConsts[nested] = true
		}

		return true
	})
}

// isLiteralConstArithmetic reports whether expr is +, -, * or / over integer and float literals only.
func isLiteralConstArithmetic(expr ast.Expr) bool {
	switch e := expr.(type) {
	case *ast.BasicLit:
		return e.Kind == token.INT || e.Kind == token.FLOAT
	case *ast.ParenExpr:
		return isLiteralConstArithmetic(e.X)
	case *ast.UnaryExpr:
		return (e.Op == token.SUB || e.Op == token.ADD) && isLiteralConstArithmetic(e.X)
	case *ast.BinaryExpr:
		switch e.Op {
		case token.ADD, token.SUB, token.MUL, token.QUO:
			return isLiteralConstArithmetic(e.X) && isLiteralConstArithmetic(e.Y)
		}
	}

	return false
}

// roundedConstFloat is Go's value for a constant at the target width: exact, rounded once.
func roundedConstFloat(val constant.Value, isFloat32 bool) float64 {
	if isFloat32 {
		f32, _ := constant.Float32Val(constant.ToFloat(val))
		return float64(f32)
	}

	f64, _ := constant.Float64Val(constant.ToFloat(val))
	return f64
}

// steppedLiteralValue is the value the emitted C# produces for a literal-only sub-expression: a
// literal is parsed to the nearest double (float for float32), a pair of integer operands stays C#
// integer arithmetic and so is exact (7 / 2 is 3, as in Go), and a float operation rounds after each step.
func (v *Visitor) steppedLiteralValue(expr ast.Expr, isFloat32 bool) (float64, bool) {
	switch e := expr.(type) {
	case *ast.ParenExpr:
		return v.steppedLiteralValue(e.X, isFloat32)
	case *ast.UnaryExpr:
		value, ok := v.steppedLiteralValue(e.X, isFloat32)

		if e.Op == token.SUB {
			value = -value
		}

		return value, ok
	case *ast.BasicLit:
		tv, ok := v.info.Types[e]

		if !ok || tv.Value == nil {
			return 0, false
		}

		return roundedConstFloat(tv.Value, isFloat32), true
	case *ast.BinaryExpr:
		tv, ok := v.info.Types[e]

		if !ok || tv.Value == nil {
			return 0, false
		}

		return v.steppedLiteralOperation(e, isFloat32)
	}

	return 0, false
}

// steppedLiteralOperation applies the expression's own operator to its operands' stepped values.
func (v *Visitor) steppedLiteralOperation(e *ast.BinaryExpr, isFloat32 bool) (float64, bool) {
	left, right := v.info.Types[e.X].Value, v.info.Types[e.Y].Value

	if left == nil || right == nil {
		return 0, false
	}

	// Two integer operands stay C# integer arithmetic, which matches Go's exact integer result.
	if left.Kind() == constant.Int && right.Kind() == constant.Int {
		op := e.Op

		if op == token.QUO {
			op = token.QUO_ASSIGN // go/constant's truncating integer division
		}

		result, exact := constant.Int64Val(constant.BinaryOp(left, op, right))

		if !exact {
			return 0, false
		}

		return float64(result), true
	}

	l, okL := v.steppedLiteralValue(e.X, isFloat32)
	r, okR := v.steppedLiteralValue(e.Y, isFloat32)

	if !okL || !okR {
		return 0, false
	}

	if isFloat32 {
		a, b := float32(l), float32(r)

		switch e.Op {
		case token.ADD:
			return float64(float32(a + b)), true
		case token.SUB:
			return float64(float32(a - b)), true
		case token.MUL:
			return float64(float32(a * b)), true
		case token.QUO:
			return float64(float32(a / b)), true
		}

		return 0, false
	}

	switch e.Op {
	case token.ADD:
		return float64(l + r), true
	case token.SUB:
		return float64(l - r), true
	case token.MUL:
		return float64(l * r), true
	case token.QUO:
		return float64(l / r), true
	}

	return 0, false
}
