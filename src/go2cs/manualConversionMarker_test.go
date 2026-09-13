// manualConversionMarker_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"os"
	"path/filepath"
	"testing"
)

// containsManualConversionMarker decides whether a reconvert may overwrite a file, so a FALSE
// negative is the expensive direction: the converter regenerates the Go version over a whole-file
// hand-own and the loss is silent (main.go drops marked files from the convert set; a marked file is
// diverted to a .cs.auto review sibling instead).
//
// It used to decide that by probing each line with strings.Contains for "/*" BEFORE asking whether
// that "/*" was inside a // line comment. core/runtime/runtime2_impl.cs opens with prose about Go's
// pointer family — "hide a *g/*p/*m inside a uintptr" — where the "/*" is ordinary English inside a
// line comment and no "*/" ever follows. Everything after it, including the file's own marker on
// line 24, was read as comment interior, so the predicate reported a hand-owned file as not
// hand-owned. It was inert there (an _impl companion has no Go counterpart emitted at that path),
// but it is exactly the shape that clobbers a hand-own, and it made the predicate's census disagree
// with the line-anchored grep census by exactly that one file (40 vs 41).
//
// These guards pin the lexing rule rather than the one file: in C# a // comment runs to end of line
// and nothing inside it opens a block comment, while a genuinely unclosed /* really does hide the
// rest of the file.

// writeMarkerFixture writes contents to a temp .cs file and returns its path.
func writeMarkerFixture(t *testing.T, contents string) string {
	t.Helper()

	file := filepath.Join(t.TempDir(), "fixture.cs")

	if err := os.WriteFile(file, []byte(contents), 0644); err != nil {
		t.Fatalf("write fixture: %v", err)
	}

	return file
}

func assertMarker(t *testing.T, contents string, want bool) {
	t.Helper()

	got, err := containsManualConversionMarker(writeMarkerFixture(t, contents))

	if err != nil {
		t.Fatalf("containsManualConversionMarker: %v", err)
	}

	if got != want {
		t.Errorf("containsManualConversionMarker = %v, want %v, for:\n%s", got, want, contents)
	}
}

// TestMarkerSurvivesBlockDelimiterInsideLineComment is the direct regression: the repro shape, an
// unbalanced "/*" carried as prose inside a // comment, ahead of a real marker.
func TestMarkerSurvivesBlockDelimiterInsideLineComment(t *testing.T) {
	assertMarker(t, "using go;\r\n"+
		"\r\n"+
		"// Go's guintptr/puintptr/muintptr hide a *g/*p/*m inside a uintptr so the Go GC does not\r\n"+
		"// observe the reference.\r\n"+
		"[module: GoManualConversion]\r\n"+
		"\r\n"+
		"partial class runtime_package {}\r\n", true)
}

// TestMarkerInsideUnclosedBlockCommentStaysInvisible is the negative control the fix must NOT break:
// a marker inside a block comment that never closes is commented out, and the predicate says so.
func TestMarkerInsideUnclosedBlockCommentStaysInvisible(t *testing.T) {
	assertMarker(t, "using go;\r\n"+
		"\r\n"+
		"/* the rest of this file is commented out, marker included\r\n"+
		"[module: GoManualConversion]\r\n"+
		"\r\n"+
		"partial class runtime_package {}\r\n", false)
}

// TestMarkerAfterClosedBlockCommentIsSeen pins the other half of the block state: once "*/" closes
// the block, scanning resumes as code — on the very same line as well as on later ones.
func TestMarkerAfterClosedBlockCommentIsSeen(t *testing.T) {
	assertMarker(t, "/* a header block\r\n"+
		"   spanning several lines */\r\n"+
		"[module: go.GoManualConversion]\r\n"+
		"partial class time_package {}\r\n", true)

	assertMarker(t, "/* header */ [module: go.GoManualConversionAttribute]\r\n"+
		"partial class time_package {}\r\n", true)
}

// TestMarkerCommentedOutIsNotAHandOwn covers the forms that mention the marker without declaring
// one. The trailing-comment case is the shape the old scanner got wrong in the other direction: it
// only skipped comments that began a line, so a marker parked after code counted as a hand-own.
func TestMarkerCommentedOutIsNotAHandOwn(t *testing.T) {
	assertMarker(t, "// A placeholder mentioning [module: go.GoManualConversion] in prose.\r\n"+
		"partial class reflect_package {}\r\n", false)

	assertMarker(t, "using go; // [module: GoManualConversion]\r\n"+
		"partial class reflect_package {}\r\n", false)

	assertMarker(t, "/* [module: GoManualConversion] */\r\n"+
		"partial class reflect_package {}\r\n", false)
}

// TestMarkerAfterClassDefinitionIsIgnored pins the early exit: a valid module marker precedes the
// first class, so anything after one is not a whole-file hand-own declaration.
func TestMarkerAfterClassDefinitionIsIgnored(t *testing.T) {
	assertMarker(t, "using go;\r\n"+
		"partial class fmt_package {\r\n"+
		"[module: GoManualConversion]\r\n"+
		"}\r\n", false)
}

// TestStripCSharpCommentsLexesLineAndBlockComments exercises the lexer directly, including the state
// it carries between lines.
func TestStripCSharpCommentsLexesLineAndBlockComments(t *testing.T) {
	tests := []struct {
		name     string
		lines    []string
		want     []string
		wantOpen bool
	}{
		{
			name:  "block delimiter inside a line comment opens nothing",
			lines: []string{"// prose about *g/*p/*m", "code();"},
			want:  []string{"", "code();"},
		},
		{
			name:  "unclosed block swallows following lines",
			lines: []string{"/* open", "still comment", "*/ code();"},
			want:  []string{"", "", " code();"},
		},
		{
			name:     "block left open at end of input",
			lines:    []string{"before(); /* open"},
			want:     []string{"before(); "},
			wantOpen: true,
		},
		{
			name:  "inline block keeps the code on both sides",
			lines: []string{"a(); /* mid */ b();"},
			want:  []string{"a();  b();"},
		},
		{
			name:  "line comment wins over a later block opener",
			lines: []string{"a(); // b(); /* c"},
			want:  []string{"a(); "},
		},
	}

	for _, test := range tests {
		t.Run(test.name, func(t *testing.T) {
			inBlockComment := false

			for index, line := range test.lines {
				if got := stripCSharpComments(line, &inBlockComment); got != test.want[index] {
					t.Errorf("line %d: stripCSharpComments(%q) = %q, want %q", index, line, got, test.want[index])
				}
			}

			if inBlockComment != test.wantOpen {
				t.Errorf("block-open state = %v, want %v", inBlockComment, test.wantOpen)
			}
		})
	}
}
