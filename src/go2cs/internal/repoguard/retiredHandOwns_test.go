// retiredHandOwns_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package repoguard

import (
	"fmt"
	"os"
	"os/exec"
	"path/filepath"
	"sort"
	"strconv"
	"strings"
	"testing"
)

// THE RETIRED-HAND-OWN GUARD (GoCorpusMigration.md H6, "Durable form"; §7 lesson 60).
//
// A hand-owned file ([module: GoManualConversion]) exists because the automatic conversion of that file
// was unsafe. Delete it, or drop its marker, and nothing fails: the automatic conversion takes its place,
// the corpus compiles, and the class the hand-own cured comes back uncured. That is how the go1.24.13 hop
// lost vendor/golang.org/x/crypto/sha3/xor.cs and its replacement re-emitted the fabricated array<T> it
// had cured, while H6 passed, because H6's census enumerates SURVIVORS.
//
// THE RULE: every path in the baseline that is no longer marked at HEAD has a row in the retirements file,
// and every row is consistent with HEAD:
//
//	(a) a baseline path not marked at HEAD with no row: a hand-own went away with no record of why;
//	(b) a row whose path is still marked at HEAD: the record says retired and the tree says present;
//	(c) a "moved" row whose target is not marked at HEAD: the cure was said to move somewhere it is not.
//
// Rows that name no baseline path, repeat a path, or are malformed fail too, so the file stays a record.
//
// THE POPULATION is the MARKED set only, H6's own predicate. *_impl.cs companions are outside it (AUDIT-h6
// OQ-1, ruled OUT 2026-10-12): a deleted companion leaves its partial declarations without bodies, so the
// build fails loudly, and the compiler already guards that case.
//
// THE BASELINE (testdata/handowns-baseline.txt) is the union of the marker census at each commit named in
// its header: the previous hop's outgoing commit and the commit the baseline was generated at. A census
// at HEAD alone reads zero absent entries and proves nothing about the hop that just closed.
// src/handown-baseline.ps1 writes it. This guard re-derives the union from the header's SHAs and fails
// when the body differs, so the file cannot be edited by hand. Regenerate it when a hop closes, naming
// that hop's outgoing commit.
//
// WHAT IT READS: the COMMITTED tree (git grep at HEAD), never the working copy, and the named SHAs, which
// must be in the clone. Run it with -count=1: Go's test cache cannot see a commit change.
//
// POSITIVE CONTROL: TestRetiredHandOwnScannerFires drives the same scanner over a synthetic baseline, HEAD
// set and rows, one plant per refusal, plus a clean control that must read empty.

const handOwnMarkerPredicate = `^\s*\[module:\s*(go\.)?GoManualConversion\]`

// retiredRow is one line of testdata/handowns-retired.tsv.
type retiredRow struct {
	line    int
	path    string
	movedTo string // empty for "deleted"
	record  string
}

// markedAt returns the hand-own marker census at ref, as repository-relative paths.
func markedAt(t *testing.T, root, ref string) []string {
	t.Helper()

	out, err := exec.Command("git", "-C", root, "grep", "-l", "-E", handOwnMarkerPredicate, ref, "--", "src/core/*.cs").Output()

	if err != nil {
		t.Fatalf("the marker census at %s failed (is the commit in this clone?): %v", ref, err)
	}

	var paths []string

	for _, line := range strings.Split(strings.TrimSpace(string(out)), "\n") {
		paths = append(paths, strings.TrimPrefix(line, ref+":"))
	}

	if len(paths) == 0 {
		t.Fatalf("the marker census at %s is empty: wrong clone or broken predicate", ref)
	}

	return paths
}

// readBaseline parses the fixture: its header SHAs with their counts, and its paths.
func readBaseline(t *testing.T, path string) (shas []string, counts []int, paths []string) {
	t.Helper()

	data, err := os.ReadFile(path)

	if err != nil {
		t.Fatal(err)
	}

	for _, line := range strings.Split(strings.ReplaceAll(string(data), "\r\n", "\n"), "\n") {
		switch {
		case line == "":
		case strings.HasPrefix(line, "# sha "):
			fields := strings.Fields(strings.TrimPrefix(line, "# sha "))
			n, err := strconv.Atoi(fields[len(fields)-1])

			if len(fields) != 2 || err != nil {
				t.Fatalf("%s: malformed header line %q", path, line)
			}

			shas = append(shas, fields[0])
			counts = append(counts, n)
		case strings.HasPrefix(line, "#"):
		default:
			paths = append(paths, line)
		}
	}

	if len(shas) == 0 || len(paths) == 0 {
		t.Fatalf("%s names no commits or no paths", path)
	}

	return shas, counts, paths
}

// parseRetired parses the retirements file's text; malformed lines come back as findings.
func parseRetired(text string) (rows []retiredRow, findings []string) {
	for i, line := range strings.Split(strings.ReplaceAll(text, "\r\n", "\n"), "\n") {
		if line == "" || strings.HasPrefix(line, "#") {
			continue
		}

		fields := strings.Split(line, "\t")

		if len(fields) != 3 || fields[0] == "" || strings.TrimSpace(fields[2]) == "" {
			findings = append(findings, fmt.Sprintf("line %d: want <path> TAB <disposition> TAB <record>, got %q", i+1, line))
			continue
		}

		row := retiredRow{line: i + 1, path: fields[0], record: fields[2]}

		switch disposition := fields[1]; {
		case disposition == "deleted":
		case strings.HasPrefix(disposition, "moved ") && strings.TrimPrefix(disposition, "moved ") != "":
			row.movedTo = strings.TrimPrefix(disposition, "moved ")
		default:
			findings = append(findings, fmt.Sprintf("line %d: disposition %q is neither \"deleted\" nor \"moved <path>\"", i+1, disposition))
			continue
		}

		rows = append(rows, row)
	}

	return rows, findings
}

// scanRetiredHandOwns applies refusals (a)-(c) and the row-integrity checks.
func scanRetiredHandOwns(baseline []string, marked map[string]bool, rows []retiredRow) (findings []string) {
	inBaseline := map[string]bool{}

	for _, p := range baseline {
		inBaseline[p] = true
	}

	byPath := map[string]retiredRow{}

	for _, r := range rows {
		if prev, dup := byPath[r.path]; dup {
			findings = append(findings, fmt.Sprintf("line %d: %s already has a row at line %d", r.line, r.path, prev.line))
			continue
		}

		byPath[r.path] = r

		switch {
		case !inBaseline[r.path]:
			findings = append(findings, fmt.Sprintf("line %d: %s is not in the baseline, so it has nothing to retire", r.line, r.path))
		case marked[r.path]:
			findings = append(findings, fmt.Sprintf("(b) line %d: %s is recorded as retired but is still marked at HEAD", r.line, r.path))
		case r.movedTo != "" && !marked[r.movedTo]:
			findings = append(findings, fmt.Sprintf("(c) line %d: %s is recorded as moved to %s, which is not marked at HEAD", r.line, r.path, r.movedTo))
		}
	}

	for _, p := range baseline {
		if !marked[p] {
			if _, ok := byPath[p]; !ok {
				findings = append(findings, fmt.Sprintf("(a) %s is in the baseline, is no longer marked at HEAD, and has no retirement row", p))
			}
		}
	}

	return findings
}

func TestRetiredHandOwnsHaveRows(t *testing.T) {
	root := repoRootFromPackageDir(t)
	dir := filepath.Join(root, "src", "go2cs", "internal", "repoguard", "testdata")

	// The baseline is the union of the census at its header's SHAs, exactly; anything else was edited.
	shas, counts, baseline := readBaseline(t, filepath.Join(dir, "handowns-baseline.txt"))
	union := map[string]bool{}

	for i, sha := range shas {
		census := markedAt(t, root, sha)

		if len(census) != counts[i] {
			t.Errorf("handowns-baseline.txt: the census at %s reads %d, the header says %d", sha, len(census), counts[i])
		}

		for _, p := range census {
			union[p] = true
		}
	}

	derived := make([]string, 0, len(union))

	for p := range union {
		derived = append(derived, p)
	}

	sort.Strings(derived)

	if strings.Join(derived, "\n") != strings.Join(baseline, "\n") {
		t.Fatalf("handowns-baseline.txt is not the union of the census at its header's SHAs (%d paths derived, %d in the file): "+
			"regenerate it with src/handown-baseline.ps1, never by hand", len(derived), len(baseline))
	}

	marked := map[string]bool{}

	for _, p := range markedAt(t, root, "HEAD") {
		marked[p] = true
	}

	text, err := os.ReadFile(filepath.Join(dir, "handowns-retired.tsv"))

	if err != nil {
		t.Fatal(err)
	}

	rows, findings := parseRetired(string(text))
	findings = append(findings, scanRetiredHandOwns(baseline, marked, rows)...)

	if len(findings) > 0 {
		t.Errorf("handowns-retired.tsv: %d finding(s). A hand-own that goes away needs a closed H6 record "+
			"(GoCorpusMigration.md H6, \"Durable form\"):\n  %s", len(findings), strings.Join(findings, "\n  "))
	}
}

func TestRetiredHandOwnScannerFires(t *testing.T) {
	baseline := []string{"src/core/a/kept.cs", "src/core/a/gone.cs", "src/core/a/moved.cs", "src/core/a/badmove.cs", "src/core/a/stillhere.cs"}
	marked := map[string]bool{"src/core/a/kept.cs": true, "src/core/b/moved.cs": true, "src/core/a/stillhere.cs": true}

	clean, malformed := parseRetired("# comment\n" +
		"src/core/a/gone.cs\tdeleted\tAUDIT row 1\n" +
		"src/core/a/moved.cs\tmoved src/core/b/moved.cs\tAUDIT row 2\n" +
		"src/core/a/badmove.cs\tdeleted\tAUDIT row 3\n")

	if len(malformed) != 0 {
		t.Fatalf("the control rows did not parse: %v", malformed)
	}

	if f := scanRetiredHandOwns(baseline, marked, clean); len(f) != 0 {
		t.Fatalf("the clean control reads %d finding(s), want 0: %v", len(f), f)
	}

	plants := []struct {
		name, rows, want string
	}{
		{"(a) absent with no row", "src/core/a/moved.cs\tmoved src/core/b/moved.cs\tr\nsrc/core/a/badmove.cs\tdeleted\tr\n",
			"(a) src/core/a/gone.cs is in the baseline"},
		{"(b) retired but still marked", "src/core/a/gone.cs\tdeleted\tr\nsrc/core/a/moved.cs\tmoved src/core/b/moved.cs\tr\n" +
			"src/core/a/badmove.cs\tdeleted\tr\nsrc/core/a/stillhere.cs\tdeleted\tr\n",
			"(b) line 4: src/core/a/stillhere.cs is recorded as retired but is still marked"},
		{"(c) moved to an unmarked target", "src/core/a/gone.cs\tdeleted\tr\nsrc/core/a/moved.cs\tmoved src/core/b/moved.cs\tr\n" +
			"src/core/a/badmove.cs\tmoved src/core/b/nowhere.cs\tr\n",
			"(c) line 3: src/core/a/badmove.cs is recorded as moved to src/core/b/nowhere.cs"},
		{"a row for a path outside the baseline", "src/core/a/gone.cs\tdeleted\tr\nsrc/core/a/moved.cs\tmoved src/core/b/moved.cs\tr\n" +
			"src/core/a/badmove.cs\tdeleted\tr\nsrc/core/z/typo.cs\tdeleted\tr\n",
			"src/core/z/typo.cs is not in the baseline"},
		{"a duplicated row", "src/core/a/gone.cs\tdeleted\tr\nsrc/core/a/gone.cs\tdeleted\tr\n" +
			"src/core/a/moved.cs\tmoved src/core/b/moved.cs\tr\nsrc/core/a/badmove.cs\tdeleted\tr\n",
			"src/core/a/gone.cs already has a row"},
	}

	for _, p := range plants {
		rows, malformed := parseRetired(p.rows)

		if len(malformed) != 0 {
			t.Fatalf("%s: the plant did not parse: %v", p.name, malformed)
		}

		f := scanRetiredHandOwns(baseline, marked, rows)

		if len(f) != 1 || !strings.Contains(f[0], p.want) {
			t.Errorf("%s: findings %v, want exactly one naming %q", p.name, f, p.want)
		}
	}

	for _, bad := range []string{
		"src/core/a/gone.cs\tremoved\tr",
		"src/core/a/gone.cs\tmoved \tr",
		"src/core/a/gone.cs\tdeleted",
		"src/core/a/gone.cs\tdeleted\t ",
	} {
		if _, malformed := parseRetired(bad); len(malformed) != 1 {
			t.Errorf("the malformed row %q was not refused", bad)
		}
	}
}
