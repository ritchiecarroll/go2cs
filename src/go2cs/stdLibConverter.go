// stdLibConverter.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// This file contains extensions to the go2cs project for handling standard library conversion
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

// StdLibConverter manages the conversion of the Go standard library. The package dependency
// graph + topological sort live in the shared DependencyGraph (dependencyGraph.go); this driver
// discovers the "std" convert-set and each package's imports, then delegates ordering to it.
type StdLibConverter struct {
	goRoot     string           // Path to Go root directory
	goPath     string           // Path to Go path directory
	go2csPath  string           // Path to output directory
	options    Options          // Conversion options
	graph      *DependencyGraph // Shared package dependency graph + conversion order
	startTime  time.Time        // Start time for reporting progress
	totalCount int              // Total number of packages
}

// NewStdLibConverter creates a new standard library converter
func NewStdLibConverter(options Options) *StdLibConverter {
	return &StdLibConverter{
		goRoot:    options.goRoot,
		goPath:    options.goPath,
		go2csPath: options.go2csPath,
		options:   options,
		graph:     NewDependencyGraph(),
		startTime: time.Now(),
	}
}

// ScanAndConvertFiltered converts the Go standard library, optionally narrowed to the named
// packages. A nil or empty packageFilter converts everything (`go2cs -stdlib`); a non-empty one
// converts just those import paths and their prerequisites (`go2cs -stdlib fmt strings io`).
func (c *StdLibConverter) ScanAndConvertFiltered(packageFilter []string) error {
	// Announce the effective build tags so the purego default is visible, not magic. A bare
	// `-stdlib` run reports `purego (default)`; an explicit `-tags` reports exactly what was passed.
	if len(c.options.buildTags) > 0 {
		suffix := ""
		if !c.options.tagsExplicit {
			suffix = " (default; pass -tags to override)"
		}
		fmt.Printf("Applying build tags: %s%s\n", strings.Join(c.options.buildTags, ","), suffix)
	} else {
		fmt.Println("Applying build tags: none (-tags= cleared the purego default)")
	}

	// Step 1: Scan all standard library packages
	fmt.Println("Scanning Go standard library packages...")
	if err := c.scanStdLib(); err != nil {
		return fmt.Errorf("failed to scan standard library: %w", err)
	}

	// Step 2: Build dependency graph and sort
	fmt.Println("Building dependency graph...")
	if err := c.buildDependencyGraph(); err != nil {
		return fmt.Errorf("failed to build dependency graph: %w", err)
	}

	// Generate dependency graph visualization
	if err := c.GenerateDependencyGraph(); err != nil {
		fmt.Printf("WARNING: Failed to generate dependency graph: %v\n", err)
	}

	// Step 3: Perform topological sort
	fmt.Println("Sorting packages by dependencies...")
	c.topologicalSort()

	// Expose the completed convert-set graph so linkname var-pull forwarding can reject a pull whose
	// project reference would cycle (see linknamePullWouldCycle). This is the CHEAPEST of the guard's
	// three oracles, not its only one: a single-package or -tests conversion leaves the graph nil and
	// answers the same question from a targeted go/packages load of the pull target. This comment used
	// to claim that "only the -stdlib graph can form such a cross-package cycle" — that was W1, and it
	// was false. A linkname edge is the one reference the converter emits that Go's import graph does
	// not contain, so the danger comes from the DIRECTIVE, which a lone package carries just as well.
	conversionGraph = c.graph

	// Generate conversion report
	if err := c.GenerateConversionReport(); err != nil {
		fmt.Printf("WARNING: Failed to generate conversion report: %v\n", err)
	}

	// Apply filter if provided
	if len(packageFilter) > 0 {
		// Create a map for O(1) lookups
		filterMap := make(map[string]bool)
		for _, pkg := range packageFilter {
			filterMap[pkg] = true
		}

		// Filter the queue
		filteredQueue := make([]string, 0)
		for _, pkg := range c.graph.sortedQueue {
			if filterMap[pkg] {
				filteredQueue = append(filteredQueue, pkg)
			}
		}

		if len(filteredQueue) == 0 {
			return fmt.Errorf("no matching packages found after filtering")
		}

		fmt.Printf("Filtered from %d to %d packages based on filter list\n",
			len(c.graph.sortedQueue), len(filteredQueue))
		c.graph.sortedQueue = filteredQueue
	}

	// Check for previously converted packages
	if err := c.checkForPreviousConversion(); err != nil {
		fmt.Printf("WARNING: Failed to check for previously converted packages: %v\n", err)
	}

	// Step 4: Convert all packages
	c.totalCount = len(c.graph.sortedQueue)
	fmt.Printf("Beginning conversion of %d packages...\n", c.totalCount)
	if err := c.convertAllPackages(); err != nil {
		return fmt.Errorf("error during package conversion: %w", err)
	}

	// Step 5: Emit a Visual Studio .slnx solution referencing every converted project, the
	// shared golib runtime, and the go2cs-gen analyzer, so the freshly converted stdlib is
	// openable / buildable as one unit.
	if err := c.GenerateSolutionFile(); err != nil {
		fmt.Printf("WARNING: Failed to generate solution file: %v\n", err)
	}

	// Step 6: Copy the Go source tree's root attribution/reference files (VERSION, LICENSE,
	// PATENTS, README.md, SECURITY.md, CONTRIBUTING.md) alongside the converted packages. The
	// generated C# is a derivative work: the per-file "// Copyright … The Go Authors … BSD-style
	// license" headers reference these root files ("as found in the LICENSE file"), so preserving
	// them keeps the output properly attributed and records the exact Go toolchain it came from.
	if err := c.copyRootAttributionFiles(); err != nil {
		fmt.Printf("WARNING: Failed to copy root attribution files: %v\n", err)
	}

	return nil
}

// rootAttributionFiles are the non-source files at the root of the Go source tree that the
// converted output must carry over for attribution/reference. Order is for readability only.
var rootAttributionFiles = []string{
	"VERSION",
	"LICENSE",
	"PATENTS",
	"README.md",
	"SECURITY.md",
	"CONTRIBUTING.md",
}

// copyRootAttributionFiles copies each rootAttributionFiles entry from the Go source root into the
// converted-output "core" directory — the same root the converted packages sit under, so these land
// at core/VERSION, core/LICENSE and so on. Missing or unreadable files are reported and skipped
// rather than failing the whole conversion.
func (c *StdLibConverter) copyRootAttributionFiles() error {
	destDir := filepath.Join(c.go2csPath, "core")
	if err := os.MkdirAll(destDir, 0755); err != nil {
		return fmt.Errorf("failed to create output directory %s: %w", destDir, err)
	}

	copied := 0
	for _, name := range rootAttributionFiles {
		srcPath := filepath.Join(c.goRoot, name)

		data, err := os.ReadFile(srcPath)
		if err != nil {
			// Not every Go version ships every file; skip what isn't there.
			fmt.Printf("WARNING: Skipping root file %s: %v\n", name, err)
			continue
		}

		// The shared LICENSE also covers the project-owned BSD-licensed hand-owns beside the
		// converted packages, so the go2cs Authors are added as a copyright holder on its second
		// line -- deterministically, HERE, so a regeneration reproduces the committed file instead
		// of reverting a hand edit (licensing.go, withGo2csCopyright).
		if name == "LICENSE" {
			data = withGo2csCopyright(data)
		}

		destPath := filepath.Join(destDir, name)
		if err := os.WriteFile(destPath, data, 0644); err != nil {
			fmt.Printf("WARNING: Failed to write %s: %v\n", destPath, err)
			continue
		}

		copied++
	}

	fmt.Printf("Copied %d of %d root attribution files into %s\n", copied, len(rootAttributionFiles), destDir)
	return nil
}

// isNonConvertedStdLibPackage reports whether a standard-library import path is deliberately kept
// OUT of the conversion queue.
//
//   - `unsafe` and `builtin` are compiler intrinsics with no convertible source.
//   - `testing` is ENTIRELY HAND-OWNED (src/core/testing). Go's implementation is a state machine
//     over the runtime's goroutine scheduler, and the converted-test host that stands in for it is
//     go2cs machinery, not a transcription. Converting it would write a second
//     [GoPackage("testing")] `testing_package` into the very tree the host lives in — the F15b
//     "ONE testing package, period" collision (CS0433 on every testing type, reached via
//     internal/testenv). Its SUBPACKAGES (testing/fstest, testing/iotest, testing/quick,
//     testing/slogtest, testing/internal/testdeps) convert normally.
//   - `cmd` and everything under it is the Go toolchain, not the library.
//
// A skipped package is still recovered for the generated solution (and therefore for NuGet): its
// converted dependents emit a `$(go2csPath)core\<pkg>` ProjectReference to it, which
// collectConvertedProjects picks up — see solutionGenerator.go.
//
// The set is a package-level VALUE rather than a switch's inline literal because a SECOND reader now
// depends on it. src\reconvert-deletions.ps1 — the H5 deletion pass — classifies a seeded file as a
// deletion candidate only if the converter emits into its directory, and `go list std` names
// `unsafe` and `testing` like any other package, so no Go question can exclude them: this predicate
// is the only authority, and the script carries a mirror of it. reconvertDeletionsSkipList_test.go
// compares the two SETS (not two spellings) under the plain `go test ./...`, so the mirror cannot
// drift silently — a converter that stopped emitting a package while the script still believed it
// did would offer that package's hand-owned files for deletion.
var (
	nonConvertedStdLibPackages = []string{"unsafe", "builtin", "testing", "cmd"}
	nonConvertedStdLibPrefixes = []string{"cmd/"}
)

func isNonConvertedStdLibPackage(importPath string) bool {
	for _, skipped := range nonConvertedStdLibPackages {
		if importPath == skipped {
			return true
		}
	}

	for _, prefix := range nonConvertedStdLibPrefixes {
		if strings.HasPrefix(importPath, prefix) {
			return true
		}
	}

	return false
}

// stdLibLoadConfig builds the go/packages configuration the standard-library driver loads through:
// GOPATH mode (GO111MODULE=off) rooted at dir, carrying the run's build tags and its target platform.
//
// It is ONE function rather than the two identical literals it replaces so that the standing canary
// in stdNamespaceGuard_test.go loads through the configuration the driver ACTUALLY uses instead of a
// transcription of it. A canary that copies the thing it guards stops guarding it at the first edit
// to either copy, and what this one watches for — a returned PkgPath carrying a `std/` prefix instead
// of the bare import path — is precisely a property of the loader's environment, so the environment
// is the part that must not drift between the two.
func stdLibLoadConfig(options Options, mode packages.LoadMode, dir string) *packages.Config {
	loadConfig := &packages.Config{
		Mode:       mode,
		Dir:        dir,
		BuildFlags: options.loaderBuildFlags(),
		Env:        append(os.Environ(), "GO111MODULE=off"), // Disable module mode
	}

	// Set the target platform in the environment if specified
	if options.targetPlatform != "" {
		targetParts := strings.Split(options.targetPlatform, "/")

		if len(targetParts) == 2 {
			loadConfig.Env = append(loadConfig.Env, fmt.Sprintf("GOOS=%s", targetParts[0]))
			loadConfig.Env = append(loadConfig.Env, fmt.Sprintf("GOARCH=%s", targetParts[1]))
		}
	}

	return loadConfig
}

// scanStdLib scans all standard library packages
func (c *StdLibConverter) scanStdLib() error {
	// Scan the src directory in GOROOT
	srcPath := filepath.Join(c.goRoot, "src")

	// Check if the directory exists
	if _, err := os.Stat(srcPath); os.IsNotExist(err) {
		return fmt.Errorf("standard library source not found at %s", srcPath)
	}

	// First, get the list of standard library packages
	loadConfig := stdLibLoadConfig(c.options, packages.NeedName|packages.NeedImports|packages.NeedDeps|packages.NeedFiles, srcPath)

	fmt.Println("Loading standard library packages (this may take a while)...")

	// Load "std" pattern to get all standard library packages
	pkgs, err := packages.Load(loadConfig, "std")

	if err != nil {
		return fmt.Errorf("failed to load standard library packages: %w", err)
	}

	fmt.Printf("Found %d standard library packages\n", len(pkgs))

	// Process each package
	for _, pkg := range pkgs {
		// Skip test packages
		if strings.HasSuffix(pkg.PkgPath, "_test") {
			continue
		}

		// Skip certain special packages that don't need conversion
		if isNonConvertedStdLibPackage(pkg.PkgPath) {
			continue
		}

		// Create new package object
		c.graph.AddPackage(pkg.PkgPath, pkg.Dir)
	}

	return nil
}

// buildDependencyGraph loads each standard-library package's imports and records the intra-set
// dependency edges in the shared graph (which filters to the convert-set and resolves
// GOROOT-vendored keys), then sorts adjacency for deterministic ordering.
//
// A package that fails to load ABORTS the run, and every such package is reported before the abort
// so one run names them all rather than surfacing them one at a time.
//
// Skipping a failed load instead — what this did until the question was settled — is a false-green
// generator, not a tolerance. The skip drops only the package's dependency EDGES; the package
// itself stays in the convert set, so the topological sort treats it as having no prerequisites and
// converts it BEFORE the packages it imports. Their package_info.cs does not exist yet, so its
// imported type aliases resolve to nothing and it emits raw type names — output that may well
// compile — while the summary still prints "Successfully converted: 304 (100.0%)". A load failure
// means the converter does not know what this package imports, and there is no correct order to
// convert it in.
//
// Worse, the shape a load failure ACTUALLY takes was not caught at all: only the two rare shapes
// (a loader error, no packages matched) were reported, while the ordinary one — a package returned
// with Errors and no imports — fell straight through to the edge recording, so the mis-ordering
// happened without even a warning. See the third check below.
//
// This costs nothing on a healthy corpus: a full `-stdlib` run produces zero load failures, and so
// does a cross-platform one (`-platforms linux/amd64`, 302 packages). If a future target does
// trigger it, the failure is real information about that target, and the message says which
// packages and which platform.
func (c *StdLibConverter) buildDependencyGraph() error {
	fmt.Println("Building dependency graph for all packages...")

	// Count to track progress
	total := len(c.graph.packages)
	count := 0

	// Every package that could not be loaded, so the abort below can name all of them at once.
	var loadFailures []string

	// For each package, find all dependencies
	for pkgPath, pkg := range c.graph.packages {
		count++
		if count%20 == 0 || count == total {
			fmt.Printf("\nAnalyzing dependencies: %d/%d packages (%.1f%%)...", count, total, float64(count)/float64(total)*100)
		}

		// Load the package with dependencies
		loadConfig := stdLibLoadConfig(c.options, packages.NeedImports, pkg.Dir)

		pkgs, err := packages.Load(loadConfig, pkgPath)
		if err != nil {
			log.Printf("ERROR: Failed to load package %s: %v", pkgPath, err)
			loadFailures = append(loadFailures, fmt.Sprintf("%s (%v)", pkgPath, err))
			continue
		}

		if len(pkgs) == 0 {
			// packages.Load matched nothing for this import path — the directory scan found it, so
			// this means the loader and the scanner disagree about what exists.
			log.Printf("ERROR: Failed to find package %s", pkgPath)
			loadFailures = append(loadFailures, pkgPath+" (not found by the package loader)")
			continue
		}

		// The two checks above are the RARE shapes. An ordinary failure — the package cannot be
		// found, or its files do not build under these tags — comes back as err == nil and exactly
		// one package carrying Errors and an EMPTY Imports map. Without this check that package is
		// indistinguishable from one that genuinely imports nothing, so it is recorded as a
		// dependency-free root and converted first: the silent mis-ordering described above, with
		// not even a warning to go on.
		if errs := pkgs[0].Errors; len(errs) > 0 {
			log.Printf("ERROR: Package %s did not load cleanly: %v", pkgPath, errs[0])
			loadFailures = append(loadFailures, fmt.Sprintf("%s (%v)", pkgPath, errs[0]))
			continue
		}

		// Record dependency edges to the packages in this conversion set. The graph filters to
		// the convert-set and resolves GOROOT-vendored keys; import order is immaterial (edges are
		// deduped and adjacency is sorted below).
		imports := make([]string, 0, len(pkgs[0].Imports))

		for importPath := range pkgs[0].Imports {
			imports = append(imports, importPath)
		}

		c.graph.addImportEdges(pkgPath, imports)
	}

	if len(loadFailures) > 0 {
		sort.Strings(loadFailures)

		return fmt.Errorf("could not load %d of %d packages for the dependency graph, so there is no "+
			"correct order to convert them in (target platform %s):\n  %s",
			len(loadFailures), total, c.options.targetPlatform, strings.Join(loadFailures, "\n  "))
	}

	fmt.Println("\nDependency analysis complete!")

	// Sort dependencies and dependents for deterministic behavior
	c.graph.sortAdjacency()

	return nil
}

// topologicalSort orders the standard-library convert-set least dependencies first by delegating
// to the shared graph, then reports the resulting order.
//
// This one genuinely has nothing to report, unlike its buildDependencyGraph neighbour: the
// underlying graph sort tolerates an import cycle with a warning rather than failing, and by this
// point every package's edges are known, so the order it produces is always a usable one.
func (c *StdLibConverter) topologicalSort() {
	fmt.Println("Sorting packages in dependency order...")

	c.graph.topologicalSort()

	// Print some statistics
	fmt.Printf("Sorted %d packages in dependency order\n", len(c.graph.sortedQueue))

	// For debugging: print first 5 and last 5 packages in the sorted queue
	if len(c.graph.sortedQueue) > 10 {
		fmt.Println("First packages to convert:")
		for i := 0; i < 5; i++ {
			fmt.Printf("  %s\n", c.graph.sortedQueue[i])
		}
		fmt.Println("Last packages to convert:")
		for i := len(c.graph.sortedQueue) - 5; i < len(c.graph.sortedQueue); i++ {
			fmt.Printf("  %s\n", c.graph.sortedQueue[i])
		}
	}
}

// checkForPreviousConversion checks if a previous conversion attempt was made
func (c *StdLibConverter) checkForPreviousConversion() error {
	// Look for the progress file
	progressFile := filepath.Join(c.go2csPath, "stdlib_conversion_progress.txt")

	if _, err := os.Stat(progressFile); os.IsNotExist(err) {
		// No previous conversion
		return nil
	}

	// Look for the specific failed packages file
	failedFile := filepath.Join(c.go2csPath, "failed_packages.txt")

	if _, err := os.Stat(failedFile); os.IsNotExist(err) {
		// No failed packages file
		fmt.Println("Found previous conversion attempt, but no failed packages file.")
		return nil
	}

	// Read the failed packages file
	data, err := os.ReadFile(failedFile)
	if err != nil {
		return fmt.Errorf("failed to read previous conversion file: %w", err)
	}

	// Parse the failed packages
	lines := strings.Split(string(data), "\n")
	var failedPackages []string

	for _, line := range lines {
		line = strings.TrimSpace(line)
		if line != "" {
			failedPackages = append(failedPackages, line)
		}
	}

	if len(failedPackages) == 0 {
		fmt.Println("Found previous conversion attempt, but no failed packages.")
		return nil
	}

	fmt.Printf("Found previous conversion attempt with %d failed packages.\n", len(failedPackages))

	// Ask the user if they want to resume
	fmt.Print("Do you want to resume converting only the failed packages? (y/n): ")
	var answer string
	fmt.Scanln(&answer)

	if strings.ToLower(answer) == "y" || strings.ToLower(answer) == "yes" {
		// Filter the queue to only include the failed packages
		fmt.Printf("Resuming conversion of %d previously failed packages.\n", len(failedPackages))

		// Create a map for O(1) lookups
		filterMap := make(map[string]bool)
		for _, pkg := range failedPackages {
			filterMap[pkg] = true
		}

		// Filter the queue
		filteredQueue := make([]string, 0)
		for _, pkg := range c.graph.sortedQueue {
			if filterMap[pkg] {
				filteredQueue = append(filteredQueue, pkg)
			}
		}

		if len(filteredQueue) == 0 {
			return fmt.Errorf("no matching packages found after filtering for failed packages")
		}

		fmt.Printf("Filtered from %d to %d packages based on previously failed packages\n",
			len(c.graph.sortedQueue), len(filteredQueue))
		c.graph.sortedQueue = filteredQueue
	} else {
		fmt.Println("Proceeding with full conversion...")
	}

	return nil
}

// convertAllPackages converts all packages in the sorted queue
func (c *StdLibConverter) convertAllPackages() error {
	successCount := 0
	failedPackages := make([]string, 0)

	// Create a benchmark file to track conversion progress
	benchmarkFile := filepath.Join(c.go2csPath, "stdlib_conversion_progress.txt")
	benchmark, err := os.Create(benchmarkFile)
	if err != nil {
		log.Printf("WARNING: Could not create benchmark file: %v", err)
	} else {
		defer benchmark.Close()
		fmt.Fprintf(benchmark, "Go Standard Library Conversion - Started %s\n\n", c.startTime.Format(time.RFC3339))
		fmt.Fprintf(benchmark, "%-45s %-15s %-15s %s\n", "Package", "Status", "Duration", "Timestamp")
		fmt.Fprintf(benchmark, "%s\n", strings.Repeat("-", 103))
	}

	// Convert each package one at a time, in dependency order. Package conversion relies on
	// package-level global state in main.go (initFuncCounter, the temp-var maps, the imported-
	// type-alias parse markers), so a converted importer must observe its dependency's finished
	// package_info.cs — the conversion is inherently sequential. Sequential conversion also keeps
	// the emitted bytes byte-reproducible run-to-run (see the per-file note in processConversion).
	for i, pkgPath := range c.graph.sortedQueue {
		pkg := c.graph.packages[pkgPath]
		pkgStartTime := time.Now()

		// Calculate progress
		progress := float64(i+1) / float64(c.totalCount) * 100
		elapsed := time.Since(c.startTime)

		var totalEstimated time.Duration
		var remaining time.Duration

		if i > 0 {
			totalEstimated = time.Duration(float64(elapsed) / float64(i) * float64(c.totalCount))
			remaining = totalEstimated - elapsed
		}

		fmt.Printf("\n[%d/%d] Converting package %s (%.1f%% complete", i+1, c.totalCount, pkgPath, progress)

		if i > 0 {
			fmt.Printf(", ~%s remaining", formatDuration(remaining))
		}

		fmt.Println(")")

		// Handle dependencies
		if len(pkg.Dependencies) > 0 {
			fmt.Printf("  Dependencies (%d): ", len(pkg.Dependencies))
			if len(pkg.Dependencies) > 5 {
				// Show just the first 5 for brevity
				fmt.Printf("%s... and %d more\n", strings.Join(pkg.Dependencies[:5], ", "),
					len(pkg.Dependencies)-5)
			} else {
				fmt.Printf("%s\n", strings.Join(pkg.Dependencies, ", "))
			}
		}

		// Create output path
		outputPath := c.getOutputPath(pkgPath)

		// Process the conversion with error handling
		var conversionErr error

		func() {
			// Use a panic recovery to catch any unexpected errors
			defer func() {
				if r := recover(); r != nil {
					conversionErr = fmt.Errorf("panic during conversion: %v", r)
				}
			}()

			conversionErr = c.convertPackage(pkg.Dir, outputPath)
		}()

		conversionDuration := time.Since(pkgStartTime)

		if conversionErr != nil {
			msg := fmt.Sprintf("failed to convert package %s: %v", pkgPath, conversionErr)
			log.Println(msg)
			failedPackages = append(failedPackages, pkgPath)

			if benchmark != nil {
				fmt.Fprintf(benchmark, "%-40s %-15s %-15s %s\n",
					pkgPath, "FAILED", formatDuration(conversionDuration),
					time.Now().Format(time.RFC3339))
			}
		} else {
			successCount++
			fmt.Printf("  Completed in %s\n", formatDuration(conversionDuration))

			if benchmark != nil {
				fmt.Fprintf(benchmark, "%-45s %-15s %-15s %s\n",
					pkgPath, "SUCCESS", formatDuration(conversionDuration),
					time.Now().Format(time.RFC3339))
			}
		}
	}

	// Print final statistics
	elapsed := time.Since(c.startTime)
	fmt.Printf("\nConversion Summary:\n")
	fmt.Printf("Total packages: %d\n", c.totalCount)
	fmt.Printf("Successfully converted: %d (%.1f%%)\n", successCount, float64(successCount)/float64(c.totalCount)*100)
	fmt.Printf("Failed: %d (%.1f%%)\n", len(failedPackages), float64(len(failedPackages))/float64(c.totalCount)*100)
	fmt.Printf("Total time: %s\n", formatDuration(elapsed))

	if successCount > 0 {
		fmt.Printf("Average time per successful package: %s\n", formatDuration(elapsed/time.Duration(successCount)))
	}

	// Write completion info to benchmark file
	if benchmark != nil {
		fmt.Fprintf(benchmark, "\nSummary:\n")
		fmt.Fprintf(benchmark, "Total packages: %d\n", c.totalCount)
		fmt.Fprintf(benchmark, "Successfully converted: %d (%.1f%%)\n", successCount, float64(successCount)/float64(c.totalCount)*100)
		fmt.Fprintf(benchmark, "Failed: %d (%.1f%%)\n", len(failedPackages), float64(len(failedPackages))/float64(c.totalCount)*100)
		fmt.Fprintf(benchmark, "Total time: %s\n", formatDuration(elapsed))

		if len(failedPackages) > 0 {
			fmt.Fprintf(benchmark, "\nFailed packages:\n")
			for _, pkg := range failedPackages {
				fmt.Fprintf(benchmark, "  %s\n", pkg)
			}
		}
	}

	// If any packages failed, create a resume file with just the failed packages
	if len(failedPackages) > 0 {
		resumeFile := filepath.Join(c.go2csPath, "failed_packages.txt")
		if file, err := os.Create(resumeFile); err == nil {
			defer file.Close()
			for _, pkg := range failedPackages {
				fmt.Fprintln(file, pkg)
			}
			fmt.Printf("\nList of failed packages written to: %s\n", resumeFile)
		}
	}

	return nil
}

// getOutputPath determines the output path for a package
func (c *StdLibConverter) getOutputPath(pkgPath string) string {
	// For standard library packages, maintain the same structure under go2cspath/core
	return filepath.Join(c.go2csPath, "core", pkgPath)
}

// defaultConvertTimeout is the per-package conversion cap the -stdlib driver applies when
// -convert-timeout is not passed. It is a SAFETY NET against a hung conversion, never a
// performance assumption: the number has to clear the slowest legitimate package on the slowest
// legitimate host, because everything it kills early is reported as a failed package and a
// killed-but-healthy conversion reads exactly like a converter defect. It was hard-coded at this
// value until 2026-09-02, when concurrent lane load on an i7-class machine pushed one package's
// conversion past it mid two-seeded A/B -- which would have banked a whole package as a spurious
// emission difference. Raise it with -convert-timeout rather than editing this constant.
const defaultConvertTimeout = 10 * time.Minute

// validateConvertTimeout rejects a non-positive -convert-timeout, naming the flag. Same fail-fast
// posture as -test-timeout: a zero or negative cap fires the instant the conversion starts, so it
// must die at the command line rather than surface as every package timing out at once.
func validateConvertTimeout(timeout time.Duration) error {
	if timeout <= 0 {
		return fmt.Errorf("-convert-timeout must be greater than zero (got %s)", timeout)
	}

	return nil
}

// packageConvertTimeout is the per-package cap this run applies. A zero value means the Options
// were built in code rather than parsed from the command line (unit tests, an internal driver);
// those take the default instead of firing instantly.
func (c *StdLibConverter) packageConvertTimeout() time.Duration {
	if c.options.convertTimeout > 0 {
		return c.options.convertTimeout
	}

	return defaultConvertTimeout
}

// runPackageConversionWithTimeout runs one package's conversion under the per-package cap,
// recovering a panic in the conversion goroutine as an ordinary error. When the cap fires the
// error names both the elapsed budget and the flag that changes it, so the next reader learns the
// knob from the failure text rather than from this source file.
func runPackageConversionWithTimeout(timeout time.Duration, convert func() error) error {
	// Use a channel to synchronize completion
	done := make(chan error, 1)

	go func() {
		// Run the conversion in a goroutine to allow for timeout handling
		defer func() {
			if r := recover(); r != nil {
				done <- fmt.Errorf("panic in package conversion: %v", r)
			}
		}()

		done <- convert()
	}()

	select {
	case err := <-done:
		return err
	case <-time.After(timeout):
		return fmt.Errorf("conversion timed out after %s (per-package -convert-timeout cap; raise it, e.g. -convert-timeout %s -- the cap is a safety net against a hung conversion, never a performance assumption)",
			timeout, timeout*3)
	}
}

// convertPackage converts a single package
func (c *StdLibConverter) convertPackage(inputPath, outputPath string) error {
	return runPackageConversionWithTimeout(c.packageConvertTimeout(), func() error {
		// Call the existing processConversion function
		return processConversion(inputPath, true, outputPath, c.options)
	})
}

// GenerateDependencyGraph creates a visual representation of the dependency graph
func (c *StdLibConverter) GenerateDependencyGraph() error {
	fmt.Println("Generating dependency graph visualization...")

	// First, ensure graphviz directory exists
	graphDir := filepath.Join(c.go2csPath, "graphs")
	if err := os.MkdirAll(graphDir, 0755); err != nil {
		return fmt.Errorf("failed to create graphs directory: %w", err)
	}

	// Create a DOT file for the dependency graph
	dotFile := filepath.Join(graphDir, "package_dependencies.dot")
	f, err := os.Create(dotFile)
	if err != nil {
		return fmt.Errorf("failed to create DOT file: %w", err)
	}
	defer f.Close()

	// Write DOT file header
	fmt.Fprintln(f, "digraph G {")
	fmt.Fprintln(f, "  rankdir=LR;")
	fmt.Fprintln(f, "  node [shape=box, style=filled, fillcolor=lightblue];")

	// Sort packages for consistent output
	sortedPackages := make([]string, 0, len(c.graph.packages))

	for pkgPath := range c.graph.packages {
		sortedPackages = append(sortedPackages, pkgPath)
	}

	sort.Strings(sortedPackages)

	// Write nodes for each package
	for _, pkgPath := range sortedPackages {
		// Sanitize package name for DOT
		pkgName := strings.ReplaceAll(pkgPath, "/", "_")
		pkgName = strings.ReplaceAll(pkgName, ".", "_")
		fmt.Fprintf(f, "  \"%s\" [label=\"%s\"];\n", pkgName, pkgPath)
	}

	// Write edges for dependencies
	for _, pkgPath := range sortedPackages {
		// Sanitize package name for DOT
		pkgName := strings.ReplaceAll(pkgPath, "/", "_")
		pkgName = strings.ReplaceAll(pkgName, ".", "_")

		// Get the package object
		pkg := c.graph.packages[pkgPath]

		// Sort dependencies for consistent output
		sort.Strings(pkg.Dependencies)

		for _, dep := range pkg.Dependencies {
			// Sanitize dependency name for DOT
			depName := strings.ReplaceAll(dep, "/", "_")
			depName = strings.ReplaceAll(depName, ".", "_")

			fmt.Fprintf(f, "  \"%s\" -> \"%s\";\n", pkgName, depName)
		}
	}

	// Write DOT file footer
	fmt.Fprintln(f, "}")

	fmt.Printf("Dependency graph generated: %s\n", dotFile)
	fmt.Println("You can visualize this with Graphviz by running:")
	fmt.Printf("  dot -Tpng %s -o %s\n", dotFile, filepath.Join(graphDir, "package_dependencies.png"))

	return nil
}

// GenerateConversionReport creates a detailed report of the conversion process
func (c *StdLibConverter) GenerateConversionReport() error {
	fmt.Println("Generating conversion report...")

	// Ensure reports directory exists
	reportDir := filepath.Join(c.go2csPath, "reports")
	if err := os.MkdirAll(reportDir, 0755); err != nil {
		return fmt.Errorf("failed to create reports directory: %w", err)
	}

	// Create report file
	reportFile := filepath.Join(reportDir, "conversion_report.html")
	f, err := os.Create(reportFile)
	if err != nil {
		return fmt.Errorf("failed to create report file: %w", err)
	}
	defer f.Close()

	// Write HTML header
	fmt.Fprintln(f, "<!DOCTYPE html>")
	fmt.Fprintln(f, "<html>")
	fmt.Fprintln(f, "<head>")
	fmt.Fprintln(f, "  <title>Go Standard Library Conversion Report</title>")
	fmt.Fprintln(f, "  <style>")
	fmt.Fprintln(f, "    body { font-family: Arial, sans-serif; margin: 20px; }")
	fmt.Fprintln(f, "    h1 { color: #0066cc; }")
	fmt.Fprintln(f, "    table { border-collapse: collapse; width: 100%; }")
	fmt.Fprintln(f, "    th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }")
	fmt.Fprintln(f, "    tr:nth-child(even) { background-color: #f2f2f2; }")
	fmt.Fprintln(f, "    th { background-color: #0066cc; color: white; }")
	fmt.Fprintln(f, "    .success { color: green; }")
	fmt.Fprintln(f, "    .failure { color: red; }")
	fmt.Fprintln(f, "  </style>")
	fmt.Fprintln(f, "</head>")
	fmt.Fprintln(f, "<body>")

	// Write report header
	fmt.Fprintln(f, "  <h1>Go Standard Library Conversion Report</h1>")
	fmt.Fprintf(f, "  <p>Generated: %s</p>\n", time.Now().Format(time.RFC1123))
	fmt.Fprintf(f, "  <p>Target platform: %s</p>\n", c.options.targetPlatform)

	// Write package summary
	fmt.Fprintln(f, "  <h2>Package Summary</h2>")
	fmt.Fprintln(f, "  <ul>")
	fmt.Fprintf(f, "    <li>Total packages: %d</li>\n", len(c.graph.packages))
	fmt.Fprintf(f, "    <li>Packages in conversion queue: %d</li>\n", len(c.graph.sortedQueue))
	fmt.Fprintln(f, "  </ul>")

	// Write package table
	fmt.Fprintln(f, "  <h2>Package Details</h2>")
	fmt.Fprintln(f, "  <table>")
	fmt.Fprintln(f, "    <tr>")
	fmt.Fprintln(f, "      <th>Package</th>")
	fmt.Fprintln(f, "      <th>Dependencies</th>")
	fmt.Fprintln(f, "      <th>Dependents</th>")
	fmt.Fprintln(f, "    </tr>")

	// Sort packages by name for consistent output
	sortedPackages := make([]string, 0, len(c.graph.packages))
	for pkgPath := range c.graph.packages {
		sortedPackages = append(sortedPackages, pkgPath)
	}
	sort.Strings(sortedPackages)

	// Add a row for each package
	for _, pkgPath := range sortedPackages {
		pkg := c.graph.packages[pkgPath]
		fmt.Fprintln(f, "    <tr>")
		fmt.Fprintf(f, "      <td>%s</td>\n", pkgPath)
		fmt.Fprintf(f, "      <td>%d</td>\n", len(pkg.Dependencies))
		fmt.Fprintf(f, "      <td>%d</td>\n", len(pkg.Dependents))
		fmt.Fprintln(f, "    </tr>")
	}

	fmt.Fprintln(f, "  </table>")

	// Write information about the conversion order
	fmt.Fprintln(f, "  <h2>Conversion Order</h2>")
	fmt.Fprintln(f, "  <p>Packages will be converted in the following order (based on dependency analysis):</p>")
	fmt.Fprintln(f, "  <ol>")

	// List the first 20 packages and last 20 packages
	firstN := 20
	lastN := 20

	if len(c.graph.sortedQueue) <= firstN+lastN {
		// If we have fewer than firstN+lastN packages, just list them all
		for _, pkgPath := range c.graph.sortedQueue {
			fmt.Fprintf(f, "    <li>%s</li>\n", pkgPath)
		}
	} else {
		// List the first firstN packages
		for i := 0; i < firstN && i < len(c.graph.sortedQueue); i++ {
			fmt.Fprintf(f, "    <li>%s</li>\n", c.graph.sortedQueue[i])
		}

		fmt.Fprintln(f, "    <li>... (and more) ...</li>")

		// List the last lastN packages
		for i := len(c.graph.sortedQueue) - lastN; i < len(c.graph.sortedQueue); i++ {
			fmt.Fprintf(f, "    <li>%s</li>\n", c.graph.sortedQueue[i])
		}
	}

	fmt.Fprintln(f, "  </ol>")

	// Write HTML footer
	fmt.Fprintln(f, "</body>")
	fmt.Fprintln(f, "</html>")

	fmt.Printf("Conversion report generated: %s\n", reportFile)

	return nil
}

// formatDuration formats a duration into a human-readable string
func formatDuration(d time.Duration) string {
	if d < time.Minute {
		return fmt.Sprintf("%.3gs", d.Seconds())
	} else if d < time.Hour {
		m := d / time.Minute
		s := (d % time.Minute) / time.Second
		return fmt.Sprintf("%dm %ds", m, s)
	} else {
		h := d / time.Hour
		m := (d % time.Hour) / time.Minute
		return fmt.Sprintf("%dh %dm", h, m)
	}
}
