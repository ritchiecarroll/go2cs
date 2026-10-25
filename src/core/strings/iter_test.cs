// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using static strings_package;
using testing = testing_package;
using iter = iter_package;
using static go.strings_internal_test_package;

partial class strings_test_package {

public static void BenchmarkSplitSeqEmptySeparator(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    foreach (var _ᴛ1 in range(b.N)) {
        foreach (var _ᴛ2 in range<@string>(SplitSeq(benchInputHard, ""u8).Invoke)) {
        }
    }
}

public static void BenchmarkSplitSeqSingleByteSeparator(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    foreach (var _ᴛ1 in range(b.N)) {
        foreach (var _ᴛ2 in range<@string>(SplitSeq(benchInputHard, "/"u8).Invoke)) {
        }
    }
}

public static void BenchmarkSplitSeqMultiByteSeparator(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    foreach (var _ᴛ1 in range(b.N)) {
        foreach (var _ᴛ2 in range<@string>(SplitSeq(benchInputHard, helloˢ).Invoke)) {
        }
    }
}

public static void BenchmarkSplitAfterSeqEmptySeparator(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    foreach (var _ᴛ1 in range(b.N)) {
        foreach (var _ᴛ2 in range<@string>(SplitAfterSeq(benchInputHard, ""u8).Invoke)) {
        }
    }
}

public static void BenchmarkSplitAfterSeqSingleByteSeparator(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    foreach (var _ᴛ1 in range(b.N)) {
        foreach (var _ᴛ2 in range<@string>(SplitAfterSeq(benchInputHard, "/"u8).Invoke)) {
        }
    }
}

public static void BenchmarkSplitAfterSeqMultiByteSeparator(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    foreach (var _ᴛ1 in range(b.N)) {
        foreach (var _ᴛ2 in range<@string>(SplitAfterSeq(benchInputHard, helloˢ).Invoke)) {
        }
    }
}

internal static @string findKvBySplit(@string s, @string k) {
    foreach (var (_, kv) in Split(s, ","u8)) {
        if (HasPrefix(kv, k)) {
            return kv;
        }
    }
    return ""u8;
}

internal static @string findKvBySplitSeq(@string s, @string k) {
    foreach (var kv in range<@string>(SplitSeq(s, ","u8).Invoke)) {
        if (HasPrefix(kv, k)) {
            return kv;
        }
    }
    return ""u8;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string k1V1K2V2K3V3K4V4ˢ = "k1=v1,k2=v2,k3=v3,k4=v4"u8;

internal partial struct BenchmarkSplitAndSplitSeq_testCases /*dyn*/ {
    internal @string name;
    internal @string input;
}

public static void BenchmarkSplitAndSplitSeq(ж<testing.B> Ꮡb) {
    @string testSplitString = k1V1K2V2K3V3K4V4ˢ;
    var testCases = new BenchmarkSplitAndSplitSeq_testCases[]{
        new(
            name: "Key found"u8,
            input: "k3"u8
        ),
        new(
            name: "Key not found"u8,
            input: "k100"u8
        )
    }.slice();

    foreach (var (_, vᴛ1) in testCases) {
        ref var testCase = ref heap(new BenchmarkSplitAndSplitSeq_testCases(), out var ᏑtestCase);
        testCase = vᴛ1;

        var testCaseʗ1 = testCase;
        Ꮡb.Run("bySplit "u8 + testCase.name, (ж<testing.B> bΔ1) => {
            bΔ1.ResetTimer();
            bΔ1.ReportAllocs();
            while (bΔ1.Loop()) {
                findKvBySplit(testSplitString, testCaseʗ1.input);
            }
        });

        var testCaseʗ2 = testCase;
        Ꮡb.Run("bySplitSeq "u8 + testCase.name, (ж<testing.B> bΔ2) => {
            bΔ2.ResetTimer();
            bΔ2.ReportAllocs();
            while (bΔ2.Loop()) {
                findKvBySplitSeq(testSplitString, testCaseʗ2.input);
            }
        });
    }
}

} // end strings_test_package
