// nonAsciiPtrCastSlice_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards convSliceExpr.go's pointer-cast slice: `(*[N]T)(p)[lo:hi]` cuts the cast prefix `(ж<T>)` off the
// converted expression to find the base pointer. The prefix was measured in BYTES and cut in RUNES, so a type
// name holding a non-ASCII character (a user's `élément`, or go2cs's own `Δ` import renames, as in
// modernc.org/sqlite's `(*libc.RawMem)(unsafe.Pointer(p))[:n:n]`) lost one character of the base: CS1003.
// The non-ASCII fixture must emit exactly what its ASCII twin emits, with the name substituted.

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

func convertPtrCastSliceFixture(t *testing.T, elem string) string {
	t.Helper()
	isolateConverterPathGlobals(t)
	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/ptrcastslice\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), strings.ReplaceAll(`package main

import "unsafe"

type ELEM byte

func view(p uintptr) []ELEM {
	return (*[16]ELEM)(unsafe.Pointer(p))[:8:8]
}

func viewFrom(p uintptr) []ELEM {
	return (*[16]ELEM)(unsafe.Pointer(p))[2:8]
}

func main() {
	src := make([]ELEM, 16)
	println(len(view(uintptr(unsafe.Pointer(&src[0])))), len(viewFrom(uintptr(unsafe.Pointer(&src[0])))))
}
`, "ELEM", elem))

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

	captureStderr(t, func() {
		if err := NewModuleConverter(options).ConvertModule(appDir); err != nil {
			t.Fatalf("ConvertModule: %v", err)
		}
	})

	return readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "ptrcastslice", "main.cs"))
}

func TestPointerCastSliceKeepsANonASCIITypeNameWhole(t *testing.T) {
	ascii := convertPtrCastSliceFixture(t, "element")
	nonASCII := convertPtrCastSliceFixture(t, "élément")

	// Control: the fixture really takes the pointer-cast span branch, twice, or the test proves nothing.
	if got := strings.Count(ascii, "new ReadOnlySpan<element>((element*)"); got != 2 {
		t.Fatalf("control: the ASCII fixture must emit the pointer-cast span twice, emitted %d:\n%s", got, ascii)
	}

	if want := strings.ReplaceAll(ascii, "element", "élément"); nonASCII != want {
		t.Errorf("the non-ASCII fixture must emit its ASCII twin's C# with the name substituted.\nwant:\n%s\ngot:\n%s", want, nonASCII)
	}
}
