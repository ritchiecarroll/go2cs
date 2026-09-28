// narrowArithmeticOperations.go - Gbtc
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

// narrowConsumer classifies what the consumer of a narrow-integer arithmetic result needs from it.
type narrowConsumer int

const (
	// narrowConsumerInvariant: the consumer only reads the low bits, and something above it narrows
	// the final result, so the C# int-promoted value is exact — a same-width `+ - * & | ^ &^`, the
	// left operand of `<<`, a unary `- ^ +`, or a conversion to an integer no wider than the operand.
	narrowConsumerInvariant narrowConsumer = iota

	// narrowConsumerValue: the consumer reads the whole VALUE but not its C# type — a widening or
	// float conversion, an index, a slice bound, a shift count, a `/`, `%` or `>>` operand, a switch
	// tag or case value. Only a result that can leave the narrow range in C# needs the cast.
	narrowConsumerValue

	// narrowConsumerTyped: the consumer takes the result AS its Go type — a typed destination, a
	// comparison, an interface or generic argument, a composite element, a channel send, a map key.
	// Every narrow result needs the cast: an interface boxes the C# type, and generic inference and
	// overload resolution bind it.
	narrowConsumerTyped
)

// markNarrowArithmeticContexts records, for every non-constant narrow-integer (int8/uint8/int16/
// uint16) arithmetic expression in the file, the C# type its own emission must be cast back to.
//
// Go evaluates such arithmetic at the operand's own width, so `int8(100) + 100` is -56. C# promotes
// sub-int arithmetic to `int`, so the same C# expression is 200 of type int32. Deferring the wrap is
// exact only while the consumer is wrap-invariant; everywhere else the unwrapped value is observable:
// `fmt.Println(a+a)` prints `200` and `%T` reports int32, `int(u+u)` is 400, `s[u+u]` panics, and a
// named empty interface does not even compile. A typed destination (assignment, return, parameter,
// struct field, composite element) already casts through narrowArithmeticCastTypeFor and its twins.
//
// The decision is made ONCE, here, from the consumer the expression reaches (seen through parens),
// and convBinaryExpr / convUnaryExpr emit the cast on the expression itself. A typed destination
// then sees a whole-expression cast of its own type (wholeExprIsCastOfType) and adds nothing, so its
// emission does not change; every other consumer class gets Go's width, including ones no sink-side
// arm enumerates. The cast reads as Go's own width: `fmt.Println((int8)(a + a))`, `s[(uint8)(u + u)]`.
//
// A NAMED narrow type is excluded: its [GoType] wrapper operators already cast back. So are the
// operators whose emission already narrows the whole result — `& | ^ &^`, `<<` and an unsigned
// unary `^` — and a CONSTANT expression, which cannot overflow its type in Go (and whose cast would
// be CS0221 in C#).
func (v *Visitor) markNarrowArithmeticContexts(file *ast.File) {
	var stack []ast.Node

	ast.Inspect(file, func(n ast.Node) bool {
		if n == nil {
			stack = stack[:len(stack)-1]
			return true
		}

		if expr, ok := n.(ast.Expr); ok {
			if castType, wrapRisk, ok := v.narrowArithmeticProducer(expr); ok {
				consumer := v.narrowConsumerOf(expr, stack)

				if consumer == narrowConsumerTyped || consumer == narrowConsumerValue && wrapRisk {
					// A destination of the identical Go type spells the cast its own way, which is how its
					// existing arm spells it: a []byte element fed uint8 arithmetic reads (byte)
					if destType := v.narrowDestinationType(expr, stack); destType != nil && types.Identical(destType, v.info.TypeOf(expr)) {
						castType = convertToCSTypeName(v.getAliasQualifiedTypeName(destType, false))
					}

					v.markNarrowArithmetic(expr, castType)
				}
			}
		}

		stack = append(stack, n)
		return true
	})
}

func (v *Visitor) markNarrowArithmetic(expr ast.Expr, castType string) {
	if v.narrowArithmeticCasts == nil {
		v.narrowArithmeticCasts = map[ast.Expr]string{}
	}

	v.narrowArithmeticCasts[expr] = castType
}

// narrowArithmeticProducer reports whether expr is a non-constant narrow-integer arithmetic
// expression whose C# emission is int-promoted, with the C# type that narrows it back. wrapRisk is
// true when the promoted VALUE can leave the narrow range (`+ - *`, unary `-`, a signed `/` for
// MinInt8 / -1); otherwise only the promoted TYPE differs (`>>`, `%`, an unsigned `/`, a signed
// unary `^`, unary `+`).
func (v *Visitor) narrowArithmeticProducer(expr ast.Expr) (castType string, wrapRisk bool, ok bool) {
	var unsignedXor bool

	switch e := expr.(type) {
	case *ast.BinaryExpr:
		switch e.Op {
		case token.ADD, token.SUB, token.MUL:
			wrapRisk = true
		case token.QUO, token.REM, token.SHR:
		default:
			return "", false, false
		}
	case *ast.UnaryExpr:
		switch e.Op {
		case token.SUB:
			wrapRisk = true
		case token.XOR:
			unsignedXor = true
		case token.ADD:
		default:
			return "", false, false
		}
	default:
		return "", false, false
	}

	tv, found := v.info.Types[expr]

	if !found || tv.Value != nil || tv.Type == nil {
		return "", false, false
	}

	basic, isBasic := types.Unalias(tv.Type).(*types.Basic)

	if !isBasic || !isNarrowIntegerKind(basic.Kind()) {
		return "", false, false
	}

	unsigned := basic.Info()&types.IsUnsigned != 0

	if unsignedXor && unsigned {
		// convUnaryExprCore already truncates an unsigned narrow complement: `((uint8)(~u))`
		return "", false, false
	}

	if binary, isBinary := expr.(*ast.BinaryExpr); isBinary && binary.Op == token.SHR && v.narrowShiftGuarded(binary) {
		// A guarded `>>` renders `receiver.Rsh(n)` on a receiver of the operand's own narrow type (see
		// narrowConsumerOf), so GoShift's narrow overload already returns Go's type and width
		return "", false, false
	}

	if unary, isUnary := expr.(*ast.UnaryExpr); isUnary && (unary.Op == token.ADD || unary.Op == token.XOR) {
		// A unary `+` or signed `^` leaves its operand unwrapped (the operand's consumer is invariant),
		// so an operand that can leave the range carries that risk up: `^(a + b)` is ~200 = -201 in C#
		if _, operandRisk, isProducer := v.narrowArithmeticProducer(ast.Unparen(unary.X)); isProducer && operandRisk {
			wrapRisk = true
		}
	}

	if binary, isBinary := expr.(*ast.BinaryExpr); isBinary && binary.Op == token.QUO && !unsigned {
		// int8(-128) / -1 is 128 in C#; Go wraps it to -128
		wrapRisk = true
	}

	return convertToCSTypeName(v.getAliasQualifiedTypeName(tv.Type, false)), wrapRisk, true
}

// narrowConsumerOf classifies the consumer of expr: its nearest ancestor that is not a ParenExpr.
// stack holds expr's ancestors, innermost last.
func (v *Visitor) narrowConsumerOf(expr ast.Expr, stack []ast.Node) narrowConsumer {
	var child ast.Node = expr
	i := len(stack) - 1

	for ; i >= 0; i-- {
		paren, isParen := stack[i].(*ast.ParenExpr)

		if !isParen {
			break
		}

		child = paren
	}

	if i < 0 {
		return narrowConsumerTyped
	}

	switch parent := stack[i].(type) {
	case *ast.BinaryExpr:
		switch parent.Op {
		case token.ADD, token.SUB, token.MUL, token.AND, token.OR, token.XOR, token.AND_NOT:
			return narrowConsumerInvariant
		case token.SHL, token.SHR:
			if child != parent.X {
				// A shift count
				return narrowConsumerValue
			}

			if v.narrowShiftGuarded(parent) {
				// A guarded shift renders `receiver.Lsh(n)` / `receiver.Rsh(n)`: the receiver's C# TYPE picks
				// GoShift's overload, so an int-promoted receiver would bind the int32 one and neither wrap nor
				// narrow. Cast it, and the narrow overload does both.
				return narrowConsumerTyped
			}

			if parent.Op == token.SHL {
				// A native `<<` narrows its whole result: `(int8)(x << (int)(1))`
				return narrowConsumerInvariant
			}

			return narrowConsumerValue
		case token.QUO, token.REM:
			return narrowConsumerValue
		}

		// A comparison reads the operand at its Go type: the existing comparison arm
		// (narrowComparisonOperand) already casts every direct operand, so this keeps that text
		return narrowConsumerTyped
	case *ast.UnaryExpr:
		switch parent.Op {
		case token.SUB, token.XOR, token.ADD:
			return narrowConsumerInvariant
		}
	case *ast.CallExpr:
		if child == parent.Fun {
			return narrowConsumerTyped
		}

		if tv, ok := v.info.Types[parent.Fun]; ok && tv.IsType() {
			return v.narrowConversionConsumer(tv.Type, v.info.TypeOf(expr))
		}
	case *ast.IndexExpr:
		if child == parent.Index && narrowMapType(v.info.TypeOf(parent.X)) == nil {
			return narrowConsumerValue
		}
	case *ast.SliceExpr:
		return narrowConsumerValue
	case *ast.CaseClause:
		// A case value is compared at the tag's type: through an interface tag, Go compares dynamic types
		if i >= 2 {
			if switchStmt, ok := stack[i-2].(*ast.SwitchStmt); ok && switchStmt.Tag != nil && isInterfaceType(v.info.TypeOf(switchStmt.Tag)) {
				return narrowConsumerTyped
			}
		}

		return narrowConsumerValue
	case *ast.SwitchStmt:
		// A tag compared against an interface-typed case value is compared by dynamic type
		if child == parent.Tag && v.switchHasInterfaceCase(parent) {
			return narrowConsumerTyped
		}

		return narrowConsumerValue
	case *ast.AssignStmt:
		switch parent.Tok {
		case token.ADD_ASSIGN, token.SUB_ASSIGN, token.MUL_ASSIGN, token.AND_ASSIGN, token.OR_ASSIGN, token.XOR_ASSIGN, token.AND_NOT_ASSIGN:
			// `x += e` is `x = x + e`: the operator reads the low bits and the assignment narrows
			return narrowConsumerInvariant
		case token.QUO_ASSIGN, token.REM_ASSIGN, token.SHL_ASSIGN, token.SHR_ASSIGN:
			// A divisor, or the right side of `x <<= n` / `x >>= n`, a shift count: read whole
			return narrowConsumerValue
		}
	}

	return narrowConsumerTyped
}

// narrowDestinationType returns the Go type of the typed destination expr is assigned to — an
// assignment's LHS, a declared variable, a function result, a parameter, a composite element, struct
// field or map key/value, a channel's element — or nil when its consumer is not such a destination.
// stack holds expr's ancestors, innermost last.
func (v *Visitor) narrowDestinationType(expr ast.Expr, stack []ast.Node) types.Type {
	var child ast.Node = expr
	i := len(stack) - 1

	for ; i >= 0; i-- {
		paren, isParen := stack[i].(*ast.ParenExpr)

		if !isParen {
			break
		}

		child = paren
	}

	if i < 0 {
		return nil
	}

	indexOf := func(exprs []ast.Expr) int {
		for k, e := range exprs {
			if e == child {
				return k
			}
		}

		return -1
	}

	switch parent := stack[i].(type) {
	case *ast.AssignStmt:
		if k := indexOf(parent.Rhs); k >= 0 && len(parent.Lhs) == len(parent.Rhs) {
			return v.info.TypeOf(parent.Lhs[k])
		}
	case *ast.ValueSpec:
		if k := indexOf(parent.Values); k >= 0 && k < len(parent.Names) {
			return v.info.TypeOf(parent.Names[k])
		}
	case *ast.ReturnStmt:
		k := indexOf(parent.Results)

		for j := i - 1; j >= 0 && k >= 0; j-- {
			var signature *types.Signature

			switch fn := stack[j].(type) {
			case *ast.FuncDecl:
				signature, _ = v.info.TypeOf(fn.Name).(*types.Signature)
			case *ast.FuncLit:
				signature, _ = v.info.TypeOf(fn).(*types.Signature)
			default:
				continue
			}

			if signature != nil && signature.Results().Len() == len(parent.Results) {
				return signature.Results().At(k).Type()
			}

			return nil
		}
	case *ast.CallExpr:
		k := indexOf(parent.Args)
		var signature *types.Signature

		if funType := v.info.TypeOf(parent.Fun); funType != nil {
			signature, _ = funType.Underlying().(*types.Signature)
		}

		if k < 0 || signature == nil {
			return nil
		}

		params := signature.Params()

		if signature.Variadic() && k >= params.Len()-1 {
			if parent.Ellipsis.IsValid() {
				return nil
			}

			if slice, ok := params.At(params.Len() - 1).Type().Underlying().(*types.Slice); ok {
				return slice.Elem()
			}

			return nil
		}

		if k < params.Len() {
			return params.At(k).Type()
		}
	case *ast.CompositeLit:
		return v.compositeElementType(parent, indexOf(parent.Elts), nil)
	case *ast.KeyValueExpr:
		if i == 0 {
			return nil
		}

		if composite, ok := stack[i-1].(*ast.CompositeLit); ok {
			if child == parent.Key {
				if mapType, ok := v.info.TypeOf(composite).Underlying().(*types.Map); ok {
					return mapType.Key()
				}

				return nil
			}

			return v.compositeElementType(composite, -1, parent.Key)
		}
	case *ast.SendStmt:
		if chanType, ok := v.info.TypeOf(parent.Chan).Underlying().(*types.Chan); ok {
			return chanType.Elem()
		}
	case *ast.IndexExpr:
		if mapType := narrowMapType(v.info.TypeOf(parent.X)); mapType != nil {
			return mapType.Key()
		}
	}

	return nil
}

// compositeElementType returns the type of a composite literal's element: the element type of a
// slice, array or map, or the struct field at a position (index) or named by a key.
func (v *Visitor) compositeElementType(composite *ast.CompositeLit, index int, key ast.Expr) types.Type {
	switch underlying := v.info.TypeOf(composite).Underlying().(type) {
	case *types.Slice:
		return underlying.Elem()
	case *types.Array:
		return underlying.Elem()
	case *types.Map:
		return underlying.Elem()
	case *types.Struct:
		if keyIdent, ok := key.(*ast.Ident); ok {
			for k := range underlying.NumFields() {
				if underlying.Field(k).Name() == keyIdent.Name {
					return underlying.Field(k).Type()
				}
			}

			return nil
		}

		if key == nil && index >= 0 && index < underlying.NumFields() {
			return underlying.Field(index).Type()
		}
	}

	return nil
}

// narrowShiftGuarded reports whether a shift with a narrow basic result takes golib's guarded form,
// `receiver.Lsh(n)` / `receiver.Rsh(n)` (emitGuardedShift), rather than the native C# operator.
func (v *Visitor) narrowShiftGuarded(shift *ast.BinaryExpr) bool {
	basic, width, ok := v.shiftGuardWidth(shift)

	return ok && isNarrowIntegerKind(basic.Kind()) && v.shiftCountGuarded(shift.Y, width) && !v.shiftLeftRendersAsUntypedWrapper(shift.X)
}

// switchHasInterfaceCase reports whether any case value of a tagged switch has an interface type.
func (v *Visitor) switchHasInterfaceCase(switchStmt *ast.SwitchStmt) bool {
	for _, stmt := range switchStmt.Body.List {
		if clause, ok := stmt.(*ast.CaseClause); ok {
			for _, value := range clause.List {
				if isInterfaceType(v.info.TypeOf(value)) {
					return true
				}
			}
		}
	}

	return false
}

// isInterfaceType reports whether t's underlying type is an interface (a type parameter is not).
func isInterfaceType(t types.Type) bool {
	if t == nil {
		return false
	}

	if _, isTypeParam := types.Unalias(t).(*types.TypeParam); isTypeParam {
		return false
	}

	_, ok := t.Underlying().(*types.Interface)
	return ok
}

// narrowMapType returns the map type an index expression's container resolves to, through a type
// parameter's map core (`M ~map[int8]string`), or nil when the container is not a map.
func narrowMapType(t types.Type) *types.Map {
	if t == nil {
		return nil
	}

	if typeParam, ok := types.Unalias(t).(*types.TypeParam); ok {
		return typeParamMapCore(typeParam)
	}

	mapType, _ := t.Underlying().(*types.Map)
	return mapType
}

// narrowConversionConsumer classifies a Go conversion T(expr): truncating to an integer no wider
// than the operand keeps exactly the low bits (invariant), an interface boxes the C# type (typed),
// and anything else reads the whole value (a wider integer, a float, a string from a rune).
func (v *Visitor) narrowConversionConsumer(target, operand types.Type) narrowConsumer {
	if target == nil || operand == nil {
		return narrowConsumerTyped
	}

	switch t := target.Underlying().(type) {
	case *types.Interface, *types.TypeParam:
		return narrowConsumerTyped
	case *types.Basic:
		operandBasic, ok := operand.Underlying().(*types.Basic)

		if ok && t.Info()&types.IsInteger != 0 && integerKindSize(t.Kind()) <= integerKindSize(operandBasic.Kind()) {
			return narrowConsumerInvariant
		}
	}

	return narrowConsumerValue
}

// integerKindSize is the Go size in bytes of an integer kind (the 64-bit targets go2cs emits for),
// or 0 for a non-integer kind.
func integerKindSize(kind types.BasicKind) int {
	switch kind {
	case types.Int8, types.Uint8:
		return 1
	case types.Int16, types.Uint16:
		return 2
	case types.Int32, types.Uint32:
		return 4
	case types.Int64, types.Uint64, types.Int, types.Uint, types.Uintptr:
		return 8
	}

	return 0
}

// narrowArithmeticSelfCast wraps a marked expression's rendering in its narrowing cast (see
// markNarrowArithmeticContexts), or returns it unchanged. The cast is always `(T)(…)`, even around a
// rendering that is already parenthesized (`(uint8)((u >> (int)(1)))`), because that is the exact text
// the typed-destination arms emit: a destination that sees it adds nothing and reads as it did.
func (v *Visitor) narrowArithmeticSelfCast(expr ast.Expr, rendered string) string {
	castType, ok := v.narrowArithmeticCasts[expr]

	if !ok || wholeExprIsCastOfType(rendered, castType) {
		return rendered
	}

	return "(" + castType + ")(" + rendered + ")"
}

// narrowArithmeticParenSelfCast reports whether a ParenExpr's rendering can drop its own parens
// because its inner expression rendered as its whole narrowing cast: `(a + a) / 2` then reads
// `(int8)(a + a) / 2` instead of `((int8)(a + a)) / 2`. A cast expression binds tighter than any
// binary operator. It binds LOOSER than a member access, so the one place a narrow operand becomes a
// receiver, a guarded shift's `.Rsh(n)` / `.Lsh(n)`, parenthesizes it again (emitGuardedShift).
func (v *Visitor) narrowArithmeticParenSelfCast(parenExpr *ast.ParenExpr, rendered string) bool {
	castType, ok := v.narrowArithmeticCasts[ast.Unparen(parenExpr)]
	return ok && wholeExprIsCastOfType(rendered, castType)
}

// narrowArithmeticRendersAsCast reports whether expr (seen through parentheses) narrowed itself and
// rendered as that whole cast, so a member access on it would bind inside the cast.
func (v *Visitor) narrowArithmeticRendersAsCast(expr ast.Expr, rendered string) bool {
	castType, ok := v.narrowArithmeticCasts[ast.Unparen(expr)]
	return ok && wholeExprIsCastOfType(rendered, castType)
}

// isFullyParenthesized reports whether expr is one parenthesized group spanning its whole length.
func isFullyParenthesized(expr string) bool {
	return len(expr) > 1 && expr[0] == '(' && balancedCloseIndex(expr, 0) == len(expr)-1
}
