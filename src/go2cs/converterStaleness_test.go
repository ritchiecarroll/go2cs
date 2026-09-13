// converterStaleness_test.go - Gbtc
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
	"path/filepath"
	"strings"
	"testing"
	"time"
)

// The stale-binary self-check (converterStaleness.go), guarded POSITIVE CONTROL FIRST.
//
// The ordering is deliberate and is the whole point: a probe that never fires would satisfy every
// "stays quiet" assertion trivially, so the fire case is asserted before the quiet ones. Neuter
// newestConverterInput's comparison and TestConverterStalenessDetectsTouchedSource goes red
// immediately, which is what makes the quiet-case tests worth anything.

// newestConverterInput must SEE a source file touched after the binary — the condition that
// produced the 2026-08-30 std.reflect artifacts from a binary predating 433e9e4e0.
func TestConverterStalenessDetectsTouchedSource(t *testing.T) {
	root := syntheticConverterRoot(t)

	older := time.Now().Add(-2 * time.Hour)
	touchAll(t, root, older)

	// One source file edited after the build — the incident's shape.
	edited := filepath.Join(root, "visitFuncDecl.go")
	touch(t, edited, time.Now())

	newest, newestPath := newestConverterInput(root)

	if newestPath == "" {
		t.Fatal("newestConverterInput found no build inputs at all in a synthetic converter root")
	}

	if filepath.Base(newestPath) != "visitFuncDecl.go" {
		t.Errorf("expected the touched source to be newest, got %q", filepath.Base(newestPath))
	}

	if !newest.After(older) {
		t.Errorf("newest time %v did not advance past the baseline %v", newest, older)
	}
}

// Route #5's half: an EMBEDDED ASSET is a build input even though it is not a .go file. Editing a
// csproj template changes every project the converter emits while touching no Go source, so a
// .go-only scan would report "up to date" over a changed emission.
func TestConverterStalenessSeesEmbeddedAssets(t *testing.T) {
	root := syntheticConverterRoot(t)

	touchAll(t, root, time.Now().Add(-2*time.Hour))
	touch(t, filepath.Join(root, "csproj-template.xml"), time.Now())

	_, newestPath := newestConverterInput(root)

	if filepath.Base(newestPath) != "csproj-template.xml" {
		t.Errorf("an embedded asset must count as a build input (route #5); newest was %q", filepath.Base(newestPath))
	}
}

// go.mod and go.sum are inputs for the same reason: a `go` directive bump or a dependency change
// alters the built binary exactly as a source edit does.
func TestConverterStalenessSeesModuleFiles(t *testing.T) {
	root := syntheticConverterRoot(t)

	touchAll(t, root, time.Now().Add(-2*time.Hour))
	touch(t, filepath.Join(root, "go.sum"), time.Now())

	if _, newestPath := newestConverterInput(root); filepath.Base(newestPath) != "go.sum" {
		t.Errorf("go.sum must count as a build input; newest was %q", filepath.Base(newestPath))
	}
}

// `bin` holds the executable itself, so walking it would compare the binary against its own
// timestamp and report every fresh build as stale — the check would then cry wolf on every run.
func TestConverterStalenessIgnoresBuildOutput(t *testing.T) {
	root := syntheticConverterRoot(t)

	touchAll(t, root, time.Now().Add(-2*time.Hour))

	binDir := filepath.Join(root, "bin")

	// A .go file inside bin/ is the adversarial case: right extension, wrong directory.
	writeFile(t, filepath.Join(binDir, "go2cs.exe"), "binary")
	writeFile(t, filepath.Join(binDir, "generated.go"), "package main\n")
	touch(t, filepath.Join(binDir, "go2cs.exe"), time.Now())
	touch(t, filepath.Join(binDir, "generated.go"), time.Now())

	_, newestPath := newestConverterInput(root)

	if filepath.Base(newestPath) == "go2cs.exe" || filepath.Base(newestPath) == "generated.go" {
		t.Errorf("bin/ must be skipped or every fresh build reports itself stale; newest was %q", newestPath)
	}
}

// The source root is identified by MARKER FILES, never by directory name — a binary that merely
// sits in some `bin` directory must not be measured against whatever module it lands next to.
func TestConverterSourceRootRequiresBothMarkers(t *testing.T) {
	root := t.TempDir()

	if isConverterSourceRoot(root) {
		t.Error("an empty directory is not a converter source root")
	}

	writeFile(t, filepath.Join(root, "main.go"), "package main\n")

	if isConverterSourceRoot(root) {
		t.Error("main.go alone is not enough — any Go program has one")
	}

	writeFile(t, filepath.Join(root, "go.mod"), "module somethingelse\n\ngo 1.23.12\n")

	if isConverterSourceRoot(root) {
		t.Error("a DIFFERENT module must not be claimed as this converter's source")
	}

	writeFile(t, filepath.Join(root, "go.mod"), "module go2cs\n\ngo 1.23.12\n")

	if !isConverterSourceRoot(root) {
		t.Error("main.go plus `module go2cs` is this converter's source root")
	}
}

// A deployed or relocated binary with no adjacent source tree is legitimate and normal, and must be
// SILENT rather than warn about a tree it cannot see.
func TestAdjacentConverterSourceAbsentWhenDeployed(t *testing.T) {
	deployed := filepath.Join(t.TempDir(), "tools", "go2cs.exe")

	if err := os.MkdirAll(filepath.Dir(deployed), 0o755); err != nil {
		t.Fatal(err)
	}

	writeFile(t, deployed, "not really an executable")

	if dir, ok := adjacentConverterSource(deployed); ok {
		t.Errorf("a deployed binary has no converter source beside it, got %q", dir)
	}
}

// THE ENUMERATION — the 2026-09-03 incident's own shape. The advisory named ONE file where six
// inputs were newer, and a lane reasoned from that single name ("that entry only touches syscall")
// to the conclusion that its measurement stood. Three files touched must yield a count of THREE and
// all three paths, never the newest one.
func TestStaleConverterInputsEnumeratesEveryNewerInput(t *testing.T) {
	root := syntheticConverterRoot(t)

	builtAt := time.Now().Add(-2 * time.Hour)
	touchAll(t, root, builtAt)

	touched := []string{"visitFuncDecl.go", "main.go", "csproj-template.xml"}

	for index, name := range touched {
		// Distinct instants so the newest-first ordering is asserted, not accidental.
		touch(t, filepath.Join(root, name), time.Now().Add(time.Duration(index)*time.Minute))
	}

	stale := staleConverterInputs(root, builtAt)

	if len(stale) != len(touched) {
		t.Fatalf("expected %d stale inputs, got %d: %v", len(touched), len(stale), staleRelPaths(stale))
	}

	found := map[string]bool{}

	for _, input := range stale {
		found[input.relPath] = true
	}

	for _, name := range touched {
		if !found[name] {
			t.Errorf("%q was modified after the binary but is missing from the enumeration: %v", name, staleRelPaths(stale))
		}
	}

	// Newest first: csproj-template.xml was touched last.
	if stale[0].relPath != "csproj-template.xml" {
		t.Errorf("enumeration must be newest-first; got %v", staleRelPaths(stale))
	}
}

// An input NOT newer than the binary is not stale — the enumeration must not simply list the whole
// build-input set, or the refusal would fire on every fresh build.
func TestStaleConverterInputsEmptyWhenBinaryIsCurrent(t *testing.T) {
	root := syntheticConverterRoot(t)

	touchAll(t, root, time.Now().Add(-2*time.Hour))

	if stale := staleConverterInputs(root, time.Now()); len(stale) != 0 {
		t.Errorf("a binary newer than every input is current; enumeration returned %v", staleRelPaths(stale))
	}
}

// The emission-affecting split. A converter `_test.go` is a real build input by the shared
// definition (ConverterBuildInputs.cs enumerates every *.go, and the harness predicates rebuild on
// one) but `go build` excludes it from go2cs.exe, so it cannot have changed the emission. Everything
// else in the set can — the embedded assets especially, which is route #5's whole point.
func TestStaleInputsClassifyEmissionAffecting(t *testing.T) {
	root := syntheticConverterRoot(t)

	builtAt := time.Now().Add(-2 * time.Hour)
	writeFile(t, filepath.Join(root, "visitFuncDecl_test.go"), "package main\n")
	touchAll(t, root, builtAt)

	touch(t, filepath.Join(root, "visitFuncDecl_test.go"), time.Now())
	touch(t, filepath.Join(root, "csproj-template.xml"), time.Now())
	touch(t, filepath.Join(root, "go.sum"), time.Now())

	report := &stalenessReport{sourceDir: root, builtAt: builtAt, inputs: staleConverterInputs(root, builtAt)}

	if len(report.inputs) != 3 {
		t.Fatalf("expected 3 stale inputs, got %v", staleRelPaths(report.inputs))
	}

	if report.emissionAffecting() != 2 {
		t.Errorf("the embedded asset and go.sum are emission-affecting and the _test.go is not; got %d of 3", report.emissionAffecting())
	}

	for _, input := range report.inputs {
		wantAffects := input.relPath != "visitFuncDecl_test.go"

		if input.affectsEmission != wantAffects {
			t.Errorf("%q: affectsEmission = %v, want %v", input.relPath, input.affectsEmission, wantAffects)
		}
	}
}

// THE DECISION TABLE. -stdlib and -tests refuse; the named flag is the only escape; every other
// shape — the single-file/single-package scratch probe, and -recurse — keeps the advisory.
func TestStalenessRefusalAppliesToTheBankedDriversOnly(t *testing.T) {
	cases := []struct {
		name       string
		stale      bool
		converting bool
		allowStale bool
		refuses    bool
	}{
		{"stdlib or tests, stale, no flag: REFUSE", true, true, false, true},
		{"stdlib or tests, stale, flag passed: proceed", true, true, true, false},
		{"single package, stale: advisory only", true, false, false, false},
		{"single package, stale, flag passed: advisory only", true, false, true, false},
		{"stdlib or tests, current binary: proceed", false, true, false, false},
		{"single package, current binary: proceed", false, false, false, false},
	}

	for _, testCase := range cases {
		if got := stalenessRefuses(testCase.stale, testCase.converting, testCase.allowStale); got != testCase.refuses {
			t.Errorf("%s: stalenessRefuses(%v, %v, %v) = %v, want %v",
				testCase.name, testCase.stale, testCase.converting, testCase.allowStale, got, testCase.refuses)
		}
	}
}

// A refusal that does not say how to proceed teaches the reader to reach for something worse, and
// the deliberate case (an A/B against a preserved binary) must stay reachable by name.
func TestRefusalCarriesTheEnumerationAndBothRemedies(t *testing.T) {
	root := syntheticConverterRoot(t)

	builtAt := time.Now().Add(-2 * time.Hour)
	touchAll(t, root, builtAt)
	touch(t, filepath.Join(root, "visitFuncDecl.go"), time.Now())

	report := &stalenessReport{sourceDir: root, builtAt: builtAt, inputs: staleConverterInputs(root, builtAt)}
	refusal := report.refusal()

	for _, required := range []string{
		"visitFuncDecl.go",         // the enumeration itself
		"1 converter build input",  // the extent, grammatical at one
		"emission-affecting",       // the split
		"go build",                 // remedy one
		"-allow-stale-converter",   // remedy two, named so a stale run says so in its command line
		root,                       // where to run the rebuild
	} {
		if !strings.Contains(refusal, required) {
			t.Errorf("the refusal must carry %q:\n%s", required, refusal)
		}
	}
}

// The cap keeps a wide staleness readable without hiding its extent: ten names plus the remainder,
// with the total still stated in full.
func TestReportCapsTheListedPathsButNotTheCount(t *testing.T) {
	root := syntheticConverterRoot(t)

	for index := 0; index < 12; index++ {
		writeFile(t, filepath.Join(root, fmt.Sprintf("visit%02d.go", index)), "package main\n")
	}

	builtAt := time.Now().Add(-2 * time.Hour)
	touchAll(t, root, builtAt)

	for index := 0; index < 12; index++ {
		touch(t, filepath.Join(root, fmt.Sprintf("visit%02d.go", index)), time.Now())
	}

	report := &stalenessReport{sourceDir: root, builtAt: builtAt, inputs: staleConverterInputs(root, builtAt)}

	if len(report.inputs) != 12 {
		t.Fatalf("expected 12 stale inputs, got %v", staleRelPaths(report.inputs))
	}

	body := report.body("  ")

	if !strings.Contains(body, "12 converter build inputs were") {
		t.Errorf("the count must state the full extent even when the list is capped:\n%s", body)
	}

	if !strings.Contains(body, "... and 2 more") {
		t.Errorf("expected the capped remainder line for 12 inputs at a cap of %d:\n%s", maxListedStaleInputs, body)
	}

	if listed := strings.Count(body, "  * visit"); listed != maxListedStaleInputs {
		t.Errorf("expected %d listed paths, counted %d:\n%s", maxListedStaleInputs, listed, body)
	}
}

// An asset named by //go:embed in two different sources is ONE build input. A count that
// double-reported it would be reporting the directives rather than the inputs — and this report's
// whole job is to state an extent a reader can trust.
func TestConverterBuildInputsDeduplicatesSharedAssets(t *testing.T) {
	root := syntheticConverterRoot(t)

	writeFile(t, filepath.Join(root, "secondEmbedder.go"),
		"package main\n\nimport _ \"embed\"\n\n//go:embed csproj-template.xml\nvar alsoTemplate string\n")

	template := filepath.Join(root, "csproj-template.xml")
	occurrences := 0

	for _, stamp := range converterBuildInputs(root) {
		if inputKey(stamp.path) == inputKey(template) {
			occurrences++
		}
	}

	if occurrences != 1 {
		t.Errorf("an asset embedded by two sources is one build input; counted %d", occurrences)
	}
}

// --- helpers -------------------------------------------------------------------------------------

func staleRelPaths(inputs []staleInput) []string {
	paths := make([]string, 0, len(inputs))

	for _, input := range inputs {
		paths = append(paths, input.relPath)
	}

	return paths
}

// syntheticConverterRoot builds a miniature converter source tree: marker files, a nested internal/
// package, and an embedded asset named by a //go:embed directive.
func syntheticConverterRoot(t *testing.T) string {
	t.Helper()

	root := t.TempDir()

	writeFile(t, filepath.Join(root, "go.mod"), "module go2cs\n\ngo 1.23.12\n")
	writeFile(t, filepath.Join(root, "go.sum"), "")
	writeFile(t, filepath.Join(root, "main.go"), "package main\n\nfunc main() {}\n")
	writeFile(t, filepath.Join(root, "visitFuncDecl.go"), "package main\n")
	writeFile(t, filepath.Join(root, "csproj-template.xml"), "<Project />\n")
	writeFile(t, filepath.Join(root, "embeddedTemplates.go"),
		"package main\n\nimport _ \"embed\"\n\n//go:embed csproj-template.xml\nvar template string\n")
	writeFile(t, filepath.Join(root, "internal", "stdlibmeta", "meta.go"), "package stdlibmeta\n")

	return root
}

// writeFile is linkStagedFixtures_test.go's helper, reused rather than duplicated — same package,
// same signature, same MkdirAll-then-write semantics.

func touch(t *testing.T, path string, when time.Time) {
	t.Helper()

	if err := os.Chtimes(path, when, when); err != nil {
		t.Fatal(err)
	}
}

// touchAll backdates every file in the tree, so a subsequent touch is unambiguously the newest.
func touchAll(t *testing.T, root string, when time.Time) {
	t.Helper()

	err := filepath.WalkDir(root, func(path string, entry os.DirEntry, err error) error {
		if err != nil || entry.IsDir() {
			return err
		}

		return os.Chtimes(path, when, when)
	})

	if err != nil {
		t.Fatal(err)
	}
}
