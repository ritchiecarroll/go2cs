// handownAuditGuard_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"os/exec"
	"runtime"
	"strings"
	"testing"
)

// THE GUARD FOR src/check-handown-audit.ps1, the H6 completeness gate.
//
// That gate decides whether a migration's corpus may be ADOPTED: every hand-own in the re-measured
// census must carry a classified delta record in the migration's audit file. It runs once per
// migration, by hand, which makes it the most rot-prone kind of instrument in the tree -- a script
// nobody runs fails open, which is false-green route #6, and "once per migration" is as close to
// never as a gate gets. So it lands in src/ WITH a guard, the same ruling that put safe-push.sh's
// here (coordinator, mailbox 35b59e60e).
//
// It is CHEAP: the gate's own -SelfTest builds a hermetic repository in a temp directory and touches
// no network, no clone and nothing outside it. About a second.
//
// THE SKIP NAMES WHAT IT DID NOT MEASURE. A host with no PowerShell cannot run the gate either, so
// there is nothing this guard could assert there -- but an unmeasured arm reported as a pass is the
// class this whole family of files exists to prevent, so the skip says UNMEASURED and names both
// interpreters it looked for. A skip that names what it did not measure is not a disarm; a silent one
// is.
func TestHandownAuditGateSelfTest(t *testing.T) {
	// A POSIX spelling: the argument is read by PowerShell's -File, which accepts forward slashes on
	// every edition, and this test's working directory is src/go2cs.
	const script = "../check-handown-audit.ps1"

	shell, args, unmeasured := handownAuditShell()
	if shell == "" {
		t.Skip(unmeasured)
	}
	t.Logf("driving src/check-handown-audit.ps1 through %s", shell)

	out, err := exec.Command(shell, append(args, script, "-SelfTest")...).CombinedOutput()
	text := string(out)
	if err != nil {
		t.Fatalf("src/check-handown-audit.ps1 -SelfTest failed: %v\n%s", err, text)
	}

	if !strings.Contains(text, "SELF-TEST CLEAN") {
		t.Fatalf("src/check-handown-audit.ps1 -SelfTest did not report a clean run:\n%s", text)
	}

	// The ARM COUNT, because an exit code cannot distinguish a suite that ran eleven arms from one
	// that silently lost ten.
	const wantArms = 11
	if got := strings.Count(text, "\n  ok   "); got != wantArms {
		t.Fatalf("expected %d passing arms from src/check-handown-audit.ps1 -SelfTest, counted %d -- an arm that quietly stops running is exactly what this count exists to catch:\n%s",
			wantArms, got, text)
	}

	// Each arm's REASON, not merely its count. Three of these carry the weight: the COMPLETE arm is
	// what stops a gate that refuses everything from passing every red arm; the work-item PASS arm is
	// the bound that stops the work-item refusal from blanketing every `c`; and the exit-2 arm is what
	// keeps a census that scanned nothing from ever reading as a clean audit.
	for _, reason := range []string{
		"a COMPLETE audit PASSES",
		"a marked path with NO row REFUSES and NAMES it",
		"a path listed TWICE REFUSES",
		"a class outside unchanged/a/b/c REFUSES",
		"a BLANK class (the skeleton state) REFUSES",
		"class b with NO reason REFUSES",
		"class c whose reason names NO work item REFUSES",
		"class c WITH a work item PASSES",
		`a row in the "no .auto emitted" state REFUSES`,
		"class c whose work item is a LADDER RUNG passes",
		"a census that finds NOTHING exits 2, not 0",
	} {
		if !strings.Contains(text, reason) {
			t.Errorf("the self-test no longer asserts the reason %q -- an arm asserts the REASON it failed, or it is not a control:\n%s", reason, text)
		}
	}
}

// handownAuditShell returns the PowerShell that can drive the gate on this host, the leading arguments
// it needs, or "" and the reason this guard is UNMEASURED here.
//
// Windows PowerShell FIRST where it exists: the corpus tooling beside this script (handown-census.ps1,
// check-roster-format.ps1) is what the gate's predicate is shared with, and that is the edition those
// are run under. pwsh is the fallback, and is the only option on a Linux lane.
//
// -ExecutionPolicy is passed only to powershell.exe. pwsh accepts it on Windows and REJECTS it
// elsewhere, and a guard that fails on a Linux lane for an argument the script does not need would
// read exactly like a broken gate.
func handownAuditShell() (string, []string, string) {
	if runtime.GOOS == "windows" {
		if p, err := exec.LookPath("powershell.exe"); err == nil {
			return p, []string{"-NoProfile", "-ExecutionPolicy", "Bypass", "-File"}, ""
		}
	}
	if p, err := exec.LookPath("pwsh"); err == nil {
		return p, []string{"-NoProfile", "-File"}, ""
	}
	return "", nil, "neither powershell.exe nor pwsh is on PATH, so src/check-handown-audit.ps1 cannot be exercised here -- " +
		"this guard is UNMEASURED on this host, not passing. A host without either cannot run the H6 gate itself."
}
