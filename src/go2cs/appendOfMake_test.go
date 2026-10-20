// appendOfMake_test.go - Gbtc
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

const appendOfMakeFixture = `package aom

type point struct{ X, Y int }

// accepted (§B): a direct make of a slice, a length, no capacity, spread into append
func extend(s []int, n int) []int { return append(s, make([]int, n)...) }

func grow(s []int, n int) []int { return append(s[:cap(s)], make([]int, n)...)[:len(s)] }

func growGeneric[S ~[]E, E any](s S, n int) S { return append(s, make(S, n)...) }

func structs(s []point, n int) []point { return append(s, make([]point, n)...) }

// refused
func withCap(s []int, n int) []int { return append(s, make([]int, n, n+1)...) }

func named(s []int, n int) []int {
	m := make([]int, n)
	return append(s, m...)
}

func arrays(s [][2]int, n int) [][2]int { return append(s, make([][2]int, n)...) }

func literal(s []int) []int { return append(s, []int{0, 0}...) }
`

// TestAppendOfMakeEmitsTheLengthOnlyOperandOnlyForGosExtendSliceShape controls appendOfMakeOperand BOTH
// ways (REC-C, docs/phase4/DESIGN-slice-idiom-allocations.md §B): Go's compiler grows x in place for
// `append(x, make([]T, n)...)` and never allocates the make, so exactly that shape emits `makeꓸꓸꓸ<T>(n)`;
// every other spread keeps the ordinary make-then-append emission.
//
// RED at fa18863b94 on the four accepted cases: no operand rendered `makeꓸꓸꓸ`.
func TestAppendOfMakeEmitsTheLengthOnlyOperandOnlyForGosExtendSliceShape(t *testing.T) {
	isolateConverterPathGlobals(t)
	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/aom\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "aom.go"), appendOfMakeFixture)

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

	emitted := readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "aom", "aom.cs"))

	cases := []struct {
		declaration string
		want        string // the operand the append must render
		refused     bool
	}{
		{"internal static slice<nint> extend(", "makeꓸꓸꓸ<nint>(n)", false},
		{"internal static slice<nint> grow(", "makeꓸꓸꓸ<nint>(n)", false},
		{" growGeneric<S, E>(", "makeꓸꓸꓸ<E>(n)", false},
		{"internal static slice<point> structs(", "makeꓸꓸꓸ<point>(n)", false},
		{"internal static slice<nint> withCap(", "", true},
		{"internal static slice<nint> named(", "", true},
		{"internal static slice<array<nint>> arrays(", "", true},
		{"internal static slice<nint> literal(", "", true},
	}

	for _, c := range cases {
		body := liftFunctionBody(t, emitted, c.declaration)

		if c.refused {
			if strings.Contains(body, "makeꓸꓸꓸ") {
				t.Errorf("%s: a refused shape rendered the length-only operand:\n%s", c.declaration, body)
			}

			continue
		}

		if !strings.Contains(body, c.want) {
			t.Errorf("%s: want the operand %q:\n%s", c.declaration, c.want, body)
		}
	}
}
