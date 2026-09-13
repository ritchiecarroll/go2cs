// untypedConstOperations.go - Gbtc
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

// markUntypedConstContexts records the CONTEXTUAL type of every untyped constant subexpression
// in the file. go/types resolves a constant expression's context (`var c complex64 = complex(2.5,
// -3.5)`) on the OUTERMOST expression only — updateExprType deliberately does not descend into
// constant operands (they never materialize at runtime in Go), so the inner literals stay recorded
// as `untyped float`/`untyped complex` and later render from their untyped DEFAULT (float64 →
// `2.5D`, no implicit conversion to float32/complex64 → CS0266/CS0019). The emitted C# preserves
// the operand structure (`complex(2.5F, -3.5F)`, `2.5F - 3.5F.i()`), so each literal needs
// the context go/types dropped: propagate a typed constant expression's resolved type down through
// the constant shapes — parens, unary `+`/`-`/`^`, arithmetic binary operands, and the complex/
// real/imag/min/max builtin arguments — into untypedConstContexts, which convBasicLit consults for
// the float32-vs-float64 (`F`/`D`) and complex64-vs-complex128 suffix decisions.
func (v *Visitor) markUntypedConstContexts(file *ast.File) {
	if v.untypedConstContexts == nil {
		v.untypedConstContexts = map[ast.Expr]types.Type{}
	}

	ast.Inspect(file, func(n ast.Node) bool {
		expr, ok := n.(ast.Expr)

		if !ok {
			return true
		}

		tv, ok := v.info.Types[expr]

		if !ok || tv.Value == nil || tv.Type == nil {
			return true
		}

		// Only a TYPED constant expression supplies context; an untyped one gets ITS context
		// from its own enclosing expression (assigned by the recursion below, not this trigger)
		if basic, ok := tv.Type.Underlying().(*types.Basic); ok && basic.Info()&types.IsUntyped == 0 {
			v.propagateUntypedConstContext(expr, tv.Type)
		}

		return true
	})
}

// propagateUntypedConstContext pushes a resolved constant context type into the untyped constant
// children of expr, per expr's shape. Float/complex contexts propagate because those are the kinds
// whose C# literal rendering is context-sensitive (`F`/`D` suffixes, builtin.i overload choice) —
// and INTEGER contexts propagate so a FLOAT literal inside a constant expression can render its
// integer form: `f(data, 3)` vs `1e9 - 7` (sort search_test's tests table, field `i int`) — the
// direct literal is typed `int` by go/types and convBasicLit already emits `1000000000`, but
// inside the operator expression the literal stays untyped float, rendered `1e9D`, making the
// whole element `double` against the `nint` field (CS1503). With the context propagated the
// literal renders `1000000000` and the arithmetic stays exact C# `int`. Division is EXCLUDED for
// integer contexts: Go evaluates an untyped-float constant `/` in exact rational arithmetic, so a
// nested quotient may be non-integral (`3.0 / 2 * 2` = 3) where the folded operands would
// int-divide (3/2*2 = 2) — a silently wrong value; those trees keep the loud `double` rendering.
func (v *Visitor) propagateUntypedConstContext(expr ast.Expr, context types.Type) {
	basic, ok := context.Underlying().(*types.Basic)

	if !ok || basic.Info()&(types.IsFloat|types.IsComplex|types.IsInteger) == 0 {
		return
	}

	intContext := basic.Info()&types.IsInteger != 0

	switch e := expr.(type) {
	case *ast.ParenExpr:
		v.assignUntypedConstContext(e.X, context)
	case *ast.UnaryExpr:
		switch e.Op {
		case token.ADD, token.SUB, token.XOR:
			v.assignUntypedConstContext(e.X, context)
		}
	case *ast.BinaryExpr:
		// Arithmetic operands adopt the result type; shifts, comparisons, and the
		// integer-only operators never carry a float/complex context (and an integer
		// context must not cross `/` — exact-rational vs truncating division, above)
		switch e.Op {
		case token.ADD, token.SUB, token.MUL:
			v.assignUntypedConstContext(e.X, context)
			v.assignUntypedConstContext(e.Y, context)
		case token.QUO:
			// Go performs INTEGER division on integer-kind operands even in a float/complex context
			// (`7 / 2` is 3, then 3.0 — not 3.5), so an integer-kind operand must NOT adopt the
			// context: rendered as a float literal (`7D`) it switches C# to float division, a
			// silently wrong value. A float-kind operand (`math.Pi`, `1.5`) already divides in float
			// and keeps the context so its F/D suffix is still chosen. (Integer contexts never reach
			// here — the outer !intContext guard — matching the exact-rational concern noted above.)
			if !intContext {
				if !v.isIntegerKindConstExpr(e.X) {
					v.assignUntypedConstContext(e.X, context)
				}
				if !v.isIntegerKindConstExpr(e.Y) {
					v.assignUntypedConstContext(e.Y, context)
				}
			}
		}
	case *ast.CallExpr:
		ident, ok := e.Fun.(*ast.Ident)

		if !ok {
			return
		}

		if _, isBuiltin := v.info.ObjectOf(ident).(*types.Builtin); !isBuiltin {
			return
		}

		var argContext types.Type

		switch ident.Name {
		case "complex":
			// The arguments are the ELEMENT type of the resulting complex type
			switch basic.Kind() {
			case types.Complex64:
				argContext = types.Typ[types.Float32]
			case types.Complex128:
				argContext = types.Typ[types.Float64]
			}
		case "real", "imag":
			// The argument is the complex COUNTERPART of the resulting float type
			switch basic.Kind() {
			case types.Float32:
				argContext = types.Typ[types.Complex64]
			case types.Float64:
				argContext = types.Typ[types.Complex128]
			}
		case "min", "max":
			argContext = context
		}

		if argContext == nil {
			return
		}

		for _, arg := range e.Args {
			v.assignUntypedConstContext(arg, argContext)
		}
	}
}

// assignUntypedConstContext records the context for one untyped constant subexpression and
// recurses into its children. An already-typed child keeps the type go/types recorded for it
// (context propagation stops there — that type IS its context).
func (v *Visitor) assignUntypedConstContext(expr ast.Expr, context types.Type) {
	tv, ok := v.info.Types[expr]

	if !ok || tv.Value == nil || tv.Type == nil {
		return
	}

	if basic, ok := tv.Type.(*types.Basic); !ok || basic.Info()&types.IsUntyped == 0 {
		return
	}

	v.untypedConstContexts[expr] = context
	v.propagateUntypedConstContext(expr, context)
}

// untypedConstContext returns the propagated contextual type of an untyped constant
// subexpression as its basic underlying type, or nil when none was recorded.
func (v *Visitor) untypedConstContext(expr ast.Expr) *types.Basic {
	if context, ok := v.untypedConstContexts[expr]; ok {
		if basic, ok := context.Underlying().(*types.Basic); ok {
			return basic
		}
	}

	return nil
}

// isIntegerKindConstExpr reports whether expr is an integer-kind constant — an untyped integer
// literal or an integer-typed constant (sub)expression. Such an operand of a `/` participates in
// Go's integer division, so it must not be pushed into a float/complex context (which would render
// it as a float literal and turn integer division into float division — a silent value change).
func (v *Visitor) isIntegerKindConstExpr(expr ast.Expr) bool {
	tv, ok := v.info.Types[expr]

	if !ok || tv.Value == nil || tv.Type == nil {
		return false
	}

	basic, ok := tv.Type.Underlying().(*types.Basic)

	return ok && basic.Info()&types.IsInteger != 0
}

// complexCallElementType reports the FLOAT element type a `complex(re, im)` call's arguments must
// be built at — float32 for a complex64 result, float64 for a complex128 one — or nil when the call
// does not type as a complex at all. It is the same answer assignUntypedConstContext records as the
// arguments' context (above); this reads it back from the CALL so the emission can pin an argument
// whose rendering could not carry it (a named untyped const — see convCallExpr's complex arm).
//
// A call whose arguments are all untyped constants is itself an untyped complex CONSTANT, so its
// own width comes from the enclosing context when the analysis recorded one (`var b complex64 =
// complex(…)`) and from Go's default type (complex128) otherwise — exactly the resolution order
// that decides a bare literal's F/D suffix.
func (v *Visitor) complexCallElementType(callExpr *ast.CallExpr) *types.Basic {
	basic, ok := v.info.TypeOf(callExpr).(*types.Basic)

	if !ok {
		return nil
	}

	if basic.Info()&types.IsUntyped != 0 {
		if context, ok := v.untypedConstContexts[callExpr]; ok {
			if contextBasic, ok := context.(*types.Basic); ok {
				basic = contextBasic
			}
		} else if defaulted, ok := types.Default(basic).(*types.Basic); ok {
			basic = defaulted
		}
	}

	switch basic.Kind() {
	case types.Complex64:
		return types.Typ[types.Float32]
	case types.Complex128:
		return types.Typ[types.Float64]
	}

	return nil
}
