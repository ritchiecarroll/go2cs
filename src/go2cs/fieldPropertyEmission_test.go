// fieldPropertyEmission_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards P1, parity seat #31 (fieldAccess.go, fieldPropertySelector): a field selected through a pointer to a named
// struct reads and writes as Go spells it, `p.f`, through the generated field property, and the package's
// package_info.cs carries one `[assembly: GoFieldAccess(typeof(T))]` record per struct, a generic one by its open
// definition, while the converted file carries none. The controls keep today's `(~p).f` / `p.Value.f`: a promoted
// field, a field named like a member of ж<T>, a field that shadows a promoted method, a header-shaped struct, an
// unnamed struct, a struct of a package the converter never emits (testing's B), a struct the package defines over
// another named struct (its fields are generated), and a zero-size store target, whose accessor form names the struct.

package main

import (
	"path/filepath"
	"regexp"
	"strings"
	"testing"
)

// convertFieldPropertyFixture converts the fixture and answers its main.cs and its package_info.cs.
func convertFieldPropertyFixture(t *testing.T) (string, string) {
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

// A struct defined over another named struct: an inherited wrapper whose fields are generated.
type wrapped point

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
	ww := new(wrapped)
	ww.X = 6
	fmt.Println(w.Value, t.Len, hh.Len, aa.n, zz.b, ww.X)
	run(func(b *testing.B) { fmt.Println(b.N) })
}

func run(f func(*testing.B)) {}

func main() {
	positives(&point{}, &node{}, map[string]*point{"k": {}})
	fmt.Println(generic(&pair[int]{}, 1))
	controls(&outer{}, &boxed{}, &sized{}, &header{}, &struct{ n int }{}, &zs{})
}
`)

	out := convertFieldAccessModule(t, root)

	return readGenerated(t, filepath.Join(out, "main.cs")), readGenerated(t, filepath.Join(out, PackageInfoFileName))
}

// fieldAccessRecordsOf answers the struct types a package_info.cs records, by simple name, in file order.
func fieldAccessRecordsOf(info string) []string {
	var names []string

	for _, m := range regexp.MustCompile(`(?m)^\[assembly: (?:global::)?(?:go\.)?GoFieldAccess\(typeof\((?:[\w.@]*\.)?([\w@]+(?:<,*>)?)\)\)\]\r?$`).FindAllStringSubmatch(info, -1) {
		names = append(names, m[1])
	}

	return names
}

// TestFieldPropertyReadsAsGoSpellsIt requires every P1 site to read and write `p.f`, package_info.cs to record each
// struct once, sorted, a generic one open, the converted file to carry no record, and every control to keep today's
// spelling.
func TestFieldPropertyReadsAsGoSpellsIt(t *testing.T) {
	isolateConverterPathGlobals(t)

	cs, info := convertFieldPropertyFixture(t)

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

	if got, want := strings.Join(fieldAccessRecordsOf(info), " "), "node outer pair<> point zs"; got != want {
		t.Errorf("package_info.cs records %q, want %q:\n%s", got, want, info)
	}

	if strings.Contains(cs, "GoFieldAccess") {
		t.Errorf("the converted file carries a record; package_info.cs holds them:\n%s", cs)
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
		"ww.Value.X = 6;",     // a wrapper over another named struct: its fields are generated
		"(~b).N",              // testing's B has no generated accessors
	} {
		if !strings.Contains(controls, want) {
			t.Errorf("a control does not keep today's spelling (want %q):\n%s", want, controls)
		}
	}
}
