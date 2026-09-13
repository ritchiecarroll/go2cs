// packageStateOperations.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/ast"
	"go/token"
	"go/types"
	"path/filepath"
	"strings"

	"golang.org/x/tools/go/packages"
)

// This file centralizes the three pieces of per-package conversion setup that every conversion
// driver must perform identically — the package-scoped global reset, FileEntry construction, and
// per-file Visitor construction. processConversion consumes these helpers today; the test-project
// conversion path (Phase 4, -tests) consumes the same helpers so the two can never drift. (The
// abandoned codex/testing-infrastructure branch carried private copies of all three; master then
// grew ~13 globals and 6 Visitor/FileEntry fields the copies never learned about — guaranteed
// nil-map panics. This extraction makes that failure mode structurally impossible.)

// resetPackageState reinitializes every package-scoped global not self-reset by its own analysis
// pass (packageBuiltinShadows, packageFuncMethodNames, packageTypeSpecRHS, captureModeCandidates,
// and packageMethodNames reinitialize themselves at the top of their passes — this function alone
// does NOT establish a clean slate), then captures the per-package inputs derived directly from the loaded
// package: the package Go doc (NuGet README) and the module-aware source dir + package name of
// every REACHABLE imported package (the transitive closure, not just direct imports), so
// cross-package references to LOCAL/USER modules (which go/build cannot resolve) can be wired up —
// both their ProjectReference and their exported-type-alias package_info.cs. The closure matters
// for a type reached ONLY through another package's signature (the on-demand alias load in
// aliasedElementTypeName): the consumer never imports the type's package directly, so a
// direct-imports-only map could not resolve it. Entries are deduped by path; the same path
// resolves to the same Dir/Name from any route, so the walk order is immaterial.
func resetPackageState(pkg *packages.Package) {
	packageName = ""
	packageNamespace = ""
	projectImports = NewHashSet([]string{})
	linknameHandles = NewHashSet([]string{})
	cgoDynamicImports = nil
	currentPackagePath = pkg.PkgPath
	exportedTypeAliases = make(map[string]string)
	importedTypeAliases = make(map[string]string)
	importedTypeAliasSourceDirs = make(map[string]string)
	packageInlineFuncTypeNames = make(map[string]bool)
	importedPointerImplements = HashSet[string]{}
	importedValueImplements = HashSet[string]{}
	importedRefPrimaries = HashSet[string]{}
	packageRefPrimaryRecords = nil
	constImportedTypeAliases = NewHashSet([]string{})
	derivedTypeAliases = NewHashSet([]string{})
	usedDerivedTypeAliases = NewHashSet([]string{})
	parsedPackageInfoFiles = NewHashSet([]string{})
	interfaceImplementations = make(map[string]HashSet[string])
	promotedInterfaceImplementations = make(map[string]HashSet[string])
	constraintProxies = make(map[string][2]string)
	nominalProductionConstraints = HashSet[string]{}
	interfaceInheritances = make(map[string]HashSet[string])
	adapterClassImplementations = HashSet[string]{}
	implicitConversions = make(map[string]HashSet[string])
	invertedImplicitConversions = make(map[string]HashSet[string])
	indirectImplicitConversions = make(map[string]HashSet[string])
	conversionPackageUsings = make(map[string]string)
	numericConversions = make(map[string]map[string]string)
	indirectNumericConversions = make(map[string]map[string]string)
	nameCollisions = make(map[string]bool)
	globalTempVarCount = make(map[string]int)
	packageDynamicTypeNames = make(map[string]string)
	packageLiftedTypeNames = HashSet[string]{}
	productionLiftedTypeNames = nil
	productionAliasLiftedTypes = nil
	productionDynamicTypeNames = nil
	packageHoistedConstOrdinals = nil
	productionHoistedConstOrdinals = nil
	metadataAnchorClassPrefix = ""
	metadataAnchorLocalTypes = false
	packageManualTypeNames = make(map[string]bool)
	packageAddressedGlobals = make(map[types.Object]bool)
	packageMovedInitVars = make(map[types.Object]int)
	packageMovedInitMethods = make(map[int]string)
	packageHoistedLits = make(map[*ast.BasicLit]*hoistedLiteral)
	packageHoistedDecls = make(map[*ast.FuncDecl][]*hoistedLiteral)
	packageHoistLitReaders = make(map[*types.Func]bool)
	packageHoistNames = make(map[string]hoistSeed)
	packageImportAliasRenames = make(map[string]string)
	packageChildNamespaces = make(map[string]bool)
	packageQualifiedNamespaces = make(map[string]bool)
	packageImportLeadingSegments = make(map[string]bool)
	testLocalTypePrefixes = nil
	packagePublicizedTypes = make(map[types.Object]bool)
	packagePublicizedLiftedTypes = make(map[types.Type]bool)
	packageEmittedTypeAccess = HashSet[string]{}
	packageCaptureModeMethods = make(map[*types.Func]bool)
	packageCaptureModeBoxIdents = make(map[types.Object]bool)
	packageDirectBoxReceiverMethods = make(map[*types.Func]bool)
	packageRefReturnPrimaryMethods = make(map[*types.Func]bool)
	initFuncCounter = 0
	usesUnsafeCode = false
	packageImportForces = HashSet[string]{}
	packageImportInits = map[string]string{}
	packageRefLoweringResult = nil

	// Capture the package-level Go doc (rendered to Markdown) for the NuGet README
	packageDoc = extractPackageDoc(pkg.Syntax)

	// Capture the package's own source directory for the README's validation badge, which has to
	// look at the Go sources the conversion never compiles (the `_test.go` files) to tell a package
	// with an unvalidated test suite from one that has no tests at all. Dir comes straight from the
	// loader; the GoFiles fallback covers a driver that reports files without a package directory.
	packageSourceDir = pkg.Dir

	if packageSourceDir == "" && len(pkg.GoFiles) > 0 {
		packageSourceDir = filepath.Dir(pkg.GoFiles[0])
	}

	importPackageDirs = make(map[string]importedPackageMeta)
	importedPackageSources = make(map[string]*packages.Package)
	importedPackages = make(map[string]*packages.Package)
	currentPackageSource = pkg
	packageInitFacts = make(map[string]bool)

	var captureImportDirs func(imports map[string]*packages.Package)

	captureImportDirs = func(imports map[string]*packages.Package) {
		for importPath, importedPkg := range imports {
			if _, exists := importPackageDirs[importPath]; exists {
				continue
			}

			importPackageDirs[importPath] = importedPackageMeta{Dir: importedPkg.Dir, Name: importedPkg.Name}

			// The loaded package itself, keyed by source dir — the handle the alias loader needs to
			// derive a dependency's collision renames when its package_info.cs is absent.
			if importedPkg.Dir != "" {
				importedPackageSources[filepath.Clean(importedPkg.Dir)] = importedPkg
			}

			// …and by import path, under both spellings a GOROOT-vendored path can take, for the
			// callers that hold a path rather than a directory (see importedPackages).
			importedPackages[importPath] = importedPkg

			if vendored := resolveGorootVendoredPath(importPath); vendored != importPath {
				importedPackages[vendored] = importedPkg
			}

			captureImportDirs(importedPkg.Imports)
		}
	}

	captureImportDirs(pkg.Imports)
}

// newFileEntry constructs a FileEntry with every per-file analysis map initialized — the escape
// map plus the two sstring maps performEscapeAnalysis and the visitors write into (a zero-valued
// map here is a guaranteed panic on first write).
func newFileEntry(file *ast.File, filePath string, manualConversion bool) FileEntry {
	return FileEntry{
		file:             file,
		filePath:         filePath,
		identEscapesHeap: map[types.Object]bool{},
		sstringEligible:  map[types.Object]bool{},
		ssliceEligible:   map[types.Object]bool{},
		sstringConvExprs: map[*ast.CallExpr]bool{},
		manualConversion: manualConversion,
	}
}

// newFileVisitor constructs the per-file Visitor with every eagerly-required field initialized.
// Fields absent here (e.g. tightenedConsts, untypedConstContexts) are deliberately lazy — their
// operations nil-check before first write.
func newFileVisitor(fset *token.FileSet, packageTypes *types.Package, info *types.Info, options Options, globalIdentNames map[*ast.Ident]string, globalScope map[string]*types.Var, needsNoInlining map[types.Object]bool, fileEntry FileEntry) *Visitor {
	return &Visitor{
		fset:                      fset,
		pkg:                       packageTypes,
		info:                      info,
		needsNoInlining:           needsNoInlining,
		outputBuilder:             &strings.Builder{},
		liftedTypeNames:           HashSet[string]{},
		liftedTypeMap:             map[types.Type]string{},
		liftedAnonStructNames:     map[string]string{},
		subStructTypes:            map[types.Type][]types.Type{},
		packageImports:            &strings.Builder{},
		requiredUsings:            HashSet[string]{},
		importQueue:               HashSet[string]{},
		referencedForeignPackages: HashSet[string]{},
		canonicalAliasImported:    HashSet[string]{},
		importAliasesEmitted:      HashSet[string]{},
		importAliasTargets:        map[string]string{},
		importPathAliases:         map[string]string{},
		typeAliasDeclarations:     &strings.Builder{},
		standAloneComments:        map[token.Pos]string{},
		sortedCommentPos:          []token.Pos{},
		processedComments:         HashSet[token.Pos]{},
		newline:                   "\r\n",
		options:                   options,
		globalIdentNames:          globalIdentNames,
		globalScope:               globalScope,
		blocks:                    Stack[*strings.Builder]{},
		manualConversion:          fileEntry.manualConversion,
		sourceFilePath:            fileEntry.filePath,
		identEscapesHeap:          fileEntry.identEscapesHeap,
		sstringEligible:           fileEntry.sstringEligible,
		ssliceEligible:            fileEntry.ssliceEligible,
		sstringConvExprs:          fileEntry.sstringConvExprs,
	}
}
