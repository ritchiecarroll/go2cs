// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Tests is a helper package to avoid cyclic dependency between go/ast and go/parser.
namespace go.go.ast.@internal;

using ast = global::go.go.ast_package;
using parser = global::go.go.parser_package;
using token = global::go.go.token_package;
using testing = testing_package;
using global::go.go;

partial class tests_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string oneImportStatementˢ = "one import statement"u8;
internal static readonly @string testGoˢ = "test.go"u8;
internal static readonly @string multipleImportStatementsˢ = "multiple import statements"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string srcᶜ = """
package test

import (
	"test"
	"test" // test comment
)

"""u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string srcᶜ1 = """
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

"""u8;

public static void TestSortImportsUpdatesFileImportsField(ж<testing.T> Ꮡt) {
    Ꮡt.Run(oneImportStatementˢ, (ж<testing.T> tΔ1) => {
        @string src = srcᶜ;
        var fset = token.NewFileSet();
        var (f, err) = parser.ParseFile(fset, testGoˢ, src, (parser.Mode)(parser.ParseComments | parser.SkipObjectResolution));
        if (err != default!) {
            tΔ1.Fatal(err);
        }
        ast.SortImports(fset, f);
        // Check that the duplicate import spec is eliminated.
        nint importDeclSpecCount = len((~(~f).Decls[0]._<ж<ast.GenDecl>>()).Specs);
        if (importDeclSpecCount != 1) {
            tΔ1.Fatalf("len(f.Decls[0].(*ast.GenDecl).Specs) = %v; want = 1"u8, importDeclSpecCount);
        }
        // Check that File.Imports is consistent.
        if (len((~f).Imports) != 1) {
            tΔ1.Fatalf("len(f.Imports) = %v; want = 1"u8, len((~f).Imports));
        }
    });
    Ꮡt.Run(multipleImportStatementsˢ, (ж<testing.T> tΔ2) => {
        @string src = srcᶜ1;
        var fset = token.NewFileSet();
        var (f, err) = parser.ParseFile(fset, testGoˢ, src, (parser.Mode)(parser.ParseComments | parser.SkipObjectResolution));
        if (err != default!) {
            tΔ2.Fatal(err);
        }
        ast.SortImports(fset, f);
        // Check that three single-spec import decls remain.
        foreach (var i in range(3)) {
            nint importDeclSpecCount = len((~(~f).Decls[i]._<ж<ast.GenDecl>>()).Specs);
            if (importDeclSpecCount != 1) {
                tΔ2.Fatalf("len(f.Decls[%v].(*ast.GenDecl).Specs) = %v; want = 1"u8, i, importDeclSpecCount);
            }
        }
        // Check that File.Imports is consistent.
        if (len((~f).Imports) != 3) {
            tΔ2.Fatalf("len(f.Imports) = %v; want = 3"u8, len((~f).Imports));
        }
    });
}

} // end tests_internal_test_package
