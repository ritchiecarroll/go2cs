// docsExcludedLinks_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package repoguard

import (
	"os"
	"path"
	"path/filepath"
	"regexp"
	"sort"
	"strconv"
	"strings"
	"testing"
)

// A page the site publishes must not link a file the site excludes. docs/_config.yml's exclude list keeps the
// internal records (plans, designs, runbooks, the phase3/phase4/doctrine trees) off go2cs.net, and the owner's ruling
// of 2026-10-26 (OQ-1) is that nothing reachable from go2cs.net carries the fleet's process. An excluded file is still
// one click away when a published page links it: .github/site-outside-links.py rewrites such a link to the file's
// GitHub copy rather than dropping it. So the exclusion only holds while no published page links into it.
//
// WHAT IS CHECKED: every page publishedMarkdownPages returns that the exclude list does not remove (the frozen release
// snapshots under docs/validation/<version>/ are outside that set already: they are write-once records, and a link
// inside one is history). Its links are read as the fragment guard reads them (inline links, reference definitions and
// raw hrefs, with fenced code, code spans and HTML comments removed, since the site strips comments). A relative link
// that resolves to a docs/ path the exclude list names is a finding. A record's link may still sit inside an HTML
// comment beside the sentence it supported; that is how provenance is kept.
//
// POSITIVE CONTROL: TestExcludedLinkRuleReadsEachShape below plants each shape. Verified red-first when this guard
// landed: re-adding the visible `[Sizing M:N scheduling](DESIGN-mn-scheduler-sizing.md)` link to docs/Limitations.md
// failed TestDocsLinkNoExcludedFile naming that page, line and target; removed, the page set read clean.

var excludedConfigEntry = regexp.MustCompile(`^\s+-\s*(.+?)\s*$`)

// siteExcludePatterns is docs/_config.yml's exclude list, read as .github/site-outside-links.py reads it: a block
// list ('exclude:' then '  - item' lines).
func siteExcludePatterns(config string) []string {
	var items []string
	inBlock := false

	for _, line := range strings.Split(strings.ReplaceAll(config, "\r\n", "\n"), "\n") {
		text, _, _ := strings.Cut(line, "#")
		text = strings.TrimRight(text, " \t")

		if inBlock {
			if m := excludedConfigEntry.FindStringSubmatch(text); m != nil {
				items = append(items, strings.Trim(m[1], `'"`))
				continue
			}

			if strings.TrimSpace(text) == "" {
				continue
			}

			inBlock = false
		}

		if strings.TrimSpace(text) == "exclude:" {
			inBlock = true
		}
	}

	return items
}

// siteExcluded applies Jekyll's own rule: a source path is excluded when it starts with an entry.
func siteExcluded(docsRel string, patterns []string) bool {
	for _, p := range patterns {
		if strings.HasPrefix(docsRel, p) {
			return true
		}
	}

	return false
}

// excludedLinkFindings are a page's visible links into an excluded docs/ path, as "line<TAB>href".
func excludedLinkFindings(rel, text string, patterns []string) (links int, findings []string) {
	for i, line := range strings.Split(fragmentReadable(text), "\n") {
		code := fragmentCodeSpan.ReplaceAllStringFunc(line, func(c string) string { return strings.Repeat(" ", len(c)) })
		var hrefs []string

		for _, m := range siteInlineLink.FindAllStringSubmatch(code, -1) {
			if f := strings.Fields(strings.TrimSpace(m[2])); len(f) > 0 {
				hrefs = append(hrefs, strings.Trim(f[0], "<>"))
			}
		}

		for _, m := range fragmentRefDef.FindAllStringSubmatch(code, -1) {
			hrefs = append(hrefs, m[1])
		}

		for _, m := range fragmentHref.FindAllStringSubmatch(code, -1) {
			hrefs = append(hrefs, m[1])
		}

		for _, href := range hrefs {
			p, _, _ := strings.Cut(href, "#")
			p, _, _ = strings.Cut(p, "?")

			if p == "" || strings.Contains(p, ":") || strings.HasPrefix(p, "/") {
				continue
			}

			links++
			target := path.Join(path.Dir(rel), p)

			if docsRel, inDocs := strings.CutPrefix(target, "docs/"); inDocs && siteExcluded(docsRel, patterns) {
				findings = append(findings, strconv.Itoa(i+1)+"\t"+href)
			}
		}
	}

	return links, findings
}

func TestExcludedLinkRuleReadsEachShape(t *testing.T) {
	config := "exclude:\n  - phase4\n  - PLAN-   # every plan\n  - 'Runbook.md'\nplugins:\n  - jemoji\n"
	patterns := siteExcludePatterns(config)

	if strings.Join(patterns, ",") != "phase4,PLAN-,Runbook.md" {
		t.Fatalf("exclude list read as %q", patterns)
	}

	page := strings.Join([]string{
		"[a plan](PLAN-x.md) and [a record](phase4/BOARD.md#row)",   // 2 findings
		"[ok](Limitations.md) [ok](../src/core/x.cs) [ok](https://e)", // published, outside docs, absolute
		"<!-- [kept as provenance](PLAN-y.md) -->",                   // a comment is not a link
		"`[code](PLAN-z.md)`",                                       // nor is a code span
		"[ref]: Runbook.md",                                         // a reference definition is
		`<a href="phase4/x.md">raw</a>`,                             // and so is a raw href
	}, "\n")

	links, findings := excludedLinkFindings("docs/Page.md", page, patterns)

	if want := "1\tPLAN-x.md|1\tphase4/BOARD.md#row|5\tRunbook.md|6\tphase4/x.md"; strings.Join(findings, "|") != want {
		t.Fatalf("findings %q, want %q", findings, want)
	}

	if links != 6 {
		t.Fatalf("read %d links, want 6", links)
	}

	// A sub-directory page resolves its links from its own directory.
	if _, findings := excludedLinkFindings("docs/news/Item.md", "[up](../PLAN-x.md) [here](PLAN-y.md)", patterns); strings.Join(findings, "|") != "1\t../PLAN-x.md" {
		t.Fatalf("sub-directory findings %q", findings)
	}
}

func TestDocsLinkNoExcludedFile(t *testing.T) {
	root := repoRootFromPackageDir(t)
	config, err := os.ReadFile(filepath.Join(root, "docs", "_config.yml"))

	if err != nil {
		t.Fatal(err)
	}

	patterns := siteExcludePatterns(string(config))

	if len(patterns) < 3 {
		t.Fatalf("VACUOUS: docs/_config.yml's exclude list read as %q", patterns)
	}

	var problems []string
	checked, pages := 0, 0

	for rel, text := range publishedMarkdownPages(t) {
		if siteExcluded(strings.TrimPrefix(rel, "docs/"), patterns) {
			continue
		}

		pages++
		links, findings := excludedLinkFindings(rel, text, patterns)
		checked += links

		for _, f := range findings {
			at, href, _ := strings.Cut(f, "\t")
			problems = append(problems, rel+":"+at+" links "+href+", which docs/_config.yml excludes from the site")
		}
	}

	if checked < 1000 {
		t.Fatalf("VACUOUS: only %d link(s) read on %d page(s); the scanner cannot have read the pages' links", checked, pages)
	}

	sort.Strings(problems)

	for _, p := range problems {
		t.Error(p)
	}

	if len(problems) > 0 {
		t.Log("Reword the sentence to state the fact for a visitor and keep the link inside an HTML comment beside it.")
	}
}
