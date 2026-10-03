// testHostLaunchDir_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"os"
	"path/filepath"
	"testing"
)

// The converted test host starts where `go test` starts a test binary: the package's SOURCE
// directory, so load-time code (a test package's init reading a relative path) finds what Go finds.
// The Behavioral project TestInitReadsModuleFixture is the end-to-end red; this pins the rule.
func TestTestHostLaunchDirIsThePackageSourceDirectory(t *testing.T) {
	source := t.TempDir()
	output := t.TempDir()

	if got := testHostLaunchDir(source, output); got != source {
		t.Fatalf("want the package source directory %s, got %s", source, got)
	}

	// A relative input resolves absolute (the host gets a real directory, not one relative to
	// whatever the converter's own working directory happens to be).
	wd, err := os.Getwd()

	if err != nil {
		t.Fatal(err)
	}

	rel, err := filepath.Rel(wd, source)

	if err == nil {
		if got := testHostLaunchDir(rel, output); got != source {
			t.Fatalf("a relative source: want %s, got %s", source, got)
		}
	}

	// An action on existing artifacts whose source no longer exists keeps the output directory.
	if got := testHostLaunchDir(filepath.Join(source, "gone"), output); got != output {
		t.Fatalf("a missing source: want the output directory %s, got %s", output, got)
	}
}
