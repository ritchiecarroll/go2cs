// validationProofPages_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

package main

import (
	"encoding/json"
	"os"
	"path/filepath"
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
// The fixture record predates the field, which is exactly the third case.
func TestValidationProofPageStatesTheTerminalContext(t *testing.T) {
	comparison, disclosures := loadProofFixture(t)

	cases := []struct {
		name     string
		config   string
		tiered   bool
		oracle   string
		terminal string
		want     string
	}{
		{name: "not probed (the fixture as committed)", want: "Measured at `Debug`."},
		{name: "controlling terminal present", terminal: driverTerminalPresent, want: "Measured at `Debug`, under a controlling terminal."},
		{name: "controlling terminal absent", terminal: driverTerminalAbsent, want: "Measured at `Debug`, with no controlling terminal."},
		{name: "release, oracle and terminal in one sentence", config: "Release", oracle: "go version go1.23.12 linux/amd64", terminal: driverTerminalPresent,
			want: "Measured at `Release` (tiered JIT off), oracle `go version go1.23.12 linux/amd64`, under a controlling terminal."},
	}

	for _, c := range cases {
		t.Run(c.name, func(t *testing.T) {
			variant := comparison
			variant.Environment.Configuration = c.config
			variant.Environment.Tiered = c.tiered
			variant.Environment.OracleGoVersion = c.oracle
			variant.Environment.Terminal = c.terminal

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
