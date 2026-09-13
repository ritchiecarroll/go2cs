// constraintProxyGenericCall_test.go - Gbtc
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
	"testing"
)

// The defect these tests lock in: the SELF-REFERENTIAL constraint proxy — the machinery that lets a
// Go pointer type serve as the C# type argument of an F-bounded constraint (`nistPoint[Point]`) —
// was reachable ONLY from an instantiated generic NAMED TYPE. constraintProxyArg took a
// *types.Named, so `crypto/elliptic`'s `nistCurve[*P224Point]` resolved to the proxy while a generic
// FUNCTION instantiated the same way did not: `benchmarkScalarMult[P nistPoint[P]]` called with
// `*P224Point` rendered the raw golib box, and `ж<P224Point>` can never NOMINALLY satisfy
// `where P : nistPoint<P>`.
//
// That was 16 × CS0311 in crypto/internal/nistec's test half — the whole 2,200-verdict package
// build-blocked, and crypto/ecdsa's 82 with it. The fix splits the (type parameter, type argument)
// core out of constraintProxyArg so a signature-shaped caller shares the F-bounded detection, then
// FORCES the explicit type-argument list at such a call: C# would otherwise infer the type argument
// from the argument's own type and infer exactly the box that cannot satisfy the bound.
//
// The second half is the delegate boundary. A `func() P` parameter renders as `Func<proxy>`, and a
// C# method-group conversion cannot apply the user-defined ж↔proxy conversion the proxy return
// needs (CS0407 on `testEquivalents(t, nistec.NewP224Point, …)`), so such an argument is re-wrapped
// as a lambda — the same remedy convCompositeLit already applied to a proxied struct's FUNC field.
//
// Negative controls carry the same weight as the positives here: the proxy is a real emission
// change, so every gate below that must NOT fire is a guard against churning unrelated generics.

// constraintProxyFixture is the nistec shape reduced to what the predicates read: a self-referential
// generic method-set interface, a concrete type whose methods are POINTER-receiver (so `*p224`
// satisfies `point[*p224]` structurally, the way Go does and C# cannot), a VALUE type that satisfies
// its own constraint nominally, and a plain non-self-referential constraint. The calls in driver()
// are the sites the predicates are asked about, one per line so they index unambiguously.
const constraintProxyFixture = `package proxy

// point is the self-referential (F-bounded) constraint — nistec's nistPoint[T].
type point[T any] interface {
	label() string
	combine(T) T
	restore([]byte) (T, error)
}

// named is a plain, NON-self-referential method-set constraint. A pointer argument widens to it, so
// it must never take a proxy.
type named interface {
	label() string
}

// p224's methods are POINTER-receiver: *p224 satisfies point[*p224].
type p224 struct{ v int }

func (p *p224) label() string                   { return "p224" }
func (p *p224) combine(o *p224) *p224           { return &p224{v: p.v + o.v} }
func (p *p224) restore(b []byte) (*p224, error) { return &p224{v: int(b[0])}, nil }

func newP224() *p224 { return &p224{v: 1} }

// vpoint's methods are VALUE-receiver: vpoint itself satisfies point[vpoint], nominally, with no
// box in the way and therefore no proxy.
type vpoint struct{ v int }

func (p vpoint) label() string                     { return "vpoint" }
func (p vpoint) combine(o vpoint) vpoint           { return vpoint{v: p.v + o.v} }
func (p vpoint) restore(b []byte) (vpoint, error)  { return vpoint{v: int(b[0])}, nil }

// benchPoint is the exact CS0311 shape: a generic FUNCTION whose type parameter carries the
// self-referential constraint, called with a value whose type drives inference.
func benchPoint[P point[P]](p P, n int) string { return p.label() }

// withFactory is the CS0407 shape: the proxied type parameter appears behind a FUNC parameter, so
// the delegate position carries the proxy and a method group cannot convert into it.
func withFactory[P point[P]](newPoint func() P, tag string) string { return newPoint().label() }

// withPlainFunc's FUNC parameter does NOT mention the proxied type parameter, so it must be left
// alone even though the CALL takes a proxy.
func withPlainFunc[P point[P]](p P, format func(string) string) string { return format(p.label()) }

// widenToNamed's constraint is not self-referential — a pointer argument widens to the interface.
func widenToNamed[N named](n N) string { return n.label() }

func pointerCall() string { return benchPoint(newP224(), 28) }

func valueCall() string { return benchPoint(vpoint{v: 1}, 28) }

func factoryCall() string { return withFactory(newP224, "tag") }

func plainFuncCall() string { return withPlainFunc(newP224(), func(s string) string { return s }) }

func widenCall() string { return widenToNamed(newP224()) }
`

// proxyFixture loads the fixture once and exposes each driver call by the name of the FUNCTION that
// encloses it — the fixture puts one call per enclosing func precisely so the two `benchPoint`
// instantiations (pointer and value) stay distinguishable.
type proxyFixture struct {
	visitor *Visitor
	calls   map[string]*ast.CallExpr
}

func loadProxyFixture(t *testing.T) proxyFixture {
	t.Helper()

	dir := t.TempDir()

	writeModuleFiles(t, dir, map[string]string{
		"go.mod":   "module example/proxy\n\ngo 1.23\n",
		"proxy.go": constraintProxyFixture,
	})

	// constraintProxyFor REGISTERS the (element, interface) pair it resolves so package_info emits
	// the ConstraintProxy record; a conversion run initializes that map per package
	// (packageStateOperations). Stand it up here and restore it, so these tests neither panic on a
	// nil map nor leak registrations into a sibling test.
	previousProxies := constraintProxies
	t.Cleanup(func() { constraintProxies = previousProxies })
	constraintProxies = make(map[string][2]string)

	production := loadProductionForDir(t, dir)
	fixture := proxyFixture{
		visitor: &Visitor{info: production.TypesInfo, pkg: production.Types},
		calls:   map[string]*ast.CallExpr{},
	}

	for _, file := range production.Syntax {
		for _, decl := range file.Decls {
			funcDecl, ok := decl.(*ast.FuncDecl)

			if !ok || funcDecl.Body == nil {
				continue
			}

			ast.Inspect(funcDecl.Body, func(node ast.Node) bool {
				callExpr, ok := node.(*ast.CallExpr)

				if !ok {
					return true
				}

				// The generic call is the OUTERMOST one in each wrapper (`benchPoint(newP224(), 28)`
				// encloses `newP224()`), so the first hit is the one under test.
				if _, exists := fixture.calls[funcDecl.Name.Name]; !exists {
					fixture.calls[funcDecl.Name.Name] = callExpr
				}

				return true
			})
		}
	}

	return fixture
}

// instance resolves the callee ident and the type arguments go/types recorded for the call inside
// the named wrapper function.
func (f proxyFixture) instance(t *testing.T, wrapper string) (*ast.Ident, *types.TypeList) {
	t.Helper()

	callExpr, ok := f.calls[wrapper]

	if !ok {
		t.Fatalf("fixture call inside %s was not found", wrapper)
	}

	funIdent := getCallFunIdent(callExpr.Fun)

	if funIdent == nil {
		t.Fatalf("call inside %s has no resolvable callee ident", wrapper)
	}

	instance, ok := f.visitor.info.Instances[funIdent]

	if !ok || instance.TypeArgs == nil {
		t.Fatalf("go/types recorded no instantiation for the call inside %s", wrapper)
	}

	return funIdent, instance.TypeArgs
}

// TestGenericCallResolvesSelfReferentialConstraintProxy is the direct regression: the type argument
// of a generic FUNCTION call must render as the proxy, not as the box.
func TestGenericCallResolvesSelfReferentialConstraintProxy(t *testing.T) {
	fixture := loadProxyFixture(t)
	funIdent, typeArgs := fixture.instance(t, "pointerCall")

	proxyName, ok := fixture.visitor.constraintProxySigArg(funIdent, typeArgs, 0)

	if !ok {
		t.Fatal("a pointer type argument against a self-referential constraint did not resolve a proxy — this is the CS0311 defect")
	}

	// The name must match ImplementGenerator's `elementType.Name + PointerPrefix +
	// interfaceDef.Name`, or the emitted reference names a proxy the generator never emits.
	want := "p224" + PointerPrefix + "point"

	if proxyName != want {
		t.Fatalf("proxy name = %q, want %q (must match ImplementGenerator's EmitConstraintProxy naming)", proxyName, want)
	}

	if !fixture.visitor.callNeedsConstraintProxy(funIdent, typeArgs) {
		t.Fatal("the call was not flagged as needing an EXPLICIT type-argument list; C# would infer the box and reject it (CS0311)")
	}

	// renderedTypeArgs is what convCallExpr actually emits.
	rendered := fixture.visitor.renderedTypeArgs(funIdent, typeArgs)

	if len(rendered) != 1 || rendered[0] != want {
		t.Fatalf("renderedTypeArgs = %v, want [%s]", rendered, want)
	}
}

// TestGenericCallProxyNegativeControls keeps the proxy scoped to the case that needs it. Both of
// these instantiate a generic function the same way; neither may take a proxy, or the change churns
// output it has no business touching.
func TestGenericCallProxyNegativeControls(t *testing.T) {
	fixture := loadProxyFixture(t)

	// A VALUE type satisfies the very same self-referential constraint NOMINALLY in C# — there is
	// no box in the way, so a proxy would change no error and only add churn.
	valueIdent, valueArgs := fixture.instance(t, "valueCall")

	if proxyName, ok := fixture.visitor.constraintProxySigArg(valueIdent, valueArgs, 0); ok {
		t.Fatalf("a VALUE type argument resolved proxy %q; only the boxed pointer needs one", proxyName)
	}

	if fixture.visitor.callNeedsConstraintProxy(valueIdent, valueArgs) {
		t.Fatal("a value type argument forced an explicit type-argument list")
	}

	// A POINTER argument whose constraint is NOT self-referential widens to the interface.
	namedIdent, namedArgs := fixture.instance(t, "widenCall")

	if proxyName, ok := fixture.visitor.constraintProxySigArg(namedIdent, namedArgs, 0); ok {
		t.Fatalf("a NON-self-referential constraint resolved proxy %q; a pointer widens to such an interface and must be left alone", proxyName)
	}

	if fixture.visitor.callNeedsConstraintProxy(namedIdent, namedArgs) {
		t.Fatal("a non-self-referential constraint forced an explicit type-argument list — unrelated generics must not churn")
	}
}

// TestProxiedFuncParameterIsLambdaWrapped covers the CS0407 half: only a FUNC parameter that
// actually MENTIONS the proxied type parameter gets the lambda re-wrap.
func TestProxiedFuncParameterIsLambdaWrapped(t *testing.T) {
	fixture := loadProxyFixture(t)
	funIdent, typeArgs := fixture.instance(t, "factoryCall")

	// Parameter 0 is `newPoint func() P` — proxied, niladic, so the lambda takes no parameters.
	params, ok := fixture.visitor.constraintProxyLambdaParams(funIdent, typeArgs, 0)

	if !ok {
		t.Fatal("a `func() P` parameter of a proxied call was not lambda-wrapped — a method group cannot convert to Func<proxy> (CS0407)")
	}

	if params != "" {
		t.Fatalf("lambda parameter list = %q, want \"\" for a niladic factory", params)
	}

	// Parameter 1 is `tag string` — not func-typed at all.
	if _, ok := fixture.visitor.constraintProxyLambdaParams(funIdent, typeArgs, 1); ok {
		t.Fatal("a non-func parameter was lambda-wrapped")
	}

	// withPlainFunc's parameter 1 is `format func(string) string` — func-typed, but it does not
	// mention the proxied type parameter, so its delegate carries no proxy and needs no re-wrap.
	plainIdent, plainArgs := fixture.instance(t, "plainFuncCall")

	if !fixture.visitor.callNeedsConstraintProxy(plainIdent, plainArgs) {
		t.Fatal("withPlainFunc's call should still need a proxy for its own type argument")
	}

	if _, ok := fixture.visitor.constraintProxyLambdaParams(plainIdent, plainArgs, 1); ok {
		t.Fatal("a func parameter that does NOT mention the proxied type parameter was lambda-wrapped — the re-wrap must follow the proxy, not the func-ness")
	}
}

// TestTypeMentionsTypeParamStructuralReach guards the predicate the lambda re-wrap depends on. The
// re-wrap must follow the proxy through the shapes a Go signature actually nests it in, and must
// terminate on a self-referential named type rather than recurse forever.
func TestTypeMentionsTypeParamStructuralReach(t *testing.T) {
	fixture := loadProxyFixture(t)
	funIdent, _ := fixture.instance(t, "factoryCall")

	typeParams := fixture.visitor.signatureTypeParams(funIdent)

	if typeParams == nil || typeParams.Len() != 1 {
		t.Fatalf("expected withFactory to declare one type parameter, got %v", typeParams)
	}

	target := typeParams.At(0)
	funcObj := fixture.visitor.info.ObjectOf(funIdent).(*types.Func)
	signature := funcObj.Type().(*types.Signature)

	// `newPoint func() P` — the proxied parameter, reached through a nested signature RESULT.
	if !typeMentionsTypeParam(signature.Params().At(0).Type(), target, map[types.Type]bool{}) {
		t.Fatal("typeMentionsTypeParam missed the type parameter behind a func result")
	}

	// `tag string` — no mention.
	if typeMentionsTypeParam(signature.Params().At(1).Type(), target, map[types.Type]bool{}) {
		t.Fatal("typeMentionsTypeParam reported a mention in a plain string parameter")
	}

	// Structural reach: pointer, slice, map value and tuple all carry the parameter.
	for name, typ := range map[string]types.Type{
		"pointer": types.NewPointer(target),
		"slice":   types.NewSlice(target),
		"map":     types.NewMap(types.Typ[types.String], target),
		"chan":    types.NewChan(types.SendRecv, target),
		"array":   types.NewArray(target, 4),
	} {
		if !typeMentionsTypeParam(typ, target, map[types.Type]bool{}) {
			t.Fatalf("typeMentionsTypeParam missed the type parameter through a %s", name)
		}
	}

	// And a type that does not mention it stays negative through the same shapes.
	if typeMentionsTypeParam(types.NewSlice(types.Typ[types.Byte]), target, map[types.Type]bool{}) {
		t.Fatal("typeMentionsTypeParam reported a mention in []byte")
	}
}
