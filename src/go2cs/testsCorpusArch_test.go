// testsCorpusArch_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"path/filepath"
	"testing"
)

// A -tests run of a standard-library package re-emits that package into the COMMITTED corpus, which is
// built for amd64 on every GOOS. On an arm64 host the -platforms default (the converter binary's own
// runtime.GOOS/runtime.GOARCH) re-emitted it for darwin/arm64 instead: hash/crc32 then selected
// crc32_arm64.go beside the committed crc32_amd64.cs and its test host failed to build, CS0111 x6
// (darwin row pilot, run 37112360853, 2026-10-03). These pin the decision that replaces that default.
func TestTestsTargetPlatformPinsTheCorpusArchForGOROOTPackages(t *testing.T) {
	goRoot := filepath.Join(t.TempDir(), "go")
	stdlibPackage := filepath.Join(goRoot, "src", "hash", "crc32")
	moduleDir := filepath.Join(t.TempDir(), "mod", "example.com", "thing")

	cases := []struct {
		name         string
		hostDefault  string
		explicit     bool
		convertTests bool
		inputDir     string
		want         string
		wantPinned   bool
	}{
		{"stdlib on darwin/arm64 pins amd64", "darwin/arm64", false, true, stdlibPackage, "darwin/amd64", true},
		{"stdlib on linux/arm64 pins amd64", "linux/arm64", false, true, stdlibPackage, "linux/amd64", true},
		{"stdlib on darwin/amd64 is unchanged", "darwin/amd64", false, true, stdlibPackage, "darwin/amd64", false},
		{"stdlib on windows/amd64 is unchanged", "windows/amd64", false, true, stdlibPackage, "windows/amd64", false},
		{"stdlib on linux/amd64 is unchanged", "linux/amd64", false, true, stdlibPackage, "linux/amd64", false},
		{"a module on darwin/arm64 keeps the host arch", "darwin/arm64", false, true, moduleDir, "darwin/arm64", false},
		{"an explicit -platforms always wins", "darwin/arm64", true, true, stdlibPackage, "darwin/arm64", false},
		{"a conversion without -tests is not pinned", "darwin/arm64", false, false, stdlibPackage, "darwin/arm64", false},
		{"GOROOT itself, not its src, is not a package", "darwin/arm64", false, true, goRoot, "darwin/arm64", false},
	}

	for _, c := range cases {
		got, pinned := testsTargetPlatform(c.hostDefault, c.explicit, c.convertTests, c.inputDir, goRoot)

		if got != c.want || pinned != c.wantPinned {
			t.Errorf("%s: testsTargetPlatform(%q, explicit=%v, tests=%v, %q) = (%q, %v), want (%q, %v)",
				c.name, c.hostDefault, c.explicit, c.convertTests, c.inputDir, got, pinned, c.want, c.wantPinned)
		}
	}

	if corpusArch != "amd64" {
		t.Errorf("corpusArch = %q; the committed corpus is emitted with -platforms windows/amd64,linux/amd64,darwin/amd64", corpusArch)
	}
}
