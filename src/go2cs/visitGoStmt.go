// visitGoStmt.go - Gbtc
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
	"go/token"
	"go/types"
	"strings"
)

func (v *Visitor) visitGoStmt(goStmt *ast.GoStmt) {
	// Analyze captures specifically for this go statement. The seeded enter keeps the
	// enclosing lambda's capture renames visible while the EAGER call arguments render —
	// they are evaluated in the enclosing scope at spawn time (see enterDeferGoLambdaConversion).
	v.enterDeferGoLambdaConversion(goStmt)
	defer v.exitLambdaConversion()

	lambdaContext := DefaultLambdaContext()
	lambdaContext.deferOrGoCall = true
	paramCount := len(goStmt.Call.Args)

	// Capture-snapshot declarations belong BEFORE the goǃ statement — including those of a
	// func-literal ARGUMENT when the callee is a func-value ident (net lookup.go's
	// `go dnsWaitGroupDone(ch, func() {})`): without a hoist target the literal's decls
	// were written INLINE in the argument list (`goǃ(fʗ1, ch, var fʗ1 = f; () => {})`,
	// CS1003 ×4).
	lambdaContext.deferredDecls = &strings.Builder{}

	var renderLambdaParams bool

	// If we have a function literal, only prepare captures there, not on the GoStmt. A PARENTHESIZED
	// literal (`go (func() { … })()`, gopkg.in/check.v1's forkCall) is the same callee: taken as a
	// non-literal it prepared the statement's captures AND let the literal hoist its own, which then
	// had no statement to precede and was written inside the call (`goǃ((⏎var cʗ2 = cʗ1;⏎() => …`,
	// CS1002/CS1003/CS1026/CS1513).
	if funcLit, ok := ast.Unparen(goStmt.Call.Fun).(*ast.FuncLit); ok {
		if captures, exists := v.lambdaCapture.stmtCaptures[goStmt]; exists {
			v.lambdaCapture.stmtCaptures[funcLit] = captures

			// Delete captures from GoStmt to avoid double processing
			delete(v.lambdaCapture.stmtCaptures, goStmt)
		}

		// Same as the deferred form: a VARIADIC literal converts to no `Action<T1, T2>`, so the
		// arity-N `go` overload cannot infer its type arguments. Take the temp-parameter form,
		// which invokes the literal through the delegate cast convCallExpr applies.
		if paramCount > 0 && v.variadicFuncLitCallee(goStmt.Call) != nil {
			renderLambdaParams = true
		}
	} else {
		v.prepareStmtCaptures(goStmt)

		// If call expression parameter counts match those of target function signature,
		// we can render call without lambda parameters
		renderLambdaParams = paramCount != v.getFunctionParamCount(goStmt.Call.Fun)
	}

	// Resolve the callee signature once (func-literal callees are excluded — convCallExpr
	// already renders those as a lambda): whether it returns a value, and whether it is a
	// NAMED func type. Both decide how the call is adapted to goǃ's void Action delegate below.
	hasResults := false
	namedFuncType := false
	variadicCallee := false

	if _, isFuncLit := ast.Unparen(goStmt.Call.Fun).(*ast.FuncLit); !isFuncLit {
		if funType := v.getType(goStmt.Call.Fun, false); funType != nil {
			if sig, ok := funType.(*types.Signature); ok {
				if sig.Results() != nil && sig.Results().Len() > 0 {
					hasResults = true
				}

				// A VARIADIC callee's C# form always carries the params parameter, so its
				// method group converts to neither Action nor WaitCallback even at a
				// zero-argument call site — the exact mirror of the defer arm's guard
				// (`go signalDone()` on `signalDone(chans ...chan struct{})` was CS1503).
				if sig.Variadic() {
					variadicCallee = true
				}
			}

			if named, ok := types.Unalias(funType).(*types.Named); ok {
				if _, isSig := named.Underlying().(*types.Signature); isSig {
					namedFuncType = true
				}
			}
		}
	}

	// A VALUE-receiver method callee cannot pass as a bare method group: every Go named type
	// emits a C# struct, the method an extension on it, and C# forbids creating a delegate
	// from an extension method over a value-type receiver (CS1113 — net/http/httputil's
	// `go spc.copyToBackend(errc)`, switchProtocolCopier). Such a callee takes the lambda
	// form instead; the receiver snapshot still evaluates at go-statement time. An INTERFACE
	// receiver keeps the method group (a genuine C# instance method binds delegates fine),
	// and pointer receivers keep the box-group machinery below.
	valueRecvMethodGroup := false

	if selectorExpr, ok := goStmt.Call.Fun.(*ast.SelectorExpr); ok {
		if funcObj, ok := v.info.ObjectOf(selectorExpr.Sel).(*types.Func); ok {
			if sig, ok := funcObj.Type().(*types.Signature); ok && sig.Recv() != nil {
				if _, isPtrRecv := sig.Recv().Type().(*types.Pointer); !isPtrRecv && !types.IsInterface(sig.Recv().Type()) {
					valueRecvMethodGroup = true
				}
			}
		}
	}

	// A value-returning callee passed as a BARE method group is a Func<...>. Force the temp-param
	// lambda form so convCallExpr renders `(ᴛ1, ᴛ2) => chunkedCopy(ᴛ1, ᴛ2)`, which converts to a
	// discarding Action — exactly Go's fire-and-forget goroutine semantics. This form is what first
	// fixed CS0407 "no overload matches the delegate" for x/net/nettest's `go chunkedCopy(c2, c2)`
	// (chunkedCopy returns error), and it mirrors the defer arm's result handling. goǃ also has
	// Func<..., TResult> twins (builtin.GoroutineLaunchers.cs), which a value-returning function
	// literal binds; whether a bare method group could bind them in place of this form is not
	// measured, so the form stays. A VALUE-receiver method group forces the same lambda form for
	// its CS1113 wall (above).
	if (hasResults || valueRecvMethodGroup) && paramCount > 0 {
		renderLambdaParams = true
	}

	// A BUILTIN callee is generic with `in` parameters — its method group neither infers nor
	// converts to Action<T> (`goǃ(close, ch)` — CS1503) — and `panic` is not rendered as a call
	// at all: the panic arm of convCallExpr fills the eager-argument slot ONLY under the
	// temp-param form, so without this `go panic(v)` emits `goǃ(throw panic(v), )` (CS0839, plus
	// a `throw` in argument position). Render the lambda (`goǃ(ᴛ1 => close(ᴛ1), ch)`) so the
	// eager argument still evaluates at spawn time. Exact mirror of the arm in visitDeferStmt.
	if calleeIdent, ok := goStmt.Call.Fun.(*ast.Ident); ok && paramCount > 0 {
		if _, isBuiltin := v.info.ObjectOf(calleeIdent).(*types.Builtin); isBuiltin {
			renderLambdaParams = true
		}
	}

	// A ж-box ref-LOWERED callee (A2, the §3.3 boxed carve-out): its `ref` parameters make the
	// method group inconvertible to goǃ's Action<…>, so the goroutine call always takes the
	// temp-param lambda form — eager arguments stay boxed and the thunk derives each ref at
	// invoke time (`ᴛ1 => f(ref ᴛ1.DerefOrNull())`), preserving Go's spawn-time argument
	// evaluation. (A func-literal callee never resolves to a lowered package function.)
	if paramCount > 0 && v.refLoweredCalleePositions(goStmt.Call) != nil {
		renderLambdaParams = true
	}

	// An sstring TWIN callee has no single method group (CS0123): take the lambda form, exact
	// mirror of the arm in visitDeferStmt.
	if paramCount > 0 && v.callSStringTwinCallee(goStmt.Call) != nil {
		renderLambdaParams = true
	}

	// A MULTI-VALUE call as the SOLE argument (`go f(g())`) always takes the temp-parameter form —
	// exact mirror of the arm in visitDeferStmt. Go evaluates `g()` on the CURRENT goroutine at the
	// `go` statement and only `f` runs on the new one, so the whole tuple is the eager argument and
	// the thunk spreads its components (`ᴛ1 => f(ᴛ1.Item1, ᴛ1.Item2)`; see convExprList).
	if paramCount > 0 && v.multiValueSpreadArity(goStmt.Call) > 1 {
		renderLambdaParams = true
	}

	// Go evaluates the receiver on the CURRENT goroutine at the `go` statement. The lambda form read it on the
	// new one when it ran: `() => cʗ1[i].Close()` indexed the array copy then, and an array copy shares its
	// elements, so `c[i] = other` after the statement leaked in. A VALUE receiver that is not a plain
	// identifier (a value method always takes the lambda form here), and an interface or pointer receiver
	// under the lambda form, take the `ᴛ0` snapshot instead -- exact mirror of the receiver snapshot in
	// visitDeferStmt. One Go reaches by its ADDRESS snapshots the address (see lateReceiverSnapshot).
	recvSnapshot := false

	var recvAddressOf ast.Expr

	if valueRecvMethodGroup {
		recvSnapshot = !v.deferReceiverIsIdentifier(goStmt.Call) && paramCount < 16
	} else {
		recvSnapshot, recvAddressOf = v.lateReceiverSnapshot(goStmt.Call, renderLambdaParams)
	}

	if recvSnapshot && paramCount > 0 {
		renderLambdaParams = true
	}

	// A CONSTANT argument renders inside the thunk's call instead of being captured, since it has the same
	// value whenever it is evaluated: `goǃ(() => fmt.Println("First"))` (goDeferConstArgs.go).
	var inlineConstArgs []bool

	inlineConstArgs, renderLambdaParams = v.goDeferInlineForm(goStmt.Call, renderLambdaParams, recvSnapshot)

	if paramCount > 0 {
		lambdaContext.callArgs = make([]string, paramCount)
		lambdaContext.renderParams = renderLambdaParams
		lambdaContext.inlineConstArgs = inlineConstArgs
	}

	result := strings.Builder{}
	wroteDecls := false

	callExpr := strings.TrimSpace(v.convCallExpr(goStmt.Call, lambdaContext))

	// The receiver snapshot splits the rendered call exactly as visitDeferStmt's does.
	recvArg := ""

	if recvSnapshot {
		temps := make([]string, paramCount)

		for i := range temps {
			temps[i] = fmt.Sprintf("%s%d", TempVarMarker, i+1)
		}

		if head, ok := strings.CutSuffix(callExpr, "("+strings.Join(temps, ", ")+")"); ok {
			if dot := strings.LastIndex(head, "."); dot > 0 && isPlainIdentifier(head[dot+1:]) {
				recvArg = head[:dot]
				callExpr = fmt.Sprintf("%s%d%s", TempVarMarker, 0, callExpr[dot:])
			}
		}

		if recvArg != "" && recvAddressOf != nil {
			recvArg = v.convUnaryExpr(&ast.UnaryExpr{Op: token.AND, X: recvAddressOf}, DefaultUnaryExprContext())
		}
	}

	if lambdaContext.deferredDecls != nil && lambdaContext.deferredDecls.Len() > 0 {
		result.WriteString(lambdaContext.deferredDecls.String())
		result.WriteString(v.indent(v.indentLevel))
		wroteDecls = true
	}

	if decls := v.generateCaptureDeclarations(); strings.TrimSpace(decls) != "" {
		result.WriteString(decls)
		wroteDecls = true
	}

	if !wroteDecls {
		result.WriteString(v.newline)
		result.WriteString(v.indent(v.indentLevel))
	}

	// The following is basically "go!". We use the unicode bang-type character
	// as a valid C# identifier symbol, where the standard "!" is not. This is
	// to disambiguate the method name from the namespace when calling function.
	result.WriteString("go\u01C3(")

	if paramCount == 0 {
		// A pointer-receiver nullary callee binds the BOX method group (see
		// pointerReceiverBoxMethodGroup — the trimmed deref-alias form is CS1113). A NAMED
		// func-type or value-returning callee keeps the lambda form instead.
		// The variadic-callee guard covers the box group too — a pointer-receiver variadic
		// method's box overload carries the same params parameter (see the defer arm).
		boxGroup := ""

		if !hasResults && !namedFuncType && !variadicCallee {
			boxGroup = v.pointerReceiverBoxMethodGroup(goStmt.Call.Fun)
		}

		// C# `go` method implementation expects an Action (or WaitCallback delegate)
		// A VARIADIC func literal has no method group to expose — convCallExpr produced
		// `((Actionꓸꓸꓸ<T>)(<literal>))()` and the trim would leave the family delegate, which is
		// not an `Action` (CS1503) — and a VARIADIC named callee's group carries the params
		// parameter, inconvertible the same way. Keep the invocation and take the wrapping arm.
		variadicLit := v.variadicFuncLitCallee(goStmt.Call) != nil

		if recvArg != "" {
			callExpr = fmt.Sprintf("%s%d => %s, %s", TempVarMarker, 0, callExpr, recvArg)
		} else if boxGroup != "" {
			callExpr = boxGroup
		} else if !hasResults && !namedFuncType && !valueRecvMethodGroup && !variadicLit && !variadicCallee && strings.HasSuffix(callExpr, "()") {
			callExpr = strings.TrimSuffix(callExpr, "()") // Action delegate
		} else if (hasResults || namedFuncType || valueRecvMethodGroup || variadicLit || variadicCallee) && strings.HasSuffix(callExpr, "()") {
			// A NAMED func-type callee (context.CancelFunc) is a DISTINCT C# delegate with no
			// conversion to Action, and a value-returning callee's bare method group is a
			// Func<...> (CS0407); keep the invocation and wrap it in an Action lambda that
			// discards any result — exactly Go's fire-and-forget goroutine semantics. A nullary
			// VALUE-receiver method group keeps the invocation too (its trimmed form is CS1113).
			callExpr = "() => " + callExpr
		} else {
			callExpr = "_ => " + callExpr // WaitCallback delegate
		}

		result.WriteString(callExpr)
	} else {
		// The thunk's parameters are the arguments that stay eager; inlined constants take none.
		eagerCount := goDeferEagerCount(inlineConstArgs, paramCount)

		if renderLambdaParams {
			// A receiver snapshot leads the thunk's parameters as `ᴛ0` (and its eager arguments below).
			firstParam := 1

			if recvArg != "" {
				firstParam = 0
			}

			if eagerCount+1-firstParam != 1 {
				result.WriteRune('(')
			}

			for i := firstParam; i <= eagerCount; i++ {
				if i > firstParam {
					result.WriteString(", ")
				}

				result.WriteString(fmt.Sprintf("%s%d", TempVarMarker, i))
			}

			if eagerCount+1-firstParam != 1 {
				result.WriteRune(')')
			}

			result.WriteString(" => ")
		} else {
			// A matching-arity pointer-receiver method group has the same CS1113 as the
			// nullary form — the deref-alias render is a struct VALUE against the [GoRecv]
			// ref extension (os/exec's `go c.watchCtx(resultc)`); bind the BOX overload.
			if boxGroup := v.pointerReceiverBoxMethodGroup(goStmt.Call.Fun); boxGroup != "" {
				callExpr = boxGroup
			}
		}

		result.WriteString(callExpr)

		var eagerArgs []string

		if recvArg != "" {
			eagerArgs = append(eagerArgs, recvArg)
		}

		eagerArgs = append(eagerArgs, goDeferEagerArgs(inlineConstArgs, lambdaContext.callArgs)...)

		if len(eagerArgs) > 0 {
			result.WriteString(", ")
			result.WriteString(strings.Join(eagerArgs, ", "))
		}
	}

	// A goroutine proved unable to recurse starts on golib's small stack reservation rather than the Go-scale default
	// (goroutineStackAnalysis.go); everything else keeps the default, unmarked.
	if bounded, _ := goroutineStackBounded(goStmt.Call, v.info); bounded {
		result.WriteString(", GoStack.Bounded")
	}

	result.WriteString(");")

	v.outputBuilder.WriteString(result.String())
}
