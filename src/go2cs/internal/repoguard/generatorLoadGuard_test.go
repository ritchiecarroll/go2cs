// generatorLoadGuard_test.go - Gbtc
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

// A source generator that fails to LOAD or INITIALIZE is only a warning to the compiler: CS8034 (the analyzer
// assembly cannot be loaded), CS8032 (it cannot be instantiated), CS8784 (the generator failed to initialize),
// CS8785 (it threw while generating). The build goes on without the generator's output and fails later, if at
// all, as a CS0246 on the first generated type the code names -- far from the cause, and a transient that
// passes on rerun (the i9's D5 reflect row, 2026-09-30: CS0246 on go/ast's FileжNode and inspector Visitor).
// Measured: a project given an unloadable analyzer prints `warning CS8034` and EXITS 0.
//
// So src/Directory.Build.props raises the four to errors, and a missing generator names itself. This guard
// pins what makes that true for every build under src/: the four codes are there and APPENDED
// ($(WarningsAsErrors) first, so nothing earlier is dropped); every nested Directory.Build.props imports the
// root one (MSBuild stops at the FIRST props file walking up, so a nested one that does not import would
// silently shadow it); and no tracked project, props or targets file REPLACES WarningsAsErrors. The census
// before the change read 0 of the four codes across 150 compilations of reflect's test closure, so raising
// them fails no build today.
var generatorLoadCodes = []string{"CS8032", "CS8034", "CS8784", "CS8785"}

var warningsAsErrorsElement = regexp.MustCompile(`<WarningsAsErrors(?:\s[^>]*)?>([^<]*)</WarningsAsErrors>`)

// clobbersWarningsAsErrors reports whether an MSBuild file SETS WarningsAsErrors without keeping the value
// already in force.
func clobbersWarningsAsErrors(text string) bool {
	for _, m := range warningsAsErrorsElement.FindAllStringSubmatch(text, -1) {
		if !strings.Contains(m[1], "$(WarningsAsErrors)") {
			return true
		}
	}

	return false
}

func TestGeneratorLoadFailuresAreBuildErrors(t *testing.T) {
	root := repoRootFromPackageDir(t)
	propsPath := filepath.Join(root, "src", "Directory.Build.props")

	props, err := os.ReadFile(propsPath)
	if err != nil {
		t.Fatalf("cannot read %s: %v", propsPath, err)
	}

	matches := warningsAsErrorsElement.FindAllStringSubmatch(string(props), -1)
	if len(matches) != 1 {
		t.Fatalf("src/Directory.Build.props sets <WarningsAsErrors> %d time(s); want exactly 1 carrying %s", len(matches), strings.Join(generatorLoadCodes, ";"))
	}

	value := matches[0][1]
	if !strings.HasPrefix(strings.TrimSpace(value), "$(WarningsAsErrors)") {
		t.Errorf("src/Directory.Build.props REPLACES WarningsAsErrors (%q); it must append to $(WarningsAsErrors)", value)
	}

	for _, code := range generatorLoadCodes {
		if !strings.Contains(value, code) {
			t.Errorf("src/Directory.Build.props does not raise %s to an error: a generator that fails that way would build on without its output", code)
		}
	}

	out, err := exec.Command("git", "-C", root, "ls-files", "-z", "src").Output()
	if err != nil {
		t.Fatalf("git ls-files failed in %s: %v", root, err)
	}

	for _, rel := range strings.Split(string(out), "\x00") {
		if rel == "" || rel == "src/Directory.Build.props" {
			continue
		}

		lower := strings.ToLower(rel)
		isProps := strings.HasSuffix(lower, "/directory.build.props")
		isMSBuild := isProps || strings.HasSuffix(lower, ".csproj") || strings.HasSuffix(lower, ".props") || strings.HasSuffix(lower, ".targets")

		if !isMSBuild {
			continue
		}

		text, err := os.ReadFile(filepath.Join(root, filepath.FromSlash(rel)))
		if err != nil {
			t.Fatalf("cannot read %s: %v", rel, err)
		}

		if isProps && !strings.Contains(string(text), "GetPathOfFileAbove") && !strings.Contains(string(text), "../Directory.Build.props") {
			t.Errorf("%s does not import the root src/Directory.Build.props, so builds under it lose the generator-load errors", rel)
		}

		if clobbersWarningsAsErrors(string(text)) {
			t.Errorf("%s sets <WarningsAsErrors> without $(WarningsAsErrors), dropping the generator-load errors for its builds", rel)
		}
	}
}

func TestWarningsAsErrorsClobberScannerFires(t *testing.T) {
	cases := []struct {
		text string
		want bool
	}{
		{"<PropertyGroup><WarningsAsErrors>CS0168</WarningsAsErrors></PropertyGroup>", true},
		{"<WarningsAsErrors Condition=\"'$(X)'=='1'\">CS0168</WarningsAsErrors>", true},
		{"<WarningsAsErrors>$(WarningsAsErrors);CS0168</WarningsAsErrors>", false},
		{"<NoWarn>CS0168</NoWarn>", false},
	}

	for _, c := range cases {
		if got := clobbersWarningsAsErrors(c.text); got != c.want {
			t.Errorf("clobbersWarningsAsErrors(%q) = %v, want %v", c.text, got, c.want)
		}
	}
}
