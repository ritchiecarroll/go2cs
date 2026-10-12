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
		fmt.Println("case one")

		fmt.Println("after the case gap")
	default:
		fmt.Println("default")
	}
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
	fmt.Println(steps(), commentLed(), commentGroups(), commentThenGap(), threeLineComment(), afterSwitch(2), deferredReturn(), localType())
	caseBody(1)
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
		{"afterTheCaseGapˢ);", true, "a blank line inside a case body"},
		{"afterTheTypeSwitchGapˢ);", true, "a blank line inside a type-switch case body"},
		{"afterTheSelectGapˢ);", true, "a blank line inside a select case body"},
	} {
		if got := blankAbove(t, lines, c.line); got != c.blank {
			t.Errorf("%s: blank line above %q = %v, want %v", c.why, c.line, got, c.blank)
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
