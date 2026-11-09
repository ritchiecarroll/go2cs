// testSignals_notunix.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

//go:build !unix

package main

import "os"

// signalsToIgnore is empty off unix: the harness's signal parity with cmd/go addresses a unix SIG_IGN inheritance,
// and Windows' Ctrl-C handling is left exactly as it was (testSignals.go).
var signalsToIgnore []os.Signal
