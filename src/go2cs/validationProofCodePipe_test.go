// validationProofCodePipe_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"maps"
	"regexp"
	"strings"
	"testing"
)

// A proof page names each test in code. In a TABLE cell a `\|` inside a code span renders differently on GitHub
// (which drops the backslash) and on the Pages site (which keeps it), so repoguard's
// TestDocsTableCodeSpanHasNoEscapedPipe refuses it; the form both render the same is `<code>…&#124;…</code>`. In
// PROSE a code span is literal on both, so a pipe there is written as it is. timefmt-go's proof page (index.md:26,
// a test name holding a pipe) is the first page that met this; no standard-library test name holds one.

// tableCodePipe matches a table row holding a backtick code span whose content has `\|` (the guard's rule, for
// single-backtick spans, which is all the proof writer emits).
var tableCodePipe = regexp.MustCompile("^\\|.*`[^`]*\\\\\\|[^`]*`")

func TestValidationProofPageCodeCellWithAPipe(t *testing.T) {
	comparison, disclosures := loadProofFixture(t)

	const name = "TestFormat/2006-01-02|15:04"

	comparison.Go = maps.Clone(comparison.Go)
	comparison.CSharp = maps.Clone(comparison.CSharp)
	comparison.Go[name] = "pass"
	comparison.CSharp[name] = "pass"

	page := renderValidationProofPage(fixtureProvenance(), comparison, disclosures, nil)

	for i, line := range strings.Split(page, "\n") {
		if tableCodePipe.MatchString(line) {
			t.Errorf("line %d: a table-row code span holds an escaped pipe, which GitHub and the Pages site render differently:\n%s", i+1, line)
		}
	}

	if want := "| <code>TestFormat/2006-01-02&#124;15:04</code> | pass | pass |"; !strings.Contains(page, want) {
		t.Errorf("the verdict row is not in the HTML code form both renderers agree on; want %q in the page", want)
	}
}

func TestProofCodeForms(t *testing.T) {
	cases := []struct {
		value, cell, span string
	}{
		// No pipe: the backtick span, byte for byte what every committed page already holds.
		{"TestParse/a_b", "`TestParse/a_b`", "`TestParse/a_b`"},
		{"first\nsecond", "`first second`", "`first second`"},
		// A pipe: the HTML code element in a cell, with backslash and the Markdown-active characters as entities;
		// the plain span in prose, where a code span is literal on both renderers.
		{"TestFormat/a|b", "<code>TestFormat/a&#124;b</code>", "`TestFormat/a|b`"},
		{`Test/x\|y_*<z>&`, "<code>Test/x&#92;&#124;y&#95;&#42;&lt;z&gt;&amp;</code>", "`Test/x\\|y_*<z>&`"},
	}

	for _, c := range cases {
		if got := proofCodeCell(c.value); got != c.cell {
			t.Errorf("proofCodeCell(%q) = %q, want %q", c.value, got, c.cell)
		}

		if got := proofCodeSpan(c.value); got != c.span {
			t.Errorf("proofCodeSpan(%q) = %q, want %q", c.value, got, c.span)
		}
	}
}
