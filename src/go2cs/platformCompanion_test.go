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
