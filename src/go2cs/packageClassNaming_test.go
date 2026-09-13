// packageClassNaming_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/build"
	"path/filepath"
	"testing"
)

// The emitted package class is named for the Go PACKAGE, not for the last segment of its import
// path, and an importer has to spell it the same way the declaration does. The two sides disagreed
// for any package whose name differs from its directory and which the namespace builder skipped:
// the standard library used to be skipped wholesale, on the premise that a stdlib package is always
// named for its directory. crypto/x509/internal/macos is `package macOS`, so its importers emitted
// `macos_package` against a declared `macOS_package` — invisible until darwin was built at all,
// then CS0234 in crypto/x509's root_darwin.cs.
//
// This pins the rule rather than the one instance: when the import graph knows a package's name,
// that name is the class segment, wherever the package lives.
func TestImportedPackageClassFollowsPackageName(t *testing.T) {
	goRootSrc := filepath.Join(build.Default.GOROOT, "src")

	saved := importPackageDirs
	defer func() { importPackageDirs = saved }()

	tests := []struct {
		name       string
		importPath string
		pkgName    string
		pkgDir     string
		expected   string
	}{
		{
			name:       "stdlib package named differently from its directory",
			importPath: "crypto/x509/internal/macos",
			pkgName:    "macOS",
			pkgDir:     filepath.Join(goRootSrc, "crypto", "x509", "internal", "macos"),
			expected:   "crypto.x509.@internal.macOS_package",
		},
		{
			name:       "ordinary stdlib package keeps its path segment",
			importPath: "encoding/json",
			pkgName:    "json",
			pkgDir:     filepath.Join(goRootSrc, "encoding", "json"),
			expected:   "encoding.json_package",
		},
		{
			name:       "stdlib major-version directory follows the package name",
			importPath: "math/rand/v2",
			pkgName:    "rand",
			pkgDir:     filepath.Join(goRootSrc, "math", "rand", "v2"),
			expected:   "math.rand.rand_package",
		},
		{
			name:       "module dependency named differently from its directory",
			importPath: "github.com/mattn/go-isatty",
			pkgName:    "isatty",
			pkgDir:     filepath.Join("C:", "gopath", "pkg", "mod", "github.com", "mattn", "go-isatty"),
			expected:   "github.com.mattn.isatty_package",
		},
	}

	for _, test := range tests {
		t.Run(test.name, func(t *testing.T) {
			importPackageDirs = map[string]importedPackageMeta{
				test.importPath: {Dir: test.pkgDir, Name: test.pkgName},
			}

			if actual := convertImportPathToNamespace(test.importPath, PackageSuffix); actual != test.expected {
				t.Errorf("convertImportPathToNamespace(%q) = %q, want %q\n"+
					"  the importer's spelling of the package class must match the declaration's,\n"+
					"  which is named for the Go package (%q), not for the path tail",
					test.importPath, actual, test.expected, test.pkgName)
			}
		})
	}
}

// The major-version convention is the FALLBACK for when the import graph has no entry, and it has
// to keep working there — a /vN directory hosts the parent-named package.
func TestMajorVersionFallbackAppliesWithoutGraphMetadata(t *testing.T) {
	saved := importPackageDirs
	defer func() { importPackageDirs = saved }()

	importPackageDirs = map[string]importedPackageMeta{}

	const expected = "math.rand.rand_package"

	if actual := convertImportPathToNamespace("math/rand/v2", PackageSuffix); actual != expected {
		t.Errorf("convertImportPathToNamespace(\"math/rand/v2\") = %q, want %q with no graph entry",
			actual, expected)
	}
}
