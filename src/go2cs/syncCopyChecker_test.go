// syncCopyChecker_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/ast"
	"go/parser"
	"go/token"
	"testing"
)

// TestSyncCopyCheckerIsHandOwned guards the one-method hand-own that makes sync.Cond's copy detector
// work. Go's copyChecker stores its own ADDRESS in itself and compares; converted, the
// compare-and-swap destination becomes `Ꮡ((uintptr)(c))`, which boxes a COPY — so the checker is
// never initialized and no copy is EVER reported (sync's TestCondCopy failed with
// `got <nil>, expect sync.Cond is copied`). Storing an address would be unsound anyway: the GC moves
// the box, so a compaction would make an untouched Cond look copied — a SPURIOUS panic, worse than no
// check. cond_impl.cs compares root-allocation identity instead. If this entry is ever dropped, the
// auto body silently returns and the guard for "a Cond must not be copied" is gone with it, so the
// registration is asserted directly — the surrounding methods (Wait/Signal/Broadcast) must NOT be
// listed, since only `check` is hand-owned.
func TestSyncCopyCheckerIsHandOwned(t *testing.T) {
	fileSet := token.NewFileSet()
	source := `package sync

type copyChecker uintptr

func (c *copyChecker) check() {}

func (c *Cond) Signal() {}
`

	file, err := parser.ParseFile(fileSet, "cond.go", source, parser.SkipObjectResolution)

	if err != nil {
		t.Fatalf("parse fixture: %v", err)
	}

	decls := map[string]*ast.FuncDecl{}

	for _, decl := range file.Decls {
		if funcDecl, ok := decl.(*ast.FuncDecl); ok {
			decls[funcDecl.Name.Name] = funcDecl
		}
	}

	if !isManualFuncDeclInPackage("sync", "windows", decls["check"]) {
		t.Error("sync copyChecker.check must be hand-owned: the auto conversion CASes a boxed copy, so it never detects a copied Cond")
	}

	if isManualFuncDeclInPackage("sync", "windows", decls["Signal"]) {
		t.Error("only copyChecker.check is hand-owned in sync/cond.go; Cond's own methods must stay auto-converted")
	}
}
