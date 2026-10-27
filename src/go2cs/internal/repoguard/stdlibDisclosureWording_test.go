// stdlibDisclosureWording_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package repoguard

import (
	"encoding/json"
	"os"
	"path/filepath"
	"regexp"
	"sort"
	"strings"
	"testing"
)

// A standard-library disclosure's rendered text is PUBLIC, exactly as a module's is (moduleDisclosureWording_test.go):
// validationProofPages.go copies each entry's `reason` (each half's, for a two-half entry), its `hostConditional`
// sentence and the manifest's `notes` onto the package's proof page under docs/validation/current, which go2cs.net
// publishes. So that text must read to a visitor: what the test does, why the converted side differs, what was
// measured. Until seat #13 of the 1.26.10.2 plan, 320 of the 371 standard-library reasons carried their own history
// (RELABEL narratives, lane names, ledger and mailbox citations, SHAs of posts), on 704 proof pages; the owner's
// OQ-1 ruling (2026-10-26) is that nothing reachable from go2cs.net carries the fleet's process. The words an entry's
// visitor reason dropped are kept, verbatim, in its `history` field, which the renderer never reads.
//
// WHAT IS REFUSED: the module guard's fleetWordPattern (COORD and BOARD upper case; ruling/ruled, lane(s) and
// bank/banked/banks in any case), plus the rest of the fleet's vocabulary that turned up in these manifests: RELABEL,
// the lane names C1, C2, P1, P2, i7 and i9 as whole words, a single-letter lane name in an attribution parenthesis
// ("(G, ...", "(R)"), and seat, train, ledger, mailbox and fleet as whole words. Only rendered text is read: want,
// reading, plan, proof and history are the manifest's own record and stay as written.
//
// POSITIVE CONTROL: TestStdlibDisclosureWordingGuardFiresOnAPlantedWord. Red-first when this guard landed: at master
// 2a1698bc21 TestStdlibDisclosureTextHasNoFleetWords failed on the standard-library manifests' rendered text; with
// the rewrite it reads clean.

const stdlibDisclosuresRoot = "src/core/"

var stdlibProcessPattern = regexp.MustCompile(`\bRELABEL(?:ED|S)?\b|\b(?:C1|C2|P1|P2|i7|i9)\b|\((?:G|H|R)[,;)]|(?i:\b(?:seats?|seated|trains?|ledger|mailbox|fleet)\b)`)

type stdlibDisclosureText struct {
	Name            string `json:"name"`
	Reason          string `json:"reason"`
	HostConditional string `json:"hostConditional"`
	Halves          []struct {
		Reason string `json:"reason"`
	} `json:"halves"`
}

type stdlibDisclosureManifest struct {
	Disclosures []stdlibDisclosureText `json:"disclosures"`
	Notes       []string               `json:"notes"`
}

// stdlibProcessWordsIn returns the fleet and process words in rendered text, in order of appearance.
func stdlibProcessWordsIn(text string) []string {
	type hit struct {
		at   int
		word string
	}

	var hits []hit

	for _, pattern := range []*regexp.Regexp{fleetWordPattern, stdlibProcessPattern} {
		for _, loc := range pattern.FindAllStringIndex(text, -1) {
			hits = append(hits, hit{loc[0], text[loc[0]:loc[1]]})
		}
	}

	sort.SliceStable(hits, func(i, j int) bool { return hits[i].at < hits[j].at })

	words := make([]string, len(hits))

	for i, h := range hits {
		words[i] = h.word
	}

	return words
}

// stdlibDisclosureWordingFindings returns one line per rendered text that carries a fleet or process word.
func stdlibDisclosureWordingFindings(path string, data []byte) ([]string, error) {
	var manifest stdlibDisclosureManifest

	if err := json.Unmarshal(data, &manifest); err != nil {
		return nil, err
	}

	var findings []string

	add := func(where, text string) {
		if words := stdlibProcessWordsIn(text); len(words) > 0 {
			findings = append(findings, path+": "+where+": "+strings.Join(words, ", "))
		}
	}

	for _, d := range manifest.Disclosures {
		add("disclosure "+d.Name+" reason", d.Reason)
		add("disclosure "+d.Name+" hostConditional", d.HostConditional)

		for i, half := range d.Halves {
			add("disclosure "+d.Name+" half "+string(rune('1'+i))+" reason", half.Reason)
		}
	}

	for i, note := range manifest.Notes {
		add("note "+string(rune('0'+i)), note)
	}

	return findings, nil
}

// TestStdlibDisclosureTextHasNoFleetWords is the guard.
func TestStdlibDisclosureTextHasNoFleetWords(t *testing.T) {
	root := repoRootFromPackageDir(t)

	var manifests []string

	for _, p := range gitTrackedFiles(t, root) {
		if strings.HasPrefix(p, stdlibDisclosuresRoot) && strings.HasSuffix(p, "/go2cs_test_disclosures.json") {
			manifests = append(manifests, p)
		}
	}

	if len(manifests) < 20 {
		t.Fatalf("VACUOUS: %d tracked manifest(s) under %s; the guard would read almost nothing", len(manifests), stdlibDisclosuresRoot)
	}

	sort.Strings(manifests)

	var findings []string

	for _, p := range manifests {
		data, err := os.ReadFile(filepath.Join(root, filepath.FromSlash(p)))

		if err != nil {
			t.Fatalf("cannot read %s: %v", p, err)
		}

		found, err := stdlibDisclosureWordingFindings(p, data)

		if err != nil {
			t.Fatalf("%s does not parse: %v", p, err)
		}

		findings = append(findings, found...)
	}

	t.Logf("standard-library disclosure manifests read: %d", len(manifests))

	for _, f := range findings {
		t.Errorf("fleet wording in public disclosure text (state the mechanism and the evidence; the words removed go to the entry's history field): %s", f)
	}
}

// TestStdlibDisclosureWordingGuardFiresOnAPlantedWord is the positive control: every refused shape, and the record
// fields and history the renderer never reads left alone.
func TestStdlibDisclosureWordingGuardFiresOnAPlantedWord(t *testing.T) {
	planted := []byte(`{"schemaVersion": 1, "disclosures": [
		{"name": "TestX", "class": "deferred", "signature": "s",
		 "reason": "allocates in the managed model. RELABEL 2026-09-23 (C1, as ruled at ledger 03:37).",
		 "plan": "COORD's seat; the BOARD row", "history": "RELABEL by C1, ruled by COORD"},
		{"name": "TestY", "class": "host-identity", "signature": "s", "reason": "reads the host's name.",
		 "hostConditional": "decided on the i9's lane box"},
		{"name": "TestZ", "halves": [{"class": "structural", "signature": "a", "reason": "boxes (R) a value"},
		 {"class": "deferred", "signature": "b", "reason": "measured on train Q"}]}],
		"notes": ["The fleet re-banked it."]}`)

	found, err := stdlibDisclosureWordingFindings("planted.json", planted)

	if err != nil {
		t.Fatalf("planted manifest does not parse: %v", err)
	}

	want := []string{
		"planted.json: disclosure TestX reason: RELABEL, C1, ruled, ledger",
		"planted.json: disclosure TestY hostConditional: i9, lane",
		"planted.json: disclosure TestZ half 1 reason: (R)",
		"planted.json: disclosure TestZ half 2 reason: train",
		"planted.json: note 0: fleet, banked",
	}

	if strings.Join(found, "\n") != strings.Join(want, "\n") {
		t.Fatalf("planted findings:\n%s\nwant:\n%s", strings.Join(found, "\n"), strings.Join(want, "\n"))
	}
}
