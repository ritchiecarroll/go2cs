// genericDefinedMapChan_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the generic defined MAP and CHANNEL shells (the hashset end-to-end, COORD 2026-10-02): a
// `type Set[T comparable] map[T]empty` declares the generic `partial struct Set<T>` forward declaration and
// the generic accessibility line, exactly as a generic defined array or slice already does. Without the type
// parameters the declaration is a non-generic struct over an unbound T, every generated member is CS0246,
// and every use (`Set<nint>`) is CS0308. The controls hold the non-generic shells byte-identical and the
// slice and array shells unchanged.

package main

import (
	"go/build"
	"path/filepath"
	"regexp"
	"runtime"
	"strings"
	"testing"
)

func TestGenericDefinedMapAndChanShellsKeepTheirTypeParameters(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/gmc\n\ngo 1.23\n")

	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import "fmt"

type empty struct{}

type Set[T comparable] map[T]empty

type Bag[T comparable] map[T]struct{}

type Pair[K comparable, V any] map[K]V

type Pipe[T any] chan T

type Source[T any] <-chan T

type List[T any] []T

type Grid[T any] [2]T

type PlainMap map[string]int

type PlainPipe chan int

func main() {
	s := Set[int]{1: {}}
	b := make(Bag[string], 2)
	p := Pair[string, int]{"a": 1}
	c := make(Pipe[int], 1)
	var r Source[int] = c
	l := List[int]{1}
	var g Grid[int]
	m := PlainMap{"x": 1}
	q := make(PlainPipe, 1)
	fmt.Println(len(s), len(b), len(p), cap(c), cap(r), len(l), len(g), len(m), cap(q))
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

	mainCs := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "gmc", "main.cs"))
	packageInfo := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "gmc", "package_info.cs"))

	// The declaration carries the parameter list and its constraints, in the same shape the array shell
	// takes: `partial struct Set<T> where T : new();`.
	declaration := func(goType, name, params string) *regexp.Regexp {
		return regexp.MustCompile(`\[GoType\("` + regexp.QuoteMeta(goType) + `"\)\] (\[[^\]]+\] )?partial struct ` + regexp.QuoteMeta(name+params) + `( where [^;]+)?;`)
	}

	for _, want := range []struct{ goType, name, params string }{
		{"map[T, empty]", "Set", "<T>"},
		{"map[T, EmptyStruct]", "Bag", "<T>"},
		{"map[K, V]", "Pair", "<K, V>"},
		{"chan T", "Pipe", "<T>"},
		{"chan T", "Source", "<T>"},
		{"[]T", "List", "<T>"},
		{"[2]T", "Grid", "<T>"},
	} {
		if !declaration(want.goType, want.name, want.params).MatchString(mainCs) {
			t.Errorf("want the generic declaration [GoType(%q)] partial struct %s%s in main.cs:\n%s", want.goType, want.name, want.params, mainCs)
		}

		if !strings.Contains(packageInfo, "public partial struct "+want.name+want.params+" {}") {
			t.Errorf("want the generic accessibility line `public partial struct %s%s {}` in package_info.cs:\n%s", want.name, want.params, packageInfo)
		}

		if strings.Contains(packageInfo, "public partial struct "+want.name+" {}") {
			t.Errorf("a NON-generic accessibility line for %s declares a second, empty type:\n%s", want.name, packageInfo)
		}
	}

	// CONTROLS: a non-generic map and channel shell keep their exact emission.
	for _, want := range []string{
		`[GoType("map[@string, nint]")] partial struct PlainMap;`,
		`[GoType("chan nint")] partial struct PlainPipe;`,
	} {
		if !strings.Contains(mainCs, want) {
			t.Errorf("control: want %q unchanged in main.cs:\n%s", want, mainCs)
		}
	}

	for _, want := range []string{"public partial struct PlainMap {}", "public partial struct PlainPipe {}"} {
		if !strings.Contains(packageInfo, want) {
			t.Errorf("control: want %q unchanged in package_info.cs:\n%s", want, packageInfo)
		}
	}
}
