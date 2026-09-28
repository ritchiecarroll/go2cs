// docsTableCodePipe_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package repoguard

import (
	"fmt"
	"sort"
	"strings"
	"testing"
)

// A `\|` inside a CODE SPAN on a table row renders differently on GitHub and on the Pages site
// (docs/phase4/briefs/pages-audit-2026-09-27.md, item 8). GitHub removes the backslash before a pipe
// anywhere in a table row, code spans included; kramdown keeps it. So one renderer is always wrong: a
// shell pipe `head -c 200 "$LOG" \| tr` shows its backslash on the site, and a grep BRE alternation
// `'DllImport\|LibraryImport'` loses its backslash on GitHub.
//
// The form both render the same is an HTML code element with entities: `<code>… &#124; …</code>` for a
// pipe, and `&#92;&#124;` for a literal backslash-pipe. Inside <code>, GitHub still applies Markdown
// escapes and emphasis to the text, so backslashes and Markdown-active characters are entities too.
//
// SCOPE: the living pages (livingMarkdownPages), less docs/phase3/, whose pages are Phase 3's closed
// records. A `\|` OUTSIDE code in a table cell is fine on both renderers and is not flagged.
//
// POSITIVE CONTROL, verified when this guard landed: restore GoCorpusMigration.md's first seeding-check
// row to its backtick form and TestDocsTableCodeSpanHasNoEscapedPipe names that line.

// tableCodeSpanPipes returns the 1-based lines of table rows (outside fenced code and HTML comments,
// blockquote prefixes stripped, the line starting with '|') that hold a backtick code span whose content
// contains `\|`.
func tableCodeSpanPipes(text string) []int {
	lines, _, skip := blockLines(text)
	var hits []int

	for i, line := range lines {
		if skip[i] || !strings.HasPrefix(strings.TrimLeft(line, " \t"), "|") {
			continue
		}

		runs := blockBacktickRun.FindAllStringIndex(line, -1)

		for k := 0; k < len(runs); {
			n := runs[k][1] - runs[k][0]
			closed := -1

			for j := k + 1; j < len(runs); j++ {
				if runs[j][1]-runs[j][0] == n {
					closed = j
					break
				}
			}

			if closed < 0 {
				k++
				continue
			}

			if strings.Contains(line[runs[k][1]:runs[closed][0]], `\|`) {
				hits = append(hits, i+1)
				break
			}

			k = closed + 1
		}
	}

	return hits
}

// TestTableCodePipeRuleMatchesTheRenderers pins the rule, one shape per case.
func TestTableCodePipeRuleMatchesTheRenderers(t *testing.T) {
	cases := []struct {
		name string
		text string
		want []int
	}{
		{"a pipe escaped inside a code span on a table row", "| a | b |\n|---|---|\n| x | `head \\| wc` |\n", []int{3}},
		{"the fix: an HTML code element with an entity", "| a | b |\n|---|---|\n| x | <code>head &#124; wc</code> |\n", nil},
		{"a literal backslash-pipe as entities", "| x | <code>grep 'a&#92;&#124;b'</code> |\n", nil},
		{"an escaped pipe OUTSIDE code is fine on both", "| x | a \\| b |\n", nil},
		{"a double-backtick span", "| x | ``a \\| b`` |\n", []int{1}},
		{"not a table row", "text `a \\| b` more\n", nil},
		{"inside a blockquote", "> | x | `a \\| b` |\n", []int{1}},
		{"fenced code is not read", "```\n| x | `a \\| b` |\n```\n", nil},
		{"an HTML comment is not read", "<!-- | x | `a \\| b` | -->\n", nil},
		{"an unclosed backtick is not a span", "| x | `a \\| b |\n", nil},
	}

	for _, c := range cases {
		if got := tableCodeSpanPipes(c.text); fmt.Sprint(got) != fmt.Sprint(c.want) {
			t.Errorf("%s: got %v, want %v", c.name, got, c.want)
		}
	}
}

// TestDocsTableCodeSpanHasNoEscapedPipe is the guard over the living pages.
func TestDocsTableCodeSpanHasNoEscapedPipe(t *testing.T) {
	var problems []string

	for rel, text := range livingMarkdownPages(t) {
		if strings.HasPrefix(rel, "docs/phase3/") {
			continue
		}

		for _, line := range tableCodeSpanPipes(text) {
			problems = append(problems, fmt.Sprintf("%s:%d: a code span on a table row holds `\\|`, which GitHub and the Pages site render differently -- write it as <code>…&#124;…</code> (see this file's header)", rel, line))
		}
	}

	sort.Strings(problems)

	if len(problems) > 0 {
		t.Fatalf("%d table-row code span(s) with an escaped pipe:\n  %s", len(problems), strings.Join(problems, "\n  "))
	}
}
