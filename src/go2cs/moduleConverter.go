// moduleConverter.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// This file drives -recurse: recursive end-user module conversion. ModuleConverter converts a
// downloaded Go application PLUS every third-party dependency package in its transitive closure,
// in dependency order (least dependencies first), while REFERENCING (not converting) the
// pre-converted standard library. It is the module-mode counterpart to StdLibConverter; both
// order conversion through the shared DependencyGraph (dependencyGraph.go).
package main

import (
	"fmt"
	"log"
	"os"
	"path/filepath"
	"sort"
	"strings"
	"time"

	"golang.org/x/tools/go/packages"
)

// packageClass classifies a package in the input module's transitive import closure.
type packageClass int

const (
	// classStdLib — under $GOROOT/src (including GOROOT-vendored golang.org/x/…). Referenced as
	// $(go2csPath)core\… against a pre-converted stdlib; never converted here.
	classStdLib packageClass = iota
	// classApp — a package of the input (main) module. Converted.
	classApp
	// classThirdParty — a package of a non-main dependency module (module cache or a `replace`).
	// Converted.
	classThirdParty
	// classSkip — unsafe/builtin/cgo pseudo-packages, test variants, or anything unclassifiable.
	// Ignored.
	classSkip
)

// ModuleConverter converts an end-user module and its third-party dependency closure in
// dependency order, referencing the pre-converted standard library.
type ModuleConverter struct {
	options              Options
	graph                *DependencyGraph
	startTime            time.Time
	convertedProjects    []string          // csproj paths of successfully converted app + third-party packages
	convertedCsproj      map[string]string // import path -> its generated .csproj path (successful conversions)
	referencedThirdParty []string          // -recurse=module: third-party import paths left OUT of the convert-set
}

// NewModuleConverter creates a recursive end-user module converter.
func NewModuleConverter(options Options) *ModuleConverter {
	return &ModuleConverter{
		options:         options,
		graph:           NewDependencyGraph(),
		startTime:       time.Now(),
		convertedCsproj: make(map[string]string),
	}
}

// ConvertModule loads the module rooted at moduleDir, partitions its transitive import closure into
// standard library (referenced), third-party, and app packages, then converts the app + third-party
// packages in dependency order (least dependencies first). The standard library is not converted —
// its imports emit $(go2csPath)core references to a pre-converted stdlib (staged by deploy-core).
func (m *ModuleConverter) ConvertModule(moduleDir string) error {
	fmt.Printf("Recursively converting module at %s\n", moduleDir)

	// The main module's directory is the CONTEXT every module-cache package is re-loaded from — see
	// the load-shape note in processConversion. Resolve it absolute once: the per-package load sets
	// it as the go command's working directory, and a relative path would resolve against whatever
	// the process' cwd happens to be by then.
	if absModuleDir, err := filepath.Abs(moduleDir); err == nil {
		m.options.mainModuleDir = absModuleDir
	} else {
		m.options.mainModuleDir = moduleDir
	}

	// 1. Load the module and its full dependency closure. This load is used only to DISCOVER and
	//    CLASSIFY the closure (import paths, source dirs, module identity) and to build the
	//    dependency graph — each package is re-loaded with full syntax/types when it is converted,
	//    via processConversion. NeedModule lets us classify by module identity (main vs. dependency).
	closure, err := m.loadClosure(moduleDir)

	if err != nil {
		return err
	}

	// 2. Partition the closure and populate the convert-set (app + third-party). Stdlib packages
	//    are deliberately left OUT of the graph, so edges to them never constrain the conversion
	//    order — they are pre-converted and only referenced.
	m.partition(closure)

	if len(m.graph.packages) == 0 {
		if m.options.moduleOnly {
			return fmt.Errorf("found no packages of the module itself to convert at %s", moduleDir)
		}

		return fmt.Errorf("found no app or third-party packages to convert in module at %s", moduleDir)
	}

	// 3. Build dependency edges among the convert-set and order least-dependencies-first, so each
	//    importer observes its dependency's finished package_info.cs (imported collision-rename
	//    aliases) before it is converted.
	m.buildEdges(closure)
	m.graph.sortAdjacency()
	m.graph.topologicalSort()

	// Expose the convert-set graph so a linkname var-pull whose forwarding project reference would
	// cycle is rejected in an END-USER module too, not only under -stdlib (see linknamePullWouldCycle).
	// A pull to the pre-converted stdlib (not a node here) resolves as acyclic — correct, since the
	// stdlib never depends back on app/third-party code.
	conversionGraph = m.graph

	// 4. Convert the convert-set in dependency order.
	m.convertAll()

	// 4a. Under -recurse=module, report the dependency packages that were referenced but deliberately
	//     left unconverted, so the unresolved references in the emitted projects are expected, listed
	//     and actionable rather than a surprise at build time.
	m.reportUnconvertedDependencies()

	// 5. Emit a per-project .slnx next to each converted .csproj, over that project + its transitive
	//    converted dependencies + golib + the analyzer, tied to the pre-converted stdlib (referenced via
	//    $(go2csPath)core) for one dotnet build. The app's own solution thus lists its whole converted
	//    dependency closure — a build-everything solution for the app — so no separate deploy-root
	//    solution is written.
	m.generatePerProjectSolutions()

	// 6. Emit the output root's Directory.Build.props / .targets: the artifact redirection that keeps
	//    every DERIVED path (obj, bin, generator output) inside MAX_PATH regardless of how deep the
	//    converted import paths are, plus the reference-resolution default for whichever reference
	//    scheme was emitted — the go2csPath runtime-root pin under local project references, or the
	//    GoStdLibVersion default for the go.* NuGet PackageReferences under -recurse=nuget.
	m.generateRecurseBuildFiles()

	return nil
}

// loadClosure loads the module at moduleDir and returns its transitive import closure keyed by
// import path (roots + every reachable import, including the standard library, which is classified
// out later).
func (m *ModuleConverter) loadClosure(moduleDir string) (map[string]*packages.Package, error) {
	// Discovery mode: names, source dirs, the import graph and module identity — everything
	// partition/buildEdges read, and nothing more. Each package is re-loaded with full syntax and
	// types when it is converted (processConversion), so the closure load never needs them.
	mode := packages.NeedName | packages.NeedFiles | packages.NeedImports | packages.NeedDeps | packages.NeedModule

	if !m.options.moduleOnly {
		// The established full-closure behavior: type-check the whole graph up front, since every
		// package in it is about to be converted anyway. Under -recurse=module the third-party
		// closure is NOT being converted, and the point of the mode is that its type errors — the
		// ones that make an unconvertible dependency graph block the app — never enter the picture.
		mode |= packages.LoadAllSyntax
	}

	cfg := &packages.Config{
		Mode:       mode,
		Dir:        moduleDir,
		BuildFlags: m.options.loaderBuildFlags(),
	}

	targetParts := strings.Split(m.options.targetPlatform, "/")

	if len(targetParts) == 2 {
		cfg.Env = append(os.Environ(), "GOOS="+targetParts[0], "GOARCH="+targetParts[1])
	}

	pkgs, err := packages.Load(cfg, "./...")

	if err != nil {
		return nil, fmt.Errorf("failed to load module at %q: %w", moduleDir, err)
	}

	if len(pkgs) == 0 {
		return nil, fmt.Errorf("no packages found in module at %q", moduleDir)
	}

	// Report load errors on the root (app) packages as warnings — non-fatal, since a package with
	// errors may still convert usefully (success criterion is a buildable solution; partial compile
	// is acceptable at this stage).
	for _, pkg := range pkgs {
		for _, e := range pkg.Errors {
			log.Printf("WARNING: load error in %s: %v", pkg.PkgPath, e)
		}
	}

	// Collect the transitive closure (roots + all reachable imports), deduped by import path.
	closure := make(map[string]*packages.Package)

	var walk func(p *packages.Package)

	walk = func(p *packages.Package) {
		if p == nil {
			return
		}

		if _, seen := closure[p.PkgPath]; seen {
			return
		}

		closure[p.PkgPath] = p

		for _, imp := range p.Imports {
			walk(imp)
		}
	}

	for _, p := range pkgs {
		walk(p)
	}

	return closure, nil
}

// classify buckets a package by source location and module identity — more robust than a
// dotted-domain heuristic: stdlib is recognized by its location under $GOROOT/src (covering
// GOROOT-vendored packages too), the app by main-module membership, and third-party by
// dependency-module membership.
func (m *ModuleConverter) classify(pkg *packages.Package) packageClass {
	pkgPath := pkg.PkgPath

	// Pseudo-packages (unsafe/builtin have no real source to convert; C is cgo) and test variants.
	if pkgPath == "unsafe" || pkgPath == "builtin" || pkgPath == "C" || strings.HasSuffix(pkgPath, "_test") {
		return classSkip
	}

	if pkg.Dir == "" {
		return classSkip
	}

	// Standard library (including GOROOT-vendored golang.org/x/… under $GOROOT/src/vendor).
	if isPathUnder(pkg.Dir, filepath.Join(m.options.goRoot, "src")) {
		return classStdLib
	}

	// The input module's own packages.
	if pkg.Module != nil && pkg.Module.Main {
		return classApp
	}

	// A dependency module (module cache or a `replace`).
	if pkg.Module != nil {
		return classThirdParty
	}

	// No module and not under GOROOT — unexpected in module mode; leave it referenced-only.
	return classSkip
}

// partition classifies every closure package and adds the convert-set to the graph, printing a
// one-line census. The convert-set is the app + third-party packages, or — under -recurse=module —
// the app's packages alone, with the third-party packages recorded as referenced-only.
func (m *ModuleConverter) partition(closure map[string]*packages.Package) {
	var appCount, thirdPartyCount, stdlibCount, skipCount int

	for pkgPath, pkg := range closure {
		switch m.classify(pkg) {
		case classApp:
			m.graph.AddPackage(pkgPath, pkg.Dir)
			// Record the app's module path so outputDirFor / reference emission can route the app's
			// own packages to src\ and every dependency to pkg\ (all app packages share this module).
			if pkg.Module != nil {
				m.options.mainModulePath = pkg.Module.Path
			}
			appCount++
		case classThirdParty:
			thirdPartyCount++

			// -recurse=module: keep the dependency OUT of the convert-set. Its references are still
			// emitted (getRecurseDependencyInfo routes it to pkg\<import-path> from the import path
			// alone, not from anything converted), so converting it later into the same output root
			// resolves them — but nothing about it can fail this run.
			if m.options.moduleOnly {
				m.referencedThirdParty = append(m.referencedThirdParty, pkgPath)
				continue
			}

			m.graph.AddPackage(pkgPath, pkg.Dir)
		case classStdLib:
			stdlibCount++
		case classSkip:
			skipCount++
		}
	}

	if m.options.moduleOnly {
		fmt.Printf("Closure: %d packages discovered — converting %d app, referencing %d third-party + %d stdlib (%d skipped)\n",
			len(closure), appCount, thirdPartyCount, stdlibCount, skipCount)
		return
	}

	fmt.Printf("Closure: %d packages discovered — converting %d app + %d third-party, referencing %d stdlib (%d skipped)\n",
		len(closure), appCount, thirdPartyCount, stdlibCount, skipCount)
}

// reportUnconvertedDependencies lists the third-party packages -recurse=module referenced without
// converting. Their generated project references point into the output root's pkg\ tree, where
// nothing has been written yet, so the emitted solution does not build until those packages are
// converted there — a deliberate trade the mode makes, printed rather than left to be discovered.
func (m *ModuleConverter) reportUnconvertedDependencies() {
	if len(m.referencedThirdParty) == 0 {
		return
	}

	sort.Strings(m.referencedThirdParty)

	const listLimit = 25

	fmt.Printf("\nThird-party packages referenced but NOT converted (-recurse=module): %d\n", len(m.referencedThirdParty))

	for i, pkgPath := range m.referencedThirdParty {
		if i == listLimit {
			fmt.Printf("  ... and %d more\n", len(m.referencedThirdParty)-listLimit)
			break
		}

		fmt.Printf("  %s\n", pkgPath)
	}

	fmt.Printf("Each is referenced as %s, so the converted projects will not build until those packages\n"+
		"are converted into the same output root (or the module is re-converted without =module).\n",
		filepath.Join(m.recurseRoot(), "pkg", "<import-path>", "<name>.csproj"))
}

// buildEdges records intra-convert-set dependency edges for every convert-set package. Imports to
// the standard library (not in the convert-set) are filtered out by the graph, so they never
// constrain the order.
func (m *ModuleConverter) buildEdges(closure map[string]*packages.Package) {
	for pkgPath, pkg := range closure {
		if !m.graph.Contains(pkgPath) {
			continue
		}

		imports := make([]string, 0, len(pkg.Imports))

		for importPath := range pkg.Imports {
			imports = append(imports, importPath)
		}

		m.graph.addImportEdges(pkgPath, imports)
	}
}

// recurseRoot returns the writable root for recursively generated app/dependency projects. It falls
// back to go2csPath for callers constructed programmatically and for the established one-positional
// CLI form, preserving the original layout.
func (m *ModuleConverter) recurseRoot() string {
	if m.options.recurseOutputRoot != "" {
		return m.options.recurseOutputRoot
	}

	return m.options.go2csPath
}

// outputDirFor returns where a convert-set package's C# output is written, in a parallel tree under
// the recurse output root that keeps the original Go source pure (nothing is written in place). The
// app's own packages (the main module) go to <output>\src\<import-path>; every dependency —
// module-cache or a co-located `replace` — goes to <output>\pkg\<import-path>. The path is built from
// the version-free import path, so a module-cache dep's @version segment is dropped, and it matches
// the reference getRecurseDependencyInfo emits, so reference and output agree.
func (m *ModuleConverter) outputDirFor(pkgPath string) string {
	root := "pkg"

	if isMainModulePackage(pkgPath, m.options.mainModulePath) {
		root = "src"
	}

	return filepath.Join(m.recurseRoot(), root, filepath.FromSlash(pkgPath))
}

// convertAll converts every convert-set package in the sorted (least-dependencies-first) order,
// surviving a panic OR a load failure in any single package so the rest still convert (partial
// compile acceptable) — one unloadable dependency must not discard the packages queued behind it.
func (m *ModuleConverter) convertAll() {
	total := len(m.graph.sortedQueue)
	fmt.Printf("Converting %d packages in dependency order...\n", total)

	var failed []string

	// Wall time per package, for the slowest-packages summary at the end of the run. A recurse
	// conversion prints one line per package over hundreds or thousands of them, so a single
	// pathological package is invisible in the scroll: issue #33's bsoncodec was identified only
	// because the run never got PAST it. This makes an outlier legible afterwards even when the run
	// completes — which is how a superlinearity gets caught while it is still merely slow.
	durations := make([]packageDuration, 0, total)

	for i, pkgPath := range m.graph.sortedQueue {
		pkg := m.graph.packages[pkgPath]
		outputDir := m.outputDirFor(pkgPath)

		// Printed BEFORE the conversion and terminated immediately: when a package does not finish,
		// this line is the only record of which one, and it is what the issue-#33 reporter read the
		// failing package off. Do not defer it to append a duration — a dangling line interleaves
		// with the stderr warnings a conversion emits whenever the two streams are merged.
		fmt.Printf("[%d/%d] Converting %s\n", i+1, total, pkgPath)

		start := time.Now()

		err := func() (err error) {
			defer func() {
				if r := recover(); r != nil {
					err = fmt.Errorf("panic converting %s: %v", pkgPath, r)
				}
			}()

			// Name the package being converted, so a module-cache load can be issued by IMPORT PATH
			// from the main module rather than by directory from inside the cache (processConversion).
			packageOptions := m.options
			packageOptions.packageImportPath = pkgPath

			return processConversion(pkg.Dir, true, outputDir, packageOptions)
		}()

		// Recorded for FAILED packages too: one that burns minutes before failing is exactly the row
		// worth seeing, and the error path below skips the rest of this iteration.
		durations = append(durations, packageDuration{path: pkgPath, elapsed: time.Since(start)})

		if err != nil {
			log.Printf("WARNING: %v", err)
			failed = append(failed, pkgPath)
			continue
		}

		// Record the generated project so the solution can list it. processConversion names the
		// csproj after getProjectName(pkg.Dir) (the dotted module/import path) in the output dir,
		// through the same projectFileBaseName budget, so solution entry and emitted file agree.
		projectName, _ := getProjectName(pkg.Dir, m.options)
		csproj := filepath.Join(outputDir, projectFileBaseName(projectName)+".csproj")
		m.convertedProjects = append(m.convertedProjects, csproj)
		m.convertedCsproj[pkgPath] = csproj
	}

	fmt.Printf("\nRecursive conversion complete in %s: %d/%d packages converted",
		formatDuration(time.Since(m.startTime)), total-len(failed), total)

	if len(failed) > 0 {
		fmt.Printf(" (%d failed: %s)", len(failed), strings.Join(failed, ", "))
	}

	fmt.Println()

	reportSlowestPackages(durations)
}

// packageDuration pairs a package's import path with its wall-clock conversion time.
type packageDuration struct {
	path    string
	elapsed time.Duration
}

// slowestPackageCount is how many rows reportSlowestPackages prints. Enough to show a cluster rather
// than a single outlier — a whole SHAPE of package being slow reads differently from one bad one.
const slowestPackageCount = 10

// reportSlowestPackages prints the longest per-package conversions of the run, slowest first.
//
// It answers two standing questions with a number instead of an assertion. First, whether any single
// package is anomalous: conversion averages well under a second per package for the standard library
// and a few seconds under -recurse, so a package an order of magnitude off that is worth a look
// before it becomes an issue report. Second, where a recurse run actually spends its time — every
// third-party package is re-loaded through go/packages individually, which is suspected to dominate;
// this is the measurement that would settle it.
func reportSlowestPackages(durations []packageDuration) {
	if len(durations) < 2 {
		return
	}

	slowest := make([]packageDuration, len(durations))
	copy(slowest, durations)

	sort.SliceStable(slowest, func(i, j int) bool {
		return slowest[i].elapsed > slowest[j].elapsed
	})

	count := min(slowestPackageCount, len(slowest))

	fmt.Printf("\nSlowest %d of %d packages:\n", count, len(slowest))

	for _, entry := range slowest[:count] {
		fmt.Printf("  %8s  %s\n", formatDuration(entry.elapsed), entry.path)
	}
}

// transitiveConvertedDeps returns the import paths of pkgPath's transitive dependencies within the
// convert-set (the app + third-party graph), in deterministic (sorted) order. Standard-library
// imports are not graph nodes, so they are naturally excluded.
func (m *ModuleConverter) transitiveConvertedDeps(pkgPath string) []string {
	seen := make(map[string]bool)

	var visit func(p string)

	visit = func(p string) {
		pkg, ok := m.graph.packages[p]

		if !ok {
			return
		}

		for _, dep := range pkg.Dependencies {
			if !seen[dep] {
				seen[dep] = true
				visit(dep)
			}
		}
	}

	visit(pkgPath)

	deps := make([]string, 0, len(seen))

	for dep := range seen {
		deps = append(deps, dep)
	}

	sort.Strings(deps)

	return deps
}

// relSolutionPath renders target as a forward-slash path relative to baseDir (the solution's own
// directory), falling back to the absolute path if no relative one can be formed (e.g. a different
// drive on Windows).
func relSolutionPath(baseDir, target string) string {
	if rel, err := filepath.Rel(baseDir, target); err == nil {
		return filepath.ToSlash(rel)
	}

	return filepath.ToSlash(target)
}

// generatePerProjectSolutions writes, next to each converted project's .csproj, a sibling .slnx over
// that project plus its transitive converted dependencies and the shared golib runtime and go2cs-gen
// analyzer. The standard library is referenced (via $(go2csPath)core), not listed. Building that one
// solution builds the project and everything it needs from the convert-set — so the app (whose solution
// lists its whole converted dependency closure) or any single package can be opened/built on its own.
// The solution's own (anchor) project is marked the VS default startup project. Projects are grouped into
// solution folders mirroring the %GOPATH% layout — the app's own (main-module) packages under src, their
// dependency packages under pkg, and the go2cs runtime/analyzer under core — emitted in that enforced
// order. Project paths are relative to the .slnx's own directory; the runtime/analyzer live under the
// deploy root ($(go2csPath)core\golib, $(go2csPath)gen\go2cs-gen).
func (m *ModuleConverter) generatePerProjectSolutions() {
	golibCsproj := filepath.Join(m.options.go2csPath, "core", "golib", "golib.csproj")
	genCsproj := filepath.Join(m.options.go2csPath, "gen", "go2cs-gen", "go2cs-gen.csproj")

	written := 0

	for _, pkgPath := range m.graph.sortedQueue {
		csproj, ok := m.convertedCsproj[pkgPath]

		if !ok {
			continue // this package failed to convert — no project to anchor a solution on
		}

		projectDir := filepath.Dir(csproj)

		// Group the anchor project + every transitive converted dependency by %GOPATH% root: the app's
		// own (main-module) packages under src, their dependency packages under pkg. golib + the analyzer
		// are the go2cs runtime under core. The standard library is referenced (via $(go2csPath)core),
		// not listed. Classify by import path (the same rule that routed each package's output), so this
		// agrees with the src\/pkg\ output tree regardless of the .slnx-relative path shape.
		var srcProjects, pkgProjects []string

		for _, importPath := range append([]string{pkgPath}, m.transitiveConvertedDeps(pkgPath)...) {
			memberCsproj, ok := m.convertedCsproj[importPath]

			if !ok {
				continue // this dependency failed to convert — nothing to list
			}

			rel := relSolutionPath(projectDir, memberCsproj)

			if isMainModulePackage(importPath, m.options.mainModulePath) {
				srcProjects = append(srcProjects, rel)
			} else {
				pkgProjects = append(pkgProjects, rel)
			}
		}

		// Under -recurse=nuget golib and the analyzer are NuGet packages (go.lib / go.gen), not local
		// projects, so they are omitted from the solution — the /core/ folder then has no members and is
		// skipped by buildRecurseSolutionXML's empty-folder guard.
		var coreProjects []string

		if !m.options.nugetRefs {
			coreProjects = []string{
				relSolutionPath(projectDir, golibCsproj),
				relSolutionPath(projectDir, genCsproj),
			}
		}

		sort.Strings(srcProjects)
		sort.Strings(pkgProjects)
		sort.Strings(coreProjects)

		// Enforced folder order — src, then pkg, then core — deliberately NOT alphabetic.
		folders := []solutionFolder{
			{name: "/src/", projects: srcProjects},
			{name: "/pkg/", projects: pkgProjects},
			{name: "/core/", projects: coreProjects},
		}

		// The solution's own project is its VS default startup project (for the app, the runnable exe).
		anchorRel := relSolutionPath(projectDir, csproj)

		contents := buildRecurseSolutionXML(folders, anchorRel)
		slnxFile := filepath.Join(projectDir, strings.TrimSuffix(filepath.Base(csproj), ".csproj")+".slnx")

		if needToWriteFile(slnxFile, []byte(contents)) {
			if err := os.WriteFile(slnxFile, []byte(contents), 0644); err != nil {
				fmt.Printf("WARNING: failed to write per-project solution %q: %v\n", slnxFile, err)
				continue
			}
		}

		written++
	}

	if written > 0 {
		fmt.Printf("Per-project solutions generated: %d (one .slnx next to each converted .csproj)\n", written)
	}
}

// generatedBuildFileMarker identifies a Directory.Build.props/.targets this converter wrote, so a
// re-conversion refreshes its own file while a user-authored or deploy-core one is left alone.
const generatedBuildFileMarker = "<!-- Generated by go2cs -recurse. -->"

// generateRecurseBuildFiles writes the output root's Directory.Build.props and Directory.Build.targets.
//
// Their job is to bound every DERIVED path. The converted tree mirrors Go import paths, which are
// unbounded, and MSBuild then hangs its own artifacts beneath each project: obj\Debug\net9.0\<AssemblyName>.*,
// bin\Debug\net9.0\, and the generator output EmitCompilerGeneratedFiles writes to Generated\. Those
// multiply the import path rather than merely repeating it — measured on issue #35's reporter's tree,
// 1,570 of 1,725 projects produced a generator path over MAX_PATH and 22 an obj path over it, at an
// output root of only 28 characters. Redirecting the three roots to one short, flat location keeps them
// all inside the limit no matter how deep the import path runs, and costs nothing in identity: what
// moves is regenerated on every build and git-ignored.
//
// The per-project token is a hash rather than the project name deliberately. .NET's built-in
// UseArtifactsOutput/ArtifactsPath was evaluated first and does not fit: it lays artifacts out under
// $(ArtifactsPath)\obj\$(MSBuildProjectName)\<config>\, and the project name IS the long thing here —
// measured at a 13-character output root, the deepest package still landed a 309-character artifact.
// A 12-hex-character hash of the project's output-root-relative directory is short, stable across
// machines (the key is relative, so it does not move with the root), and collision-free in practice
// (48 bits against a few thousand projects).
//
// Under local project references (the default), the props also defaults go2csPath — the MSBuild
// property every emitted $(go2csPath)core\... / golib / analyzer reference composes from — to the
// runtime root this conversion resolved against. The -go2cspath COMMAND-LINE flag is a
// conversion-time input only, and before issue #36 its value never reached the emitted build
// files: an isolated output root fell back to the csproj template's $(USERPROFILE)/go2cs/ default
// and no stdlib reference resolved. When the output root IS the runtime root the pin stays
// relative ($(MSBuildThisFileDirectory), the deploy-core form, so the tree can move as a unit);
// an isolated output root pins the absolute resolved root, which a re-conversion refreshes. The
// Condition keeps it a default: an env var, a -p:go2csPath global, or a higher props still wins.
//
// Under -recurse=nuget the props instead defaults GoStdLibVersion — the version every emitted go.*
// PackageReference resolves — to the converter's Go release, floating on the NuGet revision (e.g.
// 1.23.1.*), so a freshly converted project restores from nuget.org with no extra configuration. A
// consumer value (editing this file, a higher Directory.Build.props, or a -p:GoStdLibVersion global)
// overrides it via the Condition to pin exactly.
//
// Generated-file output has to be redirected from the .TARGETS rather than the .props, because the
// csproj TEMPLATE sets CompilerGeneratedFilesOutputPath in the project body and a props import loses to
// it. Directory.Build.targets is imported after the body, so it wins — and the template is left byte-for
// byte unchanged, which is what keeps the standard library and the behavioral corpus at zero movement.
func (m *ModuleConverter) generateRecurseBuildFiles() {
	root := m.recurseRoot()

	propsLines := []string{
		"<Project>",
		"",
		"  " + generatedBuildFileMarker,
		"",
		"  <!-- Derived build output (obj, bin) is redirected to one short, flat location. The converted",
		"       tree mirrors Go import paths, which are unbounded, and Windows caps a path at 260",
		"       characters; keeping MSBuild's artifacts out of the deep tree is what makes a deeply",
		"       nested package buildable. The per-project token is a hash of the project's location",
		"       relative to this file, so it is short, stable, and independent of where the tree sits. -->",
		"  <PropertyGroup Condition=\"'$(Go2csArtifactsRoot)' == ''\">",
		"    <Go2csArtifactsRoot>$(MSBuildThisFileDirectory).artifacts\\</Go2csArtifactsRoot>",
		"  </PropertyGroup>",
		"",
		"  <PropertyGroup Condition=\"'$(Go2csArtifactToken)' == ''\">",
		"    <Go2csProjectKey>$([MSBuild]::MakeRelative($(MSBuildThisFileDirectory), $(MSBuildProjectDirectory)))</Go2csProjectKey>",
		"    <Go2csProjectHash>$([MSBuild]::StableStringHash($(Go2csProjectKey), 'Sha256'))</Go2csProjectHash>",
		"    <Go2csArtifactToken>$(Go2csProjectHash.Substring(0, 12))</Go2csArtifactToken>",
		"  </PropertyGroup>",
		"",
		"  <PropertyGroup>",
		"    <BaseIntermediateOutputPath>$(Go2csArtifactsRoot)obj\\$(Go2csArtifactToken)\\</BaseIntermediateOutputPath>",
		"    <BaseOutputPath>$(Go2csArtifactsRoot)bin\\$(Go2csArtifactToken)\\</BaseOutputPath>",
		"  </PropertyGroup>",
	}

	// Under local project references, default go2csPath to the runtime root this conversion resolved
	// its $(go2csPath)core\... / golib / analyzer references against (issue #36: the -go2cspath flag
	// is a conversion-time input, so without this pin an isolated output root fell back to the csproj
	// template's $(USERPROFILE)/go2cs/ default and nothing resolved). Relative when the output root IS
	// the runtime root, absolute otherwise; the Condition keeps it an overridable default.
	if !m.options.nugetRefs {
		pin := "$(MSBuildThisFileDirectory)"

		if !samePath(root, m.options.go2csPath) {
			runtimeRoot := m.options.go2csPath

			if abs, err := filepath.Abs(runtimeRoot); err == nil {
				runtimeRoot = abs
			}

			pin = filepath.ToSlash(runtimeRoot) + "/"
		}

		propsLines = append(propsLines,
			"",
			"  <!-- The go2cs runtime root: every stdlib/runtime/analyzer ProjectReference in the converted",
			"       projects composes from $(go2csPath), e.g. $(go2csPath)core/fmt/fmt.csproj. Defaulted to",
			"       the root this conversion resolved against (-go2cspath). Override by setting a go2csPath",
			"       environment variable or a -p:go2csPath build global, or by editing this value. -->",
			"  <PropertyGroup Condition=\"'$(go2csPath)' == ''\">",
			fmt.Sprintf("    <go2csPath>%s</go2csPath>", pin),
			"  </PropertyGroup>",
		)
	}

	// Default GoStdLibVersion to the converter's Go release, floating on the NuGet revision. Omitted if the
	// Go version can't be resolved (then a consumer must define GoStdLibVersion for restore to succeed).
	if m.options.nugetRefs {
		if base := goVersion(); base != "" {
			propsLines = append(propsLines,
				"",
				"  <!-- Default the go2cs NuGet package version to the converter's Go release, floating on the",
				"       revision. Pin exactly by setting GoStdLibVersion here (or a -p: global) before build. -->",
				"  <PropertyGroup Condition=\"'$(GoStdLibVersion)' == ''\">",
				fmt.Sprintf("    <GoStdLibVersion>%s.*</GoStdLibVersion>", base),
				"  </PropertyGroup>",
			)
		}
	}

	propsLines = append(propsLines, "", "</Project>", "")

	targetsLines := []string{
		"<Project>",
		"",
		"  " + generatedBuildFileMarker,
		"",
		"  <!-- Source-generator output follows obj and bin out of the deep tree. This has to live in the",
		"       .targets rather than the .props: the generated .csproj sets CompilerGeneratedFilesOutputPath",
		"       in the project body, which beats any .props value, and .targets is imported after the body.",
		"       NOTE the deliberate absence of a trailing separator, unlike the two paths above: this value",
		"       reaches csc as /generatedfilesout:\"<path>\", and a trailing backslash there escapes the",
		"       closing quote, so the compiler is handed the directory as a FILE name and fails CS2021. -->",
		"  <PropertyGroup Condition=\"'$(EmitCompilerGeneratedFiles)' == 'true'\">",
		"    <CompilerGeneratedFilesOutputPath>$(Go2csArtifactsRoot)gen\\$(Go2csArtifactToken)</CompilerGeneratedFilesOutputPath>",
		"  </PropertyGroup>",
		"",
		"</Project>",
		"",
	}

	m.writeRecurseBuildFile(filepath.Join(root, "Directory.Build.props"), propsLines)
	m.writeRecurseBuildFile(filepath.Join(root, "Directory.Build.targets"), targetsLines)
}

// writeRecurseBuildFile writes one generated build file, refusing to clobber a FOREIGN one. The
// -recurse output root defaults to the deploy root when no second positional argument is given, and
// deploy-core writes its own Directory.Build.props there; a user may equally have authored one. Either
// is left in place — but loudly, because the artifact redirection it carries is what keeps a deep tree
// buildable, and silently skipping it would surface much later as an unexplained MAX_PATH failure.
func (m *ModuleConverter) writeRecurseBuildFile(path string, lines []string) {
	if existing, err := os.ReadFile(path); err == nil && !strings.Contains(string(existing), generatedBuildFileMarker) {
		fmt.Printf("WARNING: %s already exists and was not written by go2cs — left as is.\n", path)
		fmt.Printf("         Converted projects will keep their obj/bin/Generated output in the package tree;\n")
		fmt.Printf("         a deep import path can then exceed Windows' 260-character path limit. Convert to a\n")
		fmt.Printf("         separate output root, or merge the go2cs artifact-redirection block by hand.\n")
		return
	}

	contents := []byte(strings.Join(lines, "\r\n"))

	if !needToWriteFile(path, contents) {
		return
	}

	if err := os.WriteFile(path, contents, 0644); err != nil {
		fmt.Printf("WARNING: failed to write %q: %v\n", path, err)
		return
	}

	fmt.Printf("Build file generated: %s\n", path)
}
