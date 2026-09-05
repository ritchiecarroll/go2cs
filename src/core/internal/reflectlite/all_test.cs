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
using encoding;
using global::go.@internal;
using reflectlite = global::go.@internal.reflectlite_package;
using static global::go.@internal.reflectlite_internal_test_package;
using ꓸꓸꓸPoint = Span<reflectlite_test_package.Point>;

partial class reflectlite_test_package {

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸencodingꓸbase64() {
    builtin.initPackage(typeof(encoding.base64_package));
}

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸfmt() {
    builtin.initPackage(typeof(fmt_package));
}

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸinternalꓸabi() {
    builtin.initPackage(typeof(global::go.@internal.abi_package));
}

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸinternalꓸreflectlite() {
    builtin.initPackage(typeof(global::go.@internal.reflectlite_package));
}

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸmath() {
    builtin.initPackage(typeof(math_package));
}

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸreflect() {
    builtin.initPackage(typeof(reflect_package));
}

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸruntime() {
    builtin.initPackage(typeof(runtime_package));
}

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸtesting() {
    builtin.initPackage(typeof(testing_package));
}

public static reflectꓸValue ToValue(reflectlite.Value v) {
    return reflect.ValueOf(reflectlite_internal_test_package.ToInterface(v));
}

public static @string TypeString(reflectliteꓸType t) {
    return fmt.Sprintf("%T"u8, reflectlite_internal_test_package.ToInterface(reflectlite_internal_test_package.Zero(t)));
}

[GoType("num:nint")] partial struct integer;

[GoType] partial struct T {
    internal nint a;
    internal float64 b;
    internal @string c;
    internal ж<nint> d;
}

[GoType] partial struct pair {
    internal any i;
    internal @string s;
}

internal static void assert(ж<testing.T> Ꮡt, @string s, @string want) {
    Ꮡt.Helper();
    if (s != want) {
        Ꮡt.Errorf("have %#q want %#q"u8, s, want);
    }
}

// {struct {
// 	x (interface {
// 		a(func(func(int) int) func(func(int)) int)
// 		b()
// 	})
// }{},
// 	"interface { reflectlite_test.a(func(func(int) int) func(func(int)) int); reflectlite_test.b() }",
// },

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

    [GoType("dyn")] partial struct typeᴛ18_x {
        internal channel<ж<int32>> c;
        internal float32 d;
    }

    [GoType("dyn")] partial struct Δtypeᴛ18 {
        internal typeᴛ18_x x;
    }

    [GoType("dyn")] partial struct Δtypeᴛ19 {
        internal Action<int8, int32> x;
    }

    [GoType("dyn")] partial struct typeᴛ20_x {
        internal Action<channel<ж<integer>>, ж<int8>> c;
    }

    [GoType("dyn")] partial struct Δtypeᴛ20 {
        internal typeᴛ20_x x;
    }

    [GoType("dyn")] partial struct typeᴛ21_x {
        internal int8 a;
        internal int32 b;
    }

    [GoType("dyn")] partial struct Δtypeᴛ21 {
        internal typeᴛ21_x x;
    }

    [GoType("dyn")] partial struct typeᴛ22_x {
        internal int8 a;
        internal int8 b;
        internal int32 c;
    }

    [GoType("dyn")] partial struct Δtypeᴛ22 {
        internal typeᴛ22_x x;
    }

    [GoType("dyn")] partial struct typeᴛ23_x {
        internal int8 a;
        internal int8 b;
        internal int8 c;
        internal int32 d;
    }

    [GoType("dyn")] partial struct Δtypeᴛ23 {
        internal typeᴛ23_x x;
    }

    [GoType("dyn")] partial struct typeᴛ24_x {
        internal int8 a;
        internal int8 b;
        internal int8 c;
        internal int8 d;
        internal int32 e;
    }

    [GoType("dyn")] partial struct Δtypeᴛ24 {
        internal typeᴛ24_x x;
    }

    [GoType("dyn")] partial struct typeᴛ25_x {
        internal int8 a;
        internal int8 b;
        internal int8 c;
        internal int8 d;
        internal int8 e;
        internal int32 f;
    }

    [GoType("dyn")] partial struct Δtypeᴛ25 {
        internal typeᴛ25_x x;
    }

    [GoType("dyn")] partial struct typeᴛ26_x {
        [GoTag(@"reflect:""hi there""")]
        internal int8 a;
    }

    [GoType("dyn")] partial struct Δtypeᴛ26 {
        internal typeᴛ26_x x;
    }

    [GoType("dyn")] partial struct typeᴛ27_x {
        [GoTag(@"reflect:""hi \x00there\t\n\""\\""")]
        internal int8 a;
    }

    [GoType("dyn")] partial struct Δtypeᴛ27 {
        internal typeᴛ27_x x;
    }

    [GoType("dyn")] partial struct typeᴛ28_x {
        internal Actionꓸꓸꓸ<nint> f;
    }

    [GoType("dyn")] partial struct Δtypeᴛ28 {
        internal typeᴛ28_x x;
    }

    [GoType("dyn")] partial struct typeᴛ29_x {
        [GoEmbedded] internal int32 int32;
        [GoEmbedded] internal int64 int64;
    }

    [GoType("dyn")] partial struct Δtypeᴛ29 {
        internal typeᴛ29_x x;
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
    new(new Δtypeᴛ13(), "**reflectlite_test.integer"u8),
    new(new Δtypeᴛ14(), "[32]int32"u8),
    new(new Δtypeᴛ15(), "[]int8"u8),
    new(new Δtypeᴛ16(), "map[string]int32"u8),
    new(new Δtypeᴛ17(), "chan<- string"u8),
    new(new Δtypeᴛ18(),
        "struct { c chan *int32; d float32 }"u8
    ),
    new(new Δtypeᴛ19(), "func(int8, int32)"u8),
    new(new Δtypeᴛ20(),
        "struct { c func(chan *reflectlite_test.integer, *int8) }"u8
    ),
    new(new Δtypeᴛ21(),
        "struct { a int8; b int32 }"u8
    ),
    new(new Δtypeᴛ22(),
        "struct { a int8; b int8; c int32 }"u8
    ),
    new(new Δtypeᴛ23(),
        "struct { a int8; b int8; c int8; d int32 }"u8
    ),
    new(new Δtypeᴛ24(),
        "struct { a int8; b int8; c int8; d int8; e int32 }"u8
    ),
    new(new Δtypeᴛ25(),
        "struct { a int8; b int8; c int8; d int8; e int8; f int32 }"u8
    ),
    new(new Δtypeᴛ26(),
        @"struct { a int8 ""reflect:\""hi there\"""" }"u8
    ),
    new(new Δtypeᴛ27(),
        @"struct { a int8 ""reflect:\""hi \\x00there\\t\\n\\\""\\\\\"""" }"u8
    ),
    new(new Δtypeᴛ28(),
        "struct { f func(...int) }"u8
    ),
    new(new Δtypeᴛ29(),
        "struct { int32; int64 }"u8
    )
}.slice();

internal static slice<pair> valueTests = new pair[]{
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
    new(@new<ж<ж<integer>>>(), "**reflectlite_test.integer(0)"u8),
    new(@new<map<@string, int32>>(), "map[string]int32{<can't iterate on maps>}"u8),
    new(Ꮡ(channel/*<-*/<@string>.SendOnly), "chan<- string"u8),
    new(@new<Action<int8, int32>>(), "func(int8, int32)(arg)"u8),
    new(@new<typeᴛ18_x>(),
        "struct { c chan *int32; d float32 }{chan *int32, 0}"u8
    ),
    new(@new<typeᴛ20_x>(),
        "struct { c func(chan *reflectlite_test.integer, *int8) }{func(chan *reflectlite_test.integer, *int8)(arg)}"u8
    ),
    new(@new<typeᴛ21_x>(),
        "struct { a int8; b int32 }{0, 0}"u8
    ),
    new(@new<typeᴛ22_x>(),
        "struct { a int8; b int8; c int32 }{0, 0, 0}"u8
    )
}.slice();

internal static void testType(ж<testing.T> Ꮡt, nint i, reflectliteꓸType typ, @string want) {
    @string s = TypeString(typ);
    if (s != want) {
        Ꮡt.Errorf("#%d: have %#q, want %#q"u8, i, s, want);
    }
}

internal static void testReflectType(ж<testing.T> Ꮡt, nint i, reflectliteꓸType typ, @string want) {
    @string s = TypeString(typ);
    if (s != want) {
        Ꮡt.Errorf("#%d: have %#q, want %#q"u8, i, s, want);
    }
}

public static void TestTypes(ж<testing.T> Ꮡt) {
    foreach (var (i, tt) in typeTests) {
        testReflectType(Ꮡt, i, reflectlite_internal_test_package.Field(ValueOf(tt.i), 0).Type(), tt.s);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object stringyCheeseˢ = (@string)"stringy cheese"u8;

public static void TestSetValue(ж<testing.T> Ꮡt) {
    foreach (var (i, tt) in valueTests) {
        var v = ValueOf(tt.i).Elem();
        var exprᴛ1 = v.Kind();
        if (exprᴛ1 == abi.Int) {
            v.Set(ValueOf((nint)132));
        }
        else if (exprᴛ1 == abi.Int8) {
            v.Set(ValueOf((int8)8));
        }
        else if (exprᴛ1 == abi.Int16) {
            v.Set(ValueOf((int16)16));
        }
        else if (exprᴛ1 == abi.Int32) {
            v.Set(ValueOf((int32)32));
        }
        else if (exprᴛ1 == abi.Int64) {
            v.Set(ValueOf((int64)64));
        }
        else if (exprᴛ1 == abi.Uint) {
            v.Set(ValueOf((nuint)132));
        }
        else if (exprᴛ1 == abi.Uint8) {
            v.Set(ValueOf((uint8)8));
        }
        else if (exprᴛ1 == abi.Uint16) {
            v.Set(ValueOf((uint16)16));
        }
        else if (exprᴛ1 == abi.Uint32) {
            v.Set(ValueOf((uint32)32));
        }
        else if (exprᴛ1 == abi.Uint64) {
            v.Set(ValueOf((uint64)64));
        }
        else if (exprᴛ1 == abi.Float32) {
            v.Set(ValueOf((float32)256.25F));
        }
        else if (exprᴛ1 == abi.Float64) {
            v.Set(ValueOf(512.125D));
        }
        else if (exprᴛ1 == abi.Complex64) {
            v.Set(ValueOf((complex64)(532.125F + 10F.i())));
        }
        else if (exprᴛ1 == abi.Complex128) {
            v.Set(ValueOf((complex128)(564.25D + 1D.i())));
        }
        else if (exprᴛ1 == abi.ΔString) {
            v.Set(ValueOf(stringyCheeseˢ));
        }
        else if (exprᴛ1 == abi.Bool) {
            v.Set(ValueOf(true));
        }

        @string s = valueToString(v);
        if (s != tt.s) {
            Ꮡt.Errorf("#%d: have %#q, want %#q"u8, i, s, tt.s);
        }
    }
}

[GoType("dyn")] partial struct TestCanSetField_embed {
    internal nint x;
    public nint X;
}

[GoType("dyn")] partial struct TestCanSetField_Embed {
    internal nint x;
    public nint X;
}

[GoType("dyn")] partial struct TestCanSetField_S1 {
    internal partial ref TestCanSetField_embed embed { get; }
    internal nint x;
    public nint X;
}

[GoType("dyn")] partial struct TestCanSetField_S2 {
    internal partial ref ж<TestCanSetField_embed> embed { get; }
    internal nint x;
    public nint X;
}

[GoType("dyn")] partial struct TestCanSetField_S3 {
    public partial ref TestCanSetField_Embed Embed { get; }
    internal nint x;
    public nint X;
}

[GoType("dyn")] partial struct TestCanSetField_S4 {
    public partial ref ж<TestCanSetField_Embed> Embed { get; }
    internal nint x;
    public nint X;
}

[GoType("dyn")] partial struct TestCanSetField_testCase {
    internal slice<nint> index;
    internal bool canSet;
}

[GoType("dyn")] partial struct TestCanSetField_tests {
    internal reflectlite.Value val;
    internal slice<TestCanSetField_testCase> cases;
}

public static void TestCanSetField(ж<testing.T> Ꮡt) {
    var tests = new TestCanSetField_tests[]{new(
        val: ValueOf(Ꮡ(new TestCanSetField_S1(nil))),
        cases: new TestCanSetField_testCase[]{
            new(new nint[]{0}.slice(), false),
            new(new nint[]{0, 0}.slice(), false),
            new(new nint[]{0, 1}.slice(), true),
            new(new nint[]{1}.slice(), false),
            new(new nint[]{2}.slice(), true)
        }.slice()
    ), new(
        val: ValueOf(Ꮡ(new TestCanSetField_S2(embed: Ꮡ(new TestCanSetField_embed(nil))))),
        cases: new TestCanSetField_testCase[]{
            new(new nint[]{0}.slice(), false),
            new(new nint[]{0, 0}.slice(), false),
            new(new nint[]{0, 1}.slice(), true),
            new(new nint[]{1}.slice(), false),
            new(new nint[]{2}.slice(), true)
        }.slice()
    ), new(
        val: ValueOf(Ꮡ(new TestCanSetField_S3(nil))),
        cases: new TestCanSetField_testCase[]{
            new(new nint[]{0}.slice(), true),
            new(new nint[]{0, 0}.slice(), false),
            new(new nint[]{0, 1}.slice(), true),
            new(new nint[]{1}.slice(), false),
            new(new nint[]{2}.slice(), true)
        }.slice()
    ), new(
        val: ValueOf(Ꮡ(new TestCanSetField_S4(Embed: Ꮡ(new TestCanSetField_Embed(nil))))),
        cases: new TestCanSetField_testCase[]{
            new(new nint[]{0}.slice(), true),
            new(new nint[]{0, 0}.slice(), false),
            new(new nint[]{0, 1}.slice(), true),
            new(new nint[]{1}.slice(), false),
            new(new nint[]{2}.slice(), true)
        }.slice()
    )
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestCanSetField_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(tt.val.Type().Name(), (ж<testing.T> tΔ1) => {
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
    new(new T(123, 456.75D, "hello"u8, Ꮡ_i), "reflectlite_test.T{123, 456.75, hello, *int(&7)}"u8),
    new(@new<channel<ж<T>>>(), "*chan *reflectlite_test.T(&chan *reflectlite_test.T)"u8),
    new(new nint[]{1, 2, 3, 4, 5, 6, 7, 8, 9, 10}.array(), "[10]int{1, 2, 3, 4, 5, 6, 7, 8, 9, 10}"u8),
    new(Ꮡ(new nint[]{1, 2, 3, 4, 5, 6, 7, 8, 9, 10}.array()), "*[10]int(&[10]int{1, 2, 3, 4, 5, 6, 7, 8, 9, 10})"u8),
    new(new nint[]{1, 2, 3, 4, 5, 6, 7, 8, 9, 10}.slice(), "[]int{1, 2, 3, 4, 5, 6, 7, 8, 9, 10}"u8),
    new(Ꮡ(new nint[]{1, 2, 3, 4, 5, 6, 7, 8, 9, 10}.slice()), "*[]int(&[]int{1, 2, 3, 4, 5, 6, 7, 8, 9, 10})"u8)
}.slice();

public static void TestValueToString(ж<testing.T> Ꮡt) {
    foreach (var (i, test) in valueToStringTests) {
        @string s = valueToString(ValueOf(test.i));
        if (s != test.s) {
            Ꮡt.Errorf("#%d: have %#q, want %#q"u8, i, s, test.s);
        }
    }
}

public static void TestPtrSetNil(ж<testing.T> Ꮡt) {
    ref var i = ref heap(new int32(), out var Ꮡi);
    i = 1234;
    ref var ip = ref heap<ж<int32>>(out var Ꮡip);
    ip = Ꮡi;
    var vip = ValueOf(Ꮡip);
    vip.Elem().Set(reflectlite_internal_test_package.Zero(vip.Elem().Type()));
    if (ip != nil) {
        Ꮡt.Errorf("got non-nil (%d), want nil"u8, ip.Value);
    }
}

public static void TestMapSetNil(ж<testing.T> Ꮡt) {
    ref var m = ref heap<map<@string, nint>>(out var Ꮡm);
    m = new map<@string, nint>();
    var vm = ValueOf(Ꮡm);
    vm.Elem().Set(reflectlite_internal_test_package.Zero(vm.Elem().Type()));
    if (m != default!) {
        Ꮡt.Errorf("got non-nil (%p), want nil"u8, m);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string int8ˢ = "int8"u8;
internal static readonly @string structCChanInt32DFloat32ˢ = "*struct { c chan *int32; d float32 }"u8;
internal static readonly @string structCChanInt32DFloat32ˢ2 = "struct { c chan *int32; d float32 }"u8;

public static void TestAll(ж<testing.T> Ꮡt) {
    testType(Ꮡt, 1, TypeOf((int8)0), int8ˢ);
    testType(Ꮡt, 2, TypeOf(((ж<int8>)nil)).Elem(), int8ˢ);
    var typ = TypeOf(((ж<typeᴛ18_x>)nil));
    testType(Ꮡt, 3, typ, structCChanInt32DFloat32ˢ);
    var etyp = typ.Elem();
    testType(Ꮡt, 4, etyp, structCChanInt32DFloat32ˢ2);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string float64ˢ = "float64"u8;
internal static readonly object v2InterfaceDidNotReturnˢ = (@string)"v2.Interface() did not return float64, got "u8;

[GoType("dyn")] partial struct TestInterfaceValue_inter {
    public any E;
}

public static void TestInterfaceValue(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    ref var inter = ref heap(new TestInterfaceValue_inter(), out var Ꮡinter);
    inter.E = 123.456D;
    var v1 = ValueOf(Ꮡinter);
    var v2 = reflectlite_internal_test_package.Field(v1.Elem(), 0);
    // assert(t, TypeString(v2.Type()), "interface {}")
    var v3 = v2.Elem();
    assert(Ꮡt, TypeString(v3.Type()), float64ˢ);
    var i3 = reflectlite_internal_test_package.ToInterface(v2);
    {
        var (_, ok) = i3._<float64>(ᐧ); if (!ok) {
            Ꮡt.Error(v2InterfaceDidNotReturnˢ, TypeOf(i3));
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string funcˢ = "func()"u8;

public static void TestFunctionValue(ж<testing.T> Ꮡt) {
    any x = () => {
    };
    var v = ValueOf(x);
    if (fmt.Sprint(reflectlite_internal_test_package.ToInterface(v)) != fmt.Sprint(x)) {
        Ꮡt.Fatalf("TestFunction returned wrong pointer"u8);
    }
    assert(Ꮡt, TypeString(v.Type()), funcˢ);
}


[GoType("dyn")] partial struct appendTestsᴛ1 {
    internal slice<nint> orig, extra;
}
internal static slice<appendTestsᴛ1> appendTests = new appendTestsᴛ1[]{
    new(new slice<nint>(2, 4), new nint[]{22}.slice()),
    new(new slice<nint>(2, 4), new nint[]{22, 33, 44}.slice())
}.slice();

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

[GoType("dyn")] partial struct TestBigUnnamedStruct_b {
    internal int64 a, b, c, d;
}

[GoType("dyn")] partial struct TestBigUnnamedStruct_type {
    internal int64 a, b, c, d;
}

public static void TestBigUnnamedStruct(ж<testing.T> Ꮡt) {
    var b = new TestBigUnnamedStruct_b(1, 2, 3, 4);
    var v = ValueOf(b);
    var b1 = reflectlite_internal_test_package.ToInterface(v)._<TestBigUnnamedStruct_type>();
    if (b1.a != b.a || b1.b != b.b || b1.c != b.c || b1.d != b.d) {
        Ꮡt.Errorf("ValueOf(%v).Interface().(*Big) = %v"u8, b, b1);
    }
}

[GoType] partial struct big {
    internal int64 a, b, c, d, e;
}

public static void TestBigStruct(ж<testing.T> Ꮡt) {
    var b = new big(1, 2, 3, 4, 5);
    var v = ValueOf(b);
    var b1 = reflectlite_internal_test_package.ToInterface(v)._<big>();
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

internal static ж<Loop> Ꮡloop1 = new StandardBox<Loop>(default(Loop));
internal static ref Loop loop1 => ref Ꮡloop1.ValueSlot;
internal static ж<Loop> Ꮡloop2 = new StandardBox<Loop>(default(Loop));
internal static ref Loop loop2 => ref Ꮡloop2.ValueSlot;

internal static ж<Loopy> Ꮡloopy1 = new StandardBox<Loopy>(default(Loopy));
internal static ref Loopy loopy1 => ref Ꮡloopy1.ValueSlot;
internal static ж<Loopy> Ꮡloopy2 = new StandardBox<Loopy>(default(Loopy));
internal static ref Loopy loopy2 => ref Ꮡloopy2.ValueSlot;

[GoInit] internal static void init() {
    loop1 = Ꮡloop2;
    loop2 = Ꮡloop1;
    loopy1 = Ꮡloopy2;
    loopy2 = Ꮡloopy1;
}

// Equalities
// Inequalities
// Nil vs empty: not the same.
// Mismatched types
// Possible loops.
internal static slice<DeepEqualTest> typeOfTests = new DeepEqualTest[]{
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
    new(fn1, fn2, true),
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
    new(fn1, fn3, false),
    new(fn3, fn3, false),
    new(new slice<nint>[]{new nint[]{1}.slice()}.slice(), new slice<nint>[]{new nint[]{2}.slice()}.slice(), false),
    new(math.NaN(), math.NaN(), false),
    new(Ꮡ(new float64[]{math.NaN()}.array()), Ꮡ(new float64[]{math.NaN()}.array()), false),
    new(Ꮡ(new float64[]{math.NaN()}.array()), new self(nil), true),
    new(new float64[]{math.NaN()}.slice(), new float64[]{math.NaN()}.slice(), false),
    new(new float64[]{math.NaN()}.slice(), new self(nil), true),
    new(new map<float64, float64>{[math.NaN()] = 1D}, new map<float64, float64>{[1D] = 2D}, false),
    new(new map<float64, float64>{[math.NaN()] = 1D}, new self(nil), true),
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
    new(Ꮡloop1, Ꮡloop1, true),
    new(Ꮡloop1, Ꮡloop2, true),
    new(Ꮡloopy1, Ꮡloopy1, true),
    new(Ꮡloopy1, Ꮡloopy2, true)
}.slice();

public static void TestTypeOf(ж<testing.T> Ꮡt) {
    // Special case for nil
    {
        var typ = TypeOf(default!); if (typ != default!) {
            Ꮡt.Errorf("expected nil type for nil value; got %v"u8, typ);
        }
    }
    foreach (var (_, test) in typeOfTests) {
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

public static void Nil(any a, ж<testing.T> Ꮡt) {
    var n = reflectlite_internal_test_package.Field(ValueOf(a), 0);
    if (!n.IsNil()) {
        Ꮡt.Errorf("%v should be nil"u8, a);
    }
}

public static void NotNil(any a, ж<testing.T> Ꮡt) {
    var n = reflectlite_internal_test_package.Field(ValueOf(a), 0);
    if (n.IsNil()) {
        Ꮡt.Errorf("value of type %v should not be nil"u8, TypeString(ValueOf(a).Type()));
    }
}

[GoType("dyn")] partial struct TestIsNil_doNil {
    internal ж<nint> x;
}

[GoType("dyn")] partial struct TestIsNil_doNilᴛ1 {
    internal any x;
}

[GoType("dyn")] partial struct TestIsNil_doNilᴛ2 {
    internal map<@string, nint> x;
}

[GoType("dyn")] partial struct TestIsNil_doNilᴛ3 {
    internal Func<bool> x;
}

[GoType("dyn")] partial struct TestIsNil_doNilᴛ4 {
    internal channel<nint> x;
}

[GoType("dyn")] partial struct TestIsNil_doNilᴛ5 {
    internal slice<@string> x;
}

[GoType("dyn")] partial struct TestIsNil_doNilᴛ6 {
    internal @unsafe.Pointer x;
}

[GoType("dyn")] partial struct TestIsNil_si {
    internal slice<nint> x;
}

[GoType("dyn")] partial struct TestIsNil_mi {
    internal map<nint, nint> x;
}

[GoType("dyn")] partial struct TestIsNil_fi {
    internal Action<ж<testing.T>> x;
}

public static void TestIsNil(ж<testing.T> Ꮡt) {
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
        var ty = reflectlite_internal_test_package.TField(TypeOf(ts), 0);
        var v = reflectlite_internal_test_package.Zero(ty);
        v.IsNil(); // panics if not okay to call
    }
    // Check the implementations
    TestIsNil_doNil pi = default!;
    Nil(pi, Ꮡt);
    pi.x = @new<nint>();
    NotNil(pi, Ꮡt);
    TestIsNil_si si = default!;
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

public static void TestNilPtrValueSub(ж<testing.T> Ꮡt) {
    ж<nint> pi = default!;
    {
        var pv = ValueOf(pi.OrTypedNil()); if (pv.Elem().IsValid()) {
            Ꮡt.Error(valueOfIntNilElemIsValidˢ);
        }
    }
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

[GoType] partial struct D1 {
    internal nint d;
}

[GoType] partial struct D2 {
    internal nint d;
}

[GoType("dyn")] partial struct TestImportPath_tests {
    internal reflectliteꓸType t;
    internal @string path;
}

public static void TestImportPath(ж<testing.T> Ꮡt) {
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
        new(TypeOf(((ж<Point>)nil)).Elem(), "internal/reflectlite_test"u8)
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
internal static readonly object skippingMallocCountInˢ = (@string)"skipping malloc count in short mode"u8;
internal static readonly object skippingGomaxprocs1ˢ = (@string)"skipping; GOMAXPROCS>1"u8;

internal static void noAlloc(ж<testing.T> Ꮡt, nint n, Action<nint> f) {
    if (testing.Short()) {
        Ꮡt.Skip(skippingMallocCountInˢ);
    }
    if (Δruntime.GOMAXPROCS(0) > 1) {
        Ꮡt.Skip(skippingGomaxprocs1ˢ);
    }
    nint i = -1;
    var allocs = testing.AllocsPerRun(n, () => {
        f(i);
        i++;
    });
    if (allocs > 0D) {
        Ꮡt.Errorf("%d iterations: got %v mallocs, want 0"u8, n, allocs);
    }
}

public static void TestAllocations(ж<testing.T> Ꮡt) {
    noAlloc(Ꮡt, 100, (nint j) => {
        any i = default!;
        reflectlite.Value v = new(nil);
        i = new nint[]{j, j, j}.slice();
        v = ValueOf(i);
        if (v.Len() != 3) {
            throw panic("wrong length");
        }
    });
    noAlloc(Ꮡt, 100, (nint j) => {
        any i = default!;
        reflectlite.Value v = new(nil);
        i = (nint jΔ1) => jΔ1;
        v = ValueOf(i);
        if (reflectlite_internal_test_package.ToInterface(v)._<Func<nint, nint>>()(j) != j) {
            throw panic("wrong result");
        }
    });
}

[GoType("dyn")] partial struct TestSetPanic_t0 {
    public nint W;
}

[GoType("dyn")] partial struct TestSetPanic_t1 {
    public nint Y;
    internal partial ref TestSetPanic_t0 t0 { get; }
}

[GoType("dyn")] partial struct TestSetPanic_T2 {
    public nint Z;
    internal TestSetPanic_t0 namedT0;
}

[GoType("dyn")] partial struct TestSetPanic_T {
    public nint X;
    internal partial ref TestSetPanic_t1 t1 { get; }
    public partial ref TestSetPanic_T2 T2 { get; }
    public TestSetPanic_t1 NamedT1;
    public TestSetPanic_T2 NamedT2;
    internal TestSetPanic_t1 namedT1;
    internal TestSetPanic_T2 namedT2;
}

public static void TestSetPanic(ж<testing.T> Ꮡt) {
    void ok(Action f) {
        f();
    }
    var bad = shouldPanic;
    void clear(reflectlite.Value vΔ1) {
        vΔ1.Set(reflectlite_internal_test_package.Zero(vΔ1.Type()));
    }
    // not addressable
    ref var v = ref heap<reflectlite.Value>(out var Ꮡv);
    v = ValueOf(new TestSetPanic_T(nil));
    var clearʗ1 = clear;
    bad(() => {
        clearʗ1(reflectlite_internal_test_package.Field(Ꮡv.Value, 0)); // .X
    });
    var clearʗ2 = clear;
    bad(() => {
        clearʗ2(reflectlite_internal_test_package.Field(Ꮡv.Value, 1)); // .t1
    });
    var clearʗ3 = clear;
    bad(() => {
        clearʗ3(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 1), 0)); // .t1.Y
    });
    var clearʗ4 = clear;
    bad(() => {
        clearʗ4(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 1), 1)); // .t1.t0
    });
    var clearʗ5 = clear;
    bad(() => {
        clearʗ5(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 1), 1), 0)); // .t1.t0.W
    });
    var clearʗ6 = clear;
    bad(() => {
        clearʗ6(reflectlite_internal_test_package.Field(Ꮡv.Value, 2)); // .T2
    });
    var clearʗ7 = clear;
    bad(() => {
        clearʗ7(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 2), 0)); // .T2.Z
    });
    var clearʗ8 = clear;
    bad(() => {
        clearʗ8(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 2), 1)); // .T2.namedT0
    });
    var clearʗ9 = clear;
    bad(() => {
        clearʗ9(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 2), 1), 0)); // .T2.namedT0.W
    });
    var clearʗ10 = clear;
    bad(() => {
        clearʗ10(reflectlite_internal_test_package.Field(Ꮡv.Value, 3)); // .NamedT1
    });
    var clearʗ11 = clear;
    bad(() => {
        clearʗ11(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 3), 0)); // .NamedT1.Y
    });
    var clearʗ12 = clear;
    bad(() => {
        clearʗ12(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 3), 1)); // .NamedT1.t0
    });
    var clearʗ13 = clear;
    bad(() => {
        clearʗ13(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 3), 1), 0)); // .NamedT1.t0.W
    });
    var clearʗ14 = clear;
    bad(() => {
        clearʗ14(reflectlite_internal_test_package.Field(Ꮡv.Value, 4)); // .NamedT2
    });
    var clearʗ15 = clear;
    bad(() => {
        clearʗ15(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 4), 0)); // .NamedT2.Z
    });
    var clearʗ16 = clear;
    bad(() => {
        clearʗ16(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 4), 1)); // .NamedT2.namedT0
    });
    var clearʗ17 = clear;
    bad(() => {
        clearʗ17(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 4), 1), 0)); // .NamedT2.namedT0.W
    });
    var clearʗ18 = clear;
    bad(() => {
        clearʗ18(reflectlite_internal_test_package.Field(Ꮡv.Value, 5)); // .namedT1
    });
    var clearʗ19 = clear;
    bad(() => {
        clearʗ19(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 5), 0)); // .namedT1.Y
    });
    var clearʗ20 = clear;
    bad(() => {
        clearʗ20(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 5), 1)); // .namedT1.t0
    });
    var clearʗ21 = clear;
    bad(() => {
        clearʗ21(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 5), 1), 0)); // .namedT1.t0.W
    });
    var clearʗ22 = clear;
    bad(() => {
        clearʗ22(reflectlite_internal_test_package.Field(Ꮡv.Value, 6)); // .namedT2
    });
    var clearʗ23 = clear;
    bad(() => {
        clearʗ23(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 6), 0)); // .namedT2.Z
    });
    var clearʗ24 = clear;
    bad(() => {
        clearʗ24(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 6), 1)); // .namedT2.namedT0
    });
    var clearʗ25 = clear;
    bad(() => {
        clearʗ25(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 6), 1), 0)); // .namedT2.namedT0.W
    });
    // addressable
    v = ValueOf(Ꮡ(new TestSetPanic_T(nil))).Elem();
    var clearʗ26 = clear;
    ok(() => {
        clearʗ26(reflectlite_internal_test_package.Field(Ꮡv.Value, 0)); // .X
    });
    var clearʗ27 = clear;
    bad(() => {
        clearʗ27(reflectlite_internal_test_package.Field(Ꮡv.Value, 1)); // .t1
    });
    var clearʗ28 = clear;
    ok(() => {
        clearʗ28(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 1), 0)); // .t1.Y
    });
    var clearʗ29 = clear;
    bad(() => {
        clearʗ29(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 1), 1)); // .t1.t0
    });
    var clearʗ30 = clear;
    ok(() => {
        clearʗ30(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 1), 1), 0)); // .t1.t0.W
    });
    var clearʗ31 = clear;
    ok(() => {
        clearʗ31(reflectlite_internal_test_package.Field(Ꮡv.Value, 2)); // .T2
    });
    var clearʗ32 = clear;
    ok(() => {
        clearʗ32(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 2), 0)); // .T2.Z
    });
    var clearʗ33 = clear;
    bad(() => {
        clearʗ33(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 2), 1)); // .T2.namedT0
    });
    var clearʗ34 = clear;
    bad(() => {
        clearʗ34(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 2), 1), 0)); // .T2.namedT0.W
    });
    var clearʗ35 = clear;
    ok(() => {
        clearʗ35(reflectlite_internal_test_package.Field(Ꮡv.Value, 3)); // .NamedT1
    });
    var clearʗ36 = clear;
    ok(() => {
        clearʗ36(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 3), 0)); // .NamedT1.Y
    });
    var clearʗ37 = clear;
    bad(() => {
        clearʗ37(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 3), 1)); // .NamedT1.t0
    });
    var clearʗ38 = clear;
    ok(() => {
        clearʗ38(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 3), 1), 0)); // .NamedT1.t0.W
    });
    var clearʗ39 = clear;
    ok(() => {
        clearʗ39(reflectlite_internal_test_package.Field(Ꮡv.Value, 4)); // .NamedT2
    });
    var clearʗ40 = clear;
    ok(() => {
        clearʗ40(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 4), 0)); // .NamedT2.Z
    });
    var clearʗ41 = clear;
    bad(() => {
        clearʗ41(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 4), 1)); // .NamedT2.namedT0
    });
    var clearʗ42 = clear;
    bad(() => {
        clearʗ42(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 4), 1), 0)); // .NamedT2.namedT0.W
    });
    var clearʗ43 = clear;
    bad(() => {
        clearʗ43(reflectlite_internal_test_package.Field(Ꮡv.Value, 5)); // .namedT1
    });
    var clearʗ44 = clear;
    bad(() => {
        clearʗ44(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 5), 0)); // .namedT1.Y
    });
    var clearʗ45 = clear;
    bad(() => {
        clearʗ45(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 5), 1)); // .namedT1.t0
    });
    var clearʗ46 = clear;
    bad(() => {
        clearʗ46(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 5), 1), 0)); // .namedT1.t0.W
    });
    var clearʗ47 = clear;
    bad(() => {
        clearʗ47(reflectlite_internal_test_package.Field(Ꮡv.Value, 6)); // .namedT2
    });
    var clearʗ48 = clear;
    bad(() => {
        clearʗ48(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 6), 0)); // .namedT2.Z
    });
    var clearʗ49 = clear;
    bad(() => {
        clearʗ49(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 6), 1)); // .namedT2.namedT0
    });
    var clearʗ50 = clear;
    bad(() => {
        clearʗ50(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(reflectlite_internal_test_package.Field(Ꮡv.Value, 6), 1), 0)); // .namedT2.namedT0.W
    });
}

internal static void shouldPanic(Action f) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            if (recover() == default!) {
                throw panic("did not panic");
            }
        }, ref ᒐ);
        f();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoType] partial struct S {
    internal int64 i1;
    internal int64 i2;
}

public static void TestBigZero(ж<testing.T> Ꮡt) {
    UntypedInt size = /* 1 << 10 */ 1024;
    array<byte> v = new(1024); /* size */
    var z = reflectlite_internal_test_package.ToInterface(reflectlite_internal_test_package.Zero(ValueOf(v).Type()))._<array<byte>>();
    for (nint i = 0; i < size; i++) {
        if (z[i] != 0) {
            Ꮡt.Fatalf("Zero object not all zero, index %d"u8, i);
        }
    }
}

// Used to have inconsistency between IsValid() and Kind() != Invalid.
[GoType("dyn")] partial struct TestInvalid_T {
    internal any v;
}

public static void TestInvalid(ж<testing.T> Ꮡt) {
    var v = reflectlite_internal_test_package.Field(ValueOf(new TestInvalid_T(nil)), 0);
    if (v.IsValid() != true || v.Kind() != Interface) {
        Ꮡt.Errorf("field: IsValid=%v, Kind=%v, want true, Interface"u8, v.IsValid(), v.Kind());
    }
    v = v.Elem();
    if (v.IsValid() != false || v.Kind() != abi.Invalid) {
        Ꮡt.Errorf("field elem: IsValid=%v, Kind=%v, want false, Invalid"u8, v.IsValid(), v.Kind());
    }
}

[GoType("num:nint")] partial struct TheNameOfThisTypeIsExactly255BytesLongSoWhenTheCompilerPrependsTheReflectTestPackageNameAndExtraStarTheLinkerRuntimeAndReflectPackagesWillHaveToCorrectlyDecodeTheSecondLengthByte0123456789_0123456789_0123456789_0123456789_0123456789_012345678;

[GoType] partial struct nameTest {
    internal any v;
    internal @string want;
}

[GoType] partial struct A {
}

[GoType] partial struct B<T> {
}


        [GoType("dyn")] partial interface Δtypeᴛ30 {
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
    new(((ж<Δtypeᴛ30>)nil), ""u8),
    new(((ж<TheNameOfThisTypeIsExactly255BytesLongSoWhenTheCompilerPrependsTheReflectTestPackageNameAndExtraStarTheLinkerRuntimeAndReflectPackagesWillHaveToCorrectlyDecodeTheSecondLengthByte0123456789_0123456789_0123456789_0123456789_0123456789_012345678>)nil), "TheNameOfThisTypeIsExactly255BytesLongSoWhenTheCompilerPrependsTheReflectTestPackageNameAndExtraStarTheLinkerRuntimeAndReflectPackagesWillHaveToCorrectlyDecodeTheSecondLengthByte0123456789_0123456789_0123456789_0123456789_0123456789_012345678"u8),
    new(((ж<B<A>>)nil), "B[internal/reflectlite_test.A]"u8),
    new(((ж<B<B<A>>>)nil), "B[internal/reflectlite_test.B[internal/reflectlite_test.A]]"u8)
}.slice();

public static void TestNames(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in nameTests) {
        var typ = TypeOf(test.v).Elem();
        {
            @string got = typ.Name(); if (got != test.want) {
                Ꮡt.Errorf("%v Name()=%q, want %q"u8, typ, got, test.want);
            }
        }
    }
}

[GoType("dyn")] partial struct TestUnaddressableField_localBuffer {
    internal slice<byte> buf;
}

// TestUnaddressableField tests that the reflect package will not allow
// a type from another package to be used as a named type with an
// unexported field.
//
// This ensures that unexported fields cannot be modified by other packages.
public static void TestUnaddressableField(ж<testing.T> Ꮡt) {
    global::go.@internal.reflectlite_internal_test_package.Buffer b = default!;                                                                    // type defined in reflect, a different package
    ref var localBuffer = ref heap(new TestUnaddressableField_localBuffer(), out var ᏑlocalBuffer);
    ref var lv = ref heap<reflectlite.Value>(out var Ꮡlv);
    lv = ValueOf(ᏑlocalBuffer).Elem();
    ref var rv = ref heap<reflectlite.Value>(out var Ꮡrv);
    rv = ValueOf(b);
    var lvʗ1 = lv;
    var rvʗ1 = rv;
    shouldPanic(() => {
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
internal static readonly @string reflectliteTestTalias1ˢ = "reflectlite_test.Talias1{byte:0x1, uint8:0x2, int:3, int32:4, rune:5}"u8;
internal static readonly @string reflectliteTestTalias2ˢ = "reflectlite_test.Talias2{Tint:1, Tint2:2}"u8;

public static void TestAliasNames(ж<testing.T> Ꮡt) {
    var t1 = new Talias1(@byte: 1, uint8: 2, @int: 3, int32: 4, rune: 5);
    @string @out = fmt.Sprintf("%#v"u8, t1);
    @string want = reflectliteTestTalias1ˢ;
    if (@out != want) {
        Ꮡt.Errorf("Talias1 print:\nhave: %s\nwant: %s"u8, @out, want);
    }
    var t2 = new Talias2(Tint: 1, Tint2: 2);
    @out = fmt.Sprintf("%#v"u8, t2);
    want = reflectliteTestTalias2ˢ;
    if (@out != want) {
        Ꮡt.Errorf("Talias2 print:\nhave: %s\nwant: %s"u8, @out, want);
    }
}

} // end reflectlite_test_package
