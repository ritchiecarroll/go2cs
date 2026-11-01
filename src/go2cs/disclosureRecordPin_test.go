// disclosureRecordPin_test.go - Gbtc
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

// The disclosure-completeness guard, phase A. A disclosure's signature is a substring of the test's
// WHOLE log, so it proves the pinned failure is present, not that it is the only one: runtime/debug's
// TestStack validated on linux with five misaligned asserts around its signature. The record-count
// pin is what closes that. The shape below is TestStack's own: the GOROOT t.Logf record, then the
// signature's assert.

const stackSignature = `expected prefix "\ttesting/testing.go"`

var stackRecords = []string{
	`found GOROOT "C:\\go" from environment; checking embedded GOROOT value`,
	`in line "\tC:/go2cs/src/core/testing/TestExecution.cs:1095", expected prefix "\ttesting/testing.go"`,
}

func stackDisclosure(records int) map[string]testDisclosure {
	return map[string]testDisclosure{
		"TestStack": {Name: "TestStack", Class: "host-identity", Signature: stackSignature, Reason: "the testing frame's identity", Records: records},
	}
}

// matchStack runs the pin-aware matcher over one TestStack row, Go pass / C# fail, with the given
// C# records (joined into the output the way the host joins them).
func matchStack(disclosures map[string]testDisclosure, records []string, present bool) (mismatches, disclosed []string) {
	goResults := map[string]string{"TestStack": "pass"}
	csResults := map[string]string{"TestStack": "fail"}
	csOutputs := map[string]string{"TestStack": strings.Join(records, "\r\n")}
	csRecords := map[string]testRecords{"TestStack": {Present: present, Records: records}}
	mismatches, _, disclosed, _ = matchTerminalStatusesWithRecords([]string{"TestStack"}, goResults, csResults, disclosures, csOutputs, csRecords)
	return mismatches, disclosed
}

func planted() []string {
	return []string{stackRecords[0], `planted extra assert: expected "debug.Stack" in "goroutine 1"`, stackRecords[1]}
}

// (a) The pin holds when the C# side logs exactly the records it was minted from.
func TestRecordPinAdmitsTheMintedCount(t *testing.T) {
	mismatches, disclosed := matchStack(stackDisclosure(2), stackRecords, true)
	if len(mismatches) != 0 || len(disclosed) != 1 {
		t.Fatalf("a pinned row logging exactly its minted records must be disclosed; mismatches %q disclosed %q", mismatches, disclosed)
	}
}

// (b) One planted assert beside the signature: the row is a mismatch, and the message NAMES it.
func TestRecordPinNamesAPlantedExtraRecord(t *testing.T) {
	mismatches, disclosed := matchStack(stackDisclosure(2), planted(), true)
	if len(disclosed) != 0 || len(mismatches) != 1 {
		t.Fatalf("a planted record must fail the row; mismatches %q disclosed %q", mismatches, disclosed)
	}
	if !strings.Contains(mismatches[0], "planted extra assert") || !strings.Contains(mismatches[0], "logged 3 record(s)") || !strings.Contains(mismatches[0], "pins 2") {
		t.Fatalf("the mismatch must name the planted record and both counts; got %q", mismatches[0])
	}
	if strings.Contains(mismatches[0], "checking embedded GOROOT") == false {
		t.Fatalf("every record outside the signature is named, not just the new one; got %q", mismatches[0])
	}
}

// (c) The control of the control: the SAME planted event against the entry WITHOUT a pin is absorbed.
// That is the defect itself, and it proves the pin, not the signature, is what catches the plant.
func TestUnpinnedEntryAbsorbsThePlant(t *testing.T) {
	mismatches, disclosed := matchStack(stackDisclosure(0), planted(), true)
	if len(mismatches) != 0 || len(disclosed) != 1 {
		t.Fatalf("without a pin the signature alone absorbs the plant (the defect this guard closes); mismatches %q disclosed %q", mismatches, disclosed)
	}
}

// (d) A pinned entry against an event with no record list fails: a host without the field cannot
// show the pin holds, so it must not fall back to the signature alone.
func TestPinnedEntryRefusesAnEventWithoutRecords(t *testing.T) {
	mismatches, disclosed := matchStack(stackDisclosure(2), stackRecords, false)
	if len(disclosed) != 0 || len(mismatches) != 1 || !strings.Contains(mismatches[0], "carries no record list") {
		t.Fatalf("a pinned entry must refuse an event without records; mismatches %q disclosed %q", mismatches, disclosed)
	}
}

// A MISSING record moves the count as surely as an extra one.
func TestRecordPinCatchesAMissingRecord(t *testing.T) {
	mismatches, _ := matchStack(stackDisclosure(2), stackRecords[1:], true)
	if len(mismatches) != 1 || !strings.Contains(mismatches[0], "logged 1 record(s)") {
		t.Fatalf("a missing record must fail the pin; got %q", mismatches)
	}
}

// Records the host's log cap dropped are records nobody can count, so the pin cannot hold.
func TestRecordPinRefusesDroppedRecords(t *testing.T) {
	goResults := map[string]string{"TestStack": "pass"}
	csResults := map[string]string{"TestStack": "fail"}
	csOutputs := map[string]string{"TestStack": strings.Join(stackRecords, "\r\n")}
	csRecords := map[string]testRecords{"TestStack": {Present: true, Records: stackRecords, Dropped: 3}}
	mismatches, _, _, _ := matchTerminalStatusesWithRecords([]string{"TestStack"}, goResults, csResults, stackDisclosure(2), csOutputs, csRecords)
	if len(mismatches) != 1 || !strings.Contains(mismatches[0], "dropped 3 more") {
		t.Fatalf("dropped records must fail the pin and say so; got %q", mismatches)
	}
}

// A withdrawal ROOT must hold its pin too: a root whose count moved is itself a mismatch, so the
// Go-only rows under it are not its mechanical consequence and compare strictly.
func TestPinFailingRootDoesNotWithdrawItsFanOut(t *testing.T) {
	goResults := map[string]string{"TestStack": "pass", "TestStack/case": "pass"}
	csResults := map[string]string{"TestStack": "fail"}
	records := planted()
	csOutputs := map[string]string{"TestStack": strings.Join(records, "\r\n")}
	csRecords := map[string]testRecords{"TestStack": {Present: true, Records: records}}
	mismatches, _, _, withdrawn := matchTerminalStatusesWithRecords([]string{"TestStack", "TestStack/case"}, goResults, csResults, stackDisclosure(2), csOutputs, csRecords)
	if len(withdrawn) != 0 || len(mismatches) != 2 {
		t.Fatalf("a root failing its pin must withdraw nothing, and both rows compare strictly; mismatches %q withdrawn %q", mismatches, withdrawn)
	}

	// Positive control: the same shape with the pin holding withdraws the child.
	csRecords["TestStack"] = testRecords{Present: true, Records: stackRecords}
	csOutputs["TestStack"] = strings.Join(stackRecords, "\r\n")
	mismatches, _, _, withdrawn = matchTerminalStatusesWithRecords([]string{"TestStack", "TestStack/case"}, goResults, csResults, stackDisclosure(2), csOutputs, csRecords)
	if len(mismatches) != 0 || len(withdrawn) != 1 {
		t.Fatalf("a root holding its pin withdraws its Go-only fan-out; mismatches %q withdrawn %q", mismatches, withdrawn)
	}
}

// The host emits `records` on a terminal fail/skip event only. Present distinguishes an EMPTY list
// (a new host, a test that logged nothing) from an ABSENT one (every other event, and an old host).
func TestTerminalTestRecordsDistinguishesEmptyFromAbsent(t *testing.T) {
	stream := strings.Join([]string{
		`{"test":"TestA","action":"fail","output":"one\r\ntwo","records":["one","two"]}`,
		`{"test":"TestB","action":"fail","output":null,"records":[]}`,
		`{"test":"TestC","action":"fail","output":"old host"}`,
		`{"test":"TestD","action":"skip","output":"skipped","records":["skipped"],"recordsDropped":2}`,
	}, "\n")
	got := terminalTestRecords(stream)
	if r := got["TestA"]; !r.Present || len(r.Records) != 2 || r.Records[1] != "two" {
		t.Fatalf("TestA: want 2 present records, got %+v", r)
	}
	if r := got["TestB"]; !r.Present || len(r.Records) != 0 {
		t.Fatalf("TestB: an empty list is PRESENT with 0 records, got %+v", r)
	}
	if r := got["TestC"]; r.Present {
		t.Fatalf("TestC: an event without the field is ABSENT, got %+v", r)
	}
	if r := got["TestD"]; !r.Present || r.Dropped != 2 {
		t.Fatalf("TestD: recordsDropped must be read, got %+v", r)
	}
}

// The loader refuses a count that cannot mean anything: a negative one, and any on a host-fatal
// entry, whose test runs on neither side. The valid shapes load, so the refusals are the rule's
// and not the fixture's.
func TestDisclosureLoaderRecordRules(t *testing.T) {
	load := func(entry string) error {
		dir := t.TempDir()
		body := `{"schemaVersion":1,"disclosures":[` + entry + `]}`
		if err := os.WriteFile(filepath.Join(dir, testDisclosureFileName), []byte(body), 0o644); err != nil {
			t.Fatal(err)
		}
		_, _, err := readTestDisclosureManifest(dir)
		return err
	}

	if err := load(`{"name":"TestStack","class":"host-identity","signature":"x","reason":"r","records":2}`); err != nil {
		t.Fatalf("a positive pin must load; got %v", err)
	}
	if err := load(`{"name":"TestStack","class":"host-identity","signature":"x","reason":"r"}`); err != nil {
		t.Fatalf("an unpinned (legacy) entry must load in phase A; got %v", err)
	}
	if err := load(`{"name":"TestStack","class":"host-identity","signature":"x","reason":"r","records":-1}`); err == nil || !strings.Contains(err.Error(), "negative record count") {
		t.Fatalf("a negative pin must be refused; got %v", err)
	}
	if err := load(`{"name":"TestPanicOnFault","class":"host-fatal","signature":"","reason":"r","records":1}`); err == nil || !strings.Contains(err.Error(), "logs nothing to count") {
		t.Fatalf("a pin on a host-fatal entry must be refused; got %v", err)
	}
}

// AN OPTIONAL RECORD (ruled option A, 2026-11-01, ledger 05:11): a record a pinned test logs on SOME
// runs. runtime's TestMemStats always logs its twelve disclosed zero-field records, and adds a
// thirteenth when the CLR's real total GC pause crosses Go's 1e11 ns sanity bound, which 8 of 10 linux
// TC0 readings did and 2 did not. An exact pin of 12 reds every crossing run and 13 reds every other,
// so the entry names the thirteenth as optional: it may absorb AT MOST one record, outside the
// signature, and only a record carrying EVERY one of its substrings (the field AND the failure text),
// so no other PauseTotalNs record is ever absorbed.

const memStatsSignature = "MSpanInuse = 0: zero value"

var memStatsRecords = func() []string {
	var records []string
	for _, field := range []string{"Mallocs", "Frees", "HeapObjects", "StackInuse", "StackSys", "MSpanInuse", "MSpanSys",
		"MCacheInuse", "MCacheSys", "BuckHashSys", "GCSys", "OtherSys"} {
		records = append(records, "malloc_test.go:95: "+field+" = 0: zero value")
	}
	return records
}()

const pauseRecord = "malloc_test.go:95: PauseTotalNs = 104542321000: insanely high value (overflow?); want <= 1e+11"

const memStatsEntry = `{"name":"TestMemStats","class":"runtime-capability","signature":"` + memStatsSignature + `",` +
	`"reason":"the span and cache fields","records":12,` +
	`"optionalRecords":[{"contains":["PauseTotalNs = ","insanely high value (overflow?)"],"reason":"the real GC pause total"}]}`

func memStatsWith(extra ...string) []string {
	return append(append([]string{}, memStatsRecords...), extra...)
}

// Both states admit: the twelve alone, and the twelve plus the crossing record.
func TestAnOptionalRecordAdmitsBothStates(t *testing.T) {
	disclosures := loadRaw(t, "linux", memStatsEntry)

	for _, records := range [][]string{memStatsWith(), memStatsWith(pauseRecord)} {
		if mismatches, disclosed := matchOne(disclosures, "TestMemStats", "fail", records, true); len(mismatches) != 0 || len(disclosed) != 1 {
			t.Fatalf("%d records must be disclosed; mismatches %q disclosed %q", len(records), mismatches, disclosed)
		}
	}
}

// The pin still counts everything else: a 13th unrelated record, and a 14th beside the optional one.
func TestAnOptionalRecordAbsorbsNothingElse(t *testing.T) {
	disclosures := loadRaw(t, "linux", memStatsEntry)
	planted := "malloc_test.go:95: planted extra assert"

	for _, records := range [][]string{memStatsWith(planted), memStatsWith(pauseRecord, planted)} {
		mismatches, disclosed := matchOne(disclosures, "TestMemStats", "fail", records, true)

		if len(disclosed) != 0 || len(mismatches) != 1 || !strings.Contains(mismatches[0], "planted extra assert") {
			t.Fatalf("%d records with a plant must be a mismatch naming it; mismatches %q disclosed %q", len(records), mismatches, disclosed)
		}
	}
}

// One optional record absorbs one record: a doubled crossing line is a mismatch that says so.
func TestAnOptionalRecordAbsorbsAtMostOne(t *testing.T) {
	mismatches, disclosed := matchOne(loadRaw(t, "linux", memStatsEntry), "TestMemStats", "fail", memStatsWith(pauseRecord, pauseRecord), true)

	if len(disclosed) != 0 || len(mismatches) != 1 || !strings.Contains(mismatches[0], "absorbs at most one") {
		t.Fatalf("a doubled optional record must be a mismatch that says it absorbs at most one; mismatches %q disclosed %q", mismatches, disclosed)
	}
}

// The failure text is part of the pin: a PauseTotalNs record of another shape is NOT absorbed.
func TestAnOptionalRecordNeedsItsFailureText(t *testing.T) {
	zero := "malloc_test.go:95: PauseTotalNs = 0: zero value"
	mismatches, disclosed := matchOne(loadRaw(t, "linux", memStatsEntry), "TestMemStats", "fail", memStatsWith(zero), true)

	if len(disclosed) != 0 || len(mismatches) != 1 || !strings.Contains(mismatches[0], "PauseTotalNs = 0: zero value") {
		t.Fatalf("a PauseTotalNs record without the failure text must stay a mismatch naming it; mismatches %q disclosed %q", mismatches, disclosed)
	}
}

// The proof page states the optional record's own reason on a row of its own.
func TestTheProofPageStatesAnOptionalRecord(t *testing.T) {
	comparison := testComparison{
		Package: "runtime", Status: "validated",
		Go:     map[string]string{"TestMemStats": "pass"},
		CSharp: map[string]string{"TestMemStats": "fail"},
	}

	page := renderValidationProofPage(proofPageProvenance{importPath: "runtime", goVersion: "1.26.10", platform: "linux/amd64", date: "2026-11-01"},
		comparison, loadRaw(t, "linux", memStatsEntry), nil)

	for _, row := range []string{"| `runtime-capability` | the span and cache fields |", "| `optional record` | the real GC pause total |"} {
		if !strings.Contains(page, row) {
			t.Fatalf("the page states the entry and its optional record, missing %q:\n%s", row, page)
		}
	}
}

// The loader refuses every optional record that cannot mean what it says. The valid entry loads, so the
// refusals are the rule's and not the fixture's.
func TestTheOptionalRecordLoaderRefusals(t *testing.T) {
	entry := func(records, optionals string) string {
		return `{"name":"TestMemStats","class":"runtime-capability","signature":"` + memStatsSignature + `","reason":"r"` +
			records + `,"optionalRecords":[` + optionals + `]}`
	}
	optional := func(contains, reason string) string {
		return `{"contains":[` + contains + `],"reason":"` + reason + `"}`
	}
	pause := optional(`"PauseTotalNs = ","insanely high value"`, "p")
	load := func(body string) error {
		_, _, _, err := loadTestDisclosures(rawManifest(t, body), "linux")
		return err
	}

	if err := load(entry(`,"records":12`, pause)); err != nil {
		t.Fatalf("a valid optional record must load; got %v", err)
	}

	for _, refused := range []struct{ body, want string }{
		{entry(``, pause), "no record-count pin"},
		{entry(`,"records":12`, optional(`"PauseTotalNs = "`, "p")), "at least two"},
		{entry(`,"records":12`, optional(`"PauseTotalNs = ",""`, "p")), "empty substring"},
		{entry(`,"records":12`, optional(`"PauseTotalNs = ","insanely high value"`, "")), "requires a reason"},
		{entry(`,"records":12`, pause+","+optional(`"insanely high value","PauseTotalNs = "`, "q")), "twice"},
		{entry(`,"records":12`, optional(`"MSpanInuse = 0","zero value"`, "m")), "signature's own record"},
	} {
		if err := load(refused.body); err == nil || !strings.Contains(err.Error(), refused.want) {
			t.Errorf("want a refusal containing %q for %s; got %v", refused.want, refused.body, err)
		}
	}
}

// What the comparison record publishes for a disclosed test: every record outside the signature.
func TestNonSignatureRecordsExcludesOnlyTheSignature(t *testing.T) {
	got := nonSignatureRecords(planted(), stackSignature)
	if len(got) != 2 || !strings.Contains(got[1], "planted extra assert") {
		t.Fatalf("want the GOROOT log and the plant, got %q", got)
	}
}
