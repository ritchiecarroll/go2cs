// moduleTestsDriver.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"fmt"
	"go/version"
	"os"
	"path/filepath"
	"sort"
	"strings"

	"golang.org/x/mod/modfile"
	"golang.org/x/tools/go/packages"
)

// The -tests -recurse driver (DESIGN-multi-package-modules D1 + D4): one command validates a whole
// end-user module against its own Go tests, into ONE output root.
//
//	Phase A -- today's -recurse conversion (ModuleConverter), with the convert-set widened by the
//	           module's TEST closure: a package imported only from _test.go files (a test helper) and
//	           any test-only dependency are converted like every other package.
//	Phase B -- the -tests pipeline once per main-module package that has test files, into THAT
//	           package's phase-A directory (<outRoot>/src/<importPath>), so the test project sits beside
//	           the production project exactly as a single-package -tests run leaves it, and every
//	           sibling or dependency reference resolves through the recurse resolver.
//
// Refused up front: an output root inside the module's own source tree (D1.5), and a module whose `go`
// directive is newer than the converted standard library it would compile against (D4).

// moduleGoDirective returns the module's `go` directive ("" when go.mod has none).
func moduleGoDirective(moduleDir string) (string, error) {
	path := filepath.Join(moduleDir, "go.mod")
	data, err := os.ReadFile(path)

	if err != nil {
		return "", err
	}

	parsed, err := modfile.ParseLax(path, data, nil)

	if err != nil {
		return "", err
	}

	if parsed.Go == nil {
		return "", nil
	}

	return parsed.Go.Version, nil
}

// checkModuleGoLine refuses a module whose `go` line is NEWER than the corpus release: its code may use
// language or library features the converted standard library does not have. An older line is fine.
func checkModuleGoLine(moduleDir string, corpusRelease string) error {
	goLine, err := moduleGoDirective(moduleDir)

	if err != nil {
		return fmt.Errorf("reading the module's go directive: %w", err)
	}

	module, corpus := normalizeGoVersion(goLine), normalizeGoVersion(corpusRelease)

	if module != "" && corpus != "" && version.Compare(module, corpus) > 0 {
		return fmt.Errorf("refusing %s: its go.mod declares `go %s`, newer than the converted standard library "+
			"(%s, <GoStdLibVersion>); convert it with a corpus at %s or later", moduleDir, goLine, corpus, module)
	}

	return nil
}

// checkOutputRootOutsideModule refuses an output root at or inside the module's source tree: the
// recurse layout writes src/ and pkg/ trees that a later `./...` load would walk as the module's own.
func checkOutputRootOutsideModule(moduleDir string, outRoot string) error {
	if outRoot == moduleDir || isPathUnder(outRoot, moduleDir) {
		return fmt.Errorf("refusing output root %s: it is inside the module's source tree %s; "+
			"-tests -recurse writes its whole src/ and pkg/ layout there, so give an output root outside the module",
			outRoot, moduleDir)
	}

	return nil
}

// addTestOnlyPackages widens the convert-set with the packages the module's TESTS need that its
// production code does not. It is a separate Tests:true load used ONLY for discovery, and the go
// command's test scaffolding in it never reaches the convert-set:
//   - a test VARIANT (`p [p.test]`, `p_test [p.test]`) has an ID different from its PkgPath (and a
//     ForTest, now that NeedForTest asks for it) and is never deduped into the production walk --
//     `p [p.test]` shares p's PkgPath and would otherwise stand in for production, turning test-only
//     imports into conversion-ORDER edges;
//   - the synthesized test MAIN `p.test` has ID == PkgPath, so it needs its own test: Name main, a
//     `.test` suffix, and the package it tests (`p`) in the same load. The last clause keeps a genuine
//     package whose path happens to end in `.test` convertible.
//
// Only ordinary packages reachable from the variants are added, with their production imports.
func (m *ModuleConverter) addTestOnlyPackages(moduleDir string, closure map[string]*packages.Package) error {
	cfg := &packages.Config{
		Mode:  packages.NeedName | packages.NeedFiles | packages.NeedImports | packages.NeedDeps | packages.NeedModule | packages.NeedForTest,
		Dir:   moduleDir,
		Tests: true,
		Env:   os.Environ(),
	}

	roots, err := packages.Load(cfg, "./...")

	if err != nil {
		return fmt.Errorf("loading the module's test closure: %w", err)
	}

	loaded := make(map[string]bool)

	for _, root := range roots {
		if root.ID == root.PkgPath {
			loaded[root.PkgPath] = true
		}
	}

	isTestMain := func(pkg *packages.Package) bool {
		tested, isTestSuffix := strings.CutSuffix(pkg.PkgPath, ".test")
		return pkg.Name == "main" && pkg.ID == pkg.PkgPath && isTestSuffix && loaded[tested]
	}

	seen := make(map[string]bool)
	var visit func(pkg *packages.Package)

	visit = func(pkg *packages.Package) {
		if seen[pkg.ID] {
			return
		}

		seen[pkg.ID] = true

		isVariant := pkg.ID != pkg.PkgPath || pkg.ForTest != "" || isTestMain(pkg)

		if !isVariant && closure[pkg.PkgPath] == nil {
			switch m.classify(pkg) {
			case classApp, classThirdParty:
				closure[pkg.PkgPath] = pkg
				m.addToConvertSet(pkg)
			}
		}

		for _, imported := range pkg.Imports {
			visit(imported)
		}
	}

	for _, root := range roots {
		visit(root)
	}

	return nil
}

// mainModulePackagesWithTests lists the convert-set's main-module packages that have Go test files,
// sorted, with their source directories.
func (m *ModuleConverter) mainModulePackagesWithTests() []*Package {
	var result []*Package

	for path, pkg := range m.graph.packages {
		if !isMainModulePackage(path, m.options.mainModulePath) {
			continue
		}

		if tests, _ := filepath.Glob(filepath.Join(pkg.Dir, "*_test.go")); len(tests) > 0 {
			result = append(result, pkg)
		}
	}

	sort.Slice(result, func(i, j int) bool { return result[i].Path < result[j].Path })

	return result
}

// runModuleTests is the -tests -recurse driver. moduleDir and outRoot are absolute; options carries
// the -tests settings (action, config, timeout) and a resolved go2csPath.
func runModuleTests(moduleDir string, outRoot string, options Options, corpusRelease string) error {
	if err := checkOutputRootOutsideModule(moduleDir, outRoot); err != nil {
		return err
	}

	if err := checkModuleGoLine(moduleDir, corpusRelease); err != nil {
		return err
	}

	// Phase A: the module and its dependency closure, widened by the test closure. No test
	// conversion happens here: convertTests would otherwise run the -tests pipeline inside
	// convertAll for EVERY package, third-party included.
	phaseA := options
	phaseA.convertTests = false
	phaseA.recurse = true
	phaseA.recurseOutputRoot = outRoot

	converter := NewModuleConverter(phaseA)
	converter.includeTestClosure = true

	if err := converter.ConvertModule(moduleDir); err != nil {
		return fmt.Errorf("phase A (module conversion): %w", err)
	}

	// Phase B: the -tests pipeline per MAIN-MODULE package with tests, into its phase-A directory,
	// with the recurse resolver in force so siblings and dependencies resolve to the same tree.
	targets := converter.mainModulePackagesWithTests()

	if len(targets) == 0 {
		return fmt.Errorf("the module at %s has no package with Go test files", moduleDir)
	}

	var failed []string

	for _, pkg := range targets {
		pkgOptions := options
		pkgOptions.convertTests = true
		pkgOptions.includeComments = true
		pkgOptions.recurse = true
		pkgOptions.recurseOutputRoot = outRoot
		pkgOptions.mainModulePath = converter.options.mainModulePath
		pkgOptions.mainModuleDir = converter.options.mainModuleDir

		outputDir := converter.outputDirFor(pkg.Path)
		fmt.Printf("\n-tests %s -> %s\n", pkg.Path, outputDir)

		if err := convertAndActOnPackageTests(pkg.Dir, outputDir, pkgOptions); err != nil {
			failed = append(failed, fmt.Sprintf("  %s: %v", pkg.Path, err))
		}
	}

	if len(failed) > 0 {
		return fmt.Errorf("phase B: %d of %d package(s) failed:\n%s", len(failed), len(targets), strings.Join(failed, "\n"))
	}

	return nil
}

// convertAndActOnPackageTests is the single-package -tests flow of main() for one package: convert
// (production recompiled with the test closure's usings, then the tests), then the requested action.
func convertAndActOnPackageTests(inputDir string, outputDir string, options Options) error {
	kind, err := requireConvertibleTestTarget(inputDir, outputDir, options)

	if err != nil {
		return err
	}

	options.testHandOwnHost = kind == testTargetHandOwnHost

	if options.testAction == "convert" || options.testAction == "all" {
		collectSiblingTestClosure(inputDir, options)

		if err := processConversion(inputDir, true, outputDir, options); err != nil {
			return fmt.Errorf("conversion: %w", err)
		}
	}

	if options.testAction != "convert" {
		if err := executeTestAction(inputDir, outputDir, options); err != nil {
			return fmt.Errorf("test action %q: %w", options.testAction, err)
		}
	}

	return nil
}
