// nugetgoCgoPackSuite_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"os"
	"os/exec"
	"path/filepath"
	"regexp"
	"strings"
	"testing"
)

// THE -Cgo PACK'S SUITE ON A HOSTED RUNNER (cgo against go.*, INC 2b; COORD's ruling on condition 2: "Read both on a
// hosted osx-arm64 leg"). src/tools/nugetgo/Test-NugetgoCgo.ps1 builds a probe pack that never carries the host's RID
// and reads the RID guard and the reference-only lib/ against it. On osx-arm64, which a -Cgo pack leaves out by name,
// its arms ARE condition 2: a consumer build fails with NUGETGOCGO001, and a portable build made for a packed host fails
// at load there. The converter-cgo stage runs this package's tests only, on every runner of a GOOS (macos-15 is
// osx-arm64), so the suite is run from here on a leg that sets GO2CS_REQUIRE_DOTNET=1. Everywhere else the plain
// `go test ./...` already runs it, with every nugetgo suite (internal/repoguard TestNugetgoSuitesPass).
func TestNugetgoCgoPackSuiteOnThisHost(t *testing.T) {
	if os.Getenv("GO2CS_REQUIRE_DOTNET") != "1" {
		t.Skip("run on a leg that sets GO2CS_REQUIRE_DOTNET=1 (converter-cgo); elsewhere internal/repoguard's TestNugetgoSuitesPass runs Test-NugetgoCgo.ps1")
	}

	pwsh, err := exec.LookPath("pwsh")

	if err != nil {
		t.Fatal("pwsh is not on PATH, and GO2CS_REQUIRE_DOTNET=1 requires the suite to run")
	}

	suite, err := filepath.Abs(filepath.Join("..", "tools", "nugetgo", "Test-NugetgoCgo.ps1"))

	if err != nil {
		t.Fatal(err)
	}

	command := exec.Command(pwsh, "-NoProfile", "-NonInteractive", "-File", suite)
	command.Env = append(os.Environ(), "NO_COLOR=1", "DOTNET_ROLL_FORWARD=Major")
	output, err := command.CombinedOutput()

	// Every arm by name, so the hosted summary reads which ran (its READING lines).
	for _, line := range strings.Split(string(output), "\n") {
		if trimmed := strings.TrimSpace(line); strings.HasPrefix(trimmed, "ok ") || strings.HasPrefix(trimmed, "FAIL ") || strings.HasPrefix(trimmed, "host ") || strings.HasPrefix(trimmed, "ran ") {
			t.Logf("READING %s", trimmed)
		}
	}

	if err != nil {
		fails := strings.Join(regexp.MustCompile(`(?m)^\s*FAIL\s.*$`).FindAllString(string(output), -1), "\n")

		if fails == "" {
			fails = string(output)
		}

		t.Fatalf("Test-NugetgoCgo.ps1 failed (%v):\n%s", err, fails)
	}
}
