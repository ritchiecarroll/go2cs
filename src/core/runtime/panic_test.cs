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
public static void TestPanicWithDirectlyPrintableCustomTypes(ж<testing.T> t) {
    var tests = slice<TestPanicWithDirectlyPrintableCustomTypes_tests>([
        new("panicCustomBool", @"panic: main.MyBool(true)"),
        new("panicCustomComplex128", @"panic: main.MyComplex128(32.1+10i)"),
        new("panicCustomComplex64", @"panic: main.MyComplex64(0.11+3i)"),
        new("panicCustomFloat32", @"panic: main.MyFloat32(-93.7)"),
        new("panicCustomFloat64", @"panic: main.MyFloat64(-93.7)"),
        new("panicCustomInt", @"panic: main.MyInt(93)"),
        new("panicCustomInt8", @"panic: main.MyInt8(93)"),
        new("panicCustomInt16", @"panic: main.MyInt16(93)"),
        new("panicCustomInt32", @"panic: main.MyInt32(93)"),
        new("panicCustomInt64", @"panic: main.MyInt64(93)"),
        new("panicCustomString", @"panic: main.MyString(""Panic"u8 + "\n\t"u8 + @"line two"")"u8),
        new("panicCustomUint", @"panic: main.MyUint(93)"),
        new("panicCustomUint8", @"panic: main.MyUint8(93)"),
        new("panicCustomUint16", @"panic: main.MyUint16(93)"),
        new("panicCustomUint32", @"panic: main.MyUint32(93)"),
        new("panicCustomUint64", @"panic: main.MyUint64(93)"),
        new("panicCustomUintptr", @"panic: main.MyUintptr(93)"),
        new("panicDeferFatal", "panic: runtime.errorString(\"invalid memory address or nil pointer dereference\")\n\tfatal error: sync: unlock of unlocked mutex"),
        new("panicDoublieDeferFatal", "panic: runtime.errorString(\"invalid memory address or nil pointer dereference\") [recovered, repanicked]\n\tfatal error: sync: unlock of unlocked mutex")
    ]);

    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestPanicWithDirectlyPrintableCustomTypes_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var tΔ1 = t;
        var ttʗ1 = tt;
        tΔ1.Run(tt.name, (ж<testing.T> tΔ2) => {
            @string output = runTestProg(tΔ2, testprogˢ, ttʗ1.name);
            if (!strings.HasPrefix(output, ttʗ1.wantPanicPrefix)) {
                tΔ2.Fatalf("%q\nis not present in\n%s", ttʗ1.wantPanicPrefix, output);
            }
        });
    }
}

} // end runtime_test_package
