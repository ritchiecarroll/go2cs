// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using fmt = fmt_package;
using testenv = global::go.@internal.testenv_package;
using strings = strings_package;
using testing = testing_package;
using static global::go.go.types_package;
using global::go.@internal;
using global::go.go;
using static global::go.go.types_internal_test_package;
using token = global::go.go.token_package;
using types = global::go.go.types_package;

partial class types_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string int32ˢ = "int32"u8;
internal static readonly @string runeˢ = "rune"u8;

internal partial struct TestIsAlias_type /*dyn*/ {
    internal ж<types.TypeName> name;
    internal bool alias;
}

public static void TestIsAlias(ж<testing.T> Ꮡt) {
    void check(ж<types.TypeName> obj, bool want) {
        {
            var got = obj.IsAlias(); if (got != want) {
                Ꮡt.Errorf("%v: got IsAlias = %v; want %v"u8, obj.OrTypedNil(), got, want);
            }
        }
    }
    // predeclared types
    check(Unsafe.Scope().Lookup(pointerˢ)._<ж<types.TypeName>>(), false);
    foreach (var (_, name) in Universe.Names()) {
        {
            var (obj, _) = Universe.Lookup(name)._<ж<types.TypeName>>(ᐧ); if (obj != nil) {
                check(obj, name == "any"u8 || name == "byte"u8 || name == "rune"u8);
            }
        }
    }
    // various other types
    var pkg = NewPackage("p"u8, "p"u8);
    var t1 = NewTypeName(nopos, pkg, "t1"u8, default!);
    var n1 = NewNamed(t1, new types.StructжΔType(@new<types.Struct>()), default!);
    var t5 = NewTypeName(nopos, pkg, "t5"u8, default!);
    NewTypeParam(t5, default!);
    foreach (var (_, test) in new TestIsAlias_type[]{
        new(NewTypeName(nopos, nil, "t0"u8, default!), false), // no type yet

        new(NewTypeName(nopos, pkg, "t0"u8, default!), false), // no type yet

        new(t1, false), // type name refers to named type and vice versa

        new(NewTypeName(nopos, nil, "t2"u8, new types.InterfaceжΔType(NewInterfaceType(default!, default!))), true), // type name refers to unnamed type

        new(NewTypeName(nopos, pkg, "t3"u8, new types.NamedжΔType(n1)), true), // type name refers to named type with different type name

        new(NewTypeName(nopos, nil, "t4"u8, new types.BasicжΔType(Typ[Int32])), true), // type name refers to basic type with different name

        new(NewTypeName(nopos, nil, int32ˢ, new types.BasicжΔType(Typ[Int32])), false), // type name refers to basic type with same name

        new(NewTypeName(nopos, pkg, int32ˢ, new types.BasicжΔType(Typ[Int32])), true), // type name is declared in user-defined package (outside Universe)

        new(NewTypeName(nopos, nil, runeˢ, new types.BasicжΔType(Typ[Rune])), true), // type name refers to basic type rune which is an alias already

        new(t5, false)
    }.slice()) {
        // type name refers to type parameter and vice versa
        check(test.name, test.alias);
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string srcᶜ35 = @"package p; type I interface { error }"u8;

// TestEmbeddedMethod checks that an embedded method is represented by
// the same Func Object as the original method. See also go.dev/issue/34421.
public static void TestEmbeddedMethod(ж<testing.T> Ꮡt) {
    @string src = srcᶜ35;
    var pkg = mustTypecheck(src, nil, nil);
    // get original error.Error method
    var eface = Universe.Lookup(errorˢ3);
    var (orig, _, _) = LookupFieldOrMethod(eface.Type(), false, nil, errorˢ4);
    if (orig == default!) {
        Ꮡt.Fatalf("original error.Error not found"u8);
    }
    // get embedded error.Error method
    var iface = pkg.Scope().Lookup("I"u8);
    var (embed, _, _) = LookupFieldOrMethod(iface.Type(), false, nil, errorˢ4);
    if (embed == default!) {
        Ꮡt.Fatalf("embedded error.Error not found"u8);
    }
    // original and embedded Error object should be identical
    if (!AreEqual(orig, embed)) {
        Ꮡt.Fatalf("%s (%p) != %s (%p)"u8, orig, orig, embed, embed);
    }
}

// requires GOEXPERIMENT=aliastypeparams

partial struct testObjectsᴛ1 /*dyn*/ {
    internal @string src;
    internal @string obj;
    internal @string want;
    internal bool alias; // needs materialized (and possibly generic) aliases
}
internal static slice<testObjectsᴛ1> testObjects = new testObjectsᴛ1[]{
    new("import \"io\"; var r io.Reader"u8, "r"u8, "var p.r io.Reader"u8, false),
    new("const c = 1.2"u8, "c"u8, "const p.c untyped float"u8, false),
    new("const c float64 = 3.14"u8, "c"u8, "const p.c float64"u8, false),
    new("type t struct{f int}"u8, "t"u8, "type p.t struct{f int}"u8, false),
    new("type t func(int)"u8, "t"u8, "type p.t func(int)"u8, false),
    new("type t[P any] struct{f P}"u8, "t"u8, "type p.t[P any] struct{f P}"u8, false),
    new("type t[P any] struct{f P}"u8, "t.P"u8, "type parameter P any"u8, false),
    new("type C interface{m()}; type t[P C] struct{}"u8, "t.P"u8, "type parameter P p.C"u8, false),
    new("type t = struct{f int}"u8, "t"u8, "type p.t = struct{f int}"u8, false),
    new("type t = func(int)"u8, "t"u8, "type p.t = func(int)"u8, false),
    new("type A = B; type B = int"u8, "A"u8, "type p.A = p.B"u8, true),
    new("type A[P ~int] = struct{}"u8, "A"u8, "type p.A[P ~int] = struct{}"u8, true),
    new("var v int"u8, "v"u8, "var p.v int"u8, false),
    new("func f(int) string"u8, "f"u8, "func p.f(int) string"u8, false),
    new("func g[P any](x P){}"u8, "g"u8, "func p.g[P any](x P)"u8, false),
    new("func g[P interface{~int}](x P){}"u8, "g.P"u8, "type parameter P interface{~int}"u8, false),
    new(""u8, "any"u8, "type any = interface{}"u8, false)
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string aliastypeparamsˢ = "aliastypeparams"u8;

public static void TestObjectString(ж<testing.T> Ꮡt) {
    testenv.MustHaveGoBuild(new types_test_package.testing_TжTB(Ꮡt));
    foreach (var (i, vᴛ1) in testObjects) {
        ref var test = ref heap(new testObjectsᴛ1(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        Ꮡt.Run(fmt.Sprint(i), (ж<testing.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                if (testʗ1.alias) {
                    var revert = setGOEXPERIMENT(aliastypeparamsˢ);
                    var revertʗ1 = revert;
                    defer(revertʗ1, ref ᒐ);
                    tΔ1.Setenv(godebugˢ, gotypesalias1ˢ);
                }
                @string src = "package p; "u8 + testʗ1.src;
                var (pkg, err) = typecheck(src, nil, nil);
                if (err != default!) {
                    tΔ1.Fatalf("%s: %s"u8, src, err);
                }
                var names = strings.Split(testʗ1.obj, "."u8);
                if (len(names) != 1 && len(names) != 2) {
                    tΔ1.Fatalf("%s: invalid object path %s"u8, testʗ1.src, testʗ1.obj);
                }
                types.Object obj = default!;
                for (var s = pkg.Scope(); s != nil && obj == default!; s = s.Parent()) {
                    obj = s.Lookup(names[0]);
                }
                if (obj == default!) {
                    tΔ1.Fatalf("%s: %s not found"u8, testʗ1.src, names[0]);
                }
                if (len(names) == 2) {
                    {
                        var (typ, ok) = obj.Type()._<TestInstanceInfo_typeᴛ1>(ᐧ); if (ok){
                            obj = lookupTypeParamObj(typ.TypeParams(), names[1]);
                            if (obj == default!) {
                                tΔ1.Fatalf("%s: %s not found"u8, testʗ1.src, testʗ1.obj);
                            }
                        } else {
                            tΔ1.Fatalf("%s: %s has no type parameters"u8, testʗ1.src, names[0]);
                        }
                    }
                }
                {
                    @string got = obj.String(); if (got != testʗ1.want) {
                        tΔ1.Errorf("%s: got %s, want %s"u8, testʗ1.src, got, testʗ1.want);
                    }
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

internal static types.Object lookupTypeParamObj(ж<types.TypeParamList> Ꮡlist, @string name) {
    ref var list = ref Ꮡlist.DerefOrNull();

    for (nint i = 0; i < Ꮡlist.Len(); i++) {
        var tpar = list.At(i);
        if (tpar.Obj().Name() == name) {
            return new types.TypeNameжObject(tpar.Obj());
        }
    }
    return default!;
}

} // end types_test_package
