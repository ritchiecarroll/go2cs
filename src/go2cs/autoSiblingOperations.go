// autoSiblingOperations.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

package main

import (
	"fmt"
	"go/ast"
	"go/token"
	"go/types"
	"os"
	"path/filepath"
	"strings"
)

// emitAutoConversionSiblings converts each manually-converted (marker-skipped) source file of a
// FULLY hand-owned package — one whose convert set has NO unmarked files left — to a NON-COMPILED
// `<name>.cs.auto` sibling written beside the hand-owned `<name>.cs`, so upgrade-time review (a new
// Go version vs the hand-owned C#) has an auto-converted reference to diff against. Without this, a
// `[module: go.GoManualConversion]` whole-file rewrite leaves NO auto output at all.
//
// This pass serves ONLY the fully-hand-owned case, where the normal conversion path is skipped
// outright (no .csproj / package_info.cs / package_init.cs is regenerated — those are hand-owned
// too) and none of the per-file analyses have run. A PARTIALLY hand-owned package never reaches
// this code: its marked files stay in the main convert set — analyzed and visited with the package,
// in pkg.Syntax order, so their package-wide state (anonymous-struct lifts, package-var
// registrations, escape/addressed-global analysis, imports) reaches sibling files exactly as in an
// unseeded conversion — with only their write target redirected to `<name>.cs.auto` (see the
// file-visit loop in main.go). Emitting siblings AFTER the normal .cs set was written (this
// function's original role for partial packages) corrupted every sibling file of a seeded
// reconvert, because the marked files' state contributions arrived too late.
//
// Side-effect safety: the caller runs the whole-package analyses (name collision, capture-mode
// methods, typespec RHS, moved init vars, publicized types) first; every package-level global the
// isolated conversions below mutate is dead state afterward (processConversion resets all of it at
// the top of the next package iteration), so the pass can reuse the normal analysis + visitor
// pipeline verbatim and still add ONLY `.cs.auto` files.
//
// The `.cs.auto` extension is invisible to the generated projects: csproj-template.xml compiles
// `<Compile Include="*.cs" />` only, which cannot match a file name ending in `.auto`.
//
// Scope: package (directory) conversions only — in single-file mode the manual-conversion gate is
// not effective (the destination probe path never matches), so there is nothing to sibling. A
// marked file whose conversion panics is skipped with a warning (deterministically), matching the
// normal conversion path; a package the converter never queues (e.g. `unsafe`, compiler-intrinsic)
// gets no sibling because its files are never visited at all.
func emitAutoConversionSiblings(markedFiles []FileEntry, fset *token.FileSet, packageTypes *types.Package,
	info *types.Info, globalIdentNames map[*ast.Ident]string, globalScope map[string]*types.Var,
	packageOutputPath string, options Options) {
	if len(markedFiles) == 0 {
		return
	}

	// Mirror the per-file analysis pipeline the normal conversion runs over its convert set.
	// All package-level state these passes mutate is dead after the package's artifacts are
	// written (see the side-effect note above), so no snapshot/restore is needed.
	for _, fileEntry := range markedFiles {
		performGlobalVariableAnalysis(fileEntry.file.Decls, info, globalIdentNames, globalScope)
	}

	performEscapeAnalysis(markedFiles, fset, packageTypes, info)
	collectAddressedGlobals(markedFiles, packageTypes, info)
	computeImportAliasRenames(markedFiles, packageTypes, packageNamespace, options.go2csPath, goosOfTarget(options.targetPlatform))
	preloadImportedTypeAliases(markedFiles, options)

	var autoFileNames []string

	// Convert sequentially in markedFiles (pkg.Syntax) order — deterministic, like the normal path.
	for _, fileEntry := range markedFiles {
		func(fileEntry FileEntry) {
			defer func() {
				if !options.debugMode {
					if r := recover(); r != nil {
						showWarning("visit file error: %v in \"%s\" (auto-conversion sibling skipped)", r, filepath.Base(fileEntry.filePath))
					}
				}
			}()

			// The SAME constructor the normal per-file path uses. This was a hand-rolled copy of
			// newFileVisitor's literal until 2026-08-19, and it had drifted by two fields — which
			// is precisely the failure mode a duplicated struct literal has. `importInits` (a
			// per-file buffer the hook relocation has since retired) was nil, so visitFile's
			// `v.importInits.String()` nil-dereferenced and the
			// recover above reported "visit file error … (auto-conversion sibling skipped)" for
			// every fully-hand-owned package: internal/concurrent, internal/godebug and
			// internal/weak, the three whose single Go file is marked, so no sibling was EVER
			// produced for them. `manualConversion` was false, though the field's whole meaning is
			// "this file's destination .cs is hand-owned and the text lands in the .cs.auto review
			// sibling" — exactly this pass. Constructing through newFileVisitor keeps both correct
			// by construction and cannot drift again.
			visitor := newFileVisitor(fset, packageTypes, info, options, globalIdentNames, globalScope, map[types.Object]bool{}, fileEntry)

			visitor.visitFile(fileEntry.file)

			// A fully hand-owned package's .csproj ProjectReferences are minted from
			// `projectImports` (projectFileWriter.go's getImportPackageInfo), which the NORMAL
			// per-file path fills at its own union under packageLock. This path never did,
			// because it never wrote metadata -- so un-freezing that metadata WITHOUT this line
			// would emit a .csproj carrying NO ProjectReferences at all, which is worse than the
			// frozen file it replaces. No lock is taken: unlike the normal path's per-file
			// goroutines, this loop is sequential by construction (see the comment above it).
			projectImports.UnionWithSet(visitor.importQueue)

			baseName := strings.TrimSuffix(filepath.Base(fileEntry.filePath), ".go")
			autoFileName := filepath.Join(packageOutputPath, baseName+".cs.auto")

			visitor.finalizePositionMap(autoFileName)

			if err := writeAutoConversionSibling(autoFileName, baseName, visitor.outputBuilder.String()); err != nil {
				showWarning("%s", err)
				return
			}

			autoFileNames = append(autoFileNames, autoFileName)
		}(fileEntry)
	}

	// Resolve any deferred dynamic (anonymous struct) type markers in the sibling output using
	// the package registry, which now also contains the siblings' own lifted names.
	resolveDynamicTypeMarkers(autoFileNames)
}

// writeAutoConversionSibling writes the `.cs.auto` content prefixed with a stable banner that
// identifies the file as regenerated, non-compiled reference output (no timestamps — the sibling
// must stay byte-deterministic across runs).
func writeAutoConversionSibling(autoFileName string, baseName string, content string) error {
	banner := "// <auto-generated>" + "\r\n" +
		"//     go2cs auto-conversion of " + baseName + ".go — REFERENCE ONLY. The compiled " + baseName + ".cs is a manual" + "\r\n" +
		"//     conversion (marked go.GoManualConversion); this sibling is NOT compiled (projects include only" + "\r\n" +
		"//     *.cs) and is regenerated on every conversion for upgrade-time diff review. Do not edit." + "\r\n" +
		"// </auto-generated>" + "\r\n" + "\r\n"

	autoFile, err := os.Create(autoFileName)

	if err != nil {
		return fmt.Errorf("failed to create auto-conversion sibling file \"%s\": %s", autoFileName, err)
	}

	defer autoFile.Close()

	if _, err = autoFile.WriteString(banner + content); err != nil {
		return fmt.Errorf("failed to write to auto-conversion sibling file \"%s\": %s", autoFileName, err)
	}

	return nil
}
