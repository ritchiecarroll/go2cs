// rangeIndexOnly_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the INDEX-ONLY range arm: `for i := range x`, `for i, _ := range x` and `for range x` over a slice, an
// array or a pointer to an array read no element in Go, so they emit `foreach (var i in range(x))`, golib's index
// enumerator, and not the (index, value) tuple foreach, which reads every element into a stack temp even when the
// value is discarded (crypto/internal/fips140cache's TestCache waited forever on an element that temp kept alive).
//
// The scalar-position escapes the integer arm takes apply unchanged: a blank key iterates a marked temp (a scalar
// foreach `_` declares a variable), an assigned key copies a temp into the existing variable, a written key iterates
// a temp and declares the variable from it, and an address-taken key gets a fresh box per iteration.
//
// Controls: a range with a value variable, a range over a string (whose indices are rune starts) and a range over a
// map keep the tuple foreach.
package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// convertIndexOnlyRangeFixture converts the one fixture the test reads and returns its emitted C#.
func convertIndexOnlyRangeFixture(t *testing.T) string {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/indexonly\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

type ints []int

// SLICE, KEY ONLY.
func keyOnly(s []int) int {
	n := 0
	for i := range s {
		n += i
	}
	return n
}

// SLICE, BLANK VALUE - Go reads it as key only.
func blankValue(s []int) int {
	n := 0
	for i, _ := range s {
		n += i
	}
	return n
}

// SLICE, NO VARIABLES.
func noVars(s []int) int {
	n := 0
	for range s {
		n++
	}
	return n
}

// ARRAY, and a POINTER TO AN ARRAY local.
func overArray() int {
	var a [3]int
	p := &a
	n := 0
	for i := range a {
		n += i
	}
	for i := range p {
		n += i
	}
	return n
}

func overNilPointer() int {
	var nilp *[4]int
	n := 0
	for i := range nilp {
		n += i
	}
	return n
}

func overPointerParam(p *[2]int) int {
	n := 0
	for i := range p {
		n += i
	}
	return n
}

// ASSIGNED KEY.
func assigned(s []int) int {
	var i int
	for i = range s {
	}
	return i
}

// WRITTEN KEY.
func written(s []int) int {
	n := 0
	for i := range s {
		i++
		n += i
	}
	return n
}

// ADDRESS-TAKEN KEY.
func addressed(s []int) []*int {
	var ps []*int
	for i := range s {
		ps = append(ps, &i)
	}
	return ps
}

// NAMED SLICE TYPE, and a TYPE PARAMETER.
func named(s ints) int {
	n := 0
	for i := range s {
		n += i
	}
	return n
}

func generic[S ~[]E, E any](s S) int {
	n := 0
	for i := range s {
		n += i
	}
	return n
}

// CONTROLS - a value variable, a string and a map keep the tuple foreach.
func withValue(s []int) int {
	n := 0
	for i, v := range s {
		n += i + v
	}
	return n
}

func overString(str string) int {
	n := 0
	for i := range str {
		n += i
	}
	return n
}

func overMap(m map[int]int) int {
	n := 0
	for k := range m {
		n += k
	}
	return n
}

func main() {
	s := []int{1, 2, 3}
	println(keyOnly(s), blankValue(s), noVars(s), overArray(), assigned(s), written(s), len(addressed(s)))
	println(named(ints(s)), generic(s), withValue(s), overString("ab"), overMap(map[int]int{1: 1}))
	println(overNilPointer(), overPointerParam(&[2]int{}))
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

	return readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "indexonly", "main.cs"))
}

// TestIndexOnlyRangeEmitsTheIndexEnumerator pins the arm, its scalar escapes and its scope.
func TestIndexOnlyRangeEmitsTheIndexEnumerator(t *testing.T) {
	isolateConverterPathGlobals(t)

	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	mainCs := convertIndexOnlyRangeFixture(t)
	marker := TempVarMarker

	expect := func(fn string, wants ...string) {
		t.Helper()
		body := emittedFuncBody(t, mainCs, fn)

		for _, want := range wants {
			if !strings.Contains(body, want) {
				t.Errorf("%s: expected %q:\n%s", fn, want, body)
			}
		}
	}

	refuse := func(fn string, unwanted ...string) {
		t.Helper()
		body := emittedFuncBody(t, mainCs, fn)

		for _, bad := range unwanted {
			if strings.Contains(body, bad) {
				t.Errorf("%s: unexpected %q:\n%s", fn, bad, body)
			}
		}
	}

	expect("keyOnly", "foreach (var i in range(s))")
	refuse("keyOnly", "(i, _)")
	expect("blankValue", "foreach (var i in range(s))")
	refuse("blankValue", "(i, _)")
	expect("noVars", "foreach (var _"+marker+"1 in range(s))")
	// A *[N]T ranges its type's length beside the pointer, so a nil one ranges N times, as Go's does; a pointer
	// PARAMETER is already the dereferenced value local and ranges as an array.
	expect("overArray", "foreach (var i in range(a))", "foreach (var i in range(p, 3))")
	refuse("overArray", ".Value", "(i, _)")
	expect("overNilPointer", "foreach (var i in range(nilp, 4))")
	refuse("overNilPointer", ".Value", "(i, _)")
	expect("overPointerParam", "foreach (var i in range(p))")
	expect("assigned", "foreach (var i"+marker+"1 in range(s))", "i = i"+marker+"1;")
	expect("written", "foreach (var i"+marker+"1 in range(s))", "var i = i"+marker+"1;")
	expect("addressed", "in range(s))", "heap(")
	refuse("addressed", "(i, _)")
	expect("named", "foreach (var i in range(s))")
	expect("generic", "foreach (var i in range(s))")

	expect("withValue", "foreach (var (i, v) in s)")
	refuse("withValue", "range(s)")
	expect("overString", "foreach (var (i, _) in str)")
	expect("overMap", "foreach (var (k, _) in m)")
}
