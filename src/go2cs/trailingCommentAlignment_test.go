// trailingCommentAlignment_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards END-OF-LINE comment alignment in `-comments` output (docs/PLAN-comment-and-spacing-parity.md, class J).
//
// gofmt aligns the trailing comments of consecutive lines into one column, and the converted C# should read the
// same way. The runs are read from Go, not from C#: the Go source is gofmt's output, so a run is exactly a group
// of consecutive lines whose comments gofmt put in one column. That carries over every rule gofmt applies (a
// section break after a statement spanning lines, the key-size ratio of a composite literal, a blank line, a
// change of indentation) without re-deriving any of them on C#. Each C# line of one Go run is padded to one
// column, the longest code plus one space counted in runes, even where the converter splits the run with lines
// of its own; a comment that is the only one of its run gets one space.
//
// Only Go's comments move. A comment the converter writes (`// end main_package`, the deferred-call note) is
// never padded, and no line is ever added or removed, so the position map stays valid.

package main

import (
	"os"
	"path/filepath"
	"strings"
	"testing"
	"unicode/utf8"
)

const trailingAlignmentSource = `package main

import "fmt"

type point struct {
	x     int    // the x coordinate
	label string // what the point is called
	next  *point // the next point
}

const (
	small    = 1    // a small value
	medium   = 1000 // a middling one
	enormous = 1e9  // a very large one
)

func handle(err *error) {
	e := recover() // recovered, or nil
	if e != nil {
		*err = fmt.Errorf("%v", e)
	}
}

func main() {
	p := point{x: 1, label: "a"} // a point
	total := 0                   // the running sum
	values := []int{
		1,   // one
		22,  // twenty-two
		333, // three hundred thirty-three
	}
	n := 3                            // a count
	show := func() { fmt.Println(n) } // captures n
	n = 4                             // changes what show prints
	show()
	double := func(n int) int {
		return n * 2
	} // a closure spanning lines
	count := double(2) // gofmt starts a new section here
	weights := map[string]int{
		"a": 1, // a short key
		"a key that is long enough to break gofmt's alignment of keys": 2, // a long key
	}
	for _, v := range values {
		total += v
	}
	fmt.Println(p.label, total, small, medium, enormous, count, weights)
}

var last = 1 // the last declaration
`

// trailingCommentColumn returns the rune column at which the trailing comment with the given text starts on its
// line, and the rune width of the code ahead of it with trailing spaces removed.
func trailingCommentColumn(t *testing.T, lines []string, text string) (column, codeWidth int, line string) {
	t.Helper()

	line = lines[findCommentLine(t, lines, "// "+text)]
	at := strings.LastIndex(line, "// "+text)
	code := strings.TrimRight(line[:at], " ")

	if strings.TrimSpace(code) == "" {
		t.Fatalf("the comment %q is not a trailing comment in C#:\n%s", text, line)
	}

	return utf8.RuneCountInString(line[:at]), utf8.RuneCountInString(code), line
}

func TestTrailingCommentsAlignAsGofmtAlignedThem(t *testing.T) {
	lines := convertWithComments(t, trailingAlignmentSource)
	dump := strings.Join(lines, "\n")

	// Each Go run gofmt aligned is one column in C#: the longest code plus one space. The struct's run holds a
	// `ж<point>` field, so the width is counted in runes, as gofmt counts it. The last run is split in C# by the
	// lines of the local function `show` becomes, and its pieces still share one column.
	runs := [][]string{
		{"the x coordinate", "what the point is called", "the next point"},
		{"a small value", "a middling one", "a very large one"},
		{"a point", "the running sum"},
		{"one", "twenty-two", "three hundred thirty-three"},
		{"a count", "captures n", "changes what show prints"},
	}

	for _, run := range runs {
		want := 0
		for _, text := range run {
			if _, width, _ := trailingCommentColumn(t, lines, text); width+1 > want {
				want = width + 1
			}
		}

		for _, text := range run {
			if column, _, line := trailingCommentColumn(t, lines, text); column != want {
				t.Errorf("run %q: the comment %q starts at column %d, want %d (the longest code plus one):\n%s\n\n%s", run[0], text, column, want, line, dump)
			}
		}
	}

	// A comment gofmt did not align with its neighbour stays one space from its code, even on the very next C#
	// line: the statement after one spanning lines starts a new section, and a key far longer than the one
	// before breaks a composite literal's alignment. A comment alone in its run is one space from its code too.
	alone := []string{
		"a closure spanning lines", "gofmt starts a new section here",
		"a short key", "a long key",
		"recovered, or nil", "the last declaration",
	}

	for _, text := range alone {
		if column, width, line := trailingCommentColumn(t, lines, text); column != width+1 {
			t.Errorf("the comment %q is %d spaces from its code, want 1 (gofmt did not align it):\n%s\n\n%s", text, column-width, line, dump)
		}
	}

	// The converter's own trailing comments are never padded.
	for _, want := range []string{
		"        var calledByDefer = GoFrame.TakeDeferredCall(); // Go: is this call the deferred one?",
		"} // end main_package",
	} {
		found := false
		for _, line := range lines {
			if line == want {
				found = true
			}
		}

		if !found {
			t.Errorf("the converter's own comment line is missing or was changed; want exactly:\n%s\n\n%s", want, dump)
		}
	}

	// Nothing the alignment marks a comment with survives into the emitted text.
	for i, line := range lines {
		for _, r := range line {
			if r < 0x20 && r != '\t' {
				t.Errorf("line %d holds control character %U:\n%q", i+1, r, line)
			}
		}
	}
}

// sentinelled writes a trailing comment's text as the converter marks it for run.
func sentinelled(run, text string) string {
	return alignSentinelMark + run + alignSentinelClose + strings.TrimPrefix(text, "//")
}

// The pass changes only the spaces between a line's code and its comment: the line count, every line ending, a
// comment the converter placed on its own line and an unmarked comment all stay as they were, and no sentinel
// survives.
func TestAlignTrailingCommentsChangesOnlyThePadding(t *testing.T) {
	text := strings.Join([]string{
		"partial struct point {",
		"    internal nint x; " + sentinelled("6", "// x") + "\r",
		"    internal ж<point> next;   " + sentinelled("6", "// next") + "\r",
		"    " + sentinelled("6", "// placed on its own line") + "\r",
		"    internal nint y; " + sentinelled("9", "// alone") + "\r",
		"} // end main_package\r",
		"",
	}, "\n")

	got, runs := alignTrailingComments(text)
	want := strings.Join([]string{
		"partial struct point {",
		"    internal nint x;        // x\r",
		"    internal ж<point> next; // next\r",
		"    // placed on its own line\r",
		"    internal nint y; // alone\r",
		"} // end main_package\r",
		"",
	}, "\n")

	if got != want {
		t.Fatalf("aligned text:\n%q\nwant:\n%q", got, want)
	}

	if len(runs) != 2 || len(runs[0].lines) != 2 || runs[0].lines[0] != 1 || runs[0].lines[1] != 2 || runs[0].comments[1] != "// next" {
		t.Fatalf("runs = %+v, want the two-line run at lines 1 and 2 and the one-line run at line 4", runs)
	}
}

// A deferred-marker pass resolves a type name AFTER the file is written, which changes the width of the code ahead
// of a trailing comment. rewriteDeferredMarkers pads the file's recorded runs again, so the comments end where the
// resolved names put them; a run whose comment no longer ends its line is left as it is.
func TestDeferredMarkersRepadAlignedRuns(t *testing.T) {
	text := strings.Join([]string{
		"partial struct holder {",
		"    internal «T:inner:T» inner; " + sentinelled("6", "// an anonymous struct"),
		"    internal nint n; " + sentinelled("6", "// a plain field"),
		"}",
		"",
	}, "\r\n")

	aligned, runs := alignTrailingComments(text)
	fileName := filepath.Join(t.TempDir(), "main.cs")

	if err := os.WriteFile(fileName, []byte(aligned), 0o644); err != nil {
		t.Fatal(err)
	}

	registerAlignedRuns(fileName, runs, 0)
	t.Cleanup(func() { registerAlignedRuns(fileName, nil, 0) })

	rewriteDeferredMarkers([]string{fileName}, "test", "«T:", ":T»", func(string, int, string) (string, bool) {
		return "holder_inner", true
	})

	data, err := os.ReadFile(fileName)
	if err != nil {
		t.Fatal(err)
	}

	want := strings.Join([]string{
		"partial struct holder {",
		"    internal holder_inner inner; // an anonymous struct",
		"    internal nint n;             // a plain field",
		"}",
		"",
	}, "\r\n")

	if string(data) != want {
		t.Fatalf("after the marker pass:\n%q\nwant:\n%q", data, want)
	}
}
