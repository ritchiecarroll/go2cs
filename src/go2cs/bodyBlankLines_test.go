// bodyBlankLines_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards Go's blank lines between the statements of a body in `-comments` output (class A of
// docs/PLAN-comment-and-spacing-parity.md, the largest single class in the corpus census).
//
// A Go author separates the steps of a function with a blank line, and gofmt keeps at most one. The
// converted body must keep each one, in the same place: between two statements, before a comment
// that leads a statement, between two comment groups, and between a comment and the statement after
// it, in block bodies and in case, type-switch case and select bodies alike. It must not invent one:
// a body with no blank line in Go has none in C#, and a three-line comment is three lines, not a
// comment with blank lines around it.
//
// The blank lines are kept only under `-comments` (the plan's decision 1): without it the emission is
// unchanged, which keeps every behavioral golden (captured without `-comments`) where it is.

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

const bodyBlankLinesSource = `package main

import "fmt"

func steps() int {
	a := 1

	b := a + 1
	return b
}

func commentLed() int {
	x := 1

	// why y follows
	y := x
	return y
}

func commentGroups() int {
	p := 1
	// first group

	// second group
	q := p
	return q
}

func commentThenGap() int {
	m := 1
	// lead

	n := m
	return n
}

func threeLineComment() int {
	c := 1
	// one
	// two
	// three
	d := c
	return d
}

func afterSwitch(v int) int {
	switch {
	case v > 1:
		v--
	case v < 0:
		v++
	}

	w := v * 2
	return w
}

func switchNoGap(v, w int) int {
	switch v {
	case w:
		v--
	}
	u := v * 3
	switch {
	case u > 1:
		u--
	}
	t := u * 5
	return t
}

func switchLast(v int) (n int) {
	if v > 0 {
		switch m := v * 2; m {
		case 2:
			n = m
		}
	}
	for i := 0; i < v; i++ {
		switch {
		case i > 1:
			n++
		}
	}
	return n
}

func deferredReturn() (n int) {
	defer func() { n++ }()
	n = 1

	return
}

func localType() int {
	type pair struct{ a, b int }

	p := pair{1, 2}
	return p.a + p.b
}

func caseBody(v int) {
	switch v {
	case 1:
		// opens the case body
		fmt.Println("case one")

		fmt.Println("after the case gap")
	default:
		fmt.Println("default")
	}
}

func defaultFirst(v, w int) int {
	n := 0
	switch v {
	default:
		// default body first
		n = 22
	case w:
		// case one first
		n = 11
	}
	return n
}

func add(s string, k int) int {
	return len(s) + k
}

func emptyClause(v, w string) int {
	n := 0
	switch v {
	case w:
		// empty clause body
	case "x":
		n = add(w, 33)
	case "y":
		n = 44
	// above case z
	case "z":
		n = add(w, 55)
	}
	return n
}

func defaultInner(v, w int) int {
	n := 0
	switch v {
	default:
		if v > 100 {
			// inside the default body
			n = 1
		}
	case w:
		n = add(fmt.Sprint(w), 66)
	case w + 1: // on the label
		// below the label
		n = add(fmt.Sprint(w), 77)
	}
	return n
}

func defaultThenLabel(v, w int) int {
	n := 0
	switch v {
	default:
		n = 3

	// above case w
	case w:
		n = add(fmt.Sprint(w), 88)
	}
	return n
}

func typeSwitchBody(v any) {
	switch t := v.(type) {
	case string:
		fmt.Println("string", t)

		fmt.Println("after the type-switch gap")
	}
}

func selectBody(ch chan int) {
	select {
	case n := <-ch:
		fmt.Println("received", n)

		fmt.Println("after the select gap")
	default:
	}
}

func main() {
	fmt.Println(steps(), commentLed(), commentGroups(), commentThenGap(), threeLineComment(), afterSwitch(2), deferredReturn(), localType(), switchNoGap(1, 1), switchLast(2))
	caseBody(1)
	fmt.Println(defaultFirst(1, 1))
	fmt.Println(emptyClause("x", "w"))
	fmt.Println(defaultInner(1, 1))
	fmt.Println(defaultThenLabel(1, 1))
	typeSwitchBody(1)
	selectBody(make(chan int))
}
`

// lineContaining returns the index of the single emitted line containing text, failing if it is
// missing or repeated. A string literal is hoisted to a static field, so a call is found by the
// field's name (`afterTheCaseGapˢ);`), not by the literal's text, which sits on the hoist line.
func lineContaining(t *testing.T, lines []string, text string) int {
	t.Helper()

	found := -1

	for i, line := range lines {
		if strings.Contains(line, text) {
			if found >= 0 {
				t.Fatalf("%q emitted on two lines (%d and %d)", text, found+1, i+1)
			}
			found = i
		}
	}

	if found < 0 {
		t.Fatalf("%q not emitted:\n%s", text, strings.Join(lines, "\n"))
	}

	return found
}

// blankAbove reports whether the emitted line containing text has a blank line directly above it.
func blankAbove(t *testing.T, lines []string, text string) bool {
	t.Helper()

	i := lineContaining(t, lines, text)

	return i > 0 && strings.TrimSpace(lines[i-1]) == ""
}

func TestBodyBlankLinesKeptUnderComments(t *testing.T) {
	lines := convertWithComments(t, bodyBlankLinesSource)

	for _, c := range []struct {
		line  string
		blank bool
		why   string
	}{
		{"nint b = a + 1;", true, "a blank line between two statements"},
		{"return b;", false, "no blank line in Go"},
		{"// why y follows", true, "a blank line before a comment that leads a statement"},
		{"nint y = x;", false, "a leading comment stays on the statement"},
		{"// first group", false, "no blank line in Go"},
		{"// second group", true, "a blank line between two comment groups"},
		{"nint q = p;", false, "a leading comment stays on the statement"},
		{"// lead", false, "no blank line in Go"},
		{"nint n = m;", true, "a blank line between a comment and the statement after it"},
		{"// one", false, "no blank line in Go"},
		{"// two", false, "a three-line comment gains no blank line"},
		{"// three", false, "a three-line comment gains no blank line"},
		{"nint d = c;", false, "a three-line comment gains no blank line"},
		{"nint w = v * 2;", true, "a blank line after a lowered switch"},
		{"nint u = v * 3;", false, "no blank line after a lowered switch Go writes adjacently"},
		{"nint t = u * 5;", false, "no blank line after a lowered switch Go writes adjacently"},
		{"afterTheCaseGapˢ);", true, "a blank line inside a case body"},
		{"// opens the case body", false, "a comment leading a case body stays above its first statement"},
		{"afterTheTypeSwitchGapˢ);", true, "a blank line inside a type-switch case body"},
		{"afterTheSelectGapˢ);", true, "a blank line inside a select case body"},
	} {
		if got := blankAbove(t, lines, c.line); got != c.blank {
			t.Errorf("%s: blank line above %q = %v, want %v", c.why, c.line, got, c.blank)
		}
	}

	if next := lineContaining(t, lines, "// opens the case body") + 1; !strings.Contains(lines[next], "caseOneˢ);") {
		t.Errorf("the comment leading a case body is not directly above its first statement: next line %q", strings.TrimSpace(lines[next]))
	}

	// A switch on a non-constant case is lowered to if/else, which emits a leading `default:` clause
	// last; its leading comment must stay in its own body, not be flushed into the clause emitted first.
	for _, pair := range [][2]string{{"// default body first", "n = 22;"}, {"// case one first", "n = 11;"}} {
		if next := lineContaining(t, lines, pair[0]) + 1; !strings.Contains(lines[next], pair[1]) {
			t.Errorf("%q is not directly above %q: next line %q", pair[0], pair[1], strings.TrimSpace(lines[next]))
		}
	}

	// A comment that is an empty clause's whole body stays in that clause's block, and a comment above
	// a case label is written above its clause's first statement; neither may wait for the next flush,
	// which writes it inside that statement's argument list.
	if at := lineContaining(t, lines, "// empty clause body"); !strings.HasSuffix(strings.TrimSpace(lines[at-1]), "{") || !strings.HasPrefix(strings.TrimSpace(lines[at+1]), "}") {
		t.Errorf("the comment that is an empty clause's body is not inside its block: %q / %q / %q", strings.TrimSpace(lines[at-1]), strings.TrimSpace(lines[at]), strings.TrimSpace(lines[at+1]))
	}

	for _, pair := range [][2]string{{"// above case z", "add(w, 55)"}} {
		if next := lineContaining(t, lines, pair[0]) + 1; !strings.Contains(lines[next], pair[1]) {
			t.Errorf("%q is not directly above %q: next line %q", pair[0], pair[1], strings.TrimSpace(lines[next]))
		}
	}

	// A comment inside a `default:` clause the lowering moves to the end waits for that clause, and a
	// comment trailing a case label is written with its clause: a flush in a clause emitted earlier
	// must not write either inside an argument list.
	if next := lineContaining(t, lines, "// inside the default body") + 1; !strings.Contains(lines[next], "n = 1;") {
		t.Errorf("the comment inside the moved default body is not above its statement: next line %q", strings.TrimSpace(lines[next]))
	}

	// A comment above the case label that follows a moved `default:` belongs to that case, not to the
	// default: holding it with the default wrote it after the whole switch.
	if next := lineContaining(t, lines, "// above case w") + 1; !strings.Contains(lines[next], "fmt.Sprint(w), 88);") {
		t.Errorf("the comment above a case after a moved default is not above that case's statement: next line %q", strings.TrimSpace(lines[next]))
	}

	for _, call := range []string{"fmt.Sprint(w), 66);", "fmt.Sprint(w), 77);"} {
		if line := lines[lineContaining(t, lines, call)]; strings.Contains(line, "//") {
			t.Errorf("a comment was written inside %q: %q", call, strings.TrimSpace(line))
		}
	}

	for _, call := range []string{"add(w, 33)", "add(w, 55)"} {
		if line := lines[lineContaining(t, lines, call)]; strings.Contains(line, "//") {
			t.Errorf("a comment was written inside %q: %q", call, strings.TrimSpace(line))
		}
	}

	// gofmt never leaves two blank lines in a row, a blank line right after `{` or one right before `}`,
	// and neither may a body here. A statement that ends with its own trailing line break (a lowered
	// switch) must not gain a second blank line; a gap ahead of a statement that emits nothing (a bare
	// `return` carried by the deferred-call lowering) must not be left before the closing brace; and a gap
	// after statements that emit nothing (a local type declaration, lifted out) must not open the body.
	for i := 1; i+1 < len(lines); i++ {
		if strings.TrimSpace(lines[i]) != "" {
			continue
		}

		prev, next := strings.TrimSpace(lines[i-1]), strings.TrimSpace(lines[i+1])

		switch {
		case prev == "" && strings.HasPrefix(lines[i+1], "    "):
			t.Errorf("two blank lines in a row above line %d (%q)", i+2, next)
		case strings.HasSuffix(prev, "{") && strings.HasPrefix(lines[i+1], "    "):
			t.Errorf("a blank line right after %q (line %d)", prev, i)
		case strings.HasPrefix(lines[i+1], "    ") && strings.HasPrefix(next, "}"):
			t.Errorf("a blank line right before %q (line %d), after %q", next, i+2, prev)
		}
	}
}

func TestBodyBlankLinesNotAddedWithoutComments(t *testing.T) {
	root := t.TempDir()
	pkgDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(pkgDir, "go.mod"), "module example.com/app\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(pkgDir, "main.go"), bodyBlankLinesSource)

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{
		goRoot:              goRoot,
		goPath:              build.Default.GOPATH,
		go2csPath:           filepath.Join(root, "runtime"),
		targetPlatform:      runtime.GOOS + "/" + runtime.GOARCH,
		indentSpaces:        4,
		preferVarDecl:       true,
		useChannelOperators: true,
	}

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	outDir := filepath.Join(root, "out")

	if err := processConversion(pkgDir, true, outDir, options); err != nil {
		t.Fatalf("conversion failed: %v", err)
	}

	lines := strings.Split(strings.ReplaceAll(readGenerated(t, filepath.Join(outDir, "main.cs")), "\r", ""), "\n")

	for _, text := range []string{"nint b = a + 1;", "nint n = m;", "afterTheCaseGapˢ);", "afterTheTypeSwitchGapˢ);", "afterTheSelectGapˢ);"} {
		if blankAbove(t, lines, text) {
			t.Errorf("without -comments, a blank line was added above %q; the emission (and every behavioral golden) must not move", text)
		}
	}
}
