// valuePun_test.go - Gbtc
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

const valuePunFixture = `package pun

import "unsafe"

type celsius float64

// accepted: math's four bodies, and a local
func f64bits(f float64) uint64   { return *(*uint64)(unsafe.Pointer(&f)) }
func f64frombits(b uint64) float64 { return *(*float64)(unsafe.Pointer(&b)) }
func f32bits(f float32) uint32   { return *(*uint32)(unsafe.Pointer(&f)) }
func f32frombits(b uint32) float32 { return *(*float32)(unsafe.Pointer(&b)) }
func inf() float64 {
	var bits uint64 = 0x7FF0000000000000
	return *(*float64)(unsafe.Pointer(&bits))
}

// accepted read, but x's address is ALSO taken elsewhere: the read bitcasts, the box stays
var sink *uint32

func kept(b uint32) float32 {
	sink = &b
	return *(*float32)(unsafe.Pointer(&b))
}

// refused
func written(x, y float32) float32 {
	*(*uint32)(unsafe.Pointer(&x)) |= *(*uint32)(unsafe.Pointer(&y))
	return x
}
func unequal(x uint32) uint64    { return uint64(*(*uint16)(unsafe.Pointer(&x))) }
func named(c celsius) uint64     { return *(*uint64)(unsafe.Pointer(&c)) }
func platformWidth(i int) uint64 { return *(*uint64)(unsafe.Pointer(&i)) }
`

// TestValuePunReadRendersAsABitcastWithoutABox controls the value-pun recognition BOTH ways (the
// Float*bits seat): `*(*U)(unsafe.Pointer(&x))` read over same-size predeclared sized numerics renders
// as golib's `bitcast<T, U>(x)` and leaves x unboxed when that was its only address use; a write, an
// unequal size, a named type and a platform-width int keep the reinterpret.
//
// RED at fa18863b94: every accepted body emitted `heap(…)` and `~Ꮡx.Reinterpret<T, U>()`.
func TestValuePunReadRendersAsABitcastWithoutABox(t *testing.T) {
	isolateConverterPathGlobals(t)
	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/pun\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "pun.go"), valuePunFixture)

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{
		goRoot:              goRoot,
		goPath:              build.Default.GOPATH,
		go2csPath:           filepath.Join(root, "out"),
		recurse:             true,
		targetPlatform:      "linux/amd64",
		indentSpaces:        4,
		preferVarDecl:       true,
		useChannelOperators: true,
	}

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	if err := NewModuleConverter(options).ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	emitted := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "pun", "pun.cs"))

	cases := []struct {
		declaration string
		want        string // must appear
		boxed       bool   // whether x must still be heap-boxed
	}{
		{"internal static uint64 f64bits(", "bitcast<float64, uint64>(f)", false},
		{"internal static float64 f64frombits(", "bitcast<uint64, float64>(b)", false},
		{"internal static uint32 f32bits(", "bitcast<float32, uint32>(f)", false},
		{"internal static float32 f32frombits(", "bitcast<uint32, float32>(b)", false},
		{"internal static float64 inf(", "bitcast<uint64, float64>(bits)", false},
		{"internal static float32 kept(", "bitcast<uint32, float32>(b)", true},
	}

	for _, c := range cases {
		body := liftFunctionBody(t, emitted, c.declaration)

		if !strings.Contains(body, c.want) {
			t.Errorf("%s: want %q:\n%s", c.declaration, c.want, body)
		}

		if strings.Contains(body, "Reinterpret<") {
			t.Errorf("%s: an accepted read still reinterprets:\n%s", c.declaration, body)
		}

		if boxed := strings.Contains(body, "heap("); boxed != c.boxed {
			t.Errorf("%s: heap box = %v, want %v:\n%s", c.declaration, boxed, c.boxed, body)
		}
	}

	for _, declaration := range []string{
		"internal static uint64 unequal(",
		"internal static uint64 named(",
		"internal static uint64 platformWidth(",
	} {
		body := liftFunctionBody(t, emitted, declaration)

		if strings.Contains(body, "bitcast<") {
			t.Errorf("%s: a refused shape rendered a bitcast:\n%s", declaration, body)
		}
	}

	// the write keeps the aliasing reinterpret; its READ operand (y) is itself an accepted pun
	if body := liftFunctionBody(t, emitted, "internal static float32 written("); !strings.Contains(body, "Reinterpret<float32, uint32>()") {
		t.Errorf("written: the write must keep the reinterpret:\n%s", body)
	}
}
