// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using bits = go.math.bits_package;
using Δruntime = runtime_package;
using static sort_package;
using stringspkg = strings_package;
using testing = testing_package;
using go.math;
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
internal static slice<testsᴛ1> tests = slice<testsᴛ1>([
    new("empty", 0, default, 0),
    new("1 1", 1, (nint i) => i >= 1, 1),
    new("1 true", 1, (nint i) => true, 0),
    new("1 false", 1, (nint i) => false, 1),
    new("1e9 991", 1000000000, (nint i) => i >= 991, 991),
    new("1e9 true", 1000000000, (nint i) => true, 0),
    new("1e9 false", 1000000000, (nint i) => false, 1000000000),
    new("data -20", len(data), f(data, -20), 0),
    new("data -10", len(data), f(data, -10), 0),
    new("data -9", len(data), f(data, -9), 1),
    new("data -6", len(data), f(data, -6), 1),
    new("data -5", len(data), f(data, -5), 1),
    new("data 3", len(data), f(data, 3), 5),
    new("data 11", len(data), f(data, 11), 8),
    new("data 99", len(data), f(data, 99), 9),
    new("data 100", len(data), f(data, 100), 9),
    new("data 101", len(data), f(data, 101), 12),
    new("data 10000", len(data), f(data, 10000), 13),
    new("data 10001", len(data), f(data, 10001), 14),
    new("descending a", 7, (nint i) => slice<nint>([99, 99, 59, 42, 7, 0, -1, -1])[i] <= 7, 4),
    new("descending 7", 1000000000, (nint i) => 1000000000 - i <= 7, 1000000000 - 7),
    new("overflow", 2000000000, (nint i) => false, 2000000000)
]);

public static void TestSearch(ж<testing.T> t) {
    foreach (var (_, e) in range(tests)) {
        nint i = Search(e.n, e.f);
        if (i != e.i) {
            t.Errorf("%s: expected index %d; got %d", e.name, e.i, i);
        }
    }
}

internal partial struct TestFind_tests /*dyn*/ {
    internal slice<@string> data;
    internal @string target;
    internal nint wantPos;
    internal bool wantFound;
}

public static void TestFind(ж<testing.T> t) {
    var str1 = slice<@string>(["foo"]);
    var str2 = slice<@string>(["ab", "ca"]);
    var str3 = slice<@string>(["mo", "qo", "vo"]);
    var str4 = slice<@string>(["ab", "ad", "ca", "xy"]);

    // slice with repeating elements
    var strRepeats = slice<@string>(["ba", "ca", "da", "da", "da", "ka", "ma", "ma", "ta"]);

    // slice with all element equal
    var strSame = slice<@string>(["xx", "xx", "xx"]);

    var tests = slice<TestFind_tests>([
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
        ref var tt = ref heap(new TestFind_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(tt.target, (ж<testing.T> tΔ1) => {
            var ttʗ2 = ttʗ1;
            var cmp = (nint i) => stringspkg.Compare(ttʗ2.target, ttʗ2.data[i]);

            var (pos, found) = Find(len(ttʗ1.data), cmp);
            if (pos != ttʗ1.wantPos || found != ttʗ1.wantFound) {
                tΔ1.Errorf("Find got (%v, %v), want (%v, %v)", pos, found, ttʗ1.wantPos, ttʗ1.wantFound);
            }
        });
    }
}

// log2 computes the binary logarithm of x, rounded up to the next integer.
// (log2(0) == 0, log2(1) == 0, log2(2) == 1, log2(3) == 2, etc.)
internal static nint log2(nint x) {
    if (x < 1) {
        return 0;
    }
    return bits.Len((nuint)(x - 1));
}

public static void TestSearchEfficiency(ж<testing.T> t) {
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
                t.Errorf("n = %d: expected index %d; got %d", n, x, i);
            }
            if (count > max) {
                t.Errorf("n = %d, x = %d: expected <= %d calls; got %d", n, x, max, count);
            }
        }
        n *= 10;
        step *= 10;
    }
}

// Smoke tests for convenience wrappers - not comprehensive.
internal static slice<float64> fdata = new slice<float64>(5){[0] = -3.14D, [1] = 0D, [2] = 1D, [3] = 2D, [4] = 1000.7D};
internal static slice<@string> sdata = new slice<@string>(4){[0] = "f", [1] = "foo", [2] = "foobar", [3] = "x"};

partial struct wrappertestsᴛ1 /*dyn*/ {
    internal @string name;
    internal nint result;
    internal nint i;
}
internal static slice<wrappertestsᴛ1> wrappertests = slice<wrappertestsᴛ1>([
    new("SearchInts", SearchInts(data, 11), 8),
    new("SearchFloat64s", SearchFloat64s(fdata, 2.1D), 4),
    new("SearchStrings", SearchStrings(sdata, ""), 0),
    new("IntSlice.Search", ((sort.IntSlice)data).Search(0), 2),
    new("Float64Slice.Search", ((sort.Float64Slice)fdata).Search(2.0D), 3),
    new("StringSlice.Search", ((sort.StringSlice)sdata).Search("x"), 3)
]);

public static void TestSearchWrappers(ж<testing.T> t) {
    foreach (var (_, e) in range(wrappertests)) {
        if (e.result != e.i) {
            t.Errorf("%s: expected index %d; got %d", e.name, e.i, e.result);
        }
    }
}

internal static void runSearchWrappers() {
    SearchInts(data, 11);
    SearchFloat64s(fdata, 2.1D);
    SearchStrings(sdata, ""u8);
    ((sort.IntSlice)data).Search(0);
    ((sort.Float64Slice)fdata).Search(2.0D);
    ((sort.StringSlice)sdata).Search("x");
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingMallocCountInˢ = (@string)"skipping malloc count in short mode"u8;
internal static readonly object skippingGomaxprocs1ˢ = (@string)"skipping; GOMAXPROCS>1"u8;

public static void TestSearchWrappersDontAlloc(ж<testing.T> t) {
    if (testing.Short()) {
        t.Skip(skippingMallocCountInˢ);
    }
    if (Δruntime.GOMAXPROCS(0) > 1) {
        t.Skip(skippingGomaxprocs1ˢ);
    }
    var allocs = testing.AllocsPerRun(100, runSearchWrappers);
    if (allocs != 0D) {
        t.Errorf("expected no allocs for runSearchWrappers, got %v", allocs);
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
public static void TestSearchExhaustive(ж<testing.T> t) {
    for (nint size = 0; size <= 100; size++) {
        for (nint targᴛ1 = 0; targᴛ1 <= size; targᴛ1++) {
            var targ = targᴛ1;
            nint i = Search(size, (nint iΔ1) => iΔ1 >= targ);
            if (i != targ) {
                t.Errorf("Search(%d, %d) = %d", size, targ, i);
            }
        }
    }
}

// Abstract exhaustive test for Find.
public static void TestFindExhaustive(ж<testing.T> t) {
    // Test Find for different sequence sizes and search targets.
    // For each size, we have a (unmaterialized) sequence of integers:
    //   2,4...size*2
    // And we're looking for every possible integer between 1 and size*2 + 1.
    for (nint size = 0; size <= 100; size++) {
        for (nint xᴛ1 = 1; xᴛ1 <= size * 2 + 1; xᴛ1++) {
            var x = xᴛ1;
            bool wantFound = default;
            nint wantPos = default;

            var cmp = (nint i) => {
                // Encodes the unmaterialized sequence with elem[i] == (i+1)*2
                return x - (i + 1) * 2;
            };
            var (pos, found) = Find(size, cmp);

            if (x % 2 == 0) {
                wantPos = x / 2 - 1;
                wantFound = true;
            } else {
                wantPos = x / 2;
                wantFound = false;
            }
            if (found != wantFound || pos != wantPos) {
                t.Errorf("Find(%d, %d): got (%v, %v), want (%v, %v)", size, x, pos, found, wantPos, wantFound);
            }
        }
    }
}

} // end sort_test_package
