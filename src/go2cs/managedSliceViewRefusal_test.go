// managedSliceViewRefusal_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards managedSliceViewRefusal (convSliceExpr.go): a slice view `(*[N]T)(p)[lo:hi:max]` over a
// MANAGED element T that no door can alias is emitted as a refusal by name, never as the
// span-over-address view, which would read the words behind p as fabricated managed references (C#
// CS8500; runtime's itabInit is the corpus's one site). Three arms: managed and not an aliasing pair
// (the refusal); managed and a pair (arrayPointerAliasEmission's door, synthetic -- the corpus has
// none); and an unmanaged element (today's span form, the control).

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

func convertManagedSliceViewFixture(t *testing.T) string {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/managedview\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import "unsafe"

// REFUSAL -- uintptr words viewed as unsafe.Pointer (itabInit's shape): no aliasing pair.
func wordsAsPointers(words []uintptr, n int) []unsafe.Pointer {
	return (*[1 << 16]unsafe.Pointer)(unsafe.Pointer(&words[0]))[:n:n]
}

// DOOR -- *int elements viewed as *int: an identical element, so the alias door serves it.
func pointersAsPointers(ptrs *[4]*int, n int) []*int {
	return (*[4]*int)(unsafe.Pointer(&ptrs[0]))[:n:n]
}

// CONTROL -- an unmanaged element keeps the span-over-address view.
func bytesAsWords(b []byte, n int) []uint16 {
	return (*[1 << 16]uint16)(unsafe.Pointer(&b[0]))[:n:n]
}

func main() {
	words := []uintptr{1, 2}
	var ptrs [4]*int
	b := []byte{1, 2, 3, 4}
	_ = pointersAsPointers(&ptrs, 2)
	_ = bytesAsWords(b, 2)
	if len(words) > 2 {
		_ = wordsAsPointers(words, 2)
	}
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

	return readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "managedview", "main.cs"))
}

// emittedFunctionText returns the emitted text of the function named name, from its declaration line to the
// first closing brace at column 0.
func emittedFunctionText(t *testing.T, cs, name string) string {
	t.Helper()

	cs = strings.ReplaceAll(cs, "\r\n", "\n")
	start := strings.Index(cs, " "+name+"(")

	if start < 0 {
		t.Fatalf("%s: no declaration in:\n%s", name, cs)
	}

	end := strings.Index(cs[start:], "\n}\n")

	if end < 0 {
		t.Fatalf("%s: no closing brace", name)
	}

	return cs[start : start+end]
}

func TestManagedSliceViewRefusesWithoutAnAliasingPair(t *testing.T) {
	mainCs := convertManagedSliceViewFixture(t)

	refused := emittedFunctionText(t, mainCs, "wordsAsPointers")

	if !strings.Contains(refused, `throw panic("go2cs: a slice view (*[N]unsafe.Pointer)(p)[...] over a managed element has no managed aliasing pair (main.go:7)")`) {
		t.Errorf("a managed view with no aliasing pair must refuse by name:\n%s", refused)
	}

	if strings.Contains(refused, "ReadOnlySpan<") {
		t.Errorf("the refused view must not also read the memory as references:\n%s", refused)
	}

	door := emittedFunctionText(t, mainCs, "pointersAsPointers")

	if !strings.Contains(door, ".AliasPointer(") || strings.Contains(door, "throw panic(") {
		t.Errorf("an identical managed element takes the alias door, not the refusal:\n%s", door)
	}

	control := emittedFunctionText(t, mainCs, "bytesAsWords")

	if !strings.Contains(control, "new slice<uint16>(new ReadOnlySpan<uint16>((uint16*)") || strings.Contains(control, "throw panic(") {
		t.Errorf("an unmanaged element keeps the span-over-address view:\n%s", control)
	}
}
