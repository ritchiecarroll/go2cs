// testOnlyPackageEmission_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).
//
// Guards what a TEST-ONLY package's `-tests` emission may NAME.
//
// MEASURED at Go 1.24.13 on crypto/internal/fips140test — thirteen files, every one a `_test.go`,
// directory `fips140test`, package clause `fipstest`. Its production half converts nothing:
// conversionDriver reaches `unmarkedFileCount == 0` with an empty file list and reports
// "Skipping conversion: no target Go source files found", so no `fipstest_package` class and no
// production `.csproj` are emitted. The TEST half named that class anyway — a file-scoped
// `using static` in each of the twelve emitted test files, plus package_test_info.cs's own
// `global using static` and the `initPackage(typeof(...))` forcing hook — and the tests project
// failed to build with thirteen of:
//
//	package_test_info.cs(7,49): error CS0234: The type or namespace name 'fipstest_package' does
//	not exist in the namespace 'go.crypto.@internal' (are you missing an assembly reference?)
//
// The two arms are ONE measurement with its own control. They vary a single axis — whether the
// package has a production file — and select the same project model, which the runner asserts
// rather than assumes. The CONTROL is what keeps the refusal arm from passing vacuously: an
// emission that produced nothing, or that spelled the production class differently than this test
// looks for, would satisfy "no line names it" for the wrong reason and fails in the control
// instead.

package main

import (
	"os"
	"path/filepath"
	"runtime"
	"sort"
	"strings"
	"testing"

	"golang.org/x/tools/go/packages"
)

// The fixture's package DIRECTORY and its package CLAUSE differ deliberately: fips140test/fipstest
// does, the emitted class is named from the package clause, and a fixture whose two spellings
// agreed could not tell which one the emission used.
const (
	testOnlyFixtureDir     = "shapetest"
	testOnlyFixturePackage = "shapes"
)

// writeTestOnlyFixture writes the fixture module and returns its package directory.
//
// withProduction is the ONE axis. Without it every file in the package is a `_test.go` — the
// fips140test shape. With it the same `area` moves into a production file the test calls, so a
// production class exists and both directives are load-bearing rather than decorative.
func writeTestOnlyFixture(t *testing.T, withProduction bool) string {
	t.Helper()

	dir := t.TempDir()

	testSource := "package " + testOnlyFixturePackage + "\n\n" +
		"import \"testing\"\n\n" +
		"func TestArea(t *testing.T) {\n" +
		"\tif area(2, 3) != 6 {\n" +
		"\t\tt.Fatal(\"area\")\n" +
		"\t}\n" +
		"}\n"

	files := map[string]string{"go.mod": "module example/toponly\n\ngo 1.23\n"}

	if withProduction {
		files[testOnlyFixtureDir+"/shape.go"] = "package " + testOnlyFixturePackage + "\n\n" +
			"func area(w, h int) int { return w * h }\n"
	} else {
		testSource += "\nfunc area(w, h int) int { return w * h }\n"
	}

	files[testOnlyFixtureDir+"/shape_test.go"] = testSource

	writeModuleFiles(t, dir, files)

	return filepath.Join(dir, testOnlyFixtureDir)
}

// convertTestOnlyFixture runs the REAL `-tests` wiring over the fixture — the same Tests:true load,
// production/variant discovery, model selection and convertTestVariants call processTestConversion
// makes — and returns every emitted `.cs` keyed by base name. Going through the production path is
// what makes this a guard on the WIRING and not on a hand-set option field.
func convertTestOnlyFixture(t *testing.T, inputPath string) map[string]string {
	t.Helper()

	loaded, err := packages.Load(&packages.Config{Mode: packages.LoadAllSyntax, Dir: inputPath, Tests: true}, ".")
	if err != nil {
		t.Fatal(err)
	}

	production := findProductionPackage(loaded, inputPath)
	if production == nil {
		t.Fatal("production package was not loaded")
	}

	if production.Name != testOnlyFixturePackage {
		t.Fatalf("fixture package clause reads %q, not %q — the emitted class is named from it", production.Name, testOnlyFixturePackage)
	}

	internal, external := findTestVariants(loaded, production)
	if internal == nil {
		t.Fatal("the fixture must load an INTERNAL test variant — its suite is what selects the model both arms share")
	}
	if external != nil {
		t.Fatal("the fixture must load no EXTERNAL test variant, matching the fips140test shape")
	}

	// Asserted rather than assumed: the arms are comparable only while they select the same model,
	// and the production file the control adds is exactly the kind of edit that could move it.
	if model := selectTestProjectModel(internal, external); model != testProjectWhiteboxReference {
		t.Fatalf("fixture model = %v, want whitebox-reference", model)
	}

	outputPath := t.TempDir()

	resetPackageState(&packages.Package{})
	packageNamespace = "go"

	options := Options{
		indentSpaces:        4,
		preferVarDecl:       true,
		useChannelOperators: true,
		convertTests:        true,
		targetPlatform:      runtime.GOOS + "/" + runtime.GOARCH,
	}

	// The self-import binding processTestConversion establishes before handing off.
	options.testPackagePath = production.PkgPath
	options.testPackageName = production.Name

	if _, err = convertTestVariants(testProjectWhiteboxReference, production, internal, external,
		selectCompileExcludedTestFiles(internal, external), inputPath, outputPath, "go",
		NewHashSet(supportedTestCapabilities()), options); err != nil {
		t.Fatalf("convertTestVariants: %v", err)
	}

	emitted, err := filepath.Glob(filepath.Join(outputPath, "*.cs"))
	if err != nil {
		t.Fatal(err)
	}

	if len(emitted) == 0 {
		t.Fatalf("no converted file was emitted into %s", outputPath)
	}

	// The PROJECT, written the way processTestConversion writes it once the variants have converted:
	// the same model, into the same output directory, with Options.testProductionAbsent DERIVED by
	// the production code's own predicate rather than hand-set to the answer the guards are looking
	// for. Dependencies are left nil deliberately — resolving them needs a corpus tree, and they
	// contribute only `$(go2csPath)`-rooted references, which the colocated reading ignores by
	// construction (see colocatedProjectReferences).
	projectOptions := options
	projectOptions.go2csPath = t.TempDir()
	projectOptions.testProductionAbsent = !productionClassEmitted(production)

	projectName := projectFileBaseName(testOnlyFixtureDir)

	if err = writeTestProject(filepath.Join(outputPath, projectName+testProjectFileSuffix), projectName, "go", "",
		testProjectWhiteboxReference, nil, nil, nil, nil, projectOptions); err != nil {
		t.Fatalf("writeTestProject: %v", err)
	}

	// The emitted PROJECT joins the map beside the sources: what a test-only package's `.tests.csproj`
	// may REFERENCE is the same question as what its `.cs` files may NAME, and reading both off ONE
	// conversion is what makes the four guards below arms of the same measurement.
	projects, err := filepath.Glob(filepath.Join(outputPath, "*.csproj"))
	if err != nil {
		t.Fatal(err)
	}

	emitted = append(emitted, projects...)

	files := make(map[string]string, len(emitted))

	for _, path := range emitted {
		data, readErr := os.ReadFile(path)
		if readErr != nil {
			t.Fatal(readErr)
		}
		files[filepath.Base(path)] = string(data)
	}

	return files
}

// productionClassLines returns every emitted line naming the production class, partitioned into the
// `using static` directives and the `initPackage(typeof(...))` hooks, plus all of them together.
// Reported as "<file>: <line>" so a failure reads as the CS0234 list it stands for rather than as a
// bare count.
//
// Matching on the class NAME rather than on a fully-spelled directive is deliberate: the namespace
// a fixture lands in and whether the reference is `global::`-rooted are both emission details this
// guard has no business pinning, while "no emitted line may name a class that does not exist" is
// the property itself. `shapes_internal_test_package` — the class the test files DO emit into —
// does not contain `shapes_package`, so the bridge is not a false positive.
func productionClassLines(files map[string]string) (usings, inits, all []string) {
	className := getSanitizedImport(testOnlyFixturePackage + PackageSuffix)

	names := make([]string, 0, len(files))
	for name := range files {
		names = append(names, name)
	}

	sort.Strings(names)

	for _, name := range names {
		// SOURCES only. The map also carries the emitted `.tests.csproj` (for the reference guards
		// below), and a project file names projects rather than classes — scanning it here would
		// answer a different question in this function's report.
		if !strings.HasSuffix(name, ".cs") {
			continue
		}

		for _, line := range strings.Split(files[name], "\n") {
			line = strings.TrimSpace(strings.TrimSuffix(line, "\r"))

			if !strings.Contains(line, className) {
				continue
			}

			record := name + ": " + line
			all = append(all, record)

			switch {
			case strings.Contains(line, "using static"):
				usings = append(usings, record)
			case strings.Contains(line, "initPackage(typeof("):
				inits = append(inits, record)
			}
		}
	}

	return usings, inits, all
}

// TestTestOnlyPackageNamesNoProductionClass is the fips140test refusal: every file a `_test.go`, so
// the production conversion emitted no class, so nothing may import or initialize one.
func TestTestOnlyPackageNamesNoProductionClass(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: loads and converts a test-variant fixture")
	}

	_, _, all := productionClassLines(convertTestOnlyFixture(t, writeTestOnlyFixture(t, false)))

	if len(all) > 0 {
		t.Fatalf("a TEST-ONLY package has no %s class for the test assembly to name — the production half emitted none — yet %d emitted line(s) name it (one CS0234 each):\n\t%s",
			getSanitizedImport(testOnlyFixturePackage+PackageSuffix), len(all), strings.Join(all, "\n\t"))
	}
}

// TestPackageWithProductionFileNamesTheProductionClass is the control for the refusal above: the
// same fixture WITH a production file must still emit both directives, so the refusal cannot be
// satisfied by an emission that names the class nowhere under any circumstances.
func TestPackageWithProductionFileNamesTheProductionClass(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: loads and converts a test-variant fixture")
	}

	className := getSanitizedImport(testOnlyFixturePackage + PackageSuffix)
	usings, inits, _ := productionClassLines(convertTestOnlyFixture(t, writeTestOnlyFixture(t, true)))

	if len(usings) == 0 {
		t.Errorf("a package WITH production files must import its %s class — the test files reference production declarations through it", className)
	}

	if len(inits) == 0 {
		t.Errorf("a package WITH production files must force its %s class's init — the referenced production assembly's module constructor runs only when touched", className)
	}
}

// colocatedProjectReferences returns the emitted `.tests.csproj`'s COLOCATED ProjectReference
// values — the bare `<name>.csproj` spellings that name a sibling in the same directory.
//
// Colocated is the discriminator rather than a name match, and it is exact: every OTHER reference
// the test project carries is tree-rooted (`$(go2csPath)core/...`) — the fixed set (golib, testing,
// context) and every import-derived dependency alike. The production reference is the one the
// -tests contract deliberately spells relative, precisely because the test project is colocated
// with the production csproj. So "a reference with no directory separator and no $(…) property" IS
// the production-reference site, named structurally instead of by a spelling this guard would then
// have to keep in step with projectFileBaseName.
func colocatedProjectReferences(t *testing.T, files map[string]string) (projectFile string, references []string) {
	t.Helper()

	for name, content := range files {
		if !strings.HasSuffix(name, testProjectFileSuffix) {
			continue
		}

		if projectFile != "" {
			t.Fatalf("two test projects were emitted (%s and %s); the fixture emits one", projectFile, name)
		}

		projectFile = name

		for _, line := range strings.Split(content, "\n") {
			line = strings.TrimSpace(strings.TrimSuffix(line, "\r"))

			if !strings.Contains(line, "<ProjectReference") {
				continue
			}

			const marker = `Include="`

			start := strings.Index(line, marker)
			if start < 0 {
				continue
			}

			include := line[start+len(marker):]

			end := strings.Index(include, `"`)
			if end < 0 {
				continue
			}

			include = include[:end]

			if strings.ContainsAny(include, `/\`) || strings.Contains(include, "$(") {
				continue
			}

			references = append(references, include)
		}
	}

	// An absent project file would satisfy "no colocated reference" for the wrong reason, which is
	// the vacuous pass this whole file is written against.
	if projectFile == "" {
		t.Fatalf("no %s was emitted; the reference guards would pass vacuously", testProjectFileSuffix)
	}

	sort.Strings(references)

	return projectFile, references
}

// TestTestOnlyPackageTestProjectReferencesNoProductionProject is the fourth site of the
// fips140test class, on the PROJECT rather than on the sources.
//
// writeTestProject added the colocated `<pkg>.csproj` reference on the MODEL alone
// (model.referencesProduction()), but a test-only package's conversion never writes that production
// project — the same "Skipping conversion: no target Go source files found" that leaves it with no
// production class leaves it with no production csproj. The emitted reference therefore dangles.
// MEASURED at the version tip on crypto/internal/fips140test, which is why this is a guard and not
// a tidiness preference: restore reports
//
//	Skipping project "…\crypto.internal.fips140test.csproj" because it was not found.
//
// twice, and the build then carries
//
//	warning MSB9008: The referenced project crypto.internal.fips140test.csproj does not exist.
//
// — a WARNING, so the dangling edge is invisible to any gate reading only the error count, and it
// would stay in the emission of every future test-only package. The predicate is the one the three
// source sites already consult (productionClassEmitted, plumbed as Options.testProductionAbsent),
// so all four sites now answer the same question with the same reading.
func TestTestOnlyPackageTestProjectReferencesNoProductionProject(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: loads and converts a test-variant fixture")
	}

	projectFile, references := colocatedProjectReferences(t, convertTestOnlyFixture(t, writeTestOnlyFixture(t, false)))

	if len(references) > 0 {
		t.Fatalf("a TEST-ONLY package's production conversion writes no `.csproj` — yet %s references %d colocated project(s) that will never exist (MSB9008 each):\n\t%s",
			projectFile, len(references), strings.Join(references, "\n\t"))
	}
}

// TestPackageWithProductionFileTestProjectReferencesTheProductionProject is the control for the
// refusal above: the same fixture WITH a production file must still carry the colocated reference,
// so the refusal cannot be satisfied by an emission that references the production project under no
// circumstances — which would trade a dangling edge for a missing one (CS0246 on every production
// type the tests name, instead of MSB9008).
func TestPackageWithProductionFileTestProjectReferencesTheProductionProject(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: loads and converts a test-variant fixture")
	}

	projectFile, references := colocatedProjectReferences(t, convertTestOnlyFixture(t, writeTestOnlyFixture(t, true)))

	if len(references) != 1 {
		t.Fatalf("a package WITH production files must reference its colocated production project exactly once; %s carries %d: %v",
			projectFile, len(references), references)
	}

	// The reference must name the PRODUCTION project, never the test project itself: a self-reference
	// is MSB4006 rather than a missing type, and a structural guard that did not check this would
	// accept it.
	if references[0] == projectFile {
		t.Fatalf("%s references ITSELF (%s)", projectFile, references[0])
	}

	if !strings.HasSuffix(references[0], ".csproj") {
		t.Fatalf("%s's colocated reference %q is not a project file", projectFile, references[0])
	}
}

// TestProcessTestConversionDerivesTestProductionAbsentBeforeWritingTheTestProject pins the WIRING
// the two guards above cannot reach, in the same source-assertion style as
// TestTestsConversionConsultsTheDynamicTypeGate (dynamicTypeGate_test.go) and for the same reason:
// the gate can be perfect and still prove nothing if the value never arrives.
//
// convertTestVariants takes Options BY VALUE, so the `testProductionAbsent` it derives onto its own
// copy for the EMISSION sites never reaches processTestConversion's `options` — and writeTestProject
// is handed THAT one. The derivation inside processTestConversion is therefore the only thing
// carrying the answer to the project write in production. Both guards above call writeTestProject
// directly and supply the field themselves, so deleting that one line leaves every Go arm GREEN
// while the emitted `.tests.csproj` goes back to naming a `.csproj` a test-only package never
// writes. A source-text ordering assertion is the cheapest instrument that can fail on it.
//
// Scoped to processTestConversion's own body deliberately: the identical assignment inside
// convertTestVariants is a different function's copy and must NOT satisfy this guard.
func TestProcessTestConversionDerivesTestProductionAbsentBeforeWritingTheTestProject(t *testing.T) {
	const source = "testConversion.go"

	contents, err := os.ReadFile(source)
	if err != nil {
		t.Fatalf("reading %s: %v", source, err)
	}

	text := string(contents)

	start := strings.Index(text, "\nfunc processTestConversion(")
	if start == -1 {
		t.Fatalf("%s no longer declares processTestConversion; this guard has lost its subject", source)
	}

	// The body runs to the next TOP-LEVEL declaration. Searching from one byte in keeps the
	// function's own `\nfunc ` from terminating it immediately.
	body := text[start+1:]
	if end := strings.Index(body[1:], "\nfunc "); end != -1 {
		body = body[:end+1]
	}

	call := strings.Index(body, "writeTestProject(")
	if call == -1 {
		t.Fatalf("processTestConversion no longer calls writeTestProject; the derivation this guard orders against is gone from %s", source)
	}

	derivation := indexOfLineContaining(body, "testProductionAbsent = ", "productionClassEmitted(")

	if derivation == -1 {
		t.Fatalf("processTestConversion does not derive options.testProductionAbsent from productionClassEmitted(...).\n"+
			"\twriteTestProject is handed THIS function's options — convertTestVariants takes Options by VALUE, so its own\n"+
			"\tassignment never reaches here — and without the derivation every test-only package's .tests.csproj emits a\n"+
			"\tcolocated ProjectReference to a production .csproj that is never written (MSB9008). Restore the assignment\n"+
			"\tin %s, ahead of the writeTestProject call.", source)
	}

	if derivation > call {
		t.Errorf("processTestConversion derives options.testProductionAbsent AFTER it calls writeTestProject (offsets %d > %d);\n"+
			"\tthe project write reads the field's zero value (\"production exists\") and re-emits the dangling reference.",
			derivation, call)
	}
}

// indexOfLineContaining returns the byte offset of the first line of text holding EVERY one of
// needles, or -1. Line-scoped rather than a bare Index over the whole text so that an assignment
// and the call it must be derived from have to sit on the SAME statement, not merely both appear
// somewhere in the function.
func indexOfLineContaining(text string, needles ...string) int {
	offset := 0

	for _, line := range strings.Split(text, "\n") {
		matched := true

		for _, needle := range needles {
			if !strings.Contains(line, needle) {
				matched = false
				break
			}
		}

		if matched {
			return offset
		}

		offset += len(line) + 1
	}

	return -1
}

// ─────────────────────────────────────────────────────────────────────────────────────────────
// The MIXED shape, which the arms above cannot reach.
//
// ⚠ The fixture above deliberately loads NO external variant (`convertTestOnlyFixture` fails if one
// appears), because it reproduces `crypto/internal/fips140test`. That is the whole reason this
// defect survived those guards: a suite with BOTH an internal and an external variant writes a
// SECOND metadata file — `package_info_internal_test.cs`, the white-box BRIDGE anchor
// (internalTestPackageInfoSeed) — and that file is emitted by no path the arms above take. Its
// `using static <pkg>_package` was written unconditionally while every other site of the family
// consulted productionClassEmitted, so a package that is BOTH test-only AND mixed opened its
// bridge anchor against a class that was never emitted (CS0234).
//
// `embed/internal/embedtest` is the corpus's only such package: of the stdlib's test-only packages
// at go1.24.13 it is the ONLY one carrying both variants, which is why one row and no other showed
// the error. Measured on the real source: the bridge anchor's line 11 read
// `using static go.embed.@internal.embedtest_package;`.
//
// The two arms are one measurement with its own control, varying the SAME single axis the arms
// above vary — whether the package has a production file — while holding the mixed shape fixed.
// ⚠ THE CONTROL IS THE LOAD-BEARING HALF: it is what keeps the fix from being "stop naming the
// production class in bridge anchors", which would break every ordinary mixed suite silently.
func writeMixedSuiteFixture(t *testing.T, withProduction bool) string {
	t.Helper()

	dir := t.TempDir()

	internalSource := "package " + testOnlyFixturePackage + "\n\n" +
		"import \"testing\"\n\n" +
		"func TestArea(t *testing.T) {\n" +
		"\tif area(2, 3) != 6 {\n" +
		"\t\tt.Fatal(\"area\")\n" +
		"\t}\n" +
		"}\n"

	files := map[string]string{"go.mod": "module example/mixed\n\ngo 1.23\n"}

	if withProduction {
		files[testOnlyFixtureDir+"/shape.go"] = "package " + testOnlyFixturePackage + "\n\n" +
			"func Area(w, h int) int { return w * h }\n\n" +
			"func area(w, h int) int { return w * h }\n"
	} else {
		internalSource += "\nfunc area(w, h int) int { return w * h }\n"
	}

	files[testOnlyFixtureDir+"/shape_test.go"] = internalSource

	// The EXTERNAL variant is what selects the mixed white-box path and so is what causes the
	// bridge anchor to be written at all. It only ever touches the package's exported surface,
	// which is all an external test package can reach.
	external := "package " + testOnlyFixturePackage + "_test\n\n" +
		"import \"testing\"\n\n" +
		"func TestExternal(t *testing.T) {\n" +
		"\t_ = t\n" +
		"}\n"

	files[testOnlyFixtureDir+"/shapex_test.go"] = external

	writeModuleFiles(t, dir, files)

	return filepath.Join(dir, testOnlyFixtureDir)
}

// convertMixedSuiteFixture runs the REAL `-tests` wiring over a MIXED fixture and returns the
// emitted `.cs` files keyed by base name, exactly as convertTestOnlyFixture does for the
// internal-only shape. It asserts the external variant IS present — the arms are about the file
// that only a mixed suite writes, so a fixture that lost its external half would make both of them
// vacuous while still passing the refusal one.
func convertMixedSuiteFixture(t *testing.T, inputPath string) map[string]string {
	t.Helper()

	loaded, err := packages.Load(&packages.Config{Mode: packages.LoadAllSyntax, Dir: inputPath, Tests: true}, ".")
	if err != nil {
		t.Fatal(err)
	}

	production := findProductionPackage(loaded, inputPath)
	if production == nil {
		t.Fatal("production package was not loaded")
	}

	internal, external := findTestVariants(loaded, production)
	if internal == nil || external == nil {
		t.Fatalf("the MIXED fixture must load BOTH variants (internal=%v external=%v) — the bridge anchor is written by no other shape", internal != nil, external != nil)
	}

	if model := selectTestProjectModel(internal, external); model != testProjectWhiteboxReference {
		t.Fatalf("fixture model = %v, want whitebox-reference", model)
	}

	outputPath := t.TempDir()

	resetPackageState(&packages.Package{})
	packageNamespace = "go"

	options := Options{
		indentSpaces:        4,
		preferVarDecl:       true,
		useChannelOperators: true,
		convertTests:        true,
		targetPlatform:      runtime.GOOS + "/" + runtime.GOARCH,
	}

	options.testPackagePath = production.PkgPath
	options.testPackageName = production.Name

	if _, err = convertTestVariants(testProjectWhiteboxReference, production, internal, external,
		selectCompileExcludedTestFiles(internal, external), inputPath, outputPath, "go",
		NewHashSet(supportedTestCapabilities()), options); err != nil {
		t.Fatalf("convertTestVariants: %v", err)
	}

	emitted, err := filepath.Glob(filepath.Join(outputPath, "*.cs"))
	if err != nil {
		t.Fatal(err)
	}

	if len(emitted) == 0 {
		t.Fatalf("no converted file was emitted into %s", outputPath)
	}

	files := map[string]string{}

	for _, path := range emitted {
		body, readErr := os.ReadFile(path)
		if readErr != nil {
			t.Fatal(readErr)
		}

		files[filepath.Base(path)] = string(body)
	}

	return files
}

// bridgeAnchorOf returns the white-box bridge anchor's contents, failing if it was not emitted —
// which is the vacuity guard for both arms below: "no line names the production class" is true of
// a file that does not exist.
func bridgeAnchorOf(t *testing.T, files map[string]string) string {
	t.Helper()

	body, ok := files[internalTestPackageInfoFileName]

	if !ok {
		names := make([]string, 0, len(files))
		for name := range files {
			names = append(names, name)
		}
		sort.Strings(names)
		t.Fatalf("no %s was emitted, so neither arm below is a reading; emitted: %v", internalTestPackageInfoFileName, names)
	}

	return body
}

// The SUBJECT: a TEST-ONLY package with both variants. Its bridge anchor must not name a production
// class that was never emitted.
func TestTestOnlyMixedSuiteBridgeAnchorNamesNoProductionClass(t *testing.T) {
	files := convertMixedSuiteFixture(t, writeMixedSuiteFixture(t, false))

	anchor := bridgeAnchorOf(t, files)
	production := "using static go." + getSanitizedImport(testOnlyFixturePackage+PackageSuffix) + ";"

	if strings.Contains(anchor, production) {
		t.Fatalf("the bridge anchor of a TEST-ONLY package names a production class that was never emitted (CS0234): %q\n%s", production, anchor)
	}

	// ⚠ …and it must not have written an EMPTY class name either. Without this the seed's own
	// `productionClassName != ""` defence is untestable: with the caller's guard in place and the
	// seed's removed, the emission reads `using static go.;` — malformed, still not the production
	// class, and the Contains check above passes it. Each guard is now reachable by its own red.
	if strings.Contains(anchor, "using static go.;") {
		t.Fatalf("the bridge anchor wrote an EMPTY class name:\n%s", anchor)
	}

	// …and it still imports the bridge itself, which is the class it exists to anchor. Without this
	// the arm would pass for an anchor that imported nothing at all.
	bridge := "using static go." + getSanitizedImport(testOnlyFixturePackage+"_internal_test"+PackageSuffix) + ";"

	if !strings.Contains(anchor, bridge) {
		t.Fatalf("the bridge anchor must still import the bridge class %q:\n%s", bridge, anchor)
	}
}

// ⚠ THE CONTROL, and it is what keeps the fix narrow: the same mixed shape WITH a production half
// must still name it. A fix that simply stopped writing the directive would pass the subject and
// break every ordinary mixed suite — sort, bytes, strings, container/list — which is the shape the
// corpus actually has.
func TestMixedSuiteWithProductionBridgeAnchorNamesTheProductionClass(t *testing.T) {
	files := convertMixedSuiteFixture(t, writeMixedSuiteFixture(t, true))

	anchor := bridgeAnchorOf(t, files)
	production := "using static go." + getSanitizedImport(testOnlyFixturePackage+PackageSuffix) + ";"

	if !strings.Contains(anchor, production) {
		t.Fatalf("a mixed suite WITH a production half must still import %q in its bridge anchor:\n%s", production, anchor)
	}
}
