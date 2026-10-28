// emptyStructLiteralCtor_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards parity seat #21 (Z1): an empty struct literal `T{}` emits `new T()`, as Go writes it, for a
// local, generic, cross-package or empty struct and behind `&`. A wrapper over another named struct
// still constructs that one. A named slice or map `T{}` keeps its own form, an empty non-nil value
// (its `(NilType)` constructor would mean nil).

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

func TestEmptyStructLiteralEmitsParameterlessCtor(t *testing.T) {
	isolateConverterPathGlobals(t)
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/z1\n\ngo 1.23\n")

	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import (
	"strings"
	"time"
)

type point struct{ x, y int }
type grid struct{ cells [3]int }
type pair[T any] struct{ a, b T }
type wrapped point
type empty struct{}
type IntSlice []int
type StrMap map[string]int

func main() {
	p := point{}
	g := grid{}
	q := pair[string]{}
	w := wrapped{}
	e := empty{}
	b := strings.Builder{}
	t := time.Time{}
	pp := &point{}
	s := IntSlice{}
	m := StrMap{}
	_, _, _, _, _, _, _, _, _, _ = p, g, q, w, e, b, t, pp, s, m
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

	mainCs := strings.ReplaceAll(readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "z1", "main.cs")), "\r\n", "\n")

	for _, want := range []string{
		"var p = new point();",
		"var g = new grid();",               // array fields: the parameterless ctor allocates them
		"var q = new pair<@string>();",      // a generic struct
		"var w = new wrapped(new point());", // a wrapper constructs the struct it wraps
		"var e = new empty();",
		"var b = new strings.Builder();", // cross-package
		"var t = new time.Time();",
		"var pp = Ꮡ(new point());",
		"var s = new IntSlice(new nint[]{}.slice());", // named slice and map: unchanged, and not nil
		"var m = new StrMap(new map<@string, nint>{});",
	} {
		if !strings.Contains(mainCs, want) {
			t.Errorf("want %q in the emitted main.cs:\n%s", want, mainCs)
		}
	}

	if strings.Contains(mainCs, "point(nil)") || strings.Contains(mainCs, "grid(nil)") {
		t.Errorf("no empty struct literal may pass NilType:\n%s", mainCs)
	}
}
