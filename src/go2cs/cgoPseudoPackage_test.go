// cgoPseudoPackage_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"os"
	"path/filepath"
	"regexp"
	"strings"
	"testing"
)

// cgoMangledName matches a C type or constant name as cmd/cgo mangles it, which converted code must never spell.
var cgoMangledName = regexp.MustCompile(`\b_C(type|iconst|fconst|sconst)_\w+`)

// cgoStringLiteral matches a C# string literal: a reflection stamp carries the mangled Go name as data, which is
// Go's own name for the type, not a reference to it.
var cgoStringLiteral = regexp.MustCompile(`"(?:[^"\\]|\\.)*"`)

func readConverted(t *testing.T, output, name string) string {
	t.Helper()

	text, err := os.ReadFile(filepath.Join(output, name))

	if err != nil {
		t.Fatal(err)
	}

	return strings.ReplaceAll(string(text), "\r\n", "\n")
}

// The behavioral project CgoPseudoPackageTypes is a package as cmd/cgo leaves it, written as plain Go: its C types and
// constants convert into the package's C pseudo-package under their C names, every reference reaches them as
// `C.<name>`, and a C typedef is a `Cꓸ` alias (cgo Phase 2 increment 3d, docs/phase4/DESIGN-cgo-phase2-pinvoke.md
// §11.17). Its output comparison proves the result behaves as Go does; this holds the shape.
func TestCgoTypesAndConstantsConvertIntoThePseudoPackage(t *testing.T) {
	dir, err := filepath.Abs(filepath.Join("..", "tests", "Behavioral", "CgoPseudoPackageTypes"))

	if err != nil {
		t.Fatal(err)
	}

	stderr, output, err := convertCgoGateFixture(t, dir, nil)

	if err != nil {
		t.Fatalf("conversion failed: %v\n%s", err, stderr)
	}

	for _, file := range emittedProjectFiles(t, output) {
		if !strings.HasSuffix(file, ".cs") {
			continue
		}

		code := cgoStringLiteral.ReplaceAllString(readConverted(t, output, file), `""`)

		if mangled := cgoMangledName.FindAllString(code, -1); len(mangled) > 0 {
			t.Errorf("%s spells cmd/cgo's mangled names %v, where converted code names the pseudo-package member", file, mangled)
		}
	}

	declarations := readConverted(t, output, "cgo_gotypes.cs")
	program := readConverted(t, output, "main.cs")
	packageInfo := readConverted(t, output, "package_info.cs")

	for _, want := range []struct{ file, text, in string }{
		{"cgo_gotypes.cs", declarations, "partial class mainꓸC_package {"},
		{"cgo_gotypes.cs", declarations, "\npartial struct @int /*num:int32*/;"},
		{"cgo_gotypes.cs", declarations, "\npartial struct struct_point {"},
		{"cgo_gotypes.cs", declarations, "    internal ж<struct_point> next;"},
		{"cgo_gotypes.cs", declarations, "internal static UntypedInt LIMIT => 0x10;"},
		{"cgo_gotypes.cs", declarations, "global using Cꓸdiv_t = go.mainꓸC_package.struct___0;"},
		{"main.cs", program, "using C = mainꓸC_package;"},
		{"main.cs", program, "global using cint = go.mainꓸC_package.@int;"},
		{"main.cs", program, "Cꓸdiv_t d = default;"},
		{"main.cs", program, "d.quot = (C.@int)C.LIMIT;"},
		{"main.cs", program, "internal static C.@int add(C.@int a, C.@int b)"},
		{"package_info.cs", packageInfo, "[GoPackage(\"main\")]\npartial class mainꓸC_package\n{\n"},
		{"package_info.cs", packageInfo, "\n    [GoLocalName(\"_Ctype_int\")] internal partial struct @int {}\n"},
		{"package_info.cs", packageInfo, "\n    [GoLocalName(\"_Ctype_struct_point\")] [GoValueClone(\"name\")] internal partial struct struct_point {}\n"},
		{"package_info.cs", packageInfo, "using static go.main_package;\nusing C = go.mainꓸC_package;\n"},
	} {
		if !strings.Contains(want.text, want.in) {
			t.Errorf("%s does not contain %q", want.file, want.in)
		}
	}

	// A C typedef is no part of the package's exported surface, so it publishes no alias to importers.
	if strings.Contains(packageInfo, `GoTypeAlias("Cꓸ`) {
		t.Errorf("package_info.cs publishes a C typedef to importers:\n%s", packageInfo)
	}

	// A C type's accessibility record belongs to the pseudo-package's class: in the PACKAGE class's section it would
	// declare a second, phantom type of the same name.
	_, section, _ := strings.Cut(packageInfo, "<TypeAccessibility>")
	section, _, _ = strings.Cut(section, "</TypeAccessibility>")

	if strings.Contains(section, "struct_point") || strings.Contains(section, "@int") {
		t.Errorf("package_info.cs records a C type in the package class:\n%s", section)
	}

	// The record carries what the declaration does not (section 11 of docs/PLAN-marker-comment-parity.md).
	if strings.Contains(declarations, "[GoLocalName") || strings.Contains(declarations, "[GoValueClone") {
		t.Errorf("cgo_gotypes.cs carries an attribute its accessibility record should:\n%s", declarations)
	}
}

// cmd/cgo writes each C declaration alone, so a declaration that groups a C name with a Go one is not something it
// emits, and is refused by name rather than split.
func TestADeclarationMixingCAndGoNamesIsRefusedByName(t *testing.T) {
	dir := writeCgoGateModule(t, map[string]string{"main.go": `package main

type (
	_Ctype_int int32
	count      int
)

func main() { var _ _Ctype_int; var _ count }
`})

	stderr, output, err := convertCgoGateFixture(t, dir, nil)

	requireCgoRefusal(t, stderr, err, output, "_Ctype_int", "count", "mixes cmd/cgo's C names")
}
