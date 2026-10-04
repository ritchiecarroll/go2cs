// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
global using Loopy = object;
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
using goexperiment = @internal.goexperiment_package;
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

public static void TestBool(ж<Δtesting.T> Ꮡt) {
    var v = ValueOf(true);
    if (v.Bool() != true) {
        Ꮡt.Fatal(valueOfTrueBoolFalseˢ);
    }
}

[GoType("num:nint")] partial struct integer;

[GoType] partial struct T {
    internal nint a;
    internal float64 b;
    internal @string c;
    internal ж<nint> d;
}

internal static bool _ᴛ1ʗ = new T(nil) == new T(nil); // tests depend on T being comparable

[GoType] partial struct pair {
    internal any i;
    internal @string s;
}

internal static void assert(ж<Δtesting.T> Ꮡt, @string s, @string want) {
    if (s != want) {
        Ꮡt.Errorf("have %#q want %#q"u8, s, want);
    }
}


    [GoType("dyn")] partial struct Δtype {
        internal nint x;
    }

    [GoType("dyn")] partial struct Δtypeᴛ1 {
        internal int8 x;
    }

    [GoType("dyn")] partial struct Δtypeᴛ2 {
        internal int16 x;
    }

    [GoType("dyn")] partial struct Δtypeᴛ3 {
        internal int32 x;
    }

    [GoType("dyn")] partial struct Δtypeᴛ4 {
        internal int64 x;
    }

    [GoType("dyn")] partial struct Δtypeᴛ5 {
        internal nuint x;
    }

    [GoType("dyn")] partial struct Δtypeᴛ6 {
        internal uint8 x;
    }

    [GoType("dyn")] partial struct Δtypeᴛ7 {
        internal uint16 x;
    }

    [GoType("dyn")] partial struct Δtypeᴛ8 {
        internal uint32 x;
    }

    [GoType("dyn")] partial struct Δtypeᴛ9 {
        internal uint64 x;
    }

    [GoType("dyn")] partial struct Δtypeᴛ10 {
        internal float32 x;
    }

    [GoType("dyn")] partial struct Δtypeᴛ11 {
        internal float64 x;
    }

    [GoType("dyn")] partial struct Δtypeᴛ12 {
        internal ж<ж<int8>> x;
    }

    [GoType("dyn")] partial struct Δtypeᴛ13 {
        internal ж<ж<integer>> x;
    }

    [GoType("dyn")] partial struct Δtypeᴛ14 {
        internal array<int32> x = new(32);
    }

    [GoType("dyn")] partial struct Δtypeᴛ15 {
        internal slice<int8> x;
    }

    [GoType("dyn")] partial struct Δtypeᴛ16 {
        internal map<@string, int32> x;
    }

    [GoType("dyn")] partial struct Δtypeᴛ17 {
        internal channel/*<-*/<@string> x = channel/*<-*/<@string>.SendOnly;
    }

    [GoType("dyn")] partial struct Δtypeᴛ18 {
        internal channel/*<-*/<channel<@string>> x = channel/*<-*/<channel<@string>>.SendOnly;
    }

    [GoType("dyn")] partial struct Δtypeᴛ19 {
        internal channel/*<-*/</*<-*/channel<@string>> x = channel/*<-*/</*<-*/channel<@string>>.Nil(ChanCargo.Of(new GoChanDir[] { GoChanDir.Send, GoChanDir.Recv }, null));
    }

    [GoType("dyn")] partial struct Δtypeᴛ20 {
        internal /*<-*/channel</*<-*/channel<@string>> x = /*<-*/channel</*<-*/channel<@string>>.Nil(ChanCargo.Of(new GoChanDir[] { GoChanDir.Recv, GoChanDir.Recv }, null));
    }

    [GoType("dyn")] partial struct Δtypeᴛ21 {
        internal channel</*<-*/channel<@string>> x = channel</*<-*/channel<@string>>.Nil(ChanCargo.Of(new GoChanDir[] { GoChanDir.Both, GoChanDir.Recv }, null));
    }

    [GoType("dyn")] partial struct typeᴛ22_x {
        internal channel<ж<int32>> c;
        internal float32 d;
    }

    [GoType("dyn")] partial struct Δtypeᴛ22 {
        internal typeᴛ22_x x;
    }

    [GoType("dyn")] partial struct Δtypeᴛ23 {
        internal Action<int8, int32> x;
    }

    [GoType("dyn")] partial struct typeᴛ24_x {
        internal Action<channel<ж<integer>>, ж<int8>> c;
    }

    [GoType("dyn")] partial struct Δtypeᴛ24 {
        internal typeᴛ24_x x;
    }

    [GoType("dyn")] partial struct typeᴛ25_x {
        internal int8 a;
        internal int32 b;
    }

    [GoType("dyn")] partial struct Δtypeᴛ25 {
        internal typeᴛ25_x x;
    }

    [GoType("dyn")] partial struct typeᴛ26_x {
        internal int8 a;
        internal int8 b;
        internal int32 c;
    }

    [GoType("dyn")] partial struct Δtypeᴛ26 {
        internal typeᴛ26_x x;
    }

    [GoType("dyn")] partial struct typeᴛ27_x {
        internal int8 a;
        internal int8 b;
        internal int8 c;
        internal int32 d;
    }

    [GoType("dyn")] partial struct Δtypeᴛ27 {
        internal typeᴛ27_x x;
    }

    [GoType("dyn")] partial struct typeᴛ28_x {
        internal int8 a;
        internal int8 b;
        internal int8 c;
        internal int8 d;
        internal int32 e;
    }

    [GoType("dyn")] partial struct Δtypeᴛ28 {
        internal typeᴛ28_x x;
    }

    [GoType("dyn")] partial struct typeᴛ29_x {
        internal int8 a;
        internal int8 b;
        internal int8 c;
        internal int8 d;
        internal int8 e;
        internal int32 f;
    }

    [GoType("dyn")] partial struct Δtypeᴛ29 {
        internal typeᴛ29_x x;
    }

    [GoType("dyn")] partial struct typeᴛ30_x {
        [GoTag(@"reflect:""hi there""")]
        internal int8 a;
    }

    [GoType("dyn")] partial struct Δtypeᴛ30 {
        internal typeᴛ30_x x;
    }

    [GoType("dyn")] partial struct typeᴛ31_x {
        [GoTag(@"reflect:""hi \x00there\t\n\""\\""")]
        internal int8 a;
    }

    [GoType("dyn")] partial struct Δtypeᴛ31 {
        internal typeᴛ31_x x;
    }

    [GoType("dyn")] partial struct typeᴛ32_x {
        internal Actionꓸꓸꓸ<nint> f;
    }

    [GoType("dyn")] partial struct Δtypeᴛ32 {
        internal typeᴛ32_x x;
    }

    [GoType("dyn")] partial interface typeᴛ33_x {
        void a(Func<Func<nint, nint>, Func<Action<nint>, nint>> _);
        void b();
    }

    [GoType("dyn")] partial struct Δtypeᴛ33 {
        internal typeᴛ33_x x;
    }

    [GoType("dyn")] partial struct typeᴛ34_x {
        [GoEmbedded] internal int32 int32;
        [GoEmbedded] internal int64 int64;
    }

    [GoType("dyn")] partial struct Δtypeᴛ34 {
        internal typeᴛ34_x x;
    }
internal static slice<pair> typeTests = new pair[]{
    new(new Δtype(), "int"u8),
    new(new Δtypeᴛ1(), "int8"u8),
    new(new Δtypeᴛ2(), "int16"u8),
    new(new Δtypeᴛ3(), "int32"u8),
    new(new Δtypeᴛ4(), "int64"u8),
    new(new Δtypeᴛ5(), "uint"u8),
    new(new Δtypeᴛ6(), "uint8"u8),
    new(new Δtypeᴛ7(), "uint16"u8),
    new(new Δtypeᴛ8(), "uint32"u8),
    new(new Δtypeᴛ9(), "uint64"u8),
    new(new Δtypeᴛ10(), "float32"u8),
    new(new Δtypeᴛ11(), "float64"u8),
    new(new Δtypeᴛ1(), "int8"u8),
    new(new Δtypeᴛ12(), "**int8"u8),
    new(new Δtypeᴛ13(), "**reflect_test.integer"u8),
    new(new Δtypeᴛ14(), "[32]int32"u8),
    new(new Δtypeᴛ15(), "[]int8"u8),
    new(new Δtypeᴛ16(), "map[string]int32"u8),
    new(new Δtypeᴛ17(), "chan<- string"u8),
    new(new Δtypeᴛ18(), "chan<- chan string"u8),
    new(new Δtypeᴛ19(), "chan<- <-chan string"u8),
    new(new Δtypeᴛ20(), "<-chan <-chan string"u8),
    new(new Δtypeᴛ21(), "chan (<-chan string)"u8),
    new(new Δtypeᴛ22(),
        "struct { c chan *int32; d float32 }"u8
    ),
    new(new Δtypeᴛ23(), "func(int8, int32)"u8),
    new(new Δtypeᴛ24(),
        "struct { c func(chan *reflect_test.integer, *int8) }"u8
    ),
    new(new Δtypeᴛ25(),
        "struct { a int8; b int32 }"u8
    ),
    new(new Δtypeᴛ26(),
        "struct { a int8; b int8; c int32 }"u8
    ),
    new(new Δtypeᴛ27(),
        "struct { a int8; b int8; c int8; d int32 }"u8
    ),
    new(new Δtypeᴛ28(),
        "struct { a int8; b int8; c int8; d int8; e int32 }"u8
    ),
    new(new Δtypeᴛ29(),
        "struct { a int8; b int8; c int8; d int8; e int8; f int32 }"u8
    ),
    new(new Δtypeᴛ30(),
        @"struct { a int8 ""reflect:\""hi there\"""" }"u8
    ),
    new(new Δtypeᴛ31(),
        @"struct { a int8 ""reflect:\""hi \\x00there\\t\\n\\\""\\\\\"""" }"u8
    ),
    new(new Δtypeᴛ32(),
        "struct { f func(...int) }"u8
    ),
    new(new Δtypeᴛ33(),
        "interface { reflect_test.a(func(func(int) int) func(func(int)) int); reflect_test.b() }"u8
    ),
    new(new Δtypeᴛ34(),
        "struct { int32; int64 }"u8
    )
}.slice();

internal static ж<slice<pair>> ᏑvalueTests = new StandardBox<slice<pair>>(new pair[]{
    new(@new<nint>(), "132"u8),
    new(@new<int8>(), "8"u8),
    new(@new<int16>(), "16"u8),
    new(@new<int32>(), "32"u8),
    new(@new<int64>(), "64"u8),
    new(@new<nuint>(), "132"u8),
    new(@new<uint8>(), "8"u8),
    new(@new<uint16>(), "16"u8),
    new(@new<uint32>(), "32"u8),
    new(@new<uint64>(), "64"u8),
    new(@new<float32>(), "256.25"u8),
    new(@new<float64>(), "512.125"u8),
    new(@new<complex64>(), "532.125+10i"u8),
    new(@new<complex128>(), "564.25+1i"u8),
    new(@new<@string>(), "stringy cheese"u8),
    new(@new<bool>(), "true"u8),
    new(@new<ж<int8>>(), "*int8(0)"u8),
    new(@new<ж<ж<int8>>>(), "**int8(0)"u8),
    new(Ꮡ(new array<int32>(5)), "[5]int32{0, 0, 0, 0, 0}"u8),
    new(@new<ж<ж<integer>>>(), "**reflect_test.integer(0)"u8),
    new(@new<map<@string, int32>>(), "map[string]int32{<can't iterate on maps>}"u8),
    new(Ꮡ(channel/*<-*/<@string>.SendOnly), "chan<- string"u8),
    new(@new<Action<int8, int32>>(), "func(int8, int32)(0)"u8),
    new(@new<typeᴛ22_x>(),
        "struct { c chan *int32; d float32 }{chan *int32, 0}"u8
    ),
    new(@new<typeᴛ24_x>(),
        "struct { c func(chan *reflect_test.integer, *int8) }{func(chan *reflect_test.integer, *int8)(0)}"u8
    ),
    new(@new<typeᴛ25_x>(),
        "struct { a int8; b int32 }{0, 0}"u8
    ),
    new(@new<typeᴛ26_x>(),
        "struct { a int8; b int8; c int32 }{0, 0, 0}"u8
    )
}.slice());
internal static ref slice<pair> valueTests => ref ᏑvalueTests.ValueSlot;

internal static void testType(ж<Δtesting.T> Ꮡt, nint i, reflectꓸType typ, @string want) {
    @string s = typ.String();
    if (s != want) {
        Ꮡt.Errorf("#%d: have %#q, want %#q"u8, i, s, want);
    }
}

public static void TestTypes(ж<Δtesting.T> Ꮡt) {
    foreach (var (i, tt) in typeTests) {
        testType(Ꮡt, i, ValueOf(tt.i).Field(0).Type(), tt.s);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string stringyCheeseˢ = "stringy cheese"u8;

public static void TestSet(ж<Δtesting.T> Ꮡt) {
    foreach (var (i, tt) in valueTests) {
        var v = ValueOf(tt.i);
        v = v.Elem();
        var exprᴛ1 = v.Kind();
        if (exprᴛ1 == ΔInt) {
            v.SetInt(132);
        }
        else if (exprᴛ1 == Int8) {
            v.SetInt(8);
        }
        else if (exprᴛ1 == Int16) {
            v.SetInt(16);
        }
        else if (exprᴛ1 == Int32) {
            v.SetInt(32);
        }
        else if (exprᴛ1 == Int64) {
            v.SetInt(64);
        }
        else if (exprᴛ1 == ΔUint) {
            v.SetUint(132);
        }
        else if (exprᴛ1 == Uint8) {
            v.SetUint(8);
        }
        else if (exprᴛ1 == Uint16) {
            v.SetUint(16);
        }
        else if (exprᴛ1 == Uint32) {
            v.SetUint(32);
        }
        else if (exprᴛ1 == Uint64) {
            v.SetUint(64);
        }
        else if (exprᴛ1 == Float32) {
            v.SetFloat(256.25D);
        }
        else if (exprᴛ1 == Float64) {
            v.SetFloat(512.125D);
        }
        else if (exprᴛ1 == Complex64) {
            v.SetComplex(532.125D + 10D.i());
        }
        else if (exprᴛ1 == Complex128) {
            v.SetComplex(564.25D + 1D.i());
        }
        else if (exprᴛ1 == ΔString) {
            v.SetString(stringyCheeseˢ);
        }
        else if (exprᴛ1 == ΔBool) {
            v.SetBool(true);
        }

        @string s = valueToString(v);
        if (s != tt.s) {
            Ꮡt.Errorf("#%d: have %#q, want %#q"u8, i, s, tt.s);
        }
    }
}

public static void TestSetValue(ж<Δtesting.T> Ꮡt) {
    foreach (var (i, tt) in valueTests) {
        var v = ValueOf(tt.i).Elem();
        var exprᴛ1 = v.Kind();
        if (exprᴛ1 == ΔInt) {
            v.Set(ValueOf((nint)132));
        }
        else if (exprᴛ1 == Int8) {
            v.Set(ValueOf((int8)8));
        }
        else if (exprᴛ1 == Int16) {
            v.Set(ValueOf((int16)16));
        }
        else if (exprᴛ1 == Int32) {
            v.Set(ValueOf((int32)32));
        }
        else if (exprᴛ1 == Int64) {
            v.Set(ValueOf((int64)64));
        }
        else if (exprᴛ1 == ΔUint) {
            v.Set(ValueOf((nuint)132));
        }
        else if (exprᴛ1 == Uint8) {
            v.Set(ValueOf((uint8)8));
        }
        else if (exprᴛ1 == Uint16) {
            v.Set(ValueOf((uint16)16));
        }
        else if (exprᴛ1 == Uint32) {
            v.Set(ValueOf((uint32)32));
        }
        else if (exprᴛ1 == Uint64) {
            v.Set(ValueOf((uint64)64));
        }
        else if (exprᴛ1 == Float32) {
            v.Set(ValueOf((float32)256.25F));
        }
        else if (exprᴛ1 == Float64) {
            v.Set(ValueOf(512.125D));
        }
        else if (exprᴛ1 == Complex64) {
            v.Set(ValueOf((complex64)(532.125F + 10F.i())));
        }
        else if (exprᴛ1 == Complex128) {
            v.Set(ValueOf((complex128)(564.25D + 1D.i())));
        }
        else if (exprᴛ1 == ΔString) {
            v.Set(ValueOf(stringyCheeseˢ));
        }
        else if (exprᴛ1 == ΔBool) {
            v.Set(ValueOf(true));
        }

        @string s = valueToString(v);
        if (s != tt.s) {
            Ꮡt.Errorf("#%d: have %#q, want %#q"u8, i, s, tt.s);
        }
    }
}

public static void TestMapIterSet(ж<Δtesting.T> Ꮡt) {
    var m = new map<@string, any>(len(valueTests));
    foreach (var (_, tt) in valueTests) {
        m[tt.s] = tt.i;
    }
    ref var v = ref heap<reflectꓸValue>(out var Ꮡv);
    v = ValueOf(m);
    ref var k = ref heap<reflectꓸValue>(out var Ꮡk);
    k = New(v.Type().Key()).Elem();
    ref var e = ref heap<reflectꓸValue>(out var Ꮡe);
    e = New(v.Type().Elem()).Elem();
    var iter = v.MapRange();
    while (iter.Next()) {
        k.SetIterKey(iter);
        e.SetIterValue(iter);
        var wantΔ1 = m[k.String()];
        var gotΔ1 = e.Interface();
        if (!AreEqual(gotΔ1, wantΔ1)) {
            Ꮡt.Errorf("%q: want (%T) %v, got (%T) %v"u8, k.String(), wantΔ1, wantΔ1, gotΔ1, gotΔ1);
        }
        {
            @string setkey = valueToString(k);
            @string key = valueToString(iter.Key()); if (setkey != key) {
                Ꮡt.Errorf("MapIter.Key() = %q, MapIter.SetKey() = %q"u8, key, setkey);
            }
        }
        {
            @string setval = valueToString(e);
            @string val = valueToString(iter.Value()); if (setval != val) {
                Ꮡt.Errorf("MapIter.Value() = %q, MapIter.SetValue() = %q"u8, val, setval);
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
        Ꮡt.Errorf("wanted %d alloc, got %d"u8, want, got);
    }
}

[GoLocalName("integer")] [GoType("num:nint")] internal partial struct TestCanIntUintFloatComplex_integer;

[GoLocalName("uinteger")] [GoType("num:nuint")] internal partial struct TestCanIntUintFloatComplex_uinteger;

[GoLocalName("float")] [GoType("num:float64")] internal partial struct TestCanIntUintFloatComplex_float;

[GoLocalName("complex")] [GoType("num:complex128")] internal partial struct TestCanIntUintFloatComplex_complex;

[GoType("dyn")] internal partial struct TestCanIntUintFloatComplex_type {
    internal any i;
    internal array<bool> want = new(4);
}

[GoType("dyn")] internal partial struct TestCanIntUintFloatComplex_typeᴛ1 {
    internal nint i;
}

public static void TestCanIntUintFloatComplex(ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    array<@string> ops = new @string[]{"CanInt"u8, "CanUint"u8, "CanFloat"u8, "CanComplex"u8}.array();
// signed integer
// unsigned integer
// floating-point
// complex
// underlying
// not-acceptable
    slice<TestCanIntUintFloatComplex_type> testCases = new TestCanIntUintFloatComplex_type[]{
        new((nint)(132), new bool[]{true, false, false, false}.array()),
        new((int8)8, new bool[]{true, false, false, false}.array()),
        new((int16)16, new bool[]{true, false, false, false}.array()),
        new((int32)32, new bool[]{true, false, false, false}.array()),
        new((int64)64, new bool[]{true, false, false, false}.array()),
        new((nuint)132, new bool[]{false, true, false, false}.array()),
        new((uint8)8, new bool[]{false, true, false, false}.array()),
        new((uint16)16, new bool[]{false, true, false, false}.array()),
        new((uint32)32, new bool[]{false, true, false, false}.array()),
        new((uint64)64, new bool[]{false, true, false, false}.array()),
        new((uintptr)0xABCD, new bool[]{false, true, false, false}.array()),
        new((float32)256.25F, new bool[]{false, false, true, false}.array()),
        new((float64)512.125D, new bool[]{false, false, true, false}.array()),
        new((complex64)(532.125F + 10F.i()), new bool[]{false, false, false, true}.array()),
        new((complex128)(564.25D + 1D.i()), new bool[]{false, false, false, true}.array()),
        new(((TestCanIntUintFloatComplex_integer)(-132)), new bool[]{true, false, false, false}.array()),
        new(((TestCanIntUintFloatComplex_uinteger)132), new bool[]{false, true, false, false}.array()),
        new(((TestCanIntUintFloatComplex_float)256.25D), new bool[]{false, false, true, false}.array()),
        new(((TestCanIntUintFloatComplex_complex)(532.125D + 10D.i())), new bool[]{false, false, false, true}.array()),
        new((@string)"hello world"u8, new bool[]{false, false, false, false}.array()),
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
    }.slice();
    foreach (var (i, vᴛ1) in testCases) {
        var tc = vᴛ1.ΔClone();

        var v = ValueOf(tc.i);
        var got = new bool[]{v.CanInt(), v.CanUint(), v.CanFloat(), v.CanComplex()}.array();
        foreach (var (j, _) in tc.want) {
            if (got[j] != tc.want[j]) {
                Ꮡt.Errorf(
                    "#%d: v.%s() returned %t for type %T, want %t"u8,
                    i,
                    ops[j],
                    got[j],
                    tc.i,
                    tc.want[j]);
            }
        }
    }
}

[GoType("dyn")] internal partial struct TestCanSetField_embed {
    internal nint x;
    public nint X;
}

[GoType("dyn")] internal partial struct TestCanSetField_Embed {
    internal nint x;
    public nint X;
}

[GoType("dyn")] internal partial struct TestCanSetField_S1 {
    internal partial ref TestCanSetField_embed embed { get; }
    internal nint x;
    public nint X;
}

[GoType("dyn")] internal partial struct TestCanSetField_S2 {
    internal partial ref ж<TestCanSetField_embed> embed { get; }
    internal nint x;
    public nint X;
}

[GoType("dyn")] internal partial struct TestCanSetField_S3 {
    public partial ref TestCanSetField_Embed Embed { get; }
    internal nint x;
    public nint X;
}

[GoType("dyn")] internal partial struct TestCanSetField_S4 {
    public partial ref ж<TestCanSetField_Embed> Embed { get; }
    internal nint x;
    public nint X;
}

[GoType("dyn")] internal partial struct TestCanSetField_testCase {
    // -1 means Addr().Elem() of current value
    internal slice<nint> index;
    internal bool canSet;
}

[GoType("dyn")] internal partial struct TestCanSetField_tests {
    internal reflectꓸValue val;
    internal slice<TestCanSetField_testCase> cases;
}

public static void TestCanSetField(ж<Δtesting.T> Ꮡt) {
    var tests = new TestCanSetField_tests[]{new(
        val: ValueOf(Ꮡ(new TestCanSetField_S1(nil))),
        cases: new TestCanSetField_testCase[]{
            new(new nint[]{0}.slice(), false),
            new(new nint[]{0, -1}.slice(), false),
            new(new nint[]{0, 0}.slice(), false),
            new(new nint[]{0, 0, -1}.slice(), false),
            new(new nint[]{0, -1, 0}.slice(), false),
            new(new nint[]{0, -1, 0, -1}.slice(), false),
            new(new nint[]{0, 1}.slice(), true),
            new(new nint[]{0, 1, -1}.slice(), true),
            new(new nint[]{0, -1, 1}.slice(), true),
            new(new nint[]{0, -1, 1, -1}.slice(), true),
            new(new nint[]{1}.slice(), false),
            new(new nint[]{1, -1}.slice(), false),
            new(new nint[]{2}.slice(), true),
            new(new nint[]{2, -1}.slice(), true)
        }.slice()
    ), new(
        val: ValueOf(Ꮡ(new TestCanSetField_S2(embed: Ꮡ(new TestCanSetField_embed(nil))))),
        cases: new TestCanSetField_testCase[]{
            new(new nint[]{0}.slice(), false),
            new(new nint[]{0, -1}.slice(), false),
            new(new nint[]{0, 0}.slice(), false),
            new(new nint[]{0, 0, -1}.slice(), false),
            new(new nint[]{0, -1, 0}.slice(), false),
            new(new nint[]{0, -1, 0, -1}.slice(), false),
            new(new nint[]{0, 1}.slice(), true),
            new(new nint[]{0, 1, -1}.slice(), true),
            new(new nint[]{0, -1, 1}.slice(), true),
            new(new nint[]{0, -1, 1, -1}.slice(), true),
            new(new nint[]{1}.slice(), false),
            new(new nint[]{2}.slice(), true)
        }.slice()
    ), new(
        val: ValueOf(Ꮡ(new TestCanSetField_S3(nil))),
        cases: new TestCanSetField_testCase[]{
            new(new nint[]{0}.slice(), true),
            new(new nint[]{0, -1}.slice(), true),
            new(new nint[]{0, 0}.slice(), false),
            new(new nint[]{0, 0, -1}.slice(), false),
            new(new nint[]{0, -1, 0}.slice(), false),
            new(new nint[]{0, -1, 0, -1}.slice(), false),
            new(new nint[]{0, 1}.slice(), true),
            new(new nint[]{0, 1, -1}.slice(), true),
            new(new nint[]{0, -1, 1}.slice(), true),
            new(new nint[]{0, -1, 1, -1}.slice(), true),
            new(new nint[]{1}.slice(), false),
            new(new nint[]{2}.slice(), true)
        }.slice()
    ), new(
        val: ValueOf(Ꮡ(new TestCanSetField_S4(Embed: Ꮡ(new TestCanSetField_Embed(nil))))),
        cases: new TestCanSetField_testCase[]{
            new(new nint[]{0}.slice(), true),
            new(new nint[]{0, -1}.slice(), true),
            new(new nint[]{0, 0}.slice(), false),
            new(new nint[]{0, 0, -1}.slice(), false),
            new(new nint[]{0, -1, 0}.slice(), false),
            new(new nint[]{0, -1, 0, -1}.slice(), false),
            new(new nint[]{0, 1}.slice(), true),
            new(new nint[]{0, 1, -1}.slice(), true),
            new(new nint[]{0, -1, 1}.slice(), true),
            new(new nint[]{0, -1, 1, -1}.slice(), true),
            new(new nint[]{1}.slice(), false),
            new(new nint[]{2}.slice(), true)
        }.slice()
    )
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestCanSetField_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(tt.val.Type().Name(), (ж<Δtesting.T> tΔ1) => {
            foreach (var (_, tc) in ttʗ1.cases) {
                var f = ttʗ1.val;
                foreach (var (_, i) in tc.index) {
                    if (f.Kind() == ΔPointer) {
                        f = f.Elem();
                    }
                    if (i == -1){
                        f = f.Addr().Elem();
                    } else {
                        f = f.Field(i);
                    }
                }
                {
                    var got = f.CanSet(); if (got != tc.canSet) {
                        tΔ1.Errorf("CanSet() = %v, want %v"u8, got, tc.canSet);
                    }
                }
            }
        });
    }
}

internal static ж<nint> Ꮡ_i = new StandardBox<nint>(7);
internal static ref nint _i => ref Ꮡ_i.Value;

internal static slice<pair> valueToStringTests = new pair[]{
    new((nint)(123), "123"u8),
    new(123.5D, "123.5"u8),
    new((byte)123, "123"u8),
    new((@string)"abc"u8, "abc"u8),
    new(new T(123, 456.75D, "hello"u8, Ꮡ_i), "reflect_test.T{123, 456.75, hello, *int(&7)}"u8),
    new(@new<channel<ж<T>>>(), "*chan *reflect_test.T(&chan *reflect_test.T)"u8),
    new(new nint[]{1, 2, 3, 4, 5, 6, 7, 8, 9, 10}.array(), "[10]int{1, 2, 3, 4, 5, 6, 7, 8, 9, 10}"u8),
    new(Ꮡ(new nint[]{1, 2, 3, 4, 5, 6, 7, 8, 9, 10}.array()), "*[10]int(&[10]int{1, 2, 3, 4, 5, 6, 7, 8, 9, 10})"u8),
    new(new nint[]{1, 2, 3, 4, 5, 6, 7, 8, 9, 10}.slice(), "[]int{1, 2, 3, 4, 5, 6, 7, 8, 9, 10}"u8),
    new(Ꮡ(new nint[]{1, 2, 3, 4, 5, 6, 7, 8, 9, 10}.slice()), "*[]int(&[]int{1, 2, 3, 4, 5, 6, 7, 8, 9, 10})"u8)
}.slice();

public static void TestValueToString(ж<Δtesting.T> Ꮡt) {
    foreach (var (i, test) in valueToStringTests) {
        @string s = valueToString(ValueOf(test.i));
        if (s != test.s) {
            Ꮡt.Errorf("#%d: have %#q, want %#q"u8, i, s, test.s);
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string wantᶜ = "[10]int{1, 2, 3, 4, 123, 6, 7, 8, 9, 10}"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string want1ᶜ = "[]int{1, 2, 3, 4, 123, 6, 7, 8, 9, 10}"u8;

public static void TestArrayElemSet(ж<Δtesting.T> Ꮡt) {
    var v = ValueOf(Ꮡ(new nint[]{1, 2, 3, 4, 5, 6, 7, 8, 9, 10}.array())).Elem();
    v.Index(4).SetInt(123);
    @string s = valueToString(v);
    @string want = wantᶜ;
    if (s != want) {
        Ꮡt.Errorf("[10]int: have %#q want %#q"u8, s, want);
    }
    v = ValueOf(new nint[]{1, 2, 3, 4, 5, 6, 7, 8, 9, 10}.slice());
    v.Index(4).SetInt(123);
    s = valueToString(v);
    @string want1 = want1ᶜ;
    if (s != want1) {
        Ꮡt.Errorf("[]int: have %#q want %#q"u8, s, want1);
    }
}

public static void TestPtrPointTo(ж<Δtesting.T> Ꮡt) {
    ref var ip = ref heap<ж<int32>>(out var Ꮡip);
    ref var i = ref heap(new int32(), out var Ꮡi);
    i = 1234;
    var vip = ValueOf(Ꮡip);
    var vi = ValueOf(Ꮡi).Elem();
    vip.Elem().Set(vi.Addr());
    if (ip.Value != 1234) {
        Ꮡt.Errorf("got %d, want 1234"u8, ip.Value);
    }
    ip = default!;
    var vp = ValueOf(Ꮡip).Elem();
    vp.Set(Zero(vp.Type()));
    if (ip != nil) {
        Ꮡt.Errorf("got non-nil (%p), want nil"u8, ip.OrTypedNil());
    }
}

public static void TestPtrSetNil(ж<Δtesting.T> Ꮡt) {
    ref var i = ref heap(new int32(), out var Ꮡi);
    i = 1234;
    ref var ip = ref heap<ж<int32>>(out var Ꮡip);
    ip = Ꮡi;
    var vip = ValueOf(Ꮡip);
    vip.Elem().Set(Zero(vip.Elem().Type()));
    if (ip != nil) {
        Ꮡt.Errorf("got non-nil (%d), want nil"u8, ip.Value);
    }
}

public static void TestMapSetNil(ж<Δtesting.T> Ꮡt) {
    ref var m = ref heap<map<@string, nint>>(out var Ꮡm);
    m = new map<@string, nint>();
    var vm = ValueOf(Ꮡm);
    vm.Elem().Set(Zero(vm.Elem().Type()));
    if (m != default!) {
        Ꮡt.Errorf("got non-nil (%p), want nil"u8, m);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string int8ˢ = "int8"u8;
internal static readonly @string structCChanInt32DFloat32ˢ = "*struct { c chan *int32; d float32 }"u8;
internal static readonly @string structCChanInt32DFloat32ˢ2 = "struct { c chan *int32; d float32 }"u8;
internal static readonly @string chanInt32ˢ = "chan *int32"u8;
internal static readonly @string float32ˢ = "float32"u8;
internal static readonly @string absentˢ = "absent"u8;
internal static readonly @string int32ˢ = "[32]int32"u8;
internal static readonly @string int32ˢ2 = "int32"u8;
internal static readonly @string mapStringInt32ˢ = "map[string]*int32"u8;
internal static readonly @string stringˢ = "string"u8;
internal static readonly @string int32ˢ3 = "*int32"u8;
internal static readonly @string chanStringˢ = "chan<- string"u8;
internal static readonly @string uint32ˢ = "[]uint32"u8;

[GoType("dyn")] internal partial struct TestAll_i {
    [GoTag(@"reflect:""TAG""")]
    internal slice<uint32> d;
}

public static void TestAll(ж<Δtesting.T> Ꮡt) {
    testType(Ꮡt, 1, TypeOf((int8)0), int8ˢ);
    testType(Ꮡt, 2, TypeOf(((ж<int8>)nil)).Elem(), int8ˢ);
    var typ = TypeOf(((ж<typeᴛ22_x>)nil));
    testType(Ꮡt, 3, typ, structCChanInt32DFloat32ˢ);
    var etyp = typ.Elem();
    testType(Ꮡt, 4, etyp, structCChanInt32DFloat32ˢ2);
    var styp = etyp;
    var f = styp.Field(0);
    testType(Ꮡt, 5, f.Type, chanInt32ˢ);
    (f, var present) = styp.FieldByName("d"u8);
    if (!present) {
        Ꮡt.Errorf("FieldByName says present field is absent"u8);
    }
    testType(Ꮡt, 6, f.Type, float32ˢ);
    (f, present) = styp.FieldByName(absentˢ);
    if (present) {
        Ꮡt.Errorf("FieldByName says absent field is present"u8);
    }
    typ = TypeOf(new int32[]{}.array(32));
    testType(Ꮡt, 7, typ, int32ˢ);
    testType(Ꮡt, 8, typ.Elem(), int32ˢ2);
    typ = TypeOf(((map<@string, ж<int32>>)default!));
    testType(Ꮡt, 9, typ, mapStringInt32ˢ);
    var mtyp = typ;
    testType(Ꮡt, 10, mtyp.Key(), stringˢ);
    testType(Ꮡt, 11, mtyp.Elem(), int32ˢ3);
    typ = TypeOf(channel/*<-*/<@string>.SendOnly);
    testType(Ꮡt, 12, typ, chanStringˢ);
    testType(Ꮡt, 13, typ.Elem(), stringˢ);
    // make sure tag strings are not part of element type
    typ = TypeOf(new TestAll_i()).Field(0).Type;
    testType(Ꮡt, 14, typ, uint32ˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string interfaceˢ = "interface {}"u8;
internal static readonly @string float64ˢ = "float64"u8;

[GoType("dyn")] internal partial struct TestInterfaceGet_inter {
    public any E;
}

public static void TestInterfaceGet(ж<Δtesting.T> Ꮡt) {
    ref var inter = ref heap(new TestInterfaceGet_inter(), out var Ꮡinter);
    inter.E = 123.456D;
    var v1 = ValueOf(Ꮡinter);
    var v2 = v1.Elem().Field(0);
    assert(Ꮡt, v2.Type().String(), interfaceˢ);
    var i2 = v2.Interface();
    var v3 = ValueOf(i2);
    assert(Ꮡt, v3.Type().String(), float64ˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object v2InterfaceDidNotReturnˢ = (@string)"v2.Interface() did not return float64, got "u8;

[GoType("dyn")] internal partial struct TestInterfaceValue_inter {
    public any E;
}

public static void TestInterfaceValue(ж<Δtesting.T> Ꮡt) {
    ref var inter = ref heap(new TestInterfaceValue_inter(), out var Ꮡinter);
    inter.E = 123.456D;
    var v1 = ValueOf(Ꮡinter);
    var v2 = v1.Elem().Field(0);
    assert(Ꮡt, v2.Type().String(), interfaceˢ);
    var v3 = v2.Elem();
    assert(Ꮡt, v3.Type().String(), float64ˢ);
    var i3 = v2.Interface();
    {
        var (_, ok) = i3._<float64>(ᐧ); if (!ok) {
            Ꮡt.Error(v2InterfaceDidNotReturnˢ, TypeOf(i3));
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string funcˢ = "func()"u8;

public static void TestFunctionValue(ж<Δtesting.T> Ꮡt) {
    any x = () => {
    };
    var v = ValueOf(x);
    if (fmt.Sprint(v.Interface()) != fmt.Sprint(x)) {
        Ꮡt.Fatalf("TestFunction returned wrong pointer"u8);
    }
    assert(Ꮡt, v.Type().String(), funcˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string reflectValueGrowUsingˢ = "reflect.Value.Grow using unaddressable value"u8;
internal static readonly @string appendˢ = "Append"u8;
internal static readonly @string rateˢ = "Rate"u8;
internal static readonly @string zeroCapacityˢ = "ZeroCapacity"u8;

public static void TestGrow(ж<Δtesting.T> Ꮡt) {
    ref var v = ref heap<reflectꓸValue>(out var Ꮡv);
    v = ValueOf(slice<nint>(default!));
    shouldPanic(reflectValueGrowUsingˢ, () => {
        Ꮡv.Value.Grow(0);
    });
    v = ValueOf(@new<slice<nint>>()).Elem();
    v.Grow(0);
    if (!v.IsNil()) {
        Ꮡt.Errorf("v.Grow(0) should still be nil"u8);
    }
    v.Grow(1);
    if (v.Cap() == 0) {
        Ꮡt.Errorf("v.Cap = %v, want non-zero"u8, v.Cap());
    }
    @unsafe.Pointer want = (uintptr)v.UnsafePointer();
    v.Grow(1);
    @unsafe.Pointer got = (uintptr)v.UnsafePointer();
    if (got != want) {
        Ꮡt.Errorf("noop v.Grow should not change pointers"u8);
    }
    Ꮡt.Run(appendˢ, (ж<Δtesting.T> tΔ1) => {
        ref var gotΔ1 = ref heap<slice<T>>(out var ᏑgotΔ1);
        slice<T> wantΔ1 = default!;
        ref var vΔ1 = ref heap<reflectꓸValue>(out var ᏑvΔ1);
        vΔ1 = ValueOf(ᏑgotΔ1).Elem();
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
            tΔ1.Errorf("value mismatch:\ngot  %v\nwant %v"u8, ᏑgotΔ1.ValueSlot, wantΔ1);
        }
    });
    Ꮡt.Run(rateˢ, (ж<Δtesting.T> tΔ2) => {
        slice<byte> b = default!;
        var vΔ2 = ValueOf(@new<slice<byte>>()).Elem();
        for (nint i = 0; i < 10; i++) {
            b = appendꓸꓸꓸ(b.slice(0, cap(b)), makeꓸꓸꓸ<byte>(1));
            vΔ2.SetLen(vΔ2.Cap());
            vΔ2.Grow(1);
            if (vΔ2.Cap() != cap(b)) {
                tΔ2.Errorf("v.Cap = %v, want %v"u8, vΔ2.Cap(), cap(b));
            }
        }
    });
    Ꮡt.Run(zeroCapacityˢ, (ж<Δtesting.T> tΔ3) => {
        for (nint i = 0; i < 10; i++) {
            var vΔ3 = ValueOf(@new<slice<byte>>()).Elem();
            vΔ3.Grow(61);
            var b = vΔ3.Bytes();
            b = b.slice(0, cap(b));
            foreach (var (iΔ1, c) in b) {
                if (c != 0) {
                    tΔ3.Fatalf("Value.Bytes[%d] = 0x%02x, want 0x00"u8, iΔ1, c);
                }
                b[iΔ1] = 0xff;
            }
            Δruntime.GC();
        }
    });
}


[GoType("dyn")] partial struct appendTestsᴛ1 {
    internal slice<nint> orig, extra;
}
internal static slice<appendTestsᴛ1> appendTests = new appendTestsᴛ1[]{
    new(default!, default!),
    new(new nint[]{}.slice(), default!),
    new(default!, new nint[]{}.slice()),
    new(new nint[]{}.slice(), new nint[]{}.slice()),
    new(default!, new nint[]{22}.slice()),
    new(new nint[]{}.slice(), new nint[]{22}.slice()),
    new(new slice<nint>(2, 4), default!),
    new(new slice<nint>(2, 4), new nint[]{}.slice()),
    new(new slice<nint>(2, 4), new nint[]{22}.slice()),
    new(new slice<nint>(2, 4), new nint[]{22, 33, 44}.slice())
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string usingUnexportedFieldˢ = "using unexported field"u8;

[GoType("dyn")] internal partial struct TestAppend_i {
    internal slice<nint> x;
}

public static void TestAppend(ж<Δtesting.T> Ꮡt) {
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
        ref var e1 = ref heap<reflectꓸValue>(out var Ꮡe1);
        e1 = ValueOf(test.extra);
        // Test Append.
        var a0 = ValueOf(Ꮡtest.of(appendTestsᴛ1.Ꮡorig)).Elem();
        var have0 = Append(a0, e0.ꓸꓸꓸ);
        if (have0.CanAddr()) {
            Ꮡt.Errorf("Append #%d: have slice should not be addressable"u8, i);
        }
        if (!DeepEqual(have0.Interface(), want)) {
            Ꮡt.Errorf("Append #%d: have %v, want %v (%p %p)"u8, i, have0, want, test.orig, have0.Interface());
        }
        // Check that the orig and extra slices were not modified.
        if (a0.Len() != len(test.orig)) {
            Ꮡt.Errorf("Append #%d: a0.Len: have %d, want %d"u8, i, a0.Len(), origLen);
        }
        if (len(test.orig) != origLen) {
            Ꮡt.Errorf("Append #%d origLen: have %v, want %v"u8, i, len(test.orig), origLen);
        }
        if (len(test.extra) != extraLen) {
            Ꮡt.Errorf("Append #%d extraLen: have %v, want %v"u8, i, len(test.extra), extraLen);
        }
        // Test AppendSlice.
        var a1 = ValueOf(Ꮡtest.of(appendTestsᴛ1.Ꮡorig)).Elem();
        var have1 = AppendSlice(a1, e1);
        if (have1.CanAddr()) {
            Ꮡt.Errorf("AppendSlice #%d: have slice should not be addressable"u8, i);
        }
        if (!DeepEqual(have1.Interface(), want)) {
            Ꮡt.Errorf("AppendSlice #%d: have %v, want %v"u8, i, have1, want);
        }
        // Check that the orig and extra slices were not modified.
        if (a1.Len() != len(test.orig)) {
            Ꮡt.Errorf("AppendSlice #%d: a1.Len: have %d, want %d"u8, i, a0.Len(), origLen);
        }
        if (len(test.orig) != origLen) {
            Ꮡt.Errorf("AppendSlice #%d origLen: have %v, want %v"u8, i, len(test.orig), origLen);
        }
        if (len(test.extra) != extraLen) {
            Ꮡt.Errorf("AppendSlice #%d extraLen: have %v, want %v"u8, i, len(test.extra), extraLen);
        }
        // Test Append and AppendSlice with unexported value.
        ref var ax = ref heap<reflectꓸValue>(out var Ꮡax);
        ax = ValueOf(new TestAppend_i(test.orig)).Field(0);
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

public static void TestCopy(ж<Δtesting.T> Ꮡt) {
    var a = new nint[]{1, 2, 3, 4, 10, 9, 8, 7}.slice();
    var b = new nint[]{11, 22, 33, 44, 1010, 99, 88, 77, 66, 55, 44}.slice();
    var c = new nint[]{11, 22, 33, 44, 1010, 99, 88, 77, 66, 55, 44}.slice();
    for (nint i = 0; i < len(b); i++) {
        if (b[i] != c[i]) {
            Ꮡt.Fatalf("b != c before test"u8);
        }
    }
    ref var a1 = ref heap<slice<nint>>(out var Ꮡa1);
    a1 = a;
    ref var b1 = ref heap<slice<nint>>(out var Ꮡb1);
    b1 = b;
    var aa = ValueOf(Ꮡa1).Elem();
    var ab = ValueOf(Ꮡb1).Elem();
    for (nint tocopy = 1; tocopy <= 7; tocopy++) {
        aa.SetLen(tocopy);
        Copy(ab, aa);
        aa.SetLen(8);
        for (nint i = 0; i < tocopy; i++) {
            if (a[i] != b[i]) {
                Ꮡt.Errorf("(i) tocopy=%d a[%d]=%d, b[%d]=%d"u8,
                    tocopy, i, a[i], i, b[i]);
            }
        }
        for (nint i = tocopy; i < len(b); i++) {
            if (b[i] != c[i]){
                if (i < len(a)){
                    Ꮡt.Errorf("(ii) tocopy=%d a[%d]=%d, b[%d]=%d, c[%d]=%d"u8,
                        tocopy, i, a[i], i, b[i], i, c[i]);
                } else {
                    Ꮡt.Errorf("(iii) tocopy=%d b[%d]=%d, c[%d]=%d"u8,
                        tocopy, i, b[i], i, c[i]);
                }
            } else {
                Ꮡt.Logf("tocopy=%d elem %d is okay\n"u8, tocopy, i);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string sliceˢ = "Slice"u8;
internal static readonly @string helloˢ = "hello"u8;
internal static readonly object helloworldˢ = (@string)"helloworld"u8;
internal static readonly @string arrayˢ = "Array"u8;

public static void TestCopyString(ж<Δtesting.T> Ꮡt) {
    Ꮡt.Run(sliceˢ, (ж<Δtesting.T> tΔ1) => {
        var s = bytes.Repeat(new byte[]{(rune)'_'}.slice(), 8);
        var val = ValueOf(s);
        nint n = Copy(val, ValueOf((@string)""u8));
        {
            var expecting = slice<byte>("________"u8); if (n != 0 || !bytes.Equal(s, expecting)) {
                tΔ1.Errorf("got n = %d, s = %s, expecting n = 0, s = %s"u8, n, s, expecting);
            }
        }
        n = Copy(val, ValueOf(helloˢ));
        {
            var expecting = slice<byte>("hello___"u8); if (n != 5 || !bytes.Equal(s, expecting)) {
                tΔ1.Errorf("got n = %d, s = %s, expecting n = 5, s = %s"u8, n, s, expecting);
            }
        }
        n = Copy(val, ValueOf(helloworldˢ));
        {
            var expecting = slice<byte>("hellowor"u8); if (n != 8 || !bytes.Equal(s, expecting)) {
                tΔ1.Errorf("got n = %d, s = %s, expecting n = 8, s = %s"u8, n, s, expecting);
            }
        }
    });
    Ꮡt.Run(arrayˢ, (ж<Δtesting.T> tΔ2) => {
        ref var s = ref heap<array<byte>>(out var Ꮡs);
        Ꮡs.Value = new byte[]{(rune)'_', (rune)'_', (rune)'_', (rune)'_', (rune)'_', (rune)'_', (rune)'_', (rune)'_'}.array();
        var val = ValueOf(Ꮡs).Elem();
        nint n = Copy(val, ValueOf((@string)""u8));
        {
            var expecting = slice<byte>("________"u8); if (n != 0 || !bytes.Equal(Ꮡs.Value[..], expecting)) {
                tΔ2.Errorf("got n = %d, s = %s, expecting n = 0, s = %s"u8, n, Ꮡs.Value[..], expecting);
            }
        }
        n = Copy(val, ValueOf(helloˢ));
        {
            var expecting = slice<byte>("hello___"u8); if (n != 5 || !bytes.Equal(Ꮡs.Value[..], expecting)) {
                tΔ2.Errorf("got n = %d, s = %s, expecting n = 5, s = %s"u8, n, Ꮡs.Value[..], expecting);
            }
        }
        n = Copy(val, ValueOf(helloworldˢ));
        {
            var expecting = slice<byte>("hellowor"u8); if (n != 8 || !bytes.Equal(Ꮡs.Value[..], expecting)) {
                tΔ2.Errorf("got n = %d, s = %s, expecting n = 8, s = %s"u8, n, Ꮡs.Value[..], expecting);
            }
        }
    });
}

public static void TestCopyArray(ж<Δtesting.T> Ꮡt) {
    ref var a = ref heap<array<nint>>(out var Ꮡa);
    a = new nint[]{1, 2, 3, 4, 10, 9, 8, 7}.array();
    ref var b = ref heap<array<nint>>(out var Ꮡb);
    b = new nint[]{11, 22, 33, 44, 1010, 99, 88, 77, 66, 55, 44}.array();
    var c = b.Clone();
    var aa = ValueOf(Ꮡa).Elem();
    var ab = ValueOf(Ꮡb).Elem();
    Copy(ab, aa);
    for (nint i = 0; i < len(a); i++) {
        if (a[i] != b[i]) {
            Ꮡt.Errorf("(i) a[%d]=%d, b[%d]=%d"u8, i, a[i], i, b[i]);
        }
    }
    for (nint i = len(a); i < len(b); i++) {
        if (b[i] != c[i]){
            Ꮡt.Errorf("(ii) b[%d]=%d, c[%d]=%d"u8, i, b[i], i, c[i]);
        } else {
            Ꮡt.Logf("elem %d is okay\n"u8, i);
        }
    }
}

[GoType("dyn")] internal partial struct TestBigUnnamedStruct_b {
    internal int64 a, b, c, d;
}

public static void TestBigUnnamedStruct(ж<Δtesting.T> Ꮡt) {
    var b = new TestBigUnnamedStruct_b(1, 2, 3, 4);
    var v = ValueOf(b);
    var b1 = v.Interface()._<TestBigUnnamedStruct_b>();
    if (b1.a != b.a || b1.b != b.b || b1.c != b.c || b1.d != b.d) {
        Ꮡt.Errorf("ValueOf(%v).Interface().(*Big) = %v"u8, b, b1);
    }
}

[GoType] partial struct big {
    internal int64 a, b, c, d, e;
}

public static void TestBigStruct(ж<Δtesting.T> Ꮡt) {
    var b = new big(1, 2, 3, 4, 5);
    var v = ValueOf(b);
    var b1 = v.Interface()._<big>();
    if (b1.a != b.a || b1.b != b.b || b1.c != b.c || b1.d != b.d || b1.e != b.e) {
        Ꮡt.Errorf("ValueOf(%v).Interface().(big) = %v"u8, b, b1);
    }
}

[GoType] partial struct Basic {
    internal nint x;
    internal float32 y;
}

[GoType("Basic")] partial struct NotBasic;

[GoType] partial struct DeepEqualTest {
    internal any a, b;
    internal bool eq;
}

// Simple functions for DeepEqual tests.
internal static Action fn1;         // nil.

internal static Action fn2;         // nil.

internal static Action fn3 = () => {
    fn1();
}; // Not nil.

[GoType] partial struct self {
}

[GoType("ж<Loop>")] partial class Loop;
// Descriptor carrier for `Loopy` — uninhabited; see GoDescriptorTypeAttribute.
[GoLocalName("Loopy")] public interface Loopyᴅ { }


internal static ж<Loop> Ꮡloop1 = new StandardBox<Loop>(default(Loop));
internal static ref Loop loop1 => ref Ꮡloop1.ValueSlot;
internal static ж<Loop> Ꮡloop2 = new StandardBox<Loop>(default(Loop));
internal static ref Loop loop2 => ref Ꮡloop2.ValueSlot;

internal static ж<Loopy> Ꮡloopy1 = new StandardBox<Loopy>(default(Loopy));
internal static ref Loopy loopy1 => ref Ꮡloopy1.ValueSlot;
internal static ж<Loopy> Ꮡloopy2 = new StandardBox<Loopy>(default(Loopy));
internal static ref Loopy loopy2 => ref Ꮡloopy2.ValueSlot;

internal static ж<map<@string, any>> ᏑcycleMap1 = new StandardBox<map<@string, any>>(default(map<@string, any>));
internal static ref map<@string, any> cycleMap1 => ref ᏑcycleMap1.ValueSlot;
internal static ж<map<@string, any>> ᏑcycleMap2 = new StandardBox<map<@string, any>>(default(map<@string, any>));
internal static ref map<@string, any> cycleMap2 => ref ᏑcycleMap2.ValueSlot;
internal static ж<map<@string, any>> ᏑcycleMap3 = new StandardBox<map<@string, any>>(default(map<@string, any>));
internal static ref map<@string, any> cycleMap3 => ref ᏑcycleMap3.ValueSlot;

[GoType] partial struct structWithSelfPtr {
    internal ж<structWithSelfPtr> p;
    internal @string s;
}

[GoInit] internal static void init() {
    loop1 = Ꮡloop2;
    loop2 = Ꮡloop1;
    loopy1 = Ꮡloopy2;
    loopy2 = Ꮡloopy1;
    cycleMap1 = new map<@string, any>{};
    cycleMap1["cycle"u8] = cycleMap1;
    cycleMap2 = new map<@string, any>{};
    cycleMap2["cycle"u8] = cycleMap2;
    cycleMap3 = new map<@string, any>{};
    cycleMap3["different"u8] = cycleMap3;
}

// Equalities
// Inequalities
// Fun with floating point.
// Nil vs empty: not the same.
// Mismatched types
// Possible loops.
internal static slice<DeepEqualTest> deepEqualTests = new DeepEqualTest[]{
    new(default!, default!, true),
    new((nint)(1), (nint)(1), true),
    new((int32)1, (int32)1, true),
    new(0.5D, 0.5D, true),
    new((float32)0.5F, (float32)0.5F, true),
    new((@string)"hello"u8, (@string)"hello"u8, true),
    new(new slice<nint>(10), new slice<nint>(10), true),
    new(Ꮡ(new nint[]{1, 2, 3}.array()), Ꮡ(new nint[]{1, 2, 3}.array()), true),
    new(new Basic(1, 0.5F), new Basic(1, 0.5F), true),
    new(((error)default!), ((error)default!), true),
    new(new map<nint, @string>{[1] = "one"u8, [2] = "two"u8}, new map<nint, @string>{[2] = "two"u8, [1] = "one"u8}, true),
    new((fn1).OrTypedNilFunc(), (fn2).OrTypedNilFunc(), true),
    new(new byte[]{1, 2, 3}.slice(), new byte[]{1, 2, 3}.slice(), true),
    new(new MyByte[]{1, 2, 3}.slice(), new MyByte[]{1, 2, 3}.slice(), true),
    new(new MyBytes(new byte[]{1, 2, 3}.slice()), new MyBytes(new byte[]{1, 2, 3}.slice()), true),
    new((nint)(1), (nint)(2), false),
    new((int32)1, (int32)2, false),
    new(0.5D, 0.6D, false),
    new((float32)0.5F, (float32)0.6F, false),
    new((@string)"hello"u8, (@string)"hey"u8, false),
    new(new slice<nint>(10), new slice<nint>(11), false),
    new(Ꮡ(new nint[]{1, 2, 3}.array()), Ꮡ(new nint[]{1, 2, 4}.array()), false),
    new(new Basic(1, 0.5F), new Basic(1, 0.6F), false),
    new(new Basic(1, 0F), new Basic(2, 0F), false),
    new(new map<nint, @string>{[1] = "one"u8, [3] = "two"u8}, new map<nint, @string>{[2] = "two"u8, [1] = "one"u8}, false),
    new(new map<nint, @string>{[1] = "one"u8, [2] = "txo"u8}, new map<nint, @string>{[2] = "two"u8, [1] = "one"u8}, false),
    new(new map<nint, @string>{[1] = "one"u8}, new map<nint, @string>{[2] = "two"u8, [1] = "one"u8}, false),
    new(new map<nint, @string>{[2] = "two"u8, [1] = "one"u8}, new map<nint, @string>{[1] = "one"u8}, false),
    new(default!, (nint)(1), false),
    new((nint)(1), default!, false),
    new((fn1).OrTypedNilFunc(), (fn3).OrTypedNilFunc(), false),
    new((fn3).OrTypedNilFunc(), (fn3).OrTypedNilFunc(), false),
    new(new slice<nint>[]{new nint[]{1}.slice()}.slice(), new slice<nint>[]{new nint[]{2}.slice()}.slice(), false),
    new(Ꮡ(new structWithSelfPtr(p: Ꮡ(new structWithSelfPtr(s: "a"u8)))), Ꮡ(new structWithSelfPtr(p: Ꮡ(new structWithSelfPtr(s: "b"u8)))), false),
    new(Δmath.NaN(), Δmath.NaN(), false),
    new(Ꮡ(new float64[]{Δmath.NaN()}.array()), Ꮡ(new float64[]{Δmath.NaN()}.array()), false),
    new(Ꮡ(new float64[]{Δmath.NaN()}.array()), new self(nil), true),
    new(new float64[]{Δmath.NaN()}.slice(), new float64[]{Δmath.NaN()}.slice(), false),
    new(new float64[]{Δmath.NaN()}.slice(), new self(nil), true),
    new(new map<float64, float64>{[Δmath.NaN()] = 1D}, new map<float64, float64>{[1D] = 2D}, false),
    new(new map<float64, float64>{[Δmath.NaN()] = 1D}, new self(nil), true),
    new(new nint[]{}.slice(), slice<nint>(default!), false),
    new(new nint[]{}.slice(), new nint[]{}.slice(), true),
    new(slice<nint>(default!), slice<nint>(default!), true),
    new(new map<nint, nint>{}, ((map<nint, nint>)default!), false),
    new(new map<nint, nint>{}, new map<nint, nint>{}, true),
    new(((map<nint, nint>)default!), ((map<nint, nint>)default!), true),
    new((nint)(1), 1.0D, false),
    new((int32)1, (int64)1, false),
    new(0.5D, (@string)"hello"u8, false),
    new(new nint[]{1, 2, 3}.slice(), new nint[]{1, 2, 3}.array(), false),
    new(Ꮡ(new any[]{(nint)(1), (nint)(2), (nint)(4)}.array()), Ꮡ(new any[]{(nint)(1), (nint)(2), (@string)"s"u8}.array()), false),
    new(new Basic(1, 0.5F), new NotBasic(new Basic(1, 0.5F)), false),
    new(new map<nuint, @string>{[1] = "one"u8, [2] = "two"u8}, new map<nint, @string>{[2] = "two"u8, [1] = "one"u8}, false),
    new(new byte[]{1, 2, 3}.slice(), new MyByte[]{1, 2, 3}.slice(), false),
    new(new MyByte[]{1, 2, 3}.slice(), new MyBytes(new byte[]{1, 2, 3}.slice()), false),
    new(new byte[]{1, 2, 3}.slice(), new MyBytes(new byte[]{1, 2, 3}.slice()), false),
    new(Ꮡloop1, Ꮡloop1, true),
    new(Ꮡloop1, Ꮡloop2, true),
    new(Ꮡloopy1, Ꮡloopy1, true),
    new(Ꮡloopy1, Ꮡloopy2, true),
    new(ᏑcycleMap1, ᏑcycleMap2, true),
    new(ᏑcycleMap1, ᏑcycleMap3, false)
}.slice();

public static void TestDeepEqual(ж<Δtesting.T> Ꮡt) {
    foreach (var (i, vᴛ1) in deepEqualTests) {
        ref var test = ref heap(new DeepEqualTest(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        Ꮡt.Run(fmt.Sprint(i), (ж<Δtesting.T> tΔ1) => {
            if (AreEqual(testʗ1.b, (new self(nil)))) {
                testʗ1.b = testʗ1.a;
            }
            {
                var r = DeepEqual(testʗ1.a, testʗ1.b); if (r != testʗ1.eq) {
                    tΔ1.Errorf("DeepEqual(%#v, %#v) = %v, want %v"u8, testʗ1.a, testʗ1.b, r, testʗ1.eq);
                }
            }
        });
    }
}

public static void TestTypeOf(ж<Δtesting.T> Ꮡt) {
    // Special case for nil
    {
        var typ = TypeOf(default!); if (typ != default!) {
            Ꮡt.Errorf("expected nil type for nil value; got %v"u8, typ);
        }
    }
    foreach (var (_, test) in deepEqualTests) {
        var v = ValueOf(test.a);
        if (!v.IsValid()) {
            continue;
        }
        var typ = TypeOf(test.a);
        if (!AreEqual(typ, v.Type())) {
            Ꮡt.Errorf("TypeOf(%v) = %v, but ValueOf(%v).Type() = %v"u8, test.a, typ, test.a, v.Type());
        }
    }
}

[GoType] partial struct Recursive {
    internal nint x;
    internal ж<Recursive> r;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object deepEqualRecursiveSameˢ = (@string)"DeepEqual(recursive same) = false, want true"u8;

public static void TestDeepEqualRecursiveStruct(ж<Δtesting.T> Ꮡt) {
    var (a, b) = (@new<Recursive>(), @new<Recursive>());
    a.Value = new Recursive(12, a);
    b.Value = new Recursive(12, b);
    if (!DeepEqual(a.OrTypedNil(), b.OrTypedNil())) {
        Ꮡt.Error(deepEqualRecursiveSameˢ);
    }
}

[GoType] partial struct _Complex {
    internal nint a;
    internal array<ж<_Complex>> b = new(3);
    internal ж<@string> c;
    internal map<float64, float64> d;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object deepEqualComplexSameˢ = (@string)"DeepEqual(complex same) = false, want true"u8;

public static void TestDeepEqualComplexStruct(ж<Δtesting.T> Ꮡt) {
    var m = new map<float64, float64>();
    ref var stra = ref heap<@string>(out var Ꮡstra);
    stra = helloˢ;
    ref var strb = ref heap<@string>(out var Ꮡstrb);
    strb = helloˢ;
    var (a, b) = (@new<_Complex>(), @new<_Complex>());
    a.Value = new _Complex(5, new ж<_Complex>[]{a, b, a}.array(), Ꮡstra, m);
    b.Value = new _Complex(5, new ж<_Complex>[]{b, a, a}.array(), Ꮡstrb, m);
    if (!DeepEqual(a.OrTypedNil(), b.OrTypedNil())) {
        Ꮡt.Error(deepEqualComplexSameˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string hellooˢ = "helloo"u8;
internal static readonly object deepEqualComplexˢ = (@string)"DeepEqual(complex different) = true, want false"u8;

public static void TestDeepEqualComplexStructInequality(ж<Δtesting.T> Ꮡt) {
    var m = new map<float64, float64>();
    ref var stra = ref heap<@string>(out var Ꮡstra);
    stra = helloˢ;
    ref var strb = ref heap<@string>(out var Ꮡstrb);
    strb = hellooˢ; // Difference is here
    var (a, b) = (@new<_Complex>(), @new<_Complex>());
    a.Value = new _Complex(5, new ж<_Complex>[]{a, b, a}.array(), Ꮡstra, m);
    b.Value = new _Complex(5, new ж<_Complex>[]{b, a, a}.array(), Ꮡstrb, m);
    if (DeepEqual(a.OrTypedNil(), b.OrTypedNil())) {
        Ꮡt.Error(deepEqualComplexˢ);
    }
}

[GoType] partial struct UnexpT {
    internal map<nint, nint> m;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object deepEqualX1X2FalseWantˢ = (@string)"DeepEqual(x1, x2) = false, want true"u8;
internal static readonly object deepEqualX1Y1TrueWantˢ = (@string)"DeepEqual(x1, y1) = true, want false"u8;

public static void TestDeepEqualUnexportedMap(ж<Δtesting.T> Ꮡt) {
    // Check that DeepEqual can look at unexported fields.
    ref var x1 = ref heap<UnexpT>(out var Ꮡx1);
    x1 = new UnexpT(new map<nint, nint>{[1] = 2});
    ref var x2 = ref heap<UnexpT>(out var Ꮡx2);
    x2 = new UnexpT(new map<nint, nint>{[1] = 2});
    if (!DeepEqual(Ꮡx1, Ꮡx2)) {
        Ꮡt.Error(deepEqualX1X2FalseWantˢ);
    }
    ref var y1 = ref heap<UnexpT>(out var Ꮡy1);
    y1 = new UnexpT(new map<nint, nint>{[2] = 3});
    if (DeepEqual(Ꮡx1, Ꮡy1)) {
        Ꮡt.Error(deepEqualX1Y1TrueWantˢ);
    }
}


[GoType("dyn")] partial struct deepEqualPerfTestsᴛ1 {
    internal any x, y;
}
internal static slice<deepEqualPerfTestsᴛ1> deepEqualPerfTests = new deepEqualPerfTestsᴛ1[]{
    new(x: (int8)99, y: (int8)99),
    new(x: new int8[]{99}.slice(), y: new int8[]{99}.slice()),
    new(x: (int16)99, y: (int16)99),
    new(x: new int16[]{99}.slice(), y: new int16[]{99}.slice()),
    new(x: (int32)99, y: (int32)99),
    new(x: new int32[]{99}.slice(), y: new int32[]{99}.slice()),
    new(x: (int64)99, y: (int64)99),
    new(x: new int64[]{99}.slice(), y: new int64[]{99}.slice()),
    new(x: (nint)999999, y: (nint)999999),
    new(x: new nint[]{999999}.slice(), y: new nint[]{999999}.slice()),
    new(x: (uint8)99, y: (uint8)99),
    new(x: new uint8[]{99}.slice(), y: new uint8[]{99}.slice()),
    new(x: (uint16)99, y: (uint16)99),
    new(x: new uint16[]{99}.slice(), y: new uint16[]{99}.slice()),
    new(x: (uint32)99, y: (uint32)99),
    new(x: new uint32[]{99}.slice(), y: new uint32[]{99}.slice()),
    new(x: (uint64)99, y: (uint64)99),
    new(x: new uint64[]{99}.slice(), y: new uint64[]{99}.slice()),
    new(x: (nuint)999999, y: (nuint)999999),
    new(x: new nuint[]{999999}.slice(), y: new nuint[]{999999}.slice()),
    new(x: (uintptr)999999, y: (uintptr)999999),
    new(x: new uintptr[]{999999}.slice(), y: new uintptr[]{999999}.slice()),
    new(x: (float32)1.414F, y: (float32)1.414F),
    new(x: new float32[]{1.414F}.slice(), y: new float32[]{1.414F}.slice()),
    new(x: (float64)1.414D, y: (float64)1.414D),
    new(x: new float64[]{1.414D}.slice(), y: new float64[]{1.414D}.slice()),
    new(x: (complex64)1.414F, y: (complex64)1.414F),
    new(x: new complex64[]{1.414F}.slice(), y: new complex64[]{1.414F}.slice()),
    new(x: (complex128)1.414D, y: (complex128)1.414D),
    new(x: new complex128[]{1.414D}.slice(), y: new complex128[]{1.414D}.slice()),
    new(x: true, y: true),
    new(x: new bool[]{true}.slice(), y: new bool[]{true}.slice()),
    new(x: (@string)"abcdef"u8, y: (@string)"abcdef"u8),
    new(x: new @string[]{"abcdef"u8}.slice(), y: new @string[]{"abcdef"u8}.slice()),
    new(x: slice<byte>("abcdef"u8), y: slice<byte>("abcdef"u8)),
    new(x: new slice<byte>[]{slice<byte>("abcdef"u8)}.slice(), y: new slice<byte>[]{slice<byte>("abcdef"u8)}.slice()),
    new(x: new byte[]{(rune)'a', (rune)'b', (rune)'c', (rune)'a', (rune)'b', (rune)'c'}.array(), y: new byte[]{(rune)'a', (rune)'b', (rune)'c', (rune)'a', (rune)'b', (rune)'c'}.array()),
    new(x: GoReflect.WithElemDims(new array<byte>[]{new byte[]{(rune)'a', (rune)'b', (rune)'c', (rune)'a', (rune)'b', (rune)'c'}.array()}.slice(), 6), y: GoReflect.WithElemDims(new array<byte>[]{new byte[]{(rune)'a', (rune)'b', (rune)'c', (rune)'a', (rune)'b', (rune)'c'}.array()}.slice(), 6))
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object testAllocatesMoreWithˢ = (@string)"test allocates more with -asan; see #70079"u8;

public static void TestDeepEqualAllocs(ж<Δtesting.T> Ꮡt) {
    // TODO(prattmic): maps on stack
    if (goexperiment.SwissMap) {
        Ꮡt.Skipf("Maps on stack not yet implemented"u8);
    }
    if (asan.Enabled) {
        Ꮡt.Skip(testAllocatesMoreWithˢ);
    }
    foreach (var (_, vᴛ1) in deepEqualPerfTests) {
        ref var tt = ref heap(new deepEqualPerfTestsᴛ1(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(ValueOf(tt.x).Type().String(), (ж<Δtesting.T> tΔ1) => {
            var ttʗ2 = ttʗ1;
            var got = Δtesting.AllocsPerRun(100, () => {
                if (!DeepEqual(ttʗ2.x, ttʗ2.y)) {
                    tΔ1.Errorf("DeepEqual(%v, %v)=false"u8, ttʗ2.x, ttʗ2.y);
                }
            });
            if ((nint)got != 0) {
                tΔ1.Errorf("DeepEqual(%v, %v) allocated %d times"u8, ttʗ1.x, ttʗ1.y, (nint)got);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object mismatchedOffsetsInˢ = (@string)"mismatched offsets in structure alignment:"u8;

internal static void check2ndField(any x, uintptr offs, ж<Δtesting.T> Ꮡt) {
    var s = ValueOf(x);
    var f = s.Type().Field(1);
    if (f.Offset != offs) {
        Ꮡt.Error(mismatchedOffsetsInˢ, f.Offset, offs);
    }
}

[GoType("dyn")] internal partial struct TestAlignment_T1inner {
    internal nint a;
}

[GoType("dyn")] internal partial struct TestAlignment_T1 {
    public partial ref TestAlignment_T1inner T1inner { get; }
    internal nint f;
}

[GoType("dyn")] internal partial struct TestAlignment_T2inner {
    internal nint a, b;
}

[GoType("dyn")] internal partial struct TestAlignment_T2 {
    public partial ref TestAlignment_T2inner T2inner { get; }
    internal nint f;
}

// Check that structure alignment & offsets viewed through reflect agree with those
// from the compiler itself.
public static void TestAlignment(ж<Δtesting.T> Ꮡt) {
    ref var x = ref heap<TestAlignment_T1>(out var Ꮡx);
    x = new TestAlignment_T1(new TestAlignment_T1inner(2), 17);
    check2ndField(x, (uintptr)Ꮡx.of(TestAlignment_T1.Ꮡf) - (uintptr)Ꮡx, Ꮡt);
    ref var x1 = ref heap<TestAlignment_T2>(out var Ꮡx1);
    x1 = new TestAlignment_T2(new TestAlignment_T2inner(2, 3), 17);
    check2ndField(x1, (uintptr)Ꮡx1.of(TestAlignment_T2.Ꮡf) - (uintptr)Ꮡx1, Ꮡt);
}

public static void Nil(any a, ж<Δtesting.T> Ꮡt) {
    var n = ValueOf(a).Field(0);
    if (!n.IsNil()) {
        Ꮡt.Errorf("%v should be nil"u8, a);
    }
}

public static void NotNil(any a, ж<Δtesting.T> Ꮡt) {
    var n = ValueOf(a).Field(0);
    if (n.IsNil()) {
        Ꮡt.Errorf("value of type %v should not be nil"u8, ValueOf(a).Type().String());
    }
}

[GoType("dyn")] internal partial struct TestIsNil_doNil {
    internal ж<nint> x;
}

[GoType("dyn")] internal partial struct TestIsNil_doNilᴛ1 {
    internal any x;
}

[GoType("dyn")] internal partial struct TestIsNil_doNilᴛ2 {
    internal map<@string, nint> x;
}

[GoType("dyn")] internal partial struct TestIsNil_doNilᴛ3 {
    internal Func<bool> x;
}

[GoType("dyn")] internal partial struct TestIsNil_doNilᴛ4 {
    internal channel<nint> x;
}

[GoType("dyn")] internal partial struct TestIsNil_doNilᴛ5 {
    internal slice<@string> x;
}

[GoType("dyn")] internal partial struct TestIsNil_doNilᴛ6 {
    internal @unsafe.Pointer x;
}

[GoType("dyn")] internal partial struct TestIsNil_mi {
    internal map<nint, nint> x;
}

[GoType("dyn")] internal partial struct TestIsNil_fi {
    internal Action<ж<Δtesting.T>> x;
}

public static void TestIsNil(ж<Δtesting.T> Ꮡt) {
    // These implement IsNil.
    // Wrap in extra struct to hide interface type.
    var doNil = new any[]{
        new TestIsNil_doNil(),
        new TestIsNil_doNilᴛ1(),
        new TestIsNil_doNilᴛ2(),
        new TestIsNil_doNilᴛ3(),
        new TestIsNil_doNilᴛ4(),
        new TestIsNil_doNilᴛ5(),
        new TestIsNil_doNilᴛ6()
    }.slice();
    foreach (var (_, ts) in doNil) {
        var ty = TypeOf(ts).Field(0).Type;
        var v = Zero(ty);
        v.IsNil(); // panics if not okay to call
    }
    // Check the implementations
    TestIsNil_doNil pi = default!;
    Nil(pi, Ꮡt);
    pi.x = @new<nint>();
    NotNil(pi, Ꮡt);
    TestAppend_i si = default!;
    Nil(si, Ꮡt);
    si.x = new slice<nint>(10);
    NotNil(si, Ꮡt);
    TestIsNil_doNilᴛ4 ci = default!;
    Nil(ci, Ꮡt);
    ci.x = new channel<nint>(0);
    NotNil(ci, Ꮡt);
    TestIsNil_mi mi = default!;
    Nil(mi, Ꮡt);
    mi.x = new map<nint, nint>();
    NotNil(mi, Ꮡt);
    TestIsNil_doNilᴛ1 ii = default!;
    Nil(ii, Ꮡt);
    ii.x = (nint)(2);
    NotNil(ii, Ꮡt);
    TestIsNil_fi fi = default!;
    Nil(fi, Ꮡt);
    fi.x = TestIsNil;
    NotNil(fi, Ꮡt);
}

internal static S /*out*/ setField<S, V>(S @inʗp, uintptr offset, V value) {
    ref var @in = ref heap(@inʗp, out var Ꮡin);

    ((ж<V>)(uintptr)(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(Ꮡin), offset))).ValueSlot = value;
    return @in;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object shouldPanicForInvalidˢ = (@string)"should panic for invalid value"u8;

[GoType("dyn")] internal partial struct TestIsZero_type {
    internal any x;
    internal bool want;
}

[GoType("dyn")] internal partial struct TestIsZero_typeᴛ1 {
    internal ж<nint> p;
}

[GoType("dyn")] internal partial struct TestIsZero_typeᴛ2 {
    internal slice<nint> s;
}

[GoType("dyn")] internal partial struct TestIsZero_typeᴛ3 {
    public partial ref reflect_package.ΔValue Value { get; }
}

[GoType("dyn")] internal partial struct TestIsZero_typeᴛ4 {
    internal uintptr _;
    internal uintptr a;
    internal uintptr __;
}

[GoType("dyn")] internal partial struct TestIsZero_typeᴛ5 {
    internal Action _;
    internal Action a;
    internal Action __;
}

[GoType("dyn")] internal partial struct TestIsZero_typeᴛ6 {
    internal array<S> a = new(256);
}

[GoType("dyn")] internal partial struct TestIsZero_typeᴛ7 {
    internal array<float32> a = new(256);
}

[GoType("dyn")] internal partial struct TestIsZero_typeᴛ8 {
    internal array<S> _ = new(256);
    internal array<S> a = new(256);
}

public static void TestIsZero(ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (i, tt) in new TestIsZero_type[]{ // Booleans

        new(true, false),
        new(false, true), // Numeric types

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
        new((uintptr)128, false), // Array

        new(Zero(TypeOf(new @string[]{}.array(5))).Interface(), true),
        new(new @string[]{}.array(5), true), // comparable array

        new(new @string[]{""u8, ""u8, ""u8, "a"u8, ""u8}.array(), false), // comparable array

        new(new ж<nint>[]{}.array(1), true), // direct pointer array

        new(new ж<nint>[]{@new<nint>()}.array(), false), // direct pointer array

        new(new slice<nint>[]{}.array(3), true), // incomparable array

        new(new slice<nint>[]{new nint[]{1}.slice()}.array(3), false), // incomparable array

        new(new byte[]{}.array(4096), true),
        new(new byte[]{1}.array(4096), false),
        new(new TestIsZero_typeᴛ1[]{}.array(1), true),
        new(new TestIsZero_typeᴛ1[]{new(@new<nint>())}.array(), false),
        new(new reflectꓸValue[]{}.array(3, () => new(nil)), true),
        new(new reflectꓸValue[]{new(), ValueOf((nint)(0)), new()}.array(), false), // Chan

        new((channel<@string>)(default!), true),
        new(new channel<@string>(0), false),
        new(time.After(1), false), // Func

        new(((Action)(default!)).OrTypedNilFunc(), true),
        new(New, false), // Interface

        new(New(TypeOf(@new<error>()).Elem()).Elem(), true),
        new(((Δio.Reader)new reflect_test_package.strings_ReaderжReader(strings.NewReader(""u8))), false), // Map

        new(((map<@string, @string>)default!), true),
        new(new map<@string, @string>{}, false),
        new(new map<@string, @string>(), false), // Pointer

        new(((ж<Action>)nil), true),
        new(((ж<nint>)nil), true),
        new(@new<nint>(), false), // Slice

        new(new @string[]{}.slice(), false),
        new((slice<@string>)(default!), true),
        new(new slice<@string>(0), false), // Strings

        new((@string)""u8, true),
        new((@string)"not-zero"u8, false), // Structs

        new(new T(nil), true), // comparable struct

        new(new T(123, 456.75D, "hello"u8, Ꮡ_i), false), // comparable struct

        new(new TestIsZero_typeᴛ1(), true), // direct pointer struct

        new(new TestIsZero_typeᴛ1(@new<nint>()), false), // direct pointer struct

        new(new TestIsZero_typeᴛ2(), true), // incomparable struct

        new(new TestIsZero_typeᴛ2(new nint[]{1}.slice()), false), // incomparable struct

        new(new TestIsZero_typeᴛ3(), true),
        new(new TestIsZero_typeᴛ3(ValueOf((nint)(0))), false),
        new(new TestIsZero_typeᴛ4(), true), // comparable struct with blank fields

        new(setField(new TestIsZero_typeᴛ4(), 0 * /* unsafe.Sizeof(uintptr(0)) */ (uintptr)8, 1), true),
        new(setField(new TestIsZero_typeᴛ4(), 1 * /* unsafe.Sizeof(uintptr(0)) */ (uintptr)8, 1), false),
        new(setField(new TestIsZero_typeᴛ4(), 2 * /* unsafe.Sizeof(uintptr(0)) */ (uintptr)8, 1), true),
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
        new(setField(new TestIsZero_typeᴛ8(), 0 * /* unsafe.Sizeof(int64(0)) */ (uintptr)8, (int64)1), true), // UnsafePointer

        new(@unsafe.Pointer.OrTypedNil((@unsafe.Pointer)default!), true),
        new(@unsafe.Pointer.FromPinnedBox(@new<nint>()), false)
    }.slice()) {
        reflectꓸValue x = new(nil);
        {
            var (v, ok) = tt.x._<reflectꓸValue>(ᐧ); if (ok){
                x = v;
            } else {
                x = ValueOf(tt.x);
            }
        }
        var b = x.IsZero();
        if (b != tt.want) {
            Ꮡt.Errorf("%d: IsZero((%s)(%+v)) = %t, want %t"u8, i, x.Kind(), tt.x, b, tt.want);
        }
        if (!Zero(TypeOf(tt.x)).IsZero()) {
            Ꮡt.Errorf("%d: IsZero(Zero(TypeOf((%s)(%+v)))) is false"u8, i, x.Kind(), tt.x);
        }
        var p = New(x.Type()).Elem();
        p.Set(x);
        p.SetZero();
        if (!p.IsZero()) {
            Ꮡt.Errorf("%d: IsZero((%s)(%+v)) is true after SetZero"u8, i, p.Kind(), tt.x);
        }
    }
    ((Action)(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                {
                    var r = recover(); if (r == default!) {
                        Ꮡt.Error(shouldPanicForInvalidˢ);
                    }
                }
            }, ref ᒐ);
            (new reflectꓸValue(nil)).IsZero();
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }))();
}

public static void TestInternalIsZero(ж<Δtesting.T> Ꮡt) {
    var b = new slice<byte>(512);
    for (nint a = 0; a < 8; a++) {
        for (nint i = 1; i <= 512 - a; i++) {
            reflect_internal_test_package.InternalIsZero(b.slice(a, a + i));
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object interfaceOnInterfaceˢ = (@string)"Interface() on interface: "u8;

[GoType("dyn")] internal partial struct TestInterfaceExtraction_s {
    public Δio.Writer W;
}

public static void TestInterfaceExtraction(ж<Δtesting.T> Ꮡt) {
    ref var s = ref heap(new TestInterfaceExtraction_s(), out var Ꮡs);
    s.W = new Δos.FileжWriter(Δos.Stdout);
    var v = Indirect(ValueOf(Ꮡs)).Field(0).Interface();
    if (!AreEqual(v, s.W._<any>())) {
        Ꮡt.Error(interfaceOnInterfaceˢ, v, s.W);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object valueOfIntNilElemIsValidˢ = (@string)"ValueOf((*int)(nil)).Elem().IsValid()"u8;

public static void TestNilPtrValueSub(ж<Δtesting.T> Ꮡt) {
    ж<nint> pi = default!;
    {
        var pv = ValueOf(pi.OrTypedNil()); if (pv.Elem().IsValid()) {
            Ꮡt.Error(valueOfIntNilElemIsValidˢ);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object notPresentˢ = (@string)"not-present"u8;
internal static readonly @string notAssignableˢ = "not assignable"u8;
internal static readonly @string keyˢ = "key"u8;

[GoLocalName("S")] [GoType("@string")] internal partial struct TestMap_S;

public static void TestMap(ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    ref var m = ref heap<map<@string, nint>>(out var Ꮡm);
    m = new map<@string, nint>{["a"u8] = 1, ["b"u8] = 2};
    ref var mv = ref heap<reflectꓸValue>(out var Ꮡmv);
    mv = ValueOf(m);
    {
        nint n = mv.Len(); if (n != len(m)) {
            Ꮡt.Errorf("Len = %d, want %d"u8, n, len(m));
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
            Ꮡt.Errorf("Missing key %q"u8, k);
        }
        // Check that value lookup is correct.
        var vvΔ1 = mv.MapIndex(ValueOf(k));
        {
            var vi = vvΔ1.Int(); if (vi != (int64)vΔ1) {
                Ꮡt.Errorf("Key %q: have value %d, want %d"u8, k, vi, vΔ1);
            }
        }
        // Copy into new map.
        newmap.SetMapIndex(ValueOf(k), ValueOf(vΔ1));
    }
    var vv = mv.MapIndex(ValueOf(notPresentˢ));
    if (vv.IsValid()) {
        Ꮡt.Errorf("Invalid key: got non-nil value %s"u8, valueToString(vv));
    }
    var newm = newmap.Interface()._<map<@string, nint>>();
    if (len(newm) != len(m)) {
        Ꮡt.Errorf("length after copy: newm=%d, m=%d"u8, len(newm), len(m));
    }
    foreach (var (k, vΔ2) in newm) {
        var (mvΔ1, okΔ1) = m[k, ꟷ];
        if (mvΔ1 != vΔ2) {
            Ꮡt.Errorf("newm[%q] = %d, but m[%q] = %d, %v"u8, k, vΔ2, k, mvΔ1, okΔ1);
        }
    }
    newmap.SetMapIndex(ValueOf((@string)"a"u8), new reflectꓸValue(nil));
    var (v, ok) = newm["a"u8, ꟷ];
    if (ok) {
        Ꮡt.Errorf("newm[\"a\"] = %d after delete"u8, v);
    }
    mv = ValueOf(Ꮡm).Elem();
    mv.Set(Zero(mv.Type()));
    if (m != default!) {
        Ꮡt.Errorf("mv.Set(nil) failed"u8);
    }
    var mvʗ1 = mv;
    shouldPanic(notAssignableˢ, () => {
        mvʗ1.MapIndex(ValueOf(((TestMap_S)(@string)keyˢ)));
    });
    var mvʗ2 = mv;
    shouldPanic(notAssignableˢ, () => {
        mvʗ2.SetMapIndex(ValueOf(((TestMap_S)(@string)keyˢ)), ValueOf((nint)(0)));
    });
}

public static void TestNilMap(ж<Δtesting.T> Ꮡt) {
    map<@string, nint> m = default!;
    var mv = ValueOf(m);
    var keys = mv.MapKeys();
    if (len(keys) != 0) {
        Ꮡt.Errorf(">0 keys for nil map: %v"u8, keys);
    }
    // Check that value for missing key is zero.
    var x = mv.MapIndex(ValueOf(helloˢ));
    if (x.Kind() != Invalid) {
        Ꮡt.Errorf("m.MapIndex(\"hello\") for nil map = %v, want Invalid Value"u8, x);
    }
    // Check big value too.
    map<@string, array<byte>> mbig = default!;
    x = ValueOf(mbig).MapIndex(ValueOf(helloˢ));
    if (x.Kind() != Invalid) {
        Ꮡt.Errorf("mbig.MapIndex(\"hello\") for nil map = %v, want Invalid Value"u8, x);
    }
    // Test that deletes from a nil map succeed.
    mv.SetMapIndex(ValueOf((@string)"hi"u8), new reflectꓸValue(nil));
}

public static void TestChan(ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    for (nint loop = 0; loop < 2; loop++) {
        channel<nint> cΔ1 = default!;
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
        cvΔ1.Send(ValueOf((nint)(2)));
        {
            nint iΔ1 = ᐸꟷ(cΔ1); if (iΔ1 != 2) {
                Ꮡt.Errorf("reflect Send 2, native recv %d"u8, iΔ1);
            }
        }
        // Recv
        cΔ1.ᐸꟷ(3);
        {
            var (iΔ2, okΔ1) = cvΔ1.Recv(); if (iΔ2.Int() != 3 || !okΔ1) {
                Ꮡt.Errorf("native send 3, reflect Recv %d, %t"u8, iΔ2.Int(), okΔ1);
            }
        }
        // TryRecv fail
        var (val, ok) = cvΔ1.TryRecv();
        if (val.IsValid() || ok) {
            Ꮡt.Errorf("TryRecv on empty chan: %s, %t"u8, valueToString(val), ok);
        }
        // TryRecv success
        cΔ1.ᐸꟷ(4);
        (val, ok) = cvΔ1.TryRecv();
        if (!val.IsValid()){
            Ꮡt.Errorf("TryRecv on ready chan got nil"u8);
        } else 
        {
            var iΔ3 = val.Int(); if (iΔ3 != 4 || !ok) {
                Ꮡt.Errorf("native send 4, TryRecv %d, %t"u8, iΔ3, ok);
            }
        }
        // TrySend fail
        cΔ1.ᐸꟷ(100);
        ok = cvΔ1.TrySend(ValueOf((nint)(5)));
        nint i = ᐸꟷ(cΔ1);
        if (ok) {
            Ꮡt.Errorf("TrySend on full chan succeeded: value %d"u8, i);
        }
        // TrySend success
        ok = cvΔ1.TrySend(ValueOf((nint)(6)));
        if (!ok){
            Ꮡt.Errorf("TrySend on empty chan failed"u8);
            var selᴛ1 = cΔ1;
            switch (trySelect(ᐸꟷ(selᴛ1, ꓸꓸꓸ))) {
            case 0 when selᴛ1.ꟷᐳ(out var x): {
                Ꮡt.Errorf("TrySend failed but it did send %d"u8, x);
                break;
            }
            default: {
                break;
            }}
        } else {
            {
                i = ᐸꟷ(cΔ1); if (i != 6) {
                    Ꮡt.Errorf("TrySend 6, recv %d"u8, i);
                }
            }
        }
        // Close
        cΔ1.ᐸꟷ(123);
        cvΔ1.Close();
        {
            var (iΔ1, okΔ1) = cvΔ1.Recv(); if (iΔ1.Int() != 123 || !okΔ1) {
                Ꮡt.Errorf("send 123 then close; Recv %d, %t"u8, iΔ1.Int(), okΔ1);
            }
        }
        {
            var (iΔ2, okΔ2) = cvΔ1.Recv(); if (iΔ2.Int() != 0 || okΔ2) {
                Ꮡt.Errorf("after close Recv %d, %t"u8, iΔ2.Int(), okΔ2);
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
    channel<nint> c = default!;
    var cv = MakeChan(TypeOf(c), 0);
    c = cv.Interface()._<channel<nint>>();
    if (cv.TrySend(ValueOf((nint)(7)))) {
        Ꮡt.Errorf("TrySend on sync chan succeeded"u8);
    }
    {
        var (v, ok) = cv.TryRecv(); if (v.IsValid() || ok) {
            Ꮡt.Errorf("TryRecv on sync chan succeeded: isvalid=%v ok=%v"u8, v.IsValid(), ok);
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
            Ꮡt.Errorf("Len/Cap = %d/%d want %d/%d"u8, l, m, len(c), cap(c));
        }
    }
}

// caseInfo describes a single case in a select test.
[GoType] partial struct caseInfo {
    internal @string desc;
    internal bool canSelect;
    internal reflectꓸValue recv;
    internal bool closed;
    internal Action helper;
    internal bool panic;
}

internal static ж<bool> allselect = flag.Bool("allselect"u8, false, "exhaustive select test"u8);

public static void TestSelect(ж<Δtesting.T> Ꮡt) {
    ᏑselectWatch.of(selectWatchᴛ1.Ꮡonce).Do([MethodImpl(MethodImplOptions.NoInlining)] () => {
        goǃ(selectWatcher);
    });
    exhaustive x = default!;
    nint nch = 0;
    (reflectꓸValue ch, reflectꓸValue val) newop(nint n, nint cap) {
        reflectꓸValue ch = new(nil);
        reflectꓸValue val = new(nil);
        nch++;
        if (nch % 101 % 2 == 1){
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
            println((@string)"TestSelect"u8, n);
        }
        slice<Δreflect.SelectCase> cases = default!;
        slice<caseInfo> info = default!;
        // Ready send.
        if (x.Maybe()) {
            var (ch, val) = newop(len(cases), 1);
            cases = append(cases, new SelectCase(
                Dir: SelectSend,
                Chan: ch,
                Send: val
            ));
            info = append(info, new caseInfo(desc: "ready send"u8, canSelect: true));
        }
        // Ready recv.
        if (x.Maybe()) {
            var (ch, val) = newop(len(cases), 1);
            ch.Send(val);
            cases = append(cases, new SelectCase(
                Dir: SelectRecv,
                Chan: ch
            ));
            info = append(info, new caseInfo(desc: "ready recv"u8, canSelect: true, recv: val));
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
            if (x.Maybe()){
                var chʗ1 = ch;
                var f = () => {
                    chʗ1.Recv();
                };
                info = append(info, new caseInfo(desc: "blocking send"u8, helper: f));
            } else {
                info = append(info, new caseInfo(desc: "blocking send"u8));
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
            if (x.Maybe()){
                var chʗ2 = ch;
                var valʗ1 = val;
                var f = () => {
                    chʗ2.Send(valʗ1);
                };
                info = append(info, new caseInfo(desc: "blocking recv"u8, recv: val, helper: f));
            } else {
                info = append(info, new caseInfo(desc: "blocking recv"u8));
            }
        }
        // Zero Chan send.
        if (x.Maybe()) {
            // Maybe include value to send.
            reflectꓸValue val = new(nil);
            if (x.Maybe()) {
                val = ValueOf((nint)(100));
            }
            cases = append(cases, new SelectCase(
                Dir: SelectSend,
                Send: val
            ));
            info = append(info, new caseInfo(desc: "zero Chan send"u8));
        }
        // Zero Chan receive.
        if (x.Maybe()) {
            cases = append(cases, new SelectCase(
                Dir: SelectRecv
            ));
            info = append(info, new caseInfo(desc: "zero Chan recv"u8));
        }
        // nil Chan send.
        if (x.Maybe()) {
            cases = append(cases, new SelectCase(
                Dir: SelectSend,
                Chan: ValueOf((channel<nint>)(default!)),
                Send: ValueOf((nint)(101))
            ));
            info = append(info, new caseInfo(desc: "nil Chan send"u8));
        }
        // nil Chan recv.
        if (x.Maybe()) {
            cases = append(cases, new SelectCase(
                Dir: SelectRecv,
                Chan: ValueOf((channel<nint>)(default!))
            ));
            info = append(info, new caseInfo(desc: "nil Chan recv"u8));
        }
        // closed Chan send.
        if (x.Maybe()) {
            var ch = new channel<nint>(0);
            close(ch);
            cases = append(cases, new SelectCase(
                Dir: SelectSend,
                Chan: ValueOf(ch),
                Send: ValueOf((nint)(101))
            ));
            info = append(info, new caseInfo(desc: "closed Chan send"u8, canSelect: true, panic: true));
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
            info = append(info, new caseInfo(desc: "closed Chan recv"u8, canSelect: true, closed: true, recv: val));
        }
        Action helper = default!;        // goroutine to help the select complete
        // Add default? Must be last case here, but will permute.
        // Add the default if the select would otherwise
        // block forever, and maybe add it anyway.
        nint numCanSelect = 0;
        var canProceed = false;
        var canBlock = true;
        var canPanic = false;
        var helpers = new nint[]{}.slice();
        foreach (var (iΔ1, c) in info) {
            if (c.canSelect){
                canProceed = true;
                canBlock = false;
                numCanSelect++;
                if (c.panic) {
                    canPanic = true;
                }
            } else 
            if (c.helper != default!) {
                canProceed = true;
                helpers = append(helpers, iΔ1);
            }
        }
        if (!canProceed || x.Maybe()){
            cases = append(cases, new SelectCase(
                Dir: SelectDefault
            ));
            info = append(info, new caseInfo(desc: "default"u8, canSelect: canBlock));
            numCanSelect++;
        } else 
        if (canBlock) {
            // Select needs to communicate with another goroutine.
            var casΔ1 = Ꮡ(info, helpers[x.Choose(len(helpers))]);
            helper = casΔ1.Value.helper;
            casΔ1.Value.canSelect = true;
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
        if (helper != default!) {
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
        if (panicErr != default! && !canPanic) {
            Ꮡt.Fatalf("%s\npanicked unexpectedly: %v"u8, fmtSelect(info), panicErr);
        }
        if (panicErr == default! && canPanic && numCanSelect == 1) {
            Ꮡt.Fatalf("%s\nselected #%d incorrectly (should panic)"u8, fmtSelect(info), i);
        }
        if (panicErr != default!) {
            continue;
        }
        var cas = info[i];
        if (!cas.canSelect) {
            @string recvStr = ""u8;
            if (recv.IsValid()) {
                recvStr = fmt.Sprintf(", received %v, %v"u8, recv.Interface(), recvOK);
            }
            Ꮡt.Fatalf("%s\nselected #%d incorrectly%s"u8, fmtSelect(info), i, recvStr);
        }
        if (cas.panic) {
            Ꮡt.Fatalf("%s\nselected #%d incorrectly (case should panic)"u8, fmtSelect(info), i);
        }
        if (cases[i].Dir == SelectRecv){
            if (!recv.IsValid()) {
                Ꮡt.Fatalf("%s\nselected #%d but got %v, %v, want %v, %v"u8, fmtSelect(info), i, recv, recvOK, cas.recv.Interface(), !cas.closed);
            }
            if (!cas.recv.IsValid()) {
                Ꮡt.Fatalf("%s\nselected #%d but internal error: missing recv value"u8, fmtSelect(info), i);
            }
            if (!AreEqual(recv.Interface(), cas.recv.Interface()) || recvOK != !cas.closed) {
                if (AreEqual(recv.Interface(), cas.recv.Interface()) && recvOK == !cas.closed) {
                    Ꮡt.Fatalf("%s\nselected #%d, got %#v, %v, and DeepEqual is broken on %T"u8, fmtSelect(info), i, recv.Interface(), recvOK, recv.Interface());
                }
                Ꮡt.Fatalf("%s\nselected #%d but got %#v, %v, want %#v, %v"u8, fmtSelect(info), i, recv.Interface(), recvOK, cas.recv.Interface(), !cas.closed);
            }
        } else {
            if (recv.IsValid() || recvOK) {
                Ꮡt.Fatalf("%s\nselected #%d but got %v, %v, want %v, %v"u8, fmtSelect(info), i, recv, recvOK, new reflectꓸValue(nil), false);
            }
        }
    }
}

public static void TestSelectMaxCases(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        slice<Δreflect.SelectCase> sCases = default!;
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
                var err = recover(); if (err != default!){
                    if (err._<@string>() != "reflect.Select: too many cases (max 65536)"u8) {
                        Ꮡt.Fatalf("unexpected error from select call with greater than max supported cases"u8);
                    }
                } else {
                    Ꮡt.Fatalf("expected select call to panic with greater than max supported cases"u8);
                }
            }
        }, ref ᒐ);
        // Should panic
        (_, _, _) = Select(sCases);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestSelectNop(ж<Δtesting.T> Ꮡt) {
    // "select { default: }" should always return the default case.
    var (chosen, _, _) = Select(new Δreflect.SelectCase[]{new(Dir: SelectDefault)}.slice());
    if (chosen != 0) {
        Ꮡt.Fatalf("expected Select to return 0, but got %#v"u8, chosen);
    }
}

// selectWatch and the selectWatcher are a watchdog mechanism for running Select.
// If the selectWatcher notices that the select has been blocked for >1 second, it prints
// an error describing the select and panics the entire test binary.

[GoType("dyn")] partial struct selectWatchᴛ1 {
    public partial ref sync_package.Mutex Mutex { get; }
    internal Δsync.Once once;
    internal time.Time now;
    internal slice<caseInfo> info;
}
internal static ж<selectWatchᴛ1> ᏑselectWatch = new StandardBox<selectWatchᴛ1>(new selectWatchᴛ1(nil));
internal static ref selectWatchᴛ1 selectWatch => ref ᏑselectWatch.Value;

internal static void selectWatcher() {
    while (ᐧ) {
        time.Sleep(1 * time.ΔSecond);
        ᏑselectWatch.of(selectWatchᴛ1.ᏑMutex).Lock();
        if (selectWatch.info != default! && time.Since(selectWatch.now) > (time.Duration)(10000000000L)) {
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
    nint chosen = default!;
    reflectꓸValue recv = new(nil);
    bool recvOK = default!;
    any panicErr = default!;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            panicErr = recover();
            ᏑselectWatch.of(selectWatchᴛ1.ᏑMutex).Lock();
            selectWatch.info = default!;
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

[GoType("[2]uintptr")] partial struct two;

// Difficult test for function call because of
// implicit padding between arguments.
internal static (byte i, nint j, byte k, two l, byte m, float32 n, byte o) dummy(byte b, nint c, byte d, two e, byte f, float32 g, byte h) {
    e = e.Clone();

    return (b, c, d, e.Clone(), f, g, h);
}

public static void TestFunc(ж<Δtesting.T> Ꮡt) {
    var ret = ValueOf(dummy).Call(new reflectꓸValue[]{
        ValueOf((byte)10),
        ValueOf((nint)(20)),
        ValueOf((byte)30),
        ValueOf(new two(new uintptr[]{40, 50}.array())),
        ValueOf((byte)60),
        ValueOf((float32)70F),
        ValueOf((byte)80)
    }.slice());
    if (len(ret) != 7) {
        Ꮡt.Fatalf("Call returned %d values, want 7"u8, len(ret));
    }
    var i = (byte)ret[0].Uint();
    nint j = (nint)ret[1].Int();
    var k = (byte)ret[2].Uint();
    var l = ret[3].Interface()._<two>();
    var m = (byte)ret[4].Uint();
    var n = (float32)ret[5].Float();
    var o = (byte)ret[6].Uint();
    if (i != 10 || j != 20 || k != 30 || l != (new two(new uintptr[]{40, 50}.array())) || m != 60 || n != 70F || o != 80) {
        Ꮡt.Errorf("Call returned %d, %d, %d, %v, %d, %g, %d; want 10, 20, 30, [40, 50], 60, 70, 80"u8, i, j, k, l, m, n, o);
    }
    foreach (var (iΔ1, v) in ret) {
        if (v.CanAddr()) {
            Ꮡt.Errorf("result %d is addressable"u8, iΔ1);
        }
    }
}

public static void TestCallConvert(ж<Δtesting.T> Ꮡt) {
    var v = ValueOf(@new<Δio.ReadWriter>()).Elem();
    var f = ValueOf(Δio.Reader (Δio.Reader r) => r);
    var @out = f.Call(new reflectꓸValue[]{v}.slice());
    if (len(@out) != 1 || !AreEqual(@out[0].Type(), TypeOf(@new<Δio.Reader>()).Elem()) || !@out[0].IsNil()) {
        Ꮡt.Errorf("expected [nil], got %v"u8, @out);
    }
}

[GoType] partial struct emptyStruct {
}

[GoType] partial struct nonEmptyStruct {
    internal nint member;
}

internal static emptyStruct returnEmpty() {
    return new emptyStruct(nil);
}

internal static void takesEmpty(emptyStruct e) {
}

internal static nonEmptyStruct returnNonEmpty(nint i) {
    return new nonEmptyStruct(member: i);
}

internal static nint takesNonEmpty(nonEmptyStruct n) {
    return n.member;
}

public static void TestCallWithStruct(ж<Δtesting.T> Ꮡt) {
    var r = ValueOf(returnEmpty).Call(default!);
    if (len(r) != 1 || !AreEqual(r[0].Type(), TypeOf(new emptyStruct(nil)))) {
        Ꮡt.Errorf("returning empty struct returned %#v instead"u8, r);
    }
    r = ValueOf(takesEmpty).Call(new reflectꓸValue[]{ValueOf(new emptyStruct(nil))}.slice());
    if (len(r) != 0) {
        Ꮡt.Errorf("takesEmpty returned values: %#v"u8, r);
    }
    r = ValueOf(returnNonEmpty).Call(new reflectꓸValue[]{ValueOf((nint)(42))}.slice());
    if (len(r) != 1 || !AreEqual(r[0].Type(), TypeOf(new nonEmptyStruct(nil))) || r[0].Field(0).Int() != 42) {
        Ꮡt.Errorf("returnNonEmpty returned %#v"u8, r);
    }
    r = ValueOf(takesNonEmpty).Call(new reflectꓸValue[]{ValueOf(new nonEmptyStruct(member: 42))}.slice());
    if (len(r) != 1 || !AreEqual(r[0].Type(), TypeOf((nint)(1))) || r[0].Int() != 42) {
        Ꮡt.Errorf("takesNonEmpty returned %#v"u8, r);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object finalizerDidNotRunˢ = (@string)"finalizer did not run"u8;

public static void TestCallReturnsEmpty(ж<Δtesting.T> Ꮡt) {
    // Issue 21717: past-the-end pointer write in Call with
    // nonzero-sized frame and zero-sized return value.
    Δruntime.GC();
    ref var finalized = ref heap(new uint32(), out var Ꮡfinalized);
    var f = () => {
        var i = Ꮡ(new array<int64>(2)); // big enough to not be tinyalloc'd, so finalizer always runs when i dies
        Δruntime.SetFinalizer(i.OrTypedNil(), ([GoArrayDims(2)] ж<array<int64>> _) => {
            atomic.StoreUint32(Ꮡfinalized, 1);
        });
        return (new emptyStruct(nil), i);
    };
    var v = ValueOf((f).OrTypedNilFunc()).Call(default!)[0]; // out[0] should not alias out[1]'s memory, so the finalizer should run.
    var timeout = time.After((time.Duration)(5000000000L));
    while (atomic.LoadUint32(Ꮡfinalized) == 0) {
        var selᴛ2 = timeout;
        switch (trySelect(ᐸꟷ(selᴛ2, ꓸꓸꓸ))) {
        case 0 when selᴛ2.ꟷᐳ(out _): {
            Ꮡt.Fatal(finalizerDidNotRunˢ);
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

public static void TestMakeFunc(ж<Δtesting.T> Ꮡt) {
    ref var f = ref heap<Func<byte, nint, byte, two, byte, float32, byte, (byte i, nint j, byte k, two l, byte m, float32 n, byte o)>>(out var Ꮡf);
    f = dummy;
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
        Ꮡt.Errorf("Call returned %d, %d, %d, %v, %d, %g, %d; want 10, 20, 30, [40, 50], 60, 70, 80"u8, i, j, k, l, m, n, o);
    }
}

public static void TestMakeFuncInterface(ж<Δtesting.T> Ꮡt) {
    ref var fn = ref heap<Func<nint, nint>>(out var Ꮡfn);
    fn = (nint i) => i;
    var incr = (slice<reflectꓸValue> @in) => new reflectꓸValue[]{ValueOf((nint)(@in[0].Int() + 1))}.slice();
    var fv = MakeFunc(TypeOf((fn).OrTypedNilFunc()), incr);
    ValueOf(Ꮡfn).Elem().Set(fv);
    {
        nint r = fn(2); if (r != 3) {
            Ꮡt.Errorf("Call returned %d, want 3"u8, r);
        }
    }
    {
        var r = fv.Call(new reflectꓸValue[]{ValueOf((nint)(14))}.slice())[0].Int(); if (r != 15) {
            Ꮡt.Errorf("Call returned %d, want 15"u8, r);
        }
    }
    {
        nint r = fv.Interface()._<Func<nint, nint>>()(26); if (r != 27) {
            Ꮡt.Errorf("Call returned %d, want 27"u8, r);
        }
    }
}

public static void TestMakeFuncVariadic(ж<Δtesting.T> Ꮡt) {
    // Test that variadic arguments are packed into a slice and passed as last arg
    ref var fn = ref heap<Funcꓸꓸꓸ<nint, nint, slice<nint>>>(out var Ꮡfn);
    fn = slice<nint> (nint _, params ꓸꓸꓸnint @isʗp) => {
        var @is = @isʗp.sslice();
        return default!;
    };
    var fv = MakeFunc(TypeOf((fn).OrTypedNilFunc()), (slice<reflectꓸValue> @in) => @in[1..2]);
    ValueOf(Ꮡfn).Elem().Set(fv);
    var r = fn(1, 2, 3);
    if (r[0] != 2 || r[1] != 3) {
        Ꮡt.Errorf("Call returned [%v, %v]; want 2, 3"u8, r[0], r[1]);
    }
    r = fn(1, new nint[]{2, 3}.slice().ꓸꓸꓸ);
    if (r[0] != 2 || r[1] != 3) {
        Ꮡt.Errorf("Call returned [%v, %v]; want 2, 3"u8, r[0], r[1]);
    }
    r = fv.Call(new reflectꓸValue[]{ValueOf((nint)(1)), ValueOf((nint)(2)), ValueOf((nint)(3))}.slice())[0].Interface()._<slice<nint>>();
    if (r[0] != 2 || r[1] != 3) {
        Ꮡt.Errorf("Call returned [%v, %v]; want 2, 3"u8, r[0], r[1]);
    }
    r = fv.CallSlice(new reflectꓸValue[]{ValueOf((nint)(1)), ValueOf(new nint[]{2, 3}.slice())}.slice())[0].Interface()._<slice<nint>>();
    if (r[0] != 2 || r[1] != 3) {
        Ꮡt.Errorf("Call returned [%v, %v]; want 2, 3"u8, r[0], r[1]);
    }
    var f = fv.Interface()._<Funcꓸꓸꓸ<nint, nint, slice<nint>>>();
    r = f(1, 2, 3);
    if (r[0] != 2 || r[1] != 3) {
        Ꮡt.Errorf("Call returned [%v, %v]; want 2, 3"u8, r[0], r[1]);
    }
    r = f(1, new nint[]{2, 3}.slice().ꓸꓸꓸ);
    if (r[0] != 2 || r[1] != 3) {
        Ꮡt.Errorf("Call returned [%v, %v]; want 2, 3"u8, r[0], r[1]);
    }
}

// Dummy type that implements io.WriteCloser
[GoType] partial struct WC {
}

[GoRecv] public static (nint n, error err) Write(this ref WC w, slice<byte> p) {
    return (0, default!);
}

[GoRecv] public static error Close(this ref WC w) {
    return default!;
}

// Unnamed types should be promotable to named types.
[GoType("dyn")] internal partial struct TestMakeFuncValidReturnAssignments_T {
    internal nint a, b, c;
}

[GoType("dyn")] internal partial struct TestMakeFuncValidReturnAssignments_i {
    internal nint a, b, c;
}

public static void TestMakeFuncValidReturnAssignments(ж<Δtesting.T> Ꮡt) {
    // reflect.Values returned from the wrapped function should be assignment-converted
    // to the types returned by the result of MakeFunc.
    // Concrete types should be promotable to interfaces they implement.
    Func<error> f = default!;
    f = MakeFunc(TypeOf((f).OrTypedNilFunc()), (slice<reflectꓸValue> _) => new reflectꓸValue[]{ValueOf(Δio.EOF)}.slice()).Interface()._<Func<error>>();
    f();
    // Super-interfaces should be promotable to simpler interfaces.
    Func<Δio.Writer> g = default!;
    g = MakeFunc(TypeOf((g).OrTypedNilFunc()), (slice<reflectꓸValue> _) => {
        ref var w = ref heap<Δio.WriteCloser>(out var Ꮡw);

        w = new reflect_test_package.WCжWriteCloser(Ꮡ(new WC(nil)));
        return new reflectꓸValue[]{ValueOf(Ꮡw).Elem()}.slice();
    }).Interface()._<Func<Δio.Writer>>();
    g();
    // Channels should be promotable to directional channels.
    Func</*<-*/channel<nint>> h = default!;
    h = MakeFunc(TypeOf((h).OrTypedNilFunc()), (slice<reflectꓸValue> _) => new reflectꓸValue[]{ValueOf(new channel<nint>(0))}.slice()).Interface()._<Func</*<-*/channel<nint>>>();
    h();
    Func<TestMakeFuncValidReturnAssignments_T> i = default!;
    i = MakeFunc(TypeOf((i).OrTypedNilFunc()), (slice<reflectꓸValue> _) => new reflectꓸValue[]{ValueOf(new TestMakeFuncValidReturnAssignments_i(a: 1, b: 2, c: 3))}.slice()).Interface()._<Func<TestMakeFuncValidReturnAssignments_T>>();
    i();
}

[GoType("dyn")] internal partial struct TestMakeFuncInvalidReturnAssignments_T {
    internal nint a, b, c;
}

[GoType("dyn")] internal partial struct TestMakeFuncInvalidReturnAssignments_U {
    internal nint a, b, c;
}

public static void TestMakeFuncInvalidReturnAssignments(ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // Type doesn't implement the required interface.
    shouldPanic(""u8, () => {
        Func<error> f = default!;
        f = MakeFunc(TypeOf((f).OrTypedNilFunc()), (slice<reflectꓸValue> _) => new reflectꓸValue[]{ValueOf((nint)7)}.slice()).Interface()._<Func<error>>();
        f();
    });
    // Assigning to an interface with additional methods.
    shouldPanic(""u8, () => {
        Func<Δio.ReadWriteCloser> f = default!;
        f = MakeFunc(TypeOf((f).OrTypedNilFunc()), (slice<reflectꓸValue> _) => {
            ref var w = ref heap<Δio.WriteCloser>(out var Ꮡw);

            w = new reflect_test_package.WCжWriteCloser(Ꮡ(new WC(nil)));
            return new reflectꓸValue[]{ValueOf(Ꮡw).Elem()}.slice();
        }).Interface()._<Func<Δio.ReadWriteCloser>>();
        f();
    });
    // Directional channels can't be assigned to bidirectional ones.
    shouldPanic(""u8, () => {
        Func<channel<nint>> f = default!;
        f = MakeFunc(TypeOf((f).OrTypedNilFunc()), (slice<reflectꓸValue> _) => {
            /*<-*/channel<nint> c = new channel<nint>(0).WithDirection(GoChanDir.Recv);
            return new reflectꓸValue[]{ValueOf(c)}.slice();
        }).Interface()._<Func<channel<nint>>>();
        f();
    });
    // Two named types which are otherwise identical.
    shouldPanic(""u8, () => {
        Func<TestMakeFuncInvalidReturnAssignments_T> f = default!;
        f = MakeFunc(TypeOf((f).OrTypedNilFunc()), (slice<reflectꓸValue> _) => new reflectꓸValue[]{ValueOf(new TestMakeFuncInvalidReturnAssignments_U(a: 1, b: 2, c: 3))}.slice()).Interface()._<Func<TestMakeFuncInvalidReturnAssignments_T>>();
        f();
    });
}

[GoType] partial struct Point {
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
}

// Exercise no-argument/no-result paths.

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
[GoRecv] public static int64 Int64Method(this ref Point p, int64 x) {
    return x;
}

// This will be index 6.
[GoRecv] public static int32 Int32Method(this ref Point p, int32 x) {
    return x;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string distˢ = "Dist"u8;
internal static readonly @string noArgsˢ = "NoArgs"u8;

[GoType("dyn")] internal partial interface TestMethod_x {
    nint Dist(nint _Δp0);
}

public static void TestMethod(ж<Δtesting.T> Ꮡt) {
    // Non-curried method of type.
    ref var p = ref heap<Point>(out var Ꮡp);
    p = new Point(3, 4);
    var i = TypeOf(p).Method(1).Func.Call(new reflectꓸValue[]{ValueOf(p), ValueOf((nint)(10))}.slice())[0].Int();
    if (i != 250) {
        Ꮡt.Errorf("Type Method returned %d; want 250"u8, i);
    }
    var (m, ok) = TypeOf(p).MethodByName(distˢ);
    if (!ok) {
        Ꮡt.Fatalf("method by name failed"u8);
    }
    i = m.Func.Call(new reflectꓸValue[]{ValueOf(p), ValueOf((nint)(11))}.slice())[0].Int();
    if (i != 275) {
        Ꮡt.Errorf("Type MethodByName returned %d; want 275"u8, i);
    }
    (m, ok) = TypeOf(p).MethodByName(noArgsˢ);
    if (!ok) {
        Ꮡt.Fatalf("method by name failed"u8);
    }
    nint n = len(m.Func.Call(new reflectꓸValue[]{ValueOf(p)}.slice()));
    if (n != 0) {
        Ꮡt.Errorf("NoArgs returned %d values; want 0"u8, n);
    }
    i = TypeOf(Ꮡp).Method(1).Func.Call(new reflectꓸValue[]{ValueOf(Ꮡp), ValueOf((nint)(12))}.slice())[0].Int();
    if (i != 300) {
        Ꮡt.Errorf("Pointer Type Method returned %d; want 300"u8, i);
    }
    (m, ok) = TypeOf(Ꮡp).MethodByName(distˢ);
    if (!ok) {
        Ꮡt.Fatalf("ptr method by name failed"u8);
    }
    i = m.Func.Call(new reflectꓸValue[]{ValueOf(Ꮡp), ValueOf((nint)(13))}.slice())[0].Int();
    if (i != 325) {
        Ꮡt.Errorf("Pointer Type MethodByName returned %d; want 325"u8, i);
    }
    (m, ok) = TypeOf(Ꮡp).MethodByName(noArgsˢ);
    if (!ok) {
        Ꮡt.Fatalf("method by name failed"u8);
    }
    n = len(m.Func.Call(new reflectꓸValue[]{ValueOf(Ꮡp)}.slice()));
    if (n != 0) {
        Ꮡt.Errorf("NoArgs returned %d values; want 0"u8, n);
    }
    (_, ok) = TypeOf(Ꮡp).MethodByName("AA"u8);
    if (ok) {
        Ꮡt.Errorf(@"MethodByName(""AA"") should have failed"u8);
    }
    (_, ok) = TypeOf(Ꮡp).MethodByName("ZZ"u8);
    if (ok) {
        Ꮡt.Errorf(@"MethodByName(""ZZ"") should have failed"u8);
    }
    // Curried method of value.
    var tfunc = TypeOf(((Func<nint, nint>)(default!)).OrTypedNilFunc());
    var v = ValueOf(p).Method(1);
    {
        var tt = v.Type(); if (!AreEqual(tt, tfunc)) {
            Ꮡt.Errorf("Value Method Type is %s; want %s"u8, tt, tfunc);
        }
    }
    i = v.Call(new reflectꓸValue[]{ValueOf((nint)(14))}.slice())[0].Int();
    if (i != 350) {
        Ꮡt.Errorf("Value Method returned %d; want 350"u8, i);
    }
    v = ValueOf(p).MethodByName(distˢ);
    {
        var tt = v.Type(); if (!AreEqual(tt, tfunc)) {
            Ꮡt.Errorf("Value MethodByName Type is %s; want %s"u8, tt, tfunc);
        }
    }
    i = v.Call(new reflectꓸValue[]{ValueOf((nint)(15))}.slice())[0].Int();
    if (i != 375) {
        Ꮡt.Errorf("Value MethodByName returned %d; want 375"u8, i);
    }
    v = ValueOf(p).MethodByName(noArgsˢ);
    v.Call(default!);
    // Curried method of pointer.
    v = ValueOf(Ꮡp).Method(1);
    {
        var tt = v.Type(); if (!AreEqual(tt, tfunc)) {
            Ꮡt.Errorf("Pointer Value Method Type is %s; want %s"u8, tt, tfunc);
        }
    }
    i = v.Call(new reflectꓸValue[]{ValueOf((nint)(16))}.slice())[0].Int();
    if (i != 400) {
        Ꮡt.Errorf("Pointer Value Method returned %d; want 400"u8, i);
    }
    v = ValueOf(Ꮡp).MethodByName(distˢ);
    {
        var tt = v.Type(); if (!AreEqual(tt, tfunc)) {
            Ꮡt.Errorf("Pointer Value MethodByName Type is %s; want %s"u8, tt, tfunc);
        }
    }
    i = v.Call(new reflectꓸValue[]{ValueOf((nint)(17))}.slice())[0].Int();
    if (i != 425) {
        Ꮡt.Errorf("Pointer Value MethodByName returned %d; want 425"u8, i);
    }
    v = ValueOf(Ꮡp).MethodByName(noArgsˢ);
    v.Call(default!);
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
            Ꮡt.Errorf("Interface Method Type is %s; want %s"u8, tt, tfunc);
        }
    }
    i = v.Call(new reflectꓸValue[]{ValueOf((nint)(18))}.slice())[0].Int();
    if (i != 450) {
        Ꮡt.Errorf("Interface Method returned %d; want 450"u8, i);
    }
    v = pv.MethodByName(distˢ);
    {
        var tt = v.Type(); if (!AreEqual(tt, tfunc)) {
            Ꮡt.Errorf("Interface MethodByName Type is %s; want %s"u8, tt, tfunc);
        }
    }
    i = v.Call(new reflectꓸValue[]{ValueOf((nint)(19))}.slice())[0].Int();
    if (i != 475) {
        Ꮡt.Errorf("Interface MethodByName returned %d; want 475"u8, i);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string int64Methodˢ = "Int64Method"u8;
internal static readonly @string int32Methodˢ = "Int32Method"u8;

[GoType("dyn")] internal partial struct TestMethodValue_type {
    public TestMethod_x X;
}

public static void TestMethodValue(ж<Δtesting.T> Ꮡt) {
    ref var p = ref heap<Point>(out var Ꮡp);
    p = new Point(3, 4);
    int64 i = default!;
    // Check that method value have the same underlying code pointers.
    {
        var (p1, p2) = (ValueOf(new Point(1, 1)).Method(1), ValueOf(new Point(2, 2)).Method(1)); if (p1.Pointer() != p2.Pointer()) {
            Ꮡt.Errorf("methodValueCall mismatched: %v - %v"u8, p1, p2);
        }
    }
    // Curried method of value.
    var tfunc = TypeOf(((Func<nint, nint>)(default!)).OrTypedNilFunc());
    var v = ValueOf(p).Method(1);
    {
        var tt = v.Type(); if (!AreEqual(tt, tfunc)) {
            Ꮡt.Errorf("Value Method Type is %s; want %s"u8, tt, tfunc);
        }
    }
    i = ValueOf(v.Interface()).Call(new reflectꓸValue[]{ValueOf((nint)(10))}.slice())[0].Int();
    if (i != 250) {
        Ꮡt.Errorf("Value Method returned %d; want 250"u8, i);
    }
    v = ValueOf(p).MethodByName(distˢ);
    {
        var tt = v.Type(); if (!AreEqual(tt, tfunc)) {
            Ꮡt.Errorf("Value MethodByName Type is %s; want %s"u8, tt, tfunc);
        }
    }
    i = ValueOf(v.Interface()).Call(new reflectꓸValue[]{ValueOf((nint)(11))}.slice())[0].Int();
    if (i != 275) {
        Ꮡt.Errorf("Value MethodByName returned %d; want 275"u8, i);
    }
    v = ValueOf(p).MethodByName(noArgsˢ);
    ValueOf(v.Interface()).Call(default!);
    v.Interface()._<Action>()();
    // Curried method of pointer.
    v = ValueOf(Ꮡp).Method(1);
    {
        var tt = v.Type(); if (!AreEqual(tt, tfunc)) {
            Ꮡt.Errorf("Pointer Value Method Type is %s; want %s"u8, tt, tfunc);
        }
    }
    i = ValueOf(v.Interface()).Call(new reflectꓸValue[]{ValueOf((nint)(12))}.slice())[0].Int();
    if (i != 300) {
        Ꮡt.Errorf("Pointer Value Method returned %d; want 300"u8, i);
    }
    v = ValueOf(Ꮡp).MethodByName(distˢ);
    {
        var tt = v.Type(); if (!AreEqual(tt, tfunc)) {
            Ꮡt.Errorf("Pointer Value MethodByName Type is %s; want %s"u8, tt, tfunc);
        }
    }
    i = ValueOf(v.Interface()).Call(new reflectꓸValue[]{ValueOf((nint)(13))}.slice())[0].Int();
    if (i != 325) {
        Ꮡt.Errorf("Pointer Value MethodByName returned %d; want 325"u8, i);
    }
    v = ValueOf(Ꮡp).MethodByName(noArgsˢ);
    ValueOf(v.Interface()).Call(default!);
    v.Interface()._<Action>()();
    // Curried method of pointer to pointer.
    ref var pp = ref heap<ж<Point>>(out var Ꮡpp);
    pp = Ꮡp;
    v = ValueOf(Ꮡpp).Elem().Method(1);
    {
        var tt = v.Type(); if (!AreEqual(tt, tfunc)) {
            Ꮡt.Errorf("Pointer Pointer Value Method Type is %s; want %s"u8, tt, tfunc);
        }
    }
    i = ValueOf(v.Interface()).Call(new reflectꓸValue[]{ValueOf((nint)(14))}.slice())[0].Int();
    if (i != 350) {
        Ꮡt.Errorf("Pointer Pointer Value Method returned %d; want 350"u8, i);
    }
    v = ValueOf(Ꮡpp).Elem().MethodByName(distˢ);
    {
        var tt = v.Type(); if (!AreEqual(tt, tfunc)) {
            Ꮡt.Errorf("Pointer Pointer Value MethodByName Type is %s; want %s"u8, tt, tfunc);
        }
    }
    i = ValueOf(v.Interface()).Call(new reflectꓸValue[]{ValueOf((nint)(15))}.slice())[0].Int();
    if (i != 375) {
        Ꮡt.Errorf("Pointer Pointer Value MethodByName returned %d; want 375"u8, i);
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
            Ꮡt.Errorf("Interface Method Type is %s; want %s"u8, tt, tfunc);
        }
    }
    i = ValueOf(v.Interface()).Call(new reflectꓸValue[]{ValueOf((nint)(16))}.slice())[0].Int();
    if (i != 400) {
        Ꮡt.Errorf("Interface Method returned %d; want 400"u8, i);
    }
    v = pv.MethodByName(distˢ);
    {
        var tt = v.Type(); if (!AreEqual(tt, tfunc)) {
            Ꮡt.Errorf("Interface MethodByName Type is %s; want %s"u8, tt, tfunc);
        }
    }
    i = ValueOf(v.Interface()).Call(new reflectꓸValue[]{ValueOf((nint)(17))}.slice())[0].Int();
    if (i != 425) {
        Ꮡt.Errorf("Interface MethodByName returned %d; want 425"u8, i);
    }
    // For issue #33628: method args are not stored at the right offset
    // on amd64p32.
    var m64 = ValueOf(Ꮡp).MethodByName(int64Methodˢ).Interface()._<Func<int64, int64>>();
    {
        var x = m64(123); if (x != 123) {
            Ꮡt.Errorf("Int64Method returned %d; want 123"u8, x);
        }
    }
    var m32 = ValueOf(Ꮡp).MethodByName(int32Methodˢ).Interface()._<Func<int32, int32>>();
    {
        var x = m32(456); if (x != 456) {
            Ꮡt.Errorf("Int32Method returned %d; want 456"u8, x);
        }
    }
}

public static void TestVariadicMethodValue(ж<Δtesting.T> Ꮡt) {
    var p = new Point(3, 4);
    var points = new Point[]{new(20, 21), new(22, 23), new(24, 25)}.slice();
    var want = (int64)p.TotalDist(points[0], points[1], points[2]);
    // Variadic method of type.
    var tfunc = TypeOf(((Funcꓸꓸꓸ<Point, Point, nint>)(default!)).OrTypedNilFunc());
    {
        var tt = TypeOf(p).Method(4).Type; if (!AreEqual(tt, tfunc)) {
            Ꮡt.Errorf("Variadic Method Type from TypeOf is %s; want %s"u8, tt, tfunc);
        }
    }
    // Curried method of value.
    tfunc = TypeOf(((Funcꓸꓸꓸ<Point, nint>)(default!)).OrTypedNilFunc());
    var v = ValueOf(p).Method(4);
    {
        var tt = v.Type(); if (!AreEqual(tt, tfunc)) {
            Ꮡt.Errorf("Variadic Method Type is %s; want %s"u8, tt, tfunc);
        }
    }
    var i = ValueOf(v.Interface()).Call(new reflectꓸValue[]{ValueOf(points[0]), ValueOf(points[1]), ValueOf(points[2])}.slice())[0].Int();
    if (i != want) {
        Ꮡt.Errorf("Variadic Method returned %d; want %d"u8, i, want);
    }
    i = ValueOf(v.Interface()).CallSlice(new reflectꓸValue[]{ValueOf(points)}.slice())[0].Int();
    if (i != want) {
        Ꮡt.Errorf("Variadic Method CallSlice returned %d; want %d"u8, i, want);
    }
    var f = v.Interface()._<Funcꓸꓸꓸ<Point, nint>>();
    i = (int64)f(points[0], points[1], points[2]);
    if (i != want) {
        Ꮡt.Errorf("Variadic Method Interface returned %d; want %d"u8, i, want);
    }
    i = (int64)f(points.ꓸꓸꓸ);
    if (i != want) {
        Ꮡt.Errorf("Variadic Method Interface Slice returned %d; want %d"u8, i, want);
    }
}

[GoType] partial struct DirectIfaceT {
    internal ж<nint> p;
}

public static nint M(this DirectIfaceT d) {
    return d.p.Value;
}

public static void TestDirectIfaceMethod(ж<Δtesting.T> Ꮡt) {
    ref var x = ref heap<nint>(out var Ꮡx);
    x = 42;
    ref var v = ref heap<DirectIfaceT>(out var Ꮡv);
    v = new DirectIfaceT(Ꮡx);
    var typ = TypeOf(v);
    var (m, ok) = typ.MethodByName("M"u8);
    if (!ok) {
        Ꮡt.Fatalf("cannot find method M"u8);
    }
    var @in = new reflectꓸValue[]{ValueOf(v)}.slice();
    var @out = m.Func.Call(@in);
    {
        var got = @out[0].Int(); if (got != 42) {
            Ꮡt.Errorf("Call with value receiver got %d, want 42"u8, got);
        }
    }
    var pv = Ꮡv;
    typ = TypeOf(pv.OrTypedNil());
    (m, ok) = typ.MethodByName("M"u8);
    if (!ok) {
        Ꮡt.Fatalf("cannot find method M"u8);
    }
    @in = new reflectꓸValue[]{ValueOf(pv.OrTypedNil())}.slice();
    @out = m.Func.Call(@in);
    {
        var got = @out[0].Int(); if (got != 42) {
            Ꮡt.Errorf("Call with pointer receiver got %d, want 42"u8, got);
        }
    }
}

// Reflect version of $GOROOT/test/method5.go
// Concrete types implementing M method.
// Smaller than a word, word-sized, larger than a word.
// Value and pointer receivers.
[GoType] partial interface Tinter {
    (byte, nint) M(nint _Δp0, byte _Δp1);
}

[GoType("num:byte")] partial struct Tsmallv;

public static (byte, nint) M(this Tsmallv v, nint x, byte b) {
    return (b, x + (nint)(byte)v);
}

[GoType("num:byte")] partial struct Tsmallp;

[GoRecv] public static (byte, nint) M(this ref Tsmallp p, nint x, byte b) {
    return (b, x + (nint)(byte)(p));
}

[GoType("num:uintptr")] partial struct Twordv;

public static (byte, nint) M(this Twordv v, nint x, byte b) {
    return (b, x + (nint)(uintptr)v);
}

[GoType("num:uintptr")] partial struct Twordp;

[GoRecv] public static (byte, nint) M(this ref Twordp p, nint x, byte b) {
    return (b, x + (nint)(uintptr)(p));
}

[GoType("[2]uintptr")] partial struct Tbigv;

public static (byte, nint) M(this Tbigv v, nint x, byte b) {
    v = v.Clone();

    return (b, x + (nint)v[0] + (nint)v[1]);
}

[GoType("[2]uintptr")] partial struct Tbigp;

[GoRecv] public static (byte, nint) M(this ref Tbigp p, nint x, byte b) {
    return (b, x + (nint)p.Value[0] + (nint)p.Value[1]);
}

[GoType] partial interface tinter {
    (byte, nint) m(nint _Δp0, byte _Δp1);
}

// Embedding via pointer.
[GoType] partial struct Tm1 {
    public partial ref Tm2 Tm2 { get; }
}

[GoType] partial struct Tm2 {
    public partial ref ж<Tm3> Tm3 { get; }
}

[GoType] partial struct Tm3 {
    public partial ref ж<Tm4> Tm4 { get; }
}

[GoType] partial struct Tm4 {
}

public static (byte, nint) M(this Tm4 t4, nint x, byte b) {
    return (b, x + 40);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string methodˢ = "Method"u8;

public static void TestMethod5(ж<Δtesting.T> Ꮡt) {
    void CheckF(@string name, Func<nint, byte, (byte, nint)> f, nint inc) {
        var (b, x) = f(1000, 99);
        if (b != 99 || x != 1000 + inc) {
            Ꮡt.Errorf("%s(1000, 99) = %v, %v, want 99, %v"u8, name, b, x, 1000 + inc);
        }
    }
    var CheckFʗ1 = CheckF;
    void CheckV(@string name, reflectꓸValue i, nint inc) {
        var bx = i.Method(0).Call(new reflectꓸValue[]{ValueOf((nint)(1000)), ValueOf((byte)99)}.slice());
        var b = bx[0].Interface();
        var x = bx[1].Interface();
        if (!AreEqual(b, (byte)99) || !AreEqual(x, 1000 + inc)) {
            Ꮡt.Errorf("direct %s.M(1000, 99) = %v, %v, want 99, %v"u8, name, b, x, 1000 + inc);
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
    ref var sv = ref heap<Tsmallv>(out var Ꮡsv);
    sv = ((Tsmallv)1);
    CheckI("sv"u8, sv, 1);
    CheckI("&sv"u8, Ꮡsv, 1);
    ref var sp = ref heap<Tsmallp>(out var Ꮡsp);
    sp = ((Tsmallp)2);
    CheckI("&sp"u8, Ꮡsp, 2);
    ref var wv = ref heap<Twordv>(out var Ꮡwv);
    wv = ((Twordv)3);
    CheckI("wv"u8, wv, 3);
    CheckI("&wv"u8, Ꮡwv, 3);
    ref var wp = ref heap<Twordp>(out var Ꮡwp);
    wp = ((Twordp)4);
    CheckI("&wp"u8, Ꮡwp, 4);
    ref var bv = ref heap<Tbigv>(out var Ꮡbv);
    bv = ((Tbigv)new uintptr[]{5, 6}.array());
    CheckI("bv"u8, bv, 11);
    CheckI("&bv"u8, Ꮡbv, 11);
    ref var bp = ref heap<Tbigp>(out var Ꮡbp);
    bp = ((Tbigp)new uintptr[]{7, 8}.array());
    CheckI("&bp"u8, Ꮡbp, 15);
    ref var t4 = ref heap<Tm4>(out var Ꮡt4);
    t4 = new Tm4(nil);
    ref var t3 = ref heap<Tm3>(out var Ꮡt3);
    t3 = new Tm3(Ꮡt4);
    ref var t2 = ref heap<Tm2>(out var Ꮡt2);
    t2 = new Tm2(Ꮡt3);
    ref var t1 = ref heap<Tm1>(out var Ꮡt1);
    t1 = new Tm1(t2);
    CheckI("t4"u8, t4, 40);
    CheckI("&t4"u8, Ꮡt4, 40);
    CheckI("t3"u8, t3, 40);
    CheckI("&t3"u8, Ꮡt3, 40);
    CheckI("t2"u8, t2, 40);
    CheckI("&t2"u8, Ꮡt2, 40);
    CheckI("t1"u8, t1, 40);
    CheckI("&t1"u8, Ꮡt1, 40);
    ref var tnil = ref heap<Tinter>(out var Ꮡtnil);
    ref var vnil = ref heap<reflectꓸValue>(out var Ꮡvnil);
    vnil = ValueOf(Ꮡtnil).Elem();
    var vnilʗ1 = vnil;
    shouldPanic(methodˢ, () => {
        vnilʗ1.Method(0);
    });
}

[GoType("dyn")] internal partial struct TestInterfaceSet_s {
    public any I;
    public TestMethod_x P;
}

public static void TestInterfaceSet(ж<Δtesting.T> Ꮡt) {
    var p = Ꮡ(new Point(3, 4));
    ref var s = ref heap(new TestInterfaceSet_s(), out var Ꮡs);
    var sv = ValueOf(Ꮡs).Elem();
    sv.Field(0).Set(ValueOf(p.OrTypedNil()));
    {
        var q = s.I._<ж<Point>>(); if (q != p) {
            Ꮡt.Errorf("i: have %p want %p"u8, q.OrTypedNil(), p.OrTypedNil());
        }
    }
    var pv = sv.Field(1);
    pv.Set(ValueOf(p.OrTypedNil()));
    {
        var q = s.P._<ж<Point>>(); if (q != p) {
            Ꮡt.Errorf("i: have %p want %p"u8, q.OrTypedNil(), p.OrTypedNil());
        }
    }
    var i = pv.Method(0).Call(new reflectꓸValue[]{ValueOf((nint)(10))}.slice())[0].Int();
    if (i != 250) {
        Ꮡt.Errorf("Interface Method returned %d; want 250"u8, i);
    }
}

[GoType] partial struct T1 {
    internal @string a;
    [GoEmbedded] internal nint @int;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string intˢ = "int"u8;
internal static readonly object noFieldIntˢ = (@string)"no field 'int'"u8;
internal static readonly object fieldIndexShouldBe1Isˢ = (@string)"field index should be 1; is"u8;

public static void TestAnonymousFields(ж<Δtesting.T> Ꮡt) {
    Δreflect.StructField field = default!;
    bool ok = default!;
    T1 t1 = default!;
    var type1 = TypeOf(t1);
    {
        (field, ok) = type1.FieldByName(intˢ); if (!ok) {
            Ꮡt.Fatal(noFieldIntˢ);
        }
    }
    if (field.Index[0] != 1) {
        Ꮡt.Error(fieldIndexShouldBe1Isˢ, field.Index);
    }
}

[GoType] partial struct FTest {
    internal any s;
    internal @string name;
    internal slice<nint> index;
    internal nint value;
}

[GoType] partial struct D1 {
    internal nint d;
}

[GoType] partial struct D2 {
    internal nint d;
}

[GoType] partial struct S0 {
    public nint A, B, C;
    public partial ref D1 D1 { get; }
    public partial ref D2 D2 { get; }
}

[GoType] partial struct S1 {
    public nint B;
    public partial ref S0 S0 { get; }
}

[GoType] partial struct S2 {
    public nint A;
    public partial ref ж<S1> S1 { get; }
}

[GoType] partial struct S1x {
    public partial ref S1 S1 { get; }
}

[GoType] partial struct S1y {
    public partial ref S1 S1 { get; }
}

[GoType] partial struct S3 {
    public partial ref S1x S1x { get; }
    public partial ref S2 S2 { get; }
    public nint D, E;
    public partial ref ж<S1y> S1y { get; }
}

[GoType] partial struct S4 {
    public partial ref ж<S4> ΔS4 { get; }
    public nint A;
}

// The X in S6 and S7 annihilate, but they also block the X in S8.S9.
[GoType] partial struct S5 {
    public partial ref S6 S6 { get; }
    public partial ref S7 S7 { get; }
    public partial ref S8 S8 { get; }
}

[GoType] partial struct S6 {
    public nint X;
}

[GoType("S6")] partial struct S7;

[GoType] partial struct S8 {
    public partial ref S9 S9 { get; }
}

[GoType] partial struct S9 {
    public nint X;
    public nint Y;
}

// The X in S11.S6 and S12.S6 annihilate, but they also block the X in S13.S8.S9.
[GoType] partial struct S10 {
    public partial ref S11 S11 { get; }
    public partial ref S12 S12 { get; }
    public partial ref S13 S13 { get; }
}

[GoType] partial struct S11 {
    public partial ref S6 S6 { get; }
}

[GoType] partial struct S12 {
    public partial ref S6 S6 { get; }
}

[GoType] partial struct S13 {
    public partial ref S8 S8 { get; }
}

// The X in S15.S11.S1 and S16.S11.S1 annihilate.
[GoType] partial struct S14 {
    public partial ref S15 S15 { get; }
    public partial ref S16 S16 { get; }
}

[GoType] partial struct S15 {
    public partial ref S11 S11 { get; }
}

[GoType] partial struct S16 {
    public partial ref S11 S11 { get; }
}

internal static slice<FTest> fieldTests = new FTest[]{
    new(new EmptyStruct(), ""u8, default!, 0),
    new(new EmptyStruct(), "Foo"u8, default!, 0),
    new(new S0(A: (rune)'a'), "A"u8, new nint[]{0}.slice(), (rune)'a'),
    new(new S0(nil), "D"u8, default!, 0),
    new(new S1(S0: new S0(A: (rune)'a')), "A"u8, new nint[]{1, 0}.slice(), (rune)'a'),
    new(new S1(B: (rune)'b'), "B"u8, new nint[]{0}.slice(), (rune)'b'),
    new(new S1(nil), "S0"u8, new nint[]{1}.slice(), 0),
    new(new S1(S0: new S0(C: (rune)'c')), "C"u8, new nint[]{1, 2}.slice(), (rune)'c'),
    new(new S2(A: (rune)'a'), "A"u8, new nint[]{0}.slice(), (rune)'a'),
    new(new S2(nil), "S1"u8, new nint[]{1}.slice(), 0),
    new(new S2(S1: Ꮡ(new S1(B: (rune)'b'))), "B"u8, new nint[]{1, 0}.slice(), (rune)'b'),
    new(new S2(S1: Ꮡ(new S1(S0: new S0(C: (rune)'c')))), "C"u8, new nint[]{1, 1, 2}.slice(), (rune)'c'),
    new(new S2(nil), "D"u8, default!, 0),
    new(new S3(nil), "S1"u8, default!, 0),
    new(new S3(S2: new S2(A: (rune)'a')), "A"u8, new nint[]{1, 0}.slice(), (rune)'a'),
    new(new S3(nil), "B"u8, default!, 0),
    new(new S3(D: (rune)'d'), "D"u8, new nint[]{2}.slice(), 0),
    new(new S3(E: (rune)'e'), "E"u8, new nint[]{3}.slice(), (rune)'e'),
    new(new S4(A: (rune)'a'), "A"u8, new nint[]{1}.slice(), (rune)'a'),
    new(new S4(nil), "B"u8, default!, 0),
    new(new S5(nil), "X"u8, default!, 0),
    new(new S5(nil), "Y"u8, new nint[]{2, 0, 1}.slice(), 0),
    new(new S10(nil), "X"u8, default!, 0),
    new(new S10(nil), "Y"u8, new nint[]{2, 0, 0, 1}.slice(), 0),
    new(new S14(nil), "X"u8, default!, 0)
}.slice();

public static void TestFieldByIndex(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, test) in fieldTests) {
        var s = TypeOf(test.s);
        var f = s.FieldByIndex(test.index);
        if (f.Name != ""u8){
            if (test.index != default!){
                if (f.Name != test.name) {
                    Ꮡt.Errorf("%s.%s found; want %s"u8, s.Name(), f.Name, test.name);
                }
            } else {
                Ꮡt.Errorf("%s.%s found"u8, s.Name(), f.Name);
            }
        } else 
        if (len(test.index) > 0) {
            Ꮡt.Errorf("%s.%s not found"u8, s.Name(), test.name);
        }
        if (test.value != 0) {
            var v = ValueOf(test.s).FieldByIndex(test.index);
            if (v.IsValid()){
                {
                    var (x, ok) = v.Interface()._<nint>(ᐧ); if (ok){
                        if (x != test.value) {
                            Ꮡt.Errorf("%s%v is %d; want %d"u8, s.Name(), test.index, x, test.value);
                        }
                    } else {
                        Ꮡt.Errorf("%s%v value not an int"u8, s.Name(), test.index);
                    }
                }
            } else {
                Ꮡt.Errorf("%s%v value not found"u8, s.Name(), test.index);
            }
        }
    }
}

public static void TestFieldByName(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, test) in fieldTests) {
        var s = TypeOf(test.s);
        var (f, found) = s.FieldByName(test.name);
        if (found){
            if (test.index != default!){
                // Verify field depth and index.
                if (len(f.Index) != len(test.index)){
                    Ꮡt.Errorf("%s.%s depth %d; want %d: %v vs %v"u8, s.Name(), test.name, len(f.Index), len(test.index), f.Index, test.index);
                } else {
                    foreach (var (i, x) in f.Index) {
                        if (x != test.index[i]) {
                            Ꮡt.Errorf("%s.%s.Index[%d] is %d; want %d"u8, s.Name(), test.name, i, x, test.index[i]);
                        }
                    }
                }
            } else {
                Ꮡt.Errorf("%s.%s found"u8, s.Name(), f.Name);
            }
        } else 
        if (len(test.index) > 0) {
            Ꮡt.Errorf("%s.%s not found"u8, s.Name(), test.name);
        }
        if (test.value != 0) {
            var v = ValueOf(test.s).FieldByName(test.name);
            if (v.IsValid()){
                {
                    var (x, ok) = v.Interface()._<nint>(ᐧ); if (ok){
                        if (x != test.value) {
                            Ꮡt.Errorf("%s.%s is %d; want %d"u8, s.Name(), test.name, x, test.value);
                        }
                    } else {
                        Ꮡt.Errorf("%s.%s value not an int"u8, s.Name(), test.name);
                    }
                }
            } else {
                Ꮡt.Errorf("%s.%s value not found"u8, s.Name(), test.name);
            }
        }
    }
}

[GoType("dyn")] internal partial struct TestImportPath_tests {
    internal reflectꓸType t;
    internal @string path;
}

public static void TestImportPath(ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var tests = new TestImportPath_tests[]{
        new(TypeOf(Ꮡ(new base64.Encoding(nil))).Elem(), "encoding/base64"u8),
        new(TypeOf((nint)0), ""u8),
        new(TypeOf((int8)0), ""u8),
        new(TypeOf((int16)0), ""u8),
        new(TypeOf((int32)0), ""u8),
        new(TypeOf((int64)0), ""u8),
        new(TypeOf((nuint)0), ""u8),
        new(TypeOf((uint8)0), ""u8),
        new(TypeOf((uint16)0), ""u8),
        new(TypeOf((uint32)0), ""u8),
        new(TypeOf((uint64)0), ""u8),
        new(TypeOf((uintptr)0), ""u8),
        new(TypeOf((float32)0F), ""u8),
        new(TypeOf((float64)0D), ""u8),
        new(TypeOf((complex64)0F), ""u8),
        new(TypeOf((complex128)0D), ""u8),
        new(TypeOf((byte)0), ""u8),
        new(TypeOf((rune)0), ""u8),
        new(TypeOf(slice<byte>(default!)), ""u8),
        new(TypeOf(slice<rune>(default!)), ""u8),
        new(TypeOf(((@string)""u8)), ""u8),
        new(TypeOf(((ж<any>)nil)).Elem(), ""u8),
        new(TypeOf(((ж<byte>)nil)), ""u8),
        new(TypeOf(((ж<rune>)nil)), ""u8),
        new(TypeOf(((ж<int64>)nil)), ""u8),
        new(TypeOf(new map<@string, nint>{}), ""u8),
        new(TypeOf(((ж<error>)nil)).Elem(), ""u8),
        new(TypeOf(((ж<Point>)nil)), ""u8),
        new(TypeOf(((ж<Point>)nil)).Elem(), "reflect_test"u8)
    }.slice();
    foreach (var (_, test) in tests) {
        {
            @string path = test.t.PkgPath(); if (path != test.path) {
                Ꮡt.Errorf("%v.PkgPath() = %q, want %q"u8, test.t, path, test.path);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testStructˢ = "testStruct"u8;
internal static readonly @string localOtherPkgFieldsˢ = "localOtherPkgFields"u8;

[GoLocalName("x")] [GoType("num:nint")] internal partial struct TestFieldPkgPath_x;

[GoType("dyn")] internal partial struct TestFieldPkgPath_i {
    public @string Exported;
    internal @string unexported;
    public partial ref global::go.reflect_internal_test_package.OtherPkgFields OtherPkgFields { get; }
    [GoEmbedded] internal nint @int; // issue 21702
    internal partial ref ж<TestFieldPkgPath_x> x { get; } // issue 21122
}

[GoType("dyn")] internal partial struct TestFieldPkgPath_pkgpathTest {
    internal slice<nint> index;
    internal @string pkgPath;
    internal bool embedded;
    internal bool exported;
}

[GoLocalName("localOtherPkgFields")] [GoType("global::go.reflect_internal_test_package.OtherPkgFields")] internal partial struct TestFieldPkgPath_localOtherPkgFields;

public static void TestFieldPkgPath(ж<Δtesting.T> Ꮡt) {
    ref var typ = ref heap<reflectꓸType>(out var Ꮡtyp);
    typ = TypeOf(new TestFieldPkgPath_i());
    void checkPkgPath(@string name, slice<TestFieldPkgPath_pkgpathTest> s) {
        foreach (var (_, test) in s) {
            var f = Ꮡtyp.ValueSlot.FieldByIndex(test.index);
            {
                @string got = f.PkgPath;
                @string want = test.pkgPath; if (got != want) {
                    Ꮡt.Errorf("%s: Field(%d).PkgPath = %q, want %q"u8, name, test.index, got, want);
                }
            }
            {
                var (got, want) = (f.Anonymous, test.embedded); if (got != want) {
                    Ꮡt.Errorf("%s: Field(%d).Anonymous = %v, want %v"u8, name, test.index, got, want);
                }
            }
            {
                var (got, want) = (f.IsExported(), test.exported); if (got != want) {
                    Ꮡt.Errorf("%s: Field(%d).IsExported = %v, want %v"u8, name, test.index, got, want);
                }
            }
        }
    }
    checkPkgPath(testStructˢ, new TestFieldPkgPath_pkgpathTest[]{
        new(new nint[]{0}.slice(), ""u8, false, true), // Exported

        new(new nint[]{1}.slice(), "reflect_test"u8, false, false), // unexported

        new(new nint[]{2}.slice(), ""u8, true, true), // OtherPkgFields

        new(new nint[]{2, 0}.slice(), ""u8, false, true), // OtherExported

        new(new nint[]{2, 1}.slice(), "reflect"u8, false, false), // otherUnexported

        new(new nint[]{3}.slice(), "reflect_test"u8, true, false), // int

        new(new nint[]{4}.slice(), "reflect_test"u8, true, false)
    }.slice());
    // *x
    typ = TypeOf(new TestFieldPkgPath_localOtherPkgFields(new global::go.reflect_internal_test_package.OtherPkgFields(nil)));
    checkPkgPath(localOtherPkgFieldsˢ, new TestFieldPkgPath_pkgpathTest[]{
        new(new nint[]{0}.slice(), ""u8, false, true), // OtherExported

        new(new nint[]{1}.slice(), "reflect"u8, false, false)
    }.slice());
}

[GoType("dyn")] internal partial interface TestMethodPkgPath_I {
    void x();
    void X();
}

[GoType("dyn")] internal partial interface TestMethodPkgPath_i :
    TestMethodPkgPath_I
{
    void y();
    void Y();
}

[GoType("dyn")] internal partial struct TestMethodPkgPath_tests {
    internal @string name;
    internal @string pkgPath;
    internal bool exported;
}

// otherUnexported
public static void TestMethodPkgPath(ж<Δtesting.T> Ꮡt) {
    var typ = TypeOf(((ж<TestMethodPkgPath_i>)nil)).Elem();
    var tests = new TestMethodPkgPath_tests[]{
        new("X"u8, ""u8, true),
        new("Y"u8, ""u8, true),
        new("x"u8, "reflect_test"u8, false),
        new("y"u8, "reflect_test"u8, false)
    }.slice();
    foreach (var (_, test) in tests) {
        var (m, _) = typ.MethodByName(test.name);
        {
            @string got = m.PkgPath;
            @string want = test.pkgPath; if (got != want) {
                Ꮡt.Errorf("MethodByName(%q).PkgPath = %q, want %q"u8, test.name, got, want);
            }
        }
        {
            var (got, want) = (m.IsExported(), test.exported); if (got != want) {
                Ꮡt.Errorf("MethodByName(%q).IsExported = %v, want %v"u8, test.name, got, want);
            }
        }
    }
}

public static void TestVariadicType(ж<Δtesting.T> Ꮡt) {
    // Test example from Type documentation.
    Actionꓸꓸꓸ<nint, float64> f = default!;
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
    Ꮡt.Errorf("want NumIn() = 2, In(0) = int, In(1) = []float64"u8);
    @string s = fmt.Sprintf("have NumIn() = %d"u8, typ.NumIn());
    for (nint i = 0; i < typ.NumIn(); i++) {
        s += fmt.Sprintf(", In(%d) = %s"u8, i, typ.In(i));
    }
    Ꮡt.Error(s);
}

[GoType] partial struct inner {
    internal nint x;
}

[GoType] partial struct outer {
    internal nint y;
    internal partial ref inner inner { get; }
}

[GoRecv] internal static void M(this ref inner _) {
}

[GoRecv] internal static void M(this ref outer _) {
}

public static void TestNestedMethods(ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var typ = TypeOf(((ж<outer>)nil));
    if (typ.NumMethod() != 1 || (uintptr)typ.Method(0).Func.UnsafePointer() != (uintptr)ValueOf(((Action<ж<outer>>)(M))).UnsafePointer()) {
        Ꮡt.Errorf("Wrong method table for outer: (M=%p)"u8, ((Action<ж<outer>>)(M)));
        for (nint i = 0; i < typ.NumMethod(); i++) {
            var m = typ.Method(i);
            Ꮡt.Errorf("\t%d: %s %p\n"u8, i, m.Name, @unsafe.Pointer.OrTypedNil((uintptr)m.Func.UnsafePointer()));
        }
    }
}

[GoType] partial struct unexp {
}

[GoRecv] internal static (int32, int8) f(this ref unexp _) {
    return (7, 7);
}

[GoRecv] internal static (int64, int8) g(this ref unexp _) {
    return (8, 8);
}

[GoType] partial interface unexpI {
    (int32, int8) f();
}

public static void TestUnexportedMethods(ж<Δtesting.T> Ꮡt) {
    var typ = TypeOf(@new<unexp>());
    {
        nint got = typ.NumMethod(); if (got != 0) {
            Ꮡt.Errorf("NumMethod=%d, want 0 satisfied methods"u8, got);
        }
    }
    typ = TypeOf(((ж<unexpI>)nil));
    {
        nint got = typ.Elem().NumMethod(); if (got != 1) {
            Ꮡt.Errorf("NumMethod=%d, want 1 satisfied methods"u8, got);
        }
    }
}

[GoType] partial struct InnerInt {
    public nint X;
}

[GoType] partial struct OuterInt {
    public nint Y;
    public partial ref InnerInt InnerInt { get; }
}

[GoRecv] public static nint M(this ref InnerInt i) {
    return i.X;
}

public static void TestEmbeddedMethods(ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var typ = TypeOf(((ж<OuterInt>)nil));
    if (typ.NumMethod() != 1 || (uintptr)typ.Method(0).Func.UnsafePointer() != (uintptr)ValueOf(((Func<ж<OuterInt>, nint>)(M))).UnsafePointer()) {
        Ꮡt.Errorf("Wrong method table for OuterInt: (m=%p)"u8, ((Func<ж<OuterInt>, nint>)(M)));
        for (nint iΔ1 = 0; iΔ1 < typ.NumMethod(); iΔ1++) {
            var m = typ.Method(iΔ1);
            Ꮡt.Errorf("\t%d: %s %p\n"u8, iΔ1, m.Name, @unsafe.Pointer.OrTypedNil((uintptr)m.Func.UnsafePointer()));
        }
    }
    var i = Ꮡ(new InnerInt(3));
    {
        var v = ValueOf(i.OrTypedNil()).Method(0).Call(default!)[0].Int(); if (v != 3) {
            Ꮡt.Errorf("i.M() = %d, want 3"u8, v);
        }
    }
    var o = Ꮡ(new OuterInt(1, new InnerInt(2)));
    {
        var v = ValueOf(o.OrTypedNil()).Method(0).Call(default!)[0].Int(); if (v != 2) {
            Ꮡt.Errorf("i.M() = %d, want 2"u8, v);
        }
    }
    var f = ((Func<ж<OuterInt>, nint>)(M));
    {
        nint v = f(o); if (v != 2) {
            Ꮡt.Errorf("f(o) = %d, want 2"u8, v);
        }
    }
}

public delegate error FuncDDD(params ꓸꓸꓸany _ʗp);

public static void M(this FuncDDD f) {
}

public static void TestNumMethodOnDDD(ж<Δtesting.T> Ꮡt) {
    var rv = ValueOf((default(FuncDDD)!).OrTypedNilFunc());
    {
        nint n = rv.NumMethod(); if (n != 1) {
            Ꮡt.Fatalf("NumMethod()=%d, want 1"u8, n);
        }
    }
}

public static void TestPtrTo(ж<Δtesting.T> Ꮡt) {
    // This block of code means that the ptrToThis field of the
    // reflect data for *unsafe.Pointer is non zero, see
    // https://golang.org/issue/19003
    ref var x = ref heap<@unsafe.Pointer>(out var Ꮡx);
    ref var y = ref heap<ж<@unsafe.Pointer>>(out var Ꮡy);

    y = Ꮡx;
    ж<ж<@unsafe.Pointer>> z = Ꮡy;
    nint i = default!;
    var typ = TypeOf(z.OrTypedNil());
    for (i = 0; i < 100; i++) {
        typ = PointerTo(typ);
    }
    for (i = 0; i < 100; i++) {
        typ = typ.Elem();
    }
    if (!AreEqual(typ, TypeOf(z.OrTypedNil()))) {
        Ꮡt.Errorf("after 100 PointerTo and Elem, have %s, want %s"u8, typ, TypeOf(z.OrTypedNil()));
    }
}

[GoLocalName("T")] [GoType("ж<uintptr>")] internal partial class TestPtrToGC_T;

public static void TestPtrToGC(ж<Δtesting.T> Ꮡt) {
    var tt = TypeOf(((TestPtrToGC_T)nil));
    var pt = PointerTo(tt);
    const nint n = 100;
    slice<any> x = default!;
    for (nint i = 0; i < n; i++) {
        var v = New(pt);
        var p = @new<ж<uintptr>>();
        p.ValueSlot = @new<uintptr>();
        (p.ValueSlot).Value = (uintptr)i;
        v.Elem().Set(ValueOf(p.OrTypedNil()).Convert(pt));
        x = append(x, v.Interface());
    }
    Δruntime.GC();
    foreach (var (i, xi) in x) {
        var k = ValueOf(xi).Elem().Elem().Elem().Interface()._<uintptr>();
        if (k != (uintptr)i) {
            Ꮡt.Errorf("lost x[%d] = %d, want %d"u8, i, k, i);
        }
    }
}

[GoType("dyn")] internal partial struct TestAddr_p {
    public nint X, Y;
}

[GoType("dyn")] internal partial struct TestAddr_s {
    public ж<bool> B;
}

public static void TestAddr(ж<Δtesting.T> Ꮡt) {
    ref var p = ref heap(new TestAddr_p(), out var Ꮡp);
    var v = ValueOf(Ꮡp);
    v = v.Elem();
    v = v.Addr();
    v = v.Elem();
    v = v.Field(0);
    v.SetInt(2);
    if (p.X != 2) {
        Ꮡt.Errorf("Addr.Elem.Set failed to set value"u8);
    }
    // Again but take address of the ValueOf value.
    // Exercises generation of PtrTypes not present in the binary.
    ref var q = ref heap<ж<TestAddr_p>>(out var Ꮡq);
    q = Ꮡp;
    v = ValueOf(Ꮡq).Elem();
    v = v.Addr();
    v = v.Elem();
    v = v.Elem();
    v = v.Addr();
    v = v.Elem();
    v = v.Field(0);
    v.SetInt(3);
    if (p.X != 3) {
        Ꮡt.Errorf("Addr.Elem.Set failed to set value"u8);
    }
    // Starting without pointer we should get changed value
    // in interface.
    ref var qq = ref heap<TestAddr_p>(out var Ꮡqq);
    qq = p;
    v = ValueOf(Ꮡqq).Elem();
    var v0 = v;
    v = v.Addr();
    v = v.Elem();
    v = v.Field(0);
    v.SetInt(4);
    if (p.X != 3) {
        // should be unchanged from last time
        Ꮡt.Errorf("somehow value Set changed original p"u8);
    }
    p = v0.Interface()._<TestAddr_p>();
    if (p.X != 4) {
        Ꮡt.Errorf("Addr.Elem.Set valued to set value in top value"u8);
    }
    // Verify that taking the address of a type gives us a pointer
    // which we can convert back using the usual interface
    // notation.
    ref var s = ref heap(new TestAddr_s(), out var Ꮡs);
    var ps = ValueOf(Ꮡs).Elem().Field(0).Addr().Interface();
    (ps._<ж<ж<bool>>>()).ValueSlot = @new<bool>();
    if (s.B == nil) {
        Ꮡt.Errorf("Addr.Interface direct assignment failed"u8);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingMallocCountInˢ = (@string)"skipping malloc count in short mode"u8;
internal static readonly object skippingGomaxprocs1ˢ = (@string)"skipping; GOMAXPROCS>1"u8;

internal static void noAlloc(ж<Δtesting.T> Ꮡt, nint n, Action<nint> f) {
    if (Δtesting.Short()) {
        Ꮡt.Skip(skippingMallocCountInˢ);
    }
    if (Δruntime.GOMAXPROCS(0) > 1) {
        Ꮡt.Skip(skippingGomaxprocs1ˢ);
    }
    nint i = -1;
    var allocs = Δtesting.AllocsPerRun(n, () => {
        f(i);
        i++;
    });
    if (allocs > 0D) {
        Ꮡt.Errorf("%d iterations: got %v mallocs, want 0"u8, n, allocs);
    }
}

[GoType("dyn")] internal partial struct TestAllocations_type {
    internal nint f;
}

public static void TestAllocations(ж<Δtesting.T> Ꮡt) {
    noAlloc(Ꮡt, 100, (nint j) => {
        any i = default!;
        reflectꓸValue v = new(nil);
        i = 42 + j;
        v = ValueOf(i);
        if ((nint)v.Int() != 42 + j) {
            throw panic("wrong int");
        }
    });
    noAlloc(Ꮡt, 100, (nint j) => {
        any i = default!;
        reflectꓸValue v = new(nil);
        i = new nint[]{j, j, j}.array();
        v = ValueOf(i);
        if (v.Len() != 3) {
            throw panic("wrong length");
        }
    });
    noAlloc(Ꮡt, 100, (nint j) => {
        any i = default!;
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
        noAlloc(Ꮡt, 100, (nint _) => {
            if (typʗ1.Field(0).Index[0] != 0) {
                throw panic("wrong field index");
            }
        });
    }
}

public static void TestSmallNegativeInt(ж<Δtesting.T> Ꮡt) {
    var i = (int16)(-1);
    var v = ValueOf(i);
    if (v.Int() != -1) {
        Ꮡt.Errorf("int16(-1).Int() returned %v"u8, v.Int());
    }
}

public static void TestIndex(ж<Δtesting.T> Ꮡt) {
    var xs = new byte[]{1, 2, 3, 4, 5, 6, 7, 8}.slice();
    var v = ValueOf(xs).Index(3).Interface()._<byte>();
    if (v != xs[3]) {
        Ꮡt.Errorf("xs.Index(3) = %v; expected %v"u8, v, xs[3]);
    }
    var xa = new byte[]{10, 20, 30, 40, 50, 60, 70, 80}.array();
    v = ValueOf(xa).Index(2).Interface()._<byte>();
    if (v != xa[2]) {
        Ꮡt.Errorf("xa.Index(2) = %v; expected %v"u8, v, xa[2]);
    }
    @string s = "0123456789"u8;
    v = ValueOf(s).Index(3).Interface()._<byte>();
    if (v != s[3]) {
        Ꮡt.Errorf("s.Index(3) = %v; expected %v"u8, v, s[3]);
    }
}

public static void TestSlice(ж<Δtesting.T> Ꮡt) {
    ref var xs = ref heap<slice<nint>>(out var Ꮡxs);
    xs = new nint[]{1, 2, 3, 4, 5, 6, 7, 8}.slice();
    var v = ValueOf(xs).Slice(3, 5).Interface()._<slice<nint>>();
    if (len(v) != 2) {
        Ꮡt.Errorf("len(xs.Slice(3, 5)) = %d"u8, len(v));
    }
    if (cap(v) != 5) {
        Ꮡt.Errorf("cap(xs.Slice(3, 5)) = %d"u8, cap(v));
    }
    if (!DeepEqual(v[0..5], xs[3..])) {
        Ꮡt.Errorf("xs.Slice(3, 5)[0:5] = %v"u8, v[0..5]);
    }
    ref var xa = ref heap<array<nint>>(out var Ꮡxa);
    xa = new nint[]{10, 20, 30, 40, 50, 60, 70, 80}.array();
    v = ValueOf(Ꮡxa).Elem().Slice(2, 5).Interface()._<slice<nint>>();
    if (len(v) != 3) {
        Ꮡt.Errorf("len(xa.Slice(2, 5)) = %d"u8, len(v));
    }
    if (cap(v) != 6) {
        Ꮡt.Errorf("cap(xa.Slice(2, 5)) = %d"u8, cap(v));
    }
    if (!DeepEqual(v[0..6], xa[2..])) {
        Ꮡt.Errorf("xs.Slice(2, 5)[0:6] = %v"u8, v[0..6]);
    }
    @string s = "0123456789"u8;
    @string vs = ValueOf(s).Slice(3, 5).Interface()._<@string>();
    if (vs != s[3..5]) {
        Ꮡt.Errorf("s.Slice(3, 5) = %q; expected %q"u8, vs, s[3..5]);
    }
    var rv = ValueOf(Ꮡxs).Elem();
    rv = rv.Slice(3, 4);
    @unsafe.Pointer ptr2 = (uintptr)rv.UnsafePointer();
    rv = rv.Slice(5, 5);
    @unsafe.Pointer ptr3 = (uintptr)rv.UnsafePointer();
    if (ptr3 != ptr2) {
        Ꮡt.Errorf("xs.Slice(3,4).Slice3(5,5).UnsafePointer() = %p, want %p"u8, @unsafe.Pointer.OrTypedNil(ptr3), @unsafe.Pointer.OrTypedNil(ptr2));
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string slice3ˢ = "Slice3"u8;
internal static readonly @string helloWorldˢ = "hello world"u8;

public static void TestSlice3(ж<Δtesting.T> Ꮡt) {
    ref var xs = ref heap<slice<nint>>(out var Ꮡxs);
    xs = new nint[]{1, 2, 3, 4, 5, 6, 7, 8}.slice();
    var v = ValueOf(xs).Slice3(3, 5, 7).Interface()._<slice<nint>>();
    if (len(v) != 2) {
        Ꮡt.Errorf("len(xs.Slice3(3, 5, 7)) = %d"u8, len(v));
    }
    if (cap(v) != 4) {
        Ꮡt.Errorf("cap(xs.Slice3(3, 5, 7)) = %d"u8, cap(v));
    }
    if (!DeepEqual(v[0..4], xs.slice(3, 7, 7))) {
        Ꮡt.Errorf("xs.Slice3(3, 5, 7)[0:4] = %v"u8, v[0..4]);
    }
    ref var rv = ref heap<reflectꓸValue>(out var Ꮡrv);
    rv = ValueOf(Ꮡxs).Elem();
    shouldPanic(slice3ˢ, () => {
        Ꮡrv.Value.Slice3(1, 2, 1);
    });
    shouldPanic(slice3ˢ, () => {
        Ꮡrv.Value.Slice3(1, 1, 11);
    });
    shouldPanic(slice3ˢ, () => {
        Ꮡrv.Value.Slice3(2, 2, 1);
    });
    ref var xa = ref heap<array<nint>>(out var Ꮡxa);
    xa = new nint[]{10, 20, 30, 40, 50, 60, 70, 80}.array();
    v = ValueOf(Ꮡxa).Elem().Slice3(2, 5, 6).Interface()._<slice<nint>>();
    if (len(v) != 3) {
        Ꮡt.Errorf("len(xa.Slice(2, 5, 6)) = %d"u8, len(v));
    }
    if (cap(v) != 4) {
        Ꮡt.Errorf("cap(xa.Slice(2, 5, 6)) = %d"u8, cap(v));
    }
    if (!DeepEqual(v[0..4], xa.slice(2, 6, 6))) {
        Ꮡt.Errorf("xs.Slice(2, 5, 6)[0:4] = %v"u8, v[0..4]);
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
    ref var s = ref heap<@string>(out var Ꮡs);
    s = helloWorldˢ;
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
        Ꮡt.Errorf("xs.Slice3(3,5,7).Slice3(4,4,4).UnsafePointer() = %p, want %p"u8, @unsafe.Pointer.OrTypedNil(ptr3), @unsafe.Pointer.OrTypedNil(ptr2));
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string setLenˢ = "SetLen"u8;
internal static readonly @string setCapˢ = "SetCap"u8;

public static void TestSetLenCap(ж<Δtesting.T> Ꮡt) {
    ref var xs = ref heap<slice<nint>>(out var Ꮡxs);
    xs = new nint[]{1, 2, 3, 4, 5, 6, 7, 8}.slice();
    ref var xa = ref heap<array<nint>>(out var Ꮡxa);
    xa = new nint[]{10, 20, 30, 40, 50, 60, 70, 80}.array();
    ref var vs = ref heap<reflectꓸValue>(out var Ꮡvs);
    vs = ValueOf(Ꮡxs).Elem();
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
        vsʗ5.SetCap(6); // smaller than len
    });
    vs.SetLen(5);
    if (len(xs) != 5 || cap(xs) != 8) {
        Ꮡt.Errorf("after SetLen(5), len, cap = %d, %d, want 5, 8"u8, len(xs), cap(xs));
    }
    vs.SetCap(6);
    if (len(xs) != 5 || cap(xs) != 6) {
        Ꮡt.Errorf("after SetCap(6), len, cap = %d, %d, want 5, 6"u8, len(xs), cap(xs));
    }
    vs.SetCap(5);
    if (len(xs) != 5 || cap(xs) != 5) {
        Ꮡt.Errorf("after SetCap(5), len, cap = %d, %d, want 5, 5"u8, len(xs), cap(xs));
    }
    var vsʗ6 = vs;
    shouldPanic(setCapˢ, () => {
        vsʗ6.SetCap(4); // smaller than len
    });
    var vsʗ7 = vs;
    shouldPanic(setLenˢ, () => {
        vsʗ7.SetLen(6); // bigger than cap
    });
    ref var va = ref heap<reflectꓸValue>(out var Ꮡva);
    va = ValueOf(Ꮡxa).Elem();
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

public static void TestVariadic(ж<Δtesting.T> Ꮡt) {
    ref var b = ref heap(new strings.Builder(), out var Ꮡb);
    var V = ValueOf;
    b.Reset();
    V(fmt.Fprintfᶠ).Call(new reflectꓸValue[]{V(Ꮡb), V(sDWorldˢ), V(helloˢ), V((nint)(42))}.slice());
    if (b.String() != "hello, 42 world"u8) {
        Ꮡt.Errorf("after Fprintf Call: %q != %q"u8, b.String(), hello42Worldˢ);
    }
    b.Reset();
    V(fmt.Fprintfᶠ).CallSlice(new reflectꓸValue[]{V(Ꮡb), V(sDWorldˢ), V(new any[]{(@string)"hello"u8, (nint)(42)}.slice())}.slice());
    if (b.String() != "hello, 42 world"u8) {
        Ꮡt.Errorf("after Fprintf CallSlice: %q != %q"u8, b.String(), hello42Worldˢ);
    }
}

public static void TestFuncArg(ж<Δtesting.T> Ꮡt) {
    var f1 = (nint i, Func<nint, nint> f) => f(i);
    var f2 = (nint i) => i + 1;
    var r = ValueOf((f1).OrTypedNilFunc()).Call(new reflectꓸValue[]{ValueOf((nint)(100)), ValueOf((f2).OrTypedNilFunc())}.slice());
    if (r[0].Int() != 101) {
        Ꮡt.Errorf("function returned %d, want 101"u8, r[0].Int());
    }
}

[GoType("dyn")] internal partial struct TestStructArg_padded {
    public @string B;
    public int32 C;
}

public static void TestStructArg(ж<Δtesting.T> Ꮡt) {
    ref var gotA = ref heap(new TestStructArg_padded(), out var ᏑgotA);
    uint32 gotB = default!;
    TestStructArg_padded wantA = new TestStructArg_padded("3"u8, 4);
    uint32 wantB = (uint32)5;
    var f = (TestStructArg_padded a, uint32 b) => {
        (ᏑgotA.Value, gotB) = (a, b);
    };
    ValueOf((f).OrTypedNilFunc()).Call(new reflectꓸValue[]{ValueOf(wantA), ValueOf(wantB)}.slice());
    if (gotA != wantA || gotB != wantB) {
        Ꮡt.Errorf("function called with (%v, %v), want (%v, %v)"u8, gotA, gotB, wantA, wantB);
    }
}


[GoType("dyn")] partial struct tagGetTestsᴛ1 {
    public Δreflect.StructTag Tag;
    public @string Key;
    public @string Value;
}
internal static slice<tagGetTestsᴛ1> tagGetTests = new tagGetTestsᴛ1[]{
    new(@"protobuf:""PB(1,2)"""u8, @"protobuf"u8, @"PB(1,2)"u8),
    new(@"protobuf:""PB(1,2)"""u8, @"foo"u8, @""u8),
    new(@"protobuf:""PB(1,2)"""u8, @"rotobuf"u8, @""u8),
    new(@"protobuf:""PB(1,2)"" json:""name"""u8, @"json"u8, @"name"u8),
    new(@"protobuf:""PB(1,2)"" json:""name"""u8, @"protobuf"u8, @"PB(1,2)"u8),
    new(@"k0:""values contain spaces"" k1:""and\ttabs"""u8, "k0"u8, "values contain spaces"u8),
    new(@"k0:""values contain spaces"" k1:""and\ttabs"""u8, "k1"u8, "and\ttabs"u8)
}.slice();

public static void TestTagGet(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, tt) in tagGetTests) {
        {
            @string v = tt.Tag.Get(tt.Key); if (v != tt.Value) {
                Ꮡt.Errorf("StructTag(%#q).Get(%#q) = %#q, want %#q"u8, tt.Tag, tt.Key, v, tt.Value);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string onIntValueˢ = "on int Value"u8;
internal static readonly @string ofNonByteSliceˢ = "of non-byte slice"u8;
internal static readonly @string unaddressableˢ = "unaddressable"u8;
internal static readonly @string onPtrValueˢ = "on ptr Value"u8;

[GoLocalName("S")] [GoType("[]byte")] internal partial struct TestBytes_S;

[GoLocalName("A")] [GoType("[4]byte")] internal partial struct TestBytes_A;

[GoLocalName("B")] [GoType("num:byte")] internal partial struct TestBytes_B;

[GoLocalName("SB")] [GoType("[]TestBytes_B")] internal partial struct TestBytes_SB;

[GoLocalName("AB")] [GoType("[4]TestBytes_B")] internal partial struct TestBytes_AB;

public static void TestBytes(ж<Δtesting.T> Ꮡt) {
    shouldPanic(onIntValueˢ, () => {
        ValueOf((nint)(0)).Bytes();
    });
    shouldPanic(ofNonByteSliceˢ, () => {
        ValueOf(new @string[]{}.slice()).Bytes();
    });
    var x = new TestBytes_S(new byte[]{1, 2, 3, 4}.slice());
    var y = ValueOf(x).Bytes();
    if (!bytes.Equal(x, y)) {
        Ꮡt.Fatalf("ValueOf(%v).Bytes() = %v"u8, x, y);
    }
    if (Ꮡ(x, 0) != Ꮡ(y, 0)) {
        Ꮡt.Errorf("ValueOf(%p).Bytes() = %p"u8, Ꮡ(x, 0), Ꮡ(y, 0));
    }
    ref var a = ref heap<TestBytes_A>(out var Ꮡa);
    a = new TestBytes_A(new byte[]{1, 2, 3, 4}.array());
    shouldPanic(unaddressableˢ, () => {
        ValueOf(Ꮡa.Value).Bytes();
    });
    shouldPanic(onPtrValueˢ, () => {
        ValueOf(Ꮡa).Bytes();
    });
    var b = ValueOf(Ꮡa).Elem().Bytes();
    if (!bytes.Equal(a[..], y)) {
        Ꮡt.Fatalf("ValueOf(%v).Bytes() = %v"u8, a, b);
    }
    if (Ꮡa.at<byte>(0) != Ꮡ(b, 0)) {
        Ꮡt.Errorf("ValueOf(%p).Bytes() = %p"u8, Ꮡa.at<byte>(0), Ꮡ(b, 0));
    }
    ValueOf(new TestBytes_B[]{1, 2, 3, 4}.slice()).Bytes(); // should not panic
    ValueOf(Ꮡ(new array<TestBytes_B>(4))).Elem().Bytes(); // should not panic
    ValueOf(new TestBytes_SB(new TestBytes_B[]{1, 2, 3, 4}.slice())).Bytes(); // should not panic
    ValueOf(@new<TestBytes_AB>()).Elem().Bytes(); // should not panic
}

[GoLocalName("B")] [GoType("[]byte")] internal partial struct TestSetBytes_B;

public static void TestSetBytes(ж<Δtesting.T> Ꮡt) {
    ref var x = ref heap<TestSetBytes_B>(out var Ꮡx);
    var y = new byte[]{1, 2, 3, 4}.slice();
    ValueOf(Ꮡx).Elem().SetBytes(y);
    if (!bytes.Equal(x, y)) {
        Ꮡt.Fatalf("ValueOf(%v).Bytes() = %v"u8, x, y);
    }
    if (Ꮡ(x, 0) != Ꮡ(y, 0)) {
        Ꮡt.Errorf("ValueOf(%p).Bytes() = %p"u8, Ꮡ(x, 0), Ꮡ(y, 0));
    }
}

[GoType] partial struct Private {
    internal nint x;
    internal ж<ж<nint>> y;
    public nint Z;
}

[GoRecv] internal static void m(this ref Private p) {
}

[GoType] partial struct @private {
    public nint Z;
    internal nint z;
    public @string S;
    public array<Private> A = new(1);
    public slice<Private> T;
}

[GoRecv] internal static void P(this ref @private p) {
}

[GoType] partial struct Public {
    public nint X;
    public ж<ж<nint>> Y;
    internal partial ref @private @private { get; }
}

[GoRecv] public static void M(this ref Public p) {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string interfaceˢ2 = "Interface"u8;

public static void TestUnexported(ж<Δtesting.T> Ꮡt) {
    ref var pub = ref heap(new Public(), out var Ꮡpub);
    pub.S = "S"u8;
    pub.T = pub.A[..];
    ref var v = ref heap<reflectꓸValue>(out var Ꮡv);
    v = ValueOf(Ꮡpub);
    isValid(v.Elem().Field(0));
    isValid(v.Elem().Field(1));
    isValid(v.Elem().Field(2));
    isValid(v.Elem().FieldByName("X"u8));
    isValid(v.Elem().FieldByName("Y"u8));
    isValid(v.Elem().FieldByName("Z"u8));
    isValid(v.Type().Method(0).Func);
    var (m, _) = v.Type().MethodByName("M"u8);
    isValid(m.Func);
    (m, _) = v.Type().MethodByName("P"u8);
    isValid(m.Func);
    isNonNil(v.Elem().Field(0).Interface());
    isNonNil(v.Elem().Field(1).Interface());
    isNonNil(v.Elem().Field(2).Field(2).Index(0));
    isNonNil(v.Elem().FieldByName("X"u8).Interface());
    isNonNil(v.Elem().FieldByName("Y"u8).Interface());
    isNonNil(v.Elem().FieldByName("Z"u8).Interface());
    isNonNil(v.Elem().FieldByName("S"u8).Index(0).Interface());
    isNonNil(v.Type().Method(0).Func.Interface());
    (m, _) = v.Type().MethodByName("P"u8);
    isNonNil(m.Func.Interface());
    ref var priv = ref heap(new Private(), out var Ꮡpriv);
    v = ValueOf(Ꮡpriv);
    isValid(v.Elem().Field(0));
    isValid(v.Elem().Field(1));
    isValid(v.Elem().FieldByName("x"u8));
    isValid(v.Elem().FieldByName("y"u8));
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
        vʗ3.Elem().FieldByName("x"u8).Interface();
    });
    var vʗ4 = v;
    shouldPanic(interfaceˢ2, () => {
        vʗ4.Elem().FieldByName("y"u8).Interface();
    });
    var vʗ5 = v;
    shouldPanic(methodˢ, () => {
        vʗ5.Type().Method(0);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string setˢ = "Set"u8;

[GoType("dyn")] internal partial struct TestSetPanic_t0 {
    public nint W;
}

[GoType("dyn")] internal partial struct TestSetPanic_t1 {
    public nint Y;
    internal partial ref TestSetPanic_t0 t0 { get; }
}

[GoType("dyn")] internal partial struct TestSetPanic_T2 {
    public nint Z;
    internal TestSetPanic_t0 namedT0;
}

[GoType("dyn")] internal partial struct TestSetPanic_T {
    public nint X;
    internal partial ref TestSetPanic_t1 t1 { get; }
    public partial ref TestSetPanic_T2 T2 { get; }
    public TestSetPanic_t1 NamedT1;
    public TestSetPanic_T2 NamedT2;
    internal TestSetPanic_t1 namedT1;
    internal TestSetPanic_T2 namedT2;
}

public static void TestSetPanic(ж<Δtesting.T> Ꮡt) {
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
    ref var v = ref heap<reflectꓸValue>(out var Ꮡv);
    v = ValueOf(new TestSetPanic_T(nil));
    var clearʗ1 = clear;
    bad(() => {
        clearʗ1(Ꮡv.Value.Field(0)); // .X
    });
    var clearʗ2 = clear;
    bad(() => {
        clearʗ2(Ꮡv.Value.Field(1)); // .t1
    });
    var clearʗ3 = clear;
    bad(() => {
        clearʗ3(Ꮡv.Value.Field(1).Field(0)); // .t1.Y
    });
    var clearʗ4 = clear;
    bad(() => {
        clearʗ4(Ꮡv.Value.Field(1).Field(1)); // .t1.t0
    });
    var clearʗ5 = clear;
    bad(() => {
        clearʗ5(Ꮡv.Value.Field(1).Field(1).Field(0)); // .t1.t0.W
    });
    var clearʗ6 = clear;
    bad(() => {
        clearʗ6(Ꮡv.Value.Field(2)); // .T2
    });
    var clearʗ7 = clear;
    bad(() => {
        clearʗ7(Ꮡv.Value.Field(2).Field(0)); // .T2.Z
    });
    var clearʗ8 = clear;
    bad(() => {
        clearʗ8(Ꮡv.Value.Field(2).Field(1)); // .T2.namedT0
    });
    var clearʗ9 = clear;
    bad(() => {
        clearʗ9(Ꮡv.Value.Field(2).Field(1).Field(0)); // .T2.namedT0.W
    });
    var clearʗ10 = clear;
    bad(() => {
        clearʗ10(Ꮡv.Value.Field(3)); // .NamedT1
    });
    var clearʗ11 = clear;
    bad(() => {
        clearʗ11(Ꮡv.Value.Field(3).Field(0)); // .NamedT1.Y
    });
    var clearʗ12 = clear;
    bad(() => {
        clearʗ12(Ꮡv.Value.Field(3).Field(1)); // .NamedT1.t0
    });
    var clearʗ13 = clear;
    bad(() => {
        clearʗ13(Ꮡv.Value.Field(3).Field(1).Field(0)); // .NamedT1.t0.W
    });
    var clearʗ14 = clear;
    bad(() => {
        clearʗ14(Ꮡv.Value.Field(4)); // .NamedT2
    });
    var clearʗ15 = clear;
    bad(() => {
        clearʗ15(Ꮡv.Value.Field(4).Field(0)); // .NamedT2.Z
    });
    var clearʗ16 = clear;
    bad(() => {
        clearʗ16(Ꮡv.Value.Field(4).Field(1)); // .NamedT2.namedT0
    });
    var clearʗ17 = clear;
    bad(() => {
        clearʗ17(Ꮡv.Value.Field(4).Field(1).Field(0)); // .NamedT2.namedT0.W
    });
    var clearʗ18 = clear;
    bad(() => {
        clearʗ18(Ꮡv.Value.Field(5)); // .namedT1
    });
    var clearʗ19 = clear;
    bad(() => {
        clearʗ19(Ꮡv.Value.Field(5).Field(0)); // .namedT1.Y
    });
    var clearʗ20 = clear;
    bad(() => {
        clearʗ20(Ꮡv.Value.Field(5).Field(1)); // .namedT1.t0
    });
    var clearʗ21 = clear;
    bad(() => {
        clearʗ21(Ꮡv.Value.Field(5).Field(1).Field(0)); // .namedT1.t0.W
    });
    var clearʗ22 = clear;
    bad(() => {
        clearʗ22(Ꮡv.Value.Field(6)); // .namedT2
    });
    var clearʗ23 = clear;
    bad(() => {
        clearʗ23(Ꮡv.Value.Field(6).Field(0)); // .namedT2.Z
    });
    var clearʗ24 = clear;
    bad(() => {
        clearʗ24(Ꮡv.Value.Field(6).Field(1)); // .namedT2.namedT0
    });
    var clearʗ25 = clear;
    bad(() => {
        clearʗ25(Ꮡv.Value.Field(6).Field(1).Field(0)); // .namedT2.namedT0.W
    });
    // addressable
    v = ValueOf(Ꮡ(new TestSetPanic_T(nil))).Elem();
    var clearʗ26 = clear;
    ok(() => {
        clearʗ26(Ꮡv.Value.Field(0)); // .X
    });
    var clearʗ27 = clear;
    bad(() => {
        clearʗ27(Ꮡv.Value.Field(1)); // .t1
    });
    var clearʗ28 = clear;
    ok(() => {
        clearʗ28(Ꮡv.Value.Field(1).Field(0)); // .t1.Y
    });
    var clearʗ29 = clear;
    bad(() => {
        clearʗ29(Ꮡv.Value.Field(1).Field(1)); // .t1.t0
    });
    var clearʗ30 = clear;
    ok(() => {
        clearʗ30(Ꮡv.Value.Field(1).Field(1).Field(0)); // .t1.t0.W
    });
    var clearʗ31 = clear;
    ok(() => {
        clearʗ31(Ꮡv.Value.Field(2)); // .T2
    });
    var clearʗ32 = clear;
    ok(() => {
        clearʗ32(Ꮡv.Value.Field(2).Field(0)); // .T2.Z
    });
    var clearʗ33 = clear;
    bad(() => {
        clearʗ33(Ꮡv.Value.Field(2).Field(1)); // .T2.namedT0
    });
    var clearʗ34 = clear;
    bad(() => {
        clearʗ34(Ꮡv.Value.Field(2).Field(1).Field(0)); // .T2.namedT0.W
    });
    var clearʗ35 = clear;
    ok(() => {
        clearʗ35(Ꮡv.Value.Field(3)); // .NamedT1
    });
    var clearʗ36 = clear;
    ok(() => {
        clearʗ36(Ꮡv.Value.Field(3).Field(0)); // .NamedT1.Y
    });
    var clearʗ37 = clear;
    bad(() => {
        clearʗ37(Ꮡv.Value.Field(3).Field(1)); // .NamedT1.t0
    });
    var clearʗ38 = clear;
    ok(() => {
        clearʗ38(Ꮡv.Value.Field(3).Field(1).Field(0)); // .NamedT1.t0.W
    });
    var clearʗ39 = clear;
    ok(() => {
        clearʗ39(Ꮡv.Value.Field(4)); // .NamedT2
    });
    var clearʗ40 = clear;
    ok(() => {
        clearʗ40(Ꮡv.Value.Field(4).Field(0)); // .NamedT2.Z
    });
    var clearʗ41 = clear;
    bad(() => {
        clearʗ41(Ꮡv.Value.Field(4).Field(1)); // .NamedT2.namedT0
    });
    var clearʗ42 = clear;
    bad(() => {
        clearʗ42(Ꮡv.Value.Field(4).Field(1).Field(0)); // .NamedT2.namedT0.W
    });
    var clearʗ43 = clear;
    bad(() => {
        clearʗ43(Ꮡv.Value.Field(5)); // .namedT1
    });
    var clearʗ44 = clear;
    bad(() => {
        clearʗ44(Ꮡv.Value.Field(5).Field(0)); // .namedT1.Y
    });
    var clearʗ45 = clear;
    bad(() => {
        clearʗ45(Ꮡv.Value.Field(5).Field(1)); // .namedT1.t0
    });
    var clearʗ46 = clear;
    bad(() => {
        clearʗ46(Ꮡv.Value.Field(5).Field(1).Field(0)); // .namedT1.t0.W
    });
    var clearʗ47 = clear;
    bad(() => {
        clearʗ47(Ꮡv.Value.Field(6)); // .namedT2
    });
    var clearʗ48 = clear;
    bad(() => {
        clearʗ48(Ꮡv.Value.Field(6).Field(0)); // .namedT2.Z
    });
    var clearʗ49 = clear;
    bad(() => {
        clearʗ49(Ꮡv.Value.Field(6).Field(1)); // .namedT2.namedT0
    });
    var clearʗ50 = clear;
    bad(() => {
        clearʗ50(Ꮡv.Value.Field(6).Field(1).Field(0)); // .namedT2.namedT0.W
    });
}

[GoType("num:nint")] partial struct timp;

internal static void W(this timp t) {
}

internal static void Y(this timp t) {
}

internal static void w(this timp t) {
}

internal static void y(this timp t) {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string callˢ = "Call"u8;

[GoType("dyn")] internal partial interface TestCallPanic_t0 {
    void W();
    void w();
}

[GoType("dyn")] internal partial interface TestCallPanic_T1 {
    void Y();
    void y();
}

[GoType("dyn")] internal partial struct TestCallPanic_T2 {
    [GoEmbedded] public TestCallPanic_T1 T1;
    [GoEmbedded] internal TestCallPanic_t0 t0;
}

[GoType("dyn")] internal partial struct TestCallPanic_T {
    [GoEmbedded] internal TestCallPanic_t0 t0; // 0
    [GoEmbedded] public TestCallPanic_T1 T1; // 1
    public TestCallPanic_t0 NamedT0; // 2
    public TestCallPanic_T1 NamedT1; // 3
    public TestCallPanic_T2 NamedT2; // 4
    internal TestCallPanic_t0 namedT0; // 5
    internal TestCallPanic_T1 namedT1; // 6
    internal TestCallPanic_T2 namedT2; // 7
}

public static void TestCallPanic(ж<Δtesting.T> Ꮡt) {
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
        vΔ1.Call(default!);
    }
    timp i = ((timp)0);
    ref var v = ref heap<reflectꓸValue>(out var Ꮡv);
    v = ValueOf(new TestCallPanic_T(i, i, i, i, new TestCallPanic_T2(i, i), i, i, new TestCallPanic_T2(i, i)));
    var callʗ1 = call;
    var vʗ1 = v;
    badCall(() => {
        callʗ1(vʗ1.Field(0).Method(0)); // .t0.W
    });
    var callʗ2 = call;
    var vʗ2 = v;
    badCall(() => {
        callʗ2(vʗ2.Field(0).Elem().Method(0)); // .t0.W
    });
    var callʗ3 = call;
    var vʗ3 = v;
    badCall(() => {
        callʗ3(vʗ3.Field(0).Method(1)); // .t0.w
    });
    var callʗ4 = call;
    var vʗ4 = v;
    badMethod(() => {
        callʗ4(vʗ4.Field(0).Elem().Method(2)); // .t0.w
    });
    var callʗ5 = call;
    var vʗ5 = v;
    ok(() => {
        callʗ5(vʗ5.Field(1).Method(0)); // .T1.Y
    });
    var callʗ6 = call;
    var vʗ6 = v;
    ok(() => {
        callʗ6(vʗ6.Field(1).Elem().Method(0)); // .T1.Y
    });
    var callʗ7 = call;
    var vʗ7 = v;
    badCall(() => {
        callʗ7(vʗ7.Field(1).Method(1)); // .T1.y
    });
    var callʗ8 = call;
    var vʗ8 = v;
    badMethod(() => {
        callʗ8(vʗ8.Field(1).Elem().Method(2)); // .T1.y
    });
    var callʗ9 = call;
    var vʗ9 = v;
    ok(() => {
        callʗ9(vʗ9.Field(2).Method(0)); // .NamedT0.W
    });
    var callʗ10 = call;
    var vʗ10 = v;
    ok(() => {
        callʗ10(vʗ10.Field(2).Elem().Method(0)); // .NamedT0.W
    });
    var callʗ11 = call;
    var vʗ11 = v;
    badCall(() => {
        callʗ11(vʗ11.Field(2).Method(1)); // .NamedT0.w
    });
    var callʗ12 = call;
    var vʗ12 = v;
    badMethod(() => {
        callʗ12(vʗ12.Field(2).Elem().Method(2)); // .NamedT0.w
    });
    var callʗ13 = call;
    var vʗ13 = v;
    ok(() => {
        callʗ13(vʗ13.Field(3).Method(0)); // .NamedT1.Y
    });
    var callʗ14 = call;
    var vʗ14 = v;
    ok(() => {
        callʗ14(vʗ14.Field(3).Elem().Method(0)); // .NamedT1.Y
    });
    var callʗ15 = call;
    var vʗ15 = v;
    badCall(() => {
        callʗ15(vʗ15.Field(3).Method(1)); // .NamedT1.y
    });
    var callʗ16 = call;
    var vʗ16 = v;
    badMethod(() => {
        callʗ16(vʗ16.Field(3).Elem().Method(3)); // .NamedT1.y
    });
    var callʗ17 = call;
    var vʗ17 = v;
    ok(() => {
        callʗ17(vʗ17.Field(4).Field(0).Method(0)); // .NamedT2.T1.Y
    });
    var callʗ18 = call;
    var vʗ18 = v;
    ok(() => {
        callʗ18(vʗ18.Field(4).Field(0).Elem().Method(0)); // .NamedT2.T1.W
    });
    var callʗ19 = call;
    var vʗ19 = v;
    badCall(() => {
        callʗ19(vʗ19.Field(4).Field(1).Method(0)); // .NamedT2.t0.W
    });
    var callʗ20 = call;
    var vʗ20 = v;
    badCall(() => {
        callʗ20(vʗ20.Field(4).Field(1).Elem().Method(0)); // .NamedT2.t0.W
    });
    var callʗ21 = call;
    var vʗ21 = v;
    badCall(() => {
        callʗ21(vʗ21.Field(5).Method(0)); // .namedT0.W
    });
    var callʗ22 = call;
    var vʗ22 = v;
    badCall(() => {
        callʗ22(vʗ22.Field(5).Elem().Method(0)); // .namedT0.W
    });
    var callʗ23 = call;
    var vʗ23 = v;
    badCall(() => {
        callʗ23(vʗ23.Field(5).Method(1)); // .namedT0.w
    });
    var callʗ24 = call;
    var vʗ24 = v;
    badMethod(() => {
        callʗ24(vʗ24.Field(5).Elem().Method(2)); // .namedT0.w
    });
    var callʗ25 = call;
    var vʗ25 = v;
    badCall(() => {
        callʗ25(vʗ25.Field(6).Method(0)); // .namedT1.Y
    });
    var callʗ26 = call;
    var vʗ26 = v;
    badCall(() => {
        callʗ26(vʗ26.Field(6).Elem().Method(0)); // .namedT1.Y
    });
    var callʗ27 = call;
    var vʗ27 = v;
    badCall(() => {
        callʗ27(vʗ27.Field(6).Method(0)); // .namedT1.y
    });
    var callʗ28 = call;
    var vʗ28 = v;
    badCall(() => {
        callʗ28(vʗ28.Field(6).Elem().Method(0)); // .namedT1.y
    });
    var callʗ29 = call;
    var vʗ29 = v;
    badCall(() => {
        callʗ29(vʗ29.Field(7).Field(0).Method(0)); // .namedT2.T1.Y
    });
    var callʗ30 = call;
    var vʗ30 = v;
    badCall(() => {
        callʗ30(vʗ30.Field(7).Field(0).Elem().Method(0)); // .namedT2.T1.W
    });
    var callʗ31 = call;
    var vʗ31 = v;
    badCall(() => {
        callʗ31(vʗ31.Field(7).Field(1).Method(0)); // .namedT2.t0.W
    });
    var callʗ32 = call;
    var vʗ32 = v;
    badCall(() => {
        callʗ32(vʗ32.Field(7).Field(1).Elem().Method(0)); // .namedT2.t0.W
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string reflectValueAddrOfˢ = "reflect.Value.Addr of unaddressable value"u8;
internal static readonly @string callOfReflectValueBoolOnˢ = "call of reflect.Value.Bool on float64 Value"u8;
internal static readonly @string callOfReflectValueBytesˢ = "call of reflect.Value.Bytes on string Value"u8;
internal static readonly @string callOfReflectValueCallOnˢ = "call of reflect.Value.Call on bool Value"u8;
internal static readonly @string callOfReflectValueˢ = "call of reflect.Value.CallSlice on int Value"u8;
internal static readonly @string callOfReflectValueCloseˢ = "call of reflect.Value.Close on string Value"u8;
internal static readonly @string callOfReflectValueˢ2 = "call of reflect.Value.Complex on float64 Value"u8;
internal static readonly @string callOfReflectValueElemOnˢ = "call of reflect.Value.Elem on bool Value"u8;
internal static readonly @string callOfReflectValueFieldˢ = "call of reflect.Value.Field on int Value"u8;
internal static readonly @string callOfReflectValueFloatˢ = "call of reflect.Value.Float on string Value"u8;
internal static readonly @string callOfReflectValueIndexˢ = "call of reflect.Value.Index on float64 Value"u8;
internal static readonly @string callOfReflectValueIntOnˢ = "call of reflect.Value.Int on bool Value"u8;
internal static readonly @string callOfReflectValueIsNilˢ = "call of reflect.Value.IsNil on int Value"u8;
internal static readonly @string callOfReflectValueLenOnˢ = "call of reflect.Value.Len on bool Value"u8;
internal static readonly @string callOfReflectValueˢ3 = "call of reflect.Value.MapIndex on float64 Value"u8;
internal static readonly @string callOfReflectValueˢ4 = "call of reflect.Value.MapKeys on string Value"u8;
internal static readonly @string callOfReflectValueˢ5 = "call of reflect.Value.MapRange on int Value"u8;
internal static readonly @string callOfReflectValueMethodˢ = "call of reflect.Value.Method on zero Value"u8;
internal static readonly @string callOfReflectValueˢ6 = "call of reflect.Value.NumField on string Value"u8;
internal static readonly @string callOfReflectValueˢ7 = "call of reflect.Value.NumMethod on zero Value"u8;
internal static readonly @string callOfReflectValueˢ8 = "call of reflect.Value.OverflowComplex on float64 Value"u8;
internal static readonly @string callOfReflectValueˢ9 = "call of reflect.Value.OverflowFloat on int64 Value"u8;
internal static readonly @string callOfReflectValueˢ10 = "call of reflect.Value.OverflowInt on uint64 Value"u8;
internal static readonly @string callOfReflectValueˢ11 = "call of reflect.Value.OverflowUint on complex64 Value"u8;
internal static readonly @string callOfReflectValueRecvOnˢ = "call of reflect.Value.Recv on string Value"u8;
internal static readonly @string callOfReflectValueSendOnˢ = "call of reflect.Value.Send on bool Value"u8;
internal static readonly @string valueOfTypeStringIsNotˢ = "value of type string is not assignable to type bool"u8;
internal static readonly @string callOfReflectValueˢ12 = "call of reflect.Value.SetBool on string Value"u8;
internal static readonly @string reflectValueSetBytesˢ = "reflect.Value.SetBytes using unaddressable value"u8;
internal static readonly @string callOfReflectValueSetCapˢ = "call of reflect.Value.SetCap on string Value"u8;
internal static readonly @string callOfReflectValueˢ13 = "call of reflect.Value.SetComplex on string Value"u8;
internal static readonly @string callOfReflectValueˢ14 = "call of reflect.Value.SetFloat on string Value"u8;
internal static readonly @string callOfReflectValueSetIntˢ = "call of reflect.Value.SetInt on string Value"u8;
internal static readonly @string callOfReflectValueSetLenˢ = "call of reflect.Value.SetLen on string Value"u8;
internal static readonly @string callOfReflectValueˢ15 = "call of reflect.Value.SetString on int Value"u8;
internal static readonly @string reflectValueSetUintUsingˢ = "reflect.Value.SetUint using unaddressable value"u8;
internal static readonly @string callOfReflectValueSliceˢ = "call of reflect.Value.Slice on bool Value"u8;
internal static readonly @string callOfReflectValueSlice3ˢ = "call of reflect.Value.Slice3 on int Value"u8;
internal static readonly @string callOfReflectValueˢ16 = "call of reflect.Value.TryRecv on bool Value"u8;
internal static readonly @string callOfReflectValueˢ17 = "call of reflect.Value.TrySend on string Value"u8;
internal static readonly @string callOfReflectValueUintOnˢ = "call of reflect.Value.Uint on float64 Value"u8;

public static void TestValuePanic(ж<Δtesting.T> Ꮡt) {
    var vo = ValueOf;
    var voʗ1 = vo;
    shouldPanic(reflectValueAddrOfˢ, () => {
        voʗ1((nint)(0)).Addr();
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
        voʗ4(true).Call(default!);
    });
    var voʗ5 = vo;
    shouldPanic(callOfReflectValueˢ, () => {
        voʗ5((nint)(0)).CallSlice(default!);
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
        voʗ9((nint)(0)).Field(0);
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
        voʗ13((nint)(0)).IsNil();
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
        voʗ17((nint)(0)).MapRange();
    });
    var voʗ18 = vo;
    shouldPanic(callOfReflectValueMethodˢ, () => {
        voʗ18(default!).Method(0);
    });
    var voʗ19 = vo;
    shouldPanic(callOfReflectValueˢ6, () => {
        voʗ19((@string)""u8).NumField();
    });
    var voʗ20 = vo;
    shouldPanic(callOfReflectValueˢ7, () => {
        voʗ20(default!).NumMethod();
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
        voʗ29((@string)""u8).SetBytes(default!);
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
        voʗ38((nint)(0)).Slice3(1, 2, 3);
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
internal static readonly @string reflectˢ = "reflect"u8;

internal static void shouldPanic(@string expect, Action f) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            var r = recover();
            if (r == default!) {
                throw panic("did not panic");
            }
            if (expect != ""u8) {
                @string s = default!;
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
    if (x == default!) {
        throw panic("nil interface");
    }
}

internal static void isValid(reflectꓸValue v) {
    if (!v.IsValid()) {
        throw panic("zero Value");
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string worldˢ = "world"u8;

public static void TestAlias(ж<Δtesting.T> Ꮡt) {
    ref var x = ref heap<@string>(out var Ꮡx);
    x = ((@string)"hello"u8);
    var v = ValueOf(Ꮡx).Elem();
    var oldvalue = v.Interface();
    v.SetString(worldˢ);
    var newvalue = v.Interface();
    if (!AreEqual(oldvalue, (@string)("hello")) || !AreEqual(newvalue, (@string)("world"))) {
        Ꮡt.Errorf("aliasing: old=%q new=%q, want hello, world"u8, oldvalue, newvalue);
    }
}

public static Func<any, reflectꓸValue> V = ValueOf;

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

[GoType] partial struct Empty {
}

[GoType] partial struct MyStruct {
    [GoTag(@"some:""tag""")]
    internal nint x;
}

[GoType("dyn")] partial struct MyStruct1_x {
    [GoTag(@"some:""bar""")]
    [GoEmbedded] internal nint @int;
}

[GoType] partial struct MyStruct1 {
    internal MyStruct1_x x;
}

[GoType("dyn")] partial struct MyStruct2_x {
    [GoTag(@"some:""foo""")]
    [GoEmbedded] internal nint @int;
}

[GoType] partial struct MyStruct2 {
    internal MyStruct2_x x;
}

[GoType("@string")] partial struct MyString;

[GoType("[]byte")] partial struct MyBytes;

[GoType("ж<array<byte>>")] [GoArrayDims(0)] partial class MyBytesArrayPtr0;

[GoType("ж<array<byte>>")] [GoArrayDims(4)] partial class MyBytesArrayPtr;

[GoType("[0]byte")] partial struct MyBytesArray0;

[GoType("[4]byte")] partial struct MyBytesArray;

[GoType("[]int32")] partial struct MyRunes;

// type MyFunc is a methodless func type — rendered inline as its base delegate

[GoType("num:byte")] partial struct MyByte;

[GoType("chan nint")] partial struct IntChan;

[GoType("chan nint")] [GoChanDir(GoChanDir.Recv)] partial struct IntChanRecv;

[GoType("chan nint")] [GoChanDir(GoChanDir.Send)] partial struct IntChanSend;

[GoType("chan slice<byte>")] partial struct BytesChan;

[GoType("chan slice<byte>")] [GoChanDir(GoChanDir.Recv)] partial struct BytesChanRecv;

[GoType("chan slice<byte>")] [GoChanDir(GoChanDir.Send)] partial struct BytesChanSend;

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
// truncation
// complex
// string
// named string
// named []byte
// named []rune
// slice to array
// slice to array pointer
// named types and equal underlying types
// structs with different tags
// can convert *byte and *MyByte
// cannot convert mismatched array sizes
// cannot convert other instances
// other
// channels
// cannot convert other instances (channels)
// interfaces

[GoType("dyn")] partial struct convertTestsᴛ1 {
    internal reflectꓸValue @in;
    internal reflectꓸValue @out;
}

        [GoType("dyn")] partial struct iᴛ1 {
            [GoTag(@"some:""foo""")]
            internal nint x;
        }

        [GoType("dyn")] partial struct iᴛ2 {
            [GoTag(@"some:""bar""")]
            internal nint x;
        }
internal static slice<convertTestsᴛ1> convertTests = new convertTestsᴛ1[]{
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
    new(V((float64)1.5D), V((nint)1)),
    new(V((complex64)1F.i()), V((complex64)1F.i())),
    new(V((complex64)2F.i()), V((complex128)2D.i())),
    new(V((complex128)3D.i()), V((complex64)3F.i())),
    new(V((complex128)4D.i()), V((complex128)4D.i())),
    new(V(((@string)"hello"u8)), V(((@string)"hello"u8))),
    new(V(((@string)"bytes1"u8)), V(slice<byte>("bytes1"u8))),
    new(V(slice<byte>("bytes2"u8)), V(((@string)"bytes2"u8))),
    new(V(slice<byte>("bytes3"u8)), V(slice<byte>("bytes3"u8))),
    new(V(((@string)"runes♝"u8)), V(slice<rune>((@string)"runes♝"))),
    new(V(slice<rune>((@string)"runes♕")), V(((@string)"runes♕"u8))),
    new(V(slice<rune>((@string)"runes🙈🙉🙊")), V(slice<rune>((@string)"runes🙈🙉🙊"))),
    new(V((nint)(rune)'a'), V(((@string)"a"u8))),
    new(V((int8)(rune)'a'), V(((@string)"a"u8))),
    new(V((int16)(rune)'a'), V(((@string)"a"u8))),
    new(V((int32)(rune)'a'), V(((@string)"a"u8))),
    new(V((int64)(rune)'a'), V(((@string)"a"u8))),
    new(V((nuint)(rune)'a'), V(((@string)"a"u8))),
    new(V((uint8)(rune)'a'), V(((@string)"a"u8))),
    new(V((uint16)(rune)'a'), V(((@string)"a"u8))),
    new(V((uint32)(rune)'a'), V(((@string)"a"u8))),
    new(V((uint64)(rune)'a'), V(((@string)"a"u8))),
    new(V((uintptr)(rune)'a'), V(((@string)"a"u8))),
    new(V((nint)(-1)), V(((@string)"\uFFFD"u8))),
    new(V((int8)(-2)), V(((@string)"\uFFFD"u8))),
    new(V((int16)(-3)), V(((@string)"\uFFFD"u8))),
    new(V((int32)(-4)), V(((@string)"\uFFFD"u8))),
    new(V((int64)(-5)), V(((@string)"\uFFFD"u8))),
    new(V((int64)(-4294967296L)), V(((@string)"\uFFFD"u8))),
    new(V((int64)(4294967296L)), V(((@string)"\uFFFD"u8))),
    new(V((nuint)0x110001), V(((@string)"\uFFFD"u8))),
    new(V((uint32)0x110002), V(((@string)"\uFFFD"u8))),
    new(V((uint64)0x110003), V(((@string)"\uFFFD"u8))),
    new(V((uint64)(((uint64)1 << (int)(32)))), V(((@string)"\uFFFD"u8))),
    new(V((uintptr)0x110004), V(((@string)"\uFFFD"u8))),
    new(V(((MyString)(@string)"hello"u8)), V(((@string)"hello"u8))),
    new(V(((@string)"hello"u8)), V(((MyString)(@string)"hello"u8))),
    new(V(((@string)"hello"u8)), V(((@string)"hello"u8))),
    new(V(((MyString)(@string)"hello"u8)), V(((MyString)(@string)"hello"u8))),
    new(V(((MyString)(@string)"bytes1"u8)), V(slice<byte>("bytes1"u8))),
    new(V(slice<byte>("bytes2"u8)), V(((MyString)(@string)"bytes2"u8))),
    new(V(slice<byte>("bytes3"u8)), V(slice<byte>("bytes3"u8))),
    new(V(((MyString)(@string)"runes♝"u8)), V(slice<rune>((@string)"runes♝"))),
    new(V(slice<rune>((@string)"runes♕")), V(((MyString)(@string)"runes♕"u8))),
    new(V(slice<rune>((@string)"runes🙈🙉🙊")), V(slice<rune>((@string)"runes🙈🙉🙊"))),
    new(V(slice<rune>((@string)"runes🙈🙉🙊")), V(((MyRunes)slice<int32>((@string)"runes🙈🙉🙊"u8)))),
    new(V(((MyRunes)slice<int32>((@string)"runes🙈🙉🙊"u8))), V(slice<rune>((@string)"runes🙈🙉🙊"))),
    new(V((nint)(rune)'a'), V(((MyString)(@string)"a"u8))),
    new(V((int8)(rune)'a'), V(((MyString)(@string)"a"u8))),
    new(V((int16)(rune)'a'), V(((MyString)(@string)"a"u8))),
    new(V((int32)(rune)'a'), V(((MyString)(@string)"a"u8))),
    new(V((int64)(rune)'a'), V(((MyString)(@string)"a"u8))),
    new(V((nuint)(rune)'a'), V(((MyString)(@string)"a"u8))),
    new(V((uint8)(rune)'a'), V(((MyString)(@string)"a"u8))),
    new(V((uint16)(rune)'a'), V(((MyString)(@string)"a"u8))),
    new(V((uint32)(rune)'a'), V(((MyString)(@string)"a"u8))),
    new(V((uint64)(rune)'a'), V(((MyString)(@string)"a"u8))),
    new(V((uintptr)(rune)'a'), V(((MyString)(@string)"a"u8))),
    new(V((nint)(-1)), V(((MyString)(@string)"\uFFFD"u8))),
    new(V((int8)(-2)), V(((MyString)(@string)"\uFFFD"u8))),
    new(V((int16)(-3)), V(((MyString)(@string)"\uFFFD"u8))),
    new(V((int32)(-4)), V(((MyString)(@string)"\uFFFD"u8))),
    new(V((int64)(-5)), V(((MyString)(@string)"\uFFFD"u8))),
    new(V((nuint)0x110001), V(((MyString)(@string)"\uFFFD"u8))),
    new(V((uint32)0x110002), V(((MyString)(@string)"\uFFFD"u8))),
    new(V((uint64)0x110003), V(((MyString)(@string)"\uFFFD"u8))),
    new(V((uintptr)0x110004), V(((MyString)(@string)"\uFFFD"u8))),
    new(V(((@string)"bytes1"u8)), V(((MyBytes)slice<byte>((@string)"bytes1"u8)))),
    new(V(((MyBytes)slice<byte>((@string)"bytes2"u8))), V(((@string)"bytes2"u8))),
    new(V(((MyBytes)slice<byte>((@string)"bytes3"u8))), V(((MyBytes)slice<byte>((@string)"bytes3"u8)))),
    new(V(((MyString)(@string)"bytes1"u8)), V(((MyBytes)slice<byte>((@string)"bytes1"u8)))),
    new(V(((MyBytes)slice<byte>((@string)"bytes2"u8))), V(((MyString)(@string)"bytes2"u8))),
    new(V(((@string)"runes♝"u8)), V(((MyRunes)slice<int32>((@string)"runes♝"u8)))),
    new(V(((MyRunes)slice<int32>((@string)"runes♕"u8))), V(((@string)"runes♕"u8))),
    new(V(((MyRunes)slice<int32>((@string)"runes🙈🙉🙊"u8))), V(((MyRunes)slice<int32>((@string)"runes🙈🙉🙊"u8)))),
    new(V(((MyString)(@string)"runes♝"u8)), V(((MyRunes)slice<int32>((@string)"runes♝"u8)))),
    new(V(((MyRunes)slice<int32>((@string)"runes♕"u8))), V(((MyString)(@string)"runes♕"u8))),
    new(V(slice<byte>(default!)), V(new byte[]{}.array())),
    new(V(new byte[]{}.slice()), V(new byte[]{}.array())),
    new(V(new byte[]{1}.slice()), V(new byte[]{1}.array())),
    new(V(new byte[]{1, 2}.slice()), V(new byte[]{1, 2}.array())),
    new(V(new byte[]{1, 2, 3}.slice()), V(new byte[]{1, 2, 3}.array())),
    new(V(((MyBytes)slice<byte>(default!))), V(new byte[]{}.array())),
    new(V(new MyBytes(new byte[]{}.slice())), V(new byte[]{}.array())),
    new(V(new MyBytes(new byte[]{1}.slice())), V(new byte[]{1}.array())),
    new(V(new MyBytes(new byte[]{1, 2}.slice())), V(new byte[]{1, 2}.array())),
    new(V(new MyBytes(new byte[]{1, 2, 3}.slice())), V(new byte[]{1, 2, 3}.array())),
    new(V(slice<byte>(default!)), V(new MyBytesArray0(new byte[]{}.array()))),
    new(V(new byte[]{}.slice()), V(((MyBytesArray0)new byte[]{}.array()))),
    new(V(new byte[]{1, 2, 3, 4}.slice()), V(((MyBytesArray)new byte[]{1, 2, 3, 4}.array()))),
    new(V(new MyBytes(new byte[]{}.slice())), V(((MyBytesArray0)new byte[]{}.array()))),
    new(V(new MyBytes(new byte[]{5, 6, 7, 8}.slice())), V(((MyBytesArray)new byte[]{5, 6, 7, 8}.array()))),
    new(V(new MyByte[]{}.slice()), V(new MyByte[]{}.array())),
    new(V(new MyByte[]{1, 2}.slice()), V(new MyByte[]{1, 2}.array())),
    new(V(slice<byte>(default!)), V(ж<array<byte>>.NilBoxOfDims(0L))),
    new(V(new byte[]{}.slice()), V(Ꮡ(new array<byte>(0)))),
    new(V(new byte[]{7}.slice()), V(Ꮡ(new byte[]{7}.array()))),
    new(V(((MyBytes)slice<byte>(default!))), V(ж<array<byte>>.NilBoxOfDims(0L))),
    new(V(((MyBytes)new byte[]{}.slice())), V(Ꮡ(new array<byte>(0)))),
    new(V(((MyBytes)new byte[]{9}.slice())), V(Ꮡ(new byte[]{9}.array()))),
    new(V(slice<byte>(default!)), V(((MyBytesArrayPtr0)nil))),
    new(V(new byte[]{}.slice()), V(new MyBytesArrayPtr0(Ꮡ(new array<byte>(0))))),
    new(V(new byte[]{1, 2, 3, 4}.slice()), V(new MyBytesArrayPtr(Ꮡ(new byte[]{1, 2, 3, 4}.array())))),
    new(V(((MyBytes)new byte[]{}.slice())), V(new MyBytesArrayPtr0(Ꮡ(new array<byte>(0))))),
    new(V(((MyBytes)new byte[]{5, 6, 7, 8}.slice())), V(new MyBytesArrayPtr(Ꮡ(new byte[]{5, 6, 7, 8}.array())))),
    new(V(slice<byte>(default!)), V(ж<MyBytesArray0>.NilBoxOfDims(0L))),
    new(V(new byte[]{}.slice()), V(Ꮡ(new MyBytesArray0(new array<byte>(0))))),
    new(V(new byte[]{1, 2, 3, 4}.slice()), V(Ꮡ(new MyBytesArray(new byte[]{1, 2, 3, 4}.array())))),
    new(V(((MyBytes)slice<byte>(default!))), V(ж<MyBytesArray0>.NilBoxOfDims(0L))),
    new(V(((MyBytes)new byte[]{}.slice())), V(Ꮡ(new MyBytesArray0(new array<byte>(0))))),
    new(V(((MyBytes)new byte[]{5, 6, 7, 8}.slice())), V(Ꮡ(new MyBytesArray(new byte[]{5, 6, 7, 8}.array())))),
    new(V(Ꮡ(new array<byte>(0))), V(@new<MyBytesArray0>())),
    new(V(@new<MyBytesArray0>()), V(Ꮡ(new array<byte>(0)))),
    new(V(((MyBytesArrayPtr0)nil)), V(ж<array<byte>>.NilBoxOfDims(0L))),
    new(V(ж<array<byte>>.NilBoxOfDims(0L)), V(((MyBytesArrayPtr0)nil))),
    new(V(@new<nint>()), V(@new<integer>())),
    new(V(@new<integer>()), V(@new<nint>())),
    new(V(new Empty(nil)), V(new EmptyStruct())),
    new(V(@new<Empty>()), V(@new<EmptyStruct>())),
    new(V(new EmptyStruct()), V(new Empty(nil))),
    new(V(@new<EmptyStruct>()), V(@new<Empty>())),
    new(V(new Empty(nil)), V(new Empty(nil))),
    new(V(new MyBytes(new byte[]{}.slice())), V(new byte[]{}.slice())),
    new(V(new byte[]{}.slice()), V(new MyBytes(new byte[]{}.slice()))),
    new(V(((Action)(default!)).OrTypedNilFunc()), V((default(Action)!).OrTypedNilFunc())),
    new(V((default(Action)!).OrTypedNilFunc()), V(((Action)(default!)).OrTypedNilFunc())),
    new(V(new iᴛ1()), V(new iᴛ2())),
    new(V(new iᴛ2()), V(new iᴛ1())),
    new(V(new MyStruct(nil)), V(new iᴛ1())),
    new(V(new iᴛ1()), V(new MyStruct(nil))),
    new(V(new MyStruct(nil)), V(new iᴛ2())),
    new(V(new iᴛ2()), V(new MyStruct(nil))),
    new(V(new MyStruct1(nil)), V(new MyStruct2(nil))),
    new(V(new MyStruct2(nil)), V(new MyStruct1(nil))),
    new(V(((ж<byte>)nil)), V(((ж<MyByte>)nil))),
    new(V(((ж<MyByte>)nil)), V(((ж<byte>)nil))),
    new(V(new byte[]{}.array(2)), V(new byte[]{}.array(2))),
    new(V(new byte[]{}.array(3)), V(new byte[]{}.array(3))),
    new(V(new MyBytesArray0(new byte[]{}.array())), V(new byte[]{}.array())),
    new(V(new byte[]{}.array()), V(new MyBytesArray0(new byte[]{}.array()))),
    new(V(((ж<ж<byte>>)nil)), V(((ж<ж<byte>>)nil))),
    new(V(((ж<ж<MyByte>>)nil)), V(((ж<ж<MyByte>>)nil))),
    new(V((channel<byte>)(default!)), V((channel<byte>)(default!))),
    new(V((channel<MyByte>)(default!)), V((channel<MyByte>)(default!))),
    new(V((slice<byte>)(default!)), V((slice<byte>)(default!))),
    new(V((slice<MyByte>)(default!)), V((slice<MyByte>)(default!))),
    new(V(((map<nint, byte>)default!)), V(((map<nint, byte>)default!))),
    new(V(((map<nint, MyByte>)default!)), V(((map<nint, MyByte>)default!))),
    new(V(((map<byte, nint>)default!)), V(((map<byte, nint>)default!))),
    new(V(((map<MyByte, nint>)default!)), V(((map<MyByte, nint>)default!))),
    new(V(new byte[]{}.array(2)), V(new byte[]{}.array(2))),
    new(V(new MyByte[]{}.array(2)), V(new MyByte[]{}.array(2))),
    new(V(((ж<ж<ж<nint>>>)nil)), V(((ж<ж<ж<nint>>>)nil))),
    new(V(((ж<ж<ж<byte>>>)nil)), V(((ж<ж<ж<byte>>>)nil))),
    new(V(((ж<ж<ж<int32>>>)nil)), V(((ж<ж<ж<int32>>>)nil))),
    new(V(((ж<ж<ж<int64>>>)nil)), V(((ж<ж<ж<int64>>>)nil))),
    new(V((channel<byte>)(default!)), V((channel<byte>)(default!))),
    new(V((channel<MyByte>)(default!)), V((channel<MyByte>)(default!))),
    new(V(((map<nint, bool>)default!)), V(((map<nint, bool>)default!))),
    new(V(((map<nint, byte>)default!)), V(((map<nint, byte>)default!))),
    new(V(((map<nuint, bool>)default!)), V(((map<nuint, bool>)default!))),
    new(V(slice<nuint>(default!)), V(slice<nuint>(default!))),
    new(V(slice<nint>(default!)), V(slice<nint>(default!))),
    new(V(@new<any>()), V(@new<any>())),
    new(V(@new<Δio.Reader>()), V(@new<Δio.Reader>())),
    new(V(@new<Δio.Writer>()), V(@new<Δio.Writer>())),
    new(V(((IntChan)default!)), V(channel/*<-*/<nint>.SendOnly)),
    new(V(((IntChan)default!)), V(/*<-*/channel<nint>.RecvOnly)),
    new(V((channel<nint>)(default!)), V(((IntChanRecv)default!))),
    new(V((channel<nint>)(default!)), V(((IntChanSend)default!))),
    new(V(((IntChanRecv)default!)), V(/*<-*/channel<nint>.RecvOnly)),
    new(V(/*<-*/channel<nint>.RecvOnly), V(((IntChanRecv)default!))),
    new(V(((IntChanSend)default!)), V(channel/*<-*/<nint>.SendOnly)),
    new(V(channel/*<-*/<nint>.SendOnly), V(((IntChanSend)default!))),
    new(V(((IntChan)default!)), V((channel<nint>)(default!))),
    new(V((channel<nint>)(default!)), V(((IntChan)default!))),
    new(V((channel<nint>)(default!)), V(/*<-*/channel<nint>.RecvOnly)),
    new(V((channel<nint>)(default!)), V(channel/*<-*/<nint>.SendOnly)),
    new(V(((BytesChan)default!)), V(channel/*<-*/<slice<byte>>.SendOnly)),
    new(V(((BytesChan)default!)), V(/*<-*/channel<slice<byte>>.RecvOnly)),
    new(V((channel<slice<byte>>)(default!)), V(((BytesChanRecv)default!))),
    new(V((channel<slice<byte>>)(default!)), V(((BytesChanSend)default!))),
    new(V(((BytesChanRecv)default!)), V(/*<-*/channel<slice<byte>>.RecvOnly)),
    new(V(/*<-*/channel<slice<byte>>.RecvOnly), V(((BytesChanRecv)default!))),
    new(V(((BytesChanSend)default!)), V(channel/*<-*/<slice<byte>>.SendOnly)),
    new(V(channel/*<-*/<slice<byte>>.SendOnly), V(((BytesChanSend)default!))),
    new(V(((BytesChan)default!)), V((channel<slice<byte>>)(default!))),
    new(V((channel<slice<byte>>)(default!)), V(((BytesChan)default!))),
    new(V((channel<slice<byte>>)(default!)), V(/*<-*/channel<slice<byte>>.RecvOnly)),
    new(V((channel<slice<byte>>)(default!)), V(channel/*<-*/<slice<byte>>.SendOnly)),
    new(V(((IntChan)default!)), V(((IntChan)default!))),
    new(V(((IntChanRecv)default!)), V(((IntChanRecv)default!))),
    new(V(((IntChanSend)default!)), V(((IntChanSend)default!))),
    new(V(((BytesChan)default!)), V(((BytesChan)default!))),
    new(V(((BytesChanRecv)default!)), V(((BytesChanRecv)default!))),
    new(V(((BytesChanSend)default!)), V(((BytesChanSend)default!))),
    new(V((nint)1), EmptyInterfaceV((nint)1)),
    new(V(((@string)"hello"u8)), EmptyInterfaceV(((@string)"hello"u8))),
    new(V(@new<bytes.Buffer>()), ReaderV(new reflect_test_package.bytes_BufferжReader(@new<bytes.Buffer>()))),
    new(ReadWriterV(new reflect_test_package.bytes_BufferжReadWriter(@new<bytes.Buffer>())), ReaderV(new reflect_test_package.bytes_BufferжReader(@new<bytes.Buffer>()))),
    new(V(@new<bytes.Buffer>()), ReadWriterV(new reflect_test_package.bytes_BufferжReadWriter(@new<bytes.Buffer>())))
}.slice();

public static void TestConvert(ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var canConvert = new map<array<reflectꓸType>, bool>{};
    var all = new map<reflectꓸType, bool>{};
    foreach (var (_, tt) in convertTests) {
        var t1 = tt.@in.Type();
        if (!t1.ConvertibleTo(t1)) {
            Ꮡt.Errorf("(%s).ConvertibleTo(%s) = false, want true"u8, t1, t1);
            continue;
        }
        var t2 = tt.@out.Type();
        if (!t1.ConvertibleTo(t2)) {
            Ꮡt.Errorf("(%s).ConvertibleTo(%s) = false, want true"u8, t1, t2);
            continue;
        }
        all[t1] = true;
        all[t2] = true;
        canConvert[new reflectꓸType[]{t1, t2}.array()] = true;
        // vout1 represents the in value converted to the in type.
        var v1 = tt.@in;
        if (!v1.CanConvert(t1)) {
            Ꮡt.Errorf("ValueOf(%T(%[1]v)).CanConvert(%s) = false, want true"u8, tt.@in.Interface(), t1);
        }
        var vout1 = v1.Convert(t1);
        var out1 = vout1.Interface();
        if (!AreEqual(vout1.Type(), tt.@in.Type()) || !DeepEqual(out1, tt.@in.Interface())) {
            Ꮡt.Errorf("ValueOf(%T(%[1]v)).Convert(%s) = %T(%[3]v), want %T(%[4]v)"u8, tt.@in.Interface(), t1, out1, tt.@in.Interface());
        }
        // vout2 represents the in value converted to the out type.
        if (!v1.CanConvert(t2)) {
            Ꮡt.Errorf("ValueOf(%T(%[1]v)).CanConvert(%s) = false, want true"u8, tt.@in.Interface(), t2);
        }
        var vout2 = v1.Convert(t2);
        var out2 = vout2.Interface();
        if (!AreEqual(vout2.Type(), tt.@out.Type()) || !DeepEqual(out2, tt.@out.Interface())) {
            Ꮡt.Errorf("ValueOf(%T(%[1]v)).Convert(%s) = %T(%[3]v), want %T(%[4]v)"u8, tt.@in.Interface(), t2, out2, tt.@out.Interface());
        }
        {
            reflectꓸKind got = vout2.Kind();
            reflectꓸKind want = vout2.Type().Kind(); if (got != want) {
                Ꮡt.Errorf("ValueOf(%T(%[1]v)).Convert(%s) has internal kind %v want %v"u8, tt.@in.Interface(), t1, got, want);
            }
        }
        // vout3 represents a new value of the out type, set to vout2.  This makes
        // sure the converted value vout2 is really usable as a regular value.
        var vout3 = New(t2).Elem();
        vout3.Set(vout2);
        var out3 = vout3.Interface();
        if (!AreEqual(vout3.Type(), tt.@out.Type()) || !DeepEqual(out3, tt.@out.Interface())) {
            Ꮡt.Errorf("Set(ValueOf(%T(%[1]v)).Convert(%s)) = %T(%[3]v), want %T(%[4]v)"u8, tt.@in.Interface(), t2, out3, tt.@out.Interface());
        }
        if (reflect_internal_test_package.IsRO(v1)) {
            Ꮡt.Errorf("table entry %v is RO, should not be"u8, v1);
        }
        if (reflect_internal_test_package.IsRO(vout1)) {
            Ꮡt.Errorf("self-conversion output %v is RO, should not be"u8, vout1);
        }
        if (reflect_internal_test_package.IsRO(vout2)) {
            Ꮡt.Errorf("conversion output %v is RO, should not be"u8, vout2);
        }
        if (reflect_internal_test_package.IsRO(vout3)) {
            Ꮡt.Errorf("set(conversion output) %v is RO, should not be"u8, vout3);
        }
        if (!reflect_internal_test_package.IsRO(reflect_internal_test_package.MakeRO(v1).Convert(t1))) {
            Ꮡt.Errorf("RO self-conversion output %v is not RO, should be"u8, v1);
        }
        if (!reflect_internal_test_package.IsRO(reflect_internal_test_package.MakeRO(v1).Convert(t2))) {
            Ꮡt.Errorf("RO conversion output %v is not RO, should be"u8, v1);
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
                    Ꮡt.Errorf("(%s).ConvertibleTo(%s) = %v, want %v"u8, t1, t2, ok, expectOK);
                }
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string reflectCannotConvertˢ = "reflect: cannot convert slice with length 4 to pointer to array with length 8"u8;
internal static readonly @string reflectCannotConvertˢ2 = "reflect: cannot convert slice with length 4 to array with length 8"u8;

public static void TestConvertPanic(ж<Δtesting.T> Ꮡt) {
    var s = new slice<byte>(4);
    var p = Ꮡ(new array<byte>(8));
    ref var v = ref heap<reflectꓸValue>(out var Ꮡv);
    v = ValueOf(s);
    var pt = TypeOf(p.OrTypedNil());
    if (!v.Type().ConvertibleTo(pt)) {
        Ꮡt.Errorf("[]byte should be convertible to *[8]byte"u8);
    }
    if (v.CanConvert(pt)) {
        Ꮡt.Errorf("slice with length 4 should not be convertible to *[8]byte"u8);
    }
    var ptʗ1 = pt;
    var vʗ1 = v;
    shouldPanic(reflectCannotConvertˢ, () => {
        _ = vʗ1.Convert(ptʗ1);
    });
    if (v.CanConvert(pt.Elem())) {
        Ꮡt.Errorf("slice with length 4 should not be convertible to [8]byte"u8);
    }
    var ptʗ2 = pt;
    var vʗ2 = v;
    shouldPanic(reflectCannotConvertˢ2, () => {
        _ = vʗ2.Convert(ptʗ2.Elem());
    });
}

public static void TestConvertSlice2Array(ж<Δtesting.T> Ꮡt) {
    var s = new slice<nint>(4);
    var p = new nint[]{}.array(4);
    var pt = TypeOf(p);
    var ov = ValueOf(s);
    var v = ov.Convert(pt);
    // Converting a slice to non-empty array needs to return
    // a non-addressable copy of the original memory.
    if (v.CanAddr()) {
        Ꮡt.Fatalf("convert slice to non-empty array returns an addressable copy array"u8);
    }
    foreach (var (i, _) in s) {
        ov.Index(i).Set(ValueOf(i + 1));
    }
    foreach (var (i, _) in s) {
        if (v.Index(i).Int() != 0) {
            Ꮡt.Fatalf("slice (%v) mutation visible in converted result (%v)"u8, ov, v);
        }
    }
}

internal static float32 gFloat32;

internal const uint32 snan = 0x7f800001;

[GoLocalName("myFloat32")] [GoType("num:float32")] internal partial struct TestConvertNaNs_myFloat32;

public static void TestConvertNaNs(ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // Test to see if a store followed by a load of a signaling NaN
    // maintains the signaling bit. (This used to fail on the 387 port.)
    gFloat32 = Δmath.Float32frombits(snan);
    Δruntime.Gosched(); // make sure we don't optimize the store/load away
    {
        var got = Δmath.Float32bits(gFloat32); if (got != snan) {
            Ꮡt.Errorf("store/load of sNaN not faithful, got %x want %x"u8, got, snan);
        }
    }
    var x = V(((TestConvertNaNs_myFloat32)Δmath.Float32frombits(snan)));
    var y = x.Convert(TypeOf((float32)0F));
    var z = y.Interface()._<float32>();
    {
        var got = Δmath.Float32bits(z); if (got != snan) {
            Ꮡt.Errorf("signaling nan conversion got %x, want %x"u8, got, snan);
        }
    }
}

[GoType] partial struct ComparableStruct {
    public nint X;
}

[GoType] partial struct NonComparableStruct {
    public nint X;
    public map<@string, nint> Y;
}


[GoType("dyn")] partial struct comparableTestsᴛ1 {
    internal reflectꓸType typ;
    internal bool ok;
}
internal static slice<comparableTestsᴛ1> comparableTests = new comparableTestsᴛ1[]{
    new(TypeOf((nint)(1)), true),
    new(TypeOf((@string)"hello"u8), true),
    new(TypeOf(@new<byte>()), true),
    new(TypeOf(((Action)(default!)).OrTypedNilFunc()), false),
    new(TypeOf(new byte[]{}.slice()), false),
    new(TypeOf(new map<@string, nint>{}), false),
    new(TypeOf(new channel<nint>(0)), true),
    new(TypeOf(1.5D), true),
    new(TypeOf(false), true),
    new(TypeOf(1D.i()), true),
    new(TypeOf(new ComparableStruct(nil)), true),
    new(TypeOf(new NonComparableStruct(nil)), false),
    new(TypeOf(new map<@string, nint>[]{}.array(10)), false),
    new(TypeOf(new @string[]{}.array(10)), true),
    new(TypeOf(@new<any>()).Elem(), true)
}.slice();

public static void TestComparable(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, tt) in comparableTests) {
        {
            var ok = tt.typ.Comparable(); if (ok != tt.ok) {
                Ꮡt.Errorf("TypeOf(%v).Comparable() = %v, want %v"u8, tt.typ, ok, tt.ok);
            }
        }
    }
}

public static void TestValueOverflow(ж<Δtesting.T> Ꮡt) {
    {
        var ovf = V((float64)0D).OverflowFloat(1e300D); if (ovf) {
            Ꮡt.Errorf("%v wrongly overflows float64"u8, 1e300D);
        }
    }
    var maxFloat32 = (float64)(340282346638528859811704183484516925440D);
    {
        var ovf = V((float32)0F).OverflowFloat(maxFloat32); if (ovf) {
            Ꮡt.Errorf("%v wrongly overflows float32"u8, maxFloat32);
        }
    }
    var ovfFloat32 = (float64)(340282346638528897590636046441678635008D);
    {
        var ovf = V((float32)0F).OverflowFloat(ovfFloat32); if (!ovf) {
            Ꮡt.Errorf("%v should overflow float32"u8, ovfFloat32);
        }
    }
    {
        var ovf = V((float32)0F).OverflowFloat(-ovfFloat32); if (!ovf) {
            Ꮡt.Errorf("%v should overflow float32"u8, -ovfFloat32);
        }
    }
    var maxInt32 = (int64)0x7fffffff;
    {
        var ovf = V((int32)0).OverflowInt(maxInt32); if (ovf) {
            Ꮡt.Errorf("%v wrongly overflows int32"u8, maxInt32);
        }
    }
    {
        var ovf = V((int32)0).OverflowInt(((int64)(-1) << (int)(31))); if (ovf) {
            Ꮡt.Errorf("%v wrongly overflows int32"u8, (-(int64)1 << (int)(31)));
        }
    }
    var ovfInt32 = (int64)(2147483648L);
    {
        var ovf = V((int32)0).OverflowInt(ovfInt32); if (!ovf) {
            Ꮡt.Errorf("%v should overflow int32"u8, ovfInt32);
        }
    }
    var maxUint32 = (uint64)0xffffffffU;
    {
        var ovf = V((uint32)0).OverflowUint(maxUint32); if (ovf) {
            Ꮡt.Errorf("%v wrongly overflows uint32"u8, maxUint32);
        }
    }
    var ovfUint32 = (uint64)(((uint64)1 << (int)(32)));
    {
        var ovf = V((uint32)0).OverflowUint(ovfUint32); if (!ovf) {
            Ꮡt.Errorf("%v should overflow uint32"u8, ovfUint32);
        }
    }
}

public static void TestTypeOverflow(ж<Δtesting.T> Ꮡt) {
    {
        var ovf = TypeFor<float64>().OverflowFloat(1e300D); if (ovf) {
            Ꮡt.Errorf("%v wrongly overflows float64"u8, 1e300D);
        }
    }
    var maxFloat32 = (float64)(340282346638528859811704183484516925440D);
    {
        var ovf = TypeFor<float32>().OverflowFloat(maxFloat32); if (ovf) {
            Ꮡt.Errorf("%v wrongly overflows float32"u8, maxFloat32);
        }
    }
    var ovfFloat32 = (float64)(340282346638528897590636046441678635008D);
    {
        var ovf = TypeFor<float32>().OverflowFloat(ovfFloat32); if (!ovf) {
            Ꮡt.Errorf("%v should overflow float32"u8, ovfFloat32);
        }
    }
    {
        var ovf = TypeFor<float32>().OverflowFloat(-ovfFloat32); if (!ovf) {
            Ꮡt.Errorf("%v should overflow float32"u8, -ovfFloat32);
        }
    }
    var maxInt32 = (int64)0x7fffffff;
    {
        var ovf = TypeFor<int32>().OverflowInt(maxInt32); if (ovf) {
            Ꮡt.Errorf("%v wrongly overflows int32"u8, maxInt32);
        }
    }
    {
        var ovf = TypeFor<int32>().OverflowInt(((int64)(-1) << (int)(31))); if (ovf) {
            Ꮡt.Errorf("%v wrongly overflows int32"u8, (-(int64)1 << (int)(31)));
        }
    }
    var ovfInt32 = (int64)(2147483648L);
    {
        var ovf = TypeFor<int32>().OverflowInt(ovfInt32); if (!ovf) {
            Ꮡt.Errorf("%v should overflow int32"u8, ovfInt32);
        }
    }
    var maxUint32 = (uint64)0xffffffffU;
    {
        var ovf = TypeFor<uint32>().OverflowUint(maxUint32); if (ovf) {
            Ꮡt.Errorf("%v wrongly overflows uint32"u8, maxUint32);
        }
    }
    var ovfUint32 = (uint64)(((uint64)1 << (int)(32)));
    {
        var ovf = TypeFor<uint32>().OverflowUint(ovfUint32); if (!ovf) {
            Ꮡt.Errorf("%v should overflow uint32"u8, ovfUint32);
        }
    }
}

internal static void checkSameType(ж<Δtesting.T> Ꮡt, reflectꓸType x, any y) {
    if (!AreEqual(x, TypeOf(y)) || !AreEqual(TypeOf(Zero(x).Interface()), TypeOf(y))) {
        Ꮡt.Errorf("did not find preexisting type for %s (vs %s)"u8, TypeOf(x), TypeOf(y));
    }
}

[GoType("dyn")] internal partial struct TestArrayOf_tests {
    internal nint n;
    internal Func<nint, any> value;
    internal bool comparable;
    internal @string want;
}

[GoLocalName("Tint")] [GoType("num:nint")] internal partial struct TestArrayOf_Tint;

[GoLocalName("Tint")] [GoType("num:nint")] internal partial struct TestArrayOf_Tintᴛ1;

[GoLocalName("Tfloat")] [GoType("num:float64")] internal partial struct TestArrayOf_Tfloat;

[GoLocalName("Tstring")] [GoType("@string")] internal partial struct TestArrayOf_Tstring;

[GoType("dyn")] internal partial struct TestArrayOf_Tstruct {
    public nint V;
}

[GoLocalName("Tint")] [GoType("num:nint")] internal partial struct TestArrayOf_Tintᴛ2;

[GoLocalName("Tint")] [GoType("num:nint")] internal partial struct TestArrayOf_Tintᴛ3;

[GoType("dyn")] internal partial struct TestArrayOf_Tstructᴛ1 {
    public array<nint> V = new(1);
}

[GoType("dyn")] internal partial struct TestArrayOf_Tstructᴛ2 {
    public slice<nint> V;
}

[GoType("dyn")] internal partial struct TestArrayOf_TstructUV {
    public nint U, V;
}

[GoType("dyn")] internal partial struct TestArrayOf_TstructUVᴛ1 {
    public nint U;
    public float64 V;
}

[GoLocalName("T")] [GoType("num:nint")] internal partial struct TestArrayOf_T;

public static void TestArrayOf(ж<Δtesting.T> Ꮡt) {
    // check construction and use of type not in binary
    var tests = new TestArrayOf_tests[]{
        new(
            n: 0,
            value: (nint i) => {
                return ((TestArrayOf_Tint)i);
            },
            comparable: true,
            want: "[]"u8
        ),
        new(
            n: 10,
            value: (nint i) => {
                return ((TestArrayOf_Tintᴛ1)i);
            },
            comparable: true,
            want: "[0 1 2 3 4 5 6 7 8 9]"u8
        ),
        new(
            n: 10,
            value: (nint i) => {
                return ((TestArrayOf_Tfloat)(float64)i);
            },
            comparable: true,
            want: "[0 1 2 3 4 5 6 7 8 9]"u8
        ),
        new(
            n: 10,
            value: (nint i) => {
                return ((TestArrayOf_Tstring)strconv.Itoa(i));
            },
            comparable: true,
            want: "[0 1 2 3 4 5 6 7 8 9]"u8
        ),
        new(
            n: 10,
            value: (nint i) => {
                return new TestArrayOf_Tstruct(i);
            },
            comparable: true,
            want: "[{0} {1} {2} {3} {4} {5} {6} {7} {8} {9}]"u8
        ),
        new(
            n: 10,
            value: (nint i) => {
                return new TestArrayOf_Tintᴛ2[]{((TestArrayOf_Tintᴛ2)i)}.slice();
            },
            comparable: false,
            want: "[[0] [1] [2] [3] [4] [5] [6] [7] [8] [9]]"u8
        ),
        new(
            n: 10,
            value: (nint i) => {
                return new TestArrayOf_Tintᴛ3[]{((TestArrayOf_Tintᴛ3)i)}.array();
            },
            comparable: true,
            want: "[[0] [1] [2] [3] [4] [5] [6] [7] [8] [9]]"u8
        ),
        new(
            n: 10,
            value: (nint i) => {
                return new TestArrayOf_Tstructᴛ1(new nint[]{i}.array());
            },
            comparable: true,
            want: "[{[0]} {[1]} {[2]} {[3]} {[4]} {[5]} {[6]} {[7]} {[8]} {[9]}]"u8
        ),
        new(
            n: 10,
            value: (nint i) => {
                return new TestArrayOf_Tstructᴛ2(new nint[]{i}.slice());
            },
            comparable: false,
            want: "[{[0]} {[1]} {[2]} {[3]} {[4]} {[5]} {[6]} {[7]} {[8]} {[9]}]"u8
        ),
        new(
            n: 10,
            value: (nint i) => {
                return new TestArrayOf_TstructUV(i, i);
            },
            comparable: true,
            want: "[{0 0} {1 1} {2 2} {3 3} {4 4} {5 5} {6 6} {7 7} {8 8} {9 9}]"u8
        ),
        new(
            n: 10,
            value: (nint i) => {
                return new TestArrayOf_TstructUVᴛ1(i, (float64)i);
            },
            comparable: true,
            want: "[{0 0} {1 1} {2 2} {3 3} {4 4} {5 5} {6 6} {7 7} {8 8} {9 9}]"u8
        )
    }.slice();
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
            Ꮡt.Errorf("constructed array = %s, want %s"u8, s, table.want);
        }
        if (table.comparable != at.Comparable()) {
            Ꮡt.Errorf("constructed array (%#v) is comparable=%v, want=%v"u8, v.Interface(), at.Comparable(), table.comparable);
        }
        if (table.comparable) {
            if (table.n > 0) {
                if (DeepEqual(vnot.Interface(), v.Interface())) {
                    Ꮡt.Errorf(
                        "arrays (%#v) compare ok (but should not)"u8,
                        v.Interface());
                }
            }
            if (!DeepEqual(vok.Interface(), v.Interface())) {
                Ꮡt.Errorf(
                    "arrays (%#v) compare NOT-ok (but should)"u8,
                    v.Interface());
            }
        }
    }
    checkSameType(Ꮡt, ArrayOf(5, TypeOf(((TestArrayOf_T)1))), new TestArrayOf_T[]{}.array(5));
}

[GoLocalName("T")] [GoType("ж<uintptr>")] internal partial class TestArrayOfGC_T;

public static void TestArrayOfGC(ж<Δtesting.T> Ꮡt) {
    var tt = TypeOf(((TestArrayOfGC_T)nil));
    const nint n = 100;
    slice<any> x = default!;
    for (nint i = 0; i < n; i++) {
        var v = New(ArrayOf(n, tt)).Elem();
        for (nint j = 0; j < v.Len(); j++) {
            var p = @new<uintptr>();
            p.Value = (uintptr)(i * n + j);
            v.Index(j).Set(ValueOf(p.OrTypedNil()).Convert(tt));
        }
        x = append(x, v.Interface());
    }
    Δruntime.GC();
    foreach (var (i, xi) in x) {
        var v = ValueOf(xi);
        for (nint j = 0; j < v.Len(); j++) {
            var k = v.Index(j).Elem().Interface();
            if (!AreEqual(k, (uintptr)(i * n + j))) {
                Ꮡt.Errorf("lost x[%d][%d] = %d, want %d"u8, i, j, k, i * n + j);
            }
        }
    }
}

public static void TestArrayOfAlg(ж<Δtesting.T> Ꮡt) {
    var at = ArrayOf(6, TypeOf((byte)0));
    ref var v1 = ref heap<reflectꓸValue>(out var Ꮡv1);
    v1 = New(at).Elem();
    var v2 = New(at).Elem();
    if (!AreEqual(v1.Interface(), v1.Interface())) {
        Ꮡt.Errorf("constructed array %v not equal to itself"u8, v1.Interface());
    }
    v1.Index(5).Set(ValueOf((byte)1));
    {
        var (i1, i2) = (v1.Interface(), v2.Interface()); if (AreEqual(i1, i2)) {
            Ꮡt.Errorf("constructed arrays %v and %v should not be equal"u8, i1, i2);
        }
    }
    at = ArrayOf(6, TypeOf(slice<nint>(default!)));
    v1 = New(at).Elem();
    var v1ʗ1 = v1;
    shouldPanic(""u8, () => {
        _ = AreEqual(v1ʗ1.Interface(), v1ʗ1.Interface());
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string abcˢ = "abc"u8;
internal static readonly object efgˢ = (@string)"efg"u8;

public static void TestArrayOfGenericAlg(ж<Δtesting.T> Ꮡt) {
    var at1 = ArrayOf(5, TypeOf(((@string)""u8)));
    var at = ArrayOf(6, at1);
    var v1 = New(at).Elem();
    var v2 = New(at).Elem();
    if (!AreEqual(v1.Interface(), v1.Interface())) {
        Ꮡt.Errorf("constructed array %v not equal to itself"u8, v1.Interface());
    }
    v1.Index(0).Index(0).Set(ValueOf(abcˢ));
    v2.Index(0).Index(0).Set(ValueOf(efgˢ));
    {
        var (i1, i2) = (v1.Interface(), v2.Interface()); if (AreEqual(i1, i2)) {
            Ꮡt.Errorf("constructed arrays %v and %v should not be equal"u8, i1, i2);
        }
    }
    v1.Index(0).Index(0).Set(ValueOf(abcˢ));
    v2.Index(0).Index(0).Set(ValueOf((v1.Index(0).Index(0).String() + " "u8)[..3]));
    {
        var (i1, i2) = (v1.Interface(), v2.Interface()); if (!AreEqual(i1, i2)) {
            Ꮡt.Errorf("constructed arrays %v and %v should be equal"u8, i1, i2);
        }
    }
    // Test hash
    var m = MakeMap(MapOf(at, TypeOf((nint)0)));
    m.SetMapIndex(v1, ValueOf((nint)(1)));
    {
        var (i1, i2) = (v1.Interface(), v2.Interface()); if (!m.MapIndex(v2).IsValid()) {
            Ꮡt.Errorf("constructed arrays %v and %v have different hashes"u8, i1, i2);
        }
    }
}

[GoLocalName("T")] [GoType("[1]ж<byte>")] internal partial struct TestArrayOfDirectIface_T;

[GoLocalName("T")] [GoType("[0]ж<byte>")] internal partial struct TestArrayOfDirectIface_Tᴛ1;

public static void TestArrayOfDirectIface(ж<Δtesting.T> Ꮡt) {
    {
        ref var i1 = ref heap<any>(out var Ꮡi1);
        i1 = Zero(TypeOf(new TestArrayOfDirectIface_T(new ж<byte>[1].array()))).Interface();
        var v1 = ValueOf(Ꮡi1).Elem();
        var p1 = v1.InterfaceData()[1];
        ref var i2 = ref heap<any>(out var Ꮡi2);
        i2 = Zero(ArrayOf(1, PointerTo(TypeOf((int8)0)))).Interface();
        var v2 = ValueOf(Ꮡi2).Elem();
        var p2 = v2.InterfaceData()[1];
        if (p1 != 0) {
            Ꮡt.Errorf("got p1=%v. want=%v"u8, p1, (any)(default!));
        }
        if (p2 != 0) {
            Ꮡt.Errorf("got p2=%v. want=%v"u8, p2, (any)(default!));
        }
    }
    {
        ref var i1 = ref heap<any>(out var Ꮡi1);
        i1 = Zero(TypeOf(new TestArrayOfDirectIface_Tᴛ1(new ж<byte>[]{}.array()))).Interface();
        var v1 = ValueOf(Ꮡi1).Elem();
        var p1 = v1.InterfaceData()[1];
        ref var i2 = ref heap<any>(out var Ꮡi2);
        i2 = Zero(ArrayOf(0, PointerTo(TypeOf((int8)0)))).Interface();
        var v2 = ValueOf(Ꮡi2).Elem();
        var p2 = v2.InterfaceData()[1];
        if (p1 == 0) {
            Ꮡt.Errorf("got p1=%v. want=not-%v"u8, p1, (any)(default!));
        }
        if (p2 == 0) {
            Ꮡt.Errorf("got p2=%v. want=not-%v"u8, p2, (any)(default!));
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string reflectNegativeLengthˢ = "reflect: negative length passed to ArrayOf"u8;

// Ensure passing in negative lengths panics.
// See https://golang.org/issue/43603
public static void TestArrayOfPanicOnNegativeLength(ж<Δtesting.T> Ꮡt) {
    shouldPanic(reflectNegativeLengthˢ, () => {
        ArrayOf(-1, TypeOf((byte)0));
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string reflectTestTˢ = "[]reflect_test.T"u8;

[GoLocalName("T")] [GoType("num:nint")] internal partial struct TestSliceOf_T;

[GoLocalName("T1")] [GoType("num:nint")] internal partial struct TestSliceOf_T1;

public static void TestSliceOf(ж<Δtesting.T> Ꮡt) {
    var st = SliceOf(TypeOf(((TestSliceOf_T)1)));
    {
        @string got = st.String();
        @string wantΔ1 = reflectTestTˢ; if (got != wantΔ1) {
            Ꮡt.Errorf("SliceOf(T(1)).String()=%q, want %q"u8, got, wantΔ1);
        }
    }
    var v = MakeSlice(st, 10, 10);
    Δruntime.GC();
    for (nint i = 0; i < v.Len(); i++) {
        v.Index(i).Set(ValueOf(((TestSliceOf_T)i)));
        Δruntime.GC();
    }
    @string s = fmt.Sprint(v.Interface());
    @string want = "[0 1 2 3 4 5 6 7 8 9]"u8;
    if (s != want) {
        Ꮡt.Errorf("constructed slice = %s, want %s"u8, s, want);
    }
    checkSameType(Ꮡt, SliceOf(TypeOf(((TestSliceOf_T1)1))), new TestSliceOf_T1[]{}.slice());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object sliceSizeDoesNotOverflowˢ = (@string)"slice size does not overflow"u8;
internal static readonly object sliceOverflowDoesNotˢ = (@string)"slice overflow does not panic"u8;

public static void TestSliceOverflow(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        // check that MakeSlice panics when size of slice overflows uint
        UntypedFloat S = 1e6;
        nuint s = (nuint)S;
        nuint l = (unchecked((nuint)(18446744073709551615UL))) / s + 1;
        if (l * s >= s) {
            Ꮡt.Fatal(sliceSizeDoesNotOverflowˢ);
        }
        array<byte> x = new(1000000); /* S */
        var st = SliceOf(TypeOf(x));
        defer(() => {
            var err = recover();
            if (err == default!) {
                Ꮡt.Fatal(sliceOverflowDoesNotˢ);
            }
        }, ref ᒐ);
        MakeSlice(st, (nint)l, (nint)l);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoLocalName("T")] [GoType("ж<uintptr>")] internal partial class TestSliceOfGC_T;

public static void TestSliceOfGC(ж<Δtesting.T> Ꮡt) {
    var tt = TypeOf(((TestSliceOfGC_T)nil));
    var st = SliceOf(tt);
    const nint n = 100;
    slice<any> x = default!;
    for (nint i = 0; i < n; i++) {
        var v = MakeSlice(st, n, n);
        for (nint j = 0; j < v.Len(); j++) {
            var p = @new<uintptr>();
            p.Value = (uintptr)(i * n + j);
            v.Index(j).Set(ValueOf(p.OrTypedNil()).Convert(tt));
        }
        x = append(x, v.Interface());
    }
    Δruntime.GC();
    foreach (var (i, xi) in x) {
        var v = ValueOf(xi);
        for (nint j = 0; j < v.Len(); j++) {
            var k = v.Index(j).Elem().Interface();
            if (!AreEqual(k, (uintptr)(i * n + j))) {
                Ꮡt.Errorf("lost x[%d][%d] = %d, want %d"u8, i, j, k, i * n + j);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string hasInvalidNameˢ = "has invalid name"u8;
internal static readonly @string hasNoNameˢ = "has no name"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string structStrᶜ = @"struct { φ string; ValidName string; Val1dNam5 string }"u8;

public static void TestStructOfFieldName(ж<Δtesting.T> Ꮡt) {
    // invalid field name "1nvalid"
    shouldPanic(hasInvalidNameˢ, () => {
        StructOf(new Δreflect.StructField[]{
            new(Name: "Valid"u8, Type: TypeOf((@string)""u8)),
            new(Name: "1nvalid"u8, Type: TypeOf((@string)""u8))
        }.slice());
    });
    // invalid field name "+"
    shouldPanic(hasInvalidNameˢ, () => {
        StructOf(new Δreflect.StructField[]{
            new(Name: "Val1d"u8, Type: TypeOf((@string)""u8)),
            new(Name: "+"u8, Type: TypeOf((@string)""u8))
        }.slice());
    });
    // no field name
    shouldPanic(hasNoNameˢ, () => {
        StructOf(new Δreflect.StructField[]{
            new(Name: ""u8, Type: TypeOf((@string)""u8))
        }.slice());
    });
    // verify creation of a struct with valid struct fields
    var validFields = new Δreflect.StructField[]{
        new(
            Name: "φ"u8,
            Type: TypeOf((@string)""u8)
        ),
        new(
            Name: "ValidName"u8,
            Type: TypeOf((@string)""u8)
        ),
        new(
            Name: "Val1dNam5"u8,
            Type: TypeOf((@string)""u8)
        )
    }.slice();
    var validStruct = StructOf(validFields);
    @string structStr = structStrᶜ;
    {
        @string got = validStruct.String();
        @string want = structStr; if (got != want) {
            Ꮡt.Errorf("StructOf(validFields).String()=%q, want %q"u8, got, want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string duplicateFieldˢ = "duplicate field"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string stStrᶜ = @"struct { S string ""s""; X uint8 ""x""; Y uint64; Z [3]uint16 }"u8;

[GoType("dyn")] internal partial struct TestStructOf_i {
    public @string String;
    public byte X;
    public uint64 Y;
    public array<uint16> Z = new(3);
}

[GoType("dyn")] internal partial struct TestStructOf_iᴛ1 {
    public byte G1;
    public array<ж<byte>> G2 = new(0);
}

[GoType("dyn")] internal partial struct TestStructOf_y {
    public uint64 Y;
}

[GoType("dyn")] internal partial struct TestStructOf_yᴛ1 {
    public TestStructOf_structFieldType F;
}

public static void TestStructOf(ж<Δtesting.T> Ꮡt) {
    // check construction and use of type not in binary
    var fields = new Δreflect.StructField[]{
        new(
            Name: "S"u8,
            Tag: "s"u8,
            Type: TypeOf((@string)""u8)
        ),
        new(
            Name: "X"u8,
            Tag: "x"u8,
            Type: TypeOf((byte)0)
        ),
        new(
            Name: "Y"u8,
            Type: TypeOf((uint64)0)
        ),
        new(
            Name: "Z"u8,
            Type: TypeOf(new uint16[]{}.array(3))
        )
    }.slice();
    var st = StructOf(fields);
    var v = New(st).Elem();
    Δruntime.GC();
    v.FieldByName("X"u8).Set(ValueOf((byte)2));
    v.FieldByIndex(new nint[]{1}.slice()).Set(ValueOf((byte)1));
    Δruntime.GC();
    @string s = fmt.Sprint(v.Interface());
    @string want = @"{ 1 0 [0 0 0]}"u8;
    if (s != want) {
        Ꮡt.Errorf("constructed struct = %s, want %s"u8, s, want);
    }
    @string stStr = stStrᶜ;
    {
        @string got = st.String();
        @string wantΔ1 = stStr; if (got != wantΔ1) {
            Ꮡt.Errorf("StructOf(fields).String()=%q, want %q"u8, got, wantΔ1);
        }
    }
    // check the size, alignment and field offsets
    var stt = TypeOf(new TestStructOf_i());
    if (st.Size() != stt.Size()) {
        Ꮡt.Errorf("constructed struct size = %v, want %v"u8, st.Size(), stt.Size());
    }
    if (st.Align() != stt.Align()) {
        Ꮡt.Errorf("constructed struct align = %v, want %v"u8, st.Align(), stt.Align());
    }
    if (st.FieldAlign() != stt.FieldAlign()) {
        Ꮡt.Errorf("constructed struct field align = %v, want %v"u8, st.FieldAlign(), stt.FieldAlign());
    }
    for (nint i = 0; i < st.NumField(); i++) {
        var o1 = st.Field(i).Offset;
        var o2 = stt.Field(i).Offset;
        if (o1 != o2) {
            Ꮡt.Errorf("constructed struct field %v offset = %v, want %v"u8, i, o1, o2);
        }
    }
    // Check size and alignment with a trailing zero-sized field.
    st = StructOf(new Δreflect.StructField[]{
        new(
            Name: "F1"u8,
            Type: TypeOf((byte)0)
        ),
        new(
            Name: "F2"u8,
            Type: TypeOf(new ж<byte>[]{}.array())
        )
    }.slice());
    stt = TypeOf(new TestStructOf_iᴛ1());
    if (st.Size() != stt.Size()) {
        Ꮡt.Errorf("constructed zero-padded struct size = %v, want %v"u8, st.Size(), stt.Size());
    }
    if (st.Align() != stt.Align()) {
        Ꮡt.Errorf("constructed zero-padded struct align = %v, want %v"u8, st.Align(), stt.Align());
    }
    if (st.FieldAlign() != stt.FieldAlign()) {
        Ꮡt.Errorf("constructed zero-padded struct field align = %v, want %v"u8, st.FieldAlign(), stt.FieldAlign());
    }
    for (nint i = 0; i < st.NumField(); i++) {
        var o1 = st.Field(i).Offset;
        var o2 = stt.Field(i).Offset;
        if (o1 != o2) {
            Ꮡt.Errorf("constructed zero-padded struct field %v offset = %v, want %v"u8, i, o1, o2);
        }
    }
    // check duplicate names
    shouldPanic(duplicateFieldˢ, () => {
        StructOf(new Δreflect.StructField[]{
            new(Name: "string"u8, PkgPath: "p"u8, Type: TypeOf((@string)""u8)),
            new(Name: "string"u8, PkgPath: "p"u8, Type: TypeOf((@string)""u8))
        }.slice());
    });
    shouldPanic(hasNoNameˢ, () => {
        StructOf(new Δreflect.StructField[]{
            new(Type: TypeOf((@string)""u8)),
            new(Name: "string"u8, PkgPath: "p"u8, Type: TypeOf((@string)""u8))
        }.slice());
    });
    shouldPanic(hasNoNameˢ, () => {
        StructOf(new Δreflect.StructField[]{
            new(Type: TypeOf((@string)""u8)),
            new(Type: TypeOf((@string)""u8))
        }.slice());
    });
    // check that type already in binary is found
    checkSameType(Ꮡt, StructOf(fields[2..3]), new TestStructOf_y());
    checkSameType(Ꮡt,
        StructOf(new Δreflect.StructField[]{
            new(
                Name: "F"u8,
                Type: TypeOf(((ж<TestStructOf_structFieldType>)nil)).Elem()
            )
        }.slice()),
        new TestStructOf_yᴛ1());
}

[GoType("dyn")] internal partial struct TestStructOfExportRules_S1 {
}

[GoType("dyn")] internal partial struct TestStructOfExportRules_s2 {
}

[GoType("dyn")] internal partial struct TestStructOfExportRules_ΦType {
}

[GoType("dyn")] internal partial struct TestStructOfExportRules_φType {
}

[GoType("dyn")] internal partial struct TestStructOfExportRules_tests {
    internal Δreflect.StructField field;
    internal bool mustPanic;
    internal bool exported;
}

public static void TestStructOfExportRules(ж<Δtesting.T> Ꮡt) {
    void testPanic(nint i, bool mustPanic, Action f) {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                var err = recover();
                if (err == default! && mustPanic) {
                    Ꮡt.Errorf("test-%d did not panic"u8, i);
                }
                if (err != default! && !mustPanic) {
                    Ꮡt.Errorf("test-%d panicked: %v\n"u8, i, err);
                }
            }, ref ᒐ);
            f();
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }
    var tests = new TestStructOfExportRules_tests[]{
        new(
            field: new StructField(Name: "S1"u8, Anonymous: true, Type: TypeOf(new TestStructOfExportRules_S1(nil))),
            exported: true
        ),
        new(
            field: new StructField(Name: "S1"u8, Anonymous: true, Type: TypeOf(((ж<TestStructOfExportRules_S1>)nil))),
            exported: true
        ),
        new(
            field: new StructField(Name: "s2"u8, Anonymous: true, Type: TypeOf(new TestStructOfExportRules_s2(nil))),
            mustPanic: true
        ),
        new(
            field: new StructField(Name: "s2"u8, Anonymous: true, Type: TypeOf(((ж<TestStructOfExportRules_s2>)nil))),
            mustPanic: true
        ),
        new(
            field: new StructField(Name: "Name"u8, Type: default!, PkgPath: ""u8),
            mustPanic: true
        ),
        new(
            field: new StructField(Name: ""u8, Type: TypeOf(new TestStructOfExportRules_S1(nil)), PkgPath: ""u8),
            mustPanic: true
        ),
        new(
            field: new StructField(Name: "S1"u8, Anonymous: true, Type: TypeOf(new TestStructOfExportRules_S1(nil)), PkgPath: "other/pkg"u8),
            mustPanic: true
        ),
        new(
            field: new StructField(Name: "S1"u8, Anonymous: true, Type: TypeOf(((ж<TestStructOfExportRules_S1>)nil)), PkgPath: "other/pkg"u8),
            mustPanic: true
        ),
        new(
            field: new StructField(Name: "s2"u8, Anonymous: true, Type: TypeOf(new TestStructOfExportRules_s2(nil)), PkgPath: "other/pkg"u8),
            mustPanic: true
        ),
        new(
            field: new StructField(Name: "s2"u8, Anonymous: true, Type: TypeOf(((ж<TestStructOfExportRules_s2>)nil)), PkgPath: "other/pkg"u8),
            mustPanic: true
        ),
        new(
            field: new StructField(Name: "s2"u8, Type: TypeOf((nint)0), PkgPath: "other/pkg"u8)
        ),
        new(
            field: new StructField(Name: "s2"u8, Type: TypeOf((nint)0), PkgPath: "other/pkg"u8)
        ),
        new(
            field: new StructField(Name: "S"u8, Type: TypeOf(new TestStructOfExportRules_S1(nil))),
            exported: true
        ),
        new(
            field: new StructField(Name: "S"u8, Type: TypeOf(((ж<TestStructOfExportRules_S1>)nil))),
            exported: true
        ),
        new(
            field: new StructField(Name: "S"u8, Type: TypeOf(new TestStructOfExportRules_s2(nil))),
            exported: true
        ),
        new(
            field: new StructField(Name: "S"u8, Type: TypeOf(((ж<TestStructOfExportRules_s2>)nil))),
            exported: true
        ),
        new(
            field: new StructField(Name: "s"u8, Type: TypeOf(new TestStructOfExportRules_S1(nil))),
            mustPanic: true
        ),
        new(
            field: new StructField(Name: "s"u8, Type: TypeOf(((ж<TestStructOfExportRules_S1>)nil))),
            mustPanic: true
        ),
        new(
            field: new StructField(Name: "s"u8, Type: TypeOf(new TestStructOfExportRules_s2(nil))),
            mustPanic: true
        ),
        new(
            field: new StructField(Name: "s"u8, Type: TypeOf(((ж<TestStructOfExportRules_s2>)nil))),
            mustPanic: true
        ),
        new(
            field: new StructField(Name: "s"u8, Type: TypeOf(new TestStructOfExportRules_S1(nil)), PkgPath: "other/pkg"u8)
        ),
        new(
            field: new StructField(Name: "s"u8, Type: TypeOf(((ж<TestStructOfExportRules_S1>)nil)), PkgPath: "other/pkg"u8)
        ),
        new(
            field: new StructField(Name: "s"u8, Type: TypeOf(new TestStructOfExportRules_s2(nil)), PkgPath: "other/pkg"u8)
        ),
        new(
            field: new StructField(Name: "s"u8, Type: TypeOf(((ж<TestStructOfExportRules_s2>)nil)), PkgPath: "other/pkg"u8)
        ),
        new(
            field: new StructField(Name: ""u8, Type: TypeOf(new TestStructOfExportRules_ΦType(nil))),
            mustPanic: true
        ),
        new(
            field: new StructField(Name: ""u8, Type: TypeOf(new TestStructOfExportRules_φType(nil))),
            mustPanic: true
        ),
        new(
            field: new StructField(Name: "Φ"u8, Type: TypeOf((nint)(0))),
            exported: true
        ),
        new(
            field: new StructField(Name: "φ"u8, Type: TypeOf((nint)(0))),
            exported: false
        )
    }.slice();
    foreach (var (i, vᴛ1) in tests) {
        ref var test = ref heap(new TestStructOfExportRules_tests(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        testPanic(i, test.mustPanic, () => {
            var typ = StructOf(new Δreflect.StructField[]{testʗ1.field}.slice());
            if (typ == default!) {
                Ꮡt.Errorf("test-%d: error creating struct type"u8, i);
                return;
            }
            var field = typ.Field(0);
            @string n = field.Name;
            if (n == ""u8) {
                throw panic("field.Name must not be empty");
            }
            var exported = token.IsExported(n);
            if (exported != testʗ1.exported) {
                Ꮡt.Errorf("test-%d: got exported=%v want exported=%v"u8, i, exported, testʗ1.exported);
            }
            if (field.PkgPath != testʗ1.field.PkgPath) {
                Ꮡt.Errorf("test-%d: got PkgPath=%q want pkgPath=%q"u8, i, field.PkgPath, testʗ1.field.PkgPath);
            }
        });
    }
}

[GoLocalName("T")] [GoType("ж<uintptr>")] internal partial class TestStructOfGC_T;

public static void TestStructOfGC(ж<Δtesting.T> Ꮡt) {
    var tt = TypeOf(((TestStructOfGC_T)nil));
    var fields = new Δreflect.StructField[]{
        new(Name: "X"u8, Type: tt),
        new(Name: "Y"u8, Type: tt)
    }.slice();
    var st = StructOf(fields);
    const nint n = 10000;
    slice<any> x = default!;
    for (nint i = 0; i < n; i++) {
        var v = New(st).Elem();
        for (nint j = 0; j < v.NumField(); j++) {
            var p = @new<uintptr>();
            p.Value = (uintptr)(i * n + j);
            v.Field(j).Set(ValueOf(p.OrTypedNil()).Convert(tt));
        }
        x = append(x, v.Interface());
    }
    Δruntime.GC();
    foreach (var (i, xi) in x) {
        var v = ValueOf(xi);
        for (nint j = 0; j < v.NumField(); j++) {
            var k = v.Field(j).Elem().Interface();
            if (!AreEqual(k, (uintptr)(i * n + j))) {
                Ꮡt.Errorf("lost x[%d].%c = %d, want %d"u8, i, LiteralByteAt("XY"u8, j), k, i * n + j);
            }
        }
    }
}

public static void TestStructOfAlg(ж<Δtesting.T> Ꮡt) {
    var st = StructOf(new Δreflect.StructField[]{new(Name: "X"u8, Tag: "x"u8, Type: TypeOf((nint)0))}.slice());
    ref var v1 = ref heap<reflectꓸValue>(out var Ꮡv1);
    v1 = New(st).Elem();
    var v2 = New(st).Elem();
    if (!DeepEqual(v1.Interface(), v1.Interface())) {
        Ꮡt.Errorf("constructed struct %v not equal to itself"u8, v1.Interface());
    }
    v1.FieldByName("X"u8).Set(ValueOf((nint)1));
    {
        var (i1, i2) = (v1.Interface(), v2.Interface()); if (DeepEqual(i1, i2)) {
            Ꮡt.Errorf("constructed structs %v and %v should not be equal"u8, i1, i2);
        }
    }
    st = StructOf(new Δreflect.StructField[]{new(Name: "X"u8, Tag: "x"u8, Type: TypeOf(slice<nint>(default!)))}.slice());
    v1 = New(st).Elem();
    var v1ʗ1 = v1;
    shouldPanic(""u8, () => {
        _ = AreEqual(v1ʗ1.Interface(), v1ʗ1.Interface());
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object defˢ = (@string)"def"u8;

[GoType("dyn")] internal partial struct TestStructOfGenericAlg_tests {
    internal reflectꓸType rt;
    internal slice<nint> idx;
}

public static void TestStructOfGenericAlg(ж<Δtesting.T> Ꮡt) {
    var st1 = StructOf(new Δreflect.StructField[]{
        new(Name: "X"u8, Tag: "x"u8, Type: TypeOf((int64)0)),
        new(Name: "Y"u8, Type: TypeOf(((@string)""u8)))
    }.slice());
    var st = StructOf(new Δreflect.StructField[]{
        new(Name: "S0"u8, Type: st1),
        new(Name: "S1"u8, Type: st1)
    }.slice());
    var tests = new TestStructOfGenericAlg_tests[]{
        new(
            rt: st,
            idx: new nint[]{0, 1}.slice()
        ),
        new(
            rt: st1,
            idx: new nint[]{1}.slice()
        ),
        new(
            rt: StructOf(
                new Δreflect.StructField[]{
                    new(Name: "XX"u8, Type: TypeOf(new nint[]{}.array())),
                    new(Name: "YY"u8, Type: TypeOf((@string)""u8))
                }.slice()),
            idx: new nint[]{1}.slice()
        ),
        new(
            rt: StructOf(
                new Δreflect.StructField[]{
                    new(Name: "XX"u8, Type: TypeOf(new nint[]{}.array())),
                    new(Name: "YY"u8, Type: TypeOf((@string)""u8)),
                    new(Name: "ZZ"u8, Type: TypeOf(new nint[]{}.array(2)))
                }.slice()),
            idx: new nint[]{1}.slice()
        ),
        new(
            rt: StructOf(
                new Δreflect.StructField[]{
                    new(Name: "XX"u8, Type: TypeOf(new nint[]{}.array(1))),
                    new(Name: "YY"u8, Type: TypeOf((@string)""u8))
                }.slice()),
            idx: new nint[]{1}.slice()
        ),
        new(
            rt: StructOf(
                new Δreflect.StructField[]{
                    new(Name: "XX"u8, Type: TypeOf(new nint[]{}.array(1))),
                    new(Name: "YY"u8, Type: TypeOf((@string)""u8)),
                    new(Name: "ZZ"u8, Type: TypeOf(new nint[]{}.array(1)))
                }.slice()),
            idx: new nint[]{1}.slice()
        ),
        new(
            rt: StructOf(
                new Δreflect.StructField[]{
                    new(Name: "XX"u8, Type: TypeOf(new nint[]{}.array(2))),
                    new(Name: "YY"u8, Type: TypeOf((@string)""u8)),
                    new(Name: "ZZ"u8, Type: TypeOf(new nint[]{}.array(2)))
                }.slice()),
            idx: new nint[]{1}.slice()
        ),
        new(
            rt: StructOf(
                new Δreflect.StructField[]{
                    new(Name: "XX"u8, Type: TypeOf((int64)0)),
                    new(Name: "YY"u8, Type: TypeOf((byte)0)),
                    new(Name: "ZZ"u8, Type: TypeOf((@string)""u8))
                }.slice()),
            idx: new nint[]{2}.slice()
        ),
        new(
            rt: StructOf(
                new Δreflect.StructField[]{
                    new(Name: "XX"u8, Type: TypeOf((int64)0)),
                    new(Name: "YY"u8, Type: TypeOf((int64)0)),
                    new(Name: "ZZ"u8, Type: TypeOf((@string)""u8)),
                    new(Name: "AA"u8, Type: TypeOf(new int64[]{}.array(1)))
                }.slice()),
            idx: new nint[]{2}.slice()
        )
    }.slice();
    foreach (var (_, table) in tests) {
        var v1 = New(table.rt).Elem();
        var v2 = New(table.rt).Elem();
        if (!DeepEqual(v1.Interface(), v1.Interface())) {
            Ꮡt.Errorf("constructed struct %v not equal to itself"u8, v1.Interface());
        }
        v1.FieldByIndex(table.idx).Set(ValueOf(abcˢ));
        v2.FieldByIndex(table.idx).Set(ValueOf(defˢ));
        {
            var (i1, i2) = (v1.Interface(), v2.Interface()); if (DeepEqual(i1, i2)) {
                Ꮡt.Errorf("constructed structs %v and %v should not be equal"u8, i1, i2);
            }
        }
        @string abc = abcˢ;
        v1.FieldByIndex(table.idx).Set(ValueOf(abc));
        @string val = "+"u8 + abc + "-"u8;
        v2.FieldByIndex(table.idx).Set(ValueOf(val[1..4]));
        {
            var (i1, i2) = (v1.Interface(), v2.Interface()); if (!DeepEqual(i1, i2)) {
                Ꮡt.Errorf("constructed structs %v and %v should be equal"u8, i1, i2);
            }
        }
        // Test hash
        var m = MakeMap(MapOf(table.rt, TypeOf((nint)0)));
        m.SetMapIndex(v1, ValueOf((nint)(1)));
        {
            var (i1, i2) = (v1.Interface(), v2.Interface()); if (!m.MapIndex(v2).IsValid()) {
                Ꮡt.Errorf("constructed structs %#v and %#v have different hashes"u8, i1, i2);
            }
        }
        v2.FieldByIndex(table.idx).Set(ValueOf(abcˢ));
        {
            var (i1, i2) = (v1.Interface(), v2.Interface()); if (!DeepEqual(i1, i2)) {
                Ꮡt.Errorf("constructed structs %v and %v should be equal"u8, i1, i2);
            }
        }
        {
            var (i1, i2) = (v1.Interface(), v2.Interface()); if (!m.MapIndex(v2).IsValid()) {
                Ꮡt.Errorf("constructed structs %v and %v have different hashes"u8, i1, i2);
            }
        }
    }
}

[GoType("dyn")] internal partial struct TestStructOfDirectIface_T {
    public array<ж<byte>> X = new(1);
}

[GoType("dyn")] internal partial struct TestStructOfDirectIface_Tᴛ1 {
    public array<ж<byte>> X = new(0);
}

public static void TestStructOfDirectIface(ж<Δtesting.T> Ꮡt) {
    {
        ref var i1 = ref heap<any>(out var Ꮡi1);
        i1 = Zero(TypeOf(new TestStructOfDirectIface_T(nil))).Interface();
        var v1 = ValueOf(Ꮡi1).Elem();
        var p1 = v1.InterfaceData()[1];
        ref var i2 = ref heap<any>(out var Ꮡi2);
        i2 = Zero(StructOf(new Δreflect.StructField[]{
            new(
                Name: "X"u8,
                Type: ArrayOf(1, TypeOf(((ж<int8>)nil)))
            )
        }.slice())).Interface();
        var v2 = ValueOf(Ꮡi2).Elem();
        var p2 = v2.InterfaceData()[1];
        if (p1 != 0) {
            Ꮡt.Errorf("got p1=%v. want=%v"u8, p1, (any)(default!));
        }
        if (p2 != 0) {
            Ꮡt.Errorf("got p2=%v. want=%v"u8, p2, (any)(default!));
        }
    }
    {
        ref var i1 = ref heap<any>(out var Ꮡi1);
        i1 = Zero(TypeOf(new TestStructOfDirectIface_Tᴛ1(nil))).Interface();
        var v1 = ValueOf(Ꮡi1).Elem();
        var p1 = v1.InterfaceData()[1];
        ref var i2 = ref heap<any>(out var Ꮡi2);
        i2 = Zero(StructOf(new Δreflect.StructField[]{
            new(
                Name: "X"u8,
                Type: ArrayOf(0, TypeOf(((ж<int8>)nil)))
            )
        }.slice())).Interface();
        var v2 = ValueOf(Ꮡi2).Elem();
        var p2 = v2.InterfaceData()[1];
        if (p1 == 0) {
            Ꮡt.Errorf("got p1=%v. want=not-%v"u8, p1, (any)(default!));
        }
        if (p2 == 0) {
            Ꮡt.Errorf("got p2=%v. want=not-%v"u8, p2, (any)(default!));
        }
    }
}

[GoType("num:nint")] partial struct StructI;

public static nint Get(this StructI i) {
    return (nint)i;
}

[GoType("num:nint")] partial struct StructIPtr;

[GoRecv] public static nint Get(this ref StructIPtr i) {
    return (nint)(i);
}

public static void Set(this ж<StructIPtr> Ꮡi, nint v) {
    (Ꮡi.Reinterpret<StructIPtr, nint>()).Value = v;
}

[GoType] partial struct SettableStruct {
    public nint SettableField;
}

[GoRecv] public static void Set(this ref SettableStruct p, nint v) {
    p.SettableField = v;
}

[GoType] partial struct SettablePointer {
    public ж<nint> SettableField;
}

[GoRecv] public static void Set(this ref SettablePointer p, nint v) {
    p.SettableField.Value = v;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string getˢ = "Get"u8;

[GoType("dyn")] internal partial interface TestStructOfWithInterface_Iface {
    nint Get();
}

[GoType("dyn")] internal partial interface TestStructOfWithInterface_IfaceSet {
    void Set(nint _Δp0);
}

[GoType("dyn")] internal partial struct TestStructOfWithInterface_tests {
    internal @string name;
    internal reflectꓸType typ;
    internal reflectꓸValue val;
    internal bool impl;
}

public static void TestStructOfWithInterface(ж<Δtesting.T> Ꮡt) {
    UntypedInt want = 42;
    var tests = new TestStructOfWithInterface_tests[]{
        new(
            name: "StructI"u8,
            typ: TypeOf(((StructI)want)),
            val: ValueOf(((StructI)want)),
            impl: true
        ),
        new(
            name: "StructI"u8,
            typ: PointerTo(TypeOf(((StructI)want))),
            val: ValueOf(((Func<any>)(() => {
                ref var v = ref heap<StructI>(out var Ꮡv);
                v = ((StructI)want);
                return Ꮡv;
            }))()),
            impl: true
        ),
        new(
            name: "StructIPtr"u8,
            typ: PointerTo(TypeOf(((StructIPtr)want))),
            val: ValueOf(((Func<any>)(() => {
                ref var v = ref heap<StructIPtr>(out var Ꮡv);
                v = ((StructIPtr)want);
                return Ꮡv;
            }))()),
            impl: true
        ),
        new(
            name: "StructIPtr"u8,
            typ: TypeOf(((StructIPtr)want)),
            val: ValueOf(((StructIPtr)want)),
            impl: false
        )
    }.slice();
    // {
    //	typ:  TypeOf((*Iface)(nil)).Elem(), // FIXME(sbinet): fix method.ifn/tfn
    //	val:  ValueOf(StructI(want)),
    //	impl: true,
    // },
    foreach (var (i, table) in tests) {
        for (nint jᴛ1 = 0; jᴛ1 < 2; jᴛ1++) {
            var j = jᴛ1;
            ref var fieldsΔ1 = ref heap<slice<Δreflect.StructField>>(out var ᏑfieldsΔ1);
            if (j == 1) {
                fieldsΔ1 = append(fieldsΔ1, new StructField(
                    Name: "Dummy"u8,
                    PkgPath: ""u8,
                    Type: TypeOf((nint)0)
                ));
            }
            fieldsΔ1 = append(fieldsΔ1, new StructField(
                Name: table.name,
                Anonymous: true,
                PkgPath: ""u8,
                Type: table.typ
            ));
            // We currently do not correctly implement methods
            // for embedded fields other than the first.
            // Therefore, for now, we expect those methods
            // to not exist.  See issues 15924 and 20824.
            // When those issues are fixed, this test of panic
            // should be removed.
            if (j == 1 && table.impl) {
                ((Action)(() => {
                    GoFrame ᒐ = default;
                    try {
                        defer(() => {
                            {
                                var err = recover(); if (err == default!) {
                                    Ꮡt.Errorf("test-%d-%d did not panic"u8, i, j);
                                }
                            }
                        }, ref ᒐ);
                        _ = StructOf(ᏑfieldsΔ1.ValueSlot);
                    }
                    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                    finally { ᒐ.Run(); }
                }))();
                continue;
            }
            var rtΔ1 = StructOf(fieldsΔ1);
            var rvΔ1 = New(rtΔ1).Elem();
            rvΔ1.Field(j).Set(table.val);
            {
                var (_, ok) = rvΔ1.Interface()._<TestStructOfWithInterface_Iface>(ᐧ); if (ok != table.impl) {
                    if (table.impl){
                        Ꮡt.Errorf("test-%d-%d: type=%v fails to implement Iface.\n"u8, i, j, table.typ);
                    } else {
                        Ꮡt.Errorf("test-%d-%d: type=%v should NOT implement Iface\n"u8, i, j, table.typ);
                    }
                    continue;
                }
            }
            if (!table.impl) {
                continue;
            }
            nint v = rvΔ1.Interface()._<TestStructOfWithInterface_Iface>().Get();
            if (v != want) {
                Ꮡt.Errorf("test-%d-%d: x.Get()=%v. want=%v\n"u8, i, j, v, (nint)(want));
            }
            var fct = rvΔ1.MethodByName(getˢ);
            var @out = fct.Call(default!);
            if (!DeepEqual(@out[0].Interface(), (nint)(want))) {
                Ꮡt.Errorf("test-%d-%d: x.Get()=%v. want=%v\n"u8, i, j, @out[0].Interface(), (nint)(want));
            }
        }
    }
    // Test an embedded nil pointer with pointer methods.
    ref var fields = ref heap<slice<Δreflect.StructField>>(out var Ꮡfields);
    fields = new Δreflect.StructField[]{new(
        Name: "StructIPtr"u8,
        Anonymous: true,
        Type: PointerTo(TypeOf(((StructIPtr)want)))
    )
    }.slice();
    var rt = StructOf(fields);
    ref var rv = ref heap<reflectꓸValue>(out var Ꮡrv);
    rv = New(rt).Elem();
    // This should panic since the pointer is nil.
    shouldPanic(""u8, () => {
        Ꮡrv.Value.Interface()._<TestStructOfWithInterface_IfaceSet>().Set(want);
    });
    // Test an embedded nil pointer to a struct with pointer methods.
    fields = new Δreflect.StructField[]{new(
        Name: "SettableStruct"u8,
        Anonymous: true,
        Type: PointerTo(TypeOf(new SettableStruct(nil)))
    )
    }.slice();
    rt = StructOf(fields);
    rv = New(rt).Elem();
    // This should panic since the pointer is nil.
    shouldPanic(""u8, () => {
        Ꮡrv.Value.Interface()._<TestStructOfWithInterface_IfaceSet>().Set(want);
    });
    // The behavior is different if there is a second field,
    // since now an interface value holds a pointer to the struct
    // rather than just holding a copy of the struct.
    fields = new Δreflect.StructField[]{
        new(
            Name: "SettableStruct"u8,
            Anonymous: true,
            Type: PointerTo(TypeOf(new SettableStruct(nil)))
        ),
        new(
            Name: "EmptyStruct"u8,
            Anonymous: true,
            Type: StructOf(default!)
        )
    }.slice();
    // With the current implementation this is expected to panic.
    // Ideally it should work and we should be able to see a panic
    // if we call the Set method.
    shouldPanic(""u8, () => {
        StructOf(Ꮡfields.ValueSlot);
    });
    // Embed a field that can be stored directly in an interface,
    // with a second field.
    fields = new Δreflect.StructField[]{
        new(
            Name: "SettablePointer"u8,
            Anonymous: true,
            Type: TypeOf(new SettablePointer(nil))
        ),
        new(
            Name: "EmptyStruct"u8,
            Anonymous: true,
            Type: StructOf(default!)
        )
    }.slice();
    // With the current implementation this is expected to panic.
    // Ideally it should work and we should be able to call the
    // Set and Get methods.
    shouldPanic(""u8, () => {
        StructOf(Ꮡfields.ValueSlot);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string afterˢ = "After"u8;

public static void TestStructOfTooManyFields(ж<Δtesting.T> Ꮡt) {
    // Bug Fix: #25402 - this should not panic
    var tt = StructOf(new Δreflect.StructField[]{
        new(Name: "Time"u8, Type: TypeOf(new time.Time(nil)), Anonymous: true)
    }.slice());
    {
        var (_, present) = tt.MethodByName(afterˢ); if (!present) {
            Ꮡt.Errorf("Expected method `After` to be found"u8);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string differentPkgPathˢ = "different PkgPath"u8;

public static void TestStructOfDifferentPkgPath(ж<Δtesting.T> Ꮡt) {
    var fields = new Δreflect.StructField[]{
        new(
            Name: "f1"u8,
            PkgPath: "p1"u8,
            Type: TypeOf((nint)0)
        ),
        new(
            Name: "f2"u8,
            PkgPath: "p2"u8,
            Type: TypeOf((nint)0)
        )
    }.slice();
    var fieldsʗ1 = fields;
    shouldPanic(differentPkgPathˢ, () => {
        StructOf(fieldsʗ1);
    });
}

[GoType("dyn")] internal partial struct TestStructOfTooLarge_test {
    internal bool shouldPanic;
    internal slice<Δreflect.StructField> fields;
}

public static void TestStructOfTooLarge(ж<Δtesting.T> Ꮡt) {
    var t1 = TypeOf((byte)0);
    var t2 = TypeOf((int16)0);
    var t4 = TypeOf((int32)0);
    var t0 = ArrayOf(0, t1);
    // 2^64-3 sized type (or 2^32-3 on 32-bit archs)
    var bigType = StructOf(new Δreflect.StructField[]{
        new(Name: "F1"u8, Type: ArrayOf((nint)((~(uintptr)0 >> (int)(1))), t1)),
        new(Name: "F2"u8, Type: ArrayOf((nint)((~(uintptr)0 >> (int)(1)) - 1), t1))
    }.slice());
    var tests = new TestStructOfTooLarge_test[]{
        new(
            shouldPanic: false, // 2^64-1, ok

            fields: new Δreflect.StructField[]{
                new(Name: "F1"u8, Type: bigType),
                new(Name: "F2"u8, Type: ArrayOf(2, t1))
            }.slice()
        ),
        new(
            shouldPanic: true, // overflow in total size

            fields: new Δreflect.StructField[]{
                new(Name: "F1"u8, Type: bigType),
                new(Name: "F2"u8, Type: ArrayOf(3, t1))
            }.slice()
        ),
        new(
            shouldPanic: true, // overflow while aligning F2

            fields: new Δreflect.StructField[]{
                new(Name: "F1"u8, Type: bigType),
                new(Name: "F2"u8, Type: t4)
            }.slice()
        ),
        new(
            shouldPanic: true, // overflow while adding trailing byte for zero-sized fields

            fields: new Δreflect.StructField[]{
                new(Name: "F1"u8, Type: bigType),
                new(Name: "F2"u8, Type: ArrayOf(2, t1)),
                new(Name: "F3"u8, Type: t0)
            }.slice()
        ),
        new(
            shouldPanic: true, // overflow while aligning total size

            fields: new Δreflect.StructField[]{
                new(Name: "F1"u8, Type: t2),
                new(Name: "F2"u8, Type: bigType)
            }.slice()
        )
    }.array();
    foreach (var (i, vᴛ1) in tests.ΔRangeSnapshot()) {
        ref var tt = ref heap(new TestStructOfTooLarge_test(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        ((Action)(() => {
            GoFrame ᒐ = default;
            try {
                var ttʗ2 = ttʗ1;
                defer(() => {
                    var err = recover();
                    if (!ttʗ2.shouldPanic) {
                        if (err != default!) {
                            Ꮡt.Errorf("test %d should not panic, got %s"u8, i, err);
                        }
                        return;
                    }
                    if (err == default!) {
                        Ꮡt.Errorf("test %d expected to panic"u8, i);
                        return;
                    }
                    @string s = fmt.Sprintf("%s"u8, err);
                    if (s != "reflect.StructOf: struct size would exceed virtual address space"u8) {
                        Ꮡt.Errorf("test %d wrong panic message: %s"u8, i, s);
                        return;
                    }
                }, ref ᒐ);
                _ = StructOf(ttʗ1.fields);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }))();
    }
}

[GoType("dyn")] internal partial struct TestStructOfAnonymous_type {
    public partial ref D1 D1 { get; }
}

public static void TestStructOfAnonymous(ж<Δtesting.T> Ꮡt) {
    any s = new TestStructOfAnonymous_type();
    var f = TypeOf(s).Field(0);
    var ds = StructOf(new Δreflect.StructField[]{f}.slice());
    var st = TypeOf(s);
    var dt = New(ds).Elem();
    if (!AreEqual(st, dt.Type())) {
        Ꮡt.Errorf("StructOf returned %s, want %s"u8, dt.Type(), st);
    }
    // This should not panic.
    _ = dt.Interface()._<TestStructOfAnonymous_type>();
}

[GoLocalName("T")] [GoType("@string")] internal partial struct TestChanOf_T;

[GoLocalName("T1")] [GoType("num:nint")] internal partial struct TestChanOf_T1;

public static void TestChanOf(ж<Δtesting.T> Ꮡt) {
    var ct = ChanOf(BothDir, TypeOf(((TestChanOf_T)(@string)""u8)));
    var v = MakeChan(ct, 2);
    Δruntime.GC();
    v.Send(ValueOf(((TestChanOf_T)(@string)helloˢ)));
    Δruntime.GC();
    v.Send(ValueOf(((TestChanOf_T)(@string)worldˢ)));
    Δruntime.GC();
    var (sv1, _) = v.Recv();
    var (sv2, _) = v.Recv();
    @string s1 = sv1.String();
    @string s2 = sv2.String();
    if (s1 != "hello"u8 || s2 != "world"u8) {
        Ꮡt.Errorf("constructed chan: have %q, %q, want %q, %q"u8, s1, s2, helloˢ, worldˢ);
    }
    checkSameType(Ꮡt, ChanOf(BothDir, TypeOf(((TestChanOf_T1)1))), (channel<TestChanOf_T1>)(default!));
    // Check arrow token association in undefined chan types.
    channel/*<-*/<channel<TestChanOf_T>> left = channel/*<-*/<channel<TestChanOf_T>>.SendOnly;
    channel</*<-*/channel<TestChanOf_T>> right = channel</*<-*/channel<TestChanOf_T>>.Nil(ChanCargo.Of(new GoChanDir[] { GoChanDir.Both, GoChanDir.Recv }, null));
    var tLeft = ChanOf(SendDir, ChanOf(BothDir, TypeOf(((TestChanOf_T)(@string)""u8))));
    var tRight = ChanOf(BothDir, ChanOf(RecvDir, TypeOf(((TestChanOf_T)(@string)""u8))));
    if (!AreEqual(tLeft, TypeOf(left))) {
        Ꮡt.Errorf("chan<-chan: have %s, want %T"u8, tLeft, left);
    }
    if (!AreEqual(tRight, TypeOf(right))) {
        Ꮡt.Errorf("chan<-chan: have %s, want %T"u8, tRight, right);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object chanˢ = (@string)"<-chan"u8;
internal static readonly object chanˢ2 = (@string)"chan<-"u8;

[GoLocalName("T")] [GoType("@string")] internal partial struct TestChanOfDir_T;

[GoLocalName("T1")] [GoType("num:nint")] internal partial struct TestChanOfDir_T1;

public static void TestChanOfDir(ж<Δtesting.T> Ꮡt) {
    var crt = ChanOf(RecvDir, TypeOf(((TestChanOfDir_T)(@string)""u8)));
    var cst = ChanOf(SendDir, TypeOf(((TestChanOfDir_T)(@string)""u8)));
    checkSameType(Ꮡt, ChanOf(RecvDir, TypeOf(((TestChanOfDir_T1)1))), /*<-*/channel<TestChanOfDir_T1>.RecvOnly);
    checkSameType(Ꮡt, ChanOf(SendDir, TypeOf(((TestChanOfDir_T1)1))), channel/*<-*/<TestChanOfDir_T1>.SendOnly);
    // check String form of ChanDir
    if (crt.ChanDir().String() != "<-chan"u8) {
        Ꮡt.Errorf("chan dir: have %q, want %q"u8, crt.ChanDir().String(), chanˢ);
    }
    if (cst.ChanDir().String() != "chan<-"u8) {
        Ꮡt.Errorf("chan dir: have %q, want %q"u8, cst.ChanDir().String(), chanˢ2);
    }
}

[GoLocalName("T")] [GoType("ж<uintptr>")] internal partial class TestChanOfGC_T;

[MethodImpl(MethodImplOptions.NoInlining)] public static void TestChanOfGC(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var done = new channel<bool>(1);
        var doneʗ1 = done;
        goǃ(() => {
            var selᴛ3 = doneʗ1;
            var selᴛ4 = time.After((time.Duration)(5000000000L));
            switch (select(ᐸꟷ(selᴛ3, ꓸꓸꓸ), ᐸꟷ(selᴛ4, ꓸꓸꓸ))) {
            case 0 when selᴛ3.ꟷᐳ(out _): {
                break;
            }
            case 1 when selᴛ4.ꟷᐳ(out _): {
                throw panic("deadlock in TestChanOfGC");
                break;
            }}
        });
        var doneʗ2 = done;
        defer(() => {
            doneʗ2.ᐸꟷ(true);
        }, ref ᒐ);
        var tt = TypeOf(((TestChanOfGC_T)nil));
        var ct = ChanOf(BothDir, tt);
        // NOTE: The garbage collector handles allocated channels specially,
        // so we have to save pointers to channels in x; the pointer code will
        // use the gc info in the newly constructed chan type.
        const nint n = 100;
        slice<any> x = default!;
        for (nint i = 0; i < n; i++) {
            var v = MakeChan(ct, n);
            for (nint j = 0; j < n; j++) {
                var p = @new<uintptr>();
                p.Value = (uintptr)(i * n + j);
                v.Send(ValueOf(p.OrTypedNil()).Convert(tt));
            }
            var pv = New(ct);
            pv.Elem().Set(v);
            x = append(x, pv.Interface());
        }
        Δruntime.GC();
        foreach (var (i, xi) in x) {
            var v = ValueOf(xi).Elem();
            for (nint j = 0; j < n; j++) {
                var (pv, _) = v.Recv();
                var k = pv.Elem().Interface();
                if (!AreEqual(k, (uintptr)(i * n + j))) {
                    Ꮡt.Errorf("lost x[%d][%d] = %d, want %d"u8, i, j, k, i * n + j);
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string mapA1ˢ = "map[a:1]"u8;
internal static readonly @string invalidKeyTypeˢ = "invalid key type"u8;

[GoLocalName("K")] [GoType("@string")] internal partial struct TestMapOf_K;

[GoLocalName("V")] [GoType("num:float64")] internal partial struct TestMapOf_V;

public static void TestMapOf(ж<Δtesting.T> Ꮡt) {
    var v = MakeMap(MapOf(TypeOf(((TestMapOf_K)(@string)""u8)), TypeOf(((TestMapOf_V)0D))));
    Δruntime.GC();
    v.SetMapIndex(ValueOf(((TestMapOf_K)(@string)"a"u8)), ValueOf(((TestMapOf_V)1D)));
    Δruntime.GC();
    @string s = fmt.Sprint(v.Interface());
    @string want = mapA1ˢ;
    if (s != want) {
        Ꮡt.Errorf("constructed map = %s, want %s"u8, s, want);
    }
    // check that type already in binary is found
    checkSameType(Ꮡt, MapOf(TypeOf(((TestMapOf_V)0D)), TypeOf(((TestMapOf_K)(@string)""u8))), ((map<TestMapOf_V, TestMapOf_K>)default!));
    // check that invalid key type panics
    shouldPanic(invalidKeyTypeˢ, () => {
        MapOf(TypeOf(((Action)(default!)).OrTypedNilFunc()), TypeOf(false));
    });
}

[GoLocalName("T")] [GoType("ж<uintptr>")] internal partial class TestMapOfGCKeys_T;

public static void TestMapOfGCKeys(ж<Δtesting.T> Ꮡt) {
    var tt = TypeOf(((TestMapOfGCKeys_T)nil));
    var mt = MapOf(tt, TypeOf(false));
    // NOTE: The garbage collector handles allocated maps specially,
    // so we have to save pointers to maps in x; the pointer code will
    // use the gc info in the newly constructed map type.
    const nint n = 100;
    slice<any> x = default!;
    for (nint i = 0; i < n; i++) {
        var v = MakeMap(mt);
        for (nint j = 0; j < n; j++) {
            var p = @new<uintptr>();
            p.Value = (uintptr)(i * n + j);
            v.SetMapIndex(ValueOf(p.OrTypedNil()).Convert(tt), ValueOf(true));
        }
        var pv = New(mt);
        pv.Elem().Set(v);
        x = append(x, pv.Interface());
    }
    Δruntime.GC();
    foreach (var (i, xi) in x) {
        var v = ValueOf(xi).Elem();
        slice<nint> @out = default!;
        foreach (var (_, kv) in v.MapKeys()) {
            @out = append(@out, (nint)(kv.Elem().Interface()._<uintptr>()));
        }
        slices.Sort<slice<nint>, nint>(@out);
        foreach (var (j, k) in @out) {
            if (k != i * n + j) {
                Ꮡt.Errorf("lost x[%d][%d] = %d, want %d"u8, i, j, k, i * n + j);
            }
        }
    }
}

[GoType("dyn")] internal partial struct TestMapOfGCBigKey_KV {
    internal int64 i;
    internal int64 j;
}

// Test assignment and access to a map with keys larger than word size.
public static void TestMapOfGCBigKey(ж<Δtesting.T> Ꮡt) {
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
            Ꮡt.Errorf("lost m[%v] = %v, want %v"u8, kv, elem, kv);
        }
    }
}

[GoLocalName("T")] [GoType("ж<uintptr>")] internal partial class TestMapOfGCValues_T;

public static void TestMapOfGCValues(ж<Δtesting.T> Ꮡt) {
    var tt = TypeOf(((TestMapOfGCValues_T)nil));
    var mt = MapOf(TypeOf((nint)(1)), tt);
    // NOTE: The garbage collector handles allocated maps specially,
    // so we have to save pointers to maps in x; the pointer code will
    // use the gc info in the newly constructed map type.
    const nint n = 100;
    slice<any> x = default!;
    for (nint i = 0; i < n; i++) {
        var v = MakeMap(mt);
        for (nint j = 0; j < n; j++) {
            var p = @new<uintptr>();
            p.Value = (uintptr)(i * n + j);
            v.SetMapIndex(ValueOf(j), ValueOf(p.OrTypedNil()).Convert(tt));
        }
        var pv = New(mt);
        pv.Elem().Set(v);
        x = append(x, pv.Interface());
    }
    Δruntime.GC();
    foreach (var (i, xi) in x) {
        var v = ValueOf(xi).Elem();
        for (nint j = 0; j < n; j++) {
            var k = v.MapIndex(ValueOf(j)).Elem().Interface()._<uintptr>();
            if (k != (uintptr)(i * n + j)) {
                Ꮡt.Errorf("lost x[%d][%d] = %d, want %d"u8, i, j, k, i * n + j);
            }
        }
    }
}

public static void TestTypelinksSorted(ж<Δtesting.T> Ꮡt) {
    @string last = default!;
    foreach (var (i, n) in reflect_internal_test_package.TypeLinks()) {
        if (n < last) {
            Ꮡt.Errorf("typelinks not sorted: %q [%d] > %q [%d]"u8, last, i - 1, n, i);
        }
        last = n;
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gopherˢ = "gopher"u8;
internal static readonly @string mustBeSliceˢ = "must be slice"u8;

[GoLocalName("K")] [GoType("@string")] internal partial struct TestFuncOf_K;

[GoLocalName("V")] [GoType("num:float64")] internal partial struct TestFuncOf_V;

[GoLocalName("T1")] [GoType("num:nint")] internal partial struct TestFuncOf_T1;

[GoType("dyn")] internal partial struct TestFuncOf_testCases {
    internal slice<reflectꓸType> @in, @out;
    internal bool variadic;
    internal any want;
}

public static void TestFuncOf(ж<Δtesting.T> Ꮡt) {
    var fn = (slice<reflectꓸValue> args) => {
        if (len(args) != 1){
            Ꮡt.Errorf("args == %v, want exactly one arg"u8, args);
        } else 
        if (!AreEqual(args[0].Type(), TypeOf(((TestFuncOf_K)(@string)""u8)))){
            Ꮡt.Errorf("args[0] is type %v, want %v"u8, args[0].Type(), TypeOf(((TestFuncOf_K)(@string)""u8)));
        } else 
        if (args[0].String() != "gopher"u8) {
            Ꮡt.Errorf("args[0] = %q, want %q"u8, args[0].String(), gopherˢ);
        }
        return new reflectꓸValue[]{ValueOf(((TestFuncOf_V)3.14D))}.slice();
    };
    var v = MakeFunc(FuncOf(new reflectꓸType[]{TypeOf(((TestFuncOf_K)(@string)""u8))}.slice(), new reflectꓸType[]{TypeOf(((TestFuncOf_V)0D))}.slice(), false), fn);
    var outs = v.Call(new reflectꓸValue[]{ValueOf(((TestFuncOf_K)(@string)gopherˢ))}.slice());
    if (len(outs) != 1){
        Ꮡt.Fatalf("v.Call returned %v, want exactly one result"u8, outs);
    } else 
    if (!AreEqual(outs[0].Type(), TypeOf(((TestFuncOf_V)0D)))) {
        Ꮡt.Fatalf("c.Call[0] is type %v, want %v"u8, outs[0].Type(), TypeOf(((TestFuncOf_V)0D)));
    }
    var f = outs[0].Float();
    if (f != 3.14D) {
        Ꮡt.Errorf("constructed func returned %f, want %f"u8, f, 3.14D);
    }
    var testCases = new TestFuncOf_testCases[]{
        new(@in: new reflectꓸType[]{TypeOf(((TestFuncOf_T1)0))}.slice(), want: ((Action<TestFuncOf_T1>)(default!)).OrTypedNilFunc()),
        new(@in: new reflectꓸType[]{TypeOf((nint)0)}.slice(), want: ((Action<nint>)(default!)).OrTypedNilFunc()),
        new(@in: new reflectꓸType[]{SliceOf(TypeOf((nint)0))}.slice(), variadic: true, want: (((Actionꓸꓸꓸ<nint>)((Actionꓸꓸꓸ<nint>)(default!)))).OrTypedNilFunc()),
        new(@in: new reflectꓸType[]{TypeOf((nint)0)}.slice(), @out: new reflectꓸType[]{TypeOf(false)}.slice(), want: ((Func<nint, bool>)(default!)).OrTypedNilFunc()),
        new(@in: new reflectꓸType[]{TypeOf((nint)0)}.slice(), @out: new reflectꓸType[]{TypeOf(false), TypeOf((@string)""u8)}.slice(), want: ((Func<nint, (bool, @string)>)(default!)).OrTypedNilFunc())
    }.slice();
    foreach (var (_, tt) in testCases) {
        checkSameType(Ꮡt, FuncOf(tt.@in, tt.@out, tt.variadic), tt.want);
    }
    // check that variadic requires last element be a slice.
    FuncOf(new reflectꓸType[]{TypeOf((nint)(1)), TypeOf((@string)""u8), SliceOf(TypeOf(false))}.slice(), default!, true);
    shouldPanic(mustBeSliceˢ, () => {
        FuncOf(new reflectꓸType[]{TypeOf((nint)(0)), TypeOf((@string)""u8), TypeOf(false)}.slice(), default!, true);
    });
    shouldPanic(mustBeSliceˢ, () => {
        FuncOf(default!, default!, true);
    });
    //testcase for  #54669
    slice<reflectꓸType> @in = default!;
    for (nint i = 0; i < 51; i++) {
        @in = append(@in, TypeOf((nint)(1)));
    }
    FuncOf(@in, default!, false);
}

[GoType] partial struct R0 {
    public partial ref ж<R1> R1 { get; }
    public partial ref ж<R2> R2 { get; }
    public partial ref ж<R3> R3 { get; }
    public partial ref ж<R4> R4 { get; }
}

[GoType] partial struct R1 {
    public partial ref ж<R5> R5 { get; }
    public partial ref ж<R6> R6 { get; }
    public partial ref ж<R7> R7 { get; }
    public partial ref ж<R8> R8 { get; }
}

[GoType("R1")] partial struct R2;

[GoType("R1")] partial struct R3;

[GoType("R1")] partial struct R4;

[GoType] partial struct R5 {
    public partial ref ж<R9> R9 { get; }
    public partial ref ж<R10> R10 { get; }
    public partial ref ж<R11> R11 { get; }
    public partial ref ж<R12> R12 { get; }
}

[GoType("R5")] partial struct R6;

[GoType("R5")] partial struct R7;

[GoType("R5")] partial struct R8;

[GoType] partial struct R9 {
    public partial ref ж<R13> R13 { get; }
    public partial ref ж<R14> R14 { get; }
    public partial ref ж<R15> R15 { get; }
    public partial ref ж<R16> R16 { get; }
}

[GoType("R9")] partial struct R10;

[GoType("R9")] partial struct R11;

[GoType("R9")] partial struct R12;

[GoType] partial struct R13 {
    public partial ref ж<R17> R17 { get; }
    public partial ref ж<R18> R18 { get; }
    public partial ref ж<R19> R19 { get; }
    public partial ref ж<R20> R20 { get; }
}

[GoType("R13")] partial struct R14;

[GoType("R13")] partial struct R15;

[GoType("R13")] partial struct R16;

[GoType] partial struct R17 {
    public partial ref ж<R21> R21 { get; }
    public partial ref ж<R22> R22 { get; }
    public partial ref ж<R23> R23 { get; }
    public partial ref ж<R24> R24 { get; }
}

[GoType("R17")] partial struct R18;

[GoType("R17")] partial struct R19;

[GoType("R17")] partial struct R20;

[GoType] partial struct R21 {
    public nint X;
}

[GoType("R21")] partial struct R22;

[GoType("R21")] partial struct R23;

[GoType("R21")] partial struct R24;

public static void TestEmbed(ж<Δtesting.T> Ꮡt) {
    var typ = TypeOf(new R0(nil));
    var (f, ok) = typ.FieldByName("X"u8);
    if (ok) {
        Ꮡt.Fatalf(@"FieldByName(""X"") should fail, returned %v"u8, f.Index);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object allocsˢ = (@string)"allocs:"u8;

public static void TestAllocsInterfaceBig(ж<Δtesting.T> Ꮡt) {
    if (Δtesting.Short()) {
        Ꮡt.Skip(skippingMallocCountInˢ);
    }
    ref var v = ref heap<reflectꓸValue>(out var Ꮡv);
    v = ValueOf(new S(nil));
    {
        var vʗ1 = v;
        var allocs = Δtesting.AllocsPerRun(100, () => {
            vʗ1.Interface();
        }); if (allocs > 0D) {
            Ꮡt.Error(allocsˢ, allocs);
        }
    }
}

public static void TestAllocsInterfaceSmall(ж<Δtesting.T> Ꮡt) {
    if (Δtesting.Short()) {
        Ꮡt.Skip(skippingMallocCountInˢ);
    }
    ref var v = ref heap<reflectꓸValue>(out var Ꮡv);
    v = ValueOf((int64)0);
    {
        var vʗ1 = v;
        var allocs = Δtesting.AllocsPerRun(100, () => {
            vʗ1.Interface();
        }); if (allocs > 0D) {
            Ꮡt.Error(allocsˢ, allocs);
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
[GoType] partial struct exhaustive {
    internal ж<rand.Rand> r;
    internal nint pos;
    internal slice<choice> last;
}

[GoType] partial struct choice {
    internal nint off;
    internal nint n;
    internal nint max;
}

[GoRecv] internal static bool Next(this ref exhaustive x) {
    if (x.r == nil) {
        x.r = rand.New(rand.NewSource(time.Now().UnixNano()));
    }
    x.pos = 0;
    if (x.last == default!) {
        x.last = new choice[]{}.slice();
        return true;
    }
    for (nint i = len(x.last) - 1; i >= 0; i--) {
        var c = Ꮡ(x.last, i);
        if ((~c).n + 1 < (~c).max) {
            c.Value.n++;
            x.last = x.last.slice(0, i + 1);
            return true;
        }
    }
    return false;
}

[GoRecv] internal static nint Choose(this ref exhaustive x, nint max) {
    if (x.pos >= len(x.last)) {
        x.last = append(x.last, new choice(x.r.Intn(max), 0, max));
    }
    var c = Ꮡ(x.last, x.pos);
    x.pos++;
    if ((~c).max != max) {
        throw panic("inconsistent use of exhaustive tester");
    }
    return rem(((~c).n + (~c).off), max);
}

[GoRecv] internal static bool Maybe(this ref exhaustive x) {
    return x.Choose(2) == 1;
}

public static slice<reflectꓸValue> GCFunc(slice<reflectꓸValue> args) {
    Δruntime.GC();
    return new reflectꓸValue[]{}.slice();
}

public static void TestReflectFuncTraceback(ж<Δtesting.T> Ꮡt) {
    var f = MakeFunc(TypeOf(() => {
    }), GCFunc);
    f.Call(new reflectꓸValue[]{}.slice());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gcMethodˢ = "GCMethod"u8;

public static void TestReflectMethodTraceback(ж<Δtesting.T> Ꮡt) {
    var p = new Point(3, 4);
    var m = ValueOf(p).MethodByName(gcMethodˢ);
    var i = ValueOf(m.Interface()).Call(new reflectꓸValue[]{ValueOf((nint)(5))}.slice())[0].Int();
    if (i != 8) {
        Ꮡt.Errorf("Call returned %d; want 8"u8, i);
    }
}

[GoLocalName("T")] [GoType("[10]byte")] internal partial struct TestSmallZero_T;

public static void TestSmallZero(ж<Δtesting.T> Ꮡt) {
    var typ = TypeOf(new TestSmallZero_T(new byte[10].array()));
    {
        var typʗ1 = typ;
        var allocs = Δtesting.AllocsPerRun(100, () => {
            Zero(typʗ1);
        }); if (allocs > 0D) {
            Ꮡt.Errorf("Creating small zero values caused %f allocs, want 0"u8, allocs);
        }
    }
}

public static void TestBigZero(ж<Δtesting.T> Ꮡt) {
    UntypedInt size = /* 1 << 10 */ 1024;
    array<byte> v = new(1024); /* size */
    var z = Zero(ValueOf(v).Type()).Interface()._<array<byte>>();
    for (nint i = 0; i < size; i++) {
        if (z[i] != 0) {
            Ꮡt.Fatalf("Zero object not all zero, index %d"u8, i);
        }
    }
}

[GoLocalName("T")] [GoType("[16]byte")] internal partial struct TestZeroSet_T;

[GoType("dyn")] internal partial struct TestZeroSet_S {
    internal uint64 a;
    public TestZeroSet_T T;
    internal uint64 b;
}

public static void TestZeroSet(ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    ref var v = ref heap<TestZeroSet_S>(out var Ꮡv);
    v = new TestZeroSet_S(
        a: 0xaaaaaaaaaaaaaaaaUL,
        T: new TestZeroSet_T(new byte[]{9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9}.array()),
        b: 0xbbbbbbbbbbbbbbbbUL
    );
    ValueOf(Ꮡv).Elem().Field(1).Set(Zero(TypeOf(new TestZeroSet_T(new byte[16].array()))));
    if (v != (new TestZeroSet_S(
        a: 0xaaaaaaaaaaaaaaaaUL,
        b: 0xbbbbbbbbbbbbbbbbUL
    ))) {
        Ꮡt.Fatalf("Setting a field to a Zero value didn't work"u8);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string nilPointerToEmbeddedˢ = "nil pointer to embedded struct"u8;

[GoType("dyn")] internal partial struct TestFieldByIndexNil_P {
    public nint F;
}

[GoType("dyn")] internal partial struct TestFieldByIndexNil_T {
    public partial ref ж<TestFieldByIndexNil_P> P { get; }
}

public static void TestFieldByIndexNil(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var v = ValueOf(new TestFieldByIndexNil_T(nil));
        v.FieldByName("P"u8); // should be fine
        defer(() => {
            {
                var err = recover(); if (err == default!){
                    Ꮡt.Fatalf("no error"u8);
                } else 
                if (!strings.Contains(fmt.Sprint(err), nilPointerToEmbeddedˢ)) {
                    Ꮡt.Fatalf(@"err=%q, wanted error containing ""nil pointer to embedded struct"""u8, err);
                }
            }
        }, ref ᒐ);
        v.FieldByName("F"u8); // should panic
        Ꮡt.Fatalf("did not panic"u8);
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
[GoType] partial struct Outer {
    public partial ref ж<Inner> Inner { get; }
    public Δio.Reader R;
}

[GoType] partial struct Inner {
    public ж<Outer> X;
    public uintptr P1;
    public uintptr P2;
}

public static void M(this ж<Inner> Ꮡpi) {
    ref var pi = ref Ꮡpi.DerefOrNull();

    // Clear references to pi so that the only way the
    // garbage collection will find the pointer is in the
    // argument frame, typed as a *Outer.
    pi.X.Value.Inner = default!;
    // Set up an interface value that will cause a crash.
    // P1 = 1 is a non-zero, so the interface looks non-nil.
    // P2 = pi ensures that the data word points into the
    // allocated heap; if not the collection skips the interface
    // value as irrelevant, without dereferencing P1.
    pi.P1 = 1;
    pi.P2 = (uintptr)(uintptr)@unsafe.Pointer.FromRef(ref pi);
}

public static void TestCallMethodJump(ж<Δtesting.T> Ꮡt) {
    // In reflect.Value.Call, trigger a garbage collection after reflect.call
    // returns but before the args frame has been discarded.
    // This is a little clumsy but makes the failure repeatable.
    reflect_internal_test_package.CallGC.Value = true;
    var p = Ꮡ(new Outer(Inner: @new<Inner>()));
    p.Value.Inner.Value.X = p;
    ValueOf(p.OrTypedNil()).Method(0).Call(default!);
    // Stop garbage collecting during reflect.call.
    reflect_internal_test_package.CallGC.Value = false;
}

[GoType("dyn")] internal partial struct TestCallArgLive_T {
    public ж<@string> X, Y;
}

public static void TestCallArgLive(ж<Δtesting.T> Ꮡt) {
    var F = (TestCallArgLive_T tΔ1) => {
        tΔ1.X.Value = "ok"u8;
    };
    // In reflect.Value.Call, trigger a garbage collection in reflect.call
    // between marshaling argument and the actual call.
    reflect_internal_test_package.CallGC.Value = true;
    var x = @new<@string>();
    Δruntime.SetFinalizer(x.OrTypedNil(), (ж<@string> p) => {
        if (p.Value != "ok"u8) {
            Ꮡt.Errorf("x dead prematurely"u8);
        }
    });
    var v = new TestCallArgLive_T(x, nil);
    ValueOf((F).OrTypedNilFunc()).Call(new reflectꓸValue[]{ValueOf(v)}.slice());
    // Stop garbage collecting during reflect.call.
    reflect_internal_test_package.CallGC.Value = false;
}

public static void TestMakeFuncStackCopy(ж<Δtesting.T> Ꮡt) {
    var target = (slice<reflectꓸValue> @in) => {
        Δruntime.GC();
        useStack(16);
        return new reflectꓸValue[]{ValueOf((nint)(9))}.slice();
    };
    ref var concrete = ref heap<Func<ж<nint>, nint, nint>>(out var Ꮡconcrete);
    var fn = MakeFunc(ValueOf((concrete).OrTypedNilFunc()).Type(), target);
    ValueOf(Ꮡconcrete).Elem().Set(fn);
    nint x = concrete(nil, 7);
    if (x != 9) {
        Ꮡt.Errorf("have %#q want 9"u8, x);
    }
}

// use about n KB of stack
internal static void useStack(nint n) {
    if (n == 0) {
        return;
    }
    array<byte> b = new(1024);                     // makes frame about 1KB
    useStack(n - 1 + (nint)b[99]);
}

[GoType] partial struct Impl {
}

public static void F(this Impl _) {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object reflectTestImplValueˢ = (@string)"<reflect_test.Impl Value>"u8;
internal static readonly object funcValueˢ = (@string)"<func() Value>"u8;

public static void TestValueString(ж<Δtesting.T> Ꮡt) {
    var rv = ValueOf(new Impl(nil));
    if (rv.String() != "<reflect_test.Impl Value>"u8) {
        Ꮡt.Errorf("ValueOf(Impl{}).String() = %q, want %q"u8, rv.String(), reflectTestImplValueˢ);
    }
    var method = rv.Method(0);
    if (method.String() != "<func() Value>"u8) {
        Ꮡt.Errorf("ValueOf(Impl{}).Method(0).String() = %q, want %q"u8, method.String(), funcValueˢ);
    }
}

// Used to have inconsistency between IsValid() and Kind() != Invalid.
[GoType("dyn")] internal partial struct TestInvalid_T {
    internal any v;
}

public static void TestInvalid(ж<Δtesting.T> Ꮡt) {
    var v = ValueOf(new TestInvalid_T(nil)).Field(0);
    if (v.IsValid() != true || v.Kind() != ΔInterface) {
        Ꮡt.Errorf("field: IsValid=%v, Kind=%v, want true, Interface"u8, v.IsValid(), v.Kind());
    }
    v = v.Elem();
    if (v.IsValid() != false || v.Kind() != Invalid) {
        Ꮡt.Errorf("field elem: IsValid=%v, Kind=%v, want false, Invalid"u8, v.IsValid(), v.Kind());
    }
}

// Issue 8917.
public static void TestLarge(ж<Δtesting.T> Ꮡt) {
    var fv = ValueOf(([GoArrayDims(256)] array<ж<byte>> _) => {
    });
    fv.Call(new reflectꓸValue[]{ValueOf(new ж<byte>[]{}.array(256))}.slice());
}

internal static any /*recovered*/ fieldIndexRecover(reflectꓸType t, nint i) {
    any recovered = default!;
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

[GoType("dyn")] internal partial struct TestTypeFieldOutOfRangePanic_i {
    public nint X;
}

[GoType("dyn")] internal partial struct TestTypeFieldOutOfRangePanic_testIndices {
    internal nint i;
    internal bool mustPanic;
}

// Issue 15046.
public static void TestTypeFieldOutOfRangePanic(ж<Δtesting.T> Ꮡt) {
    var typ = TypeOf(new TestTypeFieldOutOfRangePanic_i(10));
    var testIndices = new array<TestTypeFieldOutOfRangePanic_testIndices>(4){
        [0] = new(-2, true),
        [1] = new(0, false),
        [2] = new(1, true),
        [3] = new((1 << (int)(10)), true)
    };
    foreach (var (i, tt) in testIndices.ΔRangeSnapshot()) {
        var recoveredErr = fieldIndexRecover(typ, tt.i);
        if (tt.mustPanic){
            if (recoveredErr == default!) {
                Ꮡt.Errorf("#%d: fieldIndex %d expected to panic"u8, i, tt.i);
            }
        } else {
            if (recoveredErr != default!) {
                Ꮡt.Errorf("#%d: got err=%v, expected no panic"u8, i, recoveredErr);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object testDoesNotFaultOnGoosJsˢ = (@string)"test does not fault on GOOS=js"u8;

public static void TestTypeFieldReadOnly(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        if (Δruntime.GOOS == "js"u8 || Δruntime.GOOS == "wasip1"u8) {
            // This is OK because we don't use the optimization
            // for js or wasip1.
            Ꮡt.Skip(testDoesNotFaultOnGoosJsˢ);
        }
        // It's important that changing one StructField.Index
        // value not affect other StructField.Index values.
        // Right now StructField.Index is read-only;
        // that saves allocations but is otherwise not important.
        var typ = TypeFor<TestAllocations_type>();
        ref var f = ref heap<Δreflect.StructField>(out var Ꮡf);
        f = typ.Field(0);
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
internal static readonly @string fourˢ = "four"u8;
internal static readonly @string five5ˢ = "five5"u8;
internal static readonly @string six666ˢ = "six666"u8;
internal static readonly @string seven77ˢ = "seven77"u8;
internal static readonly @string eight888ˢ = "eight888"u8;

// Issue 9179.
public static void TestCallGC(ж<Δtesting.T> Ꮡt) {
    var f = (@string a, @string b, @string c, @string d, @string e) => {
    };
    var g = slice<reflectꓸValue> (slice<reflectꓸValue> @in) => {
        Δruntime.GC();
        return default!;
    };
    var typ = ValueOf((f).OrTypedNilFunc()).Type();
    var f2 = MakeFunc(typ, g).Interface()._<Action<@string, @string, @string, @string, @string>>();
    f2(fourˢ, five5ˢ, six666ˢ, seven77ˢ, eight888ˢ);
}

// Issue 18635 (function version).
public static void TestKeepFuncLive(ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // Test that we keep makeFuncImpl live as long as it is
    // referenced on the stack.
    var typ = TypeOf((nint i) => {
    });
    Func<slice<reflectꓸValue>, slice<reflectꓸValue>> f = default!;
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
        return default!;
    };
    var fʗ1 = f;
    var typʗ2 = typ;
    g = slice<reflectꓸValue> (slice<reflectꓸValue> @in) => {
        clobber();
        nint i = (nint)@in[0].Int();
        MakeFunc(typʗ2, fʗ1).Interface()._<Action<nint>>()(i);
        return default!;
    };
    MakeFunc(typ, f).Call(new reflectꓸValue[]{ValueOf((nint)(10))}.slice());
}

[GoType("num:nint")] partial struct UnExportedFirst;

public static void ΦExported(this UnExportedFirst i) {
}

internal static void unexported(this UnExportedFirst i) {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string exportedˢ = "ΦExported"u8;

// Issue 21177
public static void TestMethodByNameUnExportedFirst(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            if (recover() != default!) {
                Ꮡt.Errorf("should not panic"u8);
            }
        }, ref ᒐ);
        var typ = TypeOf(((UnExportedFirst)0));
        var (m, _) = typ.MethodByName(exportedˢ);
        if (m.Name != "ΦExported"u8) {
            Ꮡt.Errorf("got %s, expected ΦExported"u8, m.Name);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Issue 18635 (method version).
[GoType] partial struct KeepMethodLive {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string method2ˢ = "Method2"u8;

public static void Method1(this KeepMethodLive k, nint i) {
    clobber();
    if (i > 0) {
        ValueOf(k).MethodByName(method2ˢ).Interface()._<Action<nint>>()(i - 1);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string method1ˢ = "Method1"u8;

public static void Method2(this KeepMethodLive k, nint i) {
    clobber();
    ValueOf(k).MethodByName(method1ˢ).Interface()._<Action<nint>>()(i);
}

public static void TestKeepMethodLive(ж<Δtesting.T> Ꮡt) {
    // Test that we keep methodValue live as long as it is
    // referenced on the stack.
    new KeepMethodLive(nil).Method1(10);
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

[GoType("dyn")] internal partial struct TestFuncLayout_S {
    internal uintptr a, b;
    internal ж<byte> c, d;
}

[GoType("dyn")] internal partial struct TestFuncLayout_test {
    internal reflectꓸType rcvr, typ;
    internal uintptr size, argsize, retOffset;
    internal slice<byte> stack, gc, inRegs, outRegs; // pointer bitmap: 1 is pointer, 0 is scalar
    internal nint intRegs, floatRegs;
    internal uintptr floatRegSize;
}

public static void TestFuncLayout(ж<Δtesting.T> Ꮡt) {
    uintptr align(uintptr x) => (uintptr)((x + (uintptr)goarch.PtrSize - 1) & ~(uintptr)(goarch.PtrSize - 1));
    slice<byte> r = default!;
    if (goarch.PtrSize == 4){
        r = new byte[]{0, 0, 0, 1}.slice();
    } else {
        r = new byte[]{0, 0, 1}.slice();
    }
    var tests = new TestFuncLayout_test[]{
        new(
            typ: ValueOf(@string (@string a, @string b) => ""u8).Type(),
            size: 6 * goarch.PtrSize,
            argsize: 4 * goarch.PtrSize,
            retOffset: 4 * goarch.PtrSize,
            stack: new byte[]{1, 0, 1, 0, 1}.slice(),
            gc: new byte[]{1, 0, 1, 0, 1}.slice()
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
            stack: new byte[]{1, 0, 1, 1}.slice(),
            gc: new byte[]{1, 0, 1, 1}.slice()
        ),
        new(
            typ: ValueOf((TestFuncLayout_S a) => {
            }).Type(),
            size: 4 * goarch.PtrSize,
            argsize: 4 * goarch.PtrSize,
            retOffset: 4 * goarch.PtrSize,
            stack: new byte[]{0, 0, 1, 1}.slice(),
            gc: new byte[]{0, 0, 1, 1}.slice()
        ),
        new(
            rcvr: ValueOf(((ж<byte>)nil)).Type(),
            typ: ValueOf((uintptr a, ж<nint> b) => {
            }).Type(),
            size: 3 * goarch.PtrSize,
            argsize: 3 * goarch.PtrSize,
            retOffset: 3 * goarch.PtrSize,
            stack: new byte[]{1, 0, 1}.slice(),
            gc: new byte[]{1, 0, 1}.slice()
        ),
        new(
            typ: ValueOf((uintptr a) => {
            }).Type(),
            size: goarch.PtrSize,
            argsize: goarch.PtrSize,
            retOffset: goarch.PtrSize,
            stack: new byte[]{}.slice(),
            gc: new byte[]{}.slice()
        ),
        new(
            typ: ValueOf(uintptr () => (uintptr)(0)).Type(),
            size: goarch.PtrSize,
            argsize: 0,
            retOffset: 0,
            stack: new byte[]{}.slice(),
            gc: new byte[]{}.slice()
        ),
        new(
            rcvr: ValueOf((uintptr)0).Type(),
            typ: ValueOf((uintptr a) => {
            }).Type(),
            size: 2 * goarch.PtrSize,
            argsize: 2 * goarch.PtrSize,
            retOffset: 2 * goarch.PtrSize,
            stack: new byte[]{1}.slice(),
            gc: new byte[]{1}.slice() // Note: this one is tricky, as the receiver is not a pointer. But we
 // pass the receiver by reference to the autogenerated pointer-receiver
 // version of the function.

        )
    }.slice();
    // TODO(mknyszek): Add tests for non-zero register count.
    foreach (var (_, vᴛ1) in tests) {
        ref var lt = ref heap(new TestFuncLayout_test(), out var Ꮡlt);
        lt = vᴛ1;

        @string name = lt.typ.String();
        if (lt.rcvr != default!) {
            name = lt.rcvr.String() + "."u8 + name;
        }
        var ltʗ1 = lt;
        Ꮡt.Run(name, (ж<Δtesting.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                defer(ᴛ1 => reflect_internal_test_package.SetArgRegs(ᴛ1.Item1, ᴛ1.Item2, ᴛ1.Item3), reflect_internal_test_package.SetArgRegs(ltʗ1.intRegs, ltʗ1.floatRegs, ltʗ1.floatRegSize), ref ᒐ);
                var (typ, argsize, retOffset, stack, gc, inRegs, outRegs, ptrs) = reflect_internal_test_package.FuncLayout(ltʗ1.typ, ltʗ1.rcvr);
                if (typ.Size() != ltʗ1.size) {
                    tΔ1.Errorf("funcLayout(%v, %v).size=%d, want %d"u8, ltʗ1.typ, ltʗ1.rcvr, typ.Size(), ltʗ1.size);
                }
                if (argsize != ltʗ1.argsize) {
                    tΔ1.Errorf("funcLayout(%v, %v).argsize=%d, want %d"u8, ltʗ1.typ, ltʗ1.rcvr, argsize, ltʗ1.argsize);
                }
                if (retOffset != ltʗ1.retOffset) {
                    tΔ1.Errorf("funcLayout(%v, %v).retOffset=%d, want %d"u8, ltʗ1.typ, ltʗ1.rcvr, retOffset, ltʗ1.retOffset);
                }
                if (!bytes.Equal(stack, ltʗ1.stack)) {
                    tΔ1.Errorf("funcLayout(%v, %v).stack=%v, want %v"u8, ltʗ1.typ, ltʗ1.rcvr, stack, ltʗ1.stack);
                }
                if (!bytes.Equal(gc, ltʗ1.gc)) {
                    tΔ1.Errorf("funcLayout(%v, %v).gc=%v, want %v"u8, ltʗ1.typ, ltʗ1.rcvr, gc, ltʗ1.gc);
                }
                if (!bytes.Equal(inRegs, ltʗ1.inRegs)) {
                    tΔ1.Errorf("funcLayout(%v, %v).inRegs=%v, want %v"u8, ltʗ1.typ, ltʗ1.rcvr, inRegs, ltʗ1.inRegs);
                }
                if (!bytes.Equal(outRegs, ltʗ1.outRegs)) {
                    tΔ1.Errorf("funcLayout(%v, %v).outRegs=%v, want %v"u8, ltʗ1.typ, ltʗ1.rcvr, outRegs, ltʗ1.outRegs);
                }
                if (ptrs && len(stack) == 0 || !ptrs && len(stack) > 0) {
                    tΔ1.Errorf("funcLayout(%v, %v) pointers flag=%v, want %v"u8, ltʗ1.typ, ltʗ1.rcvr, ptrs, !ptrs);
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

[MethodImpl(MethodImplOptions.NoInlining)] internal static void verifyGCBits(ж<Δtesting.T> Ꮡt, reflectꓸType typ, slice<byte> bits) {
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
    Ꮡt.Errorf("line %d: heapBits incorrect for %v\nhave %v\nwant %v"u8, line, typ, heapBits, bits);
}

[MethodImpl(MethodImplOptions.NoInlining)] internal static void verifyGCBitsSlice(ж<Δtesting.T> Ꮡt, reflectꓸType typ, nint cap, slice<byte> bits) {
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
    Ꮡt.Errorf("line %d: heapBits incorrect for make(%v, 0, %v)\nhave %v\nwant %v"u8, line, typ, cap, heapBits, bits);
}

// Building blocks for types seen by the compiler (like [2]Xscalar).
// The compiler will create the type structures for the derived types,
// including their GC metadata.
[GoType] partial struct Xscalar {
    internal uintptr x;
}

[GoType] partial struct Xptr {
    internal ж<byte> x;
}

[GoType] partial struct Xptrscalar {
    [GoEmbedded] internal ж<byte> @byte;
    [GoEmbedded] internal uintptr uintptr;
}

[GoType] partial struct Xscalarptr {
    [GoEmbedded] internal uintptr uintptr;
    [GoEmbedded] internal ж<byte> @byte;
}

[GoType] partial struct Xbigptrscalar {
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
[GoType("dyn")] internal partial struct init_Scalar {
    internal uintptr x;
}

[GoType("dyn")] internal partial struct init_Ptr {
    internal ж<byte> x;
}

[GoType("dyn")] internal partial struct init_Ptrscalar {
    [GoEmbedded] internal ж<byte> @byte;
    [GoEmbedded] internal uintptr uintptr;
}

[GoType("dyn")] internal partial struct init_Scalarptr {
    [GoEmbedded] internal uintptr uintptr;
    [GoEmbedded] internal ж<byte> @byte;
}

[GoType("dyn")] internal partial struct init_Bigptrscalar {
    internal array<ж<byte>> _ = new(100);
    internal array<uintptr> __ = new(100);
}

[GoLocalName("Int64")] [GoType("num:int64")] internal partial struct init_Int64;

[GoInit] internal static void initΔ1() {
    Tscalar = TypeOf(new init_Scalar(nil));
    Tint64 = TypeOf(((init_Int64)0));
    Tptr = TypeOf(new init_Ptr(nil));
    Tscalarptr = TypeOf(new init_Scalarptr(nil));
    Tptrscalar = TypeOf(new init_Ptrscalar(nil));
    Tbigptrscalar = TypeOf(new init_Bigptrscalar(nil));
}

internal static slice<byte> empty = new byte[]{}.slice();

public static void TestGCBits(ж<Δtesting.T> Ꮡt) {
    verifyGCBits(Ꮡt, TypeOf(((ж<byte>)nil)), new byte[]{1}.slice());
    verifyGCBits(Ꮡt, TypeOf(new Xscalar(nil)), empty);
    verifyGCBits(Ꮡt, Tscalar, empty);
    verifyGCBits(Ꮡt, TypeOf(new Xptr(nil)), lit(1));
    verifyGCBits(Ꮡt, Tptr, lit(1));
    verifyGCBits(Ꮡt, TypeOf(new Xscalarptr(nil)), lit(0, 1));
    verifyGCBits(Ꮡt, Tscalarptr, lit(0, 1));
    verifyGCBits(Ꮡt, TypeOf(new Xptrscalar(nil)), lit(1));
    verifyGCBits(Ꮡt, Tptrscalar, lit(1));
    verifyGCBits(Ꮡt, TypeOf(new Xptr[]{}.array()), empty);
    verifyGCBits(Ꮡt, ArrayOf(0, Tptr), empty);
    verifyGCBits(Ꮡt, TypeOf(new Xptrscalar[]{}.array(1)), lit(1));
    verifyGCBits(Ꮡt, ArrayOf(1, Tptrscalar), lit(1));
    verifyGCBits(Ꮡt, TypeOf(new Xscalar[]{}.array(2)), empty);
    verifyGCBits(Ꮡt, ArrayOf(2, Tscalar), empty);
    verifyGCBits(Ꮡt, TypeOf(new Xscalar[]{}.array(10000)), empty);
    verifyGCBits(Ꮡt, ArrayOf(10000, Tscalar), empty);
    verifyGCBits(Ꮡt, TypeOf(new Xptr[]{}.array(2)), lit(1, 1));
    verifyGCBits(Ꮡt, ArrayOf(2, Tptr), lit(1, 1));
    verifyGCBits(Ꮡt, TypeOf(new Xptr[]{}.array(10000)), rep(10000, lit(1)));
    verifyGCBits(Ꮡt, ArrayOf(10000, Tptr), rep(10000, lit(1)));
    verifyGCBits(Ꮡt, TypeOf(new Xscalarptr[]{}.array(2)), lit(0, 1, 0, 1));
    verifyGCBits(Ꮡt, ArrayOf(2, Tscalarptr), lit(0, 1, 0, 1));
    verifyGCBits(Ꮡt, TypeOf(new Xscalarptr[]{}.array(10000)), rep(10000, lit(0, 1)));
    verifyGCBits(Ꮡt, ArrayOf(10000, Tscalarptr), rep(10000, lit(0, 1)));
    verifyGCBits(Ꮡt, TypeOf(new Xptrscalar[]{}.array(2)), lit(1, 0, 1));
    verifyGCBits(Ꮡt, ArrayOf(2, Tptrscalar), lit(1, 0, 1));
    verifyGCBits(Ꮡt, TypeOf(new Xptrscalar[]{}.array(10000)), rep(10000, lit(1, 0)));
    verifyGCBits(Ꮡt, ArrayOf(10000, Tptrscalar), rep(10000, lit(1, 0)));
    verifyGCBits(Ꮡt, TypeOf(new array<Xptrscalar>[]{}.array(1, () => new(10000))), rep(10000, lit(1, 0)));
    verifyGCBits(Ꮡt, ArrayOf(1, ArrayOf(10000, Tptrscalar)), rep(10000, lit(1, 0)));
    verifyGCBits(Ꮡt, TypeOf(new array<Xptrscalar>[]{}.array(2, () => new(10000))), rep(2 * 10000, lit(1, 0)));
    verifyGCBits(Ꮡt, ArrayOf(2, ArrayOf(10000, Tptrscalar)), rep(2 * 10000, lit(1, 0)));
    verifyGCBits(Ꮡt, TypeOf(new Xbigptrscalar[]{}.array(4)), join(rep(3, join(rep(100, lit(1)), rep(100, lit(0)))), rep(100, lit(1))));
    verifyGCBits(Ꮡt, ArrayOf(4, Tbigptrscalar), join(rep(3, join(rep(100, lit(1)), rep(100, lit(0)))), rep(100, lit(1))));
    verifyGCBitsSlice(Ꮡt, TypeOf(new Xptr[]{}.slice()), 0, empty);
    verifyGCBitsSlice(Ꮡt, SliceOf(Tptr), 0, empty);
    verifyGCBitsSlice(Ꮡt, TypeOf(new Xptrscalar[]{}.slice()), 1, lit(1));
    verifyGCBitsSlice(Ꮡt, SliceOf(Tptrscalar), 1, lit(1));
    verifyGCBitsSlice(Ꮡt, TypeOf(new Xscalar[]{}.slice()), 2, lit(0));
    verifyGCBitsSlice(Ꮡt, SliceOf(Tscalar), 2, lit(0));
    verifyGCBitsSlice(Ꮡt, TypeOf(new Xscalar[]{}.slice()), 10000, lit(0));
    verifyGCBitsSlice(Ꮡt, SliceOf(Tscalar), 10000, lit(0));
    verifyGCBitsSlice(Ꮡt, TypeOf(new Xptr[]{}.slice()), 2, lit(1));
    verifyGCBitsSlice(Ꮡt, SliceOf(Tptr), 2, lit(1));
    verifyGCBitsSlice(Ꮡt, TypeOf(new Xptr[]{}.slice()), 10000, lit(1));
    verifyGCBitsSlice(Ꮡt, SliceOf(Tptr), 10000, lit(1));
    verifyGCBitsSlice(Ꮡt, TypeOf(new Xscalarptr[]{}.slice()), 2, lit(0, 1));
    verifyGCBitsSlice(Ꮡt, SliceOf(Tscalarptr), 2, lit(0, 1));
    verifyGCBitsSlice(Ꮡt, TypeOf(new Xscalarptr[]{}.slice()), 10000, lit(0, 1));
    verifyGCBitsSlice(Ꮡt, SliceOf(Tscalarptr), 10000, lit(0, 1));
    verifyGCBitsSlice(Ꮡt, TypeOf(new Xptrscalar[]{}.slice()), 2, lit(1, 0));
    verifyGCBitsSlice(Ꮡt, SliceOf(Tptrscalar), 2, lit(1, 0));
    verifyGCBitsSlice(Ꮡt, TypeOf(new Xptrscalar[]{}.slice()), 10000, lit(1, 0));
    verifyGCBitsSlice(Ꮡt, SliceOf(Tptrscalar), 10000, lit(1, 0));
    verifyGCBitsSlice(Ꮡt, TypeOf(GoReflect.WithElemDims(new array<Xptrscalar>[]{}.slice(), 10000)), 1, rep(10000, lit(1, 0)));
    verifyGCBitsSlice(Ꮡt, SliceOf(ArrayOf(10000, Tptrscalar)), 1, rep(10000, lit(1, 0)));
    verifyGCBitsSlice(Ꮡt, TypeOf(GoReflect.WithElemDims(new array<Xptrscalar>[]{}.slice(), 10000)), 2, rep(10000, lit(1, 0)));
    verifyGCBitsSlice(Ꮡt, SliceOf(ArrayOf(10000, Tptrscalar)), 2, rep(10000, lit(1, 0)));
    verifyGCBitsSlice(Ꮡt, TypeOf(new Xbigptrscalar[]{}.slice()), 4, join(rep(100, lit(1)), rep(100, lit(0))));
    verifyGCBitsSlice(Ꮡt, SliceOf(Tbigptrscalar), 4, join(rep(100, lit(1)), rep(100, lit(0))));
    verifyGCBits(Ꮡt, TypeOf(channel<array<Xscalar>>.Nil(ChanCargo.Of(null, new nint[] { 100 }))), lit(1));
    verifyGCBits(Ꮡt, ChanOf(BothDir, ArrayOf(100, Tscalar)), lit(1));
    verifyGCBits(Ꮡt, TypeOf(((Action<array<Xscalarptr>>)(default!)).OrTypedNilFunc()), lit(1));
    verifyGCBits(Ꮡt, FuncOf(new reflectꓸType[]{ArrayOf(10000, Tscalarptr)}.slice(), default!, false), lit(1));
    verifyGCBits(Ꮡt, TypeOf(((map<array<Xscalarptr>, Xscalar>)default!)), lit(1));
    verifyGCBits(Ꮡt, MapOf(ArrayOf(10000, Tscalarptr), Tscalar), lit(1));
    verifyGCBits(Ꮡt, TypeOf(ж<array<Xscalar>>.NilBoxOfDims(10000L)), lit(1));
    verifyGCBits(Ꮡt, PointerTo(ArrayOf(10000, Tscalar)), lit(1));
    verifyGCBits(Ꮡt, TypeOf((slice<array<Xscalar>>)(default!)), lit(1));
    verifyGCBits(Ꮡt, SliceOf(ArrayOf(10000, Tscalar)), lit(1));
    testGCBitsMap(Ꮡt);
}

internal static slice<byte> rep(nint n, slice<byte> b) {
    return bytes.Repeat(b, n);
}

internal static slice<byte> join(params Span<slice<byte>> bʗp) {
    var b = bʗp.slice();

    return bytes.Join(b, default!);
}

internal static slice<byte> lit(params ꓸꓸꓸbyte xʗp) {
    var x = xʗp.slice();

    return x;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string typeOfˢ = "TypeOf"u8;
internal static readonly @string arrayOfˢ = "ArrayOf"u8;
internal static readonly @string chanOfˢ = "ChanOf"u8;
internal static readonly @string funcOfˢ = "FuncOf"u8;
internal static readonly @string mapOfˢ = "MapOf"u8;
internal static readonly @string ptrToˢ = "PtrTo"u8;
internal static readonly @string sliceOfˢ = "SliceOf"u8;

[GoType("dyn")] internal partial struct TestTypeOfTypeOf_T {
    [GoEmbedded] internal nint @int;
}

public static void TestTypeOfTypeOf(ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // Check that all the type constructors return concrete *rtype implementations.
    // It's difficult to test directly because the reflect package is only at arm's length.
    // The easiest thing to do is just call a function that crashes if it doesn't get an *rtype.
    void check(@string name, reflectꓸType typ) {
        {
            @string underlying = TypeOf(typ).String(); if (underlying != "*reflect.rtype"u8) {
                Ꮡt.Errorf("%v returned %v, not *reflect.rtype"u8, name, underlying);
            }
        }
    }
    check(typeOfˢ, TypeOf(new TestTypeOfTypeOf_T(nil)));
    check(arrayOfˢ, ArrayOf(10, TypeOf(new TestTypeOfTypeOf_T(nil))));
    check(chanOfˢ, ChanOf(BothDir, TypeOf(new TestTypeOfTypeOf_T(nil))));
    check(funcOfˢ, FuncOf(new reflectꓸType[]{TypeOf(new TestTypeOfTypeOf_T(nil))}.slice(), default!, false));
    check(mapOfˢ, MapOf(TypeOf(new TestTypeOfTypeOf_T(nil)), TypeOf(new TestTypeOfTypeOf_T(nil))));
    check(ptrToˢ, PointerTo(TypeOf(new TestTypeOfTypeOf_T(nil))));
    check(sliceOfˢ, SliceOf(TypeOf(new TestTypeOfTypeOf_T(nil))));
}

[GoType] partial struct XM {
    internal bool _;
}

[GoRecv] public static @string String(this ref XM _) {
    return ""u8;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object doesNotImplementStringerˢ = (@string)"does not implement Stringer, but should"u8;

[GoType("dyn")] internal partial struct TestPtrToMethods_y {
    public partial ref XM XM { get; }
}

public static void TestPtrToMethods(ж<Δtesting.T> Ꮡt) {
    TestPtrToMethods_y y = new(nil);
    var yp = New(TypeOf(y)).Interface();
    var (_, ok) = yp._<fmt.Stringer>(ᐧ);
    if (!ok) {
        Ꮡt.Fatal(doesNotImplementStringerˢ);
    }
}

public static void TestMapAlloc(ж<Δtesting.T> Ꮡt) {
    if (asan.Enabled) {
        Ꮡt.Skip(testAllocatesMoreWithˢ);
    }
    ref var m = ref heap<reflectꓸValue>(out var Ꮡm);
    m = ValueOf(new map<nint, nint>(10));
    ref var k = ref heap<reflectꓸValue>(out var Ꮡk);
    k = ValueOf((nint)(5));
    ref var v = ref heap<reflectꓸValue>(out var Ꮡv);
    v = ValueOf((nint)(7));
    var kʗ1 = k;
    var mʗ1 = m;
    var vʗ1 = v;
    var allocs = Δtesting.AllocsPerRun(100, () => {
        mʗ1.SetMapIndex(kʗ1, vʗ1);
    });
    if (allocs > 0.5D) {
        Ꮡt.Errorf("allocs per map assignment: want 0 got %f"u8, allocs);
    }
    UntypedInt size = 1000;
    ref var tmp = ref heap<nint>(out var Ꮡtmp);
    tmp = 0;
    ref var val = ref heap<reflectꓸValue>(out var Ꮡval);
    val = ValueOf(Ꮡtmp).Elem();
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
        Ꮡt.Errorf("allocs per map assignment: want at most 10 got %f"u8, allocs);
    }
}

// Empirical testing shows that with capacity hint single run will trigger 3 allocations and without 91. I set
// the threshold to 10, to not make it overly brittle if something changes in the initial allocation of the
// map, but to still catch a regression where we keep re-allocating in the hashmap as new entries are added.
public static void TestChanAlloc(ж<Δtesting.T> Ꮡt) {
    if (asan.Enabled) {
        Ꮡt.Skip(testAllocatesMoreWithˢ);
    }
    // Note: for a chan int, the return Value must be allocated, so we
    // use a chan *int instead.
    ref var c = ref heap<reflectꓸValue>(out var Ꮡc);
    c = ValueOf(new channel<ж<nint>>(1));
    ref var v = ref heap<reflectꓸValue>(out var Ꮡv);
    v = ValueOf(@new<nint>());
    var cʗ1 = c;
    var vʗ1 = v;
    var allocs = Δtesting.AllocsPerRun(100, () => {
        cʗ1.Send(vʗ1);
        (_, _) = cʗ1.Recv();
    });
    if (allocs < 0.5D || allocs > 1.5D) {
        Ꮡt.Errorf("allocs per chan send/recv: want 1 got %f"u8, allocs);
    }
}

[GoType("num:nint")] partial struct TheNameOfThisTypeIsExactly255BytesLongSoWhenTheCompilerPrependsTheReflectTestPackageNameAndExtraStarTheLinkerRuntimeAndReflectPackagesWillHaveToCorrectlyDecodeTheSecondLengthByte0123456789_0123456789_0123456789_0123456789_0123456789_012345678;

// Note: there is one allocation in reflect.recv which seems to be
// a limitation of escape analysis. If that is ever fixed the
// allocs < 0.5 condition will trigger and this test should be fixed.
[GoType] partial struct nameTest {
    internal any v;
    internal @string want;
}


        [GoType("dyn")] partial interface Δtypeᴛ35 {
            void F();
        }
internal static slice<nameTest> nameTests = new nameTest[]{
    new(((ж<int32>)nil), "int32"u8),
    new(((ж<D1>)nil), "D1"u8),
    new(((ж<slice<D1>>)nil), ""u8),
    new(((ж<channel<D1>>)nil), ""u8),
    new(((ж<Func<D1>>)nil), ""u8),
    new(((ж</*<-*/channel<D1>>)nil), ""u8),
    new(((ж<channel/*<-*/<D1>>)nil), ""u8),
    new(((ж<any>)nil), ""u8),
    new(((ж<Δtypeᴛ35>)nil), ""u8),
    new(((ж<TheNameOfThisTypeIsExactly255BytesLongSoWhenTheCompilerPrependsTheReflectTestPackageNameAndExtraStarTheLinkerRuntimeAndReflectPackagesWillHaveToCorrectlyDecodeTheSecondLengthByte0123456789_0123456789_0123456789_0123456789_0123456789_012345678>)nil), "TheNameOfThisTypeIsExactly255BytesLongSoWhenTheCompilerPrependsTheReflectTestPackageNameAndExtraStarTheLinkerRuntimeAndReflectPackagesWillHaveToCorrectlyDecodeTheSecondLengthByte0123456789_0123456789_0123456789_0123456789_0123456789_012345678"u8)
}.slice();

public static void TestNames(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, test) in nameTests) {
        var typ = TypeOf(test.v).Elem();
        {
            @string got = typ.Name(); if (got != test.want) {
                Ꮡt.Errorf("%v Name()=%q, want %q"u8, typ, got, test.want);
            }
        }
    }
}

[GoType("dyn")] internal partial struct TestExported_ΦExported {
}

[GoType("dyn")] internal partial struct TestExported_φUnexported {
}

[GoLocalName("BigP")] [GoType("ж<big>")] internal partial class TestExported_BigP;

[GoLocalName("P")] [GoType("num:nint")] internal partial struct TestExported_P;

[GoLocalName("p")] [GoType("ж<TestExported_P>")] internal partial class TestExported_p;

[GoLocalName("P2")] [GoType("TestExported_p")] internal partial struct TestExported_P2;

[GoLocalName("p3")] [GoType("TestExported_p")] internal partial struct TestExported_p3;

[GoType("dyn")] internal partial struct TestExported_exportTest {
    internal any v;
    internal bool want;
}

public static void TestExported(ж<Δtesting.T> Ꮡt) {
    var exportTests = new TestExported_exportTest[]{
        new(new D1(nil), true),
        new(((ж<D1>)nil), true),
        new(new big(nil), false),
        new(((ж<big>)nil), false),
        new(((TestExported_BigP)nil), true),
        new(((ж<TestExported_BigP>)nil), true),
        new(new TestExported_ΦExported(nil), true),
        new(new TestExported_φUnexported(nil), false),
        new(((TestExported_P)0), true),
        new(((TestExported_p)nil), false),
        new(((TestExported_P2)nil), true),
        new(((TestExported_p3)nil), false)
    }.slice();
    foreach (var (i, test) in exportTests) {
        var typ = TypeOf(test.v);
        {
            var got = reflect_internal_test_package.IsExported(typ); if (got != test.want) {
                Ꮡt.Errorf("%d: %s exported=%v, want %v"u8, i, typ.Name(), got, test.want);
            }
        }
    }
}

[GoType("dyn")] internal partial struct TestTypeStrings_stringTest {
    internal reflectꓸType typ;
    internal @string want;
}

public static void TestTypeStrings(ж<Δtesting.T> Ꮡt) {
    var stringTests = new TestTypeStrings_stringTest[]{
        new(TypeOf((nint _) => {
        }), "func(int)"u8),
        new(FuncOf(new reflectꓸType[]{TypeOf((nint)0)}.slice(), default!, false), "func(int)"u8),
        new(TypeOf(new XM(nil)), "reflect_test.XM"u8),
        new(TypeOf(@new<XM>()), "*reflect_test.XM"u8),
        new(TypeOf(@new<XM>().String), "func() string"u8),
        new(TypeOf(@new<XM>()).Method(0).Type, "func(*reflect_test.XM) string"u8),
        new(ChanOf(3, TypeOf(new XM(nil))), "chan reflect_test.XM"u8),
        new(MapOf(TypeOf((nint)0), TypeOf(new XM(nil))), "map[int]reflect_test.XM"u8),
        new(ArrayOf(3, TypeOf(new XM(nil))), "[3]reflect_test.XM"u8),
        new(ArrayOf(3, TypeOf(new EmptyStruct())), "[3]struct {}"u8)
    }.slice();
    foreach (var (i, test) in stringTests) {
        {
            @string got = test.typ.String();
            @string want = test.want; if (got != want) {
                Ꮡt.Errorf("type %d String()=%q, want %q"u8, i, got, want);
            }
        }
    }
}

[MethodImpl(MethodImplOptions.NoInlining)] public static void TestOffsetLock(ж<Δtesting.T> Ꮡt) {
    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    for (nint i = 0; i < 4; i++) {
        nint iΔ1 = i;
        Ꮡwg.Add(1);
        goǃ(() => {
            for (nint j = 0; j < 50; j++) {
                reflect_internal_test_package.ResolveReflectName(fmt.Sprintf("OffsetLockName:%d:%d"u8, iΔ1, j));
            }
            Ꮡwg.Done();
        });
    }
    Ꮡwg.Wait();
}

[GoLocalName("I")] [GoType("num:nint")] internal partial struct TestSwapper_I;

[GoType("dyn")] internal partial struct TestSwapper_pair {
    internal nint x, y;
}

[GoType("dyn")] internal partial struct TestSwapper_pairPtr {
    internal nint x, y;
    internal ж<TestSwapper_I> p;
}

[GoLocalName("S")] [GoType("@string")] internal partial struct TestSwapper_S;

[GoType("dyn")] internal partial struct TestSwapper_tests {
    internal any @in;
    internal nint i, j;
    internal any want;
}

public static void TestSwapper(ж<Δtesting.T> Ꮡt) {
    ref var a = ref heap(new TestSwapper_I(), out var Ꮡa);
    ref var b = ref heap(new TestSwapper_I(), out var Ꮡb);
    ref var c = ref heap(new TestSwapper_I(), out var Ꮡc);
    var tests = new TestSwapper_tests[]{
        new(
            @in: new nint[]{1, 20, 300}.slice(),
            i: 0,
            j: 2,
            want: new nint[]{300, 20, 1}.slice()
        ),
        new(
            @in: new uintptr[]{1, 20, 300}.slice(),
            i: 0,
            j: 2,
            want: new uintptr[]{300, 20, 1}.slice()
        ),
        new(
            @in: new int16[]{1, 20, 300}.slice(),
            i: 0,
            j: 2,
            want: new int16[]{300, 20, 1}.slice()
        ),
        new(
            @in: new int8[]{1, 20, 100}.slice(),
            i: 0,
            j: 2,
            want: new int8[]{100, 20, 1}.slice()
        ),
        new(
            @in: new ж<TestSwapper_I>[]{Ꮡa, Ꮡb, Ꮡc}.slice(),
            i: 0,
            j: 2,
            want: new ж<TestSwapper_I>[]{Ꮡc, Ꮡb, Ꮡa}.slice()
        ),
        new(
            @in: new @string[]{"eric"u8, "sergey"u8, "larry"u8}.slice(),
            i: 0,
            j: 2,
            want: new @string[]{"larry"u8, "sergey"u8, "eric"u8}.slice()
        ),
        new(
            @in: new TestSwapper_S[]{"eric"u8, "sergey"u8, "larry"u8}.slice(),
            i: 0,
            j: 2,
            want: new TestSwapper_S[]{"larry"u8, "sergey"u8, "eric"u8}.slice()
        ),
        new(
            @in: new TestSwapper_pair[]{new(1, 2), new(3, 4), new(5, 6)}.slice(),
            i: 0,
            j: 2,
            want: new TestSwapper_pair[]{new(5, 6), new(3, 4), new(1, 2)}.slice()
        ),
        new(
            @in: new TestSwapper_pairPtr[]{new(1, 2, Ꮡa), new(3, 4, Ꮡb), new(5, 6, Ꮡc)}.slice(),
            i: 0,
            j: 2,
            want: new TestSwapper_pairPtr[]{new(5, 6, Ꮡc), new(3, 4, Ꮡb), new(1, 2, Ꮡa)}.slice()
        )
    }.slice();
    foreach (var (i, tt) in tests) {
        @string inStr = fmt.Sprint(tt.@in);
        Swapper(tt.@in)(tt.i, tt.j);
        if (!DeepEqual(tt.@in, tt.want)) {
            Ꮡt.Errorf("%d. swapping %v and %v of %v = %v; want %v"u8, i, tt.i, tt.j, inStr, tt.@in, tt.want);
        }
    }
}

[GoType("dyn")] internal partial struct TestUnaddressableField_localBuffer {
    internal slice<byte> buf;
}

// TestUnaddressableField tests that the reflect package will not allow
// a type from another package to be used as a named type with an
// unexported field.
//
// This ensures that unexported fields cannot be modified by other packages.
public static void TestUnaddressableField(ж<Δtesting.T> Ꮡt) {
    global::go.reflect_internal_test_package.Buffer b = default!;                                                      // type defined in reflect, a different package
    ref var localBuffer = ref heap(new TestUnaddressableField_localBuffer(), out var ᏑlocalBuffer);
    ref var lv = ref heap<reflectꓸValue>(out var Ꮡlv);
    lv = ValueOf(ᏑlocalBuffer).Elem();
    ref var rv = ref heap<reflectꓸValue>(out var Ꮡrv);
    rv = ValueOf(b);
    var lvʗ1 = lv;
    var rvʗ1 = rv;
    shouldPanic(setˢ, () => {
        lvʗ1.Set(rvʗ1);
    });
}

[GoType("num:nint")] partial struct Tint;

[GoType] partial struct Talias1 {
    [GoEmbedded] internal byte @byte;
    [GoEmbedded] internal uint8 uint8;
    [GoEmbedded] internal nint @int;
    [GoEmbedded] internal int32 int32;
    [GoEmbedded] internal rune rune;
}

[GoType] partial struct Talias2 {
    public partial ref Tint Tint { get; }
    [GoEmbedded] public Tint2 Tint2;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string reflectTestTalias1Byteˢ = "reflect_test.Talias1{byte:0x1, uint8:0x2, int:3, int32:4, rune:5}"u8;
internal static readonly @string reflectTestTalias2Tint1ˢ = "reflect_test.Talias2{Tint:1, Tint2:2}"u8;

public static void TestAliasNames(ж<Δtesting.T> Ꮡt) {
    var t1 = new Talias1(@byte: 1, uint8: 2, @int: 3, int32: 4, rune: 5);
    @string @out = fmt.Sprintf("%#v"u8, t1);
    @string want = reflectTestTalias1Byteˢ;
    if (@out != want) {
        Ꮡt.Errorf("Talias1 print:\nhave: %s\nwant: %s"u8, @out, want);
    }
    var t2 = new Talias2(Tint: 1, Tint2: 2);
    @out = fmt.Sprintf("%#v"u8, t2);
    want = reflectTestTalias2Tint1ˢ;
    if (@out != want) {
        Ꮡt.Errorf("Talias2 print:\nhave: %s\nwant: %s"u8, @out, want);
    }
}

[GoType("dyn")] internal partial struct TestIssue22031_s {
    public nint C;
}

[GoLocalName("s")] [GoType("[]TestIssue22031_s")] internal partial struct TestIssue22031_sᴛ1;

[GoType("dyn")] internal partial struct TestIssue22031_t1 {
    internal partial ref TestIssue22031_sᴛ1 s { get; }
}

[GoType("dyn")] internal partial struct TestIssue22031_t2 {
    internal TestIssue22031_sᴛ1 f;
}

public static void TestIssue22031(ж<Δtesting.T> Ꮡt) {
    var tests = new reflectꓸValue[]{
        ValueOf(new TestIssue22031_t1(new TestIssue22031_sᴛ1(new TestIssue22031_s[]{new()}.slice()))).Field(0).Index(0).Field(0),
        ValueOf(new TestIssue22031_t2(new TestIssue22031_sᴛ1(new TestIssue22031_s[]{new()}.slice()))).Field(0).Index(0).Field(0)
    }.slice();
    foreach (var (i, test) in tests) {
        if (test.CanSet()) {
            Ꮡt.Errorf("%d: CanSet: got true, want false"u8, i);
        }
    }
}

[GoType("num:nint")] partial struct NonExportedFirst;

public static void ΦExported(this NonExportedFirst i) {
}

internal static nint nonexported(this NonExportedFirst i) {
    throw panic("wrong");
}

public static void TestIssue22073(ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var m = ValueOf(((NonExportedFirst)0)).Method(0);
    {
        nint got = m.Type().NumOut(); if (got != 0) {
            Ꮡt.Errorf("NumOut: got %v, want 0"u8, got);
        }
    }
    // Shouldn't panic.
    m.Call(default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string one1Three3Two2ˢ = @"[one: 1, three: 3, two: 2]"u8;

public static void TestMapIterNonEmptyMap(ж<Δtesting.T> Ꮡt) {
    var m = new map<@string, nint>{["one"u8] = 1, ["two"u8] = 2, ["three"u8] = 3};
    var iter = ValueOf(m).MapRange();
    {
        @string got = iterateToString(iter);
        @string want = one1Three3Two2ˢ; if (got != want) {
            Ꮡt.Errorf("iterator returned %s (after sorting), want %s"u8, got, want);
        }
    }
}

public static void TestMapIterNilMap(ж<Δtesting.T> Ꮡt) {
    map<@string, nint> m = default!;
    var iter = ValueOf(m).MapRange();
    {
        @string got = iterateToString(iter);
        @string want = @"[]"u8; if (got != want) {
            Ꮡt.Errorf("non-empty result iteratoring nil map: %s"u8, got);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object nextDidNotPanicˢ = (@string)"Next did not panic"u8;
internal static readonly @string one2Two3Threeˢ = @"[1: one, 2: two, 3: three]"u8;

public static void TestMapIterReset(ж<Δtesting.T> Ꮡt) {
    var iter = @new<Δreflect.MapIter>();
    // Use of zero iterator should panic.
    var iterʗ1 = iter;
    ((Action)(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                recover();
            }, ref ᒐ);
            iterʗ1.Next();
            Ꮡt.Error(nextDidNotPanicˢ);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }))();
    // Reset to new Map should work.
    var m = new map<@string, nint>{["one"u8] = 1, ["two"u8] = 2, ["three"u8] = 3};
    iter.Reset(ValueOf(m));
    {
        @string got = iterateToString(iter);
        @string want = one1Three3Two2ˢ; if (got != want) {
            Ꮡt.Errorf("iterator returned %s (after sorting), want %s"u8, got, want);
        }
    }
    // Reset to Zero value should work, but iterating over it should panic.
    iter.Reset(new reflectꓸValue(nil));
    var iterʗ2 = iter;
    ((Action)(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                recover();
            }, ref ᒐ);
            iterʗ2.Next();
            Ꮡt.Error(nextDidNotPanicˢ);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }))();
    // Reset to a different Map with different types should work.
    var m2 = new map<nint, @string>{[1] = "one"u8, [2] = "two"u8, [3] = "three"u8};
    iter.Reset(ValueOf(m2));
    {
        @string got = iterateToString(iter);
        @string want = one2Two3Threeˢ; if (got != want) {
            Ꮡt.Errorf("iterator returned %s (after sorting), want %s"u8, got, want);
        }
    }
    // Check that Reset, Next, and SetKey/SetValue play nicely together.
    var m3 = new map<uint64, uint64>{
        [((uint64)1 << (int)(0))] = ((uint64)1 << (int)(1)),
        [((uint64)1 << (int)(1))] = ((uint64)1 << (int)(2)),
        [((uint64)1 << (int)(2))] = ((uint64)1 << (int)(3))
    };
    var kv = New(TypeOf((uint64)0)).Elem();
    for (nint i = 0; i < 5; i++) {
        uint64 seenk = default!;
        uint64 seenv = default!;
        iter.Reset(ValueOf(m3));
        while (iter.Next()) {
            kv.SetIterKey(iter);
            seenk ^= (uint64)(kv.Uint());
            kv.SetIterValue(iter);
            seenv ^= (uint64)(kv.Uint());
        }
        if (seenk != 0b111) {
            Ꮡt.Errorf("iteration yielded keys %b, want %b"u8, seenk, (nint)(0b111));
        }
        if (seenv != 0b1110) {
            Ꮡt.Errorf("iteration yielded values %b, want %b"u8, seenv, (nint)(0b1110));
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
        iterʗ3.Reset(new reflectꓸValue(nil));
    });
    if (!asan.Enabled && n > 0) {
        Ꮡt.Errorf("MapIter.Reset allocated %d times"u8, n);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object keyDidNotPanicˢ = (@string)"Key did not panic"u8;
internal static readonly object valueDidNotPanicˢ = (@string)"Value did not panic"u8;

public static void TestMapIterSafety(ж<Δtesting.T> Ꮡt) {
    // Using a zero MapIter causes a panic, but not a crash.
    ((Action)(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                recover();
            }, ref ᒐ);
            @new<Δreflect.MapIter>().Key();
            Ꮡt.Fatal(keyDidNotPanicˢ);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }))();
    ((Action)(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                recover();
            }, ref ᒐ);
            @new<Δreflect.MapIter>().Value();
            Ꮡt.Fatal(valueDidNotPanicˢ);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }))();
    ((Action)(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                recover();
            }, ref ᒐ);
            @new<Δreflect.MapIter>().Next();
            Ꮡt.Fatal(nextDidNotPanicˢ);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }))();
    // Calling Key/Value on a MapIter before Next
    // causes a panic, but not a crash.
    map<@string, nint> m = default!;
    var iter = ValueOf(m).MapRange();
    var iterʗ1 = iter;
    ((Action)(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                recover();
            }, ref ᒐ);
            iterʗ1.Key();
            Ꮡt.Fatal(keyDidNotPanicˢ);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }))();
    var iterʗ2 = iter;
    ((Action)(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                recover();
            }, ref ᒐ);
            iterʗ2.Value();
            Ꮡt.Fatal(valueDidNotPanicˢ);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }))();
    // Calling Next, Key, or Value on an exhausted iterator
    // causes a panic, but not a crash.
    iter.Next(); // -> false
    var iterʗ3 = iter;
    ((Action)(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                recover();
            }, ref ᒐ);
            iterʗ3.Key();
            Ꮡt.Fatal(keyDidNotPanicˢ);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }))();
    var iterʗ4 = iter;
    ((Action)(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                recover();
            }, ref ᒐ);
            iterʗ4.Value();
            Ꮡt.Fatal(valueDidNotPanicˢ);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }))();
    var iterʗ5 = iter;
    ((Action)(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                recover();
            }, ref ᒐ);
            iterʗ5.Next();
            Ꮡt.Fatal(nextDidNotPanicˢ);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }))();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string oneˢ = "one"u8;
internal static readonly @string one1ˢ = @"[one: 1]"u8;

public static void TestMapIterNext(ж<Δtesting.T> Ꮡt) {
    // The first call to Next should reflect any
    // insertions to the map since the iterator was created.
    var m = new map<@string, nint>{};
    var iter = ValueOf(m).MapRange();
    m[oneˢ] = 1;
    {
        @string got = iterateToString(iter);
        @string want = one1ˢ; if (got != want) {
            Ꮡt.Errorf("iterator returned deleted elements: got %s, want %s"u8, got, want);
        }
    }
}

public static void TestMapIterDelete0(ж<Δtesting.T> Ꮡt) {
    // Delete all elements before first iteration.
    var m = new map<@string, nint>{["one"u8] = 1, ["two"u8] = 2, ["three"u8] = 3};
    var iter = ValueOf(m).MapRange();
    delete(m, "one"u8);
    delete(m, "two"u8);
    delete(m, "three"u8);
    {
        @string got = iterateToString(iter);
        @string want = @"[]"u8; if (got != want) {
            Ꮡt.Errorf("iterator returned deleted elements: got %s, want %s"u8, got, want);
        }
    }
}

public static void TestMapIterDelete1(ж<Δtesting.T> Ꮡt) {
    // Delete all elements after first iteration.
    var m = new map<@string, nint>{["one"u8] = 1, ["two"u8] = 2, ["three"u8] = 3};
    var iter = ValueOf(m).MapRange();
    slice<@string> got = default!;
    while (iter.Next()) {
        got = append(got, fmt.Sprint(iter.Key(), iter.Value()));
        delete(m, "one"u8);
        delete(m, "two"u8);
        delete(m, "three"u8);
    }
    if (len(got) != 1) {
        Ꮡt.Errorf("iterator returned wrong number of elements: got %d, want 1"u8, len(got));
    }
}

// iterateToString returns the set of elements
// returned by an iterator in readable form.
internal static @string iterateToString(ж<Δreflect.MapIter> Ꮡit) {
    slice<@string> got = default!;
    while (Ꮡit.Next()) {
        @string line = fmt.Sprintf("%v: %v"u8, Ꮡit.Key(), Ꮡit.Value());
        got = append(got, line);
    }
    slices.Sort<slice<@string>, @string>(got);
    return "["u8 + strings.Join(got, ", "u8) + "]"u8;
}

public static void TestConvertibleTo(ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var t1 = ValueOf(new example1.MyStruct(nil)).Type();
    var t2 = ValueOf(new example2.MyStruct(nil)).Type();
    // Shouldn't raise stack overflow
    if (t1.ConvertibleTo(t2)) {
        Ꮡt.Fatalf("(%s).ConvertibleTo(%s) = true, want false"u8, t1, t2);
    }
    var t3 = ValueOf(new example1.MyStruct[]{}.slice()).Type();
    var t4 = ValueOf(new example2.MyStruct[]{}.slice()).Type();
    if (t3.ConvertibleTo(t4)) {
        Ꮡt.Fatalf("(%s).ConvertibleTo(%s) = true, want false"u8, t3, t4);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string valueSetIterKeyCalledˢ = "Value.SetIterKey called before Next"u8;
internal static readonly @string valueSetIterValueCalledˢ = "Value.SetIterValue called before Next"u8;
internal static readonly @string valueSetIterKeyCalledOnˢ = "Value.SetIterKey called on exhausted iterator"u8;
internal static readonly @string valueSetIterValueCalledˢ2 = "Value.SetIterValue called on exhausted iterator"u8;
internal static readonly @string valueSetIterKeyUsingˢ = "Value.SetIterKey using unaddressable value"u8;
internal static readonly @string valueSetIterValueUsingˢ = "Value.SetIterValue using unaddressable value"u8;
internal static readonly @string valueOfTypeStringIsNotˢ2 = "value of type string is not assignable to type int"u8;
internal static readonly @string valueOfTypeIntIsNotˢ = "value of type int is not assignable to type string"u8;
internal static readonly @string usingValueObtainedUsingˢ = "using value obtained using unexported field"u8;

[GoType("dyn")] internal partial struct TestSetIter_i {
    internal map<@string, nint> m;
}

public static void TestSetIter(ж<Δtesting.T> Ꮡt) {
    var data = new map<@string, nint>{
        ["foo"u8] = 1,
        ["bar"u8] = 2,
        ["baz"u8] = 3
    };
    var m = ValueOf(data);
    ref var i = ref heap<ж<Δreflect.MapIter>>(out var Ꮡi);
    i = m.MapRange();
    ref var k = ref heap<reflectꓸValue>(out var Ꮡk);
    k = New(TypeOf((@string)""u8)).Elem();
    ref var v = ref heap<reflectꓸValue>(out var Ꮡv);
    v = New(TypeOf((nint)(0))).Elem();
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
        Ꮡt.Errorf("maps not equal, got %v want %v"u8, data2, data);
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
        ValueOf((nint)(0)).SetIterValue(Ꮡi.ValueSlot);
    });
    shouldPanic(valueOfTypeStringIsNotˢ2, () => {
        New(TypeOf((nint)(0))).Elem().SetIterKey(Ꮡi.ValueSlot);
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
            Ꮡt.Errorf("got key %s which is not in map"u8, x);
        }
    }
    y.SetIterValue(i);
    if (x._<nint>() < 1 || x._<nint>() > 3) {
        Ꮡt.Errorf("got value %d which is not in map"u8, x);
    }
    // Try some key/value types which are direct interfaces.
    ref var a = ref heap<nint>(out var Ꮡa);
    a = 88;
    ref var b = ref heap<nint>(out var Ꮡb);
    b = 99;
    var pp = new map<ж<nint>, ж<nint>>{
        [Ꮡa] = Ꮡb
    };
    i = ValueOf(pp).MapRange();
    i.Next();
    y.SetIterKey(i);
    {
        nint got = y.Interface()._<ж<nint>>().Value; if (got != a) {
            Ꮡt.Errorf("pointer incorrect: got %d want %d"u8, got, a);
        }
    }
    y.SetIterValue(i);
    {
        nint got = y.Interface()._<ж<nint>>().Value; if (got != b) {
            Ꮡt.Errorf("pointer incorrect: got %d want %d"u8, got, b);
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

public static void TestMethodCallValueCodePtr(ж<Δtesting.T> Ꮡt) {
    var m = ValueOf(new Point(nil)).Method(1);
    var want = reflect_internal_test_package.MethodValueCallCodePtr();
    {
        var got = (uintptr)(uintptr)m.UnsafePointer(); if (got != want) {
            Ꮡt.Errorf("methodValueCall code pointer mismatched, want: %v, got: %v"u8, want, got);
        }
    }
    {
        var got = m.Pointer(); if (got != want) {
            Ꮡt.Errorf("methodValueCall code pointer mismatched, want: %v, got: %v"u8, want, got);
        }
    }
}

[GoType] partial struct A {
}

[GoType] partial struct B<T> {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string bReflectTestAˢ = "B[reflect_test.A]"u8;
internal static readonly @string bReflectTestBReflectTestˢ = "B[reflect_test.B[reflect_test.A]]"u8;

public static void TestIssue50208(ж<Δtesting.T> Ꮡt) {
    @string want1 = bReflectTestAˢ;
    {
        @string got = TypeOf(@new<B<A>>()).Elem().Name(); if (got != want1) {
            Ꮡt.Errorf("name of type parameter mismatched, want:%s, got:%s"u8, want1, got);
        }
    }
    @string want2 = bReflectTestBReflectTestˢ;
    {
        @string got = TypeOf(@new<B<B<A>>>()).Elem().Name(); if (got != want2) {
            Ꮡt.Errorf("name of type parameter mismatched, want:%s, got:%s"u8, want2, got);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string kind1ˢ = "kind-1"u8;

public static void TestNegativeKindString(ж<Δtesting.T> Ꮡt) {
    nint x = -1;
    @string s = ((reflectꓸKind)(nuint)x).String();
    @string want = kind1ˢ;
    if (s != want) {
        Ꮡt.Fatalf("Kind(-1).String() = %q, want %q"u8, s, want);
    }
}

[GoType("bool")] partial struct namedBool;

[GoType("[]byte")] partial struct namedBytes;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string reflectCallOfReflectˢ = "reflect: call of reflect.Value.Cap on ptr to non-array Value"u8;

public static void TestValue_Cap(ж<Δtesting.T> Ꮡt) {
    var a = Ꮡ(new nint[]{1, 2, 3}.array());
    var v = ValueOf(a.OrTypedNil());
    if (v.Cap() != 3) {
        Ꮡt.Errorf("Cap = %d want %d"u8, v.Cap(), 3);
    }
    a = ж<array<nint>>.NilBoxOfDims(3L);
    v = ValueOf(a.OrTypedNil());
    if (v.Cap() != 3) {
        Ꮡt.Errorf("Cap = %d want %d"u8, v.Cap(), 3);
    }
    @string /*errorStr*/ getError(Action f) {
        @string errorStr = default!;
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
        ж<nint> ptr = default!;
        ValueOf(ptr.OrTypedNil()).Cap();
    });
    @string wantStr = reflectCallOfReflectˢ;
    if (e != wantStr) {
        Ꮡt.Errorf("error is %q, want %q"u8, e, wantStr);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string reflectCallOfReflectˢ2 = "reflect: call of reflect.Value.Len on ptr to non-array Value"u8;

public static void TestValue_Len(ж<Δtesting.T> Ꮡt) {
    var a = Ꮡ(new nint[]{1, 2, 3}.array());
    var v = ValueOf(a.OrTypedNil());
    if (v.Len() != 3) {
        Ꮡt.Errorf("Len = %d want %d"u8, v.Len(), 3);
    }
    a = ж<array<nint>>.NilBoxOfDims(3L);
    v = ValueOf(a.OrTypedNil());
    if (v.Len() != 3) {
        Ꮡt.Errorf("Len = %d want %d"u8, v.Len(), 3);
    }
    @string /*errorStr*/ getError(Action f) {
        @string errorStr = default!;
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
        ж<nint> ptr = default!;
        ValueOf(ptr.OrTypedNil()).Len();
    });
    @string wantStr = reflectCallOfReflectˢ2;
    if (e != wantStr) {
        Ꮡt.Errorf("error is %q, want %q"u8, e, wantStr);
    }
}

[GoType("dyn")] internal partial struct TestValue_Comparable_type {
    public any I;
}

[GoType("dyn")] internal partial struct TestValue_Comparable_typeᴛ1 {
    internal reflectꓸValue value;
    internal bool comparable;
    internal bool deref;
}

[GoType("dyn")] internal partial struct TestValue_Comparable_typeᴛ2 {
    public nint I;
}

public static void TestValue_Comparable(ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    ref var a = ref heap(new nint(), out var Ꮡa);
    ref var s = ref heap<slice<nint>>(out var Ꮡs);
    ref var i = ref heap<any>(out var Ꮡi);

    i = a;
    ref var iNil = ref heap<any>(out var ᏑiNil);
    ref var iSlice = ref heap<any>(out var ᏑiSlice);

    iSlice = s;
    ref var iArrayFalse = ref heap<any>(out var ᏑiArrayFalse);

    iArrayFalse = new any[]{(nint)(1), new map<nint, nint>{}}.array();
    ref var iArrayTrue = ref heap<any>(out var ᏑiArrayTrue);

    iArrayTrue = new any[]{(nint)(1), new TestValue_Comparable_type((nint)(1))}.array();
    slice<TestValue_Comparable_typeᴛ1> testcases = new TestValue_Comparable_typeᴛ1[]{
        new(
            ValueOf(ᏑiNil),
            true,
            true
        ),
        new(
            ValueOf((nint)(32)),
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
            ValueOf(new nint[]{}.slice()),
            false,
            false
        ),
        new(
            new reflectꓸValue(nil),
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
            ValueOf(new TestValue_Comparable_type[]{new((nint)(1)), new((nint)(1))}.array()),
            true,
            false
        ),
        new(
            ValueOf(new TestValue_Comparable_type[]{new(new nint[]{}.slice()), new((nint)(1))}.array()),
            false,
            false
        ),
        new(
            ValueOf(new any[]{(nint)(1), new TestValue_Comparable_typeᴛ2(1)}.array()),
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
    }.slice();
    foreach (var (_, cas) in testcases) {
        var v = cas.value;
        if (cas.deref) {
            v = v.Elem();
        }
        var got = v.Comparable();
        if (got != cas.comparable) {
            Ꮡt.Errorf("%T.Comparable = %t, want %t"u8, v, got, cas.comparable);
        }
    }
}

[GoType] partial struct ValueEqualTest {
    internal any v, u;
    internal bool eq;
    internal bool vDeref, uDeref;
}

internal static ж<any> ᏑequalI = new StandardBox<any>((nint)(1));
internal static ref any equalI => ref ᏑequalI.ValueSlot;

internal static any equalSlice = new nint[]{1}.slice();

internal static ж<any> ᏑnilInterface = new StandardBox<any>(default(any));
internal static ref any nilInterface => ref ᏑnilInterface.ValueSlot;

internal static any mapInterface = new map<nint, nint>{};


        [GoType("dyn")] partial struct Δtypeᴛ36 {
            internal nint i;
        }
internal static slice<ValueEqualTest> valueEqualTests = new ValueEqualTest[]{
    new(
        new reflectꓸValue(nil), new reflectꓸValue(nil),
        true,
        false, false
    ),
    new(
        true, true,
        true,
        false, false
    ),
    new(
        (nint)(1), (nint)(1),
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
        }, default!,
        false,
        false, false
    ),
    new(
        ᏑequalI, (nint)(1),
        true,
        true, false
    ),
    new(
        (channel<nint>)(default!), default!,
        false,
        false, false
    ),
    new(
        (channel<nint>)(default!), (channel<nint>)(default!),
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
        (nint)(1), ValueOf(new Δtypeᴛ36(1)).Field(0),
        true,
        false, false
    )
}.slice();

public static void TestValue_Equal(ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (_, test) in valueEqualTests) {
        reflectꓸValue v = new(nil);
        reflectꓸValue u = new(nil);
        {
            var (vv, ok) = test.v._<reflectꓸValue>(ᐧ); if (ok){
                v = vv;
            } else {
                v = ValueOf(test.v);
            }
        }
        {
            var (uu, ok) = test.u._<reflectꓸValue>(ᐧ); if (ok){
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
                Ꮡt.Errorf("%s == %s got %t, want %t"u8, v.Type(), u.Type(), r, test.eq);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string areNotComparableˢ = "are not comparable"u8;

[GoType("dyn")] internal partial struct TestValue_EqualNonComparable_type {
    public any I;
}

public static void TestValue_EqualNonComparable(ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    reflectꓸValue invalid = new reflectꓸValue(nil);                // ValueOf(nil)
// Value of slice is non-comparable.
// Value of map is non-comparable.
// Value of func is non-comparable.
// Value of struct is non-comparable because of non-comparable elements.
// Value of array is non-comparable because of non-comparable elements.
    slice<reflectꓸValue> values = new reflectꓸValue[]{
        ValueOf(slice<nint>(default!)),
        ValueOf((new nint[]{}.slice())),
        ValueOf(((map<nint, nint>)default!)),
        ValueOf((new map<nint, nint>{})),
        ValueOf((((Action)(default!))).OrTypedNilFunc()),
        ValueOf(() => {
        }),
        ValueOf((new NonComparableStruct(nil))),
        ValueOf(new map<nint, nint>[]{}.array()),
        ValueOf(new Action[]{}.array()),
        ValueOf((new TestValue_EqualNonComparable_type[]{new(new nint[]{}.slice())}.array())),
        ValueOf((new any[]{new any[]{new map<nint, nint>{}}.array()}.array()))
    }.slice();
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
                Ꮡt.Errorf("%s == invalid got %t, want false"u8, value.Type(), r);
            }
        }
    }
}

[MethodImpl(MethodImplOptions.NoInlining)] public static void TestInitFuncTypes(ж<Δtesting.T> Ꮡt) {
    nint n = 100;
    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    Ꮡwg.Add(n);
    for (nint i = 0; i < n; i++) {
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                var ipT = TypeOf(new Δnet.IP(new byte[]{}.slice()));
                for (nint iΔ1 = 0; iΔ1 < ipT.NumMethod(); iΔ1++) {
                    _ = ipT.Method(iΔ1);
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
    Ꮡwg.Wait();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string reflectValueClearˢ = "reflect.Value.Clear"u8;

[GoType("dyn")] internal partial struct TestClear_tests {
    internal @string name;
    internal reflectꓸValue value;
    internal Func<reflectꓸValue, bool> testFunc;
}

public static void TestClear(ж<Δtesting.T> Ꮡt) {
    var m = new map<@string, any>(len(valueTests));
    foreach (var (_, tt) in valueTests) {
        m[tt.s] = tt.i;
    }
    var mapTestFn = (reflectꓸValue v) => {
        v.Clear();
        return v.Len() == 0;
    };
    var s = new slice<ж<pair>>(len(valueTests));
    foreach (var (i, _) in s) {
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
    var tests = new TestClear_tests[]{
        new("map"u8, ValueOf(m), mapTestFn),
        new("slice no pointer"u8, ValueOf(new nint[]{1, 2, 3, 4, 5}.slice()), sliceTestFn),
        new("slice has pointer"u8, ValueOf(s), sliceTestFn),
        new("non-map/slice"u8, ValueOf((nint)(1)), panicTestFn)
    }.slice();
    foreach (var (_, tc) in tests) {
        ref var tcΔ1 = ref heap<TestClear_tests>(out var ᏑtcΔ1);
        tcΔ1 = tc;
        var tcʗ1 = tcΔ1;
        Ꮡt.Run(tcΔ1.name, (ж<Δtesting.T> tΔ1) => {
            tΔ1.Parallel();
            if (!tcʗ1.testFunc(tcʗ1.value)) {
                tΔ1.Errorf("unexpected result for value.Clear(): %v"u8, tcʗ1.value);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fooˢ = "foo"u8;

[GoType("dyn")] internal partial struct TestValuePointerAndUnsafePointer_tests {
    internal @string name;
    internal reflectꓸValue val;
    internal @unsafe.Pointer wantUnsafePointer;
}

public static void TestValuePointerAndUnsafePointer(ж<Δtesting.T> Ꮡt) {
    var ptr = @new<nint>();
    ref var ch = ref heap<channel<nint>>(out var Ꮡch);
    ch = new channel<nint>(0);
    ref var m = ref heap<map<nint, nint>>(out var Ꮡm);
    m = new map<nint, nint>();
    @unsafe.Pointer unsafePtr = @unsafe.Pointer.FromPinnedBox(ptr);
    var Δslice = new slice<nint>(1);
    ref var fn = ref heap<Action>(out var Ꮡfn);
    fn = () => {
    };
    @string s = fooˢ;
    var tests = new TestValuePointerAndUnsafePointer_tests[]{
        new("pointer"u8, ValueOf(ptr.OrTypedNil()), @unsafe.Pointer.FromPinnedBox(ptr)),
        new("channel"u8, ValueOf(ch), ~Ꮡ(new @unsafe.Pointer((uintptr)Ꮡch))),
        new("map"u8, ValueOf(m), ~Ꮡ(new @unsafe.Pointer((uintptr)Ꮡm))),
        new("unsafe.Pointer"u8, ValueOf(@unsafe.Pointer.OrTypedNil(unsafePtr)), unsafePtr.Value),
        new("function"u8, ValueOf((fn).OrTypedNilFunc()), (~Ꮡ(Ꮡ(new @unsafe.Pointer((uintptr)Ꮡfn)))).Value),
        new("slice"u8, ValueOf(Δslice), @unsafe.Pointer.FromPinnedBox(@unsafe.SliceData(Δslice))),
        new("string"u8, ValueOf(s), @unsafe.Pointer.FromPinnedBox(@unsafe.StringData(s)))
    }.slice();
    foreach (var (_, tc) in tests) {
        ref var tcΔ1 = ref heap<TestValuePointerAndUnsafePointer_tests>(out var ᏑtcΔ1);
        tcΔ1 = tc;
        var tcʗ1 = tcΔ1;
        Ꮡt.Run(tcΔ1.name, (ж<Δtesting.T> tΔ1) => {
            {
                var got = tcʗ1.val.Pointer(); if (got != (uintptr)tcʗ1.wantUnsafePointer) {
                    tΔ1.Errorf("unexpected uintptr result, got %#x, want %#x"u8, got, (uintptr)tcʗ1.wantUnsafePointer);
                }
            }
            {
                @unsafe.Pointer got = (uintptr)tcʗ1.val.UnsafePointer(); if (got != tcʗ1.wantUnsafePointer) {
                    tΔ1.Errorf("unexpected unsafe.Pointer result, got %#x, want %#x"u8, @unsafe.Pointer.OrTypedNil(got), @unsafe.Pointer.OrTypedNil(tcʗ1.wantUnsafePointer));
                }
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object nilPointerWithZeroLengthˢ = (@string)"nil pointer with zero length must return nil"u8;

// Hoisted Go big-integer constant (single parse; Go folds constants at compile time)
private static readonly GoBigConst maxUintptrᶜ = GoBigConst.Parse("18446744073709551616");

// Test cases copied from ../../test/unsafebuiltins.go
public static void TestSliceAt(ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    GoBigConst maxUintptr = /* 1 << (8 * unsafe.Sizeof(uintptr(0))) */
            maxUintptrᶜ;
    ref var p = ref heap(new array<byte>(10), out var Ꮡp);
    var typ = TypeOf(p[0]);
    var s = SliceAt(typ, @unsafe.Pointer.FromPinnedBox(Ꮡp.at<byte>(0)), len(p));
    if (s.Pointer() != (uintptr)Ꮡp.at<byte>(0)) {
        Ꮡt.Fatalf("unexpected underlying array: %d, want: %d"u8, s.Pointer(), (uintptr)Ꮡp.at<byte>(0));
    }
    if (s.Len() != len(p) || s.Cap() != len(p)) {
        Ꮡt.Fatalf("unexpected len or cap, len: %d, cap: %d, want: %d"u8, s.Len(), s.Cap(), len(p));
    }
    typ = TypeOf((nint)(0));
    if (!SliceAt(typ, @unsafe.Pointer.FromPinnedBox(((ж<nint>)nil)), 0).IsNil()) {
        Ꮡt.Fatal(nilPointerWithZeroLengthˢ);
    }
    // nil pointer with positive length panics
    var typʗ1 = typ;
    shouldPanic(""u8, () => {
        _ = SliceAt(typʗ1, @unsafe.Pointer.FromPinnedBox(((ж<nint>)nil)), 1);
    });
    // negative length
    nint neg = -1;
    shouldPanic(""u8, () => {
        _ = SliceAt(TypeOf((byte)0), @unsafe.Pointer.FromPinnedBox(Ꮡp.at<byte>(0)), neg);
    });
    // size overflows address space
    ref var n = ref heap<uint64>(out var Ꮡn);
    n = (uint64)0;
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
public static void TestMapOfKeyUpdate(ж<Δtesting.T> Ꮡt) {
    var m = MakeMap(MapOf(TypeFor<float64>(), TypeFor<bool>()));
    var zero = (float64)0.0D;
    var negZero = Δmath.Copysign(zero, -1.0D);
    m.SetMapIndex(ValueOf(zero), ValueOf(true));
    m.SetMapIndex(ValueOf(negZero), ValueOf(true));
    if (m.Len() != 1) {
        Ꮡt.Errorf("map length got %d want 1"u8, m.Len());
    }
    var iter = m.MapRange();
    while (iter.Next()) {
        var k = iter.Key().Float();
        if (Δmath.Copysign(1.0D, k) > 0D) {
            Ꮡt.Errorf("map key %f has positive sign"u8, k);
        }
    }
}

// Test that maps created with MapOf properly panic on unhashable keys, even if
// the map is empty. (i.e., it sets the hash might panic flag in the map).
//
// This test is a simplified version of runtime.TestEmptyMapWithInterfaceKey
// for reflect.
public static void TestMapOfKeyPanic(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        defer(() => {
            var r = recover();
            if (r == default!) {
                Ꮡt.Errorf("didn't panic"u8);
            }
        }, ref ᒐ);
        var m = MakeMap(MapOf(TypeFor<any>(), TypeFor<bool>()));
        slice<nint> Δslice = default!;
        m.MapIndex(ValueOf(Δslice));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end reflect_test_package
