// cgoPhase4_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"debug/elf"
	"debug/macho"
	"debug/pe"
	"encoding/json"
	"os"
	"os/exec"
	"path/filepath"
	"regexp"
	"runtime"
	"strings"
	"testing"
	"time"

	"golang.org/x/tools/go/packages"
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
	fmt.Println("halve 7", h, err != nil, err)
}
`

// cgoPhase4VariableProgram is OQ-A's arm (the owner's ruling, 2026-11-27: in this slice): a variable the preamble
// defines, read, written from Go and changed by C, through the same table. cmd/cgo binds a C variable with
// //go:cgo_import_static, so the preamble's is not static.
const cgoPhase4VariableProgram = `package main

/*
int counter = 41;
static void bump(void) { counter++; }
*/
import "C"

import "fmt"

func main() {
	fmt.Println("counter", C.counter)
	C.bump()
	fmt.Println("bumped", C.counter)
	C.counter = 7
	C.bump()
	fmt.Println("set and bumped", C.counter)
}
`

// cgoPhase4MixedProgram holds a bound function and a side-built one in one package (review of 500b0855c7, #6): C.cos
// binds libm's export, and cos_twice, which the preamble defines, calls that same cos from inside the side library.
const cgoPhase4MixedProgram = `package main

/*
#cgo LDFLAGS: -lm
#include <math.h>
static double cos_twice(double v) { return 2 * cos(v); }
*/
import "C"

import "fmt"

func main() {
	fmt.Println("cos", C.cos(0))
	fmt.Println("cos_twice", C.cos_twice(0))
}
`

// cgoPhase4ShadowProgram defines, with no -lm, a non-static function whose name a library the host process already
// loads also exports (review of 500b0855c7, #4): Go links the preamble's own definition, and so must the side library.
const cgoPhase4ShadowProgram = `package main

/*
double gamma(double v) { return 7; }
*/
import "C"

import "fmt"

func main() {
	fmt.Println("gamma", C.gamma(2))
}
`

// The design's §3 red. Arm 1 converts on every leg with a C compiler and the probe; arm 2 builds the converted program
// and runs it against go's own build where the build tests are on (cgo-e2e, GO2CS_CGO_BUILD=1).
func TestCgoPhase4APreambleDefinedFunctionConvertsAndRunsAsGoDoes(t *testing.T) {
	runCgoPhase4(t, cgoPhase4Program)
}

// OQ-A: a variable the preamble defines converts and runs as Go does.
func TestCgoPhase4BPreambleDefinedVariableConvertsAndRunsAsGoDoes(t *testing.T) {
	runCgoPhase4(t, cgoPhase4VariableProgram)
}

// A bound function and a side-built one, side by side, the side-built one calling a library (review of 500b0855c7, #6).
func TestCgoPhase4CBoundAndSideBuiltConvertAndRunAsGoDoes(t *testing.T) {
	runCgoPhase4(t, cgoPhase4MixedProgram)
}

// A preamble definition is the one called, where a library the process holds exports the same name (review of
// 500b0855c7, #4): an ELF side library binds its own definitions first.
//
// The shadowing is ELF's, so gamma stays on linux, where it was red (f8a742d1ef) and is cured, and on windows. darwin's
// libSystem carries libm and every link includes it, so on darwin/amd64 gamma is in the package's OWN link, and the
// preamble's definition collides with it (refused there: ef0f9a89e2, 46041721404). That is F's shape, not D's. Mach-O
// binds an image's own definitions anyway, so on darwin D is a plain parity row, under a name no library exports.
func TestCgoPhase4DAPreambleDefinitionIsNotShadowedByALoadedLibrary(t *testing.T) {
	program := cgoPhase4ShadowProgram

	if runtime.GOOS == "darwin" {
		program = strings.ReplaceAll(program, "gamma", "go2cs_p4_gamma")
	}

	runCgoPhase4(t, program)
}

// cmd/go adds a package's #cgo pkg-config flags at build time, which go list's CgoCFLAGS does not carry, so a side build
// would compile with other flags than cmd/cgo's (review of 500b0855c7, #1): it is refused by name.
func TestCgoPhase4EASideBuildUnderPkgConfigIsRefusedByName(t *testing.T) {
	probeDir := cgoRealProbeDir(t)

	if _, err := exec.LookPath("pkg-config"); err != nil {
		t.Skipf("no pkg-config on PATH here: NOT MEASURED")
	}

	pcDir := t.TempDir()
	writeCgoFixture(t, pcDir, "go2cspc.pc", "Name: go2cspc\nDescription: go2cs pkg-config fixture\nVersion: 1\nCflags: -DGO2CS_PC=7\nLibs:\n")
	t.Setenv("PKG_CONFIG_PATH", pcDir)

	// The silent shape is the finding's: the define selects code that compiles either way, so a side build without it
	// succeeds and answers differently from go's (COORD, 2026-11-27). The loud shape uses the define's value, so a side
	// build without it cannot compile; it is kept as a second row.
	for _, row := range []struct{ name, preamble string }{
		{"silent", "#ifdef GO2CS_PC\nstatic int pc(void) { return 7; }\n#else\nstatic int pc(void) { return 1; }\n#endif\n"},
		{"loud", "static int pc(void) { return GO2CS_PC; }\n"},
	} {
		t.Run(row.name, func(t *testing.T) {
			program := "package main\n\n/*\n#cgo pkg-config: go2cspc\n" + row.preamble + "*/\nimport \"C\"\n\nimport \"fmt\"\n\nfunc main() { fmt.Println(\"pc\", C.pc()) }\n"
			combined, _, output, err := convertCgoReal(t, probeDir, map[string]string{"main.go": program})
			t.Logf("READING %s/%s %s: exit %v; the conversion's output ends:\n%s", runtime.GOOS, runtime.GOARCH, row.name, err, lastLines(combined, 12))
			requireNothingWritten(t, output, err, combined)
			requireCgoUseRefused(t, cgoRefusalLinesIn(combined, "main.go"), "main.go(", "C.pc", "pkg-config")
		})
	}
}

// A name the preamble defines and a library the package links also exports runs the preamble's own, as go's build does
// (cgo Phase 4b, row 1; docs/phase4/DESIGN-cgo-phase4b-own-sources.md): cmd/cgo compiles the preamble, so its
// definition is what C calls, and the side build classifies a name the hosting unit defines before it asks the probe.
// At Phase 4 the probe was asked first and the name was refused by name (review of 500b0855c7, #5).
func TestCgoPhase4FAPreambleDefinitionALibraryAlsoExportsRunsAsGoDoes(t *testing.T) {
	// A name the target's C runtime exports from a shared library: the UCRT exports _isatty, not isatty (mingw's isatty
	// is a static alias; f3b092c978's windows leg, 46029370800).
	name := "isatty"

	if runtime.GOOS == "windows" {
		name = "_isatty"
	}

	runCgoPhase4(t, "package main\n\n/*\nstatic int "+name+"(int fd) { return 42; }\n*/\nimport \"C\"\n\nimport \"fmt\"\n\nfunc main() { fmt.Println(\"tty\", C."+name+"(0)) }\n")
}

// A non-static function one file's preamble defines and another file calls runs as go's build runs it (cgo Phase 4b,
// row 2): any of the package's objects that defines the name globally side-builds it. go-sqlite3's callback.go calls
// _sqlite3_result_text, which sqlite3.go's preamble defines. At Phase 4 it was refused by name (the cross-file gap).
func TestCgoPhase4JAFunctionAnotherFilesPreambleDefinesRunsAsGoDoes(t *testing.T) {
	runCgoPhase4With(t, cgoPhase4Case{sideUnits: 2, files: map[string]string{
		"main.go":   "package main\n\n/*\nint go2cs_p4_helper(void);\n*/\nimport \"C\"\n\nimport \"fmt\"\n\nfunc main() { fmt.Println(\"helper\", C.go2cs_p4_helper()) }\n",
		"helper.go": "package main\n\n/*\nint go2cs_p4_helper(void) { return 9; }\n*/\nimport \"C\"\n",
	}})
}

// The package's own C file is compiled and linked into the side library, with one copy of its global, as in go's
// binary (cgo Phase 4b, row 3). At Phase 4 it was refused by name (design §2.7, "C source files of its own").
func TestCgoPhase4COwnCFileConvertsAndRunsAsGoDoes(t *testing.T) {
	runCgoPhase4Files(t, map[string]string{
		"main.go": "package main\n\n/*\nextern int go2cs_p4_own_count;\nint go2cs_p4_own_bump(int n);\n*/\nimport \"C\"\n\nimport \"fmt\"\n\nfunc main() {\n\tC.go2cs_p4_own_bump(3)\n\tfmt.Println(\"own\", C.go2cs_p4_own_bump(4), C.go2cs_p4_own_count)\n}\n",
		"own.c":   "int go2cs_p4_own_count = 1;\n\nint go2cs_p4_own_bump(int n) { go2cs_p4_own_count += n; return go2cs_p4_own_count; }\n",
	})
}

// A C file of the package's own that preprocesses to nothing compiles to an empty object and takes nothing from the
// side library (cgo Phase 4b, row 4): go-sqlite3's sqlite3-binding.c under libsqlite3. At Phase 4 it was refused with
// row 3's text.
func TestCgoPhase4KAnOwnCFileThatPreprocessesToNothingConvertsAndRunsAsGoDoes(t *testing.T) {
	runCgoPhase4Files(t, map[string]string{
		"main.go": "package main\n\n/*\nstatic int four(void) { return 4; }\n*/\nimport \"C\"\n\nimport \"fmt\"\n\nfunc main() { fmt.Println(\"four\", C.four()) }\n",
		"empty.c": "#ifdef GO2CS_NEVER\nint go2cs_p4_never(void) { return 0; }\n#endif\n",
	})
}

// A name two of the package's objects define globally is refused by name before the side link (cgo Phase 4b, OQ-1,
// COORD's ruling): C's link and go's both reject the duplicate, and a named refusal reads better than a linker error.
// At Phase 4 the name was side-built from the hosting unit's object alone, and the other definition was never compiled.
func TestCgoPhase4LANameTwoObjectsDefineGloballyIsRefusedByName(t *testing.T) {
	probeDir := cgoRealProbeDir(t)
	combined, _, output, err := convertCgoReal(t, probeDir, map[string]string{
		"main.go":  "package main\n\n/*\nint go2cs_p4_dup(void) { return 1; }\n*/\nimport \"C\"\n\nimport \"fmt\"\n\nfunc main() { fmt.Println(\"dup\", C.go2cs_p4_dup()) }\n",
		"other.go": "package main\n\n/*\nint go2cs_p4_dup(void) { return 2; }\n*/\nimport \"C\"\n",
	})
	t.Logf("READING %s/%s: exit %v; the conversion's output ends:\n%s", runtime.GOOS, runtime.GOARCH, err, lastLines(combined, 12))
	requireNothingWritten(t, output, err, combined)
	requireCgoUseRefused(t, cgoRefusalLinesIn(combined, "main.go"), "main.go(", "C.go2cs_p4_dup", "other.go")
}

// A relative #cgo CFLAGS include directory is the package directory's, as cmd/go runs each cgo compile there (review
// of 500b0855c7, (c)): the side compile must find sub/dir/go2cs_rel.h through -Isub/dir.
func TestCgoPhase4GARelativeIncludeDirectoryIsThePackages(t *testing.T) {
	runCgoPhase4Files(t, map[string]string{
		"main.go":             "package main\n\n/*\n#cgo CFLAGS: -Isub/dir\n#include <go2cs_rel.h>\nstatic int rel(void) { return GO2CS_REL; }\n*/\nimport \"C\"\n\nimport \"fmt\"\n\nfunc main() { fmt.Println(\"rel\", C.rel()) }\n",
		"sub/dir/go2cs_rel.h": "#define GO2CS_REL 5\n",
	})
}

// cgoPhase4PathNeedles are the path spellings a side library must not carry: the side build's work-directory stem and,
// trimmed, the temporary root and the package directory, each with either separator. Untrimmed, go itself bakes the
// package's absolute path wherever a header spells __FILE__, and the test's package lies under the temporary root, so
// only the work-directory stem is held (3748d05f94, 46054074306: the root's needle matched the package path go bakes).
func cgoPhase4PathNeedles(module string, trimmed bool) []string {
	needles := []string{"go2cs-cgo-side"}

	if !trimmed {
		return needles
	}

	paths := []string{os.TempDir(), module}

	for _, path := range paths {
		path = strings.TrimRight(path, `/\`)
		needles = append(needles, path, filepath.ToSlash(path), strings.ReplaceAll(path, "/", `\`))
	}

	return needles
}

// runCgoPhase4 converts program as package main, and where the build tests are on, builds it with go and with dotnet
// and compares the two runs.
func runCgoPhase4(t *testing.T, program string) {
	t.Helper()
	runCgoPhase4Files(t, map[string]string{"main.go": program})
}

// runCgoPhase4Files is runCgoPhase4 over a package of several files, main.go among them.
func runCgoPhase4Files(t *testing.T, files map[string]string) {
	t.Helper()
	runCgoPhase4With(t, cgoPhase4Case{files: files})
}

// cgoPhase4Case is one runCgoPhase4With program: its files, and how it is converted, built and run.
type cgoPhase4Case struct {
	files map[string]string

	// untrimmed converts with -trimpath=false and builds go's side without -trimpath. By default go2cs records what
	// `go build -trimpath` bakes (commandLineOptions.go, noTrimPath), so go's side builds with -trimpath.
	untrimmed bool

	// setup runs on the module directory before the conversion.
	setup func(t *testing.T, dir string)

	// runIn is the directory both programs run in, relative to the module directory; "" is an empty directory.
	runIn string

	// refusedAtRun, when set, is text go2cs's run must fail with, where go's runs to its end: a load go2cs refuses on
	// purpose (review of ef369204c9, #1).
	refusedAtRun []string

	// sideUnits is the number of side translation units the conversion writes; 0 means 1 (cgo Phase 4b: a package whose
	// other file defines a name compiles that file's unit too).
	sideUnits int
}

// runCgoPhase4With converts one case, checks the side library, and where the build tests are on, builds the program
// with go and with dotnet and compares the two runs.
func runCgoPhase4With(t *testing.T, c cgoPhase4Case) {
	t.Helper()

	probeDir := cgoRealProbeDir(t)
	target := runtime.GOOS + "/" + runtime.GOARCH
	files := c.files
	var args []string

	if c.untrimmed {
		args = append(args, "-trimpath=false")
	}

	var setup func(string)

	if c.setup != nil {
		setup = func(dir string) { c.setup(t, dir) }
	}

	combined, input, output, err := convertCgoRealSetup(t, probeDir, files, setup, args...)

	if err != nil {
		for _, line := range cgoRefusalLinesIn(combined, "") {
			t.Logf("READING %s: refused: %s", target, line[strings.Index(line, "error "):])
		}

		t.Fatalf("a program whose preamble defines the C it calls did not convert on %s: %v\n%s", target, err, lastLines(combined, 30))
	}

	// The side library and its translation unit, beside the bindings (design §2.4).
	sideDir := filepath.Join(cgoBuildDir(t, output), "cgo", strings.ReplaceAll(target, "/", "-"))

	wantUnits := c.sideUnits

	if wantUnits == 0 {
		wantUnits = 1
	}

	if units, _ := filepath.Glob(filepath.Join(sideDir, "*.side.c")); len(units) != wantUnits {
		t.Errorf("the conversion wrote %d side translation units in %s, want %d", len(units), sideDir, wantUnits)
	}

	if library := filepath.Join(sideDir, cgoSideLibraryFile(runtime.GOOS, "main", "example.com/cgoreal")); !fileExists(library) {
		t.Errorf("the conversion wrote no side library %s", library)
	} else {
		// The library names no conversion path (the owner's machine-path rule; review of 500b0855c7, #2): not the side
		// build's work directory, not the temporary root, and not the package's own directory.
		// Untrimmed, the package directory is excluded: an untrimmed go build bakes it too, wherever a header spells
		// __FILE__ (review of ef369204c9, #2: match Go, not stricter).
		contents, _ := os.ReadFile(library)

		for _, needle := range cgoPhase4PathNeedles(input, !c.untrimmed) {
			if strings.Contains(string(contents), needle) {
				t.Errorf("READING %s: the side library %s carries the conversion path %q", target, filepath.Base(library), needle)
			}
		}

		// No debug information at all (review of ef369204c9, #5): the link strips it.
		if sections := cgoPhase4DebugSections(t, library); len(sections) > 0 {
			t.Errorf("READING %s: the side library %s carries debug sections %v", target, filepath.Base(library), sections)
		}
	}

	if os.Getenv(cgoBuildEnv) != "1" {
		t.Skipf("NOT RUN: set %s=1 to build and run the program (Inconclusive, not a pass)", cgoBuildEnv)
	}

	framework := regexp.MustCompile(`<TargetFramework Condition="[^"]*">([^<]+)</TargetFramework>`).FindStringSubmatch(renderCsprojTemplate("Exe", "", ""))

	if framework == nil {
		t.Fatal("the rendered template names no TargetFramework")
	}

	dotnet := requireDotnetSdk(t, framework[1])

	// go's own build, with this host's compiler, in the module the conversion read, so a path either side bakes is
	// the same path.
	binaries := t.TempDir()
	goProgram := filepath.Join(binaries, "p4go")

	if runtime.GOOS == "windows" {
		goProgram += ".exe"
	}

	// On darwin go's linker runs Apple's strip unless DWARF is off, and strip refuses what the pinned zig links
	// (cgoG1_test.go).
	goBuild := []string{"build", "-o", goProgram}

	if !c.untrimmed {
		goBuild = append(goBuild, "-trimpath")
	}

	if runtime.GOOS == "darwin" {
		goBuild = append(goBuild, "-ldflags=-w")
	}

	build := exec.Command("go", append(goBuild, ".")...)
	build.Dir = input
	build.Env = append(os.Environ(), "GOWORK=off", "GOFLAGS=", "CGO_ENABLED=1", "CGO_LDFLAGS=-L"+probeDir+" -lgo2csprobe")

	if out, err := build.CombinedOutput(); err != nil {
		t.Fatalf("go did not build the program: %v\n%s", err, lastLines(string(out), 30))
	}

	empty := t.TempDir()

	if c.runIn != "" {
		empty = filepath.Join(input, c.runIn)
	}

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

	if len(c.refusedAtRun) > 0 {
		for _, want := range c.refusedAtRun {
			if csRun.exit == 0 || !strings.Contains(csRun.stderr, want) {
				t.Errorf("go2cs was to refuse the load naming %q, and exited %d %q:\n%s", want, csRun.exit, csRun.stdout, lastLines(csRun.stderr, 12))
			}
		}

		return
	}

	if csRun.exit != goRun.exit || csRun.stdout != goRun.stdout {
		t.Errorf("the two runs differ:\n   go: exit %d %q\ngo2cs: exit %d %q\n%s", goRun.exit, goRun.stdout, csRun.exit, csRun.stdout, lastLines(csRun.stderr, 12))
	}
}

// cgoPhase4DebugSections names the debug sections an object file carries: ELF's and PE's .debug_*, Mach-O's __DWARF.
func cgoPhase4DebugSections(t *testing.T, path string) []string {
	t.Helper()

	var sections []string

	switch runtime.GOOS {
	case "windows":
		file, err := pe.Open(path)

		if err != nil {
			t.Fatal(err)
		}

		defer file.Close()

		for _, section := range file.Sections {
			if strings.HasPrefix(section.Name, ".debug") {
				sections = append(sections, section.Name)
			}
		}
	case "darwin":
		file, err := macho.Open(path)

		if err != nil {
			t.Fatal(err)
		}

		defer file.Close()

		for _, section := range file.Sections {
			if section.Seg == "__DWARF" {
				sections = append(sections, section.Seg+","+section.Name)
			}
		}
	default:
		file, err := elf.Open(path)

		if err != nil {
			t.Fatal(err)
		}

		defer file.Close()

		for _, section := range file.Sections {
			if strings.HasPrefix(section.Name, ".debug") {
				sections = append(sections, section.Name)
			}
		}
	}

	return sections
}

// cgoPhase4FileProgram prints the __FILE__ of a header the preamble includes. go bakes the header's path as its build
// sees it: absolute, or under -trimpath the module's path (cmd/go/internal/work/exec.go, the -ffile-prefix-map
// cmd/go hands CC). The side build must bake the same (review of ef369204c9, #2).
var cgoPhase4FileProgram = map[string]string{
	"main.go":      "package main\n\n/*\n#include \"go2cs_file.h\"\n*/\nimport \"C\"\n\nimport \"fmt\"\n\nfunc main() { fmt.Println(\"file\", C.GoString(C.go2cs_file())) }\n",
	"go2cs_file.h": "static char *go2cs_file(void) { return __FILE__; }\n",
}

// cgoPhase4OwnFileProgram is cgoPhase4FileProgram with the preamble's own __FILE__ beside the header's. cmd/cgo names
// the preamble by its absolute path (cmd/cgo/ast.go, ParseGo: #line <n> "<abspath>"), which -trimpath's map turns into
// /_/<module path>/main.go (review of 823978ff1a, #2). Untrimmed, go prints the absolute path, and go2cs keeps the
// base name there, so as not to write a machine path into the side unit (the design, §2.2): the untrimmed arm reads
// the header only.
var cgoPhase4OwnFileProgram = map[string]string{
	"main.go":      "package main\n\n/*\n#include \"go2cs_file.h\"\nstatic char *go2cs_own_file(void) { return __FILE__; }\n*/\nimport \"C\"\n\nimport \"fmt\"\n\nfunc main() { fmt.Println(\"file\", C.GoString(C.go2cs_file()), C.GoString(C.go2cs_own_file())) }\n",
	"go2cs_file.h": cgoPhase4FileProgram["go2cs_file.h"],
}

// A header's __FILE__ is go's under each path mode: trimmed (go2cs's default, against go build -trimpath) and
// untrimmed (-trimpath=false, against a plain go build). Trimmed, so is the preamble's own.
func TestCgoPhase4IAHeadersFileNameIsGosUnderEachPathMode(t *testing.T) {
	t.Run("trimmed", func(t *testing.T) { runCgoPhase4With(t, cgoPhase4Case{files: cgoPhase4OwnFileProgram}) })
	t.Run("untrimmed", func(t *testing.T) { runCgoPhase4With(t, cgoPhase4Case{files: cgoPhase4FileProgram, untrimmed: true}) })
}

// cgoPhase4DepProgram calls, from a side-built preamble function, dep(), which a library the test builds exports
// (review of ef369204c9, #1).
const cgoPhase4DepProgram = "package main\n\n/*\n#cgo LDFLAGS: -L${SRCDIR}/dep -ldep\nint dep(void);\nstatic int via(void) { return dep() + 1; }\n*/\nimport \"C\"\n\nimport \"fmt\"\n\nfunc main() { fmt.Println(\"via\", C.via()) }\n"

// cgoPhase4BuildDep builds the library dep, exporting dep(), into dir/dep with the C compiler go uses: on darwin under
// the bare install name libdep.dylib, on windows as dep.dll with its import library.
func cgoPhase4BuildDep(t *testing.T, dir string) {
	t.Helper()

	_, cc := cgoRealToolchainLine(t)
	source := filepath.Join(t.TempDir(), "dep.c")
	depDir := filepath.Join(dir, "dep")

	if err := os.WriteFile(source, []byte("int dep(void) { return 41; }\n"), 0o644); err != nil {
		t.Fatal(err)
	}

	if err := os.MkdirAll(depDir, 0o755); err != nil {
		t.Fatal(err)
	}

	args := append([]string{}, cc[1:]...)

	switch runtime.GOOS {
	case "darwin":
		args = append(args, "-shared", "-o", filepath.Join(depDir, "libdep.dylib"), source, "-Wl,-install_name,libdep.dylib")
	case "windows":
		args = append(args, "-shared", "-o", filepath.Join(depDir, "dep.dll"), source, "-Wl,--out-implib,"+filepath.Join(depDir, "libdep.dll.a"))
	default:
		t.Fatalf("no dep library shape for %s", runtime.GOOS)
	}

	if out, err := exec.Command(cc[0], args...).CombinedOutput(); err != nil {
		t.Fatalf("the dep library did not build: %v\n%s", err, out)
	}
}

// The side library's dependents load as every Phase 2 binding's do, never by a search that reaches the current
// directory (review of ef369204c9, #1). darwin: a dependent with a bare install name is refused at conversion, by
// name, since dyld would search for it. windows: a dependent only in the current directory does not load, where a go
// binary's import loads it from there.
func TestCgoPhase4HASideLibrarysDependentsLoadOnlyFromSafePlaces(t *testing.T) {
	switch runtime.GOOS {
	case "darwin":
		probeDir := cgoRealProbeDir(t)
		combined, _, output, err := convertCgoRealSetup(t, probeDir, map[string]string{"main.go": cgoPhase4DepProgram}, func(dir string) { cgoPhase4BuildDep(t, dir) })
		t.Logf("READING %s/%s: exit %v; the conversion's output ends:\n%s", runtime.GOOS, runtime.GOARCH, err, lastLines(combined, 8))
		requireNothingWritten(t, output, err, combined)

		for _, want := range []string{"libdep.dylib", "dyld"} {
			if !strings.Contains(combined, want) {
				t.Errorf("the refusal does not name %q", want)
			}
		}
	case "windows":
		runCgoPhase4With(t, cgoPhase4Case{
			files:        map[string]string{"main.go": cgoPhase4DepProgram},
			setup:        cgoPhase4BuildDep,
			runIn:        "dep",
			refusedAtRun: []string{"go2cs-cgo-main-", "the current directory is never searched"},
		})
	default:
		t.Skipf("on %s the side library's dependents load by the dynamic loader's own search, as a go binary's do: NOT MEASURED here", runtime.GOOS)
	}
}

// TestSqlite3ReadingOnTheSideBuild is cgo Phase 4b's go-sqlite3 reading (docs/phase4/DESIGN-cgo-phase4b-own-sources.md
// §5.3; OQ-2, COORD: measure the amalgamation's compile cost, prediction first). It is a reading, not a gate: run it by
// name on a hosted leg (converter-cgo, filter ^TestSqlite3Reading). It downloads github.com/mattn/go-sqlite3 v1.14.53
// through the module proxy and logs two READING lines: where a -cgo conversion of the package stops, and how long the
// side build's own compile of sqlite3-binding.c takes with go list's flags for it. It fails only when it cannot set up.
func TestSqlite3ReadingOnTheSideBuild(t *testing.T) {
	requireHostCC(t)

	env := append(os.Environ(), "GOWORK=off", "GOFLAGS=", "GOTOOLCHAIN=local", "CGO_ENABLED=1")
	download := exec.Command("go", "mod", "download", "-json", "github.com/mattn/go-sqlite3@v1.14.53")
	download.Dir = t.TempDir()
	download.Env = env
	out, err := download.Output()

	if err != nil {
		t.Fatalf("go mod download of go-sqlite3: %v\n%s", err, out)
	}

	var module struct{ Dir string }

	if err := json.Unmarshal(out, &module); err != nil || module.Dir == "" {
		t.Fatalf("go mod download answered no directory: %v\n%s", err, out)
	}

	target := runtime.GOOS + "/" + runtime.GOARCH

	// Where a -cgo conversion of the package stops.
	treeSrc, err := filepath.Abs("..")

	if err != nil {
		t.Fatal(err)
	}

	output := t.TempDir()
	convert := exec.Command(driverBinary(t), "-cgo=true", "-go2cspath", treeSrc, module.Dir, output)
	convert.Env = env
	started := time.Now()
	combined, convertErr := convert.CombinedOutput()
	t.Logf("READING %s go-sqlite3 conversion: exit %v in %s", target, convertErr, time.Since(started).Round(time.Second))

	for _, line := range cgoRefusalLinesIn(string(combined), "") {
		t.Logf("READING %s go-sqlite3 refused: %s", target, line[strings.Index(line, "error "):])
	}

	t.Logf("READING %s go-sqlite3 conversion's output ends:\n%s", target, lastLines(string(combined), 15))

	// The side build's own compile of the amalgamation, with go list's flags for the package (OQ-2).
	load := cgoLoad{env: env, dir: module.Dir}
	toolchain, toolchainLine, err := cgoResolveToolchain(load, runtime.GOOS, runtime.GOARCH)

	if err != nil {
		t.Fatalf("the toolchain: %v", err)
	}

	pkg := &packages.Package{PkgPath: "github.com/mattn/go-sqlite3", Dir: module.Dir}
	listed, err := cgoSideList(load, Options{}, pkg)

	if err != nil {
		t.Fatal(err)
	}

	compileFlags, _, err := cgoSideCompileFlags(load, listed, pkg)

	if err != nil {
		t.Fatal(err)
	}

	trim, err := cgoSideTrimOf(load, listed, true, runtime.GOOS)

	if err != nil {
		t.Fatal(err)
	}

	workDir := t.TempDir()
	object := filepath.Join(workDir, "sqlite3-binding.o")
	started = time.Now()
	_, compileErr := cgoSideCompileOwn(toolchain, runtime.GOOS, compileFlags, filepath.Join(listed.dir(pkg), "sqlite3-binding.c"), object, listed.dir(pkg), cgoSidePrefixMaps(toolchain, workDir, trim))
	elapsed := time.Since(started)
	size := int64(0)

	if info, statErr := os.Stat(object); statErr == nil {
		size = info.Size()
	}

	t.Logf("READING %s go-sqlite3 amalgamation compile: %v in %.1f s, object %d bytes; %s; flags %q", target, compileErr, elapsed.Seconds(), size, toolchainLine, compileFlags)
}
