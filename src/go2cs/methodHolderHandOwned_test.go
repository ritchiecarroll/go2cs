// methodHolderHandOwned_test.go - Gbtc
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
	"strings"
	"testing"
)

// A hand-owned file replaces a Go file's declarations with its own, and may drop a type the Go file
// declares (internal/sync's hashtriemap.cs keeps HashTrieMap and drops entry, indirect and node). A
// holder's declaration names its type in `typeof`, so package_info.cs declares the holder of a type the
// hand-owned file's Go source has methods on only where the package's C# sources declare that type.
func TestHandOwnedFileDeclaresHoldersOnlyForTheTypesItKeeps(t *testing.T) {
	isolateConverterPathGlobals(t)
	if testing.Short() {
		t.Skip("integration test: loads the fixture package via go/packages")
	}

	pkgDir, outDir, options := incrementalFixture(t)

	writeModuleFile(t, filepath.Join(pkgDir, "c.go"), `package incr

type Kept struct{ n int }

func (k *Kept) Bump() { k.n++ }

type dropped struct{ n int }

func (d *dropped) bump() { d.n++ }

func Use() int {
	var k Kept
	k.Bump()
	var d dropped
	d.bump()
	return k.n + d.n
}
`)

	if err := processConversion(pkgDir, true, outDir, options); err != nil {
		t.Fatalf("first conversion: %v", err)
	}

	packageInfo := filepath.Join(outDir, "package_info.cs")
	before := readNormalized(t, packageInfo)

	for _, holder := range []string{"incrᴛKept", "incrᴛdropped", "incrᴛPoint"} {
		if !strings.Contains(before, "partial class "+holder+";") {
			t.Fatalf("the converted package declares no holder %s, so the fixture does not reach the case:\n%s", holder, before)
		}
	}

	writeModuleFile(t, filepath.Join(outDir, "c.cs"), "// c.cs - hand-owned\n[module: go.GoManualConversion]\n\n"+
		"namespace go.example.com;\n\npartial class incr_package {\n\n[GoType] public partial struct Kept {\n    internal nint n;\n}\n\n"+
		"} partial class incrᴛKept {\n\n[GoRecv] public static void Bump(this ref Kept k) { k.n++; }\n\n"+
		"} partial class incr_package {\n\npublic static nint Use() => 0;\n\n} // end incr_package\n")

	if err := processConversion(pkgDir, true, outDir, options); err != nil {
		t.Fatalf("second conversion: %v", err)
	}

	after := readNormalized(t, packageInfo)

	if strings.Contains(after, "partial class incrᴛdropped;") {
		t.Errorf("package_info.cs declares the holder of dropped, a type no C# source declares (CS0426):\n%s", after)
	}

	for _, holder := range []string{"incrᴛKept", "incrᴛPoint"} {
		if !strings.Contains(after, "partial class "+holder+";") {
			t.Errorf("package_info.cs lost the holder %s of a type the package's C# sources declare:\n%s", holder, after)
		}
	}
}

func readNormalized(t *testing.T, path string) string {
	t.Helper()

	content, err := os.ReadFile(path)

	if err != nil {
		t.Fatal(err)
	}

	return strings.ReplaceAll(string(content), "\r\n", "\n")
}
