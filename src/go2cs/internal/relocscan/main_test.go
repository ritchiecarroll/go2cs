// main_test.go - Gbtc
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

func writeFile(t *testing.T, path, text string) {
	t.Helper()

	if err := os.MkdirAll(filepath.Dir(path), 0o755); err != nil {
		t.Fatal(err)
	}

	if err := os.WriteFile(path, []byte(text), 0o644); err != nil {
		t.Fatal(err)
	}
}

// relocationFixture builds the arc-5 shape in two synthetic GOROOTs: TestAtoi leaves package src for package dst,
// reading the table atoiTests and the helper check, and declaring a local named ok that shares a package-level
// name in src. It returns the two roots and the arcs TSV.
func relocationFixture(t *testing.T, tableAtTarget bool) (string, string, string) {
	root := t.TempDir()
	from := filepath.Join(root, "from")
	to := filepath.Join(root, "to")

	writeFile(t, filepath.Join(from, "src", "src", "src.go"), "package src\n\nvar ok = true\n\nfunc check(int) bool { return ok }\n")
	writeFile(t, filepath.Join(from, "src", "src", "atoi_test.go"), `package src

import "testing"

var atoiTests = []int{1, 2}

func TestAtoi(t *testing.T) {
	for _, v := range atoiTests {
		ok := check(v)
		if !ok {
			t.Errorf("%d", v)
		}
	}
}

func TestStays(t *testing.T) {}
`)

	writeFile(t, filepath.Join(to, "src", "src", "src.go"), "package src\n\nvar ok = true\n")
	writeFile(t, filepath.Join(to, "src", "src", "stays_test.go"), "package src\n\nimport \"testing\"\n\nfunc TestStays(t *testing.T) {}\n")

	dstTest := "package dst\n\nimport \"testing\"\n\nfunc check(int) bool { return true }\n\nfunc TestAtoi(t *testing.T) {}\n"

	if tableAtTarget {
		dstTest += "\nvar atoiTests = []int{1, 2}\n"
	}

	writeFile(t, filepath.Join(to, "src", "dst", "atoi_test.go"), dstTest)

	arcs := filepath.Join(root, "relocations.tsv")
	writeFile(t, arcs, "# a comment\nsource\ttarget\nsrc\tdst\n")

	return from, to, arcs
}

// TestRelocationRefusedWhenATableStaysBehind is the red arm: the moved test's table is not declared at the target.
func TestRelocationRefusedWhenATableStaysBehind(t *testing.T) {
	from, to, arcs := relocationFixture(t, false)

	refused, err := run(from, to, arcs)

	if err != nil {
		t.Fatal(err)
	}

	if refused != 1 {
		t.Fatalf("refused = %d, want 1: atoiTests is declared nowhere in dst", refused)
	}
}

// TestRelocationPassesWhenTheTestBringsItsTable is the green arm. The local ok must not read as a reference to the
// package-level ok, which dst does not declare.
func TestRelocationPassesWhenTheTestBringsItsTable(t *testing.T) {
	from, to, arcs := relocationFixture(t, true)

	refused, err := run(from, to, arcs)

	if err != nil {
		t.Fatal(err)
	}

	if refused != 0 {
		t.Fatalf("refused = %d, want 0", refused)
	}
}

func TestReferencedDeclsSkipsLocalsSelectorsAndKeys(t *testing.T) {
	from, _, _ := relocationFixture(t, false)

	info := readPkg(from, "src")
	got := referencedDecls(info.tests["TestAtoi"], info.decls)

	if len(got) != 2 || got[0] != "atoiTests" || got[1] != "check" {
		t.Fatalf("referencedDecls = %v, want [atoiTests check]", got)
	}
}
