// platformLayout.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// This file owns LAYOUT L3 — increment 2 of docs/phase4/DESIGN-multiplatform-corpus.md (ACCEPTED
// 2026-08-08: layout L3 + packaging (a)).
//
// L3 in one sentence: a converted package's files that are byte-identical on every target platform
// stay FLAT in the package directory, and everything else — a file whose content varies by GOOS, or
// one only some platforms emit at all — lives in a per-GOOS subfolder the .csproj selects with
// $(GoTargetOS).
//
//	src/core/internal/goos/goos.cs            shared by windows, linux and darwin
//	src/core/internal/goos/package_info.cs    shared
//	src/core/internal/goos/windows/nonunix.cs
//	src/core/internal/goos/windows/zgoos_windows.cs
//	src/core/internal/goos/linux/unix.cs
//	src/core/internal/goos/linux/zgoos_linux.cs
//	src/core/internal/goos/darwin/...
//
// Two rules live here, and they are deliberately the SAME rule read from two directions.
//
//  1. **Which folder a file is written to (layout adoption).** A conversion emits for exactly ONE
//     target, so it cannot compute the platform axis itself — that axis is a comparison of several
//     targets' emissions (design §4.2, and the reason increment 1 exists). What a single-target
//     conversion CAN do is honor a layout the output tree already carries: if the package directory
//     already holds `<goos>/<name>.cs`, that is where this target's `<name>.cs` belongs. So a plain
//     `go2cs -stdlib` reconvert of an L3 package REPRODUCES it, file for file, instead of laying a
//     flat duplicate beside the per-GOOS copy the .csproj is already compiling (which is a
//     duplicate-member build break, arrived at silently). The rule is the same class as the
//     `[module: GoManualConversion]` hand-own detector directly above it in conversionDriver: the
//     existing output participates in the emission decision, and it is precise rather than
//     heuristic — the file must already be there.
//
//  2. **Whether the .csproj carries the conditioned <Compile Include>.** Same question, asked of
//     the directory: a package that carries per-GOOS subfolders needs the `$(GoTargetOS)/*.cs`
//     include and the property default, and one that does not must emit exactly the project file it
//     always did. That keeps the block on 37 packages instead of all 304, and it means a `-tests`
//     rewrite (which re-emits the production .csproj) cannot silently strip it.
//
// Both predicates are pure functions of the output tree, so they are idempotent and a re-run is a
// no-op — which is what makes a seeded reconvert byte-identical against an L3 corpus.
//
// The multi-target run that PRODUCES the layout in the first place is platformEmit.go.

package main

import (
	"os"
	"path/filepath"
	"strings"
)

const (
	// platformTargetOSProperty is the MSBuild property that selects a package's per-GOOS sources.
	platformTargetOSProperty = "GoTargetOS"

	// platformDefaultTargetOS is what $(GoTargetOS) resolves to when a build does not set it. It is
	// `windows` rather than the host OS on purpose: the corpus that exists today IS the Windows
	// emission, so this default is what makes a plain `dotnet build` of an L3 package reproduce the
	// single-platform package it replaced. Defaulting to the host becomes correct when the other
	// platforms' corpora are proven to compile (design §12 increments 3-4), and is a one-line change
	// to this constant's PropertyGroup when it does.
	platformDefaultTargetOS = "windows"
)

// goosOfTarget returns the GOOS half of an `os/arch` target, or the whole value when it carries no
// separator (so a malformed target degrades to a name that simply matches no folder).
func goosOfTarget(target string) string {
	if goos, _, found := strings.Cut(target, "/"); found {
		return goos
	}

	return target
}

// isPlatformSourceFolder reports whether a directory ENTRY of a converted package directory is a
// per-GOOS source folder rather than a nested package.
//
// The discriminator is the project file: every converted Go package directory holds exactly one
// `.csproj`, and a per-GOOS source folder holds none. That distinction is load-bearing, not
// decorative — `internal/syscall/windows` is a real converted package whose directory name IS a
// GOOS, and treating its sources as `internal/syscall`'s Windows variants would corrupt both.
func isPlatformSourceFolder(packageOutputPath string, name string) bool {
	if !isKnownGOOS(name) {
		return false
	}

	entries, err := os.ReadDir(filepath.Join(packageOutputPath, name))

	if err != nil {
		return false
	}

	holdsSource := false

	for _, entry := range entries {
		if entry.IsDir() {
			continue
		}

		switch strings.ToLower(filepath.Ext(entry.Name())) {
		case ".csproj":
			return false
		case ".cs":
			holdsSource = true
		}
	}

	return holdsSource
}

// platformLayoutDir returns the directory a converted package's artifact is written to: the
// package's `<goos>` subfolder when the tree already carries THIS file there, and the package
// directory itself otherwise. See rule 1 in the file header.
//
// The check is per FILE, not per directory, so a package that carries per-GOOS sources still writes
// its shared files flat — which is the whole of L3.
func platformLayoutDir(packageOutputPath string, goos string, fileName string) string {
	// isPlatformSourceFolder first, and not merely "does <goos>/<file> exist": `internal/syscall`
	// holds a NESTED PACKAGE called `windows`, and a file name that happens to match one of its
	// sources would otherwise route this package's emission into that package's directory.
	if len(goos) == 0 || !isPlatformSourceFolder(packageOutputPath, goos) {
		return packageOutputPath
	}

	platformDir := filepath.Join(packageOutputPath, goos)

	if info, err := os.Stat(filepath.Join(platformDir, fileName)); err == nil && info.Mode().IsRegular() {
		return platformDir
	}

	return packageOutputPath
}

// platformLayoutPath is platformLayoutDir with the file name joined back on — the form every
// emission site wants.
func platformLayoutPath(packageOutputPath string, goos string, fileName string) string {
	return filepath.Join(platformLayoutDir(packageOutputPath, goos, fileName), fileName)
}

// platformPackageInfoPath resolves the `package_info.cs` a CONSUMER of an already-converted package
// should read — the mirror of platformLayoutDir, for the reading side.
//
// It exists because `package_info.cs` is closure-derived and therefore one of the artifacts L3
// routes per-GOOS (27 of them corpus-wide, design §4.3), while the converter READS its dependencies'
// copies to mint each conversion's `<ImportedTypeAliases>` block and to learn their
// `[assembly: GoImplement]` records. A reader that only ever asked flat would, for those 27, find
// nothing and fall through to the DERIVED-alias path — no error, no warning, just a quietly
// different closure in every dependent. That is the same silent-empty failure mode a stale
// `-go2cspath` produced (CLAUDE.md, 2026-08-06), and it is worth the same care.
//
// Flat wins whenever it exists, so the 275 packages whose metadata is shared are untouched and this
// costs them one `os.Stat`. When neither exists the FLAT path is returned, so every caller keeps its
// existing not-exist handling unchanged.
func platformPackageInfoPath(packageDir string, goos string) string {
	flatPath := filepath.Join(packageDir, PackageInfoFileName)

	if info, err := os.Stat(flatPath); err == nil && info.Mode().IsRegular() {
		return flatPath
	}

	if len(goos) == 0 || !isPlatformSourceFolder(packageDir, goos) {
		return flatPath
	}

	platformPath := filepath.Join(packageDir, goos, PackageInfoFileName)

	if info, err := os.Stat(platformPath); err == nil && info.Mode().IsRegular() {
		return platformPath
	}

	return flatPath
}

// packageCarriesPlatformLayout reports whether a converted package directory holds any per-GOOS
// source folder, i.e. whether its project file needs the conditioned <Compile Include>.
func packageCarriesPlatformLayout(packageOutputPath string) bool {
	entries, err := os.ReadDir(packageOutputPath)

	if err != nil {
		return false
	}

	for _, entry := range entries {
		if entry.IsDir() && isPlatformSourceFolder(packageOutputPath, entry.Name()) {
			return true
		}
	}

	return false
}

// The two .csproj fragments layout L3 adds, and the anchors they attach to. The anchors are the
// stock template's own lines; a caller-supplied -csproj template that lacks them gets no block (and
// a warning from applyPlatformLayoutBlocks) rather than a mangled project file.
const (
	platformCompileAnchor = "    <Compile Include=\"*.cs\" Exclude=\"package_info.cs\" />\r\n"

	platformCompileBlock = "    <!-- Layout L3: this package's platform-varying sources live in per-GOOS subfolders, so\r\n" +
		"         exactly one platform's copy joins the compilation (docs/phase4/DESIGN-multiplatform-corpus.md). -->\r\n" +
		"    <Compile Include=\"$(GoTargetOS)/*.cs\" Exclude=\"$(GoTargetOS)/package_info.cs\" />\r\n"

	// An L3 package keeps package_info.cs in the per-GOOS folder rather than at its root, so the
	// template's root-level first item never matches and this is the one that carries the ordering
	// guarantee for these packages. It attaches BEFORE the root glob — its whole purpose is to be
	// the compilation's first item — while the per-GOOS SOURCE glob above still attaches after it,
	// leaving the established root-then-per-GOOS order of everything else untouched. Both are
	// Exists()-guarded: a platform-exclusive package built for a target it does not serve (the
	// darwin-only crypto/x509/internal/macos under windows) has no folder to name, and an item
	// naming an absent file is CS2001 rather than a no-op.
	platformPackageInfoAnchor = "    <Compile Include=\"package_info.cs\" Condition=\"Exists('package_info.cs')\" />\r\n"

	platformPackageInfoBlock = "    <Compile Include=\"$(GoTargetOS)/package_info.cs\" Condition=\"Exists('$(GoTargetOS)/package_info.cs')\" />\r\n"

	platformPropertyAnchor = "  <ItemGroup>\r\n    <!-- Remove all .cs files, including those in sub-folders -->\r\n"

	platformPropertyBlock = "  <!-- Selects which per-GOOS source folder this package compiles. Exactly one must be chosen,\r\n" +
		"       so an unset build takes `windows`, the corpus reference target; a pack pass sets it\r\n" +
		"       explicitly per RID (-p:GoTargetOS=linux). -->\r\n" +
		"  <PropertyGroup Condition=\"'$(" + platformTargetOSProperty + ")'==''\">\r\n" +
		"    <" + platformTargetOSProperty + ">" + platformDefaultTargetOS + "</" + platformTargetOSProperty + ">\r\n" +
		"  </PropertyGroup>\r\n\r\n"
)

// applyPlatformLayoutBlocks adds the $(GoTargetOS) property default and the per-GOOS <Compile
// Include> to a rendered .csproj. It is idempotent — a project file that already carries the include
// is returned unchanged — so a reconvert of an L3 package neither duplicates nor strips it.
func applyPlatformLayoutBlocks(projectFileContents string, projectFileName string) string {
	if strings.Contains(projectFileContents, "$("+platformTargetOSProperty+")/*.cs") {
		return projectFileContents
	}

	compileAt := strings.Index(projectFileContents, platformCompileAnchor)
	packageInfoAt := strings.Index(projectFileContents, platformPackageInfoAnchor)
	propertyAt := strings.Index(projectFileContents, platformPropertyAnchor)

	if compileAt < 0 || packageInfoAt < 0 || propertyAt < 0 {
		// The package carries per-GOOS sources but this template has nowhere to say so; emitting the
		// project file unchanged would leave those sources out of the build entirely, which is worth a
		// word rather than a silent short compile.
		showWarning("Project file \"%s\" carries per-GOOS sources but its template has no $(%s) anchor; per-GOOS sources will NOT be compiled",
			projectFileName, platformTargetOSProperty)

		return projectFileContents
	}

	// Insert from the LAST anchor backwards so an earlier insertion cannot move a later one's offset.
	// File order is property < packageInfo < compile, so they are applied in reverse.
	contents := projectFileContents[:compileAt+len(platformCompileAnchor)] +
		platformCompileBlock +
		projectFileContents[compileAt+len(platformCompileAnchor):]

	contents = contents[:packageInfoAt+len(platformPackageInfoAnchor)] +
		platformPackageInfoBlock +
		contents[packageInfoAt+len(platformPackageInfoAnchor):]

	return contents[:propertyAt] + platformPropertyBlock + contents[propertyAt:]
}
