// cgoConfigurationClass_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"strings"
	"testing"
)

// cgoConfigurationClass names a verdict whose divergence is the residue of the corpus and the
// oracle being pinned to DIFFERENT cgo configurations for one seam -- the converted side is
// behaviourally a cgo-LINKED build there, the oracle is pinned cgo-OFF (the state of record on
// every platform since 2026-09-03), and Go's own test source turns that difference into a skip.
// Its shape is therefore Go=pass / C#=skip, exactly platform-skip's, and it was written into
// three committed syscall entries for precisely that shape -- where it could never fire, because
// matchTerminalStatuses unlocked the skip shape for platformSkipClass ALONE and sent every other
// class to the generic arm, which requires C#=fail. It stayed invisible for as long as the Linux
// bank ran cgo ON, where Go skips those tests too (the ENOTSUP coincidence) and they matched
// skip/skip; the cgo-OFF ruling is what made the class live and the 2026-09-03 leveling re-sweep
// is what read it, four verdicts unabsorbed on a banked row.
//
// The remedy is to ADMIT the class rather than re-label the entries (coordinator ruling, mailbox
// 82ec6654c): the class name carries WHY, and re-labelling to platform-skip would throw that
// away. These guards hold the admission to exactly one shape, with the misspelled-class control
// the ruling asked for -- because an admission that fires on a name nobody checks is how the
// original defect got in.

func cgoConfigDisclosure(name, class, signature string) map[string]testDisclosure {
	return map[string]testDisclosure{
		name: {Name: name, Class: class, Signature: signature, Reason: "because " + name},
	}
}

// The POSITIVE arm: the admitted class absorbs the pass/skip pair when the pinned signature is in
// the C# side's own skip output -- the UPSTREAM message Go's source writes, which is what keeps a
// harness-injected skip from being laundered through the class.
func TestCgoConfigurationAbsorbsThePassSkipShape(t *testing.T) {
	names := []string{"TestAllThreadsSyscall"}
	goResults := map[string]string{"TestAllThreadsSyscall": "pass"}
	csResults := map[string]string{"TestAllThreadsSyscall": "skip"}
	csOutputs := map[string]string{"TestAllThreadsSyscall": "AllThreadsSyscall disabled with cgo"}
	disclosures := cgoConfigDisclosure("TestAllThreadsSyscall", cgoConfigurationClass, "AllThreadsSyscall disabled with cgo")

	mismatches, _, disclosed, _ := matchTerminalStatuses(names, goResults, csResults, disclosures, csOutputs)

	if len(mismatches) != 0 {
		t.Fatalf("an admitted cgo-configuration pass/skip pair must not read as a mismatch; got %v", mismatches)
	}
	if len(disclosed) != 1 || disclosed[0] != "TestAllThreadsSyscall" {
		t.Fatalf("the pair must be absorbed as disclosed; got %v", disclosed)
	}
}

// NEGATIVE CONTROL 1, the one the ruling names: the SAME pair with the class name MISSPELLED must
// stay unabsorbed. This is the control that would have caught the original defect -- a class name
// nothing validates is a guard that cannot go green.
func TestMisspelledClassDoesNotAbsorbThePassSkipShape(t *testing.T) {
	names := []string{"TestAllThreadsSyscall"}
	goResults := map[string]string{"TestAllThreadsSyscall": "pass"}
	csResults := map[string]string{"TestAllThreadsSyscall": "skip"}
	csOutputs := map[string]string{"TestAllThreadsSyscall": "AllThreadsSyscall disabled with cgo"}
	disclosures := cgoConfigDisclosure("TestAllThreadsSyscall", "cgo-configuraton", "AllThreadsSyscall disabled with cgo")

	mismatches, _, disclosed, _ := matchTerminalStatuses(names, goResults, csResults, disclosures, csOutputs)

	if len(disclosed) != 0 {
		t.Fatalf("a class name the pipeline does not know must absorb nothing; got %v", disclosed)
	}
	if len(mismatches) != 1 || !strings.Contains(mismatches[0], "TestAllThreadsSyscall") {
		t.Fatalf("the pair must read as a mismatch naming the test; got %v", mismatches)
	}
}

// NEGATIVE CONTROL 2: the admitted class with a signature the C# skip output does not carry. The
// row has MOVED -- it skipped for some other reason -- and moving is what the pin exists to catch.
func TestCgoConfigurationRefusesAnUnmatchedSkipSignature(t *testing.T) {
	names := []string{"TestAllThreadsSyscall"}
	goResults := map[string]string{"TestAllThreadsSyscall": "pass"}
	csResults := map[string]string{"TestAllThreadsSyscall": "skip"}
	csOutputs := map[string]string{"TestAllThreadsSyscall": "skipping: no cgroup2 mount"}
	disclosures := cgoConfigDisclosure("TestAllThreadsSyscall", cgoConfigurationClass, "AllThreadsSyscall disabled with cgo")

	mismatches, _, disclosed, _ := matchTerminalStatuses(names, goResults, csResults, disclosures, csOutputs)

	if len(disclosed) != 0 {
		t.Fatalf("a skip whose text does not carry the pinned signature must not be absorbed; got %v", disclosed)
	}
	if len(mismatches) != 1 || !strings.Contains(mismatches[0], cgoConfigurationClass) {
		t.Fatalf("the mismatch must name the class whose signature failed; got %v", mismatches)
	}
}

// NEGATIVE CONTROL 3, the anti-laundering property platform-skip already carries and the admitted
// class must carry identically: ONE shape only. A cgo-configuration row whose C# side FAILS has
// moved, and must not be absorbed through the generic arm even when the failure text happens to
// contain the pinned signature -- otherwise the class becomes a second way to disclose a failure.
func TestCgoConfigurationAdmitsExactlyOneShape(t *testing.T) {
	names := []string{"TestAllThreadsSyscall"}
	goResults := map[string]string{"TestAllThreadsSyscall": "pass"}
	csResults := map[string]string{"TestAllThreadsSyscall": "fail"}
	csOutputs := map[string]string{"TestAllThreadsSyscall": "AllThreadsSyscall disabled with cgo"}
	disclosures := cgoConfigDisclosure("TestAllThreadsSyscall", cgoConfigurationClass, "AllThreadsSyscall disabled with cgo")

	mismatches, _, disclosed, _ := matchTerminalStatuses(names, goResults, csResults, disclosures, csOutputs)

	if len(disclosed) != 0 {
		t.Fatalf("the class admits the skip shape ALONE; a failure must never be laundered through it; got %v", disclosed)
	}
	if len(mismatches) != 1 {
		t.Fatalf("the failing pair must read as a mismatch; got %v", mismatches)
	}
}

// And the class that was already admitted stays admitted, unchanged: the arm gained a member, it
// did not move.
func TestPlatformSkipStillAbsorbsThePassSkipShape(t *testing.T) {
	names := []string{"TestGCMAsm"}
	goResults := map[string]string{"TestGCMAsm": "pass"}
	csResults := map[string]string{"TestGCMAsm": "skip"}
	csOutputs := map[string]string{"TestGCMAsm": "skipping: gcmAsm not available"}
	disclosures := cgoConfigDisclosure("TestGCMAsm", platformSkipClass, "gcmAsm not available")

	mismatches, _, disclosed, _ := matchTerminalStatuses(names, goResults, csResults, disclosures, csOutputs)

	if len(mismatches) != 0 || len(disclosed) != 1 {
		t.Fatalf("platform-skip's own admission must be untouched; mismatches=%v disclosed=%v", mismatches, disclosed)
	}
}
