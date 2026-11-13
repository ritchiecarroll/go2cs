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
	"regexp"
	"runtime"
	"slices"
	"strings"
	"testing"
)

// cgo Phase 2 increment 5b (docs/phase4/DESIGN-cgo-phase2-pinvoke.md §11.20): the cgo route on a real host. go list runs
// cmd/cgo with the host's C compiler, the converter captures each C symbol's library through that compiler, and the
// bindings name the libraries it found. It runs where a C compiler and the probe library are (the hosted converter-cgo
// legs build the probe, GO2CS_CGO_PROBE_DIR), and the Phase 2 fixture reaches the probe through CGO_LDFLAGS, so its
// checked-in cmd/cgo output stays as it is.

// cgoRealProbeDir is the probe library's directory, or a skip where no leg built it. A leg that built one requires it
// (GO2CS_REQUIRE_CGO_PROBE=1), and there a missing probe or a missing C compiler FAILS: the requirement is read first,
// and the compiler is the one go itself would use (go env CC), so a leg that loses its compiler cannot skip green.
func cgoRealProbeDir(t *testing.T) string {
	t.Helper()

	required := os.Getenv("GO2CS_REQUIRE_CGO_PROBE") == "1"
	dir := os.Getenv("GO2CS_CGO_PROBE_DIR")

	if dir == "" {
		if required {
			t.Fatal("GO2CS_REQUIRE_CGO_PROBE=1 and no GO2CS_CGO_PROBE_DIR: the leg built no probe library")
		}

		t.Skip("no probe library here (GO2CS_CGO_PROBE_DIR): NOT MEASURED")
	}

	command := ""

	if out, err := exec.Command("go", "env", "CC").Output(); err == nil {
		command = strings.TrimSpace(string(out))
	}

	fields, err := splitCgoCommand(command)

	if err == nil {
		_, err = exec.LookPath(fields[0])
	}

	if err != nil {
		if required {
			t.Fatalf("GO2CS_REQUIRE_CGO_PROBE=1, and the C compiler go would use (%q) does not run here: %v", command, err)
		}

		t.Skipf("the C compiler go would use (%q) does not run here: NOT MEASURED", command)
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

	// The compiler is resolved as the route resolves it (cgoCapture): a bare name is PATH's, by its full path. The
	// name a compiler prints for itself is the one it was run by, so gcc on windows prints "gcc.exe (...)" run by
	// its path and "gcc (...)" run as "gcc". Run by the bare name here, this expected a line the route never writes
	// (the runner's own MinGW gcc, run 43202800197; a pinned toolchain's CC is already a full path).
	if !strings.ContainsAny(fields[0], `/\`) {
		if resolved, lookErr := exec.LookPath(fields[0]); lookErr == nil {
			fields[0] = resolved
		}
	}

	version, err := exec.Command(fields[0], append(fields[1:], "--version")...).Output()

	if err != nil {
		t.Fatalf("%v --version: %v", fields, err)
	}

	return cgoToolchainLine(string(version)), fields
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

	// cmd/cgo compiles its wrappers at -O2, so a compiler builtin may be inlined there and import nothing, and the route
	// binds only a name cmd/cgo's own binary imports (§11.20, the review's M3). Which calls that refuses is each host's
	// compiler's to say, so it is read here, and held to the two builtins the fixture calls.
	t.Run("a call cmd/cgo's binary imports nothing for is refused by name", func(t *testing.T) {
		stderr, _, output, err := convertCgoReal(t, probeDir, map[string]string{"fixture.go": withoutGoFuncs(t, fixture, refused...)})

		if err == nil {
			t.Logf("READING %s: no call was refused as not imported by cmd/cgo's binary", target)
			return
		}

		requireNothingWritten(t, output, err, stderr)

		functions := map[string]string{"abs": "Abs", "labs": "Labs"}
		notImported := regexp.MustCompile(`C\.(\w+): cmd/cgo's own binary does not import`)
		found := 0

		for _, line := range cgoRefusalLines(stderr) {
			match := notImported.FindStringSubmatch(line)

			if match == nil {
				t.Errorf("a refusal that is not the not-imported one: %s", line)
				continue
			}

			found++
			t.Logf("READING %s: %s", target, line[strings.Index(line, "C."):])

			if function, builtin := functions[match[1]]; builtin {
				refused = append(refused, function)
			} else {
				t.Errorf("C.%s is refused as not imported, and it is not one of the compiler builtins the fixture calls (abs, labs)", match[1])
			}
		}

		if found == 0 {
			t.Errorf("the conversion failed with no refusal line\n%s", stderr)
		}
	})

	trimmed := withoutGoFuncs(t, fixture, refused...)

	t.Run("a plain file under the cgo tag converts with the package", func(t *testing.T) {
		stderr, _, output, err := convertCgoReal(t, probeDir, map[string]string{
			"fixture.go": trimmed,
			"cgotag.go":  "//go:build cgo\n\npackage cgophase2\n\n// Tagged is declared in a plain file the cgo tag selects.\nfunc Tagged() bool { return true }\n",
		})

		if err != nil {
			t.Fatalf("the conversion failed: %v\n%s", err, stderr)
		}

		if !slices.Contains(emittedProjectFiles(t, output), "cgotag.cs") {
			t.Errorf("the plain file under //go:build cgo was not converted: %v", emittedProjectFiles(t, output))
		}
	})

	t.Run("a function a header renames is never bound by its plain name", func(t *testing.T) {
		stderr, _, output, err := convertCgoReal(t, probeDir, map[string]string{"size.go": "package cgoreal\n\n/*\n#include <sys/stat.h>\n*/\nimport \"C\"\n\nfunc Size() int64 {\n\tvar st C.struct_stat\n\tC.fstat(0, &st)\n\treturn int64(st.st_size)\n}\n"})

		// darwin/amd64's <sys/stat.h> renames fstat to fstat$INODE64, a different function with a different struct stat.
		if target == "darwin/amd64" {
			requireNothingWritten(t, output, err, stderr)
			requireCgoUseRefused(t, cgoRefusalLinesIn(stderr, "size.go"), "size.go(10,", "C.fstat", "fstat$INODE64")

			for _, line := range cgoRefusalLinesIn(stderr, "size.go") {
				t.Logf("READING %s: %s", target, line[strings.Index(line, "error "):])
			}

			return
		}

		if err != nil {
			for _, line := range cgoRefusalLinesIn(stderr, "size.go") {
				t.Logf("READING %s: C.fstat refused: %s", target, line[strings.Index(line, "error "):])
			}

			if len(cgoRefusalLinesIn(stderr, "size.go")) == 0 {
				lines := strings.Split(strings.TrimSpace(strings.ReplaceAll(stderr, "\r\n", "\n")), "\n")
				t.Logf("READING %s: the C.fstat package did not convert, with no cgo refusal line (%v); its last line: %s", target, err, lines[len(lines)-1])
			}

			return
		}

		for _, file := range emittedProjectFiles(t, output) {
			for _, line := range strings.Split(readConverted(t, output, file), "\n") {
				if strings.Contains(line, "LibraryImport(") && strings.Contains(line, "fstat") {
					t.Logf("READING %s: C.fstat bound as %s", target, strings.TrimSpace(line))
				}
			}
		}
	})

	t.Run("a library that does not link is named, and no symbol is blamed for it", func(t *testing.T) {
		stderr, _, output, err := convertCgoReal(t, probeDir, map[string]string{"cosine.go": "package cgoreal\n\n/*\n#cgo LDFLAGS: -lm -lgo2csnosuchlib\n#include <math.h>\n*/\nimport \"C\"\n\nfunc Cosine(x float64) float64 { return float64(C.cos(C.double(x))) }\n"})
		requireNothingWritten(t, output, err, stderr)

		if !strings.Contains(stderr, "go2csnosuchlib") {
			t.Errorf("the refusal does not name the library that did not link\n%s", stderr)
		}

		if strings.Contains(stderr, "Phase 4") {
			t.Errorf("a link failure is blamed on the package's C functions as Phase 4's\n%s", stderr)
		}
	})

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
				t.Errorf("the bindings file does not carry %q; its header reads:\n%s", want, strings.Join(cgoHeaderLines(bindings), "\n"))
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

// The review of increment 5b, S6: on a leg that requires the probe (GO2CS_REQUIRE_CGO_PROBE=1), a missing C compiler
// FAILS the real-route test. It used to skip, since the compiler check ran first and looked for gcc, cc or clang on
// PATH, so a leg that lost its compiler read green. The test is run again here, in a process that requires the probe and
// whose compiler (go env CC) does not exist, with PATH holding go alone.
func TestTheRealRouteTestFailsWhenARequiredCompilerIsMissing(t *testing.T) {
	goRoot, err := exec.Command("go", "env", "GOROOT").Output()

	if err != nil {
		t.Fatal(err)
	}

	cmd := exec.Command(os.Args[0], "-test.run=^TestTheCgoRouteConvertsTheFixtureOnARealHost$", "-test.v")
	cmd.Env = append(os.Environ(),
		"GO2CS_REQUIRE_CGO_PROBE=1",
		"GO2CS_CGO_PROBE_DIR="+t.TempDir(),
		"CC=go2cs-no-such-compiler",
		"PATH="+filepath.Join(strings.TrimSpace(string(goRoot)), "bin"))
	output, err := cmd.CombinedOutput()

	if err == nil || !strings.Contains(string(output), "--- FAIL: TestTheCgoRouteConvertsTheFixtureOnARealHost") {
		t.Errorf("with the probe required and no C compiler, the real-route test did not fail (%v):\n%s", err, output)
	}

	if strings.Contains(string(output), "--- SKIP") {
		t.Errorf("with the probe required and no C compiler, the real-route test skipped:\n%s", output)
	}
}

// cgoHeaderLines are a bindings file's leading comment lines that record its target and its C toolchain.
func cgoHeaderLines(bindings string) []string {
	var lines []string

	for _, line := range strings.Split(bindings, "\n") {
		if strings.HasPrefix(line, "// target: ") || strings.HasPrefix(line, "// C toolchain: ") {
			lines = append(lines, line)
		}
	}

	return lines
}
