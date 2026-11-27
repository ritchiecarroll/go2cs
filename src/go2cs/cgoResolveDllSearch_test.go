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
// The same rule covers PATH (Go's binary searches it too) and a hand-owned import in a converted cgo assembly, which
// the assembly's resolver also loads, and a library beside an assembly whose directory is MAX_PATH deep.
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
// library go2csdlla.dll, which imports it, into libDir, each with its import library, so both ld and lld link them; and
// the same pair for the hand-owned import: go2csdlly.dll into depDir, and go2csdllx.dll, which imports it, into libDir.
func cgoDllSearchBuild(t *testing.T, cc []string, depDir, libDir string) {
	t.Helper()

	sources := t.TempDir()
	depSource := writeCgoFixture(t, sources, "dep.c", "__declspec(dllexport) int go2csdlldep_value(void) { return 41; }\n")
	libSource := writeCgoFixture(t, sources, "a.c", "__declspec(dllimport) int go2csdlldep_value(void);\n"+
		"__declspec(dllexport) int go2csdlla_value(void) { return go2csdlldep_value() + 1; }\n")
	ySource := writeCgoFixture(t, sources, "y.c", "__declspec(dllexport) int go2csdlly_value(void) { return 6; }\n")
	xSource := writeCgoFixture(t, sources, "x.c", "__declspec(dllimport) int go2csdlly_value(void);\n"+
		"__declspec(dllexport) int go2csdllx_value(void) { return go2csdlly_value() * 7; }\n")

	base := append(slices.Clone(cc[1:]), cgoArchFlags(runtime.GOOS, runtime.GOARCH)...)

	builds := [][]string{
		append(slices.Clone(base), "-shared", "-o", filepath.Join(depDir, "go2csdlldep.dll"), depSource,
			"-Wl,--out-implib,"+filepath.Join(depDir, "libgo2csdlldep.dll.a")),
		append(slices.Clone(base), "-shared", "-o", filepath.Join(libDir, "go2csdlla.dll"), libSource,
			"-L"+depDir, "-lgo2csdlldep", "-Wl,--out-implib,"+filepath.Join(libDir, "libgo2csdlla.dll.a")),
		append(slices.Clone(base), "-shared", "-o", filepath.Join(depDir, "go2csdlly.dll"), ySource,
			"-Wl,--out-implib,"+filepath.Join(depDir, "libgo2csdlly.dll.a")),
		append(slices.Clone(base), "-shared", "-o", filepath.Join(libDir, "go2csdllx.dll"), xSource,
			"-L"+depDir, "-lgo2csdlly", "-Wl,--out-implib,"+filepath.Join(libDir, "libgo2csdllx.dll.a")),
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

	// A hand-owned import beside the converted code: the assembly's cgo resolver loads it too, as a name its table does
	// not own. It is called after main, and only when the run asks for it.
	writeCgoFixture(t, output, "zz_handowned.cs", cgoDllSearchHandOwned)

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
	// the current directory of the first runs, or on PATH.
	beside := filepath.Dir(assembly)
	copyCgoG1File(t, filepath.Join(libDir, "go2csdlla.dll"), filepath.Join(binaries, "go2csdlla.dll"))
	copyCgoG1File(t, filepath.Join(libDir, "go2csdlla.dll"), filepath.Join(beside, "go2csdlla.dll"))

	planted := t.TempDir()
	copyCgoG1File(t, filepath.Join(depDir, "go2csdlldep.dll"), filepath.Join(planted, "go2csdlldep.dll"))

	run := cgoG1Arm{name: "run"}
	onPath := []string{"PATH=" + planted + string(os.PathListSeparator) + os.Getenv("PATH")}

	for _, place := range []struct {
		name string
		dir  string
		env  []string
	}{
		{"the current directory", planted, nil},
		{"PATH", t.TempDir(), onPath},
	} {
		// The contrast first, so the arm can fail: go's binary takes the dependent from that place.
		goRun := runCgoG1(t, []string{goProgram}, run, place.dir, place.env)
		t.Logf("READING %s contrast: go's binary, its dependent only in %s: exit %d %q", target, place.name, goRun.exit, strings.TrimSpace(goRun.stdout))

		if goRun.exit != 0 || !strings.Contains(goRun.stdout, "a 42") {
			t.Errorf("go's binary did not load the dependent from %s (exit %d %q), so the arm below cannot fail:\n%s", place.name, goRun.exit, goRun.stdout, lastLines(goRun.stderr, 8))
			continue
		}

		// The arm: go2cs, from the same place, refuses the load by the bound library's name.
		csRun := runCgoG1(t, csProgram, run, place.dir, place.env)
		t.Logf("READING %s arm: go2cs, the dependent only in %s: exit %d %q\n%s", target, place.name, csRun.exit, strings.TrimSpace(csRun.stdout), lastLines(csRun.stderr, 6))
		cgoDllSearchRefused(t, csRun, "a 42", "go2csdlla", place.name)
	}

	// The control: with the dependent beside the bound library, go2cs loads it from a directory holding nothing, so
	// the refusals above are the dependent's place and not a library that never loads.
	copyCgoG1File(t, filepath.Join(depDir, "go2csdlldep.dll"), filepath.Join(beside, "go2csdlldep.dll"))
	control := runCgoG1(t, csProgram, run, t.TempDir(), nil)
	t.Logf("READING %s control: go2cs, the dependent beside the bound library: exit %d %q", target, control.exit, strings.TrimSpace(control.stdout))

	if control.exit != 0 || !strings.Contains(control.stdout, "a 42") {
		t.Errorf("go2cs did not load the bound library with its dependent beside it (exit %d %q):\n%s", control.exit, control.stdout, lastLines(control.stderr, 12))
	}

	// The hand-owned import: go2csdllx.dll beside the assembly, its dependent go2csdlly.dll only in the current
	// directory. The runtime's own probing loads it by absolute path, so the resolver holds it as it holds an owned one.
	copyCgoG1File(t, filepath.Join(libDir, "go2csdllx.dll"), filepath.Join(beside, "go2csdllx.dll"))
	plantedY := t.TempDir()
	copyCgoG1File(t, filepath.Join(depDir, "go2csdlly.dll"), filepath.Join(plantedY, "go2csdlly.dll"))
	probe := []string{cgoDllSearchProbeEnv + "=1"}
	onPathY := append([]string{"PATH=" + plantedY + string(os.PathListSeparator) + os.Getenv("PATH")}, probe...)

	for _, place := range []struct {
		name string
		dir  string
		env  []string
	}{
		{"the current directory", plantedY, probe},
		{"PATH", t.TempDir(), onPathY},
	} {
		handOwned := runCgoG1(t, csProgram, run, place.dir, place.env)
		t.Logf("READING %s hand-owned arm: go2cs, the import's dependent only in %s: exit %d %q", target, place.name, handOwned.exit, strings.TrimSpace(handOwned.stdout))
		cgoDllSearchRefused(t, cgoG1Run{stdout: handOwned.stdout, stderr: handOwned.stdout, exit: cgoDllSearchProbeExit(handOwned, "x")}, "x 42", "go2csdllx", place.name)
	}

	// The same import spelled the usual .NET way, a name with a dot and no .dll: [DllImport("go2cs.dllz")] loads
	// go2cs.dllz.dll, as the runtime adds .dll to any name that does not end in .dll or .exe.
	copyCgoG1File(t, filepath.Join(libDir, "go2csdllx.dll"), filepath.Join(beside, "go2cs.dllz.dll"))
	dotted := []string{cgoDllSearchProbeEnv + "=dotted"}

	handOwnedDotted := runCgoG1(t, csProgram, run, plantedY, dotted)
	t.Logf("READING %s hand-owned dotted arm: go2cs, the import's dependent only in the current directory: exit %d %q", target, handOwnedDotted.exit, strings.TrimSpace(handOwnedDotted.stdout))
	cgoDllSearchRefused(t, cgoG1Run{stdout: handOwnedDotted.stdout, stderr: handOwnedDotted.stdout, exit: cgoDllSearchProbeExit(handOwnedDotted, "z")}, "z 42", "go2cs.dllz", "the current directory")

	copyCgoG1File(t, filepath.Join(depDir, "go2csdlly.dll"), filepath.Join(beside, "go2csdlly.dll"))

	for _, control := range []struct {
		name, env, loaded string
	}{
		{"hand-owned control", "1", "x 42"},
		{"hand-owned dotted control", "dotted", "z 42"},
	} {
		handOwnedControl := runCgoG1(t, csProgram, run, t.TempDir(), []string{cgoDllSearchProbeEnv + "=" + control.env})
		t.Logf("READING %s %s: go2cs, the import's dependent beside it: exit %d %q", target, control.name, handOwnedControl.exit, strings.TrimSpace(handOwnedControl.stdout))

		if handOwnedControl.exit != 0 || !strings.Contains(handOwnedControl.stdout, control.loaded) {
			t.Errorf("go2cs did not load the %s's import with its dependent beside it (exit %d %q):\n%s", control.name, handOwnedControl.exit, handOwnedControl.stdout, lastLines(handOwnedControl.stderr, 12))
		}
	}

	// MAX_PATH: the same program in a directory 260 characters deep or more. The control loads with every dependent
	// beside it; the arm, with the bound library's dependent only in the current directory, refuses.
	cgoDllSearchDeep(t, target, csProgram, beside, planted, run)
}

// cgoDllSearchProbeEnv asks the hand-owned import to be called after main: 1 for go2csdllx.dll, dotted for go2cs.dllz.
const cgoDllSearchProbeEnv = "GO2CS_DLLSEARCH_HANDOWNED"

// cgoDllSearchHandOwned is a hand-owned [DllImport] compiled into the converted assembly. After main it calls
// go2csdllx_value and prints "x <value>", or "x refused: <message>" when the load is refused; asked for the dotted
// spelling, it calls the same export through go2cs.dllz and prints "z <value>" or "z refused: <message>".
const cgoDllSearchHandOwned = `using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

internal static class Go2csDllSearchHandOwned
{
    [DllImport("go2csdllx.dll")]
    private static extern int go2csdllx_value();

    [DllImport("go2cs.dllz", EntryPoint = "go2csdllx_value")]
    private static extern int go2csdllz_value();

    [ModuleInitializer]
    internal static void Arm()
    {
        (string label, Func<int> call) = Environment.GetEnvironmentVariable("` + cgoDllSearchProbeEnv + `") switch
        {
            "1" => ("x", (Func<int>)go2csdllx_value),
            "dotted" => ("z", (Func<int>)go2csdllz_value),
            _ => ((string)null, (Func<int>)null)
        };

        if (call is null)
            return;

        AppDomain.CurrentDomain.ProcessExit += (_, _) =>
        {
            try
            {
                Console.WriteLine($"{label} {call()}");
            }
            catch (DllNotFoundException ex)
            {
                Console.WriteLine($"{label} refused: {ex.Message}");
            }
        };
    }
}
`

// cgoDllSearchProbeExit is the hand-owned arm's outcome as an exit code: 1 when the probe printed a refusal under
// label, since the program itself exits 0 once main returns.
func cgoDllSearchProbeExit(run cgoG1Run, label string) int {
	if run.exit != 0 || strings.Contains(run.stdout, label+" refused: ") {
		return 1
	}

	return 0
}

// cgoDllSearchRefused asserts the run refused the load, naming library and the rule, where loaded would be its output
// had the dependent come from place.
func cgoDllSearchRefused(t *testing.T, run cgoG1Run, loaded, library, place string) {
	t.Helper()

	if run.exit == 0 || strings.Contains(run.stdout, loaded) {
		t.Errorf("go2cs loaded %s with its dependent from %s (exit %d %q): the current directory and PATH are never searched", library, place, run.exit, run.stdout)
		return
	}

	for _, want := range []string{library, "the current directory is never searched"} {
		if !strings.Contains(run.stderr+run.stdout, want) {
			t.Errorf("go2cs failed from %s, but not with the refusal: it does not name %q\n%s", place, want, lastLines(run.stderr+run.stdout, 12))
		}
	}
}

// cgoDllSearchDeep copies the built program into a directory whose path is MAX_PATH (260) characters or more, and runs
// the control (every dependent beside the bound library) and the arm (its dependent only in planted, the current
// directory). A host whose dotnet cannot start a program there reads NOT MEASURED, since no load is reached.
func cgoDllSearchDeep(t *testing.T, target string, csProgram []string, beside, planted string, run cgoG1Arm) {
	t.Helper()

	deep := t.TempDir()

	for len(deep) < 280 {
		deep = filepath.Join(deep, "go2cs-maxpath-segment")
	}

	if err := os.CopyFS(deep, os.DirFS(beside)); err != nil {
		t.Fatalf("the program did not copy into the deep directory: %v", err)
	}

	program := []string{csProgram[0], filepath.Join(deep, filepath.Base(csProgram[1]))}
	library := filepath.Join(deep, "go2csdlla.dll")
	t.Logf("READING %s deep: the bound library's path is %d characters", target, len(library))

	control := runCgoG1(t, program, run, t.TempDir(), nil)
	t.Logf("READING %s deep control: go2cs, every dependent beside it: exit %d %q\n%s", target, control.exit, strings.TrimSpace(control.stdout), lastLines(control.stderr, 6))

	if control.exit != 0 || !strings.Contains(control.stdout, "a 42") {
		if !strings.Contains(control.stderr+control.stdout, "cgo:") {
			t.Logf("READING %s deep: NOT MEASURED, dotnet did not start the program from a path of %d characters", target, len(library))
			return
		}

		t.Errorf("go2cs did not load the bound library from a deep directory with its dependent beside it (exit %d %q):\n%s", control.exit, control.stdout, lastLines(control.stderr, 12))
		return
	}

	if err := os.Remove(filepath.Join(deep, "go2csdlldep.dll")); err != nil {
		t.Fatalf("the deep copy's dependent did not delete: %v", err)
	}

	arm := runCgoG1(t, program, run, planted, nil)
	t.Logf("READING %s deep arm: go2cs, the dependent only in the current directory: exit %d %q", target, arm.exit, strings.TrimSpace(arm.stdout))
	cgoDllSearchRefused(t, arm, "a 42", "go2csdlla", "the current directory (deep)")
}
