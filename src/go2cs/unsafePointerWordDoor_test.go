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
	"regexp"
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

	// THE NARROWING, both ways (unsafePointerWordSlotIsManaged): a provably managed slot keeps master's emission, and a
	// slot native memory can reach stays on the doors. Its own file, so main.cs's counts above stay its own.
	writeModuleFile(t, filepath.Join(appDir, "narrow.go"), `package main

import "unsafe"

var global unsafe.Pointer
var yield = &global

// MANAGED: the converter allocated the slot, so no door.
func managedFuncRead(f func()) uintptr                 { return uintptr(*(*unsafe.Pointer)(unsafe.Pointer(&f))) }
func managedFuncStore(f func(), v unsafe.Pointer)      { *(*unsafe.Pointer)(unsafe.Pointer(&f)) = v }
func managedDoubleFunc(fn func()) unsafe.Pointer       { return **(**unsafe.Pointer)(unsafe.Pointer(&fn)) }
func managedParamRead(u unsafe.Pointer) unsafe.Pointer { return *(*unsafe.Pointer)(unsafe.Pointer(&u)) }
func managedParamStore(u, v unsafe.Pointer) unsafe.Pointer {
	*(*unsafe.Pointer)(unsafe.Pointer(&u)) = v
	return u
}
func managedLocalRead() unsafe.Pointer {
	var l unsafe.Pointer
	return *(*unsafe.Pointer)(unsafe.Pointer(&l))
}

// NATIVE-REACHABLE: a number, a package variable, a package variable's address. Always a door's.
func nativeNumberRead(k uintptr) unsafe.Pointer      { return *(*unsafe.Pointer)(unsafe.Pointer(k)) }
func nativeNumberStore(k uintptr, v unsafe.Pointer)  { *(*unsafe.Pointer)(unsafe.Pointer(k)) = v }
func nativePackageVarRead() unsafe.Pointer           { return *yield }
func nativePackageVarStore(v unsafe.Pointer)         { *yield = v }
func nativeGlobalAddressRead() unsafe.Pointer        { return *(*unsafe.Pointer)(unsafe.Pointer(&global)) }

// NATIVE-REACHABLE through a local: **&x reads the slot OUT of x, a *unsafe.Pointer that may hold a native box.
func nativeStarredLocal(k uintptr) unsafe.Pointer {
	x := (*unsafe.Pointer)(unsafe.Pointer(k))
	return **&x
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

	narrowCs := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "worddoor", "narrow.cs"))

	bodyOf := func(name string) string {
		match := regexp.MustCompile(`(?m)^( *)[^\n]*\b` + name + `\([^\n]*\n(?s:.*?)^( *)\}\s*$`).FindString(narrowCs)

		if match == "" {
			match = regexp.MustCompile(`(?m)^[^\n]*\b` + name + `\([^\n]*$`).FindString(narrowCs)
		}

		if match == "" {
			t.Fatalf("%s: not found in the emission:\n%s", name, narrowCs)
		}

		return match
	}

	for _, name := range []string{"managedFuncRead", "managedFuncStore", "managedDoubleFunc", "managedParamRead", "managedParamStore", "managedLocalRead"} {
		if body := bodyOf(name); strings.Contains(body, "LoadWord(") || strings.Contains(body, "StoreWord(") {
			t.Errorf("%s: a provably managed slot is no door's:\n%s", name, body)
		}
	}

	// A STORE through shape (b) writes the local's or parameter's own heap box through its slot, never Value, which would
	// read the held nil first (COORD ruling on P3's review, 2026-11-20); a load through (b) reads ~Ꮡu, the slot too.
	if body := bodyOf("managedParamStore"); !strings.Contains(body, "(Ꮡu).ValueSlot = v;") || strings.Contains(body, ".Value =") {
		t.Errorf("managedParamStore: a store through &param writes through ValueSlot:\n%s", body)
	}

	if body := bodyOf("managedParamRead"); !strings.Contains(body, "~Ꮡu") {
		t.Errorf("managedParamRead: a load through &param reads ~Ꮡu:\n%s", body)
	}

	// Shape (a)'s store through the carrying arm wrote into a fresh Ꮡ(OfFunc(f)) box that nothing reads: the BOARD's
	// 2026-11-20 LOST WRITE ("a store of a pointer word INTO a func variable lands in a fresh box"). S-B1 (d02c2dda3b)
	// pinned that store here as out of the word door's scope. TC-13's w4 (funcWordOperations.go) cures it, so the pin
	// moved with the cure: the store is f's own, through FuncOfWord, and never a fresh box's .Value.
	if body := bodyOf("managedFuncStore"); !strings.Contains(body, "f = @unsafe.Pointer.FuncOfWord<Action>(v);") || strings.Contains(body, ".Value =") {
		t.Errorf("managedFuncStore: a pointer word stored into a func variable is w4's (TC-13), never a fresh box's:\n%s", body)
	}

	for name, door := range map[string]string{
		"nativeNumberRead":        "LoadWord(",
		"nativeNumberStore":       "StoreWord(",
		"nativePackageVarRead":    "LoadWord(",
		"nativePackageVarStore":   "StoreWord(",
		"nativeGlobalAddressRead": "LoadWord(",
		"nativeStarredLocal":      "LoadWord(",
	} {
		if body := bodyOf(name); !strings.Contains(body, door) {
			t.Errorf("%s: a slot native memory can reach stays on the door (%s):\n%s", name, door, body)
		}
	}
}
