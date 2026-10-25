// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using isync = go.@internal.sync_package;
using testing = testing_package;
using go.@internal;
using static go.@internal.sync_internal_test_package;
using sync = go.@internal.sync_package;

partial class sync_test_package {

public static void BenchmarkHashTrieMapLoadSmall(ж<testing.B> Ꮡb) {
    benchmarkHashTrieMapLoad(Ꮡb, testDataSmall[..]);
}

public static void BenchmarkHashTrieMapLoad(ж<testing.B> Ꮡb) {
    benchmarkHashTrieMapLoad(Ꮡb, testData[..]);
}

public static void BenchmarkHashTrieMapLoadLarge(ж<testing.B> Ꮡb) {
    benchmarkHashTrieMapLoad(Ꮡb, testDataLarge[..]);
}

internal static void benchmarkHashTrieMapLoad(ж<testing.B> Ꮡb, slice<@string> data) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    ref var m = ref heap(new isync.HashTrieMap<@string, nint>(), out var Ꮡm);
    foreach (var i in range(data)) {
        Ꮡm.LoadOrStore(data[i], i);
    }
    b.ResetTimer();
    var dataʗ1 = data;
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        nint i = 0;
        while (pb.Next()) {
            (_, _) = Ꮡm.Load(dataʗ1[i]);
            i++;
            if (i >= len(dataʗ1)) {
                i = 0;
            }
        }
    });
}

public static void BenchmarkHashTrieMapLoadOrStore(ж<testing.B> Ꮡb) {
    benchmarkHashTrieMapLoadOrStore(Ꮡb, testData[..]);
}

public static void BenchmarkHashTrieMapLoadOrStoreLarge(ж<testing.B> Ꮡb) {
    benchmarkHashTrieMapLoadOrStore(Ꮡb, testDataLarge[..]);
}

internal static void benchmarkHashTrieMapLoadOrStore(ж<testing.B> Ꮡb, slice<@string> data) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    ref var m = ref heap(new isync.HashTrieMap<@string, nint>(), out var Ꮡm);

    var dataʗ1 = data;
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        nint i = 0;
        while (pb.Next()) {
            (_, _) = Ꮡm.LoadOrStore(dataʗ1[i], i);
            i++;
            if (i >= len(dataʗ1)) {
                i = 0;
            }
        }
    });
}

} // end sync_test_package
