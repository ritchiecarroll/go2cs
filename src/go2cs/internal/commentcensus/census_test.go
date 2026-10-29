// census_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// The census's own red test. testdata/planted holds one Go file and three C# renderings of it: `good` has Go's
// structure exactly, `bad` and `bad2` each plant a set of known defects (docs/PLAN-comment-and-spacing-parity.md
// §1 and Appendix B). The census must read `good` as clean and name every planted site in the other two,
// exactly once and nothing else. A census that cannot fail on bad or pass on good guards nothing, so this test
// is what makes the converter's parity tests and the corpus ratchet mean anything.

package commentcensus

import (
	"maps"
	"os"
	"slices"
	"strings"
	"testing"
)

func plantedCounts(t *testing.T, rendering string) (map[string]int, *Results) {
	t.Helper()

	pairs := FindPairs("testdata/planted/"+rendering, "testdata/planted/goroot")

	if len(pairs) != 1 {
		t.Fatalf("%s: %d pairs found, want 1 (plant/plant.cs against plant/plant.go)", rendering, len(pairs))
	}

	r := NewResults()

	if err := r.Analyze(pairs[0]); err != nil {
		t.Fatalf("%s: %v", rendering, err)
	}

	// Vacuity: the planted Go file has twelve comment groups; zero would mean nothing was read.
	if r.Groups() != 12 {
		t.Fatalf("%s: %d Go comment groups read, want 12", rendering, r.Groups())
	}

	return r.Counts(), r
}

func checkPlanted(t *testing.T, rendering string, want map[string]int) {
	t.Helper()

	got, r := plantedCounts(t, rendering)

	if !maps.Equal(got, want) {
		for _, k := range slices.Sorted(maps.Keys(got)) {
			if got[k] != want[k] {
				t.Errorf("%s: %s = %d, want %d", rendering, k, got[k], want[k])
			}
		}

		for _, k := range slices.Sorted(maps.Keys(want)) {
			if _, ok := got[k]; !ok {
				t.Errorf("%s: %s not reported, want %d", rendering, k, want[k])
			}
		}

		for _, f := range r.Findings("") {
			t.Logf("%s: %s at %s: %s", rendering, f.Class, f.Where, f.Detail)
		}
	}
}

func TestPlantedGoodIsClean(t *testing.T) {
	checkPlanted(t, "good", map[string]int{})
}

func TestPlantedBadNamesEachSite(t *testing.T) {
	checkPlanted(t, "bad", map[string]int{
		"missed:spec-doc-in-group":                        1, // the doc of C, inside the const group, dropped
		"misplaced:after-open-brace/trailing-to-own-line": 1, // `if x > 1 { // big` moved into the body
		// the row comment `1, // one` hoisted above the declaration: misplaced, and its aligned run broken
		"misplaced:in-composite-literal/trailing-to-own-line": 1,
		"eol:moved-to-own-line":                               1,
		// `A = 1 // first` / `B = 2 // second` aligned by gofmt, ragged in C#: read from both sides
		"eol:misaligned":         1,
		"eol:cs-ragged-run":      1,
		"blank2:member-boundary": 1, // two blank lines before H
		"top:go-blank-cs-none":   1, // the blank line above G dropped
		"group:go-blank-cs-none": 1, // the blank line above C's doc, inside the group, dropped
		"body:go-blank-cs-none":  1, // the blank line before `// second part` dropped
	})
}

func TestPlantedBad2NamesEachSite(t *testing.T) {
	checkPlanted(t, "bad2", map[string]int{
		"misplaced:free-stmt/anchor-differs":      1, // `// second part` moved to another statement
		"misplaced:decl-doc/own-line-to-trailing": 1, // G's doc pulled onto the previous `}`
		"eol:split":                     1, // the aligned A/B run split by a blank line
		"group:go-none-cs-blank":        1, // a blank line inserted inside the const group
		"body:extra:before-close-brace": 1, // a blank line before a closing brace
		"body:extra:unexplained":        1, // an extra blank line inside a body
	})
}

// anchorFindings returns the misplaced and missed findings for testdata/anchor/<rendering>.
func anchorFindings(t *testing.T, rendering string) map[string]int {
	t.Helper()

	pairs := FindPairs("testdata/anchor/"+rendering, "testdata/anchor/goroot")

	if len(pairs) != 1 {
		t.Fatalf("%s: %d pairs found, want 1 (anchor/anchor.cs against anchor/anchor.go)", rendering, len(pairs))
	}

	r := NewResults()

	if err := r.Analyze(pairs[0]); err != nil {
		t.Fatalf("%s: %v", rendering, err)
	}

	// Vacuity: the Go file has four comment groups past its header; fewer would mean they were not read.
	if r.Groups() < 4 {
		t.Fatalf("%s: %d Go comment groups read, want at least 4", rendering, r.Groups())
	}

	got := map[string]int{}

	for k, n := range r.Counts() {
		if strings.HasPrefix(k, "misplaced:") || strings.HasPrefix(k, "missed:") {
			got[k] = n
		}
	}

	if t.Failed() || len(got) > 0 {
		for _, f := range r.Findings("") {
			t.Logf("%s: %s at %s: %s", rendering, f.Class, f.Where, f.Detail)
		}
	}

	return got
}

// The anchor of an own-line comment is the first C# line that translates the Go code after it. The converter
// writes scaffolding ahead of a statement's own translation (a func literal's capture copy, a heap box for an
// escaping local), renames a labeled continue or break to a goto, and closes a struct literal with `)`: none of
// them moves the comment. `good` places each comment where Go has it, behind one of those shapes; `bad` puts one
// above a capture copy that the wrong statement follows, which must still read as misplaced.
func TestAnchorReadsPastConverterScaffolding(t *testing.T) {
	if got := anchorFindings(t, "good"); len(got) != 0 {
		t.Errorf("good: %v, want no misplaced or missed comment", got)
	}

	want := map[string]int{"misplaced:free-stmt/anchor-differs": 1}

	if got := anchorFindings(t, "bad"); !maps.Equal(got, want) {
		t.Errorf("bad: %v, want %v", got, want)
	}
}

// A lazily built package var is declared on one line, `[GoLazy] partial class lazyᴛX { internal static T X = ...`
// (docs/DESIGN-lazy-static-data.md). That line opens with an attribute and its initializer can name a `_package`
// class (`new text.template_package.FuncMap(...)`), yet it is the var's declaration: its doc comment anchors on it,
// it is the declaration the top-level walk maps, and the blank line above it is the var's separation.
func TestLazyHolderLineIsTheDeclaration(t *testing.T) {
	pairs := FindPairs("testdata/lazyholder/good", "testdata/lazyholder/goroot")

	if len(pairs) != 1 {
		t.Fatalf("%d pairs found, want 1 (lh/lh.cs against lh/lh.go)", len(pairs))
	}

	r := NewResults()

	if err := r.Analyze(pairs[0]); err != nil {
		t.Fatal(err)
	}

	// Vacuity: both var docs are read, and all three declarations after the import are mapped and separated as Go
	// separates them.
	if r.Groups() < 2 || r.r.topLevel["same"] != 3 {
		t.Errorf("%d comment groups, %d top-level separations alike; want at least 2 and exactly 3", r.Groups(), r.r.topLevel["same"])
	}

	for k, n := range r.Counts() {
		if n > 0 && (strings.HasPrefix(k, "misplaced:") || strings.HasPrefix(k, "missed:") || strings.HasPrefix(k, "top:")) {
			t.Errorf("%s = %d, want 0", k, n)
		}
	}

	if t.Failed() {
		for _, f := range r.Findings("") {
			t.Logf("%s at %s: %s", f.Class, f.Where, f.Detail)
		}
	}
}

// move2heapFindings reads testdata/move2heap's rendering: the classes a move line could move.
func move2heapFindings(t *testing.T, rendering string) map[string]int {
	t.Helper()

	pairs := FindPairs("testdata/move2heap/"+rendering, "testdata/move2heap/goroot")

	if len(pairs) != 1 {
		t.Fatalf("%s: %d pairs found, want 1 (m2h/m2h.cs against m2h/m2h.go)", rendering, len(pairs))
	}

	r := NewResults()

	if err := r.Analyze(pairs[0]); err != nil {
		t.Fatalf("%s: %v", rendering, err)
	}

	// Vacuity: both functions are read, or no class could move.
	if n := r.r.body["functions"]; n != 2 {
		t.Fatalf("%s: %d function bodies read, want 2", rendering, n)
	}

	got := map[string]int{}

	for k, n := range r.Counts() {
		if n > 0 && (k == "body:extra:unexplained" || k == "body:go-blank-cs-none" || strings.HasPrefix(k, "misplaced:")) {
			got[k] = n
		}
	}

	return got
}

// Go 1.26's `s = move2heap(s)` is part of the statement it precedes (cmd/compile/internal/slice inserts it there),
// and the converter writes it as its own line, `x = move2heap(xᴷ, x);`, ahead of that statement's translation:
// one per variable that escapes there. Like a capture copy or a bare heap box, it is never a statement of Go's, so
// a Go blank line above the statement sits above the move lines, and a comment above the statement sits above
// them too. `good` is the converter's emission: a blank before a two-slice return, and a comment before a range
// over a moved slice whose first identifier the move line does not carry. `bad` drops that blank and puts the
// comment above the wrong statement, which must still read as a missing blank and a misplaced comment.
func TestMove2heapLinesAreTheStatementsLowering(t *testing.T) {
	if got := move2heapFindings(t, "good"); len(got) != 0 {
		t.Errorf("good: %v, want no defect", got)
	}

	want := map[string]int{"body:go-blank-cs-none": 1, "misplaced:free-stmt/anchor-differs": 1}

	if got := move2heapFindings(t, "bad"); !maps.Equal(got, want) {
		t.Errorf("bad: %v, want %v", got, want)
	}
}

// preludeBlanks reads testdata/prelude's rendering: its body blank-line classes, and the separators read.
func preludeBlanks(t *testing.T, rendering string) (map[string]int, int) {
	t.Helper()

	pairs := FindPairs("testdata/prelude/"+rendering, "testdata/prelude/goroot")

	if len(pairs) != 1 {
		t.Fatalf("%s: %d pairs found, want 1 (prelude/prelude.cs against prelude/prelude.go)", rendering, len(pairs))
	}

	r := NewResults()

	if err := r.Analyze(pairs[0]); err != nil {
		t.Fatalf("%s: %v", rendering, err)
	}

	// Vacuity: the twelve functions are read, or no class could move.
	if n := r.r.body["functions"]; n != 12 {
		t.Fatalf("%s: %d function bodies read, want 12", rendering, n)
	}

	got := map[string]int{}

	for k, n := range r.Counts() {
		if strings.HasPrefix(k, "body:") {
			got[k] = n
		}
	}

	return got, r.r.body["prelude-separator"]
}

// The converter sets its prelude (named-result locals, variadic slices, array clones, heap boxes) apart from the
// Go body by exactly one blank line: the owner's decision 2, so that blank line is not a defect. `good` is the
// converter's emission; `bad` puts the blank line inside the prelude in four functions, as the converter did
// before, and two blank lines after the prelude in a fifth. `first` opens with a Go statement that reads like
// a prelude line (`var z int`), followed by Go's own blank line: only the blank line above it is the separator.
// `grow` opens with `buf := make([]int, len(xs))`, which the translation spells `var buf = new slice<nint>(...)`:
// Go's builtin `make` never appears on the C# line, so the identifier test must not require it. `opens` opens
// with a comment and then Go's blank line; `bad` drops that blank, as the converter does, and the census must
// report it rather than credit the prelude's separator above the comment as Go's blank. `regrow` has a Go blank
// line before `more := make(...)`: the line that places it must not require `make` either, or the blank reads
// unmapped in `good` and is never reported missing in `bad`. `boxed` has a Go blank line before an escaping
// local, whose translation is a heap box and then its assignment: the blank sits above the box, so the line that
// places it walks back over the box as it does over a comment. `boxedVar` (G's planted pair) has an escaping
// `var q int`, a box WITH an initializer and so a whole statement: `bad` drops the blank after it, and the walk
// must stop at the box rather than credit the blank above it to the line below. `grouped` has a Go blank before
// a `const (` group, which holds no identifier: the group's first spec places it. `scoped` has one before
// `if m := twice(n); …`, which the translation wraps in a scope block and whose `m` it renames `mΔ1`: the
// initializer's other identifiers place it, and the walk crosses the scope's `{`.
func TestPreludeSeparatorIsNotADefect(t *testing.T) {
	if got, separators := preludeBlanks(t, "good"); len(got) != 0 || separators != 7 {
		t.Errorf("good: %v and %d separators, want no body class and 7 separators", got, separators)
	}

	want := map[string]int{"body:extra:after-named-result-locals": 4, "body:extra:after-param-prelude": 1, "body:go-blank-cs-none": 6}

	// `bad` keeps regrow's prelude separator: only the blank before its make line is dropped.
	if got, separators := preludeBlanks(t, "bad"); !maps.Equal(got, want) || separators != 1 {
		t.Errorf("bad: %v and %d separators, want %v and 1 separator", got, separators, want)
	}
}

// A blank import emits no `using`: the converter notes it, and when the spec has a Go trailing comment the note is a
// block comment with Go's comment after it on the same line (seat #43, item 3):
// `/* blank import: unsafe_package (...) */ // for linkname`. That line's code part is only the converter's note, and
// the comment is still trailing its import, so it reads as a trailing comment where it is and is not counted as moved
// to its own line. Controls: the same comment alone on a line still reads moved, and the old fused `//` note, whose one
// comment holds Go's text inside it, still reads missed.
func TestCommentAfterAConverterNoteIsTrailing(t *testing.T) {
	const goSrc = "package p\n\nimport (\n\t\"fmt\"\n\t_ \"unsafe\" // for linkname\n)\n\nfunc F() { fmt.Println() }\n"
	const note = "blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)"

	for _, c := range []struct {
		name, line string
		want       string // the one finding the rendering must produce, or "" for none
	}{
		{"note then comment", "/* " + note + " */ // for linkname", ""},
		{"comment on its own line", "// for linkname", "misplaced:trailing-spec/trailing-to-own-line"},
		{"comment fused into the note", "// " + note + " // for linkname", "missed:trailing-spec"},
	} {
		dir := t.TempDir()
		goFile, csFile := dir+"/p.go", dir+"/p.cs"
		csSrc := "namespace go;\n\nusing fmt = fmt_package;\n" + c.line + "\n\npartial class p_package {\n\npublic static void F() {\n    fmt.Println();\n}\n\n} // end p_package\n"

		if err := os.WriteFile(goFile, []byte(goSrc), 0o644); err != nil {
			t.Fatal(err)
		}

		if err := os.WriteFile(csFile, []byte(csSrc), 0o644); err != nil {
			t.Fatal(err)
		}

		r := NewResults()

		if err := r.AnalyzeFiles(goFile, csFile, "p/p"); err != nil {
			t.Fatalf("%s: %v", c.name, err)
		}

		got := map[string]int{}

		for k, n := range r.Counts() {
			if strings.HasPrefix(k, "missed:") || strings.HasPrefix(k, "partial:") || strings.HasPrefix(k, "misplaced:") {
				got[k] = n
			}
		}

		want := map[string]int{}

		if c.want != "" {
			want[c.want] = 1
		}

		if !maps.Equal(got, want) {
			t.Errorf("%s: %v, want %v", c.name, got, want)
		}
	}
}
