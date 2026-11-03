// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build 386 || amd64
namespace go.@internal;

using static go.@internal.cpu_package;
using godebug = go.@internal.godebug_package;
using testing = testing_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using go.@internal;
using static go.@internal.cpu_internal_test_package;

partial class cpu_test_package {

public static partial void TestX86ifAVX2hasAVX(ж<testing.T> t) {
    if (X86.HasAVX2 && !X86.HasAVX) {
        t.Fatalf("HasAVX expected true when HasAVX2 is true, got false");
    }
}

public static partial void TestX86ifAVX512FhasAVX2(ж<testing.T> t) {
    if (X86.HasAVX512F && !X86.HasAVX2) {
        t.Fatalf("HasAVX2 expected true when HasAVX512F is true, got false");
    }
}

public static partial void TestX86ifAVX512BWhasAVX512F(ж<testing.T> t) {
    if (X86.HasAVX512BW && !X86.HasAVX512F) {
        t.Fatalf("HasAVX512F expected true when HasAVX512BW is true, got false");
    }
}

public static partial void TestX86ifAVX512VLhasAVX512F(ж<testing.T> t) {
    if (X86.HasAVX512VL && !X86.HasAVX512F) {
        t.Fatalf("HasAVX512F expected true when HasAVX512VL is true, got false");
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingTestCanTRunOnˢ = (@string)"skipping test: can't run on GOAMD64>v1 machines"u8;
internal static ref readonly @string testSSE3DebugOptionˢ => ref testSSE3DebugOptionˢᵇ;
private static readonly @string testSSE3DebugOptionˢᵇ = "TestSSE3DebugOption"u8;
internal static ref readonly @string cpuSse3Offˢ => ref cpuSse3Offˢᵇ;
private static readonly @string cpuSse3Offˢᵇ = "cpu.sse3=off"u8;

public static partial void TestDisableSSE3(ж<testing.T> t) {
    if (cpu_internal_test_package.GetGOAMD64level() > 1) {
        t.Skip(skippingTestCanTRunOnˢ);
    }
    runDebugOptionsTest(t, testSSE3DebugOptionˢ, cpuSse3Offˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string cpuSse3ˢ => ref cpuSse3ˢᵇ;
private static readonly @string cpuSse3ˢᵇ = "#cpu.sse3"u8;

public static void TestSSE3DebugOption(ж<testing.T> t) {
    MustHaveDebugOptionsSupport(t);

    if (godebug.New(cpuSse3ˢ).Value() != "off"u8) {
        t.Skipf("skipping test: GODEBUG=cpu.sse3=off not set");
    }

    var want = false;
    {
        var got = X86.HasSSE3; if (got != want) {
            t.Errorf("X86.HasSSE3 expected %v, got %v", want, got);
        }
    }
}

} // end cpu_test_package
