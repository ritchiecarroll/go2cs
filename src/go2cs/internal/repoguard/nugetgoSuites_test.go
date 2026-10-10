// nugetgoSuites_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package repoguard

import (
	"os"
	"os/exec"
	"path/filepath"
	"regexp"
	"strings"
	"testing"
)

// THE NUGETGO SUITES GUARD.
//
// src/tools/nugetgo carries table-driven PowerShell suites (Test-*.ps1) beside the module pack they test, and until
// this guard nothing ran them: no lane, no train gate. A pack defect they would name could reach a release with every
// gate green (the nugetgo rehearsal, 2026-10-09). This runs every suite under the plain `go test ./...` each lane
// already runs. Each suite's exit code is its number of failed arms, so a non-zero exit fails the guard, which names
// the suite and quotes its FAIL lines.
//
// The suites are found by glob, so a new Test-*.ps1 is covered the day it lands. They need pwsh (PowerShell 7: the
// host-path suite reads portable pdbs through System.Reflection.Metadata, which Windows PowerShell 5.1 does not ship).
// A host with no pwsh on PATH skips, and says so by name.
func TestNugetgoSuitesPass(t *testing.T) {
	pwsh, err := exec.LookPath("pwsh")
	if err != nil {
		t.Skip("pwsh is not on PATH: the nugetgo Test-*.ps1 suites did NOT run on this host")
	}

	suites, err := filepath.Glob(filepath.Join(repoRootFromPackageDir(t), "src", "tools", "nugetgo", "Test-*.ps1"))
	if err != nil {
		t.Fatal(err)
	}

	if len(suites) == 0 {
		t.Fatal("no src/tools/nugetgo/Test-*.ps1 suite found: the glob or the tree moved")
	}

	failLine := regexp.MustCompile(`(?m)^\s*FAIL\s.*$`)

	for _, suite := range suites {
		name := filepath.Base(suite)

		t.Run(strings.TrimSuffix(name, ".ps1"), func(t *testing.T) {
			command := exec.Command(pwsh, "-NoProfile", "-NonInteractive", "-File", suite)
			command.Env = append(os.Environ(), "NO_COLOR=1") // plain text: the failure message quotes the output
			output, err := command.CombinedOutput()

			if err != nil {
				fails := failLine.FindAllString(string(output), -1)
				detail := strings.Join(fails, "\n")

				if detail == "" {
					detail = lastLines(string(output), 20)
				}

				t.Errorf("%s failed (%v):\n%s", name, err, detail)
			}
		})
	}
}

// lastLines is the tail of a suite's output, for a failure that printed no FAIL line (a script that threw).
func lastLines(text string, n int) string {
	lines := strings.Split(strings.TrimRight(text, "\r\n"), "\n")

	if len(lines) > n {
		lines = lines[len(lines)-n:]
	}

	return strings.Join(lines, "\n")
}
