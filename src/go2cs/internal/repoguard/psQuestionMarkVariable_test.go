// psQuestionMarkVariable_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package repoguard

import (
	"fmt"
	"os"
	"path/filepath"
	"regexp"
	"strings"
	"testing"
)

// THE QUESTION-MARK VARIABLE GUARD.
//
// PowerShell admits `?` as a VARIABLE-NAME character, so `"... as $ver? (type 'publish' ...)"` does
// not interpolate $ver followed by a question mark: it interpolates a variable named `ver?`, which
// nothing assigns, and prints nothing. Nothing goes red -- the string is well formed, strict mode is
// off, and the line reads correctly to a human.
//
// ⚠ WHAT IT COST, measured 2026-10-06 on the 1.24.13.4 release run on the i7: release-nuget.ps1's
// Phase 3 asked "Publish 344 package(s) as  (type 'publish' to proceed)" -- the one prompt that
// prints what is about to become permanent on nuget.org printed no version. Reproduced under pwsh
// 7.6: `$ver="1.24.13.5"; "as $ver? (x)"` -> "as  (x)"; `"as ${ver}? (x)"` -> "as 1.24.13.5? (x)".
//
// THE RULE: no tracked PowerShell file writes `$name?` (or a scoped `$env:name?`); a variable that
// meets a `?` is braced, `${name}?`. That is also PowerShell 7's own spelling for the null-conditional
// operators (`${a}?.b`), so there is no legitimate unbraced form to admit. Line comments are skipped
// (prose may quote the defect); a backtick-escaped `$` is not a variable.
//
// POSITIVE CONTROL: restoring `$ver?` in release-nuget.ps1's publish prompt makes BOTH tests below
// name src/release-nuget.ps1 and its line. Verified at the commit that landed this guard, then
// restored byte-identical. TestQuestionMarkVariableScannerFires controls the scanner without the tree.

var psQuestionMarkVariable = regexp.MustCompile(`\$(?:[A-Za-z]+:)?[A-Za-z_][A-Za-z0-9_]*\?`)

// questionMarkVariables returns "line: text" for every unbraced `$name?` in a PowerShell source.
func questionMarkVariables(content string) []string {
	var hits []string

	for i, line := range strings.Split(content, "\n") {
		if strings.HasPrefix(strings.TrimSpace(line), "#") {
			continue
		}

		for _, at := range psQuestionMarkVariable.FindAllStringIndex(line, -1) {
			if at[0] > 0 && line[at[0]-1] == '`' {
				continue
			}

			hits = append(hits, fmt.Sprintf("%d: %s", i+1, strings.TrimSpace(line)))
		}
	}

	return hits
}

func TestNoQuestionMarkVariableInPowerShell(t *testing.T) {
	root := repoRootFromPackageDir(t)
	tracked := gitTrackedFiles(t, root)

	scanned := 0
	var problems []string

	for _, rel := range tracked {
		ext := strings.ToLower(filepath.Ext(rel))

		if ext != ".ps1" && ext != ".psm1" {
			continue
		}

		data, err := os.ReadFile(filepath.Join(root, filepath.FromSlash(rel)))

		if err != nil {
			t.Fatalf("cannot read %s: %v", rel, err)
		}

		scanned++

		for _, hit := range questionMarkVariables(string(data)) {
			problems = append(problems, rel+":"+hit)
		}
	}

	if scanned < 20 {
		t.Fatalf("VACUOUS: %d PowerShell files scanned; a guard that scans nothing passes everything", scanned)
	}

	t.Logf("PowerShell files scanned %d · unbraced $name? %d", scanned, len(problems))

	if len(problems) > 0 {
		t.Fatalf("PowerShell reads `$name?` as a variable named `name?`, which interpolates EMPTY; brace it as `${name}?`:\n  %s",
			strings.Join(problems, "\n  "))
	}
}

// TestReleasePublishPromptCarriesTheVersion holds the one prompt that caused the guard above: the
// Phase 3 confirmation must interpolate the version it is about to publish.
func TestReleasePublishPromptCarriesTheVersion(t *testing.T) {
	content := readRepoFile(t, "src/release-nuget.ps1")

	var prompts []string

	for i, line := range strings.Split(content, "\n") {
		if strings.Contains(line, "Read-Host") && strings.Contains(line, "to proceed") {
			prompts = append(prompts, fmt.Sprintf("%d: %s", i+1, strings.TrimSpace(line)))
		}
	}

	if len(prompts) != 1 {
		t.Fatalf("expected exactly ONE publish confirmation (Read-Host ... to proceed) in src/release-nuget.ps1, found %d:\n  %s",
			len(prompts), strings.Join(prompts, "\n  "))
	}

	if !strings.Contains(prompts[0], "${ver}") || len(questionMarkVariables(prompts[0])) > 0 {
		t.Fatalf("src/release-nuget.ps1:%s\n  the publish prompt must interpolate the version as ${ver}; unbraced before a `?` it prints nothing", prompts[0])
	}
}

func TestQuestionMarkVariableScannerFires(t *testing.T) {
	cases := []struct {
		line string
		hits int
	}{
		{`$answer = Read-Host "  Publish $($packages.Count) package(s) as $ver? (type 'publish' to proceed)"`, 1},
		{`Write-Host "path $env:GOROOT? and $x?"`, 2},
		{`$answer = Read-Host "  Publish $($packages.Count) package(s) as ${ver}? (type 'publish' to proceed)"`, 0},
		{"Write-Host \"literal `$ver? stays literal\"", 0},
		{`    # prose may quote the defect: "as $ver? (type ...)"`, 0},
		{`if ($a -and $b) { "is it $ok" }`, 0},
	}

	for _, c := range cases {
		if got := questionMarkVariables(c.line); len(got) != c.hits {
			t.Errorf("questionMarkVariables(%q) = %d hit(s) %v, want %d", c.line, len(got), got, c.hits)
		}
	}
}
