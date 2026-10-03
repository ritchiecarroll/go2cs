// multiValueFuncDefine_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the parallel `:=` of function values (the hashset test's `inA, inB := func..., func...`, COORD
// 2026-10-03): a C# lambda or method group has no type inside a tuple literal, so an all-new parallel define
// whose right-hand side holds a func literal, a method group, a method value or a generic func instantiation
// cannot take the tuple form `var (a, b) = (x, y);` (CS8130). It takes the split form the int and string
// exclusions already take, one declaration per target. A parallel define of ordinary values, and a
// tuple-returning call, keep the tuple.

package main

import (
	"go/build"
	"path/filepath"
	"regexp"
	"runtime"
	"strings"
	"testing"
)

func TestAParallelDefineOfFunctionValuesTakesSeparateDeclarations(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/mfd\n\ngo 1.23\n")

	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

type box struct{ n int }

func (b box) get() int    { return b.n }
func (b *box) bump(d int) { b.n += d }

func double(v int) int { return v * 2 }
func triple(v int) int { return v * 3 }

func identity[T any](v T) T { return v }

func pair() (func() int, int) { return func() int { return 7 }, 1 }

func main() {
	n := 3
	small, even := func(v int) bool { return v < n }, func(v int) bool { return v%2 == 0 }
	d, t := double, triple
	b := &box{n: 2}
	get, bump := b.get, b.bump
	show, p := func() int { return 1 }, b
	id, id2 := identity[string], identity[float64]
	q, r := b, b
	f, one := pair()
	bump(3)
	println(small(2), even(2), d(2), t(2), get(), show(), p.n, id("go"), id2(1.5), q.n, r.n, f(), one)
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

	mainCs := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "mfd", "main.cs"))

	// Each function-valued parallel define declares its targets one by one: no tuple form, and a declaration of
	// each target (`<type or var> name = ...;`).
	for _, pair := range [][2]string{{"small", "even"}, {"d", "t"}, {"get", "bump"}, {"show", "p"}, {"id", "id2"}} {
		if strings.Contains(mainCs, "var ("+pair[0]+", "+pair[1]+") =") {
			t.Errorf("`%s, %s := ...` holds a function value, so it must not take the tuple form (CS8130):\n%s", pair[0], pair[1], mainCs)
		}

		for _, name := range pair {
			if !regexp.MustCompile(`(?m)^\s*[^\s=(][^=(]*\s` + regexp.QuoteMeta(name) + ` = `).MatchString(mainCs) {
				t.Errorf("want a declaration of %s on its own:\n%s", name, mainCs)
			}
		}
	}

	// CONTROLS: a parallel define of ordinary values, and a tuple-returning call, keep the tuple.
	for _, want := range []string{"var (q, r) = (b, b);", "var (f, one) = pair();"} {
		if !strings.Contains(mainCs, want) {
			t.Errorf("control: want %q unchanged:\n%s", want, mainCs)
		}
	}
}
