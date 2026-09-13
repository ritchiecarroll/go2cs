// aliasReferenceImports_test.go - Gbtc
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
	"testing"
)

// Blocker D: a test project's references are the DIRECT-import closure plus whatever
// aliasReferenceImports can recover by scanning the emitted `using` aliases, because
// DisableTransitiveProjectReferences hides everything else from the compile view. The scan matched
// only the ROOTED namespace token (`go.hash_package`), but a SINGLE-SEGMENT package emits its alias
// UNROOTED (`using hash = hash_package;` inside `namespace go.math.rand`, where C#'s outward lookup
// finds the class without a qualifier). math/rand/v2's chacha8_test.cs needs `hash` purely because
// `sha256.New()` RETURNS hash.Hash — the package appears in no import list — so the reference went
// missing and the build failed CS0246 on `hash_package`.
func TestAliasReferenceImportsMatchesUnrootedSingleSegmentAlias(t *testing.T) {
	previous := importPackageDirs
	t.Cleanup(func() { importPackageDirs = previous })

	importPackageDirs = map[string]importedPackageMeta{
		"hash":          {Name: "hash"},
		"hash/maphash":  {Name: "maphash"},
		"crypto/sha256": {Name: "sha256"},
		"io":            {Name: "io"},
	}

	infoFile := filepath.Join(t.TempDir(), "chacha8_test.cs")

	contents := "namespace go.math.rand;\r\n" +
		"using sha256 = crypto.sha256_package;\r\n" +
		"using hash = hash_package;\r\n" +
		"using static go.math.rand.rand_package;\r\n"

	if err := os.WriteFile(infoFile, []byte(contents), 0644); err != nil {
		t.Fatalf("write alias scan fixture: %v", err)
	}

	found := aliasReferenceImports([]string{infoFile}, "math/rand/v2", []string{"crypto/sha256"})

	if len(found) != 1 || found[0] != "hash" {
		t.Fatalf("aliasReferenceImports = %v, want [hash]", found)
	}
}

// A MULTI-SEGMENT package emits its alias UNROOTED when the enclosing namespace SHADOWS the root
// `go` (go/doc/comment's std_test.cs in `namespace go.go.doc`, internal/abi's abi_test.cs in
// `namespace go.@internal`): `using exec = os.exec_package;` relies on C# outward lookup to find
// `go.os.exec_package`. The scan built only ROOTED tokens (`go.os.exec_package`), and neither the
// Contains/HasSuffix(target, token) checks (target is the shorter unrooted tail) nor the
// single-segment bareTokens path covered it, so os/exec — reached purely because testenv.Command
// RETURNS *exec.Cmd — went unreferenced and the build failed CS0246 on `os`.
func TestAliasReferenceImportsMatchesUnrootedMultiSegmentAlias(t *testing.T) {
	previous := importPackageDirs
	t.Cleanup(func() { importPackageDirs = previous })

	importPackageDirs = map[string]importedPackageMeta{
		"os":               {Name: "os"},
		"os/exec":          {Name: "exec"},
		"internal/testenv": {Name: "testenv"},
	}

	infoFile := filepath.Join(t.TempDir(), "std_test.cs")

	contents := "namespace go.go.doc;\r\n" +
		"using testenv = @internal.testenv_package;\r\n" +
		"using exec = os.exec_package;\r\n" +
		"using os;\r\n"

	if err := os.WriteFile(infoFile, []byte(contents), 0644); err != nil {
		t.Fatalf("write alias scan fixture: %v", err)
	}

	found := aliasReferenceImports([]string{infoFile}, "go/doc/comment", []string{"internal/testenv"})

	if len(found) != 1 || found[0] != "os/exec" {
		t.Fatalf("aliasReferenceImports = %v, want [os/exec]", found)
	}
}

// The unrooted-tail match is anchored on the leading ".": `os.exec_package` must not match an
// unrelated token like `go.notos.exec_package`, so a package nothing references is never pulled in.
func TestAliasReferenceImportsUnrootedTailAnchoredOnSegmentBoundary(t *testing.T) {
	previous := importPackageDirs
	t.Cleanup(func() { importPackageDirs = previous })

	importPackageDirs = map[string]importedPackageMeta{
		"notos/exec": {Name: "exec"},
	}

	infoFile := filepath.Join(t.TempDir(), "only_exec.cs")

	if err := os.WriteFile(infoFile, []byte("using exec = os.exec_package;\r\n"), 0644); err != nil {
		t.Fatalf("write alias scan fixture: %v", err)
	}

	if found := aliasReferenceImports([]string{infoFile}, "go/doc/comment", nil); len(found) != 0 {
		t.Fatalf("aliasReferenceImports = %v, want no matches (os.exec must not imply notos/exec)", found)
	}
}

// B5: an emitted CONVERSION RECORD can name a package that appears in NO import list and in no
// `using` alias. os/signal's package_test_info.cs carries
// `[assembly: GoImplement<strings_package.Builder, io_package.Writer>(Pointer = true)]` — recorded
// because the test reaches `cmd.Stdout = &buf`, whose os/exec field type names io.Writer — so with
// only the alias scan, io went unreferenced and DisableTransitiveProjectReferences turned the
// attribute itself into CS0246 (plus a cascading go2cs-gen CS8785 once the interface failed to bind).
func TestAliasReferenceImportsMatchesConversionRecordPackages(t *testing.T) {
	previous := importPackageDirs
	t.Cleanup(func() { importPackageDirs = previous })

	importPackageDirs = map[string]importedPackageMeta{
		"io":      {Name: "io"},
		"strings": {Name: "strings"},
		"context": {Name: "context"},
	}

	infoFile := filepath.Join(t.TempDir(), "package_test_info.cs")

	contents := "namespace go.os;\r\n" +
		"[assembly: GoImplement<signalCtx, stringer>(Pointer = true)]\r\n" +
		"[assembly: GoImplement<strings_package.Builder, io_package.Writer>(Pointer = true)]\r\n"

	if err := os.WriteFile(infoFile, []byte(contents), 0644); err != nil {
		t.Fatalf("write record scan fixture: %v", err)
	}

	found := aliasReferenceImports([]string{infoFile}, "os/signal", []string{"context"})

	if len(found) != 2 || found[0] != "io" || found[1] != "strings" {
		t.Fatalf("aliasReferenceImports = %v, want [io strings]", found)
	}
}

// A record's qualifiers match through the SAME token shapes an alias target does — rooted
// multi-segment (`go.io.fs_package`) and unrooted multi-segment (emitted where the namespace
// shadows the root `go`) — and a NESTED generic argument is scanned whole.
func TestAliasReferenceImportsMatchesConversionRecordQualifierShapes(t *testing.T) {
	previous := importPackageDirs
	t.Cleanup(func() { importPackageDirs = previous })

	importPackageDirs = map[string]importedPackageMeta{
		"io/fs":          {Name: "fs"},
		"os/exec":        {Name: "exec"},
		"internal/abi":   {Name: "abi"},
		"unused/nomatch": {Name: "nomatch"},
	}

	infoFile := filepath.Join(t.TempDir(), "package_test_info.cs")

	contents := "namespace go.@internal;\r\n" +
		"[assembly: GoImplement<go.io.fs_package.PathError, error>(Pointer = true)]\r\n" +
		"[assembly: GoImplement<os.exec_package.ΔError, error>]\r\n" +
		"[assembly: GoImplicitConv<Δindirect<K, V>, ж<@internal.abi_package.ΔType>>]\r\n"

	if err := os.WriteFile(infoFile, []byte(contents), 0644); err != nil {
		t.Fatalf("write record scan fixture: %v", err)
	}

	found := aliasReferenceImports([]string{infoFile}, "os/signal", nil)

	if len(found) != 3 || found[0] != "internal/abi" || found[1] != "io/fs" || found[2] != "os/exec" {
		t.Fatalf("aliasReferenceImports = %v, want [internal/abi io/fs os/exec]", found)
	}
}

// Only a record's GENERIC ARGUMENT LIST is a bindable reference; its `(Pointer = true)` /
// `(ValueType = "…")` payload is metadata (the ValueType is a STRING) and must not pull in a
// project reference. The io/fs qualifier in the generic list is the positive control proving the
// scan ran at all.
func TestAliasReferenceImportsIgnoresConversionRecordAttributePayload(t *testing.T) {
	previous := importPackageDirs
	t.Cleanup(func() { importPackageDirs = previous })

	importPackageDirs = map[string]importedPackageMeta{
		"io/fs":   {Name: "fs"},
		"os/exec": {Name: "exec"},
	}

	infoFile := filepath.Join(t.TempDir(), "package_test_info.cs")

	contents := "[assembly: GoImplicitConv<go.io.fs_package.FileMode, nuint>(Inverted = true, ValueType = \"go.os.exec_package.ΔError\")]\r\n"

	if err := os.WriteFile(infoFile, []byte(contents), 0644); err != nil {
		t.Fatalf("write record scan fixture: %v", err)
	}

	found := aliasReferenceImports([]string{infoFile}, "os/signal", nil)

	if len(found) != 1 || found[0] != "io/fs" {
		t.Fatalf("aliasReferenceImports = %v, want [io/fs] (the ValueType payload is not a reference)", found)
	}
}

// The bare token is matched on a SEGMENT boundary: a substring test would let `hash_package` match
// `go.hash.maphash_package` and pull a reference to a package nothing actually uses.
func TestAliasReferenceImportsDoesNotMatchAcrossSegmentBoundaries(t *testing.T) {
	previous := importPackageDirs
	t.Cleanup(func() { importPackageDirs = previous })

	importPackageDirs = map[string]importedPackageMeta{
		"hash": {Name: "hash"},
	}

	infoFile := filepath.Join(t.TempDir(), "only_maphash.cs")

	if err := os.WriteFile(infoFile, []byte("using maphash = go.hash.maphash_package;\r\n"), 0644); err != nil {
		t.Fatalf("write alias scan fixture: %v", err)
	}

	if found := aliasReferenceImports([]string{infoFile}, "math/rand/v2", nil); len(found) != 0 {
		t.Fatalf("aliasReferenceImports = %v, want no matches (maphash must not imply hash)", found)
	}
}
