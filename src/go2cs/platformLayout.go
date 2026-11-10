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
// layout source folder (a GOOS, GOARCH or `<goos>-<arch>` key; platformLattice.go) rather than a
// nested package.
//
// The discriminator is the project file: every converted Go package directory holds exactly one
// `.csproj`, and a per-GOOS source folder holds none. That distinction is load-bearing, not
// decorative — `internal/syscall/windows` is a real converted package whose directory name IS a
// GOOS, and treating its sources as `internal/syscall`'s Windows variants would corrupt both.
func isPlatformSourceFolder(packageOutputPath string, name string) bool {
	if !isLayoutKeyFolderName(name) {
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

// platformLayoutDir returns the directory a converted package's artifact is written to: the first of
// the target's key folders (`<goos>-<arch>`, `<arch>`, `<goos>`; platformLayoutFolders) where the
// tree already carries THIS file, and the package directory itself otherwise. See rule 1 in the file
// header. A bare GOOS probes its GOOS folder alone, which is the rule as it read before the arch axis.
//
// The check is per FILE, not per directory, so a package that carries per-GOOS sources still writes
// its shared files flat — which is the whole of L3.
func platformLayoutDir(packageOutputPath string, target string, fileName string) string {
	for _, folder := range platformLayoutFolders(target) {
		// isPlatformSourceFolder first, and not merely "does <folder>/<file> exist": `internal/syscall`
		// holds a NESTED PACKAGE called `windows`, and a file name that happens to match one of its
		// sources would otherwise route this package's emission into that package's directory.
		if !isPlatformSourceFolder(packageOutputPath, folder) {
			continue
		}

		platformDir := filepath.Join(packageOutputPath, folder)

		if info, err := os.Stat(filepath.Join(platformDir, fileName)); err == nil && info.Mode().IsRegular() {
			return platformDir
		}
	}

	return packageOutputPath
}

// platformLayoutPath is platformLayoutDir with the file name joined back on — the form every
// emission site wants.
func platformLayoutPath(packageOutputPath string, target string, fileName string) string {
	return filepath.Join(platformLayoutDir(packageOutputPath, target, fileName), fileName)
}

// reviewSiblingLayoutPath returns where a whole-file hand-own's `<baseName>.cs.auto` review sibling is written: the
// target's key folder when the tree already holds the sibling there, and otherwise beside its hand-owned `.cs`, flat
// or per-GOOS. The first arm is a FLAT hand-own whose emission differs by GOOS (runtime/runtime2.cs): the multi-target
// merge keeps one sibling per key folder for it (mergeReviewSiblings), and a single-target reconvert must write its
// flavour back there rather than lay a flat sibling beside them.
func reviewSiblingLayoutPath(packageOutputPath string, target string, baseName string) string {
	sibling := baseName + ".cs.auto"

	if dir := platformLayoutDir(packageOutputPath, target, sibling); dir != packageOutputPath {
		return filepath.Join(dir, sibling)
	}

	return platformLayoutPath(packageOutputPath, target, baseName+".cs") + ".auto"
}

// platformPackageInfoPath resolves the `package_info.cs` a CONSUMER of an already-converted package
// should read — the mirror of platformLayoutDir, for the reading side.
//
// It exists because `package_info.cs` is closure-derived and therefore one of the artifacts L3
// routes per-GOOS (for a minority of the corpus; design §4.3), while the converter READS its dependencies'
// copies to mint each conversion's `<ImportedTypeAliases>` block and to learn their
// `[assembly: GoImplement]` records. A reader that only ever asked flat would, for those, find
// nothing and fall through to the DERIVED-alias path — no error, no warning, just a quietly
// different closure in every dependent. That is the same silent-empty failure mode a stale
// `-go2cspath` produced (CLAUDE.md, 2026-08-06), and it is worth the same care.
//
// Flat wins whenever it exists, so every package whose metadata is shared is untouched and this
// costs them one `os.Stat`. Otherwise the target's key folders are probed finest first, as
// platformLayoutDir probes them. When none holds it the FLAT path is returned, so every caller keeps
// its existing not-exist handling unchanged.
func platformPackageInfoPath(packageDir string, target string) string {
	flatPath := filepath.Join(packageDir, PackageInfoFileName)

	if info, err := os.Stat(flatPath); err == nil && info.Mode().IsRegular() {
		return flatPath
	}

	for _, folder := range platformLayoutFolders(target) {
		if !isPlatformSourceFolder(packageDir, folder) {
			continue
		}

		platformPath := filepath.Join(packageDir, folder, PackageInfoFileName)

		if info, err := os.Stat(platformPath); err == nil && info.Mode().IsRegular() {
			return platformPath
		}
	}

	return flatPath
}

// packageCarriesPlatformLayout reports whether a converted package directory holds any layout key
// source folder, i.e. whether its project file needs the conditioned <Compile Include>.
func packageCarriesPlatformLayout(packageOutputPath string) bool {
	return packageHoldsLayoutFolder(packageOutputPath, isLayoutKeyFolderName)
}

// packageCarriesArchLayout reports whether a converted package directory holds a GOARCH or
// `<goos>-<arch>` source folder, i.e. whether its project file also needs the $(GoTargetArch) items
// (applyPlatformArchLayoutBlocks). A package with GOOS folders alone never does, which is what keeps
// every project file of a single-architecture corpus exactly as it was.
func packageCarriesArchLayout(packageOutputPath string) bool {
	return packageHoldsLayoutFolder(packageOutputPath, isArchLayoutFolderName)
}

// packageHoldsLayoutFolder reports whether a package directory holds a layout source folder whose
// name the predicate admits.
func packageHoldsLayoutFolder(packageOutputPath string, admits func(name string) bool) bool {
	entries, err := os.ReadDir(packageOutputPath)

	if err != nil {
		return false
	}

	for _, entry := range entries {
		if entry.IsDir() && admits(entry.Name()) && isPlatformSourceFolder(packageOutputPath, entry.Name()) {
			return true
		}
	}

	return false
}

// isNestedPackageFolder reports whether a package directory's child folder is a NESTED PACKAGE: it
// holds a project file. A merge never writes into or removes from one, whatever its name, because
// its sources are another package's (`internal/syscall/windows`).
func isNestedPackageFolder(packageDir string, name string) bool {
	entries, err := os.ReadDir(filepath.Join(packageDir, name))

	if err != nil {
		return false
	}

	for _, entry := range entries {
		if !entry.IsDir() && strings.EqualFold(filepath.Ext(entry.Name()), ".csproj") {
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

// removePlatformLayoutBlocks is applyPlatformLayoutBlocks' inverse: it takes the three L3 fragments back out of a
// project file, and the arch fragments with them, giving the file a single-target writer renders for a package with no
// per-GOOS sources. A file without them is returned unchanged.
func removePlatformLayoutBlocks(projectFileContents string) string {
	projectFileContents = removePlatformArchLayoutBlocks(projectFileContents)

	for _, block := range []string{platformCompileBlock, platformPackageInfoBlock, platformPropertyBlock} {
		projectFileContents = strings.Replace(projectFileContents, block, "", 1)
	}

	return projectFileContents
}

const (
	// platformTargetArchProperty is the MSBuild property that selects a package's per-GOARCH and
	// per-pair sources, beside platformTargetOSProperty.
	platformTargetArchProperty = "GoTargetArch"

	// platformDefaultTargetArch is what $(GoTargetArch) resolves to when a build does not set it: the
	// architecture every flavour of the corpus was CONVERTED at, not the host's. A Mac build under
	// either RID compiles the darwin/amd64 conversion, as it does today.
	platformDefaultTargetArch = "amd64"

	// The arch fragments attach to the L3 block's own lines, so they are applied only after it.
	platformArchPackageInfoAnchor = platformPackageInfoBlock

	platformArchCompileAnchor = "    <Compile Include=\"$(" + platformTargetOSProperty + ")/*.cs\" Exclude=\"$(" + platformTargetOSProperty + ")/package_info.cs\" />\r\n"

	// package_info.cs stays among the first items whichever key holds it, so the import hooks'
	// module initializers keep running ahead of the package's own (platformPackageInfoAnchor).
	platformArchPackageInfoBlock = "    <Compile Include=\"$(GoTargetArch)/package_info.cs\" Condition=\"Exists('$(GoTargetArch)/package_info.cs')\" />\r\n" +
		"    <Compile Include=\"$(GoTargetOS)-$(GoTargetArch)/package_info.cs\" Condition=\"Exists('$(GoTargetOS)-$(GoTargetArch)/package_info.cs')\" />\r\n"

	platformArchCompileBlock = "    <!-- Layout L3 arch keys: sources every target of one GOARCH shares live in $(GoTargetArch)/, and those of\r\n" +
		"         one (GOOS, GOARCH) pair in $(GoTargetOS)-$(GoTargetArch)/ (docs/phase4/DESIGN-linux-arm64-flavour.md). -->\r\n" +
		"    <Compile Include=\"$(GoTargetArch)/*.cs\" Exclude=\"$(GoTargetArch)/package_info.cs\" />\r\n" +
		"    <Compile Include=\"$(GoTargetOS)-$(GoTargetArch)/*.cs\" Exclude=\"$(GoTargetOS)-$(GoTargetArch)/package_info.cs\" />\r\n"

	platformArchPropertyBlock = "  <!-- Selects which per-GOARCH and per-(GOOS, GOARCH) source folders this package compiles: the\r\n" +
		"       architecture the flavour was converted at, so an unset build takes `amd64`. -->\r\n" +
		"  <PropertyGroup Condition=\"'$(" + platformTargetArchProperty + ")'==''\">\r\n" +
		"    <" + platformTargetArchProperty + ">" + platformDefaultTargetArch + "</" + platformTargetArchProperty + ">\r\n" +
		"  </PropertyGroup>\r\n\r\n"

	// platformArchPresence is what applyPlatformArchLayoutBlocks' idempotence check looks for. It is
	// the whole attribute, quote included: `$(GoTargetOS)-$(GoTargetArch)/*.cs` ends with the bare glob.
	platformArchPresence = "Include=\"$(" + platformTargetArchProperty + ")/*.cs\""
)

// applyPlatformArchLayoutBlocks adds the $(GoTargetArch) property default and the per-GOARCH and
// per-pair <Compile> items to a project file that already carries the L3 block, for a package that
// holds an arch or pair folder (packageCarriesArchLayout). Each arch item sits right after its GOOS
// counterpart: package_info.cs among the first items, the source globs after the per-GOOS one, so a
// file that moves from flat or a GOOS folder into an arch or pair folder moves later in item order
// and nothing else does. Idempotent, and removable again byte for byte (removePlatformArchLayoutBlocks).
func applyPlatformArchLayoutBlocks(projectFileContents string, projectFileName string) string {
	if strings.Contains(projectFileContents, platformArchPresence) {
		return projectFileContents
	}

	compileAt := strings.Index(projectFileContents, platformArchCompileAnchor)
	packageInfoAt := strings.Index(projectFileContents, platformArchPackageInfoAnchor)
	propertyAt := strings.Index(projectFileContents, platformPropertyAnchor)

	if compileAt < 0 || packageInfoAt < 0 || propertyAt < 0 {
		// Without the L3 lines there is nothing to attach to, and the arch folders' sources would not
		// be compiled at all; worth a word rather than a silent short compile.
		showWarning("Project file \"%s\" carries per-GOARCH sources but has no $(%s) block to attach the $(%s) items to; per-GOARCH sources will NOT be compiled",
			projectFileName, platformTargetOSProperty, platformTargetArchProperty)

		return projectFileContents
	}

	// The same order applyPlatformLayoutBlocks inserts in, for the same reason: property < packageInfo
	// < compile in the file, so the last anchor goes first.
	contents := projectFileContents[:compileAt+len(platformArchCompileAnchor)] +
		platformArchCompileBlock +
		projectFileContents[compileAt+len(platformArchCompileAnchor):]

	contents = contents[:packageInfoAt+len(platformArchPackageInfoAnchor)] +
		platformArchPackageInfoBlock +
		contents[packageInfoAt+len(platformArchPackageInfoAnchor):]

	return contents[:propertyAt] + platformArchPropertyBlock + contents[propertyAt:]
}

// removePlatformArchLayoutBlocks is applyPlatformArchLayoutBlocks' inverse, for a package whose last arch or pair
// folder has gone while its GOOS folders stay. A file without the arch fragments is returned unchanged.
func removePlatformArchLayoutBlocks(projectFileContents string) string {
	for _, block := range []string{platformArchCompileBlock, platformArchPackageInfoBlock, platformArchPropertyBlock} {
		projectFileContents = strings.Replace(projectFileContents, block, "", 1)
	}

	return projectFileContents
}

// renderPlatformUniverseCheck renders the shared build check of the arch axis: an MSBuild target that fails a build
// whose ($(GoTargetOS), $(GoTargetArch)) is not a target of the universe. Without it a pair the corpus was never laid
// out for compiles silently short: a windows build at arm64 takes no `amd64/` folder and no `windows-arm64/` one. An
// unset property reads as its project-file default, as the conditioned items read it. Not yet written into any file:
// where the corpus carries it is a later decision.
func renderPlatformUniverseCheck(universe []string) string {
	conditions := make([]string, 0, len(universe))

	for _, target := range universe {
		conditions = append(conditions, "'$(_GoLayoutTarget)'!='"+target+"'")
	}

	return "  <!-- The (GOOS, GOARCH) pairs the converted standard library is laid out for (platformUniverse). -->\r\n" +
		"  <Target Name=\"GoCheckLayoutTarget\" BeforeTargets=\"CoreCompile\">\r\n" +
		"    <PropertyGroup>\r\n" +
		"      <_GoLayoutOS>$(" + platformTargetOSProperty + ")</_GoLayoutOS>\r\n" +
		"      <_GoLayoutOS Condition=\"'$(_GoLayoutOS)'==''\">" + platformDefaultTargetOS + "</_GoLayoutOS>\r\n" +
		"      <_GoLayoutArch>$(" + platformTargetArchProperty + ")</_GoLayoutArch>\r\n" +
		"      <_GoLayoutArch Condition=\"'$(_GoLayoutArch)'==''\">" + platformDefaultTargetArch + "</_GoLayoutArch>\r\n" +
		"      <_GoLayoutTarget>$(_GoLayoutOS)/$(_GoLayoutArch)</_GoLayoutTarget>\r\n" +
		"    </PropertyGroup>\r\n" +
		"    <Error Condition=\"" + strings.Join(conditions, " And ") + "\"\r\n" +
		"           Text=\"go2cs: $(_GoLayoutTarget) is not a target the converted standard library is laid out for (" +
		strings.Join(universe, ", ") + "); set GoTargetOS and GoTargetArch to one of them.\" />\r\n" +
		"  </Target>\r\n"
}
