// requiresUnsafeMarker_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards for [module: go.GoRequiresUnsafe] — the declaration by which a HAND-OWNED file tells the
// converter that its package needs `/unsafe`, which the converter's own `usesUnsafeCode` predicate
// cannot observe because that predicate only sees code the converter emitted.
//
// The failure this guards against is silent in both directions and expensive in both. Too LOW and a
// package with a hand-owned [LibraryImport] cannot compile at all (SYSLIB1062), with the only
// obvious remedy — editing the .csproj — undone by the next reconvert overlay. Too HIGH and every
// project in the corpus quietly grants /unsafe it does not need.

package main

import (
	"fmt"
	"os"
	"path/filepath"
	"strconv"
	"strings"
	"testing"
)

// writeUnsafeFixture builds a converted-package directory on disk out of a path→contents map (paths
// are slash-separated and relative to the package directory) and returns it.
func writeUnsafeFixture(t *testing.T, files map[string]string) string {
	t.Helper()

	packageDir := filepath.Join(t.TempDir(), "pkg")

	for name, contents := range files {
		fullPath := filepath.Join(packageDir, filepath.FromSlash(name))

		if err := os.MkdirAll(filepath.Dir(fullPath), 0755); err != nil {
			t.Fatalf("failed to create fixture directory for %q: %v", name, err)
		}

		if err := os.WriteFile(fullPath, []byte(contents), 0644); err != nil {
			t.Fatalf("failed to write fixture file %q: %v", name, err)
		}
	}

	return packageDir
}

// renderedAllowUnsafeBlocks applies the SAME two-stage substitution writeProjectFile performs — the
// template's printf verbs, then the >>MARKER:UNSAFE<< replacement — and reads the property back out
// of the rendered project. What is asserted is therefore the .csproj that reaches disk, not the
// predicate feeding it.
func renderedAllowUnsafeBlocks(t *testing.T, packageDir string) string {
	t.Helper()

	// The template's unsafe verb is filled with the MARKER, exactly as prepareProjectFiles fills it,
	// so that the second stage below is the same substitution writeProjectFile performs. (The shared
	// renderCsprojTemplate helper hardcodes "false" there, which would make this guard vacuous.)
	contents := fmt.Sprintf(string(csprojTemplate), "Library", "go", "TestProject", UnsafeMarker, "")
	contents = strings.ReplaceAll(contents, ValidationPackMarker, "")

	if !strings.Contains(contents, UnsafeMarker) {
		t.Fatal("test setup: the rendered template carries no unsafe marker to substitute")
	}

	contents = strings.ReplaceAll(contents, UnsafeMarker, strconv.FormatBool(allowUnsafeBlocks(packageDir)))

	if strings.Contains(contents, UnsafeMarker) {
		t.Fatal("the rendered project still carries the unsubstituted unsafe marker")
	}

	properties := propertyGroupsOf(t, "csproj-template.xml", contents)

	if _, found := properties.conditionOf("AllowUnsafeBlocks"); !found {
		t.Fatal("the rendered project sets no <AllowUnsafeBlocks> at all")
	}

	return strings.TrimSpace(properties.value("AllowUnsafeBlocks"))
}

// A package whose ONLY unsafe need is a hand-own declaration gets AllowUnsafeBlocks=true. This is
// the whole point of the mechanism: `usesUnsafeCode` is false here (nothing was converted at all),
// so before the marker there was no path to a `true` that survived a reconvert.
func TestHandOwnDeclarationRaisesAllowUnsafeBlocks(t *testing.T) {
	usesUnsafeCode = false
	t.Cleanup(func() { usesUnsafeCode = false })

	packageDir := writeUnsafeFixture(t, map[string]string{
		"syscall_impl.cs": "using System.Runtime.InteropServices;\r\n\r\n" +
			"[module: go.GoManualConversion]\r\n" +
			"[module: go.GoRequiresUnsafe]\r\n\r\n" +
			"partial class syscall_package {}\r\n",
	})

	if got := renderedAllowUnsafeBlocks(t, packageDir); got != "true" {
		t.Errorf("<AllowUnsafeBlocks> = %q, want \"true\" for a package whose hand-own declares GoRequiresUnsafe", got)
	}
}

// The negative control the mechanism is only meaningful against: the SAME file with the attribute
// removed drops the flag back to false. Without this the guard above would also pass if the
// predicate simply returned true for any directory containing a .cs file.
func TestRemovingTheDeclarationDropsAllowUnsafeBlocks(t *testing.T) {
	usesUnsafeCode = false
	t.Cleanup(func() { usesUnsafeCode = false })

	const declared = "using System.Runtime.InteropServices;\r\n\r\n" +
		"[module: go.GoManualConversion]\r\n" +
		"[module: go.GoRequiresUnsafe]\r\n\r\n" +
		"partial class syscall_package {}\r\n"

	packageDir := writeUnsafeFixture(t, map[string]string{"syscall_impl.cs": declared})

	if got := renderedAllowUnsafeBlocks(t, packageDir); got != "true" {
		t.Fatalf("test setup: <AllowUnsafeBlocks> = %q with the declaration present, want \"true\"", got)
	}

	withoutMarker := strings.Replace(declared, "[module: go.GoRequiresUnsafe]\r\n", "", 1)

	if withoutMarker == declared {
		t.Fatal("test setup: the declaration line was not found in the fixture")
	}

	if err := os.WriteFile(filepath.Join(packageDir, "syscall_impl.cs"), []byte(withoutMarker), 0644); err != nil {
		t.Fatalf("failed to rewrite the fixture: %v", err)
	}

	if got := renderedAllowUnsafeBlocks(t, packageDir); got != "false" {
		t.Errorf("<AllowUnsafeBlocks> = %q after removing the declaration, want \"false\"", got)
	}
}

// A package with NEITHER an unsafe emission nor a declaration stays false — the state of all but a
// handful of the corpus's 300-odd projects, so a regression here is a corpus-wide one.
func TestPackageWithNeitherStaysSafe(t *testing.T) {
	usesUnsafeCode = false
	t.Cleanup(func() { usesUnsafeCode = false })

	packageDir := writeUnsafeFixture(t, map[string]string{
		"strings.cs":       "namespace go;\r\n\r\npartial class strings_package {}\r\n",
		"package_info.cs":  "namespace go;\r\n\r\npartial class strings_package {}\r\n",
		"strings.cs.auto":  "[module: go.GoRequiresUnsafe]\r\npartial class strings_package {}\r\n",
		"windows/os_ᴡ.cs": "// a comment mentioning [module: go.GoRequiresUnsafe] in prose\r\n" +
			"namespace go;\r\n\r\npartial class strings_package {}\r\n",
	})

	if got := renderedAllowUnsafeBlocks(t, packageDir); got != "false" {
		t.Errorf("<AllowUnsafeBlocks> = %q, want \"false\": neither the emission nor any hand-own needs /unsafe", got)
	}
}

// The converter's own emission still raises the flag on its own — the pre-existing behavior this
// change must not disturb. It is a UNION, not a replacement.
func TestConverterEmissionStillRaisesAllowUnsafeBlocksAlone(t *testing.T) {
	usesUnsafeCode = true
	t.Cleanup(func() { usesUnsafeCode = false })

	packageDir := writeUnsafeFixture(t, map[string]string{
		"unsafe_ops.cs": "namespace go;\r\n\r\npartial class runtime_package {}\r\n",
	})

	if got := renderedAllowUnsafeBlocks(t, packageDir); got != "true" {
		t.Errorf("<AllowUnsafeBlocks> = %q with usesUnsafeCode set, want \"true\"", got)
	}
}

// A hand-own declaring the marker inside a per-GOOS source folder raises the flag for the package.
// Layout L3 routes a platform-varying hand-own into `<goos>/`, and the .csproj is ONE file serving
// every $(GoTargetOS), so a scan that only looked flat would miss exactly the files this mechanism
// exists for — internal/runtime/syscall's Linux keystone lives at `linux/syscall_linux_impl.cs`.
func TestPerGOOSHandOwnDeclarationIsFound(t *testing.T) {
	usesUnsafeCode = false
	t.Cleanup(func() { usesUnsafeCode = false })

	packageDir := writeUnsafeFixture(t, map[string]string{
		"package_info.cs": "namespace go;\r\n\r\npartial class syscall_package {}\r\n",
		"linux/syscall_linux_impl.cs": "using System.Runtime.InteropServices;\r\n\r\n" +
			"[module: go.GoManualConversion]\r\n" +
			"[module: go.GoRequiresUnsafe]\r\n\r\n" +
			"partial class syscall_package {}\r\n",
	})

	if got := renderedAllowUnsafeBlocks(t, packageDir); got != "true" {
		t.Errorf("<AllowUnsafeBlocks> = %q, want \"true\" for a declaration in a per-GOOS source folder", got)
	}
}

// A NESTED PACKAGE's declaration must NOT leak into its parent's project file. `internal/runtime`
// holds `syscall` as a child package with its own .csproj, and the discriminator is exactly the one
// layout L3 uses everywhere else (a per-GOOS folder holds no project file). Getting this wrong would
// silently grant /unsafe to every ancestor of a hand-own — including, at the top, `internal`.
func TestNestedPackageDeclarationDoesNotLeakToParent(t *testing.T) {
	usesUnsafeCode = false
	t.Cleanup(func() { usesUnsafeCode = false })

	packageDir := writeUnsafeFixture(t, map[string]string{
		"runtime.cs": "namespace go;\r\n\r\npartial class runtime_package {}\r\n",
		// A child package that HAPPENS to be named after a GOOS — the case isPlatformSourceFolder
		// exists for (internal/syscall/windows is real). Its .csproj is what answers for it.
		"windows/internal.syscall.windows.csproj": "<Project Sdk=\"Microsoft.NET.Sdk\" />\r\n",
		"windows/zsyscall_windows_impl.cs": "[module: go.GoManualConversion]\r\n" +
			"[module: go.GoRequiresUnsafe]\r\n\r\n" +
			"partial class windows_package {}\r\n",
	})

	if got := renderedAllowUnsafeBlocks(t, packageDir); got != "false" {
		t.Errorf("<AllowUnsafeBlocks> = %q; a NESTED package's declaration must not raise its parent's flag", got)
	}
}

// The marker shares the hand-own marker's comment lexer rather than duplicating a scan, so it
// inherits the r48b fix: a `/*` that is ordinary prose inside a `//` line comment opens no block, and
// a marker mentioned in a comment is not a declaration. Both directions are asserted here because
// the two failures are opposite and both silent — one grants /unsafe nobody asked for, the other
// leaves a hand-own uncompilable with no diagnostic naming the cause.
func TestRequiresUnsafeMarkerSharesTheHandOwnCommentLexer(t *testing.T) {
	assertRequiresUnsafe := func(t *testing.T, want bool, contents string) {
		t.Helper()

		fixture := filepath.Join(t.TempDir(), "probe.cs")

		if err := os.WriteFile(fixture, []byte(contents), 0644); err != nil {
			t.Fatalf("failed to write fixture: %v", err)
		}

		got, err := containsRequiresUnsafeMarker(fixture)

		if err != nil {
			t.Fatalf("containsRequiresUnsafeMarker: %v", err)
		}

		if got != want {
			t.Errorf("containsRequiresUnsafeMarker = %v, want %v, for:\n%s", got, want, contents)
		}
	}

	// Declared, in every spelling C# accepts.
	assertRequiresUnsafe(t, true, "[module: go.GoRequiresUnsafe]\r\npartial class p {}\r\n")
	assertRequiresUnsafe(t, true, "[module: GoRequiresUnsafe]\r\npartial class p {}\r\n")
	assertRequiresUnsafe(t, true, "[module: go.GoRequiresUnsafeAttribute]\r\npartial class p {}\r\n")
	assertRequiresUnsafe(t, true, "[ module : go.GoRequiresUnsafe ]\r\npartial class p {}\r\n")

	// The r48b shape: prose containing `/*` inside a LINE comment opens no block comment, so the
	// real declaration below it is still seen.
	assertRequiresUnsafe(t, true, "// hide a *g/*p/*m inside a uintptr\r\n"+
		"[module: go.GoRequiresUnsafe]\r\npartial class p {}\r\n")

	// Mentioned, never declared.
	assertRequiresUnsafe(t, false, "// see [module: go.GoRequiresUnsafe] for the mechanism\r\npartial class p {}\r\n")
	assertRequiresUnsafe(t, false, "/* [module: GoRequiresUnsafe] */\r\npartial class p {}\r\n")
	assertRequiresUnsafe(t, false, "using go; // [module: GoRequiresUnsafe]\r\npartial class p {}\r\n")

	// The two markers are distinct: a hand-own marker is not an unsafe declaration, and vice versa.
	assertRequiresUnsafe(t, false, "[module: go.GoManualConversion]\r\npartial class p {}\r\n")

	manualOnly := filepath.Join(t.TempDir(), "manual.cs")

	if err := os.WriteFile(manualOnly, []byte("[module: go.GoRequiresUnsafe]\r\npartial class p {}\r\n"), 0644); err != nil {
		t.Fatalf("failed to write fixture: %v", err)
	}

	if manual, err := containsManualConversionMarker(manualOnly); err != nil || manual {
		t.Errorf("containsManualConversionMarker = %v (err %v) for a GoRequiresUnsafe-only file, want false", manual, err)
	}
}

// The marker NAME is one string shared by a C# attribute class and a Go regexp, which is why it
// lives in the canonical symbol table (src/core/go2cs/symbols.json) rather than being spelled out in
// either. This asserts the two halves still agree — a rename on one side alone would leave the
// converter scanning for an attribute nobody can apply, with no compile error anywhere.
func TestMarkerAttributesExistInGolibUnderTheirSymbolNames(t *testing.T) {
	// Resolved against the test's own working directory (src/go2cs), like projitemsIntegrity_test.go
	// — NOT fixturePath, whose root is a synthetic string used only to compose emitted paths.
	for _, marker := range []string{ManualConversionMarker, RequiresUnsafeMarker} {
		attributePath := filepath.Join("..", "core", "golib", marker+"Attribute.cs")
		contents, err := os.ReadFile(attributePath)

		if err != nil {
			t.Errorf("golib declares no attribute for marker %q (%s): %v", marker, attributePath, err)
			continue
		}

		if !strings.Contains(string(contents), "class "+marker+"Attribute") {
			t.Errorf("%s does not declare `class %sAttribute`", attributePath, marker)
		}

		if !strings.Contains(string(contents), "AttributeTargets.Module") {
			t.Errorf("%s is not a MODULE-scoped attribute; the converter's scan reads `[module: …]`", attributePath)
		}
	}
}
