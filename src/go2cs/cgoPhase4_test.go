// cgoPhase4_test.go - Gbtc
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
	"testing"
	"time"
)

// cgo Phase 4, the preamble-only slice (docs/phase4/DESIGN-cgo-phase4-sidebuild.md §3): a function the preamble
// DEFINES is compiled into a side-built library beside the bindings and called through its address table.

// cgoPhase4Program defines three functions in its preamble, none of which any library exports: PLAN §2's static
// add_ints, a non-static definition (twice), and a static that sets errno (halve), called in its errno form.
const cgoPhase4Program = `package main

/*
#include <errno.h>
static int add_ints(int a, int b) { return a + b; }
static int halve(int v) { if (v % 2 != 0) { errno = EINVAL; return -1; } return v / 2; }
int twice(int v) { return 2 * v; }
*/
import "C"

import "fmt"

func main() {
	fmt.Println("add_ints", C.add_ints(2, 3))
	fmt.Println("twice", C.twice(21))
	h, err := C.halve(8)
	fmt.Println("halve 8", h, err == nil)
	h, err = C.halve(7)
	fmt.Println("halve 7", h, err != nil)
}
`

// The design's §3 red. Arm 1 converts on every leg with a C compiler and the probe; arm 2 builds the converted program
// and runs it against go's own build where the build tests are on (cgo-e2e, GO2CS_CGO_BUILD=1).
func TestCgoPhase4APreambleDefinedFunctionConvertsAndRunsAsGoDoes(t *testing.T) {
	probeDir := cgoRealProbeDir(t)
	target := runtime.GOOS + "/" + runtime.GOARCH
	files := map[string]string{"main.go": cgoPhase4Program}

	combined, _, output, err := convertCgoReal(t, probeDir, files)

	if err != nil {
		for _, line := range cgoRefusalLinesIn(combined, "") {
			t.Logf("READING %s: refused: %s", target, line[strings.Index(line, "error "):])
		}

		t.Fatalf("a program whose preamble defines the C it calls did not convert on %s: %v\n%s", target, err, lastLines(combined, 30))
	}

	// The side library and its translation unit, beside the bindings (design §2.4).
	sideDir := filepath.Join(cgoBuildDir(t, output), "cgo", strings.ReplaceAll(target, "/", "-"))

	if units, _ := filepath.Glob(filepath.Join(sideDir, "*.side.c")); len(units) != 1 {
		t.Errorf("the conversion wrote %d side translation units in %s, want 1", len(units), sideDir)
	}

	if os.Getenv(cgoBuildEnv) != "1" {
		t.Skipf("NOT RUN: set %s=1 to build and run the program (Inconclusive, not a pass)", cgoBuildEnv)
	}

	framework := regexp.MustCompile(`<TargetFramework Condition="[^"]*">([^<]+)</TargetFramework>`).FindStringSubmatch(renderCsprojTemplate("Exe", "", ""))

	if framework == nil {
		t.Fatal("the rendered template names no TargetFramework")
	}

	dotnet := requireDotnetSdk(t, framework[1])

	// go's own build, with this host's compiler, in the same environment the conversion read.
	module := t.TempDir()
	writeCgoFixture(t, module, "go.mod", "module example.com/cgop4\n\ngo 1.26\n")
	writeCgoFixture(t, module, "main.go", cgoPhase4Program)

	binaries := t.TempDir()
	goProgram := filepath.Join(binaries, "p4go")

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

	empty := t.TempDir()
	libraryEnv := cgoG1LibraryEnv(probeDir)
	arm := cgoG1Arm{name: "all"}
	goRun := runCgoG1(t, []string{goProgram}, arm, empty, libraryEnv)

	projects, err := filepath.Glob(filepath.Join(cgoBuildDir(t, output), "*.csproj"))

	if err != nil || len(projects) != 1 {
		t.Fatalf("the conversion wrote %d project files, want 1 (%v)", len(projects), err)
	}

	started := time.Now()
	dotnetBuild := exec.Command(dotnet, cgoBuildArgs(t, projects[0], runtime.GOOS, "")...)
	dotnetBuild.Env = cgoBuildEnvironment()
	built, err := dotnetBuild.CombinedOutput()
	buildErrors := cgoCompilerErrors(string(built))

	t.Logf("READING %s: the converted program's build: %d distinct error(s) in %s", target, len(buildErrors), time.Since(started).Round(time.Second))

	if err != nil {
		t.Fatalf("the converted program does not build: %v\n%s", err, lastLines(strings.ReplaceAll(string(built), "\r\n", "\n"), 40))
	}

	assembly := cgoBuiltAssembly(t, cgoBuildDir(t, output), projects[0])
	csRun := runCgoG1(t, []string{dotnet, assembly}, arm, empty, libraryEnv)

	t.Logf("READING %s: go exit %d %q; go2cs exit %d %q %s", target, goRun.exit, strings.TrimSpace(goRun.stdout), csRun.exit, strings.TrimSpace(csRun.stdout), cgoP3FirstLine(csRun.stderr))

	if goRun.exit != 0 || goRun.stdout == "" {
		t.Fatalf("go was to run the program to its end, and exited %d:\n%s%s", goRun.exit, goRun.stdout, lastLines(goRun.stderr, 8))
	}

	if csRun.exit != goRun.exit || csRun.stdout != goRun.stdout {
		t.Errorf("the two runs differ:\n   go: exit %d %q\ngo2cs: exit %d %q\n%s", goRun.exit, goRun.stdout, csRun.exit, csRun.stdout, lastLines(csRun.stderr, 12))
	}
}
