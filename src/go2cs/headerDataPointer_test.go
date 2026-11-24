// headerDataPointer_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// Go's unsafe rule 6 for a reflect header's Data field: `unsafe.Pointer(h.Data)` in ONE expression, with h a
// *reflect.StringHeader or *reflect.SliceHeader, is a pointer into what the header describes, and it keeps that memory
// alive. The emission hands golib the header's BOX, `@unsafe.Pointer.FromHeaderData(box)`, which answers a pointer that
// carries what holds the bytes for a header box and today's number pointer for any other box, decided at run time
// (docs/phase4/DESIGN-sqlite-ccgo-ts.md, sections 10.2 and 10.3: QW-8, and P3's item 2 on run-time dispatch).
func convertHeaderDataFixture(t *testing.T) string {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/hdrdata\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import (
	"fmt"
	"reflect"
	"unsafe"
)

type RawMem [1 << 20]byte

// A struct that is not a reflect header but has a uintptr field named Data.
type notHeader struct {
	Data uintptr
	Len  int
}

// SPELLING 1: h.Data, with h a local *reflect.StringHeader (libc's Xgethostname).
func hostname(s string, dst []byte) int {
	n := len(s)
	sh := (*reflect.StringHeader)(unsafe.Pointer(&s))
	copy(dst[:n:n], (*RawMem)(unsafe.Pointer(sh.Data))[:n:n])
	return n
}

// SPELLING 2: the inline conversion.
func inline(s string) byte {
	return *(*byte)(unsafe.Pointer((*reflect.StringHeader)(unsafe.Pointer(&s)).Data))
}

// SPELLING 3: (*h).Data.
func deref(s string) byte {
	h := (*reflect.StringHeader)(unsafe.Pointer(&s))
	return *(*byte)(unsafe.Pointer((*h).Data))
}

// The SliceHeader twin of spelling 1.
func sliceData(b []byte) byte {
	h := (*reflect.SliceHeader)(unsafe.Pointer(&b))
	return *(*byte)(unsafe.Pointer(h.Data))
}

// An ALIAS of unsafe.Pointer (zeebo/xxh3's spelling) takes the same door, through the alias name (C45, P3's read).
type ptr = unsafe.Pointer

func aliasData(b []byte) byte {
	h := (*reflect.SliceHeader)(ptr(&b))
	return *(*byte)(ptr(h.Data))
}

// The alias through a pointer RECEIVER: the plain spelling's FromRef arm, never a mint over the dereferenced value.
type cell struct{ v uint64 }

func (c *cell) addr() ptr {
	return ptr(c)
}

// The alias over a NAMED uintptr: the plain spelling's hop through the underlying.
type word uintptr

func aliasWord(w word) ptr {
	return ptr(w)
}

// A header built field by field is a plain struct box; the call still goes through FromHeaderData, which answers
// today's number pointer for it at run time.
func plain(n uintptr) uintptr {
	p := &reflect.StringHeader{Data: n, Len: 1}
	return uintptr(unsafe.Pointer(p.Data))
}

// P3's (b): a pointer PARAMETER, the shape libc's helpers take.
func param(sh *reflect.StringHeader) unsafe.Pointer {
	return unsafe.Pointer(sh.Data)
}

// CONTROL 3 (P3's (c)): a header VALUE has no header box behind it; its Data stays a number.
func value(s string) unsafe.Pointer {
	var h reflect.StringHeader
	h.Len = len(s)
	return unsafe.Pointer(h.Data)
}

// CONTROL 1: a bare number stays a number pointer.
func number(n uintptr) unsafe.Pointer {
	return unsafe.Pointer(n)
}

// CONTROL 2: a Data field of a struct that is not a reflect header stays a number pointer.
func other(x *notHeader) unsafe.Pointer {
	return unsafe.Pointer(x.Data)
}

func main() {
	dst := make([]byte, 8)
	fmt.Println(hostname("host", dst), string(dst[:4]), inline("x"), deref("y"), sliceData([]byte{7}), plain(0) == 0, number(0) == nil, other(&notHeader{}) == nil, param(&reflect.StringHeader{}) == nil, value("v") == nil)
	fmt.Println(aliasData([]byte{9}), (&cell{v: 1}).addr() != nil, aliasWord(0) == nil)
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

	return readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "hdrdata", "main.cs"))
}

func TestHeaderDataPointerEmitsFromHeaderData(t *testing.T) {
	isolateConverterPathGlobals(t)
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	mainCs := convertHeaderDataFixture(t)

	// The three spellings P3 named, the SliceHeader twin and the plain struct: each hands FromHeaderData the BOX.
	for _, arm := range []struct {
		function, want string
	}{
		{"hostname", "@unsafe.Pointer.FromHeaderData(sh)"},
		{"inline", "@unsafe.Pointer.FromHeaderData(Ꮡs.Reinterpret<@string, reflect.StringHeader>())"},
		{"deref", "@unsafe.Pointer.FromHeaderData(h)"},
		{"sliceData", "@unsafe.Pointer.FromHeaderData(h)"},
		{"plain", "@unsafe.Pointer.FromHeaderData(p)"},
		{"param", "@unsafe.Pointer.FromHeaderData(sh)"},
		{"aliasData", "ptr.FromHeaderData(h)"},
	} {
		body := emittedFunctionBody(t, mainCs, arm.function)

		if !strings.Contains(body, arm.want) || strings.Contains(body, ".Data") {
			t.Errorf("%s: expected %q and no number read of .Data, got:\n%s", arm.function, arm.want, body)
		}
	}

	// P3's arm b: a pointer PARAMETER used this way keeps its BOX (ref-lowering's X3 veto), so the call has a box to
	// hand over and the function compiles. A ref-lowered parameter (`ref reflect.StringHeader sh`) would have none.
	if body := emittedFunctionBody(t, mainCs, "param"); !strings.Contains(body, "param(ж<reflect.StringHeader> sh)") {
		t.Errorf("param: the header pointer parameter must keep its box, got:\n%s", body)
	}

	// The alias takes the plain spelling's other arms too, spelled through the alias: a pointer receiver mints from the
	// receiver's own reference, and a named uintptr hops through its underlying (C45, P3's read).
	for _, arm := range []struct {
		function, want string
	}{
		{"addr", "ptr.FromRef(ref c)"},
		{"aliasWord", "(ptr)(uintptr)w"},
	} {
		if body := emittedFunctionBody(t, mainCs, arm.function); !strings.Contains(body, arm.want) {
			t.Errorf("%s: an alias of unsafe.Pointer must take the plain spelling's arm (%q), got:\n%s", arm.function, arm.want, body)
		}
	}

	// The controls: each stays a pointer made from a number, and takes no FromHeaderData.
	for _, arm := range []struct {
		function, want string
	}{
		{"number", "return (@unsafe.Pointer)n;"},
		{"other", "(@unsafe.Pointer)"},
		{"value", "(@unsafe.Pointer)"},
	} {
		body := emittedFunctionBody(t, mainCs, arm.function)

		if !strings.Contains(body, arm.want) || strings.Contains(body, "FromHeaderData") {
			t.Errorf("%s: a form that is not a reflect header's Data must stay a number pointer (%q, no FromHeaderData), got:\n%s", arm.function, arm.want, body)
		}
	}
}
