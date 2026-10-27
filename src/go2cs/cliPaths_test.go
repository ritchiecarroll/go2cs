// cliPaths_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards the command line's two path contracts, end to end through main():
//
//   - Single-file input (`go2cs example.go [out.cs]`, the usage text's own examples). The output is ONE .cs file: the
//     file the user named, or <name>.cs inside a directory the user named, or <name>.cs beside the input. Measured on
//     windows and linux before the fix: `go2cs example.go conv/example.cs` wrote conv/example.cs.cs plus a project
//     DIRECTORY conv/example.cs/, and `go2cs example.go` wrote a file named `..cs`. The package's other files are
//     loaded for analysis but never written, since the user asked for one file.
//   - A relative -go2cspath is resolved against the working directory when the command line is parsed. Left
//     relative, it reached MSBuild as written, and dotnet runs from the project directory, so it resolved there.
//
// Each case re-executes this test binary as the converter (TestCLIPathsHelperProcess), so what is asserted is what
// the real command line does, from a real working directory.

package main

import (
	"encoding/json"
	"os"
	"os/exec"
	"path/filepath"
	"strings"
	"testing"
)

const cliPathsHelperEnv = "GO2CS_CLI_PATHS_HELPER"

// The stand-in converter: runs main() with the arguments the case names, a JSON array in the environment. Inert
// unless the environment variable is set, so a normal `go test` run passes it trivially.
func TestCLIPathsHelperProcess(t *testing.T) {
	args, ok := os.LookupEnv(cliPathsHelperEnv)

	if !ok {
		return
	}

	var argv []string

	if err := json.Unmarshal([]byte(args), &argv); err != nil {
		t.Fatal(err)
	}

	os.Args = append([]string{"go2cs"}, argv...)
	main()
	os.Exit(0)
}

// runCLI runs the converter with args from dir and returns its combined output; a non-zero exit fails the test.
func runCLI(t *testing.T, dir string, args ...string) string {
	t.Helper()

	cmd := exec.Command(os.Args[0], "-test.run=^TestCLIPathsHelperProcess$")
	cmd.Dir = dir
	encoded, err := json.Marshal(args)

	if err != nil {
		t.Fatal(err)
	}

	cmd.Env = append(os.Environ(), cliPathsHelperEnv+"="+string(encoded))

	output, err := cmd.CombinedOutput()

	if err != nil {
		t.Fatalf("go2cs %s: %v\n%s", strings.Join(args, " "), err, output)
	}

	return string(output)
}

// newSingleFileModule writes a one-package module: example.go, plus helper.go as a second file of the same package,
// and returns its directory. go2cspath is an existing directory that is not a go2cs root, so the conversion warns
// about it and proceeds.
func newSingleFileModule(t *testing.T) (dir string, go2csPath string) {
	t.Helper()

	dir = t.TempDir()
	go2csPath = t.TempDir()

	files := map[string]string{
		"go.mod":     "module example\n\ngo 1.21\n",
		"example.go": "package main\n\nimport \"fmt\"\n\nfunc main() {\n\tfmt.Println(\"hello from example\")\n\thelperOnly()\n}\n",
		"helper.go":  "package main\n\nimport \"fmt\"\n\nfunc helperOnly() {\n\tfmt.Println(\"hello from helper\")\n}\n",
	}

	for name, text := range files {
		if err := os.WriteFile(filepath.Join(dir, name), []byte(text), 0o644); err != nil {
			t.Fatal(err)
		}
	}

	return dir, go2csPath
}

// assertSingleConvertedFile asserts that path is a regular file holding example.go's conversion and nothing of
// helper.go's.
func assertSingleConvertedFile(t *testing.T, path string) {
	t.Helper()

	info, err := os.Stat(path)

	if err != nil {
		t.Fatalf("expected the converted file %s: %v", path, err)
	}

	if info.IsDir() {
		t.Fatalf("%s is a directory; the named output must be one .cs file", path)
	}

	text, err := os.ReadFile(path)

	if err != nil {
		t.Fatal(err)
	}

	if !strings.Contains(string(text), "hello from example") {
		t.Errorf("%s does not hold example.go's conversion:\n%s", path, text)
	}

	if strings.Contains(string(text), "hello from helper") {
		t.Errorf("%s holds helper.go's conversion; only the named file is written:\n%s", path, text)
	}
}

// assertNoStrayOutput fails on every name a single-file conversion must never write.
func assertNoStrayOutput(t *testing.T, dirs ...string) {
	t.Helper()

	for _, dir := range dirs {
		for _, name := range []string{"..cs", "example.cs.cs", "helper.cs", ".cs"} {
			if _, err := os.Stat(filepath.Join(dir, name)); err == nil {
				t.Errorf("stray output %s", filepath.Join(dir, name))
			}
		}
	}
}

func TestSingleFileNamedOutputIsThatFile(t *testing.T) {
	dir, go2csPath := newSingleFileModule(t)

	if err := os.Mkdir(filepath.Join(dir, "conv"), 0o755); err != nil {
		t.Fatal(err)
	}

	runCLI(t, dir, "-go2cspath", go2csPath, "example.go", filepath.Join("conv", "example.cs"))

	assertSingleConvertedFile(t, filepath.Join(dir, "conv", "example.cs"))
	assertNoStrayOutput(t, dir, filepath.Join(dir, "conv"))

	// The project and package metadata sit beside the converted file, in the directory that holds it.
	for _, name := range []string{"example.csproj", PackageInfoFileName} {
		if _, err := os.Stat(filepath.Join(dir, "conv", name)); err != nil {
			t.Errorf("expected %s beside the converted file: %v", name, err)
		}
	}
}

func TestSingleFileNamedOutputDirectoryTakesTheInputName(t *testing.T) {
	dir, go2csPath := newSingleFileModule(t)

	if err := os.Mkdir(filepath.Join(dir, "conv"), 0o755); err != nil {
		t.Fatal(err)
	}

	runCLI(t, dir, "-go2cspath", go2csPath, "example.go", "conv")

	assertSingleConvertedFile(t, filepath.Join(dir, "conv", "example.cs"))
	assertNoStrayOutput(t, dir, filepath.Join(dir, "conv"))
}

func TestSingleFileDefaultOutputIsBesideTheInput(t *testing.T) {
	dir, go2csPath := newSingleFileModule(t)

	runCLI(t, dir, "-go2cspath", go2csPath, "example.go")

	assertSingleConvertedFile(t, filepath.Join(dir, "example.cs"))
	assertNoStrayOutput(t, dir)
}

func TestRelativeGo2CSPathIsResolvedAtParseTime(t *testing.T) {
	dir, _ := newSingleFileModule(t)

	// A directory that exists but is not a go2cs root, so the converter names the root it resolved in its warning.
	if err := os.Mkdir(filepath.Join(dir, "relroot"), 0o755); err != nil {
		t.Fatal(err)
	}

	output := runCLI(t, dir, "-go2cspath", "relroot", "example.go")

	resolved, err := filepath.Abs(filepath.Join(dir, "relroot"))

	if err != nil {
		t.Fatal(err)
	}

	if !strings.Contains(output, `go2cs runtime root "`+resolved+`"`) {
		t.Errorf("the runtime root was not resolved against the working directory %s; want %q named in:\n%s", dir, resolved, output)
	}
}
