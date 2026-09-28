// docsKramdownBlocks_test.go - Gbtc
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
	"sort"
	"strconv"
	"strings"
	"testing"
)

// Two block rules on which GitHub Pages' kramdown 2.4 and GitHub's renderer disagree, both invisible
// on GitHub (docs/phase4/briefs/pages-audit-2026-09-27.md, items 9 and 10).
//
//   - A WRAPPED "N. " LINE BECOMES A LIST. CommonMark lets only "1." interrupt a paragraph; kramdown
//     lets any number do it, and it renumbers from 1. "...where Windows offers\n61. The Tests..."
//     published "1." in place of 61. The fix ends the previous line with the number.
//   - A LINE WITH A BARE '|' BECOMES A TABLE. kramdown makes a table from a block in which every line
//     has an unescaped '|' outside code, with no delimiter row needed; GitHub needs the delimiter row.
//     A table-of-contents link to "The string | []byte union" was split into two cells. The fix is a
//     code span, an escape, or a real header row, whichever reads best on both renderers.
//
// SCOPE: the LIVING pages. The write-once release snapshots (frozenSnapshot) and the records under
// docs/phase4/ and docs/doctrine/ are never rewritten; their instances stay as they are.
//
// POSITIVE CONTROLS, verified when this guard landed: restore ValidatedTestPackages.md's "offers\n61."
// wrap and TestDocsNoWrappedOrderedListStart names it; restore manual-conversions.md's bare
// "address|locked" and TestDocsNoPipeOnlyTable names it.

// blockRecordDirs are the docs\ directories whose pages are records, never rewritten.
var blockRecordDirs = map[string]string{
	"docs/phase4/":   "point-in-time records, the BOARD and the briefs",
	"docs/doctrine/": "the journal, whose blob identity TestContextBudget enforces",
}

var (
	blockFence        = regexp.MustCompile("^[ \t]*(`{3,}|~{3,})")
	blockQuotePrefix  = regexp.MustCompile(`^(?:[ \t]{0,3}>[ \t]?)+`)
	blockOrderedStart = regexp.MustCompile(`^( *)(\d{1,9})\.[ \t|]`)
	blockDelimiterRow = regexp.MustCompile(`^[ \t]*\|?[ \t]*:?-+:?[ \t]*(?:\|[ \t]*:?-+:?[ \t]*)*\|?[ \t]*$`)
	blockNotParagraph = regexp.MustCompile(`^[ \t]*(?:#|\||<|[-*+][ \t]|[-*_]{3,}[ \t]*$|=+[ \t]*$)`)
	blockBacktickRun  = regexp.MustCompile("`+")
	blockListItem     = regexp.MustCompile(`^[ \t]*(?:[-*+]|\d{1,9}[.)])[ \t]+`)
	blockATXHeading   = regexp.MustCompile(`^ {0,3}#{1,6}(?:[ \t]|$)`)
)

// blockLines returns each line with its blockquote prefix stripped and that prefix's depth, and marks
// the lines inside fenced code or an HTML comment, which neither rule reads.
func blockLines(text string) (lines []string, depth []int, skip []bool) {
	lines = strings.Split(strings.ReplaceAll(text, "\r\n", "\n"), "\n")
	depth = make([]int, len(lines))
	skip = make([]bool, len(lines))
	fence, comment := "", false

	for i, raw := range lines {
		prefix := blockQuotePrefix.FindString(raw)
		depth[i] = strings.Count(prefix, ">")
		line := raw[len(prefix):]
		lines[i] = line

		if comment {
			skip[i] = true
			comment = !strings.Contains(line, "-->")
			continue
		}

		if m := blockFence.FindStringSubmatch(line); m != nil {
			switch {
			case fence == "":
				fence = m[1]
			case m[1][0] == fence[0] && len(m[1]) >= len(fence):
				fence = ""
			}

			skip[i] = true
			continue
		}

		if fence != "" {
			skip[i] = true
			continue
		}

		if t := strings.TrimSpace(line); strings.HasPrefix(t, "<!--") {
			skip[i] = true
			comment = !strings.Contains(t[4:], "-->")
		}
	}

	return lines, depth, skip
}

// blockOutsideCode removes CommonMark code spans (a backtick run closed by a run of the same length,
// which may cross lines; a removed span keeps its line breaks) and escaped pipes, leaving the text a
// table parser splits on.
func blockOutsideCode(line string) string {
	line = strings.ReplaceAll(line, `\|`, "")
	var b strings.Builder
	runs := blockBacktickRun.FindAllStringIndex(line, -1)
	at := 0

	for i := 0; i < len(runs); i++ {
		n := runs[i][1] - runs[i][0]
		closed := -1

		for j := i + 1; j < len(runs); j++ {
			if runs[j][1]-runs[j][0] == n {
				closed = j
				break
			}
		}

		if closed < 0 {
			continue
		}

		b.WriteString(line[at:runs[i][0]])
		b.WriteString(strings.Repeat("\n", strings.Count(line[runs[i][0]:runs[closed][1]], "\n")))
		at = runs[closed][1]
		i = closed
	}

	b.WriteString(line[at:])

	return b.String()
}

func blockIndent(line string) int { return len(line) - len(strings.TrimLeft(line, " ")) }

// wrappedOrderedListStarts returns the 1-based lines that kramdown opens as a list and GitHub keeps as
// paragraph text: "N. " with N != 1, right after paragraph text, indented at least as far as it.
func wrappedOrderedListStarts(text string) []int {
	lines, depth, skip := blockLines(text)
	var hits []int

	for i := 1; i < len(lines); i++ {
		if skip[i] || skip[i-1] || depth[i] != depth[i-1] {
			continue
		}

		m := blockOrderedStart.FindStringSubmatch(lines[i])

		if m == nil {
			continue
		}

		if n, _ := strconv.Atoi(m[2]); n == 1 {
			continue
		}

		prev := lines[i-1]

		if strings.TrimSpace(prev) == "" || blockNotParagraph.MatchString(prev) || blockOrderedStart.MatchString(prev) {
			continue
		}

		if blockIndent(lines[i]) >= blockIndent(prev) {
			hits = append(hits, i+1)
		}
	}

	return hits
}

// pipeOnlyTables returns the first line of every block in which each line has a '|' outside code and
// no line is a delimiter row: a table to kramdown, text to GitHub. A block ends at a blank line, and a
// list item starts one of its own (kramdown parses an item's text as its own blocks). An ATX heading is
// never a table: kramdown takes the heading first.
func pipeOnlyTables(text string) []int {
	lines, _, skip := blockLines(text)
	var hits []int

	for i := 0; i < len(lines); {
		if skip[i] || strings.TrimSpace(lines[i]) == "" || blockATXHeading.MatchString(lines[i]) {
			i++
			continue
		}

		start := i

		for i++; i < len(lines) && !skip[i] && strings.TrimSpace(lines[i]) != "" && !blockListItem.MatchString(lines[i]) && !blockATXHeading.MatchString(lines[i]); i++ {
		}

		allPipes, delimiter := true, false

		for _, line := range strings.Split(blockOutsideCode(strings.Join(lines[start:i], "\n")), "\n") {
			if blockDelimiterRow.MatchString(line) && strings.Contains(line, "-") {
				delimiter = true
			}

			if !strings.Contains(line, "|") {
				allPipes = false
			}
		}

		if allPipes && !delimiter {
			hits = append(hits, start+1)
		}
	}

	return hits
}

// TestKramdownBlockRulesMatchKramdown pins both rules, one shape per case.
func TestKramdownBlockRulesMatchKramdown(t *testing.T) {
	lists := []struct {
		name string
		text string
		want []int
	}{
		{"a wrapped number opens a list", "Windows offers\n61. The Tests column\n", []int{2}},
		{"the fix: the number ends the line", "Windows offers 61.\nThe Tests column\n", nil},
		{"1. may interrupt on both renderers", "text\n1. first\n", nil},
		{"after a blank line it is a real list", "text\n\n2. second\n", nil},
		{"a list continues a list", "1. one\n2. two\n", nil},
		{"less indented than the paragraph", "    text\n3. item\n", nil},
		{"after a heading", "## H\n2. x\n", nil},
		{"inside a blockquote", "> offers\n> 61. The\n", []int{2}},
		{"fenced code is not read", "```\ntext\n2. x\n```\n", nil},
		{"a pipe after the dot", "text\n5.| x\n", []int{2}},
		{"a sibling item after a quote inside the item above", "1. one\n   > quoted\n2. two\n", nil},
	}

	for _, c := range lists {
		if got := wrappedOrderedListStarts(c.text); fmt.Sprint(got) != fmt.Sprint(c.want) {
			t.Errorf("list rule, %s: got %v, want %v", c.name, got, c.want)
		}
	}

	tables := []struct {
		name string
		text string
		want []int
	}{
		{"a bare pipe in a one-line block", "an address|locked heading\n", []int{1}},
		{"the fix: a code span", "an `address|locked` heading\n", nil},
		{"an escaped pipe", `an address\|locked heading` + "\n", nil},
		{"a real table has a delimiter row", "| a | b |\n|---|---|\n| 1 | 2 |\n", nil},
		{"one line without a pipe saves the block", "a | b\nplain text\n", nil},
		{"every line piped, no delimiter", "a | b\nc | d\n", []int{1}},
		{"a quoted row", "> | x | y |\n", []int{1}},
		{"a double-backtick span", "use ``a|b`` here\n", nil},
		{"an unclosed backtick does not hide the pipe", "a ` b | c\n", []int{1}},
		{"fenced code is not read", "```\na | b\n```\n", nil},
		{"an HTML comment is not read", "<!-- a | b -->\n", nil},
		{"the second block is named", "text\n\nx | y\n", []int{3}},
		{"a list item is its own block", "- plain item\n- [The string | byte union](a.md)\n- plain item\n", []int{2}},
		{"an ordered item with a bare pipe", "2. text\n3. **|C| accounting**\n", []int{2}},
		{"an ATX heading is never a table", "## banks at 5 | 0\n", nil},
		{"a code span that crosses the rows", "> `| a | b\n> c |`\n", nil},
	}

	for _, c := range tables {
		if got := pipeOnlyTables(c.text); fmt.Sprint(got) != fmt.Sprint(c.want) {
			t.Errorf("table rule, %s: got %v, want %v", c.name, got, c.want)
		}
	}
}

// livingMarkdownPages returns every tracked docs\ Markdown page kramdown renders, outside the frozen
// snapshots and the record directories.
func livingMarkdownPages(t *testing.T) map[string]string {
	t.Helper()

	root := repoRootFromPackageDir(t)
	tracked := gitTrackedFiles(t, root)

	if len(tracked) < 1000 {
		t.Fatalf("git ls-files returned %d paths, too few to be this repository; a guard that scans nothing passes everything", len(tracked))
	}

	pages := map[string]string{}

	for _, rel := range tracked {
		if !strings.HasPrefix(rel, "docs/") || frozenSnapshot.MatchString(rel) || !jekyllRendersThroughLiquid(strings.TrimPrefix(rel, "docs/"), nil) {
			continue
		}

		record := false

		for dir := range blockRecordDirs {
			record = record || strings.HasPrefix(rel, dir)
		}

		if record {
			continue
		}

		data, err := os.ReadFile(filepath.Join(root, filepath.FromSlash(rel)))

		if err != nil {
			t.Fatalf("cannot read %s: %v", rel, err)
		}

		pages[rel] = string(data)
	}

	if len(pages) < 100 {
		t.Fatalf("VACUOUS: only %d living docs pages matched; the guard cannot have scanned the site", len(pages))
	}

	return pages
}

// TestDocsNoWrappedOrderedListStart is the item-9 guard over the living pages.
func TestDocsNoWrappedOrderedListStart(t *testing.T) {
	var problems []string

	for rel, text := range livingMarkdownPages(t) {
		for _, line := range wrappedOrderedListStarts(text) {
			problems = append(problems, fmt.Sprintf("%s:%d: a wrapped line starting \"N. \" becomes a list renumbered from 1 on the site -- end the previous line with the number (see this file's header)", rel, line))
		}
	}

	sort.Strings(problems)

	if len(problems) > 0 {
		t.Fatalf("%d wrapped ordered-list start(s):\n  %s", len(problems), strings.Join(problems, "\n  "))
	}
}

// TestDocsNoPipeOnlyTable is the item-10 guard over the living pages.
func TestDocsNoPipeOnlyTable(t *testing.T) {
	var problems []string

	for rel, text := range livingMarkdownPages(t) {
		for _, line := range pipeOnlyTables(text) {
			problems = append(problems, fmt.Sprintf("%s:%d: every line of this block has a bare '|' and none is a delimiter row, so the site makes it a table -- use a code span, an escape or a header row (see this file's header)", rel, line))
		}
	}

	sort.Strings(problems)

	if len(problems) > 0 {
		t.Fatalf("%d pipe-only table block(s):\n  %s", len(problems), strings.Join(problems, "\n  "))
	}
}
