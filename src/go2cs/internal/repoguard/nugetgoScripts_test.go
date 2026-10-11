// nugetgoScripts_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package repoguard

import (
	"bytes"
	"os"
	"path/filepath"
	"strings"
	"testing"
)

// THE NUGETGO SCRIPTS UNDER WINDOWS POWERSHELL 5.1 (TRAIN T3 battery, COORD 2026-10-11).
//
// Windows PowerShell 5.1 reads a script without a byte-order mark in the ANSI code page, not as UTF-8. An em dash is
// UTF-8 E2 80 94, and in Windows-1252 the byte 0x94 is a right double quotation mark, which PowerShell treats as a
// string delimiter. NugetgoReadme.psm1 therefore had 8 parse errors under 5.1 (the first at line 134, "Missing ')' in
// method call"), and Test-NugetgoReadme.ps1 had 6, while PowerShell 7 parsed both clean. A script under
// src/tools/nugetgo holds ASCII only (a non-ASCII character is written as [char]0x2014 and the like) or starts with a
// UTF-8 byte-order mark.
func TestNugetgoScriptsAreASCIIOrCarryABOM(t *testing.T) {
	for _, path := range nugetgoScripts(t) {
		contents, err := os.ReadFile(path)
		if err != nil {
			t.Fatal(err)
		}

		if bytes.HasPrefix(contents, []byte{0xEF, 0xBB, 0xBF}) {
			continue
		}

		for i, line := range strings.Split(string(contents), "\n") {
			for _, b := range []byte(line) {
				if b >= 0x80 {
					t.Errorf("%s:%d holds a non-ASCII character and the file has no UTF-8 byte-order mark: Windows PowerShell 5.1 reads it in the ANSI code page", filepath.Base(path), i+1)
					break
				}
			}
		}
	}
}

// Every nugetgo suite ends with the summary line the battery reads, "ran N, failed M". Test-NugetgoReadme.ps1 printed
// "103/103 passed", which the battery could not read (TRAIN T3, NGa7:readme).
func TestNugetgoSuitesPrintTheStandardSummaryLine(t *testing.T) {
	const summary = `Write-Host "ran $ran, failed $failed"`

	suites := 0

	for _, path := range nugetgoScripts(t) {
		if !strings.HasPrefix(filepath.Base(path), "Test-") || filepath.Ext(path) != ".ps1" {
			continue
		}

		suites++

		contents, err := os.ReadFile(path)
		if err != nil {
			t.Fatal(err)
		}

		if !strings.Contains(string(contents), summary) {
			t.Errorf("%s does not print the standard summary line %s", filepath.Base(path), summary)
		}
	}

	if suites == 0 {
		t.Fatal("no Test-*.ps1 suite found under src/tools: the glob or the tree moved")
	}
}

// nugetgoScripts lists the PowerShell scripts and modules under each of toolSuiteDirs (src/tools/nugetgo and
// src/tools/pushnuget).
func nugetgoScripts(t *testing.T) []string {
	t.Helper()

	var scripts []string

	for _, name := range toolSuiteDirs {
		dir := filepath.Join(repoRootFromPackageDir(t), "src", "tools", name)
		found := 0

		for _, pattern := range []string{"*.ps1", "*.psm1", "testdata/*.ps1"} {
			matches, err := filepath.Glob(filepath.Join(dir, pattern))
			if err != nil {
				t.Fatal(err)
			}

			found += len(matches)
			scripts = append(scripts, matches...)
		}

		if found == 0 {
			t.Fatalf("no script under src/tools/%s: the tree moved", name)
		}
	}

	return scripts
}
