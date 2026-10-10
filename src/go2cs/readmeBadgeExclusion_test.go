// readmeBadgeExclusion_test.go - Gbtc
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
	"regexp"
	"strings"
	"testing"
)

// The Tests badge honors the roster's exclusion ledger (TRAIN T3 fixup, COORD 2026-10-10). The badge went
// green on two signals only, a committed test project and a proof page, and an EXCLUSION row keeps both on
// purpose: the page is the evidence behind the exclusion (writeValidationIndex leaves it out of the index for
// that reason). So crypto/internal/fips140deps, excluded by ruling as E4, emitted "1 matched / 0 disclosed"
// while the committed README, as check-roster-format requires of an exclusion row, read "not yet validated".
// Every -stdlib regeneration rewrote that README green, on all three targets.

// The rule: a roster-excluded package never gets the green badge, whatever its artifacts say. It falls through
// to the honest has-tests classification, which is what the committed README states. The control in the same
// roster, a package the ledger does not name, stays green.
func TestValidationBadgeNotGreenOnARosterExcludedRow(t *testing.T) {
	const dotID = "crypto.internal.fips140deps"

	root, projectPath := badgeTree(t, dotID, "1.24.13.5")
	writeBadgeRoster(t, root, "crypto/internal/fips140deps")
	addProofPage(t, root, dotID, 1, 0)
	mustWriteFile(t, filepath.Join(projectPath, dotID+testProjectFileSuffix), "<Project />")

	sourceDir := addGoSources(t, map[string]string{
		"fipsdeps.go":      "package fipsdeps\n",
		"fipsdeps_test.go": "package fipsdeps\n\nimport \"testing\"\n\nfunc TestImports(t *testing.T) {}\n",
	})

	const want = "[![Tests](https://img.shields.io/badge/Tests-not_yet_validated-orange?logo=go)](https://go2cs.net/ValidatedTestPackages.html)"

	if got := readmeValidationBadgeLine(projectPath, dotID, sourceDir); got != want {
		t.Fatalf("a roster-excluded package's badge\n got: %s\nwant: %s", got, want)
	}

	controlPath := filepath.Join(root, "core", "io")
	mustMkdirAll(t, controlPath)
	addProofPage(t, root, "io", 59, 2)
	mustWriteFile(t, filepath.Join(controlPath, "io"+testProjectFileSuffix), "<Project />")

	if got := readmeValidationBadgeLine(controlPath, "io", sourceDir); !strings.Contains(got, "Tests-59_matched_%2F_2_disclosed-brightgreen") {
		t.Fatalf("control: a package the ledger does not name lost its green badge: %s", got)
	}
}

// The ledger is a REPOSITORY input of the Tests badge, like version.props and docs/validation, and the badge
// takes the same fallback when it cannot be read: no Tests badge at all. A permissive fallback would emit
// green on an excluded row whenever the roster was missing from a seeded root, which is the defect itself.
// An unreadable ledger (no heading, or two) is refused the same way, as Get-ExclusionLedgerRows refuses it.
func TestValidationBadgeOmittedWithoutAReadableExclusionLedger(t *testing.T) {
	sourceDir := addGoSources(t, map[string]string{
		"a.go":      "package a\n",
		"a_test.go": "package a\n\nimport \"testing\"\n\nfunc TestA(t *testing.T) {}\n",
	})

	for _, c := range []struct {
		name   string
		roster string // "" removes the roster
	}{
		{"no roster", ""},
		{"no Excluded packages heading", "# Validated Test Packages\n\n| Package | Verdicts |\n"},
		{"two Excluded packages headings", "## Excluded packages\n\n## Excluded packages\n"},
	} {
		t.Run(c.name, func(t *testing.T) {
			root, projectPath := badgeTree(t, "io", "1.24.13.5")
			addProofPage(t, root, "io", 59, 2)
			mustWriteFile(t, filepath.Join(projectPath, "io"+testProjectFileSuffix), "<Project />")

			rosterPath := filepath.Join(filepath.Dir(root), "docs", validationRosterFileName)

			if c.roster == "" {
				if err := os.Remove(rosterPath); err != nil {
					t.Fatal(err)
				}
			} else {
				mustWriteFile(t, rosterPath, c.roster)
			}

			if badge := readmeValidationBadgeLine(projectPath, "io", sourceDir); badge != "" {
				t.Fatalf("expected no Tests badge without a readable exclusion ledger, got: %s", badge)
			}
		})
	}
}

// THE COMMITTED TREE. Every package the committed roster's ledger names, and that has a committed README,
// composes no green badge with this converter, against the real repository: its real version.props, proof
// pages, test projects and roster. This is the guard that keeps the converter and the roster from disagreeing
// again, whichever side moves. It is not vacuous: at least one ledger row has a README
// (crypto/internal/fips140deps when this was written).
func TestCommittedExclusionRowsComposeNoGreenBadge(t *testing.T) {
	repo := repoRootFromPackageDir(t)
	excluded, err := readRosterExclusions(filepath.Join(repo, "docs"))

	if err != nil {
		t.Fatalf("read the committed exclusion ledger: %v", err)
	}

	withReadme := 0

	for importPath := range excluded {
		projectPath := filepath.Join(repo, "src", "core", filepath.FromSlash(importPath))

		if _, err := os.Stat(filepath.Join(projectPath, "README.md")); err != nil {
			continue
		}

		withReadme++

		if badge := readmeValidationBadgeLine(projectPath, validationProofDotID(importPath), ""); strings.Contains(badge, "brightgreen") {
			t.Errorf("the roster excludes %s, and the converter composes a green badge for it: %s", importPath, badge)
		}
	}

	if withReadme == 0 {
		t.Fatalf("no exclusion row of the %d in the committed ledger has a README; this guard checks nothing", len(excluded))
	}
}

// The Go reader and src/_roster.ps1 read one ledger, so they hold one pair of patterns. On the committed
// roster the ledger is non-empty and shares no package with the banked table.
func TestExclusionLedgerPatternsAgree(t *testing.T) {
	repo := repoRootFromPackageDir(t)
	data, err := os.ReadFile(filepath.Join(repo, "src", "_roster.ps1"))

	if err != nil {
		t.Fatal(err)
	}

	for _, c := range []struct {
		variable string
		goSide   *regexp.Regexp
	}{
		{"ExclusionLedgerRowPattern", exclusionLedgerRowPattern},
		{"ExclusionLedgerHeadingPattern", exclusionLedgerHeadingPattern},
	} {
		match := regexp.MustCompile(`(?m)^\$` + c.variable + ` = '([^']+)'`).FindSubmatch(data)

		if match == nil {
			t.Fatalf("src/_roster.ps1 no longer declares $%s where this guard looks", c.variable)
		}

		if string(match[1]) != c.goSide.String() {
			t.Errorf("$%s has drifted from the Go reader:\n  go:  %s\n  ps1: %s", c.variable, c.goSide.String(), match[1])
		}
	}

	excluded, err := readRosterExclusions(filepath.Join(repo, "docs"))

	if err != nil {
		t.Fatal(err)
	}

	if len(excluded) == 0 {
		t.Fatal("the committed exclusion ledger reads as empty")
	}

	banked, _, err := readValidationRoster(filepath.Join(repo, "docs"))

	if err != nil {
		t.Fatal(err)
	}

	for _, importPath := range banked {
		if excluded[importPath] {
			t.Errorf("%s is both banked and excluded", importPath)
		}
	}
}
