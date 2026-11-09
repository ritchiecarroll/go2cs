// testSignals_linux_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

//go:build linux

package main

import (
	"errors"
	"os"
	"os/exec"
	"path/filepath"
	"strconv"
	"strings"
	"syscall"
	"testing"
	"time"
)

// THE -tests HARNESS'S SIGNAL PARITY WITH cmd/go. `go test` calls base.StartSigHandlers, which Notifies SIGINT and
// SIGQUIT (cmd/go/internal/base/signal_unix.go, go1.26.10). A signal the parent HANDLES is reset to SIG_DFL in an
// exec'd child, so Go's test binary starts at the default disposition for both even when the shell that launched
// the run ignored them (an async `cmd &` in a non-interactive bash ignores both: SigIgn 0x6). go2cs launched the
// converted host with whatever it inherited, so the two sides of one comparison started with different dispositions.
// os's TestProcessWithHandleLinux read Go pass / C# fail on exactly that (the 1.26.10.3 linux re-bank, 2026-11-08).

// sigIgnOf reads the SigIgn mask from a /proc/<pid>/status text.
func sigIgnOf(t *testing.T, status string) uint64 {
	t.Helper()

	for _, line := range strings.Split(status, "\n") {
		if value, ok := strings.CutPrefix(line, "SigIgn:"); ok {
			mask, err := strconv.ParseUint(strings.TrimSpace(value), 16, 64)
			if err != nil {
				t.Fatalf("SigIgn %q: %v", value, err)
			}

			return mask
		}
	}

	t.Fatalf("no SigIgn line in:\n%s", status)
	return 0
}

const (
	sigintBit  = uint64(1) << (syscall.SIGINT - 1)
	sigquitBit = uint64(1) << (syscall.SIGQUIT - 1)
)

// The helper half of TestTestsLaunchStartsChildWithDefaultInterruptAndQuit: this test binary, started by a shell
// that ignores INT and QUIT, so it INHERITS the ignore exactly as go2cs does under `cmd &`. It prints its own SigIgn
// (the premise) and that of a child it launches through the shared -tests launcher (the verdict).
//
// Why a helper, and not signal.Ignore in the test binary: once a Go process ignores SIGINT, os/signal cannot
// un-ignore it. Notify records the current disposition (SIG_IGN) as the one to forward to, and for SIGHUP and SIGINT
// the runtime respects an inherited SIG_IGN (sigInstallGoHandler), so a later Reset puts the ignore back. The first
// run of these guards measured that: Ignore plus Reset left SIGINT ignored (SigIgn 0x2), and the next test's helper
// inherited it. Running in its own process, this guard leaks nothing.
func TestDispositionHelperProcess(t *testing.T) {
	if os.Getenv("GO2CS_SIGNAL_DISPOSITION_HELPER") != "1" {
		t.Skip("helper process only")
	}

	self, err := os.ReadFile("/proc/self/status")
	if err != nil {
		os.Exit(2)
	}

	output, err := runCommandWithTimeoutEnv(30*time.Second, os.TempDir(), Options{}, nil, "cat", "/proc/self/status")
	if err != nil {
		os.Exit(3)
	}

	println("SELF", strconv.FormatUint(sigIgnOf(t, string(self)), 16))
	println("CHILD", strconv.FormatUint(sigIgnOf(t, output), 16))
	os.Exit(0)
}

// Under an inherited ignore of SIGINT and SIGQUIT, a child launched through the shared -tests launcher starts with
// both at the DEFAULT disposition, as `go test`'s test binary does.
func TestTestsLaunchStartsChildWithDefaultInterruptAndQuit(t *testing.T) {
	helper := exec.Command("sh", "-c", `trap '' INT QUIT; exec "$0" -test.run='^TestDispositionHelperProcess$' -test.count=1`, os.Args[0])
	helper.Env = append(os.Environ(), "GO2CS_SIGNAL_DISPOSITION_HELPER=1")

	out, err := helper.CombinedOutput()
	if err != nil {
		t.Fatalf("the helper failed: %v\n%s", err, out)
	}

	masks := map[string]uint64{}
	for _, line := range strings.Split(string(out), "\n") {
		if label, hex, ok := strings.Cut(strings.TrimSpace(line), " "); ok && (label == "SELF" || label == "CHILD") {
			mask, err := strconv.ParseUint(hex, 16, 64)
			if err != nil {
				t.Fatalf("%s %q: %v", label, hex, err)
			}

			masks[label] = mask
		}
	}

	self, okSelf := masks["SELF"]
	child, okChild := masks["CHILD"]
	if !okSelf || !okChild {
		t.Fatalf("the helper printed no SELF/CHILD lines:\n%s", out)
	}

	// The shell ignores both, but only SIGINT's ignore survives into a Go process: the runtime respects an inherited
	// SIG_IGN for SIGHUP and SIGINT only (sigInstallGoHandler) and installs its own SIGQUIT handler regardless
	// (measured: the helper reads SigIgn 0x2). So under `cmd &`, go2cs's children already start SIGQUIT at the default;
	// the live asymmetry is SIGINT. The verdict below still requires both clear, which is cmd/go's guarantee.
	if self&sigintBit == 0 {
		t.Fatalf("premise: the helper should inherit the ignore of SIGINT, SigIgn %#x", self)
	}

	if child&(sigintBit|sigquitBit) != 0 {
		t.Fatalf("the launched child inherited the ignore: SigIgn %#x (SIGINT bit %v, SIGQUIT bit %v); `go test`'s binary starts both at SIG_DFL",
			child, child&sigintBit != 0, child&sigquitBit != 0)
	}
}

// The helper half of TestInterruptKillsTheRunningChildGroupAndExits130: it is this test binary, re-executed, running
// one long child through the shared launcher. The child writes its pid (its process GROUP id: it is the leader) to
// a file, then sleeps.
func TestInterruptHelperProcess(t *testing.T) {
	pidFile := os.Getenv("GO2CS_SIGNAL_HELPER_PIDFILE")
	if pidFile == "" {
		t.Skip("helper process only")
	}

	_, _ = runCommandWithTimeoutEnv(5*time.Minute, filepath.Dir(pidFile), Options{}, nil,
		"sh", "-c", "echo $$ > "+pidFile+"; exec sleep 300")
	os.Exit(0)
}

// A SIGINT to a go2cs -tests run kills the running child's whole process GROUP and exits 130, as cmd/go does on
// base.Interrupted. Before the parity change, SIGINT killed go2cs by signal and ORPHANED the child: it sits in its
// own process group (processGroupUnix.go), so the terminal's interrupt never reached it.
func TestInterruptKillsTheRunningChildGroupAndExits130(t *testing.T) {
	pidFile := filepath.Join(t.TempDir(), "child.pid")
	helper := exec.Command(os.Args[0], "-test.run=^TestInterruptHelperProcess$", "-test.count=1")
	helper.Env = append(os.Environ(), "GO2CS_SIGNAL_HELPER_PIDFILE="+pidFile)

	if err := helper.Start(); err != nil {
		t.Fatal(err)
	}

	var childGroup int
	deadline := time.Now().Add(30 * time.Second)

	for childGroup == 0 && time.Now().Before(deadline) {
		if data, err := os.ReadFile(pidFile); err == nil {
			childGroup, _ = strconv.Atoi(strings.TrimSpace(string(data)))
		}

		time.Sleep(50 * time.Millisecond)
	}

	if childGroup == 0 {
		_ = helper.Process.Kill()
		t.Fatal("premise: the helper's child never started")
	}

	// Whatever happens below, neither the long child nor the helper may outlive the test.
	defer func() { _ = syscall.Kill(-childGroup, syscall.SIGKILL) }()
	defer func() { _ = helper.Process.Kill() }()

	// Let the launcher settle past Start.
	time.Sleep(500 * time.Millisecond)

	// Premise: the helper (go2cs) must not itself be ignoring SIGINT. If it is, this test measures its parent's
	// leaked disposition, not the harness (the first run did exactly that).
	procStatus, err := os.ReadFile("/proc/" + strconv.Itoa(helper.Process.Pid) + "/status")
	if err != nil {
		t.Fatal(err)
	}

	if mask := sigIgnOf(t, string(procStatus)); mask&sigintBit != 0 {
		t.Fatalf("premise: the helper ignores SIGINT (SigIgn %#x); a signal disposition leaked from an earlier test", mask)
	}

	// Interrupt go2cs (the helper) as a terminal would.
	if err := helper.Process.Signal(syscall.SIGINT); err != nil {
		t.Fatal(err)
	}

	waited := make(chan error, 1)
	go func() { waited <- helper.Wait() }()

	select {
	case err = <-waited:
	case <-time.After(60 * time.Second):
		t.Fatal("the helper did not end within 60 s of SIGINT")
	}

	var exitErr *exec.ExitError
	if !errors.As(err, &exitErr) {
		t.Fatalf("the helper exited %v; want exit status 130", err)
	}

	status := exitErr.Sys().(syscall.WaitStatus)
	if status.Signaled() || status.ExitStatus() != 130 {
		t.Errorf("the helper ended %v (signaled %v, exit %d); want a clean exit 130, as cmd/go after an interrupt",
			err, status.Signaled(), status.ExitStatus())
	}

	// The child's whole group must be gone: kill(-pgid, 0) answers ESRCH once no process is left in it.
	gone := false
	for wait := time.Now().Add(10 * time.Second); time.Now().Before(wait); time.Sleep(100 * time.Millisecond) {
		if errors.Is(syscall.Kill(-childGroup, 0), syscall.ESRCH) {
			gone = true
			break
		}
	}

	if !gone {
		t.Errorf("the child's process group %d is still alive after go2cs was interrupted: an orphan", childGroup)
	}
}
