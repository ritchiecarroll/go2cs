// converterGofmt_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package repoguard

import (
	"bytes"
	"go/format"
	"os"
	"path/filepath"
	"strings"
	"testing"
)

// The converter's own Go sources stay gofmt-clean. Before this guard, 30 tracked files under src/go2cs (13 sources, 17
// tests) had drifted, and every later edit to one of them dragged the unrelated reformat into its diff.
//
// The check formats the file's content with every CR removed, because a Windows checkout has CRLF line endings while
// git stores LF: gofmt flags a CRLF file on its own, and its doc-comment reformatting (a blank `//` line between a
// list and the next paragraph) does not fire on CRLF input at all. The stored blobs carry no CR, so removing them reads
// exactly what git stores.
//
// testdata is excluded by rule: a fixture's layout is the converter's input. Today those are the five commentcensus
// fixtures under src/go2cs/internal/commentcensus/testdata (anchor, lazyholder, move2heap, planted, prelude), whose
// comments and positions are what the census tests read.

// gofmtViolations returns the files whose content (CR removed) gofmt would change, or that do not parse. sources maps
// each repository-relative path to its content.
func gofmtViolations(sources map[string][]byte) []string {
	var violations []string

	for path, content := range sources {
		stored := bytes.ReplaceAll(content, []byte("\r"), nil)
		formatted, err := format.Source(stored)

		if err != nil {
			violations = append(violations, path+": does not parse: "+err.Error())
		} else if !bytes.Equal(formatted, stored) {
			violations = append(violations, path+": not gofmt-clean (run gofmt on it)")
		}
	}

	return violations
}

// converterGoSources reads every tracked .go file under src/go2cs outside a testdata directory.
func converterGoSources(t *testing.T) map[string][]byte {
	t.Helper()

	root := repoRootFromPackageDir(t)
	sources := map[string][]byte{}

	for _, rel := range gitTrackedFiles(t, root) {
		if !strings.HasPrefix(rel, "src/go2cs/") || !strings.HasSuffix(rel, ".go") || strings.Contains(rel, "/testdata/") {
			continue
		}

		data, err := os.ReadFile(filepath.Join(root, filepath.FromSlash(rel)))

		if err != nil {
			t.Fatalf("%s: %v", rel, err)
		}

		sources[rel] = data
	}

	return sources
}

// TestConverterSourcesAreGofmtClean: every tracked .go file under src/go2cs outside testdata is gofmt-clean.
func TestConverterSourcesAreGofmtClean(t *testing.T) {
	sources := converterGoSources(t)

	if len(sources) < 500 {
		t.Fatalf("found %d converter .go files, too few to be this repository; a guard that reads nothing passes everything", len(sources))
	}

	for _, violation := range gofmtViolations(sources) {
		t.Error(violation)
	}
}

// TestGofmtGuardNamesAMisformattedFile: a misformatted file is named, a CRLF copy of a clean file is not, and a CRLF copy
// of a file whose only drift is a doc-comment list break is still named (the case a CR-blind check misses).
func TestGofmtGuardNamesAMisformattedFile(t *testing.T) {
	clean := "package p\n\n// F is a function.\nfunc F() int { return 1 }\n"
	listDrift := "package p\n\n// Doc:\n//   - one item\n// next paragraph\nfunc F() {}\n"

	violations := gofmtViolations(map[string][]byte{
		"src/go2cs/clean.go":     []byte(clean),
		"src/go2cs/cleancrlf.go": []byte(strings.ReplaceAll(clean, "\n", "\r\n")),
		"src/go2cs/spacing.go":   []byte("package p\nfunc  F( ) {}\n"),
		"src/go2cs/listcrlf.go":  []byte(strings.ReplaceAll(listDrift, "\n", "\r\n")),
	})

	named := strings.Join(violations, "\n")

	for _, want := range []string{"src/go2cs/spacing.go:", "src/go2cs/listcrlf.go:"} {
		if !strings.Contains(named, want) {
			t.Errorf("the guard did not name %s; it reported:\n%s", strings.TrimSuffix(want, ":"), named)
		}
	}

	if len(violations) != 2 {
		t.Errorf("the guard reported %d violations, want exactly the 2 misformatted files:\n%s", len(violations), named)
	}
}
