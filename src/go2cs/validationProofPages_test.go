// validationProofPages_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"encoding/json"
	"os"
	"path/filepath"
	"regexp"
	"strings"
	"testing"
)

const proofFixtureDir = "testdata/validationproof"

func loadProofFixture(t *testing.T) (testComparison, map[string]testDisclosure) {
	t.Helper()

	data, err := os.ReadFile(filepath.Join(proofFixtureDir, "go2cs_test_comparison.json"))

	if err != nil {
		t.Fatalf("read fixture comparison: %v", err)
	}

	var comparison testComparison

	if err := json.Unmarshal(data, &comparison); err != nil {
		t.Fatalf("parse fixture comparison: %v", err)
	}

	// Read through the production loader so the fixture manifest exercises the same validation the
	// pipeline applies (every field required, no duplicates).
	disclosures, _, err := readTestDisclosureManifest(proofFixtureDir)

	if err != nil {
		t.Fatalf("load fixture disclosures: %v", err)
	}

	return comparison, disclosures
}

func fixtureProvenance() proofPageProvenance {
	return proofPageProvenance{
		importPath: "fixture/pkg",
		goVersion:  "1.23.1",
		platform:   "windows/amd64",
		date:       "2026-08-02",
		commit:     "5b4549cf8",
	}
}

// TestValidationProofPageGolden renders the fixture differential and compares it to the committed
// expected page — the whole point of the feature is that the emitted proof is exact and reviewable.
func TestValidationProofPageGolden(t *testing.T) {
	comparison, disclosures := loadProofFixture(t)

	expectedBytes, err := os.ReadFile(filepath.Join(proofFixtureDir, "expected.md"))

	if err != nil {
		t.Fatalf("read expected page: %v", err)
	}

	expected := strings.ReplaceAll(string(expectedBytes), "\r\n", "\n")
	actual := renderValidationProofPage(fixtureProvenance(), comparison, disclosures, nil)

	if actual != expected {
		expectedLines := strings.Split(expected, "\n")
		actualLines := strings.Split(actual, "\n")

		for i := 0; i < len(expectedLines) || i < len(actualLines); i++ {
			var expectedLine, actualLine string

			if i < len(expectedLines) {
				expectedLine = expectedLines[i]
			}

			if i < len(actualLines) {
				actualLine = actualLines[i]
			}

			if expectedLine != actualLine {
				t.Fatalf("proof page differs at line %d:\n  expected: %q\n  actual:   %q", i+1, expectedLine, actualLine)
			}
		}

		t.Fatal("proof page differs from the expected page")
	}
}

// TestValidationProofPageDeterministic renders the same differential repeatedly. Go randomizes map
// iteration per range, so a renderer that let map order reach the page would diverge within a few
// hundred renders; identical output is the guarantee the converter's byte-determinism depends on.
func TestValidationProofPageDeterministic(t *testing.T) {
	comparison, disclosures := loadProofFixture(t)

	first := renderValidationProofPage(fixtureProvenance(), comparison, disclosures, nil)

	for i := 0; i < 500; i++ {
		if again := renderValidationProofPage(fixtureProvenance(), comparison, disclosures, nil); again != first {
			t.Fatalf("render %d is not byte-identical to the first render", i+1)
		}
	}
}

// TestValidationIndexDeterministic proves the roster sorts its input rather than trusting the
// directory-scan order it is handed.
func TestValidationIndexDeterministic(t *testing.T) {
	shuffled := renderValidationIndex([]string{"path.filepath", "io", "math.rand.v2", "bufio"})
	ordered := renderValidationIndex([]string{"bufio", "io", "math.rand.v2", "path.filepath"})

	if shuffled != ordered {
		t.Fatal("validation index depends on the order its entries are supplied in")
	}

	if !strings.Contains(ordered, "| `path/filepath` | [`path.filepath.md`](current/path.filepath.md) |") {
		t.Fatalf("validation index row is not in the expected form:\n%s", ordered)
	}

	bufioAt := strings.Index(ordered, "`bufio`")
	ioAt := strings.Index(ordered, "`io`")

	if bufioAt < 0 || ioAt < 0 || bufioAt > ioAt {
		t.Fatal("validation index rows are not sorted")
	}
}

// TestValidationIndexKeepsHandTextAndNamesOnlyRosterRows pins the two things a sweep's index rewrite
// got wrong. The page's hand-maintained text (the Frozen snapshots section above the CURRENT table,
// and anything after it) is history, so it is kept byte for byte. And a page under current/ is not a
// row just because it exists: a retired import path's page stays as its successor's relocation anchor,
// and an excluded package's page stays as its exclusion's evidence, but the index lists only the
// packages the roster banks.
func TestValidationIndexKeepsHandTextAndNamesOnlyRosterRows(t *testing.T) {
	docsPath := t.TempDir()
	validationPath := filepath.Join(docsPath, "validation")
	currentPath := filepath.Join(validationPath, "current")

	if err := os.MkdirAll(currentPath, 0755); err != nil {
		t.Fatal(err)
	}

	// Pages: two banked, one retired (a relocation anchor), one excluded.
	for _, dotID := range []string{"io", "bufio", "internal.weak", "internal.copyright"} {
		if err := os.WriteFile(filepath.Join(currentPath, dotID+".md"), []byte("# page\n"), 0644); err != nil {
			t.Fatal(err)
		}
	}

	// The roster banks io and bufio, and archive/tar, which has no page yet. The exclusion table's
	// row shape is not a banked row and must not be read as one.
	roster := "# Validated Test Packages\n\n" +
		"| Package | Tests matched | Disclosed | What it exercises |\n" +
		"|:--|--:|--:|:--|\n" +
		"| [`io`](https://example.invalid/io) | 12 | 0 | Readers. |\n" +
		"| [`bufio`](https://example.invalid/bufio) | 30 | 1 | Buffers. |\n" +
		"| [`archive/tar`](https://example.invalid/tar) | 9 | 0 | Archives. |\n\n" +
		"## Excluded packages\n\n" +
		"| Package | Class | Why |\n" +
		"|:--|:--|:--|\n" +
		"| `internal/copyright` | E4 | No tests of its own. |\n"

	if err := os.WriteFile(filepath.Join(docsPath, "ValidatedTestPackages.md"), []byte(roster), 0644); err != nil {
		t.Fatal(err)
	}

	head := "# Validation proofs\n\nHand-written prose.\n\n## Frozen snapshots\n\n" +
		"| Release | Snapshot | Proof pages | Roster as it stood |\n" +
		"|:--|:--|--:|:--|\n" +
		"| 1.24.13.9 | [`1.24.13.9/`](1.24.13.9/) | 3 | at the tag |\n\n" +
		"| Package | Proof | Converted package |\n" +
		"|:--|:--|:--|\n"
	tail := "\nA closing note that is also hand-written.\n"
	staleRow := "| `internal/weak` | [`internal.weak.md`](current/internal.weak.md) | [`src/core/internal/weak`](" + go2csRepositoryURL + "/tree/master/src/core/internal/weak) |\n"
	indexPath := filepath.Join(validationPath, "index.md")

	if err := os.WriteFile(indexPath, []byte(head+staleRow+tail), 0644); err != nil {
		t.Fatal(err)
	}

	if err := writeValidationIndex(docsPath); err != nil {
		t.Fatalf("write index: %v", err)
	}

	written, err := os.ReadFile(indexPath)

	if err != nil {
		t.Fatalf("read index: %v", err)
	}

	index := strings.ReplaceAll(string(written), "\r", "")
	// The rows follow the roster's order (io before bufio), as the committed page does.
	want := head +
		"| `io` | [`io.md`](current/io.md) | [`src/core/io`](" + go2csRepositoryURL + "/tree/master/src/core/io) |\n" +
		"| `bufio` | [`bufio.md`](current/bufio.md) | [`src/core/bufio`](" + go2csRepositoryURL + "/tree/master/src/core/bufio) |\n" +
		tail

	if index != want {
		t.Fatalf("index is not the hand text around the roster's rows:\n--- got ---\n%s\n--- want ---\n%s", index, want)
	}

	// Stable: a second pass over the same tree writes nothing.
	if err := writeValidationIndex(docsPath); err != nil {
		t.Fatalf("rewrite index: %v", err)
	}

	again, err := os.ReadFile(indexPath)

	if err != nil {
		t.Fatalf("reread index: %v", err)
	}

	if string(again) != string(written) {
		t.Fatal("regenerating an unchanged index rewrote it")
	}

	// A roster none of whose packages has a page yet leaves an empty table, never a full re-render
	// that would drop the hand text.
	if err := os.WriteFile(filepath.Join(docsPath, "ValidatedTestPackages.md"), []byte("| [`archive/tar`](https://example.invalid/tar) | 9 | 0 | Archives. |\n"), 0644); err != nil {
		t.Fatal(err)
	}

	if err := writeValidationIndex(docsPath); err != nil {
		t.Fatalf("write index with no banked page: %v", err)
	}

	emptied, err := os.ReadFile(indexPath)

	if err != nil {
		t.Fatalf("read emptied index: %v", err)
	}

	if index := strings.ReplaceAll(string(emptied), "\r", ""); index != head+tail {
		t.Fatalf("an index with no banked page lost its hand text:\n%s", index)
	}

	if err := os.WriteFile(indexPath, written, 0644); err != nil {
		t.Fatal(err)
	}

	// A roster that exists but reads as no rows is a broken instrument, not an empty roster: the
	// writer refuses rather than rewrite the index around nothing.
	if err := os.WriteFile(filepath.Join(docsPath, "ValidatedTestPackages.md"), []byte("# Validated Test Packages\n"), 0644); err != nil {
		t.Fatal(err)
	}

	if err := writeValidationIndex(docsPath); err == nil {
		t.Fatal("a roster with no rows did not refuse")
	}

	unchanged, err := os.ReadFile(indexPath)

	if err != nil {
		t.Fatalf("read index after refusal: %v", err)
	}

	if string(unchanged) != string(written) {
		t.Fatal("a refused regeneration still rewrote the index")
	}
}

// TestValidationIndexWithoutCommittedPage covers a tree with a roster but no index yet: the writer
// renders the full page, rows from the roster.
func TestValidationIndexWithoutCommittedPage(t *testing.T) {
	docsPath := t.TempDir()
	currentPath := filepath.Join(docsPath, "validation", "current")

	if err := os.MkdirAll(currentPath, 0755); err != nil {
		t.Fatal(err)
	}

	for _, dotID := range []string{"io", "internal.weak"} {
		if err := os.WriteFile(filepath.Join(currentPath, dotID+".md"), []byte("# page\n"), 0644); err != nil {
			t.Fatal(err)
		}
	}

	roster := "| [`io`](https://example.invalid/io) | 12 | 0 | Readers. |\n"

	if err := os.WriteFile(filepath.Join(docsPath, "ValidatedTestPackages.md"), []byte(roster), 0644); err != nil {
		t.Fatal(err)
	}

	if err := writeValidationIndex(docsPath); err != nil {
		t.Fatalf("write index: %v", err)
	}

	written, err := os.ReadFile(filepath.Join(docsPath, "validation", "index.md"))

	if err != nil {
		t.Fatalf("read index: %v", err)
	}

	if index := strings.ReplaceAll(string(written), "\r", ""); index != renderValidationIndex([]string{"io"}) {
		t.Fatalf("index without a committed page is not the full render of the roster's rows:\n%s", index)
	}
}

// TestValidationProofPageContentStability is the load-bearing rule: a re-validation that reproduces
// the same verdicts must leave the page byte-for-byte alone even though its date and converter
// commit have moved on, and must rewrite it the moment a verdict actually changes.
func TestValidationProofPageContentStability(t *testing.T) {
	comparison, disclosures := loadProofFixture(t)
	docsPath := t.TempDir()
	pagePath := filepath.Join(docsPath, "validation", "current", "fixture.pkg.md")

	if err := writeValidationProofPage(docsPath, fixtureProvenance(), comparison, disclosures, nil); err != nil {
		t.Fatalf("first write: %v", err)
	}

	original, err := os.ReadFile(pagePath)

	if err != nil {
		t.Fatalf("read first page: %v", err)
	}

	if !strings.Contains(string(original), "*Validated 2026-08-02 · converter `5b4549cf8`*") {
		t.Fatal("first page is missing its provenance line")
	}

	if !strings.Contains(string(original), "\r\n") {
		t.Fatal("proof page was not written with CRLF line endings")
	}

	// Same verdicts, later day, different converter build: provenance ALONE would move, so nothing
	// may be written.
	moved := fixtureProvenance()
	moved.date, moved.commit = "2026-09-15", "deadbeef1"

	if err := writeValidationProofPage(docsPath, moved, comparison, disclosures, nil); err != nil {
		t.Fatalf("second write: %v", err)
	}

	unchanged, err := os.ReadFile(pagePath)

	if err != nil {
		t.Fatalf("read second page: %v", err)
	}

	if string(unchanged) != string(original) {
		t.Fatalf("a provenance-only change rewrote the page:\n%s", string(unchanged))
	}

	// A verdict moves — now the page (and its provenance) must follow.
	comparison.Go["TestDelta"] = "pass"
	comparison.CSharp["TestDelta"] = "pass"

	if err := writeValidationProofPage(docsPath, moved, comparison, disclosures, nil); err != nil {
		t.Fatalf("third write: %v", err)
	}

	rewritten, err := os.ReadFile(pagePath)

	if err != nil {
		t.Fatalf("read third page: %v", err)
	}

	if !strings.Contains(string(rewritten), "| `TestDelta` | pass | pass |") {
		t.Fatal("a changed verdict did not reach the page")
	}

	if !strings.Contains(string(rewritten), "*Validated 2026-09-15 · converter `deadbeef1`*") {
		t.Fatal("provenance did not move when the verdicts moved")
	}

	// Index regeneration rides along, and is itself stable.
	indexPath := filepath.Join(docsPath, "validation", "index.md")
	index, err := os.ReadFile(indexPath)

	if err != nil {
		t.Fatalf("read index: %v", err)
	}

	if !strings.Contains(string(index), "(current/fixture.pkg.md)") {
		t.Fatalf("index does not list the page it was regenerated for:\n%s", string(index))
	}

	if err := writeValidationIndex(docsPath); err != nil {
		t.Fatalf("regenerate index: %v", err)
	}

	regenerated, err := os.ReadFile(indexPath)

	if err != nil {
		t.Fatalf("read regenerated index: %v", err)
	}

	if string(regenerated) != string(index) {
		t.Fatal("regenerating an unchanged index rewrote it")
	}
}

// TestValidationProofPageSkipsWithoutTreeRoot covers the bare-temp-conversion case: no go2cs root
// above the output path means no docs tree to publish into, and the hook must skip silently rather
// than fail a validation that genuinely succeeded.
func TestValidationProofPageSkipsWithoutTreeRoot(t *testing.T) {
	comparison, disclosures := loadProofFixture(t)
	outputPath := t.TempDir()
	manifest := testManifest{PackageImportPath: "fixture/pkg", GoVersion: "go1.23.1"}

	if err := emitValidationProofPage(outputPath, comparison, manifest, disclosures, nil, Options{targetPlatform: "windows/amd64"}); err != nil {
		t.Fatalf("emit without a tree root: %v", err)
	}

	entries, err := os.ReadDir(outputPath)

	if err != nil {
		t.Fatalf("read output path: %v", err)
	}

	if len(entries) != 0 {
		t.Fatalf("emit without a tree root created %d entries", len(entries))
	}
}

// TestValidationProofPageLocatesDocsTree walks the same ancestor chain the pipeline uses to
// self-locate $(go2csPath) — the root holding core/golib — and publishes into that root's docs
// sibling.
func TestValidationProofPageLocatesDocsTree(t *testing.T) {
	comparison, disclosures := loadProofFixture(t)
	repoPath := t.TempDir()
	outputPath := filepath.Join(repoPath, "src", "core", "fixture", "pkg")

	if err := os.MkdirAll(filepath.Join(repoPath, "src", "core", "golib"), 0755); err != nil {
		t.Fatalf("seed golib: %v", err)
	}

	if err := os.WriteFile(filepath.Join(repoPath, "src", "core", "golib", "golib.csproj"), []byte("<Project />"), 0644); err != nil {
		t.Fatalf("seed golib project: %v", err)
	}

	if err := os.MkdirAll(filepath.Join(repoPath, "docs"), 0755); err != nil {
		t.Fatalf("seed docs: %v", err)
	}

	if err := os.MkdirAll(outputPath, 0755); err != nil {
		t.Fatalf("seed output path: %v", err)
	}

	manifest := testManifest{PackageImportPath: "fixture/pkg", GoVersion: "go1.23.1"}

	if err := emitValidationProofPage(outputPath, comparison, manifest, disclosures, nil, Options{targetPlatform: "windows/amd64"}); err != nil {
		t.Fatalf("emit: %v", err)
	}

	page, err := os.ReadFile(filepath.Join(repoPath, "docs", "validation", "current", "fixture.pkg.md"))

	if err != nil {
		t.Fatalf("read published page: %v", err)
	}

	if !strings.Contains(string(page), "**2 matched · 3 disclosed** — Go 1.23.1, `windows/amd64`") {
		t.Fatalf("published page does not carry the expected totals:\n%s", string(page))
	}
}

func TestValidationProofDotID(t *testing.T) {
	for importPath, expected := range map[string]string{
		"io":             "io",
		"path/filepath":  "path.filepath",
		"math/rand/v2":   "math.rand.v2",
		"net/http/httpu": "net.http.httpu",
	} {
		if actual := validationProofDotID(importPath); actual != expected {
			t.Errorf("dot-id for %q is %q, expected %q", importPath, actual, expected)
		}

		if actual := validationProofImportPath(expected); actual != importPath {
			t.Errorf("import path for %q is %q, expected %q", expected, actual, importPath)
		}
	}
}

// TestValidationProofCellEscaping keeps a test name or a pinned reason containing a pipe from
// silently ending a table column.
func TestValidationProofCellEscaping(t *testing.T) {
	if escaped := escapeProofCell("TestParse/a|b"); escaped != `TestParse/a\|b` {
		t.Errorf("pipe was not escaped: %q", escaped)
	}

	if escaped := escapeProofCell("first\nsecond"); escaped != "first second" {
		t.Errorf("newline was not folded: %q", escaped)
	}
}

// TestFilteredRunPublishesNoRosterArtifacts pins the rule that a gated census leaves no roster
// trace. The status a filtered run earns is indistinguishable from a full sweep's — `validated`
// means "everything COMPARED matched" — so the filter, not the status, has to be what decides.
// Measured motivation: `-test-filter '^TestCallPanic$'` against reflect produced a `1/1 validated`
// README badge, a docs/validation index row and a full proof page for a package with 124 failing
// tests.
func TestFilteredRunPublishesNoRosterArtifacts(t *testing.T) {
	if !publishesRosterArtifacts("validated", "") {
		t.Error("an UNFILTERED validated run must publish: that is how a row banks")
	}

	// The positive control above and these negatives share one predicate, so the rule cannot be
	// half-applied the way an inline condition at one call site can.
	for _, filter := range []string{"^TestCallPanic$", "^(TestA|TestB)$", "  ^TestA$  "} {
		if publishesRosterArtifacts("validated", filter) {
			t.Errorf("a FILTERED validated run must publish nothing; filter %q was allowed to", filter)
		}
	}

	// Whitespace alone is not a filter — trimming it must not turn a real filter into "unfiltered",
	// nor an empty one into a suppression.
	if !publishesRosterArtifacts("validated", "   ") {
		t.Error("whitespace-only filter is no filter: it must still publish")
	}

	if publishesRosterArtifacts("failed", "") {
		t.Error("a non-validated status must never publish, filtered or not")
	}
}

// TestValidationProofPageStatesTheTerminalContext: the "Measured at" sentence carries the driver's
// terminal context when the record observed one, in the same sentence as the configuration and
// the oracle, and nothing at all when the record carries no observation — so a page from a
// Windows run, or one regenerated from a record written before the field existed, is unchanged.
// The fixture record predates the field, which is exactly the third case. A record whose -stack-append
// mode resolved off says so in the same sentence; one that carries no "off" is unchanged.
func TestValidationProofPageStatesTheTerminalContext(t *testing.T) {
	comparison, disclosures := loadProofFixture(t)

	cases := []struct {
		name        string
		config      string
		tiered      bool
		oracle      string
		terminal    string
		stackAppend string
		want        string
	}{
		{name: "not probed (the fixture as committed)", want: "Measured at `Debug`."},
		{name: "controlling terminal present", terminal: driverTerminalPresent, want: "Measured at `Debug`, under a controlling terminal."},
		{name: "controlling terminal absent", terminal: driverTerminalAbsent, want: "Measured at `Debug`, with no controlling terminal."},
		{name: "release, oracle and terminal in one sentence", config: "Release", oracle: "go version go1.23.12 linux/amd64", terminal: driverTerminalPresent,
			want: "Measured at `Release` (tiered JIT off), oracle `go version go1.23.12 linux/amd64`, under a controlling terminal."},
		{name: "stack-append buffer off", stackAppend: "off", want: "Measured at `Debug`, with the stack-append buffer off."},
		{name: "release, terminal and stack-append off in one sentence", config: "Release", terminal: driverTerminalAbsent, stackAppend: "off",
			want: "Measured at `Release` (tiered JIT off), with no controlling terminal, with the stack-append buffer off."},
	}

	for _, c := range cases {
		t.Run(c.name, func(t *testing.T) {
			variant := comparison
			variant.Environment.Configuration = c.config
			variant.Environment.Tiered = c.tiered
			variant.Environment.OracleGoVersion = c.oracle
			variant.Environment.Terminal = c.terminal
			variant.Environment.StackAppend = c.stackAppend

			page := renderValidationProofPage(fixtureProvenance(), variant, disclosures, nil)
			lines := strings.Split(page, "\n")
			found := ""
			for _, line := range lines {
				if strings.HasPrefix(line, "Measured at ") {
					found = line
					break
				}
			}

			if found != c.want {
				t.Fatalf("measurement sentence = %q, want %q", found, c.want)
			}
		})
	}
}

// TestDisclosedHeadingIsClassAware pins the other half of the golden: a page whose disclosures include
// no `deferred` entry keeps the "provably cannot" heading byte for byte and gains no class sentence,
// so only pages that disclose a deferred entry move on regeneration. The golden fixture now carries a
// deferred entry, so this control re-renders the same fixture with that entry reclassed.
//
// RED PROOF: rendering the class sentence unconditionally reds this control; the pre-change renderer
// reds the golden at line 30 (its heading claims "provably cannot" over a deferred entry).
func TestDisclosedHeadingIsClassAware(t *testing.T) {
	comparison, disclosures := loadProofFixture(t)

	deferred := 0
	for name, disclosure := range disclosures {
		if disclosure.Class == deferredClass {
			deferred++
			disclosure.Class = "alloc-count-semantics"
			disclosure.Want, disclosure.Reading, disclosure.Plan = "", "", ""
			disclosures[name] = disclosure
		}
	}

	if deferred == 0 {
		t.Fatal("the fixture carries no deferred entry, so the golden no longer exercises the class-aware heading")
	}

	page := renderValidationProofPage(fixtureProvenance(), comparison, disclosures, nil)

	if !strings.Contains(page, "A disclosed divergence is a specific Go assertion the managed CLR *provably cannot* satisfy — not\n") {
		t.Error("a page with no deferred entry lost its \"provably cannot\" heading")
	}

	if strings.Contains(page, "The **Class** column says") || strings.Contains(page, "this conversion does not satisfy") {
		t.Error("a page with no deferred entry gained the class-aware wording")
	}
}

// TestRosterRowPatternsAgree holds the roster's three readers together. rosterRowPattern must be the
// literal $RosterRowPattern of src/_roster.ps1, which the sweep and the release census read with; and
// ROSTER_ROW in docs/phase4/hopA-inputs/regen-validation-index.py, which is spelled more loosely, must
// read the same packages out of the committed roster. Nonzero, so a reader that matches nothing
// cannot agree with another that matches nothing.
func TestRosterRowPatternsAgree(t *testing.T) {
	root := repoRootFromPackageDir(t)

	readPattern := func(relativePath string, extract *regexp.Regexp) string {
		t.Helper()

		data, err := os.ReadFile(filepath.Join(root, filepath.FromSlash(relativePath)))

		if err != nil {
			t.Fatalf("read %s: %v", relativePath, err)
		}

		match := extract.FindSubmatch(data)

		if match == nil {
			t.Fatalf("%s no longer declares its roster row pattern where this guard looks", relativePath)
		}

		return string(match[1])
	}

	powerShell := readPattern("src/_roster.ps1", regexp.MustCompile(`(?m)^\$RosterRowPattern = '([^']+)'`))

	if powerShell != rosterRowPattern.String() {
		t.Fatalf("rosterRowPattern has drifted from src/_roster.ps1:\n  go:  %s\n  ps1: %s", rosterRowPattern.String(), powerShell)
	}

	python := regexp.MustCompile(readPattern("docs/phase4/hopA-inputs/regen-validation-index.py", regexp.MustCompile(`(?m)^ROSTER_ROW = re\.compile\(r"([^"]+)"\)`)))

	roster, err := os.ReadFile(filepath.Join(root, "docs", validationRosterFileName))

	if err != nil {
		t.Fatalf("read roster: %v", err)
	}

	var fromGo, fromPython []string

	for _, line := range strings.Split(strings.ReplaceAll(string(roster), "\r", ""), "\n") {
		if match := rosterRowPattern.FindStringSubmatch(line); match != nil {
			fromGo = append(fromGo, match[1])
		}

		if match := python.FindStringSubmatch(line); match != nil {
			fromPython = append(fromPython, match[1])
		}
	}

	if len(fromGo) == 0 {
		t.Fatal("the roster reads as no rows")
	}

	if strings.Join(fromGo, "\n") != strings.Join(fromPython, "\n") {
		t.Fatalf("the Go and Python roster readers disagree: %d rows against %d", len(fromGo), len(fromPython))
	}
}

// TestCommittedValidationIndexMatchesWriter holds the committed docs/validation/index.md to what
// writeValidationIndex produces from the committed roster and pages. A row banked by hand into the
// roster without its index line (or the reverse) fails here, in every lane's plain `go test`, rather
// than at the release census. The writer runs over a scratch copy: the committed index and roster,
// and an empty stand-in for each page under current/, since only the page NAMES are read.
func TestCommittedValidationIndexMatchesWriter(t *testing.T) {
	root := repoRootFromPackageDir(t)
	committedDocs := filepath.Join(root, "docs")
	docsPath := t.TempDir()
	currentPath := filepath.Join(docsPath, "validation", "current")

	if err := os.MkdirAll(currentPath, 0755); err != nil {
		t.Fatal(err)
	}

	committedIndex, err := os.ReadFile(filepath.Join(committedDocs, "validation", "index.md"))

	if err != nil {
		t.Fatalf("read committed index: %v", err)
	}

	roster, err := os.ReadFile(filepath.Join(committedDocs, validationRosterFileName))

	if err != nil {
		t.Fatalf("read roster: %v", err)
	}

	entries, err := os.ReadDir(filepath.Join(committedDocs, "validation", "current"))

	if err != nil {
		t.Fatalf("list committed pages: %v", err)
	}

	for _, entry := range entries {
		if err := os.WriteFile(filepath.Join(currentPath, entry.Name()), nil, 0644); err != nil {
			t.Fatal(err)
		}
	}

	if err := os.WriteFile(filepath.Join(docsPath, validationRosterFileName), roster, 0644); err != nil {
		t.Fatal(err)
	}

	indexPath := filepath.Join(docsPath, "validation", "index.md")

	if err := os.WriteFile(indexPath, committedIndex, 0644); err != nil {
		t.Fatal(err)
	}

	if err := writeValidationIndex(docsPath); err != nil {
		t.Fatalf("write index: %v", err)
	}

	written, err := os.ReadFile(indexPath)

	if err != nil {
		t.Fatalf("read written index: %v", err)
	}

	committedLines := strings.Split(strings.ReplaceAll(string(committedIndex), "\r", ""), "\n")
	writtenLines := strings.Split(strings.ReplaceAll(string(written), "\r", ""), "\n")

	if strings.Join(committedLines, "\n") == strings.Join(writtenLines, "\n") {
		return
	}

	// Name the rows that differ, so the fix is a line to add or remove rather than a diff to read.
	inCommitted := make(map[string]bool)

	for _, line := range committedLines {
		inCommitted[line] = true
	}

	inWritten := make(map[string]bool)

	for _, line := range writtenLines {
		inWritten[line] = true
	}

	var missing, extra []string

	for _, line := range writtenLines {
		if !inCommitted[line] {
			missing = append(missing, line)
		}
	}

	for _, line := range committedLines {
		if !inWritten[line] {
			extra = append(extra, line)
		}
	}

	t.Fatalf("docs/validation/index.md is not what the writer produces from the committed roster and pages "+
		"(rows follow the roster's order; a banked row needs its page and its index line).\n"+
		"missing from the committed index:\n  %s\nin the committed index but not written:\n  %s",
		strings.Join(missing, "\n  "), strings.Join(extra, "\n  "))
}
