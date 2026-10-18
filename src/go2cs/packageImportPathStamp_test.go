// packageImportPathStamp_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"path/filepath"
	"strings"
	"testing"
)

// The [GoPackage] ImportPath stamp: golib's package-path decoders rebuild a path from the namespace
// ('.' read as '/') plus the package name, which cannot carry a '.' inside a segment, a major-version
// directory, or a name that differs from its directory. The converter stamps the verbatim path
// exactly there, and REGENERATES the stamp on every write so a persisted package_info.cs can never
// keep a stale one.

func TestGoPackageImportPathStampNamesExactlyTheUnrecoverablePaths(t *testing.T) {
	cases := []struct {
		namespace, name, importPath, want string
	}{
		{"go.encoding", "gob", "encoding/gob", ""},
		{"go", "fmt", "fmt", ""},
		{"go.@internal", "abi", "internal/abi", ""}, // the '@' escape is a source spelling only
		{"go", "main", "main", ""},
		{"go.vendor.golang.org.x.crypto", "chacha20", "vendor/golang.org/x/crypto/chacha20", "vendor/golang.org/x/crypto/chacha20"},
		{"go.example.com.dotted", "dotted", "example.com/dotted/v2", "example.com/dotted/v2"},
		{"go.math.rand", "rand", "math/rand/v2", "math/rand/v2"},
		{"go.crypto.@internal", "fipsdeps", "crypto/internal/fips140deps", "crypto/internal/fips140deps"},
		{"go.encoding", "gob", "", ""}, // no path known: never stamp
	}

	for _, c := range cases {
		if got := goPackageImportPathStamp(c.namespace, c.name, c.importPath); got != c.want {
			t.Errorf("goPackageImportPathStamp(%q, %q, %q) = %q, want %q", c.namespace, c.name, c.importPath, got, c.want)
		}
	}
}

// withPackage sets the conversion globals the stamp classifier reads, restoring them afterwards.
func withPackage(t *testing.T, name, path string) {
	t.Helper()
	savedName, savedPath := packageName, currentPackagePath
	packageName, currentPackagePath = name, path
	t.Cleanup(func() { packageName, currentPackagePath = savedName, savedPath })
}

func TestConvergeGoPackageStampsReplacesAStalePathAndAddsAMissingOne(t *testing.T) {
	isolateConverterPathGlobals(t)
	withPackage(t, "dotted", "example.com/dotted/v2")

	for _, stale := range []string{
		`[GoPackage("dotted")]`,                                // written before the stamp existed
		`[GoPackage("dotted", ImportPath = "example/com/dotted")]`, // a stale path from an older rule
	} {
		lines := []string{"namespace go.example.com;", "", stale, "public static partial class dotted_package", "{", "}"}
		got := convergeGoPackageStamps(lines, "go.example.com")[2]
		want := `[GoPackage("dotted", ImportPath = "example.com/dotted/v2")]`

		if got != want {
			t.Errorf("converged %s to %s, want %s", stale, got, want)
		}
	}
}

func TestConvergeGoPackageStampsDropsAStampTheDerivationNoLongerNeeds(t *testing.T) {
	isolateConverterPathGlobals(t)
	withPackage(t, "gob", "encoding/gob")

	lines := []string{`[GoPackage("gob", ImportPath = "stale/path")]`, "public static partial class gob_package"}

	if got := convergeGoPackageStamps(lines, "go.encoding")[0]; got != `[GoPackage("gob")]` {
		t.Errorf("a recoverable package must converge to the plain stamp, got %s", got)
	}
}

func TestConvergeGoPackageStampsGivesEachTestVariantItsGoPath(t *testing.T) {
	isolateConverterPathGlobals(t)
	// The EXTERNAL variant writes package_test_info.cs, which also carries the production class and
	// the internal-test bridge: each class keeps its own Go path.
	withPackage(t, "dotted_test", "example.com/dotted/v2_test")

	lines := []string{
		`[GoPackage("dotted")]`, "public static partial class dotted_package",
		`[GoPackage("dotted")]`, "public static partial class dotted_internal_test_package",
		`[GoPackage("dotted_test")]`, "public static partial class dotted_test_package",
		`[GoPackage("other")]`, "public static partial class other_package", // not this conversion's
	}

	got := convergeGoPackageStamps(lines, "go.example.com")
	want := []string{
		`[GoPackage("dotted", ImportPath = "example.com/dotted/v2")]`,
		`[GoPackage("dotted", ImportPath = "example.com/dotted/v2")]`,
		`[GoPackage("dotted_test", ImportPath = "example.com/dotted/v2_test")]`,
		`[GoPackage("other")]`,
	}

	for i, w := range want {
		if got[i*2] != w {
			t.Errorf("class %s: got %s, want %s", strings.TrimPrefix(got[i*2+1], "public static partial class "), got[i*2], w)
		}
	}
}

func TestConvergeGoPackageStampsNamesPackageMainByGosRule(t *testing.T) {
	isolateConverterPathGlobals(t)
	withPackage(t, "main", "go2cs/DottedModulePath")

	lines := []string{`[GoPackage("main")]`, "public static partial class main_package"}

	if got := convergeGoPackageStamps(lines, "go")[0]; got != `[GoPackage("main")]` {
		t.Errorf("package main's reflect path is \"main\", so it needs no stamp; got %s", got)
	}
}

// withGorootVendored sets the per-package GOROOT-vendored flag, restoring it afterwards.
func withGorootVendored(t *testing.T, vendored bool) {
	t.Helper()
	saved := currentPackageGorootVendored
	currentPackageGorootVendored = vendored
	t.Cleanup(func() { currentPackageGorootVendored = saved })
}

func TestConvergeGoPackageStampsGivesAGorootVendoredPackageGosVendorPath(t *testing.T) {
	isolateConverterPathGlobals(t)
	// The loader reports $GOROOT/src/vendor/golang.org/x/net/idna as golang.org/x/net/idna. Go's
	// reflect path for it is vendor/golang.org/x/net/idna; the stamp must carry that path, and a
	// stamp written from the loader's path must converge to it.
	withPackage(t, "idna", "golang.org/x/net/idna")
	withGorootVendored(t, true)

	want := `[GoPackage("idna", ImportPath = "vendor/golang.org/x/net/idna")]`

	for _, stale := range []string{
		`[GoPackage("idna")]`,
		`[GoPackage("idna", ImportPath = "golang.org/x/net/idna")]`, // the loader's path, stamped before this rule
	} {
		lines := []string{stale, "public static partial class idna_package"}

		if got := convergeGoPackageStamps(lines, "go.vendor.golang.org.x.net")[0]; got != want {
			t.Errorf("converged %s to %s, want %s", stale, got, want)
		}
	}
}

func TestConvergeGoPackageStampsKeepsANonGorootModulesOwnPath(t *testing.T) {
	isolateConverterPathGlobals(t)
	// The control: the same path from a module OUTSIDE GOROOT (converting x/net itself) is not
	// vendored, and keeps the path the loader reports.
	withPackage(t, "idna", "golang.org/x/net/idna")
	withGorootVendored(t, false)

	lines := []string{`[GoPackage("idna")]`, "public static partial class idna_package"}
	want := `[GoPackage("idna", ImportPath = "golang.org/x/net/idna")]`

	if got := convergeGoPackageStamps(lines, "go.golang.org.x.net")[0]; got != want {
		t.Errorf("a non-vendored golang.org/x package: got %s, want %s", got, want)
	}
}

func TestGoReflectPackagePathPrefixesOnlyAnUnprefixedVendoredPath(t *testing.T) {
	isolateConverterPathGlobals(t)
	withGorootVendored(t, true)

	for loader, want := range map[string]string{
		"golang.org/x/net/idna":        "vendor/golang.org/x/net/idna",
		"vendor/golang.org/x/net/idna": "vendor/golang.org/x/net/idna", // already Go's form
		"":                             "",
	} {
		if got := goReflectPackagePath(loader); got != want {
			t.Errorf("goReflectPackagePath(%q) = %q, want %q", loader, got, want)
		}
	}
}

func TestIsGorootVendoredDirIsKeyedOnTheDirectory(t *testing.T) {
	goRoot := t.TempDir()
	outside := t.TempDir()

	cases := []struct {
		dir  string
		want bool
	}{
		{filepath.Join(goRoot, "src", "vendor", "golang.org", "x", "net", "idna"), true},
		{filepath.Join(goRoot, "src", "vendor", "golang.org", "x", "sys", "cpu"), true},
		{filepath.Join(goRoot, "src", "net", "http"), false},
		{filepath.Join(goRoot, "src", "cmd", "vendor", "golang.org", "x", "mod", "module"), false},
		{filepath.Join(outside, "golang.org", "x", "net", "idna"), false},
		{"", false},
	}

	for _, c := range cases {
		if got := isGorootVendoredDir(c.dir, goRoot); got != c.want {
			t.Errorf("isGorootVendoredDir(%q) = %v, want %v", c.dir, got, c.want)
		}
	}

	if isGorootVendoredDir(filepath.Join(goRoot, "src", "vendor", "golang.org"), "") {
		t.Errorf("an unknown GOROOT must never classify a directory as vendored")
	}
}
