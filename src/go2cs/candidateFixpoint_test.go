// candidateFixpoint_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// candidateFixpoint_test.go - Gbtc
//
//  Copyright © 2026, The go2cs Authors. All Rights Reserved.

package main

import (
	"slices"
	"testing"

	"golang.org/x/tools/go/packages"
)

// TestDeclarationClosureImportsScansPackagesTheBaseWalkReaches holds the cross-package candidate edge to a
// FIXPOINT with the interface-base walk. A package the base walk adds is visible to C# like any other
// referenced assembly, so its same-named methods are candidates too.
//
// The shape: the root takes a p.I; p imports q only for q.J, a structural base of I, so q is reached by the
// base walk and by nothing else. q declares `func (s *S) Commit() (r.Conn, error)`, and r is named nowhere
// else. The test calls `new(Tx).Commit()`, so C# weighs q's Commit and binds r.Conn: CS0012 without r.
func TestDeclarationClosureImportsScansPackagesTheBaseWalkReaches(t *testing.T) {
	dir := t.TempDir()
	writeModuleFiles(t, dir, map[string]string{
		"go.mod": "module example/fixpoint\n\ngo 1.23\n",
		"r/r.go": "package r\ntype Conn struct{ ID int }\n",
		"q/q.go": "package q\n" +
			"import \"example/fixpoint/r\"\n" +
			"type J interface{ Close() error }\n" +
			"type S struct{}\n" +
			"func (s *S) Commit() (r.Conn, error) { return r.Conn{}, nil }\n",
		"p/p.go": "package p\n" +
			"import \"example/fixpoint/q\"\n" +
			"type I interface {\n\tClose() error\n\tName() string\n}\n" +
			"var _ q.J\n",
		"main.go": "package fixpoint\n" +
			"import \"example/fixpoint/p\"\n" +
			"type Tx struct{}\n" +
			"func (t *Tx) Commit() error { return nil }\n" +
			"func Use(x p.I) string { return x.Name() }\n",
		"main_test.go": "package fixpoint\nimport \"testing\"\nfunc TestCommit(t *testing.T) { _ = new(Tx).Commit() }\n",
	})

	production := loadProductionForDir(t, dir)
	internal, external := loadTestVariantsForDir(t, dir)

	got := declarationClosureImportsFrom([]*packages.Package{production, internal, external}, nil,
		[]string{production.PkgPath, "testing", "example/fixpoint/p"}, nil, nil, nil)

	// The base walk alone must reach q; without that the arm tests nothing.
	if !slices.Contains(got, "example/fixpoint/q") {
		t.Fatalf("precondition: the interface-base walk does not reach q (p.I's base q.J); got %v", got)
	}

	if !slices.Contains(got, "example/fixpoint/r") {
		t.Fatalf("q is visible once the base walk adds it, and its `Commit() (r.Conn, error)` is a candidate of the "+
			"test's `new(Tx).Commit()`, so r must be surfaced; got %v", got)
	}
}
