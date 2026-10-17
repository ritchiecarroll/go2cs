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
	facts := arrayDimsFactsFor(production)

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
