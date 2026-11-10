// platformLattice_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"sort"
	"strings"
	"testing"
)

// The universes the lattice is exercised over. universeToday is the three-target one of before linux/arm64; universeArm is
// the one docs/phase4/DESIGN-linux-arm64-flavour.md designed, declared since; universeFive adds the fifth target the
// design names as the first place the GOOS-first split order becomes observable.
var (
	universeToday = []string{"windows/amd64", "linux/amd64", "darwin/amd64"}
	universeArm   = []string{"windows/amd64", "linux/amd64", "darwin/amd64", "linux/arm64"}
	universeFive  = []string{"windows/amd64", "linux/amd64", "darwin/amd64", "linux/arm64", "darwin/arm64"}
)

func homeFolders(lattice *platformLattice, sharing []string) string {
	var folders []string

	for _, key := range lattice.homes(sharing) {
		if key.kind == layoutKeyFlat {
			folders = append(folders, "flat")
		} else {
			folders = append(folders, key.folder+"/")
		}
	}

	return strings.Join(folders, " + ")
}

// The design's §2.1 table: every presence subset of the four targets and the keys it is homed at.
func TestPlatformLatticeHomesEveryPresenceSubset(t *testing.T) {
	const w, l, d, a = "windows/amd64", "linux/amd64", "darwin/amd64", "linux/arm64"

	rows := []struct {
		sharing []string
		want    string
	}{
		{[]string{w, l, d, a}, "flat"},
		{[]string{w, l, d}, "amd64/"},
		{[]string{l, a}, "linux/"},
		{[]string{w}, "windows/"},
		{[]string{d}, "darwin/"},
		{[]string{l}, "linux-amd64/"},
		{[]string{a}, "linux-arm64/"},
		{[]string{l, d, a}, "linux/ + darwin/"},
		{[]string{w, l, a}, "windows/ + linux/"},
		{[]string{w, d, a}, "windows/ + darwin/ + linux-arm64/"},
		{[]string{w, l}, "windows/ + linux-amd64/"},
		{[]string{w, d}, "windows/ + darwin/"},
		{[]string{l, d}, "darwin/ + linux-amd64/"},
		{[]string{w, a}, "windows/ + linux-arm64/"},
		{[]string{d, a}, "darwin/ + linux-arm64/"},
	}

	lattice := mustPlatformLattice(universeArm)

	if len(rows) != 1<<len(universeArm)-1 {
		t.Fatalf("the table holds %d rows; four targets have %d non-empty subsets", len(rows), 1<<len(universeArm)-1)
	}

	seen := map[string]bool{}

	for _, row := range rows {
		seen[strings.Join(lattice.ordered(row.sharing), " ")] = true

		if got := homeFolders(lattice, row.sharing); got != row.want {
			t.Errorf("homes(%v) = %q, want %q", row.sharing, got, row.want)
		}
	}

	if len(seen) != len(rows) {
		t.Errorf("the table repeats a subset: %d distinct of %d rows", len(seen), len(rows))
	}
}

// Today's universe is the control: three targets of one architecture, where the rule must be exactly
// "shared by all ⟹ flat, anything else ⟹ one copy per emitting GOOS" and no arch or pair folder is
// ever a home. This is what makes the generalisation's corpus footprint empty.
func TestPlatformLatticeTodayWritesOnlyFlatAndGOOSFolders(t *testing.T) {
	lattice := mustPlatformLattice(universeToday)

	for mask := 1; mask < 1<<len(universeToday); mask++ {
		var sharing, want []string

		for i, target := range universeToday {
			if mask&(1<<i) != 0 {
				sharing = append(sharing, target)
				want = append(want, goosOfTarget(target)+"/")
			}
		}

		expected := strings.Join(want, " + ")

		if len(sharing) == len(universeToday) {
			expected = "flat"
		}

		if got := homeFolders(lattice, sharing); got != expected {
			t.Errorf("homes(%v) = %q, want %q", sharing, got, expected)
		}
	}
}

// The property the layout exists for, over EVERY way the targets of a universe can emit one name:
// each target either does not emit it or belongs to one content class, so with n targets there are
// (n+1)^n labelings covering every partition of every subset. For each one, a target that emitted the
// name must compile exactly one copy and it must be its own class's; a target that did not must
// compile none.
func TestPlatformLatticeNoTargetSeesANameTwice(t *testing.T) {
	for _, universe := range [][]string{universeToday, universeArm, universeFive} {
		lattice := mustPlatformLattice(universe)
		n := len(universe)
		labels := make([]int, n) // 0 = not emitted; k = content class k
		checked := 0

		for {
			hashes := map[string]string{}

			for i, target := range universe {
				if labels[i] != 0 {
					hashes[target] = string(rune('a' + labels[i]))
				}
			}

			copies := map[string][]string{} // target -> the class hash of every copy its build compiles

			for _, class := range partitionContentClasses(universe, hashes) {
				for _, key := range lattice.homes(class) {
					for _, target := range key.cover {
						copies[target] = append(copies[target], hashes[class[0]])
					}
				}
			}

			for _, target := range universe {
				hash, emitted := hashes[target]
				got := copies[target]

				switch {
				case !emitted && len(got) != 0:
					t.Fatalf("universe %v, labeling %v: %s emitted nothing yet compiles %d copies", universe, labels, target, len(got))
				case emitted && len(got) != 1:
					t.Fatalf("universe %v, labeling %v: %s compiles %d copies, want exactly 1", universe, labels, target, len(got))
				case emitted && got[0] != hash:
					t.Fatalf("universe %v, labeling %v: %s compiles another class's bytes", universe, labels, target)
				}
			}

			checked++

			// Next labeling, odometer-style.
			at := 0

			for at < n {
				labels[at]++

				if labels[at] <= n {
					break
				}

				labels[at] = 0
				at++
			}

			if at == n {
				break
			}
		}

		want := 1

		for i := 0; i < n; i++ {
			want *= n + 1
		}

		if checked != want {
			t.Errorf("universe %v: checked %d labelings, want %d", universe, checked, want)
		}
	}
}

// A class is written once per key and never duplicated inside one key's cover: the keys of one class
// are pairwise disjoint and together cover exactly the class.
func TestPlatformLatticeHomesPartitionTheClass(t *testing.T) {
	for _, universe := range [][]string{universeToday, universeArm, universeFive} {
		lattice := mustPlatformLattice(universe)

		for mask := 1; mask < 1<<len(universe); mask++ {
			var sharing []string

			for i, target := range universe {
				if mask&(1<<i) != 0 {
					sharing = append(sharing, target)
				}
			}

			var covered []string

			for _, key := range lattice.homes(sharing) {
				covered = append(covered, key.cover...)
			}

			sort.Strings(covered)
			want := append([]string(nil), sharing...)
			sort.Strings(want)

			if strings.Join(covered, " ") != strings.Join(want, " ") {
				t.Errorf("universe %v: homes(%v) cover %v", universe, sharing, covered)
			}
		}
	}
}

// The GOOS-first split order is unobservable at four targets and observable at five (design §2.2):
// {W, L, D, darwin/arm64} is three GOOS-first keys where an arch-first split would write two.
func TestPlatformLatticeSplitTakesGOOSKeysFirst(t *testing.T) {
	lattice := mustPlatformLattice(universeFive)
	sharing := []string{"windows/amd64", "linux/amd64", "darwin/amd64", "darwin/arm64"}

	if got, want := homeFolders(lattice, sharing), "windows/ + darwin/ + linux-amd64/"; got != want {
		t.Errorf("homes(%v) = %q, want %q", sharing, got, want)
	}
}

func TestPlatformLatticeAdmitsAnArchKeyOnlyWhenShared(t *testing.T) {
	folders := func(universe []string) string {
		var names []string

		for _, key := range mustPlatformLattice(universe).keys {
			if key.kind == layoutKeyArch {
				names = append(names, key.folder)
			}
		}

		return strings.Join(names, " ")
	}

	if got := folders(universeArm); got != "amd64" {
		t.Errorf("arch keys at four targets = %q, want only amd64 (arm64 has one target)", got)
	}

	if got := folders(universeFive); got != "amd64 arm64" {
		t.Errorf("arch keys at five targets = %q, want amd64 and arm64", got)
	}
}

func TestNewPlatformLatticeRefusals(t *testing.T) {
	for name, universe := range map[string][]string{
		"empty":    {},
		"repeated": {"linux/amd64", "windows/amd64", "linux/amd64"},
		"no arch":  {"linux"},
		"no goos":  {"/amd64"},
		"three":    {"linux/amd64/v3"},
	} {
		if _, err := newPlatformLattice(universe); err == nil {
			t.Errorf("%s: newPlatformLattice(%v) succeeded, want a refusal", name, universe)
		}
	}
}

func TestPlatformTargetsMatchUniverse(t *testing.T) {
	if err := platformTargetsMatchUniverse([]string{"darwin/amd64", "windows/amd64", "linux/amd64"}, universeToday); err != nil {
		t.Errorf("the universe in another order was refused: %v", err)
	}

	for name, row := range map[string]struct {
		targets []string
		naming  string
	}{
		"strict subset": {[]string{"windows/amd64", "linux/amd64"}, "missing darwin/amd64"},
		"new target":    {[]string{"windows/amd64", "linux/amd64", "darwin/amd64", "linux/arm64"}, "not in it: linux/arm64"},
		"repeated":      {[]string{"windows/amd64", "linux/amd64", "darwin/amd64", "linux/amd64"}, "repeated"},
	} {
		err := platformTargetsMatchUniverse(row.targets, universeToday)

		if err == nil {
			t.Errorf("%s: %v was accepted", name, row.targets)
		} else if !strings.Contains(err.Error(), row.naming) {
			t.Errorf("%s: the refusal does not name %q: %v", name, row.naming, err)
		}
	}
}

func TestLayoutKeyFolderNames(t *testing.T) {
	for name, want := range map[string]bool{
		"linux": true, "windows": true, "amd64": true, "arm64": true,
		"linux-arm64": true, "windows-amd64": true,
		"internal": false, "amd64v1": false, "linux_amd64": false, "arm64-linux": false, "linux-": false, "": false,
	} {
		if got := isLayoutKeyFolderName(name); got != want {
			t.Errorf("isLayoutKeyFolderName(%q) = %v, want %v", name, got, want)
		}
	}

	for name, want := range map[string]bool{"linux": false, "amd64": true, "linux-arm64": true, "internal": false} {
		if got := isArchLayoutFolderName(name); got != want {
			t.Errorf("isArchLayoutFolderName(%q) = %v, want %v", name, got, want)
		}
	}

	if got := strings.Join(platformLayoutFolders("linux/arm64"), " "); got != "linux-arm64 arm64 linux" {
		t.Errorf("platformLayoutFolders(linux/arm64) = %q", got)
	}

	if got := strings.Join(platformLayoutFolders("linux"), " "); got != "linux" {
		t.Errorf("platformLayoutFolders(linux) = %q, want the bare GOOS folder", got)
	}

	if got := platformLayoutFolders(""); len(got) != 0 {
		t.Errorf("platformLayoutFolders(\"\") = %v, want nothing", got)
	}
}

func TestPlatformLatticeFolderCoversAndRelations(t *testing.T) {
	lattice := mustPlatformLattice(universeArm)

	for folder, want := range map[string]string{
		"":            "windows/amd64 linux/amd64 darwin/amd64 linux/arm64",
		"linux":       "linux/amd64 linux/arm64",
		"amd64":       "windows/amd64 linux/amd64 darwin/amd64",
		"arm64":       "linux/arm64", // not an admitted key, and still a folder one target would compile
		"linux-arm64": "linux/arm64",
		"freebsd":     "",
	} {
		if got := strings.Join(lattice.folderCover(folder), " "); got != want {
			t.Errorf("folderCover(%q) = %q, want %q", folder, got, want)
		}
	}

	for _, row := range []struct {
		a, b string
		want coverRelation
	}{
		{"windows", "linux", coversDisjoint},
		{"linux", "linux-arm64", coversNested},
		{"", "darwin", coversNested},
		{"windows", "amd64", coversNested},
		{"linux", "amd64", coversOverlapping},
		{"arm64", "linux-arm64", coversNested},
		{"freebsd", "linux", coversDisjoint},
	} {
		if got := relateCovers(lattice.folderCover(row.a), lattice.folderCover(row.b)); got != row.want {
			t.Errorf("relateCovers(%q, %q) = %v, want %v", row.a, row.b, got, row.want)
		}
	}

	if got, want := strings.Join(lattice.candidateFolders(), " "),
		"windows linux darwin amd64 arm64 windows-amd64 linux-amd64 darwin-amd64 linux-arm64"; got != want {
		t.Errorf("candidateFolders() = %q, want %q", got, want)
	}
}

func TestPartitionContentClasses(t *testing.T) {
	classes := partitionContentClasses(universeArm, map[string]string{
		"windows/amd64": "x", "darwin/amd64": "y", "linux/arm64": "x",
	})

	var rendered []string

	for _, class := range classes {
		rendered = append(rendered, strings.Join(class, ","))
	}

	if got, want := strings.Join(rendered, " | "), "windows/amd64,linux/arm64 | darwin/amd64"; got != want {
		t.Errorf("partitionContentClasses = %q, want %q", got, want)
	}
}
