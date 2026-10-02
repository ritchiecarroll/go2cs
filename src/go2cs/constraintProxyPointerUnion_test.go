// constraintProxyPointerUnion_test.go - Gbtc
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
	"io"
	"os"
	"path/filepath"
	"strings"
	"testing"

	"github.com/ritchiecarroll/hashset"
)

// The defect this locks in (H7 RED 8, C2 sizing 0d6cd77a2, COORD 7bc9d58d4): Go 1.24 moved crypto/elliptic's
// nistec generics into crypto/internal/fips140/ecdh and ecdsa and restated their self-referential constraint
// `Point[P]` with a union of pointer terms beside the methods. IsMethodSet() answers false over that union, so
// the constraint proxy refused it AND the declaration took the composite-union arm, rendering
// `where P : /* Point[P] */ new()` -- which neither the box nor the proxy (it has no parameterless constructor)
// satisfies: CS0310 at every instantiation. The union is emitted as a comment and the interface as a pure method
// set, so admitting it on BOTH sides -- the proxy and the declaration, on one predicate -- is exactly the
// nistPoint treatment.
//
// The converter had been naming the defect all along: getGenericDefinition WARNS on stderr for a pointer-carrying
// constraint it does not erase, one line per site (29 for the 29 elided declarations, crypto/elliptic 0). The same
// predicate silences it for the admitted shape, so the warning count is the third assertion (COORD 58e963bed).
//
// The fixture carries the fips140 shape, the elliptic shape as the control, and one constraint per way the
// predicate must still refuse.
const pointerUnionFixture = `package pointerunion

type P1 struct{ v int }
type P2 struct{ v int }

func (p *P1) Bytes() []byte    { return nil }
func (p *P1) Add(a, b *P1) *P1 { return p }
func (p *P2) Bytes() []byte    { return nil }
func (p *P2) Add(a, b *P2) *P2 { return p }

type V1 struct{ v int }
type V2 struct{ v int }

func (V1) Bytes() []byte { return nil }
func (V2) Bytes() []byte { return nil }

// unionPoint is fips140's shape: a pointer-to-named union beside the methods.
type unionPoint[P any] interface {
	*P1 | *P2
	Bytes() []byte
	Add(P, P) P
}

// methPoint is crypto/elliptic's shape: the methods alone.
type methPoint[T any] interface {
	Bytes() []byte
	Add(T, T) T
}

// tildePoint approximates its pointer terms.
type tildePoint[P any] interface {
	~*P1 | ~*P2
	Bytes() []byte
}

// valuePoint's terms are values, not pointers.
type valuePoint[P any] interface {
	V1 | V2
	Bytes() []byte
}

// unnamedPoint's term points at an unnamed type.
type unnamedPoint[P any] interface {
	*P1 | *struct{ v int }
	Bytes() []byte
}

// embedPoint embeds an interface beside its union.
type embedPoint[P any] interface {
	methPoint[P]
	*P1 | *P2
}

// ptrKeySet is runtime's mapBenchmarkKeyType: a METHODLESS union mixing value terms with ONE
// pointer term. The value terms instantiate fine; the pointer term instantiates at the abstract
// box class, which a parameterless-constructor constraint cannot admit (CS0310 x22, runtime row).
type ptrKeySet interface {
	int32 | int64 | string | V1 | *int32
}

// ptrElemSet is runtime's mapBenchmarkElemType: a union whose FIRST TERM is the named set above,
// so its pointer is one level further in. This is the half a term-only pointer test cannot see.
type ptrElemSet interface {
	ptrKeySet | []int32
}

// valueKeySet is the same methodless shape with NO pointer anywhere -- the composite-union arm's
// own case, which must keep its new() clause.
type valueKeySet interface {
	int32 | int64 | string | V1
}

type ptrKeyCurve[K ptrKeySet] struct{ k K }
type ptrElemCurve[E ptrElemSet] struct{ e E }
type valueKeyCurve[K valueKeySet] struct{ k K }

type unionCurve[P unionPoint[P]] struct{ newPoint func() P }
type methCurve[P methPoint[P]] struct{ newPoint func() P }
type tildeCurve[P tildePoint[P]] struct{ newPoint func() P }
type valueCurve[P valuePoint[P]] struct{ newPoint func() P }
type unnamedCurve[P unnamedPoint[P]] struct{ newPoint func() P }
type embedCurve[P embedPoint[P]] struct{ newPoint func() P }

func NewP1() *P1 { return &P1{} }

func unionUse() *unionCurve[*P1] { return &unionCurve[*P1]{newPoint: NewP1} }
func methUse() *methCurve[*P1]   { return &methCurve[*P1]{newPoint: NewP1} }
`

func loadPointerUnionFixture(t *testing.T) (*Visitor, *types.Scope) {
	t.Helper()

	dir := t.TempDir()

	writeModuleFiles(t, dir, map[string]string{
		"go.mod":          "module example/pointerunion\n\ngo 1.24\n",
		"pointerunion.go": pointerUnionFixture,
	})

	// constraintProxyFor registers what it resolves; stand the map up and restore it so these tests neither
	// panic on a nil map nor leak into a sibling test.
	previousProxies := constraintProxies
	t.Cleanup(func() { constraintProxies = previousProxies })
	constraintProxies = make(map[string][2]string)

	production := loadProductionForDir(t, dir)

	return &Visitor{info: production.TypesInfo, pkg: production.Types, newline: "\n"}, production.Types.Scope()
}

func pointerUnionNamed(t *testing.T, scope *types.Scope, name string) *types.Named {
	t.Helper()

	obj := scope.Lookup(name)

	if obj == nil {
		t.Fatalf("fixture has no %s", name)
	}

	named, ok := obj.Type().(*types.Named)

	if !ok {
		t.Fatalf("fixture %s is %T, not a named type", name, obj.Type())
	}

	return named
}

func TestIsMethodSetWithPointerNamedUnion(t *testing.T) {
	_, scope := loadPointerUnionFixture(t)

	for name, want := range map[string]bool{
		"unionPoint":   true,  // fips140's Point[P]
		"methPoint":    false, // no union: IsMethodSet already admits it, this predicate need not
		"tildePoint":   false, // an approximate term
		"valuePoint":   false, // value terms
		"unnamedPoint": false, // a pointer to an unnamed type
		"embedPoint":   false, // an embedded interface beside the union
	} {
		iface := pointerUnionNamed(t, scope, name).Underlying().(*types.Interface)

		if got := isMethodSetWithPointerNamedUnion(iface); got != want {
			t.Errorf("isMethodSetWithPointerNamedUnion(%s) = %v, want %v", name, got, want)
		}
	}
}

func TestConstraintProxyPointerUnion(t *testing.T) {
	visitor, scope := loadPointerUnionFixture(t)
	p1 := types.NewPointer(pointerUnionNamed(t, scope, "P1"))

	for curve, want := range map[string]string{
		"unionCurve":   "P1жunionPoint", // the cure
		"methCurve":    "P1жmethPoint",  // the elliptic control, unchanged
		"tildeCurve":   "",
		"valueCurve":   "",
		"unnamedCurve": "",
		"embedCurve":   "",
	} {
		typeParam := pointerUnionNamed(t, scope, curve).TypeParams().At(0)
		got, ok := visitor.constraintProxyFor(typeParam, p1)

		if got != want || ok != (want != "") {
			t.Errorf("constraintProxyFor(%s's P, *P1) = %q, %v; want %q, %v", curve, got, ok, want, want != "")
		}
	}
}

// TestPointerUnionDeclarationAgreesWithProxy is the half the sizing did not name: the declaration is decided in
// getGenericDefinition, not by the proxy gate, and admitting the proxy without it leaves `new()` standing.
func TestPointerUnionDeclarationAgreesWithProxy(t *testing.T) {
	visitor, scope := loadPointerUnionFixture(t)

	for curve, want := range map[string]string{
		"unionCurve": "where P : unionPoint<P>",
		"methCurve":  "where P : methPoint<P>",
		// Still refused: the composite-union arm keeps its breadcrumb and `new()`.
		"valueCurve": "where P : /* valuePoint[P] */ new()",

		// ⚠ A METHODLESS union with a POINTER term emits NO constraint at all. `new()` would be
		// CS0310 against the abstract `ж<T>` the pointer term instantiates at, and the breadcrumb
		// cannot ride alone (`where K : /* … */` is a clause with no constraint). This is the
		// `comparable` arm's answer, reached for that arm's own reason.
		"ptrKeyCurve": "",

		// ⚠ …and through a union TERM that is itself a named set. A term-only pointer test answers
		// FALSE here and left 14 of the runtime row's 28 sites standing.
		"ptrElemCurve": "",

		// THE CONTROL for both: the same methodless shape with no pointer anywhere KEEPS `new()`.
		// Without it the two arms above pass for a change that drops the clause everywhere.
		"valueKeyCurve": "where K : /* valueKeySet */ new()",
	} {
		_, constraints := visitor.getGenericDefinition(pointerUnionNamed(t, scope, curve))
		got := strings.TrimSpace(constraints)

		if got != want {
			t.Errorf("getGenericDefinition(%s) constraints = %q, want %q", curve, got, want)
		}
	}
}

// pointerConstraintWarning is the text of getGenericDefinition's warning for a pointer-carrying constraint it does not
// erase -- the diagnostic that reported RED 8 at every site.
const pointerConstraintWarning = "approximate/union/method-carrying pointer constraint"

// captureStderr runs fn with os.Stderr redirected to a pipe and returns what it wrote; showWarning writes there.
func captureStderr(t *testing.T, fn func()) string {
	t.Helper()

	reader, writer, err := os.Pipe()

	if err != nil {
		t.Fatalf("os.Pipe: %v", err)
	}

	saved := os.Stderr
	os.Stderr = writer

	func() {
		defer func() { os.Stderr = saved }()
		fn()
	}()

	writer.Close()
	captured, _ := io.ReadAll(reader)
	reader.Close()

	return string(captured)
}

// TestPointerUnionWarningFallsSilent asserts the warning count per fixture constraint: silenced for the admitted
// shape, and STILL raised for every pointer-carrying shape the predicate refuses -- the narrowing must not hide them.
func TestPointerUnionWarningFallsSilent(t *testing.T) {
	visitor, scope := loadPointerUnionFixture(t)

	for curve, want := range map[string]int{
		"unionCurve":   0, // the cure: declared and proxied on purpose
		"tildeCurve":   1, // refused, still warned
		"unnamedCurve": 1,
		"embedCurve":   1,
		"methCurve":    0, // no pointer term: never warned
		"valueCurve":   0,
	} {
		named := pointerUnionNamed(t, scope, curve)
		stderr := captureStderr(t, func() { visitor.getGenericDefinition(named) })

		if got := strings.Count(stderr, pointerConstraintWarning); got != want {
			t.Errorf("getGenericDefinition(%s) raised %d pointer-constraint warning(s), want %d; stderr: %q", curve, got, want, stderr)
		}
	}
}

// The CROSS-PACKAGE half (RED 8 g2a/g2b, COORD a4eb648a6): crypto/ecdsa instantiates crypto/internal/fips140/ecdsa's
// self-referential constraint with *nistec.P224Point. The package that DECLARES the interface owns the proxy; a consumer must
// name that proxy through the interface package's C# qualifier and record none of its own. A consumer record minted a second
// class of the same simple name (CS1503 x16 against the owner's `P224()`), and spelled the interface by its Go import path.
const crossPackageProxyFixture = `package proxycross

import "example/proxycross/curves"

func consumerCall() int { return curves.Use(curves.NewCurve()) }
`

const crossPackageProxyCurves = `package curves

type P1 struct{ v int }

func (p *P1) Bytes() []byte    { return nil }
func (p *P1) Add(a, b *P1) *P1 { return p }

type P2 struct{ v int }

func (p *P2) Bytes() []byte    { return nil }
func (p *P2) Add(a, b *P2) *P2 { return p }

type Point[P any] interface {
	*P1 | *P2
	Bytes() []byte
	Add(P, P) P
}

type Curve[P Point[P]] struct{ newPoint func() P }

func newP1() *P1 { return &P1{} }

// NewCurve is the owner closing the constraint over *P1 in a PUBLIC signature, as fips140/ecdsa's P224() does.
func NewCurve() *Curve[*P1] { return &Curve[*P1]{newPoint: newP1} }

func Use[P Point[P]](c *Curve[P]) int { return 0 }

func ownerCall() int { return Use(NewCurve()) }
`

// The THIRD package: it hands out the owner's curve and a generic over the owner's constraint, so a file can instantiate that
// constraint without importing the package that declares it.
const crossPackageProxyMid = `package mid

import "example/proxycross/curves"

func Make() *curves.Curve[*curves.P1] { return curves.NewCurve() }

func Run[P curves.Point[P]](c *curves.Curve[P]) int { return curves.Use(c) }
`

// A consumer FILE that imports only mid: the proxy's qualifier cannot be the curves alias, which this file never declares.
const crossPackageProxyIndirect = `package proxycross

import "example/proxycross/mid"

func indirectCall() int { return mid.Run(mid.Make()) }
`

func loadCrossPackageProxyFixture(t *testing.T) (owner *Visitor, consumer *Visitor, ownerCalls, consumerCalls map[string]*ast.CallExpr) {
	t.Helper()

	dir := t.TempDir()

	writeModuleFiles(t, dir, map[string]string{
		"go.mod":           "module example/proxycross\n\ngo 1.24\n",
		"proxycross.go":    crossPackageProxyFixture,
		"indirect.go":      crossPackageProxyIndirect,
		"curves/curves.go": crossPackageProxyCurves,
		"mid/mid.go":       crossPackageProxyMid,
	})

	previousProxies := constraintProxies
	t.Cleanup(func() { constraintProxies = previousProxies })

	collect := func(syntax []*ast.File) map[string]*ast.CallExpr {
		calls := map[string]*ast.CallExpr{}

		for _, file := range syntax {
			for _, decl := range file.Decls {
				funcDecl, ok := decl.(*ast.FuncDecl)

				if !ok || funcDecl.Body == nil {
					continue
				}

				ast.Inspect(funcDecl.Body, func(node ast.Node) bool {
					if callExpr, ok := node.(*ast.CallExpr); ok {
						if _, exists := calls[funcDecl.Name.Name]; !exists {
							calls[funcDecl.Name.Name] = callExpr
						}
					}

					return true
				})
			}
		}

		return calls
	}

	ownerPkg := loadProductionForDir(t, filepath.Join(dir, "curves"))
	consumerPkg := loadProductionForDir(t, dir)

	// referencedForeignPackages and importQueue as the package state initializes them (packageStateOperations.go): naming a FOREIGN
	// interface's qualifier registers that package for the file's using alias, and importQueue holds the import paths the CURRENT
	// file declares (visitImportSpec adds each), which is what decides whether that alias is in scope. Each test states its file's imports.
	owner = &Visitor{info: ownerPkg.TypesInfo, pkg: ownerPkg.Types, newline: "\n", referencedForeignPackages: hashset.HashSet[string]{}, importQueue: hashset.HashSet[string]{}}
	consumer = &Visitor{info: consumerPkg.TypesInfo, pkg: consumerPkg.Types, newline: "\n", referencedForeignPackages: hashset.HashSet[string]{}, importQueue: hashset.HashSet[string]{}}

	return owner, consumer, collect(ownerPkg.Syntax), collect(consumerPkg.Syntax)
}

func crossPackageInstance(t *testing.T, visitor *Visitor, calls map[string]*ast.CallExpr, wrapper string) (*ast.Ident, *types.TypeList) {
	t.Helper()

	callExpr, ok := calls[wrapper]

	if !ok {
		t.Fatalf("fixture call inside %s was not found", wrapper)
	}

	funIdent := getCallFunIdent(callExpr.Fun)

	if funIdent == nil {
		t.Fatalf("call inside %s has no resolvable callee ident", wrapper)
	}

	instance, ok := visitor.info.Instances[funIdent]

	if !ok || instance.TypeArgs == nil {
		t.Fatalf("go/types recorded no instantiation for the call inside %s", wrapper)
	}

	return funIdent, instance.TypeArgs
}

func TestConstraintProxyOwnerRecordsItsOwnProxy(t *testing.T) {
	owner, _, ownerCalls, _ := loadCrossPackageProxyFixture(t)
	constraintProxies = make(map[string][2]string)

	funIdent, typeArgs := crossPackageInstance(t, owner, ownerCalls, "ownerCall")
	proxyName, ok := owner.constraintProxySigArg(funIdent, typeArgs, 0)

	if !ok || proxyName != "P1жPoint" {
		t.Fatalf("the OWNER of the constraint must name its proxy bare: got %q, %v; want %q, true", proxyName, ok, "P1жPoint")
	}

	if len(constraintProxies) != 1 {
		t.Fatalf("the owner must record exactly its one (element, interface) pair, recorded %d: %v", len(constraintProxies), constraintProxies)
	}

	for _, pair := range constraintProxies {
		if pair[1] != "Point" {
			t.Errorf("the owner's record must spell its LOCAL interface bare, got %q", pair[1])
		}
	}
}

func TestConstraintProxyConsumerNamesTheOwnersProxy(t *testing.T) {
	_, consumer, _, consumerCalls := loadCrossPackageProxyFixture(t)
	constraintProxies = make(map[string][2]string)
	consumer.importQueue.Add("example/proxycross/curves") // proxycross.go imports curves: its alias is in scope

	funIdent, typeArgs := crossPackageInstance(t, consumer, consumerCalls, "consumerCall")
	proxyName, ok := consumer.constraintProxySigArg(funIdent, typeArgs, 0)

	if !ok || proxyName != "curves.P1жPoint" {
		t.Fatalf("a CONSUMER of a foreign constraint must name the owner's proxy through the interface package's C# qualifier: got %q, %v; want %q, true", proxyName, ok, "curves.P1жPoint")
	}

	if len(constraintProxies) != 0 {
		t.Fatalf("a consumer must record NO proxy of its own (a second class of the same name is CS1503), recorded %d: %v", len(constraintProxies), constraintProxies)
	}

	for key := range constraintProxies {
		if strings.Contains(key, "/") {
			t.Errorf("no record may spell a type by its Go import path: %q", key)
		}
	}
}

// TestConstraintProxyConsumerWithoutTheInterfaceImportQualifiesFully: a consumer FILE can instantiate a foreign constraint through a
// third package without importing the interface's own package. The file-local alias names nothing there (CS0246), so the qualifier
// must be the fully-qualified C# name -- and never the Go import path, which is what an unconverted fully-qualified name would be.
func TestConstraintProxyConsumerWithoutTheInterfaceImportQualifiesFully(t *testing.T) {
	_, consumer, _, consumerCalls := loadCrossPackageProxyFixture(t)
	constraintProxies = make(map[string][2]string)
	consumer.importQueue.Add("example/proxycross/mid") // indirect.go imports mid only

	funIdent, typeArgs := crossPackageInstance(t, consumer, consumerCalls, "indirectCall")
	proxyName, ok := consumer.constraintProxySigArg(funIdent, typeArgs, 0)

	if !ok {
		t.Fatalf("an indirect consumer of a foreign constraint must still name the owner's proxy, got %q, false", proxyName)
	}

	if strings.Contains(proxyName, "/") {
		t.Fatalf("the fully-qualified qualifier must be the C# name, not the Go import path: %q", proxyName)
	}

	if strings.HasPrefix(proxyName, "curves.") || !strings.HasSuffix(proxyName, "curves_package.P1жPoint") {
		t.Fatalf("a file that does not import curves must not use its alias and must fully qualify the owner's proxy, got %q", proxyName)
	}

	if len(constraintProxies) != 0 {
		t.Fatalf("an indirect consumer must record NO proxy of its own, recorded %d: %v", len(constraintProxies), constraintProxies)
	}
}
