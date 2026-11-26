// loadScopedCaches_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/ast"
	"go/types"
	"testing"

	"golang.org/x/tools/go/packages"
)

// TestLoadScopedCachesEmptyAtEachRoot holds the five caches keyed by one load's *types objects to the reset a
// root's work starts with: resetLoadScopedCaches must leave each one empty. Before it existed they lived for the
// whole process, and the converter's own go test grew to 11 GB on linux.
func TestLoadScopedCachesEmptyAtEachRoot(t *testing.T) {
	stale := types.NewPackage("example.com/stale", "stale")
	fn := types.NewFunc(0, stale, "F", types.NewSignatureType(nil, nil, nil, nil, nil, false))

	packageLock.Lock()
	foreignDerivedAliases = map[*types.Package][][2]string{stale: {{"A", "B"}}}
	importedGenericFuncDecls[stale] = map[*types.Func]*ast.FuncDecl{fn: {}}
	importedGenericFuncInfo[stale] = &types.Info{}
	packageLock.Unlock()

	declaredStructsLock.Lock()
	declaredStructsByPackage[stale] = []*types.Struct{types.NewStruct(nil, nil)}
	declaredStructsLock.Unlock()

	descriptorCompanionCacheLock.Lock()
	descriptorCompanionCache[fn] = []int{0}
	descriptorCompanionCacheLock.Unlock()

	resetLoadScopedCaches()

	sizes := map[string]int{
		"foreignDerivedAliases":    len(foreignDerivedAliases),
		"importedGenericFuncDecls": len(importedGenericFuncDecls),
		"importedGenericFuncInfo":  len(importedGenericFuncInfo),
		"declaredStructsByPackage": len(declaredStructsByPackage),
		"descriptorCompanionCache": len(descriptorCompanionCache),
	}

	for name, size := range sizes {
		if size != 0 {
			t.Errorf("%s holds %d entries after a new root: it keeps the previous load's type graph alive", name, size)
		}
	}
}

// TestLoadScopedCachesHoldAcrossTheRoutePrePass is the crossing P3 found (2026-11-26): R calls D.F[T], D.F calls
// O.G[T], and R does not import O. The route pre-pass asks for O.G's declaration with currentLoadedPackage re-pointed
// at D (newGenericRouteVisitor), where O is a direct import and the declaration is found. Emission asks again under R,
// where it is not. The two must agree, so nothing between them may empty the cache: with a reset in the root hand-in
// (collectCaptureModeMethods), the emission ask missed and kept a clause the pre-pass had decided without.
func TestLoadScopedCachesHoldAcrossTheRoutePrePass(t *testing.T) {
	savedRoot := currentLoadedPackage
	t.Cleanup(func() {
		currentLoadedPackage = savedRoot
		resetLoadScopedCaches()
	})

	dir := t.TempDir()
	writeModuleFiles(t, dir, map[string]string{
		"go.mod": "module example.com/cross\n\ngo 1.24\n",
		"o/o.go": "package o\n\nfunc G[T error](v T) string { return v.Error() }\n",
		"d/d.go": "package d\n\nimport \"example.com/cross/o\"\n\nfunc F[T error](v T) string { return o.G(v) }\n",
		"r/r.go": "package r\n\nimport \"example.com/cross/d\"\n\ntype e struct{}\n\nfunc (e) Error() string { return \"e\" }\n\nfunc Use() string { return d.F(e{}) }\n",
	})

	loaded, err := packages.Load(&packages.Config{Mode: packages.LoadAllSyntax, Dir: dir}, "./r")
	if err != nil || len(loaded) != 1 || len(loaded[0].Errors) != 0 {
		t.Fatalf("loading the fixture: %v %v", err, loaded)
	}

	root := loaded[0]
	dependency := root.Imports["example.com/cross/d"]

	if dependency == nil {
		t.Fatal("the fixture's root does not import d")
	}

	if _, importsO := root.Imports["example.com/cross/o"]; importsO {
		t.Fatal("the fixture's root imports o directly, so it cannot show the crossing")
	}

	g, _ := dependency.Imports["example.com/cross/o"].Types.Scope().Lookup("G").(*types.Func)

	if g == nil {
		t.Fatal("o.G not found")
	}

	// The root's work starts: the one reset, as processConversion and convertTestVariant make it.
	resetLoadScopedCaches()

	// The route pre-pass asks under the dependency.
	prePass, _ := newGenericRouteVisitor(dependency).genericFuncSource(g)

	if prePass == nil {
		t.Fatal("the pre-pass ask under d did not find o.G: the fixture does not reach the reader")
	}

	// The root hand-in, then emission asks under the root.
	collectCaptureModeMethods(root)
	emission, _ := (&Visitor{info: root.TypesInfo}).genericFuncSource(g)

	if emission != prePass {
		t.Errorf("the pre-pass found o.G's declaration and emission did not (got %v): one conversion takes two routes for one call; a cache was emptied between them", emission)
	}
}
