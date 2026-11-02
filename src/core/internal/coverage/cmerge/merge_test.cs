// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.coverage;

using fmt = fmt_package;
using coverage = go.@internal.coverage_package;
using cmerge = go.@internal.coverage.cmerge_package;
using testing = testing_package;
using go.@internal;
using go.@internal.coverage;

partial class cmerge_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string mdf1Dataˢ => ref mdf1Dataˢᵇ;
private static readonly @string mdf1Dataˢᵇ = "mdf1.data"u8;

public static void TestClash(ж<testing.T> t) {
    var m = Ꮡ(new cmerge.Merger());
    var err = m.SetModeAndGranularity(mdf1Dataˢ, coverage.CtrModeSet, coverage.CtrGranularityPerBlock);
    if (err != nil) {
        t.Fatalf("unexpected clash: %v", err);
    }
    err = m.SetModeAndGranularity(mdf1Dataˢ, coverage.CtrModeSet, coverage.CtrGranularityPerBlock);
    if (err != nil) {
        t.Fatalf("unexpected clash: %v", err);
    }
    err = m.SetModeAndGranularity(mdf1Dataˢ, coverage.CtrModeCount, coverage.CtrGranularityPerBlock);
    if (err == nil) {
        t.Fatalf("expected mode clash, not found");
    }
    err = m.SetModeAndGranularity(mdf1Dataˢ, coverage.CtrModeSet, coverage.CtrGranularityPerFunc);
    if (err == nil) {
        t.Fatalf("expected granularity clash, not found");
    }
    m.SetModeMergePolicy(cmerge.ModeMergeRelaxed);
    err = m.SetModeAndGranularity(mdf1Dataˢ, coverage.CtrModeCount, coverage.CtrGranularityPerBlock);
    if (err != nil) {
        t.Fatalf("unexpected clash: %v", err);
    }
    err = m.SetModeAndGranularity(mdf1Dataˢ, coverage.CtrModeSet, coverage.CtrGranularityPerBlock);
    if (err != nil) {
        t.Fatalf("unexpected clash: %v", err);
    }
    err = m.SetModeAndGranularity(mdf1Dataˢ, coverage.CtrModeAtomic, coverage.CtrGranularityPerBlock);
    if (err != nil) {
        t.Fatalf("unexpected clash: %v", err);
    }
    m.ResetModeAndGranularity();
    err = m.SetModeAndGranularity(mdf1Dataˢ, coverage.CtrModeCount, coverage.CtrGranularityPerFunc);
    if (err != nil) {
        t.Fatalf("unexpected clash after reset: %v", err);
    }
}

internal partial struct TestBasic_scenarios /*dyn*/ {
    internal coverage.CounterMode cmode;
    internal coverage.CounterGranularity cgran;
    internal slice<uint32> src, dst, res;
    internal nint iters;
    internal bool merr;
    internal bool overflow;
}

public static void TestBasic(ж<testing.T> t) {
    var scenarios = slice<TestBasic_scenarios>([
        new(
            cmode: coverage.CtrModeSet,
            cgran: coverage.CtrGranularityPerBlock,
            src: slice<uint32>([1, 0, 1]),
            dst: slice<uint32>([1, 1, 0]),
            res: slice<uint32>([1, 1, 1]),
            iters: 2,
            overflow: false
        ),
        new(
            cmode: coverage.CtrModeCount,
            cgran: coverage.CtrGranularityPerBlock,
            src: slice<uint32>([1, 0, 3]),
            dst: slice<uint32>([5, 7, 0]),
            res: slice<uint32>([6, 7, 3]),
            iters: 1,
            overflow: false
        ),
        new(
            cmode: coverage.CtrModeCount,
            cgran: coverage.CtrGranularityPerBlock,
            src: slice<uint32>([4294967200U, 0, 3]),
            dst: slice<uint32>([4294967001U, 7, 0]),
            res: slice<uint32>([4294967295U, 7, 3]),
            iters: 1,
            overflow: true
        )
    ]);

    foreach (var (k, scenario) in scenarios) {
        error err = default;
        bool ovf = default;
        var m = Ꮡ(new cmerge.Merger());
        @string mdf = fmt.Sprintf("file%d"u8, k);
        err = m.SetModeAndGranularity(mdf, scenario.cmode, scenario.cgran);
        if (err != nil) {
            t.Fatalf("case %d SetModeAndGranularity failed: %v", k, err);
        }
        for (nint i = 0; i < scenario.iters; i++) {
            (err, ovf) = m.MergeCounters(scenario.dst, scenario.src);
            if (ovf != scenario.overflow) {
                t.Fatalf("case %d overflow mismatch: got %v want %v", k, ovf, scenario.overflow);
            }
            if (!scenario.merr && err != nil) {
                t.Fatalf("case %d unexpected err %v", k, err);
            }
            if (scenario.merr && err == nil) {
                t.Fatalf("case %d expected err, not received", k);
            }
            foreach (var iΔ1 in range(scenario.dst)) {
                if (scenario.dst[iΔ1] != scenario.res[iΔ1]) {
                    t.Fatalf("case %d: bad merge at %d got %d want %d",
                        k, iΔ1, scenario.dst[iΔ1], scenario.res[iΔ1]);
                }
            }
        }
    }
}

} // end cmerge_test_package
