// layoutKeyLists_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package repoguard

import (
	"regexp"
	"slices"
	"sort"
	"strings"
	"testing"
)

// EVERY HAND-WRITTEN LIST OF LAYOUT KEY FOLDERS IS THE UNIVERSE'S (the linux/arm64 corpus apply, 2026-11-10).
//
// The corpus lays a package's files into key folders: a target's GOOS, its GOARCH and the pair. A tool outside the
// converter that names those folders in a list of its own stops seeing files the day the layout gains a folder. With
// windows, linux and darwin alone, go.lib.pack.targets wrote no build/<id>.props for the nine packages whose
// package_info.cs sits in amd64/ and linux-arm64/ (math, hash/crc32), and handown-upstream.ps1 read
// hash/crc32/amd64/crc32_amd64.cs as a hand-own of a package hash/crc32/amd64, which has no upstream.
//
// MSBuild and PowerShell cannot read platformLattice.go, so each tool keeps its list and this test holds the list equal
// to lineKeyedFolders(), which TestLineKeyedRecordsLandOnTheirCode holds to the converter's platformUniverse. A list
// that is no longer found fails as well: a pin with nothing to read would pass forever.
var layoutKeyLists = []struct {
	file    string
	pattern *regexp.Regexp
}{
	{"src/core/golib/buildTransitive/go.lib.pack.targets", regexp.MustCompile(`<_GoLayoutKey Include="([^"]*)" />`)},
	{"src/handown-upstream.ps1", regexp.MustCompile(`(?m)^\s*\$layoutKeys = @\(([^)]*)\)`)},
}

func TestLayoutKeyListsAreTheUniverses(t *testing.T) {
	want := lineKeyedFolders()

	for _, list := range layoutKeyLists {
		found := list.pattern.FindAllStringSubmatch(readRepoFile(t, list.file), -1)

		if len(found) != 1 {
			t.Errorf("%s: %d lists match %s, want 1; the layout key list moved and this pin reads nothing", list.file, len(found), list.pattern)
			continue
		}

		got := strings.FieldsFunc(found[0][1], func(r rune) bool { return strings.ContainsRune(";,' ", r) })
		sort.Strings(got)

		if !slices.Equal(got, want) {
			t.Errorf("%s lists the layout key folders %v and the universe lays files into %v; they change together", list.file, got, want)
		}
	}
}
