// commentLineEndings_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the one class of text the converter WRITES without having line-broken it itself: a
// comment's raw Text, copied out of the Go source.
//
// writeOperations.go's header says the converter emits CRLF UNCONDITIONALLY, and .gitattributes
// pins the emitted artifact types to eol=crlf so every checkout materializes those same bytes.
// That was untrue in the emitted bytes. A LINE comment's Text carries no newline, so a group of
// them is separated by v.newline and comes out uniform. A BLOCK comment's Text is its WHOLE span,
// interior newlines included — and Go sources are bare-LF, so writing it verbatim put bare LFs into
// a file whose every other line ended CRLF. The measured shape: an emitted .cs whose bare-LF count
// equals the sum over its block comments of (closing line − opening line), which held exactly on
// four files at go1.24.13 (381, 316, 511, 11) with a no-block-comment control at 0.
//
// Nothing downstream ever saw it, which is why it survived: git normalizes the committed corpus,
// so the .cs in the tree is uniform however it was written and no build, diff or golden could
// disagree. The RAW emission is the only place the defect exists — which is why this guard reads
// the emitted bytes and never a line-split view of them. convertWithComments strips CRs by design;
// asserting over its output would pass on a file made entirely of bare LFs.
//
// Three sites answered this question and had drifted to three answers: writeCommentString wrote
// comment.Text verbatim, writeStandAloneCommentString replaced "\n" without collapsing CRLF first,
// and sourceLicenseNotices spelled it correctly by hand. They now share normalizeNewlines, and
// TestNormalizeNewlines below pins its contract directly.
//
// ⚠ Only writeCommentString was a LIVE defect, and the guard says so by what it can make fail.
// Regressing writeCommentString fails TestBlockCommentEmitsUniformCRLF, naming the count.
// Regressing writeStandAloneCommentString back to the un-collapsed spelling fails NOTHING here, and
// that is correct rather than a hole: go/scanner strips carriage returns out of a general comment,
// so an ast.Comment's Text never holds a CR whatever the file's endings are (measured at go1.24.13:
// a CRLF source gives Text with CR 0, LF 3), and no Go source can drive that site to CR CR LF. Its
// alignment removes a second spelling, and the predicate it now shares is guarded directly below --
// regressing normalizeNewlines' collapse fails TestNormalizeNewlines on the idempotence and CR CR
// arms. A caller handed content that did NOT come from the scanner -- which is what normalizeToCRLF
// does -- is the case that collapse is really for.

package main

import (
	"strings"
	"testing"
)

// countBareLF returns the number of LFs NOT preceded by a CR — the defect's signature, and the
// measurement the whole finding was scored on.
func countBareLF(content string) int {
	bare := 0

	for i := 0; i < len(content); i++ {
		if content[i] != '\n' {
			continue
		}

		if i == 0 || content[i-1] != '\r' {
			bare++
		}
	}

	return bare
}

// TestBlockCommentEmitsUniformCRLF is the end-to-end half: a real conversion, read as bytes.
//
// ⚠ The fixture is asserted to be a fixture BEFORE the verdict is read. A guard that counts bare
// LFs in an emission holding no multi-line block comment reports a clean zero because its predicate
// cannot fire — a vacuous green of exactly the kind this package keeps finding, and it would read
// as the fix working. So the Go source is checked to carry the block comment, and the emission is
// checked to carry its interior line, before the count means anything.
func TestBlockCommentEmitsUniformCRLF(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: loads the fixture package via go/packages")
	}

	const interior = "the second line of the block, which lives INSIDE comment.Text"

	// ⚠ THE SHAPE MATTERS, and the first fixture written for this guard had the wrong one. A block
	// comment standing free INSIDE the file is drained by writeStandAloneCommentString, which was
	// already replacing newlines; emitted from there, before and after were byte-identical and this
	// guard passed with the defect fully restored — it measured nothing. The defect's shape is the
	// PACKAGE DOC block comment (fmt/doc.go, log/slog/doc.go, runtime/metrics/doc.go), which reaches
	// writeCommentString and had its Text written verbatim. Both shapes were emitted with the
	// converter built at origin/master and at this tip before the fixture was settled: package-doc
	// block 4 bare LF -> 0, free-floating block 0 -> 0.
	source := "/*\n" +
		"A package doc as a BLOCK comment, the fmt/doc.go shape.\n" +
		interior + "\n" +
		"and a third, so the span is unambiguously multi-line.\n" +
		"*/\n" +
		"package main\n" +
		"\n" +
		"// A line comment, whose Text carries no newline at all — the arm that was always uniform.\n" +
		"func documented(n int) int {\n" +
		"\t/* an inner block\n" +
		"\t   spanning two lines inside a body */\n" +
		"\treturn n + 1\n" +
		"}\n"

	// The fixture is bare-LF, like every Go source at the pin. If this ever stops being true the
	// guard is measuring something else.
	if strings.Contains(source, "\r") {
		t.Fatal("fixture carries a CR: it no longer reproduces the bare-LF Go source this guards")
	}

	if strings.Count(source, "/*") != 2 {
		t.Fatalf("fixture must hold exactly 2 block comments, holds %d", strings.Count(source, "/*"))
	}

	emitted := convertWithCommentsRaw(t, source)

	if !strings.Contains(emitted, interior) {
		t.Fatalf("the block comment's interior line is absent from the emission, so this guard measured nothing:\n%s", emitted)
	}

	if bare := countBareLF(emitted); bare != 0 {
		t.Errorf("emitted .cs carries %d bare LF, want 0 — a block comment's interior newlines reached the output verbatim", bare)
	}
}

// TestNormalizeNewlines pins the predicate itself, including the case that made
// writeStandAloneCommentString's hand-rolled spelling wrong: a plain "\n" replacement over
// CRLF-carrying input produces CR CR LF, silently, on exactly the inputs a Windows checkout gives.
func TestNormalizeNewlines(t *testing.T) {
	const crlf = "\r\n"

	cases := []struct {
		name string
		in   string
		want string
	}{
		{"bare LF becomes CRLF", "a\nb\nc", "a\r\nb\r\nc"},
		{"CRLF is left alone (idempotent)", "a\r\nb\r\nc", "a\r\nb\r\nc"},
		{"mixed input comes out uniform", "a\r\nb\nc", "a\r\nb\r\nc"},
		{"no newline at all is untouched", "a single line", "a single line"},
		{"empty is empty", "", ""},
		{"a lone trailing LF", "a\n", "a\r\n"},
		{"a lone trailing CRLF", "a\r\n", "a\r\n"},
		{"a block comment's whole span", "/*\nx\ny\n*/", "/*\r\nx\r\ny\r\n*/"},
	}

	for _, tc := range cases {
		t.Run(tc.name, func(t *testing.T) {
			got := normalizeNewlines(tc.in, crlf)

			if got != tc.want {
				t.Errorf("normalizeNewlines(%q) = %q, want %q", tc.in, got, tc.want)
			}

			if strings.Contains(got, "\r\r") {
				t.Errorf("normalizeNewlines(%q) produced CR CR: %q", tc.in, got)
			}

			if again := normalizeNewlines(got, crlf); again != got {
				t.Errorf("not idempotent: normalizeNewlines(%q) = %q", got, again)
			}
		})
	}

	// normalizeToCRLF is now a caller rather than a second implementation. Pinned so the two cannot
	// drift back apart unnoticed.
	for _, tc := range cases {
		if got, want := normalizeToCRLF(tc.in), normalizeNewlines(tc.in, crlf); got != want {
			t.Errorf("normalizeToCRLF(%q) = %q, but normalizeNewlines gives %q", tc.in, got, want)
		}
	}

	// A non-CRLF target, so the helper is pinned as a function of its argument and not of a
	// hardcoded pair that happens to be the only one in use.
	if got, want := normalizeNewlines("a\r\nb\nc", "\n"), "a\nb\nc"; got != want {
		t.Errorf("normalizeNewlines with an LF target = %q, want %q", got, want)
	}
}

// TestCountBareLF controls the guard's own instrument. A counter that cannot distinguish the two
// shapes would report 0 on the defect and the guard above would pass for the wrong reason.
func TestCountBareLF(t *testing.T) {
	cases := []struct {
		in   string
		want int
	}{
		{"", 0},
		{"no newline", 0},
		{"a\r\nb\r\n", 0},
		{"a\nb\n", 2},
		{"a\r\nb\nc\r\n", 1},
		{"\n", 1},
		{"\r\n", 0},
		{"\n\r\n\n", 2},
	}

	for _, tc := range cases {
		if got := countBareLF(tc.in); got != tc.want {
			t.Errorf("countBareLF(%q) = %d, want %d", tc.in, got, tc.want)
		}
	}
}
