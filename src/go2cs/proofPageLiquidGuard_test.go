// proofPageLiquidGuard_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// proofPageLiquidGuard_test.go - Gbtc
//
//  Copyright © 2026, The go2cs Authors. All Rights Reserved.

package main

import (
	"os"
	"path/filepath"
	"strings"
	"testing"
)

// GitHub Pages renders every docs page through Liquid before Markdown, so a test name holding `{{` or `{%`
// is published as empty text or breaks the site build (src/go2cs/internal/repoguard/docsJekyllLiquid_test.go).
// go.yaml.in/yaml/v3 v3.0.5's proof page was the first generated page to quote one: two subtest names carry a
// template action. A generated page that holds either delimiter carries the raw guard, its opener right after
// the H1 (jekyll-titles-from-headings reads the title from the first line) and its endraw as the last line.

const liquidTestName = `TestDecoder/test_42:_"{{.}}"`

func liquidGuardPage() (string, testComparison) {
	comparison := testComparison{Package: "example.test/m/p", Status: "matched", Matched: true,
		Go:       map[string]string{"TestA": "pass", liquidTestName: "pass"},
		CSharp:   map[string]string{"TestA": "pass", liquidTestName: "pass"},
		Excluded: []string{"Example_{%template%} (example): example execution is deferred to Phase 4D"}}

	page := renderValidationProofPage(proofPageProvenance{importPath: "example.test/m/p", goVersion: "1.26.11", platform: "linux/amd64",
		date: "2026-11-22", convertedPath: "src/example.test/m/p"}, comparison, nil, nil)

	return page, comparison
}

func assertLiquidGuarded(t *testing.T, what, page string) {
	t.Helper()

	lines := strings.Split(strings.TrimRight(strings.ReplaceAll(page, "\r", ""), "\n"), "\n")

	if len(lines) < 3 {
		t.Fatalf("%s: too short to carry a guard:\n%s", what, page)
	}

	if !strings.HasPrefix(lines[0], "# `") {
		t.Fatalf("%s: the H1 is not the first line: %q", what, lines[0])
	}

	if !strings.HasPrefix(lines[1], "<!-- {% raw %}") || !strings.HasSuffix(lines[1], "-->") {
		t.Fatalf("%s: the line after the H1 is not the raw guard's opener: %q", what, lines[1])
	}

	if last := lines[len(lines)-1]; last != "<!-- {% endraw %} -->" {
		t.Fatalf("%s: the last line is not the guard's endraw: %q", what, last)
	}

	if n := strings.Count(page, "{% raw %}"); n != 1 {
		t.Fatalf("%s: want exactly one raw opener, got %d", what, n)
	}
}

// A proof page quoting a delimiter, and the MODULE.md summed from it, carry the guard.
func TestProofPageGuardsALiquidDelimiterAfterTheH1(t *testing.T) {
	page, _ := liquidGuardPage()

	if !strings.Contains(page, liquidTestName) {
		t.Fatalf("the page does not quote the test name it was built to quote:\n%s", page)
	}

	assertLiquidGuarded(t, "proof page", page)

	moduleDir := filepath.Join(t.TempDir(), "example.test", "m")
	writeModuleFile(t, filepath.Join(moduleDir, "p", "p.md"), page)

	if err := writeThirdPartyModuleSummary(moduleDir, "example.test/m"); err != nil {
		t.Fatal(err)
	}

	summary := readGenerated(t, filepath.Join(moduleDir, "MODULE.md"))

	if !strings.Contains(summary, "{%template%}") {
		t.Fatalf("MODULE.md does not carry the excluded name it was built to carry:\n%s", summary)
	}

	assertLiquidGuarded(t, "MODULE.md", summary)
}

// A page without either delimiter is unchanged: no guard, so no committed page moves.
func TestProofPageWithoutALiquidDelimiterCarriesNoGuard(t *testing.T) {
	comparison := testComparison{Package: "example.test/m/p", Status: "matched", Matched: true,
		Go: map[string]string{"TestA": "pass"}, CSharp: map[string]string{"TestA": "pass"}}

	page := renderValidationProofPage(proofPageProvenance{importPath: "example.test/m/p", goVersion: "1.26.11", platform: "linux/amd64",
		date: "2026-11-22", convertedPath: "src/example.test/m/p"}, comparison, nil, nil)

	if strings.Contains(page, "raw %}") {
		t.Fatalf("a page with no Liquid delimiter carries a guard:\n%s", page)
	}
}

// Every committed generated page is unchanged by the guard, so regenerating the tree moves no page that holds
// neither delimiter; a page that already carries the guard is unchanged too.
func TestLiquidGuardLeavesEveryCommittedProofPageUnchanged(t *testing.T) {
	root, err := filepath.Abs(filepath.Join("..", "..", "docs", "validation"))

	if err != nil {
		t.Fatal(err)
	}

	pages := 0

	for _, dir := range []string{"current", "modules"} {
		err := filepath.WalkDir(filepath.Join(root, dir), func(path string, entry os.DirEntry, err error) error {
			if err != nil || entry.IsDir() || filepath.Ext(path) != ".md" {
				return err
			}

			data, err := os.ReadFile(path)

			if err != nil {
				return err
			}

			page := strings.ReplaceAll(string(data), "\r\n", "\n")
			pages++

			if got := liquidGuardedPage(page); got != page {
				rel, _ := filepath.Rel(root, path)
				t.Errorf("%s: the guard changes a committed page", rel)
			}

			return nil
		})

		if err != nil {
			t.Fatal(err)
		}
	}

	if pages == 0 {
		t.Fatalf("no committed page found under %s: the measurement is vacuous", root)
	}

	t.Logf("%d committed pages read, all unchanged", pages)
}
