// testSignals.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"errors"
	"os"
	"os/exec"
	"os/signal"
	"sync"
)

// The -tests harness's signals, ported from cmd/go. `go test` calls base.StartSigHandlers before it runs a test
// binary (cmd/go/internal/base/signal.go, test.go:1695, go1.26.10), which Notifies signalsToIgnore. Two things follow,
// and the harness needs both:
//
//   - A signal the parent HANDLES is reset to SIG_DFL in an exec'd child. So Go's test binary starts SIGINT and SIGQUIT
//     at the default even when the shell that launched the run ignored them (an async `cmd &` in a non-interactive
//     bash ignores both). go2cs launched the converted host with whatever it inherited, so the two sides of one
//     comparison started with different dispositions: os's TestProcessWithHandleLinux read Go pass / C# fail on
//     exactly that (the 1.26.10.3 linux re-bank). Only SIGINT's ignore actually reaches a Go process's children: the
//     runtime respects an inherited SIG_IGN for SIGHUP and SIGINT only, and installs its own SIGQUIT handler. Both are
//     Notified anyway, because that is cmd/go's list.
//   - Once go2cs handles an interrupt it must act on one, as cmd/go acts on base.Interrupted. The pipeline's children
//     run in their own process groups (processGroupUnix.go), so a terminal's Ctrl-C or Ctrl-\ reaches go2cs alone.
//     go2cs therefore kills every running child's whole group, the same kill the package deadline uses, and exits 130.
//     Before this, an interrupt killed go2cs and ORPHANED its children.
//
// Unix only (testSignals_unix.go). On Windows the list is empty and nothing here changes: the asymmetry is a unix
// SIG_IGN inheritance.

// testInterrupted is closed when go2cs receives one of signalsToIgnore during a -tests run (cmd/go's base.Interrupted).
var testInterrupted = make(chan struct{})

// errTestInterrupted is what the launcher answers once an interrupt has arrived, instead of starting a new child.
var errTestInterrupted = errors.New("interrupted")

// testChildren is every child the shared launcher is running, with the group that kills it.
var testChildren = struct {
	sync.Mutex
	running     map[*exec.Cmd]*processGroup
	interrupted bool
}{running: map[*exec.Cmd]*processGroup{}}

var startTestSigHandlersOnce = sync.OnceFunc(func() {
	if len(signalsToIgnore) == 0 {
		return
	}

	sig := make(chan os.Signal, 1)
	signal.Notify(sig, signalsToIgnore...)

	go func() {
		<-sig
		close(testInterrupted)

		testChildren.Lock()
		testChildren.interrupted = true
		for cmd, group := range testChildren.running {
			_ = group.kill(cmd)
		}
		testChildren.Unlock()

		os.Exit(130)
	}()
})

// startTestSigHandlers is cmd/go's base.StartSigHandlers for the -tests harness: called by the shared launcher before
// its first child, once.
func startTestSigHandlers() { startTestSigHandlersOnce() }

// startTrackedChild starts cmd and records it, under the same lock the interrupt takes, so an interrupt either finds
// the child (and kills its group) or arrives first (and the child is never started). The returned untrack runs after
// the child has been waited for.
func startTrackedChild(cmd *exec.Cmd, group *processGroup) (untrack func(), err error) {
	testChildren.Lock()
	defer testChildren.Unlock()

	if testChildren.interrupted {
		return func() {}, errTestInterrupted
	}

	if err := cmd.Start(); err != nil {
		return func() {}, err
	}

	testChildren.running[cmd] = group

	return func() {
		testChildren.Lock()
		delete(testChildren.running, cmd)
		testChildren.Unlock()
	}, nil
}
