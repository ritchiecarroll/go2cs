// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using fmt = fmt_package;
using static reflect_package;
using strconv = strconv_package;
using Δtesting = testing_package;
using time = time_package;
using iter = iter_package;
using static global::go.reflect_internal_test_package;
using Δreflect = reflect_package;

partial class reflect_test_package {

partial struct sourceAllᴛ1 /*dyn*/ {
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

partial struct sinkAllᴛ1 /*dyn*/ {
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

public static void BenchmarkDeepEqual(ж<Δtesting.B> b) {
    foreach (var (_, vᴛ1) in range(deepEqualPerfTests)) {
        ref var bb = ref heap(new deepEqualPerfTestsᴛ1(), out var Ꮡbb);
        bb = vᴛ1;

        var bbʗ1 = bb;
        b.Run(ValueOf(bb.x).Type().String(), (ж<Δtesting.B> bΔ1) => {
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

internal partial struct BenchmarkIsZero_Int4 /*dyn*/ {
    internal nint a, b, c, d;
}

internal partial struct BenchmarkIsZero_Int1024 /*dyn*/ {
    internal array<nint> a = new(1024);
}

internal partial struct BenchmarkIsZero_Int512 /*dyn*/ {
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

internal partial struct BenchmarkIsZero_s /*dyn*/ {
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

public static void BenchmarkIsZero(ж<Δtesting.B> b) {
    var s = new BenchmarkIsZero_s();
    s.ArrayInt_1024_NoZero[512] = 1;
    var source = ValueOf(s);

    foreach (var (field, vᴛ1) in range<Δreflect.StructField, reflectꓸValue>(source.Fields().Invoke)) {
        ref var value = ref heap(new reflectꓸValue(), out var Ꮡvalue);
        value = vᴛ1;

        var valueʗ1 = value;
        b.Run(field.Name, (ж<Δtesting.B> bΔ1) => {
            for (nint i = 0; i < (~bΔ1).N; i++) {
                sink = valueʗ1.IsZero();
            }
        });
    }
}

internal partial interface BenchmarkSetZero_type_Interface /*dyn*/ {
    @string String();
}

internal partial struct BenchmarkSetZero_type /*dyn*/ {
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

public static void BenchmarkSetZero(ж<Δtesting.B> b) {
    var source = ValueOf(@new<BenchmarkSetZero_type>()).Elem();

    foreach (var (field, vᴛ1) in range<Δreflect.StructField, reflectꓸValue>(source.Fields().Invoke)) {
        ref var value = ref heap(new reflectꓸValue(), out var Ꮡvalue);
        value = vᴛ1;

        @string name = field.Name;
        ref var zero = ref heap<reflectꓸValue>(Zero(value.Type()), out var Ꮡzero);
        var valueʗ1 = value;
        b.Run(name + "/Direct"u8, (ж<Δtesting.B> bΔ1) => {
            for (nint i = 0; i < (~bΔ1).N; i++) {
                valueʗ1.SetZero();
            }
        });
        var valueʗ2 = value;
        var zeroʗ1 = zero;
        b.Run(name + "/CachedZero"u8, (ж<Δtesting.B> bΔ2) => {
            for (nint i = 0; i < (~bΔ2).N; i++) {
                valueʗ2.Set(zeroʗ1);
            }
        });
        var valueʗ3 = value;
        b.Run(name + "/NewZero"u8, (ж<Δtesting.B> bΔ3) => {
            for (nint i = 0; i < (~bΔ3).N; i++) {
                valueʗ3.Set(Zero(valueʗ3.Type()));
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string byteArrayˢ => ref byteArrayˢᵇ;
private static readonly @string byteArrayˢᵇ = "ByteArray"u8;
internal static ref readonly @string bigStructˢ => ref bigStructˢᵇ;
private static readonly @string bigStructˢᵇ = "BigStruct"u8;
internal static ref readonly @string smallStructˢ => ref smallStructˢᵇ;
private static readonly @string smallStructˢᵇ = "SmallStruct"u8;
internal static ref readonly @string smallStructArrayˢ => ref smallStructArrayˢᵇ;
private static readonly @string smallStructArrayˢᵇ = "SmallStructArray"u8;
internal static ref readonly @string timeˢ => ref timeˢᵇ;
private static readonly @string timeˢᵇ = "Time"u8;

internal partial struct BenchmarkZero_bm /*dyn*/ {
    internal @string name;
    internal reflectꓸValue zero;
    internal reflectꓸValue nonZero;
    internal nint size;
}

internal partial struct BenchmarkZero_Small /*dyn*/ {
    public int64 A;
    public bool B, C;
}

internal partial struct BenchmarkZero_Big /*dyn*/ {
    public int64 A;
    public bool B, C;
    public array<byte> D = new(1008);
}

// BenchmarkZero overlaps some with BenchmarkSetZero,
// but the inputs are set up differently to exercise
// different optimizations.
public static void BenchmarkZero(ж<Δtesting.B> b) {
    BenchmarkZero_bm entry(@string name, any zero, any nonZero) => new BenchmarkZero_bm(name, ValueOf(zero), ValueOf(nonZero).Elem(), (nint)TypeOf(zero).Size());
    ж<time.Time> nonZeroTime() {
        ref var t = ref heap<time.Time>(time.Now(), out var Ꮡt);
        return Ꮡt;
    }

    var bms = slice<BenchmarkZero_bm>([
        entry(byteArrayˢ, new byte[]{}.array(16), Ꮡ(new byte[]{1}.array(16))),
        entry(byteArrayˢ, new byte[]{}.array(64), Ꮡ(new byte[]{1}.array(64))),
        entry(byteArrayˢ, new byte[]{}.array(1024), Ꮡ(new byte[]{1}.array(1024))),
        entry(bigStructˢ, new BenchmarkZero_Big(), Ꮡ(new BenchmarkZero_Big(A: 1))),
        entry(smallStructˢ, new BenchmarkZero_Small(), Ꮡ(new BenchmarkZero_Small(A: 1))),
        entry(smallStructArrayˢ, new BenchmarkZero_Small[]{}.array(4), Ꮡ(new array<BenchmarkZero_Small>(4){[0] = new(A: 1)})),
        entry(smallStructArrayˢ, new BenchmarkZero_Small[]{}.array(64), Ꮡ(new array<BenchmarkZero_Small>(64){[0] = new(A: 1)})),
        entry(timeˢ, new time.Time(), nonZeroTime().OrTypedNil())
    ]);

    foreach (var (_, vᴛ1) in range(bms)) {
        ref var bm = ref heap(new BenchmarkZero_bm(), out var Ꮡbm);
        bm = vᴛ1;

        var bmʗ1 = bm;
        b.Run(fmt.Sprintf("IsZero/%s/size=%d"u8, bm.name, bm.size), (ж<Δtesting.B> bΔ1) => {
            for (nint i = 0; i < (~bΔ1).N; i++) {
                bmʗ1.zero.IsZero();
            }
        });
    }
    foreach (var (_, vᴛ2) in range(bms)) {
        ref var bm = ref heap(new BenchmarkZero_bm(), out var Ꮡbm);
        bm = vᴛ2;

        var bmʗ2 = bm;
        b.Run(fmt.Sprintf("SetZero/%s/size=%d"u8, bm.name, bm.size), (ж<Δtesting.B> bΔ2) => {
            for (nint i = 0; i < (~bΔ2).N; i++) {
                bmʗ2.nonZero.Set(bmʗ2.zero);
            }
        });
    }
}

public static void BenchmarkSelect(ж<Δtesting.B> b) {
    var Δchannel = new channel<nint>(0);
    close(Δchannel);
    slice<Δreflect.SelectCase> cases = default;
    for (nint i = 0; i < 8; i++) {
        cases = append(cases, new SelectCase(
            Dir: SelectRecv,
            Chan: ValueOf(Δchannel)
        ));
    }
    foreach (var (_, numCases) in range(slice<nint>([1, 4, 8]))) {
        var casesʗ1 = cases;
        b.Run(strconv.Itoa(numCases), (ж<Δtesting.B> bΔ1) => {
            bΔ1.ReportAllocs();
            for (nint i = 0; i < (~bΔ1).N; i++) {
                (_, _, _) = Select(casesʗ1.slice(0, numCases));
            }
        });
    }
}

public static void BenchmarkCall(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    ref var fv = ref heap<reflectꓸValue>(ValueOf((@string a, @string bΔ1) => {
    }), out var Ꮡfv);
    b.ReportAllocs();
    var fvʗ1 = fv;
    Ꮡb.RunParallel((ж<Δtesting.PB> pb) => {
        var args = slice<reflectꓸValue>([ValueOf("a"), ValueOf("b")]);
        while (pb.Next()) {
            fvʗ1.Call(args);
        }
    });
}

partial struct myint /*num:int64*/;

internal static void inc(this ref myint i) {
    i = i + 1;
}

public static void BenchmarkCallMethod(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    var z = @new<myint>();

    var v = ValueOf(z.inc);
    for (nint i = 0; i < b.N; i++) {
        v.Call(default);
    }
}

internal partial struct BenchmarkCallArgCopy_sizes /*dyn*/ {
    internal reflectꓸValue fv;
    internal reflectꓸValue arg;
}

public static void BenchmarkCallArgCopy(ж<Δtesting.B> b) {
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
    foreach (var (_, vᴛ1) in range(sizes.ΔRangeSnapshot())) {
        ref var size = ref heap(new BenchmarkCallArgCopy_sizes(), out var Ꮡsize);
        size = vᴛ1;

        var sizeʗ1 = size;
        var bench = (ж<Δtesting.B> bΔ1) => {
            var args = slice<reflectꓸValue>([sizeʗ1.arg]);
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
        b.Run(name, bench);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string ptrToThisˢ => ref ptrToThisˢᵇ;
private static readonly @string ptrToThisˢᵇ = "PtrToThis"u8;

// Construct a type with a zero ptrToThis.
internal partial struct BenchmarkPtrTo_T /*dyn*/ {
    /*embed*/ internal nint @int;
}

public static void BenchmarkPtrTo(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var t = SliceOf(TypeOf(new BenchmarkPtrTo_T()));
    var ptrToThis = ValueOf(t).Elem().FieldByName(ptrToThisˢ);
    if (!ptrToThis.IsValid()) {
        Ꮡb.Skipf("%v has no ptrToThis field; was it removed from rtype?", t); // TODO fix this at top of refactoring
        // b.Fatalf("%v has no ptrToThis field; was it removed from rtype?", t)
    }
    if (ptrToThis.Int() != 0) {
        Ꮡb.Fatalf("%v.ptrToThis unexpectedly nonzero", t);
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

partial struct B1 {
    public nint X;
    public nint Y;
    public nint Z;
}

public static void BenchmarkFieldByName1(ж<Δtesting.B> b) {
    var t = TypeOf(new B1());
    var tʗ1 = t;
    b.RunParallel((ж<Δtesting.PB> pb) => {
        while (pb.Next()) {
            tʗ1.FieldByName("Z");
        }
    });
}

public static void BenchmarkFieldByName2(ж<Δtesting.B> b) {
    var t = TypeOf(new S3());
    var tʗ1 = t;
    b.RunParallel((ж<Δtesting.PB> pb) => {
        while (pb.Next()) {
            tʗ1.FieldByName("B");
        }
    });
}

public static void BenchmarkFieldByName3(ж<Δtesting.B> b) {
    var t = TypeOf(new R0());
    var tʗ1 = t;
    b.RunParallel((ж<Δtesting.PB> pb) => {
        while (pb.Next()) {
            tʗ1.FieldByName("X");
        }
    });
}

partial struct S {
    internal int64 i1;
    internal int64 i2;
}

public static void BenchmarkInterfaceBig(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    ref var v = ref heap<reflectꓸValue>(ValueOf(new S()), out var Ꮡv);
    var vʗ1 = v;
    Ꮡb.RunParallel((ж<Δtesting.PB> pb) => {
        while (pb.Next()) {
            vʗ1.Interface();
        }
    });
    b.StopTimer();
}

public static void BenchmarkInterfaceSmall(ж<Δtesting.B> b) {
    ref var v = ref heap<reflectꓸValue>(ValueOf((int64)0), out var Ꮡv);
    var vʗ1 = v;
    b.RunParallel((ж<Δtesting.PB> pb) => {
        while (pb.Next()) {
            vʗ1.Interface();
        }
    });
}

public static void BenchmarkNew(ж<Δtesting.B> b) {
    var v = TypeOf(new XM());
    var vʗ1 = v;
    b.RunParallel((ж<Δtesting.PB> pb) => {
        while (pb.Next()) {
            New(vʗ1);
        }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string mapIndexˢ => ref mapIndexˢᵇ;
private static readonly @string mapIndexˢᵇ = "MapIndex"u8;
internal static ref readonly @string setMapIndexˢ => ref setMapIndexˢᵇ;
private static readonly @string setMapIndexˢᵇ = "SetMapIndex"u8;

internal partial class BenchmarkMap_V /*ж<nint>*/;

internal partial struct BenchmarkMap_S /*@string*/;

internal partial struct BenchmarkMap_tests /*dyn*/ {
    internal @string label;
    internal reflectꓸValue m, keys, value;
}

public static void BenchmarkMap(ж<Δtesting.B> b) {
    var value = ValueOf((BenchmarkMap_V)nil);
    var stringKeys = slice<@string>([]);
    var mapOfStrings = new map<@string, BenchmarkMap_V>{};
    var uint64Keys = slice<uint64>([]);
    var mapOfUint64s = new map<uint64, BenchmarkMap_V>{};
    var userStringKeys = slice<BenchmarkMap_S>([]);
    var mapOfUserStrings = new map<BenchmarkMap_S, BenchmarkMap_V>{};
    for (nint i = 0; i < 100; i++) {
        @string stringKey = fmt.Sprintf("key%d"u8, i);
        stringKeys = append(stringKeys, stringKey);
        mapOfStrings[stringKey] = default;

        var uint64Key = (uint64)i;
        uint64Keys = append(uint64Keys, uint64Key);
        mapOfUint64s[uint64Key] = default;

        BenchmarkMap_S userStringKey = (BenchmarkMap_S)fmt.Sprintf("key%d"u8, i);
        userStringKeys = append(userStringKeys, userStringKey);
        mapOfUserStrings[userStringKey] = default;
    }

    var tests = slice<BenchmarkMap_tests>([
        new("StringKeys", ValueOf(mapOfStrings), ValueOf(stringKeys), value),
        new("Uint64Keys", ValueOf(mapOfUint64s), ValueOf(uint64Keys), value),
        new("UserStringKeys", ValueOf(mapOfUserStrings), ValueOf(userStringKeys), value)
    ]);

    foreach (var (_, vᴛ1) in range(tests)) {
        ref var tt = ref heap(new BenchmarkMap_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        b.Run(tt.label, (ж<Δtesting.B> bΔ1) => {
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

    var m = ValueOf(new map<@string, nint>{["a"] = 0, ["b"] = 1, ["c"] = 2, ["d"] = 3});
    var it = m.MapRange();
    for (nint i = 0; i < b.N; i++) {
        while (it.Next()) {
        }
        it.Reset(m);
    }
}

} // end reflect_test_package
