// main_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"os"
	"strings"
	"testing"
)

func files(m map[string]string) fileReader {
	return func(path string) ([]string, error) {
		text, ok := m[path]

		if !ok {
			return nil, os.ErrNotExist
		}

		return splitLines(text), nil
	}
}

const golden = `namespace go;

partial class main_package {

[GoType] partial struct Reader {
    [GoEmbedded] public io.Reader Reader;
    [GoTag(@"json:""n""")]
    public nint N;
}

[GoRecv] public static nint Read(this ref Reader r, slice<byte> p) {
    r.N += len(p);
    return r.N;
}

internal static void Main() {
    var r = new Reader();
    fmt.Println(r.Read(default!));
}

} // end main_package
`

// lifted is the same file after the face lift: marks removed in place, the tag line folded into its
// field, so the file is one line shorter from there on.
const lifted = `namespace go;

partial class main_package {

partial struct Reader {
    /*embed*/ public io.Reader Reader;
    public nint N; /*` + "`json:\"n\"`" + `*/
}

public static nint Read(this ref Reader r, slice<byte> p) {
    r.N += len(p);
    return r.N;
}

internal static void Main() {
    var r = new Reader();
    fmt.Println(r.Read(default!));
}

} // end main_package
`

func page(parts ...string) []string { return strings.Split(strings.Join(parts, "\n"), "\n") }

func classes(outcomes []outcome) string {
	var c []string

	for _, o := range outcomes {
		c = append(c, o.class)
	}

	return strings.Join(c, ",")
}

// The control the tool is held to: a page whose samples were read from a file reproduces byte for
// byte from that file, in each of the forms the pages use.
func TestSamplesReproduceFromTheirOwnFile(t *testing.T) {
	in := page(
		"An explicit range.",
		"",
		"<!-- source: g/main.cs.target:11-14 -->",
		"```csharp",
		"[GoRecv] public static nint Read(this ref Reader r, slice<byte> p) {",
		"    r.N += len(p);",
		"    return r.N;",
		"}",
		"```",
		"",
		"Two ranges, an ellipsis between them.",
		"",
		"<!-- source: g/main.cs.target:5 and :11 -->",
		"```csharp",
		"[GoType] partial struct Reader {",
		"…",
		"[GoRecv] public static nint Read(this ref Reader r, slice<byte> p) {",
		"```",
		"",
		"An envelope: the comment cites the stretch, the sample is abridged from it.",
		"",
		"<!-- source: g/main.cs.target:5-19 -->",
		"```csharp",
		"[GoType] partial struct Reader {",
		"    [GoEmbedded] public io.Reader Reader;",
		"    …",
		"}",
		"",
		"…",
		"",
		"internal static void Main() {",
		"    var r = new Reader();",
		"```",
		"",
		"Shown without the four spaces of its struct, inside a list item.",
		"",
		"- item",
		"",
		"  <!-- source: g/main.cs.target:6-8 (shown without its indentation) -->",
		"  ```csharp",
		"  [GoEmbedded] public io.Reader Reader;",
		"  [GoTag(@\"json:\"\"n\"\"\")]",
		"  public nint N;",
		"  ```",
	)

	src := files(map[string]string{"g/main.cs.target": golden})
	out, outcomes := regenerate("p.md", in, src, src, false)

	if got := classes(outcomes); got != "same,same,same,same" {
		t.Errorf("outcomes %s, want four the same: %+v", got, outcomes)
	}

	if strings.Join(out, "\n") != strings.Join(in, "\n") {
		t.Errorf("the page changed:\n%s", strings.Join(out, "\n"))
	}
}

// PLANT: a sample that has drifted from its file is rewritten from the file.
func TestADriftedSampleIsRewritten(t *testing.T) {
	in := page(
		"<!-- source: g/main.cs.target:11-14 -->",
		"```csharp",
		"[GoRecv] public static nint Read(this ref Reader r, slice<byte> p) {",
		"    r.N += len(p);",
		"    return r.N + 1; // drifted by hand",
		"}",
		"```",
	)

	src := files(map[string]string{"g/main.cs.target": golden})
	out, outcomes := regenerate("p.md", in, src, src, false)

	if classes(outcomes) != "rewritten" {
		t.Fatalf("outcomes %+v, want one rewritten", outcomes)
	}

	if out[4] != "    return r.N;" || len(out) != len(in) {
		t.Errorf("the drifted line reads %q, want the file's line; page:\n%s", out[4], strings.Join(out, "\n"))
	}

	// The rewritten page is then stable.
	again, outcomes := regenerate("p.md", out, src, src, false)

	if classes(outcomes) != "same" || strings.Join(again, "\n") != strings.Join(out, "\n") {
		t.Errorf("a second run changed the page again: %+v", outcomes)
	}
}

// PLANT: a sample of the cited shape that shares less than half its lines with the file is another
// excerpt, and is refused, not overwritten.
func TestAnotherExcerptOfTheSameShapeIsRefused(t *testing.T) {
	in := page(
		"<!-- source: g/main.cs.target:11-14 -->",
		"```csharp",
		"internal static void Other() {",
		"    var x = 1;",
		"    fmt.Println(x);",
		"}",
		"```",
	)

	src := files(map[string]string{"g/main.cs.target": golden})
	out, outcomes := regenerate("p.md", in, src, src, false)

	if classes(outcomes) != "refused" || strings.Join(out, "\n") != strings.Join(in, "\n") {
		t.Errorf("outcomes %+v, want one refused and the page untouched", outcomes)
	}
}

// PLANT: a source comment naming a file that does not exist is refused by name.
func TestAMissingFileIsRefusedByName(t *testing.T) {
	in := page(
		"text",
		"",
		"<!-- source: g/gone.cs.target:1-2 -->",
		"```csharp",
		"namespace go;",
		"```",
	)

	src := files(map[string]string{"g/main.cs.target": golden})
	out, outcomes := regenerate("docs/p.md", in, src, src, false)

	if len(outcomes) != 1 || outcomes[0].class != "refused" {
		t.Fatalf("outcomes %+v, want one refused", outcomes)
	}

	o := outcomes[0]

	if o.page != "docs/p.md" || o.line != 3 || !strings.Contains(o.reason, "g/gone.cs.target") {
		t.Errorf("the refusal names %s:%d %q, want docs/p.md:3 and the file", o.page, o.line, o.reason)
	}

	if strings.Join(out, "\n") != strings.Join(in, "\n") {
		t.Errorf("a refused sample was changed")
	}

	// Present at the base and gone at the target is a stale sample: marked changed, so main exits 1.
	_, outcomes = regenerate("docs/p.md", in, files(map[string]string{"g/gone.cs.target": "namespace go;\n"}), src, false)

	if len(outcomes) != 1 || outcomes[0].class != "refused" || !outcomes[0].changed {
		t.Errorf("outcomes %+v, want one refused and marked changed", outcomes)
	}
}

// The regeneration itself: the file changes between the refs, and each sample follows its lines.
func TestSamplesFollowTheirLinesToTheTarget(t *testing.T) {
	in := page(
		"<!-- source: g/main.cs.target:11-14 -->",
		"```csharp",
		"[GoRecv] public static nint Read(this ref Reader r, slice<byte> p) {",
		"    r.N += len(p);",
		"    return r.N;",
		"}",
		"```",
		"",
		"<!-- source: g/main.cs.target:5-19 (abridged) -->",
		"```csharp",
		"[GoType] partial struct Reader {",
		"    [GoEmbedded] public io.Reader Reader;",
		"    [GoTag(@\"json:\"\"n\"\"\")]",
		"    public nint N;",
		"}",
		"",
		"…",
		"",
		"internal static void Main() {",
		"    var r = new Reader();",
		"```",
		"",
		"<!-- source: g/main.cs.target:16-19 -->",
		"```csharp",
		"internal static void Main() {",
		"    var r = new Reader();",
		"    fmt.Println(r.Read(default!));",
		"}",
		"```",
	)

	want := page(
		"<!-- source: g/main.cs.target:10-13 -->",
		"```csharp",
		"public static nint Read(this ref Reader r, slice<byte> p) {",
		"    r.N += len(p);",
		"    return r.N;",
		"}",
		"```",
		"",
		"<!-- source: g/main.cs.target:5-18 (abridged) -->",
		"```csharp",
		"partial struct Reader {",
		"    /*embed*/ public io.Reader Reader;",
		"    public nint N; /*`json:\"n\"`*/",
		"}",
		"",
		"…",
		"",
		"internal static void Main() {",
		"    var r = new Reader();",
		"```",
		"",
		"<!-- source: g/main.cs.target:15-18 -->",
		"```csharp",
		"internal static void Main() {",
		"    var r = new Reader();",
		"    fmt.Println(r.Read(default!));",
		"}",
		"```",
	)

	out, outcomes := regenerate("p.md", in, files(map[string]string{"g/main.cs.target": golden}), files(map[string]string{"g/main.cs.target": lifted}), false)

	if got := classes(outcomes); got != "rewritten,rewritten,rewritten" {
		t.Errorf("outcomes %s: %+v", got, outcomes)
	}

	if strings.Join(out, "\n") != strings.Join(want, "\n") {
		t.Errorf("got:\n%s\n\nwant:\n%s", strings.Join(out, "\n"), strings.Join(want, "\n"))
	}

	for _, o := range outcomes {
		if !o.changed {
			t.Errorf("%s:%d is not marked as read from a changed file", o.page, o.line)
		}
	}
}

// A run that ends inside a stretch that changed size takes the whole stretch and is listed for review.
func TestARunEndingInsideAResizedStretchIsListedForReview(t *testing.T) {
	in := page(
		"<!-- source: g/main.cs.target:5-7 -->",
		"```csharp",
		"[GoType] partial struct Reader {",
		"    [GoEmbedded] public io.Reader Reader;",
		"    [GoTag(@\"json:\"\"n\"\"\")]",
		"```",
	)

	// The run's last line, the tag's attribute line, is gone at the target: the run simply ends
	// before it. Nothing is guessed, so nothing is listed for review.
	out, outcomes := regenerate("p.md", in, files(map[string]string{"g/main.cs.target": golden}), files(map[string]string{"g/main.cs.target": lifted}), false)

	if len(outcomes) != 1 || outcomes[0].class != "rewritten" || outcomes[0].review {
		t.Fatalf("outcomes %+v, want one rewritten and not listed for review", outcomes)
	}

	if len(out) != 5 || out[3] != "    /*embed*/ public io.Reader Reader;" || out[4] != "```" {
		t.Errorf("want the struct line and the embed line, then the fence; got:\n%s", strings.Join(out, "\n"))
	}

	// Lines replaced by a different number of unrelated lines cannot be paired, so a run that ends
	// among them takes them all and says so.
	before := "keep\nalpha one\nbeta two\ngamma three\nend\n"
	after := "keep\ndelta four\nepsilon five\nzeta six\neta seven\ntheta eight\nend\n"
	in = page("<!-- source: f.txt:1-3 -->", "```", "keep", "alpha one", "beta two", "```")
	out, outcomes = regenerate("p.md", in, files(map[string]string{"f.txt": before}), files(map[string]string{"f.txt": after}), false)

	if len(outcomes) != 1 || outcomes[0].class != "rewritten" || !outcomes[0].review {
		t.Fatalf("outcomes %+v, want one rewritten and listed for review", outcomes)
	}

	if strings.Join(out, "|") != "<!-- source: f.txt:1-6 -->|```|keep|delta four|epsilon five|zeta six|eta seven|theta eight|```" {
		t.Errorf("got %s", strings.Join(out, "|"))
	}
}

// A hand-edited envelope sample is refused; -recite takes a sample cited by the wrong lines.
func TestHandEditsAreRefusedAndWrongLinesAreRecited(t *testing.T) {
	edited := page(
		"<!-- source: g/main.cs.target:5-19 -->",
		"```csharp",
		"[GoType] partial struct Reader { … }   // abridged on one line",
		"…",
		"internal static void Main() {",
		"```",
	)

	src := files(map[string]string{"g/main.cs.target": golden})

	for _, recite := range []bool{false, true} {
		if out, outcomes := regenerate("p.md", edited, src, src, recite); classes(outcomes) != "refused" || strings.Join(out, "\n") != strings.Join(edited, "\n") {
			t.Errorf("recite=%v: outcomes %+v, want one refused and the page untouched", recite, outcomes)
		}
	}

	miscited := page(
		"<!-- source: g/main.cs.target:1 -->",
		"```csharp",
		"[GoRecv] public static nint Read(this ref Reader r, slice<byte> p) {",
		"    …",
		"}",
		"```",
	)

	if _, outcomes := regenerate("p.md", miscited, src, src, false); classes(outcomes) != "refused" {
		t.Errorf("without -recite: outcomes %+v, want one refused", outcomes)
	}

	out, outcomes := regenerate("p.md", miscited, src, src, true)

	if classes(outcomes) != "rewritten" || out[0] != "<!-- source: g/main.cs.target:11-14 -->" || strings.Join(out[1:], "\n") != strings.Join(miscited[1:], "\n") {
		t.Errorf("with -recite: outcomes %+v, comment %q; want the comment corrected and the sample untouched", outcomes, out[0])
	}
}

func TestTheFreeFormsAreRefused(t *testing.T) {
	in := page(
		"<!-- source: the generator's output for g/main.cs.target, abridged -->",
		"```csharp",
		"namespace go;",
		"```",
		"",
		"<!-- source: g/main.cs.target:1 -->",
		"",
		"No block follows.",
	)

	src := files(map[string]string{"g/main.cs.target": golden})

	if _, outcomes := regenerate("p.md", in, src, src, false); classes(outcomes) != "refused,refused" {
		t.Errorf("outcomes %+v, want two refused", outcomes)
	}
}

func TestLineMap(t *testing.T) {
	m := diffLines(splitLines(golden), splitLines(lifted))

	cases := []struct {
		in, want span
		inside   bool
	}{
		{span{1, 4}, span{1, 4}, false},     // before any change
		{span{5, 5}, span{5, 5}, false},     // an even change, line for line
		{span{5, 9}, span{5, 8}, false},     // the whole struct, across the folded line
		{span{7, 8}, span{7, 7}, false},     // the tag line and its field, exactly the resized stretch
		{span{8, 8}, span{7, 7}, false},     // the field alone is the folded line
		{span{7, 7}, span{7, 6}, false},     // the tag's attribute line alone is gone
		{span{11, 14}, span{10, 13}, false}, // after it, shifted by one
		{span{16, 21}, span{15, 20}, false},
	}

	for _, c := range cases {
		if got, inside := m.carry(c.in); got != c.want || inside != c.inside {
			t.Errorf("carry(%v) = %v inside=%v, want %v inside=%v", c.in, got, inside, c.want, c.inside)
		}
	}

	if same := diffLines(splitLines(golden), splitLines(golden)); !same.same {
		t.Errorf("a file against itself is not reported the same")
	}
}
