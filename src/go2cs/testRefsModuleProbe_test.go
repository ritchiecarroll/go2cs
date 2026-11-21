// testRefsModuleProbe_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"os"
	"slices"
	"sort"
	"strings"
	"testing"

	"golang.org/x/tools/go/packages"
)

// TestRefsClosureOverModuleTestPackages is an INSTRUMENT, not a gate: it skips unless GO2CS_REFS_MODULE_DIR names a
// module directory whose go.mod requires the packages in GO2CS_REFS_MODULE_PKGS (comma-separated import paths). For
// each package it runs the reference closure the -tests writer appends (declarationClosureImports, with no records)
// over the package and its test variants (linux/amd64, CGO_ENABLED=0, the -recurse -tests tags), from the package's
// direct imports as the already-referenced set, and logs what the closure adds.
//
// C1, 2026-11-20, for COORD: modernc.org/sqlite v1.62.0's root and pcache test packages, the two CS0012 walls P5 hit.
// At master a98c5175be..3c637731e3 the closure adds [hash io/fs] to the root and nothing to pcache; with the
// cross-package candidate edge it adds [hash io/fs math/big] and [database/sql/driver].
func TestRefsClosureOverModuleTestPackages(t *testing.T) {
	dir := os.Getenv("GO2CS_REFS_MODULE_DIR")
	if dir == "" {
		t.Skip()
	}
	tags := resolveBuildTags(false, true, true, true, false, nil)
	for _, path := range strings.Split(os.Getenv("GO2CS_REFS_MODULE_PKGS"), ",") {
		config := &packages.Config{
			Mode: packages.NeedName | packages.NeedFiles | packages.NeedCompiledGoFiles | packages.NeedImports |
				packages.NeedDeps | packages.NeedTypes | packages.NeedTypesInfo | packages.NeedSyntax | packages.NeedModule,
			Dir: dir, Env: append(os.Environ(), "GOOS=linux", "GOARCH=amd64", "CGO_ENABLED=0", "GOFLAGS=-mod=mod", "GOWORK=off"),
			BuildFlags: []string{"-tags=" + strings.Join(tags, ",")}, Tests: true,
		}
		loaded, err := packages.Load(config, path)
		if err != nil {
			t.Fatal(err)
		}
		byID := map[string]*packages.Package{}
		for _, p := range loaded {
			byID[p.ID] = p
		}
		production, internal, external := byID[path], byID[path+" ["+path+".test]"], byID[path+"_test ["+path+".test]"]
		if internal == nil {
			internal = production
		}
		roots := []*packages.Package{production, internal}
		if external != nil {
			roots = append(roots, external)
		}
		refs := map[string]bool{}
		for _, r := range roots {
			for imp := range r.Imports {
				if imp != "testing" && imp != path {
					refs[imp] = true
				}
			}
			for _, e := range r.Errors {
				t.Logf("%s load error: %v", r.ID, e)
			}
		}
		referenced := []string{path}
		for k := range refs {
			referenced = append(referenced, k)
		}
		sort.Strings(referenced)
		got := declarationClosureImports(roots, nil, referenced, nil, nil)
		t.Logf("READING %s: referenced has math/big=%v driver=%v; closure adds %v", path,
			slices.Contains(referenced, "math/big"), slices.Contains(referenced, "database/sql/driver"), got)
	}
}
