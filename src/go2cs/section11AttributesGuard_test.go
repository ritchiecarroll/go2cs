// section11AttributesGuard_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"io/fs"
	"os"
	"path/filepath"
	"strconv"
	"strings"
	"testing"
)

// section11Attributes are the inline attributes docs/PLAN-marker-comment-parity.md section 11 moves out of
// converted code, each with the row that moved it. A row is added in the change that stops the converter
// writing the attribute.
var section11Attributes = []struct {
	attribute string
	row       string
}{
	{"[GoDescriptorType(", "row 5: the descriptor carrier is recorded on the type's accessibility declaration"},
}

// TestSection11AttributesLeftConvertedCode reads the COMMITTED corpus and the behavioral suite and holds the
// section 11 rows: CONVERTED PRODUCTION code and the BEHAVIORAL sources and goldens (`.cs`, `.cs.target`) carry
// none of section11Attributes. The metadata files (package_info.cs and its test variants) are where the records
// live, and hand-written files keep the attributes. Converted TEST sources (`_test.cs`) are not read: they are
// refreshed once at the train's landing, and the first seat after the landing adds them. The converter's own
// rendering is pinned by s11Records_test.go.
func TestSection11AttributesLeftConvertedCode(t *testing.T) {
	coreDir := filepath.Join("..", "core")
	behavioralDir := filepath.Join("..", "tests", "Behavioral")

	var sites []string
	scanned := 0

	walk := func(root string, behavioral bool) error {
		return filepath.WalkDir(root, func(filePath string, entry fs.DirEntry, walkErr error) error {
			if walkErr != nil {
				return walkErr
			}

			name := entry.Name()

			if entry.IsDir() {
				if name == "bin" || name == "obj" || name == ".vs" || strings.HasPrefix(name, "Generated") {
					return fs.SkipDir
				}

				return nil
			}

			if !strings.HasSuffix(name, ".cs") && !(behavioral && strings.HasSuffix(name, ".cs.target")) {
				return nil
			}

			if strings.HasPrefix(name, "package_info") || strings.HasPrefix(name, "package_test_info") || (!behavioral && strings.HasSuffix(name, "_test.cs")) {
				return nil
			}

			data, readErr := os.ReadFile(filePath)

			if readErr != nil {
				return readErr
			}

			rel, _ := filepath.Rel(root, filePath)
			rel = filepath.ToSlash(rel)

			if !behavioral && isHandWritten(rel, data) {
				return nil
			}

			if behavioral {
				rel = "tests/Behavioral/" + rel
			}

			scanned++

			for index, line := range strings.Split(string(data), "\n") {
				code, _, _ := strings.Cut(line, "//")

				for _, moved := range section11Attributes {
					if strings.Contains(code, moved.attribute) {
						sites = append(sites, rel+":"+strconv.Itoa(index+1)+": "+moved.attribute+" ("+moved.row+")")
					}
				}
			}

			return nil
		})
	}

	if err := walk(coreDir, false); err != nil {
		t.Fatal(err)
	}

	if err := walk(behavioralDir, true); err != nil {
		t.Fatal(err)
	}

	if scanned < 3000 {
		t.Fatalf("the guard read %d files: too few to be the corpus", scanned)
	}

	for _, site := range sites {
		t.Errorf("converted code carries an attribute section 11 moved out of it: %s -- re-convert", site)
	}

	t.Logf("read %d files: %d site(s) of %d moved attribute(s)", scanned, len(sites), len(section11Attributes))
}
