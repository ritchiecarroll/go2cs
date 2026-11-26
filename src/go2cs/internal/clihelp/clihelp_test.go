// clihelp_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package clihelp

import (
	"bytes"
	"flag"
	"os"
	"path/filepath"
	"strings"
	"testing"
)

var update = flag.Bool("update", false, "rewrite the help goldens in testdata from the table")

// pagePath is docs/CommandLine.md from this package's directory.
var pagePath = filepath.Join("..", "..", "..", "..", "docs", "CommandLine.md")

// TestCommandLinePageInSync is the drift guard for docs/CommandLine.md's generated regions, as
// TestStdLibMetadataInSync is for stdlib-metadata.txt: it renders each region from the table and fails, naming the
// region and its first differing line, when the committed page differs. A hand edit inside a region and a table
// change without `go generate` both fail here. Regenerate with `go generate .` from src/go2cs; never hand-edit a
// region.
func TestCommandLinePageInSync(t *testing.T) {
	data, err := os.ReadFile(pagePath)

	if err != nil {
		t.Fatal(err)
	}

	page := string(data)
	want, err := Page(page)

	if err != nil {
		t.Fatal(err)
	}

	for _, region := range Regions {
		committed, ok := RegionText(page, region.Name)

		if !ok {
			t.Fatalf("docs/CommandLine.md has no %s region markers", region.Name)
		}

		generated, _ := RegionText(want, region.Name)

		if committed == generated {
			continue
		}

		committedLines, generatedLines := strings.Split(committed, "\n"), strings.Split(generated, "\n")

		for i := 0; i < max(len(committedLines), len(generatedLines)); i++ {
			var c, g string

			if i < len(committedLines) {
				c = committedLines[i]
			}

			if i < len(generatedLines) {
				g = generatedLines[i]
			}

			if c != g {
				t.Errorf("docs/CommandLine.md's generated %s region is not what internal/clihelp/options.go writes, "+
					"from its line %d:\n  page:  %q\n  table: %q\nEdit options.go, not the page, then run `go generate .` "+
					"in src/go2cs.", region.Name, i+1, c, g)
				break
			}
		}
	}

	if !t.Failed() && strings.ReplaceAll(want, "\r\n", "\n") != strings.ReplaceAll(page, "\r\n", "\n") {
		t.Error("docs/CommandLine.md differs from what `go generate` writes outside the compared regions")
	}
}

// A hand edit inside a region fails the sync test's comparison; the markers bound what is compared.
func TestHandEditInsideARegionIsCaught(t *testing.T) {
	data, err := os.ReadFile(pagePath)

	if err != nil {
		t.Fatal(err)
	}

	page := strings.ReplaceAll(string(data), "\r\n", "\n")
	edited := strings.Replace(page, "| [`-tests`](#option-tests) | ", "| [`-tests`](#option-tests) | Hand edit. ", 1)

	if edited == page {
		t.Fatal("the common-options region has no -tests row to edit")
	}

	committed, _ := RegionText(edited, "common-options")
	want, _ := Page(edited)
	generated, _ := RegionText(want, "common-options")

	if committed == generated {
		t.Fatal("a hand edit inside the common-options region is not caught")
	}
}

func TestPageNeedsTheMarkers(t *testing.T) {
	if _, err := Page("# a page with no markers\n"); err == nil {
		t.Fatal("Page accepted a page with no region markers")
	}
}

// The table's own rules: names unique, every group known, entries in class then group order, a summary that is one
// line of plain text with no final period, and details for every option.
func TestOptionTable(t *testing.T) {
	seen := map[string]bool{}
	previous := Option{}

	for i, option := range options {
		if option.Name == "" || seen[option.Name] {
			t.Errorf("entry %d: name %q is empty or repeated", i, option.Name)
		}

		seen[option.Name] = true
		group := groupIndex(option.Group)

		if group < 0 {
			t.Errorf("-%s: group %q is not one of Groups", option.Name, option.Group)
		}

		if i > 0 && (option.Class < previous.Class || option.Class == previous.Class && group < groupIndex(previous.Group)) {
			t.Errorf("-%s is out of order: the table is sorted by class, then by group", option.Name)
		}

		if option.Summary == "" || strings.HasSuffix(option.Summary, ".") || strings.ContainsAny(option.Summary, "`*[\n") {
			t.Errorf("-%s: the summary must be one line of plain text with no final period: %q", option.Name,
				option.Summary)
		}

		if strings.TrimSpace(option.Details) == "" {
			t.Errorf("-%s has no details", option.Name)
		}

		previous = option
	}
}

func groupIndex(group string) int {
	for i, g := range Groups {
		if g == group {
			return i
		}
	}

	return -1
}

func TestSuggest(t *testing.T) {
	cases := map[string]string{
		"test-actoin": "-test-action",
		"-recures":    "-recurse",
		"nuget":       "-nuget-map",
		"zzzzzzzz":    "",
		"v":           "",
		"tset":        "",
	}

	for name, want := range cases {
		got := Suggest(name)

		if want == "" && len(got) != 0 || want != "" && (len(got) == 0 || got[0] != want) {
			t.Errorf("Suggest(%q) = %q, want %q first", name, got, want)
		}
	}
}

func TestPlainText(t *testing.T) {
	cases := map[string]string{
		"See **[Build tags](X.md#a)**, `-tags`<!-- note -->  and more.": "See Build tags (" + SiteURL + "X.html#a), -tags and more.",
		"Ctrl-C or Ctrl-\\ stops the run":                               "Ctrl-C or Ctrl-\\ stops the run",
		"writes `<dir>\\platform-manifest.json` and `src\\`":            "writes <dir>\\platform-manifest.json and src\\",
		"an escaped \\* star and \\| pipe":                              "an escaped * star and | pipe",
		"[nugetgo.net](https://nugetgo.net) and [x](../LICENSING.md)":   "https://nugetgo.net and x (" + repositoryURL + "LICENSING.md)",
		"[the reference](#option-tests)":                                "the reference",
	}

	for markdown, want := range cases {
		if got := PlainText(markdown); got != want {
			t.Errorf("PlainText(%q) = %q, want %q", markdown, got, want)
		}
	}
}

// The help forms are held to goldens in testdata. Run `go test ./internal/clihelp -update` after a table change.
func TestHelpGoldens(t *testing.T) {
	tests, _ := Lookup("tests")

	for name, write := range map[string]func(*bytes.Buffer){
		"help-common.txt": func(b *bytes.Buffer) { WriteCommon(b) },
		"help-all.txt":    func(b *bytes.Buffer) { WriteAll(b) },
		"help-tests.txt":  func(b *bytes.Buffer) { WriteOption(b, tests, "false") },
	} {
		var b bytes.Buffer
		write(&b)
		path := filepath.Join("testdata", name)

		if *update {
			if err := os.WriteFile(path, b.Bytes(), 0o644); err != nil {
				t.Fatal(err)
			}

			continue
		}

		want, err := os.ReadFile(path)

		if err != nil {
			t.Fatal(err)
		}

		if strings.ReplaceAll(string(want), "\r\n", "\n") != b.String() {
			t.Errorf("%s is not what the table writes; run `go test ./internal/clihelp -update` after a table change "+
				"and review the diff", path)
		}
	}
}

// No help line is wider than the help's width, except a syntax too long to share its line.
func TestHelpWidth(t *testing.T) {
	var b bytes.Buffer
	WriteAll(&b)

	for _, line := range strings.Split(b.String(), "\n") {
		if len(line) > width {
			t.Errorf("help line is %d columns, over %d: %q", len(line), width, line)
		}
	}
}

// Each option table's header is padded to its longest unbreakable token: two no-break spaces per character, less
// six. -recurse[=module,nuget] (23 characters) sets the common table's header; a space in a syntax is a break point
// the table may use, so it does not count.
func TestOptionHeaderPadsToTheLongestToken(t *testing.T) {
	common := filter(func(o Option) bool { return o.Class == Common })

	if got, want := optionHeader(common), "Option"+strings.Repeat("&nbsp;", 2*23-6); got != want {
		t.Errorf("the common table's header is %q, want %d no-break spaces", got, 2*23-6)
	}

	if got := optionHeader([]Option{{Name: "inline-arrays", Value: "<pattern>[,<pattern>...]"}}); got !=
		"Option"+strings.Repeat("&nbsp;", 2*24-6) {
		t.Errorf("a value token longer than the name sets the padding: %q", got)
	}

	if got := optionHeader([]Option{{Name: "v"}}); got != "Option" {
		t.Errorf("a token shorter than the header word pads nothing: %q", got)
	}
}
