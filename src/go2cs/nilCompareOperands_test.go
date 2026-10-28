// nilCompareOperands_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards parity seat #20 (N1 stage 1, D1): an untyped nil compared with an interface or a func emits
// golib's `nil`, as Go writes it, and every other typeless zero emits `default` (no `!`). A type
// parameter keeps its own path whatever its constraint: C# binds no user-defined operator on a type
// parameter (CS8761), so a map or slice core reads IsNil and an erased pointer core is a pointer.
// Stage 2 (seat #24) widened the rule to slices, maps and chans once golib's proof showed their nil
// operators agree with Go for every construction kind (GolibTests NilHeaderEqualityTests).

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

func TestNilCompareRendersNilForInterfaceAndFuncOperands(t *testing.T) {
	isolateConverterPathGlobals(t)
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/nilcmp\n\ngo 1.23\n")

	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

type handler func(int) int

func iface(err error, a any) bool  { return err != nil && nil != a }
func fn(f func(), h handler) bool  { return f == nil || nil == h }
func ptr(p *int) bool              { return p == nil }
func slc(s []int) bool             { return s == nil }
func mp(m map[string]int) bool     { return m == nil }
func ch(c chan int) bool           { return c == nil }
func tpMap[M ~map[string]int](m M) bool { return m == nil }
func tpPtr[P *int](p P) bool       { return p == nil }
func tpFunc[F ~func()](f F) bool   { return f == nil }
func zero() error                  { return nil }

func main() {
	_ = iface(nil, nil)
	_ = fn(nil, nil)
	_ = ptr(nil)
	_ = slc(nil)
	_ = mp(nil)
	_ = ch(nil)
	_ = tpMap(map[string]int(nil))
	_ = tpPtr((*int)(nil))
	_ = tpFunc(func() {})
	_ = zero()
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

	mainCs := strings.ReplaceAll(readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "nilcmp", "main.cs")), "\r\n", "\n")

	for _, want := range []string{
		"return err != nil && nil != a;", // interfaces, either side
		"return f == nil || nil == h;",   // funcs, named or not, either side
		"return p == nil;",               // pointers already took the pointer context; an un-aliased pointer parameter keeps its Go name (seat #25 R5)
		"return s == nil;",               // slices, maps and chans: stage 2 (seat #24)
		"return m == nil;",
		"return c == nil;",
		"return m.IsNil;", // a map-core type parameter keeps IsNil
		"return default;", // D1: the typeless zero carries no `!`
	} {
		if !strings.Contains(mainCs, want) {
			t.Errorf("want %q in the emitted main.cs:\n%s", want, mainCs)
		}
	}

	// No type parameter takes the interface/func arm, whatever its core: `f == nil` on F would bind no
	// operator (CS8761), so tpFunc must not read `f == nil`.
	tpFuncBody := mainCs[strings.Index(mainCs, "tpFunc<F>"):]
	tpFuncBody = tpFuncBody[:strings.Index(tpFuncBody, "\n}")]

	if strings.Contains(tpFuncBody, "== nil") {
		t.Errorf("a func-core type parameter must keep its own path, not `== nil`:\n%s", tpFuncBody)
	}

	if strings.Contains(mainCs, "default!") {
		t.Errorf("no emitted `default!` may remain (D1):\n%s", mainCs)
	}
}
