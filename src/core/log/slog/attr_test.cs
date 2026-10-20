// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.log;

using asan = go.@internal.asan_package;
using testenv = go.@internal.testenv_package;
using testing = testing_package;
using time = time_package;
using go.@internal;
using static go.log.slog_package;

partial class slog_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object testAllocatesWithAsanˢ = (@string)"test allocates with -asan"u8;
internal static readonly @string keyˢ = "key"u8;
internal static readonly @string fooˢ = "foo"u8;

public static void TestAttrNoAlloc(ж<testing.T> Ꮡt) {
    if (asan.Enabled) {
        Ꮡt.Skip(testAllocatesWithAsanˢ);
    }
    testenv.SkipIfOptimizationOff(new slog_test_package.testing_TжTB(Ꮡt));
    // Assign values just to make sure the compiler doesn't optimize away the statements.
    ref var i = ref heap(new int64(), out var Ꮡi);
    uint64 u = default!;
    float64 f = default!;
    bool b = default!;
    @string s = default!;
    ref var x = ref heap<any>(out var Ꮡx);
    ж<int64> p = Ꮡi;
    time.Duration d = default!;
    var pʗ1 = p;
    nint a = (nint)testing.AllocsPerRun(5, () => {
        Ꮡi.Value = go.log.slog_package.Int64(keyˢ, 1).Value.Int64();
        u = go.log.slog_package.Uint64(keyˢ, 1).Value.Uint64();
        f = go.log.slog_package.Float64(keyˢ, 1D).Value.Float64();
        b = go.log.slog_package.Bool(keyˢ, true).Value.Bool();
        s = go.log.slog_package.String(keyˢ, fooˢ).Value.String();
        d = go.log.slog_package.Duration(keyˢ, d).Value.Duration();
        Ꮡx.ValueSlot = go.log.slog_package.Any(keyˢ, pʗ1.OrTypedNil()).Value.Any();
    });
    if (a != 0) {
        Ꮡt.Errorf("got %d allocs, want zero"u8, a);
    }
    _ = u;
    _ = f;
    _ = b;
    _ = s;
    _ = x;
}

public static void BenchmarkAttrString(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    ref var @is = ref heap(new @string(), out var Ꮡis);
    @string u = default!;
    @string f = default!;
    @string bn = default!;
    @string s = default!;
    @string x = default!;
    @string ds = default!;
    ж<@string> p = Ꮡis;
    time.Duration d = default!;
    b.ReportAllocs();
    for (nint i = 0; i < b.N; i++) {
        @is = go.log.slog_package.Int64(keyˢ, 1).String();
        u = go.log.slog_package.Uint64(keyˢ, 1).String();
        f = go.log.slog_package.Float64(keyˢ, 1D).String();
        bn = go.log.slog_package.Bool(keyˢ, true).String();
        s = go.log.slog_package.String(keyˢ, fooˢ).String();
        ds = go.log.slog_package.Duration(keyˢ, d).String();
        x = go.log.slog_package.Any(keyˢ, p.OrTypedNil()).String();
    }
    _ = u;
    _ = f;
    _ = bn;
    _ = s;
    _ = x;
    _ = ds;
    _ = p;
}

} // end slog_internal_test_package
