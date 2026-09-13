// bestEffortTranspile_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"os"
	"path/filepath"
	"strings"
	"testing"
)

// The tripwire for the transpile-phase false green: go2cs EXITS ZERO on a package it could not fully
// type-check. The converter says so on stderr -- "did not fully type-check" from conversionDriver.go,
// or a recovered "visit file error" -- and writes a degraded emission. check-no-regression.ps1 has
// classified both as NOT MEASURED by name since 2026-08-08, but the two harnesses that ALSO transpile
// (BehavioralRunner and the MSTest BehavioralTestBase) asked the exit code alone, so a best-effort
// conversion read as a Transpile PASS and the poisoned .cs went on to Compile, Target and Output --
// where it surfaces as a downstream break billed to the wrong layer, or as a byte-identical Target
// pass over a file the run never regenerated.
//
// The remedy is src/tests/BestEffortConversion.cs, LINKED into both harnesses exactly as
// ConverterBuildInputs.cs and PlatformExclusive.cs are: one predicate, so the two cannot drift apart
// on what a measured transpile is. These guards are the tripwire that stops it being quietly edited
// back out, and they live in the tier every lane already pays for -- the converter's own `go test`.
//
// ⚠ Same BEST-EFFORT caveat as the ConverterBuildInputs guards next door, and for the same reason:
// cmd/go's test cache drops files that resolve outside the module root (computeTestInputsID, "Do not
// recheck files outside the module, GOPATH, or GOROOT root"), and these C# sources live under
// src/tests, outside src/go2cs. Editing one does NOT invalidate a cached PASS here. A change touching
// ONLY harness C# therefore owes `go test -count=1 ./...`.

const (
	bestEffortConversionSource = "../tests/BestEffortConversion.cs"
	checkNoRegressionSource    = "../tests/Behavioral/check-no-regression.ps1"
)

// harnessTranspileSources are the two harnesses that invoke the converter on behavioral packages and
// must classify its stderr, paired with the shared predicate they both delegate to. PerformanceRunner
// is deliberately absent: it transpiles only the Perf* benchmarks, which are platform-neutral by
// construction, and its Verify phase refuses to time anything whose three binaries disagree -- so a
// degraded emission there cannot reach a published number the way it can reach a behavioral verdict.
var harnessTranspileSources = []string{
	filepath.Join("..", "tests", "Behavioral", "BehavioralRunner", "Program.cs"),
	filepath.Join("..", "tests", "Behavioral", "BehavioralTests", "BehavioralTestBase.cs"),
}

// TestBestEffortMarkersMatchWhatTheConverterActuallyPrints is the SYNC guard between the two sides:
// the shared C# predicate matches on literal substrings of diagnostics this package emits, so a
// reworded warning would silently stop being classified -- a predicate that cannot fire, reporting a
// clean transpile over a degraded one, which is the exact shape it was written to remove. Asserting
// the strings from BOTH ends is what makes a rename a red gate instead of a silent regression.
func TestBestEffortMarkersMatchWhatTheConverterActuallyPrints(t *testing.T) {
	shared, err := os.ReadFile(filepath.FromSlash(bestEffortConversionSource))

	if os.IsNotExist(err) {
		t.Skipf("%s is not present; the C# harnesses are not part of this checkout", bestEffortConversionSource)
	}

	if err != nil {
		t.Fatalf("reading %s: %v", bestEffortConversionSource, err)
	}

	// The PATTERN, not the file text. Every one of the three instruments EXPLAINS these two markers in
	// prose next to the code that matches on them -- this test file included -- so a plain
	// strings.Contains over the whole source is satisfied by the comment and passes while the live
	// predicate no longer matches anything. Measured, not reasoned: the first draft of this guard was
	// written that way, and its own positive control (reword the regex, leave the header alone) came
	// back GREEN. Same class as the corpus's GoManualConversion census reporting 63 marked files
	// against a real 40 until it was anchored -- a guard that cannot fire is a false-green seed.
	pattern := markerPattern(t, string(shared))

	// Each marker, and the converter source that must still print it. Both directions are checked:
	// the C# must match on the marker, and this package must still emit it.
	markers := map[string]string{
		"did not fully type-check": "conversionDriver.go",
		"visit file error":         "conversionDriver.go",
	}

	for marker, emitter := range markers {
		if !strings.Contains(pattern, marker) {
			t.Errorf("the s_marker pattern in %s is %q, which no longer matches %q -- the harnesses would report a best-effort conversion as a Transpile PASS and hand the degraded emission to Compile, Target and Output",
				bestEffortConversionSource, pattern, marker)
		}

		emitted, err := os.ReadFile(emitter)

		if err != nil {
			t.Fatalf("reading %s: %v", emitter, err)
		}

		if !emitsLiveMarker(string(emitted), marker, `"`, "") {
			t.Errorf("%s no longer PRINTS %q from a diagnostic (a comment mentioning it does not count), but %s still matches on it -- the predicate has become unfireable, which reports every degraded conversion as fully measured",
				emitter, marker, bestEffortConversionSource)
		}
	}

	// check-no-regression.ps1 is the third instrument classifying the SAME two classes, and it is the
	// one that re-transpiles unconditionally. The three must agree on the wording or a package can be
	// NOT MEASURED in one gate and green in another. Anchored on a live -match line for the same
	// reason the C# side is anchored on its pattern: CNR explains these markers in a comment block too.
	//
	// -notmatch lines are REJECTED, and that exclusion is the guard's second positive control talking.
	// CNR carries the markers twice: once to classify a run as unmeasured, and once as `-notmatch` to
	// keep the same lines OUT of the advisory-warning count. The second is an EXCLUSION -- it would go
	// on reading fine with the classifier removed -- so a check that accepts it passes over exactly the
	// deletion it exists to catch, which is what the control measured before this line was added.
	cnr, err := os.ReadFile(filepath.FromSlash(checkNoRegressionSource))

	if err != nil {
		t.Fatalf("reading %s: %v", checkNoRegressionSource, err)
	}

	for marker := range markers {
		if !emitsLiveMarker(string(cnr), marker, "-match", "-notmatch") {
			t.Errorf("check-no-regression.ps1 no longer CLASSIFIES %q on a live -match line -- the harnesses and CNR must agree on what an unmeasured transpile is", marker)
		}
	}
}

// markerPattern extracts the regex literal BestEffortConversion.cs constructs s_marker from -- the
// live predicate, isolated from every comment that describes it.
func markerPattern(t *testing.T, source string) string {
	t.Helper()

	const decl = "Regex s_marker ="

	index := strings.Index(source, decl)

	if index < 0 {
		t.Fatalf("%s no longer declares `%s` -- this guard reads the live pattern, so it cannot verify a predicate it cannot find", bestEffortConversionSource, decl)
	}

	rest := source[index+len(decl):]
	open := strings.Index(rest, `"`)

	if open < 0 {
		t.Fatalf("%s declares s_marker with no string literal after it", bestEffortConversionSource)
	}

	rest = rest[open+1:]
	closing := strings.Index(rest, `"`)

	if closing < 0 {
		t.Fatalf("%s has an unterminated s_marker literal", bestEffortConversionSource)
	}

	return rest[:closing]
}

// emitsLiveMarker reports whether marker appears on a line that is CODE rather than commentary: not a
// // or # comment line, carrying the given anchor (a quote for a diagnostic string, `-match` for the
// PowerShell classifier), and not carrying `reject` when one is given. Both instruments document these
// markers in prose beside the code that uses them, so "the file contains the text" is not the question
// either one is being asked -- and a NEGATED use of the same marker is not an affirmative one.
func emitsLiveMarker(source string, marker string, anchor string, reject string) bool {
	for _, line := range strings.Split(source, "\n") {
		trimmed := strings.TrimSpace(strings.TrimSuffix(line, "\r"))

		if strings.HasPrefix(trimmed, "//") || strings.HasPrefix(trimmed, "#") {
			continue
		}

		if reject != "" && strings.Contains(trimmed, reject) {
			continue
		}

		if strings.Contains(trimmed, marker) && strings.Contains(trimmed, anchor) {
			return true
		}
	}

	return false
}

// TestHarnessTranspilePhasesClassifyConverterStdErr pins that both harnesses still ASK the shared
// predicate. The structural protection is the single linked file; this is what stops a future edit
// from going back to the exit code alone, in either harness, without a red gate.
func TestHarnessTranspilePhasesClassifyConverterStdErr(t *testing.T) {
	if _, err := os.Stat(filepath.FromSlash(bestEffortConversionSource)); os.IsNotExist(err) {
		t.Skipf("%s is not present; the C# harnesses are not part of this checkout", bestEffortConversionSource)
	}

	for _, source := range harnessTranspileSources {
		contents, err := os.ReadFile(source)

		if os.IsNotExist(err) {
			t.Errorf("%s is missing; one of the two transpiling harnesses has moved and this guard no longer covers it", source)
			continue
		}

		if err != nil {
			t.Fatalf("reading %s: %v", source, err)
		}

		if !strings.Contains(string(contents), "BestEffortConversion.NotFullyRegenerated") {
			t.Errorf("%s does not consult BestEffortConversion -- go2cs exits 0 on a best-effort conversion, so a transpile phase that reads only the exit code reports PASS over output it never regenerated",
				source)
		}
	}
}
