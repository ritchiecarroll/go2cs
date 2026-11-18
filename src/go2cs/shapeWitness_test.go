// shapeWitness_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"fmt"
	"sort"
	"strings"
	"testing"

	"golang.org/x/tools/go/packages"
)

// One declaration per selection rule of shapeWitness.go, each marked with what the narrow selection must decide.
const shapeWitnessSelectionFixture = `package sw

import "fmt"

type Script interface {
	FileName() string
	Text() string
}

type lesser[T any] interface{ Less(T) bool }

type file struct{ name string }

func (f *file) FileName() string { return f.name }
func (f *file) Text() string     { return f.name }

type mem struct{}

func (mem) FileName() string { return "m" }
func (mem) Text() string     { return "m" }

type Span[T Script] struct{ S T } // SELECTED type

type Index[T Script] struct{ all []T } // SELECTED carrier

func (x *Index[T]) First() string { return x.all[0].Text() }

func Spans[T Script](s T) []Span[T] { return nil } // SELECTED result

func forward[T Script](s T) any { return Spans(s) } // SELECTED closure

var _ = forward(&file{})

func Plain[T Script](s T) string { return s.Text() } // NOT plain parameter

func Sorted[T lesser[T]](xs []T) []T { xs[0].Less(xs[0]); return xs } // NOT self-referential

func ViaInline[T interface{ Text() string }](s T) []T { s.Text(); return nil } // NOT inline constraint

func ViaForeign[T fmt.Stringer](s T) []T { s.String(); return nil } // NOT foreign constraint

func onlyValue[T Script](s T) []T { s.Text(); return nil } // NOT only a nominal instantiation

var _ = onlyValue(mem{})

func keptByPointer[T Script](s T) []T { s.Text(); helperText(s); idOf(s); return nil } // SELECTED pointer instantiation

func helperText[T Script](s T) string { return s.Text() } // SELECTED downward

func idOf[T Script](s T) T { return s } // NOT elided clause

var _ = keptByPointer(&file{})
`

// One site per GO2CS2013 refusal of stage 1, each on its own marked line, and a witnessed site that must not be refused.
const shapeWitnessRefusalFixture = `package swr

import (
	"fmt"
	"strings"
)

type writer interface{ WriteString(string) (int, error) }

func collect[T writer](x T) []T { x.WriteString("x"); return []T{x} }

func foreignPair() []*strings.Builder { return collect(&strings.Builder{}) } // REFUSED foreign-pointer

type Named interface{ Name() string }

type item struct{ n string }

func (i *item) Name() string { return i.n }

func values[T Named](xs []T) ([]T, []func() string) {
	var out []func() string
	for _, x := range xs {
		out = append(out, x.Name) // REFUSED method-value
	}
	return xs, out
}

var _, _ = values([]*item{})

func boxed[T Named](x T) ([]T, Named) {
	var n Named = x // REFUSED to-interface
	return []T{x}, n
}

var _, _ = boxed(&item{})

func allowed[T Named](x T) []T { x.Name(); return []T{x} }

var _ = allowed(&item{}) // ALLOWED

type stringish interface{ String() string }

type tag struct{ s string }

func (t *tag) String() string { return t.s }

func ext[U fmt.Stringer](u U) string { return u.String() }

func labels[T stringish](x T) []T {
	_ = x.String()
	_ = ext(x) // REFUSED onward
	return []T{x}
}

var _ = labels(&tag{})
`

func loadShapeWitnessFixture(t *testing.T, module, fixture string) *packages.Package {
	t.Helper()

	dir := t.TempDir()

	writeModuleFiles(t, dir, map[string]string{
		"go.mod":       "module example/" + module + "\n\ngo 1.23\n",
		module + ".go": fixture,
	})

	t.Cleanup(resetShapeWitnessState)

	return loadProductionForDir(t, dir)
}

// shapeWitnessFixtureLine is the 1-based line of fixture holding marker.
func shapeWitnessFixtureLine(t *testing.T, fixture, marker string) int {
	t.Helper()

	for i, line := range strings.Split(fixture, "\n") {
		if strings.Contains(line, marker) {
			return i + 1
		}
	}

	t.Fatalf("fixture has no line with %q", marker)
	return 0
}

// TestShapeWitnessSelection: the narrow selection takes exactly the SELECTED declarations -- an aliasing result, a
// generic named type, the closure's forwarder, an unexported declaration a pointer instantiates, a helper a selected body
// hands its T to (DOWNWARD) -- and none of the NOT
// ones: a plain parameter, a self-referential, inline or foreign constraint (each EXPORTED, so P3's predicate keeps it
// and only its own rule can exclude it), an unexported declaration instantiated only nominally, a helper whose clause is
// elided (its T binds the box as it is). Each NOT line is a red arm of its own (safety floor 13): loosen its rule and its name joins the set.
func TestShapeWitnessSelection(t *testing.T) {
	pkg := loadShapeWitnessFixture(t, "sw", shapeWitnessSelectionFixture)
	selection := computeShapeWitnessSelection(pkg)

	var got []string

	for object, decl := range selection {
		got = append(got, fmt.Sprintf("%s carrier=%v", object.Name(), decl.carrier))
	}

	sort.Strings(got)

	want := []string{"Index carrier=true", "Span carrier=false", "Spans carrier=true", "forward carrier=true", "helperText carrier=true", "keptByPointer carrier=true"}

	if fmt.Sprint(got) != fmt.Sprint(want) {
		t.Fatalf("selected %v\nwant     %v", got, want)
	}
}

// TestShapeWitnessRefusalsFireBySite: each stage-1 limit refuses (GO2CS2013) on exactly its own line, with its reason,
// and a witnessed site stage 1 places is not refused.
func TestShapeWitnessRefusalsFireBySite(t *testing.T) {
	pkg := loadShapeWitnessFixture(t, "swr", shapeWitnessRefusalFixture)
	refusal := prepareShapeWitness(pkg)

	if refusal == nil {
		t.Fatal("no site was refused; the fixture holds one site per stage-1 limit")
	}

	reasons := map[string]string{
		"REFUSED foreign-pointer": "is not to a non-generic named type of this package",
		"REFUSED method-value":    "needs a witness delegate",
		"REFUSED to-interface":    "converts to the interface",
		"REFUSED onward":          "keeps its C# constraint",
	}

	got := map[int]string{}

	for _, site := range refusal.sites {
		got[site.position.Line] += site.text + "\n"
	}

	for marker, reason := range reasons {
		line := shapeWitnessFixtureLine(t, shapeWitnessRefusalFixture, marker)

		if !strings.Contains(got[line], reason) {
			t.Errorf("line %d (%s): want a refusal naming %q, got %q", line, marker, reason, got[line])
		}

		delete(got, line)
	}

	for line, text := range got {
		t.Errorf("unexpected refusal at line %d: %s", line, text)
	}

	if line := shapeWitnessFixtureLine(t, shapeWitnessRefusalFixture, "// ALLOWED"); got[line] != "" {
		t.Errorf("the witnessed site at line %d was refused", line)
	}
}

// TestShapeWitnessLiftsNonSelfRefusals: through the pipeline (the selection installed first, as processConversion does),
// the GO2CS2005 shapes the witness carries -- a generic NAMED type over a pointer, a map[string]O parameter, a []O
// result -- are no longer refused, and every shape it does not select still is. Remove the shapeWitnessed check in
// refuseNonSelfConstraintProxies and the first three come back.
func TestShapeWitnessLiftsNonSelfRefusals(t *testing.T) {
	dir := t.TempDir()

	writeModuleFiles(t, dir, map[string]string{
		"go.mod": "module example/nsr\n\ngo 1.23\n",
		"nsr.go": nonSelfRefusalFixture,
	})

	pkg := loadProductionForDir(t, dir)

	t.Cleanup(resetShapeWitnessState)

	if refusal := prepareShapeWitness(pkg); refusal != nil {
		t.Fatalf("the class-B fixture holds no stage-1 limit, got:\n%v", refusal.Error())
	}

	refused := map[int]bool{}

	if refusal := refuseNonSelfConstraintProxies(pkg); refusal != nil {
		for _, site := range refusal.sites {
			refused[site.position.Line] = true
		}
	}

	for _, marker := range []string{"REFUSED named", "REFUSED aliasing", "REFUSED aliasing-result"} {
		if line := nonSelfFixtureLine(t, marker); refused[line] {
			t.Errorf("line %d (%s) is still refused; the shape witness carries it", line, marker)
		}
	}

	for _, marker := range []string{"REFUSED union", "REFUSED sibling", "REFUSED zero-call", "REFUSED zero-any", "REFUSED onward", "REFUSED onward-any"} {
		if line := nonSelfFixtureLine(t, marker); !refused[line] {
			t.Errorf("line %d (%s) is no longer refused; the shape witness does not select it", line, marker)
		}
	}
}
