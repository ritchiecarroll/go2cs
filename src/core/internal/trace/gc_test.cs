// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using Δtrace = go.@internal.trace_package;
using testtrace = go.@internal.trace.testtrace_package;
using io = io_package;
using math = math_package;
using testing = testing_package;
using time = time_package;
using go.@internal;
using go.@internal.trace;
using static go.@internal.trace_internal_test_package;
using version = go.@internal.trace.version_package;

partial class trace_test_package {

// aeq returns true if x and y are equal up to 8 digits (1 part in 100
// million).
internal static bool aeq(float64 x, float64 y) {
    if (x < 0D && y < 0D) {
        (x, y) = (-x, -y);
    }
    UntypedInt digits = 8;
    var factor = 1D - math.Pow(10D, /* -digits + 1 */ -7D);
    return x * factor <= y && y * factor <= x;
}

internal partial struct TestMMU_type /*dyn*/ {
    internal time.Duration window;
    internal float64 want;
    internal slice<float64> worst;
}

public static void TestMMU(ж<testing.T> t) {
    t.Parallel();

    // MU
    // 1.0  *****   *****   *****
    // 0.5      *   *   *   *
    // 0.0      *****   *****
    //      0   1   2   3   4   5
    var util = slice<slice<Δtrace.MutatorUtil>>([slice<Δtrace.MutatorUtil>([
        new(0, 1D),
        new(1000000000, 0D),
        new(2000000000, 1D),
        new(3000000000, 0D),
        new(4000000000, 1D),
        new(5000000000, 0D)])
    ]);
    var mmuCurve = Δtrace.NewMMUCurve(util);

    foreach (var (_, test) in slice<TestMMU_type>([
        new(0, 0D, slice<float64>([])),
        new(time.Millisecond, 0D, slice<float64>([0D, 0D])),
        new(time.ΔSecond, 0D, slice<float64>([0D, 0D])),
        new(2 * time.ΔSecond, 0.5D, slice<float64>([0.5D, 0.5D])),
        new((time.Duration)(3000000000L), 1D / 3.0D, slice<float64>([1D / 3.0D])),
        new((time.Duration)(4000000000L), 0.5D, slice<float64>([0.5D])),
        new((time.Duration)(5000000000L), 3D / 5.0D, slice<float64>([3D / 5.0D])),
        new((time.Duration)(6000000000L), 3D / 5.0D, slice<float64>([3D / 5.0D]))
    ])) {
        {
            var got = mmuCurve.MMU(test.window); if (!aeq(test.want, got)) {
                t.Errorf("for %s window, want mu = %f, got %f", test.window, test.want, got);
            }
        }
        var worst = mmuCurve.Examples(test.window, 2);
        // Which exact windows are returned is unspecified
        // (and depends on the exact banding), so we just
        // check that we got the right number with the right
        // utilizations.
        if (len(worst) != len(test.worst)) {
            t.Errorf("for %s window, want worst %v, got %v", test.window, test.worst, worst);
        } else {
            foreach (var i in range(worst)) {
                if (worst[i].MutatorUtil != test.worst[i]) {
                    t.Errorf("for %s window, want worst %v, got %v", test.window, test.worst, worst);
                    break;
                }
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingInShortModeˢ = (@string)"skipping in -short mode"u8;
internal static ref readonly @string testdataTestsGo122Gcˢ => ref testdataTestsGo122Gcˢᵇ;
private static readonly @string testdataTestsGo122Gcˢᵇ = "testdata/tests/go122-gc-stress.test"u8;

public static void TestMMUTrace(ж<testing.T> t) {
    // Can't be t.Parallel() because it modifies the
    // testingOneBand package variable.
    if (testing.Short()) {
        // test input too big for all.bash
        t.Skip(skippingInShortModeˢ);
    }
    void check(ж<testing.T> tΔ1, slice<slice<Δtrace.MutatorUtil>> mu) {
        GoFrame ᒐ = default;
        try {
            var mmuCurve = Δtrace.NewMMUCurve(mu);

            // Test the optimized implementation against the "obviously
            // correct" implementation.
            for (var window = time.ΔNanosecond; window < (time.Duration)(10000000000L); window *= 10) {
                var want = mmuSlow(mu[0], window);
                var got = mmuCurve.MMU(window);
                if (!aeq(want, got)) {
                    tΔ1.Errorf("want %f, got %f mutator utilization in window %s", want, got, window);
                }
            }

            // Test MUD with band optimization against MUD without band
            // optimization. We don't have a simple testing implementation
            // of MUDs (the simplest implementation is still quite
            // complex), but this is still a pretty good test.
            defer((nint old) => {
                trace_internal_test_package.BandsPerSeries = old;
            }, trace_internal_test_package.BandsPerSeries, ref ᒐ);
            trace_internal_test_package.BandsPerSeries = 1;
            var mmuCurve2 = Δtrace.NewMMUCurve(mu);
            var quantiles = slice<float64>([0D, /* 1 - .999 */ 0.001D, /* 1 - .99 */ 0.01D]);
            for (var window = time.Microsecond; window < time.ΔSecond; window *= 10) {
                var mud1 = mmuCurve.MUD(window, quantiles);
                var mud2 = mmuCurve2.MUD(window, quantiles);
                foreach (var i in range(mud1)) {
                    if (!aeq(mud1[i], mud2[i])) {
                        tΔ1.Errorf("for quantiles %v at window %v, want %v, got %v", quantiles, window, mud2, mud1);
                        break;
                    }
                }
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }
    var checkʗ1 = check;
    t.Run("V2", (ж<testing.T> tΔ2) => {
        @string testPath = testdataTestsGo122Gcˢ;
        var (r, _, _, err) = testtrace.ParseFile(testPath);
        if (err != nil) {
            tΔ2.Fatalf("malformed test %s: bad trace file: %v", testPath, err);
        }
        slice<traceꓸEvent> events = default;
        (var tr, err) = Δtrace.NewReader(r);
        if (err != nil) {
            tΔ2.Fatalf("malformed test %s: bad trace file: %v", testPath, err);
        }
        for (;;) {
            var (ev, errΔ1) = tr.ReadEvent();
            if (AreEqual(errΔ1, io.EOF)) {
                break;
            }
            if (errΔ1 != nil) {
                tΔ2.Fatalf("malformed test %s: bad trace file: %v", testPath, errΔ1);
            }
            events = append(events, ev.ΔClone());
        }
        // Pass the trace through MutatorUtilizationV2 and check it.
        checkʗ1(tΔ2, Δtrace.MutatorUtilizationV2(events, (Δtrace.UtilFlags)((Δtrace.UtilFlags)(Δtrace.UtilSTW | Δtrace.UtilBackground) | Δtrace.UtilAssist)));
    });
}

internal static float64 /*mmu*/ mmuSlow(slice<Δtrace.MutatorUtil> util, time.Duration window) {
    float64 mmu = default;

    {
        var max = (time.Duration)(util[len(util) - 1].Time - util[0].Time); if (window > max) {
            window = max;
        }
    }

    mmu = 1.0D;

    // muInWindow returns the mean mutator utilization between
    // util[0].Time and end.
    float64 muInWindow(slice<Δtrace.MutatorUtil> utilΔ1, int64 end) {
        var total = 0.0D;
        Δtrace.MutatorUtil prevU = default;
        foreach (var (_, u) in utilΔ1) {
            if (u.Time > end) {
                total += prevU.Util * (float64)(end - prevU.Time);
                break;
            }
            total += prevU.Util * (float64)(u.Time - prevU.Time);
            prevU = u;
        }
        return total / (float64)(end - utilΔ1[0].Time);
    }
    var muInWindowʗ1 = muInWindow;
    void update() {
        foreach (var (i, u) in util) {
            if (u.Time + (int64)window > util[len(util) - 1].Time) {
                break;
            }
            mmu = math.Min(mmu, muInWindowʗ1(util.slice(i), u.Time + (int64)window));
        }
    }

    // Consider all left-aligned windows.
    update();
    // Reverse the trace. Slightly subtle because each MutatorUtil
    // is a *change*.
    var rutil = new slice<Δtrace.MutatorUtil>(len(util));
    if (util[len(util) - 1].Util != 0D) {
        throw panic("irreversible trace");
    }
    foreach (var (i, u) in util) {
        var util1 = 0.0D;
        if (i != 0) {
            util1 = util[i - 1].Util;
        }
        rutil[len(rutil) - i - 1] = new Δtrace.MutatorUtil(Time: -u.Time, Util: util1);
    }
    util = rutil;
    // Consider all right-aligned windows.
    update();
    return mmu;
}

} // end trace_test_package
