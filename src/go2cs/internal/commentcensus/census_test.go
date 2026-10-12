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

	// Vacuity: the four functions are read, or no class could move.
	if n := r.r.body["functions"]; n != 4 {
		t.Fatalf("%s: %d function bodies read, want 4", rendering, n)
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
// converter's emission; `bad` puts the blank line inside the prelude in three functions, as the converter did
// before, and two blank lines after the prelude in the fourth. `first` opens with a Go statement that reads like
// a prelude line (`var z int`), followed by Go's own blank line: only the blank line above it is the separator.
func TestPreludeSeparatorIsNotADefect(t *testing.T) {
	if got, separators := preludeBlanks(t, "good"); len(got) != 0 || separators != 4 {
		t.Errorf("good: %v and %d separators, want no body class and 4 separators", got, separators)
	}

	want := map[string]int{"body:extra:after-named-result-locals": 3, "body:extra:after-param-prelude": 1}

	if got, separators := preludeBlanks(t, "bad"); !maps.Equal(got, want) || separators != 0 {
		t.Errorf("bad: %v and %d separators, want %v and 0 separators", got, separators, want)
	}
}
