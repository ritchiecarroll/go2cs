// fixturesCurrent_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package repoguard

import (
	"bufio"
	"crypto/sha1"
	"encoding/hex"
	"fmt"
	"go/build"
	"os"
	"os/exec"
	"path/filepath"
	"runtime"
	"sort"
	"strings"
	"testing"

	"go2cs/internal/releasestamp"
)

// Every tracked fixture under src/core/**/testdata/ is a COPY of the same path under GOROOT/src: the
// -tests pipeline stages it (testConversion.go copyTestFixtures, which overwrites any copy whose bytes
// differ). So a tracked fixture that differs from GOROOT at the pinned <GoStdLibVersion> is STALE, and
// the next -tests run of its package rewrites it and leaves the tree dirty. That is what the go/printer
// row did at the TRAIN K union (2026-09-30): it was re-banked at 1.24.13 (a71adf7b91) against staged
// 1.24.13 fixtures, the bank commit did not carry them, and generics.input/.golden stayed byte-equal to
// go1.23.12 until the i9's shard restaged them and read the rewrite as "a test mutating testdata".
// C1's census then: 1276 tracked fixtures, 1274 equal to go1.24.13, exactly those 2 stale.
//
// This guard holds the tree to the pin, so a hop re-bank that leaves a fixture behind fails HERE, by
// name, instead of on a later sweep's dirty tree. A tracked fixture fails when its bytes differ from
// GOROOT's at the pinned version (the finding names every locally cached toolchain whose bytes it DOES
// match, which is how a hop leftover reads as "== go1.23.12"), or when that GOROOT has no such file (a
// fixture Go deleted, left behind).
//
// WHAT IT READS: the COMMITTED tree (`git ls-tree -r HEAD`), never the working copy. A -tests run
// restages fixtures into the working copy, so a working-copy read would pass exactly the dirty tree
// this guard exists to catch; run it with -count=1, since it reads the git tree and Go's test cache
// cannot see a commit change. Comparison is by git blob hash of GOROOT's bytes, with no normalization:
// the testdata trees are -text in .gitattributes, so git stores their bytes verbatim (measured: all 1274
// current fixtures hash-equal).
//
// WHICH GOROOT: the running toolchain's, when its VERSION is the pin; else the pinned toolchain in the
// module cache (golang.org/toolchain@v0.0.1-go<pin>.<GOOS>-<GOARCH>, where GOTOOLCHAIN=go<pin> puts it).
// Neither found FAILS rather than skips: a guard that skips on the boxes that cannot measure it proves
// nothing on them, and every lane runs pinned.
//
// POSITIVE CONTROL: TestFixtureCurrencyScannerFires drives the same scanner over a synthetic GOROOT pair
// (pinned and one older release): a current fixture, a stale one whose bytes match the older release, a
// stale one matching neither, and one the pinned GOROOT lacks. The last three must be reported, each with
// its kind and matches, and the first must not.

// trackedFixture is one committed fixture: its repository-relative path and its git blob hash.
type trackedFixture struct {
	path string
	blob string
}

// fixtureFinding is one fixture that is not current at the pinned GOROOT.
type fixtureFinding struct {
	path    string
	kind    string   // "differs" or "absent at the pinned GOROOT"
	matches []string // other local toolchain versions whose bytes equal the tracked blob
}

func (f fixtureFinding) String() string {
	match := "matches no locally cached toolchain"

	if len(f.matches) > 0 {
		match = "bytes == " + strings.Join(f.matches, ", ")
	}

	return fmt.Sprintf("%s: %s (%s)", f.path, f.kind, match)
}

// gitBlobHash is the SHA-1 git names these bytes by as a blob.
func gitBlobHash(data []byte) string {
	h := sha1.New()
	fmt.Fprintf(h, "blob %d\x00", len(data))
	h.Write(data)

	return hex.EncodeToString(h.Sum(nil))
}

// committedFixtures lists every blob under src/core whose path has a testdata segment, from HEAD.
func committedFixtures(t *testing.T, root string) []trackedFixture {
	t.Helper()

	out, err := exec.Command("git", "-C", root, "ls-tree", "-r", "-z", "HEAD", "--", "src/core").Output()

	if err != nil {
		t.Fatalf("git ls-tree failed in %s: %v", root, err)
	}

	var fixtures []trackedFixture

	for _, record := range strings.Split(string(out), "\x00") {
		meta, path, ok := strings.Cut(record, "\t")

		if !ok || !hasPathSegment(path, "testdata") {
			continue
		}

		fields := strings.Fields(meta)

		if len(fields) != 3 || fields[1] != "blob" {
			continue
		}

		fixtures = append(fixtures, trackedFixture{path: path, blob: fields[2]})
	}

	return fixtures
}

func hasPathSegment(path, segment string) bool {
	for _, part := range strings.Split(path, "/") {
		if part == segment {
			return true
		}
	}

	return false
}

// goRootVersion is the first line of GOROOT/VERSION ("go1.24.13"), or "".
func goRootVersion(goRoot string) string {
	file, err := os.Open(filepath.Join(goRoot, "VERSION"))

	if err != nil {
		return ""
	}

	defer file.Close()

	scanner := bufio.NewScanner(file)

	if scanner.Scan() {
		return strings.TrimSpace(scanner.Text())
	}

	return ""
}

// cachedToolchains maps each toolchain version in the module cache for this GOOS/GOARCH to its GOROOT.
func cachedToolchains() map[string]string {
	modCache := os.Getenv("GOMODCACHE")

	if modCache == "" {
		gopath := build.Default.GOPATH

		if first, _, found := strings.Cut(gopath, string(os.PathListSeparator)); found {
			gopath = first
		}

		modCache = filepath.Join(gopath, "pkg", "mod")
	}

	suffix := "." + runtime.GOOS + "-" + runtime.GOARCH
	matches, _ := filepath.Glob(filepath.Join(modCache, "golang.org", "toolchain@v0.0.1-go*"+suffix))
	roots := map[string]string{}

	for _, dir := range matches {
		name := strings.TrimSuffix(filepath.Base(dir), suffix)
		version := strings.TrimPrefix(name, "toolchain@v0.0.1-")

		if goRootVersion(dir) == version {
			roots[version] = dir
		}
	}

	return roots
}

// scanFixtureCurrency compares each fixture with the same path under goRoot/src. others maps a version
// to a GOROOT, used only to say which release a stale fixture's bytes came from.
func scanFixtureCurrency(fixtures []trackedFixture, goRoot string, others map[string]string) (current int, findings []fixtureFinding) {
	versions := make([]string, 0, len(others))

	for version := range others {
		versions = append(versions, version)
	}

	sort.Strings(versions)

	sourceOf := func(root, path string) string {
		return filepath.Join(root, "src", filepath.FromSlash(strings.TrimPrefix(path, "src/core/")))
	}

	for _, fixture := range fixtures {
		kind := ""
		data, err := os.ReadFile(sourceOf(goRoot, fixture.path))

		switch {
		case err != nil:
			kind = "absent at the pinned GOROOT"
		case gitBlobHash(data) != fixture.blob:
			kind = "differs from the pinned GOROOT"
		default:
			current++
			continue
		}

		finding := fixtureFinding{path: fixture.path, kind: kind}

		for _, version := range versions {
			if other, err := os.ReadFile(sourceOf(others[version], fixture.path)); err == nil && gitBlobHash(other) == fixture.blob {
				finding.matches = append(finding.matches, version)
			}
		}

		findings = append(findings, finding)
	}

	return current, findings
}

// TestTrackedFixturesMatchPinnedGoRoot is the guard.
func TestTrackedFixturesMatchPinnedGoRoot(t *testing.T) {
	root := repoRootFromPackageDir(t)
	props, err := os.ReadFile(filepath.Join(root, "src", "version.props"))

	if err != nil {
		t.Fatalf("cannot read src/version.props: %v", err)
	}

	pin := releasestamp.StdLibVersion(string(props))

	if pin == "" {
		t.Fatalf("src/version.props has no <GoStdLibVersion>; the fixture pin has no source")
	}

	want := "go" + pin
	toolchains := cachedToolchains()
	goRoot := build.Default.GOROOT

	if goRootVersion(goRoot) != want {
		goRoot = toolchains[want]
	}

	if goRoot == "" {
		t.Fatalf("no GOROOT at the pinned %s: the running toolchain is %s (GOROOT %s) and the module cache holds no %s toolchain for %s/%s. "+
			"Run with GOTOOLCHAIN=%s, as every lane does; this guard fails rather than skips, since a skip measures nothing",
			want, runtime.Version(), build.Default.GOROOT, want, runtime.GOOS, runtime.GOARCH, want)
	}

	delete(toolchains, want)
	fixtures := committedFixtures(t, root)
	current, findings := scanFixtureCurrency(fixtures, goRoot, toolchains)

	if len(fixtures) < 1000 || current < 1000 {
		t.Fatalf("VACUOUS: %d tracked fixtures, %d current; the scan is measuring nothing", len(fixtures), current)
	}

	t.Logf("pinned %s (GOROOT %s) · tracked fixtures %d · current %d · stale %d · other cached toolchains %d",
		want, goRoot, len(fixtures), current, len(findings), len(toolchains))

	for _, finding := range findings {
		t.Errorf("STALE FIXTURE at %s: %s\n"+
			"-tests copies GOROOT's bytes over this file, so its next run leaves the tree dirty. Commit GOROOT %s's "+
			"bytes (or remove a fixture Go deleted) in the re-bank that moved the pin", want, finding, want)
	}
}

// TestFixtureCurrencyScannerFires is the positive control: the same scanner over synthetic GOROOTs.
func TestFixtureCurrencyScannerFires(t *testing.T) {
	pinned := t.TempDir()
	older := t.TempDir()

	write := func(root, rel, content string) {
		full := filepath.Join(root, "src", filepath.FromSlash(rel))

		if err := os.MkdirAll(filepath.Dir(full), 0o755); err != nil {
			t.Fatal(err)
		}

		if err := os.WriteFile(full, []byte(content), 0o644); err != nil {
			t.Fatal(err)
		}
	}

	write(pinned, "pkg/testdata/current.txt", "same\n")
	write(pinned, "pkg/testdata/hop.golden", "new release\n")
	write(older, "pkg/testdata/hop.golden", "old release\n")
	write(pinned, "pkg/testdata/edited.txt", "upstream\n")

	fixtures := []trackedFixture{
		{"src/core/pkg/testdata/current.txt", gitBlobHash([]byte("same\n"))},
		{"src/core/pkg/testdata/hop.golden", gitBlobHash([]byte("old release\n"))},
		{"src/core/pkg/testdata/edited.txt", gitBlobHash([]byte("hand edit\n"))},
		{"src/core/pkg/testdata/deleted.txt", gitBlobHash([]byte("gone\n"))},
	}

	current, findings := scanFixtureCurrency(fixtures, pinned, map[string]string{"go1.0.0": older})
	got := make([]string, 0, len(findings))

	for _, finding := range findings {
		got = append(got, finding.String())
	}

	want := []string{
		"src/core/pkg/testdata/hop.golden: differs from the pinned GOROOT (bytes == go1.0.0)",
		"src/core/pkg/testdata/edited.txt: differs from the pinned GOROOT (matches no locally cached toolchain)",
		"src/core/pkg/testdata/deleted.txt: absent at the pinned GOROOT (matches no locally cached toolchain)",
	}

	if current != 1 || strings.Join(got, "\n") != strings.Join(want, "\n") {
		t.Fatalf("scanner: current %d, findings:\n%s\nwant current 1, findings:\n%s", current, strings.Join(got, "\n"), strings.Join(want, "\n"))
	}

	// And the blob hash is git's, or the guard would compare nothing real.
	if gitBlobHash([]byte("hello\n")) != "ce013625030ba8dba906f756967f9e9ca394464a" {
		t.Fatalf("gitBlobHash disagrees with git's blob hash of \"hello\\n\"")
	}
}
