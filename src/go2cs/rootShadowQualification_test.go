// rootShadowQualification_test.go - Gbtc
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
	"runtime"
	"slices"
	"strings"
	"testing"

	"github.com/ritchiecarroll/hashset"
)

// Guards for the `go.go` root-shadow qualification and the record/reference canonicalization the
// -tests pipeline depends on. All four defects these cover are invisible to the behavioral corpus
// (it never runs `-tests` and no behavioral package imports a `go/*` package), so they are guarded
// here at the unit level instead. Each was proven to fail before its fix — see the per-test notes.

// setShadowState installs the package-scoped globals rootQualified/rootNamespaceShadowed read, and
// restores them when the test ends.
func setShadowState(t *testing.T, namespace string, childNamespaces map[string]bool) {
	t.Helper()

	previousNamespace, previousChildren := packageNamespace, packageChildNamespaces

	t.Cleanup(func() {
		packageNamespace, packageChildNamespaces = previousNamespace, previousChildren
	})

	packageNamespace = namespace

	if childNamespaces == nil {
		childNamespaces = map[string]bool{}
	}

	packageChildNamespaces = childNamespaces
}

// Blocker A(b): a using-directive target composed DIRECTLY from packageNamespace (the -tests
// production-class anchor and the test host's `using go.testing_runtime;`) bypassed rootQualified
// entirely, so its leading `go` re-bound to the `go.go` namespace a go/* import contributes —
// math/rand/v2's regress_test.go imports go/format, and the resulting CS0234 accounted for 13 of
// the package's 22 compile errors. Before globalQualifyRooted these targets emitted bare.
func TestGlobalQualifyRootedForcesGlobalUnderRootShadow(t *testing.T) {
	cases := []struct {
		name            string
		namespace       string
		childNamespaces map[string]bool
		rooted          string
		want            string
	}{
		{
			name:      "no shadow leaves the bare root prefix",
			namespace: "go.math.rand",
			rooted:    "go.math.rand.rand_package",
			want:      "go.math.rand.rand_package",
		},
		{
			name:            "go/* in the import closure forces global::",
			namespace:       "go.math.rand",
			childNamespaces: map[string]bool{"go.go": true},
			rooted:          "go.math.rand.rand_package",
			want:            "global::go.math.rand.rand_package",
		},
		{
			name:      "a go/* package's own namespace forces global::",
			namespace: "go.go.format",
			rooted:    "go.testing_runtime",
			want:      "global::go.testing_runtime",
		},
		{
			name:            "already-global target is left alone (idempotent)",
			namespace:       "go.math.rand",
			childNamespaces: map[string]bool{"go.go": true},
			rooted:          "global::go.testing_runtime",
			want:            "global::go.testing_runtime",
		},
	}

	for _, tc := range cases {
		t.Run(tc.name, func(t *testing.T) {
			setShadowState(t, tc.namespace, tc.childNamespaces)

			if got := globalQualifyRooted(tc.rooted); got != tc.want {
				t.Errorf("globalQualifyRooted(%q) = %q, want %q", tc.rooted, got, tc.want)
			}
		})
	}
}

// Blocker A(a): the shadow gate is computed from the import closure of the package being
// converted. Where the PRODUCTION sources are recompiled into the test assembly
// (testProjectRecompile), the gate has to see the union of the production and _test.go closures —
// computeImportAliasRenames folds siblingClosureImportPaths in for exactly that reason. Without the
// union the production half emitted bare `go.` prefixes into an assembly that does contain `go.go`.
//
// This is the CAPABILITY test: the fold registers the shadow when the caller asks for the union.
// WHICH unit asks is the caller's decision and is covered by TestProductionUnitExcludesSiblingClosure
// — under either reference model the production `.cs` compiles into the production assembly alone,
// so it must NOT see the union.
func TestSiblingClosureContributesRootShadow(t *testing.T) {
	previous := siblingClosureImportPaths
	t.Cleanup(func() { siblingClosureImportPaths = previous })

	previousQualified := packageQualifiedNamespaces
	t.Cleanup(func() { packageQualifiedNamespaces = previousQualified })

	// An import-free stand-in for the production package: the closure under test is the SIBLING
	// (_test.go) half, which computeImportAliasRenames folds in on top of the package's own.
	production := types.NewPackage("math/rand/v2", "rand")

	setShadowState(t, "go.math.rand", nil)
	packageQualifiedNamespaces = map[string]bool{}

	// Production-only closure: no go/* package, so no shadow.
	siblingClosureImportPaths = nil
	computeImportAliasRenames(nil, production, packageNamespace, "", "", true)

	if rootNamespaceShadowed() {
		t.Fatal("production-only closure reported a go.go root shadow")
	}

	// The _test.go half imports go/format (regress_test.go), which must now register the shadow.
	setShadowState(t, "go.math.rand", nil)
	packageQualifiedNamespaces = map[string]bool{}
	siblingClosureImportPaths = []string{"go/format"}
	computeImportAliasRenames(nil, production, packageNamespace, "", "", true)

	if !rootNamespaceShadowed() {
		t.Fatal("sibling test closure importing go/format did not register the go.go root shadow")
	}
}

// Blocker B: a record naming a type that compiles into THIS assembly through a fully-qualified
// class must render in the bare local form, so the two spellings of one resolved pair collapse in
// the emitting HashSet. Under -tests that covers the PACKAGE UNDER TEST as well as the current
// package: the external `<name>_test` variant reaches it by import path and renders it qualified,
// while the seeded production metadata carries it short, so math/rand/v2 emitted both
// `GoImplement<PCG, Source>` and `GoImplement<go.math.rand.rand_package.PCG, …Source>` and
// go2cs-gen defined `rand_package.PCGжSource` twice (CS0102 + CS0111 ×5 + CS8646).
func TestStripLocalTypeQualifier(t *testing.T) {
	const localPrefix = "go.math.rand.rand_test_package"

	previous := testLocalTypePrefixes
	t.Cleanup(func() { testLocalTypePrefixes = previous })

	// What convertTestVariant installs for a -tests run of math/rand/v2.
	testLocalTypePrefixes = []string{"go.math.rand.rand_package"}

	cases := []struct {
		name string
		in   string
		want string
	}{
		{"package-under-test type reduces to the bare name", "go.math.rand.rand_package.PCG", "PCG"},
		{"both sides of a pair reduce", "go.math.rand.rand_package.PCG, go.math.rand.rand_package.Source", "PCG, Source"},
		{"the current package's own class reduces too", "go.math.rand.rand_test_package.helper", "helper"},
		{"a foreign reference is untouched", "io_package.Reader", "io_package.Reader"},
		{"a genuinely foreign qualified reference is untouched", "go.net.http_package.ΔHandler", "go.net.http_package.ΔHandler"},
		{"an already-bare name is untouched", "PCG", "PCG"},
		{"a nested member under a local class is untouched", "go.math.rand.rand_package.Outer.Inner", "go.math.rand.rand_package.Outer.Inner"},
	}

	for _, tc := range cases {
		t.Run(tc.name, func(t *testing.T) {
			if got := stripLocalTypeQualifier(tc.in, localPrefix); got != tc.want {
				t.Errorf("stripLocalTypeQualifier(%q) = %q, want %q", tc.in, got, tc.want)
			}
		})
	}
}

// Outside a -tests conversion no extra prefix is local, so only the current package's own class is
// stripped — the property that keeps every non-test package's metadata byte-identical.
func TestStripLocalTypeQualifierIgnoresForeignPrefixesOutsideTests(t *testing.T) {
	previous := testLocalTypePrefixes
	t.Cleanup(func() { testLocalTypePrefixes = previous })

	testLocalTypePrefixes = nil

	if got := stripLocalTypeQualifier("go.math.rand.rand_package.PCG", "go.expvar_package"); got != "go.math.rand.rand_package.PCG" {
		t.Errorf("stripLocalTypeQualifier stripped a foreign prefix outside -tests: %q", got)
	}
}

// The sibling-half fold applies to DECLARATOR names too, for the same assembly-not-package reason:
// the in-package `_test.go` declarations compile into the production package's own C# class, so one
// of them shadows a production file's import using-alias there (hash/maphash's
// `func (k *bytesKey) bits() int` over maphash_purego.go's `import "math/bits"` — `bits.Mul64(a, b)`
// bound the method group, CS0119). The production pass cannot see the test half, so
// performNameCollisionAnalysis folds siblingTestFuncMethodNames into the shadow set.
func TestSiblingTestDeclaratorsContributeAliasShadow(t *testing.T) {
	previousSiblings := siblingTestFuncMethodNames
	previousShadows := packageFuncMethodNames
	previousTestShadows := packageTestAliasShadows
	previousCollisions := nameCollisions

	t.Cleanup(func() {
		siblingTestFuncMethodNames = previousSiblings
		packageFuncMethodNames = previousShadows
		packageTestAliasShadows = previousTestShadows
		nameCollisions = previousCollisions
	})

	nameCollisions = map[string]bool{}

	// A production-only universe: no _test.go files, so nothing shadows the `bits` alias.
	dir := t.TempDir()
	writeModuleFiles(t, dir, map[string]string{
		"go.mod":           "module example/shadow\n\ngo 1.23\n",
		"shadow.go":        "package shadow\nimport \"math/bits\"\nfunc Mix(a, b uint64) uint64 {\n\thi, lo := bits.Mul64(a, b)\n\treturn hi ^ lo\n}\n",
		"key_test.go":      "package shadow\ntype bytesKey struct{ b []byte }\nfunc (k *bytesKey) bits() int { return len(k.b) * 8 }\n",
		"more_test.go":     "package shadow\nimport \"testing\"\nfunc TestBits(t *testing.T) {\n\tk := &bytesKey{b: []byte{1}}\n\tif k.bits() != 8 {\n\t\tt.Fatal(\"bad\")\n\t}\n}\n",
		"external_test.go": "package shadow_test\nfunc bits() {}\n",
		"excluded_test.go": "//go:build go2cs_never\n\npackage shadow\nfunc hidden() {}\n",
	})

	production := loadProductionForDir(t, dir)

	signals := collectSiblingTestSignals(dir, "shadow", Options{
		targetPlatform: runtime.GOOS + "/" + runtime.GOARCH,
	})
	collected, hasInternal := signals.funcMethodNames, signals.hasInternalTests
	if want := []string{"TestBits", "bits"}; !slices.Equal(collected, want) {
		t.Fatalf("collected sibling test declarators = %v, want %v", collected, want)
	}
	if !hasInternal {
		t.Fatal("expected build-selected internal test detection")
	}

	siblingTestFuncMethodNames = nil
	performNameCollisionAnalysis(production)

	if packageFuncMethodNames["bits"] {
		t.Fatal("the production-only universe declares no `bits`, so it must not shadow the alias")
	}

	// The same production pass, now told what the sibling _test.go half declares.
	siblingTestFuncMethodNames = []string{"bits"}
	performNameCollisionAnalysis(production)

	if !packageFuncMethodNames["bits"] {
		t.Fatal("a sibling test declarator named after an imported package must shadow its alias")
	}

	if !packageTestAliasShadows["bits"] {
		t.Fatal("a shadow contributed only by tests must be marked for an explanatory comment")
	}

	var aliasStmt ast.Stmt
	for _, file := range production.Syntax {
		ast.Inspect(file, func(node ast.Node) bool {
			if aliasStmt != nil {
				return false
			}

			if assign, ok := node.(*ast.AssignStmt); ok {
				aliasStmt = assign
				return false
			}

			return true
		})
	}

	if aliasStmt == nil {
		t.Fatal("failed to locate the production bits assignment")
	}

	visitor := Visitor{
		info:          production.TypesInfo,
		outputBuilder: &strings.Builder{},
		newline:       "\n",
		indentLevel:   1,
		options:       Options{indentSpaces: 4},
	}
	visitor.writeTestAliasShadowComment(aliasStmt, nil)

	const wantComment = "\n    // Fully qualified to avoid alias shadowing by the same-package test declaration \"bits\"."
	if got := visitor.outputBuilder.String(); got != wantComment {
		t.Fatalf("generated alias-shadow comment = %q, want %q", got, wantComment)
	}

	// The fold is a REFERENCE-spelling concern only: it must never register a symbol rename.
	if nameCollisions["bits"] {
		t.Fatal("the sibling fold must not turn a test declarator into a package-scoped rename")
	}
}

// A package reference emitted into a file's body is also exposed to the file's own `using
// <namespace>;` directives, which C# consults at the file's namespace level BEFORE moving out to the
// root: go1.24's internal/sync put `go.@internal.sync_package` where runtime/pprof (namespace
// go.runtime, `using @internal;`) and net/http's test sources (namespace go.net,
// `using global::go.@internal;`) found it first, so an embedded `sync.Mutex` compiled as internal/sync's
// Mutex. qualifyPackageReference spells such a reference from the root; everything else stays as it was.
//
// RED PROOF: at the parent the three colliding arms read the bare `sync_package.Mutex` (the reference
// went through rootQualifyIfAmbiguous alone, which never sees a using); the controls read the same
// before and after.
func TestPackageReferenceShadowedByFileUsingIsRootQualified(t *testing.T) {
	previousQualified := packageQualifiedNamespaces
	t.Cleanup(func() { packageQualifiedNamespaces = previousQualified })

	// The closure both measured files compile against: the root sync, the converted internal/sync
	// beside internal/abi under go.@internal, and bytes, which nothing shadows.
	closure := func() {
		packageQualifiedNamespaces = map[string]bool{
			"go.sync_package":              true,
			"go.bytes_package":             true,
			"go.@internal.sync_package":    true,
			"go.@internal.abi_package":     true,
			"go.runtime.pprof_package":     true,
			"go.net.http_package":          true,
			"go.@internal.godebug_package": true,
		}
	}

	visitor := func(required []string, method []string) *Visitor {
		return &Visitor{requiredUsings: hashset.NewHashSet(required), methodNamespaceUsings: hashset.NewHashSet(method)}
	}

	cases := []struct {
		name      string
		namespace string
		required  []string
		method    []string
		ref       string
		want      string
	}{
		// runtime/pprof's shape: a RELATIVE using, resolved from go.runtime outward to go.@internal.
		{"relative using shadows (runtime/pprof)", "go.runtime", []string{"@internal", "text"}, nil, "sync_package.Mutex", "go.sync_package.Mutex"},
		// net/http's test shape: a ROOTED using.
		{"rooted using shadows (net/http tests)", "go.net", []string{"global::go.@internal", "global::go.net"}, nil, "sync_package.Mutex", "go.sync_package.Mutex"},
		// A using that lands only while the body is visited (a method call into an internal package):
		// the pre-pass is what makes it visible to a reference emitted before it.
		{"method-derived using shadows", "go.runtime", nil, []string{"@internal"}, "sync_package.Mutex", "go.sync_package.Mutex"},
		// Controls: a name the imported namespace does not declare, and a file with no using at all.
		{"non-colliding name unchanged", "go.runtime", []string{"@internal"}, nil, "bytes_package.Buffer", "bytes_package.Buffer"},
		{"no using unchanged", "go.runtime", nil, nil, "sync_package.Mutex", "sync_package.Mutex"},
		// A static or alias entry imports no namespace, and System.* is no namespace of the closure.
		{"static and alias entries ignored", "go.runtime", []string{"static go.@internal.sync_package", "sync = sync_package", "System.Runtime.CompilerServices"}, nil, "sync_package.Mutex", "sync_package.Mutex"},
	}

	for _, tc := range cases {
		t.Run(tc.name, func(t *testing.T) {
			setShadowState(t, tc.namespace, map[string]bool{"go.@internal": true, "go.runtime": true, "go.net": true})
			closure()

			if got := visitor(tc.required, tc.method).qualifyPackageReference(tc.ref); got != tc.want {
				t.Errorf("qualifyPackageReference(%q) in %s with usings %v + %v = %q, want %q", tc.ref, tc.namespace, tc.required, tc.method, got, tc.want)
			}
		})
	}

	// An alias TARGET is resolved as if the file had no usings, so it must keep binding bare: the
	// alias path never consults them, whatever the file imports.
	setShadowState(t, "go.runtime", map[string]bool{"go.@internal": true, "go.runtime": true})
	closure()

	if _, target := packageUsingAlias("sync"); target != "sync_package" {
		t.Errorf("packageUsingAlias(\"sync\") target = %q, want the bare \"sync_package\" (an alias target ignores the file's usings)", target)
	}
}
