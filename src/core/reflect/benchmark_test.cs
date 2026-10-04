// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using fmt = fmt_package;
using static reflect_package;
using strconv = strconv_package;
using Δtesting = testing_package;
using static global::go.reflect_internal_test_package;
using Δreflect = reflect_package;

partial class reflect_test_package {


[GoType("dyn")] partial struct sourceAllᴛ1 {
    public reflectꓸValue Bool;
    public reflectꓸValue String;
    public reflectꓸValue Bytes;
    public reflectꓸValue NamedBytes;
    public reflectꓸValue BytesArray;
    public reflectꓸValue SliceAny;
    public reflectꓸValue MapStringAny;
}
internal static sourceAllᴛ1 sourceAll = new sourceAllᴛ1(
    Bool: ValueOf(@new<bool>()).Elem(),
    String: ValueOf(@new<@string>()).Elem(),
    Bytes: ValueOf(@new<slice<byte>>()).Elem(),
    NamedBytes: ValueOf(@new<namedBytes>()).Elem(),
    BytesArray: ValueOf(Ꮡ(new array<byte>(32))).Elem(),
    SliceAny: ValueOf(@new<slice<any>>()).Elem(),
    MapStringAny: ValueOf(@new<map<@string, any>>()).Elem()
);


[GoType("dyn")] partial struct sinkAllᴛ1 {
    public bool RawBool;
    public @string RawString;
    public slice<byte> RawBytes;
    public nint RawInt;
}
internal static sinkAllᴛ1 sinkAll;

public static void BenchmarkBool(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        sinkAll.RawBool = sourceAll.Bool.Bool();
    }
}

public static void BenchmarkString(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        sinkAll.RawString = sourceAll.String.String();
    }
}

public static void BenchmarkBytes(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        sinkAll.RawBytes = sourceAll.Bytes.Bytes();
    }
}

public static void BenchmarkNamedBytes(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        sinkAll.RawBytes = sourceAll.NamedBytes.Bytes();
    }
}

public static void BenchmarkBytesArray(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        sinkAll.RawBytes = sourceAll.BytesArray.Bytes();
    }
}

public static void BenchmarkSliceLen(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        sinkAll.RawInt = sourceAll.SliceAny.Len();
    }
}

public static void BenchmarkMapLen(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        sinkAll.RawInt = sourceAll.MapStringAny.Len();
    }
}

public static void BenchmarkStringLen(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        sinkAll.RawInt = sourceAll.String.Len();
    }
}

public static void BenchmarkArrayLen(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        sinkAll.RawInt = sourceAll.BytesArray.Len();
    }
}

public static void BenchmarkSliceCap(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        sinkAll.RawInt = sourceAll.SliceAny.Cap();
    }
}

public static void BenchmarkDeepEqual(ж<Δtesting.B> Ꮡb) {
    foreach (var (_, vᴛ1) in deepEqualPerfTests) {
        ref var bb = ref heap(new deepEqualPerfTestsᴛ1(), out var Ꮡbb);
        bb = vᴛ1;

        var bbʗ1 = bb;
        Ꮡb.Run(ValueOf(bb.x).Type().String(), (ж<Δtesting.B> bΔ1) => {
            bΔ1.ReportAllocs();
            for (nint i = 0; i < (~bΔ1).N; i++) {
                sink = DeepEqual(bbʗ1.x, bbʗ1.y);
            }
        });
    }
}

public static void BenchmarkMapsDeepEqual(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var m1 = new map<nint, nint>{
        [1] = 1, [2] = 2
    };
    var m2 = new map<nint, nint>{
        [1] = 1, [2] = 2
    };
    for (nint i = 0; i < b.N; i++) {
        DeepEqual(m1, m2);
    }
}

[GoType("dyn")] internal partial struct BenchmarkIsZero_Int4 {
    internal nint a, b, c, d;
}

[GoType("dyn")] internal partial struct BenchmarkIsZero_Int1024 {
    internal array<nint> a = new(1024);
}

[GoType("dyn")] internal partial struct BenchmarkIsZero_Int512 {
    internal array<S> a1 = new(16);
    internal array<S> a2 = new(16);
    internal array<S> a3 = new(16);
    internal array<S> a4 = new(16);
    internal array<S> a5 = new(16);
    internal array<S> a6 = new(16);
    internal array<S> a7 = new(16);
    internal array<S> a8 = new(16);
    internal array<S> a9 = new(16);
    internal array<S> a10 = new(16);
    internal array<S> a11 = new(16);
    internal array<S> a12 = new(16);
    internal array<S> a13 = new(16);
    internal array<S> a14 = new(16);
    internal array<S> a15 = new(16);
    internal array<S> a16 = new(16);
}

[GoType("dyn")] internal partial struct BenchmarkIsZero_s {
    public array<T> ArrayComparable = new(4);
    public array<_Complex> ArrayIncomparable = new(4, () => new());
    public T StructComparable;
    public _Complex StructIncomparable;
    public array<nint> ArrayInt_4 = new(4);
    public array<nint> ArrayInt_1024 = new(1024);
    public array<nint> ArrayInt_1024_NoZero = new(1024);
    public BenchmarkIsZero_Int4 Struct4Int;
    public array<BenchmarkIsZero_Int4> ArrayStruct4Int_1024 = new(256);
    public array<channel<nint>> ArrayChanInt_1024 = new(1024);
    public BenchmarkIsZero_Int512 StructInt_512;
}

public static void BenchmarkIsZero(ж<Δtesting.B> Ꮡb) {
    var s = new BenchmarkIsZero_s();
    s.ArrayInt_1024_NoZero[512] = 1;
    var source = ValueOf(s);
    for (nint i = 0; i < source.NumField(); i++) {
        @string name = source.Type().Field(i).Name;
        ref var value = ref heap<reflectꓸValue>(out var Ꮡvalue);
        value = source.Field(i);
        var valueʗ1 = value;
        Ꮡb.Run(name, (ж<Δtesting.B> bΔ1) => {
            for (nint iΔ1 = 0; iΔ1 < (~bΔ1).N; iΔ1++) {
                sink = valueʗ1.IsZero();
            }
        });
    }
}

[GoType("dyn")] internal partial interface BenchmarkSetZero_type_Interface {
    @string String();
}

[GoType("dyn")] internal partial struct BenchmarkSetZero_type {
    public bool Bool;
    public int64 Int;
    public uint64 Uint;
    public float64 Float;
    public complex128 Complex;
    public array<reflectꓸValue> Array = new(4, () => new(nil));
    public channel<reflectꓸValue> Chan;
    public Func<reflectꓸValue> Func;
    public BenchmarkSetZero_type_Interface Interface;
    public map<@string, reflectꓸValue> Map;
    public ж<reflectꓸValue> Pointer;
    public slice<reflectꓸValue> Slice;
    public @string String;
    public reflectꓸValue Struct;
}

public static void BenchmarkSetZero(ж<Δtesting.B> Ꮡb) {
    var source = ValueOf(@new<BenchmarkSetZero_type>()).Elem();
    for (nint i = 0; i < source.NumField(); i++) {
        @string name = source.Type().Field(i).Name;
        ref var value = ref heap<reflectꓸValue>(out var Ꮡvalue);
        value = source.Field(i);
        ref var zero = ref heap<reflectꓸValue>(out var Ꮡzero);
        zero = Zero(value.Type());
        var valueʗ1 = value;
        Ꮡb.Run(name + "/Direct"u8, (ж<Δtesting.B> bΔ1) => {
            for (nint iΔ1 = 0; iΔ1 < (~bΔ1).N; iΔ1++) {
                valueʗ1.SetZero();
            }
        });
        var valueʗ2 = value;
        var zeroʗ1 = zero;
        Ꮡb.Run(name + "/CachedZero"u8, (ж<Δtesting.B> bΔ2) => {
            for (nint iΔ2 = 0; iΔ2 < (~bΔ2).N; iΔ2++) {
                valueʗ2.Set(zeroʗ1);
            }
        });
        var valueʗ3 = value;
        Ꮡb.Run(name + "/NewZero"u8, (ж<Δtesting.B> bΔ3) => {
            for (nint iΔ3 = 0; iΔ3 < (~bΔ3).N; iΔ3++) {
                valueʗ3.Set(Zero(valueʗ3.Type()));
            }
        });
    }
}

public static void BenchmarkSelect(ж<Δtesting.B> Ꮡb) {
    var Δchannel = new channel<nint>(0);
    close(Δchannel);
    slice<Δreflect.SelectCase> cases = default!;
    for (nint i = 0; i < 8; i++) {
        cases = append(cases, new SelectCase(
            Dir: SelectRecv,
            Chan: ValueOf(Δchannel)
        ));
    }
    foreach (var (_, numCases) in new nint[]{1, 4, 8}.slice()) {
        var casesʗ1 = cases;
        Ꮡb.Run(strconv.Itoa(numCases), (ж<Δtesting.B> bΔ1) => {
            bΔ1.ReportAllocs();
            for (nint i = 0; i < (~bΔ1).N; i++) {
                (_, _, _) = Select(casesʗ1.slice(0, numCases));
            }
        });
    }
}

public static void BenchmarkCall(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    ref var fv = ref heap<reflectꓸValue>(out var Ꮡfv);
    fv = ValueOf((@string a, @string bΔ1) => {
    });
    b.ReportAllocs();
    var fvʗ1 = fv;
    Ꮡb.RunParallel((ж<Δtesting.PB> pb) => {
        var args = new reflectꓸValue[]{ValueOf((@string)"a"u8), ValueOf((@string)"b"u8)}.slice();
        while (pb.Next()) {
            fvʗ1.Call(args);
        }
    });
}

[GoType("num:int64")] partial struct myint;

[GoRecv] internal static void inc(this ref myint i) {
    i = i + 1;
}

public static void BenchmarkCallMethod(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    var z = @new<myint>();
    var v = ValueOf(z.inc);
    for (nint i = 0; i < b.N; i++) {
        v.Call(default!);
    }
}

[GoType("dyn")] internal partial struct BenchmarkCallArgCopy_sizes {
    internal reflectꓸValue fv;
    internal reflectꓸValue arg;
}

public static void BenchmarkCallArgCopy(ж<Δtesting.B> Ꮡb) {
    reflectꓸValue byteArray(nint n) => Zero(ArrayOf(n, TypeOf((byte)0)));
    var sizes = new BenchmarkCallArgCopy_sizes[]{
        new(ValueOf(([GoArrayDims(128)] array<byte> a) => {
            a = a.Clone();
        }), byteArray(128)),
        new(ValueOf(([GoArrayDims(256)] array<byte> a) => {
            a = a.Clone();
        }), byteArray(256)),
        new(ValueOf(([GoArrayDims(1024)] array<byte> a) => {
            a = a.Clone();
        }), byteArray(1024)),
        new(ValueOf(([GoArrayDims(4096)] array<byte> a) => {
            a = a.Clone();
        }), byteArray(4096)),
        new(ValueOf(([GoArrayDims(65536)] array<byte> a) => {
            a = a.Clone();
        }), byteArray(65536))
    }.array();
    foreach (var (_, vᴛ1) in sizes.ΔRangeSnapshot()) {
        ref var size = ref heap(new BenchmarkCallArgCopy_sizes(), out var Ꮡsize);
        size = vᴛ1;

        var sizeʗ1 = size;
        var bench = (ж<Δtesting.B> bΔ1) => {
            var args = new reflectꓸValue[]{sizeʗ1.arg}.slice();
            bΔ1.SetBytes((int64)sizeʗ1.arg.Len());
            bΔ1.ResetTimer();
            var argsʗ1 = args;
            var sizeʗ2 = sizeʗ1;
            bΔ1.RunParallel((ж<Δtesting.PB> pb) => {
                while (pb.Next()) {
                    sizeʗ2.fv.Call(argsʗ1);
                }
            });
        };
        @string name = fmt.Sprintf("size=%v"u8, size.arg.Len());
        Ꮡb.Run(name, bench);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string ptrToThisˢ = "PtrToThis"u8;

// Construct a type with a zero ptrToThis.
[GoType("dyn")] internal partial struct BenchmarkPtrTo_T {
    [GoEmbedded] internal nint @int;
}

public static void BenchmarkPtrTo(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var t = SliceOf(TypeOf(new BenchmarkPtrTo_T(nil)));
    var ptrToThis = ValueOf(t).Elem().FieldByName(ptrToThisˢ);
    if (!ptrToThis.IsValid()) {
        Ꮡb.Skipf("%v has no ptrToThis field; was it removed from rtype?"u8, t); // TODO fix this at top of refactoring
    }
    // b.Fatalf("%v has no ptrToThis field; was it removed from rtype?", t)
    if (ptrToThis.Int() != 0) {
        Ꮡb.Fatalf("%v.ptrToThis unexpectedly nonzero"u8, t);
    }
    b.ResetTimer();
    // Now benchmark calling PointerTo on it: we'll have to hit the ptrMap cache on
    // every call.
    var tʗ1 = t;
    Ꮡb.RunParallel((ж<Δtesting.PB> pb) => {
        while (pb.Next()) {
            PointerTo(tʗ1);
        }
    });
}

[GoType] partial struct B1 {
    public nint X;
    public nint Y;
    public nint Z;
}

public static void BenchmarkFieldByName1(ж<Δtesting.B> Ꮡb) {
    var t = TypeOf(new B1(nil));
    var tʗ1 = t;
    Ꮡb.RunParallel((ж<Δtesting.PB> pb) => {
        while (pb.Next()) {
            tʗ1.FieldByName("Z"u8);
        }
    });
}

public static void BenchmarkFieldByName2(ж<Δtesting.B> Ꮡb) {
    var t = TypeOf(new S3(nil));
    var tʗ1 = t;
    Ꮡb.RunParallel((ж<Δtesting.PB> pb) => {
        while (pb.Next()) {
            tʗ1.FieldByName("B"u8);
        }
    });
}

public static void BenchmarkFieldByName3(ж<Δtesting.B> Ꮡb) {
    var t = TypeOf(new R0(nil));
    var tʗ1 = t;
    Ꮡb.RunParallel((ж<Δtesting.PB> pb) => {
        while (pb.Next()) {
            tʗ1.FieldByName("X"u8);
        }
    });
}

[GoType] partial struct S {
    internal int64 i1;
    internal int64 i2;
}

public static void BenchmarkInterfaceBig(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    ref var v = ref heap<reflectꓸValue>(out var Ꮡv);
    v = ValueOf(new S(nil));
    var vʗ1 = v;
    Ꮡb.RunParallel((ж<Δtesting.PB> pb) => {
        while (pb.Next()) {
            vʗ1.Interface();
        }
    });
    b.StopTimer();
}

public static void BenchmarkInterfaceSmall(ж<Δtesting.B> Ꮡb) {
    ref var v = ref heap<reflectꓸValue>(out var Ꮡv);
    v = ValueOf((int64)0);
    var vʗ1 = v;
    Ꮡb.RunParallel((ж<Δtesting.PB> pb) => {
        while (pb.Next()) {
            vʗ1.Interface();
        }
    });
}

public static void BenchmarkNew(ж<Δtesting.B> Ꮡb) {
    var v = TypeOf(new XM(nil));
    var vʗ1 = v;
    Ꮡb.RunParallel((ж<Δtesting.PB> pb) => {
        while (pb.Next()) {
            New(vʗ1);
        }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string mapIndexˢ = "MapIndex"u8;
internal static readonly @string setMapIndexˢ = "SetMapIndex"u8;

[GoLocalName("V")] [GoType("ж<nint>")] internal partial class BenchmarkMap_V;

[GoLocalName("S")] [GoType("@string")] internal partial struct BenchmarkMap_S;

[GoType("dyn")] internal partial struct BenchmarkMap_tests {
    internal @string label;
    internal reflectꓸValue m, keys, value;
}

public static void BenchmarkMap(ж<Δtesting.B> Ꮡb) {
    var value = ValueOf(((BenchmarkMap_V)nil));
    var stringKeys = new @string[]{}.slice();
    var mapOfStrings = new map<@string, BenchmarkMap_V>{};
    var uint64Keys = new uint64[]{}.slice();
    var mapOfUint64s = new map<uint64, BenchmarkMap_V>{};
    var userStringKeys = new BenchmarkMap_S[]{}.slice();
    var mapOfUserStrings = new map<BenchmarkMap_S, BenchmarkMap_V>{};
    for (nint i = 0; i < 100; i++) {
        @string stringKey = fmt.Sprintf("key%d"u8, i);
        stringKeys = append(stringKeys, stringKey);
        mapOfStrings[stringKey] = default!;
        var uint64Key = (uint64)i;
        uint64Keys = append(uint64Keys, uint64Key);
        mapOfUint64s[uint64Key] = default!;
        BenchmarkMap_S userStringKey = ((BenchmarkMap_S)fmt.Sprintf("key%d"u8, i));
        userStringKeys = append(userStringKeys, userStringKey);
        mapOfUserStrings[userStringKey] = default!;
    }
    var tests = new BenchmarkMap_tests[]{
        new("StringKeys"u8, ValueOf(mapOfStrings), ValueOf(stringKeys), value),
        new("Uint64Keys"u8, ValueOf(mapOfUint64s), ValueOf(uint64Keys), value),
        new("UserStringKeys"u8, ValueOf(mapOfUserStrings), ValueOf(userStringKeys), value)
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new BenchmarkMap_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡb.Run(tt.label, (ж<Δtesting.B> bΔ1) => {
            var ttʗ2 = ttʗ1;
            bΔ1.Run(mapIndexˢ, (ж<Δtesting.B> bΔ2) => {
                bΔ2.ReportAllocs();
                for (nint i = 0; i < (~bΔ2).N; i++) {
                    for (nint j = ttʗ2.keys.Len() - 1; j >= 0; j--) {
                        ttʗ2.m.MapIndex(ttʗ2.keys.Index(j));
                    }
                }
            });
            var ttʗ3 = ttʗ1;
            bΔ1.Run(setMapIndexˢ, (ж<Δtesting.B> bΔ3) => {
                bΔ3.ReportAllocs();
                for (nint i = 0; i < (~bΔ3).N; i++) {
                    for (nint j = ttʗ3.keys.Len() - 1; j >= 0; j--) {
                        ttʗ3.m.SetMapIndex(ttʗ3.keys.Index(j), ttʗ3.value);
                    }
                }
            });
        });
    }
}

public static void BenchmarkMapIterNext(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var m = ValueOf(new map<@string, nint>{["a"u8] = 0, ["b"u8] = 1, ["c"u8] = 2, ["d"u8] = 3});
    var it = m.MapRange();
    for (nint i = 0; i < b.N; i++) {
        while (it.Next()) {
        }
        it.Reset(m);
    }
}

} // end reflect_test_package
