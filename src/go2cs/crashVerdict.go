// crashVerdict.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// A CONVERTED TEST HOST THAT CRASHES, OR HOLDS A TEST UNTIL ITS PACKAGE DEADLINE. Stub (red): nothing is classified
// yet, and nothing calls it.

package main

// hostCrash is what a converted test host's abnormal end says about the run.
type hostCrash struct {
	Label    string
	InFlight []string
	Head     string
	TimedOut bool
}

// inFlightTests names every test the stream started and never finished.
func inFlightTests(output string) []string {
	return nil
}

// classifyHostCrash reads the converted host's run.
func classifyHostCrash(rawErr error, output string) (hostCrash, bool) {
	return hostCrash{}, false
}

// applyHostCrash records a crash, or a live host's package deadline, on a comparison whose converted side's failure
// already stands.
func applyHostCrash(result *testComparison, rawErr error, output string, eligible func(map[string]string) map[string]string) {
}
