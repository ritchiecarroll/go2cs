// moduleTestsDriver.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"errors"
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
	path, ok := goModFile(moduleDir)

	if !ok {
		path = filepath.Join(moduleDir, "go.mod")
	}

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
	// The run's build tags and environment, as the production load (loadClosure) and each test conversion read the
	// module: a test file under a -tags tag converts, so the packages only it imports must be discovered too.
	cfg := &packages.Config{
		Mode:       packages.NeedName | packages.NeedFiles | packages.NeedImports | packages.NeedDeps | packages.NeedModule | packages.NeedForTest,
		Dir:        moduleDir,
		Tests:      true,
		BuildFlags: m.options.loaderBuildFlags(),
		Env:        m.closureLoaderEnv(),
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

	// Each tested package's test closure: every import path its test variants (`p [p.test]`, `p_test [p.test]`)
	// reach, production and test-only imports alike. Phase B reads it to name a tested package that a refused package
	// reaches, as `go test ./...` fails that package's build.
	m.testClosures = make(map[string]map[string]bool)

	for _, root := range roots {
		if root.ForTest == "" {
			continue
		}

		reached := m.testClosures[root.ForTest]

		if reached == nil {
			reached = make(map[string]bool)
			m.testClosures[root.ForTest] = reached
		}

		addReachedImportPaths(root, reached, make(map[string]bool))
	}

	return nil
}

// addReachedImportPaths adds the import path of every package pkg reaches through its imports to reached.
func addReachedImportPaths(pkg *packages.Package, reached map[string]bool, seen map[string]bool) {
	for _, imported := range pkg.Imports {
		if seen[imported.ID] {
			continue
		}

		seen[imported.ID] = true
		reached[imported.PkgPath] = true
		addReachedImportPaths(imported, reached, seen)
	}
}

// refusedTestClosureImport is the first refused package, in import-path order, that pkgPath's test closure reaches,
// or "" when it reaches none.
func (m *ModuleConverter) refusedTestClosureImport(pkgPath string, refusedCode map[string]string) string {
	var reached []string

	for path := range m.testClosures[pkgPath] {
		if _, isRefused := refusedCode[path]; isRefused && path != pkgPath {
			reached = append(reached, path)
		}
	}

	if len(reached) == 0 {
		return ""
	}

	sort.Strings(reached)

	return reached[0]
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
func runModuleTests(moduleDir string, outRoot string, options Options, corpusRelease string) (result error) {
	if err := checkOutputRootOutsideModule(moduleDir, outRoot); err != nil {
		return err
	}

	// Before any conversion: a -module-disclosures root this run cannot apply is refused now, not after an hour of
	// conversion, and never silently (moduleDisclosurePreflight).
	if err := moduleDisclosurePreflight(options.testModuleDisclosures, moduleDir, options.testModuleVersion); err != nil {
		return err
	}

	// A module-cache release converts AND tests from the copy plain -recurse converts it from: both phases run in it, so
	// phase B's go test runs read the go.mod phase A's load read, and no go command runs in the cache (one with a go.mod
	// and no go line had it rewritten there: gopkg.in/yaml.v3 v3.0.1). A release with no go.mod
	// (github.com/pmezard/go-difflib v1.0.0, github.com/davecgh/go-spew v1.1.1) carries the cache's synthesized one; the
	// go.mod overlay this used to install failed every such run, since go list refuses an overlay beneath GOMODCACHE.
	if goModSource := moduleCacheReleaseGoMod(moduleDir); goModSource != "" {
		copyDir, leave, err := enterModuleCacheReleaseCopy(moduleDir, goModSource)

		if err != nil {
			return err
		}

		defer leave()
		defer func() { result = remapCopiedError(result) }()

		moduleDir = copyDir

		if err := fillModuleCopySums(moduleDir); err != nil {
			return err
		}
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

	// A refused package does not stop the run (recurseRefusal.go): phase B still tests every package it does not
	// reach. Any other phase-A error does.
	var refusal *refusalError

	if err := converter.ConvertModule(moduleDir); err != nil && !errors.As(err, &refusal) {
		return fmt.Errorf("phase A (module conversion): %w", err)
	}

	refusedCode := make(map[string]string, len(converter.refused))

	for _, r := range converter.refused {
		refusedCode[r.path] = r.code
	}

	// Phase B: the -tests pipeline per MAIN-MODULE package with tests, into its phase-A directory,
	// with the recurse resolver in force so siblings and dependencies resolve to the same tree.
	targets := converter.mainModulePackagesWithTests()

	if len(targets) == 0 {
		return fmt.Errorf("the module at %s has no package with Go test files", moduleDir)
	}

	var failed []string

	// The tested packages that did not validate, for MODULE.md (import path -> the run's reason).
	unvalidated := map[string]string{}

	for _, pkg := range targets {
		// As `go test ./...` fails the build of a package it cannot compile, a tested package that is refused, or whose
		// test closure imports a refused package, is named with that reason and gets no test project.
		reason := ""

		if code, isRefused := refusedCode[pkg.Path]; isRefused {
			reason = fmt.Sprintf("the package was refused (%s)", code)
		} else if refused := converter.refusedTestClosureImport(pkg.Path, refusedCode); refused != "" {
			reason = "its test closure imports the refused package " + refused
		}

		if reason != "" {
			fmt.Printf("\n-tests %s: not converted: %s\n", pkg.Path, reason)
			failed = append(failed, fmt.Sprintf("  %s: not converted: %s", pkg.Path, reason))
			unvalidated[pkg.Path] = "not converted: " + reason
			continue
		}

		pkgOptions := options
		pkgOptions.convertTests = true
		pkgOptions.includeComments = true
		pkgOptions.recurse = true
		pkgOptions.recurseOutputRoot = outRoot
		pkgOptions.mainModulePath = converter.options.mainModulePath
		pkgOptions.mainModuleDir = converter.options.mainModuleDir
		pkgOptions.programDependencies = converter.sortedThirdPartyModules()

		outputDir := converter.outputDirFor(pkg.Path)
		fmt.Printf("\n-tests %s -> %s\n", pkg.Path, outputDir)

		if err := convertAndActOnPackageTests(pkg.Dir, outputDir, pkgOptions, pkg.Path); err != nil {
			failed = append(failed, fmt.Sprintf("  %s: %v", pkg.Path, err))
			unvalidated[pkg.Path] = err.Error()
		}
	}

	if len(unvalidated) > 0 && recordsUnvalidatedPackages(options) {
		if err := recordUnvalidatedModulePackages(outRoot, converter.options.mainModulePath, unvalidated); err != nil {
			failed = append(failed, fmt.Sprintf("  MODULE.md: naming the %d package(s) that did not validate: %v", len(unvalidated), err))
		} else {
			fmt.Printf("\nMODULE.md names %d tested package(s) that did not validate.\n", len(unvalidated))
		}
	}

	var phaseB error

	if len(failed) > 0 {
		phaseB = fmt.Errorf("phase B: %d of %d package(s) failed:\n%s", len(failed), len(targets), strings.Join(failed, "\n"))
	}

	// A refusal still ends the run nonzero, as `go test ./...` exits 1 when a package fails to build.
	switch {
	case refusal != nil && phaseB != nil:
		return fmt.Errorf("phase A (module conversion): %w\n%w", refusal, phaseB)
	case refusal != nil:
		return fmt.Errorf("phase A (module conversion): %w", refusal)
	}

	return phaseB
}

// recordsUnvalidatedPackages reports whether a -tests -recurse run names its failed packages in MODULE.md: a run that
// compares, unfiltered, which is the run that writes proof pages (publishesRosterArtifacts).
func recordsUnvalidatedPackages(options Options) bool {
	return (options.testAction == "all" || options.testAction == "compare") && strings.TrimSpace(options.testFilter) == ""
}

// convertAndActOnPackageTests is the single-package -tests flow for one package pkgPath (named for the stall watch):
// convert (production recompiled with the test closure's usings, then the tests), then the requested action.
func convertAndActOnPackageTests(inputDir string, outputDir string, options Options, pkgPath string) error {
	kind, err := requireConvertibleTestTarget(inputDir, outputDir, options)

	if err != nil {
		return err
	}

	options.testHandOwnHost = kind == testTargetHandOwnHost

	if options.testAction == "convert" || options.testAction == "all" {
		collectSiblingTestClosure(inputDir, options)

		if err := stallWatch(pkgPath, func() error { return processConversion(inputDir, true, outputDir, options) }); err != nil {
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
