// internalTestLocalInterfaceStamp_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards a function-local NAMED interface declared in an INTERNAL _test.go file (BurntSushi/toml's encode
// test, COORD 2026-10-03): the white-box bridge writes type accessibility inline, so its interface lift must
// carry the [GoLocalName] stamp on the declaration, as its struct lift already does. Without it go2cs-gen
// cannot find the struct field embedding the interface and forwards a promoted method to
// `recvᴛ.<Func>_<name>.F()` (CS0120/CS1061). The struct lift's stamp and an anonymous interface lift, which has
// no Go name to stamp, are the controls.

package main

import (
	"go/types"
	"os"
	"path/filepath"
	"strings"
	"testing"
)

func TestAnInternalTestLocalInterfaceCarriesItsLocalNameStamp(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: loads and converts a test-variant fixture")
	}

	dir := t.TempDir()
	files := map[string]string{
		"go.mod": "module example/lif\n\ngo 1.23\n",
		"lib.go": "package lif\n\ntype Impl struct{ N int }\n\nfunc (i Impl) F() int { return i.N }\n",
		"lib_test.go": "package lif\n\nimport \"testing\"\n\n" +
			"func TestLocalEmbeddedInterface(t *testing.T) {\n" +
			"\ttype Inner interface{ F() int }\n" +
			"\ttype Outer struct{ Inner }\n" +
			"\tvar i Inner = Outer{Inner: Impl{3}}\n" +
			"\tvar anon interface{ F() int } = Impl{4}\n" +
			"\tif i.F()+anon.F() != 7 {\n\t\tt.Fatal(\"want 7\")\n\t}\n}\n",
	}

	for name, contents := range files {
		if err := os.WriteFile(filepath.Join(dir, name), []byte(contents), 0644); err != nil {
			t.Fatal(err)
		}
	}

	internal, _ := loadBothTestVariantsForDir(t, dir)

	if internal == nil {
		t.Fatal("the internal test variant was not loaded")
	}

	outputPath := t.TempDir()
	options := Options{indentSpaces: 4, preferVarDecl: true, useChannelOperators: true}

	options.testClassNameOverride = getSanitizedImport("lif_internal_test" + PackageSuffix)
	options.testWhiteboxReference = true
	options.testInlineTypeAccess = true
	options.testProductionPath = "example/lif"

	testMethodRenames = make(map[types.Object]bool)
	t.Cleanup(func() { testMethodRenames = nil })

	if _, _, err := convertTestVariant(internal, testFileEntries(internal), outputPath, "go", productionSeed{}, options); err != nil {
		t.Fatal(err)
	}

	data, err := os.ReadFile(filepath.Join(outputPath, "lib_test.cs"))

	if err != nil {
		t.Fatal(err)
	}

	testCs := string(data)

	const stamped = `[GoLocalName("Inner")] internal partial interface TestLocalEmbeddedInterface_Inner /*dyn*/`

	if !strings.Contains(testCs, stamped) {
		t.Errorf("want %q: the inline-access bridge must stamp the interface lift on its declaration:\n%s", stamped, testCs)
	}

	if n := strings.Count(testCs, `[GoLocalName("Inner")]`); n != 1 {
		t.Errorf("want the interface stamp exactly once, got %d", n)
	}

	// CONTROLS: the struct lift keeps its inline stamp, and the anonymous interface lift has no Go name to stamp.
	if !strings.Contains(testCs, `[GoLocalName("Outer")] internal partial struct TestLocalEmbeddedInterface_Outer /*dyn*/`) {
		t.Errorf("control: the struct lift's inline stamp must be unchanged:\n%s", testCs)
	}

	if n := strings.Count(testCs, "[GoLocalName("); n != 2 {
		t.Errorf("control: want exactly 2 stamps (Inner, Outer), got %d:\n%s", n, testCs)
	}
}
