// arrayPointerViewDoor_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the VIEW DOOR emission (docs/phase4/DESIGN-array-pointer-doors.md, door A; arrayPointerViewDoor.go): a
// slice of `(*A)(p)`, A an array type and p an unsafe.Pointer, emits one of golib's doors with the pointer
// itself, the bounds at their full value and the array's length as a constant. The form it replaces built the
// slice from a span, which copies, so a write through the slice never reached the memory.
//
// Each form has an arm, the open form of an UNNAMED array among them (decision D4, taken in section 16.10: its box
// route answered length 0 over memory that is no Go array of that type, and in a copy it could end the process).
// The CONTROLS carry the scope: an element that holds references takes no door; an operand that holds a FromRef mint keeps the span; a BOUNDED view
// under copy stays builtin copy over the view door, because only the open form needs the copy doors; and a
// conversion that another route renders whole keeps that route when it is sliced (controls 5 to 9: the
// same-element alias, the identity conversion in its bounded and open forms, the reinterpret between two array
// types of one shape, and the native field view). Each of those is already a view of the storage and banked rows
// run on them; the door replaces the copying span and nothing else.

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// convertArrayPointerViewDoorFixture converts the one fixture every arm below reads.
func convertArrayPointerViewDoorFixture(t *testing.T) string {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/viewdoor\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import (
	"fmt"
	"unsafe"
)

type RawMem [1<<50 - 1]byte

type small [64]byte

type refs [4]*int

type hdr struct {
	n    uint32
	tail [8]byte
}

// POSITIVE 1: two indices over an unnamed array.
func two(p unsafe.Pointer, n int) []byte {
	return (*[1 << 20]byte)(p)[:n]
}

// POSITIVE 2: three indices over a named array, with a low bound.
func three(p unsafe.Pointer, lo, n int) []byte {
	return (*RawMem)(p)[lo:n:n]
}

// POSITIVE 3: the open form of a named array.
func open(p unsafe.Pointer) []byte {
	return (*small)(p)[2:]
}

// POSITIVE 4: a bound wider than int travels at its full value; the operand is a number converted in place.
func wide(p uintptr, n uint64) []uint16 {
	return (*[1 << 29]uint16)(unsafe.Pointer(p))[:n:n]
}

// POSITIVE 5 to 7: the open form of a named array as copy's destination and source.
func into(p unsafe.Pointer, src []byte) int {
	return copy((*RawMem)(p)[:], src)
}

func intoString(p unsafe.Pointer) int {
	return copy((*RawMem)(p)[4:], "text")
}

func from(dst []byte, p unsafe.Pointer) int {
	return copy(dst, (*RawMem)(p)[:])
}

// POSITIVE 10 to 12: the open form of an UNNAMED array, alone and as copy's destination and source. The first two
// are over a Go value of another type (reflect's nameOff copy is the second).
func openUnnamed(x *uint32) []byte {
	return (*[4]byte)(unsafe.Pointer(x))[:]
}

func intoUnnamed(n *int32, src []byte) int {
	return copy((*[4]byte)(unsafe.Pointer(n))[:], src)
}

func fromUnnamed(dst []byte, p unsafe.Pointer) int {
	return copy(dst, (*[8]byte)(p)[2:])
}

// CONTROL 1: the open form over a field of another type, one hop off a pointer, keeps the native field view.
func fieldOpen(h *hdr) []byte {
	return (*[2]byte)(unsafe.Pointer(&h.n))[:]
}

// CONTROL 2: an element that holds references takes no door.
func managed(p unsafe.Pointer) []*int {
	return (*refs)(p)[:2]
}

// CONTROL 3: an operand that holds a FromRef mint keeps the span.
func (h *hdr) bytes() []byte {
	return (*[12]byte)(unsafe.Pointer(h))[:4]
}

// CONTROL 4: a bounded view under copy stays builtin copy over the view door.
func bounded(p unsafe.Pointer, src []byte) int {
	return copy((*RawMem)(p)[:len(src)], src)
}

// CONTROL 5: the same-element alias conversion keeps the alias route when it is sliced.
func sameElem(buf *uint16, n int) []uint16 {
	return (*[10000]uint16)(unsafe.Pointer(buf))[:n:n]
}

// CONTROL 6: the identity conversion (the operand is already a pointer to the array) keeps the box.
func identity(a *[4]int) []int {
	return (*[4]int)(unsafe.Pointer(a))[1:3]
}

// CONTROL 7: the identity conversion of a named array, in the open form and under copy.
func identityNamed(a *small, src []byte) int {
	return len((*small)(unsafe.Pointer(a))[2:]) + copy((*small)(unsafe.Pointer(a))[:], src)
}

// CONTROL 8: two array types of one shape keep the reinterpret.
func sibling(a *[64]byte) []byte {
	return (*small)(unsafe.Pointer(a))[:4]
}

// CONTROL 9: a field of another type, one hop off a pointer, keeps the native field view.
func field(h *hdr) []byte {
	return (*[2]byte)(unsafe.Pointer(&h.n))[:2]
}

// POSITIVE 8 and 9: a Go pointer to another type is no route of its own; it took the span and takes the door.
func scalar(x *uint32) []byte {
	return (*[4]byte)(unsafe.Pointer(x))[:4]
}

func elemOfNamed(b *byte, n int) []byte {
	return (*RawMem)(unsafe.Pointer(b))[:n:n]
}

func main() {
	var buf [64]byte
	p := unsafe.Pointer(&buf)
	fmt.Println(len(two(p, 4)), len(three(p, 1, 4)), len(open(p)), into(p, []byte("ab")), intoString(p), from(make([]byte, 3), p), bounded(p, []byte("xy")))
	var x uint32
	h := &hdr{}
	var n int32
	fmt.Println(len(openUnnamed(&x)), intoUnnamed(&n, []byte("abcd")), fromUnnamed(make([]byte, 8), p), len(fieldOpen(h)), len(h.bytes()), len(wide(uintptr(p), 2)))
	_ = managed
	_ = sameElem
	_ = identity
	_ = identityNamed
	_ = sibling
	_ = field
	_ = scalar
	_ = elemOfNamed
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

	return readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "viewdoor", "main.cs"))
}

func TestArrayPointerViewDoorReplacesTheCopyingSpan(t *testing.T) {
	isolateConverterPathGlobals(t)

	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	mainCs := convertArrayPointerViewDoorFixture(t)

	// The positives: the door, with the pointer as a pointer, the bounds, and the array's length last.
	for _, arm := range []struct {
		function, door string
	}{
		{"two", "return @unsafe.Pointer.View<byte>(p, 0, n, 1048576);"},
		{"three", "return @unsafe.Pointer.View<byte>(p, lo, n, n, 1125899906842623);"},
		{"open", "return @unsafe.Pointer.View<byte>(p, 2, 64);"},
		{"wide", "return @unsafe.Pointer.View<uint16>((@unsafe.Pointer)p, 0, (nint)(n), (nint)(n), 536870912);"},
		{"into", "return @unsafe.Pointer.CopyInto<byte>(p, 0, 1125899906842623, src);"},
		{"intoString", "return @unsafe.Pointer.CopyInto(p, 4, 1125899906842623, \"text\"u8);"},
		{"from", "return @unsafe.Pointer.CopyFrom<byte>(dst, p, 0, 1125899906842623);"},
		{"bounded", "return copy(@unsafe.Pointer.View<byte>(p, 0, len(src), 1125899906842623), src);"},
		{"scalar", "return @unsafe.Pointer.View<byte>(@unsafe.Pointer.FromPinnedBox(x), 0, 4, 4);"},
		{"elemOfNamed", "return @unsafe.Pointer.View<byte>(@unsafe.Pointer.FromPinnedBox(b), 0, n, n, 1125899906842623);"},
		{"openUnnamed", "return @unsafe.Pointer.View<byte>(@unsafe.Pointer.FromPinnedBox(x), 0, 4);"},
		{"intoUnnamed", "return @unsafe.Pointer.CopyInto<byte>(@unsafe.Pointer.FromPinnedBox(n), 0, 4, src);"},
		{"fromUnnamed", "return @unsafe.Pointer.CopyFrom<byte>(dst, p, 2, 8);"},
	} {
		body := emittedFunctionBody(t, mainCs, arm.function)

		if !strings.Contains(body, arm.door) {
			t.Errorf("%s: expected the door %q, got:\n%s", arm.function, arm.door, body)
		}

		if strings.Contains(body, "ReadOnlySpan<") {
			t.Errorf("%s: a view door replaces the span, which copies, got:\n%s", arm.function, body)
		}
	}

	doors := []string{"@unsafe.Pointer.View<", "@unsafe.Pointer.CopyInto", "@unsafe.Pointer.CopyFrom<"}

	// The controls take no door.
	for _, arm := range []struct {
		function, keeps, why string
	}{
		{"fieldOpen", "return @unsafe.ArrayPointer<byte>.Of(h.Ꮡn, 2).slice();", "the open form over a field of another type keeps the native field view"},
		{"managed", "", "an element that holds references takes no door"},
		{"bytes", "new ReadOnlySpan<byte>((byte*)(uintptr)(@unsafe.Pointer.FromRef(ref h)), 4)", "an operand that holds a FromRef mint keeps the span"},
		{"sameElem", "return array<uint16>.AliasPointer(buf, 10000).slice(0, n, n);", "the same-element alias conversion keeps the alias route"},
		{"identity", "return a.slice(1, 3);", "the identity conversion keeps the box"},
		{"identityNamed", "return len(a.Value[2..]) + copy(a.Value[..], src);", "the identity conversion of a named array keeps the box in the open form and under copy"},
		{"sibling", "return a.Reinterpret<array<byte>, small>().Value[..4];", "two array types of one shape keep the reinterpret"},
		{"field", "return @unsafe.ArrayPointer<byte>.Of(h.Ꮡn, 2).slice(0, 2);", "a field of another type keeps the native field view"},
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
	}
}
