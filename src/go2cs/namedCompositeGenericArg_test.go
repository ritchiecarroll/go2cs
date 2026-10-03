// namedCompositeGenericArg_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards a NAMED map, slice, channel or array passed to a generic function's UNNAMED composite parameter
// (the hashset test's named map handed to a `map[K]V` parameter, COORD 2026-10-03): Go infers through the
// named type's underlying type, but the named type emits as a wrapper struct that reaches `map<K, V>` only by
// a user-defined implicit conversion, and C# type inference never looks through one (CS0411). The call spells
// its type arguments out. An unnamed argument, a named argument at a bare type-parameter position and a named
// func argument infer in C# and keep their bare form.

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

func TestANamedCompositeArgumentToAGenericCompositeParameterSpellsItsTypeArguments(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/nci\n\ngo 1.23\n")

	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

type Names map[int]string
type Scores []float64
type Feed chan int
type Grid [2]int
type Pred func(int) bool

func keys[V any](m map[int]V) int       { return len(m) }
func total[E int | float64](s []E) int  { return len(s) }
func drain[T any](c chan T) int         { return cap(c) }
func first[T any](a [2]T) T             { return a[0] }
func count[T any](p func(T) bool, v T) bool { return p(v) }
func ident[T any](v T) T                { return v }

func main() {
	names := Names{1: "a"}
	scores := Scores{1.5}
	feed := make(Feed, 2)
	grid := Grid{4, 5}
	var even Pred = func(v int) bool { return v%2 == 0 }
	println(keys(names), total(scores), drain(feed), first(grid))
	println(keys(map[int]string{3: "c"}), len(ident(scores)), count(even, 2))
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

	mainCs := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "nci", "main.cs"))

	// A named map, slice, channel or array at an unnamed composite parameter: explicit type arguments.
	for _, want := range []string{"keys<@string>(names)", "total<float64>(scores)", "drain<nint>(feed)", "first<nint>(grid)"} {
		if !strings.Contains(mainCs, want) {
			t.Errorf("want %q: C# cannot infer through the named type's implicit conversion (CS0411):\n%s", want, mainCs)
		}
	}

	// CONTROLS: an unnamed argument, a named argument at a bare type-parameter position, and a named func
	// argument keep their bare form.
	for _, want := range []string{"keys(new map<", "ident(scores)", "count(even, 2)"} {
		if !strings.Contains(mainCs, want) {
			t.Errorf("control: want %q unchanged:\n%s", want, mainCs)
		}
	}
}
