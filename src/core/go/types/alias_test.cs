// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using ast = global::go.go.ast_package;
using parser = global::go.go.parser_package;
using token = global::go.go.token_package;
using types = global::go.go.types_package;
using testing = testing_package;
using global::go.go;
using static global::go.go.types_internal_test_package;

partial class types_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string godebugˢ = "GODEBUG"u8;
internal static readonly @string gotypesalias1ˢ = "gotypesalias=1"u8;
internal static readonly @string packagePTypeAbABTypeˢ = """
package p

type AB = A[B]

type _ struct {
	_ AB
}

type B struct {
	f *AB
}

type A[T any] struct{}
"""u8.Raw();
internal static readonly @string pGoˢ = "p.go"u8;

public static void TestIssue74181(ж<testing.T> Ꮡt) {
    Ꮡt.Setenv(godebugˢ, gotypesalias1ˢ);

    @string src = packagePTypeAbABTypeˢ;

    var fset = token.NewFileSet();
    var (@file, err) = parser.ParseFile(fset, pGoˢ, src, parser.ParseComments);
    if (err != default!) {
        Ꮡt.Fatalf("could not parse: %v"u8, err);
    }

    ref var conf = ref heap<types.Config>(out var Ꮡconf);
    conf = new types.Config(nil);
    (var pkg, err) = Ꮡconf.Check((~(~@file).Name).Name, fset, new ж<ast.File>[]{@file}.slice(), Ꮡ(new typesꓸInfo(nil)));
    if (err != default!) {
        Ꮡt.Fatalf("could not type check: %v"u8, err);
    }

    var b = pkg.Scope().Lookup("B"u8).Type();
    {
        var (n, ok) = b._<ж<types.Named>>(ᐧ); if (ok) {
            {
                var (s, okΔ1) = n.Underlying()._<ж<types.Struct>>(ᐧ); if (okΔ1) {
                    var got = s.Field(0).Type();
                    var want = types.NewPointer(pkg.Scope().Lookup("AB"u8).Type());
                    if (!types.Identical(got, new types.PointerжΔType(want))) {
                        Ꮡt.Errorf("wrong type for f: got %v, want %v"u8, got, want.OrTypedNil());
                    }
                    return;
                }
            }
        }
    }
    Ꮡt.Errorf("unexpected type for B: %v"u8, b);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string packagePTypeABUndeclaredˢ = """
package p

type A = B // undeclared
"""u8.Raw();

public static void TestPartialTypeCheckUndeclaredAliasPanic(ж<testing.T> Ꮡt) {
    Ꮡt.Setenv(godebugˢ, gotypesalias1ˢ);

    @string src = packagePTypeABUndeclaredˢ;

    var fset = token.NewFileSet();
    var (@file, err) = parser.ParseFile(fset, pGoˢ, src, parser.ParseComments);
    if (err != default!) {
        Ꮡt.Fatalf("could not parse: %v"u8, err);
    }

    ref var conf = ref heap<types.Config>(out var Ꮡconf);
    conf = new types.Config(nil); // no error handler, panic
    var (pkg, _) = Ꮡconf.Check((~(~@file).Name).Name, fset, new ж<ast.File>[]{@file}.slice(), Ꮡ(new typesꓸInfo(nil)));
    var a = pkg.Scope().Lookup("A"u8).Type();

    {
        var (alias, ok) = a._<ж<types.Alias>>(ᐧ); if (ok) {
            var got = alias.Rhs();
            var want = types.Typ[types.Invalid];

            if (!types.Identical(got, new types.BasicжΔType(want))) {
                Ꮡt.Errorf("wrong type for B: got %v, want %v"u8, got, want.OrTypedNil());
            }
            return;
        }
    }
    Ꮡt.Errorf("unexpected type for A: %v"u8, a);
}

} // end types_test_package
