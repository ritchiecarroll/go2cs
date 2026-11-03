// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using ast = global::go.go.ast_package;
using parser = global::go.go.parser_package;
using token = global::go.go.token_package;
using testing = testing_package;
using global::go.go;
using static global::go.go.ast_internal_test_package;

partial class ast_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string oneImportStatementˢ => ref oneImportStatementˢᵇ;
private static readonly @string oneImportStatementˢᵇ = "one import statement"u8;
internal static ref readonly @string testGoˢ => ref testGoˢᵇ;
private static readonly @string testGoˢᵇ = "test.go"u8;
internal static ref readonly @string multipleImportStatementsˢ => ref multipleImportStatementsˢᵇ;
private static readonly @string multipleImportStatementsˢᵇ = "multiple import statements"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string srcᶜ => ref srcᶜᵇ;
private static readonly @string srcᶜᵇ = """
package test

import (
	"test"
	"test" // test comment
)

"""u8.Raw();

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string srcᶜ1 => ref srcᶜ1ᵇ;
private static readonly @string srcᶜ1ᵇ = """
package test

import "unsafe"

import (
	"package"
	"package"
)

import (
	"test"
	"test"
)

"""u8.Raw();

public static void TestSortImportsUpdatesFileImportsField(ж<testing.T> t) {
    t.Run(oneImportStatementˢ, (ж<testing.T> tΔ1) => {
        @string src = srcᶜ;

        var fset = token.NewFileSet();
        var (f, err) = parser.ParseFile(fset, testGoˢ, src, (parser.Mode)(parser.ParseComments | parser.SkipObjectResolution));
        if (err != nil) {
            tΔ1.Fatal(err);
        }

        ast.SortImports(fset, f);

        // Check that the duplicate import spec is eliminated.
        nint importDeclSpecCount = len(f.Decls[0]._<ж<ast.GenDecl>>().Specs);
        if (importDeclSpecCount != 1) {
            tΔ1.Fatalf("len(f.Decls[0].(*ast.GenDecl).Specs) = %v; want = 1", importDeclSpecCount);
        }

        // Check that File.Imports is consistent.
        if (len(f.Imports) != 1) {
            tΔ1.Fatalf("len(f.Imports) = %v; want = 1", len(f.Imports));
        }
    });

    t.Run(multipleImportStatementsˢ, (ж<testing.T> tΔ2) => {
        @string src = srcᶜ1;

        var fset = token.NewFileSet();
        var (f, err) = parser.ParseFile(fset, testGoˢ, src, (parser.Mode)(parser.ParseComments | parser.SkipObjectResolution));
        if (err != nil) {
            tΔ2.Fatal(err);
        }

        ast.SortImports(fset, f);

        // Check that three single-spec import decls remain.
        foreach (var i in range(3)) {
            nint importDeclSpecCount = len(f.Decls[i]._<ж<ast.GenDecl>>().Specs);
            if (importDeclSpecCount != 1) {
                tΔ2.Fatalf("len(f.Decls[%v].(*ast.GenDecl).Specs) = %v; want = 1", i, importDeclSpecCount);
            }
        }

        // Check that File.Imports is consistent.
        if (len(f.Imports) != 3) {
            tΔ2.Fatalf("len(f.Imports) = %v; want = 3", len(f.Imports));
        }
    });
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string srcᶜ2 => ref srcᶜ2ᵇ;
private static readonly @string srcᶜ2ᵇ = """
package A
import (
"a"//a
"a")

"""u8.Raw();

public static void TestIssue69183(ж<testing.T> t) {
    @string src = srcᶜ2;
    var fset = token.NewFileSet();
    var (f, err) = parser.ParseFile(fset, testGoˢ, src, (parser.Mode)(parser.ParseComments | parser.SkipObjectResolution));
    if (err != nil) {
        t.Fatal(err);
    }
    ast.SortImports(fset, f); // should not panic
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string srcᶜ3 => ref srcᶜ3ᵇ;
private static readonly @string srcᶜ3ᵇ = """
package A
import (
"a"//a
"a")
func a() {}

"""u8.Raw();

public static void TestSortImportsSameLastLine(ж<testing.T> t) {
    @string src = srcᶜ3;

    var fset = token.NewFileSet();
    var (f, err) = parser.ParseFile(fset, testGoˢ, src, (parser.Mode)(parser.ParseComments | parser.SkipObjectResolution));
    if (err != nil) {
        t.Fatal(err);
    }
    ast.SortImports(fset, f);
    var fd = f.Decls[1]._<ж<ast.FuncDecl>>();
    var fdPos = fset.Position(fd.Pos());
    // After SortImports, the Position of the func, should still be at Column == 1.
    // This is related to the issue: https://go.dev/issue/69183, we were merging lines
    // incorrectly, which caused the position to be Column = 6, Line = 4.
    if (fdPos.Column != 1) {
        t.Errorf("invalid fdPos.Column = %v; want = 1", fdPos.Column);
    }
    if (fdPos.Line != 5) {
        t.Errorf("invalid fdPos.Line = %v; want = 5", fdPos.Line);
    }
}

} // end ast_test_package
