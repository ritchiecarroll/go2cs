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
internal static ref readonly @string godebugˢ => ref godebugˢᵇ;
private static readonly @string godebugˢᵇ = "GODEBUG"u8;
internal static ref readonly @string gotypesalias1ˢ => ref gotypesalias1ˢᵇ;
private static readonly @string gotypesalias1ˢᵇ = "gotypesalias=1"u8;
internal static ref readonly @string packagePTypeAbABTypeˢ => ref packagePTypeAbABTypeˢᵇ;
private static readonly @string packagePTypeAbABTypeˢᵇ = """
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
internal static ref readonly @string pGoˢ => ref pGoˢᵇ;
private static readonly @string pGoˢᵇ = "p.go"u8;

public static void TestIssue74181(ж<testing.T> t) {
    t.Setenv(godebugˢ, gotypesalias1ˢ);

    @string src = packagePTypeAbABTypeˢ;

    var fset = token.NewFileSet();
    var (@file, err) = parser.ParseFile(fset, pGoˢ, src, parser.ParseComments);
    if (err != nil) {
        t.Fatalf("could not parse: %v", err);
    }

    ref var conf = ref heap<types.Config>(new types.Config(), out var Ꮡconf);
    (var pkg, err) = Ꮡconf.Check(@file.Name.Name, fset, [@file], Ꮡ(new typesꓸInfo()));
    if (err != nil) {
        t.Fatalf("could not type check: %v", err);
    }

    var b = pkg.Scope().Lookup("B").Type();
    var (n, ok) = b._<ж<types.Named>>(ᐧ); if (ok) {
        var (s, okΔ1) = n.Underlying()._<ж<types.Struct>>(ᐧ); if (okΔ1) {
            var got = s.Field(0).Type();
            var want = types.NewPointer(pkg.Scope().Lookup("AB").Type());
            if (!types.Identical(got, new types.PointerжΔType(want))) {
                t.Errorf("wrong type for f: got %v, want %v", got, want.OrTypedNil());
            }
            return;
        }
    }
    t.Errorf("unexpected type for B: %v", b);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string packagePTypeABUndeclaredˢ => ref packagePTypeABUndeclaredˢᵇ;
private static readonly @string packagePTypeABUndeclaredˢᵇ = """
package p

type A = B // undeclared
"""u8.Raw();

public static void TestPartialTypeCheckUndeclaredAliasPanic(ж<testing.T> t) {
    t.Setenv(godebugˢ, gotypesalias1ˢ);

    @string src = packagePTypeABUndeclaredˢ;

    var fset = token.NewFileSet();
    var (@file, err) = parser.ParseFile(fset, pGoˢ, src, parser.ParseComments);
    if (err != nil) {
        t.Fatalf("could not parse: %v", err);
    }

    ref var conf = ref heap<types.Config>(new types.Config(), out var Ꮡconf); // no error handler, panic
    var (pkg, _) = Ꮡconf.Check(@file.Name.Name, fset, [@file], Ꮡ(new typesꓸInfo()));
    var a = pkg.Scope().Lookup("A").Type();

    var (alias, ok) = a._<ж<types.Alias>>(ᐧ); if (ok) {
        var got = alias.Rhs();
        var want = types.Typ[types.Invalid];

        if (!types.Identical(got, new types.BasicжΔType(want))) {
            t.Errorf("wrong type for B: got %v, want %v", got, want.OrTypedNil());
        }
        return;
    }
    t.Errorf("unexpected type for A: %v", a);
}

} // end types_test_package
