// anonStructTypeArgLift.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import "go/ast"

// explicitCallTypeArgs returns the type-argument expressions a call WRITES at its Fun, or nil when
// the call has none. `F[A](x)` parses as *ast.IndexExpr and `F[A, B](x)` as *ast.IndexListExpr, so
// both spellings are peeled; parentheses around the callee are peeled with them.
//
// An ordinary index is the same syntax — `funcs[i](x)` is also an *ast.IndexExpr — so this cannot
// be used to decide that a call IS an instantiation.
//
// ⚠ CORRECTED 2026-09-20, C1's design read (`be9a74470` §5). This comment used to say it was safe
// "because a `struct{…}` syntax node can never be a value in an index position". That sentence is
// true and it does NOT make the claim it was offered for, because the caller does not test for a
// `struct{…}` node: it calls extractStructType, whose typeSyntaxOf step PEELS VALUE FORMS
// (`*ast.CompositeLit → .Type`, `*ast.CallExpr → .Fun`, `*ast.TypeAssertExpr → .Type`). So three
// VALUE spellings in an index position do reach the lift, and C1 planted all three and measured them
// reaching it:
//
//	m[struct{ a int }{1}]()      a composite literal
//	m[struct{ b int }(v)]()      a conversion
//	m[x.(struct{ a int })]()     a type assertion
//
// What is actually true, and is a measurement rather than an argument: **the corpus has none of
// them.** C1's census, calling this very function in process over both trees — 411 index-callee
// calls and 473 indices tested at 1.24, 879 and 941 at 1.25.1 — found the two written type arguments
// in `reflect/all_test.go` and RESIDUAL 0, with the repo's own 1,088 Go sources also 0.
//
// And the residual is benign where it would fire: an anonymous struct written as a map key, a
// conversion target or an assertion type is a real Go type that needs a C# name anyway, so lifting
// and publishing it is the act this file intends rather than a mis-lift. The untested exposure is
// ORDERING — the declaration would be minted at the top of the enclosing call rather than where the
// literal renders — and nothing in the corpus reaches it.
//
// Stated at this length because the sentence it replaces was a FALSE INVARIANT, which is worse than
// no comment: the next reader builds on it.
func explicitCallTypeArgs(fun ast.Expr) []ast.Expr {
	for {
		paren, isParen := fun.(*ast.ParenExpr)

		if !isParen {
			break
		}

		fun = paren.X
	}

	switch indexed := fun.(type) {
	case *ast.IndexExpr:
		return []ast.Expr{indexed.Index}
	case *ast.IndexListExpr:
		return indexed.Indices
	}

	return nil
}

// liftExplicitAnonStructTypeArgs lifts — and PUBLISHES into the package-wide dedup registry — every
// non-empty anonymous struct written as an explicit type argument of this call.
//
// ⚠ WHY THIS EXISTS AT ALL, because the emission looks like the lift is missing and it is not.
// A type-argument position renders through getAliasQualifiedTypeName, whose anonymous-struct arm
// (deferredDynamicTypeName) resolves in FOUR steps:
//
//	1  v.liftedNameFor(t)                  this visitor's liftedTypeMap, THEN productionAliasLiftedTypes
//	2  lookupDynamicTypeName(signature)     the shared PACKAGE registry  <- the route this file opens
//	3  lookupProductionDynamicTypeName(..)  the PRODUCTION registry (the -tests reference model)
//	4  dynamicTypeMarker(signature)         the deferred marker
//
// (⚠ corrected 2026-09-20 from "three steps" on C1's design read, `be9a74470` §3: the original
// collapsed step 3 and half of step 1. Step 3 could never have held this type — the sites are in
// `all_test.go` and production `reflect` carries no such signature — so the diagnosis survives the
// miscount, but the chain is what the next reader uses to decide where a FIFTH route belongs.)
//
// Step 1 is TYPE-IDENTITY keyed and the type reaching it comes from the resolved instance
// (info.Instances), which is not the same *types.Struct the lift recorded for the written syntax
// node — so it misses even for a lift minted moments earlier in the same function. That leaves the
// SIGNATURE-keyed registry as the only route, and visitStructType publishes into it only for a
// package-level lift or one at a call boundary (`!v.inFunction || v.liftAtCallBoundary`). A type
// argument is a position that gate never enumerated, so a function-scoped one minted its lift,
// declared it, and published nothing; the marker then resolved to the raw Go signature, which is
// never valid C#.
//
// ⚠ AND THE TWO-SITE DEDUP RUNS THROUGH A THIRD LINK, which this file does not perform and a reader
// will otherwise look for here (C1 `be9a74470` §2). Publication alone does not stop the second site
// declaring its own type: `visitStructType.go:168-207`'s signature-keyed REUSE block does. Site 1
// publishes; site 2's own lift then finds the name at `:169`, is admissible because `v.inFunction`,
// records it in liftedTypeMap/liftedAnonStructNames and RETURNS WITHOUT EMITTING A DECLARATION. That
// is why the emission shows two sites naming one type and `TestTypeFieldReadOnly_type` with zero
// declarations and zero references. `liftedTypeExists` does not provide it — it is keyed on
// `v.getType(expr)`, and two syntax nodes writing the identical shape are different *types.Struct
// values, so it returns false at site 2 and the second lift genuinely runs.
//
// MEASURED SHAPE (2026-09-20, the H10 pre-staging read of the reflect row at the 1.24.13 version
// tip): `reflect.TypeFor[struct{ f int }]()` at all_test.go:3547 — a NEW assertion inside the
// pre-existing TestAllocations — and at :6921 inside TestTypeFieldReadOnly, one of the release's
// six new reflect tests. The construct appears in no earlier release. It made the whole `reflect`
// row unreadable: `go2cs -tests -test-action all` exits 1 at CONVERT in BOTH build configurations,
// so the row produced zero verdicts rather than a divergence anyone could read.
//
// ⚠ THE TWO SITES SHARE ONE SIGNATURE AND THE RESOLVER WARNS ONCE PER SIGNATURE, so the failure
// named only the first — `1 unresolved dynamic type(s) … all_test.cs(4201)` while all_test.cs(8451)
// carried the same raw text unmentioned. Anything validating this against the reported line alone
// would read green with half the defect standing; the guard's fixture carries two sites for exactly
// that reason.
//
// The lift is named "type", which is the fallback convStructType already passes from this very
// position — so the C# name this publishes is the name the converter was ALREADY minting and
// declaring, and nothing about the emitted declaration moves. Only its publication is new.
//
// Bounds, stated rather than quietly assumed:
//
//   - The EMPTY anonymous struct is excluded by extractStructType and needs no lift: it maps to
//     golib's shared EmptyStruct, which is why `TypeFor[struct{}]()` resolved on the broken
//     converter and is the control that makes this one axis rather than three.
//   - A bare generic INSTANTIATION that is not called (`var f = typeFor[struct{ f int }]`) does not
//     reach here. No corpus source carries that spelling — the stdlib census at 1.24.13 found the
//     shape only in call position — so it is left unhandled rather than covered speculatively.
//   - The anonymous-INTERFACE twin is the same gate and the same three-step resolution, and it is
//     NOT addressed here because no measurement has produced it. Named so the next reader knows it
//     was considered and excluded, not overlooked.
func (v *Visitor) liftExplicitAnonStructTypeArgs(callExpr *ast.CallExpr) {
	if callExpr == nil || callExpr.Fun == nil {
		return
	}

	for _, typeArg := range explicitCallTypeArgs(callExpr.Fun) {
		structType, exprType := v.extractStructType(typeArg)

		if structType == nil || v.liftedTypeExists(structType) {
			continue
		}

		// The same toggle the call-ARGUMENT pre-visit uses (convCallExpr's argument classification):
		// a written type argument is externally significant across function scopes in exactly the
		// sense liftAtCallBoundary's doc comment describes, so the lift publishes and two functions
		// writing the identical shape unify on one C# type instead of minting one each.
		v.liftAtCallBoundary = true
		v.indentLevel++
		v.visitStructType(structType, exprType, "type", nil, true, nil)
		v.indentLevel--
		v.liftAtCallBoundary = false
	}
}

// liftConversionTargetAnonStruct lifts -- and publishes, as liftExplicitAnonStructTypeArgs does -- a non-empty
// anonymous struct written in a CONVERSION's type, `q := (*struct{ Name string })(e)`, under the name a composite
// literal in the same position takes: the assigned variable's (visitAssignStmt passes it) or "type". A shape already
// lifted anywhere in the package is reused by signature (visitStructType's reuse block), so two conversions to one
// shape share one C# type.
//
// Without it the pointee had no lift at all: the conversion's emission named it through the deferred dynamic-type
// marker, which resolved to nothing and fell back to the raw Go signature, `Reinterpret<Employee, struct{Name
// string; Age int}>()`, which is not C# (the PointerConversionAnonymousTarget guard).
func (v *Visitor) liftConversionTargetAnonStruct(callExpr *ast.CallExpr, name string) {
	if callExpr == nil || callExpr.Fun == nil {
		return
	}

	if tv, isType := v.info.Types[callExpr.Fun]; !isType || !tv.IsType() {
		return
	}

	structType, exprType := v.extractStructType(callExpr.Fun)

	if structType == nil || v.liftedTypeExists(structType) {
		return
	}

	v.liftAtCallBoundary = true
	v.indentLevel++
	v.visitStructType(structType, exprType, name, nil, true, nil)
	v.indentLevel--
	v.liftAtCallBoundary = false
}
