// refLoweringEmissionOperations.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// This file owns the ж-box reduction arc's REF-LOWERING EMISSION — stage A2 of
// docs/phase4/DESIGN-zh-box-reduction.md (§3.3, §3.4; rulings §10.1/§10.3/§10.4). The
// classification pass (refLoweringAnalysisOperations.go) decides WHAT lowers; this file decides
// what the lowered code LOOKS like:
//
//   - A lowered pointer parameter emits as `ref T name` (visitFuncDecl reads paramIsRefLowered);
//     the entry deref alias disappears because the parameter IS the alias.
//   - Every call-site argument at a lowered position emits as a `ref` expression per the §3.3
//     rows (refLoweredArgReplacement below — the seven-row emission table).
//   - `defer`/`go` call sites are BOXED sites, categorically (§3.3): the eager arguments keep
//     today's boxed emission and the invoke-time thunk derives each ref
//     (`ᴛ1 => f(ref ᴛ1.DerefOrNull())`) — visitDeferStmt/visitGoStmt force the temp-param lambda
//     form and convExprList wraps the marker.
//
// The nil doctrine (ruling §10.4): lowered field/element address formation is EAGERLY
// nil-checked via golib's zero-allocation `nonnil(ref …)` helper, landing the lowered form
// exactly on Go (eager panic at `&e.x`, later arguments unevaluated). The wrap is elided where
// the base provably cannot be a null reference (a value local/parameter/result, an addressed
// global's ref property); it is emitted where the base is a pointer's deref alias, whose
// DerefOrNull binding is a null reference exactly when the pointer is nil.
//
// EVERY row has a boxed fallback (`ref (<today's boxed render>).DerefOrNull()`): a shape this
// file cannot prove it renders correctly keeps today's allocation and today's aliasing, derefs
// eagerly at argument evaluation (S-F2's documented at-or-between-Go-and-today timing), and
// still satisfies the `ref T` parameter. The fallback is what makes the emission TOTAL over the
// shapes the classifier admits — soundness never rests on this file's coverage being complete,
// only its coverage being correct.

package main

import (
	"fmt"
	"go/ast"
	"go/types"
	"strings"
)

// refLoweredCalleePositions resolves a call's callee against the current package's Phase-A
// ref-lowering verdicts and returns the set of parameter positions that lowered — nil when the
// callee is not a same-package lowered function (imported callees cannot lower in Phase A:
// scope is unexported functions, whose callers are all within the declaring package — §10.1).
func (v *Visitor) refLoweredCalleePositions(callExpr *ast.CallExpr) map[int]bool {
	result := packageRefLoweringResult

	if result == nil || len(result.LoweredPositions) == 0 {
		return nil
	}

	fun := ast.Unparen(callExpr.Fun)

	// Strip a generic instantiation: f[T](…) / f[T1, T2](…).
	switch indexed := fun.(type) {
	case *ast.IndexExpr:
		fun = ast.Unparen(indexed.X)
	case *ast.IndexListExpr:
		fun = ast.Unparen(indexed.X)
	}

	ident, ok := fun.(*ast.Ident)

	if !ok {
		return nil
	}

	obj, ok := v.info.Uses[ident].(*types.Func)

	if !ok || obj.Pkg() == nil || obj.Pkg().Path() != result.PkgPath {
		return nil
	}

	signature, ok := obj.Type().(*types.Signature)

	if !ok || signature.Recv() != nil {
		return nil
	}

	verdict := result.Funcs[obj.Name()]

	if verdict == nil {
		return nil
	}

	var lowered map[int]bool

	for _, param := range verdict.Params {
		if param.LoweredA {
			if lowered == nil {
				lowered = map[int]bool{}
			}

			lowered[param.Index] = true
		}
	}

	return lowered
}

// paramIsRefLowered reports whether this exact parameter object lowered under the Phase-A fixed
// point. Keyed by *types.Var identity, so receiver-offset indices and shadow renames are
// irrelevant.
func (v *Visitor) paramIsRefLowered(param *types.Var) bool {
	result := packageRefLoweringResult

	return result != nil && result.LoweredParamVars[param]
}

// refLoweredArgReplacement renders the complete `ref` argument expression for one argument at a
// Phase-A-lowered position — the §3.3 emission rows. The returned text is a TOTAL replacement
// (no DynamicCastArgMarker), so convExprList skips the default boxed render entirely.
// deferredDecls is the enclosing call's statement hoist sink (rows 5/6 hoist a temp); nil is
// tolerated — v.hoistedDecls is the statement-level fallback sink, and a site with no sink at
// all takes the boxed fallback.
func (v *Visitor) refLoweredArgReplacement(arg ast.Expr, paramType types.Type, deferredDecls *strings.Builder) string {
	shape, detail := (&refLoweringAnalysis{info: v.info}).classifyArgShape(arg, false)

	switch shape {
	case refShapeFieldAddr, refShapeElemAddr:
		// Rows 1 and 4: &e.x / &s[i] — a derived address whose storage the ref aliases directly.
		return v.refLoweredAddressArg(arg)

	case refShapeLocalAddr:
		// Row 2: &x — the address-taken local/parameter/result/global.
		return v.refLoweredLocalAddrArg(arg)

	case refShapePointerVar:
		// Row 3: a pointer-valued expression — carries a box; the ref derefs it at the argument.
		return v.refLoweredPointerVarArg(arg, detail)

	case refShapePtrConv:
		// Row 5 (§10.3's hoisted-temp rule): (*T2)(&v.x) — hoist the reinterpreted VALUE into a
		// temp and pass `ref ᴛN`. Byte-parity with today's copied-header/shared-backing semantics.
		return v.refLoweredPtrConvArg(arg, deferredDecls)

	case refShapeTempNeeded:
		// Row 6: &T{…} hoists a temp; new(T) keeps today's box through the fallback; a
		// box-yielding call/receive result derefs inline (single evaluation, argument order).
		switch detail {
		case "composite-lit":
			return v.refLoweredCompositeTempArg(arg, deferredDecls)
		case "call-result", "chan-recv":
			return fmt.Sprintf("ref (%s).DerefOrNull()", v.refBoxedArgRender(arg))
		default:
			// "new" and the builtin-* shapes: the boxed render already yields the box.
			return v.refLoweredBoxedFallback(arg)
		}

	case refShapeNilLit:
		// Row 7: the literal nil — bind the null ref; the callee's first use faults (Go's
		// deferred nil timing for a nil pointer ARGUMENT, per the §3.3 doctrine).
		return fmt.Sprintf("ref ((%s)default!).DerefOrNull()", v.getCSharpTypeName(paramType))

	default:
		// refShapeOtherVeto cannot reach a LOWERED position (the two-sided fixed point stripped
		// those); defensively keep today's box if it ever does.
		return v.refLoweredBoxedFallback(arg)
	}
}

// refLoweredBoxedFallback renders the §3.3 universal fallback: today's boxed emission of the
// whole argument, deref'd eagerly at the argument position. Allocation and aliasing are exactly
// today's; the deref timing is at-or-between Go's and today's (S-F2). Every emission path in
// this file that cannot PROVE its faster form is correct lands here.
func (v *Visitor) refLoweredBoxedFallback(arg ast.Expr) string {
	return fmt.Sprintf("ref (%s).DerefOrNull()", v.refBoxedArgRender(arg))
}

// refBoxedArgRender renders the argument exactly as the boxed convention would at a pointer
// position (a bare pointer parameter renders as its box `Ꮡp`, matching argTypeIsPtr).
func (v *Visitor) refBoxedArgRender(arg ast.Expr) string {
	ptrIdentContext := DefaultIdentContext()
	ptrIdentContext.isPointer = true

	return v.convExpr(arg, []ExprContext{ptrIdentContext})
}

// refConversionInLambda reports whether the current rendering position sits inside a lambda
// body. Lambda capture renders reference captured/boxed forms whose value chains this file's
// alias-rooted templates cannot prove ref-able, so those templates decline into the boxed
// fallback there (census-reverted locals never appear inside lambdas — a closure-crossing use
// keeps the box — so no reverted emission is lost).
func (v *Visitor) refConversionInLambda() bool {
	return v.lambdaCapture != nil && v.lambdaCapture.conversionInLambda
}

// refChainRootIdent walks a selector/index/deref chain to its root identifier (nil when the
// chain roots elsewhere — a call result, a composite literal…).
func refChainRootIdent(chain ast.Expr) *ast.Ident {
	for {
		chain = ast.Unparen(chain)

		switch e := chain.(type) {
		case *ast.Ident:
			return e
		case *ast.SelectorExpr:
			chain = e.X
		case *ast.IndexExpr:
			chain = e.X
		case *ast.StarExpr:
			chain = e.X
		default:
			return nil
		}
	}
}

// refRootNullability classifies a chain root for the §10.4 nil doctrine:
//
//	refRootValue    — provably never a null reference (value local/param/result, global ref
//	                  property, slice/array/map variable): elide nonnil.
//	refRootNullable — a pointer's deref alias (deref-aliased parameter, direct-ж receiver, or a
//	                  lowered `ref` parameter), null exactly when the pointer is nil: wrap nonnil.
//	refRootUnproven — anything else (pointer local/field, unresolvable): boxed fallback.
type refRootKind int

const (
	refRootValue refRootKind = iota
	refRootNullable
	refRootUnproven
)

func (v *Visitor) refClassifyChainRoot(rootIdent *ast.Ident) refRootKind {
	obj := v.info.ObjectOf(rootIdent)

	if obj == nil {
		return refRootUnproven
	}

	rootVar, ok := obj.(*types.Var)

	if !ok {
		return refRootUnproven
	}

	if _, isPtr := v.paramPointerType(rootVar.Type()); !isPtr {
		// A value-typed root can never be a null reference: a plain local/parameter/result
		// renders its own storage, a kept-box local renders its entry ref alias (never null —
		// heap() always allocates), and an addressed global renders its ref property over an
		// eagerly-created box.
		return refRootValue
	}

	// Pointer-typed root: only the deref-ALIAS forms render as a bare value name the templates
	// can wrap (the alias binds DerefOrNull at entry — null exactly when the pointer is nil).
	if v.paramIsRefLowered(rootVar) {
		return refRootNullable
	}

	if v.identIsParameter(rootIdent) {
		return refRootNullable
	}

	// The current method's receiver (direct-ж or plain ref-receiver): its value alias carries
	// the same nil-deferring deref binding as a parameter's.
	if v.currentFuncSignature != nil {
		if recv := v.currentFuncSignature.Recv(); recv != nil && obj == recv {
			return refRootNullable
		}
	}

	// A pointer LOCAL/FIELD root has no value alias — the value chain renders through the box,
	// which the split templates cannot wrap; the boxed fallback keeps today's exact semantics.
	return refRootUnproven
}

// refRootIsReassigned reports whether the pointer variable itself is DIRECTLY re-pointed
// (`p = q`, `p, x = …`, `for _, p = range …`) in the current function body — the one shape that
// makes the entry deref alias stale, so alias-rooted templates decline into the boxed fallback
// (which reads the box at call time, immune to staleness). Writes THROUGH the pointer
// (`*p = x`, `p.f = x`) and address-takes are not re-points and do not count. Lowered
// parameters can never be re-pointed (X4 vetoes), so this only ever fires for the unlowered
// deref-aliased forms.
func (v *Visitor) refRootIsReassigned(obj types.Object) bool {
	if v.currentFuncDecl == nil || v.currentFuncDecl.Body == nil {
		return true // no body context to prove stability — decline
	}

	repointed := false

	ast.Inspect(v.currentFuncDecl.Body, func(n ast.Node) bool {
		if repointed {
			return false
		}

		switch node := n.(type) {
		case *ast.AssignStmt:
			for _, lhs := range node.Lhs {
				if ident, ok := ast.Unparen(lhs).(*ast.Ident); ok && v.info.ObjectOf(ident) == obj && ident.Pos() != obj.Pos() {
					repointed = true
				}
			}
		case *ast.RangeStmt:
			for _, target := range []ast.Expr{node.Key, node.Value} {
				if target == nil {
					continue
				}

				if ident, ok := ast.Unparen(target).(*ast.Ident); ok && v.info.ObjectOf(ident) == obj {
					repointed = true
				}
			}
		}

		return !repointed
	})

	return repointed
}

// refLoweredAddressArg renders rows 1 and 4 — `&CHAIN` where CHAIN is a selector/index chain
// that stays in its root's own storage (the classifier's ptr-field-chain screen guarantees no
// intermediate pointer hops):
//
//	value-rooted:  ref e.x            (nonnil elided — the base cannot be null)
//	alias-rooted:  ref nonnil(ref e).x (eager nil panic at address formation — Go's timing)
//
// The alias-rooted form is built by a base/rest SPLIT of the rendered value chain, self-checked:
// if the render does not begin with the root's bare value name, the shape is one this template
// does not understand and the boxed fallback keeps today's semantics.
func (v *Visitor) refLoweredAddressArg(arg ast.Expr) string {
	unary, ok := ast.Unparen(arg).(*ast.UnaryExpr)

	if !ok {
		return v.refLoweredBoxedFallback(arg)
	}

	chain := ast.Unparen(unary.X)
	rootIdent := refChainRootIdent(chain)

	if rootIdent == nil || v.refConversionInLambda() {
		return v.refLoweredBoxedFallback(arg)
	}

	switch v.refClassifyChainRoot(rootIdent) {
	case refRootValue:
		// The whole value chain is directly ref-able (fields are fields, indexers and promoted
		// members are ref-returning); a render this template cannot prove is caught by the C#
		// compiler backstop, never silently mis-aliased (ref of a non-variable is a hard error).
		return "ref " + v.convExpr(chain, nil)

	case refRootNullable:
		obj := v.info.ObjectOf(rootIdent)

		if obj != nil && !v.paramIsRefLoweredObj(obj) && v.refRootIsReassigned(obj) {
			return v.refLoweredBoxedFallback(arg)
		}

		base := getSanitizedIdentifier(v.getIdentName(rootIdent))
		full := v.convExpr(chain, nil)
		rest, ok := refSplitRenderedChain(full, base)

		if !ok {
			return v.refLoweredBoxedFallback(arg)
		}

		return fmt.Sprintf("ref nonnil(ref %s)%s", base, rest)

	default:
		return v.refLoweredBoxedFallback(arg)
	}
}

// refSplitRenderedChain verifies the rendered chain begins with the root's bare value name
// followed by a member/index hop, returning the rest. Any other rendering (a box form, a
// captured rename, a deref wrapper) fails the split and the caller falls back.
func refSplitRenderedChain(full, base string) (string, bool) {
	if base == "" || !strings.HasPrefix(full, base) || len(full) <= len(base) {
		return "", false
	}

	switch full[len(base)] {
	case '.', '[':
		return full[len(base):], true
	}

	return "", false
}

// refLoweredLocalAddrArg renders row 2 — `&x`:
//
//	reverted local (no box): ref x    — the plain local IS the storage
//	kept box:                ref x    — the entry ref alias names the same box storage
//	addressed global:        ref x    — the global's ref property
//
// Inside a lambda a kept-box local renders through its capture forms; the boxed fallback keeps
// today's aliasing there (a reverted local can never appear inside a lambda — closure-crossing
// address uses keep the box).
func (v *Visitor) refLoweredLocalAddrArg(arg ast.Expr) string {
	unary, ok := ast.Unparen(arg).(*ast.UnaryExpr)

	if !ok {
		return v.refLoweredBoxedFallback(arg)
	}

	ident, ok := ast.Unparen(unary.X).(*ast.Ident)

	if !ok {
		return v.refLoweredBoxedFallback(arg)
	}

	obj := v.info.ObjectOf(ident)

	reverted := false

	if result := packageRefLoweringResult; result != nil && obj != nil {
		if objVar, isVar := obj.(*types.Var); isVar {
			reverted = result.RevertedLocalVars[objVar]
		}
	}

	if !reverted && v.refConversionInLambda() {
		return v.refLoweredBoxedFallback(arg)
	}

	return "ref " + v.convExpr(ident, nil)
}

// refLoweredPointerVarArg renders row 3 — a pointer-valued expression:
//
//	a lowered `ref` parameter forwarded (D2): ref p             — it already IS the ref
//	anything else (local, box read, field, deref, assert):      ref (<box>).DerefOrNull()
//
// The deref binds the identical null ref one frame earlier for a nil pointer; the fault still
// happens at first callee use (§3.3's byte-for-byte case).
func (v *Visitor) refLoweredPointerVarArg(arg ast.Expr, detail string) string {
	expr := ast.Unparen(arg)

	if ident, ok := expr.(*ast.Ident); ok {
		if obj := v.info.ObjectOf(ident); obj != nil && v.paramIsRefLoweredObj(obj) {
			return "ref " + getSanitizedIdentifier(v.getIdentName(ident))
		}
	}

	// &*p re-addresses a deref — the pointer itself is the inner operand.
	if detail == "re-addressed-deref" {
		if unary, ok := expr.(*ast.UnaryExpr); ok {
			return fmt.Sprintf("ref (%s).DerefOrNull()", v.refBoxedArgRender(ast.Unparen(unary.X)))
		}
	}

	return fmt.Sprintf("ref (%s).DerefOrNull()", v.refBoxedArgRender(arg))
}

// paramIsRefLoweredObj is paramIsRefLowered over a types.Object.
func (v *Visitor) paramIsRefLoweredObj(obj types.Object) bool {
	objVar, ok := obj.(*types.Var)

	return ok && v.paramIsRefLowered(objVar)
}

// refLoweredHoistTemp writes `var ᴛN = <value>;` into the statement hoist sink and returns the
// temp name — empty when no sink exists (the caller falls back). Mirrors the tuple-expansion
// hoist shape exactly (per-file monotonic marker index; leading newline + indentation).
func (v *Visitor) refLoweredHoistTemp(value string, deferredDecls *strings.Builder) string {
	sink := deferredDecls

	if sink == nil {
		sink = v.hoistedDecls
	}

	if sink == nil {
		return ""
	}

	v.tupleTempIndex++
	temp := fmt.Sprintf("%s%d", TempVarMarker, v.tupleTempIndex)

	sink.WriteString(v.newline)
	sink.WriteString(v.indent(v.indentLevel))
	sink.WriteString(fmt.Sprintf("var %s = %s;", temp, value))
	sink.WriteString(v.newline)

	return temp
}

// refLoweredPtrConvArg renders row 5 — the ruled hoisted-temp mechanism (§10.3) for a
// conversion of an address, `(*T2)(&CHAIN)`: hoist the chain VALUE, reinterpreted to the target
// element type, and pass `ref ᴛN`. The generated named-array wrapper's `Value` property yields
// its underlying `array<T>` header — a copy whose `T[]` backing is SHARED — so element writes
// flow through and whole-header writes are lost in both emissions equally (the byte-parity
// argument). The chain value is read through the same nonnil/value-rooted base treatment as
// row 1, so a nil base panics eagerly at the address formation (Go's timing).
func (v *Visitor) refLoweredPtrConvArg(arg ast.Expr, deferredDecls *strings.Builder) string {
	call, ok := ast.Unparen(arg).(*ast.CallExpr)

	if !ok || len(call.Args) != 1 || v.refConversionInLambda() {
		return v.refLoweredBoxedFallback(arg)
	}

	unary, ok := ast.Unparen(call.Args[0]).(*ast.UnaryExpr)

	if !ok {
		return v.refLoweredBoxedFallback(arg)
	}

	chain := ast.Unparen(unary.X)
	rootIdent := refChainRootIdent(chain)

	if rootIdent == nil {
		return v.refLoweredBoxedFallback(arg)
	}

	// The chain value expression, nil-checked per the doctrine when the base is nullable.
	var chainValue string

	switch v.refClassifyChainRoot(rootIdent) {
	case refRootValue:
		chainValue = v.convExpr(chain, nil)

	case refRootNullable:
		obj := v.info.ObjectOf(rootIdent)

		if obj != nil && !v.paramIsRefLoweredObj(obj) && v.refRootIsReassigned(obj) {
			return v.refLoweredBoxedFallback(arg)
		}

		base := getSanitizedIdentifier(v.getIdentName(rootIdent))
		full := v.convExpr(chain, nil)
		rest, ok := refSplitRenderedChain(full, base)

		if !ok {
			return v.refLoweredBoxedFallback(arg)
		}

		chainValue = fmt.Sprintf("nonnil(ref %s)%s", base, rest)

	default:
		return v.refLoweredBoxedFallback(arg)
	}

	// Reinterpret the chain value to the conversion target's element type. The corpus shape
	// (both fiat families) is named-wrapper → raw/aliased array, served by the wrapper's
	// `.Value`; identical types need nothing; any other pairing keeps today's boxed emission.
	// Both sides compare UNALIASED (fiat's `p224UntypedFieldElement` is `= [4]uint64`, whose
	// C# spelling is a global using alias — the identity underneath is what decides).
	targetType := v.info.TypeOf(call)

	if targetType == nil {
		return v.refLoweredBoxedFallback(arg)
	}

	targetPtr, ok := types.Unalias(targetType).(*types.Pointer)

	if !ok {
		return v.refLoweredBoxedFallback(arg)
	}

	sourceType := v.info.TypeOf(chain)

	if sourceType == nil {
		return v.refLoweredBoxedFallback(arg)
	}

	// The pairing DECISION is the shared predicate the locals census also consults
	// (refConvPairingSupported) — the array-wrapper family only, where the temp's copied header
	// shares its `T[]` backing. The generated `Value` property lazily materializes that backing
	// IN the source variable when read through one (struct property access binds `ref this`),
	// and the `[GoType]` wrapper carries the array↔wrapper conversions the cast forms bind.
	mechanism, supported := refConvPairingSupported(sourceType, targetPtr.Elem())

	if !supported {
		// An unsupported pairing keeps the boxed convention — byte-parity because the census's
		// matching arm guarantees the root KEPT its identity box (the shared-predicate
		// agreement); the warning is the tripwire should the two ever disagree again.
		if rootObj := v.info.ObjectOf(rootIdent); rootObj != nil {
			if rootVar, isVar := rootObj.(*types.Var); isVar && packageRefLoweringResult != nil && packageRefLoweringResult.RevertedLocalVars[rootVar] {
				v.showWarning("ref-lowered conversion argument over reverted local '%s' has no primary emission - boxed fallback may split storage: %s",
					rootIdent.Name, v.getPrintedNode(arg))
			}
		}

		return v.refLoweredBoxedFallback(arg)
	}

	targetCS := v.getCSharpTypeName(types.Unalias(targetPtr.Elem()))
	value := ""

	switch mechanism {
	case "identity":
		value = chainValue
	case "unwrap":
		// wrapper → its raw array
		value = chainValue + ".Value"
	case "wrap":
		// raw array → wrapper (one user-defined conversion)
		value = fmt.Sprintf("(%s)(%s)", targetCS, chainValue)
	case "rewrap":
		// wrapper → different wrapper over the same array: hop through the raw array (C# will
		// not chain two user-defined conversions in one cast)
		value = fmt.Sprintf("(%s)((%s).Value)", targetCS, chainValue)
	default:
		return v.refLoweredBoxedFallback(arg)
	}

	temp := v.refLoweredHoistTemp(value, deferredDecls)

	if temp == "" {
		return v.refLoweredBoxedFallback(arg)
	}

	return "ref " + temp
}

// refLoweredCompositeTempArg renders row 6's composite-literal shape — `&T{…}`: hoist the
// constructed value into a temp and pass `ref ᴛN`. Observationally identical to a distinct heap
// box for a lowered callee (its address is never compared, stored, escaped, or converted — the
// D/X rules), which is the §3.3 rows-5-7 justification.
func (v *Visitor) refLoweredCompositeTempArg(arg ast.Expr, deferredDecls *strings.Builder) string {
	unary, ok := ast.Unparen(arg).(*ast.UnaryExpr)

	if !ok {
		return v.refLoweredBoxedFallback(arg)
	}

	sink := deferredDecls

	if sink == nil {
		sink = v.hoistedDecls
	}

	if sink == nil {
		return v.refLoweredBoxedFallback(arg)
	}

	// Render the literal VALUE with the hoist sink threaded so a func-literal field's capture
	// declarations land above the temp (ordering: captures first, then the temp that uses them).
	hoistLambdaContext := DefaultLambdaContext()
	hoistLambdaContext.deferredDecls = sink

	temp := v.refLoweredHoistTemp(v.convExpr(ast.Unparen(unary.X), []ExprContext{hoistLambdaContext}), sink)

	if temp == "" {
		return v.refLoweredBoxedFallback(arg)
	}

	return "ref " + temp
}

// refLoweredFuncValueWrapper renders a func VALUE reference to a function with Phase-A ref-lowered
// parameters as a lambda that re-derives the ref at each lowered position, and reports whether it
// wrapped. A function with no lowered parameter — every function, in a `-stdlib` conversion, that the
// X5-func-value veto did its job on — is left exactly as it was spelled.
//
// The wrapper's parameters are the GO-facing shapes the func value's delegate type is rendered from
// (a pointer parameter as its `ж<T>` box), and each lowered position hands the callee
// `ref <p>.DerefOrNull()` — the same nil-DEFERRING accessor the call-site rows use, so a nil argument
// still faults at Go's own point rather than at the wrapper's entry.
//
// Declined for a VARIADIC signature: its emitted form is a `params` array, which converts to no
// delegate at all (the same reason variadicFuncLitCallee exists), so a wrapper there would trade one
// diagnostic for another. Declined too when the callee is generic, whose type arguments the caller's
// own instantiation append owns.
func (v *Visitor) refLoweredFuncValueWrapper(funcObj *types.Func, renderedName string) (string, bool) {
	sig, ok := funcObj.Type().(*types.Signature)

	if !ok || sig.Variadic() || sig.Recv() != nil || sig.TypeParams().Len() > 0 {
		return "", false
	}

	params := sig.Params()

	if params.Len() == 0 {
		return "", false
	}

	anyLowered := false

	for i := range params.Len() {
		if v.paramIsRefLowered(params.At(i)) {
			anyLowered = true
			break
		}
	}

	if !anyLowered {
		return "", false
	}

	declarations := make([]string, params.Len())
	arguments := make([]string, params.Len())

	for i := range params.Len() {
		param := params.At(i)
		name := fmt.Sprintf("%s%d", TempVarMarker, i)

		declarations[i] = fmt.Sprintf("%s %s", v.getCSharpTypeName(param.Type()), name)

		if v.paramIsRefLowered(param) {
			arguments[i] = fmt.Sprintf("ref %s.%s", name, NilDeferringDerefAccessor)
		} else {
			arguments[i] = name
		}
	}

	return fmt.Sprintf("(%s) => %s(%s)", strings.Join(declarations, ", "), renderedName, strings.Join(arguments, ", ")), true
}
