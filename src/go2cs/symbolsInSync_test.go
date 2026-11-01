// symbolsInSync_test.go - Gbtc
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
	"os/exec"
	"path/filepath"
	"strings"
	"testing"
)

// TestGeneratedSymbolsMatchTheTable runs gensymbols over a copy of the canonical table and requires both committed
// projections to equal its output. symbols.go and Symbols.cs say DO NOT EDIT, but nothing checked it under go test:
// src/check-symbol-sync.ps1 makes the same comparison and no gate runs it. Twice a seat hand-edited symbols.go
// (c5a0034c28, then #53's StaticBackingFieldMarker in d5a2c165c5) without touching symbols.json, so the next
// `go generate .` deleted a constant the converter uses. The generator runs in a temporary tree, never over the
// committed files.
func TestGeneratedSymbolsMatchTheTable(t *testing.T) {
	root := t.TempDir()
	generator := filepath.Join(root, "gensymbols.exe")
	build := exec.Command("go", "build", "-o", generator, "./internal/gensymbols")

	if output, err := build.CombinedOutput(); err != nil {
		t.Fatalf("go build ./internal/gensymbols: %v\n%s", err, output)
	}

	table, err := os.ReadFile(filepath.FromSlash("../core/go2cs/symbols.json"))

	if err != nil {
		t.Fatal(err)
	}

	workDir := filepath.Join(root, "go2cs")
	coreDir := filepath.Join(root, "core", "go2cs")

	for _, dir := range []string{workDir, coreDir} {
		if err := os.MkdirAll(dir, 0o755); err != nil {
			t.Fatal(err)
		}
	}

	if err := os.WriteFile(filepath.Join(coreDir, "symbols.json"), table, 0o644); err != nil {
		t.Fatal(err)
	}

	generate := exec.Command(generator)
	generate.Dir = workDir

	if output, err := generate.CombinedOutput(); err != nil {
		t.Fatalf("gensymbols: %v\n%s", err, output)
	}

	projections := []struct{ committed, generated string }{
		{"symbols.go", filepath.Join(workDir, "symbols.go")},
		{filepath.FromSlash("../core/go2cs/Symbols.cs"), filepath.Join(coreDir, "Symbols.cs")},
	}

	for _, projection := range projections {
		committed := symbolProjectionLines(t, projection.committed)
		generated := symbolProjectionLines(t, projection.generated)

		if strings.Join(committed, "\n") == strings.Join(generated, "\n") {
			continue
		}

		t.Errorf("%s differs from what gensymbols produces from symbols.json: edit symbols.json and run `go generate .`, never the generated file", projection.committed)

		for _, line := range lineDifference(committed, generated) {
			t.Errorf("  committed only: %s", line)
		}

		for _, line := range lineDifference(generated, committed) {
			t.Errorf("  generated only: %s", line)
		}
	}
}

// symbolProjectionLines reads a projection without its BOM and with CR stripped: the generator keeps each file's own
// BOM and line-ending convention, which the checkout decides.
func symbolProjectionLines(t *testing.T, path string) []string {
	t.Helper()

	content, err := os.ReadFile(path)

	if err != nil {
		t.Fatal(err)
	}

	content = bytes.TrimPrefix(content, []byte("\xEF\xBB\xBF"))

	return strings.Split(strings.ReplaceAll(string(content), "\r", ""), "\n")
}

// lineDifference returns the lines of first that second does not hold.
func lineDifference(first, second []string) []string {
	held := map[string]int{}

	for _, line := range second {
		held[line]++
	}

	var missing []string

	for _, line := range first {
		if held[line] > 0 {
			held[line]--
			continue
		}

		missing = append(missing, line)
	}

	return missing
}
