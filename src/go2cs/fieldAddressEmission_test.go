// fieldAddressEmission_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards P2, parity seat #32 (fieldAccess.go, fieldAddressStep): a field's address, Go's `&p.f`, reads `p.Ꮡf`, the
// address property go2cs-gen emits beside the field property, whatever the base: a pointer local, a plain pointer
// parameter, a receiver's box, a value field reached through a pointer, a heap-boxed value, a generic struct. Each
// struct it serves is recorded in package_info.cs. The controls keep the accessor's view `.of(T.Ꮡf)`: P1's own
// controls (a promoted field, a field named like a member of ж<T>, one that shadows a promoted method, a header-shaped
// struct, an unnamed struct, a wrapper over another named struct), a field whose ADDRESS property would be named like
// a member of ж<T> (`Ꮡelem`), a named pointer type (its wrapper declares `of`, not the property), and the field
// address an element address fuses with, `&p.arr[i]` as `p.at(T.Ꮡarr, i)`.

package main

import (
	"path/filepath"
	"strings"
	"testing"
)

// convertFieldAddressFixture converts the fixture and answers its main.cs and its package_info.cs.
func convertFieldAddressFixture(t *testing.T) (string, string) {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/fieldaccess\n\ngo 1.24\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import (
	"fmt"
	"unsafe"
)

type point struct{ X, Y int }

type holder struct {
	in  point
	arr [4]int
}

type counter struct{ n int }

func (c *counter) addr() *int { return &c.n }

type pair[T any] struct {
	First T
	Count int
}

type inner struct{ Z int }

type outer struct {
	inner
	W int
}

type boxed struct{ Value int }

type link struct{ elem, next int }

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

type wrapped point

type pointPtr *point

var sink []*int

func keep(p *int) { sink = append(sink, p) }

func positives(p *point, h *holder, c *counter, g *pair[string]) {
	q := p
	keep(&q.X)
	keep(&p.Y)
	keep(&h.in.X)
	keep(c.addr())
	var s point
	ps := &s.Y
	keep(ps)
	gs := &g.Count
	keep(gs)
	fmt.Println(q.X, p.Y, h.in.X, s.Y, g.Count)
}

func controls(o *outer, v *boxed, l *link, s *sized, hd *header, a *struct{ n int }, w *wrapped, pp pointPtr, h *holder) {
	keep(&o.Z)
	keep(&o.W)
	keep(&v.Value)
	keep(&l.elem)
	keep(&l.next)
	keep(&s.Len)
	keep(&hd.Len)
	keep(&a.n)
	keep(&w.X)
	keep(&pp.Y)
	keep(&h.arr[1])
	fmt.Println(o.Z, v.Value, l.elem, s.Len, hd.Len, a.n, w.X, pp.Y, h.arr[1])
}

func main() {
	positives(&point{}, &holder{}, &counter{}, &pair[string]{})
	controls(&outer{}, &boxed{}, &link{}, &sized{}, &header{}, &struct{ n int }{}, new(wrapped), pointPtr(&point{}), &holder{})
}
`)

	out := convertFieldAccessModule(t, root)

	return readGenerated(t, filepath.Join(out, "main.cs")), readGenerated(t, filepath.Join(out, PackageInfoFileName))
}

// TestFieldAddressReadsAsAProperty requires every P2 site to read `base.Ꮡf`, package_info.cs to record each struct
// a property serves, and every control to keep the accessor's view.
func TestFieldAddressReadsAsAProperty(t *testing.T) {
	isolateConverterPathGlobals(t)

	cs, info := convertFieldAddressFixture(t)

	positives := funcBody(t, cs, "void positives(")

	for _, want := range []string{
		"keep(q.ᏑX);",         // a pointer local is the box
		"keep(Ꮡp.ᏑY);",        // a deref-aliased pointer parameter's box
		"keep(Ꮡh.Ꮡin.ᏑX);",    // a value field reached through a pointer: each level its property
		"var ps = Ꮡs.ᏑY;",     // a heap-boxed value
		"var gs = Ꮡg.ᏑCount;", // a generic struct
	} {
		if !strings.Contains(positives, want) {
			t.Errorf("a field's address does not read as its property (want %q):\n%s", want, positives)
		}
	}

	if strings.Contains(positives, ".of(") {
		t.Errorf("a field's address still reads through the accessor's view:\n%s", positives)
	}

	if addr := funcBody(t, cs, "ж<nint> addr("); !strings.Contains(addr, "return Ꮡc.Ꮡn;") {
		t.Errorf("a receiver's field address does not read as its property on the receiver's box:\n%s", addr)
	}

	if got, want := strings.Join(fieldAccessRecordsOf(info), " "), "counter holder link outer pair<> point"; got != want {
		t.Errorf("package_info.cs records %q, want %q:\n%s", got, want, info)
	}

	controls := funcBody(t, cs, "void controls(")

	for _, want := range []string{
		"keep(Ꮡo.of(outer.ᏑZ));",       // a promoted field
		"keep(Ꮡo.ᏑW);",                 // ... while the struct's own field is an ordinary P2 site
		"keep(Ꮡv.of(boxed.ᏑValue));",   // a field named like a member of ж<T>
		"keep(Ꮡl.of(link.Ꮡelem));",     // an address property named like ж<T>'s own Ꮡelem
		"keep(Ꮡl.Ꮡnext);",              // ... while its sibling field is an ordinary P2 site
		"keep(Ꮡs.of(sized.ᏑLen));",     // shadows the Len method base promotes
		"keep(Ꮡhd.of(header.ᏑLen));",   // header-shaped
		"keep(Ꮡa.of(controls_a.Ꮡn));",  // an unnamed struct, which no record can name
		"keep(Ꮡw.of(wrapped.ᏑX));",     // a wrapper over another named struct: its fields are generated
		"keep(pp.of(point.ᏑY));",       // a named pointer type's wrapper declares `of`, not the property
		"keep(Ꮡh.at(holder.Ꮡarr, 1));", // the field address an element address fuses with
	} {
		if !strings.Contains(controls, want) {
			t.Errorf("a control does not keep the accessor's view (want %q):\n%s", want, controls)
		}
	}
}
