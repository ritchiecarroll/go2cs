// hoistedLiteralsStamp_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/build"
	"path/filepath"
	"runtime"
	"slices"
	"strings"
	"testing"

	"github.com/ritchiecarroll/hashset"
	"golang.org/x/tools/go/packages"
)

// The [GoHoistedLiterals] stamp: a package whose literals are hoisted passes no bare literal to an any slot, so the
// converter stamps its info files and go2cs-gen's literal interceptor runs no transform for the assembly.

// convertStampFixture converts a one-file module into out and returns its package_info.cs, LF-normalized.
func convertStampFixture(t *testing.T, root string, hoist bool) string {
	t.Helper()

	appDir := filepath.Join(root, "app")
	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/stamp\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "stamp.go"), "package stamp\n\nimport \"fmt\"\n\nfunc Show() { fmt.Println(\"hello\") }\n")

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{
		goRoot:              goRoot,
		goPath:              build.Default.GOPATH,
		go2csPath:           filepath.Join(root, "out"),
		recurse:             true,
		targetPlatform:      "linux/amd64",
		indentSpaces:        4,
		preferVarDecl:       true,
		useChannelOperators: true,
		hoistLiterals:       hoist,
	}

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	if err := NewModuleConverter(options).ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	return strings.ReplaceAll(readGenerated(t, filepath.Join(options.go2csPath, "src", "example.com", "stamp", "package_info.cs")), "\r\n", "\n")
}

// TestHoistedLiteralsStampFollowsTheLiteralForm: -hoist-literals stamps package_info.cs once, directly above the
// position-map prose; a plain package carries no stamp; and reconverting a stamped package without the flag removes
// the persisted stamp. RED without the remove-then-add regeneration: the plain reconversion kept the stamp.
func TestHoistedLiteralsStampFollowsTheLiteralForm(t *testing.T) {
	isolateConverterPathGlobals(t)
	root := t.TempDir()

	block := strings.Join(append(hoistedLiteralsStampProseLines(), "[assembly: GoHoistedLiterals]", "", goSourcePositionMapsProseLines()[0]), "\n")

	hoisted := convertStampFixture(t, root, true)

	if strings.Count(hoisted, "GoHoistedLiterals") != 1 || !strings.Contains(hoisted, block) {
		t.Fatalf("-hoist-literals: want one stamp block above the position-map prose:\n%s", hoisted)
	}

	plain := convertStampFixture(t, root, false)

	if strings.Contains(plain, "GoHoistedLiterals") || strings.Contains(plain, hoistedLiteralsStampProseLines()[0]) {
		t.Fatalf("a plain reconversion kept the stamp:\n%s", plain)
	}

	if again := convertStampFixture(t, root, true); again != hoisted {
		t.Errorf("a second -hoist-literals conversion is not byte-identical to the first")
	}
}

// TestHoistedLiteralsStampOnlyInTheAssemblysOwnInfoFile: the production and test info files take the stamp, the
// internal-test info file never does (the test assembly compiles it beside package_test_info.cs, and the attribute is
// single-use), and applying it twice changes nothing.
func TestHoistedLiteralsStampOnlyInTheAssemblysOwnInfoFile(t *testing.T) {
	saved := packageHoistsLiterals
	t.Cleanup(func() { packageHoistsLiterals = saved })
	packageHoistsLiterals = true

	lines := append([]string{"// </ImplicitConversions>", ""}, append(goSourcePositionMapsProseLines(), "// <GoSourcePositionMaps>", "// </GoSourcePositionMaps>", "", "namespace go;")...)

	for name, want := range map[string]bool{
		"package_info.cs":               true,
		"package_test_info.cs":          true,
		"package_info_internal_test.cs": false,
	} {
		once := applyHoistedLiteralsStamp(lines, filepath.Join("out", name))

		if got := slices.ContainsFunc(once, isHoistedLiteralsStampLine); got != want {
			t.Errorf("%s: stamped = %v, want %v", name, got, want)
		}

		if twice := applyHoistedLiteralsStamp(once, filepath.Join("out", name)); !slices.Equal(twice, once) {
			t.Errorf("%s: a second application changed the file:\n%s", name, strings.Join(twice, "\n"))
		}
	}

	packageHoistsLiterals = false

	if cleared := applyHoistedLiteralsStamp(applyHoistedLiteralsStampFor(t, lines), "package_info.cs"); !slices.Equal(cleared, lines) {
		t.Errorf("removing the stamp did not restore the file:\n%s", strings.Join(cleared, "\n"))
	}
}

// applyHoistedLiteralsStampFor stamps lines as a hoisting package's package_info.cs.
func applyHoistedLiteralsStampFor(t *testing.T, lines []string) []string {
	t.Helper()

	saved := packageHoistsLiterals
	packageHoistsLiterals = true
	defer func() { packageHoistsLiterals = saved }()

	return applyHoistedLiteralsStamp(lines, "package_info.cs")
}

// convertStampTestsFixture runs the real -tests wiring (convertTestVariants, as processTestConversion calls it) over
// a hoisting package with an INTERNAL and an EXTERNAL test variant, and returns package_test_info.cs and the internal
// test's emission, LF-normalized. The internal test prints internalLiteral; the external one prints nothing.
func convertStampTestsFixture(t *testing.T, internalLiteral string) (testInfo, internalTest string) {
	t.Helper()

	dir := t.TempDir()

	writeModuleFiles(t, dir, map[string]string{
		"go.mod":              "module example/stamptests\n\ngo 1.23\n",
		"stamp/stamp.go":      "package stamp\n\nfunc Name() string { return \"name\" }\n",
		"stamp/stamp_test.go": "package stamp\n\nimport (\n\t\"fmt\"\n\t\"testing\"\n)\n\nfunc TestShow(t *testing.T) { fmt.Println(" + internalLiteral + ") }\n",
		"stamp/ext_test.go":   "package stamp_test\n\nimport (\n\t\"testing\"\n\n\t\"example/stamptests/stamp\"\n)\n\nfunc TestName(t *testing.T) { _ = stamp.Name() }\n",
	})

	inputPath := filepath.Join(dir, "stamp")

	loaded, err := packages.Load(&packages.Config{Mode: packages.LoadAllSyntax, Dir: inputPath, Tests: true}, ".")
	if err != nil {
		t.Fatal(err)
	}

	production := findProductionPackage(loaded, inputPath)
	internal, external := findTestVariants(loaded, production)

	if production == nil || internal == nil || external == nil {
		t.Fatal("the fixture must load a production package and both test variants")
	}

	outputPath := t.TempDir()

	resetPackageState(&packages.Package{})
	packageNamespace = "go"

	options := Options{
		indentSpaces:        4,
		preferVarDecl:       true,
		useChannelOperators: true,
		convertTests:        true,
		hoistLiterals:       true,
		targetPlatform:      runtime.GOOS + "/" + runtime.GOARCH,
		testPackagePath:     production.PkgPath,
		testPackageName:     production.Name,
	}

	model := selectTestProjectModel(internal, external)

	if _, err = convertTestVariants(model, production, internal, external, selectCompileExcludedTestFiles(internal, external),
		inputPath, outputPath, "go", hashset.NewHashSet(supportedTestCapabilities()), options); err != nil {
		t.Fatalf("convertTestVariants: %v", err)
	}

	return strings.ReplaceAll(readGenerated(t, filepath.Join(outputPath, testPackageInfoFileName)), "\r\n", "\n"),
		strings.ReplaceAll(readGenerated(t, filepath.Join(outputPath, "stamp_test.cs")), "\r\n", "\n")
}

// TestHoistedLiteralsStampSpansTheTestAssembly: the test assembly's stamp is one decision over every variant it
// compiles. A degenerate slug (`"0x"`) in the internal test renders bare at fmt.Println's any slot (the hybrid's
// fallback), so go2cs-gen's interceptor must box it, and package_test_info.cs may carry no stamp. RED before the
// assembly-wide flag: each variant rewrites the file from its OWN collection, and the external variant, written last
// and boxing nothing, stamped the assembly (encoding/json's tests: GO2CS0004 at decode_test.cs and encode_test.cs).
// The control, a healthy slug, keeps its field and the stamp, so the refusal is not the stamp going missing.
func TestHoistedLiteralsStampSpansTheTestAssembly(t *testing.T) {
	isolateConverterPathGlobals(t)

	testInfo, internalTest := convertStampTestsFixture(t, `"0x"`)

	if !strings.Contains(internalTest, `fmt.Println("0x")`) {
		t.Fatalf("the degenerate slug should render bare at the any slot:\n%s", internalTest)
	}

	if strings.Contains(testInfo, "GoHoistedLiterals") {
		t.Errorf("package_test_info.cs carries the stamp, but the internal test boxes a bare literal:\n%s", testInfo)
	}

	testInfo, internalTest = convertStampTestsFixture(t, `"hello"`)

	if !strings.Contains(internalTest, "helloˢ") || strings.Count(testInfo, "[assembly: GoHoistedLiterals]") != 1 {
		t.Errorf("control: a healthy slug keeps its field and the assembly keeps one stamp:\n%s\n----\n%s", internalTest, testInfo)
	}
}
