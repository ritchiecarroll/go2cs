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

internal static array<@string> strs = new @string[]{""u8, "Hello"u8, "foo"u8, "bar"u8, "foo"u8, "f00"u8, "%*&^*&^&"u8, "***"u8}.array();

public static void TestSortIntSlice(ж<testing.T> Ꮡt) {
    var data = Clone<slice<nint>, nint>(ints[..]);
    Sort<slice<nint>, nint>(data);
    if (!IsSorted<slice<nint>, nint>(data)) {
        Ꮡt.Errorf("sorted %v"u8, ints);
        Ꮡt.Errorf("   got %v"u8, data);
    }
}

public static void TestSortFuncIntSlice(ж<testing.T> Ꮡt) {
    var data = Clone<slice<nint>, nint>(ints[..]);
    SortFunc(data, (nint a, nint b) => a - b);
    if (!IsSorted<slice<nint>, nint>(data)) {
        Ꮡt.Errorf("sorted %v"u8, ints);
        Ꮡt.Errorf("   got %v"u8, data);
    }
}

public static void TestSortFloat64Slice(ж<testing.T> Ꮡt) {
    var data = Clone<slice<float64>, float64>(float64s[..]);
    Sort<slice<float64>, float64>(data);
    if (!IsSorted<slice<float64>, float64>(data)) {
        Ꮡt.Errorf("sorted %v"u8, float64s);
        Ꮡt.Errorf("   got %v"u8, data);
    }
}

public static void TestSortStringSlice(ж<testing.T> Ꮡt) {
    var data = Clone<slice<@string>, @string>(strs[..]);
    Sort<slice<@string>, @string>(data);
    if (!IsSorted<slice<@string>, @string>(data)) {
        Ꮡt.Errorf("sorted %v"u8, strs);
        Ꮡt.Errorf("   got %v"u8, data);
    }
}

public static void TestSortLarge_Random(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    nint n = 1000000;
    if (testing.Short()) {
        n /= 100;
    }
    var data = new slice<nint>(n);
    for (nint i = 0; i < len(data); i++) {
        data[i] = rand.Intn(100);
    }
    if (IsSorted<slice<nint>, nint>(data)) {
        Ꮡt.Fatalf("terrible rand.rand"u8);
    }
    Sort<slice<nint>, nint>(data);
    if (!IsSorted<slice<nint>, nint>(data)) {
        Ꮡt.Errorf("sort didn't sort - 1M ints"u8);
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
    foreach (var (i, _) in d) {
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
        if (!reversed){
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

public static void TestStability(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

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
        Ꮡt.Fatalf("terrible rand.rand"u8);
    }
    data.initB();
    SortStableFunc<intPairs, intPair>(data, intPairCmp);
    if (!IsSortedFunc<intPairs, intPair>(data, intPairCmp)) {
        Ꮡt.Errorf("Stable didn't sort %d ints"u8, n);
    }
    if (!data.inOrder(false)) {
        Ꮡt.Errorf("Stable wasn't stable on %d ints"u8, n);
    }
    // already sorted
    data.initB();
    SortStableFunc<intPairs, intPair>(data, intPairCmp);
    if (!IsSortedFunc<intPairs, intPair>(data, intPairCmp)) {
        Ꮡt.Errorf("Stable shuffled sorted %d ints (order)"u8, n);
    }
    if (!data.inOrder(false)) {
        Ꮡt.Errorf("Stable shuffled sorted %d ints (stability)"u8, n);
    }
    // sorted reversed
    for (nint i = 0; i < len(data); i++) {
        data[i].a = len(data) - i;
    }
    data.initB();
    SortStableFunc<intPairs, intPair>(data, intPairCmp);
    if (!IsSortedFunc<intPairs, intPair>(data, intPairCmp)) {
        Ꮡt.Errorf("Stable didn't sort %d ints"u8, n);
    }
    if (!data.inOrder(false)) {
        Ꮡt.Errorf("Stable wasn't stable on %d ints"u8, n);
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

public static void TestMinMax(ж<testing.T> Ꮡt) {
    var intCmp = (nint a, nint b) => a - b;
    var tests = new TestMinMax_tests[]{
        new(new nint[]{7}.slice(), 7, 7),
        new(new nint[]{1, 2}.slice(), 1, 2),
        new(new nint[]{2, 1}.slice(), 1, 2),
        new(new nint[]{1, 2, 3}.slice(), 1, 3),
        new(new nint[]{3, 2, 1}.slice(), 1, 3),
        new(new nint[]{2, 1, 3}.slice(), 1, 3),
        new(new nint[]{2, 2, 3}.slice(), 2, 3),
        new(new nint[]{3, 2, 3}.slice(), 2, 3),
        new(new nint[]{0, 2, -9}.slice(), -9, 2)
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestMinMax_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var intCmpʗ1 = intCmp;
        var ttʗ1 = tt;
        Ꮡt.Run(fmt.Sprintf("%v"u8, tt.data), (ж<testing.T> tΔ1) => {
            nint gotMinΔ1 = Min<slice<nint>, nint>(ttʗ1.data);
            if (gotMinΔ1 != ttʗ1.wantMin) {
                tΔ1.Errorf("Min got %v, want %v"u8, gotMinΔ1, ttʗ1.wantMin);
            }
            nint gotMinFunc = MinFunc(ttʗ1.data, intCmpʗ1);
            if (gotMinFunc != ttʗ1.wantMin) {
                tΔ1.Errorf("MinFunc got %v, want %v"u8, gotMinFunc, ttʗ1.wantMin);
            }
            nint gotMaxΔ1 = Max<slice<nint>, nint>(ttʗ1.data);
            if (gotMaxΔ1 != ttʗ1.wantMax) {
                tΔ1.Errorf("Max got %v, want %v"u8, gotMaxΔ1, ttʗ1.wantMax);
            }
            nint gotMaxFunc = MaxFunc(ttʗ1.data, intCmpʗ1);
            if (gotMaxFunc != ttʗ1.wantMax) {
                tΔ1.Errorf("MaxFunc got %v, want %v"u8, gotMaxFunc, ttʗ1.wantMax);
            }
        });
    }
    var svals = new S[]{
        new(1, "a"u8),
        new(2, "a"u8),
        new(1, "b"u8),
        new(2, "b"u8)
    }.slice();
    var gotMin = MinFunc<slice<S>, S>(svals, cmpS);
    var wantMin = new S(1, "a"u8);
    if (gotMin != wantMin) {
        Ꮡt.Errorf("MinFunc(%v) = %v, want %v"u8, svals, gotMin, wantMin);
    }
    var gotMax = MaxFunc<slice<S>, S>(svals, cmpS);
    var wantMax = new S(2, "a"u8);
    if (gotMax != wantMax) {
        Ꮡt.Errorf("MaxFunc(%v) = %v, want %v"u8, svals, gotMax, wantMax);
    }
}

public static void TestMinMaxNaNs(ж<testing.T> Ꮡt) {
    var fs = new float64[]{1.0D, 999.9D, 3.14D, -400.4D, -5.14D}.slice();
    if (Min<slice<float64>, float64>(fs) != -400.4D) {
        Ꮡt.Errorf("got min %v, want -400.4"u8, Min<slice<float64>, float64>(fs));
    }
    if (Max<slice<float64>, float64>(fs) != 999.9D) {
        Ꮡt.Errorf("got max %v, want 999.9"u8, Max<slice<float64>, float64>(fs));
    }
    // No matter which element of fs is replaced with a NaN, both Min and Max
    // should propagate the NaN to their output.
    for (nint i = 0; i < len(fs); i++) {
        var testfs = Clone<slice<float64>, float64>(fs);
        testfs[i] = Δmath.NaN();
        var fmin = Min<slice<float64>, float64>(testfs);
        if (!Δmath.IsNaN(fmin)) {
            Ꮡt.Errorf("got min %v, want NaN"u8, fmin);
        }
        var fmax = Max<slice<float64>, float64>(testfs);
        if (!Δmath.IsNaN(fmax)) {
            Ꮡt.Errorf("got max %v, want NaN"u8, fmax);
        }
    }
}

public static void TestMinMaxPanics(ж<testing.T> Ꮡt) {
    var intCmp = (nint a, nint b) => a - b;
    var emptySlice = new nint[]{}.slice();
    var emptySliceʗ1 = emptySlice;
    if (!panics(() => {
        Min<slice<nint>, nint>(emptySliceʗ1);
    })) {
        Ꮡt.Errorf("Min([]): got no panic, want panic"u8);
    }
    var emptySliceʗ2 = emptySlice;
    if (!panics(() => {
        Max<slice<nint>, nint>(emptySliceʗ2);
    })) {
        Ꮡt.Errorf("Max([]): got no panic, want panic"u8);
    }
    var emptySliceʗ3 = emptySlice;
    var intCmpʗ1 = intCmp;
    if (!panics(() => {
        MinFunc(emptySliceʗ3, intCmpʗ1);
    })) {
        Ꮡt.Errorf("MinFunc([]): got no panic, want panic"u8);
    }
    var emptySliceʗ4 = emptySlice;
    var intCmpʗ2 = intCmp;
    if (!panics(() => {
        MaxFunc(emptySliceʗ4, intCmpʗ2);
    })) {
        Ꮡt.Errorf("MaxFunc([]): got no panic, want panic"u8);
    }
}

internal partial struct TestBinarySearch_tests /*dyn*/ {
    internal slice<@string> data;
    internal @string target;
    internal nint wantPos;
    internal bool wantFound;
}

public static void TestBinarySearch(ж<testing.T> Ꮡt) {
    var str1 = new @string[]{"foo"u8}.slice();
    var str2 = new @string[]{"ab"u8, "ca"u8}.slice();
    var str3 = new @string[]{"mo"u8, "qo"u8, "vo"u8}.slice();
    var str4 = new @string[]{"ab"u8, "ad"u8, "ca"u8, "xy"u8}.slice();
    // slice with repeating elements
    var strRepeats = new @string[]{"ba"u8, "ca"u8, "da"u8, "da"u8, "da"u8, "ka"u8, "ma"u8, "ma"u8, "ta"u8}.slice();
    // slice with all element equal
    var strSame = new @string[]{"xx"u8, "xx"u8, "xx"u8}.slice();
    var tests = new TestBinarySearch_tests[]{
        new(new @string[]{}.slice(), "foo"u8, 0, false),
        new(new @string[]{}.slice(), ""u8, 0, false),
        new(str1, "foo"u8, 0, true),
        new(str1, "bar"u8, 0, false),
        new(str1, "zx"u8, 1, false),
        new(str2, "aa"u8, 0, false),
        new(str2, "ab"u8, 0, true),
        new(str2, "ad"u8, 1, false),
        new(str2, "ca"u8, 1, true),
        new(str2, "ra"u8, 2, false),
        new(str3, "bb"u8, 0, false),
        new(str3, "mo"u8, 0, true),
        new(str3, "nb"u8, 1, false),
        new(str3, "qo"u8, 1, true),
        new(str3, "tr"u8, 2, false),
        new(str3, "vo"u8, 2, true),
        new(str3, "xr"u8, 3, false),
        new(str4, "aa"u8, 0, false),
        new(str4, "ab"u8, 0, true),
        new(str4, "ac"u8, 1, false),
        new(str4, "ad"u8, 1, true),
        new(str4, "ax"u8, 2, false),
        new(str4, "ca"u8, 2, true),
        new(str4, "cc"u8, 3, false),
        new(str4, "dd"u8, 3, false),
        new(str4, "xy"u8, 3, true),
        new(str4, "zz"u8, 4, false),
        new(strRepeats, "da"u8, 2, true),
        new(strRepeats, "db"u8, 5, false),
        new(strRepeats, "ma"u8, 6, true),
        new(strRepeats, "mb"u8, 8, false),
        new(strSame, "xx"u8, 0, true),
        new(strSame, "ab"u8, 0, false),
        new(strSame, "zz"u8, 3, false)
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestBinarySearch_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(tt.target, (ж<testing.T> tΔ1) => {
            {
                var (pos, found) = BinarySearch(ttʗ1.data, ttʗ1.target);
                if (pos != ttʗ1.wantPos || found != ttʗ1.wantFound) {
                    tΔ1.Errorf("BinarySearch got (%v, %v), want (%v, %v)"u8, pos, found, ttʗ1.wantPos, ttʗ1.wantFound);
                }
            }
            {
                var (pos, found) = BinarySearchFunc<slice<@string>, @string, @string>(ttʗ1.data, ttʗ1.target, strings.Compare);
                if (pos != ttʗ1.wantPos || found != ttʗ1.wantFound) {
                    tΔ1.Errorf("BinarySearchFunc got (%v, %v), want (%v, %v)"u8, pos, found, ttʗ1.wantPos, ttʗ1.wantFound);
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

public static void TestBinarySearchInts(ж<testing.T> Ꮡt) {
    var data = new nint[]{20, 30, 40, 50, 60, 70, 80, 90}.slice();
    var tests = new TestBinarySearchInts_tests[]{
        new(20, 0, true),
        new(23, 1, false),
        new(43, 3, false),
        new(80, 6, true)
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestBinarySearchInts_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var dataʗ1 = data;
        var ttʗ1 = tt;
        Ꮡt.Run(strconv.Itoa(tt.target), (ж<testing.T> tΔ1) => {
            {
                var (pos, found) = BinarySearch(dataʗ1, ttʗ1.target);
                if (pos != ttʗ1.wantPos || found != ttʗ1.wantFound) {
                    tΔ1.Errorf("BinarySearch got (%v, %v), want (%v, %v)"u8, pos, found, ttʗ1.wantPos, ttʗ1.wantFound);
                }
            }
            {
                var cmp = (nint a, nint b) => a - b;
                var (pos, found) = BinarySearchFunc(dataʗ1, ttʗ1.target, cmp);
                if (pos != ttʗ1.wantPos || found != ttʗ1.wantFound) {
                    tΔ1.Errorf("BinarySearchFunc got (%v, %v), want (%v, %v)"u8, pos, found, ttʗ1.wantPos, ttʗ1.wantFound);
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

public static void TestBinarySearchFloats(ж<testing.T> Ꮡt) {
    var data = new float64[]{Δmath.NaN(), -0.25D, 0.0D, 1.4D}.slice();
    var tests = new TestBinarySearchFloats_tests[]{
        new(Δmath.NaN(), 0, true),
        new(Δmath.Inf(-1), 1, false),
        new(-0.25D, 1, true),
        new(0.0D, 2, true),
        new(1.4D, 3, true),
        new(1.5D, 4, false)
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestBinarySearchFloats_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var dataʗ1 = data;
        var ttʗ1 = tt;
        Ꮡt.Run(fmt.Sprintf("%v"u8, tt.target), (ж<testing.T> tΔ1) => {
            {
                var (pos, found) = BinarySearch(dataʗ1, ttʗ1.target);
                if (pos != ttʗ1.wantPos || found != ttʗ1.wantFound) {
                    tΔ1.Errorf("BinarySearch got (%v, %v), want (%v, %v)"u8, pos, found, ttʗ1.wantPos, ttʗ1.wantFound);
                }
            }
        });
    }
}

public static void TestBinarySearchFunc(ж<testing.T> Ꮡt) {
    var data = new nint[]{1, 10, 11, 2}.slice(); // sorted lexicographically
    var cmp = (nint a, @string b) => strings.Compare(strconv.Itoa(a), b);
    var (pos, found) = BinarySearchFunc(data, (@string)"2", cmp);
    if (pos != 3 || !found) {
        Ꮡt.Errorf("BinarySearchFunc(%v, %q, cmp) = %v, %v, want %v, %v"u8, data, (@string)"2"u8, pos, found, (nint)(3), true);
    }
}

} // end slices_test_package
