// visitDeferStmt.go - Gbtc
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

func (v *Visitor) visitDeferStmt(deferStmt *ast.DeferStmt) {
	// Capability 4 — this defer is emitted into the frame's FINALLY rather than registered
	// (deferFinallyLowering.go decided that before the body was visited). The statement position
	// carries only the reached-flag; the call is rendered here as an ORDINARY call — no lambda
	// conversion, because nothing is stored and so nothing is captured — and composed into the
	// finally once the body has rendered. This arm runs before the capture machinery below for
	// exactly that reason.
	if index, lowered := v.loweredDeferIndex[deferStmt]; lowered {
		v.loweredDefers[index].call = strings.TrimSpace(v.convCallExpr(deferStmt.Call, DefaultLambdaContext()))
		v.loweredDefers[index].aliasName = v.loweredCallAliasName(deferStmt.Call)

		v.outputBuilder.WriteString(v.newline)
		v.outputBuilder.WriteString(v.indent(v.indentLevel))
		v.outputBuilder.WriteString(fmt.Sprintf("%s = true;", v.loweredDefers[index].flagName))

		return
	}

	// Analyze captures specifically for this defer statement. The seeded enter keeps the
	// enclosing lambda's capture renames visible while the EAGER call arguments render —
	// they are evaluated in the enclosing scope at defer time (see enterDeferGoLambdaConversion).
	v.enterDeferGoLambdaConversion(deferStmt)
	defer v.exitLambdaConversion()

	lambdaContext := DefaultLambdaContext()
	lambdaContext.deferOrGoCall = true
	lambdaContext.deferCall = true

	// A func-literal ARGUMENT of the deferred call — `defer once.Do(func() { stop() })`
	// (x/net/nettest conntest.go) — can capture enclosing-scope locals; the capture-snapshot
	// declarations (`var stopʗ1 = stop;`) must hoist BEFORE the `defer(...)` call, never emit
	// inline in its argument list (an invalid statement mid-expression → CS1001/CS1002/CS1003/
	// CS1026). Provide the hoist sink UNCONDITIONALLY so convFuncLit routes any argument
	// literal's captures here (threaded via convCallExpr → convExprList → the arg's
	// LambdaContext) — not only when the deferred CALLEE is itself a func literal.
	lambdaContext.deferredDecls = &strings.Builder{}

	paramCount := len(deferStmt.Call.Args)

	var renderLambdaParams bool

	// If we have a function literal, only prepare captures there, not on the DeferStmt
	if funcLit, ok := deferStmt.Call.Fun.(*ast.FuncLit); ok {
		if captures, exists := v.lambdaCapture.stmtCaptures[deferStmt]; exists {
			v.lambdaCapture.stmtCaptures[funcLit] = captures

			// Delete captures from GoStmt to avoid double processing
			delete(v.lambdaCapture.stmtCaptures, deferStmt)
		}

		renderLambdaParams = false

		// A VARIADIC literal is the one callee shape the arity-N `defer` overload cannot take
		// directly — `params ꓸꓸꓸ@string` converts to no `Action<T1, T2>`, so its type arguments do
		// not infer (CS0411). Force the temp-parameter form, whose thunk INVOKES the literal
		// instead of handing it over; convCallExpr casts it to its golib family delegate so that
		// invocation binds. Go's defer-TIME argument evaluation is unaffected — the arguments are
		// still the eager ones, and `ᴛ1`/`ᴛ2` are what the thunk receives back at unwind.
		if paramCount > 0 && v.variadicFuncLitCallee(deferStmt.Call) != nil {
			renderLambdaParams = true
		}
	} else {
		v.prepareStmtCaptures(deferStmt)

		// If call expression parameter counts match those of target function signature,
		// we can render call without lambda parameters
		renderLambdaParams = paramCount != v.getFunctionParamCount(deferStmt.Call.Fun)

		// A BUILTIN callee (`defer close(returned)`, net dial) is generic with `in`
		// parameters — its method group neither infers nor converts to Action<T>
		// (CS1503). Render the temp-param lambda (`defer(ᴛ1 => close(ᴛ1), returned,
		// ref ᒐ)`) so the eager-argument form still evaluates the argument at defer time.
		if calleeIdent, ok := deferStmt.Call.Fun.(*ast.Ident); ok && paramCount > 0 {
			if _, isBuiltin := v.info.ObjectOf(calleeIdent).(*types.Builtin); isBuiltin {
				renderLambdaParams = true
			}
		}

		// A ж-box ref-LOWERED callee (A2, the §3.3 boxed carve-out): its `ref` parameters make
		// the method group inconvertible to Action<…>, so the deferred call always takes the
		// temp-param lambda form — eager arguments stay boxed and the thunk derives each ref at
		// invoke time (`ᴛ1 => f(ref ᴛ1.DerefOrNull())`), preserving Go's defer-time argument
		// evaluation.
		if paramCount > 0 && v.refLoweredCalleePositions(deferStmt.Call) != nil {
			renderLambdaParams = true
		}
	}

	// A MULTI-VALUE call as the SOLE argument (`defer f(g())`) always takes the temp-parameter
	// form: the eager argument is the whole tuple and the thunk spreads its components at unwind
	// (`ᴛ1 => f(ᴛ1.Item1, ᴛ1.Item2)`; see convExprList). The arity test above already forces it for
	// an ordinary callee — one argument against N>1 declared parameters — but a FUNC-LITERAL callee
	// (`defer func(n int, s string) { … }(g())`) reaches neither that test nor the variadic one, and
	// stating it here keeps the form independent of getFunctionParamCount's -1/0 fallbacks.
	if paramCount > 0 && v.multiValueSpreadArity(deferStmt.Call) > 1 {
		renderLambdaParams = true
	}

	// A callee that RETURNS results does NOT need the temp-parameter form at arity N, and an
	// earlier cut of this file said it did. golib carries BOTH shapes of `defer` at every arity
	// with parameters -- `defer<T1, T2>(Action<T1, T2>, T1, T2, ref GoFrame)` and
	// `defer<T1, T2, TResult>(Func<T1, T2, TResult>, T1, T2, ref GoFrame)`, 17 Action forms and 16
	// Func forms in builtin.DeferRegistrations.cs -- so a result-returning callee passes as a bare
	// method group and the corpus has been compiling exactly that for months
	// (`defer(os.RemoveAll, @base, ref ᒐ)`, `defer(syscall.Close, fd, ref ᒐ)`).
	//
	// Arity ZERO is the real asymmetry and the arity-0 rung below is where it belongs: `defer` has
	// ONE nullary overload, `defer(Action, ref GoFrame)`, and no `Func<TResult>` form -- which is
	// why a nullary result-returning callee must be wrapped in a lambda and an arity-N one must not.
	//
	// The withdrawn rung was written to explain a CS0839 on a deferred syscall-funnel call, and it
	// explained nothing: that emission's empty argument slots came from convCallExpr intercepting
	// every funnel call ahead of the `callArgs` threading, which is fixed at the root in
	// convCallExpr now. The rung changed the malformed emission's SHAPE without filling a slot,
	// while rewriting six unrelated behavioral projects from a clean method group into a thunk that
	// costs a closure allocation per defer. Measured and withdrawn 2026-09-02; the overload table
	// is one grep and settles it, so check it before re-deriving this.

	if paramCount > 0 {
		lambdaContext.callArgs = make([]string, paramCount)
		lambdaContext.renderParams = renderLambdaParams
	}

	result := strings.Builder{}
	wroteDecls := false

	callExpr := strings.TrimSpace(v.convCallExpr(deferStmt.Call, lambdaContext))

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

	// Every registration names the frame it registers into — this function's own GoFrame local,
	// passed by reference because a ref struct cannot be captured (see goFrameOperations.go). The
	// arity-0 rung is one of the ladder like every other; the name carries no bang because there is
	// no longer a `defer`-named delegate parameter to disambiguate from.
	deferTarget := fmt.Sprintf(", ref %s);", v.goFrameName())

	if paramCount == 0 {
		result.WriteString("defer(")

		// C# `defer` method implementation expects an Action delegate. The bare
		// method-group form (`defer(k.Close)`) binds only when the callee returns VOID —
		// an error-returning method (`defer k.Close()`, registry Key.Close in time's
		// zoneinfo_windows) is a Func<error> method group (CS1503 ×2). Keep the lambda
		// form there so the call's result is discarded, exactly Go's deferred-call
		// semantics.
		hasResults := false
		namedFuncType := false
		variadicCallee := false

		if funType := v.getType(deferStmt.Call.Fun, false); funType != nil {
			if sig, ok := funType.(*types.Signature); ok {
				if sig.Results() != nil && sig.Results().Len() > 0 {
					hasResults = true
				}

				// A VARIADIC callee's C# form always carries the params parameter, so its
				// method group converts to no Action even when the Go call site passes zero
				// arguments — `defer Reset()` on `Reset(sig ...Signal)` trimmed to the group
				// was CS1503 (os/signal, 2026-08-27). Keep the lambda so the zero-operand
				// call invokes through the params overload; there are no arguments whose
				// defer-time evaluation the wrap could disturb.
				if sig.Variadic() {
					variadicCallee = true
				}
			}

			// A NAMED func-type callee (context.CancelFunc) is a DISTINCT C# delegate with
			// no conversion to Action — the bare trimmed form was CS1503 ×5 (net dial's
			// `defer cancel()`); keep the lambda so the invocation converts.
			if named, ok := types.Unalias(funType).(*types.Named); ok {
				if _, isSig := named.Underlying().(*types.Signature); isSig {
					namedFuncType = true
				}
			}
		}

		// A nullary deferred call on a POINTER-receiver method whose receiver is already a
		// pointer binds the BOX method group — the plain-call trim leaves the deref-alias form
		// (`Ꮡconf.Value.releaseSema`, net nss.go's `defer conf.releaseSema()`), a struct
		// VALUE against the [GoRecv] ref extension, which cannot create a delegate (CS1113).
		// `Ꮡconf.releaseSema` binds the ж<T> overload and captures the receiver at defer
		// time — exactly Go's binding.
		// The variadic-callee guard covers the box group too: a pointer-receiver variadic
		// method's box overload carries the same params parameter, so its group is exactly as
		// inconvertible as the plain one (`defer c.bump()` on `bump(deltas ...int)`).
		boxGroup := ""

		if !hasResults && !namedFuncType && !variadicCallee {
			boxGroup = v.pointerReceiverBoxMethodGroup(deferStmt.Call.Fun)
		}

		// The trim turns an invocation back into a method group, which golib's arity-0 `defer`
		// takes as its `Action`. A VARIADIC func literal has no method group to expose: what
		// convCallExpr produced is `((Actionꓸꓸꓸ<T>)(<literal>))()`, and trimming leaves the family
		// delegate itself, which is not an `Action` (CS1503) — and a VARIADIC named callee's
		// group carries the params parameter, inconvertible the same way. Keep the invocation
		// and let the lambda arm below wrap it.
		variadicLit := v.variadicFuncLitCallee(deferStmt.Call) != nil

		if boxGroup != "" {
			callExpr = boxGroup
		} else if !hasResults && !namedFuncType && !variadicLit && !variadicCallee && strings.HasSuffix(callExpr, "()") {
			callExpr = strings.TrimSuffix(callExpr, "()")
		} else {
			callExpr = "() => " + callExpr
		}

		result.WriteString(callExpr)
		result.WriteString(deferTarget)
	} else {
		result.WriteString("defer(")

		if renderLambdaParams {
			if paramCount > 1 {
				result.WriteRune('(')
			}

			for i := range paramCount {
				if i > 0 {
					result.WriteString(", ")
				}

				result.WriteString(fmt.Sprintf("%s%d", TempVarMarker, i+1))
			}

			if paramCount > 1 {
				result.WriteRune(')')
			}

			result.WriteString(" => ")
		} else {
			// A matching-arity pointer-receiver method group has the same CS1113 as the
			// nullary form (see above) — bind the BOX overload here too.
			if boxGroup := v.pointerReceiverBoxMethodGroup(deferStmt.Call.Fun); boxGroup != "" {
				callExpr = boxGroup
			}
		}

		result.WriteString(callExpr)
		result.WriteString(", ")
		result.WriteString(strings.Join(lambdaContext.callArgs, ", "))
		result.WriteString(deferTarget)
	}

	v.outputBuilder.WriteString(result.String())
}

// multiValueSpreadArity reports how many results a deferred/spawned call's SOLE argument
// spreads into the callee's parameter list — `defer f(g())` with `g` returning (int, string)
// — and 0 for every other shape. Go permits the spread ONLY when the multi-value call is the
// call's one and only argument, so the test is exactly that: one argument, itself a call,
// whose type is a tuple of more than one result.
//
// The shape needs the TEMP-PARAMETER form and a component-wise thunk body: C# has no splat, so
// the eager argument is the tuple itself (`g()`, evaluated once at the defer/go statement,
// which is Go's rule) and the thunk spreads its components when it runs (see convExprList's
// substitution site, the one place a deferred call's arguments are captured).
func (v *Visitor) multiValueSpreadArity(call *ast.CallExpr) int {
	if len(call.Args) != 1 || call.Ellipsis.IsValid() {
		return 0
	}

	innerCall, isCall := ast.Unparen(call.Args[0]).(*ast.CallExpr)

	if !isCall {
		return 0
	}

	tuple, isTuple := v.getExprType(innerCall).(*types.Tuple)

	if !isTuple || tuple.Len() < 2 {
		return 0
	}

	return tuple.Len()
}

func (v *Visitor) getFunctionParamCount(expr ast.Expr) int {
	tv, ok := v.info.Types[expr]

	if !ok {
		return 0
	}

	sig, ok := tv.Type.Underlying().(*types.Signature)

	if !ok {
		return 0
	}

	// Do not compare parameter counts when variadic parameters exist
	if sig.Variadic() {
		return -1
	}

	return sig.Params().Len()
}

// pointerReceiverBoxMethodGroup renders `X.M` as a BOX-bound method group for a nullary
// defer/go call whose method has a POINTER receiver and whose receiver ident is already
// pointer-typed (`defer conf.releaseSema()` with `conf *resolverConfig`): the plain call
// render deref-aliases the receiver (`Ꮡconf.Value.releaseSema()`), whose method-group
// trim is a struct VALUE against the [GoRecv] ref extension (CS1113). The box form binds
// the ж<T> overload and captures the receiver when the delegate is created — Go's
// binding time. Non-ident receivers keep the existing emission.
func (v *Visitor) pointerReceiverBoxMethodGroup(fun ast.Expr) string {
	selectorExpr, ok := fun.(*ast.SelectorExpr)

	if !ok {
		return ""
	}

	funcObj, ok := v.info.ObjectOf(selectorExpr.Sel).(*types.Func)

	if !ok {
		return ""
	}

	sig, ok := funcObj.Type().(*types.Signature)

	if !ok || sig.Recv() == nil {
		return ""
	}

	// A result-returning method group is a Func<...>, which neither defer(Action) nor
	// go(Action) accepts - those keep the lambda form.
	if sig.Results() != nil && sig.Results().Len() > 0 {
		return ""
	}

	recvPtr, isPtrRecv := sig.Recv().Type().(*types.Pointer)

	if !isPtrRecv {
		return ""
	}

	recvType := v.getType(selectorExpr.X, false)

	if recvType == nil {
		return ""
	}

	methodSel := "." + v.convIdent(selectorExpr.Sel, v.getSelIdentContext(selectorExpr))

	// The receiver is ALREADY a pointer (`defer conf.releaseSema()` with `conf *resolverConfig`):
	// an ident whose box is `Ꮡ`+name. A PROMOTED method (net's `defer zc.Unlock()` — Unlock on the
	// embedded sync.RWMutex) has no extension on the OUTER box type (CS1061), so only a method
	// declared DIRECTLY on the pointee takes the box group.
	if xPtr, alreadyPtr := recvType.(*types.Pointer); alreadyPtr {
		identX, ok := selectorExpr.X.(*ast.Ident)

		if !ok {
			return ""
		}

		if !types.Identical(recvPtr.Elem(), xPtr.Elem()) {
			return ""
		}

		ptrContext := DefaultIdentContext()
		ptrContext.isPointer = true

		return v.convIdent(identX, ptrContext) + methodSel
	}

	// The receiver is a VALUE whose type is exactly the pointer-receiver's pointee — Go auto-takes
	// its address (`defer b.deck.reset()`, `deck pcDeck` a value field, `reset` a `*pcDeck` method;
	// runtime/pprof, database/sql, log/slog). The plain render deref-aliases the field to a value
	// (`Ꮡb.Value.deck`), whose method-group trim binds the `[GoRecv] ref` extension (CS1113).
	// Render `&receiver` through the address machinery — a boxed base gives an aliasing field-ref
	// `Ꮡb.of(T.Ꮡdeck)`, an escaping value local gives its box `Ꮡx`, a plain value gives the
	// `Ꮡ(value)` copy — then bind the method; the ж<T> overload forms a valid delegate captured at
	// defer time. Restricted to a NAMED value type (its RecvGenerator box overload exists) matching
	// the pointee exactly (a promoted/embedded method has no box extension on this type).
	if !types.Identical(recvPtr.Elem(), recvType) {
		return ""
	}

	if _, ok := recvType.(*types.Named); !ok {
		return ""
	}

	addr := v.convUnaryExpr(&ast.UnaryExpr{Op: token.AND, X: selectorExpr.X}, DefaultUnaryExprContext())

	if addr == "" {
		return ""
	}

	return addr + methodSel
}
