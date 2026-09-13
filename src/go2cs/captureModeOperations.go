// captureModeOperations.go - Gbtc
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

	"golang.org/x/tools/go/packages"
)

// packageCaptureModeMethods holds the package's pointer-receiver methods that take the
// address of a receiver field (`&recv.field`). Such a method needs the real receiver
// box (it emits `<capturedName>.of(Type.ᏑField)`), which only exists when the method is
// invoked through the ж (pointer) overload. So a value var on which one of these methods
// is called must be heap-boxed and the call routed through the ж overload. Populated by a
// synchronous pre-pass before escape analysis; read-only afterward. Keyed by *types.Func
// (interned per method across files). Includes generic receivers, which use the direct-ж
// emission (see packageDirectBoxReceiverMethods) — both forms still need the value var
// boxed and the call routed through the ж overload.
var packageCaptureModeMethods map[*types.Func]bool

// packageCaptureModeBoxIdents holds the value vars on which a capture-mode pointer-receiver method
// is called (`var frontier orderEventList; frontier.Push(…)`, orderEventList a NAMED SLICE with
// `*orderEventList` Push/Pop). Such a var must be heap-boxed so the call binds the ж overload — but
// identHasHeapBox otherwise REFUSES a box for an inherently-heap-allocated type (a named slice/map/
// chan is already a reference), so this records the capture-mode reason to force the box for exactly
// those types (a struct like `atomic.Int32` is already boxed via the not-inherently-heap arm).
// Written during escape analysis (which already computes the predicate), read in identHasHeapBox.
// Keyed by the var's types.Object (package-unique); the escape pass fully precedes the visit pass.
var packageCaptureModeBoxIdents map[types.Object]bool

// packageDirectBoxReceiverMethods holds the field-address capture-mode methods that are
// emitted with the box AS the receiver directly (`this ж<T> Ꮡx` + `ref var x = ref Ꮡx.Value;`),
// where `&x.field` references the parameter box (`Ꮡx.of(Type.ᏑField)`). This replaces the
// static-ThreadLocal capture for ALL such methods (generic and non-generic): the ThreadLocal
// is a shared static reassigned per call and races across threads for distinct receivers —
// broken for concurrent types like sync/atomic — whereas the box parameter has no shared state
// (and avoids a per-call ThreadLocal allocation). For generics it also puts T in scope.
var packageDirectBoxReceiverMethods map[*types.Func]bool

// collectCaptureModeMethods records every non-generic pointer-receiver method whose body
// takes the address of a receiver field — across the package AND its (transitive) imports,
// since a calling package needs to know that an imported method (e.g. sync/atomic.Int32.Store)
// is capture-mode. `LoadAllSyntax` makes dependency ASTs + type info available; the *types.Func
// objects are interned, so call-site lookups match.
func collectCaptureModeMethods(pkg *packages.Package) {
	// The loaded package graph is what importedGlobalIsBoxed (globalAddressOperations.go) reads an
	// IMPORTED package's syntax from; record the root here, where every driver already hands it in.
	currentLoadedPackage = pkg
	importedAddressedGlobals = map[*types.Package]map[types.Object]bool{}
	visited := map[*packages.Package]bool{}
	captureModeCandidates = nil

	var scan func(p *packages.Package)

	scan = func(p *packages.Package) {
		if p == nil || visited[p] || p.TypesInfo == nil {
			return
		}

		visited[p] = true

		for _, file := range p.Syntax {
			scanFileForCaptureModeMethods(file, p.TypesInfo)
		}

		for _, imported := range p.Imports {
			scan(imported)
		}
	}

	scan(pkg)

	// Transitive fixpoint: a method that calls a direct-ж method on its own receiver must itself
	// become direct-ж (so it has a receiver box `Ꮡrecv` to route the call through). Repeat until
	// stable, since the callee may only have been marked direct-ж in an earlier pass.
	for changed := true; changed; {
		changed = false

		for _, candidate := range captureModeCandidates {
			origin := candidate.funcObj.Origin()

			if packageDirectBoxReceiverMethods[origin] {
				continue
			}

			if bodyCallsDirectBoxMethodOnReceiver(candidate.body, candidate.recvName, candidate.info) ||
				bodyCallsCaptureModeMethodOnReceiverField(candidate.body, candidate.recvName, candidate.info) {
				packageCaptureModeMethods[origin] = true
				packageDirectBoxReceiverMethods[origin] = true
				changed = true
			}
		}
	}

	selectRefReturnPrimaries(pkg)
}

// selectionCandidate pairs an eligible capture-mode candidate with its resolved receiver object,
// so the selection fixpoint can re-test its return shape without re-resolving the receiver.
type selectionCandidate struct {
	candidate *captureCandidate
	recvObj   types.Object
}

// selectRefReturnPrimaries is B′-S0's arm-(a) selection, run after the capture-mode fixpoint so
// the decision is FINAL before anything downstream reads the maps (the ref-lowering pass's X3
// flavor read, the local-reversion census, the signature form — a later demotion would corrupt
// pass-4 verdicts, which is why there is no backstop and the predicate must be sound here).
//
// A method is selected when its ONLY box need is Go's fluent `return v`: it is currently
// direct-ж, `bodyReturnsReceiver` fires, none of the other nine escape triggers fires, EVERY
// return in the body (nested func literals excluded — their returns are their own) is exactly the
// bare receiver ident (an early `return nil` or a `return v, err` needs the ж form), the receiver
// base has struct storage (XM-3), and the method is not a linkname participant (XM-4, probed with
// the same pure scan the ref-lowering pass uses). Selection then runs its own DOWNWARD fixpoint:
// a member calling any still-direct-ж method ON ITS RECEIVER is demoted back (a ref receiver
// cannot bind a ж twin — the compile-probe matrix's CS1929 row), repeated until stable.
//
// S1 hardening, stated not silently skipped: the hand-own veto (XM-1) is NOT probed here — the
// destination-path probe the driver uses is not in reach at capture time, and the S0 target
// packages carry zero markers (verified 2026-09-02). Before any wider rollout this veto moves
// into the selection.
func selectRefReturnPrimaries(pkg *packages.Package) {
	if !dualRecvEnabled {
		return
	}

	handles := censusLinknameHandles(pkg.Syntax)
	selected := map[*types.Func]*captureCandidate{}

	// The eligible candidates whose every gate but the RETURN-shape test passes; the return test
	// is deferred to the selection fixpoint because under S1 it depends on which other methods are
	// selected. Captured with its resolved receiver object so the fixpoint needs no re-resolution.
	var returnCandidates []selectionCandidate

	for _, candidate := range captureModeCandidates {
		origin := candidate.funcObj.Origin()

		if !packageDirectBoxReceiverMethods[origin] || candidate.funcObj.Pkg() != pkg.Types {
			continue
		}

		signature, hasSignature := candidate.funcObj.Type().(*types.Signature)

		if !hasSignature || signature.Recv() == nil {
			continue
		}

		recvType := types.Unalias(signature.Recv().Type())
		pointer, isPointer := recvType.(*types.Pointer)

		if !isPointer {
			continue
		}

		if _, isStruct := pointer.Elem().Underlying().(*types.Struct); !isStruct {
			continue // XM-3: no struct storage to ref
		}

		if handles.Contains(candidate.funcObj.Name()) {
			continue // XM-4
		}

		recvObj := receiverObjectFor(candidate)

		if recvObj == nil {
			continue
		}

		// The method must RETURN its receiver to be arm-(a) eligible at all. The bare form
		// (`return v`) qualifies always; under S1 a forwarding return (`return v.M(…)`) also puts
		// the method in the running — whether M is a selected primary is decided in the fixpoint,
		// but the method has to be a candidate first or the cascade never reaches it (this was the
		// gap: carryPropagate returns `v.carryPropagateGeneric()`, never the bare ident, so it was
		// filtered out here and the chain could not climb).
		if !bodyReturnsReceiver(candidate.body, candidate.recvName) &&
			!(dualRecvParamsEnabled && bodyReturnsReceiverThroughCall(candidate.body, candidate.recvName, candidate.info)) {
			continue // the box need is something else; the nine-trigger check below explains it
		}

		if bodyTakesReceiverFieldAddress(candidate.body, candidate.recvName, recvObj, candidate.info) ||
			bodyTakesImplicitReceiverFieldAddress(candidate.body, candidate.recvName, recvObj, candidate.info) ||
			bodyReassignsReceiver(candidate.body, candidate.recvName, signature.Recv(), candidate.info) ||
			bodyUsesReceiverAsPointerValue(candidate.body, candidate.recvName, candidate.info) ||
			bodyCapturesReceiverInClosure(candidate.body, candidate.recvName, signature.Recv(), candidate.info) ||
			bodyHasPointerMethodValueOnReceiver(candidate.body, candidate.recvName, candidate.info) ||
			bodyCapturesReceiverInValueMethodValue(candidate.body, candidate.recvName, candidate.info) ||
			bodyHasGoStmtLambdaCapturingReceiver(candidate.body, candidate.recvName, signature.Recv(), candidate.info) ||
			bodyPassesReceiverAsPointerArg(candidate.body, candidate.recvName, candidate.info) ||
			bodyWrappedInDeferContext(candidate.body, candidate.recvName, candidate.info) {
			continue
		}

		if bodyHasOwnDefer(candidate.body) {
			// A deferring body emits inside a GoFrame try, and C# forbids `ref` returns inside
			// try — the ж form stays. (No S0-package method defers; stated for the wider world.)
			continue
		}

		// Candidate for selection: every gate above passes; only the RETURN-shape test remains,
		// and under S1 that test depends on which OTHER methods are selected — so it is re-run in
		// the fixpoint below, not here. At S0 (no params flag) the fixpoint runs once and admits
		// only bare-receiver returns, reproducing the S0 floor exactly.
		returnCandidates = append(returnCandidates, selectionCandidate{candidate: candidate, recvObj: recvObj})
	}

	// The SELECTION FIXPOINT (B′ S1's iterated form, ruled 2026-09-03). Two coupled movements,
	// re-run until neither changes — monotone (selection only GROWS, direct-ж only SHRINKS) and
	// bounded (finite method set), so it converges; the pass counter guards against a
	// non-monotone bug rather than a real non-termination.
	//
	// (1) A return candidate whose returns are all bare receiver, OR (S1) forward the receiver
	//     through an ALREADY-SELECTED primary, is selected.
	// (2) A selected method that would still force the box — it calls a direct-ж method on its
	//     receiver that is NOT itself selected — is demoted, AND its direct-ж status is lifted so
	//     its own callers can climb. This is the cascade: carryPropagateGeneric (leaf) selects
	//     first, which lets carryPropagate's forwarding return qualify next pass, which lifts
	//     carryPropagate's direct-ж, which un-vetoes feMul.
	isSelectedFixpointGate := func(m *types.Func) bool { return selected[m.Origin()] != nil }
	s1 := dualRecvParamsEnabled

	// A method demoted in step (2) is PERMANENTLY out for this package — it genuinely forces the
	// box (it calls a direct-ж method on its receiver that no climb will ever select, e.g. a
	// field-address method). Without this, step (1) would re-admit it next pass and (2) re-demote
	// it: an oscillation, not a fixpoint. Recording the demotion is what makes the loop monotone.
	demoted := map[*types.Func]bool{}

	for pass, changed := 0, true; changed; pass++ {
		changed = false

		if pass > len(returnCandidates)+8 {
			// The fixpoint must converge within (candidates + slack) passes; exceeding it is a
			// monotonicity bug, not a slow chain. Fail loud rather than spin.
			showWarning("selectRefReturnPrimaries fixpoint did not converge in %d passes (pkg %s) — monotonicity violated", pass, pkg.PkgPath)
			break
		}

		// (1) admit newly-qualifying return candidates (never a permanently-demoted one).
		for _, rc := range returnCandidates {
			origin := rc.candidate.funcObj.Origin()

			if selected[origin] != nil || demoted[origin] {
				continue
			}

			gate := isSelectedFixpointGate

			if !s1 {
				gate = nil // S0: bare-receiver returns only
			}

			if allReturnsAreBareReceiver(rc.candidate.body, rc.recvObj, rc.candidate.info, gate) {
				selected[origin] = rc.candidate
				changed = true
			}
		}

		// (2) demote a selected method still forcing the box, permanently, and lift its direct-ж
		//     so callers can climb next pass. "Still forcing the box" is measured against the
		//     CURRENT selection: a call to a method that is neither selected nor will be (it is
		//     itself demoted or box-forcing) keeps the box.
		for origin, candidate := range selected {
			if bodyCallsMethodOnReceiverWhere(candidate.body, candidate.recvName, candidate.info, func(callee *types.Func) bool {
				calleeOrigin := callee.Origin()
				return packageDirectBoxReceiverMethods[calleeOrigin] && selected[calleeOrigin] == nil
			}) {
				delete(selected, origin)
				demoted[origin] = true
				changed = true
			}
		}

		// After selection grows, a method that was direct-ж ONLY because it called a
		// now-selected method loses that reason — recompute its direct-ж against the current
		// selection so the X3 relaxation (which reads packageDirectBoxReceiverMethods) sees the
		// climbed world. Selected methods are never direct-ж (arm (a) has no box); a candidate
		// that is direct-ж for an INDEPENDENT reason (field address, real box-forcing call) keeps
		// it. S1 only — at S0 the selection is flat and this is a no-op.
		if s1 {
			for origin := range selected {
				if packageDirectBoxReceiverMethods[origin] {
					delete(packageDirectBoxReceiverMethods, origin)
					delete(packageCaptureModeMethods, origin)
					changed = true
				}
			}
		}
	}

	for origin := range selected {
		delete(packageDirectBoxReceiverMethods, origin)
		delete(packageCaptureModeMethods, origin)
		packageRefReturnPrimaryMethods[origin] = true
	}
}

// receiverObjectFor resolves the candidate's receiver *types.Var through its body's own uses —
// the candidate does not retain the declaration, and any use of the receiver name resolving to a
// *types.Var whose type is the receiver type is the receiver (parameters cannot shadow it at
// method scope; a shadowing LOCAL would resolve to a different object, which is exactly why the
// object — not the name — is what the bare-return check compares).
func receiverObjectFor(candidate *captureCandidate) types.Object {
	signature, _ := candidate.funcObj.Type().(*types.Signature)

	if signature == nil || signature.Recv() == nil {
		return nil
	}

	var found types.Object

	ast.Inspect(candidate.body, func(n ast.Node) bool {
		if found != nil {
			return false
		}

		if ident, ok := n.(*ast.Ident); ok && ident.Name == candidate.recvName {
			if obj := candidate.info.Uses[ident]; obj != nil {
				if v, isVar := obj.(*types.Var); isVar && types.Identical(v.Type(), signature.Recv().Type()) {
					found = obj
					return false
				}
			}
		}

		return true
	})

	return found
}

// allReturnsAreBareReceiver reports whether EVERY return statement in the body (outside nested
// func literals) is a ref-return-primary-compatible receiver return — the R3 arm-(a) precondition.
//
// The bare form (`return v`) always qualifies. Under B′ S1 (isSelected non-nil), a
// RECEIVER-FORWARDING return also qualifies: `return v.M(…)` where M is a receiver-method already
// SELECTED as a ref-return primary — Go returns the receiver pointer THROUGH the chained callee,
// and once M is a `ref T`-returning primary the caller can `return ref v.M(…)` because the callee
// hands back a ref to the same receiver. This is the cascade the iterated fixpoint climbs:
// carryPropagateGeneric (a leaf primary) makes carryPropagate's `return v.carryPropagateGeneric()`
// forwardable, which makes carryPropagate a primary, which un-vetoes feMul. isSelected answers
// "is M a ref-return primary in the world this pass is building"; a nil isSelected (S0) admits only
// the bare form, keeping the S0 floor exactly.
//
// Any OTHER return — `return nil`, `return v, err`, `return v.f` (a field, not a receiver method),
// `return other.M()` (a call whose receiver is not v) — needs a ж the primary does not have.
func allReturnsAreBareReceiver(body *ast.BlockStmt, recvObj types.Object, info *types.Info, isSelected func(*types.Func) bool) bool {
	ok := true

	ast.Inspect(body, func(n ast.Node) bool {
		if !ok {
			return false
		}

		if _, isLit := n.(*ast.FuncLit); isLit {
			return false
		}

		ret, isReturn := n.(*ast.ReturnStmt)

		if !isReturn {
			return true
		}

		if len(ret.Results) != 1 {
			ok = false
			return false
		}

		// The bare receiver ident — always the arm-(a) shape.
		if ident, isIdent := ret.Results[0].(*ast.Ident); isIdent && info.Uses[ident] == recvObj {
			return true
		}

		// S1 only: `return v.M(…)` where M is a SELECTED ref-return primary receiver-method on v.
		if isSelected != nil && returnForwardsReceiverThroughPrimary(ret.Results[0], recvObj, info, isSelected) {
			return true
		}

		ok = false
		return false
	})

	return ok
}

// returnForwardsReceiverThroughPrimary reports whether result is `v.M(…)` — a call on the bare
// receiver ident to a method M that isSelected marks a ref-return primary. That is a receiver
// pointer forwarded through a ref-returning callee, which a ref-return primary can `return ref` of.
func returnForwardsReceiverThroughPrimary(result ast.Expr, recvObj types.Object, info *types.Info, isSelected func(*types.Func) bool) bool {
	call, isCall := result.(*ast.CallExpr)

	if !isCall {
		return false
	}

	selector, isSelector := call.Fun.(*ast.SelectorExpr)

	if !isSelector {
		return false
	}

	base, isIdent := selector.X.(*ast.Ident)

	if !isIdent || info.Uses[base] != recvObj {
		return false // the call's receiver is not the bare receiver
	}

	method, isFunc := info.Uses[selector.Sel].(*types.Func)

	return isFunc && isSelected(method)
}

// bodyCallsMethodOnReceiverWhere reports whether the body contains a method CALL whose receiver
// expression is the bare receiver ident and whose callee satisfies the predicate — the downward
// fixpoint's probe, shaped after bodyCallsDirectBoxMethodOnReceiver but parameterized so it can
// ask about the world the selection is constructing rather than the global map.
func bodyCallsMethodOnReceiverWhere(body *ast.BlockStmt, recvName string, info *types.Info, matches func(*types.Func) bool) bool {
	found := false

	ast.Inspect(body, func(n ast.Node) bool {
		if found {
			return false
		}

		call, isCall := n.(*ast.CallExpr)

		if !isCall {
			return true
		}

		selector, isSelector := call.Fun.(*ast.SelectorExpr)

		if !isSelector {
			return true
		}

		base, isIdent := selector.X.(*ast.Ident)

		if !isIdent || base.Name != recvName {
			return true
		}

		if callee, isFunc := info.Uses[selector.Sel].(*types.Func); isFunc && matches(callee) {
			found = true
			return false
		}

		return true
	})

	return found
}

// scanFileForCaptureModeMethods marks the file's capture-mode methods in the shared set.
func scanFileForCaptureModeMethods(file *ast.File, info *types.Info) {
	ast.Inspect(file, func(node ast.Node) bool {
		funcDecl, ok := node.(*ast.FuncDecl)

		if !ok || funcDecl.Recv == nil || funcDecl.Body == nil || len(funcDecl.Recv.List) == 0 {
			return true
		}

		recvField := funcDecl.Recv.List[0]

		if len(recvField.Names) == 0 {
			return true
		}

		recvName := recvField.Names[0].Name

		funcObj, _ := info.Defs[funcDecl.Name].(*types.Func)

		if funcObj == nil {
			return true
		}

		signature, _ := funcObj.Type().(*types.Signature)

		if signature == nil || signature.Recv() == nil {
			return true
		}

		pointer, ok := signature.Recv().Type().(*types.Pointer)

		if !ok {
			return true
		}

		if _, ok := pointer.Elem().(*types.Named); !ok {
			return true
		}

		// Record every pointer-receiver candidate for the transitive fixpoint below (a method
		// that calls a direct-ж method on its receiver must itself become direct-ж).
		captureModeCandidates = append(captureModeCandidates, &captureCandidate{
			funcObj:  funcObj,
			body:     funcDecl.Body,
			recvName: recvName,
			info:     info,
		})

		recvObj := recvObjectOf(funcDecl, info)

		if bodyTakesReceiverFieldAddress(funcDecl.Body, recvName, recvObj, info) || bodyReturnsReceiver(funcDecl.Body, recvName) || bodyReassignsReceiver(funcDecl.Body, recvName, signature.Recv(), info) || bodyUsesReceiverAsPointerValue(funcDecl.Body, recvName, info) || bodyCapturesReceiverInClosure(funcDecl.Body, recvName, signature.Recv(), info) || bodyHasPointerMethodValueOnReceiver(funcDecl.Body, recvName, info) || bodyCapturesReceiverInValueMethodValue(funcDecl.Body, recvName, info) || bodyHasGoStmtLambdaCapturingReceiver(funcDecl.Body, recvName, signature.Recv(), info) || bodyPassesReceiverAsPointerArg(funcDecl.Body, recvName, info) || bodyWrappedInDeferContext(funcDecl.Body, recvName, info) {
			// Key by the generic origin so instantiated call sites (Set[int]) match.
			origin := funcObj.Origin()
			packageCaptureModeMethods[origin] = true

			// All field-address capture methods use the direct-ж receiver — the box is passed
			// AS the receiver parameter (`this ж<T> Ꮡx`), not stashed in a static ThreadLocal.
			// The ThreadLocal capture is a shared static reassigned per call, which races across
			// threads for distinct receivers (broken for concurrent types like sync/atomic); the
			// direct-ж form has no shared state and is also alloc-free. Applies to generic AND
			// non-generic receivers.
			packageDirectBoxReceiverMethods[origin] = true
		}

		return true
	})
}

// captureCandidate is a pointer-receiver method recorded for the transitive direct-ж fixpoint.
type captureCandidate struct {
	funcObj  *types.Func
	body     *ast.BlockStmt
	recvName string
	info     *types.Info
}

// dualRecvEnabled mirrors options.dualRecv for the capture-mode pass, which has no options in
// reach across its four drivers. Set once in main; read only by selectRefReturnPrimaries.
var dualRecvEnabled bool

// dualRecvParamsEnabled mirrors options.dualRecvParams (B′ S1). Read by the ref-lowering analysis
// (the §4.3 X3 relaxation) and the primary emission (lowered parameters). Separate from
// dualRecvEnabled so `-dual-recv` alone re-emits the S0 floor after S1 lands.
var dualRecvParamsEnabled bool

// packageRefReturnPrimaryMethods is B′-S0's arm-(a) selection (the 2026-09-02 R3 ruling): fluent
// pointer-receiver methods whose ONLY box need is `return v`, emitted as `[GoRecv] this ref T`
// primaries returning `ref T` (the receiver itself) with RecvGenerator's twin returning its own
// box. Populated by selectRefReturnPrimaries AFTER the capture-mode fixpoint — members are REMOVED
// from the two capture maps there, so every later consumer (signature form, param heap-boxing,
// the ref-lowering X3 flavor read) sees one consistent world. Reset with its siblings.
var packageRefReturnPrimaryMethods map[*types.Func]bool

// captureModeCandidates holds every pointer-receiver method seen while scanning the package and
// its imports, used by the transitive fixpoint in collectCaptureModeMethods.
var captureModeCandidates []*captureCandidate

// bodyWrappedInDeferContext reports whether a method that defers or recovers at FUNCTION level also
// references its receiver, in which case the method takes the direct-ж receiver (`this ж<T> Ꮡx`)
// rather than `this ref T`. Defer/recover inside a nested function literal belongs to that literal
// (mirrors funcBodyDeferRecover) and does not count.
//
// The rule was FORCED when such a body was emitted inside the `func((defer, recover) => { … })`
// execution-context lambda: a `ref T` receiver cannot be referenced from inside a lambda (CS1628 ×3,
// fmt ss.Token's `defer func(){ recover() }()` + `s.buf`). The GoFrame emission puts the body inline
// in the method, so that constraint is gone and `this ref T` would now compile. The rule is KEPT
// anyway, deliberately: the direct-ж form is also the alloc-free, race-free one (see the ThreadLocal
// note above), and switching receiver shapes corpus-wide is a change of its own with its own blast
// radius, not a side effect of the frame. Recorded as an open simplification rather than taken here.
func bodyWrappedInDeferContext(body *ast.BlockStmt, recvName string, info *types.Info) bool {
	hasDeferRecover := false

	ast.Inspect(body, func(node ast.Node) bool {
		switch n := node.(type) {
		case *ast.FuncLit:
			return false
		case *ast.DeferStmt:
			hasDeferRecover = true
			return false
		case *ast.CallExpr:
			// Only the universe `recover` wraps the body; a declaration shadowing that name is
			// an ordinary call (see identIsUniverseBuiltin).
			if ident, ok := n.Fun.(*ast.Ident); ok && ident.Name == "recover" {
				if _, isBuiltin := info.ObjectOf(ident).(*types.Builtin); isBuiltin {
					hasDeferRecover = true
					return false
				}
			}
		}

		return !hasDeferRecover
	})

	if !hasDeferRecover {
		return false
	}

	referencesReceiver := false

	ast.Inspect(body, func(node ast.Node) bool {
		if ident, ok := node.(*ast.Ident); ok && ident.Name == recvName {
			referencesReceiver = true
		}

		return !referencesReceiver
	})

	return referencesReceiver
}

// bodyCallsDirectBoxMethodOnReceiver reports whether the body calls a direct-ж method on the
// receiver itself (`recvName.someDirectBoxMethod(...)`). The caller must then also be direct-ж so
// it has a receiver box `Ꮡrecv` to route the call through (the callee's ж overload needs it).
func bodyCallsDirectBoxMethodOnReceiver(body *ast.BlockStmt, recvName string, info *types.Info) bool {
	found := false

	ast.Inspect(body, func(node ast.Node) bool {
		callExpr, ok := node.(*ast.CallExpr)

		if !ok {
			return true
		}

		selectorExpr, ok := callExpr.Fun.(*ast.SelectorExpr)

		if !ok {
			return true
		}

		recvIdent, ok := selectorExpr.X.(*ast.Ident)

		if !ok || recvIdent.Name != recvName {
			return true
		}

		if funcObj, ok := info.ObjectOf(selectorExpr.Sel).(*types.Func); ok && funcObj != nil {
			if packageDirectBoxReceiverMethods[funcObj.Origin()] {
				found = true
				return false
			}
		}

		return true
	})

	return found
}

// bodyCallsCaptureModeMethodOnReceiverField reports whether the body calls a direct-ж
// (capture-mode) method on a VALUE field-chain of the receiver (`recvName.f1.…fn.someDirectBoxMethod(...)`,
// n>=1) — e.g. a struct embedding sync/atomic's Uint8 as a value field `u` and doing `b.u.Load()`, or
// a deeper chain like `p.scav.index.free(…)` (root `p`, value fields `scav`→`index`). The callee's ж
// overload needs a `ж<FieldType>`; the only way to produce one that aliases the real field is
// `Ꮡrecv.of(RecvType.ᏑF1).of(…ᏑFn)`, which requires the caller to itself be direct-ж so its receiver
// box `Ꮡrecv` is in scope. So the caller must be marked direct-ж too (convSelectorExpr then emits the
// field-address box form via exprIsValueFieldOfDerefdPointerRoot). The chain must be all VALUE fields —
// a pointer field mid-chain is already a box and roots elsewhere (exprIsValueFieldOfPointer's territory),
// so it needs no caller box and must not trigger promotion.
func bodyCallsCaptureModeMethodOnReceiverField(body *ast.BlockStmt, recvName string, info *types.Info) bool {
	found := false

	ast.Inspect(body, func(node ast.Node) bool {
		callExpr, ok := node.(*ast.CallExpr)

		if !ok {
			return true
		}

		selectorExpr, ok := callExpr.Fun.(*ast.SelectorExpr)

		if !ok {
			return true
		}

		// The call target must be `recvName.f1.…fn.method` — a value field-chain rooted directly at
		// the receiver. (A bare `recvName.method` is the receiver-direct case handled separately.)
		if !selectorRootsAtReceiverValueFieldChain(selectorExpr.X, recvName, info) {
			return true
		}

		if funcObj, ok := info.ObjectOf(selectorExpr.Sel).(*types.Func); ok && funcObj != nil {
			if packageDirectBoxReceiverMethods[funcObj.Origin()] {
				found = true
				return false
			}
		}

		return true
	})

	return found
}

// selectorRootsAtReceiverValueFieldChain reports whether expr is a VALUE struct-field selector chain
// `recvName.f1.…fn` (n>=1) that roots at the receiver ident recvName, with every hop a VALUE
// (non-pointer) field. This is the capture-mode pre-pass complement of convSelectorExpr's
// exprIsValueFieldOfDerefdPointerRoot: a direct-ж method called on such a chain needs the real nested
// field box `Ꮡrecv.of(T.ᏑF1).of(…ᏑFn)`, which only exists when the enclosing method is itself direct-ж.
// A pointer field anywhere in the chain is already a box (and roots the call elsewhere), so it stops
// the walk — that case must not promote the enclosing method.
func selectorRootsAtReceiverValueFieldChain(expr ast.Expr, recvName string, info *types.Info) bool {
	sel, ok := expr.(*ast.SelectorExpr)

	if !ok {
		return false
	}

	for {
		// Each hop must be a value field selection (not a method/package ref, not a pointer field).
		if selection, ok := info.Selections[sel]; !ok || selection.Kind() != types.FieldVal {
			return false
		}

		if _, isPtr := info.TypeOf(sel).(*types.Pointer); isPtr {
			return false
		}

		switch base := sel.X.(type) {
		case *ast.SelectorExpr:
			sel = base
		case *ast.Ident:
			return base.Name == recvName
		default:
			return false
		}
	}
}

// bodyReassignsReceiver reports whether the body REPOINTS the receiver variable at a different
// value (`s = s.next`, a linked-list walk). Go's pointer receiver is an ordinary local, so this is
// legal and rebinds only the callee's copy — but the converter deref-aliases a pointer receiver to a
// value var (`ref var s = ref Ꮡs.Value`), and a value alias cannot be repointed: the assignment
// emits `ж<T>` into a `ref T` (CS0029).
//
// The direct-ж receiver is what makes it expressible — with the box `Ꮡs` as the parameter,
// visitAssignStmt's exprIsCurrentDirectBoxReceiver arm repoints the box and re-aliases the value
// (`Ꮡs = s.next; s = ref Ꮡs.DerefOrNull();`, container/ring's `Move`). That arm already exists; it
// was simply unreachable for a method no OTHER trigger marked. `ring.Move` reaches it only because
// it also returns its receiver, and every other production instance is likewise carried by a
// neighbouring trigger — which is why the gap surfaced first in a test file
// (database/sql's `fakedb_test.go`, `func (s *fakeStmt) QueryContext` walking `s = s.next`).
//
// Matched by OBJECT identity, not by name: an inner `:=` that shadows the receiver's name declares a
// different variable, and assigning to THAT is not a repoint of the receiver.
func bodyReassignsReceiver(body *ast.BlockStmt, recvName string, recv *types.Var, info *types.Info) bool {
	if recv == nil || info == nil {
		return false
	}

	found := false

	ast.Inspect(body, func(node ast.Node) bool {
		assignStmt, ok := node.(*ast.AssignStmt)

		if !ok || assignStmt.Tok == token.DEFINE {
			return true
		}

		for _, lhs := range assignStmt.Lhs {
			ident, ok := lhs.(*ast.Ident)

			if !ok || ident.Name != recvName {
				continue
			}

			if info.Uses[ident] == types.Object(recv) {
				found = true
				return false
			}
		}

		return true
	})

	return found
}

// bodyReturnsReceiver reports whether the body returns the receiver itself (`return recvName`).
// Such a method also needs the real receiver box (it returns the pointer), which the direct-ж
// receiver supplies as `Ꮡrecv` — see visitReturnStmt.
func bodyReturnsReceiver(body *ast.BlockStmt, recvName string) bool {
	found := false

	ast.Inspect(body, func(node ast.Node) bool {
		returnStmt, ok := node.(*ast.ReturnStmt)

		if !ok {
			return true
		}

		for _, result := range returnStmt.Results {
			if ident, ok := result.(*ast.Ident); ok && ident.Name == recvName {
				found = true
				return false
			}
		}

		return true
	})

	return found
}

// bodyHasPointerMethodValueOnReceiver reports whether the body uses a method VALUE (a method
// referenced without calling it) whose target has a POINTER receiver and whose receiver expression
// is the method's own receiver or a VALUE field-chain rooted at it — `s.nonDefaultOnce.Do(s.register)`,
// `registerMetric(…, s.nonDefault.Load)` (internal/godebug). Go binds the receiver ADDRESS at
// method-value creation, so convSelectorExpr emits a box-bound method group (`Ꮡs.register`,
// `Ꮡs.of(Setting.ᏑnonDefault).Load`), which requires the enclosing method to be direct-ж so the
// receiver box `Ꮡrecv` is in scope.
func bodyHasPointerMethodValueOnReceiver(body *ast.BlockStmt, recvName string, info *types.Info) bool {
	// A selector that is a call's Fun is a method CALL, not a method value.
	calledFuns := map[ast.Expr]bool{}

	ast.Inspect(body, func(node ast.Node) bool {
		if callExpr, ok := node.(*ast.CallExpr); ok {
			calledFuns[callExpr.Fun] = true
		}

		return true
	})

	found := false

	ast.Inspect(body, func(node ast.Node) bool {
		sel, ok := node.(*ast.SelectorExpr)

		if !ok || calledFuns[sel] {
			return true
		}

		selection, ok := info.Selections[sel]

		if !ok || selection.Kind() != types.MethodVal {
			return true
		}

		sig, ok := selection.Obj().Type().(*types.Signature)

		if !ok || sig.Recv() == nil {
			return true
		}

		if _, isPtr := sig.Recv().Type().(*types.Pointer); !isPtr {
			return true
		}

		if ident, ok := sel.X.(*ast.Ident); ok && ident.Name == recvName {
			found = true
			return false
		}

		if selectorRootsAtReceiverValueFieldChain(sel.X, recvName, info) {
			found = true
			return false
		}

		return true
	})

	return found
}

// bodyCapturesReceiverInValueMethodValue reports whether the body uses a VALUE-receiver method
// VALUE (a method referenced without calling it) whose receiver expression is the method's own
// receiver, or a VALUE field-chain rooted at it — `kdf.hash.New`, `kdf.New` (crypto/internal/hpke's
// `hkdfKDF`, whose `hash` field is a `crypto.Hash` and `New` a value-receiver method). Such a method
// value has no C# delegate — its emitted form is an extension over a value receiver (CS1113) — so
// convSelectorExpr synthesizes a wrapping lambda `() => kdf.hash.New()`, which CAPTURES the receiver.
// A `ref T` receiver captured by a lambda is CS1628. Promoting the method to direct-ж gives it a
// receiver box `Ꮡkdf`, which the synthesized lambda then references through `Ꮡkdf.Value` (a
// capturable reference). The POINTER-receiver method-value case is handled by
// bodyHasPointerMethodValueOnReceiver (it emits a box-bound method group, not a lambda); an INTERFACE
// receiver delegate-binds directly and is excluded (both keep the plain, non-capturing emission).
func bodyCapturesReceiverInValueMethodValue(body *ast.BlockStmt, recvName string, info *types.Info) bool {
	// A selector that is a call's Fun is a method CALL, not a method value.
	calledFuns := map[ast.Expr]bool{}

	ast.Inspect(body, func(node ast.Node) bool {
		if callExpr, ok := node.(*ast.CallExpr); ok {
			calledFuns[callExpr.Fun] = true
		}

		return true
	})

	found := false

	ast.Inspect(body, func(node ast.Node) bool {
		sel, ok := node.(*ast.SelectorExpr)

		if !ok || calledFuns[sel] {
			return true
		}

		selection, ok := info.Selections[sel]

		if !ok || selection.Kind() != types.MethodVal {
			return true
		}

		sig, ok := selection.Obj().Type().(*types.Signature)

		if !ok || sig.Recv() == nil {
			return true
		}

		// A POINTER-receiver method value renders as a box-bound method group (handled separately),
		// and an INTERFACE receiver delegate-binds directly — neither synthesizes a capturing lambda.
		// Only a concrete VALUE receiver produces the `() => recv.….method()` wrapper.
		if _, isPtr := sig.Recv().Type().(*types.Pointer); isPtr {
			return true
		}

		if types.IsInterface(sig.Recv().Type()) {
			return true
		}

		if ident, ok := sel.X.(*ast.Ident); ok && ident.Name == recvName {
			found = true
			return false
		}

		if selectorRootsAtReceiverValueFieldChain(sel.X, recvName, info) {
			found = true
			return false
		}

		return true
	})

	return found
}

// bodyHasGoStmtLambdaCapturingReceiver reports whether the body contains a `go` statement whose
// emission is FORCED into a synthesized lambda that references the method's own receiver —
// `go q.conn.HandshakeContext(ctx)` inside `func (q *QUICConn) Start` (crypto/tls quic.go):
// HandshakeContext returns a value, and goǃ has only void Action overloads, so visitGoStmt
// renders `goǃ(ᴛ1 => q.conn.HandshakeContext(ᴛ1), ctx)` (the x/net/nettest CS0407 form) — a
// lambda capturing the `ref T` receiver (CS1628). The method-group emissions (a void
// matching-arity callee, e.g. os/exec's `go c.watchCtx(resultc)`) bind the receiver chain when
// the delegate is created, outside any lambda, and are excluded. A func-literal callee is
// bodyCapturesReceiverInClosure's case; a `defer` sibling needs no equivalent — any
// function-level defer already promotes via bodyWrappedInDeferContext. Promoting the method
// direct-ж gives the lambda the capturable box `Ꮡq`: the go-stmt capture analysis already
// box-ref-marks the receiver (varIsDerefdPointerParam), so only the missing promotion
// suppressed the box render (the convIdent ref-receiver guard).
func bodyHasGoStmtLambdaCapturingReceiver(body *ast.BlockStmt, recvName string, recv *types.Var, info *types.Info) bool {
	found := false

	ast.Inspect(body, func(node ast.Node) bool {
		if found {
			return false
		}

		goStmt, ok := node.(*ast.GoStmt)

		if !ok {
			return true
		}

		if _, isFuncLit := goStmt.Call.Fun.(*ast.FuncLit); isFuncLit {
			return true
		}

		funType := info.TypeOf(goStmt.Call.Fun)

		if funType == nil {
			return true
		}

		sig, ok := funType.Underlying().(*types.Signature)

		if !ok {
			return true
		}

		hasResults := sig.Results() != nil && sig.Results().Len() > 0
		namedFuncType := false

		if named, ok := types.Unalias(funType).(*types.Named); ok {
			if _, isSig := named.Underlying().(*types.Signature); isSig {
				namedFuncType = true
			}
		}

		// Mirror visitGoStmt's lambda-form decision: a nullary call synthesizes a lambda only
		// for a value-returning or named-func-type callee; a call with arguments does so when
		// the callee returns a value or the arity mismatches (a variadic callee never matches —
		// see getFunctionParamCount).
		paramCount := len(goStmt.Call.Args)
		declaredCount := sig.Params().Len()

		if sig.Variadic() {
			declaredCount = -1
		}

		var lambdaForm bool

		if paramCount == 0 {
			lambdaForm = hasResults || namedFuncType
		} else {
			lambdaForm = hasResults || paramCount != declaredCount
		}

		if !lambdaForm {
			return true
		}

		// Only the CALLEE expression (the receiver chain) renders inside the synthesized
		// lambda; arguments render outside, as goǃ call arguments evaluated at go time.
		ast.Inspect(goStmt.Call.Fun, func(inner ast.Node) bool {
			ident, ok := inner.(*ast.Ident)

			if !ok || ident.Name != recvName {
				return true
			}

			if recv != nil && info.ObjectOf(ident) != recv {
				return true
			}

			found = true
			return false
		})

		return !found
	})

	return found
}

// bodyPassesReceiverAsPointerArg reports whether the body passes the receiver itself as a
// CALL ARGUMENT bound to a POINTER parameter - `trim(a)` inside `func (a *decimal)` where
// `func trim(a *decimal)` (strconv decimal.go; syscall's SID helpers). The callee's emitted
// form takes the box `ж<T>`, so the caller needs its receiver box in scope - promote to
// direct-ж. The assignment/comparison uses are detected separately
// (bodyUsesReceiverAsPointerValue); this covers the argument position.
func bodyPassesReceiverAsPointerArg(body *ast.BlockStmt, recvName string, info *types.Info) bool {
	found := false

	ast.Inspect(body, func(node ast.Node) bool {
		if found {
			return false
		}

		callExpr, ok := node.(*ast.CallExpr)

		if !ok {
			return true
		}

		for _, arg := range callExpr.Args {
			ident, ok := arg.(*ast.Ident)

			if !ok || ident.Name != recvName {
				continue
			}

			// The receiver ident must bind a POINTER argument slot (Go only allows this when
			// the parameter is the same pointer type, but check the signature to exclude a
			// SHADOWED local of another type).
			if argType := info.TypeOf(arg); argType != nil {
				if _, isPtr := argType.(*types.Pointer); isPtr {
					found = true
					return false
				}
			}
		}

		return true
	})

	return found
}

// bodyUsesReceiverAsPointerValue reports whether the body uses the receiver itself as a bare
// pointer value: on the RHS of an assignment (`recvField = recvName`, `x := recvName`), or as an
// operand of a pointer ==/!= comparison (`p != recvName`). Such a method needs the real receiver
// box (the pointer is copied/stored/compared), which the direct-ж receiver supplies as `Ꮡrecv` —
// without it a value-ref receiver (`this ref T recv`) has no pointer to hand out, and `recv`
// (a T value) cannot be assigned to a *T field or compared with a ж<T>. The selector-`X` position
// (`recvName.field`, a value field access) is not a bare ident, so it is naturally excluded.
func bodyUsesReceiverAsPointerValue(body *ast.BlockStmt, recvName string, info *types.Info) bool {
	found := false

	ast.Inspect(body, func(node ast.Node) bool {
		switch n := node.(type) {
		case *ast.AssignStmt:
			for _, rhs := range n.Rhs {
				if ident, ok := rhs.(*ast.Ident); ok && ident.Name == recvName {
					found = true
					return false
				}
			}
		case *ast.BinaryExpr:
			if n.Op == token.EQL || n.Op == token.NEQ {
				if ident, ok := n.X.(*ast.Ident); ok && ident.Name == recvName {
					found = true
					return false
				}

				if ident, ok := n.Y.(*ast.Ident); ok && ident.Name == recvName {
					found = true
					return false
				}
			}
		case *ast.SendStmt:
			// The receiver SENT on a channel whose element is a pointer — `call.Done <- call`
			// inside net/rpc's `func (call *Call) done()`. The channel's element renders as
			// `ж<T>`, so the send value must be the receiver's BOX; a value-ref receiver
			// (`this ref T call`) has only the deref'd value, which cannot bind the `in ж<T>`
			// send parameter (CS1503). Both send forms are SendStmt nodes and route through
			// convSendValueExpr, so this covers the statement (`ch <- recv`) and the `select`
			// send case alike. Gated on the ident's type being a pointer, so a SHADOWED local
			// of another type cannot promote (mirrors bodyPassesReceiverAsPointerArg).
			if ident, ok := n.Value.(*ast.Ident); ok && ident.Name == recvName {
				if valueType := info.TypeOf(n.Value); valueType != nil {
					if _, isPtr := valueType.(*types.Pointer); isPtr {
						found = true
						return false
					}
				}
			}
		case *ast.CompositeLit:
			// The receiver placed whole into a COMPOSITE-LITERAL element whose slot is a Go
			// pointer. Its pointer identity is copied/stored, which needs the receiver's box —
			// available only when the method is direct-ж. Handled for a STRUCT field, and for a
			// SLICE/ARRAY/MAP element/value whose element type is a pointer.
			switch litType := info.TypeOf(n).Underlying().(type) {
			case *types.Struct:
				// `return funcInfo{f, mod}` inside `func (f *_func) funcInfo()` (runtime symtab.go;
				// funcInfo's first field is the embedded pointer *_func). Gated on the FIELD's
				// declared type (the element expression's own type is always *T for a pointer
				// receiver): a receiver placed into an INTERFACE-typed field also typechecks in Go.
				structType := litType

				for i, elt := range n.Elts {
					fieldIdx := i
					val := elt

					if kv, ok := elt.(*ast.KeyValueExpr); ok {
						val = kv.Value
						fieldIdx = -1

						if keyIdent, ok := kv.Key.(*ast.Ident); ok {
							for j := 0; j < structType.NumFields(); j++ {
								if structType.Field(j).Name() == keyIdent.Name {
									fieldIdx = j
									break
								}
							}
						}
					}

					if ident, ok := val.(*ast.Ident); ok && ident.Name == recvName {
						if fieldIdx >= 0 && fieldIdx < structType.NumFields() {
							if _, isPtr := structType.Field(fieldIdx).Type().(*types.Pointer); isPtr {
								found = true
								return false
							}

							// An INTERFACE-typed field also captures the receiver AS the pointer
							// (Go's interface holds the *T): archive/tar's WriteTo builds
							// `struct{ io.Reader }{fr}` — the pointer adapter wraps the box,
							// which only exists when the method is direct-ж (CS1503 ×4). This
							// was previously excluded because the old (broken) lifted-anon-embed
							// emission compiled without it; the embed is a real interface member
							// now, so the conversion genuinely needs the box.
							if fieldIface, isEmpty := isInterface(structType.Field(fieldIdx).Type()); fieldIface && !isEmpty {
								found = true
								return false
							}
						}
					}
				}
			case *types.Slice:
				// `descendents := []*UserTaskSummary{s}` inside `func (s *UserTaskSummary)
				// Descendents()` (internal/trace summary.go): the receiver becomes a POINTER element
				// of the slice, so `s` must render as its box `Ꮡs`, not the value alias (CS0029).
				if _, isPtr := litType.Elem().(*types.Pointer); isPtr && compositeLitHasReceiverElement(n.Elts, recvName) {
					found = true
					return false
				}
			case *types.Array:
				if _, isPtr := litType.Elem().(*types.Pointer); isPtr && compositeLitHasReceiverElement(n.Elts, recvName) {
					found = true
					return false
				}
				// NOTE: a pointer-value/pointer-key MAP literal (`map[K]*T{k: s}`) also stores the
				// receiver's pointer identity, but convCompositeLit's map-element emission does not
				// yet box a pointer VALUE (it renders the value alias, CS0029 — a general map-literal
				// gap independent of the receiver), so promoting here would not help. Deliberately
				// out of scope until the map-element boxing is fixed.
			}
		case *ast.IndexExpr:
			// The receiver used as a POINTER-keyed map INDEX — `t.m[c]` inside `func (c *conn)`
			// (net/http transport.go's idle-conn bookkeeping shape). The key position needs the
			// receiver's pointer identity (`m[Ꮡc]` — see convIndexExpr's pointer-key rendering),
			// which exists only when the method is direct-ж. Gated on the indexed operand being
			// a pointer-KEYED map, so a slice/array index of an integer-ish receiver name can
			// never promote.
			if ident, ok := n.Index.(*ast.Ident); ok && ident.Name == recvName {
				if baseType := info.TypeOf(n.X); baseType != nil {
					if mapType, isMap := baseType.Underlying().(*types.Map); isMap {
						if _, keyIsPtr := mapType.Key().(*types.Pointer); keyIsPtr {
							found = true
							return false
						}
					}
				}
			}
		}

		return true
	})

	return found
}

// compositeLitHasReceiverElement reports whether the receiver ident appears as a bare-ident element
// VALUE of a slice/array composite literal — a plain element (`{s}`) or the value of an indexed
// element (`{2: s}`, whose KeyValueExpr value is the stored element).
func compositeLitHasReceiverElement(elts []ast.Expr, recvName string) bool {
	for _, elt := range elts {
		target := elt

		if kv, ok := elt.(*ast.KeyValueExpr); ok {
			target = kv.Value
		}

		if ident, ok := target.(*ast.Ident); ok && ident.Name == recvName {
			return true
		}
	}

	return false
}

// bodyCapturesReceiverInClosure reports whether the body references the receiver inside a function
// literal (a closure) — e.g. runtime's `func (p *_panic) nextFrame() { … systemstack(func(){ … p.lr
// … }) }`. The receiver is normally emitted as the deref'd ref-local alias `ref var p = ref Ꮡp.Value`,
// but a C# ref-local cannot be captured by a lambda (CS8175); inside the closure the receiver must
// be referenced through its box `Ꮡp` (the convIdent/convUnaryExpr box-ref forms). That box only
// exists when the method is direct-ж — the box passed AS the receiver param (`this ж<T> Ꮡp`). A
// closure parameter that shadows the receiver name has a distinct object, so the `recv` identity
// check excludes it (no false fire).
func bodyCapturesReceiverInClosure(body *ast.BlockStmt, recvName string, recv *types.Var, info *types.Info) bool {
	found := false

	ast.Inspect(body, func(node ast.Node) bool {
		if found {
			return false
		}

		funcLit, ok := node.(*ast.FuncLit)

		if !ok {
			return true
		}

		// Any reference to the receiver from within this closure (or a closure nested inside it —
		// ast.Inspect recurses) means it is captured and must route through the box.
		ast.Inspect(funcLit.Body, func(inner ast.Node) bool {
			ident, ok := inner.(*ast.Ident)

			if !ok || ident.Name != recvName {
				return true
			}

			if recv != nil && info.ObjectOf(ident) != recv {
				return true
			}

			found = true
			return false
		})

		return !found
	})

	return found
}

// bodyTakesReceiverFieldAddress reports whether the body contains `&recvName.field`.
// The DEEP form -- `&recv.f1.f2`, a value-field chain rooted at the receiver -- was unrecognized
// until 2026-08-23, and the consequence was silent: the method was never marked direct-ж, so
// convUnaryExpr's receiver arm had no box to chain from, emission fell through to the
// Ꮡ(value) copy-box, and every write through the resulting pointer was dropped. See
// receiverValueFieldChain for the measured case (dnsmessage's incrementSectionCount, 4 sites).
//
// The walk is TYPE-AWARE rather than purely syntactic, and that matters in the CONSERVATIVE
// direction: a hop through a POINTER field is a different address (the pointee's, reached through
// a box that already exists), so marking a method direct-ж for it would be over-marking -- it
// would change that method's whole receiver form to serve an arm that will not fire.
// recvObjectOf resolves a method's receiver to its types.Object, or nil for an unnamed receiver.
func recvObjectOf(funcDecl *ast.FuncDecl, info *types.Info) types.Object {
	if funcDecl == nil || funcDecl.Recv == nil || len(funcDecl.Recv.List) == 0 || len(funcDecl.Recv.List[0].Names) == 0 {
		return nil
	}

	return info.ObjectOf(funcDecl.Recv.List[0].Names[0])
}

func bodyTakesReceiverFieldAddress(body *ast.BlockStmt, recvName string, recvObj types.Object, info *types.Info) bool {
	found := false

	ast.Inspect(body, func(node ast.Node) bool {
		unaryExpr, ok := node.(*ast.UnaryExpr)

		if !ok || unaryExpr.Op != token.AND {
			return true
		}

		if selectorExpr, ok := unaryExpr.X.(*ast.SelectorExpr); ok {
			base := selectorExpr.X
			depth := 0

			// Walk down VALUE-struct hops to the chain root; anything else stops the walk.
			for {
				inner, ok := base.(*ast.SelectorExpr)

				if !ok {
					break
				}

				selection, ok := info.Selections[inner]

				if !ok || selection.Kind() != types.FieldVal {
					return true
				}

				innerType := info.TypeOf(inner)

				if innerType == nil {
					return true
				}

				if _, isStruct := innerType.Underlying().(*types.Struct); !isStruct {
					return true
				}

				base = inner.X
				depth++
			}

			if ident, ok := base.(*ast.Ident); ok && ident.Name == recvName {
				// A DEEP chain must additionally match the receiver by OBJECT, because a LOCAL
				// shadowing the receiver name reaches a different variable entirely and marking
				// for it is pure over-marking -- it rewrites an unrelated method's whole receiver
				// form to serve an arm that will decline (convUnaryExpr matches by object too).
				// Measured: `t := other; q := &t.inner.n` inside a `func (t *Thing)` churned
				// ShadowLocalOverRecvName's golden with no behavior change at all.
				//
				// The ONE-hop case deliberately keeps the historical name-only match, so this
				// change cannot move any site that was already being marked.
				if depth > 0 && recvObj != nil && info.ObjectOf(ident) != recvObj {
					return true
				}

				found = true
				return false
			}
		}

		return true
	})

	return found
}

// exprIsReceiverValueFieldChain reports whether expr is a VALUE-struct field chain rooted at the
// receiver (`v.f`, `v.f.g`, every hop a value struct) — the same walk bodyTakesReceiverFieldAddress
// uses under its `&`, factored out so the IMPLICIT-address form below can share it. A bare receiver
// ident (`v`, depth 0) is NOT a field chain and returns false; a hop through a POINTER field stops
// the walk (that address is the pointee's, reached through a box that already exists — conservative,
// matching the sibling's own note). Matched by OBJECT beyond one hop so a shadowing local cannot
// spuriously qualify.
func exprIsReceiverValueFieldChain(expr ast.Expr, recvName string, recvObj types.Object, info *types.Info) bool {
	if _, ok := expr.(*ast.SelectorExpr); !ok {
		return false
	}

	base := expr
	depth := 0

	for {
		inner, ok := base.(*ast.SelectorExpr)

		if !ok {
			break
		}

		selection, ok := info.Selections[inner]

		if !ok || selection.Kind() != types.FieldVal {
			return false
		}

		innerType := info.TypeOf(inner)

		if innerType == nil {
			return false
		}

		if _, isStruct := innerType.Underlying().(*types.Struct); !isStruct {
			return false
		}

		base = inner.X
		depth++
	}

	ident, ok := base.(*ast.Ident)

	if !ok || ident.Name != recvName || depth == 0 {
		return false
	}

	if depth > 1 && recvObj != nil && info.ObjectOf(ident) != recvObj {
		return false
	}

	return true
}

// bodyTakesImplicitReceiverFieldAddress reports whether the body calls a POINTER-receiver method on
// a VALUE-struct field of the receiver — `v.field.M(…)` where field is a value struct and M has a
// pointer receiver. Go implicitly takes `&v.field` to make that call, so it is a receiver-field
// address exactly like the explicit `&v.field` bodyTakesReceiverFieldAddress catches, and it forces
// the receiver BOX for the same reason: only `Ꮡv.of(field)` yields an ALIASING field pointer, while
// a ref-receiver primary has no `Ꮡv` and `Ꮡ(v.field)` would box a COPY and drop M's writes. Such a
// method therefore cannot be a ref-return primary. Measured on edwards25519's projP2.Zero —
// `v.X.Zero()` (X a field.Element, Zero a pointer method) was promoted to `this ref projP2 v` yet
// its body still emitted `Ꮡv.of(projP2.ᏑX).Zero()`, 99 CS0103 across the package (the explicit-only
// predicate never saw the implicit address). Flag-gated by the dual-recv selection it feeds.
func bodyTakesImplicitReceiverFieldAddress(body *ast.BlockStmt, recvName string, recvObj types.Object, info *types.Info) bool {
	found := false

	ast.Inspect(body, func(node ast.Node) bool {
		call, ok := node.(*ast.CallExpr)

		if !ok {
			return true
		}

		sel, ok := call.Fun.(*ast.SelectorExpr)

		if !ok {
			return true
		}

		funcObj, ok := info.ObjectOf(sel.Sel).(*types.Func)

		if !ok || funcObj == nil {
			return true
		}

		signature, ok := funcObj.Type().(*types.Signature)

		if !ok || signature.Recv() == nil {
			return true
		}

		// A value-receiver method takes no address of its argument; only a pointer receiver forces
		// Go to address `v.field`.
		if _, isPtr := signature.Recv().Type().(*types.Pointer); !isPtr {
			return true
		}

		if exprIsReceiverValueFieldChain(sel.X, recvName, recvObj, info) {
			found = true
			return false
		}

		return true
	})

	return found
}

// captureModeMethodValueReceiver returns the receiver identifier of a deferred/go call whose
// target is a capture-mode method value on a heap-boxed or addressed-global receiver — e.g.
// `defer locked.Store(0)` where `locked` is a (boxed) atomic. Such a receiver must NOT be
// snapshot-captured into a value copy: the box (`Ꮡlocked`) is the stable defer-time receiver
// (matching Go's `&locked`) and is accessible directly in the lambda, while a value copy has no
// box and cannot call the ж-overload (CS1929/CS0103). Returns nil when no such receiver applies.
func (v *Visitor) captureModeMethodValueReceiver(call *ast.CallExpr) *ast.Ident {
	if call == nil {
		return nil
	}

	sel, ok := call.Fun.(*ast.SelectorExpr)

	if !ok {
		return nil
	}

	recvIdent, ok := sel.X.(*ast.Ident)

	if !ok || !v.isCaptureModeMethod(sel) {
		return nil
	}

	if v.isHeapBoxedExpr(recvIdent) || v.isAddressedGlobal(recvIdent) {
		return recvIdent
	}

	return nil
}

// isCaptureModeMethod reports whether the selector calls a package capture-mode method.
func (v *Visitor) isCaptureModeMethod(selectorExpr *ast.SelectorExpr) bool {
	if packageCaptureModeMethods == nil {
		return false
	}

	funcObj, _ := v.info.ObjectOf(selectorExpr.Sel).(*types.Func)

	// A call to a generic method resolves to the instantiation (e.g. Set[int]); normalize
	// to the generic origin, which is what the pre-pass recorded.
	return funcObj != nil && packageCaptureModeMethods[funcObj.Origin()]
}

// isDirectBoxReceiverMethod reports whether the given func declaration is a generic
// capture-mode method emitted with the box as its receiver (direct-ж).
func isDirectBoxReceiverMethod(funcDecl *ast.FuncDecl, info *types.Info) bool {
	if packageDirectBoxReceiverMethods == nil || funcDecl == nil || funcDecl.Name == nil {
		return false
	}

	funcObj, _ := info.Defs[funcDecl.Name].(*types.Func)

	return funcObj != nil && packageDirectBoxReceiverMethods[funcObj.Origin()]
}

// identResolvesToReceiver reports whether ident is the current method's receiver by OBJECT
// identity, not just name: a local or range var SHADOWING the receiver name must not take the
// receiver-specific renders — crypto/x509 isValid's `for _, c := range currentChain` inside
// `func (c *Certificate)` (direct-ж) emitted the never-declared box `ᏑcΔ1` when the range var
// was passed as a pointer argument (CS0103). Falls back to the name match when the ident does
// not resolve, mirroring identIsParameter's paramObjects guard.
func (v *Visitor) identResolvesToReceiver(ident *ast.Ident, recvName string) bool {
	if ident.Name != recvName {
		return false
	}

	if v.currentFuncSignature != nil {
		if recv := v.currentFuncSignature.Recv(); recv != nil {
			if obj := v.info.ObjectOf(ident); obj != nil {
				return obj == recv
			}
		}
	}

	return true
}

// exprIsCurrentDirectBoxReceiver reports whether expr is the bare receiver identifier of the
// current method when that method is direct-ж — i.e. its receiver box `Ꮡrecv` is in scope. Used
// to route `recv.method()` (a direct-ж method called on the receiver) through that box.
func (v *Visitor) exprIsCurrentDirectBoxReceiver(expr ast.Expr) bool {
	ident, ok := expr.(*ast.Ident)

	if !ok || v.currentFuncDecl == nil {
		return false
	}

	isPtrRecv, recvName := v.isPointerReceiver()

	return isPtrRecv && v.identResolvesToReceiver(ident, recvName) && isDirectBoxReceiverMethod(v.currentFuncDecl, v.info)
}

// exprIsCaptureModeFieldBase reports whether expr is a value field whose address can be taken as a
// field box, so a capture-mode method called on it (`base.field.Load()`) routes through
// `(&base.field)`. Two bases qualify: the current method's direct-ж receiver (`recv.field` → box
// `Ꮡrecv.of(RecvType.ᏑField)`; the fixpoint marks such methods direct-ж via
// bodyCallsCaptureModeMethodOnReceiverField), or any pointer expression (`e.field` where `e` is
// `*T` → `e.of(T.ᏑField)`, since `e` is already the box). convUnaryExpr renders `&base.field`
// to the matching form.
func (v *Visitor) exprIsCaptureModeFieldBase(expr ast.Expr) bool {
	sel, ok := expr.(*ast.SelectorExpr)

	if !ok {
		return false
	}

	// Must be a field selection (not a method value) and a value field (not already a pointer —
	// a pointer field already carries its own box and routes normally).
	selection, ok := v.info.Selections[sel]

	if !ok || selection.Kind() != types.FieldVal {
		return false
	}

	if _, isPtr := v.info.TypeOf(sel).(*types.Pointer); isPtr {
		return false
	}

	if v.exprIsCurrentDirectBoxReceiver(sel.X) {
		return true
	}

	// A field of a pointer *variable* (`e.field`, e an *T identifier): `e` is the box, so
	// `e.of(...)` works. Restricted to a bare identifier to match convUnaryExpr's `&e.field` form.
	baseIdent, ok := sel.X.(*ast.Ident)

	if !ok {
		return false
	}

	_, isPtr := v.info.TypeOf(baseIdent).(*types.Pointer)

	return isPtr
}

// exprIsDerefdPointerParam reports whether expr is a pointer-typed parameter. Such a parameter is
// emitted deref'd to a value alias (`ref var p = ref Ꮡp.Value`) with the box `Ꮡp` as the actual
// parameter, so a direct-ж method called on it must route through `Ꮡp` (a pointer *local* holds
// the box directly and needs no routing).
func (v *Visitor) exprIsDerefdPointerParam(expr ast.Expr) bool {
	ident, ok := expr.(*ast.Ident)

	if !ok || !v.identIsParameter(ident) {
		return false
	}

	_, isPtr := v.getIdentType(ident).(*types.Pointer)

	return isPtr
}

// exprHasReceiverBoxInScope reports whether a deref-aliased pointer identifier ALSO has its box
// `Ꮡ<name>` declared in the enclosing scope — the box-bearing SUBSET of exprIsDerefAliasedPointer
// below, and the only predicate that licenses SPELLING `Ꮡx` at a call site:
//
//   - a pointer PARAMETER is emitted `ж<T> Ꮡp` with `ref var p = ref Ꮡp.Value`, so the box IS the
//     parameter and always exists;
//   - a pointer RECEIVER has a box only when the enclosing method is DIRECT-ж (`this ж<T> Ꮡrecv`).
//     A `[GoRecv] this ref T recv` primary is the value alias and NOTHING else — no `Ꮡrecv` is
//     declared anywhere in that body.
//
// The two are worth separating because exprIsDerefAliasedPointer answers a DIFFERENT question, and
// answers it correctly for both arms: "does `x` already render as the pointed-to value, so a `~`
// deref would be CS0023?" Reading it as "does `x` have a box?" is what put `Ꮡa.regAssign(Ꮡt, 0)`
// into reflect's `addArg` — a `[GoRecv] this ref abiSeq a` body — the moment `abiSeq.regAssign` was
// registered as a hand-own (CS0103, measured 2026-09-03).
//
// The ordinary (non-displaced) call path never needs this gate, and that is a property of the
// capture-mode fixpoint rather than luck: it routes a caller through a receiver box only when the
// CALLEE is direct-ж, and bodyCallsDirectBoxMethodOnReceiver promotes the caller to direct-ж when it
// is — so the box it spells is always declared. A manualConversionFuncs registration is what
// bypassed that pairing; requiring the box here restores it.
func (v *Visitor) exprHasReceiverBoxInScope(expr ast.Expr) bool {
	return v.exprIsDerefdPointerParam(expr) || v.exprIsCurrentDirectBoxReceiver(expr)
}

// exprIsDerefAliasedPointer reports whether expr is a bare identifier that is a deref-aliased
// pointer — a pointer PARAMETER or a pointer RECEIVER, both emitted as the pointed-to value
// (`ref T x`), not the box itself. A pointer LOCAL, by contrast, holds the box directly. Callers
// that would otherwise deref a pointer (`~x`) must skip it for these, since `x` is already the
// value (a `~` on it would deref a non-pointer → CS0023).
//
// It says NOTHING about whether a box `Ꮡx` exists — a `[GoRecv] this ref T` receiver has none. Use
// exprHasReceiverBoxInScope above for that question.
func (v *Visitor) exprIsDerefAliasedPointer(expr ast.Expr) bool {
	if v.exprIsDerefdPointerParam(expr) {
		return true
	}

	ident, ok := expr.(*ast.Ident)

	if !ok {
		return false
	}

	// The receiver match is by OBJECT identity (identResolvesToReceiver): a local that SHADOWS the
	// receiver (`r := &y` inside a method with receiver `r`) must not take this gate — e.g.
	// `unsafe.Pointer(r)` on the inner pointer local would emit the receiver's `FromRef(ref r)`
	// form against a genuine box (pinning the box reference slot: compiles, silently wrong
	// address). The rendered==raw check stays as the fallback defense for an unresolvable ident
	// (the shadow-rename pass gives every inner same-named binding a `Δ` name, while the receiver
	// always renders under its raw name).
	if isPtrRecv, recvName := v.isPointerReceiver(); isPtrRecv && v.identResolvesToReceiver(ident, recvName) && v.getIdentName(ident) == ident.Name {
		return true
	}

	return false
}

// paramBoxReasonHolds reports whether the entry-time heap box a VALUE parameter was marked for is
// warranted against the given body: an EXPLICIT address-of on its own storage (`&p`, `&p.field…`,
// `&p[i]` — paramAddressTakenNeedsBox), a capture-mode pointer-receiver CALL on it, a pointer-receiver
// METHOD VALUE of it (the implicit `(&p).M`), or a by-box lambda capture. The analysis arm
// (markCaptureModeBoxedParams) and every emitter that materializes the box must agree on this exact
// predicate — visitFuncDecl's parameter preamble, its processPotentialCapture box-ref arm, and
// convFuncLit's literal prologue all read it. A reason recorded by analysis but missing here leaves
// body uses referencing a box (`Ꮡp`) that was never declared: CS0103; the reverse declares a box
// nothing references. Widen the two together, never one alone.
func (v *Visitor) paramBoxReasonHolds(ident *ast.Ident, param types.Object, body ast.Node) bool {
	return v.paramAddressTakenNeedsBox(param, body) ||
		v.bodyCallsCaptureModeMethodOn(ident, body) ||
		v.pointerMethodValueAddressTaken(param, body) ||
		v.isLambdaBoxRefVar(param)
}

// recvBoxReasonHolds reports whether the entry-time heap box a method's VALUE RECEIVER was marked
// for is warranted against the given body: an EXPLICIT address-of on the receiver's own storage
// (`&r`, `&r.field…`, `&r[i]` — paramAddressTakenNeedsBox). It is the emission-side twin of
// markAddressTakenBoxedReceiver and must stay identical to it, exactly as paramBoxReasonHolds must
// to markCaptureModeBoxedParams: a reason recorded by analysis but missing here leaves body uses
// referencing a box (`Ꮡr`) that was never declared (CS0103), and the reverse declares a box nothing
// references. paramNeedsHeapBox reads it for the receiver, which drives the `ʗp` signature rename
// and the entry-time preamble.
//
// The receiver set is deliberately NARROWER than the parameter set: a capture-mode (direct-ж)
// receiver is served by packageDirectBoxReceiverMethods (which emits the box AS the receiver rather
// than renaming it), and a receiver the capture analysis routed to box-ref storage must not take the
// `ʗp` form at all — so neither bodyCallsCaptureModeMethodOn nor isLambdaBoxRefVar joins here.
func (v *Visitor) recvBoxReasonHolds(recv types.Object, body ast.Node) bool {
	return v.paramAddressTakenNeedsBox(recv, body)
}

// bodyCallsCaptureModeMethodOn reports whether the body calls a capture-mode method with
// the given identifier as the (value) receiver — meaning that identifier must be boxed.
func (v *Visitor) bodyCallsCaptureModeMethodOn(ident *ast.Ident, body ast.Node) bool {
	if ident == nil {
		return false
	}

	return v.bodyCallsCaptureModeMethodOnObject(v.info.ObjectOf(ident), body)
}

// bodyCallsCaptureModeMethodOnObject is bodyCallsCaptureModeMethodOn keyed by the resolved
// object — the form a type-switch case binding needs, whose defining ident carries no object
// (go/types records the per-case *types.Var in Implicits, and ObjectOf on the guard ident
// answers nil). The receiver operand may be the bare ident OR a value-field chain rooted at it
// (`x.i.Add(delta)` — the sync/atomic shape): Go's implicit `&x.i` for the chain form addresses
// the local's own storage exactly as `&x` does, so leaving the chain unrecognized let emission
// fall to the `Ꮡ(x).of(…)` copy-box and silently dropped every write the capture-mode method
// made. The chain walk is selectorChainRootsAtIdent — the same root walk the explicit-`&` arm
// and selectsPointerMethodOn (the method-VALUE analogue of this call form) already use, whose
// Selection.Indirect() gate excludes any chain that crosses a pointer (the address then lands
// in the pointee, not in the local).
func (v *Visitor) bodyCallsCaptureModeMethodOnObject(target types.Object, body ast.Node) bool {
	if packageCaptureModeMethods == nil || target == nil {
		return false
	}

	found := false

	ast.Inspect(body, func(node ast.Node) bool {
		callExpr, ok := node.(*ast.CallExpr)

		if !ok {
			return true
		}

		selectorExpr, ok := callExpr.Fun.(*ast.SelectorExpr)

		if !ok {
			return true
		}

		recvExpr := ast.Unparen(selectorExpr.X)

		if recvIdent, ok := recvExpr.(*ast.Ident); ok {
			if v.info.ObjectOf(recvIdent) != target {
				return true
			}

			// Only a value receiver needs boxing; a pointer receiver already carries its box.
			if _, isPointer := v.info.TypeOf(recvIdent).(*types.Pointer); isPointer {
				return true
			}
		} else {
			if !selectorChainRootsAtIdent(recvExpr, target, v.info) {
				return true
			}

			// A pointer-typed chain result hands over the pointer VALUE — no address of the
			// target is taken (mirrors selectsPointerMethodOn's receiver-operand rule).
			if recvType := v.info.TypeOf(recvExpr); recvType == nil {
				return true
			} else if _, isPointer := recvType.Underlying().(*types.Pointer); isPointer {
				return true
			}
		}

		if v.isCaptureModeMethod(selectorExpr) {
			found = true
			return false
		}

		return true
	})

	return found
}

// bodyHasOwnDefer reports whether the body contains a defer statement of its OWN (nested func
// literals' defers belong to the literal) — arm (a)'s GoFrame exclusion: C# forbids `ref` returns
// inside try, and a deferring body emits as one.
func bodyHasOwnDefer(body *ast.BlockStmt) bool {
	found := false

	ast.Inspect(body, func(n ast.Node) bool {
		if found {
			return false
		}

		if _, isLit := n.(*ast.FuncLit); isLit {
			return false
		}

		if _, isDefer := n.(*ast.DeferStmt); isDefer {
			found = true
			return false
		}

		return true
	})

	return found
}

// bodyReturnsReceiverThroughCall reports whether the body has a return whose single result is a
// method CALL on the bare receiver ident (`return v.M(…)`) — the S1 forwarding-return shape. It
// does NOT check whether M is selected (the fixpoint does); it only widens candidacy so a
// forwarding-only method (carryPropagate) enters the running. recvObj-based, so a shadowing local
// does not match.
func bodyReturnsReceiverThroughCall(body *ast.BlockStmt, recvName string, info *types.Info) bool {
	recvObj := recvObjectByName(body, recvName, info)

	if recvObj == nil {
		return false
	}

	found := false

	ast.Inspect(body, func(n ast.Node) bool {
		if found {
			return false
		}

		if _, isLit := n.(*ast.FuncLit); isLit {
			return false
		}

		ret, isReturn := n.(*ast.ReturnStmt)

		if !isReturn || len(ret.Results) != 1 {
			return true
		}

		if call, ok := ret.Results[0].(*ast.CallExpr); ok {
			if sel, ok := call.Fun.(*ast.SelectorExpr); ok {
				if base, ok := sel.X.(*ast.Ident); ok && info.Uses[base] == recvObj {
					found = true
					return false
				}
			}
		}

		return true
	})

	return found
}

// recvObjectByName resolves the receiver name to its object through the body's uses — the same
// resolution receiverObjectFor does from the declaration, available where only a name is in hand.
func recvObjectByName(body *ast.BlockStmt, recvName string, info *types.Info) types.Object {
	var found types.Object

	ast.Inspect(body, func(n ast.Node) bool {
		if found != nil {
			return false
		}

		if ident, ok := n.(*ast.Ident); ok && ident.Name == recvName {
			if obj := info.Uses[ident]; obj != nil {
				found = obj
				return false
			}
		}

		return true
	})

	return found
}
