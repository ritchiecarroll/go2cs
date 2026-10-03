// elidedAnyCompositeElems_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards an ELIDED inner slice or array literal whose element type is an EMPTY interface (BurntSushi/toml's
// encode test, COORD 2026-10-03): `[][2]any{{1, 2}, {"a", "b"}}`. Its elements box at their Go type exactly as
// the typed `[]any{...}` path boxes them. Before, a string literal stayed a `u8` span, which has no conversion
// to `object` (CS0029), and an untyped constant boxed as C# `int`, so `x.(int)` read false. The controls hold
// an elided literal of a concrete element type and a typed `[]any` literal unchanged.

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

func TestAnElidedEmptyInterfaceCompositeBoxesItsElementsAtTheirGoTypes(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/eia\n\ngo 1.23\n")

	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

func main() {
	var p *int
	a := [][2]any{{1, 2}, {"a", "b"}}
	b := [2][]any{{1.5, 'x'}, {"c" + "d", true}}
	c := []*[2]any{{1, "p"}}
	d := [][]any{{p}}
	e := [][]int{{1, 2}}
	f := [][]string{{"x"}}
	g := []any{"flat", 3}
	println(len(a), len(b), len(c), len(d), len(e), len(f), len(g))
}
`)

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
		t.Fatalf("ConvertModule: %v", err)
	}

	mainCs := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "eia", "main.cs"))

	// Each element boxes at its Go type: a string literal through @string, an untyped constant through its
	// default type, a pointer as its box.
	for _, want := range []string{
		`new any[]{(nint)(1), (nint)(2)}.array()`,
		`new any[]{(@string)"a"u8, (@string)"b"u8}.array()`,
		`new any[]{1.5D, (rune)'x'}.slice()`,
		`new any[]{(@string)("c"u8 + "d"u8), true}.slice()`,
		`Ꮡ(new any[]{(nint)(1), (@string)"p"u8}.array())`,
		`new any[]{p.OrTypedNil()}.slice()`,
	} {
		if !strings.Contains(mainCs, want) {
			t.Errorf("want %q: an elided empty-interface element boxes at its Go type:\n%s", want, mainCs)
		}
	}

	// CONTROLS: an elided literal of a concrete element type, and a typed `[]any` literal, keep their emission.
	for _, want := range []string{
		`new nint[]{1, 2}.slice()`,
		`new @string[]{"x"u8}.slice()`,
		`new any[]{(@string)"flat"u8, (nint)(3)}.slice()`,
	} {
		if !strings.Contains(mainCs, want) {
			t.Errorf("control: want %q unchanged:\n%s", want, mainCs)
		}
	}
}
