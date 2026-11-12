// cgoG1_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"bytes"
	"context"
	"errors"
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

// cgo Phase 2's exit gate, G1, the "resolves and runs" half (docs/phase4/DESIGN-cgo-phase2-pinvoke.md §11.22). The gate
// program (testdata/cgo/g1) is built by go and converted by go2cs on the same host, with the same C compiler and the
// same libraries. Each of its arms is run both ways, and what the two print is compared: equal, or one of the
// divergences §11.16 rules, each side's behaviour asserted. It needs a C compiler, the probe library and the .NET SDK,
// so it runs on the hosted legs, where the absence of any of them fails.

// cgoG1FixtureDir holds the gate program's sources.
const cgoG1FixtureDir = "testdata/cgo/g1"

// cgoG1PointerPanic is the text of Go's cgo pointer check (runtime/cgocall.go), which go2cs's copy-in check repeats.
const cgoG1PointerPanic = "argument of cgo function has Go pointer to unpinned Go pointer"

// cgoG1Arm is one arm of the gate program and how its two runs relate.
type cgoG1Arm struct {
	name string
	// env is added to both runs.
	env []string
	// goFails and csFails are the text the failing side prints, for a ruled divergence. An arm with neither must
	// print the same lines and exit 0 both ways.
	goFails string
	csFails string
}

// label names the arm with its environment.
func (arm cgoG1Arm) label() string {
	if len(arm.env) == 0 {
		return arm.name
	}

	return arm.name + " (" + strings.Join(arm.env, " ") + ")"
}

// cgoG1Arms are the arms this host runs.
func cgoG1Arms() []cgoG1Arm {
	arms := []cgoG1Arm{
		{name: "cos"},
		{name: "div"},
		{name: "exp"},
		{name: "frexp"},
		{name: "conv"},
		{name: "puts"},
		{name: "nodesum"},
		{name: "nested"},
		{name: "tworesults"},
		{name: "statement"},
		{name: "ordered"},
		{name: "copyback"},
		// §11.16's divergences, each side asserted (§11.20's checklist).
		{name: "packagelevel", goFails: cgoG1PointerPanic},
		{name: "sibling", goFails: cgoG1PointerPanic},
		{name: "pinned", csFails: cgoG1PointerPanic},
		{name: "pinned", env: []string{"GODEBUG=cgocheck=0"}, csFails: cgoG1PointerPanic},
		{name: "byvalue", csFails: cgoG1PointerPanic},
		{name: "reinterpreted", csFails: "go2cs refuses the call"},
	}

	if runtime.GOOS != "windows" {
		arms = append(arms, cgoG1Arm{name: "close"}, cgoG1Arm{name: "mktime"})
	}

	return arms
}

// cgoG1Run is one run of the gate program.
type cgoG1Run struct {
	stdout string
	stderr string
	exit   int
}

// runCgoG1 runs command with one arm, in dir, and returns what it printed and its exit code.
func runCgoG1(t *testing.T, command []string, arm cgoG1Arm, dir string, env []string) cgoG1Run {
	t.Helper()

	ctx, cancel := context.WithTimeout(context.Background(), 2*time.Minute)
	defer cancel()

	cmd := exec.CommandContext(ctx, command[0], append(command[1:], arm.name)...)
	cmd.Dir = dir
	cmd.Env = append(append(os.Environ(), env...), arm.env...)

	var stdout, stderr bytes.Buffer
	cmd.Stdout, cmd.Stderr = &stdout, &stderr
	err := cmd.Run()
	run := cgoG1Run{stdout: strings.ReplaceAll(stdout.String(), "\r\n", "\n"), stderr: strings.ReplaceAll(stderr.String(), "\r\n", "\n")}

	var exited *exec.ExitError

	switch {
	case err == nil:
	case errors.As(err, &exited):
		run.exit = exited.ExitCode()
	default:
		t.Fatalf("arm %s: %v did not run: %v", arm.label(), command, err)
	}

	return run
}

// cgoG1ProbeFile is the probe library's file name on this OS.
func cgoG1ProbeFile() string {
	switch runtime.GOOS {
	case "windows":
		return "go2csprobe.dll"
	case "darwin":
		return "libgo2csprobe.dylib"
	default:
		return "libgo2csprobe.so"
	}
}

// cgoG1LibraryEnv is what lets a program find the library in dir as this OS's loader does for a Go binary: the
// loader's search path on linux and darwin. On windows a library is found beside the program, so it is copied there.
func cgoG1LibraryEnv(dir string) []string {
	switch runtime.GOOS {
	case "linux":
		return []string{"LD_LIBRARY_PATH=" + dir}
	case "darwin":
		return []string{"DYLD_LIBRARY_PATH=" + dir}
	default:
		return nil
	}
}

// copyCgoG1File copies one file.
func copyCgoG1File(t *testing.T, from, to string) {
	t.Helper()

	contents, err := os.ReadFile(from)

	if err != nil {
		t.Fatal(err)
	}

	if err := os.WriteFile(to, contents, 0o755); err != nil {
		t.Fatal(err)
	}
}

// cgoG1Decoy is a library with the probe's name whose go2cs_probe_node_sum answers 7000 more than the probe's, built
// with the host's C compiler into a directory of its own: a library that loads, and is the wrong one.
func cgoG1Decoy(t *testing.T, cc []string) string {
	t.Helper()

	dir := t.TempDir()
	source := filepath.Join(dir, "decoy.c")
	export := "__attribute__((visibility(\"default\")))"

	if runtime.GOOS == "windows" {
		export = "__declspec(dllexport)"
	}

	writeCgoFixture(t, dir, "decoy.c", "struct go2cs_probe_pair { int a; int b; };\n"+
		"struct go2cs_probe_node { int value; struct go2cs_probe_node *next; };\n"+
		"struct go2cs_probe_link { int value; struct go2cs_probe_node *node; };\n"+
		export+" long go2cs_probe_node_sum(struct go2cs_probe_node *node, long base) { (void)node; return base + 7000; }\n"+
		export+" void go2cs_probe_node_touch(struct go2cs_probe_node *node) { (void)node; }\n"+
		export+" int go2cs_probe_link_value(struct go2cs_probe_link link) { (void)link; return 7000; }\n"+
		export+" int go2cs_probe_pair_scaled(struct go2cs_probe_pair pair, int k) { (void)pair; (void)k; return 7000; }\n"+
		export+" const char *go2cs_probe_text(void) { return \"decoy\"; }\n")

	args := append(slices.Clone(cc[1:]), cgoArchFlags(runtime.GOOS, runtime.GOARCH)...)

	switch runtime.GOOS {
	case "darwin":
		args = append(args, "-dynamiclib")
	case "windows":
		args = append(args, "-shared")
	default:
		args = append(args, "-shared", "-fPIC")
	}

	args = append(args, "-o", filepath.Join(dir, cgoG1ProbeFile()), source)

	if out, err := exec.Command(cc[0], args...).CombinedOutput(); err != nil {
		t.Fatalf("the decoy library did not build: %v\n%s", err, out)
	}

	return dir
}

func TestG1TheConvertedCgoProgramResolvesAndRunsAsGoDoes(t *testing.T) {
	// The converter-cgo legs have a compiler and the probe too, and no room in their budget for this build: the gate
	// runs where it is asked for (the cgo-e2e stage).
	if os.Getenv(cgoBuildEnv) != "1" {
		t.Skipf("NOT RUN: set %s=1 to build and run the gate program (Inconclusive, not a pass)", cgoBuildEnv)
	}

	probeDir := cgoRealProbeDir(t)
	target := runtime.GOOS + "/" + runtime.GOARCH

	framework := regexp.MustCompile(`<TargetFramework Condition="[^"]*">([^<]+)</TargetFramework>`).FindStringSubmatch(renderCsprojTemplate("Exe", "", ""))

	if framework == nil {
		t.Fatal("the rendered template names no TargetFramework")
	}

	dotnet := requireDotnetSdk(t, framework[1])
	_, cc := cgoRealToolchainLine(t)
	files := cgoFixtureFiles(t, cgoG1FixtureDir)

	// The conversion, on this host's real route.
	combined, module, output, err := convertCgoReal(t, probeDir, files)

	if err != nil {
		for _, line := range cgoRefusalLinesIn(combined, "") {
			t.Logf("READING %s: refused: %s", target, line[strings.Index(line, "error "):])
		}

		t.Fatalf("the gate program did not convert on %s: %v\n%s", target, err, lastLines(combined, 30))
	}

	// go's own build of the same sources, with the same compiler and the same link line.
	binaries := t.TempDir()
	goProgram := filepath.Join(binaries, "g1go")

	if runtime.GOOS == "windows" {
		goProgram += ".exe"
	}

	// On darwin go's linker runs Apple's strip on an externally linked binary unless DWARF is off (-w), and strip
	// refuses what the pinned zig links ("the __LLVM segment too small"; first read on run 43207288962). That is go
	// and the C toolchain, before go2cs is involved, so the gate builds without DWARF there.
	goBuild := []string{"build", "-o", goProgram}

	if runtime.GOOS == "darwin" {
		goBuild = append(goBuild, "-ldflags=-w")
	}

	build := exec.Command("go", append(goBuild, ".")...)
	build.Dir = module
	build.Env = append(os.Environ(), "GOWORK=off", "GOFLAGS=", "CGO_ENABLED=1", "CGO_LDFLAGS=-L"+probeDir+" -lgo2csprobe")

	if out, err := build.CombinedOutput(); err != nil {
		t.Fatalf("go did not build the gate program: %v\n%s", err, lastLines(string(out), 30))
	}

	// The converted program, built as a project: this is "compiles". The OS is stated; the architecture is left to
	// the project, whose own default is its bindings' (there is no darwin/arm64 flavour of the standard library to
	// ask for by name, and its darwin flavour serves both).
	output = cgoBuildDir(t, output)
	projects, err := filepath.Glob(filepath.Join(output, "*.csproj"))

	if err != nil || len(projects) != 1 {
		t.Fatalf("the conversion wrote %d project files in %s, want 1 (%v)", len(projects), output, err)
	}

	started := time.Now()
	dotnetBuild := exec.Command(dotnet, cgoBuildArgs(t, projects[0], runtime.GOOS, "")...)
	dotnetBuild.Env = append(os.Environ(), "DOTNET_CLI_TELEMETRY_OPTOUT=1", "DOTNET_NOLOGO=1", "MSBUILDDISABLENODEREUSE=1")
	built, err := dotnetBuild.CombinedOutput()
	buildErrors := cgoCompilerErrors(string(built))

	t.Logf("READING %s: the converted gate program's build: %d distinct error(s) in %s", target, len(buildErrors), time.Since(started).Round(time.Second))

	for _, line := range buildErrors {
		t.Logf("READING %s: build error %s", target, line)
	}

	if err != nil {
		t.Fatalf("the converted gate program does not build: %v\n%s", err, lastLines(strings.ReplaceAll(string(built), "\r\n", "\n"), 40))
	}

	assembly := cgoBuiltAssembly(t, output, projects[0])
	assemblies := []string{assembly}
	csProgram := []string{dotnet, assembly}
	libraryEnv := cgoG1LibraryEnv(probeDir)

	if runtime.GOOS == "windows" {
		copyCgoG1File(t, filepath.Join(probeDir, cgoG1ProbeFile()), filepath.Join(binaries, cgoG1ProbeFile()))
		copyCgoG1File(t, filepath.Join(probeDir, cgoG1ProbeFile()), filepath.Join(filepath.Dir(assemblies[0]), cgoG1ProbeFile()))
	}

	// A directory with nothing in it is where every arm runs, so nothing is found by being beside the program.
	empty := t.TempDir()

	for _, arm := range cgoG1Arms() {
		t.Run(strings.NewReplacer(" ", "_", "=", "_").Replace(arm.label()), func(t *testing.T) {
			goRun := runCgoG1(t, []string{goProgram}, arm, empty, libraryEnv)
			csRun := runCgoG1(t, csProgram, arm, empty, libraryEnv)

			t.Logf("READING %s %s: go exit %d %q; go2cs exit %d %q", target, arm.label(), goRun.exit, strings.TrimSpace(goRun.stdout), csRun.exit, strings.TrimSpace(csRun.stdout))

			if arm.goFails != "" {
				if goRun.exit == 0 || !strings.Contains(goRun.stderr, arm.goFails) {
					t.Errorf("go was to fail with %q, and exited %d:\n%s%s", arm.goFails, goRun.exit, goRun.stdout, lastLines(goRun.stderr, 8))
				}
			} else if goRun.exit != 0 {
				t.Errorf("go was to run this arm to its end, and exited %d:\n%s", goRun.exit, lastLines(goRun.stderr, 8))
			}

			if arm.csFails != "" {
				if csRun.exit == 0 || !strings.Contains(csRun.stderr+csRun.stdout, arm.csFails) {
					t.Errorf("go2cs was to refuse with %q, and exited %d:\n%s%s", arm.csFails, csRun.exit, csRun.stdout, lastLines(csRun.stderr, 8))
				}
			} else if csRun.exit != 0 {
				t.Errorf("go2cs was to run this arm to its end, and exited %d:\n%s%s", csRun.exit, csRun.stdout, lastLines(csRun.stderr, 12))
			}

			if arm.goFails == "" && arm.csFails == "" {
				goLines, csLines := goRun.stdout, csRun.stdout

				if arm.name == "puts" {
					// C's own line reaches a captured stdout or not as each side's exit flushes C's buffer: a reading.
					// The arm is the line Go prints, which carries puts's result.
					t.Logf("READING %s puts: C's line in go's stdout: %v; in go2cs's: %v", target, strings.Contains(goLines, "go2cs probe text"), strings.Contains(csLines, "go2cs probe text"))
					goLines, csLines = cgoG1Line(goLines, "puts "), cgoG1Line(csLines, "puts ")
				}

				if goLines != csLines || goLines == "" {
					t.Errorf("the two runs differ, or printed nothing:\n   go: %q\ngo2cs: %q", goLines, csLines)
				}
			}
		})
	}

	// The PLAN's own arm: a library of the same name in the current directory must not be the one loaded. The control
	// first, so the arm can fail: where the decoy IS what the loader finds, go's own binary prints its answer.
	t.Run("a_library_in_the_current_directory_is_not_loaded", func(t *testing.T) {
		decoyDir := cgoG1Decoy(t, cc)
		nested := cgoG1Arm{name: "nested"}
		want := runCgoG1(t, csProgram, nested, empty, libraryEnv)

		control := filepath.Join(decoyDir, filepath.Base(goProgram))
		copyCgoG1File(t, goProgram, control)
		found := runCgoG1(t, []string{control}, nested, decoyDir, cgoG1LibraryEnv(decoyDir))

		t.Logf("READING %s decoy control: go's binary with the decoy as the library found prints %q (exit %d)", target, strings.TrimSpace(found.stdout), found.exit)

		if !strings.Contains(found.stdout, "nested 7001") {
			if runtime.GOOS == "darwin" {
				// The probe is bound by its absolute install name there, and whether dyld lets a search path override
				// one is the host's setting. Without the control the arm below is NOT MEASURED on this host.
				t.Skipf("NOT MEASURED on %s: the decoy could not be made the library found (%q), so the arm cannot fail here", target, strings.TrimSpace(found.stdout))
			}

			t.Fatalf("the control did not load the decoy (%q), so the arm below could not fail", strings.TrimSpace(found.stdout))
		}

		planted := runCgoG1(t, csProgram, nested, decoyDir, libraryEnv)

		t.Logf("READING %s decoy: go2cs from a directory holding the decoy prints %q (exit %d)", target, strings.TrimSpace(planted.stdout), planted.exit)

		if planted.exit != 0 || planted.stdout != want.stdout || !strings.Contains(planted.stdout, "nested 1") {
			t.Fatalf("a library in the current directory changed the converted program's answer: %q, want %q\n%s", planted.stdout, want.stdout, lastLines(planted.stderr, 8))
		}
	})
}

// cgoG1Line is the first line of output that starts with prefix, or "".
func cgoG1Line(output, prefix string) string {
	for _, line := range strings.Split(output, "\n") {
		if strings.HasPrefix(line, prefix) {
			return line
		}
	}

	return ""
}
