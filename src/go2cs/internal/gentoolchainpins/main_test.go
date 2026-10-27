// main_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"bytes"
	"os"
	"path/filepath"
	"testing"
)

// toolchainPins.go is generated, so it must be exactly what the generator writes. testdata/ holds a saved slice
// of the generator's two sources for the pinned release: go.dev's index entry for its six archives (dl.json) and
// sum.golang.org's lookup answers for its six toolchain modules (sumdb/lookup/...). A hand edit of the pins, or a
// go line that moved without a regeneration, fails here; regenerate with `go generate .` from src/go2cs and
// refresh testdata/ from the live sources in the same change.
func TestToolchainPinsAreWhatTheGeneratorWrites(t *testing.T) {
	output := filepath.Join(t.TempDir(), "toolchainPins.go")

	if err := run(filepath.Join("..", "..", "go.mod"), filepath.Join("testdata", "dl.json"), filepath.Join("testdata", "sumdb"), output); err != nil {
		t.Fatal(err)
	}

	want, err := os.ReadFile(output)
	if err != nil {
		t.Fatal(err)
	}

	got, err := os.ReadFile(filepath.Join("..", "..", "toolchainPins.go"))
	if err != nil {
		t.Fatal(err)
	}

	// A Windows checkout may hold the committed file with CRLF line ends.
	if !bytes.Equal(bytes.ReplaceAll(got, []byte("\r\n"), []byte("\n")), want) {
		t.Fatalf("src/go2cs/toolchainPins.go is not what gentoolchainpins writes from testdata/; regenerate it:\n%s", want)
	}
}
