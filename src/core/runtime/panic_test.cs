// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using strings = strings_package;
using testing = testing_package;
using static global::go.runtime_internal_test_package;

partial class runtime_test_package {

internal partial struct TestPanicWithDirectlyPrintableCustomTypes_tests /*dyn*/ {
    internal @string name;
    internal @string wantPanicPrefix;
}

// Test that panics print out the underlying value
// when the underlying kind is directly printable.
// Issue: https://golang.org/issues/37531
public static void TestPanicWithDirectlyPrintableCustomTypes(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var tests = new TestPanicWithDirectlyPrintableCustomTypes_tests[]{
        new("panicCustomBool"u8, @"panic: main.MyBool(true)"u8),
        new("panicCustomComplex128"u8, @"panic: main.MyComplex128(+3.210000e+001+1.000000e+001i)"u8),
        new("panicCustomComplex64"u8, @"panic: main.MyComplex64(+1.100000e-001+3.000000e+000i)"u8),
        new("panicCustomFloat32"u8, @"panic: main.MyFloat32(-9.370000e+001)"u8),
        new("panicCustomFloat64"u8, @"panic: main.MyFloat64(-9.370000e+001)"u8),
        new("panicCustomInt"u8, @"panic: main.MyInt(93)"u8),
        new("panicCustomInt8"u8, @"panic: main.MyInt8(93)"u8),
        new("panicCustomInt16"u8, @"panic: main.MyInt16(93)"u8),
        new("panicCustomInt32"u8, @"panic: main.MyInt32(93)"u8),
        new("panicCustomInt64"u8, @"panic: main.MyInt64(93)"u8),
        new("panicCustomString"u8, @"panic: main.MyString(""Panic"u8 + "\n\t"u8 + @"line two"")"u8),
        new("panicCustomUint"u8, @"panic: main.MyUint(93)"u8),
        new("panicCustomUint8"u8, @"panic: main.MyUint8(93)"u8),
        new("panicCustomUint16"u8, @"panic: main.MyUint16(93)"u8),
        new("panicCustomUint32"u8, @"panic: main.MyUint32(93)"u8),
        new("panicCustomUint64"u8, @"panic: main.MyUint64(93)"u8),
        new("panicCustomUintptr"u8, @"panic: main.MyUintptr(93)"u8)
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestPanicWithDirectlyPrintableCustomTypes_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var tΔ1 = Ꮡt;
        var ttʗ1 = tt;
        tΔ1.Run(tt.name, (ж<testing.T> tΔ2) => {
            @string output = runTestProg(tΔ2, testprogˢ, ttʗ1.name);
            if (!strings.HasPrefix(output, ttʗ1.wantPanicPrefix)) {
                tΔ2.Fatalf("%q\nis not present in\n%s"u8, ttʗ1.wantPanicPrefix, output);
            }
        });
    }
}

} // end runtime_test_package
