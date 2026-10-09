// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
global using TestTypeFor_myiface = object;

namespace go;

using Δreflect = reflect_package;
using Δtesting = testing_package;
using static global::go.reflect_internal_test_package;

partial class reflect_test_package {

internal partial struct TestTypeFor_mystring /*@string*/;

internal partial struct TestTypeFor_testcases /*dyn*/ {
    internal any wantFrom;
    internal reflectꓸType got;
}

public static void TestTypeFor(ж<Δtesting.T> Ꮡt) {
    var testcases = new TestTypeFor_testcases[]{
        new(@new<nint>(), Δreflect.TypeFor<nint>()),
        new(@new<int64>(), Δreflect.TypeFor<int64>()),
        new(@new<@string>(), Δreflect.TypeFor<@string>()),
        new(@new<TestTypeFor_mystring>(), Δreflect.TypeFor<TestTypeFor_mystring>()),
        new(@new<any>(), Δreflect.TypeFor<any>()),
        new(@new<TestTypeFor_myiface>(), Δreflect.TypeFor<TestTypeFor_myiface>())
    }.slice();
    foreach (var (_, tc) in testcases) {
        var want = Δreflect.ValueOf(tc.wantFrom).Elem().Type();
        if (!AreEqual(want, tc.got)) {
            Ꮡt.Errorf("unexpected reflect.Type: got %v; want %v"u8, tc.got, want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string structOfDoesNotSupportˢ = "StructOf does not support methods of embedded interfaces"u8;

internal partial interface TestStructOfEmbeddedIfaceMethodCall_Named /*dyn*/ {
    @string Name();
}

public static void TestStructOfEmbeddedIfaceMethodCall(ж<Δtesting.T> Ꮡt) {
    var typ = Δreflect.StructOf(new Δreflect.StructField[]{
        new(
            Anonymous: true,
            Name: "Named"u8,
            Type: Δreflect.TypeFor<TestStructOfEmbeddedIfaceMethodCall_Named>()
        )
    }.slice());
    var v = Δreflect.New(typ).Elem();
    v.Field(0).Set(
        Δreflect.ValueOf(Δreflect.TypeFor<@string>()));
    var x = v.Interface()._<TestStructOfEmbeddedIfaceMethodCall_Named>();
    var xʗ1 = x;
    shouldPanic(structOfDoesNotSupportˢ, () => {
        _ = xʗ1.Name();
    });
}

internal partial struct TestIsRegularMemory_args /*dyn*/ {
    internal reflectꓸType t;
}

internal partial struct TestIsRegularMemory_S /*dyn*/ {
    /*embed*/ internal nint @int;
}

internal partial struct TestIsRegularMemory_tests /*dyn*/ {
    internal @string name;
    internal TestIsRegularMemory_args args;
    internal bool want;
}

internal partial struct TestIsRegularMemory_i /*dyn*/ {
    internal nint i;
    internal TestIsRegularMemory_S s;
}

internal partial struct TestIsRegularMemory_iᴛ1 /*dyn*/ {
    internal TestIsRegularMemory_S _;
}

internal partial struct TestIsRegularMemory_iᴛ2 /*dyn*/ {
    internal nint i;
    internal TestIsRegularMemory_S _;
}

internal partial struct TestIsRegularMemory_iᴛ3 /*dyn*/ {
    internal int16 a;
    internal int32 b;
}

internal partial struct TestIsRegularMemory_iᴛ4 /*dyn*/ {
    internal int32 x;
    internal int16 y;
}

internal partial struct TestIsRegularMemory_iᴛ5 /*dyn*/ {
    internal int32 _;
}

public static void TestIsRegularMemory(ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var tests = new TestIsRegularMemory_tests[]{
        new("struct{i int}"u8, new TestIsRegularMemory_args(Δreflect.TypeOf(new TestCanIntUintFloatComplex_typeᴛ1())), true),
        new("struct{}"u8, new TestIsRegularMemory_args(Δreflect.TypeOf(new EmptyStruct())), true),
        new("struct{i int; s S}"u8, new TestIsRegularMemory_args(Δreflect.TypeOf(new TestIsRegularMemory_i())
        ), true),
        new("map[int][int]"u8, new TestIsRegularMemory_args(Δreflect.TypeOf(new map<nint, nint>{})), false),
        new("[4]chan int"u8, new TestIsRegularMemory_args(Δreflect.TypeOf(new channel<nint>[]{}.array(4))), true),
        new("[0]struct{_ S}"u8, new TestIsRegularMemory_args(Δreflect.TypeOf(new TestIsRegularMemory_iᴛ1[]{}.array())
        ), true),
        new("struct{i int; _ S}"u8, new TestIsRegularMemory_args(Δreflect.TypeOf(new TestIsRegularMemory_iᴛ2())
        ), false),
        new("struct{a int16; b int32}"u8, new TestIsRegularMemory_args(Δreflect.TypeOf(new TestIsRegularMemory_iᴛ3())
        ), false),
        new("struct {x int32; y int16}"u8, new TestIsRegularMemory_args(Δreflect.TypeOf(new TestIsRegularMemory_iᴛ4())
        ), false),
        new("struct {_ int32 }"u8, new TestIsRegularMemory_args(Δreflect.TypeOf(new TestIsRegularMemory_iᴛ5())), false)
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestIsRegularMemory_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(tt.name, (ж<Δtesting.T> tΔ1) => {
            {
                var got = reflect_internal_test_package.IsRegularMemory(ttʗ1.args.t); if (got != ttʗ1.want) {
                    tΔ1.Errorf("isRegularMemory() = %v, want %v"u8, got, ttʗ1.want);
                }
            }
        });
    }
}

internal static reflectꓸType sinkType;

public static void BenchmarkTypeForString(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        sinkType = Δreflect.TypeFor<@string>();
    }
}

public static void BenchmarkTypeForError(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        sinkType = Δreflect.TypeFor<error>();
    }
}

internal partial struct TestType_CanSeq_tests /*dyn*/ {
    internal @string name;
    internal reflectꓸType tr;
    internal bool want;
}

public static void TestType_CanSeq(ж<Δtesting.T> Ꮡt) {
    var tests = new TestType_CanSeq_tests[]{
        new("func(func(int) bool)"u8, Δreflect.TypeOf((Func<nint, bool> _) => {
        }), true),
        new("func(func(int))"u8, Δreflect.TypeOf((Action<nint> _) => {
        }), false),
        new("int64"u8, Δreflect.TypeOf((int64)1), true),
        new("uint64"u8, Δreflect.TypeOf((uint64)1), true),
        new("*[4]int"u8, Δreflect.TypeOf(Ꮡ(new nint[]{}.array(4))), true),
        new("chan int64"u8, Δreflect.TypeOf(new channel<int64>(0)), true),
        new("map[int]int"u8, Δreflect.TypeOf(new map<nint, nint>()), true),
        new("string"u8, Δreflect.TypeOf((@string)""u8), true),
        new("[]int"u8, Δreflect.TypeOf(new nint[]{}.slice()), true)
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestType_CanSeq_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(tt.name, (ж<Δtesting.T> tΔ1) => {
            {
                var got = ttʗ1.tr.CanSeq(); if (got != ttʗ1.want) {
                    tΔ1.Errorf("Type.CanSeq() = %v, want %v"u8, got, ttʗ1.want);
                }
            }
        });
    }
}

internal partial struct TestType_CanSeq2_tests /*dyn*/ {
    internal @string name;
    internal reflectꓸType tr;
    internal bool want;
}

public static void TestType_CanSeq2(ж<Δtesting.T> Ꮡt) {
    var tests = new TestType_CanSeq2_tests[]{
        new("func(func(int, int) bool)"u8, Δreflect.TypeOf((Func<nint, nint, bool> _) => {
        }), true),
        new("func(func(int, int))"u8, Δreflect.TypeOf((Action<nint, nint> _) => {
        }), false),
        new("int64"u8, Δreflect.TypeOf((int64)1), false),
        new("uint64"u8, Δreflect.TypeOf((uint64)1), false),
        new("*[4]int"u8, Δreflect.TypeOf(Ꮡ(new nint[]{}.array(4))), true),
        new("chan int64"u8, Δreflect.TypeOf(new channel<int64>(0)), false),
        new("map[int]int"u8, Δreflect.TypeOf(new map<nint, nint>()), true),
        new("string"u8, Δreflect.TypeOf((@string)""u8), true),
        new("[]int"u8, Δreflect.TypeOf(new nint[]{}.slice()), true)
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestType_CanSeq2_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(tt.name, (ж<Δtesting.T> tΔ1) => {
            {
                var got = ttʗ1.tr.CanSeq2(); if (got != ttʗ1.want) {
                    tΔ1.Errorf("Type.CanSeq2() = %v, want %v"u8, got, ttʗ1.want);
                }
            }
        });
    }
}

} // end reflect_test_package
