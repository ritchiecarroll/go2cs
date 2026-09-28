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
internal static readonly @string instˢ = "Inst"u8;
internal static readonly @string underlyingˢ = "Underlying"u8;
internal static readonly @string newMethodSetˢ = "NewMethodSet"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string srcᶜ32 = """

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
	
"""u8;

[GoType("dyn")] internal partial struct BenchmarkNamed_tests {
    internal @string name;
    internal typesꓸType typ;
}

public static void BenchmarkNamed(ж<testing.B> Ꮡb) {
    @string src = srcᶜ32;
    var pkg = mustTypecheck(src, nil, nil);
    typesꓸType T = pkg.Scope().Lookup("T"u8).Type();
    typesꓸType G = pkg.Scope().Lookup("G"u8).Type();
    typesꓸType SrcInst = pkg.Scope().Lookup(instˢ).Type();
    typesꓸType UserInst = mustInstantiate(new types_test_package.testing_BжTB(Ꮡb), G, new types.BasicжΔType(Typ[Int]));
    var tests = new BenchmarkNamed_tests[]{
        new("nongeneric"u8, T),
        new("generic"u8, G),
        new("src instance"u8, SrcInst),
        new("user instance"u8, UserInst)
    }.slice();
    var testsʗ1 = tests;
    Ꮡb.Run(underlyingˢ, (ж<testing.B> bΔ1) => {
        foreach (var (_, vᴛ1) in testsʗ1) {
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
    Ꮡb.Run(newMethodSetˢ, (ж<testing.B> bΔ3) => {
        foreach (var (_, vᴛ2) in testsʗ2) {
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
    if (err != default!) {
        tb.Fatal(err);
    }
    return inst;
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string srcᶜ33 = """

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

"""u8;

// Test that types do not expand infinitely, as in go.dev/issue/52715.
public static void TestFiniteTypeExpansion(ж<testing.T> Ꮡt) {
    @string src = srcᶜ33;
    var fset = token.NewFileSet();
    var f = mustParse(fset, src);
    var pkg = NewPackage("p"u8, (~(~f).Name).Name);
    {
        var err = NewChecker(nil, fset, pkg, nil).Files(new ж<ast.File>[]{f}.slice()); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    ж<types.Named> firstFieldType(ж<types.Named> n) => n.Underlying()._<ж<types.Struct>>().Field(0).Type()._<ж<types.Pointer>>().Elem()._<ж<types.Named>>();
    var Inst = Unalias(pkg.Scope().Lookup(instˢ).Type())._<ж<types.Pointer>>().Elem()._<ж<types.Named>>();
    var Node = firstFieldType(Inst);
    var Tree = firstFieldType(Node);
    if (!Identical(new types.NamedжΔType(Inst), new types.NamedжΔType(Tree))) {
        Ꮡt.Fatalf("Not a cycle: got %v, want %v"u8, Tree.OrTypedNil(), Inst.OrTypedNil());
    }
    if (Inst != Tree) {
        Ꮡt.Errorf("Duplicate instances in cycle: %s (%p) -> %s (%p) -> %s (%p)"u8, Inst.OrTypedNil(), Inst.OrTypedNil(), Node.OrTypedNil(), Node.OrTypedNil(), Tree.OrTypedNil(), Tree.OrTypedNil());
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string srcᶜ34 = """

package p

type T struct{}

func (T) a() {}
func (T) c() {}
func (T) b() {}

"""u8;

// TestMethodOrdering is a simple test verifying that the indices of methods of
// a named type remain the same as long as the same source and AddMethod calls
// are presented to the type checker in the same order (go.dev/issue/61298).
public static void TestMethodOrdering(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    @string src = srcᶜ34;
    // should get the same method order each time
    slice<@string> methods = default!;
    for (nint i = 0; i < 5; i++) {
        // collect T methods as provided in src
        var pkg = mustTypecheck(src, nil, nil);
        var T = pkg.Scope().Lookup("T"u8).Type()._<ж<types.Named>>();
        // add a few more methods manually
        foreach (var (_, name) in new @string[]{"foo"u8, "bar"u8, "bal"u8}.slice()) {
            var m = NewFunc(nopos, pkg, name, nil);
            /* don't care about signature */
            T.AddMethod(m);
        }
        // check method order
        if (i == 0){
            // first round: collect methods in given order
            methods = new slice<@string>(T.NumMethods());
            foreach (var (j, _) in methods) {
                methods[j] = T.Method(j).Name();
            }
        } else {
            // successive rounds: methods must appear in the same order
            {
                nint got = T.NumMethods(); if (got != len(methods)) {
                    Ꮡt.Errorf("got %d methods, want %d"u8, got, len(methods));
                    continue;
                }
            }
            foreach (var (j, m) in methods) {
                {
                    @string got = T.Method(j).Name(); if (got != m) {
                        Ꮡt.Errorf("got method %s, want %s"u8, got, m);
                    }
                }
            }
        }
    }
}

} // end types_test_package
