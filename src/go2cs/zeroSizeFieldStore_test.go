// zeroSizeFieldStore_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the STORE to a named zero-size field of a struct under Go's explicit layout (A17): the field
// is readonly in the emission, because it shares its offset with the field Go puts there and C# would
// write its one byte over that neighbour. Go stores nothing, so an assignment is lowered through the
// generated accessor, `T.Ꮡf(ref <base>) = <rhs>`, which answers golib's shared zero-size slot: the base
// is still evaluated (a nil pointer or an out-of-range index still panics), the right side still runs,
// and no byte of the struct is written. The controls carry as much weight: a non-zero-size field of the
// same struct, and a zero-size field of a struct that takes no explicit layout, keep the plain store.

package main

import (
	"go/build"
	"path/filepath"
	"regexp"
	"runtime"
	"strings"
	"testing"
)

func TestZeroSizeFieldStoreGoesThroughTheSharedSlot(t *testing.T) {
	isolateConverterPathGlobals(t)
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/zsstore\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import "fmt"

type nocopy struct{}

// Explicit layout: Z is readonly at offset 0, over V.
type Carrier struct {
	Z nocopy
	V uint64
}

type Outer struct {
	C Carrier
}

// A managed field keeps the struct out of the layout arc, so its zero-size field stays writable.
type Managed struct {
	Z nocopy
	S string
}

func side() nocopy  { return nocopy{} }
func sideU() uint64 { return 1 }

func main() {
	var x Carrier
	p := &x
	var arr [2]Carrier
	var o Outer
	s := []Carrier{{}, {}}
	var m Managed
	var n int

	x.Z = side()
	p.Z = side()
	arr[1].Z = side()
	o.C.Z = side()
	s[1].Z = side()
	x.Z, n = side(), 7

	x.V = sideU()
	m.Z = side()

	fmt.Println(x.V, n, m.S)
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

	mainCs := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "zsstore", "main.cs"))

	for _, want := range []string{
		`Carrier.ᏑZ(ref x) = side();`,
		`Carrier.ᏑZ(ref p.Value) = side();`,
		`Carrier.ᏑZ(ref arr[1]) = side();`,
		`Carrier.ᏑZ(ref o.C) = side();`,
		`Carrier.ᏑZ(ref s[1]) = side();`,
	} {
		if !strings.Contains(mainCs, want) {
			t.Errorf("a store to the readonly zero-size field must go through the shared slot: want %q in the emission:\n%s", want, mainCs)
		}
	}

	// The tuple target takes the same lowering; its neighbour target is untouched.
	if !regexp.MustCompile(`\(Carrier\.ᏑZ\(ref x\), n\) = \(side\(\), 7\)`).MatchString(mainCs) {
		t.Errorf("the tuple's zero-size target must go through the shared slot; emission:\n%s", mainCs)
	}

	// CONTROLS: a sized field of the same struct, and a zero-size field outside the layout arc.
	for _, want := range []string{`x.V = sideU();`, `m.Z = side();`} {
		if !strings.Contains(mainCs, want) {
			t.Errorf("control moved: want %q in the emission:\n%s", want, mainCs)
		}
	}

	// No store may be left on the readonly field itself (CS0191).
	if bad := regexp.MustCompile(`(?m)^\s*(x|p\.Value|arr\[1\]|o\.C|s\[1\])\.Z = `).FindString(mainCs); bad != "" {
		t.Errorf("a plain store to the readonly zero-size field survives (%q); emission:\n%s", strings.TrimSpace(bad), mainCs)
	}
}
