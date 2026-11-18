// nonSelfConstraintProxyRefusals_test.go - Gbtc
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
)

// One site per refusal of nonSelfConstraintProxyRefusals.go, each on its own marked line, and the shapes that must
// NOT be refused: tsc's own (a direct position, a method called through the constraint, the parameter returned), a
// zero value that is only returned, a []O PARAMETER beside the direct position (the element-wise widening's, governed
// by GO2CS2006), and class B: tsc's GetErrorBaseline shape, []O beside a func(O, O) parameter, handing O on under
// the SAME constraint. Its flat twin hands O to slices.Clone under `any`, and ruling 3c refuses it.
const nonSelfRefusalFixture = `package nsr

import "slices"

type parser interface{ Parse(string) bool }

type impl struct{ n int }

func (p *impl) Parse(s string) bool { p.n++; return s != "" }
func (p *impl) Name() string        { return "impl" }

func run[O parser](p O) O { p.Parse("x"); return p }

func allowed() *impl { return run(&impl{}) } // ALLOWED

type set[O parser] struct{ items []O }

var named set[*impl] // REFUSED named

type ptrParser interface {
	~*impl
	Parse(string) bool
}

func viaUnion[P ptrParser](p P) P { p.Parse("u"); return p }

func unionSite() *impl { return viaUnion(&impl{}) } // REFUSED union

func many[O parser](p O, all []O) O { p.Parse("m"); return p }

func manySite() *impl { return many(&impl{}, nil) } // ALLOWED

func keyed[O parser](p O, all map[string]O) O { p.Parse("k"); return p }

func aliasSite() *impl { return keyed(&impl{}, nil) } // REFUSED aliasing

func listed[O parser](p O) []O { p.Parse("l"); return []O{p} }

func aliasResultSite() []*impl { return listed(&impl{}) } // REFUSED aliasing-result

func sib[O parser, S ~[]O](p O, s S) O { p.Parse("s"); return p }

func siblingSite() *impl { return sib(&impl{}, []*impl{}) } // REFUSED sibling

func zeroCall[O parser](p O, ok bool) O {
	var zero O
	if !ok {
		zero.Parse("z")
	}
	p.Parse("p")
	return p
}

func zeroSite() *impl { return zeroCall(&impl{}, true) } // REFUSED zero-call

func zeroAny[O parser](p O) O {
	var zero O
	var a any = zero
	_ = a
	p.Parse("a")
	return p
}

func zeroAnySite() *impl { return zeroAny(&impl{}) } // REFUSED zero-any

func zeroReturn[O parser](p O, ok bool) O {
	var zero O
	p.Parse("r")
	if ok {
		return p
	}
	return zero
}

func zeroReturnSite() *impl { return zeroReturn(&impl{}, true) } // ALLOWED

type both interface {
	Parse(string) bool
	Name() string
}

type other interface{ Name() string }

func inner[Q other](q Q) Q { q.Name(); return q }

func outer[O both](p O) O { p.Parse("o"); return inner(p) }

func onwardSite() *impl { return outer(&impl{}) } // REFUSED onward

func byN(a, b *impl) int { return a.n - b.n }

func parsedAll[O parser](ps []O, cmp func(a, b O) int) int { return parsedSorted(ps, cmp) }

func parsedSorted[O parser](ps []O, cmp func(a, b O) int) int {
	qs := slices.Clone(ps)
	slices.SortFunc(qs, cmp)
	n := 0
	for _, q := range qs {
		if q.Parse("b") {
			n++
		}
	}
	return n
}

func classBSite() int { return parsedAll([]*impl{{}, {}}, byN) } // ALLOWED

func flatSite() int { return parsedSorted([]*impl{{}, {}}, byN) } // REFUSED onward-any
`

func loadNonSelfRefusal(t *testing.T) *nonSelfProxyRefusal {
	t.Helper()

	dir := t.TempDir()

	writeModuleFiles(t, dir, map[string]string{
		"go.mod": "module example/nsr\n\ngo 1.23\n",
		"nsr.go": nonSelfRefusalFixture,
	})

	return refuseNonSelfConstraintProxies(loadProductionForDir(t, dir))
}

// nonSelfFixtureLine is the 1-based line of the fixture holding `marker`.
func nonSelfFixtureLine(t *testing.T, marker string) int {
	t.Helper()

	for i, line := range strings.Split(nonSelfRefusalFixture, "\n") {
		if strings.Contains(line, marker) {
			return i + 1
		}
	}

	t.Fatalf("fixture has no line with %q", marker)
	return 0
}

// TestNonSelfProxyRefusalsFireBySite: every refusal fires on exactly its own line, and the allowed shapes are not
// refused. Each refusal is a red arm of its own (safety floor 13): drop its fixture line and its line leaves the set.
func TestNonSelfProxyRefusalsFireBySite(t *testing.T) {
	refusal := loadNonSelfRefusal(t)

	if refusal == nil {
		t.Fatal("no site was refused; the fixture holds one site per non-self proxy refusal")
	}

	got := map[int]bool{}

	for _, site := range refusal.sites {
		got[site.position.Line] = true
	}

	var gotLines []int

	for line := range got {
		gotLines = append(gotLines, line)
	}

	sort.Ints(gotLines)

	var want []int

	for _, marker := range []string{"REFUSED named", "REFUSED union", "REFUSED aliasing", "REFUSED aliasing-result", "REFUSED sibling", "REFUSED zero-call", "REFUSED zero-any", "REFUSED onward", "REFUSED onward-any"} {
		want = append(want, nonSelfFixtureLine(t, marker))
	}

	sort.Ints(want)

	if fmt.Sprint(gotLines) != fmt.Sprint(want) {
		t.Fatalf("refused lines %v, want %v\n%s", gotLines, want, refusalTexts(refusal))
	}

	for _, marker := range []string{"// ALLOWED"} {
		for i, line := range strings.Split(nonSelfRefusalFixture, "\n") {
			if strings.Contains(line, marker) && got[i+1] {
				t.Errorf("line %d (%s) was refused; the non-self proxy carries it", i+1, strings.TrimSpace(line))
			}
		}
	}
}

// TestNonSelfProxyRefusalsNameTheirReason: each refusal's text names its own reason, so a reader of the diagnostic
// knows which rule fired.
func TestNonSelfProxyRefusalsNameTheirReason(t *testing.T) {
	refusal := loadNonSelfRefusal(t)

	if refusal == nil {
		t.Fatal("no site was refused")
	}

	byLine := map[int]string{}

	for _, site := range refusal.sites {
		byLine[site.position.Line] += site.text + "\n"
	}

	for marker, reason := range map[string]string{
		"REFUSED named":           "generic NAMED type",
		"REFUSED union":           "type-term union with methods",
		"REFUSED aliasing":        "the parameter of type map[string]O",
		"REFUSED aliasing-result": "the result of type []O",
		"REFUSED sibling":         "sibling type parameter S",
		"REFUSED zero-call":       "the zero value zero has the method Parse called on it",
		"REFUSED zero-any":        "the zero value zero is assigned to an interface",
		"REFUSED onward":          "O is passed on to inner under the different constraint",
		"REFUSED onward-any":      "O is passed on to Clone under the different constraint any",
	} {
		if text := byLine[nonSelfFixtureLine(t, marker)]; !strings.Contains(text, reason) {
			t.Errorf("%s: the refusal does not name %q:\n%s", marker, reason, text)
		}
	}
}

func refusalTexts(refusal *nonSelfProxyRefusal) string {
	var lines []string

	for _, site := range refusal.sites {
		lines = append(lines, fmt.Sprintf("%d: %s", site.position.Line, site.text))
	}

	return strings.Join(lines, "\n")
}
