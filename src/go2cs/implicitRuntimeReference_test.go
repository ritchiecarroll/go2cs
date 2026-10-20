// implicitRuntimeReference_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the implicit runtime reference (projectFileWriter.go, writeProjectFile). Go links runtime into every
// program; a C# app carries only what its references reach. An app whose import closure never reaches runtime --
// an app importing only math/bits, whose overflow and divide errors are runtime's own values, reached by name since
// go1.26.9 -- would ship without runtime, and its first Div64 overflow would panic naming a missing target instead
// of Go's runtime error. Rule (B), COORD 2026-10-14: such an app gains runtime as a reference; an app that already
// reaches runtime transitively gains NOTHING, so the rule cannot drift into "always" and churn every app project.
// These drive real module conversions and read the EMITTED project file.

package main

import (
	"go/build"
	"os"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// convertAppModule converts a one-package app module and returns its emitted project file.
func convertAppModule(t *testing.T, module string, mainGo string, nuget bool) string {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/"+module+"\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"), mainGo)

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{
		goRoot:              goRoot,
		goPath:              build.Default.GOPATH,
		go2csPath:           filepath.Join(root, "out"),
		recurse:             true,
		nugetRefs:           nuget,
		targetPlatform:      runtime.GOOS + "/" + runtime.GOARCH,
		indentSpaces:        4,
		preferVarDecl:       true,
		useChannelOperators: true,
	}

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	if err := NewModuleConverter(options).ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	projects, _ := filepath.Glob(filepath.Join(options.go2csPath, "src", "example.com", module, "*.csproj"))

	if len(projects) != 1 {
		t.Fatalf("expected one app project for %s, found %v", module, projects)
	}

	project, err := os.ReadFile(projects[0])

	if err != nil {
		t.Fatalf("reading the app project: %v", err)
	}

	return string(project)
}

// referenceLines returns the project's ProjectReference / PackageReference lines that name runtime.
func runtimeReferenceLines(project string) []string {
	var lines []string

	for _, line := range strings.Split(project, "\n") {
		trimmed := strings.TrimSpace(line)

		if strings.Contains(trimmed, "core/runtime/runtime.csproj") || strings.Contains(trimmed, `Include="go.runtime"`) {
			lines = append(lines, trimmed)
		}
	}

	return lines
}

const mathBitsOnlyApp = "package main\n\n" +
	"import \"math/bits\"\n\n" +
	"func main() {\n\tq, _ := bits.Div64(0, 6, 3)\n\tprintln(q)\n}\n"

func TestAppNotReachingRuntimeReferencesIt(t *testing.T) {
	isolateConverterPathGlobals(t)

	lines := runtimeReferenceLines(convertAppModule(t, "bitsonly", mathBitsOnlyApp, false))

	if len(lines) != 1 || !strings.Contains(lines[0], "<ProjectReference") {
		t.Fatalf("an app importing only math/bits must reference runtime exactly once (Go links runtime into every "+
			"program; math/bits's errors are runtime's own values), found %v", lines)
	}
}

func TestAppNotReachingRuntimeReferencesTheRuntimePackageUnderNuGet(t *testing.T) {
	isolateConverterPathGlobals(t)

	lines := runtimeReferenceLines(convertAppModule(t, "bitsonlynuget", mathBitsOnlyApp, true))

	if len(lines) != 1 || !strings.Contains(lines[0], `<PackageReference Include="go.runtime"`) {
		t.Fatalf("under -recurse=nuget an app importing only math/bits must take the go.runtime package, found %v", lines)
	}
}

// The (B)-cannot-drift-into-(A) arm: an app that reaches runtime through fmt gains no reference line.
func TestAppReachingRuntimeTransitivelyGainsNoReference(t *testing.T) {
	isolateConverterPathGlobals(t)

	project := convertAppModule(t, "viafmt", "package main\n\nimport \"fmt\"\n\nfunc main() { fmt.Println(\"hi\") }\n", false)

	if lines := runtimeReferenceLines(project); len(lines) != 0 {
		t.Fatalf("an app that already reaches runtime through fmt must gain NO runtime reference (rule B), found %v", lines)
	}

	if !strings.Contains(project, "core/fmt/fmt.csproj") {
		t.Fatalf("control: the app's own fmt reference is missing, so this test is not reading the reference block")
	}
}
