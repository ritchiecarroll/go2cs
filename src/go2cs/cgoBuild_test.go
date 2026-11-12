// cgoBuild_test.go - Gbtc
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
	"slices"
	"strings"
	"testing"
)

// cgo Phase 2's exit gate, G1, the "compiles" half (docs/phase4/DESIGN-cgo-phase2-pinvoke.md §11.22): the cgo route's
// output built as a project. Before this, nothing compiled what the route writes: the hosted legs show that it
// converts, and GolibTests runs the generator's wrappers over hand-declared types. Here the converter's own C types,
// its bindings file, go2cs-gen and the SDK's LibraryImport generator meet in one build.
//
// The build needs no C compiler, so it runs on any lane with the .NET SDK. It is opt-in (GO2CS_CGO_BUILD=1), since it
// builds the fixture's whole closure of the converted standard library, which takes minutes.

// cgoBuildEnv turns the build tests on. A leg that sets GO2CS_REQUIRE_DOTNET=1 beside it fails where no SDK is.
const cgoBuildEnv = "GO2CS_CGO_BUILD"

// cgoCompilerErrors are the distinct C# compiler and MSBuild errors in a build's output, each with its code first.
func cgoCompilerErrors(output string) []string {
	pattern := regexp.MustCompile(`(?m)^(.*?)\s*:\s*error\s+([A-Z]+\d+)\s*:\s*(.*?)(\s+\[[^\]]*\])?\r?$`)
	seen := map[string]bool{}
	var lines []string

	for _, match := range pattern.FindAllStringSubmatch(output, -1) {
		line := match[2] + ": " + match[3] + " (" + filepath.Base(regexp.MustCompile(`\(\d+,\d+\)$`).ReplaceAllString(match[1], "")) + ")"

		if !seen[line] {
			seen[line] = true
			lines = append(lines, line)
		}
	}

	slices.Sort(lines)

	return lines
}

func TestTheCgoRoutesOutputBuildsAsAProject(t *testing.T) {
	if os.Getenv(cgoBuildEnv) != "1" {
		t.Skipf("NOT RUN: set %s=1 to build the converted fixture with the .NET SDK (Inconclusive, not a pass)", cgoBuildEnv)
	}

	framework := regexp.MustCompile(`<TargetFramework Condition="[^"]*">([^<]+)</TargetFramework>`).FindStringSubmatch(renderCsprojTemplate("Library", "", ""))

	if framework == nil {
		t.Fatal("the rendered template names no TargetFramework")
	}

	dotnet := requireDotnetSdk(t, framework[1])
	output := t.TempDir()

	// A directory the caller names keeps the converted project for reading after the run.
	if kept := os.Getenv("GO2CS_CGO_BUILD_DIR"); kept != "" {
		if err := os.MkdirAll(kept, 0o755); err != nil {
			t.Fatal(err)
		}

		output = kept
	}

	combined, _, err := convertCgoOfflineInto(t, cgoPhase2Admitted(t), output)

	if err != nil {
		t.Fatalf("the Phase 2 fixture did not convert: %v\n%s", err, lastLines(combined, 30))
	}

	projects, err := filepath.Glob(filepath.Join(output, "*.csproj"))

	if err != nil || len(projects) != 1 {
		t.Fatalf("the conversion wrote %d project files in %s, want 1 (%v)", len(projects), output, err)
	}

	// A converted project finds golib, go2cs-gen and the standard library through go2csPath, as every converted
	// project does; here it is this tree.
	treeSrc, err := filepath.Abs("..")

	if err != nil {
		t.Fatal(err)
	}

	build := exec.Command(dotnet, "build", projects[0], "-nologo", "--disable-build-servers", "-nodeReuse:false", "-p:UseSharedCompilation=false", "-v:q",
		"-p:go2csPath="+treeSrc+string(filepath.Separator))
	build.Env = append(os.Environ(), "DOTNET_CLI_TELEMETRY_OPTOUT=1", "DOTNET_NOLOGO=1", "MSBUILDDISABLENODEREUSE=1")
	built, err := build.CombinedOutput()
	errors := cgoCompilerErrors(string(built))

	t.Logf("READING the converted fixture's build: %d distinct error(s)", len(errors))

	for _, line := range errors {
		t.Logf("READING build error %s", line)
	}

	if err != nil {
		t.Fatalf("the cgo route's output does not build as a project: %v\n%s", err, lastLines(strings.ReplaceAll(string(built), "\r\n", "\n"), 40))
	}
}
