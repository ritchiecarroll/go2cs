// platformProjectConverted_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"path/filepath"
	"strings"
	"testing"
)

// templateLineMarker stands for a csproj template line the converter added since the seed was emitted (the
// interceptor opt-in, at the hop's fixed-point regen).
const templateLineMarker = "<TemplateLineMarker>true</TemplateLineMarker>"

// withTemplateLine renders a project file as a converter newer than the seed writes it.
func withTemplateLine(t *testing.T, contents string) string {
	t.Helper()

	const anchor = "<AllowUnsafeBlocks>false</AllowUnsafeBlocks>"
	updated := strings.Replace(contents, anchor, anchor+"\r\n    "+templateLineMarker, 1)

	if updated == contents {
		t.Fatalf("test setup: %q was not found in the rendered template", anchor)
	}

	return updated
}

// TestMergeTakesProjectFilesFromTargetsThatConvertedThePackage is the project-file half of F-H0-3, found at
// the hop's fixed-point regen (2026-10-23). A darwin-only package's .csproj changes when the converter's
// template does, but only the darwin stage converts the package; the Windows and Linux stages hold the seed's
// copy, because every stage is a whole-tree copy of the seed. The merge took its base from the FIRST target that
// HAD the file, Windows, so the root kept the seeded template (crypto/x509/internal/macos, internal/routebsd,
// internal/runtime/syscall/linux and internal/cgrouptest missed the interceptor opt-in), and it read the seed
// copies' reference lists as those targets' import sets, so a reference only the converting target emits came
// out conditioned on a GOOS that never builds the package (internal/syscall/windows kept such a split).
//
// The project merge now reads only the stages that converted the package, as the companion merge does. The
// Windows-only package is the control the old order got right by accident (Windows is first): its template line
// still lands, and its seed-held Linux and darwin lists no longer split its references either.
func TestMergeTakesProjectFilesFromTargetsThatConvertedThePackage(t *testing.T) {
	targets := []string{"windows/amd64", "linux/amd64", "darwin/amd64"}

	const (
		macosProject   = "crypto/x509/internal/macos/crypto.x509.internal.macos.csproj"
		macosSource    = "crypto/x509/internal/macos/corefoundation.cs"
		windowsProject = "internal/syscall/windows/internal.syscall.windows.csproj"
		windowsSource  = "internal/syscall/windows/syscall_windows.cs"
	)

	seedProject := flatProjectFile([]string{refErrors})

	seed := map[string]string{
		macosProject:   seedProject,
		macosSource:    "seed macos source",
		windowsProject: seedProject,
		windowsSource:  "seed windows source",
	}

	coreDir, stages := newCompanionStages(t, targets, seed)

	// darwin alone converts macos: a newer template line and one more reference than the seed.
	darwinProject := withTemplateLine(t, flatProjectFile([]string{refErrors, refRoute}))
	writeTestFile(t, filepath.Join(stages[2], "core", filepath.FromSlash(macosSource)), "darwin macos source")
	writeTestFile(t, filepath.Join(stages[2], "core", filepath.FromSlash(macosProject)), darwinProject)

	// Windows alone converts internal/syscall/windows, the same way.
	windowsEmitted := withTemplateLine(t, flatProjectFile([]string{refErrors, refIO}))
	writeTestFile(t, filepath.Join(stages[0], "core", filepath.FromSlash(windowsSource)), "windows source")
	writeTestFile(t, filepath.Join(stages[0], "core", filepath.FromSlash(windowsProject)), windowsEmitted)

	emissions := snapshotCompanionStages(t, targets, stages)

	conditioned, residual, _, err := mergeCompanionArtifacts(coreDir, targets, emissions)

	if err != nil {
		t.Fatalf("mergeCompanionArtifacts failed: %v", err)
	}

	for _, c := range []struct{ project, pkg, emitted, onlyReference string }{
		{macosProject, "crypto/x509/internal/macos", darwinProject, refRoute},
		{windowsProject, "internal/syscall/windows", windowsEmitted, refIO},
	} {
		got := readCompanionFile(t, filepath.Join(coreDir, filepath.FromSlash(c.project)))

		if !strings.Contains(got, templateLineMarker) {
			t.Errorf("%s: the template line its converting target emitted is missing; a stage holding the seed's copy was read instead", c.project)
		}

		if strings.Contains(got, platformReferenceBlockHeader) {
			t.Errorf("%s: references are conditioned by GOOS, but only one target builds the package; the seed copies' lists were read as import sets", c.project)
		}

		if !strings.Contains(got, c.onlyReference) {
			t.Errorf("%s: the reference only its converting target emits (%s) is missing", c.project, c.onlyReference)
		}

		if got != c.emitted {
			t.Errorf("%s: the merge did not reproduce the converting target's file\n--- want ---\n%s\n--- got ---\n%s", c.project, c.emitted, got)
		}

		if note, ok := residual[c.pkg]; ok {
			t.Errorf("%s: reported as disagreeing across targets (%q), but one target converts it", c.pkg, note)
		}

		for _, pkg := range conditioned {
			if pkg == c.pkg {
				t.Errorf("%s: listed as conditioned by GOOS", c.pkg)
			}
		}
	}
}
