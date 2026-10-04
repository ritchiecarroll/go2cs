// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using testing = testing_package;
using quick = go.testing.quick_package;
using go.testing;
using static go.crypto.subtle_package;

partial class subtle_internal_test_package {

[GoType] public partial struct TestConstantTimeCompareStruct {
    internal slice<byte> a, b;
    internal nint @out;
}

internal static slice<TestConstantTimeCompareStruct> testConstantTimeCompareData = new TestConstantTimeCompareStruct[]{
    new(new byte[]{}.slice(), new byte[]{}.slice(), 1),
    new(new byte[]{0x11}.slice(), new byte[]{0x11}.slice(), 1),
    new(new byte[]{0x12}.slice(), new byte[]{0x11}.slice(), 0),
    new(new byte[]{0x11}.slice(), new byte[]{0x11, 0x12}.slice(), 0),
    new(new byte[]{0x11, 0x12}.slice(), new byte[]{0x11}.slice(), 0)
}.slice();

public static void TestConstantTimeCompare(ж<testing.T> Ꮡt) {
    foreach (var (i, test) in testConstantTimeCompareData) {
        {
            nint r = ConstantTimeCompare(test.a, test.b); if (r != test.@out) {
                Ꮡt.Errorf("#%d bad result (got %x, want %x)"u8, i, r, test.@out);
            }
        }
    }
}

[GoType] public partial struct TestConstantTimeByteEqStruct {
    internal uint8 a, b;
    internal nint @out;
}

internal static slice<TestConstantTimeByteEqStruct> testConstandTimeByteEqData = new TestConstantTimeByteEqStruct[]{
    new(0, 0, 1),
    new(0, 1, 0),
    new(1, 0, 0),
    new(0xff, 0xff, 1),
    new(0xff, 0xfe, 0)
}.slice();

internal static nint byteEq(uint8 a, uint8 b) {
    if (a == b) {
        return 1;
    }
    return 0;
}

public static void TestConstantTimeByteEq(ж<testing.T> Ꮡt) {
    foreach (var (i, test) in testConstandTimeByteEqData) {
        {
            nint r = ConstantTimeByteEq(test.a, test.b); if (r != test.@out) {
                Ꮡt.Errorf("#%d bad result (got %x, want %x)"u8, i, r, test.@out);
            }
        }
    }
    var err = quick.CheckEqual(ConstantTimeByteEq, byteEq, nil);
    if (err != default!) {
        Ꮡt.Error(err);
    }
}

internal static nint eq(int32 a, int32 b) {
    if (a == b) {
        return 1;
    }
    return 0;
}

public static void TestConstantTimeEq(ж<testing.T> Ꮡt) {
    var err = quick.CheckEqual(ConstantTimeEq, eq, nil);
    if (err != default!) {
        Ꮡt.Error(err);
    }
}

internal static slice<byte> makeCopy(nint v, slice<byte> x, slice<byte> y) {
    if (len(x) > len(y)){
        x = x.slice(0, len(y));
    } else {
        y = y.slice(0, len(x));
    }
    if (v == 1) {
        copy(x, y);
    }
    return x;
}

internal static slice<byte> constantTimeCopyWrapper(nint v, slice<byte> x, slice<byte> y) {
    if (len(x) > len(y)){
        x = x.slice(0, len(y));
    } else {
        y = y.slice(0, len(x));
    }
    v &= (nint)(1);
    ConstantTimeCopy(v, x, y);
    return x;
}

public static void TestConstantTimeCopy(ж<testing.T> Ꮡt) {
    var err = quick.CheckEqual(constantTimeCopyWrapper, makeCopy, nil);
    if (err != default!) {
        Ꮡt.Error(err);
    }
}


[GoType("dyn")] partial struct lessOrEqTestsᴛ1 {
    internal nint x, y, result;
}
internal static slice<lessOrEqTestsᴛ1> lessOrEqTests = new lessOrEqTestsᴛ1[]{
    new(0, 0, 1),
    new(1, 0, 0),
    new(0, 1, 1),
    new(10, 20, 1),
    new(20, 10, 0),
    new(10, 10, 1)
}.slice();

public static void TestConstantTimeLessOrEq(ж<testing.T> Ꮡt) {
    foreach (var (i, test) in lessOrEqTests) {
        nint result = ConstantTimeLessOrEq(test.x, test.y);
        if (result != test.result) {
            Ꮡt.Errorf("#%d: %d <= %d gave %d, expected %d"u8, i, test.x, test.y, result, test.result);
        }
    }
}

internal static uint8 benchmarkGlobal;

public static void BenchmarkConstantTimeByteEq(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    uint8 x = default!;
    uint8 y = default!;
    for (nint i = 0; i < b.N; i++) {
        (x, y) = ((uint8)ConstantTimeByteEq(x, y), x);
    }
    benchmarkGlobal = x;
}

public static void BenchmarkConstantTimeEq(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    nint x = default!;
    nint y = default!;
    for (nint i = 0; i < b.N; i++) {
        (x, y) = (ConstantTimeEq((int32)x, (int32)y), x);
    }
    benchmarkGlobal = (uint8)x;
}

public static void BenchmarkConstantTimeLessOrEq(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    nint x = default!;
    nint y = default!;
    for (nint i = 0; i < b.N; i++) {
        (x, y) = (ConstantTimeLessOrEq(x, y), x);
    }
    benchmarkGlobal = (uint8)x;
}

} // end subtle_internal_test_package
