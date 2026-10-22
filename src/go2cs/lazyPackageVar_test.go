// lazyPackageVar_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards lazy package vars (docs/DESIGN-lazy-static-data.md, ruled 2026-10-22) at the emission: which vars the predicate
// makes lazy (eligible, the 20-node floor, closure, the exported var a lazy initializer reads), the holder shape (a ref
// property over a holder whose field takes the var's name; an exported var a lazy initializer reads keeps its initial
// value apart, and that initializer reads it), the vars that stay eager (below the floor, ineligible), and every refusal
// a LISTED var is refused with by name. The first fixture is shaped like the malloc tables of COORD's ruling (A): function
// values the ordered ctor would otherwise relocate, since the functions read a package var declared in another file.
package main

import (
	"go/build"
	"os"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

const lazyVarFixturePkg = "example.com/lazyvar"

// withLazyPackageVars lists names of the fixture package for the test's duration. The production map is never
// mutated: a copy carries the entry and the original is restored by reference.
func withLazyPackageVars(t *testing.T, names ...string) {
	t.Helper()

	saved := lazyPackageVars
	listed := make(map[string]map[string]bool, len(saved)+1)

	for pkg, vars := range saved {
		listed[pkg] = vars
	}

	listed[lazyVarFixturePkg] = map[string]bool{}

	for _, name := range names {
		listed[lazyVarFixturePkg][name] = true
	}

	lazyPackageVars = listed
	t.Cleanup(func() { lazyPackageVars = saved })
}

// convertLazyVarModule converts a fixture module whose files are given by name, returning the emitted files that exist
// (a file the converter refused is absent).
func convertLazyVarModule(t *testing.T, sources map[string]string) (map[string]string, error) {
	t.Helper()
	return convertLazyVarModuleOver(t, sources, nil)
}

// convertLazyVarModuleOver converts over destination files seeded by name (a hand-owned `.cs`), also returning a
// hand-owned file's `.cs.auto` review sibling under its own name.
func convertLazyVarModuleOver(t *testing.T, sources, seeds map[string]string) (map[string]string, error) {
	t.Helper()

	root := t.TempDir()
	appDir := filepath.Join(root, "app")
	dir := filepath.Join(root, "out", "src", "example.com", "lazyvar")

	for name, seed := range seeds {
		writeModuleFile(t, filepath.Join(dir, name), seed)
	}

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module "+lazyVarFixturePkg+"\n\ngo 1.23\n")

	for name, source := range sources {
		writeModuleFile(t, filepath.Join(appDir, name), source)
	}

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{
		goRoot:              goRoot,
		goPath:              build.Default.GOPATH,
		go2csPath:           filepath.Join(root, "out"),
		recurse:             true,
		targetPlatform:      runtime.GOOS + "/" + runtime.GOARCH,
		indentSpaces:        4,
		preferVarDecl:       true,
		useChannelOperators: true,
	}

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	if err := NewModuleConverter(options).ConvertModule(appDir); err != nil {
		return nil, err
	}

	files := map[string]string{}

	for name := range sources {
		if strings.HasSuffix(name, "_test.go") {
			continue
		}

		emitted := strings.TrimSuffix(name, ".go") + ".cs"

		if data, err := os.ReadFile(filepath.Join(dir, emitted)); err == nil {
			files[emitted] = string(data)
		}

		if data, err := os.ReadFile(filepath.Join(dir, emitted+".auto")); err == nil {
			files[emitted+".auto"] = string(data)
		}
	}

	if data, err := os.ReadFile(filepath.Join(dir, PackageInitFileName)); err == nil {
		files[PackageInitFileName] = string(data)
	}

	return files, nil
}

// convertLazyVarFixture is the two-file form: main.go and table.go.
func convertLazyVarFixture(t *testing.T, mainGo, tableGo string) (map[string]string, error) {
	t.Helper()
	return convertLazyVarModule(t, map[string]string{"main.go": mainGo, "table.go": tableGo})
}

const lazyVarFixtureMain = `package main

var calls int

func small(n int) int { calls++; return n }
func large(n int) int { calls += 2; return n * 2 }

func main() {
	println(table[1](3), table[2](3), twin[1](3), calls)
}
`

// Both tables read 'calls' through their functions and are declared in another file, which relocates an eager one.
const lazyVarFixtureTable = `package main

var table = [3]func(int) int{
	small,
	small,
	large,
}

var twin = [3]func(int) int{small, large, large}
`

// TestLazyPackageVarIsBuiltOnFirstRead pins the LISTED form (COORD's ruling (A)): the listed table is a ref property over
// its holder, the field named for the var, and is absent from package_init.cs although it is below the floor; the
// unlisted twin of the same shape stays below the floor, relocated, with its initᴛ call.
func TestLazyPackageVarIsBuiltOnFirstRead(t *testing.T) {
	isolateConverterPathGlobals(t)

	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	withLazyPackageVars(t, "table")

	files, err := convertLazyVarFixture(t, lazyVarFixtureMain, lazyVarFixtureTable)

	if err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	table, packageInit := files["table.cs"], files[PackageInitFileName]

	for _, want := range []string{
		// one line: today's declaration in its marked holder (go2cs-gen writes the accessor)
		"[GoLazy] partial class lazyᴛtable { internal static array<Func<nint, nint>> table = ",
	} {
		if !strings.Contains(table, want) {
			t.Errorf("table.cs lacks %q:\n%s", want, table)
		}
	}

	if strings.Contains(table, "initᴛtable") || strings.Contains(packageInit, "initᴛtable") {
		t.Errorf("the listed table must not be relocated into the ordered ctor:\ntable.cs:\n%s\npackage_init.cs:\n%s", table, packageInit)
	}

	// Control: the unlisted twin of the same shape is below the floor and still relocates.
	if !strings.Contains(table, "initᴛtwin()") || !strings.Contains(packageInit, "initᴛtwin();") {
		t.Errorf("control: the unlisted twin must still relocate:\ntable.cs:\n%s\npackage_init.cs:\n%s", table, packageInit)
	}
}

const lazyVarPredicateMain = `package main

type entry struct {
	name   string
	weight int
}

func weigh() int { return 7 }

func main() {
	println(len(big), len(small), len(view), len(Weights), len(byName), len(computed))
}
`

// The predicate's own choices, with no list: big is eligible and over the floor; small is eligible and under it; view
// (a slice of big) and byName (a map over Weights) are under it but read a lazy var (closure); Weights is exported and a
// lazy initializer reads it, so it keeps its initial value apart; computed makes a call and stays eager.
const lazyVarPredicateTables = `package main

var big = [...]entry{{"a", 1}, {"b", 2}, {"c", 3}, {"d", 4}, {"e", 5}, {"f", 6}, {"g", 7}}

var small = [2]int{1, 2}

var view = big[:]

var Weights = big[2:5]

var byName = map[string][]entry{"weights": Weights}

var computed = [1]int{weigh()}
`

// TestLazyPackageVarPredicateChoosesAndShapes pins the predicate's selection and the holder shapes at the emission.
func TestLazyPackageVarPredicateChoosesAndShapes(t *testing.T) {
	isolateConverterPathGlobals(t)

	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	files, err := convertLazyVarModule(t, map[string]string{"main.go": lazyVarPredicateMain, "tables.go": lazyVarPredicateTables})

	if err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	tables := files["tables.cs"]

	for _, want := range []string{
		// eligible and over the floor
		"[GoLazy] partial class lazyᴛbig { internal static array<entry> big = ",
		// closure: under the floor, but reads a lazy var
		"[GoLazy] partial class lazyᴛview { internal static slice<entry> view = ",
		// exported and read by a lazy initializer: marked for its initial value, which go2cs-gen keeps apart
		"[GoLazy(Initial = true)] partial class lazyᴛWeights { public static slice<entry> Weights = ",
		// the lazy map reads that initial value, never the property
		"[GoLazy] partial class lazyᴛbyName { internal static map<@string, slice<entry>> byName = ",
		"lazyᴛWeights.initial",
	} {
		if !strings.Contains(tables, want) {
			t.Errorf("tables.cs lacks %q:\n%s", want, tables)
		}
	}

	// Eager: small is under the floor and reads nothing lazy; computed makes a call.
	for _, eager := range []string{"small", "computed"} {
		if strings.Contains(tables, "lazyᴛ"+eager) {
			t.Errorf("%s must stay eager:\n%s", eager, tables)
		}
	}

	if !strings.Contains(tables, "static array<nint> small = ") {
		t.Errorf("control: small must keep its field initializer:\n%s", tables)
	}
}

// The hand-owned file declares a table over the floor; reader, in a converted file, would be lazy by closure.
const lazyVarHandOwnedSource = `package main

var owned = [...]entry{{"a", 1}, {"b", 2}, {"c", 3}, {"d", 4}, {"e", 5}, {"f", 6}, {"g", 7}}
`

const lazyVarHandOwnedReader = `package main

var reader = owned[:]

var control = [...]entry{{"a", 1}, {"b", 2}, {"c", 3}, {"d", 4}, {"e", 5}, {"f", 6}, {"g", 7}}
`

// TestLazyPackageVarLeavesHandOwnedEager pins that a var whose declaration is hand-owned ([module: GoManualConversion]) is
// never lazy, nor is a reader of it: the hand-owned `.cs` keeps its own declaration, which the converter does not shape,
// so its `.cs.auto` review sibling shows the eager field and the converted reader stays eager. control, the same table
// in a converted file, is lazy.
func TestLazyPackageVarLeavesHandOwnedEager(t *testing.T) {
	isolateConverterPathGlobals(t)

	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	files, err := convertLazyVarModuleOver(t,
		map[string]string{"main.go": lazyVarPredicateMainOwned, "owned.go": lazyVarHandOwnedSource, "reader.go": lazyVarHandOwnedReader},
		map[string]string{"owned.cs": "[module: GoManualConversion]\r\nnamespace go;\r\n"})

	if err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	owned, ok := files["owned.cs.auto"]

	if !ok {
		t.Fatalf("the hand-owned file's review sibling owned.cs.auto was not emitted; read %d files", len(files))
	}

	if strings.Contains(owned, "lazyᴛowned") || !strings.Contains(owned, "static array<entry> owned = ") {
		t.Errorf("owned.cs.auto must keep the eager field:\n%s", owned)
	}

	reader := files["reader.cs"]

	if strings.Contains(reader, "lazyᴛreader") {
		t.Errorf("reader reads a hand-owned var and must stay eager:\n%s", reader)
	}

	if !strings.Contains(reader, "[GoLazy] partial class lazyᴛcontrol { internal static array<entry> control = ") {
		t.Errorf("control: the same table in a converted file must be lazy:\n%s", reader)
	}
}

// TestLazyPackageVarLeavesManualVarEager is the same pin for a var whose storage a hand-owned member supplies
// (manualConversionVars): its declaration is displaced, so nothing reads a holder for it and its reader stays eager.
func TestLazyPackageVarLeavesManualVarEager(t *testing.T) {
	isolateConverterPathGlobals(t)

	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	saved := manualConversionVars
	registered := make(map[string]map[string]goosScope, len(saved)+1)

	for pkg, vars := range saved {
		registered[pkg] = vars
	}

	registered[lazyVarFixturePkg] = map[string]goosScope{"owned": goosAny}
	manualConversionVars = registered
	t.Cleanup(func() { manualConversionVars = saved })

	files, err := convertLazyVarModule(t,
		map[string]string{"main.go": lazyVarPredicateMainOwned, "owned.go": lazyVarHandOwnedSource, "reader.go": lazyVarHandOwnedReader})

	if err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	if owned := files["owned.cs"]; strings.Contains(owned, "lazyᴛowned") {
		t.Errorf("the displaced var must not be given a holder:\n%s", owned)
	}

	reader := files["reader.cs"]

	if strings.Contains(reader, "lazyᴛreader") {
		t.Errorf("reader reads a displaced var and must stay eager:\n%s", reader)
	}

	if !strings.Contains(reader, "[GoLazy] partial class lazyᴛcontrol { internal static array<entry> control = ") {
		t.Errorf("control: the same table in a converted file must be lazy:\n%s", reader)
	}
}

const lazyVarPredicateMainOwned = `package main

type entry struct {
	name   string
	weight int
}

func main() {
	println(len(owned), len(reader), len(control))
}
`

// TestLazyPackageVarRefusesByName pins every refusal at the emission: each case LISTS `table`, whose initializer breaks
// one rule; the converter reports the entry by name with the reason and emits nothing for the file.
func TestLazyPackageVarRefusesByName(t *testing.T) {
	isolateConverterPathGlobals(t)

	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	const main = "package main\n\nfunc f() int { return 1 }\n\nfunc main() { println(len(table)) }\n"

	cases := []struct {
		name    string
		sources map[string]string
		refusal string
	}{
		{"call", map[string]string{"table.go": "package main\n\nvar table = [2]int{f(), 2}\n"},
			"its initializer makes a call"},
		{"imported var", map[string]string{"table.go": "package main\n\nimport \"os\"\n\nvar table = [1][]string{os.Args}\n"},
			"its initializer reads the imported var os.Args"},
		{"index", map[string]string{"table.go": "package main\n\nvar src = [2]int{1, 2}\n\nvar table = [1]int{src[0]}\n"},
			"its initializer indexes or dereferences storage"},
		{"method value", map[string]string{"table.go": "package main\n\ntype T int\n\nfunc (T) M() {}\n\nvar t T\n\nvar table = [1]func(){t.M}\n"},
			"its initializer takes a method value"},
		{"receive", map[string]string{"table.go": "package main\n\nvar ch = make(chan int, 1)\n\nvar table = [1]int{<-ch}\n"},
			"its initializer receives from a channel"},
		{"type assertion", map[string]string{"table.go": "package main\n\nvar x any = f()\n\nvar table = [1]int{x.(int)}\n"},
			"its initializer makes a type assertion"},
		{"mutated dep", map[string]string{"table.go": "package main\n\nvar base = 1\n\nfunc init() { base = 2 }\n\nvar table = [1]int{base}\n"},
			"it reads the var base, which the package or its tests assign or take the address of"},
		{"dep mutated by a test file", map[string]string{"table.go": "package main\n\nvar base = 1\n\nvar table = [1]int{base}\n",
			"table_test.go": "package main\n\nimport \"testing\"\n\nfunc TestBase(t *testing.T) { base = 3 }\n"},
			"it reads the var base, which the package or its tests assign or take the address of"},
		{"address taken", map[string]string{"table.go": "package main\n\nvar table = [2]int{1, 2}\n\nvar p = &table\n"},
			"its address is taken"},
		{"exported constant dep", map[string]string{"table.go": "package main\n\nvar Max = 10\n\nvar table = [1]int{Max}\n"},
			"it reads the exported var Max, which an importer can assign"},
	}

	for _, c := range cases {
		t.Run(c.name, func(t *testing.T) {
			withLazyPackageVars(t, "table")

			sources := map[string]string{"main.go": main}

			for name, source := range c.sources {
				sources[name] = source
			}

			var files map[string]string
			var err error

			stderr := captureStderr(t, func() { files, err = convertLazyVarModule(t, sources) })

			if err != nil {
				t.Fatalf("ConvertModule: %v", err)
			}

			if want := "lazyPackageVars entry table cannot be built on its first read: " + c.refusal; !strings.Contains(stderr, want) {
				t.Fatalf("want the refusal naming table (%q) on stderr, got:\n%s", want, stderr)
			}

			if _, emitted := files["table.cs"]; emitted {
				t.Errorf("a refused file must not be emitted:\n%s", files["table.cs"])
			}
		})
	}
}
