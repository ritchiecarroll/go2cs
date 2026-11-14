// typedPointerDoor_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the TYPED DOOR emission (docs/phase4/DESIGN-interior-pointer-root.md 4.1): Go's `(*T)(p)` over an
// unsafe.Pointer VALUE emits `@unsafe.Pointer.To<T>(p)`, which hands golib the pointer and what it carries. The
// form it replaces, `(ж<T>)(uintptr)(p)`, cast the pointer to a number first, so a pointer derived from Go
// memory (a base plus an offset) came back as a box that kept nothing.
//
// The conversion reaches the emission by two paths, and each has an arm: the CALL path, where a parenthesized
// star type is the callee (most sites), and the CONVERSION path, the operand of a dereference. The CONTROLS
// carry the scope:
//
//   - a span view `(*[N]T)(p)[lo:hi]` keeps the cast text its lowering cuts, and reads through a raw pointer;
//   - a managed reinterpret `(*U)(unsafe.Pointer(t))` keeps its own route, ahead of the door.

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// convertTypedPointerDoorFixture converts the one fixture every arm below reads.
func convertTypedPointerDoorFixture(t *testing.T) string {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/typeddoor\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import (
	"fmt"
	"unsafe"
)

type pair struct {
	a, b int64
}

type named *int32

// POSITIVE 1 — the call path: a parenthesized star type as the callee.
func convert(p unsafe.Pointer) *int32 {
	return (*int32)(p)
}

// POSITIVE 2 — the operand of a dereference, read and written.
func read(p unsafe.Pointer) int32 {
	return *(*int32)(p)
}

func write(p unsafe.Pointer, v int32) {
	*(*int32)(p) = v
}

// POSITIVE 3 — the interior pointer the door exists for: a base plus an offset.
func second(q *pair) *int64 {
	return (*int64)(unsafe.Add(unsafe.Pointer(q), 8))
}

// POSITIVE 4 — a named type over a pointer takes the door, then its own operator.
func toNamed(p unsafe.Pointer) named {
	return named(p)
}

// CONTROL 1 — a span view keeps the cast its lowering cuts.
func view(p unsafe.Pointer, n int) []byte {
	return (*[1 << 20]byte)(p)[:n]
}

// CONTROL 2 — a managed reinterpret keeps its own route.
func bits(f *float32) *uint32 {
	return (*uint32)(unsafe.Pointer(f))
}

func main() {
	x := int32(7)
	q := &pair{1, 2}
	f := float32(1)
	p := unsafe.Pointer(&x)
	write(p, 9)
	fmt.Println(*convert(p), read(p), *second(q), *toNamed(p), len(view(p, 4)), *bits(&f))
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

	return readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "typeddoor", "main.cs"))
}

func TestTypedPointerDoorReplacesTheNumberCast(t *testing.T) {
	isolateConverterPathGlobals(t)
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	mainCs := convertTypedPointerDoorFixture(t)

	// The positives: the door with the pointee and the pointer itself, and no cast to a number around it.
	for _, arm := range []struct {
		function, door string
	}{
		{"convert", "return @unsafe.Pointer.To<int32>(p);"},
		{"read", "@unsafe.Pointer.To<int32>(p)"},
		{"write", "@unsafe.Pointer.To<int32>(p)"},
		{"second", "@unsafe.Pointer.To<int64>(@unsafe.Add("},
		{"toNamed", "(named)@unsafe.Pointer.To<int32>(p)"},
	} {
		body := emittedFunctionBody(t, mainCs, arm.function)

		if !strings.Contains(body, arm.door) {
			t.Errorf("%s: expected the typed door %q, got:\n%s", arm.function, arm.door, body)
		}

		if strings.Contains(body, ")(uintptr)(") {
			t.Errorf("%s: the pointer must reach the door as a pointer, not through a cast to a number, got:\n%s", arm.function, body)
		}
	}

	// CONTROL 1 — the span view reads through a raw pointer over the number, and takes no door.
	if body := emittedFunctionBody(t, mainCs, "view"); !strings.Contains(body, "new ReadOnlySpan<byte>((byte*)(uintptr)(p), ") || strings.Contains(body, "@unsafe.Pointer.To<") {
		t.Errorf("view: a span view must keep its raw-pointer lowering and take no door, got:\n%s", body)
	}

	// CONTROL 2 — the managed reinterpret answers first.
	if body := emittedFunctionBody(t, mainCs, "bits"); !strings.Contains(body, ".Reinterpret<") || strings.Contains(body, "@unsafe.Pointer.To<") {
		t.Errorf("bits: a managed reinterpret must keep its own route, got:\n%s", body)
	}
}
