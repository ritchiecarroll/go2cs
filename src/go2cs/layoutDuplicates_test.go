// layoutDuplicates_test.go - Gbtc
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
)

// Layout L3's invariant is that an artifact lives in exactly ONE place — flat when it is shared,
// under <goos>/ when it varies. The merge's plan loop maintains that for every path some target
// rewrote, but a path all three targets reproduce byte for byte is in no plan (needToWriteFile
// skips identical writes), so a corpus that already holds both copies keeps them forever. The
// emitted csproj compiles `*.cs` AND `$(GoTargetOS)/*.cs`, so the duplicate joins its own build:
// CS0579 on [assembly: GoPackage] and CS1537 on the global usings. Measured on darwin at
// vendor/golang.org/x/sys/cpu.

// writeTestFile lives in platformLayout_test.go, the other half of this file's subject.

func TestReconcileLayoutDuplicatesRetiresTheRedundantPerGoosCopy(t *testing.T) {
	coreDir := filepath.Join(t.TempDir(), "core")
	packageDir := filepath.Join(coreDir, "vendor", "cpu")

	const shared = "// package_info\n[assembly: GoPackage(\"cpu\")]\n"

	writeTestFile(t, filepath.Join(packageDir, "cpu.csproj"), "<Project />")
	writeTestFile(t, filepath.Join(packageDir, "package_info.cs"), shared)
	writeTestFile(t, filepath.Join(packageDir, "darwin", "package_info.cs"), shared)
	writeTestFile(t, filepath.Join(packageDir, "darwin", "cpu_darwin.cs"), "// genuinely darwin-only\n")
	writeTestFile(t, filepath.Join(packageDir, "linux", "hwcap_linux.cs"), "// genuinely linux-only\n")

	removed, err := reconcileLayoutDuplicates(coreDir, []string{"windows/amd64", "linux/amd64", "darwin/amd64"})

	if err != nil {
		t.Fatalf("reconcileLayoutDuplicates returned an error for an unambiguous duplicate: %v", err)
	}

	if removed != 1 {
		t.Errorf("removed = %d, want 1 (the redundant darwin/package_info.cs)", removed)
	}

	if _, err := os.Stat(filepath.Join(packageDir, "darwin", "package_info.cs")); !os.IsNotExist(err) {
		t.Error("the redundant per-GOOS copy survived, so the darwin build still compiles it twice")
	}

	// The shared copy is the one that stays, and the genuinely platform-varying sources beside it
	// are none of this pass's business.
	for _, kept := range []string{
		filepath.Join(packageDir, "package_info.cs"),
		filepath.Join(packageDir, "darwin", "cpu_darwin.cs"),
		filepath.Join(packageDir, "linux", "hwcap_linux.cs"),
	} {
		if _, err := os.Stat(kept); err != nil {
			t.Errorf("%s was removed but has no flat twin: %v", kept, err)
		}
	}
}

func TestReconcileLayoutDuplicatesRefusesToGuessWhenCopiesDiffer(t *testing.T) {
	coreDir := filepath.Join(t.TempDir(), "core")
	packageDir := filepath.Join(coreDir, "vendor", "cpu")

	writeTestFile(t, filepath.Join(packageDir, "cpu.csproj"), "<Project />")
	writeTestFile(t, filepath.Join(packageDir, "package_info.cs"), "// flat flavor\n")
	writeTestFile(t, filepath.Join(packageDir, "darwin", "package_info.cs"), "// darwin flavor\n")

	removed, err := reconcileLayoutDuplicates(coreDir, []string{"windows/amd64", "darwin/amd64"})

	if err == nil {
		t.Fatal("a differing flat/per-GOOS pair was silently resolved; the merge has no emission " +
			"data for it and either choice breaks a platform, so it must fail loudly instead")
	}

	if removed != 0 {
		t.Errorf("removed = %d, want 0 — nothing may be deleted on the ambiguous path", removed)
	}

	for _, path := range []string{"core/vendor/cpu/package_info.cs", "core/vendor/cpu/darwin/package_info.cs"} {
		if !strings.Contains(err.Error(), path) {
			t.Errorf("the error does not name %s, so it cannot be acted on:\n%s", path, err)
		}
	}
}

// `internal/syscall/windows` is a real PACKAGE whose directory is named for a GOOS. Its sources are
// not a platform variant of its parent's, and a bare directory-name test would delete them.
func TestReconcileLayoutDuplicatesLeavesAPackageNamedForAGoosAlone(t *testing.T) {
	coreDir := filepath.Join(t.TempDir(), "core")
	parentDir := filepath.Join(coreDir, "internal", "syscall")
	childDir := filepath.Join(parentDir, "windows")

	const contents = "// same name, different package\n"

	writeTestFile(t, filepath.Join(parentDir, "syscall.csproj"), "<Project />")
	writeTestFile(t, filepath.Join(parentDir, "package_info.cs"), contents)
	writeTestFile(t, filepath.Join(childDir, "windows.csproj"), "<Project />")
	writeTestFile(t, filepath.Join(childDir, "package_info.cs"), contents)

	removed, err := reconcileLayoutDuplicates(coreDir, []string{"windows/amd64"})

	if err != nil {
		t.Fatalf("unexpected error: %v", err)
	}

	if removed != 0 {
		t.Errorf("removed = %d, want 0 — a nested package is not its parent's platform folder", removed)
	}

	if _, err := os.Stat(filepath.Join(childDir, "package_info.cs")); err != nil {
		t.Errorf("a nested package's own source was deleted as if it were a platform duplicate: %v", err)
	}
}
