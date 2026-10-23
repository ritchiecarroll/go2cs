// buildInfoStamp_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"encoding/xml"
	"go/build"
	"os"
	"path/filepath"
	"runtime"
	"sort"
	"strings"
	"testing"
)

// modInfoLineKeys is the set of first fields of a build-info stamp's lines.
func modInfoLineKeys(t *testing.T, stamp string) []string {
	t.Helper()

	if !strings.HasSuffix(stamp, "\n") {
		t.Fatalf("stamp does not end in a newline: %q", stamp)
	}

	seen := map[string]bool{}

	for _, line := range strings.Split(strings.TrimSuffix(stamp, "\n"), "\n") {
		key, _, _ := strings.Cut(line, "\t")
		seen[key] = true
	}

	keys := make([]string, 0, len(seen))

	for key := range seen {
		keys = append(keys, key)
	}

	sort.Strings(keys)
	return keys
}

// assertModInfoCarriesNoBuildSettings is the negative control: the settings with no honest managed value never
// reach a stamp, and no build line does at all (those are composed at run time).
func assertModInfoCarriesNoBuildSettings(t *testing.T, stamp string) {
	t.Helper()

	for _, forbidden := range []string{"-compiler", "-buildmode", "GOAMD64", "vcs", "build\t"} {
		if strings.Contains(stamp, forbidden) {
			t.Errorf("stamp carries %q:\n%s", forbidden, stamp)
		}
	}
}

// TestProgramModInfoLineSet: a program's stamp is exactly {path, mod[, dep]}, in Go's own line form.
func TestProgramModInfoLineSet(t *testing.T) {
	dependencies := []thirdPartyModule{{path: "example.test/a", version: "v1.2.3"}, {path: "example.test/b", version: "v0.4.0", replaced: true}}

	single := programModInfo("example.test/prog/cmd/tool", "example.test/prog", nil)

	if want := "path\texample.test/prog/cmd/tool\nmod\texample.test/prog\t(devel)\t\n"; single != want {
		t.Errorf("single-package stamp = %q, want %q", single, want)
	}

	if got := strings.Join(modInfoLineKeys(t, single), ","); got != "mod,path" {
		t.Errorf("single-package line set = {%s}, want {mod,path}", got)
	}

	recursed := programModInfo("example.test/prog", "example.test/prog", dependencies)

	if got := strings.Join(modInfoLineKeys(t, recursed), ","); got != "dep,mod,path" {
		t.Errorf("-recurse line set = {%s}, want {dep,mod,path}", got)
	}

	if !strings.HasSuffix(recursed, "dep\texample.test/a\tv1.2.3\t\ndep\texample.test/b\tv0.4.0\t\n") {
		t.Errorf("-recurse stamp does not end in the dep lines, in path order:\n%q", recursed)
	}

	assertModInfoCarriesNoBuildSettings(t, single)
	assertModInfoCarriesNoBuildSettings(t, recursed)

	// Outside a module there is nothing to stamp: a GOPATH-mode Go binary embeds no module info, and its import
	// path can be a machine path.
	if got := programModInfo("_/C_/somewhere/prog", "", dependencies); got != "" {
		t.Errorf("a package outside any module was stamped: %q", got)
	}
}

// stampedModInfo reads the GoModInfo AssemblyAttribute value back out of a project file, as MSBuild would.
func stampedModInfo(t *testing.T, project string) (string, bool) {
	t.Helper()

	var parsed struct {
		ItemGroups []struct {
			Attributes []struct {
				Include    string `xml:"Include,attr"`
				Parameter1 string `xml:"_Parameter1"`
			} `xml:"AssemblyAttribute"`
		} `xml:"ItemGroup"`
	}

	if err := xml.Unmarshal([]byte(project), &parsed); err != nil {
		t.Fatalf("project file does not parse: %v\n%s", err, project)
	}

	for _, group := range parsed.ItemGroups {
		for _, attribute := range group.Attributes {
			if attribute.Include == "go.GoModInfoAttribute" {
				return attribute.Parameter1, true
			}
		}
	}

	return "", false
}

// TestStampModInfoProjectIsOneItemGroup: the stamp is one ItemGroup -- five lines plus the blank separator, CRLF
// like the project around it -- whose value round-trips through XML byte-exact.
func TestStampModInfoProjectIsOneItemGroup(t *testing.T) {
	before := "<Project Sdk=\"Microsoft.NET.Sdk\">\r\n\r\n  <ItemGroup>\r\n    <ProjectReference Include=\"x.csproj\" />\r\n  </ItemGroup>\r\n\r\n</Project>\r\n"
	projectFileName := filepath.Join(t.TempDir(), "prog.csproj")

	if err := os.WriteFile(projectFileName, []byte(before), 0o644); err != nil {
		t.Fatal(err)
	}

	stamp := programModInfo("example.test/prog", "example.test/prog", []thirdPartyModule{{path: "example.test/a&b", version: "v1.0.0"}})

	if err := stampModInfoProject(projectFileName, stamp); err != nil {
		t.Fatalf("stampModInfoProject: %v", err)
	}

	data, err := os.ReadFile(projectFileName)

	if err != nil {
		t.Fatal(err)
	}

	after := string(data)

	if added := strings.Count(after, "\r\n") - strings.Count(before, "\r\n"); added != 6 {
		t.Errorf("the stamp added %d lines, want 6 (one five-line ItemGroup and its blank separator):\n%s", added, after)
	}

	if strings.Count(after, "\n") != strings.Count(after, "\r\n") {
		t.Errorf("the stamp wrote a bare LF:\n%q", after)
	}

	if value, ok := stampedModInfo(t, after); !ok || value != stamp {
		t.Errorf("stamped value = %q (found %v), want %q", value, ok, stamp)
	}

	// No value, no item.
	if err := stampModInfoProject(projectFileName, ""); err != nil {
		t.Fatal(err)
	}

	if data, _ := os.ReadFile(projectFileName); string(data) != after {
		t.Errorf("an empty stamp changed the project file")
	}
}

// TestMainProgramsCarryTheirModInfo converts one module's main package both ways: -recurse stamps path, mod and
// one dep line for its replaced dependency; the same package converted on its own stamps path and mod. The
// library package is never stamped.
func TestMainProgramsCarryTheirModInfo(t *testing.T) {
	isolateConverterPathGlobals(t)
	if testing.Short() {
		t.Skip("integration test: converts a module")
	}

	root := t.TempDir()
	moduleDir := filepath.Join(root, "prog")
	dependencyDir := filepath.Join(root, "dep")

	writeModuleFile(t, filepath.Join(dependencyDir, "go.mod"), "module example.test/dep\n\ngo 1.24\n")
	writeModuleFile(t, filepath.Join(dependencyDir, "dep.go"), "package dep\n\nfunc Answer() int { return 42 }\n")
	writeModuleFile(t, filepath.Join(moduleDir, "go.mod"), "module example.test/prog\n\ngo 1.24\n\nrequire example.test/dep v0.1.0\n\nreplace example.test/dep => ../dep\n")
	writeModuleFile(t, filepath.Join(moduleDir, "main.go"), "package main\n\nimport \"example.test/dep\"\n\nfunc main() { println(dep.Answer()) }\n")

	options := godebugTestOptions()
	options.goPath = build.Default.GOPATH
	options.go2csPath = filepath.Join(root, "runtime")
	options.recurseOutputRoot = filepath.Join(root, "out")
	options.recurse = true
	options.targetPlatform = runtime.GOOS + "/" + runtime.GOARCH
	options.indentSpaces = 4
	options.preferVarDecl = true
	options.useChannelOperators = true

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	if err := NewModuleConverter(options).ConvertModule(moduleDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	projects, _ := filepath.Glob(filepath.Join(options.recurseOutputRoot, "src", "example.test", "prog", "*.csproj"))

	if len(projects) != 1 {
		t.Fatalf("want one program project, got %v", projects)
	}

	stamp, ok := stampedModInfo(t, readGenerated(t, projects[0]))

	if want := "path\texample.test/prog\nmod\texample.test/prog\t(devel)\t\ndep\texample.test/dep\tv0.1.0\t\n"; !ok || stamp != want {
		t.Errorf("-recurse program stamp = %q (found %v), want %q", stamp, ok, want)
	}

	assertModInfoCarriesNoBuildSettings(t, stamp)

	libraries, _ := filepath.Glob(filepath.Join(options.recurseOutputRoot, "pkg", "*", "*.csproj"))
	libraries2, _ := filepath.Glob(filepath.Join(options.recurseOutputRoot, "pkg", "*", "*", "*.csproj"))

	for _, library := range append(libraries, libraries2...) {
		if _, stamped := stampedModInfo(t, readGenerated(t, library)); stamped {
			t.Errorf("library project %s carries a build-info stamp", library)
		}
	}

	if len(libraries)+len(libraries2) == 0 {
		t.Errorf("found no dependency project under %s to check", filepath.Join(options.recurseOutputRoot, "pkg"))
	}

	single := options
	single.recurse = false
	single.recurseOutputRoot = ""
	single.programDependencies = nil
	singleOut := filepath.Join(root, "single")

	if err := processConversion(moduleDir, true, singleOut, single); err != nil {
		t.Fatalf("processConversion: %v", err)
	}

	singleProjects, _ := filepath.Glob(filepath.Join(singleOut, "*.csproj"))

	if len(singleProjects) != 1 {
		t.Fatalf("want one single-package project, got %v", singleProjects)
	}

	singleStamp, ok := stampedModInfo(t, readGenerated(t, singleProjects[0]))

	if want := "path\texample.test/prog\nmod\texample.test/prog\t(devel)\t\n"; !ok || singleStamp != want {
		t.Errorf("single-package program stamp = %q (found %v), want %q", singleStamp, ok, want)
	}
}
