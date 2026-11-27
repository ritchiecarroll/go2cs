// cgoSideBuild_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"encoding/json"
	"go/ast"
	"go/importer"
	"go/parser"
	"go/token"
	"go/types"
	"os"
	"os/exec"
	"path/filepath"
	"strings"
	"testing"

	"golang.org/x/tools/go/packages"
)

// cgo Phase 4's offline goldens (docs/phase4/DESIGN-cgo-phase4-sidebuild.md §3): the translation unit's text, the
// table's order and the side-call emission, none of which needs a C compiler. The compile, the object read and the link
// run on the hosted converter-cgo and cgo-e2e legs (TestCgoPhase4*). Each golden is the generator's output byte for byte;
// run with GO2CS_UPDATE_CGO_GOLDEN=1 to rewrite them, and read the diff.

// cgoSideGoldenDir holds the goldens.
var cgoSideGoldenDir = filepath.Join("testdata", "cgo", "p4")

// cgoSideFixture is cmd/cgo's declarations for cgoPhase4Program, plus a void function and a preamble variable, as
// cmd/cgo would type them.
func cgoSideFixture(t *testing.T) *types.Package {
	t.Helper()

	source := `package cgotest

type _Ctype_int int32
type _Ctype_void [0]byte

func _Cfunc_add_ints(p0 _Ctype_int, p1 _Ctype_int) (r1 _Ctype_int) { return }
func _Cfunc_twice(p0 _Ctype_int) (r1 _Ctype_int) { return }
func _C2func_halve(p0 _Ctype_int) (r1 _Ctype_int, r2 error) { return }
func _Cfunc_bump() (r1 _Ctype_void) { return }

var _Cvar_counter *_Ctype_int
`

	fset := token.NewFileSet()
	file, err := parser.ParseFile(fset, "cgotest.go", source, 0)

	if err != nil {
		t.Fatal(err)
	}

	pkg, err := (&types.Config{Importer: importer.Default(), Sizes: types.SizesFor("gc", "amd64")}).Check("cgotest", fset, []*ast.File{file}, nil)

	if err != nil {
		t.Fatal(err)
	}

	return pkg
}

// cgoSideFixtureBuild side-builds every fixture name from main.go's table, as the route would for goos.
func cgoSideFixtureBuild(goos string) *cgoSideBuild {
	return &cgoSideBuild{library: cgoSideLibraryFile(goos, "main", "example.com/cgop4"), units: []cgoSideUnit{{table: 0, file: "main.go", names: []string{"add_ints", "bump", "counter", "halve", "twice"}}}}
}

// cgoSideBindings is the pseudo-package text for the fixture, every name side-built, as the route writes it for goos.
func cgoSideBindings(t *testing.T, goos string) string {
	t.Helper()

	target := cgoBindingTarget{goos: goos}

	if goos == "windows" {
		target.crt = "ucrtbase.dll"
	}

	set := newCgoWrapperSet(target, types.SizesFor("gc", "amd64"))
	set.class = cgoPseudoPackageClass("cgotest")
	set.libraries = map[string]string{}
	set.side = cgoSideFixtureBuild(goos).slots()

	members := map[string]string{}

	for _, declaration := range cgoDeclarations(cgoSideFixture(t)) {
		if declaration.kind != cgoFunc && declaration.kind != cgoErrnoFunc && declaration.kind != cgoVar {
			continue
		}

		text, err := set.wrapper(declaration)

		if err != nil {
			t.Fatal(err)
		}

		members[declaration.cName+cgoKindMemberLabel(declaration.kind)] = text
	}

	for name, text := range set.support() {
		members[name] = text
	}

	return cgoPseudoPackageText("cgotest", members)
}

// cgoSideFixtureUnit is main.go's unit for cgoPhase4Program, with a #cgo line ahead of its preamble, whose line cmd/cgo
// blanks: its preamble read from a file, as the route reads it.
func cgoSideFixtureUnit(t *testing.T) cgoSideUnit {
	t.Helper()

	path := filepath.Join(t.TempDir(), "main.go")
	program := strings.Replace(cgoPhase4Program, "/*\n#include", "// #cgo LDFLAGS: -lm\n/*\n#include", 1)

	if err := os.WriteFile(path, []byte(program), 0o644); err != nil {
		t.Fatal(err)
	}

	preamble, err := cgoSidePreamble(path)

	if err != nil {
		t.Fatal(err)
	}

	return cgoSideUnit{table: 0, file: "main.go", preamble: preamble, names: []string{"add_ints", "halve", "twice"}}
}

func requireCgoSideGolden(t *testing.T, name, got string) {
	t.Helper()

	path := filepath.Join(cgoSideGoldenDir, name)

	if os.Getenv("GO2CS_UPDATE_CGO_GOLDEN") == "1" {
		if err := os.WriteFile(path, []byte(strings.ReplaceAll(got, "\n", "\r\n")), 0o644); err != nil {
			t.Fatal(err)
		}

		return
	}

	committed, err := os.ReadFile(path)

	if err != nil {
		t.Fatalf("%s: %v (run with GO2CS_UPDATE_CGO_GOLDEN=1 to write it)", path, err)
	}

	if want := strings.ReplaceAll(string(committed), "\r\n", "\n"); got != want {
		t.Errorf("%s is not the generator's output; run with GO2CS_UPDATE_CGO_GOLDEN=1 and read the diff\n got:\n%s\nwant:\n%s", path, got, want)
	}
}

// The translation unit (§2.2, §5): cmd/cgo's prolog include, the preamble under its file's #line with the #cgo line
// blanked, the errno location, and the exported table in name order from slot 1.
func TestCgoSideUnitGoldenIsTheGeneratorsOutput(t *testing.T) {
	requireCgoSideGolden(t, "main.go.side.c.golden", cgoSideUnitText(cgoSideFixtureUnit(t)))
}

// The side-call emission (§2.5): on linux a function pointer through the table, with the errno form's SetLastError
// sequence; on windows the errno form reads the table's slot 0. A variable boxes its slot (OQ-A).
func TestCgoSideBindingsGoldensAreTheGeneratorsOutput(t *testing.T) {
	for _, goos := range []string{"linux", "darwin", "windows"} {
		requireCgoSideGolden(t, "bindings."+goos+".golden", cgoSideBindings(t, goos))
	}
}

// The preamble is read as cmd/cgo reads it: the spec's doc comment, or the declaration's for a lone spec, under a #line
// at the comment's first line, // comments as lines, and every #cgo line blanked, so the C keeps its line numbers.
func TestCgoSidePreambleIsReadAsCmdCgoReadsIt(t *testing.T) {
	path := filepath.Join(t.TempDir(), "two.go")
	source := "package two\n\n// #cgo CFLAGS: -DX=1\n// static int one(void) { return 1; }\nimport \"C\"\n\nimport (\n\t\"fmt\"\n\n\t/*\n\t  #cgo LDFLAGS: -lm\n\t  static int two(void) { return 2; }\n\t*/\n\t\"C\"\n)\n\nvar _ = fmt.Sprint\n"

	if err := os.WriteFile(path, []byte(source), 0o644); err != nil {
		t.Fatal(err)
	}

	got, err := cgoSidePreamble(path)

	if err != nil {
		t.Fatal(err)
	}

	want := "#line 3 \"two.go\"\n\n static int one(void) { return 1; }\n\n#line 10 \"two.go\"\n\n\n\t  static int two(void) { return 2; }\n\t\n"

	if got != want {
		t.Errorf("the preamble read\n%q\nwant\n%q", got, want)
	}
}

// Slot 0 is the errno location; each unit's names take slots 1.. in their order, and every name names its table.
func TestCgoSideSlotsAreNameOrderFromSlotOne(t *testing.T) {
	build := &cgoSideBuild{library: "x", units: []cgoSideUnit{{table: 1, file: "a.go", names: []string{"f", "g"}}, {table: 3, file: "c.go", names: []string{"f2"}}}}
	slots := build.slots()

	for name, want := range map[string]cgoSideSlot{
		"f":  {table: 1, slot: 1, file: "a.go", library: "x"},
		"g":  {table: 1, slot: 2, file: "a.go", library: "x"},
		"f2": {table: 3, slot: 1, file: "c.go", library: "x"},
	} {
		if slots[name] != want {
			t.Errorf("%s: slot %+v, want %+v", name, slots[name], want)
		}
	}

	if got, want := slots["f2"].tableSymbol(), "go2cs_cgo_table_3"; got != want {
		t.Errorf("table symbol %q, want %q", got, want)
	}

	if got := (*cgoSideBuild)(nil).slots(); len(got) != 0 {
		t.Errorf("no build side-builds %d names", len(got))
	}
}

func TestCgoSideLibraryFileTakesTheTargetsForm(t *testing.T) {
	for goos, want := range map[string]string{
		"linux":   "libgo2cs-cgo-main-a9d3bf20.so",
		"darwin":  "libgo2cs-cgo-main-a9d3bf20.dylib",
		"windows": "go2cs-cgo-main-a9d3bf20.dll",
	} {
		if got := cgoSideLibraryFile(goos, "main", "example.com/cgop4"); got != want {
			t.Errorf("%s: %q, want %q", goos, got, want)
		}
	}
}

// The project copies the one library, flat, beside the assembly, once.
func TestCgoSideLibraryItemCopiesTheLibraryBesideTheAssembly(t *testing.T) {
	project := "<Project Sdk=\"Microsoft.NET.Sdk\">\r\n</Project>\r\n"
	once := cgoSideLibraryItem(project, "linux-amd64", "libgo2cs-cgo-main-a9d3bf20.so")

	if !strings.Contains(once, `<None Include="cgo\linux-amd64\libgo2cs-cgo-main-a9d3bf20.so" Link="libgo2cs-cgo-main-a9d3bf20.so" CopyToOutputDirectory="PreserveNewest" />`) {
		t.Errorf("the project does not copy the library beside the assembly:\n%s", once)
	}

	if !strings.HasSuffix(once, "</Project>\r\n") {
		t.Errorf("the item is not inside the project:\n%s", once)
	}

	if twice := cgoSideLibraryItem(once, "linux-amd64", "libgo2cs-cgo-main-a9d3bf20.so"); twice != once {
		t.Errorf("a second write adds a second item:\n%s", twice)
	}
}

// A package with C, C++, Objective-C or assembly files of its own is named (§2.7); headers are not sources.
func TestCgoSideOwnSourcesNamesWhatCmdGoCompiles(t *testing.T) {
	pkg := &packages.Package{OtherFiles: []string{filepath.Join("d", "z.h"), filepath.Join("d", "b.c"), filepath.Join("d", "a.S"), filepath.Join("d", "README.md")}}

	if got, want := strings.Join(cgoSideOwnSources(pkg), ","), "a.S,b.c"; got != want {
		t.Errorf("own sources %q, want %q", got, want)
	}

	if got := cgoSideOwnSources(&packages.Package{}); got != nil {
		t.Errorf("a package with no other files has own sources %v", got)
	}
}

// A function a library exports keeps its [LibraryImport] extern: the side form is the side-built name's alone.
func TestCgoSideFormIsTheSideBuiltNamesAlone(t *testing.T) {
	set := newCgoWrapperSet(cgoBindingTarget{goos: "linux", library: "libc.so.6"}, types.SizesFor("gc", "amd64"))
	set.class = cgoPseudoPackageClass("cgotest")
	set.side = map[string]cgoSideSlot{"twice": {table: 0, slot: 1, file: "main.go", library: "libx.so"}}

	for _, declaration := range cgoDeclarations(cgoSideFixture(t)) {
		if declaration.cName != "add_ints" && declaration.cName != "twice" {
			continue
		}

		text, err := set.wrapper(declaration)

		if err != nil {
			t.Fatal(err)
		}

		side := strings.Contains(text, "delegate* unmanaged[Cdecl]")
		extern := strings.Contains(text, "LibraryImport(\"libc.so.6\"")

		if want := declaration.cName == "twice"; side != want || extern == want {
			t.Errorf("C.%s: side form %v, extern %v; want the side form only for the side-built name\n%s", declaration.cName, side, extern, text)
		}
	}
}

// A side compile or link that fails is refused naming the file and the compiler's first diagnostic line, printable
// ASCII only and capped, never the compiler's whole output (review of 500b0855c7, #7).
func TestCgoSideFailureNamesTheFileAndTheFirstDiagnostic(t *testing.T) {
	output := "In file included from main.go:3:\nmain.go:5:3: error: unknown type name 'flot'\nmain.go:9:1: error: second\n"

	if got, want := cgoSideFailure("compile", "main.go", output).Error(), "cgo: the side build of main.go's preamble did not compile: main.go:5:3: error: unknown type name 'flot'"; got != want {
		t.Errorf("compile failure %q, want %q", got, want)
	}

	if got, want := cgoSideFailure("link", "libgo2cs-cgo-main-a9d3bf20.so", "ld: cannot find -lnone\r\ncollect2: ld returned 1 exit status\r\n").Error(), "cgo: the side build of libgo2cs-cgo-main-a9d3bf20.so's preamble did not link: ld: cannot find -lnone"; got != want {
		t.Errorf("link failure %q, want %q", got, want)
	}

	if got := cgoSideFailure("link", "x", "no diagnostic word\nsecond line\n").Error(); got != "cgo: the side build of x's preamble did not link: no diagnostic word" {
		t.Errorf("a failure with no error line reads %q", got)
	}

	if got := cgoSideFailure("compile", "x", "error \u2028"+strings.Repeat("e", 200)).Error(); strings.ContainsRune(got, '\u2028') || len(got) > len("cgo: the side build of x's preamble did not compile: ")+cgoToolchainLineLimit {
		t.Errorf("a failure line is not sanitized and capped: %q", got)
	}
}

// pkg-config's refusal names the function and every pkg-config argument (review of 500b0855c7, #1).
func TestCgoSidePkgConfigRefusalNamesTheFunctionAndTheArguments(t *testing.T) {
	got := cgoSidePkgConfigRefusal("pc", []string{"--static", "fuse"})

	for _, want := range []string{"C.pc:", "#cgo pkg-config (--static fuse)", "refused"} {
		if !strings.Contains(got, want) {
			t.Errorf("the refusal %q does not carry %q", got, want)
		}
	}
}

// A reconversion removes what an earlier side build wrote and this one does not, and nothing else (review of
// 500b0855c7, #7).
func TestCgoSideBuildCleanRemovesOnlyStaleSideOutput(t *testing.T) {
	dir := t.TempDir()

	for _, name := range []string{"a.go.side.c", "b.go.side.c", "libgo2cs-cgo-main-a9d3bf20.so", "go2cs-cgo-old-00000000.dll", "notes.txt", "libother.so"} {
		if err := os.WriteFile(filepath.Join(dir, name), nil, 0o644); err != nil {
			t.Fatal(err)
		}
	}

	build := &cgoSideBuild{library: "libgo2cs-cgo-main-a9d3bf20.so", units: []cgoSideUnit{{file: "a.go"}}}

	if err := cgoSideBuildClean(dir, build); err != nil {
		t.Fatal(err)
	}

	var left []string

	entries, _ := os.ReadDir(dir)

	for _, entry := range entries {
		left = append(left, entry.Name())
	}

	if got, want := strings.Join(left, ","), "a.go.side.c,libgo2cs-cgo-main-a9d3bf20.so,libother.so,notes.txt"; got != want {
		t.Errorf("left %q, want %q", got, want)
	}

	if err := cgoSideBuildClean(dir, nil); err != nil {
		t.Fatal(err)
	}

	left = nil
	entries, _ = os.ReadDir(dir)

	for _, entry := range entries {
		left = append(left, entry.Name())
	}

	if got, want := strings.Join(left, ","), "libother.so,notes.txt"; got != want {
		t.Errorf("with nothing side-built, left %q, want %q", got, want)
	}

	if err := cgoSideBuildClean(filepath.Join(dir, "none"), nil); err != nil {
		t.Errorf("a folder that does not exist: %v", err)
	}
}

// cmd/go's thread flag, per target (work/exec.go, compilerCmd).
func TestCgoSideThreadFlagIsCmdGos(t *testing.T) {
	for goos, want := range map[string]string{"windows": "-mthreads", "linux": "-pthread", "darwin": "-pthread"} {
		if got := cgoSideThreadFlag(goos); got != want {
			t.Errorf("%s: %q, want %q", goos, got, want)
		}
	}
}

// The darwin dependent check (review of ef369204c9, #1), offline: an absolute install name passes; a bare name and an
// @rpath name are refused, naming the library.
func TestCgoDarwinInstallNameRefusesWhatDyldWouldSearch(t *testing.T) {
	if err := cgoDarwinInstallName("libx.dylib", "/usr/lib/libSystem.B.dylib"); err != nil {
		t.Errorf("an absolute install name is refused: %v", err)
	}

	for _, name := range []string{"libdep.dylib", "@rpath/libdep.dylib", "@loader_path/libdep.dylib"} {
		if err := cgoDarwinInstallName("libx.dylib", name); err == nil || !strings.Contains(err.Error(), name) {
			t.Errorf("%s: refused %v, want a refusal naming it", name, err)
		}
	}
}

// The side compile's -trimpath mapping is cmd/go's (review of ef369204c9, #2): a module's directory to /_/<module path>,
// a module-cache directory with its version, GOROOT to /_/GOROOT whatever the setting, and nothing else untrimmed.
func TestCgoSideTrimMapsAsCmdGoDoes(t *testing.T) {
	root := t.TempDir()
	module := filepath.Join(root, "mod")
	cached := filepath.Join(root, "cache", "example.com", "dep@v1.2.3")
	goRoot := filepath.Join(root, "goroot")

	for dir, goMod := range map[string]string{module: "module example.com/mod\n", cached: "module example.com/dep\n"} {
		if err := os.MkdirAll(filepath.Join(dir, "sub"), 0o755); err != nil {
			t.Fatal(err)
		}

		if err := os.WriteFile(filepath.Join(dir, "go.mod"), []byte(goMod), 0o644); err != nil {
			t.Fatal(err)
		}
	}

	if err := os.MkdirAll(filepath.Join(goRoot, "src", "net"), 0o755); err != nil {
		t.Fatal(err)
	}

	slash := func(goos, path string) string {
		if goos == "windows" {
			return filepath.Join(`\\_\_`, path)
		}

		return filepath.Join("/_", path)
	}

	for _, goos := range []string{"linux", "windows"} {
		for _, row := range []struct {
			name     string
			trim     cgoSideTrim
			from, to string
		}{
			{"module", cgoSideTrim{trimmed: true, packageDir: filepath.Join(module, "sub")}, module, slash(goos, "example.com/mod")},
			{"module cache", cgoSideTrim{trimmed: true, packageDir: filepath.Join(cached, "sub")}, cached, slash(goos, "example.com/dep@v1.2.3")},
			{"GOROOT, untrimmed", cgoSideTrim{goRoot: goRoot, packageDir: filepath.Join(goRoot, "src", "net")}, goRoot, slash(goos, "GOROOT")},
			{"module, untrimmed", cgoSideTrim{packageDir: filepath.Join(module, "sub")}, "", ""},
		} {
			row.trim.goos = goos

			if row.trim.goRoot == "" {
				row.trim.goRoot = goRoot
			}

			if from, to := row.trim.mapping(); from != row.from || to != row.to {
				t.Errorf("%s/%s: maps %q to %q, want %q to %q", goos, row.name, from, to, row.from, row.to)
			}
		}
	}
}

// cgoSideTrimFixture writes three module shapes under root that a trim re-derived from a directory's name and go.mod
// maps wrongly (review of 823978ff1a, #1): a module replaced by a local directory, whose go list Version is the
// required one; a main module in a directory whose name holds an "@"; and a vendored module, whose go list Dir is
// empty.
func cgoSideTrimFixture(t *testing.T, root string) {
	t.Helper()

	replaced := "module example.com/main\n\ngo 1.26\n\nrequire example.com/dep v0.0.0\n\nreplace example.com/dep => ../dep\n"
	imports := "package main\n\nimport _ \"example.com/dep\"\n\nfunc main() {}\n"

	for path, contents := range map[string]string{
		"dep/go.mod":    "module example.com/dep\n\ngo 1.26\n",
		"dep/dep.go":    "package dep\n",
		"main/go.mod":   replaced,
		"main/main.go":  imports,
		"at@dir/go.mod": "module example.com/at\n\ngo 1.26\n",
		"at@dir/at.go":  "package at\n",
		"vend/go.mod":   strings.Replace(replaced, "example.com/main", "example.com/vend", 1),
		"vend/main.go":  imports,
	} {
		full := filepath.Join(root, filepath.FromSlash(path))

		if err := os.MkdirAll(filepath.Dir(full), 0o755); err != nil {
			t.Fatal(err)
		}

		if err := os.WriteFile(full, []byte(contents), 0o644); err != nil {
			t.Fatal(err)
		}
	}

	vendor := exec.Command("go", "mod", "vendor")
	vendor.Dir = filepath.Join(root, "vend")
	vendor.Env = append(os.Environ(), "GOWORK=off", "GOFLAGS=", "GOTOOLCHAIN=local")

	if out, err := vendor.CombinedOutput(); err != nil {
		t.Fatalf("go mod vendor: %v\n%s", err, out)
	}
}

// The trim maps from the module go list reports, as cmd/go does (work/exec.go, ccompile: m.Dir to m.Path@m.Version,
// and a vendored module's vendor directory to "vendor"), never from a module re-derived from the package directory's
// name and go.mod (review of 823978ff1a, #1). Each expected mapping is written from exec.go's rule, not computed.
func TestCgoSideTrimTakesTheModuleGoListReports(t *testing.T) {
	root := t.TempDir()
	cgoSideTrimFixture(t, root)

	slash := func(goos, path string) string {
		if goos == "windows" {
			return filepath.Join(`\\_\_`, path)
		}

		return filepath.Join("/_", path)
	}

	for _, goos := range []string{"linux", "windows"} {
		for _, row := range []struct {
			name, loadDir, pattern string
			from, to               string
		}{
			{"replaced by a local directory", "main", "example.com/dep", filepath.Join(root, "dep"), slash(goos, "example.com/dep@v0.0.0")},
			{"an @ in the main module's directory", "at@dir", ".", filepath.Join(root, "at@dir"), slash(goos, "example.com/at")},
			{"vendored", "vend", "example.com/dep", filepath.Join(root, "vend", "vendor"), slash(goos, "vendor")},
		} {
			load := cgoLoad{env: append(os.Environ(), "GOWORK=off", "GOFLAGS=", "GOTOOLCHAIN=local"), dir: filepath.Join(root, row.loadDir)}
			trim := cgoSideTrimForTest(t, load, row.pattern, goos)

			if from, to := trim.mapping(); from != row.from || to != row.to {
				t.Errorf("%s/%s: maps %q to %q, want %q to %q", goos, row.name, from, to, row.from, row.to)
			}
		}
	}
}

// cgoSideTrimForTest builds the trim for one package as the conversion builds it.
func cgoSideTrimForTest(t *testing.T, load cgoLoad, pattern, goos string) cgoSideTrim {
	t.Helper()

	cmd := exec.Command("go", "list", "-json=Dir,ImportPath", pattern)
	cmd.Env = load.env
	cmd.Dir = load.dir
	out, err := cmd.Output()

	if err != nil {
		t.Fatalf("go list %s in %s: %v", pattern, load.dir, err)
	}

	var listed struct{ Dir, ImportPath string }

	if err := json.Unmarshal(out, &listed); err != nil {
		t.Fatal(err)
	}

	// As cgoSideBuild builds it at 823978ff1a (cgoSideBuild.go:844).
	return cgoSideTrim{trimmed: true, goos: goos, packageDir: listed.Dir, importPath: listed.ImportPath}
}
