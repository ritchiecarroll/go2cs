// cgoPhase3_test.go - Gbtc
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
	"time"
)

// cgo Phase 3 increment 3.1 (docs/phase4/DESIGN-cgo-phase3-marshaling.md §16): cmd/cgo's own helpers, C.malloc and
// C.free, a void pointer both ways, and the deferred call through the check closure.

// cgoPhase3FixtureDir holds increment 3.1's fixture and the two files cmd/cgo generated from it: the trees the route is
// tested on where no C compiler is.
var cgoPhase3FixtureDir = filepath.Join("testdata", "cgo", "phase3")

// Phase 3's fixture is made and held as Phase 2's is, without cgo_import.go: its probe functions are not linked when
// go list compiles it, so go list hands none back.
func TestTheCgoPhase3FixtureIsCmdCgosOutputForFixtureGo(t *testing.T) {
	requireCmdCgoOutput(t, cgoPhase3FixtureDir, "example.com/cgophase3", "fixture.cgo1.go", "cgo_gotypes.go")
}

// cgoListedImports builds the one-file cgo package source makes, as go list does, and returns the names cmd/cgo's own
// binary imports: the local name of each //go:cgo_import_dynamic line of its _cgo_import.go, which is what the
// verified-names rule reads (cgoImportedByCmdCgo).
func cgoListedImports(t *testing.T, source string) map[string]bool {
	t.Helper()

	dir := t.TempDir()
	writeCgoFixture(t, dir, "go.mod", "module example.com/cgoimports\n\ngo 1.26\n")
	writeCgoFixture(t, dir, "source.go", source)

	list := exec.Command("go", "list", "-compiled", "-json", ".")
	list.Dir = dir
	list.Env = append(os.Environ(), "CGO_ENABLED=1", "GOWORK=off", "GOFLAGS=")
	output, err := list.Output()

	if err != nil {
		t.Fatalf("go list -compiled: %v", err)
	}

	var listed struct {
		Dir             string
		CompiledGoFiles []string
	}

	if err := json.Unmarshal(output, &listed); err != nil {
		t.Fatal(err)
	}

	var imports map[string]bool

	for _, path := range listed.CompiledGoFiles {
		if !filepath.IsAbs(path) {
			path = filepath.Join(listed.Dir, path)
		}

		content, err := os.ReadFile(path)

		if err != nil {
			t.Fatal(err)
		}

		for _, line := range strings.Split(strings.ReplaceAll(string(content), "\r\n", "\n"), "\n") {
			if fields := strings.Fields(line); len(fields) >= 2 && fields[0] == "//go:cgo_import_dynamic" && fields[1] != "_" {
				if imports == nil {
					imports = map[string]bool{}
				}

				imports[fields[1]] = true
			}
		}
	}

	if imports == nil {
		t.Fatalf("go list -compiled handed back no file of dynamic imports: %v", listed.CompiledGoFiles)
	}

	return imports
}

// Two READINGS at the verified-names rule's own input (§16.1 D, §16.4). The rule binds a C name only when cmd/cgo's own
// binary imports a symbol of that name.
//
// strtol, owed from increment 3.0: its char ** parameter is the out-parameter, which is increment 3.2's, so the
// conversion refuses the call before the rule reads it. What the rule WOULD read is printed here: whether the binary
// imports strtol, or the name a C23 glibc header gives it. The test asserts only that the rule's verdict follows that.
//
// malloc and free: the helpers allocate with the package's own malloc, and the caller frees with its own free, so both
// must be names the rule binds. That half is asserted on every host (stop line S3.1-C).
func TestCgoNamesCmdCgosOwnBinaryImportsAreRead(t *testing.T) {
	cgoRealProbeDir(t)

	target := runtime.GOOS + "/" + runtime.GOARCH
	toolchain, _ := cgoRealToolchainLine(t)

	matching := func(imports map[string]bool, pattern string) []string {
		var names []string

		for name := range imports {
			if regexp.MustCompile(pattern).MatchString(name) {
				names = append(names, name)
			}
		}

		slices.Sort(names)

		return names
	}

	t.Run("strtol", func(t *testing.T) {
		imports := cgoListedImports(t, "package cgoimports\n\n// #include <stdlib.h>\nimport \"C\"\n\n// Parse makes the one C call this package reads.\nfunc Parse(text *C.char) int { return int(C.strtol(text, nil, 10)) }\n")
		refused := len(cgoImportedByCmdCgo([]string{"strtol"}, imports, nil)) > 0

		t.Logf("READING %s under %q: cmd/cgo's binary imports %v of the names that hold strtol; the verified-names rule would refuse C.strtol: %t", target, toolchain, matching(imports, `strtol`), refused)

		if refused == imports["strtol"] {
			t.Errorf("the rule's verdict does not follow the imports: strtol imported %t, refused %t", imports["strtol"], refused)
		}
	})

	t.Run("malloc and free", func(t *testing.T) {
		imports := cgoListedImports(t, "package cgoimports\n\n// #include <stdlib.h>\nimport \"C\"\n\nimport \"unsafe\"\n\n// Copy allocates through a helper and frees with the package's own free.\nfunc Copy(s string) string {\n\tcs := C.CString(s)\n\tdefer C.free(unsafe.Pointer(cs))\n\n\treturn C.GoString(cs)\n}\n")

		t.Logf("READING %s under %q: cmd/cgo's binary imports %v of the names that hold malloc or free", target, toolchain, matching(imports, `malloc|free`))

		for _, name := range []string{"malloc", "free"} {
			if !imports[name] {
				t.Errorf("cmd/cgo's binary does not import %s under its plain name, so the helpers cannot bind it (S3.1-C)", name)
			}
		}
	})
}

// cgoP3FixtureDir holds the end-to-end program's sources.
const cgoP3FixtureDir = "testdata/cgo/p3"

// cgoP3GoMemory is in every refusal increment 3.1 makes of a pointer into Go memory: the increment that admits it.
const cgoP3GoMemory = "increment 3.2"

// cgoP3Arms are the end-to-end program's arms. An arm with neither goFails nor csFails prints the same lines and exits
// 0 both ways. The last five hand C or a helper a pointer into Go memory: Go runs each to its end, and go2cs refuses
// each by name, naming the increment that admits it.
func cgoP3Arms() []cgoG1Arm {
	return []cgoG1Arm{
		{name: "text"},
		{name: "roundtrip"},
		{name: "invalid"},
		{name: "nul"},
		{name: "nilstring"},
		{name: "counted"},
		{name: "zero"},
		{name: "negstring"},
		{name: "negbytes"},
		{name: "cbytes"},
		{name: "emptybytes"},
		{name: "copy"},
		{name: "fill"},
		{name: "same"},
		{name: "deferred"},
		{name: "govoid", csFails: cgoP3GoMemory},
		{name: "govoidnumber", csFails: cgoP3GoMemory},
		{name: "govoidexpr", csFails: cgoP3GoMemory},
		{name: "gobytes", csFails: cgoP3GoMemory},
		{name: "gostring", csFails: cgoP3GoMemory},
	}
}

// Increment 3.1's end-to-end test (§16.3): the program is built by go and converted by go2cs on the same host, with the
// same C compiler and the same libraries, and each arm is run both ways. go's side is built and read FIRST, so that how
// Go itself behaves on an arm is on record before go2cs is compared with it, and where the conversion is refused.
func TestCgoPhase3ProgramRunsAsGoDoes(t *testing.T) {
	if os.Getenv(cgoBuildEnv) != "1" {
		t.Skipf("NOT RUN: set %s=1 to build and run the program (Inconclusive, not a pass)", cgoBuildEnv)
	}

	probeDir := cgoRealProbeDir(t)
	target := runtime.GOOS + "/" + runtime.GOARCH

	framework := regexp.MustCompile(`<TargetFramework Condition="[^"]*">([^<]+)</TargetFramework>`).FindStringSubmatch(renderCsprojTemplate("Exe", "", ""))

	if framework == nil {
		t.Fatal("the rendered template names no TargetFramework")
	}

	dotnet := requireDotnetSdk(t, framework[1])
	files := cgoFixtureFiles(t, cgoP3FixtureDir)

	// go's own build, with this host's compiler and the probe on the link line.
	module := t.TempDir()
	writeCgoFixture(t, module, "go.mod", "module example.com/cgop3\n\ngo 1.26\n")

	for name, contents := range files {
		writeCgoFixture(t, module, name, contents)
	}

	binaries := t.TempDir()
	goProgram := filepath.Join(binaries, "p3go")

	if runtime.GOOS == "windows" {
		goProgram += ".exe"
	}

	// On darwin go's linker runs Apple's strip unless DWARF is off, and strip refuses what the pinned zig links
	// (cgoG1_test.go).
	goBuild := []string{"build", "-o", goProgram}

	if runtime.GOOS == "darwin" {
		goBuild = append(goBuild, "-ldflags=-w")
	}

	build := exec.Command("go", append(goBuild, ".")...)
	build.Dir = module
	build.Env = append(os.Environ(), "GOWORK=off", "GOFLAGS=", "CGO_ENABLED=1", "CGO_LDFLAGS=-L"+probeDir+" -lgo2csprobe")

	if out, err := build.CombinedOutput(); err != nil {
		t.Fatalf("go did not build the program: %v\n%s", err, lastLines(string(out), 30))
	}

	libraryEnv := cgoG1LibraryEnv(probeDir)

	if runtime.GOOS == "windows" {
		copyCgoG1File(t, filepath.Join(probeDir, cgoG1ProbeFile()), filepath.Join(binaries, cgoG1ProbeFile()))
	}

	// A directory with nothing in it is where every arm runs.
	empty := t.TempDir()
	arms := cgoP3Arms()
	goRuns := make([]cgoG1Run, len(arms))

	for i, arm := range arms {
		goRuns[i] = runCgoG1(t, []string{goProgram}, arm, empty, libraryEnv)
		t.Logf("READING %s %s: go exit %d %q %s", target, arm.label(), goRuns[i].exit, strings.TrimSpace(goRuns[i].stdout), cgoP3FirstLine(goRuns[i].stderr))
	}

	// The conversion, on this host's real route.
	combined, _, output, err := convertCgoReal(t, probeDir, files)

	if err != nil {
		for _, line := range cgoRefusalLinesIn(combined, "") {
			t.Logf("READING %s: refused: %s", target, line[strings.Index(line, "error "):])
		}

		t.Fatalf("the program did not convert on %s: %v\n%s", target, err, lastLines(combined, 30))
	}

	output = cgoBuildDir(t, output)
	projects, err := filepath.Glob(filepath.Join(output, "*.csproj"))

	if err != nil || len(projects) != 1 {
		t.Fatalf("the conversion wrote %d project files in %s, want 1 (%v)", len(projects), output, err)
	}

	started := time.Now()
	dotnetBuild := exec.Command(dotnet, cgoBuildArgs(t, projects[0], runtime.GOOS, "")...)
	dotnetBuild.Env = cgoBuildEnvironment()
	built, err := dotnetBuild.CombinedOutput()
	buildErrors := cgoCompilerErrors(string(built))

	t.Logf("READING %s: the converted program's build: %d distinct error(s) in %s", target, len(buildErrors), time.Since(started).Round(time.Second))

	for _, line := range buildErrors {
		t.Logf("READING %s: build error %s", target, line)
	}

	if err != nil {
		t.Fatalf("the converted program does not build: %v\n%s", err, lastLines(strings.ReplaceAll(string(built), "\r\n", "\n"), 40))
	}

	assembly := cgoBuiltAssembly(t, output, projects[0])
	csProgram := []string{dotnet, assembly}

	if runtime.GOOS == "windows" {
		copyCgoG1File(t, filepath.Join(probeDir, cgoG1ProbeFile()), filepath.Join(filepath.Dir(assembly), cgoG1ProbeFile()))
	}

	for i, arm := range arms {
		t.Run(arm.label(), func(t *testing.T) {
			goRun := goRuns[i]
			csRun := runCgoG1(t, csProgram, arm, empty, libraryEnv)

			t.Logf("READING %s %s: go exit %d %q; go2cs exit %d %q %s", target, arm.label(), goRun.exit, strings.TrimSpace(goRun.stdout), csRun.exit, strings.TrimSpace(csRun.stdout), cgoP3FirstLine(csRun.stderr))

			if arm.csFails != "" {
				// Go allows the arm, which is the control: the program is valid Go and does what it says.
				if goRun.exit != 0 || goRun.stdout == "" {
					t.Errorf("go was to run this arm to its end, and exited %d:\n%s%s", goRun.exit, goRun.stdout, lastLines(goRun.stderr, 8))
				}

				if csRun.exit == 0 || !strings.Contains(csRun.stderr+csRun.stdout, arm.csFails) {
					t.Errorf("go2cs was to refuse with %q, and exited %d:\n%s%s", arm.csFails, csRun.exit, csRun.stdout, lastLines(csRun.stderr, 8))
				}

				return
			}

			if goRun.exit != csRun.exit || goRun.stdout != csRun.stdout || (goRun.exit == 0 && goRun.stdout == "") {
				t.Errorf("the two runs differ, or printed nothing:\n   go: exit %d %q\n%s\ngo2cs: exit %d %q\n%s", goRun.exit, goRun.stdout, lastLines(goRun.stderr, 6), csRun.exit, csRun.stdout, lastLines(csRun.stderr, 12))
			}
		})
	}
}

// cgoP3FirstLine is the first line a failing run wrote to its standard error, in brackets, or "".
func cgoP3FirstLine(stderr string) string {
	for _, line := range strings.Split(stderr, "\n") {
		if line = strings.TrimSpace(line); line != "" {
			return "[" + line + "]"
		}
	}

	return ""
}
