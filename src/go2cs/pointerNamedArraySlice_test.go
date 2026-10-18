// pointerNamedArraySlice_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards convSliceExpr's slice of a POINTER to a NAMED array. It is sliced through the box's ref accessor,
// `p.Value[..]`, never the dereference `(~p).Value[..]`. golib's `~` returns the wrapper BY VALUE, and a named
// array's wrapper allocates its backing lazily, so `(~p).Value[..][i] = x` materialized the backing in a COPY and
// the write was lost: Go 1.26 runtime's mpallocbits_test.go `want[:][2] = 0x3e` read back 0, once the chain's
// missing Range indexer (go2cs-gen's array-view template) stopped hiding it behind CS1503. The control is a pointer
// to an ANONYMOUS array, which has no lazy backing and keeps its dereference.

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

func convertPointerNamedArraySliceFixture(t *testing.T) string {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/ptrarrayslice\n\ngo 1.24\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import "fmt"

type base [4]uint64
type two base

// POSITIVE -- a pointer to a named array, direct and two definitions deep, full and bounded.
func named() {
	a := new(base)
	a[:][1] = 5
	c := new(two)
	c[:][3] = 7
	n := 1
	s := c[n:]
	w := c[:2:3]
	fmt.Println(a[1], c[3], len(s), cap(w))
}

// CONTROL -- a pointer to an ANONYMOUS array keeps its dereference.
func anonymous() {
	p := new([4]uint64)
	p[:][2] = 9
	fmt.Println(p[2])
}

func main() {
	named()
	anonymous()
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

	return readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "ptrarrayslice", "main.cs"))
}

// TestPointerNamedArraySliceReadsThroughTheBox requires every slice of a pointer to a named array to go through
// `p.Value`, and the anonymous control to go through golib's pointer-to-array slice methods, `p.slice(…)`.
func TestPointerNamedArraySliceReadsThroughTheBox(t *testing.T) {
	isolateConverterPathGlobals(t)

	cs := convertPointerNamedArraySliceFixture(t)
	named := funcBody(t, cs, "void named(")

	for _, want := range []string{"a.Value[..][1] = 5", "c.Value[..][3] = 7", "c.Value.slice(n)", "c.Value.slice(0, 2, 3)"} {
		if !strings.Contains(named, want) {
			t.Errorf("a slice of a pointer to a named array does not read through the box (want %q):\n%s", want, named)
		}
	}

	if strings.Contains(named, "(~a)") || strings.Contains(named, "(~c)") {
		t.Errorf("a slice of a pointer to a named array still dereferences the box (a copy whose lazy backing loses the write):\n%s", named)
	}

	// CONTROL: an anonymous array has no lazy backing and is sliced through the box's own slice methods, which
	// ask for a native window before they dereference (anonymousArrayBoxSlice).
	if anonymous := funcBody(t, cs, "void anonymous("); !strings.Contains(anonymous, "p.slice()[2] = 9") {
		t.Errorf("the anonymous-array control is not sliced through the box's slice methods:\n%s", anonymous)
	}
}
