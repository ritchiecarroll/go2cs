// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.encoding;

using bytes = bytes_package;
using cmp = cmp_package;
using hex = go.encoding.hex_package;
using fmt = fmt_package;
using io = io_package;
using maps = maps_package;
using math = math_package;
using reflect = reflect_package;
using slices = slices_package;
using strings = strings_package;
using testing = testing_package;
using go.encoding;
using static go.encoding.gob_package;

partial class gob_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object encoderFailˢ = (@string)"encoder fail:"u8;

// Test basic operations in a safe manner.
public static void TestBasicEncoderDecoder(ж<testing.T> t) {
    slice<any> values = slice<any>([
        true,
        (nint)123,
        (int8)123,
        (int16)(-12345),
        (int32)123456,
        (int64)(-1234567),
        (nuint)123,
        (uint8)123,
        (uint16)12345,
        (uint32)123456,
        (uint64)1234567,
        (uintptr)12345678,
        (float32)1.2345F,
        (float64)1.2345678D,
        (complex64)(1.2345F + 2.3456F.i()),
        (complex128)(1.2345678D + 2.3456789D.i()),
        slice<byte>("hello"u8),
        (@string)"hello"u8
    ]);
    foreach (var (_, value) in values) {
        var b = @new<bytes.Buffer>();
        var enc = NewEncoder(new gob_test_package.bytes_BufferжWriter(b));
        var err = enc.Encode(value);
        if (err != nil) {
            t.Error(encoderFailˢ, err);
        }
        var dec = NewDecoder(new gob_test_package.bytes_BufferжReader(b));
        var result = reflect.New(reflect.TypeOf(value));
        err = dec.Decode(result.Interface());
        if (err != nil) {
            t.Fatalf("error decoding %T: %v:", reflect.TypeOf(value), err);
        }
        if (!reflect.DeepEqual(value, result.Elem().Interface())) {
            t.Fatalf("%T: expected %v got %v", value, value, result.Elem().Interface());
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string int16ˢ => ref int16ˢᵇ;
private static readonly @string int16ˢᵇ = "int16"u8;
internal static ref readonly @string int32ˢ => ref int32ˢᵇ;
private static readonly @string int32ˢᵇ = "int32"u8;
internal static ref readonly @string int64ˢ => ref int64ˢᵇ;
private static readonly @string int64ˢᵇ = "int64"u8;

public static void TestEncodeIntSlice(ж<testing.T> t) {
    var s8 = slice<int8>([1, 5, 12, 22, 35, 51, 70, 92, 117]);
    var s16 = slice<int16>([145, 176, 210, 247, 287, 330, 376, 425, 477]);
    var s32 = slice<int32>([532, 590, 651, 715, 782, 852, 925, 1001, 1080]);
    var s64 = slice<int64>([1162, 1247, 1335, 1426, 1520, 1617, 1717, 1820, 1926]);

    var s8ʗ1 = s8;
    t.Run(int8ˢ, (ж<testing.T> tΔ1) => {
        ref var sink = ref heap(new bytes.Buffer(), out var Ꮡsink);
        var enc = NewEncoder(new gob_test_package.bytes_BufferжWriter(Ꮡsink));
        enc.Encode(s8ʗ1);

        var dec = NewDecoder(new gob_test_package.bytes_BufferжReader(Ꮡsink));
        ref var res = ref heap<slice<int8>>(new slice<int8>(9), out var Ꮡres);
        dec.Decode(Ꮡres);

        if (!slices.Equal<slice<int8>, int8>(s8ʗ1, res)) {
            tΔ1.Fatalf("EncodeIntSlice: expected %v, got %v", s8ʗ1, res);
        }
    });

    var s16ʗ1 = s16;
    t.Run(int16ˢ, (ж<testing.T> tΔ2) => {
        ref var sink = ref heap(new bytes.Buffer(), out var Ꮡsink);
        var enc = NewEncoder(new gob_test_package.bytes_BufferжWriter(Ꮡsink));
        enc.Encode(s16ʗ1);

        var dec = NewDecoder(new gob_test_package.bytes_BufferжReader(Ꮡsink));
        ref var res = ref heap<slice<int16>>(new slice<int16>(9), out var Ꮡres);
        dec.Decode(Ꮡres);

        if (!slices.Equal<slice<int16>, int16>(s16ʗ1, res)) {
            tΔ2.Fatalf("EncodeIntSlice: expected %v, got %v", s16ʗ1, res);
        }
    });

    var s32ʗ1 = s32;
    t.Run(int32ˢ, (ж<testing.T> tΔ3) => {
        ref var sink = ref heap(new bytes.Buffer(), out var Ꮡsink);
        var enc = NewEncoder(new gob_test_package.bytes_BufferжWriter(Ꮡsink));
        enc.Encode(s32ʗ1);

        var dec = NewDecoder(new gob_test_package.bytes_BufferжReader(Ꮡsink));
        ref var res = ref heap<slice<int32>>(new slice<int32>(9), out var Ꮡres);
        dec.Decode(Ꮡres);

        if (!slices.Equal<slice<int32>, int32>(s32ʗ1, res)) {
            tΔ3.Fatalf("EncodeIntSlice: expected %v, got %v", s32ʗ1, res);
        }
    });

    var s64ʗ1 = s64;
    t.Run(int64ˢ, (ж<testing.T> tΔ4) => {
        ref var sink = ref heap(new bytes.Buffer(), out var Ꮡsink);
        var enc = NewEncoder(new gob_test_package.bytes_BufferжWriter(Ꮡsink));
        enc.Encode(s64ʗ1);

        var dec = NewDecoder(new gob_test_package.bytes_BufferжReader(Ꮡsink));
        ref var res = ref heap<slice<int64>>(new slice<int64>(9), out var Ꮡres);
        dec.Decode(Ꮡres);

        if (!slices.Equal<slice<int64>, int64>(s64ʗ1, res)) {
            tΔ4.Fatalf("EncodeIntSlice: expected %v, got %v", s64ʗ1, res);
        }
    });
}

public partial struct ET0 {
    public nint A;
    public @string B;
}

public partial struct ET2 {
    public @string X;
}

public partial struct ET1 {
    public nint A;
    public ж<ET2> Et2;
    public ж<ET1> Next;
}

// Like ET1 but with a different name for a field
public partial struct ET3 {
    public nint A;
    public ж<ET2> Et2;
    public ж<ET1> DifferentNext;
}

// Like ET1 but with a different type for a field
public partial struct ET4 {
    public nint A;
    public float64 Et2;
    public nint Next;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string gobsOfFunˢ => ref gobsOfFunˢᵇ;
private static readonly @string gobsOfFunˢᵇ = "gobs of fun"u8;
internal static readonly object errorDecodingEt0ˢ = (@string)"error decoding ET0:"u8;
internal static readonly object notAtEofˢ = (@string)"not at eof;"u8;
internal static readonly object bytesLeftˢ = (@string)"bytes left"u8;
internal static readonly object errorDecodingEt1ˢ = (@string)"error decoding ET1:"u8;
internal static readonly object round2ErrorDecodingEt1ˢ = (@string)"round 2: error decoding ET1:"u8;
internal static readonly object round2NotAtEofˢ = (@string)"round 2: not at eof;"u8;
internal static readonly object round3EncoderFailˢ = (@string)"round 3: encoder fail:"u8;
internal static readonly object round3ExpectedBadTypeˢ = (@string)"round 3: expected `bad type' error decoding ET2"u8;

public static void TestEncoderDecoder(ж<testing.T> t) {
    var b = @new<bytes.Buffer>();
    var enc = NewEncoder(new gob_test_package.bytes_BufferжWriter(b));
    var et0 = @new<ET0>();
    et0.A = 7;
    et0.B = gobsOfFunˢ;
    var err = enc.Encode(et0.OrTypedNil());
    if (err != nil) {
        t.Error(encoderFailˢ, err);
    }
    //fmt.Printf("% x %q\n", b, b)
    //Debug(b)
    var dec = NewDecoder(new gob_test_package.bytes_BufferжReader(b));
    var newEt0 = @new<ET0>();
    err = dec.Decode(newEt0.OrTypedNil());
    if (err != nil) {
        t.Fatal(errorDecodingEt0ˢ, err);
    }

    if (!reflect.DeepEqual(et0.OrTypedNil(), newEt0.OrTypedNil())) {
        t.Fatalf("invalid data for et0: expected %+v; got %+v", et0.Value, newEt0.Value);
    }
    if (b.Len() != 0) {
        t.Error(notAtEofˢ, b.Len(), bytesLeftˢ);
    }
    //	t.FailNow()

    b = @new<bytes.Buffer>();
    enc = NewEncoder(new gob_test_package.bytes_BufferжWriter(b));
    var et1 = @new<ET1>();
    et1.A = 7;
    et1.Et2 = @new<ET2>();
    err = enc.Encode(et1.OrTypedNil());
    if (err != nil) {
        t.Error(encoderFailˢ, err);
    }
    dec = NewDecoder(new gob_test_package.bytes_BufferжReader(b));
    var newEt1 = @new<ET1>();
    err = dec.Decode(newEt1.OrTypedNil());
    if (err != nil) {
        t.Fatal(errorDecodingEt1ˢ, err);
    }

    if (!reflect.DeepEqual(et1.OrTypedNil(), newEt1.OrTypedNil())) {
        t.Fatalf("invalid data for et1: expected %+v; got %+v", et1.Value, newEt1.Value);
    }
    if (b.Len() != 0) {
        t.Error(notAtEofˢ, b.Len(), bytesLeftˢ);
    }

    enc.Encode(et1.OrTypedNil());
    newEt1 = @new<ET1>();
    err = dec.Decode(newEt1.OrTypedNil());
    if (err != nil) {
        t.Fatal(round2ErrorDecodingEt1ˢ, err);
    }
    if (!reflect.DeepEqual(et1.OrTypedNil(), newEt1.OrTypedNil())) {
        t.Fatalf("round 2: invalid data for et1: expected %+v; got %+v", et1.Value, newEt1.Value);
    }
    if (b.Len() != 0) {
        t.Error(round2NotAtEofˢ, b.Len(), bytesLeftˢ);
    }

    // Now test with a running encoder/decoder pair that we recognize a type mismatch.
    err = enc.Encode(et1.OrTypedNil());
    if (err != nil) {
        t.Error(round3EncoderFailˢ, err);
    }
    var newEt2 = @new<ET2>();
    err = dec.Decode(newEt2.OrTypedNil());
    if (err == nil) {
        t.Fatal(round3ExpectedBadTypeˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedErrorForˢ = (@string)"expected error for"u8;
internal static readonly object unexpectedErrorForˢ = (@string)"unexpected error for"u8;

// Run one value through the encoder/decoder, but use the wrong type.
// Input is always an ET1; we compare it to whatever is under 'e'.
internal static void badTypeCheck(any e, bool shouldFail, @string msg, ж<testing.T> t) {
    var b = @new<bytes.Buffer>();
    var enc = NewEncoder(new gob_test_package.bytes_BufferжWriter(b));
    var et1 = @new<ET1>();
    et1.A = 7;
    et1.Et2 = @new<ET2>();
    var err = enc.Encode(et1.OrTypedNil());
    if (err != nil) {
        t.Error(encoderFailˢ, err);
    }
    var dec = NewDecoder(new gob_test_package.bytes_BufferжReader(b));
    err = dec.Decode(e);
    if (shouldFail && err == nil) {
        t.Error(expectedErrorForˢ, msg);
    }
    if (!shouldFail && err != nil) {
        t.Error(unexpectedErrorForˢ, msg, err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string noFieldsInCommonˢ => ref noFieldsInCommonˢᵇ;
private static readonly @string noFieldsInCommonˢᵇ = "no fields in common"u8;
internal static ref readonly @string differentNameOfFieldˢ => ref differentNameOfFieldˢᵇ;
private static readonly @string differentNameOfFieldˢᵇ = "different name of field"u8;
internal static ref readonly @string differentTypeOfFieldˢ => ref differentTypeOfFieldˢᵇ;
private static readonly @string differentTypeOfFieldˢᵇ = "different type of field"u8;

// Test that we recognize a bad type the first time.
public static void TestWrongTypeDecoder(ж<testing.T> t) {
    badTypeCheck(@new<ET2>(), true, noFieldsInCommonˢ, t);
    badTypeCheck(@new<ET3>(), false, differentNameOfFieldˢ, t);
    badTypeCheck(@new<ET4>(), true, differentTypeOfFieldˢ, t);
}

// Types not supported at top level by the Encoder.
internal static slice<any> unsupportedValues = slice<any>([
    new channel<nint>(0),
    bool (nint a) => true
]);

public static void TestUnsupported(ж<testing.T> t) {
    ref var b = ref heap(new bytes.Buffer(), out var Ꮡb);
    var enc = NewEncoder(new gob_test_package.bytes_BufferжWriter(Ꮡb));
    foreach (var (_, v) in unsupportedValues) {
        var err = enc.Encode(v);
        if (err == nil) {
            t.Errorf("expected error for %T; got none", v);
        }
    }
}

internal static error encAndDec(any @in, any @out) {
    var b = @new<bytes.Buffer>();
    var enc = NewEncoder(new gob_test_package.bytes_BufferжWriter(b));
    var err = enc.Encode(@in);
    if (err != nil) {
        return err;
    }
    var dec = NewDecoder(new gob_test_package.bytes_BufferжReader(b));
    err = dec.Decode(@out);
    if (err != nil) {
        return err;
    }
    return default;
}

// Encode a T, decode a *T
internal partial struct TestTypeToPtrType_Type0 /*dyn*/ {
    public nint A;
}

public static void TestTypeToPtrType(ж<testing.T> t) {
    var t0 = new TestTypeToPtrType_Type0(7);
    var t0p = @new<TestTypeToPtrType_Type0>();
    {
        var err = encAndDec(t0, t0p.OrTypedNil()); if (err != nil) {
            t.Error(err);
        }
    }
}

// Encode a *T, decode a T
internal partial struct TestPtrTypeToType_Type1 /*dyn*/ {
    public nuint A;
}

public static void TestPtrTypeToType(ж<testing.T> t) {
    var t1p = Ꮡ(new TestPtrTypeToType_Type1(17));
    TestPtrTypeToType_Type1 t1 = default;
    {
        var err = encAndDec(t1, t1p.OrTypedNil()); if (err != nil) {
            t.Error(err);
        }
    }
}

internal partial struct TestTypeToPtrPtrPtrPtrType_Type2 /*dyn*/ {
    public ж<ж<ж<ж<float64>>>> A;
}

public static void TestTypeToPtrPtrPtrPtrType(ж<testing.T> t) {
    var t2 = new TestTypeToPtrPtrPtrPtrType_Type2();
    t2.A = @new<ж<ж<ж<float64>>>>();
    t2.A.ValueSlot = @new<ж<ж<float64>>>();
    (t2.A.ValueSlot).ValueSlot = @new<ж<float64>>();
    ((t2.A.ValueSlot).ValueSlot).ValueSlot = @new<float64>();
    (((t2.A.ValueSlot).ValueSlot).ValueSlot).Value = 27.4D;
    var t2pppp = @new<ж<ж<ж<TestTypeToPtrPtrPtrPtrType_Type2>>>>();
    {
        var err = encAndDec(t2, t2pppp.OrTypedNil()); if (err != nil) {
            t.Fatal(err);
        }
    }
    if ((((((((t2pppp.ValueSlot).ValueSlot).ValueSlot).Value).A.ValueSlot).ValueSlot).ValueSlot).Value != (((t2.A.ValueSlot).ValueSlot).ValueSlot).Value) {
        t.Errorf("wrong value after decode: %g not %g", (((((((t2pppp.ValueSlot).ValueSlot).ValueSlot).Value).A.ValueSlot).ValueSlot).ValueSlot).Value, (((t2.A.ValueSlot).ValueSlot).ValueSlot).Value);
    }
}

internal partial struct TestSlice_Type3 /*dyn*/ {
    public slice<@string> A;
}

public static void TestSlice(ж<testing.T> t) {
    var t3p = Ꮡ(new TestSlice_Type3(slice<@string>(["hello", "world"])));
    TestSlice_Type3 t3 = default;
    {
        var err = encAndDec(t3, t3p.OrTypedNil()); if (err != nil) {
            t.Error(err);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string pointerˢ => ref pointerˢᵇ;
private static readonly @string pointerˢᵇ = "pointer"u8;
internal static readonly object expectedErrorAboutˢ = (@string)"expected error about pointer; got"u8;

// Encode a *T, decode a T
internal partial struct TestValueError_Type4 /*dyn*/ {
    public nint A;
}

public static void TestValueError(ж<testing.T> t) {
    var t4p = Ꮡ(new TestValueError_Type4(3));
    TestValueError_Type4 t4 = default; // note: not a pointer.
    {
        var err = encAndDec(t4p.OrTypedNil(), t4); if (err == nil || !strings.Contains(err.Error(), pointerˢ)) {
            t.Error(expectedErrorAboutˢ, err);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object shouldFailWithMismatchedˢ = (@string)"should fail with mismatched array sizes"u8;

internal partial struct TestArray_Type5 /*dyn*/ {
    public array<@string> A = new(3);
    public array<byte> B = new(3);
}

internal partial struct TestArray_Type6 /*dyn*/ {
    public array<@string> A = new(2); // can't hold t5.a
}

public static void TestArray(ж<testing.T> t) {
    var t5 = new TestArray_Type5(new @string[]{"hello", ",", "world"}.array(), new byte[]{1, 2, 3}.array());
    ref var t5p = ref heap(new TestArray_Type5(), out var Ꮡt5p);
    {
        var err = encAndDec(t5, Ꮡt5p); if (err != nil) {
            t.Error(err);
        }
    }
    ref var t6 = ref heap(new TestArray_Type6(), out var Ꮡt6);
    {
        var err = encAndDec(t5, Ꮡt6); if (err == nil) {
            t.Error(shouldFailWithMismatchedˢ);
        }
    }
}

internal partial struct TestRecursiveMapType_recursiveMap /*map[@string, TestRecursiveMapType_recursiveMap]*/;

public static void TestRecursiveMapType(ж<testing.T> t) {
    var r1 = new TestRecursiveMapType_recursiveMap(new map<@string, TestRecursiveMapType_recursiveMap>{["A"] = new TestRecursiveMapType_recursiveMap(new map<@string, TestRecursiveMapType_recursiveMap>{["B"] = default, ["C"] = default}), ["D"] = default});
    ref var r2 = ref heap<TestRecursiveMapType_recursiveMap>(new TestRecursiveMapType_recursiveMap(0), out var Ꮡr2);
    {
        var err = encAndDec(r1, Ꮡr2); if (err != nil) {
            t.Error(err);
        }
    }
}

internal partial struct TestRecursiveSliceType_recursiveSlice /*[]TestRecursiveSliceType_recursiveSlice*/;

public static void TestRecursiveSliceType(ж<testing.T> t) {
    var r1 = new TestRecursiveSliceType_recursiveSlice(new array<TestRecursiveSliceType_recursiveSlice>(2){[0] = new TestRecursiveSliceType_recursiveSlice(new array<TestRecursiveSliceType_recursiveSlice>(1){[0] = default}), [1] = default});
    ref var r2 = ref heap<TestRecursiveSliceType_recursiveSlice>(new TestRecursiveSliceType_recursiveSlice(0), out var Ꮡr2);
    {
        var err = encAndDec(r1, Ꮡr2); if (err != nil) {
            t.Error(err);
        }
    }
}

internal partial struct TestDefaultsInArray_Type7 /*dyn*/ {
    public slice<bool> B;
    public slice<nint> I;
    public slice<@string> S;
    public slice<float64> F;
}

// Regression test for bug: must send zero values inside arrays
public static void TestDefaultsInArray(ж<testing.T> t) {
    var t7 = new TestDefaultsInArray_Type7(
        slice<bool>([false, false, true]),
        slice<nint>([0, 0, 1]),
        slice<@string>(["hi", "", "there"]),
        slice<float64>([0D, 0D, 1D])
    );
    ref var t7p = ref heap(new TestDefaultsInArray_Type7(), out var Ꮡt7p);
    {
        var err = encAndDec(t7, Ꮡt7p); if (err != nil) {
            t.Error(err);
        }
    }
}

internal static readonly ж<nint> ᏑtestInt = new StaticBox<nint>(default(nint));
internal static ref nint testInt => ref ((StaticBox<nint>)ᏑtestInt).ValueSlot;

internal static readonly ж<float32> ᏑtestFloat32 = new StaticBox<float32>(default(float32));
internal static ref float32 testFloat32 => ref ((StaticBox<float32>)ᏑtestFloat32).ValueSlot;

internal static readonly ж<@string> ᏑtestString = new StaticBox<@string>(default(@string));
internal static ref @string testString => ref ((StaticBox<@string>)ᏑtestString).ValueSlot;

internal static readonly ж<slice<@string>> ᏑtestSlice = new StaticBox<slice<@string>>(default(slice<@string>));
internal static ref slice<@string> testSlice => ref ((StaticBox<slice<@string>>)ᏑtestSlice).ValueSlot;

internal static readonly ж<map<@string, nint>> ᏑtestMap = new StaticBox<map<@string, nint>>(default(map<@string, nint>));
internal static ref map<@string, nint> testMap => ref ((StaticBox<map<@string, nint>>)ᏑtestMap).ValueSlot;

internal static readonly ж<array<nint>> ᏑtestArray = new StaticBox<array<nint>>(new array<nint>(7));
internal static ref array<nint> testArray => ref ((StaticBox<array<nint>>)ᏑtestArray).ValueSlot;

public partial struct SingleTest {
    internal any @in;
    internal any @out;
    internal @string err;
}

internal static slice<SingleTest> singleTests = slice<SingleTest>([
    new((nint)17, ᏑtestInt, ""),
    new((float32)17.5F, ᏑtestFloat32, ""),
    new((@string)"bike shed", ᏑtestString, ""),
    new(slice<@string>(["bike", "shed", "paint", "color"]), ᏑtestSlice, ""),
    new(new map<@string, nint>{["seven"] = 7, ["twelve"] = 12}, ᏑtestMap, ""),
    new(new nint[]{4, 55, 0, 0, 0, 0, 0}.array(), ᏑtestArray, ""), // case that once triggered a bug
    new(new nint[]{4, 55, 1, 44, 22, 66, 1234}.array(), ᏑtestArray, ""),

    // Decode errors
    new((nint)172, ᏑtestFloat32, "type")
]);

public static void TestSingletons(ж<testing.T> t) {
    var b = @new<bytes.Buffer>();
    var enc = NewEncoder(new gob_test_package.bytes_BufferжWriter(b));
    var dec = NewDecoder(new gob_test_package.bytes_BufferжReader(b));
    foreach (var (_, test) in singleTests) {
        b.Reset();
        var err = enc.Encode(test.@in);
        if (err != nil) {
            t.Errorf("error encoding %v: %s", test.@in, err);
            continue;
        }
        err = dec.Decode(test.@out);
        switch (ᐧ) {
        case {} when err != nil && test.err == ""u8: {
            t.Errorf("error decoding %v: %s", test.@in, err);
            continue;
            break;
        }
        case {} when err == nil && test.err != ""u8: {
            t.Errorf("expected error decoding %v: %s", test.@in, test.err);
            continue;
            break;
        }
        case {} when err != nil && test.err != ""u8: {
            if (!strings.Contains(err.Error(), test.err)) {
                t.Errorf("wrong error decoding %v: wanted %s, got %v", test.@in, test.err, err);
            }
            continue;
            break;
        }}
        // Get rid of the pointer in the rhs
        var val = reflect.ValueOf(test.@out).Elem().Interface();
        if (!reflect.DeepEqual(test.@in, val)) {
            t.Errorf("decoding singleton: expected %v got %v", test.@in, val);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object shouldGetErrorForStructˢ = (@string)"should get error for struct/non-struct"u8;
internal static ref readonly @string typeˢ => ref typeˢᵇ;
private static readonly @string typeˢᵇ = "type"u8;
internal static readonly object forStructNonStructˢ = (@string)"for struct/non-struct expected type error; got"u8;
internal static readonly object shouldGetErrorForNonˢ = (@string)"should get error for non-struct/struct"u8;
internal static readonly object forNonStructStructˢ = (@string)"for non-struct/struct expected type error; got"u8;

internal partial struct TestStructNonStruct_Struct /*dyn*/ {
    public @string A;
}

internal partial struct TestStructNonStruct_NonStruct /*@string*/;

public static void TestStructNonStruct(ж<testing.T> t) {
    ref var s = ref heap<TestStructNonStruct_Struct>(new TestStructNonStruct_Struct("hello"), out var Ꮡs);
    ref var sp = ref heap(new TestStructNonStruct_Struct(), out var Ꮡsp);
    {
        var err = encAndDec(s, Ꮡsp); if (err != nil) {
            t.Error(err);
        }
    }
    ref var ns = ref heap(new TestStructNonStruct_NonStruct(), out var Ꮡns);
    {
        var err = encAndDec(s, Ꮡns); if (err == nil) {
            t.Error(shouldGetErrorForStructˢ);
        } else if (!strings.Contains(err.Error(), typeˢ)) {
            t.Error(forStructNonStructˢ, err);
        }
    }
    // Now try the other way
    ref var nsp = ref heap(new TestStructNonStruct_NonStruct(), out var Ꮡnsp);
    {
        var err = encAndDec(ns, Ꮡnsp); if (err != nil) {
            t.Error(err);
        }
    }
    {
        var err = encAndDec(ns, Ꮡs); if (err == nil) {
            t.Error(shouldGetErrorForNonˢ);
        } else if (!strings.Contains(err.Error(), typeˢ)) {
            t.Error(forNonStructStructˢ, err);
        }
    }
}

internal partial interface interfaceIndirectTestI {
    bool F();
}

internal partial struct interfaceIndirectTestT {
}

internal static bool F(this ref interfaceIndirectTestT @this) {
    return true;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object encodeErrorˢ = (@string)"encode error:"u8;

// A version of a bug reported on golang-nuts. Also tests top-level
// slice of interfaces. The issue was registering *T caused T to be
// stored as the concrete type.
public static void TestInterfaceIndirect(ж<testing.T> t) {
    Register(Ꮡ(new interfaceIndirectTestT()));
    var b = @new<bytes.Buffer>();
    var w = slice<interfaceIndirectTestI>([new gob_internal_test_package.interfaceIndirectTestTжinterfaceIndirectTestI(Ꮡ(new interfaceIndirectTestT()))]);
    var err = NewEncoder(new gob_test_package.bytes_BufferжWriter(b)).Encode(w);
    if (err != nil) {
        t.Fatal(encodeErrorˢ, err);
    }

    ref var r = ref heap<slice<interfaceIndirectTestI>>(out var Ꮡr);
    err = NewDecoder(new gob_test_package.bytes_BufferжReader(b)).Decode(Ꮡr);
    if (err != nil) {
        t.Fatal(decodeErrorˢ, err);
    }
}

// Now follow various tests that decode into things that can't represent the
// encoded value, all of which should be legal.
// Also, when the ignored object contains an interface value, it may define
// types. Make sure that skipping the value still defines the types by using
// the encoder/decoder pair to send a value afterwards. If an interface
// is sent, its type in the test is always NewType0, so this checks that the
// encoder and decoder don't skew with respect to type definitions.
public partial struct Struct0 {
    public any I;
}

public partial struct NewType0 {
    public @string S;
}

internal partial struct ignoreTest {
    internal any @in, @out;
}

    partial struct Δtype /*dyn*/ {
        public nint A;
    }
internal static slice<ignoreTest> ignoreTests = slice<ignoreTest>([
    // Decode normal struct into an empty struct
    new(Ꮡ(new Δtype(23)), Ꮡ(new EmptyStruct())),
    // Decode normal struct into a nil.
    new(Ꮡ(new Δtype(23)), default),
    // Decode singleton string into a nil.
    new((@string)"hello, world", default),
    // Decode singleton slice into a nil.
    new(slice<nint>([1, 2, 3, 4]), default),
    // Decode struct containing an interface into a nil.
    new(Ꮡ(new Struct0(Ꮡ(new NewType0("value0")))), default),
    // Decode singleton slice of interfaces into a nil.
    new(slice<any>([(@string)"hi", Ꮡ(new NewType0("value1")), (nint)23]), default)
]);

public static void TestDecodeIntoNothing(ж<testing.T> t) {
    Register(@new<NewType0>());
    foreach (var (i, test) in ignoreTests) {
        var b = @new<bytes.Buffer>();
        var enc = NewEncoder(new gob_test_package.bytes_BufferжWriter(b));
        var err = enc.Encode(test.@in);
        if (err != nil) {
            t.Errorf("%d: encode error %s:", i, err);
            continue;
        }
        var dec = NewDecoder(new gob_test_package.bytes_BufferжReader(b));
        err = dec.Decode(test.@out);
        if (err != nil) {
            t.Errorf("%d: decode error: %s", i, err);
            continue;
        }
        // Now see if the encoder and decoder are in a consistent state.
        ref var str = ref heap<@string>(fmt.Sprintf("Value %d"u8, i), out var Ꮡstr);
        err = enc.Encode(Ꮡ(new NewType0(str)));
        if (err != nil) {
            t.Fatalf("%d: NewType0 encode error: %s", i, err);
        }
        var ns = @new<NewType0>();
        err = dec.Decode(ns.OrTypedNil());
        if (err != nil) {
            t.Fatalf("%d: NewType0 decode error: %s", i, err);
        }
        if (ns.S != str) {
            t.Fatalf("%d: expected %q got %q", i, str, ns.S);
        }
    }
}

public static void TestIgnoreRecursiveType(ж<testing.T> t) {
    // It's hard to build a self-contained test for this because
    // we can't build compatible types in one package with
    // different items so something is ignored. Here is
    // some data that represents, according to debug.go:
    // type definition {
    //	slice "recursiveSlice" id=106
    //		elem id=106
    // }
    var data = slice<byte>([
        0x1d, 0xff, 0xd3, 0x02, 0x01, 0x01, 0x0e, 0x72,
        0x65, 0x63, 0x75, 0x72, 0x73, 0x69, 0x76, 0x65,
        0x53, 0x6c, 0x69, 0x63, 0x65, 0x01, 0xff, 0xd4,
        0x00, 0x01, 0xff, 0xd4, 0x00, 0x00, 0x07, 0xff,
        0xd4, 0x00, 0x02, 0x01, 0x00, 0x00
    ]);
    var dec = NewDecoder(new gob_test_package.bytes_ReaderжReader(bytes.NewReader(data)));
    // Issue 10415: This caused infinite recursion.
    var err = dec.Decode(default);
    if (err != nil) {
        t.Fatal(err);
    }
}

// Another bug from golang-nuts, involving nested interfaces.
public partial struct Bug0Outer {
    public any Bug0Field;
}

public partial struct Bug0Inner {
    public nint A;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object encodeˢ2 = (@string)"Encode:"u8;
internal static readonly object decodeˢ2 = (@string)"Decode:"u8;

public static void TestNestedInterfaces(ж<testing.T> t) {
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var e = NewEncoder(new gob_test_package.bytes_BufferжWriter(Ꮡbuf));
    var d = NewDecoder(new gob_test_package.bytes_BufferжReader(Ꮡbuf));
    Register(@new<Bug0Outer>());
    Register(@new<Bug0Inner>());
    var f = Ꮡ(new Bug0Outer(Ꮡ(new Bug0Outer(Ꮡ(new Bug0Inner(7))))));
    ref var v = ref heap<any>(out var Ꮡv);

    v = f.OrTypedNil();
    var err = e.Encode(Ꮡv);
    if (err != nil) {
        t.Fatal(encodeˢ2, err);
    }
    err = d.Decode(Ꮡv);
    if (err != nil) {
        t.Fatal(decodeˢ2, err);
    }
    // Make sure it decoded correctly.
    var (outer1, ok) = v._<ж<Bug0Outer>>(ᐧ);
    if (!ok) {
        t.Fatalf("v not Bug0Outer: %T", v);
    }
    (var outer2, ok) = outer1.Bug0Field._<ж<Bug0Outer>>(ᐧ);
    if (!ok) {
        t.Fatalf("v.Bug0Field not Bug0Outer: %T", outer1.Bug0Field);
    }
    (var inner, ok) = outer2.Bug0Field._<ж<Bug0Inner>>(ᐧ);
    if (!ok) {
        t.Fatalf("v.Bug0Field.Bug0Field not Bug0Inner: %T", outer2.Bug0Field);
    }
    if (inner.A != 7) {
        t.Fatalf("final value %d; expected %d", inner.A, (nint)7);
    }
}

// The bugs keep coming. We forgot to send map subtypes before the map.
public partial struct Bug1Elem {
    public @string Name;
    public nint Id;
}

public partial struct Bug1StructMap /*map[@string, Bug1Elem]*/;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string val1ˢ => ref val1ˢᵇ;
private static readonly @string val1ˢᵇ = "val1"u8;
internal static ref readonly @string val2ˢ => ref val2ˢᵇ;
private static readonly @string val2ˢᵇ = "val2"u8;

public static void TestMapBug1(ж<testing.T> t) {
    var @in = new Bug1StructMap(0);
    @in[val1ˢ] = new Bug1Elem("elem1", 1);
    @in[val2ˢ] = new Bug1Elem("elem2", 2);

    var b = @new<bytes.Buffer>();
    var enc = NewEncoder(new gob_test_package.bytes_BufferжWriter(b));
    var err = enc.Encode(@in);
    if (err != nil) {
        t.Fatal(encodeˢ, err);
    }
    var dec = NewDecoder(new gob_test_package.bytes_BufferжReader(b));
    ref var @out = ref heap<Bug1StructMap>(new Bug1StructMap(0), out var Ꮡout);
    err = dec.Decode(Ꮡout);
    if (err != nil) {
        t.Fatal(decodeˢ, err);
    }
    if (!maps.Equal<Bug1StructMap, Bug1StructMap, @string, Bug1Elem>(@in, @out)) {
        t.Errorf("mismatch: %v %v", @in, @out);
    }
}

public static void TestGobMapInterfaceEncode(ж<testing.T> t) {
    var m = new map<@string, any>{
        ["up"] = (uintptr)0,
        ["i0"] = slice<nint>([-1]),
        ["i1"] = slice<int8>([(int8)(-1)]),
        ["i2"] = slice<int16>([(int16)(-1)]),
        ["i3"] = slice<int32>([-1]),
        ["i4"] = slice<int64>([-1]),
        ["u0"] = slice<nuint>([1]),
        ["u1"] = slice<uint8>([1]),
        ["u2"] = slice<uint16>([1]),
        ["u3"] = slice<uint32>([1]),
        ["u4"] = slice<uint64>([1]),
        ["f0"] = slice<float32>([1F]),
        ["f1"] = slice<float64>([1D]),
        ["c0"] = slice<complex64>([complex(2F, -2F)]),
        ["c1"] = slice<complex128>([complex(2D, (float64)(-2D))]),
        ["us"] = slice<uintptr>([0]),
        ["bo"] = slice<bool>([false]),
        ["st"] = slice<@string>(["s"])
    };
    var enc = NewEncoder(new gob_test_package.bytes_BufferжWriter(@new<bytes.Buffer>()));
    var err = enc.Encode(m);
    if (err != nil) {
        t.Errorf("encode map: %s", err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object bytesDecodeˢ = (@string)"bytes: decode:"u8;
internal static readonly object intsDecodeˢ = (@string)"ints: decode:"u8;

public static void TestSliceReusesMemory(ж<testing.T> t) {
    var buf = @new<bytes.Buffer>();
    // Bytes
    {
        var x = slice<byte>("abcd"u8);
        var enc = NewEncoder(new gob_test_package.bytes_BufferжWriter(buf));
        var err = enc.Encode(x);
        if (err != nil) {
            t.Errorf("bytes: encode: %s", err);
        }
        // Decode into y, which is big enough.
        ref var y = ref heap<slice<byte>>(slice<byte>("ABCDE"u8), out var Ꮡy);
        var addr = Ꮡ(y, 0);
        var dec = NewDecoder(new gob_test_package.bytes_BufferжReader(buf));
        err = dec.Decode(Ꮡy);
        if (err != nil) {
            t.Fatal(bytesDecodeˢ, err);
        }
        if (!bytes.Equal(x, y)) {
            t.Errorf("bytes: expected %q got %q\n", x, y);
        }
        if (addr != Ꮡ(y, 0)) {
            t.Errorf("bytes: unnecessary reallocation");
        }
    }
    // general slice
    {
        var x = slice<rune>((@string)"abcd");
        var enc = NewEncoder(new gob_test_package.bytes_BufferжWriter(buf));
        var err = enc.Encode(x);
        if (err != nil) {
            t.Errorf("ints: encode: %s", err);
        }
        // Decode into y, which is big enough.
        ref var y = ref heap<slice<rune>>(slice<rune>((@string)"ABCDE"), out var Ꮡy);
        var addr = Ꮡ(y, 0);
        var dec = NewDecoder(new gob_test_package.bytes_BufferжReader(buf));
        err = dec.Decode(Ꮡy);
        if (err != nil) {
            t.Fatal(intsDecodeˢ, err);
        }
        if (!slices.Equal<slice<rune>, rune>(x, y)) {
            t.Errorf("ints: expected %q got %q\n", x, y);
        }
        if (addr != Ꮡ(y, 0)) {
            t.Errorf("ints: unnecessary reallocation");
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedErrorFromBadˢ = (@string)"expected error from bad count"u8;
internal static readonly object expectedBadCountErrorGotˢ = (@string)"expected bad count error; got"u8;

// Used to crash: negative count in recvMessage.
public static void TestBadCount(ж<testing.T> t) {
    var b = slice<byte>([0xfb, 0xa5, 0x82, 0x2f, 0xca, 0x1]);
    {
        var err = NewDecoder(new gob_test_package.bytes_ReaderжReader(bytes.NewReader(b))).Decode(default); if (err == nil) {
            t.Error(expectedErrorFromBadˢ);
        } else if (err.Error() != errBadCount.Error()) {
            t.Error(expectedBadCountErrorGotˢ, err);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object decoderFailˢ = (@string)"decoder fail:"u8;

// Verify that sequential Decoders built on a single input will
// succeed if the input implements ReadByte and there is no
// type information in the stream.
public static void TestSequentialDecoder(ж<testing.T> t) {
    var b = @new<bytes.Buffer>();
    var enc = NewEncoder(new gob_test_package.bytes_BufferжWriter(b));
    const nint count = 10;
    for (nint i = 0; i < count; i++) {
        @string s = fmt.Sprintf("%d"u8, i);
        {
            var err = enc.Encode(s); if (err != nil) {
                t.Error(encoderFailˢ, err);
            }
        }
    }
    for (nint i = 0; i < count; i++) {
        var dec = NewDecoder(new gob_test_package.bytes_BufferжReader(b));
        ref var s = ref heap(new @string(), out var Ꮡs);
        {
            var err = dec.Decode(Ꮡs); if (err != nil) {
                t.Fatal(decoderFailˢ, err);
            }
        }
        if (s != fmt.Sprintf("%d"u8, i)) {
            t.Fatalf("decode expected %d got %s", i, s);
        }
    }
}

// Should be able to have unrepresentable fields (chan, func, *chan etc.); we just ignore them.
public partial struct Bug2 {
    public nint A;
    public channel<nint> C;
    public ж<channel<nint>> CP;
    public Action F;
    public ж<ж<Action>> FPP;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object errorEncodingˢ = (@string)"error encoding:"u8;
internal static readonly object unexpectedValueForChanOrˢ = (@string)"unexpected value for chan or func"u8;

public static void TestChanFuncIgnored(ж<testing.T> t) {
    ref var c = ref heap<channel<nint>>(new channel<nint>(0), out var Ꮡc);
    ref var f = ref heap<Action>(() => {
    }, out var Ꮡf);
    ref var fp = ref heap<ж<Action>>(Ꮡf, out var Ꮡfp);
    var b0 = new Bug2(23, c, Ꮡc, f, Ꮡfp);
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var enc = NewEncoder(new gob_test_package.bytes_BufferжWriter(Ꮡbuf));
    {
        var errΔ1 = enc.Encode(b0); if (errΔ1 != nil) {
            t.Fatal(errorEncodingˢ, errΔ1);
        }
    }
    ref var b1 = ref heap(new Bug2(), out var Ꮡb1);
    var err = NewDecoder(new gob_test_package.bytes_BufferжReader(Ꮡbuf)).Decode(Ꮡb1);
    if (err != nil) {
        t.Fatal(decodeˢ, err);
    }
    if (b1.A != b0.A) {
        t.Fatalf("got %d want %d", b1.A, b0.A);
    }
    if (b1.C != nil || b1.CP != nil || b1.F != nil || b1.FPP != nil) {
        t.Fatal(unexpectedValueForChanOrˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedCompatibilityˢ = (@string)"expected compatibility error"u8;

public static void TestSliceIncompatibility(ж<testing.T> t) {
    slice<byte> @in = slice<byte>([1, 2, 3]);
    ref var @out = ref heap<slice<nint>>(out var Ꮡout);
    {
        var err = encAndDec(@in, Ꮡout); if (err == nil) {
            t.Error(expectedCompatibilityˢ);
        }
    }
}

// Mutually recursive slices of structs caused problems.
public partial struct Bug3 {
    public nint Num;
    public slice<ж<Bug3>> Children;
}

public static void TestGobPtrSlices(ж<testing.T> t) {
    ref var @in = ref heap<slice<ж<Bug3>>>(slice<ж<Bug3>>([
        Ꮡ(new Bug3(1, default)),
        Ꮡ(new Bug3(2, default))
    ]), out var Ꮡin);
    var b = @new<bytes.Buffer>();
    var err = NewEncoder(new gob_test_package.bytes_BufferжWriter(b)).Encode(Ꮡin);
    if (err != nil) {
        t.Fatal(encodeˢ, err);
    }

    ref var @out = ref heap<slice<ж<Bug3>>>(out var Ꮡout);
    err = NewDecoder(new gob_test_package.bytes_BufferжReader(b)).Decode(Ꮡout);
    if (err != nil) {
        t.Fatal(decodeˢ, err);
    }
    if (!reflect.DeepEqual(@in, @out)) {
        t.Fatalf("got %v; wanted %v", @out, @in);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string barˢ => ref barˢᵇ;
private static readonly @string barˢᵇ = "bar"u8;
internal static readonly object bazˢ = (@string)"baz"u8;
internal static ref readonly @string fooˢ => ref fooˢᵇ;
private static readonly @string fooˢᵇ = "foo"u8;

// getDecEnginePtr cached engine for ut.base instead of ut.user so we passed
// a *map and then tried to reuse its engine to decode the inner map.
public static void TestPtrToMapOfMap(ж<testing.T> t) {
    Register(new map<@string, any>());
    var subdata = new map<@string, any>();
    subdata[barˢ] = bazˢ;
    var data = new map<@string, any>();
    data[fooˢ] = subdata;

    var b = @new<bytes.Buffer>();
    var err = NewEncoder(new gob_test_package.bytes_BufferжWriter(b)).Encode(data);
    if (err != nil) {
        t.Fatal(encodeˢ, err);
    }
    ref var newData = ref heap<map<@string, any>>(out var ᏑnewData);
    err = NewDecoder(new gob_test_package.bytes_BufferжReader(b)).Decode(ᏑnewData);
    if (err != nil) {
        t.Fatal(decodeˢ, err);
    }
    if (!reflect.DeepEqual(data, newData)) {
        t.Fatalf("expected %v got %v", data, newData);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string nilValueˢ => ref nilValueˢᵇ;
private static readonly @string nilValueˢᵇ = "nil value"u8;

// Test that untyped nils generate an error, not a panic.
// See Issue 16204.
public static void TestCatchInvalidNilValue(ж<testing.T> t) {
    var (encodeErr, panicErr) = encodeAndRecover(default);
    if (panicErr != nil) {
        t.Fatalf("panicErr=%v, should not panic encoding untyped nil", panicErr);
    }
    if (encodeErr == nil) {
        t.Errorf("got err=nil, want non-nil error when encoding untyped nil value");
    } else if (!strings.Contains(encodeErr.Error(), nilValueˢ)) {
        t.Errorf("expected 'nil value' error; got err=%v", encodeErr);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object errorInEncodeˢ = (@string)"error in encode:"u8;
internal static readonly object topLevelNilPointerDidNotˢ = (@string)"top-level nil pointer did not panic"u8;
internal static ref readonly @string nilPointerˢ => ref nilPointerˢᵇ;
private static readonly @string nilPointerˢᵇ = "nil pointer"u8;
internal static readonly object expectedNilPointerErrorˢ = (@string)"expected nil pointer error, got:"u8;

// A top-level nil pointer generates a panic with a helpful string-valued message.
public static void TestTopLevelNilPointer(ж<testing.T> t) {
    ж<nint> ip = default;
    var (encodeErr, panicErr) = encodeAndRecover(ip.OrTypedNil());
    if (encodeErr != nil) {
        t.Fatal(errorInEncodeˢ, encodeErr);
    }
    if (panicErr == nil) {
        t.Fatal(topLevelNilPointerDidNotˢ);
    }
    @string errMsg = panicErr.Error();
    if (!strings.Contains(errMsg, nilPointerˢ)) {
        t.Fatal(expectedNilPointerErrorˢ, errMsg);
    }
}

internal static (error encodeErr, error panicErr) encodeAndRecover(any value) {
    error encodeErr = default;
    error panicErr = default;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            var e = recover();
            if (e != nil) {
                switch (e.type()) {
                case {} Δerr when Δerr._<error>(out var err): {
                    panicErr = err;
                    break;
                }
                default: {
                    var err = e;
                    panicErr = fmt.Errorf("%v"u8, err);
                    break;
                }}
            }
        }, ref ᒐ);

        encodeErr = NewEncoder(io.Discard).Encode(value);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return (encodeErr, panicErr);
}

internal partial struct TestNilPointerPanics_testCases /*dyn*/ {
    internal any value;
    internal bool mustPanic;
}

public static void TestNilPointerPanics(ж<testing.T> t) {
    ж<@string> nilStringPtr = default;
    ref var intMap = ref heap<map<nint, nint>>(out var ᏑintMap);

    intMap = new map<nint, nint>();
    ж<map<nint, nint>> intMapPtr = ᏑintMap;
    ж<map<nint, nint>> nilIntMapPtr = default;
    nint zero = default;
    channel<bool> nilBoolChannel = default;
    ж<channel<bool>> nilBoolChannelPtr = default;
    slice<@string> nilStringSlice = default;
    slice<@string> stringSlice = new slice<@string>(1);
    ж<slice<@string>> nilStringSlicePtr = default;

    var testCases = slice<TestNilPointerPanics_testCases>([
        new(nilStringPtr.OrTypedNil(), true),
        new(intMap, false),
        new(intMapPtr.OrTypedNil(), false),
        new(nilIntMapPtr.OrTypedNil(), true),
        new(zero, false),
        new(nilStringSlice, false),
        new(stringSlice, false),
        new(nilStringSlicePtr.OrTypedNil(), true),
        new(nilBoolChannel, false),
        new(nilBoolChannelPtr.OrTypedNil(), true)
    ]);

    foreach (var (_, tt) in testCases) {
        var (_, panicErr) = encodeAndRecover(tt.value);
        if (tt.mustPanic) {
            if (panicErr == nil) {
                t.Errorf("expected panic with input %#v, did not panic", tt.value);
            }
            continue;
        }
        if (panicErr != nil) {
            t.Fatalf("expected no panic with input %#v, got panic=%v", tt.value, panicErr);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedErrorGotNoneˢ2 = (@string)"expected error, got none"u8;
internal static ref readonly @string interfaceˢ => ref interfaceˢᵇ;
private static readonly @string interfaceˢᵇ = "interface"u8;
internal static readonly object expectedErrorAboutNilˢ = (@string)"expected error about nil pointer and interface, got:"u8;

internal partial struct TestNilPointerInsideInterface_si /*dyn*/ {
    public any I;
}

public static void TestNilPointerInsideInterface(ж<testing.T> t) {
    ж<nint> ip = default;
    var si = new TestNilPointerInsideInterface_si(
        I: ip.OrTypedNil()
    );
    var buf = @new<bytes.Buffer>();
    var err = NewEncoder(new gob_test_package.bytes_BufferжWriter(buf)).Encode(si);
    if (err == nil) {
        t.Fatal(expectedErrorGotNoneˢ2);
    }
    @string errMsg = err.Error();
    if (!strings.Contains(errMsg, nilPointerˢ) || !strings.Contains(errMsg, interfaceˢ)) {
        t.Fatal(expectedErrorAboutNilˢ, errMsg);
    }
}

public partial struct Bug4Public {
    public @string Name;
    public Bug4Secret Secret;
}

public partial struct Bug4Secret {
    internal nint a; // error: no exported fields.
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object firstEncodingExpectedˢ = (@string)"first encoding: expected error"u8;
internal static readonly object secondEncodingExpectedˢ = (@string)"second encoding: expected error"u8;
internal static ref readonly @string noExportedFieldsˢ => ref noExportedFieldsˢᵇ;
private static readonly @string noExportedFieldsˢᵇ = "no exported fields"u8;

// Test that a failed compilation doesn't leave around an executable encoder.
// Issue 3723.
public static void TestMultipleEncodingsOfBadType(ж<testing.T> t) {
    var x = new Bug4Public(
        Name: "name",
        Secret: new Bug4Secret(1)
    );
    var buf = @new<bytes.Buffer>();
    var enc = NewEncoder(new gob_test_package.bytes_BufferжWriter(buf));
    var err = enc.Encode(x);
    if (err == nil) {
        t.Fatal(firstEncodingExpectedˢ);
    }
    buf.Reset();
    enc = NewEncoder(new gob_test_package.bytes_BufferжWriter(buf));
    err = enc.Encode(x);
    if (err == nil) {
        t.Fatal(secondEncodingExpectedˢ);
    }
    if (!strings.Contains(err.Error(), noExportedFieldsˢ)) {
        t.Errorf("expected error about no exported fields; got %v", err);
    }
}

// There was an error check comparing the length of the input with the
// length of the slice being decoded. It was wrong because the next
// thing in the input might be a type definition, which would lead to
// an incorrect length check. This test reproduces the corner case.
public partial struct Z {
}

public static void Test29ElementSlice(ж<testing.T> t) {
    Register(new Z());
    var src = new slice<any>(100); // Size needs to be bigger than size of type definition.
    foreach (var i in range(src)) {
        src[i] = new Z();
    }
    var buf = @new<bytes.Buffer>();
    var err = NewEncoder(new gob_test_package.bytes_BufferжWriter(buf)).Encode(src);
    if (err != nil) {
        t.Fatalf("encode: %v", err);
        return;
    }

    ref var dst = ref heap<slice<any>>(out var Ꮡdst);
    err = NewDecoder(new gob_test_package.bytes_BufferжReader(buf)).Decode(Ꮡdst);
    if (err != nil) {
        t.Errorf("decode: %v", err);
        return;
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object decodeNoErrorˢ = (@string)"decode: no error"u8;
internal static ref readonly @string sliceTooBigˢ => ref sliceTooBigˢᵇ;
private static readonly @string sliceTooBigˢᵇ = "slice too big"u8;

// Don't crash, just give error when allocating a huge slice.
// Issue 8084.
public static void TestErrorForHugeSlice(ж<testing.T> t) {
    // Encode an int slice.
    var buf = @new<bytes.Buffer>();
    ref var Δslice = ref heap<slice<nint>>(slice<nint>([1, 1, 1, 1, 1, 1, 1, 1, 1, 1]), out var Ꮡslice);
    var err = NewEncoder(new gob_test_package.bytes_BufferжWriter(buf)).Encode(Δslice);
    if (err != nil) {
        t.Fatal(encodeˢ, err);
    }
    // Reach into the buffer and smash the count to make the encoded slice very long.
    buf.Bytes()[buf.Len() - len(Δslice) - 1] = 0xfa;
    // Decode and see error.
    err = NewDecoder(new gob_test_package.bytes_BufferжReader(buf)).Decode(Ꮡslice);
    if (err == nil) {
        t.Fatal(decodeNoErrorˢ);
    }
    if (!strings.Contains(err.Error(), sliceTooBigˢ)) {
        t.Fatalf("decode: expected slice too big error, got %s", err.Error());
    }
}

internal partial struct badDataTest {
    internal @string input; // The input encoded as a hex string.
    internal @string error; // A substring of the error that should result.
    internal any data;      // What to decode into.
}

internal static slice<badDataTest> badDataTests = slice<badDataTest>([
    new("", "EOF", default),
    new("7F6869", "unexpected EOF", default),
    new("036e6f77206973207468652074696d6520666f7220616c6c20676f6f64206d656e", "unknown type id", @new<ET2>()),
    new("0424666f6f", "field numbers out of bounds", @new<ET2>()), // Issue 6323.
    new("05100028557b02027f8302", "interface encoding", default),  // Issue 10270.
    // Issue 10273.
    new("130a00fb5dad0bf8ff020263e70002fa28020202a89859", "slice length too large", default),
    new("0f1000fb285d003316020735ff023a65c5", "interface encoding", default),
    new("03fffb0616fffc00f902ff02ff03bf005d02885802a311a8120228022c028ee7", "GobDecoder", default),
    // Issue 10491.
    new("10fe010f020102fe01100001fe010e000016fe010d030102fe010e00010101015801fe01100000000bfe011000f85555555555555555", "exceeds input size", default)
]);

// TestBadData tests that various problems caused by malformed input
// are caught as errors and do not cause panics.
public static void TestBadData(ж<testing.T> t) {
    foreach (var (i, test) in badDataTests) {
        var (data, err) = hex.DecodeString(test.input);
        if (err != nil) {
            t.Fatalf("#%d: hex error: %s", i, err);
        }
        var d = NewDecoder(new gob_test_package.bytes_ReaderжReader(bytes.NewReader(data)));
        err = d.Decode(test.data);
        if (err == nil) {
            t.Errorf("decode: no error");
            continue;
        }
        if (!strings.Contains(err.Error(), test.error)) {
            t.Errorf("#%d: decode: expected %q error, got %s", i, test.error, err.Error());
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string duplicateTypeˢ => ref duplicateTypeˢᵇ;
private static readonly @string duplicateTypeˢᵇ = "duplicate type"u8;

internal partial struct TestDecodeErrorMultipleTypes_Test /*dyn*/ {
    public @string A;
    public nint B;
}

public static void TestDecodeErrorMultipleTypes(ж<testing.T> t) {
    ref var b = ref heap(new bytes.Buffer(), out var Ꮡb);
    NewEncoder(new gob_test_package.bytes_BufferжWriter(Ꮡb)).Encode(new TestDecodeErrorMultipleTypes_Test("one", 1));

    ref var result = ref heap(new TestDecodeErrorMultipleTypes_Test(), out var Ꮡresult);
    ref var result2 = ref heap(new TestDecodeErrorMultipleTypes_Test(), out var Ꮡresult2);
    var dec = NewDecoder(new gob_test_package.bytes_BufferжReader(Ꮡb));
    var err = dec.Decode(Ꮡresult);
    if (err != nil) {
        t.Errorf("decode: unexpected error %v", err);
    }

    b.Reset();
    NewEncoder(new gob_test_package.bytes_BufferжWriter(Ꮡb)).Encode(new TestDecodeErrorMultipleTypes_Test("two", 2));
    err = dec.Decode(Ꮡresult2);
    if (err == nil) {
        t.Errorf("decode: expected duplicate type error, got nil");
    } else if (!strings.Contains(err.Error(), duplicateTypeˢ)) {
        t.Errorf("decode: expected duplicate type error, got %s", err.Error());
    }
}

internal partial struct TestMarshalFloatMap_mapEntry /*dyn*/ {
    internal uint64 keyBits;
    internal @string value;
}

// Issue 24075
public static void TestMarshalFloatMap(ж<testing.T> t) {
    var nan1 = math.NaN();
    var nan2 = math.Float64frombits(math.Float64bits(nan1) ^ 1); // A different NaN in the same class.

    var @in = new map<float64, @string>{
        [nan1] = "a",
        [nan1] = "b",
        [nan2] = "c"
    };

    ref var b = ref heap(new bytes.Buffer(), out var Ꮡb);
    var enc = NewEncoder(new gob_test_package.bytes_BufferжWriter(Ꮡb));
    {
        var err = enc.Encode(@in); if (err != nil) {
            t.Errorf("Encode : %v", err);
        }
    }

    ref var @out = ref heap<map<float64, @string>>(new map<float64, @string>{}, out var Ꮡout);
    var dec = NewDecoder(new gob_test_package.bytes_BufferжReader(Ꮡb));
    {
        var err = dec.Decode(Ꮡout); if (err != nil) {
            t.Fatalf("Decode : %v", err);
        }
    }
    slice<TestMarshalFloatMap_mapEntry> /*entries*/ readMap(map<float64, @string> m) {
        slice<TestMarshalFloatMap_mapEntry> entries = default;
        foreach (var (k, v) in m) {
            entries = append(entries, new TestMarshalFloatMap_mapEntry(math.Float64bits(k), v));
        }
        slices.SortFunc(entries, (TestMarshalFloatMap_mapEntry a, TestMarshalFloatMap_mapEntry bΔ1) => {
            nint r = cmp.Compare(a.keyBits, bΔ1.keyBits);
            if (r != 0) {
                return r;
            }
            return cmp.Compare(a.value, bΔ1.value);
        });
        return entries;
    }

    var got = readMap(@out);
    var want = readMap(@in);
    if (!slices.Equal<slice<TestMarshalFloatMap_mapEntry>, TestMarshalFloatMap_mapEntry>(got, want)) {
        t.Fatalf("\nEncode: %v\nDecode: %v", want, got);
    }
}

internal partial struct TestDecodePartial_T /*dyn*/ {
    public slice<nint> X;
    public @string Y;
}

public static void TestDecodePartial(ж<testing.T> t) {
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    ref var t1 = ref heap<TestDecodePartial_T>(new TestDecodePartial_T(X: slice<nint>([1, 2, 3]), Y: "foo"), out var Ꮡt1);
    ref var t2 = ref heap<TestDecodePartial_T>(new TestDecodePartial_T(X: slice<nint>([4, 5, 6]), Y: "bar"), out var Ꮡt2);
    var enc = NewEncoder(new gob_test_package.bytes_BufferжWriter(Ꮡbuf));

    nint t1start = 0;
    {
        var err = enc.Encode(Ꮡt1); if (err != nil) {
            t.Fatal(err);
        }
    }

    nint t2start = buf.Len();
    {
        var err = enc.Encode(Ꮡt2); if (err != nil) {
            t.Fatal(err);
        }
    }

    var data = buf.Bytes();
    for (nint i = 0; i <= len(data); i++) {
        var bufr = bytes.NewReader(data.slice(0, i));

        // Decode both values, stopping at the first error.
        ref var t1b = ref heap(new TestDecodePartial_T(), out var Ꮡt1b);
        ref var t2b = ref heap(new TestDecodePartial_T(), out var Ꮡt2b);
        var dec = NewDecoder(new gob_test_package.bytes_ReaderжReader(bufr));
        error err = default;
        err = dec.Decode(Ꮡt1b);
        if (err == nil) {
            err = dec.Decode(Ꮡt2b);
        }

        switch (i) {
        case var ᴛ when ᴛ == t1start || ᴛ == t2start: {
            // Either the first or the second Decode calls had zero input.
            if (!AreEqual(err, io.EOF)) {
                t.Errorf("%d/%d: expected io.EOF: %v", i, len(data), err);
            }
            break;
        }
        case var ᴛ when ᴛ == len(data): {
            // We reached the end of the entire input.
            if (err != nil) {
                t.Errorf("%d/%d: unexpected error: %v", i, len(data), err);
            }
            if (!reflect.DeepEqual(t1b, t1)) {
                t.Fatalf("t1 value mismatch: got %v, want %v", t1b, t1);
            }
            if (!reflect.DeepEqual(t2b, t2)) {
                t.Fatalf("t2 value mismatch: got %v, want %v", t2b, t2);
            }
            break;
        }
        default: {
            // In between, we must see io.ErrUnexpectedEOF.
            // The decoder used to erroneously return io.EOF in some cases here,
            // such as if the input was cut off right after some type specs,
            // but before any value was actually transmitted.
            if (!AreEqual(err, io.ErrUnexpectedEOF)) {
                t.Errorf("%d/%d: expected io.ErrUnexpectedEOF: %v", i, len(data), err);
            }
            break;
        }}
    }
}

public static void TestDecoderOverflow(ж<testing.T> t) {
    // Issue 55337.
    var dec = NewDecoder(new gob_test_package.bytes_ReaderжReader(bytes.NewReader(slice<byte>([
        0x12, 0xff, 0xff, 0x2, 0x2, 0x20, 0x0, 0xf8, 0x7f, 0xff, 0xff, 0xff,
        0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x20, 0x20, 0x20, 0x20, 0x20
    ]))));
    any r = default;
    var err = dec.Decode(r);
    if (err == nil) {
        t.Fatalf("expected an error");
    }
}

} // end gob_internal_test_package
