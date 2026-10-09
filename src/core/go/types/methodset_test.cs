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

public static void TestNewMethodSet(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // Tests are expressed src -> methods, for simplifying the composite literal.
    // Should be kept in sync with TestLookupFieldOrMethod.
    var tests = new map<@string, slice<TestNewMethodSet_method>>{ // Named types

        ["var a T; type T struct{}; func (T) f() {}"u8] = new TestNewMethodSet_method[]{new("f"u8, new nint[]{0}.slice(), false)}.slice(),
        ["var a *T; type T struct{}; func (T) f() {}"u8] = new TestNewMethodSet_method[]{new("f"u8, new nint[]{0}.slice(), true)}.slice(),
        ["var a T; type T struct{}; func (*T) f() {}"u8] = new TestNewMethodSet_method[]{}.slice(),
        ["var a *T; type T struct{}; func (*T) f() {}"u8] = new TestNewMethodSet_method[]{new("f"u8, new nint[]{0}.slice(), true)}.slice(), // Generic named types

        ["var a T[int]; type T[P any] struct{}; func (T[P]) f() {}"u8] = new TestNewMethodSet_method[]{new("f"u8, new nint[]{0}.slice(), false)}.slice(),
        ["var a *T[int]; type T[P any] struct{}; func (T[P]) f() {}"u8] = new TestNewMethodSet_method[]{new("f"u8, new nint[]{0}.slice(), true)}.slice(),
        ["var a T[int]; type T[P any] struct{}; func (*T[P]) f() {}"u8] = new TestNewMethodSet_method[]{}.slice(),
        ["var a *T[int]; type T[P any] struct{}; func (*T[P]) f() {}"u8] = new TestNewMethodSet_method[]{new("f"u8, new nint[]{0}.slice(), true)}.slice(), // Interfaces

        ["var a T; type T interface{ f() }"u8] = new TestNewMethodSet_method[]{new("f"u8, new nint[]{0}.slice(), true)}.slice(),
        ["var a T1; type ( T1 T2; T2 interface{ f() } )"u8] = new TestNewMethodSet_method[]{new("f"u8, new nint[]{0}.slice(), true)}.slice(),
        ["var a T1; type ( T1 interface{ T2 }; T2 interface{ f() } )"u8] = new TestNewMethodSet_method[]{new("f"u8, new nint[]{0}.slice(), true)}.slice(), // Generic interfaces

        ["var a T[int]; type T[P any] interface{ f() }"u8] = new TestNewMethodSet_method[]{new("f"u8, new nint[]{0}.slice(), true)}.slice(),
        ["var a T1[int]; type ( T1[P any] T2[P]; T2[P any] interface{ f() } )"u8] = new TestNewMethodSet_method[]{new("f"u8, new nint[]{0}.slice(), true)}.slice(),
        ["var a T1[int]; type ( T1[P any] interface{ T2[P] }; T2[P any] interface{ f() } )"u8] = new TestNewMethodSet_method[]{new("f"u8, new nint[]{0}.slice(), true)}.slice(), // Embedding

        ["var a struct{ E }; type E interface{ f() }"u8] = new TestNewMethodSet_method[]{new("f"u8, new nint[]{0, 0}.slice(), true)}.slice(),
        ["var a *struct{ E }; type E interface{ f() }"u8] = new TestNewMethodSet_method[]{new("f"u8, new nint[]{0, 0}.slice(), true)}.slice(),
        ["var a struct{ E }; type E struct{}; func (E) f() {}"u8] = new TestNewMethodSet_method[]{new("f"u8, new nint[]{0, 0}.slice(), false)}.slice(),
        ["var a struct{ *E }; type E struct{}; func (E) f() {}"u8] = new TestNewMethodSet_method[]{new("f"u8, new nint[]{0, 0}.slice(), true)}.slice(),
        ["var a struct{ E }; type E struct{}; func (*E) f() {}"u8] = new TestNewMethodSet_method[]{}.slice(),
        ["var a struct{ *E }; type E struct{}; func (*E) f() {}"u8] = new TestNewMethodSet_method[]{new("f"u8, new nint[]{0, 0}.slice(), true)}.slice(), // Embedding of generic types

        ["var a struct{ E[int] }; type E[P any] interface{ f() }"u8] = new TestNewMethodSet_method[]{new("f"u8, new nint[]{0, 0}.slice(), true)}.slice(),
        ["var a *struct{ E[int] }; type E[P any] interface{ f() }"u8] = new TestNewMethodSet_method[]{new("f"u8, new nint[]{0, 0}.slice(), true)}.slice(),
        ["var a struct{ E[int] }; type E[P any] struct{}; func (E[P]) f() {}"u8] = new TestNewMethodSet_method[]{new("f"u8, new nint[]{0, 0}.slice(), false)}.slice(),
        ["var a struct{ *E[int] }; type E[P any] struct{}; func (E[P]) f() {}"u8] = new TestNewMethodSet_method[]{new("f"u8, new nint[]{0, 0}.slice(), true)}.slice(),
        ["var a struct{ E[int] }; type E[P any] struct{}; func (*E[P]) f() {}"u8] = new TestNewMethodSet_method[]{}.slice(),
        ["var a struct{ *E[int] }; type E[P any] struct{}; func (*E[P]) f() {}"u8] = new TestNewMethodSet_method[]{new("f"u8, new nint[]{0, 0}.slice(), true)}.slice(), // collisions

        ["var a struct{ E1; *E2 }; type ( E1 interface{ f() }; E2 struct{ f int })"u8] = new TestNewMethodSet_method[]{}.slice(),
        ["var a struct{ E1; *E2 }; type ( E1 struct{ f int }; E2 struct{} ); func (E2) f() {}"u8] = new TestNewMethodSet_method[]{}.slice(), // recursive generic types; see go.dev/issue/52715

        ["var a T[int]; type ( T[P any] struct { *N[P] }; N[P any] struct { *T[P] } ); func (N[P]) m() {}"u8] = new TestNewMethodSet_method[]{new("m"u8, new nint[]{0, 0}.slice(), true)}.slice(),
        ["var a T[int]; type ( T[P any] struct { *N[P] }; N[P any] struct { *T[P] } ); func (T[P]) m() {}"u8] = new TestNewMethodSet_method[]{new("m"u8, new nint[]{0}.slice(), false)}.slice()
    };
    var tParamTests = new map<@string, slice<TestNewMethodSet_method>>{ // By convention, look up a in the scope of "g"

        ["type C interface{ f() }; func g[T C](a T){}"u8] = new TestNewMethodSet_method[]{new("f"u8, new nint[]{0}.slice(), true)}.slice(),
        ["type C interface{ f() }; func g[T C]() { var a T; _ = a }"u8] = new TestNewMethodSet_method[]{new("f"u8, new nint[]{0}.slice(), true)}.slice()
    };
    // go.dev/issue/43621: We don't allow this anymore. Keep this code in case we
    // decide to revisit this decision.
    // "type C interface{ f() }; func g[T C]() { var a struct{T}; _ = a }": {{"f", []int{0, 0}, true}},
    // go.dev/issue/45639: We also don't allow this anymore.
    // "type C interface{ f() }; func g[T C]() { type Y T; var a Y; _ = a }": {},
    void check(@string src, slice<TestNewMethodSet_method> methods, bool generic) {
        var pkg = mustTypecheck("package p;"u8 + src, nil, nil);
        var scope = pkg.Scope();
        if (generic) {
            var fn = pkg.Scope().Lookup("g"u8)._<ж<types.Func>>();
            scope = fn.Scope();
        }
        var obj = scope.Lookup("a"u8);
        if (obj == default!) {
            Ꮡt.Errorf("%s: incorrect test case - no object a"u8, src);
            return;
        }
        var ms = NewMethodSet(obj.Type());
        {
            nint got = ms.Len();
            nint want = len(methods); if (got != want) {
                Ꮡt.Errorf("%s: got %d methods, want %d"u8, src, got, want);
                return;
            }
        }
        foreach (var (i, m) in methods) {
            var sel = ms.At(i);
            {
                @string got = sel.Obj().Name();
                @string want = m.name; if (got != want) {
                    Ꮡt.Errorf("%s [method %d]: got name = %q at, want %q"u8, src, i, got, want);
                }
            }
            {
                var (got, want) = (sel.Index(), m.index); if (!slices.Equal<slice<nint>, nint>(got, want)) {
                    Ꮡt.Errorf("%s [method %d]: got index = %v, want %v"u8, src, i, got, want);
                }
            }
            {
                var (got, want) = (sel.Indirect(), m.indirect); if (got != want) {
                    Ꮡt.Errorf("%s [method %d]: got indirect = %v, want %v"u8, src, i, got, want);
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
internal static readonly @string fooGoˢ = "foo.go"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string srcᶜ30 = """

package pkg

type Tree[T any] struct {
	*Node[T]
}

type Node[T any] struct {
	*Tree[T]
}

type Instance = *Tree[int]

"""u8;

// Test for go.dev/issue/52715
public static void TestNewMethodSet_RecursiveGeneric(ж<testing.T> Ꮡt) {
    @string src = srcᶜ30;
    var fset = token.NewFileSet();
    var (f, err) = parser.ParseFile(fset, fooGoˢ, src, 0);
    if (err != default!) {
        throw panic(err);
    }
    var pkg = NewPackage(pkgˢ, (~(~f).Name).Name);
    {
        var errΔ1 = NewChecker(nil, fset, pkg, nil).Files(new ж<ast.File>[]{f}.slice()); if (errΔ1 != default!) {
            throw panic(errΔ1);
        }
    }
    var T = pkg.Scope().Lookup(instanceˢ).Type();
    _ = NewMethodSet(T); // verify that NewMethodSet terminates
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string pGoˢ = "p.go"u8;
internal static readonly @string invalidReceiverTypeˢ = "invalid receiver type"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string srcᶜ31 = """

package p
type T *int
func (T) m() {} // expected error: invalid receiver type

"""u8;

public static void TestIssue60634(ж<testing.T> Ꮡt) {
    @string src = srcᶜ31;
    var fset = token.NewFileSet();
    var (f, err) = parser.ParseFile(fset, pGoˢ, src, 0);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    ref var conf = ref heap(new types.Config(), out var Ꮡconf);
    (var pkg, err) = Ꮡconf.Check("p"u8, fset, new ж<ast.File>[]{f}.slice(), nil);
    if (err == default! || !strings.Contains(err.Error(), invalidReceiverTypeˢ)) {
        Ꮡt.Fatalf("missing or unexpected error: %v"u8, err);
    }
    // look up T.m and (*T).m
    var T = pkg.Scope().Lookup("T"u8).Type();
    @string name = "m"u8;
    foreach (var (_, recv) in new typesꓸType[]{T, new types.PointerжΔType(NewPointer(T))}.slice()) {
        // LookupFieldOrMethod and NewMethodSet must match:
        // either both find m or neither finds it.
        var (obj1, _, _) = LookupFieldOrMethod(recv, false, pkg, name);
        var mset = NewMethodSet(recv);
        if ((obj1 != default!) != (mset.Len() == 1)) {
            Ꮡt.Fatalf("lookup(%v.%s): got obj = %v, mset = %v"u8, recv, name, obj1, mset.OrTypedNil());
        }
        // If the method exists, both must return the same object.
        if (obj1 != default!) {
            var obj2 = mset.At(0).Obj();
            if (!AreEqual(obj1, obj2)) {
                Ꮡt.Fatalf("%v != %v"u8, obj1, obj2);
            }
        }
    }
}

} // end types_test_package
