// cgoRealRoute_test.go - Gbtc
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
	"runtime"
	"strings"
	"testing"
)

// cgo Phase 2 increment 5b (docs/phase4/DESIGN-cgo-phase2-pinvoke.md §11.20): the cgo route on a real host. go list runs
// cmd/cgo with the host's C compiler, the converter captures each C symbol's library through that compiler, and the
// bindings name the libraries it found. It runs where a C compiler and the probe library are (the hosted converter-cgo
// legs build the probe, GO2CS_CGO_PROBE_DIR), and the Phase 2 fixture reaches the probe through CGO_LDFLAGS, so its
// checked-in cmd/cgo output stays as it is.

// cgoRealProbeDir is the probe library's directory, or a skip where no leg built it.
func cgoRealProbeDir(t *testing.T) string {
	t.Helper()
	requireCCompiler(t)

	dir := os.Getenv("GO2CS_CGO_PROBE_DIR")

	if dir == "" {
		if os.Getenv("GO2CS_REQUIRE_CGO_PROBE") == "1" {
			t.Fatal("GO2CS_REQUIRE_CGO_PROBE=1 and no GO2CS_CGO_PROBE_DIR: the leg built no probe library")
		}

		t.Skip("no probe library here (GO2CS_CGO_PROBE_DIR): NOT MEASURED")
	}

	return dir
}

// cgoRealToolchainLine is the first line the host's C compiler prints for --version, which the bindings' header records.
func cgoRealToolchainLine(t *testing.T) (string, []string) {
	t.Helper()

	out, err := exec.Command("go", "env", "CC").Output()

	if err != nil {
		t.Fatal(err)
	}

	fields, err := splitCgoCommand(strings.TrimSpace(string(out)))

	if err != nil {
		t.Fatal(err)
	}

	version, err := exec.Command(fields[0], append(fields[1:], "--version")...).Output()

	if err != nil {
		t.Fatalf("%v --version: %v", fields, err)
	}

	return strings.TrimSpace(strings.SplitN(strings.ReplaceAll(string(version), "\r\n", "\n"), "\n", 2)[0]), fields
}

// convertCgoReal converts the package files make, in a module of its own, under -cgo with the probe library linked
// through CGO_LDFLAGS, and returns its combined output, its module and output directories and the exit error.
func convertCgoReal(t *testing.T, probeDir string, files map[string]string, args ...string) (string, string, string, error) {
	t.Helper()

	treeSrc, err := filepath.Abs("..")

	if err != nil {
		t.Fatal(err)
	}

	dir := t.TempDir()
	writeCgoFixture(t, dir, "go.mod", "module example.com/cgoreal\n\ngo 1.26\n")

	for name, contents := range files {
		writeCgoFixture(t, dir, name, contents)
	}

	output := t.TempDir()
	cmd := exec.Command(driverBinary(t), append(append([]string{"-cgo=true", "-comments", "-go2cspath", treeSrc}, args...), dir, output)...)
	cmd.Env = append(os.Environ(), "GOWORK=off", "GOFLAGS=", "CGO_ENABLED=", "CGO_LDFLAGS=-L"+probeDir+" -lgo2csprobe")
	combined, err := cmd.CombinedOutput()

	return string(combined), dir, output, err
}

// cgoRealProbeName is the probe library's load-time name as each OS's capture reads it.
func cgoRealProbeName(probeDir string) string {
	switch runtime.GOOS {
	case "windows":
		return "go2csprobe.dll"
	case "darwin":
		return probeDir + "/libgo2csprobe.dylib"
	default:
		return "libgo2csprobe.so"
	}
}

func TestTheCgoRouteConvertsTheFixtureOnARealHost(t *testing.T) {
	probeDir := cgoRealProbeDir(t)
	target := runtime.GOOS + "/" + runtime.GOARCH
	source, err := os.ReadFile(filepath.Join(cgoPhase2FixtureDir, "fixture.go"))

	if err != nil {
		t.Fatal(err)
	}

	fixture := strings.ReplaceAll(string(source), "\r\n", "\n")

	t.Run("the fixture refuses its deferred and goroutine calls", func(t *testing.T) {
		stderr, _, output, err := convertCgoReal(t, probeDir, map[string]string{"fixture.go": fixture})
		requireNothingWritten(t, output, err, stderr)

		lines := cgoRefusalLines(stderr)
		requireCgoUseRefused(t, lines, "fixture.go(106,", "C.go2cs_probe_node_touch", "deferred", "Phase 2 later")
		requireCgoUseRefused(t, lines, "fixture.go(113,", "C.go2cs_probe_node_touch", "goroutine", "Phase 2 later")

		if len(lines) != 2 {
			t.Errorf("%d refusal lines, want 2:\n%s", len(lines), stderr)
		}
	})

	// llvm-mingw's C runtime resolves mktime statically (converter-cgo 43148958897, windows), and Phase 2 binds only a
	// shared library's export, so on windows the copy that converts leaves Mktime out, and the next subtest holds the
	// refusal.
	refused := []string{"Deferred", "Started"}

	if runtime.GOOS == "windows" {
		refused = append(refused, "Mktime")

		t.Run("a C function the C runtime resolves statically is refused at its use", func(t *testing.T) {
			stderr, _, output, err := convertCgoReal(t, probeDir, map[string]string{"fixture.go": withoutGoFuncs(t, fixture, "Deferred", "Started")})
			requireNothingWritten(t, output, err, stderr)
			requireCgoUseRefused(t, cgoRefusalLines(stderr), "fixture.go(74,", "C.mktime", "statically")
		})
	}

	trimmed := withoutGoFuncs(t, fixture, refused...)

	t.Run("a copy without them converts, naming each library the C compiler found", func(t *testing.T) {
		toolchain, cc := cgoRealToolchainLine(t)
		stderr, dir, output, err := convertCgoReal(t, probeDir, map[string]string{"fixture.go": trimmed})

		if err != nil {
			t.Fatalf("the conversion failed: %v\n%s", err, stderr)
		}

		bindingsPath := filepath.Join(strings.ReplaceAll(target, "/", "-"), "cgophase2.cgo.cs")
		bindings := readConverted(t, output, bindingsPath)

		for _, want := range []string{
			"// target: " + target + "\n",
			"// C toolchain: " + toolchain + "\n",
			`GoCgoLibraries.Register(typeof(cgophase2ꓸC_package).Assembly, "` + target + `",`,
			`"` + cgoRealProbeName(probeDir) + `")`,
		} {
			if !strings.Contains(bindings, want) {
				t.Errorf("the bindings file does not carry %q", want)
			}
		}

		for _, line := range strings.Split(bindings, "\n") {
			if strings.Contains(line, "new global::go.golib.GoCgoLibrary(") {
				t.Logf("READING %s library: %s", target, strings.TrimSpace(line))
			}
		}

		// No path of the machine that converted reaches the emission, beyond a captured library's own name (a darwin
		// install name is absolute by rule, §11.12): not the module, the output, GOROOT, the probe's directory or the
		// C compiler's path (COORD's condition 3 on the 5b plan).
		goRoot, _ := exec.Command("go", "env", "GOROOT").Output()
		forbidden := []string{dir, output, strings.TrimSpace(string(goRoot)), probeDir}

		if filepath.IsAbs(cc[0]) {
			forbidden = append(forbidden, filepath.Dir(cc[0]))
		}

		// A captured library's own name is its identity wherever the bindings write it (the table's row, its binding's
		// [LibraryImport], the comment above), so each is taken out of a line before the line is searched.
		var captured []string

		for _, line := range strings.Split(bindings, "\n") {
			if _, row, found := strings.Cut(line, "new global::go.golib.GoCgoLibrary("); found {
				if fields := strings.Split(row, `"`); len(fields) >= 4 {
					captured = append(captured, fields[3])
				}
			}
		}

		found := 0

		for _, file := range emittedProjectFiles(t, output) {
			for _, line := range strings.Split(readConverted(t, output, file), "\n") {
				for _, name := range captured {
					line = strings.ReplaceAll(line, name, "")
				}

				for _, path := range forbidden {
					for _, spelling := range []string{path, filepath.ToSlash(path)} {
						if spelling != "" && strings.Contains(line, spelling) {
							found++
							t.Errorf("%s writes a path of the converting machine (%s): %s", file, spelling, strings.TrimSpace(line))
						}
					}
				}
			}
		}

		t.Logf("READING build-machine paths in the emission, beyond captured library names: %d (checked: the module, the output, GOROOT, the probe directory, the C compiler's directory)", found)

		// §11.20's _cgo_import.go prediction: with the probe linked, does go list hand back cmd/cgo's dynamic imports?
		list := exec.Command("go", "list", "-compiled", "-json", ".")
		list.Dir = dir
		list.Env = append(os.Environ(), "CGO_ENABLED=1", "GOWORK=off", "GOFLAGS=", "CGO_LDFLAGS=-L"+probeDir+" -lgo2csprobe")
		listed, err := list.Output()

		if err != nil {
			t.Fatalf("go list -compiled: %v", err)
		}

		var pkg struct {
			Dir             string
			CompiledGoFiles []string
		}

		if err := json.Unmarshal(listed, &pkg); err != nil {
			t.Fatal(err)
		}

		dynamic := false

		for _, path := range pkg.CompiledGoFiles {
			if !filepath.IsAbs(path) {
				path = filepath.Join(pkg.Dir, path)
			}

			if content, err := os.ReadFile(path); err == nil && strings.Contains(string(content), "//go:cgo_import_dynamic") {
				dynamic = true
			}
		}

		t.Logf("READING %s: go list handed back cmd/cgo's _cgo_import.go with the probe linked: %t", target, dynamic)
	})

	t.Run("a function the preamble defines is Phase 4's", func(t *testing.T) {
		stderr, _, output, err := convertCgoReal(t, probeDir, map[string]string{"add.go": "package cgoreal\n\n/*\nstatic int add(int a, int b) { return a + b; }\n*/\nimport \"C\"\n\nfunc Add() int { return int(C.add(1, 2)) }\n"})
		requireNothingWritten(t, output, err, stderr)

		if !strings.Contains(stderr, "C.add") || !strings.Contains(stderr, "Phase 4") {
			t.Errorf("the refusal does not name C.add as Phase 4's\n%s", stderr)
		}
	})

	t.Run("another target is refused naming it and the host", func(t *testing.T) {
		other := "linux/amd64"

		if runtime.GOOS == "linux" {
			other = "darwin/amd64"
		}

		stderr, _, output, err := convertCgoReal(t, probeDir, map[string]string{"fixture.go": trimmed}, "-platforms", other)
		requireNothingWritten(t, output, err, stderr)

		if !strings.Contains(stderr, other) || !strings.Contains(stderr, target) {
			t.Errorf("the refusal does not name both the target asked for (%s) and the host (%s)\n%s", other, target, stderr)
		}
	})
}
