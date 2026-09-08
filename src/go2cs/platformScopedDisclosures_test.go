// platformScopedDisclosures_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

package main

import (
	"os"
	"os/exec"
	"path/filepath"
	"strings"
	"testing"
)

// PLATFORM-SCOPED DISCLOSURE ENTRIES (increment 2 of the orphan-disclosure check).
//
// A per-package disclosure manifest is ONE file shared by every platform, so REMOVING an entry is a
// cross-platform edit — the first Linux annotation refresh to remove one turned the Windows row red
// on the next union battery. Until the optional `platforms` list existed there were only two states,
// present everywhere and absent everywhere, and a per-platform retirement could not be expressed at
// all. That is why increment 3 (refusing an orphan) is gated on this one: a refusal needs somewhere
// for the refused entry to GO.
//
// The distinction every arm below turns on: an OUT-OF-SCOPE entry is INERT, not stale. Inert means
// the run never applied it and has no evidence about it in either direction. Stale means this
// platform ran the test and it PASSED. Reporting the first as the second would be the same inversion
// the terminal-pass predicate already refuses one level down.
//
// See docs/phase4/DESIGN-orphan-disclosure-check.md §8.

// scopedManifest writes a manifest carrying one entry with the given platforms clause and returns
// its directory. The clause is spliced as raw JSON so an arm can plant a MALFORMED one (an unknown
// GOOS, a duplicate) that a []string parameter could not express.
func scopedManifest(t *testing.T, name, class, platformsClause string) string {
	t.Helper()

	dir := t.TempDir()
	entry := `{"name":"` + name + `","class":"` + class + `","signature":"the pinned text","reason":"because ` + name + `"`
	if platformsClause != "" {
		entry += `,"platforms":` + platformsClause
	}
	entry += `}`

	body := `{"schemaVersion":1,"disclosures":[` + entry + `]}`
	if err := os.WriteFile(filepath.Join(dir, testDisclosureFileName), []byte(body), 0o644); err != nil {
		t.Fatal(err)
	}

	return dir
}

// THE WHITELIST ITSELF, and the one caveat that comes with typing it goosScope: an EMPTY goosScope
// means "every target", so an emptied list would silently turn the validator into accept-anything.
// Pinned rather than remembered.
func TestDisclosurePlatformTargetsAreTheCorpusTargets(t *testing.T) {
	if len(disclosurePlatformTargets) == 0 {
		t.Fatal("an empty goosScope INCLUDES every target, so an emptied whitelist would accept any " +
			"string as a GOOS and disable the refusal entirely")
	}
	for _, want := range []string{"windows", "linux", "darwin"} {
		if !disclosurePlatformTargets.includes(want) {
			t.Fatalf("the corpus builds %s (layout L3), so a disclosure must be scopable to it", want)
		}
	}
	if len(disclosurePlatformTargets) != 3 {
		t.Fatalf("the whitelist is the corpus's three L3 targets; a fourth needs this arm updated "+
			"deliberately rather than silently, got %v", disclosurePlatformTargets)
	}
}

// POSITIVE ARM 1 — an entry scoped to ANOTHER platform absorbs NOTHING here: the row it names reads
// as a plain mismatch, exactly as it would with no manifest at all. Measured through
// matchTerminalStatuses rather than asserted about the map, because "absorbs nothing" is a claim
// about the comparison, not about a data structure.
//
// The precondition is what makes it a measurement: the SAME entry unscoped absorbs the SAME row, so
// the only thing separating the two readings is the scope.
func TestEntryScopedToAnotherPlatformAbsorbsNothing(t *testing.T) {
	names := []string{"TestSendmsgN"}
	goResults := map[string]string{"TestSendmsgN": "pass"}
	csResults := map[string]string{"TestSendmsgN": "fail"}
	csOutputs := map[string]string{"TestSendmsgN": "got 2 allocs, the pinned text, want 0"}

	// Precondition: unscoped, the entry absorbs.
	unscoped, _, _, err := loadTestDisclosures(scopedManifest(t, "TestSendmsgN", "alloc-profile", ""), "windows")
	if err != nil {
		t.Fatalf("load unscoped: %v", err)
	}
	_, _, disclosed, _ := matchTerminalStatuses(names, goResults, csResults, unscoped, csOutputs)
	if len(disclosed) != 1 {
		t.Fatalf("precondition: the unscoped entry must absorb this row; got %v", disclosed)
	}

	// Scoped to linux, running windows: inert.
	scoped, outOfScope, _, err := loadTestDisclosures(scopedManifest(t, "TestSendmsgN", "alloc-profile", `["linux"]`), "windows")
	if err != nil {
		t.Fatalf("load scoped: %v", err)
	}
	if len(scoped) != 0 {
		t.Fatalf("an entry scoped to another platform must not reach any consumer; got %v", scoped)
	}

	mismatches, _, disclosed, _ := matchTerminalStatuses(names, goResults, csResults, scoped, csOutputs)
	if len(disclosed) != 0 {
		t.Fatalf("a linux-scoped entry must absorb nothing on windows; got disclosed=%v", disclosed)
	}
	if len(mismatches) != 1 {
		t.Fatalf("the row must read as a plain mismatch, exactly as with no manifest at all; got %v", mismatches)
	}
	if len(outOfScope) != 1 || outOfScope[0].Name != "TestSendmsgN" {
		t.Fatalf("the entry must still be PUBLISHED as out of scope, or a scoped manifest is silently "+
			"smaller than the file on disk; got %v", outOfScope)
	}
}

// POSITIVE ARM 2 — an entry scoped to THIS platform behaves exactly as an unscoped one. The scope is
// a filter, never a second kind of disclosure: once in scope, every downstream rule is untouched.
func TestEntryScopedToThisPlatformAbsorbsExactlyAsUnscoped(t *testing.T) {
	names := []string{"TestSendmsgN"}
	goResults := map[string]string{"TestSendmsgN": "pass"}
	csResults := map[string]string{"TestSendmsgN": "fail"}
	csOutputs := map[string]string{"TestSendmsgN": "got 2 allocs, the pinned text, want 0"}

	for _, clause := range []string{`["linux"]`, `["windows","linux","darwin"]`} {
		entries, outOfScope, _, err := loadTestDisclosures(scopedManifest(t, "TestSendmsgN", "alloc-profile", clause), "linux")
		if err != nil {
			t.Fatalf("load %s: %v", clause, err)
		}
		if len(entries) != 1 {
			t.Fatalf("%s includes linux, so the entry is in scope; got %v", clause, entries)
		}
		if len(outOfScope) != 0 {
			t.Fatalf("%s is in scope and must not be listed out of scope; got %v", clause, outOfScope)
		}

		mismatches, _, disclosed, _ := matchTerminalStatuses(names, goResults, csResults, entries, csOutputs)
		if len(disclosed) != 1 || len(mismatches) != 0 {
			t.Fatalf("%s: an in-scope entry absorbs exactly as an unscoped one; got disclosed=%v mismatches=%v",
				clause, disclosed, mismatches)
		}
	}
}

// NEGATIVE 1 — the state every committed manifest is in. No `platforms` key at all: in scope on
// every target, and nothing published out of scope. This is the arm that says the field is ADDITIVE.
func TestUnscopedEntryAppliesOnEveryPlatform(t *testing.T) {
	dir := scopedManifest(t, "TestOnceFunc", "alloc-profile", "")

	for _, goos := range []string{"windows", "linux", "darwin"} {
		entries, outOfScope, _, err := loadTestDisclosures(dir, goos)
		if err != nil {
			t.Fatalf("%s: %v", goos, err)
		}
		if len(entries) != 1 || len(outOfScope) != 0 {
			t.Fatalf("an entry with no scope applies everywhere, exactly as before this field existed; "+
				"%s got entries=%v outOfScope=%v", goos, entries, outOfScope)
		}
	}
}

// NEGATIVE 2 — an EXPLICIT empty list reads as "every platform" too, which is goosScope's documented
// zero value. Pinned because the natural alternative reading — "no platform", i.e. inert everywhere —
// is a silent way to disable an entry, and the two are one character apart in a manifest.
func TestEmptyPlatformListMeansEveryPlatform(t *testing.T) {
	entries, outOfScope, _, err := loadTestDisclosures(scopedManifest(t, "TestOnceFunc", "alloc-profile", `[]`), "darwin")
	if err != nil {
		t.Fatalf("an empty platforms list is legal and means every platform: %v", err)
	}
	if len(entries) != 1 || len(outOfScope) != 0 {
		t.Fatalf("an empty list must not read as 'no platform', which would disable the entry silently; "+
			"got entries=%v outOfScope=%v", entries, outOfScope)
	}
}

// REFUSAL 1 — an unknown GOOS is refused AT LOAD, by name. This is the arm that earns the narrow
// whitelist: a misspelling would otherwise produce an entry out of scope on every run, forever,
// absorbing nothing and saying nothing.
func TestManifestRefusesAnUnknownPlatform(t *testing.T) {
	for _, bad := range []string{`["windwos"]`, `["freebsd"]`, `["linux","plan9"]`, `[""]`} {
		err := readTestDisclosureManifestErr(t, scopedManifest(t, "TestX", "alloc-profile", bad))
		if err == nil {
			t.Fatalf("%s names a target this corpus does not build; accepting it would disable the "+
				"entry's absorption silently and forever", bad)
		}
		// The message must name the offending value AND the accepted set: a refusal that says only
		// "invalid" sends the reader to the schema instead of to the typo.
		if !strings.Contains(err.Error(), "windows, linux, darwin") {
			t.Fatalf("the refusal must name the accepted targets; got %v", err)
		}
	}
}

// REFUSAL 2 — a duplicate platform. It changes NOTHING about the scope, which is exactly why it is
// refused: it is evidence the list was edited without being read.
func TestManifestRefusesADuplicatePlatform(t *testing.T) {
	err := readTestDisclosureManifestErr(t, scopedManifest(t, "TestX", "alloc-profile", `["linux","linux"]`))
	if err == nil {
		t.Fatal("a duplicate platform must be refused, not silently deduplicated")
	}
	if !strings.Contains(err.Error(), "twice") {
		t.Fatalf("the refusal must say what is wrong with the list; got %v", err)
	}
}

// REFUSAL 3 — the AMBIGUOUS case, and the reason it is an error rather than a default. A scoped
// entry with no run platform cannot be applied or skipped: treating it as in scope widens the oracle,
// treating it as out of scope disables an absorption, and neither leaves a message. Both directions
// are asserted, because the second half is what keeps the refusal from reaching existing manifests.
func TestScopedEntryWithNoTargetPlatformIsRefused(t *testing.T) {
	if _, _, _, err := loadTestDisclosures(scopedManifest(t, "TestX", "alloc-profile", `["linux"]`), ""); err == nil {
		t.Fatal("a scoped entry with no target platform is a question the entry cannot answer; " +
			"guessing either way changes the oracle silently")
	}

	// The other direction: a manifest with NO scoped entry is unaffected by an absent GOOS, which is
	// what keeps all 46 committed manifests and every existing fixture loading exactly as before.
	entries, outOfScope, _, err := loadTestDisclosures(scopedManifest(t, "TestX", "alloc-profile", ""), "")
	if err != nil || len(entries) != 1 || len(outOfScope) != 0 {
		t.Fatalf("an unscoped manifest asks nothing of the platform and must load under any GOOS, "+
			"including none; got entries=%v outOfScope=%v err=%v", entries, outOfScope, err)
	}
}

// THE INERT/STALE DISTINCTION, measured. An out-of-scope entry whose test PASSES here is exactly the
// shape that would be reported as an orphan if the scope were ignored — and it must NOT be, because
// this run said nothing about that entry. It must still be visible, as out-of-scope.
//
// This is the arm increment 3 rests on: a refusal that could not tell these apart would refuse
// entries on the evidence of a platform that never applied them.
func TestOutOfScopeEntryIsNotAnOrphanButIsListed(t *testing.T) {
	entries, outOfScope, _, err := loadTestDisclosures(
		scopedManifest(t, "TestSendmsgN", "alloc-profile", `["linux"]`), "windows")
	if err != nil {
		t.Fatalf("load: %v", err)
	}

	// Both sides pass — the pass/pass shape the orphan check reports for an IN-SCOPE entry.
	orphans := orphanedDisclosures(entries,
		map[string]string{"TestSendmsgN": "pass"},
		map[string]string{"TestSendmsgN": "pass"},
		"windows")
	if len(orphans) != 0 {
		t.Fatalf("an entry this run never applied is INERT, not stale: reporting it would accuse a "+
			"linux entry on the strength of a windows run, which is the cross-platform removal "+
			"doctrine rule (1) was written after; got %v", orphans)
	}

	if len(outOfScope) != 1 {
		t.Fatalf("inert is not invisible: the entry must be published so a reader can see it exists; got %v", outOfScope)
	}
	got := outOfScope[0]
	if got.Name != "TestSendmsgN" || got.Class != "alloc-profile" {
		t.Fatalf("the out-of-scope record must carry the entry's own name and class; got %+v", got)
	}
	if len(got.Platforms) != 1 || got.Platforms[0] != "linux" {
		t.Fatalf("it must carry the scope, or a reader cannot tell WHICH platform it is for; got %+v", got)
	}
	if got.GOOS != "windows" {
		t.Fatalf("it must name the platform that excluded it, so the element is self-describing "+
			"when quoted out of the record; got %+v", got)
	}
}

// WHY THE FILTER IS AT LOAD, measured rather than argued. A host-fatal entry WITHDRAWS its test from
// both command lines before either child runs, so an out-of-scope one filtered any later would
// already have changed what the run contains — a linux-only host-killer would silently stop windows
// from running a test it passes.
func TestOutOfScopeHostFatalDoesNotWithdrawItsTest(t *testing.T) {
	// A host-fatal entry needs no signature (its test produces no verdict to pin).
	dir := t.TempDir()
	body := `{"schemaVersion":1,"disclosures":[{"name":"TestPanicOnFault","class":"` + hostFatalClass +
		`","reason":"takes the host down on linux","platforms":["linux"]}]}`
	if err := os.WriteFile(filepath.Join(dir, testDisclosureFileName), []byte(body), 0o644); err != nil {
		t.Fatal(err)
	}

	// Precondition: in scope, it withdraws.
	onLinux, _, _, err := loadTestDisclosures(dir, "linux")
	if err != nil {
		t.Fatalf("load linux: %v", err)
	}
	if skip := hostFatalSkipExpression(onLinux); !strings.Contains(skip, "TestPanicOnFault") {
		t.Fatalf("precondition: an in-scope host-fatal entry must withdraw its test; got %q", skip)
	}

	// Out of scope: it withdraws nothing, so windows runs the test it passes.
	onWindows, outOfScope, _, err := loadTestDisclosures(dir, "windows")
	if err != nil {
		t.Fatalf("load windows: %v", err)
	}
	if skip := hostFatalSkipExpression(onWindows); skip != "" {
		t.Fatalf("a linux-scoped host-killer must not withdraw the test on windows: the scope has to "+
			"apply BEFORE the command lines are built, not after; got %q", skip)
	}
	if len(outOfScope) != 1 || outOfScope[0].Class != hostFatalClass {
		t.Fatalf("the withdrawn-nowhere entry is still published; got %v", outOfScope)
	}
}

// Deterministic order, for the same reason the orphan report is sorted: these come out of a map, and
// a list that reshuffles run to run is one a reader cannot diff.
func TestOutOfScopeListIsSortedByName(t *testing.T) {
	dir := t.TempDir()
	body := `{"schemaVersion":1,"disclosures":[
		{"name":"TestZulu","class":"alloc-profile","signature":"s","reason":"r","platforms":["linux"]},
		{"name":"TestAlpha","class":"alloc-profile","signature":"s","reason":"r","platforms":["linux"]},
		{"name":"TestMike","class":"alloc-profile","signature":"s","reason":"r","platforms":["darwin"]}]}`
	if err := os.WriteFile(filepath.Join(dir, testDisclosureFileName), []byte(body), 0o644); err != nil {
		t.Fatal(err)
	}

	_, outOfScope, _, err := loadTestDisclosures(dir, "windows")
	if err != nil {
		t.Fatalf("load: %v", err)
	}
	if len(outOfScope) != 3 {
		t.Fatalf("all three are scoped away from windows; got %v", outOfScope)
	}
	if outOfScope[0].Name != "TestAlpha" || outOfScope[1].Name != "TestMike" || outOfScope[2].Name != "TestZulu" {
		t.Fatalf("the list must be name-sorted so two runs are diffable; got %v", outOfScope)
	}
}

// THE RECORD KEY, both directions. omitempty keeps a clean run's record byte-for-byte what it was
// before this field existed, which is exactly the property omitempty can silently lose.
func TestOutOfScopeDisclosuresReachTheComparisonRecord(t *testing.T) {
	dir := t.TempDir()
	result := testComparison{
		Package: "syscall",
		OutOfScopeDisclosures: []outOfScopeDisclosure{
			{Name: "TestSendmsgN", Class: "alloc-profile", Platforms: goosScope{"linux"}, GOOS: "windows"},
		},
	}
	if err := writeComparisonRecord(dir, &result, ""); err != nil {
		t.Fatalf("write record: %v", err)
	}

	raw, present := readComparisonRecord(t, dir)["outOfScopeDisclosures"]
	if !present {
		t.Fatal("a run that skipped an entry for scope must SAY SO: an omitempty field nothing tests " +
			"is a report that can go silently missing, and a scoped manifest would then be quietly " +
			"smaller than the file on disk")
	}
	list, ok := raw.([]any)
	if !ok || len(list) != 1 {
		t.Fatalf("the record must carry the one out-of-scope entry; got %#v", raw)
	}
	entry, ok := list[0].(map[string]any)
	if !ok {
		t.Fatalf("each out-of-scope entry is an object; got %#v", list[0])
	}
	for key, want := range map[string]string{
		"name": "TestSendmsgN", "class": "alloc-profile", "goos": "windows",
	} {
		if got := entry[key]; got != want {
			t.Fatalf("record key %q = %#v, want %q", key, got, want)
		}
	}
	platforms, ok := entry["platforms"].([]any)
	if !ok || len(platforms) != 1 || platforms[0] != "linux" {
		t.Fatalf("the scope must survive into the record; got %#v", entry["platforms"])
	}
}

// The other direction, and the reason omitempty is right: a run with nothing out of scope — which is
// every run over every committed manifest today — writes the record every banked row already has.
func TestCleanRecordCarriesNoOutOfScopeKey(t *testing.T) {
	dir := t.TempDir()
	result := testComparison{Package: "unicode/utf8"}
	if err := writeComparisonRecord(dir, &result, ""); err != nil {
		t.Fatalf("write record: %v", err)
	}
	if _, present := readComparisonRecord(t, dir)["outOfScopeDisclosures"]; present {
		t.Fatal("a run with nothing out of scope must write the record it wrote before this field existed")
	}
}

// THE COMMITTED CORPUS, and the arm that makes "additive" a measurement rather than a claim. Every
// manifest in the tree must load unchanged on all three targets: same entry count, nothing out of
// scope, no refusal.
//
// CACHE CAVEAT, the same one fleetIdentifierCensus_test.go carries: the files read here live under
// src/core, OUTSIDE this module, and cmd/go drops out-of-module files from the test input hash — so
// a cached PASS would survive a manifest gaining a scope. Run the converter suite with -count=1,
// which every gate in this repo already does.
func TestEveryCommittedManifestLoadsUnchanged(t *testing.T) {
	root := repoRootFromPackageDir(t)

	out, err := exec.Command("git", "-C", root, "ls-files", "-z", "src/core/**/go2cs_test_disclosures.json").Output()
	if err != nil {
		t.Fatalf("git ls-files: %v", err)
	}

	manifests := []string{}
	for _, path := range strings.Split(string(out), "\x00") {
		if strings.TrimSpace(path) != "" {
			manifests = append(manifests, path)
		}
	}

	// Vacuity guard: a wrong pathspec would make every assertion below pass over nothing, which is
	// the shape this whole increment exists to refuse one level up.
	if len(manifests) == 0 {
		t.Fatal("no committed disclosure manifest was found: the pathspec is wrong and this arm is " +
			"asserting nothing")
	}

	totalEntries := 0
	scopedEntries := 0

	for _, rel := range manifests {
		pkgDir := filepath.Join(root, filepath.FromSlash(filepath.Dir(rel)))

		// The unscoped read is the reference: what the loader saw before this increment existed.
		reference, _, err := readTestDisclosureManifest(pkgDir)
		if err != nil {
			t.Fatalf("%s: a committed manifest must load: %v", rel, err)
		}
		totalEntries += len(reference)

		for _, entry := range reference {
			if len(entry.Platforms) > 0 {
				scopedEntries++
			}
		}

		for _, goos := range []string{"windows", "linux", "darwin"} {
			entries, outOfScope, _, err := loadTestDisclosures(pkgDir, goos)
			if err != nil {
				t.Fatalf("%s on %s: %v", rel, goos, err)
			}
			if len(entries) != len(reference) {
				t.Fatalf("%s on %s: scoping changed a committed manifest — %d entries against %d "+
					"unscoped; every count the record and the proof page publish would move with it",
					rel, goos, len(entries), len(reference))
			}
			if len(outOfScope) != 0 {
				t.Fatalf("%s on %s: no committed manifest carries a scope today, so nothing may be "+
					"out of scope; got %v", rel, goos, outOfScope)
			}
		}
	}

	t.Logf("committed manifests: %d, entries: %d, of which scoped: %d (all three targets)",
		len(manifests), totalEntries, scopedEntries)
}

// readTestDisclosureManifestErr is the refusal arms' reader: it returns the error ALONE, so an arm
// asserting a REFUSAL cannot accidentally read a successfully-loaded map as evidence.
func readTestDisclosureManifestErr(t *testing.T, dir string) error {
	t.Helper()
	_, _, err := readTestDisclosureManifest(dir)
	return err
}
