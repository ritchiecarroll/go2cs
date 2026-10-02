// golibInode64Renames_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package repoguard

import (
	"go/build"
	"os"
	"path/filepath"
	"regexp"
	"runtime"
	"sort"
	"strings"
	"testing"
)

// GOLIB'S $INODE64 LIST MUST BE GO'S LINKER LIST.
//
// Go's linker renames some libSystem imports on macOS/amd64 to their 64-bit-inode symbols
// (cmd/link/internal/ld/macho.go, "Some 64-bit functions have a $INODE64 or $INODE64$UNIX2003 suffix"):
// the bare x86_64 names are the LEGACY 32-bit-inode ABI. golib applies the same rename when it resolves
// a //go:cgo_import_dynamic record (GoCgoDynamicImports.Inode64RenamedSymbols), because binding the bare
// readdir_r read four empty, directory-typed names per directory on osx-x64 and sent filepath.WalkDir
// into an unbounded self-recursion (run 36976772120, 2026-10-02).
//
// The list is copied, so it can drift: a Go release that renames one more import would leave golib
// binding the legacy symbol with nothing failing until a mac row read garbage. This arm reads both lists
// -- the `case "..."` names inside macho.go's $INODE64 block, and the string literals of golib's array --
// and requires them equal.

var (
	golibInode64Array  = regexp.MustCompile(`(?s)Inode64RenamedSymbols\s*=\s*\[(.*?)\]`)
	goQuotedName       = regexp.MustCompile(`"([^"]+)"`)
	machoCaseLine      = regexp.MustCompile(`^\s*case\s+(.*):\s*$`)
	goArchCaseNames    = map[string]bool{"amd64": true, "arm64": true, "386": true, "arm": true}
	machoInode64Marker = "$INODE64"
)

// machoInode64Renames returns the symbol names in macho.go's $INODE64 rename block.
func machoInode64Renames(t *testing.T, source string) []string {
	t.Helper()

	start := strings.Index(source, "Some 64-bit functions have a")

	if start < 0 {
		t.Fatalf("macho.go no longer carries the $INODE64 rename comment; re-derive this arm against the linker")
	}

	block := source[start:]

	if end := strings.Index(block, "\n\t\t}\n"); end > 0 && strings.Contains(block[:end], machoInode64Marker) {
		block = block[:end]
	}

	var names []string

	for _, line := range strings.Split(block, "\n") {
		m := machoCaseLine.FindStringSubmatch(line)

		if m == nil {
			continue
		}

		for _, q := range goQuotedName.FindAllStringSubmatch(m[1], -1) {
			if !goArchCaseNames[q[1]] {
				names = append(names, q[1])
			}
		}
	}

	sort.Strings(names)

	return names
}

// golibInode64Renames returns the string literals of golib's Inode64RenamedSymbols array.
func golibInode64Renames(t *testing.T, source string) []string {
	t.Helper()

	m := golibInode64Array.FindStringSubmatch(source)

	if m == nil {
		t.Fatalf("golib's GoCgoDynamicImports.cs declares no Inode64RenamedSymbols array: the macOS/amd64 $INODE64 rename Go's linker applies is missing from golib's resolver")
	}

	var names []string

	for _, q := range goQuotedName.FindAllStringSubmatch(m[1], -1) {
		names = append(names, q[1])
	}

	sort.Strings(names)

	return names
}

func TestGolibInode64RenamesMatchGoLinker(t *testing.T) {
	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	macho, err := os.ReadFile(filepath.Join(goRoot, "src", "cmd", "link", "internal", "ld", "macho.go"))

	if err != nil {
		t.Fatalf("reading GOROOT's macho.go (the oracle): %v", err)
	}

	golib, err := os.ReadFile(filepath.Join(repoRootFromPackageDir(t), "src", "core", "golib", "GoCgoDynamicImports.cs"))

	if err != nil {
		t.Fatalf("reading golib's resolver: %v", err)
	}

	want := machoInode64Renames(t, string(macho))
	got := golibInode64Renames(t, string(golib))

	if len(want) == 0 {
		t.Fatalf("VACUOUS: no names parsed from macho.go's $INODE64 block")
	}

	if strings.Join(got, ",") != strings.Join(want, ",") {
		t.Errorf("golib's $INODE64 list %v != Go's linker list %v (macho.go): bind each listed libSystem import as <name>$INODE64 on macOS/amd64, exactly as the linker does", got, want)
	}

	t.Logf("$INODE64 renames: %v (Go linker) == %v (golib)", want, got)
}

// TestMachoInode64ParserFires is the arm's positive control on synthetic source shaped like macho.go.
func TestMachoInode64ParserFires(t *testing.T) {
	source := "x\n\t\t// Some 64-bit functions have a \"$INODE64\" suffix.\n\t\tif ok {\n" +
		"\t\t\tswitch n {\n\t\t\tcase \"alpha\":\n\t\t\t\tswitch arch {\n\t\t\t\tcase \"amd64\":\n\t\t\t\t\tset(n+\"$INODE64\")\n\t\t\t\t}\n" +
		"\t\t\tcase \"beta\", \"gamma\":\n\t\t\t\tswitch arch {\n\t\t\t\tcase \"amd64\":\n\t\t\t\t}\n\t\t\t}\n\t\t}\n" +
		"\tcase \"outside\":\n"

	if got := strings.Join(machoInode64Renames(t, source), ","); got != "alpha,beta,gamma" {
		t.Fatalf("machoInode64Renames = %q, want alpha,beta,gamma (and never the arch names or a case outside the block)", got)
	}
}
