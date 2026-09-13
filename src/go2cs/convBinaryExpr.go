// convBinaryExpr.go - Gbtc
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
	"strconv"
	"strings"
)

// isComputedConstOperand reports whether expr is a COMPUTED constant expression — one that carries a
// constant Go value but is neither a bare literal nor a named-constant reference (a binary/unary
// expression such as `(1 << k) - 1` or `^(pageSize - 1)`). Because it can involve a non-const native
// value (e.g. a `(int)` cast), it emits as a non-constant C# expression and so does not benefit from
// C#'s implicit constant conversion — under a native-int bitwise operator it must be cast explicitly.
func (v *Visitor) isComputedConstOperand(expr ast.Expr) bool {
	if tv, ok := v.info.Types[expr]; !ok || tv.Value == nil {
		return false
	}

	// A literal or a (named-)constant reference is handled by C#'s constant conversion / the
	// named-const-ref cast; only a computed expression needs this explicit cast.
	switch expr.(type) {
	case *ast.BasicLit, *ast.Ident, *ast.SelectorExpr:
		return false
	}

	return true
}

// containsUntypedNamedConstRef reports whether any subexpression is a reference to a named
// untyped numeric constant (see isUntypedNamedConstRef) — used to distinguish a computed
// constant operand that renders with a golib `Untyped*` wrapper somewhere inside
// (`4 * goarch.PtrSize`) from pure-literal constant arithmetic (`2 * 3`), which needs no cast.
func (v *Visitor) containsUntypedNamedConstRef(expr ast.Expr) bool {
	found := false

	ast.Inspect(expr, func(n ast.Node) bool {
		if found {
			return false
		}

		if e, ok := n.(ast.Expr); ok && v.isUntypedNamedConstRef(e) {
			found = true
			return false
		}

		return true
	})

	return found
}

// containsUntypedNamedIntegerConstRef reports whether any subexpression is a named untyped
// INTEGER constant that will be emitted as an UntypedInt wrapper. It is narrower than
// containsUntypedNamedConstRef because an UntypedFloat wrapper preserves fractional operands;
// UntypedInt is the wrapper whose arithmetic overload can truncate a float literal before the
// destination's float type is applied.
func (v *Visitor) containsUntypedNamedIntegerConstRef(expr ast.Expr) bool {
	found := false

	ast.Inspect(expr, func(n ast.Node) bool {
		if found {
			return false
		}

		if callExpr, ok := n.(*ast.CallExpr); ok {
			if isConversion, _ := v.isTypeConversion(callExpr); isConversion {
				return false
			}
		}

		if e, ok := n.(ast.Expr); ok && v.isUntypedNamedIntegerConstRef(e) {
			found = true
			return false
		}

		return true
	})

	return found
}

// foldedNamedFloatConstLiteral returns the single-rounded folded literal — with a `/* <gofmt> */`
// comment, like the package-level const fold in visitValueSpec — for a COMPUTED float-kind constant
// operand that references a NAMED untyped-float const (`100000 * Pi`, `1 / Ln10`), rendered at the
// resolved float width; it returns "" for anything else. The runtime C# arithmetic the converter
// would otherwise emit (a `double`/`float` cast of the operand) rounds a SECOND time and can land a
// ULP off Go's single arbitrary-precision fold — `(float64)(100000D * Pi)` = 314159.2653589793
// (−1 ULP) where Go's `float64(100000*Pi)` = 314159.26535897935. Folding at the target width matches
// Go's single round; a float32 target rounds the EXACT constant straight to float32 inside
// exactFloatText (constant.Float32Val), NEVER via a float64 intermediate — that would double-round
// differently than Go's single round-to-float32. Excluded (kept on their existing form):
//   - a bare named-const REFERENCE (`Ln10`, an ident/selector) — it already renders as a
//     single-rounded golib `UntypedFloat` wrapper cast, so folding it would only churn a readable
//     reference into a magic number.
//   - an int-valued or complex constant, and a pure-literal float const (`1.5 * 2.0`, no named ref)
//     — the latter computes exactly in C# double, so its readable operator form is kept.
//
// `targetCSType` is the resolved float C# type name ("float64"/"float32"); any other value is "".
func (v *Visitor) foldedNamedFloatConstLiteral(operand ast.Expr, targetCSType string) string {
	isFloat32 := targetCSType == "float32"

	if targetCSType != "float64" && !isFloat32 {
		return ""
	}

	tv, ok := v.info.Types[operand]

	if !ok || tv.Value == nil || tv.Value.Kind() != constant.Float {
		return ""
	}

	if !v.isComputedConstOperand(operand) || !v.containsUntypedNamedConstRef(operand) {
		return ""
	}

	suffix := "D"

	if isFloat32 {
		suffix = "F"
	}

	return fmt.Sprintf("/* %s */ %s%s", strings.TrimSpace(v.getPrintedNode(operand)), exactFloatText(tv.Value, "", isFloat32), suffix)
}

// isUntypedNamedConstRef reports whether the expression is a reference (ident or selector) to an
// untyped numeric constant — which the converter emits as a golib `Untyped*` wrapper. It is false
// for literals (those render as ordinary C# literals and follow normal numeric promotion rules).
func (v *Visitor) isUntypedNamedConstRef(expr ast.Expr) bool {
	var sel *ast.Ident

	switch e := expr.(type) {
	case *ast.Ident:
		sel = e
	case *ast.SelectorExpr:
		sel = e.Sel
	default:
		return false
	}

	if constObj, ok := v.info.ObjectOf(sel).(*types.Const); ok {
		// A TIGHTENED local const is declared at its concrete type — no wrapper, so none
		// of the wrapper-driven casts apply (see performUntypedConstAnalysis).
		if _, tightened := v.tightenedConsts[constObj]; tightened {
			return false
		}

		if basic, ok := constObj.Type().(*types.Basic); ok {
			return basic.Info()&types.IsUntyped != 0 && basic.Info()&types.IsNumeric != 0
		}
	}

	return false
}

// isUntypedNamedIntegerConstRef is the integer-kind subset of isUntypedNamedConstRef.
func (v *Visitor) isUntypedNamedIntegerConstRef(expr ast.Expr) bool {
	if !v.isUntypedNamedConstRef(expr) {
		return false
	}

	var sel *ast.Ident

	switch e := expr.(type) {
	case *ast.Ident:
		sel = e
	case *ast.SelectorExpr:
		sel = e.Sel
	default:
		return false
	}

	if constObj, ok := v.info.ObjectOf(sel).(*types.Const); ok {
		if basic, ok := constObj.Type().(*types.Basic); ok {
			return basic.Info()&types.IsInteger != 0
		}
	}

	return false
}

// isBigIntegerBackedConstRef reports whether the expression references an untyped numeric constant
// whose value does not fit int64/uint64 (or float64) and is therefore emitted as `GoBigConst`
// (= System.Numerics.BigInteger). Such a constant has no implicit operator with the built-in
// numeric types, so it must be cast even in comparisons.
func (v *Visitor) isBigIntegerBackedConstRef(expr ast.Expr) bool {
	if !v.isUntypedNamedConstRef(expr) {
		return false
	}

	var sel *ast.Ident

	switch e := expr.(type) {
	case *ast.Ident:
		sel = e
	case *ast.SelectorExpr:
		sel = e.Sel
	}

	constObj, ok := v.info.ObjectOf(sel).(*types.Const)

	if !ok {
		return false
	}

	switch constObj.Val().Kind() {
	case constant.Int:
		s := constObj.Val().ExactString()
		_, errUint := strconv.ParseUint(s, 0, 64)
		_, errInt := strconv.ParseInt(s, 0, 64)
		return errUint != nil && errInt != nil
	case constant.Float:
		_, err := strconv.ParseFloat(constObj.Val().String(), 64)
		return err != nil
	}

	return false
}

// isLargeIntLiteralOperand reports whether expr is an integer literal whose value falls OUTSIDE the
// C# int32 range. convBasicLit emits such a literal with a `(nint)`/unsigned cast (it no longer fits
// a bare `int`), which makes it a non-`int` operand — so under a native-int bitwise operator
// (`uintptrMask & 0x00ffffffffff`) it is `nuint & nint` → CS0019, like a computed-const operand. An
// in-range literal (`x & 7`) is emitted bare and C#'s constant conversion fits it, so it is excluded.
func (v *Visitor) isLargeIntLiteralOperand(expr ast.Expr) bool {
	lit, ok := expr.(*ast.BasicLit)

	if !ok || lit.Kind != token.INT {
		return false
	}

	if tv, ok := v.info.Types[expr]; ok && tv.Value != nil {
		if i, exact := constant.Int64Val(tv.Value); exact {
			return i > math.MaxInt32 || i < math.MinInt32
		}

		// Value exceeds int64 (a large unsigned literal) — also outside int32.
		return true
	}

	return false
}

// isWideShiftType reports whether a C# integer type does NOT promote to `int` when used as the
// left operand of a shift — i.e. the shift is performed in that type's own width. For these,
// shifting a bare `1` (an `int` literal) overflows before any result cast, so the operand must be
// cast instead. Narrower types (uint16/byte/…) promote to `int`, so a result cast suffices.
//
// `uintptr` belongs here even though it is a golib STRUCT rather than a C# primitive: it carries a
// native-width `nuint` and declares `operator <<(uintptr, int)` over it, so a cast operand shifts at
// 64 bits exactly as `nuint` does. Omitting it sent Go's `uintptr` — alone among the wide unsigned
// types, since `uint` renders as the primitive `nuint` — down the narrow arm and cast the RESULT,
// which is the very thing this function's own comment says does not help: `1 << (UintptrSize / 2)`
// emitted `(uintptr)(1 << (int)(32))`, and C# masks an `int` shift count to five bits, so the value
// was 1. That is runtime/internal/math's `TestMulUintptr` table (`MulUintptr(1, 1) = 1, false`), and
// the same shape sits latent in runtime/mpagealloc_64bit.go's `1 << heapAddrBits` — 1<<16 where Go
// computes 2^48, silently. The neighbouring `1<<(UintptrSize/2) - 1` was always right, which is what
// made this hard to see: there the shift is an INNER node still typed `untyped int`, so
// overflowingConstLiteral's signed arm folds it whole.
func isWideShiftType(csType string) bool {
	switch csType {
	case "uint32", "uint64", "int64", "nuint", "uintptr":
		return true
	}

	return false
}

// shiftGuardWidth reports the bit width of a shift whose LEFT operand is a BASIC integer type — the
// scope in which the converter applies its Go-semantics shift guard (GoShift.Rsh/Lsh). It returns
// (basic, width, true) for an unnamed basic-integer left operand (8/16/32/64; native int/uint/uintptr
// → 64 on this target), or (nil, 0, false) otherwise. The shift's result type equals its left operand's
// type in Go, so it is read from the whole shift node. A NAMED [GoType] wrapper left operand yields a
// *types.Named (not *types.Basic) here and is deliberately OUT of scope: those keep their existing
// generated-operator emission (the current, untouched path).
func (v *Visitor) shiftGuardWidth(binaryExpr *ast.BinaryExpr) (*types.Basic, int, bool) {
	tv, ok := v.info.Types[binaryExpr]

	if !ok || tv.Type == nil {
		return nil, 0, false
	}

	basic, ok := tv.Type.(*types.Basic)

	if !ok || basic.Info()&types.IsInteger == 0 || basic.Info()&types.IsUntyped != 0 {
		return nil, 0, false
	}

	switch basic.Kind() {
	case types.Int8, types.Uint8:
		return basic, 8, true
	case types.Int16, types.Uint16:
		return basic, 16, true
	case types.Int32, types.Uint32:
		return basic, 32, true
	case types.Int64, types.Uint64:
		return basic, 64, true
	case types.Int, types.Uint, types.Uintptr:
		return basic, 64, true
	}

	return nil, 0, false
}

// shiftLeftRendersAsUntypedWrapper reports whether the shift's left operand renders as a golib Untyped*
// wrapper — a COMPUTED constant that references a named untyped constant (`bias - 1`, emitted as
// UntypedInt arithmetic). Such an operand shifts through UntypedInt.operator<< / >> on its own 64-bit
// backing, so it is left on that existing path rather than routed to the basic-width guard (UntypedInt
// has no Rsh/Lsh overload, and its operators are the intended emission — see the UntypedIntWideShift
// behavioral test). A bare named-const reference or literal is NOT a computed operand, so it stays
// guardable: emitGuardedShift casts it to the shift's resolved basic type first.
func (v *Visitor) shiftLeftRendersAsUntypedWrapper(expr ast.Expr) bool {
	return v.isComputedConstOperand(expr) && v.containsUntypedNamedConstRef(expr)
}

// shiftCountGuarded reports whether the shift COUNT may reach or exceed `width` at runtime and so needs
// the Go-semantics guard. It returns false (→ native emission, unchanged) when the count is PROVABLY in
// [0, width): a constant in range (R1), a mask `y & M` whose constant `M` satisfies 0 <= M <= width-1
// (R2), or a modulo `y % M` whose constant `M` satisfies 1 <= M <= width (R3, bounding the count to
// [0, width-1]). A constant count >= width IS guarded (native masking would produce the wrong value); a
// NEGATIVE constant count is Go-illegal (a runtime panic) and left native — out of scope.
func (v *Visitor) shiftCountGuarded(right ast.Expr, width int) bool {
	// R1 — a constant count.
	if c, ok := v.constIntShiftValue(right); ok {
		if c < 0 {
			return false
		}

		return c >= int64(width)
	}

	// R2 / R3 — a masked or modulo'd count is bounded below the width. Unwrap parentheses so the
	// structural match sees `y & M` / `y % M` even when the source parenthesized the count.
	inner := right

	for {
		paren, ok := inner.(*ast.ParenExpr)

		if !ok {
			break
		}

		inner = paren.X
	}

	if bin, ok := inner.(*ast.BinaryExpr); ok {
		switch bin.Op {
		case token.AND:
			// EITHER operand a constant mask in [0, width-1] bounds the count.
			if m, ok := v.constIntShiftValue(bin.X); ok && m >= 0 && m <= int64(width)-1 {
				return false
			}

			if m, ok := v.constIntShiftValue(bin.Y); ok && m >= 0 && m <= int64(width)-1 {
				return false
			}
		case token.REM:
			// A modulo by a constant in [1, width] bounds the count to [0, width-1].
			if m, ok := v.constIntShiftValue(bin.Y); ok && m >= 1 && m <= int64(width) {
				return false
			}
		}
	}

	return true
}

// constIntShiftValue returns the exact int64 value of an integer-constant expression, or (0, false).
// A constant too large for int64 returns false — the caller treats an out-of-range shift count as a
// large (guarded) value, which is the correct outcome.
func (v *Visitor) constIntShiftValue(expr ast.Expr) (int64, bool) {
	tv, ok := v.info.Types[expr]

	if !ok || tv.Value == nil {
		return 0, false
	}

	val := constant.ToInt(tv.Value)

	if val.Kind() != constant.Int {
		return 0, false
	}

	return constant.Int64Val(val)
}

// emitGuardedShift renders a shift as golib's Go-semantics guard (`left.Rsh(count)` for `>>`,
// `left.Lsh(count)` for `<<`) for a count that is not provably within `basic`'s width. `leftOperand` is
// the already-converted left operand; `rawCount` is the already-converted count WITHOUT the `(int)`
// truncation the native path adds — the guard needs the count's full magnitude widened to uint64.
func (v *Visitor) emitGuardedShift(binaryExpr *ast.BinaryExpr, leftOperand, rawCount, binaryOp string, basic *types.Basic) string {
	method := "Lsh"

	if binaryOp == ">>" {
		method = "Rsh"
	}

	// LEFT operand → the method receiver. A TYPED operand already carries its width, so its natural C#
	// type selects the width-matching overload; parenthesize it only when its render is not a bare
	// primary. An UNTYPED-const (or tightened narrow-const) left operand renders as a bare C# literal
	// whose default type would bind the WRONG-width overload (`1` → int32's `Lsh`), so cast it to the
	// shift's resolved basic type first — the same width the native path's retype logic would apply.
	var receiver string

	if v.isUntypedNumericConstArg(binaryExpr.X) || v.tightenedNarrowConstRef(binaryExpr.X) {
		underlyingCS := v.getCSharpTypeName(basic)

		if v.needsParentheses(binaryExpr.X) {
			receiver = fmt.Sprintf("((%s)(%s))", underlyingCS, leftOperand)
		} else {
			receiver = fmt.Sprintf("((%s)%s)", underlyingCS, leftOperand)
		}
	} else if v.needsParentheses(binaryExpr.X) || v.shiftReceiverRendersAsCast(binaryExpr.X) {
		// A compound expression, or a Go type CONVERSION whose render is a low-precedence C# cast
		// (`uint64(1)` → `(uint64)1`), on which a trailing `.Lsh(…)` would mis-bind to the inner
		// operand — parenthesize so the method binds to the whole converted value.
		receiver = "(" + leftOperand + ")"
	} else {
		receiver = leftOperand
	}

	return fmt.Sprintf("%s.%s(%s)", receiver, method, v.shiftCountUint64Operand(binaryExpr.Y, rawCount))
}

// shiftReceiverRendersAsCast reports whether a shift's left operand renders as a leading C# cast — a Go
// type CONVERSION (`T(x)` → `(T)x`) — so it needs parenthesizing before a trailing `.Rsh`/`.Lsh` (a cast
// binds looser than member access, so `(T)x.Rsh(…)` would apply to `x`, not `(T)x`).
func (v *Visitor) shiftReceiverRendersAsCast(expr ast.Expr) bool {
	call, ok := expr.(*ast.CallExpr)
	return ok && v.callExprIsTypeConversion(call)
}

// shiftCountUint64Operand renders the shift count widened to the guard's uint64 parameter, so its FULL
// magnitude is seen before the width comparison (a computed count such as `64 - n` that unsigned-wraps
// stays huge; a narrower negative count reads as huge → 0, matching the out-of-scope negative-count
// behavior). A count already typed uint64 or native uint widens implicitly and is passed through
// unwrapped; a NAMED numeric count converts only to its own underlying basic, so it is routed through it
// (`(uint64)(nint)(sh)`) and a numeric TYPE PARAMETER through the ConvertToUInt64 bridge — both mirroring
// intCastOperand. Every other integer count takes a plain `(uint64)(…)` wrap.
func (v *Visitor) shiftCountUint64Operand(expr ast.Expr, converted string) string {
	if !v.shiftCountNeedsWiden(expr) {
		return converted
	}

	if named, ok := v.getType(expr, false).(*types.Named); ok {
		if basic, ok := named.Underlying().(*types.Basic); ok && basic.Info()&types.IsNumeric != 0 {
			return fmt.Sprintf("(uint64)(%s)(%s)", v.getCSharpTypeName(basic), converted)
		}
	}

	if tp, ok := types.Unalias(v.getType(expr, false)).(*types.TypeParam); ok && typeParamIsInteger(tp) {
		return fmt.Sprintf("ConvertToUInt64<%s>(%s)", v.getCSharpTypeName(tp), converted)
	}

	return fmt.Sprintf("(uint64)(%s)", converted)
}

// shiftCountNeedsWiden reports whether a shift count needs widening for the guard — true unless the count
// is already typed uint64 or native uint, which convert to the uint64 parameter implicitly (so their
// emitted form is passed through unwrapped).
func (v *Visitor) shiftCountNeedsWiden(expr ast.Expr) bool {
	t := v.getExprType(expr)

	if t == nil {
		return true
	}

	if basic, ok := t.Underlying().(*types.Basic); ok {
		switch basic.Kind() {
		case types.Uint64, types.Uint:
			return false
		}
	}

	return true
}

// concreteNumericCSType returns the C# name of a concrete (non-untyped) numeric type's UNDERLYING
// basic type, or "". The underlying (e.g. `uint8` for a named `Tag uint8`) is used so an
// untyped-const cast is `(uint8)c`, not `(Tag)c`: an UntypedInt wrapper converts to the basic type
// in one step, then the basic type converts to the named type implicitly — `(Tag)c` would need two
// user-defined conversions and is rejected (CS0030). Basic types are their own underlying.
func (v *Visitor) concreteNumericCSType(t types.Type) string {
	if t == nil {
		return ""
	}

	if basic, ok := t.Underlying().(*types.Basic); ok {
		if basic.Info()&types.IsNumeric != 0 && basic.Info()&types.IsUntyped == 0 {
			// A NAMED type over uintptr takes the cast to the NAMED type, not the underlying:
			// golib `uintptr` is a STRUCT with its own operators plus implicit conversions to
			// nuint, so `Errno - (uintptr)c` leaves multiple built-in candidates (CS0034 —
			// syscall Errno.Error). The named wrapper's UintptrBridgeOperators provide the
			// one-step UntypedInt conversion (`(Errno)APPLICATION_ERROR`), and the named type's
			// single predefined-type conversion (nuint) then binds unambiguously. Non-uintptr
			// named types keep the underlying cast (`(Tag)c` would be CS0030 — see above).
			if _, isNamed := t.(*types.Named); isNamed && basic.Kind() == types.Uintptr {
				return convertToCSTypeName(v.getAliasQualifiedTypeName(t, false))
			}

			return convertToCSTypeName(v.getAliasQualifiedTypeName(t.Underlying(), false))
		}
	}

	return ""
}

// overflowingConstLiteral reports the folded literal form of a SIGNED compile-time integer constant
// expression whose value falls OUTSIDE the C# int32 range, or "" otherwise. C# evaluates an operator
// expression like `1 << 63` or `12345 * 1000000000` in int32 and overflows at compile time in checked
// mode (CS0220), whereas Go evaluates the constant in its (64-bit) type; emitting the folded value
// (`9223372036854775807L`) computes it correctly. A fold whose type is Go `int` additionally carries
// its own `(nint)(…)` cast — see the typed-int arm below. Scope notes:
//   - Restricted to SIGNED targets: an unsigned constant shift already emits with a width-cast operand
//     (`(uint64)1 << 40`) from the named-numeric path, so folding it would needlessly lose the readable
//     `1<<40` form. The signed builtin-int64 path is the one that lacks the width cast and overflows.
//   - In-range constants return "" so ordinary constant arithmetic keeps its readable operator form;
//     this only fires on the overflow cases — EXCEPT when an operand has no 64-bit materialization at
//     all (see unrepresentable below), where the operator form cannot be emitted at any magnitude.
func (v *Visitor) overflowingConstLiteral(expr ast.Expr) string {
	tv, ok := v.info.Types[expr]

	if !ok || tv.Value == nil || tv.Type == nil {
		return ""
	}

	basic, ok := tv.Type.Underlying().(*types.Basic)

	if !ok || basic.Info()&types.IsInteger == 0 {
		return ""
	}

	val := constant.ToInt(tv.Value)

	if val.Kind() != constant.Int {
		return ""
	}

	// An operand with NO 64-bit materialization forces the fold whatever the whole value's
	// magnitude: unlike the overflow cases, the operator form there does not merely compute in the
	// wrong width — it THROWS at run time (see constExprHasBeyondUint64UntypedConstRef).
	unrepresentable := v.constExprHasBeyondUint64UntypedConstRef(expr)

	if basic.Info()&types.IsUnsigned != 0 {
		// UNSIGNED targets normally keep the readable operator form: a TYPED unsigned constant
		// shift emits with a width-cast operand (`(uint64)1 << 40`) from the retype path below,
		// an int64-range untyped constant subtree is folded by the SIGNED arm when recursion
		// reaches it (`(281474976710655L)` in runtime mranges), and a named-const reference
		// renders via its Untyped* wrapper. The one unfixable shape is an untyped constant
		// OPERATOR subtree whose value exceeds int64 entirely (`1 << 63` nested inside
		// `(1 << 63) - 1` — go/types lands the uint64 conversion on the outermost node, so the
		// inner shift stays untyped, no width cast reaches it, and C# computes it in int32:
		// math/rand Int63n, CS0220). Fold exactly those trees, under uint64 or a native-width
		// target (uint/uintptr → nuint).
		// `uintptr` is a golib STRUCT distinct from `nuint` (golib/uintptr.cs), so Go uintptr and
		// Go uint render under different names for the same native width; both are in scope.
		csType := v.getCSharpTypeName(basic)

		// A NARROWER unsigned target (uint8/16/32) is folded too, and for a reason the wider
		// arms never meet: the operator form's own OPERAND overflows. `1<<32 - 2` against a
		// `uint32` slot emitted `4294967296L - 2` — the literal path widens the beyond-int32
		// operand to `long` (correctly, in isolation), and C# has no implicit `long`→`uint`
		// conversion, so the assignment fails with CS0266 even though the FOLDED value fits the
		// target exactly. The second darwin census surfaced it in os/user's
		// `*_C_pw_uidp(&sp) = 1<<32 - 2` (`_C_uid_t = uint32`), which is Go's own negative-test
		// constant. Folding first and casting the RESULT is what makes it representable, in the
		// same parenthesized form the other arms use.
		// The target must be a PLAIN basic type, not a named/aliased one over the same underlying:
		// `basic` here is the UNDERLYING, so `fs.FileMode` (a named uint32) reaches this arm looking
		// exactly like `uint32`, and folding it erased the type on every mode expression in the
		// corpus (`(fs.FileMode)(fs.ModeDir | 365)` → `unchecked((uint32)(2147484013UL))`). A named
		// target already has its own arm below, which carries the type in the fold — that is where
		// it belongs, and the operand-overflow condition is not a reason to steal it.
		// An ALIAS to a basic type counts as plain: `type _C_uid_t = uint32` has no distinct C#
		// type to erase (it emits as the primitive itself), which is exactly the target shape the
		// darwin census failed on. A NAMED type does have one, and keeps its own arm.
		targetIsPlainBasic := false

		switch target := tv.Type.(type) {
		case *types.Basic:
			targetIsPlainBasic = true
		case *types.Alias:
			_, targetIsPlainBasic = target.Rhs().Underlying().(*types.Basic)
		}

		// A tree referencing NAMED constants is left alone: those render through their Untyped*
		// wrappers, which is how every wider arm already preserves them, and folding erases the
		// names the Go source is written in — math/bits' `x>>1 & (m0 & m)` became
		// `unchecked((uint32)(1431655765UL))`, arithmetic no reader can follow back to `m0`. The
		// shape that genuinely needs the fold is a LITERAL expression whose operand overflows
		// int32 (image/color's `uint32(1<<32 - 1)`), and it has no names to lose.
		if targetIsPlainBasic && !v.containsUntypedNamedConstRef(expr) &&
			(csType == "uint8" || csType == "uint16" || csType == "uint32") {
			u, exact := constant.Uint64Val(val)

			if !exact {
				return ""
			}

			// ONLY when an OPERAND overflows int32. That is what forces the literal path to widen
			// it to `long`, which is what C# then refuses to convert to a narrower unsigned target
			// (CS0266) — `1<<32 - 2` against `_C_uid_t = uint32`. Every other narrow-unsigned
			// constant keeps the readable operator form it has today, and MUST: folding them all
			// flattened 754 sites across 157 files in a three-target regen, including named-type
			// expressions whose identity is the point (os's `ModeDevice | ModeCharDevice` became a
			// bare `unchecked((uint32)(69206016UL))`, losing the FileMode). Measured, reverted, and
			// narrowed to the shape that actually fails — the coordinator's "stop rather than widen
			// silently" clause, applied to my own fix.
			if !v.constExprOperandExceeds(expr, math.MaxUint32) {
				return ""
			}

			return "unchecked((" + csType + ")(" + strconv.FormatUint(u, 10) + "UL))"
		}

		if csType != "uint64" && csType != "nuint" && csType != "uintptr" {
			return ""
		}

		if !unrepresentable && !v.constExprHasBeyondInt64UntypedOperatorSubexpr(expr) {
			return ""
		}

		u, exact := constant.Uint64Val(val)

		if !exact {
			return ""
		}

		lit := strconv.FormatUint(u, 10) + "UL"

		// `nuint` has NO implicit conversion from `ulong`, so a native-width target carries the
		// narrowing cast in the fold itself — the same rule (and the same parenthesized
		// `(nuint)(…)` form wholeExprIsCastOfType recognizes, so enclosing paths do not re-wrap)
		// as the `types.Int`/`(nint)(…)` arm below. This also covers a NAMED type over uintptr
		// with no further synthesis: `nuint` converts implicitly to its [GoType] wrapper, so
		// math/big's `nat{…, 1 + 1<<(_W-1), _M ^ (1 << (_W - 1))}` — where the untyped `1 << 63`
		// subtree exceeds int64 and C# would otherwise compute the element in int32 (CS0029 +
		// CS0019, int_test.go's TestQuoStepD6) — lands as a `Word` element of the exact value.
		// The cast is spelled `nuint` for BOTH native-width targets: it is the primitive C# native
		// unsigned type, and it converts implicitly to golib's `uintptr` struct and to a [GoType]
		// wrapper over uintptr alike, so one spelling covers uint, uintptr and named types over
		// either without the fold having to name the target.
		if csType != "uint64" {
			return uncheckedIfBeyond32Bit("nuint", lit, u > math.MaxUint32)
		}

		return lit
	}

	i, exact := constant.Int64Val(val)

	if !exact {
		return ""
	}

	if i >= math.MinInt32 && i <= math.MaxInt32 {
		// In range — keep the readable operator form, unless an operand has no 64-bit
		// materialization. The forced fold is confined to the 64-bit-wide signed targets, the only
		// ones this function's `…L` / `(nint)(…L)` contract covers; a NARROWER signed target would
		// need its own width cast, and no such site exists (a 128-bit-class untyped constant reaches
		// a signed target only through the whole-width bitmap idiom). It stays on the operator form,
		// where the wrapper cast fails LOUDLY rather than silently computing the wrong value.
		if !unrepresentable || (basic.Kind() != types.Int && basic.Kind() != types.Int64) {
			return ""
		}
	}

	lit := strconv.FormatInt(i, 10) + "L"

	// A constant TYPED Go `int` lands in a C# `nint` context, which has NO implicit conversion
	// from `long` — a bare fold fails loudly at every non-assignment use (strings' SplitN table
	// element `math.MaxInt / 4` emitted `2305843009213693951L` against the `n int` field,
	// CS1503). Carry the narrowing cast in the fold itself, in the parenthesized `(nint)(…)`
	// form nativeIntConstCastType recognizes (wholeExprIsCastOfType), so the assignment path
	// does not re-wrap and those sites stay byte-identical. The value always fits — nint is
	// 64-bit on all supported platforms — and the runtime conversion is unchecked by default.
	// An UNTYPED-int subtree keeps the bare `L` form (kind UntypedInt — its enclosing context
	// supplies the conversion), as does an int64 target (`long` is already exact).
	if basic.Kind() == types.Int {
		return uncheckedIfBeyond32Bit("nint", lit, i > math.MaxInt32 || i < math.MinInt32)
	}

	// A constant of a NAMED integer type folds to a bare basic literal, which DROPS the Go type:
	// `8 * time.Hour` became `28800000000000L`, a C# `long`, so `(8 * time.Hour).Seconds()` had no
	// receiver (CS1929) and `d := 8 * time.Hour` inferred `long` — the silent half, since a `long`
	// prints as its digit count where a Duration prints `8h0m0s`. Carry the type in the fold, in the
	// same parenthesized `(T)(…)` shape the native-int arm above uses (wholeExprIsCastOfType reads it,
	// so enclosing paths do not re-wrap). The [GoType] wrapper converts implicitly from its underlying,
	// so the cast is always legal, and the Go source's own parentheses around a method-call receiver
	// keep the postfix `.M()` binding to the cast rather than to the literal.
	if named, isNamed := tv.Type.(*types.Named); isNamed {
		return "(" + convertToCSTypeName(v.getScopeCheckedTypeName(named)) + ")(" + lit + ")"
	}

	return lit
}

// uncheckedIfBeyond32Bit renders a folded constant's narrowing cast to a native-width C# target
// (`nint`/`nuint`), adding an `unchecked` wrapper when the folded value falls OUTSIDE that
// target's 32-bit range. Only then is the conversion CS8778 ("constant value may overflow at
// runtime"): nint/nuint are 32-bit on a 32-bit platform, which go2cs does not target, so the
// value is exact and `unchecked` merely states it. The parenthesized `(T)(…)` body is preserved
// either way — wholeExprIsCastOfType peels an `unchecked(` wrapper, so the enclosing redundancy
// guards still recognize the cast and do not re-wrap it — and an in-range fold stays
// byte-identical to the pre-fix emission.
func uncheckedIfBeyond32Bit(csType, lit string, beyond32Bit bool) string {
	cast := "(" + csType + ")(" + lit + ")"

	if !beyond32Bit {
		return cast
	}

	return "unchecked(" + cast + ")"
}

// constExprHasBeyondInt64UntypedOperatorSubexpr reports whether any PROPER subexpression of the
// constant expression is an UNTYPED constant OPERATOR computation (a BinaryExpr — an inner
// `1 << 63`) whose value exceeds int64 — the one shape no other mechanism can rescue: the signed
// fold needs int64 range, the width-cast retype needs a TYPED shift, and named-const references
// (idents/selectors, any range) render via their Untyped* wrappers. C# would evaluate such a
// subtree in int32 before any enclosing cast could widen it.
func (v *Visitor) constExprHasBeyondInt64UntypedOperatorSubexpr(expr ast.Expr) bool {
	found := false

	ast.Inspect(expr, func(n ast.Node) bool {
		if found {
			return false
		}

		e, ok := n.(*ast.BinaryExpr)

		if !ok || ast.Expr(e) == expr {
			return true
		}

		tv, ok := v.info.Types[e]

		if !ok || tv.Value == nil || tv.Type == nil {
			return true
		}

		basic, ok := tv.Type.(*types.Basic)

		if !ok || basic.Info()&types.IsUntyped == 0 {
			return true
		}

		val := constant.ToInt(tv.Value)

		if val.Kind() != constant.Int {
			return true
		}

		// Fits int64 → the signed fold handles it when recursion reaches the subtree.
		if _, exact := constant.Int64Val(val); exact {
			return true
		}

		found = true
		return false
	})

	return found
}

// constExprHasBeyondUint64UntypedConstRef reports whether any PROPER subexpression of the constant
// expression REFERENCES a named untyped constant whose value fits neither int64 nor uint64 — the
// shape emitted as golib's `GoBigConst` wrapper over a System.Numerics.BigInteger (see
// isBigIntegerBackedConstRef). Such a reference has NO 64-bit materialization: every emission that
// gives it a width casts the wrapper (`(uint64)mask`, from the shift guard's receiver retype), and
// BigInteger's explicit numeric conversion THROWS System.OverflowException at run time.
//
// Go permits the 128-bit intermediate because only a constant expression's FINAL value must be
// representable in its type — the whole-width byte-classification bitmap idiom (`const mask = … ;
// (1<<c)&(mask&(1<<64-1)) | (1<<(c-64))&(mask>>64)`, go/doc/comment isHost/isPath/isName and
// net/textproto validHeaderFieldByte) is built on exactly that. Folding the enclosing expression at
// its go/types-recorded value is what reproduces it: `mask&(1<<64-1)` already folds through
// constExprHasBeyondInt64UntypedOperatorSubexpr (its `1<<64-1` operand subtree exceeds int64), while
// the sibling `mask>>64` has no such subtree — its only unrepresentable operand is the REFERENCE,
// which is what this predicate adds.
//
// The root is excluded (a bare reference is not an operator expression; there is nothing to fold, and
// Go could not have placed it in a 64-bit context in the first place).
func (v *Visitor) constExprHasBeyondUint64UntypedConstRef(expr ast.Expr) bool {
	found := false

	ast.Inspect(expr, func(n ast.Node) bool {
		if found {
			return false
		}

		e, ok := n.(ast.Expr)

		if !ok || e == expr {
			return true
		}

		if v.isBigIntegerBackedConstRef(e) {
			found = true
			return false
		}

		return true
	})

	return found
}

// bigIntegerConstMaterialization returns the emission a BigInteger-backed untyped named constant
// REFERENCE needs to stand in a CONCRETE numeric context — the rendered reference wrapped in that
// context's explicit cast — or "" when expr is not such a reference, or the context has no concrete
// numeric type, or the rendering already carries the cast.
//
// A constant too large for int64/uint64 emits as golib `GoBigConst` (= System.Numerics.BigInteger),
// which has NO implicit conversion to any built-in numeric type — a bare reference is a hard CS1503
// wherever a concrete numeric value is required. Only the COMPARISON arm of convBinaryExpr cast it
// (`x > (float64)Two129`), so every other position emitted bare: strconv's `[]ftoaTest{{below1e23,
// 'e', 17, …}}` produced twelve `BigInteger → double` errors in ftoa_test.cs alone. The cast belongs
// with the reference itself, not with each consuming construct, so all of them are served at once —
// composite-literal elements (positional, keyed, map values, array/slice), call arguments, `var`
// initializers with an explicit type, assignments, returns, send values, and index/range operands.
//
// Go's own rule is what makes the context type reliable: go/types records the CONVERTED type on the
// reference (the recorded type of `below1e23` inside `[]float64{…}` is float64, not untyped), and a
// constant is only legal in a context whose type can represent it — so a BigInteger-backed value's
// concrete context is necessarily a float/complex one, never a 64-bit integer that would overflow.
// The rendered reference is KEPT (readable, like the comparison arm) rather than folded to a
// literal; foldedNamedFloatConstLiteral makes the same call for a bare reference.
func (v *Visitor) bigIntegerConstMaterialization(expr ast.Expr, rendered string) string {
	if rendered == "" || !v.isBigIntegerBackedConstRef(expr) {
		return ""
	}

	tv, ok := v.info.Types[expr]

	if !ok {
		return ""
	}

	csType := v.concreteNumericCSType(tv.Type)

	if csType == "" || wholeExprIsCastOfType(rendered, csType) {
		return ""
	}

	return fmt.Sprintf("(%s)%s", csType, rendered)
}

// floatContextConstLiteral reports the folded literal form of a FLOAT- or complex128-typed
// compile-time constant expression built purely from integer literals whose value falls OUTSIDE the
// C# int32 range, or "" otherwise. Go evaluates such a constant in exact arithmetic and converts the RESULT to the float
// type, but the emitted C# operand form is all `int` literals, so C# evaluates the operators in
// int32 — and a float target makes the damage SILENT rather than a compile error: `var hf float64 =
// 1 << 63` emits `1 << (int)(63)`, and C# MASKS a shift count to 5 bits (63 & 31 = 31) → int.MinValue;
// `hf / (1 << 60)` divides by 2^28 (60 & 31 = 28) instead of 2^60. Emitting the Go-evaluated value as
// a float literal (`9223372036854775808D`) computes it correctly. This is the float counterpart of
// overflowingConstLiteral, kept separate because that fold's result is contracted to be 64-bit-wide
// signed emission — a `long` literal, `(nint)`-wrapped when typed Go `int` — whose non-empty result
// nativeIntConstCastType reads as proof the emitted arithmetic is 64-bit wide.
// Scope notes:
//   - ALL-INT-LITERAL operands only — that is what makes C# evaluate in int32. A float-literal
//     operand (`1e18 * 10.0`) already renders as a C# `double` computation, and a named-const operand
//     renders via its golib Untyped* wrapper; both are correct as-is and keep the readable operator form.
//   - OUTSIDE int32 only — an in-range constant (`1 << 10`) computes identically in C# int32, so it
//     keeps the readable operator form.
//   - Both the node go/types TYPED float and an inner node left `untyped float` fold; the latter is
//     what `1<<40 * 1.5` needs (see the untyped context handling below). The int-context counterpart
//     — an inner shift left `untyped int` by an int64-typed parent (`1<<40 + 7`) — is not this
//     function's business: it keeps IsInteger and overflowingConstLiteral folds it to `…L`.
// complexConstLiteral returns the EXACT folded C# text for a COMPLEX-kind constant operator
// expression — `1230000 - 0i` as `1230000D + 0D.i()` — or "" for anything that is not one.
//
// The float sibling above folds only where a named untyped const forces it, on the stated ground
// that a pure-literal float constant "computes exactly in C# double, so its readable operator form
// is kept". That ground does not hold for COMPLEX, and the counter-example is not exotic: .NET's
// mixed real/complex operators are not Go's constant arithmetic and are not even IEEE. Its
// `double - Complex` computes the imaginary part as `-right.Imaginary` rather than
// `left.Imaginary - right.Imaginary`, so `1230000D - 0D.i()` yields imaginary **-0** where Go —
// which folds the untyped expression in exact rational arithmetic, and an exact zero has no sign —
// yields +0. fmt's own TestSprintf reads the sign back out: `%#12.5g` of `1230000 - 0i` printed
// `-0.0000i` against Go's `+0.0000i`.
//
// So the rule is stated the way Go states it — a complex constant expression HAS an exact value and
// is emitted as that value — rather than as a repair of one operator. Deliberately no `/* <gofmt> */`
// annotation, unlike the float fold: `exactComplexConstString` re-renders in the same `re + imi`
// shape convBasicLit already emits, so the common case (`3 + 4i` → `3F + 4F.i()`) is byte-identical
// to the operator rendering it replaces and only the cases where the two genuinely DISAGREE move.
// A constant that will not fit its declared width keeps its operator form (the ok flag), which is
// the same fallback exactComplexConstString serves at a const declaration.
func (v *Visitor) complexConstLiteral(expr ast.Expr) string {
	tv, ok := v.info.Types[expr]

	// Kind() == Complex is exactly "this expression has an imaginary component": go/constant
	// normalizes a real-valued result to an Int/Float kind, so a real-only expression in a complex
	// context is left to the float/integer folds above and to its own operator form.
	if !ok || tv.Value == nil || tv.Type == nil || tv.Value.Kind() != constant.Complex {
		return ""
	}

	// The width follows the UNDERLYING basic type, so a named complex type folds at its own width
	// and the surrounding conversion still wraps the result (convCallExpr's named-numeric arm keys
	// on the AST, not on the rendered text).
	basic, isBasic := tv.Type.Underlying().(*types.Basic)

	if !isBasic {
		return ""
	}

	kind := basic.Kind()

	// An UNTYPED complex constant takes its width from the PROPAGATED context first and only then
	// from Go's default type. go/types resolves a constant expression's context on the OUTERMOST
	// node, so `real(2.5 - 3.5i)` assigned to a float32 leaves the argument `untyped complex` while
	// its real width is 32 — the same asymmetry markUntypedConstContexts exists to repair for the
	// float fold, and reading the default type instead would widen `real(…)`'s result to float64
	// against a float32 slot (which C# rejects outright — there is no implicit double→float).
	if basic.Info()&types.IsUntyped != 0 {
		if kind != types.UntypedComplex {
			return ""
		}

		kind = types.Complex128

		if context := v.untypedConstContext(expr); context != nil {
			switch context.Kind() {
			case types.Float32, types.Complex64:
				kind = types.Complex64
			}
		}
	}

	if kind != types.Complex64 && kind != types.Complex128 {
		return ""
	}

	text, representable := exactComplexConstString(tv.Value, kind == types.Complex64)

	if !representable {
		return ""
	}

	// A fold that swallows a NAMED constant's reference keeps it visible as a comment, exactly as
	// foldedNamedFloatConstLiteral does for the same reason: `1i * gPi` folding silently to a magic
	// number costs the reader the one identifier that explained it. A pure-literal fold needs no
	// annotation — it re-renders in the shape convBasicLit already emits, so most sites stay
	// byte-identical to the operator form they replace.
	if v.containsUntypedNamedConstRef(expr) {
		return fmt.Sprintf("/* %s */ %s", strings.TrimSpace(v.getPrintedNode(expr)), text)
	}

	return text
}

func (v *Visitor) floatContextConstLiteral(expr ast.Expr) string {
	tv, ok := v.info.Types[expr]

	if !ok || tv.Value == nil || tv.Type == nil {
		return ""
	}

	// The suffix follows the UNDERLYING basic type: a named float type takes the bare literal and
	// converts implicitly through its [GoType] underlying, exactly as convBasicLit emits `2.5D`.
	basic, ok := tv.Type.Underlying().(*types.Basic)

	if !ok {
		return ""
	}

	// An UNTYPED node takes its float-ness from the propagated context. go/types resolves a
	// constant expression's context on the OUTERMOST node only, so a shift nested in a float
	// computation is left `untyped float` — Go promotes the operands of `1<<40 * 1.5` to a common
	// kind, so the shift is no longer `untyped int` and overflowingConstLiteral's integer arm does
	// not see it either. Without the context it would emit as a bare int32 shift (256), so the
	// float multiply silently yields 0.375 instead of 1.610612736e+09. See markUntypedConstContexts.
	if basic.Info()&types.IsUntyped != 0 {
		if basic = v.untypedConstContext(expr); basic == nil {
			return ""
		}
	}

	var suffix string

	// complex128 is float64-backed (System.Numerics.Complex), so an int-literal shift in a
	// complex128 context (`[]complex128{1 << 35}`) carries the same silent int32-masking hazard and
	// folds through its real part below. complex64 is deliberately excluded: its float32 parts would
	// overflow to a C# compile error for the beyond-float32 magnitudes this fold targets, and such
	// constants do not arise in practice.
	switch basic.Kind() {
	case types.Float32:
		suffix = "F"
	case types.Float64, types.Complex128:
		suffix = "D"
	default:
		return ""
	}

	if !constExprIsIntLiteralArithmetic(expr) {
		return ""
	}

	// An all-int-literal tree is exact integer arithmetic in Go (an untyped-int `/` is integer
	// division), so the value always converts back to exact digits — which read far closer to the
	// Go source than a float rendering would. A complex128 value carries the integer in its real
	// part (its imaginary part is 0 — an imaginary literal is not int-literal arithmetic), so take
	// that component before converting.
	value := tv.Value
	if basic.Info()&types.IsComplex != 0 {
		value = constant.Real(value)
	}
	val := constant.ToInt(value)

	if val.Kind() != constant.Int {
		return ""
	}

	if i, exact := constant.Int64Val(val); exact && i >= math.MinInt32 && i <= math.MaxInt32 {
		return ""
	}

	return val.ExactString() + suffix
}

// exprIsSlicedStringLiteral reports whether expr is a SLICE of a string literal (`"abc"[:n]`,
// possibly parenthesized at either level) — the shape that renders as `"abc"u8[..n]`, a bare
// `ReadOnlySpan<byte>` rather than the UTF-8 literal C#'s span concatenation requires.
func exprIsSlicedStringLiteral(expr ast.Expr) bool {
	for {
		paren, isParen := expr.(*ast.ParenExpr)

		if !isParen {
			break
		}

		expr = paren.X
	}

	sliceExpr, isSlice := expr.(*ast.SliceExpr)

	if !isSlice {
		return false
	}

	base := sliceExpr.X

	for {
		paren, isParen := base.(*ast.ParenExpr)

		if !isParen {
			break
		}

		base = paren.X
	}

	lit, isLit := base.(*ast.BasicLit)

	return isLit && lit.Kind == token.STRING
}

// constExprIsIntLiteralArithmetic reports whether expr is built exclusively from INTEGER literals
// combined by paren/unary/binary operators — the exact shape the converter renders as an all-`int`
// C# operator expression, which C# therefore evaluates in int32. Any other leaf carries its own
// width and is deliberately excluded: a float/imaginary literal makes C# compute in `double`, a
// named-const reference emits as a golib Untyped* wrapper, and a conversion emits concretely typed.
func constExprIsIntLiteralArithmetic(expr ast.Expr) bool {
	switch e := expr.(type) {
	case *ast.ParenExpr:
		return constExprIsIntLiteralArithmetic(e.X)
	case *ast.UnaryExpr:
		return constExprIsIntLiteralArithmetic(e.X)
	case *ast.BinaryExpr:
		return constExprIsIntLiteralArithmetic(e.X) && constExprIsIntLiteralArithmetic(e.Y)
	case *ast.BasicLit:
		return e.Kind == token.INT
	}

	return false
}

func (v *Visitor) convBinaryExpr(binaryExpr *ast.BinaryExpr, context PatternMatchExprContext, litContext BasicLitContext) string {
	// A TYPED float constant expression containing a named untyped constant must materialize
	// the whole expression at the resolved width. C# overload resolution otherwise prefers the
	// Untyped* wrapper's arithmetic when both operands are still constants: `.5 * REP`, with
	// REP emitted as UntypedInt, converts .5 TO UntypedInt and truncates it to zero before the
	// surrounding float64 slot can observe the result. Folding the exact Go constant once at
	// the target width also avoids the double-rounding hazard documented by the shared helper.
	if v.containsUntypedNamedIntegerConstRef(binaryExpr) {
		if tv, ok := v.info.Types[binaryExpr]; ok {
			if targetCSType := v.concreteNumericCSType(tv.Type); targetCSType != "" {
				if lit := v.foldedNamedFloatConstLiteral(binaryExpr, targetCSType); lit != "" {
					return lit
				}
			}
		}
	}

	// A COMPLEX constant operator expression folds to its exact value, because C# cannot reproduce
	// Go's constant arithmetic for one — see complexConstLiteral.
	if lit := v.complexConstLiteral(binaryExpr); lit != "" {
		return lit
	}

	// A constant operator expression whose value overflows int32 must emit as the folded 64-bit
	// literal — C# would compute the operators in int32 and overflow at compile time (CS0220).
	if lit := v.overflowingConstLiteral(binaryExpr); lit != "" {
		return lit
	}

	// The same in a FLOAT context, where the fold above does not apply (the constant's type is not
	// an integer) but C# still evaluates the int-literal operands in int32 — silently, not loudly.
	if lit := v.floatContextConstLiteral(binaryExpr); lit != "" {
		return lit
	}

	core := v.convBinaryExprCore(binaryExpr, context, litContext)

	// A TYPED-integer constant expression whose VALUE fits its target type but whose emitted
	// arithmetic an operand fold has widened to `long` carries its own narrowing cast — see
	// widenedConstExprCastType. The BITWISE path already returns its result wrapped in exactly
	// this cast, so skip when the whole rendering is one (the shared wholeExprIsCastOfType test).
	if castType := v.widenedConstExprCastType(binaryExpr); castType != "" && !wholeExprIsCastOfType(core, castType) {
		return "(" + castType + ")(" + core + ")"
	}

	return core
}

// widenedConstExprCastType returns the C# type a TYPED-integer constant operator expression must be
// cast to because its emitted arithmetic renders as 64-bit `long` — or "" when none is needed. Go
// evaluates a constant expression in arbitrary precision and requires only the FINAL value to be
// representable in the target type, so `[]int32{1<<31 - 2}` is legal Go even though the inner shift
// (2147483648) overflows int32. That inner shift is UNTYPED and out of int32 range, so
// overflowingConstLiteral folds it to a bare `2147483648L`, while the whole expression's value
// (2147483646) IS in range and therefore keeps its operator form — leaving a `long`-typed rendering
// against a narrower target, which C# rejects (CS0266 at a composite element or assignment, CS1503
// at an argument). Casting the whole rendering narrows it back exactly once, mirroring Go's
// evaluate-wide-then-represent rule; the value provably fits, since go/types already typed it.
//
// Applies to every integer target EXCEPT int64, whose C# `long` already is the widened width. Two
// scope restrictions carry over from the sibling assignment-path mechanism (nativeIntConstCastType,
// which keeps handling the out-of-int32-range values overflowingConstLiteral folds whole — the cast
// string matches, so wholeExprIsCastOfType stops it re-wrapping these):
//   - CRUX: at least one OPERAND must itself fold to a `long` literal. A bare `1 << 40` (both
//     operands small) emits as a 32-bit `1 << (int)(40)` whose count C# MASKS to 8, truncating the
//     value; casting that would turn a loud error into a silent wrong value. Requiring a folded
//     operand also guarantees the non-folded operands compute exactly — a shift is only left
//     unfolded when its value fits int32, which bounds its count below 32.
//   - The whole value must be int64-exact. Beyond that, an operand can exceed int64 and mis-emit as
//     a truncating shift (a `uintptr` past int64 range), which must keep its visible error.
//
// PLAIN basic types only (not `.Underlying()`): a named type's [GoType] cast rejects a `long`
// operand (CS0030), and an UNTYPED node is left to its enclosing context.
//
// BINARY and UNARY roots both qualify. go/types types only the ROOT of a constant operator
// expression and leaves its operands untyped (updateExprType stops descending once it finds a
// constant), so a NEGATED widened constant — `[]int32{-(1<<31 - 1)}`, strconv's `atoi_test`
// parseInt32 table — records `untyped int` on the inner `1<<31 - 1` and `int32` only on the
// negation: there is no typed binary anywhere to hang the cast on, and it can only be placed at
// the unary root (CS1503 without it).
func (v *Visitor) widenedConstExprCastType(expr ast.Expr) string {
	// The unary operators that can appear in a constant integer expression (`-`, `+`, `^`) all
	// render as C# arithmetic over the operand, so they widen exactly as a binary root does. The
	// non-constant ones (`&x`, `<-ch`, `!b`) are rejected by the value/kind guards below.
	var operandX, operandY ast.Expr

	switch e := expr.(type) {
	case *ast.BinaryExpr:
		operandX, operandY = e.X, e.Y
	case *ast.UnaryExpr:
		operandX = e.X
	default:
		return ""
	}

	tv, ok := v.info.Types[expr]

	if !ok || tv.Value == nil || tv.Type == nil {
		return ""
	}

	basic, ok := tv.Type.(*types.Basic)

	if !ok || basic.Info()&types.IsInteger == 0 || basic.Info()&types.IsUntyped != 0 || basic.Kind() == types.Int64 {
		return ""
	}

	if _, exact := constant.Int64Val(constant.ToInt(tv.Value)); !exact {
		return ""
	}

	widened := v.operandRendersWidenedFold(operandX)

	if !widened && operandY != nil {
		widened = v.operandRendersWidenedFold(operandY)
	}

	if !widened {
		return ""
	}

	return convertToCSTypeName(v.getAliasQualifiedTypeName(basic, false))
}

// operandRendersWidenedFold reports whether an operand's emission is a folded 64-bit `long`
// literal. Only an OPERATOR subtree is rewritten by overflowingConstLiteral (via convBinaryExpr) —
// a named untyped-const REFERENCE of the same value renders as its golib `Untyped*` wrapper, whose
// implicit conversions already narrow at the use site (wrapping those would churn green
// emissions) — so the check strips paren/unary wrapping and requires a BinaryExpr.
//
// The fold need not be a DIRECT operand: the operator form is emitted over the operand renderings,
// so a subtree that does not fold itself still renders `long` when one of ITS operands does —
// `[]int32{(1<<31 - 1) - 1}` folds only the grandchild shift. The walk therefore descends the whole
// constant subtree. Descent never over-reports: go/types leaves the operands of a constant operator
// expression UNTYPED, so no interior node ever carries its own narrowing cast (which would put its
// rendering back at the narrow width) — the guard test pins the single-cast emission.
func (v *Visitor) operandRendersWidenedFold(expr ast.Expr) bool {
	switch e := expr.(type) {
	case *ast.ParenExpr:
		return v.operandRendersWidenedFold(e.X)
	case *ast.UnaryExpr:
		return v.operandRendersWidenedFold(e.X)
	case *ast.BinaryExpr:
		return v.overflowingConstLiteral(e) != "" ||
			v.operandRendersWidenedFold(e.X) || v.operandRendersWidenedFold(e.Y)
	}

	return false
}

func (v *Visitor) convBinaryExprCore(binaryExpr *ast.BinaryExpr, context PatternMatchExprContext, litContext BasicLitContext) string {
	lhsType := v.getExprType(binaryExpr.X)
	rhsType := v.getExprType(binaryExpr.Y)

	identContext := DefaultIdentContext()
	basicLitContext := DefaultBasicLitContext()
	// Honor an INCOMING u8 suppression (an object/interface vararg argument context, e.g. print's
	// `object[]`) for a string CONCAT only: a literal-concat argument — print's newline+tab join
	// in runtime stack.go's newstack diagnostics — otherwise rendered BOTH halves as `"…"u8`
	// spans; a ReadOnlySpan<byte> cannot box to object (CS1503), and span + span has no
	// operator. With suppression the operands render as plain C# strings, whose `+` and boxing
	// are fine. Gated to token.ADD: comparisons and other binary kinds under a suppressed
	// context (case-when clauses, equality inside varargs — `major == "0"u8`) keep their own
	// operand rendering, which already binds golib's span-aware operators (a first-cut
	// suppression of ALL kinds churned 212 stdlib files and risked @string↔string operator
	// re-binding). The default context has spanTargetUnsupported=false, so every other path is
	// unchanged. The signal is DEDICATED (see BasicLitContext.spanTargetUnsupported) rather than
	// derived from `!u8StringOK`: a span-hostile slot may still render a STANDALONE literal in the
	// combined `(@string)"…"u8` form it accepts, and the old coupling turned that rendering choice
	// into a concat compile break.
	concatSuppressed := litContext.spanTargetUnsupported && binaryExpr.Op == token.ADD
	basicLitContext.u8StringOK = !concatSuppressed && v.isStringType(binaryExpr.X) && v.isStringType(binaryExpr.Y)

	// Propagate to the OPERAND context so a nested concat is suppressed exactly where the old
	// coupling suppressed it. That coupling read `!litContext.u8StringOK`, so the signal a child
	// saw was ALWAYS its parent's freshly-computed operand u8 decision — never the grandparent's.
	// Two shapes depend on both halves of that:
	//   - u8 goes off merely because this node is not a string-string operator, and the child IS a
	//     concat: `FullPath(d + "\\" + p[2:])` (outer `+` has a sub-slice operand) rendered its
	//     inner `d + "\\"` literal plain — so the signal must be SET here.
	//   - a non-ADD node RE-ESTABLISHES u8 under a suppressed parent: `lastErr == nil || fqdn ==
	//     name + "."` (the `||`'s operands are bool, the `==`'s are strings) kept `"."u8` — so the
	//     signal must NOT be inherited past a node that turned u8 back on.
	// Assigning the negated operand decision reproduces both, which is what makes the split a pure
	// refactor; the flips below are the only intended emission change.
	basicLitContext.spanTargetUnsupported = !basicLitContext.u8StringOK

	// A comparison against a stack string (sstring) KEEPS the `"…"u8` span form for the literal operand:
	// sstring has zero-allocation comparison operators against ReadOnlySpan<byte>, so `s == "x"u8`
	// binds them and compares the backing spans in place — no per-comparison allocation. (Rendering the
	// literal as a plain C# string would force a `UTF8.GetBytes` conversion to sstring on every compare.)
	// The default u8StringOK above already leaves it as u8, so no adjustment is needed here.

	// In a `==`/`!=` comparison, a deref'd pointer PARAMETER (or the deref'd pointer RECEIVER) must
	// compare its box `Ꮡp`, not its value alias `p` (a struct value). Each operand's pointer context
	// is otherwise taken from the OTHER operand's pointer-ness, so `p != nil` (nil is not a pointer
	// type) would convert p in value form — the wrong comparison, and for a promoted-embed struct
	// `p == nil` then binds the generated `T.operator==(T, NilType)`, a null-embed-box NRE (os.File's
	// `func (f *File) checkValid() { if f == nil … }`). Forcing the box form here is safe only for a
	// deref-aliased pointer param/receiver: a pointer LOCAL is already the box, and forcing isPointer
	// there would emit a non-existent `Ꮡlocal` (see convIdent).
	isEqualityComparison := binaryExpr.Op == token.EQL || binaryExpr.Op == token.NEQ
	leftIsDerefdPtrParam := false
	rightIsDerefdPtrParam := false

	if isEqualityComparison {
		if ident, ok := binaryExpr.X.(*ast.Ident); ok {
			leftIsDerefdPtrParam = v.isDerefdPointerParamIdent(ident) || v.isDerefdPointerReceiverIdent(ident)
		}

		if ident, ok := binaryExpr.Y.(*ast.Ident); ok {
			rightIsDerefdPtrParam = v.isDerefdPointerParamIdent(ident) || v.isDerefdPointerReceiverIdent(ident)
		}
	}

	// A pointer comparand puts the OTHER operand in pointer (box) context — but not when that
	// operand is itself an INTERFACE: `v == dequeueNil(nil)` (sync/poolqueue; v is `any`, the
	// comparand a defined pointer type) must compare v's held VALUE, and the box form emitted a
	// nonexistent `Ꮡv` (CS0103).
	lhsIsInterfaceType, _ := isInterface(lhsType)
	rhsIsInterfaceType, _ := isInterface(rhsType)

	// An ERASED pointer-core type parameter ([P *T]) reads as an interface (its underlying is
	// the constraint) but IS a pointer (ж<T>): without this the interface exclusion below
	// suppressed the box form for `p == nil`, comparing the deref'd value alias against
	// `default!` (CS0019/CS8761 on an unconstrained T).
	if v.typeIsErasedPointerCore(lhsType) {
		lhsIsInterfaceType = false
	}

	if v.typeIsErasedPointerCore(rhsType) {
		rhsIsInterfaceType = false
	}

	// A CONSTRAINED type parameter compared against nil has NO operator to bind (CS8761): C# forbids
	// a user-defined operator on a type parameter, so the `x == default!` form that every CONCRETE
	// site uses — correct there, because those are structs that DEFINE `operator ==` — does not
	// compile when the operand is the parameter. The family interface's IsNil property carries the
	// exact check, and the constraint already names that interface (`where M : IMap<K,V>` /
	// `where S : ISlice<E>`), so this is a CONSTRAINED call: no cast and no boxing.
	//
	// MAPS came first (`m == nil` — maps.Clone's nil-preserve guard; IMap.IsNil is backing-store
	// null, an allocated empty map is NOT nil). SLICES joined at go1.24, whose slices.Clone gained
	// `if s == nil { return nil }` where 1.23 had `return append(s[:0:0], s...)` and never compared.
	// The gate is WIDENED here rather than copied into a second arm: one rule that answers for both
	// families cannot drift the way two rules can, which is exactly how the sibling literal branches
	// of defect D came to disagree.
	//
	// CHANNELS are deliberately NOT included: IChannel has no IsNil member today, and the corpus
	// carries exactly ONE chan-constrained generic declaration in either release. Adding the member
	// without a reaching case would be speculative; this arm gains `|| typeParamChanCore(tp) != nil`
	// the day one arrives, and until then a chan type parameter keeps the operator form it has.
	if isEqualityComparison {
		nilCompareOperand := func(operand ast.Expr, other ast.Expr) (string, bool) {
			if ident, ok := other.(*ast.Ident); !ok || ident.Name != "nil" {
				return "", false
			}

			if tp, ok := types.Unalias(v.info.TypeOf(operand)).(*types.TypeParam); ok && (typeParamMapCore(tp) != nil || typeParamSliceCore(tp) != nil) {
				return v.convExpr(operand, nil), true
			}

			return "", false
		}

		operandExpr, isMapNilCompare := nilCompareOperand(binaryExpr.X, binaryExpr.Y)

		if !isMapNilCompare {
			operandExpr, isMapNilCompare = nilCompareOperand(binaryExpr.Y, binaryExpr.X)
		}

		if isMapNilCompare {
			if binaryExpr.Op == token.NEQ {
				return fmt.Sprintf("!%s.IsNil", operandExpr)
			}

			return fmt.Sprintf("%s.IsNil", operandExpr)
		}
	}

	// An `unsafe.Pointer` compared to another `unsafe.Pointer` renders as its BOX, not as its
	// address. Go compares unsafe.Pointer by IDENTITY, and golib's `Pointer : ж<uintptr>` already
	// answers exactly that: its overridden Equals compares PointerOrderToken, which the base
	// `==`/`!=` operators route through and which is nil-safe by construction (`IsNull ? 0`).
	// convIdent's pointer-context arm instead emits `x.Value`, the raw uintptr — right by accident
	// for two non-nil pointers, and a NullReferenceException on a nil one, because a nil
	// unsafe.Pointer local is `default!`, a C# null reference. sync/atomic's TestLoadPointer /
	// TestStorePointer / TestSwapPointer each walk `testPointers()`, whose first element is nil,
	// and died there. Suppressing the pointer context is the whole fix: an unsafe.Pointer ident
	// then renders bare and the operator binds to the box.
	//
	// Scoped to equality with unsafe.Pointer on BOTH sides. Go admits no other pairing without a
	// conversion — a mixed `unsafe.Pointer == *T` or `== uintptr` is a type error — except against
	// untyped nil, which the dedicated nil arms above already handle and which never reaches here.
	unsafePointerIdentityCompare := (binaryExpr.Op == token.EQL || binaryExpr.Op == token.NEQ) &&
		isUnsafePointer(lhsType) && isUnsafePointer(rhsType)

	// An erased pointer-core operand counts as a pointer comparand too (a P-typed LOCAL holds
	// the box directly, so the other operand takes the box form exactly as against a `*T`).
	rhsIsPointer := isPointer(rhsType) || v.typeIsErasedPointerCore(rhsType)
	identContext.isPointer = (rhsIsPointer || leftIsDerefdPtrParam) && !lhsIsInterfaceType && !unsafePointerIdentityCompare
	leftOperand := v.convExpr(binaryExpr.X, []ExprContext{identContext, basicLitContext})

	binaryOp := binaryExpr.Op.String()

	lhsIsPointer := isPointer(lhsType) || v.typeIsErasedPointerCore(lhsType)
	identContext.isPointer = (lhsIsPointer || rightIsDerefdPtrParam) && !rhsIsInterfaceType && !unsafePointerIdentityCompare
	rightOperand := v.convExpr(binaryExpr.Y, []ExprContext{identContext, basicLitContext})

	// A binary op between an integer-constrained TYPE PARAMETER and an untyped CONSTANT —
	// `n <= 0` in rand.N[Int intType] — leaves the constant as C# `int`, which the lifted
	// IComparisonOperators<Int, Int, bool> does not accept (CS0019). Go types the constant AS
	// the type parameter; materialize it via golib ConvertToType<Int>(…).
	wrapTypeParamConst := func(operandType types.Type, other ast.Expr, otherOperand string) string {
		tp, ok := types.Unalias(operandType).(*types.TypeParam)

		if !ok || !typeParamIsInteger(tp) {
			return otherOperand
		}

		if tv, ok := v.info.Types[other]; ok && tv.Value != nil {
			if otherType := v.info.TypeOf(other); otherType != nil {
				if basic, isBasic := otherType.(*types.Basic); types.Identical(otherType, tp) || (isBasic && basic.Info()&types.IsUntyped != 0) {
					return fmt.Sprintf("ConvertToType<%s>(%s)", v.getCSharpTypeName(tp), otherOperand)
				}
			}
		}

		return otherOperand
	}

	// A SHIFT count is typed independently in Go (never as the left operand's type parameter),
	// and the emission coerces it to int — wrapping it would cast ConvertToType<E>(1) to int
	// (CS0030). Only non-shift operators take the const materialization.
	if binaryExpr.Op != token.SHL && binaryExpr.Op != token.SHR {
		rightOperand = wrapTypeParamConst(lhsType, binaryExpr.Y, rightOperand)
	}

	leftOperand = wrapTypeParamConst(rhsType, binaryExpr.X, leftOperand)

	// A SLICED string literal renders as a bare `ReadOnlySpan<byte>` (`"012345678"u8[..n]`), and C#'s
	// `+` over UTF-8 spans is a LITERAL-only feature — a slice of a literal is not one, so
	// `"012345678"[:n] + "000000000"[:9-n]` (time's format_test.go) had no operator at all (CS9047).
	// Give the concat one `@string` operand and golib's `operator +(@string, ReadOnlySpan<byte>)` (and
	// its mirror) binds, leaving the OTHER half a span. The literal deliberately keeps its `u8` form:
	// Go slices a string by BYTES, so re-rendering it as a C# `string` and slicing that would index by
	// UTF-16 code units — right for ASCII, silently wrong for anything else. Only token.ADD is
	// affected; a comparison against a sliced literal already binds golib's span-aware operators.
	if binaryExpr.Op == token.ADD {
		if exprIsSlicedStringLiteral(binaryExpr.X) {
			leftOperand = fmt.Sprintf("((@string)%s)", leftOperand)
		} else if exprIsSlicedStringLiteral(binaryExpr.Y) {
			rightOperand = fmt.Sprintf("((@string)%s)", rightOperand)
		}
	}

	if !context.usePattenMatch {
		// Check for comparisons between interface and pointer types. Go compares an interface
		// against a pointer by POINTER IDENTITY (the interface holds the *T); the interface value
		// is a generated IжAdapter wrapping the receiver box, and AreEqual unwraps adapters to
		// compare box identity. The old deref form (`iface == ~p`) boxed a COPY of the pointed-to
		// value — wrong identity semantics, and CS0019 once the adapter replaced the partial-struct
		// comparison operators (InterfaceImplementation's `zoo[0] == f`).
		if binaryOp == "==" || binaryOp == "!=" {
			lhsIsInterface, isEmpty := isInterface(lhsType)
			rhsIsInterface, rhsIsEmpty := isInterface(rhsType)

			// A CONCRETE comparable value against a non-empty interface (`err ==
			// ERROR_ENVVAR_NOT_FOUND`, error vs the Errno named numeric; syscall CS0019 x12)
			// also routes through AreEqual: Go compares the interface's dynamic type+value,
			// which AreEqual's boxed same-type value compare reproduces - no operator between
			// the interface and the implementing struct exists (user-defined operators must be
			// public, which an internal-scoped pair cannot declare). Excludes nil (the
			// dedicated nil arms above), pointers (box identity below), and interfaces.
			concreteOperand := func(t types.Type) bool {
				if t == nil || isPointer(t) {
					return false
				}

				if basic, ok := t.Underlying().(*types.Basic); ok && basic.Kind() == types.UntypedNil {
					return false
				}

				iface, _ := isInterface(t)
				return !iface
			}

			if (lhsIsInterface && !isEmpty && rhsIsPointer) || (rhsIsInterface && !rhsIsEmpty && lhsIsPointer) || (lhsIsInterface && rhsIsInterface) ||
				(lhsIsInterface && concreteOperand(rhsType)) || (rhsIsInterface && concreteOperand(lhsType)) {
				// The CONCRETE operand of an interface `==`/`!=` compares by BOX: AreEqual reflects on
				// the boxed values' runtime types (an early `leftType != right.GetType()` bail), so an
				// untyped constant that rendered as a bare C# `int` literal (System.Int32) fails
				// against the interface's boxed Go `int` (go2cs `nint`, a System.IntPtr) —
				// `e.Value != 1` wrongly reports unequal (container/list TestIssue6349) — and a named
				// untyped const rendered as its golib wrapper STRUCT matches no Go type at all. Cast
				// the constant to Go's DEFAULT TYPE for its kind so it boxes as Go's dynamic type,
				// mirroring the castArgToType treatment the interface-argument path already applies at
				// call sites (see untypedConstBoxCast) — the helper yields "" for the interface operand
				// and for any non-untyped-constant operand, so a bare int compared against a concrete
				// int (which never routes through AreEqual) stays untouched.
				leftOperand = v.applyUntypedConstBoxCast(binaryExpr.X, leftOperand)
				rightOperand = v.applyUntypedConstBoxCast(binaryExpr.Y, rightOperand)

				// Handle interface comparison with special runtime function
				if binaryOp == "==" {
					return fmt.Sprintf("AreEqual(%s, %s)", leftOperand, rightOperand)
				}

				return fmt.Sprintf("!AreEqual(%s, %s)", leftOperand, rightOperand)
			}
		} else if binaryOp == "<<" || binaryOp == ">>" {
			// Go zeroes (or, for a signed right shift, sign-extends) a shift whose count reaches or
			// exceeds the operand's width, but C#'s native shift MASKS the count (`n & 63`/`n & 31`,
			// and sub-int operands promote to `int` → `& 31`). So when the count is NOT provably in
			// [0, width) the shift must route through golib's Go-semantics guard (GoShift.Rsh/Lsh)
			// rather than the native operator. In scope only for a basic-integer left operand (a
			// NAMED [GoType] wrapper keeps its existing generated-operator emission); a constant,
			// masked, or modulo'd count that is provably bounded stays native (unchanged, ~zero churn).
			if basic, width, inScope := v.shiftGuardWidth(binaryExpr); inScope && v.shiftCountGuarded(binaryExpr.Y, width) && !v.shiftLeftRendersAsUntypedWrapper(binaryExpr.X) {
				return v.emitGuardedShift(binaryExpr, leftOperand, rightOperand, binaryOp, basic)
			}

			rightOperand = v.intCastOperand(binaryExpr.Y, rightOperand)

			// Go's shift operators bind at the multiplicative level (tighter than `+`/`-`),
			// but C#'s `<<`/`>>` bind looser than the additive operators. So Go's
			// `x>>4 + x` (meaning `(x>>4) + x`) would re-associate in C# as `x >> (4 + x)`.
			// Parenthesize the shift so the original grouping is preserved in every context.
			shiftExpr := fmt.Sprintf("(%s %s %s)", leftOperand, binaryOp, rightOperand)

			// When the shifted (left) operand is an untyped constant — `1 << k` — Go gives the
			// whole shift the type it assumes from context (e.g. uintptr when compared with a
			// uintptr), but the bare C# literal makes the result `int`, which then can't compare
			// or combine with the typed operand (CS0034). Re-type the shift to its resolved type.
			// A TIGHTENED const left operand (declared at the shift's own concrete type) normally
			// makes the retype redundant — EXCEPT for sub-int32 shift types, where the cast is
			// VALUE-CHANGING: C# promotes a narrow shifted operand to `int`, so without the
			// `(uint8)(…)` result cast Go's wraparound at the declared width is lost
			// (`const cb = 200; b + cb<<k` → Go 145, promoted C# 401). Keep the retype there.
			if v.isUntypedNumericConstArg(binaryExpr.X) || v.tightenedNarrowConstRef(binaryExpr.X) {
				if shiftType := v.info.Types[binaryExpr].Type; shiftType != nil {
					if basic, ok := shiftType.Underlying().(*types.Basic); ok && basic.Info()&types.IsInteger != 0 && basic.Info()&types.IsUntyped == 0 {
						// Shift width is decided by the UNDERLYING basic type. For a NAMED numeric
						// (`type arenaIdx uint`), the result is additionally wrapped to the named type
						// *through* its underlying — `(arenaIdx)((nuint)1 << k)` — because the
						// `[GoType]` conversion only accepts its exact underlying, never C# `int`
						// (a bare `(arenaIdx)(1 << k)` is CS0030).
						underlyingCS := v.getCSharpTypeName(basic)
						resolvedCS := convertToCSTypeName(v.getAliasQualifiedTypeName(shiftType, false))
						isNamed := resolvedCS != underlyingCS

						if isWideShiftType(underlyingCS) {
							// A type that does not promote to `int` in a C# shift (uint/ulong/long/
							// nuint). Cast the LEFT operand so the shift is performed in that type —
							// casting the result would not help, as `1 << 63` already overflowed
							// `int` to a negative value before the cast (e.g. `(uint64)(1 << 63)`
							// → CS0221). `((uint64)1) << 63` shifts in uint64.
							// A LEFT operand that leads with a unary +/- (`-1 << bits`, archive/tar,
							// debug/dwarf) must be parenthesized: `(int64)-1` is parsed as the type
							// `int64` MINUS `1` (CS0119), not a cast, because the width type is a
							// using-ALIAS (int64=long, …) not a C# keyword. `(int64)(-1)` is unambiguous.
							castLeftOperand := leftOperand

							if strings.HasPrefix(leftOperand, "-") || strings.HasPrefix(leftOperand, "+") {
								castLeftOperand = "(" + leftOperand + ")"
							}

							shiftExpr = fmt.Sprintf("((%s)%s %s %s)", underlyingCS, castLeftOperand, binaryOp, rightOperand)

							if isNamed {
								shiftExpr = fmt.Sprintf("(%s)%s", resolvedCS, shiftExpr)
							}
						} else if underlyingCS != "int" && underlyingCS != "nint" {
							// A narrow type (uint16/byte/…) promotes to `int` in the shift, so the
							// computation is correct; just re-type the result (through the underlying
							// for a named type).
							shiftExpr = fmt.Sprintf("(%s)%s", underlyingCS, shiftExpr)

							if isNamed {
								shiftExpr = fmt.Sprintf("(%s)%s", resolvedCS, shiftExpr)
							}
						} else if isNamed {
							// Underlying is `int`/`nint`; a named type over it still needs the cast
							// routed through the underlying — `(arenaIdx)(nint)(1 << k)`.
							shiftExpr = fmt.Sprintf("(%s)(%s)%s", resolvedCS, underlyingCS, shiftExpr)
						}
					}
				}
			} else if binaryExpr.Op == token.SHL {
				// A TYPED narrow-width shifted operand — `cb << k` where cb is a byte/int8/int16/
				// uint16 var or TYPED const — also loses Go's width semantics: C# promotes the
				// sub-int left operand to `int`, so `byte(200) << 1` computes 400 with no
				// wraparound where Go wraps to 144 at byte width. Re-type the result exactly as
				// the untyped-const narrow branch above does, keyed on the SHIFT EXPRESSION's
				// resolved Go type (int32-and-wider left operands already shift at their Go width
				// in C#, so only the four sub-int kinds take the cast). Right shifts need no cast:
				// a narrow operand zero-/sign-extends into the int-width shift, so the result
				// always fits the narrow width. A whole-expression Go CONSTANT is skipped — Go
				// constant arithmetic cannot overflow its type, and the wrap cast on a C#
				// compile-time constant expression would even be rejected in checked constant
				// evaluation (CS0221).
				if tv, ok := v.info.Types[binaryExpr]; ok && tv.Value == nil && tv.Type != nil {
					if basic, ok := tv.Type.Underlying().(*types.Basic); ok && isNarrowIntegerKind(basic.Kind()) {
						underlyingCS := v.getCSharpTypeName(basic)
						resolvedCS := convertToCSTypeName(v.getAliasQualifiedTypeName(tv.Type, false))

						shiftExpr = fmt.Sprintf("(%s)%s", underlyingCS, shiftExpr)

						// A NAMED narrow type routes through its underlying (see above) — the
						// `[GoType]` conversion accepts its exact underlying, never C# `int`.
						if resolvedCS != underlyingCS {
							shiftExpr = fmt.Sprintf("(%s)%s", resolvedCS, shiftExpr)
						}
					}
				}
			}

			return shiftExpr
		}

		bitwiseOp := binaryOp == "&" || binaryOp == "|" || binaryOp == "^" || binaryOp == "&^"

		if binaryOp == "&^" {
			binaryOp = " & ~"
		} else {
			binaryOp = " " + binaryOp + " "
		}

		// bitwise operations need to be in parentheses in C# and usually case to target type
		if bitwiseOp {
			binaryType := v.info.Types[binaryExpr].Type
			var binaryTypeName string

			if binaryType != nil {
				binaryTypeName = convertToCSTypeName(v.getAliasQualifiedTypeName(binaryType, false))
			}

			// A named untyped-const operand (UntypedInt wrapper) resolves to `int` under a
			// bitwise operator — including the `~` of `&^` — which breaks a wider context
			// (`Float64bits(f) &^ signBit`, ulong, signBit=1<<63 → `ulong & int`, CS0019). Cast
			// such an operand to the result's UNDERLYING basic type so its width is preserved (the
			// underlying, not the named type — see concreteNumericCSType; `(Tag)c` would be CS0030).
			if operandCast := v.concreteNumericCSType(binaryType); operandCast != "" {
				if v.isUntypedNamedConstRef(binaryExpr.X) {
					leftOperand = fmt.Sprintf("(%s)%s", operandCast, leftOperand)
				}

				if v.isUntypedNamedConstRef(binaryExpr.Y) {
					rightOperand = fmt.Sprintf("(%s)%s", operandCast, rightOperand)
				}
			}

			// The right operand of `&^` is complemented (the operator was rendered `& ~`), and C#
			// applies `~` to a PROMOTED operand, so a BARE LITERAL complements as `int`. Whether
			// that binds depends on the enclosing result type, and exactly one concrete width has
			// no answer: `uint64`. A narrower-than-int result promotes to `int` and truncates
			// correctly under the enclosing result cast; `uint32 & int` promotes BOTH to `long` and
			// truncates the same way (debug/macho's `Magic32 &^ 1` is that shape and compiles);
			// `int64 & int` widens the int. But `ulong` and `int` share no type C# converts to, so
			// `^uint64(0) &^ 1` emits `~((uint64)0) & ~1` -> CS0019 (net/netip uint128_test.go:76,79).
			// Impose the result type so the complement happens in Go's width.
			//
			// A BasicLit is the ONLY operand shape that reaches `~` untyped, which is why the
			// predicate is the literal and not merely constness: a named untyped-const ref is cast
			// by the block above, a Go conversion (`uint64(1)`) already renders typed, and a
			// computed constant subtree carries the width cast from the shift-retype path
			// (`hi &^ (1 << 63)` emits `~(((uint64)1 << 63))` today). Casting those again would be
			// pure churn -- measured zero corpus movers with this predicate, and the whole point of
			// the deferral this closes was to know that number.
			//
			// `uint64`/`nuint`/`uintptr` are the three unsigned 64-bit results this file already
			// groups (see the constant-fold arm); the other two carry the same cast in the
			// native-int block below, and this completes the set.
			if binaryTypeName == "uint64" && binaryExpr.Op == token.AND_NOT {
				if _, isLit := binaryExpr.Y.(*ast.BasicLit); isLit {
					rightOperand = fmt.Sprintf("(%s)%s", binaryTypeName, rightOperand)
				}
			}

			// The same for a NAMED-numeric result — `tp & (1<<taggedPointerBits - 1)` (runtime
			// tagptr_64bit.go, taggedPointer over uint64): the computed mask renders as an
			// UntypedInt-typed C# expression, and `taggedPointer & int` has no operator (CS0019).
			// Cast the computed operand to the result's UNDERLYING basic (`(uint64)(1<<bits - 1)`
			// — the named type itself would be CS0030), after which the named operand's implicit
			// underlying conversion binds. Gated tightly to an ARITHMETIC-shaped constant operand
			// containing a named untyped-const ref: a BITWISE-shaped constant operand (`m0 & m`,
			// `a | b`) is recursively emitted with its own concrete `(type)(…)` wrap and casting
			// again just doubles it; pure-literal masks convert as C# constants and need no cast.
			arithConstOperand := func(operand ast.Expr) bool {
				for {
					paren, ok := operand.(*ast.ParenExpr)

					if !ok {
						break
					}

					operand = paren.X
				}

				binary, ok := operand.(*ast.BinaryExpr)

				if !ok {
					return false
				}

				switch binary.Op {
				case token.ADD, token.SUB, token.MUL, token.QUO, token.REM:
				default:
					return false
				}

				return v.isComputedConstOperand(operand) && v.containsUntypedNamedConstRef(operand)
			}

			if operandCast := v.concreteNumericCSType(binaryType); operandCast != "" &&
				binaryTypeName != "nuint" && binaryTypeName != "nint" && binaryTypeName != "uintptr" {
				if arithConstOperand(binaryExpr.X) {
					leftOperand = fmt.Sprintf("(%s)(%s)", operandCast, leftOperand)
				}

				if arithConstOperand(binaryExpr.Y) {
					rightOperand = fmt.Sprintf("(%s)(%s)", operandCast, rightOperand)
				}
			}

			// A computed untyped-constant operand — a mask like `(1 << k) - 1` or `~(pageSize - 1)`
			// — in a bitwise op with a NATIVE-int result (`nuint`/`nint`/`uintptr`) is emitted as a
			// bare C# `int` expression (it involves a non-const native value such as a cast, so it is
			// not a C# compile-time constant), so `nuint & ((1<<k)-1)` is `nuint & int` → CS0019. Cast
			// such an operand to the native result type. A bare literal is left alone (C#'s
			// constant-conversion implicitly fits it), and a named untyped-const ref is handled above.
			if binaryTypeName == "nuint" || binaryTypeName == "nint" || binaryTypeName == "uintptr" {
				if v.isComputedConstOperand(binaryExpr.X) || v.isLargeIntLiteralOperand(binaryExpr.X) {
					leftOperand = fmt.Sprintf("(%s)%s", binaryTypeName, leftOperand)
				}

				// The right operand of `&^` is complemented (the operator was rendered `& ~`); `~`
				// promotes to `int`, so a CONSTANT operand `p &^ 15` becomes `nuint & ~15` =
				// `nuint & (int)-16` → CS0019 (a negative `int` does not convert to an unsigned native
				// type, even as a constant). Cast the complemented constant to the native type so the
				// complement is performed in that width (`& ~(uintptr)15`). A non-constant native
				// operand (`p &^ mask`) already complements correctly and is left alone.
				yConst := false

				if tv, ok := v.info.Types[binaryExpr.Y]; ok && tv.Value != nil {
					yConst = true
				}

				andNotConst := binaryExpr.Op == token.AND_NOT && yConst

				if v.isComputedConstOperand(binaryExpr.Y) || v.isLargeIntLiteralOperand(binaryExpr.Y) || andNotConst {
					rightOperand = fmt.Sprintf("(%s)%s", binaryTypeName, rightOperand)
				}
			}

			return fmt.Sprintf("(%s)(%s%s%s)", binaryTypeName, leftOperand, binaryOp, rightOperand)
		}

		// ARITHMETIC only. When one operand references a *named* untyped numeric constant and the
		// other is a concrete numeric type, the constant's wrapper (`UntypedInt`/`UntypedFloat`) has
		// bidirectional implicit conversions, so e.g. `q1 * two32` (ulong * UntypedInt) resolves to
		// `int` (CS0029). Cast the untyped-const operand to the concrete operand's type. Bare
		// literals are not wrapped and follow normal C# rules, so this targets named consts — bare
		// (`q1 * two32`) or inside a COMPUTED constant operand (`arg0 + 4*goarch.PtrSize`, runtime
		// stkframe.go: the product renders as an UntypedInt-typed expression, so the whole sum types
		// as UntypedInt and breaks a following conversion or inference; pure-literal arithmetic like
		// `x + 2*3` has no wrapper and is left alone).
		//
		// A BARE named-const reference in a COMPARISON needs nothing here: the only such operand
		// shape that could not resolve is a BigInteger-backed reference (`x > Two129` — GoBigConst
		// has no implicit operator with `double`, CS0019), and bigIntegerConstMaterialization now
		// casts such a reference at the reference itself, in EVERY concrete numeric context.
		// Re-casting here would only double it (`(float64)(float64)Two129` — the bare-ident form
		// carries no parens, so the wholeExprIsCastOfType redundancy guard below does not recognize
		// it). A single wrapper-backed comparison operand resolves via its own implicit conversion
		// and was never cast — but a COMPUTED operand combining TWO DIFFERENT wrapper kinds (runtime
		// mgclimit_test.go's `procs * capacityPerProc`, UntypedInt * UntypedFloat) has neither an
		// operator overload of its own nor a concrete operand for the arithmetic case below to key
		// off, so it silently resolves through whichever primitive type BOTH wrappers happen to
		// share first (int32) — comparing that against a genuinely concrete `ulong` on the other
		// side is then CS0034, ambiguous between widening the ulong up and narrowing the int32's
		// wrapper-mediated result down. Comparison gets the identical arithmetic-operand treatment
		// below for exactly this computed-operand shape; a bare named-const reference is unaffected,
		// since untypedConstOperand's own bare-reference arm only recurses into a COMPUTED operand.
		var castLeft, castRight bool

		untypedConstOperand := func(operand ast.Expr) bool {
			if v.isUntypedNamedConstRef(operand) {
				return true
			}

			// A Go CONVERSION operand (`float64(1<<x) / 1e9`) already renders concretely
			// typed — casting again would just double the cast.
			if call, ok := operand.(*ast.CallExpr); ok && v.callExprIsTypeConversion(call) {
				return false
			}

			return v.isComputedConstOperand(operand) && v.containsUntypedNamedConstRef(operand)
		}

		// The COMPUTED-only half of untypedConstOperand — excludes a BARE named-const reference,
		// which a comparison resolves fine without help (see the comment above).
		computedUntypedConstOperand := func(operand ast.Expr) bool {
			return !v.isUntypedNamedConstRef(operand) && untypedConstOperand(operand)
		}

		switch binaryExpr.Op {
		case token.ADD, token.SUB, token.MUL, token.QUO, token.REM:
			castLeft = untypedConstOperand(binaryExpr.X)
			castRight = !castLeft && untypedConstOperand(binaryExpr.Y)
		case token.EQL, token.NEQ, token.LSS, token.LEQ, token.GTR, token.GEQ:
			castLeft = computedUntypedConstOperand(binaryExpr.X)
			castRight = !castLeft && computedUntypedConstOperand(binaryExpr.Y)
		}

		// An operand whose own emission is ALREADY exactly this cast — the widened-const narrowing
		// above renders `(uint64)(4503599627370496L - 1)` for `1<<52 - 1` — needs no second one;
		// the same doubling the Go-conversion exclusion in untypedConstOperand guards against.
		if castLeft {
			if tn := v.concreteNumericCSType(rhsType); tn != "" && !wholeExprIsCastOfType(leftOperand, tn) {
				// A COMPUTED float const operand with a named-const ref, in a float64/float32
				// context, FOLDS to a single-rounded literal instead of casting the runtime
				// arithmetic (which rounds twice — the `1 / Ln10` double-round; see HOOK 1).
				if folded := v.foldedNamedFloatConstLiteral(binaryExpr.X, tn); folded != "" {
					leftOperand = folded
				} else if v.needsParentheses(binaryExpr.X) {
					leftOperand = fmt.Sprintf("(%s)(%s)", tn, leftOperand)
				} else {
					leftOperand = fmt.Sprintf("(%s)%s", tn, leftOperand)
				}
			}
		} else if castRight {
			if tn := v.concreteNumericCSType(lhsType); tn != "" && !wholeExprIsCastOfType(rightOperand, tn) {
				if folded := v.foldedNamedFloatConstLiteral(binaryExpr.Y, tn); folded != "" {
					rightOperand = folded
				} else if v.needsParentheses(binaryExpr.Y) {
					rightOperand = fmt.Sprintf("(%s)(%s)", tn, rightOperand)
				} else {
					rightOperand = fmt.Sprintf("(%s)%s", tn, rightOperand)
				}
			}
		}

		// A narrow-integer (int8/uint8/int16/uint16) NON-CONSTANT arithmetic result COMPARED directly —
		// `v + 1 != MinInt8` (math's TestMaxInt/TestMaxUint) — keeps Go's modular-overflow value: C#
		// promotes the sub-int operands to `int`, so `int8(127) + 1` is 128 where Go wraps to -128 at
		// int8 width, flipping the comparison. Wrap each such operand at its own narrow width, exactly as
		// the narrow-arith DESTINATION casts already do (assignment/return/arg/shift). A CONSTANT operand
		// is left alone — a Go constant cannot overflow its type, and a wrap cast on a C# compile-time
		// constant expression is CS0221 (the shift-retype path guards the same way).
		switch binaryExpr.Op {
		case token.LSS, token.LEQ, token.GTR, token.GEQ, token.EQL, token.NEQ:
			leftOperand = v.narrowComparisonOperand(binaryExpr.X, leftOperand)
			rightOperand = v.narrowComparisonOperand(binaryExpr.Y, rightOperand)
		}

		// Go's `&&`/`||` on a NAMED boolean type (`type boolVal bool`) yields that named type,
		// which satisfies an interface return (`case boolVal: return x && y` in go/constant's
		// BinaryOp, returned as the `Value` interface). In C# the `[GoType("bool")]` struct that
		// models the named type has no logical operators, so `left && right` collapses to a bare
		// `bool` — returning it where the named type/interface is expected is CS0029. Cast each
		// operand to `bool`, apply the operator, then cast back to the named type so the result
		// keeps satisfying the interface. Mirrors the unary `!` handling in convUnaryExpr; a
		// predeclared-`bool` result keeps the bare form below (no golden churn).
		if (binaryExpr.Op == token.LAND || binaryExpr.Op == token.LOR) && v.isNamedBooleanType(binaryExpr) {
			typeName := convertToCSTypeName(v.getAliasQualifiedTypeName(v.getType(binaryExpr, false), false))
			return fmt.Sprintf("((%s)((bool)%s %s (bool)%s))", typeName, leftOperand, binaryOp, rightOperand)
		}

		return fmt.Sprintf("%s%s%s", leftOperand, binaryOp, rightOperand)
	}

	// Handle start of pattern match expression with "is" keyword. Go's `!=` has no C# pattern
	// operator spelling — `x is != y` is a parse error (CS1525); the negation pattern
	// `x is not y` carries the exact semantics. Relational ops (<, <=, >, >=) map verbatim.
	if context.declareIsExpr {
		switch binaryOp {
		case "==":
			binaryOp = ""
		case "!=":
			binaryOp = " not"
		default:
			binaryOp = " " + binaryOp
		}

		return fmt.Sprintf("%s is%s %s", leftOperand, binaryOp, rightOperand)
	}

	// Handle remaining pattern match expressions (an `or`-chained subsequent case value):
	switch binaryOp {
	case "==":
		binaryOp = ""
	case "!=":
		binaryOp = "not "
	default:
		binaryOp = binaryOp + " "
	}

	return fmt.Sprintf("%s%s", binaryOp, rightOperand)
}

// constExprHasBeyondInt32Operand reports whether any operand inside a constant expression exceeds
// int32 range — the condition that makes the literal path emit a `long` (`4294967296L`) and so the
// only shape a narrow-unsigned target cannot accept without a fold.
//
// It exists to keep that fold NARROW. Folding every narrow-unsigned constant expression flattens
// readable operator forms and, worse, erases named types whose identity is the emission's point
// (os's `ModeDevice | ModeCharDevice`); measured at 754 sites across 157 files before this bound
// was added. With it, the fold reaches exactly the constants that would otherwise not compile.
func (v *Visitor) constExprOperandExceeds(expr ast.Expr, limit uint64) bool {
	found := false

	ast.Inspect(expr, func(n ast.Node) bool {
		if found {
			return false
		}

		inner, ok := n.(ast.Expr)

		// The whole expression is the FOLD ITSELF — its own value is what the target must accept,
		// never evidence that an OPERAND overflowed. Only proper subexpressions count, and they
		// are read by VALUE rather than by literal text: the operand that forces the widening is
		// usually computed (`1<<32` inside `1<<32 - 2`), so a text scan misses exactly the shape
		// this bound exists to admit.
		if !ok || inner == expr {
			return true
		}

		// Only an UNTYPED subexpression counts. A typed conversion carries its own width and emits
		// correctly on its own — `^uint32(0)` is 4294967295 but renders as a uint32 expression, so
		// runtime's `^uint32(0)/8 + 1` never needed a fold; counting it flattened the whole
		// class_to_divmagic table into 68 unchecked casts (regen 4). The shapes that genuinely
		// force the `long` widening are untyped: `1<<32 - 1`, `1<<32 - 2`.
		tv, recorded := v.info.Types[inner]

		if !recorded || tv.Value == nil {
			return true
		}

		if basicInner, isBasic := tv.Type.(*types.Basic); !isBasic || basicInner.Info()&types.IsUntyped == 0 {
			return true
		}

		if u, exact := constant.Uint64Val(constant.ToInt(tv.Value)); exact && u > limit {
			found = true
			return false
		}

		return true
	})

	return found
}
