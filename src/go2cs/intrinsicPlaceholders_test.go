// intrinsicPlaceholders_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/ast"
	"go/build"
	"go/parser"
	"go/token"
	"io/fs"
	"os"
	"path/filepath"
	"runtime"
	"sort"
	"strconv"
	"strings"
	"testing"
)

// A Go COMPILER INTRINSIC is a function the compiler replaces, whose written body is a lone panic the
// author never expects to run. Converted faithfully that panic runs. compilerIntrinsic.go answers Go's
// own convention, `panic("intrinsic")`, whose meaning is a no-op or a zero value (abi.EscapeNonString,
// maphash's escapeForHash). Go 1.26 added one in another text and with a MEANING --
// crypto/internal/constanttime's boolToUint8, `panic("unreachable; must be intrinsicified")`, 1 for
// true and 0 for false -- and it reached the corpus silently: every converted GCM Open panicked in
// Release (H7).
//
// So every such placeholder in the pinned GOROOT must be CLASSIFIED: either it is the rule's (its text is
// exactly the marker, and a no-op/zero is its meaning) or it names a hand-own in manualConversionFuncs.
// An unclassified placeholder fails, naming the function, so the next Go release cannot add one quietly.
//
// THE POPULATION is every non-test source under GOROOT/src outside cmd/ and testdata: a FuncDecl whose body
// is exactly one statement, a call to `panic` with one STRING-literal argument whose text mentions
// "intrinsic". Today that is two bodies. POSITIVE CONTROL: TestIntrinsicPlaceholderScannerFires.

type intrinsicPlaceholder struct {
	pkgPath string
	name    string
	text    string
	where   string
}

// intrinsicPlaceholdersIn returns the placeholders declared in one parsed file of package pkgPath.
func intrinsicPlaceholdersIn(fset *token.FileSet, file *ast.File, pkgPath string) []intrinsicPlaceholder {
	var found []intrinsicPlaceholder

	for _, decl := range file.Decls {
		funcDecl, ok := decl.(*ast.FuncDecl)

		if !ok || funcDecl.Body == nil || len(funcDecl.Body.List) != 1 {
			continue
		}

		exprStmt, ok := funcDecl.Body.List[0].(*ast.ExprStmt)

		if !ok {
			continue
		}

		call, ok := exprStmt.X.(*ast.CallExpr)

		if !ok || len(call.Args) != 1 {
			continue
		}

		if ident, ok := call.Fun.(*ast.Ident); !ok || ident.Name != "panic" {
			continue
		}

		literal, ok := call.Args[0].(*ast.BasicLit)

		if !ok || literal.Kind != token.STRING {
			continue
		}

		text, err := strconv.Unquote(literal.Value)

		if err != nil || !strings.Contains(text, compilerIntrinsicMarker) {
			continue
		}

		name := funcDecl.Name.Name

		if funcDecl.Recv != nil && len(funcDecl.Recv.List) == 1 {
			receiver := funcDecl.Recv.List[0].Type

			if star, ok := receiver.(*ast.StarExpr); ok {
				receiver = star.X
			}

			if index, ok := receiver.(*ast.IndexExpr); ok {
				receiver = index.X
			}

			if ident, ok := receiver.(*ast.Ident); ok {
				name = ident.Name + "." + name
			}
		}

		found = append(found, intrinsicPlaceholder{pkgPath, name, text, fset.Position(funcDecl.Pos()).String()})
	}

	return found
}

// unclassifiedIntrinsicPlaceholders returns the placeholders neither the intrinsic rule nor a registered
// hand-own accounts for.
func unclassifiedIntrinsicPlaceholders(placeholders []intrinsicPlaceholder, handOwns map[string]map[string]goosScope) []intrinsicPlaceholder {
	var unclassified []intrinsicPlaceholder

	for _, placeholder := range placeholders {
		if placeholder.text == compilerIntrinsicMarker {
			continue
		}

		if _, owned := handOwns[placeholder.pkgPath][placeholder.name]; owned {
			continue
		}

		unclassified = append(unclassified, placeholder)
	}

	return unclassified
}

func TestIntrinsicPlaceholdersAreClassified(t *testing.T) {
	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	srcRoot := filepath.Join(goRoot, "src")
	fset := token.NewFileSet()
	var placeholders []intrinsicPlaceholder
	scanned := 0

	err := filepath.WalkDir(srcRoot, func(path string, entry fs.DirEntry, err error) error {
		if err != nil {
			return err
		}

		if entry.IsDir() {
			if name := entry.Name(); name == "testdata" || path == filepath.Join(srcRoot, "cmd") {
				return filepath.SkipDir
			}

			return nil
		}

		if !strings.HasSuffix(path, ".go") || strings.HasSuffix(path, "_test.go") {
			return nil
		}

		scanned++
		data, err := os.ReadFile(path)

		if err != nil {
			return err
		}

		if !strings.Contains(string(data), compilerIntrinsicMarker) {
			return nil
		}

		file, err := parser.ParseFile(fset, path, data, parser.SkipObjectResolution)

		if err != nil {
			return nil // a file the pinned toolchain itself would not build here (a build-tag dialect)
		}

		rel, _ := filepath.Rel(srcRoot, filepath.Dir(path))
		placeholders = append(placeholders, intrinsicPlaceholdersIn(fset, file, filepath.ToSlash(rel))...)

		return nil
	})

	if err != nil {
		t.Fatalf("walking %s: %v", srcRoot, err)
	}

	if scanned < 2000 {
		t.Fatalf("read %d non-test sources under %s: too few to be a Go source tree", scanned, srcRoot)
	}

	sort.Slice(placeholders, func(i, j int) bool { return placeholders[i].where < placeholders[j].where })

	for _, placeholder := range unclassifiedIntrinsicPlaceholders(placeholders, manualConversionFuncs) {
		t.Errorf("%s: %s.%s is a compiler-intrinsic placeholder, `panic(%q)`, that is neither the intrinsic rule's "+
			"(text exactly %q, meaning a no-op or zero) nor a registered hand-own in manualConversionFuncs[%q]: "+
			"converted, its panic runs -- classify it", placeholder.where, placeholder.pkgPath, placeholder.name,
			placeholder.text, compilerIntrinsicMarker, placeholder.pkgPath)
	}

	if len(placeholders) == 0 {
		t.Fatalf("found no intrinsic placeholder in %d sources: the scanner no longer sees abi.EscapeNonString", scanned)
	}

	for _, placeholder := range placeholders {
		t.Logf("%s.%s  %q  (%s)", placeholder.pkgPath, placeholder.name, placeholder.text, placeholder.where)
	}
}

func TestIntrinsicPlaceholderScannerFires(t *testing.T) {
	source := `package probe

func ruled() { panic("intrinsic") }

func meaning(b bool) uint8 { panic("unreachable; must be intrinsicified") }

func owned(b bool) uint8 { panic("must be intrinsicified") }

func twoStatements() { println(); panic("intrinsic") }

func otherPanic() { panic("not one") }
`

	fset := token.NewFileSet()
	file, err := parser.ParseFile(fset, "probe.go", source, parser.SkipObjectResolution)

	if err != nil {
		t.Fatal(err)
	}

	placeholders := intrinsicPlaceholdersIn(fset, file, "probe")

	if len(placeholders) != 3 {
		t.Fatalf("want the three lone intrinsic panics (ruled, meaning, owned), got %v", placeholders)
	}

	unclassified := unclassifiedIntrinsicPlaceholders(placeholders, map[string]map[string]goosScope{"probe": {"owned": goosAny}})

	if len(unclassified) != 1 || unclassified[0].name != "meaning" {
		t.Fatalf("want exactly `meaning` unclassified (ruled is the rule's, owned is registered), got %v", unclassified)
	}
}
