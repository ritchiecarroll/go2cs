// reconvertDeletionsSkipList_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

// The deletion pass's skip-list mirror, both directions.
//
// src\reconvert-deletions.ps1 (the H5 deletion pass) removes files from a seeded reconvert root, and
// it decides what is even a CANDIDATE by asking whether the converter emits into the file's
// directory. Two of the three answers it needs come from Go itself -- `go list std` at the source
// release names every package the -stdlib driver walks -- but the third cannot: `go list std` names
// `unsafe` and `testing` like any other package, and only isNonConvertedStdLibPackage (above, in
// stdLibConverter.go) knows they are deliberately kept out of the queue. So the script carries a
// MIRROR of that set, and a mirror drifts.
//
// What drift costs here is not a wrong report. It is a deletion: the pass would classify a
// skip-listed package's HAND-OWNED files as ordinary conversion output and offer them for removal --
// which is the exact shape of the defect this guard was written alongside. R's first real dry run of
// the instrument (mailbox 1f5e8f276) produced a 205-row delete set of which 117 rows were
// src\core\golib\*.cs and src\core\go2cs\Symbols.cs, because the pass asked "does Go still select
// this principal" of a directory that was never a Go package at all.
//
// Both directions are asserted, because a one-sided seam check passes the exact failure it was
// written for in mirror form:
//
//  1. Every name in the script's literal really is skip-listed by the converter. An EXTRA entry in
//     the script would protect a package the converter does emit -- files that should be deleted at a
//     hop would survive into the overlay and collide (the CS0102 the pass exists to prevent).
//  2. Every name the converter skip-lists is in the script's literal. A MISSING entry is the
//     destructive direction: the pass would offer a hand-owned package's files for deletion.
//
// The set comparison is against the converter's package-level VALUES, not against a regex over its
// own source -- the decision, never the text. Only the SCRIPT side is extracted by pattern, because
// PowerShell has no other way in; that extraction is line-anchored and positive-controlled below
// (a commented-out assignment must not be read as the live one).
//
// The two HAND-WRITTEN roots the script also protects (golib\, go2cs\) correspond to no Go import
// path at any release, so no set here can name them; what this file can check is that the script
// still names them and that they still exist on disk under src\core.
//
// CACHE CAVEAT: reconvert-deletions.ps1 lives under src\, OUTSIDE this module root, and cmd/go drops
// such files from a test's input fingerprint (computeTestInputsID: "Do not recheck files outside the
// module, GOPATH, or GOROOT root"). This test therefore reports `ok (cached)` after a change to the
// script and only re-runs under `-count=1`. Any change touching the .ps1 owes
// `go test -count=1 ./...`, exactly as archExclusive_test.go and embeddedAssets_test.go do.

import (
	"os"
	"path/filepath"
	"regexp"
	"sort"
	"strings"
	"testing"
)

var (
	deletionPassScript = filepath.Join("..", "reconvert-deletions.ps1")

	// Line-anchored, and requiring the assignment form: this file's own prose names the variables,
	// and so does the script's, so an unanchored scan would read a comment as the live literal. A
	// PowerShell comment line begins with '#', which `^\s*\$` cannot match.
	deletionPassPackagesLiteral = regexp.MustCompile(`(?m)^\s*\$NonConvertedStdPackages\s*=\s*@\(([^)]*)\)`)
	deletionPassPrefixesLiteral = regexp.MustCompile(`(?m)^\s*\$NonConvertedStdPrefixes\s*=\s*@\(([^)]*)\)`)
	deletionPassRootsLiteral    = regexp.MustCompile(`(?m)^\s*\$HandWrittenRoots\s*=\s*@\(([^)]*)\)`)

	// Single-quoted PowerShell string elements inside the @( ... ) list.
	deletionPassListElement = regexp.MustCompile(`'([^']*)'`)
)

// extractPowerShellList returns the single-quoted elements of the one line matching pattern, and
// requires that there be EXACTLY one such line: a second assignment anywhere in the file would make
// the mirror ambiguous, and this guard would silently compare against whichever came first.
func extractPowerShellList(t *testing.T, source string, pattern *regexp.Regexp, label string) []string {
	t.Helper()

	matches := pattern.FindAllStringSubmatch(source, -1)

	if len(matches) != 1 {
		t.Fatalf("%s: expected exactly 1 line-anchored assignment of %s in %s, found %d.\n"+
			"The guard reads that literal; keep it on ONE line and in one place.",
			label, label, deletionPassScript, len(matches))
	}

	var values []string

	for _, element := range deletionPassListElement.FindAllStringSubmatch(matches[0][1], -1) {
		values = append(values, element[1])
	}

	if len(values) == 0 {
		t.Fatalf("%s: the assignment in %s parsed to ZERO elements. An empty mirror disarms this "+
			"guard in the destructive direction (nothing is protected).", label, deletionPassScript)
	}

	return values
}

func sortedCopy(values []string) []string {
	out := append([]string(nil), values...)
	sort.Strings(out)

	return out
}

func TestReconvertDeletionsSkipListMirrorsTheConverter(t *testing.T) {
	raw, err := os.ReadFile(deletionPassScript)

	if err != nil {
		t.Fatalf("cannot read the deletion pass at %s: %v", deletionPassScript, err)
	}

	source := string(raw)

	scriptPackages := extractPowerShellList(t, source, deletionPassPackagesLiteral, "$NonConvertedStdPackages")
	scriptPrefixes := extractPowerShellList(t, source, deletionPassPrefixesLiteral, "$NonConvertedStdPrefixes")

	// Direction 1 -- every name the SCRIPT protects is genuinely skip-listed. Asserted by CALLING the
	// converter's predicate, so this leg is a statement about the decision rather than about either
	// literal's spelling.
	for _, name := range scriptPackages {
		if !isNonConvertedStdLibPackage(name) {
			t.Errorf("%s protects %q, but isNonConvertedStdLibPackage(%q) is FALSE -- the converter "+
				"DOES emit that package, so the deletion pass would keep stale files it should remove.",
				deletionPassScript, name, name)
		}
	}

	for _, prefix := range scriptPrefixes {
		if !isNonConvertedStdLibPackage(prefix + "probe") {
			t.Errorf("%s protects the prefix %q, but isNonConvertedStdLibPackage(%q) is FALSE.",
				deletionPassScript, prefix, prefix+"probe")
		}
	}

	// Direction 2 -- the destructive one. Every name the CONVERTER skip-lists must appear in the
	// script, or the pass offers that package's hand-owned files for deletion.
	wantPackages := sortedCopy(nonConvertedStdLibPackages)
	gotPackages := sortedCopy(scriptPackages)

	if strings.Join(wantPackages, ",") != strings.Join(gotPackages, ",") {
		t.Errorf("skip-list SETS disagree.\n  converter (nonConvertedStdLibPackages): %v\n  script    ($NonConvertedStdPackages):      %v\n"+
			"A name missing from the script is the destructive direction: %s would offer that "+
			"package's hand-owned files for deletion at a release hop.",
			wantPackages, gotPackages, deletionPassScript)
	}

	wantPrefixes := sortedCopy(nonConvertedStdLibPrefixes)
	gotPrefixes := sortedCopy(scriptPrefixes)

	if strings.Join(wantPrefixes, ",") != strings.Join(gotPrefixes, ",") {
		t.Errorf("skip-list PREFIX sets disagree.\n  converter (nonConvertedStdLibPrefixes): %v\n  script    ($NonConvertedStdPrefixes):      %v",
			wantPrefixes, gotPrefixes)
	}
}

// The negative half of the extraction: a control set of ordinary converted packages must NOT be
// skip-listed. Without it, direction 1 above is satisfied by a predicate that returns true for
// everything -- and such a predicate would empty the whole conversion queue.
func TestReconvertDeletionsSkipListDoesNotSwallowConvertedPackages(t *testing.T) {
	// testing/fstest and the other testing SUBPACKAGES are the sharp cases: `testing` is skip-listed
	// and they are not, which is exactly the distinction the deletion pass's path test relies on.
	for _, converted := range []string{
		"testing/fstest", "testing/iotest", "testing/quick", "testing/slogtest",
		"testing/internal/testdeps", "runtime", "reflect", "internal/weak", "unique", "cmp",
	} {
		if isNonConvertedStdLibPackage(converted) {
			t.Errorf("isNonConvertedStdLibPackage(%q) is TRUE, but that package IS converted.", converted)
		}
	}
}

// The extraction's own positive control. A guard whose reader can be fooled by a comment is the
// text-grepping failure this repository has paid for repeatedly, so the pattern is exercised against
// a synthetic decoy: a commented-out assignment must not be read as the live one.
func TestReconvertDeletionsSkipListExtractionRejectsAComment(t *testing.T) {
	decoy := "# $NonConvertedStdPackages = @('decoy')\r\n" +
		"    #$NonConvertedStdPackages = @('decoy2')\r\n"

	if got := deletionPassPackagesLiteral.FindAllStringSubmatch(decoy, -1); len(got) != 0 {
		t.Fatalf("the literal pattern matched a COMMENTED assignment (%d hit(s)) -- it would read "+
			"prose as the live skip-list.", len(got))
	}

	live := "$NonConvertedStdPackages = @('unsafe', 'builtin')\r\n"

	if got := deletionPassPackagesLiteral.FindAllStringSubmatch(live, -1); len(got) != 1 {
		t.Fatalf("the literal pattern failed to match a LIVE assignment (%d hit(s)) -- a pattern that "+
			"cannot fire reports an empty mirror, which disarms the guard.", len(got))
	}
}

// The two hand-written roots the deletion pass protects by PATH rather than by any Go question. They
// are literals in the script because no `go list` at any release can name them; what is checkable
// here is that the script still names both and that both still exist under src\core -- a rename would
// otherwise leave the pass protecting a directory that is gone while the real one went unguarded.
func TestReconvertDeletionsProtectsTheHandWrittenRoots(t *testing.T) {
	raw, err := os.ReadFile(deletionPassScript)

	if err != nil {
		t.Fatalf("cannot read the deletion pass at %s: %v", deletionPassScript, err)
	}

	roots := extractPowerShellList(t, string(raw), deletionPassRootsLiteral, "$HandWrittenRoots")

	for _, want := range []string{"golib", "go2cs"} {
		found := false

		for _, root := range roots {
			if root == want {
				found = true
				break
			}
		}

		if !found {
			t.Errorf("%s no longer protects core\\%s\\ -- that directory is the repository's own "+
				"hand-written C#, carries no [module: GoManualConversion] marker (nothing converts "+
				"into it), and was offered for deletion 117 files at a time before it was named here.",
				deletionPassScript, want)
		}
	}

	for _, root := range roots {
		path := filepath.Join("..", "core", root)

		if info, err := os.Stat(path); err != nil || !info.IsDir() {
			t.Errorf("%s protects core\\%s\\, which is not a directory (%v). A protected root that "+
				"does not exist guards nothing.", deletionPassScript, root, err)
		}
	}
}
