// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.log.slog.@internal;

using race = go.@internal.race_package;
using testenv = go.@internal.testenv_package;
using testing = testing_package;
using go.@internal;
using static go.log.slog.@internal.buffer_package;

partial class buffer_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string helloˢ = "hello"u8;
internal static readonly @string helloWorldˢ = "hello, world"u8;

public static void Test(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var b = New();
        var bʗ1 = b;
        defer(bʗ1.Free, ref ᒐ);
        b.WriteString(helloˢ);
        b.WriteByte((rune)',');
        b.Write(slice<byte>(" world"u8));

        @string got = b.String();
        @string want = helloWorldˢ;
        if (got != want) {
            Ꮡt.Errorf("got %q, want %q"u8, got, want);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingTestInRaceModeˢ = (@string)"skipping test in race mode"u8;
internal static readonly @string not1kWorthOfBytesˢ = "not 1K worth of bytes"u8;

public static void TestAlloc(ж<testing.T> Ꮡt) {
    if (race.Enabled) {
        Ꮡt.Skip(skippingTestInRaceModeˢ);
    }
    testenv.SkipIfOptimizationOff(new buffer_internal_test_package.testing_TжTB(Ꮡt));
    nint got = (nint)testing.AllocsPerRun(5, () => {
        GoFrame ᒐ = default;
        try {
            var b = New();
            var bʗ1 = b;
            defer(bʗ1.Free, ref ᒐ);
            b.WriteString(not1kWorthOfBytesˢ);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
    if (got != 0) {
        Ꮡt.Errorf("got %d allocs, want 0"u8, got);
    }
}

} // end buffer_internal_test_package
