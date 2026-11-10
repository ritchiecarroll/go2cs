// platformCanonicalForm_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"bytes"
	"fmt"
	"io/fs"
	"os"
	"path/filepath"
	"sort"
	"strings"
	"testing"
)

// The canonical-form guard of docs/phase4/DESIGN-linux-arm64-flavour.md §2.4: a check of the TREE,
// with no conversion behind it, that every source name of every package sits where the key lattice
// puts it. The merge writes that form, but the merge only speaks for what it emitted: a hand-owned
// file is placed by a person, a single-target conversion writes into whatever folders it finds, and a
// name every target reproduces byte for byte is in no plan at all. This is the instrument that reads
// what the tree actually holds.
//
// Three properties, per package and file name:
//
//   - ONE LOCATION. No two folders holding the name have intersecting covers. Otherwise one target's
//     project compiles two copies (CS0579 on the assembly attributes, CS0111 on the members), or
//     compiles one while the other silently rots.
//   - CANONICAL. Copies with identical bytes sit in exactly the keys the rule gives for the union of
//     their covers. Three identical per-GOOS copies of a name every target shares is the same build
//     as one flat copy, and it is three files to keep in step by hand; the rule having one answer is
//     what makes a reconvert's footprint readable.
//   - COVERED. A key-shaped source folder no target of the universe compiles holds nothing. Its
//     files are dead in every build of the corpus and read exactly like live platform code.
//
// Bytes are compared with CR removed: a hand-owned file can be checked out with either line ending,
// and two copies that differ only there are one content.

// canonicalFormViolations reads every package under coreDir and returns one line per violation, the
// number of (package, name) pairs it judged, and how many of those sit in a key folder.
func canonicalFormViolations(coreDir string, lattice *platformLattice) (violations []string, names int, keyed int, err error) {
	err = filepath.WalkDir(coreDir, func(dir string, entry fs.DirEntry, walkErr error) error {
		if walkErr != nil {
			return walkErr
		}

		if !entry.IsDir() {
			return nil
		}

		switch entry.Name() {
		case "bin", "obj", "Generated", ".vs":
			return filepath.SkipDir
		}

		children, readErr := os.ReadDir(dir)

		if readErr != nil {
			return readErr
		}

		if !holdsProjectFile(children) {
			return nil
		}

		// folder ("" is flat) by source name. A key-shaped child that holds a project file is a
		// nested PACKAGE named for a platform, `internal/syscall/windows` being the standing example,
		// and none of its sources are this package's.
		locations := map[string][]string{}

		for _, child := range children {
			if !child.IsDir() {
				if isLayoutSourceName(child.Name()) {
					locations[child.Name()] = append(locations[child.Name()], "")
				}

				continue
			}

			if !isLayoutKeyFolderName(child.Name()) {
				continue
			}

			grandchildren, readErr := os.ReadDir(filepath.Join(dir, child.Name()))

			if readErr != nil {
				return readErr
			}

			if holdsProjectFile(grandchildren) {
				continue
			}

			for _, grandchild := range grandchildren {
				if !grandchild.IsDir() && isLayoutSourceName(grandchild.Name()) {
					locations[grandchild.Name()] = append(locations[grandchild.Name()], child.Name())
				}
			}
		}

		relative, relErr := filepath.Rel(coreDir, dir)

		if relErr != nil {
			return relErr
		}

		packagePath := filepath.ToSlash(relative)

		sourceNames := make([]string, 0, len(locations))

		for name := range locations {
			sourceNames = append(sourceNames, name)
		}

		sort.Strings(sourceNames)

		for _, name := range sourceNames {
			folders := orderLayoutFolders(lattice, locations[name])
			names++

			if len(folders) > 1 || folders[0] != "" {
				keyed++
			}

			found, checkErr := canonicalFormOfName(dir, name, folders, lattice)

			if checkErr != nil {
				return checkErr
			}

			for _, violation := range found {
				violations = append(violations, fmt.Sprintf("%s/%s: %s", packagePath, name, violation))
			}
		}

		return nil
	})

	return violations, names, keyed, err
}

func holdsProjectFile(entries []fs.DirEntry) bool {
	for _, entry := range entries {
		if !entry.IsDir() && strings.HasSuffix(entry.Name(), ".csproj") {
			return true
		}
	}

	return false
}

// A converted source, or the review copy a hand-owned one keeps beside it.
func isLayoutSourceName(name string) bool {
	return strings.HasSuffix(name, ".cs") || strings.HasSuffix(name, ".cs.auto")
}

// orderLayoutFolders puts the folders holding one name in the lattice's own order, coarsest first:
// flat, then each candidate folder as the lattice lists it, then anything it has no name for. A
// violation then reads the same on every file system, whatever order the directory was listed in.
func orderLayoutFolders(lattice *platformLattice, folders []string) []string {
	rank := map[string]int{"": -1}

	for i, folder := range lattice.candidateFolders() {
		rank[folder] = i
	}

	position := func(folder string) int {
		if at, known := rank[folder]; known {
			return at
		}

		return len(rank)
	}

	ordered := append([]string(nil), folders...)

	sort.SliceStable(ordered, func(i, j int) bool {
		if position(ordered[i]) != position(ordered[j]) {
			return position(ordered[i]) < position(ordered[j])
		}

		return ordered[i] < ordered[j]
	})

	return ordered
}

func describeLayoutFolders(folders []string) string {
	described := make([]string, len(folders))

	for i, folder := range folders {
		if len(folder) == 0 {
			described[i] = "flat"
		} else {
			described[i] = folder + "/"
		}
	}

	return strings.Join(described, " + ")
}

// canonicalFormOfName judges one name of one package against the three properties. The bytes are
// read only when the name sits in more than one folder: a single copy has one content class, and its
// place is decided by its cover alone.
func canonicalFormOfName(packageDir, name string, folders []string, lattice *platformLattice) ([]string, error) {
	var violations []string

	for _, folder := range folders {
		if len(lattice.folderCover(folder)) == 0 {
			violations = append(violations, fmt.Sprintf("COVERED: %s is compiled by no target of %v",
				describeLayoutFolders([]string{folder}), lattice.universe))
		}
	}

	oneLocation := true

	for i := range folders {
		for j := i + 1; j < len(folders); j++ {
			if relateCovers(lattice.folderCover(folders[i]), lattice.folderCover(folders[j])) != coversDisjoint {
				oneLocation = false

				violations = append(violations, fmt.Sprintf("ONE LOCATION: %s both hold it, and a target both cover compiles two copies",
					describeLayoutFolders([]string{folders[i], folders[j]})))
			}
		}
	}

	// Two copies one target compiles is the finding; where each "should" live is not decidable from
	// the tree until that is resolved. An uncovered folder has no cover to home.
	if !oneLocation || len(violations) > 0 {
		return violations, nil
	}

	classes := map[string][]string{} // content -> the folders holding it
	var order []string

	if len(folders) == 1 {
		classes[""] = folders
		order = []string{""}
	} else {
		for _, folder := range folders {
			contents, err := os.ReadFile(filepath.Join(packageDir, folder, name))

			if err != nil {
				return nil, err
			}

			content := string(bytes.ReplaceAll(contents, []byte("\r"), nil))

			if _, exists := classes[content]; !exists {
				order = append(order, content)
			}

			classes[content] = append(classes[content], folder)
		}
	}

	for _, content := range order {
		held := classes[content]
		var sharing []string

		for _, folder := range held {
			sharing = append(sharing, lattice.folderCover(folder)...)
		}

		var want []string

		for _, key := range lattice.homes(sharing) {
			want = append(want, key.folder)
		}

		sortedHeld := append([]string(nil), held...)
		sort.Strings(sortedHeld)
		sortedWant := append([]string(nil), want...)
		sort.Strings(sortedWant)

		if strings.Join(sortedHeld, "\x00") != strings.Join(sortedWant, "\x00") {
			violations = append(violations, fmt.Sprintf("CANONICAL: identical in %s, and the rule homes %v at %s",
				describeLayoutFolders(held), lattice.ordered(sharing), describeLayoutFolders(want)))
		}
	}

	return violations, nil
}

// The guard over the repository's own corpus, at the declared universe.
func TestCorpusLayoutIsCanonical(t *testing.T) {
	coreDir := filepath.Join(repoRootFromPackageDir(t), "src", "core")
	violations, names, keyed, err := canonicalFormViolations(coreDir, corpusLattice())

	if err != nil {
		t.Fatalf("reading %s: %v", coreDir, err)
	}

	// A guard that walked the wrong root, or a root some cleanup emptied, judges nothing and passes.
	// The corpus holds several thousand source names and several hundred of them vary by platform;
	// the floors are a fraction of both, so they hold across any real change and fail an empty read.
	if names < 2000 || keyed < 200 {
		t.Fatalf("judged %d source names under %s, %d of them in a key folder; that is not the corpus",
			names, coreDir, keyed)
	}

	t.Logf("judged %d source names, %d of them in a key folder (universe %v)", names, keyed, corpusLattice().universe)

	for _, violation := range violations {
		t.Error(violation)
	}

	if len(violations) > 0 {
		t.Errorf("%d layout violations over %d names (universe %v)", len(violations), names, corpusLattice().universe)
	}
}

// Every violation class the guard exists for, planted in a tree of its own, and beside each the
// clean shape it must NOT report. The four-target lattice is the one the guard is written for; the
// three-target rows pin today's reading.
func TestCanonicalFormCheckNamesEachViolation(t *testing.T) {
	const x, y = "// one content\n", "// another content\n"

	rows := []struct {
		name     string
		universe []string
		files    map[string]string // path under the package -> contents
		want     []string          // a substring of each expected violation, in order; empty = clean
	}{
		{"shared by all, flat", universeArm, map[string]string{"a.cs": x}, nil},
		{"one copy per GOOS, all different", universeToday,
			map[string]string{"windows/a.cs": x, "linux/a.cs": y, "darwin/a.cs": "// third\n"}, nil},
		{"two GOOS folders identical, the third differs", universeToday,
			map[string]string{"windows/a.cs": x, "linux/a.cs": x, "darwin/a.cs": y}, nil},
		{"line endings are not content", universeToday,
			map[string]string{"windows/a.cs": "// r\r\n", "linux/a.cs": "// r\n", "darwin/a.cs": "// r\r\n"},
			[]string{"CANONICAL: identical in windows/ + linux/ + darwin/, and the rule homes [windows/amd64 linux/amd64 darwin/amd64] at flat"}},
		{"flat beside a GOOS folder", universeToday,
			map[string]string{"a.cs": x, "darwin/a.cs": x},
			[]string{"ONE LOCATION: flat + darwin/"}},
		{"flat beside a GOOS folder, different bytes", universeToday,
			map[string]string{"a.cs": x, "darwin/a.cs": y},
			[]string{"ONE LOCATION: flat + darwin/"}},
		{"three identical per-GOOS copies", universeToday,
			map[string]string{"windows/a.cs": x, "linux/a.cs": x, "darwin/a.cs": x},
			[]string{"CANONICAL: identical in windows/ + linux/ + darwin/"}},
		{"an arch folder where one architecture is the whole universe", universeToday,
			map[string]string{"amd64/a.cs": x},
			[]string{"CANONICAL: identical in amd64/, and the rule homes [windows/amd64 linux/amd64 darwin/amd64] at flat"}},
		{"a pair folder where the GOOS has one target", universeToday,
			map[string]string{"linux-amd64/a.cs": x},
			[]string{"CANONICAL: identical in linux-amd64/, and the rule homes [linux/amd64] at linux/"}},
		{"a folder no target compiles", universeToday,
			map[string]string{"linux-arm64/a.cs": x},
			[]string{"COVERED: linux-arm64/ is compiled by no target"}},
		{"amd64 shared, arm64 its own", universeArm,
			map[string]string{"amd64/a.cs": x, "linux-arm64/a.cs": y}, nil},
		{"linux shared across architectures", universeArm,
			map[string]string{"linux/a.cs": x, "windows/a.cs": y}, nil},
		{"a GOOS folder beside its own pair folder", universeArm,
			map[string]string{"linux/a.cs": x, "linux-arm64/a.cs": y},
			[]string{"ONE LOCATION: linux/ + linux-arm64/"}},
		{"overlapping covers, identical bytes", universeArm,
			map[string]string{"linux/a.cs": x, "amd64/a.cs": x},
			[]string{"ONE LOCATION: linux/ + amd64/"}},
		{"the two linux pairs identical", universeArm,
			map[string]string{"linux-amd64/a.cs": x, "linux-arm64/a.cs": x},
			[]string{"CANONICAL: identical in linux-amd64/ + linux-arm64/, and the rule homes [linux/amd64 linux/arm64] at linux/"}},
		{"amd64 spelled as three folders", universeArm,
			map[string]string{"windows/a.cs": x, "linux-amd64/a.cs": x, "darwin/a.cs": x, "linux-arm64/a.cs": y},
			[]string{"at amd64/"}},
		{"an unadmitted arch folder", universeArm,
			map[string]string{"arm64/a.cs": x},
			[]string{"CANONICAL: identical in arm64/, and the rule homes [linux/arm64] at linux-arm64/"}},
		{"a review copy is judged by its own content", universeArm,
			map[string]string{"a.cs": x, "linux/a.cs.auto": x, "linux-arm64/a.cs.auto": x},
			[]string{"a.cs.auto: ONE LOCATION: linux/ + linux-arm64/"}},
		{"a nested package named for a key", universeArm,
			map[string]string{"a.cs": x, "windows/a.cs": x, "windows/windows.csproj": "<Project />",
				"amd64/a.cs": y, "amd64/amd64.csproj": "<Project />"}, nil},
	}

	for _, row := range rows {
		coreDir := filepath.Join(t.TempDir(), "core")
		packageDir := filepath.Join(coreDir, "some", "pkg")

		writeTestFile(t, filepath.Join(packageDir, "pkg.csproj"), "<Project />")

		for path, contents := range row.files {
			writeTestFile(t, filepath.Join(packageDir, filepath.FromSlash(path)), contents)
		}

		violations, _, _, err := canonicalFormViolations(coreDir, mustPlatformLattice(row.universe))

		if err != nil {
			t.Errorf("%s: %v", row.name, err)
			continue
		}

		if len(violations) != len(row.want) {
			t.Errorf("%s: %d violations, want %d:\n  %s", row.name, len(violations), len(row.want), strings.Join(violations, "\n  "))
			continue
		}

		for i, want := range row.want {
			if !strings.Contains(violations[i], want) {
				t.Errorf("%s: violation %d = %q, want it to hold %q", row.name, i, violations[i], want)
			}

			if !strings.HasPrefix(violations[i], "some/pkg/") {
				t.Errorf("%s: violation %d does not name its package and file: %q", row.name, i, violations[i])
			}
		}
	}
}
