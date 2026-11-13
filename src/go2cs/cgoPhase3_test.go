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
	"fmt"
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

// cgoPhase3Offline is the Phase 3 fixture as checked-in files, with the dynamic imports a real host's go list hands
// back once the package links: one for each C symbol the offline capture's table names.
func cgoPhase3Offline(t *testing.T) map[string]string {
	t.Helper()

	files := cgoFixtureFiles(t, cgoPhase3FixtureDir)
	files["cgo_import.go"] = cgoOfflineImportFile(cgoPhase2Libraries)

	return files
}

// cgoBindingsMember is the text of one member of a bindings file: from the line that declares it to the line that
// closes it.
func cgoBindingsMember(t *testing.T, bindings, declaration string) string {
	t.Helper()

	start := strings.Index(bindings, declaration)

	if start < 0 {
		t.Fatalf("the bindings file declares no %q", declaration)
	}

	member := bindings[start:]

	if end := strings.Index(member, "\n    }\n"); end >= 0 {
		member = member[:end]
	}

	return member
}

// §16.1: the Phase 3 fixture converts, where every one of its C uses was refused. What it wrote is read here as text;
// TestCgoPhase3FixturesOutputBuildsAsAProject compiles it and the end-to-end program runs it.
func TestCgoPhase3FixtureConvertsItsHelpersVoidPointersAndDeferredCalls(t *testing.T) {
	stderr, output, err := convertCgoOffline(t, cgoPhase3Offline(t))

	if err != nil {
		t.Fatalf("the conversion failed: %v\n%s", err, stderr)
	}

	bindings := strings.ReplaceAll(readConverted(t, output, filepath.Join("linux-amd64", "cgophase3.cgo.cs")), "\r\n", "\n")
	converted := strings.ReplaceAll(readConverted(t, output, "fixture.cs"), "\r\n", "\n")

	// A. Each of cmd/cgo's helpers is a member of the pseudo-package, over golib's copies.
	for declaration, calls := range map[string][]string{
		"internal static ж<@char> CString(@string p0)":                     {"ᴍallocate(", "GoCgoArgs.StringSize(p0)", "GoCgoArgs.CopyString(p0, address)"},
		"internal static @unsafe.Pointer CBytes(slice<byte> p0)":           {"ᴍallocate(", "GoCgoArgs.BytesSize(p0)", "GoCgoArgs.CopyBytes(p0, address)"},
		"internal static @string GoString(ж<@char> p0)":                    {`GoCgoArgs.GoString(global::go.golib.GoCgoArgs.HelperPointer(p0, "GoString"))`},
		"internal static @string GoStringN(ж<@char> p0, @int p1)":          {`GoCgoArgs.HelperPointer(p0, "GoStringN")`, "(int)(int32)p1"},
		"internal static slice<byte> GoBytes(@unsafe.Pointer p0, @int p1)": {`GoCgoArgs.HelperVoidPointer(p0, "GoBytes")`, "(int)(int32)p1"},
		"internal static @unsafe.Pointer malloc(@ulong p0)":                {"ᴍallocate((nuint)(uint64)p0)"},
	} {
		member := cgoBindingsMember(t, bindings, declaration)

		for _, call := range calls {
			if !strings.Contains(member, call) {
				t.Errorf("%s does not hold %q:\n%s", declaration, call, member)
			}
		}
	}

	// The allocator is the package's own malloc, bound once, in the library the capture named for it.
	if count := strings.Count(bindings, `LibraryImport("libc.so.6", EntryPoint = "malloc")`); count != 1 {
		t.Errorf("the bindings file binds malloc %d times in libc.so.6, want 1", count)
	}

	// No helper is bound as a C symbol: they are Go functions, and no library holds one.
	for name := range cgoBuiltinNames {
		if strings.Contains(bindings, `EntryPoint = "`+name+`"`) {
			t.Errorf("the bindings file binds cmd/cgo's helper %s as a C symbol", name)
		}
	}

	// B. A void pointer is classified before the transition, and a void pointer result is its number.
	for _, function := range []string{"free", "go2cs_probe_release", "go2cs_probe_same", "go2cs_probe_sum"} {
		member := cgoBindingsMember(t, bindings, " "+function+"(@unsafe.Pointer p0")
		classified := strings.Index(member, `GoCgoArgs.VoidPointer(p0, "`+function+`")`)
		entered := strings.Index(member, "syscallEntersyscall()")

		if classified < 0 || entered < 0 || classified > entered {
			t.Errorf("C.%s does not classify its void pointer before the transition:\n%s", function, member)
		}
	}

	if member := cgoBindingsMember(t, bindings, "internal static @unsafe.Pointer go2cs_probe_same("); !strings.Contains(member, "return new @unsafe.Pointer((uintptr)(nuint)r);") {
		t.Errorf("C.go2cs_probe_same does not return its result as a number:\n%s", member)
	}

	// P7: every native call sits inside the transition bracket, so externs equal brackets.
	externs, brackets := strings.Count(bindings, "LibraryImport("), strings.Count(bindings, "syscallEntersyscall();")
	t.Logf("READING the Phase 3 fixture's bindings: %d externs, %d transition brackets", externs, brackets)

	if externs == 0 || externs != brackets {
		t.Errorf("the bindings file holds %d externs and %d transition brackets", externs, brackets)
	}

	// C. Each deferred call through the check closure is a deferred call of the bound function, its argument evaluated
	// at the defer, and none of cmd/cgo's own variables reaches the C#.
	if count := strings.Count(converted, "defer(C.free, "); count != 6 {
		t.Errorf("fixture.cs defers C.free %d times, want 6", count)
	}

	if count := strings.Count(converted, "defer(C.go2cs_probe_release, "); count != 2 {
		t.Errorf("fixture.cs defers C.go2cs_probe_release %d times, want 2", count)
	}

	if !regexp.MustCompile(`defer\(C\.go2cs_probe_log_put, [^,]+, value, ref ᒐ\);\n\s*value = 9;`).MatchString(converted) {
		t.Errorf("the deferred call's argument is not handed over at the defer, ahead of its reassignment:\n%s", lastLines(converted, 40))
	}

	if strings.Contains(converted, "_cgo") {
		t.Error("one of cmd/cgo's own variables reached fixture.cs")
	}
}

// The one address of Go memory that reaches a void pointer as a bare number with no uintptr in the source: inside a
// method with a pointer receiver, unsafe.Pointer of a pointer of the receiver's own type. The receiver converts by
// reference, and the converter mints that conversion from a reference with no box to carry. Read while the pin
// moments were listed for the review of the void pointer's door: the conversion handed C.f `(uintptr)FromRef(ref h)`,
// and for a local copy of the receiver `FromRef(ref p)`, the address of the local itself. The route refuses each form
// by name, before anything is written. The control beside them is the way round the refusal names: a function that
// takes the pointer as a parameter converts, and mints it with its box.
func TestCgoUnsafePointerOfAMethodsOwnReceiverTypeIsRefusedByName(t *testing.T) {
	const closure = "func() _cgo_unsafe.Pointer { _cgo0 := %s; _cgoCheckPointer(_cgo0, nil); return _Cfunc_go2cs_probe_same(_cgo0) }() != nil"

	method := func(name, prelude, argument string) string {
		return "\nfunc (h *holder) " + name + "(other *holder) bool {\n" + prelude + "\treturn " + fmt.Sprintf(closure, argument) + "\n}\n"
	}

	holder := "\ntype holder struct{ n _Ctype_int }\n"

	for _, arm := range []struct {
		name    string
		methods string
		refused int
	}{
		{"the receiver, handed to C as it is", method("Same", "", "unsafe.Pointer(h)"), 1},
		{"the receiver, as a number in a variable", method("Number", "\tn := uintptr(unsafe.Pointer(h))\n", "unsafe.Pointer(n)"), 1},
		{"the receiver, under parentheses", method("Wrapped", "", "unsafe.Pointer((h))"), 1},
		{"a local copy of the receiver", method("Local", "\tp := h\n", "unsafe.Pointer(p)"), 1},
		{"another pointer of the receiver's type", method("Other", "", "unsafe.Pointer(other)"), 1},
		// The review of 3.1, finding 5: the walk read the receiver's type through its NAME, so a receiver with no name,
		// or the blank one, was skipped, and the same conversion crossed.
		{"a pointer of the type of a receiver with no name", "\nfunc (*holder) Unnamed(other *holder) bool {\n\treturn " + fmt.Sprintf(closure, "unsafe.Pointer(other)") + "\n}\n", 1},
		{"a pointer of the type of a blank receiver", "\nfunc (_ *holder) Blank(other *holder) bool {\n\treturn " + fmt.Sprintf(closure, "unsafe.Pointer(other)") + "\n}\n", 1},
		// The control: a function takes the pointer as a parameter, which carries its box.
		{"in a function that takes the pointer as a parameter", "\nfunc same(p *holder) bool {\n\treturn " + fmt.Sprintf(closure, "unsafe.Pointer(p)") + "\n}\n", 0},
	} {
		t.Run(arm.name, func(t *testing.T) {
			files := cgoPhase3Offline(t)
			files["fixture.cgo1.go"] += holder + arm.methods

			stderr, output, err := convertCgoOffline(t, files)
			lines := cgoRefusalLinesIn(stderr, "")

			if arm.refused == 0 {
				if err != nil {
					t.Fatalf("the control did not convert: %v\n%s", err, lastLines(stderr, 12))
				}

				if converted := readConverted(t, output, "fixture.cs"); !regexp.MustCompile(`C\.go2cs_probe_same\((\(uintptr\))?@unsafe\.Pointer\.From(Pinned)?Box\(Ꮡ?p\)\)`).MatchString(converted) {
					t.Errorf("the parameter is not minted with its box:\n%s", lastLines(converted, 14))
				}

				return
			}

			requireNothingWritten(t, output, err, stderr)

			if len(lines) != arm.refused {
				t.Errorf("%d refusal lines, want %d:\n%s", len(lines), arm.refused, strings.Join(lines, "\n"))
			}

			requireCgoUseRefused(t, lines, "unsafe.Pointer of a pointer of the method's own receiver type", "takes it as a parameter")
		})
	}
}

// The compile half of the test above: what the Phase 3 fixture converts to builds as a project.
func TestCgoPhase3FixturesOutputBuildsAsAProject(t *testing.T) {
	buildCgoOfflineFixture(t, "Phase 3", cgoPhase3Offline(t))
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
// 0 both ways. The arms with csFails hand C or a helper a pointer into Go memory: Go runs each to its end, and go2cs
// refuses each by name, naming the increment that admits it. innerdefer and innergo are the controls of goinnerdefer
// and goinnergo: the same deferred or started Go function over C memory runs to its end both ways.
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
		{name: "innerdefer"},
		{name: "innergo"},
		{name: "goinnerdefer", csFails: cgoP3GoMemory},
		{name: "goinnergo", csFails: cgoP3GoMemory},
		{name: "govoid", csFails: cgoP3GoMemory},
		{name: "govoidnumber", csFails: cgoP3GoMemory},
		{name: "govoidexpr", csFails: cgoP3GoMemory},
		{name: "gobytes", csFails: cgoP3GoMemory},
		{name: "gostring", csFails: cgoP3GoMemory},
		// Pointers DERIVED from a view of Go memory (the review of increment 3.1, §16.11).
		{name: "goviewfield", csFails: cgoP3GoMemory},
		{name: "goviewfieldarray", csFails: cgoP3GoMemory},
		{name: "goviewagain", csFails: cgoP3GoMemory},
		{name: "goviewelem", csFails: cgoP3GoMemory},
		{name: "goviewelemstring", csFails: cgoP3GoMemory},
		{name: "goslice", csFails: cgoP3GoMemory},
		{name: "goslicedata", csFails: cgoP3GoMemory},
		{name: "goadd", csFails: cgoP3GoMemory},
		{name: "goaddlocal", csFails: cgoP3GoMemory},
		// A pointer that reaches C as the result of a call, through a variable and through a user function.
		{name: "goaddvar", csFails: cgoP3GoMemory},
		{name: "goaddfunc", csFails: cgoP3GoMemory},
		{name: "goviewfieldfunc", csFails: cgoP3GoMemory},
		{name: "goviewfieldvar", csFails: cgoP3GoMemory},
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

			// An arm Go itself ends (a negative count) ends the same way here: the same exit status, and the same first
			// line on standard error, which says whether it was a panic or a fatal error and with what text.
			if goRun.exit != 0 && cgoP3FirstLine(goRun.stderr) != cgoP3FirstLine(csRun.stderr) {
				t.Errorf("the two runs end differently:\n   go: %s\ngo2cs: %s", cgoP3FirstLine(goRun.stderr), cgoP3FirstLine(csRun.stderr))
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
