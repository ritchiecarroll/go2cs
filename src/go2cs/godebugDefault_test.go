// godebugDefault_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/build"
	"os"
	"os/exec"
	"path/filepath"
	"runtime"
	"strings"
	"testing"

	"golang.org/x/mod/modfile"
)

func godebugTestOptions() Options {
	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	return Options{goRoot: goRoot}
}

// toolchainDefaultGODEBUG is the toolchain's OWN answer: `go list -f {{.DefaultGODEBUG}}` for the main
// package in dir, from the converting GOROOT's go binary.
func toolchainDefaultGODEBUG(t *testing.T, goRoot string, dir string) string {
	t.Helper()

	goBinary := filepath.Join(goRoot, "bin", "go")

	if runtime.GOOS == "windows" {
		goBinary += ".exe"
	}

	cmd := exec.Command(goBinary, "list", "-f", "{{.DefaultGODEBUG}}", ".")
	cmd.Dir = dir
	cmd.Env = append(os.Environ(), "GOROOT="+goRoot, "GOTOOLCHAIN=local", "GOWORK=off", "GOFLAGS=", "GOFIPS140=off")
	output, err := cmd.CombinedOutput()

	if err != nil {
		t.Fatalf("go list in %s: %v\n%s", dir, err, output)
	}

	return strings.TrimSpace(string(output))
}

// TestDefaultGODEBUGMatchesTheToolchain is the oracle arm: for each fixture main package, the converter's
// reproduction of cmd/go's algorithm must equal what the toolchain itself reports -- a go line older than
// the release (old values for every later change), one at the release (nothing), a go.mod godebug block
// with a `default=` override and an explicit setting, and a //go:debug directive in a source file.
func TestDefaultGODEBUGMatchesTheToolchain(t *testing.T) {
	options := godebugTestOptions()

	fixtures := []struct {
		name   string
		goMod  string
		mainGo string
		empty  bool
	}{
		{name: "go120", goMod: "module example.test/gd\n\ngo 1.20\n"},
		{name: "go123", goMod: "module example.test/gd\n\ngo 1.23\n"},
		{name: "go124", goMod: "module example.test/gd\n\ngo 1.24\n", empty: true},
		// No go line: cmd/go reads the module as go 1.16 (a module-cache release that predates modules).
		{name: "nogoline", goMod: "module example.test/gd\n"},
		{name: "block", goMod: "module example.test/gd\n\ngo 1.23\n\ngodebug (\n\tdefault=go1.21\n\tpanicnil=0\n)\n"},
		{name: "directive", goMod: "module example.test/gd\n\ngo 1.24\n",
			mainGo: "//go:debug panicnil=1\n//go:debug randseednop=0\n\npackage main\n\nfunc main() {}\n"},
	}

	for _, fixture := range fixtures {
		t.Run(fixture.name, func(t *testing.T) {
			dir := t.TempDir()
			mainGo := fixture.mainGo

			if mainGo == "" {
				mainGo = "package main\n\nfunc main() {}\n"
			}

			writeModuleFile(t, filepath.Join(dir, "go.mod"), fixture.goMod)
			writeModuleFile(t, filepath.Join(dir, "main.go"), mainGo)

			want := toolchainDefaultGODEBUG(t, options.goRoot, dir)
			got, err := packageDefaultGODEBUG(dir, []string{filepath.Join(dir, "main.go")}, options)

			if err != nil {
				t.Fatal(err)
			}

			if got != want {
				t.Errorf("DefaultGODEBUG\n got: %q\nwant: %q (go list -f {{.DefaultGODEBUG}})", got, want)
			}

			if fixture.empty != (want == "") {
				t.Errorf("fixture premise broken: the toolchain's answer is %q (empty expected: %v)", want, fixture.empty)
			}
		})
	}
}

// TestRecurseMainProgramCarriesItsDefaultGODEBUG: a main package converted under -recurse is a program of
// its module, so its project stamps the module's DefaultGODEBUG as an assembly attribute; the SAME package
// converted on its own (no -recurse) is unchanged -- the M6 scope ruling, so the behavioral corpus is
// byte-identical.
func TestRecurseMainProgramCarriesItsDefaultGODEBUG(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: converts a module")
	}

	root := t.TempDir()
	moduleDir := filepath.Join(root, "prog")
	writeModuleFile(t, filepath.Join(moduleDir, "go.mod"), "module example.test/prog\n\ngo 1.20\n")
	writeModuleFile(t, filepath.Join(moduleDir, "main.go"), "package main\n\nfunc main() {}\n")

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

	project := readGenerated(t, projects[0])

	if !strings.Contains(project, `<AssemblyAttribute Include="go.GoDefaultGodebugAttribute">`) || !strings.Contains(project, "panicnil=1") {
		t.Errorf("the -recurse program's project does not stamp its go 1.20 DefaultGODEBUG:\n%s", project)
	}

	// The same package, converted on its own: no stamp.
	single := options
	single.recurse = false
	single.recurseOutputRoot = ""
	singleOut := filepath.Join(root, "single")

	if err := processConversion(moduleDir, true, singleOut, single); err != nil {
		t.Fatalf("processConversion: %v", err)
	}

	singleProjects, _ := filepath.Glob(filepath.Join(singleOut, "*.csproj"))

	if len(singleProjects) != 1 {
		t.Fatalf("want one single-package project, got %v", singleProjects)
	}

	if strings.Contains(readGenerated(t, singleProjects[0]), "GoDefaultGodebug") {
		t.Errorf("a plain single-package conversion stamped a DefaultGODEBUG -- out of M6's scope")
	}
}

// TestModuleGodebugReadsTheGodebugBlockStrictly guards the strict-modfile hazard: modfile.ParseLax DROPS
// go.mod `godebug` directives, so reading the block through the lax parser -- the one the converter uses
// for the go line elsewhere -- would silently lose every override. The control proves the hazard is real.
func TestModuleGodebugReadsTheGodebugBlockStrictly(t *testing.T) {
	dir := t.TempDir()
	goMod := "module example.test/gd\n\ngo 1.23\n\ngodebug (\n\tdefault=go1.21\n\tpanicnil=0\n)\n"
	writeModuleFile(t, filepath.Join(dir, "go.mod"), goMod)

	lax, err := modfile.ParseLax("go.mod", []byte(goMod), nil)

	if err != nil {
		t.Fatal(err)
	}

	if len(lax.Godebug) != 0 {
		t.Fatalf("control: ParseLax now keeps godebug directives (%d) -- the hazard this arm guards is gone; revisit", len(lax.Godebug))
	}

	goLine, settings, err := moduleGodebug(dir)

	if err != nil {
		t.Fatal(err)
	}

	if goLine != "1.23" || len(settings) != 2 || settings[0] != (godebugSetting{"default", "go1.21"}) || settings[1] != (godebugSetting{"panicnil", "0"}) {
		t.Errorf("moduleGodebug = %q, %v; want 1.23 and [default=go1.21 panicnil=0]", goLine, settings)
	}
}
