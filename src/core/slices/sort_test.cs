// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using cmp = cmp_package;
using fmt = fmt_package;
using Δmath = math_package;
using rand = go.math.rand_package;
using static slices_package;
using strconv = strconv_package;
using strings = strings_package;
using testing = testing_package;
using go.math;

partial class slices_test_package {

internal static array<nint> ints = new nint[]{74, 59, 238, -784, 9845, 959, 905, 0, 0, 42, 7586, -5467984, 7586}.array();
internal static array<float64> float64s = new float64[]{74.3D, 59.0D, Δmath.Inf(1), 238.2D, -784.0D, 2.3D, Δmath.Inf(-1), 9845.768D, -959.7485D, 905D, 7.8D, 7.8D, 74.3D, 59.0D, Δmath.Inf(1), 238.2D, -784.0D, 2.3D}.array();
internal static array<@string> strs = new @string[]{"", "Hello", "foo", "bar", "foo", "f00", "%*&^*&^&", "***"}.array();

public static void TestSortIntSlice(ж<testing.T> t) {
    var data = Clone<slice<nint>, nint>(ints[..]);
    Sort<slice<nint>, nint>(data);
    if (!IsSorted<slice<nint>, nint>(data)) {
        t.Errorf("sorted %v", ints);
        t.Errorf("   got %v", data);
    }
}

public static void TestSortFuncIntSlice(ж<testing.T> t) {
    var data = Clone<slice<nint>, nint>(ints[..]);
    SortFunc(data, (nint a, nint b) => a - b);
    if (!IsSorted<slice<nint>, nint>(data)) {
        t.Errorf("sorted %v", ints);
        t.Errorf("   got %v", data);
    }
}

public static void TestSortFloat64Slice(ж<testing.T> t) {
    var data = Clone<slice<float64>, float64>(float64s[..]);
    Sort<slice<float64>, float64>(data);
    if (!IsSorted<slice<float64>, float64>(data)) {
        t.Errorf("sorted %v", float64s);
        t.Errorf("   got %v", data);
    }
}

public static void TestSortStringSlice(ж<testing.T> t) {
    var data = Clone<slice<@string>, @string>(strs[..]);
    Sort<slice<@string>, @string>(data);
    if (!IsSorted<slice<@string>, @string>(data)) {
        t.Errorf("sorted %v", strs);
        t.Errorf("   got %v", data);
    }
}

public static void TestSortLarge_Random(ж<testing.T> t) {
    nint n = 1000000;
    if (testing.Short()) {
        n /= 100;
    }
    var data = new slice<nint>(n);
    for (nint i = 0; i < len(data); i++) {
        data[i] = rand.Intn(100);
    }
    if (IsSorted<slice<nint>, nint>(data)) {
        t.Fatalf("terrible rand.rand");
    }
    Sort<slice<nint>, nint>(data);
    if (!IsSorted<slice<nint>, nint>(data)) {
        t.Errorf("sort didn't sort - 1M ints");
    }
}

partial struct intPair {
    internal nint a, b;
}

partial struct intPairs /*[]intPair*/;

// Pairs compare on a only.
internal static nint intPairCmp(intPair x, intPair y) {
    return x.a - y.a;
}

// Record initial order in B.
internal static void initB(this intPairs d) {
    foreach (var i in range(len(d))) {
        d[i].b = i;
    }
}

// InOrder checks if a-equal elements were not reordered.
// If reversed is true, expect reverse ordering.
internal static bool inOrder(this intPairs d, bool reversed) {
    nint lastA = -1;
    nint lastB = 0;
    for (nint i = 0; i < len(d); i++) {
        if (lastA != d[i].a) {
            lastA = d[i].a;
            lastB = d[i].b;
            continue;
        }
        if (!reversed) {
            if (d[i].b <= lastB) {
                return false;
            }
        } else {
            if (d[i].b >= lastB) {
                return false;
            }
        }
        lastB = d[i].b;
    }
    return true;
}

public static void TestStability(ж<testing.T> t) {
    nint n = 100000;
    nint m = 1000;
    if (testing.Short()) {
        (n, m) = (1000, 100);
    }
    var data = new intPairs(n);

    // random distribution
    for (nint i = 0; i < len(data); i++) {
        data[i].a = rand.Intn(m);
    }
    if (IsSortedFunc<intPairs, intPair>(data, intPairCmp)) {
        t.Fatalf("terrible rand.rand");
    }
    data.initB();
    SortStableFunc<intPairs, intPair>(data, intPairCmp);
    if (!IsSortedFunc<intPairs, intPair>(data, intPairCmp)) {
        t.Errorf("Stable didn't sort %d ints", n);
    }
    if (!data.inOrder(false)) {
        t.Errorf("Stable wasn't stable on %d ints", n);
    }

    // already sorted
    data.initB();
    SortStableFunc<intPairs, intPair>(data, intPairCmp);
    if (!IsSortedFunc<intPairs, intPair>(data, intPairCmp)) {
        t.Errorf("Stable shuffled sorted %d ints (order)", n);
    }
    if (!data.inOrder(false)) {
        t.Errorf("Stable shuffled sorted %d ints (stability)", n);
    }

    // sorted reversed
    for (nint i = 0; i < len(data); i++) {
        data[i].a = len(data) - i;
    }
    data.initB();
    SortStableFunc<intPairs, intPair>(data, intPairCmp);
    if (!IsSortedFunc<intPairs, intPair>(data, intPairCmp)) {
        t.Errorf("Stable didn't sort %d ints", n);
    }
    if (!data.inOrder(false)) {
        t.Errorf("Stable wasn't stable on %d ints", n);
    }
}

partial struct S {
    internal nint a;
    internal @string b;
}

internal static nint cmpS(S s1, S s2) {
    return cmp.Compare(s1.a, s2.a);
}

internal partial struct TestMinMax_tests /*dyn*/ {
    internal slice<nint> data;
    internal nint wantMin;
    internal nint wantMax;
}

public static void TestMinMax(ж<testing.T> t) {
    var intCmp = (nint a, nint b) => a - b;

    var tests = slice<TestMinMax_tests>([
        new([7], 7, 7),
        new([1, 2], 1, 2),
        new([2, 1], 1, 2),
        new([1, 2, 3], 1, 3),
        new([3, 2, 1], 1, 3),
        new([2, 1, 3], 1, 3),
        new([2, 2, 3], 2, 3),
        new([3, 2, 3], 2, 3),
        new([0, 2, -9], -9, 2)
    ]);
    foreach (var (_, vᴛ1) in range(tests)) {
        ref var tt = ref heap(new TestMinMax_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var intCmpʗ1 = intCmp;
        var ttʗ1 = tt;
        t.Run(fmt.Sprintf("%v"u8, tt.data), (ж<testing.T> tΔ1) => {
            nint gotMinΔ1 = Min<slice<nint>, nint>(ttʗ1.data);
            if (gotMinΔ1 != ttʗ1.wantMin) {
                tΔ1.Errorf("Min got %v, want %v", gotMinΔ1, ttʗ1.wantMin);
            }

            nint gotMinFunc = MinFunc(ttʗ1.data, intCmpʗ1);
            if (gotMinFunc != ttʗ1.wantMin) {
                tΔ1.Errorf("MinFunc got %v, want %v", gotMinFunc, ttʗ1.wantMin);
            }

            nint gotMaxΔ1 = Max<slice<nint>, nint>(ttʗ1.data);
            if (gotMaxΔ1 != ttʗ1.wantMax) {
                tΔ1.Errorf("Max got %v, want %v", gotMaxΔ1, ttʗ1.wantMax);
            }

            nint gotMaxFunc = MaxFunc(ttʗ1.data, intCmpʗ1);
            if (gotMaxFunc != ttʗ1.wantMax) {
                tΔ1.Errorf("MaxFunc got %v, want %v", gotMaxFunc, ttʗ1.wantMax);
            }
        });
    }

    var svals = slice<S>([
        new(1, "a"),
        new(2, "a"),
        new(1, "b"),
        new(2, "b")
    ]);

    var gotMin = MinFunc<slice<S>, S>(svals, cmpS);
    var wantMin = new S(1, "a");
    if (gotMin != wantMin) {
        t.Errorf("MinFunc(%v) = %v, want %v", svals, gotMin, wantMin);
    }

    var gotMax = MaxFunc<slice<S>, S>(svals, cmpS);
    var wantMax = new S(2, "a");
    if (gotMax != wantMax) {
        t.Errorf("MaxFunc(%v) = %v, want %v", svals, gotMax, wantMax);
    }
}

public static void TestMinMaxNaNs(ж<testing.T> t) {
    var fs = slice<float64>([1.0D, 999.9D, 3.14D, -400.4D, -5.14D]);
    if (Min<slice<float64>, float64>(fs) != -400.4D) {
        t.Errorf("got min %v, want -400.4", Min<slice<float64>, float64>(fs));
    }
    if (Max<slice<float64>, float64>(fs) != 999.9D) {
        t.Errorf("got max %v, want 999.9", Max<slice<float64>, float64>(fs));
    }

    // No matter which element of fs is replaced with a NaN, both Min and Max
    // should propagate the NaN to their output.
    for (nint i = 0; i < len(fs); i++) {
        var testfs = Clone<slice<float64>, float64>(fs);
        testfs[i] = Δmath.NaN();

        var fmin = Min<slice<float64>, float64>(testfs);
        if (!Δmath.IsNaN(fmin)) {
            t.Errorf("got min %v, want NaN", fmin);
        }

        var fmax = Max<slice<float64>, float64>(testfs);
        if (!Δmath.IsNaN(fmax)) {
            t.Errorf("got max %v, want NaN", fmax);
        }
    }
}

public static void TestMinMaxPanics(ж<testing.T> t) {
    var intCmp = (nint a, nint b) => a - b;
    var emptySlice = slice<nint>([]);

    var emptySliceʗ1 = emptySlice;
    if (!panics(() => {
        _ = Min<slice<nint>, nint>(emptySliceʗ1);
    })) {
        t.Errorf("Min([]): got no panic, want panic");
    }

    var emptySliceʗ2 = emptySlice;
    if (!panics(() => {
        _ = Max<slice<nint>, nint>(emptySliceʗ2);
    })) {
        t.Errorf("Max([]): got no panic, want panic");
    }

    var emptySliceʗ3 = emptySlice;
    var intCmpʗ1 = intCmp;
    if (!panics(() => {
        _ = MinFunc(emptySliceʗ3, intCmpʗ1);
    })) {
        t.Errorf("MinFunc([]): got no panic, want panic");
    }

    var emptySliceʗ4 = emptySlice;
    var intCmpʗ2 = intCmp;
    if (!panics(() => {
        _ = MaxFunc(emptySliceʗ4, intCmpʗ2);
    })) {
        t.Errorf("MaxFunc([]): got no panic, want panic");
    }
}

internal partial struct TestBinarySearch_tests /*dyn*/ {
    internal slice<@string> data;
    internal @string target;
    internal nint wantPos;
    internal bool wantFound;
}

public static void TestBinarySearch(ж<testing.T> t) {
    var str1 = slice<@string>(["foo"]);
    var str2 = slice<@string>(["ab", "ca"]);
    var str3 = slice<@string>(["mo", "qo", "vo"]);
    var str4 = slice<@string>(["ab", "ad", "ca", "xy"]);

    // slice with repeating elements
    var strRepeats = slice<@string>(["ba", "ca", "da", "da", "da", "ka", "ma", "ma", "ta"]);

    // slice with all element equal
    var strSame = slice<@string>(["xx", "xx", "xx"]);

    var tests = slice<TestBinarySearch_tests>([
        new([], "foo", 0, false),
        new([], "", 0, false),

        new(str1, "foo", 0, true),
        new(str1, "bar", 0, false),
        new(str1, "zx", 1, false),

        new(str2, "aa", 0, false),
        new(str2, "ab", 0, true),
        new(str2, "ad", 1, false),
        new(str2, "ca", 1, true),
        new(str2, "ra", 2, false),

        new(str3, "bb", 0, false),
        new(str3, "mo", 0, true),
        new(str3, "nb", 1, false),
        new(str3, "qo", 1, true),
        new(str3, "tr", 2, false),
        new(str3, "vo", 2, true),
        new(str3, "xr", 3, false),

        new(str4, "aa", 0, false),
        new(str4, "ab", 0, true),
        new(str4, "ac", 1, false),
        new(str4, "ad", 1, true),
        new(str4, "ax", 2, false),
        new(str4, "ca", 2, true),
        new(str4, "cc", 3, false),
        new(str4, "dd", 3, false),
        new(str4, "xy", 3, true),
        new(str4, "zz", 4, false),

        new(strRepeats, "da", 2, true),
        new(strRepeats, "db", 5, false),
        new(strRepeats, "ma", 6, true),
        new(strRepeats, "mb", 8, false),

        new(strSame, "xx", 0, true),
        new(strSame, "ab", 0, false),
        new(strSame, "zz", 3, false)
    ]);
    foreach (var (_, vᴛ1) in range(tests)) {
        ref var tt = ref heap(new TestBinarySearch_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(tt.target, (ж<testing.T> tΔ1) => {
            {
                var (pos, found) = BinarySearch(ttʗ1.data, ttʗ1.target);
                if (pos != ttʗ1.wantPos || found != ttʗ1.wantFound) {
                    tΔ1.Errorf("BinarySearch got (%v, %v), want (%v, %v)", pos, found, ttʗ1.wantPos, ttʗ1.wantFound);
                }
            }

            {
                var (pos, found) = BinarySearchFunc<slice<@string>, @string, @string>(ttʗ1.data, ttʗ1.target, strings.Compare);
                if (pos != ttʗ1.wantPos || found != ttʗ1.wantFound) {
                    tΔ1.Errorf("BinarySearchFunc got (%v, %v), want (%v, %v)", pos, found, ttʗ1.wantPos, ttʗ1.wantFound);
                }
            }
        });
    }
}

internal partial struct TestBinarySearchInts_tests /*dyn*/ {
    internal nint target;
    internal nint wantPos;
    internal bool wantFound;
}

public static void TestBinarySearchInts(ж<testing.T> t) {
    var data = slice<nint>([20, 30, 40, 50, 60, 70, 80, 90]);
    var tests = slice<TestBinarySearchInts_tests>([
        new(20, 0, true),
        new(23, 1, false),
        new(43, 3, false),
        new(80, 6, true)
    ]);
    foreach (var (_, vᴛ1) in range(tests)) {
        ref var tt = ref heap(new TestBinarySearchInts_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var dataʗ1 = data;
        var ttʗ1 = tt;
        t.Run(strconv.Itoa(tt.target), (ж<testing.T> tΔ1) => {
            {
                var (pos, found) = BinarySearch(dataʗ1, ttʗ1.target);
                if (pos != ttʗ1.wantPos || found != ttʗ1.wantFound) {
                    tΔ1.Errorf("BinarySearch got (%v, %v), want (%v, %v)", pos, found, ttʗ1.wantPos, ttʗ1.wantFound);
                }
            }

            {
                var cmp = (nint a, nint b) => a - b;
                var (pos, found) = BinarySearchFunc(dataʗ1, ttʗ1.target, cmp);
                if (pos != ttʗ1.wantPos || found != ttʗ1.wantFound) {
                    tΔ1.Errorf("BinarySearchFunc got (%v, %v), want (%v, %v)", pos, found, ttʗ1.wantPos, ttʗ1.wantFound);
                }
            }
        });
    }
}

internal partial struct TestBinarySearchFloats_tests /*dyn*/ {
    internal float64 target;
    internal nint wantPos;
    internal bool wantFound;
}

public static void TestBinarySearchFloats(ж<testing.T> t) {
    var data = slice<float64>([Δmath.NaN(), -0.25D, 0.0D, 1.4D]);
    var tests = slice<TestBinarySearchFloats_tests>([
        new(Δmath.NaN(), 0, true),
        new(Δmath.Inf(-1), 1, false),
        new(-0.25D, 1, true),
        new(0.0D, 2, true),
        new(1.4D, 3, true),
        new(1.5D, 4, false)
    ]);
    foreach (var (_, vᴛ1) in range(tests)) {
        ref var tt = ref heap(new TestBinarySearchFloats_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var dataʗ1 = data;
        var ttʗ1 = tt;
        t.Run(fmt.Sprintf("%v"u8, tt.target), (ж<testing.T> tΔ1) => {
            {
                var (pos, found) = BinarySearch(dataʗ1, ttʗ1.target);
                if (pos != ttʗ1.wantPos || found != ttʗ1.wantFound) {
                    tΔ1.Errorf("BinarySearch got (%v, %v), want (%v, %v)", pos, found, ttʗ1.wantPos, ttʗ1.wantFound);
                }
            }
        });
    }
}

public static void TestBinarySearchFunc(ж<testing.T> t) {
    var data = slice<nint>([1, 10, 11, 2]); // sorted lexicographically
    var cmp = (nint a, @string b) => strings.Compare(strconv.Itoa(a), b);
    var (pos, found) = BinarySearchFunc(data, (@string)"2", cmp);
    if (pos != 3 || !found) {
        t.Errorf("BinarySearchFunc(%v, %q, cmp) = %v, %v, want %v, %v", data, (@string)"2", pos, found, (nint)3, true);
    }
}

} // end slices_test_package
