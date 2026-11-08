// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using slices = slices_package;
using strings = strings_package;
using testing = testing_package;
using ast = global::go.go.ast_package;
using parser = global::go.go.parser_package;
using token = global::go.go.token_package;
using static global::go.go.types_package;
using global::go.go;
using static global::go.go.types_internal_test_package;
using types = global::go.go.types_package;

partial class types_test_package {

internal partial struct TestNewMethodSet_method /*dyn*/ {
    internal @string name;
    internal slice<nint> index;
    internal bool indirect;
}

public static void TestNewMethodSet(ж<testing.T> t) {
    // Tests are expressed src -> methods, for simplifying the composite literal.
    // Should be kept in sync with TestLookupFieldOrMethod.
    var tests = new map<@string, slice<TestNewMethodSet_method>>{
        // Named types
        ["var a T; type T struct{}; func (T) f() {}"] = [new("f", [0], false)],
        ["var a *T; type T struct{}; func (T) f() {}"] = [new("f", [0], true)],
        ["var a T; type T struct{}; func (*T) f() {}"] = [],
        ["var a *T; type T struct{}; func (*T) f() {}"] = [new("f", [0], true)],

        // Generic named types
        ["var a T[int]; type T[P any] struct{}; func (T[P]) f() {}"] = [new("f", [0], false)],
        ["var a *T[int]; type T[P any] struct{}; func (T[P]) f() {}"] = [new("f", [0], true)],
        ["var a T[int]; type T[P any] struct{}; func (*T[P]) f() {}"] = [],
        ["var a *T[int]; type T[P any] struct{}; func (*T[P]) f() {}"] = [new("f", [0], true)],

        // Interfaces
        ["var a T; type T interface{ f() }"] = [new("f", [0], true)],
        ["var a T1; type ( T1 T2; T2 interface{ f() } )"] = [new("f", [0], true)],
        ["var a T1; type ( T1 interface{ T2 }; T2 interface{ f() } )"] = [new("f", [0], true)],

        // Generic interfaces
        ["var a T[int]; type T[P any] interface{ f() }"] = [new("f", [0], true)],
        ["var a T1[int]; type ( T1[P any] T2[P]; T2[P any] interface{ f() } )"] = [new("f", [0], true)],
        ["var a T1[int]; type ( T1[P any] interface{ T2[P] }; T2[P any] interface{ f() } )"] = [new("f", [0], true)],

        // Embedding
        ["var a struct{ E }; type E interface{ f() }"] = [new("f", [0, 0], true)],
        ["var a *struct{ E }; type E interface{ f() }"] = [new("f", [0, 0], true)],
        ["var a struct{ E }; type E struct{}; func (E) f() {}"] = [new("f", [0, 0], false)],
        ["var a struct{ *E }; type E struct{}; func (E) f() {}"] = [new("f", [0, 0], true)],
        ["var a struct{ E }; type E struct{}; func (*E) f() {}"] = [],
        ["var a struct{ *E }; type E struct{}; func (*E) f() {}"] = [new("f", [0, 0], true)],

        // Embedding of generic types
        ["var a struct{ E[int] }; type E[P any] interface{ f() }"] = [new("f", [0, 0], true)],
        ["var a *struct{ E[int] }; type E[P any] interface{ f() }"] = [new("f", [0, 0], true)],
        ["var a struct{ E[int] }; type E[P any] struct{}; func (E[P]) f() {}"] = [new("f", [0, 0], false)],
        ["var a struct{ *E[int] }; type E[P any] struct{}; func (E[P]) f() {}"] = [new("f", [0, 0], true)],
        ["var a struct{ E[int] }; type E[P any] struct{}; func (*E[P]) f() {}"] = [],
        ["var a struct{ *E[int] }; type E[P any] struct{}; func (*E[P]) f() {}"] = [new("f", [0, 0], true)],

        // collisions
        ["var a struct{ E1; *E2 }; type ( E1 interface{ f() }; E2 struct{ f int })"] = [],
        ["var a struct{ E1; *E2 }; type ( E1 struct{ f int }; E2 struct{} ); func (E2) f() {}"] = [],

        // recursive generic types; see go.dev/issue/52715
        ["var a T[int]; type ( T[P any] struct { *N[P] }; N[P any] struct { *T[P] } ); func (N[P]) m() {}"] = [new("m", [0, 0], true)],
        ["var a T[int]; type ( T[P any] struct { *N[P] }; N[P any] struct { *T[P] } ); func (T[P]) m() {}"] = [new("m", [0], false)]
    };

    var tParamTests = new map<@string, slice<TestNewMethodSet_method>>{
        // By convention, look up a in the scope of "g"
        ["type C interface{ f() }; func g[T C](a T){}"] = [new("f", [0], true)],
        ["type C interface{ f() }; func g[T C]() { var a T; _ = a }"] = [new("f", [0], true)]

        // go.dev/issue/43621: We don't allow this anymore. Keep this code in case we
        // decide to revisit this decision.
        // "type C interface{ f() }; func g[T C]() { var a struct{T}; _ = a }": {{"f", []int{0, 0}, true}},

        // go.dev/issue/45639: We also don't allow this anymore.
        // "type C interface{ f() }; func g[T C]() { type Y T; var a Y; _ = a }": {},
    };

    void check(@string src, slice<TestNewMethodSet_method> methods, bool generic) {
        var pkg = mustTypecheck("package p;"u8 + src, nil, nil);

        var scope = pkg.Scope();
        if (generic) {
            var fn = pkg.Scope().Lookup("g")._<ж<types.Func>>();
            scope = fn.Scope();
        }
        var obj = scope.Lookup("a");
        if (obj == nil) {
            t.Errorf("%s: incorrect test case - no object a", src);
            return;
        }

        var ms = NewMethodSet(obj.Type());
        {
            nint got = ms.Len();
            nint want = len(methods); if (got != want) {
                t.Errorf("%s: got %d methods, want %d", src, got, want);
                return;
            }
        }
        foreach (var (i, m) in range(methods)) {
            var sel = ms.At(i);
            {
                @string got = sel.Obj().Name();
                @string want = m.name; if (got != want) {
                    t.Errorf("%s [method %d]: got name = %q at, want %q", src, i, got, want);
                }
            }
            {
                var (got, want) = (sel.Index(), m.index); if (!slices.Equal<slice<nint>, nint>(got, want)) {
                    t.Errorf("%s [method %d]: got index = %v, want %v", src, i, got, want);
                }
            }
            {
                var (got, want) = (sel.Indirect(), m.indirect); if (got != want) {
                    t.Errorf("%s [method %d]: got indirect = %v, want %v", src, i, got, want);
                }
            }
        }
    }

    foreach (var (src, methods) in tests) {
        check(src, methods, false);
    }

    foreach (var (src, methods) in tParamTests) {
        check(src, methods, true);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fooGoˢ => ref fooGoˢᵇ;
private static readonly @string fooGoˢᵇ = "foo.go"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string srcᶜ31 => ref srcᶜ31ᵇ;
private static readonly @string srcᶜ31ᵇ = """

package pkg

type Tree[T any] struct {
	*Node[T]
}

type Node[T any] struct {
	*Tree[T]
}

type Instance = *Tree[int]

"""u8.Raw();

// Test for go.dev/issue/52715
public static void TestNewMethodSet_RecursiveGeneric(ж<testing.T> t) {
    @string src = srcᶜ31;

    var fset = token.NewFileSet();
    var (f, err) = parser.ParseFile(fset, fooGoˢ, src, 0);
    if (err != nil) {
        throw panic(err);
    }
    var pkg = NewPackage(pkgˢ, f.Name.Name);
    var errΔ1 = NewChecker(nil, fset, pkg, nil).Files([f]); if (errΔ1 != nil) {
        throw panic(errΔ1);
    }

    var T = pkg.Scope().Lookup(instanceˢ).Type();
    _ = NewMethodSet(T); // verify that NewMethodSet terminates
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string invalidReceiverTypeˢ => ref invalidReceiverTypeˢᵇ;
private static readonly @string invalidReceiverTypeˢᵇ = "invalid receiver type"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string srcᶜ32 => ref srcᶜ32ᵇ;
private static readonly @string srcᶜ32ᵇ = """

package p
type T *int
func (T) m() {} // expected error: invalid receiver type

"""u8.Raw();

public static void TestIssue60634(ж<testing.T> t) {
    @string src = srcᶜ32;

    var fset = token.NewFileSet();
    var (f, err) = parser.ParseFile(fset, pGoˢ, src, 0);
    if (err != nil) {
        t.Fatal(err);
    }

    ref var conf = ref heap(new types.Config(), out var Ꮡconf);
    (var pkg, err) = Ꮡconf.Check("p", fset, [f], nil);
    if (err == nil || !strings.Contains(err.Error(), invalidReceiverTypeˢ)) {
        t.Fatalf("missing or unexpected error: %v", err);
    }

    // look up T.m and (*T).m
    var T = pkg.Scope().Lookup("T").Type();
    @string name = "m";
    foreach (var (_, recv) in range(slice<typesꓸType>([T, new types.PointerжΔType(NewPointer(T))]))) {
        // LookupFieldOrMethod and NewMethodSet must match:
        // either both find m or neither finds it.
        var (obj1, _, _) = LookupFieldOrMethod(recv, false, pkg, name);
        var mset = NewMethodSet(recv);
        if ((obj1 != nil) != (mset.Len() == 1)) {
            t.Fatalf("lookup(%v.%s): got obj = %v, mset = %v", recv, name, obj1, mset.OrTypedNil());
        }
        // If the method exists, both must return the same object.
        if (obj1 != nil) {
            var obj2 = mset.At(0).Obj();
            if (!AreEqual(obj1, obj2)) {
                t.Fatalf("%v != %v", obj1, obj2);
            }
        }
    }
}

} // end types_test_package
