// commentParityUtf8_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// The acceptance test of docs/PLAN-comment-and-spacing-parity.md (§3, §5): Go's own unicode/utf8/utf8.go,
// converted with `-comments` by the converter in this tree, measured by the comment census
// (internal/commentcensus) against the Go source. utf8.go is the plan's worked example because it carries
// nearly every class in one small file with no imports: a const group with docs and blank lines, a gofmt-aligned
// end-of-line run, a commented composite-literal table, comments after `{`, and blank lines inside bodies.
//
// wantUtf8Census is the census reading of THIS tree. It is not a target: each seat of the plan lowers the
// classes it fixes and edits this table in the same commit, so the table always says what the converter does
// today and the diff of a seat says what it changed. A count that moves without an edit here fails, in either
// direction.

package main

import (
	"go/build"
	"maps"
	"os"
	"path/filepath"
	"slices"
	"strings"
	"testing"

	"go2cs/internal/commentcensus"
)

var wantUtf8Census = map[string]int{
	// B: doc comments of specs inside a group
	"missed:spec-doc-in-group": 2,
	// D: blank lines between the specs of a group
	"group:go-blank-cs-none": 4,
	"group:go-none-cs-blank": 4,
	// E: the converter's own blank lines inside bodies
	"body:extra:after-lowered-switch":      2,
	"body:extra:after-named-result-locals": 3,
	"body:extra:before-close-brace":        2,
	// F: comments inside composite literals (the `first` table)
	"misplaced:in-composite-literal/trailing-to-own-line": 16,
	"misplaced:in-composite-literal/anchor-differs":       1,
	// H: a trailing comment after `{`
	"misplaced:after-open-brace/trailing-to-own-line": 2,
	// J: end-of-line alignment
	"eol:misaligned":        1,
	"eol:split":             1,
	"eol:moved-to-own-line": 2,
	"eol:cs-ragged-run":     1,
}

func TestCommentParityUtf8(t *testing.T) {
	goRoot := build.Default.GOROOT
	goFile := filepath.Join(goRoot, "src", "unicode", "utf8", "utf8.go")

	source, err := os.ReadFile(goFile)
	if err != nil {
		t.Fatalf("reading Go's utf8.go: %v", err)
	}

	lines := convertWithComments(t, string(source))

	root := t.TempDir()
	goCopy := filepath.Join(root, "utf8.go")
	csFile := filepath.Join(root, "utf8.cs")

	writeModuleFile(t, goCopy, string(source))
	writeModuleFile(t, csFile, strings.Join(lines, "\n"))

	r := commentcensus.NewResults()

	if err := r.AnalyzeFiles(goCopy, csFile, "unicode/utf8/utf8"); err != nil {
		t.Fatalf("census: %v", err)
	}

	// Vacuity: utf8.go has dozens of comment groups; zero means the census read nothing.
	if r.Groups() < 40 {
		t.Fatalf("VACUOUS: %d Go comment groups read from utf8.go", r.Groups())
	}

	got := r.Counts()

	if maps.Equal(got, wantUtf8Census) {
		return
	}

	keys := map[string]bool{}
	for k := range got {
		keys[k] = true
	}
	for k := range wantUtf8Census {
		keys[k] = true
	}

	for _, k := range slices.Sorted(maps.Keys(keys)) {
		if got[k] != wantUtf8Census[k] {
			t.Errorf("%s = %d, want %d", k, got[k], wantUtf8Census[k])
		}
	}

	for _, f := range r.Findings("") {
		t.Logf("%s at %s: %s", f.Class, f.Where, f.Detail)
	}
}
