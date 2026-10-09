// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using Δruntime = runtime_package;
using static sort_package;
using stringspkg = strings_package;
using testing = testing_package;
using sort = sort_package;
using static go.sort_internal_test_package;

partial class sort_test_package {

internal static Func<nint, bool> f(slice<nint> a, nint x) {
    var aʗ1 = a;
    return (nint i) => aʗ1[i] >= x;
}

internal static slice<nint> data = new slice<nint>(14){[0] = -10, [1] = -5, [2] = 0, [3] = 1, [4] = 2, [5] = 3, [6] = 5, [7] = 7, [8] = 11, [9] = 100, [10] = 100, [11] = 100, [12] = 1000, [13] = 10000};


partial struct testsᴛ1 /*dyn*/ {
    internal @string name;
    internal nint n;
    internal Func<nint, bool> f;
    internal nint i;
}
internal static slice<testsᴛ1> tests = new testsᴛ1[]{
    new("empty"u8, 0, default!, 0),
    new("1 1"u8, 1, (nint i) => i >= 1, 1),
    new("1 true"u8, 1, (nint i) => true, 0),
    new("1 false"u8, 1, (nint i) => false, 1),
    new("1e9 991"u8, 1000000000, (nint i) => i >= 991, 991),
    new("1e9 true"u8, 1000000000, (nint i) => true, 0),
    new("1e9 false"u8, 1000000000, (nint i) => false, 1000000000),
    new("data -20"u8, len(data), f(data, -20), 0),
    new("data -10"u8, len(data), f(data, -10), 0),
    new("data -9"u8, len(data), f(data, -9), 1),
    new("data -6"u8, len(data), f(data, -6), 1),
    new("data -5"u8, len(data), f(data, -5), 1),
    new("data 3"u8, len(data), f(data, 3), 5),
    new("data 11"u8, len(data), f(data, 11), 8),
    new("data 99"u8, len(data), f(data, 99), 9),
    new("data 100"u8, len(data), f(data, 100), 9),
    new("data 101"u8, len(data), f(data, 101), 12),
    new("data 10000"u8, len(data), f(data, 10000), 13),
    new("data 10001"u8, len(data), f(data, 10001), 14),
    new("descending a"u8, 7, (nint i) => new nint[]{99, 99, 59, 42, 7, 0, -1, -1}.slice()[i] <= 7, 4),
    new("descending 7"u8, 1000000000, (nint i) => 1000000000 - i <= 7, 1000000000 - 7),
    new("overflow"u8, 2000000000, (nint i) => false, 2000000000)
}.slice();

public static void TestSearch(ж<testing.T> Ꮡt) {
    foreach (var (_, e) in tests) {
        nint i = Search(e.n, e.f);
        if (i != e.i) {
            Ꮡt.Errorf("%s: expected index %d; got %d"u8, e.name, e.i, i);
        }
    }
}

internal partial struct TestFind_tests /*dyn*/ {
    internal slice<@string> data;
    internal @string target;
    internal nint wantPos;
    internal bool wantFound;
}

public static void TestFind(ж<testing.T> Ꮡt) {
    var str1 = new @string[]{"foo"u8}.slice();
    var str2 = new @string[]{"ab"u8, "ca"u8}.slice();
    var str3 = new @string[]{"mo"u8, "qo"u8, "vo"u8}.slice();
    var str4 = new @string[]{"ab"u8, "ad"u8, "ca"u8, "xy"u8}.slice();
    // slice with repeating elements
    var strRepeats = new @string[]{"ba"u8, "ca"u8, "da"u8, "da"u8, "da"u8, "ka"u8, "ma"u8, "ma"u8, "ta"u8}.slice();
    // slice with all element equal
    var strSame = new @string[]{"xx"u8, "xx"u8, "xx"u8}.slice();
    var tests = new TestFind_tests[]{
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
        ref var tt = ref heap(new TestFind_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(tt.target, (ж<testing.T> tΔ1) => {
            var ttʗ2 = ttʗ1;
            var cmp = (nint i) => stringspkg.Compare(ttʗ2.target, ttʗ2.data[i]);
            var (pos, found) = Find(len(ttʗ1.data), cmp);
            if (pos != ttʗ1.wantPos || found != ttʗ1.wantFound) {
                tΔ1.Errorf("Find got (%v, %v), want (%v, %v)"u8, pos, found, ttʗ1.wantPos, ttʗ1.wantFound);
            }
        });
    }
}

// log2 computes the binary logarithm of x, rounded up to the next integer.
// (log2(0) == 0, log2(1) == 0, log2(2) == 1, log2(3) == 2, etc.)
internal static nint log2(nint x) {
    nint n = 0;
    for (nint p = 1; p < x; p += p) {
        // p == 2**n
        n++;
    }
    // p/2 < x <= p == 2**n
    return n;
}

public static void TestSearchEfficiency(ж<testing.T> Ꮡt) {
    nint n = 100;
    nint step = 1;
    for (nint exp = 2; exp < 10; exp++) {
        // n == 10**exp
        // step == 10**(exp-2)
        nint max = log2(n);
        for (nint xᴛ1 = 0; xᴛ1 < n; xᴛ1 += step) {
            var x = xᴛ1;
            nint count = 0;
            nint i = Search(n, (nint iΔ1) => {
                count++;
                return iΔ1 >= x;
            });
            if (i != x) {
                Ꮡt.Errorf("n = %d: expected index %d; got %d"u8, n, x, i);
            }
            if (count > max) {
                Ꮡt.Errorf("n = %d, x = %d: expected <= %d calls; got %d"u8, n, x, max, count);
            }
        }
        n *= 10;
        step *= 10;
    }
}

// Smoke tests for convenience wrappers - not comprehensive.
internal static slice<float64> fdata = new slice<float64>(5){[0] = -3.14D, [1] = 0D, [2] = 1D, [3] = 2D, [4] = 1000.7D};

internal static slice<@string> sdata = new slice<@string>(4){[0] = "f"u8, [1] = "foo"u8, [2] = "foobar"u8, [3] = "x"u8};


partial struct wrappertestsᴛ1 /*dyn*/ {
    internal @string name;
    internal nint result;
    internal nint i;
}
internal static slice<wrappertestsᴛ1> wrappertests = new wrappertestsᴛ1[]{
    new("SearchInts"u8, SearchInts(data, 11), 8),
    new("SearchFloat64s"u8, SearchFloat64s(fdata, 2.1D), 4),
    new("SearchStrings"u8, SearchStrings(sdata, ""u8), 0),
    new("IntSlice.Search"u8, ((sort.IntSlice)data).Search(0), 2),
    new("Float64Slice.Search"u8, ((sort.Float64Slice)fdata).Search(2.0D), 3),
    new("StringSlice.Search"u8, ((sort.StringSlice)sdata).Search("x"u8), 3)
}.slice();

public static void TestSearchWrappers(ж<testing.T> Ꮡt) {
    foreach (var (_, e) in wrappertests) {
        if (e.result != e.i) {
            Ꮡt.Errorf("%s: expected index %d; got %d"u8, e.name, e.i, e.result);
        }
    }
}

internal static void runSearchWrappers() {
    SearchInts(data, 11);
    SearchFloat64s(fdata, 2.1D);
    SearchStrings(sdata, ""u8);
    ((sort.IntSlice)data).Search(0);
    ((sort.Float64Slice)fdata).Search(2.0D);
    ((sort.StringSlice)sdata).Search("x"u8);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingMallocCountInˢ = (@string)"skipping malloc count in short mode"u8;
internal static readonly object skippingGomaxprocs1ˢ = (@string)"skipping; GOMAXPROCS>1"u8;

public static void TestSearchWrappersDontAlloc(ж<testing.T> Ꮡt) {
    if (testing.Short()) {
        Ꮡt.Skip(skippingMallocCountInˢ);
    }
    if (Δruntime.GOMAXPROCS(0) > 1) {
        Ꮡt.Skip(skippingGomaxprocs1ˢ);
    }
    var allocs = testing.AllocsPerRun(100, runSearchWrappers);
    if (allocs != 0D) {
        Ꮡt.Errorf("expected no allocs for runSearchWrappers, got %v"u8, allocs);
    }
}

public static void BenchmarkSearchWrappers(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        runSearchWrappers();
    }
}

// Abstract exhaustive test: all sizes up to 100,
// all possible return values. If there are any small
// corner cases, this test exercises them.
public static void TestSearchExhaustive(ж<testing.T> Ꮡt) {
    for (nint size = 0; size <= 100; size++) {
        for (nint targᴛ1 = 0; targᴛ1 <= size; targᴛ1++) {
            var targ = targᴛ1;
            nint i = Search(size, (nint iΔ1) => iΔ1 >= targ);
            if (i != targ) {
                Ꮡt.Errorf("Search(%d, %d) = %d"u8, size, targ, i);
            }
        }
    }
}

// Abstract exhaustive test for Find.
public static void TestFindExhaustive(ж<testing.T> Ꮡt) {
    // Test Find for different sequence sizes and search targets.
    // For each size, we have a (unmaterialized) sequence of integers:
    //   2,4...size*2
    // And we're looking for every possible integer between 1 and size*2 + 1.
    for (nint size = 0; size <= 100; size++) {
        for (nint xᴛ1 = 1; xᴛ1 <= size * 2 + 1; xᴛ1++) {
            var x = xᴛ1;
            bool wantFound = default!;
            nint wantPos = default!;
            var cmp = (nint i) => {
                // Encodes the unmaterialized sequence with elem[i] == (i+1)*2
                return x - (i + 1) * 2;
            };
            var (pos, found) = Find(size, cmp);
            if (x % 2 == 0){
                wantPos = x / 2 - 1;
                wantFound = true;
            } else {
                wantPos = x / 2;
                wantFound = false;
            }
            if (found != wantFound || pos != wantPos) {
                Ꮡt.Errorf("Find(%d, %d): got (%v, %v), want (%v, %v)"u8, size, x, pos, found, wantPos, wantFound);
            }
        }
    }
}

} // end sort_test_package
