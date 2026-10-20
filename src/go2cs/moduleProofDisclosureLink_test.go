// moduleProofDisclosureLink_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// A third-party proof page that discloses a divergence names the manifest that pins it. The page ships in
// the module's NuGet package and its conversion repository, so the link must reach the manifest the run
// actually read. Found on the golang.org/x/sync v0.19.0 kit (2026-10-12): the page linked
// blob/master/src/core/golang.org/x/sync/semaphore/, the STANDARD LIBRARY's home, while the manifest was
// read from the committed -module-disclosures tree (src/tests/ModuleDisclosures/golang.org/x/sync@v0.19.0/
// semaphore/), so the shipped link would read 404.

package main

import (
	"os"
	"path/filepath"
	"strings"
	"testing"
)

// moduleDisclosurePageFixture converts example.com/mod/sub at v1.2.3 from a module-cache-shaped directory,
// with one test that Go passes and C# fails, into <repo>/modout. It returns the options a -tests run would
// carry, the package's output directory and the comparison.
func moduleDisclosurePageFixture(t *testing.T) (Options, string, string, testComparison, testManifest) {
	t.Helper()
	temp := t.TempDir()
	root, _ := fakeCheckout(t, filepath.Join(temp, "repo"))
	moduleDir := filepath.Join(temp, "modcache", "example.com", "mod@v1.2.3")
	outputPath := filepath.Join(root, "modout", "src", "example.com", "mod", "sub")

	for _, dir := range []string{filepath.Join(moduleDir, "sub"), outputPath} {
		if err := os.MkdirAll(dir, 0o755); err != nil {
			t.Fatal(err)
		}
	}

	options := Options{
		targetPlatform:        "linux/amd64",
		go2csPath:             root,
		testModuleDisclosures: filepath.Join(root, "tests", "ModuleDisclosures"),
		mainModulePath:        "example.com/mod",
		mainModuleDir:         moduleDir,
	}
	comparison := testComparison{
		Package: "example.com/mod/sub", Status: "validated", Matched: true,
		Go:     map[string]string{"TestA": "pass", "TestB": "pass"},
		CSharp: map[string]string{"TestA": "pass", "TestB": "fail"},
	}
	manifest := testManifest{PackageImportPath: "example.com/mod/sub", ModulePath: "example.com/mod", GoVersion: "go1.24.13"}

	return options, outputPath, filepath.Join(root, "modout"), comparison, manifest
}

func readModuleDisclosurePage(t *testing.T, outRoot string) string {
	t.Helper()
	page, err := os.ReadFile(filepath.Join(outRoot, "validation", "example.com", "mod", "sub.md"))
	if err != nil {
		t.Fatalf("the third-party page was not written: %v", err)
	}
	return string(page)
}

// Arm 1: the manifest the run read is in the committed -module-disclosures tree inside the go2cs checkout:
// the page links THAT file, at its path in the go2cs repository, and never the standard library's src/core.
func TestThirdPartyProofPageLinksTheModuleDisclosureTree(t *testing.T) {
	options, outputPath, outRoot, comparison, manifest := moduleDisclosurePageFixture(t)
	manifestDir := filepath.Join(options.testModuleDisclosures, "example.com", "mod@v1.2.3", "sub")
	writeModuleRootManifest(t, manifestDir, "TestB")

	disclosures, _, _, err := loadTestDisclosures(manifestDir, "linux")
	if err != nil || len(disclosures) != 1 {
		t.Fatalf("fixture manifest: %v, %v", disclosures, err)
	}

	if err := emitValidationProofPage(outputPath, comparison, manifest, disclosures, nil, options); err != nil {
		t.Fatalf("emitValidationProofPage: %v", err)
	}

	text := readModuleDisclosurePage(t, outRoot)
	want := "[`go2cs_test_disclosures.json`](" + go2csRepositoryURL +
		"/blob/master/src/tests/ModuleDisclosures/example.com/mod@v1.2.3/sub/go2cs_test_disclosures.json)"

	if !strings.Contains(text, want) {
		t.Errorf("page: want the link to the manifest the run read,\n  %s\nin:\n%s", want, text)
	}
	if strings.Contains(text, "src/core/") {
		t.Errorf("page: a third-party page must not link the standard library's src/core:\n%s", text)
	}
}

// Arm 2: the manifest sits in the package's own output directory, outside the go2cs checkout: no go2cs
// URL reaches it, so the page names the file beside the converted package and links nothing.
func TestThirdPartyProofPageNamesAnOutputDirectoryManifest(t *testing.T) {
	options, outputPath, outRoot, comparison, manifest := moduleDisclosurePageFixture(t)
	options.go2csPath = filepath.Join(t.TempDir(), "elsewhere", "src")
	writeModuleRootManifest(t, outputPath, "TestB")

	disclosures, _, _, err := loadTestDisclosures(outputPath, "linux")
	if err != nil || len(disclosures) != 1 {
		t.Fatalf("fixture manifest: %v, %v", disclosures, err)
	}

	if err := emitValidationProofPage(outputPath, comparison, manifest, disclosures, nil, options); err != nil {
		t.Fatalf("emitValidationProofPage: %v", err)
	}

	text := readModuleDisclosurePage(t, outRoot)
	want := "hand-owned `go2cs_test_disclosures.json` beside the converted package (`src/example.com/mod/sub`);"

	if !strings.Contains(text, want) {
		t.Errorf("page: want the manifest named beside the converted package,\n  %s\nin:\n%s", want, text)
	}
	if strings.Contains(text, "/blob/") {
		t.Errorf("page: no go2cs URL reaches a manifest outside the checkout, so none is written:\n%s", text)
	}
}
