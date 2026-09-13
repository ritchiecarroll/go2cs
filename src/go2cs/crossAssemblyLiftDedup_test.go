// crossAssemblyLiftDedup_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).
//
// Guards the one rule the anonymous struct/interface dedup's PRODUCTION-registry arm has to obey
// that its same-pass arm does not: a reuse that crosses an ASSEMBLY boundary is admissible only when
// the reused declaration is REACHABLE from the assembly doing the reusing.
//
// TWO measured failures, in OPPOSITE directions, and the rule has to hold both — which is why the
// arms below vary the MODEL and never a hand-set variant flag.
//
// REFUSAL (2026-09-01; bisected, first bad commit 5442b402e "Residual pass round 3: anonymous
// struct/interface dedup, cross-variant and within-pass", last good its parent a5e3347f5). All four
// of `errors`' test files are `package errors_test`, so the package has no sibling internal test
// file, so selectTestProjectModel picks the plain REFERENCE model, so its production .csproj carries
// no `InternalsVisibleTo` (insertFriendAssemblyAccess) and its external suite compiles into a plain
// referencing assembly. join_test.go's `err.(interface{ Unwrap() []error })` sits inside TestJoin —
// function-local, so the guard's `v.inFunction` disjunct short-circuits unconditionally — and the
// conversion stopped declaring the local `TestJoin_typeᴛ1` it used to emit, binding production's
// `is_typeᴛ1` instead:
//
//	join_test.cs(49,48): error CS0122: 'errors_package.is_typeᴛ1' is inaccessible due to its
//	protection level
//
// ADOPTION (2026-09-04; the fix for that refusal, f38c2ae01, keyed on the Go VARIANT, and the axis is
// the test ASSEMBLY). `runtime` HAS internal test files, so its model is WHITEBOX REFERENCE and both
// variants emit into the one `.tests` project whose `InternalsVisibleTo` grant that same fact
// produces. hash_test.go's package-level `type IfaceKey struct { i interface{ F() } }` calls
// production `ifaceHash` through the export_test.go bridge, so refusing the reuse there minted a
// second `IfaceKey_i` and the call could not bind:
//
//	hash_test.cs(540,52): error CS1503: cannot convert from 'go.runtime_test_package.IfaceKey_i'
//	to 'go.runtime_package.ifaceHash_i'
//
// The arms are one measurement with its own positive controls. A refusal arm alone could pass for the
// wrong reason — a seeded signature key that matches nothing refuses everything — so the ADOPTION
// arms use the identical signature key and identical seeded registry, varying only what the rule
// reads. If the key were wrong, those would fail where the refusal passes.

package main

import (
	"go/types"
	"os"
	"path/filepath"
	"strings"
	"testing"

	"golang.org/x/tools/go/packages"
)

// crossAssemblyUnwrapSignature is the structural signature of `interface{ Unwrap() []error }` —
// errors' own shape, and the key GoDynamicTypeLift publishes / productionDynamicTypeNames is keyed
// by (see seedProductionDynamicTypeLifts). Spelled exactly as types.Type.String() renders it, the
// same contract runtimeScavengeIndexSignature (dynamicTypeGate_test.go) records for the struct side.
const crossAssemblyUnwrapSignature = "interface{Unwrap() []error}"

// crossAssemblyLiftFiles is the shared fixture body. The production package deliberately does NOT
// write the anonymous interface, so the candidate under test can only come from the SEEDED
// production registry and never from this pass's own registrations (the isolation
// TestResolvedViaProductionRegistryLeavesTheGateGreen relies on too).
//
// The external test file carries BOTH shapes the two regressions came in as: a function-local
// assertion (errors' join_test.go) and a PACKAGE-LEVEL struct field (runtime's hash_test.go
// `IfaceKey`), so one fixture measures both lift paths.
var crossAssemblyLiftFiles = map[string]string{
	"go.mod": "module example/caclift\n\ngo 1.23\n",
	"join.go": "package caclift\n\n" +
		"// Join is the production surface both variants reach. It deliberately does NOT write\n" +
		"// the anonymous interface: the candidate under test must come from the SEEDED\n" +
		"// production registry alone, never from this pass's own registrations.\n" +
		"func Join(errs ...error) error {\n" +
		"\tfor _, err := range errs {\n" +
		"\t\tif err != nil {\n" +
		"\t\t\treturn err\n" +
		"\t\t}\n" +
		"\t}\n\n" +
		"\treturn nil\n" +
		"}\n",
	"join_test.go": "package caclift_test\n\n" +
		"import (\n\t\"testing\"\n\n\t\"example/caclift\"\n)\n\n" +
		"// IfaceKey mirrors runtime hash_test.go's package-level struct whose FIELD type is the\n" +
		"// anonymous interface production already lifted — the shape that reaches the lift through\n" +
		"// visitStructType rather than through a function body.\n" +
		"type IfaceKey struct {\n" +
		"\ti interface{ Unwrap() []error }\n" +
		"}\n\n" +
		"func TestUnwrapJoined(t *testing.T) {\n" +
		"\terr := caclift.Join(nil)\n\n" +
		"\tif x, ok := err.(interface{ Unwrap() []error }); ok {\n" +
		"\t\t_ = x.Unwrap()\n" +
		"\t}\n" +
		"}\n",
}

// crossAssemblyLiftInternalTestFile is what decides the MODEL. Present, selectTestProjectModel picks
// white-box reference (runtime's shape and the IVT grant that comes with it); absent, the plain
// reference model (errors' shape, no grant).
const crossAssemblyLiftInternalTestFile = "package caclift\n\n" +
	"func UnwrapAll(err error) []error {\n" +
	"\tif x, ok := err.(interface{ Unwrap() []error }); ok {\n" +
	"\t\treturn x.Unwrap()\n" +
	"\t}\n\n" +
	"\treturn nil\n" +
	"}\n"

// crossAssemblyLiftFixture writes the fixture, with or without the sibling INTERNAL test file that
// decides which project model the package gets.
func crossAssemblyLiftFixture(t *testing.T, withInternalTestFile bool) string {
	t.Helper()

	dir := t.TempDir()

	files := map[string]string{}

	for name, contents := range crossAssemblyLiftFiles {
		files[name] = contents
	}

	if withInternalTestFile {
		files["export_test.go"] = crossAssemblyLiftInternalTestFile
	}

	for name, contents := range files {
		if err := os.WriteFile(filepath.Join(dir, name), []byte(contents), 0644); err != nil {
			t.Fatal(err)
		}
	}

	return dir
}

// convertCrossAssemblyVariant converts one loaded variant with productionDynamicTypeNames seeded to
// map crossAssemblyUnwrapSignature onto candidate, and returns the emitted C#.
//
// The per-variant options come from testVariantOptions — the production path, not a hand-set flag —
// so this exercises the WIRING that decides reachability and not merely the predicate that reads it.
func convertCrossAssemblyVariant(t *testing.T, pkg *packages.Package, candidate string, model testProjectModel, external bool) string {
	t.Helper()

	outputPath := t.TempDir()

	base := Options{indentSpaces: 4, preferVarDecl: true, useChannelOperators: true}
	options := testVariantOptions(base, model, external, "bridge")

	seed := productionSeed{dynamicTypeNames: map[string]string{crossAssemblyUnwrapSignature: candidate}}

	testMethodRenames = make(map[types.Object]bool)
	defer func() { testMethodRenames = nil }()

	if _, _, err := convertTestVariant(pkg, testFileEntries(pkg), outputPath, "go", seed, options); err != nil {
		t.Fatalf("convertTestVariant: %v", err)
	}

	return readConvertedAssembly(t, outputPath)
}

// crossAssemblyLiftCandidates returns the internal and public seeded candidate names, asserting they
// READ the way every arm below claims — generatedTypeScope is the same reader the rule consults, so
// a fixture whose names read differently would measure something else entirely.
func crossAssemblyLiftCandidates(t *testing.T) (internalCandidate, publicCandidate string) {
	t.Helper()

	internalCandidate = "is_type" + TempVarMarker + "1"
	publicCandidate = "Is_type" + TempVarMarker + "1"

	if got := generatedTypeScope(internalCandidate); got != "internal" {
		t.Fatalf("the fixture's internal candidate %q reads %q, not internal", internalCandidate, got)
	}

	if got := generatedTypeScope(publicCandidate); got != "public" {
		t.Fatalf("the fixture's public candidate %q reads %q, not public", publicCandidate, got)
	}

	return internalCandidate, publicCandidate
}

// TestReferenceModelExternalSuiteRefusesInternalProductionLift is the `errors` regression's guard: no
// sibling internal test file, so no IVT grant, so an INTERNAL production candidate must be refused
// and the lift minted locally — while the same fixture, same seeded registry and same signature
// adopt a PUBLIC candidate, which is the positive control that the key matches at all.
func TestReferenceModelExternalSuiteRefusesInternalProductionLift(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: loads and converts a test-variant fixture")
	}

	dir := crossAssemblyLiftFixture(t, false)
	internal, external := loadBothTestVariantsForDir(t, dir)

	if internal != nil {
		t.Fatal("the errors-shaped fixture must have NO internal test variant; the model would not be the one under test")
	}

	if external == nil {
		t.Fatal("the external test variant was not loaded")
	}

	// The model is DERIVED, never asserted by hand: it is what the refusal hangs on.
	model := selectTestProjectModel(internal, external)

	if model != testProjectReference {
		t.Fatalf("a package with no internal test file must take the reference model; got %v", model)
	}

	internalCandidate, publicCandidate := crossAssemblyLiftCandidates(t)

	t.Run("refuses an internal production candidate", func(t *testing.T) {
		withDynamicTypeRegistry(t, func() {
			emitted := convertCrossAssemblyVariant(t, external, internalCandidate, model, true)

			if strings.Contains(emitted, internalCandidate) {
				t.Errorf("the external suite adopted production's INTERNAL lift %q — under the reference model it compiles into a separate assembly with no view of it (CS0122, the errors join_test.cs regression):\n%s", internalCandidate, emitted)
			}

			// The refusal must MINT rather than silently emit nothing. `IfaceKey_i` is the witness
			// on purpose: it is the same declaration the white-box arm requires to be ABSENT, so the
			// two arms differ on exactly the axis under test and neither can pass vacuously. (The
			// function-local assertion in TestUnwrapJoined reuses this file-level lift through the
			// SAME-PASS registry — that arm is not a second mint and its name is not pinned here.)
			if !strings.Contains(emitted, "partial interface IfaceKey_i") {
				t.Errorf("refusing the production candidate must MINT a local lift; none was declared:\n%s", emitted)
			}
		})
	})

	t.Run("adopts a public production candidate", func(t *testing.T) {
		withDynamicTypeRegistry(t, func() {
			emitted := convertCrossAssemblyVariant(t, external, publicCandidate, model, true)

			if !strings.Contains(emitted, publicCandidate) {
				t.Errorf("a PUBLIC production lift is reachable from the external assembly and must still dedup; %q was not adopted:\n%s", publicCandidate, emitted)
			}

			if strings.Contains(emitted, "partial interface IfaceKey_i") {
				t.Errorf("an adopted candidate must emit NO declaration of its own:\n%s", emitted)
			}
		})
	})
}

// TestWhiteboxModelExternalSuiteAdoptsInternalProductionLift is the `runtime` regression's guard, and
// the direction f38c2ae01 broke: a package WITH an internal test file takes the white-box model, both
// variants emit into the one `.tests` assembly whose `InternalsVisibleTo` grant that same fact
// produces, so an INTERNAL production candidate is reachable and MUST be adopted — by the
// package-level struct FIELD path (runtime's `IfaceKey.i`, the CS1503) as much as by the
// function-local one.
func TestWhiteboxModelExternalSuiteAdoptsInternalProductionLift(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: loads and converts a test-variant fixture")
	}

	dir := crossAssemblyLiftFixture(t, true)
	internal, external := loadBothTestVariantsForDir(t, dir)

	if internal == nil {
		t.Fatal("the runtime-shaped fixture must have an internal test variant; the model would not be the one under test")
	}

	if external == nil {
		t.Fatal("the external test variant was not loaded")
	}

	model := selectTestProjectModel(internal, external)

	if model != testProjectWhiteboxReference {
		t.Fatalf("a package with an internal test file must take the white-box reference model; got %v", model)
	}

	internalCandidate, _ := crossAssemblyLiftCandidates(t)

	t.Run("external suite adopts an internal production candidate", func(t *testing.T) {
		withDynamicTypeRegistry(t, func() {
			emitted := convertCrossAssemblyVariant(t, external, internalCandidate, model, true)

			// The PACKAGE-LEVEL struct field is the runtime shape: `IfaceKey.i` must BE the
			// production lift, or the bridge call cannot bind (CS1503).
			if strings.Contains(emitted, "partial interface IfaceKey_i") {
				t.Errorf("the external suite minted its OWN lift for a field whose type production already lifted — this is runtime hash_test.cs's CS1503:\n%s", emitted)
			}

			if !strings.Contains(emitted, internalCandidate) {
				t.Errorf("under the white-box model the external suite has package-private sight of production and must adopt %q:\n%s", internalCandidate, emitted)
			}

			if strings.Contains(emitted, "partial interface TestUnwrapJoined_type") {
				t.Errorf("an adopted candidate must emit NO declaration of its own, in the function-local path either:\n%s", emitted)
			}

			// The refusal arm's witness, inverted: no mint of ANY name for this shape.
			if strings.Contains(emitted, `[GoType("dyn")] partial interface`) {
				t.Errorf("an adopted candidate must leave the assembly with no dyn-interface lift of its own:\n%s", emitted)
			}
		})
	})

	t.Run("internal variant adopts an internal production candidate", func(t *testing.T) {
		withDynamicTypeRegistry(t, func() {
			emitted := convertCrossAssemblyVariant(t, internal, internalCandidate, model, false)

			if !strings.Contains(emitted, internalCandidate) {
				t.Errorf("the internal variant has package-private sight of production and must still dedup; %q was not adopted:\n%s", internalCandidate, emitted)
			}

			if strings.Contains(emitted, "partial interface UnwrapAll_type") {
				t.Errorf("an adopted candidate must emit NO declaration of its own:\n%s", emitted)
			}
		})
	})
}

// TestTestVariantOptionsRecordsProductionInternalVisibility pins the WIRING the predicate reads — the
// decision testVariantOptions records, not the artifact it produces (route #8). Reachability is a
// property of the test ASSEMBLY: only the plain reference model puts the external suite outside
// production's internals, because it is chosen exactly when there is no internal test file and
// therefore no `InternalsVisibleTo` grant.
func TestTestVariantOptionsRecordsProductionInternalVisibility(t *testing.T) {
	cases := []struct {
		name     string
		model    testProjectModel
		external bool
		want     bool
	}{
		{"recompile, internal variant", testProjectRecompile, false, true},
		{"recompile, external variant", testProjectRecompile, true, true},
		{"whitebox reference, internal variant", testProjectWhiteboxReference, false, true},
		{"whitebox reference, external variant", testProjectWhiteboxReference, true, true},
		{"reference, internal variant", testProjectReference, false, true},
		{"reference, external variant", testProjectReference, true, false},
	}

	for _, c := range cases {
		t.Run(c.name, func(t *testing.T) {
			options := testVariantOptions(Options{}, c.model, c.external, "bridge")

			if got := options.testProductionInternalsVisible; got != c.want {
				t.Errorf("testVariantOptions(%v, external=%v).testProductionInternalsVisible = %v, want %v", c.model, c.external, got, c.want)
			}

			if got := options.testExternalVariant; got != c.external {
				t.Errorf("testVariantOptions(%v, external=%v).testExternalVariant = %v, want %v", c.model, c.external, got, c.external)
			}
		})
	}
}

// TestProductionLiftReuseReachable pins the predicate itself, including the arms the fixture-driven
// tests cannot reach cheaply: an empty candidate (no production hit at all) and a PRODUCTION
// conversion, where testProductionInternalsVisible is false because there is no test variant —
// harmless, because productionDynamicTypeNames is nil there and the caller never reaches this check.
func TestProductionLiftReuseReachable(t *testing.T) {
	cases := []struct {
		name             string
		candidate        string
		internalsVisible bool
		want             bool
	}{
		{"no production hit", "", false, false},
		{"no production hit, internals visible", "", true, false},
		{"internal candidate, internals visible", "is_type" + TempVarMarker + "1", true, true},
		{"internal candidate, internals NOT visible", "is_type" + TempVarMarker + "1", false, false},
		{"public candidate, internals NOT visible", "Is_type" + TempVarMarker + "1", false, true},
		{"public candidate, internals visible", "Is_type" + TempVarMarker + "1", true, true},
	}

	for _, c := range cases {
		t.Run(c.name, func(t *testing.T) {
			options := Options{testProductionInternalsVisible: c.internalsVisible}

			if got := productionLiftReuseReachable(c.candidate, options); got != c.want {
				t.Errorf("productionLiftReuseReachable(%q, internalsVisible=%v) = %v, want %v", c.candidate, c.internalsVisible, got, c.want)
			}
		})
	}
}
