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
// The blank lines are kept in every conversion (the owner's decision 1). Without `-comments` the
// comments are not written, so a blank line is kept when any line between two statements is blank in Go;
// a line a comment covers is not blank.

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

func braceComments(v int) int {
	n := 0
	if v > 0 { // after the if brace
		n = 1
	}
	switch v {
	case 1: // on the case label
		n = 2
	// above case two
	case 2: // on case two
		n = 3
	}
	for i := 0; i < v; i++ {
		n += i
		// before the loop's closing brace
	}
	return n
}

func emptyAndEnds(v int) int {
	n := 0
	if v > 5 {
		// nothing to do
	} else {
		n = 9
	}
	switch v {
	case 1:
		n = 2
		// end of case one
	case 2:
		n = 3
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
	fmt.Println(braceComments(2))
	fmt.Println(emptyAndEnds(1))
	typeSwitchBody(1)
	selectBody(make(chan int))
}
`

// lineContaining returns the index of the single emitted line containing text, failing if it is
// missing or repeated. An fmt.Println literal is a bare C# literal at its call (go2cs-gen intercepts the call to
// box it), so a call is found by its literal's text and closing parenthesis (`"after the case gap");`).
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
	isolateConverterPathGlobals(t)
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
		{`"after the case gap");`, true, "a blank line inside a case body"},
		{"// opens the case body", false, "a comment leading a case body stays above its first statement"},
		{`"after the type-switch gap");`, true, "a blank line inside a type-switch case body"},
		{`"after the select gap");`, true, "a blank line inside a select case body"},
	} {
		if got := blankAbove(t, lines, c.line); got != c.blank {
			t.Errorf("%s: blank line above %q = %v, want %v", c.why, c.line, got, c.blank)
		}
	}

	if next := lineContaining(t, lines, "// opens the case body") + 1; !strings.Contains(lines[next], `"case one");`) {
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
	// a case label is written above that clause's header; neither may wait for the next flush, which
	// writes it inside a statement's argument list.
	// (A C# switch clause closes with the `break;` its block needs, so the closing brace may follow it.)
	if at := lineContaining(t, lines, "// empty clause body"); !strings.HasSuffix(strings.TrimSpace(lines[at-1]), "{") ||
		!strings.HasPrefix(strings.TrimSpace(lines[at+1]), "}") &&
			!(strings.TrimSpace(lines[at+1]) == "break;" && strings.HasPrefix(strings.TrimSpace(lines[at+2]), "}")) {
		t.Errorf("the comment that is an empty clause's body is not inside its block: %q / %q / %q", strings.TrimSpace(lines[at-1]), strings.TrimSpace(lines[at]), strings.TrimSpace(lines[at+1]))
	}

	// The clause header reads `if (… == "z"u8)` in the if-chain and `case … == "z"u8:` in a switch.
	for _, pair := range [][2]string{{"// above case z", "== \"z\"u8"}} {
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
	if next := lineContaining(t, lines, "// above case w") + 1; !strings.Contains(lines[next], "== w)") && !strings.Contains(lines[next], "== w:") {
		t.Errorf("the comment above a case after a moved default is not above that case's header: next line %q", strings.TrimSpace(lines[next]))
	}

	// Class H: a comment after `{` or after `case X:` on the same line trails that line, and a comment
	// after a block's last statement stays above its closing brace.
	for _, c := range []struct{ comment, header string }{
		{"// after the if brace", "if (v > 0) {"},
		{"// on the case label", "case 1:"},
		{"// on case two", "case 2:"},
	} {
		if line := lines[lineContaining(t, lines, c.comment)]; !strings.Contains(line, c.header) {
			t.Errorf("%q does not trail %q: %q", c.comment, c.header, strings.TrimSpace(line))
		}
	}

	// The rest of class H: a comment above a case label stays above that label's header; a comment
	// alone in an empty block stays in that block; a comment after a case body's last statement stays
	// at the end of that body.
	if next := lineContaining(t, lines, "// above case two") + 1; !strings.Contains(lines[next], "case 2:") {
		t.Errorf("the comment above a case label is not above its header: next line %q", strings.TrimSpace(lines[next]))
	}

	if at := lineContaining(t, lines, "// nothing to do"); !strings.Contains(lines[at-1], "if (v > 5)") || !strings.HasPrefix(strings.TrimSpace(lines[at+1]), "}") {
		t.Errorf("the comment alone in an empty block is not inside it: %q / %q / %q", strings.TrimSpace(lines[at-1]), strings.TrimSpace(lines[at]), strings.TrimSpace(lines[at+1]))
	}

	if at := lineContaining(t, lines, "// end of case one"); !strings.Contains(lines[at-1], "n = 2;") {
		t.Errorf("the comment at the end of a case body does not follow its last statement: %q above it", strings.TrimSpace(lines[at-1]))
	}

	if at := lineContaining(t, lines, "// before the loop's closing brace"); !strings.Contains(lines[at-1], "n += i;") || strings.TrimSpace(lines[at+1]) != "}" {
		t.Errorf("the comment before a closing brace is not between the last statement and the brace: %q / %q / %q", strings.TrimSpace(lines[at-1]), strings.TrimSpace(lines[at]), strings.TrimSpace(lines[at+1]))
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

func TestBodyBlankLinesKeptWithoutComments(t *testing.T) {
	isolateConverterPathGlobals(t)
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

	for _, c := range []struct {
		line  string
		blank bool
		why   string
	}{
		{"nint b = a + 1;", true, "a blank line between two statements"},
		{"return b;", false, "no blank line in Go"},
		{"nint y = x;", true, "a blank line, then a comment not written"},
		{"nint q = p;", true, "a blank line between two comment groups not written"},
		{"nint n = m;", true, "a comment not written, then a blank line"},
		{"nint d = c;", false, "a three-line comment not written is no blank line"},
		// A lowered switch keeps its trailing line break without -comments (class E1 trims it under
		// -comments only), so the statement after it has a blank line above whether Go has one or not; the
		// gap adds no second one.
		{"nint w = v * 2;", true, "a blank line after a lowered switch"},
		{`"after the case gap");`, true, "a blank line inside a case body"},
		{`"after the type-switch gap");`, true, "a blank line inside a type-switch case body"},
		{`"after the select gap");`, true, "a blank line inside a select case body"},
	} {
		if got := blankAbove(t, lines, c.line); got != c.blank {
			t.Errorf("without -comments, blank line above %q = %v, want %v (%s)", c.line, got, c.blank, c.why)
		}
	}

	for i, line := range lines {
		if i > 0 && strings.TrimSpace(line) == "" && strings.TrimSpace(lines[i-1]) == "" {
			t.Errorf("without -comments, lines %d-%d are two blank lines in a row", i, i+1)
		}

		if strings.Contains(line, "// why y follows") || strings.Contains(line, "// first group") || strings.Contains(line, "// one") {
			t.Errorf("without -comments, line %d writes a Go comment: %q", i+1, line)
		}
	}
}

// Without `-comments`, some statements open with a line of their own that is blank or holds only
// indentation: one Go leads with a comment that is not written, a variable initialized by a func
// literal. Where Go has a blank line above such a statement, that line already is the blank line, and
// the gap must not add a second one (gofmt never writes two in a row).
const doubledGapSource = `package main

import "fmt"

type inner struct{ n int }

func (i *inner) Add(d int) int { i.n += d; return i.n }

type outer struct{ inner }

func check(name string, f func()) {
	defer func() { fmt.Println(name, recover() != nil) }()
	f()
}

func main() {
	lv := 1
	fmt.Println("lv", lv)

	// Method expression of a promoted method.
	add := (*outer).Add
	var o outer
	fmt.Println(add(&o, 2))

	var s any = []int{1, 2}
	check("slice", func() { _ = s == s })

	var fn any = func() {}
	check("func", func() { _ = fn == fn })
}
`

func TestBodyBlankLinesNotDoubledWithoutComments(t *testing.T) {
	isolateConverterPathGlobals(t)
	lines := convertWithoutComments(t, doubledGapSource)

	for i := 1; i < len(lines); i++ {
		if strings.TrimSpace(lines[i]) == "" && strings.TrimSpace(lines[i-1]) == "" {
			t.Errorf("lines %d-%d are two blank lines in a row:\n%s", i, i+1, strings.Join(lines[max(0, i-4):min(len(lines), i+3)], "\n"))
		}
	}

	for _, text := range []string{"var add = ", "any fn = "} {
		if !blankAbove(t, lines, text) {
			t.Errorf("no blank line above %q, where Go has one", text)
		}
	}
}

// convertWithoutComments converts a single-file package with `-comments` off and returns the emitted
// C# split into lines.
func convertWithoutComments(t *testing.T, source string) []string {
	t.Helper()

	root := t.TempDir()
	pkgDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(pkgDir, "go.mod"), "module example.com/app\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(pkgDir, "main.go"), source)

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

	return strings.Split(strings.ReplaceAll(readGenerated(t, filepath.Join(outDir, "main.cs")), "\r", ""), "\n")
}

// A comment that documents a var or const statement inside a body is that statement's doc, written with it rather
// than flushed as a standalone comment ahead of it, so the gap test must not count its lines as blank ones.
// math/big's nat.expNN (`w := _W - int(shift)` / `// zz and r are used ...` / `var zz, r nat`) gained a blank line
// Go does not have before such a comment, at 82 corpus sites under -comments (TRAIN T4's census re-baseline).
const declDocGapSource = `package main

import "fmt"

func adjacent(shift int) int {
	w := 64 - shift
	// zz and r are used to avoid allocating
	// otherwise the arguments would alias.
	var zz, r int
	const k = 2
	// the padding length
	const pad = 16
	return w + zz + r + k + pad
}

func spaced(shift int) int {
	w := 64 - shift

	// keep the blank line Go has
	var v int
	return w + v
}

func afterBlock(x bool) int {
	if x {
		fmt.Println("x")
	}
	// Read the data
	n := 2
	return n
}

func afterStatement(n int) []byte {
	m := n + 1
	// Allocate new buffer.
	buf := make([]byte, m)
	return buf
}

func main() {
	fmt.Println(adjacent(1), spaced(1), afterBlock(true), len(afterStatement(1)))
}
`

func TestNoBlankLineBeforeDeclStatementDoc(t *testing.T) {
	isolateConverterPathGlobals(t)
	lines := convertWithComments(t, declDocGapSource)

	for _, c := range []struct {
		line  string
		blank bool
		why   string
	}{
		{"// zz and r are used to avoid allocating", false, "Go writes the var statement's doc directly after the statement before it"},
		{"// the padding length", false, "Go writes the const statement's doc directly after the statement before it"},
		{"// keep the blank line Go has", true, "a blank line Go has before a var statement's doc stays"},
		{"// Read the data", false, "control: a comment leading `n := 2` after a block gains no blank line"},
		{"// Allocate new buffer.", false, "control: a comment leading `buf := make(...)` gains no blank line"},
	} {
		if got := blankAbove(t, lines, c.line); got != c.blank {
			t.Errorf("%q: blank line above = %v, want %v (%s)", c.line, got, c.blank, c.why)
		}
	}
}
