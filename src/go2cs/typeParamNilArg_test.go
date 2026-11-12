// typeParamNilArg_test.go - Gbtc
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

// The defect this locks in (H7 red 2, COORD 89c281d32): go/types infer.go's `slices.Contains(inferred, nil)`,
// new at 1.24, emitted `slices.Contains(inferred, default!)`. C# infers a generic call's type arguments from
// its arguments, and a typeless `default!` names no E, so both sites failed CS0411. The fix casts an untyped
// nil bound to a TYPE-PARAMETER parameter of an INFERRED call to the INSTANTIATED parameter type.
//
// These tests pin the three facts that rule stands on, each against the converter's own helpers:
//   - an INFERRED call's callee signature is the generic ORIGIN, so the nil's parameter IS a type parameter;
//   - instantiatedParamType answers the type go/types inferred for that parameter;
//   - an EXPLICIT instantiation's signature is already instantiated, so its parameter is NOT a type
//     parameter and the rule never fires (its emitted type arguments already bind the nil).
//
// A variadic tail and a non-generic nil complete the positive and negative sides.
const typeParamNilFixture = `package tpnil

type Ifc interface{ M() }

func Contains[S ~[]E, E comparable](s S, v E) bool { return false }
func Some[T any](v ...T) int                        { return len(v) }
func G(p *int) bool                                  { return p == nil }

var xs []Ifc
var p *int

func inferred() bool { return Contains(xs, nil) }
func explicit() bool { return Contains[[]Ifc, Ifc](xs, nil) }
func variadic() int  { return Some(p, nil) }
func plain() bool    { return G(nil) }
`

func loadTypeParamNilFixture(t *testing.T) (*Visitor, map[string]*ast.CallExpr) {
	t.Helper()

	dir := t.TempDir()

	writeModuleFiles(t, dir, map[string]string{
		"go.mod":   "module example/tpnil\n\ngo 1.23\n",
		"tpnil.go": typeParamNilFixture,
	})

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

func TestTypeParamNilArgPredicate(t *testing.T) {
	visitor, calls := loadTypeParamNilFixture(t)

	declaredIsTypeParam := func(wrapper string, arg int) bool {
		t.Helper()

		callExpr, ok := calls[wrapper]

		if !ok {
			t.Fatalf("fixture call inside %s was not found", wrapper)
		}

		sig := visitor.getFunctionSignature(callExpr)

		if sig == nil {
			t.Fatalf("no callee signature for the call inside %s", wrapper)
		}

		paramType, ok := getParameterType(sig, arg)

		if !ok {
			t.Fatalf("no parameter %d for the call inside %s", arg, wrapper)
		}

		_, isTypeParam := paramType.(*types.TypeParam)

		return isTypeParam
	}

	ifc := visitor.pkg.Scope().Lookup("Ifc").Type()
	ptrInt := types.NewPointer(types.Typ[types.Int])

	// INFERRED: the rule fires, and the cast names the inferred interface.
	if !declaredIsTypeParam("inferred", 1) {
		t.Errorf("inferred Contains(xs, nil): parameter 1 is not a type parameter on the callee signature")
	}

	if !argIsUntypedNil(calls["inferred"].Args[1], visitor.info) {
		t.Errorf("inferred Contains(xs, nil): argument 1 is not read as the untyped nil")
	}

	if got := visitor.instantiatedParamType(calls["inferred"], 1); got == nil || !types.Identical(got, ifc) {
		t.Errorf("inferred Contains(xs, nil): instantiated parameter 1 = %v, want %v", got, ifc)
	}

	// EXPLICIT: never reaches the rule.
	if declaredIsTypeParam("explicit", 1) {
		t.Errorf("explicit Contains[[]Ifc, Ifc](xs, nil): parameter 1 reads as a type parameter, so the rule would fire on a call that already binds")
	}

	// VARIADIC tail: the trailing nil's element parameter is the type parameter, instantiated to *int.
	if !declaredIsTypeParam("variadic", 1) {
		t.Errorf("variadic Some(p, nil): the tail element is not a type parameter on the callee signature")
	}

	if got := visitor.instantiatedParamType(calls["variadic"], 1); got == nil || !types.Identical(got, ptrInt) {
		t.Errorf("variadic Some(p, nil): instantiated tail element = %v, want %v", got, ptrInt)
	}

	// NON-GENERIC: never reaches the rule.
	if declaredIsTypeParam("plain", 0) {
		t.Errorf("plain G(nil): a non-generic parameter reads as a type parameter")
	}
}
