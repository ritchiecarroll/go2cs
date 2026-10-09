// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.math;

using fmt = fmt_package;
using math = math_package;
using Δrand = go.math.rand_package;
using testing = testing_package;
using go.math;
using static go.math.big_package;

partial class big_internal_test_package {

// TestFloatSqrt64 tests that Float.Sqrt of numbers with 53bit mantissa
// behaves like float math.Sqrt.
public static void TestFloatSqrt64(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < 100000; i++) {
        if (i == 100 && testing.Short()) {
            break;
        }
        var r = Δrand.Float64();
        var got = @new<global::go.math.big_package.Float>().SetPrec(53);
        got.Sqrt(NewFloat(r));
        var want = NewFloat(math.Sqrt(r));
        if (got.Cmp(want) != 0) {
            Ꮡt.Fatalf("Sqrt(%g) =\n got %g;\nwant %g"u8, r, got.OrTypedNil(), want.OrTypedNil());
        }
    }
}

internal partial struct TestFloatSqrt_type /*dyn*/ {
    internal @string x;
    internal @string want;
}

public static void TestFloatSqrt(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in new TestFloatSqrt_type[]{ // Test values were generated on Wolfram Alpha using query
 //   'sqrt(N) to 350 digits'
 // 350 decimal digits give up to 1000 binary digits.

        new("0.03125"u8, "0.17677669529663688110021109052621225982120898442211850914708496724884155980776337985629844179095519659187673077886403712811560450698134215158051518713749197892665283324093819909447499381264409775757143376369499645074628431682460775184106467733011114982619404115381053858929018135497032545349940642599871090667456829147610370507757690729404938184321879"u8),
        new("0.125"u8, "0.35355339059327376220042218105242451964241796884423701829416993449768311961552675971259688358191039318375346155772807425623120901396268430316103037427498395785330566648187639818894998762528819551514286752738999290149256863364921550368212935466022229965238808230762107717858036270994065090699881285199742181334913658295220741015515381458809876368643757"u8),
        new("0.5"u8, "0.70710678118654752440084436210484903928483593768847403658833986899536623923105351942519376716382078636750692311545614851246241802792536860632206074854996791570661133296375279637789997525057639103028573505477998580298513726729843100736425870932044459930477616461524215435716072541988130181399762570399484362669827316590441482031030762917619752737287514"u8),
        new("2.0"u8, "1.4142135623730950488016887242096980785696718753769480731766797379907324784621070388503875343276415727350138462309122970249248360558507372126441214970999358314132226659275055927557999505011527820605714701095599716059702745345968620147285174186408891986095523292304843087143214508397626036279952514079896872533965463318088296406206152583523950547457503"u8),
        new("3.0"u8, "1.7320508075688772935274463415058723669428052538103806280558069794519330169088000370811461867572485756756261414154067030299699450949989524788116555120943736485280932319023055820679748201010846749232650153123432669033228866506722546689218379712270471316603678615880190499865373798593894676503475065760507566183481296061009476021871903250831458295239598"u8),
        new("4.0"u8, "2.0"u8),
        new("1p512"u8, "1p256"u8),
        new("4p1024"u8, "2p512"u8),
        new("9p2048"u8, "3p1024"u8),
        new("1p-1024"u8, "1p-512"u8),
        new("4p-2048"u8, "2p-1024"u8),
        new("9p-4096"u8, "3p-2048"u8)
    }.slice()) {
        foreach (var (_, prec) in new nuint[]{24, 53, 64, 65, 100, 128, 129, 200, 256, 400, 600, 800, 1000}.slice()) {
            var x = @new<global::go.math.big_package.Float>().SetPrec(prec);
            x.Parse(test.x, 10);
            var got = @new<global::go.math.big_package.Float>().SetPrec(prec).Sqrt(x);
            var want = @new<global::go.math.big_package.Float>().SetPrec(prec);
            want.Parse(test.want, 10);
            if (got.Cmp(want) != 0) {
                Ꮡt.Errorf("prec = %d, Sqrt(%v) =\ngot  %g;\nwant %g"u8,
                    prec, test.x, got.OrTypedNil(), want.OrTypedNil());
            }
            // Square test.
            // If got holds the square root of x to precision p, then
            //   got = √x + k
            // for some k such that |k| < 2**(-p). Thus,
            //   got² = (√x + k)² = x + 2k√n + k²
            // and the error must satisfy
            //   err = |got² - x| ≈ | 2k√n | < 2**(-p+1)*√n
            // Ignoring the k² term for simplicity.
            // err = |got² - x|
            // (but do intermediate steps with 32 guard digits to
            // avoid introducing spurious rounding-related errors)
            var sq = @new<global::go.math.big_package.Float>().SetPrec(prec + 32).Mul(got, got);
            var diff = @new<global::go.math.big_package.Float>().Sub(sq, x);
            var err = diff.Abs(diff).SetPrec(prec);
            // maxErr = 2**(-p+1)*√x
            var one = @new<global::go.math.big_package.Float>().SetPrec(prec).SetInt64(1);
            var maxErr = @new<global::go.math.big_package.Float>().Mul(@new<global::go.math.big_package.Float>().SetMantExp(one, -(nint)prec + 1), got);
            if (err.Cmp(maxErr) >= 0) {
                Ꮡt.Errorf("prec = %d, Sqrt(%v) =\ngot err  %g;\nwant maxErr %g"u8,
                    prec, test.x, err.OrTypedNil(), maxErr.OrTypedNil());
            }
        }
    }
}

internal partial struct TestFloatSqrtSpecial_type /*dyn*/ {
    internal ж<global::go.math.big_package.Float> x;
    internal ж<global::go.math.big_package.Float> want;
}

public static void TestFloatSqrtSpecial(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in new TestFloatSqrtSpecial_type[]{
        new(NewFloat(+0D), NewFloat(+0D)),
        new(NewFloat(-0D), NewFloat(-0D)),
        new(NewFloat(math.Inf(+1)), NewFloat(math.Inf(+1)))
    }.slice()) {
        var got = @new<global::go.math.big_package.Float>().Sqrt(test.x);
        if ((~got).neg != (~test.want).neg || (~got).form != (~test.want).form) {
            Ꮡt.Errorf("Sqrt(%v) = %v (neg: %v); want %v (neg: %v)"u8,
                test.x.OrTypedNil(), got.OrTypedNil(), (~got).neg, test.want.OrTypedNil(), (~test.want).neg);
        }
    }
}

// Benchmarks
public static void BenchmarkFloatSqrt(ж<testing.B> Ꮡb) {
    foreach (var (_, prec) in new nuint[]{64, 128, 256, 1000, 10000, 100000, 1000000}.slice()) {
        var x = NewFloat(2D);
        var z = @new<global::go.math.big_package.Float>().SetPrec(prec);
        var xʗ1 = x;
        var zʗ1 = z;
        Ꮡb.Run(fmt.Sprintf("%v"u8, prec), (ж<testing.B> bΔ1) => {
            bΔ1.ReportAllocs();
            for (nint n = 0; n < (~bΔ1).N; n++) {
                zʗ1.Sqrt(xʗ1);
            }
        });
    }
}

} // end big_internal_test_package
