// visitAssignStmt.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

package main

import (
	"fmt"
	"go/ast"
	"go/constant"
	"go/token"
	"go/types"
	"math"
	"sort"
	"strings"
)

// rhsPointerCopyContext returns an IdentContext that forces the pointer (box) form when the
// assignment RHS is a plain pointer-typed identifier — a Go pointer copy (`r := p` / `r = p`
// where p is *T). A deref'd pointer parameter then emits its box `Ꮡp` so the target is a
// ж<T> (the rest of the converter already treats such a target as a pointer via `.Value`/`~`).
// A pointer *local* holds the pointer directly, so convIdent returns it unchanged. Returns
// nil when rhs is not a plain non-nil pointer identifier (callers append nothing).
func (v *Visitor) rhsPointerCopyContext(rhs ast.Expr) ExprContext {
	rhsIdent, isIdent := rhs.(*ast.Ident)

	if !isIdent || rhsIdent.Name == "nil" {
		return nil
	}

	// paramPointerType also classifies an ERASED pointer-core type parameter (`q := p` under
	// `[P *T]` is a Go pointer copy — the deref'd parameter must supply its box `Ꮡp`, and a
	// P-typed local already holds the box, exactly like a plain `*T`).
	if _, isPtr := v.paramPointerType(v.getIdentType(rhsIdent)); !isPtr {
		return nil
	}

	ptrContext := DefaultIdentContext()
	ptrContext.isPointer = true

	return ptrContext
}

// appendRhsPtrContext returns base with a pointer-copy IdentContext appended when rhs is a
// plain pointer identifier. A fresh slice is returned so the caller's backing array (often
// shared between LHS and RHS conversion) is not mutated.
func (v *Visitor) appendRhsPtrContext(base []ExprContext, rhs ast.Expr) []ExprContext {
	ptrContext := v.rhsPointerCopyContext(rhs)

	if ptrContext == nil {
		return base
	}

	out := make([]ExprContext, len(base), len(base)+1)
	copy(out, base)

	return append(out, ptrContext)
}

// cloneValueCopy appends the strongly-typed `.Clone()` to a converted assignment RHS
// when the Go statement copies an ARRAY BY VALUE out of existing storage. Go array assignment
// copies the whole array (`data := ints` yields independent storage), but the emitted
// `array<T>` — and the generated named-array wrapper — is a struct over a shared T[] backing
// store, so the plain C# struct copy ALIASES — a later write through the copy mutated the
// source (sort's TestReverseSortIntSlice: `data := ints; data1 := ints` left ONE store sorted
// twice). The shape/type gate is the shared exprReadsValueNeedingClone (an ident,
// selector, index, or deref RHS reads a value some other name can still reach; a composite
// literal or call result is freshly constructed) — which covers DIRECT, alias-declared, AND
// NAMED array types alike, the named wrapper having its own strongly-typed Clone(). A blank
// LHS discards the value, so it takes no clone. (Known remaining edge: a named↔underlying
// array CONVERSION RHS — `[4]int(named)` — is excluded as a call result but hands the
// wrapper's backing through the implicit operator uncloned.)
// appendNilArrayDimsContext carries the ARRAY LENGTH of a pointer-to-array assignment target down
// to the `nil` on the right-hand side. `*[3]int` converts to `ж<array<nint>>` and `array<E>` is
// LENGTH-ERASED, so a plain `default!` loses the 3 -- and unlike the conversion position, nothing
// downstream can recover it: `OrTypedNil()` sees only the erased C# type by the time the value
// reaches the reflection boundary. The dimension has to ride the value from the one place it is
// still statically known, which is here.
//
// Measured, not assumed: reflect/all_test.go's TestValue_Cap and TestValue_Len assign `nil` to a
// local `*[3]int` and then call ValueOf(a).Cap()/.Len(), where Go answers 3 and the erased form
// answers 0.
func (v *Visitor) appendNilArrayDimsContext(base []ExprContext, rhs ast.Expr, lhs ast.Expr) []ExprContext {
	return v.appendNilArrayDimsTypeContext(base, rhs, v.getExprType(lhs))
}

func (v *Visitor) cloneValueCopy(lhs ast.Expr, rhs ast.Expr, rhsExpr string) string {
	if lhs != nil {
		if ident := getIdentifier(lhs); ident != nil && ident.Name == "_" {
			return rhsExpr
		}
	}

	if !v.exprReadsValueNeedingClone(rhs) {
		return rhsExpr
	}

	return appendValueClone(rhsExpr, v.getExprType(rhs))
}

// lhsIsEmptyInterface reports whether the assignment target's static type is an EMPTY
// interface (`any`). A string literal assigned to such a target must render as a boxed
// @string (`(@string)"…"`) — the default `"…"u8` ReadOnlySpan<byte> has no conversion to
// object (CS0029; go/types format.go sprintf's `arg = "<nil>"`), and boxing a golib
// @string (not a C# string) preserves Go string identity for a later type assertion.
// Mirrors the visitReturnStmt `any`-result arm; a NON-empty interface target is handled
// separately by convertExprToInterfaceType.
func (v *Visitor) lhsIsEmptyInterface(lhs ast.Expr) bool {
	lhsType := v.getType(lhs, false)

	if lhsType == nil {
		return false
	}

	isIface, isEmpty := isInterface(lhsType)

	return isIface && isEmpty
}

// appendEmptyIfaceLitContext returns base with a boxed-@string literal context appended when
// the assignment target is an EMPTY interface (see lhsIsEmptyInterface). Only string
// basic-literals consult these flags (convBasicLit), so any other RHS is unaffected. A fresh
// slice is returned so the caller's backing array is not mutated.
func (v *Visitor) appendEmptyIfaceLitContext(base []ExprContext, lhs ast.Expr) []ExprContext {
	if !v.lhsIsEmptyInterface(lhs) {
		return base
	}

	basicLitContext := DefaultBasicLitContext()
	basicLitContext.u8StringOK = true
	basicLitContext.castToGoString = true
	basicLitContext.spanTargetUnsupported = true

	out := make([]ExprContext, len(base), len(base)+1)
	copy(out, base)

	return append(out, basicLitContext)
}

// untypedConstDeclTypeName returns the C# name of the DEFAULT Go type a `:=`-declared local
// binds when its RHS is a reference to a NAMED UNTYPED NUMERIC constant, or "" when the
// `var` form is fine. Such a constant is emitted as a `static readonly` Untyped* wrapper
// (golib UntypedInt/UntypedFloat/UntypedComplex — e.g. `unicode.ReplacementChar`), so `var`
// binds the local to the WRAPPER type instead of the Go-inferred default type (rune), and a
// later Go conversion like `string(codepoint)` fails (CS0030 — no UntypedInt→@string form).
// Materializing the inferred type (`rune codepoint = …`) restores Go's typing. Literal and
// computed constant expressions render as plain C# literals and are unaffected.
func (v *Visitor) untypedConstDeclTypeName(ident *ast.Ident, rhs ast.Expr) string {
	if ident == nil {
		return ""
	}

	for {
		paren, ok := rhs.(*ast.ParenExpr)

		if !ok {
			break
		}

		rhs = paren.X
	}

	var obj types.Object

	switch e := rhs.(type) {
	case *ast.Ident:
		obj = v.info.ObjectOf(e)
	case *ast.SelectorExpr:
		obj = v.info.ObjectOf(e.Sel)
	default:
		return ""
	}

	constObj, ok := obj.(*types.Const)

	if !ok {
		return ""
	}

	basic, ok := constObj.Type().(*types.Basic)

	if !ok {
		return ""
	}

	switch basic.Kind() {
	case types.UntypedInt, types.UntypedRune, types.UntypedFloat, types.UntypedComplex:
	default:
		return ""
	}

	return convertToCSTypeName(v.getExpressionTypeName(ident, false))
}

// materializeDefaultTypedConstDeclValue folds a computed untyped-float constant at the DEFAULT
// type inferred for a newly `:=`-declared local when the expression contains a named untyped
// constant. Unlike an explicitly typed slot, go/types leaves the RHS itself untyped and records
// float64 only on the new variable, so convBinaryExpr cannot see the target width. Without this
// declaration-edge materialization, C# binds `.5 * REP` to UntypedInt.operator*, truncates .5 to
// zero, and only then assigns the zero result to the float64 local. The shared fold is exact and
// single-rounded, and returns "" for literals, bare constant references, non-float constants, and
// expressions without a wrapper-emitted named constant.
func (v *Visitor) materializeDefaultTypedConstDeclValue(ident *ast.Ident, rhs ast.Expr, rendered string) string {
	if ident == nil {
		return rendered
	}

	tv, ok := v.info.Types[rhs]

	if !ok || tv.Value == nil || tv.Type == nil {
		return rendered
	}

	basic, ok := tv.Type.Underlying().(*types.Basic)

	if !ok || basic.Info()&types.IsUntyped == 0 {
		return rendered
	}

	if !v.containsUntypedNamedIntegerConstRef(rhs) {
		return rendered
	}

	targetCSType := convertToCSTypeName(v.getExpressionTypeName(ident, false))

	if folded := v.foldedNamedFloatConstLiteral(rhs, targetCSType); folded != "" {
		return folded
	}

	return rendered
}

// narrowArithmeticCastType returns the C# narrow-integer type a binary/unary arithmetic assignment
// RHS must be cast to when its Go type matches a narrow-integer LHS (int8/uint8/int16/uint16), or
// "" when no cast applies. Go evaluates such arithmetic at the operand's own width (wrapping); C#
// promotes sub-int arithmetic to int, so the result needs a cast back to compile (CS0266) and to
// preserve the wrap. The optional alreadyCast guard skips the cast when the converted RHS already
// starts with `(type)(` — another path narrowed it (e.g. `(byte)(b | 128)`) — to avoid a double cast.
func (v *Visitor) narrowArithmeticCastType(lhs, rhs ast.Expr, alreadyCast string) string {
	return v.narrowArithmeticCastTypeFor(v.getType(lhs, false), rhs, alreadyCast)
}

// narrowArithmeticCastTypeFor is the type-keyed form of narrowArithmeticCastType — used where the
// narrow target is a declared variable's type (a `var x uint8 = a + b` value-spec initializer)
// rather than an LHS expression.
func (v *Visitor) narrowArithmeticCastTypeFor(targetType types.Type, rhs ast.Expr, alreadyCast string) string {
	switch rhs.(type) {
	case *ast.BinaryExpr, *ast.UnaryExpr:
	default:
		return ""
	}

	if targetType == nil {
		return ""
	}

	targetBasic, ok := targetType.Underlying().(*types.Basic)

	if !ok || !isNarrowIntegerKind(targetBasic.Kind()) {
		return ""
	}

	if rhsType := v.getType(rhs, false); rhsType == nil || !types.Identical(rhsType, targetType) {
		return ""
	}

	castType := convertToCSTypeName(v.getAliasQualifiedTypeName(targetType, false))

	// Skip ONLY when the WHOLE converted RHS is already `(castType)(…)` — another path narrowed the
	// entire binary result (`(byte)(b | 128)`). A leading `(castType)(` that closes BEFORE the end is
	// just the FIRST OPERAND's own conversion — `(byte)(e / 100) + (rune)'0'`, where the `(byte)(` casts
	// only `e / 100`, so the binary result is still `int` and the narrowing cast is still required
	// (CS0266). Verify the cast-paren's matching close is at the very end before treating it as covered.
	if len(alreadyCast) > 0 && wholeExprIsCastOfType(alreadyCast, castType) {
		return ""
	}

	return castType
}

// narrowComparisonOperand wraps a comparison operand that is a NON-CONSTANT narrow-integer
// arithmetic/complement result at its own width, so Go's modular overflow survives the comparison —
// `int8(127) + 1 != MinInt8` must see -128, not the C# int-promoted 128 (see convBinaryExprCore).
// A constant operand (Go constants cannot overflow their type; a wrap cast on a C# constant is
// CS0221) or a non-arithmetic operand is returned unchanged, and narrowArithmeticCastTypeFor's
// already-cast guard prevents double-wrapping.
func (v *Visitor) narrowComparisonOperand(operand ast.Expr, converted string) string {
	if tv, ok := v.info.Types[operand]; !ok || tv.Value != nil {
		return converted
	}

	castType := v.narrowArithmeticCastTypeFor(v.getType(operand, false), operand, converted)

	if castType == "" {
		return converted
	}

	return fmt.Sprintf("(%s)(%s)", castType, converted)
}

// wholeExprIsCastOfType reports whether expr is exactly `(castType)(…)` spanning its whole length — the
// cast wraps the entire expression, not just a leading sub-expression. Used to decide whether a
// narrow-integer arithmetic RHS has already been narrowed in full (skip the redundant cast) versus only
// having its first operand cast (the binary result still needs the cast). Balance-counts parentheses,
// skipping any inside a char/string literal (`(rune)'('`) so a paren character does not perturb the depth.
func wholeExprIsCastOfType(expr, castType string) bool {
	// An `unchecked(…)` wrapper does not change WHAT the expression is. A beyond-int32 constant
	// folded to a native-width target carries one so the CONSTANT conversion does not warn
	// (CS8778 — see convBinaryExpr's fold), and every caller here is asking "is this already a
	// whole-expression cast of that type?", to which the answer is still yes. Peel it first, or
	// the guards re-wrap and emit `(nint)(unchecked((nint)(…)))`.
	const uncheckedPrefix = "unchecked("

	if strings.HasPrefix(expr, uncheckedPrefix) &&
		balancedCloseIndex(expr, len(uncheckedPrefix)-1) == len(expr)-1 {
		expr = expr[len(uncheckedPrefix) : len(expr)-1]
	}

	prefix := fmt.Sprintf("(%s)(", castType)

	if !strings.HasPrefix(expr, prefix) {
		return false
	}

	// Balance from the opening paren of the cast body (the final '(' of the prefix).
	return balancedCloseIndex(expr, len(prefix)-1) == len(expr)-1
}

// balancedCloseIndex returns the index of the `)` closing the `(` at openIdx, or -1 when the
// parentheses do not balance before the end of expr. Parens inside a char/string literal
// (`(rune)'('`) are skipped so a paren CHARACTER does not perturb the depth.
func balancedCloseIndex(expr string, openIdx int) int {
	depth := 0

	for i := openIdx; i < len(expr); i++ {
		switch expr[i] {
		case '\'', '"':
			// Skip the literal's contents. C# escapes an embedded quote as `\'`/`\"`, so honor
			// the backslash escape.
			quote := expr[i]

			for i++; i < len(expr); i++ {
				if expr[i] == '\\' {
					i++ // skip the escaped character
					continue
				}

				if expr[i] == quote {
					break
				}
			}
		case '(':
			depth++
		case ')':
			depth--

			if depth == 0 {
				return i
			}
		}
	}

	return -1
}

// nativeIntConstCastType returns the C# native-integer type (`uintptr`/`nuint`/`nint`) a computed
// CONSTANT arithmetic RHS must be cast to when assigned to a native-width integer LHS, or "" when none
// applies. A Go untyped constant expression like `1<<maxBits - 1` (target `uintptr`, `maxBits` = 57) has
// its sub-shift `1<<maxBits` folded by overflowingConstLiteral to a SIGNED C# `long` literal
// (`144115188075855872L`, > int32), so the whole RHS is `long` — which has no implicit conversion to the
// native unsigned/`nint` target (CS0266); a `UL`/`(nuint)` suffix would not help either (ulong→nuint is
// also explicit). Wrapping the whole RHS in `(uintptr)(…)` narrows it back (the value fits the 64-bit
// native type).
//
// The cast is applied ONLY when the emitted arithmetic is provably ≥64-bit — i.e. at least one OPERAND is
// itself folded to a signed `long` literal (`overflowingConstLiteral != ""`). This is the crux of
// correctness: a BARE shift `1 << 40` (both operands small) is emitted as a 32-bit `1 << (int)(40)` that
// MASKS the count (`40 & 31`) and truncates to `256`; casting that wrongly-computed result would convert a
// loud CS0266 into a SILENT wrong value. The mbitmap form passes because `1<<maxBits` (an operand) folds to
// a `long`, forcing `long - 1`. Other exclusions: a bare literal (convBasicLit emits `UL`/`(nuint)`), an
// in-range const (a bare `int` that converts implicitly), a non-const native arithmetic (`nuint - nuint`),
// a value that OVERFLOWS int64 (a large unsigned `uintptr` like `1<<63 + 1<<62` — its sub-shift also
// mis-emits, so it is left as a loud CS0266, not masked), and a NAMED type over a native int (a `[GoType]`
// cast only accepts its exact underlying — `(myUintptr)(long)` is CS0030 — so those keep the loud error).
func (v *Visitor) nativeIntConstCastType(lhs, rhs ast.Expr, alreadyCast string) string {
	var operands []ast.Expr

	switch e := rhs.(type) {
	case *ast.BinaryExpr:
		operands = []ast.Expr{e.X, e.Y}
	case *ast.UnaryExpr:
		operands = []ast.Expr{e.X}
	default:
		return ""
	}

	// PLAIN native-width target only (not `.Underlying()`): a named type over uintptr/uint/int is
	// excluded — a `[GoType]` cast rejects a non-underlying operand (CS0030).
	basic, ok := v.getType(lhs, false).(*types.Basic)

	if !ok {
		return ""
	}

	switch basic.Kind() {
	case types.Uintptr, types.Uint, types.Int:
		// C# nuint/nint — no implicit conversion from a long/ulong literal.
	default:
		return ""
	}

	// The whole RHS must be a CONSTANT whose value fits int64 but falls OUTSIDE the C# int32 range — the
	// range overflowingConstLiteral folds to a signed `long`. An in-range constant emits as a bare `int`
	// (converts implicitly); a value that overflows int64 goes through a different, already-imperfect path
	// and is left as a loud CS0266 rather than masked.
	tv, ok := v.info.Types[rhs]

	if !ok || tv.Value == nil {
		return ""
	}

	if i, exact := constant.Int64Val(constant.ToInt(tv.Value)); !exact || (i >= math.MinInt32 && i <= math.MaxInt32) {
		return ""
	}

	// CRUX: at least one operand must itself fold to a signed `long` literal, so the emitted arithmetic is
	// done in 64-bit width. Without this a bare `1 << 40` (small operands, 32-bit truncating shift) would be
	// silently mis-cast; requiring a folded operand keeps such forms as a loud CS0266.
	widened := false

	for _, operand := range operands {
		if len(v.overflowingConstLiteral(operand)) > 0 {
			widened = true
			break
		}
	}

	if !widened {
		return ""
	}

	castType := convertToCSTypeName(v.getAliasQualifiedTypeName(basic, false))

	if len(alreadyCast) > 0 && wholeExprIsCastOfType(alreadyCast, castType) {
		return ""
	}

	return castType
}

// visitAssignStmt emits a Go assignment statement as C#.
//
// The size is earned: `=` and `:=` in Go cover far more ground than C# assignment does, and each
// shape needs its own rendering. One statement here may be a declaration or a plain assignment;
// may assign one value or destructure a MULTI-VALUE call; may be the comma-ok form of a map read,
// type assertion or channel receive; may swap several variables at once (Go evaluates every
// right-hand side before assigning any, so `a, b = b, a` needs temporaries); may write through a
// pointer, a heap box, a promoted embedded field or an interface slot; and may need a conversion,
// a cast or a defensive clone on the way in.
//
// Roughly three quarters of this file is this one function. It is planned for decomposition, and
// the natural seams are those shapes — the branches on assignStmt.Tok and on the arity of Lhs/Rhs.
// nativeBackedArrayPointerStore recognizes Go's write-barrier-dodging store into a slot whose
// static type is a POINTER TO ARRAY, and emits a box MINT rather than a raw-address write:
//
//	*(*uintptr)(unsafe.Pointer(&p.chunks[c.l1()])) = uintptr(r)   // runtime/mpagealloc.go:420
//
// Go writes the raw address straight into the pointer slot to keep the store off the write barrier
// ("grow is used in call chains that disallow write barriers"). The faithful conversion of that is
// `(Ꮡ(...).Reinterpret<ж<array<T>>, uintptr>()).Value = (uintptr)r`, which takes Reinterpret's
// ALIASING arm and hands back a ref naming the slot ITSELF -- so the raw word lands exactly where a
// managed reference belongs. Nothing fails at the store. The next READ of the slot reinterprets
// element bytes as an array<T> header, the T[] it finds is garbage, and the process dies inside the
// prestub with no managed frame to name it: Q58's measured wall, eight page-allocator rows exiting
// 139 with a blank stderr.
//
// A managed slot cannot hold a bare address, so the honest answer is to MINT a pointer that NAMES
// the native block. This is a converter change and not a golib one because the store writes THROUGH
// A REF: `ж<T>.Value` is a `ref T` property, and a box cannot observe a write performed through a
// ref it has already surrendered (DESIGN-native-backed-array-pointer.md, the 2026-09-05 amendment).
//
// SCOPE, censused rather than assumed. The assignment shape appears 20 times in the pinned GOROOT
// (go1.23.12, production and _test.go), and the destination is a pointer to ARRAY at exactly ONE of
// them -- the site above. The other 19 store into pointer-to-struct, pointer-to-byte and
// unsafe.Pointer slots; 12 of those reach the same aliasing reinterpret in today's emission and are
// latent in the same way, one type-kind over. They are deliberately NOT served here: none of them
// has a measured failing row, and a native box for those kinds is a separate question with its own
// evidence to gather. Widening this gate is a change of SCOPE, not a tweak of the predicate -- so it
// tests the destination type and refuses everything else, rather than matching on the syntax alone.
func (v *Visitor) nativeBackedArrayPointerStore(lhs ast.Expr, rhs ast.Expr) (string, bool) {
	star, isStar := ast.Unparen(lhs).(*ast.StarExpr)

	if !isStar {
		return "", false
	}

	// The deref target is the dodge's own cast -- `*uintptr` and nothing else.
	castPtr, isCastPtr := types.Unalias(v.info.TypeOf(star.X)).(*types.Pointer)

	if !isCastPtr {
		return "", false
	}

	if castBasic, isBasic := types.Unalias(castPtr.Elem()).(*types.Basic); !isBasic || castBasic.Kind() != types.Uintptr {
		return "", false
	}

	cast, isCast := ast.Unparen(star.X).(*ast.CallExpr)

	if !isCast || len(cast.Args) != 1 || !v.callExprIsTypeConversion(cast) {
		return "", false
	}

	addressed := v.unwrapUnsafePointerConversion(cast.Args[0])

	if addressed == nil {
		return "", false
	}

	unary, isUnary := ast.Unparen(addressed).(*ast.UnaryExpr)

	if !isUnary || unary.Op != token.AND {
		return "", false
	}

	// The DESTINATION's own static type is what decides, never the shape around it.
	destPtr, destIsPtr := types.Unalias(v.info.TypeOf(unary.X)).(*types.Pointer)

	if !destIsPtr {
		return "", false
	}

	destArray, destIsArray := types.Unalias(destPtr.Elem()).(*types.Array)

	if !destIsArray {
		return "", false
	}

	// The length is Go's own N, read from the destination type rather than from the store site: the
	// write site happens to know it here (l2Size is one line up), but the TYPE knows it everywhere.
	elemType := convertToCSTypeName(v.getAliasQualifiedTypeName(destArray.Elem(), false))

	return fmt.Sprintf("%s.Value = NativeArrayPointer<%s>((nuint)(%s), %d);",
		v.convExpr(unary, nil), elemType, v.convExpr(rhs, nil), destArray.Len()), true
}

func (v *Visitor) visitAssignStmt(assignStmt *ast.AssignStmt, format FormattingContext) {
	result := &strings.Builder{}

	// A func literal on the RHS (or inside a composite-literal element of it) emits its captured-
	// variable snapshot declarations inline at the literal's position — invalid C# in an expression
	// slot. For a standalone statement, collect them in a buffer and write them before the statement.
	// Save/restore so a nested statement's hoisted decls don't leak into this buffer.
	savedHoist := v.hoistedDecls
	var hoistBuf *strings.Builder

	if format.useNewLine {
		hoistBuf = &strings.Builder{}
		v.hoistedDecls = hoistBuf
	}

	defer func() { v.hoistedDecls = savedHoist }()

	lhsExprs := assignStmt.Lhs
	rhsExprs := assignStmt.Rhs

	lhsLen := len(lhsExprs)
	rhsLen := len(rhsExprs)

	// A NESTED map assignment `m[k1][k2] = v` — the outer index `m[k1]` returns a map VALUE
	// (golib map is a readonly struct), so a C# indexer SETTER on that rvalue is CS1612
	// (internal/dag's `g.edges[from][to] = true`). Emit `m[k1].Set(k2, v)`: a method call runs
	// on the rvalue and mutates the shared backing dictionary, so the write is visible through
	// the original. Only a plain `=` whose LHS is a map-index over a map-index base needs it.
	if lhsLen == 1 && rhsLen == 1 && assignStmt.Tok == token.ASSIGN {
		if outerIndex, ok := lhsExprs[0].(*ast.IndexExpr); ok {
			if _, baseIsIndex := outerIndex.X.(*ast.IndexExpr); baseIsIndex {
				if baseType := v.getType(outerIndex.X, false); baseType != nil {
					if _, isMap := baseType.Underlying().(*types.Map); isMap {
						outerExpr := v.convExpr(outerIndex.X, nil)
						keyExpr := v.convExpr(outerIndex.Index, nil)
						valExpr := v.convExpr(rhsExprs[0], nil)
						result.WriteString(fmt.Sprintf("%s.Set(%s, %s);", outerExpr, keyExpr, valExpr))

						if hoistBuf != nil && hoistBuf.Len() > 0 {
							v.outputBuilder.WriteString(hoistBuf.String())
						} else if format.useNewLine {
							v.outputBuilder.WriteString(v.newline)
						}

						if format.useIndent {
							v.outputBuilder.WriteString(v.indent(v.indentLevel))
						}

						v.outputBuilder.WriteString(result.String())
						return
					}
				}
			}
		}
	}

	// Go's write-barrier-dodging store into a POINTER-TO-ARRAY slot mints a native-backed pointer
	// instead of punching a raw address through a uintptr view of the slot -- see
	// nativeBackedArrayPointerStore for the wall this answers and the census that bounds it.
	if lhsLen == 1 && rhsLen == 1 && assignStmt.Tok == token.ASSIGN {
		if mint, isMint := v.nativeBackedArrayPointerStore(lhsExprs[0], rhsExprs[0]); isMint {
			result.WriteString(mint)

			if hoistBuf != nil && hoistBuf.Len() > 0 {
				v.outputBuilder.WriteString(hoistBuf.String())
			} else if format.useNewLine {
				v.outputBuilder.WriteString(v.newline)
			}

			if format.useIndent {
				v.outputBuilder.WriteString(v.indent(v.indentLevel))
			}

			v.outputBuilder.WriteString(result.String())
			return
		}
	}

	// A `name := func(…) {…}` whose variable is only ever CALLED emits as a C# LOCAL FUNCTION
	// rather than a lambda bound to a variable — the capturing form costs a display class plus a
	// delegate on every evaluation, the local function nothing (see localFunctionDefine). The
	// literal renders the whole statement, so nothing here appends a `;`.
	if ident, funcLit, ok := v.localFunctionDefine(assignStmt, format); ok {
		lambdaContext := DefaultLambdaContext()
		lambdaContext.localFuncName = getSanitizedIdentifier(v.getIdentName(ident))
		lambdaContext.deferredDecls = hoistBuf

		localFunc := v.convFuncLit(funcLit, lambdaContext)

		if hoistBuf != nil && hoistBuf.Len() > 0 {
			v.outputBuilder.WriteString(hoistBuf.String())
		} else if format.useNewLine {
			v.outputBuilder.WriteString(v.newline)
		}

		if format.useIndent {
			v.outputBuilder.WriteString(v.indent(v.indentLevel))
		}

		v.outputBuilder.WriteString(localFunc)
		return
	}

	reassignedCount := 0
	declaredCount := 0

	// Check for interface types in LHS as RHS will need to be casted to the interface type
	lhsTypeIsInterface := make([]bool, lhsLen)

	// Check for string types in LHS, u8 readonly spans are not supported in value tuple
	lhsTypeIsString := make([]bool, lhsLen)
	anyTypeIsString := false

	// Ensure that the correct type is used for integer, we do this since int and uint in
	// converted Go code target C# nint or nuint to match original Go code behavior and a
	// "var" based assignment to an int type could result in a very subtle type mismatch
	lhsTypeIsInt := make([]bool, lhsLen)
	anyTypeIsInt := false

	// Ensure that the correct type is used for unsafe.Pointer, we do this since unsafe.Pointer
	// in converted Go code is usually cast to uintptr in C# - in these cases we need to ensure
	// that the type specifically @unsafe.Pointer in C# and not uintptr when using var
	lhsTypeIsUnsafePointer := make([]bool, lhsLen)
	anyTypeIsUnsafePointer := false

	// Check if rhs is a call with a tuple result
	var tupleResult bool

	if rhsLen == 1 {
		if callExpr, ok := rhsExprs[0].(*ast.CallExpr); ok {
			funType := v.info.TypeOf(callExpr.Fun)

			// A CONVERSION to a func type — `f := (func(...) (T, bool))(g)` — has a Fun that IS
			// the func type, so TypeOf(Fun) is that *types.Signature; its result count describes
			// the CONVERTED function, not a tuple-returning call. Only a genuine call site
			// (Fun is a value) can produce a tuple result.
			funIsType := false

			if tv, ok := v.info.Types[callExpr.Fun]; ok {
				funIsType = tv.IsType()
			}

			// Underlying: the callee may be typed by a NAMED func type — bufio Scanner's
			// `split SplitFunc` field — whose TypeOf is the *types.Named, not the signature;
			// the bare assertion missed it and the 3-tuple deconstruction SHATTERED into one
			// full call per LHS element (triple side effects, CS0029/CS1503 x4).
			//
			// funType is nil when the callee's operand went invalid — a package that did not fully
			// type-check (issue #33); with no signature there is no tuple to deconstruct, which is
			// the same answer the non-signature case gives.
			if signature, ok := underlyingOf(funType).(*types.Signature); ok && !funIsType {
				results := signature.Results()

				if results != nil {
					tupleResult = results.Len() > 1
				}
			}
		} else if unaryExpr, ok := rhsExprs[0].(*ast.UnaryExpr); ok {
			if unaryExpr.Op == token.ARROW {
				tupleResult = lhsLen > 1
			}
		} else if _, ok := rhsExprs[0].(*ast.TypeAssertExpr); ok {
			tupleResult = lhsLen > 1
		} else if indexExpr, ok := rhsExprs[0].(*ast.IndexExpr); ok && lhsLen > 1 {
			// Comma-ok map access: `v, ok := m[k]`. Detecting it as a tuple result routes
			// the RHS through golib's two-value map indexer `m[key, ꟷ]` (see convIndexExpr).
			// A CONSTRAINED TYPE PARAMETER with a map core (`M ~map[K]V`, the maps package)
			// indexes through the same IMap<K, V> comma-ok surface.
			if _, isMap := v.getType(indexExpr.X, true).(*types.Map); isMap {
				tupleResult = true
			} else if tp, isTypeParam := types.Unalias(v.getType(indexExpr.X, false)).(*types.TypeParam); isTypeParam && typeParamMapCore(tp) != nil {
				tupleResult = true
			}
		}
	}

	// Count the number of reassigned and declared variables
	for i, lhs := range lhsExprs {
		ident := getIdentifier(lhs)

		// A selector LHS (`pkg.Var = …`, `x.field = …`) is always an assignment to existing
		// storage, never a declaration. getIdentifier would return its BASE ident (e.g. the
		// package name `sub`), which — not being a local reassignment — would wrongly be counted
		// as a new declaration and later prefixed with `var`. Treat it as the struct-member case.
		if _, ok := lhs.(*ast.SelectorExpr); ok {
			ident = nil
		}

		if ident == nil {
			// Check if lhs is a struct member
			if selectorExpr, ok := lhs.(*ast.SelectorExpr); ok {
				ident = getIdentifier(selectorExpr.Sel)

				// A FIELD/package-var write is a write to EXISTING storage, exactly like the
				// star-deref and index forms below — so it counts as REASSIGNED. Leaving it
				// uncounted made a parallel assignment whose targets are all selectors satisfy
				// NEITHER tuple-path gate (`lhsLen == reassignedCount` nor `lhsLen ==
				// declaredCount`), so it shattered into sequential stores and lost the swap's
				// implicit temporary: regexp's onepass `inst.Out, inst.Arg = inst.Arg, inst.Out`
				// emitted `Out = Arg; Arg = Out;`, leaving BOTH fields equal to the original Arg —
				// every alt whose empty-match leg needed swapping was then rejected, so `^[a-c]*$`
				// and friends silently lost their one-pass engine (and `^[a-c]+$` stopped matching).
				reassignedCount++

				isInterface, isEmpty := v.isInterface(ident)
				lhsTypeIsInterface[i] = isInterface && !isEmpty
				typeName := v.getExpressionTypeName(ident, true)

				lhsTypeIsString[i] = typeName == "string"

				if !anyTypeIsString && lhsTypeIsString[i] {
					anyTypeIsString = true
				}

				lhsTypeIsInt[i] = typeName == "int" || typeName == "uint"

				if !anyTypeIsInt && lhsTypeIsInt[i] {
					anyTypeIsInt = true
				}
			} else if indexExpr, ok := lhs.(*ast.IndexExpr); ok {
				// An INDEX-expression LHS whose base has no plain-ident root — a deref through a
				// paren (`(*h)[i]`, a pointer-receiver method writing its named-slice element), a
				// call result, etc. — is a write to EXISTING storage, exactly like the plain-base
				// `hw[i]` form counted in the ident != nil arm below (see its comment). It must
				// count as REASSIGNED so a parallel assignment `(*h)[i], (*h)[j] = (*h)[j], (*h)[i]`
				// stays a simultaneous tuple deconstruction `((h)[i], (h)[j]) = ((h)[j], (h)[i])`
				// instead of shattering into sequential statements that drop the swap's temporary
				// (heap's myHeap.Swap corrupted the heap: the first store clobbered the value the
				// second needed). getIdentifier does not unwrap ParenExpr, so ident stays nil here;
				// the single-element form (`(*h)[i] = v`) emits identically on either path.
				ident = getIdentifier(indexExpr.X)
				reassignedCount++

				isInterface, isEmpty := v.isInterface(ident)
				lhsTypeIsInterface[i] = isInterface && !isEmpty
			} else if starExpr, ok := lhs.(*ast.StarExpr); ok && lhsLen > 1 {
				// A STAR-DEREF LHS whose OPERAND has no plain-ident root — the deref of a CALL
				// result (`*l.Ptr(i)`), of a paren, etc. — is a write to EXISTING storage, exactly
				// like the plain-base `*v` form counted in the ident != nil arm below. getIdentifier
				// has no CallExpr arm, so ident is nil here and the deref fell through BOTH arms
				// above, counting as neither reassigned nor declared: the parallel assignment then
				// satisfied no tuple-path gate and shattered into sequential stores that drop the
				// swap's implicit temporary. internal/trace/internal/oldtrace's Events.Swap —
				// `*l.Ptr(i), *l.Ptr(j) = *l.Ptr(j), *l.Ptr(i)`, the sort.Stable comparator's only
				// mutator — emitted `Ptr(i).Value = Ptr(j).Value; Ptr(j).Value = Ptr(i).Value;`,
				// making every swap a no-op that DUPLICATED element j over element i. The corrupted
				// event stream then failed the old-trace parser's own post-pass consistency checks
				// ("p N is running before start", "previous sweeping is not ended before a new one").
				// Counting it REASSIGNED keeps it a simultaneous tuple deconstruction.
				//
				// Scoped to lhsLen > 1 deliberately, unlike the two arms above: SIMULTANEITY is the
				// only property at stake, and a single-element deref write has no hazard to fix. That
				// single form is also the overwhelmingly common one — 213 sites in the converted
				// corpus at Go 1.23.12 (233 counting cmd/), 60 in reflect/value.go alone, nearly all
				// the `*(*T)(p) = v` unsafe-write idiom — so leaving them on their existing path
				// holds this change's emission footprint to the ONE site in the Go tree that is
				// genuinely a parallel deref assignment. (Measured: the single-element form emits
				// identically on either path, so the scoping buys footprint, not correctness.)
				ident = getIdentifier(starExpr.X)
				reassignedCount++

				isInterface, isEmpty := v.isInterface(ident)
				lhsTypeIsInterface[i] = isInterface && !isEmpty
			}
		} else {
			// A STAR-DEREF LHS (`*v = …`) writes through EXISTING storage — never a
			// declaration, even when the base ident is a fresh clause-scoped binding whose
			// object the analysis has no reassignment record for (a type-switch case var:
			// fmt scanOne's `*v = s.scanBool(verb)` took a spurious `var` prefix, CS1003 x18).
			// An INDEX-expression LHS (`hw[i], ok = xtoi2(…)`, net mac.go) is the same: an
			// ELEMENT write to existing storage — getIdentifier digs to the CONTAINER ident
			// (`hw`, a named RESULT with no reassignment record), which was counted as a new
			// declaration and the tuple element took a `var hw[i]` prefix (46-error cascade).
			// Counting it REASSIGNED (not ident=nil) keeps the parallel-tuple path selection
			// (`p[i], p[j] = p[j], p[i]` must stay a simultaneous deconstruction).
			if _, isStarDeref := lhs.(*ast.StarExpr); isStarDeref {
				reassignedCount++
			} else if _, isIndexWrite := lhs.(*ast.IndexExpr); isIndexWrite {
				reassignedCount++
			} else if v.isReassignment(ident) {
				reassignedCount++
			} else {
				obj := v.info.ObjectOf(ident)

				if obj != nil {
					if !v.identEscapesHeap[obj] {
						declaredCount++
					} else if lhsLen > 1 && v.convertToHeapTypeDecl(ident, false) == "" {
						// An escaping TUPLE element is excluded from declaredCount only when it actually
						// gets a heap-type decl — the escaping-tuple path at the gate below owns its
						// declaration (`ref var list = ref heap(…)`). An escaping element with NO heap
						// decl (e.g. an already-pointer local that merely gets returned, so escape
						// analysis flags it but convertToHeapTypeDecl yields nothing) is declared nowhere
						// else, so it must still be counted to receive a `var`; otherwise the tuple emits
						// `(pp, now) = …` with `pp` never declared (CS0103). Single-var (lhsLen==1)
						// escaping declarations keep the original exclusion — they route to the
						// capture-hoist emission path which relies on declaredCount staying 0.
						declaredCount++
					}
				}
			}

			// A STAR-DEREF LHS writes the POINTED-TO storage, so interface-ness (and the RHS
			// interface wrap it gates) comes from the deref'd expression type, not the pointer
			// ident (dwarf zeroArray's `*t = &tt` with `t *Type` — the ж<ArrayType> RHS missed
			// the pointer-adapter wrap, CS0266).
			if _, isStarDeref := lhs.(*ast.StarExpr); isStarDeref {
				lhsIsIface, isEmpty := isInterface(v.getType(lhs, false))
				lhsTypeIsInterface[i] = lhsIsIface && !isEmpty
			} else {
				lhsIsIface, isEmpty := v.isInterface(ident)
				lhsTypeIsInterface[i] = lhsIsIface && !isEmpty
			}

			typeName := v.getExpressionTypeName(ident, true)

			lhsTypeIsString[i] = typeName == "string"

			if !anyTypeIsString && lhsTypeIsString[i] {
				anyTypeIsString = true
			}

			lhsTypeIsInt[i] = typeName == "int" || typeName == "uint"

			if !anyTypeIsInt && lhsTypeIsInt[i] {
				anyTypeIsInt = true
			}

			lhsTypeIsUnsafePointer[i] = typeName == "unsafe.Pointer"

			if !anyTypeIsUnsafePointer && lhsTypeIsUnsafePointer[i] {
				anyTypeIsUnsafePointer = true
			}
		}

		// An INDEX-expression LHS (`mr.readers[0] = eofReader{}`, `m[k] = impl{}`) assigns to a
		// container ELEMENT: the interface-cast check must use the ELEMENT type — the type of the
		// whole index expression — not the container's root ident (`mr`), which both branches
		// above test and which can never be an interface (Go forbids indexing one). Without this
		// the concrete RHS is emitted bare and no GoImplement pair is recorded (io/multi.go
		// `mr.readers[0] = eofReader{}` → CS0029).
		if _, ok := lhs.(*ast.IndexExpr); ok {
			if lhsType := v.getType(lhs, false); lhsType != nil {
				isIfaceElem, isEmptyElem := isInterface(lhsType)
				lhsTypeIsInterface[i] = isIfaceElem && !isEmptyElem
			}
		}

		// A SELECTOR-expression LHS (`x.expr = impl`) also assigns to storage whose type belongs
		// to the WHOLE selector expression. Checking only the selected identifier (`expr`) misses
		// struct fields such as go/types operand.expr (an ast.Expr interface), so the concrete RHS
		// skips the interface adapter and pointer-shaped implementations feed the adapter a value
		// alias instead of the pointer box.
		if _, ok := lhs.(*ast.SelectorExpr); ok {
			if lhsType := v.getType(lhs, false); lhsType != nil {
				isIfaceField, isEmptyField := isInterface(lhsType)
				lhsTypeIsInterface[i] = isIfaceField && !isEmptyField
			}
		}
	}

	// Lift an anonymous-struct composite-literal RHS up front (named after the LHS var) when the
	// local escapes to the heap. The heap declaration (`ref var x = ref heap<T>(…)`) renders its
	// box type explicitly via convertToHeapTypeDecl, which runs BEFORE the RHS composite would
	// normally trigger the lift — so without this the box type emits a raw, un-compilable
	// `struct{…}` (e.g. runtime/mpagealloc's `firstFree := struct{…}{…}` whose address is taken).
	// Mirrors convCompositeLit's lift; the liftedTypeExists guard makes the later one a no-op.
	if assignStmt.Tok == token.DEFINE {
		for i, lhs := range lhsExprs {
			if i >= rhsLen {
				break
			}

			lhsIdent := getIdentifier(lhs)

			if lhsIdent == nil || isDiscardedVar(lhsIdent.Name) {
				continue
			}

			if _, isSel := lhs.(*ast.SelectorExpr); isSel {
				continue
			}

			obj := v.info.ObjectOf(lhsIdent)

			if obj == nil || !v.identEscapesHeap[obj] {
				continue
			}

			if compositeLit, ok := rhsExprs[i].(*ast.CompositeLit); ok {
				if structType, exprType := v.extractStructType(compositeLit.Type); structType != nil && !v.liftedTypeExists(structType) {
					var indentOffset int

					if v.inFunction {
						indentOffset = 1
					}

					v.indentLevel += indentOffset
					v.visitStructType(structType, exprType, v.getIdentName(lhsIdent), nil, true, nil)
					v.indentLevel -= indentOffset
				}
			}
		}
	}

	// Map Go tokens to C# string equivalents
	var operator string

	// andNotUncheckedClose closes the `unchecked((Type)~` wrapper opened by the operator for a
	// narrow/unsigned `&^=` (see the AND_NOT_ASSIGN case). The matching `)` is written immediately
	// after the RHS at every operator-emission site (each statement uses exactly one of them).
	andNotUncheckedClose := false

	switch assignStmt.Tok {
	case token.ADD_ASSIGN:
		operator = " += "
	case token.SUB_ASSIGN:
		operator = " -= "
	case token.MUL_ASSIGN:
		operator = " *= "
	case token.QUO_ASSIGN:
		operator = " /= "
	case token.REM_ASSIGN:
		operator = " %= "
	case token.AND_ASSIGN:
		operator = " &= "
	case token.OR_ASSIGN:
		operator = " |= "
	case token.XOR_ASSIGN:
		operator = " ^= "
	case token.SHL_ASSIGN:
		operator = " <<= "
	case token.SHR_ASSIGN:
		operator = " >>= "
	case token.AND_NOT_ASSIGN:
		// C# doesn't have a direct AND NOT equivalent, so expand `&^=` to `&= ~`. The `~` promotes
		// its operand to `int`, and `int` is not implicitly convertible to a narrower or unsigned LHS
		// type (byte/ushort/uint/ulong/…), so `flags &= ~X` is CS0266. Cast the complemented RHS back
		// to the LHS type for those: `flags &= unchecked((uint8)~X)`. The cast is `unchecked` because
		// for a CONSTANT operand `~X` folds to a negative `int` constant whose checked narrowing
		// overflows (CS0221). Types that `int` widens to implicitly (int/int32/int64) need no cast.
		operator = " &= ~"

		if len(assignStmt.Lhs) == 1 {
			if lhsType := v.info.TypeOf(assignStmt.Lhs[0]); lhsType != nil {
				if basic, ok := lhsType.Underlying().(*types.Basic); ok && basic.Info()&types.IsInteger != 0 {
					switch basic.Kind() {
					case types.Int, types.Int32, types.Int64:
						// int widens to these implicitly; no cast needed
					default:
						operator = fmt.Sprintf(" &= unchecked((%s)~", convertToCSTypeName(v.getAliasQualifiedTypeName(lhsType, false)))
						andNotUncheckedClose = true
					}
				}
			}
		}
	default:
		operator = " = "
	}

	bitwiseAssignOp := assignStmt.Tok == token.AND_ASSIGN ||
		assignStmt.Tok == token.OR_ASSIGN ||
		assignStmt.Tok == token.XOR_ASSIGN ||
		assignStmt.Tok == token.SHL_ASSIGN ||
		assignStmt.Tok == token.SHR_ASSIGN ||
		assignStmt.Tok == token.AND_NOT_ASSIGN

	// A for-loop init clause is a single `;`-free clause: multiple declarations of differing
	// types cannot be emitted as `;`-separated decls (the `;` terminates the clause), and the
	// combined `var (a, b) = ...` form is unavailable when the types differ (mixed int/pointer,
	// etc.). Detect a multi-variable, all-new for-init that does not need any heap box so it can
	// be emitted as a single tuple-deconstruction declaration `(nint i, var e) = (..., ...)`.
	forInitTupleDecl := format.forInit && lhsLen > 1 && lhsLen == rhsLen && reassignedCount == 0
	if forInitTupleDecl {
		for i := range lhsLen {
			if ident := getIdentifier(lhsExprs[i]); ident == nil || ident.Name == "_" || v.convertToHeapTypeDecl(ident, false) != "" {
				forInitTupleDecl = false
				break
			}
		}
	}

	// A genuine parallel assignment (`rhsLen == lhsLen`) that MIXES declared and reassigned targets is
	// SIMULTANEOUS in Go — every RHS is evaluated before any store (Go spec, two-phase assignment) — but
	// sequential C# emission writes an earlier target before a LATER RHS reads it, corrupting that read.
	// strconv's Ryū `dc, fracc := dc>>extra, dc&extraMask` is exactly this: sequential emission shifts dc
	// first, so `fracc` reads the SHIFTED dc and the rounding residual is wrong (FloatMinMax rounds down).
	// Route such a read-after-write hazard through the tuple deconstruction path below, where C# evaluates
	// the whole RHS tuple before deconstructing. The existing swap fix covers the PURE-reassigned parallel
	// case (`lhsLen == reassignedCount`); this covers the mixed declared/reassigned case. Scoped to the
	// actual hazard (a written target reused in a later RHS) so drift stays minimal, and it inherits the
	// same int/string/unsafe.Pointer exclusions as the declared branch (the tuple path cannot infer those
	// cleanly — a hazard on those narrow types keeps its explicit-typed sequential decls, a separate follow-up).
	// A newly-declared int/uint element cannot use `var` (the RHS may infer C# 32-bit `int` instead of
	// the Go-int-target `nint`/`nuint`); the per-element declared prefix below emits its explicit type
	// instead, so int hazards are NOT excluded here. String is excluded — an `@string`'s u8 span cannot
	// sit on a value-tuple LHS — as is unsafe.Pointer (rare; `var` would infer uintptr); those keep their
	// buggy-but-compiling sequential form, a documented parallel-hazard limitation.
	parallelHazard := rhsLen == lhsLen && lhsLen > 1 && reassignedCount > 0 && declaredCount > 0 &&
		!anyTypeIsString && !anyTypeIsUnsafePointer && v.lhsReusedInLaterRhs(lhsExprs, rhsExprs)

	if tupleResult || lhsLen == reassignedCount || parallelHazard || lhsLen == declaredCount && !anyTypeIsString && !anyTypeIsInt && !anyTypeIsUnsafePointer {
		leftExprs := HashSet[string]{}

		// Go's partial redeclaration `a, b := f()` reuses any already-declared LHS variable and
		// declares only the new ones. A single blanket `var` prefix would re-declare the reused
		// variable (CS0136, and its earlier uses become "before declaration" CS0841), so for a
		// mixed declared/reassigned tuple, emit `var` per newly-declared element instead.
		mixedDeclare := declaredCount > 0 && reassignedCount > 0

		// A tuple element whose address is taken (`list, delta := netpoll(0); injectglist(&list)`)
		// must be heap-boxed so its `Ꮡlist` companion exists — the combined `var (list, delta) = …`
		// deconstruction cannot create it (a missing box → CS0103, and a `Ꮡ(value)` copy fallback
		// would silently lose writes through the pointer). Emit each escaping element's heap decl
		// (`ref var list = ref heap<gList>(out var Ꮡlist);`) first, then a mixed deconstruction-
		// ASSIGNMENT: the escaping element is the pre-declared ref-local (so the deconstructed value
		// lands in the box) and the rest declare with `var` — handled by the mixedDeclare path with a
		// per-element `escaping` skip. Only when at least one element escapes; otherwise unchanged.
		escaping := make([]bool, lhsLen)
		escapingHeapDecls := ""

		// Any `:=` tuple with a NEWLY-DECLARED escaping element — including a MIXED redeclaration
		// (`rbr2, err := r.makeReverseBitReader(…)` where only `err` is reused): the escaping
		// element is not counted in declaredCount (the heap-decl path owns it), so without this
		// pass it would never be declared at all (internal/zstd literals, CS0103 ×9). A REASSIGNED
		// element is skipped — its box already exists from its original declaration.
		if assignStmt.Tok == token.DEFINE && lhsLen > 1 {
			for i := range lhsExprs {
				if ident := getIdentifier(lhsExprs[i]); ident != nil && !v.isReassignment(ident) {
					if decl := v.convertToHeapTypeDecl(ident, false); decl != "" {
						escaping[i] = true
						escapingHeapDecls += decl + v.newline + v.indent(v.indentLevel)
					}
				}
			}
		}

		if escapingHeapDecls != "" {
			mixedDeclare = true

			// In a for-init clause the heap decls cannot precede the deconstruction INLINE — the
			// `;` after each decl becomes a fourth for-clause (CS1003; reflect/iter.go's
			// `for value, ok := v.Recv(); ok; …`). Route them to the for statement's hoist
			// target (visitForStmt's ForVarInitMarker mechanism), mirroring the single-variable
			// path below; the init clause then emits the pure mixed deconstruction-assignment
			// `(value, var ok) = …`.
			if format.heapTypeDeclTarget != nil {
				format.heapTypeDeclTarget.WriteString(strings.TrimSuffix(escapingHeapDecls, v.newline+v.indent(v.indentLevel)))
			} else {
				result.WriteString(escapingHeapDecls)
			}
		}

		// Handle LHS
		if declaredCount > 0 && !mixedDeclare {
			if declaredCount > 1 || v.options.preferVarDecl {
				isDiscarded := lhsLen == 1 && getIdentifier(lhsExprs[0]).Name == "_"

				// A single `:=` whose RHS references a NAMED UNTYPED constant materializes the
				// Go-inferred default type instead of `var` — the constant's `static readonly`
				// Untyped* wrapper would otherwise become the local's type (`rune codepoint =
				// unicode.ReplacementChar;`, see untypedConstDeclTypeName).
				untypedConstDeclType := ""

				if !isDiscarded && lhsLen == 1 && rhsLen == 1 {
					untypedConstDeclType = v.untypedConstDeclTypeName(getIdentifier(lhsExprs[0]), rhsExprs[0])
				}

				if untypedConstDeclType != "" {
					result.WriteString(untypedConstDeclType)
					result.WriteRune(' ')
				} else if !isDiscarded {
					result.WriteString("var ")
				}
			} else {
				ident := getIdentifier(lhsExprs[0])
				lhsType := convertToCSTypeName(v.getExpressionTypeName(ident, false))
				result.WriteString(lhsType)
				result.WriteRune(' ')
			}
		}

		if lhsLen > 1 {
			result.WriteRune('(')
		}

		lambdaContext := DefaultLambdaContext()
		lambdaContext.isAssignment = true

		for i, lhs := range lhsExprs {
			if i > 0 {
				result.WriteString(", ")
			}

			ident := getIdentifier(lhsExprs[i])
			context := DefaultIdentContext()

			if (!v.isPointer(ident) || v.identIsParameter(ident)) && i < rhsLen {
				// If rhs is a address of expression, we need to convert identifier to its pointer variable.
				// NOT when the LHS is an INTERFACE local: `r = &rb` with `var r ResourceBody` converts
				// the RHS through the pointer-interface adapter (the adapter IS the interface value), so
				// the assignment stays plain — the pointer-box LHS form referenced a nonexistent `Ꮡr`
				// and appended a ref re-alias against a non-ref local (dnsmessage's unpack switch,
				// CS0103/CS8373 x33).
				if unaryExpr, ok := rhsExprs[i].(*ast.UnaryExpr); ok {
					if unaryExpr.Op == token.AND {
						if lhsIsIface, _ := isInterface(v.getType(lhsExprs[i], false)); !lhsIsIface {
							context.isPointer = true
						}
					}
				}
			}

			// rhsElemIsPointer reports whether the RHS value feeding THIS element is pointer-typed —
			// element-wise for a multi-RHS assign, or the i-th component of the single call RHS's
			// result tuple for a tuple deconstruction (`(left, x, idx) = binarySearchTree(…)`,
			// runtime mgcstack.go / `(pp, _) = pidleget(0)`, proc.go — a pointer param reassigned
			// INSIDE a tuple otherwise misses the box-reassignment triggers below, assigning the
			// tuple's ж<T> component into the deref'd value alias → CS0029).
			rhsElemIsPointer := func(i int) bool {
				// A tuple DECONSTRUCTION (one call RHS, several LHS) probes the call's result
				// tuple for every element — including element 0, whose raw expression type is
				// the whole *types.Tuple (never a pointer), not its first component.
				if rhsLen == 1 && lhsLen > 1 {
					if tuple, ok := v.getExprType(rhsExprs[0]).(*types.Tuple); ok && i < tuple.Len() {
						return isPointer(tuple.At(i).Type())
					}

					return false
				}

				if i < rhsLen {
					// The untyped `nil` literal has no pointer type of its own, yet `p = nil` on a
					// pointer variable IS a pointer repoint. Without this the box-reassignment
					// triggers below missed it, the LHS rendered as the deref'd VALUE alias, and
					// `p = default!` ZEROED THE POINTEE while leaving the box `Ꮡp` non-nil — so
					// the caller's `p != nil` still passed and it walked a wiped-out struct
					// (regexp's `makeOnePass`, whose `p = nil` gave compileOnePass an
					// onePassProg with an empty Inst slice → index-out-of-range in cleanupOnePass).
					if argIsUntypedNil(rhsExprs[i], v.info) {
						return i < lhsLen && isPointer(v.getExprType(lhsExprs[i]))
					}

					return isPointer(v.getExprType(rhsExprs[i]))
				}

				return false
			}

			// Both box-reassignment triggers below apply to a REASSIGNED element only: a `:=`-DECLARED
			// pointer element binds the tuple's ж<T> component into a fresh pointer local — which IS
			// the box — directly (`var (cΔ1, …) = …`), and needs no repoint/re-alias. Without this
			// gate, an inner `:=` local shadowing a pointer parameter's name (`cΔ1`, Δ-renamed)
			// mis-took the parameter trigger and emitted a re-alias of an undeclared name
			// (crypto/x509 `c, _, err := …cert(i)`, net/http routing_tree).
			elemIsReassigned := ident != nil && v.isReassignment(ident)

			// Reassigning the direct-ж receiver to a pointer value (`r = r.prev`, a ring walk):
			// emit the box `Ꮡr` on the LHS so the pointer-reassignment path below repoints the box
			// and re-aliases the value var (`Ꮡr = r.prev; r = ref Ꮡr.Value;`). The deref'd value var
			// alone cannot be repointed at a different node.
			if elemIsReassigned && v.exprIsCurrentDirectBoxReceiver(lhsExprs[i]) && rhsElemIsPointer(i) {
				context.isPointer = true
			}

			// Reassigning a deref'd pointer PARAMETER to a new pointer value (`bits = addb(bits, n)`,
			// a *byte memory walk in the runtime). Every named pointer param is deref-aliased to a
			// value var (`ref var bits = ref Ꮡbits.Value`), which cannot be repointed; emit the box
			// `Ꮡbits` on the LHS so the pointer-reassignment path below repoints it and re-aliases the
			// value var (`Ꮡbits = addb(Ꮡbits, n); bits = ref Ꮡbits.Value;`). The RHS already references
			// the box form. (The `&`-RHS case above is a subset; setting isPointer twice is harmless.)
			if elemIsReassigned && v.identIsParameter(ident) && v.isPointer(ident) && rhsElemIsPointer(i) {
				context.isPointer = true
			}

			lhsElemContexts := []ExprContext{context, lambdaContext}

			// An index-expression LHS marks itself the assignment TARGET so its base takes the
			// `.Value` write path (`req.Header[k] = vv`; see IndexExprContext.isAssignmentTarget).
			if _, lhsIsIndex := lhs.(*ast.IndexExpr); lhsIsIndex {
				indexContext := DefaultIndexExprContext()
				indexContext.isAssignmentTarget = true
				lhsElemContexts = append(lhsElemContexts, indexContext)
			}

			lhsExpr := v.convExpr(lhs, lhsElemContexts)
			leftExprs.Add(lhsExpr)

			// Per-element `var` for the newly-declared members of a mixed redeclaration tuple. An
			// escaping element is already declared by its heap decl above, so it takes no `var`.
			// Only a DIRECT ident element can declare — an index/selector/star element assigns
			// to existing storage (`var hw[i]` is a syntax error; net mac.go).
			_, lhsIsDirectIdent := lhsExprs[i].(*ast.Ident)

			if mixedDeclare && lhsIsDirectIdent && ident != nil && ident.Name != "_" && !v.isReassignment(ident) && !escaping[i] {
				// A parallel-hazard int/uint element takes its EXPLICIT type: `var` would infer C# 32-bit
				// `int` from a literal/int32 RHS instead of the Go-int-target `nint`/`nuint` (see the
				// anyTypeIsInt note above), so `(a, b, nint c) = (b, a, a + b)` types c correctly. Scoped
				// to the hazard path so existing call-deconstruction mixed tuples keep their `var`.
				if parallelHazard && lhsTypeIsInt[i] {
					result.WriteString(convertToCSTypeName(v.getExpressionTypeName(ident, false)))
					result.WriteRune(' ')
				} else {
					result.WriteString("var ")
				}
			}

			result.WriteString(lhsExpr)
		}

		if lhsLen > 1 {
			result.WriteRune(')')
		}

		result.WriteString(operator)

		// A tuple DECONSTRUCTION (one call RHS, several LHS) whose call components need an
		// INTERFACE conversion the C# tuple assignment cannot perform implicitly — a pointer
		// component satisfies the target interface only through its generated adapter, an
		// EXPLICIT conversion (`(c, err) = Ꮡsd.dialTCP(…)` with `var c Conn`, net dial.go,
		// CS0266 ×11). Mirror visitReturnStmt's tuple-conversion arm: hoist the call into
		// temps, then convert each component (`var (ᴛ1, ᴛ2) = call; (c, err) = (~ᴛ1, ᴛ2);`).
		tupleConverted := false

		if rhsLen == 1 && lhsLen > 1 && v.hoistedDecls != nil {
			if tuple, ok := v.getType(rhsExprs[0], false).(*types.Tuple); ok && tuple.Len() == lhsLen {
				needsConversion := false

				for i := range tuple.Len() {
					if !lhsTypeIsInterface[i] {
						continue
					}

					declared := v.getType(lhsExprs[i], false)
					actual := tuple.At(i).Type()

					if declared != nil && !types.Identical(declared, actual) {
						if _, actualIsIface := actual.Underlying().(*types.Interface); !actualIsIface {
							needsConversion = true
							break
						}
					}
				}

				if needsConversion {
					tupleLambdaContext := DefaultLambdaContext()
					tupleLambdaContext.isAssignment = true
					callExpr := v.convExpr(rhsExprs[0], []ExprContext{tupleLambdaContext})

					tempNames := make([]string, lhsLen)

					for i := range lhsLen {
						tempNames[i] = fmt.Sprintf("%s%d", TempVarMarker, i+1)
					}

					v.hoistedDecls.WriteString(v.newline)
					v.hoistedDecls.WriteString(v.indent(v.indentLevel))
					v.hoistedDecls.WriteString(fmt.Sprintf("var (%s) = %s;", strings.Join(tempNames, ", "), callExpr))
					v.hoistedDecls.WriteString(v.newline)

					result.WriteRune('(')

					for i := range lhsLen {
						if i > 0 {
							result.WriteString(", ")
						}

						declared := v.getType(lhsExprs[i], false)
						actual := tuple.At(i).Type()
						converted := false

						if lhsTypeIsInterface[i] && declared != nil && !types.Identical(declared, actual) {
							if _, actualIsIface := actual.Underlying().(*types.Interface); !actualIsIface {
								result.WriteString(v.convertToInterfaceType(declared, actual, tempNames[i]))
								converted = true
							}
						}

						if !converted {
							result.WriteString(tempNames[i])
						}
					}

					result.WriteRune(')')
					tupleConverted = true
				}
			}
		}

		// Thread the statement's hoist buffer into the RHS conversion so an inner MULTI-VALUE call
		// spread into the enclosing call's parameters is deconstructed into temps and passed
		// expanded — convExprList's tuple-expand (`var (ᴛ1, …) = inner; f(ᴛ1, …)`) fires only when
		// deferredDecls != nil. Without it `r := t.newRange(t.parseControl("range"))` — parseControl
		// returns 5 values feeding newRange's 5 params — passed the whole tuple as ONE argument
		// (CS7036, text/template/parse). The return-form already hoists this way (visitReturnStmt);
		// mirror it for the `:=`/`=` single-value RHS path. Set after the LHS loop so LHS rendering
		// is untouched, and skipped when tupleConverted (that block hoists via v.hoistedDecls itself).
		if hoistBuf != nil && !tupleConverted {
			lambdaContext.deferredDecls = hoistBuf
		}

		// Handle RHS
		if !tupleConverted && rhsLen > 1 {
			result.WriteRune('(')
		}

		for i, rhs := range rhsExprs {
			if tupleConverted {
				break
			}
			var lhs ast.Expr

			if i < lhsLen {
				lhs = lhsExprs[i]
			} else {
				lhs = lhsExprs[lhsLen-1]
			}

			ident := getIdentifier(lhs)

			if i > 0 {
				result.WriteString(", ")
			}

			contexts := []ExprContext{lambdaContext}

			// A plain pointer-typed identifier on the RHS is a pointer copy (Go `r := p` /
			// `r = p` where p is *T): emit its pointer form so the target is a ж<T>, not a
			// copy of the pointed-to value. For a deref'd pointer parameter this yields the
			// box `Ꮡp` (without this it emits the value alias `p`, and the target — which the
			// rest of the converter treats as a pointer via `.Value`/`~` — fails to compile).
			// Interface targets need the same pointer form so the adapter wraps the pointer box,
			// not the dereferenced value alias.
			contexts = v.appendRhsPtrContext(contexts, rhs)

			// A u8 string literal is a ReadOnlySpan<byte> (ref struct) and cannot be
			// a ValueTuple element, so suppress the u8 form for string literals in a
			// multi-value (tuple) assignment, e.g. `field, env = env, ""`. An EMPTY-
			// interface target instead takes the boxed @string form `(@string)"…"`
			// (see lhsIsEmptyInterface) — in a tuple or standalone.
			emptyIfaceTarget := v.lhsIsEmptyInterface(lhs)

			if rhsLen > 1 || emptyIfaceTarget {
				basicLitContext := DefaultBasicLitContext()
				// The EMPTY-interface arm takes the combined `(@string)"…"u8` form — the cast
				// makes it an @string, which is a perfectly good ValueTuple element too, so the
				// arms compose. Only the BARE span is a ref struct, so the tuple arm alone
				// (`field, env = env, ""`) keeps u8 off.
				basicLitContext.u8StringOK = emptyIfaceTarget
				basicLitContext.castToGoString = emptyIfaceTarget
				basicLitContext.spanTargetUnsupported = true
				contexts = append(contexts, basicLitContext)
			}

			if selectorExpr, ok := rhs.(*ast.SelectorExpr); ok {
				// A POINTER-receiver method value over a VALUE receiver binds the receiver's
				// ADDRESS (Go's implicit `(&x).M`), so there is no copy to snapshot — see
				// methodValueBindsReceiverAddress. convSelectorExpr emits the method group over
				// the box for it, and a snapshot rename would make that box undeclared (CS0103).
				if v.isMethodValue(selectorExpr, false) && !v.methodValueBindsReceiverAddress(selectorExpr) {
					v.enterLambdaConversion(selectorExpr)
					defer v.exitLambdaConversion()

					// First prepare the captures
					v.prepareStmtCaptures(selectorExpr)

					// Then generate declarations. In this combined LHS/RHS block the LHS and the
					// `=` operator are ALREADY written to `result` by the time the RHS loop reaches
					// here, so writing the capture snapshot decl (`var nvcʗ1 = nvc;` — a statement)
					// into `result` splits the assignment (`checker = <newline> var nvcʗ1 = nvc;
					// <newline> (lambda)`, CS1002). Route it to the hoist buffer instead so it is
					// emitted BEFORE the whole statement (matching the `:=`-define path, which hoists
					// method-value captures the same way). The reassignment block below can write to
					// `result` directly because there it precedes the LHS.
					if decls := v.generateCaptureDeclarations(); decls != "" {
						if hoistBuf != nil {
							hoistBuf.WriteString(decls)
						} else {
							result.WriteString(decls)
						}
					}
				}
			}

			if _, ok := rhs.(*ast.CompositeLit); ok {
				if ident != nil {
					// Track the name of the variable on the LHS for composite literals,
					// this is needed for sparse array initializations
					keyValueContext := DefaultKeyValueContext()
					keyValueContext.ident = ident
					contexts = append(contexts, keyValueContext)
				}
			}

			if tupleResult {
				tupleResultContext := DefaultUnaryExprContext()
				tupleResultContext.isTupleResult = tupleResult
				contexts = append(contexts, tupleResultContext)

				// A comma-ok map access (`v, ok := m[k]`) reaches convIndexExpr, which
				// needs the tuple-result flag to emit the two-value indexer `m[key, ꟷ]`.
				indexResultContext := DefaultIndexExprContext()
				indexResultContext.isTupleResult = true
				contexts = append(contexts, indexResultContext)
			}

			rhsExpr := v.convExpr(rhs, v.appendNilArrayDimsContext(contexts, rhs, lhs))

			if assignStmt.Tok == token.DEFINE && ident != nil && !v.isReassignment(ident) {
				rhsExpr = v.materializeDefaultTypedConstDeclValue(ident, rhs, rhsExpr)
			}

			// Box an untyped CONSTANT assigned to an EMPTY-interface LHS at Go's default type for its
			// kind (the numeric twin of the castToGoString @string boxing above), so a later `x.(int)` /
			// `case int:` matches Go's boxed dynamic type. emptyIfaceTarget already reports the
			// LHS is `any`; the downstream narrow/named casts are gated off for an interface LHS.
			if emptyIfaceTarget {
				rhsExpr = v.applyUntypedConstBoxCast(rhs, rhsExpr)

				// The POINTER twin: a pointer assigned into an `any` LHS carries its Go type
				// across even when nil (see typedNilInterfaceBoxing.go). appendRhsPtrContext has
				// already rendered it as the BOX rather than a deref alias.
				rhsExpr = v.applyTypedNilPointerBox(rhs, rhsExpr)
			}

			// A `:=` DECLARATION whose RHS is a constant-folded NAMED-NUMERIC conversion
			// (`p := printFlags(0)` renders the bare `0`) loses the type the declaration
			// infers from — re-impose the named cast (regexp/syntax writeRegexp's p,
			// CS1503 ×2). The same fold is harmless in already-typed positions.
			if assignStmt.Tok == token.DEFINE && ident != nil && !v.isReassignment(ident) {
				if callExpr, ok := rhs.(*ast.CallExpr); ok {
					if tv, ok := v.info.Types[rhs]; ok && tv.Value != nil {
						if isConv, _ := v.isTypeConversion(callExpr); isConv {
							if named, ok := types.Unalias(tv.Type).(*types.Named); ok {
								if _, isBasic := named.Underlying().(*types.Basic); isBasic {
									namedCS := v.getCSharpTypeName(named)

									if !strings.HasPrefix(rhsExpr, "(("+namedCS+")") && !strings.HasPrefix(rhsExpr, "("+namedCS+")") && !strings.HasPrefix(rhsExpr, "new ") {
										rhsExpr = fmt.Sprintf("((%s)%s)", namedCS, rhsExpr)
									}
								}
							}
						}
					}
				}
			}

			// A Go array copied by value out of existing storage takes golib's `.Clone()` so the
			// target gets independent backing storage (see cloneValueCopy).
			rhsExpr = v.cloneValueCopy(lhs, rhs, rhsExpr)

			// A directional channel LHS taking a bidirectional value narrows the Go type with no
			// construction to hook (`s = ch`) — the assignment arm of the live-copy narrowing
			// (see chanDirNarrowedValue). A `:=` define is never a narrowing: the LHS type IS the
			// RHS type, so chanDirCargoName finds no direction and this is a no-op.
			if narrowedChan := v.chanDirNarrowedValue(v.info.TypeOf(lhs), v.info.TypeOf(rhs), rhsExpr); narrowedChan != "" {
				rhsExpr = narrowedChan
			}

			// Narrow-integer arithmetic RHS assigned to a narrow LHS needs a cast back to the LHS
			// type (see narrowArithmeticCastType). The existing bitwise-assign / `&^=` wrappers take
			// precedence (their own cast already applies).
			var narrowCastType string

			if !bitwiseAssignOp && !andNotUncheckedClose && i < lhsLen && !lhsTypeIsInterface[i] {
				narrowCastType = v.narrowArithmeticCastType(lhsExprs[i], rhs, rhsExpr)

				if len(narrowCastType) == 0 {
					// A computed CONSTANT arithmetic RHS assigned to a native-width integer LHS whose folded
					// value overflows int32 is emitted as a C# `long` (`144115188075855872L - 1`, runtime
					// mbitmap's `pattern = 1<<maxBits - 1`); `long` has no implicit conversion to the
					// `uintptr`/`nuint`/`nint` target (CS0266). Cast it back through the same wrapper
					// (mutually exclusive with the narrow-int cast — narrow targets are sub-int).
					narrowCastType = v.nativeIntConstCastType(lhsExprs[i], rhs, rhsExpr)
				}
			}

			var binaryTypeName string

			if bitwiseAssignOp {
				if assignStmt.Tok == token.SHL_ASSIGN || assignStmt.Tok == token.SHR_ASSIGN {
					// The shift count in a C# compound shift-assignment must be `int`;
					// casting it to the RHS's own (possibly unsigned or native-width)
					// type — e.g. `y <<= (nuint)s` — is rejected with CS0019.
					binaryTypeName = "int"
				} else {
					binaryType := v.info.Types[rhs].Type

					if binaryType != nil {
						binaryTypeName = convertToCSTypeName(v.getAliasQualifiedTypeName(binaryType, false))
					}
				}
			}

			if len(binaryTypeName) > 0 {
				result.WriteString(fmt.Sprintf("(%s)(", binaryTypeName))
			}

			if len(narrowCastType) > 0 {
				result.WriteString(fmt.Sprintf("(%s)(", narrowCastType))
			}

			if lhsTypeIsInterface[i] {
				result.WriteString(v.convertExprToInterfaceType(lhsExprs[i], rhs, rhsExpr))
			} else {
				result.WriteString(rhsExpr)
			}

			if len(narrowCastType) > 0 {
				result.WriteRune(')')
			}

			if len(binaryTypeName) > 0 {
				result.WriteRune(')')
			}

			if andNotUncheckedClose {
				// Close the `unchecked((Type)~` wrapper opened by the narrow `&^=` operator.
				result.WriteRune(')')
			}
		}

		if !tupleConverted && rhsLen > 1 {
			result.WriteRune(')')
		}

		if format.includeSemiColon {
			result.WriteRune(';')
		}

		if len(leftExprs) > 0 && operator == " = " {
			// Sorted for deterministic output: leftExprs is a map-backed set, so a multi-assign
			// re-aliasing several boxes (`(Ꮡx, Ꮡy) = (Ꮡy, Ꮡx)`) emitted its independent
			// `n = ref Ꮡn.Value` refreshers in an order that flipped run-to-run.
			sortedLeftExprs := leftExprs.Keys()
			sort.Strings(sortedLeftExprs)

			// The deref ref-local re-alias (`p = ref Ꮡp.Value`) rebinds a `ref var` local. Inside a
			// lambda that is illegal: the re-aliased value var is ALWAYS an enclosing captured ref-local
			// (a lambda's own pointer param is passed as the box `ж<T>`, never deref-aliased; a
			// heap-boxed value local is written THROUGH its box `Ꮡb.Value = …`, never box-repointed), and
			// C# forbids referencing an outer `ref` local inside a lambda (CS8175 — crypto/x509
			// buildChains' `considerCandidate` reassigns the captured `*int` param `sigChecks`). The box
			// reassignment `Ꮡp = …` was already emitted (it captures the box field, which is legal); only
			// the ref-local refresh is dropped. The outer value alias is then stale after the lambda runs,
			// but every in-lambda AND post-lambda deref of a repointed captured pointer routes through the
			// box `Ꮡp.Value`, so the alias is never read — an accepted modeling gap, not a miscompile.
			insideLambda := v.lambdaCapture != nil && v.lambdaCapture.conversionInLambda

			for _, leftExpr := range sortedLeftExprs {
				// Only a bare pointer-box reassignment (`Ꮡp = …`) needs the deref ref-local
				// re-aliased (`p = ref Ꮡp.Value`). A write *through* the box (`Ꮡp.Value = …`, emitted
				// for `*p = …` inside a lambda — see convStarExpr) has member access and must not
				// trigger this; it is a plain value assignment.
				if strings.HasPrefix(leftExpr, AddressPrefix) && !strings.Contains(leftExpr, ".") && !insideLambda {
					// This is a special case for pointer reassignments which should be extended
					// to also update local deference variable as well, e.g.: `x = ref Ꮡx.Value`
					boxBaseName := leftExpr[len(AddressPrefix):]
					derefExpr := getSanitizedIdentifier(boxBaseName)

					// A repointed box re-aliases the way its ENTRY alias binds — nil-DEFERRING, for
					// every box alike (see NilDeferringDerefAccessor). Repointing a pointer is not a
					// dereference in Go, so the re-alias must not fault: a walk that repoints its box
					// to the nil terminator (`for p != nil { …; p = p.next }`) re-aliases on the final
					// step BEFORE the loop guard re-checks, and the plain `.Value` getter threw a
					// nil-pointer dereference there. DerefOrNull binds a null ref instead, so a deref
					// of the repointed pointer still panics — at its own point, with Go's message —
					// while a repoint that is merely walked past costs nothing. Anything else would
					// leave the two halves of one alias disagreeing: entry says a nil pointer is legal
					// to hold, the re-alias would throw on it.
					derefAccessor := NilDeferringDerefAccessor

					// In a for-loop POST, the re-alias cannot follow the box-repoint in the
					// single post slot (`for(…; Ꮡscope = scope.Outer scope = ref …)` is a syntax
					// error) — stash it for visitForStmt to inject at the top of the loop body.
					if v.inForPost {
						v.forPostReAlias += fmt.Sprintf("%s = ref %s.%s;", derefExpr, leftExpr, derefAccessor)
					} else {
						result.WriteString(fmt.Sprintf(" %s = ref %s.%s;", derefExpr, leftExpr, derefAccessor))
					}
				}
			}
		}

	} else if forInitTupleDecl {
		// Emit a single tuple-deconstruction declaration with per-element types:
		// `(nint i, var e) = (other.Len(), other.Front())`.
		lambdaContext := DefaultLambdaContext()
		lambdaContext.isAssignment = true

		result.WriteRune('(')

		for i := range lhsLen {
			if i > 0 {
				result.WriteString(", ")
			}

			ident := getIdentifier(lhsExprs[i])

			// A string-underlying element declares with its EXPLICIT type via the general arm
			// below (never `var` — a u8 literal would infer ReadOnlySpan<byte>), which also
			// preserves a NAMED string type (the old hardcoded `@string` arm discarded it).
			if v.options.preferVarDecl && !(lhsTypeIsString[i] || lhsTypeIsInt[i] || lhsTypeIsUnsafePointer[i]) {
				result.WriteString("var ")
			} else {
				lhsType := convertToCSTypeName(v.getExpressionTypeName(ident, false))
				result.WriteString(lhsType)
				result.WriteRune(' ')
			}

			context := DefaultIdentContext()

			if (!v.isPointer(ident) || v.identIsParameter(ident)) && i < rhsLen {
				if unaryExpr, ok := rhsExprs[i].(*ast.UnaryExpr); ok && unaryExpr.Op == token.AND {
					context.isPointer = true
				}
			}

			result.WriteString(v.convExpr(lhsExprs[i], []ExprContext{context, lambdaContext}))
		}

		result.WriteString(") = (")

		for i, rhs := range rhsExprs {
			if i > 0 {
				result.WriteString(", ")
			}

			contexts := []ExprContext{lambdaContext}

			// A u8 string literal is a ReadOnlySpan<byte> (ref struct) and cannot be a
			// ValueTuple element, so suppress the u8 form for string literals here.
			basicLitContext := DefaultBasicLitContext()
			basicLitContext.u8StringOK = false
			basicLitContext.spanTargetUnsupported = true
			contexts = append(contexts, basicLitContext)

			rhsExpr := v.convExpr(rhs, v.appendNilArrayDimsContext(contexts, rhs, lhsExprs[i]))

			// A for-init tuple element declared from an existing array value takes golib's
			// `.Clone()` for independent backing storage (see cloneValueCopy).
			if i < lhsLen {
				rhsExpr = v.cloneValueCopy(lhsExprs[i], rhs, rhsExpr)
			}

			if i < lhsLen && lhsTypeIsInterface[i] {
				result.WriteString(v.convertExprToInterfaceType(lhsExprs[i], rhs, rhsExpr))
			} else {
				result.WriteString(rhsExpr)
			}
		}

		result.WriteRune(')')

		// No trailing semicolon: visitForStmt appends "; " after the init clause.
	} else {
		// Some variables are declared and some are reassigned, or one of the types is a string or integer
		for i := range lhsLen {
			lhs := lhsExprs[i]

			var rhs ast.Expr

			if i < rhsLen {
				rhs = rhsExprs[i]
			} else {
				rhs = rhsExprs[rhsLen-1]
			}

			lambdaContext := DefaultLambdaContext()
			lambdaContext.isAssignment = true

			// Same tuple-spread hoist as the single-declare block above: a `:=` whose RHS is a call
			// wrapping a MULTI-VALUE call (`r := t.newRange(t.parseControl("range"))`) reaches THIS
			// mixed/escaping branch when the declared local escapes the heap (so it is not counted in
			// declaredCount) — thread the hoist buffer so convExprList deconstructs the inner call into
			// temps instead of passing the whole tuple as one argument (CS7036, text/template/parse).
			if hoistBuf != nil {
				lambdaContext.deferredDecls = hoistBuf
			}

			if i > 0 {
				if format.useNewLine {
					result.WriteString(v.newline)
				}

				if format.useIndent {
					result.WriteString(v.indent(v.indentLevel))
				}
			}

			contexts := []ExprContext{lambdaContext}
			ident := getIdentifier(lhs)

			// A selector LHS is a plain assignment to existing storage, not a declaration — force
			// the `ident == nil` path so it is never prefixed with `var` (see the counting loop).
			if _, ok := lhs.(*ast.SelectorExpr); ok {
				ident = nil
			}

			// A `s := string(x)` element the escape pass proved may become a stack-only sstring (see
			// markSStringEligible): its explicit LHS type below becomes sstring, and its RHS conversion
			// is steered (via emitStringConvAsSString, consumed by convCallExpr) to the zero-copy
			// `(sstring)x` view instead of the heap `(@string)x` copy.
			elemIsSString := false

			if ident != nil {
				if obj := v.info.ObjectOf(ident); obj != nil && v.sstringEligible[obj] {
					elemIsSString = true
				}
			}

			if selectorExpr, ok := rhs.(*ast.SelectorExpr); ok {
				// A POINTER-receiver method value over a VALUE receiver binds the receiver's
				// ADDRESS (Go's implicit `(&x).M`), so there is no copy to snapshot — see
				// methodValueBindsReceiverAddress. convSelectorExpr emits the method group over
				// the box for it, and a snapshot rename would make that box undeclared (CS0103).
				if v.isMethodValue(selectorExpr, false) && !v.methodValueBindsReceiverAddress(selectorExpr) {
					v.enterLambdaConversion(selectorExpr)
					defer v.exitLambdaConversion()

					// First prepare the captures
					v.prepareStmtCaptures(selectorExpr)

					// Then generate declarations
					if decls := v.generateCaptureDeclarations(); decls != "" {
						result.WriteString(decls)
					}
				}
			}

			if _, ok := rhs.(*ast.CompositeLit); ok {
				if ident != nil {
					// Track the name of the variable on the LHS for composite literals,
					// this is needed for sparse array initializations
					keyValueContext := DefaultKeyValueContext()
					keyValueContext.ident = ident
					contexts = append(contexts, keyValueContext)
				}
			}

			// An index-expression LHS marks itself the assignment TARGET so its base takes the
			// `.Value` write path (see IndexExprContext.isAssignmentTarget). Prepended into a
			// FRESH slice — `contexts` is reused for the RHS conversions below, where an index
			// READ must keep the deref form.
			lhsContexts := contexts

			if _, lhsIsIndex := lhs.(*ast.IndexExpr); lhsIsIndex {
				indexContext := DefaultIndexExprContext()
				indexContext.isAssignmentTarget = true
				lhsContexts = append([]ExprContext{indexContext}, contexts...)
			}

			if ident == nil {
				if _, ok := lhs.(*ast.StarExpr); ok {
					starExprContext := DefaultStarExprContext()
					starExprContext.inLhsAssign = true
					contexts = append(contexts, starExprContext)
					lhsContexts = append(lhsContexts, starExprContext)
				}

				result.WriteString(v.convExpr(lhs, lhsContexts))
				result.WriteString(operator)

				rhsExpr := v.convExpr(rhs, v.appendNilArrayDimsContext(v.appendEmptyIfaceLitContext(v.appendRhsPtrContext(contexts, rhs), lhs), rhs, lhs))

				// Box an untyped CONSTANT assigned to an EMPTY-interface LHS at Go's default type (the
				// numeric twin of appendEmptyIfaceLitContext's @string boxing); a no-op for a non-empty
				// or non-interface LHS and for any non-constant RHS. The POINTER twin follows it — a
				// nil pointer carries its Go type across (typedNilInterfaceBoxing.go).
				rhsExpr = v.boxUntypedConstAsDefaultType(v.getType(lhs, false), rhs, rhsExpr)
				rhsExpr = v.boxPointerIntoEmptyInterface(v.getType(lhs, false), rhs, rhsExpr)

				// A Go array copied by value into existing storage (`s.arr = a` / `*p = a`) takes
				// golib's `.Clone()` for independent backing storage (see cloneValueCopy).
				rhsExpr = v.cloneValueCopy(lhs, rhs, rhsExpr)

				// A C# compound shift-assign requires an `int` shift count; the RHS's own (possibly
				// unsigned/native-width) type — `s.allocCache >>= (nuint)x` — is rejected (CS0019). A
				// selector/pointer-field LHS routes through this block (its base ident is nil'd in the
				// counting loop), so it needs the same `(int)` cast the simple-variable path applies.
				shiftAssignCast := assignStmt.Tok == token.SHL_ASSIGN || assignStmt.Tok == token.SHR_ASSIGN

				// A narrow-integer arithmetic RHS assigned to a narrow struct-field LHS (a pure
				// selector, e.g. `it.i = i + 1`) routes through this block, so it needs the same narrow
				// cast as the var/element forms above. A shift-assign uses the `(int)` cast instead.
				var narrowCastType string

				if !shiftAssignCast && !andNotUncheckedClose && !lhsTypeIsInterface[i] {
					narrowCastType = v.narrowArithmeticCastType(lhs, rhs, rhsExpr)
				}

				if shiftAssignCast {
					result.WriteString("(int)(")
				}

				if len(narrowCastType) > 0 {
					result.WriteString(fmt.Sprintf("(%s)(", narrowCastType))
				}

				if lhsTypeIsInterface[i] {
					result.WriteString(v.convertExprToInterfaceType(lhs, rhs, rhsExpr))
				} else {
					result.WriteString(rhsExpr)
				}

				if len(narrowCastType) > 0 {
					result.WriteRune(')')
				}

				if shiftAssignCast {
					result.WriteRune(')')
				}

				if andNotUncheckedClose {
					// Close the `unchecked((Type)~` wrapper opened by the narrow `&^=` operator.
					result.WriteRune(')')
				}

				if format.includeSemiColon || i < lhsLen-1 {
					result.WriteRune(';')
				}
			} else {
				if v.isReassignment(ident) {
					result.WriteString(v.convExpr(lhs, lhsContexts))
					result.WriteString(operator)

					rhsExpr := v.convExpr(rhs, v.appendNilArrayDimsContext(v.appendEmptyIfaceLitContext(v.appendRhsPtrContext(contexts, rhs), lhs), rhs, lhs))

					// Box an untyped CONSTANT reassigned to an EMPTY-interface LHS at Go's default type
					// (twin of appendEmptyIfaceLitContext's @string boxing); a no-op otherwise. The
					// POINTER twin follows it (typedNilInterfaceBoxing.go).
					rhsExpr = v.boxUntypedConstAsDefaultType(v.getType(lhs, false), rhs, rhsExpr)
					rhsExpr = v.boxPointerIntoEmptyInterface(v.getType(lhs, false), rhs, rhsExpr)

					// A Go array copied by value over an existing variable takes golib's `.Clone()`
					// for independent backing storage (see cloneValueCopy).
					rhsExpr = v.cloneValueCopy(lhs, rhs, rhsExpr)

					if lhsTypeIsInterface[i] {
						result.WriteString(v.convertExprToInterfaceType(lhs, rhs, rhsExpr))
					} else {
						result.WriteString(rhsExpr)
					}

					if andNotUncheckedClose {
						// Close the `unchecked((Type)~` wrapper opened by the narrow `&^=` operator.
						result.WriteRune(')')
					}

					if format.includeSemiColon || i < lhsLen-1 {
						result.WriteRune(';')
					}
				} else {
					// A blank-identifier LHS is a C# DISCARD, never a declaration, so no arm below may
					// name a type for it: `stateFn _ = lexText;` declares a local literally named `_`,
					// which makes every OTHER `_ = x` in the same scope an assignment to that local
					// (CS0841 before it, CS0123/CS0029 after) and collides outright with a second one
					// (CS0128). Deciding it here rather than inside one arm also keeps each `_` in a
					// split multi-assign like `_, _, _, _ = a, b, c, d` a discard.
					isDiscarded := ident.Name == "_"

					// A discard infers its type FROM its RHS, so a func-typed RHS with no C# type of
					// its own needs one supplied (CS8183) — see discardTargetTypeName.
					discardCastType := ""

					if isDiscarded {
						discardCastType = v.discardTargetTypeName(rhs)
					}

					// Check if the variable needs to be allocated on the heap
					heapTypeDecl := v.convertToHeapTypeDecl(ident, false)

					if len(heapTypeDecl) > 0 {
						if format.heapTypeDeclTarget == nil {
							result.WriteString(heapTypeDecl)
							result.WriteString(v.newline)
							result.WriteString(v.indent(v.indentLevel))
						} else {
							format.heapTypeDeclTarget.WriteString(heapTypeDecl)
						}
					} else {
						// A `:=` declaration initialized from a bare function/method reference (a C#
						// METHOD GROUP, e.g. `state := lexText`) cannot be typed with `var` — C# cannot
						// infer a delegate type from a method group (CS8917). Go infers the local's type
						// as the UNNAMED signature `func(*lexer) stateFn`; naming it structurally emits a
						// `Func<…>` delegate that is a DISTINCT C# type from the `stateFn` delegate the
						// method group produces and that later `state = state(l)` assignments yield
						// (CS0029). When the signature matches a package named func type, declare the
						// local with that named delegate so the method group binds and same-type
						// reassignments stay interconvertible. See namedFuncTypeNameForSignature.
						methodGroupDelegateType := ""

						if !isDiscarded && !v.isReassignment(ident) && v.exprIsMethodGroup(rhs) {
							if sig, ok := v.getExprType(rhs).(*types.Signature); ok {
								methodGroupDelegateType = v.namedFuncTypeNameForSignature(sig)

								// A VARIADIC signature with no named func type of its own cannot be
								// left to `var` either: the RHS renders as a forwarding lambda whose
								// tail is `params ꓸꓸꓸT`, and an inferred natural delegate type binds
								// only THAT lambda — the reassignment one statement later (`errorf =
								// t.Logf`, slices' TestGrow/TestConcat) is a second lambda C# has no
								// reason to give the same type. Name golib's variadic delegate family
								// instead (`Actionꓸꓸꓸ<@string, any>`), which both lambdas convert to
								// and which carries the loose-argument call form — the same lowering
								// getCSharpTypeName already gives every func type used as a value.
								// Non-variadic signatures keep `var`, unchanged.
								if methodGroupDelegateType == "" && sig.Variadic() && sig.Params().Len() > 0 {
									methodGroupDelegateType = v.iifeDelegateType(sig)
								}
							}
						}

						// A string-underlying LHS always declares with its EXPLICIT type: `var`
						// would infer ReadOnlySpan<byte> from a u8 literal, and the explicit form
						// preserves a NAMED string type (`goVersion fileVersion = asGoVersion(…)`,
						// go/types check.go — the old hardcoded `@string` branch discarded it,
						// CS1929 ×4, and bypassed the heap-box check above, CS0103).
						if methodGroupDelegateType != "" {
							result.WriteString(methodGroupDelegateType)
							result.WriteRune(' ')
						} else if isDiscarded {
							// `_ = x;` — no declaration of any kind (see isDiscarded above).
						} else if v.options.preferVarDecl && !(lhsTypeIsString[i] || lhsTypeIsInt[i] || lhsTypeIsUnsafePointer[i]) {
							// Same untyped-const materialization as the all-declared path (see
							// untypedConstDeclTypeName) for a define routed through this mixed branch.
							untypedConstDeclType := v.untypedConstDeclTypeName(ident, rhs)

							if untypedConstDeclType != "" {
								result.WriteString(untypedConstDeclType)
								result.WriteRune(' ')
							} else {
								result.WriteString("var ")
							}
						} else if elemIsSString {
							result.WriteString("sstring ")
						} else {
							lhsType := convertToCSTypeName(v.getExpressionTypeName(ident, false))
							result.WriteString(lhsType)
							result.WriteRune(' ')
						}
					}

					result.WriteString(v.convExpr(lhs, contexts))
					result.WriteString(operator)

					// Steer an eligible stack-string's RHS conversion to the zero-copy (sstring)x view.
					// convCallExpr consumes the flag on the one string(x) conversion; the reset after is
					// belt-and-suspenders so it cannot leak to a later element or statement.
					if elemIsSString {
						v.emitStringConvAsSString = true
					}

					rhsExpr := v.convExpr(rhs, v.appendRhsPtrContext(contexts, rhs))

					v.emitStringConvAsSString = false

					if assignStmt.Tok == token.DEFINE && !v.isReassignment(ident) {
						rhsExpr = v.materializeDefaultTypedConstDeclValue(ident, rhs, rhsExpr)
					}

					// A `:=`-declared local initialized from an existing array value takes golib's
					// `.Clone()` for independent backing storage (see cloneValueCopy).
					rhsExpr = v.cloneValueCopy(lhs, rhs, rhsExpr)

					// A directional channel LHS taking a bidirectional value narrows the Go type
					// with no construction to hook (`s = ch`), so the direction is re-stamped onto
					// the value here — the assignment arm of the live-copy narrowing (see
					// chanDirNarrowedValue). A `:=` define is never a narrowing: the LHS type IS
					// the RHS type, so chanDirCargoName finds no direction and this is a no-op.
					if narrowedChan := v.chanDirNarrowedValue(v.info.TypeOf(lhs), v.info.TypeOf(rhs), rhsExpr); narrowedChan != "" {
						rhsExpr = narrowedChan
					}

					// Give a discarded method group or lambda the target type C# cannot infer. The
					// parentheses are load-bearing for the lambda form: `(Func<…>)(nint p1) => …`
					// does not parse as a cast.
					if discardCastType != "" {
						rhsExpr = fmt.Sprintf("(%s)(%s)", discardCastType, rhsExpr)
					}

					_, rhsIsTypeAssert := rhs.(*ast.TypeAssertExpr)

					if lhsTypeIsInterface[i] && !rhsIsTypeAssert {
						result.WriteString(v.convertExprToInterfaceType(lhs, rhs, rhsExpr))
					} else {
						result.WriteString(rhsExpr)
					}

					if format.includeSemiColon || i < lhsLen-1 {
						result.WriteRune(';')
					}
				}
			}
		}
	}

	if hoistBuf != nil && hoistBuf.Len() > 0 {
		// The hoisted decls carry their own leading newline + per-line indentation.
		v.outputBuilder.WriteString(hoistBuf.String())
	} else if format.useNewLine {
		v.outputBuilder.WriteString(v.newline)
	}

	if format.useIndent {
		v.outputBuilder.WriteString(v.indent(v.indentLevel))
	}

	v.outputBuilder.WriteString(result.String())
}

// lhsReusedInLaterRhs reports whether a WRITTEN left-hand identifier of a parallel assignment appears in
// a LATER right-hand expression — the read-after-write hazard that makes sequential C# emission diverge
// from Go's simultaneous assignment (Go evaluates every RHS before any store; sequential C# writes the
// earlier target first, so the later RHS reads the already-updated value). Identifiers are matched by
// go/types object, so a same-named but distinct variable is never a false positive. Only earlier targets
// can be re-read (index j > i); a target reused in its OWN RHS (j == i) reads the same value under either
// ordering and is not a hazard.
func (v *Visitor) lhsReusedInLaterRhs(lhsExprs, rhsExprs []ast.Expr) bool {
	for i, lhs := range lhsExprs {
		ident := getIdentifier(lhs)

		if ident == nil || ident.Name == "_" {
			continue
		}

		obj := v.info.ObjectOf(ident)

		if obj == nil {
			continue
		}

		for j := i + 1; j < len(rhsExprs); j++ {
			reused := false

			ast.Inspect(rhsExprs[j], func(n ast.Node) bool {
				if reused {
					return false
				}

				if id, ok := n.(*ast.Ident); ok && v.info.ObjectOf(id) == obj {
					reused = true
					return false
				}

				return true
			})

			if reused {
				return true
			}
		}
	}

	return false
}
