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
	"os"
	"path/filepath"
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

// TestTestRecordInterfacesSeedsAGenericInterface: a record whose interface argument is a generic instantiation,
// `GoImplement<…, x_package.Checker<nint>>`, seeds the generic declaration Checker, so the declaration's emitted bases
// are walked. The converter emits a generic interface's structural bases from the generic declaration itself
// (getStructuralInterfaceBases takes the declared, uninstantiated type), so the walk looks up the same type.
// Here Checker's base q.J is the only reason the project needs q.
func TestTestRecordInterfacesSeedsAGenericInterface(t *testing.T) {
	dir := t.TempDir()
	writeModuleFiles(t, dir, map[string]string{
		"go.mod": "module example/genrec\n\ngo 1.23\n",
		"q/q.go": "package q\ntype J interface{ Close() error }\n",
		"x/x.go": "package x\n" +
			"import \"example/genrec/q\"\n" +
			"type Checker[T any] interface {\n\tClose() error\n\tCheck(T) bool\n}\n" +
			"var _ q.J\n",
		"main.go":      "package genrec\nimport \"example/genrec/x\"\nfunc Use[T any](c x.Checker[T]) error { return nil }\n",
		"main_test.go": "package genrec\nimport \"testing\"\nfunc TestNothing(t *testing.T) {}\n",
	})

	info := filepath.Join(dir, "package_info.cs")
	record := "[assembly: GoImplement<go.testing_package.T, go.example.genrec.x_package.Checker<nint>>]\n"
	if err := os.WriteFile(info, []byte(record), 0o644); err != nil {
		t.Fatal(err)
	}

	production := loadProductionForDir(t, dir)
	internal, external := loadTestVariantsForDir(t, dir)
	roots := []*packages.Package{production, internal, external}

	seeds := testRecordInterfaces(info, roots)
	if len(seeds) != 1 || seeds[0].Obj().Name() != "Checker" || seeds[0].Obj().Pkg().Path() != "example/genrec/x" {
		t.Fatalf("the record names x.Checker<nint>, so the generic declaration x.Checker must seed the walk; got %v", seeds)
	}

	got := declarationClosureImportsFrom(roots, nil, []string{production.PkgPath, "testing", "example/genrec/x"}, nil, nil, seeds)
	if !slices.Contains(got, "example/genrec/q") {
		t.Fatalf("Checker's structural base q.J needs q; got %v", got)
	}
}
