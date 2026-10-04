// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
global using BenchmarkCompactFunc_Large_Element = nint;

namespace go;

using cmp = cmp_package;
using asan = @internal.asan_package;
using msan = @internal.msan_package;
using race = @internal.race_package;
using testenv = @internal.testenv_package;
using Δmath = math_package;
using static slices_package;
using strings = strings_package;
using testing = testing_package;
using @unsafe = unsafe_package;
using @internal;
using ꓸꓸꓸany = Span<any>;

partial class slices_test_package {


[GoType("dyn")] partial struct equalIntTestsᴛ1 {
    internal slice<nint> s1, s2;
    internal bool want;
}
internal static slice<equalIntTestsᴛ1> equalIntTests = new equalIntTestsᴛ1[]{
    new(
        new nint[]{1}.slice(),
        default!,
        false
    ),
    new(
        new nint[]{}.slice(),
        default!,
        true
    ),
    new(
        new nint[]{1, 2, 3}.slice(),
        new nint[]{1, 2, 3}.slice(),
        true
    ),
    new(
        new nint[]{1, 2, 3}.slice(),
        new nint[]{1, 2, 3, 4}.slice(),
        false
    )
}.slice();


[GoType("dyn")] partial struct equalFloatTestsᴛ1 {
    internal slice<float64> s1, s2;
    internal bool wantEqual;
    internal bool wantEqualNaN;
}
internal static slice<equalFloatTestsᴛ1> equalFloatTests = new equalFloatTestsᴛ1[]{
    new(
        new float64[]{1D, 2D}.slice(),
        new float64[]{1D, 2D}.slice(),
        true,
        true
    ),
    new(
        new float64[]{1D, 2D, Δmath.NaN()}.slice(),
        new float64[]{1D, 2D, Δmath.NaN()}.slice(),
        false,
        true
    )
}.slice();

public static void TestEqual(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (_, test) in equalIntTests) {
        {
            var got = Equal<slice<nint>, nint>(test.s1, test.s2); if (got != test.want) {
                Ꮡt.Errorf("Equal(%v, %v) = %t, want %t"u8, test.s1, test.s2, got, test.want);
            }
        }
    }
    foreach (var (_, test) in equalFloatTests) {
        {
            var got = Equal<slice<float64>, float64>(test.s1, test.s2); if (got != test.wantEqual) {
                Ꮡt.Errorf("Equal(%v, %v) = %t, want %t"u8, test.s1, test.s2, got, test.wantEqual);
            }
        }
    }
}

// equal is simply ==.
internal static bool equal<T>(T v1, T v2) {
    return AreEqual(v1, v2);
}

// equalNaN is like == except that all NaNs are equal.
internal static bool equalNaN<T>(T v1, T v2) {
    bool isNaN(T f) => !AreEqual(f, f);
    return AreEqual(v1, v2) || (isNaN(v1) && isNaN(v2));
}

// offByOne returns true if integers v1 and v2 differ by 1.
internal static bool offByOne(nint v1, nint v2) {
    return v1 == v2 + 1 || v1 == v2 - 1;
}

public static void TestEqualFunc(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (_, test) in equalIntTests) {
        {
            var got = EqualFunc<slice<nint>, slice<nint>, nint, nint>(test.s1, test.s2, equal<nint>); if (got != test.want) {
                Ꮡt.Errorf("EqualFunc(%v, %v, equal[int]) = %t, want %t"u8, test.s1, test.s2, got, test.want);
            }
        }
    }
    foreach (var (_, test) in equalFloatTests) {
        {
            var got = EqualFunc<slice<float64>, slice<float64>, float64, float64>(test.s1, test.s2, equal<float64>); if (got != test.wantEqual) {
                Ꮡt.Errorf("Equal(%v, %v, equal[float64]) = %t, want %t"u8, test.s1, test.s2, got, test.wantEqual);
            }
        }
        {
            var got = EqualFunc<slice<float64>, slice<float64>, float64, float64>(test.s1, test.s2, equalNaN<float64>); if (got != test.wantEqualNaN) {
                Ꮡt.Errorf("Equal(%v, %v, equalNaN[float64]) = %t, want %t"u8, test.s1, test.s2, got, test.wantEqualNaN);
            }
        }
    }
    var s1 = new nint[]{1, 2, 3}.slice();
    var s2 = new nint[]{2, 3, 4}.slice();
    if (EqualFunc<slice<nint>, slice<nint>, nint, nint>(s1, s1, offByOne)) {
        Ꮡt.Errorf("EqualFunc(%v, %v, offByOne) = true, want false"u8, s1, s1);
    }
    if (!EqualFunc<slice<nint>, slice<nint>, nint, nint>(s1, s2, offByOne)) {
        Ꮡt.Errorf("EqualFunc(%v, %v, offByOne) = false, want true"u8, s1, s2);
    }
    var s3 = new @string[]{"a"u8, "b"u8, "c"u8}.slice();
    var s4 = new @string[]{"A"u8, "B"u8, "C"u8}.slice();
    if (!EqualFunc<slice<@string>, slice<@string>, @string, @string>(s3, s4, strings.EqualFold)) {
        Ꮡt.Errorf("EqualFunc(%v, %v, strings.EqualFold) = false, want true"u8, s3, s4);
    }
    var cmpIntString = (nint v1, @string v2) => ((@string)((rune)v1 - 1 + (rune)'a')) == v2;
    if (!EqualFunc(s1, s3, cmpIntString)) {
        Ꮡt.Errorf("EqualFunc(%v, %v, cmpIntString) = false, want true"u8, s1, s3);
    }
}

[GoLocalName("Large")] [GoType("[4096]byte")] /* [4 * 1024]byte */
internal partial struct BenchmarkEqualFunc_Large_Large;

public static void BenchmarkEqualFunc_Large(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var xs = GoReflect.WithElemDims(new slice<BenchmarkEqualFunc_Large_Large>(1024), 4096);
    var ys = GoReflect.WithElemDims(new slice<BenchmarkEqualFunc_Large_Large>(1024), 4096);
    for (nint i = 0; i < b.N; i++) {
        _ = EqualFunc(xs, ys, (BenchmarkEqualFunc_Large_Large x, BenchmarkEqualFunc_Large_Large y) => {
            x = x.Clone();
            y = y.Clone();
            return x == y;
        });
    }
}


[GoType("dyn")] partial struct compareIntTestsᴛ1 {
    internal slice<nint> s1, s2;
    internal nint want;
}
internal static slice<compareIntTestsᴛ1> compareIntTests = new compareIntTestsᴛ1[]{
    new(
        new nint[]{1}.slice(),
        new nint[]{1}.slice(),
        0
    ),
    new(
        new nint[]{1}.slice(),
        new nint[]{}.slice(),
        1
    ),
    new(
        new nint[]{}.slice(),
        new nint[]{1}.slice(),
        -1
    ),
    new(
        new nint[]{}.slice(),
        new nint[]{}.slice(),
        0
    ),
    new(
        new nint[]{1, 2, 3}.slice(),
        new nint[]{1, 2, 3}.slice(),
        0
    ),
    new(
        new nint[]{1, 2, 3}.slice(),
        new nint[]{1, 2, 3, 4}.slice(),
        -1
    ),
    new(
        new nint[]{1, 2, 3, 4}.slice(),
        new nint[]{1, 2, 3}.slice(),
        +1
    ),
    new(
        new nint[]{1, 2, 3}.slice(),
        new nint[]{1, 4, 3}.slice(),
        -1
    ),
    new(
        new nint[]{1, 4, 3}.slice(),
        new nint[]{1, 2, 3}.slice(),
        +1
    ),
    new(
        new nint[]{1, 4, 3}.slice(),
        new nint[]{1, 2, 3, 8, 9}.slice(),
        +1
    )
}.slice();


[GoType("dyn")] partial struct compareFloatTestsᴛ1 {
    internal slice<float64> s1, s2;
    internal nint want;
}
internal static slice<compareFloatTestsᴛ1> compareFloatTests = new compareFloatTestsᴛ1[]{
    new(
        new float64[]{}.slice(),
        new float64[]{}.slice(),
        0
    ),
    new(
        new float64[]{1D}.slice(),
        new float64[]{1D}.slice(),
        0
    ),
    new(
        new float64[]{Δmath.NaN()}.slice(),
        new float64[]{Δmath.NaN()}.slice(),
        0
    ),
    new(
        new float64[]{1D, 2D, Δmath.NaN()}.slice(),
        new float64[]{1D, 2D, Δmath.NaN()}.slice(),
        0
    ),
    new(
        new float64[]{1D, Δmath.NaN(), 3D}.slice(),
        new float64[]{1D, Δmath.NaN(), 4D}.slice(),
        -1
    ),
    new(
        new float64[]{1D, Δmath.NaN(), 3D}.slice(),
        new float64[]{1D, 2D, 4D}.slice(),
        -1
    ),
    new(
        new float64[]{1D, Δmath.NaN(), 3D}.slice(),
        new float64[]{1D, 2D, Δmath.NaN()}.slice(),
        -1
    ),
    new(
        new float64[]{1D, 2D, 3D}.slice(),
        new float64[]{1D, 2D, Δmath.NaN()}.slice(),
        +1
    ),
    new(
        new float64[]{1D, 2D, 3D}.slice(),
        new float64[]{1D, Δmath.NaN(), 3D}.slice(),
        +1
    ),
    new(
        new float64[]{1D, Δmath.NaN(), 3D, 4D}.slice(),
        new float64[]{1D, 2D, Δmath.NaN()}.slice(),
        -1
    )
}.slice();

public static void TestCompare(ж<testing.T> Ꮡt) {
    @string intWant(bool want) {
        if (want) {
            return "0"u8;
        }
        return "!= 0"u8;
    }
    foreach (var (_, test) in equalIntTests) {
        {
            nint got = Compare<slice<nint>, nint>(test.s1, test.s2); if ((got == 0) != test.want) {
                Ꮡt.Errorf("Compare(%v, %v) = %d, want %s"u8, test.s1, test.s2, got, intWant(test.want));
            }
        }
    }
    foreach (var (_, test) in equalFloatTests) {
        {
            nint got = Compare<slice<float64>, float64>(test.s1, test.s2); if ((got == 0) != test.wantEqualNaN) {
                Ꮡt.Errorf("Compare(%v, %v) = %d, want %s"u8, test.s1, test.s2, got, intWant(test.wantEqualNaN));
            }
        }
    }
    foreach (var (_, test) in compareIntTests) {
        {
            nint got = Compare<slice<nint>, nint>(test.s1, test.s2); if (got != test.want) {
                Ꮡt.Errorf("Compare(%v, %v) = %d, want %d"u8, test.s1, test.s2, got, test.want);
            }
        }
    }
    foreach (var (_, test) in compareFloatTests) {
        {
            nint got = Compare<slice<float64>, float64>(test.s1, test.s2); if (got != test.want) {
                Ꮡt.Errorf("Compare(%v, %v) = %d, want %d"u8, test.s1, test.s2, got, test.want);
            }
        }
    }
}

internal static Func<T, T, nint> equalToCmp<T>(Func<T, T, bool> eq) {
    return (T v1, T v2) => {
        if (eq(v1, v2)) {
            return 0;
        }
        return 1;
    };
}

public static void TestCompareFunc(ж<testing.T> Ꮡt) {
    @string intWant(bool want) {
        if (want) {
            return "0"u8;
        }
        return "!= 0"u8;
    }
    foreach (var (_, test) in equalIntTests) {
        {
            nint got = CompareFunc(test.s1, test.s2, equalToCmp<nint>(equal<nint>)); if ((got == 0) != test.want) {
                Ꮡt.Errorf("CompareFunc(%v, %v, equalToCmp(equal[int])) = %d, want %s"u8, test.s1, test.s2, got, intWant(test.want));
            }
        }
    }
    foreach (var (_, test) in equalFloatTests) {
        {
            nint got = CompareFunc(test.s1, test.s2, equalToCmp<float64>(equal<float64>)); if ((got == 0) != test.wantEqual) {
                Ꮡt.Errorf("CompareFunc(%v, %v, equalToCmp(equal[float64])) = %d, want %s"u8, test.s1, test.s2, got, intWant(test.wantEqual));
            }
        }
    }
    foreach (var (_, test) in compareIntTests) {
        {
            nint got = CompareFunc<slice<nint>, slice<nint>, nint, nint>(test.s1, test.s2, cmp.Compare<nint>); if (got != test.want) {
                Ꮡt.Errorf("CompareFunc(%v, %v, cmp[int]) = %d, want %d"u8, test.s1, test.s2, got, test.want);
            }
        }
    }
    foreach (var (_, test) in compareFloatTests) {
        {
            nint got = CompareFunc<slice<float64>, slice<float64>, float64, float64>(test.s1, test.s2, cmp.Compare<float64>); if (got != test.want) {
                Ꮡt.Errorf("CompareFunc(%v, %v, cmp[float64]) = %d, want %d"u8, test.s1, test.s2, got, test.want);
            }
        }
    }
    var s1 = new nint[]{1, 2, 3}.slice();
    var s2 = new nint[]{2, 3, 4}.slice();
    {
        nint got = CompareFunc(s1, s2, equalToCmp<nint>(offByOne)); if (got != 0) {
            Ꮡt.Errorf("CompareFunc(%v, %v, offByOne) = %d, want 0"u8, s1, s2, got);
        }
    }
    var s3 = new @string[]{"a"u8, "b"u8, "c"u8}.slice();
    var s4 = new @string[]{"A"u8, "B"u8, "C"u8}.slice();
    {
        nint got = CompareFunc<slice<@string>, slice<@string>, @string, @string>(s3, s4, strings.Compare); if (got != 1) {
            Ꮡt.Errorf("CompareFunc(%v, %v, strings.Compare) = %d, want 1"u8, s3, s4, got);
        }
    }
    var compareLower = (@string v1, @string v2) => strings.Compare(strings.ToLower(v1), strings.ToLower(v2));
    {
        nint got = CompareFunc(s3, s4, compareLower); if (got != 0) {
            Ꮡt.Errorf("CompareFunc(%v, %v, compareLower) = %d, want 0"u8, s3, s4, got);
        }
    }
    var cmpIntString = (nint v1, @string v2) => strings.Compare(((@string)((rune)v1 - 1 + (rune)'a')), v2);
    {
        nint got = CompareFunc(s1, s3, cmpIntString); if (got != 0) {
            Ꮡt.Errorf("CompareFunc(%v, %v, cmpIntString) = %d, want 0"u8, s1, s3, got);
        }
    }
}


[GoType("dyn")] partial struct indexTestsᴛ1 {
    internal slice<nint> s;
    internal nint v;
    internal nint want;
}
internal static slice<indexTestsᴛ1> indexTests = new indexTestsᴛ1[]{
    new(
        default!,
        0,
        -1
    ),
    new(
        new nint[]{}.slice(),
        0,
        -1
    ),
    new(
        new nint[]{1, 2, 3}.slice(),
        2,
        1
    ),
    new(
        new nint[]{1, 2, 2, 3}.slice(),
        2,
        1
    ),
    new(
        new nint[]{1, 2, 3, 2}.slice(),
        2,
        1
    )
}.slice();

public static void TestIndex(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in indexTests) {
        {
            nint got = Index(test.s, test.v); if (got != test.want) {
                Ꮡt.Errorf("Index(%v, %v) = %d, want %d"u8, test.s, test.v, got, test.want);
            }
        }
    }
}

internal static Func<T, bool> equalToIndex<T>(Func<T, T, bool> f, T v1) {
    return (T v2) => f(v1, v2);
}

[GoLocalName("Large")] [GoType("[4096]byte")] /* [4 * 1024]byte */
internal partial struct BenchmarkIndex_Large_Large;

public static void BenchmarkIndex_Large(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var ss = GoReflect.WithElemDims(new slice<BenchmarkIndex_Large_Large>(1024), 4096);
    for (nint i = 0; i < b.N; i++) {
        _ = Index(ss, new BenchmarkIndex_Large_Large(new byte[]{1}.array(4096)));
    }
}

public static void TestIndexFunc(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in indexTests) {
        {
            nint got = IndexFunc(test.s, equalToIndex<nint>(equal<nint>, test.v)); if (got != test.want) {
                Ꮡt.Errorf("IndexFunc(%v, equalToIndex(equal[int], %v)) = %d, want %d"u8, test.s, test.v, got, test.want);
            }
        }
    }
    var s1 = new @string[]{"hi"u8, "HI"u8}.slice();
    {
        nint got = IndexFunc(s1, equalToIndex<@string>(equal<@string>, (@string)"HI")); if (got != 1) {
            Ꮡt.Errorf("IndexFunc(%v, equalToIndex(equal[string], %q)) = %d, want %d"u8, s1, (@string)"HI"u8, got, (nint)(1));
        }
    }
    {
        nint got = IndexFunc(s1, equalToIndex<@string>(strings.EqualFold, (@string)"HI")); if (got != 0) {
            Ꮡt.Errorf("IndexFunc(%v, equalToIndex(strings.EqualFold, %q)) = %d, want %d"u8, s1, (@string)"HI"u8, got, (nint)(0));
        }
    }
}

[GoLocalName("Large")] [GoType("[4096]byte")] /* [4 * 1024]byte */
internal partial struct BenchmarkIndexFunc_Large_Large;

public static void BenchmarkIndexFunc_Large(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var ss = GoReflect.WithElemDims(new slice<BenchmarkIndexFunc_Large_Large>(1024), 4096);
    for (nint i = 0; i < b.N; i++) {
        _ = IndexFunc(ss, (BenchmarkIndexFunc_Large_Large e) => {
            e = e.Clone();
            return e == new BenchmarkIndexFunc_Large_Large(new byte[]{1}.array(4096));
        });
    }
}

public static void TestContains(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (_, test) in indexTests) {
        {
            var got = Contains(test.s, test.v); if (got != (test.want != -1)) {
                Ꮡt.Errorf("Contains(%v, %v) = %t, want %t"u8, test.s, test.v, got, test.want != -1);
            }
        }
    }
}

public static void TestContainsFunc(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (_, test) in indexTests) {
        {
            var got = ContainsFunc(test.s, equalToIndex<nint>(equal<nint>, test.v)); if (got != (test.want != -1)) {
                Ꮡt.Errorf("ContainsFunc(%v, equalToIndex(equal[int], %v)) = %t, want %t"u8, test.s, test.v, got, test.want != -1);
            }
        }
    }
    var s1 = new @string[]{"hi"u8, "HI"u8}.slice();
    {
        var got = ContainsFunc(s1, equalToIndex<@string>(equal<@string>, (@string)"HI")); if (got != true) {
            Ꮡt.Errorf("ContainsFunc(%v, equalToContains(equal[string], %q)) = %t, want %t"u8, s1, (@string)"HI"u8, got, true);
        }
    }
    {
        var got = ContainsFunc(s1, equalToIndex<@string>(equal<@string>, (@string)"hI")); if (got != false) {
            Ꮡt.Errorf("ContainsFunc(%v, equalToContains(strings.EqualFold, %q)) = %t, want %t"u8, s1, (@string)"hI"u8, got, false);
        }
    }
    {
        var got = ContainsFunc(s1, equalToIndex<@string>(strings.EqualFold, (@string)"hI")); if (got != true) {
            Ꮡt.Errorf("ContainsFunc(%v, equalToContains(strings.EqualFold, %q)) = %t, want %t"u8, s1, (@string)"hI"u8, got, true);
        }
    }
}


[GoType("dyn")] partial struct insertTestsᴛ1 {
    internal slice<nint> s;
    internal nint i;
    internal slice<nint> add;
    internal slice<nint> want;
}
internal static slice<insertTestsᴛ1> insertTests = new insertTestsᴛ1[]{
    new(
        new nint[]{1, 2, 3}.slice(),
        0,
        new nint[]{4}.slice(),
        new nint[]{4, 1, 2, 3}.slice()
    ),
    new(
        new nint[]{1, 2, 3}.slice(),
        1,
        new nint[]{4}.slice(),
        new nint[]{1, 4, 2, 3}.slice()
    ),
    new(
        new nint[]{1, 2, 3}.slice(),
        3,
        new nint[]{4}.slice(),
        new nint[]{1, 2, 3, 4}.slice()
    ),
    new(
        new nint[]{1, 2, 3}.slice(),
        2,
        new nint[]{4, 5}.slice(),
        new nint[]{1, 2, 4, 5, 3}.slice()
    )
}.slice();

public static void TestInsert(ж<testing.T> Ꮡt) {
    var s = new nint[]{1, 2, 3}.slice();
    {
        var got = Insert<slice<nint>, nint>(s, 0); if (!Equal<slice<nint>, nint>(got, s)) {
            Ꮡt.Errorf("Insert(%v, 0) = %v, want %v"u8, s, got, s);
        }
    }
    foreach (var (_, test) in insertTests) {
        var copy = Clone<slice<nint>, nint>(test.s);
        {
            var got = Insert(copy, test.i, test.add.ꓸꓸꓸ); if (!Equal<slice<nint>, nint>(got, test.want)) {
                Ꮡt.Errorf("Insert(%v, %d, %v...) = %v, want %v"u8, test.s, test.i, test.add, got, test.want);
            }
        }
    }
    if (!testenv.OptimizationOff() && !race.Enabled && !asan.Enabled && !msan.Enabled) {
        // Allocations should be amortized.
        UntypedInt count = 50;
        var n = testing.AllocsPerRun(10, () => {
            var sΔ1 = new nint[]{1, 2, 3}.slice();
            for (nint i = 0; i < count; i++) {
                sΔ1 = Insert(sΔ1, 0, (nint)(1));
            }
        });
        if (n > /* count / 2 */ 25D) {
            Ꮡt.Errorf("too many allocations inserting %d elements: got %v, want less than %d"u8, (nint)(count), n, (nint)(count / 2));
        }
    }
}

public static void TestInsertOverlap(ж<testing.T> Ꮡt) {
    UntypedInt N = 10;
    var a = new slice<nint>(N);
    var want = new slice<nint>(2 * N);
    for (nint n = 0; n <= N; n++) {
        // length
        for (nint i = 0; i <= n; i++) {
            // insertion point
            for (nint x = 0; x <= N; x++) {
                // start of inserted data
                for (nint y = x; y <= N; y++) {
                    // end of inserted data
                    for (nint k = 0; k < N; k++) {
                        a[k] = k;
                    }
                    want = want[..0];
                    want = appendꓸꓸꓸ(want, a.slice(0, i));
                    want = appendꓸꓸꓸ(want, a.slice(x, y));
                    want = appendꓸꓸꓸ(want, a.slice(i, n));
                    var got = Insert(a.slice(0, n), i, a.slice(x, y).ꓸꓸꓸ);
                    if (!Equal<slice<nint>, nint>(got, want)) {
                        Ꮡt.Errorf("Insert with overlap failed n=%d i=%d x=%d y=%d, got %v want %v"u8, n, i, x, y, got, want);
                    }
                }
            }
        }
    }
}

[GoType("dyn")] internal partial struct TestInsertPanics_type {
    internal @string name;
    internal slice<nint> s;
    internal nint i;
    internal slice<nint> v;
}

public static void TestInsertPanics(ж<testing.T> Ꮡt) {
    var a = new nint[]{}.array(3);
    var b = new nint[]{}.array(1);
    foreach (var (_, vᴛ1) in new TestInsertPanics_type[]{ // There are no values.

        new("with negative index"u8, a.slice(0, 1, 1), -1, default!),
        new("with out-of-bounds index and > cap"u8, a.slice(0, 1, 1), 2, default!),
        new("with out-of-bounds index and = cap"u8, a.slice(0, 1, 2), 2, default!),
        new("with out-of-bounds index and < cap"u8, a.slice(0, 1, 3), 2, default!), // There are values.

        new("with negative index"u8, a.slice(0, 1, 1), -1, b[..]),
        new("with out-of-bounds index and > cap"u8, a.slice(0, 1, 1), 2, b[..]),
        new("with out-of-bounds index and = cap"u8, a.slice(0, 1, 2), 2, b[..]),
        new("with out-of-bounds index and < cap"u8, a.slice(0, 1, 3), 2, b[..])
    }.slice()) {
        ref var test = ref heap(new TestInsertPanics_type(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        if (!panics(() => {
            _ = Insert(testʗ1.s, testʗ1.i, testʗ1.v.ꓸꓸꓸ);
        })) {
            Ꮡt.Errorf("Insert %s: got no panic, want panic"u8, test.name);
        }
    }
}


[GoType("dyn")] partial struct deleteTestsᴛ1 {
    internal slice<nint> s;
    internal nint i, j;
    internal slice<nint> want;
}
internal static slice<deleteTestsᴛ1> deleteTests = new deleteTestsᴛ1[]{
    new(
        new nint[]{1, 2, 3}.slice(),
        0,
        0,
        new nint[]{1, 2, 3}.slice()
    ),
    new(
        new nint[]{1, 2, 3}.slice(),
        0,
        1,
        new nint[]{2, 3}.slice()
    ),
    new(
        new nint[]{1, 2, 3}.slice(),
        3,
        3,
        new nint[]{1, 2, 3}.slice()
    ),
    new(
        new nint[]{1, 2, 3}.slice(),
        0,
        2,
        new nint[]{3}.slice()
    ),
    new(
        new nint[]{1, 2, 3}.slice(),
        0,
        3,
        new nint[]{}.slice()
    )
}.slice();

public static void TestDelete(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in deleteTests) {
        var copy = Clone<slice<nint>, nint>(test.s);
        {
            var got = Delete<slice<nint>, nint>(copy, test.i, test.j); if (!Equal<slice<nint>, nint>(got, test.want)) {
                Ꮡt.Errorf("Delete(%v, %d, %d) = %v, want %v"u8, test.s, test.i, test.j, got, test.want);
            }
        }
    }
}


[GoType("dyn")] partial struct deleteFuncTestsᴛ1 {
    internal slice<nint> s;
    internal Func<nint, bool> fn;
    internal slice<nint> want;
}
internal static slice<deleteFuncTestsᴛ1> deleteFuncTests = new deleteFuncTestsᴛ1[]{
    new(
        default!,
        (nint _) => true,
        default!
    ),
    new(
        new nint[]{1, 2, 3}.slice(),
        (nint _) => true,
        default!
    ),
    new(
        new nint[]{1, 2, 3}.slice(),
        (nint _) => false,
        new nint[]{1, 2, 3}.slice()
    ),
    new(
        new nint[]{1, 2, 3}.slice(),
        (nint i) => i > 2,
        new nint[]{1, 2}.slice()
    ),
    new(
        new nint[]{1, 2, 3}.slice(),
        (nint i) => i < 2,
        new nint[]{2, 3}.slice()
    ),
    new(
        new nint[]{10, 2, 30}.slice(),
        (nint i) => i >= 10,
        new nint[]{2}.slice()
    )
}.slice();

public static void TestDeleteFunc(ж<testing.T> Ꮡt) {
    foreach (var (i, test) in deleteFuncTests) {
        var copy = Clone<slice<nint>, nint>(test.s);
        {
            var got = DeleteFunc(copy, test.fn); if (!Equal<slice<nint>, nint>(got, test.want)) {
                Ꮡt.Errorf("DeleteFunc case %d: got %v, want %v"u8, i, got, test.want);
            }
        }
    }
}

internal static bool /*b*/ panics(Action f) {
    bool b = default!;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var x = recover(); if (x != default!) {
                    b = true;
                }
            }
        }, ref ᒐ);
        f();
        b = false;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return b;
}

[GoType("dyn")] internal partial struct TestDeletePanics_type {
    internal @string name;
    internal slice<nint> s;
    internal nint i, j;
}

public static void TestDeletePanics(ж<testing.T> Ꮡt) {
    var s = new nint[]{0, 1, 2, 3, 4}.slice();
    s = s[0..2];
    _ = s[0..4]; // this is a valid slice of s
    foreach (var (_, vᴛ1) in new TestDeletePanics_type[]{
        new("with negative first index"u8, new nint[]{42}.slice(), -2, 1),
        new("with negative second index"u8, new nint[]{42}.slice(), 1, -1),
        new("with out-of-bounds first index"u8, new nint[]{42}.slice(), 2, 3),
        new("with out-of-bounds second index"u8, new nint[]{42}.slice(), 0, 2),
        new("with out-of-bounds both indexes"u8, new nint[]{42}.slice(), 2, 2),
        new("with invalid i>j"u8, new nint[]{42}.slice(), 1, 0),
        new("s[i:j] is valid and j > len(s)"u8, s, 0, 4),
        new("s[i:j] is valid and i == j > len(s)"u8, s, 3, 3)
    }.slice()) {
        ref var test = ref heap(new TestDeletePanics_type(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        if (!panics(() => {
            _ = Delete<slice<nint>, nint>(testʗ1.s, testʗ1.i, testʗ1.j);
        })) {
            Ꮡt.Errorf("Delete %s: got no panic, want panic"u8, test.name);
        }
    }
}

public static void TestDeleteClearTail(ж<testing.T> Ꮡt) {
    var mem = new ж<nint>[]{@new<nint>(), @new<nint>(), @new<nint>(), @new<nint>(), @new<nint>(), @new<nint>()}.slice();
    var s = mem[0..5]; // there is 1 element beyond len(s), within cap(s)
    s = Delete<slice<ж<nint>>, ж<nint>>(s, 2, 4);
    if (mem[3] != nil || mem[4] != nil) {
        // Check that potential memory leak is avoided
        Ꮡt.Errorf("Delete: want nil discarded elements, got %v, %v"u8, mem[3].OrTypedNil(), mem[4].OrTypedNil());
    }
    if (mem[5] == nil) {
        Ꮡt.Errorf("Delete: want unchanged elements beyond original len, got nil"u8);
    }
}

public static void TestDeleteFuncClearTail(ж<testing.T> Ꮡt) {
    var mem = new ж<nint>[]{@new<nint>(), @new<nint>(), @new<nint>(), @new<nint>(), @new<nint>(), @new<nint>()}.slice();
    (mem[2].Value, mem[3].Value) = (42, 42);
    var s = mem[0..5]; // there is 1 element beyond len(s), within cap(s)
    s = DeleteFunc(s, (ж<nint> i) => i != nil && i.Value == 42);
    if (mem[3] != nil || mem[4] != nil) {
        // Check that potential memory leak is avoided
        Ꮡt.Errorf("DeleteFunc: want nil discarded elements, got %v, %v"u8, mem[3].OrTypedNil(), mem[4].OrTypedNil());
    }
    if (mem[5] == nil) {
        Ꮡt.Errorf("DeleteFunc: want unchanged elements beyond original len, got nil"u8);
    }
}

public static void TestClone(ж<testing.T> Ꮡt) {
    var s1 = new nint[]{1, 2, 3}.slice();
    var s2 = Clone<slice<nint>, nint>(s1);
    if (!Equal<slice<nint>, nint>(s1, s2)) {
        Ꮡt.Errorf("Clone(%v) = %v, want %v"u8, s1, s2, s1);
    }
    s1[0] = 4;
    var want = new nint[]{1, 2, 3}.slice();
    if (!Equal<slice<nint>, nint>(s2, want)) {
        Ꮡt.Errorf("Clone(%v) changed unexpectedly to %v"u8, want, s2);
    }
    {
        var got = Clone<slice<nint>, nint>(slice<nint>(default!)); if (got != default!) {
            Ꮡt.Errorf("Clone(nil) = %#v, want nil"u8, got);
        }
    }
    {
        var got = Clone<slice<nint>, nint>(s1[..0]); if (got == default! || len(got) != 0) {
            Ꮡt.Errorf("Clone(%v) = %#v, want %#v"u8, s1[..0], got, s1[..0]);
        }
    }
}


[GoType("dyn")] partial struct compactTestsᴛ1 {
    internal @string name;
    internal slice<nint> s;
    internal slice<nint> want;
}
internal static slice<compactTestsᴛ1> compactTests = new compactTestsᴛ1[]{
    new(
        "nil"u8,
        default!,
        default!
    ),
    new(
        "one"u8,
        new nint[]{1}.slice(),
        new nint[]{1}.slice()
    ),
    new(
        "sorted"u8,
        new nint[]{1, 2, 3}.slice(),
        new nint[]{1, 2, 3}.slice()
    ),
    new(
        "2 items"u8,
        new nint[]{1, 1, 2}.slice(),
        new nint[]{1, 2}.slice()
    ),
    new(
        "unsorted"u8,
        new nint[]{1, 2, 1}.slice(),
        new nint[]{1, 2, 1}.slice()
    ),
    new(
        "many"u8,
        new nint[]{1, 2, 2, 3, 3, 4}.slice(),
        new nint[]{1, 2, 3, 4}.slice()
    )
}.slice();

public static void TestCompact(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in compactTests) {
        var copy = Clone<slice<nint>, nint>(test.s);
        {
            var got = Compact<slice<nint>, nint>(copy); if (!Equal<slice<nint>, nint>(got, test.want)) {
                Ꮡt.Errorf("Compact(%v) = %v, want %v"u8, test.s, got, test.want);
            }
        }
    }
}

public static void BenchmarkCompact(ж<testing.B> Ꮡb) {
    foreach (var (_, vᴛ1) in compactTests) {
        ref var c = ref heap(new compactTestsᴛ1(), out var Ꮡc);
        c = vᴛ1;

        var cʗ1 = c;
        Ꮡb.Run(c.name, (ж<testing.B> bΔ1) => {
            var ss = new slice<nint>(0, 64);
            for (nint k = 0; k < (~bΔ1).N; k++) {
                ss = ss[..0];
                ss = appendꓸꓸꓸ(ss, cʗ1.s);
                _ = Compact<slice<nint>, nint>(ss);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string allDupˢ = "all_dup"u8;
private static readonly @string noDupˢ = "no_dup"u8;

[GoLocalName("Large")] [GoType("[16]nint")] internal partial struct BenchmarkCompact_Large_Large;

public static void BenchmarkCompact_Large(ж<testing.B> Ꮡb) {
    const nint N = 1024;
    Ꮡb.Run(allDupˢ, (ж<testing.B> bΔ1) => {
        var ss = GoReflect.WithElemDims(new slice<BenchmarkCompact_Large_Large>(N), 16);
        bΔ1.ResetTimer();
        for (nint i = 0; i < (~bΔ1).N; i++) {
            _ = Compact<slice<BenchmarkCompact_Large_Large>, BenchmarkCompact_Large_Large>(ss);
        }
    });
    Ꮡb.Run(noDupˢ, (ж<testing.B> bΔ2) => {
        var ss = GoReflect.WithElemDims(new slice<BenchmarkCompact_Large_Large>(N), 16);
        foreach (var (i, _) in ss) {
            ss[i][0] = i;
        }
        bΔ2.ResetTimer();
        for (nint i = 0; i < (~bΔ2).N; i++) {
            _ = Compact<slice<BenchmarkCompact_Large_Large>, BenchmarkCompact_Large_Large>(ss);
        }
    });
}

public static void TestCompactFunc(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in compactTests) {
        var copyΔ1 = Clone<slice<nint>, nint>(test.s);
        {
            var got = CompactFunc<slice<nint>, nint>(copyΔ1, equal<nint>); if (!Equal<slice<nint>, nint>(got, test.want)) {
                Ꮡt.Errorf("CompactFunc(%v, equal[int]) = %v, want %v"u8, test.s, got, test.want);
            }
        }
    }
    var s1 = new @string[]{"a"u8, "a"u8, "A"u8, "B"u8, "b"u8}.slice();
    var copy = Clone<slice<@string>, @string>(s1);
    var want = new @string[]{"a"u8, "B"u8}.slice();
    {
        var got = CompactFunc<slice<@string>, @string>(copy, strings.EqualFold); if (!Equal<slice<@string>, @string>(got, want)) {
            Ꮡt.Errorf("CompactFunc(%v, strings.EqualFold) = %v, want %v"u8, s1, got, want);
        }
    }
}

public static void TestCompactClearTail(ж<testing.T> Ꮡt) {
    ref var one = ref heap<nint>(out var Ꮡone);
    one = 1;
    ref var two = ref heap<nint>(out var Ꮡtwo);
    two = 2;
    ref var three = ref heap<nint>(out var Ꮡthree);
    three = 3;
    ref var four = ref heap<nint>(out var Ꮡfour);
    four = 4;
    var mem = new ж<nint>[]{Ꮡone, Ꮡone, Ꮡtwo, Ꮡtwo, Ꮡthree, Ꮡfour}.slice();
    var s = mem[0..5]; // there is 1 element beyond len(s), within cap(s)
    var copy = Clone<slice<ж<nint>>, ж<nint>>(s);
    s = Compact<slice<ж<nint>>, ж<nint>>(s);
    {
        var want = new ж<nint>[]{Ꮡone, Ꮡtwo, Ꮡthree}.slice(); if (!Equal<slice<ж<nint>>, ж<nint>>(s, want)) {
            Ꮡt.Errorf("Compact(%v) = %v, want %v"u8, copy, s, want);
        }
    }
    if (mem[3] != nil || mem[4] != nil) {
        // Check that potential memory leak is avoided
        Ꮡt.Errorf("Compact: want nil discarded elements, got %v, %v"u8, mem[3].OrTypedNil(), mem[4].OrTypedNil());
    }
    if (mem[5] != Ꮡfour) {
        Ꮡt.Errorf("Compact: want unchanged element beyond original len, got %v"u8, mem[5].OrTypedNil());
    }
}

public static void TestCompactFuncClearTail(ж<testing.T> Ꮡt) {
    ref var a = ref heap<nint>(out var Ꮡa);
    a = 1;
    ref var b = ref heap<nint>(out var Ꮡb);
    b = 1;
    ref var c = ref heap<nint>(out var Ꮡc);
    c = 2;
    ref var d = ref heap<nint>(out var Ꮡd);
    d = 2;
    ref var e = ref heap<nint>(out var Ꮡe);
    e = 3;
    ref var f = ref heap<nint>(out var Ꮡf);
    f = 4;
    var mem = new ж<nint>[]{Ꮡa, Ꮡb, Ꮡc, Ꮡd, Ꮡe, Ꮡf}.slice();
    var s = mem[0..5]; // there is 1 element beyond len(s), within cap(s)
    var copy = Clone<slice<ж<nint>>, ж<nint>>(s);
    s = CompactFunc(s, (ж<nint> x, ж<nint> y) => {
        if (x == nil || y == nil) {
            return x == y;
        }
        return x.Value == y.Value;
    });
    {
        var want = new ж<nint>[]{Ꮡa, Ꮡc, Ꮡe}.slice(); if (!Equal<slice<ж<nint>>, ж<nint>>(s, want)) {
            Ꮡt.Errorf("CompactFunc(%v) = %v, want %v"u8, copy, s, want);
        }
    }
    if (mem[3] != nil || mem[4] != nil) {
        // Check that potential memory leak is avoided
        Ꮡt.Errorf("CompactFunc: want nil discarded elements, got %v, %v"u8, mem[3].OrTypedNil(), mem[4].OrTypedNil());
    }
    if (mem[5] != Ꮡf) {
        Ꮡt.Errorf("CompactFunc: want unchanged elements beyond original len, got %v"u8, mem[5].OrTypedNil());
    }
}

public static void BenchmarkCompactFunc(ж<testing.B> Ꮡb) {
    foreach (var (_, vᴛ1) in compactTests) {
        ref var c = ref heap(new compactTestsᴛ1(), out var Ꮡc);
        c = vᴛ1;

        var cʗ1 = c;
        Ꮡb.Run(c.name, (ж<testing.B> bΔ1) => {
            var ss = new slice<nint>(0, 64);
            for (nint k = 0; k < (~bΔ1).N; k++) {
                ss = ss[..0];
                ss = appendꓸꓸꓸ(ss, cʗ1.s);
                _ = CompactFunc(ss, (nint a, nint bΔ2) => a == bΔ2);
            }
        });
    }
}

public static void BenchmarkCompactFunc_Large(ж<testing.B> Ꮡb) {
    const nint N = /* 1024 * 1024 */ 1048576;
    Ꮡb.Run(allDupˢ, (ж<testing.B> bΔ1) => {
        var ss = new slice<BenchmarkCompactFunc_Large_Element>(N);
        bΔ1.ResetTimer();
        for (nint i = 0; i < (~bΔ1).N; i++) {
            _ = CompactFunc(ss, (BenchmarkCompactFunc_Large_Element a, BenchmarkCompactFunc_Large_Element bΔ2) => a == bΔ2);
        }
    });
    Ꮡb.Run(noDupˢ, (ж<testing.B> bΔ3) => {
        var ss = new slice<BenchmarkCompactFunc_Large_Element>(N);
        foreach (var (i, _) in ss) {
            ss[i] = i;
        }
        bΔ3.ResetTimer();
        for (nint i = 0; i < (~bΔ3).N; i++) {
            _ = CompactFunc(ss, (BenchmarkCompactFunc_Large_Element a, BenchmarkCompactFunc_Large_Element bΔ4) => a == bΔ4);
        }
    });
}

public static void TestGrow(ж<testing.T> Ꮡt) {
    var s1 = new nint[]{1, 2, 3}.slice();
    var copy = Clone<slice<nint>, nint>(s1);
    var s2 = Grow<slice<nint>, nint>(copy, 1000);
    if (!Equal<slice<nint>, nint>(s1, s2)) {
        Ꮡt.Errorf("Grow(%v) = %v, want %v"u8, s1, s2, s1);
    }
    if (cap(s2) < 1000 + len(s1)) {
        Ꮡt.Errorf("after Grow(%v) cap = %d, want >= %d"u8, s1, cap(s2), 1000 + len(s1));
    }
    // Test mutation of elements between length and capacity.
    copy = Clone<slice<nint>, nint>(s1);
    var s3 = Grow<slice<nint>, nint>(copy[..1], 2)[..3];
    if (!Equal<slice<nint>, nint>(s1, s3)) {
        Ꮡt.Errorf("Grow should not mutate elements between length and capacity"u8);
    }
    s3 = Grow<slice<nint>, nint>(copy[..1], 1000)[..3];
    if (!Equal<slice<nint>, nint>(s1, s3)) {
        Ꮡt.Errorf("Grow should not mutate elements between length and capacity"u8);
    }
    // Test number of allocations.
    {
        var s2ʗ1 = s2;
        var n = testing.AllocsPerRun(100, () => {
            _ = Grow<slice<nint>, nint>(s2ʗ1, cap(s2ʗ1) - len(s2ʗ1));
        }); if (n != 0D) {
            Ꮡt.Errorf("Grow should not allocate when given sufficient capacity; allocated %v times"u8, n);
        }
    }
    {
        var s2ʗ2 = s2;
        var n = testing.AllocsPerRun(100, () => {
            _ = Grow<slice<nint>, nint>(s2ʗ2, cap(s2ʗ2) - len(s2ʗ2) + 1);
        }); if (n != 1D) {
            
            Actionꓸꓸꓸ<@string, any> errorf = (@string p1, params ꓸꓸꓸany p2) => Ꮡt.Errorf(p1, p2);
            if (race.Enabled || msan.Enabled || asan.Enabled || testenv.OptimizationOff()) {
                                errorf = (@string p1, params ꓸꓸꓸany p2) => Ꮡt.Logf(p1, p2); // this allocates multiple times in race detector mode
            }
            errorf("Grow should allocate once when given insufficient capacity; allocated %v times"u8, n);
        }
    }
    // Test for negative growth sizes.
    bool gotPanic = default!;
    var s1ʗ1 = s1;
    ((Action)(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                gotPanic = recover() != default!;
            }, ref ᒐ);
            _ = Grow<slice<nint>, nint>(s1ʗ1, -1);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }))();
    if (!gotPanic) {
        Ꮡt.Errorf("Grow(-1) did not panic; expected a panic"u8);
    }
}

public static void TestClip(ж<testing.T> Ꮡt) {
    var s1 = new nint[]{1, 2, 3, 4, 5, 6}.slice()[..3];
    var orig = Clone<slice<nint>, nint>(s1);
    if (len(s1) != 3) {
        Ꮡt.Errorf("len(%v) = %d, want 3"u8, s1, len(s1));
    }
    if (cap(s1) < 6) {
        Ꮡt.Errorf("cap(%v[:3]) = %d, want >= 6"u8, orig, cap(s1));
    }
    var s2 = Clip<slice<nint>, nint>(s1);
    if (!Equal<slice<nint>, nint>(s1, s2)) {
        Ꮡt.Errorf("Clip(%v) = %v, want %v"u8, s1, s2, s1);
    }
    if (cap(s2) != 3) {
        Ꮡt.Errorf("cap(Clip(%v)) = %d, want 3"u8, orig, cap(s2));
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string oneTwoThreeˢ = "one two three"u8;
private static readonly @string threeTwoOneˢ = "three two one"u8;

public static void TestReverse(ж<testing.T> Ꮡt) {
    var even = new nint[]{3, 1, 4, 1, 5, 9}.slice(); // len = 6
    Reverse<slice<nint>, nint>(even);
    {
        var want = new nint[]{9, 5, 1, 4, 1, 3}.slice(); if (!Equal<slice<nint>, nint>(even, want)) {
            Ꮡt.Errorf("Reverse(even) = %v, want %v"u8, even, want);
        }
    }
    var odd = new nint[]{3, 1, 4, 1, 5, 9, 2}.slice(); // len = 7
    Reverse<slice<nint>, nint>(odd);
    {
        var want = new nint[]{2, 9, 5, 1, 4, 1, 3}.slice(); if (!Equal<slice<nint>, nint>(odd, want)) {
            Ꮡt.Errorf("Reverse(odd) = %v, want %v"u8, odd, want);
        }
    }
    var words = strings.Fields(oneTwoThreeˢ);
    Reverse<slice<@string>, @string>(words);
    {
        var want = strings.Fields(threeTwoOneˢ); if (!Equal<slice<@string>, @string>(words, want)) {
            Ꮡt.Errorf("Reverse(words) = %v, want %v"u8, words, want);
        }
    }
    var singleton = new @string[]{"one"u8}.slice();
    Reverse<slice<@string>, @string>(singleton);
    {
        var want = new @string[]{"one"u8}.slice(); if (!Equal<slice<@string>, @string>(singleton, want)) {
            Ꮡt.Errorf("Reverse(singleton) = %v, want %v"u8, singleton, want);
        }
    }
    Reverse<slice<@string>, @string>(default!);
}

// naiveReplace is a baseline implementation to the Replace function.
internal static S naiveReplace<S, E>(S s, nint i, nint j, params Span<E> vʗp)
    where S : /* ~[]E */ ISlice<E>, ISupportMake<S>, ISliceWrap<S, E>, new()
{
    var v = vʗp.sslice();

    s = Delete<S, E>(s, i, j);
    s = Insert(s, i, v.ꓸꓸꓸ);
    return s;
}

[GoType("dyn")] internal partial struct TestReplace_type {
    internal slice<nint> s, v;
    internal nint i, j;
}

public static void TestReplace(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in new TestReplace_type[]{
        new(), // all zero value

        new(
            s: new nint[]{1, 2, 3, 4}.slice(),
            v: new nint[]{5}.slice(),
            i: 1,
            j: 2
        ),
        new(
            s: new nint[]{1, 2, 3, 4}.slice(),
            v: new nint[]{5, 6, 7, 8}.slice(),
            i: 1,
            j: 2
        ),
        new(
            s: ((Func<slice<nint>>)(() => {
                var s = new slice<nint>(3, 20);
                s[0] = 0;
                s[1] = 1;
                s[2] = 2;
                return s;
            }))(),
            v: new nint[]{3, 4, 5, 6, 7}.slice(),
            i: 0,
            j: 1
        )
    }.slice()) {
        var (ss, vv) = (Clone<slice<nint>, nint>(test.s), Clone<slice<nint>, nint>(test.v));
        var want = naiveReplace(ss, test.i, test.j, vv.ꓸꓸꓸ);
        var got = Replace(test.s, test.i, test.j, test.v.ꓸꓸꓸ);
        if (!Equal<slice<nint>, nint>(got, want)) {
            Ꮡt.Errorf("Replace(%v, %v, %v, %v) = %v, want %v"u8, test.s, test.i, test.j, test.v, got, want);
        }
    }
}

[GoType("dyn")] internal partial struct TestReplacePanics_type {
    internal @string name;
    internal slice<nint> s, v;
    internal nint i, j;
}

public static void TestReplacePanics(ж<testing.T> Ꮡt) {
    var s = new nint[]{0, 1, 2, 3, 4}.slice();
    s = s[0..2];
    _ = s[0..4]; // this is a valid slice of s
    foreach (var (_, vᴛ1) in new TestReplacePanics_type[]{
        new("indexes out of order"u8, new nint[]{1, 2}.slice(), new nint[]{3}.slice(), 2, 1),
        new("large index"u8, new nint[]{1, 2}.slice(), new nint[]{3}.slice(), 1, 10),
        new("negative index"u8, new nint[]{1, 2}.slice(), new nint[]{3}.slice(), -1, 2),
        new("s[i:j] is valid and j > len(s)"u8, s, default!, 0, 4)
    }.slice()) {
        ref var test = ref heap(new TestReplacePanics_type(), out var Ꮡtest);
        test = vᴛ1;

        var (ss, vv) = (Clone<slice<nint>, nint>(test.s), Clone<slice<nint>, nint>(test.v));
        var ssʗ1 = ss;
        var testʗ1 = test;
        var vvʗ1 = vv;
        if (!panics(() => {
            _ = Replace(ssʗ1, testʗ1.i, testʗ1.j, vvʗ1.ꓸꓸꓸ);
        })) {
            Ꮡt.Errorf("Replace %s: should have panicked"u8, test.name);
        }
    }
}

public static void TestReplaceGrow(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // When Replace needs to allocate a new slice, we want the original slice
    // to not be changed.
    ref var a = ref heap<nint>(out var Ꮡa);
    a = 1;
    ref var b = ref heap<nint>(out var Ꮡb);
    b = 2;
    ref var c = ref heap<nint>(out var Ꮡc);
    c = 3;
    ref var d = ref heap<nint>(out var Ꮡd);
    d = 4;
    ref var e = ref heap<nint>(out var Ꮡe);
    e = 5;
    ref var f = ref heap<nint>(out var Ꮡf);
    f = 6;
    var mem = new ж<nint>[]{Ꮡa, Ꮡb, Ꮡc, Ꮡd, Ꮡe, Ꮡf}.slice();
    var memcopy = Clone<slice<ж<nint>>, ж<nint>>(mem);
    var s = mem[0..5]; // there is 1 element beyond len(s), within cap(s)
    var copy = Clone<slice<ж<nint>>, ж<nint>>(s);
    var original = s;
    // The new elements don't fit within cap(s), so Replace will allocate.
    ref var z = ref heap<nint>(out var Ꮡz);
    z = 99;
    s = Replace(s, 1, 3, Ꮡz, Ꮡz, Ꮡz, Ꮡz);
    {
        var want = new ж<nint>[]{Ꮡa, Ꮡz, Ꮡz, Ꮡz, Ꮡz, Ꮡd, Ꮡe}.slice(); if (!Equal<slice<ж<nint>>, ж<nint>>(s, want)) {
            Ꮡt.Errorf("Replace(%v, 1, 3, %v, %v, %v, %v) = %v, want %v"u8, copy, Ꮡz, Ꮡz, Ꮡz, Ꮡz, s, want);
        }
    }
    if (!Equal<slice<ж<nint>>, ж<nint>>(original, copy)) {
        Ꮡt.Errorf("original slice has changed, got %v, want %v"u8, original, copy);
    }
    if (!Equal<slice<ж<nint>>, ж<nint>>(mem, memcopy)) {
        // Changing the original tail s[len(s):cap(s)] is unwanted
        Ꮡt.Errorf("original backing memory has changed, got %v, want %v"u8, mem, memcopy);
    }
}

public static void TestReplaceClearTail(ж<testing.T> Ꮡt) {
    ref var a = ref heap<nint>(out var Ꮡa);
    a = 1;
    ref var b = ref heap<nint>(out var Ꮡb);
    b = 2;
    ref var c = ref heap<nint>(out var Ꮡc);
    c = 3;
    ref var d = ref heap<nint>(out var Ꮡd);
    d = 4;
    ref var e = ref heap<nint>(out var Ꮡe);
    e = 5;
    ref var f = ref heap<nint>(out var Ꮡf);
    f = 6;
    var mem = new ж<nint>[]{Ꮡa, Ꮡb, Ꮡc, Ꮡd, Ꮡe, Ꮡf}.slice();
    var s = mem[0..5]; // there is 1 element beyond len(s), within cap(s)
    var copy = Clone<slice<ж<nint>>, ж<nint>>(s);
    ref var y = ref heap<nint>(out var Ꮡy);
    y = 8;
    ref var z = ref heap<nint>(out var Ꮡz);
    z = 9;
    s = Replace(s, 1, 4, Ꮡy, Ꮡz);
    {
        var want = new ж<nint>[]{Ꮡa, Ꮡy, Ꮡz, Ꮡe}.slice(); if (!Equal<slice<ж<nint>>, ж<nint>>(s, want)) {
            Ꮡt.Errorf("Replace(%v) = %v, want %v"u8, copy, s, want);
        }
    }
    if (mem[4] != nil) {
        // Check that potential memory leak is avoided
        Ꮡt.Errorf("Replace: want nil discarded element, got %v"u8, mem[4].OrTypedNil());
    }
    if (mem[5] != Ꮡf) {
        Ꮡt.Errorf("Replace: want unchanged elements beyond original len, got %v"u8, mem[5].OrTypedNil());
    }
}

public static void TestReplaceOverlap(ж<testing.T> Ꮡt) {
    UntypedInt N = 10;
    var a = new slice<nint>(N);
    var want = new slice<nint>(2 * N);
    for (nint n = 0; n <= N; n++) {
        // length
        for (nint i = 0; i <= n; i++) {
            // insertion point 1
            for (nint j = i; j <= n; j++) {
                // insertion point 2
                for (nint x = 0; x <= N; x++) {
                    // start of inserted data
                    for (nint y = x; y <= N; y++) {
                        // end of inserted data
                        for (nint k = 0; k < N; k++) {
                            a[k] = k;
                        }
                        want = want[..0];
                        want = appendꓸꓸꓸ(want, a.slice(0, i));
                        want = appendꓸꓸꓸ(want, a.slice(x, y));
                        want = appendꓸꓸꓸ(want, a.slice(j, n));
                        var got = Replace(a.slice(0, n), i, j, a.slice(x, y).ꓸꓸꓸ);
                        if (!Equal<slice<nint>, nint>(got, want)) {
                            Ꮡt.Errorf("Insert with overlap failed n=%d i=%d j=%d x=%d y=%d, got %v want %v"u8, n, i, j, x, y, got, want);
                        }
                    }
                }
            }
        }
    }
}

public static void TestReplaceEndClearTail(ж<testing.T> Ꮡt) {
    var s = new nint[]{11, 22, 33}.slice();
    var v = new nint[]{99}.slice();
    // case when j == len(s)
    nint i = 1;
    nint j = 3;
    s = Replace(s, i, j, v.ꓸꓸꓸ);
    nint x = s[..3][2];
    {
        nint want = 0; if (x != want) {
            Ꮡt.Errorf("TestReplaceEndClearTail: obsolete element is %d, want %d"u8, x, want);
        }
    }
}

[GoType("dyn")] internal partial struct BenchmarkReplace_cases {
    internal @string name;
    internal Func<slice<nint>> s, v;
    internal nint i, j;
}

public static void BenchmarkReplace(ж<testing.B> Ꮡb) {
    var cases = new BenchmarkReplace_cases[]{
        new(
            name: "fast"u8,
            s: () => new slice<nint>(100),
            v: () => new slice<nint>(20),
            i: 10,
            j: 40
        ),
        new(
            name: "slow"u8,
            s: () => new slice<nint>(100),
            v: () => new slice<nint>(20),
            i: 0,
            j: 2
        )
    }.slice();
    foreach (var (_, vᴛ1) in cases) {
        ref var c = ref heap(new BenchmarkReplace_cases(), out var Ꮡc);
        c = vᴛ1;

        var cʗ1 = c;
        Ꮡb.Run("naive-"u8 + c.name, (ж<testing.B> bΔ1) => {
            for (nint k = 0; k < (~bΔ1).N; k++) {
                var s = cʗ1.s();
                var v = cʗ1.v();
                _ = naiveReplace(s, cʗ1.i, cʗ1.j, v.ꓸꓸꓸ);
            }
        });
        var cʗ2 = c;
        Ꮡb.Run("optimized-"u8 + c.name, (ж<testing.B> bΔ2) => {
            for (nint k = 0; k < (~bΔ2).N; k++) {
                var s = cʗ2.s();
                var v = cʗ2.v();
                _ = Replace(s, cʗ2.i, cʗ2.j, v.ꓸꓸꓸ);
            }
        });
    }
}

public static void TestInsertGrowthRate(ж<testing.T> Ꮡt) {
    var b = new slice<byte>(1);
    nint maxCap = cap(b);
    nint nGrow = 0;
    UntypedFloat N = 1e6;
    for (nint i = 0; i < N; i++) {
        b = Insert(b, len(b) - 1, (byte)(0));
        if (cap(b) > maxCap) {
            maxCap = cap(b);
            nGrow++;
        }
    }
    nint want = (nint)(Δmath.Log(N) / Δmath.Log(1.25D)); // 1.25 == growth rate for large slices
    if (nGrow > want) {
        Ꮡt.Errorf("too many grows. got:%d want:%d"u8, nGrow, want);
    }
}

public static void TestReplaceGrowthRate(ж<testing.T> Ꮡt) {
    var b = new slice<byte>(2);
    nint maxCap = cap(b);
    nint nGrow = 0;
    UntypedFloat N = 1e6;
    for (nint i = 0; i < N; i++) {
        b = Replace(b, len(b) - 2, len(b) - 1, (byte)(0), (byte)(0));
        if (cap(b) > maxCap) {
            maxCap = cap(b);
            nGrow++;
        }
    }
    nint want = (nint)(Δmath.Log(N) / Δmath.Log(1.25D)); // 1.25 == growth rate for large slices
    if (nGrow > want) {
        Ꮡt.Errorf("too many grows. got:%d want:%d"u8, nGrow, want);
    }
}

internal static void apply<T>(T v, Action<T> f) {
    f(v);
}

[GoLocalName("S")] [GoType("[]nint")] internal partial struct TestInference_S;

// Test type inference with a named slice type.
public static void TestInference(ж<testing.T> Ꮡt) {
    var s1 = new nint[]{1, 2, 3}.slice();
    apply<slice<nint>>(s1, Reverse<slice<nint>, nint>);
    {
        var want = new nint[]{3, 2, 1}.slice(); if (!Equal<slice<nint>, nint>(s1, want)) {
            Ꮡt.Errorf("Reverse(%v) = %v, want %v"u8, new nint[]{1, 2, 3}.slice(), s1, want);
        }
    }
    var s2 = new TestInference_S(new nint[]{4, 5, 6}.slice());
    apply<TestInference_S>(s2, Reverse<TestInference_S, nint>);
    {
        var want = (new TestInference_S(new nint[]{6, 5, 4}.slice())); if (!Equal<TestInference_S, nint>(s2, want)) {
            Ꮡt.Errorf("Reverse(%v) = %v, want %v"u8, new TestInference_S(new nint[]{4, 5, 6}.slice()), s2, want);
        }
    }
}

[GoType("dyn")] internal partial struct TestConcat_cases {
    internal slice<slice<nint>> s;
    internal slice<nint> want;
}

public static void TestConcat(ж<testing.T> Ꮡt) {
    var cases = new TestConcat_cases[]{
        new(
            s: new slice<nint>[]{default!}.slice(),
            want: default!
        ),
        new(
            s: new slice<nint>[]{new nint[]{1}.slice()}.slice(),
            want: new nint[]{1}.slice()
        ),
        new(
            s: new slice<nint>[]{new nint[]{1}.slice(), new nint[]{2}.slice()}.slice(),
            want: new nint[]{1, 2}.slice()
        ),
        new(
            s: new slice<nint>[]{new nint[]{1}.slice(), default!, new nint[]{2}.slice()}.slice(),
            want: new nint[]{1, 2}.slice()
        )
    }.slice();
    foreach (var (_, vᴛ1) in cases) {
        ref var tc = ref heap(new TestConcat_cases(), out var Ꮡtc);
        tc = vᴛ1;

        var got = Concat<slice<nint>, nint>(tc.s.ꓸꓸꓸ);
        if (!Equal<slice<nint>, nint>(tc.want, got)) {
            Ꮡt.Errorf("Concat(%v) = %v, want %v"u8, tc.s, got, tc.want);
        }
        ref var sink = ref heap<slice<nint>>(out var Ꮡsink);
        var tcʗ1 = tc;
        var allocs = testing.AllocsPerRun(5, () => {
            Ꮡsink.ValueSlot = Concat<slice<nint>, nint>(tcʗ1.s.ꓸꓸꓸ);
        });
        _ = sink;
        if (allocs > 1D) {
            
            Actionꓸꓸꓸ<@string, any> errorf = (@string p1, params ꓸꓸꓸany p2) => Ꮡt.Errorf(p1, p2);
            if (testenv.OptimizationOff() || race.Enabled || asan.Enabled || msan.Enabled) {
                                errorf = (@string p1, params ꓸꓸꓸany p2) => Ꮡt.Logf(p1, p2);
            }
            errorf("Concat(%v) allocated %v times; want 1"u8, tc.s, allocs);
        }
    }
}

// Use zero length element to minimize memory in testing
[GoType("dyn")] internal partial struct TestConcat_too_large_void {
}

[GoType("dyn")] internal partial struct TestConcat_too_large_cases {
    internal slice<nint> lengths;
    internal bool shouldPanic;
}

public static void TestConcat_too_large(ж<testing.T> Ꮡt) {
    var cases = new TestConcat_too_large_cases[]{
        new(
            lengths: new nint[]{0, 0}.slice(),
            shouldPanic: false
        ),
        new(
            lengths: new nint[]{Δmath.MaxInt, 0}.slice(),
            shouldPanic: false
        ),
        new(
            lengths: new nint[]{0, Δmath.MaxInt}.slice(),
            shouldPanic: false
        ),
        new(
            lengths: new nint[]{unchecked((nint)(9223372036854775806L)), 1}.slice(),
            shouldPanic: false
        ),
        new(
            lengths: new nint[]{unchecked((nint)(9223372036854775806L)), 1, 1}.slice(),
            shouldPanic: true
        ),
        new(
            lengths: new nint[]{Δmath.MaxInt, 1}.slice(),
            shouldPanic: true
        ),
        new(
            lengths: new nint[]{Δmath.MaxInt, Δmath.MaxInt}.slice(),
            shouldPanic: true
        )
    }.slice();
    foreach (var (_, tc) in cases) {
        ref var r = ref heap<any>(out var Ꮡr);
        ref var ss = ref heap<slice<slice<TestConcat_too_large_void>>>(out var Ꮡss);
        ss = new slice<slice<TestConcat_too_large_void>>(0, len(tc.lengths));
        foreach (var (_, l) in tc.lengths) {
            var s = new slice<TestConcat_too_large_void>(l);
            ss = append(ss, s);
        }
        ((Action)(() => {
            GoFrame ᒐ = default;
            try {
                defer(() => {
                    Ꮡr.ValueSlot = recover();
                }, ref ᒐ);
                _ = Concat<slice<TestConcat_too_large_void>, TestConcat_too_large_void>(Ꮡss.ValueSlot.ꓸꓸꓸ);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }))();
        {
            var didPanic = r != default!; if (didPanic != tc.shouldPanic) {
                Ꮡt.Errorf("slices.Concat(lens(%v)) got panic == %v"u8,
                    tc.lengths, didPanic);
            }
        }
    }
}

[GoType("dyn")] internal partial struct TestRepeat_type {
    internal slice<nint> x;
    internal nint count;
    internal slice<nint> want;
}

[GoType("dyn")] internal partial struct TestRepeat_typeᴛ1 {
    internal slice<EmptyStruct> x;
    internal nint count;
    internal slice<EmptyStruct> want;
}

public static void TestRepeat(ж<testing.T> Ꮡt) {
    // normal cases
    foreach (var (_, tc) in new TestRepeat_type[]{
        new(x: slice<nint>(default!), count: 0, want: new nint[]{}.slice()),
        new(x: slice<nint>(default!), count: 1, want: new nint[]{}.slice()),
        new(x: slice<nint>(default!), count: Δmath.MaxInt, want: new nint[]{}.slice()),
        new(x: new nint[]{}.slice(), count: 0, want: new nint[]{}.slice()),
        new(x: new nint[]{}.slice(), count: 1, want: new nint[]{}.slice()),
        new(x: new nint[]{}.slice(), count: Δmath.MaxInt, want: new nint[]{}.slice()),
        new(x: new nint[]{0}.slice(), count: 0, want: new nint[]{}.slice()),
        new(x: new nint[]{0}.slice(), count: 1, want: new nint[]{0}.slice()),
        new(x: new nint[]{0}.slice(), count: 2, want: new nint[]{0, 0}.slice()),
        new(x: new nint[]{0}.slice(), count: 3, want: new nint[]{0, 0, 0}.slice()),
        new(x: new nint[]{0}.slice(), count: 4, want: new nint[]{0, 0, 0, 0}.slice()),
        new(x: new nint[]{0, 1}.slice(), count: 0, want: new nint[]{}.slice()),
        new(x: new nint[]{0, 1}.slice(), count: 1, want: new nint[]{0, 1}.slice()),
        new(x: new nint[]{0, 1}.slice(), count: 2, want: new nint[]{0, 1, 0, 1}.slice()),
        new(x: new nint[]{0, 1}.slice(), count: 3, want: new nint[]{0, 1, 0, 1, 0, 1}.slice()),
        new(x: new nint[]{0, 1}.slice(), count: 4, want: new nint[]{0, 1, 0, 1, 0, 1, 0, 1}.slice()),
        new(x: new nint[]{0, 1, 2}.slice(), count: 0, want: new nint[]{}.slice()),
        new(x: new nint[]{0, 1, 2}.slice(), count: 1, want: new nint[]{0, 1, 2}.slice()),
        new(x: new nint[]{0, 1, 2}.slice(), count: 2, want: new nint[]{0, 1, 2, 0, 1, 2}.slice()),
        new(x: new nint[]{0, 1, 2}.slice(), count: 3, want: new nint[]{0, 1, 2, 0, 1, 2, 0, 1, 2}.slice()),
        new(x: new nint[]{0, 1, 2}.slice(), count: 4, want: new nint[]{0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2}.slice())
    }.slice()) {
        {
            var got = Repeat<slice<nint>, nint>(tc.x, tc.count); if (got == default! || cap(got) != cap(tc.want) || !Equal<slice<nint>, nint>(got, tc.want)) {
                Ꮡt.Errorf("Repeat(%v, %v): got: %v, want: %v, (got == nil): %v, cap(got): %v, cap(want): %v"u8,
                    tc.x, tc.count, got, tc.want, got == default!, cap(got), cap(tc.want));
            }
        }
    }
    // big slices
    foreach (var (_, tc) in new TestRepeat_typeᴛ1[]{
        new(x: new slice<EmptyStruct>(unchecked((nint)(9223372036854775807L))), count: 1, want: new slice<EmptyStruct>(unchecked((nint)(9223372036854775807L)))),
        new(x: new slice<EmptyStruct>(unchecked((nint)(4611686018427387902L))), count: 2, want: new slice<EmptyStruct>(unchecked((nint)(9223372036854775804L)))),
        new(x: new slice<EmptyStruct>(unchecked((nint)(3074457345618258600L))), count: 3, want: new slice<EmptyStruct>(unchecked((nint)(9223372036854775800L)))),
        new(x: new slice<EmptyStruct>(unchecked((nint)(2305843009213693948L))), count: 4, want: new slice<EmptyStruct>(unchecked((nint)(9223372036854775792L)))),
        new(x: new slice<EmptyStruct>(unchecked((nint)(1844674407370955157L))), count: 5, want: new slice<EmptyStruct>(unchecked((nint)(9223372036854775785L)))),
        new(x: new slice<EmptyStruct>(unchecked((nint)(1537228672809129296L))), count: 6, want: new slice<EmptyStruct>(unchecked((nint)(9223372036854775776L)))),
        new(x: new slice<EmptyStruct>(unchecked((nint)(1317624576693539395L))), count: 7, want: new slice<EmptyStruct>(unchecked((nint)(9223372036854775765L)))),
        new(x: new slice<EmptyStruct>(unchecked((nint)(1152921504606846968L))), count: 8, want: new slice<EmptyStruct>(unchecked((nint)(9223372036854775744L)))),
        new(x: new slice<EmptyStruct>(unchecked((nint)(1024819115206086192L))), count: 9, want: new slice<EmptyStruct>(unchecked((nint)(9223372036854775728L))))
    }.slice()) {
        {
            var got = Repeat<slice<EmptyStruct>, EmptyStruct>(tc.x, tc.count); if (got == default! || len(got) != len(tc.want) || cap(got) != cap(tc.want)) {
                Ꮡt.Errorf("Repeat(make([]struct{}, %v), %v): (got == nil): %v, len(got): %v, len(want): %v, cap(got): %v, cap(want): %v"u8,
                    len(tc.x), tc.count, got == default!, len(got), len(tc.want), cap(got), cap(tc.want));
            }
        }
    }
}

[GoType("dyn")] internal partial struct TestRepeatPanics_type {
    internal @string name;
    internal slice<EmptyStruct> x;
    internal nint count;
}

public static void TestRepeatPanics(ж<testing.T> Ꮡt) {
    foreach (var (_, vᴛ1) in new TestRepeatPanics_type[]{
        new(name: "cannot be negative"u8, x: new slice<EmptyStruct>(0), count: -1),
        new(name: "the result of (len(x) * count) overflows, hi > 0"u8, x: new slice<EmptyStruct>(3), count: Δmath.MaxInt),
        new(name: "the result of (len(x) * count) overflows, lo > maxInt"u8, x: new slice<EmptyStruct>(2), count: unchecked((nint)(4611686018427387904L)))
    }.slice()) {
        ref var test = ref heap(new TestRepeatPanics_type(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        if (!panics(() => {
            _ = Repeat<slice<EmptyStruct>, EmptyStruct>(testʗ1.x, testʗ1.count);
        })) {
            Ꮡt.Errorf("Repeat %s: got no panic, want panic"u8, test.name);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object cloneKeepsAliveSDueToˢ = (@string)"clone keeps alive s due to array overlap"u8;

public static void TestIssue68488(ж<testing.T> Ꮡt) {
    var s = new slice<nint>(3);
    var clone = Clone<slice<nint>, nint>(s[1..1]);
    var exprᴛ1 = @unsafe.SliceData(clone);
    if (exprᴛ1 == Ꮡ(s, 0) || exprᴛ1 == Ꮡ(s, 1) || exprᴛ1 == Ꮡ(s, 2)) {
        Ꮡt.Error(cloneKeepsAliveSDueToˢ);
    }

}

} // end slices_test_package
