// lazyPackageVar_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards lazyPackageVars (COORD's ruling (A), 2026-10-22): a listed var is declared as a ref property over a holder
// class built on its first read, and package_init.cs does not call it; a listed var whose value could depend on when it
// is built is refused by name. The fixture's table has runtime's malloc-table shape (function values the ordered ctor
// would otherwise relocate, since the functions read a package var), and an unlisted twin of the same shape is the
// control that still relocates.
package main

import (
	"go/build"
	"os"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

const lazyVarFixturePkg = "example.com/lazyvar"

// withLazyPackageVars lists names of the fixture package for the test's duration. The production map is never
// mutated: a copy carries the entry and the original is restored by reference.
func withLazyPackageVars(t *testing.T, names ...string) {
	t.Helper()

	saved := lazyPackageVars
	listed := make(map[string]map[string]bool, len(saved)+1)

	for pkg, vars := range saved {
		listed[pkg] = vars
	}

	listed[lazyVarFixturePkg] = map[string]bool{}

	for _, name := range names {
		listed[lazyVarFixturePkg][name] = true
	}

	lazyPackageVars = listed
	t.Cleanup(func() { lazyPackageVars = saved })
}

// convertLazyVarFixture converts a fixture whose main.go and table.go are given, returning the emitted files by name.
func convertLazyVarFixture(t *testing.T, mainGo, tableGo string) (map[string]string, error) {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module "+lazyVarFixturePkg+"\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), mainGo)
	writeModuleFile(t, filepath.Join(appDir, "table.go"), tableGo)

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{
		goRoot:              goRoot,
		goPath:              build.Default.GOPATH,
		go2csPath:           filepath.Join(root, "out"),
		recurse:             true,
		targetPlatform:      runtime.GOOS + "/" + runtime.GOARCH,
		indentSpaces:        4,
		preferVarDecl:       true,
		useChannelOperators: true,
	}

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	if err := NewModuleConverter(options).ConvertModule(appDir); err != nil {
		return nil, err
	}

	dir := filepath.Join(options.go2csPath, "src", "example.com", "lazyvar")
	files := map[string]string{}

	// A file the converter refused is absent, so only the files that exist are read.
	for _, name := range []string{"main.cs", "table.cs", PackageInitFileName} {
		if data, err := os.ReadFile(filepath.Join(dir, name)); err == nil {
			files[name] = string(data)
		}
	}

	return files, nil
}

const lazyVarFixtureMain = `package main

var calls int

func small(n int) int { calls++; return n }
func large(n int) int { calls += 2; return n * 2 }

func main() {
	println(table[1](3), table[2](3), twin[1](3), calls)
}
`

// Both tables read 'calls' through their functions and are declared in another file, which relocates an unlisted one.
const lazyVarFixtureTable = `package main

var table = [3]func(int) int{
	small,
	small,
	large,
}

var twin = [3]func(int) int{small, large, large}
`

// TestLazyPackageVarIsBuiltOnFirstRead pins the emission: the listed table is a ref property over its holder and is
// absent from package_init.cs; the unlisted twin keeps the relocated field and its initᴛ call.
func TestLazyPackageVarIsBuiltOnFirstRead(t *testing.T) {
	isolateConverterPathGlobals(t)

	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	withLazyPackageVars(t, "table")

	files, err := convertLazyVarFixture(t, lazyVarFixtureMain, lazyVarFixtureTable)

	if err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	table, packageInit := files["table.cs"], files[PackageInitFileName]

	for _, want := range []string{
		"static ref array<Func<nint, nint>> table => ref lazyᴛtable.value;",
		"private static class lazyᴛtable { internal static array<Func<nint, nint>> value = ",
	} {
		if !strings.Contains(table, want) {
			t.Errorf("table.cs lacks %q:\n%s", want, table)
		}
	}

	if strings.Contains(table, "initᴛtable") || strings.Contains(packageInit, "initᴛtable") {
		t.Errorf("the listed table must not be relocated into the ordered ctor:\ntable.cs:\n%s\npackage_init.cs:\n%s", table, packageInit)
	}

	// Control: the unlisted twin of the same shape still relocates, so the fixture does exercise the relocation.
	if !strings.Contains(table, "initᴛtwin()") || !strings.Contains(packageInit, "initᴛtwin();") {
		t.Errorf("control: the unlisted twin must still relocate:\ntable.cs:\n%s\npackage_init.cs:\n%s", table, packageInit)
	}
}

// TestLazyPackageVarRefusesAnInitializerThatCallsOrReads pins the refusal by name: a listed var whose initializer makes
// a call, or reads a package var, would be built at a different value on its first read.
func TestLazyPackageVarRefusesAnInitializerThatCallsOrReads(t *testing.T) {
	isolateConverterPathGlobals(t)

	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	cases := []struct {
		name, table, refusal string
	}{
		{"call", "package main\n\nvar table = [3]func(int) int{small, small, pick()}\n\nvar twin = [3]func(int) int{small, large, large}\n\nfunc pick() func(int) int { return large }\n",
			"its initializer makes a call"},
		{"var read", "package main\n\nvar base = 1\n\nvar table = [3]int{base, 2, 3}\n\nvar twin = [3]func(int) int{small, large, large}\n",
			"its initializer reads the var base"},
	}

	for _, c := range cases {
		t.Run(c.name, func(t *testing.T) {
			withLazyPackageVars(t, "table")

			main := lazyVarFixtureMain

			if c.name == "var read" {
				main = strings.Replace(main, "table[1](3), table[2](3)", "table[1], table[2]", 1)
			}

			// The converter reports a refused file on stderr and does not emit it.
			var files map[string]string
			var err error

			stderr := captureStderr(t, func() { files, err = convertLazyVarFixture(t, main, c.table) })

			if err != nil {
				t.Fatalf("ConvertModule: %v", err)
			}

			if want := "lazyPackageVars entry table cannot be built on its first read: " + c.refusal; !strings.Contains(stderr, want) {
				t.Fatalf("want the refusal naming table (%q) on stderr, got:\n%s", want, stderr)
			}

			if _, emitted := files["table.cs"]; emitted {
				t.Errorf("a refused file must not be emitted:\n%s", files["table.cs"])
			}
		})
	}
}
