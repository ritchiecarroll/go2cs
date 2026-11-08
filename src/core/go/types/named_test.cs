// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using ast = global::go.go.ast_package;
using token = global::go.go.token_package;
using testing = testing_package;
using static global::go.go.types_package;
using global::go.go;
using static global::go.go.types_internal_test_package;
using types = global::go.go.types_package;
using ꓸꓸꓸtypesꓸType = Span<typesꓸType>;

partial class types_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string instˢ => ref instˢᵇ;
private static readonly @string instˢᵇ = "Inst"u8;
internal static ref readonly @string underlyingˢ => ref underlyingˢᵇ;
private static readonly @string underlyingˢᵇ = "Underlying"u8;
internal static ref readonly @string newMethodSetˢ => ref newMethodSetˢᵇ;
private static readonly @string newMethodSetˢᵇ = "NewMethodSet"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string srcᶜ33 => ref srcᶜ33ᵇ;
private static readonly @string srcᶜ33ᵇ = """

package p

type T struct {
	P int
}

func (T) M(int) {}
func (T) N() (i int) { return }

type G[P any] struct {
	F P
}

func (G[P]) M(P) {}
func (G[P]) N() (p P) { return }

type Inst = G[int]
	
"""u8.Raw();

internal partial struct BenchmarkNamed_tests /*dyn*/ {
    internal @string name;
    internal typesꓸType typ;
}

public static void BenchmarkNamed(ж<testing.B> b) {
    @string src = srcᶜ33;
    var pkg = mustTypecheck(src, nil, nil);

    typesꓸType T = pkg.Scope().Lookup("T").Type();
    typesꓸType G = pkg.Scope().Lookup("G").Type();
    typesꓸType SrcInst = pkg.Scope().Lookup(instˢ).Type();
    typesꓸType UserInst = mustInstantiate(new types_test_package.testing_BжTB(b), G, new types.BasicжΔType(Typ[Int]));

    var tests = slice<BenchmarkNamed_tests>([
        new("nongeneric", T),
        new("generic", G),
        new("src instance", SrcInst),
        new("user instance", UserInst)
    ]);

    var testsʗ1 = tests;
    b.Run(underlyingˢ, (ж<testing.B> bΔ1) => {
        foreach (var (_, vᴛ1) in range(testsʗ1)) {
            ref var test = ref heap(new BenchmarkNamed_tests(), out var Ꮡtest);
            test = vᴛ1;

            var testʗ1 = test;
            bΔ1.Run(test.name, (ж<testing.B> bΔ2) => {
                // Access underlying once, to trigger any lazy calculation.
                _ = testʗ1.typ.Underlying();
                bΔ2.ResetTimer();
                for (nint i = 0; i < (~bΔ2).N; i++) {
                    _ = testʗ1.typ.Underlying();
                }
            });
        }
    });

    var testsʗ2 = tests;
    b.Run(newMethodSetˢ, (ж<testing.B> bΔ3) => {
        foreach (var (_, vᴛ2) in range(testsʗ2)) {
            ref var test = ref heap(new BenchmarkNamed_tests(), out var Ꮡtest);
            test = vᴛ2;

            var testʗ2 = test;
            bΔ3.Run(test.name, (ж<testing.B> bΔ4) => {
                // Access underlying once, to trigger any lazy calculation.
                _ = NewMethodSet(testʗ2.typ);
                bΔ4.ResetTimer();
                for (nint i = 0; i < (~bΔ4).N; i++) {
                    _ = NewMethodSet(testʗ2.typ);
                }
            });
        }
    });
}

internal static typesꓸType mustInstantiate(testing.TB tb, typesꓸType orig, params ꓸꓸꓸtypesꓸType targsʗp) {
    var targs = targsʗp.slice();

    var (inst, err) = Instantiate(nil, orig, targs, true);
    if (err != nil) {
        tb.Fatal(err);
    }
    return inst;
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string srcᶜ34 => ref srcᶜ34ᵇ;
private static readonly @string srcᶜ34ᵇ = """

package p

type Tree[T any] struct {
	*Node[T]
}

func (*Tree[R]) N(r R) R { return r }

type Node[T any] struct {
	*Tree[T]
}

func (Node[Q]) M(Q) {}

type Inst = *Tree[int]

"""u8.Raw();

// Test that types do not expand infinitely, as in go.dev/issue/52715.
public static void TestFiniteTypeExpansion(ж<testing.T> t) {
    @string src = srcᶜ34;

    var fset = token.NewFileSet();
    var f = mustParse(fset, src);
    var pkg = NewPackage("p", f.Name.Name);
    var err = NewChecker(nil, fset, pkg, nil).Files([f]); if (err != nil) {
        t.Fatal(err);
    }

    ж<types.Named> firstFieldType(ж<types.Named> n) => n.Underlying()._<ж<types.Struct>>().Field(0).Type()._<ж<types.Pointer>>().Elem()._<ж<types.Named>>();

    var Inst = Unalias(pkg.Scope().Lookup(instˢ).Type())._<ж<types.Pointer>>().Elem()._<ж<types.Named>>();
    var Node = firstFieldType(Inst);
    var Tree = firstFieldType(Node);
    if (!Identical(new types.NamedжΔType(Inst), new types.NamedжΔType(Tree))) {
        t.Fatalf("Not a cycle: got %v, want %v", Tree.OrTypedNil(), Inst.OrTypedNil());
    }
    if (Inst != Tree) {
        t.Errorf("Duplicate instances in cycle: %s (%p) -> %s (%p) -> %s (%p)", Inst.OrTypedNil(), Inst.OrTypedNil(), Node.OrTypedNil(), Node.OrTypedNil(), Tree.OrTypedNil(), Tree.OrTypedNil());
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string srcᶜ35 => ref srcᶜ35ᵇ;
private static readonly @string srcᶜ35ᵇ = """

package p

type T struct{}

func (T) a() {}
func (T) c() {}
func (T) b() {}

"""u8.Raw();

// TestMethodOrdering is a simple test verifying that the indices of methods of
// a named type remain the same as long as the same source and AddMethod calls
// are presented to the type checker in the same order (go.dev/issue/61298).
public static void TestMethodOrdering(ж<testing.T> t) {
    @string src = srcᶜ35;
    // should get the same method order each time
    slice<@string> methods = default;
    for (nint i = 0; i < 5; i++) {
        // collect T methods as provided in src
        var pkg = mustTypecheck(src, nil, nil);
        var T = pkg.Scope().Lookup("T").Type()._<ж<types.Named>>();

        // add a few more methods manually
        foreach (var (_, name) in range(slice<@string>(["foo", "bar", "bal"]))) {
            var m = NewFunc(nopos, pkg, name, nil /* don't care about signature */);
            T.AddMethod(m);
        }

        // check method order
        if (i == 0) {
            // first round: collect methods in given order
            methods = new slice<@string>(T.NumMethods());
            foreach (var j in range(len(methods))) {
                methods[j] = T.Method(j).Name();
            }
        } else {
            // successive rounds: methods must appear in the same order
            {
                nint got = T.NumMethods(); if (got != len(methods)) {
                    t.Errorf("got %d methods, want %d", got, len(methods));
                    continue;
                }
            }
            foreach (var (j, m) in range(methods)) {
                @string got = T.Method(j).Name(); if (got != m) {
                    t.Errorf("got method %s, want %s", got, m);
                }
            }
        }
    }
}

} // end types_test_package
