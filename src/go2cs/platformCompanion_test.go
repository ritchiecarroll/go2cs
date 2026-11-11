// platformCompanion_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards for the companion merge (platformEmit.go's mergeCompanionArtifacts) over REAL staging roots:
// each stage is a whole-tree copy of an L3 seed, snapshotted by snapshotConvertedRoot, so the
// per-GOOS fold and the emitted-by-modification-time reading are the production ones.

package main

import (
	"os"
	"path/filepath"
	"strings"
	"testing"
)

// newCompanionStages builds an L3 seed root and one staging root per target, each a copy of it with
// every file stamped as seeded. A target then "emits" a file by rewriting it with a fresh modification
// time. The shape: a `syscall` package with a hand-owned per-GOOS whole file under `linux/` and its
// `.cs.auto` review sibling beside it; a flat review sibling; and a package whose own NAME is a GOOS
// (`internal/syscall/windows`), which must never read as a per-GOOS folder.
func newCompanionStages(t *testing.T, targets []string, seed map[string]string) (string, []string) {
	t.Helper()

	base := t.TempDir()
	coreDir := filepath.Join(base, "root", "core")
	stages := make([]string, 0, len(targets))

	for relPath, contents := range seed {
		writeTestFile(t, filepath.Join(coreDir, filepath.FromSlash(relPath)), contents)
	}

	for i := range targets {
		stage := filepath.Join(base, "stage", string(rune('a'+i)))
		stages = append(stages, stage)

		for relPath, contents := range seed {
			filePath := filepath.Join(stage, "core", filepath.FromSlash(relPath))
			writeTestFile(t, filePath, contents)

			if err := os.Chtimes(filePath, censusSeedSentinel, censusSeedSentinel); err != nil {
				t.Fatalf("failed to stamp %q: %v", filePath, err)
			}
		}
	}

	return coreDir, stages
}

// snapshotCompanionStages turns the staging roots into the emissions the merge reads.
func snapshotCompanionStages(t *testing.T, targets []string, stages []string) []*platformEmission {
	t.Helper()

	emissions := make([]*platformEmission, 0, len(targets))

	for i, target := range targets {
		artifacts, err := snapshotConvertedRoot(filepath.Join(stages[i], "core"))

		if err != nil {
			t.Fatalf("snapshot of %q failed: %v", stages[i], err)
		}

		emissions = append(emissions, &platformEmission{target: target, goos: goosOfTarget(target), root: stages[i], artifacts: artifacts})
	}

	return emissions
}

func readCompanionFile(t *testing.T, filePath string) string {
	t.Helper()

	contents, err := os.ReadFile(filePath)

	if err != nil {
		t.Fatalf("failed to read %q: %v", filePath, err)
	}

	return string(contents)
}

// TestMergeCarriesPerGOOSReviewSiblingsFromTheirOwnTarget is F-H0-3 (H0, 2026-10-14). A review
// sibling in a per-GOOS folder can be rewritten only by that GOOS's target, while every other stage
// holds its seed copy. The merge took the FIRST target that HAD the file, which is Windows, so the
// root got its own seed bytes back and Linux's rewrite was lost (siginfo_linux.cs.auto,
// exec_unix.cs.auto). The flat sibling and the GOOS-named package are the controls: first target
// still wins for both.
func TestMergeCarriesPerGOOSReviewSiblingsFromTheirOwnTarget(t *testing.T) {
	targets := []string{"windows/amd64", "linux/amd64", "darwin/amd64"}
	seed := map[string]string{
		"syscall/syscall.csproj":                             "<Project />",
		"syscall/linux/exec_unix.cs":                         "hand-owned, linux",
		"syscall/linux/exec_unix.cs.auto":                    "seed sibling",
		"syscall/syscall.cs.auto":                            "seed flat sibling",
		"internal/syscall/windows/windows.csproj":            "<Project />",
		"internal/syscall/windows/exec_windows_test.cs.auto": "seed named-package sibling",
	}

	coreDir, stages := newCompanionStages(t, targets, seed)

	// Linux rewrites its own per-GOOS sibling; every target rewrites the flat one, each differently;
	// Windows and Linux both rewrite the GOOS-named package's sibling.
	writeTestFile(t, filepath.Join(stages[1], "core", "syscall", "linux", "exec_unix.cs.auto"), "linux sibling")

	for i, target := range targets {
		writeTestFile(t, filepath.Join(stages[i], "core", "syscall", "syscall.cs.auto"), "flat sibling from "+target)
	}

	writeTestFile(t, filepath.Join(stages[0], "core", "internal", "syscall", "windows", "exec_windows_test.cs.auto"), "named-package sibling from windows")
	writeTestFile(t, filepath.Join(stages[1], "core", "internal", "syscall", "windows", "exec_windows_test.cs.auto"), "named-package sibling from linux")

	emissions := snapshotCompanionStages(t, targets, stages)

	if _, _, _, err := mergeCompanionArtifacts(coreDir, targets, emissions); err != nil {
		t.Fatalf("mergeCompanionArtifacts failed: %v", err)
	}

	if got := readCompanionFile(t, filepath.Join(coreDir, "syscall", "linux", "exec_unix.cs.auto")); got != "linux sibling" {
		t.Errorf("root syscall/linux/exec_unix.cs.auto = %q, want %q: the per-GOOS sibling must come from its own target's stage", got, "linux sibling")
	}

	// Controls: the flat rule is unchanged, so the head of -platforms wins in both places.
	if got := readCompanionFile(t, filepath.Join(coreDir, "syscall", "syscall.cs.auto")); got != "flat sibling from windows/amd64" {
		t.Errorf("root syscall/syscall.cs.auto = %q, want the first target's bytes", got)
	}

	if got := readCompanionFile(t, filepath.Join(coreDir, "internal", "syscall", "windows", "exec_windows_test.cs.auto")); got != "named-package sibling from windows" {
		t.Errorf("root internal/syscall/windows/exec_windows_test.cs.auto = %q, want the first target's bytes: a package NAMED windows is not a per-GOOS folder", got)
	}

	// The hand-owned file itself is not a companion and is never written by this step.
	if got := readCompanionFile(t, filepath.Join(coreDir, "syscall", "linux", "exec_unix.cs")); got != "hand-owned, linux" {
		t.Errorf("root syscall/linux/exec_unix.cs = %q, want it untouched", got)
	}
}

// TestMergeTakesCompanionsFromATargetThatConvertedThePackage is the companion half of F-H0-3 (H5,
// 2026-10-15). crypto/x509/internal/macos is darwin-only: the Windows and Linux stages never convert
// it and hold only the seed's copy of its README, yet Windows, first in -platforms order, "had" it,
// so the root kept the seeded `macOS_package` while the darwin stage emitted `macos_package`. The
// merge now takes a companion from the first target that CONVERTED the package (emitted a .cs in it).
//
// os/exec/internal/fdtest is the control, and the reason the rule reads conversion rather than the
// companion's own emission: its README varies by platform, Windows converts the package but rewrites
// the README to the bytes it already holds (so its modification time never moves), and Linux emits
// its own flavor. The Windows flavor must still win.
func TestMergeTakesCompanionsFromATargetThatConvertedThePackage(t *testing.T) {
	targets := []string{"windows/amd64", "linux/amd64", "darwin/amd64"}
	seed := map[string]string{
		"crypto/x509/internal/macos/macos.csproj": "<Project />",
		"crypto/x509/internal/macos/macos.cs":     "seed macos source",
		"crypto/x509/internal/macos/README.md":    "macOS_package",
		"os/exec/internal/fdtest/fdtest.csproj":   "<Project />",
		"os/exec/internal/fdtest/exist.cs":        "seed fdtest source",
		"os/exec/internal/fdtest/README.md":       "fdtest README, windows flavor",
	}

	coreDir, stages := newCompanionStages(t, targets, seed)

	// darwin alone converts macos, and its README changed.
	writeTestFile(t, filepath.Join(stages[2], "core", "crypto", "x509", "internal", "macos", "macos.cs"), "darwin macos source")
	writeTestFile(t, filepath.Join(stages[2], "core", "crypto", "x509", "internal", "macos", "README.md"), "macos_package")

	// Every target converts fdtest; only Linux's README differs from the seed, so only Linux's moves.
	for i := range targets {
		writeTestFile(t, filepath.Join(stages[i], "core", "os", "exec", "internal", "fdtest", "exist.cs"), "fdtest source")
	}

	writeTestFile(t, filepath.Join(stages[1], "core", "os", "exec", "internal", "fdtest", "README.md"), "fdtest README, linux flavor")

	emissions := snapshotCompanionStages(t, targets, stages)

	if _, _, _, err := mergeCompanionArtifacts(coreDir, targets, emissions); err != nil {
		t.Fatalf("mergeCompanionArtifacts failed: %v", err)
	}

	if got := readCompanionFile(t, filepath.Join(coreDir, "crypto", "x509", "internal", "macos", "README.md")); got != "macos_package" {
		t.Errorf("root macos README.md = %q, want %q: a single-target package's companion must come from the target that converted it, not a stage holding the seed", got, "macos_package")
	}

	if got := readCompanionFile(t, filepath.Join(coreDir, "os", "exec", "internal", "fdtest", "README.md")); got != "fdtest README, windows flavor" {
		t.Errorf("root fdtest README.md = %q, want the Windows flavor: the first target that converted the package wins even when its rewrite left the bytes unchanged", got)
	}
}

// runtimeSiblingSeed is runtime's shape for the review-sibling split: per-GOOS source folders (an L3 package), a FLAT
// whole-file hand-own, runtime2.cs, and its review sibling wherever `siblings` puts it.
func runtimeSiblingSeed(siblings map[string]string) map[string]string {
	seed := map[string]string{
		"runtime/runtime.csproj":        "<Project />",
		"runtime/runtime2.cs":           "hand-owned, flat",
		"runtime/windows/os_windows.cs": "seed windows source",
		"runtime/linux/os_linux.cs":     "seed linux source",
		"runtime/darwin/os_darwin.cs":   "seed darwin source",
	}

	for relPath, contents := range siblings {
		seed[relPath] = contents
	}

	return seed
}

// convertRuntimeIn has every target convert runtime: each rewrites its own per-GOOS source, so the merge reads the
// package as converted there, and writes its flavour of the review sibling at relPath(goos).
func convertRuntimeIn(t *testing.T, targets []string, stages []string, relPath func(goos string) string, flavour func(goos string) string) {
	t.Helper()

	for i, target := range targets {
		goos := goosOfTarget(target)
		writeTestFile(t, filepath.Join(stages[i], "core", "runtime", goos, "os_"+goos+".cs"), goos+" source")
		writeTestFile(t, filepath.Join(stages[i], "core", filepath.FromSlash(relPath(goos))), flavour(goos))
	}
}

func companionFileExists(t *testing.T, filePath string) bool {
	t.Helper()

	_, err := os.Stat(filePath)

	if err != nil && !os.IsNotExist(err) {
		t.Fatalf("failed to stat %q: %v", filePath, err)
	}

	return err == nil
}

// A review sibling exists to show a whole-file hand-own's drift from what the converter emits, and a FLAT hand-own's
// emission can differ by GOOS: runtime/runtime2.go's m carries "sigmask" in its [GoValueClone] list on linux only,
// and g is `public partial struct g` there (R's end-regen F2). One flat sibling kept windows' flavour and hid the
// others (COORD's S5 ruling (a)). When the converting targets' siblings DIFFER, the merge keeps one per GOOS folder
// and drops the flat one; when they AGREE it keeps one flat sibling and drops the folders' copies.
func TestMergeSplitsAFlatHandOwnsReviewSiblingWhenItsFlavoursDiffer(t *testing.T) {
	targets := []string{"windows/amd64", "linux/amd64", "darwin/amd64"}
	flat := func(string) string { return "runtime/runtime2.cs.auto" }
	perGOOS := func(goos string) string { return "runtime/" + goos + "/runtime2.cs.auto" }

	// (i) The committed shape: one flat sibling. Each target emits it flat, each in its own flavour.
	coreDir, stages := newCompanionStages(t, targets, runtimeSiblingSeed(map[string]string{"runtime/runtime2.cs.auto": "seed flat sibling"}))
	convertRuntimeIn(t, targets, stages, flat, func(goos string) string { return goos + " flavour" })

	if _, _, _, err := mergeCompanionArtifacts(coreDir, targets, snapshotCompanionStages(t, targets, stages)); err != nil {
		t.Fatalf("(i) mergeCompanionArtifacts failed: %v", err)
	}

	for _, target := range targets {
		goos := goosOfTarget(target)

		if got := readCompanionFile(t, filepath.Join(coreDir, "runtime", goos, "runtime2.cs.auto")); got != goos+" flavour" {
			t.Errorf("(i) root runtime/%s/runtime2.cs.auto = %q, want %q", goos, got, goos+" flavour")
		}
	}

	if companionFileExists(t, filepath.Join(coreDir, "runtime", "runtime2.cs.auto")) {
		t.Error("(i) the flat sibling survived a split: the root holds both shapes")
	}

	// (ii) The split shape, after the flavours converge: each target writes its own folder's sibling (the single-target
	// routing, reviewSiblingLayoutPath), all with the same bytes. One flat sibling again, and no folder copies.
	coreDir, stages = newCompanionStages(t, targets, runtimeSiblingSeed(map[string]string{
		"runtime/windows/runtime2.cs.auto": "seed windows",
		"runtime/linux/runtime2.cs.auto":   "seed linux",
		"runtime/darwin/runtime2.cs.auto":  "seed darwin",
	}))
	convertRuntimeIn(t, targets, stages, perGOOS, func(string) string { return "one flavour" })

	if _, _, _, err := mergeCompanionArtifacts(coreDir, targets, snapshotCompanionStages(t, targets, stages)); err != nil {
		t.Fatalf("(ii) mergeCompanionArtifacts failed: %v", err)
	}

	if got := readCompanionFile(t, filepath.Join(coreDir, "runtime", "runtime2.cs.auto")); got != "one flavour" {
		t.Errorf("(ii) root runtime/runtime2.cs.auto = %q, want the agreed flavour flat", got)
	}

	for _, target := range targets {
		if goos := goosOfTarget(target); companionFileExists(t, filepath.Join(coreDir, "runtime", goos, "runtime2.cs.auto")) {
			t.Errorf("(ii) runtime/%s/runtime2.cs.auto survived flavours that agree", goos)
		}
	}
}

// subtleSiblingSeed is crypto/internal/fips140/subtle's shape for the review-sibling split: a package with NO key
// source folder, a FLAT whole-file hand-own, xor_generic.cs, and its review sibling wherever `siblings` puts it.
func subtleSiblingSeed(siblings map[string]string) map[string]string {
	seed := map[string]string{
		"subtle/subtle.csproj":  "<Project />",
		"subtle/xor.cs":         "seed source",
		"subtle/xor_generic.cs": "hand-owned, flat",
	}

	for relPath, contents := range siblings {
		seed[relPath] = contents
	}

	return seed
}

// convertSubtleIn has every target convert subtle: each rewrites the shared source, so the merge reads the package as
// converted there, and writes its flavour of the review sibling at relPath(target).
func convertSubtleIn(t *testing.T, targets []string, stages []string, relPath func(target string) string, flavour func(target string) string) {
	t.Helper()

	for i, target := range targets {
		writeTestFile(t, filepath.Join(stages[i], "core", "subtle", "xor.cs"), "source")
		writeTestFile(t, filepath.Join(stages[i], "core", filepath.FromSlash(relPath(target))), flavour(target))
	}
}

// A review sibling is never compiled, so the key folder it is split into needs no source of its own.
// crypto/internal/fips140/subtle's xor_generic.cs is a flat hand-own whose emission differs by GOARCH in one line, and
// the package holds no key source folder. The merge used to split only into SOURCE folders, so it kept the first
// target's flavour flat and named the package, and a linux/arm64 reconvert then wrote its own flavour over the flat
// sibling (the linux/arm64 flavour's single-target residual). Each class of identical flavours now goes to the key the
// lattice homes it at, in a folder that holds the sibling alone; the split shape is stable under a second merge; and
// flavours that converge fold back to one flat sibling, leaving no empty folder behind.
func TestMergeSplitsAReviewSiblingIntoKeyFoldersThatHoldNoSource(t *testing.T) {
	targets := []string{"windows/amd64", "linux/amd64", "darwin/amd64", "linux/arm64"}
	flat := func(string) string { return "subtle/xor_generic.cs.auto" }
	byArch := func(target string) string { return archOfTarget(target) + " flavour" }

	// Where the single-target routing writes each target's flavour once the tree holds the split shape.
	keyed := func(target string) string {
		if target == "linux/arm64" {
			return "subtle/linux-arm64/xor_generic.cs.auto"
		}

		return "subtle/amd64/xor_generic.cs.auto"
	}

	splitSeed := map[string]string{
		"subtle/amd64/xor_generic.cs.auto":       "amd64 flavour",
		"subtle/linux-arm64/xor_generic.cs.auto": "arm64 flavour",
	}

	assertSplit := func(arm string, coreDir string, residual map[string]string) {
		t.Helper()

		for relPath, want := range splitSeed {
			if got := readCompanionFile(t, filepath.Join(coreDir, filepath.FromSlash(relPath))); got != want {
				t.Errorf("%s root %s = %q, want %q", arm, relPath, got, want)
			}
		}

		if companionFileExists(t, filepath.Join(coreDir, "subtle", "xor_generic.cs.auto")) {
			t.Errorf("%s the flat sibling survived a split: the root holds both shapes", arm)
		}

		if note, named := residual["subtle"]; named {
			t.Errorf("%s the package was named for a difference the merge can now express: %q", arm, note)
		}
	}

	// (i) The committed shape: one flat sibling. Each target emits it flat, in its GOARCH's flavour.
	coreDir, stages := newCompanionStages(t, targets, subtleSiblingSeed(map[string]string{"subtle/xor_generic.cs.auto": "seed flat sibling"}))
	convertSubtleIn(t, targets, stages, flat, byArch)

	_, residual, _, err := mergeCompanionArtifacts(coreDir, targets, snapshotCompanionStages(t, targets, stages))

	if err != nil {
		t.Fatalf("(i) mergeCompanionArtifacts failed: %v", err)
	}

	assertSplit("(i)", coreDir, residual)

	// (ii) The split shape, flavours unchanged: each target writes its own key folder's sibling. Nothing moves.
	coreDir, stages = newCompanionStages(t, targets, subtleSiblingSeed(splitSeed))
	convertSubtleIn(t, targets, stages, keyed, byArch)

	if _, residual, _, err = mergeCompanionArtifacts(coreDir, targets, snapshotCompanionStages(t, targets, stages)); err != nil {
		t.Fatalf("(ii) mergeCompanionArtifacts failed: %v", err)
	}

	assertSplit("(ii)", coreDir, residual)

	// (iii) The split shape, after the flavours converge: one flat sibling again, and the folders that held nothing
	// else are gone with their copies.
	coreDir, stages = newCompanionStages(t, targets, subtleSiblingSeed(splitSeed))
	convertSubtleIn(t, targets, stages, keyed, func(string) string { return "one flavour" })

	if _, _, _, err = mergeCompanionArtifacts(coreDir, targets, snapshotCompanionStages(t, targets, stages)); err != nil {
		t.Fatalf("(iii) mergeCompanionArtifacts failed: %v", err)
	}

	if got := readCompanionFile(t, filepath.Join(coreDir, "subtle", "xor_generic.cs.auto")); got != "one flavour" {
		t.Errorf("(iii) root subtle/xor_generic.cs.auto = %q, want the agreed flavour flat", got)
	}

	for _, folder := range []string{"amd64", "linux-arm64"} {
		if _, err := os.Stat(filepath.Join(coreDir, "subtle", folder)); !os.IsNotExist(err) {
			t.Errorf("(iii) subtle/%s survived flavours that agree (stat: %v)", folder, err)
		}
	}
}

// The one difference the merge still cannot express: a flavour's key folder is a NESTED PACKAGE of the same name
// (`internal/syscall/windows`), whose directory is another package's and is never written into. There the merge keeps
// the first converting target's flavour flat and NAMES the package rather than hiding the difference; no other key
// folder gains a copy, so the root never holds both shapes.
func TestMergeNamesADifferingReviewSiblingItCannotSplit(t *testing.T) {
	targets := []string{"windows/amd64", "linux/amd64", "darwin/amd64"}
	seed := map[string]string{
		"internal/syscall/syscall.csproj":         "<Project />",
		"internal/syscall/sleep.cs":               "seed source",
		"internal/syscall/tick.cs":                "hand-owned, flat",
		"internal/syscall/tick.cs.auto":           "seed sibling",
		"internal/syscall/windows/windows.csproj": "<Project />",
		"internal/syscall/windows/zsyscall.cs":    "seed nested source",
	}

	coreDir, stages := newCompanionStages(t, targets, seed)

	for i, target := range targets {
		writeTestFile(t, filepath.Join(stages[i], "core", "internal", "syscall", "sleep.cs"), "source")
		writeTestFile(t, filepath.Join(stages[i], "core", "internal", "syscall", "tick.cs.auto"), goosOfTarget(target)+" flavour")
	}

	_, residual, _, err := mergeCompanionArtifacts(coreDir, targets, snapshotCompanionStages(t, targets, stages))

	if err != nil {
		t.Fatalf("mergeCompanionArtifacts failed: %v", err)
	}

	packageDir := filepath.Join(coreDir, "internal", "syscall")

	if got := readCompanionFile(t, filepath.Join(packageDir, "tick.cs.auto")); got != "windows flavour" {
		t.Errorf("root internal/syscall/tick.cs.auto = %q, want the first converting target's flavour", got)
	}

	for _, goos := range []string{"windows", "linux", "darwin"} {
		if companionFileExists(t, filepath.Join(packageDir, goos, "tick.cs.auto")) {
			t.Errorf("internal/syscall/%s/tick.cs.auto was written beside a sibling kept flat", goos)
		}
	}

	if note := residual["internal/syscall"]; !strings.Contains(note, "tick.cs.auto") {
		t.Errorf("the unsplittable difference was not named: residual[internal/syscall] = %q", note)
	}
}
