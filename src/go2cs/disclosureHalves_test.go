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
	mismatches, _, disclosed, _ = matchTerminalStatusesWithRecords([]string{name}, goResults, csResults, disclosures, csOutputs, csRecords, nil)
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

// ---- the Go-side host-data skip: goSkipSignature (docs/phase4/DESIGN-comparator-arms.md §2) --------
//
// The one shape pinned on the GO side: Go=skip / C#=pass, admitted only when Go's own skip output
// carries the entry's goSkipSignature. The pin below is test data for the matcher, not the shipped
// entry's pin, which is read from the Go side's own output.

const goSkipPin = "the profiler delivered no sample in the window"

func goSkipEntry() string {
	return `{"name":"TestProfile/Default","class":"host-identity","platforms":["darwin"],"goSkipSignature":"` + goSkipPin + `","signature":"","reason":"the host's profiler delivers no sample"}`
}

// matchGoSkip runs the matcher over one test, and, when child is set, one C#-only subtest under it.
func matchGoSkip(disclosures map[string]testDisclosure, goStatus, csStatus, goOutput string, child bool) (mismatches, disclosed, withdrawn []string) {
	const name = "TestProfile/Default"

	names := []string{name}
	goResults := map[string]string{name: goStatus}
	csResults := map[string]string{name: csStatus}

	if child {
		names = append(names, name+"/case")
		csResults[name+"/case"] = "pass"
	}

	goOutputs := map[string]string{name: goOutput}
	mismatches, _, disclosed, withdrawn = matchTerminalStatusesWithRecords(names, goResults, csResults, disclosures, map[string]string{}, nil, goOutputs)
	return mismatches, disclosed, withdrawn
}

// The red-first table. Without the arm the first row reads as a mismatch; every other row is a guard,
// and the third (Go=fail) is the arm's made-to-fail proof: admitting a Go failure turns it red.
func TestAGoSideHostSkipIsDisclosedOnlyInItsOneShape(t *testing.T) {
	disclosures := loadRaw(t, "darwin", goSkipEntry())
	skipText := "    trace_test.go:121: " + goSkipPin + "\n--- SKIP: TestProfile/Default (0.10s)\n"

	cases := []struct {
		row                string
		goStatus, csStatus string
		goOutput           string
		disclosed          bool
		mismatchContains   string
	}{
		{"Go skips with the pin, C# passes", "skip", "pass", skipText, true, ""},
		{"Go skips for another reason", "skip", "pass", "    x_test.go:9: some other reason\n", false, goSkipPin},
		{"Go FAILS with the pin in its output", "fail", "pass", "    trace_test.go:121: " + goSkipPin + "\n--- FAIL: TestProfile/Default\n", false, `Go="fail"`},
		{"both sides pass", "pass", "pass", "", false, ""},
		{"C# fails where Go skipped", "skip", "fail", skipText, false, `C#="fail"`},
		{"C# skips where Go skipped", "skip", "skip", skipText, false, ""},
		{"C# fails where Go passed (the empty C# signature pins nothing)", "pass", "fail", "", false, `C#="fail"`},
		{"C# skips where Go passed", "pass", "skip", "", false, `C#="skip"`},
	}

	for _, c := range cases {
		mismatches, disclosed, _ := matchGoSkip(disclosures, c.goStatus, c.csStatus, c.goOutput, false)

		if c.disclosed {
			if len(mismatches) != 0 || len(disclosed) != 1 {
				t.Errorf("%s: must be DISCLOSED; mismatches %q disclosed %q", c.row, mismatches, disclosed)
			}
			continue
		}

		if len(disclosed) != 0 {
			t.Errorf("%s: must never be disclosed; disclosed %q", c.row, disclosed)
		}
		if c.mismatchContains == "" {
			if len(mismatches) != 0 {
				t.Errorf("%s: the sides agree, so the entry is inert; mismatches %q", c.row, mismatches)
			}
			continue
		}
		if len(mismatches) != 1 || !strings.Contains(mismatches[0], c.mismatchContains) {
			t.Errorf("%s: must be one strict mismatch naming %q; got %q", c.row, c.mismatchContains, mismatches)
		}
	}
}

func TestAGoSideHostSkipRootsNoWithdrawal(t *testing.T) {
	// No fan-out admission: a C#-only row under the skipped test compares strictly.
	mismatches, disclosed, withdrawn := matchGoSkip(loadRaw(t, "darwin", goSkipEntry()), "skip", "pass", goSkipPin, true)

	if len(disclosed) != 1 || len(withdrawn) != 0 || len(mismatches) != 1 || !strings.HasPrefix(mismatches[0], "TestProfile/Default/case:") {
		t.Fatalf("the parent is disclosed and its C#-only child is a mismatch; mismatches %q disclosed %q withdrawn %q", mismatches, disclosed, withdrawn)
	}
}

func TestAGoSideHostSkipNeedsGoOutput(t *testing.T) {
	// A caller that carries no Go output (matchTerminalStatuses) cannot see the pin, so it never admits.
	disclosures := loadRaw(t, "darwin", goSkipEntry())
	mismatches, _, disclosed, _ := matchTerminalStatuses([]string{"TestProfile/Default"},
		map[string]string{"TestProfile/Default": "skip"}, map[string]string{"TestProfile/Default": "pass"}, disclosures, nil)

	if len(disclosed) != 0 || len(mismatches) != 1 {
		t.Fatalf("no Go output, no admission; mismatches %q disclosed %q", mismatches, disclosed)
	}
}

func TestAPlainHostIdentityEntryStillRefusesAGoSkip(t *testing.T) {
	// A GUARD, green before and after: the shape is unlocked by the field, not by the class.
	entry := `{"name":"TestProfile/Default","class":"host-identity","signature":"` + goSkipPin + `","reason":"r"}`
	mismatches, disclosed, _ := matchGoSkip(loadRaw(t, "darwin", entry), "skip", "pass", goSkipPin, false)

	if len(disclosed) != 0 || len(mismatches) != 1 {
		t.Fatalf("host-identity without goSkipSignature admits no Go skip; mismatches %q disclosed %q", mismatches, disclosed)
	}
}

func TestTheGoSkipSignatureLoaderGuards(t *testing.T) {
	entry := func(fields string) string {
		return `{"name":"TestX","reason":"r"` + fields + `}`
	}
	pin := `,"goSkipSignature":"` + goSkipPin + `"`

	refused := []struct {
		name, entry, want string
	}{
		{"another class", entry(`,"class":"platform-skip","signature":"s"` + pin), "host-identity"},
		{"a fail class", entry(`,"class":"structural","signature":"s"` + pin), "host-identity"},
		{"beside hostConditional", entry(`,"class":"host-identity","signature":"s","hostConditional":"network"` + pin), "hostConditional"},
		{"an empty pin", entry(`,"class":"host-identity","signature":"","goSkipSignature":"  "`), "goSkipSignature"},
		{"a short pin", entry(`,"class":"host-identity","signature":"","goSkipSignature":"skipping test"`), "16"},
		{"an empty signature without the pin", entry(`,"class":"host-identity","signature":""`), "signature"},
		{"on a two-half entry", `{"name":"TestX"` + pin + `,"halves":[{"class":"host-identity","signature":"a","reason":"r"},{"class":"host-identity","signature":"b","reason":"r"}]}`, "host-identity"},
	}

	for _, c := range refused {
		_, _, err := readTestDisclosureManifest(rawManifest(t, c.entry))
		if err == nil || !strings.Contains(err.Error(), c.want) || !strings.Contains(err.Error(), "TestX") {
			t.Errorf("%s: the loader must refuse it, naming the entry and %q; got %v", c.name, c.want, err)
		}
	}

	// And the shape it exists for loads: host-identity, the pin, an empty C# signature.
	if _, _, err := readTestDisclosureManifest(rawManifest(t, entry(`,"class":"host-identity","signature":""`+pin))); err != nil {
		t.Fatalf("a host-identity entry with a goSkipSignature and an empty signature must load: %v", err)
	}
}

func TestALiveGoSideHostSkipIsNotOrphaned(t *testing.T) {
	// The orphan report's predicate is a converted-side pass, which is this shape's LIVE reading. A
	// Go-skip entry whose Go side skipped is doing its job; where both sides pass it is idle here and
	// reported like any other entry.
	disclosures := loadRaw(t, "darwin", goSkipEntry())
	name := "TestProfile/Default"

	if orphans := orphanedDisclosures(disclosures, map[string]string{name: "skip"}, map[string]string{name: "pass"}, "darwin"); len(orphans) != 0 {
		t.Fatalf("a live Go-skip disclosure must not be reported orphaned; got %+v", orphans)
	}
	if orphans := orphanedDisclosures(disclosures, map[string]string{name: "pass"}, map[string]string{name: "pass"}, "darwin"); len(orphans) != 1 {
		t.Fatalf("an idle Go-skip disclosure (pass/pass) is reported like any other; got %+v", orphans)
	}
}

func TestAGoSideHostSkipIsStatedOnTheProofPage(t *testing.T) {
	// The converted side PASSES this row, so the page must not claim the managed CLR cannot satisfy
	// it, and must say in words that it was Go that skipped.
	comparison := testComparison{
		Package: "internal/trace", Status: "validated",
		Go:     map[string]string{"TestProfile/Default": "skip", "TestOther": "pass"},
		CSharp: map[string]string{"TestProfile/Default": "pass", "TestOther": "pass"},
	}

	page := renderValidationProofPage(proofPageProvenance{importPath: "internal/trace", goVersion: "1.26.10", platform: "darwin/amd64", date: "2026-11-09"},
		comparison, loadRaw(t, "darwin", goSkipEntry()), nil)

	if strings.Contains(page, "provably cannot") || strings.Contains(page, "not\na skipped test") {
		t.Fatalf("a page disclosing a Go-side skip must not claim the converted side cannot satisfy it, nor that no test skipped:\n%s", page)
	}
	if !strings.Contains(page, "`TestProfile/Default` is a **Go-side host skip**") {
		t.Fatalf("the page names the row as a Go-side host skip in words:\n%s", page)
	}
}

// ---- the native-subprocess skip: skipEitherSide (COORD ruling 2026-11-14) ----------------------------
//
// internal/trace's TestTraceCPUProfile subtests build and run one native Go program on BOTH sides, and
// that program decides pass or skip, so the pinned skip can land on either side. The entry admits the
// mirror shape, Go=pass / C#=skip, pinned on the converted side's skip output carrying Go's own text.

const nativeSkipPin = "CPU profile did not include any samples while tracing was active"

func nativeSkipEntry(eitherSide bool) string {
	flag := ""
	if eitherSide {
		flag = `,"skipEitherSide":true`
	}

	return `{"name":"TestProfile/Default","class":"host-identity","platforms":["darwin"],"goSkipSignature":"` + nativeSkipPin + `"` + flag + `,"signature":"","reason":"both sides run one native Go program"}`
}

func matchNativeSkip(disclosures map[string]testDisclosure, goStatus, csStatus, goOutput, csOutput string) (mismatches, disclosed []string) {
	const name = "TestProfile/Default"

	mismatches, _, disclosed, _ = matchTerminalStatusesWithRecords([]string{name},
		map[string]string{name: goStatus}, map[string]string{name: csStatus}, disclosures,
		map[string]string{name: csOutput}, nil, map[string]string{name: goOutput})
	return mismatches, disclosed
}

func TestANativeSubprocessSkipIsDisclosedOnEitherSide(t *testing.T) {
	disclosures := loadRaw(t, "darwin", nativeSkipEntry(true))
	pinned := "    trace_test.go:121: " + nativeSkipPin + "\n"
	other := "    trace_test.go:99: skipping: some other reason\n"

	cases := []struct {
		row                string
		goStatus, csStatus string
		goOutput, csOutput string
		disclosed          bool
		mismatchContains   string
	}{
		{"Go skips with the pin, C# passes", "skip", "pass", pinned, "", true, ""},
		{"C# skips with the pin, Go passes", "pass", "skip", "", pinned, true, ""},
		{"C# skips for another reason", "pass", "skip", "", other, false, `C#'s skip output does not carry`},
		{"Go skips for another reason", "skip", "pass", other, "", false, `Go's skip output does not carry`},
		{"C# skips with the pin in Go's output only", "pass", "skip", pinned, other, false, `C#'s skip output does not carry`},
		{"C# fails where Go passed", "pass", "fail", "", pinned, false, `C#="fail"`},
		{"Go fails where C# skipped with the pin", "fail", "skip", pinned, pinned, false, `Go="fail"`},
		{"Go fails where C# passed", "fail", "pass", pinned, "", false, `Go="fail"`},
		{"both sides skip", "skip", "skip", pinned, pinned, false, ""},
		{"both sides pass", "pass", "pass", "", "", false, ""},
	}

	for _, c := range cases {
		mismatches, disclosed := matchNativeSkip(disclosures, c.goStatus, c.csStatus, c.goOutput, c.csOutput)

		if c.disclosed {
			if len(mismatches) != 0 || len(disclosed) != 1 {
				t.Errorf("%s: must be DISCLOSED; mismatches %q disclosed %q", c.row, mismatches, disclosed)
			}
			continue
		}

		if len(disclosed) != 0 {
			t.Errorf("%s: must never be disclosed; disclosed %q", c.row, disclosed)
		}
		if c.mismatchContains == "" {
			if len(mismatches) != 0 {
				t.Errorf("%s: the sides agree, so the entry is inert; mismatches %q", c.row, mismatches)
			}
			continue
		}
		if len(mismatches) != 1 || !strings.Contains(mismatches[0], c.mismatchContains) {
			t.Errorf("%s: must be one strict mismatch naming %q; got %q", c.row, c.mismatchContains, mismatches)
		}
	}
}

func TestTheMirrorSkipNeedsSkipEitherSide(t *testing.T) {
	// The control: the same entry without the flag keeps G's one-direction arm, so a C# skip carrying
	// the pin where Go passed is still a strict mismatch.
	mismatches, disclosed := matchNativeSkip(loadRaw(t, "darwin", nativeSkipEntry(false)), "pass", "skip", "", "    trace_test.go:121: "+nativeSkipPin+"\n")

	if len(disclosed) != 0 || len(mismatches) != 1 || !strings.Contains(mismatches[0], `C#="skip"`) {
		t.Fatalf("without skipEitherSide a C# skip is a mismatch; mismatches %q disclosed %q", mismatches, disclosed)
	}
}

func TestSkipEitherSideIsRefusedWithoutAGoSkipSignature(t *testing.T) {
	entry := `{"name":"TestX","class":"host-identity","signature":"some converted text","skipEitherSide":true,"reason":"r"}`

	_, _, err := readTestDisclosureManifest(rawManifest(t, entry))
	if err == nil || !strings.Contains(err.Error(), "TestX") || !strings.Contains(err.Error(), "goSkipSignature") {
		t.Fatalf("skipEitherSide alone must be refused, naming the entry and goSkipSignature; got %v", err)
	}
}

func TestANativeSubprocessSkipIsStatedOnTheProofPage(t *testing.T) {
	comparison := testComparison{
		Package: "internal/trace", Status: "validated",
		Go:     map[string]string{"TestProfile/Default": "pass", "TestOther": "pass"},
		CSharp: map[string]string{"TestProfile/Default": "skip", "TestOther": "pass"},
	}

	page := renderValidationProofPage(proofPageProvenance{importPath: "internal/trace", goVersion: "1.26.10", platform: "darwin/arm64", date: "2026-11-14"},
		comparison, loadRaw(t, "darwin", nativeSkipEntry(true)), nil)

	if strings.Contains(page, "provably cannot") || !strings.Contains(page, "**native-subprocess skip**") || strings.Contains(page, "**Go-side host skip**") {
		t.Fatalf("the page names the row as a native-subprocess skip in words, and only that:\n%s", page)
	}
}

func TestTheGoSkipPinIsReadFromGoTestOutputEvents(t *testing.T) {
	// go test -json's own shape for a skipped subtest, as go1.26.10 writes it: the t.Skip text is an
	// "output" event and the terminal "skip" event carries no Output. Reading only terminal events
	// left the Go side's skip text empty, so the Go=skip shape could never be admitted outside a
	// unit test that supplied the text by hand.
	stream := strings.Join([]string{
		`{"Time":"2026-11-14T12:38:20.86Z","Action":"run","Package":"internal/trace","Test":"TestProfile/Default"}`,
		`{"Time":"2026-11-14T12:38:20.86Z","Action":"output","Package":"internal/trace","Test":"TestProfile/Default","Output":"=== RUN   TestProfile/Default\n"}`,
		`{"Time":"2026-11-14T12:38:20.86Z","Action":"output","Package":"internal/trace","Test":"TestProfile/Default","Output":"    trace_test.go:121: ` + nativeSkipPin + `\n"}`,
		`{"Time":"2026-11-14T12:38:20.86Z","Action":"output","Package":"internal/trace","Test":"TestProfile/Default","Output":"--- SKIP: TestProfile/Default (0.00s)\n"}`,
		`{"Time":"2026-11-14T12:38:20.86Z","Action":"skip","Package":"internal/trace","Test":"TestProfile/Default","Elapsed":0}`,
	}, "\n")

	goOutputs := goSideTestOutputs(stream)
	if !strings.Contains(goOutputs["TestProfile/Default"], nativeSkipPin) {
		t.Fatalf("Go's skip text must be read from its output events; got %q", goOutputs["TestProfile/Default"])
	}

	mismatches, disclosed := matchNativeSkip(loadRaw(t, "darwin", nativeSkipEntry(true)), "skip", "pass", goOutputs["TestProfile/Default"], "")
	if len(mismatches) != 0 || len(disclosed) != 1 {
		t.Fatalf("a real go test skip with the pin is disclosed; mismatches %q disclosed %q", mismatches, disclosed)
	}
}
