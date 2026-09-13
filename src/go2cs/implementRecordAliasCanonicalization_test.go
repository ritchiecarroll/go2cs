// implementRecordAliasCanonicalization_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"os"
	"path/filepath"
	"strings"
	"testing"

	"golang.org/x/tools/go/packages"
)

// seedPackageInfoFile writes a minimal package_info.cs carrying the four marker sections and a
// package class, the shape writePackageInfoFile expects to find.
func seedPackageInfoFile(t *testing.T, dir string, className string) string {
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
		"[GoPackage(\"" + className + "\")]",
		"public static partial class " + className + "_package",
		"{",
		"}",
	}, "\r\n")

	if err := os.WriteFile(fileName, []byte(seed), 0644); err != nil {
		t.Fatal(err)
	}

	return fileName
}

// goImplementLines returns the emitted `[assembly: GoImplement<…>]` lines, trimmed.
func goImplementLines(contents string) []string {
	var records []string

	for _, line := range strings.Split(contents, "\n") {
		line = strings.TrimSpace(line)

		if strings.HasPrefix(line, "[assembly: GoImplement<") {
			records = append(records, line)
		}
	}

	return records
}

// ONE (implementation, interface) pair must produce exactly ONE GoImplement record, whichever
// spellings the cast sites happened to register it under.
//
// The dedup that enforces this compares each registry key against a covered set built from
// exportedTypeAliases — whose values visitTypeSpec has already canonicalized, reverting any
// file-local import rename. The registry key, by contrast, keeps whatever the cast site rendered.
// os is the reached case: it aliases its `io` import to `Δio` (io is shadowed once io/fs is in the
// reference closure), so `unixDirent`→`fs.DirEntry` registers as `DirEntry` through os's own
// `type DirEntry = fs.DirEntry` AND as `Δio.fs_package.DirEntry` through the io/fs name — and
// neither compared equal to the canonical `io.fs_package.DirEntry` the covered set holds. Both
// records were emitted for the one pair, so ImplementGenerator composed `unixDirentжDirEntry` twice
// (CS0102, CS0111 ×9, CS8646 ×4), and the resulting FALSE collision made the adapter-name collision
// pass qualify one cast site to a third spelling, `unixDirentжfs_DirEntry`, that neither record
// produces (CS0246). Keying the comparison on the same canonicalization the emission applies is what
// makes the two sides comparable.
func TestImplementRecordDedupesAcrossRenamedImportSpelling(t *testing.T) {
	dir := t.TempDir()
	fileName := seedPackageInfoFile(t, dir, "os")

	resetPackageState(&packages.Package{})
	packageName = "os"
	packageNamespace = "go"

	exportedTypeAliases["DirEntry"] = "go.io.fs_package.DirEntry"

	// The SAME pair, registered under both spellings the two cast sites produce.
	interfaceImplementations["DirEntry"] = NewHashSet([]string{PointerPrefix + "<unixDirent>"})
	interfaceImplementations[ShadowVarMarker+"io.fs_package.DirEntry"] = NewHashSet([]string{PointerPrefix + "<unixDirent>"})

	writePackageInfoFile(fileName, false)

	data, err := os.ReadFile(fileName)

	if err != nil {
		t.Fatal(err)
	}

	records := goImplementLines(string(data))

	if len(records) != 1 {
		t.Fatalf("one (impl, interface) pair must emit ONE record, got %d:\n%s", len(records), strings.Join(records, "\n"))
	}

	// The ALIASED spelling wins — its simple name resolves through the package usings and keeps the
	// generator's last-dot-segment adapter naming. (Normalizing to the qualified form instead was
	// measured and rejected: it broke generator resolution and regressed os 8 errors → 77.)
	if want := "[assembly: GoImplement<unixDirent, DirEntry>(Pointer = true)]"; records[0] != want {
		t.Fatalf("the alias-spelled record must be the survivor:\n got %s\nwant %s", records[0], want)
	}
}

// The dedup must not fire on a pair the alias does NOT cover. This is the Windows `os` shape
// exactly: only the renamed-canonical spelling is registered, no alias-keyed record exists, so the
// qualified record is the ONLY record and must survive. Without this control, "emit one record"
// could be satisfied by dropping the wrong one — or by dropping both.
func TestImplementRecordKeepsQualifiedRecordWhenNoAliasRecordExists(t *testing.T) {
	dir := t.TempDir()
	fileName := seedPackageInfoFile(t, dir, "os")

	resetPackageState(&packages.Package{})
	packageName = "os"
	packageNamespace = "go"

	exportedTypeAliases["DirEntry"] = "go.io.fs_package.DirEntry"
	interfaceImplementations[ShadowVarMarker+"io.fs_package.DirEntry"] = NewHashSet([]string{"dirEntry"})

	writePackageInfoFile(fileName, false)

	data, err := os.ReadFile(fileName)

	if err != nil {
		t.Fatal(err)
	}

	records := goImplementLines(string(data))

	if len(records) != 1 {
		t.Fatalf("the sole record must survive, got %d:\n%s", len(records), strings.Join(records, "\n"))
	}

	if want := "[assembly: GoImplement<dirEntry, go.io.fs_package.DirEntry>]"; records[0] != want {
		t.Fatalf("the qualified record must survive unchanged:\n got %s\nwant %s", records[0], want)
	}
}

// Two GENUINELY different pairs must both survive — the dedup keys on (interface, implementation),
// so a second implementation of the same interface is not a duplicate.
func TestImplementRecordKeepsDistinctImplementationsOfOneInterface(t *testing.T) {
	dir := t.TempDir()
	fileName := seedPackageInfoFile(t, dir, "os")

	resetPackageState(&packages.Package{})
	packageName = "os"
	packageNamespace = "go"

	exportedTypeAliases["DirEntry"] = "go.io.fs_package.DirEntry"
	interfaceImplementations["DirEntry"] = NewHashSet([]string{PointerPrefix + "<unixDirent>"})
	interfaceImplementations[ShadowVarMarker+"io.fs_package.DirEntry"] = NewHashSet([]string{PointerPrefix + "<unixDirent>", PointerPrefix + "<otherDirent>"})

	writePackageInfoFile(fileName, false)

	data, err := os.ReadFile(fileName)

	if err != nil {
		t.Fatal(err)
	}

	records := goImplementLines(string(data))

	if len(records) != 2 {
		t.Fatalf("the duplicate must collapse while the distinct pair survives, got %d:\n%s", len(records), strings.Join(records, "\n"))
	}

	joined := strings.Join(records, "\n")

	if !strings.Contains(joined, "GoImplement<unixDirent, DirEntry>(Pointer = true)") {
		t.Errorf("the collapsed pair must keep its alias spelling:\n%s", joined)
	}

	if !strings.Contains(joined, "GoImplement<otherDirent, go.io.fs_package.DirEntry>(Pointer = true)") {
		t.Errorf("the uncovered pair must survive:\n%s", joined)
	}
}
