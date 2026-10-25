// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// TestCalibrate determines appropriate thresholds for when to use
// different calculation algorithms. To run it, use:
//
//	go test -run=Calibrate -calibrate >cal.log
//
// Calibration data is printed in CSV format, along with the normal test output.
// See calibrate.md for more details about using the output.
namespace go.math;

using flag = flag_package;
using fmt = fmt_package;
using sysinfo = @internal.sysinfo_package;
using math = math_package;
using runtime = runtime_package;
using slices = slices_package;
using strings = strings_package;
using sync = sync_package;
using testing = testing_package;
using time = time_package;
using @internal;
using static go.math.big_package;

partial class big_internal_test_package {

internal static ж<bool> calibrate = flag.Bool("calibrate"u8, false, "run calibration test"u8);

internal static sync.Once calibrateOnce;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string karatsubaMulˢ = "KaratsubaMul"u8;
internal static readonly @string basicSqrˢ = "BasicSqr"u8;
internal static readonly @string karatsubaSqrˢ = "KaratsubaSqr"u8;
internal static readonly @string divRecursiveˢ = "DivRecursive"u8;

public static void TestCalibrate(ж<testing.T> Ꮡt) {
    if (!calibrate.Value) {
        return;
    }

    Ꮡt.Run(karatsubaMulˢ, computeKaratsubaThreshold);
    Ꮡt.Run(basicSqrˢ, computeBasicSqrThreshold);
    Ꮡt.Run(karatsubaSqrˢ, computeKaratsubaSqrThreshold);
    Ꮡt.Run(divRecursiveˢ, computeDivRecursiveThreshold);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string karatsubaˢ = "karatsuba"u8;

internal static void computeKaratsubaThreshold(ж<testing.T> Ꮡt) {
    var set = (nint n) => {
        karatsubaThreshold = n;
    };
    computeThreshold(Ꮡt, karatsubaˢ, set, 0, 4, 200, benchMul, 200, 8, 400);
}

internal static Action benchMul(nint size) {
    var x = rndNat(size);
    var y = rndNat(size);
    global::go.math.big_package.Δnat z = default!;
    var xʗ1 = x;
    var yʗ1 = y;
    var zʗ1 = z;
    return () => {
        zʗ1.mul(nil, xʗ1, yʗ1);
    };
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string basicSqrˢ2 = "basicSqr"u8;

internal static void computeBasicSqrThreshold(ж<testing.T> Ꮡt) {
    setDuringTest(Ꮡt, ᏑkaratsubaSqrThreshold, (nint)1000000000);
    var set = (nint n) => {
        basicSqrThreshold = n;
    };
    computeThreshold(Ꮡt, basicSqrˢ2, set, 2, 1, 40, benchBasicSqr, 1, 1, 40);
}

internal static Action benchBasicSqr(nint size) {
    var x = rndNat(size);
    global::go.math.big_package.Δnat z = default!;
    var xʗ1 = x;
    var zʗ1 = z;
    return () => {
        // Run 100 squarings because 1 is too fast at the small sizes we consider.
        // Some systems don't even have precise enough clocks to measure it accurately.
        foreach (var _ᴛ1 in range(100)) {
            zʗ1.sqr(nil, xʗ1);
        }
    };
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string karatsubaSqrˢ2 = "karatsubaSqr"u8;

internal static void computeKaratsubaSqrThreshold(ж<testing.T> Ꮡt) {
    var set = (nint n) => {
        karatsubaSqrThreshold = n;
    };
    computeThreshold(Ꮡt, karatsubaSqrˢ2, set, 0, 4, 200, benchSqr, 200, 8, 400);
}

internal static Action benchSqr(nint size) {
    var x = rndNat(size);
    global::go.math.big_package.Δnat z = default!;
    var xʗ1 = x;
    var zʗ1 = z;
    return () => {
        zʗ1.sqr(nil, xʗ1);
    };
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string divRecursiveˢ2 = "divRecursive"u8;

internal static void computeDivRecursiveThreshold(ж<testing.T> Ꮡt) {
    var set = (nint n) => {
        divRecursiveThreshold = n;
    };
    computeThreshold(Ꮡt, divRecursiveˢ2, set, 4, 4, 200, benchDiv, 200, 8, 400);
}

internal static Action benchDiv(nint size) {
    var divx = rndNat(2 * size);
    var divy = rndNat(size);
    global::go.math.big_package.Δnat z = default!;
    global::go.math.big_package.Δnat r = default!;
    var divxʗ1 = divx;
    var divyʗ1 = divy;
    var rʗ1 = r;
    var zʗ1 = z;
    return () => {
        zʗ1.div(nil, rʗ1, divxʗ1, divyʗ1);
    };
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string sizeThresholdˢ = "size \\ threshold"u8;
internal static readonly @string geomeanˢ = "geomean"u8;

internal static void computeThreshold(ж<testing.T> Ꮡt, @string name, Action<nint> set, nint thresholdLo, nint thresholdStep, nint thresholdHi, Func<nint, Action> bench, nint sizeLo, nint sizeStep, nint sizeHi) {
    GoFrame ᒐ = default;
    try {
        var sizesᴷ = new stackAppend<nint>();
        var thresholdsᴷ = new stackAppend<nint>();

        // Start CSV output; wrapped in txtar framing to separate CSV from other test ouptut.
        fmt.Printf("-- calibrate-%s.csv --\n"u8, name);
        defer(ᴛ1 => fmt.Printf(ᴛ1), (@string)"-- eof --\n", ref ᒐ);

        fmt.Printf("goos,%s\n"u8, runtime.GOOS);
        fmt.Printf("goarch,%s\n"u8, runtime.GOARCH);
        fmt.Printf("cpu,%s\n"u8, sysinfo.CPUName());
        fmt.Printf("calibrate,%s\n"u8, name);

        // Expand lists of sizes and thresholds we will test.
        slice<nint> sizes = default!;
        slice<nint> thresholds = default!;
        for (nint size = sizeLo; size <= sizeHi; size += sizeStep) {
            sizes = append(sizesᴷ, sizes, size);
        }
        for (nint thresh = thresholdLo; thresh <= thresholdHi; thresh += thresholdStep) {
            thresholds = append(thresholdsᴷ, thresholds, thresh);
        }

        fmt.Printf("%s\n"u8, csv(sizeThresholdˢ, thresholds));

        // Track minimum time observed for each size, threshold pair.
        var times = new slice<slice<float64>>(len(sizes));
        foreach (var i in range(sizes)) {
            times[i] = new slice<float64>(len(thresholds));
            foreach (var j in range(thresholds)) {
                times[i][j] = math.Inf(+1);
            }
        }

        // For each size, run at most MaxRounds of considering every threshold.
        // If we run a threshold Stable times in a row without seeing more
        // than a 1% improvement in the observed minimum, move on to the next one.
        // After we run Converged rounds (not necessarily in a row)
        // without seeing any threshold improve by more than 1%, stop.
        const nint MaxRounds = 1600;
        const nint Stable = 20;
        const nint Converged = 200;

        foreach (var (i, size) in sizes) {
            var b = bench(size);
            nint same = 0;
            foreach (var _ᴛ1 in range(MaxRounds)) {
                var better = false;
                foreach (var (j, threshold) in thresholds) {
                    // No point if threshold is far beyond size
                    if (false && threshold > size + 2 * sizeStep) {
                        continue;
                    }

                    // BasicSqr is different from the recursive thresholds: it either applies or not,
                    // without any question of recursive subproblems. Only try the thresholds
                    //	size-1, size, size+1, size+2
                    // to get two data points using basic multiplication and two using basic squaring.
                    // This avoids gathering many redundant data points.
                    // (The others have redundant data points as well, but for them the math is less trivial
                    // and best not duplicated in the calibration code.)
                    if (false && name == "basicSqr"u8 && (threshold < size - 1 || threshold > size + 3)) {
                        continue;
                    }

                    set(threshold);
                    b(); // warm up
                    b();
                    var tmin = times[i][j];
                    for (nint k = 0; k < Stable; k++) {
                        var start = time.Now();
                        b();
                        var tΔ1 = (float64)(int64)time.Since(start);
                        if (tΔ1 < tmin) {
                            if (tΔ1 < tmin * 99D / 100D) {
                                better = true;
                                k = 0;
                            }
                            tmin = tΔ1;
                        }
                    }
                    times[i][j] = tmin;
                }
                if (!better) {
                    {
                        same++; if (same >= Converged) {
                            break;
                        }
                    }
                }
            }

            fmt.Printf("%s\n"u8, csv(fmt.Sprint(size), times[i]));
        }

        // For each size, normalize timings by the minimum achieved for that size.
        fmt.Printf("%s\n"u8, csv(sizeThresholdˢ, thresholds));
        var norms = new slice<slice<float64>>(len(sizes));
        foreach (var (i, timesΔ1) in times) {
            var m = min(1e100D, slices.Min<slice<float64>, float64>(timesΔ1)); // make finite so divide preserves inf values
            norms[i] = new slice<float64>(len(timesΔ1));
            foreach (var (j, d) in timesΔ1) {
                norms[i][j] = d / m;
            }
            fmt.Printf("%s\n"u8, csv(fmt.Sprint(sizes[i]), norms[i]));
        }

        // For each threshold, compute geomean of normalized timings across all sizes.
        var geomeans = new slice<float64>(len(thresholds));
        foreach (var j in range(thresholds)) {
            var p = 1.0D;
            nint n = 0;
            foreach (var i in range(sizes)) {
                {
                    var v = norms[i][j]; if (!math.IsInf(v, +1)) {
                        p *= v;
                        n++;
                    }
                }
            }
            if (n == 0){
                geomeans[j] = math.Inf(+1);
            } else {
                geomeans[j] = math.Pow(p, 1D / (float64)n);
            }
        }
        fmt.Printf("%s\n"u8, csv(geomeanˢ, geomeans));

        // Add best threshold and smallest, largest within 10% and 5% of best.
        nint lo10 = default!;
        nint lo5 = default!;
        nint best = default!;
        nint hi5 = default!;
        nint hi10 = default!;
        foreach (var (i, g) in geomeans) {
            if (g < geomeans[best]) {
                best = i;
            }
        }
        lo5 = best;
        while (lo5 > 0 && geomeans[lo5 - 1] <= 1.05D) {
            lo5--;
        }
        lo10 = lo5;
        while (lo10 > 0 && geomeans[lo10 - 1] <= 1.10D) {
            lo10--;
        }
        hi5 = best;
        while (hi5 + 1 < len(geomeans) && geomeans[hi5 + 1] <= 1.05D) {
            hi5++;
        }
        hi10 = hi5;
        while (hi10 + 1 < len(geomeans) && geomeans[hi10 + 1] <= 1.10D) {
            hi10++;
        }
        fmt.Printf("lo10%%,%d\n"u8, thresholds[lo10]);
        fmt.Printf("lo5%%,%d\n"u8, thresholds[lo5]);
        fmt.Printf("min,%d\n"u8, thresholds[best]);
        fmt.Printf("hi5%%,%d\n"u8, thresholds[hi5]);
        fmt.Printf("hi10%%,%d\n"u8, thresholds[hi10]);

        set(thresholds[best]);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// csv returns a single csv line starting with name and followed by the values.
// Values that are float64 +infinity, denoting missing data, are replaced by an empty string.
internal static @string csv<T>(@string name, slice<T> values)
    where T : /* int | float64 */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IUnaryNegationOperators<T, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    var lineᴷ = new stackAppend<@string>();

    var line = new @string[]{name}.slice();
    foreach (var (_, v) in values) {
        if (math.IsInf(ConvertToFloat64<T>(v), +1)){
            line = append(lineᴷ, line, ""u8);
        } else {
            line = append(line, fmt.Sprint(v));
        }
    }
    return strings.Join(line, ","u8);
}

} // end big_internal_test_package
