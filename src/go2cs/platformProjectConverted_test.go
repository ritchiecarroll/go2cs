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

// conditionedProjectFile renders a project file carrying a per-GOOS reference block, as the merge and the
// single-target writer both render it.
func conditionedProjectFile(t *testing.T, sets map[string][]string) string {
	t.Helper()

	shared, deltas, _ := splitPlatformReferenceSets(sets)
	rendered, err := renderPlatformReferences(flatProjectFile(sets["windows"]), shared, deltas)

	if err != nil {
		t.Fatalf("test setup: renderPlatformReferences: %v", err)
	}

	if !strings.Contains(rendered, platformReferenceBlockHeader) {
		t.Fatalf("test setup: the rendered file carries no per-GOOS block:\n%s", rendered)
	}

	return rendered
}

// TestMergeDropsAPerGOOSAxisNoConvertingTargetConfirms is the second half of the project-file finding (the hop's
// freeze regen, 2026-10-23). internal/syscall/windows selects Go files on Windows alone, yet its committed .csproj
// carries a per-GOOS block recording darwin and linux as taking part (empty groups). The single-target writer
// keeps it, because it recovers the other platforms' sets from the file it overwrites, and when all three stage
// copies are byte-identical the merge only copies the file, so the axis survived every run and went only when an
// unrelated change made the copies differ. The emission had two fixed points for the file, split and flat, chosen
// by history. A project file that carries a per-GOOS block now always goes through the converted-only merge.
//
// log/syslog is the idempotence control: all three targets convert it and their import sets really differ, so
// re-rendering from the three converting stages gives back exactly the file they hold.
func TestMergeDropsAPerGOOSAxisNoConvertingTargetConfirms(t *testing.T) {
	targets := []string{"windows/amd64", "linux/amd64", "darwin/amd64"}

	const (
		windowsProject = "internal/syscall/windows/internal.syscall.windows.csproj"
		windowsSource  = "internal/syscall/windows/syscall_windows.cs"
		syslogProject  = "log/syslog/log.syslog.csproj"
		syslogSource   = "log/syslog/syslog.cs"
	)

	staleSplit := conditionedProjectFile(t, map[string][]string{
		"windows": {refErrors, refIO},
		"linux":   {refErrors},
		"darwin":  {refErrors},
	})

	syslogSplit := conditionedProjectFile(t, map[string][]string{
		"windows": {refErrors},
		"linux":   {refErrors, refIO},
		"darwin":  {refErrors, refIO},
	})

	seed := map[string]string{
		windowsProject: staleSplit,
		windowsSource:  "seed windows source",
		syslogProject:  syslogSplit,
		syslogSource:   "seed syslog source",
	}

	coreDir, stages := newCompanionStages(t, targets, seed)

	// Windows alone converts internal/syscall/windows and, adopting the axis from the seed, writes the seed's
	// bytes back: every stage now holds the same project file.
	writeTestFile(t, filepath.Join(stages[0], "core", filepath.FromSlash(windowsSource)), "windows source")
	writeTestFile(t, filepath.Join(stages[0], "core", filepath.FromSlash(windowsProject)), staleSplit)

	// All three convert log/syslog, and each writes back the same conditioned file.
	for i := range targets {
		writeTestFile(t, filepath.Join(stages[i], "core", filepath.FromSlash(syslogSource)), "syslog source")
		writeTestFile(t, filepath.Join(stages[i], "core", filepath.FromSlash(syslogProject)), syslogSplit)
	}

	emissions := snapshotCompanionStages(t, targets, stages)

	if _, _, _, err := mergeCompanionArtifacts(coreDir, targets, emissions); err != nil {
		t.Fatalf("mergeCompanionArtifacts failed: %v", err)
	}

	got := readCompanionFile(t, filepath.Join(coreDir, filepath.FromSlash(windowsProject)))

	if strings.Contains(got, platformReferenceBlockHeader) {
		t.Errorf("%s: the per-GOOS block survived although only windows converts the package; the stages' identical copies were copied instead of merged", windowsProject)
	}

	if !strings.Contains(got, refIO) {
		t.Errorf("%s: a reference windows imports (%s) is missing from the flat list", windowsProject, refIO)
	}

	if want := flatProjectFile([]string{refErrors, refIO}); got != want {
		t.Errorf("%s: want windows's flat project file\n--- want ---\n%s\n--- got ---\n%s", windowsProject, want, got)
	}

	if got := readCompanionFile(t, filepath.Join(coreDir, filepath.FromSlash(syslogProject))); got != syslogSplit {
		t.Errorf("%s: a genuinely multi-platform project file changed under the re-merge\n--- want ---\n%s\n--- got ---\n%s", syslogProject, syslogSplit, got)
	}
}
