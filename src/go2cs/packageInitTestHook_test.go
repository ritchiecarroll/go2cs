// packageInitTestHook_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"errors"
	"os"
	"path/filepath"
	"strings"
	"testing"

	"github.com/ritchiecarroll/hashset"
	"golang.org/x/tools/go/packages"
)

// A production package_init.cs is a file BOTH -stdlib and -tests write, so it must be byte-identical
// from either mode. It was not: under -tests the static ctor ended with a call to an erasable
// `initᴛᴛtests()` partial, declared beside it, for an internal test variant to implement. So the file
// flipped with whichever mode wrote it last (internal/trace and runtime/windows, found by the H7
// consolidated re-emit), and in the corpus nothing implemented the hook at all.
//
// The hook is gone from production in BOTH modes (H7 ruling F). The one case that needed it -- an
// internal variant on the RECOMPILE model (it shares the production class, which already owns the one
// static-ctor slot) with its own relocated initializers -- runs them from a module initializer in its
// *_test.cs instead: it forces the production class's static ctor, then calls the variant's ordered
// init methods, so production relocations still run first.
//
// Two fixtures, each converted in both modes: one with no implementer, and one that reaches the
// implementer through the real whitebox attempt and its recompile fallback. RED at 94a0677c0b: both
// -tests package_init.cs carry the hook, and the implementer is a `static partial void`.

const packageInitFixtureProduction = `package initfix

// Total depends on Seed, declared in another file, so Go's InitOrder relocates it (package_init.cs).
var Total = Seed * 2

type Point struct{ X, Y int }

func (p Point) Sum() int { return p.X + p.Y }
`

const packageInitFixtureSeed = `package initfix

var Seed = compute()

func compute() int { return 21 }
`

// The internal test file reads the production relocated var from a var of its own, which is
// relocated too; and it binds the production Point to a test-declared interface as a generic
// constraint, which only Point's own base list could satisfy -- a mutation of a closed production
// type the whitebox model cannot host, so the recompile fallback
// (nominalConstraintsRequireProductionMutation).
const packageInitFixtureImplementer = `package initfix

import "testing"

var probe = Total + 1

type summer interface{ Sum() int }

func total[P summer](p P) int { return p.Sum() }

func TestOrder(t *testing.T) {
	if probe != 43 || total(Point{1, 2}) != 3 {
		t.Fatalf("probe = %d, want 43 (production relocations first)", probe)
	}
}
`

const packageInitFixtureNoImplementer = `package initfix

import "testing"

func TestTotal(t *testing.T) {
	if Total != 42 {
		t.Fatalf("Total = %d, want 42", Total)
	}
}
`

func TestPackageInitIsIdenticalFromEitherMode(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: loads the fixture package via go/packages")
	}

	for _, fixture := range []struct {
		name        string
		testSource  string
		implementer bool
	}{
		{name: "no implementer", testSource: packageInitFixtureNoImplementer},
		{name: "recompile-fallback implementer", testSource: packageInitFixtureImplementer, implementer: true},
	} {
		t.Run(fixture.name, func(t *testing.T) {
			pkgDir, outDir, options := incrementalFixture(t)

			for _, name := range []string{"a.go", "b.go"} {
				if err := os.Remove(filepath.Join(pkgDir, name)); err != nil {
					t.Fatal(err)
				}
			}

			writeModuleFile(t, filepath.Join(pkgDir, "go.mod"), "module example.com/initfix\n\ngo 1.23\n")
			writeModuleFile(t, filepath.Join(pkgDir, "a.go"), packageInitFixtureProduction)
			writeModuleFile(t, filepath.Join(pkgDir, "b.go"), packageInitFixtureSeed)
			writeModuleFile(t, filepath.Join(pkgDir, "x_test.go"), fixture.testSource)

			stdlibOut := outDir + "-stdlib"

			if err := processConversion(pkgDir, true, stdlibOut, options); err != nil {
				t.Fatalf("-stdlib-mode production conversion: %v", err)
			}

			testOptions := options
			testOptions.convertTests = true

			if err := processConversion(pkgDir, true, outDir, testOptions); err != nil {
				t.Fatalf("-tests-mode production conversion: %v", err)
			}

			if fixture.implementer {
				convertThroughRecompileFallback(t, pkgDir, outDir, testOptions)
			}

			stdlibInit := readGenerated(t, filepath.Join(stdlibOut, PackageInitFileName))
			testsInit := readGenerated(t, filepath.Join(outDir, PackageInitFileName))

			if !strings.Contains(stdlibInit, "initᴛTotal();") {
				t.Fatalf("the fixture no longer relocates Total, so it cannot guard package_init.cs:\n%s", stdlibInit)
			}

			if testsInit != stdlibInit {
				t.Errorf("package_init.cs differs by mode\n--- -stdlib ---\n%s\n--- -tests ---\n%s", stdlibInit, testsInit)
			}

			if strings.Contains(testsInit, PackageTestInitHookMethod) {
				t.Errorf("the production package_init.cs carries the test hook:\n%s", testsInit)
			}

			if !fixture.implementer {
				return
			}

			implementer := readGenerated(t, filepath.Join(outDir, "package_init_internal_test.cs"))

			for _, want := range []string{
				"[GoInit] internal static void " + PackageTestInitHookMethod + "() {",
				"RuntimeHelpers.RunClassConstructor(typeof(initfix_package).TypeHandle);",
				"initᴛprobe();",
			} {
				if !strings.Contains(implementer, want) {
					t.Errorf("the recompile-model implementer lacks %q:\n%s", want, implementer)
				}
			}

			if strings.Contains(implementer, "static partial void") {
				t.Errorf("the implementer still implements a production partial:\n%s", implementer)
			}

			// Production relocations first: the class ctor is forced BEFORE the variant's own inits.
			if strings.Index(implementer, "RunClassConstructor") > strings.Index(implementer, "initᴛprobe();") {
				t.Errorf("the variant's inits run before the production static ctor:\n%s", implementer)
			}
		})
	}
}

// convertThroughRecompileFallback is what processTestConversion does for a suite with an internal
// variant: the whitebox attempt, which must fail with errProductionAnchoredRecords (the fixture's
// self-check), then the recompile model.
func convertThroughRecompileFallback(t *testing.T, pkgDir, outDir string, testOptions Options) {
	t.Helper()

	loaded, err := packages.Load(&packages.Config{Mode: packages.LoadAllSyntax, Dir: pkgDir, Tests: true}, ".")
	if err != nil {
		t.Fatal(err)
	}

	production := findProductionPackage(loaded, pkgDir)
	internal, external := findTestVariants(loaded, production)

	if model := selectTestProjectModel(internal, external); model != testProjectWhiteboxReference {
		t.Fatalf("fixture model = %v, want whitebox-reference (the attempt that falls back)", model)
	}

	testOptions.testPackagePath = production.PkgPath
	testOptions.testPackageName = production.Name

	_, projectNamespace := getProjectName(pkgDir, testOptions)
	compileExcluded := selectCompileExcludedTestFiles(internal, external)
	supported := hashset.NewHashSet(supportedTestCapabilities())

	_, err = convertTestVariants(testProjectWhiteboxReference, production, internal, external, compileExcluded, pkgDir, outDir, projectNamespace, supported, testOptions)

	if !errors.Is(err, errProductionAnchoredRecords) {
		t.Fatalf("the fixture no longer takes the recompile fallback (whitebox attempt: %v)", err)
	}

	if _, err = convertTestVariants(testProjectRecompile, production, internal, external, compileExcluded, pkgDir, outDir, projectNamespace, supported, testOptions); err != nil {
		t.Fatalf("recompile conversion: %v", err)
	}
}
