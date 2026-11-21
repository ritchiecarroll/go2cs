// arrayPointerElementDoor_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the ELEMENT DOOR emission (docs/phase4/DESIGN-array-pointer-doors.md, door B; arrayPointerElementDoor.go):
// an index, an element's address, and a whole-array load or store applied directly to `(*A)(p)`, A an array type
// and p an unsafe.Pointer, emit one of golib's doors with the pointer itself and the array's length as a constant.
// The forms they replace went through a box of the whole array, which memory that holds no Go array cannot answer.
//
// Each form has an arm. The CONTROLS carry the scope, which is the view doors' scope read from the same
// predicates: a conversion that another route renders whole keeps that route (the identity conversion, the
// same-element alias, the reinterpret between two array types of one shape, the native field view); an element
// that holds references takes no door; an operand that holds a FromRef mint keeps its emission. Two more are the
// element doors' own: a dereference Go does not evaluate
// (measured by len or cap, or ranged for its index alone) is no load and no box: it is the type's constant length.
// A dereference Go does evaluate there (ranged for its values, or measured with a call in its operand) is a load.

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// convertArrayPointerElementDoorFixture converts the one fixture every arm below reads.
func convertArrayPointerElementDoorFixture(t *testing.T) string {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/elemdoor\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import (
	"fmt"
	"unsafe"
)

type RawMem [1<<50 - 1]byte

type small [64]byte

type uuidT = [16]uint8

type pair struct {
	a int32
	b int32
}

type hdr struct {
	n    uint32
	tail [8]byte
}

func take(v int32) int32 { return v }

func takeArr(v [2]byte) byte { return v[1] }

// POSITIVE 1: a read at an index.
func read(p unsafe.Pointer, i int) int32 {
	return (*[4]int32)(p)[i]
}

// POSITIVE 2: a store, the operand a number converted in place.
func store(u uintptr) {
	(*[4]int32)(unsafe.Pointer(u))[1] = 7
}

// POSITIVE 3: compound assignment and increment.
func compound(p unsafe.Pointer, i int) {
	(*[4]int32)(p)[i] += 3
	(*[4]int32)(p)[i]++
}

// POSITIVE 4: a named array far larger than any memory; the length travels as a constant.
func named(p unsafe.Pointer, n int) {
	(*RawMem)(p)[n] = 0
}

// POSITIVE 5: an index wider than int travels at its full value.
func wide(p unsafe.Pointer, i uint64) int32 {
	return (*[4]int32)(p)[i]
}

// POSITIVE 6: the read is an argument.
func arg(p unsafe.Pointer) int32 {
	return take((*[4]int32)(p)[1])
}

// POSITIVE 7: a field of an element that is a struct of scalars.
func field(p unsafe.Pointer) {
	(*[3]pair)(p)[1].a = 5
}

// POSITIVE 8: the address of an element.
func addr(p unsafe.Pointer) *int32 {
	return &(*[4]int32)(p)[2]
}

// POSITIVE 9: the explicit dereference spells the same index.
func deref(p unsafe.Pointer) int32 {
	return (*(*[4]int32)(p))[1]
}

// POSITIVE 10: a Go pointer to another type is no route of its own.
func scalar(x *uint32) byte {
	return (*[4]byte)(unsafe.Pointer(x))[0]
}

// POSITIVE 11 to 14: a whole array loaded for its value.
func loadBound(p unsafe.Pointer) int64 {
	x := *(*[2]int64)(p)
	return x[1]
}

func loadReturn(p unsafe.Pointer) [2]uintptr {
	return *(*[2]uintptr)(p)
}

func loadCompare(p, q unsafe.Pointer) bool {
	return *(*[2]int64)(p) == *(*[2]int64)(q)
}

func loadArg(p unsafe.Pointer) byte {
	return takeArr(*(*[2]byte)(p))
}

// POSITIVE 18 and 19: a dereference Go evaluates is a load: ranged for its values, or measured with a call in it.
func rangedValue(p unsafe.Pointer) (n int) {
	for _, b := range *(*[4]byte)(p) {
		n += int(b)
	}

	return
}

// POSITIVE 20: the conversion itself ranged for its values reads each element through the pointer, so it ranges
// the view door's window of the whole array and takes no copy.
func rangedPointer(p unsafe.Pointer) (n int) {
	for _, b := range (*[4]byte)(p) {
		n += int(b)
	}

	return
}

// The same conversion ranged for its index alone never reads the memory: the pointer and the type's length.
func rangedPointerIndex(p unsafe.Pointer) (n int) {
	for i := range (*[4]byte)(p) {
		n += i
	}

	return
}

func get(p unsafe.Pointer) unsafe.Pointer { return p }

func measuredCall(p unsafe.Pointer) int {
	return len(*(*[4]byte)(get(p)))
}

// POSITIVE 21: a NAMED array's whole load and store take the same doors; the value is the named type's.
func namedWhole(p unsafe.Pointer, s small) byte {
	*(*small)(p) = s
	x := *(*small)(p)
	return x[3]
}

// POSITIVE 22: an ALIAS of an unnamed array is that array (modernc.org/libc's uuid.Uuid_t on linux).
func aliasWhole(dst, src uintptr) {
	*(*uuidT)(unsafe.Pointer(dst)) = *(*uuidT)(unsafe.Pointer(src))
}

// POSITIVE 15 to 17: a whole array stored.
func storeLiteral(u uintptr) {
	*(*[2]byte)(unsafe.Pointer(u)) = [2]byte{'C', 0}
}

func storeVar(p unsafe.Pointer, v [2]byte) {
	*(*[2]byte)(p) = v
}

func storeLoad(p, q unsafe.Pointer) {
	*(*[2]byte)(p) = *(*[2]byte)(q)
}

// CONTROL 1: the identity conversion keeps the box.
func identity(a *[4]int) int {
	return (*[4]int)(unsafe.Pointer(a))[1]
}

// CONTROL 2: the same-element alias conversion keeps the alias route.
func sameElem(buf *uint16, i int) uint16 {
	return (*[10000]uint16)(unsafe.Pointer(buf))[i]
}

// CONTROL 3: two array types of one shape keep the reinterpret.
func sibling(a *[64]byte) byte {
	return (*small)(unsafe.Pointer(a))[1]
}

// CONTROL 4: a field of another type, one hop off a pointer, keeps the native field view.
func fieldView(h *hdr) byte {
	return (*[2]byte)(unsafe.Pointer(&h.n))[1]
}

// CONTROL 5: an element that holds references takes no door.
func managed(p unsafe.Pointer) unsafe.Pointer {
	return (*[2]unsafe.Pointer)(p)[1]
}

// CONTROL 6: an operand that holds a FromRef mint keeps its emission.
func (h *hdr) first() byte {
	return (*[12]byte)(unsafe.Pointer(h))[0]
}

// CONTROL 8: a dereference Go does not evaluate is no load and no box. Its length is the type's: the constant.
func measured(p unsafe.Pointer) int {
	return len(*(*[4]byte)(p)) + cap(*(*[4]byte)(p))
}

func measuredPointer(p unsafe.Pointer) int {
	return len((*[4]byte)(p)) + cap((*small)(p))
}

func rangedIndex(p unsafe.Pointer) (n int) {
	for i := range *(*[4]byte)(p) {
		n += i
	}

	return
}

// CONTROL 9: the identity conversion's whole-array store and load keep the box.
func identityWhole(a *[4]int, v [4]int) int {
	*(*[4]int)(unsafe.Pointer(a)) = v
	x := *(*[4]int)(unsafe.Pointer(a))
	return x[0]
}

func main() {
	var i32 [4]int32
	p := unsafe.Pointer(&i32)
	store(uintptr(p))
	compound(p, 0)
	*addr(p) = 9
	fmt.Println(read(p, 1), wide(p, 2), arg(p), deref(p))
	var x uint32
	h := &hdr{}
	fmt.Println(scalar(&x), h.first())
	_ = named
	_ = field
	_ = loadBound
	_ = loadReturn
	_ = loadCompare
	_ = loadArg
	_ = storeLiteral
	_ = storeVar
	_ = storeLoad
	_ = identity
	_ = sameElem
	_ = sibling
	_ = fieldView
	_ = managed
	_ = namedWhole
	_ = aliasWhole
	_ = measured
	_ = measuredPointer
	_ = rangedIndex
	_ = rangedValue
	_ = measuredCall
	_ = rangedPointer
	_ = rangedPointerIndex
	_ = identityWhole
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

	return readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "elemdoor", "main.cs"))
}

func TestArrayPointerElementDoorReplacesTheWholeArrayBox(t *testing.T) {
	isolateConverterPathGlobals(t)

	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	mainCs := convertArrayPointerElementDoorFixture(t)

	// The positives: the door, with the pointer as a pointer, the index, and the array's length last.
	for _, arm := range []struct {
		function, door string
	}{
		{"read", "return @unsafe.Pointer.ElementRef<int32>(p, i, 4);"},
		{"store", "@unsafe.Pointer.ElementRef<int32>((@unsafe.Pointer)u, 1, 4) = 7;"},
		{"compound", "@unsafe.Pointer.ElementRef<int32>(p, i, 4) += 3;"},
		{"compound", "@unsafe.Pointer.ElementRef<int32>(p, i, 4)++;"},
		{"named", "@unsafe.Pointer.ElementRef<byte>(p, n, 1125899906842623) = 0;"},
		{"wide", "return @unsafe.Pointer.ElementRef<int32>(p, (nint)(i), 4);"},
		{"arg", "return take(@unsafe.Pointer.ElementRef<int32>(p, 1, 4));"},
		{"field", "@unsafe.Pointer.ElementRef<pair>(p, 1, 3).a = 5;"},
		{"addr", "return @unsafe.Pointer.Element<int32>(p, 2, 4);"},
		{"deref", "return @unsafe.Pointer.ElementRef<int32>(p, 1, 4);"},
		{"scalar", "return @unsafe.Pointer.ElementRef<byte>(@unsafe.Pointer.FromPinnedBox(x), 0, 4);"},
		{"loadBound", "var x = @unsafe.Pointer.LoadArray<int64>(p, 2);"},
		{"loadReturn", "return @unsafe.Pointer.LoadArray<uintptr>(p, 2);"},
		{"loadCompare", "return @unsafe.Pointer.LoadArray<int64>(p, 2) == @unsafe.Pointer.LoadArray<int64>(q, 2);"},
		{"loadArg", "return takeArr(@unsafe.Pointer.LoadArray<byte>(p, 2));"},
		{"storeLiteral", "@unsafe.Pointer.StoreArray<byte>((@unsafe.Pointer)u, 2, "},
		{"storeVar", "@unsafe.Pointer.StoreArray<byte>(p, 2, v);"},
		{"storeLoad", "@unsafe.Pointer.StoreArray<byte>(p, 2, @unsafe.Pointer.LoadArray<byte>(q, 2));"},
		{"rangedValue", "in range(@unsafe.Pointer.LoadArray<byte>(p, 4)))"},
		{"measuredCall", "return len(@unsafe.Pointer.LoadArray<byte>(get(p), 4));"},
		{"rangedPointer", "in range(@unsafe.Pointer.View<byte>(p, 0, 4, 4)))"},
		{"namedWhole", "@unsafe.Pointer.StoreArray<byte>(p, 64, s);"},
		{"namedWhole", "var x = (small)@unsafe.Pointer.LoadArray<byte>(p, 64);"},
		{"aliasWhole", "@unsafe.Pointer.StoreArray<uint8>((@unsafe.Pointer)dst, 16, @unsafe.Pointer.LoadArray<uint8>((@unsafe.Pointer)src, 16));"},
	} {
		body := emittedFunctionBody(t, mainCs, arm.function)

		if !strings.Contains(body, arm.door) {
			t.Errorf("%s: expected the door %q, got:\n%s", arm.function, arm.door, body)
		}

		if strings.Contains(body, "@unsafe.Pointer.To<array<") {
			t.Errorf("%s: an element door replaces the box of the whole array, got:\n%s", arm.function, body)
		}
	}

	doors := []string{"@unsafe.Pointer.ElementRef<", "@unsafe.Pointer.Element<", "@unsafe.Pointer.LoadArray<", "@unsafe.Pointer.StoreArray<"}

	unevaluated := map[string]bool{"measured": true, "measuredPointer": true, "rangedIndex": true}

	// The controls take no door.
	for _, arm := range []struct {
		function, keeps, why string
	}{
		{"identity", "return (a).Value[1];", "the identity conversion keeps the box"},
		{"sameElem", "array<uint16>.AliasPointer(buf, 10000)", "the same-element alias conversion keeps the alias route"},
		{"sibling", "a.Reinterpret<array<byte>, small>()", "two array types of one shape keep the reinterpret"},
		{"fieldView", "@unsafe.ArrayPointer<byte>.Of(h.Ꮡn, 2)", "a field of another type keeps the native field view"},
		{"managed", "", "an element that holds references takes no door"},
		{"first", "FromRef(ref h)", "an operand that holds a FromRef mint keeps its emission"},
		{"measured", "return 4 + 4;", "a measured dereference is the type's constant length"},
		{"measuredPointer", "return 4 + 64;", "a measured conversion is the type's constant length"},
		{"rangedIndex", "in range(4))", "a dereference ranged for its index alone ranges the type's constant length"},
		{"rangedPointerIndex", "in range((@unsafe.Pointer.ToArray<byte>(p, 4)), 4))", "a conversion ranged for its index alone ranges the pointer (door C, section 20) and the type's length"},
		{"identityWhole", "", "the identity conversion's whole-array store and load keep the box"},
	} {
		body := emittedFunctionBody(t, mainCs, arm.function)

		if !strings.Contains(body, arm.keeps) {
			t.Errorf("%s: %s, expected %q, got:\n%s", arm.function, arm.why, arm.keeps, body)
		}

		for _, door := range doors {
			if strings.Contains(body, door) {
				t.Errorf("%s: %s, and it emitted %s, got:\n%s", arm.function, arm.why, door, body)
			}
		}

		if unevaluated[arm.function] && strings.Contains(body, "@unsafe.Pointer.To<") {
			t.Errorf("%s: %s, and Go evaluates nothing there, got:\n%s", arm.function, arm.why, body)
		}
	}
}
