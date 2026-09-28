// docsKramdownRawHTML_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package repoguard

import (
	"fmt"
	"os"
	"path/filepath"
	"regexp"
	"strings"
	"testing"
)

// GitHub Pages renders docs\ with kramdown 2.4 (the github-pages gem); GitHub's own renderer does not.
// They disagree on one line shape, and the difference is silent on GitHub.
//
// In GitHub's renderer a code span may cross a line break, and an HTML block cannot interrupt a
// paragraph. kramdown ends a paragraph at any line that STARTS with `<name`, where name is not a span
// element (parser/kramdown/paragraph.rb, LAZY_END_HTML_START), and opens a raw HTML block there
// (parser/html.rb, parse_raw_html). With no closing tag, everything to the end of the page, or of the
// list item, is published as raw text: headings lose their ids and fences show as ``` text. A wrapped
// code span whose next line starts `<placeholder>` is enough. Measured on the built site at
// 4fb6e460c6 (docs/phase4/briefs/pages-audit-2026-09-27.md, item 1): GoCorpusMigration.md lost
// everything after its line 1075, and 128 in-site links pointed at headings the break had swallowed.
//
// The remedy keeps the same words and the same line count: move the break earlier, so the code span
// STARTS the next line. GitHub renders the soft break as a space either way.
//
// POSITIVE CONTROL: undo the rewrap at ConversionStrategies-Reference/manual-conversions.md (the line
// ending `**one-argument**`, whose next line is `` `//go:linkname <thisFunc>` handle. ``) and
// TestDocsSurviveKramdownRawHTML goes RED naming that line. Verified when this guard landed, together
// with the red at e798b3e7fb, which named exactly the five living lines rewrapped with it.

// kramdownLazyEndSpanElements is paragraph.rb's LAZY_END_HTML_SPAN_ELEMENTS at REL_2_4_0: html.rb's
// HTML_SPAN_ELEMENTS plus script. A line starting with one of these does NOT end a paragraph.
var kramdownLazyEndSpanElements = strings.Fields(`a abbr acronym b big bdo br button cite code del dfn em i
	img input ins kbd label mark option q rb rbc rp rt rtc ruby samp select small span strong sub sup tt
	u var script`)

// kramdownTagStart is html.rb's HTML_TAG_RE, anchored at the start of the stripped line: a name, then
// attributes, an optional '/', and '>'. The name is REXML's UNAME_STR, ASCII only here (the docs'
// placeholders are ASCII).
var kramdownTagStart = regexp.MustCompile(`^<([A-Za-z_][-\w.]*(?::[A-Za-z_][-\w.]*)?)\s*(?:\s+[A-Za-z_][-\w.:]*(?:\s*=\s*(?:\w+|"[^"]*"|'[^']*'))?)*\s*/?>`)

// kramdownFence opens or closes fenced code, backtick or tilde, after the container prefixes.
var kramdownFence = regexp.MustCompile("^(`{3,}|~{3,})")

// containerPrefix is one blockquote marker or one list marker, with its following space.
var containerPrefix = regexp.MustCompile(`^(?:>[ \t]?|(?:[-*+]|\d{1,9}[.)])(?:[ \t]+|$))`)

// kramdownRawHTMLExempt are the pages this guard does not read, each for a stated reason. Records are
// never rewritten: whether their trigger lines are rewrapped is an owner decision (the audit's item 1,
// "Owner decision"), and until then they stay exempt BY NAME, so a new page never inherits the pass.
var kramdownRawHTMLExempt = map[string]string{
	"docs/doctrine/JOURNAL-2026-09-12.md":             "the byte-identical pre-split CLAUDE.md; its blob identity is enforced by TestContextBudget",
	"docs/phase4/BOARD-next-validation-candidates.md": "the append-only findings ledger",
	"docs/phase4/CENSUS-preservation-2026-09-12.md":   "a point-in-time record",
	"docs/phase4/hopA-time-prestage.md":               "a point-in-time record",
	"docs/phase4/DESIGN-managed-pointer-token.md":     "a point-in-time record",
	"docs/phase4/Phase4-Autonomous-Loop-Charter.md":   "a point-in-time record",
	"docs/phase4/SESSION-ROLL-2026-09-01-EVENING.md":  "a point-in-time record",
}

// stripContainers removes indentation and every blockquote and list-marker prefix, so a line inside a
// quote or a list item reads as it does inside its container.
func stripContainers(line string) string {
	for {
		line = strings.TrimLeft(line, " \t")
		m := containerPrefix.FindString(line)

		if m == "" {
			return line
		}

		line = line[len(m):]
	}
}

// isKramdownLazyEndSpan reports whether kramdown's `(?!(?:span|...)\b)` lookahead refuses name: it
// starts with a span element followed by a word boundary.
func isKramdownLazyEndSpan(name string) bool {
	for _, e := range kramdownLazyEndSpanElements {
		if !strings.HasPrefix(name, e) {
			continue
		}

		if len(name) == len(e) {
			return true
		}

		c := name[len(e)]

		if !(c == '_' || c >= '0' && c <= '9' || c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z') {
			return true
		}
	}

	return false
}

// kramdownRawHTMLTriggers returns the 1-based lines outside fenced code that open a raw HTML block in
// the middle of a paragraph: the stripped line starts with a non-span tag, and the stripped line
// before it is non-blank text.
func kramdownRawHTMLTriggers(text string) []int {
	var hits []int
	fence := ""
	previous := ""

	for i, raw := range strings.Split(strings.ReplaceAll(text, "\r\n", "\n"), "\n") {
		line := stripContainers(raw)

		if m := kramdownFence.FindString(line); m != "" {
			switch {
			case fence == "":
				fence = m
			case m[0] == fence[0] && len(m) >= len(fence) && strings.TrimSpace(line[len(m):]) == "":
				fence = ""
			}

			previous = ""
			continue
		}

		if fence != "" {
			continue
		}

		if m := kramdownTagStart.FindStringSubmatch(line); m != nil && !isKramdownLazyEndSpan(m[1]) && strings.TrimSpace(previous) != "" {
			hits = append(hits, i+1)
		}

		previous = line
	}

	return hits
}

// TestKramdownRawHTMLRuleMatchesKramdown pins the detector to kramdown's rule, one shape per case.
func TestKramdownRawHTMLRuleMatchesKramdown(t *testing.T) {
	cases := []struct {
		name string
		text string
		want []int
	}{
		{"a wrapped code span whose next line starts a placeholder", "the **one-argument** `//go:linkname\n<thisFunc>` handle.\n", []int{2}},
		{"the rewrap: the code span starts the line", "the **one-argument**\n`//go:linkname <thisFunc>` handle.\n", nil},
		{"inside a blockquote", "> resume from\n> <incoming-sha> and read it\n", []int{2}},
		{"inside a list item", "- run it at\n  <commit-sha> first\n", []int{2}},
		{"a tag with attributes", "text\n<div class=\"x\">\n", []int{2}},
		{"a self-closed tag", "text\n<hr/>\n", []int{2}},
		{"after a blank line it is an ordinary HTML block", "text\n\n<details>\n", nil},
		{"a span element continues the paragraph", "text\n<code>x</code> more\n", nil},
		{"script is a lazy-end span too", "text\n<script>\n", nil},
		{"a span name needs a word boundary", "text\n<abc>\n", []int{2}},
		{"a hyphen is a word boundary", "text\n<a-b>\n", nil},
		{"not a whole tag: no closing bracket", "text\n<T and more\n", nil},
		{"a closing or comment tag is not an opening tag", "text\n</div>\n<!-- c -->\n", nil},
		{"fenced code is not read", "text\n```\nx\n<T>\n```\n", nil},
		{"fences inside a blockquote are not read", "> text\n> ```\n> x\n> <T>\n> ```\n", nil},
		{"a longer closing fence closes", "text\n````\nx\n``````\ny\n<T>\n", []int{6}},
	}

	for _, c := range cases {
		if got := kramdownRawHTMLTriggers(c.text); fmt.Sprint(got) != fmt.Sprint(c.want) {
			t.Errorf("%s: got %v, want %v", c.name, got, c.want)
		}
	}
}

// TestDocsSurviveKramdownRawHTML is the guard over the committed tree.
func TestDocsSurviveKramdownRawHTML(t *testing.T) {
	root := repoRootFromPackageDir(t)
	tracked := gitTrackedFiles(t, root)

	if len(tracked) < 1000 {
		t.Fatalf("git ls-files returned %d paths, too few to be this repository; a guard that scans nothing passes everything", len(tracked))
	}

	isTracked := map[string]bool{}

	for _, rel := range tracked {
		isTracked[rel] = true
	}

	var problems []string

	for rel := range kramdownRawHTMLExempt {
		if !isTracked[rel] {
			problems = append(problems, fmt.Sprintf("%s: exempt by name but not tracked -- remove its exemption", rel))
		}
	}

	pages := 0

	for _, rel := range tracked {
		if !strings.HasPrefix(rel, "docs/") || frozenSnapshot.MatchString(rel) || kramdownRawHTMLExempt[rel] != "" {
			continue
		}

		// Markdown by extension, under Jekyll's own path exclusions: what kramdown renders.
		if !jekyllRendersThroughLiquid(strings.TrimPrefix(rel, "docs/"), nil) {
			continue
		}

		data, err := os.ReadFile(filepath.Join(root, filepath.FromSlash(rel)))

		if err != nil {
			t.Fatalf("cannot read %s: %v", rel, err)
		}

		pages++

		for _, line := range kramdownRawHTMLTriggers(strings.TrimPrefix(string(data), "\xef\xbb\xbf")) {
			problems = append(problems, fmt.Sprintf("%s:%d: a line starting with a non-span tag ends the paragraph above it, so kramdown publishes the rest of the page or list item as raw text -- move the line break earlier so the code span starts this line (see this file's header)", rel, line))
		}
	}

	if pages < 100 {
		t.Fatalf("VACUOUS: only %d docs pages matched; the guard cannot have scanned the site", pages)
	}

	if len(problems) > 0 {
		t.Fatalf("%d docs page problem(s) under GitHub Pages' kramdown:\n  %s", len(problems), strings.Join(problems, "\n  "))
	}
}
