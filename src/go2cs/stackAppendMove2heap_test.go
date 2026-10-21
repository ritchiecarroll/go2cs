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
	"io"
	"os"
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
func ifInit(n int) int {
	var p []*int
	for range n {
		p = append(p, nil)
	}
	if t := p; len(t) > 0 {
		return cap(t)
	}
	return 0
}

//go:noinline
func labeled(n int) int {
	var l []byte
	for i := range n {
		l = append(l, byte(i))
	}
	t := 0
outer:
	for _, b := range l {
		if b > 100 {
			break outer
		}
		t += int(b)
	}
	return t
}

//go:noinline
func twoSites(n int) []byte {
	var m []byte
	m = append(m, 1)
	for i := range n {
		m = append(m, byte(i))
	}
	return m
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

	// The converter names on stderr each transition Go logs that it does not emit.
	stderr := os.Stderr
	reader, writer, err := os.Pipe()

	if err != nil {
		t.Fatal(err)
	}

	os.Stderr = writer
	captured := make(chan string)

	go func() {
		data, _ := io.ReadAll(reader)
		captured <- string(data)
	}()

	convertErr := NewModuleConverter(options).ConvertModule(appDir)
	os.Stderr = stderr
	writer.Close()
	warnings := <-captured

	if convertErr != nil {
		t.Fatalf("ConvertModule: %v", convertErr)
	}

	emitted := strings.ReplaceAll(readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "m2h", "m2h.cs")), "\r\n", "\n")

	for _, want := range []string{
		// return: the buffer at the append, the move ahead of the return
		"    var rᴷ = new stackAppend<byte>(moves: true);\n",
		"r = append(rᴷ, r, (byte)i);",
		"    r = move2heap(rᴷ, r);\n    return r;\n",
		// an assignment through a pointer
		"    var qᴷ = new stackAppend<ж<byte>>(moves: true);\n",
		"    q = move2heap(qᴷ, q);\n",
		// the capacity observed: every append passes the buffer, declared capacityUsed
		"    var cᴷ = new stackAppend<byte>(moves: true, capacityUsed: true);\n",
		"c = append(cᴷ, c, (byte)i);",
		"c = append(cᴷ, c, (byte)1);",
		"    c = move2heap(cᴷ, c);\n    return (c, cap(c));\n",
		// an assignment to a field
		"    f = move2heap(fᴷ, f);\n",
		// a bare return of a named result
		"    nr = move2heap(nrᴷ, nr);\n    return nr;\n",
		// a range over a slice that never escapes
		"    var gᴷ = new stackAppend<byte>(moves: true);\n",
		"    g = move2heap(gᴷ, g);\n    foreach (var (_, b) in g) {\n",
		// x = x[i:j] observes the capacity
		"    var sᴷ = new stackAppend<byte>(moves: true, capacityUsed: true);\n",
		"    s = move2heap(sᴷ, s);\n    return s;\n",
		// an if's init: the move goes ahead of the if, as Go prepends it to the init
		"    var pᴷ = new stackAppend<ж<nint>>(moves: true);\n",
		"    p = move2heap(pᴷ, p);\n",
		// a labeled range: the move goes ahead of the labeled statement
		"    var lᴷ = new stackAppend<byte>(moves: true);\n",
		"    l = move2heap(lᴷ, l);\n",
	} {
		if strings.Count(emitted, want) != 1 {
			t.Errorf("want exactly one %q in:\n%s", want, emitted)
		}
	}

	if got := strings.Count(emitted, "move2heap("); got != 9 {
		t.Errorf("move2heap written %d times, want 9 (ret, store, capUsed, field, named, ranged, reslice, ifInit, labeled):\n%s", got, emitted)
	}

	// The moves come ahead of the statement Go's transition belongs to.
	for _, order := range [][2]string{{"p = move2heap(pᴷ, p);", "len(t) > 0"}, {"l = move2heap(lᴷ, l);", "foreach (var (_, b) in l)"}} {
		if move, use := strings.Index(emitted, order[0]), strings.Index(emitted, order[1]); move < 0 || use < 0 || move > use {
			t.Errorf("%q is not ahead of %q:\n%s", order[0], order[1], emitted)
		}
	}

	// A standard-strategy mover with two append sites is not buffered, and the converter says why.
	if !strings.Contains(warnings, "WARNING: Go moves m to the heap at m2h.go:") || !strings.Contains(warnings, "more than one site") {
		t.Errorf("want the not-emitted warning for twoSites's m, got:\n%s", warnings)
	}

	// Not committed by Go's pass: a variable appended once, and one declared by a conversion.
	for _, unwanted := range []string{"oᴷ", "dᴷ", "mᴷ"} {
		if strings.Contains(emitted, unwanted) {
			t.Errorf("unwanted %q in:\n%s", unwanted, emitted)
		}
	}
}
