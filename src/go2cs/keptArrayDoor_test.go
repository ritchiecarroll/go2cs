// keptArrayDoor_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards DOOR C's unnamed form (docs/phase4/DESIGN-array-pointer-doors.md, section 20; keptArrayDoor.go): a kept
// `(*[N]E)(p)` emits `@unsafe.Pointer.ToArray<E>(p, N)`, and an index through a local the door bound, `f[i]` or
// `(*f)[i]`, emits `f.ElementRef(i)`. ElementRef is the only index form a native window answers; `f.Value[i]`
// refuses by name there. No behavioral package emits the door, so CNR holds nothing for it: without these arms a
// change that made isKeptArrayDoorLocal answer false would put modernc.org/sqlite's page-cache binding back on
// `f.Value[0]` and only that module's own run would show it.
//
// The positives are the sqlite shape (a uintptr operand, a tuple store) and the dereference form. The controls are
// the four declines: a local re-pointed after its binding, a local whose address is taken, an element that holds a
// reference, and a named array. Each keeps `f.Value[i]` (or the pointer conversion), and none emits ElementRef.

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// convertKeptArrayDoorFixture converts the one fixture every arm below reads.
func convertKeptArrayDoorFixture(t *testing.T) string {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/keptdoor\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import (
	"fmt"
	"unsafe"
)

type quad [4]uint32

func sqliteShape(stub uintptr, a, b uintptr) {
	f := (*[2]uintptr)(unsafe.Pointer(stub))
	f[0], f[1] = a, b
}

func derefIndex(p unsafe.Pointer) uint32 {
	f := (*[4]uint32)(p)
	(*f)[1] = 7
	return f[2]
}

func repointed(p, q unsafe.Pointer) {
	f := (*[4]uint32)(p)
	f = (*[4]uint32)(q)
	f[0] = 1
}

func addressTaken(p unsafe.Pointer) **[4]uint32 {
	f := (*[4]uint32)(p)
	f[3] = 2
	return &f
}

func referenceElem(p unsafe.Pointer) {
	f := (*[2]*int)(p)
	f[0] = nil
}

func namedArray(p unsafe.Pointer) {
	f := (*quad)(p)
	f[0] = 1
}

func main() {
	var buf [4]uint32
	sqliteShape(0, 1, 2)
	fmt.Println(derefIndex(unsafe.Pointer(&buf)))
	_ = repointed
	_ = addressTaken
	_ = referenceElem
	_ = namedArray
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

	return readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "keptdoor", "main.cs"))
}

func TestKeptArrayDoorIndexesThroughElementRef(t *testing.T) {
	isolateConverterPathGlobals(t)

	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	mainCs := convertKeptArrayDoorFixture(t)

	// The positives: the door with N from the type, and every index through the bound local as ElementRef.
	for _, arm := range []struct {
		function string
		wants    []string
	}{
		{"sqliteShape", []string{"@unsafe.Pointer.ToArray<uintptr>(", ", 2);", "f.ElementRef(0)", "f.ElementRef(1)"}},
		{"derefIndex", []string{"@unsafe.Pointer.ToArray<uint32>(p, 4);", "ElementRef(1) = 7;", "return f.ElementRef(2);"}},
	} {
		body := emittedFunctionBody(t, mainCs, arm.function)

		for _, want := range arm.wants {
			if !strings.Contains(body, want) {
				t.Errorf("%s: expected %q, got:\n%s", arm.function, want, body)
			}
		}

		if strings.Contains(body, ".Value[") {
			t.Errorf("%s: an index through a door-bound local reads through ElementRef, never .Value[i], got:\n%s", arm.function, body)
		}
	}

	// The declines: each keeps .Value[i] or the pointer conversion, and none indexes through ElementRef.
	for _, arm := range []struct {
		function, keeps, why string
	}{
		{"repointed", "f.Value[0] = 1;", "a local re-pointed after its binding keeps .Value[i]"},
		{"addressTaken", "f.Value[3] = 2;", "a local whose address is taken keeps .Value[i]"},
		{"referenceElem", "@unsafe.Pointer.To<array<", "an element that holds a reference keeps the pointer conversion"},
		{"namedArray", ".Value[0] = 1;", "a named array keeps its own box"},
	} {
		body := emittedFunctionBody(t, mainCs, arm.function)

		if !strings.Contains(body, arm.keeps) {
			t.Errorf("%s: %s, expected %q, got:\n%s", arm.function, arm.why, arm.keeps, body)
		}

		if strings.Contains(body, "ElementRef(") {
			t.Errorf("%s: %s, and it indexed through ElementRef, got:\n%s", arm.function, arm.why, body)
		}

		if arm.function == "referenceElem" || arm.function == "namedArray" {
			if strings.Contains(body, "ToArray<") {
				t.Errorf("%s: %s, and it took the door, got:\n%s", arm.function, arm.why, body)
			}
		}
	}
}
