// recvRefLowering_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards ж-box A2's reversion (§3.3) on a method's VALUE RECEIVER: a receiver whose every address-connected use feeds a
// Phase-A ref-lowered position needs no entry box, exactly as a value parameter of the same shape does. The receiver is
// the method's own by-value copy (Go's receiver-copy semantic), so `ref b` at the lowered call writes into it and later
// reads see the write. censusFuncLocals never collected funcDecl.Recv, so the receiver kept
// `ref var b = ref heap(bʗp, out var Ꮡb);` where the parameter form already reads `clip(ref r, …)`.
//
// Controls: a receiver whose address escapes as a pointer value keeps its box, and an ARRAY receiver keeps its by-value
// `.Clone()` entry form (array<T> shares its backing), the same form an array parameter takes.
package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// convertRecvRefLoweringFixture converts the one fixture the test reads and returns its emitted C#.
func convertRecvRefLoweringFixture(t *testing.T) string {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/recvlowering\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

type Rect struct {
	Min, Max int
}

type Box struct {
	R   Rect
	Tag int
}

type Trio [3]int

func clip(r *Rect, lo, hi int) {
	if r.Min < lo {
		r.Min = lo
	}
	if r.Max > hi {
		r.Max = hi
	}
}

func bump(p *int) {
	*p += 10
}

func bumpBox(b *Box) {
	b.Tag += 10
}

// REVERTS - the receiver's address feeds only lowered calls.
func (b Box) BumpedRecv() int {
	bumpBox(&b)
	return b.Tag
}

func (b Box) ClippedRecv() (int, int) {
	clip(&b.R, 5, 5)
	return b.R.Min, b.R.Max
}

func (a Trio) BumpedElemRecv() (int, int, int) {
	bump(&a[1])
	return a[0], a[1], a[2]
}

// KEEPS - the receiver's address escapes as a pointer value.
func (b Box) ReadOnlyRecv() (int, int) {
	p := &b
	return p.R.Min, p.Tag
}

func (b Box) Escaping() *Box {
	return &b
}

func main() {
	b := Box{R: Rect{0, 16}}
	println(b.BumpedRecv(), b.Tag)
	println(b.ClippedRecv())
	println(Trio{1, 2, 3}.BumpedElemRecv())
	println(b.ReadOnlyRecv())
	println(b.Escaping().Tag)
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

	return readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "recvlowering", "main.cs"))
}

// TestValueReceiverRevertsToAPlainParameter pins the receiver reversion and its controls at the emission.
func TestValueReceiverRevertsToAPlainParameter(t *testing.T) {
	isolateConverterPathGlobals(t)

	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	mainCs := convertRecvRefLoweringFixture(t)

	body := func(fn string) string {
		t.Helper()
		return emittedFuncBody(t, mainCs, fn)
	}

	expect := func(fn string, wants ...string) {
		t.Helper()
		got := body(fn)

		for _, want := range wants {
			if !strings.Contains(got, want) {
				t.Errorf("%s: expected %q:\n%s", fn, want, got)
			}
		}
	}

	refuse := func(fn string, unwanted ...string) {
		t.Helper()
		got := body(fn)

		for _, bad := range unwanted {
			if strings.Contains(got, bad) {
				t.Errorf("%s: unexpected %q:\n%s", fn, bad, got)
			}
		}
	}

	expect("BumpedRecv", "BumpedRecv(this Box b)", "bumpBox(ref b);")
	refuse("BumpedRecv", "ʗp", "heap(")
	expect("ClippedRecv", "ClippedRecv(this Box b)", "clip(ref b.R, 5, 5);")
	refuse("ClippedRecv", "ʗp", "heap(")
	expect("BumpedElemRecv", "BumpedElemRecv(this Trio a)", "a = a.Clone();", "bump(ref a[1]);")
	refuse("BumpedElemRecv", "ʗp", "heap(")

	expect("ReadOnlyRecv", "ReadOnlyRecv(this Box bʗp)", "heap(bʗp, out var Ꮡb)")
	expect("Escaping", "Escaping(this Box bʗp)", "heap(bʗp, out var Ꮡb)", "return Ꮡb;")
}
