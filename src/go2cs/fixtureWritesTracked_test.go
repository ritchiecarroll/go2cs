// fixtureWritesTracked_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"bufio"
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

// Every file -tests stages into src/core (copyTestFixtures: the package's top-level .go sources,
// its testdata tree, its nested packages' testdata and the fixtures its tests read from above it)
// must be TRACKED or IGNORED. One that is neither is left behind as an untracked file by every run
// of its package, so a sweep's tree reads dirty and a lane must decide by hand whether to commit
// it. At master 172d437e66 there were ten (the i9's and the i7's shards, 2026-10-01): seven
// fixtures new in Go 1.24 that the hop's re-banks staged but never committed, and three go/build
// fixtures that were never committed at all.
//
// THE RULE the tree already follows, measured over the same write set: a staged .go file is
// ignored (src/core/.gitignore's *.go -- this is a C# tree, and a .go file here is a pipeline
// input), and every other fixture is committed, where the fixture-currency guard
// (internal/repoguard/fixturesCurrent_test.go) holds its bytes to the pinned GOROOT. So a new
// non-.go fixture is committed with the re-bank that first stages it, never ignored.
//
// WHICH PACKAGES: every GOROOT package that has tests and a directory in src/core holding tracked
// files -- the packages a -tests run can target, a superset of the roster, so a row's first run
// cannot introduce a write this guard never saw. The write set is computed by the converter's
// OWN testFixturePaths and linkStagedFixtureDirs, never by a second derivation, and from the
// pinned GOROOT (src/version.props), since the fixture set is a fact about one Go release.
//
// It reads the git INDEX (ls-files) and the ignore rules (check-ignore --no-index), never the
// working copy, so a tree a -tests run has already dirtied still reads its committed state. Run it
// with -count=1: Go's test cache cannot see an index change.
//
// POSITIVE CONTROL: TestFixtureWriteScannerFires drives the same enumeration and classification
// over a synthetic GOROOT and a synthetic repository holding one tracked fixture, one ignored .go
// copy and one fixture that is neither. Only the last may be reported.

// fixtureWrite is one file copyTestFixtures stages: the package that stages it and its repository path.
type fixtureWrite struct {
	pkg  string
	path string // slash-separated, relative to the repository root ("src/core/<pkg>/testdata/x")
}

// stagedFixtureWrites lists every file a -tests run of any package under goRoot/src with tests and
// a tracked directory under <repo>/src/core would stage, through the converter's own functions.
func stagedFixtureWrites(t *testing.T, goRoot string, trackedDirs map[string]bool) (writes []fixtureWrite, packages int) {
	t.Helper()

	src := filepath.Join(goRoot, "src")

	err := filepath.WalkDir(src, func(path string, entry os.DirEntry, walkErr error) error {
		if walkErr != nil {
			return walkErr
		}

		if !entry.IsDir() {
			return nil
		}

		relative, err := filepath.Rel(src, path)

		if err != nil {
			return err
		}

		relative = filepath.ToSlash(relative)

		if relative == "." {
			return nil
		}

		// go/build's ignored-directory convention, and no package lives inside a testdata tree.
		if name := entry.Name(); strings.HasPrefix(name, ".") || strings.HasPrefix(name, "_") || hasPathSegment(relative, "testdata") {
			return filepath.SkipDir
		}

		if !trackedDirs["src/core/"+relative] {
			return nil
		}

		if tests, _ := filepath.Glob(filepath.Join(path, "*_test.go")); len(tests) == 0 {
			return nil
		}

		fixtures, err := testFixturePaths(path)

		if err != nil {
			return err
		}

		linkStaged, err := linkStagedFixtureDirs(path)

		if err != nil {
			return err
		}

		packages++

		for _, fixture := range fixtures {
			if isUnderLinkStagedDir(fixture, linkStaged) {
				continue
			}

			// copyTestFixtures writes filepath.Join(outputPath, fixture), which cleans a "../" prefix
			// into the mirrored ancestor directory, so the same join names the file here.
			target := filepath.ToSlash(filepath.Join("src/core", relative, filepath.FromSlash(fixture)))
			writes = append(writes, fixtureWrite{pkg: relative, path: target})
		}

		return nil
	})

	if err != nil {
		t.Fatalf("enumerating staged fixtures under %s: %v", src, err)
	}

	return writes, packages
}

// untrackedUnignoredWrites returns the writes that root's git index does not track and its ignore
// rules do not ignore, sorted and distinct, plus the tracked and ignored counts.
func untrackedUnignoredWrites(t *testing.T, root string, writes []fixtureWrite) (findings []fixtureWrite, tracked, ignored int) {
	t.Helper()

	trackedSet := map[string]bool{}

	for _, path := range gitIndexFiles(t, root, "src/core") {
		trackedSet[path] = true
	}

	// One file can be staged by several packages: a parent stages its nested packages' testdata
	// (nestedFixturePaths), so crypto stages crypto/internal/fips140/rsa/testdata too. A finding
	// names every stager, the file's own package among them.
	stagers := map[string][]string{}
	var distinct []fixtureWrite

	for _, write := range writes {
		if stagers[write.path] == nil {
			distinct = append(distinct, write)
		}

		stagers[write.path] = append(stagers[write.path], write.pkg)
	}

	var candidates []fixtureWrite

	for _, write := range distinct {
		write.pkg = strings.Join(stagers[write.path], ", ")

		if trackedSet[write.path] {
			tracked++
			continue
		}

		candidates = append(candidates, write)
	}

	ignoredSet := map[string]bool{}

	if len(candidates) > 0 {
		var input strings.Builder

		for _, candidate := range candidates {
			input.WriteString(candidate.path + "\n")
		}

		cmd := exec.Command("git", "-C", root, "check-ignore", "--no-index", "--stdin")
		cmd.Stdin = strings.NewReader(input.String())
		out, err := cmd.Output()

		// check-ignore exits 1 when nothing is ignored, which is an answer, not a failure.
		if exitErr, isExit := err.(*exec.ExitError); err != nil && !(isExit && exitErr.ExitCode() == 1) {
			t.Fatalf("git check-ignore failed in %s: %v", root, err)
		}

		scanner := bufio.NewScanner(strings.NewReader(string(out)))

		for scanner.Scan() {
			ignoredSet[scanner.Text()] = true
		}
	}

	for _, candidate := range candidates {
		if ignoredSet[candidate.path] {
			ignored++
			continue
		}

		findings = append(findings, candidate)
	}

	sort.Slice(findings, func(i, j int) bool { return findings[i].path < findings[j].path })

	return findings, tracked, ignored
}

// gitIndexFiles is `git ls-files` under prefix, slash paths relative to root.
func gitIndexFiles(t *testing.T, root string, prefix string) []string {
	t.Helper()

	out, err := exec.Command("git", "-C", root, "ls-files", "-z", "--", prefix).Output()

	if err != nil {
		t.Fatalf("git ls-files failed in %s: %v", root, err)
	}

	var files []string

	for _, path := range strings.Split(string(out), "\x00") {
		if path != "" {
			files = append(files, path)
		}
	}

	return files
}

// directoriesOf maps each tracked file's directory, and every ancestor of it, to true.
func directoriesOf(files []string) map[string]bool {
	dirs := map[string]bool{}

	for _, file := range files {
		for dir := filepath.ToSlash(filepath.Dir(file)); dir != "." && !dirs[dir]; dir = filepath.ToSlash(filepath.Dir(dir)) {
			dirs[dir] = true
		}
	}

	return dirs
}

// pinnedGoRootForFixtures is the GOROOT of the release src/version.props pins: the running
// toolchain's when its VERSION is the pin, else the pinned toolchain in the module cache. Neither
// found FAILS rather than skips: the write set is a fact about one release, and a skip measures nothing.
func pinnedGoRootForFixtures(t *testing.T, root string) string {
	t.Helper()

	props, err := os.ReadFile(filepath.Join(root, "src", "version.props"))

	if err != nil {
		t.Fatalf("cannot read src/version.props: %v", err)
	}

	pin := releasestamp.StdLibVersion(string(props))

	if pin == "" {
		t.Fatalf("src/version.props has no <GoStdLibVersion>")
	}

	want := "go" + pin
	candidates := []string{build.Default.GOROOT}

	if modCache := os.Getenv("GOMODCACHE"); modCache != "" {
		candidates = append(candidates, filepath.Join(modCache, "golang.org", "toolchain@v0.0.1-"+want+"."+runtime.GOOS+"-"+runtime.GOARCH))
	}

	gopath, _, _ := strings.Cut(build.Default.GOPATH, string(os.PathListSeparator))
	candidates = append(candidates, filepath.Join(gopath, "pkg", "mod", "golang.org", "toolchain@v0.0.1-"+want+"."+runtime.GOOS+"-"+runtime.GOARCH))

	for _, candidate := range candidates {
		if version, err := os.ReadFile(filepath.Join(candidate, "VERSION")); err == nil {
			if first, _, _ := strings.Cut(string(version), "\n"); strings.TrimSpace(first) == want {
				return candidate
			}
		}
	}

	t.Fatalf("no GOROOT at the pinned %s (running %s, GOROOT %s, and none in the module cache for %s/%s); run with GOTOOLCHAIN=%s",
		want, runtime.Version(), build.Default.GOROOT, runtime.GOOS, runtime.GOARCH, want)

	return ""
}

// TestEveryStagedFixtureIsTrackedOrIgnored is the guard.
func TestEveryStagedFixtureIsTrackedOrIgnored(t *testing.T) {
	root := repoRootFromPackageDir(t)
	goRoot := pinnedGoRootForFixtures(t, root)
	writes, packages := stagedFixtureWrites(t, goRoot, directoriesOf(gitIndexFiles(t, root, "src/core")))
	findings, tracked, ignored := untrackedUnignoredWrites(t, root, writes)

	if packages < 200 || len(writes) < 5000 {
		t.Fatalf("VACUOUS: %d packages staging %d files; the scan is measuring nothing", packages, len(writes))
	}

	t.Logf("GOROOT %s · packages %d · staged files %d · tracked %d · ignored %d · neither %d",
		goRoot, packages, len(writes), tracked, ignored, len(findings))

	for _, finding := range findings {
		t.Errorf("STAGED FIXTURE NEITHER TRACKED NOR IGNORED: %s (staged by %s)\n"+
			"every -tests run of that package leaves it behind untracked. A non-.go fixture is committed (its bytes "+
			"from the pinned GOROOT, which the fixture-currency guard then holds); a staged .go file is already ignored",
			finding.path, finding.pkg)
	}
}

// TestFixtureWriteScannerFires is the positive control: the same enumeration and classification
// over a synthetic GOROOT and repository.
func TestFixtureWriteScannerFires(t *testing.T) {
	goRoot := t.TempDir()
	repo := t.TempDir()

	write := func(base, rel, content string) {
		full := filepath.Join(base, filepath.FromSlash(rel))

		if err := os.MkdirAll(filepath.Dir(full), 0o755); err != nil {
			t.Fatal(err)
		}

		if err := os.WriteFile(full, []byte(content), 0o644); err != nil {
			t.Fatal(err)
		}
	}

	git := func(args ...string) {
		if out, err := exec.Command("git", append([]string{"-C", repo}, args...)...).CombinedOutput(); err != nil {
			t.Fatalf("git %v: %v\n%s", args, err, out)
		}
	}

	write(goRoot, "src/pkg/pkg.go", "package pkg\n")
	write(goRoot, "src/pkg/pkg_test.go", "package pkg\n")
	write(goRoot, "src/pkg/testdata/kept.txt", "kept\n")
	write(goRoot, "src/pkg/testdata/missing.bin", "missing\n")
	write(goRoot, "src/notconverted/notconverted_test.go", "package notconverted\n")
	write(goRoot, "src/notconverted/testdata/never.txt", "never\n")

	write(repo, "src/core/.gitignore", "*.go\n")
	write(repo, "src/core/pkg/pkg.cs", "// converted\n")
	write(repo, "src/core/pkg/testdata/kept.txt", "kept\n")
	git("init", "-q")
	git("add", "src/core/.gitignore", "src/core/pkg/pkg.cs", "src/core/pkg/testdata/kept.txt")

	writes, packages := stagedFixtureWrites(t, goRoot, directoriesOf(gitIndexFiles(t, repo, "src/core")))
	findings, tracked, ignored := untrackedUnignoredWrites(t, repo, writes)

	var got []string

	for _, finding := range findings {
		got = append(got, finding.pkg+" "+finding.path)
	}

	// pkg stages pkg.go, pkg_test.go (both ignored), kept.txt (tracked) and missing.bin (neither);
	// notconverted has no src/core directory and stages nothing.
	if packages != 1 || len(writes) != 4 || tracked != 1 || ignored != 2 ||
		strings.Join(got, "\n") != "pkg src/core/pkg/testdata/missing.bin" {
		t.Fatalf("scanner: packages %d, writes %d, tracked %d, ignored %d, findings %q; want 1, 4, 1, 2, [pkg src/core/pkg/testdata/missing.bin]",
			packages, len(writes), tracked, ignored, got)
	}
}
