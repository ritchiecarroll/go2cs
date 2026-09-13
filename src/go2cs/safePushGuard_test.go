// safePushGuard_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"fmt"
	"os"
	"os/exec"
	"path/filepath"
	"runtime"
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
// projitemsIntegrity_test.go and internal/repoguard/fleetIdentifierCensus_test.go do: the converter's
// own `go test ./...` is the one gate every lane already pays for.
//
// ⚠ IT NESTS, DELIBERATELY AND AT A MEASURED PRICE. The self-test's positive control is a REAL push
// to a hermetic bare repository, and a real push runs the script's security gate, which invokes
// `go test` on go2cs/internal/repoguard -- so this test spawns bash, which spawns go test. About 25 s on a 215 s
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
	// A POSIX spelling, not filepath.Join: the argument is read by bash, not by Windows. The host form
	// `..\safe-push.sh` happens to survive Git Bash, and reaches a WSL bash as `..safe-push.sh`.
	script := "../safe-push.sh"

	// bash is how every lane already drives git in this fleet, on Windows through Git Bash and
	// natively elsewhere. A host without a usable one cannot run the composition either, so there is
	// nothing this guard could assert about it -- but the skip NAMES itself rather than passing
	// quietly, because an unmeasured arm reported as a pass is the class this whole file is about.
	bash, unmeasured := safePushBash()
	if bash == "" {
		t.Skip(unmeasured)
	}
	t.Logf("driving src/safe-push.sh through %s", bash)

	out, err := exec.Command(bash, script, "--self-test").CombinedOutput()
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

// safePushBash returns the bash that can actually run src/safe-push.sh on this host, or "" and the
// reason this guard is UNMEASURED here.
//
// ⚠ ON WINDOWS, "SOME bash ON PATH" IS NOT "A bash THAT CAN RUN THIS SCRIPT" (measured 2026-09-12). A
// PowerShell session resolved `bash` to C:\Windows\System32\bash.exe -- WSL -- and the guard FAILED in
// ~5 s with a message reading exactly like a broken safe-push.sh, while the same tree with Git Bash
// first on PATH PASSED in ~44 s through a real hermetic push. A false red on the script that gates
// every fleet push, pointing away from its cause. Fixing only the path spelling does not rescue WSL:
// measured, it gets three arms in and dies on `fatal: not a git repository`, because a worktree's
// `.git` file names its gitdir as a Windows path Linux git cannot open; and a non-login WSL bash has
// no `go` for the security gate either (probed) -- the script would run against Linux's own git and
// toolchain, not this host's, which is a different measurement even where it succeeds.
//
// So on Windows the guard does not trust PATH order at all. It resolves Git for Windows' bash from
// the git this host actually uses (`git --exec-path` sits inside the install), and when there is no
// such bash it SKIPS as UNMEASURED naming what it looked for -- never a false red, and never a pass
// that was not earned by the script running.
func safePushBash() (string, string) {
	if runtime.GOOS != "windows" {
		if path, err := exec.LookPath("bash"); err == nil {
			return path, ""
		}
		return "", "bash is not on PATH, so src/safe-push.sh cannot be exercised here -- this guard is UNMEASURED on this host, not passing"
	}

	pathBash := "no bash"
	if p, err := exec.LookPath("bash"); err == nil {
		pathBash = p
	}
	unmeasured := func(why string) string {
		return fmt.Sprintf("%s, so Git Bash cannot be located and src/safe-push.sh cannot be exercised here -- "+
			"this guard is UNMEASURED on this host, not passing. PATH's bash (%s) is deliberately NOT used: "+
			"on Windows it may be WSL, which cannot run this script against this host's git and toolchain", why, pathBash)
	}

	git, err := exec.LookPath("git")
	if err != nil {
		return "", unmeasured("git is not on PATH")
	}
	out, err := exec.Command(git, "--exec-path").Output()
	if err != nil {
		return "", unmeasured(fmt.Sprintf("`%s --exec-path` failed (%v)", git, err))
	}
	execPath := strings.TrimSpace(string(out))
	if !filepath.IsAbs(execPath) {
		return "", unmeasured(fmt.Sprintf("`git --exec-path` answered %q, which is not a Windows path", execPath))
	}

	// <install>\mingw64\libexec\git-core: the install root is the nearest ancestor holding the MSYS
	// runtime's usr\bin\bash.exe. bin\bash.exe is preferred -- it is the launcher that puts the
	// install's own git on PATH, and the form the measured passing arm used.
	dir := filepath.Clean(execPath)
	for range 4 {
		parent := filepath.Dir(dir)
		if parent == dir {
			break
		}
		dir = parent
		runtimeBash := filepath.Join(dir, "usr", "bin", "bash.exe")
		if !isRegularFile(runtimeBash) {
			continue
		}
		if launcher := filepath.Join(dir, "bin", "bash.exe"); isRegularFile(launcher) {
			return launcher, ""
		}
		return runtimeBash, ""
	}
	return "", unmeasured(fmt.Sprintf("no Git for Windows install holding usr\\bin\\bash.exe was found above %s", execPath))
}

func isRegularFile(path string) bool {
	info, err := os.Stat(path)
	return err == nil && info.Mode().IsRegular()
}
