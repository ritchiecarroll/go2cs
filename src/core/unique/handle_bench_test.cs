// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using fmt = fmt_package;
using Δruntime = runtime_package;
using testing = testing_package;
using static go.unique_package;

partial class unique_internal_test_package {

public static void BenchmarkMake(ж<testing.B> Ꮡb) {
    benchmarkMake(Ꮡb, new @string[]{"foo"u8}.slice());
}

public static void BenchmarkMakeMany(ж<testing.B> Ꮡb) {
    benchmarkMake(Ꮡb, testData[..]);
}

public static void BenchmarkMakeManyMany(ж<testing.B> Ꮡb) {
    benchmarkMake(Ꮡb, testDataLarge[..]);
}

internal static void benchmarkMake(ж<testing.B> Ꮡb, slice<@string> testData) {
    ref var b = ref Ꮡb.DerefOrNull();

    var handles = new slice<global::go.unique_package.Handle<@string>>(0, len(testData));
    foreach (var (i, _) in testData) {
        handles = append(handles, Make<@string>(testData[i]));
    }

    b.ReportAllocs();
    b.ResetTimer();

    var testDataʗ1 = testData;
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        nint i = 0;
        while (pb.Next()) {
            _ = Make<@string>(testDataʗ1[i]);
            i++;
            if (i >= len(testDataʗ1)) {
                i = 0;
            }
        }
    });

    b.StopTimer();

    Δruntime.GC();
    Δruntime.GC();
}

internal static array<@string> testData = new(128);
internal static array<@string> testDataLarge = new(131072);

[GoInit] internal static void init() {
    foreach (var (i, _) in testData) {
        testData[i] = fmt.Sprintf("%b"u8, i);
    }
    foreach (var (i, _) in testDataLarge) {
        testDataLarge[i] = fmt.Sprintf("%b"u8, i);
    }
}

} // end unique_internal_test_package
