// pointerArithmetic_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the two emissions of pointerArithmetic.go (docs/phase4/DESIGN-interior-pointer-root.md 4.2 and 4.3):
//
//   - THE ARITHMETIC FORM: `unsafe.Pointer(uintptr(P) + n)` emits `@unsafe.Add(P', n)`. Its controls are the
//     forms the emission does not take, which stay numbers: the pointer on the right of the sum, a masked
//     sum, and a sum over a number held in a variable.
//
//   - THE RECEIVER AT A PROVEN USE: `unsafe.Pointer(recv)` emits the receiver's box as unsafe.Add's first
//     argument, as the P of an arithmetic form and under Go's own `uintptr(...)`. Its controls are the uses
//     not known at the node, which keep `(uintptr)@unsafe.Pointer.FromRef(ref recv)`: a call argument and a
//     return.

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// convertPointerArithmeticFixture converts the one fixture every arm below reads.
func convertPointerArithmeticFixture(t *testing.T) string {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/ptrarith\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import (
	"fmt"
	"unsafe"
)

type hdr struct {
	Len   uint64
	Level int32
	Type  int32
}

func sink(p unsafe.Pointer) uintptr {
	return uintptr(p)
}

// ARITHMETIC 1 — the plain sum.
func sum(p unsafe.Pointer, n uintptr) unsafe.Pointer {
	return unsafe.Pointer(uintptr(p) + n)
}

// ARITHMETIC 2 — a longer chain: everything after the pointer is the offset.
func chain(p unsafe.Pointer, i uintptr) unsafe.Pointer {
	return unsafe.Pointer(uintptr(p) + i<<2 + 1)
}

// ARITHMETIC 3 — a difference keeps Go's wrapping arithmetic.
func back(p unsafe.Pointer, n uintptr) unsafe.Pointer {
	return unsafe.Pointer(uintptr(p) - n)
}

// ARITHMETIC CONTROL 1 — the pointer is not the base of the sum.
func reversed(p unsafe.Pointer, n uintptr) unsafe.Pointer {
	return unsafe.Pointer(n + uintptr(p))
}

// ARITHMETIC CONTROL 2 — a masked sum is not a sum.
func masked(p unsafe.Pointer, n uintptr) unsafe.Pointer {
	return unsafe.Pointer((uintptr(p) + n) &^ 7)
}

// ARITHMETIC CONTROL 3 — a number held in a variable keeps nothing, in Go too.
func held(p unsafe.Pointer, n uintptr) unsafe.Pointer {
	u := uintptr(p)
	return unsafe.Pointer(u + n)
}

// RECEIVER 1 — the first argument of unsafe.Add.
func (h *hdr) level() *int32 {
	return (*int32)(unsafe.Add(unsafe.Pointer(h), 8))
}

// RECEIVER 2 — the P of an arithmetic form.
func (h *hdr) data(offset uintptr) unsafe.Pointer {
	return unsafe.Pointer(uintptr(unsafe.Pointer(h)) + unsafe.Sizeof(*h) + offset)
}

// RECEIVER 3 — the operand of Go's own uintptr conversion.
func (h *hdr) number() uintptr {
	return uintptr(unsafe.Pointer(h))
}

// RECEIVER CONTROL 1 — a call argument: what the callee does with it is not known here.
func (h *hdr) handed() uintptr {
	return sink(unsafe.Pointer(h))
}

// RECEIVER CONTROL 2 — a return.
func (h *hdr) returned() unsafe.Pointer {
	return unsafe.Pointer(h)
}

func main() {
	buf := make([]uint64, 8)
	p := unsafe.Pointer(&buf[0])
	h := (*hdr)(p)
	*h.level() = 5
	fmt.Println(h.Level, sum(p, 8) != nil, chain(p, 1) != nil, back(p, 0) == p, reversed(p, 8) != nil, masked(p, 8) != nil, held(p, 8) != nil)
	fmt.Println(h.data(0) != nil, h.number() != 0, h.handed() != 0, h.returned() != nil)
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

	return readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "ptrarith", "main.cs"))
}

func TestPointerArithmeticFormEmitsUnsafeAdd(t *testing.T) {
	isolateConverterPathGlobals(t)
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	mainCs := convertPointerArithmeticFixture(t)

	// The positives: unsafe.Add over the pointer itself, with everything after it as the offset.
	for _, arm := range []struct {
		function, want string
	}{
		{"sum", "return @unsafe.Add(p, n);"},
		{"chain", "return @unsafe.Add(p, (i << 2) + 1);"},
		{"back", "return @unsafe.Add(p, (uintptr)0 - n);"},
	} {
		if body := emittedFunctionBody(t, mainCs, arm.function); !strings.Contains(body, arm.want) {
			t.Errorf("%s: expected %q, got:\n%s", arm.function, arm.want, body)
		}
	}

	// The controls: each stays a pointer made from a number, and takes no unsafe.Add.
	for _, arm := range []struct {
		function, want string
	}{
		{"reversed", "return (@unsafe.Pointer)(n + (uintptr)p);"},
		{"masked", "(@unsafe.Pointer)("},
		{"held", "return (@unsafe.Pointer)(u + n);"},
	} {
		body := emittedFunctionBody(t, mainCs, arm.function)

		if !strings.Contains(body, arm.want) || strings.Contains(body, "@unsafe.Add(") {
			t.Errorf("%s: a form the arithmetic emission does not take must stay a number (%q, no unsafe.Add), got:\n%s", arm.function, arm.want, body)
		}
	}
}

func TestReceiverPointerEmitsItsBoxAtAProvenUse(t *testing.T) {
	isolateConverterPathGlobals(t)
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	mainCs := convertPointerArithmeticFixture(t)

	// The positives: the box, uncast where golib receives a pointer, bare under Go's own uintptr conversion.
	for _, arm := range []struct {
		function, want string
	}{
		{"level", "@unsafe.Add(@unsafe.Pointer.FromPinnedBox(Ꮡh), 8)"},
		{"data", "return @unsafe.Add(@unsafe.Pointer.FromPinnedBox(Ꮡh), "},
		{"number", "return (uintptr)Ꮡh;"},
	} {
		body := emittedFunctionBody(t, mainCs, arm.function)

		if !strings.Contains(body, arm.want) {
			t.Errorf("%s: expected %q, got:\n%s", arm.function, arm.want, body)
		}

		if strings.Contains(body, "FromRef(") {
			t.Errorf("%s: a use known at the node must not render the receiver through FromRef, got:\n%s", arm.function, body)
		}
	}

	// The controls: a use not known at the node keeps FromRef and its cast.
	for _, arm := range []struct {
		function, want string
	}{
		{"handed", "return sink((uintptr)@unsafe.Pointer.FromRef(ref h));"},
		{"returned", "return (uintptr)@unsafe.Pointer.FromRef(ref h);"},
	} {
		if body := emittedFunctionBody(t, mainCs, arm.function); !strings.Contains(body, arm.want) {
			t.Errorf("%s: a use not known at the node must keep FromRef (%q), got:\n%s", arm.function, arm.want, body)
		}
	}
}
