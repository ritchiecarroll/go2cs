// linkStagedFixtures_test.go - Gbtc
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
	"strings"
	"testing"
)

// The RUNNABLE-PROGRAM fixture predicate, over a synthetic tree.
//
// The predicate decides which fixture directories are staged as a LINK into the real Go source tree
// instead of as file copies, which is what lets the toolchain accept their `internal/…` imports.
// Both directions are load-bearing and fail differently: selecting too little leaves the refusal in
// place (a named verdict loss), and selecting too much link-stages a tree a test may WRITE to, where
// the write refusal turns a passing row red. So the synthetic tree carries the accepting shapes AND
// the shapes measured over $GOROOT/src that must keep the copy path — go/doc's parse fixtures
// (`.go` files, no `package main`) and internal/types' check fixtures (mostly non-main, a few main).
func writeFile(t *testing.T, path, contents string) {
	t.Helper()

	if err := os.MkdirAll(filepath.Dir(path), 0755); err != nil {
		t.Fatalf("mkdir %s: %v", filepath.Dir(path), err)
	}
	if err := os.WriteFile(path, []byte(contents), 0644); err != nil {
		t.Fatalf("write %s: %v", path, err)
	}
}

// syntheticFixtureTree builds a package directory holding every shape the predicate has to rule on
// and returns its path.
func syntheticFixtureTree(t *testing.T) string {
	t.Helper()

	root := t.TempDir()

	// The package's own sources — outside testdata, so never a candidate however they are declared.
	writeFile(t, filepath.Join(root, "pkg.go"), "package pkg\n")
	writeFile(t, filepath.Join(root, "pkg_test.go"), "package pkg\n")

	// A runnable-program tree: every .go file is `package main`. SELECTED.
	writeFile(t, filepath.Join(root, "testdata", "testprog", "a.go"), "//go:build ignore\n\npackage main\n\nimport _ \"internal/profile\"\n\nfunc main() {}\n")
	writeFile(t, filepath.Join(root, "testdata", "testprog", "b.go"), "package main\n\nfunc helper() {}\n")
	// A non-.go file in the same directory plays no part in the decision.
	writeFile(t, filepath.Join(root, "testdata", "testprog", "README"), "not go\n")

	// Parse fixtures — .go files, none of them `package main`. NOT selected (go/doc's shape).
	writeFile(t, filepath.Join(root, "testdata", "docfixtures", "a.go"), "package a\n")
	writeFile(t, filepath.Join(root, "testdata", "docfixtures", "b.go"), "package b\n")

	// Type-check fixtures — mostly non-main with a few main. NOT selected (internal/types' shape),
	// and this is the row that proves the predicate is EVERY file rather than ANY file.
	writeFile(t, filepath.Join(root, "testdata", "check", "one.go"), "package main\n\nfunc main() {}\n")
	writeFile(t, filepath.Join(root, "testdata", "check", "two.go"), "package p\n")

	// No .go files at all. NOT selected — a link is for a tree the toolchain COMPILES.
	writeFile(t, filepath.Join(root, "testdata", "corpus", "input.bin"), "\x00\x01")

	// A file that will not parse keeps its directory on the copy path: a link is the sharper
	// instrument and an unreadable package clause is not evidence for it.
	writeFile(t, filepath.Join(root, "testdata", "broken", "a.go"), "package main\n")
	writeFile(t, filepath.Join(root, "testdata", "broken", "b.go"), "this is not go source\n")

	// A `package main` tree OUTSIDE testdata is a sibling package, not a fixture. NOT selected.
	writeFile(t, filepath.Join(root, "cmd", "tool", "main.go"), "package main\n\nfunc main() {}\n")

	// go/build's ignored-directory convention: never descended into.
	writeFile(t, filepath.Join(root, "testdata", "_ignored", "main.go"), "package main\n\nfunc main() {}\n")

	return root
}

func TestLinkStagedFixtureDirsSelectsOnlyRunnableProgramTrees(t *testing.T) {
	root := syntheticFixtureTree(t)

	selected, err := linkStagedFixtureDirs(root)
	if err != nil {
		t.Fatalf("linkStagedFixtureDirs: %v", err)
	}

	want := []string{"testdata/testprog"}

	if strings.Join(selected, ",") != strings.Join(want, ",") {
		t.Fatalf("selection mismatch\n got: %v\nwant: %v", selected, want)
	}
}

// A selected directory is staged WHOLE, so a qualifying directory nested inside another qualifying
// one is never listed separately — descending would plant a link inside a link, which is a write
// into the real Go installation. internal/coverage/cfile is the live instance: its `testdata`
// qualifies and carries `issue56006`, which qualifies too.
func TestLinkStagedFixtureDirsTakesTheOutermostQualifyingDirectory(t *testing.T) {
	root := t.TempDir()

	writeFile(t, filepath.Join(root, "testdata", "harness.go"), "package main\n\nfunc main() {}\n")
	writeFile(t, filepath.Join(root, "testdata", "issue56006", "repro.go"), "package main\n\nfunc main() {}\n")
	writeFile(t, filepath.Join(root, "testdata", "issue59563", "coverpkg.go"), "package p\n")

	selected, err := linkStagedFixtureDirs(root)
	if err != nil {
		t.Fatalf("linkStagedFixtureDirs: %v", err)
	}

	if len(selected) != 1 || selected[0] != "testdata" {
		t.Fatalf("expected the outermost qualifying directory alone, got %v", selected)
	}
}

// The two consumers of the split must disagree, deliberately and in one direction only: the fixture
// list the csproj and the host read EXCLUDES what the link carries (a copy beside the link would be
// a second, divergent truth), while the digest input keeps every file (F7 — editing a fixture must
// still invalidate a prior comparison, and it is the same edit whether the tree is copied or
// linked).
func TestLinkStagedFilesLeaveTheCopySetButNotTheDigestSet(t *testing.T) {
	root := syntheticFixtureTree(t)

	all, err := testFixturePaths(root)
	if err != nil {
		t.Fatalf("testFixturePaths: %v", err)
	}

	copied, links, err := copyTestFixtures(root, root)
	if err != nil {
		t.Fatalf("copyTestFixtures: %v", err)
	}

	if len(links) != 1 || links[0] != "testdata/testprog" {
		t.Fatalf("expected testdata/testprog to be link-staged, got %v", links)
	}

	for _, linked := range []string{"testdata/testprog/a.go", "testdata/testprog/b.go", "testdata/testprog/README"} {
		if !containsString(all, linked) {
			t.Fatalf("digest input lost %q — a fixture edit inside a link-staged tree would stop invalidating a prior comparison (F7)", linked)
		}
		if containsString(copied, linked) {
			t.Fatalf("copy set still carries %q — the csproj would emit a <None> item and the host would stage a copy beside the link", linked)
		}
	}

	// Everything the link does NOT carry is untouched by the split.
	for _, kept := range []string{"pkg.go", "testdata/docfixtures/a.go", "testdata/check/two.go", "testdata/corpus/input.bin"} {
		if !containsString(copied, kept) {
			t.Fatalf("copy set lost %q, which no link carries", kept)
		}
	}
}

// F7, stated where it can fail: editing a fixture INSIDE a link-staged tree must still invalidate a
// prior comparison. It is the one property the link could plausibly have broken — the tree's files
// left the csproj and the host's fixture list, and leaving the digest along with them would have
// made a fixture edit invisible to staleness detection. It does not, because the digest walks the
// real directory at conversion time and never consults either of those lists.
func TestInputDigestMovesWhenALinkStagedFixtureIsEdited(t *testing.T) {
	root := syntheticFixtureTree(t)
	options := Options{targetPlatform: "windows/amd64"}

	before, err := testInputDigest(root, root, options, "rev")
	if err != nil {
		t.Fatalf("testInputDigest: %v", err)
	}

	writeFile(t, filepath.Join(root, "testdata", "testprog", "b.go"), "package main\n\nfunc helper() { println(\"edited\") }\n")

	after, err := testInputDigest(root, root, options, "rev")
	if err != nil {
		t.Fatalf("testInputDigest: %v", err)
	}

	if before == after {
		t.Fatal("editing a fixture inside a link-staged tree did not move the input digest — a stale comparison would be reported as current (F7)")
	}
}
