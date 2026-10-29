// seededAliasNameCollision_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// go2cs keys an imported type alias by SHORT NAME — `<rootPackageName>.<member>` — and declares it
// as a `global using` whose identifier is that key with TypeAliasDot for the `.`. Two packages that
// share a base name and export the same type name therefore want ONE identifier for two types, and
// C# gives an alias exactly one meaning per compilation.
//
// The shape is invisible until a `-tests` RECOMPILE-model conversion, which is the only place two
// packages' import views meet in one file: package_test_info.cs is SEEDED from the production
// package_info.cs (the production `.cs` are compile items of the test assembly and are not
// regenerated, so their alias declarations must carry over verbatim), and each test variant's own
// aliases are then MERGED into that same section. crypto/ecdh is the corpus's first instance — it
// imports crypto/internal/fips140/ecdh, whose package name is also `ecdh` and which also exports
// `PublicKey` — and the merge produced two declarations of `ecdhꓸPublicKey` with different targets,
// CS1537, the row's only build error.
//
// Both halves are armed here: the RESOLUTION (a variant's colliding alias renders fully qualified
// and declares nothing) and the GUARD (a duplicate reaching the writer from any other source is
// refused by name rather than emitted as C# that cannot compile).

package main

import (
	"fmt"
	"os"
	"os/exec"
	"path/filepath"
	"strings"
	"testing"

	"golang.org/x/tools/go/packages"
)

const duplicateGlobalUsingProbeEnv = "GO2CS_DUPLICATE_GLOBAL_USING_PROBE"

// The two real bindings, verbatim from the emitted crypto/ecdh files at the Go 1.24.13 tip.
const (
	ecdhAliasName        = "ecdh" + TypeAliasDot + "PublicKey"
	ecdhProductionTarget = RootNamespace + ".crypto.@internal.fips140.ecdh" + PackageSuffix + "." + ShadowVarMarker + "PublicKey"
	ecdhTestTarget       = RootNamespace + ".crypto.ecdh" + PackageSuffix + "." + ShadowVarMarker + "PublicKey"
)

// seedTestPackageInfo writes a package_test_info.cs fixture whose <ImportedTypeAliases> section
// already carries `seeded` — the state the recompile-model seed leaves behind — and returns its
// path. Every marker section writePackageInfoFile rebuilds is present, since a missing one is fatal.
func seedAliasCollisionTestInfo(t *testing.T, dir string, seeded ...string) string {
	t.Helper()

	fileName := filepath.Join(dir, testPackageInfoFileName)

	lines := []string{
		"namespace go.crypto;",
		"",
		"// <ImportedTypeAliases>",
	}

	lines = append(lines, seeded...)
	lines = append(lines,
		"// </ImportedTypeAliases>",
		"",
		"// <ExportedTypeAliases>",
		"// </ExportedTypeAliases>",
		"",
		"// <InterfaceImplementations>",
		"// </InterfaceImplementations>",
		"",
		"// <ImplicitConversions>",
		"// </ImplicitConversions>",
		"",
		"[GoPackage(\"ecdh\")]",
		"public static partial class ecdh"+PackageSuffix+" {",
		"",
		"    // <TypeAccessibility>",
		"    // </TypeAccessibility>",
		"",
		"    // <ImportInitializers>",
		"    // </ImportInitializers>",
		"}",
		"",
	)

	if err := os.WriteFile(fileName, []byte(strings.Join(lines, "\r\n")), 0644); err != nil {
		t.Fatalf("failed to seed the test package info fixture: %v", err)
	}

	return fileName
}

// emittedImportedAliases returns the `global using` lines the written file carries in its
// <ImportedTypeAliases> section.
func emittedImportedAliases(t *testing.T, fileName string) []string {
	t.Helper()

	contents, err := os.ReadFile(fileName)

	if err != nil {
		t.Fatalf("failed to read the written package info file: %v", err)
	}

	var (
		section   []string
		inSection bool
	)

	for _, line := range splitLines(string(contents)) {
		trimmed := strings.TrimSpace(line)

		if strings.Contains(trimmed, "<ImportedTypeAliases>") {
			inSection = true
			continue
		}

		if strings.Contains(trimmed, "</ImportedTypeAliases>") {
			break
		}

		if inSection && strings.HasPrefix(trimmed, "global using ") {
			section = append(section, trimmed)
		}
	}

	return section
}

// applyEcdhTestAlias records what the EXTERNAL test variant's import of crypto/ecdh records: the
// package's own exported `PublicKey`, through the same entry point a real conversion uses.
func applyEcdhTestAlias() {
	applyExportedTypeAliases([][2]string{{"PublicKey", ShadowVarMarker + "PublicKey"}},
		PackageInfo{PackageName: "crypto.ecdh", RootPackageName: "ecdh", SourceDir: "crypto/ecdh"})
}

// TestSeededAliasNameCollisionDeclaresOneName is the arm that pays for this file. The production
// binding is immovable — nist.cs and ecdh.cs are compile items of the same assembly and reference
// it — so the variant's binding is the one that must give way, and it gives way by rendering
// FULLY QUALIFIED, which needs no declaration at all.
//
// Reverting either half reproduces the defect: without the writer's skip the section carries two
// declarations of one name, and without getAliasedTypeName's qualified return the emitted sources
// spell `ecdhꓸPublicKey` for a type that identifier does not name.
func TestSeededAliasNameCollisionDeclaresOneName(t *testing.T) {
	isolateConverterPathGlobals(t)
	dir := t.TempDir()
	fileName := seedAliasCollisionTestInfo(t, dir, fmt.Sprintf("global using %s = %s;", ecdhAliasName, ecdhProductionTarget))

	resetPackageState(&packages.Package{})
	packageName = "ecdh"
	packageNamespace = "go.crypto"

	seeded, err := os.ReadFile(fileName)

	if err != nil {
		t.Fatalf("failed to read the seeded fixture: %v", err)
	}

	seededGlobalTypeAliases = parseSeededGlobalTypeAliasLines(splitLines(string(seeded)))
	t.Cleanup(func() { seededGlobalTypeAliases = nil })

	if got := seededGlobalTypeAliases[ecdhAliasName]; got != ecdhProductionTarget {
		t.Fatalf("the seed parse did not read the production binding: %q", got)
	}

	applyEcdhTestAlias()

	// The entry is still in the map — every existence check that routes a reference through the
	// alias machinery must keep seeing it — but it is marked.
	if got, exists := importedTypeAliases["ecdh.PublicKey"]; !exists || got != ecdhTestTarget {
		t.Fatalf("the variant's alias entry is missing or wrong: %q (exists=%v)", got, exists)
	}

	if !qualifiedImportedTypeAliases.Contains("ecdh.PublicKey") {
		t.Fatalf("the colliding key was not marked: a `global using %s` is already bound to %q", ecdhAliasName, ecdhProductionTarget)
	}

	// THE PROPERTY, half one: the reference renders through the target, not through an identifier
	// the seed has already given a different meaning.
	if got := getAliasedTypeName("ecdh.PublicKey"); got != ecdhTestTarget {
		t.Errorf("a colliding foreign type must render fully qualified, got %q, want %q", got, ecdhTestTarget)
	}

	// THE PROPERTY, half two: exactly one declaration of that name survives, and it is the seeded
	// production one.
	writePackageInfoFile(fileName, true)

	declarations := emittedImportedAliases(t, fileName)
	matches := []string{}

	for _, declaration := range declarations {
		if strings.Contains(declaration, ecdhAliasName+" =") {
			matches = append(matches, declaration)
		}
	}

	if len(matches) != 1 {
		t.Fatalf("the <ImportedTypeAliases> section declares %s %d times, want 1:\n%s", ecdhAliasName, len(matches), strings.Join(declarations, "\n"))
	}

	if !strings.Contains(matches[0], ecdhProductionTarget) {
		t.Errorf("the surviving declaration must be the seeded production binding, got %q", matches[0])
	}
}

// TestSeededAliasNameCollisionLeavesConstKeysUnmarked is the CONST arm: the marking reaches only the
// keys the writer can DECLARE, and a `const:` entry is not one of them. The writer skips every const
// key outright (packageInfoWriter.go, the `continue` above the qualified one), so a const key can
// never be the second declaration of a `global using` name and marking it resolves nothing.
//
// It COSTS, though. getAliasedTypeName tests isQualified BEFORE isConst, so a marked const key
// returns the map VALUE — the bare Δ-renamed member, which the `const:` branch deliberately leaves
// un-qualified (importOperations.go) — where the const arm composes
// `importQualifier(qualifier) + "." + member`. The bare member names nothing at compilation scope
// (CS0103), and the `_package`-qualified const path further down the SAME function keeps composing
// its qualifier regardless, as does convIdent.go's const lookup: one decision, spelled three ways,
// two of which would still be right. This arm pins the plain path to the qualified answer and then
// asks the qualified path the same question, so a future reordering cannot make them disagree
// quietly.
//
// The shape: production binds the alias NAME non-const (the seed), while a variant binds that same
// name as a const entry. Not reachable in today's corpus — its one genuine pair is a TYPE on both
// sides — which is why this is an arm and not a row. From C1's structural read of this seat.
func TestSeededAliasNameCollisionLeavesConstKeysUnmarked(t *testing.T) {
	isolateConverterPathGlobals(t)
	dir := t.TempDir()
	fileName := seedAliasCollisionTestInfo(t, dir, fmt.Sprintf("global using %s = %s;", ecdhAliasName, ecdhProductionTarget))

	resetPackageState(&packages.Package{})
	packageName = "ecdh"
	packageNamespace = "go.crypto"

	// The import qualifier is Δ-renamed, which is the population const entries exist for at all
	// (collision-renamed members) and what makes the assertion below discriminating: the const arm's
	// answer then differs from the bare member in BOTH segments, so neither can be read for the other.
	packageImportAliasRenames["ecdh"] = ShadowVarMarker + "ecdh"

	seeded, err := os.ReadFile(fileName)

	if err != nil {
		t.Fatalf("failed to read the seeded fixture: %v", err)
	}

	seededGlobalTypeAliases = parseSeededGlobalTypeAliasLines(splitLines(string(seeded)))
	t.Cleanup(func() { seededGlobalTypeAliases = nil })

	// The variant records the same member the seed already binds, as a CONST entry. The `const:`
	// branch strips the prefix and keeps the BARE member, so the recorded value differs from the
	// seeded target by construction and the key reaches the marking site.
	applyExportedTypeAliases([][2]string{{"PublicKey", "const:" + ShadowVarMarker + "PublicKey"}},
		PackageInfo{PackageName: "crypto.ecdh", RootPackageName: "ecdh", SourceDir: "crypto/ecdh"})

	if !constImportedTypeAliases.Contains("ecdh.PublicKey") {
		t.Fatalf("the fixture recorded no const entry, so this arm would assert nothing")
	}

	if got, exists := importedTypeAliases["ecdh.PublicKey"]; !exists || got != ShadowVarMarker+"PublicKey" {
		t.Fatalf("the const entry's map value is missing or wrong: %q (exists=%v)", got, exists)
	}

	// THE PROPERTY, half one: mark only what the writer can declare.
	if qualifiedImportedTypeAliases.Contains("ecdh.PublicKey") {
		t.Errorf("a CONST key was marked: the writer declares no `global using` for it, so it cannot re-bind %s", ecdhAliasName)
	}

	// THE PROPERTY, half two: it renders through the CONST arm — the qualified member — and not
	// through the marked arm's bare map value.
	wantConst := ShadowVarMarker + "ecdh." + ShadowVarMarker + "PublicKey"

	if got := getAliasedTypeName("ecdh.PublicKey"); got != wantConst {
		t.Errorf("a const alias must render through its qualifier, got %q, want %q", got, wantConst)
	}

	// ... and the `_package`-qualified const path, which composes its own qualifier and never
	// consults the marking, still answers the same question the same way.
	qualified := RootNamespace + ".crypto.ecdh" + PackageSuffix

	if got := getAliasedTypeName(qualified + ".PublicKey"); got != qualified+"."+ShadowVarMarker+"PublicKey" {
		t.Errorf("the `_package`-qualified const path disagrees with the plain one, got %q", got)
	}

	// THE PROPERTY, half three: the writer declares nothing for the const key — the seeded
	// production line is the only declaration of that name, and no line anywhere binds the bare
	// member. This half holds on both sides of the guard, and that is the point: the marking was
	// never load-bearing here, so it was pure cost.
	writePackageInfoFile(fileName, true)

	declarations := emittedImportedAliases(t, fileName)
	matches := []string{}

	for _, declaration := range declarations {
		if strings.Contains(declaration, ecdhAliasName+" =") {
			matches = append(matches, declaration)
		}

		if strings.HasSuffix(declaration, "= "+ShadowVarMarker+"PublicKey;") {
			t.Errorf("the writer declared a `global using` for a CONST key: %q", declaration)
		}
	}

	if len(matches) != 1 || !strings.Contains(matches[0], ecdhProductionTarget) {
		t.Fatalf("the seeded production binding must be the only declaration of %s:\n%s", ecdhAliasName, strings.Join(declarations, "\n"))
	}
}

// TestUncollidedAliasStillDeclaresItsName is the control, and it is what makes the arm above
// something other than a line count. crypto/ecdh's `Curve` travels the identical route — an
// exported type of the package under test, recorded by the same call, written by the same writer —
// and collides with nothing, so it must still declare its `global using` and still render through
// the alias identifier. A fix that suppressed declarations generally would pass the arm and fail
// here.
func TestUncollidedAliasStillDeclaresItsName(t *testing.T) {
	isolateConverterPathGlobals(t)
	dir := t.TempDir()
	fileName := seedAliasCollisionTestInfo(t, dir, fmt.Sprintf("global using %s = %s;", ecdhAliasName, ecdhProductionTarget))

	resetPackageState(&packages.Package{})
	packageName = "ecdh"
	packageNamespace = "go.crypto"

	seeded, err := os.ReadFile(fileName)

	if err != nil {
		t.Fatalf("failed to read the seeded fixture: %v", err)
	}

	seededGlobalTypeAliases = parseSeededGlobalTypeAliasLines(splitLines(string(seeded)))
	t.Cleanup(func() { seededGlobalTypeAliases = nil })

	applyExportedTypeAliases([][2]string{{"Curve", ShadowVarMarker + "Curve"}},
		PackageInfo{PackageName: "crypto.ecdh", RootPackageName: "ecdh", SourceDir: "crypto/ecdh"})

	if qualifiedImportedTypeAliases.Contains("ecdh.Curve") {
		t.Fatalf("`ecdh.Curve` collides with nothing and must not be marked")
	}

	wantName := "ecdh" + TypeAliasDot + "Curve"

	if got := getAliasedTypeName("ecdh.Curve"); got != wantName {
		t.Errorf("an uncollided foreign type must render through its alias, got %q, want %q", got, wantName)
	}

	writePackageInfoFile(fileName, true)

	declarations := emittedImportedAliases(t, fileName)
	found := false

	for _, declaration := range declarations {
		if strings.HasPrefix(declaration, "global using "+wantName+" = ") {
			found = true
		}
	}

	if !found {
		t.Errorf("the uncollided alias lost its declaration:\n%s", strings.Join(declarations, "\n"))
	}
}

// TestDuplicateGlobalUsingIsRefused arms the GUARD, in a CHILD PROCESS because the refusal is a
// log.Fatalf — the conversion cannot continue past a metadata file it knows will not compile, and
// an in-process assertion would take the test binary down with it.
//
// The child drives the writer with a section that already declares the name and a LIVE entry whose
// target disagrees, with the marking deliberately absent — the state the resolution above prevents,
// and the state any future third source of alias declarations would arrive in. What the parent
// checks is that the refusal NAMES the thing: the alias, and both targets, so the reader does not
// have to find the duplicate in a build log several stages downstream.
func TestDuplicateGlobalUsingIsRefused(t *testing.T) {
	if os.Getenv(duplicateGlobalUsingProbeEnv) == "1" {
		dir := t.TempDir()
		fileName := seedAliasCollisionTestInfo(t, dir, fmt.Sprintf("global using %s = %s;", ecdhAliasName, ecdhProductionTarget))

		resetPackageState(&packages.Package{})
		packageName = "ecdh"
		packageNamespace = "go.crypto"

		// No seededGlobalTypeAliases: nothing marks the key, so the writer meets the duplicate once a
		// reference resolves through the alias, which is what declares an imported alias at all.
		applyEcdhTestAlias()
		getAliasedTypeName("ecdh.PublicKey")

		writePackageInfoFile(fileName, true)

		fmt.Fprintln(os.Stdout, "the writer emitted a duplicate global using instead of refusing")

		return
	}

	probe := exec.Command(os.Args[0], "-test.run=^TestDuplicateGlobalUsingIsRefused$", "-test.timeout=60s")
	probe.Env = append(os.Environ(), duplicateGlobalUsingProbeEnv+"=1")

	output, err := probe.CombinedOutput()
	text := string(output)

	if err == nil {
		t.Fatalf("the writer did NOT refuse a duplicate `global using`:\n%s", text)
	}

	for _, want := range []string{ecdhAliasName, ecdhProductionTarget, ecdhTestTarget} {
		if !strings.Contains(text, want) {
			t.Errorf("the refusal does not name %q:\n%s", want, text)
		}
	}
}
