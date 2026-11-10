// testsCorpusArch_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"context"
	"fmt"
	"os"
	"os/exec"
	"path/filepath"
	"regexp"
	"slices"
	"strings"
	"testing"
	"time"
)

// A -tests run of a standard-library package re-emits that package into the COMMITTED corpus, which is built
// for amd64 on every GOOS (and, on linux alone, for arm64 too). On an arm64 host the -platforms default (the
// converter binary's own runtime.GOOS/runtime.GOARCH) re-emitted it for darwin/arm64 instead: hash/crc32 then
// selected crc32_arm64.go beside the committed crc32_amd64.cs and its test host failed to build, CS0111 x6
// (darwin row pilot, run 37112360853, 2026-10-03). These pin the decision that replaces that default.
func TestTestsTargetPlatformPinsTheCorpusArchForGOROOTPackages(t *testing.T) {
	goRoot := filepath.Join(t.TempDir(), "go")
	stdlibPackage := filepath.Join(goRoot, "src", "hash", "crc32")
	moduleDir := filepath.Join(t.TempDir(), "mod", "example.com", "thing")

	cases := []struct {
		name         string
		hostDefault  string
		explicit     bool
		convertTests bool
		inputDir     string
		want         string
		wantPinned   bool
	}{
		{"stdlib on darwin/arm64 pins amd64", "darwin/arm64", false, true, stdlibPackage, "darwin/amd64", true},
		{"stdlib on linux/arm64 is its own flavour", "linux/arm64", false, true, stdlibPackage, "linux/arm64", false},
		{"stdlib on darwin/amd64 is unchanged", "darwin/amd64", false, true, stdlibPackage, "darwin/amd64", false},
		{"stdlib on windows/amd64 is unchanged", "windows/amd64", false, true, stdlibPackage, "windows/amd64", false},
		{"stdlib on linux/amd64 is unchanged", "linux/amd64", false, true, stdlibPackage, "linux/amd64", false},
		{"a module on darwin/arm64 keeps the host arch", "darwin/arm64", false, true, moduleDir, "darwin/arm64", false},
		{"an explicit -platforms always wins", "darwin/arm64", true, true, stdlibPackage, "darwin/arm64", false},
		{"a conversion without -tests is not pinned", "darwin/arm64", false, false, stdlibPackage, "darwin/arm64", false},
		{"GOROOT itself, not its src, is not a package", "darwin/arm64", false, true, goRoot, "darwin/arm64", false},
	}

	for _, c := range cases {
		got, pinned := testsTargetPlatform(c.hostDefault, c.explicit, c.convertTests, c.inputDir, goRoot)

		if got != c.want || pinned != c.wantPinned {
			t.Errorf("%s: testsTargetPlatform(%q, explicit=%v, tests=%v, %q) = (%q, %v), want (%q, %v)",
				c.name, c.hostDefault, c.explicit, c.convertTests, c.inputDir, got, pinned, c.want, c.wantPinned)
		}
	}
}

// The pin is the host default's corpus flavour (corpusFlavourOf), so it follows the universe: once the universe carries
// linux/arm64, a linux/arm64 host is its own flavour and is not pinned, while darwin/arm64 still converts at darwin's
// one flavour. A host GOOS no flavour serves keeps its default at either universe; until 2026-11-09 it was pinned to
// <goos>/amd64, a corpus nobody converts (S1b).
func TestTestsTargetPlatformFollowsTheUniverse(t *testing.T) {
	goRoot := filepath.Join(t.TempDir(), "go")
	stdlibPackage := filepath.Join(goRoot, "src", "hash", "crc32")

	cases := []struct {
		universe    []string
		hostDefault string
		want        string
		wantPinned  bool
	}{
		{universeArm, "linux/arm64", "linux/arm64", false},
		{universeArm, "darwin/arm64", "darwin/amd64", true},
		{universeArm, "linux/amd64", "linux/amd64", false},
		{universeArm, "freebsd/arm64", "freebsd/arm64", false},
		{universeToday, "freebsd/arm64", "freebsd/arm64", false},
		{universeToday, "freebsd/amd64", "freebsd/amd64", false},
	}

	for _, c := range cases {
		got, pinned := testsTargetPlatformOver(mustPlatformLattice(c.universe), c.hostDefault, false, true, stdlibPackage, goRoot)

		if got != c.want || pinned != c.wantPinned {
			t.Errorf("over %v, testsTargetPlatform(%q) = (%q, %v), want (%q, %v)", c.universe, c.hostDefault, got, pinned, c.want, c.wantPinned)
		}
	}
}

// One stand-in rule for every reader: a target of the universe is its own flavour, any other target the first
// universe target of its GOOS, and a GOOS with no universe target (or a value that is not os/arch) is unchanged.
func TestCorpusFlavourOf(t *testing.T) {
	cases := []struct {
		universe []string
		target   string
		want     string
	}{
		{universeToday, "windows/amd64", "windows/amd64"},
		{universeToday, "linux/amd64", "linux/amd64"},
		{universeToday, "darwin/amd64", "darwin/amd64"},
		{universeToday, "darwin/arm64", "darwin/amd64"},
		{universeToday, "linux/arm64", "linux/amd64"},
		{universeToday, "windows/arm64", "windows/amd64"},
		{universeToday, "freebsd/arm64", "freebsd/arm64"},
		{universeToday, "linux", "linux"},
		{universeToday, "", ""},
		{universeArm, "linux/arm64", "linux/arm64"},
		{universeArm, "linux/riscv64", "linux/amd64"},
		{universeArm, "darwin/arm64", "darwin/amd64"},
		{universeFive, "darwin/arm64", "darwin/arm64"},
	}

	for _, c := range cases {
		if got := corpusFlavourOf(mustPlatformLattice(c.universe), c.target); got != c.want {
			t.Errorf("over %v, corpusFlavourOf(%q) = %q, want %q", c.universe, c.target, got, c.want)
		}
	}
}

// pathsScript is src/_paths.ps1, the harness's one copy of the corpus universe and its flavour rule.
var pathsScript = filepath.Join("..", "_paths.ps1")

// The harness restates platformUniverse as $CorpusPlatformUniverse, so a target joins the harness and the converter
// together or neither reads it: the literal must equal the constant element for element, in order. Like
// archExclusive_test.go's reads, this file lies outside the module root, so a change to it re-runs this only under
// -count=1.
func TestPathsCorpusPlatformUniverseMirrorsTheConverter(t *testing.T) {
	data, err := os.ReadFile(pathsScript)

	if err != nil {
		t.Fatalf("cannot read %s: %v", pathsScript, err)
	}

	declaration := regexp.MustCompile(`(?m)^\$CorpusPlatformUniverse = @\(([^)]*)\)\r?$`).FindAllSubmatch(data, -1)

	if len(declaration) != 1 {
		t.Fatalf("%s declares $CorpusPlatformUniverse = @(...) %d times on a line of its own; want exactly once", pathsScript, len(declaration))
	}

	var literal []string

	for _, quoted := range regexp.MustCompile(`'([^']*)'`).FindAllSubmatch(declaration[0][1], -1) {
		literal = append(literal, string(quoted[1]))
	}

	if !slices.Equal(literal, platformUniverse) {
		t.Errorf("%s's $CorpusPlatformUniverse = %q, platformUniverse = %q; they change together", pathsScript, literal, platformUniverse)
	}
}

// The harness's Get-CorpusFlavorArch is corpusFlavourOf restated, so the two are run against each other: ONE PowerShell
// process dot-sources src/_paths.ps1 and src/_roster.ps1 and answers a table over today's universe and one carrying
// linux/arm64, and each answer must be the Go helper's flavour arch ($null where the Go helper leaves a GOOS with no
// flavour unchanged). The same process reads Get-SweepTargetGoarch with GoTargetArch set in its environment (which the
// _paths.ps1 pin must not overwrite) and then unset. pwsh is tried first and Windows PowerShell 5.1 after it; the
// edition that ran is logged, and a host with neither skips by name.
func TestPathsCorpusFlavorArchMatchesTheConverter(t *testing.T) {
	paths, err := filepath.Abs(pathsScript)

	if err != nil {
		t.Fatal(err)
	}

	roster := filepath.Join(filepath.Dir(paths), "_roster.ps1")
	quote := func(value string) string { return "'" + strings.ReplaceAll(value, "'", "''") + "'" }
	psArray := func(values []string) string {
		quoted := make([]string, 0, len(values))

		for _, value := range values {
			quoted = append(quoted, quote(value))
		}

		return "@(" + strings.Join(quoted, ",") + ")"
	}

	universes := map[string][]string{"u3": universeToday, "u4": universeArm}
	pairs := [][2]string{
		{"windows", "amd64"}, {"windows", "arm64"}, {"linux", "amd64"}, {"linux", "arm64"}, {"linux", "riscv64"},
		{"darwin", "amd64"}, {"darwin", "arm64"}, {"freebsd", "arm64"}, {"plan9", "amd64"},
	}

	// Single quotes only, and one line: the script is one -Command argument on every edition and platform.
	statements := []string{
		"'edition=' + $PSVersionTable.PSEdition + ' ' + $PSVersionTable.PSVersion",
		"$ErrorActionPreference = 'Stop'",
		". " + quote(paths),
		". " + quote(roster),
		"function Show-FlavorArch($value) { if ($null -eq $value) { '<null>' } else { $value } }",
		"'preset=' + $env:GoTargetArch",
		"'sweep-set=' + (Get-SweepTargetGoarch)",
		"Remove-Item Env:GoTargetArch",
		"'sweep-unset=' + (Get-SweepTargetGoarch)",
	}

	want := map[string]string{"preset": "arm64", "sweep-set": "arm64", "sweep-unset": "amd64"}

	expect := func(lattice *platformLattice, goos, goarch string) string {
		if flavour := corpusFlavourOf(lattice, goos+"/"+goarch); lattice.includes(flavour) {
			return archOfTarget(flavour)
		}

		return "<null>"
	}

	for _, name := range []string{"u3", "u4"} {
		for _, pair := range pairs {
			key := fmt.Sprintf("%s|%s|%s", name, pair[0], pair[1])
			statements = append(statements, fmt.Sprintf("'%s=' + (Show-FlavorArch (Get-CorpusFlavorArch -Goos %s -Goarch %s -Universe %s))",
				key, quote(pair[0]), quote(pair[1]), psArray(universes[name])))
			want[key] = expect(mustPlatformLattice(universes[name]), pair[0], pair[1])
		}
	}

	for _, pair := range pairs {
		key := fmt.Sprintf("default|%s|%s", pair[0], pair[1])
		statements = append(statements, fmt.Sprintf("'%s=' + (Show-FlavorArch (Get-CorpusFlavorArch -Goos %s -Goarch %s))", key, quote(pair[0]), quote(pair[1])))
		want[key] = expect(corpusLattice(), pair[0], pair[1])
	}

	script := strings.Join(statements, "; ")

	var env []string

	for _, entry := range os.Environ() {
		if name, _, _ := strings.Cut(entry, "="); !strings.EqualFold(name, "GoTargetArch") {
			env = append(env, entry)
		}
	}

	env = append(env, "GoTargetArch=arm64", "NO_COLOR=1", "DOTNET_ROLL_FORWARD=Major")

	var output string
	var ran string

	for _, shell := range []string{"pwsh", "powershell"} {
		path, err := exec.LookPath(shell)

		if err != nil {
			continue
		}

		ctx, cancel := context.WithTimeout(context.Background(), 2*time.Minute)
		command := exec.CommandContext(ctx, path, "-NoProfile", "-NonInteractive", "-ExecutionPolicy", "Bypass", "-Command", script)
		command.Env = env
		combined, runErr := command.CombinedOutput()
		cancel()

		if !strings.Contains(string(combined), "edition=") {
			// It did not start (a broken pwsh shim exits before running anything): try the next shell.
			t.Logf("%s did not start (%v): %s", path, runErr, strings.TrimSpace(string(combined)))
			continue
		}

		if runErr != nil {
			t.Fatalf("%s ran the parity script and failed (%v):\n%s", path, runErr, combined)
		}

		output, ran = string(combined), path
		break
	}

	if ran == "" {
		t.Skip("neither pwsh nor powershell starts on this host: Get-CorpusFlavorArch was NOT compared with corpusFlavourOf")
	}

	got := map[string]string{}

	for _, line := range strings.Split(output, "\n") {
		line = strings.TrimSpace(line)

		if strings.HasPrefix(line, "edition=") {
			t.Logf("%s: %s", ran, line)
			continue
		}

		if key, value, found := strings.Cut(line, "="); found {
			got[key] = value
		}
	}

	for key, value := range want {
		if answer, found := got[key]; !found || answer != value {
			t.Errorf("%s: %s = %q (present %v), want %q", ran, key, answer, found, value)
		}
	}
}
