// cgoVariables_test.go - Gbtc
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
	"strings"
	"testing"
)

// cgoVariablesSource declares C variables as cmd/cgo does, _Cvar_x of type *T, over the C types a header gives them.
const cgoVariablesSource = `package p

type _Ctype_int int32
type _Ctype_long int64
type _Ctype_char int8
type _Ctype_struct_gauge struct {
	count _Ctype_int
	_     [4]byte
	total _Ctype_long
}
type _Ctype_union_u [8]byte

var _Cvar_counter *_Ctype_int
var _Cvar_gauge *_Ctype_struct_gauge
var _Cvar_table *[4]_Ctype_int
var _Cvar_hook **[0]byte
var _Cvar_name **_Ctype_char
var _Cvar_either *_Ctype_union_u
`

// cgoVariablesDeclarations type-checks cgoVariablesSource and answers its C variables by C name.
func cgoVariablesDeclarations(t *testing.T) map[string]cgoDeclaration {
	t.Helper()

	fset := token.NewFileSet()
	file, err := parser.ParseFile(fset, "p.go", cgoVariablesSource, 0)

	if err != nil {
		t.Fatal(err)
	}

	pkg, err := (&types.Config{Importer: importer.Default()}).Check("p", fset, []*ast.File{file}, nil)

	if err != nil {
		t.Fatal(err)
	}

	declarations := map[string]cgoDeclaration{}

	for _, declaration := range cgoDeclarations(pkg) {
		if declaration.kind == cgoVar {
			declarations[declaration.cName] = declaration
		}
	}

	return declarations
}

// cgo item 3: a C scalar or struct variable binds as a box over its exported address, looked up on first use, with C.x
// a ref property over the box's value and C.Ꮡx the box itself.
func TestACScalarOrStructVariableBindsThroughABoxOverItsAddress(t *testing.T) {
	declarations := cgoVariablesDeclarations(t)
	set := newCgoWrapperSet(cgoBindingTarget{goos: "linux", library: "libprobe.so"}, types.SizesFor("gc", "amd64"))
	set.class = cgoPseudoPackageClass("p")

	for name, goSide := range map[string]string{"counter": "@int", "gauge": "struct_gauge"} {
		text, err := set.wrapper(declarations[name])

		if err != nil {
			t.Fatalf("C.%s is refused: %v", name, err)
		}

		for _, want := range []string{
			"private static ж<" + goSide + ">? " + name + "ᴍvariable;",
			"internal static ж<" + goSide + "> Ꮡ" + name + " => " + name + "ᴍvariable ??= global::go.golib.GoCgoLibraries.Variable<" + goSide + ">(typeof(pꓸC_package).Assembly, \"libprobe.so\", \"" + name + "\");",
			"internal static ref " + goSide + " " + name + " => ref Ꮡ" + name + ".Value;",
		} {
			if !strings.Contains(text, want) {
				t.Errorf("C.%s's binding lacks %q:\n%s", name, want, text)
			}
		}
	}
}

// The near-miss: a C variable whose type no box holds stays refused, by name and with its type.
func TestACVariableOfAnotherTypeStaysRefusedByName(t *testing.T) {
	declarations := cgoVariablesDeclarations(t)
	set := newCgoWrapperSet(cgoBindingTarget{goos: "linux", library: "libprobe.so"}, types.SizesFor("gc", "amd64"))

	for name, want := range map[string]string{
		"table":  "C.table is a C variable of type [4]C.int",
		"hook":   "C.hook is a C variable of type *[0]byte",
		"name":   "C.name is a C variable of type *C.char",
		"either": "C.either is a C variable of type C.union_u",
	} {
		_, err := set.wrapper(declarations[name])

		if err == nil {
			t.Errorf("C.%s binds; want it refused", name)
			continue
		}

		if !strings.Contains(err.Error(), want) || !strings.Contains(err.Error(), "go2cs reads and writes only") {
			t.Errorf("C.%s is refused as %q; want it to name %q and what go2cs reads", name, err, want)
		}
	}
}
