// uint8SliceLiteralConversion_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards a string-literal conversion to a slice spelled with the ALIASED element name (R's mapstructure reading,
// B5, COORD 2026-10-03): `[]uint8("foo")` is the same conversion as `[]byte("foo")`, but the element-decoding
// rules were keyed on the spelling `[]byte`/`[]rune`, so every position emitted `slice<uint8>("foo")`, a
// System.String with no conversion to the slice (CS1503). The rules now key on the element's basic kind and keep
// the source's spelling. The controls hold `[]byte`, a named slice and an alias target unchanged.

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

func TestAStringLiteralConvertsToASliceSpelledWithTheAliasedElement(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/u8c\n\ngo 1.23\n")

	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import "fmt"

type buf []uint8

type B = []byte

func take(b []uint8) int { return len(b) }

var g = []uint8("glob")

func main() {
	a := []uint8("foo")
	b := []byte("bar")
	var c buf = buf("baz")
	r := []int32("héllo")
	s := []uint8("a" + "b")
	fmt.Println(len(a), len(b), len(c), take([]uint8("qux")), len(g), len(r), len(s), len(B("ali")))
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

	mainCs := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "u8c", "main.cs"))

	// Every position takes the u8 span (or the @string decode for runes and concatenations), spelled as written.
	for _, want := range []string{
		`internal static slice<uint8> g = slice<uint8>("glob"u8);`,
		`var a = slice<uint8>("foo"u8);`,
		`take(slice<uint8>("qux"u8))`,
		`var r = slice<int32>((@string)"héllo");`,
		`var s = slice<uint8>((@string)("a"u8 + "b"u8));`,
	} {
		if !strings.Contains(mainCs, want) {
			t.Errorf("want %q: []uint8 is []byte and []int32 is []rune (CS1503):\n%s", want, mainCs)
		}
	}

	// CONTROLS: []byte, a named slice and an alias target render as before.
	for _, want := range []string{
		`var b = slice<byte>("bar"u8);`,
		`buf c = ((buf)slice<uint8>((@string)"baz"u8));`,
		`((B)"ali"u8)`,
	} {
		if !strings.Contains(mainCs, want) {
			t.Errorf("control: want %q unchanged:\n%s", want, mainCs)
		}
	}
}
