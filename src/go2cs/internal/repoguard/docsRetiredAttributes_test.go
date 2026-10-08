// docsRetiredAttributes_test.go - Gbtc
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

// A page must not show an attribute the converter no longer writes. The face lift
// (docs/PLAN-marker-comment-parity.md) moves a set of attributes out of converted code, into generated
// code, a marker comment or nothing. A sample or a sentence that still shows one of them describes
// output the converter does not produce, and nothing else fails when that happens: the page builds,
// the links resolve, and the reader is told something false.
//
// THE LIST IS docsRetiredAttributes, AND IT GROWS. A seat that stops the converter writing an attribute
// adds its row in the same change. The list is EMPTY until the first face-lift seat lands, so
// TestDocsShowNoRetiredAttribute cannot go red on today's tree; TestRetiredAttributeGuardFiresOnAPlantedPage
// is its control, run against docsFaceLiftAttributes, the set the plan rules out of converted code.
//
// WHAT COUNTS AS SHOWING ONE: the attribute written as an attribute, "[Name]", "[Name(" or "[Name,",
// in a fenced sample, a code span or plain prose. A row may name one form: plain "[GoType]" and
// "[GoType("num:int64")]" leave in different seats. The class name without its bracket (a generator's
// name, "GoRecvAttribute") is not a showing.
//
// WHAT IS NOT READ:
//   - an HTML comment, which a visitor never sees and which holds the dated derivation of a rule;
//   - the records: docs/phase4/ and docs/doctrine/ (blockRecordDirs), the release snapshots
//     (frozenSnapshot), and the pages retiredAttributeHistory names, which describe an earlier state
//     and say so.
//
// THE DECLARED EXCEPTION. Hand-written files keep these attributes for good, and a generated part
// carries some of them, so a page may still have to show one. It says so with a marker comment on the
// line before the block:
//
//	<!-- attribute-shown: hand-written code keeps it -->
//
// The marker covers the block that follows it: the fenced sample if one opens next, otherwise the
// lines up to the next blank line. It needs a reason, and a marker that covers no showing is itself a
// finding, so the exceptions cannot outlive what they excuse.
//
// SCOPE: the living pages under docs/ (livingMarkdownPages, less retiredAttributeHistory), and every
// tracked Markdown file under .claude/rules/ and .claude/skills/.

// retiredAttribute is one attribute the converter no longer writes into converted code.
type retiredAttribute struct {
	name string // the attribute's name as converted code spelled it, without brackets
	form string // "" for every form, "plain" for [Name] alone, "arg" for [Name(...)]
	seat string // the seat that stopped the converter writing it
}

func (a retiredAttribute) String() string {
	switch a.form {
	case "plain":
		return "[" + a.name + "]"
	case "arg":
		return "[" + a.name + "(...)]"
	}

	return "[" + a.name + "] in any form"
}

// docsRetiredAttributes is the guard's list. Add a row in the change that stops the converter writing
// the attribute; never remove one.
var docsRetiredAttributes = []retiredAttribute{}

// docsFaceLiftAttributes is the set docs/PLAN-marker-comment-parity.md sections 5.1 to 5.7 rule out of
// converted code. The planted control runs against it, and the guard reads it over the real pages to
// show its scanner reaches them.
var docsFaceLiftAttributes = []retiredAttribute{
	{"GoRecv", "", "face lift A"},
	{"GoType", "plain", "face lift B"},
	{"GoType", "arg", "face lift C"},
	{"GoEmbedded", "", "face lift F"},
	{"GoTag", "", "face lift E"},
	{"GoArrayDims", "", "face lift D"},
	{"GoParamDims", "", "face lift D"},
	{"GoStr", "", "face lift S"},
}

// retiredAttributeHistory are the living-tree pages that describe an earlier state of the converter.
var retiredAttributeHistory = map[string]string{
	"docs/phase3/":                       "Phase 3's closed records",
	"docs/RoadmapHistory.md":             "the dated history of the roadmap",
	"docs/CleanupBacklog.md":             "a dated backlog of findings",
	"docs/PLAN-marker-comment-parity.md": "the face lift's own plan, which shows each attribute before and after",
}

var retiredAttributeMarker = regexp.MustCompile(`<!--\s*attribute-shown:\s*(.*?)\s*-->`)

type retiredFinding struct {
	line int
	what string
}

// retiredAttributePattern matches the attribute written as an attribute. Its first group is the
// character after the name: "(" opens an argument list, "]" or "," ends a plain use.
func retiredAttributePattern(name string) *regexp.Regexp {
	return regexp.MustCompile(`\[\s*(?:global::)?(?:go\.)?` + regexp.QuoteMeta(name) + `\s*([\](,])`)
}

// retiredAttributesShown returns, by 1-based line, each showing of a listed attribute in text, and a
// finding for each marker that has no reason or covers no showing. exceptions counts the markers used.
func retiredAttributesShown(text string, retired []retiredAttribute) (findings []retiredFinding, exceptions int) {
	type compiled struct {
		attr retiredAttribute
		re   *regexp.Regexp
	}

	var patterns []compiled

	for _, a := range retired {
		patterns = append(patterns, compiled{a, retiredAttributePattern(a.name)})
	}

	lines := strings.Split(strings.ReplaceAll(text, "\r\n", "\n"), "\n")
	fence, comment := "", false

	// A marker waits for its block, then covers it. markerLine is the marker's own line while it is
	// pending or open; covered counts the showings it excused.
	pending, open, openFence := false, false, false
	markerLine, covered := 0, 0

	closeMarker := func() {
		if open && covered == 0 {
			findings = append(findings, retiredFinding{markerLine, "an attribute-shown marker covers no listed attribute -- remove the marker"})
		}

		if open {
			exceptions++
		}

		pending, open, openFence, covered = false, false, false, 0
	}

	for i, raw := range lines {
		line := raw[len(blockQuotePrefix.FindString(raw)):]
		visible := line
		isFenceLine := false

		if !comment {
			if m := blockFence.FindStringSubmatch(line); m != nil {
				isFenceLine = true

				switch {
				case fence == "":
					fence = m[1]

					if pending {
						pending, open, openFence = false, true, true
					}
				case m[1][0] == fence[0] && len(m[1]) >= len(fence):
					fence = ""

					if open && openFence {
						closeMarker()
					}
				}
			}
		}

		if isFenceLine {
			continue
		}

		if fence == "" {
			// Outside a sample, drop what sits inside an HTML comment; the comment may span lines.
			var b strings.Builder
			rest := line

			for rest != "" {
				if comment {
					end := strings.Index(rest, "-->")

					if end < 0 {
						rest = ""
						break
					}

					comment = false
					rest = rest[end+3:]
					continue
				}

				start := strings.Index(rest, "<!--")

				if start < 0 {
					b.WriteString(rest)
					break
				}

				b.WriteString(rest[:start])
				comment = true
				rest = rest[start+4:]
			}

			visible = b.String()

			if m := retiredAttributeMarker.FindStringSubmatch(line); m != nil {
				closeMarker()

				if m[1] == "" {
					findings = append(findings, retiredFinding{i + 1, "an attribute-shown marker gives no reason"})
				}

				pending, markerLine = true, i+1
				continue
			}

			blank := strings.TrimSpace(line) == ""

			switch {
			case pending && !blank:
				pending, open = false, true
			case open && !openFence && blank:
				closeMarker()
			}
		}

		for _, p := range patterns {
			for _, m := range p.re.FindAllStringSubmatch(visible, -1) {
				hasArg := m[1] == "("

				if (p.attr.form == "plain" && hasArg) || (p.attr.form == "arg" && !hasArg) {
					continue
				}

				if open {
					covered++
					continue
				}

				findings = append(findings, retiredFinding{i + 1, fmt.Sprintf("shows %s, which the converter no longer writes (%s)", p.attr, p.attr.seat)})
			}
		}
	}

	if pending {
		findings = append(findings, retiredFinding{markerLine, "an attribute-shown marker covers no listed attribute -- remove the marker"})
	}

	closeMarker()

	return findings, exceptions
}

// TestRetiredAttributeRuleReadsEachShape pins the rule, one shape per case.
func TestRetiredAttributeRuleReadsEachShape(t *testing.T) {
	all := docsFaceLiftAttributes
	plainOnly := []retiredAttribute{{"GoType", "plain", "test"}}
	argOnly := []retiredAttribute{{"GoType", "arg", "test"}}

	cases := []struct {
		name    string
		retired []retiredAttribute
		text    string
		want    []int
		except  int
	}{
		{"a fenced sample", all, "```csharp\n[GoRecv] public static void Read(this ref Reader b) {\n```\n", []int{2}, 0},
		{"a code span in prose", all, "The method is marked `[GoRecv]`.\n", []int{1}, 0},
		{"plain prose", all, "The [GoEmbedded] field is promoted.\n", []int{1}, 0},
		{"a table cell", all, "| a | `[GoTag(\"x\")]` |\n", []int{1}, 0},
		{"inside a blockquote", all, "> `[GoStr] public static`\n", []int{1}, 0},
		{"on a parameter", all, "```csharp\nnint format(this Duration d, [GoArrayDims(32)] ж<array<byte>> Ꮡbuf) {\n```\n", []int{2}, 0},
		{"first in an attribute list", all, "```csharp\n[GoType, GoLocalName(\"x\")] partial struct T;\n```\n", []int{2}, 0},
		{"two on one line are two findings", all, "`[GoRecv]` and `[GoStr]`\n", []int{1, 1}, 0},
		{"the plain row leaves the argument form", plainOnly, "`[GoType(\"num:int64\")] partial struct Duration;`\n", nil, 0},
		{"the plain row reads the plain form", plainOnly, "`[GoType] partial struct Scanner {`\n", []int{1}, 0},
		{"the argument row leaves the plain form", argOnly, "`[GoType] partial struct Scanner {`\n", nil, 0},
		{"the argument row reads the argument form", argOnly, "`[GoType(\"num:int64\")] partial struct Duration;`\n", []int{1}, 0},
		{"the class name alone is not a showing", all, "RecvGenerator read GoRecvAttribute; the GoType test is in Common.cs.\n", nil, 0},
		{"a longer name is another attribute", all, "`[GoTypeAlias(\"x\")]` and `[GoRecvFuture]`\n", nil, 0},
		{"the marker comments that replace them", all, "```csharp\npartial struct Duration /*num:int64*/;\n/*embed*/ public Reader Reader;\npublic @string Method; /*`json:\"method\"`*/\n```\n", nil, 0},
		{"an HTML comment is not read", all, "<!-- was `[GoRecv]` before the face lift -->\n", nil, 0},
		{"a comment that spans lines", all, "text <!-- [GoRecv]\n[GoType] -->\n", nil, 0},
		{"text after a comment on its line is read", all, "<!-- note --> `[GoRecv]`\n", []int{1}, 0},
		{"a comment opener inside a sample is code", all, "```csharp\n// <!--\n[GoRecv] void M() {\n```\n", []int{3}, 0},
		{"an empty list reads nothing", nil, "`[GoRecv]`\n", nil, 0},
		{"a marker covers the sample after it", all, "<!-- attribute-shown: hand-written code keeps it -->\n```csharp\n[GoRecv] void M(this ref T t) {\n```\n", nil, 1},
		{"a marker covers a paragraph to its blank line", all, "<!-- attribute-shown: hand-written -->\nA hand-written method keeps `[GoRecv]`,\nand `[GoType]` too.\n\nA converted one shows `[GoRecv]`.\n", []int{5}, 1},
		{"a marker does not reach the second sample", all, "<!-- attribute-shown: generated part -->\n```csharp\n[GoType] partial struct T;\n```\n\n```csharp\n[GoType] partial struct U;\n```\n", []int{7}, 1},
		{"a blank line may stand before the covered block", all, "<!-- attribute-shown: generated part -->\n\n```csharp\n[GoType] partial struct T;\n```\n", nil, 1},
		{"a marker with no reason", all, "<!-- attribute-shown: -->\n`[GoRecv]`\n", []int{1}, 1},
		{"a marker that covers nothing", all, "<!-- attribute-shown: hand-written -->\nNo attribute here.\n", []int{1}, 1},
		{"a marker at the end of the page", all, "text\n\n<!-- attribute-shown: hand-written -->\n", []int{3}, 0},
	}

	for _, c := range cases {
		findings, exceptions := retiredAttributesShown(c.text, c.retired)
		var got []int

		for _, f := range findings {
			got = append(got, f.line)
		}

		sort.Ints(got)

		if fmt.Sprint(got) != fmt.Sprint(c.want) || exceptions != c.except {
			t.Errorf("%s: got lines %v and %d exception(s), want %v and %d", c.name, got, exceptions, c.want, c.except)
		}
	}
}

// retiredAttributePages returns the pages the guard reads: the living docs pages less the history
// pages, and the tracked rules and skills files.
func retiredAttributePages(t *testing.T) (pages map[string]string, agentFiles int) {
	t.Helper()

	pages = map[string]string{}

	for rel, text := range livingMarkdownPages(t) {
		history := false

		for prefix := range retiredAttributeHistory {
			history = history || strings.HasPrefix(rel, prefix)
		}

		if !history {
			pages[rel] = text
		}
	}

	root := repoRootFromPackageDir(t)

	for _, rel := range gitTrackedFiles(t, root) {
		if !strings.HasSuffix(rel, ".md") || !(strings.HasPrefix(rel, ".claude/rules/") || strings.HasPrefix(rel, ".claude/skills/")) {
			continue
		}

		data, err := os.ReadFile(filepath.Join(root, filepath.FromSlash(rel)))

		if err != nil {
			t.Fatalf("cannot read %s: %v", rel, err)
		}

		pages[rel] = string(data)
		agentFiles++
	}

	if agentFiles < 5 {
		t.Fatalf("VACUOUS: only %d rules and skills files matched; the guard cannot have read them", agentFiles)
	}

	return pages, agentFiles
}

// TestRetiredAttributeListIsWellFormed refuses a row the scanner would read as something else.
func TestRetiredAttributeListIsWellFormed(t *testing.T) {
	for _, list := range [][]retiredAttribute{docsRetiredAttributes, docsFaceLiftAttributes} {
		forms := map[string]map[string]bool{}

		for _, a := range list {
			if a.seat == "" || !regexp.MustCompile(`^[A-Za-z][A-Za-z0-9]*$`).MatchString(a.name) {
				t.Errorf("row %+v: a row needs a bare attribute name and the seat that retired it", a)
			}

			if a.form != "" && a.form != "plain" && a.form != "arg" {
				t.Errorf("row %+v: form is \"\", \"plain\" or \"arg\"", a)
			}

			if forms[a.name] == nil {
				forms[a.name] = map[string]bool{}
			}

			// A second row for one name is right only when the two name different single forms.
			if forms[a.name][a.form] || forms[a.name][""] || (a.form == "" && len(forms[a.name]) > 0) {
				t.Errorf("row %+v: %s is listed twice, or in every form and again in one", a, a.name)
			}

			forms[a.name][a.form] = true
		}
	}
}

// TestRetiredAttributeGuardFiresOnAPlantedPage is the guard's control: one planted page that shows
// every face-lift attribute in every position the pages use, and one written the way the face lift
// leaves the code, which must read clean. It does not depend on docsRetiredAttributes.
func TestRetiredAttributeGuardFiresOnAPlantedPage(t *testing.T) {
	planted := strings.Join([]string{
		"# A planted page", // 1
		"",                 // 2
		"A pointer-receiver method is marked `[GoRecv]`.", // 3  prose, GoRecv
		"",                                       // 4
		"```csharp",                              // 5
		"[GoType] partial struct Scanner {",      // 6  sample, plain GoType
		"    [GoEmbedded] public Reader Reader;", // 7  sample, GoEmbedded
		"    [GoTag(@\"json:\"\"method\"\"\")]",  // 8  sample, GoTag
		"    public @string Method;",             // 9
		"}",                                      // 10
		"[GoType(\"num:int64\")] partial struct Duration;",                                      // 11 sample, GoType with an argument
		"[GoRecv] public static (nint n, error err) Read(this ref Reader b) {",                  // 12 sample, GoRecv
		"internal static nint format(this Duration d, [GoArrayDims(32)] ж<array<byte>> Ꮡbuf) {", // 13 sample, GoArrayDims
		"[GoStr] public static (rune r, nint size) DecodeRuneInString(sstring s) {",             // 14 sample, GoStr
		"```",         // 15
		"",            // 16
		"| Go | C# |", // 17
		"|---|---|",   // 18
		"| `type Float float64` | `[GoType(\"num:float64\")] public partial struct Float;` |", // 19 table, GoType with an argument
		"| a parameter | `[GoParamDims(4)] array<byte> a` |",                                  // 20 table, GoParamDims
	}, "\n")

	want := map[int]string{
		3: "[GoRecv]", 6: "[GoType]", 7: "[GoEmbedded]", 8: "[GoTag]", 11: "[GoType(...)]", 12: "[GoRecv]",
		13: "[GoArrayDims]", 14: "[GoStr]", 19: "[GoType(...)]", 20: "[GoParamDims]",
	}

	findings, _ := retiredAttributesShown(planted, docsFaceLiftAttributes)
	got := map[int]string{}

	for _, f := range findings {
		got[f.line] = f.what
	}

	if len(findings) != len(want) {
		t.Errorf("the planted page: %d finding(s), want %d: %v", len(findings), len(want), findings)
	}

	for line, attr := range want {
		if !strings.Contains(got[line], "shows "+attr) {
			t.Errorf("the planted page, line %d: want a finding for %s, got %q", line, attr, got[line])
		}
	}

	named := map[string]bool{}

	for _, attr := range want {
		named[attr] = true
	}

	for _, a := range docsFaceLiftAttributes {
		if key := strings.TrimSuffix(a.String(), " in any form"); !named[key] {
			t.Errorf("the planted page has no line for %s; a row with no control is not known to fire", key)
		}
	}

	clean := strings.Join([]string{
		"# The same page after the face lift",
		"",
		"A pointer-receiver method takes its receiver as `this ref`.",
		"",
		"```csharp",
		"partial struct Scanner {",
		"    /*embed*/ public Reader Reader;",
		"    public @string Method; /*`json:\"method\"`*/",
		"}",
		"partial struct Duration /*num:int64*/;",
		"public static (nint n, error err) Read(this ref Reader b) {",
		"internal static nint format(this Duration d, /*[32]*/ж<array<byte>> Ꮡbuf) {",
		"public static (rune r, nint size) DecodeRuneInString(sstring s) {",
		"[GoInit] internal static void init() {",
		"```",
		"",
		"<!-- attribute-shown: a hand-written file keeps the attribute -->",
		"```csharp",
		"[GoRecv] internal static void userArenaKeep(this ref userArena a) {",
		"```",
	}, "\n")

	if findings, exceptions := retiredAttributesShown(clean, docsFaceLiftAttributes); len(findings) != 0 || exceptions != 1 {
		t.Errorf("the face-lifted page: %d finding(s) and %d exception(s), want 0 and 1: %v", len(findings), exceptions, findings)
	}
}

// TestDocsShowNoRetiredAttribute is the guard over the committed pages.
func TestDocsShowNoRetiredAttribute(t *testing.T) {
	pages, agentFiles := retiredAttributePages(t)
	var problems []string
	exceptions, reach := 0, 0

	for rel, text := range pages {
		findings, used := retiredAttributesShown(text, docsRetiredAttributes)
		exceptions += used

		for _, f := range findings {
			problems = append(problems, fmt.Sprintf("%s:%d: %s", rel, f.line, f.what))
		}

		// The reach reading: every face-lift attribute, listed or not, with the marked blocks counted
		// in. While any page shows or declares one, a zero here means the scanner read nothing.
		all, marked := retiredAttributesShown(text, docsFaceLiftAttributes)
		reach += len(all) + marked
	}

	if reach == 0 {
		t.Fatalf("VACUOUS: no page shows or declares any face-lift attribute, so the scanner cannot be seen to reach the %d pages; if the pages no longer name one at all, replace this reading with another proof of reach", len(pages))
	}

	t.Logf("read %d pages (%d rules and skills files); %d attribute(s) listed; %d declared exception(s); reach reading %d", len(pages), agentFiles, len(docsRetiredAttributes), exceptions, reach)

	sort.Strings(problems)

	if len(problems) > 0 {
		t.Fatalf("%d place(s) show an attribute the converter no longer writes. Regenerate the sample from the converter's output, or, where hand-written or generated code is meant, declare it with an attribute-shown marker (see this file's header):\n  %s", len(problems), strings.Join(problems, "\n  "))
	}
}
