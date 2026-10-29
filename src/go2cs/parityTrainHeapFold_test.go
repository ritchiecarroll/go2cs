// parityTrainHeapFold_test.go - Gbtc
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

// Seat #29 of the visual-parity train (docs/PLAN-parity-train.md), H2: a heap-boxed local declared by a single-LHS
// `x := rhs` folds its box and its value into one statement, `ref var x = ref heap<T>(rhs, out var Ꮡx);`, where the
// split form declared a zero box and assigned the value on the next line. The type argument is always explicit
// (COORD's R1: inference would type `10` as int, not nint). A multi-LHS define, a tuple deconstruction, a loop's
// per-iteration copy and a `var x T` declaration keep the box apart from the value (R2).
//
// The fixture is HeapFoldShapes' Go program, the behavioral guard that runs these shapes against Go.

const heapFoldFixture = `
package main

import "fmt"

// Each local below has its address taken, so its declaration boxes it on the heap. A single-LHS x := rhs
// folds the box and its value into one statement; the forms that keep the box apart from the value are
// here too, and every value is read back through its pointer so the output shows the box holds it.

type grid struct {
	cells [3]int
	name  string
}

func bump(p *int) { *p++ }

func mark(g *grid, i int) { g.cells[i] = 9 }

func pair() (int, error) { return 7, nil }

// Pointers stored here outlive the call that took them, so each local they point at must live on the heap.
var (
	keptInts  []*int
	keptGrids []*grid
)

func main() {
	// An untyped constant: the box is the Go type (int), not the constant's C# default.
	a := 10
	keptInts = append(keptInts, &a)
	bump(&a)
	fmt.Println("a:", a)

	// A string literal.
	s := "hi"
	ps := &s
	*ps += "!"
	fmt.Println("s:", s)

	// A struct holding an array, then a COPY of it boxed by := and mutated through its pointer after the
	// fold: the copy is independent, so the source keeps its cells.
	g := grid{name: "g"}
	keptGrids = append(keptGrids, &g)
	mark(&g, 0)
	h := g
	keptGrids = append(keptGrids, &h)
	mark(&h, 1)
	h.name = "h"
	fmt.Println("g:", g.cells, g.name, "h:", h.cells, h.name, "kept:", keptGrids[0].cells, keptGrids[1].cells, *keptInts[0])

	// A closure that takes the address of a folded local and writes through it.
	c := 1
	inc := func() { p := &c; *p += 10 }
	inc()
	inc()
	fmt.Println("c:", c)

	// An anonymous struct.
	anon := struct{ v int }{v: 3}
	pa := &anon
	pa.v *= 2
	fmt.Println("anon:", anon.v)

	// A multi-LHS define keeps the box apart from the value: Go evaluates every RHS first.
	x, y := 1, 2
	px, py := &x, &y
	*px, *py = *py, *px
	fmt.Println("x y:", x, y)

	// A multi-value define keeps the split too.
	n, err := pair()
	pn, pe := &n, &err
	*pn += 1
	fmt.Println("n err:", n, *pe)

	// A for-init variable boxed per iteration keeps its box ahead of the loop.
	var keep []*int
	for i := 0; i < 3; i++ {
		keep = append(keep, &i)
	}
	fmt.Println("loop:", *keep[0], *keep[1], *keep[2])

	// A declaration and a later assignment are two Go statements and stay two.
	var z int
	pz := &z
	z = 5
	fmt.Println("z:", *pz)
}
`

func convertHeapFoldFixture(t *testing.T) string {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/heapfold\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), heapFoldFixture)

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

	return readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "heapfold", "main.cs"))
}

func TestHeapFoldEmission(t *testing.T) {
	isolateConverterPathGlobals(t)
	mainCs := strings.ReplaceAll(convertHeapFoldFixture(t), "\r\n", "\n")

	lines := make(map[string]bool)

	for _, line := range strings.Split(mainCs, "\n") {
		lines[strings.TrimSpace(line)] = true
	}

	wants := map[string][]string{
		"folded": {
			`ref var a = ref heap<nint>(10, out var Ꮡa);`,
			`ref var s = ref heap<@string>("hi", out var Ꮡs);`,
			`ref var g = ref heap<grid>(new grid(name: "g"), out var Ꮡg);`,
			`ref var h = ref heap<grid>(g.ΔClone(), out var Ꮡh);`,
			`ref var c = ref heap<nint>(1, out var Ꮡc);`,
			`ref var anon = ref heap<main_anon>(new main_anon(v: 3), out var Ꮡanon);`,
		},
		"kept apart": {
			`ref var x = ref heap<nint>(out var Ꮡx);`, `x = 1;`,
			`ref var y = ref heap<nint>(out var Ꮡy);`, `y = 2;`,
			`ref var n = ref heap<nint>(out var Ꮡn);`, `(n, err) = pair();`,
			`ref var i = ref heap<nint>(out var Ꮡi);`, `i = iᴛ1;`,
			`ref var z = ref heap(new nint(), out var Ꮡz);`, `z = 5;`,
		},
	}

	for kind, fragments := range wants {
		for _, want := range fragments {
			if !lines[want] {
				t.Errorf("%s: no emitted line reads %q", kind, want)
			}
		}
	}

	// The type argument is never inferred: every `ref heap(` is the `var x T` declaration's `heap(new T(), …)`.
	if all, typed := strings.Count(mainCs, "ref heap("), strings.Count(mainCs, "ref heap(new "); all != typed {
		t.Errorf("a heap box infers its type argument: %d `ref heap(`, only %d of them `ref heap(new `", all, typed)
	}

	if t.Failed() {
		t.Logf("emitted main.cs:\n%s", mainCs)
	}
}
