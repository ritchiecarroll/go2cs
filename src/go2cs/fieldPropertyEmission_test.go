// fieldPropertyEmission_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards P1, parity seat #31 (fieldAccess.go, fieldPropertySelector): a field selected through a pointer to a named
// struct reads and writes as Go spells it, `p.f`, through the generated field property, and the file's
// `[GoFieldAccess(…)]` record names each struct, a generic one by its open definition. The controls keep today's
// `(~p).f` / `p.Value.f`: a promoted field, a field named like a member of ж<T>, a field that shadows a promoted method, a header-shaped
// struct, an unnamed struct, a struct of a package the converter never emits (testing's B), and a zero-size store
// target, whose accessor form names the struct.

package main

import (
	"path/filepath"
	"strings"
	"testing"
)

func convertFieldPropertyFixture(t *testing.T) string {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/fieldaccess\n\ngo 1.24\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import (
	"fmt"
	"testing"
	"unsafe"
)

type point struct{ X, Y int }

type node struct {
	x          int
	prev, next *node
}

type inner struct{ Z int }

type outer struct {
	inner
	W int
}

type pair[T any] struct {
	First T
	Count int
}

type boxed struct{ Value int }

type base struct{ n int }

func (b *base) Len() int { return b.n }

type sized struct {
	base
	Len int
}

type header struct {
	Data unsafe.Pointer
	Len  int
}

type zs struct {
	a int32
	z struct{}
	b int32
}

func mk() *point { return &point{1, 2} }

func positives(p *point, c *node, m map[string]*point) {
	q := p
	q.X = 10
	q.Y += q.X
	q.X++
	c.prev.prev.next.x = 20
	m["k"].Y = 3
	fmt.Println(q.X, c.prev.next.x, mk().Y, m["k"].Y)
}

func generic[T any](p *pair[T], v T) T {
	g := p
	g.First = v
	g.Count++
	return g.First
}

func controls(o *outer, v *boxed, s *sized, h *header, a *struct{ n int }, z *zs) {
	o2 := o
	o2.Z = 7
	o2.W = 8
	fmt.Println(o2.Z)
	w := v
	w.Value = 1
	t := s
	t.Len = 2
	hh := h
	hh.Len = 3
	aa := a
	aa.n = 4
	zz := z
	zz.z = struct{}{}
	zz.b = 5
	fmt.Println(w.Value, t.Len, hh.Len, aa.n, zz.b)
	run(func(b *testing.B) { fmt.Println(b.N) })
}

func run(f func(*testing.B)) {}

func main() {
	positives(&point{}, &node{}, map[string]*point{"k": {}})
	fmt.Println(generic(&pair[int]{}, 1))
	controls(&outer{}, &boxed{}, &sized{}, &header{}, &struct{ n int }{}, &zs{})
}
`)

	return readGenerated(t, filepath.Join(convertFieldAccessModule(t, root), "main.cs"))
}

// TestFieldPropertyReadsAsGoSpellsIt requires every P1 site to read and write `p.f`, the record to name each struct
// once, a generic one open, and every control to keep today's spelling.
func TestFieldPropertyReadsAsGoSpellsIt(t *testing.T) {
	isolateConverterPathGlobals(t)

	cs := convertFieldPropertyFixture(t)

	positives := funcBody(t, cs, "void positives(")

	for _, want := range []string{
		"q.X = 10;",
		"q.Y += q.X;",
		"q.X++;",
		"c.prev.prev.next.x = 20;",
		`m["k"].Y = 3;`,
		`fmt.Println(q.X, c.prev.next.x, mk().Y, m["k"].Y);`,
	} {
		if !strings.Contains(positives, want) {
			t.Errorf("a field through a pointer does not read as Go spells it (want %q):\n%s", want, positives)
		}
	}

	if strings.Contains(positives, ".Value.") || strings.Contains(positives, "(~") {
		t.Errorf("a field through a pointer still reads through the whole value:\n%s", positives)
	}

	if generic := funcBody(t, cs, "generic<T>("); !strings.Contains(generic, "g.First = v;") || !strings.Contains(generic, "g.Count++;") ||
		!strings.Contains(generic, "return g.First;") {
		t.Errorf("a generic struct's field does not read as Go spells it:\n%s", generic)
	}

	record := "[GoFieldAccess(typeof(point), typeof(node), typeof(pair<>), typeof(outer), typeof(zs))]"

	if !strings.Contains(cs, record+"\r\npartial class main_package {") && !strings.Contains(cs, record+"\npartial class main_package {") {
		t.Errorf("the file does not carry the record %s on its package class:\n%s", record, cs)
	}

	controls := funcBody(t, cs, "void controls(")

	for _, want := range []string{
		"o2.Value.Z = 7;", // a promoted field: the site would have to name the embed
		"fmt.Println((~o2).Z);",
		"o2.W = 8;",           // ... while the struct's own field is an ordinary P1 site
		"w.Value.Value = 1;",  // a field named like a member of ж<T>
		"t.Value.Len = 2;",    // shadows the Len method base promotes
		"hh.Value.Len = 3;",   // header-shaped: its properties are get/set, not refs
		"aa.Value.n = 4;",     // an unnamed struct, which no record can name
		"zs.Ꮡz(ref zz.Value)", // a zero-size store target's accessor names the struct
		"zz.b = 5;",           // ... while its sibling field is an ordinary P1 site
		"(~b).N",              // testing's B has no generated accessors
	} {
		if !strings.Contains(controls, want) {
			t.Errorf("a control does not keep today's spelling (want %q):\n%s", want, controls)
		}
	}
}
