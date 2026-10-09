// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.math;

using fmt = fmt_package;
using testenv = @internal.testenv_package;
using bits = go.math.bits_package;
using Δrand = go.math.rand_package;
using strings = strings_package;
using testing = testing_package;
using @internal;
using go.math;
using io = io_package;
using static go.math.big_package;

partial class big_internal_test_package {

internal static bool isRaceBuilder = strings.HasSuffix(testenv.Builder(), "-race"u8);

// type funVV is a methodless func type — rendered inline as its base delegate

internal partial struct argVV {
    internal global::go.math.big_package.nat z, x, y;
    internal global::go.math.big_package.Word c;
}

internal static slice<argVV> sumVV = new argVV[]{
    new(),
    new(new nat(new global::go.math.big_package.Word[]{0}.slice()), new nat(new global::go.math.big_package.Word[]{0}.slice()), new nat(new global::go.math.big_package.Word[]{0}.slice()), 0),
    new(new nat(new global::go.math.big_package.Word[]{1}.slice()), new nat(new global::go.math.big_package.Word[]{1}.slice()), new nat(new global::go.math.big_package.Word[]{0}.slice()), 0),
    new(new nat(new global::go.math.big_package.Word[]{0}.slice()), new nat(new global::go.math.big_package.Word[]{_M}.slice()), new nat(new global::go.math.big_package.Word[]{1}.slice()), 1),
    new(new nat(new global::go.math.big_package.Word[]{80235}.slice()), new nat(new global::go.math.big_package.Word[]{12345}.slice()), new nat(new global::go.math.big_package.Word[]{67890}.slice()), 0),
    new(new nat(new global::go.math.big_package.Word[]{_M - 1}.slice()), new nat(new global::go.math.big_package.Word[]{_M}.slice()), new nat(new global::go.math.big_package.Word[]{_M}.slice()), 1),
    new(new nat(new global::go.math.big_package.Word[]{0, 0, 0, 0}.slice()), new nat(new global::go.math.big_package.Word[]{_M, _M, _M, _M}.slice()), new nat(new global::go.math.big_package.Word[]{1, 0, 0, 0}.slice()), 1),
    new(new nat(new global::go.math.big_package.Word[]{0, 0, 0, _M}.slice()), new nat(new global::go.math.big_package.Word[]{_M, _M, _M, _M - 1}.slice()), new nat(new global::go.math.big_package.Word[]{1, 0, 0, 0}.slice()), 0),
    new(new nat(new global::go.math.big_package.Word[]{0, 0, 0, 0}.slice()), new nat(new global::go.math.big_package.Word[]{_M, 0, _M, 0}.slice()), new nat(new global::go.math.big_package.Word[]{1, _M, 0, _M}.slice()), 1)
}.slice();

internal static void testFunVV(ж<testing.T> Ꮡt, @string msg, Func<slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, global::go.math.big_package.Word> f, argVV a) {
    var z = new global::go.math.big_package.nat(len(a.z));
    global::go.math.big_package.Word c = f(z, a.x, a.y);
    foreach (var (i, zi) in z) {
        if (zi != a.z[i]) {
            Ꮡt.Errorf("%s%+v\n\tgot z[%d] = %#x; want %#x"u8, msg, a, i, zi, a.z[i]);
            break;
        }
    }
    if (c != a.c) {
        Ꮡt.Errorf("%s%+v\n\tgot c = %#x; want %#x"u8, msg, a, c, a.c);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string addVVGˢ = "addVV_g"u8;
internal static readonly @string addVVˢ = "addVV"u8;
internal static readonly @string addVVGSymmetricˢ = "addVV_g symmetric"u8;
internal static readonly @string addVVSymmetricˢ = "addVV symmetric"u8;
internal static readonly @string subVVGˢ = "subVV_g"u8;
internal static readonly @string subVVˢ = "subVV"u8;
internal static readonly @string subVVGSymmetricˢ = "subVV_g symmetric"u8;
internal static readonly @string subVVSymmetricˢ = "subVV symmetric"u8;

public static void TestFunVV(ж<testing.T> Ꮡt) {
    foreach (var (_, a) in sumVV) {
        var arg = a;
        testFunVV(Ꮡt, addVVGˢ, new Func<slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, global::go.math.big_package.Word>(addVV_g), arg);
        testFunVV(Ꮡt, addVVˢ, new Func<slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, global::go.math.big_package.Word>(addVV), arg);
        arg = new argVV(a.z, a.y, a.x, a.c);
        testFunVV(Ꮡt, addVVGSymmetricˢ, new Func<slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, global::go.math.big_package.Word>(addVV_g), arg);
        testFunVV(Ꮡt, addVVSymmetricˢ, new Func<slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, global::go.math.big_package.Word>(addVV), arg);
        arg = new argVV(a.x, a.z, a.y, a.c);
        testFunVV(Ꮡt, subVVGˢ, new Func<slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, global::go.math.big_package.Word>(subVV_g), arg);
        testFunVV(Ꮡt, subVVˢ, new Func<slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, global::go.math.big_package.Word>(subVV), arg);
        arg = new argVV(a.y, a.z, a.x, a.c);
        testFunVV(Ꮡt, subVVGSymmetricˢ, new Func<slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, global::go.math.big_package.Word>(subVV_g), arg);
        testFunVV(Ꮡt, subVVSymmetricˢ, new Func<slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, global::go.math.big_package.Word>(subVV), arg);
    }
}

// Always the same seed for reproducible results.
internal static ж<Δrand.Rand> rnd = Δrand.New(Δrand.NewSource(0));

internal static global::go.math.big_package.Word rndW() {
    return ((global::go.math.big_package.Word)(nuint)((int64)((rnd.Int63() << (int)(1)) | rnd.Int63n(2))));
}

internal static slice<global::go.math.big_package.Word> rndV(nint n) {
    var v = new slice<global::go.math.big_package.Word>(n);
    foreach (var (i, _) in v) {
        v[i] = rndW();
    }
    return v;
}

internal static slice<nint> benchSizes = new nint[]{1, 2, 3, 4, 5, 10, 100, 1000, 10000, 100000}.slice();

public static void BenchmarkAddVV(ж<testing.B> Ꮡb) {
    foreach (var (_, n) in benchSizes) {
        if (isRaceBuilder && n > 1000) {
            continue;
        }
        var x = rndV(n);
        var y = rndV(n);
        var z = new slice<global::go.math.big_package.Word>(n);
        var xʗ1 = x;
        var yʗ1 = y;
        var zʗ1 = z;
        Ꮡb.Run(fmt.Sprint(n), (ж<testing.B> bΔ1) => {
            bΔ1.SetBytes((int64)(n * (nint)_W));
            for (nint i = 0; i < (~bΔ1).N; i++) {
                addVV(zʗ1, xʗ1, yʗ1);
            }
        });
    }
}

public static void BenchmarkSubVV(ж<testing.B> Ꮡb) {
    foreach (var (_, n) in benchSizes) {
        if (isRaceBuilder && n > 1000) {
            continue;
        }
        var x = rndV(n);
        var y = rndV(n);
        var z = new slice<global::go.math.big_package.Word>(n);
        var xʗ1 = x;
        var yʗ1 = y;
        var zʗ1 = z;
        Ꮡb.Run(fmt.Sprint(n), (ж<testing.B> bΔ1) => {
            bΔ1.SetBytes((int64)(n * (nint)_W));
            for (nint i = 0; i < (~bΔ1).N; i++) {
                subVV(zʗ1, xʗ1, yʗ1);
            }
        });
    }
}

// type funVW is a methodless func type — rendered inline as its base delegate

internal partial struct argVW {
    internal global::go.math.big_package.nat z, x;
    internal global::go.math.big_package.Word y;
    internal global::go.math.big_package.Word c;
}

internal static slice<argVW> sumVW = new argVW[]{
    new(),
    new(default!, default!, 2, 2),
    new(new nat(new global::go.math.big_package.Word[]{0}.slice()), new nat(new global::go.math.big_package.Word[]{0}.slice()), 0, 0),
    new(new nat(new global::go.math.big_package.Word[]{1}.slice()), new nat(new global::go.math.big_package.Word[]{0}.slice()), 1, 0),
    new(new nat(new global::go.math.big_package.Word[]{1}.slice()), new nat(new global::go.math.big_package.Word[]{1}.slice()), 0, 0),
    new(new nat(new global::go.math.big_package.Word[]{0}.slice()), new nat(new global::go.math.big_package.Word[]{_M}.slice()), 1, 1),
    new(new nat(new global::go.math.big_package.Word[]{0, 0, 0, 0}.slice()), new nat(new global::go.math.big_package.Word[]{_M, _M, _M, _M}.slice()), 1, 1),
    new(new nat(new global::go.math.big_package.Word[]{585}.slice()), new nat(new global::go.math.big_package.Word[]{314}.slice()), 271, 0)
}.slice();

internal static slice<argVW> lshVW = new argVW[]{
    new(),
    new(new nat(new global::go.math.big_package.Word[]{0}.slice()), new nat(new global::go.math.big_package.Word[]{0}.slice()), 0, 0),
    new(new nat(new global::go.math.big_package.Word[]{0}.slice()), new nat(new global::go.math.big_package.Word[]{0}.slice()), 1, 0),
    new(new nat(new global::go.math.big_package.Word[]{0}.slice()), new nat(new global::go.math.big_package.Word[]{0}.slice()), 20, 0),
    new(new nat(new global::go.math.big_package.Word[]{_M}.slice()), new nat(new global::go.math.big_package.Word[]{_M}.slice()), 0, 0),
    new(new nat(new global::go.math.big_package.Word[]{unchecked((nuint)(18446744073709551614UL))}.slice()), new nat(new global::go.math.big_package.Word[]{_M}.slice()), 1, 1),
    new(new nat(new global::go.math.big_package.Word[]{unchecked((nuint)(18446744073708503040UL))}.slice()), new nat(new global::go.math.big_package.Word[]{_M}.slice()), 20, (global::go.math.big_package.Word)((nuint)_M >> (int)((_W - 20)))),
    new(new nat(new global::go.math.big_package.Word[]{_M, _M, _M}.slice()), new nat(new global::go.math.big_package.Word[]{_M, _M, _M}.slice()), 0, 0),
    new(new nat(new global::go.math.big_package.Word[]{unchecked((nuint)(18446744073709551614UL)), _M, _M}.slice()), new nat(new global::go.math.big_package.Word[]{_M, _M, _M}.slice()), 1, 1),
    new(new nat(new global::go.math.big_package.Word[]{unchecked((nuint)(18446744073708503040UL)), _M, _M}.slice()), new nat(new global::go.math.big_package.Word[]{_M, _M, _M}.slice()), 20, (global::go.math.big_package.Word)((nuint)_M >> (int)((_W - 20))))
}.slice();

internal static slice<argVW> rshVW = new argVW[]{
    new(),
    new(new nat(new global::go.math.big_package.Word[]{0}.slice()), new nat(new global::go.math.big_package.Word[]{0}.slice()), 0, 0),
    new(new nat(new global::go.math.big_package.Word[]{0}.slice()), new nat(new global::go.math.big_package.Word[]{0}.slice()), 1, 0),
    new(new nat(new global::go.math.big_package.Word[]{0}.slice()), new nat(new global::go.math.big_package.Word[]{0}.slice()), 20, 0),
    new(new nat(new global::go.math.big_package.Word[]{_M}.slice()), new nat(new global::go.math.big_package.Word[]{_M}.slice()), 0, 0),
    new(new nat(new global::go.math.big_package.Word[]{(global::go.math.big_package.Word)((nuint)_M >> (int)(1))}.slice()), new nat(new global::go.math.big_package.Word[]{_M}.slice()), 1, unchecked((nuint)(9223372036854775808UL))),
    new(new nat(new global::go.math.big_package.Word[]{(global::go.math.big_package.Word)((nuint)_M >> (int)(20))}.slice()), new nat(new global::go.math.big_package.Word[]{_M}.slice()), 20, unchecked((nuint)(18446726481523507200UL))),
    new(new nat(new global::go.math.big_package.Word[]{_M, _M, _M}.slice()), new nat(new global::go.math.big_package.Word[]{_M, _M, _M}.slice()), 0, 0),
    new(new nat(new global::go.math.big_package.Word[]{_M, _M, (global::go.math.big_package.Word)((nuint)_M >> (int)(1))}.slice()), new nat(new global::go.math.big_package.Word[]{_M, _M, _M}.slice()), 1, unchecked((nuint)(9223372036854775808UL))),
    new(new nat(new global::go.math.big_package.Word[]{_M, _M, (global::go.math.big_package.Word)((nuint)_M >> (int)(20))}.slice()), new nat(new global::go.math.big_package.Word[]{_M, _M, _M}.slice()), 20, unchecked((nuint)(18446726481523507200UL)))
}.slice();

internal static void testFunVW(ж<testing.T> Ꮡt, @string msg, Func<slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, global::go.math.big_package.Word, global::go.math.big_package.Word> f, argVW a) {
    var z = new global::go.math.big_package.nat(len(a.z));
    global::go.math.big_package.Word c = f(z, a.x, a.y);
    foreach (var (i, zi) in z) {
        if (zi != a.z[i]) {
            Ꮡt.Errorf("%s%+v\n\tgot z[%d] = %#x; want %#x"u8, msg, a, i, zi, a.z[i]);
            break;
        }
    }
    if (c != a.c) {
        Ꮡt.Errorf("%s%+v\n\tgot c = %#x; want %#x"u8, msg, a, c, a.c);
    }
}

internal static void testFunVWext(ж<testing.T> Ꮡt, @string msg, Func<slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, global::go.math.big_package.Word, global::go.math.big_package.Word> f, Func<slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, global::go.math.big_package.Word, global::go.math.big_package.Word> f_g, argVW a) {
    // using the result of addVW_g/subVW_g as golden
    var z_g = new global::go.math.big_package.nat(len(a.z));
    global::go.math.big_package.Word c_g = f_g(z_g, a.x, a.y);
    global::go.math.big_package.Word c = f(a.z, a.x, a.y);
    foreach (var (i, zi) in a.z) {
        if (zi != z_g[i]) {
            Ꮡt.Errorf("%s\n\tgot z[%d] = %#x; want %#x"u8, msg, i, zi, z_g[i]);
            break;
        }
    }
    if (c != c_g) {
        Ꮡt.Errorf("%s\n\tgot c = %#x; want %#x"u8, msg, c, c_g);
    }
}

internal static Func<slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, global::go.math.big_package.Word, global::go.math.big_package.Word> makeFunVW(Func<slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, nuint, global::go.math.big_package.Word> f) {
    return (slice<global::go.math.big_package.Word> z, slice<global::go.math.big_package.Word> x, global::go.math.big_package.Word s) => {
        return f(z, x, (nuint)s);
    };
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string addVWGˢ = "addVW_g"u8;
internal static readonly @string addVWˢ = "addVW"u8;
internal static readonly @string subVWGˢ = "subVW_g"u8;
internal static readonly @string subVWˢ = "subVW"u8;
internal static readonly @string shlVUGˢ = "shlVU_g"u8;
internal static readonly @string shlVUˢ = "shlVU"u8;
internal static readonly @string shrVUGˢ = "shrVU_g"u8;
internal static readonly @string shrVUˢ = "shrVU"u8;

public static void TestFunVW(ж<testing.T> Ꮡt) {
    foreach (var (_, a) in sumVW) {
        var arg = a;
        testFunVW(Ꮡt, addVWGˢ, new Func<slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, global::go.math.big_package.Word, global::go.math.big_package.Word>(addVW_g), arg);
        testFunVW(Ꮡt, addVWˢ, new Func<slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, global::go.math.big_package.Word, global::go.math.big_package.Word>(addVW), arg);
        arg = new argVW(a.x, a.z, a.y, a.c);
        testFunVW(Ꮡt, subVWGˢ, new Func<slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, global::go.math.big_package.Word, global::go.math.big_package.Word>(subVW_g), arg);
        testFunVW(Ꮡt, subVWˢ, new Func<slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, global::go.math.big_package.Word, global::go.math.big_package.Word>(subVW), arg);
    }
    var shlVW_g = makeFunVW(shlVU_g);
    var shlVW = makeFunVW(shlVU);
    foreach (var (_, a) in lshVW) {
        var arg = a;
        testFunVW(Ꮡt, shlVUGˢ, shlVW_g, arg);
        testFunVW(Ꮡt, shlVUˢ, shlVW, arg);
    }
    var shrVW_g = makeFunVW(shrVU_g);
    var shrVW = makeFunVW(shrVU);
    foreach (var (_, a) in rshVW) {
        var arg = a;
        testFunVW(Ꮡt, shrVUGˢ, shrVW_g, arg);
        testFunVW(Ꮡt, shrVUˢ, shrVW, arg);
    }
}

// Construct a vector comprising the same word, usually '0' or 'maximum uint'
internal static slice<global::go.math.big_package.Word> makeWordVec(global::go.math.big_package.Word e, nint n) {
    var v = new slice<global::go.math.big_package.Word>(n);
    foreach (var (i, _) in v) {
        v[i] = e;
    }
    return v;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string addVWRandomInputsˢ = "addVW, random inputs"u8;
internal static readonly @string subVWRandomInputsˢ = "subVW, random inputs"u8;
internal static readonly @string addVWRandomInputsSharingˢ = "addVW, random inputs, sharing storage"u8;
internal static readonly @string subVWRandomInputsSharingˢ = "subVW, random inputs, sharing storage"u8;
internal static readonly @string addVWVectorOfMaxUintˢ = "addVW, vector of max uint"u8;
internal static readonly @string subVWVectorOfZeroˢ = "subVW, vector of zero"u8;

// Extended testing to addVW and subVW using various kinds of input data.
// We utilize the results of addVW_g and subVW_g as golden reference to check
// correctness.
public static void TestFunVWExt(ж<testing.T> Ꮡt) {
    // 32 is the current threshold that triggers an optimized version of
    // calculation for large-sized vector, ensure we have sizes around it tested.
    slice<nint> vwSizes = new nint[]{0, 1, 3, 4, 5, 8, 9, 23, 31, 32, 33, 34, 35, 36, 50, 120}.slice();
    foreach (var (_, n) in vwSizes) {
        // vector of random numbers, using the result of addVW_g/subVW_g as golden
        var x = rndV(n);
        global::go.math.big_package.Word y = rndW();
        var z = new global::go.math.big_package.nat(n);
        var arg = new argVW(z, x, y, 0);
        testFunVWext(Ꮡt, addVWRandomInputsˢ, new Func<slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, global::go.math.big_package.Word, global::go.math.big_package.Word>(addVW), new Func<slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, global::go.math.big_package.Word, global::go.math.big_package.Word>(addVW_g), arg);
        testFunVWext(Ꮡt, subVWRandomInputsˢ, new Func<slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, global::go.math.big_package.Word, global::go.math.big_package.Word>(subVW), new Func<slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, global::go.math.big_package.Word, global::go.math.big_package.Word>(subVW_g), arg);
        // vector of random numbers, but make 'x' and 'z' share storage
        arg = new argVW(x, x, y, 0);
        testFunVWext(Ꮡt, addVWRandomInputsSharingˢ, new Func<slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, global::go.math.big_package.Word, global::go.math.big_package.Word>(addVW), new Func<slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, global::go.math.big_package.Word, global::go.math.big_package.Word>(addVW_g), arg);
        testFunVWext(Ꮡt, subVWRandomInputsSharingˢ, new Func<slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, global::go.math.big_package.Word, global::go.math.big_package.Word>(subVW), new Func<slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, global::go.math.big_package.Word, global::go.math.big_package.Word>(subVW_g), arg);
        // vector of maximum uint, to force carry flag set in each 'add'
        y = ~((global::go.math.big_package.Word)((global::go.math.big_package.Word)0));
        x = makeWordVec(y, n);
        arg = new argVW(z, x, y, 0);
        testFunVWext(Ꮡt, addVWVectorOfMaxUintˢ, new Func<slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, global::go.math.big_package.Word, global::go.math.big_package.Word>(addVW), new Func<slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, global::go.math.big_package.Word, global::go.math.big_package.Word>(addVW_g), arg);
        // vector of '0', to force carry flag set in each 'sub'
        x = makeWordVec(0, n);
        arg = new argVW(z, x, 1, 0);
        testFunVWext(Ꮡt, subVWVectorOfZeroˢ, new Func<slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, global::go.math.big_package.Word, global::go.math.big_package.Word>(subVW), new Func<slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, global::go.math.big_package.Word, global::go.math.big_package.Word>(subVW_g), arg);
    }
}

internal partial struct argVU {
    internal slice<global::go.math.big_package.Word> d; // d is a Word slice, the input parameters x and z come from this array.
    internal nuint l;  // l is the length of the input parameters x and z.
    internal nuint xp;  // xp is the starting position of the input parameter x, x := d[xp:xp+l].
    internal nuint zp;  // zp is the starting position of the input parameter z, z := d[zp:zp+l].
    internal nuint s;  // s is the shift number.
    internal slice<global::go.math.big_package.Word> r; // r is the expected output result z.
    internal global::go.math.big_package.Word c;   // c is the expected return value.
    internal @string m; // message.
}

internal static slice<global::go.math.big_package.Word> argshlVUIn = new global::go.math.big_package.Word[]{1, 2, 4, 8, 16, 32, 64, 0, 0, 0}.slice();

internal static slice<global::go.math.big_package.Word> argshlVUr0 = new global::go.math.big_package.Word[]{1, 2, 4, 8, 16, 32, 64}.slice();

internal static slice<global::go.math.big_package.Word> argshlVUr1 = new global::go.math.big_package.Word[]{2, 4, 8, 16, 32, 64, 128}.slice();

internal static slice<global::go.math.big_package.Word> argshlVUrWm1 = new global::go.math.big_package.Word[]{(global::go.math.big_package.Word)((nuint)1 << (int)((_W - 1))), 0, 1, 2, 4, 8, 16}.slice();

// test cases for shlVU
// additional test cases with shift values of 0, 1 and (_W-1)
internal static slice<argVU> argshlVU = new argVU[]{
    new(new global::go.math.big_package.Word[]{1, _M, _M, _M, _M, _M, (global::go.math.big_package.Word)((nuint)3 << (int)((_W - 2))), 0}.slice(), 7, 0, 0, 1, new global::go.math.big_package.Word[]{2, _M - 1, _M, _M, _M, _M, unchecked((nuint)(9223372036854775809UL))}.slice(), 1, "complete overlap of shlVU"u8),
    new(new global::go.math.big_package.Word[]{1, _M, _M, _M, _M, _M, (global::go.math.big_package.Word)((nuint)3 << (int)((_W - 2))), 0, 0, 0, 0}.slice(), 7, 0, 3, 1, new global::go.math.big_package.Word[]{2, _M - 1, _M, _M, _M, _M, unchecked((nuint)(9223372036854775809UL))}.slice(), 1, "partial overlap by half of shlVU"u8),
    new(new global::go.math.big_package.Word[]{1, _M, _M, _M, _M, _M, (global::go.math.big_package.Word)((nuint)3 << (int)((_W - 2))), 0, 0, 0, 0, 0, 0, 0}.slice(), 7, 0, 6, 1, new global::go.math.big_package.Word[]{2, _M - 1, _M, _M, _M, _M, unchecked((nuint)(9223372036854775809UL))}.slice(), 1, "partial overlap by 1 Word of shlVU"u8),
    new(new global::go.math.big_package.Word[]{1, _M, _M, _M, _M, _M, (global::go.math.big_package.Word)((nuint)3 << (int)((_W - 2))), 0, 0, 0, 0, 0, 0, 0, 0}.slice(), 7, 0, 7, 1, new global::go.math.big_package.Word[]{2, _M - 1, _M, _M, _M, _M, unchecked((nuint)(9223372036854775809UL))}.slice(), 1, "no overlap of shlVU"u8),
    new(argshlVUIn, 7, 0, 0, 0, argshlVUr0, 0, "complete overlap of shlVU and shift of 0"u8),
    new(argshlVUIn, 7, 0, 0, 1, argshlVUr1, 0, "complete overlap of shlVU and shift of 1"u8),
    new(argshlVUIn, 7, 0, 0, _W - 1, argshlVUrWm1, 32, "complete overlap of shlVU and shift of _W - 1"u8),
    new(argshlVUIn, 7, 0, 1, 0, argshlVUr0, 0, "partial overlap by 6 Words of shlVU and shift of 0"u8),
    new(argshlVUIn, 7, 0, 1, 1, argshlVUr1, 0, "partial overlap by 6 Words of shlVU and shift of 1"u8),
    new(argshlVUIn, 7, 0, 1, _W - 1, argshlVUrWm1, 32, "partial overlap by 6 Words of shlVU and shift of _W - 1"u8),
    new(argshlVUIn, 7, 0, 2, 0, argshlVUr0, 0, "partial overlap by 5 Words of shlVU and shift of 0"u8),
    new(argshlVUIn, 7, 0, 2, 1, argshlVUr1, 0, "partial overlap by 5 Words of shlVU and shift of 1"u8),
    new(argshlVUIn, 7, 0, 2, _W - 1, argshlVUrWm1, 32, "partial overlap by 5 Words of shlVU abd shift of _W - 1"u8),
    new(argshlVUIn, 7, 0, 3, 0, argshlVUr0, 0, "partial overlap by 4 Words of shlVU and shift of 0"u8),
    new(argshlVUIn, 7, 0, 3, 1, argshlVUr1, 0, "partial overlap by 4 Words of shlVU and shift of 1"u8),
    new(argshlVUIn, 7, 0, 3, _W - 1, argshlVUrWm1, 32, "partial overlap by 4 Words of shlVU and shift of _W - 1"u8)
}.slice();

internal static slice<global::go.math.big_package.Word> argshrVUIn = new global::go.math.big_package.Word[]{0, 0, 0, 1, 2, 4, 8, 16, 32, 64}.slice();

internal static slice<global::go.math.big_package.Word> argshrVUr0 = new global::go.math.big_package.Word[]{1, 2, 4, 8, 16, 32, 64}.slice();

internal static slice<global::go.math.big_package.Word> argshrVUr1 = new global::go.math.big_package.Word[]{0, 1, 2, 4, 8, 16, 32}.slice();

internal static slice<global::go.math.big_package.Word> argshrVUrWm1 = new global::go.math.big_package.Word[]{4, 8, 16, 32, 64, 128, 0}.slice();

// test cases for shrVU
// additional test cases with shift values of 0, 1 and (_W-1)
internal static slice<argVU> argshrVU = new argVU[]{
    new(new global::go.math.big_package.Word[]{0, 3, _M, _M, _M, _M, _M, (global::go.math.big_package.Word)((nuint)1 << (int)((_W - 1)))}.slice(), 7, 1, 1, 1, new global::go.math.big_package.Word[]{unchecked((nuint)(9223372036854775809UL)), _M, _M, _M, _M, (global::go.math.big_package.Word)((nuint)_M >> (int)(1)), (global::go.math.big_package.Word)((nuint)1 << (int)((_W - 2)))}.slice(), (global::go.math.big_package.Word)((nuint)1 << (int)((_W - 1))), "complete overlap of shrVU"u8),
    new(new global::go.math.big_package.Word[]{0, 0, 0, 0, 3, _M, _M, _M, _M, _M, (global::go.math.big_package.Word)((nuint)1 << (int)((_W - 1)))}.slice(), 7, 4, 1, 1, new global::go.math.big_package.Word[]{unchecked((nuint)(9223372036854775809UL)), _M, _M, _M, _M, (global::go.math.big_package.Word)((nuint)_M >> (int)(1)), (global::go.math.big_package.Word)((nuint)1 << (int)((_W - 2)))}.slice(), (global::go.math.big_package.Word)((nuint)1 << (int)((_W - 1))), "partial overlap by half of shrVU"u8),
    new(new global::go.math.big_package.Word[]{0, 0, 0, 0, 0, 0, 0, 3, _M, _M, _M, _M, _M, (global::go.math.big_package.Word)((nuint)1 << (int)((_W - 1)))}.slice(), 7, 7, 1, 1, new global::go.math.big_package.Word[]{unchecked((nuint)(9223372036854775809UL)), _M, _M, _M, _M, (global::go.math.big_package.Word)((nuint)_M >> (int)(1)), (global::go.math.big_package.Word)((nuint)1 << (int)((_W - 2)))}.slice(), (global::go.math.big_package.Word)((nuint)1 << (int)((_W - 1))), "partial overlap by 1 Word of shrVU"u8),
    new(new global::go.math.big_package.Word[]{0, 0, 0, 0, 0, 0, 0, 0, 3, _M, _M, _M, _M, _M, (global::go.math.big_package.Word)((nuint)1 << (int)((_W - 1)))}.slice(), 7, 8, 1, 1, new global::go.math.big_package.Word[]{unchecked((nuint)(9223372036854775809UL)), _M, _M, _M, _M, (global::go.math.big_package.Word)((nuint)_M >> (int)(1)), (global::go.math.big_package.Word)((nuint)1 << (int)((_W - 2)))}.slice(), (global::go.math.big_package.Word)((nuint)1 << (int)((_W - 1))), "no overlap of shrVU"u8),
    new(argshrVUIn, 7, 3, 3, 0, argshrVUr0, 0, "complete overlap of shrVU and shift of 0"u8),
    new(argshrVUIn, 7, 3, 3, 1, argshrVUr1, (global::go.math.big_package.Word)((nuint)1 << (int)((_W - 1))), "complete overlap of shrVU and shift of 1"u8),
    new(argshrVUIn, 7, 3, 3, _W - 1, argshrVUrWm1, 2, "complete overlap of shrVU and shift of _W - 1"u8),
    new(argshrVUIn, 7, 3, 2, 0, argshrVUr0, 0, "partial overlap by 6 Words of shrVU and shift of 0"u8),
    new(argshrVUIn, 7, 3, 2, 1, argshrVUr1, (global::go.math.big_package.Word)((nuint)1 << (int)((_W - 1))), "partial overlap by 6 Words of shrVU and shift of 1"u8),
    new(argshrVUIn, 7, 3, 2, _W - 1, argshrVUrWm1, 2, "partial overlap by 6 Words of shrVU and shift of _W - 1"u8),
    new(argshrVUIn, 7, 3, 1, 0, argshrVUr0, 0, "partial overlap by 5 Words of shrVU and shift of 0"u8),
    new(argshrVUIn, 7, 3, 1, 1, argshrVUr1, (global::go.math.big_package.Word)((nuint)1 << (int)((_W - 1))), "partial overlap by 5 Words of shrVU and shift of 1"u8),
    new(argshrVUIn, 7, 3, 1, _W - 1, argshrVUrWm1, 2, "partial overlap by 5 Words of shrVU and shift of _W - 1"u8),
    new(argshrVUIn, 7, 3, 0, 0, argshrVUr0, 0, "partial overlap by 4 Words of shrVU and shift of 0"u8),
    new(argshrVUIn, 7, 3, 0, 1, argshrVUr1, (global::go.math.big_package.Word)((nuint)1 << (int)((_W - 1))), "partial overlap by 4 Words of shrVU and shift of 1"u8),
    new(argshrVUIn, 7, 3, 0, _W - 1, argshrVUrWm1, 2, "partial overlap by 4 Words of shrVU and shift of _W - 1"u8)
}.slice();

internal static void testShiftFunc(ж<testing.T> Ꮡt, Func<slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, nuint, global::go.math.big_package.Word> f, argVU a) {
    // work on copy of a.d to preserve the original data.
    var b = new slice<global::go.math.big_package.Word>(len(a.d));
    copy(b, a.d);
    var z = b.slice((nint)(a.zp), (nint)(a.zp + a.l));
    var x = b.slice((nint)(a.xp), (nint)(a.xp + a.l));
    global::go.math.big_package.Word c = f(z, x, a.s);
    foreach (var (i, zi) in z) {
        if (zi != a.r[i]) {
            Ꮡt.Errorf("d := %v, %s(d[%d:%d], d[%d:%d], %d)\n\tgot z[%d] = %#x; want %#x"u8, a.d, a.m, a.zp, a.zp + a.l, a.xp, a.xp + a.l, a.s, i, zi, a.r[i]);
            break;
        }
    }
    if (c != a.c) {
        Ꮡt.Errorf("d := %v, %s(d[%d:%d], d[%d:%d], %d)\n\tgot c = %#x; want %#x"u8, a.d, a.m, a.zp, a.zp + a.l, a.xp, a.xp + a.l, a.s, c, a.c);
    }
}

public static void TestShiftOverlap(ж<testing.T> Ꮡt) {
    foreach (var (_, a) in argshlVU) {
        var arg = a;
        testShiftFunc(Ꮡt, shlVU, arg);
    }
    foreach (var (_, a) in argshrVU) {
        var arg = a;
        testShiftFunc(Ꮡt, shrVU, arg);
    }
}

public static void TestIssue31084(ж<testing.T> Ꮡt) {
    // compute 10^n via 5^n << n.
    UntypedInt n = 165;
    var p = ((global::go.math.big_package.nat)default!).expNN(new nat(new global::go.math.big_package.Word[]{5}.slice()), new nat(new global::go.math.big_package.Word[]{n}.slice()), default!, false);
    p = p.shl(p, n);
    @string got = ((@string)p.utoa(10));
    @string want = "1"u8 + strings.Repeat("0"u8, n);
    if (got != want) {
        Ꮡt.Errorf("shl(%v, %v)\n\tgot  %s\n\twant %s"u8, p, (nint)(n), got, want);
    }
}

internal static readonly @string issue42838Value = "159309191113245227702888039776771180559110455519261878607388585338616290151305816094308987472018268594098344692611135542392730712890625"u8;

public static void TestIssue42838(ж<testing.T> Ꮡt) {
    UntypedInt s = 192;
    var (z, _, _, _) = ((global::go.math.big_package.nat)default!).scan(new big_test_package.strings_ReaderжByteScanner(strings.NewReader(issue42838Value)), 0, false);
    z = z.shl(z, s);
    @string got = ((@string)z.utoa(10));
    @string want = "1"u8 + strings.Repeat("0"u8, s);
    if (got != want) {
        Ꮡt.Errorf("shl(%v, %v)\n\tgot  %s\n\twant %s"u8, z, (nint)(s), got, want);
    }
}

public static void BenchmarkAddVW(ж<testing.B> Ꮡb) {
    foreach (var (_, n) in benchSizes) {
        if (isRaceBuilder && n > 1000) {
            continue;
        }
        var x = rndV(n);
        global::go.math.big_package.Word y = rndW();
        var z = new slice<global::go.math.big_package.Word>(n);
        var xʗ1 = x;
        var zʗ1 = z;
        Ꮡb.Run(fmt.Sprint(n), (ж<testing.B> bΔ1) => {
            bΔ1.SetBytes((int64)(n * (nint)_S));
            for (nint i = 0; i < (~bΔ1).N; i++) {
                addVW(zʗ1, xʗ1, y);
            }
        });
    }
}

// Benchmarking addVW using vector of maximum uint to force carry flag set
public static void BenchmarkAddVWext(ж<testing.B> Ꮡb) {
    foreach (var (_, n) in benchSizes) {
        if (isRaceBuilder && n > 1000) {
            continue;
        }
        global::go.math.big_package.Word y = ~((global::go.math.big_package.Word)((global::go.math.big_package.Word)0));
        var x = makeWordVec(y, n);
        var z = new slice<global::go.math.big_package.Word>(n);
        var xʗ1 = x;
        var zʗ1 = z;
        Ꮡb.Run(fmt.Sprint(n), (ж<testing.B> bΔ1) => {
            bΔ1.SetBytes((int64)(n * (nint)_S));
            for (nint i = 0; i < (~bΔ1).N; i++) {
                addVW(zʗ1, xʗ1, y);
            }
        });
    }
}

public static void BenchmarkSubVW(ж<testing.B> Ꮡb) {
    foreach (var (_, n) in benchSizes) {
        if (isRaceBuilder && n > 1000) {
            continue;
        }
        var x = rndV(n);
        global::go.math.big_package.Word y = rndW();
        var z = new slice<global::go.math.big_package.Word>(n);
        var xʗ1 = x;
        var zʗ1 = z;
        Ꮡb.Run(fmt.Sprint(n), (ж<testing.B> bΔ1) => {
            bΔ1.SetBytes((int64)(n * (nint)_S));
            for (nint i = 0; i < (~bΔ1).N; i++) {
                subVW(zʗ1, xʗ1, y);
            }
        });
    }
}

// Benchmarking subVW using vector of zero to force carry flag set
public static void BenchmarkSubVWext(ж<testing.B> Ꮡb) {
    foreach (var (_, n) in benchSizes) {
        if (isRaceBuilder && n > 1000) {
            continue;
        }
        var x = makeWordVec(0, n);
        global::go.math.big_package.Word y = ((global::go.math.big_package.Word)1);
        var z = new slice<global::go.math.big_package.Word>(n);
        var xʗ1 = x;
        var zʗ1 = z;
        Ꮡb.Run(fmt.Sprint(n), (ж<testing.B> bΔ1) => {
            bΔ1.SetBytes((int64)(n * (nint)_S));
            for (nint i = 0; i < (~bΔ1).N; i++) {
                subVW(zʗ1, xʗ1, y);
            }
        });
    }
}

// type funVWW is a methodless func type — rendered inline as its base delegate

internal partial struct argVWW {
    internal global::go.math.big_package.nat z, x;
    internal global::go.math.big_package.Word y, r;
    internal global::go.math.big_package.Word c;
}

internal static slice<argVWW> prodVWW = new argVWW[]{
    new(),
    new(new nat(new global::go.math.big_package.Word[]{0}.slice()), new nat(new global::go.math.big_package.Word[]{0}.slice()), 0, 0, 0),
    new(new nat(new global::go.math.big_package.Word[]{991}.slice()), new nat(new global::go.math.big_package.Word[]{0}.slice()), 0, 991, 0),
    new(new nat(new global::go.math.big_package.Word[]{0}.slice()), new nat(new global::go.math.big_package.Word[]{_M}.slice()), 0, 0, 0),
    new(new nat(new global::go.math.big_package.Word[]{991}.slice()), new nat(new global::go.math.big_package.Word[]{_M}.slice()), 0, 991, 0),
    new(new nat(new global::go.math.big_package.Word[]{0}.slice()), new nat(new global::go.math.big_package.Word[]{0}.slice()), _M, 0, 0),
    new(new nat(new global::go.math.big_package.Word[]{991}.slice()), new nat(new global::go.math.big_package.Word[]{0}.slice()), _M, 991, 0),
    new(new nat(new global::go.math.big_package.Word[]{1}.slice()), new nat(new global::go.math.big_package.Word[]{1}.slice()), 1, 0, 0),
    new(new nat(new global::go.math.big_package.Word[]{992}.slice()), new nat(new global::go.math.big_package.Word[]{1}.slice()), 1, 991, 0),
    new(new nat(new global::go.math.big_package.Word[]{22793}.slice()), new nat(new global::go.math.big_package.Word[]{991}.slice()), 23, 0, 0),
    new(new nat(new global::go.math.big_package.Word[]{22800}.slice()), new nat(new global::go.math.big_package.Word[]{991}.slice()), 23, 7, 0),
    new(new nat(new global::go.math.big_package.Word[]{0, 0, 0, 22793}.slice()), new nat(new global::go.math.big_package.Word[]{0, 0, 0, 991}.slice()), 23, 0, 0),
    new(new nat(new global::go.math.big_package.Word[]{7, 0, 0, 22793}.slice()), new nat(new global::go.math.big_package.Word[]{0, 0, 0, 991}.slice()), 23, 7, 0),
    new(new nat(new global::go.math.big_package.Word[]{0, 0, 0, 0}.slice()), new nat(new global::go.math.big_package.Word[]{7893475, 7395495, 798547395, 68943}.slice()), 0, 0, 0),
    new(new nat(new global::go.math.big_package.Word[]{991, 0, 0, 0}.slice()), new nat(new global::go.math.big_package.Word[]{7893475, 7395495, 798547395, 68943}.slice()), 0, 991, 0),
    new(new nat(new global::go.math.big_package.Word[]{0, 0, 0, 0}.slice()), new nat(new global::go.math.big_package.Word[]{0, 0, 0, 0}.slice()), 894375984, 0, 0),
    new(new nat(new global::go.math.big_package.Word[]{991, 0, 0, 0}.slice()), new nat(new global::go.math.big_package.Word[]{0, 0, 0, 0}.slice()), 894375984, 991, 0),
    new(new nat(new global::go.math.big_package.Word[]{unchecked((nuint)(18446744073709551614UL))}.slice()), new nat(new global::go.math.big_package.Word[]{_M}.slice()), (global::go.math.big_package.Word)((nuint)1 << (int)(1)), 0, (global::go.math.big_package.Word)((nuint)_M >> (int)((_W - 1)))),
    new(new nat(new global::go.math.big_package.Word[]{unchecked((nuint)(18446744073709551615UL))}.slice()), new nat(new global::go.math.big_package.Word[]{_M}.slice()), (global::go.math.big_package.Word)((nuint)1 << (int)(1)), 1, (global::go.math.big_package.Word)((nuint)_M >> (int)((_W - 1)))),
    new(new nat(new global::go.math.big_package.Word[]{unchecked((nuint)(18446744073709551488UL))}.slice()), new nat(new global::go.math.big_package.Word[]{_M}.slice()), (global::go.math.big_package.Word)((nuint)1 << (int)(7)), 0, (global::go.math.big_package.Word)((nuint)_M >> (int)((_W - 7)))),
    new(new nat(new global::go.math.big_package.Word[]{unchecked((nuint)(18446744073709551552UL))}.slice()), new nat(new global::go.math.big_package.Word[]{_M}.slice()), (global::go.math.big_package.Word)((nuint)1 << (int)(7)), (global::go.math.big_package.Word)((nuint)1 << (int)(6)), (global::go.math.big_package.Word)((nuint)_M >> (int)((_W - 7)))),
    new(new nat(new global::go.math.big_package.Word[]{unchecked((nuint)(18446744073709551488UL)), _M, _M, _M}.slice()), new nat(new global::go.math.big_package.Word[]{_M, _M, _M, _M}.slice()), (global::go.math.big_package.Word)((nuint)1 << (int)(7)), 0, (global::go.math.big_package.Word)((nuint)_M >> (int)((_W - 7)))),
    new(new nat(new global::go.math.big_package.Word[]{unchecked((nuint)(18446744073709551552UL)), _M, _M, _M}.slice()), new nat(new global::go.math.big_package.Word[]{_M, _M, _M, _M}.slice()), (global::go.math.big_package.Word)((nuint)1 << (int)(7)), (global::go.math.big_package.Word)((nuint)1 << (int)(6)), (global::go.math.big_package.Word)((nuint)_M >> (int)((_W - 7))))
}.slice();

internal static void testFunVWW(ж<testing.T> Ꮡt, @string msg, Func<slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, global::go.math.big_package.Word, global::go.math.big_package.Word, global::go.math.big_package.Word> f, argVWW a) {
    var z = new global::go.math.big_package.nat(len(a.z));
    global::go.math.big_package.Word c = f(z, a.x, a.y, a.r);
    foreach (var (i, zi) in z) {
        if (zi != a.z[i]) {
            Ꮡt.Errorf("%s%+v\n\tgot z[%d] = %#x; want %#x"u8, msg, a, i, zi, a.z[i]);
            break;
        }
    }
    if (c != a.c) {
        Ꮡt.Errorf("%s%+v\n\tgot c = %#x; want %#x"u8, msg, a, c, a.c);
    }
}

// type funWVW is a methodless func type — rendered inline as its base delegate

// TODO(gri) mulAddVWW and divWVW are symmetric operations but
// their signature is not symmetric. Try to unify.
internal partial struct argWVW {
    internal global::go.math.big_package.nat z;
    internal global::go.math.big_package.Word xn;
    internal global::go.math.big_package.nat x;
    internal global::go.math.big_package.Word y;
    internal global::go.math.big_package.Word r;
}

internal static void testFunWVW(ж<testing.T> Ꮡt, @string msg, Func<slice<global::go.math.big_package.Word>, global::go.math.big_package.Word, slice<global::go.math.big_package.Word>, global::go.math.big_package.Word, global::go.math.big_package.Word> f, argWVW a) {
    var z = new global::go.math.big_package.nat(len(a.z));
    global::go.math.big_package.Word r = f(z, a.xn, a.x, a.y);
    foreach (var (i, zi) in z) {
        if (zi != a.z[i]) {
            Ꮡt.Errorf("%s%+v\n\tgot z[%d] = %#x; want %#x"u8, msg, a, i, zi, a.z[i]);
            break;
        }
    }
    if (r != a.r) {
        Ꮡt.Errorf("%s%+v\n\tgot r = %#x; want %#x"u8, msg, a, r, a.r);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string mulAddVWWGˢ = "mulAddVWW_g"u8;
internal static readonly @string mulAddVWWˢ = "mulAddVWW"u8;
internal static readonly @string divWVWˢ = "divWVW"u8;

public static void TestFunVWW(ж<testing.T> Ꮡt) {
    foreach (var (_, a) in prodVWW) {
        var arg = a;
        testFunVWW(Ꮡt, mulAddVWWGˢ, new Func<slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, global::go.math.big_package.Word, global::go.math.big_package.Word, global::go.math.big_package.Word>(mulAddVWW_g), arg);
        testFunVWW(Ꮡt, mulAddVWWˢ, new Func<slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, global::go.math.big_package.Word, global::go.math.big_package.Word, global::go.math.big_package.Word>(mulAddVWW), arg);
        if (a.y != 0 && a.r < a.y) {
            var argΔ1 = new argWVW(a.x, a.c, a.z, a.y, a.r);
            testFunWVW(Ꮡt, divWVWˢ, new Func<slice<global::go.math.big_package.Word>, global::go.math.big_package.Word, slice<global::go.math.big_package.Word>, global::go.math.big_package.Word, global::go.math.big_package.Word>(divWVW), argΔ1);
        }
    }
}

// 32 bit only: {0xc47dfa8c, 50911, 0x98a4, 0x998587f4},

partial struct mulWWTestsᴛ1 /*dyn*/ {
    internal global::go.math.big_package.Word x, y;
    internal global::go.math.big_package.Word q, r;
}
internal static slice<mulWWTestsᴛ1> mulWWTests = new mulWWTestsᴛ1[]{
    new(_M, _M, _M - 1, 1)
}.slice();

public static void TestMulWW(ж<testing.T> Ꮡt) {
    foreach (var (i, test) in mulWWTests) {
        var (q, r) = mulWW(test.x, test.y);
        if (q != test.q || r != test.r) {
            Ꮡt.Errorf("#%d got (%x, %x) want (%x, %x)"u8, i, q, r, test.q, test.r);
        }
    }
}

// TODO(agl): These will only work on 64-bit platforms.
// {15064310297182388543, 0xe7df04d2d35d5d80, 13537600649892366549, 13644450054494335067, 10832252001440893781},
// {15064310297182388543, 0xdab2f18048baa68d, 13644450054494335067, 12869334219691522700, 14233854684711418382},

partial struct mulAddWWWTestsᴛ1 /*dyn*/ {
    internal global::go.math.big_package.Word x, y, c;
    internal global::go.math.big_package.Word q, r;
}
internal static slice<mulAddWWWTestsᴛ1> mulAddWWWTests = new mulAddWWWTestsᴛ1[]{
    new(_M, _M, 0, _M - 1, 1),
    new(_M, _M, _M, _M, 0)
}.slice();

public static void TestMulAddWWW(ж<testing.T> Ꮡt) {
    foreach (var (i, test) in mulAddWWWTests) {
        var (q, r) = mulAddWWW_g(test.x, test.y, test.c);
        if (q != test.q || r != test.r) {
            Ꮡt.Errorf("#%d got (%x, %x) want (%x, %x)"u8, i, q, r, test.q, test.r);
        }
    }
}


partial struct divWWTestsᴛ1 /*dyn*/ {
    internal global::go.math.big_package.Word x1, x0, y;
    internal global::go.math.big_package.Word q, r;
}
internal static slice<divWWTestsᴛ1> divWWTests = new divWWTestsᴛ1[]{
    new((global::go.math.big_package.Word)((nuint)_M >> (int)(1)), 0, _M, (global::go.math.big_package.Word)((nuint)_M >> (int)(1)), (global::go.math.big_package.Word)((nuint)_M >> (int)(1))),
    new(_M - (4611686018427387904L), _M, (global::go.math.big_package.Word)((nuint)3 << (int)((_W - 2))), _M, _M - (4611686018427387904L))
}.slice();

internal static UntypedInt testsNumber => /* 1 << 16 */ 65536;

public static void TestDivWW(ж<testing.T> Ꮡt) {
    nint i = 0;
    foreach (var (iΔ1, test) in divWWTests) {
        global::go.math.big_package.Word rec = reciprocalWord(test.y);
        var (q, r) = divWW(test.x1, test.x0, test.y, rec);
        if (q != test.q || r != test.r) {
            Ꮡt.Errorf("#%d got (%x, %x) want (%x, %x)"u8, iΔ1, q, r, test.q, test.r);
        }
    }
    //random tests
    for (; i < testsNumber; i++) {
        global::go.math.big_package.Word x1 = rndW();
        global::go.math.big_package.Word x0 = rndW();
        global::go.math.big_package.Word y = rndW();
        if (x1 >= y) {
            continue;
        }
        global::go.math.big_package.Word rec = reciprocalWord(y);
        var (qGot, rGot) = divWW(x1, x0, y, rec);
        var (qWant, rWant) = bits.Div((nuint)x1, (nuint)x0, (nuint)y);
        if ((nuint)qGot != qWant || (nuint)rGot != rWant) {
            Ꮡt.Errorf("#%d got (%x, %x) want (%x, %x)"u8, i, qGot, rGot, qWant, rWant);
        }
    }
}

public static void BenchmarkMulAddVWW(ж<testing.B> Ꮡb) {
    foreach (var (_, n) in benchSizes) {
        if (isRaceBuilder && n > 1000) {
            continue;
        }
        var z = new slice<global::go.math.big_package.Word>(n + 1);
        var x = rndV(n);
        global::go.math.big_package.Word y = rndW();
        global::go.math.big_package.Word r = rndW();
        var xʗ1 = x;
        var zʗ1 = z;
        Ꮡb.Run(fmt.Sprint(n), (ж<testing.B> bΔ1) => {
            bΔ1.SetBytes((int64)(n * (nint)_W));
            for (nint i = 0; i < (~bΔ1).N; i++) {
                mulAddVWW(zʗ1, xʗ1, y, r);
            }
        });
    }
}

public static void BenchmarkAddMulVVW(ж<testing.B> Ꮡb) {
    foreach (var (_, n) in benchSizes) {
        if (isRaceBuilder && n > 1000) {
            continue;
        }
        var x = rndV(n);
        global::go.math.big_package.Word y = rndW();
        var z = new slice<global::go.math.big_package.Word>(n);
        var xʗ1 = x;
        var zʗ1 = z;
        Ꮡb.Run(fmt.Sprint(n), (ж<testing.B> bΔ1) => {
            bΔ1.SetBytes((int64)(n * (nint)_W));
            for (nint i = 0; i < (~bΔ1).N; i++) {
                addMulVVW(zʗ1, xʗ1, y);
            }
        });
    }
}

public static void BenchmarkDivWVW(ж<testing.B> Ꮡb) {
    foreach (var (_, n) in benchSizes) {
        if (isRaceBuilder && n > 1000) {
            continue;
        }
        var x = rndV(n);
        global::go.math.big_package.Word y = rndW();
        var z = new slice<global::go.math.big_package.Word>(n);
        var xʗ1 = x;
        var zʗ1 = z;
        Ꮡb.Run(fmt.Sprint(n), (ж<testing.B> bΔ1) => {
            bΔ1.SetBytes((int64)(n * (nint)_W));
            for (nint i = 0; i < (~bΔ1).N; i++) {
                divWVW(zʗ1, 0, xʗ1, y);
            }
        });
    }
}

public static void BenchmarkNonZeroShifts(ж<testing.B> Ꮡb) {
    foreach (var (_, n) in benchSizes) {
        if (isRaceBuilder && n > 1000) {
            continue;
        }
        var x = rndV(n);
        nuint s = (nuint)Δrand.Int63n(_W - 2) + 1; // avoid 0 and over-large shifts
        var z = new slice<global::go.math.big_package.Word>(n);
        var xʗ1 = x;
        var zʗ1 = z;
        Ꮡb.Run(fmt.Sprint(n), (ж<testing.B> bΔ1) => {
            bΔ1.SetBytes((int64)(n * (nint)_W));
            var xʗ2 = xʗ1;
            var zʗ2 = zʗ1;
            bΔ1.Run(shrVUˢ, (ж<testing.B> bΔ2) => {
                for (nint i = 0; i < (~bΔ2).N; i++) {
                    _ = shrVU(zʗ2, xʗ2, s);
                }
            });
            var xʗ3 = xʗ1;
            var zʗ3 = zʗ1;
            bΔ1.Run(shlVUˢ, (ж<testing.B> bΔ3) => {
                for (nint i = 0; i < (~bΔ3).N; i++) {
                    _ = shlVU(zʗ3, xʗ3, s);
                }
            });
        });
    }
}

} // end big_internal_test_package
