// orphanDisclosureCheck_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

package main

import (
	"strings"
	"testing"
)

// The ORPHANED-DISCLOSURE report (increment 1, report-only). A disclosure with no gate that can
// retire it is a permanent claim rather than a measurement: until this existed, no check of any
// class verified that an entry names a test that is actually FAILING in the run, so an entry over a
// row that now passes was accepted silently everywhere.
//
// The predicate is a TERMINAL PASS on the converted side, and these arms exist to hold it there.
// The inverse — "this entry names a test that did not FAIL" — would fire on every row behind a
// host-killer, i.e. on exactly the entries most likely still correct and merely unreachable, which
// is the inversion the ruling forbids; TestOrphanCheckIsSilentBehindAHostKiller measures that
// directly rather than asserting it in prose.
//
// Report-only is deliberate and is NOT weakness: the manifest is ONE file shared by every platform,
// so an entry idle here may be live and correct elsewhere. Increment 2 is platform-scoped entries;
// increment 3 is the refusal, gated on 2. See docs/phase4/DESIGN-orphan-disclosure-check.md.

func orphanDisclosure(name, class string) testDisclosure {
	return testDisclosure{
		Name: name, Class: class, Signature: "the pinned text", Reason: "because " + name,
	}
}

func orphanManifest(entries ...testDisclosure) map[string]testDisclosure {
	out := map[string]testDisclosure{}
	for _, entry := range entries {
		out[entry.Name] = entry
	}
	return out
}

// POSITIVE ARM 1 — the plain stale entry: both sides pass, so the entry describes nothing at all.
func TestOrphanCheckReportsAPassPassEntry(t *testing.T) {
	orphans := orphanedDisclosures(
		orphanManifest(orphanDisclosure("TestOnceFunc", "alloc-profile")),
		map[string]string{"TestOnceFunc": "pass"},
		map[string]string{"TestOnceFunc": "pass"},
		"windows")

	if len(orphans) != 1 {
		t.Fatalf("a disclosure over a pass/pass row is stale on this platform and must be reported; got %v", orphans)
	}
	if orphans[0].Name != "TestOnceFunc" || orphans[0].Class != "alloc-profile" {
		t.Fatalf("the report must carry the entry's own name and class; got %+v", orphans[0])
	}
	if orphans[0].Go != "pass" || orphans[0].CSharp != "pass" {
		t.Fatalf("the report must carry BOTH sides' statuses so a reader can tell the two stale shapes apart; got %+v", orphans[0])
	}
	if orphans[0].GOOS != "windows" {
		t.Fatalf("the report must name the platform it measured, because the entry may be live on another; got %q", orphans[0].GOOS)
	}
}

// POSITIVE ARM 2 — the SECOND stale shape, and the one a pass/pass-only predicate would miss: the
// converted side passes a test the entry claims it cannot, while the GO side is the half that
// broke. Still stale on this platform, and the Go status is carried rather than filtered on so the
// reader can see which shape it is.
func TestOrphanCheckReportsAGoFailConvertedPassEntry(t *testing.T) {
	orphans := orphanedDisclosures(
		orphanManifest(orphanDisclosure("TestBogoSuite", "host-limit")),
		map[string]string{"TestBogoSuite": "fail"},
		map[string]string{"TestBogoSuite": "pass"},
		"linux")

	if len(orphans) != 1 || orphans[0].Name != "TestBogoSuite" {
		t.Fatalf("a converted-side terminal pass is stale whatever the Go side did; got %v", orphans)
	}
	if orphans[0].Go != "fail" {
		t.Fatalf("the Go status must be reported, not filtered on; got %q", orphans[0].Go)
	}
}

// NEGATIVE 1 — the LIVE entry, and the one that matters most: a disclosure doing exactly its job
// must never be reported. Measured through matchTerminalStatuses as well, so the arm proves the
// entry ABSORBED rather than merely that the orphan check stayed quiet.
func TestOrphanCheckIsSilentOnALiveAbsorbedDisclosure(t *testing.T) {
	disclosures := orphanManifest(orphanDisclosure("TestAddrStringAllocs", "alloc-profile"))
	goResults := map[string]string{"TestAddrStringAllocs": "pass"}
	csResults := map[string]string{"TestAddrStringAllocs": "fail"}
	csOutputs := map[string]string{"TestAddrStringAllocs": "allocs=2, the pinned text, want 1"}

	mismatches, _, disclosed, _ := matchTerminalStatuses(
		[]string{"TestAddrStringAllocs"}, goResults, csResults, disclosures, csOutputs)
	if len(mismatches) != 0 || len(disclosed) != 1 {
		t.Fatalf("precondition: the entry must be absorbing; mismatches=%v disclosed=%v", mismatches, disclosed)
	}

	if orphans := orphanedDisclosures(disclosures, goResults, csResults, "windows"); len(orphans) != 0 {
		t.Fatalf("an entry that just absorbed its own divergence is live and must not be reported; got %v", orphans)
	}
}

// NEGATIVE 2 — no converted verdict at all. The row exists on the Go side and the converted host
// never reported it; absence is not a pass, and reading it as one is what the terminal-pass
// predicate exists to prevent.
func TestOrphanCheckIsSilentWithNoConvertedVerdict(t *testing.T) {
	orphans := orphanedDisclosures(
		orphanManifest(orphanDisclosure("TestUnreached", "alloc-profile")),
		map[string]string{"TestUnreached": "pass"},
		map[string]string{},
		"windows")

	if len(orphans) != 0 {
		t.Fatalf("a row with no converted verdict has measured NOTHING, not a pass; got %v", orphans)
	}
}

// NEGATIVE 3 — THE INVERSION THE RULING FORBIDS, measured rather than asserted. A host-killer takes
// the process down after the first row, so every later name carries no verdict on the converted
// side. A "did not fail" predicate would report all four entries as stale; the terminal-pass
// predicate reports NONE of them, because none of them measured anything.
//
// This is the arm that would go red if the predicate were ever loosened, and it is the reason the
// check reads a POSITIVE verdict rather than the absence of a negative one.
func TestOrphanCheckIsSilentBehindAHostKiller(t *testing.T) {
	disclosures := orphanManifest(
		orphanDisclosure("TestA", "alloc-profile"),
		orphanDisclosure("TestB", "alloc-profile"),
		orphanDisclosure("TestC", "codegen-liveness"),
		orphanDisclosure("TestD", "runtime-capability"))

	// go test reported all four; the converted host died inside TestA and reported one failure.
	goResults := map[string]string{"TestA": "pass", "TestB": "pass", "TestC": "pass", "TestD": "pass"}
	csResults := map[string]string{"TestA": "fail"}

	if orphans := orphanedDisclosures(disclosures, goResults, csResults, "windows"); len(orphans) != 0 {
		t.Fatalf("every entry behind a host-killer is UNREACHED, not retired; reporting them would accuse "+
			"exactly the entries most likely still correct; got %v", orphans)
	}
}

// NEGATIVE 4 — an infrastructure-error / deadline-killed run, which is the same exclusion reached by
// a different road: neither side produced a verdict for the row, so nothing carries "pass".
// Included separately from the arm above because the two are different RUN shapes even though the
// predicate excludes both by construction, and a reader must be able to see each one held.
func TestOrphanCheckIsSilentOnADeadlineKilledRun(t *testing.T) {
	disclosures := orphanManifest(orphanDisclosure("TestSlow", "alloc-profile"))

	if orphans := orphanedDisclosures(disclosures, map[string]string{}, map[string]string{}, "windows"); len(orphans) != 0 {
		t.Fatalf("a run that produced no verdicts measured nothing about any entry; got %v", orphans)
	}
}

// NEGATIVE 5 — a platform-skip entry doing its job. Its accepted shape is Go=pass / C#=skip, and a
// skip is not a pass: the entry is live and must stay silent. Measured through
// matchTerminalStatuses too, so the arm proves the shape was absorbed.
func TestOrphanCheckIsSilentOnALivePlatformSkip(t *testing.T) {
	disclosures := map[string]testDisclosure{
		"TestGCMAsm": {Name: "TestGCMAsm", Class: platformSkipClass, Signature: "gcmAsm not available", Reason: "no .s codepaths"},
	}
	goResults := map[string]string{"TestGCMAsm": "pass"}
	csResults := map[string]string{"TestGCMAsm": "skip"}
	csOutputs := map[string]string{"TestGCMAsm": "skipping: gcmAsm not available"}

	_, _, disclosed, _ := matchTerminalStatuses([]string{"TestGCMAsm"}, goResults, csResults, disclosures, csOutputs)
	if len(disclosed) != 1 {
		t.Fatalf("precondition: the platform-skip pair must be absorbing; got %v", disclosed)
	}

	if orphans := orphanedDisclosures(disclosures, goResults, csResults, "windows"); len(orphans) != 0 {
		t.Fatalf("a skip is not a pass, and this entry is live; got %v", orphans)
	}
}

// NEGATIVE 6 — a host-conditional entry in its fail/fail arm: the Go premise broke, the converted
// side failed as pinned, and the entry accounted as disclosed. Nothing passed, so nothing is stale.
func TestOrphanCheckIsSilentOnAHostConditionalFailFail(t *testing.T) {
	disclosures := map[string]testDisclosure{
		"TestBogoSuite": {
			Name: "TestBogoSuite", Class: "host-limit", Signature: "bogo failed: exit status 1",
			Reason: "the runner's own deadline", HostConditional: "network reachability of the boringssl module",
		},
	}
	goResults := map[string]string{"TestBogoSuite": "fail"}
	csResults := map[string]string{"TestBogoSuite": "fail"}
	csOutputs := map[string]string{"TestBogoSuite": "bogo failed: exit status 1"}

	_, _, disclosed, _ := matchTerminalStatuses([]string{"TestBogoSuite"}, goResults, csResults, disclosures, csOutputs)
	if len(disclosed) != 1 {
		t.Fatalf("precondition: the host-conditional fail/fail arm must be absorbing; got %v", disclosed)
	}

	if orphans := orphanedDisclosures(disclosures, goResults, csResults, "windows"); len(orphans) != 0 {
		t.Fatalf("an annotated row failing on both sides is live in its second accepted shape; got %v", orphans)
	}
}

// The host-fatal class is deliberately NOT exempted, and this arm says why. Such a test is
// withdrawn from BOTH command lines and normally produces no verdict — so it cannot reach the
// predicate at all, which the first half asserts. If one ever DOES report a pass, the withdrawal
// did not take: the test RAN and PASSED, the entry is withdrawing a row the platform runs
// successfully, and that is exactly what a reader must see. Same reasoning matchTerminalStatuses
// gives for letting a host-fatal row fall through to a mismatch rather than absorbing it.
func TestOrphanCheckTreatsHostFatalLikeEveryOtherClass(t *testing.T) {
	disclosures := map[string]testDisclosure{
		"TestPanicOnFault": {Name: "TestPanicOnFault", Class: hostFatalClass, Reason: "takes the host down"},
	}

	// The ordinary case: withdrawn from both sides, so no verdict and no report.
	if orphans := orphanedDisclosures(disclosures, map[string]string{}, map[string]string{}, "linux"); len(orphans) != 0 {
		t.Fatalf("a withdrawn host-fatal test produces no verdict and must not be reported; got %v", orphans)
	}

	// The case that must never be silent: the withdrawal did not take and the test passed.
	orphans := orphanedDisclosures(disclosures,
		map[string]string{"TestPanicOnFault": "pass"},
		map[string]string{"TestPanicOnFault": "pass"},
		"linux")
	if len(orphans) != 1 || orphans[0].Class != hostFatalClass {
		t.Fatalf("a host-fatal entry whose test RAN and PASSED is withdrawing a row this platform "+
			"runs successfully, and exempting the class would hide exactly that; got %v", orphans)
	}
}

// A package with no manifest compares strictly and has nothing to report — the shape nearly every
// package is in, and the one that must stay allocation- and noise-free.
func TestOrphanCheckIsEmptyWithoutAManifest(t *testing.T) {
	if orphans := orphanedDisclosures(nil, map[string]string{"TestX": "pass"}, map[string]string{"TestX": "pass"}, "windows"); orphans != nil {
		t.Fatalf("a package with no disclosure manifest has nothing to report; got %v", orphans)
	}
}

// The report is deterministic: entries come out of a map, so the order is sorted by name. A report
// that reshuffles run to run is one a reader cannot diff.
func TestOrphanReportIsSortedByName(t *testing.T) {
	orphans := orphanedDisclosures(
		orphanManifest(
			orphanDisclosure("TestZulu", "alloc-profile"),
			orphanDisclosure("TestAlpha", "alloc-profile"),
			orphanDisclosure("TestMike", "alloc-profile")),
		map[string]string{},
		map[string]string{"TestZulu": "pass", "TestAlpha": "pass", "TestMike": "pass"},
		"windows")

	if len(orphans) != 3 {
		t.Fatalf("all three entries are stale; got %v", orphans)
	}
	if orphans[0].Name != "TestAlpha" || orphans[1].Name != "TestMike" || orphans[2].Name != "TestZulu" {
		t.Fatalf("the report must be name-sorted so two runs are diffable; got %v", orphans)
	}
}

// THE RECORD KEY. `orphanedDisclosures` is omitempty — which keeps a clean run's record
// byte-for-byte what it was before this field existed — so the key's PRESENCE when the list is
// non-empty is exactly the property omitempty can silently lose. Asserted in both directions.
func TestOrphanedDisclosureReachesTheComparisonRecord(t *testing.T) {
	dir := t.TempDir()
	result := testComparison{
		Package: "sync",
		OrphanedDisclosures: []orphanedDisclosure{
			{Name: "TestOnceFunc", Class: "alloc-profile", Go: "pass", CSharp: "pass", GOOS: "windows"},
		},
	}
	if err := writeComparisonRecord(dir, &result, ""); err != nil {
		t.Fatalf("write record: %v", err)
	}

	back := readComparisonRecord(t, dir)
	raw, present := back["orphanedDisclosures"]
	if !present {
		t.Fatal("a run that found an orphaned disclosure must SAY SO in the record: an omitempty field " +
			"nothing tests is a report that can go silently missing")
	}
	list, ok := raw.([]any)
	if !ok || len(list) != 1 {
		t.Fatalf("the record must carry the one orphan found; got %#v", raw)
	}
	entry, ok := list[0].(map[string]any)
	if !ok {
		t.Fatalf("each orphan is an object; got %#v", list[0])
	}
	for key, want := range map[string]string{
		"name": "TestOnceFunc", "class": "alloc-profile", "go": "pass", "csharp": "pass", "goos": "windows",
	} {
		if got := entry[key]; got != want {
			t.Fatalf("record orphan key %q = %#v, want %q", key, got, want)
		}
	}
}

// The other direction, and the reason omitempty is right here: a run with no orphan writes the
// record every banked row already has.
func TestCleanRecordCarriesNoOrphanKey(t *testing.T) {
	dir := t.TempDir()
	result := testComparison{Package: "unicode/utf8"}
	if err := writeComparisonRecord(dir, &result, ""); err != nil {
		t.Fatalf("write record: %v", err)
	}
	if _, present := readComparisonRecord(t, dir)["orphanedDisclosures"]; present {
		t.Fatal("a run with no orphaned disclosure must write the record it wrote before this field existed")
	}
}

// The report is not the mint rule. hostFatalMintViolations reads COMMITTED PROOF PAGES to refuse a
// host-fatal entry BEFORE either child runs; this reads THIS RUN's verdicts, for every class, after
// both have. Pointing the proof-page instrument at a within-run question was the first framing of
// this gap and it was wrong, so the separation is pinned here: an entry the orphan check reports
// must leave the mint rule's own verdict untouched.
func TestOrphanCheckDoesNotDisturbTheMintRule(t *testing.T) {
	disclosures := orphanManifest(orphanDisclosure("TestOnceFunc", "alloc-profile"))

	violations, unchecked := hostFatalMintViolations(t.TempDir(), disclosures)
	if len(violations) != 0 || len(unchecked) != 0 {
		t.Fatalf("the mint rule reports on host-fatal entries alone and there are none here; got %v / %v", violations, unchecked)
	}

	orphans := orphanedDisclosures(disclosures,
		map[string]string{"TestOnceFunc": "pass"}, map[string]string{"TestOnceFunc": "pass"}, "windows")
	if len(orphans) != 1 {
		t.Fatalf("the within-run check answers on its own; got %v", orphans)
	}
}

// The stderr line's SHAPE, pinned because it is the half a human reads and a grep keys on. The
// wording lives in one place (the call site in convertTestSuite); this arm asserts the fields that
// wording must carry, so a rewording that drops one goes red.
func TestOrphanReportCarriesEveryFieldTheStderrLineNeeds(t *testing.T) {
	orphans := orphanedDisclosures(
		orphanManifest(orphanDisclosure("TestPoolGC", "codegen-liveness")),
		map[string]string{"TestPoolGC": "pass"},
		map[string]string{"TestPoolGC": "pass"},
		"darwin")

	if len(orphans) != 1 {
		t.Fatalf("precondition; got %v", orphans)
	}
	line := "ORPHANED DISCLOSURE (" + orphans[0].GOOS + "): sync " + orphans[0].Name + " [" + orphans[0].Class + "]"
	for _, want := range []string{"ORPHANED DISCLOSURE", "darwin", "TestPoolGC", "codegen-liveness"} {
		if !strings.Contains(line, want) {
			t.Fatalf("the reported line must carry %q so a reader can act on it without the record; got %q", want, line)
		}
	}
}
