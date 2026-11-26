// testsHostBuildFiles_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"encoding/json"
	"os"
	"os/exec"
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

var (
	xmlComment   = regexp.MustCompile(`(?s)<!--.*?-->`)
	xmlCondition = regexp.MustCompile(`Condition="([^"]*)"`)
)

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

// testsHostNoModule is an input directory with no go.mod above it, as a standard-library package's reads to this
// writer: no module root is recorded for it.
func testsHostNoModule(t *testing.T) string {
	t.Helper()
	return filepath.Join(t.TempDir(), "strings")
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

	writeTestsHostBuildFiles(testsHostNoModule(t), output, Options{go2csPath: root, targetPlatform: "linux/amd64"})

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

	if strings.Contains(xmlComment.ReplaceAllString(props, ""), "SolutionDir") {
		t.Errorf("Directory.Build.props must not consult SolutionDir, which an IDE sets to the host folder or leaves empty:\n%s", props)
	}

	targets := readTestsHostFile(t, filepath.Join(output, "Directory.Build.targets"))

	// MSBuild's condition parser takes no property function with quoted arguments inside a quoted operand (MSB4092,
	// measured on the first emission): such a path goes through a property first.
	for _, contents := range []string{props, targets} {
		for _, condition := range xmlCondition.FindAllStringSubmatch(contents, -1) {
			if strings.Contains(condition[1], "$([") {
				t.Errorf("a Condition calls a property function MSBuild cannot parse there: %s", condition[1])
			}
		}
	}

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

		writeTestsHostBuildFiles(testsHostNoModule(t), output, Options{go2csPath: root, targetPlatform: "linux/amd64"})

		for _, name := range []string{"Directory.Build.props", "Directory.Build.targets"} {
			if _, err := os.Stat(filepath.Join(output, name)); err == nil {
				t.Errorf("%s written inside the go2cs root, whose own props pin go2csPath", name)
			}
		}
	})

	// P3 182613Z SHOULD 1: the configured root is a deployed one, and the output sits under a CLONE's src/core,
	// another go2cs root whose own props pins go2csPath. Untracked files written there are what a later sweep or
	// `git add` trips on.
	t.Run("under a go2cs root other than the configured one", func(t *testing.T) {
		deployed, _ := testsHostFixture(t)
		clone := filepath.Join(t.TempDir(), "clone", "src")
		output := filepath.Join(clone, "core", "strings")

		for _, dir := range []string{filepath.Join(clone, "core", "golib"), output} {
			if err := os.MkdirAll(dir, 0o755); err != nil {
				t.Fatal(err)
			}
		}

		if err := os.WriteFile(filepath.Join(clone, "core", "golib", "golib.csproj"), []byte("<Project />"), 0o644); err != nil {
			t.Fatal(err)
		}

		writeTestsHostBuildFiles(testsHostNoModule(t), output, Options{go2csPath: deployed, targetPlatform: "linux/amd64"})

		for _, name := range []string{"Directory.Build.props", "Directory.Build.targets"} {
			if _, err := os.Stat(filepath.Join(output, name)); err == nil {
				t.Errorf("%s written under a clone's go2cs root, whose own props pins go2csPath", name)
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

		writeTestsHostBuildFiles(testsHostNoModule(t), output, Options{go2csPath: root, targetPlatform: "linux/amd64"})

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

	writeTestsHostBuildFiles(testsHostNoModule(t), output, Options{go2csPath: root, targetPlatform: "linux/amd64"})

	if got := readTestsHostFile(t, propsPath); got != foreign {
		t.Errorf("a foreign Directory.Build.props was rewritten:\n%s", got)
	}

	if targets := readTestsHostFile(t, filepath.Join(output, "Directory.Build.targets")); !strings.Contains(targets, testsHostBuildFileMarker) {
		t.Errorf("the generated Directory.Build.targets was not written beside a foreign props:\n%s", targets)
	}

	// A file go2cs wrote itself is refreshed.
	writeTestsHostBuildFiles(testsHostNoModule(t), output, Options{go2csPath: root, targetPlatform: "darwin/arm64"})

	if targets := readTestsHostFile(t, filepath.Join(output, "Directory.Build.targets")); !strings.Contains(targets, testsHostBuildFileMarker) {
		t.Errorf("the generated Directory.Build.targets did not survive a second run:\n%s", targets)
	}
}

// The single-package -tests flow writes the files right after the tests csproj, so every host it emits gets them.
var testsHostHookCall = regexp.MustCompile(`(?s)writeTestProject\(filepath\.Join\(outputPath, testProjectName\).*?\n\t}\n\n\twriteTestsHostBuildFiles\(inputPath, outputPath, options\)\n`)

func TestTestsConversionWritesTheHostBuildFiles(t *testing.T) {
	source, err := os.ReadFile("testConversion.go")
	if err != nil {
		t.Fatal(err)
	}

	if !testsHostHookCall.Match([]byte(strings.ReplaceAll(string(source), "\r\n", "\n"))) {
		t.Errorf("testConversion.go does not call writeTestsHostBuildFiles(inputPath, outputPath, options) right after writeTestProject")
	}
}

// A module package's host also records the module's directory (the T17 seat, COORD 182438Z): GoModuleRoot, relative to
// the props as the go2csPath pin is, carried into the host's runtimeconfig as go2cs.ModuleRoot, which the host reads
// when GO2CS_MODULE_ROOT is unset. The emitted file holds no absolute path. A standard-library package records none.
func TestTestsHostBuildFilesRecordTheModuleRoot(t *testing.T) {
	root, output := testsHostFixture(t)
	module := filepath.Join(filepath.Dir(filepath.Dir(output)), "mod")
	input := filepath.Join(module, "greet")

	if err := os.MkdirAll(input, 0o755); err != nil {
		t.Fatal(err)
	}

	if err := os.WriteFile(filepath.Join(module, "go.mod"), []byte("module example.com/hello\n\ngo 1.23\n"), 0o644); err != nil {
		t.Fatal(err)
	}

	writeTestsHostBuildFiles(input, output, Options{go2csPath: root, targetPlatform: "linux/amd64"})

	props := readTestsHostFile(t, filepath.Join(output, "Directory.Build.props"))

	for _, want := range []string{
		"<PropertyGroup Condition=\"'$(GoModuleRoot)' == ''\">",
		"<GoModuleRoot>$([System.IO.Path]::GetFullPath('$(MSBuildThisFileDirectory)../../mod'))</GoModuleRoot>",
		"<ItemGroup Condition=\"'$(GoModuleRoot)' != ''\">",
		"<RuntimeHostConfigurationOption Include=\"go2cs.ModuleRoot\" Value=\"$(GoModuleRoot)\" />",
	} {
		if !strings.Contains(props, want) {
			t.Errorf("a module host's Directory.Build.props lacks %q:\n%s", want, props)
		}
	}

	if strings.Contains(props, filepath.ToSlash(module)) || strings.Contains(props, module) {
		t.Errorf("the module root is emitted as an absolute path:\n%s", props)
	}

	for _, condition := range xmlCondition.FindAllStringSubmatch(props, -1) {
		if strings.Contains(condition[1], "$([") {
			t.Errorf("a Condition calls a property function MSBuild cannot parse there: %s", condition[1])
		}
	}

	// The standard-library shape records nothing.
	_, stdOutput := testsHostFixture(t)
	writeTestsHostBuildFiles(testsHostNoModule(t), stdOutput, Options{go2csPath: root, targetPlatform: "linux/amd64"})

	if stdProps := readTestsHostFile(t, filepath.Join(stdOutput, "Directory.Build.props")); strings.Contains(stdProps, "GoModuleRoot") {
		t.Errorf("a host with no module records a module root:\n%s", stdProps)
	}
}

// testsHostModuleFixture is a module beside the fixture's output (<base>/work/<name>, holding go.mod and a package
// directory greet), returning the package's input directory.
func testsHostModuleFixture(t *testing.T, output, name string) string {
	t.Helper()

	module := filepath.Join(filepath.Dir(filepath.Dir(output)), name)
	input := filepath.Join(module, "greet")

	if err := os.MkdirAll(input, 0o755); err != nil {
		t.Fatal(err)
	}

	if err := os.WriteFile(filepath.Join(module, "go.mod"), []byte("module example.com/hello\n\ngo 1.23\n"), 0o644); err != nil {
		t.Fatal(err)
	}

	return input
}

// A recorded module root is emitted text that people commit, so a machine path must never land in it (P3 194127Z,
// COORD 194711Z). A root is recorded only as a path relative to the props that stays below a common ancestor short of
// the volume root, is not in the module cache, and needs no MSBuild escaping; anything else records nothing, and the
// host keeps today's refusal naming GO2CS_MODULE_ROOT.
func TestTestsHostModuleRootIsRecordedOnlyAsASafeRelativePath(t *testing.T) {
	volume := filepath.VolumeName(os.TempDir()) + string(os.PathSeparator)
	output := filepath.Join(volume, "work", "out", "greet")

	saved := goModCache
	goModCache = filepath.Join(volume, "cache", "mod")
	t.Cleanup(func() { goModCache = saved })

	for _, row := range []struct {
		name, target, want string
	}{
		{"a sibling of the output", filepath.Join(volume, "work", "mod"), "$(MSBuildThisFileDirectory)../../mod"},
		{"a path that climbs to the volume root", filepath.Join(volume, "home", "user", "mod"), ""},
		{"a module-cache root", filepath.Join(volume, "cache", "mod", "github.com", "x", "y@v1.2.3"), ""},
		{"a path on another volume (filepath.Rel fails)", filepath.Join("relative", "mod"), ""},
		{"a single quote", filepath.Join(volume, "work", "it's"), ""},
		{"a property reference", filepath.Join(volume, "work", "$(x)"), ""},
		{"an item reference", filepath.Join(volume, "work", "@(x)"), ""},
		{"a percent escape", filepath.Join(volume, "work", "%41"), ""},
	} {
		got, ok := testsHostRecordableModulePath(output, row.target)

		if want := row.want != ""; ok != want || got != row.want {
			t.Errorf("%s: testsHostRecordableModulePath(%q) = %q, %v; want %q, %v", row.name, row.target, got, ok, row.want, want)
		}
	}
}

// End to end through the writer: a module-cache module and a module whose path needs escaping record nothing.
func TestTestsHostBuildFilesRecordNoUnsafeModuleRoot(t *testing.T) {
	root, output := testsHostFixture(t)
	cached := testsHostModuleFixture(t, output, "mod")

	saved := goModCache
	goModCache = filepath.Dir(filepath.Dir(cached))
	t.Cleanup(func() { goModCache = saved })

	writeTestsHostBuildFiles(cached, output, Options{go2csPath: root, targetPlatform: "linux/amd64"})

	if props := readTestsHostFile(t, filepath.Join(output, "Directory.Build.props")); strings.Contains(props, "GoModuleRoot") {
		t.Errorf("a module-cache module's root was recorded:\n%s", props)
	}

	goModCache = saved
	root, output = testsHostFixture(t)
	quoted := testsHostModuleFixture(t, output, "it's")

	writeTestsHostBuildFiles(quoted, output, Options{go2csPath: root, targetPlatform: "linux/amd64"})

	if props := readTestsHostFile(t, filepath.Join(output, "Directory.Build.props")); strings.Contains(props, "GoModuleRoot") {
		t.Errorf("a module root holding a single quote was recorded:\n%s", props)
	}
}

// The SDK step the record rides on (P3 194127Z, F4): RuntimeHostConfigurationOption in the generated props must reach
// bin/<host>.runtimeconfig.json as go2cs.ModuleRoot, holding the module's full path. A trivial console project stands
// in for the host, so the row needs the SDK alone, at the SDK's own target framework, and no go2cs build.
func TestTestsHostBuildFilesCarryTheModuleRootIntoTheRuntimeconfig(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: runs dotnet build")
	}

	dotnet, err := exec.LookPath("dotnet")

	if err != nil {
		t.Skip("NOT RUN: no dotnet on PATH (Inconclusive, not a pass)")
	}

	version, err := exec.Command(dotnet, "--version").Output()

	if err != nil {
		t.Skipf("NOT RUN: dotnet on PATH has no SDK (dotnet --version: %v; Inconclusive, not a pass)", err)
	}

	major, _, _ := strings.Cut(strings.TrimSpace(string(version)), ".")
	root, output := testsHostFixture(t)
	input := testsHostModuleFixture(t, output, "mod")
	module := filepath.Dir(input)

	writeTestsHostBuildFiles(input, output, Options{go2csPath: root, targetPlatform: "linux/amd64"})

	project := "<Project Sdk=\"Microsoft.NET.Sdk\">\n  <PropertyGroup>\n    <OutputType>Exe</OutputType>\n" +
		"    <TargetFramework>net" + major + ".0</TargetFramework>\n  </PropertyGroup>\n</Project>\n"

	if err := os.WriteFile(filepath.Join(output, "probe.csproj"), []byte(project), 0o644); err != nil {
		t.Fatal(err)
	}

	if err := os.WriteFile(filepath.Join(output, "Program.cs"), []byte("System.Console.WriteLine();\n"), 0o644); err != nil {
		t.Fatal(err)
	}

	build := exec.Command(dotnet, "build", "probe.csproj", "-c", "Debug", "-o", "bin", "-nologo", "-p:UseSharedCompilation=false")
	build.Dir = output
	build.Env = append(os.Environ(), "MSBUILDDISABLENODEREUSE=1", "DOTNET_CLI_TELEMETRY_OPTOUT=1")

	if log, err := build.CombinedOutput(); err != nil {
		t.Fatalf("dotnet build of the probe host failed: %v\n%s", err, log)
	}

	contents, err := os.ReadFile(filepath.Join(output, "bin", "probe.runtimeconfig.json"))

	if err != nil {
		t.Fatal(err)
	}

	var config struct {
		RuntimeOptions struct {
			ConfigProperties map[string]any `json:"configProperties"`
		} `json:"runtimeOptions"`
	}

	if err := json.Unmarshal(contents, &config); err != nil {
		t.Fatalf("probe.runtimeconfig.json: %v\n%s", err, contents)
	}

	got, _ := config.RuntimeOptions.ConfigProperties["go2cs.ModuleRoot"].(string)

	if filepath.Clean(got) != filepath.Clean(module) {
		t.Errorf("the built host's runtimeconfig carries go2cs.ModuleRoot %q; want the module's directory %q\n%s", got, module, contents)
	}
}

// goModCacheDir's last fallback, for GOMODCACHE unset and `go env` failing, is Go's own default: the first GOPATH
// entry's pkg/mod, and with no GOPATH, <home>/go/pkg/mod (P3 202012Z, nit 2). Joining an empty GOPATH gave the
// relative pkg/mod, which isPathUnder never matches, so the module-cache arm of testsHostRecordableModulePath went
// silent and ~/out/y beside ~/go/pkg/mod recorded the cache layout.
func TestTheModuleCacheFallbackIsGosDefault(t *testing.T) {
	home := filepath.Join(filepath.VolumeName(os.TempDir())+string(os.PathSeparator), "users", "a")
	first := filepath.Join(home, "gopath")
	second := filepath.Join(home, "other")

	for _, row := range []struct {
		name, gopath, home, want string
	}{
		{"GOPATH set", first, home, filepath.Join(first, "pkg", "mod")},
		{"GOPATH a list", first + string(os.PathListSeparator) + second, home, filepath.Join(first, "pkg", "mod")},
		{"GOPATH unset", "", home, filepath.Join(home, "go", "pkg", "mod")},
		{"neither", "", "", ""},
	} {
		if got := defaultGoModCache(row.gopath, row.home); got != row.want {
			t.Errorf("%s: defaultGoModCache(%q, %q) = %q; want %q", row.name, row.gopath, row.home, got, row.want)
		}
	}
}
