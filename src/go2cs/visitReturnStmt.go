// visitReturnStmt.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

package main

import (
	"fmt"
	"go/ast"
	"go/token"
	"go/types"
	"strings"
)

const DeferredDeclsMarker = ">>MARKER:DEFERRED_DECLS<<"

// lambdaConstReturnCastType returns the C# type name to cast a constant integer-literal RETURN expression
// to, when that return sits inside a LAMBDA body whose result type is an unsigned/pointer-sized integer.
// C# infers a lambda's delegate type from the *types* of its return expressions; a lambda that mixes
// `return 0` (the literal is typed `int`) with `return pcs[i]` (typed `nuint`/`uint`/`ulong`) has no
// best-common-type between `int` and the unsigned target, so the enclosing `var f = …` assignment fails
// with CS8917 ("no best type found"). Casting the literal to the result type (`return (nuint)0`) makes both
// returns the same type, so the delegate is inferable. (runtime/select.go `casePC := func(casi int) uintptr
// { if pcs == nil { return 0 }; return pcs[casi] }`.) Gated narrowly to avoid pointless golden churn:
//   - only inside a lambda body (conversionInLambda) — a NAMED func's `return 0` to a `nuint` result
//     compiles as an ordinary constant conversion and needs no cast;
//   - only for a bare INTEGER literal (optionally negated) — the sole shape that trips the inference gap;
//   - only when the result kind is uint/uint32/uint64/uintptr — `int` shares a common type with
//     byte/uint16 (they widen to int) and with the signed/nint/long kinds, so those never hit CS8917.
func (v *Visitor) lambdaConstReturnCastType(targetType types.Type, expr ast.Expr) string {
	if v.lambdaCapture == nil || !v.lambdaCapture.conversionInLambda {
		return ""
	}

	lit := expr

	if unary, ok := lit.(*ast.UnaryExpr); ok && unary.Op == token.SUB {
		lit = unary.X
	}

	if basicLit, ok := lit.(*ast.BasicLit); !ok || basicLit.Kind != token.INT {
		return ""
	}

	// Require a BASIC target (not a named type over an unsigned kind): a basic uintptr/uint/uint32/uint64
	// emits a C# alias cast (`(uintptr)0`, `(uint64)0`) that always compiles, whereas a named numeric type
	// `type gclinkptr uintptr` would emit `(gclinkptr)0`, which only compiles if that type happens to
	// define an int conversion — casting it here could introduce a NEW error. The runtime CS8917 site
	// (select.go, basic uintptr) needs only the basic case; leave named-type lambda returns alone.
	if targetType == nil {
		return ""
	}

	targetBasic, ok := targetType.(*types.Basic)

	if !ok {
		return ""
	}

	switch targetBasic.Kind() {
	case types.Uint, types.Uint32, types.Uint64, types.Uintptr:
		// C# uint / ulong / nuint — a bare `int` literal has no best-common-type with these.
	default:
		return ""
	}

	return convertToCSTypeName(v.getAliasQualifiedTypeName(targetType, false))
}

// lambdaFuncLitReturnCastType is lambdaConstReturnCastType's sibling for the OTHER expression shape
// that renders without a natural C# type: a Go FUNCTION LITERAL, which becomes a bare lambda. C#
// infers an enclosing lambda's delegate type from its return expressions, and a bare lambda
// contributes nothing — so `mergeCancel := func(ctx, cancelCtx Context) (Context, CancelFunc) { …
// return ctx, func(){…} }` (context's ExampleAfterFunc_merge) left `var mergeCancel = (…) => {…}`
// with no inferable type: CS8917, and CS8130 at every deconstruction of its result. Naming the
// declared result type turns the returned literal into a typed value, which types the tuple, which
// types the enclosing lambda — and it is the more faithful rendering besides, since Go's declared
// result here IS the named `CancelFunc` delegate.
//
// Gated exactly like its sibling, to avoid golden churn where C# already succeeds:
//   - only inside a lambda body (conversionInLambda) — a NAMED func's returned literal is
//     target-typed by the method's declared return type and needs no cast;
//   - only for a func literal (through parentheses), the shape that carries no natural type;
//   - only when the declared result is a NAMED func type, whose emitted C# delegate name is exactly
//     what a cast needs. An UNNAMED `func() bool` result has no corpus site today; it would need the
//     synthesized `Func<…>`/`Action` spelling and is deliberately left until one appears.
//
// resultRendersAsBareLambda reports whether a return result renders as a C# lambda with no natural
// type — a Go function literal, through any number of parentheses. It lived beside the retired
// execution-context inference rules (a lambda contributed nothing to the wrapper's `T`); the frame
// form returns against the method's own declared result type, so the only remaining consumer is the
// named-func-type return cast below.
func resultRendersAsBareLambda(expr ast.Expr) bool {
	for {
		switch e := expr.(type) {
		case *ast.ParenExpr:
			expr = e.X
		case *ast.FuncLit:
			return true
		default:
			return false
		}
	}
}

// isNamedStringType reports whether t is a DEFINED type whose underlying is a Go string — `type
// String string` (internal/poll), `type Name string`. A plain `string` is deliberately excluded: it
// emits as golib's `@string`, which a literal already reaches in one conversion. So is a type
// PARAMETER, whose emitted form is not a `[GoType]` wrapper.
func isNamedStringType(t types.Type) bool {
	if t == nil {
		return false
	}

	named, ok := types.Unalias(t).(*types.Named)

	if !ok {
		return false
	}

	basic, ok := named.Underlying().(*types.Basic)

	return ok && basic.Info()&types.IsString != 0
}

func (v *Visitor) lambdaFuncLitReturnCastType(targetType types.Type, expr ast.Expr) string {
	if v.lambdaCapture == nil || !v.lambdaCapture.conversionInLambda || targetType == nil {
		return ""
	}

	if !resultRendersAsBareLambda(expr) {
		return ""
	}

	named, ok := types.Unalias(targetType).(*types.Named)

	if !ok {
		return ""
	}

	if _, isSig := named.Underlying().(*types.Signature); !isSig {
		return ""
	}

	return convertToCSTypeName(v.getAliasQualifiedTypeName(targetType, false))
}

func (v *Visitor) visitReturnStmt(returnStmt *ast.ReturnStmt) {
	recvIndex := -1
	refReturnForwardIndex := -1
	var capturedRecvName string

	// Check if receiver is directly returned (object identity — a shadowing local returned
	// here must keep its own render, not the receiver box; see identResolvesToReceiver)
	if ptrRecv, recvName := v.isPointerReceiver(); ptrRecv {
		for i, result := range returnStmt.Results {
			if ident, ok := result.(*ast.Ident); ok {
				if v.identResolvesToReceiver(ident, recvName) {
					recvIndex = i

					// Direct-ж: the receiver box is the parameter `Ꮡrecv`, so return it
					// directly. A method that returns its receiver is marked direct-box by the
					// pre-pass (bodyReturnsReceiver → packageDirectBoxReceiverMethods), so it is
					// emitted with the `ж<T> Ꮡrecv` receiver. This replaces the old static
					// ThreadLocal capture, which raced across threads for distinct receivers.
					capturedRecvName = AddressPrefix + recvName

					// B′-S0 arm (a): a ref-return primary has NO box — the receiver IS the ref
					// parameter, and Go's `return v` is the receiver itself: `return ref v;`.
					// The selection guarantees every return in such a method is this shape.
					if v.currentRefReturnPrimary {
						capturedRecvName = "ref " + getSanitizedIdentifier(recvName)
					}

					break
				}
			}
		}
	}

	// B′-S1 forwarding-return specialization: a ref-return primary whose single return is
	// `v.M(…)` — a call on the bare receiver to another ref-return primary — returns the ref
	// M yields, so it renders `return ref v.M(…)`. The receiver un-boxing (v, not Ꮡv, so the
	// call binds the primary rather than its ж twin) is the general call-site seam; this adds
	// only the `ref` prefix. The cascade's selection guarantees every return in such a method
	// is either this shape or the bare-receiver arm (a) above.
	if refReturnForwardIndex == -1 && v.currentRefReturnPrimary && v.currentFuncSignature != nil && v.currentFuncSignature.Recv() != nil {
		recvObj := v.currentFuncSignature.Recv()
		isRefReturnPrimary := func(fn *types.Func) bool {
			return packageRefReturnPrimaryMethods != nil && packageRefReturnPrimaryMethods[fn.Origin()]
		}

		for i, result := range returnStmt.Results {
			if returnForwardsReceiverThroughPrimary(result, recvObj, v.info, isRefReturnPrimary) {
				refReturnForwardIndex = i
				break
			}
		}
	}

	result := strings.Builder{}
	deferredDecls := strings.Builder{}

	// A func literal passed as a call ARGUMENT inside a return expression (`return HandlerFunc(
	// func(w, r) {…}), "", nil` — net/http registerErr) emits its captured-variable snapshot
	// declarations (`var allowedMethodsʗ1 = allowedMethods;`) — statements, invalid inside an
	// expression. A DIRECT func-literal result threads lambdaContext.deferredDecls, but a literal
	// nested as a call argument falls back to the pre-statement hoist sink (see convFuncLit /
	// convExprList); provide one and splice it in before the `return` through the
	// DeferredDeclsMarker slot (mirrors visitIfStmt/visitForStmt/visitExprStmt).
	savedHoist := v.hoistedDecls
	hoistBuf := &strings.Builder{}
	v.hoistedDecls = hoistBuf

	defer func() { v.hoistedDecls = savedHoist }()

	// In namedReturnDeferMode a `return e1, e2` assigns the named result params and then returns
	// (via the void func() wrapper); the wrapper's caller side emits the actual `return <named>`
	// after the defers run. So here we emit `(named1, named2) = (e1, e2); return;` for an explicit
	// return, or a bare `return;` for a naked return (the result params are already set).
	namedDefer := v.namedReturnDeferMode

	// An explicit `return n1, n2` that returns exactly the named result identifiers (in order) is
	// equivalent to a naked return — emitting the assignment would be a `(n1, n2) = (n1, n2)`
	// self-assignment. Skip it. Only collapse when *every* result is a bare named-result ident:
	// `return foo(), msg` must keep the assignment because foo() may have side effects.
	explicitIsNamed := namedDefer && returnStmt.Results != nil && len(returnStmt.Results) == len(v.namedReturnNames)

	if explicitIsNamed {
		for i, expr := range returnStmt.Results {
			ident, ok := expr.(*ast.Ident)

			if !ok || getSanitizedIdentifier(v.getIdentName(ident)) != v.namedReturnNames[i] {
				explicitIsNamed = false
				break
			}
		}
	}

	// namedDeferAssign is true when we must emit the `(named) = (results); return;` form (an
	// explicit return whose results are not simply the named results themselves).
	namedDeferAssign := namedDefer && returnStmt.Results != nil && !explicitIsNamed

	// A namedReturnDefer return that is the last statement of the function body is terminal: the
	// void func() wrapper simply falls off its end, so the trailing `return;` is redundant. A
	// terminal naked/collapsed return contributes no statement at all; a terminal assignment
	// return emits just the assignment (no `return;`).
	isTerminalReturn := false

	if namedDefer && v.currentFuncDecl != nil && v.currentFuncDecl.Body != nil {
		list := v.currentFuncDecl.Body.List
		isTerminalReturn = len(list) > 0 && list[len(list)-1] == returnStmt
	}

	if isTerminalReturn && !namedDeferAssign {
		return
	}

	result.WriteString(DeferredDeclsMarker)
	result.WriteString(v.indent(v.indentLevel))

	// Emit the results against the signature of the function/literal this `return` belongs to — a nested
	// function literal returns against ITS OWN results, not the enclosing function's (see convFuncLit).
	signature := v.currentReturnSignature

	if signature == nil {
		signature = v.currentFuncSignature
	}

	if namedDeferAssign {
		// A box-ref named result's assignment target reads through its box inside a function
		// literal's conversion (see namedReturnAssignTargets).
		assignTargets := v.namedReturnAssignTargets(signature)

		if len(assignTargets) > 1 {
			result.WriteString("(" + strings.Join(assignTargets, ", ") + ") = ")
		} else if len(assignTargets) == 1 {
			result.WriteString(assignTargets[0] + " = ")
		}
	} else if namedDefer && v.inGoFrame {
		// The named-result frame form's exit (§4.4): the results live outside the try and are
		// returned after the finally, so an exit from inside it leaves through a goto — which runs
		// the finally exactly as a return would, and unlike a return does not freeze a result value
		// the deferred calls are still entitled to change.
		result.WriteString("goto " + v.goFrameExitLabel())
		v.goFrameNamedExit = true
	} else {
		result.WriteString("return")
	}

	if returnStmt.Results == nil || explicitIsNamed {
		// In namedReturnDeferMode a naked return — or an explicit return of exactly the named
		// results — is just `return;` (the result params are already set). Otherwise:
		// In namedReturnDeferMode a naked return is just `return;` — the result params are
		// already assigned and are returned (post-defer) by the wrapper's caller side.
		if signature != nil && !namedDefer {
			// Check if result signature has named return values
			results := &strings.Builder{}

			for i := range signature.Results().Len() {
				if i > 0 && results.Len() > 0 {
					results.WriteString(", ")
				}

				if recvIndex != -1 && i == recvIndex {
					result.WriteString(capturedRecvName)
				} else {
					param := signature.Results().At(i)

					if param.Name() == "_" {
						results.WriteString("default!")
					} else if param.Name() != "" {
						ident := v.getVarIdent(param)

						if ident != nil {
							results.WriteString(getSanitizedIdentifier(v.getIdentName(ident)))
						} else {
							results.WriteString(getSanitizedIdentifier(param.Name()))
						}
					}
				}

			}
			if results.Len() > 0 {
				result.WriteRune(' ')

				if signature.Results().Len() > 1 {
					result.WriteRune('(')
					result.WriteString(results.String())
					result.WriteRune(')')
				} else {
					result.WriteString(results.String())
				}
			}
		}
	} else {
		// In namedReturnDeferMode the LHS already ends with "= "; otherwise separate "return"
		// from its operand with a space.
		if !namedDefer {
			result.WriteRune(' ')
		}

		basicLitContext := DefaultBasicLitContext()

		if len(returnStmt.Results) > 1 {
			result.WriteRune('(')

			// u8 readonly spans are not supported in value tuple
			basicLitContext.u8StringOK = false
			basicLitContext.spanTargetUnsupported = true
		}

		lambdaContext := DefaultLambdaContext()

		resultParams := signature.Results()
		resultParamIsInterface := paramsAreInterfaces(resultParams, true)
		resultParamIsPointer := v.paramsArePointers(resultParams)

		// A single multi-value CALL forwarded as the whole result list (`return newRawConn(f)`)
		// whose tuple elements need an interface conversion cannot convert in place — C# tuple
		// conversions do not consult user conversions element-wise (os SyscallConn returning
		// (*rawConn, error) as (syscall.RawConn, error), CS0266). Deconstruct into temps and
		// convert each element through the usual interface machinery (which also records the
		// GoImplement pairing). Elements whose actual type is itself an interface are left to
		// the structural-inheritance emission (see getStructuralInterfaceBases).
		forwarded := false

		if len(returnStmt.Results) == 1 && resultParams != nil && resultParams.Len() > 1 {
			if tuple, ok := v.getType(returnStmt.Results[0], false).(*types.Tuple); ok && tuple.Len() == resultParams.Len() {
				needsConversion := false

				for i := range tuple.Len() {
					declared := resultParams.At(i).Type()
					actual := tuple.At(i).Type()

					if declaredIsIface, _ := isInterface(declared); declaredIsIface && !types.Identical(declared, actual) {
						if _, actualIsIface := actual.Underlying().(*types.Interface); !actualIsIface {
							needsConversion = true
							break
						}
					}
				}

				if needsConversion {
					lambdaContext.deferredDecls = &strings.Builder{}
					callExpr := v.convExpr(returnStmt.Results[0], []ExprContext{basicLitContext, lambdaContext})

					if lambdaContext.deferredDecls.Len() > 0 {
						deferredDecls.WriteString(lambdaContext.deferredDecls.String())
					}

					tempNames := make([]string, tuple.Len())

					for i := range tuple.Len() {
						tempNames[i] = fmt.Sprintf("%s%d", TempVarMarker, i+1)
					}

					deferredDecls.WriteString(v.newline)
					deferredDecls.WriteString(v.indent(v.indentLevel))
					deferredDecls.WriteString(fmt.Sprintf("var (%s) = %s;", strings.Join(tempNames, ", "), callExpr))
					deferredDecls.WriteString(v.newline)

					result.WriteRune('(')

					for i := range tuple.Len() {
						if i > 0 {
							result.WriteString(", ")
						}

						declared := resultParams.At(i).Type()
						actual := tuple.At(i).Type()

						if declaredIsIface, declaredIfaceEmpty := isInterface(declared); declaredIsIface && !types.Identical(declared, actual) {
							if declaredIfaceEmpty {
								// The EMPTY interface takes the boundary treatment directly, NOT
								// the adapter route: `any` has no adapter to hold a pointer box, so
								// convertToInterfaceType finds no arm and falls through to its
								// pointer-DEREF prefix — boxing a COPY of the pointee where Go's
								// interface holds the pointer. See typedNilInterfaceBoxing.go.
								result.WriteString(v.applyTypedNilPointerBoxToType(actual, tempNames[i]))
							} else {
								result.WriteString(v.convertToInterfaceType(declared, actual, tempNames[i]))
							}
						} else {
							result.WriteString(tempNames[i])
						}
					}

					result.WriteRune(')')
					forwarded = true
				}
			}
		}

		if !forwarded {
			// gc spills a return's CALLS to temporaries before assembling the result list, so every
			// plain operand is read AFTER them; a C# tuple literal evaluates strictly left to right
			// and would read an operand BEFORE a later call mutates it. Emit gc's own rewrite where
			// that difference is observable — see returnOperandOrder.go, and lhsReusedInLaterRhs for
			// the parallel-assignment sibling of the same C#-sequencing-versus-Go-freedom problem.
			hoistThrough := v.returnMultiValueHoistThrough(returnStmt.Results)

			for i, expr := range returnStmt.Results {
				if i > 0 {
					result.WriteString(", ")
				}

				// The operand is rendered into its own builder so a hoisted one can be redirected
				// whole — every treatment below (interface conversion, clone, the narrowing and
				// constant casts) belongs to the VALUE, so it must land in the temporary, leaving
				// the tuple naming nothing but the temporary.
				elem := strings.Builder{}

				var replacementVal string

				lambdaContext.deferredDecls = &strings.Builder{}

				// A string literal RETURNED as an EMPTY interface (`any`) must box to object: emit
				// `(@string)"…"` (which boxes a golib @string, preserving Go string identity for a
				// later type assertion), NOT the `"…"u8` ReadOnlySpan<byte> that has no conversion to
				// object (CS0029; testing's `func (f *chattyFlag) Get() any { return "test2json" }`).
				// resultParamIsInterface excludes the empty interface (andNotEmptyInterface), so the
				// interface-conversion arm below never fires for `any`. Only string basic-literals
				// consult these flags (convBasicLit), so a non-string `any` result is unaffected.
				elemBasicLitContext := basicLitContext

				if resultParams != nil && i < resultParams.Len() {
					if isIface, isEmpty := isInterface(resultParams.At(i).Type()); isIface && isEmpty {
						elemBasicLitContext.u8StringOK = true
						elemBasicLitContext.castToGoString = true
						elemBasicLitContext.spanTargetUnsupported = true
					} else if isNamedStringType(resultParams.At(i).Type()) {
						// A string literal returned at a NAMED string result type needs the
						// @string step written out. Go converts the untyped constant to the named
						// type implicitly, but the emitted C# would have to cross TWO user-defined
						// conversions to get there — C# string → golib `@string` → the type's
						// `[GoType("@string")]` wrapper — and C# admits only one in a conversion,
						// so a bare `""` has no conversion to the named type at all (CS0029), and
						// in a tuple the failed element leaves its `default!` siblings with no
						// target type (CS8716). Naming the intermediate makes each step a separate
						// conversion. os's `getPollFDAndNetwork` is the reached case: its
						// `return nil, ""` at `(*poll.FD, poll.String)` is the linux corpus's L6.
						elemBasicLitContext.castToGoString = true
					}
				}

				// A POINTER value converting to an INTERFACE result must render as the box —
				// `Ꮡf`, not the deref-aliased receiver `f` — since the pointer-adapter wraps the
				// ж<T> itself (`new subFSᴵFS(Ꮡf)`; io/fs subFS.Sub returning its own receiver,
				// CS1503). Mirrors the argument-position rule in convExprList.
				exprContexts := []ExprContext{elemBasicLitContext, lambdaContext}

				if resultParamIsInterface != nil && i < len(resultParamIsInterface) && resultParamIsInterface[i] {
					if _, isPtr := v.getType(expr, false).(*types.Pointer); isPtr {
						identContext := DefaultIdentContext()
						identContext.isPointer = true
						exprContexts = []ExprContext{elemBasicLitContext, identContext, lambdaContext}
					}
				}

				// A deref-aliased pointer parameter returned WHOLE (`return p`) must yield its box
				// `Ꮡp` (the value alias cannot bind the pointer result). Render it in pointer ident
				// context — which resolves the box name through boxBaseName — rather than string-
				// prefixing the converted expression: inside a synthesized lambda the plain render is
				// already the box READ `Ꮡp.Value` (the ref-local alias is uncapturable), and prefixing
				// that emitted the undeclared `ᏑᏑp` (CS0103 — crypto/x509 AddCert's closure
				// `return cert, nil`). Only a DIRECT ident qualifies — `return it.key` (a field of a
				// pointer param) returns the FIELD value, and the box `Ꮡit` has no `key` (CS1061) —
				// and only a GENUINE `*T` parameter: an unsafe.Pointer parameter counts as a pointer
				// result type (isPointer includes the UnsafePointer basic) but renders as a plain
				// VALUE param with NO box (CS0103; runtime map.go `mapaccess1_fat`'s `return zero`).
				if resultParamIsPointer != nil && i < len(resultParamIsPointer) && resultParamIsPointer[i] {
					if ident, isIdent := expr.(*ast.Ident); isIdent && v.identIsParameter(ident) {
						// paramPointerType covers both a genuine `*T` parameter and an ERASED
						// pointer-core type parameter (`return p` under `[P *T]` — the emitted
						// result type is ж<T>, so the box must bind here too); an unsafe.Pointer
						// parameter still returns false and keeps its plain value render.
						if _, isRealPointer := v.paramPointerType(v.getIdentType(ident)); isRealPointer {
							identContext := DefaultIdentContext()
							identContext.isPointer = true
							exprContexts = []ExprContext{elemBasicLitContext, identContext, lambdaContext}
						}
					}
				}

				// An EMPTY-interface RESULT takes a POINTER as its BOX for the same reason the
				// pointer-RESULT arm above does — Go's interface value holds the *T. Without it a
				// deref-aliased pointer parameter returned as `any` boxes a COPY of the POINTEE
				// (dynamic type T where Go says *T, and pointer identity gone), and a NIL one
				// panics at the deref instead of crossing intact. See typedNilInterfaceBoxing.go.
				if resultParams != nil && i < resultParams.Len() {
					exprContexts = v.emptyInterfacePointerContexts(resultParams.At(i).Type(), expr, exprContexts)

					// A `return nil` whose RESULT is a pointer-to-array carries the array's length for
					// the same reason an assignment does -- `array<E>` erases it and the caller's
					// reflection sees only the erased type. archive/tar's readHeader is the corpus's
					// shape (`return nil, nil, err`, result `*block` = `[512]byte`).
					exprContexts = v.appendNilArrayDimsTypeContext(exprContexts, expr, resultParams.At(i).Type())
				}

				resultExpr := v.convExpr(expr, exprContexts)

				// A Go array RETURNED by value out of existing storage takes the strongly-typed
				// `.Clone()` so the caller receives independent backing storage: the callee may
				// have leaked the local's address (`return arr` after `p = &arr`), or the result
				// reads a field/element/deref some other name still reaches — and the emitted
				// struct copy aliases the shared backing either way (see
				// exprReadsValueNeedingClone). Applied before any interface conversion below,
				// so a boxed array result (`func f() any { return arr }`) boxes the clone.
				if v.exprReadsValueNeedingClone(expr) {
					resultExpr = appendValueClone(resultExpr, v.getExprType(expr))
				}

				// A bidirectional channel returned as a DIRECTIONAL result narrows the Go type with
				// no construction to hook (`func f() <-chan T { return ch }`) — the return arm of
				// the live-copy narrowing (see chanDirNarrowedValue). Keyed on the declared RESULT
				// type versus the expression's own, so a result that is already directional, or
				// bidirectional on both sides, renders unchanged.
				if resultParams != nil && i < resultParams.Len() {
					if narrowedChan := v.chanDirNarrowedValue(resultParams.At(i).Type(), v.getExprType(expr), resultExpr); narrowedChan != "" {
						resultExpr = narrowedChan
					}
				}

				// Box an untyped CONSTANT returned as an EMPTY interface at Go's default type (the
				// numeric twin of the castToGoString @string boxing above), so a later `x.(int)` on the
				// result matches Go's boxed `int` dynamic type. A non-empty interface result routes
				// through convertToInterfaceType below (which the empty interface deliberately bypasses),
				// and no numeric result type reaches the empty interface, so this is the only return-path
				// site that needs the cast.
				// The POINTER twin follows it: a nil pointer returned as `any` carries its Go type
				// across (typedNilInterfaceBoxing.go).
				if resultParams != nil && i < resultParams.Len() {
					resultExpr = v.boxUntypedConstAsDefaultType(resultParams.At(i).Type(), expr, resultExpr)
					resultExpr = v.boxPointerIntoEmptyInterface(resultParams.At(i).Type(), expr, resultExpr)
				}

				// Record any dynamic-struct implicit conversion AFTER converting the result expr.
				// checkForDynamicStructs resolves each side's C# name through liftedTypeMap, but an
				// anonymous-struct COMPOSITE LITERAL returned here (`return struct{…}{…}`) is only
				// lifted into that registry DURING its convExpr above. Recording earlier read the
				// unlifted arg and stringified its type as raw Go `struct{…}` text — an invalid C#
				// generic argument in the emitted `[assembly: GoImplicitConv<…>]` (CS1031). By this
				// point the literal's lifted name (e.g. `fn_type`) is registered, so both the source
				// and the func-result target resolve to their lifted names. (The dynamicCast template
				// this may return is applied identically below, so the reorder is otherwise neutral.)
				if resultParams != nil && i < resultParams.Len() {
					argType := v.getType(expr, false)
					targetType := resultParams.At(i).Type()
					replacementVal = v.checkForDynamicStructs(argType, targetType)
				}

				if len(replacementVal) > 0 {
					resultExpr = strings.ReplaceAll(replacementVal, DynamicCastArgMarker, resultExpr)
				}

				if lambdaContext.deferredDecls.Len() > 0 {
					deferredDecls.WriteString(lambdaContext.deferredDecls.String())
				}

				if resultParamIsInterface != nil && i < len(resultParamIsInterface) && resultParamIsInterface[i] {
					resultParamType := resultParams.At(i).Type()
					elem.WriteString(v.convertToInterfaceType(resultParamType, v.getType(expr, false), resultExpr))
				} else {
					var narrowCast string
					var lambdaConstCast string
					var crossBaseCast string
					if resultParams != nil && i < resultParams.Len() {
						narrowCast = v.narrowArithmeticCastTypeFor(resultParams.At(i).Type(), expr, resultExpr)

						if len(narrowCast) == 0 {
							lambdaConstCast = v.lambdaConstReturnCastType(resultParams.At(i).Type(), expr)
						}

						// Same slot, same reason (CS8917), other typeless shape — a returned func literal.
						if len(narrowCast) == 0 && len(lambdaConstCast) == 0 {
							lambdaConstCast = v.lambdaFuncLitReturnCastType(resultParams.At(i).Type(), expr)
						}

						if len(narrowCast) == 0 && len(lambdaConstCast) == 0 {
							crossBaseCast = v.crossBaseConstCastFor(resultParams.At(i).Type(), expr)
						}
					}

					if recvIndex != -1 && i == recvIndex {
						elem.WriteString(capturedRecvName)
					} else if len(crossBaseCast) > 0 {
						// A CONSTANT result whose type is defined over a CROSS-PACKAGE named base
						// (`return 0, err` with a registry Key result) hops through the base — the
						// [GoType("syscall_package.ΔHandle")] wrapper has no numeric bridge (CS0029).
						elem.WriteString(crossBaseCast + "(" + resultExpr + ")")
					} else if len(lambdaConstCast) > 0 {
						// A constant integer-literal return inside a lambda whose result is an unsigned/
						// pointer-sized integer must be cast to that type so the delegate return type is
						// inferable (CS8917) — see lambdaConstReturnCastType.
						elem.WriteString("(" + lambdaConstCast + ")(" + resultExpr + ")")
					} else if len(narrowCast) > 0 {
						// A narrow-integer (byte/int8/uint8/int16/uint16) arithmetic RESULT is promoted to
						// `int` by C#, so returning it from a narrow-typed function needs the cast back to
						// compile and to preserve Go's wrap (CS0266) — `func lowerASCII(c byte) byte { return
						// c + ('a'-'A') }`. The assignment / value-spec paths already narrow such a result;
						// the return path had omitted it. narrowArithmeticCastTypeFor gates on a binary/unary
						// arith RHS whose Go type matches the (narrow) result type, and skips a whole-expr
						// already-cast RHS — so a bare ident / call / already-narrowed return is untouched.
						elem.WriteString("(" + narrowCast + ")(" + resultExpr + ")")
					} else {
						// B′-S1: forwarding a bare-receiver call to a ref-return primary — return
						// the ref it yields (`return ref v.M(…)`). See refReturnForwardIndex above.
						if i == refReturnForwardIndex {
							elem.WriteString("ref ")
						}

						elem.WriteString(resultExpr)
					}
				}

				// Spill this operand's call to a temporary ahead of the statement so the tuple's
				// PLAIN operands are read after it, as gc reads them. Every call-bearing operand at
				// or below the hazard index spills, keeping the calls in their own lexical order —
				// the one ordering Go's spec does fix.
				// A result that recorded a keep-alive (a bridged-wrapper argument, Q49) is hoisted
				// into a temp exactly like a multi-value operand, so the KeepAlive lines below land
				// between the call and the `return` — a `return f(noescape(unsafe.Pointer(&x)))`
				// becomes `var ᴛN = f(…); System.GC.KeepAlive(Ꮡx); return ᴛN;`.
				if (i <= hoistThrough || len(v.pendingSyscallKeepAlive) > 0) && v.exprEvaluatesCall(expr) {
					v.tupleTempIndex++
					temp := fmt.Sprintf("%s%d", TempVarMarker, v.tupleTempIndex)

					// The slot's leading newline is written once: a second spill in the same
					// return would otherwise leave a blank line between the two declarations.
					if pending := deferredDecls.String(); pending == "" || !strings.HasSuffix(pending, v.newline) {
						deferredDecls.WriteString(v.newline)
					}

					deferredDecls.WriteString(v.indent(v.indentLevel))
					deferredDecls.WriteString("var " + temp + " = " + elem.String() + ";")
					deferredDecls.WriteString(v.newline)

					elem.Reset()
					elem.WriteString(temp)
				}

				result.WriteString(elem.String())
			}

			if len(returnStmt.Results) > 1 {
				result.WriteRune(')')
			}
		}

		// The return-statement drain (Q49): every keep-alive recorded while converting the results
		// is written after their hoisted temps and before the `return`. Shapes whose result is not
		// a plain expression the hoist above can capture — a receiver return, a ref-return
		// forwarding, the named-return defer protocol — refuse loudly rather than emit a KeepAlive
		// that fires after the return; Go 1.23.12 has no such site (the census found the class in
		// plain returns only).
		if len(v.pendingSyscallKeepAlive) > 0 {
			if namedDefer || recvIndex != -1 || refReturnForwardIndex != -1 {
				panic(fmt.Sprintf(
					"@visitReturnStmt - a keep-alive was recorded inside a return statement the hoist cannot carry (namedDefer=%v recv=%v refForward=%v): %s -- names: %v",
					namedDefer, recvIndex != -1, refReturnForwardIndex != -1, v.getPrintedNode(returnStmt), v.pendingSyscallKeepAlive,
				))
			}

			for _, name := range dedupeKeepAliveNames(v.pendingSyscallKeepAlive) {
				if pending := deferredDecls.String(); pending == "" || !strings.HasSuffix(pending, v.newline) {
					deferredDecls.WriteString(v.newline)
				}

				deferredDecls.WriteString(v.indent(v.indentLevel))
				deferredDecls.WriteString("System.GC.KeepAlive(" + name + ");")
				deferredDecls.WriteString(v.newline)
			}

			v.pendingSyscallKeepAlive = nil
		}
	}

	result.WriteRune(';')

	// In namedReturnDeferMode a non-terminal explicit return assigned the named result params
	// above; follow it with a bare `return;` to exit the void func() wrapper. A terminal return
	// needs none (the wrapper falls off its end), and a naked/collapsed return already produced
	// just `return;`.
	if namedDeferAssign && !isTerminalReturn {
		if v.inGoFrame {
			result.WriteString(" goto " + v.goFrameExitLabel() + ";")
			v.goFrameNamedExit = true
		} else {
			result.WriteString(" return;")
		}
	}

	// Hoisted capture-snapshot decls (each `\n<indent>decl;` with a trailing newline) precede any
	// deferred decls in the marker slot; the return's own indent follows the slot.
	hoisted := hoistBuf.String() + deferredDecls.String()

	if len(hoisted) == 0 {
		hoisted = v.newline
	}

	v.outputBuilder.WriteString(strings.ReplaceAll(result.String(), DeferredDeclsMarker, hoisted))
}

// crossBaseConstCastFor returns the two-step cast chain for an untyped-CONSTANT result whose
// type is a defined type over a CROSS-PACKAGE named base — `return 0, err` with a registry Key
// result: the [GoType("syscall_package.ΔHandle")] wrapper declares operators only to/from that
// base (no numeric bridge), so the bare literal has no implicit path (CS0029). The chain hops
// the base with one user conversion per cast: `((Key)(syscall_package.ΔHandle)(0))`. Returns ""
// when not applicable (non-constant expr, non-named result, same-package or non-numeric base —
// same-package chains resolve to [GoType("num:…")] with a direct bridge).
func (v *Visitor) crossBaseConstCastFor(targetType types.Type, expr ast.Expr) string {
	tv, ok := v.info.Types[expr]

	if !ok || tv.Value == nil {
		return ""
	}

	named, ok := types.Unalias(targetType).(*types.Named)

	if !ok {
		return ""
	}

	rhs, okRHS := packageTypeSpecRHS[named.Obj()]

	if !okRHS || rhs == nil {
		return ""
	}

	rhsNamed, ok := types.Unalias(rhs).(*types.Named)

	if !ok || rhsNamed.Obj().Pkg() == named.Obj().Pkg() {
		return ""
	}

	if basic, ok := rhsNamed.Underlying().(*types.Basic); !ok || basic.Info()&types.IsNumeric == 0 {
		return ""
	}

	return "(" + v.getCSharpTypeName(named) + ")(" + v.getCSharpTypeName(rhsNamed) + ")"
}
