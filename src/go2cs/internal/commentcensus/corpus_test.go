// corpus_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// TestCommentParityCensus is the corpus ratchet of docs/PLAN-comment-and-spacing-parity.md §5: the census over
// every converted production file in src/core, against Go's source at the pinned GOROOT, compared class by class
// with testdata/corpus_baseline.txt.
//
// A RISE fails, naming the class and the files that carry it: a converter change has started dropping or moving
// comments or blank lines again. A FALL fails too, until the baseline is updated in the same commit (the
// declared-set pattern of declaredNotImplemented_test.go): a fix that lowers a class must say by how much, so the
// next change cannot spend the improvement silently. To rewrite the baseline from the tree, run this test with
// GO2CS_COMMENT_CENSUS_UPDATE=1 and commit the file.
//
// The counts are a property of the committed corpus, so a converter fix moves them only when its corpus
// re-emission is committed with it.

package commentcensus

import (
	"bufio"
	"fmt"
	"go/build"
	"maps"
	"os"
	"path/filepath"
	"runtime"
	"slices"
	"sort"
	"strconv"
	"strings"
	"testing"
)

const baselinePath = "testdata/corpus_baseline.txt"

func firstLine(path string) (string, error) {
	data, err := os.ReadFile(path)
	if err != nil {
		return "", err
	}

	line, _, _ := strings.Cut(strings.ReplaceAll(string(data), "\r\n", "\n"), "\n")

	return strings.TrimSpace(line), nil
}

func readBaseline(t *testing.T) map[string]int {
	t.Helper()

	f, err := os.Open(baselinePath)
	if err != nil {
		t.Fatalf("reading the baseline: %v", err)
	}
	defer f.Close()

	base := map[string]int{}
	scanner := bufio.NewScanner(f)

	for scanner.Scan() {
		line := strings.TrimSpace(scanner.Text())

		if line == "" || strings.HasPrefix(line, "#") {
			continue
		}

		key, value, ok := strings.Cut(line, "\t")
		n, err := strconv.Atoi(strings.TrimSpace(value))

		if !ok || err != nil {
			t.Fatalf("%s: malformed line %q (want <class>TAB<count>)", baselinePath, line)
		}

		base[key] = n
	}

	if err := scanner.Err(); err != nil {
		t.Fatalf("reading the baseline: %v", err)
	}

	return base
}

func writeBaseline(t *testing.T, counts map[string]int, files, groups int, goVersion string) {
	t.Helper()

	var b strings.Builder

	fmt.Fprintf(&b, "# Comment and spacing census of src/core against GOROOT %s: %d files, %d Go comment groups.\n", goVersion, files, groups)
	b.WriteString("# Written by TestCommentParityCensus with GO2CS_COMMENT_CENSUS_UPDATE=1; one defect class per line.\n")
	b.WriteString("# Lower a count only in the commit whose corpus re-emission lowers it (see corpus_test.go).\n")

	for _, k := range slices.Sorted(maps.Keys(counts)) {
		fmt.Fprintf(&b, "%s\t%d\n", k, counts[k])
	}

	if err := os.WriteFile(baselinePath, []byte(b.String()), 0o644); err != nil {
		t.Fatalf("writing the baseline: %v", err)
	}
}

func TestCommentParityCensus(t *testing.T) {
	coreDir := filepath.Join("..", "..", "..", "core")

	goRoot := build.Default.GOROOT
	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	// NOT A SKIP: the baseline is a reading of this corpus against ITS Go release. Under another toolchain every
	// class moves for reasons that have nothing to do with the converter, and a skip would let the guard go
	// silent on exactly the hosts that run it unpinned.
	corpusVersion, err := firstLine(filepath.Join(coreDir, "VERSION"))
	if err != nil {
		t.Fatalf("reading the corpus's Go release (src/core/VERSION): %v", err)
	}

	goVersion, err := firstLine(filepath.Join(goRoot, "VERSION"))
	if err != nil {
		t.Fatalf("reading GOROOT's release (%s/VERSION): %v", goRoot, err)
	}

	if goVersion != corpusVersion {
		t.Fatalf("GOROOT is %s but the corpus was converted from %s: run under the pinned toolchain (GOTOOLCHAIN=local with %s's GOROOT)",
			goVersion, corpusVersion, corpusVersion)
	}

	pairs := FindPairs(coreDir, goRoot)

	// Vacuity: the corpus has about 1,600 pairs; a handful means the walk is measuring nothing.
	if len(pairs) < 1000 {
		t.Fatalf("VACUOUS: %d converted files paired with Go sources under %s; the census is measuring nothing", len(pairs), coreDir)
	}

	got := map[string]int{}
	perFile := map[string]map[string]int{} // class -> file -> count
	files, groups := 0, 0

	for _, p := range pairs {
		one := NewResults()

		if err := one.Analyze(p); err != nil {
			continue // a Go file this toolchain cannot parse is not a comment finding
		}

		files += one.Files()
		groups += one.Groups()

		for k, n := range one.Counts() {
			got[k] += n

			if perFile[k] == nil {
				perFile[k] = map[string]int{}
			}
			perFile[k][p.Pkg+"/"+p.Base+".cs"] = n
		}
	}

	if os.Getenv("GO2CS_COMMENT_CENSUS_UPDATE") == "1" {
		writeBaseline(t, got, files, groups, goVersion)
		t.Logf("baseline rewritten: %d classes over %d files", len(got), files)
		return
	}

	want := readBaseline(t)

	keys := map[string]bool{}
	for k := range got {
		keys[k] = true
	}
	for k := range want {
		keys[k] = true
	}

	for _, k := range slices.Sorted(maps.Keys(keys)) {
		g, w := got[k], want[k]

		switch {
		case g > w:
			type fileCount struct {
				file string
				n    int
			}

			var files []fileCount
			for f, n := range perFile[k] {
				files = append(files, fileCount{f, n})
			}

			sort.Slice(files, func(i, j int) bool {
				if files[i].n != files[j].n {
					return files[i].n > files[j].n
				}
				return files[i].file < files[j].file
			})

			var top []string
			for i, fc := range files {
				if i == 10 {
					top = append(top, fmt.Sprintf("... %d more", len(files)-10))
					break
				}
				top = append(top, fmt.Sprintf("%s %d", fc.file, fc.n))
			}

			t.Errorf("ROSE: %s %d -> %d (+%d); files carrying it: %s", k, w, g, g-w, strings.Join(top, ", "))

		case g < w:
			t.Errorf("FELL: %s %d -> %d (-%d); update %s in this commit (GO2CS_COMMENT_CENSUS_UPDATE=1)", k, w, g, w-g, baselinePath)
		}
	}
}
