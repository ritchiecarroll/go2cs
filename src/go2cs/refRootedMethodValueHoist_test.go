// refRootedMethodValueHoist_test.go - Gbtc
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
	"strings"
	"testing"

	"github.com/ritchiecarroll/hashset"
)

// RED 11. A concrete VALUE-receiver method VALUE in a call ARGUMENT mints a wrapping lambda
// (`() => recv.M()`), and convSelectorExpr's argument arm had exactly two ways to avoid rendering the
// receiver LIVE inside that lambda: a bare-IDENT receiver (snapshot) and a POINTER receiver expression
// auto-deref'd to a value receiver (hoistReceiverTemp). A receiver expression that is a CHAIN rooting at
// a ref-lowered ident matched neither, so it fell through to convExprInLambdaContext and rendered the
// ref alias into the lambda body: crypto/tls's `hs.suite.hash.New` emitted
// `() => (~hs.suite).hash.New()` inside a `[GoRecv] this ref serverHandshakeStateTLS13 hs` method —
// capturing a ref local, which is CS1628, nine times across handshake_{client,server}_tls13.
//
// The cut engages the SAME hoist for that shape: the receiver chain is evaluated once into a
// statement-level temp, which is also what Go's method-value semantics require ("the receiver is
// evaluated and saved" at evaluation, not at call).
const refRootedMethodValueFixture = `package refrooted

type digest struct{ n int }

// New is the VALUE-receiver method whose method VALUE is taken below.
func (d digest) New() int { return d.n }

type suite struct{ hash digest }

type state struct {
	suite *suite
	own   digest
}

func use(f func() int) int { return f() }

// chain is THE CLASS: the method value's receiver expression is a CHAIN (` + "`hs.suite.hash`" + `) whose
// ROOT is the ref-lowered receiver. Rendered live, the lambda captures ` + "`hs`" + ` -- CS1628.
func (hs *state) chain() int {
	return use(hs.suite.hash.New)
}

// bare is the BARE-IDENT arm: a plain identifier receiver of value type, which the existing snapshot
// arm already copies. It must stay a COPY and must NOT acquire a receiver temp.
func (hs *state) bare(d digest) int {
	return use(d.New)
}

// localRoot is the BOUNDING NEGATIVE: the same chain shape rooted at a pointer LOCAL, which holds its
// box directly and is perfectly capturable. Nothing here is broken, so nothing may move.
func localRoot() int {
	s := &suite{}
	return use(s.hash.New)
}
`

// loadRefRootedFixture returns a Visitor over the fixture plus, per method name, the `use(...)` call
// whose single argument is the method value under test.
func loadRefRootedFixture(t *testing.T) (*Visitor, map[string]*ast.CallExpr, map[string]*ast.FuncDecl) {
	t.Helper()

	dir := t.TempDir()

	writeModuleFiles(t, dir, map[string]string{
		"go.mod":       "module example/refrooted\n\ngo 1.24\n",
		"refrooted.go": refRootedMethodValueFixture,
	})

	production := loadProductionForDir(t, dir)
	// fset is not optional here even though RED 10's assignment-path harness did without it: the
	// argument path runs through convExprList, whose isLineFeedBetween reads positions.
	visitor := &Visitor{fset: production.Fset, info: production.TypesInfo, pkg: production.Types, newline: "\n"}
	calls := map[string]*ast.CallExpr{}
	decls := map[string]*ast.FuncDecl{}

	for _, file := range production.Syntax {
		for _, decl := range file.Decls {
			funcDecl, ok := decl.(*ast.FuncDecl)

			if !ok || funcDecl.Body == nil {
				continue
			}

			decls[funcDecl.Name.Name] = funcDecl

			ast.Inspect(funcDecl.Body, func(node ast.Node) bool {
				call, isCall := node.(*ast.CallExpr)

				if !isCall {
					return true
				}

				// The `use(...)` call itself, not a nested one: its callee is the plain ident `use`.
				if ident, isIdent := call.Fun.(*ast.Ident); isIdent && ident.Name == "use" {
					if _, exists := calls[funcDecl.Name.Name]; !exists {
						calls[funcDecl.Name.Name] = call
					}
				}

				return true
			})
		}
	}

	return visitor, calls, decls
}

// withRefRootedFixtureParams installs the per-method state visitFuncDecl establishes for a real
// conversion. Deliberately NOT named withFixtureParams: RED 10's nested-map arms define a helper by
// that name in this same package, and the two seats merge in chain -- a shared helper name would build
// in each worktree alone and fail only once both had landed.
func withRefRootedFixtureParams(t *testing.T, visitor *Visitor, funcDecl *ast.FuncDecl) {
	t.Helper()

	visitor.paramNames = hashset.HashSet[string]{}
	visitor.paramObjects = map[types.Object]bool{}

	for _, field := range funcDecl.Type.Params.List {
		for _, name := range field.Names {
			visitor.paramNames.Add(name.Name)

			if obj := visitor.info.ObjectOf(name); obj != nil {
				visitor.paramObjects[obj] = true
			}
		}
	}

	// isPointerReceiver reads all three, and the whole rendering under test turns on whether the
	// chain's root resolves to THIS method's receiver.
	visitor.inFunction = true
	visitor.currentFuncDecl = funcDecl

	if obj, isFunc := visitor.info.ObjectOf(funcDecl.Name).(*types.Func); isFunc {
		if sig, isSig := obj.Type().(*types.Signature); isSig {
			visitor.currentFuncSignature = sig
		}
	}
}

// emitRefRootedMethodValue converts the `use(<method value>)` call and returns the rendered call text
// together with whatever was written to the statement-level hoist sink, which is where a receiver temp's
// declaration lands. Both halves are read: the lambda body alone cannot distinguish a hoist that
// declared its temp from one that named a temp nothing declares (CS0103).
func emitRefRootedMethodValue(t *testing.T, visitor *Visitor, call *ast.CallExpr, funcDecl *ast.FuncDecl) (string, string) {
	t.Helper()

	withRefRootedFixtureParams(t, visitor, funcDecl)

	hoisted := &strings.Builder{}
	visitor.hoistedDecls = hoisted
	visitor.lambdaCapture = newLambdaCapture()
	visitor.capturedVarCount = map[string]int{}
	visitor.indentLevel = 1
	visitor.options = Options{indentSpaces: 4}

	return visitor.convExpr(call, nil), hoisted.String()
}

// THE CLASS, red-first: the receiver chain roots at the ref-lowered receiver, so it must be evaluated
// ONCE into a statement-level temp and the lambda must name that temp. Against the unfixed converter the
// lambda body carries the live chain and captures `hs`, which is the nine sites' CS1628.
func TestRefRootedReceiverChainHoistsIntoATemp(t *testing.T) {
	visitor, calls, decls := loadRefRootedFixture(t)

	call, ok := calls["chain"]

	if !ok {
		t.Fatal("fixture call inside chain was not found")
	}

	got, hoisted := emitRefRootedMethodValue(t, visitor, call, decls["chain"])
	tempName := receiverTempPrefix + CapturedVarMarker + "1"

	if !strings.Contains(got, "() => "+tempName+".New()") {
		t.Fatalf("a receiver chain rooting at a ref-lowered ident must be hoisted:\n got %q\nwant a lambda over %q\n(the live rendering captures the ref receiver -- CS1628, crypto/tls ×9)",
			got, tempName)
	}

	if !strings.Contains(hoisted, "var "+tempName+" = ") {
		t.Fatalf("the hoisted temp must also be DECLARED, else the lambda names nothing (CS0103):\nhoist sink was %q", hoisted)
	}

	// The temp holds the receiver VALUE, evaluated outside the lambda -- Go saves the receiver when the
	// method value is evaluated. A temp initialized from anything still mentioning the lambda would put
	// the capture back.
	if !strings.Contains(hoisted, "(~hs.suite).hash") {
		t.Fatalf("the temp must be initialized from the receiver chain rendered OUTSIDE lambda context:\nhoist sink was %q", hoisted)
	}
}

// THE BARE-IDENT ARM, asserted as a COPY: an ident receiver already gets a once-evaluated snapshot, and
// hoisting it again would emit a second copy of a single evaluation. This arm must be GREEN both before
// and after the cut -- it is what bounds the widening.
func TestBareIdentReceiverStaysACopy(t *testing.T) {
	visitor, calls, decls := loadRefRootedFixture(t)

	call, ok := calls["bare"]

	if !ok {
		t.Fatal("fixture call inside bare was not found")
	}

	got, hoisted := emitRefRootedMethodValue(t, visitor, call, decls["bare"])
	snapshot := "d" + CapturedVarMarker + "1"

	if !strings.Contains(got, "() => "+snapshot+".New()") {
		t.Fatalf("a bare-ident receiver must keep its snapshot COPY:\n got %q\nwant a lambda over %q", got, snapshot)
	}

	if !strings.Contains(hoisted, "var "+snapshot+" = d;") {
		t.Fatalf("the snapshot must copy the receiver itself:\nhoist sink was %q", hoisted)
	}

	if strings.Contains(got, receiverTempPrefix+CapturedVarMarker) || strings.Contains(hoisted, receiverTempPrefix+CapturedVarMarker) {
		t.Fatalf("the bare-ident arm must NOT acquire a receiver temp -- that is a second copy of one evaluation:\n got %q\nhoist %q", got, hoisted)
	}
}

// THE BOUNDING NEGATIVE: the identical chain SHAPE rooted at a pointer LOCAL. The local holds its box
// directly and is capturable, so the live rendering compiles and must not move. This arm is what keeps
// the widening keyed on the ROOT being ref-lowered rather than on "the receiver is a chain".
func TestLocalRootedReceiverChainIsNotHoisted(t *testing.T) {
	visitor, calls, decls := loadRefRootedFixture(t)

	call, ok := calls["localRoot"]

	if !ok {
		t.Fatal("fixture call inside localRoot was not found")
	}

	got, hoisted := emitRefRootedMethodValue(t, visitor, call, decls["localRoot"])

	if strings.Contains(got, receiverTempPrefix+CapturedVarMarker) || strings.Contains(hoisted, receiverTempPrefix+CapturedVarMarker) {
		t.Fatalf("a chain rooted at a pointer LOCAL is capturable and must not be hoisted:\n got %q\nhoist %q", got, hoisted)
	}
}
