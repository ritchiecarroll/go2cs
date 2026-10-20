// moduleDisclosureWording_test.go - Gbtc
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
	"strconv"
	"strings"
	"testing"
)

// A module disclosure's reason and notes are PUBLIC text. The converter copies each one verbatim onto the
// proof page it renders (validationProofPages.go), and that page ships inside the nugetgo package as
// VALIDATION.md and is copied onto docs/validation/modules. So the text must read to a visitor: what the
// test does, why go2cs differs, what was measured, when the entry would be removed. Who ruled an entry
// and when is fleet provenance; it belongs in git history and the BOARD, never in the manifest.
// Three manifests carried it (`COORD ruling 2026-10-12 (wave 2)`, `the windows bank's count`, `see the
// BOARD row of that date`) until 90904df583 reworded them.
//
// WHAT IS REFUSED (fleetWordPattern): COORD and BOARD as the fleet spells them (upper case), and
// ruling/ruled, lane(s) and bank/banked/banks in any case, each as a whole word. Whole words keep
// "keyboard", "bankruptcy" and "ruler" out.
//
// POSITIVE CONTROL: TestModuleDisclosureWordingGuardFiresOnAPlantedWord runs the same check on planted
// text. Verified red-first when this guard landed: TestModuleDisclosureTextHasNoFleetWords fails at
// master 3125828203 (before 90904df583), naming the semaphore, uuid and go-cmp manifests.

const moduleDisclosuresRoot = "src/tests/ModuleDisclosures/"

var fleetWordPattern = regexp.MustCompile(`\b(?:COORD|BOARD)\b|(?i:\b(?:rul(?:ing|ings|ed)|lanes?|bank(?:s|ed)?)\b)`)

type moduleDisclosureManifest struct {
	Disclosures []struct {
		Name   string `json:"name"`
		Reason string `json:"reason"`
	} `json:"disclosures"`
	Notes []string `json:"notes"`
}

// fleetWordsIn returns the fleet words in text, in order of appearance.
func fleetWordsIn(text string) []string {
	return fleetWordPattern.FindAllString(text, -1)
}

// moduleDisclosureWordingFindings returns one line per reason or note that carries a fleet word.
func moduleDisclosureWordingFindings(path string, data []byte) ([]string, error) {
	var manifest moduleDisclosureManifest

	if err := json.Unmarshal(data, &manifest); err != nil {
		return nil, err
	}

	var findings []string

	for _, d := range manifest.Disclosures {
		if words := fleetWordsIn(d.Reason); len(words) > 0 {
			findings = append(findings, path+": disclosure "+d.Name+" reason: "+strings.Join(words, ", "))
		}
	}

	for i, note := range manifest.Notes {
		if words := fleetWordsIn(note); len(words) > 0 {
			findings = append(findings, path+": note "+strconv.Itoa(i)+": "+strings.Join(words, ", "))
		}
	}

	return findings, nil
}

// TestModuleDisclosureTextHasNoFleetWords is the guard.
func TestModuleDisclosureTextHasNoFleetWords(t *testing.T) {
	root := repoRootFromPackageDir(t)

	var manifests []string

	for _, p := range gitTrackedFiles(t, root) {
		if strings.HasPrefix(p, moduleDisclosuresRoot) && strings.HasSuffix(p, "/go2cs_test_disclosures.json") {
			manifests = append(manifests, p)
		}
	}

	if len(manifests) == 0 {
		t.Fatalf("no tracked manifest under %s: the guard would read nothing", moduleDisclosuresRoot)
	}

	sort.Strings(manifests)

	var findings []string

	for _, p := range manifests {
		data, err := os.ReadFile(filepath.Join(root, filepath.FromSlash(p)))

		if err != nil {
			t.Fatalf("cannot read %s: %v", p, err)
		}

		found, err := moduleDisclosureWordingFindings(p, data)

		if err != nil {
			t.Fatalf("%s does not parse: %v", p, err)
		}

		findings = append(findings, found...)
	}

	t.Logf("module disclosure manifests read: %d", len(manifests))

	for _, f := range findings {
		t.Errorf("fleet wording in public disclosure text (state the mechanism and the evidence; keep who ruled it in git history): %s", f)
	}
}

// TestModuleDisclosureWordingGuardFiresOnAPlantedWord is the positive control.
func TestModuleDisclosureWordingGuardFiresOnAPlantedWord(t *testing.T) {
	planted := []byte(`{"schemaVersion": 1, "disclosures": [{"name": "TestX", "class": "runtime-capability",
		"signature": "s", "reason": "the first call is slow. COORD ruling 2026-10-12: disclose."}],
		"notes": ["Measured on a lane box; see the BOARD row."]}`)

	found, err := moduleDisclosureWordingFindings("planted.json", planted)

	if err != nil {
		t.Fatalf("planted manifest does not parse: %v", err)
	}

	want := []string{
		"planted.json: disclosure TestX reason: COORD, ruling",
		"planted.json: note 0: lane, BOARD",
	}

	if strings.Join(found, "\n") != strings.Join(want, "\n") {
		t.Fatalf("planted findings:\n  got  %q\n  want %q", found, want)
	}

	clean := []string{
		"the keyboard handler is slow; bankruptcy and rulers are not fleet words",
		"measured identically in two runs; removed if the host warms first calls",
		"a board of directors",
	}

	for _, text := range clean {
		if words := fleetWordsIn(text); len(words) > 0 {
			t.Errorf("clean text refused (%v): %q", words, text)
		}
	}
}
