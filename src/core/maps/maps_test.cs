// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using Δmath = math_package;
using strconv = strconv_package;
using testing = testing_package;
using static go.maps_package;

partial class maps_internal_test_package {

internal static map<nint, nint> m1 = new map<nint, nint>{[1] = 2, [2] = 4, [4] = 8, [8] = 16};

internal static map<nint, @string> m2 = new map<nint, @string>{[1] = "2"u8, [2] = "4"u8, [4] = "8"u8, [8] = "16"u8};

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object equalNilNilFalseWantTrueˢ = (@string)"Equal(nil, nil) = false, want true"u8;

public static void TestEqual(ж<testing.T> Ꮡt) {
    if (!Equal<map<nint, nint>, map<nint, nint>, nint, nint>(m1, m1)) {
        Ꮡt.Errorf("Equal(%v, %v) = false, want true"u8, m1, m1);
    }
    if (Equal<map<nint, nint>, map<nint, nint>, nint, nint>(m1, ((map<nint, nint>)default!))) {
        Ꮡt.Errorf("Equal(%v, nil) = true, want false"u8, m1);
    }
    if (Equal<map<nint, nint>, map<nint, nint>, nint, nint>(((map<nint, nint>)default!), m1)) {
        Ꮡt.Errorf("Equal(nil, %v) = true, want false"u8, m1);
    }
    if (!Equal<map<nint, nint>, map<nint, nint>, nint, nint>(default!, default!)) {
        Ꮡt.Error(equalNilNilFalseWantTrueˢ);
    }
    {
        var ms = new map<nint, nint>{[1] = 2}; if (Equal<map<nint, nint>, map<nint, nint>, nint, nint>(m1, ms)) {
            Ꮡt.Errorf("Equal(%v, %v) = true, want false"u8, m1, ms);
        }
    }
    // Comparing NaN for equality is expected to fail.
    var mf = new map<nint, float64>{[1] = 0D, [2] = Δmath.NaN()};
    if (Equal<map<nint, float64>, map<nint, float64>, nint, float64>(mf, mf)) {
        Ꮡt.Errorf("Equal(%v, %v) = true, want false"u8, mf, mf);
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

// equalIntStr compares ints and strings.
internal static bool equalIntStr(nint v1, @string v2) {
    return strconv.Itoa(v1) == v2;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object equalFuncNilNilEqualˢ = (@string)"EqualFunc(nil, nil, equal) = false, want true"u8;

public static void TestEqualFunc(ж<testing.T> Ꮡt) {
    if (!EqualFunc<map<nint, nint>, map<nint, nint>, nint, nint, nint>(m1, m1, equal<nint>)) {
        Ꮡt.Errorf("EqualFunc(%v, %v, equal) = false, want true"u8, m1, m1);
    }
    if (EqualFunc<map<nint, nint>, map<nint, nint>, nint, nint, nint>(m1, ((map<nint, nint>)default!), equal<nint>)) {
        Ꮡt.Errorf("EqualFunc(%v, nil, equal) = true, want false"u8, m1);
    }
    if (EqualFunc<map<nint, nint>, map<nint, nint>, nint, nint, nint>(((map<nint, nint>)default!), m1, equal<nint>)) {
        Ꮡt.Errorf("EqualFunc(nil, %v, equal) = true, want false"u8, m1);
    }
    if (!EqualFunc<map<nint, nint>, map<nint, nint>, nint, nint, nint>(default!, default!, equal<nint>)) {
        Ꮡt.Error(equalFuncNilNilEqualˢ);
    }
    {
        var ms = new map<nint, nint>{[1] = 2}; if (EqualFunc<map<nint, nint>, map<nint, nint>, nint, nint, nint>(m1, ms, equal<nint>)) {
            Ꮡt.Errorf("EqualFunc(%v, %v, equal) = true, want false"u8, m1, ms);
        }
    }
    // Comparing NaN for equality is expected to fail.
    var mf = new map<nint, float64>{[1] = 0D, [2] = Δmath.NaN()};
    if (EqualFunc<map<nint, float64>, map<nint, float64>, nint, float64, float64>(mf, mf, equal<float64>)) {
        Ꮡt.Errorf("EqualFunc(%v, %v, equal) = true, want false"u8, mf, mf);
    }
    // But it should succeed using equalNaN.
    if (!EqualFunc<map<nint, float64>, map<nint, float64>, nint, float64, float64>(mf, mf, equalNaN<float64>)) {
        Ꮡt.Errorf("EqualFunc(%v, %v, equalNaN) = false, want true"u8, mf, mf);
    }
    if (!EqualFunc<map<nint, nint>, map<nint, @string>, nint, nint, @string>(m1, m2, equalIntStr)) {
        Ꮡt.Errorf("EqualFunc(%v, %v, equalIntStr) = false, want true"u8, m1, m2);
    }
}

public static void TestClone(ж<testing.T> Ꮡt) {
    var mc = Clone<map<nint, nint>, nint, nint>(m1);
    if (!Equal<map<nint, nint>, map<nint, nint>, nint, nint>(mc, m1)) {
        Ꮡt.Errorf("Clone(%v) = %v, want %v"u8, m1, mc, m1);
    }
    mc[16] = 32;
    if (Equal<map<nint, nint>, map<nint, nint>, nint, nint>(mc, m1)) {
        Ꮡt.Errorf("Equal(%v, %v) = true, want false"u8, mc, m1);
    }
}

public static void TestCloneNil(ж<testing.T> Ꮡt) {
    map<@string, nint> m1 = default!;
    var mc = Clone<map<@string, nint>, @string, nint>(m1);
    if (mc != default!) {
        Ꮡt.Errorf("Clone(%v) = %v, want %v"u8, m1, mc, m1);
    }
}

internal partial struct TestCopy_M1 /*map[nint, bool]*/;

internal partial struct TestCopy_M2 /*map[nint, bool]*/;

public static void TestCopy(ж<testing.T> Ꮡt) {
    var mc = Clone<map<nint, nint>, nint, nint>(m1);
    Copy<map<nint, nint>, map<nint, nint>, nint, nint>(mc, mc);
    if (!Equal<map<nint, nint>, map<nint, nint>, nint, nint>(mc, m1)) {
        Ꮡt.Errorf("Copy(%v, %v) = %v, want %v"u8, m1, m1, mc, m1);
    }
    Copy<map<nint, nint>, map<nint, nint>, nint, nint>(mc, new map<nint, nint>{[16] = 32});
    var want = new map<nint, nint>{[1] = 2, [2] = 4, [4] = 8, [8] = 16, [16] = 32};
    if (!Equal<map<nint, nint>, map<nint, nint>, nint, nint>(mc, want)) {
        Ꮡt.Errorf("Copy result = %v, want %v"u8, mc, want);
    }
    Copy<TestCopy_M1, TestCopy_M2, nint, bool>(new TestCopy_M1(0), new TestCopy_M2(0));
}

public static void TestDeleteFunc(ж<testing.T> Ꮡt) {
    var mc = Clone<map<nint, nint>, nint, nint>(m1);
    DeleteFunc(mc, (nint _Δp0, nint _Δp1) => false);
    if (!Equal<map<nint, nint>, map<nint, nint>, nint, nint>(mc, m1)) {
        Ꮡt.Errorf("DeleteFunc(%v, true) = %v, want %v"u8, m1, mc, m1);
    }
    DeleteFunc(mc, (nint k, nint v) => k > 3);
    var want = new map<nint, nint>{[1] = 2, [2] = 4};
    if (!Equal<map<nint, nint>, map<nint, nint>, nint, nint>(mc, want)) {
        Ꮡt.Errorf("DeleteFunc result = %v, want %v"u8, mc, want);
    }
}

internal static map<nint, nint> n;

public static void BenchmarkMapClone(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    map<nint, nint> m = new map<nint, nint>();
    for (nint i = 0; i < 1000000; i++) {
        m[i] = i;
    }
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        n = Clone<map<nint, nint>, nint, nint>(m);
    }
}

public static void TestCloneWithDelete(ж<testing.T> Ꮡt) {
    map<nint, nint> m = new map<nint, nint>();
    for (nint i = 0; i < 32; i++) {
        m[i] = i;
    }
    for (nint i = 8; i < 32; i++) {
        delete(m, i);
    }
    var m2 = Clone<map<nint, nint>, nint, nint>(m);
    if (len(m2) != 8) {
        Ꮡt.Errorf("len2(m2) = %d, want %d"u8, len(m2), (nint)(8));
    }
    for (nint i = 0; i < 8; i++) {
        if (m2[i] != m[i]) {
            Ꮡt.Errorf("m2[%d] = %d, want %d"u8, i, m2[i], m[i]);
        }
    }
}

public static void TestCloneWithMapAssign(ж<testing.T> Ꮡt) {
    map<nint, nint> m = new map<nint, nint>();
    const nint N = 25;
    for (nint i = 0; i < N; i++) {
        m[i] = i;
    }
    var m2 = Clone<map<nint, nint>, nint, nint>(m);
    if (len(m2) != N) {
        Ꮡt.Errorf("len2(m2) = %d, want %d"u8, len(m2), (nint)(N));
    }
    for (nint i = 0; i < N; i++) {
        if (m2[i] != m[i]) {
            Ꮡt.Errorf("m2[%d] = %d, want %d"u8, i, m2[i], m[i]);
        }
    }
}

internal partial struct TestCloneLarge_K /*[17]float64*/; // > 128 bytes

internal partial struct TestCloneLarge_V /*[17]float64*/;

public static void TestCloneLarge(ж<testing.T> Ꮡt) {
    float64 zero = default!;
    var negZero = -zero;
    for (nint tst = 0; tst < 3; tst++) {
        // Initialize m with a key and value.
        var m = new map<TestCloneLarge_K, TestCloneLarge_V>{};
        TestCloneLarge_K k1 = default!;
        TestCloneLarge_V v1 = default!;
        m[k1] = v1.Clone();
        switch (tst) {
        case 0: {
            break;
        }
        case 1: {
            for (nint i = 0; i < 7 + 1; i++) {
                // nothing, just a 1-entry map
                // Add more entries to make it 2 buckets
                // 1 entry already
                // 7 more fill up 1 bucket
                // 1 more to grow to 2 buckets
                m[new TestCloneLarge_K(new float64[]{(float64)i + 1D}.array(17))] = new TestCloneLarge_V(new float64[17].array());
            }
            break;
        }
        case 2: {
            for (nint i = 0; i < 7 + 5 + 13 + 1; i++) {
                // Capture the map mid-grow
                // 1 entry already
                // 7 more fill up 1 bucket
                // 5 more (13 total) fill up 2 buckets
                // 13 more (26 total) fill up 4 buckets
                // 1 more to start the 4->8 bucket grow
                m[new TestCloneLarge_K(new float64[]{(float64)i + 1D}.array(17))] = new TestCloneLarge_V(new float64[17].array());
            }
            break;
        }}

        // Clone m, which should freeze the map's contents.
        var c = Clone<map<TestCloneLarge_K, TestCloneLarge_V>, TestCloneLarge_K, TestCloneLarge_V>(m);
        // Update m with new key and value.
        var (k2, v2) = (k1.Clone(), v1.Clone());
        k2[0] = negZero;
        v2[0] = 1.0D;
        m[k2] = v2.Clone();
        // Make sure c still has its old key and value.
        foreach (var (kᴛ1, vᴛ1) in c) {
            var k = kᴛ1.Clone();
            var v = vᴛ1.Clone();

            if (Δmath.Signbit(k[0])) {
                Ꮡt.Errorf("tst%d: sign bit of key changed; got %v want %v"u8, tst, k, k1);
            }
            if (v != v1) {
                Ꮡt.Errorf("tst%d: value changed; got %v want %v"u8, tst, v, v1);
            }
        }
    }
}

} // end maps_internal_test_package
