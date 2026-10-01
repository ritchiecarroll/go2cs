// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using static runtime_package;
using testing = testing_package;
using time = time_package;
using static global::go.runtime_internal_test_package;
using Δruntime = runtime_package;

partial class runtime_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object needUpdateEvenThoughˢ = (@string)"need update even though updated half a period ago"u8;
internal static readonly object doesnTNeedUpdateEvenˢ = (@string)"doesn't need update even though updated 1.5 periods ago"u8;
internal static readonly object needUpdateEvenThoughJustˢ = (@string)"need update even though just updated"u8;

public static void TestGCCPULimiter(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    UntypedInt procs = 14;
    // Create mock time.
    var ticks = (int64)0;
    int64 advance(time.Duration d) {
        Ꮡt.Helper();
        ticks += (int64)d;
        return ticks;
    }
    // assistTime computes the CPU time for assists using frac of GOMAXPROCS
    // over the wall-clock duration d.
    int64 assistTime(time.Duration d, float64 frac) {
        Ꮡt.Helper();
        return (int64)(frac * (float64)(int64)d * (float64)procs);
    }
    var l = runtime_internal_test_package.NewGCCPULimiter(ticks, procs);
    // Do the whole test twice to make sure state doesn't leak across.
    uint64 baseOverflow = default!;  // Track total overflow across iterations.
    for (nint i = 0; i < 2; i++) {
        Ꮡt.Logf("Iteration %d"u8, i + 1);
        if (l.Capacity() != (uint64)(/* procs * CapacityPerProc */ 14000000000UL)) {
            Ꮡt.Fatalf("unexpected capacity: %d"u8, l.Capacity());
        }
        if (l.Fill() != 0) {
            Ꮡt.Fatalf("expected empty bucket to start"u8);
        }
        // Test filling the bucket with just mutator time.
        l.Update(advance(10 * time.Millisecond));
        l.Update(advance(1 * time.ΔSecond));
        l.Update(advance((time.Duration)(3600000000000L)));
        if (l.Fill() != 0) {
            Ꮡt.Fatalf("expected empty bucket from only accumulating mutator time, got fill of %d cpu-ns"u8, l.Fill());
        }
        // Test needUpdate.
        if (l.NeedUpdate(advance(runtime_internal_test_package.GCCPULimiterUpdatePeriod / 2))) {
            Ꮡt.Fatal(needUpdateEvenThoughˢ);
        }
        if (!l.NeedUpdate(advance(runtime_internal_test_package.GCCPULimiterUpdatePeriod))) {
            Ꮡt.Fatal(doesnTNeedUpdateEvenˢ);
        }
        l.Update(advance(0));
        if (l.NeedUpdate(advance(0))) {
            Ꮡt.Fatal(needUpdateEvenThoughJustˢ);
        }
        // Test transitioning the bucket to enable the GC.
        l.StartGCTransition(true, advance(109 * time.Millisecond));
        l.FinishGCTransition(advance(2 * time.Millisecond + 1 * time.Microsecond));
        {
            var expect = (uint64)(int64)((2 * time.Millisecond + 1 * time.Microsecond) * (int64)procs); if (l.Fill() != expect) {
                Ꮡt.Fatalf("expected fill of %d, got %d cpu-ns"u8, expect, l.Fill());
            }
        }
        // Test passing time without assists during a GC. Specifically, just enough to drain the bucket to
        // exactly procs nanoseconds (easier to get to because of rounding).
        //
        // The window we need to drain the bucket is 1/(1-2*gcBackgroundUtilization) times the current fill:
        //
        //   fill + (window * procs * gcBackgroundUtilization - window * procs * (1-gcBackgroundUtilization)) = n
        //   fill = n - (window * procs * gcBackgroundUtilization - window * procs * (1-gcBackgroundUtilization))
        //   fill = n + window * procs * ((1-gcBackgroundUtilization) - gcBackgroundUtilization)
        //   fill = n + window * procs * (1-2*gcBackgroundUtilization)
        //   window = (fill - n) / (procs * (1-2*gcBackgroundUtilization)))
        //
        // And here we want n=procs:
        var factor = (1D / (1D - 2D * runtime_internal_test_package.GCBackgroundUtilization));
        var fill = (2 * time.Millisecond + 1 * time.Microsecond) * (int64)procs;
        l.Update(advance(((time.Duration)(int64)(factor * (float64)(int64)(fill - (int64)procs) / (float64)procs))));
        if (l.Fill() != procs) {
            Ꮡt.Fatalf("expected fill %d cpu-ns from draining after a GC started, got fill of %d cpu-ns"u8, (nint)(procs), l.Fill());
        }
        // Drain to zero for the rest of the test.
        l.Update(advance((time.Duration)(28000000000L)));
        if (l.Fill() != 0) {
            Ꮡt.Fatalf("expected empty bucket from draining, got fill of %d cpu-ns"u8, l.Fill());
        }
        // Test filling up the bucket with 50% total GC work (so, not moving the bucket at all).
        l.AddAssistTime(assistTime(10 * time.Millisecond, 0.5D - runtime_internal_test_package.GCBackgroundUtilization));
        l.Update(advance(10 * time.Millisecond));
        if (l.Fill() != 0) {
            Ꮡt.Fatalf("expected empty bucket from 50%% GC work, got fill of %d cpu-ns"u8, l.Fill());
        }
        // Test adding to the bucket overall with 100% GC work.
        l.AddAssistTime(assistTime(time.Millisecond, 1.0D - runtime_internal_test_package.GCBackgroundUtilization));
        l.Update(advance(time.Millisecond));
        {
            var expect = (uint64)(int64)((int64)procs * time.Millisecond); if (l.Fill() != expect) {
                Ꮡt.Errorf("expected %d fill from 100%% GC CPU, got fill of %d cpu-ns"u8, expect, l.Fill());
            }
        }
        if (l.Limiting()) {
            Ꮡt.Errorf("limiter is enabled after filling bucket but shouldn't be"u8);
        }
        if (Ꮡt.Failed()) {
            Ꮡt.FailNow();
        }
        // Test filling the bucket exactly full.
        l.AddAssistTime(assistTime((int64)runtime_internal_test_package.CapacityPerProc - time.Millisecond, 1.0D - runtime_internal_test_package.GCBackgroundUtilization));
        l.Update(advance((int64)runtime_internal_test_package.CapacityPerProc - time.Millisecond));
        if (l.Fill() != l.Capacity()) {
            Ꮡt.Errorf("expected bucket filled to capacity %d, got %d"u8, l.Capacity(), l.Fill());
        }
        if (!l.Limiting()) {
            Ꮡt.Errorf("limiter is not enabled after filling bucket but should be"u8);
        }
        if (l.Overflow() != 0 + baseOverflow) {
            Ꮡt.Errorf("bucket filled exactly should not have overflow, found %d"u8, l.Overflow());
        }
        if (Ꮡt.Failed()) {
            Ꮡt.FailNow();
        }
        // Test adding with a delta of exactly zero. That is, GC work is exactly 50% of all resources.
        // Specifically, the limiter should still be on, and no overflow should accumulate.
        l.AddAssistTime(assistTime(1 * time.ΔSecond, 0.5D - runtime_internal_test_package.GCBackgroundUtilization));
        l.Update(advance(1 * time.ΔSecond));
        if (l.Fill() != l.Capacity()) {
            Ꮡt.Errorf("expected bucket filled to capacity %d, got %d"u8, l.Capacity(), l.Fill());
        }
        if (!l.Limiting()) {
            Ꮡt.Errorf("limiter is not enabled after filling bucket but should be"u8);
        }
        if (l.Overflow() != 0 + baseOverflow) {
            Ꮡt.Errorf("bucket filled exactly should not have overflow, found %d"u8, l.Overflow());
        }
        if (Ꮡt.Failed()) {
            Ꮡt.FailNow();
        }
        // Drain the bucket by half.
        l.AddAssistTime(assistTime(runtime_internal_test_package.CapacityPerProc, 0D));
        l.Update(advance(runtime_internal_test_package.CapacityPerProc));
        {
            var expect = l.Capacity() / 2; if (l.Fill() != expect) {
                Ꮡt.Errorf("failed to drain to %d, got fill %d"u8, expect, l.Fill());
            }
        }
        if (l.Limiting()) {
            Ꮡt.Errorf("limiter is enabled after draining bucket but shouldn't be"u8);
        }
        if (Ꮡt.Failed()) {
            Ꮡt.FailNow();
        }
        // Test overfilling the bucket.
        l.AddAssistTime(assistTime(runtime_internal_test_package.CapacityPerProc, 1.0D - runtime_internal_test_package.GCBackgroundUtilization));
        l.Update(advance(runtime_internal_test_package.CapacityPerProc));
        if (l.Fill() != l.Capacity()) {
            Ꮡt.Errorf("failed to fill to capacity %d, got fill %d"u8, l.Capacity(), l.Fill());
        }
        if (!l.Limiting()) {
            Ꮡt.Errorf("limiter is not enabled after overfill but should be"u8);
        }
        {
            var expect = (uint64)(/* CapacityPerProc * procs / 2 */ 7000000000UL); if (l.Overflow() != expect + baseOverflow) {
                Ꮡt.Errorf("bucket overfilled should have overflow %d, found %d"u8, expect, l.Overflow());
            }
        }
        if (Ꮡt.Failed()) {
            Ꮡt.FailNow();
        }
        // Test ending the cycle with some assists left over.
        l.AddAssistTime(assistTime(1 * time.Millisecond, 1.0D - runtime_internal_test_package.GCBackgroundUtilization));
        l.StartGCTransition(false, advance(1 * time.Millisecond));
        if (l.Fill() != l.Capacity()) {
            Ꮡt.Errorf("failed to maintain fill to capacity %d, got fill %d"u8, l.Capacity(), l.Fill());
        }
        if (!l.Limiting()) {
            Ꮡt.Errorf("limiter is not enabled after overfill but should be"u8);
        }
        {
            var expect = (uint64)(int64)((time.Duration)(7014000000L)); if (l.Overflow() != expect + baseOverflow) {
                Ꮡt.Errorf("bucket overfilled should have overflow %d, found %d"u8, expect, l.Overflow());
            }
        }
        if (Ꮡt.Failed()) {
            Ꮡt.FailNow();
        }
        // Make sure the STW adds to the bucket.
        l.FinishGCTransition(advance(5 * time.Millisecond));
        if (l.Fill() != l.Capacity()) {
            Ꮡt.Errorf("failed to maintain fill to capacity %d, got fill %d"u8, l.Capacity(), l.Fill());
        }
        if (!l.Limiting()) {
            Ꮡt.Errorf("limiter is not enabled after overfill but should be"u8);
        }
        {
            var expect = (uint64)(int64)((time.Duration)(7084000000L)); if (l.Overflow() != expect + baseOverflow) {
                Ꮡt.Errorf("bucket overfilled should have overflow %d, found %d"u8, expect, l.Overflow());
            }
        }
        if (Ꮡt.Failed()) {
            Ꮡt.FailNow();
        }
        // Resize procs up and make sure limiting stops.
        var expectFill = l.Capacity();
        l.ResetCapacity(advance(0), procs + 10);
        if (l.Fill() != expectFill) {
            Ꮡt.Errorf("failed to maintain fill at old capacity %d, got fill %d"u8, expectFill, l.Fill());
        }
        if (l.Limiting()) {
            Ꮡt.Errorf("limiter is enabled after resetting capacity higher"u8);
        }
        {
            var expect = (uint64)(int64)((time.Duration)(7084000000L)); if (l.Overflow() != expect + baseOverflow) {
                Ꮡt.Errorf("bucket overflow %d should have remained constant, found %d"u8, expect, l.Overflow());
            }
        }
        if (Ꮡt.Failed()) {
            Ꮡt.FailNow();
        }
        // Resize procs down and make sure limiting begins again.
        // Also make sure resizing doesn't affect overflow. This isn't
        // a case where we want to report overflow, because we're not
        // actively doing work to achieve it. It's that we have fewer
        // CPU resources now.
        l.ResetCapacity(advance(0), procs - 10);
        if (l.Fill() != l.Capacity()) {
            Ꮡt.Errorf("failed lower fill to new capacity %d, got fill %d"u8, l.Capacity(), l.Fill());
        }
        if (!l.Limiting()) {
            Ꮡt.Errorf("limiter is disabled after resetting capacity lower"u8);
        }
        {
            var expect = (uint64)(int64)((time.Duration)(7084000000L)); if (l.Overflow() != expect + baseOverflow) {
                Ꮡt.Errorf("bucket overflow %d should have remained constant, found %d"u8, expect, l.Overflow());
            }
        }
        if (Ꮡt.Failed()) {
            Ꮡt.FailNow();
        }
        // Get back to a zero state. The top of the loop will double check.
        l.ResetCapacity(advance((time.Duration)(14000000000L)), procs);
        // Track total overflow for future iterations.
        baseOverflow += (uint64)(int64)((time.Duration)(7084000000L));
    }
}

} // end runtime_test_package
