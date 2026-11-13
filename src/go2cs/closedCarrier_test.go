// closedCarrier_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/ast"
	"go/importer"
	"go/parser"
	"go/token"
	"go/types"
	"testing"
)

// TestClosedCarrierRoute pins closedCarrierRouteOf, the converter's choice of a closed adapter class for one
// instantiation of a generic type converted to a non-generic interface. The closed route is taken only when a method
// the interface requires names a type parameter of the generic definition; a promoted method among the required
// ones refuses the pair; every other row answers closedCarrierNone, which leaves the emission as it was.
func TestClosedCarrierRoute(t *testing.T) {
	const src = `package lib

type point struct{ x, y int }

type valuer interface{ val() point }
type setter interface{ set(p point) }
type sizer interface{ size() int }
type both interface {
	val() point
	size() int
}
type source[T any] interface{ val() T }

type box[T any] struct{ v T }

func (b box[T]) val() T   { return b.v }
func (b box[T]) size() int { return 1 }

type cell[T any] struct{ v T }

func (c *cell[T]) val() T  { return c.v }
func (c *cell[T]) set(v T) { c.v = v }

type pair[T, U any] struct {
	a T
	b U
}

func (p pair[T, U]) val() T { return p.a }

// outer promotes val, which names outer's own type parameter through the embedded box[T].
type outer[T any] struct {
	box[T]
	n int
}

// fixed embeds a closed box: its promoted val returns point and names no type parameter.
type fixed[T any] struct {
	box[point]
	t T
}

// mixed declares val itself and promotes size from a non-generic field.
type counter struct{}

func (counter) size() int { return 2 }

type mixed[T any] struct {
	counter
	v T
}

func (m mixed[T]) val() T { return m.v }

// plain is not generic, and embeds a closed box.
type plain struct{ box[point] }

func generic[T any](b box[T]) {}
`

	fset := token.NewFileSet()
	file, err := parser.ParseFile(fset, "lib.go", src, 0)

	if err != nil {
		t.Fatal(err)
	}

	pkg, err := (&types.Config{Importer: importer.Default()}).Check("lib", fset, []*ast.File{file}, nil)

	if err != nil {
		t.Fatal(err)
	}

	other := types.NewPackage("other", "other")
	lookup := func(name string) types.Type { return pkg.Scope().Lookup(name).Type() }

	instantiate := func(name string, args ...types.Type) types.Type {
		inst, err := types.Instantiate(nil, lookup(name), args, true)

		if err != nil {
			t.Fatalf("instantiate %s: %v", name, err)
		}

		return inst
	}

	point, integer, str := lookup("point"), types.Typ[types.Int], types.Typ[types.String]

	// box[T] over the type parameter of the generic function: an open target.
	genericParam := lookup("generic").(*types.Signature).Params().At(0).Type()

	cases := []struct {
		name   string
		target types.Type
		iface  types.Type
		pkg    *types.Package
		want   closedCarrierRoute
	}{
		{"box[point] as valuer: a result names T", instantiate("box", point), lookup("valuer"), pkg, closedCarrierClosed},
		{"*box[point] as valuer: the pointer form", types.NewPointer(instantiate("box", point)), lookup("valuer"), pkg, closedCarrierClosed},
		{"*cell[point] as valuer: a pointer receiver", types.NewPointer(instantiate("cell", point)), lookup("valuer"), pkg, closedCarrierClosed},
		{"*cell[point] as setter: a parameter names T", types.NewPointer(instantiate("cell", point)), lookup("setter"), pkg, closedCarrierClosed},
		{"box[point] as both: one method names T, one does not", instantiate("box", point), lookup("both"), pkg, closedCarrierClosed},
		{"pair[point, int] as valuer: two type parameters", instantiate("pair", point, integer), lookup("valuer"), pkg, closedCarrierClosed},
		{"mixed[point] as both: a promoted method beside a named one", instantiate("mixed", point), lookup("both"), pkg, closedCarrierRefused},
		{"outer[point] as valuer: promoted and naming T", instantiate("outer", point), lookup("valuer"), pkg, closedCarrierRefused},
		{"box[point] as sizer (control): no method names T", instantiate("box", point), lookup("sizer"), pkg, closedCarrierNone},
		{"box[string] as sizer (control): any instantiation", instantiate("box", str), lookup("sizer"), pkg, closedCarrierNone},
		{"box[point] as source[point]: a generic interface", instantiate("box", point), instantiate("source", point), pkg, closedCarrierNone},
		{"fixed[int] as valuer: promoted from a closed field", instantiate("fixed", integer), lookup("valuer"), pkg, closedCarrierNone},
		{"plain as valuer: not generic", lookup("plain"), lookup("valuer"), pkg, closedCarrierNone},
		{"box[T] as valuer: an open instantiation", genericParam, lookup("valuer"), pkg, closedCarrierNone},
		{"box[point] as valuer from another package: not this package's generic", instantiate("box", point), lookup("valuer"), other, closedCarrierNone},
		{"box[point] as an interface literal: not a named interface", instantiate("box", point), lookup("valuer").Underlying(), pkg, closedCarrierNone},
		{"point as valuer: no such method", point, lookup("valuer"), pkg, closedCarrierNone},
	}

	for _, c := range cases {
		if got := closedCarrierRouteOf(c.target, c.iface, c.pkg); got != c.want {
			t.Errorf("%s: closedCarrierRouteOf = %v, want %v", c.name, got, c.want)
		}
	}
}

// TestClosedCarrierRecordsAreNotTrustedByAnImporter pins the other half of the marker: an importing package reads a
// declaring package's records to learn that a type implements an interface in C#, and a closed pair's type does not.
// The marked records must reach neither set, so the importer records and generates a carrier of its own. The
// unmarked forms are the positive controls.
func TestClosedCarrierRecordsAreNotTrustedByAnImporter(t *testing.T) {
	marked := []string{
		"[assembly: GoImplement<Box<Point>, Valuer>(Closed = true)]",
		"[assembly: GoImplement<Box<Point>, Valuer>(Pointer = true, Closed = true)]",
	}

	if pairs := parseExportedValueImplementLines(marked); len(pairs) != 0 {
		t.Errorf("value records read from closed-carrier lines: %v", pairs)
	}

	if pairs := parseExportedPointerImplementLines(marked); len(pairs) != 0 {
		t.Errorf("pointer records read from closed-carrier lines: %v", pairs)
	}

	if pairs := parseExportedValueImplementLines([]string{"[assembly: GoImplement<Box<Point>, Valuer>]"}); len(pairs) != 1 {
		t.Errorf("control: the unmarked value record reads as %v", pairs)
	}

	if pairs := parseExportedPointerImplementLines([]string{"[assembly: GoImplement<Box<Point>, Valuer>(Pointer = true)]"}); len(pairs) != 1 {
		t.Errorf("control: the unmarked pointer record reads as %v", pairs)
	}
}

// TestClosedCarrierBareName pins the name an adapter class is composed from.
func TestClosedCarrierBareName(t *testing.T) {
	for typeName, want := range map[string]string{
		"box<point>": "box",
		"boxlib_package.Box<boxlib_package.Point>": "boxlib_package.Box",
		"pair<point, map<@string, nint>>":          "pair",
		"point":                                    "point",
	} {
		if got := closedCarrierBareName(typeName); got != want {
			t.Errorf("closedCarrierBareName(%q) = %q, want %q", typeName, got, want)
		}
	}
}
