// cgoDeclarations_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Phase 1 of docs/PLAN-cgo-interop.md: the recognizer and the source mapping, over the declarations cmd/cgo writes.
// The first test type-checks cgo-shaped declarations written here. The fixture tests read cmd/cgo's real output for
// lane P1's five-kind fixture, checked in under testdata/cgo/phase1 (its README says how it was generated): cgo's
// rewriting needs a C compiler, but what it writes is plain Go that loads without one. Every test runs on any host.

package main

import (
	"encoding/base64"
	"encoding/json"
	"errors"
	"go/ast"
	"go/importer"
	"go/parser"
	"go/token"
	"go/types"
	"io/fs"
	"os"
	"os/exec"
	"path/filepath"
	"runtime"
	"slices"
	"strings"
	"testing"

	"golang.org/x/tools/go/packages"
)

// cgoShapedSource carries one declaration of each recognized kind, cgo's plumbing as P1 listed it, and an ordinary
// Go declaration, under the names cmd/cgo gives them.
const cgoShapedSource = `package cgoshaped

import "unsafe"

type _Ctype_int int32
type _Ctype_char int8
type _Ctype_double float64
type _Ctype_void [0]byte
type _Ctype_struct_x struct {
	a    _Ctype_int
	next *_Ctype_struct_x
}
type _Ctype_union_u [8]byte
type _Ctype_enum_color uint32
type _Ctype_xptr = *_Ctype_struct_x

const _Ciconst_CONST = 42
const _Ciconst_GREEN = 5
const _Cfconst_FCONST = 2.5
const _Csconst_SCONST = "hi"

var _Cvar_counter *_Ctype_int

func _Cfunc_add(p0 _Ctype_int, p1 _Ctype_int) (r1 _Ctype_int) { return p0 + p1 }
func _Cfunc_free(p0 unsafe.Pointer) (r1 _Ctype_void)         { return }
func _Cfunc_CString(s string) *_Ctype_char                    { return nil }
func _Cfunc__CMalloc(n uintptr) unsafe.Pointer                { return nil }

func _C2func_halve(p0 _Ctype_int) (r1 _Ctype_int, r2 error) { return }
func _Cmacro_NEXT() (r1 _Ctype_int)                          { return }

var _Cfpvar_fp_twice unsafe.Pointer

var _cgo_0123456789ab_Cfunc_add byte
var __cgofn__cgo_0123456789ab_Cfunc_add byte
var _Cgo_always_false bool

func _Cgo_use(interface{})                {}
func _Cgo_ptr(p unsafe.Pointer) unsafe.Pointer { return p }
func _cgoCheckPointer(interface{}, interface{}) {}

func Add(a, b int) int { return int(_Cfunc_add(_Ctype_int(a), _Ctype_int(b))) }
`

func TestCgoDeclarationsClassifiesEachKindAndFiltersThePlumbing(t *testing.T) {
	fset := token.NewFileSet()
	file, err := parser.ParseFile(fset, "cgoshaped.go", cgoShapedSource, 0)

	if err != nil {
		t.Fatal(err)
	}

	config := types.Config{Importer: importer.Default()}
	pkg, err := config.Check("cgoshaped", fset, []*ast.File{file}, nil)

	if err != nil {
		t.Fatal(err)
	}

	want := map[string]cgoDeclKind{
		"CONST":      cgoIntConst,
		"CString":    cgoFunc,
		"FCONST":     cgoFloatConst,
		"GREEN":      cgoIntConst,
		"NEXT":       cgoMacro,
		"SCONST":     cgoStringConst,
		"add":        cgoFunc,
		"char":       cgoType,
		"counter":    cgoVar,
		"double":     cgoType,
		"enum_color": cgoEnum,
		"free":       cgoFunc,
		"halve":      cgoErrnoFunc,
		"int":        cgoType,
		"malloc":     cgoFunc, // cmd/cgo's _Cfunc__CMalloc, which is what a source's C.malloc names (cgoMallocWrapper)
		"struct_x":   cgoStruct,
		"twice":      cgoFuncValue,
		"union_u":    cgoUnion,
		"void":       cgoType,
		"xptr":       cgoType,
	}

	got := cgoDeclarations(pkg)

	if len(got) != len(want) {
		t.Errorf("recognized %d cgo declarations, want %d: %v", len(got), len(want), cgoDeclarationNames(got))
	}

	seen := map[string]bool{}

	for _, declaration := range got {
		seen[declaration.cName] = true
		kind, known := want[declaration.cName]

		if !known {
			t.Errorf("C.%s was recognized, but it is plumbing or not a cgo declaration", declaration.cName)
			continue
		}

		if declaration.kind != kind {
			t.Errorf("C.%s classified as %d, want %d", declaration.cName, declaration.kind, kind)
		}

		if declaration.object == nil {
			t.Errorf("C.%s carries no typed object", declaration.cName)
		}
	}

	// A declaration cmd/cgo writes under a prefix this does not know is dropped as plumbing, silently; name each one.
	for name := range want {
		if !seen[name] {
			t.Errorf("C.%s is declared but was not recognized", name)
		}
	}
}

func cgoDeclarationNames(declarations []cgoDeclaration) []string {
	names := make([]string, 0, len(declarations))

	for _, declaration := range declarations {
		names = append(names, declaration.cName)
	}

	return names
}

// cgoFixtureDir holds fixture.go and the three files cmd/cgo generated from it.
var cgoFixtureDir = filepath.Join("testdata", "cgo", "phase1")

// cgoFixtureEntry is one declaration the fixture's generated files carry: a C name and its kind. One name can carry two
// kinds: cmd/cgo declares C.halve as a plain function (_Cfunc_halve) beside the two-result form fixture.go calls
// (_C2func_halve).
type cgoFixtureEntry struct {
	cName string
	kind  cgoDeclKind
}

// cgoFixtureWant is every declaration the fixture's generated files carry: the seventeen C names fixture.go writes
// (TestTheCgoFixtureWritesSeventeenCNames derives them from its source), the seven C types cmd/cgo declares on the way
// to them (scale returns a double that Go reads through float64, so C.double is never written), and halve's plain
// function. cgo's own _GoString_ is not here: its C name is reserved.
var cgoFixtureWant = map[cgoFixtureEntry]bool{
	{"BLUE", cgoIntConst}:      true,
	{"CONST", cgoIntConst}:     true,
	{"CString", cgoFunc}:       true,
	{"FCONST", cgoFloatConst}:  true,
	{"GREEN", cgoIntConst}:     true,
	{"GoString", cgoFunc}:      true,
	{"NEXT", cgoMacro}:         true,
	{"SCONST", cgoStringConst}: true,
	{"add", cgoFunc}:           true,
	{"char", cgoType}:          true,
	{"double", cgoType}:        true,
	{"enum_color", cgoEnum}:    true,
	{"free", cgoFunc}:          true,
	{"halve", cgoErrnoFunc}:    true,
	{"halve", cgoFunc}:         true,
	{"int", cgoType}:           true,
	{"intgo", cgoType}:         true,
	{"long", cgoType}:          true,
	{"ptrdiff_t", cgoType}:     true,
	{"scale", cgoFunc}:         true,
	{"struct_x", cgoStruct}:    true,
	{"twice", cgoFuncValue}:    true,
	{"uchar", cgoType}:         true,
	{"void", cgoType}:          true,
	{"xptr", cgoType}:          true,
}

// loadCgoFixture loads the fixture directory at CGO_ENABLED=0, where fixture.go drops out (it imports "C") and the
// three generated files are the package. withIncomplete adds an Overlay giving runtime/cgo the one declaration it
// loses at CGO_ENABLED=0: Incomplete is declared in cgo.go, which imports "C" itself, and every _cgo_gotypes.go names
// it in `type _ _cgopackage.Incomplete`. The fixture's own files are never touched.
func loadCgoFixture(t *testing.T, withIncomplete bool) *packages.Package {
	t.Helper()

	dir, err := filepath.Abs(cgoFixtureDir)

	if err != nil {
		t.Fatal(err)
	}

	config := &packages.Config{
		Mode: packages.LoadAllSyntax,
		Dir:  dir,
		Env:  append(os.Environ(), "CGO_ENABLED=0"),
	}

	if withIncomplete {
		goRoot, err := getGoEnvFrom(dir, "GOROOT")

		if err != nil {
			t.Fatal(err)
		}

		config.Overlay = map[string][]byte{
			filepath.Join(goRoot, "src", "runtime", "cgo", "go2cs_incomplete.go"): []byte(cgoIncompleteOverlay),
		}

		// Go refuses an overlay beneath GOMODCACHE, and a toolchain the go command fetched lives there. The load then
		// takes a module cache of its own (the fixture's module has no dependencies), pinned to the toolchain whose
		// GOROOT this resolved, so nothing can switch it.
		if modCache, err := getGoEnvFrom(dir, "GOMODCACHE"); err == nil && modCache != "" && isPathUnder(goRoot, modCache) {
			config.Env = append(config.Env, "GOMODCACHE="+t.TempDir(), "GOTOOLCHAIN=local",
				"PATH="+filepath.Join(goRoot, "bin")+string(os.PathListSeparator)+os.Getenv("PATH"))
		}
	}

	pkgs, err := packages.Load(config, ".")

	if err != nil {
		t.Fatal(err)
	}

	if len(pkgs) != 1 || pkgs[0].Name != "cgofixture" {
		t.Fatalf("loaded %d packages from the fixture, want one cgofixture", len(pkgs))
	}

	return pkgs[0]
}

// The premise of every fixture test, and the reason for the Overlay: at CGO_ENABLED=0 the generated files are the
// package, and runtime/cgo's Incomplete is the ONE thing they cannot resolve. A toolchain that moves Incomplete, or
// a fixture regenerated with a new dependency, reds here by name.
func TestTheCgoFixtureLoadsAtCgoDisabledMissingOnlyRuntimeCgosIncomplete(t *testing.T) {
	bare := loadCgoFixture(t, false)

	var names []string

	for _, file := range bare.CompiledGoFiles {
		names = append(names, filepath.Base(file))
	}

	slices.Sort(names)

	if want := []string{"cgo_gotypes.go", "cgo_import.go", "fixture.cgo1.go"}; !slices.Equal(names, want) {
		t.Errorf("the fixture's package compiles %v, want %v", names, want)
	}

	if len(bare.Errors) != 1 || !strings.Contains(bare.Errors[0].Msg, "undefined: _cgopackage.Incomplete") {
		t.Errorf("without the Overlay the fixture reports %v, want exactly runtime/cgo's Incomplete undefined", bare.Errors)
	}

	if overlaid := loadCgoFixture(t, true); len(overlaid.Errors) != 0 {
		t.Errorf("with the Overlay the fixture reports %v, want none", overlaid.Errors)
	}
}

// A toolchain the go command fetched lives INSIDE the module cache (GOROOT is $GOMODCACHE/golang.org/toolchain@…, as
// cloud lanes get it), and Go refuses an overlay beneath GOMODCACHE, so the Overlay above must still reach runtime/cgo
// there. The layout is planted by naming GOROOT's parent as the module cache, which needs no second toolchain.
func TestTheCgoFixtureLoadsWhenGorootIsInsideTheModuleCache(t *testing.T) {
	dir, err := filepath.Abs(cgoFixtureDir)

	if err != nil {
		t.Fatal(err)
	}

	goRoot, err := getGoEnvFrom(dir, "GOROOT")

	if err != nil {
		t.Fatal(err)
	}

	t.Setenv("GOMODCACHE", filepath.Dir(goRoot))

	if overlaid := loadCgoFixture(t, true); len(overlaid.Errors) != 0 {
		t.Errorf("with GOROOT inside the module cache, the Overlay fixture reports %v, want none", overlaid.Errors)
	}
}

// fixture.go writes seventeen C names, and the want table carries every one of them.
func TestTheCgoFixtureWritesSeventeenCNames(t *testing.T) {
	file, err := parser.ParseFile(token.NewFileSet(), filepath.Join(cgoFixtureDir, "fixture.go"), nil, 0)

	if err != nil {
		t.Fatal(err)
	}

	written := map[string]bool{}

	ast.Inspect(file, func(node ast.Node) bool {
		if selector, ok := node.(*ast.SelectorExpr); ok {
			if ident, ok := selector.X.(*ast.Ident); ok && ident.Name == "C" {
				written[selector.Sel.Name] = true
			}
		}

		return true
	})

	if len(written) != 17 {
		t.Errorf("fixture.go writes %d C names, want 17: %v", len(written), written)
	}

	carried := map[string]bool{}

	for entry := range cgoFixtureWant {
		carried[entry.cName] = true
	}

	for name := range written {
		if !carried[name] {
			t.Errorf("fixture.go writes C.%s, which the want table does not carry", name)
		}
	}
}

func TestCgoDeclarationsResolvesEveryCNameInTheFixture(t *testing.T) {
	pkg := loadCgoFixture(t, true)
	got := cgoDeclarations(pkg.Types)

	if len(got) != len(cgoFixtureWant) {
		t.Errorf("recognized %d cgo declarations in the fixture, want %d: %v", len(got), len(cgoFixtureWant), cgoDeclarationNames(got))
	}

	seen := map[cgoFixtureEntry]bool{}

	for _, declaration := range got {
		entry := cgoFixtureEntry{declaration.cName, declaration.kind}
		seen[entry] = true

		if !cgoFixtureWant[entry] {
			t.Errorf("C.%s was recognized as kind %d, which the fixture does not declare", declaration.cName, declaration.kind)
			continue
		}

		if declaration.object == nil || declaration.object.Pkg() != pkg.Types {
			t.Errorf("C.%s carries no typed object of the fixture's package", declaration.cName)
		}
	}

	for entry := range cgoFixtureWant {
		if !seen[entry] {
			t.Errorf("C.%s (kind %d) is declared by the fixture but was not recognized", entry.cName, entry.kind)
		}
	}
}

// fixture.cgo1.go maps to fixture.go through its //line directive; cmd/cgo's own two files map to no source.
func TestCgoSourceFilesMapsTheFixturesTreesToTheirSource(t *testing.T) {
	pkg := loadCgoFixture(t, true)
	mapped := cgoSourceFiles(pkg.Fset, pkg.Syntax)

	dir, err := filepath.Abs(cgoFixtureDir)

	if err != nil {
		t.Fatal(err)
	}

	want := map[string]string{
		"cgo_gotypes.go":  "",
		"cgo_import.go":   "",
		"fixture.cgo1.go": filepath.Join(dir, "fixture.go"),
	}

	if len(mapped) != len(want) {
		t.Fatalf("mapped %d trees, want %d", len(mapped), len(want))
	}

	for _, entry := range mapped {
		source, known := want[filepath.Base(entry.generated)]

		if !known {
			t.Errorf("mapped an unexpected tree %s", filepath.Base(entry.generated))
			continue
		}

		if entry.source != source {
			t.Errorf("%s maps to source %q, want %q", filepath.Base(entry.generated), entry.source, source)
		}

		if entry.file == nil {
			t.Errorf("%s carries no syntax tree", filepath.Base(entry.generated))
		}
	}
}

// cgoFixtureSummaryCounts is what cgoRecognitionSummary reports for the fixture, by kind (cgoFixtureWant counted).
const cgoFixtureSummaryCounts = "recognized 25 C declarations (6 func, 9 type, 1 struct, 1 enum, 3 int const, 1 float const, 1 string const, 1 errno func, 1 func value, 1 macro)"

// cgoFixtureSummaryRoles is the rest of the fixture's summary: its trees by role, which no file name can change.
const cgoFixtureSummaryRoles = "1 generated file -> fixture.go; 2 generated by cmd/cgo with no source"

// The summary a -cgo refusal carries, read over the checked-in fixture: the counts, then each tree by ROLE. The
// fixture's generated files carry readable names; a real host's go list hands the same files back under build-cache
// names (the first hosted converter-cgo run read nothing else), and a mixed package adds a plain file cmd/cgo leaves
// alone. The summary must read the same over all three, naming only the sources.
func TestCgoRecognitionSummaryCountsTheFixtureByKindAndNamesEachTreeByRole(t *testing.T) {
	pkg := loadCgoFixture(t, true)

	dir, err := filepath.Abs(cgoFixtureDir)

	if err != nil {
		t.Fatal(err)
	}

	goFiles := []string{filepath.Join(dir, "fixture.go")}

	// The same three generated files, re-parsed under cache-shaped names. fixture.cgo1.go's //line directive is
	// relative, so it resolves against the cache directory and still names fixture.go.
	cached := token.NewFileSet()
	cache := t.TempDir()
	var cachedTrees []*ast.File

	for index, name := range []string{"cgo_gotypes.go", "cgo_import.go", "fixture.cgo1.go"} {
		source, err := os.ReadFile(filepath.Join(dir, name))

		if err != nil {
			t.Fatal(err)
		}

		tree, err := parser.ParseFile(cached, filepath.Join(cache, strings.Repeat(string(rune('a'+index)), 64)+"-d"), source, parser.ParseComments)

		if err != nil {
			t.Fatal(err)
		}

		cachedTrees = append(cachedTrees, tree)
	}

	// A mixed package's plain file: go list hands it over at its own path, with no //line directive.
	plainPath := filepath.Join(dir, "plain.go")
	plainTree, err := parser.ParseFile(pkg.Fset, plainPath, "package cgofixture\n", 0)

	if err != nil {
		t.Fatal(err)
	}

	shapes := []struct {
		name    string
		fset    *token.FileSet
		files   []*ast.File
		goFiles []string
		want    string
	}{
		{"readable names", pkg.Fset, pkg.Syntax, goFiles, cgoFixtureSummaryCounts + "; " + cgoFixtureSummaryRoles},
		{"build-cache names", cached, cachedTrees, goFiles, cgoFixtureSummaryCounts + "; " + cgoFixtureSummaryRoles},
		{"a plain file beside", pkg.Fset, append(slices.Clone(pkg.Syntax), plainTree), append(slices.Clone(goFiles), plainPath),
			cgoFixtureSummaryCounts + "; 1 generated file -> fixture.go; plain.go: plain Go source; 2 generated by cmd/cgo with no source"},
	}

	for _, shape := range shapes {
		t.Run(shape.name, func(t *testing.T) {
			if got := cgoRecognitionSummary(pkg.Types, shape.fset, shape.files, shape.goFiles); got != shape.want {
				t.Errorf("summary\n got: %s\nwant: %s", got, shape.want)
			}
		})
	}
}

// With no C compiler go list never runs cgo, and the raw `import "C"` file is parsed as itself: no tree points back
// to a source, so the summary says cgo did not run rather than reporting zero declarations as a reading.
func TestCgoRecognitionSummarySaysWhenCmdCgoDidNotRun(t *testing.T) {
	fset := token.NewFileSet()
	rawPath := filepath.Join(t.TempDir(), "add.go")
	raw, err := parser.ParseFile(fset, rawPath, cgoImportSource, parser.ParseComments)

	if err != nil {
		t.Fatal(err)
	}

	want := "cmd/cgo did not run for this load (no C compiler), so nothing was recognized"

	if got := cgoRecognitionSummary(types.NewPackage("p", "p"), fset, []*ast.File{raw}, []string{rawPath}); got != want {
		t.Errorf("summary\n got: %s\nwant: %s", got, want)
	}
}

// The fixture's three generated files are cmd/cgo's own output for fixture.go, made as testdata/cgo/phase1/README.md
// says. On linux/amd64 with a C compiler this makes them again the same way and compares fixture.cgo1.go and
// cgo_gotypes.go byte for byte, from line 9 of each committed file where the header ends. cgo_import.go names the host
// C library's symbol versions, so it is read and logged but not compared. Each regenerated file is logged whole, as
// base64 on one line, so a fixture.go that changes is re-committed from this test's own output.
func TestTheCgoFixtureIsCmdCgosOutputForFixtureGo(t *testing.T) {
	requireCmdCgoOutput(t, cgoFixtureDir, "example.com/cgofixture", "fixture.cgo1.go", "cgo_gotypes.go", "cgo_import.go")
}

// cgoPhase2FixtureDir holds cgo Phase 2's extern-only fixture and the two files cmd/cgo generated from it
// (testdata/cgo/phase2/README.md): the trees the converter's cgo route is tested on where no C compiler is.
var cgoPhase2FixtureDir = filepath.Join("testdata", "cgo", "phase2")

// Phase 2's fixture is made and held the same way as Phase 1's (increment 5a-0), without cgo_import.go: go list
// hands back none for it (README.md).
func TestTheCgoPhase2FixtureIsCmdCgosOutputForFixtureGo(t *testing.T) {
	requireCmdCgoOutput(t, cgoPhase2FixtureDir, "example.com/cgophase2", "fixture.cgo1.go", "cgo_gotypes.go")
}

// requireCmdCgoOutput makes fixtureDir's generated files again from its fixture.go, in a module named modulePath, and
// compares each of names with the committed one. A generated file not yet committed fails by name, and its REGEN line carries
// the license header the renamed files take, so it can be committed from the decode as it stands.
func requireCmdCgoOutput(t *testing.T, fixtureDir, modulePath string, names ...string) {
	t.Helper()
	requireCCompiler(t)

	if runtime.GOOS != "linux" || runtime.GOARCH != "amd64" {
		t.Skipf("the fixture's generated files are linux/amd64's (README.md); this host is %s/%s (Inconclusive, not a pass)", runtime.GOOS, runtime.GOARCH)
	}

	source, err := os.ReadFile(filepath.Join(fixtureDir, "fixture.go"))

	if err != nil {
		t.Fatal(err)
	}

	dir := t.TempDir()
	writeCgoFixture(t, dir, "go.mod", "module "+modulePath+"\ngo 1.26\n")
	writeCgoFixture(t, dir, "fixture.go", string(source))

	list := exec.Command("go", "list", "-compiled", "-json", "-trimpath", ".")
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

	// go list may hand the files back under build-cache names, so each is known by what it holds.
	generated := map[string]string{}

	for _, path := range listed.CompiledGoFiles {
		if !filepath.IsAbs(path) {
			path = filepath.Join(listed.Dir, path)
		}

		content, err := os.ReadFile(path)

		if err != nil {
			t.Fatal(err)
		}

		text := strings.ReplaceAll(string(content), "\r\n", "\n")

		switch {
		case strings.Contains(text, "//go:cgo_import_dynamic"):
			generated["cgo_import.go"] = text
		case strings.Contains(text, "_Cgo_always_false"):
			generated["cgo_gotypes.go"] = text
		default:
			// go list -trimpath leaves the fixture directory's absolute path in line 3's //line directive.
			lines := strings.SplitN(text, "\n", 4)

			if len(lines) == 4 && strings.HasPrefix(lines[2], "//line ") && strings.HasSuffix(lines[2], "fixture.go:1:1") {
				lines[2] = "//line fixture.go:1:1"
			}

			generated["fixture.cgo1.go"] = strings.Join(lines, "\n")
		}
	}

	for _, name := range []string{"fixture.cgo1.go", "cgo_gotypes.go", "cgo_import.go"} {
		if _, found := generated[name]; !found && !slices.Contains(names, name) {
			t.Logf("READING %s: not handed back, as expected", name)
		}
	}

	for _, name := range names {
		text, found := generated[name]

		if !found {
			t.Errorf("go list -compiled handed back no %s: %v", name, listed.CompiledGoFiles)
			continue
		}

		committed, err := os.ReadFile(filepath.Join(fixtureDir, name))

		if errors.Is(err, fs.ErrNotExist) {
			header := ""

			if name != "fixture.cgo1.go" {
				// fixture.go's seven-line license header, naming this file, and a blank line.
				lines := strings.SplitAfterN(strings.ReplaceAll(string(source), "\r\n", "\n"), "\n", 8)
				header = strings.Replace(strings.Join(lines[:7], ""), "fixture.go", name, 1) + "\n"
			}

			t.Logf("REGEN %s %s", name, base64.StdEncoding.EncodeToString([]byte(header+text)))
			t.Errorf("%s is not committed yet; commit it from this test's REGEN line", name)
			continue
		}

		if err != nil {
			t.Fatal(err)
		}

		committedText := strings.ReplaceAll(string(committed), "\r\n", "\n")
		header := ""

		if name != "fixture.cgo1.go" {
			// The two renamed files carry the repository's seven-line license header and a blank line first.
			lines := strings.SplitAfterN(committedText, "\n", 9)
			header = strings.Join(lines[:8], "")
			committedText = lines[8]
		}

		equal := text == committedText
		t.Logf("READING %s: regenerated %d bytes, equal to the committed file: %t", name, len(text), equal)
		t.Logf("REGEN %s %s", name, base64.StdEncoding.EncodeToString([]byte(header+text)))

		if !equal && name != "cgo_import.go" {
			t.Errorf("%s is not what cmd/cgo writes for fixture.go now; re-commit it from this test's REGEN line", name)
		}
	}
}
