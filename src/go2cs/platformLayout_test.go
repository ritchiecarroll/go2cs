// platformLayout_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards for layout L3's two rules (platformLayout.go) and for the logical-path folding the census
// and the merge classify by (platformEmit.go).
//
// The rules are small, but each one has a negative case that would corrupt the corpus if it flipped:
// a package whose own directory name is a GOOS (`internal/syscall/windows` is real) must never be
// read as its parent's Windows variants, and a package with no per-GOOS sources must emit exactly
// the project file it always did — the block belongs on 37 packages, not on all 304.

package main

import (
	"os"
	"path/filepath"
	"strings"
	"testing"
)

// writeTestFile creates a file (and its directory) with the given contents.
func writeTestFile(t *testing.T, filePath string, contents string) {
	t.Helper()

	if err := os.MkdirAll(filepath.Dir(filePath), 0755); err != nil {
		t.Fatalf("failed to create %q: %v", filepath.Dir(filePath), err)
	}

	if err := os.WriteFile(filePath, []byte(contents), 0644); err != nil {
		t.Fatalf("failed to write %q: %v", filePath, err)
	}
}

// newL3PackageDir builds the internal/goos shape: two shared files flat, per-GOOS sources in
// windows/linux/darwin folders, and the package's project file.
func newL3PackageDir(t *testing.T) string {
	t.Helper()

	packageDir := filepath.Join(t.TempDir(), "goos")

	writeTestFile(t, filepath.Join(packageDir, "internal.goos.csproj"), "<Project />")
	writeTestFile(t, filepath.Join(packageDir, "goos.cs"), "shared")
	writeTestFile(t, filepath.Join(packageDir, PackageInfoFileName), "shared")
	writeTestFile(t, filepath.Join(packageDir, "windows", "nonunix.cs"), "windows")
	writeTestFile(t, filepath.Join(packageDir, "windows", "zgoos_windows.cs"), "windows")
	writeTestFile(t, filepath.Join(packageDir, "linux", "unix.cs"), "linux")
	writeTestFile(t, filepath.Join(packageDir, "linux", "zgoos_linux.cs"), "linux")

	return packageDir
}

// TestPlatformLayoutRoutesOnlyExistingPerGoosFiles is layout adoption, rule 1: a single-target
// conversion reproduces the layout the tree already carries, file by file — shared files stay flat
// and per-GOOS ones return to their folder — so a plain reconvert of an L3 package cannot lay a
// duplicate beside the copy the .csproj is already compiling.
func TestPlatformLayoutRoutesOnlyExistingPerGoosFiles(t *testing.T) {
	packageDir := newL3PackageDir(t)

	tests := []struct {
		goos     string
		fileName string
		wantDir  string
	}{
		{"windows", "zgoos_windows.cs", filepath.Join(packageDir, "windows")},
		{"windows", "nonunix.cs", filepath.Join(packageDir, "windows")},
		{"windows", "goos.cs", packageDir},
		{"windows", PackageInfoFileName, packageDir},
		{"linux", "zgoos_linux.cs", filepath.Join(packageDir, "linux")},
		{"linux", "goos.cs", packageDir},
		// A file the tree does not carry per-GOOS lands flat, even in an L3 package: the routing is
		// per FILE, which is the whole of L3.
		{"windows", "brand_new.cs", packageDir},
		// The windows run must not adopt another platform's folder.
		{"windows", "unix.cs", packageDir},
		// An unknown or empty GOOS can only mean flat.
		{"", "zgoos_windows.cs", packageDir},
		{"notanos", "zgoos_windows.cs", packageDir},
	}

	for _, test := range tests {
		got := platformLayoutDir(packageDir, test.goos, test.fileName)

		if got != test.wantDir {
			t.Errorf("platformLayoutDir(%q, %q) = %q, want %q", test.goos, test.fileName, got, test.wantDir)
		}
	}
}

// TestPlatformLayoutIgnoresNestedPackageNamedForGOOS is the negative case that matters most:
// `internal/syscall/windows` is a converted PACKAGE whose directory name is a GOOS. Folding its
// sources into `internal/syscall` would corrupt both, so the discriminator is the project file every
// converted package directory holds and a per-GOOS source folder never does.
func TestPlatformLayoutIgnoresNestedPackageNamedForGOOS(t *testing.T) {
	parentDir := filepath.Join(t.TempDir(), "syscall")

	writeTestFile(t, filepath.Join(parentDir, "windows", "internal.syscall.windows.csproj"), "<Project />")
	writeTestFile(t, filepath.Join(parentDir, "windows", "syscall_windows.cs"), "nested package")

	if isPlatformSourceFolder(parentDir, "windows") {
		t.Error("a nested converted package (it holds a .csproj) was read as a per-GOOS source folder")
	}

	if packageCarriesPlatformLayout(parentDir) {
		t.Error("a package whose only GOOS-named subdirectory is a nested package must not claim the L3 layout")
	}

	if got := platformLayoutDir(parentDir, "windows", "syscall_windows.cs"); got != parentDir {
		t.Errorf("platformLayoutDir routed into a nested package directory: %q", got)
	}
}

// TestPackageCarriesPlatformLayout covers the csproj-block predicate, rule 2, including the
// no-source case: an empty (or non-.cs) GOOS-named folder is not a layout.
func TestPackageCarriesPlatformLayout(t *testing.T) {
	if !packageCarriesPlatformLayout(newL3PackageDir(t)) {
		t.Error("an L3 package directory must claim the layout")
	}

	flatDir := filepath.Join(t.TempDir(), "strconv")
	writeTestFile(t, filepath.Join(flatDir, "strconv.csproj"), "<Project />")
	writeTestFile(t, filepath.Join(flatDir, "atoi.cs"), "flat")

	if packageCarriesPlatformLayout(flatDir) {
		t.Error("a flat package must not claim the layout — the block belongs on 37 packages, not 304")
	}

	emptyFolderDir := filepath.Join(t.TempDir(), "empty")
	writeTestFile(t, filepath.Join(emptyFolderDir, "empty.csproj"), "<Project />")

	if err := os.MkdirAll(filepath.Join(emptyFolderDir, "linux"), 0755); err != nil {
		t.Fatalf("failed to create linux folder: %v", err)
	}

	if packageCarriesPlatformLayout(emptyFolderDir) {
		t.Error("a GOOS-named folder holding no .cs is not a per-GOOS source folder")
	}
}

// TestApplyPlatformLayoutBlocks proves the emitted project file selects the per-GOOS sources, keeps
// `windows` as the standing default, places the include AFTER the wildcard removal that would
// otherwise wipe it, and is idempotent — a reconvert of an L3 package neither duplicates the block
// nor strips it.
func TestApplyPlatformLayoutBlocks(t *testing.T) {
	rendered := applyPlatformLayoutBlocks(string(csprojTemplate), "internal.goos.csproj")

	if !strings.Contains(rendered, "<Compile Include=\"$(GoTargetOS)/*.cs\" Exclude=\"$(GoTargetOS)/package_info.cs\" />") {
		t.Fatal("the conditioned per-GOOS <Compile Include> was not emitted")
	}

	if !strings.Contains(rendered, "<GoTargetOS>windows</GoTargetOS>") {
		t.Error("the $(GoTargetOS) default is not windows — a plain build must reproduce the single-platform package")
	}

	removeAt := strings.Index(rendered, "<Compile Remove=\"**/*.cs\" />")
	includeAt := strings.Index(rendered, "<Compile Include=\"$(GoTargetOS)/*.cs\" Exclude=\"$(GoTargetOS)/package_info.cs\" />")

	if removeAt < 0 || includeAt < removeAt {
		t.Error("the per-GOOS include must follow <Compile Remove=\"**/*.cs\" />, which would otherwise remove it")
	}

	if propertyAt := strings.Index(rendered, "<GoTargetOS>"); propertyAt < 0 || propertyAt > includeAt {
		t.Error("the $(GoTargetOS) default should be declared above the item that reads it")
	}

	if second := applyPlatformLayoutBlocks(rendered, "internal.goos.csproj"); second != rendered {
		t.Error("applyPlatformLayoutBlocks is not idempotent — a reconvert would duplicate the block")
	}

	// A template with no anchor is left exactly as it came in (with a warning, not a mangling).
	const foreign = "<Project Sdk=\"Microsoft.NET.Sdk\">\r\n</Project>\r\n"

	if got := applyPlatformLayoutBlocks(foreign, "foreign.csproj"); got != foreign {
		t.Error("a template without the anchors must be returned unchanged")
	}
}

// TestNormalizeArtifactLogicalPaths guards the fold the classification is keyed by: a census of an
// L3 corpus must reproduce the census of the flat corpus it was built from, and a package whose own
// name is a GOOS must not be folded into its parent.
func TestNormalizeArtifactLogicalPaths(t *testing.T) {
	artifacts := map[string]artifactState{
		"internal/goos/internal.goos.csproj":                       {},
		"internal/goos/goos.cs":                                    {},
		"internal/goos/windows/zgoos_windows.cs":                   {},
		"internal/goos/linux/unix.cs":                              {},
		"internal/syscall/windows/internal.syscall.windows.csproj": {},
		"internal/syscall/windows/syscall_windows.cs":              {},
		"internal/syscall/windows/registry/registry.csproj":        {},
		"internal/syscall/windows/registry/key.cs":                 {},
	}

	normalizeArtifactLogicalPaths(artifacts)

	want := map[string]string{
		"internal/goos/goos.cs":                       "internal/goos/goos.cs",
		"internal/goos/windows/zgoos_windows.cs":      "internal/goos/zgoos_windows.cs",
		"internal/goos/linux/unix.cs":                 "internal/goos/unix.cs",
		"internal/syscall/windows/syscall_windows.cs": "internal/syscall/windows/syscall_windows.cs",
		"internal/syscall/windows/registry/key.cs":    "internal/syscall/windows/registry/key.cs",
	}

	for rawPath, wantLogical := range want {
		if got := artifacts[rawPath].logical; got != wantLogical {
			t.Errorf("logical path of %q = %q, want %q", rawPath, got, wantLogical)
		}
	}
}

// TestGoosOfTarget covers the one-line target split every routing call goes through.
func TestGoosOfTarget(t *testing.T) {
	for target, want := range map[string]string{
		"windows/amd64": "windows",
		"darwin/arm64":  "darwin",
		"linux":         "linux",
	} {
		if got := goosOfTarget(target); got != want {
			t.Errorf("goosOfTarget(%q) = %q, want %q", target, got, want)
		}
	}
}

// TestPackageInfoIsTheFirstCompileItem proves the ordering guarantee the forced-init import hooks
// depend on: Roslyn runs module initializers in compilation ITEM order, so package_info.cs has to be
// the first <Compile> item for "every import hook precedes every one of the package's own inits" to
// be deterministic rather than an alphabetical accident (a bare glob puts package_info.cs wherever
// its name happens to sort, which for log/slog is AFTER logger.cs and is exactly the nil-deref
// tests/Behavioral/NamedImportInitOrder exists to catch).
//
// Both layouts are checked from the one template, because one shape has to serve both: a flat
// package holds the file at its root, an L3 package holds it in the per-GOOS folder, and each item
// is Exists()-guarded so the other simply does not match. A platform-exclusive package built for a
// target it does not serve matches NEITHER, which is why the guard is Exists() and not an
// unconditional item — that item would be CS2001, not a no-op.
func TestPackageInfoIsTheFirstCompileItem(t *testing.T) {
	const (
		rootInclude    = "<Compile Include=\"package_info.cs\" Condition=\"Exists('package_info.cs')\" />"
		perGOOSInclude = "<Compile Include=\"$(GoTargetOS)/package_info.cs\" Condition=\"Exists('$(GoTargetOS)/package_info.cs')\" />"
		rootGlob       = "<Compile Include=\"*.cs\" Exclude=\"package_info.cs\" />"
	)

	flat := string(csprojTemplate)

	rootAt := strings.Index(flat, rootInclude)
	globAt := strings.Index(flat, rootGlob)

	if rootAt < 0 {
		t.Fatal("the flat package_info.cs <Compile Include> was not emitted; a flat package loses the ordering guarantee")
	}

	if globAt < 0 {
		t.Fatal("the root glob no longer carries Exclude=\"package_info.cs\"; without it the file is compiled TWICE (CS2002)")
	}

	if rootAt > globAt {
		t.Error("package_info.cs must precede the root glob, or its module initializers do not run first")
	}

	if removeAt := strings.Index(flat, "<Compile Remove=\"**/*.cs\" />"); removeAt < 0 || rootAt < removeAt {
		t.Error("the package_info.cs include must follow <Compile Remove=\"**/*.cs\" />, which would otherwise remove it")
	}

	// L3: the per-GOOS companion carries the guarantee for the 34 packages whose package_info.cs is
	// not at the root, and it too must land ahead of every glob.
	l3 := applyPlatformLayoutBlocks(flat, "os.csproj")

	perGOOSAt := strings.Index(l3, perGOOSInclude)

	if perGOOSAt < 0 {
		t.Fatal("the per-GOOS package_info.cs <Compile Include> was not emitted; every L3 package loses the ordering guarantee")
	}

	if l3GlobAt := strings.Index(l3, rootGlob); l3GlobAt < 0 || perGOOSAt > l3GlobAt {
		t.Error("the per-GOOS package_info.cs must precede the root glob")
	}

	perGOOSGlobAt := strings.Index(l3, "<Compile Include=\"$(GoTargetOS)/*.cs\" Exclude=\"$(GoTargetOS)/package_info.cs\" />")

	if perGOOSGlobAt < 0 || perGOOSAt > perGOOSGlobAt {
		t.Error("the per-GOOS package_info.cs must precede the per-GOOS source glob")
	}

	// The per-GOOS SOURCE glob still follows the root glob: this change reorders package_info.cs
	// only, and must not quietly reshuffle everything else's initializer order.
	if l3GlobAt := strings.Index(l3, rootGlob); l3GlobAt > perGOOSGlobAt {
		t.Error("the per-GOOS source glob moved ahead of the root glob; that reorders initializers this change has no business touching")
	}
}
