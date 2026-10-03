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
//   - a TWO-hop selector keeps today's raw route — the door folds one field's offset, no more.

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

// CONTROL 3 — a two-hop selector.
func nested(o *outer) byte {
	n := (*[2]byte)(unsafe.Pointer(&o.inner.Port))
	return n[0]
}

func main() {
	sa := &sockaddr{}
	setPort(sa, 80)
	other := [2]byte{1, 2}
	xs := [4]byte{1, 2, 3, 4}
	h := &holder{cheap: 1}
	w0, w1 := words(h)
	fmt.Println(portOf(sa), repointed(sa, &other), sameElem(&xs), nested(&outer{}), w0, w1)
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
	if body := emittedFunctionBody(t, mainCs, "portOf"); !strings.Contains(body, "@unsafe.ArrayPointer<byte>.Of(Ꮡsa.of(sockaddr.ᏑPort), 2);") || strings.Count(body, "Ꮡsa.of(sockaddr.ᏑPort)") != 1 {
		t.Errorf("portOf: expected the helper over Ꮡsa.of(sockaddr.ᏑPort), named once, with N 2, got:\n%s", body)
	}

	if body := emittedFunctionBody(t, mainCs, "words"); !strings.Contains(body, "@unsafe.ArrayPointer<uint32>.Of(Ꮡh.of(holder.Ꮡcheap), 2);") || strings.Count(body, "Ꮡh.of(holder.Ꮡcheap)") != 1 {
		t.Errorf("words: expected the helper over Ꮡh.of(holder.Ꮡcheap), named once, with N 2, got:\n%s", body)
	}

	// CONTROL 1 — the re-pointed local keeps Value: after `q = other` it may name a managed array.
	if body := emittedFunctionBody(t, mainCs, "repointed"); !strings.Contains(body, "q.Value[0]") || strings.Contains(body, "q.ElementRef(") {
		t.Errorf("repointed: a re-pointed local must keep q.Value[0], got:\n%s", body)
	}

	// CONTROL 2 — a same-element view keeps AliasPointer and takes no door.
	if body := emittedFunctionBody(t, mainCs, "sameElem"); !strings.Contains(body, "array<byte>.AliasPointer(") || strings.Contains(body, "@unsafe.ArrayPointer<") {
		t.Errorf("sameElem: a same-element view must keep array<T>.AliasPointer, got:\n%s", body)
	}

	// CONTROL 3 — a two-hop selector keeps today's raw route.
	if body := emittedFunctionBody(t, mainCs, "nested"); strings.Contains(body, "@unsafe.ArrayPointer<") || !strings.Contains(body, "(ж<array<byte>>)(uintptr)(") {
		t.Errorf("nested: a two-hop selector must keep today's raw route, got:\n%s", body)
	}
}
