// foreignTypeAliases_test.go - Gbtc
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
	"strings"
	"testing"

	"golang.org/x/tools/go/packages"
)

// loadReExportFixtureDependency materializes a three-package module — a consumer, the `dep` package
// whose re-exports are under test, and the `other` package `dep` re-exports FROM — and returns the
// loaded `dep` as the consumer sees it, the same handle the alias loader resolves through
// importedPackageSources.
func loadReExportFixtureDependency(t *testing.T) *packages.Package {
	t.Helper()

	dir := t.TempDir()

	for _, sub := range []string{"dep", "other"} {
		if err := os.MkdirAll(filepath.Join(dir, sub), 0755); err != nil {
			t.Fatal(err)
		}
	}

	writeModuleFiles(t, dir, map[string]string{
		"go.mod": "module example/sibmeta\n\ngo 1.23\n",
		"app.go": "package app\n\nimport \"example/sibmeta/dep\"\n\nvar Mode dep.Thing\n",
	})

	// `other` carries the TARGET shapes: a plain struct, a type named like a method IN ITS OWN
	// package (which keeps its Go name), an INLINE interface definition (a real
	// nested C# interface), a methodless named func type (rendered inline as its base delegate,
	// with no named type to point at), and a defined type over a named interface (itself emitted as
	// a using alias, so a reference to it would need a second hop).
	writeModuleFiles(t, filepath.Join(dir, "other"), map[string]string{
		"other.go": `package other

type Thing struct{ N int }

type Kind uint8

func (Kind) Kind() string { return "" }

type Stream interface{ Read() int }

type Handle func(int)

type Wrapped Stream
`,
	})

	// `dep` carries one declaration of every shape this derivation must publish or decline. `Clock`
	// gives `Kind` a same-package method so the ALIAS-exempt rule is exercised (the reflectlite
	// shape: `type Kind = abi.Kind` alongside `(*rtype).Kind()`).
	writeModuleFiles(t, filepath.Join(dir, "dep"), map[string]string{
		"dep.go": `package dep

import "example/sibmeta/other"

type Thing = other.Thing

type Kind = other.Kind

type Stream = other.Stream

type Handler = other.Handle

type Wrapped = other.Wrapped

type Payload any

type Table = map[string]int

type Anon = struct{ X int }

type Local struct{ X int }

type Reader other.Stream

type Inline interface{ Close() error }

type hidden = other.Thing

type Clock struct{}

func (Clock) Kind() string { return "" }
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

	dependency := loaded[0].Imports["example/sibmeta/dep"]

	if dependency == nil {
		t.Fatal("the dependency was not reachable through the consumer's import graph")
	}

	return dependency
}

// A foreign package's RE-EXPORTED type aliases must be derivable from that package ALONE. Before
// this they were read only from the dependency's emitted package_info.cs, so a consumer converted
// WITHOUT its dependency in the same run emitted `os.FileMode` — which is an assembly-scoped
// `global using` inside os, not a member of `os_package` (CS0426) — while a whole-stdlib run
// emitted the alias. This pins the derivation, one case per published shape and one per deliberate
// omission.
func TestForeignReExportedAliasesAreDerivedFromTheDependency(t *testing.T) {
	isolateConverterPathGlobals(t)
	resetPackageState(&packages.Package{})
	foreignDerivedAliases = nil

	dependency := loadReExportFixtureDependency(t)
	derived := aliasPairs(foreignDerivedTypeAliases(dependency))

	otherClass := RootNamespace + ".example.sibmeta.other" + PackageSuffix

	// A Go type alias to a foreign DEFINED type: the end-user case (os's `type FileMode =
	// fs.FileMode` → `go.io.fs_package.FileMode`).
	if got := derived["Thing"]; got != otherClass+".Thing" {
		t.Fatalf("alias to a foreign defined type must publish its qualified target, got %q", got)
	}

	// A target named like a METHOD of its own package keeps its Go name (internal/reflectlite's
	// `type Kind = abi.Kind`, where abi declares `(Kind).String` and others beside the type): the
	// method stands in its receiver's holder, so the package class declares `Kind` alone. `Kind`
	// names a method in `dep` too, which changes nothing either.
	if got := derived["Kind"]; got != otherClass+".Kind" {
		t.Fatalf("alias target named like a method of its package must keep its Go name, got %q", got)
	}

	// An INLINE interface definition in the target package is a real nested C# interface (io/fs's
	// FileInfo/DirEntry, go/ast's Expr).
	if got := derived["Stream"]; got != otherClass+".Stream" {
		t.Fatalf("alias to a foreign inline interface must publish its qualified target, got %q", got)
	}

	// A DEFINED type over a NAMED interface takes the same using-alias route as a Go alias
	// (visitTypeSpec's definedOverInterface arm), so it publishes its RHS's rendering.
	if got := derived["Reader"]; got != otherClass+".Stream" {
		t.Fatalf("defined type over a named interface must publish its RHS target, got %q", got)
	}

	// A defined type over the EMPTY interface renders as `object`, never a package member (crypto's
	// PublicKey/PrivateKey/DecrypterOpts, plugin's Symbol, database/sql/driver's Value).
	if got := derived["Payload"]; got != "object" {
		t.Fatalf("defined type over the empty interface must publish object, got %q", got)
	}

	// --- the deliberate omissions: a wrong target is worse than none ---

	// A methodless named func type is rendered inline as its base delegate — there is no
	// `<pkg>_package.Handle` type for an alias to name.
	if got, published := derived["Handler"]; published {
		t.Fatalf("alias to a methodless named func type must publish nothing, got %q", got)
	}

	// The target is ITSELF emitted as a using alias by its own package; following that second hop
	// is not attempted.
	if got, published := derived["Wrapped"]; published {
		t.Fatalf("alias to a foreign using-alias must publish nothing, got %q", got)
	}

	// A composite RHS renders through the whole convertToCSFullTypeName lowering.
	if got, published := derived["Table"]; published {
		t.Fatalf("alias to a composite type must publish nothing, got %q", got)
	}

	// An anonymous struct RHS is LIFTED under a generated name only a conversion assigns
	// (internal/fuzz's `CorpusEntry` → `CorpusEntryᴛ1`).
	if got, published := derived["Anon"]; published {
		t.Fatalf("alias to an anonymous struct must publish nothing, got %q", got)
	}

	// A real nested type publishes no alias at all — neither a defined struct nor an INLINE
	// interface definition takes the using-alias route.
	if got, published := derived["Local"]; published {
		t.Fatalf("a defined struct must publish no alias, got %q", got)
	}

	if got, published := derived["Inline"]; published {
		t.Fatalf("an inline interface definition must publish no alias, got %q", got)
	}

	// An UNEXPORTED alias is unreachable from another package and is never recorded.
	if got, published := derived["hidden"]; published {
		t.Fatalf("an unexported alias must publish nothing, got %q", got)
	}
}

// The INVARIANT, for this lane too: the derivation depends only on the dependency's own
// declarations, never on what the current run has converted or accumulated.
func TestForeignReExportedAliasesIgnoreRunAccumulatedState(t *testing.T) {
	isolateConverterPathGlobals(t)
	resetPackageState(&packages.Package{})
	foreignDerivedAliases = nil

	dependency := loadReExportFixtureDependency(t)
	fromCleanState := aliasPairs(foreignDerivedTypeAliases(dependency))

	// Re-derive with the run-accumulated state poisoned in both directions: a name the dependency
	// does NOT rename marked as colliding, and an alias map claiming a different target.
	foreignDerivedAliases = nil
	nameCollisions = map[string]bool{"Thing": true, "Kind": true}
	importedTypeAliases = map[string]string{"other.Thing": "go.wrong_package.Thing"}

	fromPoisonedState := aliasPairs(foreignDerivedTypeAliases(dependency))

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
// still render the foreign re-export through its alias, identically to a run in which the
// dependency was converted first.
func TestUnconvertedDependencyRendersReExportedAlias(t *testing.T) {
	resetPackageState(&packages.Package{})
	foreignDerivedAliases = nil

	dependency := loadReExportFixtureDependency(t)

	info := PackageInfo{PackageName: "example.sibmeta.dep", RootPackageName: "dep", SourceDir: dependency.Dir}
	applyExportedTypeAliases(foreignDerivedTypeAliases(dependency), info)

	// The reference renders through the `global using` the consumer's own package_info declares.
	if got := getAliasedTypeName("dep.Thing"); got != "dep"+TypeAliasDot+"Thing" {
		t.Fatalf("a foreign re-export must render through its alias, got %q", got)
	}

	if got := importedTypeAliases["dep.Thing"]; got != RootNamespace+".example.sibmeta.other"+PackageSuffix+".Thing" {
		t.Fatalf("the re-export's alias target is wrong: %q", got)
	}

	// The `object` target is imported BARE — it is a C# built-in, not a member of the dependency's
	// package class (isCSharpBuiltinTypeName).
	if got := importedTypeAliases["dep.Payload"]; got != "object" {
		t.Fatalf("an empty-interface re-export must alias to bare object, got %q", got)
	}

	// A DECLINED shape leaves the reference exactly as it converts today.
	if got := getAliasedTypeName("dep.Table"); got != "dep.Table" {
		t.Fatalf("a declined re-export must render unchanged, got %q", got)
	}

	// A derived alias declares its `global using` only once a reference has resolved through it.
	if !usedImportedTypeAliases.Contains("dep.Thing") {
		t.Fatal("a referenced derived alias must be marked used so its global using is emitted")
	}

	if usedImportedTypeAliases.Contains("dep.Stream") {
		t.Fatal("an unreferenced derived alias must not be marked used")
	}
}

func aliasPairs(aliases [][2]string) map[string]string {
	pairs := make(map[string]string, len(aliases))

	for _, alias := range aliases {
		pairs[alias[0]] = alias[1]
	}

	return pairs
}

// loadMethodNamedFixtureDependency materializes a two-package module whose `dep` package declares a
// const, three defined types and a methodless func type, each named like a METHOD of the package,
// loads the CONSUMER, and returns the loaded dependency as the consumer sees it.
func loadMethodNamedFixtureDependency(t *testing.T) *packages.Package {
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

type Set[T comparable] map[T]struct{}

type Clock struct{}

func (Clock) Second() int      { return 0 }
func (Clock) Month() Month     { return 0 }
func (Clock) Token() Token     { return nil }
func (Clock) Filter() Filter   { return nil }
func (Clock) Duration() string { return "" }
func (Clock) Set(int)          {}
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

// A package-level name that is also a METHOD name of its package is spelled as Go wrote it by every
// consumer. Until 2026-11-11 the methods were members of the package class, the element took a
// marker there (`time.ΔSecond`, `time.ΔMonth`), and a consumer converted without its dependency had
// to derive that rename from the dependency's own declarations. A method is now a member of its
// receiver type's holder, so the dependency publishes no rename, and what it does publish for such
// a name is what it publishes for any other: a defined type over the empty interface is `object`.
func TestForeignNameSharedWithAMethodIsNotRenamed(t *testing.T) {
	isolateConverterPathGlobals(t)
	resetPackageState(&packages.Package{})
	foreignDerivedAliases = nil

	dependency := loadMethodNamedFixtureDependency(t)
	derived := foreignDerivedTypeAliases(dependency)

	for _, alias := range derived {
		if strings.Contains(alias[0]+alias[1], ShadowVarMarker) {
			t.Fatalf("no derived entry may carry a marker, got %q -> %q", alias[0], alias[1])
		}
	}

	pairs := aliasPairs(derived)

	// The const, the defined types and the generic type are members of the package class under
	// their Go names, and a methodless func type is rendered as its delegate: none takes an alias.
	for _, name := range []string{"Second", "Nanosecond", "Month", "Duration", "Filter", "Set"} {
		if got, published := pairs[name]; published {
			t.Fatalf("%s must publish no alias, got %q", name, got)
		}
	}

	if got := pairs["Token"]; got != "object" {
		t.Fatalf("a defined type over the empty interface must publish object, got %q", got)
	}

	// End-to-end at the decision that emits: a consumer with NO package_info.cs to read.
	info := PackageInfo{PackageName: "example.collision.dep", RootPackageName: "dep", SourceDir: dependency.Dir}
	applyExportedTypeAliases(derived, info)

	for _, name := range []string{"dep.Second", "dep" + PackageSuffix + ".Second", "dep.Month", "dep.Duration", "dep.Nanosecond"} {
		if got := getAliasedTypeName(name); got != name {
			t.Fatalf("%s must render as Go wrote it, got %q", name, got)
		}
	}

	if got := importedTypeAliases["dep.Token"]; got != "object" {
		t.Fatalf("the empty-interface type must alias to bare object, got %q", got)
	}
}
