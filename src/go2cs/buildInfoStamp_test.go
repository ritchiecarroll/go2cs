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

	"golang.org/x/tools/go/packages"
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
	sums := map[string]string{"example.test/a v1.2.3": "h1:aaaa=", "example.test/b v0.4.0": "h1:bbbb="}

	single := programModInfo("example.test/prog/cmd/tool", "example.test/prog", nil, sums)

	if want := "path\texample.test/prog/cmd/tool\nmod\texample.test/prog\t(devel)\t\n"; single != want {
		t.Errorf("single-package stamp = %q, want %q", single, want)
	}

	if got := strings.Join(modInfoLineKeys(t, single), ","); got != "mod,path" {
		t.Errorf("single-package line set = {%s}, want {mod,path}", got)
	}

	recursed := programModInfo("example.test/prog", "example.test/prog", dependencies, sums)

	if got := strings.Join(modInfoLineKeys(t, recursed), ","); got != "dep,mod,path" {
		t.Errorf("-recurse line set = {%s}, want {dep,mod,path}", got)
	}

	// Go's own dep lines (cmd/go, measured at go1.26.10): an ordinary dependency carries its go.sum hash; a replaced
	// one carries no sum column, since Go writes the sum on its `=>` line -- which the stamp does not carry.
	if !strings.HasSuffix(recursed, "dep\texample.test/a\tv1.2.3\th1:aaaa=\ndep\texample.test/b\tv0.4.0\n") {
		t.Errorf("-recurse stamp does not end in Go's dep lines, in path order:\n%q", recursed)
	}

	if strings.Contains(recursed, "=>") {
		t.Errorf("-recurse stamp carries a replacement line, which can name a machine path:\n%q", recursed)
	}

	assertModInfoCarriesNoBuildSettings(t, single)
	assertModInfoCarriesNoBuildSettings(t, recursed)

	// Outside a module there is nothing to stamp: a GOPATH-mode Go binary embeds no module info, and its import
	// path can be a machine path.
	if got := programModInfo("_/C_/somewhere/prog", "", dependencies, sums); got != "" {
		t.Errorf("a package outside any module was stamped: %q", got)
	}
}

// TestGoSumHashesReadsModuleLinesOnly: a dep's sum is its module-content `h1:` line from go.sum, never the
// `/go.mod` line beside it, and a module with no go.sum reads as no sums.
func TestGoSumHashesReadsModuleLinesOnly(t *testing.T) {
	moduleDir := t.TempDir()

	if got := goSumHashes(moduleDir); len(got) != 0 {
		t.Errorf("a module with no go.sum read %v", got)
	}

	goSum := "golang.org/x/mod v0.42.0 h1:ICeL3t2L30ljSPHHxDnR4fqVcBt/zR2zJAb9juavmQg=\r\n" +
		"golang.org/x/mod v0.42.0/go.mod h1:gomodlinehash=\r\n" +
		"golang.org/x/sync v0.1.0/go.mod h1:onlygomod=\r\n"

	if err := os.WriteFile(filepath.Join(moduleDir, "go.sum"), []byte(goSum), 0o644); err != nil {
		t.Fatal(err)
	}

	got := goSumHashes(moduleDir)
	want := map[string]string{"golang.org/x/mod v0.42.0": "h1:ICeL3t2L30ljSPHHxDnR4fqVcBt/zR2zJAb9juavmQg="}

	if len(got) != len(want) || got["golang.org/x/mod v0.42.0"] != want["golang.org/x/mod v0.42.0"] {
		t.Errorf("goSumHashes = %v, want %v", got, want)
	}

	stamp := programModInfo("example.test/prog", "example.test/prog", []thirdPartyModule{{path: "golang.org/x/mod", version: "v0.42.0"}}, got)

	if !strings.HasSuffix(stamp, "dep\tgolang.org/x/mod\tv0.42.0\th1:ICeL3t2L30ljSPHHxDnR4fqVcBt/zR2zJAb9juavmQg=\n") {
		t.Errorf("the stamp does not carry the go.sum hash Go embeds:\n%q", stamp)
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

	stamp := programModInfo("example.test/prog", "example.test/prog", []thirdPartyModule{{path: "example.test/a&b", version: "v1.0.0"}}, map[string]string{"example.test/a&b v1.0.0": "h1:x="})

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

	// The replaced dependency, as cmd/go writes its dep line (measured at go1.26.10): no sum column.
	if want := "path\texample.test/prog\nmod\texample.test/prog\t(devel)\t\ndep\texample.test/dep\tv0.1.0\n"; !ok || stamp != want {
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

// A Go test binary of a MAIN package copies the package's own build info, so it lists the modules the package's
// production import closure links, outside the main module, once each, sorted (cmd/go setBuildInfo). A test of any
// other package lists none (go1.26.10). The converted host gives the same answer: a standard-library package has no
// module and is skipped, a module reached twice is listed once, a test-only import is not linked into the copied
// build info, and a replaced module keeps Go's dep line without the sum column (the `=>` line is not stamped).
func TestTestBinaryDependenciesFollowGoTestMain(t *testing.T) {
	main := &packages.Module{Path: "example.com/m", Main: true}
	sync := &packages.Module{Path: "golang.org/x/sync", Version: "v0.24.0"}
	text := &packages.Module{Path: "golang.org/x/text", Version: "v0.3.0", Replace: &packages.Module{Path: "../text"}}

	stdlib := &packages.Package{PkgPath: "fmt"}
	textPkg := &packages.Package{PkgPath: "golang.org/x/text/language", Module: text, Imports: map[string]*packages.Package{"fmt": stdlib}}
	syncPkg := &packages.Package{PkgPath: "golang.org/x/sync/errgroup", Module: sync, Imports: map[string]*packages.Package{"golang.org/x/text/language": textPkg}}
	internal := &packages.Package{PkgPath: "example.com/m/internal", Module: main, Imports: map[string]*packages.Package{"golang.org/x/sync/errgroup": syncPkg}}
	program := &packages.Package{Name: "main", PkgPath: "example.com/m/cmd/p", Module: main, Imports: map[string]*packages.Package{"example.com/m/internal": internal, "fmt": stdlib, "golang.org/x/text/language": textPkg}}

	sums := map[string]string{"golang.org/x/sync v0.24.0": "h1:sync=", "golang.org/x/text v0.3.0": "h1:text="}

	got := testBinaryDependencies(program, "example.com/m", sums)
	want := []string{"golang.org/x/sync\tv0.24.0\th1:sync=", "golang.org/x/text\tv0.3.0"}

	if strings.Join(got, "|") != strings.Join(want, "|") {
		t.Fatalf("dependencies = %q, want %q", got, want)
	}

	library := &packages.Package{Name: "m", PkgPath: "example.com/m", Module: main, Imports: map[string]*packages.Package{"example.com/m/internal": internal}}

	if deps := testBinaryDependencies(library, "example.com/m", sums); len(deps) != 0 {
		t.Fatalf("a non-main package's test binary lists no deps in Go, got %q", deps)
	}

	if deps := testBinaryDependencies(&packages.Package{Name: "main", PkgPath: "cmd/x", Imports: map[string]*packages.Package{"fmt": stdlib}}, "", nil); len(deps) != 0 {
		t.Fatalf("a standard-library test binary links no module, got %q", deps)
	}
}

// A module package's test host carries its dependency lines as the registry's last argument, each field separated by
// a C# `\t` escape (a raw tab or newline cannot sit in a C# string literal), so ReadBuildInfo reports Go's Deps. A host
// with no dependencies keeps exactly the shape it had, which is every standard-library host.
func TestTestHostCarriesModuleDependencies(t *testing.T) {
	dir := t.TempDir()

	if err := writeTestHost(dir, "go", "example.com/m", nil, nil, []string{"m_test.go"}, nil, nil, "example.com/m", []string{"golang.org/x/sync\tv0.24.0\th1:sync="}, ""); err != nil {
		t.Fatal(err)
	}

	data, err := os.ReadFile(filepath.Join(dir, testHostFileName))
	if err != nil {
		t.Fatal(err)
	}

	contents := string(data)

	if !strings.Contains(contents, "        }, \"example.com/m\", new string[]\r\n        {\r\n            \"golang.org/x/sync\\tv0.24.0\\th1:sync=\",\r\n        });\r\n") {
		t.Fatalf("the module host must pass its dependency lines after its module path:\n%s", contents)
	}

	if strings.ContainsRune(contents, '\t') {
		t.Fatalf("a raw tab reached the host's C# source:\n%s", contents)
	}

	bare := t.TempDir()

	if err := writeTestHost(bare, "go", "example.com/m", nil, nil, []string{"m_test.go"}, nil, nil, "example.com/m", nil, ""); err != nil {
		t.Fatal(err)
	}

	data, err = os.ReadFile(filepath.Join(bare, testHostFileName))
	if err != nil {
		t.Fatal(err)
	}

	if !strings.Contains(string(data), "        }, \"example.com/m\");\r\n") || strings.Count(string(data), "new string[]") != 3 {
		t.Fatalf("a module host with no dependencies must keep its shape:\n%s", data)
	}
}

// A MAIN package's tests convert like any other's: go/packages returns the package itself (ID = its import path),
// its test variant (`p [p.test]`) and the synthetic test main (`p.test`), and all three are package main. The
// production package is the bare one; only the synthetic `.test` main and the bracketed variants are skipped. A filter
// on the package NAME skipped the real main package too, and a module with a tested main package failed its -tests run.
func TestFindProductionPackageKeepsAMainPackage(t *testing.T) {
	dir := t.TempDir()
	production := &packages.Package{ID: "example.com/m/cmd/p", Name: "main", PkgPath: "example.com/m/cmd/p", Dir: dir}
	loaded := []*packages.Package{
		{ID: "example.com/m/cmd/p.test", Name: "main", PkgPath: "example.com/m/cmd/p.test", Dir: dir},
		{ID: "example.com/m/cmd/p [example.com/m/cmd/p.test]", Name: "main", PkgPath: "example.com/m/cmd/p", Dir: dir},
		production,
	}

	if got := findProductionPackage(loaded, dir); got != production {
		t.Fatalf("production package = %v, want the bare main package %q", got, production.ID)
	}
}
