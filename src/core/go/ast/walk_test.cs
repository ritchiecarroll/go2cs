// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using ast = global::go.go.ast_package;
using parser = global::go.go.parser_package;
using token = global::go.go.token_package;
using reflect = reflect_package;
using slices = slices_package;
using strings = strings_package;
using testing = testing_package;
using global::go.go;
using iter = iter_package;
using static global::go.go.ast_internal_test_package;

partial class ast_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string packagePTypeTStructFIntˢ = "package p\ntype T struct {\n\tF int `json:\"f\"` // a field\n}\n"u8;

public static void TestPreorder_Break(ж<testing.T> Ꮡt) {
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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string aGoˢ = "a.go"u8;
internal static readonly @string astˢ = "*ast."u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string srcᶜ4 = """
package a
func f() {
	print("hello")
}
func g() {
	print("goodbye")
	panic("oops")
}

"""u8.Raw();

public static void TestPreorderStack(ж<testing.T> Ꮡt) {
    @string src = srcᶜ4;
    var fset = token.NewFileSet();
    var (f, _) = parser.ParseFile(fset, aGoˢ, src, 0);

    @string str(ast.Node n) => strings.TrimPrefix(reflect.TypeOf(n).String(), astˢ);

    ref var events = ref heap<slice<@string>>(out var Ꮡevents);
    ref var gotStack = ref heap<slice<@string>>(out var ᏑgotStack);
    var strʗ1 = str;
    ast.PreorderStack(new ast.FileжNode(f), default!, (ast.Node n, slice<ast.Node> stack) => {
        Ꮡevents.ValueSlot = append(Ꮡevents.ValueSlot, strʗ1(n));
        {
            var (decl, ok) = n._<ж<ast.FuncDecl>>(ᐧ); if (ok && (~(~decl).Name).Name == "f"u8) {
                return false; // skip subtree of f()
            }
        }
        {
            var (lit, ok) = n._<ж<ast.BasicLit>>(ᐧ); if (ok && (~lit).Value == @"""oops"""u8) {
                foreach (var (_, nΔ1) in stack) {
                    ᏑgotStack.ValueSlot = append(ᏑgotStack.ValueSlot, strʗ1(nΔ1));
                }
            }
        }
        return true;
    });

    // Check sequence of events.
    var wantEvents = new @string[]{
        "File"u8, "Ident"u8, // package a
        "FuncDecl"u8, // func f()  [pruned]
        "FuncDecl"u8, "Ident"u8, "FuncType"u8, "FieldList"u8, "BlockStmt"u8, // func g()
        "ExprStmt"u8, "CallExpr"u8, "Ident"u8, "BasicLit"u8, // print...
        "ExprStmt"u8, "CallExpr"u8, "Ident"u8, "BasicLit"u8 // panic...
    }.slice();
    if (!slices.Equal<slice<@string>, @string>(events, wantEvents)) {
        Ꮡt.Errorf("PreorderStack events:\ngot:  %s\nwant: %s"u8, events, wantEvents);
    }

    // Check captured stack.
    var wantStack = new @string[]{"File"u8, "FuncDecl"u8, "BlockStmt"u8, "ExprStmt"u8, "CallExpr"u8}.slice();
    if (!slices.Equal<slice<@string>, @string>(gotStack, wantStack)) {
        Ꮡt.Errorf("PreorderStack stack:\ngot:  %s\nwant: %s"u8, gotStack, wantStack);
    }
}

} // end ast_test_package
