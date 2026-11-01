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
internal static ref readonly @string packagePTypeTStructFIntˢ => ref packagePTypeTStructFIntˢᵇ;
private static readonly @string packagePTypeTStructFIntˢᵇ = "package p\ntype T struct {\n\tF int `json:\"f\"` // a field\n}\n"u8;

public static void TestPreorder_Break(ж<testing.T> t) {
    // This test checks that Preorder correctly handles a break statement while
    // in the middle of walking a node. Previously, incorrect handling of the
    // boolean returned by the yield function resulted in the iterator calling
    // yield for sibling nodes even after yield had returned false. With that
    // bug, this test failed with a runtime panic.
    @string src = packagePTypeTStructFIntˢ;

    var fset = token.NewFileSet();
    var (f, err) = parser.ParseFile(fset, ""u8, src, 0);
    if (err != nil) {
        throw panic(err);
    }

    foreach (var n in range<ast.Node>(ast.Preorder(new ast.FileжNode(f)).Invoke)) {
        {
            var (id, ok) = n._<ж<ast.Ident>>(ᐧ); if (ok && id.Name == "F"u8) {
                break;
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string aGoˢ => ref aGoˢᵇ;
private static readonly @string aGoˢᵇ = "a.go"u8;
internal static ref readonly @string astˢ => ref astˢᵇ;
private static readonly @string astˢᵇ = "*ast."u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string srcᶜ4 => ref srcᶜ4ᵇ;
private static readonly @string srcᶜ4ᵇ = """
package a
func f() {
	print("hello")
}
func g() {
	print("goodbye")
	panic("oops")
}

"""u8.Raw();

public static void TestPreorderStack(ж<testing.T> t) {
    @string src = srcᶜ4;
    var fset = token.NewFileSet();
    var (f, _) = parser.ParseFile(fset, aGoˢ, src, 0);

    @string str(ast.Node n) => strings.TrimPrefix(reflect.TypeOf(n).String(), astˢ);

    ref var events = ref heap<slice<@string>>(out var Ꮡevents);
    ref var gotStack = ref heap<slice<@string>>(out var ᏑgotStack);
    var strʗ1 = str;
    ast.PreorderStack(new ast.FileжNode(f), default, (ast.Node n, slice<ast.Node> stack) => {
        Ꮡevents.ValueSlot = append(Ꮡevents.ValueSlot, strʗ1(n));
        {
            var (decl, ok) = n._<ж<ast.FuncDecl>>(ᐧ); if (ok && decl.Name.Name == "f"u8) {
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
    var wantEvents = slice<@string>([
        "File", "Ident", // package a
        "FuncDecl",                                                // func f()  [pruned]
        "FuncDecl", "Ident", "FuncType", "FieldList", "BlockStmt", // func g()
        "ExprStmt", "CallExpr", "Ident", "BasicLit", // print...
        "ExprStmt", "CallExpr", "Ident", "BasicLit"  // panic...
    ]);
    if (!slices.Equal<slice<@string>, @string>(events, wantEvents)) {
        t.Errorf("PreorderStack events:\ngot:  %s\nwant: %s", events, wantEvents);
    }

    // Check captured stack.
    var wantStack = slice<@string>(["File", "FuncDecl", "BlockStmt", "ExprStmt", "CallExpr"]);
    if (!slices.Equal<slice<@string>, @string>(gotStack, wantStack)) {
        t.Errorf("PreorderStack stack:\ngot:  %s\nwant: %s", gotStack, wantStack);
    }
}

} // end ast_test_package
