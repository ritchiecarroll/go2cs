// safePushGuard_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

package main

import (
	"os/exec"
	"path/filepath"
	"strings"
	"testing"
)

// src/safe-push.sh is the push/announce/security composition (coordinator ruling, 2026-09-06). It
// exists because on that day four participants -- including the two who had written the rule down --
// each composed those three steps wrongly at least once: a census placed in the same command as the
// push it gated, a rejected push reporting rc=0 through a pipe, a --force-with-lease SHA expanded
// from a nine-character prefix, and an `echo` asserting "silence above = clean" over output that
// falsified it. In every case the instrument was correct and the composition made its verdict inert.
//
// A script nobody runs fails open, which is false-green route #6, so the ruling was that it lands in
// src/ WITH A GUARD. This is the guard, and it lives here for the same reason
// projitemsIntegrity_test.go and fleetIdentifierCensus_test.go do: the converter's own `go test ./...`
// is the one gate every lane already pays for.
//
// ⚠ IT NESTS, DELIBERATELY AND AT A MEASURED PRICE. The self-test's positive control is a REAL push
// to a hermetic bare repository, and a real push runs the script's security gate, which invokes
// `go test` on this package -- so this test spawns bash, which spawns go test. About 25 s on a 215 s
// suite, roughly 10%, paid by every lane on every run.
//
// The cheaper alternative was to run only the arms that need no network and leave the real-push arm
// to lanes. It was REFUSED, and by the finding that produced this file: the self-test's own earlier
// version reached only --dry-run, so it guarded everything except the push path the script exists
// for. Adopting that here would re-commit, knowingly and by name, the defect the instrument was
// built to close. A cheaper guard that omits the dangerous path is not a cheaper guard; it is a
// weaker one wearing the same name. The cost is reducible later by narrowing the inner invocation --
// an optimisation, not a design change, and not a reason to defer the arm.
func TestSafePushSelfTest(t *testing.T) {
	script := filepath.Join("..", "safe-push.sh")

	// bash is how every lane already drives git in this fleet, on Windows through Git Bash and
	// natively elsewhere. A host without it cannot run the composition either, so there is nothing
	// this guard could assert about it -- but the skip NAMES itself rather than passing quietly,
	// because an unmeasured arm reported as a pass is the class this whole file is about.
	if _, err := exec.LookPath("bash"); err != nil {
		t.Skip("bash is not on PATH, so src/safe-push.sh cannot be exercised here -- this guard is UNMEASURED on this host, not passing")
	}

	out, err := exec.Command("bash", script, "--self-test").CombinedOutput()
	text := string(out)

	if err != nil {
		t.Fatalf("src/safe-push.sh --self-test failed: %v\n%s", err, text)
	}

	// The verdict line, and then the ARM COUNT -- because an exit code cannot distinguish a suite
	// that ran ten arms from one that silently lost nine of them, which is the count-match lesson
	// this repository has already paid for in a GolibTests reading taken from a stale tree.
	if !strings.Contains(text, "SELF-TEST CLEAN") {
		t.Fatalf("src/safe-push.sh --self-test did not report a clean run:\n%s", text)
	}

	const wantArms = 10
	if got := strings.Count(text, "\n  ok   "); got != wantArms {
		t.Fatalf("expected %d passing arms from src/safe-push.sh --self-test, counted %d -- an arm that quietly stops running is exactly what this count exists to catch:\n%s",
			wantArms, got, text)
	}

	// Each arm's REASON, not merely its count. An earlier version of that suite had three arms
	// aborting on an unrelated branch-existence check before reaching the validation they existed to
	// test, and "it refused" read as proof the check worked while the check never executed. A red
	// that goes red for the wrong reason looks exactly like a control working, which makes it worse
	// than one that never fires. These are the reasons, so a rewrite that keeps the arm names and
	// loses their meaning fails here.
	for _, reason := range []string{
		"never expanded from a prefix",
		"does not resolve to a commit",
		"not a hex object name",
		"Pass --new if that is intended",
		"Announce the new SHA",
		"announce-then-push protects nobody",
		"WITHOUT evaluating --force-with-lease",
		"SAFEPUSH OK",
		"push failed",
		"cmd/go DOES cache this invocation",
	} {
		if !strings.Contains(text, reason) {
			t.Errorf("the self-test no longer asserts the reason %q -- an arm asserts the REASON it failed, or it is not a control:\n%s", reason, text)
		}
	}
}
