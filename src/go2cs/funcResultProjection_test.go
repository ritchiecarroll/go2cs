// funcResultProjection_test.go - Gbtc
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

// The defect this locks in (H7 red 3, COORD 17e1ba0d2): crypto/internal/fips140/hmac's cast.go, new at
// 1.24, calls `New(sha256.New, input)` against `New[H fips140.Hash](h func() H, key []byte)`. The method-group
// argument forces explicit type arguments, which rendered the box — `New<ж<sha256.Digest>>` — and a box can
// never NOMINALLY satisfy `where H : fips140.Hash`: its generated pointer adapter implements the interface,
// the box does not (CS0311). The fix renders H as the constraint and widens the delegate through the adapter,
// the func-result twin of the slice-element projection go/ast's walkList already takes.
//
// RED 4 (COORD 50c02fe0e) is the same rule one type-argument kind over: crypto/hkdf passes a `func() hash.Hash`
// into `[H fips140.Hash]`, and `hash.Hash` is a SIBLING of the constraint (identical method set, no embedding
// edge), so C# sees no nominal relation and `Extract<hash.Hash>` is CS0311 too. A sibling interface argument
// projects through the generated INTERFACE adapter; an interface that embeds the constraint, or IS it, already
// satisfies it nominally in the emitted C# and declines.
//
// These tests pin the predicate's scope against the converter's own helpers: the func-result reach projects,
// a sibling-parameterized constraint is instantiated over the call's arguments, and every other reach — a
// value argument, a self-referential constraint (the proxy's), a bare parameter, a result naming the type
// parameter, a two-result factory, an interface already derived from the constraint — declines.
const funcResultFixture = `package funcresult

type named interface{ label() string }

// labeler is a SIBLING of named: the same method set, no embedding edge (hash.Hash vs fips140.Hash).
type labeler interface{ label() string }

// labelerPlus EMBEDS named, so the emitted C# interface derives from it nominally.
type labelerPlus interface {
	named
	extra()
}

// keyed is parameterized by a SIBLING type parameter, not by the one it constrains.
type keyed[E any] interface{ encap() E }

// keyedNamed is crypto/mlkem's decapsulationKey[E]: parameterized by a sibling whose OWN constraint
// is a plain method set, so both parameters project and the closed-over form must be the sibling's
// PROJECTION rather than its box.
type keyedNamed[E named] interface {
	label() string
	encapKey() E
}

// point is self-referential: the constraint proxy's case, never this rule's.
type point[T any] interface{ combine(T) T }

type digest struct{ v int }

func (d *digest) label() string            { return "digest" }
func (d *digest) encap() int               { return d.v }
func (d *digest) combine(o *digest) *digest { return o }
func (d *digest) extra()                    {}
func (d *digest) encapKey() *digest           { return d }

type value struct{}

func (value) label() string { return "value" }

func newDigest() *digest                   { return &digest{} }
func newValue() value                      { return value{} }
func newLabeler() labeler                  { return &digest{} }
func newPlus() labelerPlus                 { return &digest{} }
func newNamed() named                      { return &digest{} }
func newAnon() interface{ label() string } { return &digest{} }

func factory[H named](h func() H, key []byte) int        { return len(key) }
func sibling[E any, D keyed[E]](newD func() D, e E) int  { return 0 }
func proxied[P point[P]](newP func() P) int              { return 0 }
func bareToo[H named](h func() H, other H) int           { return 0 }
func returns[H named](h func() H) H                      { return h() }
func twoResults[H named](h func() (H, error)) int        { return 0 }
func variadic[H named](hs ...func() H) int               { return len(hs) }

// ctor is crypto/mlkem's shape and the Go CONSTRUCTOR idiom: the func parameter takes ARGUMENTS and
// returns (T, error). mlkem reaches E through newEncapsulationKey func([]byte) (E, error) and D
// through generateKey func() (D, error); both are refused by the niladic/one-result gate, which is
// why its four sites emitted an explicit type-argument list naming the BOXES and failed CS0311 x4.
func ctor[H named](h func([]byte) (H, error), key []byte) int { return 0 }

// pair is crypto/mlkem testRoundTrip: BOTH type parameters are reached as func RESULTS and D
// constraint names E, so both project and D constraint must close over E PROJECTION.
func pair[E named, D keyedNamed[E]](newD func() (D, error), newE func([]byte) (E, error)) int { return 0 }

func pointerCall() int   { return factory(newDigest, nil) }
func ctorCall() int      { return ctor(func(b []byte) (*digest, error) { return newDigest(), nil }, nil) }
func pairCall() int      { return pair(func() (*digest, error) { return newDigest(), nil }, func(b []byte) (*digest, error) { return newDigest(), nil }) }
func valueCall() int     { return factory(newValue, nil) }
func siblingCall() int   { return sibling(newDigest, 1) }
func proxiedCall() int   { return proxied(newDigest) }
func bareCall() int      { return bareToo(newDigest, newDigest()) }
func returnsCall() named { return returns(newDigest) }
func twoCall() int       { return twoResults(func() (*digest, error) { return newDigest(), nil }) }
func variadicCall() int  { return variadic(newDigest) }
func ifaceCall() int     { return factory(newLabeler, nil) }
func embedCall() int     { return factory(newPlus, nil) }
func sameCall() int      { return factory(newNamed, nil) }
func anonCall() int      { return factory(newAnon, nil) }
`

func loadFuncResultFixture(t *testing.T) (*Visitor, map[string]*ast.CallExpr) {
	t.Helper()

	dir := t.TempDir()

	writeModuleFiles(t, dir, map[string]string{
		"go.mod":        "module example/funcresult\n\ngo 1.23\n",
		"funcresult.go": funcResultFixture,
	})

	// renderedTypeArgs consults the constraint proxy first, which registers what it resolves; stand the
	// map up and restore it so these tests neither panic on a nil map nor leak into a sibling test.
	previousProxies := constraintProxies
	t.Cleanup(func() { constraintProxies = previousProxies })
	constraintProxies = make(map[string][2]string)

	production := loadProductionForDir(t, dir)
	visitor := &Visitor{info: production.TypesInfo, pkg: production.Types}
	calls := map[string]*ast.CallExpr{}

	for _, file := range production.Syntax {
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

	return visitor, calls
}

func funcResultInstance(t *testing.T, visitor *Visitor, calls map[string]*ast.CallExpr, wrapper string) (*ast.Ident, *types.TypeList) {
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

// TestFuncResultProjectionPositive is the direct regression: the hmac shape projects, renders its type argument
// as the constraint, and maps only the `func() H` argument onto the projection.
func TestFuncResultProjectionPositive(t *testing.T) {
	visitor, calls := loadFuncResultFixture(t)
	funIdent, typeArgs := funcResultInstance(t, visitor, calls, "pointerCall")

	arg, constraint, ok := visitor.funcResultProjection(funIdent, typeArgs, 0)

	if !ok {
		t.Fatal("a pointer reached as a func RESULT against a plain method-set constraint did not project — this is the CS0311 defect")
	}

	if got := arg.String(); got != "*example/funcresult.digest" {
		t.Fatalf("projected pointer = %s, want *example/funcresult.digest", got)
	}

	if got := constraint.String(); got != "example/funcresult.named" {
		t.Fatalf("projected constraint = %s, want example/funcresult.named", got)
	}

	if rendered := visitor.renderedTypeArgs(funIdent, typeArgs); len(rendered) != 1 || rendered[0] != "named" {
		t.Fatalf("renderedTypeArgs = %v, want [named] — the constraint, not the box", rendered)
	}

	if _, _, ok := visitor.funcResultProjectionArg(funIdent, typeArgs, 0); !ok {
		t.Fatal("argument 0 (`h func() H`) did not map onto the projection")
	}

	if _, _, ok := visitor.funcResultProjectionArg(funIdent, typeArgs, 1); ok {
		t.Fatal("argument 1 (`key []byte`) mapped onto the projection")
	}

	// A constraint parameterized by a SIBLING type parameter is not self-referential: it is instantiated
	// over the call's own type arguments (keyed[int]) and projects.
	siblingIdent, siblingArgs := funcResultInstance(t, visitor, calls, "siblingCall")

	if _, constraint, ok := visitor.funcResultProjection(siblingIdent, siblingArgs, 1); !ok {
		t.Fatal("a sibling-parameterized constraint did not project")
	} else if got := constraint.String(); got != "example/funcresult.keyed[int]" {
		t.Fatalf("sibling constraint = %s, want example/funcresult.keyed[int]", got)
	}

	if _, _, ok := visitor.funcResultProjection(siblingIdent, siblingArgs, 0); ok {
		t.Fatal("the sibling's own `E any` position (an int, no method set) projected")
	}
}

// TestFuncResultProjectionConstructorShape is crypto/mlkem's regression: the Go CONSTRUCTOR idiom —
// a func parameter that takes ARGUMENTS and returns (T, error) — projects exactly as `func() T` does.
//
// ⚠ This is the row's real gate, and it was NOT a missing constraint proxy. funcResultProjectionArg
// required `paramSig.Params().Len() == 0 && paramSig.Results().Len() == 1`, so mlkem's
// `newEncapsulationKey func([]byte) (E, error)` was refused on BOTH clauses and its four sites
// emitted an explicit type-argument list naming the boxes: CS0311 ×4 on `E`, measured live at the
// version tip with the converter unmodified. Neither clause bears on whether the RESULT projects —
// the parameter list is the caller's business, and a trailing `error` is the idiom, not a second
// projectable position.
//
// The two properties that DO bear on it keep their negative controls unchanged: a type parameter also
// reached as a BARE parameter (`bareCall`) and one named by the function's own RESULT (`returnsCall`)
// still refuse, and mlkem has neither — `testRoundTrip` takes no bare E or D and returns nothing.
func TestFuncResultProjectionConstructorShape(t *testing.T) {
	visitor, calls := loadFuncResultFixture(t)
	funIdent, typeArgs := funcResultInstance(t, visitor, calls, "ctorCall")

	arg, constraint, ok := visitor.funcResultProjection(funIdent, typeArgs, 0)

	if !ok {
		t.Fatal("a pointer reached as the FIRST RESULT of a `func([]byte) (H, error)` constructor did not project — this is crypto/mlkem's CS0311 ×4")
	}

	if got := arg.String(); got != "*example/funcresult.digest" {
		t.Fatalf("projected pointer = %s, want *example/funcresult.digest", got)
	}

	if got := constraint.String(); got != "example/funcresult.named" {
		t.Fatalf("projected constraint = %s, want example/funcresult.named", got)
	}

	if rendered := visitor.renderedTypeArgs(funIdent, typeArgs); len(rendered) != 1 || rendered[0] != "named" {
		t.Fatalf("renderedTypeArgs = %v, want [named] — the constraint, not the box; naming the box IS the CS0311", rendered)
	}

	if _, _, ok := visitor.funcResultProjectionArg(funIdent, typeArgs, 0); !ok {
		t.Fatal("argument 0 (`h func([]byte) (H, error)`) did not map onto the projection")
	}

	// `key []byte` carries no type parameter and must not map, exactly as the niladic fixture's
	// second argument does not.
	if _, _, ok := visitor.funcResultProjectionArg(funIdent, typeArgs, 1); ok {
		t.Fatal("argument 1 (`key []byte`) mapped onto the projection")
	}

	// ⚠ twoCall (`func() (H, error)`) MOVED OUT of TestFuncResultProjectionNegativeControls with this
	// seat, and it is the only control that moved. Its stated reason there was "a two-result factory
	// is not `func() H`" — a restatement of the gate rather than a property of the shape, unlike its
	// neighbours (`bareCall` and `returnsCall` name a real mis-rendering, `variadicCall` a slice of
	// delegates). mlkem trips only the definitional one, so it is flipped deliberately and asserted
	// here instead: the niladic half of the constructor idiom projects for the same reason the
	// parameterized half does.
	twoIdent, twoArgs := funcResultInstance(t, visitor, calls, "twoCall")

	if _, _, ok := visitor.funcResultProjection(twoIdent, twoArgs, 0); !ok {
		t.Fatal("`func() (H, error)` did not project — the trailing error is the Go idiom, not a second projectable result")
	}

	if position, ok := visitor.funcResultProjectionResultIndex(twoIdent, 0); !ok || position != 0 {
		t.Fatalf("result index = %d (ok=%v), want 0 — the type parameter is the FIRST result and the error follows", position, ok)
	}
}

// TestFuncResultProjectionSiblingClosesOverProjection is the RE-RULED sibling control (COORD
// 59e713bbb): when a constraint names a sibling type parameter and BOTH project, the constraint is
// closed over the sibling's PROJECTION and never over its box.
//
// ⚠ The rule used to refuse any type parameter another constraint mentioned, which is why
// crypto/mlkem could not reach this state at all. Relaxing it without closing over the projection
// would emit the HALF-STATE the row already showed once — `decapsulationKey<ж<EncapsulationKey768>>`,
// a constraint naming the very box the projection exists to avoid — so the closure is the point of
// the relaxation and not a detail of it.
func TestFuncResultProjectionSiblingClosesOverProjection(t *testing.T) {
	visitor, calls := loadFuncResultFixture(t)
	funIdent, typeArgs := funcResultInstance(t, visitor, calls, "pairCall")

	// E: a plain method-set constraint reached through `newE func([]byte) (E, error)`.
	if _, constraint, ok := visitor.funcResultProjection(funIdent, typeArgs, 0); !ok {
		t.Fatal("E did not project — a constraint naming it must no longer refuse it outright")
	} else if got := constraint.String(); got != "example/funcresult.named" {
		t.Fatalf("E's constraint = %s, want example/funcresult.named", got)
	}

	// D: `keyedNamed[E]`, reached through `newD func() (D, error)`. Its constraint must name E's
	// PROJECTION (`named`), not E's type argument (`*digest`, the box in the emitted C#).
	_, constraint, ok := visitor.funcResultProjection(funIdent, typeArgs, 1)

	if !ok {
		t.Fatal("D did not project — both parameters are func-result reaches and neither is self-referential")
	}

	if got := constraint.String(); got != "example/funcresult.keyedNamed[example/funcresult.named]" {
		t.Fatalf("D's constraint = %s, want example/funcresult.keyedNamed[example/funcresult.named] — closed over E's PROJECTION; naming *digest here is the half-state that re-raises CS0311", got)
	}
}

// TestFuncResultProjectionSiblingInterface is RED 4's regression: a declared interface that satisfies the
// constraint by method set but does not derive from it projects exactly as the pointer does, renders the
// constraint, and hands back the interface itself as the argument to wrap.
func TestFuncResultProjectionSiblingInterface(t *testing.T) {
	visitor, calls := loadFuncResultFixture(t)
	funIdent, typeArgs := funcResultInstance(t, visitor, calls, "ifaceCall")

	arg, constraint, ok := visitor.funcResultProjection(funIdent, typeArgs, 0)

	if !ok {
		t.Fatal("a SIBLING interface reached as a func RESULT did not project — this is RED 4's CS0311 (hash.Hash vs fips140.Hash)")
	}

	if got := arg.String(); got != "example/funcresult.labeler" {
		t.Fatalf("projected argument = %s, want example/funcresult.labeler", got)
	}

	if got := constraint.String(); got != "example/funcresult.named" {
		t.Fatalf("projected constraint = %s, want example/funcresult.named", got)
	}

	if rendered := visitor.renderedTypeArgs(funIdent, typeArgs); len(rendered) != 1 || rendered[0] != "named" {
		t.Fatalf("renderedTypeArgs = %v, want [named] — the constraint, not the sibling interface", rendered)
	}

	if _, _, ok := visitor.funcResultProjectionArg(funIdent, typeArgs, 0); !ok {
		t.Fatal("argument 0 of the sibling-interface call did not map onto the projection")
	}
}

// TestFuncResultProjectionNegativeControls keeps the rule to the reach it can carry. Each call below
// instantiates the same kind of callee; none may project, or unrelated generics churn.
func TestFuncResultProjectionNegativeControls(t *testing.T) {
	visitor, calls := loadFuncResultFixture(t)

	for wrapper, reason := range map[string]string{
		"valueCall":    "a VALUE type argument satisfies its constraint nominally",
		"proxiedCall":  "a SELF-REFERENTIAL constraint is the constraint proxy's",
		"bareCall":     "a type parameter also reached as a BARE parameter would receive the pointer where C# wants the interface",
		"returnsCall":  "a RESULT naming the type parameter would hand the caller the interface where Go has the pointer",
		"variadicCall": "a variadic `...func() H` slot is a slice of delegates, not a delegate",
		"embedCall":    "an interface that EMBEDS the constraint derives from it nominally in the emitted C#",
		"sameCall":     "the constraint ITSELF as the type argument needs no projection",
		"anonCall":     "an ANONYMOUS interface has no generated adapter class to wrap with",
	} {
		funIdent, typeArgs := funcResultInstance(t, visitor, calls, wrapper)

		if _, _, ok := visitor.funcResultProjection(funIdent, typeArgs, 0); ok {
			t.Errorf("%s projected: %s", wrapper, reason)
		}

		if _, _, ok := visitor.funcResultProjectionArg(funIdent, typeArgs, 0); ok {
			t.Errorf("%s mapped argument 0 onto the projection: %s", wrapper, reason)
		}
	}
}

// TestProjectedGenericConstraintRecordsItsAdapter is part (d) of crypto/mlkem's seat, red-first: the
// three settled parts reach a COHERENT type-argument list —
// `testRoundTrip<encapsulationKey, decapsulationKey<encapsulationKey>>` — and then the two D-side
// arguments stay unwidened, because convertToInterfaceType hands back no `new …` for a PARAMETERIZED
// constraint and the row sits at CS0407 ×8.
//
// ⚠ The absent adapter is the CONVERTER's, not the generator's. The generator mints one adapter per
// recorded (element, interface) pair, and mlkem's emission records exactly TWO — both for the PLAIN
// constraint `encapsulationKey` — and none for `decapsulationKey` of anything. Nothing was refused;
// nothing was asked. Billing it to the generator was this lane's own misreading (mailbox cf3a2d76,
// corrected at 44812e89, taken by COORD at 9b9f779de).
//
// The reason the pair is never recorded is the RETURN-COVARIANCE shape, and it is the same one that
// refuted this seat's first cut: `recordSatisfiesIface` asks `types.Implements(targetType, iface)`
// against the interface as NAMED, and the PROJECTED form is `keyedNamed[named]`, whose `encapKey()`
// returns the interface where `*digest.encapKey()` returns the pointer. Go has no return covariance,
// so the answer is false and every record and emission arm downstream is gated off. The satisfaction
// question must be asked of the form closed over the TYPE ARGUMENTS (`keyedNamed[*digest]`, which the
// Go checker already admitted) while the RECORDED and RENDERED name stays the projection — the same
// two-instantiation split funcResultProjection itself carries.
func TestProjectedGenericConstraintRecordsItsAdapter(t *testing.T) {
	visitor, calls := loadFuncResultFixture(t)
	funIdent, typeArgs := funcResultInstance(t, visitor, calls, "pairCall")

	// Argument 0 of `pair` is `newD func() (D, error)` — the D side, whose constraint is parameterized.
	ptr, constraint, checkConstraint, ok := visitor.funcResultProjectionArgChecked(funIdent, typeArgs, 0)

	if !ok {
		t.Fatal("D's argument did not map onto the projection — the settled parts must hold before the adapter question is asked")
	}

	if got := constraint.String(); got != "example/funcresult.keyedNamed[example/funcresult.named]" {
		t.Fatalf("D's constraint = %s, want example/funcresult.keyedNamed[example/funcresult.named]", got)
	}

	previousImplementations, previousAdapterClasses := interfaceImplementations, adapterClassImplementations

	t.Cleanup(func() {
		interfaceImplementations, adapterClassImplementations = previousImplementations, previousAdapterClasses
	})

	interfaceImplementations = make(map[string]hashset.HashSet[string])
	adapterClassImplementations = hashset.HashSet[string]{}

	wrapped := visitor.convertToProjectedInterfaceType(constraint, checkConstraint, ptr, "src")
	if !strings.HasPrefix(wrapped, "new ") {
		t.Fatalf("the projected conversion returned %q, want a `new <adapter>(src)` construction; recorded pairs = %v", wrapped, interfaceImplementations)
	}

	// ⚠ The CLASS name is the BARE interface name and the RECORD is the CLOSED instantiation, and
	// the two being different is the point rather than an inconsistency: `digestжkeyedNamed<named>`
	// is not an identifier a non-generic class can carry, so a name composed with the argument list
	// would reference a class the generator never emits (CS0246). The corpus already runs on this
	// shape — crypto/elliptic records `nistPoint<P224Point>` and references `P224PointжnistPoint`.
	//
	// Composed through adapterNameMarker rather than spelled out, so the expectation cannot drift
	// from the marker format the resolver reads.
	if want := "new " + adapterNameMarker("digest", "keyedNamed") + "(src)"; wrapped != want {
		t.Fatalf("projected conversion = %q, want %q — the adapter CLASS takes the interface's BARE name", wrapped, want)
	}

	recorded, exists := interfaceImplementations["keyedNamed<named>"]

	if !exists {
		t.Fatalf("no implement pair was recorded under the CLOSED interface name; recorded = %v — the generator mints an adapter only for a recorded pair, so the cast site would reference a class that is never emitted (CS0246)", interfaceImplementations)
	}

	if !recorded.Contains(PointerPrefix + "<digest>") {
		t.Fatalf("the pair recorded under keyedNamed<named> = %v, want the POINTER form %s<digest> — a value record generates the boxing partial struct, not the ж adapter the cast site constructs", recorded, PointerPrefix)
	}

	// The negative half, and it is what proves the SPLIT rather than the conversion: asking the
	// satisfaction question of the PROJECTED form — which is what convertToInterfaceType does, and
	// what this seat did before the split — still declines, because no Go type implements
	// `keyedNamed[named]`. If this ever starts returning an adapter, the two-instantiation split has
	// stopped being load-bearing and this test's green above means something else.
	beforeUnsplit := interfaceImplementations
	interfaceImplementations = make(map[string]hashset.HashSet[string])

	if unsplit := visitor.convertToInterfaceType(constraint, ptr, "src"); strings.HasPrefix(unsplit, "new ") {
		t.Fatalf("the UNSPLIT conversion returned %q — Go has no return covariance, so asking types.Implements of the projected form must still decline", unsplit)
	} else if len(interfaceImplementations) != 0 {
		t.Fatalf("the UNSPLIT conversion recorded %v, want nothing", interfaceImplementations)
	}

	interfaceImplementations = beforeUnsplit
}
