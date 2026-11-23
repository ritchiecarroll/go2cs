// testAssemblyOptimize_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// testAssemblyOptimize_test.go - Gbtc
//
//  Copyright © 2026, The go2cs Authors. All Rights Reserved.

package main

import (
	"os"
	"os/exec"
	"path/filepath"
	"strings"
	"testing"
)

// A converted test assembly is compiled without optimization (COORD 2026-11-22 23:57Z, provisional; the owner's scope:
// test code only, production stays optimized). runtime.Caller reads a call's line from its frame's IL offset, and a
// Release frame carries only its statement's offset: two same-name calls in one statement read one line (testify's
// Test_Mock_Chained_UnsetOnlyUnsetsLastCall), and an optimized frame can lose it altogether. Only code compiled with
// optimizations disabled carries a per-call offset. The switch lives in golib's go.lib.symbols.targets, which every
// in-tree project, a module conversion's generated Directory.Build.targets and every NuGet consumer import, so the
// stdlib's test hosts and a module's alike get it and no emitted file changes. GoTestOptimize=true keeps a test
// assembly optimized, for a benchmark run.

// evaluatedOptimize is the Optimize property MSBuild evaluates for project in a Release build.
func evaluatedOptimize(t *testing.T, project string, extra ...string) string {
	t.Helper()

	src, err := filepath.Abs("..")

	if err != nil {
		t.Fatal(err)
	}

	args := append([]string{"msbuild", project, "-nologo", "-getProperty:Optimize", "-p:Configuration=Release",
		"-p:go2csPath=" + src + string(filepath.Separator)}, extra...)
	out, err := exec.Command("dotnet", args...).CombinedOutput()

	if err != nil {
		t.Fatalf("dotnet msbuild -getProperty:Optimize %s: %v\n%s", project, err, out)
	}

	return strings.TrimSpace(string(out))
}

const optimizeProbeProject = `<Project>
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <RootNamespace>%s</RootNamespace>
  </PropertyGroup>
  <Import Project="Sdk.props" Sdk="Microsoft.NET.Sdk" />
  <Import Project="Sdk.targets" Sdk="Microsoft.NET.Sdk" />
</Project>
`

func TestATestAssemblyIsCompiledWithoutOptimization(t *testing.T) {
	if testing.Short() {
		t.Skip("evaluates MSBuild projects")
	}

	if _, err := exec.LookPath("dotnet"); err != nil {
		t.Skipf("the .NET SDK is not on PATH: %v", err)
	}

	src, err := filepath.Abs("..")

	if err != nil {
		t.Fatal(err)
	}

	// The stdlib's own projects, through src/Directory.Build.targets.
	stdlibTests := filepath.Join(src, "core", "fmt", "fmt.tests.csproj")
	stdlibProduction := filepath.Join(src, "core", "fmt", "fmt.csproj")

	// A module conversion's projects, through the Directory.Build.targets the converter generates for its output root.
	root := t.TempDir()
	NewModuleConverter(Options{go2csPath: src + string(filepath.Separator), recurseOutputRoot: root, recurse: true,
		targetPlatform: "linux/amd64"}).generateRecurseBuildFiles()

	moduleDir := filepath.Join(root, "src", "example.test", "m")
	moduleTests := filepath.Join(moduleDir, "example.test.m.tests.csproj")
	moduleProduction := filepath.Join(moduleDir, "example.test.m.csproj")
	userTests := filepath.Join(moduleDir, "acme.tests.csproj")

	for path, namespace := range map[string]string{moduleTests: "go.example.test", moduleProduction: "go.example.test", userTests: "acme"} {
		writeModuleFile(t, path, strings.Replace(optimizeProbeProject, "%s", namespace, 1))
	}

	for _, arm := range []struct {
		name    string
		project string
		extra   []string
		want    string
	}{
		{"the stdlib's test host", stdlibTests, nil, "false"},
		{"the stdlib's production project", stdlibProduction, nil, "true"},
		{"the stdlib's test host with GoTestOptimize=true", stdlibTests, []string{"-p:GoTestOptimize=true"}, "true"},
		{"a module's test host", moduleTests, nil, "false"},
		{"a module's production project", moduleProduction, nil, "true"},
		{"a user's own .tests project outside the go namespace", userTests, nil, "true"},
	} {
		if got := evaluatedOptimize(t, arm.project, arm.extra...); !strings.EqualFold(got, arm.want) {
			t.Errorf("%s: Optimize is %q, want %q", arm.name, got, arm.want)
		}
	}

	if _, err := os.Stat(filepath.Join(root, "Directory.Build.targets")); err != nil {
		t.Fatalf("the module arm ran without the generated Directory.Build.targets: %v", err)
	}
}
