// docsSiteLinksTitles_test.go - Gbtc
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
	"strings"
	"testing"
)

// Two GitHub Pages plugins read the Markdown SOURCE with single-line regexes, so a line break GitHub's
// renderer ignores changes what the site publishes (docs/phase4/briefs/pages-audit-2026-09-27.md,
// items 12 and 17).
//
//   - jekyll-relative-links 0.6.1 rewrites a link to a relative .md target into the page's .html URL.
//     Its LINK_TEXT_REGEX is %r!(.*?)! with no /m flag, so link text that spans a line break is never
//     rewritten, and the published link lands on the raw .md copy (served as text/markdown). The fix
//     moves the break to just BEFORE the '['. The target stays .md, since .html 404s on GitHub.
//   - jekyll-titles-from-headings 0.5.3 takes a page's title from a heading only at the very start
//     of its content: TITLE_REGEX is \A\s* then an atx heading (#, ## or ###) or a setext heading.
//     Anything before the first heading (a logo, a blockquote, an HTML comment) leaves the page with
//     the site's default title, in the browser tab and the banner.
//
// POSITIVE CONTROLS, verified when these guards landed: rewrap NEWS.md's first fixed link back
// across its line break and TestDocsLinkTextDoesNotWrap names that line; put NEWS.md's logo back
// above its H1 and TestDocsPagesTitleFromTheirFirstHeading names NEWS.md.

// siteLinkFence opens or closes fenced code, backtick or tilde, after any blockquote, list or indent
// prefix.
var siteLinkFence = regexp.MustCompile("^(?:[ \t]*(?:>[ \t]?|(?:[-*+]|\\d{1,9}[.)])[ \t]+))*[ \t]*(`{3,}|~{3,})")

// siteInlineLink is an inline link whose text may hold one level of nested brackets (a code span
// with an index, say), and whose target runs to the first ')'.
var siteInlineLink = regexp.MustCompile(`\[((?:[^\[\]]|\[[^\[\]]*\])*)\]\(([^)]*)\)`)

// siteRelativeMarkdown is a relative .md target, with or without a #fragment.
var siteRelativeMarkdown = regexp.MustCompile(`(?i)^[^:/#\s][^:\s]*\.md(?:#\S*)?$`)

// siteTitleFromHeadings is jekyll-titles-from-headings' TITLE_REGEX, with Ruby's line-anchored '$'.
var siteTitleFromHeadings = regexp.MustCompile(`(?m)\A\s*(?:#{1,3}\s+(.*)|(.*)\r?\n[-=]+\s*)$`)

// siteFrontMatter is YAML front matter, which Jekyll strips before the plugin reads the content.
var siteFrontMatter = regexp.MustCompile(`(?s)\A---[ \t]*\r?\n(.*?)\r?\n---[ \t]*\r?\n`)

// siteBlankFencedCode replaces every line inside fenced code with an empty line, so line numbers
// survive and nothing inside a fence is read as a link.
func siteBlankFencedCode(text string) string {
	lines := strings.Split(text, "\n")
	fence := ""

	for i, line := range lines {
		m := siteLinkFence.FindStringSubmatch(line)

		if m != nil {
			switch {
			case fence == "":
				fence = m[1]
			case m[1][0] == fence[0] && len(m[1]) >= len(fence):
				fence = ""
			}

			lines[i] = ""
			continue
		}

		if fence != "" {
			lines[i] = ""
		}
	}

	return strings.Join(lines, "\n")
}

// wrappedRelativeMarkdownLinks returns the 1-based line of the '[' of every inline link outside fenced
// code whose target is a relative .md path and whose text spans a line break.
func wrappedRelativeMarkdownLinks(text string) []int {
	text = siteBlankFencedCode(strings.ReplaceAll(text, "\r\n", "\n"))

	var hits []int

	for _, m := range siteInlineLink.FindAllStringSubmatchIndex(text, -1) {
		label, target := text[m[2]:m[3]], strings.TrimSpace(text[m[4]:m[5]])

		if strings.Contains(label, "\n") && siteRelativeMarkdown.MatchString(target) {
			hits = append(hits, strings.Count(text[:m[0]], "\n")+1)
		}
	}

	return hits
}

// pageHasHeadingTitle reports whether jekyll-titles-from-headings finds a title for the page: its
// front matter names one, or its content opens with a heading.
func pageHasHeadingTitle(text string) bool {
	text = strings.TrimPrefix(text, "\xef\xbb\xbf")

	if m := siteFrontMatter.FindStringSubmatchIndex(text); m != nil {
		if regexp.MustCompile(`(?m)^title:`).MatchString(text[m[2]:m[3]]) {
			return true
		}

		text = text[m[1]:]
	}

	return siteTitleFromHeadings.MatchString(text)
}

// TestSiteLinkAndTitleRulesMatchThePlugins pins both emulations, one shape per case.
func TestSiteLinkAndTitleRulesMatchThePlugins(t *testing.T) {
	links := []struct {
		name string
		text string
		want []int
	}{
		{"link text across a line break", "see [the\nnews post](news/a.md) today\n", []int{1}},
		{"the fix: the break before the '['", "see\n[the news post](news/a.md) today\n", nil},
		{"a fragment on the target", "x [a\nb](ref/page.md#anchor)\n", []int{1}},
		{"a code span with brackets in the text", "x [`s[0]`\nhere](a.md)\n", []int{1}},
		{"an absolute URL is not rewritten", "x [a\nb](https://example.com/a.md)\n", nil},
		{"a site-absolute path is not relative", "x [a\nb](/docs/a.md)\n", nil},
		{"a fragment-only target", "x [a\nb](#a.md)\n", nil},
		{"not a .md target", "x [a\nb](images/a.png)\n", nil},
		{"fenced code is not read", "```\n[a\nb](a.md)\n```\n", nil},
		{"the line of the '[' is named", "one\ntwo [a\nb](a.md)\n", []int{2}},
	}

	for _, c := range links {
		if got := wrappedRelativeMarkdownLinks(c.text); fmt.Sprint(got) != fmt.Sprint(c.want) {
			t.Errorf("link rule, %s: got %v, want %v", c.name, got, c.want)
		}
	}

	titles := []struct {
		name string
		text string
		want bool
	}{
		{"an H1 first", "# Title\n\ntext\n", true},
		{"blank lines before the H1", "\n\n# Title\n", true},
		{"a BOM before the H1", "\xef\xbb\xbf# Title\n", true},
		{"an H3 counts", "### Title\n", true},
		{"an H4 does not", "#### Title\n", false},
		{"a setext heading", "Title\n=====\n\ntext\n", true},
		{"a logo before the H1", "![go2cs](images/go2cs-small.png)\n\n# Title\n", false},
		{"a comment before the H1", "<!-- note -->\n\n# Title\n", false},
		{"a blockquote before the H1", "> moved\n\n# Title\n", false},
		{"front matter with a title", "---\ntitle: T\n---\ntext\n", true},
		{"front matter without a title, then the H1", "---\nlayout: x\n---\n# Title\n", true},
		{"CRLF", "# Title\r\n\r\ntext\r\n", true},
	}

	for _, c := range titles {
		if got := pageHasHeadingTitle(c.text); got != c.want {
			t.Errorf("title rule, %s: got %v, want %v", c.name, got, c.want)
		}
	}
}

// publishedMarkdownPages returns every tracked docs\ Markdown page kramdown renders, with its bytes,
// skipping the write-once release snapshots.
func publishedMarkdownPages(t *testing.T) map[string]string {
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

		data, err := os.ReadFile(filepath.Join(root, filepath.FromSlash(rel)))

		if err != nil {
			t.Fatalf("cannot read %s: %v", rel, err)
		}

		pages[rel] = string(data)
	}

	if len(pages) < 100 {
		t.Fatalf("VACUOUS: only %d docs pages matched; the guard cannot have scanned the site", len(pages))
	}

	return pages
}

// TestDocsLinkTextDoesNotWrap is the link guard over the committed tree.
func TestDocsLinkTextDoesNotWrap(t *testing.T) {
	var problems []string

	for rel, text := range publishedMarkdownPages(t) {
		for _, line := range wrappedRelativeMarkdownLinks(text) {
			problems = append(problems, fmt.Sprintf("%s:%d: link text spans a line break, so jekyll-relative-links never rewrites it and the site serves the raw .md -- move the break to just before the '[' (see this file's header)", rel, line))
		}
	}

	sort.Strings(problems)

	if len(problems) > 0 {
		t.Fatalf("%d wrapped link(s):\n  %s", len(problems), strings.Join(problems, "\n  "))
	}
}

// TestDocsPagesTitleFromTheirFirstHeading is the title guard over the committed tree. docs/README.md
// is the home page, whose site-default title is intended.
func TestDocsPagesTitleFromTheirFirstHeading(t *testing.T) {
	var problems []string

	for rel, text := range publishedMarkdownPages(t) {
		if rel != "docs/README.md" && !pageHasHeadingTitle(text) {
			problems = append(problems, fmt.Sprintf("%s: something precedes the first heading, so the page publishes with the site's default title -- put the H1 first (see this file's header)", rel))
		}
	}

	sort.Strings(problems)

	if len(problems) > 0 {
		t.Fatalf("%d page(s) without a heading title:\n  %s", len(problems), strings.Join(problems, "\n  "))
	}
}
