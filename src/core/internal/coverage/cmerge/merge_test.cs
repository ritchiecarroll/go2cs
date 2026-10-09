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
private static readonly @string mdf1Dataˢ = "mdf1.data"u8;

public static void TestClash(ж<testing.T> Ꮡt) {
    var m = Ꮡ(new cmerge.Merger(nil));
    var err = m.SetModeAndGranularity(mdf1Dataˢ, coverage.CtrModeSet, coverage.CtrGranularityPerBlock);
    if (err != default!) {
        Ꮡt.Fatalf("unexpected clash: %v"u8, err);
    }
    err = m.SetModeAndGranularity(mdf1Dataˢ, coverage.CtrModeSet, coverage.CtrGranularityPerBlock);
    if (err != default!) {
        Ꮡt.Fatalf("unexpected clash: %v"u8, err);
    }
    err = m.SetModeAndGranularity(mdf1Dataˢ, coverage.CtrModeCount, coverage.CtrGranularityPerBlock);
    if (err == default!) {
        Ꮡt.Fatalf("expected mode clash, not found"u8);
    }
    err = m.SetModeAndGranularity(mdf1Dataˢ, coverage.CtrModeSet, coverage.CtrGranularityPerFunc);
    if (err == default!) {
        Ꮡt.Fatalf("expected granularity clash, not found"u8);
    }
    m.SetModeMergePolicy(cmerge.ModeMergeRelaxed);
    err = m.SetModeAndGranularity(mdf1Dataˢ, coverage.CtrModeCount, coverage.CtrGranularityPerBlock);
    if (err != default!) {
        Ꮡt.Fatalf("unexpected clash: %v"u8, err);
    }
    err = m.SetModeAndGranularity(mdf1Dataˢ, coverage.CtrModeSet, coverage.CtrGranularityPerBlock);
    if (err != default!) {
        Ꮡt.Fatalf("unexpected clash: %v"u8, err);
    }
    err = m.SetModeAndGranularity(mdf1Dataˢ, coverage.CtrModeAtomic, coverage.CtrGranularityPerBlock);
    if (err != default!) {
        Ꮡt.Fatalf("unexpected clash: %v"u8, err);
    }
    m.ResetModeAndGranularity();
    err = m.SetModeAndGranularity(mdf1Dataˢ, coverage.CtrModeCount, coverage.CtrGranularityPerFunc);
    if (err != default!) {
        Ꮡt.Fatalf("unexpected clash after reset: %v"u8, err);
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

public static void TestBasic(ж<testing.T> Ꮡt) {
    var scenarios = new TestBasic_scenarios[]{
        new(
            cmode: coverage.CtrModeSet,
            cgran: coverage.CtrGranularityPerBlock,
            src: new uint32[]{1, 0, 1}.slice(),
            dst: new uint32[]{1, 1, 0}.slice(),
            res: new uint32[]{1, 1, 1}.slice(),
            iters: 2,
            overflow: false
        ),
        new(
            cmode: coverage.CtrModeCount,
            cgran: coverage.CtrGranularityPerBlock,
            src: new uint32[]{1, 0, 3}.slice(),
            dst: new uint32[]{5, 7, 0}.slice(),
            res: new uint32[]{6, 7, 3}.slice(),
            iters: 1,
            overflow: false
        ),
        new(
            cmode: coverage.CtrModeCount,
            cgran: coverage.CtrGranularityPerBlock,
            src: new uint32[]{4294967200U, 0, 3}.slice(),
            dst: new uint32[]{4294967001U, 7, 0}.slice(),
            res: new uint32[]{4294967295U, 7, 3}.slice(),
            iters: 1,
            overflow: true
        )
    }.slice();
    foreach (var (k, scenario) in scenarios) {
        error err = default!;
        bool ovf = default!;
        var m = Ꮡ(new cmerge.Merger(nil));
        @string mdf = fmt.Sprintf("file%d"u8, k);
        err = m.SetModeAndGranularity(mdf, scenario.cmode, scenario.cgran);
        if (err != default!) {
            Ꮡt.Fatalf("case %d SetModeAndGranularity failed: %v"u8, k, err);
        }
        for (nint i = 0; i < scenario.iters; i++) {
            (err, ovf) = m.MergeCounters(scenario.dst, scenario.src);
            if (ovf != scenario.overflow) {
                Ꮡt.Fatalf("case %d overflow mismatch: got %v want %v"u8, k, ovf, scenario.overflow);
            }
            if (!scenario.merr && err != default!) {
                Ꮡt.Fatalf("case %d unexpected err %v"u8, k, err);
            }
            if (scenario.merr && err == default!) {
                Ꮡt.Fatalf("case %d expected err, not received"u8, k);
            }
            foreach (var (iΔ1, _) in scenario.dst) {
                if (scenario.dst[iΔ1] != scenario.res[iΔ1]) {
                    Ꮡt.Fatalf("case %d: bad merge at %d got %d want %d"u8,
                        k, iΔ1, scenario.dst[iΔ1], scenario.res[iΔ1]);
                }
            }
        }
    }
}

} // end cmerge_test_package
