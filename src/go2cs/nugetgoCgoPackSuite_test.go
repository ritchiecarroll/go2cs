// nugetgoCgoPackSuite_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"archive/zip"
	"os"
	"os/exec"
	"path/filepath"
	"regexp"
	"strings"
	"testing"
)

// THE -Cgo PACK'S SUITE ON A HOSTED RUNNER (cgo against go.*, INC 2b; COORD's ruling on condition 2: "Read both on a
// hosted osx-arm64 leg"). src/tools/nugetgo/Test-NugetgoCgo.ps1 builds a probe pack that never carries the host's RID
// and reads the RID guard and the reference-only lib/ against it, the host standing in for an unpacked RID: a consumer
// build fails with NUGETGOCGO001, and a portable build made for a packed host fails at load there. Until the cgo arm
// (2026-11-28) a -Cgo pack left osx-arm64 out by name, so on that leg the arms WERE condition 2. The converter-cgo stage runs this package's tests only, on every runner of a GOOS (macos-15 is
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

// A NON-CGO RID-SPECIFIC MODULE, PACKED END TO END (P3's INC 2b read, MUST 1). The -Cgo seat once put its block between
// the pack's layout `if` and its `elseif`, and the pack then refused every non-cgo RID-specific package ("a neutral
// package carries runtimes/ entries"): mousetrap v1.1.0's pack, measured at 326a7e7932, exited 1 there and 0 at the seat's
// base. No suite ran a whole pack, so this row does: the repack driver's rehearsal pack of mousetrap v1.1.0 (no Go tests;
// its trap_windows.go and trap_others.go are why its flavors differ), from this tree, against the published go.*
// release. It needs go, dotnet, pwsh, the network and a few GB, so it runs where GO2CS_TESTS_E2E=1 (the cgo-e2e stage,
// filtered to it).
func TestNugetgoPackOfANonCgoRidSpecificModuleEndToEnd(t *testing.T) {
	if os.Getenv(testsE2EEnv) != "1" {
		t.Skip("opt-in on GO2CS_TESTS_E2E=1 (the cgo-e2e stage, filter ^TestNugetgoPackOfANonCgoRidSpecificModuleEndToEnd$): a whole pack against the published go.* release")
	}

	pwsh, err := exec.LookPath("pwsh")

	if err != nil {
		t.Fatal("pwsh is not on PATH, and GO2CS_TESTS_E2E=1 requires the pack to run")
	}

	tree, err := filepath.Abs(filepath.Join("..", ".."))

	if err != nil {
		t.Fatal(err)
	}

	release, err := exec.Command("go", "run", ".", "-version").Output()

	if err != nil {
		t.Fatalf("go2cs -version: %v", err)
	}

	version := ""

	for _, line := range strings.Split(string(release), "\n") {
		if value, found := strings.CutPrefix(strings.TrimSpace(line), "go2cs.release="); found {
			version = value
		}
	}

	if version == "" {
		t.Fatalf("go2cs -version names no release:\n%s", release)
	}

	work := filepath.Join(t.TempDir(), "work")
	driver := filepath.Join(tree, "src", "tools", "nugetgo", "Invoke-NugetgoRepack.ps1")
	command := exec.Command(pwsh, "-NoProfile", "-NonInteractive", "-File", driver, "-Module", "github.com/inconshreveable/mousetrap@v1.1.0",
		"-Revision", "2", "-T", tree, "-Release", version, "-Work", work, "-Packs", "int1", "-FloorGB", "4")
	command.Env = append(os.Environ(), "NO_COLOR=1", "DOTNET_ROLL_FORWARD=Major")
	output, err := command.CombinedOutput()

	if err != nil {
		t.Fatalf("the rehearsal pack of mousetrap v1.1.0 against go.* %s failed (%v):\n%s", version, err, lastLines(string(output), 30))
	}

	packages, _ := filepath.Glob(filepath.Join(work, "kit", "nupkg", "*", "*.nupkg"))

	if len(packages) == 0 {
		packages, _ = filepath.Glob(filepath.Join(work, "out-int1", "*.nupkg"))
	}

	if len(packages) != 1 {
		t.Fatalf("want one mousetrap package under %s, found %v", work, packages)
	}

	archive, err := zip.OpenReader(packages[0])

	if err != nil {
		t.Fatal(err)
	}

	defer archive.Close()

	entries := map[string]bool{}

	for _, f := range archive.File {
		entries[f.Name] = true
	}

	const assembly = "github.com.inconshreveable.mousetrap.dll"

	for _, want := range []string{"lib/net10.0/" + assembly, "runtimes/win-x64/lib/net10.0/" + assembly, "runtimes/linux-x64/lib/net10.0/" + assembly,
		"runtimes/osx-x64/lib/net10.0/" + assembly, "runtimes/osx-arm64/lib/net10.0/" + assembly, "runtimes/linux-arm64/lib/net10.0/" + assembly} {
		if !entries[want] {
			t.Errorf("the package lacks %s", want)
		}
	}

	t.Logf("READING %s: %d entries, RID-specific, packed against go.* %s", filepath.Base(packages[0]), len(entries), version)
}
