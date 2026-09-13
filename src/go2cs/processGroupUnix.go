// processGroupUnix.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

//go:build unix

package main

import (
	"os/exec"
	"syscall"
)

// processGroup — unix flavour. The pipeline's children are spawned as process-group LEADERS
// (Setpgid), so the deadline kill can name the whole group with a negative pid and take the child's
// own descendants with it.
//
// Why this is needed at all: the converted test host RE-EXECS ITSELF for the subprocess-style rows
// Go's own suites carry (an environment marker plus -test.run, the GO_WANT_HELPER_PROCESS shape),
// and a test goroutine can be blocked BEFORE its deferred cmd.Process.Kill() when the pipeline's
// package deadline fires from outside. Killing the host alone then orphans that child — six alive
// at once on one measured run. The child answers every signal normally; nothing is wrong with it
// except that nobody is left to send one. So the remedy belongs at the LAUNCHER, which is here.
//
// Setpgid, deliberately NOT Setsid. setsid would detach the controlling terminal as well as change
// the group, and the pipeline has a row that depends on the terminal being there: the comparison
// record's `terminal` field is observed by opening /dev/tty, and Go's terminal-gated tests
// (syscall's TestForeground pair) skip themselves when it cannot be opened. Setpgid keeps the
// session — and therefore /dev/tty — and changes only what the kill needs changed.
//
// The residual it DOES carry, stated rather than assumed: a child in its own process group is no
// longer in the terminal's FOREGROUND group, so a descendant that READS from the terminal would
// take SIGTTIN where it previously did not. It applies identically to both sides of a comparison
// (the oracle `go test` and the converted host go through the same helper), so it cannot produce a
// cross-SIDE divergence — but it is a real exposure for a terminal-gated row and belongs in the
// record, not in a hope.
type processGroup struct{}

// newProcessGroup prepares cmd so its descendants can be killed as a unit. Called BEFORE Start.
// The error half exists for the Windows flavour, whose job object can be refused by the OS; unix
// asks the kernel for nothing here — setpgid is requested at fork time and cannot fail separately —
// so this half never returns one, and the shared caller needs no platform knowledge either way.
func newProcessGroup(cmd *exec.Cmd) (*processGroup, error) {
	if cmd.SysProcAttr == nil {
		cmd.SysProcAttr = &syscall.SysProcAttr{}
	}
	cmd.SysProcAttr.Setpgid = true
	return &processGroup{}, nil
}

// attach is the unix no-op half of the two-phase Windows contract: setpgid is requested at fork
// time by the SysProcAttr above, so there is nothing left to do once the child exists.
func (g *processGroup) attach(cmd *exec.Cmd) error { return nil }

// kill takes the child's whole process GROUP, then falls back to the single-process kill that was
// the behaviour before this existed. It is what cmd.Cancel runs when the package deadline expires,
// so the group goes first and the safety net stays behind it.
func (g *processGroup) kill(cmd *exec.Cmd) error {
	if cmd.Process == nil || cmd.Process.Pid <= 0 {
		return nil
	}

	// A negative pid names the group. The child IS its own group leader (Setpgid above), so the
	// group id equals its pid and this can never reach the converter's own group — the failure
	// mode that would otherwise make a group kill unusable here.
	if err := syscall.Kill(-cmd.Process.Pid, syscall.SIGKILL); err == nil {
		return nil
	}

	// The group kill only fails when the child was never placed in one (an exec that failed
	// between fork and setpgid) or the group is already gone. Either way the single-process kill
	// is the honest fallback, and it is exactly what exec.CommandContext would have done alone.
	return cmd.Process.Kill()
}

// close releases whatever the group needed to stay killable. Nothing on unix; the Windows half
// holds a job handle here.
func (g *processGroup) close() {}
