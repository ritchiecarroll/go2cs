// main.go — the allocation-unit extractor (hop readiness A14; runbook §7 lessons 31, 63 and 70).
//
// The converted test host's testing.AllocsPerRun reports a COUNT per run when golib's allocation
// counter charged something and falls back to BYTES per run when it charged nothing. Go's own
// failure text ("got 176 allocs, want 0") cannot tell the two apart, so the host attaches a note
// naming the unit to the RUNNING test's log, at most once per test
// (src/core/testing/testing.cs AllocsPerRun; TestExecution.NoteMeasurementUnitOnce). The note rides
// the test's terminal event in go2cs_test_results.json. This tool reads it back, one row per test,
// so an H10 reading run classes every alloc label from the unit the host measured.
//
// Usage:
//
//	alloc-units [-entries <file>] <results.json | package-dir> [...]
//
// Each argument is a go2cs_test_results.json or the package directory that holds one. A sibling
// go2cs_test_comparison.json, when present, supplies the Go verdict. Without -entries, every test
// that carries a note is reported. With -entries (one "<package> <test>" pair per line, '#' at the
// start of a line or after whitespace starts a comment, so a test name's #NN suffix is kept), every
// named entry gets exactly one row, whether or not it carries a note.
//
// Output is TSV on stdout with a header; a one-line summary goes to stderr. Columns:
//
//	package  test  cs  go  unit  per_run  count  bytes  runs  note_on  note
//
// unit is COUNT or BYTES from the entry's own note; none when the test reached a terminal verdict
// with no note (every AllocsPerRun it made returned exact zero, or it made none); parent-only when
// the entry has no note but a parent test does (note_on names the parent, the figures are its);
// capped when the host dropped log records for the test and no note survived, so the unit is
// unknown; not-run when there is no terminal verdict, or the package's results were not given.
// per_run is the figure AllocsPerRun returned: max(1, figure/runs) in integer division, where the
// figure is the count for COUNT and the bytes for BYTES.
//
// It REFUSES (exit 1, naming the file and the test) rather than guess: a results file with no
// events or an unknown schema, a test with two notes or two terminal events, and any log line that
// starts like a note but matches neither form. The last is the host-drift guard: if testing.cs
// changes the note's wording, this tool stops instead of reading every entry as "none". The
// literals it parses are checked against testing.cs itself by main_test.go.
//
// The once-per-test limit is the host's, not this tool's: the note records the FIRST nonzero
// AllocsPerRun of a test, so a later call in the same test has no recorded unit anywhere.
package main

import (
	"bufio"
	"encoding/json"
	"errors"
	"flag"
	"fmt"
	"io"
	"os"
	"path/filepath"
	"regexp"
	"sort"
	"strconv"
	"strings"
)

const (
	resultsFileName    = "go2cs_test_results.json"
	comparisonFileName = "go2cs_test_comparison.json"
	notePrefix         = "go2cs: testing.AllocsPerRun "
)

// The host formats every figure with N0, so a figure carries the culture's group separator:
// "2,800" under the invariant and en-US cultures, "2.800" or "2 800" under others.
const figure = `([0-9][0-9.,'\x{00a0}\x{202f} ]*)`

var (
	countNote = regexp.MustCompile(`^go2cs: testing\.AllocsPerRun counted ` + figure +
		` go2cs-runtime object allocations \(` + figure + ` bytes\) over ` + figure + ` run\(s\) `)
	bytesNote = regexp.MustCompile(`^go2cs: testing\.AllocsPerRun measured ` + figure +
		` allocated BYTES over ` + figure + ` run\(s\) `)
	droppedTrailer = regexp.MustCompile(`^testing: [0-9]+ further log record\(s\) dropped after `)
)

type resultEvent struct {
	Package string   `json:"package"`
	Test    string   `json:"test"`
	Action  string   `json:"action"`
	Output  *string  `json:"output"`
	Records []string `json:"records"`
	Dropped *int     `json:"dropped"`
}

type resultsFile struct {
	SchemaVersion int           `json:"schemaVersion"`
	Package       string        `json:"package"`
	Events        []resultEvent `json:"events"`
}

type comparisonFile struct {
	Go map[string]string `json:"go"`
}

// note is one parsed unit note.
type note struct {
	unit   string // COUNT or BYTES
	count  int64  // -1 for BYTES
	bytes  int64
	runs   int64
	text   string
	perRun int64
}

// testReading is what one test's terminal event says.
type testReading struct {
	verdict string
	note    *note
	capped  bool
}

type packageReading struct {
	pkg   string
	file  string
	tests map[string]*testReading
	goV   map[string]string
}

type refusal struct{ msg string }

func (r refusal) Error() string { return r.msg }

func refuse(format string, args ...any) error { return refusal{fmt.Sprintf(format, args...)} }

func parseFigure(s string) (int64, error) {
	var digits strings.Builder
	for _, r := range s {
		if r >= '0' && r <= '9' {
			digits.WriteRune(r)
		}
	}
	return strconv.ParseInt(digits.String(), 10, 64)
}

// parseNote returns (nil, nil) for a line that is not a note, and a refusal for a line that starts
// like one but matches neither form.
func parseNote(line string) (*note, error) {
	if !strings.HasPrefix(line, notePrefix) {
		return nil, nil
	}
	n := &note{text: line, count: -1}
	var figures []string
	if m := countNote.FindStringSubmatch(line); m != nil {
		n.unit, figures = "COUNT", m[1:]
	} else if m := bytesNote.FindStringSubmatch(line); m != nil {
		n.unit, figures = "BYTES", []string{"", m[1], m[2]}
	} else {
		return nil, refuse("a log line starts like an AllocsPerRun unit note but matches neither form the host writes "+
			"(has testing.cs changed the wording?): %q", line)
	}
	var err error
	if figures[0] != "" {
		if n.count, err = parseFigure(figures[0]); err != nil {
			return nil, refuse("unreadable count in note %q", line)
		}
	}
	if n.bytes, err = parseFigure(figures[1]); err != nil {
		return nil, refuse("unreadable byte total in note %q", line)
	}
	if n.runs, err = parseFigure(figures[2]); err != nil || n.runs <= 0 {
		return nil, refuse("unreadable or zero run count in note %q", line)
	}
	measured := n.bytes
	if n.unit == "COUNT" {
		measured = n.count
	}
	n.perRun = max(1, measured/n.runs)
	return n, nil
}

func logLines(e resultEvent) []string {
	if e.Records != nil {
		return e.Records
	}
	if e.Output == nil {
		return nil
	}
	lines := strings.Split(*e.Output, "\n")
	for i, l := range lines {
		lines[i] = strings.TrimSuffix(l, "\r")
	}
	return lines
}

func isTerminal(action string) bool {
	return action == "pass" || action == "fail" || action == "skip"
}

func resolveResults(arg string) (string, error) {
	info, err := os.Stat(arg)
	if err != nil {
		return "", refuse("%s: %v", arg, err)
	}
	if info.IsDir() {
		return filepath.Join(arg, resultsFileName), nil
	}
	return arg, nil
}

func readPackage(path string) (*packageReading, error) {
	data, err := os.ReadFile(path)
	if err != nil {
		return nil, refuse("%s: %v", path, err)
	}
	var rf resultsFile
	if err := json.Unmarshal(data, &rf); err != nil {
		return nil, refuse("%s: not a results file: %v", path, err)
	}
	if rf.SchemaVersion != 1 {
		return nil, refuse("%s: schemaVersion %d, this tool reads 1", path, rf.SchemaVersion)
	}
	if len(rf.Events) == 0 {
		return nil, refuse("%s: no events (read the results tail before anything else: a host that died at init writes none)", path)
	}
	pr := &packageReading{pkg: rf.Package, file: path, tests: map[string]*testReading{}}
	for _, e := range rf.Events {
		if e.Test == "" || !isTerminal(e.Action) {
			continue
		}
		if _, seen := pr.tests[e.Test]; seen {
			return nil, refuse("%s: %s has two terminal events", path, e.Test)
		}
		tr := &testReading{verdict: e.Action, capped: e.Dropped != nil && *e.Dropped > 0}
		for _, line := range logLines(e) {
			if droppedTrailer.MatchString(line) {
				tr.capped = true
			}
			n, err := parseNote(line)
			if err != nil {
				return nil, refuse("%s: %s: %v", path, e.Test, err)
			}
			if n == nil {
				continue
			}
			if tr.note != nil {
				return nil, refuse("%s: %s carries two unit notes; the host writes at most one per test", path, e.Test)
			}
			tr.note = n
		}
		pr.tests[e.Test] = tr
	}
	if data, err := os.ReadFile(filepath.Join(filepath.Dir(path), comparisonFileName)); err == nil {
		var cf comparisonFile
		if err := json.Unmarshal(data, &cf); err != nil {
			return nil, refuse("%s: unreadable comparison record beside the results: %v", path, err)
		}
		pr.goV = cf.Go
	}
	return pr, nil
}

type entry struct{ pkg, test string }

func readEntries(path string) ([]entry, error) {
	f, err := os.Open(path)
	if err != nil {
		return nil, refuse("%s: %v", path, err)
	}
	defer f.Close()
	var entries []entry
	seen := map[entry]bool{}
	sc := bufio.NewScanner(f)
	for n := 1; sc.Scan(); n++ {
		line := sc.Text()
		// '#' starts a comment only at the start of the line or after whitespace: Go's #NN suffix on a
		// repeated subtest name (TestSizeAllocs/[]binary.Struct#01) is part of the name.
		for i := 0; i < len(line); i++ {
			if line[i] == '#' && (i == 0 || line[i-1] == ' ' || line[i-1] == '\t') {
				line = line[:i]
				break
			}
		}
		fields := strings.Fields(line)
		if len(fields) == 0 {
			continue
		}
		if len(fields) != 2 {
			return nil, refuse("%s:%d: want \"<package> <test>\", got %q", path, n, sc.Text())
		}
		e := entry{fields[0], fields[1]}
		if seen[e] {
			return nil, refuse("%s:%d: %s %s is listed twice", path, n, e.pkg, e.test)
		}
		seen[e] = true
		entries = append(entries, e)
	}
	return entries, sc.Err()
}

type row struct {
	pkg, test, cs, goV, unit, noteOn string
	n                                *note
}

func (r row) tsv() string {
	perRun, count, bytes, runs, text := "-", "-", "-", "-", ""
	if r.n != nil {
		perRun, bytes, runs, text = fmt.Sprint(r.n.perRun), fmt.Sprint(r.n.bytes), fmt.Sprint(r.n.runs), r.n.text
		if r.n.count >= 0 {
			count = fmt.Sprint(r.n.count)
		}
	}
	return strings.Join([]string{r.pkg, r.test, r.cs, r.goV, r.unit, perRun, count, bytes, runs, r.noteOn, text}, "\t")
}

func dash(s string) string {
	if s == "" {
		return "-"
	}
	return s
}

func readEntry(pr *packageReading, e entry) row {
	r := row{pkg: e.pkg, test: e.test, cs: "-", goV: "-", unit: "not-run", noteOn: "-"}
	if pr == nil {
		return r
	}
	r.goV = dash(pr.goV[e.test])
	tr := pr.tests[e.test]
	if tr == nil {
		return r
	}
	r.cs = tr.verdict
	switch {
	case tr.note != nil:
		r.unit, r.noteOn, r.n = tr.note.unit, "self", tr.note
	case tr.capped:
		r.unit = "capped"
	default:
		r.unit = "none"
		for name := e.test; strings.Contains(name, "/"); {
			name = name[:strings.LastIndex(name, "/")]
			if p := pr.tests[name]; p != nil && p.note != nil {
				r.unit, r.noteOn, r.n = "parent-only", name, p.note
				break
			}
		}
	}
	return r
}

func run(args []string, stdout, stderr io.Writer) error {
	fs := flag.NewFlagSet("alloc-units", flag.ContinueOnError)
	fs.SetOutput(stderr)
	entriesPath := fs.String("entries", "", "file of \"<package> <test>\" lines; every entry gets one row")
	if err := fs.Parse(args); err != nil {
		return err
	}
	if fs.NArg() == 0 {
		fs.Usage()
		return errors.New("usage: alloc-units [-entries <file>] <results.json | package-dir> [...]")
	}
	byPkg := map[string]*packageReading{}
	var pkgs []string
	for _, arg := range fs.Args() {
		path, err := resolveResults(arg)
		if err != nil {
			return err
		}
		pr, err := readPackage(path)
		if err != nil {
			return err
		}
		if prev := byPkg[pr.pkg]; prev != nil {
			return refuse("%s and %s are both results for %s", prev.file, path, pr.pkg)
		}
		byPkg[pr.pkg] = pr
		pkgs = append(pkgs, pr.pkg)
	}
	var rows []row
	if *entriesPath != "" {
		entries, err := readEntries(*entriesPath)
		if err != nil {
			return err
		}
		for _, e := range entries {
			rows = append(rows, readEntry(byPkg[e.pkg], e))
		}
	} else {
		sort.Strings(pkgs)
		for _, p := range pkgs {
			pr := byPkg[p]
			var names []string
			for name, tr := range pr.tests {
				if tr.note != nil {
					names = append(names, name)
				}
			}
			sort.Strings(names)
			for _, name := range names {
				rows = append(rows, readEntry(pr, entry{p, name}))
			}
		}
	}
	fmt.Fprintln(stdout, "package\ttest\tcs\tgo\tunit\tper_run\tcount\tbytes\truns\tnote_on\tnote")
	tally := map[string]int{}
	for _, r := range rows {
		fmt.Fprintln(stdout, r.tsv())
		tally[r.unit]++
	}
	var parts []string
	for _, u := range []string{"COUNT", "BYTES", "none", "parent-only", "capped", "not-run"} {
		parts = append(parts, fmt.Sprintf("%s %d", u, tally[u]))
	}
	fmt.Fprintf(stderr, "alloc-units: %d row(s) from %d results file(s): %s\n", len(rows), len(pkgs), strings.Join(parts, ", "))
	return nil
}

func main() {
	if err := run(os.Args[1:], os.Stdout, os.Stderr); err != nil {
		var r refusal
		if errors.As(err, &r) {
			fmt.Fprintln(os.Stderr, "alloc-units: REFUSED:", err)
			os.Exit(1)
		}
		fmt.Fprintln(os.Stderr, "alloc-units:", err)
		os.Exit(2)
	}
}
