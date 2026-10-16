// untypedIntConstFold.go - Gbtc
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
	"math/big"
)

// untypedIntEvaluationDiffers reports whether the constant integer expression expr, evaluated the way its
// emission evaluates it at run time -- as golib UntypedInt operators, which hold every value as its low 64 bits
// in an int64 -- reaches a different value than Go's exact constant. Go evaluates a constant expression in
// arbitrary precision at compile time; UntypedInt wraps + - *, truncates / and % on the SIGNED bits, and masks a
// shift count to six bits. Most constant arithmetic never leaves int64, or wraps back to the right bits
// (`maxUint64 - 1`), and keeps its operators. When it does not, the expression folds to Go's value
// (foldedMixedIntegerConstLiteral), with its Go text beside it:
//
//   - internal/strconv's compile-time assert `_ = assert[(div1e8m*5*5*5*5*5*5*5*5)%(1<<64)-1]` is the constant
//     index 0, and was -1 at run time (`1 << 64` is 1 in int64), so trimZeros panicked under every shortest
//     %v of a float64 at Go 1.26;
//   - its table entry `maxUint64 / 5` is 3689348814741910323, and was -1 / 5 = 0.
//
// The model is the operator semantics, so it answers for the UntypedInt rendering only; the caller requires an
// untyped named integer constant in expr (containsUntypedNamedIntegerConstRef), which is what makes the
// arithmetic around it UntypedInt. A sub-expression whose value is not an integer (a float operand) has no
// model here and answers false; the float trigger beside this one owns that shape.
func (v *Visitor) untypedIntEvaluationDiffers(expr ast.Expr) bool {
	tv, ok := v.info.Types[expr]

	if !ok || tv.Value == nil {
		return false
	}

	exact, ok := constLow64(tv.Value)

	if !ok {
		return false
	}

	evaluated, ok := v.untypedIntEvaluate(expr)

	return ok && evaluated != exact
}

// untypedIntEvaluate evaluates a constant expression with UntypedInt's run-time operator semantics. A node that
// is not an operator (a literal, a named constant, a conversion) contributes its exact value's low 64 bits,
// which is how UntypedInt stores any value in [MinInt64, MaxUint64].
func (v *Visitor) untypedIntEvaluate(expr ast.Expr) (int64, bool) {
	tv, ok := v.info.Types[expr]

	if !ok || tv.Value == nil {
		return 0, false
	}

	switch e := expr.(type) {
	case *ast.ParenExpr:
		return v.untypedIntEvaluate(e.X)
	case *ast.UnaryExpr:
		x, ok := v.untypedIntEvaluate(e.X)

		if !ok {
			return 0, false
		}

		switch e.Op {
		case token.ADD:
			return x, true
		case token.SUB:
			return -x, true
		case token.XOR:
			return ^x, true
		}

		return 0, false
	case *ast.BinaryExpr:
		x, ok := v.untypedIntEvaluate(e.X)

		if !ok {
			return 0, false
		}

		if e.Op == token.SHL || e.Op == token.SHR {
			countValue, ok := v.info.Types[e.Y]

			if !ok || countValue.Value == nil {
				return 0, false
			}

			count, exact := constant.Uint64Val(constant.ToInt(countValue.Value))

			if !exact {
				return 0, false
			}

			if e.Op == token.SHL {
				return x << (count & 63), true
			}

			return x >> (count & 63), true
		}

		y, ok := v.untypedIntEvaluate(e.Y)

		if !ok {
			return 0, false
		}

		switch e.Op {
		case token.ADD:
			return x + y, true
		case token.SUB:
			return x - y, true
		case token.MUL:
			return x * y, true
		case token.QUO, token.REM:
			if y == 0 {
				return 0, false
			}

			if e.Op == token.QUO {
				return x / y, true
			}

			return x % y, true
		case token.AND:
			return x & y, true
		case token.OR:
			return x | y, true
		case token.XOR:
			return x ^ y, true
		case token.AND_NOT:
			return x &^ y, true
		}

		return 0, false
	}

	return constLow64(tv.Value)
}

// constLow64 is an integer-valued constant's low 64 bits as an int64 (two's complement).
func constLow64(value constant.Value) (int64, bool) {
	integer := constant.ToInt(value)

	if integer.Kind() != constant.Int {
		return 0, false
	}

	if i, exact := constant.Int64Val(integer); exact {
		return i, true
	}

	bits, ok := constant.Val(integer).(*big.Int)

	if !ok {
		return 0, false
	}

	return int64(new(big.Int).Mod(bits, new(big.Int).Lsh(big.NewInt(1), 64)).Uint64()), true
}
