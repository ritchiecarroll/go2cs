// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using testing = testing_package;
using quick = go.testing.quick_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using go.testing;
using static go.crypto.subtle_package;

partial class subtle_internal_test_package {

public partial struct TestConstantTimeCompareStruct {
    internal slice<byte> a, b;
    internal nint @out;
}

internal static slice<TestConstantTimeCompareStruct> testConstantTimeCompareData = slice<TestConstantTimeCompareStruct>([
    new(slice<byte>([]), slice<byte>([]), 1),
    new(slice<byte>([0x11]), slice<byte>([0x11]), 1),
    new(slice<byte>([0x12]), slice<byte>([0x11]), 0),
    new(slice<byte>([0x11]), slice<byte>([0x11, 0x12]), 0),
    new(slice<byte>([0x11, 0x12]), slice<byte>([0x11]), 0)
]);

public static void TestConstantTimeCompare(ж<testing.T> t) {
    foreach (var (i, test) in testConstantTimeCompareData) {
        {
            nint r = ConstantTimeCompare(test.a, test.b); if (r != test.@out) {
                t.Errorf("#%d bad result (got %x, want %x)", i, r, test.@out);
            }
        }
    }
}

public partial struct TestConstantTimeByteEqStruct {
    internal uint8 a, b;
    internal nint @out;
}

internal static slice<TestConstantTimeByteEqStruct> testConstandTimeByteEqData = slice<TestConstantTimeByteEqStruct>([
    new(0, 0, 1),
    new(0, 1, 0),
    new(1, 0, 0),
    new(0xff, 0xff, 1),
    new(0xff, 0xfe, 0)
]);

internal static nint byteEq(uint8 a, uint8 b) {
    if (a == b) {
        return 1;
    }
    return 0;
}

public static void TestConstantTimeByteEq(ж<testing.T> t) {
    foreach (var (i, test) in testConstandTimeByteEqData) {
        {
            nint r = ConstantTimeByteEq(test.a, test.b); if (r != test.@out) {
                t.Errorf("#%d bad result (got %x, want %x)", i, r, test.@out);
            }
        }
    }
    var err = quick.CheckEqual(ConstantTimeByteEq, byteEq, nil);
    if (err != nil) {
        t.Error(err);
    }
}

internal static nint eq(int32 a, int32 b) {
    if (a == b) {
        return 1;
    }
    return 0;
}

public static partial void TestConstantTimeEq(ж<testing.T> t) {
    var err = quick.CheckEqual(ConstantTimeEq, eq, nil);
    if (err != nil) {
        t.Error(err);
    }
}

internal static slice<byte> makeCopy(nint v, slice<byte> x, slice<byte> y) {
    if (len(x) > len(y)) {
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
    if (len(x) > len(y)) {
        x = x.slice(0, len(y));
    } else {
        y = y.slice(0, len(x));
    }
    v &= (nint)(1);
    ConstantTimeCopy(v, x, y);
    return x;
}

public static partial void TestConstantTimeCopy(ж<testing.T> t) {
    var err = quick.CheckEqual(constantTimeCopyWrapper, makeCopy, nil);
    if (err != nil) {
        t.Error(err);
    }
}

partial struct lessOrEqTestsᴛ1 /*dyn*/ {
    internal nint x, y, result;
}
internal static slice<lessOrEqTestsᴛ1> lessOrEqTests = slice<lessOrEqTestsᴛ1>([
    new(0, 0, 1),
    new(1, 0, 0),
    new(0, 1, 1),
    new(10, 20, 1),
    new(20, 10, 0),
    new(10, 10, 1)
]);

public static void TestConstantTimeLessOrEq(ж<testing.T> t) {
    foreach (var (i, test) in lessOrEqTests) {
        nint result = ConstantTimeLessOrEq(test.x, test.y);
        if (result != test.result) {
            t.Errorf("#%d: %d <= %d gave %d, expected %d", i, test.x, test.y, result, test.result);
        }
    }
}

internal static uint8 benchmarkGlobal;

public static void BenchmarkConstantTimeSelect(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    nint x = (nint)benchmarkGlobal;
    nint y = default;
    nint z = default;

    foreach (var _ᴛ1 in range(b.N)) {
        (y, z, x) = (ConstantTimeSelect(x, y, z), y, z);
    }

    benchmarkGlobal = (uint8)x;
}

public static void BenchmarkConstantTimeByteEq(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    uint8 x = default;
    uint8 y = default;

    for (nint i = 0; i < b.N; i++) {
        (x, y) = ((uint8)ConstantTimeByteEq(x, y), x);
    }

    benchmarkGlobal = x;
}

public static void BenchmarkConstantTimeEq(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    nint x = default;
    nint y = default;

    for (nint i = 0; i < b.N; i++) {
        (x, y) = (ConstantTimeEq((int32)x, (int32)y), x);
    }

    benchmarkGlobal = (uint8)x;
}

public static void BenchmarkConstantTimeLessOrEq(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    nint x = default;
    nint y = default;

    for (nint i = 0; i < b.N; i++) {
        (x, y) = (ConstantTimeLessOrEq(x, y), x);
    }

    benchmarkGlobal = (uint8)x;
}

} // end subtle_internal_test_package
