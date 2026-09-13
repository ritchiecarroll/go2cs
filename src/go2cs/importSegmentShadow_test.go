// importSegmentShadow_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/types"
	"os"
	"path/filepath"
	"testing"
)

// The forcing hook a `[GoInit]` emission writes into a class body is the ONE place the converter
// spells a namespace-qualified path where CLASS-member lookup applies. C# resolves the leading
// identifier of a namespace-or-type-name by walking the enclosing type declarations OUTWARD FIRST,
// so a nested type sharing that identifier occludes the namespace for the whole class body — while
// the `using` alias directive five lines above, resolved at namespace scope, is untouched.
//
// Both halves of the class are guarded here: the TEST-LOCAL shadow (Go's own image_test.go declares
// `type image interface{…}`, occluding the hooks for `image/color` and `image/color/palette`) and
// the PRODUCTION shadow (`type sync struct{}` beside `import "sync/atomic"`, which
// tests/Behavioral/ImportSegmentTypeShadow also compiles and runs end to end).

// TestGlobalQualifyForcingTarget pins the rooting itself. `global::` is what makes the reference
// collision-PROOF rather than merely collision-avoiding — it restarts lookup at the global
// namespace, so nothing the enclosing class declares can reach it.
func TestGlobalQualifyForcingTarget(t *testing.T) {
	previousQualified, previousChildren := packageQualifiedNamespaces, packageChildNamespaces
	t.Cleanup(func() {
		packageQualifiedNamespaces, packageChildNamespaces = previousQualified, previousChildren
	})

	// `go.image` is a real child namespace of the closure; `go.go.ast` is what a stripped go/*
	// reference becomes once re-rooted.
	packageQualifiedNamespaces = map[string]bool{}
	packageChildNamespaces = map[string]bool{"go.image": true, "go.go": true}

	for _, test := range []struct {
		name  string
		input string
		want  string
	}{
		{"bare single segment", "errors_package", "global::go.errors_package"},
		{"bare sub-package", "image.color_package", "global::go.image.color_package"},
		{"bare nested sub-package", "image.color.palette_package", "global::go.image.color.palette_package"},
		{"already rooted", "go.image.gif_package", "global::go.image.gif_package"},
		{"already global", "global::go.math.rand_package", "global::go.math.rand_package"},
		// A go/*-package reference whose root the redundant-root strip removed re-roots rather
		// than merely taking a prefix: `go.ast_package` names go/ast, whose real namespace is
		// `go.go` — prefixing alone would yield `global::go.ast_package`, which resolves nowhere.
		{"stripped go/* package", "go.ast_package", "global::go.go.ast_package"},
	} {
		t.Run(test.name, func(t *testing.T) {
			if got := globalQualifyForcingTarget(test.input); got != test.want {
				t.Errorf("globalQualifyForcingTarget(%q) = %q, want %q", test.input, got, test.want)
			}
		})
	}
}

// TestForcingTargetShadowedOnlyByOwnClassType pins the GATE. Over-qualifying is harmless C# but
// would churn every hook in the corpus; under-qualifying is a hard CS0426. The rule is narrow in
// both directions: only a TYPE occludes (a namespace-or-type-name lookup ignores funcs and vars),
// and only a type emitted into THIS class does (which is what keeps a production hook bare when
// the shadow lives in the test-variant class, so the `-stdlib` and `-tests` emissions of the
// production files stay identical).
func TestForcingTargetShadowedOnlyByOwnClassType(t *testing.T) {
	previousName := packageName
	t.Cleanup(func() { packageName = previousName })

	packageName = "consumer"

	pkg := types.NewPackage("example/consumer", "consumer")

	// A package-level TYPE named for the leading segment of `sync/atomic`'s namespace...
	shadowType := types.NewTypeName(0, pkg, "sync", types.Typ[types.Int])
	pkg.Scope().Insert(shadowType)

	// ...and a package-level FUNC named for the leading segment of `math/rand`'s. A func is not a
	// namespace-or-type-name candidate, so it must NOT trigger qualification.
	pkg.Scope().Insert(types.NewFunc(0, pkg, "math", types.NewSignatureType(nil, nil, nil, nil, nil, false)))

	visitor := &Visitor{pkg: pkg, emittedClassName: "consumer_package"}

	if !visitor.forcingTargetShadowed("sync.atomic_package") {
		t.Error("a package-level TYPE named for the target's leading segment must shadow the hook (CS0426)")
	}

	if visitor.forcingTargetShadowed("math.rand_package") {
		t.Error("a package-level FUNC must not shadow: typeof(a.b) is a namespace-or-type-name, and that lookup considers types and namespaces only")
	}

	if visitor.forcingTargetShadowed("fmt_package") {
		t.Error("an unrelated target must not be qualified — the gate exists to keep the corpus footprint at zero")
	}

	if visitor.forcingTargetShadowed("global::go.sync.atomic_package") {
		t.Error("an already-global target is unreachable by class lookup and must be left alone")
	}

	// The same declaration, emitted into the PRODUCTION class, does not occlude a hook written into
	// the TEST-VARIANT class: the two are sibling partial classes, and a `using static` import of
	// the production class loses to the namespace's own members at namespace scope.
	whiteboxInternalTestObjects = map[types.Object]bool{shadowType: true}
	t.Cleanup(func() { whiteboxInternalTestObjects = nil })

	testVariant := &Visitor{
		pkg:              pkg,
		emittedClassName: "consumer_internal_test_package",
		options:          Options{testClassNameOverride: "consumer_internal_test_package"},
	}

	if !testVariant.forcingTargetShadowed("sync.atomic_package") {
		t.Error("a TEST-declared type must shadow a hook emitted into the test-variant class")
	}

	production := &Visitor{
		pkg:              pkg,
		emittedClassName: "consumer_package",
		options:          Options{testClassNameOverride: "consumer_internal_test_package"},
	}

	if production.forcingTargetShadowed("sync.atomic_package") {
		t.Error("a TEST-declared type must NOT qualify a hook emitted into the PRODUCTION class — that would drift the -stdlib and -tests emissions of the same production file")
	}
}

// TestTestLocalTypeShadowRootsForcingHook converts a real internal-test variant whose `_test.go`
// declares a type named for an imported package's leading path segment — Go's image_test.go shape,
// reduced — and reads the emitted hook back. This is the end-to-end half: the unit test above pins
// the decision, this pins that the decision reaches the emitted bytes.
func TestTestLocalTypeShadowRootsForcingHook(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: loads and converts a test-variant fixture")
	}

	dir := t.TempDir()
	files := map[string]string{
		"go.mod": "module example/shad\n\ngo 1.23\n",
		"value.go": "package shad\n\nimport \"sync/atomic\"\n\n" +
			"var Hits atomic.Int32\n\n" +
			"func Bump() int32 { return Hits.Add(1) }\n",
		// `sync` is free to name a type here: importing "sync/atomic" binds `atomic`, not `sync`.
		"shad_test.go": "package shad\n\nimport (\n\t\"fmt\"\n\t\"sync/atomic\"\n)\n\n" +
			"type sync struct{ n atomic.Int32 }\n\n" +
			"func (s *sync) take() string { return fmt.Sprint(s.n.Add(1) + Bump()) }\n",
	}

	for name, contents := range files {
		if err := os.WriteFile(filepath.Join(dir, name), []byte(contents), 0644); err != nil {
			t.Fatal(err)
		}
	}

	internal, _ := loadBothTestVariantsForDir(t, dir)

	if internal == nil {
		t.Fatal("the internal test variant was not loaded")
	}

	outputPath := t.TempDir()
	options := Options{indentSpaces: 4, preferVarDecl: true, useChannelOperators: true}

	options.testClassNameOverride = getSanitizedImport("shad_internal_test" + PackageSuffix)
	options.testWhiteboxReference = true
	options.testInlineTypeAccess = true
	options.testProductionPath = "example/shad"

	testMethodRenames = make(map[types.Object]bool)
	t.Cleanup(func() { testMethodRenames = nil })

	if _, _, err := convertTestVariant(internal, testFileEntries(internal), outputPath, "go", productionSeed{}, options); err != nil {
		t.Fatal(err)
	}

	// Assert the FORCING TARGET the conversion decided on, not the text of a file it landed in.
	// The rooting decision is what this guard is about, and packageImportInits records it per import
	// path — which is where it lives since the hook relocation moved the emission out of the
	// importing file's class body and into the emission unit's metadata file. That file is written
	// by the DRIVER (convertTestVariants → writePackageInfoFile), not by the single-variant call
	// this test exercises, so no glob over `outputPath` can see the hooks at all any more: a
	// file-reading form of this guard fails its two positive assertions loudly and passes its
	// NEGATIVE one vacuously, since "the bare form is absent" is trivially true of a file that holds
	// no hook. Keying on the decision is both closer to the property and impossible to satisfy by
	// accident.
	shadowed, forced := packageImportInits["sync/atomic"]

	if !forced {
		t.Fatalf("sync/atomic was not forced at all; claims: %v", importInitClaims())
	}

	if shadowed != "global::go.sync.atomic_package" {
		t.Errorf("the forcing hook must be rooted past the test-local `sync` type (CS0426 otherwise), got %q", shadowed)
	}

	// The unshadowed import in the SAME file stays bare, so the gate is not a blanket rooting.
	bare, forced := packageImportInits["fmt"]

	if !forced {
		t.Fatalf("fmt was not forced at all; claims: %v", importInitClaims())
	}

	if bare != "fmt_package" {
		t.Errorf("an unshadowed import must keep its bare target — the gate exists to hold the corpus footprint at zero, got %q", bare)
	}
}
