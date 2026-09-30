// moduleTestsDriver_integration_test.go - Gbtc
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
	"runtime"
	"strings"
	"sync"
	"testing"
)

// The -tests -recurse driver, driven through the REAL command line: the converter is built from this
// tree once, then run over a hermetic three-package module shaped like golang-jwt/jwt -- a root package,
// a sub-package with an external test, and a test helper imported ONLY from _test.go files.

var (
	driverBinaryOnce sync.Once
	driverBinaryPath string
	driverBinaryErr  error
)

func driverBinary(t *testing.T) string {
	t.Helper()

	driverBinaryOnce.Do(func() {
		dir, err := os.MkdirTemp("", "go2cs-m3-driver")

		if err != nil {
			driverBinaryErr = err
			return
		}

		driverBinaryPath = filepath.Join(dir, "go2cs")

		if runtime.GOOS == "windows" {
			driverBinaryPath += ".exe"
		}

		build := exec.Command("go", "build", "-o", driverBinaryPath, ".")

		if output, buildErr := build.CombinedOutput(); buildErr != nil {
			driverBinaryErr = buildErr
			driverBinaryPath = string(output)
		}
	})

	if driverBinaryErr != nil {
		t.Fatalf("building the converter: %v\n%s", driverBinaryErr, driverBinaryPath)
	}

	return driverBinaryPath
}

// writeJwtLikeModule writes the fixture module with the given `go` line and returns its directory.
func writeJwtLikeModule(t *testing.T, root string, goLine string) string {
	t.Helper()

	moduleDir := filepath.Join(root, "jwtlike")
	depDir := filepath.Join(root, "dep")

	// A third-party dependency (a local replace) WITH its own tests: converted, because production
	// imports it, but never given a test project -- phase B runs over main-module packages only.
	writeModuleFile(t, filepath.Join(depDir, "go.mod"), "module example.test/dep\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(depDir, "prefix.go"), "package dep\n\nfunc Prefix() string {\n\treturn \"signed:\"\n}\n")
	writeModuleFile(t, filepath.Join(depDir, "prefix_test.go"),
		"package dep\n\nimport \"testing\"\n\nfunc TestPrefix(t *testing.T) {\n\tif Prefix() == \"\" {\n\t\tt.Fatal(\"empty\")\n\t}\n}\n")

	// A test-only DEPENDENCY: a separate module imported only from a _test.go file. The production load
	// (`./...` over the module) never reaches it -- unlike the in-module helper testutil below, which
	// `./...` matches -- so only the driver's test-closure discovery converts it.
	testkitDir := filepath.Join(root, "testkit")
	writeModuleFile(t, filepath.Join(testkitDir, "go.mod"), "module example.test/testkit\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(testkitDir, "same.go"),
		"package testkit\n\nfunc Same(a, b string) bool {\n\treturn a == b\n}\n")

	writeModuleFile(t, filepath.Join(moduleDir, "go.mod"),
		"module example.test/jwtlike\n\ngo "+goLine+"\n\nrequire (\n\texample.test/dep v0.0.0\n\texample.test/testkit v0.0.0\n)\n\n"+
			"replace example.test/dep => ../dep\n\nreplace example.test/testkit => ../testkit\n")
	writeModuleFile(t, filepath.Join(moduleDir, "token.go"),
		"package jwtlike\n\nimport \"example.test/dep\"\n\nfunc Sign(claim string) string {\n\treturn dep.Prefix() + claim\n}\n")
	writeModuleFile(t, filepath.Join(moduleDir, "token_test.go"),
		"package jwtlike\n\nimport (\n\t\"testing\"\n\n\t\"example.test/jwtlike/testutil\"\n)\n\n"+
			"func TestSign(t *testing.T) {\n\ttestutil.Expect(t, Sign(\"a\"), \"signed:a\")\n}\n")
	writeModuleFile(t, filepath.Join(moduleDir, "request", "request.go"),
		"package request\n\nimport \"example.test/jwtlike\"\n\nfunc FromHeader(h string) string {\n\treturn jwtlike.Sign(h)\n}\n")
	writeModuleFile(t, filepath.Join(moduleDir, "request", "request_test.go"),
		"package request_test\n\nimport (\n\t\"testing\"\n\n\t\"example.test/jwtlike\"\n\t\"example.test/jwtlike/request\"\n\t\"example.test/jwtlike/testutil\"\n\t\"example.test/testkit\"\n)\n\n"+
			"func TestFromHeader(t *testing.T) {\n\ttestutil.Expect(t, request.FromHeader(\"h\"), jwtlike.Sign(\"h\"))\n\tif !testkit.Same(\"x\", \"x\") {\n\t\tt.Fatal(\"testkit\")\n\t}\n}\n")
	writeModuleFile(t, filepath.Join(moduleDir, "testutil", "expect.go"),
		"package testutil\n\nimport \"testing\"\n\nfunc Expect(t *testing.T, got, want string) {\n\tt.Helper()\n\tif got != want {\n\t\tt.Fatalf(\"got %q, want %q\", got, want)\n\t}\n}\n")

	return moduleDir
}

func runTestsRecurse(t *testing.T, moduleDir string, outRoot string) (string, error) {
	t.Helper()

	treeSrc, err := filepath.Abs("..")

	if err != nil {
		t.Fatal(err)
	}

	cmd := exec.Command(driverBinary(t), "-tests", "-test-action", "convert", "-recurse", "-go2cspath", treeSrc, moduleDir, outRoot)
	cmd.Env = append(os.Environ(), "GOWORK=off", "GOFLAGS=")
	output, err := cmd.CombinedOutput()

	return string(output), err
}

var absolutePathPattern = regexp.MustCompile(`Include="(?:[A-Za-z]:[\\/]|/)`)

// TestTestsRecurseConvertsAModuleAgainstItsTests is arms (a)-(c): the command line accepts -tests
// -recurse; the test-only helper is converted; and every test project references its siblings
// RELATIVELY -- no emitted csproj carries a machine path.
func TestTestsRecurseConvertsAModuleAgainstItsTests(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: builds the converter and converts a module with its tests")
	}

	root := t.TempDir()
	moduleDir := writeJwtLikeModule(t, root, "1.23")
	outRoot := filepath.Join(root, "out")

	// (a) The combination is accepted.
	output, err := runTestsRecurse(t, moduleDir, outRoot)

	if err != nil {
		t.Fatalf("go2cs -tests -recurse failed: %v\n%s", err, output)
	}

	srcRoot := filepath.Join(outRoot, "src", "example.test", "jwtlike")

	// (c) The test-only DEPENDENCY -- a module imported only from a _test.go file, which the production
	// closure never reaches -- is converted into pkg/, and the in-module helper beside its siblings.
	testkitDir := filepath.Join(outRoot, "pkg", "example.test", "testkit")

	if cs, _ := filepath.Glob(filepath.Join(testkitDir, "*.cs")); len(cs) == 0 {
		t.Errorf("the test-only dependency example.test/testkit was not converted (no .cs under %s)", testkitDir)
	}

	if cs, _ := filepath.Glob(filepath.Join(srcRoot, "testutil", "*.cs")); len(cs) == 0 {
		t.Errorf("the in-module helper example.test/jwtlike/testutil was not converted (no .cs under %s)", filepath.Join(srcRoot, "testutil"))
	}

	// Both packages with tests got a test project in their own phase-A directory.
	var testProjects []string

	for _, dir := range []string{srcRoot, filepath.Join(srcRoot, "request")} {
		projects, _ := filepath.Glob(filepath.Join(dir, "*.tests.csproj"))

		if len(projects) != 1 {
			t.Fatalf("want one test project in %s, got %v\n%s", dir, projects, output)
		}

		testProjects = append(testProjects, projects[0])
	}

	// Nothing EXTRA was converted: the go command's synthesized test mains (`p.test`) are scaffolding,
	// and a directory for one means phase A converted it -- and phase B then took it for a package.
	// (Keyed on the exact paths a test main would take: the fixture's own domain directory
	// `example.test` also ends in `.test`, so a suffix match would fire on it.)
	for _, testMain := range []string{srcRoot + ".test", filepath.Join(srcRoot, "request.test")} {
		if _, statErr := os.Stat(testMain); statErr == nil {
			t.Errorf("a synthesized test main was converted: %s", testMain)
		}
	}

	// Phase B ran over MAIN-MODULE packages only: the third-party dependency is converted (production
	// imports it) but has no test project, although it has tests of its own.
	depDir := filepath.Join(outRoot, "pkg", "example.test", "dep")

	if cs, _ := filepath.Glob(filepath.Join(depDir, "*.cs")); len(cs) == 0 {
		t.Errorf("the third-party dependency example.test/dep was not converted into %s", depDir)
	}

	if tests, _ := filepath.Glob(filepath.Join(depDir, "*.tests.csproj")); len(tests) != 0 {
		t.Errorf("phase B converted a THIRD-PARTY package's tests: %v", tests)
	}

	// (b) No machine path in any emitted project, test or production.
	err = filepath.WalkDir(outRoot, func(path string, entry os.DirEntry, walkErr error) error {
		if walkErr != nil || entry.IsDir() || !strings.HasSuffix(path, ".csproj") {
			return walkErr
		}

		data, readErr := os.ReadFile(path)

		if readErr != nil {
			return readErr
		}

		if match := absolutePathPattern.Find(data); match != nil {
			t.Errorf("%s carries an ABSOLUTE reference (%s...):\n%s", path, match, data)
		}

		return nil
	})

	if err != nil {
		t.Fatal(err)
	}

	// Each test project reaches what its tests import -- the root package, the helper -- through the
	// recurse layout, relatively.
	expected := map[string][]string{
		testProjects[0]: {"testutil/example.test.jwtlike.testutil.csproj"},
		testProjects[1]: {"../example.test.jwtlike.csproj", "../testutil/example.test.jwtlike.testutil.csproj",
			"../../../../pkg/example.test/testkit/example.test.testkit.csproj"},
	}

	for project, wants := range expected {
		contents := readGenerated(t, project)

		for _, want := range wants {
			if !strings.Contains(contents, `Include="`+want+`"`) {
				t.Errorf("%s does not reference %s relatively:\n%s", project, want, contents)
			}
		}
	}
}

// TestTestsRecurseRefusesANewerGoLineAndAnInModuleRoot is arms (d) and (e): a module whose go line is
// newer than the corpus release is refused naming both, and so is an output root inside the module.
func TestTestsRecurseRefusesANewerGoLineAndAnInModuleRoot(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: builds the converter")
	}

	root := t.TempDir()
	newer := writeJwtLikeModule(t, filepath.Join(root, "newer"), "1.99")

	output, err := runTestsRecurse(t, newer, filepath.Join(root, "newer-out"))

	if err == nil || !strings.Contains(output, "go 1.99") || !strings.Contains(output, "GoStdLibVersion") {
		t.Errorf("a `go 1.99` module was not refused naming its go line and the corpus release (err=%v):\n%s", err, output)
	}

	current := writeJwtLikeModule(t, filepath.Join(root, "current"), "1.23")

	output, err = runTestsRecurse(t, current, filepath.Join(current, "out"))

	if err == nil || !strings.Contains(output, "inside the module's source tree") {
		t.Errorf("an output root inside the module was not refused (err=%v):\n%s", err, output)
	}
}
