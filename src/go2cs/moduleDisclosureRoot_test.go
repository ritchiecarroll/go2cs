// moduleDisclosureRoot_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// The -module-disclosures root (COORD ruling 2026-10-05, 21:27Z): a third-party module row's disclosure
// manifests live in a committed tree keyed by module path AND version, and the -tests pipeline reads one
// only when the package's output directory holds none, and only for the module version the run
// resolved. These arms pin the four properties the ruling names, plus the no-version case.

package main

import (
	"os"
	"path/filepath"
	"strings"
	"testing"
)

const moduleRootTestModule = "github.com/google/go-cmp"

// moduleRootFixture lays out a module-cache-shaped module directory (…/github.com/google/go-cmp@v0.7.0)
// with one package (cmp), an empty output directory, and an empty disclosure root.
func moduleRootFixture(t *testing.T) (moduleDir, pkgDir, outputDir, root string) {
	t.Helper()
	base := t.TempDir()
	moduleDir = filepath.Join(base, "modcache", "github.com", "google", "go-cmp@v0.7.0")
	pkgDir = filepath.Join(moduleDir, "cmp")
	outputDir = filepath.Join(base, "out", "src", "github.com", "google", "go-cmp", "cmp")
	root = filepath.Join(base, "ModuleDisclosures")

	for _, dir := range []string{pkgDir, outputDir, root} {
		if err := os.MkdirAll(dir, 0o755); err != nil {
			t.Fatal(err)
		}
	}

	return moduleDir, pkgDir, outputDir, root
}

func writeModuleRootManifest(t *testing.T, dir string, names ...string) {
	t.Helper()
	if err := os.MkdirAll(dir, 0o755); err != nil {
		t.Fatal(err)
	}

	var entries []string
	for _, name := range names {
		entries = append(entries, `{"name": "`+name+`", "class": "runtime-capability", "signature": "the pinned text", "reason": "because"}`)
	}

	manifest := `{"schemaVersion": 1, "disclosures": [` + strings.Join(entries, ", ") + `]}`

	if err := os.WriteFile(filepath.Join(dir, testDisclosureFileName), []byte(manifest), 0o644); err != nil {
		t.Fatal(err)
	}
}

// Arm 1: the root holds <module>@<resolved version>/<package dir>, the output directory holds nothing:
// that manifest is the one read, and the line names its file.
func TestModuleDisclosureRootApplied(t *testing.T) {
	moduleDir, pkgDir, outputDir, root := moduleRootFixture(t)
	want := filepath.Join(root, "github.com", "google", "go-cmp@v0.7.0", "cmp")
	writeModuleRootManifest(t, want, "TestDiff/Comparer/MapKeyPointer")

	dir, note := resolveDisclosureManifestDir(outputDir, root, moduleRootTestModule, moduleDir, pkgDir)

	if dir != want {
		t.Fatalf("the module root's manifest for the resolved version must be the one read; got dir %q, want %q (note %q)", dir, want, note)
	}
	if !strings.Contains(note, filepath.Join(want, testDisclosureFileName)) {
		t.Fatalf("the line must name the applied file; got %q", note)
	}

	disclosures, _, _, err := loadTestDisclosures(dir, "linux")
	if err != nil || len(disclosures) != 1 {
		t.Fatalf("the applied manifest must load through the production door; got %v, %v", disclosures, err)
	}
}

// Arm 2: the root holds the module under ANOTHER version only: nothing is applied, and the line names
// both the version present and the version this run resolved.
func TestModuleDisclosureRootWrongVersionRefused(t *testing.T) {
	moduleDir, pkgDir, outputDir, root := moduleRootFixture(t)
	writeModuleRootManifest(t, filepath.Join(root, "github.com", "google", "go-cmp@v0.6.0", "cmp"), "TestDiff/Comparer/MapKeyPointer")

	dir, note := resolveDisclosureManifestDir(outputDir, root, moduleRootTestModule, moduleDir, pkgDir)

	if dir != outputDir {
		t.Fatalf("a manifest for another version must NOT be applied; got dir %q", dir)
	}
	if !strings.Contains(note, "v0.6.0") || !strings.Contains(note, "v0.7.0") || !strings.Contains(note, "not applied") {
		t.Fatalf("the line must name the version present and the version resolved, and say not applied; got %q", note)
	}
}

// Arm 3: both the output directory and the root hold a manifest: the output directory's still wins, and
// the root is not consulted.
func TestModuleDisclosureRootOutputDirWins(t *testing.T) {
	moduleDir, pkgDir, outputDir, root := moduleRootFixture(t)
	writeModuleRootManifest(t, outputDir, "TestFromOutputDir")
	writeModuleRootManifest(t, filepath.Join(root, "github.com", "google", "go-cmp@v0.7.0", "cmp"), "TestFromRoot")

	dir, note := resolveDisclosureManifestDir(outputDir, root, moduleRootTestModule, moduleDir, pkgDir)

	if dir != outputDir || note != "" {
		t.Fatalf("a manifest in the output directory must win, silently; got dir %q, note %q", dir, note)
	}

	disclosures, _, _, err := loadTestDisclosures(dir, "linux")
	if _, ok := disclosures["TestFromOutputDir"]; err != nil || !ok || len(disclosures) != 1 {
		t.Fatalf("only the output directory's entries may load; got %v, %v", disclosures, err)
	}
}

// Arm 4: a manifest applied from the root is subject to the orphan rule exactly as a std manifest is: a
// disclosed row whose converted side passes is reported.
func TestModuleDisclosureRootOrphanReported(t *testing.T) {
	moduleDir, pkgDir, outputDir, root := moduleRootFixture(t)
	writeModuleRootManifest(t, filepath.Join(root, "github.com", "google", "go-cmp@v0.7.0", "cmp"), "TestDiff/Comparer/MapKeyPointer")

	dir, _ := resolveDisclosureManifestDir(outputDir, root, moduleRootTestModule, moduleDir, pkgDir)
	disclosures, _, _, err := loadTestDisclosures(dir, "linux")
	if err != nil {
		t.Fatal(err)
	}

	orphans := orphanedDisclosures(disclosures,
		map[string]string{"TestDiff/Comparer/MapKeyPointer": "pass"},
		map[string]string{"TestDiff/Comparer/MapKeyPointer": "pass"},
		"linux")

	if len(orphans) != 1 || orphans[0].Name != "TestDiff/Comparer/MapKeyPointer" {
		t.Fatalf("a root-applied disclosure over a passing row must be reported as orphaned; got %v", orphans)
	}
}

// Arm 5: a main module that is not in a module cache (a plain checkout) has no resolved version: the root
// is not consulted, and the line says so.
func TestModuleDisclosureRootNoResolvedVersion(t *testing.T) {
	_, _, outputDir, root := moduleRootFixture(t)
	checkout := filepath.Join(t.TempDir(), "go-cmp")
	pkgDir := filepath.Join(checkout, "cmp")
	writeModuleRootManifest(t, filepath.Join(root, "github.com", "google", "go-cmp@v0.7.0", "cmp"), "TestDiff/Comparer/MapKeyPointer")

	dir, note := resolveDisclosureManifestDir(outputDir, root, moduleRootTestModule, checkout, pkgDir)

	if dir != outputDir {
		t.Fatalf("with no resolved version the root must not be consulted; got dir %q", dir)
	}
	if !strings.Contains(note, "no resolved version") || !strings.Contains(note, "not consulted") {
		t.Fatalf("the line must say the module has no resolved version and the root was not consulted; got %q", note)
	}
}

// The standard library (no module path) and a run without the flag never consult a root, silently.
func TestModuleDisclosureRootInertForStdAndWithoutFlag(t *testing.T) {
	moduleDir, pkgDir, outputDir, root := moduleRootFixture(t)
	writeModuleRootManifest(t, filepath.Join(root, "github.com", "google", "go-cmp@v0.7.0", "cmp"), "TestDiff/Comparer/MapKeyPointer")

	// Run from INSIDE the tree, so an empty root that leaked through as a relative path would find the
	// manifest: only the flag's own guard keeps this arm inert.
	t.Chdir(root)

	if dir, note := resolveDisclosureManifestDir(outputDir, "", moduleRootTestModule, moduleDir, pkgDir); dir != outputDir || note != "" {
		t.Fatalf("without the flag nothing changes; got %q, %q", dir, note)
	}
	if dir, note := resolveDisclosureManifestDir(outputDir, root, "", moduleDir, pkgDir); dir != outputDir || note != "" {
		t.Fatalf("a standard-library package never consults the root; got %q, %q", dir, note)
	}
}
