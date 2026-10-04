// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Calibration used to determine thresholds for using
// different algorithms.  Ideally, this would be converted
// to go generate to create thresholds.go
// This file prints execution times for the Mul benchmark
// given different Karatsuba thresholds. The result may be
// used to manually fine-tune the threshold constant. The
// results are somewhat fragile; use repeated runs to get
// a clear picture.
// Calculates lower and upper thresholds for when basicSqr
// is faster than standard multiplication.
// Usage: go test -run='^TestCalibrate$' -v -calibrate
namespace go.math;

using flag = flag_package;
using fmt = fmt_package;
using testing = testing_package;
using time = time_package;
using static go.math.big_package;

partial class big_internal_test_package {

internal static ж<bool> calibrate = flag.Bool("calibrate"u8, false, "run calibration test"u8);

internal static readonly @string sqrModeMul = "mul(x, x)"u8;
internal static readonly @string sqrModeBasic = "basicSqr(x)"u8;
internal static readonly @string sqrModeKaratsuba = "karatsubaSqr(x)"u8;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object noBasicSqrThresholdFoundˢ = (@string)"no basicSqrThreshold found"u8;
internal static readonly object noKaratsubaSqrThresholdˢ = (@string)"no karatsubaSqrThreshold found"u8;

public static void TestCalibrate(ж<testing.T> Ꮡt) {
    if (!calibrate.Value) {
        return;
    }
    computeKaratsubaThresholds();
    // compute basicSqrThreshold where overhead becomes negligible
    nint minSqr = computeSqrThreshold(10, 30, 1, 3, sqrModeMul, sqrModeBasic);
    // compute karatsubaSqrThreshold where karatsuba is faster
    nint maxSqr = computeSqrThreshold(200, 500, 10, 3, sqrModeBasic, sqrModeKaratsuba);
    if (minSqr != 0){
        fmt.Printf("found basicSqrThreshold = %d\n"u8, minSqr);
    } else {
        fmt.Println(noBasicSqrThresholdFoundˢ);
    }
    if (maxSqr != 0){
        fmt.Printf("found karatsubaSqrThreshold = %d\n"u8, maxSqr);
    } else {
        fmt.Println(noKaratsubaSqrThresholdˢ);
    }
}

internal static void karatsubaLoad(ж<testing.B> Ꮡb) {
    BenchmarkMul(Ꮡb);
}

// measureKaratsuba returns the time to run a Karatsuba-relevant benchmark
// given Karatsuba threshold th.
internal static time.Duration measureKaratsuba(nint th) {
    (th, karatsubaThreshold) = (karatsubaThreshold, th);
    var res = testing.Benchmark(karatsubaLoad);
    karatsubaThreshold = th;
    return ((time.Duration)res.NsPerOp());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object breakEvenPointˢ = (@string)"  break-even point"u8;
internal static readonly object diminishingReturnˢ = (@string)"  diminishing return"u8;

internal static void computeKaratsubaThresholds() {
    fmt.Printf("Multiplication times for varying Karatsuba thresholds\n"u8);
    fmt.Printf("(run repeatedly for good results)\n"u8);
    // determine Tk, the work load execution time using basic multiplication
    var Tb = measureKaratsuba(1000000000); // th == 1e9 => Karatsuba multiplication disabled
    fmt.Printf("Tb = %10s\n"u8, Tb);
    // thresholds
    nint th = 4;
    nint th1 = -1;
    nint th2 = -1;
    time.Duration deltaOld = default!;
    for (nint count = -1; count != 0 && th < 128; count--) {
        // determine Tk, the work load execution time using Karatsuba multiplication
        var Tk = measureKaratsuba(th);
        // improvement over Tb
        var delta = (Tb - Tk) * 100 / Tb;
        fmt.Printf("th = %3d  Tk = %10s  %4d%%"u8, th, Tk, delta);
        // determine break-even point
        if (Tk < Tb && th1 < 0) {
            th1 = th;
            fmt.Print(breakEvenPointˢ);
        }
        // determine diminishing return
        if (0 < delta && delta < deltaOld && th2 < 0) {
            th2 = th;
            fmt.Print(diminishingReturnˢ);
        }
        deltaOld = delta;
        fmt.Println();
        // trigger counter
        if (th1 >= 0 && th2 >= 0 && count < 0) {
            count = 10; // this many extra measurements after we got both thresholds
        }
        th++;
    }
}

internal static time.Duration measureSqr(nint words, nint nruns, @string mode) {
    // more runs for better statistics
    nint initBasicSqr = basicSqrThreshold;
    nint initKaratsubaSqr = karatsubaSqrThreshold;
    var exprᴛ1 = mode;
    if (exprᴛ1 == sqrModeMul) {
        basicSqrThreshold = words + 1;
    }
    else if (exprᴛ1 == sqrModeBasic) {
        (basicSqrThreshold, karatsubaSqrThreshold) = (words - 1, words + 1);
    }
    else if (exprᴛ1 == sqrModeKaratsuba) {
        karatsubaSqrThreshold = words - 1;
    }

    int64 testval = default!;
    for (nint i = 0; i < nruns; i++) {
        var res = testing.Benchmark((ж<testing.B> b) => {
            benchmarkNatSqr(b, words);
        });
        testval += res.NsPerOp();
    }
    testval = quo(testval, (int64)nruns);
    (basicSqrThreshold, karatsubaSqrThreshold) = (initBasicSqr, initKaratsubaSqr);
    return ((time.Duration)testval);
}

internal static nint computeSqrThreshold(nint from, nint to, nint step, nint nruns, @string lower, @string upper) {
    fmt.Printf("Calibrating threshold between %s and %s\n"u8, lower, upper);
    fmt.Printf("Looking for a timing difference for x between %d - %d words by %d step\n"u8, from, to, step);
    bool initPos = default!;
    nint threshold = default!;
    for (nint i = from; i <= to; i += step) {
        var baseline = measureSqr(i, nruns, lower);
        var testval = measureSqr(i, nruns, upper);
        var pos = baseline > testval;
        var delta = baseline - testval;
        var percent = delta * 100 / baseline;
        fmt.Printf("words = %3d deltaT = %10s (%4d%%) is %s better: %v"u8, i, delta, percent, upper, pos);
        if (i == from) {
            initPos = pos;
        }
        if (threshold == 0 && pos != initPos) {
            threshold = i;
            fmt.Printf("  threshold  found"u8);
        }
        fmt.Println();
    }
    if (threshold != 0){
        fmt.Printf("Found threshold = %d between %d - %d\n"u8, threshold, from, to);
    } else {
        fmt.Printf("Found NO threshold between %d - %d\n"u8, from, to);
    }
    return threshold;
}

} // end big_internal_test_package
