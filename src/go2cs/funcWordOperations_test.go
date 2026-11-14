// funcWordOperations_test.go - Gbtc
// Copyright (c) 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the func-word shapes (funcWordOperations.go): a func value carried as a number in the six
// structural classes ccgo writes, each emitted through golib's GoFuncCookie, and the near misses that
// keep today's pointer reinterpret. The m1 CALL SITE is asserted too: it must stay a method-group
// conversion, which C# caches per site, never `new F(M)`, which mints a fresh delegate per evaluation
// (sqlite-cs S1's stop line X1.6).

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

func TestFuncWordShapes(t *testing.T) {
	isolateConverterPathGlobals(t)
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), `module example.com/fword

go 1.23
`)
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import (
	"fmt"
	"unsafe"
)

func inc(a int) int { return a + 1 }

func ccgoFP(f interface{}) uintptr {
	return (*[2]uintptr)(unsafe.Pointer(&f))[1]
}

func ccgoFPLocal(f interface{}) uintptr {
	type iface [2]uintptr
	return (*iface)(unsafe.Pointer(&f))[1]
}

func typeWord(f interface{}) uintptr {
	return (*[2]uintptr)(unsafe.Pointer(&f))[0]
}

func wordOf(f func(int) int) uintptr {
	return *(*uintptr)(unsafe.Pointer(&f))
}

func wordOfParam[T any](v T) uintptr {
	return *(*uintptr)(unsafe.Pointer(&struct{ f T }{v}))
}

func notAFunc(n int) uintptr {
	return *(*uintptr)(unsafe.Pointer(&n))
}

func main() {
	w := ccgoFP(inc)
	fmt.Println(ccgoFPLocal(inc) == w)
	lit := *(*uintptr)(unsafe.Pointer(&struct{ f func(int) int }{inc}))
	fmt.Println((*(*func(int) int)(unsafe.Pointer(&struct{ uintptr }{w})))(1))
	stored := *(*func(int) int)(unsafe.Pointer(&struct{ uintptr }{lit}))
	fmt.Println(stored(2), (*struct{ f func(int) int })(unsafe.Pointer(&struct{ uintptr }{w})).f(3))

	var slot uintptr
	*(*func(int) int)(unsafe.Pointer(&slot)) = inc

	var g func(int) int
	*(*uintptr)(unsafe.Pointer(&g)) = slot

	var memory [1]uintptr
	*(*func(int) int)(unsafe.Pointer(&memory)) = inc

	fmt.Println(g(4), wordOf(g) == w, wordOfParam(inc) == w, typeWord(inc) != 0, notAFunc(5) != 0, memory[0] != 0)
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
	mainCs := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "fword", "main.cs"))

	for _, want := range []string{
		"return (uintptr)GoFuncCookie.Of((Delegate)f);",                      // m1
		"(ccgoFP(inc), out var Ꮡw);",                                         // m1 call site: a method-group conversion
		"return (uintptr)GoFuncCookie.Of(f);",                                // m2, a func variable
		"return (uintptr)GoFuncCookie.Of((Delegate?)(object?)v);",            // m2, struct{ f T }
		"((uintptr)GoFuncCookie.Of((Func<nint, nint>)inc), out var Ꮡlit);",   // m2, struct{ f func }{fn}
		"fmt.Println(GoFuncCookie.Func<Func<nint, nint>>(w)(1));",            // r1, called at once
		"var stored = GoFuncCookie.Func<Func<nint, nint>>(lit);",             // r1, stored
		"fmt.Println(stored(2), GoFuncCookie.Func<Func<nint, nint>>(w)(3));", // r2
		"slot = (uintptr)GoFuncCookie.Of((Func<nint, nint>)inc);",            // w2
		"g = GoFuncCookie.Func<Func<nint, nint>>(slot);",                     // w1
	} {
		if !strings.Contains(mainCs, want) {
			t.Errorf("func word shape must emit %q: %s", want, mainCs)
		}
	}

	if got := strings.Count(mainCs, "return (uintptr)GoFuncCookie.Of((Delegate)f);"); got != 2 {
		t.Errorf("both m1 spellings (inline, and ccgo's with a local array type first) must read the word, got %d: %s", got, mainCs)
	}

	if strings.Contains(mainCs, "new Func<nint, nint>(inc)") {
		t.Errorf("a func word is never minted from `new F(M)`, which defeats C#'s per-site delegate cache: %s", mainCs)
	}

	// The near misses keep today's emission: an interface's TYPE word, a word read from an int, and a
	// func stored into memory that is no uintptr variable. Ten GoFuncCookie uses are the ten shapes
	// above less the call site, plus the second m1 spelling.
	if got := strings.Count(mainCs, "GoFuncCookie."); got != 10 {
		t.Errorf("want 10 GoFuncCookie uses (the recognised shapes only), got %d: %s", got, mainCs)
	}
}
