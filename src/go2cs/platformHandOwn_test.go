// platformHandOwn_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// Guards for layout L3's hand-owned-file routing (platformHandOwn.go), and one guard that reads the
// REAL corpus rather than a synthetic tree.
//
// The corpus guard is the one that matters. The defect it locks out is not a wrong answer, it is a
// question never asked: a hand-owned file is never emitted, so the emission-based classifier never
// sees it, so it silently keeps whatever placement it already had while the file it supplements moves
// per-GOOS. That is invisible on Windows — the compile item set is identical either way — and it took
// the ENTIRE Linux corpus down at increment 3 (runtime/lock_sema_impl.cs, 8 errors, 249 packages
// skipped behind it). A synthetic test cannot catch the next one, because the next one will be a file
// somebody adds to src/core; only a walk of the corpus can.

package main

import (
	"fmt"
	"os"
	"path/filepath"
	"sort"
	"strings"
	"testing"
)

// TestHandOwnPrincipalsCoverBothShapes pins the two bindings a hand-own can have. Both are needed:
// the `_impl.cs` companion is the shape that broke `runtime`, and the marked whole-file hand-own is
// the shape that would have broken `syscall` two layers further up (dll_windows.cs / exec_windows.cs
// replace Windows-ONLY Go files, so they must not be in a Linux build at all).
func TestHandOwnPrincipalsCoverBothShapes(t *testing.T) {
	tests := []struct {
		logical string
		want    []string
	}{
		// A marked whole-file hand-own: its emission diverts to the `.cs.auto` review sibling, which
		// is therefore emitted by exactly the platforms that compile the Go file it replaces.
		{"syscall/exec_windows.cs", []string{"syscall/exec_windows.cs.auto"}},
		{"runtime/runtime2.cs", []string{"runtime/runtime2.cs.auto"}},
		// An `*_impl.cs` companion has no Go counterpart at all — it supplements `<name>.cs`.
		{"runtime/lock_sema_impl.cs", []string{"runtime/lock_sema_impl.cs.auto", "runtime/lock_sema.cs"}},
		{"os/dir_windows_impl.cs", []string{"os/dir_windows_impl.cs.auto", "os/dir_windows.cs"}},
	}

	for _, test := range tests {
		got := platformHandOwnPrincipals(test.logical)

		if strings.Join(got, ",") != strings.Join(test.want, ",") {
			t.Errorf("platformHandOwnPrincipals(%q) = %v, want %v", test.logical, got, test.want)
		}
	}
}

// TestHandOwnDestinationsSharePlatformNeutralFiles is the reason the rule is stated as a platform SET
// rather than as "the folder the principal is in". `os/proc.cs` and `syscall/syscall.cs` are per-GOOS
// VARIANTS present on all three platforms; inheriting their folder literally would triplicate one
// hand-written file into three copies that must then be hand-maintained in lockstep, for no compile
// benefit at all. L3 duplicates a file only when it cannot be shared.
func TestHandOwnDestinationsSharePlatformNeutralFiles(t *testing.T) {
	coreDir := filepath.Join("root", "core")
	targets := []string{"windows/amd64", "linux/amd64", "darwin/amd64"}

	shared := platformHandOwnDestinations(coreDir, "os", targets, targets)
	wantShared := []string{filepath.Join(coreDir, "os")}

	if strings.Join(shared, "|") != strings.Join(wantShared, "|") {
		t.Errorf("principal on every target: got %v, want %v", shared, wantShared)
	}

	// lock_sema.go is selected on Windows AND macOS; Linux takes lock_futex.go instead. Two copies,
	// and deliberately not a third.
	partial := platformHandOwnDestinations(coreDir, "runtime", targets, []string{"windows/amd64", "darwin/amd64"})
	wantPartial := []string{filepath.Join(coreDir, "runtime", "windows"), filepath.Join(coreDir, "runtime", "darwin")}

	if strings.Join(partial, "|") != strings.Join(wantPartial, "|") {
		t.Errorf("principal on a subset: got %v, want %v", partial, wantPartial)
	}
}

// newHandOwnCorpus builds a two-package corpus in layout L3: `runtime` carries per-GOOS sources with a
// Windows+macOS principal and a flat companion, plus a platform-neutral companion whose principal is
// shared; `fmt` carries no per-GOOS sources at all and must not be touched.
func newHandOwnCorpus(t *testing.T) string {
	t.Helper()

	coreDir := filepath.Join(t.TempDir(), "core")

	writeTestFile(t, filepath.Join(coreDir, "runtime", "runtime.csproj"), "<Project />")
	writeTestFile(t, filepath.Join(coreDir, "runtime", "stubs.cs"), "shared")
	writeTestFile(t, filepath.Join(coreDir, "runtime", "stubs_impl.cs"), "hand-owned, platform neutral")
	writeTestFile(t, filepath.Join(coreDir, "runtime", "lock_sema_impl.cs"), "hand-owned, windows+darwin")
	writeTestFile(t, filepath.Join(coreDir, "runtime", "windows", "lock_sema.cs"), "windows")
	writeTestFile(t, filepath.Join(coreDir, "runtime", "darwin", "lock_sema.cs"), "darwin")
	writeTestFile(t, filepath.Join(coreDir, "runtime", "linux", "lock_futex.cs"), "linux")

	writeTestFile(t, filepath.Join(coreDir, "fmt", "fmt.csproj"), "<Project />")
	writeTestFile(t, filepath.Join(coreDir, "fmt", "print.cs"), "shared")
	writeTestFile(t, filepath.Join(coreDir, "fmt", "print_impl.cs"), "hand-owned")

	return coreDir
}

// newHandOwnEmissions describes what each target emitted over that corpus: the hand-owned files are
// absent from every emission (that is what makes them hand-owned), `lock_sema.cs` comes from Windows
// and macOS only, and everything else is shared.
func newHandOwnEmissions(coreDir string, targets []string) []*platformEmission {
	emitted := map[string][]string{
		"runtime/stubs.cs":      targets,
		"runtime/lock_sema.cs":  {targets[0], targets[2]},
		"runtime/lock_futex.cs": {targets[1]},
		"fmt/print.cs":          targets,
	}

	emissions := make([]*platformEmission, 0, len(targets))

	for _, target := range targets {
		artifacts := map[string]artifactState{}

		// The seeded corpus every staging root is a copy of, none of it emitted.
		for _, seeded := range []string{
			"runtime/runtime.csproj", "runtime/stubs_impl.cs", "runtime/lock_sema_impl.cs",
			"fmt/fmt.csproj", "fmt/print_impl.cs",
		} {
			artifacts[seeded] = artifactState{hash: seeded, logical: seeded}
		}

		for logical, emitters := range emitted {
			for _, emitter := range emitters {
				if emitter != target {
					continue
				}

				artifacts[logical] = artifactState{hash: logical + target, logical: logical, emitted: true}
			}
		}

		emissions = append(emissions, &platformEmission{
			target:    target,
			root:      filepath.Dir(coreDir),
			artifacts: artifacts,
		})
	}

	return emissions
}

// TestMergeRoutesHandOwnsToTheirPrincipalsPlatforms is the whole rule end to end, over a corpus
// shaped like the one that failed: the companion of a Windows+macOS principal lands in BOTH those
// folders and leaves the Linux build, the companion of a shared principal stays flat, and a package
// with no per-GOOS sources is not considered at all.
func TestMergeRoutesHandOwnsToTheirPrincipalsPlatforms(t *testing.T) {
	coreDir := newHandOwnCorpus(t)
	targets := []string{"windows/amd64", "linux/amd64", "darwin/amd64"}
	emissions := newHandOwnEmissions(coreDir, targets)

	packages, written, removed, err := mergeHandOwnedArtifacts(coreDir, targets, emissions)

	if err != nil {
		t.Fatalf("mergeHandOwnedArtifacts failed: %v", err)
	}

	if strings.Join(packages, ",") != "runtime" {
		t.Errorf("touched packages = %v, want [runtime] — `fmt` carries no per-GOOS sources", packages)
	}

	if written != 2 || removed != 1 {
		t.Errorf("written/removed = %d/%d, want 2/1 (two per-GOOS copies, one flat original)", written, removed)
	}

	assertFileExists(t, filepath.Join(coreDir, "runtime", "windows", "lock_sema_impl.cs"), true)
	assertFileExists(t, filepath.Join(coreDir, "runtime", "darwin", "lock_sema_impl.cs"), true)
	assertFileExists(t, filepath.Join(coreDir, "runtime", "linux", "lock_sema_impl.cs"), false)
	assertFileExists(t, filepath.Join(coreDir, "runtime", "lock_sema_impl.cs"), false)

	// The principal is shared, so its companion is too — one file, not three.
	assertFileExists(t, filepath.Join(coreDir, "runtime", "stubs_impl.cs"), true)
	assertFileExists(t, filepath.Join(coreDir, "runtime", "windows", "stubs_impl.cs"), false)

	// A package with no per-GOOS sources is outside the rule entirely.
	assertFileExists(t, filepath.Join(coreDir, "fmt", "print_impl.cs"), true)

	// Idempotent: the second run is seeded from the tree the first produced, so the companion's raw
	// path is now per-GOOS and its logical path folds back to the same key.
	emissions = newHandOwnEmissions(coreDir, targets)

	for _, emission := range emissions {
		delete(emission.artifacts, "runtime/lock_sema_impl.cs")
		emission.artifacts["runtime/windows/lock_sema_impl.cs"] = artifactState{logical: "runtime/lock_sema_impl.cs"}
		emission.artifacts["runtime/darwin/lock_sema_impl.cs"] = artifactState{logical: "runtime/lock_sema_impl.cs"}
	}

	if _, written, removed, err = mergeHandOwnedArtifacts(coreDir, targets, emissions); err != nil {
		t.Fatalf("second mergeHandOwnedArtifacts failed: %v", err)
	}

	if written != 0 || removed != 0 {
		t.Errorf("re-run wrote %d and removed %d; a merge over an already-routed corpus must be a no-op", written, removed)
	}
}

// TestMergeRefusesDivergentHandOwnCopies: a hand-owned file duplicated across per-GOOS folders is
// hand-maintained in each, so a merge that silently propagated one copy over the other is exactly how
// a fix applied to a single flavor would disappear. Refuse instead.
func TestMergeRefusesDivergentHandOwnCopies(t *testing.T) {
	coreDir := newHandOwnCorpus(t)
	targets := []string{"windows/amd64", "linux/amd64", "darwin/amd64"}

	writeTestFile(t, filepath.Join(coreDir, "runtime", "windows", "lock_sema_impl.cs"), "a hand fix applied to one flavor")

	_, _, _, err := mergeHandOwnedArtifacts(coreDir, targets, newHandOwnEmissions(coreDir, targets))

	if err == nil {
		t.Fatal("two disagreeing copies of a hand-owned file merged silently; the merge must refuse")
	}

	if !strings.Contains(err.Error(), "DIFFERENT contents") {
		t.Errorf("error does not name the divergence: %v", err)
	}
}

// TestMergeLeavesPrincipalLessCompanionsWhereTheyAre: a companion with no Go counterpart AND no
// converted principal — internal/poll's runtime_netpoll_impl.cs, which supplies bodies for the FLAT
// fd_poll_runtime.cs's bodyless partials — carries a DIFFERENT implementation per flavor by design (a
// Windows completion poller; a Linux no-poller fallback), so two per-GOOS copies that disagree are not
// a divergence to reconcile. The merge must neither refuse them (the divergence rule above is about
// ONE hand-own duplicated across folders) nor move them: with no emitted principal there is no
// evidence of a platform set, and "no evidence, no rule" is the placement's own contract.
func TestMergeLeavesPrincipalLessCompanionsWhereTheyAre(t *testing.T) {
	coreDir := newHandOwnCorpus(t)
	targets := []string{"windows/amd64", "linux/amd64", "darwin/amd64"}

	writeTestFile(t, filepath.Join(coreDir, "runtime", "windows", "poller_impl.cs"), "a completion poller")
	writeTestFile(t, filepath.Join(coreDir, "runtime", "linux", "poller_impl.cs"), "the no-poller fallback")

	emissions := newHandOwnEmissions(coreDir, targets)

	// Seeded, never emitted, on every target -- exactly how a staging root sees a hand-owned file.
	for _, emission := range emissions {
		for _, rawPath := range []string{"runtime/windows/poller_impl.cs", "runtime/linux/poller_impl.cs"} {
			emission.artifacts[rawPath] = artifactState{hash: rawPath, logical: "runtime/poller_impl.cs"}
		}
	}

	_, written, removed, err := mergeHandOwnedArtifacts(coreDir, targets, emissions)

	if err != nil {
		t.Fatalf("two flavors of a principal-less companion were refused as divergent: %v", err)
	}

	// Only lock_sema_impl.cs's placement moves anything; the companion contributes no write and no removal.
	if written != 2 || removed != 1 {
		t.Errorf("written/removed = %d/%d, want 2/1 (the companion must contribute nothing)", written, removed)
	}

	assertFileExists(t, filepath.Join(coreDir, "runtime", "windows", "poller_impl.cs"), true)
	assertFileExists(t, filepath.Join(coreDir, "runtime", "linux", "poller_impl.cs"), true)
	assertFileExists(t, filepath.Join(coreDir, "runtime", "poller_impl.cs"), false)
	assertFileExists(t, filepath.Join(coreDir, "runtime", "darwin", "poller_impl.cs"), false)
}

// TestCorpusHandOwnsFollowTheirPrincipals walks the REAL src/core and asserts the invariant the merge
// establishes, so a hand-owned file added or moved by hand cannot re-open the seam. Two rules, both
// purely structural:
//
//  1. an `*_impl.cs` whose principal is in SOME but not ALL of its package's per-GOOS folders must be
//     in exactly those folders — this is `runtime/lock_sema_impl.cs`, and it is the whole of the
//     increment-3 Linux failure;
//  2. a `.cs.auto` review sibling lives in the same directory as the hand-owned `.cs` it reviews,
//     because a single-target reconvert writes them to the same folder and a split pair means one of
//     the two is stale in a way no build can report;
//  3. a source whose name carries Go's own GOOS filename constraint (`*_windows.cs`, `*_linux.cs`,
//     `*_darwin.cs`, with an `_impl` companion suffix stripped first) is never flat in an L3 package —
//     it lives in the folder its suffix names. Rule 1 cannot see a MARKED whole-file hand-own, whose
//     principal is its `.cs.auto` rather than another `.cs`, and this is the rule that catches those:
//     `syscall/dll_windows.cs` and `syscall/exec_windows.cs` replace Windows-only Go files and sat
//     flat — latently in every Linux build — through the whole of increment 3, unreached only because
//     the build stopped at `runtime` two layers below.
//
// It skips when src/core is not beside the converter (a bare converter checkout), so it costs a
// non-corpus clone nothing.
func TestCorpusHandOwnsFollowTheirPrincipals(t *testing.T) {
	coreDir := filepath.Join("..", "core")

	if info, err := os.Stat(filepath.Join(coreDir, "golib", "golib.csproj")); err != nil || !info.Mode().IsRegular() {
		t.Skip("src/core is not beside the converter; nothing to walk")
	}

	packages := corpusPlatformPackages(t, coreDir)

	if len(packages) == 0 {
		t.Fatal("no package in src/core carries per-GOOS sources; the corpus is not in layout L3")
	}

	for _, packageDir := range packages {
		goosDirs := corpusPlatformFolders(t, packageDir)
		names := corpusSourceLocations(t, packageDir, goosDirs)

		for name, locations := range names {
			stem := strings.TrimSuffix(name, ".cs")

			if goos, constrained := goosFilenameConstraint(stem); constrained {
				for _, location := range locations {
					if location != goos {
						t.Errorf("%s: %s carries Go's %q filename build constraint but is in [%s]; a GOOS-constrained source "+
							"belongs in that GOOS's folder, never flat (docs/phase4/DESIGN-multiplatform-corpus.md §12, increment 3.5)",
							packageDir, name, goos, orFlat(location))
					}
				}
			}

			if !strings.HasSuffix(stem, "_impl") || strings.HasSuffix(stem, "_test_impl") {
				continue
			}

			principal := strings.TrimSuffix(stem, "_impl") + ".cs"
			principalAt, found := names[principal]

			// A principal present in EVERY folder used to end the question here — "shared either
			// way" — and that skip is the hole this arm closes. Being emitted on every target says
			// the principal is built everywhere; it says nothing about whether the COMPANION applies
			// everywhere, because manualConversionFuncs scopes a registration per GOOS. `runtime`'s
			// trace.go is converted on all three targets while only linux DISPLACES `StartTrace`
			// (registered goosLinux), so runtime/linux/trace_impl.cs is correct and a flat copy is a
			// CS0111 on windows — the shape the three-target merge produced and this guard waved
			// through (found 2026-09-01 by a seeded temp-root build; A/B'd byte-identical at master,
			// so it is the merge's routing rule, not any one arc).
			if found && len(principalAt) == len(goosDirs) && len(goosDirs) > 0 {
				assertScopedHandOwnFollowsItsRegistration(t, coreDir, packageDir, name, locations)
				continue
			}

			// No principal in this package: a go2cs runtime companion with no Go file behind it
			// (runtime/managed_impl.cs, internal/poll/runtime_sema_impl.cs). No evidence, no rule.
			// A flat principal is shared, and shared needs no per-GOOS reasoning.
			if !found || len(principalAt) == 0 || (len(principalAt) == 1 && principalAt[0] == "") {
				continue
			}

			if strings.Join(locations, ",") != strings.Join(principalAt, ",") {
				t.Errorf("%s: %s is in [%s] but its principal %s is in [%s]; a hand-owned companion must take part in "+
					"exactly its principal's platform builds (docs/phase4/DESIGN-multiplatform-corpus.md §12, increment 3.5)",
					packageDir, name, strings.Join(locations, " "), principal, strings.Join(principalAt, " "))
			}
		}
	}

	assertCorpusAutoSiblingsAreColocated(t, coreDir)
}

// corpusPlatformPackages returns every converted package directory that carries per-GOOS sources.
func corpusPlatformPackages(t *testing.T, coreDir string) []string {
	t.Helper()

	var packages []string

	err := filepath.WalkDir(coreDir, func(filePath string, entry os.DirEntry, walkErr error) error {
		if walkErr != nil {
			return walkErr
		}

		if !entry.IsDir() || !packageCarriesPlatformLayout(filePath) {
			return nil
		}

		packages = append(packages, filePath)

		return nil
	})

	if err != nil {
		t.Fatalf("failed to walk %q: %v", coreDir, err)
	}

	sort.Strings(packages)

	return packages
}

// corpusPlatformFolders returns a package's per-GOOS folder names, sorted.
func corpusPlatformFolders(t *testing.T, packageDir string) []string {
	t.Helper()

	entries, err := os.ReadDir(packageDir)

	if err != nil {
		t.Fatalf("failed to read %q: %v", packageDir, err)
	}

	var folders []string

	for _, entry := range entries {
		if entry.IsDir() && isPlatformSourceFolder(packageDir, entry.Name()) {
			folders = append(folders, entry.Name())
		}
	}

	sort.Strings(folders)

	return folders
}

// corpusSourceLocations maps each `.cs` file name in a package to the sorted locations holding it —
// "" for the package directory itself, the folder name for a per-GOOS copy. Converted test artifacts
// are excluded: they are compiled by the colocated `<name>.tests.csproj`, never by the production
// project, so layout L3 does not route them.
func corpusSourceLocations(t *testing.T, packageDir string, goosDirs []string) map[string][]string {
	t.Helper()

	locations := map[string][]string{}

	record := func(dir string, label string) {
		entries, err := os.ReadDir(dir)

		if err != nil {
			t.Fatalf("failed to read %q: %v", dir, err)
		}

		for _, entry := range entries {
			name := entry.Name()

			if entry.IsDir() || !strings.HasSuffix(name, ".cs") || isConvertedTestArtifact(name) {
				continue
			}

			locations[name] = append(locations[name], label)
		}
	}

	record(packageDir, "")

	for _, goos := range goosDirs {
		record(filepath.Join(packageDir, goos), goos)
	}

	for name := range locations {
		sort.Strings(locations[name])
	}

	return locations
}

// goosFilenameConstraint reads Go's own implicit filename build constraint off a converted source's
// stem: `zoneinfo_windows.go` is compiled on Windows and nowhere else, so `zoneinfo_windows.cs` can
// only ever belong to the Windows build. An `_impl` companion suffix is stripped first, since it sits
// outside the Go name (`dir_windows_impl.cs` supplements `dir_windows.cs`).
func goosFilenameConstraint(stem string) (string, bool) {
	stem = strings.TrimSuffix(stem, "_impl")

	if index := strings.LastIndex(stem, "_"); index >= 0 && isKnownGOOS(stem[index+1:]) {
		return stem[index+1:], true
	}

	return "", false
}

// orFlat renders a source location for a message — a per-GOOS folder by name, the package directory
// as "flat".
func orFlat(location string) string {
	if len(location) == 0 {
		return "flat"
	}

	return location
}

// isConvertedTestArtifact reports whether a converted `.cs` belongs to a package's test project
// rather than to the package itself — the same set the emitted .csproj removes from its compile
// items.
func isConvertedTestArtifact(name string) bool {
	return strings.HasSuffix(name, "_test.cs") || name == "package_test_info.cs" || name == "go2cs_test_host.cs"
}

// assertCorpusAutoSiblingsAreColocated checks rule 2: every `<name>.cs.auto` has its hand-owned
// `<name>.cs` in the same directory.
func assertCorpusAutoSiblingsAreColocated(t *testing.T, coreDir string) {
	t.Helper()

	err := filepath.WalkDir(coreDir, func(filePath string, entry os.DirEntry, walkErr error) error {
		if walkErr != nil {
			return walkErr
		}

		if entry.IsDir() || !strings.HasSuffix(entry.Name(), ".cs.auto") {
			return nil
		}

		sibling := strings.TrimSuffix(filePath, ".auto")

		if info, err := os.Stat(sibling); err != nil || !info.Mode().IsRegular() {
			t.Errorf("%s has no hand-owned .cs beside it; a review sibling must follow the file it reviews", filePath)
		}

		return nil
	})

	if err != nil {
		t.Fatalf("failed to walk %q: %v", coreDir, err)
	}
}

// assertFileExists is a presence assertion that names what it expected.
func assertFileExists(t *testing.T, filePath string, want bool) {
	t.Helper()

	_, err := os.Stat(filePath)

	if got := err == nil; got != want {
		t.Errorf("exists(%q) = %v, want %v", filePath, got, want)
	}
}

// assertScopedHandOwnFollowsItsRegistration is the arm for a companion whose PRINCIPAL is emitted on
// every target. The principal's ubiquity settles nothing on its own: `manualConversionFuncs` scopes a
// registration per GOOS, so a shared Go source can keep its generated body on two targets and carry a
// placeholder on the third, and the companion belongs only where the displacement happened.
//
// The expected location set is derived from the REGISTRY — the union of the goosScopes of the
// registered members this hand-own defines. That is deliberately a DIFFERENT derivation from the one
// the merge's routing uses (which reads the displacement placeholders out of each target's emission):
// two independent derivations agreeing is worth more than one shared, and a guard that re-implements
// the fix is only testing that the fix ran.
//
// Silent when the file defines no registered member (an unregistered companion has no scope to
// follow) or when every member is unscoped (goosAny — genuinely shared, and today's flat placement is
// right).
func assertScopedHandOwnFollowsItsRegistration(t *testing.T, coreDir string, packageDir string, name string, locations []string) {
	t.Helper()

	pkg := filepath.ToSlash(strings.TrimPrefix(strings.TrimPrefix(packageDir, coreDir), string(filepath.Separator)))
	funcScopes, registered := manualConversionFuncs[pkg]

	if !registered {
		return
	}

	// THIS file's members, not the package's. handOwnedDefinitionsByFlavor walks every hand-own in the
	// package, and using it here made the first run of this guard answer for the wrong file: with
	// `runtime`'s trace_impl.cs deliberately moved flat, the union over ALL of runtime's hand-owns
	// contained an unscoped member, the helper concluded "shared", and the very file the guard was
	// written for went unreported — while three other packages were flagged on evidence that was not
	// theirs either. Scope the read to the companion under test.
	defined := handOwnedDefinitionsInFile(t, packageDir, name, locations)
	scoped := map[string]bool{}
	sawRegisteredMember := false

	for key, scope := range funcScopes {
		member := key

		if dot := strings.LastIndex(member, "."); dot >= 0 {
			member = member[dot+1:]
		}

		if !defined[member] {
			continue
		}

		sawRegisteredMember = true

		// One unscoped member makes the whole companion shared: it is needed wherever the principal
		// is built, which is everywhere.
		if len(scope) == 0 {
			return
		}

		for _, goos := range scope {
			scoped[goos] = true
		}
	}

	if !sawRegisteredMember || len(scoped) == 0 {
		return
	}

	want := sortedKeys(scoped)
	got := append([]string(nil), locations...)
	sort.Strings(got)

	if strings.Join(got, ",") != strings.Join(want, ",") {
		t.Errorf("%s: %s is in [%s] but the registrations it defines are scoped to [%s]; a companion whose members are "+
			"displaced on only some targets belongs in exactly those GOOS folders — flat compiles it beside the "+
			"undisplaced body (CS0111). Its principal being emitted on every target does not make the companion shared.",
			packageDir, name, orFlat(strings.Join(got, " ")), strings.Join(want, " "))
	}
}

// handOwnedDefinitionsInFile returns the member names ONE hand-owned companion declares, read from
// every location the corpus holds it in (a companion legitimately exists in two GOOS folders — see
// runtime's lock_sema_impl.cs). It reuses the declaration-line and callable-name patterns
// manualConversionDestination_test.go calibrated against this corpus, so the two guards agree on what
// counts as a definition.
func handOwnedDefinitionsInFile(t *testing.T, packageDir string, name string, locations []string) map[string]bool {
	t.Helper()

	defined := map[string]bool{}

	for _, location := range locations {
		dir := packageDir

		if location != "" {
			dir = filepath.Join(packageDir, location)
		}

		contents, err := os.ReadFile(filepath.Join(dir, name))

		if err != nil {
			continue
		}

		text := string(contents)

		for _, line := range csharpDeclarationLine.FindAllString(text, -1) {
			for _, match := range csharpCallableName.FindAllStringSubmatch(line, -1) {
				defined[match[1]] = true
			}
		}
	}

	return defined
}

// TestFuncPlaceholderLeadMatchesTheFormat pins the two placeholder constants to each other. The lead
// is a separate constant rather than a slice of the format because the readers SCAN for it, and a
// derived prefix would silently follow the format if somebody reworded the line — leaving the scans
// matching a string the converter no longer writes, which is a false NEGATIVE in two guards at once
// (the registry's source-side check would report every registration as undisplaced; L3's merge would
// see no displacement anywhere and quietly fall back to the old routing).
func TestFuncPlaceholderLeadMatchesTheFormat(t *testing.T) {
	rendered := fmt.Sprintf(funcPlaceholderFormat, "SomeMember")

	if !strings.HasPrefix(rendered, funcPlaceholderLead) {
		t.Errorf("funcPlaceholderLead %q is not a prefix of the rendered placeholder %q — every scanner of "+
			"this witness would stop matching while the emission kept writing it", funcPlaceholderLead, rendered)
	}

	if !strings.Contains(rendered, "SomeMember") {
		t.Errorf("the placeholder format must carry the member name: %q", rendered)
	}
}

// TestMergeRoutesScopedHandOwnByDisplacement is the routing narrowing, over the shape that broke:
// a principal EMITTED on all three targets whose body is displaced on only one. The old rule read
// the emitter set, called the companion shared, left it flat, and windows compiled it beside its own
// undisplaced body — CS0111 on `runtime.StartTrace`, the whole merged corpus failing to build.
//
// Note what this fixture does that newHandOwnEmissions does not: it writes the principal's emitted
// BYTES into each staging root, because the displacement witness is a line inside them. The synthetic
// emissions elsewhere in this file describe artifacts that never touch disk, which is exactly why the
// narrowing falls back to the emitter set there and those tests keep passing unchanged.
func TestMergeRoutesScopedHandOwnByDisplacement(t *testing.T) {
	coreDir := filepath.Join(t.TempDir(), "core")
	targets := []string{"windows/amd64", "linux/amd64", "darwin/amd64"}

	writeTestFile(t, filepath.Join(coreDir, "runtime", "runtime.csproj"), "<Project />")
	writeTestFile(t, filepath.Join(coreDir, "runtime", "trace_impl.cs"), "hand-owned, linux only")

	for _, goos := range []string{"windows", "linux", "darwin"} {
		writeTestFile(t, filepath.Join(coreDir, "runtime", goos, "trace.cs"), "converted "+goos)
	}

	emissions := make([]*platformEmission, 0, len(targets))

	for _, target := range targets {
		goos := goosOfTarget(target)
		root := filepath.Join(t.TempDir(), goos)

		// Only the LINUX emission displaces the body; the other two carry it.
		body := "public static error StartTrace() { … }"

		if goos == "linux" {
			body = fmt.Sprintf(funcPlaceholderFormat, "StartTrace")
		}

		writeTestFile(t, filepath.Join(root, "core", "runtime", goos, "trace.cs"), body)

		emissions = append(emissions, &platformEmission{
			target: target,
			root:   root,
			artifacts: map[string]artifactState{
				"runtime/" + goos + "/trace.cs": {logical: "runtime/trace.cs", emitted: true},
				"runtime/trace_impl.cs":         {logical: "runtime/trace_impl.cs"},
				"runtime/runtime.csproj":        {logical: "runtime/runtime.csproj"},
			},
		})
	}

	if _, _, _, err := mergeHandOwnedArtifacts(coreDir, targets, emissions); err != nil {
		t.Fatalf("mergeHandOwnedArtifacts failed: %v", err)
	}

	routed := filepath.Join(coreDir, "runtime", "linux", "trace_impl.cs")

	if _, err := os.Stat(routed); err != nil {
		t.Errorf("the companion must land in the folder whose emission DISPLACED its member: %v", err)
	}

	for _, wrong := range []string{
		filepath.Join(coreDir, "runtime", "trace_impl.cs"),
		filepath.Join(coreDir, "runtime", "windows", "trace_impl.cs"),
		filepath.Join(coreDir, "runtime", "darwin", "trace_impl.cs"),
	} {
		if _, err := os.Stat(wrong); err == nil {
			t.Errorf("%s must not exist: windows and darwin keep the generated body, so a copy there is CS0111", wrong)
		}
	}
}
