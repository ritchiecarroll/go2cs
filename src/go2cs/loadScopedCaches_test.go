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

// TestLoadScopedCachesEmptyAtEachRoot holds the five caches keyed by one load's *types objects to
// the root hand-in: collectCaptureModeMethods must leave each one empty. Before resetLoadScopedCaches
// they lived for the whole process, and the converter's own go test grew to 11 GB on linux.
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

	root := &packages.Package{PkgPath: "example.com/root", Types: types.NewPackage("example.com/root", "root")}
	collectCaptureModeMethods(root)

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
