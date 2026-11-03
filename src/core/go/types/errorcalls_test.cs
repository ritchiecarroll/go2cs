// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using ast = global::go.go.ast_package;
using token = global::go.go.token_package;
using strconv = strconv_package;
using testing = testing_package;
using global::go.go;
using static global::go.go.types_internal_test_package;

partial class types_test_package {

internal static UntypedInt errorfMinArgCount => 4;
internal static UntypedInt errorfFormatIndex => 2;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string checkˢ => ref checkˢᵇ;
private static readonly @string checkˢᵇ = "check"u8;
internal static ref readonly @string errorfˢ => ref errorfˢᵇ;
private static readonly @string errorfˢᵇ = "errorf"u8;

// TestErrorCalls makes sure that check.errorf calls have at least
// errorfMinArgCount arguments (otherwise we should use check.error)
// and use balanced parentheses/brackets.
public static void TestErrorCalls(ж<testing.T> t) {
    var fset = token.NewFileSet();
    var (files, err) = pkgFiles(fset, ".");
    if (err != nil) {
        t.Fatal(err);
    }

    foreach (var (_, @file) in files) {
        var fsetʗ1 = fset;
        ast.Inspect(new ast.FileжNode(@file), (ast.Node n) => {
            var (call, _) = n._<ж<ast.CallExpr>>(ᐧ);
            if (call == nil) {
                return true;
            }
            var (selx, _) = call.Fun._<ж<ast.SelectorExpr>>(ᐧ);
            if (selx == nil) {
                return true;
            }
            if (!(isName(selx.X, checkˢ) && isName(new ast.IdentжNode(selx.Sel), errorfˢ))) {
                return true;
            }
            // check.errorf calls should have at least errorfMinArgCount arguments:
            // position, code, format string, and arguments to format
            {
                nint nΔ1 = len(call.Args); if (nΔ1 < errorfMinArgCount) {
                    t.Errorf("%s: got %d arguments, want at least %d", fsetʗ1.Position(call.Pos()), nΔ1, (nint)(errorfMinArgCount));
                    return false;
                }
            }
            var format = call.Args[errorfFormatIndex];
            var fsetʗ2 = fsetʗ1;
            ast.Inspect(format, (ast.Node nΔ2) => {
                {
                    var (lit, _) = nΔ2._<ж<ast.BasicLit>>(ᐧ); if (lit != nil && lit.Kind == token.STRING) {
                        {
                            var (s, errΔ1) = strconv.Unquote((~lit).Value); if (errΔ1 == nil) {
                                if (!balancedParentheses(s)) {
                                    t.Errorf("%s: unbalanced parentheses/brackets", fsetʗ2.Position(lit.ValuePos));
                                }
                            }
                        }
                        return false;
                    }
                }
                return true;
            });
            return false;
        });
    }
}

internal static bool isName(ast.Node n, @string name) {
    {
        var (nΔ1, ok) = n._<ж<ast.Ident>>(ᐧ); if (ok) {
            return nΔ1.Name == name;
        }
    }
    return false;
}

internal static bool balancedParentheses(@string s) {
    var stackᴷ = new stackAppend<byte>();

    slice<byte> stack = default;
    foreach (var (_, ch) in s) {
        byte open = default;
        switch (ch) {
        case '(' or '[' or '{': {
            stack = append(stackᴷ, stack, (byte)ch);
            continue;
            break;
        }
        case ')': {
            open = (byte)'(';
            break;
        }
        case ']': {
            open = (byte)'[';
            break;
        }
        case '}': {
            open = (byte)'{';
            break;
        }
        default: {
            continue;
            break;
        }}
        // closing parenthesis/bracket must have matching opening
        nint top = len(stack) - 1;
        if (top < 0 || stack[top] != open) {
            return false;
        }
        stack = stack.slice(0, top);
    }
    return len(stack) == 0;
}

} // end types_test_package
