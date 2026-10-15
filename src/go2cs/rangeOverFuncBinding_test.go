// rangeOverFuncBinding_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the two properties a RANGE-OVER-FUNC loop and a GENERIC named-delegate parameter need,
// both surfaced by internal/synctest's iterator tests and both scoped by controls that must keep
// today's emission:
//
//  1. PER-ITERATION BOX (visitRangeStmt's deferRangeVarBox). A heap-boxed yielded variable is
//     declared by the foreach itself, so emitting its box at the loop's own scope declared the
//     name TWICE — CS0136. The box belongs inside the body, fresh each pass, with the foreach
//     iterating a temp. Controls: an UNBOXED yielded variable keeps the bare `foreach (var now in
//     range(seq))` with no temp at all, and a range over a SLICE is untouched.
//
//  2. INSTANTIATED DELEGATE PARAM (convCallExpr's named func-type argument rule). A value bound
//     to a GENERIC named func-type parameter — `iter.Pull[V any](seq Seq[V])` — reached the rule
//     as `Seq[V]` with V unbound and was left bare, so C# could infer no type argument (CS0411)
//     and the result pair would not deconstruct (CS0029). The instantiation go/types recorded
//     supplies the concrete `Seq[time.Time]`. Controls: a func LITERAL argument converts natively
//     and stays bare, a value already OF the parameter's type stays bare, and a call whose
//     substituted parameter is still open stays bare.

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// convertRangeOverFuncFixture converts the one fixture both tests read and returns its emitted C#.
func convertRangeOverFuncFixture(t *testing.T) string {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/rangeoverfunc\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

import (
	"fmt"
	"iter"
)

type stamp struct{ n int }

// BOXED - the yielded variable lives in a goroutine body, where the converter heap-boxes it. The
// foreach declares the name, so the box must be emitted per iteration INSIDE the loop.
func boxedYield(seq func(yield func(stamp) bool)) []stamp {
	var got []stamp
	go func() {
		for now := range seq {
			got = append(got, now)
		}
	}()
	return got
}

// CONTROL - the identical loop OUTSIDE a goroutine takes no box, so it keeps the bare foreach.
func unboxedYield(seq func(yield func(stamp) bool)) []stamp {
	var got []stamp
	for now := range seq {
		got = append(got, now)
	}
	return got
}

// BOXED, TWO-VALUE - the same rule for a func(yield func(K, V) bool) range. BOTH yielded variables
// are boxed here (the key's address is taken), so both take the per-iteration form.
func boxedYieldPair(seq func(yield func(int, stamp) bool)) ([]*int, []stamp) {
	var idx []*int
	var got []stamp
	go func() {
		for i, now := range seq {
			now.n += i
			idx = append(idx, &i)
			got = append(got, now)
		}
	}()
	return idx, got
}

// CONTROL - the same two-value range with only the VALUE boxed: the key keeps the name the foreach
// declares and takes no temp, so the two positions are decided independently.
func boxedYieldPairValueOnly(seq func(yield func(int, stamp) bool)) []stamp {
	var got []stamp
	go func() {
		for i, now := range seq {
			now.n += i
			got = append(got, now)
		}
	}()
	return got
}

// CONTROL - a SLICE range in the same boxed position; its per-iteration box predates this rule and
// its emission must not move.
func boxedSliceRange(items []stamp) []*stamp {
	var got []*stamp
	go func() {
		for _, it := range items {
			got = append(got, &it)
		}
	}()
	return got
}

// INSTANTIATED - the argument's Go type is the STRUCTURAL func type; the parameter is the generic
// named iter.Seq[V]. Go converts implicitly; C# needs the delegate wrap, with V substituted.
func pullImplicit() stamp {
	seq := func(yield func(stamp) bool) { yield(stamp{1}) }
	next, stop := iter.Pull(seq)
	defer stop()
	s, _ := next()
	return s
}

// CONTROL - a func LITERAL passed directly converts natively and must stay bare.
func pullLiteral() stamp {
	next, stop := iter.Pull(iter.Seq[stamp](func(yield func(stamp) bool) { yield(stamp{2}) }))
	defer stop()
	s, _ := next()
	return s
}

// CONTROL - the value is ALREADY of the parameter's instantiated type, so there is nothing to wrap.
func pullTyped() stamp {
	var seq iter.Seq[stamp] = func(yield func(stamp) bool) { yield(stamp{3}) }
	next, stop := iter.Pull(seq)
	defer stop()
	s, _ := next()
	return s
}

// CONTROL - a GENERIC caller forwarding its OWN type parameter: the substituted parameter is still
// open, so the rule must decline and leave the native conversion.
func pullForwarded[T any](f func(yield func(T) bool)) T {
	next, stop := iter.Pull(f)
	defer stop()
	v, _ := next()
	return v
}

func main() {
	idx, pair := boxedYieldPair(nil)
	fmt.Println(boxedYield(nil), unboxedYield(nil), idx, pair, boxedYieldPairValueOnly(nil), boxedSliceRange(nil))
	fmt.Println(pullImplicit(), pullLiteral(), pullTyped(), pullForwarded[stamp](nil))
}
`)

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{
		goRoot:              goRoot,
		goPath:              build.Default.GOPATH,
		go2csPath:           filepath.Join(root, "out"),
		recurse:             true,
		targetPlatform:      runtime.GOOS + "/" + runtime.GOARCH,
		indentSpaces:        4,
		preferVarDecl:       true,
		useChannelOperators: true,
	}

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	converter := NewModuleConverter(options)

	if err := converter.ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	return readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "rangeoverfunc", "main.cs"))
}

// emittedFuncBody returns the emitted lines of the named function, from its DECLARATION line to
// the next `static` declaration, so an assertion can be scoped to one function rather than to the
// whole file. The declaration is found by requiring `static` on the same line — a bare name match
// also finds the call sites in Main, and a generic function's name is followed by `<T>` rather
// than by its parameter list.
func emittedFuncBody(t *testing.T, mainCs, name string) string {
	t.Helper()

	lines := strings.Split(mainCs, "\n")
	start := -1

	for i, line := range lines {
		if !strings.Contains(line, "static ") {
			continue
		}

		if !strings.Contains(line, name+"(") && !strings.Contains(line, name+"<") {
			continue
		}

		start = i
		break
	}

	if start < 0 {
		t.Fatalf("emitted C# has no declaration of %s:\n%s", name, mainCs)
	}

	end := len(lines)

	for i := start + 1; i < len(lines); i++ {
		if strings.Contains(lines[i], "static ") {
			end = i
			break
		}
	}

	return strings.Join(lines[start:end], "\n")
}

// TestRangeOverFuncBoxesItsYieldedVariablePerIteration pins property 1 and its scope.
func TestRangeOverFuncBoxesItsYieldedVariablePerIteration(t *testing.T) {
	isolateConverterPathGlobals(t)
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	mainCs := convertRangeOverFuncFixture(t)
	temp := "i" + TempVarMarker + "1"

	boxed := emittedFuncBody(t, mainCs, "boxedYield")

	// The foreach iterates the temp; the box and the copy into it open the body.
	if !strings.Contains(boxed, "foreach (var "+temp+" in range(") {
		t.Errorf("boxedYield: expected the foreach to iterate %s:\n%s", temp, boxed)
	}

	boxDecl := "ref var now = ref heap(new stamp(), out var " + AddressPrefix + "now);"

	if !strings.Contains(boxed, boxDecl) {
		t.Errorf("boxedYield: expected the per-iteration box %q:\n%s", boxDecl, boxed)
	}

	// The box must follow the foreach — emitted before it, the name is declared twice (CS0136).
	if strings.Index(boxed, boxDecl) < strings.Index(boxed, "foreach (") {
		t.Errorf("boxedYield: the box is emitted at the loop's own scope, which re-declares `now` (CS0136):\n%s", boxed)
	}

	if strings.Contains(boxed, "foreach (var now in range(") {
		t.Errorf("boxedYield: the foreach still declares `now` alongside its box (CS0136):\n%s", boxed)
	}

	// CONTROL - no box, so no temp and no body prologue.
	unboxed := emittedFuncBody(t, mainCs, "unboxedYield")

	if !strings.Contains(unboxed, "foreach (var now in range(") {
		t.Errorf("unboxedYield: an unboxed yielded variable must keep the bare foreach:\n%s", unboxed)
	}

	if strings.Contains(unboxed, TempVarMarker) || strings.Contains(unboxed, "ref heap") {
		t.Errorf("unboxedYield: an unboxed yielded variable took a temp or a box it does not need:\n%s", unboxed)
	}

	// The TWO-VALUE form takes the same treatment, for each boxed variable independently.
	pair := emittedFuncBody(t, mainCs, "boxedYieldPair")
	valTemp := "v" + TempVarMarker + "1"

	if !strings.Contains(pair, "foreach (var ("+temp+", "+valTemp+") in range(") {
		t.Errorf("boxedYieldPair: expected the foreach to iterate (%s, %s):\n%s", temp, valTemp, pair)
	}

	for _, decl := range []string{
		"ref var i = ref heap(new nint(), out var " + AddressPrefix + "i);",
		"ref var now = ref heap(new stamp(), out var " + AddressPrefix + "now);",
	} {
		at := strings.Index(pair, decl)

		if at < 0 {
			t.Errorf("boxedYieldPair: expected the per-iteration box %q:\n%s", decl, pair)
			continue
		}

		if at < strings.Index(pair, "foreach (") {
			t.Errorf("boxedYieldPair: %q is emitted at the loop's own scope (CS0136):\n%s", decl, pair)
		}
	}

	// CONTROL - only the VALUE is boxed, so only it takes a temp; the key stays the declared name.
	valueOnly := emittedFuncBody(t, mainCs, "boxedYieldPairValueOnly")

	if !strings.Contains(valueOnly, "foreach (var (i, "+valTemp+") in range(") {
		t.Errorf("boxedYieldPairValueOnly: an unboxed key took a temp it does not need:\n%s", valueOnly)
	}

	// CONTROL - the slice arm's own per-iteration box, unchanged.
	sliceRange := emittedFuncBody(t, mainCs, "boxedSliceRange")

	if !strings.Contains(sliceRange, "ref var it = ref heap(new stamp(), out var "+AddressPrefix+"it);") {
		t.Errorf("boxedSliceRange: the slice arm's per-iteration box moved:\n%s", sliceRange)
	}
}

// TestGenericNamedDelegateParamWrapsWithItsInstantiation pins property 2 and its scope.
func TestGenericNamedDelegateParamWrapsWithItsInstantiation(t *testing.T) {
	isolateConverterPathGlobals(t)
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	mainCs := convertRangeOverFuncFixture(t)

	implicit := emittedFuncBody(t, mainCs, "pullImplicit")

	if !strings.Contains(implicit, "iter.Pull(new iter.Seq<stamp>(seq))") {
		t.Errorf("pullImplicit: expected the substituted delegate wrap `new iter.Seq<stamp>(seq)` (without it C# infers no type argument, CS0411):\n%s", implicit)
	}

	// CONTROL - a func literal converts natively; wrapping it would be redundant construction.
	literal := emittedFuncBody(t, mainCs, "pullLiteral")

	if strings.Contains(literal, "iter.Pull(new iter.Seq<stamp>(new ") {
		t.Errorf("pullLiteral: a func literal argument was double-wrapped:\n%s", literal)
	}

	// CONTROL - the value already has the parameter's type, so nothing is wrapped.
	typed := emittedFuncBody(t, mainCs, "pullTyped")

	if strings.Contains(typed, "new iter.Seq<stamp>(seq)") {
		t.Errorf("pullTyped: a value already of the parameter's type was wrapped anyway:\n%s", typed)
	}

	// CONTROL - the substituted parameter is still open (T), so the rule declines.
	forwarded := emittedFuncBody(t, mainCs, "pullForwarded")

	if strings.Contains(forwarded, "new iter.Seq<T>(") {
		t.Errorf("pullForwarded: an unsubstituted type parameter was rendered into a wrap:\n%s", forwarded)
	}
}
