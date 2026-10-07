// aliasPublicization_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// AN EXPORTED ALIAS OF AN UNEXPORTED TYPE. logrus declares `type MutexWrap = mutexWrap` and testify
// `type CompareType = compareResult`. Every importer declares each exported alias of an import as a
// `global using` over its target (ImportedTypeAliases), so an internal target is CS0122 in every
// importing assembly, whether or not the alias is used. collectPublicizedTypes now publicizes what an
// exported PRODUCTION alias exposes.
//
// ⚠ THE NEGATIVE ARM IS THE TEST-FILE ALIAS. runtime's export_test.go declares `type G = g`, `type M =
// m`, `type Mutex = mutex`: the only consumer is the package's own tests, which already see internals.
// The -tests driver runs this pass over the white-box variant, whose scope holds those aliases; widening
// `g` there would make the test variant's `public partial struct g` contradict production's `internal
// partial struct g` (CS0262). The stdlib holds 12 such test-file aliases and no production one.
package main

import (
	"strings"
	"testing"

	"golang.org/x/tools/go/packages"
)

const aliasPublicizationFixture = `package alias

type mutexWrap struct{}

func (mw *mutexWrap) Lock() lockToken { return lockToken{} }

type lockToken struct{}

type compareResult int

type hiddenElem struct{}

type notExposed struct{}

// THE SUBJECT, logrus's shape: an exported alias directly over an unexported struct.
type MutexWrap = mutexWrap

// testify's shape: an exported alias over an unexported defined integer.
type CompareType = compareResult

// A COMPOSITE target: the alias exposes the element type.
type Elems = []*hiddenElem

// CONTROL: an UNEXPORTED alias exposes nothing to an importer.
type local = notExposed
`

// ARM 1, THE SUBJECT. Each exported alias's unexported target is publicized, a composite target is
// peeled to its element, and the cascade carries the target's exported method's result type.
func TestAnExportedAliasPublicizesItsUnexportedTarget(t *testing.T) {
	publicized := collectFixturePublicized(t, aliasPublicizationFixture)

	for _, name := range []string{"mutexWrap", "compareResult", "hiddenElem"} {
		if !publicized[name] {
			t.Errorf("an exported alias over %s must publicize it; publicized: %v", name, publicized)
		}
	}

	if !publicized["lockToken"] {
		t.Errorf("the publicized target's exported method result must cascade; publicized: %v", publicized)
	}
}

// ARM 2, THE CONTROL. An unexported alias gives an importer nothing to name.
func TestAnUnexportedAliasPublicizesNothing(t *testing.T) {
	publicized := collectFixturePublicized(t, aliasPublicizationFixture)

	if publicized["notExposed"] {
		t.Errorf("an UNEXPORTED alias exposes nothing and must not publicize its target; publicized: %v", publicized)
	}
}

// ARM 3, THE NEGATIVE ARM. The pass over the WHITE-BOX variant (production plus in-package test files,
// what the -tests driver hands it) must not publicize a target that only a test-file alias exposes.
func TestATestFileAliasPublicizesNothing(t *testing.T) {
	dir := t.TempDir()

	writeModuleFiles(t, dir, map[string]string{
		"go.mod":            "module example/export\n\ngo 1.23\n",
		"export.go":         "package export\n\ntype g struct{}\n\nfunc use() g { return g{} }\n",
		"export_test.go":    "package export\n\ntype G = g\n",
		"export_xx_test.go": "package export\n\nimport \"testing\"\n\nfunc TestG(t *testing.T) { _ = G{} }\n",
	})

	loaded, err := packages.Load(&packages.Config{Mode: packages.LoadAllSyntax, Dir: dir, Tests: true}, ".")
	if err != nil {
		t.Fatal(err)
	}

	var whitebox *packages.Package

	for _, pkg := range loaded {
		if pkg.Name == "export" && strings.HasSuffix(pkg.ID, ".test]") {
			whitebox = pkg
		}
	}

	if whitebox == nil || len(whitebox.Errors) > 0 {
		t.Fatalf("white-box variant not loaded: %v", loaded)
	}

	// The fixture is only a control if the alias is really in the scope the pass walks.
	if whitebox.Types.Scope().Lookup("G") == nil {
		t.Fatalf("the white-box scope must hold the test-file alias G")
	}

	resetPackageState(whitebox)
	packagePublicizedTypes = nil
	packagePublicizedLiftedTypes = nil

	collectPublicizedTypes(whitebox.Types, whitebox.Fset)

	if publicized := publicizedTypeNames(); publicized["g"] {
		t.Fatalf("an alias declared in a _test.go file must not publicize its target (CS0262 against production); publicized: %v", publicized)
	}
}
