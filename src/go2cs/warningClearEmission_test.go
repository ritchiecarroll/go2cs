// warningClearEmission_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the converter-side clears from the stdlib warnings census (CENSUS-stdlib-warnings-2026-10-02):
// emission shapes that compiled, but drew a C# warning because the C# said something other than the Go.
// Each test converts one small module and reads the emitted main.cs.

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// convertWarningFixture converts a one-file main package and returns its emitted main.cs.
func convertWarningFixture(t *testing.T, module, source string) string {
	t.Helper()

	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/"+module+"\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), source)

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

	converter := NewModuleConverter(options)

	if err := converter.ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	return readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", module, "main.cs"))
}

// TestEmptyInterfacePointerCompareUsesAreEqual pins CS0252 (context's `key == &cancelCtxKey`). Go
// compares an empty interface against a pointer by pointer identity. A pointer that reached the `any`
// through a non-empty interface is held as the generated pointer adapter, so C#'s reference `==`
// compares the adapter object with the box and answers false where Go answers true
// (Behavioral/AnyFromInterfacePointerCompare). AreEqual unwraps the adapter first, and the
// non-empty-interface case already routes through it.
func TestEmptyInterfacePointerCompareUsesAreEqual(t *testing.T) {
	mainCs := convertWarningFixture(t, "eipc", `package main

type node struct{ id int }

func lookup(key any, want *node) bool { return key == want }

func miss(key any, want *node) bool { return want != key }

func main() {
	p := &node{id: 1}
	println(lookup(p, p), miss(p, p))
}
`)

	for _, want := range []string{"AreEqual(key, ", "!AreEqual("} {
		if !strings.Contains(mainCs, want) {
			t.Errorf("empty interface against a pointer must compare through AreEqual (%q): %s", want, mainCs)
		}
	}

	for _, reference := range []string{"key == ", "!= key"} {
		if strings.Contains(mainCs, reference) {
			t.Errorf("empty interface against a pointer emitted a C# reference comparison (%q): %s", reference, mainCs)
		}
	}
}

// TestEmptySelectEmitsBareCall pins CS1522 (net/http/httptest's `select {}`). A select with no clauses
// blocks forever; it lowered to `switch (select()) {` with an empty block, which C# warns about and
// which closed on a brace at column zero. The statement is the call itself.
func TestEmptySelectEmitsBareCall(t *testing.T) {
	mainCs := convertWarningFixture(t, "esel", `package main

func serve(flag bool) {
	if flag {
		select {}
	}
}

func block() error {
	select {}
}

func main() {
	serve(false)

	if false {
		_ = block()
	}
}
`)

	if !strings.Contains(mainCs, "        select();") {
		t.Errorf("select {} must emit a bare, indented `select();`: %s", mainCs)
	}

	if strings.Contains(mainCs, "switch (select())") {
		t.Errorf("select {} emitted an empty switch block: %s", mainCs)
	}

	// A value-returning function ending in `select {}` still needs the unreachable trailing return
	// (CS0161 otherwise).
	if want := "    select();\n    return default!;"; !strings.Contains(strings.ReplaceAll(mainCs, "\r\n", "\n"), want) {
		t.Errorf("select {} ending a value-returning function must keep its trailing return (%q): %s", want, mainCs)
	}
}
