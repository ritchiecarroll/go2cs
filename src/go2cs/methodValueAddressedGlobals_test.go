// methodValueAddressedGlobals_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/types"
	"runtime"
	"testing"
)

// methodValueFixture is one package whose vars each sit in exactly one cell: a pointer-receiver
// METHOD VALUE (`f := g.inc`) on addressable storage rooted at a package-level value var takes that
// var's address, exactly as `&g` does, so the var must be heap-boxed or the method value binds a copy.
const methodValueFixture = "package mv\n" +
	"type T struct{ n int }\n" +
	"func (p *T) inc() { p.n++ }\n" +
	"func (t T) peek() int { return t.n }\n" +
	"type H struct{ t T }\n" +
	"type E struct{ *T }\n" +
	"type V struct{ T }\n" +
	"var g T\n" +
	"var gh H\n" +
	"var ga [2]T\n" +
	"var gvv V\n" +
	"var gs = make([]T, 1)\n" +
	"var gv T\n" +
	"var gp = &T{}\n" +
	"var ge = E{&T{}}\n" +
	"var gc T\n" +
	"var gu T\n"

// boxedMethodValueVars are the vars a method value addresses; unboxedMethodValueVars are the
// boundary: a slice element is reference storage already, a value receiver copies by definition, a
// pointer-typed var and a method promoted through an embedded POINTER need no address of the var,
// and an untouched var has no pointer to alias it.
var (
	boxedMethodValueVars   = []string{"g", "gh", "ga", "gvv"}
	unboxedMethodValueVars = []string{"gs", "gv", "gp", "ge", "gu"}
)

// TestScanAddressedGlobalsMarksPointerMethodValues pins the method-value arm of scanAddressedGlobals:
// before it, `f := g.inc` on a var nothing else addressed emitted `Ꮡ(g).inc`, a box of a COPY, so the
// call through f left g unchanged (Go prints 1, the conversion printed 0), and `ga[1].inc` named a
// `Ꮡga` no declaration supplied (CS0103).
func TestScanAddressedGlobalsMarksPointerMethodValues(t *testing.T) {
	dir := t.TempDir()
	writeModuleFiles(t, dir, map[string]string{
		"go.mod": "module example/mv\n\ngo 1.23\n",
		"prod.go": methodValueFixture +
			"func use() {\n" +
			"\tf := g.inc\n" +
			"\tf()\n" +
			"\t_ = gh.t.inc\n" +
			"\t_ = ga[1].inc\n" +
			"\t_ = gvv.inc\n" +
			"\t_ = gs[0].inc\n" +
			"\t_ = gv.peek\n" +
			"\t_ = gp.inc\n" +
			"\t_ = ge.inc\n" +
			"\tgc.inc()\n" +
			"\t_ = gu\n" +
			"}\n",
	})

	production := loadProductionForDir(t, dir)
	boxed := map[types.Object]bool{}

	for _, file := range production.Syntax {
		scanAddressedGlobals(file, production.Types, production.TypesInfo, boxed)
	}

	scope := production.Types.Scope()

	// gc is the existing call arm's control: a pointer-receiver CALL boxes its root already.
	for _, name := range append([]string{"gc"}, boxedMethodValueVars...) {
		if !boxed[scope.Lookup(name)] {
			t.Errorf("%s must be heap-boxed: a pointer-receiver method value or call takes its address", name)
		}
	}

	for _, name := range unboxedMethodValueVars {
		if boxed[scope.Lookup(name)] {
			t.Errorf("%s must not be heap-boxed: nothing takes its address", name)
		}
	}
}

// TestSiblingTestMethodValuesAreCollected pins the same rule for a method value the package's own
// in-package `_test.go` half takes on a PRODUCTION var: the production pass cannot see that file, so
// the type-free sibling scan carries each candidate (its root name and selector path) and the
// production pass resolves it against the real types.
func TestSiblingTestMethodValuesAreCollected(t *testing.T) {
	dir := t.TempDir()
	writeModuleFiles(t, dir, map[string]string{
		"go.mod":  "module example/mv\n\ngo 1.23\n",
		"prod.go": methodValueFixture,
		"mv_test.go": "package mv\n" +
			"import \"testing\"\n" +
			"var held = g.inc\n" +
			"func TestValues(t *testing.T) {\n" +
			"\t_ = gh.t.inc\n" +
			"\t_ = ga[1].inc\n" +
			"\t_ = gvv.inc\n" +
			"\t_ = gs[0].inc\n" +
			"\t_ = gv.peek\n" +
			"\t_ = gp.inc\n" +
			"\t_ = ge.inc\n" +
			"\tgu.inc()\n" +
			"}\n" +
			// A same-named LOCAL is not the package var.
			"func TestShadow(t *testing.T) {\n" +
			"\tgu := T{}\n" +
			"\t_ = gu.inc\n" +
			"}\n",
	})

	signals := collectSiblingTestSignals(dir, "mv", Options{
		targetPlatform: runtime.GOOS + "/" + runtime.GOARCH,
	})

	production := loadProductionForDir(t, dir)

	previousGlobals := packageAddressedGlobals
	previousNames := siblingTestAddressedGlobalNames
	previousValues := siblingTestMethodValues
	t.Cleanup(func() {
		packageAddressedGlobals = previousGlobals
		siblingTestAddressedGlobalNames = previousNames
		siblingTestMethodValues = previousValues
	})

	packageAddressedGlobals = map[types.Object]bool{}
	siblingTestAddressedGlobalNames = signals.addressedGlobalNames
	siblingTestMethodValues = signals.methodValues
	collectAddressedGlobals(nil, production.Types, production.TypesInfo)

	scope := production.Types.Scope()

	for _, name := range boxedMethodValueVars {
		if !packageAddressedGlobals[scope.Lookup(name)] {
			t.Errorf("%s must be heap-boxed: the package's own tests take a pointer-receiver method value of it", name)
		}
	}

	// gu is CALLED in the test half (a call binds the field in place) and method-valued only as a local.
	for _, name := range unboxedMethodValueVars {
		if packageAddressedGlobals[scope.Lookup(name)] {
			t.Errorf("%s must not be heap-boxed: nothing in the test half takes its address", name)
		}
	}
}
