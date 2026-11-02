// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
global using Loopy = object;

// gccgo used to fail this test.
global using TestStructOf_structFieldType = object;
global using Tint2 = go.reflect_test_package.Tint;

namespace go;

using bytes = bytes_package;
using base64 = encoding.base64_package;
using flag = flag_package;
using fmt = fmt_package;
using token = global::go.go.token_package;
using asan = @internal.asan_package;
using goarch = @internal.goarch_package;
using msan = @internal.msan_package;
using race = @internal.race_package;
using testenv = @internal.testenv_package;
using Δio = io_package;
using Δmath = math_package;
using rand = global::go.math.rand_package;
using Δnet = net_package;
using Δos = os_package;
using static reflect_package;
using example1 = global::go.reflect.@internal.example1_package;
using example2 = global::go.reflect.@internal.example2_package;
using Δruntime = runtime_package;
using debug = global::go.runtime.debug_package;
using slices = slices_package;
using strconv = strconv_package;
using strings = strings_package;
using Δsync = sync_package;
using atomic = global::go.sync.atomic_package;
using Δtesting = testing_package;
using time = time_package;
using @unsafe = unsafe_package;
using @internal;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using encoding;
using global::go.go;
using global::go.math;
using global::go.reflect.@internal;
using global::go.runtime;
using global::go.sync;
using iter = iter_package;
using static global::go.reflect_internal_test_package;
using Δreflect = reflect_package;
using ꓸꓸꓸPoint = Span<reflect_test_package.Point>;
using ꓸꓸꓸany = Span<any>;
using ꓸꓸꓸbyte = Span<byte>;
using ꓸꓸꓸnint = Span<nint>;

partial class reflect_test_package {

internal static any sink;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object valueOfTrueBoolFalseˢ = (@string)"ValueOf(true).Bool() = false"u8;

public static partial void TestBool(ж<Δtesting.T> t) {
    var v = ValueOf(true);
    if (v.Bool() != true) {
        t.Fatal(valueOfTrueBoolFalseˢ);
    }
}

partial struct integer /*num:nint*/;

partial struct T {
    internal nint a;
    internal float64 b;
    internal @string c;
    internal ж<nint> d;
}

internal static bool _ᴛ1ʗ = new T() == new T(); // tests depend on T being comparable

partial struct pair {
    internal any i;
    internal @string s;
}

internal static partial void assert(ж<Δtesting.T> t, @string s, @string want) {
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
    partial struct Δtypeᴛ18 /*dyn*/ {
        internal channel/*<-*/<channel<@string>> x = channel/*<-*/<channel<@string>>.SendOnly;
    }
    partial struct Δtypeᴛ19 /*dyn*/ {
        internal channel/*<-*/</*<-*/channel<@string>> x = channel/*<-*/</*<-*/channel<@string>>.Nil(ChanCargo.Of(new GoChanDir[] { GoChanDir.Send, GoChanDir.Recv }, null));
    }
    partial struct Δtypeᴛ20 /*dyn*/ {
        internal /*<-*/channel</*<-*/channel<@string>> x = /*<-*/channel</*<-*/channel<@string>>.Nil(ChanCargo.Of(new GoChanDir[] { GoChanDir.Recv, GoChanDir.Recv }, null));
    }
    partial struct Δtypeᴛ21 /*dyn*/ {
        internal channel</*<-*/channel<@string>> x = channel</*<-*/channel<@string>>.Nil(ChanCargo.Of(new GoChanDir[] { GoChanDir.Both, GoChanDir.Recv }, null));
    }
    partial struct typeᴛ22_x /*dyn*/ {
        internal channel<ж<int32>> c;
        internal float32 d;
    }

    partial struct Δtypeᴛ22 /*dyn*/ {
        internal typeᴛ22_x x;
    }
    partial struct Δtypeᴛ23 /*dyn*/ {
        internal Action<int8, int32> x;
    }
    partial struct typeᴛ24_x /*dyn*/ {
        internal Action<channel<ж<integer>>, ж<int8>> c;
    }

    partial struct Δtypeᴛ24 /*dyn*/ {
        internal typeᴛ24_x x;
    }
    partial struct typeᴛ25_x /*dyn*/ {
        internal int8 a;
        internal int32 b;
    }

    partial struct Δtypeᴛ25 /*dyn*/ {
        internal typeᴛ25_x x;
    }
    partial struct typeᴛ26_x /*dyn*/ {
        internal int8 a;
        internal int8 b;
        internal int32 c;
    }

    partial struct Δtypeᴛ26 /*dyn*/ {
        internal typeᴛ26_x x;
    }
    partial struct typeᴛ27_x /*dyn*/ {
        internal int8 a;
        internal int8 b;
        internal int8 c;
        internal int32 d;
    }

    partial struct Δtypeᴛ27 /*dyn*/ {
        internal typeᴛ27_x x;
    }
    partial struct typeᴛ28_x /*dyn*/ {
        internal int8 a;
        internal int8 b;
        internal int8 c;
        internal int8 d;
        internal int32 e;
    }

    partial struct Δtypeᴛ28 /*dyn*/ {
        internal typeᴛ28_x x;
    }
    partial struct typeᴛ29_x /*dyn*/ {
        internal int8 a;
        internal int8 b;
        internal int8 c;
        internal int8 d;
        internal int8 e;
        internal int32 f;
    }

    partial struct Δtypeᴛ29 /*dyn*/ {
        internal typeᴛ29_x x;
    }
    partial struct typeᴛ30_x /*dyn*/ {
        internal int8 a; /*`reflect:"hi there"`*/
    }

    partial struct Δtypeᴛ30 /*dyn*/ {
        internal typeᴛ30_x x;
    }
    partial struct typeᴛ31_x /*dyn*/ {
        internal int8 a; /*`reflect:"hi \x00there\t\n\"\\"`*/
    }

    partial struct Δtypeᴛ31 /*dyn*/ {
        internal typeᴛ31_x x;
    }
    partial struct typeᴛ32_x /*dyn*/ {
        internal Actionꓸꓸꓸ<nint> f;
    }

    partial struct Δtypeᴛ32 /*dyn*/ {
        internal typeᴛ32_x x;
    }
    partial interface typeᴛ33_x /*dyn*/ {
        void a(Func<Func<nint, nint>, Func<Action<nint>, nint>> _);
        void b();
    }

    partial struct Δtypeᴛ33 /*dyn*/ {
        internal typeᴛ33_x x;
    }
    partial struct typeᴛ34_x /*dyn*/ {
        /*embed*/ internal int32 int32;
        /*embed*/ internal int64 int64;
    }

    partial struct Δtypeᴛ34 /*dyn*/ {
        internal typeᴛ34_x x;
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
    new(new Δtypeᴛ13(), "**reflect_test.integer"),
    new(new Δtypeᴛ14(), "[32]int32"),
    new(new Δtypeᴛ15(), "[]int8"),
    new(new Δtypeᴛ16(), "map[string]int32"),
    new(new Δtypeᴛ17(), "chan<- string"),
    new(new Δtypeᴛ18(), "chan<- chan string"),
    new(new Δtypeᴛ19(), "chan<- <-chan string"),
    new(new Δtypeᴛ20(), "<-chan <-chan string"),
    new(new Δtypeᴛ21(), "chan (<-chan string)"),
    new(new Δtypeᴛ22(),
        "struct { c chan *int32; d float32 }"
    ),
    new(new Δtypeᴛ23(), "func(int8, int32)"),
    new(new Δtypeᴛ24(),
        "struct { c func(chan *reflect_test.integer, *int8) }"
    ),
    new(new Δtypeᴛ25(),
        "struct { a int8; b int32 }"
    ),
    new(new Δtypeᴛ26(),
        "struct { a int8; b int8; c int32 }"
    ),
    new(new Δtypeᴛ27(),
        "struct { a int8; b int8; c int8; d int32 }"
    ),
    new(new Δtypeᴛ28(),
        "struct { a int8; b int8; c int8; d int8; e int32 }"
    ),
    new(new Δtypeᴛ29(),
        "struct { a int8; b int8; c int8; d int8; e int8; f int32 }"
    ),
    new(new Δtypeᴛ30(),
        @"struct { a int8 ""reflect:\""hi there\"""" }"
    ),
    new(new Δtypeᴛ31(),
        @"struct { a int8 ""reflect:\""hi \\x00there\\t\\n\\\""\\\\\"""" }"
    ),
    new(new Δtypeᴛ32(),
        "struct { f func(...int) }"
    ),
    new(new Δtypeᴛ33(),
        "interface { reflect_test.a(func(func(int) int) func(func(int)) int); reflect_test.b() }"
    ),
    new(new Δtypeᴛ34(),
        "struct { int32; int64 }"
    )
]);

internal static readonly ж<slice<pair>> ᏑvalueTests = new StaticBox<slice<pair>>(slice<pair>([
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
    new(@new<ж<ж<integer>>>(), "**reflect_test.integer(0)"),
    new(@new<map<@string, int32>>(), "map[string]int32{<can't iterate on maps>}"),
    new(Ꮡ(channel/*<-*/<@string>.SendOnly), "chan<- string"),
    new(@new<Action<int8, int32>>(), "func(int8, int32)(0)"),
    new(@new<typeᴛ22_x>(),
        "struct { c chan *int32; d float32 }{chan *int32, 0}"
    ),
    new(@new<typeᴛ24_x>(),
        "struct { c func(chan *reflect_test.integer, *int8) }{func(chan *reflect_test.integer, *int8)(0)}"
    ),
    new(@new<typeᴛ25_x>(),
        "struct { a int8; b int32 }{0, 0}"
    ),
    new(@new<typeᴛ26_x>(),
        "struct { a int8; b int8; c int32 }{0, 0, 0}"
    )
]));
internal static ref slice<pair> valueTests => ref ((StaticBox<slice<pair>>)ᏑvalueTests).ValueSlot;

internal static partial void testType(ж<Δtesting.T> t, nint i, reflectꓸType typ, @string want) {
    @string s = typ.String();
    if (s != want) {
        t.Errorf("#%d: have %#q, want %#q", i, s, want);
    }
}

public static void TestTypes(ж<Δtesting.T> t) {
    foreach (var (i, tt) in typeTests) {
        testType(t, i, ValueOf(tt.i).Field(0).Type(), tt.s);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string stringyCheeseˢ => ref stringyCheeseˢᵇ;
private static readonly @string stringyCheeseˢᵇ = "stringy cheese"u8;

public static void TestSet(ж<Δtesting.T> t) {
    foreach (var (i, tt) in valueTests) {
        var v = ValueOf(tt.i);
        v = v.Elem();
        switch (v.Kind()) {
        case var ᴛ when ᴛ == ΔInt: {
            v.SetInt(132);
            break;
        }
        case var ᴛ when ᴛ == Int8: {
            v.SetInt(8);
            break;
        }
        case var ᴛ when ᴛ == Int16: {
            v.SetInt(16);
            break;
        }
        case var ᴛ when ᴛ == Int32: {
            v.SetInt(32);
            break;
        }
        case var ᴛ when ᴛ == Int64: {
            v.SetInt(64);
            break;
        }
        case var ᴛ when ᴛ == ΔUint: {
            v.SetUint(132);
            break;
        }
        case var ᴛ when ᴛ == Uint8: {
            v.SetUint(8);
            break;
        }
        case var ᴛ when ᴛ == Uint16: {
            v.SetUint(16);
            break;
        }
        case var ᴛ when ᴛ == Uint32: {
            v.SetUint(32);
            break;
        }
        case var ᴛ when ᴛ == Uint64: {
            v.SetUint(64);
            break;
        }
        case var ᴛ when ᴛ == Float32: {
            v.SetFloat(256.25D);
            break;
        }
        case var ᴛ when ᴛ == Float64: {
            v.SetFloat(512.125D);
            break;
        }
        case var ᴛ when ᴛ == Complex64: {
            v.SetComplex(532.125D + 10D.i());
            break;
        }
        case var ᴛ when ᴛ == Complex128: {
            v.SetComplex(564.25D + 1D.i());
            break;
        }
        case var ᴛ when ᴛ == ΔString: {
            v.SetString(stringyCheeseˢ);
            break;
        }
        case var ᴛ when ᴛ == ΔBool: {
            v.SetBool(true);
            break;
        }}
        @string s = valueToString(v);
        if (s != tt.s) {
            t.Errorf("#%d: have %#q, want %#q", i, s, tt.s);
        }
    }
}

public static void TestSetValue(ж<Δtesting.T> t) {
    foreach (var (i, tt) in valueTests) {
        var v = ValueOf(tt.i).Elem();
        switch (v.Kind()) {
        case var ᴛ when ᴛ == ΔInt: {
            v.Set(ValueOf((nint)132));
            break;
        }
        case var ᴛ when ᴛ == Int8: {
            v.Set(ValueOf((int8)8));
            break;
        }
        case var ᴛ when ᴛ == Int16: {
            v.Set(ValueOf((int16)16));
            break;
        }
        case var ᴛ when ᴛ == Int32: {
            v.Set(ValueOf((int32)32));
            break;
        }
        case var ᴛ when ᴛ == Int64: {
            v.Set(ValueOf((int64)64));
            break;
        }
        case var ᴛ when ᴛ == ΔUint: {
            v.Set(ValueOf((nuint)132));
            break;
        }
        case var ᴛ when ᴛ == Uint8: {
            v.Set(ValueOf((uint8)8));
            break;
        }
        case var ᴛ when ᴛ == Uint16: {
            v.Set(ValueOf((uint16)16));
            break;
        }
        case var ᴛ when ᴛ == Uint32: {
            v.Set(ValueOf((uint32)32));
            break;
        }
        case var ᴛ when ᴛ == Uint64: {
            v.Set(ValueOf((uint64)64));
            break;
        }
        case var ᴛ when ᴛ == Float32: {
            v.Set(ValueOf((float32)256.25F));
            break;
        }
        case var ᴛ when ᴛ == Float64: {
            v.Set(ValueOf(512.125D));
            break;
        }
        case var ᴛ when ᴛ == Complex64: {
            v.Set(ValueOf((complex64)(532.125F + 10F.i())));
            break;
        }
        case var ᴛ when ᴛ == Complex128: {
            v.Set(ValueOf((complex128)(564.25D + 1D.i())));
            break;
        }
        case var ᴛ when ᴛ == ΔString: {
            v.Set(ValueOf(stringyCheeseˢ));
            break;
        }
        case var ᴛ when ᴛ == ΔBool: {
            v.Set(ValueOf(true));
            break;
        }}
        @string s = valueToString(v);
        if (s != tt.s) {
            t.Errorf("#%d: have %#q, want %#q", i, s, tt.s);
        }
    }
}

public static void TestMapIterSet(ж<Δtesting.T> t) {
    var m = new map<@string, any>(len(valueTests));
    foreach (var (_, tt) in valueTests) {
        m[tt.s] = tt.i;
    }
    ref var v = ref heap<reflectꓸValue>(ValueOf(m), out var Ꮡv);

    ref var k = ref heap<reflectꓸValue>(New(v.Type().Key()).Elem(), out var Ꮡk);
    ref var e = ref heap<reflectꓸValue>(New(v.Type().Elem()).Elem(), out var Ꮡe);

    var iter = v.MapRange();
    while (iter.Next()) {
        k.SetIterKey(iter);
        e.SetIterValue(iter);
        var wantΔ1 = m[k.String()];
        var gotΔ1 = e.Interface();
        if (!AreEqual(gotΔ1, wantΔ1)) {
            t.Errorf("%q: want (%T) %v, got (%T) %v", k.String(), wantΔ1, wantΔ1, gotΔ1, gotΔ1);
        }
        {
            @string setkey = valueToString(k);
            @string key = valueToString(iter.Key()); if (setkey != key) {
                t.Errorf("MapIter.Key() = %q, MapIter.SetKey() = %q", key, setkey);
            }
        }
        {
            @string setval = valueToString(e);
            @string val = valueToString(iter.Value()); if (setval != val) {
                t.Errorf("MapIter.Value() = %q, MapIter.SetValue() = %q", val, setval);
            }
        }
    }

    if (testenv.OptimizationOff()) {
        return; // no inlining with the noopt builder
    }

    var eʗ1 = e;
    var kʗ1 = k;
    var vʗ1 = v;
    nint got = (nint)Δtesting.AllocsPerRun(10, () => {
        var iterΔ1 = vʗ1.MapRange();
        while (iterΔ1.Next()) {
            kʗ1.SetIterKey(iterΔ1);
            eʗ1.SetIterValue(iterΔ1);
        }
    });
    // Calling MapRange should not allocate even though it returns a *MapIter.
    // The function is inlineable, so if the local usage does not escape
    // the *MapIter, it can remain stack allocated.
    nint want = 0;
    if (got != want) {
        t.Errorf("wanted %d alloc, got %d", want, got);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string helloWorldˢ => ref helloWorldˢᵇ;
private static readonly @string helloWorldˢᵇ = "hello world"u8;

internal partial struct TestCanIntUintFloatComplex_integer /*num:nint*/;

internal partial struct TestCanIntUintFloatComplex_uinteger /*num:nuint*/;

internal partial struct TestCanIntUintFloatComplex_float /*num:float64*/;

internal partial struct TestCanIntUintFloatComplex_complex /*num:complex128*/;

internal partial struct TestCanIntUintFloatComplex_type /*dyn*/ {
    internal any i;
    internal array<bool> want = new(4);
}

internal partial struct TestCanIntUintFloatComplex_typeᴛ1 /*dyn*/ {
    internal nint i;
}

public static void TestCanIntUintFloatComplex(ж<Δtesting.T> t) {
    array<@string> ops = new @string[]{"CanInt", "CanUint", "CanFloat", "CanComplex"}.array();

    slice<TestCanIntUintFloatComplex_type> testCases = slice<TestCanIntUintFloatComplex_type>([
        // signed integer
        new((nint)132, new bool[]{true, false, false, false}.array()),
        new((int8)8, new bool[]{true, false, false, false}.array()),
        new((int16)16, new bool[]{true, false, false, false}.array()),
        new((int32)32, new bool[]{true, false, false, false}.array()),
        new((int64)64, new bool[]{true, false, false, false}.array()),
        // unsigned integer
        new((nuint)132, new bool[]{false, true, false, false}.array()),
        new((uint8)8, new bool[]{false, true, false, false}.array()),
        new((uint16)16, new bool[]{false, true, false, false}.array()),
        new((uint32)32, new bool[]{false, true, false, false}.array()),
        new((uint64)64, new bool[]{false, true, false, false}.array()),
        new((uintptr)0xABCD, new bool[]{false, true, false, false}.array()),
        // floating-point
        new((float32)256.25F, new bool[]{false, false, true, false}.array()),
        new((float64)512.125D, new bool[]{false, false, true, false}.array()),
        // complex
        new((complex64)(532.125F + 10F.i()), new bool[]{false, false, false, true}.array()),
        new((complex128)(564.25D + 1D.i()), new bool[]{false, false, false, true}.array()),
        // underlying
        new((TestCanIntUintFloatComplex_integer)(-132), new bool[]{true, false, false, false}.array()),
        new((TestCanIntUintFloatComplex_uinteger)132, new bool[]{false, true, false, false}.array()),
        new((TestCanIntUintFloatComplex_float)256.25D, new bool[]{false, false, true, false}.array()),
        new((TestCanIntUintFloatComplex_complex)(532.125D + 10D.i()), new bool[]{false, false, false, true}.array()),
        // not-acceptable
        new(helloWorldˢ, new bool[]{false, false, false, false}.array()),
        new(@new<nint>(), new bool[]{false, false, false, false}.array()),
        new(@new<nuint>(), new bool[]{false, false, false, false}.array()),
        new(@new<float64>(), new bool[]{false, false, false, false}.array()),
        new(@new<complex64>(), new bool[]{false, false, false, false}.array()),
        new(Ꮡ(new array<nint>(5)), new bool[]{false, false, false, false}.array()),
        new(@new<TestCanIntUintFloatComplex_integer>(), new bool[]{false, false, false, false}.array()),
        new(@new<map<nint, nint>>(), new bool[]{false, false, false, false}.array()),
        new(Ꮡ(channel/*<-*/<nint>.SendOnly), new bool[]{false, false, false, false}.array()),
        new(@new<Action<int8>>(), new bool[]{false, false, false, false}.array()),
        new(@new<TestCanIntUintFloatComplex_typeᴛ1>(), new bool[]{false, false, false, false}.array())
    ]);

    foreach (var (i, vᴛ1) in testCases) {
        var tc = vᴛ1.ΔClone();

        var v = ValueOf(tc.i);
        var got = new bool[]{v.CanInt(), v.CanUint(), v.CanFloat(), v.CanComplex()}.array();

        foreach (var j in range(tc.want)) {
            if (got[j] != tc.want[j]) {
                t.Errorf(
                    "#%d: v.%s() returned %t for type %T, want %t",
                    i,
                    ops[j],
                    got[j],
                    tc.i,
                    tc.want[j]);
            }
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
    // -1 means Addr().Elem() of current value
    internal slice<nint> index;
    internal bool canSet;
}

internal partial struct TestCanSetField_tests /*dyn*/ {
    internal reflectꓸValue val;
    internal slice<TestCanSetField_testCase> cases;
}

public static void TestCanSetField(ж<Δtesting.T> t) {
    var tests = slice<TestCanSetField_tests>([new(
        val: ValueOf(Ꮡ(new TestCanSetField_S1())),
        cases: slice<TestCanSetField_testCase>([
            new(slice<nint>([0]), false),
            new(slice<nint>([0, -1]), false),
            new(slice<nint>([0, 0]), false),
            new(slice<nint>([0, 0, -1]), false),
            new(slice<nint>([0, -1, 0]), false),
            new(slice<nint>([0, -1, 0, -1]), false),
            new(slice<nint>([0, 1]), true),
            new(slice<nint>([0, 1, -1]), true),
            new(slice<nint>([0, -1, 1]), true),
            new(slice<nint>([0, -1, 1, -1]), true),
            new(slice<nint>([1]), false),
            new(slice<nint>([1, -1]), false),
            new(slice<nint>([2]), true),
            new(slice<nint>([2, -1]), true)
        ])
    ), new(
        val: ValueOf(Ꮡ(new TestCanSetField_S2(embed: Ꮡ(new TestCanSetField_embed())))),
        cases: slice<TestCanSetField_testCase>([
            new(slice<nint>([0]), false),
            new(slice<nint>([0, -1]), false),
            new(slice<nint>([0, 0]), false),
            new(slice<nint>([0, 0, -1]), false),
            new(slice<nint>([0, -1, 0]), false),
            new(slice<nint>([0, -1, 0, -1]), false),
            new(slice<nint>([0, 1]), true),
            new(slice<nint>([0, 1, -1]), true),
            new(slice<nint>([0, -1, 1]), true),
            new(slice<nint>([0, -1, 1, -1]), true),
            new(slice<nint>([1]), false),
            new(slice<nint>([2]), true)
        ])
    ), new(
        val: ValueOf(Ꮡ(new TestCanSetField_S3())),
        cases: slice<TestCanSetField_testCase>([
            new(slice<nint>([0]), true),
            new(slice<nint>([0, -1]), true),
            new(slice<nint>([0, 0]), false),
            new(slice<nint>([0, 0, -1]), false),
            new(slice<nint>([0, -1, 0]), false),
            new(slice<nint>([0, -1, 0, -1]), false),
            new(slice<nint>([0, 1]), true),
            new(slice<nint>([0, 1, -1]), true),
            new(slice<nint>([0, -1, 1]), true),
            new(slice<nint>([0, -1, 1, -1]), true),
            new(slice<nint>([1]), false),
            new(slice<nint>([2]), true)
        ])
    ), new(
        val: ValueOf(Ꮡ(new TestCanSetField_S4(Embed: Ꮡ(new TestCanSetField_Embed())))),
        cases: slice<TestCanSetField_testCase>([
            new(slice<nint>([0]), true),
            new(slice<nint>([0, -1]), true),
            new(slice<nint>([0, 0]), false),
            new(slice<nint>([0, 0, -1]), false),
            new(slice<nint>([0, -1, 0]), false),
            new(slice<nint>([0, -1, 0, -1]), false),
            new(slice<nint>([0, 1]), true),
            new(slice<nint>([0, 1, -1]), true),
            new(slice<nint>([0, -1, 1]), true),
            new(slice<nint>([0, -1, 1, -1]), true),
            new(slice<nint>([1]), false),
            new(slice<nint>([2]), true)
        ])
    )
    ]);

    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestCanSetField_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(tt.val.Type().Name(), (ж<Δtesting.T> tΔ1) => {
            foreach (var (_, tc) in ttʗ1.cases) {
                var f = ttʗ1.val;
                foreach (var (_, i) in tc.index) {
                    if (f.Kind() == ΔPointer) {
                        f = f.Elem();
                    }
                    if (i == -1) {
                        f = f.Addr().Elem();
                    } else {
                        f = f.Field(i);
                    }
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
    new(new T(123, 456.75D, "hello", Ꮡ_i), "reflect_test.T{123, 456.75, hello, *int(&7)}"),
    new(@new<channel<ж<T>>>(), "*chan *reflect_test.T(&chan *reflect_test.T)"),
    new(new nint[]{1, 2, 3, 4, 5, 6, 7, 8, 9, 10}.array(), "[10]int{1, 2, 3, 4, 5, 6, 7, 8, 9, 10}"),
    new(Ꮡ(new nint[]{1, 2, 3, 4, 5, 6, 7, 8, 9, 10}.array()), "*[10]int(&[10]int{1, 2, 3, 4, 5, 6, 7, 8, 9, 10})"),
    new(slice<nint>([1, 2, 3, 4, 5, 6, 7, 8, 9, 10]), "[]int{1, 2, 3, 4, 5, 6, 7, 8, 9, 10}"),
    new(Ꮡ(slice<nint>([1, 2, 3, 4, 5, 6, 7, 8, 9, 10])), "*[]int(&[]int{1, 2, 3, 4, 5, 6, 7, 8, 9, 10})")
]);

public static void TestValueToString(ж<Δtesting.T> t) {
    foreach (var (i, test) in valueToStringTests) {
        @string s = valueToString(ValueOf(test.i));
        if (s != test.s) {
            t.Errorf("#%d: have %#q, want %#q", i, s, test.s);
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string wantᶜ => ref wantᶜᵇ;
private static readonly @string wantᶜᵇ = "[10]int{1, 2, 3, 4, 123, 6, 7, 8, 9, 10}"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string want1ᶜ => ref want1ᶜᵇ;
private static readonly @string want1ᶜᵇ = "[]int{1, 2, 3, 4, 123, 6, 7, 8, 9, 10}"u8;

public static void TestArrayElemSet(ж<Δtesting.T> t) {
    var v = ValueOf(Ꮡ(new nint[]{1, 2, 3, 4, 5, 6, 7, 8, 9, 10}.array())).Elem();
    v.Index(4).SetInt(123);
    @string s = valueToString(v);
    @string want = wantᶜ;
    if (s != want) {
        t.Errorf("[10]int: have %#q want %#q", s, want);
    }

    v = ValueOf(slice<nint>([1, 2, 3, 4, 5, 6, 7, 8, 9, 10]));
    v.Index(4).SetInt(123);
    s = valueToString(v);
    @string want1 = want1ᶜ;
    if (s != want1) {
        t.Errorf("[]int: have %#q want %#q", s, want1);
    }
}

public static void TestPtrPointTo(ж<Δtesting.T> t) {
    ref var ip = ref heap<ж<int32>>(out var Ꮡip);
    ref var i = ref heap(new int32(), out var Ꮡi);
    i = 1234;
    var vip = ValueOf(Ꮡip);
    var vi = ValueOf(Ꮡi).Elem();
    vip.Elem().Set(vi.Addr());
    if (ip.Value != 1234) {
        t.Errorf("got %d, want 1234", ip.Value);
    }

    ip = default;
    var vp = ValueOf(Ꮡip).Elem();
    vp.Set(Zero(vp.Type()));
    if (ip != nil) {
        t.Errorf("got non-nil (%p), want nil", ip.OrTypedNil());
    }
}

public static void TestPtrSetNil(ж<Δtesting.T> t) {
    ref var i = ref heap(new int32(), out var Ꮡi);
    i = 1234;
    ref var ip = ref heap<ж<int32>>(Ꮡi, out var Ꮡip);
    var vip = ValueOf(Ꮡip);
    vip.Elem().Set(Zero(vip.Elem().Type()));
    if (ip != nil) {
        t.Errorf("got non-nil (%d), want nil", ip.Value);
    }
}

public static void TestMapSetNil(ж<Δtesting.T> t) {
    ref var m = ref heap<map<@string, nint>>(new map<@string, nint>(), out var Ꮡm);
    var vm = ValueOf(Ꮡm);
    vm.Elem().Set(Zero(vm.Elem().Type()));
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
internal static ref readonly @string chanInt32ˢ => ref chanInt32ˢᵇ;
private static readonly @string chanInt32ˢᵇ = "chan *int32"u8;
internal static ref readonly @string float32ˢ => ref float32ˢᵇ;
private static readonly @string float32ˢᵇ = "float32"u8;
internal static ref readonly @string absentˢ => ref absentˢᵇ;
private static readonly @string absentˢᵇ = "absent"u8;
internal static ref readonly @string int32ˢ => ref int32ˢᵇ;
private static readonly @string int32ˢᵇ = "[32]int32"u8;
internal static ref readonly @string int32ˢ2 => ref int32ˢ2ᵇ;
private static readonly @string int32ˢ2ᵇ = "int32"u8;
internal static ref readonly @string mapStringInt32ˢ => ref mapStringInt32ˢᵇ;
private static readonly @string mapStringInt32ˢᵇ = "map[string]*int32"u8;
internal static ref readonly @string stringˢ => ref stringˢᵇ;
private static readonly @string stringˢᵇ = "string"u8;
internal static ref readonly @string int32ˢ3 => ref int32ˢ3ᵇ;
private static readonly @string int32ˢ3ᵇ = "*int32"u8;
internal static ref readonly @string chanStringˢ => ref chanStringˢᵇ;
private static readonly @string chanStringˢᵇ = "chan<- string"u8;
internal static ref readonly @string uint32ˢ => ref uint32ˢᵇ;
private static readonly @string uint32ˢᵇ = "[]uint32"u8;

internal partial struct TestAll_i /*dyn*/ {
    internal slice<uint32> d; /*`reflect:"TAG"`*/
}

public static void TestAll(ж<Δtesting.T> t) {
    testType(t, 1, TypeOf((int8)0), int8ˢ);
    testType(t, 2, TypeOf((ж<int8>)nil).Elem(), int8ˢ);

    var typ = TypeOf((ж<typeᴛ22_x>)nil);
    testType(t, 3, typ, structCChanInt32DFloat32ˢ);
    var etyp = typ.Elem();
    testType(t, 4, etyp, structCChanInt32DFloat32ˢ2);
    var styp = etyp;
    var f = styp.Field(0);
    testType(t, 5, f.Type, chanInt32ˢ);

    (f, var present) = styp.FieldByName("d");
    if (!present) {
        t.Errorf("FieldByName says present field is absent");
    }
    testType(t, 6, f.Type, float32ˢ);

    (f, present) = styp.FieldByName(absentˢ);
    if (present) {
        t.Errorf("FieldByName says absent field is present");
    }

    typ = TypeOf(new int32[]{}.array(32));
    testType(t, 7, typ, int32ˢ);
    testType(t, 8, typ.Elem(), int32ˢ2);

    typ = TypeOf((map<@string, ж<int32>>)default);
    testType(t, 9, typ, mapStringInt32ˢ);
    var mtyp = typ;
    testType(t, 10, mtyp.Key(), stringˢ);
    testType(t, 11, mtyp.Elem(), int32ˢ3);

    typ = TypeOf(channel/*<-*/<@string>.SendOnly);
    testType(t, 12, typ, chanStringˢ);
    testType(t, 13, typ.Elem(), stringˢ);

    // make sure tag strings are not part of element type
    typ = TypeOf(new TestAll_i()).Field(0).Type;
    testType(t, 14, typ, uint32ˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string interfaceˢ => ref interfaceˢᵇ;
private static readonly @string interfaceˢᵇ = "interface {}"u8;
internal static ref readonly @string float64ˢ => ref float64ˢᵇ;
private static readonly @string float64ˢᵇ = "float64"u8;

internal partial struct TestInterfaceGet_inter /*dyn*/ {
    public any E;
}

public static void TestInterfaceGet(ж<Δtesting.T> t) {
    ref var inter = ref heap(new TestInterfaceGet_inter(), out var Ꮡinter);
    inter.E = 123.456D;
    var v1 = ValueOf(Ꮡinter);
    var v2 = v1.Elem().Field(0);
    assert(t, v2.Type().String(), interfaceˢ);
    var i2 = v2.Interface();
    var v3 = ValueOf(i2);
    assert(t, v3.Type().String(), float64ˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object v2InterfaceDidNotReturnˢ = (@string)"v2.Interface() did not return float64, got "u8;

internal partial struct TestInterfaceValue_inter /*dyn*/ {
    public any E;
}

public static void TestInterfaceValue(ж<Δtesting.T> t) {
    ref var inter = ref heap(new TestInterfaceValue_inter(), out var Ꮡinter);
    inter.E = 123.456D;
    var v1 = ValueOf(Ꮡinter);
    var v2 = v1.Elem().Field(0);
    assert(t, v2.Type().String(), interfaceˢ);
    var v3 = v2.Elem();
    assert(t, v3.Type().String(), float64ˢ);

    var i3 = v2.Interface();
    {
        var (_, ok) = i3._<float64>(ᐧ); if (!ok) {
            t.Error(v2InterfaceDidNotReturnˢ, TypeOf(i3));
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string funcˢ => ref funcˢᵇ;
private static readonly @string funcˢᵇ = "func()"u8;

public static void TestFunctionValue(ж<Δtesting.T> t) {
    any x = () => {
    };
    var v = ValueOf(x);
    if (fmt.Sprint(v.Interface()) != fmt.Sprint(x)) {
        t.Fatalf("TestFunction returned wrong pointer");
    }
    assert(t, v.Type().String(), funcˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string reflectValueGrowUsingˢ => ref reflectValueGrowUsingˢᵇ;
private static readonly @string reflectValueGrowUsingˢᵇ = "reflect.Value.Grow using unaddressable value"u8;
internal static ref readonly @string appendˢ => ref appendˢᵇ;
private static readonly @string appendˢᵇ = "Append"u8;
internal static ref readonly @string rateˢ => ref rateˢᵇ;
private static readonly @string rateˢᵇ = "Rate"u8;
internal static ref readonly @string zeroCapacityˢ => ref zeroCapacityˢᵇ;
private static readonly @string zeroCapacityˢᵇ = "ZeroCapacity"u8;

public static void TestGrow(ж<Δtesting.T> t) {
    ref var v = ref heap<reflectꓸValue>(ValueOf(slice<nint>(default)), out var Ꮡv);
    shouldPanic(reflectValueGrowUsingˢ, () => {
        Ꮡv.Value.Grow(0);
    });
    v = ValueOf(@new<slice<nint>>()).Elem();
    v.Grow(0);
    if (!v.IsNil()) {
        t.Errorf("v.Grow(0) should still be nil");
    }
    v.Grow(1);
    if (v.Cap() == 0) {
        t.Errorf("v.Cap = %v, want non-zero", v.Cap());
    }
    @unsafe.Pointer want = (uintptr)v.UnsafePointer();
    v.Grow(1);
    @unsafe.Pointer got = (uintptr)v.UnsafePointer();
    if (got != want) {
        t.Errorf("noop v.Grow should not change pointers");
    }

    t.Run(appendˢ, (ж<Δtesting.T> tΔ1) => {
        ref var gotΔ1 = ref heap<slice<T>>(out var ᏑgotΔ1);
        slice<T> wantΔ1 = default;
        ref var vΔ1 = ref heap<reflectꓸValue>(ValueOf(ᏑgotΔ1).Elem(), out var ᏑvΔ1);
        var vʗ1 = vΔ1;
        void appendValue(T vt) {
            vʗ1.Grow(1);
            vʗ1.SetLen(vʗ1.Len() + 1);
            vʗ1.Index(vʗ1.Len() - 1).Set(ValueOf(vt));
        }
        for (nint iᴛ1 = 0; iᴛ1 < 10; iᴛ1++) {
            ref var i = ref heap<nint>(out var Ꮡi);
            i = iᴛ1;
            var vt = new T(i, (float64)i, strconv.Itoa(i), Ꮡi);
            appendValue(vt);
            wantΔ1 = append(wantΔ1, vt);
            iᴛ1 = i;
        }
        if (!DeepEqual(ᏑgotΔ1.ValueSlot, wantΔ1)) {
            tΔ1.Errorf("value mismatch:\ngot  %v\nwant %v", ᏑgotΔ1.ValueSlot, wantΔ1);
        }
    });

    t.Run(rateˢ, (ж<Δtesting.T> tΔ2) => {
        slice<byte> b = default;
        var vΔ2 = ValueOf(@new<slice<byte>>()).Elem();
        for (nint i = 0; i < 10; i++) {
            b = appendꓸꓸꓸ(b.slice(0, cap(b)), makeꓸꓸꓸ<byte>(1));
            vΔ2.SetLen(vΔ2.Cap());
            vΔ2.Grow(1);
            if (vΔ2.Cap() != cap(b)) {
                tΔ2.Errorf("v.Cap = %v, want %v", vΔ2.Cap(), cap(b));
            }
        }
    });

    t.Run(zeroCapacityˢ, (ж<Δtesting.T> tΔ3) => {
        for (nint i = 0; i < 10; i++) {
            var vΔ3 = ValueOf(@new<slice<byte>>()).Elem();
            vΔ3.Grow(61);
            var b = vΔ3.Bytes();
            b = b.slice(0, cap(b));
            foreach (var (iΔ1, c) in b) {
                if (c != 0) {
                    tΔ3.Fatalf("Value.Bytes[%d] = 0x%02x, want 0x00", iΔ1, c);
                }
                b[iΔ1] = 0xff;
            }
            Δruntime.GC();
        }
    });
}

partial struct appendTestsᴛ1 /*dyn*/ {
    internal slice<nint> orig, extra;
}
internal static slice<appendTestsᴛ1> appendTests = slice<appendTestsᴛ1>([
    new(default, default),
    new(slice<nint>([]), default),
    new(default, slice<nint>([])),
    new(slice<nint>([]), slice<nint>([])),
    new(default, slice<nint>([22])),
    new(slice<nint>([]), slice<nint>([22])),
    new(new slice<nint>(2, 4), default),
    new(new slice<nint>(2, 4), slice<nint>([])),
    new(new slice<nint>(2, 4), slice<nint>([22])),
    new(new slice<nint>(2, 4), slice<nint>([22, 33, 44]))
]);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string usingUnexportedFieldˢ => ref usingUnexportedFieldˢᵇ;
private static readonly @string usingUnexportedFieldˢᵇ = "using unexported field"u8;

internal partial struct TestAppend_i /*dyn*/ {
    internal slice<nint> x;
}

public static void TestAppend(ж<Δtesting.T> t) {
    foreach (var (i, vᴛ1) in appendTests) {
        ref var test = ref heap(new appendTestsᴛ1(), out var Ꮡtest);
        test = vᴛ1;

        nint origLen = len(test.orig);
        nint extraLen = len(test.extra);
        var want = appendꓸꓸꓸ(test.orig, test.extra);
        // Convert extra from []int to []Value.
        var e0 = new slice<reflectꓸValue>(len(test.extra), () => new(nil));
        foreach (var (j, e) in test.extra) {
            e0[j] = ValueOf(e);
        }
        // Convert extra from []int to *SliceValue.
        ref var e1 = ref heap<reflectꓸValue>(ValueOf(test.extra), out var Ꮡe1);

        // Test Append.
        var a0 = ValueOf(Ꮡtest.of(appendTestsᴛ1.Ꮡorig)).Elem();
        var have0 = Append(a0, e0.ꓸꓸꓸ);
        if (have0.CanAddr()) {
            t.Errorf("Append #%d: have slice should not be addressable", i);
        }
        if (!DeepEqual(have0.Interface(), want)) {
            t.Errorf("Append #%d: have %v, want %v (%p %p)", i, have0, want, test.orig, have0.Interface());
        }
        // Check that the orig and extra slices were not modified.
        if (a0.Len() != len(test.orig)) {
            t.Errorf("Append #%d: a0.Len: have %d, want %d", i, a0.Len(), origLen);
        }
        if (len(test.orig) != origLen) {
            t.Errorf("Append #%d origLen: have %v, want %v", i, len(test.orig), origLen);
        }
        if (len(test.extra) != extraLen) {
            t.Errorf("Append #%d extraLen: have %v, want %v", i, len(test.extra), extraLen);
        }

        // Test AppendSlice.
        var a1 = ValueOf(Ꮡtest.of(appendTestsᴛ1.Ꮡorig)).Elem();
        var have1 = AppendSlice(a1, e1);
        if (have1.CanAddr()) {
            t.Errorf("AppendSlice #%d: have slice should not be addressable", i);
        }
        if (!DeepEqual(have1.Interface(), want)) {
            t.Errorf("AppendSlice #%d: have %v, want %v", i, have1, want);
        }
        // Check that the orig and extra slices were not modified.
        if (a1.Len() != len(test.orig)) {
            t.Errorf("AppendSlice #%d: a1.Len: have %d, want %d", i, a0.Len(), origLen);
        }
        if (len(test.orig) != origLen) {
            t.Errorf("AppendSlice #%d origLen: have %v, want %v", i, len(test.orig), origLen);
        }
        if (len(test.extra) != extraLen) {
            t.Errorf("AppendSlice #%d extraLen: have %v, want %v", i, len(test.extra), extraLen);
        }

        // Test Append and AppendSlice with unexported value.
        ref var ax = ref heap<reflectꓸValue>(ValueOf(new TestAppend_i(test.orig)).Field(0), out var Ꮡax);
        var axʗ1 = ax;
        var e0ʗ1 = e0;
        shouldPanic(usingUnexportedFieldˢ, () => {
            Append(axʗ1, e0ʗ1.ꓸꓸꓸ);
        });
        var axʗ2 = ax;
        var e1ʗ1 = e1;
        shouldPanic(usingUnexportedFieldˢ, () => {
            AppendSlice(axʗ2, e1ʗ1);
        });
    }
}

public static void TestCopy(ж<Δtesting.T> t) {
    var a = slice<nint>([1, 2, 3, 4, 10, 9, 8, 7]);
    var b = slice<nint>([11, 22, 33, 44, 1010, 99, 88, 77, 66, 55, 44]);
    var c = slice<nint>([11, 22, 33, 44, 1010, 99, 88, 77, 66, 55, 44]);
    for (nint i = 0; i < len(b); i++) {
        if (b[i] != c[i]) {
            t.Fatalf("b != c before test");
        }
    }
    ref var a1 = ref heap<slice<nint>>(a, out var Ꮡa1);
    ref var b1 = ref heap<slice<nint>>(b, out var Ꮡb1);
    var aa = ValueOf(Ꮡa1).Elem();
    var ab = ValueOf(Ꮡb1).Elem();
    for (nint tocopy = 1; tocopy <= 7; tocopy++) {
        aa.SetLen(tocopy);
        Copy(ab, aa);
        aa.SetLen(8);
        for (nint i = 0; i < tocopy; i++) {
            if (a[i] != b[i]) {
                t.Errorf("(i) tocopy=%d a[%d]=%d, b[%d]=%d",
                    tocopy, i, a[i], i, b[i]);
            }
        }
        for (nint i = tocopy; i < len(b); i++) {
            if (b[i] != c[i]) {
                if (i < len(a)) {
                    t.Errorf("(ii) tocopy=%d a[%d]=%d, b[%d]=%d, c[%d]=%d",
                        tocopy, i, a[i], i, b[i], i, c[i]);
                } else {
                    t.Errorf("(iii) tocopy=%d b[%d]=%d, c[%d]=%d",
                        tocopy, i, b[i], i, c[i]);
                }
            } else {
                t.Logf("tocopy=%d elem %d is okay\n", tocopy, i);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string sliceˢ => ref sliceˢᵇ;
private static readonly @string sliceˢᵇ = "Slice"u8;
internal static ref readonly @string helloˢ => ref helloˢᵇ;
private static readonly @string helloˢᵇ = "hello"u8;
internal static readonly object helloworldˢ = (@string)"helloworld"u8;
internal static ref readonly @string arrayˢ => ref arrayˢᵇ;
private static readonly @string arrayˢᵇ = "Array"u8;

public static void TestCopyString(ж<Δtesting.T> t) {
    t.Run(sliceˢ, (ж<Δtesting.T> tΔ1) => {
        var s = bytes.Repeat(slice<byte>([(byte)'_']), 8);
        var val = ValueOf(s);

        nint n = Copy(val, ValueOf((@string)""u8));
        {
            var expecting = slice<byte>("________"u8); if (n != 0 || !bytes.Equal(s, expecting)) {
                tΔ1.Errorf("got n = %d, s = %s, expecting n = 0, s = %s", n, s, expecting);
            }
        }

        n = Copy(val, ValueOf(helloˢ));
        {
            var expecting = slice<byte>("hello___"u8); if (n != 5 || !bytes.Equal(s, expecting)) {
                tΔ1.Errorf("got n = %d, s = %s, expecting n = 5, s = %s", n, s, expecting);
            }
        }

        n = Copy(val, ValueOf(helloworldˢ));
        {
            var expecting = slice<byte>("hellowor"u8); if (n != 8 || !bytes.Equal(s, expecting)) {
                tΔ1.Errorf("got n = %d, s = %s, expecting n = 8, s = %s", n, s, expecting);
            }
        }
    });
    t.Run(arrayˢ, (ж<Δtesting.T> tΔ2) => {
        ref var s = ref heap<array<byte>>(new byte[]{(byte)'_', (byte)'_', (byte)'_', (byte)'_', (byte)'_', (byte)'_', (byte)'_', (byte)'_'}.array(), out var Ꮡs);
        var val = ValueOf(Ꮡs).Elem();

        nint n = Copy(val, ValueOf((@string)""u8));
        {
            var expecting = slice<byte>("________"u8); if (n != 0 || !bytes.Equal(Ꮡs.Value[..], expecting)) {
                tΔ2.Errorf("got n = %d, s = %s, expecting n = 0, s = %s", n, Ꮡs.Value[..], expecting);
            }
        }

        n = Copy(val, ValueOf(helloˢ));
        {
            var expecting = slice<byte>("hello___"u8); if (n != 5 || !bytes.Equal(Ꮡs.Value[..], expecting)) {
                tΔ2.Errorf("got n = %d, s = %s, expecting n = 5, s = %s", n, Ꮡs.Value[..], expecting);
            }
        }

        n = Copy(val, ValueOf(helloworldˢ));
        {
            var expecting = slice<byte>("hellowor"u8); if (n != 8 || !bytes.Equal(Ꮡs.Value[..], expecting)) {
                tΔ2.Errorf("got n = %d, s = %s, expecting n = 8, s = %s", n, Ꮡs.Value[..], expecting);
            }
        }
    });
}

public static void TestCopyArray(ж<Δtesting.T> t) {
    ref var a = ref heap<array<nint>>(new nint[]{1, 2, 3, 4, 10, 9, 8, 7}.array(), out var Ꮡa);
    ref var b = ref heap<array<nint>>(new nint[]{11, 22, 33, 44, 1010, 99, 88, 77, 66, 55, 44}.array(), out var Ꮡb);
    var c = b.Clone();
    var aa = ValueOf(Ꮡa).Elem();
    var ab = ValueOf(Ꮡb).Elem();
    Copy(ab, aa);
    for (nint i = 0; i < len(a); i++) {
        if (a[i] != b[i]) {
            t.Errorf("(i) a[%d]=%d, b[%d]=%d", i, a[i], i, b[i]);
        }
    }
    for (nint i = len(a); i < len(b); i++) {
        if (b[i] != c[i]) {
            t.Errorf("(ii) b[%d]=%d, c[%d]=%d", i, b[i], i, c[i]);
        } else {
            t.Logf("elem %d is okay\n", i);
        }
    }
}

internal partial struct TestBigUnnamedStruct_b /*dyn*/ {
    internal int64 a, b, c, d;
}

public static void TestBigUnnamedStruct(ж<Δtesting.T> t) {
    var b = new TestBigUnnamedStruct_b(1, 2, 3, 4);
    var v = ValueOf(b);
    var b1 = v.Interface()._<TestBigUnnamedStruct_b>();
    if (b1.a != b.a || b1.b != b.b || b1.c != b.c || b1.d != b.d) {
        t.Errorf("ValueOf(%v).Interface().(*Big) = %v", b, b1);
    }
}

partial struct big {
    internal int64 a, b, c, d, e;
}

public static void TestBigStruct(ж<Δtesting.T> t) {
    var b = new big(1, 2, 3, 4, 5);
    var v = ValueOf(b);
    var b1 = v.Interface()._<big>();
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

internal static readonly ж<map<@string, any>> ᏑcycleMap1 = new StaticBox<map<@string, any>>(default(map<@string, any>));
internal static ref map<@string, any> cycleMap1 => ref ((StaticBox<map<@string, any>>)ᏑcycleMap1).ValueSlot;
internal static readonly ж<map<@string, any>> ᏑcycleMap2 = new StaticBox<map<@string, any>>(default(map<@string, any>));
internal static ref map<@string, any> cycleMap2 => ref ((StaticBox<map<@string, any>>)ᏑcycleMap2).ValueSlot;
internal static readonly ж<map<@string, any>> ᏑcycleMap3 = new StaticBox<map<@string, any>>(default(map<@string, any>));
internal static ref map<@string, any> cycleMap3 => ref ((StaticBox<map<@string, any>>)ᏑcycleMap3).ValueSlot;

partial struct structWithSelfPtr {
    internal ж<structWithSelfPtr> p;
    internal @string s;
}

[GoInit] internal static void init() {
    loop1 = Ꮡloop2;
    loop2 = Ꮡloop1;

    loopy1 = Ꮡloopy2;
    loopy2 = Ꮡloopy1;

    cycleMap1 = new map<@string, any>{};
    cycleMap1["cycle"] = cycleMap1;
    cycleMap2 = new map<@string, any>{};
    cycleMap2["cycle"] = cycleMap2;
    cycleMap3 = new map<@string, any>{};
    cycleMap3["different"] = cycleMap3;
}

internal static slice<DeepEqualTest> deepEqualTests = slice<DeepEqualTest>([
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
    new(slice<byte>([1, 2, 3]), slice<byte>([1, 2, 3]), true),
    new(slice<MyByte>([1, 2, 3]), slice<MyByte>([1, 2, 3]), true),
    new(new MyBytes(new byte[]{1, 2, 3}.slice()), new MyBytes(new byte[]{1, 2, 3}.slice()), true),

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
    new(Ꮡ(new structWithSelfPtr(p: Ꮡ(new structWithSelfPtr(s: "a")))), Ꮡ(new structWithSelfPtr(p: Ꮡ(new structWithSelfPtr(s: "b")))), false),

    // Fun with floating point.
    new(Δmath.NaN(), Δmath.NaN(), false),
    new(Ꮡ(new float64[]{Δmath.NaN()}.array()), Ꮡ(new float64[]{Δmath.NaN()}.array()), false),
    new(Ꮡ(new float64[]{Δmath.NaN()}.array()), new self(), true),
    new(slice<float64>([Δmath.NaN()]), slice<float64>([Δmath.NaN()]), false),
    new(slice<float64>([Δmath.NaN()]), new self(), true),
    new(new map<float64, float64>{[Δmath.NaN()] = 1D}, new map<float64, float64>{[1D] = 2D}, false),
    new(new map<float64, float64>{[Δmath.NaN()] = 1D}, new self(), true),

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
    new(slice<byte>([1, 2, 3]), slice<MyByte>([1, 2, 3]), false),
    new(slice<MyByte>([1, 2, 3]), new MyBytes(new byte[]{1, 2, 3}.slice()), false),
    new(slice<byte>([1, 2, 3]), new MyBytes(new byte[]{1, 2, 3}.slice()), false),

    // Possible loops.
    new(Ꮡloop1, Ꮡloop1, true),
    new(Ꮡloop1, Ꮡloop2, true),
    new(Ꮡloopy1, Ꮡloopy1, true),
    new(Ꮡloopy1, Ꮡloopy2, true),
    new(ᏑcycleMap1, ᏑcycleMap2, true),
    new(ᏑcycleMap1, ᏑcycleMap3, false)
]);

public static void TestDeepEqual(ж<Δtesting.T> t) {
    foreach (var (i, vᴛ1) in deepEqualTests) {
        ref var test = ref heap(new DeepEqualTest(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        t.Run(fmt.Sprint(i), (ж<Δtesting.T> tΔ1) => {
            if (AreEqual(testʗ1.b, (new self()))) {
                testʗ1.b = testʗ1.a;
            }
            {
                var r = DeepEqual(testʗ1.a, testʗ1.b); if (r != testʗ1.eq) {
                    tΔ1.Errorf("DeepEqual(%#v, %#v) = %v, want %v", testʗ1.a, testʗ1.b, r, testʗ1.eq);
                }
            }
        });
    }
}

public static void TestTypeOf(ж<Δtesting.T> t) {
    // Special case for nil
    {
        var typ = TypeOf(default); if (typ != nil) {
            t.Errorf("expected nil type for nil value; got %v", typ);
        }
    }
    foreach (var (_, test) in deepEqualTests) {
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

partial struct Recursive {
    internal nint x;
    internal ж<Recursive> r;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object deepEqualRecursiveSameˢ = (@string)"DeepEqual(recursive same) = false, want true"u8;

public static void TestDeepEqualRecursiveStruct(ж<Δtesting.T> t) {
    var (a, b) = (@new<Recursive>(), @new<Recursive>());
    a.Value = new Recursive(12, a);
    b.Value = new Recursive(12, b);
    if (!DeepEqual(a.OrTypedNil(), b.OrTypedNil())) {
        t.Error(deepEqualRecursiveSameˢ);
    }
}

partial struct _Complex {
    internal nint a;
    internal array<ж<_Complex>> b = new(3);
    internal ж<@string> c;
    internal map<float64, float64> d;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object deepEqualComplexSameˢ = (@string)"DeepEqual(complex same) = false, want true"u8;

public static void TestDeepEqualComplexStruct(ж<Δtesting.T> t) {
    var m = new map<float64, float64>();
    ref var stra = ref heap<@string>(out var Ꮡstra);
    stra = helloˢ;
    ref var strb = ref heap<@string>(out var Ꮡstrb);
    strb = helloˢ;
    var (a, b) = (@new<_Complex>(), @new<_Complex>());
    a.Value = new _Complex(5, new ж<_Complex>[]{a, b, a}.array(), Ꮡstra, m);
    b.Value = new _Complex(5, new ж<_Complex>[]{b, a, a}.array(), Ꮡstrb, m);
    if (!DeepEqual(a.OrTypedNil(), b.OrTypedNil())) {
        t.Error(deepEqualComplexSameˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string hellooˢ => ref hellooˢᵇ;
private static readonly @string hellooˢᵇ = "helloo"u8;
internal static readonly object deepEqualComplexˢ = (@string)"DeepEqual(complex different) = true, want false"u8;

public static void TestDeepEqualComplexStructInequality(ж<Δtesting.T> t) {
    var m = new map<float64, float64>();
    ref var stra = ref heap<@string>(out var Ꮡstra);
    stra = helloˢ;
    ref var strb = ref heap<@string>(out var Ꮡstrb);
    strb = hellooˢ; // Difference is here
    var (a, b) = (@new<_Complex>(), @new<_Complex>());
    a.Value = new _Complex(5, new ж<_Complex>[]{a, b, a}.array(), Ꮡstra, m);
    b.Value = new _Complex(5, new ж<_Complex>[]{b, a, a}.array(), Ꮡstrb, m);
    if (DeepEqual(a.OrTypedNil(), b.OrTypedNil())) {
        t.Error(deepEqualComplexˢ);
    }
}

partial struct UnexpT {
    internal map<nint, nint> m;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object deepEqualX1X2FalseWantˢ = (@string)"DeepEqual(x1, x2) = false, want true"u8;
internal static readonly object deepEqualX1Y1TrueWantˢ = (@string)"DeepEqual(x1, y1) = true, want false"u8;

public static void TestDeepEqualUnexportedMap(ж<Δtesting.T> t) {
    // Check that DeepEqual can look at unexported fields.
    ref var x1 = ref heap<UnexpT>(new UnexpT(new map<nint, nint>{[1] = 2}), out var Ꮡx1);
    ref var x2 = ref heap<UnexpT>(new UnexpT(new map<nint, nint>{[1] = 2}), out var Ꮡx2);
    if (!DeepEqual(Ꮡx1, Ꮡx2)) {
        t.Error(deepEqualX1X2FalseWantˢ);
    }

    ref var y1 = ref heap<UnexpT>(new UnexpT(new map<nint, nint>{[2] = 3}), out var Ꮡy1);
    if (DeepEqual(Ꮡx1, Ꮡy1)) {
        t.Error(deepEqualX1Y1TrueWantˢ);
    }
}

partial struct deepEqualPerfTestsᴛ1 /*dyn*/ {
    internal any x, y;
}
internal static slice<deepEqualPerfTestsᴛ1> deepEqualPerfTests = slice<deepEqualPerfTestsᴛ1>([
    new(x: (int8)99, y: (int8)99),
    new(x: slice<int8>([99]), y: slice<int8>([99])),
    new(x: (int16)99, y: (int16)99),
    new(x: slice<int16>([99]), y: slice<int16>([99])),
    new(x: (int32)99, y: (int32)99),
    new(x: slice<int32>([99]), y: slice<int32>([99])),
    new(x: (int64)99, y: (int64)99),
    new(x: slice<int64>([99]), y: slice<int64>([99])),
    new(x: (nint)999999, y: (nint)999999),
    new(x: slice<nint>([999999]), y: slice<nint>([999999])),

    new(x: (uint8)99, y: (uint8)99),
    new(x: slice<uint8>([99]), y: slice<uint8>([99])),
    new(x: (uint16)99, y: (uint16)99),
    new(x: slice<uint16>([99]), y: slice<uint16>([99])),
    new(x: (uint32)99, y: (uint32)99),
    new(x: slice<uint32>([99]), y: slice<uint32>([99])),
    new(x: (uint64)99, y: (uint64)99),
    new(x: slice<uint64>([99]), y: slice<uint64>([99])),
    new(x: (nuint)999999, y: (nuint)999999),
    new(x: slice<nuint>([999999]), y: slice<nuint>([999999])),
    new(x: (uintptr)999999, y: (uintptr)999999),
    new(x: slice<uintptr>([999999]), y: slice<uintptr>([999999])),

    new(x: (float32)1.414F, y: (float32)1.414F),
    new(x: slice<float32>([1.414F]), y: slice<float32>([1.414F])),
    new(x: (float64)1.414D, y: (float64)1.414D),
    new(x: slice<float64>([1.414D]), y: slice<float64>([1.414D])),

    new(x: (complex64)1.414F, y: (complex64)1.414F),
    new(x: slice<complex64>([1.414F]), y: slice<complex64>([1.414F])),
    new(x: (complex128)1.414D, y: (complex128)1.414D),
    new(x: slice<complex128>([1.414D]), y: slice<complex128>([1.414D])),

    new(x: true, y: true),
    new(x: slice<bool>([true]), y: slice<bool>([true])),

    new(x: (@string)"abcdef", y: (@string)"abcdef"),
    new(x: slice<@string>(["abcdef"]), y: slice<@string>(["abcdef"])),

    new(x: slice<byte>("abcdef"u8), y: slice<byte>("abcdef"u8)),
    new(x: slice<slice<byte>>([slice<byte>("abcdef"u8)]), y: slice<slice<byte>>([slice<byte>("abcdef"u8)])),

    new(x: new byte[]{(byte)'a', (byte)'b', (byte)'c', (byte)'a', (byte)'b', (byte)'c'}.array(), y: new byte[]{(byte)'a', (byte)'b', (byte)'c', (byte)'a', (byte)'b', (byte)'c'}.array()),
    new(x: GoReflect.WithElemDims(new array<byte>[]{new byte[]{(byte)'a', (byte)'b', (byte)'c', (byte)'a', (byte)'b', (byte)'c'}.array()}.slice(), 6), y: GoReflect.WithElemDims(new array<byte>[]{new byte[]{(byte)'a', (byte)'b', (byte)'c', (byte)'a', (byte)'b', (byte)'c'}.array()}.slice(), 6))
]);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object testAllocatesMoreWithˢ = (@string)"test allocates more with -asan; see #70079"u8;

public static void TestDeepEqualAllocs(ж<Δtesting.T> t) {
    if (asan.Enabled) {
        t.Skip(testAllocatesMoreWithˢ);
    }

    foreach (var (_, vᴛ1) in deepEqualPerfTests) {
        ref var tt = ref heap(new deepEqualPerfTestsᴛ1(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(ValueOf(tt.x).Type().String(), (ж<Δtesting.T> tΔ1) => {
            var ttʗ2 = ttʗ1;
            var got = Δtesting.AllocsPerRun(100, () => {
                if (!DeepEqual(ttʗ2.x, ttʗ2.y)) {
                    tΔ1.Errorf("DeepEqual(%v, %v)=false", ttʗ2.x, ttʗ2.y);
                }
            });
            if ((nint)got != 0) {
                tΔ1.Errorf("DeepEqual(%v, %v) allocated %d times", ttʗ1.x, ttʗ1.y, (nint)got);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object mismatchedOffsetsInˢ = (@string)"mismatched offsets in structure alignment:"u8;

internal static void check2ndField(any x, uintptr offs, ж<Δtesting.T> t) {
    var s = ValueOf(x);
    var f = s.Type().Field(1);
    if (f.Offset != offs) {
        t.Error(mismatchedOffsetsInˢ, f.Offset, offs);
    }
}

internal partial struct TestAlignment_T1inner /*dyn*/ {
    internal nint a;
}

internal partial struct TestAlignment_T1 /*dyn*/ {
    /*embed*/ public TestAlignment_T1inner T1inner;
    internal nint f;
}

internal partial struct TestAlignment_T2inner /*dyn*/ {
    internal nint a, b;
}

internal partial struct TestAlignment_T2 /*dyn*/ {
    /*embed*/ public TestAlignment_T2inner T2inner;
    internal nint f;
}

// Check that structure alignment & offsets viewed through reflect agree with those
// from the compiler itself.
public static void TestAlignment(ж<Δtesting.T> t) {
    ref var x = ref heap<TestAlignment_T1>(new TestAlignment_T1(new TestAlignment_T1inner(2), 17), out var Ꮡx);
    check2ndField(x, (uintptr)Ꮡx.Ꮡf - (uintptr)Ꮡx, t);

    ref var x1 = ref heap<TestAlignment_T2>(new TestAlignment_T2(new TestAlignment_T2inner(2, 3), 17), out var Ꮡx1);
    check2ndField(x1, (uintptr)Ꮡx1.Ꮡf - (uintptr)Ꮡx1, t);
}

public static partial void Nil(any a, ж<Δtesting.T> t) {
    var n = ValueOf(a).Field(0);
    if (!n.IsNil()) {
        t.Errorf("%v should be nil", a);
    }
}

public static partial void NotNil(any a, ж<Δtesting.T> t) {
    var n = ValueOf(a).Field(0);
    if (n.IsNil()) {
        t.Errorf("value of type %v should not be nil", ValueOf(a).Type().String());
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

internal partial struct TestIsNil_mi /*dyn*/ {
    internal map<nint, nint> x;
}

internal partial struct TestIsNil_fi /*dyn*/ {
    internal Action<ж<Δtesting.T>> x;
}

public static void TestIsNil(ж<Δtesting.T> t) {
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
        var ty = TypeOf(ts).Field(0).Type;
        var v = Zero(ty);
        v.IsNil(); // panics if not okay to call
    }

    // Check the implementations
    TestIsNil_doNil pi = default;
    Nil(pi, t);
    pi.x = @new<nint>();
    NotNil(pi, t);

    TestAppend_i si = default;
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

internal static S /*out*/ setField<S, V>(S @inʗp, uintptr offset, V value) {
    ref var @in = ref heap(@inʗp, out var Ꮡin);

    ((ж<V>)(uintptr)(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(Ꮡin), offset))).ValueSlot = value;
    return @in;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object notZeroˢ = (@string)"not-zero"u8;
internal static readonly object shouldPanicForInvalidˢ = (@string)"should panic for invalid value"u8;

internal partial struct TestIsZero_type /*dyn*/ {
    internal any x;
    internal bool want;
}

internal partial struct TestIsZero_typeᴛ1 /*dyn*/ {
    internal ж<nint> p;
}

internal partial struct TestIsZero_typeᴛ2 /*dyn*/ {
    internal slice<nint> s;
}

internal partial struct TestIsZero_typeᴛ3 /*dyn*/ {
    /*embed*/ public reflect_package.ΔValue Value;
}

internal partial struct TestIsZero_typeᴛ4 /*dyn*/ {
    internal uintptr _;
    internal uintptr a;
    internal uintptr __;
}

internal partial struct TestIsZero_typeᴛ5 /*dyn*/ {
    internal Action _;
    internal Action a;
    internal Action __;
}

internal partial struct TestIsZero_typeᴛ6 /*dyn*/ {
    internal array<S> a = new(256);
}

internal partial struct TestIsZero_typeᴛ7 /*dyn*/ {
    internal array<float32> a = new(256);
}

internal partial struct TestIsZero_typeᴛ8 /*dyn*/ {
    internal array<S> _ = new(256);
    internal array<S> a = new(256);
}

public static void TestIsZero(ж<Δtesting.T> t) {
    foreach (var (i, tt) in slice<TestIsZero_type>([
        // Booleans
        new(true, false),
        new(false, true),
        // Numeric types
        new((nint)0, true),
        new((nint)1, false),
        new((int8)0, true),
        new((int8)1, false),
        new((int16)0, true),
        new((int16)1, false),
        new((int32)0, true),
        new((int32)1, false),
        new((int64)0, true),
        new((int64)1, false),
        new((nuint)0, true),
        new((nuint)1, false),
        new((uint8)0, true),
        new((uint8)1, false),
        new((uint16)0, true),
        new((uint16)1, false),
        new((uint32)0, true),
        new((uint32)1, false),
        new((uint64)0, true),
        new((uint64)1, false),
        new((float32)0F, true),
        new((float32)1.2F, false),
        new((float64)0D, true),
        new((float64)1.2D, false),
        new(Δmath.Copysign(0D, -1D), true),
        new((complex64)0F, true),
        new((complex64)1.2F, false),
        new((complex128)0D, true),
        new((complex128)1.2D, false),
        new(complex(Δmath.Copysign(0D, -1D), 0D), true),
        new(complex(0D, Δmath.Copysign(0D, -1D)), true),
        new(complex(Δmath.Copysign(0D, -1D), Δmath.Copysign(0D, -1D)), true),
        new((uintptr)0, true),
        new((uintptr)128, false),
        // Array
        new(Zero(TypeOf(new @string[]{}.array(5))).Interface(), true),
        new(new @string[]{}.array(5), true),                      // comparable array
        new(new @string[]{"", "", "", "a", ""}.array(), false),   // comparable array
        new(new ж<nint>[]{}.array(1), true),                      // direct pointer array
        new(new ж<nint>[]{@new<nint>()}.array(), false),          // direct pointer array
        new(new slice<nint>[]{}.array(3), true),                  // incomparable array
        new(new slice<nint>[]{slice<nint>([1])}.array(3), false), // incomparable array
        new(new byte[]{}.array(4096), true),
        new(new byte[]{1}.array(4096), false),
        new(new TestIsZero_typeᴛ1[]{}.array(1), true),
        new(new TestIsZero_typeᴛ1[]{new(@new<nint>())}.array(), false),
        new(new reflectꓸValue[]{}.array(3, () => new(nil)), true),
        new(new reflectꓸValue[]{new(), ValueOf((nint)0), new()}.array(), false),
        // Chan
        new((channel<@string>)(default), true),
        new(new channel<@string>(0), false),
        new(time.After(1), false),
        // Func
        new(((Action)(default)).OrTypedNilFunc(), true),
        new(New, false),
        // Interface
        new(New(TypeOf(@new<error>()).Elem()).Elem(), true),
        new((Δio.Reader)new reflect_test_package.strings_ReaderжReader(strings.NewReader(""u8)), false),
        // Map
        new((map<@string, @string>)default, true),
        new(new map<@string, @string>{}, false),
        new(new map<@string, @string>(), false),
        // Pointer
        new((ж<Action>)nil, true),
        new((ж<nint>)nil, true),
        new(@new<nint>(), false),
        // Slice
        new(slice<@string>([]), false),
        new((slice<@string>)(default), true),
        new(new slice<@string>(0), false),
        // Strings
        new((@string)""u8, true),
        new(notZeroˢ, false),
        // Structs
        new(new T(), true),                                  // comparable struct
        new(new T(123, 456.75D, "hello", Ꮡ_i), false),       // comparable struct
        new(new TestIsZero_typeᴛ1(), true),                  // direct pointer struct
        new(new TestIsZero_typeᴛ1(@new<nint>()), false),     // direct pointer struct
        new(new TestIsZero_typeᴛ2(), true),                  // incomparable struct
        new(new TestIsZero_typeᴛ2(slice<nint>([1])), false), // incomparable struct
        new(new TestIsZero_typeᴛ3(), true),
        new(new TestIsZero_typeᴛ3(ValueOf((nint)0)), false),
        new(new TestIsZero_typeᴛ4(), true), // comparable struct with blank fields
        new(setField<TestIsZero_typeᴛ4, nint>(new TestIsZero_typeᴛ4(), 0 * /* unsafe.Sizeof(uintptr(0)) */ (uintptr)8, 1), true),
        new(setField<TestIsZero_typeᴛ4, nint>(new TestIsZero_typeᴛ4(), 1 * /* unsafe.Sizeof(uintptr(0)) */ (uintptr)8, 1), false),
        new(setField<TestIsZero_typeᴛ4, nint>(new TestIsZero_typeᴛ4(), 2 * /* unsafe.Sizeof(uintptr(0)) */ (uintptr)8, 1), true),
        new(new TestIsZero_typeᴛ5(), true), // incomparable struct with blank fields
        new(setField(new TestIsZero_typeᴛ5(), 0 * /* unsafe.Sizeof((func())(nil)) */ (uintptr)8, () => {
        }), true),
        new(setField(new TestIsZero_typeᴛ5(), 1 * /* unsafe.Sizeof((func())(nil)) */ (uintptr)8, () => {
        }), false),
        new(setField(new TestIsZero_typeᴛ5(), 2 * /* unsafe.Sizeof((func())(nil)) */ (uintptr)8, () => {
        }), true),
        new(new TestIsZero_typeᴛ6(), true),
        new(new TestIsZero_typeᴛ6(a: new array<S>(256){[2] = new(i1: 1)}), false),
        new(new TestIsZero_typeᴛ7(), true),
        new(new TestIsZero_typeᴛ7(a: new array<float32>(256){[2] = 1.0F}), false),
        new(new TestIsZero_typeᴛ8(), true),
        new(setField(new TestIsZero_typeᴛ8(), 0 * /* unsafe.Sizeof(int64(0)) */ (uintptr)8, (int64)1), true),
        // UnsafePointer
        new(@unsafe.Pointer.OrTypedNil((@unsafe.Pointer)default), true),
        new(@unsafe.Pointer.FromPinnedBox(@new<nint>()), false)
    ])) {
        reflectꓸValue x = new(nil);
        {
            var (v, ok) = tt.x._<reflectꓸValue>(ᐧ); if (ok) {
                x = v;
            } else {
                x = ValueOf(tt.x);
            }
        }

        var b = x.IsZero();
        if (b != tt.want) {
            t.Errorf("%d: IsZero((%s)(%+v)) = %t, want %t", i, x.Kind(), tt.x, b, tt.want);
        }

        if (!Zero(TypeOf(tt.x)).IsZero()) {
            t.Errorf("%d: IsZero(Zero(TypeOf((%s)(%+v)))) is false", i, x.Kind(), tt.x);
        }

        var p = New(x.Type()).Elem();
        p.Set(x);
        p.SetZero();
        if (!p.IsZero()) {
            t.Errorf("%d: IsZero((%s)(%+v)) is true after SetZero", i, p.Kind(), tt.x);
        }
    }

    func(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                {
                    var r = recover(); if (r == nil) {
                        t.Error(shouldPanicForInvalidˢ);
                    }
                }
            }, ref ᒐ);
            (new reflectꓸValue()).IsZero();
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    })();
}

public static void TestInternalIsZero(ж<Δtesting.T> t) {
    var b = new slice<byte>(512);
    for (nint a = 0; a < 8; a++) {
        for (nint i = 1; i <= 512 - a; i++) {
            reflect_internal_test_package.InternalIsZero(b.slice(a, a + i));
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object interfaceOnInterfaceˢ = (@string)"Interface() on interface: "u8;

internal partial struct TestInterfaceExtraction_s /*dyn*/ {
    public Δio.Writer W;
}

public static void TestInterfaceExtraction(ж<Δtesting.T> t) {
    ref var s = ref heap(new TestInterfaceExtraction_s(), out var Ꮡs);

    s.W = new Δos.FileжWriter(Δos.Stdout);
    var v = Indirect(ValueOf(Ꮡs)).Field(0).Interface();
    if (!AreEqual(v, s.W._<any>())) {
        t.Error(interfaceOnInterfaceˢ, v, s.W);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object valueOfIntNilElemIsValidˢ = (@string)"ValueOf((*int)(nil)).Elem().IsValid()"u8;

public static void TestNilPtrValueSub(ж<Δtesting.T> t) {
    ж<nint> pi = default;
    {
        var pv = ValueOf(pi.OrTypedNil()); if (pv.Elem().IsValid()) {
            t.Error(valueOfIntNilElemIsValidˢ);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object notPresentˢ = (@string)"not-present"u8;
internal static ref readonly @string notAssignableˢ => ref notAssignableˢᵇ;
private static readonly @string notAssignableˢᵇ = "not assignable"u8;
internal static ref readonly @string keyˢ => ref keyˢᵇ;
private static readonly @string keyˢᵇ = "key"u8;

internal partial struct TestMap_S /*@string*/;

public static void TestMap(ж<Δtesting.T> t) {
    ref var m = ref heap<map<@string, nint>>(new map<@string, nint>{["a"] = 1, ["b"] = 2}, out var Ꮡm);
    ref var mv = ref heap<reflectꓸValue>(ValueOf(m), out var Ꮡmv);
    {
        nint n = mv.Len(); if (n != len(m)) {
            t.Errorf("Len = %d, want %d", n, len(m));
        }
    }
    var keys = mv.MapKeys();
    var newmap = MakeMap(mv.Type());
    foreach (var (k, vΔ1) in m) {
        // Check that returned Keys match keys in range.
        // These aren't required to be in the same order.
        var seen = false;
        foreach (var (_, kv) in keys) {
            if (kv.String() == k) {
                seen = true;
                break;
            }
        }
        if (!seen) {
            t.Errorf("Missing key %q", k);
        }

        // Check that value lookup is correct.
        var vvΔ1 = mv.MapIndex(ValueOf(k));
        {
            var vi = vvΔ1.Int(); if (vi != (int64)vΔ1) {
                t.Errorf("Key %q: have value %d, want %d", k, vi, vΔ1);
            }
        }

        // Copy into new map.
        newmap.SetMapIndex(ValueOf(k), ValueOf(vΔ1));
    }
    var vv = mv.MapIndex(ValueOf(notPresentˢ));
    if (vv.IsValid()) {
        t.Errorf("Invalid key: got non-nil value %s", valueToString(vv));
    }

    var newm = newmap.Interface()._<map<@string, nint>>();
    if (len(newm) != len(m)) {
        t.Errorf("length after copy: newm=%d, m=%d", len(newm), len(m));
    }

    foreach (var (k, vΔ2) in newm) {
        var (mvΔ1, okΔ1) = m[k, ꟷ];
        if (mvΔ1 != vΔ2) {
            t.Errorf("newm[%q] = %d, but m[%q] = %d, %v", k, vΔ2, k, mvΔ1, okΔ1);
        }
    }

    newmap.SetMapIndex(ValueOf("a"), new reflectꓸValue());
    var (v, ok) = newm["a", ꟷ];
    if (ok) {
        t.Errorf("newm[\"a\"] = %d after delete", v);
    }

    mv = ValueOf(Ꮡm).Elem();
    mv.Set(Zero(mv.Type()));
    if (m != nil) {
        t.Errorf("mv.Set(nil) failed");
    }
    var mvʗ1 = mv;
    shouldPanic(notAssignableˢ, () => {
        mvʗ1.MapIndex(ValueOf((TestMap_S)(@string)keyˢ));
    });
    var mvʗ2 = mv;
    shouldPanic(notAssignableˢ, () => {
        mvʗ2.SetMapIndex(ValueOf((TestMap_S)(@string)keyˢ), ValueOf((nint)0));
    });
}

public static void TestNilMap(ж<Δtesting.T> t) {
    map<@string, nint> m = default;
    var mv = ValueOf(m);
    var keys = mv.MapKeys();
    if (len(keys) != 0) {
        t.Errorf(">0 keys for nil map: %v", keys);
    }

    // Check that value for missing key is zero.
    var x = mv.MapIndex(ValueOf(helloˢ));
    if (x.Kind() != Invalid) {
        t.Errorf("m.MapIndex(\"hello\") for nil map = %v, want Invalid Value", x);
    }

    // Check big value too.
    map<@string, array<byte>> mbig = default;
    x = ValueOf(mbig).MapIndex(ValueOf(helloˢ));
    if (x.Kind() != Invalid) {
        t.Errorf("mbig.MapIndex(\"hello\") for nil map = %v, want Invalid Value", x);
    }

    // Test that deletes from a nil map succeed.
    mv.SetMapIndex(ValueOf("hi"), new reflectꓸValue());
}

public static void TestChan(ж<Δtesting.T> t) {
    for (nint loop = 0; loop < 2; loop++) {
        channel<nint> cΔ1 = default;
        reflectꓸValue cvΔ1 = new(nil);

        // check both ways to allocate channels
        switch (loop) {
        case 1: {
            cΔ1 = new channel<nint>(1);
            cvΔ1 = ValueOf(cΔ1);
            break;
        }
        case 0: {
            cvΔ1 = MakeChan(TypeOf(cΔ1), 1);
            cΔ1 = cvΔ1.Interface()._<channel<nint>>();
            break;
        }}

        // Send
        cvΔ1.Send(ValueOf((nint)2));
        {
            nint iΔ1 = ᐸꟷ(cΔ1); if (iΔ1 != 2) {
                t.Errorf("reflect Send 2, native recv %d", iΔ1);
            }
        }

        // Recv
        cΔ1.ᐸꟷ(3);
        {
            var (iΔ2, okΔ1) = cvΔ1.Recv(); if (iΔ2.Int() != 3 || !okΔ1) {
                t.Errorf("native send 3, reflect Recv %d, %t", iΔ2.Int(), okΔ1);
            }
        }

        // TryRecv fail
        var (val, ok) = cvΔ1.TryRecv();
        if (val.IsValid() || ok) {
            t.Errorf("TryRecv on empty chan: %s, %t", valueToString(val), ok);
        }

        // TryRecv success
        cΔ1.ᐸꟷ(4);
        (val, ok) = cvΔ1.TryRecv();
        if (!val.IsValid()) {
            t.Errorf("TryRecv on ready chan got nil");
        } else {
            var iΔ3 = val.Int(); if (iΔ3 != 4 || !ok) {
                t.Errorf("native send 4, TryRecv %d, %t", iΔ3, ok);
            }
        }

        // TrySend fail
        cΔ1.ᐸꟷ(100);
        ok = cvΔ1.TrySend(ValueOf((nint)5));
        nint i = ᐸꟷ(cΔ1);
        if (ok) {
            t.Errorf("TrySend on full chan succeeded: value %d", i);
        }

        // TrySend success
        ok = cvΔ1.TrySend(ValueOf((nint)6));
        if (!ok) {
            t.Errorf("TrySend on empty chan failed");
            var selᴛ1 = cΔ1;
            switch (trySelect(ᐸꟷ(selᴛ1, ꓸꓸꓸ))) {
            case 0 when selᴛ1.ꟷᐳ(out var x): {
                t.Errorf("TrySend failed but it did send %d", x);
                break;
            }
            default: {
                break;
            }}
        } else {
            {
                i = ᐸꟷ(cΔ1); if (i != 6) {
                    t.Errorf("TrySend 6, recv %d", i);
                }
            }
        }

        // Close
        cΔ1.ᐸꟷ(123);
        cvΔ1.Close();
        {
            var (iΔ1, okΔ1) = cvΔ1.Recv(); if (iΔ1.Int() != 123 || !okΔ1) {
                t.Errorf("send 123 then close; Recv %d, %t", iΔ1.Int(), okΔ1);
            }
        }
        {
            var (iΔ2, okΔ2) = cvΔ1.Recv(); if (iΔ2.Int() != 0 || okΔ2) {
                t.Errorf("after close Recv %d, %t", iΔ2.Int(), okΔ2);
            }
        }
        // Closing a read-only channel
        shouldPanic(""u8, () => {
            var cΔ2 = new /*<-*/channel<nint>(1, GoChanDir.Recv);
            var cvΔ2 = ValueOf(cΔ2);
            cvΔ2.Close();
        });
    }

    // check creation of unbuffered channel
    channel<nint> c = default;
    var cv = MakeChan(TypeOf(c), 0);
    c = cv.Interface()._<channel<nint>>();
    if (cv.TrySend(ValueOf((nint)7))) {
        t.Errorf("TrySend on sync chan succeeded");
    }
    {
        var (v, ok) = cv.TryRecv(); if (v.IsValid() || ok) {
            t.Errorf("TryRecv on sync chan succeeded: isvalid=%v ok=%v", v.IsValid(), ok);
        }
    }

    // len/cap
    cv = MakeChan(TypeOf(c), 10);
    c = cv.Interface()._<channel<nint>>();
    for (nint i = 0; i < 3; i++) {
        c.ᐸꟷ(i);
    }
    {
        nint l = cv.Len();
        nint m = cv.Cap(); if (l != len(c) || m != cap(c)) {
            t.Errorf("Len/Cap = %d/%d want %d/%d", l, m, len(c), cap(c));
        }
    }
}

// caseInfo describes a single case in a select test.
partial struct caseInfo {
    internal @string desc;
    internal bool canSelect;
    internal reflectꓸValue recv;
    internal bool closed;
    internal Action helper;
    internal bool panic;
}

internal static ж<bool> allselect = flag.Bool("allselect", false, "exhaustive select test");

public static void TestSelect(ж<Δtesting.T> t) {
    var helpersᴷ = new stackAppend<nint>();

    ᏑselectWatch.of(selectWatchᴛ1.Ꮡonce).Do([MethodImpl(MethodImplOptions.NoInlining)] () => {
        goǃ(selectWatcher);
    });

    exhaustive x = default;
    nint nch = 0;
    (reflectꓸValue ch, reflectꓸValue val) newop(nint n, nint cap) {
        reflectꓸValue ch = new(nil);
        reflectꓸValue val = new(nil);
        nch++;
        if (nch % 101 % 2 == 1) {
            var c = new channel<nint>(cap);
            ch = ValueOf(c);
            val = ValueOf(n);
        } else {
            var c = new channel<@string>(cap);
            ch = ValueOf(c);
            val = ValueOf(fmt.Sprint(n));
        }
        return (ch, val);
    }

    for (nint n = 0; x.Next(); n++) {
        if (Δtesting.Short() && n >= 1000) {
            break;
        }
        if (n >= 100000 && !allselect.Value) {
            break;
        }
        if (n % 100000 == 0 && Δtesting.Verbose()) {
            println((@string)"TestSelect", n);
        }
        slice<Δreflect.SelectCase> cases = default;
        slice<caseInfo> info = default;

        // Ready send.
        if (x.Maybe()) {
            var (ch, val) = newop(len(cases), 1);
            cases = append(cases, new SelectCase(
                Dir: SelectSend,
                Chan: ch,
                Send: val
            ));
            info = append(info, new caseInfo(desc: "ready send", canSelect: true));
        }

        // Ready recv.
        if (x.Maybe()) {
            var (ch, val) = newop(len(cases), 1);
            ch.Send(val);
            cases = append(cases, new SelectCase(
                Dir: SelectRecv,
                Chan: ch
            ));
            info = append(info, new caseInfo(desc: "ready recv", canSelect: true, recv: val));
        }

        // Blocking send.
        if (x.Maybe()) {
            ref var ch = ref heap<reflectꓸValue>(out var Ꮡch);
            (ch, var val) = newop(len(cases), 0);
            cases = append(cases, new SelectCase(
                Dir: SelectSend,
                Chan: ch,
                Send: val
            ));
            // Let it execute?
            if (x.Maybe()) {
                var chʗ1 = ch;
                var f = () => {
                    chʗ1.Recv();
                };
                info = append(info, new caseInfo(desc: "blocking send", helper: f));
            } else {
                info = append(info, new caseInfo(desc: "blocking send"));
            }
        }

        // Blocking recv.
        if (x.Maybe()) {
            ref var ch = ref heap<reflectꓸValue>(out var Ꮡch);
            ref var val = ref heap<reflectꓸValue>(out var Ꮡval);
            (ch, val) = newop(len(cases), 0);
            cases = append(cases, new SelectCase(
                Dir: SelectRecv,
                Chan: ch
            ));
            // Let it execute?
            if (x.Maybe()) {
                var chʗ2 = ch;
                var valʗ1 = val;
                var f = () => {
                    chʗ2.Send(valʗ1);
                };
                info = append(info, new caseInfo(desc: "blocking recv", recv: val, helper: f));
            } else {
                info = append(info, new caseInfo(desc: "blocking recv"));
            }
        }

        // Zero Chan send.
        if (x.Maybe()) {
            // Maybe include value to send.
            reflectꓸValue val = new(nil);
            if (x.Maybe()) {
                val = ValueOf((nint)100);
            }
            cases = append(cases, new SelectCase(
                Dir: SelectSend,
                Send: val
            ));
            info = append(info, new caseInfo(desc: "zero Chan send"));
        }

        // Zero Chan receive.
        if (x.Maybe()) {
            cases = append(cases, new SelectCase(
                Dir: SelectRecv
            ));
            info = append(info, new caseInfo(desc: "zero Chan recv"));
        }

        // nil Chan send.
        if (x.Maybe()) {
            cases = append(cases, new SelectCase(
                Dir: SelectSend,
                Chan: ValueOf((channel<nint>)(default)),
                Send: ValueOf((nint)101)
            ));
            info = append(info, new caseInfo(desc: "nil Chan send"));
        }

        // nil Chan recv.
        if (x.Maybe()) {
            cases = append(cases, new SelectCase(
                Dir: SelectRecv,
                Chan: ValueOf((channel<nint>)(default))
            ));
            info = append(info, new caseInfo(desc: "nil Chan recv"));
        }

        // closed Chan send.
        if (x.Maybe()) {
            var ch = new channel<nint>(0);
            close(ch);
            cases = append(cases, new SelectCase(
                Dir: SelectSend,
                Chan: ValueOf(ch),
                Send: ValueOf((nint)101)
            ));
            info = append(info, new caseInfo(desc: "closed Chan send", canSelect: true, panic: true));
        }

        // closed Chan recv.
        if (x.Maybe()) {
            var (ch, val) = newop(len(cases), 0);
            ch.Close();
            val = Zero(val.Type());
            cases = append(cases, new SelectCase(
                Dir: SelectRecv,
                Chan: ch
            ));
            info = append(info, new caseInfo(desc: "closed Chan recv", canSelect: true, closed: true, recv: val));
        }

        Action helper = default; // goroutine to help the select complete

        // Add default? Must be last case here, but will permute.
        // Add the default if the select would otherwise
        // block forever, and maybe add it anyway.
        nint numCanSelect = 0;
        var canProceed = false;
        var canBlock = true;
        var canPanic = false;
        var helpers = slice<nint>([]);
        foreach (var (iΔ1, c) in info) {
            if (c.canSelect) {
                canProceed = true;
                canBlock = false;
                numCanSelect++;
                if (c.panic) {
                    canPanic = true;
                }
            } else if (c.helper != nil) {
                canProceed = true;
                helpers = append(helpersᴷ, helpers, iΔ1);
            }
        }
        if (!canProceed || x.Maybe()) {
            cases = append(cases, new SelectCase(
                Dir: SelectDefault
            ));
            info = append(info, new caseInfo(desc: "default", canSelect: canBlock));
            numCanSelect++;
        } else if (canBlock) {
            // Select needs to communicate with another goroutine.
            var casΔ1 = Ꮡ(info, helpers[x.Choose(len(helpers))]);
            helper = casΔ1.helper;
            casΔ1.canSelect = true;
            numCanSelect++;
        }

        // Permute cases and case info.
        // Doing too much here makes the exhaustive loop
        // too exhausting, so just do two swaps.
        for (nint loop = 0; loop < 2; loop++) {
            nint iΔ2 = x.Choose(len(cases));
            nint j = x.Choose(len(cases));
            (cases[iΔ2], cases[j]) = (cases[j], cases[iΔ2]);
            (info[iΔ2], info[j]) = (info[j], info[iΔ2]);
        }

        if (helper != nil) {
            // We wait before kicking off a goroutine to satisfy a blocked select.
            // The pause needs to be big enough to let the select block before
            // we run the helper, but if we lose that race once in a while it's okay: the
            // select will just proceed immediately. Not a big deal.
            // For short tests we can grow [sic] the timeout a bit without fear of taking too long
            var pause = 10 * time.Microsecond;
            if (Δtesting.Short()) {
                pause = 100 * time.Microsecond;
            }
            time.AfterFunc(pause, helper);
        }

        // Run select.
        var (i, recv, recvOK, panicErr) = runSelect(cases, info);
        if (panicErr != nil && !canPanic) {
            t.Fatalf("%s\npanicked unexpectedly: %v", fmtSelect(info), panicErr);
        }
        if (panicErr == nil && canPanic && numCanSelect == 1) {
            t.Fatalf("%s\nselected #%d incorrectly (should panic)", fmtSelect(info), i);
        }
        if (panicErr != nil) {
            continue;
        }

        var cas = info[i];
        if (!cas.canSelect) {
            @string recvStr = ""u8;
            if (recv.IsValid()) {
                recvStr = fmt.Sprintf(", received %v, %v"u8, recv.Interface(), recvOK);
            }
            t.Fatalf("%s\nselected #%d incorrectly%s", fmtSelect(info), i, recvStr);
        }
        if (cas.panic) {
            t.Fatalf("%s\nselected #%d incorrectly (case should panic)", fmtSelect(info), i);
        }

        if (cases[i].Dir == SelectRecv) {
            if (!recv.IsValid()) {
                t.Fatalf("%s\nselected #%d but got %v, %v, want %v, %v", fmtSelect(info), i, recv, recvOK, cas.recv.Interface(), !cas.closed);
            }
            if (!cas.recv.IsValid()) {
                t.Fatalf("%s\nselected #%d but internal error: missing recv value", fmtSelect(info), i);
            }
            if (!AreEqual(recv.Interface(), cas.recv.Interface()) || recvOK != !cas.closed) {
                if (AreEqual(recv.Interface(), cas.recv.Interface()) && recvOK == !cas.closed) {
                    t.Fatalf("%s\nselected #%d, got %#v, %v, and DeepEqual is broken on %T", fmtSelect(info), i, recv.Interface(), recvOK, recv.Interface());
                }
                t.Fatalf("%s\nselected #%d but got %#v, %v, want %#v, %v", fmtSelect(info), i, recv.Interface(), recvOK, cas.recv.Interface(), !cas.closed);
            }
        } else {
            if (recv.IsValid() || recvOK) {
                t.Fatalf("%s\nselected #%d but got %v, %v, want %v, %v", fmtSelect(info), i, recv, recvOK, new reflectꓸValue(), false);
            }
        }
    }
}

public static void TestSelectMaxCases(ж<Δtesting.T> t) {
    GoFrame ᒐ = default;
    try {
        slice<Δreflect.SelectCase> sCases = default;
        var Δchannel = new channel<nint>(0);
        close(Δchannel);
        for (nint i = 0; i < 65536; i++) {
            sCases = append(sCases, new SelectCase(
                Dir: SelectRecv,
                Chan: ValueOf(Δchannel)
            ));
        }
        // Should not panic
        (_, _, _) = Select(sCases);
        sCases = append(sCases, new SelectCase(
            Dir: SelectRecv,
            Chan: ValueOf(Δchannel)
        ));
        defer(() => {
            {
                var err = recover(); if (err != nil) {
                    if (err._<@string>() != "reflect.Select: too many cases (max 65536)"u8) {
                        t.Fatalf("unexpected error from select call with greater than max supported cases");
                    }
                } else {
                    t.Fatalf("expected select call to panic with greater than max supported cases");
                }
            }
        }, ref ᒐ);
        // Should panic
        (_, _, _) = Select(sCases);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static partial void TestSelectNop(ж<Δtesting.T> t) {
    // "select { default: }" should always return the default case.
    var (chosen, _, _) = Select(slice<Δreflect.SelectCase>([new(Dir: SelectDefault)]));
    if (chosen != 0) {
        t.Fatalf("expected Select to return 0, but got %#v", chosen);
    }
}

// selectWatch and the selectWatcher are a watchdog mechanism for running Select.
// If the selectWatcher notices that the select has been blocked for >1 second, it prints
// an error describing the select and panics the entire test binary.
partial struct selectWatchᴛ1 /*dyn*/ {
    /*embed*/ public sync_package.Mutex Mutex;
    internal Δsync.Once once;
    internal time.Time now;
    internal slice<caseInfo> info;
}
internal static readonly ж<selectWatchᴛ1> ᏑselectWatch = new StaticBox<selectWatchᴛ1>(new selectWatchᴛ1(nil));
internal static ref selectWatchᴛ1 selectWatch => ref ((StaticBox<selectWatchᴛ1>)ᏑselectWatch).ValueSlot;

internal static void selectWatcher() {
    for (;;) {
        time.Sleep(1 * time.ΔSecond);
        ᏑselectWatch.of(selectWatchᴛ1.ᏑMutex).Lock();
        if (selectWatch.info != nil && time.Since(selectWatch.now) > (time.Duration)(10000000000L)) {
            fmt.Fprintf(new Δos.FileжWriter(Δos.Stderr), "TestSelect:\n%s blocked indefinitely\n"u8, fmtSelect(selectWatch.info));
            throw panic("select stuck");
        }
        ᏑselectWatch.of(selectWatchᴛ1.ᏑMutex).Unlock();
    }
}

// runSelect runs a single select test.
// It returns the values returned by Select but also returns
// a panic value if the Select panics.
internal static (nint chosen, reflectꓸValue recv, bool recvOK, any panicErr) runSelect(slice<Δreflect.SelectCase> cases, slice<caseInfo> info) {
    nint chosen = default;
    reflectꓸValue recv = new(nil);
    bool recvOK = default;
    any panicErr = default;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            panicErr = recover();

            ᏑselectWatch.of(selectWatchᴛ1.ᏑMutex).Lock();
            selectWatch.info = default;
            ᏑselectWatch.of(selectWatchᴛ1.ᏑMutex).Unlock();
        }, ref ᒐ);

        ᏑselectWatch.of(selectWatchᴛ1.ᏑMutex).Lock();
        selectWatch.now = time.Now();
        selectWatch.info = info;
        ᏑselectWatch.of(selectWatchᴛ1.ᏑMutex).Unlock();

        (chosen, recv, recvOK) = Select(cases);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return (chosen, recv, recvOK, panicErr);
}

// fmtSelect formats the information about a single select test.
internal static @string fmtSelect(slice<caseInfo> info) {
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    fmt.Fprintf(new reflect_test_package.strings_BuilderжWriter(Ꮡbuf), "\nselect {\n"u8);
    foreach (var (i, cas) in info) {
        fmt.Fprintf(new reflect_test_package.strings_BuilderжWriter(Ꮡbuf), "%d: %s"u8, i, cas.desc);
        if (cas.recv.IsValid()) {
            fmt.Fprintf(new reflect_test_package.strings_BuilderжWriter(Ꮡbuf), " val=%#v"u8, cas.recv.Interface());
        }
        if (cas.canSelect) {
            fmt.Fprintf(new reflect_test_package.strings_BuilderжWriter(Ꮡbuf), " canselect"u8);
        }
        if (cas.panic) {
            fmt.Fprintf(new reflect_test_package.strings_BuilderжWriter(Ꮡbuf), " panic"u8);
        }
        fmt.Fprintf(new reflect_test_package.strings_BuilderжWriter(Ꮡbuf), "\n"u8);
    }
    fmt.Fprintf(new reflect_test_package.strings_BuilderжWriter(Ꮡbuf), "}"u8);
    return buf.String();
}

partial struct two /*[2]uintptr*/;

// Difficult test for function call because of
// implicit padding between arguments.
internal static (byte i, nint j, byte k, two l, byte m, float32 n, byte o) dummy(byte b, nint c, byte d, two e, byte f, float32 g, byte h) {
    e = e.Clone();

    return (b, c, d, e.Clone(), f, g, h);
}

public static void TestFunc(ж<Δtesting.T> t) {
    var ret = ValueOf(dummy).Call(slice<reflectꓸValue>([
        ValueOf((byte)10),
        ValueOf((nint)20),
        ValueOf((byte)30),
        ValueOf(new two(new uintptr[]{40, 50}.array())),
        ValueOf((byte)60),
        ValueOf((float32)70F),
        ValueOf((byte)80)
    ]));
    if (len(ret) != 7) {
        t.Fatalf("Call returned %d values, want 7", len(ret));
    }

    var i = (byte)ret[0].Uint();
    nint j = (nint)ret[1].Int();
    var k = (byte)ret[2].Uint();
    var l = ret[3].Interface()._<two>();
    var m = (byte)ret[4].Uint();
    var n = (float32)ret[5].Float();
    var o = (byte)ret[6].Uint();

    if (i != 10 || j != 20 || k != 30 || l != (new two(new uintptr[]{40, 50}.array())) || m != 60 || n != 70F || o != 80) {
        t.Errorf("Call returned %d, %d, %d, %v, %d, %g, %d; want 10, 20, 30, [40, 50], 60, 70, 80", i, j, k, l, m, n, o);
    }

    foreach (var (iΔ1, v) in ret) {
        if (v.CanAddr()) {
            t.Errorf("result %d is addressable", iΔ1);
        }
    }
}

public static void TestCallConvert(ж<Δtesting.T> t) {
    var v = ValueOf(@new<Δio.ReadWriter>()).Elem();
    var f = ValueOf(Δio.Reader (Δio.Reader r) => r);
    var @out = f.Call(slice<reflectꓸValue>([v]));
    if (len(@out) != 1 || !AreEqual(@out[0].Type(), TypeOf(@new<Δio.Reader>()).Elem()) || !@out[0].IsNil()) {
        t.Errorf("expected [nil], got %v", @out);
    }
}

partial struct emptyStruct {
}

partial struct nonEmptyStruct {
    internal nint member;
}

internal static emptyStruct returnEmpty() {
    return new emptyStruct();
}

internal static void takesEmpty(emptyStruct e) {
}

internal static nonEmptyStruct returnNonEmpty(nint i) {
    return new nonEmptyStruct(member: i);
}

internal static nint takesNonEmpty(nonEmptyStruct n) {
    return n.member;
}

public static void TestCallWithStruct(ж<Δtesting.T> t) {
    var r = ValueOf(returnEmpty).Call(default);
    if (len(r) != 1 || !AreEqual(r[0].Type(), TypeOf(new emptyStruct()))) {
        t.Errorf("returning empty struct returned %#v instead", r);
    }
    r = ValueOf(takesEmpty).Call(slice<reflectꓸValue>([ValueOf(new emptyStruct())]));
    if (len(r) != 0) {
        t.Errorf("takesEmpty returned values: %#v", r);
    }
    r = ValueOf(returnNonEmpty).Call(slice<reflectꓸValue>([ValueOf((nint)42)]));
    if (len(r) != 1 || !AreEqual(r[0].Type(), TypeOf(new nonEmptyStruct())) || r[0].Field(0).Int() != 42) {
        t.Errorf("returnNonEmpty returned %#v", r);
    }
    r = ValueOf(takesNonEmpty).Call(slice<reflectꓸValue>([ValueOf(new nonEmptyStruct(member: 42))]));
    if (len(r) != 1 || !AreEqual(r[0].Type(), TypeOf((nint)1)) || r[0].Int() != 42) {
        t.Errorf("takesNonEmpty returned %#v", r);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object cleanupDidNotRunˢ = (@string)"cleanup did not run"u8;

public static void TestCallReturnsEmpty(ж<Δtesting.T> t) {
    // Issue 21717: past-the-end pointer write in Call with
    // nonzero-sized frame and zero-sized return value.
    Δruntime.GC();
    ref var cleanedUp = ref heap(new atomic.Uint32(), out var ᏑcleanedUp);
    var f = () => {
        var i = Ꮡ(new array<int64>(2)); // big enough to not be tinyalloc'd, so cleanup always runs when i dies
        Δruntime.AddCleanup(i, (ж<atomic.Uint32> cu) => {
            cu.Store((uint32)1);
        }, ᏑcleanedUp);
        return (new emptyStruct(), i);
    };
    var v = ValueOf((f).OrTypedNilFunc()).Call(default)[0]; // out[0] should not alias out[1]'s memory, so the cleanup should run.
    var timeout = time.After((time.Duration)(5000000000L));
    while (ᏑcleanedUp.Load() == 0) {
        var selᴛ2 = timeout;
        switch (trySelect(ᐸꟷ(selᴛ2, ꓸꓸꓸ))) {
        case 0 when selᴛ2.ꟷᐳ(out _): {
            t.Fatal(cleanupDidNotRunˢ);
            break;
        }
        default: {
            break;
        }}
        Δruntime.Gosched();
        Δruntime.GC();
    }
    Δruntime.KeepAlive(v);
}

public static void TestMakeFunc(ж<Δtesting.T> t) {
    ref var f = ref heap<Func<byte, nint, byte, two, byte, float32, byte, (byte i, nint j, byte k, two l, byte m, float32 n, byte o)>>(dummy, out var Ꮡf);
    var fv = MakeFunc(TypeOf((f).OrTypedNilFunc()), (slice<reflectꓸValue> @in) => @in);
    ValueOf(Ꮡf).Elem().Set(fv);

    // Call g with small arguments so that there is
    // something predictable (and different from the
    // correct results) in those positions on the stack.
    var g = dummy;
    g(1, 2, 3, new two(new uintptr[]{4, 5}.array()), 6, 7F, 8);

    // Call constructed function f.
    var (i, j, k, l, m, n, o) = f(10, 20, 30, new two(new uintptr[]{40, 50}.array()), 60, 70F, 80);
    if (i != 10 || j != 20 || k != 30 || l != (new two(new uintptr[]{40, 50}.array())) || m != 60 || n != 70F || o != 80) {
        t.Errorf("Call returned %d, %d, %d, %v, %d, %g, %d; want 10, 20, 30, [40, 50], 60, 70, 80", i, j, k, l, m, n, o);
    }
}

public static void TestMakeFuncInterface(ж<Δtesting.T> t) {
    ref var fn = ref heap<Func<nint, nint>>((nint i) => i, out var Ꮡfn);
    var incr = (slice<reflectꓸValue> @in) => slice<reflectꓸValue>([ValueOf((nint)(@in[0].Int() + 1))]);
    var fv = MakeFunc(TypeOf((fn).OrTypedNilFunc()), incr);
    ValueOf(Ꮡfn).Elem().Set(fv);
    {
        nint r = fn(2); if (r != 3) {
            t.Errorf("Call returned %d, want 3", r);
        }
    }
    {
        var r = fv.Call(slice<reflectꓸValue>([ValueOf((nint)14)]))[0].Int(); if (r != 15) {
            t.Errorf("Call returned %d, want 15", r);
        }
    }
    {
        nint r = fv.Interface()._<Func<nint, nint>>()(26); if (r != 27) {
            t.Errorf("Call returned %d, want 27", r);
        }
    }
}

public static void TestMakeFuncVariadic(ж<Δtesting.T> t) {
    // Test that variadic arguments are packed into a slice and passed as last arg
    ref var fn = ref heap<Funcꓸꓸꓸ<nint, nint, slice<nint>>>(slice<nint> (nint _, params ꓸꓸꓸnint @isʗp) => {
        var @is = @isʗp.sslice();
        return default;
    }, out var Ꮡfn);
    var fv = MakeFunc(TypeOf((fn).OrTypedNilFunc()), (slice<reflectꓸValue> @in) => @in[1..2]);
    ValueOf(Ꮡfn).Elem().Set(fv);

    var r = fn(1, 2, 3);
    if (r[0] != 2 || r[1] != 3) {
        t.Errorf("Call returned [%v, %v]; want 2, 3", r[0], r[1]);
    }

    r = fn(1, slice<nint>([2, 3]).ꓸꓸꓸ);
    if (r[0] != 2 || r[1] != 3) {
        t.Errorf("Call returned [%v, %v]; want 2, 3", r[0], r[1]);
    }

    r = fv.Call(slice<reflectꓸValue>([ValueOf((nint)1), ValueOf((nint)2), ValueOf((nint)3)]))[0].Interface()._<slice<nint>>();
    if (r[0] != 2 || r[1] != 3) {
        t.Errorf("Call returned [%v, %v]; want 2, 3", r[0], r[1]);
    }

    r = fv.CallSlice(slice<reflectꓸValue>([ValueOf((nint)1), ValueOf(slice<nint>([2, 3]))]))[0].Interface()._<slice<nint>>();
    if (r[0] != 2 || r[1] != 3) {
        t.Errorf("Call returned [%v, %v]; want 2, 3", r[0], r[1]);
    }

    var f = fv.Interface()._<Funcꓸꓸꓸ<nint, nint, slice<nint>>>();

    r = f(1, 2, 3);
    if (r[0] != 2 || r[1] != 3) {
        t.Errorf("Call returned [%v, %v]; want 2, 3", r[0], r[1]);
    }
    r = f(1, slice<nint>([2, 3]).ꓸꓸꓸ);
    if (r[0] != 2 || r[1] != 3) {
        t.Errorf("Call returned [%v, %v]; want 2, 3", r[0], r[1]);
    }
}

// Dummy type that implements io.WriteCloser
partial struct WC {
}

public static (nint n, error err) Write(this ref WC w, slice<byte> p) {
    return (0, default);
}

public static error Close(this ref WC w) {
    return default;
}

// Unnamed types should be promotable to named types.
internal partial struct TestMakeFuncValidReturnAssignments_T /*dyn*/ {
    internal nint a, b, c;
}

internal partial struct TestMakeFuncValidReturnAssignments_i /*dyn*/ {
    internal nint a, b, c;
}

public static void TestMakeFuncValidReturnAssignments(ж<Δtesting.T> t) {
    // reflect.Values returned from the wrapped function should be assignment-converted
    // to the types returned by the result of MakeFunc.

    // Concrete types should be promotable to interfaces they implement.
    Func<error> f = default;
    f = MakeFunc(TypeOf((f).OrTypedNilFunc()), (slice<reflectꓸValue> _) => slice<reflectꓸValue>([ValueOf(Δio.EOF)])).Interface()._<Func<error>>();
    f();

    // Super-interfaces should be promotable to simpler interfaces.
    Func<Δio.Writer> g = default;
    g = MakeFunc(TypeOf((g).OrTypedNilFunc()), (slice<reflectꓸValue> _) => {
        ref var w = ref heap<Δio.WriteCloser>(out var Ꮡw);

        w = new reflect_test_package.WCжWriteCloser(Ꮡ(new WC()));
        return slice<reflectꓸValue>([ValueOf(Ꮡw).Elem()]);
    }).Interface()._<Func<Δio.Writer>>();
    g();

    // Channels should be promotable to directional channels.
    Func</*<-*/channel<nint>> h = default;
    h = MakeFunc(TypeOf((h).OrTypedNilFunc()), (slice<reflectꓸValue> _) => slice<reflectꓸValue>([ValueOf(new channel<nint>(0))])).Interface()._<Func</*<-*/channel<nint>>>();
    h();
    Func<TestMakeFuncValidReturnAssignments_T> i = default;
    i = MakeFunc(TypeOf((i).OrTypedNilFunc()), (slice<reflectꓸValue> _) => slice<reflectꓸValue>([ValueOf(new TestMakeFuncValidReturnAssignments_i(a: 1, b: 2, c: 3))])).Interface()._<Func<TestMakeFuncValidReturnAssignments_T>>();
    i();
}

internal partial struct TestMakeFuncInvalidReturnAssignments_T /*dyn*/ {
    internal nint a, b, c;
}

internal partial struct TestMakeFuncInvalidReturnAssignments_U /*dyn*/ {
    internal nint a, b, c;
}

public static void TestMakeFuncInvalidReturnAssignments(ж<Δtesting.T> t) {
    // Type doesn't implement the required interface.
    shouldPanic(""u8, () => {
        Func<error> f = default;
        f = MakeFunc(TypeOf((f).OrTypedNilFunc()), (slice<reflectꓸValue> _) => slice<reflectꓸValue>([ValueOf((nint)7)])).Interface()._<Func<error>>();
        f();
    });
    // Assigning to an interface with additional methods.
    shouldPanic(""u8, () => {
        Func<Δio.ReadWriteCloser> f = default;
        f = MakeFunc(TypeOf((f).OrTypedNilFunc()), (slice<reflectꓸValue> _) => {
            ref var w = ref heap<Δio.WriteCloser>(out var Ꮡw);

            w = new reflect_test_package.WCжWriteCloser(Ꮡ(new WC()));
            return slice<reflectꓸValue>([ValueOf(Ꮡw).Elem()]);
        }).Interface()._<Func<Δio.ReadWriteCloser>>();
        f();
    });
    // Directional channels can't be assigned to bidirectional ones.
    shouldPanic(""u8, () => {
        Func<channel<nint>> f = default;
        f = MakeFunc(TypeOf((f).OrTypedNilFunc()), (slice<reflectꓸValue> _) => {
            /*<-*/channel<nint> c = new channel<nint>(0).AsRecvOnly();
            return slice<reflectꓸValue>([ValueOf(c)]);
        }).Interface()._<Func<channel<nint>>>();
        f();
    });
    // Two named types which are otherwise identical.
    shouldPanic(""u8, () => {
        Func<TestMakeFuncInvalidReturnAssignments_T> f = default;
        f = MakeFunc(TypeOf((f).OrTypedNilFunc()), (slice<reflectꓸValue> _) => slice<reflectꓸValue>([ValueOf(new TestMakeFuncInvalidReturnAssignments_U(a: 1, b: 2, c: 3))])).Interface()._<Func<TestMakeFuncInvalidReturnAssignments_T>>();
        f();
    });
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

// This will be index 5.
public static int64 Int64Method(this ref Point p, int64 x) {
    return x;
}

// This will be index 6.
public static int32 Int32Method(this ref Point p, int32 x) {
    return x;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string distˢ => ref distˢᵇ;
private static readonly @string distˢᵇ = "Dist"u8;
internal static ref readonly @string noArgsˢ => ref noArgsˢᵇ;
private static readonly @string noArgsˢᵇ = "NoArgs"u8;

internal partial interface TestMethod_x /*dyn*/ {
    nint Dist(nint _Δp0);
}

public static void TestMethod(ж<Δtesting.T> t) {
    // Non-curried method of type.
    ref var p = ref heap<Point>(new Point(3, 4), out var Ꮡp);
    var i = TypeOf(p).Method(1).Func.Call(slice<reflectꓸValue>([ValueOf(p), ValueOf((nint)10)]))[0].Int();
    if (i != 250) {
        t.Errorf("Type Method returned %d; want 250", i);
    }

    var (m, ok) = TypeOf(p).MethodByName(distˢ);
    if (!ok) {
        t.Fatalf("method by name failed");
    }
    i = m.Func.Call(slice<reflectꓸValue>([ValueOf(p), ValueOf((nint)11)]))[0].Int();
    if (i != 275) {
        t.Errorf("Type MethodByName returned %d; want 275", i);
    }

    (m, ok) = TypeOf(p).MethodByName(noArgsˢ);
    if (!ok) {
        t.Fatalf("method by name failed");
    }
    nint n = len(m.Func.Call(slice<reflectꓸValue>([ValueOf(p)])));
    if (n != 0) {
        t.Errorf("NoArgs returned %d values; want 0", n);
    }

    i = TypeOf(Ꮡp).Method(1).Func.Call(slice<reflectꓸValue>([ValueOf(Ꮡp), ValueOf((nint)12)]))[0].Int();
    if (i != 300) {
        t.Errorf("Pointer Type Method returned %d; want 300", i);
    }

    (m, ok) = TypeOf(Ꮡp).MethodByName(distˢ);
    if (!ok) {
        t.Fatalf("ptr method by name failed");
    }
    i = m.Func.Call(slice<reflectꓸValue>([ValueOf(Ꮡp), ValueOf((nint)13)]))[0].Int();
    if (i != 325) {
        t.Errorf("Pointer Type MethodByName returned %d; want 325", i);
    }

    (m, ok) = TypeOf(Ꮡp).MethodByName(noArgsˢ);
    if (!ok) {
        t.Fatalf("method by name failed");
    }
    n = len(m.Func.Call(slice<reflectꓸValue>([ValueOf(Ꮡp)])));
    if (n != 0) {
        t.Errorf("NoArgs returned %d values; want 0", n);
    }

    (_, ok) = TypeOf(Ꮡp).MethodByName("AA");
    if (ok) {
        t.Errorf(@"MethodByName(""AA"") should have failed");
    }

    (_, ok) = TypeOf(Ꮡp).MethodByName("ZZ");
    if (ok) {
        t.Errorf(@"MethodByName(""ZZ"") should have failed");
    }

    // Curried method of value.
    var tfunc = TypeOf(((Func<nint, nint>)(default)).OrTypedNilFunc());
    var v = ValueOf(p).Method(1);
    {
        var tt = v.Type(); if (!AreEqual(tt, tfunc)) {
            t.Errorf("Value Method Type is %s; want %s", tt, tfunc);
        }
    }
    i = v.Call(slice<reflectꓸValue>([ValueOf((nint)14)]))[0].Int();
    if (i != 350) {
        t.Errorf("Value Method returned %d; want 350", i);
    }
    v = ValueOf(p).MethodByName(distˢ);
    {
        var tt = v.Type(); if (!AreEqual(tt, tfunc)) {
            t.Errorf("Value MethodByName Type is %s; want %s", tt, tfunc);
        }
    }
    i = v.Call(slice<reflectꓸValue>([ValueOf((nint)15)]))[0].Int();
    if (i != 375) {
        t.Errorf("Value MethodByName returned %d; want 375", i);
    }
    v = ValueOf(p).MethodByName(noArgsˢ);
    v.Call(default);

    // Curried method of pointer.
    v = ValueOf(Ꮡp).Method(1);
    {
        var tt = v.Type(); if (!AreEqual(tt, tfunc)) {
            t.Errorf("Pointer Value Method Type is %s; want %s", tt, tfunc);
        }
    }
    i = v.Call(slice<reflectꓸValue>([ValueOf((nint)16)]))[0].Int();
    if (i != 400) {
        t.Errorf("Pointer Value Method returned %d; want 400", i);
    }
    v = ValueOf(Ꮡp).MethodByName(distˢ);
    {
        var tt = v.Type(); if (!AreEqual(tt, tfunc)) {
            t.Errorf("Pointer Value MethodByName Type is %s; want %s", tt, tfunc);
        }
    }
    i = v.Call(slice<reflectꓸValue>([ValueOf((nint)17)]))[0].Int();
    if (i != 425) {
        t.Errorf("Pointer Value MethodByName returned %d; want 425", i);
    }
    v = ValueOf(Ꮡp).MethodByName(noArgsˢ);
    v.Call(default);

    // Curried method of interface value.
    // Have to wrap interface value in a struct to get at it.
    // Passing it to ValueOf directly would
    // access the underlying Point, not the interface.
    ref var x = ref heap<TestMethod_x>(out var Ꮡx);

    x = p;
    var pv = ValueOf(Ꮡx).Elem();
    v = pv.Method(0);
    {
        var tt = v.Type(); if (!AreEqual(tt, tfunc)) {
            t.Errorf("Interface Method Type is %s; want %s", tt, tfunc);
        }
    }
    i = v.Call(slice<reflectꓸValue>([ValueOf((nint)18)]))[0].Int();
    if (i != 450) {
        t.Errorf("Interface Method returned %d; want 450", i);
    }
    v = pv.MethodByName(distˢ);
    {
        var tt = v.Type(); if (!AreEqual(tt, tfunc)) {
            t.Errorf("Interface MethodByName Type is %s; want %s", tt, tfunc);
        }
    }
    i = v.Call(slice<reflectꓸValue>([ValueOf((nint)19)]))[0].Int();
    if (i != 475) {
        t.Errorf("Interface MethodByName returned %d; want 475", i);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string int64Methodˢ => ref int64Methodˢᵇ;
private static readonly @string int64Methodˢᵇ = "Int64Method"u8;
internal static ref readonly @string int32Methodˢ => ref int32Methodˢᵇ;
private static readonly @string int32Methodˢᵇ = "Int32Method"u8;

internal partial struct TestMethodValue_type /*dyn*/ {
    public TestMethod_x X;
}

public static void TestMethodValue(ж<Δtesting.T> t) {
    ref var p = ref heap<Point>(new Point(3, 4), out var Ꮡp);
    int64 i = default;

    // Check that method value have the same underlying code pointers.
    {
        var (p1, p2) = (ValueOf(new Point(1, 1)).Method(1), ValueOf(new Point(2, 2)).Method(1)); if (p1.Pointer() != p2.Pointer()) {
            t.Errorf("methodValueCall mismatched: %v - %v", p1, p2);
        }
    }

    // Curried method of value.
    var tfunc = TypeOf(((Func<nint, nint>)(default)).OrTypedNilFunc());
    var v = ValueOf(p).Method(1);
    {
        var tt = v.Type(); if (!AreEqual(tt, tfunc)) {
            t.Errorf("Value Method Type is %s; want %s", tt, tfunc);
        }
    }
    i = ValueOf(v.Interface()).Call(slice<reflectꓸValue>([ValueOf((nint)10)]))[0].Int();
    if (i != 250) {
        t.Errorf("Value Method returned %d; want 250", i);
    }
    v = ValueOf(p).MethodByName(distˢ);
    {
        var tt = v.Type(); if (!AreEqual(tt, tfunc)) {
            t.Errorf("Value MethodByName Type is %s; want %s", tt, tfunc);
        }
    }
    i = ValueOf(v.Interface()).Call(slice<reflectꓸValue>([ValueOf((nint)11)]))[0].Int();
    if (i != 275) {
        t.Errorf("Value MethodByName returned %d; want 275", i);
    }
    v = ValueOf(p).MethodByName(noArgsˢ);
    ValueOf(v.Interface()).Call(default);
    v.Interface()._<Action>()();

    // Curried method of pointer.
    v = ValueOf(Ꮡp).Method(1);
    {
        var tt = v.Type(); if (!AreEqual(tt, tfunc)) {
            t.Errorf("Pointer Value Method Type is %s; want %s", tt, tfunc);
        }
    }
    i = ValueOf(v.Interface()).Call(slice<reflectꓸValue>([ValueOf((nint)12)]))[0].Int();
    if (i != 300) {
        t.Errorf("Pointer Value Method returned %d; want 300", i);
    }
    v = ValueOf(Ꮡp).MethodByName(distˢ);
    {
        var tt = v.Type(); if (!AreEqual(tt, tfunc)) {
            t.Errorf("Pointer Value MethodByName Type is %s; want %s", tt, tfunc);
        }
    }
    i = ValueOf(v.Interface()).Call(slice<reflectꓸValue>([ValueOf((nint)13)]))[0].Int();
    if (i != 325) {
        t.Errorf("Pointer Value MethodByName returned %d; want 325", i);
    }
    v = ValueOf(Ꮡp).MethodByName(noArgsˢ);
    ValueOf(v.Interface()).Call(default);
    v.Interface()._<Action>()();

    // Curried method of pointer to pointer.
    ref var pp = ref heap<ж<Point>>(Ꮡp, out var Ꮡpp);
    v = ValueOf(Ꮡpp).Elem().Method(1);
    {
        var tt = v.Type(); if (!AreEqual(tt, tfunc)) {
            t.Errorf("Pointer Pointer Value Method Type is %s; want %s", tt, tfunc);
        }
    }
    i = ValueOf(v.Interface()).Call(slice<reflectꓸValue>([ValueOf((nint)14)]))[0].Int();
    if (i != 350) {
        t.Errorf("Pointer Pointer Value Method returned %d; want 350", i);
    }
    v = ValueOf(Ꮡpp).Elem().MethodByName(distˢ);
    {
        var tt = v.Type(); if (!AreEqual(tt, tfunc)) {
            t.Errorf("Pointer Pointer Value MethodByName Type is %s; want %s", tt, tfunc);
        }
    }
    i = ValueOf(v.Interface()).Call(slice<reflectꓸValue>([ValueOf((nint)15)]))[0].Int();
    if (i != 375) {
        t.Errorf("Pointer Pointer Value MethodByName returned %d; want 375", i);
    }

    // Curried method of interface value.
    // Have to wrap interface value in a struct to get at it.
    // Passing it to ValueOf directly would
    // access the underlying Point, not the interface.
    TestMethodValue_type s = new TestMethodValue_type(p);
    var pv = ValueOf(s).Field(0);
    v = pv.Method(0);
    {
        var tt = v.Type(); if (!AreEqual(tt, tfunc)) {
            t.Errorf("Interface Method Type is %s; want %s", tt, tfunc);
        }
    }
    i = ValueOf(v.Interface()).Call(slice<reflectꓸValue>([ValueOf((nint)16)]))[0].Int();
    if (i != 400) {
        t.Errorf("Interface Method returned %d; want 400", i);
    }
    v = pv.MethodByName(distˢ);
    {
        var tt = v.Type(); if (!AreEqual(tt, tfunc)) {
            t.Errorf("Interface MethodByName Type is %s; want %s", tt, tfunc);
        }
    }
    i = ValueOf(v.Interface()).Call(slice<reflectꓸValue>([ValueOf((nint)17)]))[0].Int();
    if (i != 425) {
        t.Errorf("Interface MethodByName returned %d; want 425", i);
    }

    // For issue #33628: method args are not stored at the right offset
    // on amd64p32.
    var m64 = ValueOf(Ꮡp).MethodByName(int64Methodˢ).Interface()._<Func<int64, int64>>();
    {
        var x = m64(123); if (x != 123) {
            t.Errorf("Int64Method returned %d; want 123", x);
        }
    }
    var m32 = ValueOf(Ꮡp).MethodByName(int32Methodˢ).Interface()._<Func<int32, int32>>();
    {
        var x = m32(456); if (x != 456) {
            t.Errorf("Int32Method returned %d; want 456", x);
        }
    }
}

public static void TestVariadicMethodValue(ж<Δtesting.T> t) {
    var p = new Point(3, 4);
    var points = slice<Point>([new(20, 21), new(22, 23), new(24, 25)]);
    var want = (int64)p.TotalDist(points[0], points[1], points[2]);

    // Variadic method of type.
    var tfunc = TypeOf(((Funcꓸꓸꓸ<Point, Point, nint>)(default)).OrTypedNilFunc());
    {
        var tt = TypeOf(p).Method(4).Type; if (!AreEqual(tt, tfunc)) {
            t.Errorf("Variadic Method Type from TypeOf is %s; want %s", tt, tfunc);
        }
    }

    // Curried method of value.
    tfunc = TypeOf(((Funcꓸꓸꓸ<Point, nint>)(default)).OrTypedNilFunc());
    var v = ValueOf(p).Method(4);
    {
        var tt = v.Type(); if (!AreEqual(tt, tfunc)) {
            t.Errorf("Variadic Method Type is %s; want %s", tt, tfunc);
        }
    }
    var i = ValueOf(v.Interface()).Call(slice<reflectꓸValue>([ValueOf(points[0]), ValueOf(points[1]), ValueOf(points[2])]))[0].Int();
    if (i != want) {
        t.Errorf("Variadic Method returned %d; want %d", i, want);
    }
    i = ValueOf(v.Interface()).CallSlice(slice<reflectꓸValue>([ValueOf(points)]))[0].Int();
    if (i != want) {
        t.Errorf("Variadic Method CallSlice returned %d; want %d", i, want);
    }

    var f = v.Interface()._<Funcꓸꓸꓸ<Point, nint>>();
    i = (int64)f(points[0], points[1], points[2]);
    if (i != want) {
        t.Errorf("Variadic Method Interface returned %d; want %d", i, want);
    }
    i = (int64)f(points.ꓸꓸꓸ);
    if (i != want) {
        t.Errorf("Variadic Method Interface Slice returned %d; want %d", i, want);
    }
}

partial struct DirectIfaceT {
    internal ж<nint> p;
}

public static nint M(this DirectIfaceT d) {
    return d.p.Value;
}

public static void TestDirectIfaceMethod(ж<Δtesting.T> t) {
    ref var x = ref heap<nint>(42, out var Ꮡx);
    ref var v = ref heap<DirectIfaceT>(new DirectIfaceT(Ꮡx), out var Ꮡv);
    var typ = TypeOf(v);
    var (m, ok) = typ.MethodByName("M");
    if (!ok) {
        t.Fatalf("cannot find method M");
    }
    var @in = slice<reflectꓸValue>([ValueOf(v)]);
    var @out = m.Func.Call(@in);
    {
        var got = @out[0].Int(); if (got != 42) {
            t.Errorf("Call with value receiver got %d, want 42", got);
        }
    }

    var pv = Ꮡv;
    typ = TypeOf(pv.OrTypedNil());
    (m, ok) = typ.MethodByName("M");
    if (!ok) {
        t.Fatalf("cannot find method M");
    }
    @in = slice<reflectꓸValue>([ValueOf(pv.OrTypedNil())]);
    @out = m.Func.Call(@in);
    {
        var got = @out[0].Int(); if (got != 42) {
            t.Errorf("Call with pointer receiver got %d, want 42", got);
        }
    }
}

// Reflect version of $GOROOT/test/method5.go
// Concrete types implementing M method.
// Smaller than a word, word-sized, larger than a word.
// Value and pointer receivers.
partial interface Tinter {
    (byte, nint) M(nint _Δp0, byte _Δp1);
}

partial struct Tsmallv /*num:byte*/;

public static (byte, nint) M(this Tsmallv v, nint x, byte b) {
    return (b, x + (nint)v);
}

partial struct Tsmallp /*num:byte*/;

public static (byte, nint) M(this ref Tsmallp p, nint x, byte b) {
    return (b, x + (nint)(p));
}

partial struct Twordv /*num:uintptr*/;

public static (byte, nint) M(this Twordv v, nint x, byte b) {
    return (b, x + (nint)v);
}

partial struct Twordp /*num:uintptr*/;

public static (byte, nint) M(this ref Twordp p, nint x, byte b) {
    return (b, x + (nint)(p));
}

partial struct Tbigv /*[2]uintptr*/;

public static (byte, nint) M(this Tbigv v, nint x, byte b) {
    v = v.Clone();

    return (b, x + (nint)v[0] + (nint)v[1]);
}

partial struct Tbigp /*[2]uintptr*/;

public static (byte, nint) M(this ref Tbigp p, nint x, byte b) {
    return (b, x + (nint)p.Value[0] + (nint)p.Value[1]);
}

partial interface tinter {
    (byte, nint) m(nint _Δp0, byte _Δp1);
}

// Embedding via pointer.
partial struct Tm1 {
    /*embed*/ public Tm2 Tm2;
}

partial struct Tm2 {
    /*embed*/ public ж<Tm3> Tm3;
}

partial struct Tm3 {
    /*embed*/ public ж<Tm4> Tm4;
}

partial struct Tm4 {
}

public static (byte, nint) M(this Tm4 t4, nint x, byte b) {
    return (b, x + 40);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string methodˢ => ref methodˢᵇ;
private static readonly @string methodˢᵇ = "Method"u8;

public static void TestMethod5(ж<Δtesting.T> t) {
    void CheckF(@string name, Func<nint, byte, (byte, nint)> f, nint inc) {
        var (b, x) = f(1000, 99);
        if (b != 99 || x != 1000 + inc) {
            t.Errorf("%s(1000, 99) = %v, %v, want 99, %v", name, b, x, 1000 + inc);
        }
    }

    var CheckFʗ1 = CheckF;
    void CheckV(@string name, reflectꓸValue i, nint inc) {
        var bx = i.Method(0).Call(slice<reflectꓸValue>([ValueOf((nint)1000), ValueOf((byte)99)]));
        var b = bx[0].Interface();
        var x = bx[1].Interface();
        if (!AreEqual(b, (byte)99) || !AreEqual(x, 1000 + inc)) {
            t.Errorf("direct %s.M(1000, 99) = %v, %v, want 99, %v", name, b, x, 1000 + inc);
        }

        CheckFʗ1(name + ".M"u8, i.Method(0).Interface()._<Func<nint, byte, (byte, nint)>>(), inc);
    }

    reflectꓸType TinterType = TypeOf(@new<Tinter>()).Elem();

    var CheckVʗ1 = CheckV;
    var TinterTypeʗ1 = TinterType;
    void CheckI(@string name, any i, nint inc) {
        var v = ValueOf(i);
        CheckVʗ1(name, v, inc);
        CheckVʗ1("(i="u8 + name + ")"u8, v.Convert(TinterTypeʗ1), inc);
    }

    ref var sv = ref heap<Tsmallv>((Tsmallv)1, out var Ꮡsv);
    CheckI("sv", sv, 1);
    CheckI("&sv", Ꮡsv, 1);

    ref var sp = ref heap<Tsmallp>((Tsmallp)2, out var Ꮡsp);
    CheckI("&sp", Ꮡsp, 2);

    ref var wv = ref heap<Twordv>((Twordv)3, out var Ꮡwv);
    CheckI("wv", wv, 3);
    CheckI("&wv", Ꮡwv, 3);

    ref var wp = ref heap<Twordp>((Twordp)4, out var Ꮡwp);
    CheckI("&wp", Ꮡwp, 4);

    ref var bv = ref heap<Tbigv>(((Tbigv)new uintptr[]{5, 6}.array()), out var Ꮡbv);
    CheckI("bv", bv, 11);
    CheckI("&bv", Ꮡbv, 11);

    ref var bp = ref heap<Tbigp>(((Tbigp)new uintptr[]{7, 8}.array()), out var Ꮡbp);
    CheckI("&bp", Ꮡbp, 15);

    ref var t4 = ref heap<Tm4>(new Tm4(), out var Ꮡt4);
    ref var t3 = ref heap<Tm3>(new Tm3(Ꮡt4), out var Ꮡt3);
    ref var t2 = ref heap<Tm2>(new Tm2(Ꮡt3), out var Ꮡt2);
    ref var t1 = ref heap<Tm1>(new Tm1(t2), out var Ꮡt1);
    CheckI("t4", t4, 40);
    CheckI("&t4", Ꮡt4, 40);
    CheckI("t3", t3, 40);
    CheckI("&t3", Ꮡt3, 40);
    CheckI("t2", t2, 40);
    CheckI("&t2", Ꮡt2, 40);
    CheckI("t1", t1, 40);
    CheckI("&t1", Ꮡt1, 40);

    ref var tnil = ref heap<Tinter>(out var Ꮡtnil);
    ref var vnil = ref heap<reflectꓸValue>(ValueOf(Ꮡtnil).Elem(), out var Ꮡvnil);
    var vnilʗ1 = vnil;
    shouldPanic(methodˢ, () => {
        vnilʗ1.Method(0);
    });
}

internal partial struct TestInterfaceSet_s /*dyn*/ {
    public any I;
    public TestMethod_x P;
}

public static void TestInterfaceSet(ж<Δtesting.T> t) {
    var p = Ꮡ(new Point(3, 4));

    ref var s = ref heap(new TestInterfaceSet_s(), out var Ꮡs);
    var sv = ValueOf(Ꮡs).Elem();
    sv.Field(0).Set(ValueOf(p.OrTypedNil()));
    {
        var q = s.I._<ж<Point>>(); if (q != p) {
            t.Errorf("i: have %p want %p", q.OrTypedNil(), p.OrTypedNil());
        }
    }

    var pv = sv.Field(1);
    pv.Set(ValueOf(p.OrTypedNil()));
    {
        var q = s.P._<ж<Point>>(); if (q != p) {
            t.Errorf("i: have %p want %p", q.OrTypedNil(), p.OrTypedNil());
        }
    }

    var i = pv.Method(0).Call(slice<reflectꓸValue>([ValueOf((nint)10)]))[0].Int();
    if (i != 250) {
        t.Errorf("Interface Method returned %d; want 250", i);
    }
}

partial struct T1 {
    internal @string a;
    /*embed*/ internal nint @int;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string intˢ => ref intˢᵇ;
private static readonly @string intˢᵇ = "int"u8;
internal static readonly object noFieldIntˢ = (@string)"no field 'int'"u8;
internal static readonly object fieldIndexShouldBe1Isˢ = (@string)"field index should be 1; is"u8;

public static void TestAnonymousFields(ж<Δtesting.T> t) {
    Δreflect.StructField field = default;
    bool ok = default;
    T1 t1 = default;
    var type1 = TypeOf(t1);
    {
        (field, ok) = type1.FieldByName(intˢ); if (!ok) {
            t.Fatal(noFieldIntˢ);
        }
    }
    if (field.Index[0] != 1) {
        t.Error(fieldIndexShouldBe1Isˢ, field.Index);
    }
}

partial struct FTest {
    internal any s;
    internal @string name;
    internal slice<nint> index;
    internal nint value;
}

partial struct D1 {
    internal nint d;
}

partial struct D2 {
    internal nint d;
}

partial struct S0 {
    public nint A, B, C;
    /*embed*/ public D1 D1;
    /*embed*/ public D2 D2;
}

partial struct S1 {
    public nint B;
    /*embed*/ public S0 S0;
}

partial struct S2 {
    public nint A;
    /*embed*/ public ж<S1> S1;
}

partial struct S1x {
    /*embed*/ public S1 S1;
}

partial struct S1y {
    /*embed*/ public S1 S1;
}

partial struct S3 {
    /*embed*/ public S1x S1x;
    /*embed*/ public S2 S2;
    public nint D, E;
    /*embed*/ public ж<S1y> S1y;
}

partial struct S4 {
    /*embed*/ public ж<S4> ΔS4;
    public nint A;
}

// The X in S6 and S7 annihilate, but they also block the X in S8.S9.
partial struct S5 {
    /*embed*/ public S6 S6;
    /*embed*/ public S7 S7;
    /*embed*/ public S8 S8;
}

partial struct S6 {
    public nint X;
}

partial struct S7 /*S6*/;

partial struct S8 {
    /*embed*/ public S9 S9;
}

partial struct S9 {
    public nint X;
    public nint Y;
}

// The X in S11.S6 and S12.S6 annihilate, but they also block the X in S13.S8.S9.
partial struct S10 {
    /*embed*/ public S11 S11;
    /*embed*/ public S12 S12;
    /*embed*/ public S13 S13;
}

partial struct S11 {
    /*embed*/ public S6 S6;
}

partial struct S12 {
    /*embed*/ public S6 S6;
}

partial struct S13 {
    /*embed*/ public S8 S8;
}

// The X in S15.S11.S1 and S16.S11.S1 annihilate.
partial struct S14 {
    /*embed*/ public S15 S15;
    /*embed*/ public S16 S16;
}

partial struct S15 {
    /*embed*/ public S11 S11;
}

partial struct S16 {
    /*embed*/ public S11 S11;
}

internal static slice<FTest> fieldTests = slice<FTest>([
    new(new EmptyStruct(), "", default, 0),
    new(new EmptyStruct(), "Foo", default, 0),
    new(new S0(A: 'a'), "A", slice<nint>([0]), 'a'),
    new(new S0(), "D", default, 0),
    new(new S1(S0: new S0(A: 'a')), "A", slice<nint>([1, 0]), 'a'),
    new(new S1(B: 'b'), "B", slice<nint>([0]), 'b'),
    new(new S1(), "S0", slice<nint>([1]), 0),
    new(new S1(S0: new S0(C: 'c')), "C", slice<nint>([1, 2]), 'c'),
    new(new S2(A: 'a'), "A", slice<nint>([0]), 'a'),
    new(new S2(), "S1", slice<nint>([1]), 0),
    new(new S2(S1: Ꮡ(new S1(B: 'b'))), "B", slice<nint>([1, 0]), 'b'),
    new(new S2(S1: Ꮡ(new S1(S0: new S0(C: 'c')))), "C", slice<nint>([1, 1, 2]), 'c'),
    new(new S2(), "D", default, 0),
    new(new S3(), "S1", default, 0),
    new(new S3(S2: new S2(A: 'a')), "A", slice<nint>([1, 0]), 'a'),
    new(new S3(), "B", default, 0),
    new(new S3(D: 'd'), "D", slice<nint>([2]), 0),
    new(new S3(E: 'e'), "E", slice<nint>([3]), 'e'),
    new(new S4(A: 'a'), "A", slice<nint>([1]), 'a'),
    new(new S4(), "B", default, 0),
    new(new S5(), "X", default, 0),
    new(new S5(), "Y", slice<nint>([2, 0, 1]), 0),
    new(new S10(), "X", default, 0),
    new(new S10(), "Y", slice<nint>([2, 0, 0, 1]), 0),
    new(new S14(), "X", default, 0)
]);

public static void TestFieldByIndex(ж<Δtesting.T> t) {
    foreach (var (_, test) in fieldTests) {
        var s = TypeOf(test.s);
        var f = s.FieldByIndex(test.index);
        if (f.Name != ""u8) {
            if (test.index != nil) {
                if (f.Name != test.name) {
                    t.Errorf("%s.%s found; want %s", s.Name(), f.Name, test.name);
                }
            } else {
                t.Errorf("%s.%s found", s.Name(), f.Name);
            }
        } else if (len(test.index) > 0) {
            t.Errorf("%s.%s not found", s.Name(), test.name);
        }

        if (test.value != 0) {
            var v = ValueOf(test.s).FieldByIndex(test.index);
            if (v.IsValid()) {
                {
                    var (x, ok) = v.Interface()._<nint>(ᐧ); if (ok) {
                        if (x != test.value) {
                            t.Errorf("%s%v is %d; want %d", s.Name(), test.index, x, test.value);
                        }
                    } else {
                        t.Errorf("%s%v value not an int", s.Name(), test.index);
                    }
                }
            } else {
                t.Errorf("%s%v value not found", s.Name(), test.index);
            }
        }
    }
}

public static void TestFieldByName(ж<Δtesting.T> t) {
    foreach (var (_, test) in fieldTests) {
        var s = TypeOf(test.s);
        var (f, found) = s.FieldByName(test.name);
        if (found) {
            if (test.index != nil) {
                // Verify field depth and index.
                if (len(f.Index) != len(test.index)) {
                    t.Errorf("%s.%s depth %d; want %d: %v vs %v", s.Name(), test.name, len(f.Index), len(test.index), f.Index, test.index);
                } else {
                    foreach (var (i, x) in f.Index) {
                        if (x != test.index[i]) {
                            t.Errorf("%s.%s.Index[%d] is %d; want %d", s.Name(), test.name, i, x, test.index[i]);
                        }
                    }
                }
            } else {
                t.Errorf("%s.%s found", s.Name(), f.Name);
            }
        } else if (len(test.index) > 0) {
            t.Errorf("%s.%s not found", s.Name(), test.name);
        }

        if (test.value != 0) {
            var v = ValueOf(test.s).FieldByName(test.name);
            if (v.IsValid()) {
                {
                    var (x, ok) = v.Interface()._<nint>(ᐧ); if (ok) {
                        if (x != test.value) {
                            t.Errorf("%s.%s is %d; want %d", s.Name(), test.name, x, test.value);
                        }
                    } else {
                        t.Errorf("%s.%s value not an int", s.Name(), test.name);
                    }
                }
            } else {
                t.Errorf("%s.%s value not found", s.Name(), test.name);
            }
        }
    }
}

internal partial struct TestImportPath_tests /*dyn*/ {
    internal reflectꓸType t;
    internal @string path;
}

public static void TestImportPath(ж<Δtesting.T> Ꮡt) {
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
        new(TypeOf((ж<Point>)nil).Elem(), "reflect_test")
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
internal static ref readonly @string testStructˢ => ref testStructˢᵇ;
private static readonly @string testStructˢᵇ = "testStruct"u8;
internal static ref readonly @string localOtherPkgFieldsˢ => ref localOtherPkgFieldsˢᵇ;
private static readonly @string localOtherPkgFieldsˢᵇ = "localOtherPkgFields"u8;

internal partial struct TestFieldPkgPath_x /*num:nint*/;

internal partial struct TestFieldPkgPath_i /*dyn*/ {
    public @string Exported;
    internal @string unexported;
    /*embed*/ public global::go.reflect_internal_test_package.OtherPkgFields OtherPkgFields;
    /*embed*/ internal nint @int;               // issue 21702
    /*embed*/ internal ж<TestFieldPkgPath_x> x; // issue 21122
}

internal partial struct TestFieldPkgPath_pkgpathTest /*dyn*/ {
    internal slice<nint> index;
    internal @string pkgPath;
    internal bool embedded;
    internal bool exported;
}

internal partial struct TestFieldPkgPath_localOtherPkgFields /*global::go.reflect_internal_test_package.OtherPkgFields*/;

public static void TestFieldPkgPath(ж<Δtesting.T> t) {
    ref var typ = ref heap<reflectꓸType>(TypeOf(new TestFieldPkgPath_i()), out var Ꮡtyp);

    void checkPkgPath(@string name, slice<TestFieldPkgPath_pkgpathTest> s) {
        foreach (var (_, test) in s) {
            var f = Ꮡtyp.ValueSlot.FieldByIndex(test.index);
            {
                @string got = f.PkgPath;
                @string want = test.pkgPath; if (got != want) {
                    t.Errorf("%s: Field(%d).PkgPath = %q, want %q", name, test.index, got, want);
                }
            }
            {
                var (got, want) = (f.Anonymous, test.embedded); if (got != want) {
                    t.Errorf("%s: Field(%d).Anonymous = %v, want %v", name, test.index, got, want);
                }
            }
            {
                var (got, want) = (f.IsExported(), test.exported); if (got != want) {
                    t.Errorf("%s: Field(%d).IsExported = %v, want %v", name, test.index, got, want);
                }
            }
        }
    }

    checkPkgPath(testStructˢ, slice<TestFieldPkgPath_pkgpathTest>([
        new(slice<nint>([0]), "", false, true),              // Exported
        new(slice<nint>([1]), "reflect_test", false, false), // unexported
        new(slice<nint>([2]), "", true, true),               // OtherPkgFields
        new(slice<nint>([2, 0]), "", false, true),           // OtherExported
        new(slice<nint>([2, 1]), "reflect", false, false),   // otherUnexported
        new(slice<nint>([3]), "reflect_test", true, false),  // int
        new(slice<nint>([4]), "reflect_test", true, false)   // *x
    ]));
    typ = TypeOf(new TestFieldPkgPath_localOtherPkgFields(new global::go.reflect_internal_test_package.OtherPkgFields()));
    checkPkgPath(localOtherPkgFieldsˢ, slice<TestFieldPkgPath_pkgpathTest>([
        new(slice<nint>([0]), "", false, true),        // OtherExported
        new(slice<nint>([1]), "reflect", false, false) // otherUnexported
    ]));
}

internal partial interface TestMethodPkgPath_I /*dyn*/ {
    void x();
    void X();
}

internal partial interface TestMethodPkgPath_i /*dyn*/ :
    TestMethodPkgPath_I
{
    void y();
    void Y();
}

internal partial struct TestMethodPkgPath_tests /*dyn*/ {
    internal @string name;
    internal @string pkgPath;
    internal bool exported;
}

public static void TestMethodPkgPath(ж<Δtesting.T> t) {
    var typ = TypeOf((ж<TestMethodPkgPath_i>)nil).Elem();

    var tests = slice<TestMethodPkgPath_tests>([
        new("X", "", true),
        new("Y", "", true),
        new("x", "reflect_test", false),
        new("y", "reflect_test", false)
    ]);

    foreach (var (_, test) in tests) {
        var (m, _) = typ.MethodByName(test.name);
        {
            @string got = m.PkgPath;
            @string want = test.pkgPath; if (got != want) {
                t.Errorf("MethodByName(%q).PkgPath = %q, want %q", test.name, got, want);
            }
        }
        {
            var (got, want) = (m.IsExported(), test.exported); if (got != want) {
                t.Errorf("MethodByName(%q).IsExported = %v, want %v", test.name, got, want);
            }
        }
    }
}

public static void TestVariadicType(ж<Δtesting.T> t) {
    // Test example from Type documentation.
    Actionꓸꓸꓸ<nint, float64> f = default;
    var typ = TypeOf((f).OrTypedNilFunc());
    if (typ.NumIn() == 2 && AreEqual(typ.In(0), TypeOf((nint)0))) {
        var sl = typ.In(1);
        if (sl.Kind() == ΔSlice) {
            if (AreEqual(sl.Elem(), TypeOf(0.0D))) {
                // ok
                return;
            }
        }
    }

    // Failed
    t.Errorf("want NumIn() = 2, In(0) = int, In(1) = []float64");
    @string s = fmt.Sprintf("have NumIn() = %d"u8, typ.NumIn());
    for (nint i = 0; i < typ.NumIn(); i++) {
        s += fmt.Sprintf(", In(%d) = %s"u8, i, typ.In(i));
    }
    t.Error(s);
}

partial struct inner {
    internal nint x;
}

partial struct outer {
    internal nint y;
    /*embed*/ internal inner inner;
}

internal static void M(this ref inner _) {
}

internal static void M(this ref outer _) {
}

public static void TestNestedMethods(ж<Δtesting.T> t) {
    var typ = TypeOf((ж<outer>)nil);
    if (typ.NumMethod() != 1 || (uintptr)typ.Method(0).Func.UnsafePointer() != (uintptr)ValueOf(((Action<ж<outer>>)(M))).UnsafePointer()) {
        t.Errorf("Wrong method table for outer: (M=%p)", ((Action<ж<outer>>)(M)));
        for (nint i = 0; i < typ.NumMethod(); i++) {
            var m = typ.Method(i);
            t.Errorf("\t%d: %s %p\n", i, m.Name, @unsafe.Pointer.OrTypedNil((uintptr)m.Func.UnsafePointer()));
        }
    }
}

partial struct unexp {
}

internal static (int32, int8) f(this ref unexp _) {
    return (7, 7);
}

internal static (int64, int8) g(this ref unexp _) {
    return (8, 8);
}

partial interface unexpI {
    (int32, int8) f();
}

public static void TestUnexportedMethods(ж<Δtesting.T> t) {
    var typ = TypeOf(@new<unexp>());
    {
        nint got = typ.NumMethod(); if (got != 0) {
            t.Errorf("NumMethod=%d, want 0 satisfied methods", got);
        }
    }

    typ = TypeOf((ж<unexpI>)nil);
    {
        nint got = typ.Elem().NumMethod(); if (got != 1) {
            t.Errorf("NumMethod=%d, want 1 satisfied methods", got);
        }
    }
}

partial struct InnerInt {
    public nint X;
}

partial struct OuterInt {
    public nint Y;
    /*embed*/ public InnerInt InnerInt;
}

public static nint M(this ref InnerInt i) {
    return i.X;
}

public static void TestEmbeddedMethods(ж<Δtesting.T> t) {
    var typ = TypeOf((ж<OuterInt>)nil);
    if (typ.NumMethod() != 1 || (uintptr)typ.Method(0).Func.UnsafePointer() != (uintptr)ValueOf(((Func<ж<OuterInt>, nint>)(M))).UnsafePointer()) {
        t.Errorf("Wrong method table for OuterInt: (m=%p)", ((Func<ж<OuterInt>, nint>)(M)));
        for (nint iΔ1 = 0; iΔ1 < typ.NumMethod(); iΔ1++) {
            var m = typ.Method(iΔ1);
            t.Errorf("\t%d: %s %p\n", iΔ1, m.Name, @unsafe.Pointer.OrTypedNil((uintptr)m.Func.UnsafePointer()));
        }
    }

    var i = Ꮡ(new InnerInt(3));
    {
        var v = ValueOf(i.OrTypedNil()).Method(0).Call(default)[0].Int(); if (v != 3) {
            t.Errorf("i.M() = %d, want 3", v);
        }
    }

    var o = Ꮡ(new OuterInt(1, new InnerInt(2)));
    {
        var v = ValueOf(o.OrTypedNil()).Method(0).Call(default)[0].Int(); if (v != 2) {
            t.Errorf("i.M() = %d, want 2", v);
        }
    }

    var f = ((Func<ж<OuterInt>, nint>)(M));
    {
        nint v = f(o); if (v != 2) {
            t.Errorf("f(o) = %d, want 2", v);
        }
    }
}

public delegate error FuncDDD(params ꓸꓸꓸany _ʗp);

public static void M(this FuncDDD f) {
}

public static void TestNumMethodOnDDD(ж<Δtesting.T> t) {
    var rv = ValueOf((default(FuncDDD)!).OrTypedNilFunc());
    {
        nint n = rv.NumMethod(); if (n != 1) {
            t.Fatalf("NumMethod()=%d, want 1", n);
        }
    }
}

public static void TestPtrTo(ж<Δtesting.T> t) {
    // This block of code means that the ptrToThis field of the
    // reflect data for *unsafe.Pointer is non zero, see
    // https://golang.org/issue/19003
    ref var x = ref heap<@unsafe.Pointer>(out var Ꮡx);
    ref var y = ref heap<ж<@unsafe.Pointer>>(out var Ꮡy);

    y = Ꮡx;
    ж<ж<@unsafe.Pointer>> z = Ꮡy;

    nint i = default;

    var typ = TypeOf(z.OrTypedNil());
    for (i = 0; i < 100; i++) {
        typ = PointerTo(typ);
    }
    for (i = 0; i < 100; i++) {
        typ = typ.Elem();
    }
    if (!AreEqual(typ, TypeOf(z.OrTypedNil()))) {
        t.Errorf("after 100 PointerTo and Elem, have %s, want %s", typ, TypeOf(z.OrTypedNil()));
    }
}

internal partial class TestPtrToGC_T /*ж<uintptr>*/;

public static void TestPtrToGC(ж<Δtesting.T> t) {
    var xᴷ = new stackAppend<any>(moves: true);

    var tt = TypeOf((TestPtrToGC_T)nil);
    var pt = PointerTo(tt);
    const nint n = 100;
    slice<any> x = default;
    for (nint i = 0; i < n; i++) {
        var v = New(pt);
        var p = @new<ж<uintptr>>();
        p.ValueSlot = @new<uintptr>();
        (p.ValueSlot).Value = (uintptr)i;
        v.Elem().Set(ValueOf(p.OrTypedNil()).Convert(pt));
        x = append(xᴷ, x, v.Interface());
    }
    Δruntime.GC();

    x = move2heap(xᴷ, x);
    foreach (var (i, xi) in x) {
        var k = ValueOf(xi).Elem().Elem().Elem().Interface()._<uintptr>();
        if (k != (uintptr)i) {
            t.Errorf("lost x[%d] = %d, want %d", i, k, i);
        }
    }
}

internal partial struct TestAddr_p /*dyn*/ {
    public nint X, Y;
}

internal partial struct TestAddr_s /*dyn*/ {
    public ж<bool> B;
}

public static void TestAddr(ж<Δtesting.T> t) {
    ref var p = ref heap(new TestAddr_p(), out var Ꮡp);

    var v = ValueOf(Ꮡp);
    v = v.Elem();
    v = v.Addr();
    v = v.Elem();
    v = v.Field(0);
    v.SetInt(2);
    if (p.X != 2) {
        t.Errorf("Addr.Elem.Set failed to set value");
    }

    // Again but take address of the ValueOf value.
    // Exercises generation of PtrTypes not present in the binary.
    ref var q = ref heap<ж<TestAddr_p>>(Ꮡp, out var Ꮡq);
    v = ValueOf(Ꮡq).Elem();
    v = v.Addr();
    v = v.Elem();
    v = v.Elem();
    v = v.Addr();
    v = v.Elem();
    v = v.Field(0);
    v.SetInt(3);
    if (p.X != 3) {
        t.Errorf("Addr.Elem.Set failed to set value");
    }

    // Starting without pointer we should get changed value
    // in interface.
    ref var qq = ref heap<TestAddr_p>(p, out var Ꮡqq);
    v = ValueOf(Ꮡqq).Elem();
    var v0 = v;
    v = v.Addr();
    v = v.Elem();
    v = v.Field(0);
    v.SetInt(4);
    if (p.X != 3) { // should be unchanged from last time
        t.Errorf("somehow value Set changed original p");
    }
    p = v0.Interface()._<TestAddr_p>();
    if (p.X != 4) {
        t.Errorf("Addr.Elem.Set valued to set value in top value");
    }

    // Verify that taking the address of a type gives us a pointer
    // which we can convert back using the usual interface
    // notation.
    ref var s = ref heap(new TestAddr_s(), out var Ꮡs);
    var ps = ValueOf(Ꮡs).Elem().Field(0).Addr().Interface();
    (ps._<ж<ж<bool>>>()).ValueSlot = @new<bool>();
    if (s.B == nil) {
        t.Errorf("Addr.Interface direct assignment failed");
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingMallocCountInˢ = (@string)"skipping malloc count in short mode"u8;
internal static readonly object skippingGomaxprocs1ˢ = (@string)"skipping; GOMAXPROCS>1"u8;

internal static void noAlloc(ж<Δtesting.T> t, nint n, Action<nint> f) {
    if (Δtesting.Short()) {
        t.Skip(skippingMallocCountInˢ);
    }
    if (Δruntime.GOMAXPROCS(0) > 1) {
        t.Skip(skippingGomaxprocs1ˢ);
    }
    nint i = -1;
    var allocs = Δtesting.AllocsPerRun(n, () => {
        f(i);
        i++;
    });
    if (allocs > 0D) {
        t.Errorf("%d iterations: got %v mallocs, want 0", n, allocs);
    }
}

internal partial struct TestAllocations_type /*dyn*/ {
    internal nint f;
}

public static void TestAllocations(ж<Δtesting.T> t) {
    noAlloc(t, 100, (nint j) => {
        any i = default;
        reflectꓸValue v = new(nil);

        i = 42 + j;
        v = ValueOf(i);
        if ((nint)v.Int() != 42 + j) {
            throw panic("wrong int");
        }
    });
    noAlloc(t, 100, (nint j) => {
        any i = default;
        reflectꓸValue v = new(nil);
        i = new nint[]{j, j, j}.array();
        v = ValueOf(i);
        if (v.Len() != 3) {
            throw panic("wrong length");
        }
    });
    noAlloc(t, 100, (nint j) => {
        any i = default;
        reflectꓸValue v = new(nil);
        i = nint (nint jΔ1) => jΔ1;
        v = ValueOf(i);
        if (v.Interface()._<Func<nint, nint>>()(j) != j) {
            throw panic("wrong result");
        }
    });
    if (Δruntime.GOOS != "js"u8 && Δruntime.GOOS != "wasip1"u8) {
        var typ = TypeFor<TestAllocations_type>();
        var typʗ1 = typ;
        noAlloc(t, 100, (nint _) => {
            if (typʗ1.Field(0).Index[0] != 0) {
                throw panic("wrong field index");
            }
        });
    }
}

public static void TestSmallNegativeInt(ж<Δtesting.T> t) {
    var i = (int16)(-1);
    var v = ValueOf(i);
    if (v.Int() != -1) {
        t.Errorf("int16(-1).Int() returned %v", v.Int());
    }
}

public static void TestIndex(ж<Δtesting.T> t) {
    var xs = slice<byte>([1, 2, 3, 4, 5, 6, 7, 8]);
    var v = ValueOf(xs).Index(3).Interface()._<byte>();
    if (v != xs[3]) {
        t.Errorf("xs.Index(3) = %v; expected %v", v, xs[3]);
    }
    var xa = new byte[]{10, 20, 30, 40, 50, 60, 70, 80}.array();
    v = ValueOf(xa).Index(2).Interface()._<byte>();
    if (v != xa[2]) {
        t.Errorf("xa.Index(2) = %v; expected %v", v, xa[2]);
    }
    @string s = "0123456789";
    v = ValueOf(s).Index(3).Interface()._<byte>();
    if (v != s[3]) {
        t.Errorf("s.Index(3) = %v; expected %v", v, s[3]);
    }
}

public static void TestSlice(ж<Δtesting.T> t) {
    ref var xs = ref heap<slice<nint>>(slice<nint>([1, 2, 3, 4, 5, 6, 7, 8]), out var Ꮡxs);
    var v = ValueOf(xs).Slice(3, 5).Interface()._<slice<nint>>();
    if (len(v) != 2) {
        t.Errorf("len(xs.Slice(3, 5)) = %d", len(v));
    }
    if (cap(v) != 5) {
        t.Errorf("cap(xs.Slice(3, 5)) = %d", cap(v));
    }
    if (!DeepEqual(v[0..5], xs[3..])) {
        t.Errorf("xs.Slice(3, 5)[0:5] = %v", v[0..5]);
    }
    ref var xa = ref heap<array<nint>>(new nint[]{10, 20, 30, 40, 50, 60, 70, 80}.array(), out var Ꮡxa);
    v = ValueOf(Ꮡxa).Elem().Slice(2, 5).Interface()._<slice<nint>>();
    if (len(v) != 3) {
        t.Errorf("len(xa.Slice(2, 5)) = %d", len(v));
    }
    if (cap(v) != 6) {
        t.Errorf("cap(xa.Slice(2, 5)) = %d", cap(v));
    }
    if (!DeepEqual(v[0..6], xa[2..])) {
        t.Errorf("xs.Slice(2, 5)[0:6] = %v", v[0..6]);
    }
    @string s = "0123456789";
    @string vs = ValueOf(s).Slice(3, 5).Interface()._<@string>();
    if (vs != s[3..5]) {
        t.Errorf("s.Slice(3, 5) = %q; expected %q", vs, s[3..5]);
    }

    var rv = ValueOf(Ꮡxs).Elem();
    rv = rv.Slice(3, 4);
    @unsafe.Pointer ptr2 = (uintptr)rv.UnsafePointer();
    rv = rv.Slice(5, 5);
    @unsafe.Pointer ptr3 = (uintptr)rv.UnsafePointer();
    if (ptr3 != ptr2) {
        t.Errorf("xs.Slice(3,4).Slice3(5,5).UnsafePointer() = %p, want %p", @unsafe.Pointer.OrTypedNil(ptr3), @unsafe.Pointer.OrTypedNil(ptr2));
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string slice3ˢ => ref slice3ˢᵇ;
private static readonly @string slice3ˢᵇ = "Slice3"u8;

public static void TestSlice3(ж<Δtesting.T> t) {
    ref var xs = ref heap<slice<nint>>(slice<nint>([1, 2, 3, 4, 5, 6, 7, 8]), out var Ꮡxs);
    var v = ValueOf(xs).Slice3(3, 5, 7).Interface()._<slice<nint>>();
    if (len(v) != 2) {
        t.Errorf("len(xs.Slice3(3, 5, 7)) = %d", len(v));
    }
    if (cap(v) != 4) {
        t.Errorf("cap(xs.Slice3(3, 5, 7)) = %d", cap(v));
    }
    if (!DeepEqual(v[0..4], xs.slice(3, 7, 7))) {
        t.Errorf("xs.Slice3(3, 5, 7)[0:4] = %v", v[0..4]);
    }
    ref var rv = ref heap<reflectꓸValue>(ValueOf(Ꮡxs).Elem(), out var Ꮡrv);
    shouldPanic(slice3ˢ, () => {
        Ꮡrv.Value.Slice3(1, 2, 1);
    });
    shouldPanic(slice3ˢ, () => {
        Ꮡrv.Value.Slice3(1, 1, 11);
    });
    shouldPanic(slice3ˢ, () => {
        Ꮡrv.Value.Slice3(2, 2, 1);
    });

    ref var xa = ref heap<array<nint>>(new nint[]{10, 20, 30, 40, 50, 60, 70, 80}.array(), out var Ꮡxa);
    v = ValueOf(Ꮡxa).Elem().Slice3(2, 5, 6).Interface()._<slice<nint>>();
    if (len(v) != 3) {
        t.Errorf("len(xa.Slice(2, 5, 6)) = %d", len(v));
    }
    if (cap(v) != 4) {
        t.Errorf("cap(xa.Slice(2, 5, 6)) = %d", cap(v));
    }
    if (!DeepEqual(v[0..4], xa.slice(2, 6, 6))) {
        t.Errorf("xs.Slice(2, 5, 6)[0:4] = %v", v[0..4]);
    }
    rv = ValueOf(Ꮡxa).Elem();
    shouldPanic(slice3ˢ, () => {
        Ꮡrv.Value.Slice3(1, 2, 1);
    });
    shouldPanic(slice3ˢ, () => {
        Ꮡrv.Value.Slice3(1, 1, 11);
    });
    shouldPanic(slice3ˢ, () => {
        Ꮡrv.Value.Slice3(2, 2, 1);
    });

    ref var s = ref heap<@string>(helloWorldˢ, out var Ꮡs);
    rv = ValueOf(Ꮡs).Elem();
    shouldPanic(slice3ˢ, () => {
        Ꮡrv.Value.Slice3(1, 2, 3);
    });

    rv = ValueOf(Ꮡxs).Elem();
    rv = rv.Slice3(3, 5, 7);
    @unsafe.Pointer ptr2 = (uintptr)rv.UnsafePointer();
    rv = rv.Slice3(4, 4, 4);
    @unsafe.Pointer ptr3 = (uintptr)rv.UnsafePointer();
    if (ptr3 != ptr2) {
        t.Errorf("xs.Slice3(3,5,7).Slice3(4,4,4).UnsafePointer() = %p, want %p", @unsafe.Pointer.OrTypedNil(ptr3), @unsafe.Pointer.OrTypedNil(ptr2));
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string setLenˢ => ref setLenˢᵇ;
private static readonly @string setLenˢᵇ = "SetLen"u8;
internal static ref readonly @string setCapˢ => ref setCapˢᵇ;
private static readonly @string setCapˢᵇ = "SetCap"u8;

public static void TestSetLenCap(ж<Δtesting.T> t) {
    ref var xs = ref heap<slice<nint>>(slice<nint>([1, 2, 3, 4, 5, 6, 7, 8]), out var Ꮡxs);
    ref var xa = ref heap<array<nint>>(new nint[]{10, 20, 30, 40, 50, 60, 70, 80}.array(), out var Ꮡxa);

    ref var vs = ref heap<reflectꓸValue>(ValueOf(Ꮡxs).Elem(), out var Ꮡvs);
    var vsʗ1 = vs;
    shouldPanic(setLenˢ, () => {
        vsʗ1.SetLen(10);
    });
    var vsʗ2 = vs;
    shouldPanic(setCapˢ, () => {
        vsʗ2.SetCap(10);
    });
    var vsʗ3 = vs;
    shouldPanic(setLenˢ, () => {
        vsʗ3.SetLen(-1);
    });
    var vsʗ4 = vs;
    shouldPanic(setCapˢ, () => {
        vsʗ4.SetCap(-1);
    });
    var vsʗ5 = vs;
    shouldPanic(setCapˢ, () => {
        vsʗ5.SetCap(6);
    }); // smaller than len
    vs.SetLen(5);
    if (len(xs) != 5 || cap(xs) != 8) {
        t.Errorf("after SetLen(5), len, cap = %d, %d, want 5, 8", len(xs), cap(xs));
    }
    vs.SetCap(6);
    if (len(xs) != 5 || cap(xs) != 6) {
        t.Errorf("after SetCap(6), len, cap = %d, %d, want 5, 6", len(xs), cap(xs));
    }
    vs.SetCap(5);
    if (len(xs) != 5 || cap(xs) != 5) {
        t.Errorf("after SetCap(5), len, cap = %d, %d, want 5, 5", len(xs), cap(xs));
    }
    var vsʗ6 = vs;
    shouldPanic(setCapˢ, () => {
        vsʗ6.SetCap(4);
    }); // smaller than len
    var vsʗ7 = vs;
    shouldPanic(setLenˢ, () => {
        vsʗ7.SetLen(6);
    }); // bigger than cap

    ref var va = ref heap<reflectꓸValue>(ValueOf(Ꮡxa).Elem(), out var Ꮡva);
    var vaʗ1 = va;
    shouldPanic(setLenˢ, () => {
        vaʗ1.SetLen(8);
    });
    var vaʗ2 = va;
    shouldPanic(setCapˢ, () => {
        vaʗ2.SetCap(8);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object sDWorldˢ = (@string)"%s, %d world"u8;
internal static readonly object hello42Worldˢ = (@string)"hello 42 world"u8;

public static void TestVariadic(ж<Δtesting.T> t) {
    ref var b = ref heap(new strings.Builder(), out var Ꮡb);
    var V = ValueOf;

    b.Reset();
    V(fmt.Fprintfᶠ).Call(slice<reflectꓸValue>([V(Ꮡb), V(sDWorldˢ), V(helloˢ), V((nint)42)]));
    if (b.String() != "hello, 42 world"u8) {
        t.Errorf("after Fprintf Call: %q != %q", b.String(), hello42Worldˢ);
    }

    b.Reset();
    V(fmt.Fprintfᶠ).CallSlice(slice<reflectꓸValue>([V(Ꮡb), V(sDWorldˢ), V(slice<any>([helloˢ, (nint)42]))]));
    if (b.String() != "hello, 42 world"u8) {
        t.Errorf("after Fprintf CallSlice: %q != %q", b.String(), hello42Worldˢ);
    }
}

public static void TestFuncArg(ж<Δtesting.T> t) {
    var f1 = (nint i, Func<nint, nint> f) => f(i);
    var f2 = (nint i) => i + 1;
    var r = ValueOf((f1).OrTypedNilFunc()).Call(slice<reflectꓸValue>([ValueOf((nint)100), ValueOf((f2).OrTypedNilFunc())]));
    if (r[0].Int() != 101) {
        t.Errorf("function returned %d, want 101", r[0].Int());
    }
}

internal partial struct TestStructArg_padded /*dyn*/ {
    public @string B;
    public int32 C;
}

public static void TestStructArg(ж<Δtesting.T> t) {
    ref var gotA = ref heap(new TestStructArg_padded(), out var ᏑgotA);
    uint32 gotB = default;
    TestStructArg_padded wantA = new TestStructArg_padded("3", 4);
    uint32 wantB = (uint32)5;
    var f = (TestStructArg_padded a, uint32 b) => {
        (ᏑgotA.Value, gotB) = (a, b);
    };
    ValueOf((f).OrTypedNilFunc()).Call(slice<reflectꓸValue>([ValueOf(wantA), ValueOf(wantB)]));
    if (gotA != wantA || gotB != wantB) {
        t.Errorf("function called with (%v, %v), want (%v, %v)", gotA, gotB, wantA, wantB);
    }
}

partial struct tagGetTestsᴛ1 /*dyn*/ {
    public Δreflect.StructTag Tag;
    public @string Key;
    public @string Value;
}
internal static slice<tagGetTestsᴛ1> tagGetTests = slice<tagGetTestsᴛ1>([
    new((@string)@"protobuf:""PB(1,2)""", @"protobuf", @"PB(1,2)"),
    new((@string)@"protobuf:""PB(1,2)""", @"foo", @""),
    new((@string)@"protobuf:""PB(1,2)""", @"rotobuf", @""),
    new((@string)@"protobuf:""PB(1,2)"" json:""name""", @"json", @"name"),
    new((@string)@"protobuf:""PB(1,2)"" json:""name""", @"protobuf", @"PB(1,2)"),
    new((@string)@"k0:""values contain spaces"" k1:""and\ttabs""", "k0", "values contain spaces"),
    new((@string)@"k0:""values contain spaces"" k1:""and\ttabs""", "k1", "and\ttabs")
]);

public static void TestTagGet(ж<Δtesting.T> t) {
    foreach (var (_, tt) in tagGetTests) {
        {
            @string v = tt.Tag.Get(tt.Key); if (v != tt.Value) {
                t.Errorf("StructTag(%#q).Get(%#q) = %#q, want %#q", tt.Tag, tt.Key, v, tt.Value);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string onIntValueˢ => ref onIntValueˢᵇ;
private static readonly @string onIntValueˢᵇ = "on int Value"u8;
internal static ref readonly @string ofNonByteSliceˢ => ref ofNonByteSliceˢᵇ;
private static readonly @string ofNonByteSliceˢᵇ = "of non-byte slice"u8;
internal static ref readonly @string unaddressableˢ => ref unaddressableˢᵇ;
private static readonly @string unaddressableˢᵇ = "unaddressable"u8;
internal static ref readonly @string onPtrValueˢ => ref onPtrValueˢᵇ;
private static readonly @string onPtrValueˢᵇ = "on ptr Value"u8;

internal partial struct TestBytes_S /*[]byte*/;

internal partial struct TestBytes_A /*[4]byte*/;

// Per issue #24746, it was decided that Bytes can be called on byte slices
// that normally cannot be converted from per Go language semantics.
internal partial struct TestBytes_B /*num:byte*/;

internal partial struct TestBytes_SB /*[]TestBytes_B*/;

internal partial struct TestBytes_AB /*[4]TestBytes_B*/;

public static void TestBytes(ж<Δtesting.T> t) {
    shouldPanic(onIntValueˢ, () => {
        ValueOf((nint)0).Bytes();
    });
    shouldPanic(ofNonByteSliceˢ, () => {
        ValueOf(slice<@string>([])).Bytes();
    });
    var x = new TestBytes_S(new byte[]{1, 2, 3, 4}.slice());
    var y = ValueOf(x).Bytes();
    if (!bytes.Equal(x, y)) {
        t.Fatalf("ValueOf(%v).Bytes() = %v", x, y);
    }
    if (Ꮡelem<byte>(x, 0) != Ꮡelem(y, 0)) {
        t.Errorf("ValueOf(%p).Bytes() = %p", Ꮡ(x, 0), Ꮡ(y, 0));
    }
    ref var a = ref heap<TestBytes_A>(new TestBytes_A(new byte[]{1, 2, 3, 4}.array()), out var Ꮡa);
    shouldPanic(unaddressableˢ, () => {
        ValueOf(Ꮡa.Value).Bytes();
    });
    shouldPanic(onPtrValueˢ, () => {
        ValueOf(Ꮡa).Bytes();
    });
    var b = ValueOf(Ꮡa).Elem().Bytes();
    if (!bytes.Equal(a[..], y)) {
        t.Fatalf("ValueOf(%v).Bytes() = %v", a, b);
    }
    if (Ꮡa.at<byte>(0) != Ꮡ(b, 0)) {
        t.Errorf("ValueOf(%p).Bytes() = %p", Ꮡa.at<byte>(0), Ꮡ(b, 0));
    }
    ValueOf(slice<TestBytes_B>([1, 2, 3, 4])).Bytes();                        // should not panic
    ValueOf(Ꮡ(new array<TestBytes_B>(4))).Elem().Bytes();                     // should not panic
    ValueOf(new TestBytes_SB(new TestBytes_B[]{1, 2, 3, 4}.slice())).Bytes(); // should not panic
    ValueOf(@new<TestBytes_AB>()).Elem().Bytes();                             // should not panic
}

internal partial struct TestSetBytes_B /*[]byte*/;

public static void TestSetBytes(ж<Δtesting.T> t) {
    ref var x = ref heap<TestSetBytes_B>(out var Ꮡx);
    var y = slice<byte>([1, 2, 3, 4]);
    ValueOf(Ꮡx).Elem().SetBytes(y);
    if (!bytes.Equal(x, y)) {
        t.Fatalf("ValueOf(%v).Bytes() = %v", x, y);
    }
    if (Ꮡelem<byte>(x, 0) != Ꮡelem(y, 0)) {
        t.Errorf("ValueOf(%p).Bytes() = %p", Ꮡ(x, 0), Ꮡ(y, 0));
    }
}

partial struct Private {
    internal nint x;
    internal ж<ж<nint>> y;
    public nint Z;
}

internal static void m(this ref Private p) {
}

partial struct @private {
    public nint Z;
    internal nint z;
    public @string S;
    public array<Private> A = new(1);
    public slice<Private> T;
}

internal static void P(this ref @private p) {
}

partial struct Public {
    public nint X;
    public ж<ж<nint>> Y;
    /*embed*/ internal @private @private;
}

public static void M(this ref Public p) {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string interfaceˢ2 => ref interfaceˢ2ᵇ;
private static readonly @string interfaceˢ2ᵇ = "Interface"u8;

public static void TestUnexported(ж<Δtesting.T> t) {
    ref var pub = ref heap(new Public(), out var Ꮡpub);
    pub.S = "S";
    pub.T = pub.A[..];
    ref var v = ref heap<reflectꓸValue>(ValueOf(Ꮡpub), out var Ꮡv);
    isValid(v.Elem().Field(0));
    isValid(v.Elem().Field(1));
    isValid(v.Elem().Field(2));
    isValid(v.Elem().FieldByName("X"));
    isValid(v.Elem().FieldByName("Y"));
    isValid(v.Elem().FieldByName("Z"));
    isValid(v.Type().Method(0).Func);
    var (m, _) = v.Type().MethodByName("M");
    isValid(m.Func);
    (m, _) = v.Type().MethodByName("P");
    isValid(m.Func);
    isNonNil(v.Elem().Field(0).Interface());
    isNonNil(v.Elem().Field(1).Interface());
    isNonNil(v.Elem().Field(2).Field(2).Index(0));
    isNonNil(v.Elem().FieldByName("X").Interface());
    isNonNil(v.Elem().FieldByName("Y").Interface());
    isNonNil(v.Elem().FieldByName("Z").Interface());
    isNonNil(v.Elem().FieldByName("S").Index(0).Interface());
    isNonNil(v.Type().Method(0).Func.Interface());
    (m, _) = v.Type().MethodByName("P");
    isNonNil(m.Func.Interface());

    ref var priv = ref heap(new Private(), out var Ꮡpriv);
    v = ValueOf(Ꮡpriv);
    isValid(v.Elem().Field(0));
    isValid(v.Elem().Field(1));
    isValid(v.Elem().FieldByName("x"));
    isValid(v.Elem().FieldByName("y"));
    var vʗ1 = v;
    shouldPanic(interfaceˢ2, () => {
        vʗ1.Elem().Field(0).Interface();
    });
    var vʗ2 = v;
    shouldPanic(interfaceˢ2, () => {
        vʗ2.Elem().Field(1).Interface();
    });
    var vʗ3 = v;
    shouldPanic(interfaceˢ2, () => {
        vʗ3.Elem().FieldByName("x").Interface();
    });
    var vʗ4 = v;
    shouldPanic(interfaceˢ2, () => {
        vʗ4.Elem().FieldByName("y").Interface();
    });
    var vʗ5 = v;
    shouldPanic(methodˢ, () => {
        vʗ5.Type().Method(0);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string setˢ => ref setˢᵇ;
private static readonly @string setˢᵇ = "Set"u8;

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

public static void TestSetPanic(ж<Δtesting.T> t) {
    void ok(Action f) {
        f();
    }
    void bad(Action f) {
        shouldPanic(setˢ, f);
    }
    void clear(reflectꓸValue vΔ1) {
        vΔ1.Set(Zero(vΔ1.Type()));
    }

    // not addressable
    ref var v = ref heap<reflectꓸValue>(ValueOf(new TestSetPanic_T()), out var Ꮡv);
    var clearʗ1 = clear;
    bad(() => {
        clearʗ1(Ꮡv.Value.Field(0));
    }); // .X
    var clearʗ2 = clear;
    bad(() => {
        clearʗ2(Ꮡv.Value.Field(1));
    }); // .t1
    var clearʗ3 = clear;
    bad(() => {
        clearʗ3(Ꮡv.Value.Field(1).Field(0));
    }); // .t1.Y
    var clearʗ4 = clear;
    bad(() => {
        clearʗ4(Ꮡv.Value.Field(1).Field(1));
    }); // .t1.t0
    var clearʗ5 = clear;
    bad(() => {
        clearʗ5(Ꮡv.Value.Field(1).Field(1).Field(0));
    }); // .t1.t0.W
    var clearʗ6 = clear;
    bad(() => {
        clearʗ6(Ꮡv.Value.Field(2));
    }); // .T2
    var clearʗ7 = clear;
    bad(() => {
        clearʗ7(Ꮡv.Value.Field(2).Field(0));
    }); // .T2.Z
    var clearʗ8 = clear;
    bad(() => {
        clearʗ8(Ꮡv.Value.Field(2).Field(1));
    }); // .T2.namedT0
    var clearʗ9 = clear;
    bad(() => {
        clearʗ9(Ꮡv.Value.Field(2).Field(1).Field(0));
    }); // .T2.namedT0.W
    var clearʗ10 = clear;
    bad(() => {
        clearʗ10(Ꮡv.Value.Field(3));
    }); // .NamedT1
    var clearʗ11 = clear;
    bad(() => {
        clearʗ11(Ꮡv.Value.Field(3).Field(0));
    }); // .NamedT1.Y
    var clearʗ12 = clear;
    bad(() => {
        clearʗ12(Ꮡv.Value.Field(3).Field(1));
    }); // .NamedT1.t0
    var clearʗ13 = clear;
    bad(() => {
        clearʗ13(Ꮡv.Value.Field(3).Field(1).Field(0));
    }); // .NamedT1.t0.W
    var clearʗ14 = clear;
    bad(() => {
        clearʗ14(Ꮡv.Value.Field(4));
    }); // .NamedT2
    var clearʗ15 = clear;
    bad(() => {
        clearʗ15(Ꮡv.Value.Field(4).Field(0));
    }); // .NamedT2.Z
    var clearʗ16 = clear;
    bad(() => {
        clearʗ16(Ꮡv.Value.Field(4).Field(1));
    }); // .NamedT2.namedT0
    var clearʗ17 = clear;
    bad(() => {
        clearʗ17(Ꮡv.Value.Field(4).Field(1).Field(0));
    }); // .NamedT2.namedT0.W
    var clearʗ18 = clear;
    bad(() => {
        clearʗ18(Ꮡv.Value.Field(5));
    }); // .namedT1
    var clearʗ19 = clear;
    bad(() => {
        clearʗ19(Ꮡv.Value.Field(5).Field(0));
    }); // .namedT1.Y
    var clearʗ20 = clear;
    bad(() => {
        clearʗ20(Ꮡv.Value.Field(5).Field(1));
    }); // .namedT1.t0
    var clearʗ21 = clear;
    bad(() => {
        clearʗ21(Ꮡv.Value.Field(5).Field(1).Field(0));
    }); // .namedT1.t0.W
    var clearʗ22 = clear;
    bad(() => {
        clearʗ22(Ꮡv.Value.Field(6));
    }); // .namedT2
    var clearʗ23 = clear;
    bad(() => {
        clearʗ23(Ꮡv.Value.Field(6).Field(0));
    }); // .namedT2.Z
    var clearʗ24 = clear;
    bad(() => {
        clearʗ24(Ꮡv.Value.Field(6).Field(1));
    }); // .namedT2.namedT0
    var clearʗ25 = clear;
    bad(() => {
        clearʗ25(Ꮡv.Value.Field(6).Field(1).Field(0));
    }); // .namedT2.namedT0.W

    // addressable
    v = ValueOf(Ꮡ(new TestSetPanic_T())).Elem();
    var clearʗ26 = clear;
    ok(() => {
        clearʗ26(Ꮡv.Value.Field(0));
    }); // .X
    var clearʗ27 = clear;
    bad(() => {
        clearʗ27(Ꮡv.Value.Field(1));
    }); // .t1
    var clearʗ28 = clear;
    ok(() => {
        clearʗ28(Ꮡv.Value.Field(1).Field(0));
    }); // .t1.Y
    var clearʗ29 = clear;
    bad(() => {
        clearʗ29(Ꮡv.Value.Field(1).Field(1));
    }); // .t1.t0
    var clearʗ30 = clear;
    ok(() => {
        clearʗ30(Ꮡv.Value.Field(1).Field(1).Field(0));
    }); // .t1.t0.W
    var clearʗ31 = clear;
    ok(() => {
        clearʗ31(Ꮡv.Value.Field(2));
    }); // .T2
    var clearʗ32 = clear;
    ok(() => {
        clearʗ32(Ꮡv.Value.Field(2).Field(0));
    }); // .T2.Z
    var clearʗ33 = clear;
    bad(() => {
        clearʗ33(Ꮡv.Value.Field(2).Field(1));
    }); // .T2.namedT0
    var clearʗ34 = clear;
    bad(() => {
        clearʗ34(Ꮡv.Value.Field(2).Field(1).Field(0));
    }); // .T2.namedT0.W
    var clearʗ35 = clear;
    ok(() => {
        clearʗ35(Ꮡv.Value.Field(3));
    }); // .NamedT1
    var clearʗ36 = clear;
    ok(() => {
        clearʗ36(Ꮡv.Value.Field(3).Field(0));
    }); // .NamedT1.Y
    var clearʗ37 = clear;
    bad(() => {
        clearʗ37(Ꮡv.Value.Field(3).Field(1));
    }); // .NamedT1.t0
    var clearʗ38 = clear;
    ok(() => {
        clearʗ38(Ꮡv.Value.Field(3).Field(1).Field(0));
    }); // .NamedT1.t0.W
    var clearʗ39 = clear;
    ok(() => {
        clearʗ39(Ꮡv.Value.Field(4));
    }); // .NamedT2
    var clearʗ40 = clear;
    ok(() => {
        clearʗ40(Ꮡv.Value.Field(4).Field(0));
    }); // .NamedT2.Z
    var clearʗ41 = clear;
    bad(() => {
        clearʗ41(Ꮡv.Value.Field(4).Field(1));
    }); // .NamedT2.namedT0
    var clearʗ42 = clear;
    bad(() => {
        clearʗ42(Ꮡv.Value.Field(4).Field(1).Field(0));
    }); // .NamedT2.namedT0.W
    var clearʗ43 = clear;
    bad(() => {
        clearʗ43(Ꮡv.Value.Field(5));
    }); // .namedT1
    var clearʗ44 = clear;
    bad(() => {
        clearʗ44(Ꮡv.Value.Field(5).Field(0));
    }); // .namedT1.Y
    var clearʗ45 = clear;
    bad(() => {
        clearʗ45(Ꮡv.Value.Field(5).Field(1));
    }); // .namedT1.t0
    var clearʗ46 = clear;
    bad(() => {
        clearʗ46(Ꮡv.Value.Field(5).Field(1).Field(0));
    }); // .namedT1.t0.W
    var clearʗ47 = clear;
    bad(() => {
        clearʗ47(Ꮡv.Value.Field(6));
    }); // .namedT2
    var clearʗ48 = clear;
    bad(() => {
        clearʗ48(Ꮡv.Value.Field(6).Field(0));
    }); // .namedT2.Z
    var clearʗ49 = clear;
    bad(() => {
        clearʗ49(Ꮡv.Value.Field(6).Field(1));
    }); // .namedT2.namedT0
    var clearʗ50 = clear;
    bad(() => {
        clearʗ50(Ꮡv.Value.Field(6).Field(1).Field(0));
    }); // .namedT2.namedT0.W
}

partial struct timp /*num:nint*/;

internal static void W(this timp t) {
}

internal static void Y(this timp t) {
}

internal static void w(this timp t) {
}

internal static void y(this timp t) {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string callˢ => ref callˢᵇ;
private static readonly @string callˢᵇ = "Call"u8;

internal partial interface TestCallPanic_t0 /*dyn*/ {
    void W();
    void w();
}

internal partial interface TestCallPanic_T1 /*dyn*/ {
    void Y();
    void y();
}

internal partial struct TestCallPanic_T2 /*dyn*/ {
    /*embed*/ public TestCallPanic_T1 T1;
    /*embed*/ internal TestCallPanic_t0 t0;
}

internal partial struct TestCallPanic_T /*dyn*/ {
    /*embed*/ internal TestCallPanic_t0 t0; // 0
    /*embed*/ public TestCallPanic_T1 T1;   // 1
    public TestCallPanic_t0 NamedT0; // 2
    public TestCallPanic_T1 NamedT1; // 3
    public TestCallPanic_T2 NamedT2; // 4
    internal TestCallPanic_t0 namedT0; // 5
    internal TestCallPanic_T1 namedT1; // 6
    internal TestCallPanic_T2 namedT2; // 7
}

public static void TestCallPanic(ж<Δtesting.T> t) {
    void ok(Action f) {
        f();
    }
    void badCall(Action f) {
        shouldPanic(callˢ, f);
    }
    void badMethod(Action f) {
        shouldPanic(methodˢ, f);
    }
    void call(reflectꓸValue vΔ1) {
        vΔ1.Call(default);
    }

    timp i = (timp)0;
    ref var v = ref heap<reflectꓸValue>(ValueOf(new TestCallPanic_T(i, i, i, i, new TestCallPanic_T2(i, i), i, i, new TestCallPanic_T2(i, i))), out var Ꮡv);
    var callʗ1 = call;
    var vʗ1 = v;
    badCall(() => {
        callʗ1(vʗ1.Field(0).Method(0));
    }); // .t0.W
    var callʗ2 = call;
    var vʗ2 = v;
    badCall(() => {
        callʗ2(vʗ2.Field(0).Elem().Method(0));
    }); // .t0.W
    var callʗ3 = call;
    var vʗ3 = v;
    badCall(() => {
        callʗ3(vʗ3.Field(0).Method(1));
    }); // .t0.w
    var callʗ4 = call;
    var vʗ4 = v;
    badMethod(() => {
        callʗ4(vʗ4.Field(0).Elem().Method(2));
    }); // .t0.w
    var callʗ5 = call;
    var vʗ5 = v;
    ok(() => {
        callʗ5(vʗ5.Field(1).Method(0));
    }); // .T1.Y
    var callʗ6 = call;
    var vʗ6 = v;
    ok(() => {
        callʗ6(vʗ6.Field(1).Elem().Method(0));
    }); // .T1.Y
    var callʗ7 = call;
    var vʗ7 = v;
    badCall(() => {
        callʗ7(vʗ7.Field(1).Method(1));
    }); // .T1.y
    var callʗ8 = call;
    var vʗ8 = v;
    badMethod(() => {
        callʗ8(vʗ8.Field(1).Elem().Method(2));
    }); // .T1.y

    var callʗ9 = call;
    var vʗ9 = v;
    ok(() => {
        callʗ9(vʗ9.Field(2).Method(0));
    }); // .NamedT0.W
    var callʗ10 = call;
    var vʗ10 = v;
    ok(() => {
        callʗ10(vʗ10.Field(2).Elem().Method(0));
    }); // .NamedT0.W
    var callʗ11 = call;
    var vʗ11 = v;
    badCall(() => {
        callʗ11(vʗ11.Field(2).Method(1));
    }); // .NamedT0.w
    var callʗ12 = call;
    var vʗ12 = v;
    badMethod(() => {
        callʗ12(vʗ12.Field(2).Elem().Method(2));
    }); // .NamedT0.w

    var callʗ13 = call;
    var vʗ13 = v;
    ok(() => {
        callʗ13(vʗ13.Field(3).Method(0));
    }); // .NamedT1.Y
    var callʗ14 = call;
    var vʗ14 = v;
    ok(() => {
        callʗ14(vʗ14.Field(3).Elem().Method(0));
    }); // .NamedT1.Y
    var callʗ15 = call;
    var vʗ15 = v;
    badCall(() => {
        callʗ15(vʗ15.Field(3).Method(1));
    }); // .NamedT1.y
    var callʗ16 = call;
    var vʗ16 = v;
    badMethod(() => {
        callʗ16(vʗ16.Field(3).Elem().Method(3));
    }); // .NamedT1.y

    var callʗ17 = call;
    var vʗ17 = v;
    ok(() => {
        callʗ17(vʗ17.Field(4).Field(0).Method(0));
    }); // .NamedT2.T1.Y
    var callʗ18 = call;
    var vʗ18 = v;
    ok(() => {
        callʗ18(vʗ18.Field(4).Field(0).Elem().Method(0));
    }); // .NamedT2.T1.W
    var callʗ19 = call;
    var vʗ19 = v;
    badCall(() => {
        callʗ19(vʗ19.Field(4).Field(1).Method(0));
    }); // .NamedT2.t0.W
    var callʗ20 = call;
    var vʗ20 = v;
    badCall(() => {
        callʗ20(vʗ20.Field(4).Field(1).Elem().Method(0));
    }); // .NamedT2.t0.W

    var callʗ21 = call;
    var vʗ21 = v;
    badCall(() => {
        callʗ21(vʗ21.Field(5).Method(0));
    }); // .namedT0.W
    var callʗ22 = call;
    var vʗ22 = v;
    badCall(() => {
        callʗ22(vʗ22.Field(5).Elem().Method(0));
    }); // .namedT0.W
    var callʗ23 = call;
    var vʗ23 = v;
    badCall(() => {
        callʗ23(vʗ23.Field(5).Method(1));
    }); // .namedT0.w
    var callʗ24 = call;
    var vʗ24 = v;
    badMethod(() => {
        callʗ24(vʗ24.Field(5).Elem().Method(2));
    }); // .namedT0.w

    var callʗ25 = call;
    var vʗ25 = v;
    badCall(() => {
        callʗ25(vʗ25.Field(6).Method(0));
    }); // .namedT1.Y
    var callʗ26 = call;
    var vʗ26 = v;
    badCall(() => {
        callʗ26(vʗ26.Field(6).Elem().Method(0));
    }); // .namedT1.Y
    var callʗ27 = call;
    var vʗ27 = v;
    badCall(() => {
        callʗ27(vʗ27.Field(6).Method(0));
    }); // .namedT1.y
    var callʗ28 = call;
    var vʗ28 = v;
    badCall(() => {
        callʗ28(vʗ28.Field(6).Elem().Method(0));
    }); // .namedT1.y

    var callʗ29 = call;
    var vʗ29 = v;
    badCall(() => {
        callʗ29(vʗ29.Field(7).Field(0).Method(0));
    }); // .namedT2.T1.Y
    var callʗ30 = call;
    var vʗ30 = v;
    badCall(() => {
        callʗ30(vʗ30.Field(7).Field(0).Elem().Method(0));
    }); // .namedT2.T1.W
    var callʗ31 = call;
    var vʗ31 = v;
    badCall(() => {
        callʗ31(vʗ31.Field(7).Field(1).Method(0));
    }); // .namedT2.t0.W
    var callʗ32 = call;
    var vʗ32 = v;
    badCall(() => {
        callʗ32(vʗ32.Field(7).Field(1).Elem().Method(0));
    }); // .namedT2.t0.W
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string reflectValueAddrOfˢ => ref reflectValueAddrOfˢᵇ;
private static readonly @string reflectValueAddrOfˢᵇ = "reflect.Value.Addr of unaddressable value"u8;
internal static ref readonly @string callOfReflectValueBoolOnˢ => ref callOfReflectValueBoolOnˢᵇ;
private static readonly @string callOfReflectValueBoolOnˢᵇ = "call of reflect.Value.Bool on float64 Value"u8;
internal static ref readonly @string callOfReflectValueBytesˢ => ref callOfReflectValueBytesˢᵇ;
private static readonly @string callOfReflectValueBytesˢᵇ = "call of reflect.Value.Bytes on string Value"u8;
internal static ref readonly @string callOfReflectValueCallOnˢ => ref callOfReflectValueCallOnˢᵇ;
private static readonly @string callOfReflectValueCallOnˢᵇ = "call of reflect.Value.Call on bool Value"u8;
internal static ref readonly @string callOfReflectValueˢ => ref callOfReflectValueˢᵇ;
private static readonly @string callOfReflectValueˢᵇ = "call of reflect.Value.CallSlice on int Value"u8;
internal static ref readonly @string callOfReflectValueCloseˢ => ref callOfReflectValueCloseˢᵇ;
private static readonly @string callOfReflectValueCloseˢᵇ = "call of reflect.Value.Close on string Value"u8;
internal static ref readonly @string callOfReflectValueˢ2 => ref callOfReflectValueˢ2ᵇ;
private static readonly @string callOfReflectValueˢ2ᵇ = "call of reflect.Value.Complex on float64 Value"u8;
internal static ref readonly @string callOfReflectValueElemOnˢ => ref callOfReflectValueElemOnˢᵇ;
private static readonly @string callOfReflectValueElemOnˢᵇ = "call of reflect.Value.Elem on bool Value"u8;
internal static ref readonly @string callOfReflectValueFieldˢ => ref callOfReflectValueFieldˢᵇ;
private static readonly @string callOfReflectValueFieldˢᵇ = "call of reflect.Value.Field on int Value"u8;
internal static ref readonly @string callOfReflectValueFloatˢ => ref callOfReflectValueFloatˢᵇ;
private static readonly @string callOfReflectValueFloatˢᵇ = "call of reflect.Value.Float on string Value"u8;
internal static ref readonly @string callOfReflectValueIndexˢ => ref callOfReflectValueIndexˢᵇ;
private static readonly @string callOfReflectValueIndexˢᵇ = "call of reflect.Value.Index on float64 Value"u8;
internal static ref readonly @string callOfReflectValueIntOnˢ => ref callOfReflectValueIntOnˢᵇ;
private static readonly @string callOfReflectValueIntOnˢᵇ = "call of reflect.Value.Int on bool Value"u8;
internal static ref readonly @string callOfReflectValueIsNilˢ => ref callOfReflectValueIsNilˢᵇ;
private static readonly @string callOfReflectValueIsNilˢᵇ = "call of reflect.Value.IsNil on int Value"u8;
internal static ref readonly @string callOfReflectValueLenOnˢ => ref callOfReflectValueLenOnˢᵇ;
private static readonly @string callOfReflectValueLenOnˢᵇ = "call of reflect.Value.Len on bool Value"u8;
internal static ref readonly @string callOfReflectValueˢ3 => ref callOfReflectValueˢ3ᵇ;
private static readonly @string callOfReflectValueˢ3ᵇ = "call of reflect.Value.MapIndex on float64 Value"u8;
internal static ref readonly @string callOfReflectValueˢ4 => ref callOfReflectValueˢ4ᵇ;
private static readonly @string callOfReflectValueˢ4ᵇ = "call of reflect.Value.MapKeys on string Value"u8;
internal static ref readonly @string callOfReflectValueˢ5 => ref callOfReflectValueˢ5ᵇ;
private static readonly @string callOfReflectValueˢ5ᵇ = "call of reflect.Value.MapRange on int Value"u8;
internal static ref readonly @string callOfReflectValueMethodˢ => ref callOfReflectValueMethodˢᵇ;
private static readonly @string callOfReflectValueMethodˢᵇ = "call of reflect.Value.Method on zero Value"u8;
internal static ref readonly @string callOfReflectValueˢ6 => ref callOfReflectValueˢ6ᵇ;
private static readonly @string callOfReflectValueˢ6ᵇ = "call of reflect.Value.NumField on string Value"u8;
internal static ref readonly @string callOfReflectValueˢ7 => ref callOfReflectValueˢ7ᵇ;
private static readonly @string callOfReflectValueˢ7ᵇ = "call of reflect.Value.NumMethod on zero Value"u8;
internal static ref readonly @string callOfReflectValueˢ8 => ref callOfReflectValueˢ8ᵇ;
private static readonly @string callOfReflectValueˢ8ᵇ = "call of reflect.Value.OverflowComplex on float64 Value"u8;
internal static ref readonly @string callOfReflectValueˢ9 => ref callOfReflectValueˢ9ᵇ;
private static readonly @string callOfReflectValueˢ9ᵇ = "call of reflect.Value.OverflowFloat on int64 Value"u8;
internal static ref readonly @string callOfReflectValueˢ10 => ref callOfReflectValueˢ10ᵇ;
private static readonly @string callOfReflectValueˢ10ᵇ = "call of reflect.Value.OverflowInt on uint64 Value"u8;
internal static ref readonly @string callOfReflectValueˢ11 => ref callOfReflectValueˢ11ᵇ;
private static readonly @string callOfReflectValueˢ11ᵇ = "call of reflect.Value.OverflowUint on complex64 Value"u8;
internal static ref readonly @string callOfReflectValueRecvOnˢ => ref callOfReflectValueRecvOnˢᵇ;
private static readonly @string callOfReflectValueRecvOnˢᵇ = "call of reflect.Value.Recv on string Value"u8;
internal static ref readonly @string callOfReflectValueSendOnˢ => ref callOfReflectValueSendOnˢᵇ;
private static readonly @string callOfReflectValueSendOnˢᵇ = "call of reflect.Value.Send on bool Value"u8;
internal static ref readonly @string valueOfTypeStringIsNotˢ => ref valueOfTypeStringIsNotˢᵇ;
private static readonly @string valueOfTypeStringIsNotˢᵇ = "value of type string is not assignable to type bool"u8;
internal static ref readonly @string callOfReflectValueˢ12 => ref callOfReflectValueˢ12ᵇ;
private static readonly @string callOfReflectValueˢ12ᵇ = "call of reflect.Value.SetBool on string Value"u8;
internal static ref readonly @string reflectValueSetBytesˢ => ref reflectValueSetBytesˢᵇ;
private static readonly @string reflectValueSetBytesˢᵇ = "reflect.Value.SetBytes using unaddressable value"u8;
internal static ref readonly @string callOfReflectValueSetCapˢ => ref callOfReflectValueSetCapˢᵇ;
private static readonly @string callOfReflectValueSetCapˢᵇ = "call of reflect.Value.SetCap on string Value"u8;
internal static ref readonly @string callOfReflectValueˢ13 => ref callOfReflectValueˢ13ᵇ;
private static readonly @string callOfReflectValueˢ13ᵇ = "call of reflect.Value.SetComplex on string Value"u8;
internal static ref readonly @string callOfReflectValueˢ14 => ref callOfReflectValueˢ14ᵇ;
private static readonly @string callOfReflectValueˢ14ᵇ = "call of reflect.Value.SetFloat on string Value"u8;
internal static ref readonly @string callOfReflectValueSetIntˢ => ref callOfReflectValueSetIntˢᵇ;
private static readonly @string callOfReflectValueSetIntˢᵇ = "call of reflect.Value.SetInt on string Value"u8;
internal static ref readonly @string callOfReflectValueSetLenˢ => ref callOfReflectValueSetLenˢᵇ;
private static readonly @string callOfReflectValueSetLenˢᵇ = "call of reflect.Value.SetLen on string Value"u8;
internal static ref readonly @string callOfReflectValueˢ15 => ref callOfReflectValueˢ15ᵇ;
private static readonly @string callOfReflectValueˢ15ᵇ = "call of reflect.Value.SetString on int Value"u8;
internal static ref readonly @string reflectValueSetUintUsingˢ => ref reflectValueSetUintUsingˢᵇ;
private static readonly @string reflectValueSetUintUsingˢᵇ = "reflect.Value.SetUint using unaddressable value"u8;
internal static ref readonly @string callOfReflectValueSliceˢ => ref callOfReflectValueSliceˢᵇ;
private static readonly @string callOfReflectValueSliceˢᵇ = "call of reflect.Value.Slice on bool Value"u8;
internal static ref readonly @string callOfReflectValueSlice3ˢ => ref callOfReflectValueSlice3ˢᵇ;
private static readonly @string callOfReflectValueSlice3ˢᵇ = "call of reflect.Value.Slice3 on int Value"u8;
internal static ref readonly @string callOfReflectValueˢ16 => ref callOfReflectValueˢ16ᵇ;
private static readonly @string callOfReflectValueˢ16ᵇ = "call of reflect.Value.TryRecv on bool Value"u8;
internal static ref readonly @string callOfReflectValueˢ17 => ref callOfReflectValueˢ17ᵇ;
private static readonly @string callOfReflectValueˢ17ᵇ = "call of reflect.Value.TrySend on string Value"u8;
internal static ref readonly @string callOfReflectValueUintOnˢ => ref callOfReflectValueUintOnˢᵇ;
private static readonly @string callOfReflectValueUintOnˢᵇ = "call of reflect.Value.Uint on float64 Value"u8;

public static void TestValuePanic(ж<Δtesting.T> t) {
    var vo = ValueOf;
    var voʗ1 = vo;
    shouldPanic(reflectValueAddrOfˢ, () => {
        voʗ1((nint)0).Addr();
    });
    var voʗ2 = vo;
    shouldPanic(callOfReflectValueBoolOnˢ, () => {
        voʗ2(0.0D).Bool();
    });
    var voʗ3 = vo;
    shouldPanic(callOfReflectValueBytesˢ, () => {
        voʗ3((@string)""u8).Bytes();
    });
    var voʗ4 = vo;
    shouldPanic(callOfReflectValueCallOnˢ, () => {
        voʗ4(true).Call(default);
    });
    var voʗ5 = vo;
    shouldPanic(callOfReflectValueˢ, () => {
        voʗ5((nint)0).CallSlice(default);
    });
    var voʗ6 = vo;
    shouldPanic(callOfReflectValueCloseˢ, () => {
        voʗ6((@string)""u8).Close();
    });
    var voʗ7 = vo;
    shouldPanic(callOfReflectValueˢ2, () => {
        voʗ7(0.0D).Complex();
    });
    var voʗ8 = vo;
    shouldPanic(callOfReflectValueElemOnˢ, () => {
        voʗ8(false).Elem();
    });
    var voʗ9 = vo;
    shouldPanic(callOfReflectValueFieldˢ, () => {
        voʗ9((nint)0).Field(0);
    });
    var voʗ10 = vo;
    shouldPanic(callOfReflectValueFloatˢ, () => {
        voʗ10((@string)""u8).Float();
    });
    var voʗ11 = vo;
    shouldPanic(callOfReflectValueIndexˢ, () => {
        voʗ11(0.0D).Index(0);
    });
    var voʗ12 = vo;
    shouldPanic(callOfReflectValueIntOnˢ, () => {
        voʗ12(false).Int();
    });
    var voʗ13 = vo;
    shouldPanic(callOfReflectValueIsNilˢ, () => {
        voʗ13((nint)0).IsNil();
    });
    var voʗ14 = vo;
    shouldPanic(callOfReflectValueLenOnˢ, () => {
        voʗ14(false).Len();
    });
    var voʗ15 = vo;
    shouldPanic(callOfReflectValueˢ3, () => {
        voʗ15(0.0D).MapIndex(voʗ15(0.0D));
    });
    var voʗ16 = vo;
    shouldPanic(callOfReflectValueˢ4, () => {
        voʗ16((@string)""u8).MapKeys();
    });
    var voʗ17 = vo;
    shouldPanic(callOfReflectValueˢ5, () => {
        voʗ17((nint)0).MapRange();
    });
    var voʗ18 = vo;
    shouldPanic(callOfReflectValueMethodˢ, () => {
        voʗ18(default).Method(0);
    });
    var voʗ19 = vo;
    shouldPanic(callOfReflectValueˢ6, () => {
        voʗ19((@string)""u8).NumField();
    });
    var voʗ20 = vo;
    shouldPanic(callOfReflectValueˢ7, () => {
        voʗ20(default).NumMethod();
    });
    var voʗ21 = vo;
    shouldPanic(callOfReflectValueˢ8, () => {
        voʗ21((float64)0D).OverflowComplex(0D);
    });
    var voʗ22 = vo;
    shouldPanic(callOfReflectValueˢ9, () => {
        voʗ22((int64)0).OverflowFloat(0D);
    });
    var voʗ23 = vo;
    shouldPanic(callOfReflectValueˢ10, () => {
        voʗ23((uint64)0).OverflowInt(0);
    });
    var voʗ24 = vo;
    shouldPanic(callOfReflectValueˢ11, () => {
        voʗ24((complex64)0F).OverflowUint(0);
    });
    var voʗ25 = vo;
    shouldPanic(callOfReflectValueRecvOnˢ, () => {
        voʗ25((@string)""u8).Recv();
    });
    var voʗ26 = vo;
    shouldPanic(callOfReflectValueSendOnˢ, () => {
        voʗ26(true).Send(voʗ26(true));
    });
    var voʗ27 = vo;
    shouldPanic(valueOfTypeStringIsNotˢ, () => {
        voʗ27(@new<bool>()).Elem().Set(voʗ27((@string)""u8));
    });
    var voʗ28 = vo;
    shouldPanic(callOfReflectValueˢ12, () => {
        voʗ28(@new<@string>()).Elem().SetBool(false);
    });
    var voʗ29 = vo;
    shouldPanic(reflectValueSetBytesˢ, () => {
        voʗ29((@string)""u8).SetBytes(default);
    });
    var voʗ30 = vo;
    shouldPanic(callOfReflectValueSetCapˢ, () => {
        voʗ30(@new<@string>()).Elem().SetCap(0);
    });
    var voʗ31 = vo;
    shouldPanic(callOfReflectValueˢ13, () => {
        voʗ31(@new<@string>()).Elem().SetComplex(0D);
    });
    var voʗ32 = vo;
    shouldPanic(callOfReflectValueˢ14, () => {
        voʗ32(@new<@string>()).Elem().SetFloat(0D);
    });
    var voʗ33 = vo;
    shouldPanic(callOfReflectValueSetIntˢ, () => {
        voʗ33(@new<@string>()).Elem().SetInt(0);
    });
    var voʗ34 = vo;
    shouldPanic(callOfReflectValueSetLenˢ, () => {
        voʗ34(@new<@string>()).Elem().SetLen(0);
    });
    var voʗ35 = vo;
    shouldPanic(callOfReflectValueˢ15, () => {
        voʗ35(@new<nint>()).Elem().SetString(""u8);
    });
    var voʗ36 = vo;
    shouldPanic(reflectValueSetUintUsingˢ, () => {
        voʗ36(0.0D).SetUint(0);
    });
    var voʗ37 = vo;
    shouldPanic(callOfReflectValueSliceˢ, () => {
        voʗ37(true).Slice(1, 2);
    });
    var voʗ38 = vo;
    shouldPanic(callOfReflectValueSlice3ˢ, () => {
        voʗ38((nint)0).Slice3(1, 2, 3);
    });
    var voʗ39 = vo;
    shouldPanic(callOfReflectValueˢ16, () => {
        voʗ39(true).TryRecv();
    });
    var voʗ40 = vo;
    shouldPanic(callOfReflectValueˢ17, () => {
        voʗ40((@string)""u8).TrySend(voʗ40((@string)""u8));
    });
    var voʗ41 = vo;
    shouldPanic(callOfReflectValueUintOnˢ, () => {
        voʗ41(0.0D).Uint();
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string reflectˢ => ref reflectˢᵇ;
private static readonly @string reflectˢᵇ = "reflect"u8;

internal static void shouldPanic(@string expect, Action f) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            var r = recover();
            if (r == nil) {
                throw panic("did not panic");
            }
            if (expect != ""u8) {
                @string s = default;
                switch (r.type()) {
                case @string rΔ1: {
                    s = rΔ1;
                    break;
                }
                case ж<Δreflect.ValueError> rΔ1: {
                    s = rΔ1.Error();
                    break;
                }
                default: {
                    var rΔ1 = r;
                    throw panic(fmt.Sprintf("panicked with unexpected type %T"u8, rΔ1));
                    break;
                }}
                if (!strings.HasPrefix(s, reflectˢ)) {
                    throw panic(@"panic string does not start with ""reflect"": " + s);
                }
                if (!strings.Contains(s, expect)) {
                    throw panic(@"panic string does not contain """ + expect + @""": " + s);
                }
            }
        }, ref ᒐ);
        f();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void isNonNil(any x) {
    if (x == nil) {
        throw panic("nil interface");
    }
}

internal static void isValid(reflectꓸValue v) {
    if (!v.IsValid()) {
        throw panic("zero Value");
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string worldˢ => ref worldˢᵇ;
private static readonly @string worldˢᵇ = "world"u8;

public static void TestAlias(ж<Δtesting.T> t) {
    ref var x = ref heap<@string>((@string)"hello"u8, out var Ꮡx);
    var v = ValueOf(Ꮡx).Elem();
    var oldvalue = v.Interface();
    v.SetString(worldˢ);
    var newvalue = v.Interface();

    if (!AreEqual(oldvalue, (@string)("hello")) || !AreEqual(newvalue, (@string)("world"))) {
        t.Errorf("aliasing: old=%q new=%q, want hello, world", oldvalue, newvalue);
    }
}

public static readonly ж<Func<any, reflectꓸValue>> ᏑV = new StaticBox<Func<any, reflectꓸValue>>(ValueOf);
public static ref Func<any, reflectꓸValue> V => ref ((StaticBox<Func<any, reflectꓸValue>>)ᏑV).ValueSlot;

public static reflectꓸValue EmptyInterfaceV(any xʗp) {
    ref var x = ref heap(xʗp, out var Ꮡx);

    return ValueOf(Ꮡx).Elem();
}

public static reflectꓸValue ReaderV(Δio.Reader xʗp) {
    ref var x = ref heap(xʗp, out var Ꮡx);

    return ValueOf(Ꮡx).Elem();
}

public static reflectꓸValue ReadWriterV(Δio.ReadWriter xʗp) {
    ref var x = ref heap(xʗp, out var Ꮡx);

    return ValueOf(Ꮡx).Elem();
}

partial struct Empty {
}

partial struct MyStruct {
    internal nint x; /*`some:"tag"`*/
}

partial struct MyStruct1_x /*dyn*/ {
    /*embed*/ internal nint @int; /*`some:"bar"`*/
}

partial struct MyStruct1 {
    internal MyStruct1_x x;
}

partial struct MyStruct2_x /*dyn*/ {
    /*embed*/ internal nint @int; /*`some:"foo"`*/
}

partial struct MyStruct2 {
    internal MyStruct2_x x;
}

partial struct MyString /*@string*/;

partial struct MyBytes /*[]byte*/;

/*[0]*/ partial class MyBytesArrayPtr0 /*ж<array<byte>>*/;

/*[4]*/ partial class MyBytesArrayPtr /*ж<array<byte>>*/;

partial struct MyBytesArray0 /*[0]byte*/;

partial struct MyBytesArray /*[4]byte*/;

partial struct MyRunes /*[]int32*/;

// type MyFunc is a methodless func type — rendered inline as its base delegate

partial struct MyByte /*num:byte*/;

partial struct IntChan /*chan nint*/;

partial struct IntChanRecv /*<-chan nint*/;

partial struct IntChanSend /*chan<- nint*/;

partial struct BytesChan /*chan slice<byte>*/;

partial struct BytesChanRecv /*<-chan slice<byte>*/;

partial struct BytesChanSend /*chan<- slice<byte>*/;

partial struct convertTestsᴛ1 /*dyn*/ {
    internal reflectꓸValue @in;
    internal reflectꓸValue @out;
}
        partial struct iᴛ1 /*dyn*/ {
            internal nint x; /*`some:"foo"`*/
        }
        partial struct iᴛ2 /*dyn*/ {
            internal nint x; /*`some:"bar"`*/
        }
internal static slice<convertTestsᴛ1> convertTests = slice<convertTestsᴛ1>([
    // numbers
    /*
		Edit .+1,/\*\//-1>cat >/tmp/x.go && go run /tmp/x.go

		package main

		import "fmt"

		var numbers = []string{
			"int8", "uint8", "int16", "uint16",
			"int32", "uint32", "int64", "uint64",
			"int", "uint", "uintptr",
			"float32", "float64",
		}

		func main() {
			// all pairs but in an unusual order,
			// to emit all the int8, uint8 cases
			// before n grows too big.
			n := 1
			for i, f := range numbers {
				for _, g := range numbers[i:] {
					fmt.Printf("\t{V(%s(%d)), V(%s(%d))},\n", f, n, g, n)
					n++
					if f != g {
						fmt.Printf("\t{V(%s(%d)), V(%s(%d))},\n", g, n, f, n)
						n++
					}
				}
			}
		}
	*/
    new(V((int8)1), V((int8)1)),
    new(V((int8)2), V((uint8)2)),
    new(V((uint8)3), V((int8)3)),
    new(V((int8)4), V((int16)4)),
    new(V((int16)5), V((int8)5)),
    new(V((int8)6), V((uint16)6)),
    new(V((uint16)7), V((int8)7)),
    new(V((int8)8), V((int32)8)),
    new(V((int32)9), V((int8)9)),
    new(V((int8)10), V((uint32)10)),
    new(V((uint32)11), V((int8)11)),
    new(V((int8)12), V((int64)12)),
    new(V((int64)13), V((int8)13)),
    new(V((int8)14), V((uint64)14)),
    new(V((uint64)15), V((int8)15)),
    new(V((int8)16), V((nint)16)),
    new(V((nint)17), V((int8)17)),
    new(V((int8)18), V((nuint)18)),
    new(V((nuint)19), V((int8)19)),
    new(V((int8)20), V((uintptr)20)),
    new(V((uintptr)21), V((int8)21)),
    new(V((int8)22), V((float32)22F)),
    new(V((float32)23F), V((int8)23)),
    new(V((int8)24), V((float64)24D)),
    new(V((float64)25D), V((int8)25)),
    new(V((uint8)26), V((uint8)26)),
    new(V((uint8)27), V((int16)27)),
    new(V((int16)28), V((uint8)28)),
    new(V((uint8)29), V((uint16)29)),
    new(V((uint16)30), V((uint8)30)),
    new(V((uint8)31), V((int32)31)),
    new(V((int32)32), V((uint8)32)),
    new(V((uint8)33), V((uint32)33)),
    new(V((uint32)34), V((uint8)34)),
    new(V((uint8)35), V((int64)35)),
    new(V((int64)36), V((uint8)36)),
    new(V((uint8)37), V((uint64)37)),
    new(V((uint64)38), V((uint8)38)),
    new(V((uint8)39), V((nint)39)),
    new(V((nint)40), V((uint8)40)),
    new(V((uint8)41), V((nuint)41)),
    new(V((nuint)42), V((uint8)42)),
    new(V((uint8)43), V((uintptr)43)),
    new(V((uintptr)44), V((uint8)44)),
    new(V((uint8)45), V((float32)45F)),
    new(V((float32)46F), V((uint8)46)),
    new(V((uint8)47), V((float64)47D)),
    new(V((float64)48D), V((uint8)48)),
    new(V((int16)49), V((int16)49)),
    new(V((int16)50), V((uint16)50)),
    new(V((uint16)51), V((int16)51)),
    new(V((int16)52), V((int32)52)),
    new(V((int32)53), V((int16)53)),
    new(V((int16)54), V((uint32)54)),
    new(V((uint32)55), V((int16)55)),
    new(V((int16)56), V((int64)56)),
    new(V((int64)57), V((int16)57)),
    new(V((int16)58), V((uint64)58)),
    new(V((uint64)59), V((int16)59)),
    new(V((int16)60), V((nint)60)),
    new(V((nint)61), V((int16)61)),
    new(V((int16)62), V((nuint)62)),
    new(V((nuint)63), V((int16)63)),
    new(V((int16)64), V((uintptr)64)),
    new(V((uintptr)65), V((int16)65)),
    new(V((int16)66), V((float32)66F)),
    new(V((float32)67F), V((int16)67)),
    new(V((int16)68), V((float64)68D)),
    new(V((float64)69D), V((int16)69)),
    new(V((uint16)70), V((uint16)70)),
    new(V((uint16)71), V((int32)71)),
    new(V((int32)72), V((uint16)72)),
    new(V((uint16)73), V((uint32)73)),
    new(V((uint32)74), V((uint16)74)),
    new(V((uint16)75), V((int64)75)),
    new(V((int64)76), V((uint16)76)),
    new(V((uint16)77), V((uint64)77)),
    new(V((uint64)78), V((uint16)78)),
    new(V((uint16)79), V((nint)79)),
    new(V((nint)80), V((uint16)80)),
    new(V((uint16)81), V((nuint)81)),
    new(V((nuint)82), V((uint16)82)),
    new(V((uint16)83), V((uintptr)83)),
    new(V((uintptr)84), V((uint16)84)),
    new(V((uint16)85), V((float32)85F)),
    new(V((float32)86F), V((uint16)86)),
    new(V((uint16)87), V((float64)87D)),
    new(V((float64)88D), V((uint16)88)),
    new(V((int32)89), V((int32)89)),
    new(V((int32)90), V((uint32)90)),
    new(V((uint32)91), V((int32)91)),
    new(V((int32)92), V((int64)92)),
    new(V((int64)93), V((int32)93)),
    new(V((int32)94), V((uint64)94)),
    new(V((uint64)95), V((int32)95)),
    new(V((int32)96), V((nint)96)),
    new(V((nint)97), V((int32)97)),
    new(V((int32)98), V((nuint)98)),
    new(V((nuint)99), V((int32)99)),
    new(V((int32)100), V((uintptr)100)),
    new(V((uintptr)101), V((int32)101)),
    new(V((int32)102), V((float32)102F)),
    new(V((float32)103F), V((int32)103)),
    new(V((int32)104), V((float64)104D)),
    new(V((float64)105D), V((int32)105)),
    new(V((uint32)106), V((uint32)106)),
    new(V((uint32)107), V((int64)107)),
    new(V((int64)108), V((uint32)108)),
    new(V((uint32)109), V((uint64)109)),
    new(V((uint64)110), V((uint32)110)),
    new(V((uint32)111), V((nint)111)),
    new(V((nint)112), V((uint32)112)),
    new(V((uint32)113), V((nuint)113)),
    new(V((nuint)114), V((uint32)114)),
    new(V((uint32)115), V((uintptr)115)),
    new(V((uintptr)116), V((uint32)116)),
    new(V((uint32)117), V((float32)117F)),
    new(V((float32)118F), V((uint32)118)),
    new(V((uint32)119), V((float64)119D)),
    new(V((float64)120D), V((uint32)120)),
    new(V((int64)121), V((int64)121)),
    new(V((int64)122), V((uint64)122)),
    new(V((uint64)123), V((int64)123)),
    new(V((int64)124), V((nint)124)),
    new(V((nint)125), V((int64)125)),
    new(V((int64)126), V((nuint)126)),
    new(V((nuint)127), V((int64)127)),
    new(V((int64)128), V((uintptr)128)),
    new(V((uintptr)129), V((int64)129)),
    new(V((int64)130), V((float32)130F)),
    new(V((float32)131F), V((int64)131)),
    new(V((int64)132), V((float64)132D)),
    new(V((float64)133D), V((int64)133)),
    new(V((uint64)134), V((uint64)134)),
    new(V((uint64)135), V((nint)135)),
    new(V((nint)136), V((uint64)136)),
    new(V((uint64)137), V((nuint)137)),
    new(V((nuint)138), V((uint64)138)),
    new(V((uint64)139), V((uintptr)139)),
    new(V((uintptr)140), V((uint64)140)),
    new(V((uint64)141), V((float32)141F)),
    new(V((float32)142F), V((uint64)142)),
    new(V((uint64)143), V((float64)143D)),
    new(V((float64)144D), V((uint64)144)),
    new(V((nint)145), V((nint)145)),
    new(V((nint)146), V((nuint)146)),
    new(V((nuint)147), V((nint)147)),
    new(V((nint)148), V((uintptr)148)),
    new(V((uintptr)149), V((nint)149)),
    new(V((nint)150), V((float32)150F)),
    new(V((float32)151F), V((nint)151)),
    new(V((nint)152), V((float64)152D)),
    new(V((float64)153D), V((nint)153)),
    new(V((nuint)154), V((nuint)154)),
    new(V((nuint)155), V((uintptr)155)),
    new(V((uintptr)156), V((nuint)156)),
    new(V((nuint)157), V((float32)157F)),
    new(V((float32)158F), V((nuint)158)),
    new(V((nuint)159), V((float64)159D)),
    new(V((float64)160D), V((nuint)160)),
    new(V((uintptr)161), V((uintptr)161)),
    new(V((uintptr)162), V((float32)162F)),
    new(V((float32)163F), V((uintptr)163)),
    new(V((uintptr)164), V((float64)164D)),
    new(V((float64)165D), V((uintptr)165)),
    new(V((float32)166F), V((float32)166F)),
    new(V((float32)167F), V((float64)167D)),
    new(V((float64)168D), V((float32)168F)),
    new(V((float64)169D), V((float64)169D)),

    // truncation
    new(V((float64)1.5D), V((nint)1)),

    // complex
    new(V((complex64)1F.i()), V((complex64)1F.i())),
    new(V((complex64)2F.i()), V((complex128)2D.i())),
    new(V((complex128)3D.i()), V((complex64)3F.i())),
    new(V((complex128)4D.i()), V((complex128)4D.i())),

    // string
    new(V((@string)"hello"u8), V((@string)"hello"u8)),
    new(V((@string)"bytes1"u8), V(slice<byte>("bytes1"u8))),
    new(V(slice<byte>("bytes2"u8)), V((@string)"bytes2"u8)),
    new(V(slice<byte>("bytes3"u8)), V(slice<byte>("bytes3"u8))),
    new(V((@string)"runes♝"u8), V(slice<rune>((@string)"runes♝"))),
    new(V(slice<rune>((@string)"runes♕")), V((@string)"runes♕"u8)),
    new(V(slice<rune>((@string)"runes🙈🙉🙊")), V(slice<rune>((@string)"runes🙈🙉🙊"))),
    new(V((nint)'a'), V((@string)"a"u8)),
    new(V((int8)'a'), V((@string)"a"u8)),
    new(V((int16)'a'), V((@string)"a"u8)),
    new(V((int32)'a'), V((@string)"a"u8)),
    new(V((int64)'a'), V((@string)"a"u8)),
    new(V((nuint)'a'), V((@string)"a"u8)),
    new(V((uint8)'a'), V((@string)"a"u8)),
    new(V((uint16)'a'), V((@string)"a"u8)),
    new(V((uint32)'a'), V((@string)"a"u8)),
    new(V((uint64)'a'), V((@string)"a"u8)),
    new(V((uintptr)(rune)'a'), V((@string)"a"u8)),
    new(V((nint)(-1)), V((@string)"\uFFFD"u8)),
    new(V((int8)(-2)), V((@string)"\uFFFD"u8)),
    new(V((int16)(-3)), V((@string)"\uFFFD"u8)),
    new(V((int32)(-4)), V((@string)"\uFFFD"u8)),
    new(V((int64)(-5)), V((@string)"\uFFFD"u8)),
    new(V((int64)(-4294967296L)), V((@string)"\uFFFD"u8)),
    new(V((int64)(4294967296L)), V((@string)"\uFFFD"u8)),
    new(V((nuint)0x110001), V((@string)"\uFFFD"u8)),
    new(V((uint32)0x110002), V((@string)"\uFFFD"u8)),
    new(V((uint64)0x110003), V((@string)"\uFFFD"u8)),
    new(V((uint64)((uint64)1 << 32)), V((@string)"\uFFFD"u8)),
    new(V((uintptr)0x110004), V((@string)"\uFFFD"u8)),

    // named string
    new(V((MyString)(@string)"hello"), V((@string)"hello"u8)),
    new(V((@string)"hello"u8), V((MyString)(@string)"hello")),
    new(V((@string)"hello"u8), V((@string)"hello"u8)),
    new(V((MyString)(@string)"hello"), V((MyString)(@string)"hello")),
    new(V((MyString)(@string)"bytes1"), V(slice<byte>("bytes1"u8))),
    new(V(slice<byte>("bytes2"u8)), V((MyString)(@string)"bytes2")),
    new(V(slice<byte>("bytes3"u8)), V(slice<byte>("bytes3"u8))),
    new(V((MyString)(@string)"runes♝"), V(slice<rune>((@string)"runes♝"))),
    new(V(slice<rune>((@string)"runes♕")), V((MyString)(@string)"runes♕")),
    new(V(slice<rune>((@string)"runes🙈🙉🙊")), V(slice<rune>((@string)"runes🙈🙉🙊"))),
    new(V(slice<rune>((@string)"runes🙈🙉🙊")), V((MyRunes)slice<int32>((@string)"runes🙈🙉🙊"u8))),
    new(V((MyRunes)slice<int32>((@string)"runes🙈🙉🙊"u8)), V(slice<rune>((@string)"runes🙈🙉🙊"))),
    new(V((nint)'a'), V((MyString)(@string)"a")),
    new(V((int8)'a'), V((MyString)(@string)"a")),
    new(V((int16)'a'), V((MyString)(@string)"a")),
    new(V((int32)'a'), V((MyString)(@string)"a")),
    new(V((int64)'a'), V((MyString)(@string)"a")),
    new(V((nuint)'a'), V((MyString)(@string)"a")),
    new(V((uint8)'a'), V((MyString)(@string)"a")),
    new(V((uint16)'a'), V((MyString)(@string)"a")),
    new(V((uint32)'a'), V((MyString)(@string)"a")),
    new(V((uint64)'a'), V((MyString)(@string)"a")),
    new(V((uintptr)(rune)'a'), V((MyString)(@string)"a")),
    new(V((nint)(-1)), V((MyString)(@string)"\uFFFD")),
    new(V((int8)(-2)), V((MyString)(@string)"\uFFFD")),
    new(V((int16)(-3)), V((MyString)(@string)"\uFFFD")),
    new(V((int32)(-4)), V((MyString)(@string)"\uFFFD")),
    new(V((int64)(-5)), V((MyString)(@string)"\uFFFD")),
    new(V((nuint)0x110001), V((MyString)(@string)"\uFFFD")),
    new(V((uint32)0x110002), V((MyString)(@string)"\uFFFD")),
    new(V((uint64)0x110003), V((MyString)(@string)"\uFFFD")),
    new(V((uintptr)0x110004), V((MyString)(@string)"\uFFFD")),

    // named []byte
    new(V((@string)"bytes1"u8), V((MyBytes)slice<byte>((@string)"bytes1"u8))),
    new(V((MyBytes)slice<byte>((@string)"bytes2"u8)), V((@string)"bytes2"u8)),
    new(V((MyBytes)slice<byte>((@string)"bytes3"u8)), V((MyBytes)slice<byte>((@string)"bytes3"u8))),
    new(V((MyString)(@string)"bytes1"), V((MyBytes)slice<byte>((@string)"bytes1"u8))),
    new(V((MyBytes)slice<byte>((@string)"bytes2"u8)), V((MyString)(@string)"bytes2")),

    // named []rune
    new(V((@string)"runes♝"u8), V((MyRunes)slice<int32>((@string)"runes♝"u8))),
    new(V((MyRunes)slice<int32>((@string)"runes♕"u8)), V((@string)"runes♕"u8)),
    new(V((MyRunes)slice<int32>((@string)"runes🙈🙉🙊"u8)), V((MyRunes)slice<int32>((@string)"runes🙈🙉🙊"u8))),
    new(V((MyString)(@string)"runes♝"), V((MyRunes)slice<int32>((@string)"runes♝"u8))),
    new(V((MyRunes)slice<int32>((@string)"runes♕"u8)), V((MyString)(@string)"runes♕")),

    // slice to array
    new(V(slice<byte>(default)), V(new byte[]{}.array())),
    new(V(slice<byte>([])), V(new byte[]{}.array())),
    new(V(slice<byte>([1])), V(new byte[]{1}.array())),
    new(V(slice<byte>([1, 2])), V(new byte[]{1, 2}.array())),
    new(V(slice<byte>([1, 2, 3])), V(new byte[]{1, 2, 3}.array())),
    new(V((MyBytes)slice<byte>(default)), V(new byte[]{}.array())),
    new(V(new MyBytes(new byte[]{}.slice())), V(new byte[]{}.array())),
    new(V(new MyBytes(new byte[]{1}.slice())), V(new byte[]{1}.array())),
    new(V(new MyBytes(new byte[]{1, 2}.slice())), V(new byte[]{1, 2}.array())),
    new(V(new MyBytes(new byte[]{1, 2, 3}.slice())), V(new byte[]{1, 2, 3}.array())),
    new(V(slice<byte>(default)), V(new MyBytesArray0(new byte[]{}.array()))),
    new(V(slice<byte>([])), V((MyBytesArray0)new byte[]{}.array())),
    new(V(slice<byte>([1, 2, 3, 4])), V(((MyBytesArray)new byte[]{1, 2, 3, 4}.array()))),
    new(V(new MyBytes(new byte[]{}.slice())), V((MyBytesArray0)new byte[]{}.array())),
    new(V(new MyBytes(new byte[]{5, 6, 7, 8}.slice())), V(((MyBytesArray)new byte[]{5, 6, 7, 8}.array()))),
    new(V(slice<MyByte>([])), V(new MyByte[]{}.array())),
    new(V(slice<MyByte>([1, 2])), V(new MyByte[]{1, 2}.array())),

    // slice to array pointer
    new(V(slice<byte>(default)), V(ж<array<byte>>.NilBoxOfDims(0L))),
    new(V(slice<byte>([])), V(Ꮡ(new array<byte>(0)))),
    new(V(slice<byte>([7])), V(Ꮡ(new byte[]{7}.array()))),
    new(V((MyBytes)slice<byte>(default)), V(ж<array<byte>>.NilBoxOfDims(0L))),
    new(V((MyBytes)slice<byte>([])), V(Ꮡ(new array<byte>(0)))),
    new(V((MyBytes)slice<byte>([9])), V(Ꮡ(new byte[]{9}.array()))),
    new(V(slice<byte>(default)), V((MyBytesArrayPtr0)nil)),
    new(V(slice<byte>([])), V(new MyBytesArrayPtr0(Ꮡ(new array<byte>(0))))),
    new(V(slice<byte>([1, 2, 3, 4])), V(new MyBytesArrayPtr(Ꮡ(new byte[]{1, 2, 3, 4}.array())))),
    new(V((MyBytes)slice<byte>([])), V(new MyBytesArrayPtr0(Ꮡ(new array<byte>(0))))),
    new(V((MyBytes)slice<byte>([5, 6, 7, 8])), V(new MyBytesArrayPtr(Ꮡ(new byte[]{5, 6, 7, 8}.array())))),

    new(V(slice<byte>(default)), V(ж<MyBytesArray0>.NilBoxOfDims(0L))),
    new(V(slice<byte>([])), V(Ꮡ(new MyBytesArray0(new array<byte>(0))))),
    new(V(slice<byte>([1, 2, 3, 4])), V(Ꮡ(new MyBytesArray(new byte[]{1, 2, 3, 4}.array())))),
    new(V((MyBytes)slice<byte>(default)), V(ж<MyBytesArray0>.NilBoxOfDims(0L))),
    new(V((MyBytes)slice<byte>([])), V(Ꮡ(new MyBytesArray0(new array<byte>(0))))),
    new(V((MyBytes)slice<byte>([5, 6, 7, 8])), V(Ꮡ(new MyBytesArray(new byte[]{5, 6, 7, 8}.array())))),
    new(V(Ꮡ(new array<byte>(0))), V(@new<MyBytesArray0>())),
    new(V(@new<MyBytesArray0>()), V(Ꮡ(new array<byte>(0)))),
    new(V((MyBytesArrayPtr0)nil), V(ж<array<byte>>.NilBoxOfDims(0L))),
    new(V(ж<array<byte>>.NilBoxOfDims(0L)), V((MyBytesArrayPtr0)nil)),

    // named types and equal underlying types
    new(V(@new<nint>()), V(@new<integer>())),
    new(V(@new<integer>()), V(@new<nint>())),
    new(V(new Empty()), V(new EmptyStruct())),
    new(V(@new<Empty>()), V(@new<EmptyStruct>())),
    new(V(new EmptyStruct()), V(new Empty())),
    new(V(@new<EmptyStruct>()), V(@new<Empty>())),
    new(V(new Empty()), V(new Empty())),
    new(V(new MyBytes(new byte[]{}.slice())), V(slice<byte>([]))),
    new(V(slice<byte>([])), V(new MyBytes(new byte[]{}.slice()))),
    new(V(((Action)(default)).OrTypedNilFunc()), V((default(Action)!).OrTypedNilFunc())),
    new(V((default(Action)!).OrTypedNilFunc()), V(((Action)(default)).OrTypedNilFunc())),

    // structs with different tags
    new(V(new iᴛ1()), V(new iᴛ2())),

    new(V(new iᴛ2()), V(new iᴛ1())),

    new(V(new MyStruct()), V(new iᴛ1())),

    new(V(new iᴛ1()), V(new MyStruct())),

    new(V(new MyStruct()), V(new iᴛ2())),

    new(V(new iᴛ2()), V(new MyStruct())),

    new(V(new MyStruct1()), V(new MyStruct2())),
    new(V(new MyStruct2()), V(new MyStruct1())),

    // can convert *byte and *MyByte
    new(V((ж<byte>)nil), V((ж<MyByte>)nil)),
    new(V((ж<MyByte>)nil), V((ж<byte>)nil)),

    // cannot convert mismatched array sizes
    new(V(new byte[]{}.array(2)), V(new byte[]{}.array(2))),
    new(V(new byte[]{}.array(3)), V(new byte[]{}.array(3))),
    new(V(new MyBytesArray0(new byte[]{}.array())), V(new byte[]{}.array())),
    new(V(new byte[]{}.array()), V(new MyBytesArray0(new byte[]{}.array()))),

    // cannot convert other instances
    new(V((ж<ж<byte>>)nil), V((ж<ж<byte>>)nil)),
    new(V((ж<ж<MyByte>>)nil), V((ж<ж<MyByte>>)nil)),
    new(V((channel<byte>)(default)), V((channel<byte>)(default))),
    new(V((channel<MyByte>)(default)), V((channel<MyByte>)(default))),
    new(V((slice<byte>)(default)), V((slice<byte>)(default))),
    new(V((slice<MyByte>)(default)), V((slice<MyByte>)(default))),
    new(V((map<nint, byte>)default), V((map<nint, byte>)default)),
    new(V((map<nint, MyByte>)default), V((map<nint, MyByte>)default)),
    new(V((map<byte, nint>)default), V((map<byte, nint>)default)),
    new(V((map<MyByte, nint>)default), V((map<MyByte, nint>)default)),
    new(V(new byte[]{}.array(2)), V(new byte[]{}.array(2))),
    new(V(new MyByte[]{}.array(2)), V(new MyByte[]{}.array(2))),

    // other
    new(V((ж<ж<ж<nint>>>)nil), V((ж<ж<ж<nint>>>)nil)),
    new(V((ж<ж<ж<byte>>>)nil), V((ж<ж<ж<byte>>>)nil)),
    new(V((ж<ж<ж<int32>>>)nil), V((ж<ж<ж<int32>>>)nil)),
    new(V((ж<ж<ж<int64>>>)nil), V((ж<ж<ж<int64>>>)nil)),
    new(V((channel<byte>)(default)), V((channel<byte>)(default))),
    new(V((channel<MyByte>)(default)), V((channel<MyByte>)(default))),
    new(V((map<nint, bool>)default), V((map<nint, bool>)default)),
    new(V((map<nint, byte>)default), V((map<nint, byte>)default)),
    new(V((map<nuint, bool>)default), V((map<nuint, bool>)default)),
    new(V(slice<nuint>(default)), V(slice<nuint>(default))),
    new(V(slice<nint>(default)), V(slice<nint>(default))),
    new(V(@new<any>()), V(@new<any>())),
    new(V(@new<Δio.Reader>()), V(@new<Δio.Reader>())),
    new(V(@new<Δio.Writer>()), V(@new<Δio.Writer>())),

    // channels
    new(V((IntChan)default), V(channel/*<-*/<nint>.SendOnly)),
    new(V((IntChan)default), V(/*<-*/channel<nint>.RecvOnly)),
    new(V((channel<nint>)(default)), V((IntChanRecv)default)),
    new(V((channel<nint>)(default)), V((IntChanSend)default)),
    new(V((IntChanRecv)default), V(/*<-*/channel<nint>.RecvOnly)),
    new(V(/*<-*/channel<nint>.RecvOnly), V((IntChanRecv)default)),
    new(V((IntChanSend)default), V(channel/*<-*/<nint>.SendOnly)),
    new(V(channel/*<-*/<nint>.SendOnly), V((IntChanSend)default)),
    new(V((IntChan)default), V((channel<nint>)(default))),
    new(V((channel<nint>)(default)), V((IntChan)default)),
    new(V((channel<nint>)(default)), V(/*<-*/channel<nint>.RecvOnly)),
    new(V((channel<nint>)(default)), V(channel/*<-*/<nint>.SendOnly)),
    new(V((BytesChan)default), V(channel/*<-*/<slice<byte>>.SendOnly)),
    new(V((BytesChan)default), V(/*<-*/channel<slice<byte>>.RecvOnly)),
    new(V((channel<slice<byte>>)(default)), V((BytesChanRecv)default)),
    new(V((channel<slice<byte>>)(default)), V((BytesChanSend)default)),
    new(V((BytesChanRecv)default), V(/*<-*/channel<slice<byte>>.RecvOnly)),
    new(V(/*<-*/channel<slice<byte>>.RecvOnly), V((BytesChanRecv)default)),
    new(V((BytesChanSend)default), V(channel/*<-*/<slice<byte>>.SendOnly)),
    new(V(channel/*<-*/<slice<byte>>.SendOnly), V((BytesChanSend)default)),
    new(V((BytesChan)default), V((channel<slice<byte>>)(default))),
    new(V((channel<slice<byte>>)(default)), V((BytesChan)default)),
    new(V((channel<slice<byte>>)(default)), V(/*<-*/channel<slice<byte>>.RecvOnly)),
    new(V((channel<slice<byte>>)(default)), V(channel/*<-*/<slice<byte>>.SendOnly)),

    // cannot convert other instances (channels)
    new(V((IntChan)default), V((IntChan)default)),
    new(V((IntChanRecv)default), V((IntChanRecv)default)),
    new(V((IntChanSend)default), V((IntChanSend)default)),
    new(V((BytesChan)default), V((BytesChan)default)),
    new(V((BytesChanRecv)default), V((BytesChanRecv)default)),
    new(V((BytesChanSend)default), V((BytesChanSend)default)),

    // interfaces
    new(V((nint)1), EmptyInterfaceV((nint)1)),
    new(V((@string)"hello"u8), EmptyInterfaceV((@string)"hello"u8)),
    new(V(@new<bytes.Buffer>()), ReaderV(new reflect_test_package.bytes_BufferжReader(@new<bytes.Buffer>()))),
    new(ReadWriterV(new reflect_test_package.bytes_BufferжReadWriter(@new<bytes.Buffer>())), ReaderV(new reflect_test_package.bytes_BufferжReader(@new<bytes.Buffer>()))),
    new(V(@new<bytes.Buffer>()), ReadWriterV(new reflect_test_package.bytes_BufferжReadWriter(@new<bytes.Buffer>())))
]);

public static void TestConvert(ж<Δtesting.T> t) {
    var canConvert = new map<array<reflectꓸType>, bool>{};
    var all = new map<reflectꓸType, bool>{};

    foreach (var (_, tt) in convertTests) {
        var t1 = tt.@in.Type();
        if (!t1.ConvertibleTo(t1)) {
            t.Errorf("(%s).ConvertibleTo(%s) = false, want true", t1, t1);
            continue;
        }

        var t2 = tt.@out.Type();
        if (!t1.ConvertibleTo(t2)) {
            t.Errorf("(%s).ConvertibleTo(%s) = false, want true", t1, t2);
            continue;
        }

        all[t1] = true;
        all[t2] = true;
        canConvert[new reflectꓸType[]{t1, t2}.array()] = true;

        // vout1 represents the in value converted to the in type.
        var v1 = tt.@in;
        if (!v1.CanConvert(t1)) {
            t.Errorf("ValueOf(%T(%[1]v)).CanConvert(%s) = false, want true", tt.@in.Interface(), t1);
        }
        var vout1 = v1.Convert(t1);
        var out1 = vout1.Interface();
        if (!AreEqual(vout1.Type(), tt.@in.Type()) || !DeepEqual(out1, tt.@in.Interface())) {
            t.Errorf("ValueOf(%T(%[1]v)).Convert(%s) = %T(%[3]v), want %T(%[4]v)", tt.@in.Interface(), t1, out1, tt.@in.Interface());
        }

        // vout2 represents the in value converted to the out type.
        if (!v1.CanConvert(t2)) {
            t.Errorf("ValueOf(%T(%[1]v)).CanConvert(%s) = false, want true", tt.@in.Interface(), t2);
        }
        var vout2 = v1.Convert(t2);
        var out2 = vout2.Interface();
        if (!AreEqual(vout2.Type(), tt.@out.Type()) || !DeepEqual(out2, tt.@out.Interface())) {
            t.Errorf("ValueOf(%T(%[1]v)).Convert(%s) = %T(%[3]v), want %T(%[4]v)", tt.@in.Interface(), t2, out2, tt.@out.Interface());
        }
        {
            reflectꓸKind got = vout2.Kind();
            reflectꓸKind want = vout2.Type().Kind(); if (got != want) {
                t.Errorf("ValueOf(%T(%[1]v)).Convert(%s) has internal kind %v want %v", tt.@in.Interface(), t1, got, want);
            }
        }

        // vout3 represents a new value of the out type, set to vout2.  This makes
        // sure the converted value vout2 is really usable as a regular value.
        var vout3 = New(t2).Elem();
        vout3.Set(vout2);
        var out3 = vout3.Interface();
        if (!AreEqual(vout3.Type(), tt.@out.Type()) || !DeepEqual(out3, tt.@out.Interface())) {
            t.Errorf("Set(ValueOf(%T(%[1]v)).Convert(%s)) = %T(%[3]v), want %T(%[4]v)", tt.@in.Interface(), t2, out3, tt.@out.Interface());
        }

        if (reflect_internal_test_package.IsRO(v1)) {
            t.Errorf("table entry %v is RO, should not be", v1);
        }
        if (reflect_internal_test_package.IsRO(vout1)) {
            t.Errorf("self-conversion output %v is RO, should not be", vout1);
        }
        if (reflect_internal_test_package.IsRO(vout2)) {
            t.Errorf("conversion output %v is RO, should not be", vout2);
        }
        if (reflect_internal_test_package.IsRO(vout3)) {
            t.Errorf("set(conversion output) %v is RO, should not be", vout3);
        }
        if (!reflect_internal_test_package.IsRO(reflect_internal_test_package.MakeRO(v1).Convert(t1))) {
            t.Errorf("RO self-conversion output %v is not RO, should be", v1);
        }
        if (!reflect_internal_test_package.IsRO(reflect_internal_test_package.MakeRO(v1).Convert(t2))) {
            t.Errorf("RO conversion output %v is not RO, should be", v1);
        }
    }

    // Assume that of all the types we saw during the tests,
    // if there wasn't an explicit entry for a conversion between
    // a pair of types, then it's not to be allowed. This checks for
    // things like 'int64' converting to '*int'.
    foreach (var (t1, _) in all) {
        foreach (var (t2, _) in all) {
            var expectOK = AreEqual(t1, t2) || canConvert[new reflectꓸType[]{t1, t2}.array()] || t2.Kind() == ΔInterface && t2.NumMethod() == 0;
            {
                var ok = t1.ConvertibleTo(t2); if (ok != expectOK) {
                    t.Errorf("(%s).ConvertibleTo(%s) = %v, want %v", t1, t2, ok, expectOK);
                }
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string reflectCannotConvertˢ => ref reflectCannotConvertˢᵇ;
private static readonly @string reflectCannotConvertˢᵇ = "reflect: cannot convert slice with length 4 to pointer to array with length 8"u8;
internal static ref readonly @string reflectCannotConvertˢ2 => ref reflectCannotConvertˢ2ᵇ;
private static readonly @string reflectCannotConvertˢ2ᵇ = "reflect: cannot convert slice with length 4 to array with length 8"u8;

public static void TestConvertPanic(ж<Δtesting.T> t) {
    var s = new slice<byte>(4);
    var p = Ꮡ(new array<byte>(8));
    ref var v = ref heap<reflectꓸValue>(ValueOf(s), out var Ꮡv);
    var pt = TypeOf(p.OrTypedNil());
    if (!v.Type().ConvertibleTo(pt)) {
        t.Errorf("[]byte should be convertible to *[8]byte");
    }
    if (v.CanConvert(pt)) {
        t.Errorf("slice with length 4 should not be convertible to *[8]byte");
    }
    var ptʗ1 = pt;
    var vʗ1 = v;
    shouldPanic(reflectCannotConvertˢ, () => {
        _ = vʗ1.Convert(ptʗ1);
    });

    if (v.CanConvert(pt.Elem())) {
        t.Errorf("slice with length 4 should not be convertible to [8]byte");
    }
    var ptʗ2 = pt;
    var vʗ2 = v;
    shouldPanic(reflectCannotConvertˢ2, () => {
        _ = vʗ2.Convert(ptʗ2.Elem());
    });
}

public static void TestConvertSlice2Array(ж<Δtesting.T> t) {
    var s = new slice<nint>(4);
    var p = new nint[]{}.array(4);
    var pt = TypeOf(p);
    var ov = ValueOf(s);
    var v = ov.Convert(pt);
    // Converting a slice to non-empty array needs to return
    // a non-addressable copy of the original memory.
    if (v.CanAddr()) {
        t.Fatalf("convert slice to non-empty array returns an addressable copy array");
    }
    foreach (var i in range(s)) {
        ov.Index(i).Set(ValueOf(i + 1));
    }
    foreach (var i in range(s)) {
        if (v.Index(i).Int() != 0) {
            t.Fatalf("slice (%v) mutation visible in converted result (%v)", ov, v);
        }
    }
}

internal static float32 gFloat32;

internal const uint32 snan = 0x7f800001;

// Test reflect's conversion between float32s. See issue 36400.
internal partial struct TestConvertNaNs_myFloat32 /*num:float32*/;

public static void TestConvertNaNs(ж<Δtesting.T> t) {
    // Test to see if a store followed by a load of a signaling NaN
    // maintains the signaling bit. (This used to fail on the 387 port.)
    gFloat32 = Δmath.Float32frombits(snan);
    Δruntime.Gosched(); // make sure we don't optimize the store/load away
    {
        var got = Δmath.Float32bits(gFloat32); if (got != snan) {
            t.Errorf("store/load of sNaN not faithful, got %x want %x", got, snan);
        }
    }
    var x = V((TestConvertNaNs_myFloat32)Δmath.Float32frombits(snan));
    var y = x.Convert(TypeOf((float32)0F));
    var z = y.Interface()._<float32>();
    {
        var got = Δmath.Float32bits(z); if (got != snan) {
            t.Errorf("signaling nan conversion got %x, want %x", got, snan);
        }
    }
}

partial struct ComparableStruct {
    public nint X;
}

partial struct NonComparableStruct {
    public nint X;
    public map<@string, nint> Y;
}

partial struct comparableTestsᴛ1 /*dyn*/ {
    internal reflectꓸType typ;
    internal bool ok;
}
internal static slice<comparableTestsᴛ1> comparableTests = slice<comparableTestsᴛ1>([
    new(TypeOf((nint)1), true),
    new(TypeOf("hello"), true),
    new(TypeOf(@new<byte>()), true),
    new(TypeOf(((Action)(default)).OrTypedNilFunc()), false),
    new(TypeOf(slice<byte>([])), false),
    new(TypeOf(new map<@string, nint>{}), false),
    new(TypeOf(new channel<nint>(0)), true),
    new(TypeOf(1.5D), true),
    new(TypeOf(false), true),
    new(TypeOf(1D.i()), true),
    new(TypeOf(new ComparableStruct()), true),
    new(TypeOf(new NonComparableStruct()), false),
    new(TypeOf(new map<@string, nint>[]{}.array(10)), false),
    new(TypeOf(new @string[]{}.array(10)), true),
    new(TypeOf(@new<any>()).Elem(), true)
]);

public static void TestComparable(ж<Δtesting.T> t) {
    foreach (var (_, tt) in comparableTests) {
        {
            var ok = tt.typ.Comparable(); if (ok != tt.ok) {
                t.Errorf("TypeOf(%v).Comparable() = %v, want %v", tt.typ, ok, tt.ok);
            }
        }
    }
}

public static void TestValueOverflow(ж<Δtesting.T> t) {
    {
        var ovf = V((float64)0D).OverflowFloat(1e300D); if (ovf) {
            t.Errorf("%v wrongly overflows float64", 1e300D);
        }
    }

    var maxFloat32 = (float64)(340282346638528859811704183484516925440D);
    {
        var ovf = V((float32)0F).OverflowFloat(maxFloat32); if (ovf) {
            t.Errorf("%v wrongly overflows float32", maxFloat32);
        }
    }
    var ovfFloat32 = (float64)(340282346638528897590636046441678635008D);
    {
        var ovf = V((float32)0F).OverflowFloat(ovfFloat32); if (!ovf) {
            t.Errorf("%v should overflow float32", ovfFloat32);
        }
    }
    {
        var ovf = V((float32)0F).OverflowFloat(-ovfFloat32); if (!ovf) {
            t.Errorf("%v should overflow float32", -ovfFloat32);
        }
    }

    var maxInt32 = (int64)0x7fffffff;
    {
        var ovf = V((int32)0).OverflowInt(maxInt32); if (ovf) {
            t.Errorf("%v wrongly overflows int32", maxInt32);
        }
    }
    {
        var ovf = V((int32)0).OverflowInt((int64)(-1) << 31); if (ovf) {
            t.Errorf("%v wrongly overflows int32", -(int64)1 << 31);
        }
    }
    var ovfInt32 = (int64)(2147483648L);
    {
        var ovf = V((int32)0).OverflowInt(ovfInt32); if (!ovf) {
            t.Errorf("%v should overflow int32", ovfInt32);
        }
    }

    var maxUint32 = (uint64)0xffffffffU;
    {
        var ovf = V((uint32)0).OverflowUint(maxUint32); if (ovf) {
            t.Errorf("%v wrongly overflows uint32", maxUint32);
        }
    }
    var ovfUint32 = (uint64)((uint64)1 << 32);
    {
        var ovf = V((uint32)0).OverflowUint(ovfUint32); if (!ovf) {
            t.Errorf("%v should overflow uint32", ovfUint32);
        }
    }
}

public static void TestTypeOverflow(ж<Δtesting.T> t) {
    {
        var ovf = TypeFor<float64>().OverflowFloat(1e300D); if (ovf) {
            t.Errorf("%v wrongly overflows float64", 1e300D);
        }
    }

    var maxFloat32 = (float64)(340282346638528859811704183484516925440D);
    {
        var ovf = TypeFor<float32>().OverflowFloat(maxFloat32); if (ovf) {
            t.Errorf("%v wrongly overflows float32", maxFloat32);
        }
    }
    var ovfFloat32 = (float64)(340282346638528897590636046441678635008D);
    {
        var ovf = TypeFor<float32>().OverflowFloat(ovfFloat32); if (!ovf) {
            t.Errorf("%v should overflow float32", ovfFloat32);
        }
    }
    {
        var ovf = TypeFor<float32>().OverflowFloat(-ovfFloat32); if (!ovf) {
            t.Errorf("%v should overflow float32", -ovfFloat32);
        }
    }

    var maxInt32 = (int64)0x7fffffff;
    {
        var ovf = TypeFor<int32>().OverflowInt(maxInt32); if (ovf) {
            t.Errorf("%v wrongly overflows int32", maxInt32);
        }
    }
    {
        var ovf = TypeFor<int32>().OverflowInt((int64)(-1) << 31); if (ovf) {
            t.Errorf("%v wrongly overflows int32", -(int64)1 << 31);
        }
    }
    var ovfInt32 = (int64)(2147483648L);
    {
        var ovf = TypeFor<int32>().OverflowInt(ovfInt32); if (!ovf) {
            t.Errorf("%v should overflow int32", ovfInt32);
        }
    }

    var maxUint32 = (uint64)0xffffffffU;
    {
        var ovf = TypeFor<uint32>().OverflowUint(maxUint32); if (ovf) {
            t.Errorf("%v wrongly overflows uint32", maxUint32);
        }
    }
    var ovfUint32 = (uint64)((uint64)1 << 32);
    {
        var ovf = TypeFor<uint32>().OverflowUint(ovfUint32); if (!ovf) {
            t.Errorf("%v should overflow uint32", ovfUint32);
        }
    }
}

internal static partial void checkSameType(ж<Δtesting.T> t, reflectꓸType x, any y) {
    if (!AreEqual(x, TypeOf(y)) || !AreEqual(TypeOf(Zero(x).Interface()), TypeOf(y))) {
        t.Errorf("did not find preexisting type for %s (vs %s)", TypeOf(x), TypeOf(y));
    }
}

internal partial struct TestArrayOf_tests /*dyn*/ {
    internal nint n;
    internal Func<nint, any> value;
    internal bool comparable;
    internal @string want;
}

internal partial struct TestArrayOf_Tint /*num:nint*/;

internal partial struct TestArrayOf_Tintᴛ1 /*num:nint*/;

internal partial struct TestArrayOf_Tfloat /*num:float64*/;

internal partial struct TestArrayOf_Tstring /*@string*/;

internal partial struct TestArrayOf_Tstruct /*dyn*/ {
    public nint V;
}

internal partial struct TestArrayOf_Tintᴛ2 /*num:nint*/;

internal partial struct TestArrayOf_Tintᴛ3 /*num:nint*/;

internal partial struct TestArrayOf_Tstructᴛ1 /*dyn*/ {
    public array<nint> V = new(1);
}

internal partial struct TestArrayOf_Tstructᴛ2 /*dyn*/ {
    public slice<nint> V;
}

internal partial struct TestArrayOf_TstructUV /*dyn*/ {
    public nint U, V;
}

internal partial struct TestArrayOf_TstructUVᴛ1 /*dyn*/ {
    public nint U;
    public float64 V;
}

// check that type already in binary is found
internal partial struct TestArrayOf_T /*num:nint*/;

public static void TestArrayOf(ж<Δtesting.T> t) {
    // check construction and use of type not in binary
    var tests = slice<TestArrayOf_tests>([
        new(
            n: 0,
            value: (nint i) => {
                return (TestArrayOf_Tint)i;
            },
            comparable: true,
            want: "[]"
        ),
        new(
            n: 10,
            value: (nint i) => {
                return (TestArrayOf_Tintᴛ1)i;
            },
            comparable: true,
            want: "[0 1 2 3 4 5 6 7 8 9]"
        ),
        new(
            n: 10,
            value: (nint i) => {
                return (TestArrayOf_Tfloat)(float64)i;
            },
            comparable: true,
            want: "[0 1 2 3 4 5 6 7 8 9]"
        ),
        new(
            n: 10,
            value: (nint i) => {
                return (TestArrayOf_Tstring)strconv.Itoa(i);
            },
            comparable: true,
            want: "[0 1 2 3 4 5 6 7 8 9]"
        ),
        new(
            n: 10,
            value: (nint i) => {
                return new TestArrayOf_Tstruct(i);
            },
            comparable: true,
            want: "[{0} {1} {2} {3} {4} {5} {6} {7} {8} {9}]"
        ),
        new(
            n: 10,
            value: (nint i) => {
                return slice<TestArrayOf_Tintᴛ2>([(TestArrayOf_Tintᴛ2)i]);
            },
            comparable: false,
            want: "[[0] [1] [2] [3] [4] [5] [6] [7] [8] [9]]"
        ),
        new(
            n: 10,
            value: (nint i) => {
                return new TestArrayOf_Tintᴛ3[]{(TestArrayOf_Tintᴛ3)i}.array();
            },
            comparable: true,
            want: "[[0] [1] [2] [3] [4] [5] [6] [7] [8] [9]]"
        ),
        new(
            n: 10,
            value: (nint i) => {
                return new TestArrayOf_Tstructᴛ1(new nint[]{i}.array());
            },
            comparable: true,
            want: "[{[0]} {[1]} {[2]} {[3]} {[4]} {[5]} {[6]} {[7]} {[8]} {[9]}]"
        ),
        new(
            n: 10,
            value: (nint i) => {
                return new TestArrayOf_Tstructᴛ2(slice<nint>([i]));
            },
            comparable: false,
            want: "[{[0]} {[1]} {[2]} {[3]} {[4]} {[5]} {[6]} {[7]} {[8]} {[9]}]"
        ),
        new(
            n: 10,
            value: (nint i) => {
                return new TestArrayOf_TstructUV(i, i);
            },
            comparable: true,
            want: "[{0 0} {1 1} {2 2} {3 3} {4 4} {5 5} {6 6} {7 7} {8 8} {9 9}]"
        ),
        new(
            n: 10,
            value: (nint i) => {
                return new TestArrayOf_TstructUVᴛ1(i, (float64)i);
            },
            comparable: true,
            want: "[{0 0} {1 1} {2 2} {3 3} {4 4} {5 5} {6 6} {7 7} {8 8} {9 9}]"
        )
    ]);

    foreach (var (_, table) in tests) {
        var at = ArrayOf(table.n, TypeOf(table.value(0)));
        var v = New(at).Elem();
        var vok = New(at).Elem();
        var vnot = New(at).Elem();
        for (nint i = 0; i < v.Len(); i++) {
            v.Index(i).Set(ValueOf(table.value(i)));
            vok.Index(i).Set(ValueOf(table.value(i)));
            nint j = i;
            if (i + 1 == v.Len()) {
                j = i + 1;
            }
            vnot.Index(i).Set(ValueOf(table.value(j))); // make it differ only by last element
        }
        @string s = fmt.Sprint(v.Interface());
        if (s != table.want) {
            t.Errorf("constructed array = %s, want %s", s, table.want);
        }

        if (table.comparable != at.Comparable()) {
            t.Errorf("constructed array (%#v) is comparable=%v, want=%v", v.Interface(), at.Comparable(), table.comparable);
        }
        if (table.comparable) {
            if (table.n > 0) {
                if (DeepEqual(vnot.Interface(), v.Interface())) {
                    t.Errorf(
                        "arrays (%#v) compare ok (but should not)",
                        v.Interface());
                }
            }
            if (!DeepEqual(vok.Interface(), v.Interface())) {
                t.Errorf(
                    "arrays (%#v) compare NOT-ok (but should)",
                    v.Interface());
            }
        }
    }
    checkSameType(t, ArrayOf(5, TypeOf((TestArrayOf_T)1)), new TestArrayOf_T[]{}.array(5));
}

internal partial class TestArrayOfGC_T /*ж<uintptr>*/;

public static void TestArrayOfGC(ж<Δtesting.T> t) {
    var xᴷ = new stackAppend<any>(moves: true);

    var tt = TypeOf((TestArrayOfGC_T)nil);
    const nint n = 100;
    slice<any> x = default;
    for (nint i = 0; i < n; i++) {
        var v = New(ArrayOf(n, tt)).Elem();
        for (nint j = 0; j < v.Len(); j++) {
            var p = @new<uintptr>();
            p.Value = (uintptr)(i * n + j);
            v.Index(j).Set(ValueOf(p.OrTypedNil()).Convert(tt));
        }
        x = append(xᴷ, x, v.Interface());
    }
    Δruntime.GC();

    x = move2heap(xᴷ, x);
    foreach (var (i, xi) in x) {
        var v = ValueOf(xi);
        for (nint j = 0; j < v.Len(); j++) {
            var k = v.Index(j).Elem().Interface();
            if (!AreEqual(k, (uintptr)(i * n + j))) {
                t.Errorf("lost x[%d][%d] = %d, want %d", i, j, k, i * n + j);
            }
        }
    }
}

public static void TestArrayOfAlg(ж<Δtesting.T> t) {
    var at = ArrayOf(6, TypeOf((byte)0));
    ref var v1 = ref heap<reflectꓸValue>(New(at).Elem(), out var Ꮡv1);
    var v2 = New(at).Elem();
    if (!AreEqual(v1.Interface(), v1.Interface())) {
        t.Errorf("constructed array %v not equal to itself", v1.Interface());
    }
    v1.Index(5).Set(ValueOf((byte)1));
    {
        var (i1, i2) = (v1.Interface(), v2.Interface()); if (AreEqual(i1, i2)) {
            t.Errorf("constructed arrays %v and %v should not be equal", i1, i2);
        }
    }

    at = ArrayOf(6, TypeOf(slice<nint>(default)));
    v1 = New(at).Elem();
    var v1ʗ1 = v1;
    shouldPanic(""u8, () => {
        _ = AreEqual(v1ʗ1.Interface(), v1ʗ1.Interface());
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string abcˢ => ref abcˢᵇ;
private static readonly @string abcˢᵇ = "abc"u8;
internal static readonly object efgˢ = (@string)"efg"u8;

public static void TestArrayOfGenericAlg(ж<Δtesting.T> t) {
    var at1 = ArrayOf(5, TypeOf((@string)""u8));
    var at = ArrayOf(6, at1);
    var v1 = New(at).Elem();
    var v2 = New(at).Elem();
    if (!AreEqual(v1.Interface(), v1.Interface())) {
        t.Errorf("constructed array %v not equal to itself", v1.Interface());
    }

    v1.Index(0).Index(0).Set(ValueOf(abcˢ));
    v2.Index(0).Index(0).Set(ValueOf(efgˢ));
    {
        var (i1, i2) = (v1.Interface(), v2.Interface()); if (AreEqual(i1, i2)) {
            t.Errorf("constructed arrays %v and %v should not be equal", i1, i2);
        }
    }

    v1.Index(0).Index(0).Set(ValueOf(abcˢ));
    v2.Index(0).Index(0).Set(ValueOf((v1.Index(0).Index(0).String() + " "u8)[..3]));
    {
        var (i1, i2) = (v1.Interface(), v2.Interface()); if (!AreEqual(i1, i2)) {
            t.Errorf("constructed arrays %v and %v should be equal", i1, i2);
        }
    }

    // Test hash
    var m = MakeMap(MapOf(at, TypeOf((nint)0)));
    m.SetMapIndex(v1, ValueOf((nint)1));
    {
        var (i1, i2) = (v1.Interface(), v2.Interface()); if (!m.MapIndex(v2).IsValid()) {
            t.Errorf("constructed arrays %v and %v have different hashes", i1, i2);
        }
    }
}

internal partial struct TestArrayOfDirectIface_T /*[1]ж<byte>*/;

internal partial struct TestArrayOfDirectIface_Tᴛ1 /*[0]ж<byte>*/;

public static void TestArrayOfDirectIface(ж<Δtesting.T> t) {
    {
        ref var i1 = ref heap<any>(Zero(TypeOf(new TestArrayOfDirectIface_T(new ж<byte>[1].array()))).Interface(), out var Ꮡi1);
        var v1 = ValueOf(Ꮡi1).Elem();
        var p1 = v1.InterfaceData()[1];

        ref var i2 = ref heap<any>(Zero(ArrayOf(1, PointerTo(TypeOf((int8)0)))).Interface(), out var Ꮡi2);
        var v2 = ValueOf(Ꮡi2).Elem();
        var p2 = v2.InterfaceData()[1];

        if (p1 != 0) {
            t.Errorf("got p1=%v. want=%v", p1, (any)(default));
        }

        if (p2 != 0) {
            t.Errorf("got p2=%v. want=%v", p2, (any)(default));
        }
    }
    {
        ref var i1 = ref heap<any>(Zero(TypeOf(new TestArrayOfDirectIface_Tᴛ1(new ж<byte>[]{}.array()))).Interface(), out var Ꮡi1);
        var v1 = ValueOf(Ꮡi1).Elem();
        var p1 = v1.InterfaceData()[1];

        ref var i2 = ref heap<any>(Zero(ArrayOf(0, PointerTo(TypeOf((int8)0)))).Interface(), out var Ꮡi2);
        var v2 = ValueOf(Ꮡi2).Elem();
        var p2 = v2.InterfaceData()[1];

        if (p1 == 0) {
            t.Errorf("got p1=%v. want=not-%v", p1, (any)(default));
        }

        if (p2 == 0) {
            t.Errorf("got p2=%v. want=not-%v", p2, (any)(default));
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string reflectNegativeLengthˢ => ref reflectNegativeLengthˢᵇ;
private static readonly @string reflectNegativeLengthˢᵇ = "reflect: negative length passed to ArrayOf"u8;

// Ensure passing in negative lengths panics.
// See https://golang.org/issue/43603
public static void TestArrayOfPanicOnNegativeLength(ж<Δtesting.T> t) {
    shouldPanic(reflectNegativeLengthˢ, () => {
        ArrayOf(-1, TypeOf((byte)0));
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string reflectTestTˢ => ref reflectTestTˢᵇ;
private static readonly @string reflectTestTˢᵇ = "[]reflect_test.T"u8;

// check construction and use of type not in binary
internal partial struct TestSliceOf_T /*num:nint*/;

// check that type already in binary is found
internal partial struct TestSliceOf_T1 /*num:nint*/;

public static void TestSliceOf(ж<Δtesting.T> t) {
    var st = SliceOf(TypeOf((TestSliceOf_T)1));
    {
        @string got = st.String();
        @string wantΔ1 = reflectTestTˢ; if (got != wantΔ1) {
            t.Errorf("SliceOf(T(1)).String()=%q, want %q", got, wantΔ1);
        }
    }
    var v = MakeSlice(st, 10, 10);
    Δruntime.GC();
    for (nint i = 0; i < v.Len(); i++) {
        v.Index(i).Set(ValueOf((TestSliceOf_T)i));
        Δruntime.GC();
    }
    @string s = fmt.Sprint(v.Interface());
    @string want = "[0 1 2 3 4 5 6 7 8 9]";
    if (s != want) {
        t.Errorf("constructed slice = %s, want %s", s, want);
    }
    checkSameType(t, SliceOf(TypeOf((TestSliceOf_T1)1)), slice<TestSliceOf_T1>([]));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object sliceSizeDoesNotOverflowˢ = (@string)"slice size does not overflow"u8;
internal static readonly object sliceOverflowDoesNotˢ = (@string)"slice overflow does not panic"u8;

public static void TestSliceOverflow(ж<Δtesting.T> t) {
    GoFrame ᒐ = default;
    try {
        // check that MakeSlice panics when size of slice overflows uint
        UntypedFloat S = 1e6;
        nuint s = (nuint)S;
        nuint l = (unchecked((nuint)(18446744073709551615UL))) / s + 1;
        if (l * s >= s) {
            t.Fatal(sliceSizeDoesNotOverflowˢ);
        }
        array<byte> x = new(1000000); /* S */
        var st = SliceOf(TypeOf(x));
        defer(() => {
            var err = recover();
            if (err == nil) {
                t.Fatal(sliceOverflowDoesNotˢ);
            }
        }, ref ᒐ);
        MakeSlice(st, (nint)l, (nint)l);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial class TestSliceOfGC_T /*ж<uintptr>*/;

public static void TestSliceOfGC(ж<Δtesting.T> t) {
    var xᴷ = new stackAppend<any>(moves: true);

    var tt = TypeOf((TestSliceOfGC_T)nil);
    var st = SliceOf(tt);
    const nint n = 100;
    slice<any> x = default;
    for (nint i = 0; i < n; i++) {
        var v = MakeSlice(st, n, n);
        for (nint j = 0; j < v.Len(); j++) {
            var p = @new<uintptr>();
            p.Value = (uintptr)(i * n + j);
            v.Index(j).Set(ValueOf(p.OrTypedNil()).Convert(tt));
        }
        x = append(xᴷ, x, v.Interface());
    }
    Δruntime.GC();

    x = move2heap(xᴷ, x);
    foreach (var (i, xi) in x) {
        var v = ValueOf(xi);
        for (nint j = 0; j < v.Len(); j++) {
            var k = v.Index(j).Elem().Interface();
            if (!AreEqual(k, (uintptr)(i * n + j))) {
                t.Errorf("lost x[%d][%d] = %d, want %d", i, j, k, i * n + j);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string hasInvalidNameˢ => ref hasInvalidNameˢᵇ;
private static readonly @string hasInvalidNameˢᵇ = "has invalid name"u8;
internal static ref readonly @string hasNoNameˢ => ref hasNoNameˢᵇ;
private static readonly @string hasNoNameˢᵇ = "has no name"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string structStrᶜ => ref structStrᶜᵇ;
private static readonly @string structStrᶜᵇ = @"struct { φ string; ValidName string; Val1dNam5 string }"u8;

public static void TestStructOfFieldName(ж<Δtesting.T> t) {
    // invalid field name "1nvalid"
    shouldPanic(hasInvalidNameˢ, () => {
        StructOf(slice<Δreflect.StructField>([
            new(Name: "Valid", Type: TypeOf((@string)""u8)),
            new(Name: "1nvalid", Type: TypeOf((@string)""u8))
        ]));
    });

    // invalid field name "+"
    shouldPanic(hasInvalidNameˢ, () => {
        StructOf(slice<Δreflect.StructField>([
            new(Name: "Val1d", Type: TypeOf((@string)""u8)),
            new(Name: "+", Type: TypeOf((@string)""u8))
        ]));
    });

    // no field name
    shouldPanic(hasNoNameˢ, () => {
        StructOf(slice<Δreflect.StructField>([
            new(Name: "", Type: TypeOf((@string)""u8))
        ]));
    });

    // verify creation of a struct with valid struct fields
    var validFields = slice<Δreflect.StructField>([
        new(
            Name: "φ",
            Type: TypeOf((@string)""u8)
        ),
        new(
            Name: "ValidName",
            Type: TypeOf((@string)""u8)
        ),
        new(
            Name: "Val1dNam5",
            Type: TypeOf((@string)""u8)
        )
    ]);

    var validStruct = StructOf(validFields);

    @string structStr = structStrᶜ;
    {
        @string got = validStruct.String();
        @string want = structStr; if (got != want) {
            t.Errorf("StructOf(validFields).String()=%q, want %q", got, want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string duplicateFieldˢ => ref duplicateFieldˢᵇ;
private static readonly @string duplicateFieldˢᵇ = "duplicate field"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string stStrᶜ => ref stStrᶜᵇ;
private static readonly @string stStrᶜᵇ = @"struct { S string ""s""; X uint8 ""x""; Y uint64; Z [3]uint16 }"u8;

internal partial struct TestStructOf_i /*dyn*/ {
    public @string String;
    public byte X;
    public uint64 Y;
    public array<uint16> Z = new(3);
}

internal partial struct TestStructOf_iᴛ1 /*dyn*/ {
    public byte G1;
    public array<ж<byte>> G2 = new(0);
}

internal partial struct TestStructOf_y /*dyn*/ {
    public uint64 Y;
}

internal partial struct TestStructOf_yᴛ1 /*dyn*/ {
    public TestStructOf_structFieldType F;
}

public static void TestStructOf(ж<Δtesting.T> t) {
    // check construction and use of type not in binary
    var fields = slice<Δreflect.StructField>([
        new(
            Name: "S",
            Tag: (@string)"s",
            Type: TypeOf((@string)""u8)
        ),
        new(
            Name: "X",
            Tag: (@string)"x",
            Type: TypeOf((byte)0)
        ),
        new(
            Name: "Y",
            Type: TypeOf((uint64)0)
        ),
        new(
            Name: "Z",
            Type: TypeOf(new uint16[]{}.array(3))
        )
    ]);

    var st = StructOf(fields);
    var v = New(st).Elem();
    Δruntime.GC();
    v.FieldByName("X").Set(ValueOf((byte)2));
    v.FieldByIndex(slice<nint>([1])).Set(ValueOf((byte)1));
    Δruntime.GC();

    @string s = fmt.Sprint(v.Interface());
    @string want = @"{ 1 0 [0 0 0]}";
    if (s != want) {
        t.Errorf("constructed struct = %s, want %s", s, want);
    }
    @string stStr = stStrᶜ;
    {
        @string got = st.String();
        @string wantΔ1 = stStr; if (got != wantΔ1) {
            t.Errorf("StructOf(fields).String()=%q, want %q", got, wantΔ1);
        }
    }

    // check the size, alignment and field offsets
    var stt = TypeOf(new TestStructOf_i());
    if (st.Size() != stt.Size()) {
        t.Errorf("constructed struct size = %v, want %v", st.Size(), stt.Size());
    }
    if (st.Align() != stt.Align()) {
        t.Errorf("constructed struct align = %v, want %v", st.Align(), stt.Align());
    }
    if (st.FieldAlign() != stt.FieldAlign()) {
        t.Errorf("constructed struct field align = %v, want %v", st.FieldAlign(), stt.FieldAlign());
    }
    for (nint i = 0; i < st.NumField(); i++) {
        var o1 = st.Field(i).Offset;
        var o2 = stt.Field(i).Offset;
        if (o1 != o2) {
            t.Errorf("constructed struct field %v offset = %v, want %v", i, o1, o2);
        }
    }

    // Check size and alignment with a trailing zero-sized field.
    st = StructOf(slice<Δreflect.StructField>([
        new(
            Name: "F1",
            Type: TypeOf((byte)0)
        ),
        new(
            Name: "F2",
            Type: TypeOf(new ж<byte>[]{}.array())
        )
    ]));
    stt = TypeOf(new TestStructOf_iᴛ1());
    if (st.Size() != stt.Size()) {
        t.Errorf("constructed zero-padded struct size = %v, want %v", st.Size(), stt.Size());
    }
    if (st.Align() != stt.Align()) {
        t.Errorf("constructed zero-padded struct align = %v, want %v", st.Align(), stt.Align());
    }
    if (st.FieldAlign() != stt.FieldAlign()) {
        t.Errorf("constructed zero-padded struct field align = %v, want %v", st.FieldAlign(), stt.FieldAlign());
    }
    for (nint i = 0; i < st.NumField(); i++) {
        var o1 = st.Field(i).Offset;
        var o2 = stt.Field(i).Offset;
        if (o1 != o2) {
            t.Errorf("constructed zero-padded struct field %v offset = %v, want %v", i, o1, o2);
        }
    }

    // check duplicate names
    shouldPanic(duplicateFieldˢ, () => {
        StructOf(slice<Δreflect.StructField>([
            new(Name: "string", PkgPath: "p", Type: TypeOf((@string)""u8)),
            new(Name: "string", PkgPath: "p", Type: TypeOf((@string)""u8))
        ]));
    });
    shouldPanic(hasNoNameˢ, () => {
        StructOf(slice<Δreflect.StructField>([
            new(Type: TypeOf((@string)""u8)),
            new(Name: "string", PkgPath: "p", Type: TypeOf((@string)""u8))
        ]));
    });
    shouldPanic(hasNoNameˢ, () => {
        StructOf(slice<Δreflect.StructField>([
            new(Type: TypeOf((@string)""u8)),
            new(Type: TypeOf((@string)""u8))
        ]));
    });
    // check that type already in binary is found
    checkSameType(t, StructOf(fields[2..3]), new TestStructOf_y());
    checkSameType(t,
        StructOf(slice<Δreflect.StructField>([
            new(
                Name: "F",
                Type: TypeOf((ж<TestStructOf_structFieldType>)nil).Elem()
            )
        ])),
        new TestStructOf_yᴛ1());
}

internal partial struct TestStructOfExportRules_S1 /*dyn*/ {
}

internal partial struct TestStructOfExportRules_s2 /*dyn*/ {
}

internal partial struct TestStructOfExportRules_ΦType /*dyn*/ {
}

internal partial struct TestStructOfExportRules_φType /*dyn*/ {
}

internal partial struct TestStructOfExportRules_tests /*dyn*/ {
    internal Δreflect.StructField field;
    internal bool mustPanic;
    internal bool exported;
}

public static void TestStructOfExportRules(ж<Δtesting.T> t) {
    void testPanic(nint i, bool mustPanic, Action f) {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                var err = recover();
                if (err == nil && mustPanic) {
                    t.Errorf("test-%d did not panic", i);
                }
                if (err != nil && !mustPanic) {
                    t.Errorf("test-%d panicked: %v\n", i, err);
                }
            }, ref ᒐ);
            f();
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }

    var tests = slice<TestStructOfExportRules_tests>([
        new(
            field: new StructField(Name: "S1", Anonymous: true, Type: TypeOf(new TestStructOfExportRules_S1())),
            exported: true
        ),
        new(
            field: new StructField(Name: "S1", Anonymous: true, Type: TypeOf((ж<TestStructOfExportRules_S1>)nil)),
            exported: true
        ),
        new(
            field: new StructField(Name: "s2", Anonymous: true, Type: TypeOf(new TestStructOfExportRules_s2())),
            mustPanic: true
        ),
        new(
            field: new StructField(Name: "s2", Anonymous: true, Type: TypeOf((ж<TestStructOfExportRules_s2>)nil)),
            mustPanic: true
        ),
        new(
            field: new StructField(Name: "Name", Type: default, PkgPath: ""),
            mustPanic: true
        ),
        new(
            field: new StructField(Name: "", Type: TypeOf(new TestStructOfExportRules_S1()), PkgPath: ""),
            mustPanic: true
        ),
        new(
            field: new StructField(Name: "S1", Anonymous: true, Type: TypeOf(new TestStructOfExportRules_S1()), PkgPath: "other/pkg"),
            mustPanic: true
        ),
        new(
            field: new StructField(Name: "S1", Anonymous: true, Type: TypeOf((ж<TestStructOfExportRules_S1>)nil), PkgPath: "other/pkg"),
            mustPanic: true
        ),
        new(
            field: new StructField(Name: "s2", Anonymous: true, Type: TypeOf(new TestStructOfExportRules_s2()), PkgPath: "other/pkg"),
            mustPanic: true
        ),
        new(
            field: new StructField(Name: "s2", Anonymous: true, Type: TypeOf((ж<TestStructOfExportRules_s2>)nil), PkgPath: "other/pkg"),
            mustPanic: true
        ),
        new(
            field: new StructField(Name: "s2", Type: TypeOf((nint)0), PkgPath: "other/pkg")
        ),
        new(
            field: new StructField(Name: "s2", Type: TypeOf((nint)0), PkgPath: "other/pkg")
        ),
        new(
            field: new StructField(Name: "S", Type: TypeOf(new TestStructOfExportRules_S1())),
            exported: true
        ),
        new(
            field: new StructField(Name: "S", Type: TypeOf((ж<TestStructOfExportRules_S1>)nil)),
            exported: true
        ),
        new(
            field: new StructField(Name: "S", Type: TypeOf(new TestStructOfExportRules_s2())),
            exported: true
        ),
        new(
            field: new StructField(Name: "S", Type: TypeOf((ж<TestStructOfExportRules_s2>)nil)),
            exported: true
        ),
        new(
            field: new StructField(Name: "s", Type: TypeOf(new TestStructOfExportRules_S1())),
            mustPanic: true
        ),
        new(
            field: new StructField(Name: "s", Type: TypeOf((ж<TestStructOfExportRules_S1>)nil)),
            mustPanic: true
        ),
        new(
            field: new StructField(Name: "s", Type: TypeOf(new TestStructOfExportRules_s2())),
            mustPanic: true
        ),
        new(
            field: new StructField(Name: "s", Type: TypeOf((ж<TestStructOfExportRules_s2>)nil)),
            mustPanic: true
        ),
        new(
            field: new StructField(Name: "s", Type: TypeOf(new TestStructOfExportRules_S1()), PkgPath: "other/pkg")
        ),
        new(
            field: new StructField(Name: "s", Type: TypeOf((ж<TestStructOfExportRules_S1>)nil), PkgPath: "other/pkg")
        ),
        new(
            field: new StructField(Name: "s", Type: TypeOf(new TestStructOfExportRules_s2()), PkgPath: "other/pkg")
        ),
        new(
            field: new StructField(Name: "s", Type: TypeOf((ж<TestStructOfExportRules_s2>)nil), PkgPath: "other/pkg")
        ),
        new(
            field: new StructField(Name: "", Type: TypeOf(new TestStructOfExportRules_ΦType())),
            mustPanic: true
        ),
        new(
            field: new StructField(Name: "", Type: TypeOf(new TestStructOfExportRules_φType())),
            mustPanic: true
        ),
        new(
            field: new StructField(Name: "Φ", Type: TypeOf((nint)0)),
            exported: true
        ),
        new(
            field: new StructField(Name: "φ", Type: TypeOf((nint)0)),
            exported: false
        )
    ]);

    foreach (var (i, vᴛ1) in tests) {
        ref var test = ref heap(new TestStructOfExportRules_tests(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        testPanic(i, test.mustPanic, () => {
            var typ = StructOf(slice<Δreflect.StructField>([testʗ1.field]));
            if (typ == nil) {
                t.Errorf("test-%d: error creating struct type", i);
                return;
            }
            var field = typ.Field(0);
            @string n = field.Name;
            if (n == ""u8) {
                throw panic("field.Name must not be empty");
            }
            var exported = token.IsExported(n);
            if (exported != testʗ1.exported) {
                t.Errorf("test-%d: got exported=%v want exported=%v", i, exported, testʗ1.exported);
            }
            if (field.PkgPath != testʗ1.field.PkgPath) {
                t.Errorf("test-%d: got PkgPath=%q want pkgPath=%q", i, field.PkgPath, testʗ1.field.PkgPath);
            }
        });
    }
}

internal partial class TestStructOfGC_T /*ж<uintptr>*/;

public static void TestStructOfGC(ж<Δtesting.T> t) {
    var xᴷ = new stackAppend<any>(moves: true);

    var tt = TypeOf((TestStructOfGC_T)nil);
    var fields = slice<Δreflect.StructField>([
        new(Name: "X", Type: tt),
        new(Name: "Y", Type: tt)
    ]);
    var st = StructOf(fields);

    const nint n = 10000;
    slice<any> x = default;
    for (nint i = 0; i < n; i++) {
        var v = New(st).Elem();
        for (nint j = 0; j < v.NumField(); j++) {
            var p = @new<uintptr>();
            p.Value = (uintptr)(i * n + j);
            v.Field(j).Set(ValueOf(p.OrTypedNil()).Convert(tt));
        }
        x = append(xᴷ, x, v.Interface());
    }
    Δruntime.GC();

    x = move2heap(xᴷ, x);
    foreach (var (i, xi) in x) {
        var v = ValueOf(xi);
        for (nint j = 0; j < v.NumField(); j++) {
            var k = v.Field(j).Elem().Interface();
            if (!AreEqual(k, (uintptr)(i * n + j))) {
                t.Errorf("lost x[%d].%c = %d, want %d", i, LiteralByteAt("XY"u8, j), k, i * n + j);
            }
        }
    }
}

public static void TestStructOfAlg(ж<Δtesting.T> t) {
    var st = StructOf(slice<Δreflect.StructField>([new(Name: "X", Tag: (@string)"x", Type: TypeOf((nint)0))]));
    ref var v1 = ref heap<reflectꓸValue>(New(st).Elem(), out var Ꮡv1);
    var v2 = New(st).Elem();
    if (!DeepEqual(v1.Interface(), v1.Interface())) {
        t.Errorf("constructed struct %v not equal to itself", v1.Interface());
    }
    v1.FieldByName("X").Set(ValueOf((nint)1));
    {
        var (i1, i2) = (v1.Interface(), v2.Interface()); if (DeepEqual(i1, i2)) {
            t.Errorf("constructed structs %v and %v should not be equal", i1, i2);
        }
    }

    st = StructOf(slice<Δreflect.StructField>([new(Name: "X", Tag: (@string)"x", Type: TypeOf(slice<nint>(default)))]));
    v1 = New(st).Elem();
    var v1ʗ1 = v1;
    shouldPanic(""u8, () => {
        _ = AreEqual(v1ʗ1.Interface(), v1ʗ1.Interface());
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object defˢ = (@string)"def"u8;

internal partial struct TestStructOfGenericAlg_tests /*dyn*/ {
    internal reflectꓸType rt;
    internal slice<nint> idx;
}

public static void TestStructOfGenericAlg(ж<Δtesting.T> t) {
    var st1 = StructOf(slice<Δreflect.StructField>([
        new(Name: "X", Tag: (@string)"x", Type: TypeOf((int64)0)),
        new(Name: "Y", Type: TypeOf((@string)""u8))
    ]));
    var st = StructOf(slice<Δreflect.StructField>([
        new(Name: "S0", Type: st1),
        new(Name: "S1", Type: st1)
    ]));

    var tests = slice<TestStructOfGenericAlg_tests>([
        new(
            rt: st,
            idx: slice<nint>([0, 1])
        ),
        new(
            rt: st1,
            idx: slice<nint>([1])
        ),
        new(
            rt: StructOf(
                slice<Δreflect.StructField>([
                    new(Name: "XX", Type: TypeOf(new nint[]{}.array())),
                    new(Name: "YY", Type: TypeOf((@string)""u8))
                ])),
            idx: slice<nint>([1])
        ),
        new(
            rt: StructOf(
                slice<Δreflect.StructField>([
                    new(Name: "XX", Type: TypeOf(new nint[]{}.array())),
                    new(Name: "YY", Type: TypeOf((@string)""u8)),
                    new(Name: "ZZ", Type: TypeOf(new nint[]{}.array(2)))
                ])),
            idx: slice<nint>([1])
        ),
        new(
            rt: StructOf(
                slice<Δreflect.StructField>([
                    new(Name: "XX", Type: TypeOf(new nint[]{}.array(1))),
                    new(Name: "YY", Type: TypeOf((@string)""u8))
                ])),
            idx: slice<nint>([1])
        ),
        new(
            rt: StructOf(
                slice<Δreflect.StructField>([
                    new(Name: "XX", Type: TypeOf(new nint[]{}.array(1))),
                    new(Name: "YY", Type: TypeOf((@string)""u8)),
                    new(Name: "ZZ", Type: TypeOf(new nint[]{}.array(1)))
                ])),
            idx: slice<nint>([1])
        ),
        new(
            rt: StructOf(
                slice<Δreflect.StructField>([
                    new(Name: "XX", Type: TypeOf(new nint[]{}.array(2))),
                    new(Name: "YY", Type: TypeOf((@string)""u8)),
                    new(Name: "ZZ", Type: TypeOf(new nint[]{}.array(2)))
                ])),
            idx: slice<nint>([1])
        ),
        new(
            rt: StructOf(
                slice<Δreflect.StructField>([
                    new(Name: "XX", Type: TypeOf((int64)0)),
                    new(Name: "YY", Type: TypeOf((byte)0)),
                    new(Name: "ZZ", Type: TypeOf((@string)""u8))
                ])),
            idx: slice<nint>([2])
        ),
        new(
            rt: StructOf(
                slice<Δreflect.StructField>([
                    new(Name: "XX", Type: TypeOf((int64)0)),
                    new(Name: "YY", Type: TypeOf((int64)0)),
                    new(Name: "ZZ", Type: TypeOf((@string)""u8)),
                    new(Name: "AA", Type: TypeOf(new int64[]{}.array(1)))
                ])),
            idx: slice<nint>([2])
        )
    ]);

    foreach (var (_, table) in tests) {
        var v1 = New(table.rt).Elem();
        var v2 = New(table.rt).Elem();

        if (!DeepEqual(v1.Interface(), v1.Interface())) {
            t.Errorf("constructed struct %v not equal to itself", v1.Interface());
        }

        v1.FieldByIndex(table.idx).Set(ValueOf(abcˢ));
        v2.FieldByIndex(table.idx).Set(ValueOf(defˢ));
        {
            var (i1, i2) = (v1.Interface(), v2.Interface()); if (DeepEqual(i1, i2)) {
                t.Errorf("constructed structs %v and %v should not be equal", i1, i2);
            }
        }

        @string abc = abcˢ;
        v1.FieldByIndex(table.idx).Set(ValueOf(abc));
        @string val = "+"u8 + abc + "-"u8;
        v2.FieldByIndex(table.idx).Set(ValueOf(val[1..4]));
        {
            var (i1, i2) = (v1.Interface(), v2.Interface()); if (!DeepEqual(i1, i2)) {
                t.Errorf("constructed structs %v and %v should be equal", i1, i2);
            }
        }

        // Test hash
        var m = MakeMap(MapOf(table.rt, TypeOf((nint)0)));
        m.SetMapIndex(v1, ValueOf((nint)1));
        {
            var (i1, i2) = (v1.Interface(), v2.Interface()); if (!m.MapIndex(v2).IsValid()) {
                t.Errorf("constructed structs %#v and %#v have different hashes", i1, i2);
            }
        }

        v2.FieldByIndex(table.idx).Set(ValueOf(abcˢ));
        {
            var (i1, i2) = (v1.Interface(), v2.Interface()); if (!DeepEqual(i1, i2)) {
                t.Errorf("constructed structs %v and %v should be equal", i1, i2);
            }
        }

        {
            var (i1, i2) = (v1.Interface(), v2.Interface()); if (!m.MapIndex(v2).IsValid()) {
                t.Errorf("constructed structs %v and %v have different hashes", i1, i2);
            }
        }
    }
}

internal partial struct TestStructOfDirectIface_T /*dyn*/ {
    public array<ж<byte>> X = new(1);
}

internal partial struct TestStructOfDirectIface_Tᴛ1 /*dyn*/ {
    public array<ж<byte>> X = new(0);
}

public static void TestStructOfDirectIface(ж<Δtesting.T> t) {
    {
        ref var i1 = ref heap<any>(Zero(TypeOf(new TestStructOfDirectIface_T())).Interface(), out var Ꮡi1);
        var v1 = ValueOf(Ꮡi1).Elem();
        var p1 = v1.InterfaceData()[1];

        ref var i2 = ref heap<any>(Zero(StructOf(slice<Δreflect.StructField>([
            new(
                Name: "X",
                Type: ArrayOf(1, TypeOf((ж<int8>)nil))
            )
        ]))).Interface(), out var Ꮡi2);
        var v2 = ValueOf(Ꮡi2).Elem();
        var p2 = v2.InterfaceData()[1];

        if (p1 != 0) {
            t.Errorf("got p1=%v. want=%v", p1, (any)(default));
        }

        if (p2 != 0) {
            t.Errorf("got p2=%v. want=%v", p2, (any)(default));
        }
    }
    {
        ref var i1 = ref heap<any>(Zero(TypeOf(new TestStructOfDirectIface_Tᴛ1())).Interface(), out var Ꮡi1);
        var v1 = ValueOf(Ꮡi1).Elem();
        var p1 = v1.InterfaceData()[1];

        ref var i2 = ref heap<any>(Zero(StructOf(slice<Δreflect.StructField>([
            new(
                Name: "X",
                Type: ArrayOf(0, TypeOf((ж<int8>)nil))
            )
        ]))).Interface(), out var Ꮡi2);
        var v2 = ValueOf(Ꮡi2).Elem();
        var p2 = v2.InterfaceData()[1];

        if (p1 == 0) {
            t.Errorf("got p1=%v. want=not-%v", p1, (any)(default));
        }

        if (p2 == 0) {
            t.Errorf("got p2=%v. want=not-%v", p2, (any)(default));
        }
    }
}

partial struct StructI /*num:nint*/;

public static nint Get(this StructI i) {
    return (nint)i;
}

partial struct StructIPtr /*num:nint*/;

public static nint Get(this ref StructIPtr i) {
    return (nint)(i);
}

public static void Set(this ж<StructIPtr> Ꮡi, nint v) {
    (Ꮡi.Reinterpret<StructIPtr, nint>()).Value = v;
}

partial struct SettableStruct {
    public nint SettableField;
}

public static void Set(this ref SettableStruct p, nint v) {
    p.SettableField = v;
}

partial struct SettablePointer {
    public ж<nint> SettableField;
}

public static void Set(this ref SettablePointer p, nint v) {
    p.SettableField.Value = v;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string getˢ => ref getˢᵇ;
private static readonly @string getˢᵇ = "Get"u8;

internal partial interface TestStructOfWithInterface_Iface /*dyn*/ {
    nint Get();
}

internal partial interface TestStructOfWithInterface_IfaceSet /*dyn*/ {
    void Set(nint _Δp0);
}

internal partial struct TestStructOfWithInterface_tests /*dyn*/ {
    internal @string name;
    internal reflectꓸType typ;
    internal reflectꓸValue val;
    internal bool impl;
}

public static void TestStructOfWithInterface(ж<Δtesting.T> t) {
    UntypedInt want = 42;
    var tests = slice<TestStructOfWithInterface_tests>([
        new(
            name: "StructI",
            typ: TypeOf((StructI)want),
            val: ValueOf((StructI)want),
            impl: true
        ),
        new(
            name: "StructI",
            typ: PointerTo(TypeOf((StructI)want)),
            val: ValueOf(func(any () => {
                ref var v = ref heap<StructI>((StructI)want, out var Ꮡv);
                return Ꮡv;
            })()),
            impl: true
        ),
        new(
            name: "StructIPtr",
            typ: PointerTo(TypeOf((StructIPtr)want)),
            val: ValueOf(func(any () => {
                ref var v = ref heap<StructIPtr>((StructIPtr)want, out var Ꮡv);
                return Ꮡv;
            })()),
            impl: true
        ),
        new(
            name: "StructIPtr",
            typ: TypeOf((StructIPtr)want),
            val: ValueOf((StructIPtr)want),
            impl: false
        )
        // {
        //	typ:  TypeOf((*Iface)(nil)).Elem(), // FIXME(sbinet): fix method.ifn/tfn
        //	val:  ValueOf(StructI(want)),
        //	impl: true,
        // },
    ]);

    foreach (var (i, table) in tests) {
        for (nint jᴛ1 = 0; jᴛ1 < 2; jᴛ1++) {
            var j = jᴛ1;
            ref var fieldsΔ1 = ref heap<slice<Δreflect.StructField>>(out var ᏑfieldsΔ1);
            if (j == 1) {
                fieldsΔ1 = append(fieldsΔ1, new StructField(
                    Name: "Dummy",
                    PkgPath: "",
                    Type: TypeOf((nint)0)
                ));
            }
            fieldsΔ1 = append(fieldsΔ1, new StructField(
                Name: table.name,
                Anonymous: true,
                PkgPath: "",
                Type: table.typ
            ));

            // We currently do not correctly implement methods
            // for embedded fields other than the first.
            // Therefore, for now, we expect those methods
            // to not exist.  See issues 15924 and 20824.
            // When those issues are fixed, this test of panic
            // should be removed.
            if (j == 1 && table.impl) {
                func(() => {
                    GoFrame ᒐ = default;
                    try {
                        defer(() => {
                            {
                                var err = recover(); if (err == nil) {
                                    t.Errorf("test-%d-%d did not panic", i, j);
                                }
                            }
                        }, ref ᒐ);
                        _ = StructOf(ᏑfieldsΔ1.ValueSlot);
                    }
                    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                    finally { ᒐ.Run(); }
                })();
                continue;
            }

            var rtΔ1 = StructOf(fieldsΔ1);
            var rvΔ1 = New(rtΔ1).Elem();
            rvΔ1.Field(j).Set(table.val);

            {
                var (_, ok) = rvΔ1.Interface()._<TestStructOfWithInterface_Iface>(ᐧ); if (ok != table.impl) {
                    if (table.impl) {
                        t.Errorf("test-%d-%d: type=%v fails to implement Iface.\n", i, j, table.typ);
                    } else {
                        t.Errorf("test-%d-%d: type=%v should NOT implement Iface\n", i, j, table.typ);
                    }
                    continue;
                }
            }

            if (!table.impl) {
                continue;
            }

            nint v = rvΔ1.Interface()._<TestStructOfWithInterface_Iface>().Get();
            if (v != want) {
                t.Errorf("test-%d-%d: x.Get()=%v. want=%v\n", i, j, v, (nint)(want));
            }

            var fct = rvΔ1.MethodByName(getˢ);
            var @out = fct.Call(default);
            if (!DeepEqual(@out[0].Interface(), (nint)(want))) {
                t.Errorf("test-%d-%d: x.Get()=%v. want=%v\n", i, j, @out[0].Interface(), (nint)(want));
            }
        }
    }

    // Test an embedded nil pointer with pointer methods.
    ref var fields = ref heap<slice<Δreflect.StructField>>(slice<Δreflect.StructField>([new(
        Name: "StructIPtr",
        Anonymous: true,
        Type: PointerTo(TypeOf((StructIPtr)want))
    )
    ]), out var Ꮡfields);
    var rt = StructOf(fields);
    ref var rv = ref heap<reflectꓸValue>(New(rt).Elem(), out var Ꮡrv);
    // This should panic since the pointer is nil.
    shouldPanic(""u8, () => {
        Ꮡrv.Value.Interface()._<TestStructOfWithInterface_IfaceSet>().Set(want);
    });

    // Test an embedded nil pointer to a struct with pointer methods.

    fields = slice<Δreflect.StructField>([new(
        Name: "SettableStruct",
        Anonymous: true,
        Type: PointerTo(TypeOf(new SettableStruct()))
    )
    ]);
    rt = StructOf(fields);
    rv = New(rt).Elem();
    // This should panic since the pointer is nil.
    shouldPanic(""u8, () => {
        Ꮡrv.Value.Interface()._<TestStructOfWithInterface_IfaceSet>().Set(want);
    });

    // The behavior is different if there is a second field,
    // since now an interface value holds a pointer to the struct
    // rather than just holding a copy of the struct.
    fields = slice<Δreflect.StructField>([
        new(
            Name: "SettableStruct",
            Anonymous: true,
            Type: PointerTo(TypeOf(new SettableStruct()))
        ),
        new(
            Name: "EmptyStruct",
            Anonymous: true,
            Type: StructOf(default)
        )
    ]);
    // With the current implementation this is expected to panic.
    // Ideally it should work and we should be able to see a panic
    // if we call the Set method.
    shouldPanic(""u8, () => {
        StructOf(Ꮡfields.ValueSlot);
    });

    // Embed a field that can be stored directly in an interface,
    // with a second field.
    fields = slice<Δreflect.StructField>([
        new(
            Name: "SettablePointer",
            Anonymous: true,
            Type: TypeOf(new SettablePointer())
        ),
        new(
            Name: "EmptyStruct",
            Anonymous: true,
            Type: StructOf(default)
        )
    ]);
    // With the current implementation this is expected to panic.
    // Ideally it should work and we should be able to call the
    // Set and Get methods.
    shouldPanic(""u8, () => {
        StructOf(Ꮡfields.ValueSlot);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string afterˢ => ref afterˢᵇ;
private static readonly @string afterˢᵇ = "After"u8;

public static void TestStructOfTooManyFields(ж<Δtesting.T> t) {
    // Bug Fix: #25402 - this should not panic
    var tt = StructOf(slice<Δreflect.StructField>([
        new(Name: "Time", Type: TypeOf(new time.Time()), Anonymous: true)
    ]));

    {
        var (_, present) = tt.MethodByName(afterˢ); if (!present) {
            t.Errorf("Expected method `After` to be found");
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string differentPkgPathˢ => ref differentPkgPathˢᵇ;
private static readonly @string differentPkgPathˢᵇ = "different PkgPath"u8;

public static void TestStructOfDifferentPkgPath(ж<Δtesting.T> t) {
    var fields = slice<Δreflect.StructField>([
        new(
            Name: "f1",
            PkgPath: "p1",
            Type: TypeOf((nint)0)
        ),
        new(
            Name: "f2",
            PkgPath: "p2",
            Type: TypeOf((nint)0)
        )
    ]);
    var fieldsʗ1 = fields;
    shouldPanic(differentPkgPathˢ, () => {
        StructOf(fieldsʗ1);
    });
}

internal partial struct TestStructOfTooLarge_test /*dyn*/ {
    internal bool shouldPanic;
    internal slice<Δreflect.StructField> fields;
}

public static void TestStructOfTooLarge(ж<Δtesting.T> t) {
    var t1 = TypeOf((byte)0);
    var t2 = TypeOf((int16)0);
    var t4 = TypeOf((int32)0);
    var t0 = ArrayOf(0, t1);

    // 2^64-3 sized type (or 2^32-3 on 32-bit archs)
    var bigType = StructOf(slice<Δreflect.StructField>([
        new(Name: "F1", Type: ArrayOf((nint)(~(uintptr)0 >> 1), t1)),
        new(Name: "F2", Type: ArrayOf((nint)((~(uintptr)0 >> 1) - 1), t1))
    ]));

    var tests = new TestStructOfTooLarge_test[]{
        new(
            shouldPanic: false, // 2^64-1, ok
            fields: slice<Δreflect.StructField>([
                new(Name: "F1", Type: bigType),
                new(Name: "F2", Type: ArrayOf(2, t1))
            ])
        ),
        new(
            shouldPanic: true, // overflow in total size
            fields: slice<Δreflect.StructField>([
                new(Name: "F1", Type: bigType),
                new(Name: "F2", Type: ArrayOf(3, t1))
            ])
        ),
        new(
            shouldPanic: true, // overflow while aligning F2
            fields: slice<Δreflect.StructField>([
                new(Name: "F1", Type: bigType),
                new(Name: "F2", Type: t4)
            ])
        ),
        new(
            shouldPanic: true, // overflow while adding trailing byte for zero-sized fields
            fields: slice<Δreflect.StructField>([
                new(Name: "F1", Type: bigType),
                new(Name: "F2", Type: ArrayOf(2, t1)),
                new(Name: "F3", Type: t0)
            ])
        ),
        new(
            shouldPanic: true, // overflow while aligning total size
            fields: slice<Δreflect.StructField>([
                new(Name: "F1", Type: t2),
                new(Name: "F2", Type: bigType)
            ])
        )
    }.array();

    foreach (var (i, vᴛ1) in tests.ΔRangeSnapshot()) {
        ref var tt = ref heap(new TestStructOfTooLarge_test(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        func(() => {
            GoFrame ᒐ = default;
            try {
                var ttʗ2 = ttʗ1;
                defer(() => {
                    var err = recover();
                    if (!ttʗ2.shouldPanic) {
                        if (err != nil) {
                            t.Errorf("test %d should not panic, got %s", i, err);
                        }
                        return;
                    }
                    if (err == nil) {
                        t.Errorf("test %d expected to panic", i);
                        return;
                    }
                    @string s = fmt.Sprintf("%s"u8, err);
                    if (s != "reflect.StructOf: struct size would exceed virtual address space"u8) {
                        t.Errorf("test %d wrong panic message: %s", i, s);
                        return;
                    }
                }, ref ᒐ);
                _ = StructOf(ttʗ1.fields);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        })();
    }
}

internal partial struct TestStructOfAnonymous_type /*dyn*/ {
    /*embed*/ public D1 D1;
}

public static void TestStructOfAnonymous(ж<Δtesting.T> t) {
    any s = new TestStructOfAnonymous_type();
    var f = TypeOf(s).Field(0);
    var ds = StructOf(slice<Δreflect.StructField>([f]));
    var st = TypeOf(s);
    var dt = New(ds).Elem();
    if (!AreEqual(st, dt.Type())) {
        t.Errorf("StructOf returned %s, want %s", dt.Type(), st);
    }

    // This should not panic.
    _ = dt.Interface()._<TestStructOfAnonymous_type>();
}

// check construction and use of type not in binary
internal partial struct TestChanOf_T /*@string*/;

// check that type already in binary is found
internal partial struct TestChanOf_T1 /*num:nint*/;

public static void TestChanOf(ж<Δtesting.T> t) {
    var ct = ChanOf(BothDir, TypeOf((TestChanOf_T)(@string)""u8));
    var v = MakeChan(ct, 2);
    Δruntime.GC();
    v.Send(ValueOf((TestChanOf_T)(@string)helloˢ));
    Δruntime.GC();
    v.Send(ValueOf((TestChanOf_T)(@string)worldˢ));
    Δruntime.GC();

    var (sv1, _) = v.Recv();
    var (sv2, _) = v.Recv();
    @string s1 = sv1.String();
    @string s2 = sv2.String();
    if (s1 != "hello"u8 || s2 != "world"u8) {
        t.Errorf("constructed chan: have %q, %q, want %q, %q", s1, s2, helloˢ, worldˢ);
    }
    checkSameType(t, ChanOf(BothDir, TypeOf((TestChanOf_T1)1)), (channel<TestChanOf_T1>)(default));

    // Check arrow token association in undefined chan types.
    channel/*<-*/<channel<TestChanOf_T>> left = channel/*<-*/<channel<TestChanOf_T>>.SendOnly;
    channel</*<-*/channel<TestChanOf_T>> right = channel</*<-*/channel<TestChanOf_T>>.Nil(ChanCargo.Of(new GoChanDir[] { GoChanDir.Both, GoChanDir.Recv }, null));
    var tLeft = ChanOf(SendDir, ChanOf(BothDir, TypeOf((TestChanOf_T)(@string)""u8)));
    var tRight = ChanOf(BothDir, ChanOf(RecvDir, TypeOf((TestChanOf_T)(@string)""u8)));
    if (!AreEqual(tLeft, TypeOf(left))) {
        t.Errorf("chan<-chan: have %s, want %T", tLeft, left);
    }
    if (!AreEqual(tRight, TypeOf(right))) {
        t.Errorf("chan<-chan: have %s, want %T", tRight, right);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object chanˢ = (@string)"<-chan"u8;
internal static readonly object chanˢ2 = (@string)"chan<-"u8;

// check construction and use of type not in binary
internal partial struct TestChanOfDir_T /*@string*/;

// check that type already in binary is found
internal partial struct TestChanOfDir_T1 /*num:nint*/;

public static void TestChanOfDir(ж<Δtesting.T> t) {
    var crt = ChanOf(RecvDir, TypeOf((TestChanOfDir_T)(@string)""u8));
    var cst = ChanOf(SendDir, TypeOf((TestChanOfDir_T)(@string)""u8));
    checkSameType(t, ChanOf(RecvDir, TypeOf((TestChanOfDir_T1)1)), /*<-*/channel<TestChanOfDir_T1>.RecvOnly);
    checkSameType(t, ChanOf(SendDir, TypeOf((TestChanOfDir_T1)1)), channel/*<-*/<TestChanOfDir_T1>.SendOnly);

    // check String form of ChanDir
    if (crt.ChanDir().String() != "<-chan"u8) {
        t.Errorf("chan dir: have %q, want %q", crt.ChanDir().String(), chanˢ);
    }
    if (cst.ChanDir().String() != "chan<-"u8) {
        t.Errorf("chan dir: have %q, want %q", cst.ChanDir().String(), chanˢ2);
    }
}

internal partial class TestChanOfGC_T /*ж<uintptr>*/;

public static void TestChanOfGC(ж<Δtesting.T> t) {
    var xᴷ = new stackAppend<any>(moves: true);

    var tt = TypeOf((TestChanOfGC_T)nil);
    var ct = ChanOf(BothDir, tt);

    // NOTE: The garbage collector handles allocated channels specially,
    // so we have to save pointers to channels in x; the pointer code will
    // use the gc info in the newly constructed chan type.
    const nint n = 100;
    slice<any> x = default;
    for (nint i = 0; i < n; i++) {
        var v = MakeChan(ct, n);
        for (nint j = 0; j < n; j++) {
            var p = @new<uintptr>();
            p.Value = (uintptr)(i * n + j);
            v.Send(ValueOf(p.OrTypedNil()).Convert(tt));
        }
        var pv = New(ct);
        pv.Elem().Set(v);
        x = append(xᴷ, x, pv.Interface());
    }
    Δruntime.GC();

    x = move2heap(xᴷ, x);
    foreach (var (i, xi) in x) {
        var v = ValueOf(xi).Elem();
        for (nint j = 0; j < n; j++) {
            var (pv, _) = v.Recv();
            var k = pv.Elem().Interface();
            if (!AreEqual(k, (uintptr)(i * n + j))) {
                t.Errorf("lost x[%d][%d] = %d, want %d", i, j, k, i * n + j);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string mapA1ˢ => ref mapA1ˢᵇ;
private static readonly @string mapA1ˢᵇ = "map[a:1]"u8;
internal static ref readonly @string invalidKeyTypeˢ => ref invalidKeyTypeˢᵇ;
private static readonly @string invalidKeyTypeˢᵇ = "invalid key type"u8;

// check construction and use of type not in binary
internal partial struct TestMapOf_K /*@string*/;

internal partial struct TestMapOf_V /*num:float64*/;

public static void TestMapOf(ж<Δtesting.T> t) {
    var v = MakeMap(MapOf(TypeOf((TestMapOf_K)(@string)""u8), TypeOf((TestMapOf_V)0D)));
    Δruntime.GC();
    v.SetMapIndex(ValueOf((TestMapOf_K)(@string)"a"), ValueOf((TestMapOf_V)1D));
    Δruntime.GC();

    @string s = fmt.Sprint(v.Interface());
    @string want = mapA1ˢ;
    if (s != want) {
        t.Errorf("constructed map = %s, want %s", s, want);
    }

    // check that type already in binary is found
    checkSameType(t, MapOf(TypeOf((TestMapOf_V)0D), TypeOf((TestMapOf_K)(@string)""u8)), (map<TestMapOf_V, TestMapOf_K>)default);

    // check that invalid key type panics
    shouldPanic(invalidKeyTypeˢ, () => {
        MapOf(TypeOf(((Action)(default)).OrTypedNilFunc()), TypeOf(false));
    });
}

internal partial class TestMapOfGCKeys_T /*ж<uintptr>*/;

public static void TestMapOfGCKeys(ж<Δtesting.T> t) {
    var xᴷ = new stackAppend<any>(moves: true);
    var outᴷ = new stackAppend<nint>();

    var tt = TypeOf((TestMapOfGCKeys_T)nil);
    var mt = MapOf(tt, TypeOf(false));

    // NOTE: The garbage collector handles allocated maps specially,
    // so we have to save pointers to maps in x; the pointer code will
    // use the gc info in the newly constructed map type.
    const nint n = 100;
    slice<any> x = default;
    for (nint i = 0; i < n; i++) {
        var v = MakeMap(mt);
        for (nint j = 0; j < n; j++) {
            var p = @new<uintptr>();
            p.Value = (uintptr)(i * n + j);
            v.SetMapIndex(ValueOf(p.OrTypedNil()).Convert(tt), ValueOf(true));
        }
        var pv = New(mt);
        pv.Elem().Set(v);
        x = append(xᴷ, x, pv.Interface());
    }
    Δruntime.GC();

    x = move2heap(xᴷ, x);
    foreach (var (i, xi) in x) {
        var v = ValueOf(xi).Elem();
        slice<nint> @out = default;
        foreach (var (_, kv) in v.MapKeys()) {
            @out = append(outᴷ, @out, (nint)(kv.Elem().Interface()._<uintptr>()));
        }
        slices.Sort<slice<nint>, nint>(@out);
        foreach (var (j, k) in @out) {
            if (k != i * n + j) {
                t.Errorf("lost x[%d][%d] = %d, want %d", i, j, k, i * n + j);
            }
        }
    }
}

internal partial struct TestMapOfGCBigKey_KV /*dyn*/ {
    internal int64 i;
    internal int64 j;
}

// Test assignment and access to a map with keys larger than word size.
public static void TestMapOfGCBigKey(ж<Δtesting.T> t) {
    var kvTyp = TypeFor<TestMapOfGCBigKey_KV>();
    var mt = MapOf(kvTyp, kvTyp);

    const nint n = 100;
    var m = MakeMap(mt);
    for (nint i = 0; i < n; i++) {
        var kv = new TestMapOfGCBigKey_KV((int64)i, (int64)(i + 1));
        m.SetMapIndex(ValueOf(kv), ValueOf(kv));
    }

    for (nint i = 0; i < n; i++) {
        var kv = new TestMapOfGCBigKey_KV((int64)i, (int64)(i + 1));
        var elem = m.MapIndex(ValueOf(kv)).Interface()._<TestMapOfGCBigKey_KV>();
        if (elem != kv) {
            t.Errorf("lost m[%v] = %v, want %v", kv, elem, kv);
        }
    }
}

internal partial class TestMapOfGCValues_T /*ж<uintptr>*/;

public static void TestMapOfGCValues(ж<Δtesting.T> t) {
    var xᴷ = new stackAppend<any>(moves: true);

    var tt = TypeOf((TestMapOfGCValues_T)nil);
    var mt = MapOf(TypeOf((nint)1), tt);

    // NOTE: The garbage collector handles allocated maps specially,
    // so we have to save pointers to maps in x; the pointer code will
    // use the gc info in the newly constructed map type.
    const nint n = 100;
    slice<any> x = default;
    for (nint i = 0; i < n; i++) {
        var v = MakeMap(mt);
        for (nint j = 0; j < n; j++) {
            var p = @new<uintptr>();
            p.Value = (uintptr)(i * n + j);
            v.SetMapIndex(ValueOf(j), ValueOf(p.OrTypedNil()).Convert(tt));
        }
        var pv = New(mt);
        pv.Elem().Set(v);
        x = append(xᴷ, x, pv.Interface());
    }
    Δruntime.GC();

    x = move2heap(xᴷ, x);
    foreach (var (i, xi) in x) {
        var v = ValueOf(xi).Elem();
        for (nint j = 0; j < n; j++) {
            var k = v.MapIndex(ValueOf(j)).Elem().Interface()._<uintptr>();
            if (k != (uintptr)(i * n + j)) {
                t.Errorf("lost x[%d][%d] = %d, want %d", i, j, k, i * n + j);
            }
        }
    }
}

public static void TestTypelinksSorted(ж<Δtesting.T> t) {
    @string last = default;
    foreach (var (i, n) in reflect_internal_test_package.TypeLinks()) {
        if (n < last) {
            t.Errorf("typelinks not sorted: %q [%d] > %q [%d]", last, i - 1, n, i);
        }
        last = n;
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string gopherˢ => ref gopherˢᵇ;
private static readonly @string gopherˢᵇ = "gopher"u8;
internal static ref readonly @string mustBeSliceˢ => ref mustBeSliceˢᵇ;
private static readonly @string mustBeSliceˢᵇ = "must be slice"u8;

// check construction and use of type not in binary
internal partial struct TestFuncOf_K /*@string*/;

internal partial struct TestFuncOf_V /*num:float64*/;

// check that types already in binary are found
internal partial struct TestFuncOf_T1 /*num:nint*/;

internal partial struct TestFuncOf_testCases /*dyn*/ {
    internal slice<reflectꓸType> @in, @out;
    internal bool variadic;
    internal any want;
}

public static void TestFuncOf(ж<Δtesting.T> t) {
    var inᴷ = new stackAppend<reflectꓸType>();

    var fn = (slice<reflectꓸValue> args) => {
        if (len(args) != 1) {
            t.Errorf("args == %v, want exactly one arg", args);
        } else if (!AreEqual(args[0].Type(), TypeOf((TestFuncOf_K)(@string)""u8))) {
            t.Errorf("args[0] is type %v, want %v", args[0].Type(), TypeOf((TestFuncOf_K)(@string)""u8));
        } else if (args[0].String() != "gopher"u8) {
            t.Errorf("args[0] = %q, want %q", args[0].String(), gopherˢ);
        }
        return slice<reflectꓸValue>([ValueOf((TestFuncOf_V)3.14D)]);
    };
    var v = MakeFunc(FuncOf(slice<reflectꓸType>([TypeOf((TestFuncOf_K)(@string)""u8)]), slice<reflectꓸType>([TypeOf((TestFuncOf_V)0D)]), false), fn);

    var outs = v.Call(slice<reflectꓸValue>([ValueOf((TestFuncOf_K)(@string)gopherˢ)]));
    if (len(outs) != 1) {
        t.Fatalf("v.Call returned %v, want exactly one result", outs);
    } else if (!AreEqual(outs[0].Type(), TypeOf((TestFuncOf_V)0D))) {
        t.Fatalf("c.Call[0] is type %v, want %v", outs[0].Type(), TypeOf((TestFuncOf_V)0D));
    }
    var f = outs[0].Float();
    if (f != 3.14D) {
        t.Errorf("constructed func returned %f, want %f", f, 3.14D);
    }
    var testCases = slice<TestFuncOf_testCases>([
        new(@in: slice<reflectꓸType>([TypeOf((TestFuncOf_T1)0)]), want: ((Action<TestFuncOf_T1>)(default)).OrTypedNilFunc()),
        new(@in: slice<reflectꓸType>([TypeOf((nint)0)]), want: ((Action<nint>)(default)).OrTypedNilFunc()),
        new(@in: slice<reflectꓸType>([SliceOf(TypeOf((nint)0))]), variadic: true, want: (((Actionꓸꓸꓸ<nint>)((Actionꓸꓸꓸ<nint>)(default)))).OrTypedNilFunc()),
        new(@in: slice<reflectꓸType>([TypeOf((nint)0)]), @out: slice<reflectꓸType>([TypeOf(false)]), want: ((Func<nint, bool>)(default)).OrTypedNilFunc()),
        new(@in: slice<reflectꓸType>([TypeOf((nint)0)]), @out: slice<reflectꓸType>([TypeOf(false), TypeOf((@string)""u8)]), want: ((Func<nint, (bool, @string)>)(default)).OrTypedNilFunc())
    ]);
    foreach (var (_, tt) in testCases) {
        checkSameType(t, FuncOf(tt.@in, tt.@out, tt.variadic), tt.want);
    }

    // check that variadic requires last element be a slice.
    FuncOf(slice<reflectꓸType>([TypeOf((nint)1), TypeOf((@string)""u8), SliceOf(TypeOf(false))]), default, true);
    shouldPanic(mustBeSliceˢ, () => {
        FuncOf(slice<reflectꓸType>([TypeOf((nint)0), TypeOf((@string)""u8), TypeOf(false)]), default, true);
    });
    shouldPanic(mustBeSliceˢ, () => {
        FuncOf(default, default, true);
    });

    //testcase for  #54669
    slice<reflectꓸType> @in = default;
    for (nint i = 0; i < 51; i++) {
        @in = append(inᴷ, @in, TypeOf((nint)1));
    }
    FuncOf(@in, default, false);
}

partial struct R0 {
    /*embed*/ public ж<R1> R1;
    /*embed*/ public ж<R2> R2;
    /*embed*/ public ж<R3> R3;
    /*embed*/ public ж<R4> R4;
}

partial struct R1 {
    /*embed*/ public ж<R5> R5;
    /*embed*/ public ж<R6> R6;
    /*embed*/ public ж<R7> R7;
    /*embed*/ public ж<R8> R8;
}

partial struct R2 /*R1*/;

partial struct R3 /*R1*/;

partial struct R4 /*R1*/;

partial struct R5 {
    /*embed*/ public ж<R9> R9;
    /*embed*/ public ж<R10> R10;
    /*embed*/ public ж<R11> R11;
    /*embed*/ public ж<R12> R12;
}

partial struct R6 /*R5*/;

partial struct R7 /*R5*/;

partial struct R8 /*R5*/;

partial struct R9 {
    /*embed*/ public ж<R13> R13;
    /*embed*/ public ж<R14> R14;
    /*embed*/ public ж<R15> R15;
    /*embed*/ public ж<R16> R16;
}

partial struct R10 /*R9*/;

partial struct R11 /*R9*/;

partial struct R12 /*R9*/;

partial struct R13 {
    /*embed*/ public ж<R17> R17;
    /*embed*/ public ж<R18> R18;
    /*embed*/ public ж<R19> R19;
    /*embed*/ public ж<R20> R20;
}

partial struct R14 /*R13*/;

partial struct R15 /*R13*/;

partial struct R16 /*R13*/;

partial struct R17 {
    /*embed*/ public ж<R21> R21;
    /*embed*/ public ж<R22> R22;
    /*embed*/ public ж<R23> R23;
    /*embed*/ public ж<R24> R24;
}

partial struct R18 /*R17*/;

partial struct R19 /*R17*/;

partial struct R20 /*R17*/;

partial struct R21 {
    public nint X;
}

partial struct R22 /*R21*/;

partial struct R23 /*R21*/;

partial struct R24 /*R21*/;

public static void TestEmbed(ж<Δtesting.T> t) {
    var typ = TypeOf(new R0());
    var (f, ok) = typ.FieldByName("X");
    if (ok) {
        t.Fatalf(@"FieldByName(""X"") should fail, returned %v", f.Index);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object allocsˢ = (@string)"allocs:"u8;

public static void TestAllocsInterfaceBig(ж<Δtesting.T> t) {
    if (Δtesting.Short()) {
        t.Skip(skippingMallocCountInˢ);
    }
    ref var v = ref heap<reflectꓸValue>(ValueOf(new S()), out var Ꮡv);
    {
        var vʗ1 = v;
        var allocs = Δtesting.AllocsPerRun(100, () => {
            vʗ1.Interface();
        }); if (allocs > 0D) {
            t.Error(allocsˢ, allocs);
        }
    }
}

public static void TestAllocsInterfaceSmall(ж<Δtesting.T> t) {
    if (Δtesting.Short()) {
        t.Skip(skippingMallocCountInˢ);
    }
    ref var v = ref heap<reflectꓸValue>(ValueOf((int64)0), out var Ꮡv);
    {
        var vʗ1 = v;
        var allocs = Δtesting.AllocsPerRun(100, () => {
            vʗ1.Interface();
        }); if (allocs > 0D) {
            t.Error(allocsˢ, allocs);
        }
    }
}

// An exhaustive is a mechanism for writing exhaustive or stochastic tests.
// The basic usage is:
//
//	for x.Next() {
//		... code using x.Maybe() or x.Choice(n) to create test cases ...
//	}
//
// Each iteration of the loop returns a different set of results, until all
// possible result sets have been explored. It is okay for different code paths
// to make different method call sequences on x, but there must be no
// other source of non-determinism in the call sequences.
//
// When faced with a new decision, x chooses randomly. Future explorations
// of that path will choose successive values for the result. Thus, stopping
// the loop after a fixed number of iterations gives somewhat stochastic
// testing.
//
// Example:
//
//	for x.Next() {
//		v := make([]bool, x.Choose(4))
//		for i := range v {
//			v[i] = x.Maybe()
//		}
//		fmt.Println(v)
//	}
//
// prints (in some order):
//
//	[]
//	[false]
//	[true]
//	[false false]
//	[false true]
//	...
//	[true true]
//	[false false false]
//	...
//	[true true true]
//	[false false false false]
//	...
//	[true true true true]
partial struct exhaustive {
    internal ж<rand.Rand> r;
    internal nint pos;
    internal slice<choice> last;
}

partial struct choice {
    internal nint off;
    internal nint n;
    internal nint max;
}

internal static bool Next(this ref exhaustive x) {
    if (x.r == nil) {
        x.r = rand.New(rand.NewSource(time.Now().UnixNano()));
    }
    x.pos = 0;
    if (x.last == nil) {
        x.last = slice<choice>([]);
        return true;
    }
    for (nint i = len(x.last) - 1; i >= 0; i--) {
        var c = Ꮡ(x.last, i);
        if (c.n + 1 < c.max) {
            c.n++;
            x.last = x.last.slice(0, i + 1);
            return true;
        }
    }
    return false;
}

internal static nint Choose(this ref exhaustive x, nint max) {
    if (x.pos >= len(x.last)) {
        x.last = append(x.last, new choice(x.r.Intn(max), 0, max));
    }
    var c = Ꮡ(x.last, x.pos);
    x.pos++;
    if (c.max != max) {
        throw panic("inconsistent use of exhaustive tester");
    }
    return rem((c.n + c.off), max);
}

internal static bool Maybe(this ref exhaustive x) {
    return x.Choose(2) == 1;
}

public static slice<reflectꓸValue> GCFunc(slice<reflectꓸValue> args) {
    Δruntime.GC();
    return slice<reflectꓸValue>([]);
}

public static void TestReflectFuncTraceback(ж<Δtesting.T> t) {
    var f = MakeFunc(TypeOf(() => {
    }), GCFunc);
    f.Call(slice<reflectꓸValue>([]));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string gcMethodˢ => ref gcMethodˢᵇ;
private static readonly @string gcMethodˢᵇ = "GCMethod"u8;

public static void TestReflectMethodTraceback(ж<Δtesting.T> t) {
    var p = new Point(3, 4);
    var m = ValueOf(p).MethodByName(gcMethodˢ);
    var i = ValueOf(m.Interface()).Call(slice<reflectꓸValue>([ValueOf((nint)5)]))[0].Int();
    if (i != 8) {
        t.Errorf("Call returned %d; want 8", i);
    }
}

internal partial struct TestSmallZero_T /*[10]byte*/;

public static void TestSmallZero(ж<Δtesting.T> t) {
    var typ = TypeOf(new TestSmallZero_T(new byte[10].array()));
    {
        var typʗ1 = typ;
        var allocs = Δtesting.AllocsPerRun(100, () => {
            Zero(typʗ1);
        }); if (allocs > 0D) {
            t.Errorf("Creating small zero values caused %f allocs, want 0", allocs);
        }
    }
}

public static void TestBigZero(ж<Δtesting.T> t) {
    UntypedInt size = /* 1 << 10 */ 1024;
    array<byte> v = new(1024); /* size */
    var z = Zero(ValueOf(v).Type()).Interface()._<array<byte>>();
    for (nint i = 0; i < size; i++) {
        if (z[i] != 0) {
            t.Fatalf("Zero object not all zero, index %d", i);
        }
    }
}

internal partial struct TestZeroSet_T /*[16]byte*/;

internal partial struct TestZeroSet_S /*dyn*/ {
    internal uint64 a;
    public TestZeroSet_T T;
    internal uint64 b;
}

public static void TestZeroSet(ж<Δtesting.T> t) {
    ref var v = ref heap<TestZeroSet_S>(new TestZeroSet_S(
        a: 0xaaaaaaaaaaaaaaaaUL,
        T: new TestZeroSet_T(new byte[]{9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9}.array()),
        b: 0xbbbbbbbbbbbbbbbbUL
    ), out var Ꮡv);
    ValueOf(Ꮡv).Elem().Field(1).Set(Zero(TypeOf(new TestZeroSet_T(new byte[16].array()))));
    if (v != (new TestZeroSet_S(
        a: 0xaaaaaaaaaaaaaaaaUL,
        b: 0xbbbbbbbbbbbbbbbbUL
    ))) {
        t.Fatalf("Setting a field to a Zero value didn't work");
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string nilPointerToEmbeddedˢ => ref nilPointerToEmbeddedˢᵇ;
private static readonly @string nilPointerToEmbeddedˢᵇ = "nil pointer to embedded struct"u8;

internal partial struct TestFieldByIndexNil_P /*dyn*/ {
    public nint F;
}

internal partial struct TestFieldByIndexNil_T /*dyn*/ {
    /*embed*/ public ж<TestFieldByIndexNil_P> P;
}

public static void TestFieldByIndexNil(ж<Δtesting.T> t) {
    GoFrame ᒐ = default;
    try {
        var v = ValueOf(new TestFieldByIndexNil_T());

        v.FieldByName("P"); // should be fine

        defer(() => {
            {
                var err = recover(); if (err == nil) {
                    t.Fatalf("no error");
                } else if (!strings.Contains(fmt.Sprint(err), nilPointerToEmbeddedˢ)) {
                    t.Fatalf(@"err=%q, wanted error containing ""nil pointer to embedded struct""", err);
                }
            }
        }, ref ᒐ);
        v.FieldByName("F"); // should panic

        t.Fatalf("did not panic");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Given
//	type Outer struct {
//		*Inner
//		...
//	}
// the compiler generates the implementation of (*Outer).M dispatching to the embedded Inner.
// The implementation is logically:
//	func (p *Outer) M() {
//		(p.Inner).M()
//	}
// but since the only change here is the replacement of one pointer receiver with another,
// the actual generated code overwrites the original receiver with the p.Inner pointer and
// then jumps to the M method expecting the *Inner receiver.
//
// During reflect.Value.Call, we create an argument frame and the associated data structures
// to describe it to the garbage collector, populate the frame, call reflect.call to
// run a function call using that frame, and then copy the results back out of the frame.
// The reflect.call function does a memmove of the frame structure onto the
// stack (to set up the inputs), runs the call, and the memmoves the stack back to
// the frame structure (to preserve the outputs).
//
// Originally reflect.call did not distinguish inputs from outputs: both memmoves
// were for the full stack frame. However, in the case where the called function was
// one of these wrappers, the rewritten receiver is almost certainly a different type
// than the original receiver. This is not a problem on the stack, where we use the
// program counter to determine the type information and understand that
// during (*Outer).M the receiver is an *Outer while during (*Inner).M the receiver in the same
// memory word is now an *Inner. But in the statically typed argument frame created
// by reflect, the receiver is always an *Outer. Copying the modified receiver pointer
// off the stack into the frame will store an *Inner there, and then if a garbage collection
// happens to scan that argument frame before it is discarded, it will scan the *Inner
// memory as if it were an *Outer. If the two have different memory layouts, the
// collection will interpret the memory incorrectly.
//
// One such possible incorrect interpretation is to treat two arbitrary memory words
// (Inner.P1 and Inner.P2 below) as an interface (Outer.R below). Because interpreting
// an interface requires dereferencing the itab word, the misinterpretation will try to
// deference Inner.P1, causing a crash during garbage collection.
//
// This came up in a real program in issue 7725.
partial struct Outer {
    /*embed*/ public ж<Inner> Inner;
    public Δio.Reader R;
}

partial struct Inner {
    public ж<Outer> X;
    public uintptr P1;
    public uintptr P2;
}

public static void M(this ж<Inner> Ꮡpi) {
    ref var pi = ref Ꮡpi.DerefOrNull();

    // Clear references to pi so that the only way the
    // garbage collection will find the pointer is in the
    // argument frame, typed as a *Outer.
    pi.X.Inner = default;

    // Set up an interface value that will cause a crash.
    // P1 = 1 is a non-zero, so the interface looks non-nil.
    // P2 = pi ensures that the data word points into the
    // allocated heap; if not the collection skips the interface
    // value as irrelevant, without dereferencing P1.
    pi.P1 = 1;
    pi.P2 = (uintptr)(uintptr)@unsafe.Pointer.FromRef(ref pi);
}

public static void TestCallMethodJump(ж<Δtesting.T> t) {
    // In reflect.Value.Call, trigger a garbage collection after reflect.call
    // returns but before the args frame has been discarded.
    // This is a little clumsy but makes the failure repeatable.
    reflect_internal_test_package.CallGC.Value = true;

    var p = Ꮡ(new Outer(Inner: @new<Inner>()));
    p.Inner.X = p;
    ValueOf(p.OrTypedNil()).Method(0).Call(default);

    // Stop garbage collecting during reflect.call.
    reflect_internal_test_package.CallGC.Value = false;
}

internal partial struct TestCallArgLive_T /*dyn*/ { // pointerful aggregate
    public ж<@string> X, Y;
}

public static void TestCallArgLive(ж<Δtesting.T> t) {
    var F = (TestCallArgLive_T tΔ1) => {
        tΔ1.X.Value = "ok";
    };

    // In reflect.Value.Call, trigger a garbage collection in reflect.call
    // between marshaling argument and the actual call.
    reflect_internal_test_package.CallGC.Value = true;

    var x = @new<@string>();
    Δruntime.SetFinalizer(x.OrTypedNil(), (ж<@string> p) => {
        if (p.Value != "ok"u8) {
            t.Errorf("x dead prematurely");
        }
    });
    var v = new TestCallArgLive_T(x, nil);

    ValueOf((F).OrTypedNilFunc()).Call(slice<reflectꓸValue>([ValueOf(v)]));

    // Stop garbage collecting during reflect.call.
    reflect_internal_test_package.CallGC.Value = false;
}

public static void TestMakeFuncStackCopy(ж<Δtesting.T> t) {
    var target = (slice<reflectꓸValue> @in) => {
        Δruntime.GC();
        useStack(16);
        return slice<reflectꓸValue>([ValueOf((nint)9)]);
    };

    ref var concrete = ref heap<Func<ж<nint>, nint, nint>>(out var Ꮡconcrete);
    var fn = MakeFunc(ValueOf((concrete).OrTypedNilFunc()).Type(), target);
    ValueOf(Ꮡconcrete).Elem().Set(fn);
    nint x = concrete(nil, 7);
    if (x != 9) {
        t.Errorf("have %d want 9", x);
    }
}

// use about n KB of stack
internal static void useStack(nint n) {
    if (n == 0) {
        return;
    }
    array<byte> b = new(1024); // makes frame about 1KB
    useStack(n - 1 + (nint)b[99]);
}

partial struct Impl {
}

public static void F(this Impl _) {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object reflectTestImplValueˢ = (@string)"<reflect_test.Impl Value>"u8;
internal static readonly object funcValueˢ = (@string)"<func() Value>"u8;

public static void TestValueString(ж<Δtesting.T> t) {
    var rv = ValueOf(new Impl());
    if (rv.String() != "<reflect_test.Impl Value>"u8) {
        t.Errorf("ValueOf(Impl{}).String() = %q, want %q", rv.String(), reflectTestImplValueˢ);
    }

    var method = rv.Method(0);
    if (method.String() != "<func() Value>"u8) {
        t.Errorf("ValueOf(Impl{}).Method(0).String() = %q, want %q", method.String(), funcValueˢ);
    }
}

// Used to have inconsistency between IsValid() and Kind() != Invalid.
internal partial struct TestInvalid_T /*dyn*/ {
    internal any v;
}

public static void TestInvalid(ж<Δtesting.T> t) {
    var v = ValueOf(new TestInvalid_T()).Field(0);
    if (v.IsValid() != true || v.Kind() != ΔInterface) {
        t.Errorf("field: IsValid=%v, Kind=%v, want true, Interface", v.IsValid(), v.Kind());
    }
    v = v.Elem();
    if (v.IsValid() != false || v.Kind() != Invalid) {
        t.Errorf("field elem: IsValid=%v, Kind=%v, want false, Invalid", v.IsValid(), v.Kind());
    }
}

// Issue 8917.
public static void TestLarge(ж<Δtesting.T> t) {
    var fv = ValueOf(([GoArrayDims(256)] array<ж<byte>> _) => {
    });
    fv.Call(slice<reflectꓸValue>([ValueOf(new ж<byte>[]{}.array(256))]));
}

internal static any /*recovered*/ fieldIndexRecover(reflectꓸType t, nint i) {
    any recovered = default;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            recovered = recover();
        }, ref ᒐ);

        t.Field(i);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return recovered;
}

internal partial struct TestTypeFieldOutOfRangePanic_i /*dyn*/ {
    public nint X;
}

internal partial struct TestTypeFieldOutOfRangePanic_testIndices /*dyn*/ {
    internal nint i;
    internal bool mustPanic;
}

// Issue 15046.
public static void TestTypeFieldOutOfRangePanic(ж<Δtesting.T> t) {
    var typ = TypeOf(new TestTypeFieldOutOfRangePanic_i(10));
    var testIndices = new array<TestTypeFieldOutOfRangePanic_testIndices>(4){
        [0] = new(-2, true),
        [1] = new(0, false),
        [2] = new(1, true),
        [3] = new(1 << 10, true)
    };
    foreach (var (i, tt) in testIndices.ΔRangeSnapshot()) {
        var recoveredErr = fieldIndexRecover(typ, tt.i);
        if (tt.mustPanic) {
            if (recoveredErr == nil) {
                t.Errorf("#%d: fieldIndex %d expected to panic", i, tt.i);
            }
        } else {
            if (recoveredErr != nil) {
                t.Errorf("#%d: got err=%v, expected no panic", i, recoveredErr);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object testDoesNotFaultOnGoosJsˢ = (@string)"test does not fault on GOOS=js"u8;

public static void TestTypeFieldReadOnly(ж<Δtesting.T> t) {
    GoFrame ᒐ = default;
    try {
        if (Δruntime.GOOS == "js"u8 || Δruntime.GOOS == "wasip1"u8) {
            // This is OK because we don't use the optimization
            // for js or wasip1.
            t.Skip(testDoesNotFaultOnGoosJsˢ);
        }

        // It's important that changing one StructField.Index
        // value not affect other StructField.Index values.
        // Right now StructField.Index is read-only;
        // that saves allocations but is otherwise not important.
        var typ = TypeFor<TestAllocations_type>();
        ref var f = ref heap<Δreflect.StructField>(typ.Field(0), out var Ꮡf);
        defer(debug.SetPanicOnFault, debug.SetPanicOnFault(true), ref ᒐ);
        var fʗ1 = f;
        shouldPanic(""u8, () => {
            fʗ1.Index[0] = 1;
        });
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fourˢ => ref fourˢᵇ;
private static readonly @string fourˢᵇ = "four"u8;
internal static ref readonly @string five5ˢ => ref five5ˢᵇ;
private static readonly @string five5ˢᵇ = "five5"u8;
internal static ref readonly @string six666ˢ => ref six666ˢᵇ;
private static readonly @string six666ˢᵇ = "six666"u8;
internal static ref readonly @string seven77ˢ => ref seven77ˢᵇ;
private static readonly @string seven77ˢᵇ = "seven77"u8;
internal static ref readonly @string eight888ˢ => ref eight888ˢᵇ;
private static readonly @string eight888ˢᵇ = "eight888"u8;

// Issue 9179.
public static void TestCallGC(ж<Δtesting.T> t) {
    var f = (@string a, @string b, @string c, @string d, @string e) => {
    };
    var g = slice<reflectꓸValue> (slice<reflectꓸValue> @in) => {
        Δruntime.GC();
        return default;
    };
    var typ = ValueOf((f).OrTypedNilFunc()).Type();
    var f2 = MakeFunc(typ, g).Interface()._<Action<@string, @string, @string, @string, @string>>();
    f2(fourˢ, five5ˢ, six666ˢ, seven77ˢ, eight888ˢ);
}

// Issue 18635 (function version).
public static void TestKeepFuncLive(ж<Δtesting.T> t) {
    // Test that we keep makeFuncImpl live as long as it is
    // referenced on the stack.
    var typ = TypeOf((nint i) => {
    });
    Func<slice<reflectꓸValue>, slice<reflectꓸValue>> f = default;
    ref var g = ref heap<Func<slice<reflectꓸValue>, slice<reflectꓸValue>>>(out var Ꮡg);
    var typʗ1 = typ;
    f = slice<reflectꓸValue> (slice<reflectꓸValue> @in) => {
        clobber();
        nint i = (nint)@in[0].Int();
        if (i > 0) {
            // We can't use Value.Call here because
            // runtime.call* will keep the makeFuncImpl
            // alive. However, by converting it to an
            // interface value and calling that,
            // reflect.callReflect is the only thing that
            // can keep the makeFuncImpl live.
            //
            // Alternate between f and g so that if we do
            // reuse the memory prematurely it's more
            // likely to get obviously corrupted.
            MakeFunc(typʗ1, Ꮡg.ValueSlot).Interface()._<Action<nint>>()(i - 1);
        }
        return default;
    };
    var fʗ1 = f;
    var typʗ2 = typ;
    g = slice<reflectꓸValue> (slice<reflectꓸValue> @in) => {
        clobber();
        nint i = (nint)@in[0].Int();
        MakeFunc(typʗ2, fʗ1).Interface()._<Action<nint>>()(i);
        return default;
    };
    MakeFunc(typ, f).Call(slice<reflectꓸValue>([ValueOf((nint)10)]));
}

partial struct UnExportedFirst /*num:nint*/;

public static void ΦExported(this UnExportedFirst i) {
}

internal static void unexported(this UnExportedFirst i) {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string exportedˢ => ref exportedˢᵇ;
private static readonly @string exportedˢᵇ = "ΦExported"u8;

// Issue 21177
public static void TestMethodByNameUnExportedFirst(ж<Δtesting.T> t) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            if (recover() != nil) {
                t.Errorf("should not panic");
            }
        }, ref ᒐ);
        var typ = TypeOf((UnExportedFirst)0);
        var (m, _) = typ.MethodByName(exportedˢ);
        if (m.Name != "ΦExported"u8) {
            t.Errorf("got %s, expected ΦExported", m.Name);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Issue 18635 (method version).
partial struct KeepMethodLive {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string method2ˢ => ref method2ˢᵇ;
private static readonly @string method2ˢᵇ = "Method2"u8;

public static void Method1(this KeepMethodLive k, nint i) {
    clobber();
    if (i > 0) {
        ValueOf(k).MethodByName(method2ˢ).Interface()._<Action<nint>>()(i - 1);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string method1ˢ => ref method1ˢᵇ;
private static readonly @string method1ˢᵇ = "Method1"u8;

public static void Method2(this KeepMethodLive k, nint i) {
    clobber();
    ValueOf(k).MethodByName(method1ˢ).Interface()._<Action<nint>>()(i);
}

public static void TestKeepMethodLive(ж<Δtesting.T> t) {
    // Test that we keep methodValue live as long as it is
    // referenced on the stack.
    new KeepMethodLive().Method1(10);
}

// clobber tries to clobber unreachable memory.
internal static void clobber() {
    Δruntime.GC();
    for (nint i = 1; i < 32; i++) {
        for (nint j = 0; j < 10; j++) {
            var obj = new slice<ж<byte>>(i);
            sink = obj;
        }
    }
    Δruntime.GC();
}

internal partial struct TestFuncLayout_S /*dyn*/ {
    internal uintptr a, b;
    internal ж<byte> c, d;
}

internal partial struct TestFuncLayout_test /*dyn*/ {
    internal reflectꓸType rcvr, typ;
    internal uintptr size, argsize, retOffset;
    internal slice<byte> stack, gc, inRegs, outRegs; // pointer bitmap: 1 is pointer, 0 is scalar
    internal nint intRegs, floatRegs;
    internal uintptr floatRegSize;
}

public static void TestFuncLayout(ж<Δtesting.T> t) {
    uintptr align(uintptr x) => (uintptr)((x + (uintptr)goarch.PtrSize - 1) & ~(uintptr)(goarch.PtrSize - 1));
    slice<byte> r = default;
    if (goarch.PtrSize == 4) {
        r = slice<byte>([0, 0, 0, 1]);
    } else {
        r = slice<byte>([0, 0, 1]);
    }
    var tests = slice<TestFuncLayout_test>([
        new(
            typ: ValueOf(@string (@string a, @string b) => ""u8).Type(),
            size: 6 * goarch.PtrSize,
            argsize: 4 * goarch.PtrSize,
            retOffset: 4 * goarch.PtrSize,
            stack: slice<byte>([1, 0, 1, 0, 1]),
            gc: slice<byte>([1, 0, 1, 0, 1])
        ),
        new(
            typ: ValueOf((uint32 a, uint32 b, uint32 c, ж<byte> p, uint16 d) => {
            }).Type(),
            size: align(align(3 * 4) + (uintptr)goarch.PtrSize + 2),
            argsize: align(3 * 4) + (uintptr)goarch.PtrSize + 2,
            retOffset: align(align(3 * 4) + (uintptr)goarch.PtrSize + 2),
            stack: r,
            gc: r
        ),
        new(
            typ: ValueOf((map<nint, nint> a, uintptr b, any c) => {
            }).Type(),
            size: 4 * goarch.PtrSize,
            argsize: 4 * goarch.PtrSize,
            retOffset: 4 * goarch.PtrSize,
            stack: slice<byte>([1, 0, 1, 1]),
            gc: slice<byte>([1, 0, 1, 1])
        ),
        new(
            typ: ValueOf((TestFuncLayout_S a) => {
            }).Type(),
            size: 4 * goarch.PtrSize,
            argsize: 4 * goarch.PtrSize,
            retOffset: 4 * goarch.PtrSize,
            stack: slice<byte>([0, 0, 1, 1]),
            gc: slice<byte>([0, 0, 1, 1])
        ),
        new(
            rcvr: ValueOf((ж<byte>)nil).Type(),
            typ: ValueOf((uintptr a, ж<nint> b) => {
            }).Type(),
            size: 3 * goarch.PtrSize,
            argsize: 3 * goarch.PtrSize,
            retOffset: 3 * goarch.PtrSize,
            stack: slice<byte>([1, 0, 1]),
            gc: slice<byte>([1, 0, 1])
        ),
        new(
            typ: ValueOf((uintptr a) => {
            }).Type(),
            size: goarch.PtrSize,
            argsize: goarch.PtrSize,
            retOffset: goarch.PtrSize,
            stack: slice<byte>([]),
            gc: slice<byte>([])
        ),
        new(
            typ: ValueOf(uintptr () => (uintptr)(0)).Type(),
            size: goarch.PtrSize,
            argsize: 0,
            retOffset: 0,
            stack: slice<byte>([]),
            gc: slice<byte>([])
        ),
        new(
            rcvr: ValueOf((uintptr)0).Type(),
            typ: ValueOf((uintptr a) => {
            }).Type(),
            size: 2 * goarch.PtrSize,
            argsize: 2 * goarch.PtrSize,
            retOffset: 2 * goarch.PtrSize,
            stack: slice<byte>([1]),
            gc: slice<byte>([1]) // Note: this one is tricky, as the receiver is not a pointer. But we
 // pass the receiver by reference to the autogenerated pointer-receiver
 // version of the function.

        )
        // TODO(mknyszek): Add tests for non-zero register count.
    ]);
    foreach (var (_, vᴛ1) in tests) {
        ref var lt = ref heap(new TestFuncLayout_test(), out var Ꮡlt);
        lt = vᴛ1;

        @string name = lt.typ.String();
        if (lt.rcvr != nil) {
            name = lt.rcvr.String() + "."u8 + name;
        }
        var ltʗ1 = lt;
        t.Run(name, (ж<Δtesting.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                defer(ᴛ1 => reflect_internal_test_package.SetArgRegs(ᴛ1.Item1, ᴛ1.Item2, ᴛ1.Item3), reflect_internal_test_package.SetArgRegs(ltʗ1.intRegs, ltʗ1.floatRegs, ltʗ1.floatRegSize), ref ᒐ);

                var (typ, argsize, retOffset, stack, gc, inRegs, outRegs, ptrs) = reflect_internal_test_package.FuncLayout(ltʗ1.typ, ltʗ1.rcvr);
                if (typ.Size() != ltʗ1.size) {
                    tΔ1.Errorf("funcLayout(%v, %v).size=%d, want %d", ltʗ1.typ, ltʗ1.rcvr, typ.Size(), ltʗ1.size);
                }
                if (argsize != ltʗ1.argsize) {
                    tΔ1.Errorf("funcLayout(%v, %v).argsize=%d, want %d", ltʗ1.typ, ltʗ1.rcvr, argsize, ltʗ1.argsize);
                }
                if (retOffset != ltʗ1.retOffset) {
                    tΔ1.Errorf("funcLayout(%v, %v).retOffset=%d, want %d", ltʗ1.typ, ltʗ1.rcvr, retOffset, ltʗ1.retOffset);
                }
                if (!bytes.Equal(stack, ltʗ1.stack)) {
                    tΔ1.Errorf("funcLayout(%v, %v).stack=%v, want %v", ltʗ1.typ, ltʗ1.rcvr, stack, ltʗ1.stack);
                }
                if (!bytes.Equal(gc, ltʗ1.gc)) {
                    tΔ1.Errorf("funcLayout(%v, %v).gc=%v, want %v", ltʗ1.typ, ltʗ1.rcvr, gc, ltʗ1.gc);
                }
                if (!bytes.Equal(inRegs, ltʗ1.inRegs)) {
                    tΔ1.Errorf("funcLayout(%v, %v).inRegs=%v, want %v", ltʗ1.typ, ltʗ1.rcvr, inRegs, ltʗ1.inRegs);
                }
                if (!bytes.Equal(outRegs, ltʗ1.outRegs)) {
                    tΔ1.Errorf("funcLayout(%v, %v).outRegs=%v, want %v", ltʗ1.typ, ltʗ1.rcvr, outRegs, ltʗ1.outRegs);
                }
                if (ptrs && len(stack) == 0 || !ptrs && len(stack) > 0) {
                    tΔ1.Errorf("funcLayout(%v, %v) pointers flag=%v, want %v", ltʗ1.typ, ltʗ1.rcvr, ptrs, !ptrs);
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

// trimBitmap removes trailing 0 elements from b and returns the result.
internal static slice<byte> trimBitmap(slice<byte> b) {
    while (len(b) > 0 && b[len(b) - 1] == 0) {
        b = b.slice(0, len(b) - 1);
    }
    return b;
}

internal static partial void verifyGCBits(ж<Δtesting.T> t, reflectꓸType typ, slice<byte> bits) {
    var heapBits = reflect_internal_test_package.GCBits(New(typ).Interface());

    // Trim scalars at the end, as bits might end in zero,
    // e.g. with rep(2, lit(1, 0)).
    bits = trimBitmap(bits);

    if (bytes.HasPrefix(heapBits, bits)) {
        // Just the prefix matching is OK.
        //
        // The Go runtime's pointer/scalar iterator generates pointers beyond
        // the size of the type, up to the size of the size class. This space
        // is safe for the GC to scan since it's zero, and GCBits checks to
        // make sure that's true. But we need to handle the fact that the bitmap
        // may be larger than we expect.
        return;
    }
    var (_, _, line, _) = Δruntime.Caller(1);
    t.Errorf("line %d: heapBits incorrect for %v\nhave %v\nwant %v", line, typ, heapBits, bits);
}

internal static partial void verifyGCBitsSlice(ж<Δtesting.T> t, reflectꓸType typ, nint cap, slice<byte> bits) {
    // Creating a slice causes the runtime to repeat a bitmap,
    // which exercises a different path from making the compiler
    // repeat a bitmap for a small array or executing a repeat in
    // a GC program.
    var val = MakeSlice(typ, 0, cap);
    var data = NewAt(typ.Elem(), (uintptr)val.UnsafePointer());
    var heapBits = reflect_internal_test_package.GCBits(data.Interface());
    // Repeat the bitmap for the slice size, trimming scalars in
    // the last element.
    bits = trimBitmap(rep(cap, bits));
    if (bytes.Equal(heapBits, bits)) {
        return;
    }
    if (len(heapBits) > len(bits) && bytes.Equal(heapBits.slice(0, len(bits)), bits)) {
        // Just the prefix matching is OK.
        return;
    }
    var (_, _, line, _) = Δruntime.Caller(1);
    t.Errorf("line %d: heapBits incorrect for make(%v, 0, %v)\nhave %v\nwant %v", line, typ, cap, heapBits, bits);
}

// Building blocks for types seen by the compiler (like [2]Xscalar).
// The compiler will create the type structures for the derived types,
// including their GC metadata.
partial struct Xscalar {
    internal uintptr x;
}

partial struct Xptr {
    internal ж<byte> x;
}

partial struct Xptrscalar {
    /*embed*/ internal ж<byte> @byte;
    /*embed*/ internal uintptr uintptr;
}

partial struct Xscalarptr {
    /*embed*/ internal uintptr uintptr;
    /*embed*/ internal ж<byte> @byte;
}

partial struct Xbigptrscalar {
    internal array<ж<byte>> _ = new(100);
    internal array<uintptr> __ = new(100);
}

public static reflectꓸType Tscalar;
public static reflectꓸType Tint64;
public static reflectꓸType Tptr;
public static reflectꓸType Tscalarptr;
public static reflectꓸType Tptrscalar;
public static reflectꓸType Tbigptrscalar;

// Building blocks for types constructed by reflect.
// This code is in a separate block so that code below
// cannot accidentally refer to these.
// The compiler must NOT see types derived from these
// (for example, [2]Scalar must NOT appear in the program),
// or else reflect will use it instead of having to construct one.
// The goal is to test the construction.
internal partial struct init_Scalar /*dyn*/ {
    internal uintptr x;
}

internal partial struct init_Ptr /*dyn*/ {
    internal ж<byte> x;
}

internal partial struct init_Ptrscalar /*dyn*/ {
    /*embed*/ internal ж<byte> @byte;
    /*embed*/ internal uintptr uintptr;
}

internal partial struct init_Scalarptr /*dyn*/ {
    /*embed*/ internal uintptr uintptr;
    /*embed*/ internal ж<byte> @byte;
}

internal partial struct init_Bigptrscalar /*dyn*/ {
    internal array<ж<byte>> _ = new(100);
    internal array<uintptr> __ = new(100);
}

internal partial struct init_Int64 /*num:int64*/;

[GoInit] internal static void initΔ1() {
    Tscalar = TypeOf(new init_Scalar());
    Tint64 = TypeOf((init_Int64)0);
    Tptr = TypeOf(new init_Ptr());
    Tscalarptr = TypeOf(new init_Scalarptr());
    Tptrscalar = TypeOf(new init_Ptrscalar());
    Tbigptrscalar = TypeOf(new init_Bigptrscalar());
}

internal static slice<byte> empty = slice<byte>([]);

public static void TestGCBits(ж<Δtesting.T> t) {
    verifyGCBits(t, TypeOf((ж<byte>)nil), slice<byte>([1]));

    verifyGCBits(t, TypeOf(new Xscalar()), empty);
    verifyGCBits(t, Tscalar, empty);
    verifyGCBits(t, TypeOf(new Xptr()), lit(1));
    verifyGCBits(t, Tptr, lit(1));
    verifyGCBits(t, TypeOf(new Xscalarptr()), lit(0, 1));
    verifyGCBits(t, Tscalarptr, lit(0, 1));
    verifyGCBits(t, TypeOf(new Xptrscalar()), lit(1));
    verifyGCBits(t, Tptrscalar, lit(1));

    verifyGCBits(t, TypeOf(new Xptr[]{}.array()), empty);
    verifyGCBits(t, ArrayOf(0, Tptr), empty);
    verifyGCBits(t, TypeOf(new Xptrscalar[]{}.array(1)), lit(1));
    verifyGCBits(t, ArrayOf(1, Tptrscalar), lit(1));
    verifyGCBits(t, TypeOf(new Xscalar[]{}.array(2)), empty);
    verifyGCBits(t, ArrayOf(2, Tscalar), empty);
    verifyGCBits(t, TypeOf(new Xscalar[]{}.array(10000)), empty);
    verifyGCBits(t, ArrayOf(10000, Tscalar), empty);
    verifyGCBits(t, TypeOf(new Xptr[]{}.array(2)), lit(1, 1));
    verifyGCBits(t, ArrayOf(2, Tptr), lit(1, 1));
    verifyGCBits(t, TypeOf(new Xptr[]{}.array(10000)), rep(10000, lit(1)));
    verifyGCBits(t, ArrayOf(10000, Tptr), rep(10000, lit(1)));
    verifyGCBits(t, TypeOf(new Xscalarptr[]{}.array(2)), lit(0, 1, 0, 1));
    verifyGCBits(t, ArrayOf(2, Tscalarptr), lit(0, 1, 0, 1));
    verifyGCBits(t, TypeOf(new Xscalarptr[]{}.array(10000)), rep(10000, lit(0, 1)));
    verifyGCBits(t, ArrayOf(10000, Tscalarptr), rep(10000, lit(0, 1)));
    verifyGCBits(t, TypeOf(new Xptrscalar[]{}.array(2)), lit(1, 0, 1));
    verifyGCBits(t, ArrayOf(2, Tptrscalar), lit(1, 0, 1));
    verifyGCBits(t, TypeOf(new Xptrscalar[]{}.array(10000)), rep(10000, lit(1, 0)));
    verifyGCBits(t, ArrayOf(10000, Tptrscalar), rep(10000, lit(1, 0)));
    verifyGCBits(t, TypeOf(new array<Xptrscalar>[]{}.array(1, () => new(10000))), rep(10000, lit(1, 0)));
    verifyGCBits(t, ArrayOf(1, ArrayOf(10000, Tptrscalar)), rep(10000, lit(1, 0)));
    verifyGCBits(t, TypeOf(new array<Xptrscalar>[]{}.array(2, () => new(10000))), rep(2 * 10000, lit(1, 0)));
    verifyGCBits(t, ArrayOf(2, ArrayOf(10000, Tptrscalar)), rep(2 * 10000, lit(1, 0)));
    verifyGCBits(t, TypeOf(new Xbigptrscalar[]{}.array(4)), join(rep(3, join(rep(100, lit(1)), rep(100, lit(0)))), rep(100, lit(1))));
    verifyGCBits(t, ArrayOf(4, Tbigptrscalar), join(rep(3, join(rep(100, lit(1)), rep(100, lit(0)))), rep(100, lit(1))));

    verifyGCBitsSlice(t, TypeOf(slice<Xptr>([])), 0, empty);
    verifyGCBitsSlice(t, SliceOf(Tptr), 0, empty);
    verifyGCBitsSlice(t, TypeOf(slice<Xptrscalar>([])), 1, lit(1));
    verifyGCBitsSlice(t, SliceOf(Tptrscalar), 1, lit(1));
    verifyGCBitsSlice(t, TypeOf(slice<Xscalar>([])), 2, lit(0));
    verifyGCBitsSlice(t, SliceOf(Tscalar), 2, lit(0));
    verifyGCBitsSlice(t, TypeOf(slice<Xscalar>([])), 10000, lit(0));
    verifyGCBitsSlice(t, SliceOf(Tscalar), 10000, lit(0));
    verifyGCBitsSlice(t, TypeOf(slice<Xptr>([])), 2, lit(1));
    verifyGCBitsSlice(t, SliceOf(Tptr), 2, lit(1));
    verifyGCBitsSlice(t, TypeOf(slice<Xptr>([])), 10000, lit(1));
    verifyGCBitsSlice(t, SliceOf(Tptr), 10000, lit(1));
    verifyGCBitsSlice(t, TypeOf(slice<Xscalarptr>([])), 2, lit(0, 1));
    verifyGCBitsSlice(t, SliceOf(Tscalarptr), 2, lit(0, 1));
    verifyGCBitsSlice(t, TypeOf(slice<Xscalarptr>([])), 10000, lit(0, 1));
    verifyGCBitsSlice(t, SliceOf(Tscalarptr), 10000, lit(0, 1));
    verifyGCBitsSlice(t, TypeOf(slice<Xptrscalar>([])), 2, lit(1, 0));
    verifyGCBitsSlice(t, SliceOf(Tptrscalar), 2, lit(1, 0));
    verifyGCBitsSlice(t, TypeOf(slice<Xptrscalar>([])), 10000, lit(1, 0));
    verifyGCBitsSlice(t, SliceOf(Tptrscalar), 10000, lit(1, 0));
    verifyGCBitsSlice(t, TypeOf(GoReflect.WithElemDims(new array<Xptrscalar>[]{}.slice(), 10000)), 1, rep(10000, lit(1, 0)));
    verifyGCBitsSlice(t, SliceOf(ArrayOf(10000, Tptrscalar)), 1, rep(10000, lit(1, 0)));
    verifyGCBitsSlice(t, TypeOf(GoReflect.WithElemDims(new array<Xptrscalar>[]{}.slice(), 10000)), 2, rep(10000, lit(1, 0)));
    verifyGCBitsSlice(t, SliceOf(ArrayOf(10000, Tptrscalar)), 2, rep(10000, lit(1, 0)));
    verifyGCBitsSlice(t, TypeOf(slice<Xbigptrscalar>([])), 4, join(rep(100, lit(1)), rep(100, lit(0))));
    verifyGCBitsSlice(t, SliceOf(Tbigptrscalar), 4, join(rep(100, lit(1)), rep(100, lit(0))));

    verifyGCBits(t, TypeOf(channel<array<Xscalar>>.Nil(ChanCargo.Of(null, new nint[] { 100 }))), lit(1));
    verifyGCBits(t, ChanOf(BothDir, ArrayOf(100, Tscalar)), lit(1));

    verifyGCBits(t, TypeOf(((Action<array<Xscalarptr>>)(default)).OrTypedNilFunc()), lit(1));
    verifyGCBits(t, FuncOf(slice<reflectꓸType>([ArrayOf(10000, Tscalarptr)]), default, false), lit(1));

    verifyGCBits(t, TypeOf((map<array<Xscalarptr>, Xscalar>)default), lit(1));
    verifyGCBits(t, MapOf(ArrayOf(10000, Tscalarptr), Tscalar), lit(1));

    verifyGCBits(t, TypeOf(ж<array<Xscalar>>.NilBoxOfDims(10000L)), lit(1));
    verifyGCBits(t, PointerTo(ArrayOf(10000, Tscalar)), lit(1));

    verifyGCBits(t, TypeOf((slice<array<Xscalar>>)(default)), lit(1));
    verifyGCBits(t, SliceOf(ArrayOf(10000, Tscalar)), lit(1));

    // For maps, we don't manually construct GC data, instead using the
    // public reflect API in groupAndSlotOf.
}

internal static slice<byte> rep(nint n, slice<byte> b) {
    return bytes.Repeat(b, n);
}

internal static slice<byte> join(params Span<slice<byte>> bʗp) {
    var b = bʗp.slice();

    return bytes.Join(b, default);
}

internal static slice<byte> lit(params ꓸꓸꓸbyte xʗp) {
    var x = xʗp.slice();

    return x;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string typeOfˢ => ref typeOfˢᵇ;
private static readonly @string typeOfˢᵇ = "TypeOf"u8;
internal static ref readonly @string arrayOfˢ => ref arrayOfˢᵇ;
private static readonly @string arrayOfˢᵇ = "ArrayOf"u8;
internal static ref readonly @string chanOfˢ => ref chanOfˢᵇ;
private static readonly @string chanOfˢᵇ = "ChanOf"u8;
internal static ref readonly @string funcOfˢ => ref funcOfˢᵇ;
private static readonly @string funcOfˢᵇ = "FuncOf"u8;
internal static ref readonly @string mapOfˢ => ref mapOfˢᵇ;
private static readonly @string mapOfˢᵇ = "MapOf"u8;
internal static ref readonly @string ptrToˢ => ref ptrToˢᵇ;
private static readonly @string ptrToˢᵇ = "PtrTo"u8;
internal static ref readonly @string sliceOfˢ => ref sliceOfˢᵇ;
private static readonly @string sliceOfˢᵇ = "SliceOf"u8;

internal partial struct TestTypeOfTypeOf_T /*dyn*/ {
    /*embed*/ internal nint @int;
}

public static void TestTypeOfTypeOf(ж<Δtesting.T> t) {
    // Check that all the type constructors return concrete *rtype implementations.
    // It's difficult to test directly because the reflect package is only at arm's length.
    // The easiest thing to do is just call a function that crashes if it doesn't get an *rtype.
    void check(@string name, reflectꓸType typ) {
        {
            @string underlying = TypeOf(typ).String(); if (underlying != "*reflect.rtype"u8) {
                t.Errorf("%v returned %v, not *reflect.rtype", name, underlying);
            }
        }
    }
    check(typeOfˢ, TypeOf(new TestTypeOfTypeOf_T()));

    check(arrayOfˢ, ArrayOf(10, TypeOf(new TestTypeOfTypeOf_T())));
    check(chanOfˢ, ChanOf(BothDir, TypeOf(new TestTypeOfTypeOf_T())));
    check(funcOfˢ, FuncOf(slice<reflectꓸType>([TypeOf(new TestTypeOfTypeOf_T())]), default, false));
    check(mapOfˢ, MapOf(TypeOf(new TestTypeOfTypeOf_T()), TypeOf(new TestTypeOfTypeOf_T())));
    check(ptrToˢ, PointerTo(TypeOf(new TestTypeOfTypeOf_T())));
    check(sliceOfˢ, SliceOf(TypeOf(new TestTypeOfTypeOf_T())));
}

partial struct XM {
    internal bool _;
}

public static @string String(this ref XM _) {
    return ""u8;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object doesNotImplementStringerˢ = (@string)"does not implement Stringer, but should"u8;

internal partial struct TestPtrToMethods_y /*dyn*/ {
    /*embed*/ public XM XM;
}

public static void TestPtrToMethods(ж<Δtesting.T> t) {
    TestPtrToMethods_y y = new(nil);
    var yp = New(TypeOf(y)).Interface();
    var (_, ok) = yp._<fmt.Stringer>(ᐧ);
    if (!ok) {
        t.Fatal(doesNotImplementStringerˢ);
    }
}

public static void TestMapAlloc(ж<Δtesting.T> t) {
    if (asan.Enabled) {
        t.Skip(testAllocatesMoreWithˢ);
    }
    ref var m = ref heap<reflectꓸValue>(ValueOf(new map<nint, nint>(10)), out var Ꮡm);
    ref var k = ref heap<reflectꓸValue>(ValueOf((nint)5), out var Ꮡk);
    ref var v = ref heap<reflectꓸValue>(ValueOf((nint)7), out var Ꮡv);
    var kʗ1 = k;
    var mʗ1 = m;
    var vʗ1 = v;
    var allocs = Δtesting.AllocsPerRun(100, () => {
        mʗ1.SetMapIndex(kʗ1, vʗ1);
    });
    if (allocs > 0.5D) {
        t.Errorf("allocs per map assignment: want 0 got %f", allocs);
    }

    UntypedInt size = 1000;
    ref var tmp = ref heap<nint>(0, out var Ꮡtmp);
    ref var val = ref heap<reflectꓸValue>(ValueOf(Ꮡtmp).Elem(), out var Ꮡval);
    var valʗ1 = val;
    allocs = Δtesting.AllocsPerRun(100, () => {
        var mv = MakeMapWithSize(TypeOf(new map<nint, nint>{}), size);
        // Only adding half of the capacity to not trigger re-allocations due too many overloaded buckets.
        for (nint i = 0; i < (nint)(size / 2); i++) {
            valʗ1.SetInt((int64)i);
            mv.SetMapIndex(valʗ1, valʗ1);
        }
    });
    if (allocs > 10D) {
        t.Errorf("allocs per map assignment: want at most 10 got %f", allocs);
    }
    // Empirical testing shows that with capacity hint single run will trigger 3 allocations and without 91. I set
    // the threshold to 10, to not make it overly brittle if something changes in the initial allocation of the
    // map, but to still catch a regression where we keep re-allocating in the hashmap as new entries are added.
}

public static void TestChanAlloc(ж<Δtesting.T> t) {
    if (asan.Enabled) {
        t.Skip(testAllocatesMoreWithˢ);
    }
    // Note: for a chan int, the return Value must be allocated, so we
    // use a chan *int instead.
    ref var c = ref heap<reflectꓸValue>(ValueOf(new channel<ж<nint>>(1)), out var Ꮡc);
    ref var v = ref heap<reflectꓸValue>(ValueOf(@new<nint>()), out var Ꮡv);
    var cʗ1 = c;
    var vʗ1 = v;
    var allocs = Δtesting.AllocsPerRun(100, () => {
        cʗ1.Send(vʗ1);
        (_, _) = cʗ1.Recv();
    });
    if (allocs < 0.5D || allocs > 1.5D) {
        t.Errorf("allocs per chan send/recv: want 1 got %f", allocs);
    }
    // Note: there is one allocation in reflect.recv which seems to be
    // a limitation of escape analysis. If that is ever fixed the
    // allocs < 0.5 condition will trigger and this test should be fixed.
}

partial struct TheNameOfThisTypeIsExactly255BytesLongSoWhenTheCompilerPrependsTheReflectTestPackageNameAndExtraStarTheLinkerRuntimeAndReflectPackagesWillHaveToCorrectlyDecodeTheSecondLengthByte0123456789_0123456789_0123456789_0123456789_0123456789_012345678 /*num:nint*/;

partial struct nameTest {
    internal any v;
    internal @string want;
}


        partial interface Δtypeᴛ35 /*dyn*/ {
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
    new((ж<Δtypeᴛ35>)nil, ""),
    new((ж<TheNameOfThisTypeIsExactly255BytesLongSoWhenTheCompilerPrependsTheReflectTestPackageNameAndExtraStarTheLinkerRuntimeAndReflectPackagesWillHaveToCorrectlyDecodeTheSecondLengthByte0123456789_0123456789_0123456789_0123456789_0123456789_012345678>)nil, "TheNameOfThisTypeIsExactly255BytesLongSoWhenTheCompilerPrependsTheReflectTestPackageNameAndExtraStarTheLinkerRuntimeAndReflectPackagesWillHaveToCorrectlyDecodeTheSecondLengthByte0123456789_0123456789_0123456789_0123456789_0123456789_012345678")
]);

public static void TestNames(ж<Δtesting.T> t) {
    foreach (var (_, test) in nameTests) {
        var typ = TypeOf(test.v).Elem();
        {
            @string got = typ.Name(); if (got != test.want) {
                t.Errorf("%v Name()=%q, want %q", typ, got, test.want);
            }
        }
    }
}

internal partial struct TestExported_ΦExported /*dyn*/ {
}

internal partial struct TestExported_φUnexported /*dyn*/ {
}

internal partial class TestExported_BigP /*ж<big>*/;

internal partial struct TestExported_P /*num:nint*/;

internal partial class TestExported_p /*ж<TestExported_P>*/;

internal partial struct TestExported_P2 /*TestExported_p*/;

internal partial struct TestExported_p3 /*TestExported_p*/;

internal partial struct TestExported_exportTest /*dyn*/ {
    internal any v;
    internal bool want;
}

public static void TestExported(ж<Δtesting.T> t) {
    var exportTests = slice<TestExported_exportTest>([
        new(new D1(), true),
        new((ж<D1>)nil, true),
        new(new big(), false),
        new((ж<big>)nil, false),
        new((TestExported_BigP)nil, true),
        new((ж<TestExported_BigP>)nil, true),
        new(new TestExported_ΦExported(), true),
        new(new TestExported_φUnexported(), false),
        new((TestExported_P)0, true),
        new((TestExported_p)nil, false),
        new((TestExported_P2)nil, true),
        new((TestExported_p3)nil, false)
    ]);

    foreach (var (i, test) in exportTests) {
        var typ = TypeOf(test.v);
        {
            var got = reflect_internal_test_package.IsExported(typ); if (got != test.want) {
                t.Errorf("%d: %s exported=%v, want %v", i, typ.Name(), got, test.want);
            }
        }
    }
}

internal partial struct TestTypeStrings_stringTest /*dyn*/ {
    internal reflectꓸType typ;
    internal @string want;
}

public static void TestTypeStrings(ж<Δtesting.T> t) {
    var stringTests = slice<TestTypeStrings_stringTest>([
        new(TypeOf((nint _) => {
        }), "func(int)"),
        new(FuncOf(slice<reflectꓸType>([TypeOf((nint)0)]), default, false), "func(int)"),
        new(TypeOf(new XM()), "reflect_test.XM"),
        new(TypeOf(@new<XM>()), "*reflect_test.XM"),
        new(TypeOf(@new<XM>().String), "func() string"),
        new(TypeOf(@new<XM>()).Method(0).Type, "func(*reflect_test.XM) string"),
        new(ChanOf(3, TypeOf(new XM())), "chan reflect_test.XM"),
        new(MapOf(TypeOf((nint)0), TypeOf(new XM())), "map[int]reflect_test.XM"),
        new(ArrayOf(3, TypeOf(new XM())), "[3]reflect_test.XM"),
        new(ArrayOf(3, TypeOf(new EmptyStruct())), "[3]struct {}")
    ]);

    foreach (var (i, test) in stringTests) {
        {
            @string got = test.typ.String();
            @string want = test.want; if (got != want) {
                t.Errorf("type %d String()=%q, want %q", i, got, want);
            }
        }
    }
}

public static partial void TestOffsetLock(ж<Δtesting.T> t) {
    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    for (nint iᴛ1 = 0; iᴛ1 < 4; iᴛ1++) {
        var i = iᴛ1;
        Ꮡwg.Add(1);
        goǃ(() => {
            for (nint j = 0; j < 50; j++) {
                reflect_internal_test_package.ResolveReflectName(fmt.Sprintf("OffsetLockName:%d:%d"u8, i, j));
            }
            Ꮡwg.Done();
        });
    }
    Ꮡwg.Wait();
}

internal partial struct TestSwapper_I /*num:nint*/;

internal partial struct TestSwapper_pair /*dyn*/ {
    internal nint x, y;
}

internal partial struct TestSwapper_pairPtr /*dyn*/ {
    internal nint x, y;
    internal ж<TestSwapper_I> p;
}

internal partial struct TestSwapper_S /*@string*/;

internal partial struct TestSwapper_tests /*dyn*/ {
    internal any @in;
    internal nint i, j;
    internal any want;
}

public static void TestSwapper(ж<Δtesting.T> t) {
    ref var a = ref heap(new TestSwapper_I(), out var Ꮡa);
    ref var b = ref heap(new TestSwapper_I(), out var Ꮡb);
    ref var c = ref heap(new TestSwapper_I(), out var Ꮡc);

    var tests = slice<TestSwapper_tests>([
        new(
            @in: slice<nint>([1, 20, 300]),
            i: 0,
            j: 2,
            want: slice<nint>([300, 20, 1])
        ),
        new(
            @in: slice<uintptr>([1, 20, 300]),
            i: 0,
            j: 2,
            want: slice<uintptr>([300, 20, 1])
        ),
        new(
            @in: slice<int16>([1, 20, 300]),
            i: 0,
            j: 2,
            want: slice<int16>([300, 20, 1])
        ),
        new(
            @in: slice<int8>([1, 20, 100]),
            i: 0,
            j: 2,
            want: slice<int8>([100, 20, 1])
        ),
        new(
            @in: slice<ж<TestSwapper_I>>([Ꮡa, Ꮡb, Ꮡc]),
            i: 0,
            j: 2,
            want: slice<ж<TestSwapper_I>>([Ꮡc, Ꮡb, Ꮡa])
        ),
        new(
            @in: slice<@string>(["eric", "sergey", "larry"]),
            i: 0,
            j: 2,
            want: slice<@string>(["larry", "sergey", "eric"])
        ),
        new(
            @in: slice<TestSwapper_S>([(@string)"eric", (@string)"sergey", (@string)"larry"]),
            i: 0,
            j: 2,
            want: slice<TestSwapper_S>([(@string)"larry", (@string)"sergey", (@string)"eric"])
        ),
        new(
            @in: slice<TestSwapper_pair>([new(1, 2), new(3, 4), new(5, 6)]),
            i: 0,
            j: 2,
            want: slice<TestSwapper_pair>([new(5, 6), new(3, 4), new(1, 2)])
        ),
        new(
            @in: slice<TestSwapper_pairPtr>([new(1, 2, Ꮡa), new(3, 4, Ꮡb), new(5, 6, Ꮡc)]),
            i: 0,
            j: 2,
            want: slice<TestSwapper_pairPtr>([new(5, 6, Ꮡc), new(3, 4, Ꮡb), new(1, 2, Ꮡa)])
        )
    ]);

    foreach (var (i, tt) in tests) {
        @string inStr = fmt.Sprint(tt.@in);
        Swapper(tt.@in)(tt.i, tt.j);
        if (!DeepEqual(tt.@in, tt.want)) {
            t.Errorf("%d. swapping %v and %v of %v = %v; want %v", i, tt.i, tt.j, inStr, tt.@in, tt.want);
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
public static void TestUnaddressableField(ж<Δtesting.T> t) {
    global::go.reflect_internal_test_package.Buffer b = default; // type defined in reflect, a different package
    ref var localBuffer = ref heap(new TestUnaddressableField_localBuffer(), out var ᏑlocalBuffer);
    ref var lv = ref heap<reflectꓸValue>(ValueOf(ᏑlocalBuffer).Elem(), out var Ꮡlv);
    ref var rv = ref heap<reflectꓸValue>(ValueOf(b), out var Ꮡrv);
    var lvʗ1 = lv;
    var rvʗ1 = rv;
    shouldPanic(setˢ, () => {
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
internal static ref readonly @string reflectTestTalias1Byteˢ => ref reflectTestTalias1Byteˢᵇ;
private static readonly @string reflectTestTalias1Byteˢᵇ = "reflect_test.Talias1{byte:0x1, uint8:0x2, int:3, int32:4, rune:5}"u8;
internal static ref readonly @string reflectTestTalias2Tint1ˢ => ref reflectTestTalias2Tint1ˢᵇ;
private static readonly @string reflectTestTalias2Tint1ˢᵇ = "reflect_test.Talias2{Tint:1, Tint2:2}"u8;

public static void TestAliasNames(ж<Δtesting.T> t) {
    var t1 = new Talias1(@byte: 1, uint8: 2, @int: 3, int32: 4, rune: 5);
    @string @out = fmt.Sprintf("%#v"u8, t1);
    @string want = reflectTestTalias1Byteˢ;
    if (@out != want) {
        t.Errorf("Talias1 print:\nhave: %s\nwant: %s", @out, want);
    }

    var t2 = new Talias2(Tint: 1, Tint2: 2);
    @out = fmt.Sprintf("%#v"u8, t2);
    want = reflectTestTalias2Tint1ˢ;
    if (@out != want) {
        t.Errorf("Talias2 print:\nhave: %s\nwant: %s", @out, want);
    }
}

internal partial struct TestIssue22031_s /*dyn*/ {
    public nint C;
}

internal partial struct TestIssue22031_sᴛ1 /*[]TestIssue22031_s*/;

internal partial struct TestIssue22031_t1 /*dyn*/ {
    /*embed*/ internal TestIssue22031_sᴛ1 s;
}

internal partial struct TestIssue22031_t2 /*dyn*/ {
    internal TestIssue22031_sᴛ1 f;
}

public static void TestIssue22031(ж<Δtesting.T> t) {
    var tests = slice<reflectꓸValue>([
        ValueOf(new TestIssue22031_t1(new TestIssue22031_sᴛ1(new TestIssue22031_s[]{new()}.slice()))).Field(0).Index(0).Field(0),
        ValueOf(new TestIssue22031_t2(new TestIssue22031_sᴛ1(new TestIssue22031_s[]{new()}.slice()))).Field(0).Index(0).Field(0)
    ]);

    foreach (var (i, test) in tests) {
        if (test.CanSet()) {
            t.Errorf("%d: CanSet: got true, want false", i);
        }
    }
}

partial struct NonExportedFirst /*num:nint*/;

public static void ΦExported(this NonExportedFirst i) {
}

internal static nint nonexported(this NonExportedFirst i) {
    throw panic("wrong");
}

public static void TestIssue22073(ж<Δtesting.T> t) {
    var m = ValueOf((NonExportedFirst)0).Method(0);

    {
        nint got = m.Type().NumOut(); if (got != 0) {
            t.Errorf("NumOut: got %v, want 0", got);
        }
    }

    // Shouldn't panic.
    m.Call(default);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string one1Three3Two2ˢ => ref one1Three3Two2ˢᵇ;
private static readonly @string one1Three3Two2ˢᵇ = @"[one: 1, three: 3, two: 2]"u8;

public static void TestMapIterNonEmptyMap(ж<Δtesting.T> t) {
    var m = new map<@string, nint>{["one"] = 1, ["two"] = 2, ["three"] = 3};
    var iter = ValueOf(m).MapRange();
    {
        @string got = iterateToString(iter);
        @string want = one1Three3Two2ˢ; if (got != want) {
            t.Errorf("iterator returned %s (after sorting), want %s", got, want);
        }
    }
}

public static void TestMapIterNilMap(ж<Δtesting.T> t) {
    map<@string, nint> m = default;
    var iter = ValueOf(m).MapRange();
    {
        @string got = iterateToString(iter);
        @string want = @"[]"; if (got != want) {
            t.Errorf("non-empty result iteratoring nil map: %s", got);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object nextDidNotPanicˢ = (@string)"Next did not panic"u8;
internal static ref readonly @string one2Two3Threeˢ => ref one2Two3Threeˢᵇ;
private static readonly @string one2Two3Threeˢᵇ = @"[1: one, 2: two, 3: three]"u8;

public static void TestMapIterReset(ж<Δtesting.T> t) {
    var iter = @new<Δreflect.MapIter>();

    // Use of zero iterator should panic.
    var iterʗ1 = iter;
    func(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                recover();
            }, ref ᒐ);
            iterʗ1.Next();
            t.Error(nextDidNotPanicˢ);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    })();

    // Reset to new Map should work.
    var m = new map<@string, nint>{["one"] = 1, ["two"] = 2, ["three"] = 3};
    iter.Reset(ValueOf(m));
    {
        @string got = iterateToString(iter);
        @string want = one1Three3Two2ˢ; if (got != want) {
            t.Errorf("iterator returned %s (after sorting), want %s", got, want);
        }
    }

    // Reset to Zero value should work, but iterating over it should panic.
    iter.Reset(new reflectꓸValue());
    var iterʗ2 = iter;
    func(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                recover();
            }, ref ᒐ);
            iterʗ2.Next();
            t.Error(nextDidNotPanicˢ);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    })();

    // Reset to a different Map with different types should work.
    var m2 = new map<nint, @string>{[1] = "one", [2] = "two", [3] = "three"};
    iter.Reset(ValueOf(m2));
    {
        @string got = iterateToString(iter);
        @string want = one2Two3Threeˢ; if (got != want) {
            t.Errorf("iterator returned %s (after sorting), want %s", got, want);
        }
    }

    // Check that Reset, Next, and SetKey/SetValue play nicely together.
    var m3 = new map<uint64, uint64>{
        [((uint64)1 << 0)] = ((uint64)1 << 1),
        [((uint64)1 << 1)] = ((uint64)1 << 2),
        [((uint64)1 << 2)] = ((uint64)1 << 3)
    };
    var kv = New(TypeOf((uint64)0)).Elem();
    for (nint i = 0; i < 5; i++) {
        uint64 seenk = default;
        uint64 seenv = default;
        iter.Reset(ValueOf(m3));
        while (iter.Next()) {
            kv.SetIterKey(iter);
            seenk ^= (uint64)(kv.Uint());
            kv.SetIterValue(iter);
            seenv ^= (uint64)(kv.Uint());
        }
        if (seenk != 0b111) {
            t.Errorf("iteration yielded keys %b, want %b", seenk, (nint)0b111);
        }
        if (seenv != 0b1110) {
            t.Errorf("iteration yielded values %b, want %b", seenv, (nint)0b1110);
        }
    }

    // Reset should not allocate.
    //
    // Except with -asan, where there are additional allocations.
    // See #70079.
    var iterʗ3 = iter;
    var m2ʗ1 = m2;
    nint n = (nint)Δtesting.AllocsPerRun(10, () => {
        iterʗ3.Reset(ValueOf(m2ʗ1));
        iterʗ3.Reset(new reflectꓸValue());
    });
    if (!asan.Enabled && n > 0) {
        t.Errorf("MapIter.Reset allocated %d times", n);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object keyDidNotPanicˢ = (@string)"Key did not panic"u8;
internal static readonly object valueDidNotPanicˢ = (@string)"Value did not panic"u8;

public static void TestMapIterSafety(ж<Δtesting.T> t) {
    // Using a zero MapIter causes a panic, but not a crash.
    func(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                recover();
            }, ref ᒐ);
            @new<Δreflect.MapIter>().Key();
            t.Fatal(keyDidNotPanicˢ);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    })();
    func(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                recover();
            }, ref ᒐ);
            @new<Δreflect.MapIter>().Value();
            t.Fatal(valueDidNotPanicˢ);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    })();
    func(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                recover();
            }, ref ᒐ);
            @new<Δreflect.MapIter>().Next();
            t.Fatal(nextDidNotPanicˢ);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    })();

    // Calling Key/Value on a MapIter before Next
    // causes a panic, but not a crash.
    map<@string, nint> m = default;
    var iter = ValueOf(m).MapRange();

    var iterʗ1 = iter;
    func(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                recover();
            }, ref ᒐ);
            iterʗ1.Key();
            t.Fatal(keyDidNotPanicˢ);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    })();
    var iterʗ2 = iter;
    func(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                recover();
            }, ref ᒐ);
            iterʗ2.Value();
            t.Fatal(valueDidNotPanicˢ);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    })();

    // Calling Next, Key, or Value on an exhausted iterator
    // causes a panic, but not a crash.
    iter.Next(); // -> false
    var iterʗ3 = iter;
    func(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                recover();
            }, ref ᒐ);
            iterʗ3.Key();
            t.Fatal(keyDidNotPanicˢ);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    })();
    var iterʗ4 = iter;
    func(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                recover();
            }, ref ᒐ);
            iterʗ4.Value();
            t.Fatal(valueDidNotPanicˢ);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    })();
    var iterʗ5 = iter;
    func(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                recover();
            }, ref ᒐ);
            iterʗ5.Next();
            t.Fatal(nextDidNotPanicˢ);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    })();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string oneˢ => ref oneˢᵇ;
private static readonly @string oneˢᵇ = "one"u8;
internal static ref readonly @string one1ˢ => ref one1ˢᵇ;
private static readonly @string one1ˢᵇ = @"[one: 1]"u8;

public static void TestMapIterNext(ж<Δtesting.T> t) {
    // The first call to Next should reflect any
    // insertions to the map since the iterator was created.
    var m = new map<@string, nint>{};
    var iter = ValueOf(m).MapRange();
    m[oneˢ] = 1;
    {
        @string got = iterateToString(iter);
        @string want = one1ˢ; if (got != want) {
            t.Errorf("iterator returned deleted elements: got %s, want %s", got, want);
        }
    }
}

public static void TestMapIterDelete0(ж<Δtesting.T> t) {
    // Delete all elements before first iteration.
    var m = new map<@string, nint>{["one"] = 1, ["two"] = 2, ["three"] = 3};
    var iter = ValueOf(m).MapRange();
    delete(m, "one"u8);
    delete(m, "two"u8);
    delete(m, "three"u8);
    {
        @string got = iterateToString(iter);
        @string want = @"[]"; if (got != want) {
            t.Errorf("iterator returned deleted elements: got %s, want %s", got, want);
        }
    }
}

public static void TestMapIterDelete1(ж<Δtesting.T> t) {
    var gotᴷ = new stackAppend<@string>();

    // Delete all elements after first iteration.
    var m = new map<@string, nint>{["one"] = 1, ["two"] = 2, ["three"] = 3};
    var iter = ValueOf(m).MapRange();
    slice<@string> got = default;
    while (iter.Next()) {
        got = append(gotᴷ, got, fmt.Sprint(iter.Key(), iter.Value()));
        delete(m, "one"u8);
        delete(m, "two"u8);
        delete(m, "three"u8);
    }
    if (len(got) != 1) {
        t.Errorf("iterator returned wrong number of elements: got %d, want 1", len(got));
    }
}

// iterateToString returns the set of elements
// returned by an iterator in readable form.
internal static @string iterateToString(ж<Δreflect.MapIter> it) {
    var gotᴷ = new stackAppend<@string>();

    slice<@string> got = default;
    while (it.Next()) {
        @string line = fmt.Sprintf("%v: %v"u8, it.Key(), it.Value());
        got = append(gotᴷ, got, line);
    }
    slices.Sort<slice<@string>, @string>(got);
    return "["u8 + strings.Join(got, ", ") + "]"u8;
}

public static void TestConvertibleTo(ж<Δtesting.T> t) {
    var t1 = ValueOf(new example1.MyStruct()).Type();
    var t2 = ValueOf(new example2.MyStruct()).Type();

    // Shouldn't raise stack overflow
    if (t1.ConvertibleTo(t2)) {
        t.Fatalf("(%s).ConvertibleTo(%s) = true, want false", t1, t2);
    }

    var t3 = ValueOf(slice<example1.MyStruct>([])).Type();
    var t4 = ValueOf(slice<example2.MyStruct>([])).Type();

    if (t3.ConvertibleTo(t4)) {
        t.Fatalf("(%s).ConvertibleTo(%s) = true, want false", t3, t4);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string valueSetIterKeyCalledˢ => ref valueSetIterKeyCalledˢᵇ;
private static readonly @string valueSetIterKeyCalledˢᵇ = "Value.SetIterKey called before Next"u8;
internal static ref readonly @string valueSetIterValueCalledˢ => ref valueSetIterValueCalledˢᵇ;
private static readonly @string valueSetIterValueCalledˢᵇ = "Value.SetIterValue called before Next"u8;
internal static ref readonly @string valueSetIterKeyCalledOnˢ => ref valueSetIterKeyCalledOnˢᵇ;
private static readonly @string valueSetIterKeyCalledOnˢᵇ = "Value.SetIterKey called on exhausted iterator"u8;
internal static ref readonly @string valueSetIterValueCalledˢ2 => ref valueSetIterValueCalledˢ2ᵇ;
private static readonly @string valueSetIterValueCalledˢ2ᵇ = "Value.SetIterValue called on exhausted iterator"u8;
internal static ref readonly @string valueSetIterKeyUsingˢ => ref valueSetIterKeyUsingˢᵇ;
private static readonly @string valueSetIterKeyUsingˢᵇ = "Value.SetIterKey using unaddressable value"u8;
internal static ref readonly @string valueSetIterValueUsingˢ => ref valueSetIterValueUsingˢᵇ;
private static readonly @string valueSetIterValueUsingˢᵇ = "Value.SetIterValue using unaddressable value"u8;
internal static ref readonly @string valueOfTypeStringIsNotˢ2 => ref valueOfTypeStringIsNotˢ2ᵇ;
private static readonly @string valueOfTypeStringIsNotˢ2ᵇ = "value of type string is not assignable to type int"u8;
internal static ref readonly @string valueOfTypeIntIsNotˢ => ref valueOfTypeIntIsNotˢᵇ;
private static readonly @string valueOfTypeIntIsNotˢᵇ = "value of type int is not assignable to type string"u8;
internal static ref readonly @string usingValueObtainedUsingˢ => ref usingValueObtainedUsingˢᵇ;
private static readonly @string usingValueObtainedUsingˢᵇ = "using value obtained using unexported field"u8;

internal partial struct TestSetIter_i /*dyn*/ {
    internal map<@string, nint> m;
}

public static void TestSetIter(ж<Δtesting.T> t) {
    var data = new map<@string, nint>{
        ["foo"] = 1,
        ["bar"] = 2,
        ["baz"] = 3
    };

    var m = ValueOf(data);
    ref var i = ref heap<ж<Δreflect.MapIter>>(m.MapRange(), out var Ꮡi);
    ref var k = ref heap<reflectꓸValue>(New(TypeOf((@string)""u8)).Elem(), out var Ꮡk);
    ref var v = ref heap<reflectꓸValue>(New(TypeOf((nint)0)).Elem(), out var Ꮡv);
    var kʗ1 = k;
    shouldPanic(valueSetIterKeyCalledˢ, () => {
        kʗ1.SetIterKey(Ꮡi.ValueSlot);
    });
    var vʗ1 = v;
    shouldPanic(valueSetIterValueCalledˢ, () => {
        vʗ1.SetIterValue(Ꮡi.ValueSlot);
    });
    var data2 = new map<@string, nint>{};
    while (i.Next()) {
        k.SetIterKey(i);
        v.SetIterValue(i);
        data2[k.Interface()._<@string>()] = v.Interface()._<nint>();
    }
    if (!DeepEqual(data, data2)) {
        t.Errorf("maps not equal, got %v want %v", data2, data);
    }
    var kʗ2 = k;
    shouldPanic(valueSetIterKeyCalledOnˢ, () => {
        kʗ2.SetIterKey(Ꮡi.ValueSlot);
    });
    var vʗ2 = v;
    shouldPanic(valueSetIterValueCalledˢ2, () => {
        vʗ2.SetIterValue(Ꮡi.ValueSlot);
    });

    i.Reset(m);
    i.Next();
    shouldPanic(valueSetIterKeyUsingˢ, () => {
        ValueOf((@string)""u8).SetIterKey(Ꮡi.ValueSlot);
    });
    shouldPanic(valueSetIterValueUsingˢ, () => {
        ValueOf((nint)0).SetIterValue(Ꮡi.ValueSlot);
    });
    shouldPanic(valueOfTypeStringIsNotˢ2, () => {
        New(TypeOf((nint)0)).Elem().SetIterKey(Ꮡi.ValueSlot);
    });
    shouldPanic(valueOfTypeIntIsNotˢ, () => {
        New(TypeOf((@string)""u8)).Elem().SetIterValue(Ꮡi.ValueSlot);
    });

    // Make sure assignment conversion works.
    ref var x = ref heap<any>(out var Ꮡx);
    var y = ValueOf(Ꮡx).Elem();
    y.SetIterKey(i);
    {
        var (_, ok) = data[x._<@string>(), ꟷ]; if (!ok) {
            t.Errorf("got key %s which is not in map", x);
        }
    }
    y.SetIterValue(i);
    if (x._<nint>() < 1 || x._<nint>() > 3) {
        t.Errorf("got value %d which is not in map", x);
    }

    // Try some key/value types which are direct interfaces.
    ref var a = ref heap<nint>(88, out var Ꮡa);
    ref var b = ref heap<nint>(99, out var Ꮡb);
    var pp = new map<ж<nint>, ж<nint>>{
        [Ꮡa] = Ꮡb
    };
    i = ValueOf(pp).MapRange();
    i.Next();
    y.SetIterKey(i);
    {
        nint got = y.Interface()._<ж<nint>>().Value; if (got != a) {
            t.Errorf("pointer incorrect: got %d want %d", got, a);
        }
    }
    y.SetIterValue(i);
    {
        nint got = y.Interface()._<ж<nint>>().Value; if (got != b) {
            t.Errorf("pointer incorrect: got %d want %d", got, b);
        }
    }

    // Make sure we panic assigning from an unexported field.
    m = ValueOf(new TestSetIter_i(data)).Field(0);
    for (var iterᴛ1 = m.MapRange(); iterᴛ1.Next(); ) {
        var iter = iterᴛ1;
        var iterʗ1 = iter;
        var kʗ3 = k;
        shouldPanic(usingValueObtainedUsingˢ, () => {
            kʗ3.SetIterKey(iterʗ1);
        });
        var iterʗ2 = iter;
        var vʗ3 = v;
        shouldPanic(usingValueObtainedUsingˢ, () => {
            vʗ3.SetIterValue(iterʗ2);
        });
    }
}

public static void TestMethodCallValueCodePtr(ж<Δtesting.T> t) {
    var m = ValueOf(new Point()).Method(1);
    var want = reflect_internal_test_package.MethodValueCallCodePtr();
    {
        var got = (uintptr)(uintptr)m.UnsafePointer(); if (got != want) {
            t.Errorf("methodValueCall code pointer mismatched, want: %v, got: %v", want, got);
        }
    }
    {
        var got = m.Pointer(); if (got != want) {
            t.Errorf("methodValueCall code pointer mismatched, want: %v, got: %v", want, got);
        }
    }
}

partial struct A {
}

partial struct B<T> {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string bReflectTestAˢ => ref bReflectTestAˢᵇ;
private static readonly @string bReflectTestAˢᵇ = "B[reflect_test.A]"u8;
internal static ref readonly @string bReflectTestBReflectTestˢ => ref bReflectTestBReflectTestˢᵇ;
private static readonly @string bReflectTestBReflectTestˢᵇ = "B[reflect_test.B[reflect_test.A]]"u8;

public static void TestIssue50208(ж<Δtesting.T> t) {
    @string want1 = bReflectTestAˢ;
    {
        @string got = TypeOf(@new<B<A>>()).Elem().Name(); if (got != want1) {
            t.Errorf("name of type parameter mismatched, want:%s, got:%s", want1, got);
        }
    }
    @string want2 = bReflectTestBReflectTestˢ;
    {
        @string got = TypeOf(@new<B<B<A>>>()).Elem().Name(); if (got != want2) {
            t.Errorf("name of type parameter mismatched, want:%s, got:%s", want2, got);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string kind1ˢ => ref kind1ˢᵇ;
private static readonly @string kind1ˢᵇ = "kind-1"u8;

public static void TestNegativeKindString(ж<Δtesting.T> t) {
    nint x = -1;
    @string s = ((reflectꓸKind)(nuint)x).String();
    @string want = kind1ˢ;
    if (s != want) {
        t.Fatalf("Kind(-1).String() = %q, want %q", s, want);
    }
}

partial struct namedBool /*bool*/;

partial struct namedBytes /*[]byte*/;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string reflectCallOfReflectˢ => ref reflectCallOfReflectˢᵇ;
private static readonly @string reflectCallOfReflectˢᵇ = "reflect: call of reflect.Value.Cap on ptr to non-array Value"u8;

public static void TestValue_Cap(ж<Δtesting.T> t) {
    var a = Ꮡ(new nint[]{1, 2, 3}.array());
    var v = ValueOf(a.OrTypedNil());
    if (v.Cap() != 3) {
        t.Errorf("Cap = %d want %d", v.Cap(), 3);
    }

    a = ж<array<nint>>.NilBoxOfDims(3L);
    v = ValueOf(a.OrTypedNil());
    if (v.Cap() != 3) {
        t.Errorf("Cap = %d want %d", v.Cap(), 3);
    }

    @string /*errorStr*/ getError(Action f) {
        @string errorStr = default;
        GoFrame ᒐ = default;
        try {
            defer(() => {
                var eΔ1 = recover();
                {
                    var (str, ok) = eΔ1._<@string>(ᐧ); if (ok) {
                        errorStr = str;
                    }
                }
            }, ref ᒐ);
            f();
            goto ᒐdone;
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
        ᒐdone: return errorStr;
    }
    @string e = getError(() => {
        ж<nint> ptr = default;
        ValueOf(ptr.OrTypedNil()).Cap();
    });
    @string wantStr = reflectCallOfReflectˢ;
    if (e != wantStr) {
        t.Errorf("error is %q, want %q", e, wantStr);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string reflectCallOfReflectˢ2 => ref reflectCallOfReflectˢ2ᵇ;
private static readonly @string reflectCallOfReflectˢ2ᵇ = "reflect: call of reflect.Value.Len on ptr to non-array Value"u8;

public static void TestValue_Len(ж<Δtesting.T> t) {
    var a = Ꮡ(new nint[]{1, 2, 3}.array());
    var v = ValueOf(a.OrTypedNil());
    if (v.Len() != 3) {
        t.Errorf("Len = %d want %d", v.Len(), 3);
    }

    a = ж<array<nint>>.NilBoxOfDims(3L);
    v = ValueOf(a.OrTypedNil());
    if (v.Len() != 3) {
        t.Errorf("Len = %d want %d", v.Len(), 3);
    }

    @string /*errorStr*/ getError(Action f) {
        @string errorStr = default;
        GoFrame ᒐ = default;
        try {
            defer(() => {
                var eΔ1 = recover();
                {
                    var (str, ok) = eΔ1._<@string>(ᐧ); if (ok) {
                        errorStr = str;
                    }
                }
            }, ref ᒐ);
            f();
            goto ᒐdone;
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
        ᒐdone: return errorStr;
    }
    @string e = getError(() => {
        ж<nint> ptr = default;
        ValueOf(ptr.OrTypedNil()).Len();
    });
    @string wantStr = reflectCallOfReflectˢ2;
    if (e != wantStr) {
        t.Errorf("error is %q, want %q", e, wantStr);
    }
}

internal partial struct TestValue_Comparable_type /*dyn*/ {
    public any I;
}

internal partial struct TestValue_Comparable_typeᴛ1 /*dyn*/ {
    internal reflectꓸValue value;
    internal bool comparable;
    internal bool deref;
}

internal partial struct TestValue_Comparable_typeᴛ2 /*dyn*/ {
    public nint I;
}

public static void TestValue_Comparable(ж<Δtesting.T> t) {
    ref var a = ref heap(new nint(), out var Ꮡa);
    ref var s = ref heap<slice<nint>>(out var Ꮡs);
    ref var i = ref heap<any>(out var Ꮡi);

    i = a;
    ref var iNil = ref heap<any>(out var ᏑiNil);
    ref var iSlice = ref heap<any>(out var ᏑiSlice);

    iSlice = s;
    ref var iArrayFalse = ref heap<any>(out var ᏑiArrayFalse);

    iArrayFalse = new any[]{(nint)1, new map<nint, nint>{}}.array();
    ref var iArrayTrue = ref heap<any>(out var ᏑiArrayTrue);

    iArrayTrue = new any[]{(nint)1, new TestValue_Comparable_type((nint)1)}.array();
    slice<TestValue_Comparable_typeᴛ1> testcases = slice<TestValue_Comparable_typeᴛ1>([
        new(
            ValueOf(ᏑiNil),
            true,
            true
        ),
        new(
            ValueOf((nint)32),
            true,
            false
        ),
        new(
            ValueOf((int8)1),
            true,
            false
        ),
        new(
            ValueOf((int16)1),
            true,
            false
        ),
        new(
            ValueOf((int32)1),
            true,
            false
        ),
        new(
            ValueOf((int64)1),
            true,
            false
        ),
        new(
            ValueOf((uint8)1),
            true,
            false
        ),
        new(
            ValueOf((uint16)1),
            true,
            false
        ),
        new(
            ValueOf((uint32)1),
            true,
            false
        ),
        new(
            ValueOf((uint64)1),
            true,
            false
        ),
        new(
            ValueOf((float32)1F),
            true,
            false
        ),
        new(
            ValueOf((float64)1D),
            true,
            false
        ),
        new(
            ValueOf(complex((float32)1F, (float32)1F)),
            true,
            false
        ),
        new(
            ValueOf(complex((float64)1D, (float64)1D)),
            true,
            false
        ),
        new(
            ValueOf(abcˢ),
            true,
            false
        ),
        new(
            ValueOf(true),
            true,
            false
        ),
        new(
            ValueOf(new map<nint, nint>{}),
            false,
            false
        ),
        new(
            ValueOf(slice<nint>([])),
            false,
            false
        ),
        new(
            new reflectꓸValue(),
            false,
            false
        ),
        new(
            ValueOf(Ꮡa),
            true,
            false
        ),
        new(
            ValueOf(Ꮡs),
            true,
            false
        ),
        new(
            ValueOf(Ꮡi),
            true,
            true
        ),
        new(
            ValueOf(ᏑiSlice),
            false,
            true
        ),
        new(
            ValueOf(new nint[]{}.array(2)),
            true,
            false
        ),
        new(
            ValueOf(new map<nint, nint>[]{}.array(2)),
            false,
            false
        ),
        new(
            ValueOf(new Action[]{}.array()),
            false,
            false
        ),
        new(
            ValueOf(new TestValue_Comparable_type[]{new((nint)1), new((nint)1)}.array()),
            true,
            false
        ),
        new(
            ValueOf(new TestValue_Comparable_type[]{new(slice<nint>([])), new((nint)1)}.array()),
            false,
            false
        ),
        new(
            ValueOf(new any[]{(nint)1, new TestValue_Comparable_typeᴛ2(1)}.array()),
            true,
            false
        ),
        new(
            ValueOf(new any[]{new any[]{new map<nint, nint>{}}.array(), new TestValue_Comparable_typeᴛ2(1)}.array()),
            false,
            false
        ),
        new(
            ValueOf(ᏑiArrayFalse),
            false,
            true
        ),
        new(
            ValueOf(ᏑiArrayTrue),
            true,
            true
        )
    ]);

    foreach (var (_, cas) in testcases) {
        var v = cas.value;
        if (cas.deref) {
            v = v.Elem();
        }
        var got = v.Comparable();
        if (got != cas.comparable) {
            t.Errorf("%T.Comparable = %t, want %t", v, got, cas.comparable);
        }
    }
}

partial struct ValueEqualTest {
    internal any v, u;
    internal bool eq;
    internal bool vDeref, uDeref;
}

internal static readonly ж<any> ᏑequalI = new StaticBox<any>((nint)1);
internal static ref any equalI => ref ((StaticBox<any>)ᏑequalI).ValueSlot;

internal static any equalSlice = slice<nint>([1]);

internal static readonly ж<any> ᏑnilInterface = new StaticBox<any>(default(any));
internal static ref any nilInterface => ref ((StaticBox<any>)ᏑnilInterface).ValueSlot;

internal static any mapInterface = new map<nint, nint>{};

        partial struct Δtypeᴛ36 /*dyn*/ {
            internal nint i;
        }
internal static slice<ValueEqualTest> valueEqualTests = slice<ValueEqualTest>([
    new(
        new reflectꓸValue(), new reflectꓸValue(),
        true,
        false, false
    ),
    new(
        true, true,
        true,
        false, false
    ),
    new(
        (nint)1, (nint)1,
        true,
        false, false
    ),
    new(
        (int8)1, (int8)1,
        true,
        false, false
    ),
    new(
        (int16)1, (int16)1,
        true,
        false, false
    ),
    new(
        (int32)1, (int32)1,
        true,
        false, false
    ),
    new(
        (int64)1, (int64)1,
        true,
        false, false
    ),
    new(
        (nuint)1, (nuint)1,
        true,
        false, false
    ),
    new(
        (uint8)1, (uint8)1,
        true,
        false, false
    ),
    new(
        (uint16)1, (uint16)1,
        true,
        false, false
    ),
    new(
        (uint32)1, (uint32)1,
        true,
        false, false
    ),
    new(
        (uint64)1, (uint64)1,
        true,
        false, false
    ),
    new(
        (float32)1F, (float32)1F,
        true,
        false, false
    ),
    new(
        (float64)1D, (float64)1D,
        true,
        false, false
    ),
    new(
        complex(1D, 1D), complex(1D, 1D),
        true,
        false, false
    ),
    new(
        (complex128)(1D + 1D.i()), (complex128)(1D + 1D.i()),
        true,
        false, false
    ),
    new(
        () => {
        }, default,
        false,
        false, false
    ),
    new(
        ᏑequalI, (nint)1,
        true,
        true, false
    ),
    new(
        (channel<nint>)(default), default,
        false,
        false, false
    ),
    new(
        (channel<nint>)(default), (channel<nint>)(default),
        true,
        false, false
    ),
    new(
        ᏑequalI, ᏑequalI,
        true,
        false, false
    ),
    new(
        new Δtypeᴛ36(1), new Δtypeᴛ36(1),
        true,
        false, false
    ),
    new(
        new Δtypeᴛ36(1), new Δtypeᴛ36(2),
        false,
        false, false
    ),
    new(
        ᏑnilInterface, ᏑnilInterface,
        true,
        true, true
    ),
    new(
        (nint)1, ValueOf(new Δtypeᴛ36(1)).Field(0),
        true,
        false, false
    )
]);

public static void TestValue_Equal(ж<Δtesting.T> t) {
    foreach (var (_, test) in valueEqualTests) {
        reflectꓸValue v = new(nil);
        reflectꓸValue u = new(nil);
        {
            var (vv, ok) = test.v._<reflectꓸValue>(ᐧ); if (ok) {
                v = vv;
            } else {
                v = ValueOf(test.v);
            }
        }

        {
            var (uu, ok) = test.u._<reflectꓸValue>(ᐧ); if (ok) {
                u = uu;
            } else {
                u = ValueOf(test.u);
            }
        }
        if (test.vDeref) {
            v = v.Elem();
        }

        if (test.uDeref) {
            u = u.Elem();
        }

        {
            var r = v.Equal(u); if (r != test.eq) {
                t.Errorf("%s == %s got %t, want %t", v.Type(), u.Type(), r, test.eq);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string areNotComparableˢ => ref areNotComparableˢᵇ;
private static readonly @string areNotComparableˢᵇ = "are not comparable"u8;

internal partial struct TestValue_EqualNonComparable_type /*dyn*/ {
    public any I;
}

public static void TestValue_EqualNonComparable(ж<Δtesting.T> t) {
    reflectꓸValue invalid = new reflectꓸValue(); // ValueOf(nil)
    slice<reflectꓸValue> values = slice<reflectꓸValue>([
        // Value of slice is non-comparable.
        ValueOf(slice<nint>(default)),
        ValueOf((slice<nint>([]))),

        // Value of map is non-comparable.
        ValueOf((map<nint, nint>)default),
        ValueOf((new map<nint, nint>{})),

        // Value of func is non-comparable.
        ValueOf((((Action)(default))).OrTypedNilFunc()),
        ValueOf(() => {
        }),

        // Value of struct is non-comparable because of non-comparable elements.
        ValueOf((new NonComparableStruct())),

        // Value of array is non-comparable because of non-comparable elements.
        ValueOf(new map<nint, nint>[]{}.array()),
        ValueOf(new Action[]{}.array()),
        ValueOf((new TestValue_EqualNonComparable_type[]{new(slice<nint>([]))}.array())),
        ValueOf((new any[]{new any[]{new map<nint, nint>{}}.array()}.array()))
    ]);
    foreach (var (_, vᴛ1) in values) {
        ref var value = ref heap(new reflectꓸValue(), out var Ꮡvalue);
        value = vᴛ1;

        // Panic when reflect.Value.Equal using two valid non-comparable values.
        var valueʗ1 = value;
        shouldPanic(areNotComparableˢ, () => {
            valueʗ1.Equal(valueʗ1);
        });

        // If one is non-comparable and the other is invalid, the expected result is always false.
        {
            var r = value.Equal(invalid); if (r != false) {
                t.Errorf("%s == invalid got %t, want false", value.Type(), r);
            }
        }
    }
}

public static partial void TestInitFuncTypes(ж<Δtesting.T> t) {
    nint n = 100;
    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);

    Ꮡwg.Add(n);
    for (nint i = 0; i < n; i++) {
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                var ipT = TypeOf(new Δnet.IP(new byte[]{}.slice()));
                foreach (var _ᴛ1 in range<reflectꓸMethod>(ipT.Methods().Invoke)) {
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
    Ꮡwg.Wait();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string reflectValueClearˢ => ref reflectValueClearˢᵇ;
private static readonly @string reflectValueClearˢᵇ = "reflect.Value.Clear"u8;

internal partial struct TestClear_tests /*dyn*/ {
    internal @string name;
    internal reflectꓸValue value;
    internal Func<reflectꓸValue, bool> testFunc;
}

public static void TestClear(ж<Δtesting.T> t) {
    var m = new map<@string, any>(len(valueTests));
    foreach (var (_, tt) in valueTests) {
        m[tt.s] = tt.i;
    }
    var mapTestFn = (reflectꓸValue v) => {
        v.Clear();
        return v.Len() == 0;
    };

    var s = new slice<ж<pair>>(len(valueTests));
    foreach (var i in range(s)) {
        s[i] = Ꮡ(valueTests, i);
    }
    var sliceTestFn = (reflectꓸValue v) => {
        v.Clear();
        for (nint i = 0; i < v.Len(); i++) {
            if (!v.Index(i).IsZero()) {
                return false;
            }
        }
        return true;
    };

    var panicTestFn = (reflectꓸValue v) => {
        var vʗ1 = v;
        shouldPanic(reflectValueClearˢ, () => {
            vʗ1.Clear();
        });
        return true;
    };

    var tests = slice<TestClear_tests>([
        new("map", ValueOf(m), mapTestFn),
        new("slice no pointer", ValueOf(slice<nint>([1, 2, 3, 4, 5])), sliceTestFn),
        new("slice has pointer", ValueOf(s), sliceTestFn),
        new("non-map/slice", ValueOf((nint)1), panicTestFn)
    ]);

    foreach (var (_, vᴛ1) in tests) {
        ref var tc = ref heap(new TestClear_tests(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        t.Run(tc.name, (ж<Δtesting.T> tΔ1) => {
            tΔ1.Parallel();
            if (!tcʗ1.testFunc(tcʗ1.value)) {
                tΔ1.Errorf("unexpected result for value.Clear(): %v", tcʗ1.value);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fooˢ => ref fooˢᵇ;
private static readonly @string fooˢᵇ = "foo"u8;

internal partial struct TestValuePointerAndUnsafePointer_tests /*dyn*/ {
    internal @string name;
    internal reflectꓸValue val;
    internal @unsafe.Pointer wantUnsafePointer;
}

public static void TestValuePointerAndUnsafePointer(ж<Δtesting.T> t) {
    var ptr = @new<nint>();
    ref var ch = ref heap<channel<nint>>(new channel<nint>(0), out var Ꮡch);
    ref var m = ref heap<map<nint, nint>>(new map<nint, nint>(), out var Ꮡm);
    @unsafe.Pointer unsafePtr = @unsafe.Pointer.FromPinnedBox(ptr);
    var Δslice = new slice<nint>(1);
    ref var fn = ref heap<Action>(() => {
    }, out var Ꮡfn);
    @string s = fooˢ;

    var tests = slice<TestValuePointerAndUnsafePointer_tests>([
        new("pointer", ValueOf(ptr.OrTypedNil()), @unsafe.Pointer.FromPinnedBox(ptr)),
        new("channel", ValueOf(ch), ~Ꮡ(new @unsafe.Pointer((uintptr)Ꮡch))),
        new("map", ValueOf(m), ~Ꮡ(new @unsafe.Pointer((uintptr)Ꮡm))),
        new("unsafe.Pointer", ValueOf(@unsafe.Pointer.OrTypedNil(unsafePtr)), unsafePtr.Value),
        new("function", ValueOf((fn).OrTypedNilFunc()), (~Ꮡ(Ꮡ(new @unsafe.Pointer((uintptr)Ꮡfn)))).Value),
        new("slice", ValueOf(Δslice), @unsafe.Pointer.FromPinnedBox(@unsafe.SliceData(Δslice))),
        new("string", ValueOf(s), @unsafe.Pointer.FromPinnedBox(@unsafe.StringData(s)))
    ]);

    foreach (var (_, vᴛ1) in tests) {
        ref var tc = ref heap(new TestValuePointerAndUnsafePointer_tests(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        t.Run(tc.name, (ж<Δtesting.T> tΔ1) => {
            {
                var got = tcʗ1.val.Pointer(); if (got != (uintptr)tcʗ1.wantUnsafePointer) {
                    tΔ1.Errorf("unexpected uintptr result, got %#x, want %#x", got, (uintptr)tcʗ1.wantUnsafePointer);
                }
            }
            {
                @unsafe.Pointer got = (uintptr)tcʗ1.val.UnsafePointer(); if (got != tcʗ1.wantUnsafePointer) {
                    tΔ1.Errorf("unexpected unsafe.Pointer result, got %#x, want %#x", @unsafe.Pointer.OrTypedNil(got), @unsafe.Pointer.OrTypedNil(tcʗ1.wantUnsafePointer));
                }
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object nilPointerWithZeroLengthˢ = (@string)"nil pointer with zero length must return nil"u8;

// Hoisted Go big-integer constant (single parse; Go folds constants at compile time)
private static ref readonly GoBigConst maxUintptrᶜ => ref maxUintptrᶜᵇ;
private static readonly GoBigConst maxUintptrᶜᵇ = GoBigConst.Parse("18446744073709551616");

// Test cases copied from ../../test/unsafebuiltins.go
public static void TestSliceAt(ж<Δtesting.T> t) {
    GoBigConst maxUintptr = /* 1 << (8 * unsafe.Sizeof(uintptr(0))) */
            maxUintptrᶜ;
    ref var p = ref heap(new array<byte>(10), out var Ꮡp);

    var typ = TypeOf(p[0]);

    var s = SliceAt(typ, @unsafe.Pointer.FromPinnedBox(Ꮡp.at<byte>(0)), len(p));
    if (s.Pointer() != (uintptr)Ꮡp.at<byte>(0)) {
        t.Fatalf("unexpected underlying array: %d, want: %d", s.Pointer(), (uintptr)Ꮡp.at<byte>(0));
    }
    if (s.Len() != len(p) || s.Cap() != len(p)) {
        t.Fatalf("unexpected len or cap, len: %d, cap: %d, want: %d", s.Len(), s.Cap(), len(p));
    }

    typ = TypeOf((nint)0);
    if (!SliceAt(typ, @unsafe.Pointer.FromPinnedBox((ж<nint>)nil), 0).IsNil()) {
        t.Fatal(nilPointerWithZeroLengthˢ);
    }

    // nil pointer with positive length panics
    var typʗ1 = typ;
    shouldPanic(""u8, () => {
        _ = SliceAt(typʗ1, @unsafe.Pointer.FromPinnedBox((ж<nint>)nil), 1);
    });

    // negative length
    nint neg = -1;
    shouldPanic(""u8, () => {
        _ = SliceAt(TypeOf((byte)0), @unsafe.Pointer.FromPinnedBox(Ꮡp.at<byte>(0)), neg);
    });

    // size overflows address space
    ref var n = ref heap<uint64>((uint64)0, out var Ꮡn);
    shouldPanic(""u8, () => {
        _ = SliceAt(TypeOf(Ꮡn.Value), @unsafe.Pointer.FromPinnedBox(Ꮡn), unchecked((nint)(2305843009213693952L)));
    });
    shouldPanic(""u8, () => {
        _ = SliceAt(TypeOf(Ꮡn.Value), @unsafe.Pointer.FromPinnedBox(Ꮡn), unchecked((nint)(2305843009213693953L)));
    });

    // sliced memory overflows address space
    var last = (ж<byte>)(uintptr)((@unsafe.Pointer)(~(uintptr)0));
    // This panics here, but won't panic in ../../test/unsafebuiltins.go,
    // because unsafe.Slice(last, 1) does not escape.
    //
    // _ = SliceAt(typ, unsafe.Pointer(last), 1)
    var lastʗ1 = last;
    var typʗ2 = typ;
    shouldPanic(""u8, () => {
        _ = SliceAt(typʗ2, @unsafe.Pointer.FromPinnedBox(lastʗ1), 2);
    });
}

// Test that maps created with MapOf properly updates keys on overwrite as
// expected (i.e., it sets the key update flag in the map).
//
// This test is based on runtime.TestNegativeZero.
public static void TestMapOfKeyUpdate(ж<Δtesting.T> t) {
    var m = MakeMap(MapOf(TypeFor<float64>(), TypeFor<bool>()));

    var zero = (float64)0.0D;
    var negZero = Δmath.Copysign(zero, -1.0D);

    m.SetMapIndex(ValueOf(zero), ValueOf(true));
    m.SetMapIndex(ValueOf(negZero), ValueOf(true));

    if (m.Len() != 1) {
        t.Errorf("map length got %d want 1", m.Len());
    }

    var iter = m.MapRange();
    while (iter.Next()) {
        var k = iter.Key().Float();
        if (Δmath.Copysign(1.0D, k) > 0D) {
            t.Errorf("map key %f has positive sign", k);
        }
    }
}

// Test that maps created with MapOf properly panic on unhashable keys, even if
// the map is empty. (i.e., it sets the hash might panic flag in the map).
//
// This test is a simplified version of runtime.TestEmptyMapWithInterfaceKey
// for reflect.
public static void TestMapOfKeyPanic(ж<Δtesting.T> t) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            var r = recover();
            if (r == nil) {
                t.Errorf("didn't panic");
            }
        }, ref ᒐ);

        var m = MakeMap(MapOf(TypeFor<any>(), TypeFor<bool>()));

        slice<nint> Δslice = default;
        m.MapIndex(ValueOf(Δslice));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testValueˢ => ref testValueˢᵇ;
private static readonly @string testValueˢᵇ = "test value"u8;

internal partial interface TestTypeAssert_type /*dyn*/ {
    void A();
}

public static void TestTypeAssert(ж<Δtesting.T> t) {
    testTypeAssert<nint, nint, nint>(t, (nint)123456789, (nint)123456789, true);
    testTypeAssert<nint, nint, nint>(t, (nint)(-123456789), (nint)(-123456789), true);
    testTypeAssert<int32, int32, int32>(t, (int32)123456789, (int32)123456789, true);
    testTypeAssert<int8, int8, int8>(t, (int8)(-123), (int8)(-123), true);
    testTypeAssert<array<nint>, array<nint>, array<nint>>(t, new nint[]{1234, -5678}.array(), new nint[]{1234, -5678}.array(), true);
    testTypeAssert<@string, @string, @string>(t, testValueˢ, testValueˢ, true);
    testTypeAssert<any, any, any>(t, (any)(@string)("test value"u8), (any)(@string)("test value"u8), true);

    ref var v = ref heap<nint>(123456789, out var Ꮡv);
    testTypeAssert<ж<nint>, ж<nint>, ж<nint>>(t, Ꮡv, Ꮡv, true);

    testTypeAssert<nuint, nint, nuint>(t, (nint)123, (nuint)0, false);

    testTypeAssert<any, nint, any>(t, 1, (nint)1, true);
    testTypeAssert<fmt.Stringer, nint, fmt.Stringer>(t, 1, default, false);

    ref var vv = ref heap<testTypeWithMethod>(new testTypeWithMethod("test"), out var Ꮡvv);
    testTypeAssert<any, testTypeWithMethod, any>(t, vv, vv, true);
    testTypeAssert<any, ж<testTypeWithMethod>, any>(t, Ꮡvv, Ꮡvv, true);
    testTypeAssert<fmt.Stringer, testTypeWithMethod, fmt.Stringer>(t, vv, vv, true);
    testTypeAssert<fmt.Stringer, ж<testTypeWithMethod>, fmt.Stringer>(t, Ꮡvv, new reflect_test_package.testTypeWithMethodжStringer(Ꮡvv), true);
    testTypeAssert<TestTypeAssert_type, testTypeWithMethod, TestTypeAssert_type>(t, vv, default, false);
    testTypeAssert<TestTypeAssert_type, ж<testTypeWithMethod>, TestTypeAssert_type>(t, Ꮡvv, default, false);
    testTypeAssert<any, any, any>(t, (any)vv, (any)vv, true);
    testTypeAssert<fmt.Stringer, fmt.Stringer, fmt.Stringer>(t, (fmt.Stringer)vv, (fmt.Stringer)vv, true);

    testTypeAssert<any, fmt.Stringer, any>(t, (fmt.Stringer)vv, (any)vv, true);
    testTypeAssert<fmt.Stringer, any, fmt.Stringer>(t, (any)vv, (fmt.Stringer)vv, true);
    testTypeAssert<packIfaceValueIntoEmptyIface_type, fmt.Stringer, packIfaceValueIntoEmptyIface_type>(t, (fmt.Stringer)vv, (packIfaceValueIntoEmptyIface_type)(vv), true);
    testTypeAssert<fmt.Stringer, packIfaceValueIntoEmptyIface_type, fmt.Stringer>(t, (packIfaceValueIntoEmptyIface_type)(vv), (fmt.Stringer)vv, true);

    testTypeAssert<nint, any, nint>(t, (any)(nint)1, (nint)1, true);
    testTypeAssert<byte, any, byte>(t, (any)(nint)1, (byte)0, false);
    testTypeAssert<testTypeWithMethod, fmt.Stringer, testTypeWithMethod>(t, (fmt.Stringer)vv, vv, true);

    testTypeAssert<any, any, any>(t, (any)default, (any)default, false);
    testTypeAssert<error, any, error>(t, (any)default, (error)default, false);
    testTypeAssert<any, error, any>(t, (error)default, (any)default, false);
    testTypeAssert<error, error, error>(t, (error)default, (error)default, false);
}

internal static void testTypeAssert<T, V, Tᴺ>(ж<Δtesting.T> t, V valʗp, T wantVal, bool wantOk) {
    ref var val = ref heap(valʗp, out var Ꮡval);

    t.Helper();

    var (v, ok) = TypeAssert<T>(ValueOf(Ꮡval).Elem());
    if (!AreEqual(v, wantVal) || ok != wantOk) {
        t.Errorf("TypeAssert[%v](%#v) = (%#v, %v); want = (%#v, %v)", TypeFor<Tᴺ>(), (val).OrTypedNilOf(), (v).OrTypedNilOf(), ok, (wantVal).OrTypedNilOf(), wantOk);
    }

    // Additionally make sure that TypeAssert[T](v) behaves in the same way as v.Interface().(T).
    var (v2, ok2) = ValueOf(Ꮡval).Elem().Interface()._<T>(ᐧ);
    if (!AreEqual(v, v2) || ok != ok2) {
        t.Errorf("reflect.ValueOf(%#v).Interface().(%v) = (%#v, %v); want = (%#v, %v)", (val).OrTypedNilOf(), TypeFor<Tᴺ>(), (v2).OrTypedNilOf(), ok2, (v).OrTypedNilOf(), ok);
    }
}

partial struct testTypeWithMethod {
    internal @string val;
}

internal static @string String(this testTypeWithMethod v) {
    return v.val;
}

internal static void M(this testTypeWithMethod v) {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string stringˢ2 => ref stringˢ2ᵇ;
private static readonly @string stringˢ2ᵇ = "String"u8;

public static void TestTypeAssertMethod(ж<Δtesting.T> t) {
    var method = ValueOf(Ꮡ(new testTypeWithMethod(val: "test value"))).MethodByName(stringˢ2);
    var (f, ok) = TypeAssert<Func<@string>>(method);
    if (!ok) {
        t.Fatalf(@"TypeAssert[func() string](method) = (,false); want = (,true)");
    }

    @string @out = f();
    if (@out != "test value"u8) {
        t.Fatalf(@"TypeAssert[func() string](method)() = %q; want ""test value""", @out);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string zeroValˢ => ref zeroValˢᵇ;
private static readonly @string zeroValˢᵇ = "zero val"u8;
internal static ref readonly @string readOnlyˢ => ref readOnlyˢᵇ;
private static readonly @string readOnlyˢᵇ = "read only"u8;
internal static ref readonly @string valˢ => ref valˢᵇ;
private static readonly @string valˢᵇ = "val"u8;

public static void TestTypeAssertPanic(ж<Δtesting.T> t) {
    t.Run(zeroValˢ, (ж<Δtesting.T> tΔ1) => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                recover();
            }, ref ᒐ);
            TypeAssert<nint>(new reflectꓸValue());
            tΔ1.Fatalf("TypeAssert did not panic");
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
    t.Run(readOnlyˢ, (ж<Δtesting.T> tΔ2) => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                recover();
            }, ref ᒐ);
            TypeAssert<nint>(ValueOf(Ꮡ(new testTypeWithMethod())).FieldByName(valˢ));
            tΔ2.Fatalf("TypeAssert did not panic");
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object instrumentationBreaksˢ = (@string)"instrumentation breaks this optimization"u8;

internal partial interface TestTypeAssertAllocs_I /*dyn*/ {
    void foo();
}

public static void TestTypeAssertAllocs(ж<Δtesting.T> t) {
    if (race.Enabled || asan.Enabled || msan.Enabled) {
        t.Skip(instrumentationBreaksˢ);
    }
    typeAssertAllocs<array<nint>, array<nint>>(t, ValueOf(new nint[]{}.array(128)), 0);
    typeAssertAllocs<any, any>(t, ValueOf(new nint[]{}.array(128)), 0);

    ref var val = ref heap<nint>(123, out var Ꮡval);
    typeAssertAllocs<any, any>(t, ValueOf(val), 0);
    typeAssertAllocs<any, any>(t, ValueOf(Ꮡval).Elem(), 1); // must allocate, so that Set() does not modify the returned inner iface value.
    typeAssertAllocs<nint, nint>(t, ValueOf(val), 0);
    typeAssertAllocs<nint, nint>(t, ValueOf(Ꮡval).Elem(), 0);

    typeAssertAllocs<time.Time, time.Time>(t, ValueOf(@new<time.Time>()).Elem(), 0);
    typeAssertAllocs<time.Time, time.Time>(t, ValueOf(@new<time.Time>().Value), 0);
    typeAssertAllocs<TestTypeAssertAllocs_I, TestTypeAssertAllocs_I>(t, ValueOf(@new<@string>()).Elem(), 0); // assert fail doesn't alloc
}

internal static void typeAssertAllocs<T, Tᴺ>(ж<Δtesting.T> t, reflectꓸValue val, nint wantAllocs) {
    t.Helper();
    var valʗ1 = val;
    var allocs = Δtesting.AllocsPerRun(10, () => {
        TypeAssert<T>(valʗ1);
    });
    if (allocs != (float64)wantAllocs) {
        t.Errorf("TypeAssert[%v](%v) unexpected amount of allocations = %v; want = %v", TypeFor<Tᴺ>(), val.Type(), allocs, wantAllocs);
    }
}

public static void BenchmarkTypeAssert(ж<Δtesting.B> b) {
    benchmarkTypeAssert<nint, nint>(b, ValueOf((nint)1));
    benchmarkTypeAssert<byte, byte>(b, ValueOf((nint)1));

    benchmarkTypeAssert<fmt.Stringer, fmt.Stringer>(b, ValueOf(new testTypeWithMethod()));
    benchmarkTypeAssert<fmt.Stringer, fmt.Stringer>(b, ValueOf(Ꮡ(new testTypeWithMethod())));
    benchmarkTypeAssert<any, any>(b, ValueOf((nint)1));
    benchmarkTypeAssert<any, any>(b, ValueOf(new testTypeWithMethod()));

    benchmarkTypeAssert<time.Time, time.Time>(b, ValueOf(@new<time.Time>().Value));

    benchmarkTypeAssert<Func<@string>, Func<@string>>(b, ValueOf(time.Now()).MethodByName(stringˢ2));
}

internal static void benchmarkTypeAssert<T, Tᴺ>(ж<Δtesting.B> b, reflectꓸValue val) {
    var valʗ1 = val;
    b.Run(fmt.Sprintf("TypeAssert[%v](%v)"u8, TypeFor<Tᴺ>(), val.Type()), (ж<Δtesting.B> bΔ1) => {
        while (bΔ1.Loop()) {
            TypeAssert<T>(valʗ1);
        }
    });
}

} // end reflect_test_package
