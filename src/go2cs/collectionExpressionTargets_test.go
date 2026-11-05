// collectionExpressionTargets_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards B3, Go's slice literals as C# collection expressions (docs/PLAN-collection-expressions.md, ruled
// 2026-11-05). A slice literal whose position gives C# its slice<T> target is a bare `[1, 2]`; every other
// position keeps golib's builtin, `slice<nint>([1, 2])`. A slice or array enumerates its values, so Go's pair
// loop ranges golib's range(x) and its index-only loop ranges the length.

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

func TestSliceLiteralsAtTargetedPositionsAreCollectionExpressions(t *testing.T) {
	isolateConverterPathGlobals(t)
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/b3\n\ngo 1.23\n")

	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

type pt struct {
	xs   []int
	name string
}

type names []string

var pkg = []int{1, 2}
var pkgTyped []string = []string{"a"}
var pkgAny any = []int{3}

func one() []int { return []int{4, 5} }
func two() ([]int, error) { return []int{6}, nil }
func take(xs []int) int { return len(xs) }
func takeAny(x any) {}
func gen[T any](xs []T) int { return len(xs) }
func vari(xs ...[]int) {}
func f() int { return 1 }

func main() {
	local := []int{7}
	local = []int{9}
	nested := [][]int{{1}, {2, 3}, []int{4}}
	p := pt{xs: []int{10}, name: "n"}
	q := pt{[]int{11}, "q"}
	m := map[string][]int{"k": {12}}
	take([]int{13})
	takeAny([]int{14})
	gen([]int{15})
	vari([]int{16})
	withCall := [][]int{{f()}}
	local = []int{f()}
	empty := [][]int{{}}
	var n names = []string{"x"}
	fn := func() []int { return []int{17} }
	defer take([]int{18})
	for _, v := range []int{19} {
		println(v)
	}
	println(len(local), len(nested), len(p.xs), len(q.xs), len(m), len(withCall), len(empty), len(n), len(fn()))
	println(len(pkg), len(pkgTyped), pkgAny != nil, len(one()))
}
`)

	writeModuleFile(t, filepath.Join(appDir, "ranges.go"), `package main

type nat []uint
type tab [4]uint32

func sum[S ~[]E, E ~int](s S) (t E) {
	for _, v := range s {
		t += v
	}
	return
}

func ranges() {
	s := []int{1, 2, 3}
	for i, v := range s {
		println(i, v)
	}
	for i := range s {
		println(i)
	}
	var a [3]int
	for i, v := range a {
		println(i, v)
	}
	p := &a
	for i, v := range p {
		println(i, v)
	}
	for i := range p {
		println(i)
	}
	x := nat{4, 5}
	for i, d := range x {
		println(i, d)
	}
	var t tab
	pt := &t
	for i, w := range pt {
		println(i, w)
	}
	m := map[string]int{"a": 1}
	for k, v := range m {
		println(k, v)
	}
	println(sum(s))
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

	mainCs := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "b3", "main.cs"))

	// Targeted: C# knows the target is the literal's own slice<T>.
	for _, want := range []string{
		`internal static slice<nint> pkg = [1, 2];`,
		`internal static slice<@string> pkgTyped = ["a"];`,
		`return [4, 5];`,
		`local = [9];`,
		`slice<slice<nint>>([[1], [2, 3], [4]])`,
		`new pt(xs: [10], name: "n")`,
		`new pt([11], "q")`,
		`new map<@string, slice<nint>>{["k"] = [12]}`,
		`take([13]);`,
		`slice<slice<nint>>([[]])`,
	} {
		if !strings.Contains(mainCs, want) {
			t.Errorf("want %q: a slice literal at a targeted position is a bare collection expression:\n%s", want, mainCs)
		}
	}

	// Untargeted, or an element holding a call: the builtin stays.
	for _, want := range []string{
		`internal static any pkgAny = slice<nint>([3]);`,
		`return (slice<nint>([6]), default);`,
		`var local = slice<nint>([7]);`,
		`takeAny(slice<nint>([14]));`,
		`gen(slice<nint>([15]));`,
		`vari(slice<nint>([16]));`,
		`slice<slice<nint>>([slice<nint>([f()])])`,
		`local = slice<nint>([f()]);`,
		`names n = slice<@string>(["x"]);`,
		`slice<nint>([17])`,
		`slice<nint>([18])`,
		`range(slice<nint>([19]))`,
	} {
		if !strings.Contains(mainCs, want) {
			t.Errorf("want %q: an untargeted slice literal keeps the builtin:\n%s", want, mainCs)
		}
	}

	rangesCs := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "b3", "ranges.cs"))

	for _, want := range []string{
		`foreach (var (_, v) in range(s))`,                  // a type parameter's sequence
		`foreach (var (i, v) in range(s))`,                  // a slice
		`foreach (var i in range(len(s)))`,                  // index only
		`foreach (var (i, v) in range(a.ΔRangeSnapshot()))`, // an array value ranges its copy
		`foreach (var (i, v) in range(p.Value))`,            // through a pointer to an array
		`foreach (var i in range(p, 3))`,                    // a pointer to an array, index only
		`foreach (var (i, d) in range<nuint>(x))`,           // a named slice type names its element type
		`foreach (var (i, w) in range(pt.Value))`,           // a named array type through a pointer
		`foreach (var (k, v) in m)`,                         // a map keeps its own pairs
	} {
		if !strings.Contains(rangesCs, want) {
			t.Errorf("want %q:\n%s", want, rangesCs)
		}
	}
}

// A package that declares its own `len` method (index/suffixarray's `func (a *ints) len() int`) shadows golib's
// using-static builtin inside the package class, so an index-only range qualifies the length it ranges.
func TestAnIndexOnlyRangeQualifiesAShadowedLen(t *testing.T) {
	isolateConverterPathGlobals(t)
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/b3len\n\ngo 1.23\n")

	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

type ints struct{ v []int }

func (a *ints) len() int { return len(a.v) }

func main() {
	s := []int{1, 2}
	for i := range s {
		println(i)
	}
	a := &ints{s}
	println(a.len())
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

	mainCs := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "b3len", "main.cs"))

	if want := "foreach (var i in range(builtin.len(s)))"; !strings.Contains(mainCs, want) {
		t.Errorf("want %q: a shadowed len is qualified in an index-only range:\n%s", want, mainCs)
	}
}
