// handOwnTypeAccessibility_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// The sibling of handOwnReferences_test.go, and the file that exists because that one did not go far
// enough.
//
// `package_info.cs`'s <TypeAccessibility> section is REBUILT by a package conversion. Its entries
// come from packageEmittedTypeAccess — the `[GoType]` declarations this conversion pass emitted — and
// writePackageInfoFile carries the file's EXISTING entries only when mergeExisting is true, which
// conversionDriver passes as `!isDir`: a single-FILE conversion merges, a PACKAGE conversion does not.
//
// For a package that is hand-owned BY CONSEQUENCE — every production file carrying
// [module: GoManualConversion] — that derivation is EMPTY, because the `[GoType]` declarations live in
// the uncompiled `.cs.auto` siblings rather than in the compiled hand-written file. So a package
// re-mint emits the section's prose and markers intact and its BODY empty.
//
// That was invisible until the metadata un-freeze: while these packages' metadata was frozen a
// package re-mint never ran for them, so the declarations survived by accident rather than by design.
// Un-freezing them re-minted the file and dropped `public partial struct Pointer<T> {}` out of
// internal/weak, leaving a partial type whose only remaining part is bare — and therefore internal —
// while `Make<T>` returning it stayed public: CS0050 and CS0051, which unseated a train-47 seat.
//
// The remedy is the same one the csproj block uses and for the same reason: a DECLARATION a human
// states, placed OUTSIDE the section a re-mint rebuilds. Measured both ways before it was adopted — a
// sentinel INSIDE the markers is destroyed by a package conversion, one OUTSIDE them survives — so
// the mechanism needs no preservation code, only this guard to keep it true.

package main

import (
	"io/fs"
	"os"
	"path/filepath"
	"regexp"
	"strings"
	"testing"

	"golang.org/x/tools/go/packages"
)

const handOwnTypeAccessOpen = "// <GoHandOwnTypeAccessibility>"
const handOwnTypeAccessClose = "// </GoHandOwnTypeAccessibility>"

// seedPackageInfoWithBlocks writes a package_info.cs carrying BOTH an entry inside the rebuilt
// <TypeAccessibility> section and a declared block outside it, which is the state a re-mint of a
// hand-owned package meets.
func seedPackageInfoWithBlocks(t *testing.T, dir string) string {
	t.Helper()

	fileName := filepath.Join(dir, PackageInfoFileName)

	seed := strings.Join([]string{
		"namespace go;",
		"",
		"// <ImportedTypeAliases>",
		"// </ImportedTypeAliases>",
		"",
		"// <ExportedTypeAliases>",
		"// </ExportedTypeAliases>",
		"",
		"// <InterfaceImplementations>",
		"// </InterfaceImplementations>",
		"",
		"// <ImplicitConversions>",
		"// </ImplicitConversions>",
		"",
		"[GoPackage(\"weak\")]",
		"public static partial class weak_package {",
		"",
		"    // <TypeAccessibility>",
		"    public partial struct InsideTheRebuiltSection {}",
		"    // </TypeAccessibility>",
		"",
		"    " + handOwnTypeAccessOpen,
		"    public partial struct Pointer<T> {}",
		"    " + handOwnTypeAccessClose,
		"",
		"    // <ImportInitializers>",
		"    // </ImportInitializers>",
		"}",
		"",
	}, "\r\n")

	if err := os.WriteFile(fileName, []byte(seed), 0644); err != nil {
		t.Fatalf("failed to seed the package info fixture: %v", err)
	}

	return fileName
}

// TestADeclaredTypeAccessibilityBlockSurvivesAPackageReMint is the arm that pays for this file.
//
// It carries its OWN positive control rather than trusting that the re-mint did anything: the entry
// INSIDE <TypeAccessibility> must be gone. Without that half a passing test is indistinguishable from
// a writePackageInfoFile that never touched the file, which is exactly the vacuous green this seat's
// unseat was hiding behind.
func TestADeclaredTypeAccessibilityBlockSurvivesAPackageReMint(t *testing.T) {
	dir := t.TempDir()
	fileName := seedPackageInfoWithBlocks(t, dir)

	resetPackageState(&packages.Package{})
	packageName = "weak"
	packageNamespace = "go"

	// mergeExisting FALSE — the PACKAGE conversion path (conversionDriver passes !isDir).
	writePackageInfoFile(fileName, false)

	data, err := os.ReadFile(fileName)

	if err != nil {
		t.Fatal(err)
	}

	got := string(data)

	// POSITIVE CONTROL: the rebuilt section really was rebuilt.
	if strings.Contains(got, "InsideTheRebuiltSection") {
		t.Fatalf("the <TypeAccessibility> section was NOT rebuilt, so this test proves nothing about survival:\n%s", got)
	}

	// THE PROPERTY: the declared block and its declaration are untouched.
	if !strings.Contains(got, handOwnTypeAccessOpen) || !strings.Contains(got, handOwnTypeAccessClose) {
		t.Errorf("the declared %s block did not survive a package re-mint:\n%s", handOwnTypeAccessOpen, got)
	}

	if !strings.Contains(got, "public partial struct Pointer<T> {}") {
		t.Errorf("the declared accessibility entry did not survive a package re-mint:\n%s", got)
	}

	// Exactly one block: a re-mint must not duplicate what it does not own.
	if n := strings.Count(got, handOwnTypeAccessOpen); n != 1 {
		t.Errorf("expected exactly 1 declared block after the re-mint, got %d", n)
	}
}

// TestAPackageReMintInventsNoDeclaredTypeAccessibilityBlock keeps the mechanism a DECLARATION. A
// package_info.cs without the block must not gain one: the absence of a declaration is not an
// invitation to infer one, and an inferred block would be the guessing this design avoids.
func TestAPackageReMintInventsNoDeclaredTypeAccessibilityBlock(t *testing.T) {
	dir := t.TempDir()
	fileName := seedPackageInfoFile(t, dir, "weak")

	resetPackageState(&packages.Package{})
	packageName = "weak"
	packageNamespace = "go"

	writePackageInfoFile(fileName, false)

	data, err := os.ReadFile(fileName)

	if err != nil {
		t.Fatal(err)
	}

	if strings.Contains(string(data), handOwnTypeAccessOpen) {
		t.Errorf("a package re-mint INVENTED a %s block:\n%s", handOwnTypeAccessOpen, string(data))
	}
}

// TestNoHandOwnDuplicatesAnImportInitHook is guard 2, as a CORPUS CENSUS rather than a unit test —
// the shape the property actually has. `package_info.cs` is where import-init hooks belong: it is
// compiled FIRST (its own `<Compile Include="package_info.cs" />` ahead of the glob), so Roslyn
// orders its module initializers before the package's own inits. A hand-owned file that ALSO
// declares one produces two methods of one name in one partial class — CS0111.
//
// bcache is the measured case and the reason this exists: while its metadata was frozen,
// `package_info.cs` carried no hook, so `cache.cs` declared its own. The un-freeze restored the
// minted hook and the two collided, which is half of what unseated a train-47 seat. The hand-own's
// copy is the legacy; this census keeps a second one from being added back anywhere.
func TestNoHandOwnDuplicatesAnImportInitHook(t *testing.T) {
	root := repoRootFromPackageDir(t)
	coreDir := filepath.Join(root, "src", "core")

	if _, err := os.Stat(coreDir); err != nil {
		t.Skipf("no src/core at %s: %v", coreDir, err)
	}

	hookName := regexp.MustCompile(`\[GoInit\][^\r\n]*?\s(init\S+?)\s*\(`)

	packagesScanned, markedFilesScanned, duplicates := 0, 0, 0

	err := filepath.WalkDir(coreDir, func(path string, d fs.DirEntry, err error) error {
		if err != nil || !d.IsDir() {
			return err
		}

		infoPath := filepath.Join(path, PackageInfoFileName)
		infoBytes, readErr := os.ReadFile(infoPath)

		if readErr != nil {
			return nil
		}

		packagesScanned++

		minted := HashSet[string]{}

		for _, m := range hookName.FindAllStringSubmatch(string(infoBytes), -1) {
			minted.Add(m[1])
		}

		if len(minted.Keys()) == 0 {
			return nil
		}

		entries, dirErr := os.ReadDir(path)

		if dirErr != nil {
			return nil
		}

		for _, e := range entries {
			name := e.Name()

			if e.IsDir() || !strings.HasSuffix(name, ".cs") ||
				name == PackageInfoFileName || strings.HasSuffix(name, ".cs.auto") ||
				name == "package_test_info.cs" {
				continue
			}

			filePath := filepath.Join(path, name)
			marked, markErr := containsManualConversionMarker(filePath)

			if markErr != nil || !marked {
				continue
			}

			markedFilesScanned++
			fileBytes, fileErr := os.ReadFile(filePath)

			if fileErr != nil {
				continue
			}

			for _, m := range hookName.FindAllStringSubmatch(string(fileBytes), -1) {
				if minted.Contains(m[1]) {
					duplicates++
					t.Errorf("%s declares import-init hook %s, which %s also mints — CS0111",
						filePath, m[1], infoPath)
				}
			}
		}

		return nil
	})

	if err != nil {
		t.Fatalf("walking %s: %v", coreDir, err)
	}

	// REFUSE a vacuous pass: a census that scanned nothing reports clean for the wrong reason.
	if packagesScanned == 0 {
		t.Fatalf("scanned 0 packages under %s — the census is measuring nothing", coreDir)
	}

	if markedFilesScanned == 0 {
		t.Fatalf("scanned 0 marked files under %s — the predicate never fired, so a zero proves nothing", coreDir)
	}

	t.Logf("scanned %d packages, %d marked files, %d duplicate hooks", packagesScanned, markedFilesScanned, duplicates)
}
