// untypedConstAnalysisOperations.go - Gbtc
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
	"math"
	"strconv"
)

// performUntypedConstAnalysis records, for the given function, every FUNCTION-LOCAL untyped
// numeric constant whose declaration can be TIGHTENED to a single concrete basic type — the
// type is written into v.tightenedConsts and consulted by (a) the const declaration emission
// (visitValueSpec emits the concrete type, with C#'s `const` keyword where legal for it,
// instead of the `Untyped*` wrapper) and (b) every wrapper-keyed cast-insertion site
// (isUntypedNamedConstRef, isUntypedNumericConstArg, the deferred-call default-type wrap),
// which then skip their now-unneeded casts. The result reads like the Go source: math cbrt's
// `s := C + r*t` emits `var s = C + r * t;` over `const float64 C = …` instead of
// `var s = (float64)C + r * t;` over an `UntypedFloat` wrapper.
//
// A constant qualifies only when ALL of the following hold (conservative by design — any
// doubt keeps today's wrapper form, correctness beats beauty):
//
//   - It is declared inside the function (package-level consts are out of scope this pass)
//     with an untyped INTEGER/RUNE/FLOAT/COMPLEX type. (Untyped complex was excluded while
//     complex128 had no non-`const` emission; visitValueSpec now emits every representable
//     complex const as `static readonly` — the same demotion uintptr takes — and the wrapper it
//     replaces is actively WRONG here: UntypedComplex converts implicitly in BOTH directions, so
//     comparing a wrapper-typed const against a complex128 is AMBIGUOUS, CS0034 — strconv
//     atoc_test's TestParseComplexIncorrectBitSize `c != want`.)
//   - Its value fits the native numeric range — a value needing the GoBigConst (BigInteger)
//     emission is never tightened (mirrors visitValueSpec's writeUntypedConst triggers).
//   - EVERY use records a concrete (non-untyped) NUMERIC *types.Basic in
//     go/types' Info.Types, and all uses record the SAME basic kind. go/types records the
//     implicit-conversion target for an untyped operand (float64 for `C + x` with x float64),
//     the untyped type where the constant stays untyped (another const's initializer), and a
//     named/type-parameter type where the context demands one — the latter two disqualify.
//   - NO use participates in constant folding: an ANCESTOR expression carrying a folded
//     constant value (`uint64(B1) << 32`, `int64(ns) + int64(ns)`) disqualifies the constant.
//     Go folds untyped constant expressions at arbitrary precision; re-expressing the operand
//     at a concrete C# type could change the folded result (or re-fold it in C#'s checked
//     int32 arithmetic), so any folding participation keeps the wrapper form.
//   - The exact value is representable in the resolved type (belt-and-braces — go/types
//     already validated each use's conversion, so this can only re-confirm).
//
// The map is keyed by the *types.Const object (globally unique per load), so no per-function
// reset is needed; uses inside nested function literals are analyzed with their enclosing
// declared function.
func (v *Visitor) performUntypedConstAnalysis(funcDecl *ast.FuncDecl) {
	if funcDecl.Body == nil {
		return
	}

	// Pass 1: collect the function-local untyped numeric const objects still eligible.
	eligible := map[*types.Const]bool{}

	ast.Inspect(funcDecl.Body, func(n ast.Node) bool {
		decl, ok := n.(*ast.GenDecl)

		if !ok || decl.Tok != token.CONST {
			return true
		}

		for _, spec := range decl.Specs {
			valueSpec, ok := spec.(*ast.ValueSpec)

			if !ok {
				continue
			}

			for _, name := range valueSpec.Names {
				c, ok := v.info.Defs[name].(*types.Const)

				if !ok {
					continue
				}

				basic, ok := c.Type().(*types.Basic)

				if !ok || basic.Info()&types.IsUntyped == 0 || basic.Info()&types.IsNumeric == 0 {
					continue
				}

				if constNeedsGoBigConst(c.Val()) {
					continue
				}

				eligible[c] = true
			}
		}

		return true
	})

	if len(eligible) == 0 {
		return
	}

	// Pass 2: resolve every use against the eligibility rules, tracking the expression
	// ancestor chain so constant-folding participation is visible.
	resolved := map[*types.Const]*types.Basic{}
	var stack []ast.Node

	ast.Inspect(funcDecl.Body, func(n ast.Node) bool {
		if n == nil {
			stack = stack[:len(stack)-1]
			return true
		}

		stack = append(stack, n)

		ident, ok := n.(*ast.Ident)

		if !ok {
			return true
		}

		c, ok := v.info.Uses[ident].(*types.Const)

		if !ok {
			return true
		}

		if stillEligible, isCandidate := eligible[c]; !isCandidate || !stillEligible {
			return true
		}

		// The use must record a concrete numeric basic type.
		tv, hasType := v.info.Types[ident]
		basic, isBasic := tv.Type.(*types.Basic)

		if !hasType || !isBasic || basic.Info()&types.IsUntyped != 0 || basic.Info()&types.IsNumeric == 0 {
			eligible[c] = false
			return true
		}

		// A COMPLEX use type tightens only an untyped-COMPLEX const. Widening the other way —
		// re-declaring `const k = 2` (untyped int) as complex128 because one use is complex-typed —
		// would rewrite a perfectly good integer declaration into a struct for no gain, so that arm
		// stays closed. An untyped COMPLEX const has no other concrete form and its wrapper is
		// actively wrong (the two-way implicit conversion makes every comparison ambiguous).
		if basic.Info()&types.IsComplex != 0 {
			declared, isDeclaredBasic := c.Type().(*types.Basic)

			if !isDeclaredBasic || declared.Kind() != types.UntypedComplex {
				eligible[c] = false
				return true
			}
		}

		// No ancestor expression may itself be constant-folded (the ident is stack top;
		// expression ancestors are contiguous below it — the chain ends at the first
		// non-expression node).
		for i := len(stack) - 2; i >= 0; i-- {
			ancestor, isExpr := stack[i].(ast.Expr)

			if !isExpr {
				break
			}

			if atv, ok := v.info.Types[ancestor]; ok && atv.Value != nil {
				eligible[c] = false
				return true
			}
		}

		// All uses must agree on ONE basic kind.
		if prev, seen := resolved[c]; seen {
			if prev.Kind() != basic.Kind() {
				eligible[c] = false
			}
		} else {
			resolved[c] = basic
		}

		return true
	})

	for c, stillEligible := range eligible {
		target, hasUse := resolved[c]

		if !stillEligible || !hasUse || !constRepresentableAs(c.Val(), target) {
			continue
		}

		if v.tightenedConsts == nil {
			v.tightenedConsts = map[*types.Const]*types.Basic{}
		}

		v.tightenedConsts[c] = target
	}
}

// tightenedNarrowConstRef reports whether expr references a TIGHTENED function-local const
// whose concrete type is narrower than 32 bits (int8/int16/uint8/uint16). Such a reference
// still needs the shift-width retype when it is the LEFT operand of a shift: C# promotes a
// sub-int shifted operand to `int`, so without the result cast Go's wraparound at the
// declared width is lost (`const cb = 200; b + cb<<k` — Go wraps 200<<1 to 144 at byte
// width; the promoted C# shift yields 400). This is the one cast-insertion site where the
// suppressed cast was VALUE-CHANGING rather than made redundant by the concrete declaration
// — 32-bit-and-wider shift types compute at the declared C# width already. See the
// isUntypedNumericConstArg consult in convBinaryExpr's shift arm.
func (v *Visitor) tightenedNarrowConstRef(expr ast.Expr) bool {
	ident := getIdentifier(expr)

	if ident == nil {
		return false
	}

	constObj, ok := v.info.ObjectOf(ident).(*types.Const)

	if !ok {
		return false
	}

	target, tightened := v.tightenedConsts[constObj]

	if !tightened {
		return false
	}

	switch target.Kind() {
	case types.Int8, types.Int16, types.Uint8, types.Uint16:
		return true
	}

	return false
}

// constNeedsGoBigConst reports whether a constant value routes to visitValueSpec's GoBigConst
// (BigInteger) emission — an integer that fits neither int64 nor uint64, or a float beyond
// float64. EXACTLY mirrors the writeUntypedConst triggers (which parse the emitted value
// text), so a tightened const can never reach that path.
func constNeedsGoBigConst(val constant.Value) bool {
	switch val.Kind() {
	case constant.Int:
		s := val.ExactString()
		_, errUint := strconv.ParseUint(s, 0, 64)
		_, errInt := strconv.ParseInt(s, 0, 64)
		return errUint != nil && errInt != nil
	case constant.Float:
		f64, _ := constant.Float64Val(val)
		return math.IsInf(f64, 0)
	case constant.Complex:
		// Mirrors exactComplexConstString's representability test: each HALF must land inside
		// float64 (the emission renders and range-tests them independently).
		re, _ := constant.Float64Val(constant.Real(val))
		im, _ := constant.Float64Val(constant.Imag(val))
		return math.IsInf(re, 0) || math.IsInf(im, 0)
	}

	return false
}

// constRepresentableAs reports whether the constant value is representable in the concrete
// basic type per Go's conversion rules — exact for integer targets, rounding allowed (but no
// overflow) for float targets. go/types already validated each use-site conversion, so this
// is a belt-and-braces re-check, deliberately conservative: an unhandled kind returns false.
func constRepresentableAs(val constant.Value, target *types.Basic) bool {
	if target.Info()&types.IsInteger != 0 {
		intVal := constant.ToInt(val)

		if intVal.Kind() != constant.Int {
			return false
		}

		// The converter targets 64-bit platforms: int/uint/uintptr take the 64-bit ranges
		// (matching the emitted nint/nuint whose beyond-int32 constants visitValueSpec
		// already demotes to `static readonly unchecked((nint)…)`).
		if target.Info()&types.IsUnsigned != 0 {
			u, exact := constant.Uint64Val(intVal)

			if !exact {
				return false
			}

			switch target.Kind() {
			case types.Uint8:
				return u <= math.MaxUint8
			case types.Uint16:
				return u <= math.MaxUint16
			case types.Uint32:
				return u <= math.MaxUint32
			case types.Uint64, types.Uint, types.Uintptr:
				return true
			}

			return false
		}

		i, exact := constant.Int64Val(intVal)

		if !exact {
			return false
		}

		switch target.Kind() {
		case types.Int8:
			return i >= math.MinInt8 && i <= math.MaxInt8
		case types.Int16:
			return i >= math.MinInt16 && i <= math.MaxInt16
		case types.Int32:
			return i >= math.MinInt32 && i <= math.MaxInt32
		case types.Int64, types.Int:
			return true
		}

		return false
	}

	if target.Info()&types.IsFloat != 0 {
		switch target.Kind() {
		case types.Float32:
			f32, _ := constant.Float32Val(val)
			return !math.IsInf(float64(f32), 0)
		case types.Float64:
			f64, _ := constant.Float64Val(val)
			return !math.IsInf(f64, 0)
		}
	}

	if target.Info()&types.IsComplex != 0 {
		// Both halves must land inside the target's element width — the same per-half test the
		// complex emission applies (exactComplexConstString).
		switch target.Kind() {
		case types.Complex64:
			re, _ := constant.Float32Val(constant.Real(val))
			im, _ := constant.Float32Val(constant.Imag(val))
			return !math.IsInf(float64(re), 0) && !math.IsInf(float64(im), 0)
		case types.Complex128:
			re, _ := constant.Float64Val(constant.Real(val))
			im, _ := constant.Float64Val(constant.Imag(val))
			return !math.IsInf(re, 0) && !math.IsInf(im, 0)
		}
	}

	return false
}
