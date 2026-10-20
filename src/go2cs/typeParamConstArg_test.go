// typeParamConstArg_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/ast"
	"strings"
	"testing"
)

// RED 12 (COORD 26e86351e, from i9's rung-7 census fd8b949b8). An untyped numeric CONSTANT bound to a
// TYPE-PARAMETER parameter of an INFERRED generic call is the untyped-nil defect of RED 2 reached by a
// different argument kind, and it fails the same way: CS0411.
//
// THE MECHANISM, derived from the type declarations rather than from the error text. A `ref T`
// parameter contributes an EXACT inference bound, so T's candidate set is {the ref argument's type,
// the literal's type} and the exact bound deletes the literal's. The survivor then stands only if every
// remaining bound converts to it IMPLICITLY — and `int` -> `System.UInt32` is the one conversion C# does
// not provide. No candidate survives fixing, so INFERENCE ITSELF fails rather than the argument being
// rejected, which is why the code is CS0411 and not CS1503. net/http emitted
// `http2setDefault(ref …MaxConcurrentStreams, 1, math.MaxUint32, http2defaultMaxStreams)` and failed at
// h2_bundle.cs (869,5) (870,5) (871,5).
//
// WHY THE SIBLINGS IN THAT SAME FILE COMPILE, which is what makes the failing set an INTERSECTION rather
// than a property of the function: `int32` IS `System.Int32` (GlobalUsings), so the literal matches by
// IDENTITY; `time.Duration` is a `[GoType("num:int64")]` wrapper whose generated implicit operator
// composes with the standard `int` -> `int64`; and the one call carrying no inline literal never needed
// anything. Unsigned receiver AND inline constant — both, or nothing happens.
//
// Go itself has no trouble: it converts the untyped constant to T, so go/types RECORDED the
// instantiation. That recording is the cure — emit the constant at its recorded type — and
// instantiatedParamType is the accessor RED 2 already proved.
//
// ⚠ THESE ARMS DRIVE THE EMISSION, NOT THE HELPERS. RED 2's own arms assert
// declaredIsTypeParam/instantiatedParamType, every one of which passes against the UNFIXED converter —
// that file locks in facts, it does not bound a branch. These call convExpr and read the rendered call,
// so reverting the cut turns the first one red and leaves the other three green.
const typeParamConstFixture = `package tpconst

type conf struct {
	U uint32
	I int32
	N int
}

var c conf

// setDefault is THE SHAPE: T is bound EXACTLY by a POINTER parameter and ALSO taken BY VALUE, so C#
// pins T from the pointer argument and every other argument becomes a conversion test.
func setDefault[T ~int | ~int32 | ~uint32 | ~int64](v *T, minval, maxval, defval T) {}

func takesU(x uint32) {}

// unsigned is THE FAILING CLASS.
func unsigned() { setDefault(&c.U, 1, 2, 3) }

// explicitU is BOUND 1: an EXPLICIT instantiation. Its signature is already instantiated, so the
// parameter is no longer a type parameter and the rule must never reach it.
func explicitU() { setDefault[uint32](&c.U, 1, 2, 3) }

// plain is BOUND 2: a NON-GENERIC callee. It compiles today through C#'s constant-expression
// conversion, and nothing about it is a type-parameter position.
func plain() { takesU(1) }

// intDefault is the NARROWING bound: T instantiates to the constant's OWN default type, so the cast is
// pure churn and the rule declines it.
func intDefault() { setDefault(&c.N, 1, 2, 3) }
`

// loadTypeParamConstFixture returns a Visitor over the fixture plus, per wrapper function, the call
// under test. Named for this seat: the package already carries loadTypeParamNilFixture (RED 2) and two
// other fixture loaders, and a shared name would build in each worktree alone and break once merged.
func loadTypeParamConstFixture(t *testing.T) (*Visitor, map[string]*ast.CallExpr) {
	t.Helper()

	dir := t.TempDir()

	writeModuleFiles(t, dir, map[string]string{
		"go.mod":     "module example/tpconst\n\ngo 1.24\n",
		"tpconst.go": typeParamConstFixture,
	})

	production := loadProductionForDir(t, dir)
	// fset is required: the argument path runs through convExprList, whose isLineFeedBetween reads
	// positions. RED 11 paid for learning that.
	visitor := &Visitor{fset: production.Fset, info: production.TypesInfo, pkg: production.Types, newline: "\n"}
	calls := map[string]*ast.CallExpr{}

	for _, file := range production.Syntax {
		for _, decl := range file.Decls {
			funcDecl, ok := decl.(*ast.FuncDecl)

			if !ok || funcDecl.Body == nil {
				continue
			}

			ast.Inspect(funcDecl.Body, func(node ast.Node) bool {
				if callExpr, isCall := node.(*ast.CallExpr); isCall {
					if _, exists := calls[funcDecl.Name.Name]; !exists {
						calls[funcDecl.Name.Name] = callExpr
					}
				}

				return true
			})
		}
	}

	return visitor, calls
}

// emitTypeParamConstCall converts the wrapper's call and returns the rendered text.
func emitTypeParamConstCall(t *testing.T, visitor *Visitor, calls map[string]*ast.CallExpr, wrapper string) string {
	t.Helper()

	call, ok := calls[wrapper]

	if !ok {
		t.Fatalf("fixture call inside %s was not found", wrapper)
	}

	hoisted := &strings.Builder{}
	visitor.hoistedDecls = hoisted
	visitor.lambdaCapture = newLambdaCapture()
	visitor.capturedVarCount = map[string]int{}
	visitor.indentLevel = 1
	visitor.options = Options{indentSpaces: 4}

	return visitor.convExpr(call, nil)
}

// THE CLASS, red-first. Each untyped constant in a type-parameter position must render AT THE RECORDED
// TYPE. The expected strings are CONCRETE rather than "contains a cast": the rendered form is
// `(type)(value)` (convExprList), and a containment check on the bare type name would also pass on a
// cast applied to the wrong argument or in the wrong shape.
func TestUntypedConstArgInTypeParamPositionTakesTheInstantiatedCast(t *testing.T) {
	visitor, calls := loadTypeParamConstFixture(t)
	got := emitTypeParamConstCall(t, visitor, calls, "unsigned")

	for _, want := range []string{"(uint32)1", "(uint32)2", "(uint32)3"} {
		if !strings.Contains(got, want) {
			t.Fatalf("an untyped constant bound to a type parameter inferred as uint32 must be emitted at that recorded type:\n got %q\nwant it to contain %q\n(a bare C# int literal has NO implicit conversion to System.UInt32, so type inference itself fails -- CS0411, net/http h2_bundle ×3)",
				got, want)
		}
	}
}

// requireRenderedCall is the ANTI-VACUITY guard the negative arms below need and the positive arm does
// not. A `!strings.Contains(...)` assertion passes just as happily on an empty string, on a render that
// silently lost its arguments, or on a fixture function that stopped being found — so every negative
// arm first proves it is looking at the call it claims to bound. RED 6's min/max arms carry the same
// guard in its other form ("VACUOUS: n of m fixture functions reached"), for the same reason.
func requireRenderedCall(t *testing.T, got string, callee string) {
	t.Helper()

	if !strings.Contains(got, callee) {
		t.Fatalf("VACUOUS: the render does not contain the callee %q, so a negative assertion over it proves nothing:\n got %q", callee, got)
	}

	if !strings.Contains(got, "1") {
		t.Fatalf("VACUOUS: the render carries no constant argument, so 'it was not cast' is not a reading:\n got %q", got)
	}
}

// BOUND 1 (COORD's first): an EXPLICIT instantiation already binds its type arguments, so the rule must
// not fire. This arm is GREEN on both sides of the cut and is what keeps the widening keyed on the
// parameter being a type parameter rather than on "the callee is generic".
func TestExplicitInstantiationTakesNoConstCast(t *testing.T) {
	visitor, calls := loadTypeParamConstFixture(t)
	got := emitTypeParamConstCall(t, visitor, calls, "explicitU")

	requireRenderedCall(t, got, "setDefault")

	if strings.Contains(got, "(uint32)1") {
		t.Fatalf("an EXPLICIT instantiation needs no inference and must not acquire a cast:\n got %q", got)
	}
}

// BOUND 2 (COORD's second): a NON-GENERIC callee. Nothing here is a type-parameter position, and the
// call already compiles through C#'s constant-expression conversion.
func TestNonGenericCallTakesNoConstCast(t *testing.T) {
	visitor, calls := loadTypeParamConstFixture(t)
	got := emitTypeParamConstCall(t, visitor, calls, "plain")

	requireRenderedCall(t, got, "takesU")

	if strings.Contains(got, "(uint32)1") {
		t.Fatalf("a non-generic parameter is not a type-parameter position and must not acquire a cast:\n got %q", got)
	}
}

// THE NARROWING BOUND — mine, not one of the two COORD named, and stated as such. The cut declines the
// cast when the instantiated type IS the constant's own default type, because such a cast changes no
// binding and only churns the golden. Without this arm the anti-churn half of the design has no guard,
// and a later widening could re-introduce corpus-wide noise with every test still green.
func TestConstArgMatchingItsDefaultTypeTakesNoCast(t *testing.T) {
	visitor, calls := loadTypeParamConstFixture(t)
	got := emitTypeParamConstCall(t, visitor, calls, "intDefault")

	requireRenderedCall(t, got, "setDefault")

	if strings.Contains(got, "(nint)1") || strings.Contains(got, "(nint)(1)") {
		t.Fatalf("a type parameter inferred as the constant's OWN default type needs no cast -- applying one is pure churn:\n got %q", got)
	}
}
