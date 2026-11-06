// oracleArch_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import "testing"

// The darwin corpus is converted ONCE, for darwin/amd64, and runs under both mac RIDs. On osx-arm64 the converted
// host is a native arm64 .NET process, while the oracle `go test` child took GOARCH=amd64 from the conversion target
// and ran under Rosetta. That oracle read Rosetta's own artifacts as the package's behaviour: runtime's
// TestMmapErrorSign fails there (mmap returns EINVAL, 22, where the native kernel answers ENOMEM, 12) and passes
// natively, and the comparison charged the difference to the converted side (COORD's osx-arm64 oracle seat, ruling
// (A)). -test-oracle-goarch runs the oracle at the arch the converted host runs at; the conversion keeps its target.
func TestOracleRunsAtItsOwnArchAndTheConversionKeepsItsTarget(t *testing.T) {
	options := Options{targetPlatform: "darwin/amd64", testOracleGOARCH: "arm64"}

	if got := oracleOptions(options).targetPlatform; got != "darwin/arm64" {
		t.Errorf("the oracle child runs at %q, want darwin/arm64", got)
	}

	if options.targetPlatform != "darwin/amd64" {
		t.Errorf("the conversion's own target moved to %q; only the oracle's copy may change", options.targetPlatform)
	}

	if got := testEnvironmentFromOptions(options).OracleGOARCH; got != "arm64" {
		t.Errorf("the comparison record's oracleGoarch = %q, want arm64: a reading must say its oracle ran at another arch", got)
	}
}

// Unset, or naming the target's own arch, the flag changes nothing: every other leg's oracle and record stay
// byte-identical.
func TestOracleArchUnsetOrEqualChangesNothing(t *testing.T) {
	for _, arch := range []string{"", "amd64"} {
		options := Options{targetPlatform: "darwin/amd64", testOracleGOARCH: arch}

		if got := oracleOptions(options).targetPlatform; got != "darwin/amd64" {
			t.Errorf("-test-oracle-goarch %q: the oracle runs at %q, want the target darwin/amd64", arch, got)
		}

		if got := testEnvironmentFromOptions(options).OracleGOARCH; got != "" {
			t.Errorf("-test-oracle-goarch %q: oracleGoarch = %q, want it omitted", arch, got)
		}
	}
}
