// testDeadline_test.go - Gbtc
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

// The converted host's per-test deadline (DESIGN-object-lifetime-disclosure.md OQ-L3(b)) reaches it through
// the environment, never argv, and "0" switches it off so a value in the launching shell is never inherited.
func TestHostEnvCarriesThePerTestDeadline(t *testing.T) {
	cases := []struct {
		deadline time.Duration
		want     string
	}{
		{defaultTestDeadline, "GO2CS_TEST_DEADLINE=15m0s"},
		{90 * time.Minute, "GO2CS_TEST_DEADLINE=1h30m0s"},
		{0, "GO2CS_TEST_DEADLINE=0"},
	}

	for _, c := range cases {
		env := testHostEnv(t.TempDir(), Options{testDeadline: c.deadline, testConfig: "Release"})

		found := 0
		for _, entry := range env {
			if strings.HasPrefix(entry, "GO2CS_TEST_DEADLINE=") {
				found++
				if entry != c.want {
					t.Errorf("deadline %s: env carries %q, want %q", c.deadline, entry, c.want)
				}
			}
		}

		if found != 1 {
			t.Errorf("deadline %s: env carries %d GO2CS_TEST_DEADLINE entries, want exactly 1 (%v)", c.deadline, found, env)
		}
	}

	for _, a := range convertedHostArgs(Options{testDeadline: defaultTestDeadline, testTimeout: 20 * time.Minute}) {
		if strings.Contains(a, "deadline") {
			t.Errorf("the deadline must not reach the host's argv, which is the converted program's os.Args; got %q", a)
		}
	}
}

// The watchdog's verdict still FAILS the row (COORD's ruling: an abandoned test counts as fail), but its
// mismatch line says the test did not finish, so it is never read as converted code failing. Only the
// abandoned test's line is labelled.
func TestLabelAbandonedTestsMarksOnlyTheWatchdogsVerdict(t *testing.T) {
	stream := strings.Join([]string{
		`{"test":"TestHangs","action":"run"}`,
		`{"test":"TestHangs","action":"fail","abandoned":true,"output":"testing: TestHangs did not finish within the per-test deadline (15m)"}`,
		`{"test":"TestBroken","action":"run","afterAbandoned":"TestHangs"}`,
		`{"test":"TestBroken","action":"fail","afterAbandoned":"TestHangs"}`,
	}, "\n")

	abandoned := terminalAbandonedTests(stream)

	if !abandoned["TestHangs"] || abandoned["TestBroken"] {
		t.Fatalf("terminalAbandonedTests = %v, want only TestHangs", abandoned)
	}

	mismatches := []string{`TestBroken: Go="pass" C#="fail"`, `TestHangs: Go="pass" C#="fail"`}
	labelled := labelAbandonedTests(mismatches, abandoned, defaultTestDeadline)

	if labelled[0] != mismatches[0] {
		t.Errorf("a failure the watchdog did not report must keep its line; got %q", labelled[0])
	}

	if !strings.HasPrefix(labelled[1], mismatches[1]) || !strings.Contains(labelled[1], "abandoned: did not finish within the per-test deadline 15m0s") {
		t.Errorf("the abandoned verdict's line must keep its status and say it was abandoned; got %q", labelled[1])
	}
}
