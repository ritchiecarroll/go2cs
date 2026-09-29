// disclosureHalves_test.go - Gbtc
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

// THE TWO-HALF SHAPE (ruled 2026-09-28 09:47, ledger 435557647c, item 3b; per-half platforms added at
// 17:56, ledger a4dbed7620). One test can fail for two reasons that deserve two classes: runtime's
// TestReadMetricsConsistency logs "found no time spent on GC work" (the mark CPU classes, which the
// linux-only check reads, runtime-capability under the 09-27 ruling) and "scannable GC space is
// empty: 0" (the scan bytes, structural, on every target). A manifest entry had one class and one
// signature, a second entry of the same name is refused, and the signature is a substring of the
// test's whole log, so pinning either line alone would absorb any change in the other.
//
// An entry may instead carry `halves`, which replace its top-level class and signature. Each half
// carries its own class, signature, reason and retirement fields, and may be scoped to platforms of
// its own. EVERY in-scope half must match, each against a DISTINCT record when the converted host's
// record list is present; the entry's record-count pin covers the whole test; the proof page reports
// each half.
//
// Also here, the ruling's first tooling arm (3a): runtime-capability admits the pass/skip shape, its
// signature read from the skip output, WITHOUT losing the pass/fail shape it already admits.

const (
	gcWorkSignature = "found no time spent on GC work"
	scanSignature   = "scannable GC space is empty: 0"
)

var gcWorkRecord = gcWorkSignature + ": struct { gcAssist float64; gcDedicated float64; gcIdle float64 }{gcAssist:0, gcDedicated:0, gcIdle:0}"

// consistencyEntry is TestReadMetricsConsistency's two halves, the mark-class half scoped to linux.
const consistencyEntry = `{"name":"TestReadMetricsConsistency","halves":[` +
	`{"class":"runtime-capability","signature":"` + gcWorkSignature + `","reason":"the mark CPU classes","platforms":["linux"]},` +
	`{"class":"structural","signature":"` + scanSignature + `","reason":"the scan bytes"}]}`

func rawManifest(t *testing.T, entries ...string) string {
	t.Helper()

	dir := t.TempDir()
	body := `{"schemaVersion":1,"disclosures":[` + strings.Join(entries, ",") + `]}`
	if err := os.WriteFile(filepath.Join(dir, testDisclosureFileName), []byte(body), 0o644); err != nil {
		t.Fatal(err)
	}

	return dir
}

func loadRaw(t *testing.T, goos string, entries ...string) map[string]testDisclosure {
	t.Helper()

	disclosures, _, _, err := loadTestDisclosures(rawManifest(t, entries...), goos)
	if err != nil {
		t.Fatalf("the manifest must load: %v", err)
	}

	return disclosures
}

// matchOne runs the pin-aware matcher over one test row with the given C# status and records.
func matchOne(disclosures map[string]testDisclosure, name, csStatus string, records []string, present bool) (mismatches, disclosed []string) {
	goResults := map[string]string{name: "pass"}
	csResults := map[string]string{name: csStatus}
	csOutputs := map[string]string{name: strings.Join(records, "\r\n")}
	csRecords := map[string]testRecords{name: {Present: present, Records: records}}
	mismatches, _, disclosed, _ = matchTerminalStatusesWithRecords([]string{name}, goResults, csResults, disclosures, csOutputs, csRecords)
	return mismatches, disclosed
}

func TestAHalvesEntryLoads(t *testing.T) {
	if _, present := loadRaw(t, "linux", consistencyEntry)["TestReadMetricsConsistency"]; !present {
		t.Fatal("a two-half entry is in scope on linux")
	}
}

func TestEveryHalfMatchingADistinctRecordIsDisclosed(t *testing.T) {
	mismatches, disclosed := matchOne(loadRaw(t, "linux", consistencyEntry), "TestReadMetricsConsistency", "fail",
		[]string{gcWorkRecord, scanSignature}, true)

	if len(mismatches) != 0 || len(disclosed) != 1 {
		t.Fatalf("both halves match their own record; mismatches %q disclosed %q", mismatches, disclosed)
	}
}

func TestAMissingHalfIsAMismatchThatNamesIt(t *testing.T) {
	mismatches, disclosed := matchOne(loadRaw(t, "linux", consistencyEntry), "TestReadMetricsConsistency", "fail",
		[]string{scanSignature}, true)

	if len(disclosed) != 0 || len(mismatches) != 1 {
		t.Fatalf("a missing half fails the row; mismatches %q disclosed %q", mismatches, disclosed)
	}
	if !strings.Contains(mismatches[0], gcWorkSignature) {
		t.Fatalf("the mismatch names the half that did not match; got %q", mismatches[0])
	}
}

func TestTwoHalvesCannotShareOneRecord(t *testing.T) {
	// Both signatures inside ONE record: each half needs a record of its own.
	mismatches, disclosed := matchOne(loadRaw(t, "linux", consistencyEntry), "TestReadMetricsConsistency", "fail",
		[]string{gcWorkRecord + "; " + scanSignature}, true)

	if len(disclosed) != 0 || len(mismatches) != 1 {
		t.Fatalf("two halves matched in one record must fail; mismatches %q disclosed %q", mismatches, disclosed)
	}
}

func TestWithoutARecordListEachHalfMatchesTheWholeLog(t *testing.T) {
	// A host that carries no record list (the legacy event): each half's signature must still appear.
	disclosures := loadRaw(t, "linux", consistencyEntry)

	if _, disclosed := matchOne(disclosures, "TestReadMetricsConsistency", "fail", []string{gcWorkRecord, scanSignature}, false); len(disclosed) != 1 {
		t.Fatalf("both signatures in the log must disclose; disclosed %q", disclosed)
	}
	if mismatches, _ := matchOne(disclosures, "TestReadMetricsConsistency", "fail", []string{scanSignature}, false); len(mismatches) != 1 {
		t.Fatalf("one signature missing from the log must fail; mismatches %q", mismatches)
	}
}

func TestAHalfScopedToAnotherPlatformIsNotRequired(t *testing.T) {
	// windows: the mark-class check is linux-only in Go's source, so only the scan half applies.
	disclosures := loadRaw(t, "windows", consistencyEntry)

	mismatches, disclosed := matchOne(disclosures, "TestReadMetricsConsistency", "fail", []string{scanSignature}, true)
	if len(mismatches) != 0 || len(disclosed) != 1 {
		t.Fatalf("the in-scope half alone discloses on windows; mismatches %q disclosed %q", mismatches, disclosed)
	}
}

func TestAnEntryWithNoHalfInScopeIsOutOfScope(t *testing.T) {
	entry := `{"name":"TestX","halves":[` +
		`{"class":"structural","signature":"a","reason":"r","platforms":["darwin"]},` +
		`{"class":"structural","signature":"b","reason":"r","platforms":["darwin"]}]}`

	disclosures, outOfScope, _, err := loadTestDisclosures(rawManifest(t, entry), "linux")
	if err != nil {
		t.Fatal(err)
	}
	if _, present := disclosures["TestX"]; present || len(outOfScope) != 1 || outOfScope[0].Name != "TestX" {
		t.Fatalf("an entry none of whose halves apply is out of scope, and listed; in scope %v, out of scope %+v", disclosures, outOfScope)
	}
}

func TestTheRecordPinCoversTheWholeTest(t *testing.T) {
	pinned := strings.Replace(consistencyEntry, `{"name":"TestReadMetricsConsistency",`, `{"name":"TestReadMetricsConsistency","records":2,`, 1)
	disclosures := loadRaw(t, "linux", pinned)

	if _, disclosed := matchOne(disclosures, "TestReadMetricsConsistency", "fail", []string{gcWorkRecord, scanSignature}, true); len(disclosed) != 1 {
		t.Fatalf("the minted count discloses; disclosed %q", disclosed)
	}

	mismatches, _ := matchOne(disclosures, "TestReadMetricsConsistency", "fail", []string{gcWorkRecord, "gomaxprocs doesn't match", scanSignature}, true)
	if len(mismatches) != 1 || !strings.Contains(mismatches[0], "gomaxprocs doesn't match") {
		t.Fatalf("a planted third record fails the row and is named; got %q", mismatches)
	}
	if strings.Contains(mismatches[0], "struct {") || strings.Contains(mismatches[0], `"`+scanSignature+`"`) {
		t.Fatalf("the records the halves matched are not named as outside the signature; got %q", mismatches[0])
	}
}

func TestTheProofPageReportsEachHalf(t *testing.T) {
	comparison := testComparison{
		Package: "runtime", Status: "validated",
		Go:     map[string]string{"TestReadMetricsConsistency": "pass"},
		CSharp: map[string]string{"TestReadMetricsConsistency": "fail"},
	}

	page := renderValidationProofPage(proofPageProvenance{importPath: "runtime", goVersion: "1.24.13", platform: "linux/amd64", date: "2026-09-28"},
		comparison, loadRaw(t, "linux", consistencyEntry), nil)

	for _, row := range []string{"| `runtime-capability` | the mark CPU classes |", "| `structural` | the scan bytes |"} {
		if !strings.Contains(page, row) {
			t.Fatalf("the page reports each half, missing %q:\n%s", row, page)
		}
	}
}

func TestTheHalvesLoaderRefusals(t *testing.T) {
	half := func(class, extra string) string {
		return `{"class":"` + class + `","signature":"s-` + class + `","reason":"r"` + extra + `}`
	}
	entry := func(top, halves string) string {
		return `{"name":"TestX"` + top + `,"halves":[` + halves + `]}`
	}

	cases := []struct {
		name, entry, want string
	}{
		{"a top-level class", entry(`,"class":"structural"`, half("structural", "")+","+half("deferred", `,"want":"w","reading":"r","plan":"p"`)), "halves replace"},
		{"a top-level signature", entry(`,"signature":"s"`, half("structural", "")+","+half("structural", "")), "halves replace"},
		{"a top-level retirement field", entry(`,"plan":"p"`, half("structural", "")+","+half("structural", "")), "halves replace"},
		{"one half", entry("", half("structural", "")), "at least two halves"},
		{"a host-fatal half", entry("", half("structural", "")+","+half("host-fatal", "")), "host-fatal"},
		{"a host-conditional entry", entry(`,"hostConditional":"network"`, half("structural", "")+","+half("structural", "")), "host-conditional"},
		{"a half without a signature", entry("", half("structural", "")+`,{"class":"structural","reason":"r"}`), "half 2"},
		{"a half without a reason", entry("", half("structural", "")+`,{"class":"structural","signature":"s"}`), "half 2"},
		{"a deferred half without its plan", entry("", half("structural", "")+","+half("deferred", `,"want":"w","reading":"r"`)), "plan"},
		{"a structural half with a plan", entry("", half("structural", `,"plan":"p"`)+","+half("structural", "")), "plan"},
		{"a half naming an unknown platform", entry("", half("structural", `,"platforms":["plan9"]`)+","+half("structural", "")), "plan9"},
		{"halves admitting no common shape", entry("", half("platform-skip", "")+","+half("structural", "")), "shape"},
		{"a compiler-property half", entry("", half("compiler-property", "")+","+half("platform-skip", "")), "compiler-property"},
	}

	for _, c := range cases {
		dir := rawManifest(t, c.entry)
		_, _, err := readTestDisclosureManifest(dir)
		if err == nil || !strings.Contains(err.Error(), c.want) || !strings.Contains(err.Error(), "TestX") {
			t.Errorf("%s: the loader must refuse it, naming the entry and %q; got %v", c.name, c.want, err)
		}
	}
}

// ---- the first tooling arm: runtime-capability in the pass/skip shape ----------------------------

const recursionSkip = "skipping: the managed host's CPU profile carries no recursive frames"

func runtimeCapabilityEntry() string {
	return `{"name":"TestCPUProfileRecursion","class":"runtime-capability","signature":"` + recursionSkip + `","reason":"no recursive frames"}`
}

func TestRuntimeCapabilityAdmitsTheSkipShape(t *testing.T) {
	mismatches, disclosed := matchOne(loadRaw(t, "linux", runtimeCapabilityEntry()), "TestCPUProfileRecursion", "skip",
		[]string{recursionSkip}, true)

	if len(mismatches) != 0 || len(disclosed) != 1 {
		t.Fatalf("a runtime-capability skip with its signature in the skip output is disclosed; mismatches %q disclosed %q", mismatches, disclosed)
	}
}

func TestARuntimeCapabilitySkipForAnotherReasonIsAMismatch(t *testing.T) {
	mismatches, disclosed := matchOne(loadRaw(t, "linux", runtimeCapabilityEntry()), "TestCPUProfileRecursion", "skip",
		[]string{"skipping: some other reason"}, true)

	if len(disclosed) != 0 || len(mismatches) != 1 {
		t.Fatalf("a skip without the signature moved; mismatches %q disclosed %q", mismatches, disclosed)
	}
}

func TestRuntimeCapabilityStillAdmitsTheFailShape(t *testing.T) {
	// A GUARD, green before and after: the fail arm's exclusion reads the skip-shape predicate, so
	// admitting runtime-capability to the skip shape must not drop TestCaller and its siblings.
	entry := `{"name":"TestCaller","class":"runtime-capability","signature":"incorrect symbol info","reason":"opaque PC tokens"}`

	mismatches, disclosed := matchOne(loadRaw(t, "linux", entry), "TestCaller", "fail", []string{"incorrect symbol info"}, true)
	if len(mismatches) != 0 || len(disclosed) != 1 {
		t.Fatalf("runtime-capability keeps the pass/fail shape; mismatches %q disclosed %q", mismatches, disclosed)
	}
}

func TestASkipOnlyClassStillRefusesTheFailShape(t *testing.T) {
	// A GUARD, green before and after: platform-skip admits the skip shape alone, so a failure that
	// happens to carry its skip message is a mismatch.
	entry := `{"name":"TestGCMAsm","class":"platform-skip","signature":"no assembly","reason":"no .s codepaths"}`

	mismatches, disclosed := matchOne(loadRaw(t, "linux", entry), "TestGCMAsm", "fail", []string{"no assembly"}, true)
	if len(disclosed) != 0 || len(mismatches) != 1 {
		t.Fatalf("a skip-only class never discloses a failure; mismatches %q disclosed %q", mismatches, disclosed)
	}
}

func TestARuntimeCapabilitySkipIsStatedOnTheProofPage(t *testing.T) {
	// The anti-laundering clause platform-skip and compiler-property already honor: Go ran the test and
	// the converted side skipped, so the page must not claim no test was skipped, and says so in words.
	comparison := testComparison{
		Package: "runtime/pprof", Status: "validated",
		Go:     map[string]string{"TestCPUProfileRecursion": "pass", "TestOther": "pass"},
		CSharp: map[string]string{"TestCPUProfileRecursion": "skip", "TestOther": "pass"},
	}

	page := renderValidationProofPage(proofPageProvenance{importPath: "runtime/pprof", goVersion: "1.24.13", platform: "linux/amd64", date: "2026-09-28"},
		comparison, loadRaw(t, "linux", runtimeCapabilityEntry()), nil)

	if strings.Contains(page, "not\na skipped test") {
		t.Fatalf("a page disclosing a runtime-capability skip must not claim no test was skipped:\n%s", page)
	}
	if !strings.Contains(page, "`TestCPUProfileRecursion` is a **runtime-capability skip**") {
		t.Fatalf("the page names the row as a runtime-capability skip in words:\n%s", page)
	}
}
