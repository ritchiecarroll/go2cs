// transitiveAliasRename_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/types"
	"testing"

	"github.com/ritchiecarroll/hashset"
)

// RED 9. A package reached only TRANSITIVELY — never imported by the package being converted — is still
// qualified by its name wherever one of its types is named (collectTypePackages records it and visitFile
// supplies its canonical using). computeImportAliasRenames tested only direct import names and explicit
// aliases, so such a name stayed bare where a same-named child namespace is visible: crypto/internal/hpke
// names fips140.Hash without importing crypto/internal/fips140, and `using fips140 = …` inside
// go.crypto.@internal is CS0576 against the go.crypto.@internal.fips140 child that its fips140/hkdf import
// makes visible.
func withCleanAliasRenameState(t *testing.T) {
	t.Helper()

	previousRenames, previousSegments := packageImportAliasRenames, packageImportLeadingSegments
	previousQualified, previousSibling := packageQualifiedNamespaces, siblingClosureImportPaths

	t.Cleanup(func() {
		packageImportAliasRenames, packageImportLeadingSegments = previousRenames, previousSegments
		packageQualifiedNamespaces, siblingClosureImportPaths = previousQualified, previousSibling
	})

	corpusReferenceClosureCache = nil
	corpusCsprojDirectRefs = nil

	packageImportAliasRenames = map[string]string{}
	packageImportLeadingSegments = map[string]bool{}
	packageQualifiedNamespaces = map[string]bool{}
	siblingClosureImportPaths = nil
}

// hpke's shape in the synthetic: the converted package lives in go.crypto.@internal, imports ONLY
// crypto/internal/fips140/hkdf, and hkdf imports crypto/internal/fips140. The fips140 NAME is reached
// transitively and its child namespace is visible, so it must be renamed like an import would be.
func TestTransitivelyReachedCollidingNameIsRenamed(t *testing.T) {
	withCleanAliasRenameState(t)

	fips140 := types.NewPackage("crypto/internal/fips140", "fips140")
	hkdf := types.NewPackage("crypto/internal/fips140/hkdf", "hkdf")
	hkdf.SetImports([]*types.Package{fips140})

	hpke := types.NewPackage("crypto/internal/hpke", "hpke")
	hpke.SetImports([]*types.Package{hkdf})

	const packageNS = RootNamespace + ".crypto.@internal"

	setShadowState(t, packageNS, nil)
	computeImportAliasRenames(nil, hpke, packageNS, "", "", false)

	if got, ok := packageImportAliasRenames["fips140"]; !ok || got != ShadowVarMarker+"fips140" {
		t.Fatalf("fips140 is reached only through hkdf and collides with go.crypto.@internal.fips140: want %q, got %q (renamed=%v)",
			ShadowVarMarker+"fips140", got, ok)
	}

	if got := importQualifier("fips140"); got != ShadowVarMarker+"fips140" {
		t.Fatalf("every render site reads importQualifier: want %q, got %q", ShadowVarMarker+"fips140", got)
	}

	// The DIRECT import keeps its existing, non-colliding spelling: no go.crypto.@internal.hkdf exists.
	if got, ok := packageImportAliasRenames["hkdf"]; ok {
		t.Fatalf("hkdf has no same-named child namespace: want no rename, got %q", got)
	}

	// A transitive package binds no using by itself, so it must not become a leading segment.
	if packageImportLeadingSegments["fips140"] {
		t.Fatalf("a transitively reached package must not be recorded as a bound leading segment")
	}
}

// The negative: a transitively reached name with NO same-named child namespace stays bare. A predicate
// that renamed every closure name passes the arm above and fails this one.
func TestTransitivelyReachedNonCollidingNameStaysBare(t *testing.T) {
	withCleanAliasRenameState(t)

	errorsPkg := types.NewPackage("errors", "errors")
	hkdf := types.NewPackage("crypto/internal/fips140/hkdf", "hkdf")
	hkdf.SetImports([]*types.Package{errorsPkg})

	hpke := types.NewPackage("crypto/internal/hpke", "hpke")
	hpke.SetImports([]*types.Package{hkdf})

	const packageNS = RootNamespace + ".crypto.@internal"

	setShadowState(t, packageNS, nil)
	computeImportAliasRenames(nil, hpke, packageNS, "", "", false)

	if got, ok := packageImportAliasRenames["errors"]; ok {
		t.Fatalf("errors has no go.crypto.@internal.errors child: want no rename, got %q", got)
	}

	if got := importQualifier("errors"); got != "errors" {
		t.Fatalf("a non-colliding transitive name renders bare: want %q, got %q", "errors", got)
	}
}

// The class PLANTED through the real loader and the real type renderer: package red9 imports ONLY
// example/red9/child/x, and names child.T and plain.T solely through inferred type arguments. The
// child/x import makes go.example.red9.child a visible namespace, so the type-reached `child` must
// render through its Δ-renamed qualifier; `plain` has no such child and must render bare.
const transitiveAliasFixtureRoot = `package red9

import "example/red9/child/x"

func use[T any](v T) T { return v }

// child.T and plain.T are named ONLY through inferred types: red9 imports neither package.
var made = use(x.Make())

var plainly = use(x.Plain())
`

const transitiveAliasFixtureX = `package x

import (
	"example/red9/child"
	"example/red9/plain"
)

func Make() child.T { return child.T{} }

func Plain() plain.T { return plain.T{} }
`

func TestTypeReachedCollidingPackageRendersRenamedQualifier(t *testing.T) {
	withCleanAliasRenameState(t)

	dir := t.TempDir()

	writeModuleFiles(t, dir, map[string]string{
		"go.mod":         "module example/red9\n\ngo 1.24\n",
		"red9.go":        transitiveAliasFixtureRoot,
		"child/child.go": "package child\n\ntype T struct{ V int }\n",
		"child/x/x.go":   transitiveAliasFixtureX,
		"plain/plain.go": "package plain\n\ntype T struct{ V int }\n",
	})

	production := loadProductionForDir(t, dir)

	// The premise the class rests on: neither package is a direct import of the converted package.
	for _, imp := range production.Types.Imports() {
		if imp.Name() == "child" || imp.Name() == "plain" {
			t.Fatalf("fixture premise broken: red9 must not import %q directly", imp.Path())
		}
	}

	const packageNS = RootNamespace + ".example.red9"

	setShadowState(t, packageNS, nil)
	computeImportAliasRenames(nil, production.Types, packageNS, "", "", false)

	visitor := &Visitor{info: production.TypesInfo, pkg: production.Types, newline: "\n", referencedForeignPackages: hashset.HashSet[string]{}, importQueue: hashset.HashSet[string]{}}
	scope := production.Types.Scope()

	made := scope.Lookup("made")
	plainly := scope.Lookup("plainly")

	if made == nil || plainly == nil {
		t.Fatalf("fixture vars not found: made=%v plainly=%v", made, plainly)
	}

	if got, want := visitor.getAliasQualifiedTypeName(made.Type(), false), ShadowVarMarker+"child.T"; got != want {
		t.Fatalf("a type-reached package whose child namespace is visible must render its renamed qualifier (a bare one is CS0576 at the supplied using): want %q, got %q", want, got)
	}

	if !visitor.referencedForeignPackages.Contains("example/red9/child") {
		t.Fatalf("the render must record the type-reached package so visitFile supplies its using")
	}

	if got, want := visitor.getAliasQualifiedTypeName(plainly.Type(), false), "plain.T"; got != want {
		t.Fatalf("a type-reached package with no visible child namespace renders bare: want %q, got %q", want, got)
	}
}
