// testSignals_unix.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

//go:build unix

package main

import (
	"os"
	"syscall"
)

// signalsToIgnore is cmd/go's list (cmd/go/internal/base/signal_unix.go, go1.26.10): the signals the -tests harness
// handles, so its children start them at SIG_DFL and an interrupt ends the run cleanly (testSignals.go).
var signalsToIgnore = []os.Signal{os.Interrupt, syscall.SIGQUIT}
