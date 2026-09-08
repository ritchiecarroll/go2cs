// conversionDriver.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// This file owns the per-conversion DRIVER: given one resolved input (a single .go file or a
// package directory) and the options that describe what to do with it, processConversion runs the
// whole pipeline — load types, run the analysis passes, visit each file, then write the package
// metadata and project scaffolding.
//
// It is the layer between main() (which decides WHAT to convert, possibly hundreds of times) and
// the visit*/conv* files (which decide how one syntax node becomes C#). Read this first to see the
// order the passes run in and why.

package main

import (
	"fmt"
	"go/ast"
	"go/types"
	"log"
	"os"
	"path/filepath"
	"runtime"
	"strings"

	"golang.org/x/tools/go/packages"
)

// goModCache memoizes goModCacheDir's resolution (conversions run sequentially); tests pin it
// directly to point the module-cache classification at a fixture root.
var goModCache string

// goModCacheDir returns the Go module cache root (GOMODCACHE), resolved once: the environment
// override first, then `go env`, falling back to the documented default <GOPATH>/pkg/mod
// (main() has normalized the GOPATH environment variable by the time any conversion runs).
func goModCacheDir() string {
	if goModCache != "" {
		return goModCache
	}

	if goModCache = os.Getenv("GOMODCACHE"); goModCache != "" {
		return goModCache
	}

	if dir, err := getGoEnv("GOMODCACHE"); err == nil && dir != "" {
		goModCache = dir
		return goModCache
	}

	goModCache = filepath.Join(os.Getenv("GOPATH"), "pkg", "mod")

	return goModCache
}

// loadedPackageIsAt reports whether the load produced exactly the package that lives at dir. Used to
// confirm that an import-path load landed on the directory the convert-set entry names, since the
// output path is derived from that directory. Comparison is case-insensitive on Windows, matching
// isPathUnder's treatment of the same paths.
func loadedPackageIsAt(pkgs []*packages.Package, dir string) bool {
	for _, pkg := range pkgs {
		if pkg.Dir == "" {
			continue
		}

		if pkg.Dir == dir {
			return true
		}

		if runtime.GOOS == "windows" && strings.EqualFold(pkg.Dir, dir) {
			return true
		}
	}

	return false
}

// processConversion converts ONE resolved input — a single .go file or a package directory — into
// C# at outputFilePath. It returns an error only for a PACKAGE LOAD failure, which is the one
// failure mode that belongs to the input rather than to the environment: a batch driver
// (ModuleConverter, StdLibConverter) records the package as failed and converts the rest, while a
// single-package caller (main) reports it and exits. Everything after the load still exits on
// failure — those are I/O faults on the output tree, not a property of the package being converted.
func processConversion(inputFilePath string, isDir bool, outputFilePath string, options Options) error {
	var err error

	cfg := &packages.Config{
		Mode:       packages.LoadAllSyntax,
		Dir:        inputFilePath,
		BuildFlags: options.loaderBuildFlags(),
	}

	targetParts := strings.Split(options.targetPlatform, "/")

	if len(targetParts) != 2 {
		log.Fatalf("Invalid target platform format: %s\n", options.targetPlatform)
	}

	// Two separate KEY=VALUE entries — matching stdLibConverter/moduleConverter. The old
	// single-token form (`"GOOS=%s", "GOARCH=%s"` through ONE Sprintf) set an env var
	// literally named `"GOOS` that the go command ignored, so -platforms never reached
	// the loader here: it loaded host-platform files while the converter's filename
	// filter used the requested platform, silently dropping BOTH platforms' constrained
	// files from a cross-platform conversion.
	cfg.Env = append(os.Environ(), fmt.Sprintf("GOOS=%s", targetParts[0]), fmt.Sprintf("GOARCH=%s", targetParts[1]))

	// A MODULE-CACHE package is loaded from the MAIN MODULE's directory, by import path — not from
	// its own directory, by path. The distinction is not stylistic: the go command treats the module
	// whose go.mod it finds by walking up from cfg.Dir as the MAIN module, and a cache directory is
	// not one. Promoting a dependency's go.mod to main-module status activates directives that are
	// authoritative only there, and a published module zip routinely carries ones that are vestigial
	// outside the source repo they were written for:
	//
	//   - `replace` (issue #33) — `go.opentelemetry.io/otel`'s go.mod says `replace
	//     go.opentelemetry.io/otel/trace => ./trace`, correct in the monorepo where that is a sibling
	//     directory. The zip EXCLUDES it (trace is its own module), so in the cache the go command
	//     reports "replacement directory ./trace does not exist", `otel/trace` never loads, its
	//     types.Package comes back empty-named, and go/types reports `could not import
	//     go.opentelemetry.io/otel/trace (invalid package name: "")` at every use site — untyping
	//     189 of the 244 packages in that one module. A `replace` is honored ONLY in the main module,
	//     so loading from the app's directory ignores it, which is the correct semantics.
	//   - `go.work` (issue #32) — the cloud.google.com/go monorepo ships one listing ~200 sibling
	//     modules that are not in the cache, and the load failed with "cannot load module ../<sibling>
	//     listed in go.work file".
	//
	// Loading from the main module resolves the package exactly as the app's own build does, which is
	// also how ModuleConverter.loadClosure discovered it in the first place. The go command never
	// enters the dependency's directory, so BOTH families above are structurally out of reach rather
	// than each needing its own gate.
	//
	// The GOWORK=off gate stays for every load that must still run from inside the cache — a
	// single-package conversion has no main module to borrow a context from.
	loadPattern := inputFilePath
	inModuleCache := isPathUnder(inputFilePath, goModCacheDir())
	fromMainModule := inModuleCache && options.recurse && options.mainModuleDir != "" && options.packageImportPath != ""

	if fromMainModule {
		cfg.Dir = options.mainModuleDir
		loadPattern = options.packageImportPath
	} else if inModuleCache {
		cfg.Env = append(cfg.Env, "GOWORK=off")
	}

	var pkgs []*packages.Package

	// Under -recurse, ModuleConverter drives conversion one package at a time and passes the exact
	// package dir; load only THAT package (never "./...", which would additionally pull in and
	// re-convert sibling sub-packages — each is already its own convert-set entry, including
	// read-only module-cache packages that must route to the recurse-output pkg tree individually). Outside
	// recurse, a GOPATH input keeps the "./..." subtree behavior unchanged.
	if !options.recurse && strings.HasPrefix(strings.ToLower(inputFilePath), strings.ToLower(options.goPath)) {
		pkgs, err = packages.Load(cfg, "./...")
	} else {
		pkgs, err = packages.Load(cfg, loadPattern)
	}

	// An import path resolves through the main module's version selection, so it MUST land on the
	// directory the closure was built from — the same selection produced both. If it somehow does
	// not, the output would be written for one package under another's path, silently; fall back to
	// the directory load rather than emit that. Defensive, and it has never been observed to fire.
	if fromMainModule && err == nil && !loadedPackageIsAt(pkgs, inputFilePath) {
		showWarning("Import path %q resolved away from %q; re-loading that directory directly", options.packageImportPath, inputFilePath)

		cfg.Dir = inputFilePath
		cfg.Env = append(cfg.Env, "GOWORK=off")
		pkgs, err = packages.Load(cfg, inputFilePath)
	}

	// A package that loads WITH errors still converts, best-effort — but say so, and name it. Every
	// expression downstream of one of these errors is left untyped by go/types, so the emitted C# for
	// that region cannot compile no matter how the converter behaves; the surrounding declarations
	// convert normally. Under -recurse this line is the only account of WHY a package's output is
	// degraded, and it scrolls past among hundreds of packages, so it has to identify itself rather
	// than print a bare "Errors:" (issue #33).
	for _, pkg := range pkgs {
		if len(pkg.Errors) > 0 {
			log.Printf("WARNING: %s did not fully type-check; converting best-effort — code depending on the following is emitted untyped: %v", pkg.PkgPath, pkg.Errors)
		}
	}

	// A load failure is a property of THIS package (a dependency with a missing go.sum entry, a
	// toolchain the module requires but the host lacks, a broken import), so it is returned rather
	// than fatal: under -recurse a single unloadable third-party package used to abort the entire
	// run, discarding every package still queued behind it.
	if err != nil {
		return fmt.Errorf("failed to parse files in directory %q: %w", inputFilePath, err)
	}

	for _, pkg := range pkgs {
		// Keep production reference spelling stable between ordinary and -tests conversion:
		// go/packages omits `_test.go` from this production package, so cheaply scan the
		// build-selected in-package test files for declarator names before collision analysis.
		// This is package-local (important for ./... loads) and reads no test dependencies.
		siblingSignals := collectSiblingTestSignals(pkg.Dir, pkg.Name, options)
		siblingTestFuncMethodNames = siblingSignals.funcMethodNames
		siblingTestAddressedGlobalNames = siblingSignals.addressedGlobalNames
		hasSiblingInternalTestFiles = siblingSignals.hasInternalTests
		options.testFriendAssembly = hasSiblingInternalTestFiles

		// Reset package level variables and capture the per-package inputs (packageDoc,
		// importPackageDirs) — shared with the test-conversion path, see packageStateOperations.go
		resetPackageState(pkg)

		files := []FileEntry{}
		unmarkedFileCount := 0
		fset := pkg.Fset
		packageTypes := pkg.Types
		info := pkg.TypesInfo

		packageInputPath := inputFilePath
		packageOutputPath := outputFilePath

		if len(pkg.Dir) > 0 && pkg.Dir != packageInputPath {
			// Adjust output path if the input is a subdirectory of the package directory
			subPath := strings.Replace(pkg.Dir, packageInputPath, "", 1)
			packageOutputPath = filepath.Join(packageOutputPath, subPath)
			packageInputPath = pkg.Dir
		}

		var projectName, projectFileName, projectFileContents string
		projectName, packageNamespace = getProjectName(packageInputPath, options)

		if projectFileName, projectFileContents, err = prepareProjectFiles(projectName, packageNamespace, packageOutputPath); err != nil {
			log.Fatalf("Failed to write project files for directory \"%s\": %s\n", packageOutputPath, err)
		} else {
			paired, skippedGenerated := syntaxSourceFiles(pkg)

			// Only a cgo package produces these (never met on Windows, where no cgo files are
			// selected for any stdlib package): the parsed set then carries toolchain-GENERATED
			// intermediates whose mangled content has no C# conversion. Say so per file — this
			// line is the only account of why the emitted package is missing its cgo half.
			for _, skippedPath := range skippedGenerated {
				showWarning("Skipping generated file %q: not among package %s's plain Go sources (a cgo intermediate has no C# conversion); the package converts best-effort without it", skippedPath, pkg.PkgPath)
			}

			for _, pair := range paired {
				file, path := pair.file, pair.path

				// cfg.Dir is the directory the loader ran the go command in, so it is also the
				// directory whose GOTOOLCHAIN resolution decides which Go release tags the
				// constraint re-check must agree with.
				if match, err := CheckBuildConstraints(path, options.targetPlatform, options.buildTags, cfg.Dir); err != nil {
					showWarning("Failed to evaluate build constraints for file \"%s\": %s", path, err)
				} else if !match {
					// Skipping file due to non-matching build constraints
					continue
				}

				// See if output already exists and has been marked as manually converted. The
				// probe follows layout L3's routing (platformLayout.go): a hand-owned file of an
				// L3 package lives in the per-GOOS folder its emission does, so asking flat would
				// miss the marker and convert over it.
				outputFileName := platformLayoutPath(packageOutputPath, goosOfTarget(options.targetPlatform),
					strings.TrimSuffix(filepath.Base(path), ".go")+".cs")
				manualConv, err := containsManualConversionMarker(outputFileName)

				if err != nil {
					log.Fatalf("Failed to check for manual conversion in file \"%s\": %s\n", outputFileName, err)
				}

				if !manualConv {
					files = append(files, newFileEntry(file, path, false))
					unmarkedFileCount++
				} else if isDir {
					// Manually-converted destination: the hand-owned `.cs` is never overwritten,
					// but the source .go MUST stay in the convert set, in pkg.Syntax order — its
					// analysis and visit feed package-wide emission state that sibling files depend
					// on (anonymous-struct lifts, package-var registrations, escape/addressed-global
					// analysis, imports, init/temp-var numbering). Only the file's EMISSION is
					// redirected, to the non-compiled `<name>.cs.auto` review sibling (see the
					// file-visit loop below). Dropping the visit entirely corrupted every sibling
					// file of a seeded reconvert: raw Go `struct{...}` text where a lifted type
					// name belongs, and package-var assignments re-declared as shadowing locals.
					files = append(files, newFileEntry(file, path, true))
				}
			}
		}

		if unmarkedFileCount == 0 {
			if len(files) > 0 {
				// FULLY hand-owned package: nothing to (re)convert normally — the .csproj,
				// package_info.cs and package_init.cs stay hand-owned too — but still emit the
				// `.cs.auto` review siblings. Run the whole-package analyses the sibling
				// conversion depends on first — safe, since every package-level global they and
				// the sibling visits mutate is reset at the top of the next package iteration.
				performNameCollisionAnalysis(pkg)
				collectCaptureModeMethods(pkg)
				collectTypeSpecRHS(pkg)
				collectHoistedLiterals(files, packageTypes, info, goosOfTarget(options.targetPlatform), nil, true)
				collectMovedInitVars(fset, packageTypes, info, pkg.Syntax)
				collectPublicizedTypes(packageTypes)

				// ж-box A1: the ref-lowering classification runs in the hand-owned-sibling driver
				// too (the three-driver rule, DESIGN-zh-box-reduction §3.5) — analysis only, no
				// emission reads it; -debug surfaces the census.
				performRefLoweringAnalysis(files, packageTypes, info, options)

				emitAutoConversionSiblings(files, fset, packageTypes, info, map[*ast.Ident]string{}, map[string]*types.Var{}, packageOutputPath, options)

				// UN-FREEZE this package's METADATA. Until now the `continue` below skipped
				// writeProjectFile/writePackageInfoFile entirely, so the four packages whose
				// every production file is marked -- crypto/internal/boring/bcache,
				// internal/concurrent, internal/godebug, internal/weak -- carried a `.csproj`
				// and `package_info.cs` that NO conversion ever re-emitted. Their frozen
				// `<ImportedTypeAliases>` block then aged against its own dependencies: at Go
				// 1.24 `internal/abi.MapType` splits into OldMapType/SwissMapType, so the stale
				// `abi`-MapType alias becomes a CS0426 in a file nothing regenerates
				// (CENSUS-h6-handown-package-aliases.md, 2026-09-08 amendment). Re-minting here
				// makes every future release hop carry these four along with the rest.
				//
				// The hand-owned `.cs` files are still never overwritten -- ONLY the metadata is
				// re-minted, from the analyses the sibling emission above has just run.
				// `projectFileName`/`projectFileContents` come from prepareProjectFiles earlier
				// in this same iteration, and `projectImports` is filled by the union inside
				// emitAutoConversionSiblings (added with this change -- without it the emitted
				// .csproj would carry NO ProjectReferences at all).
				// recordSamePackageImplements is DELIBERATELY not run here. The normal path calls
				// it with the package's globalIdentNames/globalScope; this path has neither --
				// emitAutoConversionSiblings above passes EMPTY maps inline -- so calling it here
				// would record against empty global state rather than record nothing. Whether any
				// of the four packages loses a [GoImplement] record it previously carried is a
				// question for the two-seeded diff to answer, not for this comment to assume.
				if err = writeProjectFile(projectFileName, projectFileContents, packageOutputPath, packageTypes, options); err != nil {
					log.Fatalf("Error while writing project file \"%s\": %s\n", projectFileName, err)
				}

				if err := collectPublishedRefVerdicts(pkg, packageOutputPath); err != nil {
					return err
				}

				writePackageInfoFile(packageInfoPath(packageOutputPath, isDir, options), !isDir)
			} else {
				showMessage("Skipping conversion: no target Go source files found for conversion in input path \"%s\"", packageInputPath)
			}

			continue
		}

		globalIdentNames := make(map[*ast.Ident]string)
		globalScope := map[string]*types.Var{}

		// Perform name collision analysis
		performNameCollisionAnalysis(pkg)

		// Pre-process all global variables in package
		for _, fileEntry := range files {
			performGlobalVariableAnalysis(fileEntry.file.Decls, info, globalIdentNames, globalScope)

			if options.showParseTree {
				ast.Fprint(os.Stdout, fset, fileEntry.file, nil)
			}
		}

		// Package-wide, computed once and shared by every file's Visitor — see
		// callerInliningAnalysis.go. Must run after `files` is fully populated (it walks every
		// file's declarations) but has no other ordering dependency on the analyses above/below it.
		needsNoInlining := computeNoInliningClosure(files, packageTypes, info)

		// Perform escape analysis for each file
		// Identify capture-mode methods (those taking &recv.field) — across the package
		// and its imports — before escape analysis, so a value var on which one is
		// called can be marked as escaping (and the call routed through the ж overload).
		collectCaptureModeMethods(pkg)

		// Record each defined type's WRITTEN right-hand side (lost by Named.Underlying()'s
		// full resolution) — the array-reinterpret emission in convCallExpr consults it.
		collectTypeSpecRHS(pkg)

		// ж-box A2 (DESIGN-zh-box-reduction §3.3/§3.4): classify every package-level function's
		// pointer parameters for ref-lowering BEFORE escape analysis — the reversion refinement
		// ("address-taken only into lowered positions → stack") is consulted by the escape
		// analysis, and the signature/call-site emission reads the verdicts during the visits.
		performRefLoweringAnalysis(files, packageTypes, info, options)

		performEscapeAnalysis(files, fset, packageTypes, info)

		// Find package-level vars whose address is taken (cross-file) so their
		// declarations can be emitted as heap boxes that &global references directly.
		collectAddressedGlobals(files, packageTypes, info)

		// Decide which string literals are hoisted to package-scoped `static readonly` fields
		// (Tier C — see hoistedLiteralOperations.go). A whole-package PRE-pass: pre-boxing needs
		// every use of a literal before any file emits, and collectMovedInitVars below consults
		// the reader set this produces, so it must run first.
		collectHoistedLiterals(files, packageTypes, info, goosOfTarget(options.targetPlatform), nil, true)

		// Find package-level var initializers whose Go dependency order cannot be reproduced by
		// C#'s static-field-initializer order (cross-file / same-file forward reference /
		// dependency on a relocated var — resolved transitively through package function bodies,
		// mirroring Go's own analysis), so their initialization can be relocated into an ordered
		// static constructor (package_init.cs).
		collectMovedInitVars(fset, packageTypes, info, pkg.Syntax)

		// Find import aliases whose name collides with a child namespace visible from the
		// transitive import closure (CS0576) so alias emission and every package-qualifier
		// render Δ-renames them consistently.
		computeImportAliasRenames(files, packageTypes, packageNamespace, options.go2csPath, goosOfTarget(options.targetPlatform))

		// Find unexported types used as exported struct fields so they can be emitted as public
		// (an exported field's type must be at least as accessible — CS0051/CS0052).
		collectPublicizedTypes(packageTypes)

		// Find this package's definition-side one-arg //go:linkname handles (Go 1.23's opt-in that
		// authorizes cross-package linkname pulls) so the handled vars emit `public` — letting a
		// puller in another assembly reach them through its forwarding property (see linknameOperations).
		// (The ref-lowering pass above reads its OWN production-file linkname scan — never this
		// global — so this ordering is a display concern only; see performRefLoweringAnalysis.)
		collectLinknameHandles(pkg.Syntax)

		// Bind this package's //go:cgo_import_dynamic pragmas to the trampoline declarations they
		// stand for, so package_info.cs can publish the records golib's GoCgoDynamicImports resolves
		// abi.FuncPCABI0 of a darwin trampoline through (see cgoDynamicImports.go). Package-wide for
		// the same reason the linkname pass is: the pragma and the declaration it names need not
		// share a file.
		collectCgoDynamicImports(pkg.Syntax)

		// Preload the imported type aliases of every package these files import, BEFORE converting any
		// file, so a foreign renamed type reached transitively (through a value whose package this file
		// does not itself import) resolves through its recorded alias regardless of file order (see
		// preloadImportedTypeAliases — go/printer comment.go's `slash` token.Pos heap box, CS0426).
		preloadImportedTypeAliases(files, options)

		var outputFileNames []string

		// Convert files SEQUENTIALLY, in the deterministic pkg.Syntax (sorted filename) order. Files
		// were previously converted in concurrent goroutines, but the per-file visitors share package-
		// level state claimed at visit time — initFuncCounter (initΔN indices), getGlobalTempVarName
		// (blank `_` func/var numbering, an unsynchronized map), and the loadImportedTypeAliases
		// check-then-act (a file marked an imported package_info "parsed" BEFORE the parse finished, so
		// a concurrently-converting file saw the marker, skipped the wait, and emitted an imported
		// const collision-rename bare — e.g. `abi.String` instead of `abi.ΔString`, a compile error
		// that came and went with goroutine scheduling). Claim order = schedule order made the emitted
		// bytes nondeterministic across otherwise-identical runs. Per-file emission is a small fraction
		// of conversion cost (dominated by go/packages type-graph loading), so sequential conversion
		// buys byte-reproducible output for free: a full-stdlib conversion (305 packages) measured
		// 3m42s with the concurrent per-file goroutines and 3m39s sequential — within noise.

		// The position-map records of these files belong in THIS package's info file, which is the
		// compilation that compiles them. Resolved before the loop because each visitor needs it;
		// writePackageInfoFile below is handed the same expression. Claiming the key here rather
		// than in resetPackageState is deliberate: the -tests flow resets per VARIANT while its
		// info files accumulate records ACROSS variants, so ownership of a key belongs to the
		// conversion that will write the file, not to the reset.
		options.positionMapTarget = claimPositionMapTarget(packageInfoPath(packageOutputPath, isDir, options))

		for _, fileEntry := range files {
			func(fileEntry FileEntry) {
				defer func() {
					if !options.debugMode {
						if r := recover(); r != nil {
							if fileEntry.manualConversion {
								showWarning("visit file error: %v in \"%s\" (auto-conversion sibling skipped)", r, filepath.Base(fileEntry.filePath))
							} else {
								showWarning("visit file error: %v in \"%s\"", r, filepath.Base(fileEntry.filePath))
							}
						}
					}
				}()

				visitor := newFileVisitor(fset, packageTypes, info, options, globalIdentNames, globalScope, needsNoInlining, fileEntry)

				visitor.visitFile(fileEntry.file)

				var outputFileName string
				baseName := strings.TrimSuffix(filepath.Base(fileEntry.filePath), ".go")

				if !isDir {
					outputFileName = strings.TrimSuffix(packageOutputPath, ".go") + ".cs"
				} else if fileEntry.manualConversion {
					// The `.cs.auto` review sibling follows its hand-owned `.cs` into whichever
					// folder layout L3 put that file in, so the pair stays together.
					outputFileName = platformLayoutPath(packageOutputPath, goosOfTarget(options.targetPlatform), baseName+".cs") + ".auto"
				} else {
					outputFileName = platformLayoutPath(packageOutputPath, goosOfTarget(options.targetPlatform), baseName+".cs")
				}

				if fileEntry.manualConversion {
					// Hand-owned destination: the visit above already fed this file's package-wide
					// state (the part its sibling files depend on); emit the auto conversion to the
					// non-compiled `<name>.cs.auto` review sibling, leaving the marked `.cs` untouched.
					visitor.finalizePositionMap(outputFileName)

					if err := writeAutoConversionSibling(outputFileName, baseName, visitor.outputBuilder.String()); err != nil {
						showWarning("%s", err)
					}
				} else if err := visitor.writeOutputFile(outputFileName); err != nil {
					log.Printf("%s\n", err)
				}

				packageLock.Lock()
				projectImports.UnionWithSet(visitor.importQueue)
				outputFileNames = append(outputFileNames, outputFileName)
				packageLock.Unlock()
			}(fileEntry)
		}

		// Record the [assembly: GoImplement] pairs this package SATISFIES but never WITNESSES —
		// a defined type whose VALUE or POINTER method set implements an exported interface the SAME
		// package declares, with no cast anywhere to record it (encoding/binary's `var BigEndian
		// bigEndian` carries no `var _ ByteOrder = …`; syscall's Sockaddr pairs are witnessed only
		// inside one method body). Runs after the visits so it records into the same state they did,
		// and before writePackageInfoFile so the interface-inheritance prune sees the additions.
		// See samePackageImplements.go.
		recordSamePackageImplements(fset, packageTypes, info, options, globalIdentNames, globalScope, files)

		// Resolve any deferred cross-file dynamic (anonymous struct) type references
		// now that every file's lifted names are registered in the shared registry.
		resolveDynamicTypeMarkers(outputFileNames)

		// Write project file with correct output type and unsafe code settings
		err = writeProjectFile(projectFileName, projectFileContents, packageOutputPath, packageTypes, options)

		if err != nil {
			log.Fatalf("Error while writing project file \"%s\": %s\n", projectFileName, err)
		}

		// The cross-package lowering contract: compute the `ref`-primary records this package
		// publishes (its selected exported primaries plus any registered hand-own declaration,
		// which must be on disk) before the info file is written. A registered hand-own primary
		// the output does not declare is a hard error, never a silent record.
		if err := collectPublishedRefVerdicts(pkg, packageOutputPath); err != nil {
			return err
		}

		packageInfoFileName := packageInfoPath(packageOutputPath, isDir, options)

		writePackageInfoFile(packageInfoFileName, !isDir)

		// Resolve the deferred pointer-adapter names now that the GoImplement records are FINAL —
		// after the interface-inheritance prune and the alias-covered skip, both of which decide
		// which pairs survive to own an adapter class. Must follow writePackageInfoFile, unlike
		// the dynamic-type barrier above, which only needs the file-visit registry.
		resolveAdapterNameMarkers(outputFileNames)

		// Emit the ordered package-var initialization file (no-op unless any initializer was
		// relocated for init-order correctness). Package (directory) conversions only. Under
		// -tests, the ctor carries the erasable test hook so the internal test variant can
		// append its own relocations (writeTestVariantInitFile) — like the IP-4 csproj
		// exclusions, this production-file difference is intended -tests output, not drift.
		if isDir {
			// Go's InitOrder differs when the file set does, so package_init.cs is per-GOOS in the
			// four packages where it varies (design §4.3) — routed by the same L3 rule, passed as
			// the directory this writer joins its fixed file name onto.
			packageInitDir := platformLayoutDir(packageOutputPath, goosOfTarget(options.targetPlatform), PackageInitFileName)

			if err := writePackageInitFile(packageInitDir, packageNamespace, packageName, options.convertTests); err != nil {
				log.Fatalf("Failed to write package init file for \"%s\": %s\n", packageOutputPath, err)
			}
		}

		// NOTE: `.cs.auto` review siblings for manually-converted files were emitted inline by the
		// file-visit loop above — marked files convert WITH the package (same order, same analyses)
		// so their package-wide state reaches sibling files, and only their write target differs.
	}

	// -tests: with the production conversion complete (its package_info.cs is the seed for the
	// test metadata), convert the package's _test.go variants into the colocated test project.
	if options.convertTests {
		if err := processTestConversion(inputFilePath, outputFilePath, options); err != nil {
			log.Fatalf("Failed to convert package tests in %q: %v\n", inputFilePath, err)
		}

		// The W2b GATE, and the LAST thing a -tests conversion does: any dynamic-type marker that
		// went unresolved above — in the production emission, a `.cs.auto` review sibling, or a
		// test variant — was replaced by raw Go type text, which cannot compile. Return it as an
		// error rather than exiting 0 into a build whose diagnostics point away from the cause.
		// See unresolvedDynamicTypeError (dynamicTypeOperations.go) for why this is scoped here.
		//
		// Riding the existing error return is deliberate: main.go already turns it into a nonzero
		// exit, so no new call site can forget to check the gate.
		if err := unresolvedDynamicTypeError(); err != nil {
			return err
		}
	}

	return nil
}

// syntaxSourceFile pairs one parsed file with the on-disk source path it was parsed from.
type syntaxSourceFile struct {
	file *ast.File
	path string
}

// syntaxSourceFiles pairs pkg.Syntax with the source path each syntax tree was parsed from, in
// pkg.Syntax order — the package's plain Go sources as paired entries, every other parsed file's
// path in skipped.
//
// go/packages fills Syntax in parallel with CompiledGoFiles, NOT GoFiles, and for a cgo package
// the two lists differ: each `import "C"` source is replaced in the compiled set by its
// cgo-generated build-cache intermediates (<name>.cgo1.go, _cgo_gotypes.go, …), so Syntax
// outgrows GoFiles. The historical `pkg.GoFiles[i]` walk here therefore panicked with
// index-out-of-range on every cgo package — the whole class on linux/amd64 (internal/testpty,
// net, os/user, plugin, runtime/cgo); Windows selects no cgo files for any of them, so the lists
// coincide there and the panic was unreachable, which is why it survived every Windows gate.
//
// Rather than pair by index against ANY list, each file's path is derived from its OWN token
// position — UNADJUSTED, i.e. the raw path the parser was handed: the //line-ADJUSTED Position of
// a .cgo1.go intermediate reports the original `import "C"` source its directives point back at
// (misnaming what was actually parsed), and a plain source legally opening with a //line
// directive would adjust AWAY from its GoFiles spelling and be wrongly skipped. Self-derivation
// removes the index-pairing class structurally instead of bounds-patching it, and stays honest
// even when Syntax is misaligned with CompiledGoFiles (an unreadable file drops its slot).
//
// The GoFiles MEMBERSHIP test then restores exactly the historical intent — convert the package's
// plain Go sources: for a non-cgo package every syntax file is a GoFiles member under the same
// spelling (both lists come from the same `go list` run), so the pairing is byte-identical to the
// old walk; a non-member is a cgo/toolchain-generated intermediate whose mangled content has no
// C# conversion (its `import "C"` original never reaches Syntax at all), so it is skipped
// EXPLICITLY — reported by the caller — rather than processed accidentally under a build-cache
// path that downstream logic (build-constraint re-check, base-name output routing, hand-own
// marker probe) would misread as a source file.
func syntaxSourceFiles(pkg *packages.Package) (paired []syntaxSourceFile, skipped []string) {
	plainGoFiles := make(map[string]bool, len(pkg.GoFiles))

	for _, path := range pkg.GoFiles {
		plainGoFiles[filepath.Clean(path)] = true
	}

	for _, file := range pkg.Syntax {
		path := pkg.Fset.PositionFor(file.Pos(), false).Filename

		if !plainGoFiles[filepath.Clean(path)] {
			skipped = append(skipped, path)
			continue
		}

		paired = append(paired, syntaxSourceFile{file: file, path: path})
	}

	return paired, skipped
}

// aliasCoveredImplementationKeys returns the "canonicalIface|impl" keys of every GoImplement pair
// that is ALREADY carried by a record under a package type ALIAS of the same interface (os converts
// dirEntry to fs.DirEntry through its own `type DirEntry = fs.DirEntry` AND through the io/fs name).
// The aliased record wins and the qualified duplicate is skipped — this set drives that skip in
// writePackageInfoFile's emission loop, and its adapter-name collision prune consults the same set
// so a duplicate that will be skipped never owns an adapter name. Callers hold packageLock.
func aliasCoveredImplementationKeys() HashSet[string] {
	covered := HashSet[string]{}

	for alias, typeName := range exportedTypeAliases {
		if implementations, ok := interfaceImplementations[alias]; ok {
			canonIface := strings.TrimPrefix(typeName, RootNamespace+".")

			for implementation := range implementations {
				covered.Add(canonIface + "|" + implementation)
			}
		}
	}

	return covered
}

// packageInfoPath resolves a converted package's info file. Shared by the conversion driver's two
// consumers -- the position-map target handed to each visitor before the file loop, and the
// writePackageInfoFile call after it -- so the records and the file they land in can never be
// resolved differently.
//
// package_info.cs is closure-derived, so it is one of the artifacts that can vary by platform (27 of
// them corpus-wide, design section 4.3); it follows the same layout L3 routing a converted source
// file does.
func packageInfoPath(packageOutputPath string, isDir bool, options Options) string {
	if isDir {
		return platformLayoutPath(packageOutputPath, goosOfTarget(options.targetPlatform), PackageInfoFileName)
	}

	return filepath.Join(filepath.Dir(packageOutputPath), PackageInfoFileName)
}
