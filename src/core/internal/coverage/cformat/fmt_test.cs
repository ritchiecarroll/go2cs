// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.coverage;

using coverage = go.@internal.coverage_package;
using cformat = go.@internal.coverage.cformat_package;
using slices = slices_package;
using strings = strings_package;
using testing = testing_package;
using go.@internal;
using go.@internal.coverage;
using io = io_package;

partial class cformat_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string myPack1ˢ = "my/pack1"u8;
private static readonly @string pGoˢ = "p.go"u8;
private static readonly @string qGoˢ = "q.go"u8;
private static readonly @string myPack2ˢ = "my/pack2"u8;
private static readonly @string litGoˢ = "lit.go"u8;
private static readonly @string modeAtomicPGo10011020PGoˢ = """

mode: atomic
p.go:10.0,11.0 2 0
p.go:15.0,11.0 1 1
q.go:20.0,25.0 3 0
q.go:30.0,31.0 2 1
q.go:33.0,40.0 7 2
lit.go:99.0,100.0 1 0
"""u8.Raw();
private static readonly @string modeAtomicLitGo990100010ˢ = """

mode: atomic
lit.go:99.0,100.0 1 0
"""u8.Raw();
private static readonly @string myPack1Coverage667Ofˢ = """

       	my/pack1		coverage: 66.7% of statements
        my/pack2		coverage: 0.0% of statements

"""u8.Raw();
private static readonly @string coverage625OfStatementsˢ = """

		coverage: 62.5% of statements in ./...

"""u8.Raw();
private static readonly @string pGo10F1333QGo20F2750ˢ = """

p.go:10:	f1		33.3%
q.go:20:	f2		75.0%
total		(statements)	62.5%
"""u8.Raw();
private static readonly @string myPack1Coverage667Ofˢ2 = """

       	my/pack1		coverage: 66.7% of statements

"""u8.Raw();

public static void TestBasics(ж<testing.T> Ꮡt) {
    var fm = cformat.NewFormatter(coverage.CtrModeAtomic);

    coverage.CoverableUnit mku(uint32 stl, uint32 enl, uint32 nx) => new coverage.CoverableUnit(
            StLine: stl,
            EnLine: enl,
            NxStmts: nx
        );
    var fn1units = new coverage.CoverableUnit[]{
        mku(10, 11, 2),
        mku(15, 11, 1)
    }.slice();
    var fn2units = new coverage.CoverableUnit[]{
        mku(20, 25, 3),
        mku(30, 31, 2),
        mku(33, 40, 7)
    }.slice();
    var fn3units = new coverage.CoverableUnit[]{
        mku(99, 100, 1)
    }.slice();
    fm.SetPackage(myPack1ˢ);
    foreach (var (k, u) in fn1units) {
        fm.AddUnit(pGoˢ, "f1"u8, false, u, (uint32)k);
    }
    foreach (var (k, u) in fn2units) {
        fm.AddUnit(qGoˢ, "f2"u8, false, u, 0);
        fm.AddUnit(qGoˢ, "f2"u8, false, u, (uint32)k);
    }
    fm.SetPackage(myPack2ˢ);
    foreach (var (_, u) in fn3units) {
        fm.AddUnit(litGoˢ, "f3"u8, true, u, 0);
    }

    ref var b1 = ref heap(new strings.Builder(), out var Ꮡb1);
    ref var b2 = ref heap(new strings.Builder(), out var Ꮡb2);
    ref var b3 = ref heap(new strings.Builder(), out var Ꮡb3);
    ref var b4 = ref heap(new strings.Builder(), out var Ꮡb4);
    ref var b5 = ref heap(new strings.Builder(), out var Ꮡb5);
    {
        var err = fm.EmitTextual(default!, new strings_BuilderжWriter(Ꮡb1)); if (err != default!) {
            Ꮡt.Fatalf("EmitTextual returned %v"u8, err);
        }
    }
    @string wantText = strings.TrimSpace(modeAtomicPGo10011020PGoˢ);
    @string gotText = strings.TrimSpace(b1.String());
    if (wantText != gotText) {
        Ꮡt.Errorf("emit text: got:\n%s\nwant:\n%s\n"u8, gotText, wantText);
    }

    var selected = new @string[]{"my/pack2"u8}.slice();
    {
        var err = fm.EmitTextual(selected, new strings_BuilderжWriter(Ꮡb5)); if (err != default!) {
            Ꮡt.Fatalf("EmitTextual returned %v"u8, err);
        }
    }
    wantText = strings.TrimSpace(modeAtomicLitGo990100010ˢ);
    gotText = strings.TrimSpace(b5.String());
    if (wantText != gotText) {
        Ꮡt.Errorf("emit text: got:\n%s\nwant:\n%s\n"u8, gotText, wantText);
    }

    // Percent output with no aggregation.
    @string noCoverPkg = ""u8;
    {
        var err = fm.EmitPercent(new strings_BuilderжWriter(Ꮡb2), default!, noCoverPkg, false, false); if (err != default!) {
            Ꮡt.Fatalf("EmitPercent returned %v"u8, err);
        }
    }
    var wantPercent = strings.Fields(myPack1Coverage667Ofˢ);
    var gotPercent = strings.Fields(b2.String());
    if (!slices.Equal<slice<@string>, @string>(wantPercent, gotPercent)) {
        Ꮡt.Errorf("emit percent: got:\n%+v\nwant:\n%+v\n"u8,
            gotPercent, wantPercent);
    }

    // Percent mode with aggregation.
    @string withCoverPkg = " in ./..."u8;
    {
        var err = fm.EmitPercent(new strings_BuilderжWriter(Ꮡb3), default!, withCoverPkg, false, true); if (err != default!) {
            Ꮡt.Fatalf("EmitPercent returned %v"u8, err);
        }
    }
    wantPercent = strings.Fields(coverage625OfStatementsˢ);
    gotPercent = strings.Fields(b3.String());
    if (!slices.Equal<slice<@string>, @string>(wantPercent, gotPercent)) {
        Ꮡt.Errorf("emit percent: got:\n%+v\nwant:\n%+v\n"u8,
            gotPercent, wantPercent);
    }

    {
        var err = fm.EmitFuncs(new strings_BuilderжWriter(Ꮡb4)); if (err != default!) {
            Ꮡt.Fatalf("EmitFuncs returned %v"u8, err);
        }
    }
    @string wantFuncs = strings.TrimSpace(pGo10F1333QGo20F2750ˢ);
    @string gotFuncs = strings.TrimSpace(b4.String());
    if (wantFuncs != gotFuncs) {
        Ꮡt.Errorf("emit funcs: got:\n%s\nwant:\n%s\n"u8, gotFuncs, wantFuncs);
    }
    if (false) {
        Ꮡt.Logf("text is %s\n"u8, b1.String());
        Ꮡt.Logf("perc is %s\n"u8, b2.String());
        Ꮡt.Logf("perc2 is %s\n"u8, b3.String());
        Ꮡt.Logf("funcs is %s\n"u8, b4.String());
    }

    // Percent output with specific packages selected.
    {
        ref var b = ref heap(new strings.Builder(), out var Ꮡb);
        var selpkgs = new @string[]{"foo/bar"u8, "my/pack1"u8}.slice();
        {
            var err = fm.EmitPercent(new strings_BuilderжWriter(Ꮡb), selpkgs, noCoverPkg, false, false); if (err != default!) {
                Ꮡt.Fatalf("EmitPercent returned %v"u8, err);
            }
        }
        var wantPercentΔ1 = strings.Fields(myPack1Coverage667Ofˢ2);
        var gotPercentΔ1 = strings.Fields(b.String());
        if (!slices.Equal<slice<@string>, @string>(wantPercentΔ1, gotPercentΔ1)) {
            Ꮡt.Errorf("emit percent: got:\n%+v\nwant:\n%+v\n"u8,
                gotPercentΔ1, wantPercentΔ1);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string myPack1CoverageNoˢ = """

       	my/pack1 coverage:	[no statements]
        my/pack2 coverage:	[no statements]

"""u8.Raw();
private static readonly @string coverageNoStatementsˢ = """

       	coverage:	[no statements]

"""u8.Raw();

public static void TestEmptyPackages(ж<testing.T> Ꮡt) {
    var fm = cformat.NewFormatter(coverage.CtrModeAtomic);
    fm.SetPackage(myPack1ˢ);
    fm.SetPackage(myPack2ˢ);

    // No aggregation.
    {
        ref var b = ref heap(new strings.Builder(), out var Ꮡb);
        @string noCoverPkg = ""u8;
        {
            var err = fm.EmitPercent(new strings_BuilderжWriter(Ꮡb), default!, noCoverPkg, true, false); if (err != default!) {
                Ꮡt.Fatalf("EmitPercent returned %v"u8, err);
            }
        }
        var wantPercent = strings.Fields(myPack1CoverageNoˢ);
        var gotPercent = strings.Fields(b.String());
        if (!slices.Equal<slice<@string>, @string>(wantPercent, gotPercent)) {
            Ꮡt.Errorf("emit percent: got:\n%+v\nwant:\n%+v\n"u8,
                gotPercent, wantPercent);
        }
    }

    // With aggregation.
    {
        ref var b = ref heap(new strings.Builder(), out var Ꮡb);
        @string noCoverPkg = ""u8;
        {
            var err = fm.EmitPercent(new strings_BuilderжWriter(Ꮡb), default!, noCoverPkg, true, true); if (err != default!) {
                Ꮡt.Fatalf("EmitPercent returned %v"u8, err);
            }
        }
        var wantPercent = strings.Fields(coverageNoStatementsˢ);
        var gotPercent = strings.Fields(b.String());
        if (!slices.Equal<slice<@string>, @string>(wantPercent, gotPercent)) {
            Ꮡt.Errorf("emit percent: got:\n%+v\nwant:\n%+v\n"u8,
                gotPercent, wantPercent);
        }
    }
}

} // end cformat_test_package
