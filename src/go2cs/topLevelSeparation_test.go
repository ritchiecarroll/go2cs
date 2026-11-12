// topLevelSeparation_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards class K of docs/PLAN-comment-and-spacing-parity.md, as the owner ruled it on 2026-11-05
// (docs/phase4/CENSUS-comment-parity-K-scout.md):
//
//   - K1 (b): a blank line between two top-level declarations only where Go has one, except that a
//     function Go writes on one line and C# cannot keeps a blank line on each side;
//   - K2 (a): a function Go writes on one line is one line in C#, a block body `{ return t.a; }`, and an
//     empty one is `{ }`;
//   - every conversion, with or without `-comments`.

package main

import (
	"strings"
	"testing"
)

const topLevelSeparationSource = `package main

import "fmt"

type T struct{ a, b int }

type U int
type V int

const A = 1
const B = 2

var X = 3
var Y = 4

// Pa is documented.
func (t *T) Pa() int { return t.a }
func (t *T) Pb() int { return t.b }

func (t *T) C() {
	t.a++
}
func F() {}
func G() int { x := 1; return x }
func H(p *T) { p.a = 1 }
func K() int { return 7 } // a trailing note
func M() int {
	return 8
}
func N() int {
	return 9
}
func O() {}

func main() {
	t := &T{}
	t.C()
	F()
	H(t)
	fmt.Println(t.Pa(), t.Pb(), A, B, X, Y, G(), K(), M(), N(), U(1), V(2))
}
`

// topLevelSeparationCases are the blank lines the source above must keep or lose, in both conversions.
var topLevelSeparationCases = []struct {
	line  string
	blank bool
	why   string
}{
	{"partial struct U", true, "Go has a blank line after T"},
	{"partial struct V", false, "Go writes U and V adjacent"},
	{"A => 1", true, "Go has a blank line before the consts"},
	{"B => 2", false, "Go writes the consts adjacent"},
	{"nint X = 3", true, "Go has a blank line before the vars"},
	{"nint Y = 4", false, "Go writes the vars adjacent"},
	{"Pb(this ref T t) { return t.b; }", false, "adjacent one-line methods stay adjacent"},
	{"void C(this ref T t) {", true, "Go has a blank line before C"},
	{"void F() { }", true, "C stands in T's holder and F in the package class: a class switch stands between two blank lines"},
	{"nint G() {", true, "G is one line in Go but two statements in C#, so it keeps a blank line"},
	{"void H(", true, "H is one line in Go but has a prelude in C#, so it keeps a blank line"},
	{"nint K() { return 7; }", true, "K follows H, which keeps its blank line"},
	{"nint M() {", false, "Go writes M under K"},
	{"nint N() {", false, "Go writes two multi-line functions adjacent"},
	{"void O() { }", false, "Go writes O under N's closing brace"},
	{"void Main() {", true, "Go has a blank line before main"},
}

func checkTopLevelSeparation(t *testing.T, lines []string) {
	t.Helper()

	for _, c := range topLevelSeparationCases {
		if got := blankAbove(t, lines, c.line); got != c.blank {
			t.Errorf("blank line above %q: got %v, want %v (%s)", c.line, got, c.blank, c.why)
		}
	}

	// K2: the one-line functions are one line, and the ones C# cannot write on one line are not.
	for _, one := range []string{
		"public static nint Pa(this ref T t) { return t.a; }",
		"public static nint Pb(this ref T t) { return t.b; }",
		"public static void F() { }",
	} {
		if i := lineContaining(t, lines, one); strings.TrimSpace(lines[i]) != one {
			t.Errorf("one-line function: got %q, want %q", strings.TrimSpace(lines[i]), one)
		}
	}

	for _, multi := range []string{"nint G() {", "void H("} {
		if i := lineContaining(t, lines, multi); strings.HasSuffix(strings.TrimSpace(lines[i]), "}") {
			t.Errorf("%q was put on one line, but its C# body has more than one statement: %q", multi, lines[i])
		}
	}
}

func TestTopLevelSeparationWithoutComments(t *testing.T) {
	isolateConverterPathGlobals(t)
	checkTopLevelSeparation(t, convertWithoutComments(t, topLevelSeparationSource))
}

func TestTopLevelSeparationUnderComments(t *testing.T) {
	isolateConverterPathGlobals(t)
	lines := convertWithComments(t, topLevelSeparationSource)
	checkTopLevelSeparation(t, lines)

	// The doc stays on its function, which stays one line, and Go's blank line stays above the doc.
	if !blankAbove(t, lines, "// Pa is documented.") {
		t.Errorf("Go's blank line above Pa's doc is missing")
	}

	if blankAbove(t, lines, "Pa(this ref T t) { return t.a; }") {
		t.Errorf("a blank line separates Pa from its doc")
	}

	// A trailing comment follows the one-line body on its line.
	if i := lineContaining(t, lines, "// a trailing note"); !strings.Contains(lines[i], "K() { return 7; }") {
		t.Errorf("K's trailing comment is not on K's one line: %q", lines[i])
	}
}
