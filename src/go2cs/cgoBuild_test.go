// cgoBuild_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"bytes"
	"os"
	"os/exec"
	"path/filepath"
	"regexp"
	"runtime"
	"slices"
	"strings"
	"testing"
)

// cgo Phase 2's exit gate, G1, the "compiles" half (docs/phase4/DESIGN-cgo-phase2-pinvoke.md §11.22): the cgo route's
// output built as a project. Before this, nothing compiled what the route writes: the hosted legs show that it
// converts, and GolibTests runs the generator's wrappers over hand-declared types. Here the converter's own C types,
// its bindings file, go2cs-gen and the SDK's LibraryImport generator meet in one build.
//
// The build needs no C compiler, so it runs on any lane with the .NET SDK. It is opt-in (GO2CS_CGO_BUILD=1), since it
// builds the fixture's whole closure of the converted standard library, which takes minutes.

// cgoBuildEnv turns the build tests on. A leg that sets GO2CS_REQUIRE_DOTNET=1 beside it fails where no SDK is.
const cgoBuildEnv = "GO2CS_CGO_BUILD"

// cgoCompilerErrors are the distinct C# compiler and MSBuild errors in a build's output, each with its code first.
func cgoCompilerErrors(output string) []string {
	pattern := regexp.MustCompile(`(?m)^(.*?)\s*:\s*error\s+([A-Z]+\d+)\s*:\s*(.*?)(\s+\[[^\]]*\])?\r?$`)
	seen := map[string]bool{}
	var lines []string

	for _, match := range pattern.FindAllStringSubmatch(output, -1) {
		line := match[2] + ": " + match[3] + " (" + filepath.Base(regexp.MustCompile(`\(\d+,\d+\)$`).ReplaceAllString(match[1], "")) + ")"

		if !seen[line] {
			seen[line] = true
			lines = append(lines, line)
		}
	}

	slices.Sort(lines)

	return lines
}

// cgoBuildDir is dir with its symbolic links resolved. macOS's temporary directory is one (/var for /private/var),
// and MSBuild reaches go2csPath from the project by a relative path: counted from the link and walked from the real
// directory, that path ends one level short, and every referenced project is "not found" (MSB3202).
func cgoBuildDir(t *testing.T, dir string) string {
	t.Helper()

	resolved, err := filepath.EvalSymlinks(dir)

	if err != nil {
		t.Fatal(err)
	}

	return resolved
}

// cgoBuildArgs are dotnet's arguments to build one converted project against this tree, for goos and, when goarch is
// not empty, that architecture. A converted project finds golib, go2cs-gen and the standard library through
// go2csPath, as every converted project does; here it is this tree.
func cgoBuildArgs(t *testing.T, project, goos, goarch string) []string {
	t.Helper()

	treeSrc, err := filepath.Abs("..")

	if err != nil {
		t.Fatal(err)
	}

	args := []string{"build", project, "-nologo", "--disable-build-servers", "-nodeReuse:false", "-p:UseSharedCompilation=false", "-v:q",
		"-p:go2csPath=" + cgoBuildDir(t, treeSrc) + string(filepath.Separator)}

	// An empty goos passes no target at all: the build is the project's own default.
	if goos != "" {
		args = append(args, "-p:GoTargetOS="+goos)
	}

	if goarch != "" {
		args = append(args, "-p:GoTargetArch="+goarch)
	}

	return args
}

// cgoBuiltAssembly is the assembly a build of project left under output. It is named by the project's AssemblyName,
// which is not always its file's name, and its directory is the build configuration's, which a leg's environment may
// set. So it is found, not assumed.
func cgoBuiltAssembly(t *testing.T, output, project string) string {
	t.Helper()

	contents, err := os.ReadFile(project)

	if err != nil {
		t.Fatal(err)
	}

	assemblyName := declaredAssemblyName(string(contents)) + ".dll"
	var assemblies, left []string

	_ = filepath.WalkDir(filepath.Join(output, "bin"), func(path string, entry os.DirEntry, err error) error {
		if err == nil && !entry.IsDir() {
			if entry.Name() == assemblyName {
				assemblies = append(assemblies, path)
			}

			if relative, relErr := filepath.Rel(output, path); relErr == nil && len(left) < 40 {
				left = append(left, relative)
			}
		}

		return nil
	})

	if len(assemblies) != 1 {
		t.Fatalf("the build left %d assemblies named %s, want 1; under bin: %v", len(assemblies), assemblyName, left)
	}

	return assemblies[0]
}

func TestTheCgoRoutesOutputBuildsAsAProject(t *testing.T) {
	if os.Getenv(cgoBuildEnv) != "1" {
		t.Skipf("NOT RUN: set %s=1 to build the converted fixture with the .NET SDK (Inconclusive, not a pass)", cgoBuildEnv)
	}

	framework := regexp.MustCompile(`<TargetFramework Condition="[^"]*">([^<]+)</TargetFramework>`).FindStringSubmatch(renderCsprojTemplate("Library", "", ""))

	if framework == nil {
		t.Fatal("the rendered template names no TargetFramework")
	}

	dotnet := requireDotnetSdk(t, framework[1])
	output := cgoBuildDir(t, t.TempDir())

	// A directory the caller names keeps the converted project for reading after the run.
	if kept := os.Getenv("GO2CS_CGO_BUILD_DIR"); kept != "" {
		if err := os.MkdirAll(kept, 0o755); err != nil {
			t.Fatal(err)
		}

		output = cgoBuildDir(t, kept)
	}

	combined, _, err := convertCgoOfflineInto(t, cgoPhase2Admitted(t), output)

	if err != nil {
		t.Fatalf("the Phase 2 fixture did not convert: %v\n%s", err, lastLines(combined, 30))
	}

	projects, err := filepath.Glob(filepath.Join(output, "*.csproj"))

	if err != nil || len(projects) != 1 {
		t.Fatalf("the conversion wrote %d project files in %s, want 1 (%v)", len(projects), output, err)
	}

	// A converted project finds golib, go2cs-gen and the standard library through go2csPath, as every converted
	// project does; here it is this tree.
	// The fixture is converted for linux/amd64 on every host, so the build is told that target. A leg's own
	// GoTargetOS (MSBuild reads the environment as properties) would otherwise select another OS's source folders,
	// leave the bindings file out, and fail far from the cause: CS0051 on the first C type in an exported signature.
	build := exec.Command(dotnet, cgoBuildArgs(t, projects[0], "linux", "amd64")...)
	build.Env = append(os.Environ(), "DOTNET_CLI_TELEMETRY_OPTOUT=1", "DOTNET_NOLOGO=1", "MSBUILDDISABLENODEREUSE=1")
	built, err := build.CombinedOutput()
	errors := cgoCompilerErrors(string(built))

	t.Logf("READING the converted fixture's build: %d distinct error(s)", len(errors))

	for _, line := range errors {
		t.Logf("READING build error %s", line)
	}

	if err != nil {
		t.Fatalf("the cgo route's output does not build as a project: %v\n%s", err, lastLines(strings.ReplaceAll(string(built), "\r\n", "\n"), 40))
	}
}

// The compile half of TestACNamesFirstLetterDoesNotMakeItsTypePublic: cmd/cgo's types for C names that begin with an
// upper-case letter, as plain Go, in a program that names them only from unexported code. With the pointer typedef
// public and its struct internal, go2cs-gen's members for the pointer type did not compile (CS0050 to CS0057, the
// hosted windows leg of the exit gate, run 43207294212). It needs no C compiler.
func TestACPointerTypedefBuildsWithTheStructItNames(t *testing.T) {
	if os.Getenv(cgoBuildEnv) != "1" {
		t.Skipf("NOT RUN: set %s=1 to build the converted program with the .NET SDK (Inconclusive, not a pass)", cgoBuildEnv)
	}

	framework := regexp.MustCompile(`<TargetFramework Condition="[^"]*">([^<]+)</TargetFramework>`).FindStringSubmatch(renderCsprojTemplate("Exe", "", ""))

	if framework == nil {
		t.Fatal("the rendered template names no TargetFramework")
	}

	dotnet := requireDotnetSdk(t, framework[1])

	dir := writeCgoGateModule(t, map[string]string{
		"cgo_gotypes.go": "package main\n" + cgoNamedCTypes,
		"main.go": `package main

func fill(p _Ctype_LPSYSTEMTIME, n _Ctype_DWORD) _Ctype_DWORD {
	p.wYear = 2026
	return n + 1
}

func main() {
	var st _Ctype_SYSTEMTIME
	println(fill(&st, 1), st.wYear)
}
`})

	stderr, output, err := convertCgoGateFixture(t, dir, nil)

	if err != nil {
		t.Fatalf("conversion failed: %v\n%s", err, stderr)
	}

	output = cgoBuildDir(t, output)
	projects, err := filepath.Glob(filepath.Join(output, "*.csproj"))

	if err != nil || len(projects) != 1 {
		t.Fatalf("the conversion wrote %d project files in %s, want 1 (%v)", len(projects), output, err)
	}

	build := exec.Command(dotnet, cgoBuildArgs(t, projects[0], runtime.GOOS, "")...)
	build.Env = append(os.Environ(), "DOTNET_CLI_TELEMETRY_OPTOUT=1", "DOTNET_NOLOGO=1", "MSBUILDDISABLENODEREUSE=1")
	built, err := build.CombinedOutput()
	errors := cgoCompilerErrors(string(built))

	t.Logf("READING the pointer typedef program's build: %d distinct error(s)", len(errors))

	for _, line := range errors {
		t.Logf("READING build error %s", line)
	}

	if err != nil {
		t.Fatalf("a program naming a C pointer typedef does not build: %v\n%s", err, lastLines(strings.ReplaceAll(string(built), "\r\n", "\n"), 40))
	}
}

// A program that converts between C numeric types, and between a C type and a Go one, builds and prints what go
// prints. The types are cmd/cgo's, written as plain Go, so go runs the same sources with no C compiler. go2cs-gen
// hosted each conversion operator in a part of the source type declared inside the package class, where a C type is
// not, and the build failed on CS1061 (GenTests' AConversionBetweenCTypesCompiles holds the generator; this holds the
// converter and the generator together, and the values).
func TestAProgramConvertingBetweenCTypesBuildsAndRunsAsGoDoes(t *testing.T) {
	if os.Getenv(cgoBuildEnv) != "1" {
		t.Skipf("NOT RUN: set %s=1 to build the converted program with the .NET SDK (Inconclusive, not a pass)", cgoBuildEnv)
	}

	framework := regexp.MustCompile(`<TargetFramework Condition="[^"]*">([^<]+)</TargetFramework>`).FindStringSubmatch(renderCsprojTemplate("Exe", "", ""))

	if framework == nil {
		t.Fatal("the rendered template names no TargetFramework")
	}

	dotnet := requireDotnetSdk(t, framework[1])

	dir := writeCgoGateModule(t, map[string]string{
		"cgo_gotypes.go": `package main

type _Ctype_int int32
type _Ctype_long int64
type _Ctype_short int16
type _Ctype_ulong uint64
type _Ctype_size_t = _Ctype_ulong
type _Ctype_ushort uint16
type _Ctype_DWORD uint32
`,
		"main.go": `package main

type count uint32

func main() {
	n := _Ctype_int(7)
	size := _Ctype_size_t(n)
	wide := _Ctype_long(n)
	big := _Ctype_long(70000)
	narrow := _Ctype_short(big)
	var d _Ctype_DWORD = 70000
	low := _Ctype_ushort(d)
	c := count(d)
	back := _Ctype_DWORD(c + 1)
	println(uint64(size), int64(wide), int16(narrow), uint16(low), uint32(c), uint32(back))
}
`})

	goRun := exec.Command("go", "run", ".")
	goRun.Dir = dir
	goRun.Env = append(os.Environ(), "GOWORK=off", "GOFLAGS=", "CGO_ENABLED=0")
	want, err := goRun.CombinedOutput()

	if err != nil {
		t.Fatalf("go did not run the program: %v\n%s", err, want)
	}

	stderr, output, err := convertCgoGateFixture(t, dir, nil)

	if err != nil {
		t.Fatalf("conversion failed: %v\n%s", err, stderr)
	}

	output = cgoBuildDir(t, output)
	projects, err := filepath.Glob(filepath.Join(output, "*.csproj"))

	if err != nil || len(projects) != 1 {
		t.Fatalf("the conversion wrote %d project files in %s, want 1 (%v)", len(projects), output, err)
	}

	build := exec.Command(dotnet, cgoBuildArgs(t, projects[0], runtime.GOOS, "")...)
	build.Env = append(os.Environ(), "DOTNET_CLI_TELEMETRY_OPTOUT=1", "DOTNET_NOLOGO=1", "MSBUILDDISABLENODEREUSE=1")
	built, err := build.CombinedOutput()
	errors := cgoCompilerErrors(string(built))

	t.Logf("READING the C conversions program's build: %d distinct error(s)", len(errors))

	for _, line := range errors {
		t.Logf("READING build error %s", line)
	}

	if err != nil {
		t.Fatalf("a program converting between C numeric types does not build: %v\n%s", err, lastLines(strings.ReplaceAll(string(built), "\r\n", "\n"), 40))
	}

	got, err := exec.Command(dotnet, cgoBuiltAssembly(t, output, projects[0])).CombinedOutput()

	if err != nil {
		t.Fatalf("the converted program did not run: %v\n%s", err, got)
	}

	wantLine, gotLine := strings.TrimSpace(strings.ReplaceAll(string(want), "\r\n", "\n")), strings.TrimSpace(strings.ReplaceAll(string(got), "\r\n", "\n"))

	t.Logf("READING the C conversions program: go prints %q; go2cs prints %q", wantLine, gotLine)

	if wantLine != gotLine || wantLine == "" {
		t.Fatalf("the two runs differ, or printed nothing:\n   go: %q\ngo2cs: %q", wantLine, gotLine)
	}
}

// cgoBuildEnvironment is the environment a build of a converted project runs in. MSBuild reads the environment as
// properties, so a leg's own GoTargetOS and GoTargetArch are taken out: each test says which target it builds for.
func cgoBuildEnvironment() []string {
	env := []string{"DOTNET_CLI_TELEMETRY_OPTOUT=1", "DOTNET_NOLOGO=1", "MSBUILDDISABLENODEREUSE=1"}

	for _, entry := range os.Environ() {
		name, _, _ := strings.Cut(entry, "=")

		if !strings.EqualFold(name, "GoTargetOS") && !strings.EqualFold(name, "GoTargetArch") {
			env = append(env, entry)
		}
	}

	return env
}

// cgoConvertedFixtureProject converts the Phase 2 fixture, offline and for linux/amd64 as on every host, and returns
// dotnet, the output directory and the one project file the conversion wrote there.
func cgoConvertedFixtureProject(t *testing.T) (string, string, string) {
	t.Helper()

	if os.Getenv(cgoBuildEnv) != "1" {
		t.Skipf("NOT RUN: set %s=1 to build the converted fixture with the .NET SDK (Inconclusive, not a pass)", cgoBuildEnv)
	}

	framework := regexp.MustCompile(`<TargetFramework Condition="[^"]*">([^<]+)</TargetFramework>`).FindStringSubmatch(renderCsprojTemplate("Library", "", ""))

	if framework == nil {
		t.Fatal("the rendered template names no TargetFramework")
	}

	dotnet := requireDotnetSdk(t, framework[1])
	output := cgoBuildDir(t, t.TempDir())
	combined, _, err := convertCgoOfflineInto(t, cgoPhase2Admitted(t), output)

	if err != nil {
		t.Fatalf("the Phase 2 fixture did not convert: %v\n%s", err, lastLines(combined, 30))
	}

	projects, err := filepath.Glob(filepath.Join(output, "*.csproj"))

	if err != nil || len(projects) != 1 {
		t.Fatalf("the conversion wrote %d project files in %s, want 1 (%v)", len(projects), output, err)
	}

	return dotnet, output, projects[0]
}

// The exit gate's OPEN 2 (DESIGN-cgo-phase3-marshaling §15, R5): a cgo package's bindings are for the one target it was
// converted for. Built for another, the bindings file is left out, and the build failed far from the cause, on the
// first C type in an exported signature. The project refuses that build by name, with both targets, before the
// compiler runs.
func TestACgoProjectBuiltForAnotherTargetIsRefusedByName(t *testing.T) {
	dotnet, _, project := cgoConvertedFixtureProject(t)

	build := exec.Command(dotnet, cgoBuildArgs(t, project, "windows", "amd64")...)
	build.Env = cgoBuildEnvironment()
	built, err := build.CombinedOutput()
	text := strings.ReplaceAll(string(built), "\r\n", "\n")
	coded := cgoCompilerErrors(text)

	for _, line := range coded {
		t.Logf("READING build error %s", line)
	}

	if err == nil {
		t.Fatalf("a cgo project converted for linux/amd64 built for windows/amd64:\n%s", lastLines(text, 20))
	}

	var refusals []string

	for _, line := range strings.Split(text, "\n") {
		if strings.Contains(line, "error") && strings.Contains(line, "go2cs: ") {
			refusals = append(refusals, strings.TrimSpace(line))
		}
	}

	slices.Sort(refusals)
	refusals = slices.Compact(refusals)

	if len(refusals) != 1 || !strings.Contains(refusals[0], "linux/amd64") || !strings.Contains(refusals[0], "windows/amd64") {
		t.Errorf("the build is not refused once, by go2cs, naming both targets; go2cs's errors: %q\n%s", refusals, lastLines(text, 20))
	}

	if len(coded) > 0 {
		t.Errorf("the build reached the compiler or failed elsewhere: %v", coded)
	}
}

// R5's second half: a cgo project's default target is its bindings'. That default reached only the project itself,
// and the standard library it references took its own default, windows/amd64. So a build that names no target
// compiled linux bindings beside the windows standard library, and said nothing. The project hands its one target to
// its project references. runtime is laid out per GOOS, and the fixture imports it, so its assembly (go.runtime.dll)
// beside the fixture's says which flavour the build took.
func TestACgoProjectBuiltWithNoTargetTakesItsOwnTargetsStandardLibrary(t *testing.T) {
	dotnet, output, project := cgoConvertedFixtureProject(t)

	for _, arm := range []struct{ name, goos, goarch string }{
		{"no target named", "", ""},
		{"the target passed by hand", "linux", "amd64"},
	} {
		t.Run(arm.name, func(t *testing.T) {
			build := exec.Command(dotnet, cgoBuildArgs(t, project, arm.goos, arm.goarch)...)
			build.Env = cgoBuildEnvironment()
			built, err := build.CombinedOutput()

			if err != nil {
				t.Fatalf("the cgo route's output does not build: %v\n%s", err, lastLines(strings.ReplaceAll(string(built), "\r\n", "\n"), 40))
			}

			var runtimes []string

			_ = filepath.WalkDir(filepath.Join(output, "bin"), func(path string, entry os.DirEntry, err error) error {
				if err == nil && !entry.IsDir() && entry.Name() == "go.runtime.dll" {
					runtimes = append(runtimes, path)
				}

				return nil
			})

			if len(runtimes) != 1 {
				t.Fatalf("the build left %d go.runtime.dll beside the fixture, want 1: %v", len(runtimes), runtimes)
			}

			assembly, err := os.ReadFile(runtimes[0])

			if err != nil {
				t.Fatal(err)
			}

			// Names only one flavour of the converted runtime declares (os_linux.go, os_windows.go).
			linux, windows := bytes.Contains(assembly, []byte("rt_sigaction")), bytes.Contains(assembly, []byte("loadOptionalSyscalls"))
			t.Logf("READING %s: the runtime beside the linux/amd64 fixture is linux's: %v, windows's: %v", arm.name, linux, windows)

			if !linux || windows {
				t.Errorf("the standard library beside a linux/amd64 cgo project is not linux's (linux %v, windows %v)", linux, windows)
			}

			if err := os.RemoveAll(filepath.Join(output, "bin")); err != nil {
				t.Fatal(err)
			}
		})
	}
}

// A cgo project hands its project references a target, and the standard library builds only for the targets it is laid
// out for (platformUniverse). A package's bindings are for the host's own target, which is not always one of them:
// handing down darwin/arm64 refused every build of a cgo project on that host, by the library's own universe check
// (DESIGN-cgo-phase3-marshaling §15, R5's second half; first read on the hosted leg 43335132134). The references take
// the flavour the library has for the package's operating system. The project's own refusal still names the bindings'
// own target.
func TestACgoProjectHandsItsReferencesATargetTheStandardLibraryIsLaidOutFor(t *testing.T) {
	for _, arm := range []struct{ target, reference string }{
		{"windows/amd64", "windows/amd64"},
		{"linux/amd64", "linux/amd64"},
		{"linux/arm64", "linux/arm64"},
		{"darwin/amd64", "darwin/amd64"},
		// The library has one darwin flavour, which serves both of darwin's architectures.
		{"darwin/arm64", "darwin/amd64"},
	} {
		goos, goarch, _ := strings.Cut(arm.target, "/")
		referenceOS, referenceArch, _ := strings.Cut(arm.reference, "/")
		blocks := cgoTargetBlocks(goos, goarch)

		if !slices.Contains(platformUniverse, arm.reference) {
			t.Errorf("%s: the arm expects %s, which the standard library is not laid out for", arm.target, arm.reference)
		}

		if want := "<AdditionalProperties>GoTargetOS=" + referenceOS + ";GoTargetArch=" + referenceArch + "</AdditionalProperties>"; !strings.Contains(blocks, want) {
			t.Errorf("%s: the project does not hand its references %s:\n%s", arm.target, arm.reference, blocks)
		}

		if want := "!='" + arm.target + "'\""; !strings.Contains(blocks, want) {
			t.Errorf("%s: the project's own refusal does not hold it to its bindings' target:\n%s", arm.target, blocks)
		}
	}
}
