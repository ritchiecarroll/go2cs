// nativeFieldArrayView_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the NATIVE FIELD-VIEW emission (docs/phase4/DESIGN-native-array-view.md, the LookupServicePort
// door): Go's `(*[N]T)(unsafe.Pointer(&p.f))`, where f's type is not T, emits the unsafe package's
// helper over the field reference, `@unsafe.ArrayPointer<T>.Of(Ꮡp.of(S.Ꮡf), N)`, naming the field ONCE,
// and a local bound by it and never re-pointed reads `p[i]` through ElementRef rather than through
// Value. golib resolves f's Go offset at run time, so the emission carries no offset literal and reads
// the same on every target.
//
// The corpus instance is net's darwin cgoLookupServicePort (Go net/cgo_unix.go:154/158), whose `sa` is
// libc memory: the raw route views the port at the field's CLR offset, which is not Go's for a struct
// carrying an array<> field, and Value refuses a native array by design. The CONTROLS carry the scope:
//
//   - a re-pointed local keeps `.Value[i]` — after `q = other` the local may name a managed array;
//   - a SAME-element view keeps array<T>.AliasPointer, the half that already windows real storage;
//   - a field reached through a PROMOTION, an index or a call keeps the raw route. The raw route is spelled as
//     the typed door, `@unsafe.Pointer.To<array<T>>(…)`, since the interior-pointer root
//     (typedPointerDoor_test.go); it was the cast `(ж<array<T>>)(uintptr)(…)`.
//
// A chain of field selections off a variable takes the helper since docs/phase4/DESIGN-array-pointer-doors.md,
// section 19 (`&sa.raw.Port`, golang.org/x/sys's port): a two-hop selector was a control here until then.

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// convertNativeFieldArrayViewFixture converts the one fixture every arm below reads.
func convertNativeFieldArrayViewFixture(t *testing.T) string {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/nativefieldview\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import (
	"fmt"
	"unsafe"
)

// The corpus shape: a sockaddr whose array fields make its converted form reference-bearing.
type sockaddr struct {
	Family uint16
	Port   uint16
	Addr   [4]byte
	Zero   [8]uint8
}

type holder struct {
	pad   uint32
	cheap uint64
}

type outer struct {
	inner sockaddr
}

type top struct {
	o outer
}

type embeds struct {
	sockaddr
	extra uint32
}

var shared sockaddr

// POSITIVE 1 — the read, net's cgoLookupServicePort.
func portOf(sa *sockaddr) int {
	p := (*[2]byte)(unsafe.Pointer(&sa.Port))
	return int(p[0])<<8 | int(p[1])
}

// POSITIVE 2 — the write, syscall's sockaddr encoders.
func setPort(sa *sockaddr, port int) {
	p := (*[2]byte)(unsafe.Pointer(&sa.Port))
	p[0] = byte(port >> 8)
	p[1] = byte(port)
}

// POSITIVE 3 — runtime's cheaprand: a uint64 field viewed as two uint32s, at a non-zero offset.
func words(h *holder) (uint32, uint32) {
	t := (*[2]uint32)(unsafe.Pointer(&h.cheap))
	return t[0], t[1]
}

// CONTROL 1 — a re-pointed local.
func repointed(sa *sockaddr, other *[2]byte) byte {
	q := (*[2]byte)(unsafe.Pointer(&sa.Port))
	q = other
	return q[0]
}

// CONTROL 2 — a same-element view.
func sameElem(xs *[4]byte) byte {
	a := (*[2]byte)(unsafe.Pointer(&xs[0]))
	return a[1]
}

// CHAIN 1 — a two-hop selector, x/sys's port (it was control 3 until section 19).
func nested(o *outer) byte {
	n := (*[2]byte)(unsafe.Pointer(&o.inner.Port))
	return n[0]
}

// CHAIN 2 — three hops, bound and stored through; and the two-hop view indexed where it is written.
func deep(t *top) {
	d := (*[2]byte)(unsafe.Pointer(&t.o.inner.Port))
	d[1] = 7
	(*[2]byte)(unsafe.Pointer(&t.o.inner.Port))[0] = 9
}

// CHAIN 3 — a struct variable and a package variable.
func variable() uint16 {
	var s sockaddr
	v := (*[2]byte)(unsafe.Pointer(&s.Port))
	v[0] = 1
	g := (*[2]byte)(unsafe.Pointer(&shared.Port))
	g[1] = 2
	return s.Port
}

// CONTROL 3 — a promoted field: the selection passes through the embedded struct.
func promoted(e *embeds) byte {
	m := (*[2]byte)(unsafe.Pointer(&e.Port))
	return m[0]
}

// CONTROL 4 — a field of an element and of a call's result: the root is no variable.
func elementRoot(xs []sockaddr) byte {
	x := (*[2]byte)(unsafe.Pointer(&xs[1].Port))
	return x[0]
}

func callRoot(o *outer) byte {
	c := (*[2]byte)(unsafe.Pointer(&pick(o).Port))
	return c[0]
}

func pick(o *outer) *sockaddr {
	return &o.inner
}

// DIRECT 1 — the view indexed where it is written: a read, a store, a compound store and a step.
func directPort(sa *sockaddr) int {
	(*[2]byte)(unsafe.Pointer(&sa.Port))[0] = 1
	(*[2]byte)(unsafe.Pointer(&sa.Port))[1] += 2
	(*[2]byte)(unsafe.Pointer(&sa.Port))[1]++
	return int((*[2]byte)(unsafe.Pointer(&sa.Port))[0])<<8 | int((*[2]byte)(unsafe.Pointer(&sa.Port))[1])
}

// DIRECT 2 — the explicit dereference names the same element.
func directDeref(h *holder) uint32 {
	(*(*[2]uint32)(unsafe.Pointer(&h.cheap)))[0] = 7
	return (*(*[2]uint32)(unsafe.Pointer(&h.cheap)))[1]
}

// DIRECT CONTROL — a same-element view indexed where it is written keeps its own route.
func directSameElem(xs *[4]byte) byte {
	return (*[2]byte)(unsafe.Pointer(&xs[0]))[1]
}

// WHOLE 1 — the whole array read for its value: a copy.
func wholeLoad(sa *sockaddr) [2]byte {
	p := (*[2]byte)(unsafe.Pointer(&sa.Port))
	v := *p
	return v
}

// WHOLE 2 — the whole array stored.
func wholeStore(sa *sockaddr, v [2]byte) {
	p := (*[2]byte)(unsafe.Pointer(&sa.Port))
	*p = v
}

// WHOLE 3 — a range that reads the values, each when its round comes.
func rangeSum(sa *sockaddr) int {
	p := (*[2]byte)(unsafe.Pointer(&sa.Port))
	sum := 0
	for _, b := range p {
		sum += int(b)
	}
	return sum
}

// WHOLE 4 — the explicit dereference of the local, indexed: the view's own element.
func derefIndex(sa *sockaddr) byte {
	p := (*[2]byte)(unsafe.Pointer(&sa.Port))
	(*p)[0] = 3
	return (*p)[1]
}

// WHOLE 5 — the store and the load through the view where it is written.
func wholeDirect(h *holder, v [2]uint32) [2]uint32 {
	*(*[2]uint32)(unsafe.Pointer(&h.cheap)) = v
	return *(*[2]uint32)(unsafe.Pointer(&h.cheap))
}

// WHOLE 6 — the dereference sliced, its length, and a range for its index alone: Go reads no array for any.
func derefForms(sa *sockaddr) ([]byte, int) {
	p := (*[2]byte)(unsafe.Pointer(&sa.Port))
	n := len(*p)
	for i := range *p {
		n += i
	}
	return (*p)[:1], n
}

type rawPort struct {
	port [2]byte
}

// WHOLE 7 — a struct literal's field value: x/sys/windows' SockaddrBth shape.
func inLiteral(sa *sockaddr) rawPort {
	return rawPort{port: *(*[2]byte)(unsafe.Pointer(&sa.Port))}
}

// WHOLE CONTROL — a re-pointed local may name a managed array, so every whole-array form keeps Value.
func wholeRepointed(sa *sockaddr, other *[2]byte) [2]byte {
	q := (*[2]byte)(unsafe.Pointer(&sa.Port))
	q = other
	*q = [2]byte{9, 9}
	sum := 0
	for _, b := range q {
		sum += int(b)
	}
	(*q)[0] = byte(sum)
	return *q
}

func main() {
	sa := &sockaddr{}
	setPort(sa, 80)
	wholeStore(sa, wholeLoad(sa))
	fmt.Println(rangeSum(sa), derefIndex(sa), wholeDirect(&holder{}, [2]uint32{1, 2}), wholeRepointed(sa, &[2]byte{}))
	fmt.Println(derefForms(sa))
	fmt.Println(inLiteral(sa))
	fmt.Println(directPort(sa), directDeref(&holder{cheap: 1}), directSameElem(&[4]byte{1, 2, 3, 4}))
	other := [2]byte{1, 2}
	xs := [4]byte{1, 2, 3, 4}
	h := &holder{cheap: 1}
	w0, w1 := words(h)
	fmt.Println(portOf(sa), repointed(sa, &other), sameElem(&xs), nested(&outer{}), w0, w1)
	deep(&top{})
	fmt.Println(variable(), promoted(&embeds{}), elementRoot(make([]sockaddr, 2)), callRoot(&outer{}))
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

	return readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "nativefieldview", "main.cs"))
}

// emittedFunctionBody returns the emitted body of the named function — from its declaration to the
// closing brace at the declaration's own indentation — so each arm is read at ITS OWN function rather
// than anywhere in the file, where one positive would satisfy every control.
func emittedFunctionBody(t *testing.T, mainCs string, name string) string {
	t.Helper()

	lines := strings.Split(strings.ReplaceAll(mainCs, "\r\n", "\n"), "\n")

	for i, line := range lines {
		if !strings.Contains(line, " "+name+"(") || !strings.HasSuffix(strings.TrimSpace(line), "{") {
			continue
		}

		indent := line[:len(line)-len(strings.TrimLeft(line, " "))]

		for j := i + 1; j < len(lines); j++ {
			if lines[j] == indent+"}" {
				return strings.Join(lines[i:j+1], "\n")
			}
		}
	}

	t.Fatalf("no emitted function %q; emission:\n%s", name, mainCs)

	return ""
}

func TestNativeFieldArrayViewEmitsTheDoorAndElementRef(t *testing.T) {
	isolateConverterPathGlobals(t)
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	mainCs := convertNativeFieldArrayViewFixture(t)

	// The positives: the door with the field's GO offset and the array's N, ahead of today's raw route,
	// and the local's index through ElementRef.
	for _, arm := range []struct {
		function, door, local string
		reads                 []string
	}{
		{"portOf", "@unsafe.ArrayPointer<byte>.Of(", "p", []string{"p.ElementRef(0)", "p.ElementRef(1)"}},
		{"setPort", "@unsafe.ArrayPointer<byte>.Of(", "p", []string{"p.ElementRef(0) =", "p.ElementRef(1) ="}},
		{"words", "@unsafe.ArrayPointer<uint32>.Of(", "t", []string{"t.ElementRef(0)", "t.ElementRef(1)"}},
	} {
		body := emittedFunctionBody(t, mainCs, arm.function)

		if !strings.Contains(body, arm.door) {
			t.Errorf("%s: expected the native field-view door %q, got:\n%s", arm.function, arm.door, body)
		}

		if strings.Contains(body, "(uintptr)(") {
			t.Errorf("%s: the raw route lives inside the helper; the call site must not spell it, got:\n%s", arm.function, body)
		}

		for _, read := range arm.reads {
			if !strings.Contains(body, read) {
				t.Errorf("%s: expected %q, got:\n%s", arm.function, read, body)
			}
		}

		if strings.Contains(body, arm.local+".Value[") {
			t.Errorf("%s: a native array pointer has no Value to index — %s.Value[ must not be emitted, got:\n%s", arm.function, arm.local, body)
		}
	}

	// The helper is handed the FIELD REFERENCE, once, and N — never an offset literal, which is a property
	// of the target and would make one source file emit differently per flavour.
	if body := emittedFunctionBody(t, mainCs, "portOf"); !strings.Contains(body, "@unsafe.ArrayPointer<byte>.Of(sa.ᏑPort, 2);") || strings.Count(body, "sa.ᏑPort") != 1 {
		t.Errorf("portOf: expected the helper over sa.ᏑPort (the address property, parity seat #32), named once, with N 2, got:\n%s", body)
	}

	if body := emittedFunctionBody(t, mainCs, "words"); !strings.Contains(body, "@unsafe.ArrayPointer<uint32>.Of(h.Ꮡcheap, 2);") || strings.Count(body, "h.Ꮡcheap") != 1 {
		t.Errorf("words: expected the helper over h.Ꮡcheap, named once, with N 2, got:\n%s", body)
	}

	// CONTROL 1 — the re-pointed local keeps Value: after `q = other` it may name a managed array.
	if body := emittedFunctionBody(t, mainCs, "repointed"); !strings.Contains(body, "q.Value[0]") || strings.Contains(body, "q.ElementRef(") {
		t.Errorf("repointed: a re-pointed local must keep q.Value[0], got:\n%s", body)
	}

	// CONTROL 2 — a same-element view keeps AliasPointer and takes no door.
	if body := emittedFunctionBody(t, mainCs, "sameElem"); !strings.Contains(body, "array<byte>.AliasPointer(") || strings.Contains(body, "@unsafe.ArrayPointer<") {
		t.Errorf("sameElem: a same-element view must keep array<T>.AliasPointer, got:\n%s", body)
	}

	// CONTROL 3 — a promoted field takes no field view; CONTROL 4 — nor does a field of an element or of a call's
	// result. Each is the kept conversion of door C (section 20), whose window is cut only over storage a box holds
	// still: over these fields, which nothing pins, it answers the raw route's refusal by name
	// (GolibTests KeptArrayDoorTests.AFieldNothingHoldsStillIsNoWindow).
	for _, control := range []string{"promoted", "elementRoot", "callRoot"} {
		if body := emittedFunctionBody(t, mainCs, control); strings.Contains(body, "@unsafe.ArrayPointer<") || !strings.Contains(body, "@unsafe.Pointer.ToArray<byte>(") || !strings.Contains(body, ", 2);") {
			t.Errorf("%s: a field that is not reached by field selections off a variable must take door C, not the field view, got:\n%s", control, body)
		}
	}
}

// A chain of field selections off a variable takes the helper, as one selection off a pointer does: the
// converter already emits the address of such a field as the field's own pointer, which is what the helper
// takes (docs/phase4/DESIGN-array-pointer-doors.md, section 19). golang.org/x/sys writes a port through
// `&sa.raw.Port`; the raw route's pointer over such a field has no array behind its Value on any root.
func TestNativeFieldArrayViewTakesAChainOfFieldSelections(t *testing.T) {
	isolateConverterPathGlobals(t)
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	mainCs := convertNativeFieldArrayViewFixture(t)

	for _, arm := range []struct {
		function string
		expected []string
	}{
		{"nested", []string{"var n = @unsafe.ArrayPointer<byte>.Of(o.Ꮡinner.ᏑPort, 2);", "n.ElementRef(0)"}},
		{"deep", []string{"var d = @unsafe.ArrayPointer<byte>.Of(t.Ꮡo.Ꮡinner.ᏑPort, 2);", "d.ElementRef(1) = 7;", "@unsafe.ArrayPointer<byte>.Of(t.Ꮡo.Ꮡinner.ᏑPort, 2).ElementRef(0) = 9;"}},
		{"variable", []string{"var v = @unsafe.ArrayPointer<byte>.Of(Ꮡs.ᏑPort, 2);", "v.ElementRef(0) = 1;", "var g = @unsafe.ArrayPointer<byte>.Of(Ꮡshared.ᏑPort, 2);", "g.ElementRef(1) = 2;"}},
	} {
		body := emittedFunctionBody(t, mainCs, arm.function)

		for _, expected := range arm.expected {
			if !strings.Contains(body, expected) {
				t.Errorf("%s: expected %q, got:\n%s", arm.function, expected, body)
			}
		}

		if strings.Contains(body, ".Value[") || strings.Contains(body, "@unsafe.Pointer.To<array<byte>>(") {
			t.Errorf("%s: the raw route's pointer has no array behind its Value; neither may be emitted, got:\n%s", arm.function, body)
		}
	}
}

// The view indexed where it is written reads through ElementRef, as a local bound by it does. It went through
// Value, which a native array pointer refuses, and the view is one over every root that has an address
// (docs/phase4/DESIGN-array-pointer-doors.md, section 18).
func TestNativeFieldArrayViewIndexedDirectlyReadsThroughElementRef(t *testing.T) {
	isolateConverterPathGlobals(t)
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	mainCs := convertNativeFieldArrayViewFixture(t)

	for _, arm := range []struct {
		function string
		forms    []string
	}{
		{"directPort", []string{
			"@unsafe.ArrayPointer<byte>.Of(sa.ᏑPort, 2).ElementRef(0) = 1;",
			"@unsafe.ArrayPointer<byte>.Of(sa.ᏑPort, 2).ElementRef(1) += 2;",
			"@unsafe.ArrayPointer<byte>.Of(sa.ᏑPort, 2).ElementRef(1)++;",
			"(nint)@unsafe.ArrayPointer<byte>.Of(sa.ᏑPort, 2).ElementRef(0) << 8",
		}},
		{"directDeref", []string{
			"@unsafe.ArrayPointer<uint32>.Of(h.Ꮡcheap, 2).ElementRef(0) = 7;",
			"return @unsafe.ArrayPointer<uint32>.Of(h.Ꮡcheap, 2).ElementRef(1);",
		}},
	} {
		body := emittedFunctionBody(t, mainCs, arm.function)

		for _, form := range arm.forms {
			if !strings.Contains(body, form) {
				t.Errorf("%s: expected %q, got:\n%s", arm.function, form, body)
			}
		}

		if strings.Contains(body, ".Value[") {
			t.Errorf("%s: the view has no Value to index, got:\n%s", arm.function, body)
		}
	}

	// CONTROL — a same-element view is not this shape: it keeps array<T>.AliasPointer and its Value.
	if body := emittedFunctionBody(t, mainCs, "directSameElem"); !strings.Contains(body, "array<byte>.AliasPointer(") || strings.Contains(body, ".ElementRef(") {
		t.Errorf("directSameElem: a same-element view must keep array<T>.AliasPointer and Value, got:\n%s", body)
	}
}

// The whole array through the view goes through its elements: the view has no array<T> for Value to return. A
// load is a copy, a store writes every element, a range over the pointer reads each element in its round, and
// the explicit dereference indexed is the view's own element (docs/phase4/DESIGN-array-pointer-doors.md,
// section 18, part C).
func TestNativeFieldArrayViewWholeArrayGoesThroughTheHelpers(t *testing.T) {
	isolateConverterPathGlobals(t)
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	mainCs := convertNativeFieldArrayViewFixture(t)

	for _, arm := range []struct {
		function string
		forms    []string
	}{
		{"wholeLoad", []string{"var v = @unsafe.ArrayPointer<byte>.Load(p);"}},
		{"wholeStore", []string{"@unsafe.ArrayPointer<byte>.Store(p, v);"}},
		{"rangeSum", []string{"range(p.slice())"}},
		{"derefIndex", []string{"p.ElementRef(0) = 3;", "return p.ElementRef(1);"}},
		{"wholeDirect", []string{
			"@unsafe.ArrayPointer<uint32>.Store(@unsafe.ArrayPointer<uint32>.Of(h.Ꮡcheap, 2), v);",
			"return @unsafe.ArrayPointer<uint32>.Load(@unsafe.ArrayPointer<uint32>.Of(h.Ꮡcheap, 2));",
		}},
		{"derefForms", []string{"nint n = 2;", "in range(2))", "p.slice(0, 1)"}},
		{"inLiteral", []string{"port: @unsafe.ArrayPointer<byte>.Load(@unsafe.ArrayPointer<byte>.Of(sa.ᏑPort, 2))"}},
	} {
		body := emittedFunctionBody(t, mainCs, arm.function)

		for _, form := range arm.forms {
			if !strings.Contains(body, form) {
				t.Errorf("%s: expected %q, got:\n%s", arm.function, form, body)
			}
		}

		if strings.Contains(body, ".Value") {
			t.Errorf("%s: the view has no Value, got:\n%s", arm.function, body)
		}

		// A local's own copy (`v.Clone()`) is the array value's and stays. A clone of a helper's answer would
		// be a second copy of a copy.
		if strings.Contains(body, ").Clone()") {
			t.Errorf("%s: the load is already a copy and the store copies, so no clone of a helper's answer is owed, got:\n%s", arm.function, body)
		}
	}

	// CONTROL — a re-pointed local keeps Value in every whole-array form and takes no helper.
	body := emittedFunctionBody(t, mainCs, "wholeRepointed")

	for _, helper := range []string{".Load(", ".Store(", "q.slice()", "q.ElementRef("} {
		if strings.Contains(body, helper) {
			t.Errorf("wholeRepointed: a re-pointed local must take no view helper (%s), got:\n%s", helper, body)
		}
	}

	if !strings.Contains(body, "q.Value") {
		t.Errorf("wholeRepointed: a re-pointed local must keep q.Value, got:\n%s", body)
	}
}
