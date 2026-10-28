// pcensus_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"fmt"
	"os"
	"os/exec"
	"path/filepath"
	"strings"
	"testing"
)

// The instrument's own guard (docs/PLAN-parity-train.md §1 "#22": "an instrument fault reads as a seat fault, so every
// arm is planted"). Every item carries a GOOD plant, the seat's rendering, which must be attributed to exactly that
// item, and BAD plants, near misses, which must be refused, each named by its own text in the refusal.

func diffOf(file string, rem, add []string) string {
	var b strings.Builder
	fmt.Fprintf(&b, "diff --git a/%s b/%s\n--- a/%s\n+++ b/%s\n@@ -1,%d +1,%d @@\n", file, file, file, file, len(rem), len(add))
	for _, l := range rem {
		b.WriteString("-" + l + "\r\n") // CR on purpose: the classifier reads CR-normalized
	}
	for _, l := range add {
		b.WriteString("+" + l + "\n")
	}
	return b.String()
}

type plant struct {
	name     string
	file     string
	rem, add []string
	want     string // the one label every pair reads, or the refusal kind's prefix
}

const prodFile = "src/core/p/p.cs"
const testFile = "src/core/p/p_test.cs"

func one(name, file, rem, add, want string) plant {
	return plant{name, file, []string{rem}, []string{add}, want}
}

var plants = []plant{
	// N1 and D1 (#20)
	one("N1 good", prodFile, `if (err != default!) {`, `if (err != nil) {`, "N1"),
	one("N1 good ==", prodFile, `return f == default!;`, `return f == nil;`, "N1"),
	one("N1 bad: nil on the other operand", prodFile, `if (err != default!) {`, `if (nil != err) {`, "UNCLASSIFIED"),
	one("D1 good", prodFile, `@string s = default!;`, `@string s = default;`, "D1"),
	one("D1 bad: the value changed", prodFile, `@string s = default!;`, `@string s = "";`, "UNCLASSIFIED"),
	one("N1+D1 composition", prodFile, `if (err != default!) { x = default!; }`, `if (err != nil) { x = default; }`, "N1+D1"),
	one("N1 beside a slice compare that keeps default", prodFile, `if (err == default! && s != default!) {`, `if (err == nil && s != default) {`, "N1"),
	one("D1 on a compare alone", prodFile, `if (s == default!) {`, `if (s == default) {`, "D1"),
	one("N1 bad: the kept compare changed", prodFile, `if (err == default! && s != default!) {`, `if (err == nil && t != default) {`, "UNCLASSIFIED"),
	// Z1 (#21)
	one("Z1 good", prodFile, `var b = new Buffer(nil);`, `var b = new Buffer();`, "Z1"),
	one("Z1 good generic", prodFile, `var g = new G<nint>(nil);`, `var g = new G<nint>();`, "Z1"),
	one("Z1 bad: the type changed", prodFile, `var b = new Buffer(nil);`, `var b = new Reader();`, "UNCLASSIFIED"),
	// C1 (#23)
	one("C1 good", prodFile, `if (c == (rune)'a') {`, `if (c == 'a') {`, "C1"),
	one("C1 good byte target", prodFile, `b[i] = (byte)((rune)'x');`, `b[i] = (byte)'x';`, "C1"),
	one("C1 bad: the literal changed", prodFile, `if (c == (rune)'a') {`, `if (c == 'b') {`, "UNCLASSIFIED"),
	// N1 stage 2 (#24)
	one("N1b good", prodFile, `if (s == default) {`, `if (s == nil) {`, "N1b"),
	one("N1b bad: the operand changed", prodFile, `if (s == default) {`, `if (t == nil) {`, "UNCLASSIFIED"),
	// R5 (#25)
	one("R5 good param", prodFile, `public static void TestX(ж<testing.T> Ꮡt) {`, `public static void TestX(ж<testing.T> t) {`, "R5"),
	one("R5 good use", testFile, `Ꮡt.Errorf("bad"u8);`, `t.Errorf("bad"u8);`, "R5"),
	one("R5 bad: a field's Ꮡ is not a parameter's", prodFile, `x = Ꮡb.of(Buffer.Ꮡoff);`, `x = Ꮡb.of(Buffer.off);`, "UNCLASSIFIED"),
	one("R5+N1 composition", testFile, `if (Ꮡt.err != default!) {`, `if (t.err != nil) {`, "N1+R5"),
	// N2 (#26)
	one("N2 good", prodFile, `var n = (nint)(byte)k;`, `var n = (nint)k;`, "N2"),
	one("N2 bad: the outer cast changed", prodFile, `var n = (nint)(byte)k;`, `var n = (long)k;`, "UNCLASSIFIED"),
	// F1, DIR, FOR, SHIFT (#27)
	one("F1 good open", prodFile, `((Action)(() => {`, `func(() => {`, "F1"),
	one("F1 good close", prodFile, `}))();`, `})();`, "F1"),
	one("F1 good value form", prodFile, `var n = ((Func<nint>)(() => {`, `var n = func(nint () => {`, "F1"),
	one("F1 bad: a call dropped", prodFile, `}))();`, `});`, "UNCLASSIFIED"),
	one("DIR good send", prodFile, `var c = ch.WithDirection(GoChanDir.Send);`, `var c = ch.AsSendOnly();`, "DIR"),
	one("DIR good recv", prodFile, `f(ch.WithDirection(GoChanDir.Recv));`, `f(ch.AsRecvOnly());`, "DIR"),
	one("DIR bad: the direction flipped", prodFile, `var c = ch.WithDirection(GoChanDir.Send);`, `var c = ch.AsRecvOnly();`, "UNCLASSIFIED"),
	one("DIR bad: the nil channel's static spelling", prodFile, `var c = ch.WithDirection(GoChanDir.Recv);`, `var c = ch.RecvOnly;`, "UNCLASSIFIED"),
	one("FOR good while", prodFile, `while (ᐧ) {`, `for (;;) {`, "FOR"),
	one("FOR good for", prodFile, `for (nint i = 0; ᐧ ; i++) {`, `for (nint i = 0; ; i++) {`, "FOR"),
	one("FOR bad: the post statement changed", prodFile, `for (nint i = 0; ᐧ ; i++) {`, `for (nint i = 0; ; i--) {`, "UNCLASSIFIED"),
	one("SHIFT good", prodFile, `x <<= (int)(3);`, `x <<= 3;`, "SHIFT"),
	one("SHIFT bad: a non-constant count", prodFile, `x <<= (int)(n);`, `x <<= n;`, "UNCLASSIFIED"),
	// H2 (#29): a join
	{"H2 good fold", prodFile, []string{`    ref var a = ref heap<nint>(out var Ꮡa);`, `    a = 10;`}, []string{`    ref var a = ref heap(10, out var Ꮡa);`}, "H2"},
	{"H2 bad: another variable's assignment", prodFile, []string{`    ref var a = ref heap<nint>(out var Ꮡa);`, `    b = 10;`}, []string{`    ref var a = ref heap(10, out var Ꮡa);`}, "UNPAIRED"},
	// L1 (#30)
	one("L1 good argument", prodFile, `f(new nint[]{1, 2, 3}.slice());`, `f([1, 2, 3]);`, "L1"),
	one("L1 good typed local", prodFile, `var x = new nint[]{1, 2}.slice();`, `slice<nint> x = [1, 2];`, "L1"),
	one("L1 good multi-line open", testFile, `var tests = new testCase[]{`, `slice<testCase> tests = [`, "L1"),
	one("L1 good multi-line close", testFile, `}.slice();`, `];`, "L1"),
	one("L1 bad: an element changed", prodFile, `f(new nint[]{1, 2, 3}.slice());`, `f([1, 2, 4]);`, "UNCLASSIFIED"),
	// P1, P2 (#31, #32)
	one("P1 good deref", prodFile, `var n = (~p).next;`, `var n = p.next;`, "P1"),
	one("P1 good value", prodFile, `e.Value.prev.Value.next = x;`, `e.prev.next = x;`, "P1"),
	one("P1 bad: the field changed", prodFile, `var n = (~p).next;`, `var n = p.prev;`, "UNCLASSIFIED"),
	one("P2 good", prodFile, `var off = Ꮡb.of(Buffer.Ꮡoff);`, `var off = Ꮡb.Ꮡoff;`, "P2"),
	one("R5+P2 composition", prodFile, `var off = Ꮡb.of(Buffer.Ꮡoff);`, `var off = b.Ꮡoff;`, "R5+P2"),
	one("P2 bad: the field changed", prodFile, `var off = Ꮡb.of(Buffer.Ꮡoff);`, `var off = Ꮡb.Ꮡbuf;`, "UNCLASSIFIED"),
	// E1 (#19): `) {` at the end of a line, and the else-if join
	one("E1 good brace", prodFile, `if (x > 0){`, `if (x > 0) {`, "E1"),
	{"E1 good else-if join", prodFile, []string{`    } else`, `    if (x > 0) {`}, []string{`    } else if (x > 0) {`}, "E1"},
	{"E1 bad: the condition changed", prodFile, []string{`    } else`, `    if (x > 0) {`}, []string{`    } else if (x > 1) {`}, "UNCLASSIFIED"},
	one("E1 good brace before a comment", prodFile, `if (x > 0){ // why`, `if (x > 0) { // why`, "E1"),
	{"E1 good init-clause else join", prodFile, []string{`    } else`, `    {`}, []string{`    } else {`}, "E1"},
	one("E1 bad: the comment changed", prodFile, `if (x > 0){ // why`, `if (x > 0) { // how`, "UNCLASSIFIED"),
	{"E1 bad: the init-clause join gained code", prodFile, []string{`    } else`, `    {`}, []string{`    } else { f();`}, "UNCLASSIFIED"},
	// test-refresh-1262: a, b, c, and the position map
	one("TRa good", testFile, `    });`, `    }, GoStack.Bounded);`, "TRa"),
	one("TRa good launch", testFile, `        goǃ(Ꮡs.send, (nint)7);`, `        goǃ(Ꮡs.send, (nint)7, GoStack.Bounded);`, "TRa"),
	one("TRa good close with arguments", testFile, `    }, @out);`, `    }, @out, GoStack.Bounded);`, "TRa"),
	one("TRa good golden", "src/tests/Behavioral/G/main.cs.target", `    });`, `    }, GoStack.Bounded);`, "TRa"),
	one("TRa bad: not a launch", testFile, `    f(x);`, `    f(x, GoStack.Bounded);`, "UNCLASSIFIED"),
	one("TRa bad: not the last argument", testFile, `        goǃ(f, x);`, `        goǃ(f, GoStack.Bounded, x);`, "UNCLASSIFIED"),
	one("TRa bad: in a production file", prodFile, `    });`, `    }, GoStack.Bounded);`, "TEST-ONLY(TRa)"),
	one("TRb good", testFile, `if (Ꮡ(x, i) != Ꮡ(y, j)) {`, `if (Ꮡelem(x, i) != Ꮡelem(y, j)) {`, "TRb"),
	one("TRb good at form", testFile, `ok = p.at<uint16>(0) == Ꮡ(dst, 2);`, `ok = p.Ꮡelem<uint16>(0) == Ꮡelem(dst, 2);`, "TRb"),
	one("TRb bad: the index changed", testFile, `if (Ꮡ(x, i) != Ꮡ(y, j)) {`, `if (Ꮡelem(x, i) != Ꮡelem(y, k)) {`, "UNCLASSIFIED"),
	one("TRc good defer", testFile, `        defer(() => srv.Close(), ref ᒐ);`, `        defer(ᴛ0 => ᴛ0.Close(), srv, ref ᒐ);`, "TRc"),
	one("TRc good go", testFile, `        goǃ(() => w.run(ctx));`, `        goǃ(ᴛ0 => ᴛ0.run(ctx), w);`, "TRc"),
	one("TRc bad: the receiver changed", testFile, `        defer(() => srv.Close(), ref ᒐ);`, `        defer(ᴛ0 => ᴛ0.Close(), cli, ref ᒐ);`, "UNCLASSIFIED"),
	one("TRc bad: the method changed", testFile, `        defer(() => srv.Close(), ref ᒐ);`, `        defer(ᴛ0 => ᴛ0.Stop(), srv, ref ᒐ);`, "UNCLASSIFIED"),
	one("TRc bad: `ref ᒐ` dropped", testFile, `        defer(() => srv.Close(), ref ᒐ);`, `        defer(ᴛ0 => ᴛ0.Close(), srv);`, "UNCLASSIFIED"),
	one("TRc bad: defer became goǃ", testFile, `        defer(() => srv.Close(), ref ᒐ);`, `        goǃ(ᴛ0 => ᴛ0.Close(), srv, ref ᒐ);`, "UNCLASSIFIED"),
	one("TRc bad: in a production file", prodFile, `        defer(() => srv.Close(), ref ᒐ);`, `        defer(ᴛ0 => ᴛ0.Close(), srv, ref ᒐ);`, "TEST-ONLY(TRc)"),
	one("MAP good", testFile, `[assembly: GoPositionMap("AAA")]`, `[assembly: GoPositionMap("AAB")]`, "MAP"),
	// A change no item makes, and a removal with nothing beside it.
	one("bad: an unrelated rewrite", prodFile, `return x + 1;`, `return x + 2;`, "UNCLASSIFIED"),
	{"bad: a lone removal", prodFile, []string{`    x = 1;`}, nil, "UNPAIRED"},
}

func TestPlants(t *testing.T) {
	c, _ := newClassifier("", false)
	for _, p := range plants {
		tl := c.classifyDiff(diffOf(p.file, p.rem, p.add))
		refusal := strings.ToUpper(p.want[:1]) == p.want[:1] && (strings.HasPrefix(p.want, "UNCLASSIFIED") ||
			strings.HasPrefix(p.want, "UNPAIRED") || strings.HasPrefix(p.want, "TEST-ONLY") || strings.HasPrefix(p.want, "AMBIGUOUS"))
		if refusal {
			if len(tl.refused) == 0 {
				t.Errorf("%s: wanted %s, but every pair was attributed: %v", p.name, p.want, tl.labels)
				continue
			}
			r := tl.refused[0]
			if !strings.HasPrefix(r.kind, p.want) {
				t.Errorf("%s: wanted %s, refused as %s", p.name, p.want, r.kind)
			}
			// The refusal names its site: the planted text itself.
			site := first(p.rem) + first(p.add)
			if !strings.Contains(r.rem+r.add, strings.TrimSpace(firstNonEmpty(p.rem, p.add))) {
				t.Errorf("%s: the refusal does not name its site %q: -%q +%q", p.name, site, r.rem, r.add)
			}
			continue
		}
		if len(tl.refused) > 0 {
			r := tl.refused[0]
			t.Errorf("%s: wanted %s, refused as %s\n  - %s\n  + %s", p.name, p.want, r.kind, r.rem, r.add)
			continue
		}
		if len(tl.labels) != 1 || tl.labels[p.want] == 0 {
			t.Errorf("%s: wanted every pair to read %s, read %v", p.name, p.want, tl.labels)
		}
	}
}

func firstNonEmpty(a, b []string) string {
	if len(a) > 0 {
		return a[0]
	}
	return first(b)
}

// S1 (#28) is structural: the if-chain over exprᴛN is deleted, a pattern switch inserted, and the bodies it keeps
// re-indent. The bad plant changes a kept body's text inside the same block.
func TestBlockPlants(t *testing.T) {
	old := []string{
		`    var exprᴛ1 = time.Now().Weekday();`,
		`    if (exprᴛ1 == time.Saturday || exprᴛ1 == time.Sunday) {`,
		`        fmt.Println("It's the weekend");`,
		`    }`,
		`    else { /* default: */`,
		`        fmt.Println("It's a weekday");`,
		`    }`,
	}
	good := []string{
		`    switch (time.Now().Weekday()) {`,
		`    case var ᴛ when ᴛ == time.Saturday || ᴛ == time.Sunday:`,
		`        fmt.Println("It's the weekend");`,
		`        break;`,
		`    default:`,
		`        fmt.Println("It's a weekday");`,
		`        break;`,
		`    }`,
	}
	c, _ := newClassifier("", false)
	tl := c.classifyDiff(diffOf(prodFile, old, good))
	if len(tl.refused) != 0 || len(tl.labels) != 1 || tl.labels["S1"] == 0 {
		t.Errorf("S1 good: wanted every line to read S1, read %v, refused %v", tl.labels, tl.refused)
	}

	bad := append([]string(nil), good...)
	bad[5] = `        fmt.Println("It's a weekend day");`
	tl = c.classifyDiff(diffOf(prodFile, old, bad))
	if len(tl.refused) == 0 || !strings.Contains(tl.refused[0].rem+tl.refused[0].add, "weekend day") &&
		!strings.Contains(tl.refused[len(tl.refused)-1].rem+tl.refused[len(tl.refused)-1].add, "weekend day") {
		t.Errorf("S1 bad: a changed body inside the block was not refused by name: %v", tl.refused)
	}

	// Without S1's anchors the same deletions are refused: the block rule is not a licence outside its own hunk.
	tl = c.classifyDiff(diffOf(prodFile, []string{`    do {`, `    } while (false);`}, nil))
	if len(tl.refused) != 2 {
		t.Errorf("S1 anchorless: wanted both lines refused, refused %d (%v)", len(tl.refused), tl.labels)
	}
}

// -items restricts attribution: a pair only an excluded item explains is refused, so a seat's golden check reads
// "every changed line is MY mechanism" (§3.3 step 4).
func TestItemsRestriction(t *testing.T) {
	c, err := newClassifier("N1,D1", false)
	if err != nil {
		t.Fatal(err)
	}
	tl := c.classifyDiff(diffOf(prodFile, []string{`var b = new Buffer(nil);`}, []string{`var b = new Buffer();`}))
	if len(tl.refused) != 1 {
		t.Errorf("a Z1 pair under -items N1,D1 was not refused: %v", tl.labels)
	}
	if _, err := newClassifier("N1,NOPE", false); err == nil {
		t.Error("an unknown item ID was accepted")
	}
}

// -test-only refuses a production file whatever its pairs read (the test-refresh-1262 rule 2).
func TestTestOnlyFlag(t *testing.T) {
	c, _ := newClassifier("", true)
	tl := c.classifyDiff(diffOf(prodFile, []string{`if (err != default!) {`}, []string{`if (err != nil) {`}))
	if len(tl.refused) != 1 || tl.refused[0].kind != "NOT-A-TEST-FILE" {
		t.Errorf("-test-only accepted a production file: %v %v", tl.labels, tl.refused)
	}
}

// Every measure counts its planted occurrences in its planted set and nothing in a near miss, over a planted
// checkout read by readTree.
func TestCountPlants(t *testing.T) {
	root := t.TempDir()
	write := func(rel, text string) {
		p := filepath.Join(root, filepath.FromSlash(rel))
		if err := os.MkdirAll(filepath.Dir(p), 0o755); err != nil {
			t.Fatal(err)
		}
		if err := os.WriteFile(p, []byte(text), 0o644); err != nil {
			t.Fatal(err)
		}
	}
	// One occurrence of every measure's shape per planted file, CRLF as the corpus is; a near miss beside each.
	hits := map[string]string{
		"N1/cmp-default":          "if (e != default!) {}\r\nif (e != default) {}\r\n",
		"D1/default-bang":         "x = default!;\r\nx = default;\r\n",
		"Z1/new-nil":              "var b = new Buffer(nil);\r\nvar b = new Buffer();\r\n",
		"C1/rune-cast":            "r = (rune)'a';\r\nr = (rune)x;\r\n",
		"N1b/cmp-default-nobang":  "if (s == default) {}\r\nif (s == default!) {}\r\n",
		"R5/box-param":            "void F(ж<T> Ꮡx) {}\r\nvoid F(this ж<T> Ꮡx) {}\r\n",
		"N2/double-cast":          "n = (nint)(byte)k;\r\nn = (nint)(Named)k;\r\n",
		"F1/iife-close":           "}))();\r\n}));\r\n",
		"DIR/with-direction":      "c = ch.WithDirection(GoChanDir.Send);\r\nc = ch.Direction;\r\n",
		"FOR/while-true":          "while (ᐧ) {}\r\nwhile (x) {}\r\n",
		"FOR/for-true":            "for (i = 0; ᐧ ; i++) {}\r\nfor (i = 0; x ; i++) {}\r\n",
		"SHIFT/shift-assign-cast": "x <<= (int)(3);\r\nx <<= (int)(n);\r\n",
		"S1/tag-temp":             "var exprᴛ1 = f();\r\nvar expr = f();\r\n",
		"S1/do-while-false":       "} while (false);\r\n} while (x);\r\n",
		"H2/heap-decl":            "ref var a = ref heap<nint>(out var Ꮡa);\r\nref var a = ref heap(1, out var Ꮡa);\r\n",
		"L1/slice-literal":        "f(new nint[]{1, 2}.slice());\r\nf(new T[]{new U{1}}.slice());\r\n",
		"P1/deref-select":         "n = (~p).next;\r\nn = (~p);\r\n",
		"P1/value-select":         "n = p.Value.next;\r\nn = p.Value;\r\n",
		"P2/of-field":             "o = Ꮡb.of(Buffer.Ꮡoff);\r\no = Ꮡb.of(Buffer.off);\r\n",
		"E1/else-newline-if":      "} else\r\n    if (x) {}\r\n} else if (x) {}\r\n",
		"E1/paren-brace-eol":      "if (x){\r\nif (x) {\r\n",
		"TRa/bounded":             "}, GoStack.Bounded);\r\n});\r\n",
		"TRb/elem":                "Ꮡelem(x, i)\r\nꮡelement(x)\r\n",
		"TRc/receiver-snapshot":   "defer(ᴛ0 => ᴛ0.Close(), s, ref ᒐ);\r\ndefer(() => s.Close(), ref ᒐ);\r\n",
	}
	sets := []string{"src/core/p%d/p.cs", "src/core/p%d/p_test.cs", "src/tests/Behavioral/P%d/P.cs.target"}
	names := []string{}
	for k := range hits {
		names = append(names, k)
	}
	for i, k := range names {
		for _, s := range sets {
			write(fmt.Sprintf(s, i), hits[k])
		}
	}
	write("src/core/golib/g.cs", "x = default!;\r\n")         // golib is never corpus
	write("src/core/p0/bin/Debug/x.cs", "x = default!;\r\n")  // nor is build output
	write("src/tests/Behavioral/P/P.cs", "x = default!;\r\n") // a golden is the .cs.target only
	files, err := readTree(root)
	if err != nil {
		t.Fatal(err)
	}
	var all []measure
	for _, it := range items {
		all = append(all, it.Measures...)
	}
	if len(all) != len(hits) {
		t.Fatalf("%d measures, %d planted: every measure needs its plant", len(all), len(hits))
	}
	for _, r := range census(files, all) {
		key := r.Item + "/" + r.Measure
		// Each plant file holds the shape once; measures sharing text with another's plant read that too, so the
		// count is checked against the planted texts themselves.
		want := 0
		for _, k := range names {
			want += len(measureOf(all, key).Pattern.FindAllIndex([]byte(hits[k]), -1))
		}
		if r.Occurrences != want || want == 0 {
			t.Errorf("%s %s: counted %d, planted %d", key, r.Set, r.Occurrences, want)
		}
		if own := len(measureOf(all, key).Pattern.FindAllIndex([]byte(hits[key]), -1)); own != 1 {
			t.Errorf("%s: its own plant holds %d occurrences, want exactly 1 (the near miss must not count)", key, own)
		}
	}
}

func measureOf(all []measure, key string) measure {
	for _, m := range all {
		if m.Item+"/"+m.Name == key {
			return m
		}
	}
	panic(key)
}

// The census at master 5931bad29b, the plan's base, read through git with no checkout. testdata/counts-5931bad29b.tsv
// is the reading of record; GO2CS_PCENSUS_UPDATE=1 rewrites it. The rows the plan's footprint table states and this
// census reproduces exactly are pinned separately, so a change to either reads red by name.
func TestCountsAtPlanBase(t *testing.T) {
	const base = "5931bad29b"
	repo, err := filepath.Abs(filepath.Join("..", "..", "..", ".."))
	if err != nil {
		t.Fatal(err)
	}
	if err := exec.Command("git", "-C", repo, "cat-file", "-e", base+"^{commit}").Run(); err != nil {
		t.Skipf("the plan base %s is not in this clone: %v", base, err)
	}
	files, err := readRef(repo, base)
	if err != nil {
		t.Fatal(err)
	}
	n := map[string]int{}
	for _, f := range files {
		n[f.set]++
	}
	if n[setProd] != 3221 || n[setTest] != 1148 || n[setGolden] != 925 {
		t.Errorf("file sets at %s: prod %d test %d golden %d, the plan's 3,221 / 1,148 / 925", base, n[setProd], n[setTest], n[setGolden])
	}

	var all []measure
	for _, it := range items {
		all = append(all, it.Measures...)
	}
	var b strings.Builder
	b.WriteString("# pcensus count at 5931bad29b (the parity plan's base). Written by TestCountsAtPlanBase with GO2CS_PCENSUS_UPDATE=1.\n")
	got := map[string]row{}
	for _, r := range census(files, all) {
		fmt.Fprintf(&b, "%s\t%s\t%s\t%d\t%d\n", r.Item, r.Measure, r.Set, r.Occurrences, r.Files)
		got[r.Item+"/"+r.Measure+"/"+r.Set] = r
	}

	golden := filepath.Join("testdata", "counts-5931bad29b.tsv")
	if os.Getenv("GO2CS_PCENSUS_UPDATE") == "1" {
		if err := os.WriteFile(golden, []byte(b.String()), 0o644); err != nil {
			t.Fatal(err)
		}
	}
	want, err := os.ReadFile(golden)
	if err != nil {
		t.Fatal(err)
	}
	// The golden is checked out with CRLF where the tree pins eol=crlf (a windows checkout); compare line content.
	if strings.ReplaceAll(string(want), "\r\n", "\n") != b.String() {
		t.Errorf("the census at %s moved from %s:\n%s", base, golden, b.String())
	}

	// The plan's table (docs/PLAN-parity-train.md §1), where this census spells the same shape: occurrences / files.
	exact := []struct {
		key        string
		occ, files int // files -1: the plan states none
	}{
		{"N1/cmp-default/prod", 9105, 928}, {"N1/cmp-default/test", 10066, 652}, {"N1/cmp-default/golden", 565, 180},
		{"D1/default-bang/prod", 34094, 1369}, {"D1/default-bang/test", 17778, 762}, {"D1/default-bang/golden", 1571, 385},
		{"C1/rune-cast/prod", 7949, 347}, {"C1/rune-cast/test", 1966, 169}, {"C1/rune-cast/golden", 171, 53},
		{"S1/tag-temp/prod", 1309, -1}, {"S1/tag-temp/test", 376, -1}, {"S1/tag-temp/golden", 50, -1},
		{"S1/do-while-false/prod", 113, -1}, {"S1/do-while-false/test", 7, -1}, {"S1/do-while-false/golden", 11, -1},
		{"SHIFT/shift-assign-cast/prod", 257, 91}, {"SHIFT/shift-assign-cast/test", 69, 18}, {"SHIFT/shift-assign-cast/golden", 2, -1},
		{"F1/iife-close/prod", 6, -1}, {"F1/iife-close/test", 96, 41}, {"F1/iife-close/golden", 64, 36},
		{"DIR/with-direction/prod", 44, 15}, {"DIR/with-direction/test", 49, 26}, {"DIR/with-direction/golden", 21, 7},
		{"FOR/while-true/prod", 713, -1}, {"FOR/while-true/test", 279, -1},
		{"E1/paren-brace-eol/prod", 3863, -1}, {"E1/paren-brace-eol/test", 1216, -1}, {"E1/paren-brace-eol/golden", 78, -1},
		{"E1/else-newline-if/test", 408, -1}, {"E1/else-newline-if/golden", 5, -1},
	}
	for _, e := range exact {
		r, ok := got[e.key]
		if !ok || r.Occurrences != e.occ || (e.files >= 0 && r.Files != e.files) {
			t.Errorf("%s at %s: %d / %d, the plan's %d / %d", e.key, base, r.Occurrences, r.Files, e.occ, e.files)
		}
	}
	// `for (…; ᐧ ;…)` is the plan's "94 (core total)": production, tests and goldens together.
	if s := got["FOR/for-true/prod"].Occurrences + got["FOR/for-true/test"].Occurrences + got["FOR/for-true/golden"].Occurrences; s != 94 {
		t.Errorf("FOR/for-true total at %s: %d, the plan's 94", base, s)
	}
}

// Positive controls from real history: the golden re-baselines of the three seats behind the test-refresh-1262
// class read entirely as their shapes (a whole new golden is reported, never paired). 5895dfe67's two re-baselined
// pairs read as shape c; its other hunks insert new test source and are refused, which is the reading a re-baseline
// check wants.
func TestRealGoldenControls(t *testing.T) {
	repo, err := filepath.Abs(filepath.Join("..", "..", "..", ".."))
	if err != nil {
		t.Fatal(err)
	}
	c, _ := newClassifier("", false)
	for _, k := range []struct {
		commit  string
		label   string
		pairs   int
		refused int
	}{
		{"4b19dd0c4", "TRa", 55, 0},
		{"0400b63f7", "TRa", 5, 0},
		{"b31294be2", "TRb", 17, 0},
		{"5895dfe67", "TRc", 2, 85},
	} {
		out, err := exec.Command("git", "-C", repo, "show", "--format=", "-U0", k.commit, "--", "src/tests/Behavioral/*.cs.target").Output()
		if err != nil {
			t.Skipf("%s is not in this clone: %v", k.commit, err)
		}
		tl := c.classifyDiff(string(out))
		if tl.labels[k.label] != k.pairs || len(tl.refused) != k.refused {
			t.Errorf("%s: %s=%d refused %d, want %d and %d (%v)", k.commit, k.label, tl.labels[k.label], len(tl.refused), k.pairs, k.refused, tl.labels)
		}
	}
}
