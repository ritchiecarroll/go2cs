// unsafePointerWordRead_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the pointer-WORD read `*(*unsafe.Pointer)(unsafe.Pointer(&x))` (reinterpretManagedEmission's
// carrying arm): for a Go pointer, an unsafe.Pointer and a func source, the word is READ FROM x's
// storage -- the pointer itself, the Pointer itself, the func's GoFuncCookie -- where the carrying form
// used to yield the token of x's BOX, i.e. `&x`, one level off (ruling 2026-09-28 15:03 (1), folded into
// the func cookie seat). The controls carry as much weight: a uintptr source keeps its exact read, a
// channel keeps the box token its consumers are pinned to, and the two-level func read keeps its form.

package main

import (
	"go/build"
	"path/filepath"
	"regexp"
	"runtime"
	"testing"
)

func TestUnsafePointerWordReadTakesTheStoredWord(t *testing.T) {
	isolateConverterPathGlobals(t)
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/wordread\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import (
	"fmt"
	"unsafe"
)

// POSITIVES - the word is the stored value.
func onFunc(f func()) uintptr                  { return uintptr(*(*unsafe.Pointer)(unsafe.Pointer(&f))) }
func onPointer(p *int) unsafe.Pointer          { return *(*unsafe.Pointer)(unsafe.Pointer(&p)) }
func onPointerLocal(n int) unsafe.Pointer {
	q := &n
	return *(*unsafe.Pointer)(unsafe.Pointer(&q))
}
func onUnsafe(u unsafe.Pointer) unsafe.Pointer { return *(*unsafe.Pointer)(unsafe.Pointer(&u)) }

// CONTROLS - forms that must not move.
func onUintptr(x uintptr) unsafe.Pointer       { return *(*unsafe.Pointer)(unsafe.Pointer(&x)) }
func onChan(c chan int) unsafe.Pointer         { return *(*unsafe.Pointer)(unsafe.Pointer(&c)) }
func onDoubleFunc(fn func()) unsafe.Pointer    { return **(**unsafe.Pointer)(unsafe.Pointer(&fn)) }

// POSITIVES in runtime's nestedCall shape: a variadic argument, a deferring function, a statement call.
func sink(a ...uintptr) uintptr { return a[len(a)-1] }
func probeVariadic(f func()) uintptr { return sink(0, 0, uintptr(*(*unsafe.Pointer)(unsafe.Pointer(&f)))) }
func probeDefer(f func()) uintptr {
	defer fmt.Print("")
	return uintptr(*(*unsafe.Pointer)(unsafe.Pointer(&f)))
}
func probeStmt(f func()) {
	sink(0, uintptr(*(*unsafe.Pointer)(unsafe.Pointer(&f))))
}

func main() {
	n := 1
	fmt.Println(onFunc(func() {}) != 0, onPointer(&n) != nil, onPointerLocal(2) != nil, onUnsafe(unsafe.Pointer(&n)) != nil)
	fmt.Println(onUintptr(7) != nil, onChan(make(chan int)) != nil, onDoubleFunc(func() {}) != nil)
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

	mainCs := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "wordread", "main.cs"))

	bodyOf := func(name string) string {
		match := regexp.MustCompile(`(?m)^( *)[^\n]*\b` + name + `\([^\n]*\n(?s:.*?)^( *)\}\s*$`).FindString(mainCs)

		if match == "" {
			match = regexp.MustCompile(`(?m)^[^\n]*\b` + name + `\([^\n]*$`).FindString(mainCs)
		}

		if match == "" {
			t.Fatalf("%s: not found in the emission:\n%s", name, mainCs)
		}

		return match
	}

	boxToken := regexp.MustCompile(`new @unsafe\.Pointer\(\(uintptr\)`)

	cases := []struct {
		name string
		want *regexp.Regexp
		// boxTokenAllowed: the control arms keep the carrying box token.
		boxTokenAllowed bool
	}{
		// The pointer arms take the STORED pointer: the parameter's own box, the local's own value.
		{"onFunc", regexp.MustCompile(`@unsafe\.Pointer\.OfFunc\(f\)`), false},
		{"onPointer", regexp.MustCompile(`@unsafe\.Pointer\.FromPinnedBox\(p\)`), false},
		{"onPointerLocal", regexp.MustCompile(`@unsafe\.Pointer\.FromPinnedBox\(q\)`), false},
		// Ꮡ(u) is folded to Ꮡu by the emitter, so the read is the stored Pointer itself.
		{"probeVariadic", regexp.MustCompile(`sink\(0, 0, \(uintptr\)\(~Ꮡ\(@unsafe\.Pointer\.OfFunc\(f\)\)\)\)`), false},
		{"probeDefer", regexp.MustCompile(`@unsafe\.Pointer\.OfFunc\(f\)`), false},
		{"probeStmt", regexp.MustCompile(`@unsafe\.Pointer\.OfFunc\(f\)`), false},
		{"onUnsafe", regexp.MustCompile(`~Ꮡu\b`), false},
		{"onUintptr", regexp.MustCompile(`new @unsafe\.Pointer\(~Ꮡx\)`), false},
		{"onChan", boxToken, true},
		{"onDoubleFunc", boxToken, true},
	}

	for _, c := range cases {
		body := bodyOf(c.name)

		if !c.want.MatchString(body) {
			t.Errorf("%s: expected %s; emission:\n%s", c.name, c.want, body)
		}

		if !c.boxTokenAllowed && boxToken.MatchString(body) {
			t.Errorf("%s: must not read the BOX token (&x, one level off); emission:\n%s", c.name, body)
		}
	}
}
