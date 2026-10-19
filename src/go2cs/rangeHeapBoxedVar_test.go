// rangeHeapBoxedVar_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the per-iteration heap box on the STRING, CHAN and INT range forms. A range variable the
// escape analysis boxes must get a fresh box each pass (Go 1.22 per-iteration semantics), declared
// inside the loop body from a foreach temp. These three arms declared the box once BEFORE the loop
// instead: where the foreach also declared the name, the two declarations met in one scope
// (CS0136, os's Go 1.26 tests `for o := range write` and `for linkKind := range testFileMax`), and
// where the body iterated a temp, every stored `&i` shared one box. The slice/array/map and
// range-over-func arms already deferred the box (rangeOverFuncBinding_test.go pins the func arm).
//
// Controls: a variable that is never boxed keeps the bare `foreach (var x in …)` with no temp.
package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// convertHeapBoxedRangeVarFixture converts the one fixture the test reads and returns its emitted C#.
func convertHeapBoxedRangeVarFixture(t *testing.T) string {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/heapboxedrange\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

// INT, ADDRESS TAKEN - each pass stores a pointer to its own iteration's variable.
func intAddrs() []*int {
	var ps []*int

	for i := range 3 {
		ps = append(ps, &i)
	}

	return ps
}

// INT, READ ONLY - the control.
func sumTo(n int) int {
	sum := 0

	for i := range n {
		sum += i
	}

	return sum
}

// CHAN, ADDRESS TAKEN.
func chanAddrs(ch chan int) []*int {
	var ps []*int

	for v := range ch {
		ps = append(ps, &v)
	}

	return ps
}

// CHAN, READ ONLY - the control.
func drained(ch chan int) int {
	sum := 0

	for v := range ch {
		sum += v
	}

	return sum
}

// STRING, ADDRESS TAKEN - key and value each boxed.
func strAddrs(s string) ([]*int, []*rune) {
	var is []*int
	var rs []*rune

	for i, r := range s {
		is = append(is, &i)
		rs = append(rs, &r)
	}

	return is, rs
}

// STRING, READ ONLY - the control.
func runeCount(s string) int {
	n := 0

	for _, r := range s {
		if r != 0 {
			n++
		}
	}

	return n
}

// The two os shapes (Go 1.26 tests). Neither takes the variable's address; the escape analysis
// boxes each anyway (a value copied into an address-taken composite literal, and a struct read
// inside a goroutine's func literal). Whether they are boxed is not this test's subject; that a
// box, when emitted, is per-iteration and does not redeclare the foreach's name is.
type desc struct {
	k      int
	target *desc
}

type op struct {
	s   string
	off int64
}

func linkTargets() []*desc {
	var ds []*desc

	for linkKind := range 3 {
		ds = append(ds, &desc{target: &desc{k: linkKind}})
	}

	return ds
}

func writer(write chan op, read chan struct{}) {
	go func() {
		for o := range write {
			println(o.s, o.off)
			read <- struct{}{}
		}
	}()
}

func main() {
	ch := make(chan int, 2)
	ch <- 1
	ch <- 2
	close(ch)
	is, rs := strAddrs("ab")
	println(len(intAddrs()), sumTo(4), len(chanAddrs(ch)), drained(ch), len(is), len(rs), runeCount("ab"))
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

	return readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "heapboxedrange", "main.cs"))
}

// assertBoxedPerIteration fails unless body iterates a temp and opens the loop with a fresh box for
// name and the copy into it: no box before the foreach, and no foreach declaring the name itself.
func assertBoxedPerIteration(t *testing.T, fn, body, name, temp string) {
	t.Helper()

	box := "ref var " + name + " = ref heap("
	loop := "foreach ("
	boxAt, loopAt := strings.Index(body, box), strings.Index(body, loop)

	if boxAt < 0 || loopAt < 0 || boxAt < loopAt {
		t.Errorf("%s: expected the box for %s inside the loop body, after the foreach (one box per pass; CS0136 or a shared box otherwise):\n%s", fn, name, body)
	}

	if !strings.Contains(body, name+" = "+temp+";") {
		t.Errorf("%s: expected the body to copy %s into the box (`%s = %s;`):\n%s", fn, temp, name, temp, body)
	}

	if strings.Contains(body, "foreach (var "+name+" in ") {
		t.Errorf("%s: the foreach must iterate a temp, not declare %s itself:\n%s", fn, name, body)
	}
}

// TestRangeBoxesAHeapVariablePerIteration pins the deferred box on the string, chan and int arms
// and its scope.
func TestRangeBoxesAHeapVariablePerIteration(t *testing.T) {
	isolateConverterPathGlobals(t)

	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	mainCs := convertHeapBoxedRangeVarFixture(t)
	temp := func(prefix string) string { return prefix + TempVarMarker + "1" }

	// INT
	assertBoxedPerIteration(t, "intAddrs", emittedFuncBody(t, mainCs, "intAddrs"), "i", temp("i"))

	if sum := emittedFuncBody(t, mainCs, "sumTo"); !strings.Contains(sum, "foreach (var i in range(") || strings.Contains(sum, TempVarMarker) || strings.Contains(sum, "heap(") {
		t.Errorf("sumTo: an unboxed int range variable must keep the bare foreach:\n%s", sum)
	}

	// CHAN
	assertBoxedPerIteration(t, "chanAddrs", emittedFuncBody(t, mainCs, "chanAddrs"), "v", temp("i"))

	if drain := emittedFuncBody(t, mainCs, "drained"); !strings.Contains(drain, "foreach (var v in ") || strings.Contains(drain, TempVarMarker) || strings.Contains(drain, "heap(") {
		t.Errorf("drained: an unboxed chan range variable must keep the bare foreach:\n%s", drain)
	}

	// STRING - key and value both boxed, each from its own temp.
	strs := emittedFuncBody(t, mainCs, "strAddrs")
	assertBoxedPerIteration(t, "strAddrs", strs, "i", temp("i"))
	assertBoxedPerIteration(t, "strAddrs", strs, "r", temp("r"))

	// The os shapes: CS0136 when the box was declared before a foreach that names the variable.
	for _, c := range []struct{ fn, name string }{{"linkTargets", "linkKind"}, {"writer", "o"}} {
		if body := emittedFuncBody(t, mainCs, c.fn); strings.Contains(body, "heap(") {
			assertBoxedPerIteration(t, c.fn, body, c.name, temp("i"))
		}
	}

	if runes := emittedFuncBody(t, mainCs, "runeCount"); !strings.Contains(runes, "foreach (var (_, r) in ") || strings.Contains(runes, TempVarMarker) || strings.Contains(runes, "heap(") {
		t.Errorf("runeCount: an unboxed string range variable must keep the bare foreach:\n%s", runes)
	}
}
