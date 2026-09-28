// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.encoding;

using bytes = bytes_package;
using fmt = fmt_package;
using asan = @internal.asan_package;
using io = io_package;
using math = math_package;
using reflect = reflect_package;
using strings = strings_package;
using sync = sync_package;
using testing = testing_package;
using @unsafe = unsafe_package;
using @internal;
using static go.encoding.binary_package;

partial class binary_internal_test_package {

[GoType] [GoValueClone("Array", "BoolArray")] public partial struct Struct {
    public int8 Int8;
    public int16 Int16;
    public int32 Int32;
    public int64 Int64;
    public uint8 Uint8;
    public uint16 Uint16;
    public uint32 Uint32;
    public uint64 Uint64;
    public float32 Float32;
    public float64 Float64;
    public complex64 Complex64;
    public complex128 Complex128;
    public array<uint8> Array = new(4);
    public bool Bool;
    public array<bool> BoolArray = new(4);
}

[GoType] [GoValueClone("Array")] public partial struct T {
    public nint Int;
    public nuint Uint;
    public uintptr Uintptr;
    public array<nint> Array = new(4);
}

internal static ж<Struct> Ꮡs = new StandardBox<Struct>(new Struct(
    0x01,
    0x0203,
    0x04050607,
    0x08090a0b0c0d0e0fL,
    0x10,
    0x1112,
    0x13141516,
    0x1718191a1b1c1d1eUL,
    math.Float32frombits(0x1f202122),
    math.Float64frombits(0x232425262728292aUL),
    complex(
        math.Float32frombits(0x2b2c2d2e),
        math.Float32frombits(0x2f303132)),
    complex(
        math.Float64frombits(0x333435363738393aUL),
        math.Float64frombits(0x3b3c3d3e3f404142UL)),
    new uint8[]{0x43, 0x44, 0x45, 0x46}.array(),
    true,
    new bool[]{true, false, true, false}.array()
));
internal static ref Struct s => ref Ꮡs.Value;

internal static slice<byte> big = new byte[]{
    1,
    2, 3,
    4, 5, 6, 7,
    8, 9, 10, 11, 12, 13, 14, 15,
    16,
    17, 18,
    19, 20, 21, 22,
    23, 24, 25, 26, 27, 28, 29, 30,
    31, 32, 33, 34,
    35, 36, 37, 38, 39, 40, 41, 42,
    43, 44, 45, 46, 47, 48, 49, 50,
    51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 66,
    67, 68, 69, 70,
    1,
    1, 0, 1, 0
}.slice();

internal static slice<byte> little = new byte[]{
    1,
    3, 2,
    7, 6, 5, 4,
    15, 14, 13, 12, 11, 10, 9, 8,
    16,
    18, 17,
    22, 21, 20, 19,
    30, 29, 28, 27, 26, 25, 24, 23,
    34, 33, 32, 31,
    42, 41, 40, 39, 38, 37, 36, 35,
    46, 45, 44, 43, 50, 49, 48, 47,
    58, 57, 56, 55, 54, 53, 52, 51, 66, 65, 64, 63, 62, 61, 60, 59,
    67, 68, 69, 70,
    1,
    1, 0, 1, 0
}.slice();

internal static slice<byte> src = new byte[]{1, 2, 3, 4, 5, 6, 7, 8}.slice();

internal static slice<int32> res = new int32[]{0x01020304, 0x05060708}.slice();

internal static slice<byte> putbuf = new byte[]{0, 0, 0, 0, 0, 0, 0, 0}.slice();

internal static void checkResult(ж<testing.T> Ꮡt, @string dir, global::go.encoding.binary_package.ByteOrder order, error err, any have, any want) {
    Ꮡt.Helper();
    if (err != default!) {
        Ꮡt.Errorf("%v %v: %v"u8, dir, order, err);
        return;
    }
    if (!reflect.DeepEqual(have, want)) {
        Ꮡt.Errorf("%v %v:\n\thave %+v\n\twant %+v"u8, dir, order, have, want);
    }
}


[GoType("dyn")] partial struct encodersᴛ1 {
    internal @string name;
    internal Func<global::go.encoding.binary_package.ByteOrder, any, (slice<byte>, error)> fn;
}
internal static slice<encodersᴛ1> encoders;
internal static void initᴛencoders() { encoders = new encodersᴛ1[]{
    new(
        "Write"u8,
        (global::go.encoding.binary_package.ByteOrder order, any data) => {
            var buf = @new<bytes.Buffer>();
            var err = Write(new binary_test_package.bytes_BufferжWriter(buf), order, data);
            return (buf.Bytes(), err);
        }
    ),
    new(
        "Encode"u8,
        (global::go.encoding.binary_package.ByteOrder order, any data) => {
            nint size = Size(data);
            slice<byte> buf = default!;
            if (size > 0) {
                buf = new slice<byte>(Size(data));
            }
            var (n, err) = Encode(buf, order, data);
            if (err == default! && n != size) {
                return (default!, fmt.Errorf("returned size %d instead of %d"u8, n, size));
            }
            return (buf, err);
        }
    ), new(
    "Append"u8,
    (global::go.encoding.binary_package.ByteOrder order, any data) => Append(default!, order, data)
)
}.slice(); }


[GoType("dyn")] partial struct decodersᴛ1 {
    internal @string name;
    internal Func<global::go.encoding.binary_package.ByteOrder, any, slice<byte>, error> fn;
}
internal static slice<decodersᴛ1> decoders;
internal static void initᴛdecoders() { decoders = new decodersᴛ1[]{
    new(
        "Read"u8,
        (global::go.encoding.binary_package.ByteOrder order, any data, slice<byte> buf) => go.encoding.binary_package.Read(new binary_test_package.bytes_ReaderжReader(bytes.NewReader(buf)), order, data)
    ),
    new(
        "Decode"u8,
        (global::go.encoding.binary_package.ByteOrder order, any data, slice<byte> buf) => {
            var (n, err) = Decode(buf, order, data);
            if (err == default! && n != Size(data)) {
                return fmt.Errorf("returned size %d instead of %d"u8, n, Size(data));
            }
            return err;
        }
    )
}.slice(); }

internal static void testRead(ж<testing.T> Ꮡt, global::go.encoding.binary_package.ByteOrder order, slice<byte> b, any s1) {
    Ꮡt.Helper();
    foreach (var (_, vᴛ1) in decoders) {
        ref var dec = ref heap(new decodersᴛ1(), out var Ꮡdec);
        dec = vᴛ1;

        var bʗ1 = b;
        var decʗ1 = dec;
        Ꮡt.Run(dec.name, (ж<testing.T> tΔ1) => {
            ref var s2 = ref heap(new Struct(), out var Ꮡs2);
            var err = decʗ1.fn(order, Ꮡs2, bʗ1);
            checkResult(tΔ1, decʗ1.name, order, err, s2, s1);
        });
    }
}

internal static void testWrite(ж<testing.T> Ꮡt, global::go.encoding.binary_package.ByteOrder order, slice<byte> b, any s1) {
    Ꮡt.Helper();
    foreach (var (_, vᴛ1) in encoders) {
        ref var enc = ref heap(new encodersᴛ1(), out var Ꮡenc);
        enc = vᴛ1;

        var bʗ1 = b;
        var encʗ1 = enc;
        Ꮡt.Run(enc.name, (ж<testing.T> tΔ1) => {
            var (buf, err) = encʗ1.fn(order, s1);
            checkResult(tΔ1, encʗ1.name, order, err, buf, bʗ1);
        });
    }
}

public static void TestLittleEndianRead(ж<testing.T> Ꮡt) {
    testRead(Ꮡt, LittleEndian, little, s);
}

public static void TestLittleEndianWrite(ж<testing.T> Ꮡt) {
    testWrite(Ꮡt, LittleEndian, little, s);
}

public static void TestLittleEndianPtrWrite(ж<testing.T> Ꮡt) {
    testWrite(Ꮡt, LittleEndian, little, Ꮡs);
}

public static void TestBigEndianRead(ж<testing.T> Ꮡt) {
    testRead(Ꮡt, BigEndian, big, s);
}

public static void TestBigEndianWrite(ж<testing.T> Ꮡt) {
    testWrite(Ꮡt, BigEndian, big, s);
}

public static void TestBigEndianPtrWrite(ж<testing.T> Ꮡt) {
    testWrite(Ꮡt, BigEndian, big, Ꮡs);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string readˢ = "Read"u8;
internal static readonly @string readSliceˢ = "ReadSlice"u8;
internal static readonly @string decodeˢ = "Decode"u8;

public static void TestReadSlice(ж<testing.T> Ꮡt) {
    Ꮡt.Run(readˢ, (ж<testing.T> tΔ1) => {
        var Δslice = new slice<int32>(2);
        var err = go.encoding.binary_package.Read(new binary_test_package.bytes_ReaderжReader(bytes.NewReader(src)), BigEndian, Δslice);
        checkResult(tΔ1, readSliceˢ, BigEndian, err, Δslice, res);
    });
    Ꮡt.Run(decodeˢ, (ж<testing.T> tΔ2) => {
        var Δslice = new slice<int32>(2);
        var (_, err) = Decode(src, BigEndian, Δslice);
        checkResult(tΔ2, readSliceˢ, BigEndian, err, Δslice, res);
    });
}

public static void TestWriteSlice(ж<testing.T> Ꮡt) {
    testWrite(Ꮡt, BigEndian, src, res);
}

public static void TestReadBool(ж<testing.T> Ꮡt) {
    foreach (var (_, vᴛ1) in decoders) {
        ref var dec = ref heap(new decodersᴛ1(), out var Ꮡdec);
        dec = vᴛ1;

        var decʗ1 = dec;
        Ꮡt.Run(dec.name, (ж<testing.T> tΔ1) => {
            ref var res = ref heap(new bool(), out var Ꮡres);
            error err = default!;
            err = decʗ1.fn(BigEndian, Ꮡres, new byte[]{0}.slice());
            checkResult(tΔ1, decʗ1.name, BigEndian, err, res, false);
            res = false;
            err = decʗ1.fn(BigEndian, Ꮡres, new byte[]{1}.slice());
            checkResult(tΔ1, decʗ1.name, BigEndian, err, res, true);
            res = false;
            err = decʗ1.fn(BigEndian, Ꮡres, new byte[]{2}.slice());
            checkResult(tΔ1, decʗ1.name, BigEndian, err, res, true);
        });
    }
}

public static void TestReadBoolSlice(ж<testing.T> Ꮡt) {
    foreach (var (_, vᴛ1) in decoders) {
        ref var dec = ref heap(new decodersᴛ1(), out var Ꮡdec);
        dec = vᴛ1;

        var decʗ1 = dec;
        Ꮡt.Run(dec.name, (ж<testing.T> tΔ1) => {
            var Δslice = new slice<bool>(4);
            var err = decʗ1.fn(BigEndian, Δslice, new byte[]{0, 1, 2, 255}.slice());
            checkResult(tΔ1, decʗ1.name, BigEndian, err, Δslice, new bool[]{false, true, true, true}.slice());
        });
    }
}

// Addresses of arrays are easier to manipulate with reflection than are slices.
internal static slice<any> intArrays = new any[]{
    Ꮡ(new int8[]{}.array(100)),
    Ꮡ(new int16[]{}.array(100)),
    Ꮡ(new int32[]{}.array(100)),
    Ꮡ(new int64[]{}.array(100)),
    Ꮡ(new uint8[]{}.array(100)),
    Ꮡ(new uint16[]{}.array(100)),
    Ꮡ(new uint32[]{}.array(100)),
    Ꮡ(new uint64[]{}.array(100))
}.slice();

public static void TestSliceRoundTrip(ж<testing.T> Ꮡt) {
    foreach (var (_, vᴛ1) in encoders) {
        ref var enc = ref heap(new encodersᴛ1(), out var Ꮡenc);
        enc = vᴛ1;

        foreach (var (_, vᴛ2) in decoders) {
            ref var dec = ref heap(new decodersᴛ1(), out var Ꮡdec);
            dec = vᴛ2;

            var decʗ1 = dec;
            var encʗ1 = enc;
            Ꮡt.Run(fmt.Sprintf("%s,%s"u8, enc.name, dec.name), (ж<testing.T> tΔ1) => {
                foreach (var (_, Δarray) in intArrays) {
                    ref var src = ref heap<reflectꓸValue>(out var Ꮡsrc);
                    src = reflect.ValueOf(Δarray).Elem();
                    var decʗ2 = decʗ1;
                    var encʗ2 = encʗ1;
                    var srcʗ1 = src;
                    tΔ1.Run(src.Index(0).Type().Name(), (ж<testing.T> tΔ2) => {
                        var unsigned = false;
                        var exprᴛ1 = srcʗ1.Index(0).Kind();
                        if (exprᴛ1 == reflect.Uint8 || exprᴛ1 == reflect.Uint16 || exprᴛ1 == reflect.Uint32 || exprᴛ1 == reflect.Uint64) {
                            unsigned = true;
                        }

                        for (nint i = 0; i < srcʗ1.Len(); i++) {
                            if (unsigned){
                                srcʗ1.Index(i).SetUint((uint64)(i * 0x07654321));
                            } else {
                                srcʗ1.Index(i).SetInt((int64)(i * 0x07654321));
                            }
                        }
                        var srcSlice = srcʗ1.Slice(0, srcʗ1.Len());
                        var (buf, err) = encʗ2.fn(BigEndian, srcSlice.Interface());
                        if (err != default!) {
                            tΔ2.Fatal(err);
                        }
                        var dst = reflect.New(srcʗ1.Type()).Elem();
                        var dstSlice = dst.Slice(0, dst.Len());
                        err = decʗ2.fn(BigEndian, dstSlice.Interface(), buf);
                        if (err != default!) {
                            tΔ2.Fatal(err);
                        }
                        if (!reflect.DeepEqual(srcʗ1.Interface(), dst.Interface())) {
                            tΔ2.Log(dst);
                            tΔ2.Fatal(srcʗ1);
                        }
                    });
                }
            });
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string intˢ = "int"u8;

public static void TestWriteT(ж<testing.T> Ꮡt) {
    foreach (var (_, vᴛ1) in encoders) {
        ref var enc = ref heap(new encodersᴛ1(), out var Ꮡenc);
        enc = vᴛ1;

        var encʗ1 = enc;
        Ꮡt.Run(enc.name, (ж<testing.T> tΔ1) => {
            var ts = new T(nil);
            {
                var (_, err) = encʗ1.fn(BigEndian, ts); if (err == default!) {
                    tΔ1.Errorf("WriteT: have err == nil, want non-nil"u8);
                }
            }
            var tv = reflect.Indirect(reflect.ValueOf(ts));
            for ((nint i, nint n) = (0, tv.NumField()); i < n; i++) {
                @string typ = tv.Field(i).Type().String();
                if (typ == "[4]int"u8) {
                    typ = intˢ; // the problem is int, not the [4]
                }
                {
                    var (_, err) = encʗ1.fn(BigEndian, tv.Field(i).Interface()); if (err == default!){
                        tΔ1.Errorf("WriteT.%v: have err == nil, want non-nil"u8, tv.Field(i).Type());
                    } else 
                    if (!strings.Contains(err.Error(), typ)) {
                        tΔ1.Errorf("WriteT: have err == %q, want it to mention %s"u8, err, typ);
                    }
                }
            }
        });
    }
}

[GoType("dyn")] [GoValueClone("f")] partial struct BlankFields__ {
    internal array<float32> f = new(8);
}

[GoType] public partial struct BlankFields {
    public uint32 A;
    internal int32 _;
    public float64 B;
    internal array<int16> __ = new(4);
    public byte C;
    internal array<byte> ___ = new(7);
    internal BlankFields__ ____;
}

[GoType("dyn")] [GoValueClone("F")] partial struct BlankFieldsProbe_P3 {
    public array<float32> F = new(8);
}

[GoType] [GoValueClone("P1", "P2", "P3")] public partial struct BlankFieldsProbe {
    public uint32 A;
    public int32 P0;
    public float64 B;
    public array<int16> P1 = new(4);
    public byte C;
    public array<byte> P2 = new(7);
    public BlankFieldsProbe_P3 P3;
}

public static void TestBlankFields(ж<testing.T> Ꮡt) {
    foreach (var (_, vᴛ1) in encoders) {
        ref var enc = ref heap(new encodersᴛ1(), out var Ꮡenc);
        enc = vᴛ1;

        var encʗ1 = enc;
        Ꮡt.Run(enc.name, (ж<testing.T> tΔ1) => {
            ref var b1 = ref heap<BlankFields>(out var Ꮡb1);
            b1 = new BlankFields(A: 1234567890, B: 2.718281828D, C: 42);
            var (buf, err) = encʗ1.fn(LittleEndian, Ꮡb1);
            if (err != default!) {
                tΔ1.Error(err);
            }
            // zero values must have been written for blank fields
            ref var p = ref heap(new BlankFieldsProbe(), out var Ꮡp);
            {
                var errΔ1 = go.encoding.binary_package.Read(new binary_test_package.bytes_ReaderжReader(bytes.NewReader(buf)), LittleEndian, Ꮡp); if (errΔ1 != default!) {
                    tΔ1.Error(errΔ1);
                }
            }
            // quick test: only check first value of slices
            if (p.P0 != 0 || p.P1[0] != 0 || p.P2[0] != 0 || p.P3.F[0] != 0F) {
                tΔ1.Errorf("non-zero values for originally blank fields: %#v"u8, p);
            }
            // write p and see if we can probe only some fields
            (buf, err) = encʗ1.fn(LittleEndian, Ꮡp);
            if (err != default!) {
                tΔ1.Error(err);
            }
            // read should ignore blank fields in b2
            ref var b2 = ref heap(new BlankFields(), out var Ꮡb2);
            {
                var errΔ2 = go.encoding.binary_package.Read(new binary_test_package.bytes_ReaderжReader(bytes.NewReader(buf)), LittleEndian, Ꮡb2); if (errΔ2 != default!) {
                    tΔ1.Error(errΔ2);
                }
            }
            if (b1.A != b2.A || b1.B != b2.B || b1.C != b2.C) {
                tΔ1.Errorf("%#v != %#v"u8, b1, b2);
            }
        });
    }
}

[GoType("dyn")] [GoLocalName("foo")] internal partial struct TestSizeStructCache_foo {
    public uint32 A;
}

[GoType("dyn")] [GoLocalName("bar")] [GoValueClone("A", "C")] internal partial struct TestSizeStructCache_bar {
    public Struct A;
    public TestSizeStructCache_foo B;
    public Struct C;
}

[GoType("dyn")] internal partial struct TestSizeStructCache_testcases {
    internal any val;
    internal nint want;
}

[GoType("dyn")] [GoValueClone("A")] internal partial struct TestSizeStructCache_type {
    public Struct A;
}

public static void TestSizeStructCache(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // Reset the cache, otherwise multiple test runs fail.
    structSize = new sync.Map(nil);
    nint count() {
        nint i = default!;
        ᏑstructSize.Range((any _Δp0, any _Δp1) => {
            i++;
            return true;
        });
        return i;
    }
    nint total = default!;
    var countʗ1 = count;
    nint added() {
        nint delta = countʗ1() - total;
        total += delta;
        return delta;
    }
    var testcases = new TestSizeStructCache_testcases[]{
        new(@new<TestSizeStructCache_foo>(), 1),
        new(Ꮡ(new array<TestSizeStructCache_foo>(1)), 0),
        new(new slice<TestSizeStructCache_foo>(1), 0),
        new(@new<TestSizeStructCache_bar>(), 1),
        new(@new<TestSizeStructCache_bar>(), 0),
        new(@new<TestSizeStructCache_type>(), 1),
        new(@new<TestSizeStructCache_type>(), 0),
        new(Ꮡ(new array<TestSizeStructCache_type>(1, () => new())), 0),
        new(new slice<TestSizeStructCache_type>(1, () => new()), 0)
    }.slice();
    foreach (var (_, tc) in testcases) {
        if (Size(tc.val) == -1) {
            Ꮡt.Fatalf("Can't get the size of %T"u8, tc.val);
        }
        {
            nint n = added(); if (n != tc.want) {
                Ꮡt.Errorf("Sizing %T added %d entries to the cache, want %d"u8, tc.val, n, tc.want);
            }
        }
    }
}

public static void TestSizeInvalid(ж<testing.T> Ꮡt) {
    var testcases = new any[]{
        (nint)0,
        @new<nint>(),
        ((ж<nint>)nil),
        new nuint[]{}.array(1),
        Ꮡ(new array<nuint>(1)),
        ж<array<nuint>>.NilBoxOfDims(1L),
        new nint[]{}.slice(),
        slice<nint>(default!),
        @new<slice<nint>>(),
        ((ж<slice<nint>>)nil),
        ((ж<int8>)nil),
        ((ж<uint8>)nil),
        ((ж<int16>)nil),
        ((ж<uint16>)nil),
        ((ж<int32>)nil),
        ((ж<uint32>)nil),
        ((ж<int64>)nil),
        ((ж<uint64>)nil),
        ((ж<float32>)nil),
        ((ж<float64>)nil),
        ((ж<complex64>)nil),
        ((ж<complex128>)nil)
    }.slice();
    foreach (var (_, tc) in testcases) {
        {
            nint got = Size(tc); if (got != -1) {
                Ꮡt.Errorf("Size(%T) = %d, want -1"u8, tc, got);
            }
        }
    }
}

// An attempt to read into a struct with an unexported field will
// panic. This is probably not the best choice, but at this point
// anything else would be an API change.
[GoType] public partial struct Unexported {
    internal int32 a;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object didNotPanicˢ = (@string)"did not panic"u8;

public static void TestUnexportedRead(ж<testing.T> Ꮡt) {
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    ref var u1 = ref heap<Unexported>(out var Ꮡu1);
    u1 = new Unexported(a: 1);
    {
        var err = Write(new binary_test_package.bytes_BufferжWriter(Ꮡbuf), LittleEndian, Ꮡu1); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    foreach (var (_, vᴛ1) in decoders) {
        ref var dec = ref heap(new decodersᴛ1(), out var Ꮡdec);
        dec = vᴛ1;

        var decʗ1 = dec;
        Ꮡt.Run(dec.name, (ж<testing.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                defer(() => {
                    if (recover() == default!) {
                        tΔ1.Fatal(didNotPanicˢ);
                    }
                }, ref ᒐ);
                ref var u2 = ref heap(new Unexported(), out var Ꮡu2);
                decʗ1.fn(LittleEndian, Ꮡu2, Ꮡbuf.Value.Bytes());
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

public static void TestReadErrorMsg(ж<testing.T> Ꮡt) {
    foreach (var (_, vᴛ1) in decoders) {
        ref var dec = ref heap(new decodersᴛ1(), out var Ꮡdec);
        dec = vᴛ1;

        var decʗ1 = dec;
        Ꮡt.Run(dec.name, (ж<testing.T> tΔ1) => {
            var decʗ2 = decʗ1;
            void read(any data) {
                var err = decʗ2.fn(LittleEndian, data, default!);
                @string want = fmt.Sprintf("binary.%s: invalid type %s"u8, decʗ2.name, reflect.TypeOf(data).String());
                if (err == default!) {
                    tΔ1.Errorf("%T: got no error; want %q"u8, data, want);
                    return;
                }
                {
                    @string got = err.Error(); if (got != want) {
                        tΔ1.Errorf("%T: got %q; want %q"u8, data, got, want);
                    }
                }
            }
            read((nint)(0));
            ref var s = ref heap<ж<EmptyStruct>>(out var Ꮡs);
            s = @new<EmptyStruct>();
            read(Ꮡs);
            ref var p = ref heap<ж<ж<EmptyStruct>>>(out var Ꮡp);
            p = Ꮡs;
            read(Ꮡp);
        });
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string dataᶜ = "0123456789abcdef"u8;

[GoType("dyn")] internal partial struct TestReadTruncated_b2 {
    public byte A, B, C, D;
    public int32 E;
    public float64 F;
}

public static void TestReadTruncated(ж<testing.T> Ꮡt) {
    @string data = dataᶜ;
    ref var b1 = ref heap<slice<int32>>(out var Ꮡb1);

    b1 = new slice<int32>(4);
    ref var b2 = ref heap(new TestReadTruncated_b2(), out var Ꮡb2);
    for (nint i = 0; i <= len(data); i++) {
        error errWant = default!;
        var exprᴛ1 = i;
        if (exprᴛ1 is 0) {
            errWant = io.EOF;
        }
        else if (exprᴛ1 == len(data)) {
            errWant = default!;
        }
        else { /* default: */
            errWant = io.ErrUnexpectedEOF;
        }

        {
            var err = go.encoding.binary_package.Read(new binary_test_package.strings_ReaderжReader(strings.NewReader(data[..(int)(i)])), LittleEndian, Ꮡb1); if (!AreEqual(err, errWant)) {
                Ꮡt.Errorf("Read(%d) with slice: got %v, want %v"u8, i, err, errWant);
            }
        }
        {
            var err = go.encoding.binary_package.Read(new binary_test_package.strings_ReaderжReader(strings.NewReader(data[..(int)(i)])), LittleEndian, Ꮡb2); if (!AreEqual(err, errWant)) {
                Ꮡt.Errorf("Read(%d) with struct: got %v, want %v"u8, i, err, errWant);
            }
        }
    }
}

internal static bool /*panicked*/ testUint64SmallSliceLengthPanics() {
    bool panicked = default!;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            panicked = recover() != default!;
        }, ref ᒐ);
        var b = new byte[]{1, 2, 3, 4, 5, 6, 7, 8}.array();
        LittleEndian.Uint64(b[..4]);
        panicked = false;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return panicked;
}

internal static bool /*panicked*/ testPutUint64SmallSliceLengthPanics() {
    bool panicked = default!;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            panicked = recover() != default!;
        }, ref ᒐ);
        var b = new byte[]{}.array(8);
        LittleEndian.PutUint64(b[..4], 0x0102030405060708UL);
        panicked = false;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return panicked;
}

[GoType("dyn")] internal partial interface TestByteOrder_byteOrder :
    ByteOrder,
    AppendByteOrder
{
}

public static void TestByteOrder(ж<testing.T> Ꮡt) {
    var buf = new slice<byte>(8);
    foreach (var (_, order) in new TestByteOrder_byteOrder[]{new binary_internal_test_package.binary_littleEndianᴠTestByteOrder_byteOrder(LittleEndian), new binary_internal_test_package.binary_bigEndianᴠTestByteOrder_byteOrder(BigEndian)}.slice()) {
        UntypedInt offset = 3;
        foreach (var (_, value) in new uint64[]{
            0x0000000000000000,
            0x0123456789abcdefUL,
            0xfedcba9876543210UL,
            0xffffffffffffffffUL,
            0xaaaaaaaaaaaaaaaaUL,
            math.Float64bits(math.Pi),
            math.Float64bits(math.E)
        }.slice()) {
            var want16 = (uint16)value;
            order.PutUint16(buf[..2], want16);
            {
                var got = order.Uint16(buf[..2]); if (got != want16) {
                    Ꮡt.Errorf("PutUint16: Uint16 = %v, want %v"u8, got, want16);
                }
            }
            buf = order.AppendUint16(buf[..(int)(offset)], want16);
            {
                var got = order.Uint16(buf[(int)(offset)..]); if (got != want16) {
                    Ꮡt.Errorf("AppendUint16: Uint16 = %v, want %v"u8, got, want16);
                }
            }
            if (len(buf) != (nint)(offset + 2)) {
                Ꮡt.Errorf("AppendUint16: len(buf) = %d, want %d"u8, len(buf), (nint)(offset + 2));
            }
            var want32 = (uint32)value;
            order.PutUint32(buf[..4], want32);
            {
                var got = order.Uint32(buf[..4]); if (got != want32) {
                    Ꮡt.Errorf("PutUint32: Uint32 = %v, want %v"u8, got, want32);
                }
            }
            buf = order.AppendUint32(buf[..(int)(offset)], want32);
            {
                var got = order.Uint32(buf[(int)(offset)..]); if (got != want32) {
                    Ꮡt.Errorf("AppendUint32: Uint32 = %v, want %v"u8, got, want32);
                }
            }
            if (len(buf) != (nint)(offset + 4)) {
                Ꮡt.Errorf("AppendUint32: len(buf) = %d, want %d"u8, len(buf), (nint)(offset + 4));
            }
            var want64 = (uint64)value;
            order.PutUint64(buf[..8], want64);
            {
                var got = order.Uint64(buf[..8]); if (got != want64) {
                    Ꮡt.Errorf("PutUint64: Uint64 = %v, want %v"u8, got, want64);
                }
            }
            buf = order.AppendUint64(buf[..(int)(offset)], want64);
            {
                var got = order.Uint64(buf[(int)(offset)..]); if (got != want64) {
                    Ꮡt.Errorf("AppendUint64: Uint64 = %v, want %v"u8, got, want64);
                }
            }
            if (len(buf) != (nint)(offset + 8)) {
                Ꮡt.Errorf("AppendUint64: len(buf) = %d, want %d"u8, len(buf), (nint)(offset + 8));
            }
        }
    }
}

public static void TestEarlyBoundsChecks(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (testUint64SmallSliceLengthPanics() != true) {
        Ꮡt.Errorf("binary.LittleEndian.Uint64 expected to panic for small slices, but didn't"u8);
    }
    if (testPutUint64SmallSliceLengthPanics() != true) {
        Ꮡt.Errorf("binary.LittleEndian.PutUint64 expected to panic for small slices, but didn't"u8);
    }
}

public static void TestReadInvalidDestination(ж<testing.T> Ꮡt) {
    testReadInvalidDestination(Ꮡt, BigEndian);
    testReadInvalidDestination(Ꮡt, LittleEndian);
}

internal static void testReadInvalidDestination(ж<testing.T> Ꮡt, global::go.encoding.binary_package.ByteOrder order) {
    var destinations = new any[]{
        (int8)0,
        (int16)0,
        (int32)0,
        (int64)0,
        (uint8)0,
        (uint16)0,
        (uint32)0,
        (uint64)0,
        (bool)false
    }.slice();
    foreach (var (_, dst) in destinations) {
        var err = go.encoding.binary_package.Read(new binary_test_package.bytes_ReaderжReader(bytes.NewReader(new byte[]{1, 2, 3, 4, 5, 6, 7, 8}.slice())), order, dst);
        @string want = fmt.Sprintf("binary.Read: invalid type %T"u8, dst);
        if (err == default! || err.Error() != want) {
            Ꮡt.Fatalf("for type %T: got %q; want %q"u8, dst, err, want);
        }
    }
}

[GoType("dyn")] [GoLocalName("Person")] internal partial struct TestNoFixedSize_Person {
    public nint Age;
    public float64 Weight;
    public float64 Height;
}

public static void TestNoFixedSize(ж<testing.T> Ꮡt) {
    ref var person = ref heap<TestNoFixedSize_Person>(out var Ꮡperson);
    person = new TestNoFixedSize_Person(
        Age: 27,
        Weight: 67.3D,
        Height: 177.8D
    );
    foreach (var (_, vᴛ1) in encoders) {
        ref var enc = ref heap(new encodersᴛ1(), out var Ꮡenc);
        enc = vᴛ1;

        var encʗ1 = enc;
        Ꮡt.Run(enc.name, (ж<testing.T> tΔ1) => {
            var (_, err) = encʗ1.fn(LittleEndian, Ꮡperson);
            if (err == default!) {
                tΔ1.Fatalf("binary.%s: unexpected success as size of type *binary.Person is not fixed"u8, encʗ1.name);
            }
            @string errs = fmt.Sprintf("binary.%s: some values are not fixed-sized in type *binary.Person"u8, encʗ1.name);
            if (err.Error() != errs) {
                tΔ1.Fatalf("got %q, want %q"u8, err, errs);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object testAllocatesMoreWithˢ = (@string)"test allocates more with -asan; see #70079"u8;
internal static readonly object appendFailedˢ = (@string)"Append failed:"u8;

public static void TestAppendAllocs(ж<testing.T> Ꮡt) {
    if (asan.Enabled) {
        Ꮡt.Skip(testAllocatesMoreWithˢ);
    }
    var buf = new slice<byte>(0, Size(Ꮡs));
    ref var err = ref heap<error>(out var Ꮡerr);
    var bufʗ1 = buf;
    var allocs = testing.AllocsPerRun(1, () => {
        (_, Ꮡerr.ValueSlot) = Append(bufʗ1, LittleEndian, Ꮡs);
    });
    if (err != default!) {
        Ꮡt.Fatal(appendFailedˢ, err);
    }
    if (allocs != 0D) {
        Ꮡt.Fatalf("Append allocated %v times instead of not allocating at all"u8, allocs);
    }
}

internal static slice<any> sizableTypes = new any[]{
    (bool)false,
    (int8)0,
    (int16)0,
    (int32)0,
    (int64)0,
    (uint8)0,
    (uint16)0,
    (uint32)0,
    (uint64)0,
    (float32)0F,
    (float64)0D,
    (complex64)0F,
    (complex128)0D,
    new Struct(nil),
    Ꮡ(new Struct(nil)),
    new Struct[]{}.slice(),
    (slice<Struct>)(default!),
    new Struct[]{}.array(1, () => new())
}.slice();

public static void TestSizeAllocs(ж<testing.T> Ꮡt) {
    if (asan.Enabled) {
        Ꮡt.Skip(testAllocatesMoreWithˢ);
    }
    foreach (var (_, data) in sizableTypes) {
        var dataʗ1 = data;
        Ꮡt.Run(fmt.Sprintf("%T"u8, data), (ж<testing.T> tΔ1) => {
            // Size uses a sync.Map behind the scenes. The slow lookup path of
            // that does allocate, so we need a couple of runs here to be
            // allocation free.
            var dataʗ2 = dataʗ1;
            var allocs = testing.AllocsPerRun(10, () => {
                _ = Size(dataʗ2);
            });
            if (allocs != 0D) {
                tΔ1.Fatalf("Expected no allocations, got %v"u8, allocs);
            }
        });
    }
}

[GoType] internal partial struct byteSliceReader {
    internal slice<byte> remain;
}

[GoRecv] internal static (nint, error) Read(this ref byteSliceReader br, slice<byte> p) {
    nint n = copy(p, br.remain);
    br.remain = br.remain[(int)(n)..];
    return (n, default!);
}

public static void BenchmarkReadSlice1000Int32s(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var bsr = Ꮡ(new byteSliceReader(nil));
    var Δslice = new slice<int32>(1000);
    var buf = new slice<byte>(len(Δslice) * 4);
    b.SetBytes((int64)len(buf));
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        bsr.Value.remain = buf;
        go.encoding.binary_package.Read(new binary_internal_test_package.byteSliceReaderжReader(bsr), BigEndian, Δslice);
    }
}

public static void BenchmarkReadStruct(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var bsr = Ꮡ(new byteSliceReader(nil));
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    Write(new binary_test_package.bytes_BufferжWriter(Ꮡbuf), BigEndian, Ꮡs);
    b.SetBytes((int64)dataSize(reflect.ValueOf(s)));
    ref var t = ref heap<Struct>(out var Ꮡt);
    t = s.ΔClone();
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        bsr.Value.remain = buf.Bytes();
        go.encoding.binary_package.Read(new binary_internal_test_package.byteSliceReaderжReader(bsr), BigEndian, Ꮡt);
    }
    b.StopTimer();
    if (b.N > 0 && !reflect.DeepEqual(s, t)) {
        Ꮡb.Fatalf("struct doesn't match:\ngot  %v;\nwant %v"u8, t, s);
    }
}

public static void BenchmarkWriteStruct(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.SetBytes((int64)Size(Ꮡs));
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        Write(io.Discard, BigEndian, Ꮡs);
    }
}

public static void BenchmarkAppendStruct(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var buf = new slice<byte>(0, Size(Ꮡs));
    b.SetBytes((int64)cap(buf));
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        Encode(buf, BigEndian, Ꮡs);
    }
}

public static void BenchmarkWriteSlice1000Structs(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var Δslice = new slice<Struct>(1000, () => new());
    var buf = @new<bytes.Buffer>();
    io.Writer w = new binary_test_package.bytes_BufferжWriter(buf);
    b.SetBytes((int64)Size(Δslice));
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        buf.Reset();
        Write(w, BigEndian, Δslice);
    }
    b.StopTimer();
}

public static void BenchmarkAppendSlice1000Structs(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var Δslice = new slice<Struct>(1000, () => new());
    var buf = new slice<byte>(0, Size(Δslice));
    b.SetBytes((int64)cap(buf));
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        Append(buf, BigEndian, Δslice);
    }
    b.StopTimer();
}

public static void BenchmarkReadSlice1000Structs(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var bsr = Ꮡ(new byteSliceReader(nil));
    var Δslice = new slice<Struct>(1000, () => new());
    var buf = new slice<byte>(Size(Δslice));
    b.SetBytes((int64)len(buf));
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        bsr.Value.remain = buf;
        go.encoding.binary_package.Read(new binary_internal_test_package.byteSliceReaderжReader(bsr), BigEndian, Δslice);
    }
}

public static void BenchmarkReadInts(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    ref var ls = ref heap(new Struct(), out var Ꮡls);
    var bsr = Ꮡ(new byteSliceReader(nil));
    io.Reader r = new binary_internal_test_package.byteSliceReaderжReader(bsr);
    b.SetBytes(2 * (1 + 2 + 4 + 8));
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        bsr.Value.remain = big;
        go.encoding.binary_package.Read(r, BigEndian, Ꮡls.of(Struct.ᏑInt8));
        go.encoding.binary_package.Read(r, BigEndian, Ꮡls.of(Struct.ᏑInt16));
        go.encoding.binary_package.Read(r, BigEndian, Ꮡls.of(Struct.ᏑInt32));
        go.encoding.binary_package.Read(r, BigEndian, Ꮡls.of(Struct.ᏑInt64));
        go.encoding.binary_package.Read(r, BigEndian, Ꮡls.of(Struct.ᏑUint8));
        go.encoding.binary_package.Read(r, BigEndian, Ꮡls.of(Struct.ᏑUint16));
        go.encoding.binary_package.Read(r, BigEndian, Ꮡls.of(Struct.ᏑUint32));
        go.encoding.binary_package.Read(r, BigEndian, Ꮡls.of(Struct.ᏑUint64));
    }
    b.StopTimer();
    var want = s.ΔClone();
    want.Float32 = 0F;
    want.Float64 = 0D;
    want.Complex64 = 0F;
    want.Complex128 = 0D;
    want.Array = new uint8[]{0, 0, 0, 0}.array();
    want.Bool = false;
    want.BoolArray = new bool[]{false, false, false, false}.array();
    if (b.N > 0 && !reflect.DeepEqual(ls, want)) {
        Ꮡb.Fatalf("struct doesn't match:\ngot  %v;\nwant %v"u8, ls, want);
    }
}

public static void BenchmarkWriteInts(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var buf = @new<bytes.Buffer>();
    io.Writer w = new binary_test_package.bytes_BufferжWriter(buf);
    b.SetBytes(2 * (1 + 2 + 4 + 8));
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        buf.Reset();
        Write(w, BigEndian, s.Int8);
        Write(w, BigEndian, s.Int16);
        Write(w, BigEndian, s.Int32);
        Write(w, BigEndian, s.Int64);
        Write(w, BigEndian, s.Uint8);
        Write(w, BigEndian, s.Uint16);
        Write(w, BigEndian, s.Uint32);
        Write(w, BigEndian, s.Uint64);
    }
    b.StopTimer();
    if (b.N > 0 && !bytes.Equal(buf.Bytes(), big[..30])) {
        Ꮡb.Fatalf("first half doesn't match: %x %x"u8, buf.Bytes(), big[..30]);
    }
}

public static void BenchmarkAppendInts(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var buf = new slice<byte>(0, 256);
    b.SetBytes(2 * (1 + 2 + 4 + 8));
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        buf = buf[..0];
        (buf, _) = Append(buf, BigEndian, s.Int8);
        (buf, _) = Append(buf, BigEndian, s.Int16);
        (buf, _) = Append(buf, BigEndian, s.Int32);
        (buf, _) = Append(buf, BigEndian, s.Int64);
        (buf, _) = Append(buf, BigEndian, s.Uint8);
        (buf, _) = Append(buf, BigEndian, s.Uint16);
        (buf, _) = Append(buf, BigEndian, s.Uint32);
        (buf, _) = Append(buf, BigEndian, s.Uint64);
    }
    b.StopTimer();
    if (b.N > 0 && !bytes.Equal(buf, big[..30])) {
        Ꮡb.Fatalf("first half doesn't match: %x %x"u8, buf, big[..30]);
    }
}

public static void BenchmarkWriteSlice1000Int32s(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var Δslice = new slice<int32>(1000);
    var buf = @new<bytes.Buffer>();
    io.Writer w = new binary_test_package.bytes_BufferжWriter(buf);
    b.SetBytes(4 * 1000);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        buf.Reset();
        Write(w, BigEndian, Δslice);
    }
    b.StopTimer();
}

public static void BenchmarkAppendSlice1000Int32s(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var Δslice = new slice<int32>(1000);
    var buf = new slice<byte>(0, Size(Δslice));
    b.SetBytes((int64)cap(buf));
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        Append(buf, BigEndian, Δslice);
    }
    b.StopTimer();
}

public static void BenchmarkPutUint16(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.SetBytes(2);
    for (nint i = 0; i < b.N; i++) {
        BigEndian.PutUint16(putbuf[..2], (uint16)i);
    }
}

public static void BenchmarkAppendUint16(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.SetBytes(2);
    for (nint i = 0; i < b.N; i++) {
        putbuf = BigEndian.AppendUint16(putbuf[..0], (uint16)i);
    }
}

public static void BenchmarkPutUint32(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.SetBytes(4);
    for (nint i = 0; i < b.N; i++) {
        BigEndian.PutUint32(putbuf[..4], (uint32)i);
    }
}

public static void BenchmarkAppendUint32(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.SetBytes(4);
    for (nint i = 0; i < b.N; i++) {
        putbuf = BigEndian.AppendUint32(putbuf[..0], (uint32)i);
    }
}

public static void BenchmarkPutUint64(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.SetBytes(8);
    for (nint i = 0; i < b.N; i++) {
        BigEndian.PutUint64(putbuf[..8], (uint64)i);
    }
}

public static void BenchmarkAppendUint64(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.SetBytes(8);
    for (nint i = 0; i < b.N; i++) {
        putbuf = BigEndian.AppendUint64(putbuf[..0], (uint64)i);
    }
}

public static void BenchmarkLittleEndianPutUint16(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.SetBytes(2);
    for (nint i = 0; i < b.N; i++) {
        LittleEndian.PutUint16(putbuf[..2], (uint16)i);
    }
}

public static void BenchmarkLittleEndianAppendUint16(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.SetBytes(2);
    for (nint i = 0; i < b.N; i++) {
        putbuf = LittleEndian.AppendUint16(putbuf[..0], (uint16)i);
    }
}

public static void BenchmarkLittleEndianPutUint32(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.SetBytes(4);
    for (nint i = 0; i < b.N; i++) {
        LittleEndian.PutUint32(putbuf[..4], (uint32)i);
    }
}

public static void BenchmarkLittleEndianAppendUint32(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.SetBytes(4);
    for (nint i = 0; i < b.N; i++) {
        putbuf = LittleEndian.AppendUint32(putbuf[..0], (uint32)i);
    }
}

public static void BenchmarkLittleEndianPutUint64(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.SetBytes(8);
    for (nint i = 0; i < b.N; i++) {
        LittleEndian.PutUint64(putbuf[..8], (uint64)i);
    }
}

public static void BenchmarkLittleEndianAppendUint64(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.SetBytes(8);
    for (nint i = 0; i < b.N; i++) {
        putbuf = LittleEndian.AppendUint64(putbuf[..0], (uint64)i);
    }
}

public static void BenchmarkReadFloats(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    ref var ls = ref heap(new Struct(), out var Ꮡls);
    var bsr = Ꮡ(new byteSliceReader(nil));
    io.Reader r = new binary_internal_test_package.byteSliceReaderжReader(bsr);
    b.SetBytes(4 + 8);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        bsr.Value.remain = big[30..];
        go.encoding.binary_package.Read(r, BigEndian, Ꮡls.of(Struct.ᏑFloat32));
        go.encoding.binary_package.Read(r, BigEndian, Ꮡls.of(Struct.ᏑFloat64));
    }
    b.StopTimer();
    var want = s.ΔClone();
    want.Int8 = 0;
    want.Int16 = 0;
    want.Int32 = 0;
    want.Int64 = 0;
    want.Uint8 = 0;
    want.Uint16 = 0;
    want.Uint32 = 0;
    want.Uint64 = 0;
    want.Complex64 = 0F;
    want.Complex128 = 0D;
    want.Array = new uint8[]{0, 0, 0, 0}.array();
    want.Bool = false;
    want.BoolArray = new bool[]{false, false, false, false}.array();
    if (b.N > 0 && !reflect.DeepEqual(ls, want)) {
        Ꮡb.Fatalf("struct doesn't match:\ngot  %v;\nwant %v"u8, ls, want);
    }
}

public static void BenchmarkWriteFloats(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var buf = @new<bytes.Buffer>();
    io.Writer w = new binary_test_package.bytes_BufferжWriter(buf);
    b.SetBytes(4 + 8);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        buf.Reset();
        Write(w, BigEndian, s.Float32);
        Write(w, BigEndian, s.Float64);
    }
    b.StopTimer();
    if (b.N > 0 && !bytes.Equal(buf.Bytes(), big[30..(int)(30 + 4 + 8)])) {
        Ꮡb.Fatalf("first half doesn't match: %x %x"u8, buf.Bytes(), big[30..(int)(30 + 4 + 8)]);
    }
}

public static void BenchmarkReadSlice1000Float32s(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var bsr = Ꮡ(new byteSliceReader(nil));
    var Δslice = new slice<float32>(1000);
    var buf = new slice<byte>(len(Δslice) * 4);
    b.SetBytes((int64)len(buf));
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        bsr.Value.remain = buf;
        go.encoding.binary_package.Read(new binary_internal_test_package.byteSliceReaderжReader(bsr), BigEndian, Δslice);
    }
}

public static void BenchmarkWriteSlice1000Float32s(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var Δslice = new slice<float32>(1000);
    var buf = @new<bytes.Buffer>();
    io.Writer w = new binary_test_package.bytes_BufferжWriter(buf);
    b.SetBytes(4 * 1000);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        buf.Reset();
        Write(w, BigEndian, Δslice);
    }
    b.StopTimer();
}

public static void BenchmarkReadSlice1000Uint8s(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var bsr = Ꮡ(new byteSliceReader(nil));
    var Δslice = new slice<uint8>(1000);
    var buf = new slice<byte>(len(Δslice));
    b.SetBytes((int64)len(buf));
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        bsr.Value.remain = buf;
        go.encoding.binary_package.Read(new binary_internal_test_package.byteSliceReaderжReader(bsr), BigEndian, Δslice);
    }
}

public static void BenchmarkWriteSlice1000Uint8s(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var Δslice = new slice<uint8>(1000);
    var buf = @new<bytes.Buffer>();
    io.Writer w = new binary_test_package.bytes_BufferжWriter(buf);
    b.SetBytes(1000);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        buf.Reset();
        Write(w, BigEndian, Δslice);
    }
}

public static void BenchmarkSize(ж<testing.B> Ꮡb) {
    foreach (var (_, data) in sizableTypes) {
        var dataʗ1 = data;
        Ꮡb.Run(fmt.Sprintf("%T"u8, data), (ж<testing.B> bΔ1) => {
            foreach (var _ᴛ1 in range((~bΔ1).N)) {
                _ = Size(dataʗ1);
            }
        });
    }
}

public static void TestNativeEndian(ж<testing.T> Ꮡt) {
    UntypedInt val = 0x12345678;
    ref var i = ref heap<uint32>(out var Ꮡi);
    i = (uint32)val;
    var s = @unsafe.Slice(Ꮡi.Reinterpret<uint32, byte>(), /* unsafe.Sizeof(i) */ (uintptr)4);
    {
        var v = NativeEndian.Uint32(s); if (v != val) {
            Ꮡt.Errorf("NativeEndian.Uint32 returned %#x, expected %#x"u8, v, (nint)(val));
        }
    }
}

} // end binary_internal_test_package
