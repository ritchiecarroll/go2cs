// publishTimeout_test.go - Gbtc
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
	"time"
)

// The converted test host's `dotnet publish` budget.
//
// -test-timeout is the PACKAGE deadline (2m by default) and it also bounded the host's publish. The
// first publish on a fresh tree builds the test project's whole standard-library closure, which takes
// far longer than two minutes, so every package of a first run on a fresh checkout read "dotnet timed
// out after 2m0s" (the first x/sync and x/mod module runs, 2026-10-02). These tests pin that the
// publish gets a floor the default cannot undercut, that a larger -test-timeout still raises it, and
// that publishTestHost really uses it.

// TestPublishTimeoutHasAFloor pins the budget: never below the floor, and -test-timeout when larger.
func TestPublishTimeoutHasAFloor(t *testing.T) {
	cases := []struct {
		testTimeout time.Duration
		want        time.Duration
	}{
		{2 * time.Minute, 30 * time.Minute}, // the flag's default: a first publish cannot finish in it
		{30 * time.Minute, 30 * time.Minute},
		{45 * time.Minute, 45 * time.Minute}, // a long suite's deadline still raises the publish budget
	}

	for _, c := range cases {
		if got := testPublishTimeout(Options{testTimeout: c.testTimeout}); got != c.want {
			t.Errorf("testPublishTimeout(-test-timeout %s) = %s, want %s", c.testTimeout, got, c.want)
		}
	}
}

// TestPublishTestHostUsesThePublishBudget pins the call site: the floor is worth nothing unless the
// publish is the command it bounds.
func TestPublishTestHostUsesThePublishBudget(t *testing.T) {
	source := readConverterSource(t, "testConversion.go")

	const call = `runCommandWithTimeout(testPublishTimeout(options), outputPath, options, "dotnet", args...)`

	if !strings.Contains(source, call) {
		t.Errorf("publishTestHost does not run dotnet publish under testPublishTimeout (looked for %q)", call)
	}
}
