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

	dir, note, _ := resolveDisclosureManifestDir(outputDir, root, moduleRootTestModule, moduleDir, "", pkgDir)

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

	dir, note, _ := resolveDisclosureManifestDir(outputDir, root, moduleRootTestModule, moduleDir, "", pkgDir)

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

	dir, note, _ := resolveDisclosureManifestDir(outputDir, root, moduleRootTestModule, moduleDir, "", pkgDir)

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

	dir, _, _ := resolveDisclosureManifestDir(outputDir, root, moduleRootTestModule, moduleDir, "", pkgDir)
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

// Arm 5: a main module that is not in a module cache (a plain checkout or a copy) has no resolved version, and
// -module-disclosures was given: the run REFUSES, naming both remedies, rather than printing a note and applying
// nothing (P5's sqlite run, 2026-11-20: the note let a host-fatal disclosure go unapplied and the host aborted).
func TestModuleDisclosureRootNoResolvedVersion(t *testing.T) {
	_, _, outputDir, root := moduleRootFixture(t)
	checkout := filepath.Join(t.TempDir(), "go-cmp")
	pkgDir := filepath.Join(checkout, "cmp")
	writeModuleRootManifest(t, filepath.Join(root, "github.com", "google", "go-cmp@v0.7.0", "cmp"), "TestDiff/Comparer/MapKeyPointer")

	dir, _, err := resolveDisclosureManifestDir(outputDir, root, moduleRootTestModule, checkout, "", pkgDir)

	if dir != outputDir {
		t.Fatalf("with no resolved version the root must not be consulted; got dir %q", dir)
	}
	if err == nil || !strings.Contains(err.Error(), "-module-version") || !strings.Contains(err.Error(), "module-cache") {
		t.Fatalf("a disclosure root that cannot be applied must be refused, naming -module-version and the module-cache "+
			"directory as the two remedies; got %v", err)
	}
}

// Arm 6: a COPY of the module (the scout and sqlite recipe) with its version named by -module-version: the root is
// consulted for that version, exactly as for a module-cache directory.
func TestModuleDisclosureRootCopyWithNamedVersionApplies(t *testing.T) {
	_, _, outputDir, root := moduleRootFixture(t)
	checkout := filepath.Join(t.TempDir(), "go-cmp")
	pkgDir := filepath.Join(checkout, "cmp")
	want := filepath.Join(root, "github.com", "google", "go-cmp@v0.7.0", "cmp")
	writeModuleRootManifest(t, want, "TestDiff/Comparer/MapKeyPointer")

	dir, note, err := resolveDisclosureManifestDir(outputDir, root, moduleRootTestModule, checkout, "v0.7.0", pkgDir)

	if err != nil || dir != want || !strings.Contains(note, filepath.Join(want, testDisclosureFileName)) {
		t.Fatalf("a copy named v0.7.0 must apply the v0.7.0 manifest; got dir %q, note %q, err %v", dir, note, err)
	}
}

// Arm 7: a module-cache directory names its own version; a different -module-version is a contradiction and is
// refused, naming both. The same version is accepted (the control).
func TestModuleDisclosureRootNamedVersionMustMatchTheCache(t *testing.T) {
	moduleDir, pkgDir, outputDir, root := moduleRootFixture(t)
	want := filepath.Join(root, "github.com", "google", "go-cmp@v0.7.0", "cmp")
	writeModuleRootManifest(t, want, "TestDiff/Comparer/MapKeyPointer")

	if _, _, err := resolveDisclosureManifestDir(outputDir, root, moduleRootTestModule, moduleDir, "v0.6.0", pkgDir); err == nil ||
		!strings.Contains(err.Error(), "v0.6.0") || !strings.Contains(err.Error(), "v0.7.0") {
		t.Fatalf("-module-version v0.6.0 on a v0.7.0 module-cache directory must be refused naming both; got %v", err)
	}

	if dir, _, err := resolveDisclosureManifestDir(outputDir, root, moduleRootTestModule, moduleDir, "v0.7.0", pkgDir); err != nil || dir != want {
		t.Fatalf("the matching version must apply; got dir %q, err %v", dir, err)
	}
}

// Arm 8: -module-version must be a semantic version.
func TestModuleDisclosureRootNamedVersionMustBeValid(t *testing.T) {
	_, _, outputDir, root := moduleRootFixture(t)
	checkout := filepath.Join(t.TempDir(), "go-cmp")

	if _, _, err := resolveDisclosureManifestDir(outputDir, root, moduleRootTestModule, checkout, "0.7.0", filepath.Join(checkout, "cmp")); err == nil ||
		!strings.Contains(err.Error(), "0.7.0") {
		t.Fatalf("-module-version 0.7.0 (no leading v) is not a module version and must be refused; got %v", err)
	}
}

// Arm 9: the PREFLIGHT runs before any conversion, from the module's go.mod, so a run that cannot apply its
// disclosure root fails in seconds rather than after an hour of conversion.
func TestModuleDisclosurePreflight(t *testing.T) {
	moduleDir, _, _, root := moduleRootFixture(t)
	checkout := filepath.Join(t.TempDir(), "go-cmp")

	for _, dir := range []string{moduleDir, checkout} {
		if err := os.MkdirAll(dir, 0o755); err != nil {
			t.Fatal(err)
		}
		if err := os.WriteFile(filepath.Join(dir, "go.mod"), []byte("module "+moduleRootTestModule+"\n\ngo 1.21\n"), 0o644); err != nil {
			t.Fatal(err)
		}
	}

	if err := moduleDisclosurePreflight(root, checkout, ""); err == nil || !strings.Contains(err.Error(), "-module-version") {
		t.Fatalf("a copy with no named version must be refused before conversion; got %v", err)
	}

	for _, arm := range []struct{ root, dir, version string }{
		{root, checkout, "v0.7.0"},
		{root, moduleDir, ""},
		{"", checkout, ""},
	} {
		if err := moduleDisclosurePreflight(arm.root, arm.dir, arm.version); err != nil {
			t.Errorf("preflight(root %q, dir %q, version %q) must pass; got %v", arm.root, arm.dir, arm.version, err)
		}
	}
}

// The standard library (no module path) and a run without the flag never consult a root, silently.
func TestModuleDisclosureRootInertForStdAndWithoutFlag(t *testing.T) {
	moduleDir, pkgDir, outputDir, root := moduleRootFixture(t)
	writeModuleRootManifest(t, filepath.Join(root, "github.com", "google", "go-cmp@v0.7.0", "cmp"), "TestDiff/Comparer/MapKeyPointer")

	// Run from INSIDE the tree, so an empty root that leaked through as a relative path would find the
	// manifest: only the flag's own guard keeps this arm inert.
	t.Chdir(root)

	if dir, note, _ := resolveDisclosureManifestDir(outputDir, "", moduleRootTestModule, moduleDir, "", pkgDir); dir != outputDir || note != "" {
		t.Fatalf("without the flag nothing changes; got %q, %q", dir, note)
	}
	if dir, note, _ := resolveDisclosureManifestDir(outputDir, root, "", moduleDir, "", pkgDir); dir != outputDir || note != "" {
		t.Fatalf("a standard-library package never consults the root; got %q, %q", dir, note)
	}
}

// P3 093309Z follow-ups (COORD 093422Z): a CLAIMED version is said and recorded, checked against the module cache's
// go.mod for that version when the cache has it, refused when the root holds the module under other versions only, and
// the two flags are refused where they cannot take effect.

// claimedCache points the claimed-version check at a module cache holding <module>@<version>'s go.mod.
func claimedCache(t *testing.T, version, goMod string) {
	t.Helper()
	cache := t.TempDir()
	dir := filepath.Join(cache, "cache", "download", "github.com", "google", "go-cmp", "@v")
	if err := os.MkdirAll(dir, 0o755); err != nil {
		t.Fatal(err)
	}
	if err := os.WriteFile(filepath.Join(dir, version+".mod"), []byte(goMod), 0o644); err != nil {
		t.Fatal(err)
	}
	previous := claimedVersionCacheDir
	claimedVersionCacheDir = func() string { return cache }
	t.Cleanup(func() { claimedVersionCacheDir = previous })
}

// copyWithGoMod makes a module copy (not a module-cache directory) whose go.mod is goMod.
func copyWithGoMod(t *testing.T, goMod string) (checkout, pkgDir string) {
	t.Helper()
	checkout = filepath.Join(t.TempDir(), "go-cmp")
	pkgDir = filepath.Join(checkout, "cmp")
	if err := os.MkdirAll(pkgDir, 0o755); err != nil {
		t.Fatal(err)
	}
	if err := os.WriteFile(filepath.Join(checkout, "go.mod"), []byte(goMod), 0o644); err != nil {
		t.Fatal(err)
	}
	return checkout, pkgDir
}

const goCmpGoMod = "module github.com/google/go-cmp\n\ngo 1.21\n"

// MUST 1(a): the applying line says the version was CLAIMED, and the record carries it.
func TestModuleDisclosureRootClaimedVersionIsSaidAndRecorded(t *testing.T) {
	_, _, outputDir, root := moduleRootFixture(t)
	claimedCache(t, "v0.7.0", goCmpGoMod)
	checkout, pkgDir := copyWithGoMod(t, goCmpGoMod)
	writeModuleRootManifest(t, filepath.Join(root, "github.com", "google", "go-cmp@v0.7.0", "cmp"), "TestDiff/Comparer/MapKeyPointer")

	_, note, err := resolveDisclosureManifestDir(outputDir, root, moduleRootTestModule, checkout, "v0.7.0", pkgDir)
	if err != nil || !strings.Contains(note, "claimed by -module-version, not resolved") {
		t.Fatalf("the applying line must say the version was claimed, not resolved; got note %q, err %v", note, err)
	}

	options := Options{testModuleDisclosures: root, mainModulePath: moduleRootTestModule, mainModuleDir: checkout, testModuleVersion: "v0.7.0"}
	if got := moduleVersionClaimed(options); got != "v0.7.0" {
		t.Fatalf("the comparison record must carry the claimed version; got %q", got)
	}

	moduleDir, _, _, _ := moduleRootFixture(t)
	options.mainModuleDir = moduleDir
	if got := moduleVersionClaimed(options); got != "" {
		t.Fatalf("a module-cache directory RESOLVES its version, even when -module-version agrees; got %q", got)
	}

	page := renderValidationProofPage(proofPageProvenance{importPath: "github.com/google/go-cmp/cmp", goVersion: "1.26.11",
		platform: "linux/amd64", convertedPath: "src/github.com/google/go-cmp/cmp"},
		testComparison{Package: "github.com/google/go-cmp/cmp", Status: "validated", Matched: true,
			Go: map[string]string{"TestA": "pass"}, CSharp: map[string]string{"TestA": "pass"},
			Environment: testEnvironmentRecord{ModuleVersionClaimed: "v0.7.0"}}, nil, nil)
	if !strings.Contains(page, "v0.7.0") || !strings.Contains(page, "claimed by -module-version") {
		t.Fatalf("a module proof page read under a claimed version must say so:\n%s", page)
	}
}

// MUST 1(b): when the module cache holds the claimed version, the copy's go.mod must be that version's.
func TestModuleDisclosureRootClaimedVersionMatchesTheCachesGoMod(t *testing.T) {
	_, _, outputDir, root := moduleRootFixture(t)
	claimedCache(t, "v0.7.0", goCmpGoMod)
	writeModuleRootManifest(t, filepath.Join(root, "github.com", "google", "go-cmp@v0.7.0", "cmp"), "TestDiff/Comparer/MapKeyPointer")

	checkout, pkgDir := copyWithGoMod(t, "module github.com/google/go-cmp\n\ngo 1.22\n")
	if _, _, err := resolveDisclosureManifestDir(outputDir, root, moduleRootTestModule, checkout, "v0.7.0", pkgDir); err == nil ||
		!strings.Contains(err.Error(), "go.mod") || !strings.Contains(err.Error(), "v0.7.0") {
		t.Fatalf("a copy whose go.mod differs from the cache's v0.7.0 go.mod must be refused; got %v", err)
	}

	same, samePkg := copyWithGoMod(t, strings.ReplaceAll(goCmpGoMod, "\n", "\r\n"))
	if _, _, err := resolveDisclosureManifestDir(outputDir, root, moduleRootTestModule, same, "v0.7.0", samePkg); err != nil {
		t.Fatalf("the same go.mod (CRLF aside) must be accepted; got %v", err)
	}
}

// SHOULD 2: an explicit -module-version whose root keeps the module under OTHER versions only is refused (it is most
// likely a wrong claim), in the lookup and in the preflight.
func TestModuleDisclosureRootClaimedVersionWithOtherVersionsOnlyIsRefused(t *testing.T) {
	_, _, outputDir, root := moduleRootFixture(t)
	claimedCache(t, "v0.6.0", goCmpGoMod)
	checkout, pkgDir := copyWithGoMod(t, goCmpGoMod)
	writeModuleRootManifest(t, filepath.Join(root, "github.com", "google", "go-cmp@v0.7.0", "cmp"), "TestDiff/Comparer/MapKeyPointer")

	if _, _, err := resolveDisclosureManifestDir(outputDir, root, moduleRootTestModule, checkout, "v0.6.0", pkgDir); err == nil ||
		!strings.Contains(err.Error(), "v0.7.0") || !strings.Contains(err.Error(), "v0.6.0") {
		t.Fatalf("claimed v0.6.0 with the root holding v0.7.0 only must be refused naming both; got %v", err)
	}

	if err := moduleDisclosurePreflight(root, checkout, "v0.6.0"); err == nil || !strings.Contains(err.Error(), "v0.7.0") {
		t.Fatalf("the preflight must refuse the same claim before any conversion; got %v", err)
	}
}

// SHOULD 3 + NIT 4: both flags take effect only on a -tests -recurse run (the single-package route never knows its
// module, so the root would be ignored silently), and -module-version only with -module-disclosures.
func TestModuleDisclosureFlagsAreRefusedWhereTheyCannotTakeEffect(t *testing.T) {
	for _, arm := range []struct {
		name      string
		options   Options
		wantError string
	}{
		{"disclosures without -recurse", Options{convertTests: true, testModuleDisclosures: "/r"}, "-recurse"},
		{"disclosures without -tests", Options{recurse: true, testModuleDisclosures: "/r"}, "-tests"},
		{"version without disclosures", Options{convertTests: true, recurse: true, testModuleVersion: "v1.0.0"}, "-module-disclosures"},
		{"both, -tests -recurse", Options{convertTests: true, recurse: true, testModuleDisclosures: "/r", testModuleVersion: "v1.0.0"}, ""},
		{"neither", Options{convertTests: true}, ""},
	} {
		err := moduleDisclosureFlagsError(arm.options)
		if arm.wantError == "" && err != nil || arm.wantError != "" && (err == nil || !strings.Contains(err.Error(), arm.wantError)) {
			t.Errorf("%s: got %v, want an error naming %q (empty: none)", arm.name, err, arm.wantError)
		}
	}
}

// P3 NIT 6 (COORD 101957Z): a disclosure root whose path holds a glob metacharacter ([ or *) still sees the module's
// other versions. filepath.Glob read "root[x]" as a pattern, matched nothing, and the wrong-version line and the
// claimed-version refusal both went silent.
func TestModuleDisclosureRootWithGlobCharactersStillSeesOtherVersions(t *testing.T) {
	moduleDir, pkgDir, outputDir, _ := moduleRootFixture(t)
	root := filepath.Join(t.TempDir(), "Module[Disclosures]*")
	writeModuleRootManifest(t, filepath.Join(root, "github.com", "google", "go-cmp@v0.6.0", "cmp"), "TestDiff/Comparer/MapKeyPointer")

	if _, note, err := resolveDisclosureManifestDir(outputDir, root, moduleRootTestModule, moduleDir, "", pkgDir); err != nil ||
		!strings.Contains(note, "v0.6.0") || !strings.Contains(note, "not applied") {
		t.Fatalf("the root holds v0.6.0 only and this run resolved v0.7.0: the line must say so; got note %q, err %v", note, err)
	}

	claimedCache(t, "v0.7.0", goCmpGoMod)
	checkout, _ := copyWithGoMod(t, goCmpGoMod)

	if err := moduleDisclosurePreflight(root, checkout, "v0.7.0"); err == nil || !strings.Contains(err.Error(), "v0.6.0") {
		t.Fatalf("a claimed v0.7.0 against a root holding v0.6.0 only must be refused, whatever the root's path; got %v", err)
	}
}
