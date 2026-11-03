// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
global using Loopy = object;
global using Tint2 = go.@internal.reflectlite_test_package.Tint;

namespace go.@internal;

using base64 = encoding.base64_package;
using fmt = fmt_package;
using abi = global::go.@internal.abi_package;
using static global::go.@internal.reflectlite_package;
using math = math_package;
using reflect = reflect_package;
using Δruntime = runtime_package;
using testing = testing_package;
using @unsafe = unsafe_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using encoding;
using global::go.@internal;
using reflectlite = global::go.@internal.reflectlite_package;
using static global::go.@internal.reflectlite_internal_test_package;
using ꓸꓸꓸPoint = Span<reflectlite_test_package.Point>;

partial class reflectlite_test_package {

public static reflectꓸValue ToValue(reflectlite.Value v) {
    return reflect.ValueOf(reflectlite_internal_test_package.ToInterface(v));
}

public static @string TypeString(reflectliteꓸType t) {
    return fmt.Sprintf("%T"u8, reflectlite_internal_test_package.ToInterface(reflectlite_internal_test_package.Zero(t)));
}

partial struct integer /*num:nint*/;

partial struct T {
    internal nint a;
    internal float64 b;
    internal @string c;
    internal ж<nint> d;
}

partial struct pair {
    internal any i;
    internal @string s;
}

internal static partial void assert(ж<testing.T> t, @string s, @string want) {
    t.Helper();
    if (s != want) {
        t.Errorf("have %#q want %#q", s, want);
    }
}

    partial struct Δtype /*dyn*/ {
        internal nint x;
    }
    partial struct Δtypeᴛ1 /*dyn*/ {
        internal int8 x;
    }
    partial struct Δtypeᴛ2 /*dyn*/ {
        internal int16 x;
    }
    partial struct Δtypeᴛ3 /*dyn*/ {
        internal int32 x;
    }
    partial struct Δtypeᴛ4 /*dyn*/ {
        internal int64 x;
    }
    partial struct Δtypeᴛ5 /*dyn*/ {
        internal nuint x;
    }
    partial struct Δtypeᴛ6 /*dyn*/ {
        internal uint8 x;
    }
    partial struct Δtypeᴛ7 /*dyn*/ {
        internal uint16 x;
    }
    partial struct Δtypeᴛ8 /*dyn*/ {
        internal uint32 x;
    }
    partial struct Δtypeᴛ9 /*dyn*/ {
        internal uint64 x;
    }
    partial struct Δtypeᴛ10 /*dyn*/ {
        internal float32 x;
    }
    partial struct Δtypeᴛ11 /*dyn*/ {
        internal float64 x;
    }
    partial struct Δtypeᴛ12 /*dyn*/ {
        internal ж<ж<int8>> x;
    }
    partial struct Δtypeᴛ13 /*dyn*/ {
        internal ж<ж<integer>> x;
    }
    partial struct Δtypeᴛ14 /*dyn*/ {
        internal array<int32> x = new(32);
    }
    partial struct Δtypeᴛ15 /*dyn*/ {
        internal slice<int8> x;
    }
    partial struct Δtypeᴛ16 /*dyn*/ {
        internal map<@string, int32> x;
    }
    partial struct Δtypeᴛ17 /*dyn*/ {
        internal channel/*<-*/<@string> x = channel/*<-*/<@string>.SendOnly;
    }
    partial struct typeᴛ18_x /*dyn*/ {
        internal channel<ж<int32>> c;
        internal float32 d;
    }

    partial struct Δtypeᴛ18 /*dyn*/ {
        internal typeᴛ18_x x;
    }
    partial struct Δtypeᴛ19 /*dyn*/ {
        internal Action<int8, int32> x;
    }
    partial struct typeᴛ20_x /*dyn*/ {
        internal Action<channel<ж<integer>>, ж<int8>> c;
    }

    partial struct Δtypeᴛ20 /*dyn*/ {
        internal typeᴛ20_x x;
    }
    partial struct typeᴛ21_x /*dyn*/ {
        internal int8 a;
        internal int32 b;
    }

    partial struct Δtypeᴛ21 /*dyn*/ {
        internal typeᴛ21_x x;
    }
    partial struct typeᴛ22_x /*dyn*/ {
        internal int8 a;
        internal int8 b;
        internal int32 c;
    }

    partial struct Δtypeᴛ22 /*dyn*/ {
        internal typeᴛ22_x x;
    }
    partial struct typeᴛ23_x /*dyn*/ {
        internal int8 a;
        internal int8 b;
        internal int8 c;
        internal int32 d;
    }

    partial struct Δtypeᴛ23 /*dyn*/ {
        internal typeᴛ23_x x;
    }
    partial struct typeᴛ24_x /*dyn*/ {
        internal int8 a;
        internal int8 b;
        internal int8 c;
        internal int8 d;
        internal int32 e;
    }

    partial struct Δtypeᴛ24 /*dyn*/ {
        internal typeᴛ24_x x;
    }
    partial struct typeᴛ25_x /*dyn*/ {
        internal int8 a;
        internal int8 b;
        internal int8 c;
        internal int8 d;
        internal int8 e;
        internal int32 f;
    }

    partial struct Δtypeᴛ25 /*dyn*/ {
        internal typeᴛ25_x x;
    }
    partial struct typeᴛ26_x /*dyn*/ {
        internal int8 a; /*`reflect:"hi there"`*/
    }

    partial struct Δtypeᴛ26 /*dyn*/ {
        internal typeᴛ26_x x;
    }
    partial struct typeᴛ27_x /*dyn*/ {
        internal int8 a; /*`reflect:"hi \x00there\t\n\"\\"`*/
    }

    partial struct Δtypeᴛ27 /*dyn*/ {
        internal typeᴛ27_x x;
    }
    partial struct typeᴛ28_x /*dyn*/ {
        internal Actionꓸꓸꓸ<nint> f;
    }

    partial struct Δtypeᴛ28 /*dyn*/ {
        internal typeᴛ28_x x;
    }
    partial struct typeᴛ29_x /*dyn*/ {
        /*embed*/ internal int32 int32;
        /*embed*/ internal int64 int64;
    }

    partial struct Δtypeᴛ29 /*dyn*/ {
        internal typeᴛ29_x x;
    }
internal static slice<pair> typeTests = slice<pair>([
    new(new Δtype(), "int"),
    new(new Δtypeᴛ1(), "int8"),
    new(new Δtypeᴛ2(), "int16"),
    new(new Δtypeᴛ3(), "int32"),
    new(new Δtypeᴛ4(), "int64"),
    new(new Δtypeᴛ5(), "uint"),
    new(new Δtypeᴛ6(), "uint8"),
    new(new Δtypeᴛ7(), "uint16"),
    new(new Δtypeᴛ8(), "uint32"),
    new(new Δtypeᴛ9(), "uint64"),
    new(new Δtypeᴛ10(), "float32"),
    new(new Δtypeᴛ11(), "float64"),
    new(new Δtypeᴛ1(), "int8"),
    new(new Δtypeᴛ12(), "**int8"),
    new(new Δtypeᴛ13(), "**reflectlite_test.integer"),
    new(new Δtypeᴛ14(), "[32]int32"),
    new(new Δtypeᴛ15(), "[]int8"),
    new(new Δtypeᴛ16(), "map[string]int32"),
    new(new Δtypeᴛ17(), "chan<- string"),
    new(new Δtypeᴛ18(),
        "struct { c chan *int32; d float32 }"
    ),
    new(new Δtypeᴛ19(), "func(int8, int32)"),
    new(new Δtypeᴛ20(),
        "struct { c func(chan *reflectlite_test.integer, *int8) }"
    ),
    new(new Δtypeᴛ21(),
        "struct { a int8; b int32 }"
    ),
    new(new Δtypeᴛ22(),
        "struct { a int8; b int8; c int32 }"
    ),
    new(new Δtypeᴛ23(),
        "struct { a int8; b int8; c int8; d int32 }"
    ),
    new(new Δtypeᴛ24(),
        "struct { a int8; b int8; c int8; d int8; e int32 }"
    ),
    new(new Δtypeᴛ25(),
        "struct { a int8; b int8; c int8; d int8; e int8; f int32 }"
    ),
    new(new Δtypeᴛ26(),
        @"struct { a int8 ""reflect:\""hi there\"""" }"
    ),
    new(new Δtypeᴛ27(),
        @"struct { a int8 ""reflect:\""hi \\x00there\\t\\n\\\""\\\\\"""" }"
    ),
    new(new Δtypeᴛ28(),
        "struct { f func(...int) }"
    ),
    // {struct {
    // 	x (interface {
    // 		a(func(func(int) int) func(func(int)) int)
    // 		b()
    // 	})
    // }{},
    // 	"interface { reflectlite_test.a(func(func(int) int) func(func(int)) int); reflectlite_test.b() }",
    // },
    new(new Δtypeᴛ29(),
        "struct { int32; int64 }"
    )
]);

internal static slice<pair> valueTests = slice<pair>([
    new(@new<nint>(), "132"),
    new(@new<int8>(), "8"),
    new(@new<int16>(), "16"),
    new(@new<int32>(), "32"),
    new(@new<int64>(), "64"),
    new(@new<nuint>(), "132"),
    new(@new<uint8>(), "8"),
    new(@new<uint16>(), "16"),
    new(@new<uint32>(), "32"),
    new(@new<uint64>(), "64"),
    new(@new<float32>(), "256.25"),
    new(@new<float64>(), "512.125"),
    new(@new<complex64>(), "532.125+10i"),
    new(@new<complex128>(), "564.25+1i"),
    new(@new<@string>(), "stringy cheese"),
    new(@new<bool>(), "true"),
    new(@new<ж<int8>>(), "*int8(0)"),
    new(@new<ж<ж<int8>>>(), "**int8(0)"),
    new(Ꮡ(new array<int32>(5)), "[5]int32{0, 0, 0, 0, 0}"),
    new(@new<ж<ж<integer>>>(), "**reflectlite_test.integer(0)"),
    new(@new<map<@string, int32>>(), "map[string]int32{<can't iterate on maps>}"),
    new(Ꮡ(channel/*<-*/<@string>.SendOnly), "chan<- string"),
    new(@new<Action<int8, int32>>(), "func(int8, int32)(arg)"),
    new(@new<typeᴛ18_x>(),
        "struct { c chan *int32; d float32 }{chan *int32, 0}"
    ),
    new(@new<typeᴛ20_x>(),
        "struct { c func(chan *reflectlite_test.integer, *int8) }{func(chan *reflectlite_test.integer, *int8)(arg)}"
    ),
    new(@new<typeᴛ21_x>(),
        "struct { a int8; b int32 }{0, 0}"
    ),
    new(@new<typeᴛ22_x>(),
        "struct { a int8; b int8; c int32 }{0, 0, 0}"
    )
]);

internal static partial void testType(ж<testing.T> t, nint i, reflectliteꓸType typ, @string want) {
    @string s = TypeString(typ);
    if (s != want) {
        t.Errorf("#%d: have %#q, want %#q", i, s, want);
    }
}

internal static partial void testReflectType(ж<testing.T> t, nint i, reflectliteꓸType typ, @string want) {
    @string s = TypeString(typ);
    if (s != want) {
        t.Errorf("#%d: have %#q, want %#q", i, s, want);
    }
}

public static void TestTypes(ж<testing.T> t) {
    foreach (var (i, tt) in typeTests) {
        testReflectType(t, i, reflectlite_internal_test_package.Field(ValueOf(tt.i), 0).Type(), tt.s);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object stringyCheeseˢ = (@string)"stringy cheese"u8;

public static void TestSetValue(ж<testing.T> t) {
    foreach (var (i, tt) in valueTests) {
        var v = ValueOf(tt.i).Elem();
        switch (v.Kind()) {
        case var ᴛ when ᴛ == abi.Int: {
            v.Set(ValueOf((nint)132));
            break;
        }
        case var ᴛ when ᴛ == abi.Int8: {
            v.Set(ValueOf((int8)8));
            break;
        }
        case var ᴛ when ᴛ == abi.Int16: {
            v.Set(ValueOf((int16)16));
            break;
        }
        case var ᴛ when ᴛ == abi.Int32: {
            v.Set(ValueOf((int32)32));
            break;
        }
        case var ᴛ when ᴛ == abi.Int64: {
            v.Set(ValueOf((int64)64));
            break;
        }
        case var ᴛ when ᴛ == abi.Uint: {
            v.Set(ValueOf((nuint)132));
            break;
        }
        case var ᴛ when ᴛ == abi.Uint8: {
            v.Set(ValueOf((uint8)8));
            break;
        }
        case var ᴛ when ᴛ == abi.Uint16: {
            v.Set(ValueOf((uint16)16));
            break;
        }
        case var ᴛ when ᴛ == abi.Uint32: {
            v.Set(ValueOf((uint32)32));
            break;
        }
        case var ᴛ when ᴛ == abi.Uint64: {
            v.Set(ValueOf((uint64)64));
            break;
        }
        case var ᴛ when ᴛ == abi.Float32: {
            v.Set(ValueOf((float32)256.25F));
            break;
        }
        case var ᴛ when ᴛ == abi.Float64: {
            v.Set(ValueOf(512.125D));
            break;
        }
        case var ᴛ when ᴛ == abi.Complex64: {
            v.Set(ValueOf((complex64)(532.125F + 10F.i())));
            break;
        }
        case var ᴛ when ᴛ == abi.Complex128: {
            v.Set(ValueOf((complex128)(564.25D + 1D.i())));
            break;
        }
        case var ᴛ when ᴛ == abi.ΔString: {
            v.Set(ValueOf(stringyCheeseˢ));
            break;
        }
        case var ᴛ when ᴛ == abi.Bool: {
            v.Set(ValueOf(true));
            break;
        }}
        @string s = valueToString(v);
        if (s != tt.s) {
            t.Errorf("#%d: have %#q, want %#q", i, s, tt.s);
        }
    }
}

internal partial struct TestCanSetField_embed /*dyn*/ {
    internal nint x;
    public nint X;
}

internal partial struct TestCanSetField_Embed /*dyn*/ {
    internal nint x;
    public nint X;
}

internal partial struct TestCanSetField_S1 /*dyn*/ {
    /*embed*/ internal TestCanSetField_embed embed;
    internal nint x;
    public nint X;
}

internal partial struct TestCanSetField_S2 /*dyn*/ {
    /*embed*/ internal ж<TestCanSetField_embed> embed;
    internal nint x;
    public nint X;
}

internal partial struct TestCanSetField_S3 /*dyn*/ {
    /*embed*/ public TestCanSetField_Embed Embed;
    internal nint x;
    public nint X;
}

internal partial struct TestCanSetField_S4 /*dyn*/ {
    /*embed*/ public ж<TestCanSetField_Embed> Embed;
    internal nint x;
    public nint X;
}

internal partial struct TestCanSetField_testCase /*dyn*/ {
    internal slice<nint> index;
    internal bool canSet;
}

internal partial struct TestCanSetField_tests /*dyn*/ {
    internal reflectlite.Value val;
    internal slice<TestCanSetField_testCase> cases;
}

public static void TestCanSetField(ж<testing.T> t) {
    var tests = slice<TestCanSetField_tests>([new(
        val: ValueOf(Ꮡ(new TestCanSetField_S1())),
        cases: slice<TestCanSetField_testCase>([
            new(slice<nint>([0]), false),
            new(slice<nint>([0, 0]), false),
            new(slice<nint>([0, 1]), true),
            new(slice<nint>([1]), false),
            new(slice<nint>([2]), true)
        ])
    ), new(
        val: ValueOf(Ꮡ(new TestCanSetField_S2(embed: Ꮡ(new TestCanSetField_embed())))),
        cases: slice<TestCanSetField_testCase>([
            new(slice<nint>([0]), false),
            new(slice<nint>([0, 0]), false),
            new(slice<nint>([0, 1]), true),
            new(slice<nint>([1]), false),
            new(slice<nint>([2]), true)
        ])
    ), new(
        val: ValueOf(Ꮡ(new TestCanSetField_S3())),
        cases: slice<TestCanSetField_testCase>([
            new(slice<nint>([0]), true),
            new(slice<nint>([0, 0]), false),
            new(slice<nint>([0, 1]), true),
            new(slice<nint>([1]), false),
            new(slice<nint>([2]), true)
        ])
    ), new(
        val: ValueOf(Ꮡ(new TestCanSetField_S4(Embed: Ꮡ(new TestCanSetField_Embed())))),
        cases: slice<TestCanSetField_testCase>([
            new(slice<nint>([0]), true),
            new(slice<nint>([0, 0]), false),
            new(slice<nint>([0, 1]), true),
            new(slice<nint>([1]), false),
            new(slice<nint>([2]), true)
        ])
    )
    ]);

    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestCanSetField_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(tt.val.Type().Name(), (ж<testing.T> tΔ1) => {
            foreach (var (_, tc) in ttʗ1.cases) {
                var f = ttʗ1.val;
                foreach (var (_, i) in tc.index) {
                    if (f.Kind() == Ptr) {
                        f = f.Elem();
                    }
                    f = reflectlite_internal_test_package.Field(f, i);
                }
                {
                    var got = f.CanSet(); if (got != tc.canSet) {
                        tΔ1.Errorf("CanSet() = %v, want %v", got, tc.canSet);
                    }
                }
            }
        });
    }
}

internal static readonly ж<nint> Ꮡ_i = new StaticBox<nint>(7);
internal static ref nint _i => ref ((StaticBox<nint>)Ꮡ_i).ValueSlot;

internal static slice<pair> valueToStringTests = slice<pair>([
    new((nint)123, "123"),
    new(123.5D, "123.5"),
    new((byte)123, "123"),
    new((@string)"abc", "abc"),
    new(new T(123, 456.75D, "hello", Ꮡ_i), "reflectlite_test.T{123, 456.75, hello, *int(&7)}"),
    new(@new<channel<ж<T>>>(), "*chan *reflectlite_test.T(&chan *reflectlite_test.T)"),
    new(new nint[]{1, 2, 3, 4, 5, 6, 7, 8, 9, 10}.array(), "[10]int{1, 2, 3, 4, 5, 6, 7, 8, 9, 10}"),
    new(Ꮡ(new nint[]{1, 2, 3, 4, 5, 6, 7, 8, 9, 10}.array()), "*[10]int(&[10]int{1, 2, 3, 4, 5, 6, 7, 8, 9, 10})"),
    new(slice<nint>([1, 2, 3, 4, 5, 6, 7, 8, 9, 10]), "[]int{1, 2, 3, 4, 5, 6, 7, 8, 9, 10}"),
    new(Ꮡ(slice<nint>([1, 2, 3, 4, 5, 6, 7, 8, 9, 10])), "*[]int(&[]int{1, 2, 3, 4, 5, 6, 7, 8, 9, 10})")
]);

public static void TestValueToString(ж<testing.T> t) {
    foreach (var (i, test) in valueToStringTests) {
        @string s = valueToString(ValueOf(test.i));
        if (s != test.s) {
            t.Errorf("#%d: have %#q, want %#q", i, s, test.s);
        }
    }
}

public static void TestPtrSetNil(ж<testing.T> t) {
    ref var i = ref heap(new int32(), out var Ꮡi);
    i = 1234;
    ref var ip = ref heap<ж<int32>>(Ꮡi, out var Ꮡip);
    var vip = ValueOf(Ꮡip);
    vip.Elem().Set(reflectlite_internal_test_package.Zero(vip.Elem().Type()));
    if (ip != nil) {
        t.Errorf("got non-nil (%d), want nil", ip.Value);
    }
}

public static void TestMapSetNil(ж<testing.T> t) {
    ref var m = ref heap<map<@string, nint>>(new map<@string, nint>(), out var Ꮡm);
    var vm = ValueOf(Ꮡm);
    vm.Elem().Set(reflectlite_internal_test_package.Zero(vm.Elem().Type()));
    if (m != nil) {
        t.Errorf("got non-nil (%p), want nil", m);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string int8ˢ => ref int8ˢᵇ;
private static readonly @string int8ˢᵇ = "int8"u8;
internal static ref readonly @string structCChanInt32DFloat32ˢ => ref structCChanInt32DFloat32ˢᵇ;
private static readonly @string structCChanInt32DFloat32ˢᵇ = "*struct { c chan *int32; d float32 }"u8;
internal static ref readonly @string structCChanInt32DFloat32ˢ2 => ref structCChanInt32DFloat32ˢ2ᵇ;
private static readonly @string structCChanInt32DFloat32ˢ2ᵇ = "struct { c chan *int32; d float32 }"u8;

public static void TestAll(ж<testing.T> t) {
    testType(t, 1, TypeOf((int8)0), int8ˢ);
    testType(t, 2, TypeOf((ж<int8>)nil).Elem(), int8ˢ);

    var typ = TypeOf((ж<typeᴛ18_x>)nil);
    testType(t, 3, typ, structCChanInt32DFloat32ˢ);
    var etyp = typ.Elem();
    testType(t, 4, etyp, structCChanInt32DFloat32ˢ2);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string float64ˢ => ref float64ˢᵇ;
private static readonly @string float64ˢᵇ = "float64"u8;
internal static readonly object v2InterfaceDidNotReturnˢ = (@string)"v2.Interface() did not return float64, got "u8;

internal partial struct TestInterfaceValue_inter /*dyn*/ {
    public any E;
}

public static void TestInterfaceValue(ж<testing.T> t) {
    ref var inter = ref heap(new TestInterfaceValue_inter(), out var Ꮡinter);
    inter.E = 123.456D;
    var v1 = ValueOf(Ꮡinter);
    var v2 = reflectlite_internal_test_package.Field(v1.Elem(), 0);
    // assert(t, TypeString(v2.Type()), "interface {}")
    var v3 = v2.Elem();
    assert(t, TypeString(v3.Type()), float64ˢ);

    var i3 = reflectlite_internal_test_package.ToInterface(v2);
    {
        var (_, ok) = i3._<float64>(ᐧ); if (!ok) {
            t.Error(v2InterfaceDidNotReturnˢ, TypeOf(i3));
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string funcˢ => ref funcˢᵇ;
private static readonly @string funcˢᵇ = "func()"u8;

public static void TestFunctionValue(ж<testing.T> t) {
    any x = () => {
    };
    var v = ValueOf(x);
    if (fmt.Sprint(reflectlite_internal_test_package.ToInterface(v)) != fmt.Sprint(x)) {
        t.Fatalf("TestFunction returned wrong pointer");
    }
    assert(t, TypeString(v.Type()), funcˢ);
}

partial struct appendTestsᴛ1 /*dyn*/ {
    internal slice<nint> orig, extra;
}
internal static slice<appendTestsᴛ1> appendTests = slice<appendTestsᴛ1>([
    new(new slice<nint>(2, 4), slice<nint>([22])),
    new(new slice<nint>(2, 4), slice<nint>([22, 33, 44]))
]);

internal static bool sameInts(slice<nint> x, slice<nint> y) {
    if (len(x) != len(y)) {
        return false;
    }
    foreach (var (i, xx) in x) {
        if (xx != y[i]) {
            return false;
        }
    }
    return true;
}

internal partial struct TestBigUnnamedStruct_b /*dyn*/ {
    internal int64 a, b, c, d;
}

public static void TestBigUnnamedStruct(ж<testing.T> t) {
    var b = new TestBigUnnamedStruct_b(1, 2, 3, 4);
    var v = ValueOf(b);
    var b1 = reflectlite_internal_test_package.ToInterface(v)._<TestBigUnnamedStruct_b>();
    if (b1.a != b.a || b1.b != b.b || b1.c != b.c || b1.d != b.d) {
        t.Errorf("ValueOf(%v).Interface().(*Big) = %v", b, b1);
    }
}

partial struct big {
    internal int64 a, b, c, d, e;
}

public static void TestBigStruct(ж<testing.T> t) {
    var b = new big(1, 2, 3, 4, 5);
    var v = ValueOf(b);
    var b1 = reflectlite_internal_test_package.ToInterface(v)._<big>();
    if (b1.a != b.a || b1.b != b.b || b1.c != b.c || b1.d != b.d || b1.e != b.e) {
        t.Errorf("ValueOf(%v).Interface().(big) = %v", b, b1);
    }
}

partial struct Basic {
    internal nint x;
    internal float32 y;
}

partial struct NotBasic /*Basic*/;

partial struct DeepEqualTest {
    internal any a, b;
    internal bool eq;
}

// Simple functions for DeepEqual tests.
internal static Action fn1; // nil.
internal static Action fn2; // nil.
internal static Action fn3 = () => {
    fn1();
};                          // Not nil.

partial struct self {
}

partial class Loop /*ж<Loop>*/;

internal static readonly ж<Loop> Ꮡloop1 = new StaticBox<Loop>(default(Loop));
internal static ref Loop loop1 => ref ((StaticBox<Loop>)Ꮡloop1).ValueSlot;
internal static readonly ж<Loop> Ꮡloop2 = new StaticBox<Loop>(default(Loop));
internal static ref Loop loop2 => ref ((StaticBox<Loop>)Ꮡloop2).ValueSlot;

internal static readonly ж<Loopy> Ꮡloopy1 = new StaticBox<Loopy>(default(Loopy));
internal static ref Loopy loopy1 => ref ((StaticBox<Loopy>)Ꮡloopy1).ValueSlot;
internal static readonly ж<Loopy> Ꮡloopy2 = new StaticBox<Loopy>(default(Loopy));
internal static ref Loopy loopy2 => ref ((StaticBox<Loopy>)Ꮡloopy2).ValueSlot;

[GoInit] internal static void init() {
    loop1 = Ꮡloop2;
    loop2 = Ꮡloop1;

    loopy1 = Ꮡloopy2;
    loopy2 = Ꮡloopy1;
}

internal static slice<DeepEqualTest> typeOfTests = slice<DeepEqualTest>([
    // Equalities
    new(default, default, true),
    new((nint)1, (nint)1, true),
    new((int32)1, (int32)1, true),
    new(0.5D, 0.5D, true),
    new((float32)0.5F, (float32)0.5F, true),
    new((@string)"hello", (@string)"hello", true),
    new(new slice<nint>(10), new slice<nint>(10), true),
    new(Ꮡ(new nint[]{1, 2, 3}.array()), Ꮡ(new nint[]{1, 2, 3}.array()), true),
    new(new Basic(1, 0.5F), new Basic(1, 0.5F), true),
    new((error)default, (error)default, true),
    new(new map<nint, @string>{[1] = "one", [2] = "two"}, new map<nint, @string>{[2] = "two", [1] = "one"}, true),
    new((fn1).OrTypedNilFunc(), (fn2).OrTypedNilFunc(), true),

    // Inequalities
    new((nint)1, (nint)2, false),
    new((int32)1, (int32)2, false),
    new(0.5D, 0.6D, false),
    new((float32)0.5F, (float32)0.6F, false),
    new((@string)"hello", (@string)"hey", false),
    new(new slice<nint>(10), new slice<nint>(11), false),
    new(Ꮡ(new nint[]{1, 2, 3}.array()), Ꮡ(new nint[]{1, 2, 4}.array()), false),
    new(new Basic(1, 0.5F), new Basic(1, 0.6F), false),
    new(new Basic(1, 0F), new Basic(2, 0F), false),
    new(new map<nint, @string>{[1] = "one", [3] = "two"}, new map<nint, @string>{[2] = "two", [1] = "one"}, false),
    new(new map<nint, @string>{[1] = "one", [2] = "txo"}, new map<nint, @string>{[2] = "two", [1] = "one"}, false),
    new(new map<nint, @string>{[1] = "one"}, new map<nint, @string>{[2] = "two", [1] = "one"}, false),
    new(new map<nint, @string>{[2] = "two", [1] = "one"}, new map<nint, @string>{[1] = "one"}, false),
    new(default, (nint)1, false),
    new((nint)1, default, false),
    new((fn1).OrTypedNilFunc(), (fn3).OrTypedNilFunc(), false),
    new((fn3).OrTypedNilFunc(), (fn3).OrTypedNilFunc(), false),
    new(slice<slice<nint>>([slice<nint>([1])]), slice<slice<nint>>([slice<nint>([2])]), false),
    new(math.NaN(), math.NaN(), false),
    new(Ꮡ(new float64[]{math.NaN()}.array()), Ꮡ(new float64[]{math.NaN()}.array()), false),
    new(Ꮡ(new float64[]{math.NaN()}.array()), new self(), true),
    new(slice<float64>([math.NaN()]), slice<float64>([math.NaN()]), false),
    new(slice<float64>([math.NaN()]), new self(), true),
    new(new map<float64, float64>{[math.NaN()] = 1D}, new map<float64, float64>{[1D] = 2D}, false),
    new(new map<float64, float64>{[math.NaN()] = 1D}, new self(), true),

    // Nil vs empty: not the same.
    new(slice<nint>([]), slice<nint>(default), false),
    new(slice<nint>([]), slice<nint>([]), true),
    new(slice<nint>(default), slice<nint>(default), true),
    new(new map<nint, nint>{}, (map<nint, nint>)default, false),
    new(new map<nint, nint>{}, new map<nint, nint>{}, true),
    new((map<nint, nint>)default, (map<nint, nint>)default, true),

    // Mismatched types
    new((nint)1, 1.0D, false),
    new((int32)1, (int64)1, false),
    new(0.5D, (@string)"hello", false),
    new(slice<nint>([1, 2, 3]), new nint[]{1, 2, 3}.array(), false),
    new(Ꮡ(new any[]{(nint)1, (nint)2, (nint)4}.array()), Ꮡ(new any[]{(nint)1, (nint)2, (@string)"s"}.array()), false),
    new(new Basic(1, 0.5F), new NotBasic(new Basic(1, 0.5F)), false),
    new(new map<nuint, @string>{[1] = "one", [2] = "two"}, new map<nint, @string>{[2] = "two", [1] = "one"}, false),

    // Possible loops.
    new(Ꮡloop1, Ꮡloop1, true),
    new(Ꮡloop1, Ꮡloop2, true),
    new(Ꮡloopy1, Ꮡloopy1, true),
    new(Ꮡloopy1, Ꮡloopy2, true)
]);

public static void TestTypeOf(ж<testing.T> t) {
    // Special case for nil
    {
        var typ = TypeOf(default); if (typ != nil) {
            t.Errorf("expected nil type for nil value; got %v", typ);
        }
    }
    foreach (var (_, test) in typeOfTests) {
        var v = ValueOf(test.a);
        if (!v.IsValid()) {
            continue;
        }
        var typ = TypeOf(test.a);
        if (!AreEqual(typ, v.Type())) {
            t.Errorf("TypeOf(%v) = %v, but ValueOf(%v).Type() = %v", test.a, typ, test.a, v.Type());
        }
    }
}

public static partial void Nil(any a, ж<testing.T> t) {
    var n = reflectlite_internal_test_package.Field(ValueOf(a), 0);
    if (!n.IsNil()) {
        t.Errorf("%v should be nil", a);
    }
}

public static partial void NotNil(any a, ж<testing.T> t) {
    var n = reflectlite_internal_test_package.Field(ValueOf(a), 0);
    if (n.IsNil()) {
        t.Errorf("value of type %v should not be nil", TypeString(ValueOf(a).Type()));
    }
}

internal partial struct TestIsNil_doNil /*dyn*/ {
    internal ж<nint> x;
}

internal partial struct TestIsNil_doNilᴛ1 /*dyn*/ {
    internal any x;
}

internal partial struct TestIsNil_doNilᴛ2 /*dyn*/ {
    internal map<@string, nint> x;
}

internal partial struct TestIsNil_doNilᴛ3 /*dyn*/ {
    internal Func<bool> x;
}

internal partial struct TestIsNil_doNilᴛ4 /*dyn*/ {
    internal channel<nint> x;
}

internal partial struct TestIsNil_doNilᴛ5 /*dyn*/ {
    internal slice<@string> x;
}

internal partial struct TestIsNil_doNilᴛ6 /*dyn*/ {
    internal @unsafe.Pointer x;
}

internal partial struct TestIsNil_si /*dyn*/ {
    internal slice<nint> x;
}

internal partial struct TestIsNil_mi /*dyn*/ {
    internal map<nint, nint> x;
}

internal partial struct TestIsNil_fi /*dyn*/ {
    internal Action<ж<testing.T>> x;
}

public static void TestIsNil(ж<testing.T> t) {
    // These implement IsNil.
    // Wrap in extra struct to hide interface type.
    var doNil = slice<any>([
        new TestIsNil_doNil(),
        new TestIsNil_doNilᴛ1(),
        new TestIsNil_doNilᴛ2(),
        new TestIsNil_doNilᴛ3(),
        new TestIsNil_doNilᴛ4(),
        new TestIsNil_doNilᴛ5(),
        new TestIsNil_doNilᴛ6()
    ]);
    foreach (var (_, ts) in doNil) {
        var ty = reflectlite_internal_test_package.TField(TypeOf(ts), 0);
        var v = reflectlite_internal_test_package.Zero(ty);
        v.IsNil(); // panics if not okay to call
    }

    // Check the implementations
    TestIsNil_doNil pi = default;
    Nil(pi, t);
    pi.x = @new<nint>();
    NotNil(pi, t);

    TestIsNil_si si = default;
    Nil(si, t);
    si.x = new slice<nint>(10);
    NotNil(si, t);

    TestIsNil_doNilᴛ4 ci = default;
    Nil(ci, t);
    ci.x = new channel<nint>(0);
    NotNil(ci, t);

    TestIsNil_mi mi = default;
    Nil(mi, t);
    mi.x = new map<nint, nint>();
    NotNil(mi, t);

    TestIsNil_doNilᴛ1 ii = default;
    Nil(ii, t);
    ii.x = (nint)2;
    NotNil(ii, t);

    TestIsNil_fi fi = default;
    Nil(fi, t);
    fi.x = TestIsNil;
    NotNil(fi, t);
}

// Indirect returns the value that v points to.
// If v is a nil pointer, Indirect returns a zero Value.
// If v is not a pointer, Indirect returns v.
public static reflectlite.Value Indirect(reflectlite.Value v) {
    if (v.Kind() != Ptr) {
        return v;
    }
    return v.Elem();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object valueOfIntNilElemIsValidˢ = (@string)"ValueOf((*int)(nil)).Elem().IsValid()"u8;

public static void TestNilPtrValueSub(ж<testing.T> t) {
    ж<nint> pi = default;
    {
        var pv = ValueOf(pi.OrTypedNil()); if (pv.Elem().IsValid()) {
            t.Error(valueOfIntNilElemIsValidˢ);
        }
    }
}

partial struct Point {
    internal nint x, y;
}

// This will be index 0.
public static nint AnotherMethod(this Point p, nint scale) {
    return -1;
}

// This will be index 1.
public static nint Dist(this Point p, nint scale) {
    //println("Point.Dist", p.x, p.y, scale)
    return p.x * p.x * scale + p.y * p.y * scale;
}

// This will be index 2.
public static nint GCMethod(this Point p, nint k) {
    Δruntime.GC();
    return k + p.x;
}

// This will be index 3.
public static void NoArgs(this Point p) {
    // Exercise no-argument/no-result paths.
}

// This will be index 4.
public static nint TotalDist(this Point p, params ꓸꓸꓸPoint pointsʗp) {
    var points = pointsʗp.sslice();

    nint tot = 0;
    foreach (var (_, q) in points) {
        nint dx = q.x - p.x;
        nint dy = q.y - p.y;
        tot += dx * dx + dy * dy; // Should call Sqrt, but it's just a test.
    }
    return tot;
}

partial struct D1 {
    internal nint d;
}

partial struct D2 {
    internal nint d;
}

internal partial struct TestImportPath_tests /*dyn*/ {
    internal reflectliteꓸType t;
    internal @string path;
}

public static void TestImportPath(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var tests = slice<TestImportPath_tests>([
        new(TypeOf(Ꮡ(new base64.Encoding())).Elem(), "encoding/base64"),
        new(TypeOf((nint)0), ""),
        new(TypeOf((int8)0), ""),
        new(TypeOf((int16)0), ""),
        new(TypeOf((int32)0), ""),
        new(TypeOf((int64)0), ""),
        new(TypeOf((nuint)0), ""),
        new(TypeOf((uint8)0), ""),
        new(TypeOf((uint16)0), ""),
        new(TypeOf((uint32)0), ""),
        new(TypeOf((uint64)0), ""),
        new(TypeOf((uintptr)0), ""),
        new(TypeOf((float32)0F), ""),
        new(TypeOf((float64)0D), ""),
        new(TypeOf((complex64)0F), ""),
        new(TypeOf((complex128)0D), ""),
        new(TypeOf((byte)0), ""),
        new(TypeOf((rune)0), ""),
        new(TypeOf(slice<byte>(default)), ""),
        new(TypeOf(slice<rune>(default)), ""),
        new(TypeOf((@string)""u8), ""),
        new(TypeOf((ж<any>)nil).Elem(), ""),
        new(TypeOf((ж<byte>)nil), ""),
        new(TypeOf((ж<rune>)nil), ""),
        new(TypeOf((ж<int64>)nil), ""),
        new(TypeOf(new map<@string, nint>{}), ""),
        new(TypeOf((ж<error>)nil).Elem(), ""),
        new(TypeOf((ж<Point>)nil), ""),
        new(TypeOf((ж<Point>)nil).Elem(), "internal/reflectlite_test")
    ]);
    foreach (var (_, test) in tests) {
        {
            @string path = test.t.PkgPath(); if (path != test.path) {
                Ꮡt.Errorf("%v.PkgPath() = %q, want %q", test.t, path, test.path);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingMallocCountInˢ = (@string)"skipping malloc count in short mode"u8;
internal static readonly object skippingGomaxprocs1ˢ = (@string)"skipping; GOMAXPROCS>1"u8;

internal static void noAlloc(ж<testing.T> t, nint n, Action<nint> f) {
    if (testing.Short()) {
        t.Skip(skippingMallocCountInˢ);
    }
    if (Δruntime.GOMAXPROCS(0) > 1) {
        t.Skip(skippingGomaxprocs1ˢ);
    }
    nint i = -1;
    var allocs = testing.AllocsPerRun(n, () => {
        f(i);
        i++;
    });
    if (allocs > 0D) {
        t.Errorf("%d iterations: got %v mallocs, want 0", n, allocs);
    }
}

public static void TestAllocations(ж<testing.T> t) {
    noAlloc(t, 100, (nint j) => {
        any i = default;
        reflectlite.Value v = new(nil);

        i = slice<nint>([j, j, j]);
        v = ValueOf(i);
        if (v.Len() != 3) {
            throw panic("wrong length");
        }
    });
    noAlloc(t, 100, (nint j) => {
        any i = default;
        reflectlite.Value v = new(nil);

        i = nint (nint jΔ1) => jΔ1;
        v = ValueOf(i);
        if (reflectlite_internal_test_package.ToInterface(v)._<Func<nint, nint>>()(j) != j) {
            throw panic("wrong result");
        }
    });
}

internal partial struct TestSetPanic_t0 /*dyn*/ {
    public nint W;
}

internal partial struct TestSetPanic_t1 /*dyn*/ {
    public nint Y;
    /*embed*/ internal TestSetPanic_t0 t0;
}

internal partial struct TestSetPanic_T2 /*dyn*/ {
    public nint Z;
    internal TestSetPanic_t0 namedT0;
}

internal partial struct TestSetPanic_T /*dyn*/ {
    public nint X;
    /*embed*/ internal TestSetPanic_t1 t1;
    /*embed*/ public TestSetPanic_T2 T2;
    public TestSetPanic_t1 NamedT1;
    public TestSetPanic_T2 NamedT2;
    internal TestSetPanic_t1 namedT1;
    internal TestSetPanic_T2 namedT2;
}

public static void TestSetPanic(ж<testing.T> t) {
    void ok(Action f) {
        f();
    }
    var bad = shouldPanic;
    void clear(reflectlite.Value vΔ1) {
        vΔ1.Set(reflectlite_internal_test_package.Zero(vΔ1.Type()));
    }

    // not addressable
    ref var v = ref heap<reflectlite.Value>(ValueOf(new TestSetPanic_T()), out var Ꮡv);
    var clearʗ1 = clear;
    bad(() => {
        clearʗ1(reflectlite_internal_test_package.Field(Ꮡv.Value, 0));
    }); // .X
    var clearʗ2 = clear;
    bad(() => {
        clearʗ2(reflectlite_internal_test_package.Field(Ꮡv.Value, 1));
    }); // .t1
    var clearʗ3 = clear;
    bad(() => {
        clearʗ3(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 1), 0));
    }); // .t1.Y
    var clearʗ4 = clear;
    bad(() => {
        clearʗ4(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 1), 1));
    }); // .t1.t0
    var clearʗ5 = clear;
    bad(() => {
        clearʗ5(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 1), 1), 0));
    }); // .t1.t0.W
    var clearʗ6 = clear;
    bad(() => {
        clearʗ6(reflectlite_internal_test_package.Field(Ꮡv.Value, 2));
    }); // .T2
    var clearʗ7 = clear;
    bad(() => {
        clearʗ7(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 2), 0));
    }); // .T2.Z
    var clearʗ8 = clear;
    bad(() => {
        clearʗ8(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 2), 1));
    }); // .T2.namedT0
    var clearʗ9 = clear;
    bad(() => {
        clearʗ9(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 2), 1), 0));
    }); // .T2.namedT0.W
    var clearʗ10 = clear;
    bad(() => {
        clearʗ10(reflectlite_internal_test_package.Field(Ꮡv.Value, 3));
    }); // .NamedT1
    var clearʗ11 = clear;
    bad(() => {
        clearʗ11(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 3), 0));
    }); // .NamedT1.Y
    var clearʗ12 = clear;
    bad(() => {
        clearʗ12(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 3), 1));
    }); // .NamedT1.t0
    var clearʗ13 = clear;
    bad(() => {
        clearʗ13(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 3), 1), 0));
    }); // .NamedT1.t0.W
    var clearʗ14 = clear;
    bad(() => {
        clearʗ14(reflectlite_internal_test_package.Field(Ꮡv.Value, 4));
    }); // .NamedT2
    var clearʗ15 = clear;
    bad(() => {
        clearʗ15(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 4), 0));
    }); // .NamedT2.Z
    var clearʗ16 = clear;
    bad(() => {
        clearʗ16(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 4), 1));
    }); // .NamedT2.namedT0
    var clearʗ17 = clear;
    bad(() => {
        clearʗ17(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 4), 1), 0));
    }); // .NamedT2.namedT0.W
    var clearʗ18 = clear;
    bad(() => {
        clearʗ18(reflectlite_internal_test_package.Field(Ꮡv.Value, 5));
    }); // .namedT1
    var clearʗ19 = clear;
    bad(() => {
        clearʗ19(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 5), 0));
    }); // .namedT1.Y
    var clearʗ20 = clear;
    bad(() => {
        clearʗ20(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 5), 1));
    }); // .namedT1.t0
    var clearʗ21 = clear;
    bad(() => {
        clearʗ21(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 5), 1), 0));
    }); // .namedT1.t0.W
    var clearʗ22 = clear;
    bad(() => {
        clearʗ22(reflectlite_internal_test_package.Field(Ꮡv.Value, 6));
    }); // .namedT2
    var clearʗ23 = clear;
    bad(() => {
        clearʗ23(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 6), 0));
    }); // .namedT2.Z
    var clearʗ24 = clear;
    bad(() => {
        clearʗ24(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 6), 1));
    }); // .namedT2.namedT0
    var clearʗ25 = clear;
    bad(() => {
        clearʗ25(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 6), 1), 0));
    }); // .namedT2.namedT0.W

    // addressable
    v = ValueOf(Ꮡ(new TestSetPanic_T())).Elem();
    var clearʗ26 = clear;
    ok(() => {
        clearʗ26(reflectlite_internal_test_package.Field(Ꮡv.Value, 0));
    }); // .X
    var clearʗ27 = clear;
    bad(() => {
        clearʗ27(reflectlite_internal_test_package.Field(Ꮡv.Value, 1));
    }); // .t1
    var clearʗ28 = clear;
    ok(() => {
        clearʗ28(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 1), 0));
    }); // .t1.Y
    var clearʗ29 = clear;
    bad(() => {
        clearʗ29(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 1), 1));
    }); // .t1.t0
    var clearʗ30 = clear;
    ok(() => {
        clearʗ30(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 1), 1), 0));
    }); // .t1.t0.W
    var clearʗ31 = clear;
    ok(() => {
        clearʗ31(reflectlite_internal_test_package.Field(Ꮡv.Value, 2));
    }); // .T2
    var clearʗ32 = clear;
    ok(() => {
        clearʗ32(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 2), 0));
    }); // .T2.Z
    var clearʗ33 = clear;
    bad(() => {
        clearʗ33(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 2), 1));
    }); // .T2.namedT0
    var clearʗ34 = clear;
    bad(() => {
        clearʗ34(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 2), 1), 0));
    }); // .T2.namedT0.W
    var clearʗ35 = clear;
    ok(() => {
        clearʗ35(reflectlite_internal_test_package.Field(Ꮡv.Value, 3));
    }); // .NamedT1
    var clearʗ36 = clear;
    ok(() => {
        clearʗ36(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 3), 0));
    }); // .NamedT1.Y
    var clearʗ37 = clear;
    bad(() => {
        clearʗ37(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 3), 1));
    }); // .NamedT1.t0
    var clearʗ38 = clear;
    ok(() => {
        clearʗ38(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 3), 1), 0));
    }); // .NamedT1.t0.W
    var clearʗ39 = clear;
    ok(() => {
        clearʗ39(reflectlite_internal_test_package.Field(Ꮡv.Value, 4));
    }); // .NamedT2
    var clearʗ40 = clear;
    ok(() => {
        clearʗ40(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 4), 0));
    }); // .NamedT2.Z
    var clearʗ41 = clear;
    bad(() => {
        clearʗ41(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 4), 1));
    }); // .NamedT2.namedT0
    var clearʗ42 = clear;
    bad(() => {
        clearʗ42(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 4), 1), 0));
    }); // .NamedT2.namedT0.W
    var clearʗ43 = clear;
    bad(() => {
        clearʗ43(reflectlite_internal_test_package.Field(Ꮡv.Value, 5));
    }); // .namedT1
    var clearʗ44 = clear;
    bad(() => {
        clearʗ44(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 5), 0));
    }); // .namedT1.Y
    var clearʗ45 = clear;
    bad(() => {
        clearʗ45(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 5), 1));
    }); // .namedT1.t0
    var clearʗ46 = clear;
    bad(() => {
        clearʗ46(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 5), 1), 0));
    }); // .namedT1.t0.W
    var clearʗ47 = clear;
    bad(() => {
        clearʗ47(reflectlite_internal_test_package.Field(Ꮡv.Value, 6));
    }); // .namedT2
    var clearʗ48 = clear;
    bad(() => {
        clearʗ48(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 6), 0));
    }); // .namedT2.Z
    var clearʗ49 = clear;
    bad(() => {
        clearʗ49(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 6), 1));
    }); // .namedT2.namedT0
    var clearʗ50 = clear;
    bad(() => {
        clearʗ50(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 6), 1), 0));
    }); // .namedT2.namedT0.W
}

internal static void shouldPanic(Action f) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            if (recover() == nil) {
                throw panic("did not panic");
            }
        }, ref ᒐ);
        f();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

partial struct S {
    internal int64 i1;
    internal int64 i2;
}

public static void TestBigZero(ж<testing.T> t) {
    UntypedInt size = /* 1 << 10 */ 1024;
    array<byte> v = new(1024); /* size */
    var z = reflectlite_internal_test_package.ToInterface(reflectlite_internal_test_package.Zero(ValueOf(v).Type()))._<array<byte>>();
    for (nint i = 0; i < size; i++) {
        if (z[i] != 0) {
            t.Fatalf("Zero object not all zero, index %d", i);
        }
    }
}

// Used to have inconsistency between IsValid() and Kind() != Invalid.
internal partial struct TestInvalid_T /*dyn*/ {
    internal any v;
}

public static void TestInvalid(ж<testing.T> t) {
    var v = reflectlite_internal_test_package.Field(ValueOf(new TestInvalid_T()), 0);
    if (v.IsValid() != true || v.Kind() != Interface) {
        t.Errorf("field: IsValid=%v, Kind=%v, want true, Interface", v.IsValid(), v.Kind());
    }
    v = v.Elem();
    if (v.IsValid() != false || v.Kind() != abi.Invalid) {
        t.Errorf("field elem: IsValid=%v, Kind=%v, want false, Invalid", v.IsValid(), v.Kind());
    }
}

partial struct TheNameOfThisTypeIsExactly255BytesLongSoWhenTheCompilerPrependsTheReflectTestPackageNameAndExtraStarTheLinkerRuntimeAndReflectPackagesWillHaveToCorrectlyDecodeTheSecondLengthByte0123456789_0123456789_0123456789_0123456789_0123456789_012345678 /*num:nint*/;

partial struct nameTest {
    internal any v;
    internal @string want;
}

partial struct A {
}

partial struct B<T> {
}


        partial interface Δtypeᴛ30 /*dyn*/ {
            void F();
        }
internal static slice<nameTest> nameTests = slice<nameTest>([
    new((ж<int32>)nil, "int32"),
    new((ж<D1>)nil, "D1"),
    new((ж<slice<D1>>)nil, ""),
    new((ж<channel<D1>>)nil, ""),
    new((ж<Func<D1>>)nil, ""),
    new((ж</*<-*/channel<D1>>)nil, ""),
    new((ж<channel/*<-*/<D1>>)nil, ""),
    new((ж<any>)nil, ""),
    new((ж<Δtypeᴛ30>)nil, ""),
    new((ж<TheNameOfThisTypeIsExactly255BytesLongSoWhenTheCompilerPrependsTheReflectTestPackageNameAndExtraStarTheLinkerRuntimeAndReflectPackagesWillHaveToCorrectlyDecodeTheSecondLengthByte0123456789_0123456789_0123456789_0123456789_0123456789_012345678>)nil, "TheNameOfThisTypeIsExactly255BytesLongSoWhenTheCompilerPrependsTheReflectTestPackageNameAndExtraStarTheLinkerRuntimeAndReflectPackagesWillHaveToCorrectlyDecodeTheSecondLengthByte0123456789_0123456789_0123456789_0123456789_0123456789_012345678"),
    new((ж<B<A>>)nil, "B[internal/reflectlite_test.A]"),
    new((ж<B<B<A>>>)nil, "B[internal/reflectlite_test.B[internal/reflectlite_test.A]]")
]);

public static void TestNames(ж<testing.T> t) {
    foreach (var (_, test) in nameTests) {
        var typ = TypeOf(test.v).Elem();
        {
            @string got = typ.Name(); if (got != test.want) {
                t.Errorf("%v Name()=%q, want %q", typ, got, test.want);
            }
        }
    }
}

internal partial struct TestUnaddressableField_localBuffer /*dyn*/ {
    internal slice<byte> buf;
}

// TestUnaddressableField tests that the reflect package will not allow
// a type from another package to be used as a named type with an
// unexported field.
//
// This ensures that unexported fields cannot be modified by other packages.
public static void TestUnaddressableField(ж<testing.T> t) {
    global::go.@internal.reflectlite_internal_test_package.Buffer b = default; // type defined in reflect, a different package
    ref var localBuffer = ref heap(new TestUnaddressableField_localBuffer(), out var ᏑlocalBuffer);
    ref var lv = ref heap<reflectlite.Value>(ValueOf(ᏑlocalBuffer).Elem(), out var Ꮡlv);
    ref var rv = ref heap<reflectlite.Value>(ValueOf(b), out var Ꮡrv);
    var lvʗ1 = lv;
    var rvʗ1 = rv;
    shouldPanic(() => {
        lvʗ1.Set(rvʗ1);
    });
}

partial struct Tint /*num:nint*/;

partial struct Talias1 {
    /*embed*/ internal byte @byte;
    /*embed*/ internal uint8 uint8;
    /*embed*/ internal nint @int;
    /*embed*/ internal int32 int32;
    /*embed*/ internal rune rune;
}

partial struct Talias2 {
    /*embed*/ public Tint Tint;
    /*embed*/ public Tint2 Tint2;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string reflectliteTestTalias1ˢ => ref reflectliteTestTalias1ˢᵇ;
private static readonly @string reflectliteTestTalias1ˢᵇ = "reflectlite_test.Talias1{byte:0x1, uint8:0x2, int:3, int32:4, rune:5}"u8;
internal static ref readonly @string reflectliteTestTalias2ˢ => ref reflectliteTestTalias2ˢᵇ;
private static readonly @string reflectliteTestTalias2ˢᵇ = "reflectlite_test.Talias2{Tint:1, Tint2:2}"u8;

public static void TestAliasNames(ж<testing.T> t) {
    var t1 = new Talias1(@byte: 1, uint8: 2, @int: 3, int32: 4, rune: 5);
    @string @out = fmt.Sprintf("%#v"u8, t1);
    @string want = reflectliteTestTalias1ˢ;
    if (@out != want) {
        t.Errorf("Talias1 print:\nhave: %s\nwant: %s", @out, want);
    }

    var t2 = new Talias2(Tint: 1, Tint2: 2);
    @out = fmt.Sprintf("%#v"u8, t2);
    want = reflectliteTestTalias2ˢ;
    if (@out != want) {
        t.Errorf("Talias2 print:\nhave: %s\nwant: %s", @out, want);
    }
}

} // end reflectlite_test_package
