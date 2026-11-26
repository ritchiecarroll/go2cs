// testsHostBuildFiles_test.go - Gbtc
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

// A test host converted with -tests OUTSIDE a go2cs root has three ways to be built by hand: `dotnet run`, a VS Code
// open (no SolutionDir) and a Visual Studio open (SolutionDir is the project's own folder). Measured on a module
// package under .NET 11 (the usability pass, 2026-11-26), all three failed: the production csproj's go2csPath
// defaulted to $(SolutionDir) or nothing (CS0246, CS0234 'go.builtin', MSB9008), and under Visual Studio the tests
// csproj's relative pin was never reached (NU1104). One rule fixes all three: a Directory.Build.props beside the host
// sets go2csPath before either project body runs.

func testsHostFixture(t *testing.T) (root, output string) {
	t.Helper()

	base := t.TempDir()
	root = filepath.Join(base, "go2cs")
	output = filepath.Join(base, "work", "out", "greet")

	for _, dir := range []string{filepath.Join(root, "core", "golib"), output} {
		if err := os.MkdirAll(dir, 0o755); err != nil {
			t.Fatal(err)
		}
	}

	return root, output
}

func readTestsHostFile(t *testing.T, path string) string {
	t.Helper()

	contents, err := os.ReadFile(path)
	if err != nil {
		t.Fatalf("expected %s to be written: %v", path, err)
	}

	return string(contents)
}

func TestTestsHostBuildFilesPinGo2CSPathOutsideTheRoot(t *testing.T) {
	root, output := testsHostFixture(t)

	writeTestsHostBuildFiles(output, Options{go2csPath: root, targetPlatform: "linux/amd64"})

	props := readTestsHostFile(t, filepath.Join(output, "Directory.Build.props"))

	for _, want := range []string{
		testsHostBuildFileMarker,
		"<PropertyGroup Condition=\"'$(go2csPath)' == ''\">",
		"<go2csPath>$(MSBuildThisFileDirectory)../../../go2cs/</go2csPath>",
		"<GoTargetOS>linux</GoTargetOS>",
		"<GoTargetArch>amd64</GoTargetArch>",
		"GetPathOfFileAbove('Directory.Build.props', '$(MSBuildThisFileDirectory)../')",
	} {
		if !strings.Contains(props, want) {
			t.Errorf("Directory.Build.props lacks %q:\n%s", want, props)
		}
	}

	if strings.Contains(props, "SolutionDir") {
		t.Errorf("Directory.Build.props must not consult SolutionDir, which an IDE sets to the host folder or leaves empty:\n%s", props)
	}

	targets := readTestsHostFile(t, filepath.Join(output, "Directory.Build.targets"))

	for _, want := range []string{
		testsHostBuildFileMarker,
		"GetPathOfFileAbove('Directory.Build.targets', '$(MSBuildThisFileDirectory)../')",
		"$(go2csPath)core/golib/buildTransitive/go.lib.rollforward.targets",
		"'$(GoLibRollForwardTargetsImported)' != 'true'",
		"<_GenerateRestoreGraphProjectEntryInputProperties>",
	} {
		if !strings.Contains(targets, want) {
			t.Errorf("Directory.Build.targets lacks %q:\n%s", want, targets)
		}
	}
}

func TestTestsHostBuildFilesSkipAPinnedOutput(t *testing.T) {
	t.Run("inside the go2cs root", func(t *testing.T) {
		root, _ := testsHostFixture(t)
		output := filepath.Join(root, "core", "strings")

		if err := os.MkdirAll(output, 0o755); err != nil {
			t.Fatal(err)
		}

		writeTestsHostBuildFiles(output, Options{go2csPath: root, targetPlatform: "linux/amd64"})

		for _, name := range []string{"Directory.Build.props", "Directory.Build.targets"} {
			if _, err := os.Stat(filepath.Join(output, name)); err == nil {
				t.Errorf("%s written inside the go2cs root, whose own props pin go2csPath", name)
			}
		}
	})

	t.Run("under a -recurse output root", func(t *testing.T) {
		root, output := testsHostFixture(t)
		recurseRoot := filepath.Dir(filepath.Dir(output))

		recurseProps := "<Project>\r\n  " + generatedBuildFileMarker + "\r\n</Project>\r\n"
		if err := os.WriteFile(filepath.Join(recurseRoot, "Directory.Build.props"), []byte(recurseProps), 0o644); err != nil {
			t.Fatal(err)
		}

		writeTestsHostBuildFiles(output, Options{go2csPath: root, targetPlatform: "linux/amd64"})

		for _, name := range []string{"Directory.Build.props", "Directory.Build.targets"} {
			if _, err := os.Stat(filepath.Join(output, name)); err == nil {
				t.Errorf("%s written under a -recurse output root, whose props pin go2csPath", name)
			}
		}
	})
}

func TestTestsHostBuildFilesLeaveAForeignFile(t *testing.T) {
	root, output := testsHostFixture(t)
	foreign := "<Project><!-- the user's own --></Project>"
	propsPath := filepath.Join(output, "Directory.Build.props")

	if err := os.WriteFile(propsPath, []byte(foreign), 0o644); err != nil {
		t.Fatal(err)
	}

	writeTestsHostBuildFiles(output, Options{go2csPath: root, targetPlatform: "linux/amd64"})

	if got := readTestsHostFile(t, propsPath); got != foreign {
		t.Errorf("a foreign Directory.Build.props was rewritten:\n%s", got)
	}

	if targets := readTestsHostFile(t, filepath.Join(output, "Directory.Build.targets")); !strings.Contains(targets, testsHostBuildFileMarker) {
		t.Errorf("the generated Directory.Build.targets was not written beside a foreign props:\n%s", targets)
	}

	// A file go2cs wrote itself is refreshed.
	writeTestsHostBuildFiles(output, Options{go2csPath: root, targetPlatform: "darwin/arm64"})

	if targets := readTestsHostFile(t, filepath.Join(output, "Directory.Build.targets")); !strings.Contains(targets, testsHostBuildFileMarker) {
		t.Errorf("the generated Directory.Build.targets did not survive a second run:\n%s", targets)
	}
}

// The single-package -tests flow writes the files right after the tests csproj, so every host it emits gets them.
var testsHostHookCall = regexp.MustCompile(`(?s)writeTestProject\(filepath\.Join\(outputPath, testProjectName\).*?\n\t}\n\n\twriteTestsHostBuildFiles\(outputPath, options\)\n`)

func TestTestsConversionWritesTheHostBuildFiles(t *testing.T) {
	source, err := os.ReadFile("testConversion.go")
	if err != nil {
		t.Fatal(err)
	}

	if !testsHostHookCall.Match([]byte(strings.ReplaceAll(string(source), "\r\n", "\n"))) {
		t.Errorf("testConversion.go does not call writeTestsHostBuildFiles(outputPath, options) right after writeTestProject")
	}
}
