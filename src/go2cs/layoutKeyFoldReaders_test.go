// layoutKeyFoldReaders_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"os"
	"path/filepath"
	"testing"
)

// Two readers of the corpus layout that named windows, linux and darwin themselves, and kept passing
// when the corpus gained amd64/, linux-amd64/ and linux-arm64/: the declared-not-implemented census
// read each of those folders as a package of its own (60 packages where the corpus holds 43), and the
// ref-lowering census reported a package laid out by architecture alone as not laid out. Both now ask
// the lattice, and these tests hold them to every key folder of every target in platformUniverse.

// universeLayoutKeyFolders returns every key folder a target of platformUniverse lays files into.
func universeLayoutKeyFolders(t *testing.T) []string {
	t.Helper()

	var folders []string

	for _, target := range platformUniverse {
		folders = append(folders, platformLayoutFolders(target)...)
	}

	if len(folders) < 3*len(platformUniverse) {
		t.Fatalf("VACUOUS: %d key folders for %d targets; every target has a pair, a GOARCH and a GOOS folder", len(folders), len(platformUniverse))
	}

	return folders
}

func TestMemberKeyFoldsEveryLayoutKeyFolder(t *testing.T) {
	for _, folder := range universeLayoutKeyFolders(t) {
		if got := memberKey("pkg/sub/"+folder+"/x.cs", "f"); got != "pkg/sub.f" {
			t.Errorf("memberKey folds pkg/sub/%s/x.cs to %q, want %q: a member in that folder is keyed to a package that does not exist", folder, got, "pkg/sub.f")
		}
	}

	if got := memberKey("pkg/sub/x.cs", "f"); got != "pkg/sub.f" {
		t.Errorf("memberKey folds the flat pkg/sub/x.cs to %q, want %q", got, "pkg/sub.f")
	}
}

func TestPackageIsLayoutL3ReadsEveryLayoutKeyFolder(t *testing.T) {
	root := t.TempDir()

	plant := func(relative string) {
		t.Helper()

		path := filepath.Join(root, "core", filepath.FromSlash(relative))

		if err := os.MkdirAll(filepath.Dir(path), 0755); err != nil {
			t.Fatal(err)
		}

		if err := os.WriteFile(path, []byte("// fixture\n"), 0644); err != nil {
			t.Fatal(err)
		}
	}

	for _, folder := range universeLayoutKeyFolders(t) {
		pkg := "laid-" + folder
		plant(pkg + "/x.cs")
		plant(pkg + "/" + folder + "/y.cs")

		if !packageIsLayoutL3(root, pkg) {
			t.Errorf("a package whose only key folder is %s/ reads as not laid out", folder)
		}
	}

	// A nested PACKAGE named after a GOOS is not a layout folder: it holds a project file.
	plant("nested/x.cs")
	plant("nested/windows/y.cs")
	plant("nested/windows/nested.windows.csproj")

	if packageIsLayoutL3(root, "nested") {
		t.Errorf("a package whose windows/ child is a nested package (it holds a project file) reads as laid out")
	}

	plant("flat/x.cs")

	if packageIsLayoutL3(root, "flat") {
		t.Errorf("a flat package reads as laid out")
	}
}
