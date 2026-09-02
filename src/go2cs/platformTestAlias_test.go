// platformTestAlias_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

package main

import (
	"os"
	"path/filepath"
	"testing"
)

// The `-tests` write MERGES its package_test_info.cs (mergeExisting=true at every call site in
// testConversion.go) because it seeds from the production package_info.cs and then accumulates each
// test variant's additions. Layout L3 splits production metadata per GOOS and leaves
// package_test_info.cs FLAT — one file serving three flavours — so preservation carries a
// windows-minted alias into a linux run untouched, binding a type that flavour never declares.
//
// These assert the predicate in BOTH directions on one fixture, because a filter that drops the
// alias everywhere "fixes" linux by breaking windows and no gate on a Windows box would see it.

// seedPlatformPackage writes an L3 package: per-GOOS production package_info.cs files declaring the
// named types, and nothing else. Returns the package directory.
func seedPlatformPackage(t *testing.T, declarations map[string][]string) string {
	t.Helper()

	packageDir := t.TempDir()

	for goos, types := range declarations {
		flavourDir := filepath.Join(packageDir, goos)

		if err := os.MkdirAll(flavourDir, 0755); err != nil {
			t.Fatalf("create %s flavour dir: %v", goos, err)
		}

		contents := "namespace go;\r\n\r\npartial class syscall_package {\r\n"

		for _, name := range types {
			contents += "    public partial struct " + name + " {}\r\n"
		}

		contents += "}\r\n"

		if err := os.WriteFile(filepath.Join(flavourDir, PackageInfoFileName), []byte(contents), 0644); err != nil {
			t.Fatalf("write %s package_info.cs: %v", goos, err)
		}
	}

	return packageDir
}

// TestFlavourContradictedAliasesNamesOnlyPlatformVaryingTypes is the WINDOWS half and the LINUX half
// of the same predicate. syscall's real shape: ΔHandle and ΔSockaddr are declared by windows alone,
// while ΔErrno is declared by all three.
func TestFlavourContradictedAliasesNamesOnlyPlatformVaryingTypes(t *testing.T) {
	packageDir := seedPlatformPackage(t, map[string][]string{
		"windows": {"ΔHandle", "ΔSockaddr", "ΔErrno"},
		"linux":   {"ΔErrno"},
		"darwin":  {"ΔErrno"},
	})

	// LINUX: the two windows-only declarations are contradicted; the shared one is not.
	linux := flavourContradictedAliases(packageDir, "linux")

	for _, want := range []string{"ΔHandle", "ΔSockaddr"} {
		if !linux.Contains(want) {
			t.Errorf("flavourContradictedAliases(linux) missing %s — the linux `-tests` write would keep an alias to a type linux never declares (CS0426)", want)
		}
	}

	if linux.Contains("ΔErrno") {
		t.Errorf("flavourContradictedAliases(linux) names ΔErrno, which linux DOES declare — dropping it would break the alias everywhere")
	}

	// WINDOWS: nothing is contradicted, because windows declares everything the others do plus its
	// own. This is the half that fails if the predicate is written as "not declared here".
	if windows := flavourContradictedAliases(packageDir, "windows"); len(windows) != 0 {
		t.Errorf("flavourContradictedAliases(windows) = %v, want empty — windows declares ΔHandle and ΔSockaddr, so its own aliases must be PRESERVED", windows.Keys())
	}
}

// TestAliasContradictsFlavourMatchesOnlyTheAliasTarget guards the line predicate the merge applies.
// The merge's default is preservation, so an unrecognized line must be KEPT: this only ever
// subtracts what it can positively identify.
func TestAliasContradictsFlavourMatchesOnlyTheAliasTarget(t *testing.T) {
	contradicted := NewHashSet([]string{"ΔHandle"})

	dropped := []string{
		"global using syscallꓸHandle = go.syscall_package.ΔHandle;",
		"    global using syscallꓸHandle = go.syscall_package.ΔHandle;",
	}

	for _, line := range dropped {
		if !aliasContradictsFlavour(line, contradicted) {
			t.Errorf("aliasContradictsFlavour(%q) = false, want true", line)
		}
	}

	kept := []string{
		// a DIFFERENT target in the same package
		"global using syscallꓸErrno = go.syscall_package.ΔErrno;",
		// the exported-section form: strings, binds no type, and is not this predicate's business
		"[assembly: GoTypeAlias(\"Handle\", \"ΔHandle\")]",
		// a file-local using, not a global alias
		"using abi = go.internal.abi_package;",
		// prose and markers
		"// ΔHandle is windows-only",
		"",
	}

	for _, line := range kept {
		if aliasContradictsFlavour(line, contradicted) {
			t.Errorf("aliasContradictsFlavour(%q) = true, want false — the merge preserves what it cannot positively identify", line)
		}
	}
}

// TestFlavourContradictedAliasesIgnoresNonLayoutPackages keeps the predicate off the 275 packages
// whose metadata is shared: a package with no per-GOOS folder has one production info, so no alias
// in it can be flavour-specific. internal/buildcfg is the real instance — per-GOOS FOLDERS but a
// FLAT package_info.cs — and it is why the L3 census reconciled at 19 rather than 20.
func TestFlavourContradictedAliasesIgnoresNonLayoutPackages(t *testing.T) {
	flat := t.TempDir()

	if err := os.WriteFile(filepath.Join(flat, PackageInfoFileName), []byte("partial class x_package {\r\n    public partial struct ΔHandle {}\r\n}\r\n"), 0644); err != nil {
		t.Fatalf("write flat package_info.cs: %v", err)
	}

	if got := flavourContradictedAliases(flat, "linux"); len(got) != 0 {
		t.Errorf("flavourContradictedAliases(flat package) = %v, want empty", got.Keys())
	}

	if got := flavourContradictedAliases(t.TempDir(), ""); len(got) != 0 {
		t.Errorf("flavourContradictedAliases(no goos) = %v, want empty", got.Keys())
	}
}
