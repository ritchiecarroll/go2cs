// rangeNamedIntType_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// RANGE-OVER-INT KEEPS THE OPERAND'S NAMED TYPE. Go gives the loop variable of `for i := range n`
// the type of n, so `for days := range absDays(1e6)` gives `days` the type `absDays` and every
// method on that type must bind. The emission cast the operand down to its underlying width and
// named that width as range<T>'s type argument, so the variable came out a bare `ulong`: time's
// abs_test.go calls `.split()`, `.date()` and `.yearYday()` on it, and every one failed CS1929
// with three CS8130 deconstructions downstream — six errors, one root.
//
// ⚠ THE CONSTRAINT ADMITS THE NAMED TYPE, which is what makes the type argument the right fix
// rather than a declared loop-variable type plus a cast. range<T>'s bound is
// `struct, IComparisonOperators<T, T, bool>, IIncrementOperators<T>` — the operator pair the loop
// body uses, narrowed to exactly that so golib's hand-written `uintptr` could bind (builtin.cs
// says so in its own remark) — and a converted `[GoType("num:…")]` type declares both from
// InheritedTypeTemplate. So the named type needs nothing added to it, and the emission reads like
// the Go instead of spelling two conversions Go does not have.
//
// ⚠ SCOPE, and the controls that hold it: a named type over plain `int` ALREADY emits the bare
// `range(expr)` form, where C#'s inference binds T to the named type by itself — right today, and
// naming the argument there would move corpus bytes for nothing. An UNNAMED non-`int` width keeps
// the underlying-width argument it always had. Both are armed below, and both were green before
// this change.
package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// convertNamedIntRangeFixture converts the one fixture every arm reads and returns its emitted C#.
func convertNamedIntRangeFixture(t *testing.T) string {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/namedintrange\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), `package main

// THE SUBJECT: time's absDays shape — a named type over a non-`+"`int`"+` width, with methods.
type absDays uint64

func (d absDays) split() (absDays, uint64) { return d, uint64(d) }

func (d absDays) yearYday() (int, int) { return int(d), int(d) }

// A named type over plain int: the CONTROL that must keep the bare, inferred form.
type plainNamed int

func (p plainNamed) doubled() plainNamed { return p * 2 }

func namedWidthRange() uint64 {
	var total uint64

	// The exact time shape: a named non-int width, an UNTYPED CONSTANT operand, and a method call
	// on the loop variable — the call is what CS1929'd when the variable lost its type.
	for days := range absDays(1e6) {
		whole, rest := days.split()
		total += uint64(whole) + rest
	}

	return total
}

func namedWidthRangeOverVariable(limit absDays) int {
	sum := 0

	// The same, over a VARIABLE rather than a conversion of a constant.
	for days := range limit {
		year, _ := days.yearYday()
		sum += year
	}

	return sum
}

func plainNamedRange() plainNamed {
	var total plainNamed

	for step := range plainNamed(4) {
		total += step.doubled()
	}

	return total
}

func unnamedWidthRange(limit uint64) uint64 {
	var total uint64

	for i := range limit {
		total += i
	}

	return total
}

func plainIntRange(limit int) int {
	total := 0

	for i := range limit {
		total += i
	}

	return total
}

func main() {
	println(namedWidthRange(), namedWidthRangeOverVariable(3), int(plainNamedRange()), unnamedWidthRange(2), plainIntRange(2))
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

	return readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "namedintrange", "main.cs"))
}

// ARM 1, THE SUBJECT. A named non-`int` width is its OWN type argument and the operand is passed
// through unchanged — no cast down to the underlying width, which is what erased the type.
func TestRangeOverANamedIntWidthKeepsTheNamedType(t *testing.T) {
	isolateConverterPathGlobals(t)
	emitted := convertNamedIntRangeFixture(t)

	// The operand's own rendering is a pre-existing convention and not this seat's axis: a Go
	// conversion of an untyped constant folds and emits as a C# cast. What is asserted is the TYPE
	// ARGUMENT, and it is asserted against the spelling the converter actually produces.
	if !strings.Contains(emitted, "range<absDays>(((absDays)1000000))") {
		t.Errorf("the untyped-constant operand must range as range<absDays>; emitted:\n%s", emitted)
	}

	if !strings.Contains(emitted, "range<absDays>(limit)") {
		t.Errorf("a variable operand must range as range<absDays>(limit); emitted:\n%s", emitted)
	}

	// THE DEFECT'S OWN SPELLING: the underlying width as the argument, with the operand cast down.
	// THE DEFECT'S OWN SHAPE: the underlying width as the argument with the operand cast down to
	// it. Tested as a substring of the CAST, so any operand rendering that reaches it still reds.
	if strings.Contains(emitted, "range<uint64>((uint64)") {
		t.Errorf("the operand was cast down to its underlying width, which erases the named type; emitted:\n%s", emitted)
	}
}

// ARM 2, THE CONTROLS. A named type over plain `int` keeps the bare inferred form; an UNNAMED
// non-`int` width keeps the underlying-width argument; a plain `int` keeps the bare form. All
// three were green before this change and must stay byte-identical.
func TestRangeEmissionIsUnmovedOutsideTheNamedWidthCase(t *testing.T) {
	isolateConverterPathGlobals(t)
	emitted := convertNamedIntRangeFixture(t)

	if !strings.Contains(emitted, "range(((plainNamed)4))") {
		t.Errorf("a named type over plain int keeps the BARE form — C# infers T from the operand; emitted:\n%s", emitted)
	}

	if strings.Contains(emitted, "range<plainNamed>(") {
		t.Errorf("naming the argument for a named int32 moves corpus bytes for nothing; emitted:\n%s", emitted)
	}

	if !strings.Contains(emitted, "range<uint64>(limit)") {
		t.Errorf("an UNNAMED non-int width keeps its underlying-width argument; emitted:\n%s", emitted)
	}
}
