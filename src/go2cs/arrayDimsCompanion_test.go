// arrayDimsCompanion_test.go - Gbtc
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
	"maps"
	"slices"
	"strings"
	"testing"

	"golang.org/x/tools/go/packages"
)

// Which generics gain an array-dims companion (arrayDimsCompanion.go), and which exported call is
// refused. One axis per function: each control differs from `zero` in exactly one of the three
// conditions (a zero-value sink, an array instantiation, unexported).
const arrayDimsCompanionFixture = `package dims

// zero builds a zero T and is instantiated with an array: it carries a companion.
func zero[T any]() T { var x T; return x }

// boxed builds its zero through new(T), the other sink.
func boxed[T any]() *T { return new(T) }

// forward has no sink of its own; it forwards T to zero, so the fixpoint gives it a companion.
func forward[T any]() T { return zero[T]() }

// noArray builds a zero T but no instantiation passes an array: no companion.
func noArray[T any]() T { var x T; return x }

// noSink is instantiated with an array but builds no zero: no companion.
func noSink[T any](v T) T { return v }

// Exported builds a zero T and is instantiated with an array; its C# arity is seen by other
// assemblies, so the call is refused instead.
func Exported[T any]() T { var x T; return x }

func use() {
	_ = zero[[4]byte]()
	_ = boxed[[2][3]int32]()
	_ = forward[[8]uint16]()
	_ = noArray[int]()
	_ = noSink([3]int{})
	_ = Exported[[5]byte]()
	_ = Exported[int]()
}
`

func loadArrayDimsFixture(t *testing.T) (*Visitor, *packages.Package) {
	t.Helper()

	dir := t.TempDir()

	writeModuleFiles(t, dir, map[string]string{
		"go.mod":  "module example/dims\n\ngo 1.23\n",
		"dims.go": arrayDimsCompanionFixture,
	})

	production := loadProductionForDir(t, dir)

	// The facts read the declaring package through descriptorCarrierPackage, as the descriptor
	// companion does (see descriptorCompanionCall_test.go), so the fixture installs it.
	previousSource := currentPackageSource

	t.Cleanup(func() {
		currentPackageSource = previousSource
		arrayDimsFactsCacheLock.Lock()
		arrayDimsFactsKey, arrayDimsFactsValue = nil, nil
		arrayDimsFactsCacheLock.Unlock()
	})

	currentPackageSource = production

	return &Visitor{info: production.TypesInfo, pkg: production.Types}, production
}

func TestArrayDimsCompanionSelectsOnlyCarryingGenerics(t *testing.T) {
	visitor, production := loadArrayDimsFixture(t)

	want := map[string][]int{
		"zero":     {0},
		"boxed":    {0},
		"forward":  {0},
		"noArray":  nil,
		"noSink":   nil,
		"Exported": nil,
	}

	for name, indices := range want {
		fn, _ := production.Types.Scope().Lookup(name).(*types.Func)

		if fn == nil {
			t.Fatalf("fixture has no function %s", name)
		}

		if got := visitor.arrayDimsCompanionParams(fn); !slices.Equal(got, indices) {
			t.Errorf("%s: companion indices = %v, want %v", name, got, indices)
		}
	}

	// Each dims type is declared once, by the function that owns it.
	owned := map[string]string{}
	facts := arrayDimsFactsFor(production, visitor.arrayDimsInputs())

	for fn, dimsList := range facts.ownedTypes {
		for _, dims := range dimsList {
			name := arrayDimsTypeName(dims)

			if previous, seen := owned[name]; seen {
				t.Errorf("%s declared by both %s and %s", name, previous, fn.Name())
			}

			owned[name] = fn.Name()
		}
	}

	for _, name := range []string{"ᴀ4", "ᴀ2_3", "ᴀ8"} {
		if _, declared := owned[name]; !declared {
			t.Errorf("no function declares %s (declared: %v)", name, owned)
		}
	}

	if _, declared := owned["ᴀ5"]; declared {
		t.Errorf("ᴀ5 belongs to the refused exported call and must not be declared")
	}
}

func TestArrayDimsRefusesOnlyTheExportedArrayCall(t *testing.T) {
	visitor, production := loadArrayDimsFixture(t)

	refused := map[string]bool{}

	for _, file := range production.Syntax {
		ast.Inspect(file, func(node ast.Node) bool {
			callExpr, isCall := node.(*ast.CallExpr)

			if !isCall {
				return true
			}

			funIdent := getCallFunIdent(callExpr.Fun)

			if funIdent == nil {
				return true
			}

			instance, ok := production.TypesInfo.Instances[funIdent]

			if !ok {
				return true
			}

			key := funIdent.Name + "[" + instance.TypeArgs.At(0).String() + "]"

			if refusal := visitor.arrayDimsRefusal(funIdent, instance.TypeArgs); refusal != "" {
				refused[key] = true

				if !strings.Contains(refusal, "Exported") || !strings.Contains(refusal, "[5]byte") {
					t.Errorf("%s: refusal does not name the function and the type argument: %q", key, refusal)
				}
			}

			return true
		})
	}

	if !refused["Exported[[5]byte]"] {
		t.Errorf("Exported[[5]byte] was not refused (refused: %v)", refused)
	}

	if len(refused) != 1 {
		t.Errorf("exactly one call should be refused, got %v", refused)
	}
}

func TestArrayDimsCompanionArgSpellsDimsAndForwards(t *testing.T) {
	visitor, _ := loadArrayDimsFixture(t)

	byteType := types.Typ[types.Byte]

	if got := visitor.arrayDimsCompanionArg(types.NewArray(byteType, 16)); got != "ᴀ16" {
		t.Errorf("[16]byte: %q, want ᴀ16", got)
	}

	if got := visitor.arrayDimsCompanionArg(types.NewArray(types.NewArray(byteType, 3), 2)); got != "ᴀ2_3" {
		t.Errorf("[2][3]byte: %q, want ᴀ2_3", got)
	}

	if got := visitor.arrayDimsCompanionArg(byteType); got != arrayDimsNoneType {
		t.Errorf("byte: %q, want %s", got, arrayDimsNoneType)
	}

	typeParam := types.NewTypeParam(types.NewTypeName(0, nil, "T", nil), types.NewInterfaceType(nil, nil))
	visitor.currentFuncArrayDimsNames = map[*types.TypeParam]string{typeParam: "Tᴀ"}

	if got := visitor.arrayDimsCompanionArg(typeParam); got != "Tᴀ" {
		t.Errorf("forwarded T: %q, want Tᴀ", got)
	}
}

// The array-dims OVERLOAD (DESIGN-array-type-argument-length.md 3.3 and 3.5): an exported generic that
// reaches a TYPE-ONLY sink, here another package's function that published a record, gains an
// additive arity-2 form, and only a call site with dims to pass supplies the companion.
const arrayDimsOverloadDep = `package dep

// Sink stands for abi.TypeFor[T](): its package published a GoArrayDimsCompanion record for it.
func Sink[T any]() int { return 0 }
`

const arrayDimsOverloadFixture = `package dims

import "example/dims/dep"

// Reach is exported and reaches the sink: it gains the overload whether or not a caller here needs it.
func Reach[T any](v T) int { return dep.Sink[T]() }

// Zero is exported and builds a zero value: it keeps its refusal (COORD ruling 2026-11-25).
func Zero[T any]() T { var x T; return x }

// reach is unexported, forwards to the sink and is instantiated with an array: an ordinary companion.
func reach[T any]() int { return dep.Sink[T]() }

func use() {
	_ = Reach([2]string{})
	_ = Reach(1)
	_ = dep.Sink[[3]int]()
	_ = dep.Sink[int]()
	_ = reach[[4]byte]()
}
`

func loadArrayDimsOverloadFixture(t *testing.T, records map[string][]int) (*Visitor, *packages.Package) {
	t.Helper()

	dir := t.TempDir()

	writeModuleFiles(t, dir, map[string]string{
		"go.mod":     "module example/dims\n\ngo 1.23\n",
		"dims.go":    arrayDimsOverloadFixture,
		"dep/dep.go": arrayDimsOverloadDep,
	})

	production := loadProductionForDir(t, dir)
	previousSource := currentPackageSource

	t.Cleanup(func() {
		currentPackageSource = previousSource
		arrayDimsFactsCacheLock.Lock()
		arrayDimsFactsKey, arrayDimsFactsValue = nil, nil
		arrayDimsFactsCacheLock.Unlock()
		resetArrayDimsRecords()
	})

	currentPackageSource = production

	// The dependency's metadata, as importPackageInfoLines would read it: a record, or none at all.
	resetArrayDimsRecords()
	arrayDimsRecordsLock.Lock()
	importedArrayDimsCompanions["example/dims/dep"] = records
	arrayDimsRecordsLock.Unlock()

	return &Visitor{info: production.TypesInfo, pkg: production.Types}, production
}

// arrayDimsCallSupplies answers each instantiated call's companion supply, keyed `Name[typeArg]`.
func arrayDimsCallSupplies(visitor *Visitor, production *packages.Package) map[string]string {
	supplies := map[string]string{}

	for _, file := range production.Syntax {
		ast.Inspect(file, func(node ast.Node) bool {
			callExpr, isCall := node.(*ast.CallExpr)

			if !isCall {
				return true
			}

			funIdent := getCallFunIdent(callExpr.Fun)

			if funIdent == nil {
				return true
			}

			instance, ok := production.TypesInfo.Instances[funIdent]

			if !ok {
				return true
			}

			fn, _ := production.TypesInfo.Uses[funIdent].(*types.Func)
			supplies[funIdent.Name+"["+instance.TypeArgs.At(0).String()+"]"] = strings.Join(visitor.arrayDimsCompanionSupply(fn, instance.TypeArgs), ",")

			return true
		})
	}

	return supplies
}

func TestArrayDimsOverloadForAnExportedSinkReachingGeneric(t *testing.T) {
	visitor, production := loadArrayDimsOverloadFixture(t, map[string][]int{"Sink": {0}})

	lookup := func(name string) *types.Func {
		fn, _ := production.Types.Scope().Lookup(name).(*types.Func)

		if fn == nil {
			t.Fatalf("fixture has no function %s", name)
		}

		return fn
	}

	for name, want := range map[string]struct {
		indices    []int
		overloaded bool
	}{
		"Reach": {[]int{0}, true},
		"Zero":  {nil, false},
		"reach": {[]int{0}, false},
	} {
		fn := lookup(name)

		if got := visitor.arrayDimsCompanionParams(fn); !slices.Equal(got, want.indices) {
			t.Errorf("%s: companion indices = %v, want %v", name, got, want.indices)
		}

		if got := visitor.arrayDimsCompanionOverloaded(fn); got != want.overloaded {
			t.Errorf("%s: overloaded = %v, want %v", name, got, want.overloaded)
		}
	}

	// An array call site supplies the phantom; a non-array call site of an overload keeps arity 1; an
	// unexported carrier's every call site supplies.
	want := map[string]string{
		"Reach[[2]string]": "ᴀ2",
		"Reach[int]":       "",
		"Sink[[3]int]":     "ᴀ3",
		"Sink[int]":        "",
		"reach[[4]byte]":   "ᴀ4",
	}

	got := arrayDimsCallSupplies(visitor, production)

	for key, supply := range want {
		if got[key] != supply {
			t.Errorf("%s: supply %q, want %q", key, got[key], supply)
		}
	}

	// The overload's call sites declare their dims in the CALLER (use); the unexported carrier owns its own.
	facts := arrayDimsFactsFor(production, visitor.arrayDimsInputs())
	owners := map[string]string{}

	for fn, dimsList := range facts.ownedTypes {
		for _, dims := range dimsList {
			owners[arrayDimsTypeName(dims)] = fn.Name()
		}
	}

	if !maps.Equal(owners, map[string]string{"ᴀ2": "use", "ᴀ3": "use", "ᴀ4": "reach"}) {
		t.Errorf("phantom owners = %v", owners)
	}
}

func TestArrayDimsOverloadNeedsTheRecord(t *testing.T) {
	visitor, production := loadArrayDimsOverloadFixture(t, map[string][]int{})

	for key, supply := range arrayDimsCallSupplies(visitor, production) {
		if key == "reach[[4]byte]" {
			continue
		}

		if supply != "" {
			t.Errorf("%s: supplies %q with no record to read", key, supply)
		}
	}

	if fn, _ := production.Types.Scope().Lookup("Reach").(*types.Func); visitor.arrayDimsCompanionOverloaded(fn) {
		t.Errorf("Reach gained an overload with no record for the sink it reaches")
	}
}

func TestArrayDimsOverloadForwarderLine(t *testing.T) {
	typeParam := types.NewTypeParam(types.NewTypeName(0, nil, "T", nil), types.NewInterfaceType(nil, nil))
	visitor := &Visitor{currentFuncArrayDimsNames: map[*types.TypeParam]string{typeParam: "Tᴀ"}}

	got := visitor.arrayDimsOverloadForwarder("public", false, "Handle<T>", "Make", "<T, Tᴀ>", "T value", "\r\n    where Tᴀ : IGoArrayDims")

	if want := "public static Handle<T> Make<T>(T value) => Make<T, ᴀnone>(value);"; got != want {
		t.Errorf("forwarder\n got %s\nwant %s", got, want)
	}

	got = visitor.arrayDimsOverloadForwarder("public", false, "ref T", "At", "<T, K, Tᴀ>", "ref T x, K k", "\r\n    where K : new()\r\n    where Tᴀ : IGoArrayDims")

	if want := "public static ref T At<T, K>(ref T x, K k) where K : new() => ref At<T, K, ᴀnone>(ref x, k);"; got != want {
		t.Errorf("ref forwarder\n got %s\nwant %s", got, want)
	}
}

// TestArrayDimsFactsMeasureATargetScope pins the target the facts hand the hand-own registry. A function
// registered for one os/arch target (internal/cpu's osInit is registered for darwin/arm64 alone) is read by
// the facts' displaced test, and goosScope.includes refuses a bare GOOS against such an entry by panicking.
// The facts passed goosOfTarget(...) until the darwin/arm64 merge, so every target's conversion of
// internal/cpu failed on cpu.go (GO2CS2008) and dropped its records from package_info.cs.
func TestArrayDimsFactsMeasureATargetScope(t *testing.T) {
	visitor, production := loadArrayDimsFixture(t)

	previous, had := manualConversionFuncs["example/dims"]
	manualConversionFuncs["example/dims"] = map[string]goosScope{"noSink": {"darwin/arm64"}}

	t.Cleanup(func() {
		if had {
			manualConversionFuncs["example/dims"] = previous
		} else {
			delete(manualConversionFuncs, "example/dims")
		}
	})

	var noSink *ast.FuncDecl

	for _, file := range production.Syntax {
		for _, decl := range file.Decls {
			if funcDecl, ok := decl.(*ast.FuncDecl); ok && funcDecl.Name.Name == "noSink" {
				noSink = funcDecl
			}
		}
	}

	if noSink == nil {
		t.Fatal("fixture has no noSink declaration")
	}

	for _, target := range []string{"windows/amd64", "darwin/amd64", "darwin/arm64"} {
		visitor.options.targetPlatform = target
		inputs := visitor.arrayDimsInputs()

		if inputs.target != target {
			t.Errorf("%s: the facts read target %q, want the full os/arch target", target, inputs.target)
		}

		func() {
			defer func() {
				if r := recover(); r != nil {
					t.Errorf("%s: the facts panicked over a target-scoped registration: %v", target, r)
				}
			}()

			computeArrayDimsFacts(production, inputs)
		}()

		// The answer, not only the absence of a panic: the facts' displaced test asks the registry with
		// inputs.target, and noSink is displaced at darwin/arm64 and at no other target. The facts' own outputs
		// cannot show it for this fixture, since an unexported displaced generic keeps its companion analysis,
		// so the registry call is read directly; TestTargetScopedRegistrationConvertsEndToEnd reads the
		// displacement in a whole conversion's emission.
		if got, want := isManualFuncDeclInPackage(production.Types.Path(), inputs.target, noSink), target == "darwin/arm64"; got != want {
			t.Errorf("%s: noSink displaced = %v, want %v", target, got, want)
		}
	}
}
