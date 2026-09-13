// foreignNameCollisions_test.go - Gbtc
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

	"golang.org/x/tools/go/packages"
)

// loadCollisionFixtureDependency materializes a two-package module whose `dep` package carries one
// of every collision shape, loads the CONSUMER, and returns the loaded dependency as the consumer
// sees it — the same handle the alias loader resolves through importedPackageSources.
func loadCollisionFixtureDependency(t *testing.T) *packages.Package {
	t.Helper()

	dir := t.TempDir()

	if err := os.MkdirAll(filepath.Join(dir, "dep"), 0755); err != nil {
		t.Fatal(err)
	}

	writeModuleFiles(t, dir, map[string]string{
		"go.mod": "module example/collision\n\ngo 1.23\n",
		"app.go": "package app\n\nimport \"example/collision/dep\"\n\nvar Elapsed = 2 * dep.Second\n",
	})

	writeModuleFiles(t, filepath.Join(dir, "dep"), map[string]string{
		"dep.go": `package dep

type Duration int64

const (
	Nanosecond Duration = 1
	Second     Duration = 1000 * Nanosecond
)

type Month int

type Token any

type Filter func(string) bool

type Clock struct{}

func (Clock) Second() int      { return 0 }
func (Clock) Month() Month     { return 0 }
func (Clock) Token() Token     { return nil }
func (Clock) Filter() Filter   { return nil }
func (Clock) Duration() string { return "" }
`,
	})

	loaded, err := packages.Load(&packages.Config{Mode: packages.LoadAllSyntax, Dir: dir}, ".")

	if err != nil {
		t.Fatal(err)
	}

	if len(loaded) != 1 {
		t.Fatalf("expected the consumer package to load, got %d packages", len(loaded))
	}

	if len(loaded[0].Errors) > 0 {
		t.Fatalf("consumer package load failed: %v", loaded[0].Errors)
	}

	dependency := loaded[0].Imports["example/collision/dep"]

	if dependency == nil {
		t.Fatal("the dependency was not reachable through the consumer's import graph")
	}

	return dependency
}

func aliasPairs(aliases [][2]string) map[string]string {
	pairs := make(map[string]string, len(aliases))

	for _, alias := range aliases {
		pairs[alias[0]] = alias[1]
	}

	return pairs
}

// A foreign package's collision renames must be derivable from that package ALONE. Before this,
// they were read only from the dependency's emitted package_info.cs, so a consumer converted
// WITHOUT its dependency in the same run emitted the unrenamed foreign name (`time.Second`, which
// binds the `Second(this Time)` extension method group — CS0019/CS1503) while a whole-stdlib run
// emitted `time.ΔSecond`. This pins the derivation, one case per published shape.
func TestForeignCollisionAliasesAreDerivedFromTheDependency(t *testing.T) {
	dependency := loadCollisionFixtureDependency(t)
	derived := aliasPairs(foreignCollisionTypeAliases(dependency))

	// A colliding const Δ-renames the MEMBER (the filed defect: `time.Second`/`Minute`/`Hour`).
	if got := derived["Second"]; got != "const:"+ShadowVarMarker+"Second" {
		t.Fatalf("colliding const must publish a const: rename, got %q", got)
	}

	// A colliding defined type Δ-renames the TYPE.
	if got := derived["Month"]; got != ShadowVarMarker+"Month" {
		t.Fatalf("colliding type must publish a type rename, got %q", got)
	}

	// A defined type over the EMPTY interface is emitted as an alias, not a nested type, so the
	// renamed form carries a second hop to its concrete target (encoding/json's `Token`).
	if got := derived["Token"]; got != ShadowVarMarker+"Token" {
		t.Fatalf("colliding empty-interface type must publish a type rename, got %q", got)
	}

	if got := derived[ShadowVarMarker+"Token"]; got != "object" {
		t.Fatalf("colliding empty-interface type must publish its second hop to object, got %q", got)
	}

	// A methodless named func type is rendered inline as its base delegate — there is no
	// `<pkg>_package.Δname` type for an alias to name (go/doc's `ast.Filter`, CS0426).
	if got, published := derived["Filter"]; published {
		t.Fatalf("methodless named func type must publish no alias, got %q", got)
	}

	// A name that is NOT also a method name is not renamed at all.
	if got, published := derived["Nanosecond"]; published {
		t.Fatalf("non-colliding const must publish no alias, got %q", got)
	}

	// `Duration` collides (the Clock method) and must be renamed — the guard's positive control
	// that the collision test is actually consulting the dependency's method set.
	if got := derived["Duration"]; got != ShadowVarMarker+"Duration" {
		t.Fatalf("colliding type must publish a type rename, got %q", got)
	}
}

// The INVARIANT: the derivation depends only on the dependency's own declarations, never on what
// the current run has converted or accumulated. Same dependency, hostile run state, same answer.
func TestForeignCollisionAliasesIgnoreRunAccumulatedState(t *testing.T) {
	dependency := loadCollisionFixtureDependency(t)

	resetPackageState(&packages.Package{})
	fromCleanState := aliasPairs(foreignCollisionTypeAliases(dependency))

	// Re-derive with the run-accumulated collision state poisoned in both directions: a name the
	// dependency does NOT rename marked as colliding, and the map otherwise empty. A derivation
	// that consulted this state (the defect) would answer differently.
	foreignCollisionAliases = nil
	nameCollisions = map[string]bool{"Nanosecond": true}

	fromPoisonedState := aliasPairs(foreignCollisionTypeAliases(dependency))

	if len(fromCleanState) != len(fromPoisonedState) {
		t.Fatalf("derivation changed with run state: %v vs %v", fromCleanState, fromPoisonedState)
	}

	for name, target := range fromCleanState {
		if fromPoisonedState[name] != target {
			t.Fatalf("derivation of %q changed with run state: %q vs %q", name, target, fromPoisonedState[name])
		}
	}
}

// End-to-end at the decision that actually emits: a consumer with NO package_info.cs to read must
// still spell the foreign member the renamed way, identically to a run in which the dependency was
// converted first.
func TestUnconvertedDependencySpellsCollisionRenamedMember(t *testing.T) {
	dependency := loadCollisionFixtureDependency(t)

	resetPackageState(&packages.Package{})
	foreignCollisionAliases = nil

	info := PackageInfo{PackageName: "example.collision.dep", RootPackageName: "dep", SourceDir: dependency.Dir}
	applyExportedTypeAliases(foreignCollisionTypeAliases(dependency), info, true)

	if got := getAliasedTypeName("dep.Second"); got != "dep."+ShadowVarMarker+"Second" {
		t.Fatalf("a colliding foreign const must render renamed, got %q", got)
	}

	// The `_package`-qualified form (a same-package method shadows the import's using alias) must
	// follow the same rename — it consults the map by the plain package name.
	if got := getAliasedTypeName("dep" + PackageSuffix + ".Second"); got != "dep"+PackageSuffix+"."+ShadowVarMarker+"Second" {
		t.Fatalf("a package-qualified foreign const must render renamed, got %q", got)
	}

	// A colliding foreign TYPE renders through its global-using alias, whose declaration the
	// consumer's package_info.cs emits from this same entry.
	if got := getAliasedTypeName("dep.Month"); got != "dep"+TypeAliasDot+"Month" {
		t.Fatalf("a colliding foreign type must render through its alias, got %q", got)
	}

	if got := importedTypeAliases["dep.Month"]; got != RootNamespace+".example.collision.dep"+PackageSuffix+"."+ShadowVarMarker+"Month" {
		t.Fatalf("the colliding foreign type's alias target is wrong: %q", got)
	}

	// A non-colliding member is untouched.
	if got := getAliasedTypeName("dep.Nanosecond"); got != "dep.Nanosecond" {
		t.Fatalf("a non-colliding foreign const must render unchanged, got %q", got)
	}

	// A DERIVED alias declares its `global using` only once a reference has resolved through it:
	// the derivation describes what go2cs WOULD emit for the dependency's source, which a
	// hand-written proxy (the baseline core/time stub) does not satisfy — an unused declaration
	// would then be CS0426 for a consumer that never names the type. `Month` was just referenced;
	// `Token` was not.
	if !usedDerivedTypeAliases.Contains("dep.Month") {
		t.Fatal("a referenced derived alias must be marked used so its global using is emitted")
	}

	if usedDerivedTypeAliases.Contains("dep.Token") {
		t.Fatal("an unreferenced derived alias must not be marked used")
	}
}
