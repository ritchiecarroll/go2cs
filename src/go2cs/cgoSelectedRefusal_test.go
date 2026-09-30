// conversionDriver_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// A cgo source has no C# conversion yet. When the loader SELECTS an `import "C"` file for this
// GOOS/GOARCH/tag set, go/packages hands the driver its cgo-generated intermediates instead of the
// file, and the driver used to skip them with a warning and "convert best-effort without them" -- a
// package that compiles and is missing code, which is the worst answer a converter can give. The
// ruled answer (owner, ruling 578 item 7) is a NON-ZERO exit that names the file, the package and
// the missing conversion.
//
// What must stay quiet, in every direction, is anything the build never selects: a test-only file,
// a platform-deselected file, and everything the corpus convert (CGO_ENABLED=0) never selects. The
// fixtures write REAL `import "C"` sources for those cases on purpose, so an implementation that
// scanned the package directory for the import would fail them; the decision is made from what the
// loader selected, which is exactly what the build compiles.

package main

import (
	"errors"
	"go/ast"
	"go/token"
	"os"
	"os/exec"
	"path/filepath"
	"strings"
	"testing"

	"golang.org/x/tools/go/packages"
)

const cgoImportSource = "package p\n\n/*\nint one(void) { return 1; }\n*/\nimport \"C\"\n\nfunc One() int { return int(C.one()) }\n"

func writeCgoFixture(t *testing.T, dir, name, contents string) string {
	t.Helper()

	path := filepath.Join(dir, name)

	if err := os.WriteFile(path, []byte(contents), 0o644); err != nil {
		t.Fatalf("cannot write fixture %s: %v", path, err)
	}

	return path
}

func TestSelectedCgoSourceIsRefusedByName(t *testing.T) {
	dir := t.TempDir()
	cacheDir := filepath.Join(dir, "go-build", "23")

	plainPath := writeCgoFixture(t, dir, "user.go", "package user\n")
	cgoPath := writeCgoFixture(t, dir, "cgo_lookup_cgo.go", cgoImportSource)

	// go/packages lists a cgo package's `import "C"` source among GoFiles, and replaces it in the
	// compiled files (and so in Syntax) with content-hashed build-cache objects, not <name>.cgo1.go.
	fset := token.NewFileSet()
	cgo1Path := filepath.Join(cacheDir, "232ab32eba32101433d3549fd886528b94f5a3bfcbcd325391c43c420958a0c3-d")
	gotypesPath := filepath.Join(cacheDir, "54357aa138be876d61fd82e3d2b396d1ab2fecd7da7581da8b7d21d2fcd0763e-d")

	pkg := &packages.Package{
		PkgPath:         "os/user",
		Dir:             dir,
		GoFiles:         []string{plainPath, cgoPath},
		CompiledGoFiles: []string{plainPath, cgo1Path, gotypesPath},
		Syntax: []*ast.File{
			newPairingTestSyntaxFile(fset, plainPath),
			newPairingTestSyntaxFile(fset, cgo1Path),
			newPairingTestSyntaxFile(fset, gotypesPath),
		},
		Fset: fset,
	}

	paired, _ := syntaxSourceFiles(pkg)
	err := refuseSelectedCgoSources(pkg, paired)

	if err == nil {
		t.Fatalf("a selected `import \"C\"` file was skipped without a refusal")
	}

	for _, want := range []string{"cgo_lookup_cgo.go", "os/user", "no C# conversion", "CGO_ENABLED=0"} {
		if !strings.Contains(err.Error(), want) {
			t.Errorf("refusal %q does not name %q", err.Error(), want)
		}
	}
}

func TestATestOnlyCgoFileStaysQuiet(t *testing.T) {
	dir := t.TempDir()

	plainPath := writeCgoFixture(t, dir, "p.go", "package p\n")
	writeCgoFixture(t, dir, "cgo_test.go", cgoImportSource)

	fset := token.NewFileSet()

	pkg := &packages.Package{
		PkgPath:         "p",
		Dir:             dir,
		GoFiles:         []string{plainPath},
		CompiledGoFiles: []string{plainPath},
		Syntax:          []*ast.File{newPairingTestSyntaxFile(fset, plainPath)},
		Fset:            fset,
	}

	paired, _ := syntaxSourceFiles(pkg)

	if err := refuseSelectedCgoSources(pkg, paired); err != nil {
		t.Errorf("a test-only import \"C\" file is not part of the package build, got refusal: %v", err)
	}
}

func TestAPlatformDeselectedCgoFileStaysQuiet(t *testing.T) {
	dir := t.TempDir()

	plainPath := writeCgoFixture(t, dir, "p.go", "package p\n")
	deselected := writeCgoFixture(t, dir, "cgo_plan9.go", cgoImportSource)

	fset := token.NewFileSet()

	pkg := &packages.Package{
		PkgPath:         "p",
		Dir:             dir,
		GoFiles:         []string{plainPath},
		CompiledGoFiles: []string{plainPath},
		IgnoredFiles:    []string{deselected},
		Syntax:          []*ast.File{newPairingTestSyntaxFile(fset, plainPath)},
		Fset:            fset,
	}

	paired, _ := syntaxSourceFiles(pkg)

	if err := refuseSelectedCgoSources(pkg, paired); err != nil {
		t.Errorf("a platform-deselected import \"C\" file is not part of this build, got refusal: %v", err)
	}
}

// A generated file that is not a cgo intermediate keeps its old treatment (the warning), so the
// refusal is exactly the cgo class and nothing else.
func TestANonCgoGeneratedFileIsNotRefused(t *testing.T) {
	dir := t.TempDir()

	plainPath := writeCgoFixture(t, dir, "p.go", "package p\n")
	generated := filepath.Join(dir, "go-build", "b001", "p_generated.go")

	fset := token.NewFileSet()

	pkg := &packages.Package{
		PkgPath:         "p",
		Dir:             dir,
		GoFiles:         []string{plainPath},
		CompiledGoFiles: []string{plainPath, generated},
		Syntax: []*ast.File{
			newPairingTestSyntaxFile(fset, plainPath),
			newPairingTestSyntaxFile(fset, generated),
		},
		Fset: fset,
	}

	paired, skipped := syntaxSourceFiles(pkg)

	if len(skipped) != 1 {
		t.Fatalf("premise: the generated file must be skipped, got %v", skipped)
	}

	if err := refuseSelectedCgoSources(pkg, paired); err != nil {
		t.Errorf("a non-cgo generated file must only warn, got refusal: %v", err)
	}
}

// A selected file that has no syntax tree but does not import "C" (one that failed to parse, say) is a
// different fault with its own reporting; it must not be mislabelled as a cgo source.
func TestAGoFileWithoutASyntaxTreeThatIsNotCgoIsNotRefused(t *testing.T) {
	dir := t.TempDir()

	plainPath := writeCgoFixture(t, dir, "p.go", "package p\n")
	brokenPath := writeCgoFixture(t, dir, "broken.go", "package p\n\nfunc {\n")

	fset := token.NewFileSet()

	pkg := &packages.Package{
		PkgPath:         "p",
		Dir:             dir,
		GoFiles:         []string{plainPath, brokenPath},
		CompiledGoFiles: []string{plainPath, brokenPath},
		Syntax:          []*ast.File{newPairingTestSyntaxFile(fset, plainPath)},
		Fset:            fset,
	}

	paired, _ := syntaxSourceFiles(pkg)

	if err := refuseSelectedCgoSources(pkg, paired); err != nil {
		t.Errorf("a non-cgo file without a syntax tree must not be refused as cgo: %v", err)
	}
}

// The same questions asked of a REAL go/packages load, so the premise the unit fixtures rest on (a
// selected cgo file stays in GoFiles but is parsed into no syntax tree, its content-hashed cache objects
// standing in; a platform-deselected one lands in IgnoredFiles) is read from the toolchain rather than
// assumed. A test-only cgo file is not exercised here because the Go toolchain itself refuses one
// ("use of cgo in test not supported"), which spoils the whole load; the unit fixture covers it.
// Skipped where there is no C compiler.
func TestRealLoadRefusesOnlyASelectedCgoFile(t *testing.T) {
	if _, err := exec.LookPath("gcc"); err != nil {
		if _, err := exec.LookPath("cc"); err != nil {
			t.Skip("no C compiler: a real cgo load is not possible here")
		}
	}

	dir := t.TempDir()

	writeCgoFixture(t, dir, "go.mod", "module cgofixture\n\ngo 1.24\n")
	writeCgoFixture(t, dir, "p.go", "package p\n")
	writeCgoFixture(t, dir, "cgo_here.go", cgoImportSource)
	writeCgoFixture(t, dir, "cgo_plan9.go", strings.Replace(cgoImportSource, "func One", "func Two", 1))

	load := func(cgo string) (*packages.Package, error) {
		cfg := &packages.Config{
			Dir:  dir,
			Mode: packages.NeedName | packages.NeedFiles | packages.NeedCompiledGoFiles | packages.NeedSyntax | packages.NeedTypes | packages.NeedImports | packages.NeedDeps,
			Env:  append(os.Environ(), "CGO_ENABLED="+cgo, "GOOS=linux", "GOARCH=amd64", "GOFLAGS=-mod=mod"),
		}

		pkgs, err := packages.Load(cfg, ".")

		if err != nil || len(pkgs) != 1 {
			return nil, errors.Join(err, errors.New("load did not return exactly one package"))
		}

		return pkgs[0], nil
	}

	enabled, err := load("1")

	if err != nil {
		t.Fatalf("CGO_ENABLED=1 load: %v", err)
	}

	paired, _ := syntaxSourceFiles(enabled)
	refusal := refuseSelectedCgoSources(enabled, paired)

	if refusal == nil || !strings.Contains(refusal.Error(), "cgo_here.go") {
		t.Errorf("CGO_ENABLED=1: the selected cgo_here.go must be refused by name, got %v", refusal)
	}

	if refusal != nil && strings.Contains(refusal.Error(), "cgo_plan9.go") {
		t.Errorf("CGO_ENABLED=1: the refusal names an unselected file: %v", refusal)
	}

	disabled, err := load("0")

	if err != nil {
		t.Fatalf("CGO_ENABLED=0 load: %v", err)
	}

	paired, _ = syntaxSourceFiles(disabled)

	if refusal := refuseSelectedCgoSources(disabled, paired); refusal != nil {
		t.Errorf("CGO_ENABLED=0 selects no cgo file, so the corpus convert must be quiet, got %v", refusal)
	}
}
