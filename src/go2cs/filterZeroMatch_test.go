// filterZeroMatch_test.go - Gbtc
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
	"strings"
	"testing"
	"time"
)

// The ZERO-MATCH GUARD. A `-test-filter` run that matches nothing compares zero verdicts, so both
// sides exit 0, no mismatch exists, and the run reads exactly like a clean validation — a vacuous
// proof that is indistinguishable from a real one. filterMatchedNothing is what makes that case
// loud.
//
// This test is a POSITIVE CONTROL first and a specification second: the fire case is asserted
// before the quiet cases, because a guard that never fires would pass every "does not fire" test
// trivially. Neuter the condition in filterMatchedNothing and TestFilterZeroMatchGuardFires goes
// red immediately.
func TestFilterZeroMatchGuardFires(t *testing.T) {
	message, fired := filterMatchedNothing("^TestNoSuchName$", "validated", true, 0, 0, 0)

	if !fired {
		t.Fatal("guard must fire: a filtered run that compared ZERO verdicts measured nothing and must not read as a pass")
	}

	// The message has to name the filter — an operator reading a failed census needs to see the
	// regex that matched nothing without going back to the command line.
	if !strings.Contains(message, "^TestNoSuchName$") {
		t.Errorf("message must quote the filter that matched nothing, got: %s", message)
	}

	// ...and must say which case this is, per the excluded/silent distinction the guard exists to
	// disambiguate rather than to exempt.
	if !strings.Contains(message, "excludes no declarations") {
		t.Errorf("with nothing excluded the message must say the regex itself matched nothing, got: %s", message)
	}
}

// The excluded case still FIRES — it is not an exemption. A filter naming a deliberately excluded
// declaration measured just as little as a typo'd one; the only difference the guard owes is
// telling the operator which situation they are in.
func TestFilterZeroMatchGuardReportsExcludedDeclarations(t *testing.T) {
	message, fired := filterMatchedNothing("^TestGatedThing$", "validated", true, 0, 0, 3)

	if !fired {
		t.Fatal("guard must still fire when the package has excluded declarations: an excluded target measured nothing either")
	}

	if !strings.Contains(message, "excludes 3 declaration(s)") {
		t.Errorf("message must report the excluded count so the operator can tell the two cases apart, got: %s", message)
	}
}

// Everything the guard must NOT claim. Each row is a case that already has a correct outcome
// elsewhere, and a guard that stole it would turn a precise diagnosis into a misleading one.
func TestFilterZeroMatchGuardStaysQuiet(t *testing.T) {
	cases := []struct {
		name          string
		filter        string
		status        string
		matched       bool
		goCount       int
		csCount       int
		excludedCount int
		why           string
	}{
		{
			name: "no filter", filter: "", status: "validated", matched: true,
			why: "an UNFILTERED package with no verdicts is a different situation; this guard is about the filter",
		},
		{
			name: "filter matched something", filter: "^TestReal$", status: "validated", matched: true,
			goCount: 4, csCount: 4,
			why: "verdicts were compared, so the census measured what it was asked to",
		},
		{
			name: "not-applicable package", filter: "^TestAnything$", status: "not-applicable", matched: true,
			why: "zero verdicts are a property of a package with no eligible tests, not of the filter",
		},
		{
			name: "already failing", filter: "^TestAnything$", status: "failing", matched: false,
			why: "a real failure already owns this run; the guard must not overwrite its diagnosis",
		},
		{
			name: "one-sided match, go only", filter: "^TestOneSided$", status: "validated", matched: true,
			goCount: 2, csCount: 0,
			why: "one-sided rows are mismatches and stay fatal through the existing path",
		},
		{
			name: "one-sided match, c# only", filter: "^TestOneSided$", status: "validated", matched: true,
			goCount: 0, csCount: 2,
			why: "same as above, mirrored — the guard must not claim either direction",
		},
		{
			name: "infrastructure-blocked", filter: "^TestAnything$", status: "infrastructure-blocked", matched: false,
			why: "a capability block is a more specific and more useful diagnosis than 'matched nothing'",
		},
	}

	for _, test := range cases {
		t.Run(test.name, func(t *testing.T) {
			if message, fired := filterMatchedNothing(
				test.filter, test.status, test.matched,
				test.goCount, test.csCount, test.excludedCount); fired {
				t.Errorf("guard must not fire (%s), got: %s", test.why, message)
			}
		})
	}
}

// The GATED-RECORD STAMP. A `-test-filter` run rewrites the SAME go2cs_test_comparison.json a full
// run writes, and nothing in the file says which it is. That is not hypothetical: a gated census
// once read its own filter's survivor set back as a package's real verdicts and reported the row
// bankable, and the only tell was arithmetic (nine `go` entries where the full run had ten). It is
// worse than a stale file, because the fleet's restore step CANNOT clear it -- the record is
// gitignored, and `git clean -fd` skips ignored paths -- so a diagnostic record survives into the
// next run looking exactly like that run's own output.
//
// These exercise writeComparisonRecord, the single write path, rather than marshalling a struct in
// the test: a contract-only assertion stays GREEN when the stamp is deleted from the code, which is
// a guard that cannot fail for the reason it exists. Verified by control -- removing the stamp
// reddens TestGatedRecordCarriesItsFilter naming the missing key.
func readComparisonRecord(t *testing.T, dir string) map[string]any {
	t.Helper()
	blob, err := os.ReadFile(filepath.Join(dir, "go2cs_test_comparison.json"))
	if err != nil {
		t.Fatalf("read written record: %v", err)
	}
	var back map[string]any
	if err := json.Unmarshal(blob, &back); err != nil {
		t.Fatalf("unmarshal written record: %v", err)
	}
	return back
}

func TestGatedRecordCarriesItsFilter(t *testing.T) {
	dir := t.TempDir()
	result := testComparison{Package: "net"}
	if err := writeComparisonRecord(dir, &result, "^TestUnixgramServer$"); err != nil {
		t.Fatalf("write gated record: %v", err)
	}
	if got := readComparisonRecord(t, dir)["testFilter"]; got != "^TestUnixgramServer$" {
		t.Fatalf("a filtered record must name its filter so it cannot be read back as a full run; got %#v", got)
	}
}

// The other half, and the one that keeps an ungated record byte-identical to what every consumer
// already reads: no key at all when no filter was in force.
func TestUngatedRecordCarriesNoFilterKey(t *testing.T) {
	dir := t.TempDir()
	result := testComparison{Package: "net"}
	if err := writeComparisonRecord(dir, &result, ""); err != nil {
		t.Fatalf("write ungated record: %v", err)
	}
	if _, present := readComparisonRecord(t, dir)["testFilter"]; present {
		t.Fatal("an unfiltered record must be unchanged -- no testFilter key at all")
	}
}

// The early-exit shapes (infrastructure-blocked, not-applicable) are maps rather than the struct,
// and they are written by the same path, so they get the same stamp. Guarded because they are the
// two shapes a reader is LEAST likely to check and a filtered run reaches them just as readily.
func TestGatedStampReachesTheEarlyExitShapes(t *testing.T) {
	dir := t.TempDir()
	result := map[string]any{"package": "net", "status": "not-applicable", "matched": true}
	if err := writeComparisonRecord(dir, result, "^TestNothing$"); err != nil {
		t.Fatalf("write gated early-exit record: %v", err)
	}
	if got := readComparisonRecord(t, dir)["testFilter"]; got != "^TestNothing$" {
		t.Fatalf("an early-exit record produced under a filter must say so; got %#v", got)
	}
}

// A shape the stamp cannot reach must REFUSE to write, never write silently unstamped. This is the
// mistake the cut itself made once: the struct site passed testComparison by VALUE, the type switch
// matched only the pointer, and the record would have published unstamped with the build green.
func TestUnstampableRecordIsRefusedRatherThanWrittenUnstamped(t *testing.T) {
	dir := t.TempDir()
	err := writeComparisonRecord(dir, testComparison{Package: "net"}, "^TestSomething$")
	if err == nil {
		t.Fatal("a record shape the stamp cannot reach must be refused: writing it unstamped publishes a gated run as a full one")
	}
	if !strings.Contains(err.Error(), "testComparison") {
		t.Fatalf("the refusal must name the offending shape so the fix is obvious; got %v", err)
	}
}

// The COLLISION guard, and the reason the key is `testFilter` and not `gated`. `gated` is already
// taken by the Gated array (live data: net/http carries one entry, TestTransportGCRequest), so a
// boolean of that name would land an array and a scalar under one key on exactly the rows most
// worth reading carefully.
func TestGatedArrayAndFilterStampCoexist(t *testing.T) {
	dir := t.TempDir()
	result := testComparison{
		Package: "net/http",
		Gated:   []capabilityGatedDeclaration{{Name: "TestTransportGCRequest", Capabilities: "gc-liveness"}},
	}
	if err := writeComparisonRecord(dir, &result, "^TestWriteDeadline"); err != nil {
		t.Fatalf("write record carrying both: %v", err)
	}
	back := readComparisonRecord(t, dir)
	if _, isArray := back["gated"].([]any); !isArray {
		t.Fatalf("`gated` must remain the capability-gated ARRAY, never a filter flag; got %T", back["gated"])
	}
	if got := back["testFilter"]; got != "^TestWriteDeadline" {
		t.Fatalf("`testFilter` must be the filter expression as a string; got %#v", got)
	}
}


// THE RUN ACTION'S FILTER. `-test-filter` was honoured by `compare` and SILENTLY IGNORED by
// `run`: the run action built its own argv of `--json -timeout` and never passed `--run`, so a
// caller asking for a gated single-test run got the WHOLE package and no diagnostic said
// otherwise. That is the zero-match guard's own failure shape one layer out -- a run that
// measured something entirely different from what was asked, reading exactly like the run that
// was asked for. It cost a real reading: a probe patched into one test's emission ran a package
// that died before reaching that test, while the probe's own validity check still said intact.
//
// convertedHostArgs IS the argv -- `case "run"` passes `convertedHostArgs(options)...` verbatim
// and compare seeds `csArgs` from it -- so asserting the function asserts the command line, not a
// re-implementation of it. Both actions derive from this one source precisely so they cannot
// drift apart again.
//
// POSITIVE CONTROL FIRST, as the guard above: a test that only asserts ABSENCE passes trivially
// against a function that returns nothing at all.
func TestRunActionHostArgvCarriesTheFilter(t *testing.T) {
	args := convertedHostArgs(Options{testFilter: "^TestFinalizerType$", testTimeout: 5 * time.Minute})

	idx := -1
	for i, a := range args {
		if a == "--run" {
			idx = i
		}
	}
	if idx < 0 {
		t.Fatalf("a -test-filter run must hand the host --run, or the run measures the whole package while reading as gated; got %v", args)
	}
	if idx+1 >= len(args) || args[idx+1] != "^TestFinalizerType$" {
		t.Fatalf("--run must be followed by the filter VERBATIM, the same string compare hands both sides; got %v", args)
	}

	// The prefix the host needs regardless of gating stays present -- a filter must ADD to the
	// argv, never replace it.
	if len(args) < 3 || args[0] != "--json" || args[1] != "-timeout" {
		t.Fatalf("the host argv must still open with --json -timeout; got %v", args)
	}
}

// ...and the ABSENCE case, which is only meaningful because the arm above proves the function can
// emit --run at all. An ungated run must hand the host NO filter: passing an empty --run would
// make every ungated run a filtered one with an empty regex, which matches everything on Go's
// flag semantics today and is one library change from matching nothing.
func TestUngatedRunActionHostArgvCarriesNoFilter(t *testing.T) {
	args := convertedHostArgs(Options{testTimeout: 5 * time.Minute})

	for _, a := range args {
		if a == "--run" {
			t.Fatalf("an ungated run must pass no --run at all, not an empty one; got %v", args)
		}
	}
	if len(args) != 3 || args[0] != "--json" || args[1] != "-timeout" {
		t.Fatalf("the ungated host argv is exactly --json -timeout <d>; got %v", args)
	}
}
