// nugetgoSuites_test.go - Gbtc
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
			output, err := nugetgoSuiteCommand(pwsh, suite).CombinedOutput()

			if err != nil {
				t.Error(nugetgoSuiteFailure(name, pwsh, err, string(output), failLine))
			}
		})
	}
}

// nugetgoSuiteCommand is the pwsh launch for one suite.
func nugetgoSuiteCommand(pwsh string, suite string) *exec.Cmd {
	command := exec.Command(pwsh, "-NoProfile", "-NonInteractive", "-File", suite)
	command.Env = append(os.Environ(), "NO_COLOR=1") // plain text: the failure message quotes the output

	return command
}

// nugetgoSuiteFailure is the message for a suite that exited non-zero: its FAIL lines, or the output tail when the
// script threw.
func nugetgoSuiteFailure(name string, pwsh string, err error, output string, failLine *regexp.Regexp) string {
	detail := strings.Join(failLine.FindAllString(output, -1), "\n")

	if detail == "" {
		detail = lastLines(output, 20)
	}

	return fmt.Sprintf("%s failed (%v):\n%s", name, err, detail)
}

// lastLines is the tail of a suite's output, for a failure that printed no FAIL line (a script that threw).
func lastLines(text string, n int) string {
	lines := strings.Split(strings.TrimRight(text, "\r\n"), "\n")

	if len(lines) > n {
		lines = lines[len(lines)-n:]
	}

	return strings.Join(lines, "\n")
}

// THE LAUNCH ENVIRONMENT (TRAIN T3 battery, COORD 2026-10-11). On the battery box, `pwsh` resolved to the .NET
// global-tool shim (~/.dotnet/tools/pwsh, PowerShell 7.4 on .NET 8) while DOTNET_ROOT named a .NET 10-only install.
// The host refused to start it ("framework missing ... framework_version=8.0.0", 0x80008096), and all six subtests
// failed although every suite passes. A pwsh launched for a suite therefore runs with DOTNET_ROLL_FORWARD=Major: a
// framework-dependent pwsh starts on any newer installed runtime. Where the runtime it asks for is installed, Major
// changes nothing. Running is preferred to skipping, which would hide the gate on exactly the box that runs the
// battery.
func TestNugetgoSuiteCommandRollsForwardToANewerRuntime(t *testing.T) {
	t.Setenv("DOTNET_ROLL_FORWARD", "Disable") // a parent setting must not decide it

	command := nugetgoSuiteCommand("pwsh", "Test-Example.ps1")
	value := ""

	for _, entry := range command.Env {
		if name, v, ok := strings.Cut(entry, "="); ok && name == "DOTNET_ROLL_FORWARD" {
			value = v // the last one is what the child sees
		}
	}

	if value != "Major" {
		t.Fatalf("the suite's pwsh runs with DOTNET_ROLL_FORWARD=%q; want Major", value)
	}
}

// A launch failure has to read as an environment problem at a glance, so the message names the pwsh it ran.
func TestNugetgoSuiteFailureNamesThePwshItRan(t *testing.T) {
	const pwsh = "/opt/dotnet-tools/pwsh"

	message := nugetgoSuiteFailure("Test-Example.ps1", pwsh, fmt.Errorf("exit status 150"),
		"You must install or update .NET to run this application.\n", regexp.MustCompile(`(?m)^\s*FAIL\s.*$`))

	if !strings.Contains(message, pwsh) {
		t.Fatalf("the failure message does not name the pwsh it ran (%s):\n%s", pwsh, message)
	}
}
