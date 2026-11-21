// hostFatalModuleEvidence_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"os"
	"path/filepath"
	"strings"
	"testing"
)

// The host-fatal mint rule for a MODULE row (COORD 20261120T091624Z, GAP 2; P5's sqlite run 091457Z). A module's
// banked record is its committed proof page, docs/validation/modules/<module>@<version>/<package>.md (index.md for
// the module's root package), in the go2cs checkout the run converts against (-go2cspath). Before this, the rule keyed
// its evidence by the package's path under <root>/core, which a module output is never under, so every module
// host-fatal entry was reported UNCHECKED and no module page was ever read.

// moduleMintFixture: a go2cs checkout with one committed module page, and a module-cache-shaped module directory.
func moduleMintFixture(t *testing.T, pageName, page string) (Options, string, string) {
	t.Helper()
	base := t.TempDir()
	checkout := filepath.Join(base, "go2cs")
	pages := filepath.Join(checkout, "docs", validationDocsDirName, "modules", "example.com", "mod@v1.0.0")

	for _, dir := range []string{filepath.Join(checkout, "src", "core"), pages} {
		if err := os.MkdirAll(dir, 0o755); err != nil {
			t.Fatal(err)
		}
	}

	if err := os.WriteFile(filepath.Join(pages, pageName), []byte(page), 0o644); err != nil {
		t.Fatal(err)
	}

	moduleDir := filepath.Join(base, "modcache", "example.com", "mod@v1.0.0")
	pkgDir := filepath.Join(moduleDir, "sub")
	outputPath := filepath.Join(base, "out", "src", "example.com", "mod", "sub")

	for _, dir := range []string{pkgDir, outputPath} {
		if err := os.MkdirAll(dir, 0o755); err != nil {
			t.Fatal(err)
		}
	}

	options := Options{go2csPath: filepath.Join(checkout, "src"), mainModulePath: "example.com/mod", mainModuleDir: moduleDir,
		targetPlatform: "linux/amd64"}

	return options, pkgDir, outputPath
}

const moduleMintPage = "# `example.com/mod/sub` — validation proof\n\n" +
	"**2 matched · 1 disclosed** — Go 1.26.11, `linux/amd64`, converted package\n`src/example.com/mod/sub`.\n\n" +
	"| Test | `go test` | go2cs |\n|---|---|---|\n" +
	"| `TestMatches` | pass | pass |\n" +
	"| `TestDiverges` | pass | fail ([disclosed](#d)) |\n"

func TestHostFatalMintReadsTheModuleProofPage(t *testing.T) {
	options, pkgDir, outputPath := moduleMintFixture(t, "sub.md", moduleMintPage)

	v, u := hostFatalMintViolationsFor(outputPath, pkgDir, hostFatal("TestMatches"), options)
	if len(v) != 1 || !strings.Contains(v[0], "TestMatches") || len(u) != 0 {
		t.Fatalf("the module's committed page records TestMatches as MATCHING, so the mint must refuse it; got v=%#v u=%#v", v, u)
	}

	if v, u := hostFatalMintViolationsFor(outputPath, pkgDir, hostFatal("TestDiverges"), options); len(v) != 0 || len(u) != 0 {
		t.Fatalf("the page records TestDiverges as diverging: minted and positively cleared; got v=%#v u=%#v", v, u)
	}

	v, u = hostFatalMintViolationsFor(outputPath, pkgDir, hostFatal("TestNotOnThePage"), options)
	if len(v) != 0 || len(u) != 1 || !strings.Contains(u[0], "sub.md") {
		t.Fatalf("a name the module page does not carry is UNCHECKED, naming the page read; got v=%#v u=%#v", v, u)
	}
}

// The module's ROOT package's page is index.md.
func TestHostFatalMintReadsTheModuleRootPackagesIndexPage(t *testing.T) {
	options, _, outputPath := moduleMintFixture(t, "index.md", strings.ReplaceAll(moduleMintPage, "example.com/mod/sub", "example.com/mod"))

	v, _ := hostFatalMintViolationsFor(filepath.Dir(outputPath), options.mainModuleDir, hostFatal("TestMatches"), options)
	if len(v) != 1 || !strings.Contains(v[0], "index.md") {
		t.Fatalf("the module's root package reads index.md; got %#v", v)
	}
}

// With no version (a copy and no -module-version) there is no page to key, and the rule says so rather than
// reporting the generic not-under-core reason.
func TestHostFatalMintModuleWithoutVersionIsUncheckedAndSaysWhy(t *testing.T) {
	options, _, outputPath := moduleMintFixture(t, "sub.md", moduleMintPage)
	copyDir := filepath.Join(t.TempDir(), "mod")
	options.mainModuleDir = copyDir

	v, u := hostFatalMintViolationsFor(outputPath, filepath.Join(copyDir, "sub"), hostFatal("TestMatches"), options)
	if len(v) != 0 || len(u) != 1 || !strings.Contains(u[0], "version") {
		t.Fatalf("no version means no page: unchecked, naming the version as the reason; got v=%#v u=%#v", v, u)
	}

	options.testModuleVersion = "v1.0.0"
	if v, _ := hostFatalMintViolationsFor(outputPath, filepath.Join(copyDir, "sub"), hostFatal("TestMatches"), options); len(v) != 1 {
		t.Fatalf("with -module-version the copy reads the page and refuses; got %#v", v)
	}
}
