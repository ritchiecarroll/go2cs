// docsFragmentLinks_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package repoguard

import (
	"fmt"
	"net/url"
	"os"
	"path"
	"path/filepath"
	"regexp"
	"sort"
	"strings"
	"testing"
	"unicode"
)

// A docs link's #fragment must name an anchor on the page it targets. The site check
// (.github/site-check.py, in the Docs site workflow) reads the BUILT site and refuses a deploy on a broken
// fragment, but only after a master push, and only on the docs workflow. 40b216ba17 (2026-10-14)
// restructured ConversionStrategies-Reference/comments.md while both reference indexes still linked its
// two removed headings, so every docs build failed for two days while repoguard read green. 4898b619bd
// repointed the links.
//
// WHAT IS CHECKED: the same rule as site-check.py's fragment pass, read from the Markdown sources so it
// fails before a push. A source is every page publishedMarkdownPages returns. Its links are inline links,
// reference definitions and raw HTML hrefs, with fenced code, inline code spans and HTML comments removed
// first, since the site strips comments before its check (.github/site-strip-comments.py). A link whose
// target is a published docs page (.md, its .html twin, or a directory's index), or the page itself for a
// bare #fragment, must name one of that page's anchors, or `top`.
//
// THE ANCHORS OF A PAGE (fragmentAnchors), as kramdown builds them on GitHub Pages (input GFM,
// auto_ids on): each heading's id is kramdown-parser-gfm's generate_gfm_header_id of its source text,
// which lowercases it, drops every character that is not a letter, mark, digit, connector, '-' or ' ', and
// turns each space into '-'. A repeated id takes -1, -2, ... in page order. An explicit `{#id}` or a
// following `{: #id}` replaces it. Any raw `id="..."`, and a `name="..."` on an <a>, adds one.
//
// KNOWN FINDINGS (docsFragmentKnown) follow site-check.py's baseline semantics: each is excused while it
// still fails, a row that no longer fails is itself a failure, and a second copy of a known finding is
// new. The list is empty: the site's own baseline holds no fragment finding.
//
// POSITIVE CONTROL: verified red-first when this guard landed. 4898b619bd reverted in the working tree
// failed TestDocsFragmentLinksResolve on exactly its 4 fragments, #the-rule and #what-is-still-deferred
// from each reference index; restored, the page set read clean.

// docsFragmentKnown counts the known fragment findings by key, page<TAB>href.
var docsFragmentKnown = map[string]int{}

var (
	fragmentAtx       = regexp.MustCompile(`^ {0,3}(#{1,6})(?:[ \t]+(.*?))?(?:[ \t]+#+)?[ \t]*$`)
	fragmentSetext    = regexp.MustCompile(`^ {0,3}(=+|-+)[ \t]*$`)
	fragmentHeadIAL   = regexp.MustCompile(`[ \t]*\{:?[ \t]*#([^\s}]+)[^}]*\}[ \t]*$`)
	fragmentBlockIAL  = regexp.MustCompile(`^ {0,3}\{:[ \t]*#([^\s}]+)[^}]*\}[ \t]*$`)
	fragmentHTMLID    = regexp.MustCompile(`(?i)<[a-z][^>]*?\sid\s*=\s*["']([^"']+)["']`)
	fragmentAName     = regexp.MustCompile(`(?i)<a\s[^>]*?\bname\s*=\s*["']([^"']+)["']`)
	fragmentRefDef    = regexp.MustCompile(`(?m)^ {0,3}\[[^\]]+\]:[ \t]*<?([^\s>]+)>?`)
	fragmentHref      = regexp.MustCompile(`(?i)\shref\s*=\s*["']([^"']*)["']`)
	fragmentCodeSpan  = regexp.MustCompile("(`+).+?(`+)")
	fragmentComment   = regexp.MustCompile(`(?s)<!--.*?-->`)
	fragmentNotSlugCh = func(r rune) bool {
		return !(unicode.IsLetter(r) || unicode.Is(unicode.M, r) || unicode.IsDigit(r) || unicode.Is(unicode.Pc, r) || r == '-' || r == ' ')
	}
)

// fragmentReadable is a page's text with what the site never reads blanked, line numbers kept: fenced
// code (siteBlankFencedCode) and HTML comments (stripped from the built pages before the check).
func fragmentReadable(text string) string {
	text = strings.ReplaceAll(text, "\r\n", "\n")
	text = fragmentComment.ReplaceAllStringFunc(text, func(c string) string {
		return strings.Repeat("\n", strings.Count(c, "\n"))
	})

	return siteBlankFencedCode(text)
}

// fragmentSlug is kramdown-parser-gfm's generate_gfm_header_id before its de-duplication.
func fragmentSlug(raw string) string {
	s := strings.Map(func(r rune) rune {
		if fragmentNotSlugCh(r) {
			return -1
		}

		return r
	}, strings.ToLower(raw))

	return strings.ReplaceAll(s, " ", "-")
}

// fragmentAnchors are a page's anchors, built as kramdown builds them (see the header).
func fragmentAnchors(text string) map[string]bool {
	lines := strings.Split(fragmentReadable(text), "\n")
	ids := map[string]bool{}
	seen := map[string]int{}

	heading := func(raw string, next string) {
		id := ""

		if m := fragmentHeadIAL.FindStringSubmatch(raw); m != nil {
			id, raw = m[1], raw[:len(raw)-len(m[0])]
		} else if m := fragmentBlockIAL.FindStringSubmatch(next); m != nil {
			id = m[1]
		}

		slug := fragmentSlug(strings.TrimSpace(raw))
		n := seen[slug]
		seen[slug] = n + 1

		if id == "" {
			id = slug

			if n > 0 {
				id = fmt.Sprintf("%s-%d", slug, n)
			}
		}

		ids[id] = true
	}

	for i, line := range lines {
		next := ""

		if i+1 < len(lines) {
			next = lines[i+1]
		}

		if m := fragmentAtx.FindStringSubmatch(line); m != nil {
			heading(m[2], next)
		} else if strings.TrimSpace(line) != "" && !strings.HasPrefix(strings.TrimSpace(line), "|") && i+1 < len(lines) &&
			fragmentSetext.MatchString(next) && (i == 0 || strings.TrimSpace(lines[i-1]) == "" || fragmentAtx.MatchString(lines[i-1])) {
			after := ""

			if i+2 < len(lines) {
				after = lines[i+2]
			}

			heading(line, after)
		}

		for _, m := range fragmentHTMLID.FindAllStringSubmatch(line, -1) {
			ids[m[1]] = true
		}

		for _, m := range fragmentAName.FindAllStringSubmatch(line, -1) {
			ids[m[1]] = true
		}
	}

	return ids
}

// fragmentRef is one link with a fragment: where it is, what it says, and the page it targets.
type fragmentRef struct {
	line     int
	href     string
	target   string
	fragment string
}

// fragmentRefs are a page's links that carry a #fragment into a docs page; isPage answers whether a
// docs-relative path is a published page.
func fragmentRefs(rel, text string, isPage func(string) bool) []fragmentRef {
	var out []fragmentRef

	for i, line := range strings.Split(fragmentReadable(text), "\n") {
		code := fragmentCodeSpan.ReplaceAllStringFunc(line, func(c string) string { return strings.Repeat(" ", len(c)) })
		var hrefs []string

		for _, m := range siteInlineLink.FindAllStringSubmatch(code, -1) {
			target := strings.TrimSpace(m[2])

			if f := strings.Fields(target); len(f) > 0 {
				target = strings.Trim(f[0], "<>")
			}

			hrefs = append(hrefs, target)
		}

		for _, m := range fragmentRefDef.FindAllStringSubmatch(code, -1) {
			hrefs = append(hrefs, m[1])
		}

		for _, m := range fragmentHref.FindAllStringSubmatch(code, -1) {
			hrefs = append(hrefs, m[1])
		}

		for _, href := range hrefs {
			p, frag, found := strings.Cut(href, "#")

			if !found || frag == "" || strings.Contains(p, ":") || strings.HasPrefix(p, "//") {
				continue
			}

			target := rel

			if p != "" {
				target = fragmentTarget(path.Join(path.Dir(rel), p), strings.HasSuffix(p, "/"), isPage)

				if target == "" {
					continue // not a published docs page: the site's broken-link pass owns it
				}
			}

			if f, err := url.PathUnescape(frag); err == nil {
				frag = f
			}

			out = append(out, fragmentRef{line: i + 1, href: href, target: target, fragment: frag})
		}
	}

	return out
}

// fragmentTarget is the published page a joined link path serves, or "".
func fragmentTarget(p string, dir bool, isPage func(string) bool) string {
	var cands []string

	switch {
	case dir || path.Ext(p) == "":
		cands = []string{p + "/index.md", p + "/README.md", p + ".md"}
	case strings.HasSuffix(p, ".html"):
		stem := strings.TrimSuffix(p, ".html")
		cands = []string{stem + ".md"}

		if path.Base(stem) == "index" {
			cands = append(cands, path.Dir(stem)+"/README.md")
		}
	default:
		cands = []string{p}
	}

	for _, c := range cands {
		if strings.HasPrefix(c, "docs/") && isPage(c) {
			return c
		}
	}

	return ""
}

// docsFragmentProblems is the rule over a set of pages: the links it checked, and the broken ones keyed
// page<TAB>href, with where each sits.
func docsFragmentProblems(pages map[string]string, isPage func(string) bool, read func(string) (string, bool)) (checked int, broken map[string][]string) {
	broken = map[string][]string{}
	anchors := map[string]map[string]bool{}

	for rel, text := range pages {
		for _, r := range fragmentRefs(rel, text, isPage) {
			checked++
			ids, ok := anchors[r.target]

			if !ok {
				if body, found := pages[r.target]; found {
					ids = fragmentAnchors(body)
				} else if body, found := read(r.target); found {
					ids = fragmentAnchors(body)
				}

				anchors[r.target] = ids
			}

			if r.fragment != "top" && !ids[r.fragment] {
				key := rel + "\t" + r.href
				broken[key] = append(broken[key], fmt.Sprintf("%s:%d: `%s` -- %s has no anchor #%s", rel, r.line, r.href, r.target, r.fragment))
			}
		}
	}

	return checked, broken
}

// TestFragmentRuleReadsEachShape pins the anchors and the links, one shape per case.
func TestFragmentRuleReadsEachShape(t *testing.T) {
	target := strings.Join([]string{
		"# The Title",
		"## `unsafe.Pointer` and `uintptr`",
		"## Repeated",
		"## Repeated",
		"## Custom {#custom-id}",
		"Setext Heading",
		"--------------",
		"",
		"## Block IAL",
		"{: #block-id}",
		`<a id="raw-anchor"></a>`,
		`<a name="named-anchor"></a>`,
		"## Ünïcode ж — dash",
		"```",
		"## not a heading",
		"```",
		"<!-- <a id=\"commented\"></a> -->",
	}, "\n")
	pages := map[string]string{"docs/t.md": target, "docs/sub/README.md": "# Sub Page\n"}
	isPage := func(p string) bool { _, ok := pages[p]; return ok }
	read := func(p string) (string, bool) { s, ok := pages[p]; return s, ok }

	cases := []struct {
		name, link string
		ok         bool
	}{
		{"an H1", "[x](t.md#the-title)", true},
		{"code spans and dots drop", "[x](t.md#unsafepointer-and-uintptr)", true},
		{"the first of a repeat", "[x](t.md#repeated)", true},
		{"the second of a repeat", "[x](t.md#repeated-1)", true},
		{"a third that is not there", "[x](t.md#repeated-2)", false},
		{"an explicit id", "[x](t.md#custom-id)", true},
		{"the slug an explicit id replaced", "[x](t.md#custom)", false},
		{"a setext heading", "[x](t.md#setext-heading)", true},
		{"a block IAL", "[x](t.md#block-id)", true},
		{"a raw id", "[x](t.md#raw-anchor)", true},
		{"a named anchor", "[x](t.md#named-anchor)", true},
		{"letters kept, punctuation dropped", "[x](t.md#ünïcode-ж--dash)", true},
		{"a heading in fenced code", "[x](t.md#not-a-heading)", false},
		{"an id in a comment", "[x](t.md#commented)", false},
		{"the .html twin", "[x](t.html#the-title)", true},
		{"top", "[x](t.md#top)", true},
		{"a directory's README", "[x](sub/#sub-page)", true},
		{"a reference definition", "[x]: t.md#gone", false},
		{"a raw href", `<a href="t.md#gone">x</a>`, false},
		{"an escaped fragment", "[x](t.md#%C3%BCn%C3%AFcode-%D0%B6--dash)", true},
		{"a link in a code span is not read", "`[x](t.md#gone)`", true},
		{"a link in a comment is not read", "<!-- [x](t.md#gone) -->", true},
		{"a page outside the site is not read", "[x](../src/x.md#gone)", true},
		{"an external link is not read", "[x](https://example.com/t.md#gone)", true},
	}

	for _, c := range cases {
		src := map[string]string{"docs/src.md": "# Source\n\n" + c.link + "\n"}
		_, broken := docsFragmentProblems(src, isPage, read)

		if (len(broken) == 0) != c.ok {
			t.Errorf("%s: %q broken=%v, want ok=%v", c.name, c.link, broken, c.ok)
		}
	}

	// A bare #fragment targets its own page, and the line it is on is named.
	_, broken := docsFragmentProblems(map[string]string{"docs/src.md": "# Source\n\nsee [x](#source) and [y](#nope)\n"}, isPage, read)

	if len(broken) != 1 || !strings.Contains(strings.Join(broken["docs/src.md\t#nope"], ""), "docs/src.md:3:") {
		t.Errorf("same-page fragments: got %v, want only #nope, at line 3", broken)
	}
}

// TestDocsFragmentLinksResolve is the guard over the published pages.
func TestDocsFragmentLinksResolve(t *testing.T) {
	root := repoRootFromPackageDir(t)
	pages := publishedMarkdownPages(t)
	tracked := map[string]bool{}

	for _, p := range gitTrackedFiles(t, root) {
		tracked[p] = true
	}

	isPage := func(p string) bool {
		return tracked[p] && strings.HasSuffix(p, ".md") && jekyllRendersThroughLiquid(strings.TrimPrefix(p, "docs/"), nil)
	}

	read := func(p string) (string, bool) {
		if s, ok := pages[p]; ok {
			return s, true
		}

		data, err := os.ReadFile(filepath.Join(root, filepath.FromSlash(p)))

		return string(data), err == nil
	}

	checked, broken := docsFragmentProblems(pages, isPage, read)

	if checked < 500 {
		t.Fatalf("VACUOUS: only %d fragment link(s) checked; the scanner cannot have read the pages' links", checked)
	}

	var problems []string
	var keys []string

	for k := range broken {
		keys = append(keys, k)
	}

	for k := range docsFragmentKnown {
		if _, ok := broken[k]; !ok {
			keys = append(keys, k)
		}
	}

	sort.Strings(keys)

	for _, k := range keys {
		at, known := broken[k], docsFragmentKnown[k]

		switch {
		case len(at) > known:
			problems = append(problems, at[known:]...)
		case len(at) < known:
			problems = append(problems, fmt.Sprintf("docsFragmentKnown row %q is listed %d time(s) but fails %d: shrink it", k, known, len(at)))
		}
	}

	t.Logf("%d fragment link(s) checked on %d page(s), %d known", checked, len(pages), len(docsFragmentKnown))

	if len(problems) > 0 {
		sort.Strings(problems)
		t.Fatalf("%d docs link(s) name a #fragment their target page does not have (the site check refuses the deploy on each); point each at a heading that exists, or keep the old slug as an <a id> on the target:\n  %s",
			len(problems), strings.Join(problems, "\n  "))
	}
}
