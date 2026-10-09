// moduleProofPages_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards M1 of the multi-package module design (docs/phase4/DESIGN-multi-package-modules.md, D2 and D7,
// ruled 2026-09-29): a comparison record is keyed by the package's FULL import path, so two packages
// named alike in different directories never collide; and a THIRD-PARTY package's proof page is written
// beside its conversion, at <outRoot>/validation/<modulePath>/<relative path>.md, never into a go2cs
// checkout's standard-library roster. The standard-library destination is the control: a package
// converted at <root>/core/<importPath> still publishes into that checkout's docs/ exactly as before.

package main

import (
	"os"
	"path/filepath"
	"strings"
	"testing"
)

func TestComparisonRecordsAreKeyedByImportPath(t *testing.T) {
	first := comparisonRecordPackage(testManifest{PackageImportPath: "example.com/alpha/test"}, filepath.Join("x", "alpha", "test"))
	second := comparisonRecordPackage(testManifest{PackageImportPath: "example.com/beta/test"}, filepath.Join("x", "beta", "test"))

	if first == second {
		t.Fatalf("two packages named alike in different directories share one record key %q", first)
	}

	if first != "example.com/alpha/test" {
		t.Errorf("record key: want the full import path, got %q", first)
	}

	// A manifest with no import path (a load that never reached the package) keeps the directory name.
	if got := comparisonRecordPackage(testManifest{}, filepath.Join("x", "gamma")); got != "gamma" {
		t.Errorf("record key without an import path: want the directory name, got %q", got)
	}
}

// fakeCheckout lays out the smallest tree findGo2CSRootAbove recognizes: <repo>/src/core/golib/golib.csproj
// and <repo>/docs. It returns <repo>/src (the go2cs root) and <repo>/docs.
func fakeCheckout(t *testing.T, repo string) (string, string) {
	t.Helper()

	root := filepath.Join(repo, "src")
	docs := filepath.Join(repo, "docs")

	for _, dir := range []string{filepath.Join(root, "core", "golib"), filepath.Join(docs, "validation", "current")} {
		if err := os.MkdirAll(dir, 0o755); err != nil {
			t.Fatal(err)
		}
	}

	if err := os.WriteFile(filepath.Join(root, "core", "golib", "golib.csproj"), []byte("<Project/>"), 0o644); err != nil {
		t.Fatal(err)
	}

	return root, docs
}

func TestThirdPartyProofPageDestinations(t *testing.T) {
	temp := t.TempDir()
	root, docs := fakeCheckout(t, filepath.Join(temp, "repo"))

	cases := []struct {
		name       string
		outputPath string
		manifest   testManifest
		wantDocs   string
		wantPage   string
	}{
		{
			// CONTROL: the standard library, converted at <root>/core/<importPath>.
			name:       "stdlib package in its checkout",
			outputPath: filepath.Join(root, "core", "crypto", "tls"),
			manifest:   testManifest{PackageImportPath: "crypto/tls"},
			wantDocs:   docs,
		},
		{
			// The recurse layout INSIDE a checkout: must never publish into that checkout's roster.
			name:       "third-party subpackage inside a checkout",
			outputPath: filepath.Join(root, "modout", "src", "example.com", "mod", "sub"),
			manifest:   testManifest{PackageImportPath: "example.com/mod/sub", ModulePath: "example.com/mod"},
			wantPage:   filepath.Join(root, "modout", "validation", "example.com", "mod", "sub.md"),
		},
		{
			name:       "third-party root package outside any checkout",
			outputPath: filepath.Join(temp, "elsewhere", "src", "example.com", "mod"),
			manifest:   testManifest{PackageImportPath: "example.com/mod", ModulePath: "example.com/mod"},
			wantPage:   filepath.Join(temp, "elsewhere", "validation", "example.com", "mod", "index.md"),
		},
		{
			// Not the recurse layout: the output directory itself is the root.
			name:       "third-party package in a plain output directory",
			outputPath: filepath.Join(temp, "plain"),
			manifest:   testManifest{PackageImportPath: "example.com/mod/deep/leaf", ModulePath: "example.com/mod"},
			wantPage:   filepath.Join(temp, "plain", "validation", "example.com", "mod", "deep", "leaf.md"),
		},
	}

	for _, c := range cases {
		got := validationProofDestination(c.outputPath, c.manifest)

		if got.docsPath != c.wantDocs || got.pagePath != c.wantPage {
			t.Errorf("%s: want docs %q page %q, got docs %q page %q", c.name, c.wantDocs, c.wantPage, got.docsPath, got.pagePath)
		}
	}
}

func TestThirdPartyProofPageIsWrittenBesideTheConversion(t *testing.T) {
	temp := t.TempDir()
	root, docs := fakeCheckout(t, filepath.Join(temp, "repo"))
	outRoot := filepath.Join(root, "modout")

	comparison := testComparison{
		Package: "example.com/mod/sub", Status: "validated", Matched: true,
		Go:     map[string]string{"TestA": "pass", "TestB": "pass", "TestC": "skip"},
		CSharp: map[string]string{"TestA": "pass", "TestB": "pass", "TestC": "skip"},
	}
	manifest := testManifest{PackageImportPath: "example.com/mod/sub", ModulePath: "example.com/mod", GoVersion: "go1.24.13"}

	if err := emitValidationProofPage(filepath.Join(outRoot, "src", "example.com", "mod", "sub"), comparison, manifest, nil, nil,
		Options{targetPlatform: "windows/amd64"}); err != nil {
		t.Fatalf("emitValidationProofPage: %v", err)
	}

	page, err := os.ReadFile(filepath.Join(outRoot, "validation", "example.com", "mod", "sub.md"))
	if err != nil {
		t.Fatalf("the third-party page was not written beside the conversion: %v", err)
	}

	text := string(page)
	for _, want := range []string{"`example.com/mod/sub` — validation proof", "**3 matched · 0 disclosed**"} {
		if !strings.Contains(text, want) {
			t.Errorf("page: want %q in:\n%s", want, text)
		}
	}

	for _, stdlibOnly := range []string{"ValidatedTestPackages.md", "src/core/"} {
		if strings.Contains(text, stdlibOnly) {
			t.Errorf("page: a third-party page must not point into the standard-library roster (%q):\n%s", stdlibOnly, text)
		}
	}

	module, err := os.ReadFile(filepath.Join(outRoot, "validation", "example.com", "mod", "MODULE.md"))
	if err != nil {
		t.Fatalf("the module summary was not written: %v", err)
	}

	if !strings.Contains(string(module), "example.com/mod/sub") || !strings.Contains(string(module), "3 matched") {
		t.Errorf("MODULE.md: want the package row and its total:\n%s", module)
	}

	// The checkout's standard-library docs are untouched.
	entries, err := os.ReadDir(filepath.Join(docs, "validation", "current"))
	if err != nil {
		t.Fatal(err)
	}

	if len(entries) != 0 {
		t.Errorf("the checkout's docs/validation/current gained %d file(s) from a third-party package", len(entries))
	}
}

// A module's verdicts are Go's under the build tags the reading used (a -recurse run adds `safe`), so
// each module proof page states them, and MODULE.md states them once when every page agrees and says
// so when they do not. A standard-library page carries no such sentence: its purego default is
// documented once for the whole library, and adding one would rewrite every banked page.
func TestModuleProofPagesStateTheirBuildTags(t *testing.T) {
	temp := t.TempDir()
	root, _ := fakeCheckout(t, filepath.Join(temp, "repo"))
	outRoot := filepath.Join(root, "modout")
	moduleTags := resolveBuildTags(false, true, true, true, false, nil)

	emit := func(importPath string, tags []string) {
		t.Helper()

		comparison := testComparison{
			Package: importPath, Status: "validated", Matched: true,
			Go: map[string]string{"TestA": "pass"}, CSharp: map[string]string{"TestA": "pass"},
		}
		manifest := testManifest{PackageImportPath: importPath, ModulePath: "example.com/mod", GoVersion: "go1.24.13"}
		output := filepath.Join(append([]string{outRoot, "src"}, strings.Split(importPath, "/")...)...)

		if err := emitValidationProofPage(output, comparison, manifest, nil, nil,
			Options{targetPlatform: "windows/amd64", buildTags: tags}); err != nil {
			t.Fatalf("emitValidationProofPage(%s): %v", importPath, err)
		}
	}

	read := func(parts ...string) string {
		t.Helper()

		data, err := os.ReadFile(filepath.Join(append([]string{outRoot, "validation", "example.com", "mod"}, parts...)...))
		if err != nil {
			t.Fatal(err)
		}

		return string(data)
	}

	emit("example.com/mod/sub", moduleTags)

	tagsText := "`-tags " + strings.Join(moduleTags, ",") + "`"

	if page := read("sub.md"); !strings.Contains(page, "Both sides were built with "+tagsText+".") {
		t.Errorf("a module proof page must state its build tags (%s):\n%s", tagsText, page)
	}

	if module := read("MODULE.md"); !strings.Contains(module, "Both sides of every reading were built with "+tagsText+".") {
		t.Errorf("MODULE.md must state the tags its pages agree on (%s):\n%s", tagsText, module)
	}

	// A second package read under other tags: MODULE.md no longer names one set.
	emit("example.com/mod/other", defaultStdLibBuildTags)

	module := read("MODULE.md")
	if !strings.Contains(module, "read with different build tags") || strings.Contains(module, "Both sides of every reading") {
		t.Errorf("MODULE.md must say the pages' build tags differ rather than name one set:\n%s", module)
	}

	stdlib := renderValidationProofPage(proofPageProvenance{importPath: "sort", goVersion: "1.24.13", platform: "windows/amd64", buildTags: moduleTags},
		testComparison{Package: "sort", Status: "validated", Matched: true, Go: map[string]string{"TestA": "pass"}, CSharp: map[string]string{"TestA": "pass"}},
		nil, nil)

	if strings.Contains(stdlib, "Both sides were built with") {
		t.Errorf("a standard-library page must render exactly as before, with no build-tags sentence:\n%s", stdlib)
	}
}

// The nugetgo rehearsal (2026-10-09, gap 7): MODULE.md is what a module pack ships as VALIDATION.md, so it lists the
// declarations each proof page excluded from both sides, not only the counts the pages claim.
func TestModuleSummaryListsTheExcludedDeclarations(t *testing.T) {
	temp := t.TempDir()
	root, _ := fakeCheckout(t, filepath.Join(temp, "repo"))
	outRoot := filepath.Join(root, "modout")

	emit := func(importPath string, excluded []string) {
		t.Helper()

		comparison := testComparison{
			Package: importPath, Status: "validated", Matched: true, Excluded: excluded,
			Go: map[string]string{"TestA": "pass"}, CSharp: map[string]string{"TestA": "pass"},
		}
		manifest := testManifest{PackageImportPath: importPath, ModulePath: "example.com/mod", GoVersion: "go1.24.13"}
		output := filepath.Join(append([]string{outRoot, "src"}, strings.Split(importPath, "/")...)...)

		if err := emitValidationProofPage(output, comparison, manifest, nil, nil, Options{targetPlatform: "linux/amd64"}); err != nil {
			t.Fatalf("emitValidationProofPage(%s): %v", importPath, err)
		}
	}

	emit("example.com/mod", []string{
		"ExampleB (example): example execution is deferred to Phase 4D",
		"ExampleA (example): example execution is deferred to Phase 4D",
	})
	emit("example.com/mod/sub", nil)

	data, err := os.ReadFile(filepath.Join(outRoot, "validation", "example.com", "mod", "MODULE.md"))
	if err != nil {
		t.Fatal(err)
	}

	module := string(data)

	for _, want := range []string{
		"## Excluded declarations",
		"2 declaration(s) were excluded from both sides of the comparison",
		"- `example.com/mod`: ExampleA (example): example execution is deferred to Phase 4D\n" +
			"- `example.com/mod`: ExampleB (example): example execution is deferred to Phase 4D\n",
	} {
		if !strings.Contains(module, want) {
			t.Errorf("MODULE.md: want %q in:\n%s", want, module)
		}
	}

	if strings.Contains(module, "`example.com/mod/sub`:") {
		t.Errorf("MODULE.md lists an exclusion for a package that has none:\n%s", module)
	}
}
