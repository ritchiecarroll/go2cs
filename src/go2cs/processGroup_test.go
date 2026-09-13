// processGroup_test.go - Gbtc
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
	"strings"
	"testing"
	"time"
)

// The two environment markers that select a re-exec ROLE. The guard below runs this very test
// binary three deep — guard, parent, grandchild — which is not a convenience: it is the SHAPE of
// the defect. The converted test host re-execs ITSELF for the subprocess-style rows Go's suites
// carry (an environment marker plus -test.run), so a helper that spawned some other program would
// be testing a different thing than the one that orphaned six processes.
const (
	orphanParentHeartbeatEnv = "GO2CS_ORPHAN_PARENT_HEARTBEAT"
	orphanChildHeartbeatEnv  = "GO2CS_ORPHAN_CHILD_HEARTBEAT"
)

// orphanGuardDeadline is the package deadline the guard hands runCommandWithTimeoutEnv. Sized for
// the SLOWEST legitimate host rather than the fastest: the whole measurement depends on the
// grandchild being alive before the kill, and a deadline that fires first would report an
// instrument failure on a loaded box. It is a safety net, never a performance assumption.
const orphanGuardDeadline = 10 * time.Second

// TestDeadlineKillTakesTheChildsDescendants is the standing guard for the orphan the -tests
// pipeline measured: the package deadline SIGKILLs the converted host from outside while a test
// goroutine is blocked before its deferred cmd.Process.Kill(), and the host's re-exec'd child
// survives with nobody left to signal it — six alive at once on one row.
//
// It measures liveness by HEARTBEAT rather than by pid, deliberately. A pid probe needs a different
// primitive per platform and cannot tell a live process from a reused pid; a file the grandchild
// appends to for its whole life answers "is it still RUNNING" directly, in one portable read, and
// the answer cannot be faked by a process that has already exited.
//
// The anti-vacuity check is the load-bearing half: an EMPTY heartbeat fails the test. Without it a
// grandchild that never started would produce exactly the reading a successful kill produces — no
// growth — and the guard would pass while measuring nothing, which is the false-green shape this
// repository names routes #1 through #8 after.
func TestDeadlineKillTakesTheChildsDescendants(t *testing.T) {
	if orphanHelperRole() {
		t.Skip("re-exec helper role; the guard itself runs only in the top-level process")
	}

	dir := t.TempDir()
	heartbeat := filepath.Join(dir, "heartbeat")

	start := time.Now()
	output, err := runCommandWithTimeoutEnv(orphanGuardDeadline, dir, Options{},
		[]string{orphanParentHeartbeatEnv + "=" + heartbeat},
		os.Args[0], "-test.run=^TestOrphanHelperParent$", "-test.timeout=0")
	elapsed := time.Since(start)

	if err == nil || !strings.Contains(err.Error(), "timed out") {
		t.Fatalf("expected the package deadline to fire and kill the helper; err=%v elapsed=%s output=%q", err, elapsed, output)
	}

	first, readErr := os.ReadFile(heartbeat)
	if readErr != nil || len(first) == 0 {
		t.Fatalf("the grandchild never wrote a heartbeat (read %d bytes, err=%v) — this guard cannot "+
			"tell a killed descendant from one that never ran, so the run measured nothing; helper output=%q",
			len(first), readErr, output)
	}

	// Several beat intervals, so a survivor cannot be mistaken for a corpse by arriving late.
	time.Sleep(15 * orphanHeartbeatInterval)

	second, readErr := os.ReadFile(heartbeat)
	if readErr != nil {
		t.Fatalf("heartbeat unreadable on the second sample: %v", readErr)
	}

	if len(second) != len(first) {
		t.Fatalf("the descendant SURVIVED the deadline kill: heartbeat grew %d -> %d bytes after the "+
			"helper was killed, so the kill reached the child and not its process group", len(first), len(second))
	}
}

// orphanHeartbeatInterval is how often the grandchild proves it is alive.
const orphanHeartbeatInterval = 100 * time.Millisecond

// orphanHelperRole reports whether this process was re-exec'd as one of the guard's helpers.
func orphanHelperRole() bool {
	return os.Getenv(orphanParentHeartbeatEnv) != "" || os.Getenv(orphanChildHeartbeatEnv) != ""
}

// TestOrphanHelperParent stands in for the converted test host: it starts a descendant and then
// BLOCKS past the deadline without reaping it — the exact state a test goroutine is in when it is
// waiting on something before its deferred Kill. Skipped in an ordinary run.
func TestOrphanHelperParent(t *testing.T) {
	heartbeat := os.Getenv(orphanParentHeartbeatEnv)
	if heartbeat == "" {
		t.Skip("helper: runs only when re-exec'd by TestDeadlineKillTakesTheChildsDescendants")
	}

	child := exec.Command(os.Args[0], "-test.run=^TestOrphanHelperChild$", "-test.timeout=0")
	child.Env = append(os.Environ(), orphanChildHeartbeatEnv+"="+heartbeat)

	// The grandchild must NOT inherit this process's stdout/stderr. The guard captures them through
	// a pipe, and a descendant holding the write end open would block the guard's own Wait after the
	// kill — a DIFFERENT failure from the one under test, and one that would read as a hang rather
	// than as an orphan. nil means the null device, which is what Go's own helper-process tests use.
	child.Stdout = nil
	child.Stderr = nil

	if err := child.Start(); err != nil {
		t.Fatalf("helper could not start its descendant: %v", err)
	}
	fmt.Printf("orphan helper: descendant pid %d\n", child.Process.Pid)

	// Wait until the descendant is demonstrably ALIVE before blocking, so the guard's measurement
	// does not race process startup on a loaded host. If it never comes up the guard says so.
	deadline := time.Now().Add(orphanGuardDeadline - 2*time.Second)
	for time.Now().Before(deadline) {
		if info, err := os.Stat(heartbeat); err == nil && info.Size() > 0 {
			break
		}
		time.Sleep(orphanHeartbeatInterval)
	}

	// Block well past the package deadline. Sleeping rather than select{}: a process whose every
	// goroutine is parked forever trips Go's deadlock detector and would exit on its own, which is
	// the one outcome that would make the kill unnecessary and the measurement meaningless.
	time.Sleep(2 * time.Minute)
}

// TestOrphanHelperChild is the descendant whose survival the guard measures. It appends one byte per
// interval for its whole life, so "still running" is a file that is still growing. Skipped in an
// ordinary run.
func TestOrphanHelperChild(t *testing.T) {
	heartbeat := os.Getenv(orphanChildHeartbeatEnv)
	if heartbeat == "" {
		t.Skip("helper: runs only when re-exec'd by TestOrphanHelperParent")
	}

	file, err := os.OpenFile(heartbeat, os.O_CREATE|os.O_WRONLY|os.O_APPEND, 0644)
	if err != nil {
		t.Fatalf("descendant could not open its heartbeat: %v", err)
	}
	defer file.Close()

	// A self-imposed ceiling so a RED run — the whole point of which is that nobody kills this
	// process — cannot leave a stray behind for longer than the guard needs.
	stop := time.Now().Add(30 * time.Second)
	for time.Now().Before(stop) {
		if _, err := file.Write([]byte{'.'}); err != nil {
			return
		}
		time.Sleep(orphanHeartbeatInterval)
	}
}
