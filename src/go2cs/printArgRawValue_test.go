// printArgRawValue_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the print/println companion of the ToString forward. A converted Go value's C# ToString is its Go
// Error()/String() text (golib's GoMethodText), but Go's print and println write a named type's RAW value and
// never call its methods: println(time.Second) prints 1000000000. So a print argument of a named basic type
// whose VALUE method set holds Error() or String() is passed as its underlying value. The controls are not
// cast: a named type with neither method (its ToString is already its underlying value), a named type whose
// String() has a POINTER receiver (not in the value's method set, so the value's ToString does not forward),
// and a plain basic value.

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

func TestPrintArgumentsKeepGosRawValue(t *testing.T) {
	isolateConverterPathGlobals(t)
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/par\n\ngo 1.23\n")

	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

type Kind uint8

func (k Kind) String() string { return "kind" }

type Code int32

func (c Code) Error() string { return "code" }

type Name string

func (n Name) String() string { return "name" }

type Plain uint16

type Ptr uint32

func (p *Ptr) String() string { return "ptr" }

func main() {
	var k Kind = 3
	var c Code = 4
	var n Name = "n"
	var plain Plain = 5
	var ptr Ptr = 6
	var basic int64 = 7
	println(k, c, n)
	print(plain, ptr, basic, "\n")
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

	mainCs := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "par", "main.cs"))

	for _, want := range []string{
		"println((uint8)(k), (int32)(c), (@string)(n));",
		"print(plain, ptr, basic, ",
	} {
		if !strings.Contains(mainCs, want) {
			t.Errorf("main.cs lacks %q\n--- main.cs ---\n%s", want, mainCs)
		}
	}
}
