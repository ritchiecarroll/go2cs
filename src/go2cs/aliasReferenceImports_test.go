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
	"strings"
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

// A bare single-segment target resolves from the namespace that EMITS it, as C# outward lookup does. fmt's
// `using strconv = strconv_package;` and `using math = math_package;` sit in `namespace go`, so they name
// go.strconv_package and go.math_package. The unrooted-tail shape matched ANY token ending in ".strconv_package",
// so fmt.tests gained references to internal/strconv (go.@internal.strconv_package) and internal/runtime/math that
// nothing uses: the same leaf, a different package.
func TestAliasReferenceImportsBareTargetResolvesFromItsNamespace(t *testing.T) {
	previous := importPackageDirs
	t.Cleanup(func() { importPackageDirs = previous })

	importPackageDirs = map[string]importedPackageMeta{
		"strconv":               {Name: "strconv"},
		"internal/strconv":      {Name: "strconv"},
		"math":                  {Name: "math"},
		"internal/runtime/math": {Name: "math"},
	}

	infoFile := filepath.Join(t.TempDir(), "format.cs")

	contents := "namespace go;\r\n" +
		"using strconv = strconv_package;\r\n" +
		"using math = math_package;\r\n"

	if err := os.WriteFile(infoFile, []byte(contents), 0644); err != nil {
		t.Fatalf("write alias scan fixture: %v", err)
	}

	if found := aliasReferenceImports([]string{infoFile}, "fmt", []string{"math", "strconv"}); len(found) != 0 {
		t.Fatalf("aliasReferenceImports = %v, want no matches (strconv_package in namespace go is go.strconv_package)", found)
	}
}

// The field-access record scan shares the matcher. fmt's package_info.cs records
// `[assembly: GoFieldAccess(typeof(strconv_package.NumError))]` at compilation-unit level, where the qualifier resolves
// from the root `go`, so it names strconv and never internal/strconv.
func TestFieldAccessReceiverImportsBareRecordTargetResolvesFromRoot(t *testing.T) {
	previous := importPackageDirs
	t.Cleanup(func() { importPackageDirs = previous })

	importPackageDirs = map[string]importedPackageMeta{
		"strconv":          {Name: "strconv"},
		"internal/strconv": {Name: "strconv"},
	}

	infoFile := filepath.Join(t.TempDir(), "package_info.cs")

	contents := "[assembly: GoFieldAccess(typeof(strconv_package.NumError))]\r\n" +
		"namespace go;\r\n"

	if err := os.WriteFile(infoFile, []byte(contents), 0644); err != nil {
		t.Fatalf("write record scan fixture: %v", err)
	}

	if found := fieldAccessReceiverImports(infoFile, "fmt", []string{"strconv"}); len(found) != 0 {
		t.Fatalf("fieldAccessReceiverImports = %v, want no matches (strconv_package names strconv, not internal/strconv)", found)
	}
}

// qualifierTargetNamesPackage answers for a conversion record, read at compilation-unit level: a bare single-segment
// qualifier names only the single-segment package, and every rooted and unrooted spelling of a multi-segment package
// still names it.
func TestQualifierTargetNamesPackageResolvesFromRoot(t *testing.T) {
	cases := []struct {
		target, importPath string
		want               bool
	}{
		{"strconv_package", "strconv", true},
		{"strconv_package", "internal/strconv", false},
		{"math_package", "internal/runtime/math", false},
		{"io.fs_package", "io/fs", true},
		{"go.io.fs_package", "io/fs", true},
		{"global::go.io.fs_package", "io/fs", true},
		{"go.io.fs_package.FileInfo", "io/fs", true},
		{"fs_package", "io/fs", false},
	}

	for _, c := range cases {
		if got := qualifierTargetNamesPackage(c.target, c.importPath); got != c.want {
			t.Errorf("qualifierTargetNamesPackage(%q, %q) = %v, want %v", c.target, c.importPath, got, c.want)
		}
	}
}

// The shapes the alias scan has met in emitted output must all still match once a bare target resolves from its
// namespace:
//   - a ROOTED target that continues past the package class (`go.@internal.abi_package.ΔKind`, sort's metadata);
//   - a ROOTED target that ends at it, `global::`-qualified or not (math/rand's default_test.cs);
//   - an UNROOTED multi-segment tail inside a namespace that shadows the root (go/doc/comment's std_test.cs);
//   - an UNROOTED single-segment target that outward lookup resolves to a MULTI-segment package: internal/godebug's
//     godebug.cs writes `using bisect = bisect_package;` inside `namespace go.@internal`, which binds
//     go.@internal.bisect_package. A rule that refused every single-segment tail would lose this one.
func TestAliasReferenceImportsKeepsEveryObservedShape(t *testing.T) {
	previous := importPackageDirs
	t.Cleanup(func() { importPackageDirs = previous })

	importPackageDirs = map[string]importedPackageMeta{
		"internal/abi":    {Name: "abi"},
		"os/exec":         {Name: "exec"},
		"internal/diff":   {Name: "diff"},
		"internal/bisect": {Name: "bisect"},
		"strings":         {Name: "strings"},
	}

	dir := t.TempDir()

	fixtures := map[string]string{
		"package_test_info.cs": "global using reflectliteꓸKind = go.@internal.abi_package.ΔKind;\r\nnamespace go;\r\n",
		"default_test.cs":      "namespace go.math;\r\nusing exec = global::go.os.exec_package;\r\n",
		"std_test.cs":          "namespace go.go.doc;\r\nusing diff = @internal.diff_package;\r\n",
		"godebug.cs":           "namespace go.@internal;\r\nusing bisect = bisect_package;\r\nusing strings = strings_package;\r\n",
	}

	var files []string

	for name, contents := range fixtures {
		path := filepath.Join(dir, name)

		if err := os.WriteFile(path, []byte(contents), 0644); err != nil {
			t.Fatalf("write alias scan fixture %s: %v", name, err)
		}

		files = append(files, path)
	}

	found := aliasReferenceImports(files, "internal/godebug", nil)
	want := []string{"internal/abi", "internal/bisect", "internal/diff", "os/exec", "strings"}

	if strings.Join(found, ",") != strings.Join(want, ",") {
		t.Fatalf("aliasReferenceImports = %v, want %v", found, want)
	}
}
