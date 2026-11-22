// cgoPointerDoor_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/build"
	"go/token"
	"go/types"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// cgo Phase 3, increment 3.2, seat 1c (docs/phase4/DESIGN-cgo-phase3-marshaling.md 18.11, arm E-1): on the cgo route,
// `*x` and `*x = v` for x a **U, U a type cmd/cgo declares, are GoCgoArgs.LoadPointer and StorePointer. Each shape
// either takes the door or is a named exclusion, and the controls keep their emission. The rows are EXPORTED functions:
// an unexported function whose pointer parameter ref-lowers has no box, which is control (e) and nothing else.
const cgoPointerDoorFixture = `package cgophase2

import "unsafe"

type doorGoNode struct{ next *doorGoNode }

func DoorLoad(pp **_Ctype_struct_go2cs_probe_node) *_Ctype_struct_go2cs_probe_node { return *pp }

func DoorStore(pp **_Ctype_struct_go2cs_probe_node, v *_Ctype_struct_go2cs_probe_node) { *pp = v }

func DoorAddrOfDeref(pp **_Ctype_struct_go2cs_probe_node) **_Ctype_struct_go2cs_probe_node { return &*pp }

func DoorFieldStore(pp **_Ctype_struct_go2cs_probe_node) { (*pp).value = 3 }

func DoorClosure(pp **_Ctype_struct_go2cs_probe_node) func() *_Ctype_struct_go2cs_probe_node {
	return func() *_Ctype_struct_go2cs_probe_node { return *pp }
}

func DoorChar(pp **_Ctype_char) *_Ctype_char { return *pp }

func DoorTuple(pp **_Ctype_struct_go2cs_probe_node, v *_Ctype_struct_go2cs_probe_node) int {
	var n int
	*pp, n = v, 1
	return n
}

func DoorGoType(pp **doorGoNode) *doorGoNode { return *pp }

func DoorUnsafe(pp *unsafe.Pointer) unsafe.Pointer { return *pp }

func entryNext(e *_Ctype_struct_go2cs_probe_node) **_Ctype_struct_go2cs_probe_node { return &e.next }

func DoorWalk(l *_Ctype_struct_go2cs_probe_node) int {
	n := 0
	for r := l; r != nil; r = *entryNext(r) {
		n++
	}
	return n
}

func doorLoweredSet(pp **_Ctype_struct_go2cs_probe_node, v *_Ctype_struct_go2cs_probe_node) { *pp = v }

func doorLoweredCaller(v *_Ctype_struct_go2cs_probe_node) *_Ctype_struct_go2cs_probe_node {
	var local *_Ctype_struct_go2cs_probe_node
	doorLoweredSet(&local, v)
	return local
}
`

func TestCgoPointerDoorTakesTheLoadAndTheStoreOfAPointerToACPointer(t *testing.T) {
	files := cgoPhase2Admitted(t)
	files["door.go"] = cgoPointerDoorFixture

	stderr, output, err := convertCgoOffline(t, files)

	if err != nil {
		t.Fatalf("the conversion failed: %v\n%s", err, stderr)
	}

	var cs strings.Builder

	for _, file := range emittedProjectFiles(t, output) {
		if strings.HasSuffix(file, ".cs") {
			cs.WriteString(strings.ReplaceAll(readConverted(t, output, file), "\r\n", "\n"))
		}
	}

	emission := cs.String()
	body := func(name string) string { return funcBody(t, emission, " "+name+"(") }

	for _, row := range []struct {
		name, want, wantNot, why string
	}{
		{"DoorLoad", "GoCgoArgs.LoadPointer(", "", "a load takes the door"},
		{"DoorStore", "GoCgoArgs.StorePointer(", "ref var v", "a store takes the door, and its value is the pointer v, never v's deref alias"},
		{"DoorAddrOfDeref", "", "LoadPointer(", "&*pp is not a load: it is pp after its nil check"},
		{"DoorFieldStore", "GoCgoArgs.LoadPointer(", "StorePointer(", "(*pp).value = 3 loads *pp through the door, then stores the field"},
		{"DoorClosure", "GoCgoArgs.LoadPointer(", "", "*pp inside a closure takes the door"},
		{"DoorChar", "GoCgoArgs.LoadPointer(", "", "**C.char is on the door (cgoType)"},
		{"DoorTuple", "", "Pointer(", "a tuple target keeps its emission (the floor refuses it over a native slot)"},
		{"DoorGoType", "", "Pointer(", "control (a): **T of a Go type keeps its emission"},
		{"DoorUnsafe", "LoadWord(", "LoadPointer(", "control (d): *unsafe.Pointer keeps W1"},
	} {
		got := body(row.name)

		if row.want != "" && !strings.Contains(got, row.want) {
			t.Errorf("%s: %s, so it holds %q:\n%s", row.name, row.why, row.want, got)
		}

		if row.wantNot != "" && strings.Contains(got, row.wantNot) {
			t.Errorf("%s: %s, so it holds no %q:\n%s", row.name, row.why, row.wantNot, got)
		}
	}

	// Control (e): a parameter that ref-lowers is a C# ref to a managed slot and has no box, so it keeps its emission.
	// Whether doorLoweredSet's parameter lowers is the classifier's call; the row reads which, and holds the door to it.
	set := body("doorLoweredSet")
	lowered := strings.Contains(set, "ref ")

	t.Logf("doorLoweredSet (parameter ref-lowered: %v):\n%s", lowered, set)

	for _, name := range []string{"DoorLoad", "DoorStore", "DoorFieldStore", "DoorClosure", "DoorAddrOfDeref", "DoorTuple", "DoorWalk", "entryNext"} {
		t.Logf("%s:\n%s", name, body(name))
	}

	if lowered && strings.Contains(set, "StorePointer(") {
		t.Errorf("doorLoweredSet: a ref-lowered parameter has no box, so it keeps its emission:\n%s", set)
	}

	if !lowered && !strings.Contains(set, "StorePointer(") {
		t.Errorf("doorLoweredSet: a parameter that does not lower is a box, so its store takes the door:\n%s", set)
	}
}

// Control (b), 1a's D-3b near miss for the door: a package that does not import "C" and declares a type spelled like
// cmd/cgo's keeps `.val` for a **U dereference. The route, not the name, admits the door.
func TestCgoPointerDoorIsNotTakenOffTheCgoRoute(t *testing.T) {
	isolateConverterPathGlobals(t)

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/cgodoornearmiss\n\ngo 1.24\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import "fmt"

type _Ctype_struct_x struct{ next *_Ctype_struct_x }

func load(pp **_Ctype_struct_x) *_Ctype_struct_x { return *pp }

func store(pp **_Ctype_struct_x, v *_Ctype_struct_x) { *pp = v }

func main() {
	var x _Ctype_struct_x
	p := &x
	store(&p, nil)
	fmt.Println(load(&p) == nil)
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

	cs := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "cgodoornearmiss", "main.cs"))

	if strings.Contains(cs, "LoadPointer(") || strings.Contains(cs, "StorePointer(") {
		t.Errorf("a package off the cgo route took the **C.T door:\n%s", cs)
	}
}

// The predicate itself, over constructed types: a DEFINED pointer typedef as the pointee (`*_Ctype_xptr`, cmd/cgo's
// spelling of a C typedef of a pointer) is a named type, not *U, and keeps its emission; `**C.char` is on the door.
func TestCgoPointerDoorPredicateReadsTheTypeShape(t *testing.T) {
	pkg := types.NewPackage("example.com/cgo", "cgo")
	named := func(name string, underlying types.Type) *types.Named {
		return types.NewNamed(types.NewTypeName(token.NoPos, pkg, name, nil), underlying, nil)
	}

	cStruct := named("_Ctype_struct_x", types.NewStruct(nil, nil))
	cChar := named("_Ctype_char", types.Typ[types.Int8])
	cTypedef := named("_Ctype_xptr", types.NewPointer(cStruct))
	goStruct := named("doorGoNode", types.NewStruct(nil, nil))
	cFunc := named("_Cfunc_x", types.NewStruct(nil, nil))

	for _, row := range []struct {
		name string
		typ  types.Type
		want bool
	}{
		{"**C.struct_x", types.NewPointer(types.NewPointer(cStruct)), true},
		{"**C.char", types.NewPointer(types.NewPointer(cChar)), true},
		{"*C.xptr (a defined pointer typedef)", types.NewPointer(cTypedef), false},
		{"**doorGoNode", types.NewPointer(types.NewPointer(goStruct)), false},
		{"*C.struct_x", types.NewPointer(cStruct), false},
		{"***C.struct_x", types.NewPointer(types.NewPointer(types.NewPointer(cStruct))), false},
		{"**_Cfunc_x (not a type prefix)", types.NewPointer(types.NewPointer(cFunc)), false},
	} {
		if got := isCgoPointerPointerType(row.typ); got != row.want {
			t.Errorf("%s: isCgoPointerPointerType = %v, want %v", row.name, got, row.want)
		}
	}
}
