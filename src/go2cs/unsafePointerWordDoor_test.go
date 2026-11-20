// unsafePointerWordDoor_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the unsafe.Pointer WORD DOOR (W1/W2, docs/phase4/DESIGN-cgo-phase3-marshaling.md 18.3-18.7, seat S-B1):
// `*p` and `*p = v` for p a *unsafe.Pointer are emitted as unsafe.Pointer.LoadWord / StoreWord, and NOTHING ELSE is.
// The scope half pays for the file: a pointer to another pointee, a tuple assignment target, and a *unsafe.Pointer
// parameter's value alias (which the door makes dead) are each pinned.

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

func TestUnsafePointerWordDoor(t *testing.T) {
	isolateConverterPathGlobals(t)
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/worddoor\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import "unsafe"

func load(k unsafe.Pointer) unsafe.Pointer {
	return *(*unsafe.Pointer)(k)
}

func store(dst *unsafe.Pointer, val unsafe.Pointer) {
	*dst = val
}

func swapBoth(a, b *unsafe.Pointer) {
	*a, *b = *b, *a
}

func notAWord(p *uintptr, q *int) {
	*p = 7
	*q = int(*p)
}

func main() {
	var w uintptr
	var x int
	store((*unsafe.Pointer)(unsafe.Pointer(&w)), nil)
	notAWord(&w, &x)
	var a, b unsafe.Pointer
	swapBoth(&a, &b)
	println(load(unsafe.Pointer(&w)) == nil, x)
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

	converter := NewModuleConverter(options)

	if err := converter.ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	mainCs := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "worddoor", "main.cs"))

	// The POSITIVE half: the load and the store are calls on the box.
	if !strings.Contains(mainCs, "return @unsafe.Pointer.LoadWord(") {
		t.Errorf("`*(*unsafe.Pointer)(k)` must be the word door's load:\n%s", mainCs)
	}

	if !strings.Contains(mainCs, "@unsafe.Pointer.StoreWord(dst, val);") {
		t.Errorf("`*dst = val` must be the word door's store on the parameter's box:\n%s", mainCs)
	}

	// The parameter's value alias is dead by construction: the door takes the box.
	if strings.Contains(mainCs, "ref var dst =") {
		t.Errorf("a *unsafe.Pointer parameter must get no value alias:\n%s", mainCs)
	}

	// The SCOPE half.
	if got := strings.Count(mainCs, "StoreWord("); got != 1 {
		t.Errorf("exactly one store is the word door's (the single assignment), got %d:\n%s", got, mainCs)
	}

	// A REF-LOWERED *unsafe.Pointer parameter (every caller passes &local) is a `ref` to a CLR Pointer slot, never a
	// word: it keeps the plain ref emission and is no door's.
	if !strings.Contains(mainCs, "(a, b) = (b, a);") {
		t.Errorf("a ref-lowered *unsafe.Pointer parameter keeps its ref emission:\n%s", mainCs)
	}

	// Every other pointee keeps its emission: the one load is load's.
	if got := strings.Count(mainCs, "LoadWord("); got != 1 {
		t.Errorf("exactly one load is the word door's, got %d:\n%s", got, mainCs)
	}
}
