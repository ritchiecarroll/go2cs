// stackAppendMove2heap_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards Go 1.26's move2heap in the stack-append harvest (stackAppendHints.go): a variable whose one escape is a single
// statement outside its loops takes the stack buffer at its appends, and `x = move2heap(xᴷ, x);` is emitted ahead of
// that statement, read from the transition -m logs under -d=variablemakehash. The shapes are the ones Go's slice pass
// (cmd/compile/internal/slice) commits: a return, an assignment through a pointer or to a field, a bare return of a
// named result, a range over the variable (which moves a slice that never escapes, too), and a capacity observed by
// cap(x) or x = x[i:j], which declares the buffer capacityUsed and passes it at every append. Two it does not commit
// stay unbuffered: a variable appended once, and one declared by a conversion the pass does not understand.

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

const stackAppendMove2heapFixture = `package m2h

type holder struct{ s []byte }

//go:noinline
func ret(n int) []byte {
	var r []byte
	for i := range n {
		r = append(r, byte(i))
	}
	return r
}

//go:noinline
func store(n int, p *[]*byte) {
	var q []*byte
	for range n {
		q = append(q, nil)
	}
	*p = q
}

//go:noinline
func capUsed(n int) ([]byte, int) {
	var c []byte
	for i := range n {
		c = append(c, byte(i))
	}
	c = append(c, 1)
	return c, cap(c)
}

//go:noinline
func field(n int, h *holder) {
	var f []byte
	for i := range n {
		f = append(f, byte(i))
	}
	h.s = f
}

//go:noinline
func named(n int) (nr []byte) {
	for i := range n {
		nr = append(nr, byte(i))
	}
	return
}

//go:noinline
func ranged(n int) int {
	var g []byte
	for i := range n {
		g = append(g, byte(i))
	}
	t := 0
	for _, b := range g {
		t += int(b)
	}
	return t
}

//go:noinline
func reslice(n int) []byte {
	var s []byte
	for i := range n {
		s = append(s, byte(i))
	}
	s = s[:len(s)]
	return s
}

//go:noinline
func once(n int) []byte {
	var o []byte
	o = append(o, byte(n))
	return o
}

//go:noinline
func define(n int) []byte {
	d := []byte(nil)
	for i := range n {
		d = append(d, byte(i))
	}
	return d
}
`

func TestStackAppendMove2heapEmission(t *testing.T) {
	isolateConverterPathGlobals(t)

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/m2h\n\ngo 1.26\n")
	writeModuleFile(t, filepath.Join(appDir, "m2h.go"), stackAppendMove2heapFixture)

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

	emitted := strings.ReplaceAll(readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "m2h", "m2h.cs")), "\r\n", "\n")

	for _, want := range []string{
		// return: the buffer at the append, the move ahead of the return
		"    var rᴷ = new stackAppend<byte>();\n",
		"r = append(rᴷ, r, (byte)i);",
		"    r = move2heap(rᴷ, r);\n    return r;\n",
		// an assignment through a pointer
		"    var qᴷ = new stackAppend<ж<byte>>();\n",
		"    q = move2heap(qᴷ, q);\n",
		// the capacity observed: every append passes the buffer, declared capacityUsed
		"    var cᴷ = new stackAppend<byte>(capacityUsed: true);\n",
		"c = append(cᴷ, c, (byte)i);",
		"c = append(cᴷ, c, (byte)1);",
		"    c = move2heap(cᴷ, c);\n    return (c, cap(c));\n",
		// an assignment to a field
		"    f = move2heap(fᴷ, f);\n",
		// a bare return of a named result
		"    nr = move2heap(nrᴷ, nr);\n    return nr;\n",
		// a range over a slice that never escapes
		"    var gᴷ = new stackAppend<byte>();\n",
		"    g = move2heap(gᴷ, g);\n    foreach (var (_, b) in g) {\n",
		// x = x[i:j] observes the capacity
		"    var sᴷ = new stackAppend<byte>(capacityUsed: true);\n",
		"    s = move2heap(sᴷ, s);\n    return s;\n",
	} {
		if strings.Count(emitted, want) != 1 {
			t.Errorf("want exactly one %q in:\n%s", want, emitted)
		}
	}

	if got := strings.Count(emitted, "move2heap("); got != 7 {
		t.Errorf("move2heap written %d times, want 7 (ret, store, capUsed, field, named, ranged, reslice):\n%s", got, emitted)
	}

	// Not committed by Go's pass: a variable appended once, and one declared by a conversion.
	for _, unwanted := range []string{"oᴷ", "dᴷ"} {
		if strings.Contains(emitted, unwanted) {
			t.Errorf("unwanted %q in:\n%s", unwanted, emitted)
		}
	}
}
