// fieldAccessReferenceModel_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

// Seat #45 follow-up. A [GoFieldAccess] record never needs the production class: go2cs-gen emits a record's field
// properties into the USING compilation, in an internal class, and extends a foreign struct as readily as a local
// one. Seat #45 partitioned the external suite's records with the conversion-pair rule, so a qualified record counted
// as production-anchored, and the reference model refused it:
//   - F1: 20 roster rows fell back to the recompile model (`errors`, for errors_test's `fs.PathError`);
//   - F2: the hand-owned host `testing`, which has no fallback, refused outright.
// The second half: a refusing -tests conversion leaves its output directory exactly as it found it.

import (
	"errors"
	"os"
	"path/filepath"
	"runtime"
	"sort"
	"testing"

	"github.com/ritchiecarroll/hashset"
	"golang.org/x/tools/go/packages"
)

// writeForeignFieldFixture writes a black-box-only suite whose external test reads a field of another package's
// struct through a pointer, as errors_test does with fs.PathError. withValueImplementer adds the control: the
// external test also uses a production struct as an io.Writer by value, a record that genuinely needs the production
// class.
func writeForeignFieldFixture(t *testing.T, withValueImplementer bool) string {
	t.Helper()

	dir := t.TempDir()

	production := "package widget\n\nfunc Name() string { return \"widget\" }\n"
	testBody := "\te := &fs.PathError{Op: widget.Name()}\n" +
		"\tif e.Op != \"widget\" {\n" +
		"\t\tt.Fatal(e.Op)\n" +
		"\t}\n"
	imports := "\t\"io/fs\"\n\t\"testing\"\n"

	if withValueImplementer {
		production += "\ntype Buffer struct{ N int }\n\nfunc (b Buffer) Write(p []byte) (int, error) { return len(p), nil }\n"
		testBody += "\tvar w io.Writer = widget.Buffer{}\n" +
			"\tif _, err := w.Write(nil); err != nil {\n" +
			"\t\tt.Fatal(err)\n" +
			"\t}\n"
		imports = "\t\"io\"\n" + imports
	}

	writeModuleFiles(t, dir, map[string]string{
		"go.mod":           "module example/foreignfield\n\ngo 1.23\n",
		"widget/widget.go": production,
		"widget/widget_test.go": "package widget_test\n\nimport (\n" + imports + "\n\t\"example/foreignfield/widget\"\n)\n\n" +
			"func TestPathErrorOp(t *testing.T) {\n" + testBody + "}\n",
	})

	return filepath.Join(dir, "widget")
}

func foreignFieldTestOptions(production *packages.Package) Options {
	options := Options{
		indentSpaces:        4,
		preferVarDecl:       true,
		useChannelOperators: true,
		convertTests:        true,
		targetPlatform:      runtime.GOOS + "/" + runtime.GOARCH,
	}

	options.testPackagePath = production.PkgPath
	options.testPackageName = production.Name

	return options
}

// F1 at the emission: the reference model converts the suite, with the foreign struct's record in its one anchor.
func TestForeignFieldRecordKeepsTheReferenceModel(t *testing.T) {
	isolateConverterPathGlobals(t)

	inputPath := writeForeignFieldFixture(t, false)

	loaded, err := packages.Load(&packages.Config{Mode: packages.LoadAllSyntax, Dir: inputPath, Tests: true}, ".")
	if err != nil {
		t.Fatal(err)
	}

	production := findProductionPackage(loaded, inputPath)
	if production == nil {
		t.Fatal("production package was not loaded")
	}

	internal, external := findTestVariants(loaded, production)
	if internal != nil || external == nil {
		t.Fatalf("the fixture must load the EXTERNAL variant alone (internal=%v external=%v)", internal != nil, external != nil)
	}

	if model := selectTestProjectModel(internal, external); model != testProjectReference {
		t.Fatalf("fixture model = %v, want reference", model)
	}

	outputPath := t.TempDir()

	resetPackageState(&packages.Package{})
	packageNamespace = "go"

	_, err = convertTestVariants(testProjectReference, production, internal, external,
		selectCompileExcludedTestFiles(internal, external), inputPath, outputPath, "go",
		hashset.NewHashSet(supportedTestCapabilities()), foreignFieldTestOptions(production))

	if errors.Is(err, errProductionAnchoredRecords) {
		t.Fatalf("the reference model refused a foreign struct's [GoFieldAccess] record, so the row falls back to recompile: %v", err)
	}
	if err != nil {
		t.Fatalf("convertTestVariants: %v", err)
	}

	body, err := os.ReadFile(filepath.Join(outputPath, testPackageInfoFileName))
	if err != nil {
		t.Fatal(err)
	}

	records := fieldAccessRecordLines(string(body))
	t.Logf("%s: %v", testPackageInfoFileName, records)

	// Vacuity: the suite did record the foreign struct, so the arm above read the record it is about.
	if len(recordsNaming(records, "PathError")) == 0 {
		t.Fatalf("%s must carry the external suite's PathError record: %v", testPackageInfoFileName, records)
	}

	if _, err := os.Stat(filepath.Join(outputPath, externalTestPackageInfoFileName)); err == nil {
		t.Errorf("%s was written: that file is the recompile model's", externalTestPackageInfoFileName)
	}

	resetPackageState(&packages.Package{})
}

// F1 and F2 at the predicate, under both host flags: qualified field-access records alone require no production
// anchor, while a production-qualified VALUE implementer still does (the control that keeps the predicate live).
func TestFieldAccessRecordsNeverRequireTheProductionAnchor(t *testing.T) {
	isolateConverterPathGlobals(t)

	for _, handOwnHost := range []bool{false, true} {
		resetPackageState(&packages.Package{})
		packageNamespace = "go"

		fieldAccessRecords.Add("go.io.fs_package.PathError")
		fieldAccessRecords.Add("value_package.Box")

		if recordsRequireProductionAnchor("value_package", "value", handOwnHost) {
			t.Errorf("handOwnHost=%v: qualified [GoFieldAccess] records required the production anchor", handOwnHost)
		}

		interfaceImplementations["io_package.Writer"] = hashset.NewHashSet([]string{"value_package.Buffer"})

		if !recordsRequireProductionAnchor("value_package", "value", handOwnHost) {
			t.Errorf("handOwnHost=%v: control: a production-qualified VALUE implementer must still require the production anchor", handOwnHost)
		}
	}

	resetPackageState(&packages.Package{})
}

// The side finding at the real entry point: a conversion that refuses leaves its output directory as it found it.
// The control fixture's value implementer refuses the reference model; the recompile fallback then needs the
// production package_info.cs, which this test never writes, so processTestConversion fails after the reference
// attempt has written its test files.
func TestRefusedTestConversionLeavesTheOutputUntouched(t *testing.T) {
	isolateConverterPathGlobals(t)

	inputPath := writeForeignFieldFixture(t, true)
	outputPath := t.TempDir()

	before := map[string]string{
		"widget_test.cs":             "// committed test file\n",
		testPackageInfoFileName:      "// committed test metadata\n",
		filepath.Join("bin", "keep"): "build output\n",
	}

	for name, body := range before {
		path := filepath.Join(outputPath, name)

		if err := os.MkdirAll(filepath.Dir(path), 0755); err != nil {
			t.Fatal(err)
		}
		if err := os.WriteFile(path, []byte(body), 0644); err != nil {
			t.Fatal(err)
		}
	}

	resetPackageState(&packages.Package{})
	packageNamespace = "go"

	options := Options{
		indentSpaces:        4,
		preferVarDecl:       true,
		useChannelOperators: true,
		convertTests:        true,
		targetPlatform:      runtime.GOOS + "/" + runtime.GOARCH,
	}

	err := processTestConversion(inputPath, outputPath, options)

	// Vacuity: the conversion must refuse, or there is nothing to restore.
	if err == nil {
		t.Fatal("the fixture's conversion must fail: the recompile fallback has no production package_info.cs to read")
	}
	t.Logf("refusal: %v", err)

	after := map[string]string{}

	walkErr := filepath.WalkDir(outputPath, func(path string, entry os.DirEntry, err error) error {
		if err != nil || entry.IsDir() {
			return err
		}

		body, readErr := os.ReadFile(path)
		if readErr != nil {
			return readErr
		}

		relative, relErr := filepath.Rel(outputPath, path)
		if relErr != nil {
			return relErr
		}

		after[relative] = string(body)

		return nil
	})
	if walkErr != nil {
		t.Fatal(walkErr)
	}

	var changed []string

	for name, body := range after {
		if want, ok := before[name]; !ok {
			changed = append(changed, "written: "+name)
		} else if body != want {
			changed = append(changed, "rewritten: "+name)
		}
	}

	for name := range before {
		if _, ok := after[name]; !ok {
			changed = append(changed, "removed: "+name)
		}
	}

	sort.Strings(changed)

	if len(changed) > 0 {
		t.Errorf("a refused -tests conversion changed its output directory: %v", changed)
	}

	resetPackageState(&packages.Package{})
}
