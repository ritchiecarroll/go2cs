// conversionDriver_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// Guards for the conversion driver's Syntax↔source pairing (syntaxSourceFiles).
//
// go/packages fills pkg.Syntax in parallel with CompiledGoFiles, NOT GoFiles. For a cgo package
// the two lists differ — each `import "C"` source is replaced in the compiled set by its
// cgo-generated build-cache intermediates (<name>.cgo1.go, _cgo_gotypes.go, …) — so Syntax
// outgrows GoFiles, and the driver's historical `pkg.GoFiles[i]` walk inside
// `for i, file := range pkg.Syntax` panicked with index-out-of-range on every cgo package
// (measured on linux/amd64 across the whole class: internal/testpty, net, os/user, plugin,
// runtime/cgo — the panic shapes matched the list-length delta exactly).
//
// This host (Windows) selects no cgo files for any of those packages, so the mismatch shape is
// UNREACHABLE through a real load here — the constructed package below IS the reproduction: the
// exact list shape lane R measured, built directly. The Linux lane re-measures the live class
// (plugin/os/user converting without panic) after this lands.

package main

import (
	"go/ast"
	"go/token"
	"path/filepath"
	"testing"

	"golang.org/x/tools/go/packages"
)

// newPairingTestSyntaxFile registers path in fset and returns a minimal *ast.File whose Pos()
// resolves into that token.File — all syntaxSourceFiles reads from a syntax tree. The 100-byte
// size leaves room for the line-directive case to remap an interior offset.
func newPairingTestSyntaxFile(fset *token.FileSet, path string) *ast.File {
	tokenFile := fset.AddFile(path, -1, 100)

	return &ast.File{Package: token.Pos(tokenFile.Base())}
}

// TestSyntaxSourceFilesSkipsCgoIntermediatesWithoutPanic constructs the cgo mismatch shape —
// GoFiles one entry, Syntax/CompiledGoFiles longer by the cgo-generated intermediates — and
// requires the pairing to (a) not index past GoFiles' end, (b) pair the plain source with its own
// syntax tree, and (c) report the intermediates as skipped rather than processing them
// accidentally. Against the unfixed driver logic this test panics:
// index out of range [1] with length 1 — the same shape as R's plugin/os/user/testpty readings.
func TestSyntaxSourceFilesSkipsCgoIntermediatesWithoutPanic(t *testing.T) {
	fset := token.NewFileSet()

	sourceDir := filepath.Join("testpair", "src", "plugin")
	cacheDir := filepath.Join("testpair", "go-build", "b001")

	plainPath := filepath.Join(sourceDir, "plugin.go")
	cgo1Path := filepath.Join(cacheDir, "plugin_dlopen.cgo1.go")
	gotypesPath := filepath.Join(cacheDir, "_cgo_gotypes.go")

	plainFile := newPairingTestSyntaxFile(fset, plainPath)
	cgo1File := newPairingTestSyntaxFile(fset, cgo1Path)
	gotypesFile := newPairingTestSyntaxFile(fset, gotypesPath)

	pkg := &packages.Package{
		PkgPath:         "plugin",
		GoFiles:         []string{plainPath},
		CompiledGoFiles: []string{plainPath, cgo1Path, gotypesPath},
		Syntax:          []*ast.File{plainFile, cgo1File, gotypesFile},
		Fset:            fset,
	}

	paired, skipped := syntaxSourceFiles(pkg)

	if len(paired) != 1 {
		t.Fatalf("expected exactly the one plain Go source paired, got %d entries", len(paired))
	}

	if paired[0].file != plainFile {
		t.Errorf("plain source paired with the wrong syntax tree")
	}

	if paired[0].path != plainPath {
		t.Errorf("plain source paired with path %q, want %q", paired[0].path, plainPath)
	}

	if len(skipped) != 2 {
		t.Fatalf("expected both cgo intermediates skipped, got %d: %v", len(skipped), skipped)
	}

	if skipped[0] != cgo1Path || skipped[1] != gotypesPath {
		t.Errorf("skipped = %v, want [%q %q] in Syntax order", skipped, cgo1Path, gotypesPath)
	}
}

// TestSyntaxSourceFilesPairsPlainPackagesIdentically is the non-cgo control — the unit-level
// statement of the CNR byte-identity expectation: when Syntax, GoFiles and CompiledGoFiles
// coincide (every Windows conversion, every non-cgo package anywhere), each syntax tree pairs
// with exactly the path the historical GoFiles walk gave it, in the same order, nothing skipped.
func TestSyntaxSourceFilesPairsPlainPackagesIdentically(t *testing.T) {
	fset := token.NewFileSet()

	sourceDir := filepath.Join("testpair", "src", "hash", "adler32")
	pathA := filepath.Join(sourceDir, "adler32.go")
	pathB := filepath.Join(sourceDir, "adler32_extra.go")

	fileA := newPairingTestSyntaxFile(fset, pathA)
	fileB := newPairingTestSyntaxFile(fset, pathB)

	pkg := &packages.Package{
		PkgPath:         "hash/adler32",
		GoFiles:         []string{pathA, pathB},
		CompiledGoFiles: []string{pathA, pathB},
		Syntax:          []*ast.File{fileA, fileB},
		Fset:            fset,
	}

	paired, skipped := syntaxSourceFiles(pkg)

	if len(skipped) != 0 {
		t.Fatalf("non-cgo package must skip nothing, skipped %v", skipped)
	}

	if len(paired) != 2 {
		t.Fatalf("expected both plain sources paired, got %d entries", len(paired))
	}

	if paired[0].file != fileA || paired[0].path != pathA {
		t.Errorf("first pair = (%p, %q), want (%p, %q)", paired[0].file, paired[0].path, fileA, pathA)
	}

	if paired[1].file != fileB || paired[1].path != pathB {
		t.Errorf("second pair = (%p, %q), want (%p, %q)", paired[1].file, paired[1].path, fileB, pathB)
	}
}

// TestSyntaxSourceFilesIgnoresLineDirectives pins the pairing to the UNADJUSTED token position —
// the raw path the parser was handed. A plain Go source may legally open with a `//line` directive
// (generated-then-committed code does), and the line-ADJUSTED Position of its package clause then
// names the directive's target, not the file itself; deriving the path adjusted would bounce such
// a file out of the GoFiles membership set and wrongly skip a real source. The same property keeps
// a skipped .cgo1.go intermediate reported under the build-cache path that was actually parsed,
// not the `import "C"` source its directives point back at.
func TestSyntaxSourceFilesIgnoresLineDirectives(t *testing.T) {
	fset := token.NewFileSet()

	sourceDir := filepath.Join("testpair", "src", "directive")
	sourcePath := filepath.Join(sourceDir, "generated.go")

	tokenFile := fset.AddFile(sourcePath, -1, 100)
	// Remap everything from offset 0 as a `//line elsewhere.go:1` directive would, then place the
	// package clause AFTER the remap point so the adjusted Position reports the directive's target.
	tokenFile.AddLineColumnInfo(0, filepath.Join("somewhere", "else.go"), 1, 1)
	syntaxFile := &ast.File{Package: token.Pos(tokenFile.Base() + 50)}

	pkg := &packages.Package{
		PkgPath:         "directive",
		GoFiles:         []string{sourcePath},
		CompiledGoFiles: []string{sourcePath},
		Syntax:          []*ast.File{syntaxFile},
		Fset:            fset,
	}

	paired, skipped := syntaxSourceFiles(pkg)

	if len(skipped) != 0 {
		t.Fatalf("line-directive-bearing plain source must not be skipped, skipped %v", skipped)
	}

	if len(paired) != 1 || paired[0].path != sourcePath {
		t.Fatalf("expected the directive-bearing source paired under its own path %q, got %v", sourcePath, paired)
	}
}
