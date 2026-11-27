// cgoResolveDllSearch_test.go - Gbtc
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
	"slices"
	"strings"
	"testing"
	"time"
)

// cgo Phase 2's bound libraries load through golib's GoCgoLibraries.Resolve, under the rule that the current directory
// is never searched (docs/phase4/DESIGN-cgo-phase2-pinvoke.md §5.4). This row holds that rule for a bound library's
// DEPENDENTS on windows: Phase 4's side library broke it there (NativeLibrary.TryLoad on an absolute path loaded a
// dependent found only in the current directory, TestCgoPhase4H on cgo-e2e 46054089826), and Resolve loads with the
// same NativeLibrary.TryLoad.
//
// This is NOT a parity check. Go's binary loads the dependent from the current directory, since windows' standard
// search order includes it, and that run is the contrast. go2cs refusing here is stricter than Go, by design: the row
// tests that go2cs holds its own rule.

// cgoDllSearchProgram calls one function of the bound library, which calls its dependent.
const cgoDllSearchProgram = `package main

/*
int go2csdlla_value(void);
*/
import "C"

import "fmt"

func main() {
	fmt.Println("a", int(C.go2csdlla_value()))
}
`

// cgoDllSearchBuild builds, with the C compiler go uses, the dependent go2csdlldep.dll into depDir and the bound
// library go2csdlla.dll, which imports it, into libDir, each with its import library, so both ld and lld link them.
func cgoDllSearchBuild(t *testing.T, cc []string, depDir, libDir string) {
	t.Helper()

	sources := t.TempDir()
	depSource := writeCgoFixture(t, sources, "dep.c", "__declspec(dllexport) int go2csdlldep_value(void) { return 41; }\n")
	libSource := writeCgoFixture(t, sources, "a.c", "__declspec(dllimport) int go2csdlldep_value(void);\n"+
		"__declspec(dllexport) int go2csdlla_value(void) { return go2csdlldep_value() + 1; }\n")

	base := append(slices.Clone(cc[1:]), cgoArchFlags(runtime.GOOS, runtime.GOARCH)...)

	builds := [][]string{
		append(slices.Clone(base), "-shared", "-o", filepath.Join(depDir, "go2csdlldep.dll"), depSource,
			"-Wl,--out-implib,"+filepath.Join(depDir, "libgo2csdlldep.dll.a")),
		append(slices.Clone(base), "-shared", "-o", filepath.Join(libDir, "go2csdlla.dll"), libSource,
			"-L"+depDir, "-lgo2csdlldep", "-Wl,--out-implib,"+filepath.Join(libDir, "libgo2csdlla.dll.a")),
	}

	for _, args := range builds {
		if out, err := exec.Command(cc[0], args...).CombinedOutput(); err != nil {
			t.Fatalf("a library did not build: %v %v\n%s", cc[0], args, out)
		}
	}
}

func TestCgoResolveHoldsABoundLibrarysDependentsOutOfTheCurrentDirectory(t *testing.T) {
	if runtime.GOOS != "windows" {
		t.Skipf("on %s a bound library's dependents load by the dynamic loader's own search, as a go binary's do (and darwin binds only absolute install names): NOT MEASURED here", runtime.GOOS)
	}

	if os.Getenv(cgoBuildEnv) != "1" {
		t.Skipf("NOT RUN: set %s=1 to build and run the program (Inconclusive, not a pass)", cgoBuildEnv)
	}

	cgoRealProbeDir(t)
	target := runtime.GOOS + "/" + runtime.GOARCH

	framework := regexp.MustCompile(`<TargetFramework Condition="[^"]*">([^<]+)</TargetFramework>`).FindStringSubmatch(renderCsprojTemplate("Exe", "", ""))

	if framework == nil {
		t.Fatal("the rendered template names no TargetFramework")
	}

	dotnet := requireDotnetSdk(t, framework[1])
	_, cc := cgoRealToolchainLine(t)

	depDir, libDir := t.TempDir(), t.TempDir()
	cgoDllSearchBuild(t, cc, depDir, libDir)
	ldflags := "CGO_LDFLAGS=-L" + libDir + " -lgo2csdlla"

	// The conversion, on this host's real route, with the bound library linked as a Phase 2 binding is.
	treeSrc, err := filepath.Abs("..")

	if err != nil {
		t.Fatal(err)
	}

	module, output := t.TempDir(), t.TempDir()
	writeCgoFixture(t, module, "go.mod", "module example.com/cgodllsearch\n\ngo 1.26\n")
	writeCgoFixture(t, module, "main.go", cgoDllSearchProgram)

	convert := exec.Command(driverBinary(t), "-cgo=true", "-comments", "-go2cspath", treeSrc, module, output)
	convert.Env = append(os.Environ(), "GOWORK=off", "GOFLAGS=", "CGO_ENABLED=", ldflags)
	combined, err := convert.CombinedOutput()

	if err != nil {
		t.Fatalf("the program did not convert on %s: %v\n%s", target, err, lastLines(string(combined), 30))
	}

	// go's own build of the same source, with the same link line.
	binaries := t.TempDir()
	goProgram := filepath.Join(binaries, "dllsearch.exe")
	build := exec.Command("go", "build", "-o", goProgram, ".")
	build.Dir = module
	build.Env = append(os.Environ(), "GOWORK=off", "GOFLAGS=", "CGO_ENABLED=1", ldflags)

	if out, err := build.CombinedOutput(); err != nil {
		t.Fatalf("go did not build the program: %v\n%s", err, lastLines(string(out), 30))
	}

	output = cgoBuildDir(t, output)
	projects, err := filepath.Glob(filepath.Join(output, "*.csproj"))

	if err != nil || len(projects) != 1 {
		t.Fatalf("the conversion wrote %d project files in %s, want 1 (%v)", len(projects), output, err)
	}

	started := time.Now()
	dotnetBuild := exec.Command(dotnet, cgoBuildArgs(t, projects[0], runtime.GOOS, "")...)
	dotnetBuild.Env = append(os.Environ(), "DOTNET_CLI_TELEMETRY_OPTOUT=1", "DOTNET_NOLOGO=1", "MSBUILDDISABLENODEREUSE=1")
	built, err := dotnetBuild.CombinedOutput()

	t.Logf("READING %s: the converted program built in %s", target, time.Since(started).Round(time.Second))

	if err != nil {
		t.Fatalf("the converted program does not build: %v\n%s", err, lastLines(strings.ReplaceAll(string(built), "\r\n", "\n"), 40))
	}

	assembly := cgoBuiltAssembly(t, output, projects[0])
	csProgram := []string{dotnet, assembly}

	// The bound library is found beside each program, as a go binary's is. Its dependent is only in planted, which is
	// the current directory of the runs below.
	copyCgoG1File(t, filepath.Join(libDir, "go2csdlla.dll"), filepath.Join(binaries, "go2csdlla.dll"))
	copyCgoG1File(t, filepath.Join(libDir, "go2csdlla.dll"), filepath.Join(filepath.Dir(assembly), "go2csdlla.dll"))

	planted := t.TempDir()
	copyCgoG1File(t, filepath.Join(depDir, "go2csdlldep.dll"), filepath.Join(planted, "go2csdlldep.dll"))

	run := cgoG1Arm{name: "run"}

	// The contrast first, so the arm can fail: go's binary takes the dependent from the current directory.
	goRun := runCgoG1(t, []string{goProgram}, run, planted, nil)
	t.Logf("READING %s contrast: go's binary, its dependent only in the current directory: exit %d %q", target, goRun.exit, strings.TrimSpace(goRun.stdout))

	if goRun.exit != 0 || !strings.Contains(goRun.stdout, "a 42") {
		t.Fatalf("go's binary did not load the dependent from the current directory (exit %d %q), so the arm below could not fail:\n%s", goRun.exit, goRun.stdout, lastLines(goRun.stderr, 8))
	}

	// The arm: go2cs, from the same directory, refuses the load by the bound library's name.
	csRun := runCgoG1(t, csProgram, run, planted, nil)
	t.Logf("READING %s arm: go2cs, the dependent only in the current directory: exit %d %q\n%s", target, csRun.exit, strings.TrimSpace(csRun.stdout), lastLines(csRun.stderr, 6))

	if csRun.exit == 0 || strings.Contains(csRun.stdout, "a 42") {
		t.Errorf("go2cs loaded the bound library's dependent from the current directory (exit %d %q): the current directory is to be never searched", csRun.exit, csRun.stdout)
	} else {
		for _, want := range []string{"go2csdlla", "the current directory is never searched"} {
			if !strings.Contains(csRun.stderr+csRun.stdout, want) {
				t.Errorf("go2cs failed, but not with the refusal: it does not name %q\n%s", want, lastLines(csRun.stderr, 12))
			}
		}
	}

	// The control: with the dependent beside the bound library, go2cs loads it from a directory holding nothing, so
	// the refusal above is the dependent's place and not a library that never loads.
	copyCgoG1File(t, filepath.Join(depDir, "go2csdlldep.dll"), filepath.Join(filepath.Dir(assembly), "go2csdlldep.dll"))
	control := runCgoG1(t, csProgram, run, t.TempDir(), nil)
	t.Logf("READING %s control: go2cs, the dependent beside the bound library: exit %d %q", target, control.exit, strings.TrimSpace(control.stdout))

	if control.exit != 0 || !strings.Contains(control.stdout, "a 42") {
		t.Errorf("go2cs did not load the bound library with its dependent beside it (exit %d %q):\n%s", control.exit, control.stdout, lastLines(control.stderr, 12))
	}
}
