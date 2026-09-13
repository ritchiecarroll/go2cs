// refLoweringAnalysis_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the ж-box arc's A1 classification pass (docs/phase4/DESIGN-zh-box-reduction.md §3.2/§3.3,
// refLoweringAnalysisOperations.go): the D1/D1′/D2 whitelist, every X veto family, the two-sided
// fixed point (forward strips and caller-shape strips), the §3.3 argument-shape census rows, the
// address-taken-local reversion census, the exported-candidate (A′) flag, and the §3.5
// production-files-only determinism invariant (an export_test.go func-value alias must not
// desynchronize the -tests classification from the -stdlib one).

package main

import (
	"go/ast"
	"go/types"
	"strings"
	"testing"

	"golang.org/x/tools/go/packages"
)

// loadRefLoweringFixture writes a one-package fixture module and loads it with full syntax.
func loadRefLoweringFixture(t *testing.T, files map[string]string, withTests bool) *packages.Package {
	t.Helper()

	dir := t.TempDir()
	writeModuleFiles(t, dir, files)

	loaded, err := packages.Load(&packages.Config{Mode: packages.LoadAllSyntax, Dir: dir, Tests: withTests}, ".")

	if err != nil {
		t.Fatalf("fixture load: %v", err)
	}

	if !withTests {
		if len(loaded) != 1 {
			t.Fatalf("fixture load: expected 1 package, got %d", len(loaded))
		}

		return loaded[0]
	}

	// With Tests: pick the test-augmented variant (its Syntax includes the _test.go files) —
	// the same merged white-box package the -tests driver type-checks.
	for _, pkg := range loaded {
		for _, file := range pkg.Syntax {
			filename := pkg.Fset.Position(file.Pos()).Filename

			if strings.HasSuffix(filename, "_test.go") {
				return pkg
			}
		}
	}

	t.Fatal("fixture load: no test-augmented package variant found")

	return nil
}

// analyzeFixture runs the classification pass exactly as the census driver does — including the
// capture-mode pre-pass, without which every receiver-position use falls back to the box (see
// receiverUseKeptReason's nil-map arm) and B′ §4.2's selection rule would be untested here.
func analyzeFixture(t *testing.T, pkg *packages.Package) *refLoweringPackageResult {
	t.Helper()

	if len(pkg.Errors) > 0 {
		t.Fatalf("fixture did not type-check cleanly: %v", pkg.Errors)
	}

	savedCaptureMode, savedDirectBox := packageCaptureModeMethods, packageDirectBoxReceiverMethods

	packageCaptureModeMethods = make(map[*types.Func]bool)
	packageDirectBoxReceiverMethods = make(map[*types.Func]bool)

	defer func() {
		packageCaptureModeMethods, packageDirectBoxReceiverMethods = savedCaptureMode, savedDirectBox
	}()

	collectCaptureModeMethods(pkg)

	return analyzeRefLowering(pkg.Fset, pkg.Syntax, pkg.Types, pkg.TypesInfo, censusLinknameHandles(pkg.Syntax), nil)
}

// paramOf fetches one parameter verdict, failing loudly when the shape moved.
func paramOf(t *testing.T, result *refLoweringPackageResult, funcName string, index int) *refParamVerdict {
	t.Helper()

	verdict, ok := result.Funcs[funcName]

	if !ok {
		t.Fatalf("function %q not classified", funcName)
	}

	for _, param := range verdict.Params {
		if param.Index == index {
			return param
		}
	}

	t.Fatalf("function %q has no pointer parameter at index %d", funcName, index)

	return nil
}

// hasVeto reports whether a veto tag (exact) was recorded.
func hasVeto(param *refParamVerdict, tag string) bool {
	for _, veto := range param.Vetoes {
		if veto == tag {
			return true
		}
	}

	return false
}

const refLoweringCoreFixture = `package fixture

import "unsafe"

type pair struct {
	x uint64
	y uint64
}

func (p *pair) bump() { p.x++ }

var sink *int
var sunkAddr *uint64

// D1: pure deref traffic — both parameters lower.
func sub(out, a *uint64) { *out = *a + 1 }

// D1': derived field addresses fed to lowered positions — lowers.
func fieldFwd(p *pair) { sub(&p.x, &p.y) }

// D2: forwarding into lowered positions — lowers.
func fwd(p *uint64) { sub(p, p) }

// D2 chain: lowers only because fwd lowers (fixed-point transitivity).
func chainFwd(p *uint64) { fwd(p) }

// X1: nil comparison.
func nilCheck(p *int) bool { return p == nil }

// X2-return: the constructor/self-return shape.
func constructor(p *int) *int { return p }

// X2-escape: stored into a global.
func store(p *int) { sink = p }

// Forward into a vetoed callee: strips at the fixed point (forward-unlowered).
func chainBroken(p *int) { store(p) }

// X2-capture: referenced inside a closure body.
func capture(p *int) func() int { return func() int { return *p } }

// X2-defer-arg: a derived address in a defer frame (the boxed thunk needs a box the lowered
// parameter would no longer have).
func deferArg(p *pair) {
	defer sub(&p.x, &p.x)
	p.y = 1
}

// X3: representation.
func repr(p *int) uintptr { return uintptr(unsafe.Pointer(p)) }

// X3: a method call on p (receivers stay ж in Phase A).
func methodOn(p *pair) { p.bump() }

// X4: re-pointing the parameter.
func repoint(p *int, q *int) { p = q; _ = p }

// Variadic pointer parameter: a SLICE of pointers, never a candidate.
func variadicPtr(ps ...*int) {
	for range ps {
	}
}

// Forward into a variadic position: the pointer lands in the arg slice (X2).
func variadicFwd(p *int) { variadicPtr(p) }

// X5-named-pointer: the wrapper's operator surface must survive.
type ptrT *int

func namedPtr(p ptrT) { _ = p }

// X5-func-value: otherwise clean, but taken as a value below.
func fwdValue(p *uint64) { sub(p, p) }

var handler = fwdValue

// Exported and clean: not lowered in Phase A, exported-candidate under A'.
func Clean(out, a *uint64) { sub(out, a) }
`

func TestRefLoweringClassificationCore(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: loads the fixture package via go/packages")
	}

	pkg := loadRefLoweringFixture(t, map[string]string{
		"go.mod":     "module example.com/fixture\n\ngo 1.23\n",
		"fixture.go": refLoweringCoreFixture,
	}, false)

	result := analyzeFixture(t, pkg)

	// The D family lowers.
	for _, tc := range []struct {
		fn    string
		index int
	}{
		{"sub", 0}, {"sub", 1}, {"fieldFwd", 0}, {"fwd", 0}, {"chainFwd", 0},
	} {
		param := paramOf(t, result, tc.fn, tc.index)

		if !param.LoweredA {
			t.Errorf("%s#%d must lower (vetoes %v, strippedBy %q)", tc.fn, tc.index, param.Vetoes, param.StrippedBy)
		}
	}

	// Each X family vetoes with its own tag.
	for _, tc := range []struct {
		fn    string
		index int
		tag   string
	}{
		{"nilCheck", 0, refVetoX1Identity},
		{"constructor", 0, refVetoX2Return},
		{"store", 0, refVetoX2Escape},
		{"capture", 0, refVetoX2Capture},
		{"deferArg", 0, refVetoX2DeferArg},
		{"repr", 0, refVetoX3Repr},
		{"methodOn", 0, refVetoX3Repr},
		{"repoint", 0, refVetoX4Repoint},
		{"variadicFwd", 0, refVetoX2Escape},
		{"namedPtr", 0, refVetoX5NamedPtr},
	} {
		param := paramOf(t, result, tc.fn, tc.index)

		if param.LoweredA {
			t.Errorf("%s#%d must NOT lower", tc.fn, tc.index)
		}

		if !hasVeto(param, tc.tag) {
			t.Errorf("%s#%d missing veto %s (recorded %v)", tc.fn, tc.index, tc.tag, param.Vetoes)
		}
	}

	// A variadic ...*T parameter is a slice, never a candidate.
	if verdict := result.Funcs["variadicPtr"]; verdict == nil {
		t.Error("variadicPtr not classified")
	} else if len(verdict.Params) != 0 {
		t.Errorf("variadicPtr must have no candidate pointer params, got %d", len(verdict.Params))
	}

	// The fixed point strips a forward into a vetoed callee.
	broken := paramOf(t, result, "chainBroken", 0)

	if broken.LoweredA {
		t.Error("chainBroken#0 must strip: it forwards into store, which is X2-vetoed")
	}

	if !strings.HasPrefix(broken.StrippedBy, refVetoForward) {
		t.Errorf("chainBroken#0 strippedBy = %q, want %s:*", broken.StrippedBy, refVetoForward)
	}

	// X5-func-value is function-level.
	if verdict := result.Funcs["fwdValue"]; verdict == nil {
		t.Error("fwdValue not classified")
	} else {
		found := false

		for _, veto := range verdict.FuncVetoes {
			if veto == refFuncVetoFuncValue {
				found = true
			}
		}

		if !found {
			t.Errorf("fwdValue must carry %s (recorded %v)", refFuncVetoFuncValue, verdict.FuncVetoes)
		}
	}

	// Exported functions stay un-lowered in Phase A and flag as A' candidates.
	clean := paramOf(t, result, "Clean", 0)

	if clean.LoweredA {
		t.Error("Clean#0 must not lower in Phase A (exported)")
	}

	unionFuncs := map[string]*refFuncVerdict{}

	for name, verdict := range result.Funcs {
		unionFuncs[result.PkgPath+"|"+name] = verdict
	}

	lowered, stripped := resolveRefLoweringFixedPoint(unionFuncs, result.CallArgs, true)
	applyRefLoweringVerdicts(unionFuncs, lowered, stripped, true)

	if !result.Funcs["Clean"].ExportedCandidate {
		t.Error("Clean must flag as an exported candidate under the A' fixed point")
	}

	if !paramOf(t, result, "Clean", 0).LoweredAPrime {
		t.Error("Clean#0 must lower under the A' fixed point")
	}
}

func TestRefLoweringSyntheticFixedPointStrips(t *testing.T) {
	// The resolver in isolation: a caller-shape strip (an other-veto argument shape) and the D2
	// cascade it triggers — the two-sided fixed point of §3.2.
	pos := func(name string) refPosKey { return refPosKey{PkgPath: "p", Func: name, Index: 0} }

	funcs := map[string]*refFuncVerdict{
		"leaf": {PkgPath: "p", Name: "leaf", Params: []*refParamVerdict{{Index: 0, Name: "a"}}},
		"mid": {PkgPath: "p", Name: "mid", Params: []*refParamVerdict{{
			Index: 0, Name: "b", Forwards: []refForward{{Target: pos("leaf"), Kind: "D2"}},
		}}},
		"top": {PkgPath: "p", Name: "top", Params: []*refParamVerdict{{
			Index: 0, Name: "c", Forwards: []refForward{{Target: pos("mid"), Kind: "D2"}},
		}}},
	}

	callArgs := []refCallArg{{Target: pos("leaf"), Shape: refShapeOtherVeto, Detail: "synthetic"}}

	lowered, stripped := resolveRefLoweringFixedPoint(funcs, callArgs, false)

	if lowered[pos("leaf")] || lowered[pos("mid")] || lowered[pos("top")] {
		t.Errorf("caller-shape strip must cascade through the D2 chain: lowered=%v", lowered)
	}

	if !strings.HasPrefix(stripped[pos("leaf")], refVetoCallerShape) {
		t.Errorf("leaf strip reason = %q, want %s:*", stripped[pos("leaf")], refVetoCallerShape)
	}

	if !strings.HasPrefix(stripped[pos("mid")], refVetoForward) || !strings.HasPrefix(stripped[pos("top")], refVetoForward) {
		t.Errorf("chain strips must be forward-tagged: mid=%q top=%q", stripped[pos("mid")], stripped[pos("top")])
	}

	// Control: without the shape veto the whole chain lowers.
	lowered, _ = resolveRefLoweringFixedPoint(funcs, nil, false)

	if !lowered[pos("leaf")] || !lowered[pos("mid")] || !lowered[pos("top")] {
		t.Errorf("control chain must lower end to end: lowered=%v", lowered)
	}
}

const refLoweringShapeFixture = `package shapes

type mont [4]uint64

type elem struct{ x mont }

type big struct {
	a uint64
	b uint64
}

var globalBig big

// The lowered sinks.
func sink(p *big)          { p.a++ }
func sinkArr(p *[4]uint64) { p[0]++ }
func sub2(out *uint64)     { *out = 7 }

func mk() *big { return &big{} }

type holder struct{ f big }

// row 1: field address of a deref'd receiver base.
func (h *holder) row1() { sink(&h.f) }

// rows 2/6/7 and defer-go.
func rows() {
	var x big
	sink(&x)          // row 2 (local)
	sink(&globalBig)  // row 2 (global detail)
	sink(&big{})      // row 6 composite
	sink(new(big))    // row 6 new
	sink(mk())        // row 6 call result
	sink(nil)         // row 7
	var d big
	defer sink(&d) // defer-go bucket; d keeps its box
	_ = d
	_ = x
}

// row 3: a pointer variable forwarded (also a D2 use of q).
func relay(q *big) { sink(q) }

// row 4: element address.
func row4() {
	var arr [3]uint64
	sub2(&arr[0])
}

// row 5: the fiat conversion shape.
func conv(e *elem) { sinkArr((*[4]uint64)(&e.x)) }
`

func TestRefLoweringCallSiteShapes(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: loads the fixture package via go/packages")
	}

	pkg := loadRefLoweringFixture(t, map[string]string{
		"go.mod":    "module example.com/shapes\n\ngo 1.23\n",
		"shapes.go": refLoweringShapeFixture,
	}, false)

	result := analyzeFixture(t, pkg)
	summary := summarizeRefLowering(result)

	expected := map[string]int{
		refShapeFieldAddr:  2, // (&h.f) + conv's (&e.x) inside the row-5 conversion? No: conv classifies row5 at the arg; &h.f and... see below
		refShapeLocalAddr:  3, // &x, &globalBig(global), &arr[0]? No — &arr[0] is row 4. Recounted below.
		refShapePointerVar: 1, // relay's q
		refShapeElemAddr:   1, // &arr[0]
		refShapePtrConv:    1, // (*[4]uint64)(&e.x)
		refShapeTempNeeded: 3, // &big{}, new(big), mk()
		refShapeNilLit:     1, // nil
		refShapeDeferGo:    1, // defer sink(&d)
	}

	// Correct the two mis-annotated rows above (kept as comments for the arithmetic): the shape
	// walk classifies &h.f as row 1 (selector operand) and &e.x reaches the census only inside
	// the row-5 conversion, so field-addr counts exactly 1; &x and &globalBig are the two row-2
	// entries.
	expected[refShapeFieldAddr] = 1
	expected[refShapeLocalAddr] = 2

	for shape, want := range expected {
		if got := summary.ShapeCounts[shape]; got != want {
			t.Errorf("shape %s: got %d, want %d (full: %v)", shape, got, want, summary.ShapeCounts)
		}
	}

	if got := summary.ShapeCounts[refShapeOtherVeto]; got != 0 {
		t.Errorf("no other-veto shapes expected in the fixture, got %d", got)
	}

	// The row-5 site is recorded with conversion-of-address detail (the §10.3 temp rule's priced
	// constituency).
	found := false

	for _, callArg := range result.CallArgs {
		if callArg.Shape == refShapePtrConv {
			found = true

			if callArg.Detail != "conv-of-address" {
				t.Errorf("row-5 detail = %q, want conv-of-address", callArg.Detail)
			}
		}
	}

	if !found {
		t.Error("row-5 conversion site not recorded")
	}
}

const refLoweringLocalsFixture = `package locals

type mutexish struct {
	held  bool
	spare uint64
}

func (m *mutexish) lock() { m.held = true }

type boxy struct {
	n     uint64
	spare uint64
}

// Direct-ж: takes the address of a receiver field, so the box IS its emitted receiver.
func (b *boxy) addr() *uint64 { return &b.n }

var escape *uint64

func sub3(out *uint64) { *out = 3 }

func use(v uint64) uint64   { return v }
func useSlice(b []byte) int { return len(b) }

// Reverts: every address use feeds a lowered position.
func lowersLocal() uint64 {
	var x uint64
	sub3(&x)
	return use(x)
}

// Kept: the address also escapes to a global.
func keptStore() {
	var y uint64
	sub3(&y)
	escape = &y
}

// Kept: the address flows to a lowered position under defer (carve-out (a)).
func keptDefer() {
	var z uint64
	defer sub3(&z)
	_ = z
}

// Reverts (B′ §4.2): an implicit address-take by a pointer-receiver method whose receiver is
// emitted as a [GoRecv] ref binds the local's own storage — no box is consumed, so the other
// address use feeding a lowered position is free to revert.
func revertsRefMethod() {
	var m mutexish
	m.lock()
	sub3(&m.spare)
	_ = m.held
}

// Kept: the callee is DIRECT-ж (it takes the address of a receiver field), so the box IS its
// receiver.
func keptBoxMethod() {
	var b boxy
	sub3(b.addr())
	sub3(&b.spare)
}

// Kept: a method VALUE, not a call — the delegate closes over the box.
func keptMethodValue() {
	var m mutexish
	f := m.lock
	f()
	sub3(&m.spare)
}

// Kept: a receiver under defer — the frame outlives the statement (§3.3 carve-out (a)).
func keptDeferRecv() {
	var m mutexish
	defer m.lock()
	sub3(&m.spare)
}

// Kept: a receiver under go — same carve-out.
func keptGoRecv() {
	var m mutexish
	go m.lock()
	sub3(&m.spare)
}

// Kept: slicing an array local aliases its storage.
func keptSlice() int {
	var a [4]byte
	return useSlice(a[:])
}
`

func TestRefLoweringLocalsCensus(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: loads the fixture package via go/packages")
	}

	pkg := loadRefLoweringFixture(t, map[string]string{
		"go.mod":    "module example.com/locals\n\ngo 1.23\n",
		"locals.go": refLoweringLocalsFixture,
	}, false)

	result := analyzeFixture(t, pkg)

	verdictFor := func(funcName, varName string) *refLocalVerdict {
		t.Helper()

		for i := range result.Locals {
			local := &result.Locals[i]

			if local.Func == funcName && local.Name == varName {
				return local
			}
		}

		t.Fatalf("local %s.%s not in the census (have %+v)", funcName, varName, result.Locals)

		return nil
	}

	if v := verdictFor("lowersLocal", "x"); !v.Lowers {
		t.Errorf("lowersLocal.x must revert (kept: %v)", v.KeptReasons)
	}

	// B′ §4.2: a `[GoRecv] this ref T` receiver consumes no box, so the receiver-position use
	// imposes no kept-reason and the local reverts. Before 2026-08-26 this was a blanket
	// "ptr-receiver" keep, which minted a `heap()` box the emitted body never referenced.
	if v := verdictFor("revertsRefMethod", "m"); !v.Lowers {
		t.Errorf("revertsRefMethod.m must revert — a ref-receiver call takes no box (kept: %v)", v.KeptReasons)
	}

	for _, tc := range []struct {
		fn, name, reason string
	}{
		{"keptStore", "y", "addr-escapes"},
		{"keptDefer", "z", "defer-go-arg"},
		{"keptSlice", "a", "array-slice"},
		{"keptBoxMethod", "b", "ptr-receiver-box"},
		{"keptMethodValue", "m", "ptr-receiver-value"},
		{"keptDeferRecv", "m", "ptr-receiver-defer-go"},
		{"keptGoRecv", "m", "ptr-receiver-defer-go"},
	} {
		v := verdictFor(tc.fn, tc.name)

		if v.Lowers {
			t.Errorf("%s.%s must keep its box", tc.fn, tc.name)
			continue
		}

		found := false

		for _, reason := range v.KeptReasons {
			if reason == tc.reason {
				found = true
			}
		}

		if !found {
			t.Errorf("%s.%s kept reasons %v, want %s", tc.fn, tc.name, v.KeptReasons, tc.reason)
		}
	}
}

func TestRefLoweringProductionOnlyDeterminism(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: loads the fixture package via go/packages")
	}

	// The §3.5 invariant, at the analysis level: an export_test.go func-value alias must not
	// change the production classification, because the pass reads production files only. The
	// -tests driver hands the pass production entries AND the entry point filters _test.go
	// structurally; this test proves the underlying rule both ways.
	pkg := loadRefLoweringFixture(t, map[string]string{
		"go.mod": "module example.com/detrm\n\ngo 1.23\n",
		"detrm.go": `package detrm

func lowerme(out *uint64) { *out = 1 }

func caller() {
	var x uint64
	lowerme(&x)
	_ = x
}
`,
		"export_test.go": `package detrm

// The white-box alias shape: a func value over the unexported function.
var Hook = lowerme
`,
	}, true)

	var production, all []*ast.File

	for _, file := range pkg.Syntax {
		filename := pkg.Fset.Position(file.Pos()).Filename
		all = append(all, file)

		if !strings.HasSuffix(filename, "_test.go") {
			production = append(production, file)
		}
	}

	if len(production) == len(all) {
		t.Fatal("fixture must include a _test.go file in the merged package")
	}

	prodResult := analyzeRefLowering(pkg.Fset, production, pkg.Types, pkg.TypesInfo, censusLinknameHandles(production), nil)

	if param := paramOf(t, prodResult, "lowerme", 0); !param.LoweredA {
		t.Errorf("production-only classification must lower lowerme#0 (vetoes %v)", param.Vetoes)
	}

	allResult := analyzeRefLowering(pkg.Fset, all, pkg.Types, pkg.TypesInfo, censusLinknameHandles(all), nil)
	verdict := allResult.Funcs["lowerme"]

	if verdict == nil {
		t.Fatal("lowerme missing from the all-files control")
	}

	// The control proves the test file WOULD veto — which is exactly why the pass must not see it.
	found := false

	for _, veto := range verdict.FuncVetoes {
		if veto == refFuncVetoFuncValue {
			found = true
		}
	}

	if !found {
		t.Error("control: the export_test.go alias must X5 the function when test files leak into the walk")
	}

	// And the driver-facing entry filters by filename even when handed the merged set.
	entries := make([]FileEntry, 0, len(all))

	for _, file := range all {
		entries = append(entries, FileEntry{file: file, filePath: pkg.Fset.Position(file.Pos()).Filename})
	}

	savedResult := packageRefLoweringResult
	defer func() { packageRefLoweringResult = savedResult }()

	performRefLoweringAnalysis(entries, pkg.Types, pkg.TypesInfo, Options{})

	if packageRefLoweringResult == nil {
		t.Fatal("performRefLoweringAnalysis recorded no result")
	}

	if param := paramOf(t, packageRefLoweringResult, "lowerme", 0); !param.LoweredA {
		t.Error("performRefLoweringAnalysis must classify from production files only (structural _test.go filter)")
	}
}

func TestRefLoweringBodilessAndLinknameVetoes(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: loads the fixture package via go/packages")
	}

	// Bodiless declarations type-check with an error ("missing function body"), which the census
	// driver skips packages over — but the classifier itself must still veto them (the -tests and
	// hand-owned drivers can meet them through assembly stubs under purego-less tag sets). Load
	// tolerating the error.
	dir := t.TempDir()
	writeModuleFiles(t, dir, map[string]string{
		"go.mod": "module example.com/stubs\n\ngo 1.23\n",
		"stubs.go": `package stubs

import _ "unsafe"

// The assembly-stub shape: vacuously D-clean, must veto X5-bodiless.
func asmStub(p *int)

//go:linkname handled
var handled int

// A one-arg linkname HANDLE on a function: the registry exposure (X5-linkname).
//go:linkname exposed
func exposed(p *uint64) { *p = 1 }
`,
	})

	loaded, err := packages.Load(&packages.Config{Mode: packages.LoadAllSyntax, Dir: dir}, ".")

	if err != nil || len(loaded) != 1 {
		t.Fatalf("fixture load: %v (%d packages)", err, len(loaded))
	}

	pkg := loaded[0]
	result := analyzeRefLowering(pkg.Fset, pkg.Syntax, pkg.Types, pkg.TypesInfo, censusLinknameHandles(pkg.Syntax), nil)

	assertFuncVeto := func(name, tag string) {
		t.Helper()

		verdict := result.Funcs[name]

		if verdict == nil {
			t.Fatalf("%s not classified", name)
		}

		for _, veto := range verdict.FuncVetoes {
			if veto == tag {
				return
			}
		}

		t.Errorf("%s must carry %s (recorded %v)", name, tag, verdict.FuncVetoes)
	}

	assertFuncVeto("asmStub", refFuncVetoBodiless)
	assertFuncVeto("exposed", refFuncVetoLinkname)
}

func TestRefLoweringHandOwnVetoes(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: loads the fixture package via go/packages")
	}

	// The A2 hand-own arms: a function declared in a file whose emission is hand-owned vetoes
	// mechanically (X5-hand-owned); a function on the curated called-from-hand-own list vetoes by
	// name (X5-hand-own-caller). Both keep the boxed signature the frozen C# compiled against.
	pkg := loadRefLoweringFixture(t, map[string]string{
		"go.mod": "module example.com/handown\n\ngo 1.23\n",
		"frozen.go": `package handown

// Emission of THIS file is hand-owned (simulated via the driver's manualConversion flag).
func frozenFn(out *uint64) { *out = 1 }
`,
		"open.go": `package handown

func openFn(out *uint64) { *out = 2 }

func curatedFn(out *uint64) { *out = 3 }
`,
	}, false)

	if len(pkg.Errors) > 0 {
		t.Fatalf("fixture did not type-check cleanly: %v", pkg.Errors)
	}

	// Identify the files by their declared functions.
	var frozenFile *ast.File

	for _, file := range pkg.Syntax {
		for _, decl := range file.Decls {
			if fn, ok := decl.(*ast.FuncDecl); ok && fn.Name.Name == "frozenFn" {
				frozenFile = file
			}
		}
	}

	if frozenFile == nil {
		t.Fatal("frozen.go's syntax not found")
	}

	// Curated-list seam: register the fixture's function, restore after.
	curatedKey := pkg.Types.Path() + ".curatedFn"
	refLoweringHandOwnCallers[curatedKey] = true
	defer delete(refLoweringHandOwnCallers, curatedKey)

	result := analyzeRefLowering(pkg.Fset, pkg.Syntax, pkg.Types, pkg.TypesInfo,
		censusLinknameHandles(pkg.Syntax), map[*ast.File]bool{frozenFile: true})

	assertVeto := func(name, tag string, wantLowered bool) {
		t.Helper()

		verdict := result.Funcs[name]

		if verdict == nil {
			t.Fatalf("%s not classified", name)
		}

		found := false

		for _, veto := range verdict.FuncVetoes {
			if veto == tag {
				found = true
			}
		}

		if tag != "" && !found {
			t.Errorf("%s must carry %s (recorded %v)", name, tag, verdict.FuncVetoes)
		}

		lowered := false

		for _, param := range verdict.Params {
			if param.LoweredA {
				lowered = true
			}
		}

		if lowered != wantLowered {
			t.Errorf("%s lowered=%v, want %v", name, lowered, wantLowered)
		}
	}

	assertVeto("frozenFn", refFuncVetoHandOwned, false)
	assertVeto("curatedFn", refFuncVetoHandOwnCaller, false)
	assertVeto("openFn", "", true)
}

func TestRefLoweringEmittabilityNarrowing(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: loads the fixture package via go/packages")
	}

	// The A2 emittability narrowing: (1) a derived address whose chain auto-derefs through a
	// POINTER field vetoes the parameter (D1' side) and strips the target position (shape side);
	// (2) a row-5 pointer conversion of a VALUE (not an address) strips its position — the ruled
	// hoisted-temp mechanism covers conv-of-address only.
	pkg := loadRefLoweringFixture(t, map[string]string{
		"go.mod": "module example.com/narrow\n\ngo 1.23\n",
		"narrow.go": `package narrow

type inner struct{ n uint64 }

type outer struct{ p *inner }

var globalOuter outer
var globalArrPtr *[4]uint64

// sinkA stays lowered: its shapes are all clean.
func sinkA(x *uint64) { *x = 1 }

// sinkB strips: its ONLY call site's shape is a pointer-field chain.
func sinkB(x *uint64) { *x = 2 }

// sinkC stays lowered (conv-of-address); sinkD strips (conv-of-value); sinkE strips (the D1'
// fixture's ptr-field-chain site — the strip lands on BOTH sides of the fixed point).
func sinkC(p *[4]uint64) { p[0]++ }
func sinkD(p *[4]uint64) { p[0]++ }
func sinkE(x *uint64)   { *x = 5 }

// A clean site keeps sinkA lowered.
func cleanSite() {
	var v uint64
	sinkA(&v)
	_ = v
}

// D1' side: the chain crosses the pointer field p — o vetoes (and sinkE's position strips).
func viaPtrField(o *outer) { sinkE(&o.p.n) }

// Shape side: the same chain at sinkB's only site strips sinkB#0.
func shapeSite() { sinkB(&globalOuter.p.n) }

// Row 5: conv-of-address stays; conv-of-value strips.
type mont [4]uint64

var m mont

func convClean() { sinkC((*[4]uint64)(&m)) }
func convValue() { sinkD((*[4]uint64)(globalArrPtr)) }
`,
	}, false)

	result := analyzeFixture(t, pkg)

	// D1' param-side veto.
	via := paramOf(t, result, "viaPtrField", 0)

	if via.LoweredA {
		t.Error("viaPtrField#0 must veto: its derived address crosses a pointer field")
	}

	if !hasVeto(via, refVetoOtherUse) {
		t.Errorf("viaPtrField#0 vetoes = %v, want %s", via.Vetoes, refVetoOtherUse)
	}

	// Shape-side strips.
	if p := paramOf(t, result, "sinkB", 0); p.LoweredA {
		t.Error("sinkB#0 must strip: its only site is a ptr-field-chain shape")
	} else if !strings.HasPrefix(p.StrippedBy, refVetoCallerShape) {
		t.Errorf("sinkB#0 strippedBy = %q, want %s:*", p.StrippedBy, refVetoCallerShape)
	}

	if p := paramOf(t, result, "sinkD", 0); p.LoweredA {
		t.Error("sinkD#0 must strip: conv-of-value has no ruled emission")
	}

	// Controls.
	if p := paramOf(t, result, "sinkA", 0); !p.LoweredA {
		t.Errorf("sinkA#0 must stay lowered (vetoes %v, strippedBy %q)", p.Vetoes, p.StrippedBy)
	}

	if p := paramOf(t, result, "sinkC", 0); !p.LoweredA {
		t.Errorf("sinkC#0 must stay lowered (conv-of-address; vetoes %v, strippedBy %q)", p.Vetoes, p.StrippedBy)
	}
}

// TestReceiverFieldAddressExcludesPrimary guards B′'s eligibility rule (ruling 2026-09-03,
// superseding the capture-mode keep-box): a method that takes the address of a RECEIVER FIELD
// cannot be a ref-return primary. It needs the box receiver, because only `Ꮡv.of(field)` yields an
// ALIASING field pointer — a ref receiver's `Ꮡ(v.field)` boxes a COPY and drops the write. TWO
// shapes reach that address and both must exclude:
//   - EXPLICIT `&v.field` — bodyTakesReceiverFieldAddress (the pre-existing sibling);
//   - IMPLICIT `v.field.M()` where field is a value struct and M has a pointer receiver, so Go
//     addresses v.field to make the call — bodyTakesImplicitReceiverFieldAddress (the fix). This is
//     the edwards25519 projP2.Zero case (`v.X.Zero()`), promoted to a `this ref projP2 v` receiver
//     whose body still emitted `Ꮡv.of(projP2.ᏑX).Zero()` — 99 CS0103 across the package.
// The pointer-field call `o.ptr.Bump()` is the negative control: that address is the pointee's,
// reached through a box that already exists, so it is NOT a receiver-field address.
func TestReceiverFieldAddressExcludesPrimary(t *testing.T) {
	pkg := loadRefLoweringFixture(t, map[string]string{
		"go.mod": "module fieldaddr\n\ngo 1.23\n",
		"p.go": `package fieldaddr

type Inner struct{ n int }

func (i *Inner) Bump() { i.n++ }

type Outer struct {
	x   Inner
	ptr *Inner
}

func (o *Outer) Explicit() *Outer   { _ = &o.x; return o }
func (o *Outer) Implicit() *Outer   { o.x.Bump(); return o }
func (o *Outer) ThroughPtr() *Outer { o.ptr.Bump(); return o }
func (o *Outer) Bare() *Outer       { return o }
`,
	}, false)

	info := pkg.TypesInfo

	find := func(name string) (*ast.BlockStmt, string, types.Object) {
		t.Helper()

		for _, file := range pkg.Syntax {
			for _, decl := range file.Decls {
				fn, ok := decl.(*ast.FuncDecl)

				if !ok || fn.Recv == nil || fn.Name.Name != name {
					continue
				}

				return fn.Body, fn.Recv.List[0].Names[0].Name, recvObjectOf(fn, info)
			}
		}

		t.Fatalf("method %s not found in fixture", name)

		return nil, "", nil
	}

	// Shape 1 — explicit &o.x: the explicit predicate fires, the implicit one does not.
	if body, recv, obj := find("Explicit"); !bodyTakesReceiverFieldAddress(body, recv, obj, info) {
		t.Error("Explicit: bodyTakesReceiverFieldAddress must catch `&o.x`")
	} else if bodyTakesImplicitReceiverFieldAddress(body, recv, obj, info) {
		t.Error("Explicit: bodyTakesImplicitReceiverFieldAddress must not fire on an explicit `&` (distinct shapes)")
	}

	// Shape 2 — implicit &o.x via a pointer-method call on a value struct field: the implicit
	// predicate fires, the explicit one does not.
	if body, recv, obj := find("Implicit"); !bodyTakesImplicitReceiverFieldAddress(body, recv, obj, info) {
		t.Error("Implicit: bodyTakesImplicitReceiverFieldAddress must catch `o.x.Bump()` (Bump ptr-receiver, x value struct)")
	} else if bodyTakesReceiverFieldAddress(body, recv, obj, info) {
		t.Error("Implicit: bodyTakesReceiverFieldAddress must not fire (no explicit `&`)")
	}

	// Negative control — pointer field: no receiver-field address is taken (the pointer is the box).
	if body, recv, obj := find("ThroughPtr"); bodyTakesImplicitReceiverFieldAddress(body, recv, obj, info) {
		t.Error("ThroughPtr: `o.ptr.Bump()` takes no receiver-field address (ptr is already a pointer)")
	}

	// Control — bare receiver return: neither predicate fires.
	if body, recv, obj := find("Bare"); bodyTakesReceiverFieldAddress(body, recv, obj, info) || bodyTakesImplicitReceiverFieldAddress(body, recv, obj, info) {
		t.Error("Bare: no receiver-field-address predicate should fire on `return o`")
	}
}
