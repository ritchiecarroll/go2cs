// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using ast = global::go.go.ast_package;
using parser = global::go.go.parser_package;
using token = global::go.go.token_package;
using testing = testing_package;
using global::go.go;
using iter = iter_package;
using static global::go.go.ast_internal_test_package;

partial class ast_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string packagePTypeTStructFIntˢ = "package p\ntype T struct {\n\tF int `json:\"f\"` // a field\n}\n"u8;

public static void TestPreorderBreak(ж<testing.T> Ꮡt) {
    // This test checks that Preorder correctly handles a break statement while
    // in the middle of walking a node. Previously, incorrect handling of the
    // boolean returned by the yield function resulted in the iterator calling
    // yield for sibling nodes even after yield had returned false. With that
    // bug, this test failed with a runtime panic.
    @string src = packagePTypeTStructFIntˢ;

    var fset = token.NewFileSet();
    var (f, err) = parser.ParseFile(fset, ""u8, src, 0);
    if (err != default!) {
        throw panic(err);
    }

    foreach (var n in range<ast.Node>(ast.Preorder(new ast.FileжNode(f)).Invoke)) {
        {
            var (id, ok) = n._<ж<ast.Ident>>(ᐧ); if (ok && (~id).Name == "F"u8) {
                break;
            }
        }
    }
}

} // end ast_test_package
