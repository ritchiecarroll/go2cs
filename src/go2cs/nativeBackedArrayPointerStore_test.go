// nativeBackedArrayPointerStore_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// Guards Q58's write half: Go's write-barrier-dodging store into a POINTER-TO-ARRAY slot mints a
// native-backed pointer, and NOTHING ELSE does.
//
// The store `*(*uintptr)(unsafe.Pointer(&x)) = uintptr(p)` writes a raw address into whatever slot
// `x` names, to keep the write off Go's write barrier. Converted literally it takes Reinterpret's
// ALIASING arm and lands the raw word in a slot whose static type is a MANAGED reference; the next
// read of that slot reinterprets element bytes as an array header and the process dies in the
// prestub with no managed frame (eight runtime page-allocator rows, exit 139, blank stderr).
//
// The guard has two halves and the SECOND is the one that pays for the file. The positive half
// asserts the mint and its length. The scope half asserts that the same syntax into a
// pointer-to-STRUCT slot and into an unsafe.Pointer slot is left exactly where it was, and that an
// ordinary store into the very same pointer-to-array field is untouched -- because the predicate
// keys on the DESTINATION TYPE, and a syntax-only match would silently rewrite 19 other sites in
// the pinned GOROOT that have no measured failing row between them.

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// TestNativeBackedArrayPointerStore pins the emission of the write-barrier-dodging store, in both
// directions: the pointer-to-array destination mints, every other destination does not.
func TestNativeBackedArrayPointerStore(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/wbdodge\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import "unsafe"

type payload struct{ a, b uint64 }

type table struct {
	// The SEAM's shape, runtime/mpagealloc.go:420 in miniature: a slot whose static type is a
	// pointer to ARRAY. The array's length is the N the mint must carry.
	chunks [4]*[8]payload

	// The same dodge into a pointer-to-STRUCT slot. Deliberately NOT served: same class, one type
	// kind over, and no measured failing row.
	head *payload

	// ...and into an unsafe.Pointer slot, which has its own established emission.
	raw unsafe.Pointer
}

func store(tbl *table, i int, addr unsafe.Pointer) {
	*(*uintptr)(unsafe.Pointer(&tbl.chunks[i])) = uintptr(addr)
	*(*uintptr)(unsafe.Pointer(&tbl.head)) = uintptr(addr)
	*(*uintptr)(unsafe.Pointer(&tbl.raw)) = uintptr(addr)

	// The ORDINARY store into the very same pointer-to-array field -- no dodge, no reinterpret.
	// It must be left alone, which a syntax-blind rule would not do.
	tbl.chunks[i] = (*[8]payload)(addr)
}

func main() {
	var tbl table
	var block [8]payload

	store(&tbl, 0, unsafe.Pointer(&block))
	println(tbl.chunks[0] != nil, tbl.head != nil, tbl.raw != nil)
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

	outDir := filepath.Join(options.go2csPath, "src", "example.com", "wbdodge")
	mainCs := readGenerated(t, filepath.Join(outDir, "main.cs"))

	// The POSITIVE half. The length is read from the DESTINATION TYPE, not from the store site, so
	// the 8 here is the `[8]payload` in the field declaration.
	if !strings.Contains(mainCs, "NativeArrayPointer<payload>(") {
		t.Errorf("the pointer-to-array destination must mint a native-backed pointer:\n%s", mainCs)
	}

	if !strings.Contains(mainCs, ", 8);") {
		t.Errorf("the mint must carry the destination array's own length (8):\n%s", mainCs)
	}

	// It is a store INTO the slot, through the element pointer -- not a new expression somewhere.
	if !strings.Contains(mainCs, ".Value = NativeArrayPointer<payload>(") {
		t.Errorf("the mint must be assigned into the slot it replaces:\n%s", mainCs)
	}

	// The SCOPE half, and the reason this file exists. Exactly ONE of the four stores in the
	// fixture may mint: the three others are a pointer-to-struct destination, an unsafe.Pointer
	// destination, and an ordinary assignment to the same pointer-to-array field. A predicate that
	// matched on the SYNTAX would take all three of the first kind; one that ignored the dodge
	// would take the fourth.
	if got := strings.Count(mainCs, "NativeArrayPointer<"); got != 1 {
		t.Errorf("exactly one store may mint (the pointer-to-array destination), got %d:\n%s", got, mainCs)
	}

	// The pointer-to-STRUCT destination keeps the aliasing reinterpret it has today. Asserted
	// positively rather than left to the count above, so that a change which stops emitting it at
	// all -- rather than merely not minting -- is caught here too.
	if !strings.Contains(mainCs, "uintptr>()") {
		t.Errorf("a non-array destination must keep its existing reinterpret emission:\n%s", mainCs)
	}
}
