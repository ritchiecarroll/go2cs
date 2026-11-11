// funcMethodCollision_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/ast"
	"go/importer"
	"go/parser"
	"go/token"
	"go/types"
	"testing"
)

// TestProductionFuncMethodCollisionRename pins funcRenamedForMethodCollision (C40, tsc's internal/checker): a
// production free function whose emitted signature equals a same-named production method's takes the Δ-rename,
// and nothing else does. A method declared in a `_test.go` file must not rename a production function: the
// production .cs keeps the bare name, and resolveReceiverParameterCollisions renames the test side instead.
func TestProductionFuncMethodCollisionRename(t *testing.T) {
	const lib = `package lib

type Type struct{ flags int }

func (t *Type) IsTupleType() bool { return t.flags&1 != 0 }

func IsTupleType(t *Type) bool { return t.flags&1 != 0 }

func (t *Type) Combine(other int) int { return t.flags + other }

func Combine(t *Type, other int) int { return other }

func (t Type) Flags() int { return t.flags }

func Flags(t Type) int { return t.flags }

func (t *Type) Size() int { return t.flags }

func Size(t *Type, scale int) int { return scale }

func (t *Type) Kind() int { return 0 }

func Kind[X any](t *Type) int { return 0 }

func Probe(t *Type) bool { return false }
`

	const libTest = `package lib

func (t *Type) Probe() bool { return true }
`

	fset := token.NewFileSet()
	var files []*ast.File

	for _, source := range []struct{ name, text string }{{"lib.go", lib}, {"lib_test.go", libTest}} {
		file, err := parser.ParseFile(fset, source.name, source.text, 0)

		if err != nil {
			t.Fatal(err)
		}

		files = append(files, file)
	}

	pkg, err := (&types.Config{Importer: importer.Default()}).Check("lib", fset, files, nil)

	if err != nil {
		t.Fatal(err)
	}

	for name, want := range map[string]bool{
		"IsTupleType": true,  // the TS pair: (t *Type) IsTupleType() and IsTupleType(t *Type)
		"Combine":     true,  // with a parameter after the receiver
		"Flags":       true,  // a value receiver
		"Size":        false, // control: the free function's signature differs
		"Kind":        false, // a generic free function keeps distinct C# signatures
		"Probe":       false, // the method is declared in a _test.go file: production stays pinned
	} {
		if got := funcRenamedForMethodCollision(pkg.Scope().Lookup(name), fset); got != want {
			t.Errorf("%s: funcRenamedForMethodCollision = %v, want %v", name, got, want)
		}
	}

	// A METHOD is never the renamed side.
	typeName := pkg.Scope().Lookup("Type").(*types.TypeName)

	for i := range typeName.Type().(*types.Named).NumMethods() {
		if method := typeName.Type().(*types.Named).Method(i); funcRenamedForMethodCollision(method, fset) {
			t.Errorf("method %s reported as renamed; only the free function takes the Δ", method.Name())
		}
	}
}
