package main

import (
	"bytes"
	"encoding/json"
	"errors"
	"os"
	"path/filepath"
	"regexp"
	"strings"
	"testing"
)

// hostNoteTemplates reads the two note literals from the host's own AllocsPerRun
// (src/core/testing/testing.cs), so a change to the wording fails here before it can make a
// reading run class every entry as "none".
func hostNoteTemplates(t *testing.T) (count, bytes string) {
	t.Helper()
	src, err := os.ReadFile(filepath.Join("..", "..", "core", "testing", "testing.cs"))
	if err != nil {
		t.Fatalf("the host source is not beside this tool: %v", err)
	}
	text := string(src)
	start := strings.Index(text, "NoteMeasurementUnitOnce(countUsable")
	if start < 0 {
		t.Fatal("testing.cs no longer calls NoteMeasurementUnitOnce(countUsable ...): re-read the host before trusting this tool")
	}
	end := strings.Index(text[start:], "return average;")
	if end < 0 {
		t.Fatal("testing.cs: no 'return average;' after the note")
	}
	literal := regexp.MustCompile(`\$?"((?:[^"\\]|\\.)*)"`)
	var parts []string
	for _, m := range literal.FindAllStringSubmatch(text[start:start+end], -1) {
		parts = append(parts, m[1])
	}
	split := -1
	for i, p := range parts {
		if strings.HasPrefix(p, "go2cs: testing.AllocsPerRun measured") {
			split = i
		}
	}
	if split <= 0 || !strings.HasPrefix(parts[0], "go2cs: testing.AllocsPerRun counted") {
		t.Fatalf("testing.cs: the two note forms are not where this tool expects them: %q", parts)
	}
	return strings.Join(parts[:split], ""), strings.Join(parts[split:], "")
}

func fill(template, counted, allocated, runs string) string {
	return strings.NewReplacer("{counted:N0}", counted, "{allocated:N0}", allocated, "{runs:N0}", runs).Replace(template)
}

func TestTheHostsOwnNoteLiteralsParse(t *testing.T) {
	countT, bytesT := hostNoteTemplates(t)
	for _, holes := range []string{"{counted:N0}", "{allocated:N0}", "{runs:N0}"} {
		if !strings.Contains(countT+bytesT, holes) {
			t.Fatalf("testing.cs's note no longer carries %s; the figures this tool reads have moved", holes)
		}
	}
	// N0 under three cultures' group separators.
	for _, sep := range []string{",", ".", "\u00a0"} {
		c := fill(countT, "1"+sep+"234", "56"+sep+"789", "10")
		n, err := parseNote(c)
		if err != nil || n == nil {
			t.Fatalf("sep %q: the host's COUNT note does not parse: %v\n%s", sep, err, c)
		}
		if n.unit != "COUNT" || n.count != 1234 || n.bytes != 56789 || n.runs != 10 || n.perRun != 123 {
			t.Errorf("sep %q: COUNT note read as %+v", sep, *n)
		}
		b := fill(bytesT, "", "56"+sep+"789", "10")
		if n, err = parseNote(b); err != nil || n == nil {
			t.Fatalf("sep %q: the host's BYTES note does not parse: %v\n%s", sep, err, b)
		}
		if n.unit != "BYTES" || n.count != -1 || n.bytes != 56789 || n.runs != 10 || n.perRun != 5678 {
			t.Errorf("sep %q: BYTES note read as %+v", sep, *n)
		}
	}
}

func TestPerRunIsAllocsPerRunsOwnFigure(t *testing.T) {
	countT, bytesT := hostNoteTemplates(t)
	// Integer division, floored at 1: what AllocsPerRun returned and the test printed.
	for _, c := range []struct {
		note string
		want int64
	}{
		{fill(countT, "10", "2,800", "10"), 1},
		{fill(countT, "7,500", "1", "100"), 75},
		{fill(countT, "3", "1", "10"), 1},
		{fill(bytesT, "", "880", "5"), 176},
		{fill(bytesT, "", "4", "10"), 1},
	} {
		n, err := parseNote(c.note)
		if err != nil || n == nil {
			t.Fatalf("%v", err)
		}
		if n.perRun != c.want {
			t.Errorf("per run %d, want %d: %s", n.perRun, c.want, c.note[:80])
		}
	}
}

// writeResults writes a results file (and optionally a comparison record) into a fresh package dir.
func writeResults(t *testing.T, pkg string, events []map[string]any, goVerdicts map[string]string) string {
	t.Helper()
	dir := filepath.Join(t.TempDir(), strings.ReplaceAll(pkg, "/", "_"))
	if err := os.MkdirAll(dir, 0o755); err != nil {
		t.Fatal(err)
	}
	data, _ := json.Marshal(map[string]any{"schemaVersion": 1, "package": pkg, "events": events})
	if err := os.WriteFile(filepath.Join(dir, resultsFileName), data, 0o644); err != nil {
		t.Fatal(err)
	}
	if goVerdicts != nil {
		data, _ = json.Marshal(map[string]any{"package": pkg, "go": goVerdicts})
		if err := os.WriteFile(filepath.Join(dir, comparisonFileName), data, 0o644); err != nil {
			t.Fatal(err)
		}
	}
	return dir
}

func ev(test, action string, records ...string) map[string]any {
	e := map[string]any{"package": "p", "test": test, "action": action, "elapsed": 0}
	if records != nil {
		e["records"] = records
		e["output"] = strings.Join(records, "\r\n")
	}
	return e
}

func runTool(t *testing.T, args ...string) (rows map[string][]string, err error) {
	t.Helper()
	var out, errOut bytes.Buffer
	err = run(args, &out, &errOut)
	rows = map[string][]string{}
	lines := strings.Split(strings.TrimSpace(out.String()), "\n")
	for _, l := range lines[min(1, len(lines)):] {
		f := strings.Split(l, "\t")
		rows[f[0]+" "+f[1]] = f
	}
	return rows, err
}

func writeEntries(t *testing.T, lines ...string) string {
	t.Helper()
	p := filepath.Join(t.TempDir(), "entries.txt")
	if err := os.WriteFile(p, []byte(strings.Join(lines, "\n")+"\n"), 0o644); err != nil {
		t.Fatal(err)
	}
	return p
}

// col indexes: package test cs go unit per_run count bytes runs note_on note
const (
	cCS, cGo, cUnit, cPerRun, cCount, cBytes, cRuns, cNoteOn = 2, 3, 4, 5, 6, 7, 8, 9
)

func TestPlantedArmsAreEachClassedByName(t *testing.T) {
	countT, bytesT := hostNoteTemplates(t)
	countNoteText := fill(countT, "100", "5,600", "100")
	bytesNoteText := fill(bytesT, "", "880", "5")
	passWithOutput := map[string]any{"package": "p", "test": "TestPassCRLF", "action": "pass",
		"output": "a log line\r\n" + countNoteText + "\r\nanother"}
	capped := ev("TestCapped", "fail", "x")
	capped["dropped"] = 3
	cappedTrailer := ev("TestCappedTrailer", "pass")
	cappedTrailer["output"] = "x\r\ntesting: 7 further log record(s) dropped after 1048576 characters"
	dir := writeResults(t, "p", []map[string]any{
		ev("TestCount", "run"),
		ev("TestCount", "fail", countNoteText, "got 1 allocs, want 0"),
		ev("TestBytes", "fail", "first", bytesNoteText),
		ev("TestNoteless", "pass"),
		ev("TestSkipped", "skip", "skipping: not on this host"),
		passWithOutput,
		ev("TestParent", "fail", countNoteText),
		ev("TestParent/child", "pass"),
		ev("TestParent/child/grandchild", "pass"),
		capped,
		cappedTrailer,
		ev("TestRunOnly", "run"),
		// One record that spans two lines: only the records list says the second line is inside the
		// test's own log record and not a note of the host's, which is always a record of its own.
		ev("TestMultiLineRecord", "fail", "setup\r\ngo2cs: testing.AllocsPerRun tallied 3 over 1 run(s)"),
		{"package": "p", "test": "", "action": "pass"},
	}, map[string]string{"TestCount": "pass", "TestBytes": "pass"})

	entries := writeEntries(t, "# a comment", "p TestCount", "p TestBytes", "p TestNoteless", "p TestSkipped",
		"p TestPassCRLF", "p TestParent/child/grandchild", "p TestCapped", "p TestCappedTrailer", "p TestRunOnly",
		"p TestMultiLineRecord", "p TestAbsent", "q TestNoResultsGiven", "")
	rows, err := runTool(t, "-entries", entries, dir)
	if err != nil {
		t.Fatal(err)
	}
	want := map[string][]string{ // cs, go, unit, per_run, count, bytes, runs, note_on
		"p TestCount":                   {"fail", "pass", "COUNT", "1", "100", "5600", "100", "self"},
		"p TestBytes":                   {"fail", "pass", "BYTES", "176", "-", "880", "5", "self"},
		"p TestNoteless":                {"pass", "-", "none", "-", "-", "-", "-", "-"},
		"p TestSkipped":                 {"skip", "-", "none", "-", "-", "-", "-", "-"},
		"p TestPassCRLF":                {"pass", "-", "COUNT", "1", "100", "5600", "100", "self"},
		"p TestParent/child/grandchild": {"pass", "-", "parent-only", "1", "100", "5600", "100", "TestParent"},
		"p TestCapped":                  {"fail", "-", "capped", "-", "-", "-", "-", "-"},
		"p TestCappedTrailer":           {"pass", "-", "capped", "-", "-", "-", "-", "-"},
		"p TestRunOnly":                 {"-", "-", "not-run", "-", "-", "-", "-", "-"},
		"p TestMultiLineRecord":         {"fail", "-", "none", "-", "-", "-", "-", "-"},
		"p TestAbsent":                  {"-", "-", "not-run", "-", "-", "-", "-", "-"},
		"q TestNoResultsGiven":          {"-", "-", "not-run", "-", "-", "-", "-", "-"},
	}
	if len(rows) != len(want) {
		t.Errorf("%d rows, want one per entry (%d)", len(rows), len(want))
	}
	for k, w := range want {
		r := rows[k]
		if r == nil {
			t.Errorf("%s: no row", k)
			continue
		}
		got := []string{r[cCS], r[cGo], r[cUnit], r[cPerRun], r[cCount], r[cBytes], r[cRuns], r[cNoteOn]}
		if strings.Join(got, "|") != strings.Join(w, "|") {
			t.Errorf("%s:\n got %v\nwant %v", k, got, w)
		}
	}
	if r := rows["p TestPassCRLF"]; r != nil && r[len(r)-1] != countNoteText {
		t.Errorf("TestPassCRLF: the note column is %q, want the note exactly (no line terminator)", r[len(r)-1])
	}

	// Without -entries, only the tests that carry their own note are reported.
	rows, err = runTool(t, dir)
	if err != nil {
		t.Fatal(err)
	}
	var names []string
	for k := range rows {
		names = append(names, k)
	}
	if len(rows) != 4 || rows["p TestCount"] == nil || rows["p TestBytes"] == nil || rows["p TestPassCRLF"] == nil || rows["p TestParent"] == nil {
		t.Errorf("without -entries: rows %v, want the four noted tests", names)
	}
}

func TestItRefusesRatherThanGuesses(t *testing.T) {
	countT, _ := hostNoteTemplates(t)
	note := fill(countT, "10", "2,800", "10")
	cases := []struct {
		name, want string
		events     []map[string]any
	}{
		{"two notes", "two unit notes", []map[string]any{ev("TestA", "fail", note, note)}},
		{"drifted wording", "matches neither form", []map[string]any{ev("TestA", "fail",
			"go2cs: testing.AllocsPerRun tallied 10 objects over 10 run(s)")}},
		{"two terminal events", "two terminal events", []map[string]any{ev("TestA", "pass"), ev("TestA", "fail")}},
		{"zero runs", "zero run count", []map[string]any{ev("TestA", "fail", fill(countT, "10", "2,800", "0"))}},
		{"no events", "no events", nil},
	}
	for _, c := range cases {
		dir := writeResults(t, "p", c.events, nil)
		_, err := runTool(t, dir)
		var r refusal
		if !errors.As(err, &r) || !strings.Contains(err.Error(), c.want) {
			t.Errorf("%s: err %v, want a refusal naming %q", c.name, err, c.want)
		}
	}

	dir := writeResults(t, "p", []map[string]any{ev("TestA", "pass")}, nil)
	for name, args := range map[string][]string{
		"the same package twice": {dir, filepath.Join(dir, resultsFileName)},
		"a malformed entry":      {"-entries", writeEntries(t, "p TestA extra"), dir},
		"a duplicated entry":     {"-entries", writeEntries(t, "p TestA", "p  TestA"), dir},
	} {
		_, err := runTool(t, args...)
		var r refusal
		if !errors.As(err, &r) {
			t.Errorf("%s: err %v, want a refusal", name, err)
		}
	}
}

// TestAHashSuffixIsPartOfATestName pins the -entries comment rule: '#' starts a comment only at the
// start of a line or after whitespace. Go uniquifies a repeated subtest name with a #NN suffix
// (TestSizeAllocs/[]binary.Struct#01 beside TestSizeAllocs/[]binary.Struct), and a '#' anywhere
// cut that name to its sibling's, so the pair read as one entry listed twice.
func TestAHashSuffixIsPartOfATestName(t *testing.T) {
	got, err := readEntries(writeEntries(t,
		"# a whole-line comment",
		"encoding/binary TestSizeAllocs/[]binary.Struct",
		"encoding/binary TestSizeAllocs/[]binary.Struct#01",
		"p TestA # a trailing comment after a space",
		"p TestB\t# a trailing comment after a tab",
		"   # an indented comment",
	))
	if err != nil {
		t.Fatalf("readEntries: %v", err)
	}
	want := []entry{
		{"encoding/binary", "TestSizeAllocs/[]binary.Struct"},
		{"encoding/binary", "TestSizeAllocs/[]binary.Struct#01"},
		{"p", "TestA"},
		{"p", "TestB"},
	}
	if len(got) != len(want) {
		t.Fatalf("got %d entries %q, want %d %q", len(got), got, len(want), want)
	}
	for i := range want {
		if got[i] != want[i] {
			t.Errorf("entry %d: got %q, want %q", i, got[i], want[i])
		}
	}
}

// TestRealResults reads two results files the converted host wrote on linux at master 226afb810a
// (go2cs -tests -test-action all -test-config Release, tiering off), one per unit. The readings
// equal the ones the packages' disclosures cite from the i7 reading run at bb54ff0920.
func TestRealResults(t *testing.T) {
	var args []string
	for _, d := range []string{"unicode_utf16", "log_slog_internal_buffer"} {
		dir := filepath.Join(t.TempDir(), d)
		if err := os.MkdirAll(dir, 0o755); err != nil {
			t.Fatal(err)
		}
		for from, to := range map[string]string{"results.json": resultsFileName, "comparison.json": comparisonFileName} {
			data, err := os.ReadFile(filepath.Join("testdata", "real", d, from))
			if err != nil {
				t.Fatal(err)
			}
			if err := os.WriteFile(filepath.Join(dir, to), data, 0o644); err != nil {
				t.Fatal(err)
			}
		}
		args = append(args, dir)
	}
	entries := writeEntries(t, "unicode/utf16 TestAllocationsDecode", "unicode/utf16 TestDecode",
		"log/slog/internal/buffer TestAlloc")
	rows, err := runTool(t, append([]string{"-entries", entries}, args...)...)
	if err != nil {
		t.Fatal(err)
	}
	want := map[string]string{
		"unicode/utf16 TestAllocationsDecode": "fail|pass|COUNT|1|10|2800|10|self",
		"unicode/utf16 TestDecode":            "pass|pass|none|-|-|-|-|-",
		"log/slog/internal/buffer TestAlloc":  "fail|pass|BYTES|176|-|880|5|self",
	}
	for k, w := range want {
		r := rows[k]
		if r == nil {
			t.Errorf("%s: no row", k)
			continue
		}
		if got := strings.Join(r[cCS:cNoteOn+1], "|"); got != w {
			t.Errorf("%s: got %s, want %s", k, got, w)
		}
	}
}
