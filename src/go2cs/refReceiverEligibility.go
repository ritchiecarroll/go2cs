// refReceiverEligibility.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/ast"
	"go/types"
)

// B′ §4.1 receiver eligibility — which pointer-receiver methods may dual-emit.
//
// This is the ANALYSIS half of stage S0 and it emits nothing: it records, per method, whether the
// declaration is eligible to carry a `[GoRecv] this ref T` primary beside the ж twin RecvGenerator
// already mints. Nothing reads the verdict at S0a; the census prints it, exactly as A1's parameter
// classification landed before A2 consumed it.
//
// Why this is a separate, smaller thing than Phase A's classifier, and why that is not a shortcut:
// §4.1's rule is DECLARATION-LOCAL by construction — "everything else dual-emits unconditionally at
// the declaration, because selection pressure lives at call sites, not declarations". There is no
// fixed point to resolve and no whole-program question to answer, so eligibility needs none of the
// two-sided strip machinery `refLoweringAnalysisOperations.go` runs for package-level functions.
// That is also why §8 OQ-2's receiver-only ruling for S0 is load-bearing rather than cosmetic: with
// parameters left boxed, S0 never needs the method-scope parameter fixed point at all. That
// apparatus — which does not exist today, since collectFunc returns early for every method — is
// S1's parameter half.
const (
	// XM-1: declared in a hand-owned file, or supplied by an *_impl.cs companion. The converter does
	// not re-emit these, and a hand-own already chose its own form — the same reasoning (and the
	// same mechanical per-file marker probe) as Phase A's X5-hand-owned arm, reused verbatim.
	refRecvVetoHandOwned = "XM-1-hand-owned"
	// XM-2: value receiver. Not in scope rather than refused — a value receiver already emits the
	// `this T` / `this ref T` shapes without a box, so there is no box for B′ to remove. Recorded so
	// the census denominator stays honest about what it looked at.
	refRecvVetoValueReceiver = "XM-2-value-receiver"
	// XM-3: the receiver's base is an interface or a type parameter — no struct storage to `ref`.
	refRecvVetoNoStorage = "XM-3-no-storage"
	// XM-4: a //go:linkname participant. The registries publicize the symbol across the assembly
	// boundary in the boxed convention, invisibly to a per-package scan — Phase A's X5-linkname arm,
	// reused for the same reason it exists there.
	refRecvVetoLinkname = "XM-4-linkname"
	// XM-5: the receiver type's C# representation IS the box (ж<T> subclasses, reflect-bridge
	// special types), so a `ref T` receiver would name a type that does not exist in that form.
	// Curated rather than inferred, exactly like the linkname and hand-own-caller registries and for
	// the same reason: the frozen C# these describe is invisible to a Go-source scan. The set is
	// deliberately empty today — every instance the corpus actually carries is declared in a
	// hand-owned file and is therefore already caught by XM-1 — and the census will say so rather
	// than leaving the arm's emptiness to be assumed.
	refRecvVetoBoxRepresented = "XM-5-box-represented"
	// XM-6: the receiver ESCAPES — the R3 ruling's veto (2026-09-02 amendment), covering both the
	// return dimension (the receiver appears among a return's results beside non-receiver results —
	// the `return v, nil` family) and the use dimension (any body use of the receiver identifier
	// outside the emittable set: a selector base, a bare receiver-return, or an argument at a
	// LOWERED same-package parameter position — everything else needs the box the primary does not
	// have, including any use inside a nested func literal, whose closure cannot capture a ref).
	// Measured on the S0 packages before it was ruled: ~13 of 75 eligible methods, all top-level
	// orchestrators whose per-run call counts are noise against the field-op leaf traffic that
	// drives the A3 floor.
	refRecvVetoReceiverEscapes = "XM-6-receiver-escapes"
)

// R3 arms recorded on eligible verdicts by classifyMethodBodiesR3 (the 2026-09-02 amendment).
const (
	refRecvR3ArmRefReturn = "ref-return"
	refRecvR3ArmPlain     = "plain"
)

// refRecvBoxRepresentedTypes is XM-5's curated set, keyed `<canonical-pkg-path>.<TypeName>`. Empty
// by construction today (see refRecvVetoBoxRepresented); adding an entry is a deliberate act with a
// cited C# declaration, never a guess from Go source.
var refRecvBoxRepresentedTypes = map[string]bool{}

// refMethodVerdict records one method's §4.1 eligibility. A method with no vetoes is eligible to
// dual-emit; the vetoes are kept as a list rather than a bool so the census can report WHY, which
// is what makes an unexpectedly small eligible set diagnosable instead of merely disappointing.
type refMethodVerdict struct {
	PkgPath  string   `json:"pkg"`
	Recv     string   `json:"recv"`
	Name     string   `json:"name"`
	Exported bool     `json:"exported"`
	Vetoes   []string `json:"vetoes,omitempty"`
	// R3Arm records the fluent-body ruling's arm for an ELIGIBLE method (the 2026-09-02 dated
	// amendment): "ref-return" — every return result is the bare receiver, so the PRIMARY returns
	// `ref T` and its receiver-returns rewrite to `return ref v;` with the twin returning its own
	// box; "plain" — no return mentions the receiver, the ratified §3 shapes verbatim. Empty when
	// the method is vetoed (XM-6 included). Filled by classifyMethodBodiesR3 (pass 5), which runs
	// AFTER the Phase-A fixed point because its lowered-argument arm consults the resolved set.
	R3Arm string `json:"r3Arm,omitempty"`

	// The declaration and receiver object, retained for pass 5's body walk (never serialized —
	// the census output stays position-independent).
	decl    *ast.FuncDecl
	recvObj types.Object
}

// Eligible reports whether this method may carry the ref-receiver primary.
func (verdict *refMethodVerdict) Eligible() bool {
	return len(verdict.Vetoes) == 0
}

// classifyMethodReceiver applies §4.1 to one method declaration and returns its verdict. It is
// called from collectFunc's method branch, which previously did nothing but count pointer-param
// positions for B′'s pricing context.
//
// fileIsHandOwned carries the declaring file's manual-conversion status, exactly as the
// package-level path receives it.
func (a *refLoweringAnalysis) classifyMethodReceiver(funcDecl *ast.FuncDecl, obj *types.Func, signature *types.Signature, fileIsHandOwned bool) *refMethodVerdict {
	recv := signature.Recv()

	if recv == nil {
		return nil
	}

	verdict := &refMethodVerdict{
		PkgPath:  a.pkg.Path(),
		Recv:     refReceiverBaseName(recv.Type()),
		Name:     obj.Name(),
		Exported: obj.Exported(),
	}

	recvType := types.Unalias(recv.Type())
	pointer, isPointer := recvType.(*types.Pointer)

	if !isPointer {
		// XM-2 — and return immediately: the remaining arms all reason about a pointer receiver's
		// base, and reporting them alongside "this was never a pointer" would inflate every count.
		verdict.Vetoes = append(verdict.Vetoes, refRecvVetoValueReceiver)
		return verdict
	}

	if !refReceiverHasRefStorage(pointer.Elem()) {
		verdict.Vetoes = append(verdict.Vetoes, refRecvVetoNoStorage)
	}

	if fileIsHandOwned || funcDecl.Body == nil {
		// A bodiless method is the assembly/cgo partial-stub shape: its emitted partial declaration
		// pairs with a hand-written *_impl.cs, so its form is frozen for the same reason a
		// hand-owned file's is. Folded into XM-1 rather than given its own arm, because the remedy
		// and the reasoning are identical.
		verdict.Vetoes = append(verdict.Vetoes, refRecvVetoHandOwned)
	}

	if a.isLinknameExposed(obj.Name()) || a.isLinknameExposed(verdict.Recv+"."+obj.Name()) {
		// Both spellings are probed because the registries key methods inconsistently across the
		// three of them (handle sets carry the bare name; the forward/push maps carry the qualified
		// form). Probing one and trusting it is how a linkname participant slips through.
		verdict.Vetoes = append(verdict.Vetoes, refRecvVetoLinkname)
	}

	if refRecvBoxRepresentedTypes[refCanonicalPkgPath(a.pkg.Path())+"."+verdict.Recv] {
		verdict.Vetoes = append(verdict.Vetoes, refRecvVetoBoxRepresented)
	}

	// Retained for pass 5 (classifyMethodBodiesR3): the R3 body walk needs the declaration and
	// the receiver's own object, and only this call site holds both.
	verdict.decl = funcDecl

	if len(funcDecl.Recv.List) > 0 && len(funcDecl.Recv.List[0].Names) > 0 {
		verdict.recvObj = a.info.Defs[funcDecl.Recv.List[0].Names[0]]
	}

	return verdict
}

// classifyMethodBodiesR3 is pass 5 of analyzeRefLowering — the R3 arms (the 2026-09-02 dated
// amendment in DESIGN-zh-box-b-prime.md §10). It runs AFTER the Phase-A fixed point because its
// lowered-argument arm consults the resolved position set: for each still-ELIGIBLE method it walks
// the body once, classifying every use of the receiver identifier and every return statement, and
// records either an arm (ref-return / plain) or the XM-6 veto.
//
// The emittable-use set is deliberately CLOSED — a shape this pass cannot prove box-free vetoes
// the method rather than trusting the emitter to improvise (the same whitelist discipline the A1
// classifier runs on): a SELECTOR BASE (`v.f`, `v.M(…)` — the ref renders both directly), a BARE
// receiver-return (arm ref-return's rewrite), and an ARGUMENT at a same-package parameter position
// the fixed point LOWERED (`feMul(v, x, y)` — the position takes `ref`, so `ref v` is the exact
// A2 row-2 plain-local emission). An unnamed receiver has no uses by construction and takes the
// plain arm. Everything else — receiver as a ж-position argument, deref-assignment through it,
// composite capture, any use inside a nested func literal (a closure cannot capture a ref) —
// escapes, and escaping is XM-6.
func (a *refLoweringAnalysis) classifyMethodBodiesR3() {
	for _, verdict := range a.result.Methods {
		if !verdict.Eligible() || verdict.decl == nil || verdict.decl.Body == nil {
			continue
		}

		if verdict.recvObj == nil {
			verdict.R3Arm = refRecvR3ArmPlain
			continue
		}

		parents := buildParentMap(verdict.decl)
		bareReturns, otherReturnResults, escapes := 0, 0, false

		ast.Inspect(verdict.decl.Body, func(n ast.Node) bool {
			if escapes {
				return false
			}

			ident, isIdent := n.(*ast.Ident)

			if !isIdent || a.info.Uses[ident] != verdict.recvObj {
				return true
			}

			// A receiver use inside a nested func literal escapes unconditionally: the primary's
			// receiver is a ref, and C# closures cannot capture one.
			for p := parents[ident]; p != nil; p = parents[p] {
				if _, inLit := p.(*ast.FuncLit); inLit {
					escapes = true
					return false
				}

				if p == verdict.decl.Body {
					break
				}
			}

			switch parent := parents[ident].(type) {
			case *ast.SelectorExpr:
				if parent.X == ident {
					return true // v.f / v.M(…) — renders off the ref directly
				}
			case *ast.StarExpr:
				// `*v` — a deref THROUGH the receiver, read or written (`*v = feZero`, `x := *v`).
				// Under a ref receiver the deref is the receiver itself: `v = feZero` / `x = v`.
				// Unconditionally emittable; it is what un-vetoes the Zero/One/Set family.
				return true
			case *ast.ReturnStmt:
				bareReturns++
				return true
			case *ast.CallExpr:
				for argIndex, arg := range parent.Args {
					if arg != ident {
						continue
					}

					if callee := a.resolveCandidateCallee(parent); callee != nil && callee.Pkg() == a.pkg {
						key := refPosKey{PkgPath: a.pkg.Path(), Func: callee.Name(), Index: argIndex}

						if a.result.LoweredPositions[key] {
							return true // argument at a LOWERED position — `ref v`, the row-2 emission
						}
					}

					break
				}
			}

			escapes = true
			return false
		})

		// Return-shape analysis, two halves. (1) The multi-result mixed family: a return carrying
		// the bare receiver beside other results (`return v, nil`) — count the others. (2) The
		// FLUENT-THROUGH-CALLEE family, caught by TYPE rather than by ident: a result whose static
		// type is the receiver's own pointer type but is NOT the bare receiver ident
		// (`return v.carryPropagateGeneric()` — Go returns the receiver's pointer THROUGH the
		// chained callee) needs a ж the primary does not have; under a ref receiver the chained
		// call would bind the callee's own primary and return `ref`, which cannot satisfy a
		// ж-typed return (CS). At S0 that is an escape; S1's parameter half is where chained
		// ref-returns become expressible. The selector-base USE check above cannot see this — the
		// use is emittable, the RETURN of its result is not.
		if !escapes {
			recvPtrType := a.info.Defs[verdict.decl.Recv.List[0].Names[0]].Type()

			ast.Inspect(verdict.decl.Body, func(n ast.Node) bool {
				if escapes {
					return false
				}

				if _, inLit := n.(*ast.FuncLit); inLit {
					return false // a literal's returns are its own
				}

				ret, isReturn := n.(*ast.ReturnStmt)

				if !isReturn {
					return true
				}

				carriesReceiver := false

				for _, result := range ret.Results {
					if id, isID := result.(*ast.Ident); isID && a.info.Uses[id] == verdict.recvObj {
						carriesReceiver = true
						continue
					}

					if resultType := a.info.TypeOf(result); resultType != nil && types.Identical(resultType, recvPtrType) {
						escapes = true
						return false
					}
				}

				if carriesReceiver {
					for _, result := range ret.Results {
						if id, isID := result.(*ast.Ident); !isID || a.info.Uses[id] != verdict.recvObj {
							otherReturnResults++
						}
					}
				}

				return true
			})
		}

		switch {
		case escapes, bareReturns > 0 && otherReturnResults > 0:
			verdict.Vetoes = append(verdict.Vetoes, refRecvVetoReceiverEscapes)
		case bareReturns > 0:
			verdict.R3Arm = refRecvR3ArmRefReturn
		default:
			verdict.R3Arm = refRecvR3ArmPlain
		}
	}
}

// refReceiverHasRefStorage reports whether a pointer receiver's base type has struct storage a
// `ref` can bind — XM-3's test. An interface has no storage of its own, and a type parameter names
// no single layout, so neither can carry the primary.
func refReceiverHasRefStorage(base types.Type) bool {
	base = types.Unalias(base)

	if _, isTypeParam := base.(*types.TypeParam); isTypeParam {
		return false
	}

	if _, isInterface := base.Underlying().(*types.Interface); isInterface {
		return false
	}

	return true
}

// refReceiverBaseName renders a receiver type's base name (`*Element` → `Element`) for census
// keying. Unnamed bases render empty rather than synthesized: a method cannot be declared on one in
// Go, so an empty name is a signal the caller should never see, not a case to paper over.
func refReceiverBaseName(recvType types.Type) string {
	t := types.Unalias(recvType)

	if pointer, isPointer := t.(*types.Pointer); isPointer {
		t = types.Unalias(pointer.Elem())
	}

	switch named := t.(type) {
	case *types.Named:
		return named.Obj().Name()
	case *types.TypeParam:
		return named.Obj().Name()
	}

	return ""
}
