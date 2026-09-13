// escapedArrayElementAddress_test.go - Gbtc
// Copyright (c) 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards two emission rules a white-box test conversion of sync/atomic was the first thing in the
// corpus to reach. Both are shapes the production corpus happens not to contain, which is why they
// survived to be found by a Phase-4 measurement rather than by a build.
//
//  1. Taking the address of an ELEMENT of a heap-escaped array LOCAL composed two box spellings
//     into one name that was never declared. The array's default render is already its box's value
//     alias, and the element-address arm prefixed the address operator onto that.
//
//  2. A call whose unsafe.Pointer result is DISCARDED took the (uintptr) construct prefix that
//     types a CONSUMED result, turning a legal expression statement into a cast (CS0201).

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// TestEscapedArrayElementAddress pins the address-of-element emission for an array LOCAL that
// escapes to the heap because a closure captured it. Such a local owns an identity box, and C#
// cannot capture the ref alias that names its value, so every reference inside the closure renders
// through the box. Prefixing the address operator onto THAT render names a box of a box: the Go
// below emitted a doubled prefix and CS0103, one hop deep and two. The element must be aliased
// THROUGH the box instead, which is also what keeps writes through the returned pointer landing in
// the escaped storage rather than in a copy.
func TestEscapedArrayElementAddress(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), `module example.com/eaea

go 1.23
`)
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import "fmt"

func store(p *int32, v int32) { *p = v }

func main() {
	// Both locals escape: the goroutine literal below captures them. sync/atomic's
	// TestStoreLoadSeqCst32 is this exact shape, one and two index hops deep.
	var X [2]int32
	ack := [2][3]int32{{-1, -1, -1}, {-1, -1, -1}}

	done := make(chan bool)

	go func(me int) {
		store(&X[me], 7)
		store(&ack[me][1], 9)
		done <- true
	}(0)

	<-done

	// Read back through the ORIGINAL locals: a copy-boxed element address leaves these unwritten.
	fmt.Println(X[0], ack[0][1])
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

	mainCs := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "eaea", "main.cs"))

	// The defect's own signature, and the one assertion that cannot be satisfied by accident: two
	// address prefixes run together name a box of a box, which is never a declared identifier.
	if strings.Contains(mainCs, AddressPrefix+AddressPrefix) {
		t.Errorf("address prefix applied to an already-boxed render (doubled prefix): %s", mainCs)
	}

	// The single-hop element address goes THROUGH the box.
	if want := AddressPrefix + "X.at<int32>("; !strings.Contains(mainCs, want) {
		t.Errorf("escaped array element address must alias through the box (%q): %s", want, mainCs)
	}

	// The two-hop form chains onto the inner element address rather than indexing the box's value.
	if want := AddressPrefix + "ack.at<array<int32>>("; !strings.Contains(mainCs, want) {
		t.Errorf("escaped 2-D array element address must chain through the box (%q): %s", want, mainCs)
	}
}

// TestDiscardedUnsafePointerResultStatement pins the one syntactic slot where C# admits a call but
// not a cast. A call returning unsafe.Pointer takes a (uintptr) construct prefix so a CONSUMED
// result types correctly; applied to a bare expression statement that prefix has nothing to serve
// and is a compile error. The suppression is by AST-node identity, so a call nested inside the same
// statement, whose value IS consumed, must keep its conversion.
func TestDiscardedUnsafePointerResultStatement(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), `module example.com/dupr

go 1.23
`)
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import (
	"fmt"
	"unsafe"
)

var sink int64

func swapPtr(p unsafe.Pointer) unsafe.Pointer { return p }

func take(p unsafe.Pointer) bool { return p != nil }

func main() {
	// Result DISCARDED - a statement slot. sync/atomic's nil-deref table is a list of these.
	swapPtr(unsafe.Pointer(&sink))

	// Result CONSUMED, as an assignment RHS and as a nested argument: the conversion that types
	// the value is still owed in each.
	q := swapPtr(unsafe.Pointer(&sink))

	fmt.Println(take(q), take(swapPtr(unsafe.Pointer(&sink))))
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

	mainCs := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "dupr", "main.cs"))

	// The defect's signature: a cast standing where a statement belongs. Match the emitted
	// statement form directly - leading whitespace then the cast - so a consumed occurrence
	// elsewhere on a line cannot satisfy or trip this.
	for _, line := range strings.Split(mainCs, "\n") {
		if strings.HasPrefix(strings.TrimSpace(line), "(uintptr)swapPtr(") {
			t.Errorf("discarded unsafe.Pointer result kept its construct cast (CS0201): %s", mainCs)
		}
	}

	// The discarded call still renders - the fix drops the cast, not the statement.
	if !strings.Contains(mainCs, "swapPtr(") {
		t.Errorf("the discarded call must still be emitted: %s", mainCs)
	}

	// A CONSUMED result keeps the conversion: the assignment RHS is the unambiguous witness.
	if !strings.Contains(mainCs, "(uintptr)swapPtr(") {
		t.Errorf("a consumed unsafe.Pointer result must keep its construct cast: %s", mainCs)
	}
}
