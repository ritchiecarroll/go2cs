// docsJekyllLiquid_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package repoguard

import (
	"bytes"
	"fmt"
	"os"
	"path/filepath"
	"regexp"
	"strings"
	"testing"
)

// GitHub Pages publishes docs\ as the project site, built by Jekyll 3.10 (the github-pages gem).
// jekyll-optional-front-matter makes EVERY Markdown file a page, and Jekyll renders a page through
// Liquid BEFORE Markdown -- the whole file, fenced code included. Go and C# text is full of Liquid's
// delimiters: a composite literal `[]T{{...}}`, a `go list -f '{{.GoFiles}}'` template.
//
// Two failure shapes, both measured against the real build:
//
//   - A '{{' whose token does not end in '}}' is a Liquid SyntaxError, and the WHOLE site build
//     fails: nothing deploys until it is fixed. pages-build-deployment at 1dae85e093 (2026-09-27)
//     died on ConversionStrategies.md line 8760, `{{Dr. Michał 18}`, from the new summary.
//   - A well-formed '{{ x }}' renders SILENTLY as empty text. The build before it passed while
//     logging 17 Liquid warnings in 7 files; the published runbook showed `go list -f ''` where the
//     source reads `go list -f '{{.GoFiles}}'`.
//
// The remedy is the raw guard, hidden from GitHub's renderer inside HTML comments:
//
//	# Title
//	<!-- {% raw %} — Jekyll/Liquid guard: ... -->
//	...
//	<!-- {% endraw %} -->
//
// The opener goes AFTER the H1. jekyll-titles-from-headings reads the page title only from a heading
// at the very start of the file, so a guard on line 1 titles the published page with the site name
// ("go2cs | Golang to C# Converter"). Nineteen guarded pages were published that way until
// 2026-09-27.
//
// POSITIVE CONTROL: delete the guard lines from docs/ConversionStrategies.md and
// TestDocsSurviveJekyllLiquid goes RED naming ConversionStrategies.md:8760 as a build break;
// move a guard back to line 1 of docs/Glossary.md and it goes RED naming the title. Both were
// verified when this guard landed. TestLiquidTokenizerMatchesJekyll pins the tokenizer to outcomes
// read from the real build logs.

// liquidToken is Liquid 4.0.4's TemplateParser: a complete tag, a variable ending at the FIRST '}'
// (optionally '}}'), or a bare opener. Go's regexp is leftmost-first like Ruby's, so alternation
// order carries over.
var liquidToken = regexp.MustCompile(`(?s)\{%.*?%\}|\{\{.*?\}\}?|\{%|\{\{`)

// liquidVariable is Liquid's ContentOfVariable: a '{{' token is well formed only when it ends '}}'.
var liquidVariable = regexp.MustCompile(`(?s)\A\{\{-?.*?-?\}\}\z`)

// liquidTag is Liquid's FullToken for a '{%' token.
var liquidTag = regexp.MustCompile(`(?s)\A\{%-?\s*(\w+)\s*.*?-?%\}\z`)

// liquidRawEnd is Liquid's FullTokenPossiblyInvalid, which the raw tag applies to every token in its
// body: the LAST '{%' in the token names the tag, so an endraw ends the span even when the tokenizer
// glued it to earlier body text.
var liquidRawEnd = regexp.MustCompile(`(?s)\A.*\{%-?\s*(\w+)\s*.*?-?%\}\z`)

// frozenSnapshot matches the write-once release snapshots under docs\validation\<version>\. They
// are never rewritten, so the title rule does not apply to them; the Liquid rule still does.
var frozenSnapshot = regexp.MustCompile(`^docs/validation/\d+(\.\d+)+/`)

type liquidFinding struct {
	line  int
	class string // "build-break" or "silent-loss"
	token string
}

// liquidOutsideRaw returns every Liquid token outside a raw span, and whether a raw span is left
// open at the end of the text (Liquid raises "raw tag was never closed").
func liquidOutsideRaw(text string) (findings []liquidFinding, rawOpen bool) {
	for _, loc := range liquidToken.FindAllStringIndex(text, -1) {
		token := text[loc[0]:loc[1]]

		if rawOpen {
			if m := liquidRawEnd.FindStringSubmatch(token); m != nil && m[1] == "endraw" {
				rawOpen = false
			}

			continue
		}

		if m := liquidTag.FindStringSubmatch(token); m != nil && m[1] == "raw" {
			rawOpen = true
			continue
		}

		class := "build-break"

		if strings.HasPrefix(token, "{{") && liquidVariable.MatchString(token) {
			class = "silent-loss"
		}

		shown := token

		if len(shown) > 60 {
			shown = shown[:60] + "..."
		}

		findings = append(findings, liquidFinding{
			line:  strings.Count(text[:loc[0]], "\n") + 1,
			class: class,
			token: strings.ReplaceAll(shown, "\n", `\n`),
		})
	}

	return findings, rawOpen
}

// jekyllRendersThroughLiquid reports whether Jekyll renders a tracked docs\ file as a page: a
// Markdown extension (jekyll-optional-front-matter), or YAML front matter on the first line. A path
// component starting with '.', '_' or '#', or ending with '~', is excluded by Jekyll itself (its
// layouts and includes are templates, and Liquid there is intended).
func jekyllRendersThroughLiquid(rel string, head []byte) bool {
	for _, part := range strings.Split(rel, "/") {
		if part == "" || strings.ContainsRune("._#", rune(part[0])) || strings.HasSuffix(part, "~") {
			return false
		}
	}

	switch strings.ToLower(filepath.Ext(rel)) {
	case ".md", ".markdown", ".mkdown", ".mkdn", ".mkd":
		return true
	}

	head = bytes.TrimPrefix(head, []byte("\xef\xbb\xbf"))

	return regexp.MustCompile(`\A---[ \t]*\r?\n`).Match(head)
}

// TestLiquidTokenizerMatchesJekyll pins the emulation to what the real build did. Every "want" was
// read from a pages-build-deployment log: the fatal error of 2026-09-27 and the warnings of the build
// before it.
func TestLiquidTokenizerMatchesJekyll(t *testing.T) {
	cases := []struct {
		name string
		text string
		want []string // class per finding, in order
	}{
		{"unterminated variable breaks the build", "fmt.Println(record) // {{Dr. Michał 18} {software engineer}}\n", []string{"build-break"}},
		{"a closed pair renders empty", "x := []Person{{Dr. Michał 18}}\n", []string{"silent-loss"}},
		{"go list template renders empty", "go list -f '{{.GoFiles}}'\n", []string{"silent-loss"}},
		{"two templates on one line", "'{{.GoFiles}} {{.CgoFiles}}'", []string{"silent-loss", "silent-loss"}},
		{"empty braces render empty", "var zero = T{{}}", []string{"silent-loss"}},
		{"a bare tag opener breaks the build", "fmt.Sprintf(\"{%d\", n)", []string{"build-break"}},
		{"raw guard covers everything", "# T\n<!-- {% raw %} -->\n[]T{{1, 2}}\n{{.Dir}}\n<!-- {% endraw %} -->\n", nil},
		{"text after the endraw is live again", "<!-- {% raw %} -->\n{{a}}\n<!-- {% endraw %} -->\n{{b}}", []string{"silent-loss"}},
		{"plain braces are not Liquid", "func f() { if x { return } }", nil},
	}

	for _, c := range cases {
		findings, rawOpen := liquidOutsideRaw(c.text)

		var got []string

		for _, f := range findings {
			got = append(got, f.class)
		}

		if rawOpen || fmt.Sprint(got) != fmt.Sprint(c.want) {
			t.Errorf("%s: got %v (raw left open: %v), want %v", c.name, got, rawOpen, c.want)
		}
	}

	if _, rawOpen := liquidOutsideRaw("<!-- {% raw %} -->\n{{x}}\n"); !rawOpen {
		t.Errorf("an unclosed raw span must be reported: Liquid refuses the page")
	}
}

// TestDocsSurviveJekyllLiquid is the guard over the committed tree.
func TestDocsSurviveJekyllLiquid(t *testing.T) {
	root := repoRootFromPackageDir(t)
	tracked := gitTrackedFiles(t, root)

	if len(tracked) < 1000 {
		t.Fatalf("git ls-files returned %d paths, too few to be this repository; a guard that scans nothing passes everything", len(tracked))
	}

	var problems []string
	pages := 0

	for _, rel := range tracked {
		if !strings.HasPrefix(rel, "docs/") {
			continue
		}

		data, err := os.ReadFile(filepath.Join(root, filepath.FromSlash(rel)))

		if err != nil {
			t.Fatalf("cannot read %s: %v", rel, err)
		}

		site := strings.TrimPrefix(rel, "docs/")

		if !jekyllRendersThroughLiquid(site, data) {
			continue
		}

		pages++
		text := strings.TrimPrefix(string(data), "\xef\xbb\xbf")
		findings, rawOpen := liquidOutsideRaw(text)

		for _, f := range findings {
			effect := "fails the whole site build"

			if f.class == "silent-loss" {
				effect = "is published as EMPTY text"
			}

			problems = append(problems, fmt.Sprintf("%s:%d: Liquid token %q %s -- add the raw guard after the H1 (see this file's header)", rel, f.line, f.token, effect))
		}

		if rawOpen {
			problems = append(problems, fmt.Sprintf("%s: a {%% raw %%} guard is never closed -- Liquid refuses the page", rel))
		}

		if !strings.Contains(text, "{% raw %}") {
			continue
		}

		lines := strings.Split(strings.ReplaceAll(text, "\r\n", "\n"), "\n")

		if strings.Contains(lines[0], "{% raw %}") && !frozenSnapshot.MatchString(rel) {
			problems = append(problems, fmt.Sprintf("%s:1: the raw guard precedes the H1, so the published page is titled with the site name -- move it below the H1", rel))
		}

		last := ""

		for i := len(lines) - 1; i >= 0; i-- {
			if strings.TrimSpace(lines[i]) != "" {
				last = lines[i]
				break
			}
		}

		if !strings.Contains(last, "{% endraw %}") {
			problems = append(problems, fmt.Sprintf("%s: the {%% endraw %%} guard is not the last line, so text after it is live Liquid again", rel))
		}
	}

	if pages < 100 {
		t.Fatalf("VACUOUS: only %d docs pages matched Jekyll's rendering predicate; the guard cannot have scanned the site", pages)
	}

	if len(problems) > 0 {
		t.Fatalf("%d docs page problem(s) under GitHub Pages' Jekyll/Liquid build:\n  %s", len(problems), strings.Join(problems, "\n  "))
	}
}
