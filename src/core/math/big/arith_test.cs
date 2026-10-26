// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file defines tests of consistent behavior between assembly and Go versions of basic operators,
// as well as tests of pure Go implementations.
namespace go.math;

using fmt = fmt_package;
using testenv = @internal.testenv_package;
using iter = iter_package;
using bits = go.math.bits_package;
using Δrand = go.math.rand.rand_package;
using slices = slices_package;
using strings = strings_package;
using testing = testing_package;
using @internal;
using go.math;
using go.math.rand;
using io = io_package;
using static go.math.big_package;

partial class big_internal_test_package {

internal static bool isRaceBuilder = strings.HasSuffix(testenv.Builder(), "-race"u8);

internal static slice<global::go.math.big_package.Word> words4 = new global::go.math.big_package.Word[]{0, 1, _M - 1, _M}.slice();

internal static slice<global::go.math.big_package.Word> words2 = new global::go.math.big_package.Word[]{0, _M}.slice();

internal static slice<global::go.math.big_package.Word> muls = new global::go.math.big_package.Word[]{0, 1, 2, 3, 4, 5, unchecked((nuint)(/* _M / 4 */ 4611686018427387903UL)), unchecked((nuint)(/* _M / 2 */ 9223372036854775807UL)), _M - 3, _M - 2, _M - 1, _M}.slice();

internal static slice<global::go.math.big_package.Word> adds = new global::go.math.big_package.Word[]{0, 1, _M - 1, _M}.slice();

internal static slice<nuint> shifts = new nuint[]{1, 2, 3, _W / 4 - 1, _W / 4, _W / 4 + 1, _W / 2 - 1, _W / 2, _W / 2 + 1, _W - 3, _W - 2, _W - 1}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string addVVˢ = "addVV"u8;

public static void TestAddVV(ж<testing.T> Ꮡt) {
    testVV(Ꮡt, addVVˢ, addVV, addVV_g);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string subVVˢ = "subVV"u8;

public static void TestSubVV(ж<testing.T> Ꮡt) {
    testVV(Ꮡt, subVVˢ, subVV, subVV_g);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string addVWˢ = "addVW"u8;

public static void TestAddVW(ж<testing.T> Ꮡt) {
    testVW(Ꮡt, addVWˢ, addVW, addVW_ref, words4);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string subVWˢ = "subVW"u8;

public static void TestSubVW(ж<testing.T> Ꮡt) {
    testVW(Ꮡt, subVWˢ, subVW, subVW_ref, words4);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string lshVUˢ = "lshVU"u8;

public static void TestLshVU(ж<testing.T> Ꮡt) {
    testVU(Ꮡt, lshVUˢ, lshVU, lshVU_g, shifts);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string rshVUˢ = "rshVU"u8;

public static void TestRshVU(ж<testing.T> Ꮡt) {
    testVU(Ꮡt, rshVUˢ, rshVU, rshVU_g, shifts);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string mulAddVWWˢ = "mulAddVWW"u8;

public static void TestMulAddVWW(ж<testing.T> Ꮡt) {
    testVWW(Ꮡt, mulAddVWWˢ, mulAddVWW, mulAddVWW_g, muls);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string addMulVVWWˢ = "addMulVVWW"u8;

public static void TestAddMulVVWW(ж<testing.T> Ꮡt) {
    testVVWW(Ꮡt, addMulVVWWˢ, addMulVVWW, addMulVVWW_g, muls, adds);
}

// Note: It would be nice to avoid all the duplication of these test variants,
// but the only obvious way is to use reflection. These tests are already
// pretty expensive, and hitting them with reflect call overhead would
// reduce the amount of exhaustive testing it's reasonable to do, so instead
// we put up with the duplication.
internal static void testVV(ж<testing.T> Ꮡt, @string name, Func<slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, global::go.math.big_package.Word> fn, Func<slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, global::go.math.big_package.Word> @ref) {
    foreach (var size in range(100)) {
        var xx = new slice<global::go.math.big_package.Word>(1 + size + 1);
        var yy = new slice<global::go.math.big_package.Word>(1 + size + 1);
        var zz = new slice<global::go.math.big_package.Word>(1 + size + 1);
        var words = words4;
        if (size > 5) {
            words = words2;
        }
        if (size > 10) {
            words = default!; // random
        }
        foreach (var x in range<slice<global::go.math.big_package.Word>>(nats(words, size).Invoke)) {
            foreach (var y in range<slice<global::go.math.big_package.Word>>(nats(words, size).Invoke)) {
                var wantZ = new slice<global::go.math.big_package.Word>(size);
                global::go.math.big_package.Word wantC = @ref(wantZ, x, y);

                foreach (var (_, inplace) in new bool[]{false, true}.slice()) {
                    @string nameΔ1 = name;
                    if (inplace) {
                        nameΔ1 = "in-place "u8 + nameΔ1;
                    }
                    setSlice(xx, 1, x);
                    setSlice(yy, 2, y);
                    var zzΔ1 = zz;
                    if (inplace){
                        zzΔ1 = xx;
                    } else {
                        foreach (var i in range(zzΔ1)) {
                            zzΔ1[i] = 0x9876;
                        }
                    }
                    setSlice(zzΔ1, 3, default!);
                    global::go.math.big_package.Word c = fn(zzΔ1.slice(1, 1 + size), xx.slice(1, 1 + size), yy.slice(1, 1 + size));
                    if (!slices.Equal<slice<global::go.math.big_package.Word>, global::go.math.big_package.Word>(zzΔ1.slice(1, 1 + size), wantZ) || c != wantC) {
                        Ꮡt.Errorf("%s(%#x, %#x) = %#x, %#x, want %#x, %#x"u8, nameΔ1, x, y, zzΔ1.slice(1, 1 + size), c, wantZ, wantC);
                    }
                    if (!inplace) {
                        checkSlice(Ꮡt, nameΔ1, "x"u8, xx, 1, x);
                    }
                    checkSlice(Ꮡt, nameΔ1, "y"u8, yy, 2, y);
                    checkSlice(Ꮡt, nameΔ1, "z"u8, zzΔ1, 3, default!);
                    if (Ꮡt.Failed()) {
                        Ꮡt.FailNow();
                    }
                }
            }
        }
    }
}

internal static void testVV2(ж<testing.T> Ꮡt, @string name, Func<slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, (global::go.math.big_package.Word, global::go.math.big_package.Word)> fn, Func<slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, (global::go.math.big_package.Word, global::go.math.big_package.Word)> @ref) {
    foreach (var size in range(100)) {
        var xx = new slice<global::go.math.big_package.Word>(1 + size + 1);
        var yy = new slice<global::go.math.big_package.Word>(1 + size + 1);
        var zz1 = new slice<global::go.math.big_package.Word>(1 + size + 1);
        var zz2 = new slice<global::go.math.big_package.Word>(1 + size + 1);
        var words = words4;
        if (size > 5) {
            words = words2;
        }
        if (size > 10) {
            words = default!; // random
        }
        foreach (var x in range<slice<global::go.math.big_package.Word>>(nats(words, size).Invoke)) {
            foreach (var y in range<slice<global::go.math.big_package.Word>>(nats(words, size).Invoke)) {
                var wantZ1 = new slice<global::go.math.big_package.Word>(size);
                var wantZ2 = new slice<global::go.math.big_package.Word>(size);
                var (wantC1, wantC2) = @ref(wantZ1, wantZ2, x, y);

                foreach (var (_, inplace) in new bool[]{false, true}.slice()) {
                    @string nameΔ1 = name;
                    if (inplace) {
                        nameΔ1 = "in-place "u8 + nameΔ1;
                    }
                    setSlice(xx, 1, x);
                    setSlice(yy, 2, y);
                    var zz1Δ1 = zz1;
                    var zz2Δ1 = zz2;
                    if (inplace){
                        zz1Δ1 = xx;
                        zz2Δ1 = yy;
                    } else {
                        foreach (var i in range(zz1Δ1)) {
                            zz1Δ1[i] = 0x9876;
                        }
                        foreach (var i in range(zz2Δ1)) {
                            zz2Δ1[i] = 0x8765;
                        }
                    }
                    setSlice(zz1Δ1, 3, default!);
                    setSlice(zz2Δ1, 4, default!);
                    var (c1, c2) = fn(zz1Δ1.slice(1, 1 + size), zz2Δ1.slice(1, 1 + size), xx.slice(1, 1 + size), yy.slice(1, 1 + size));
                    if (!slices.Equal<slice<global::go.math.big_package.Word>, global::go.math.big_package.Word>(zz1Δ1.slice(1, 1 + size), wantZ1) || !slices.Equal<slice<global::go.math.big_package.Word>, global::go.math.big_package.Word>(zz2Δ1.slice(1, 1 + size), wantZ2) || c1 != wantC1 || c2 != wantC2) {
                        Ꮡt.Errorf("%s(%#x, %#x) = %#x, %#x, %#x, %#x, want %#x, %#x, %#x, %#x"u8, nameΔ1, x, y, zz1Δ1.slice(1, 1 + size), zz2Δ1.slice(1, 1 + size), c1, c2, wantZ1, wantZ2, wantC1, wantC2);
                    }
                    if (!inplace) {
                        checkSlice(Ꮡt, nameΔ1, "x"u8, xx, 1, x);
                        checkSlice(Ꮡt, nameΔ1, "y"u8, yy, 2, y);
                    }
                    checkSlice(Ꮡt, nameΔ1, "z1"u8, zz1Δ1, 3, default!);
                    checkSlice(Ꮡt, nameΔ1, "z2"u8, zz2Δ1, 4, default!);
                    if (Ꮡt.Failed()) {
                        Ꮡt.FailNow();
                    }
                }
            }
        }
    }
}

internal static void testVW(ж<testing.T> Ꮡt, @string name, Func<slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, global::go.math.big_package.Word, global::go.math.big_package.Word> fn, Func<slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, global::go.math.big_package.Word, global::go.math.big_package.Word> @ref, slice<global::go.math.big_package.Word> ws) {
    UntypedInt magic0 = 0x123450;
    UntypedInt magic1 = 0x543210;

    foreach (var size in range(100)) {
        var xx = new slice<global::go.math.big_package.Word>(1 + size + 1);
        var zz = new slice<global::go.math.big_package.Word>(1 + size + 1);
        var words = words4;
        if (size > 5) {
            words = words2;
        }
        if (size > 10) {
            words = default!; // random
        }
        foreach (var x in range<slice<global::go.math.big_package.Word>>(nats(words, size).Invoke)) {
            foreach (var (_, w) in ws) {
                var wantZ = new slice<global::go.math.big_package.Word>(size);
                global::go.math.big_package.Word wantC = @ref(wantZ, x, w);

                copy(xx[1..], x);
                foreach (var (_, inplace) in new bool[]{false, true}.slice()) {
                    @string nameΔ1 = name;
                    if (inplace) {
                        nameΔ1 = "in-place "u8 + nameΔ1;
                    }
                    setSlice(xx, 1, x);
                    var zzΔ1 = zz;
                    if (inplace){
                        zzΔ1 = xx;
                    } else {
                        foreach (var i in range(zzΔ1)) {
                            zzΔ1[i] = 0x9876;
                        }
                    }
                    setSlice(zzΔ1, 2, default!);
                    global::go.math.big_package.Word c = fn(zzΔ1.slice(1, 1 + size), xx.slice(1, 1 + size), w);
                    if (!slices.Equal<slice<global::go.math.big_package.Word>, global::go.math.big_package.Word>(zzΔ1.slice(1, 1 + size), wantZ) || c != wantC) {
                        Ꮡt.Errorf("%s(%#x, %#x) = %#x, %#x, want %#x, %#x"u8, nameΔ1, x, w, zzΔ1.slice(1, 1 + size), c, wantZ, wantC);
                    }
                    if (!inplace) {
                        checkSlice(Ꮡt, nameΔ1, "x"u8, xx, 1, x);
                    }
                    checkSlice(Ꮡt, nameΔ1, "z"u8, zzΔ1, 2, default!);
                    if (Ꮡt.Failed()) {
                        Ꮡt.FailNow();
                    }
                }
            }
        }
    }
}

internal static void testVU(ж<testing.T> Ꮡt, @string name, Func<slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, nuint, global::go.math.big_package.Word> fn, Func<slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, nuint, global::go.math.big_package.Word> @ref, slice<nuint> ys) {
    var wys = new slice<global::go.math.big_package.Word>(len(ys));
    foreach (var (i, y) in ys) {
        wys[i] = (global::go.math.big_package.Word)y;
    }
    testVW(Ꮡt, name,
        (slice<global::go.math.big_package.Word> z, slice<global::go.math.big_package.Word> x, global::go.math.big_package.Word y) => fn(z, x, (nuint)y),
        (slice<global::go.math.big_package.Word> z, slice<global::go.math.big_package.Word> x, global::go.math.big_package.Word y) => @ref(z, x, (nuint)y),
        wys);
}

internal static void testVWW(ж<testing.T> Ꮡt, @string name, Func<slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, global::go.math.big_package.Word, global::go.math.big_package.Word, global::go.math.big_package.Word> fn, Func<slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, global::go.math.big_package.Word, global::go.math.big_package.Word, global::go.math.big_package.Word> @ref, slice<global::go.math.big_package.Word> ys) {
    UntypedInt magic0 = 0x123450;
    UntypedInt magic1 = 0x543210;

    foreach (var size in range(100)) {
        var xx = new slice<global::go.math.big_package.Word>(1 + size + 1);
        var zz = new slice<global::go.math.big_package.Word>(1 + size + 1);
        var words = words4;
        if (size > 5) {
            words = words2;
        }
        if (size > 10) {
            words = default!; // random
        }
        foreach (var x in range<slice<global::go.math.big_package.Word>>(nats(words, size).Invoke)) {
            foreach (var (_, y) in ys) {
                foreach (var (_, r) in ys) {
                    var wantZ = new slice<global::go.math.big_package.Word>(size);
                    global::go.math.big_package.Word wantC = @ref(wantZ, x, y, r);

                    copy(xx[1..], x);
                    foreach (var (_, inplace) in new bool[]{false, true}.slice()) {
                        @string nameΔ1 = name;
                        if (inplace) {
                            nameΔ1 = "in-place "u8 + nameΔ1;
                        }
                        setSlice(xx, 1, x);
                        var zzΔ1 = zz;
                        if (inplace){
                            zzΔ1 = xx;
                        } else {
                            foreach (var i in range(zzΔ1)) {
                                zzΔ1[i] = 0x9876;
                            }
                        }
                        setSlice(zzΔ1, 2, default!);
                        global::go.math.big_package.Word c = fn(zzΔ1.slice(1, 1 + size), xx.slice(1, 1 + size), y, r);
                        if (!slices.Equal<slice<global::go.math.big_package.Word>, global::go.math.big_package.Word>(zzΔ1.slice(1, 1 + size), wantZ) || c != wantC) {
                            Ꮡt.Errorf("%s(%#x, %#x, %#x) = %#x, %#x, want %#x, %#x"u8, nameΔ1, x, y, r, zzΔ1.slice(1, 1 + size), c, wantZ, wantC);
                        }
                        if (!inplace) {
                            checkSlice(Ꮡt, nameΔ1, "x"u8, xx, 1, x);
                        }
                        checkSlice(Ꮡt, nameΔ1, "z"u8, zzΔ1, 2, default!);
                        if (Ꮡt.Failed()) {
                            Ꮡt.FailNow();
                        }
                    }
                }
            }
        }
    }
}

internal static void testVVU(ж<testing.T> Ꮡt, @string name, Func<slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, nuint, global::go.math.big_package.Word> fn, Func<slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, nuint, global::go.math.big_package.Word> @ref, slice<nuint> shifts) {
    foreach (var size in range(100)) {
        var xx = new slice<global::go.math.big_package.Word>(1 + size + 1);
        var yy = new slice<global::go.math.big_package.Word>(1 + size + 1);
        var zz = new slice<global::go.math.big_package.Word>(1 + size + 1);
        var words = words4;
        if (size > 5) {
            words = words2;
        }
        if (size > 10) {
            words = default!; // random
        }
        foreach (var x in range<slice<global::go.math.big_package.Word>>(nats(words, size).Invoke)) {
            foreach (var y in range<slice<global::go.math.big_package.Word>>(nats(words, size).Invoke)) {
                foreach (var (_, s) in shifts) {
                    var wantZ = new slice<global::go.math.big_package.Word>(size);
                    global::go.math.big_package.Word wantC = @ref(wantZ, x, y, s);

                    foreach (var (_, inplace) in new bool[]{false, true}.slice()) {
                        @string nameΔ1 = name;
                        if (inplace) {
                            nameΔ1 = "in-place "u8 + nameΔ1;
                        }
                        setSlice(xx, 1, x);
                        setSlice(yy, 2, y);
                        var zzΔ1 = zz;
                        if (inplace){
                            zzΔ1 = xx;
                        } else {
                            foreach (var i in range(zzΔ1)) {
                                zzΔ1[i] = 0x9876;
                            }
                        }
                        setSlice(zzΔ1, 3, default!);
                        global::go.math.big_package.Word c = fn(zzΔ1.slice(1, 1 + size), xx.slice(1, 1 + size), yy.slice(1, 1 + size), s);
                        if (!slices.Equal<slice<global::go.math.big_package.Word>, global::go.math.big_package.Word>(zzΔ1.slice(1, 1 + size), wantZ) || c != wantC) {
                            Ꮡt.Errorf("%s(%#x, %#x, %#x) = %#x, %#x, want %#x, %#x"u8, nameΔ1, x, y, s, zzΔ1.slice(1, 1 + size), c, wantZ, wantC);
                        }
                        if (!inplace) {
                            checkSlice(Ꮡt, nameΔ1, "x"u8, xx, 1, x);
                        }
                        checkSlice(Ꮡt, nameΔ1, "y"u8, yy, 2, y);
                        checkSlice(Ꮡt, nameΔ1, "z"u8, zzΔ1, 3, default!);
                        if (Ꮡt.Failed()) {
                            Ꮡt.FailNow();
                        }
                    }
                }
            }
        }
    }
}

internal static void testVVWW(ж<testing.T> Ꮡt, @string name, Func<slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, global::go.math.big_package.Word, global::go.math.big_package.Word, global::go.math.big_package.Word> fn, Func<slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, global::go.math.big_package.Word, global::go.math.big_package.Word, global::go.math.big_package.Word> @ref, slice<global::go.math.big_package.Word> ms, slice<global::go.math.big_package.Word> @as) {
    foreach (var size in range(100)) {
        var zz = new slice<global::go.math.big_package.Word>(1 + size + 1);
        var xx = new slice<global::go.math.big_package.Word>(1 + size + 1);
        var yy = new slice<global::go.math.big_package.Word>(1 + size + 1);
        var words = words4;
        if (size > 3) {
            words = words2;
        }
        if (size > 7) {
            words = default!; // random
        }
        foreach (var x in range<slice<global::go.math.big_package.Word>>(nats(words, size).Invoke)) {
            foreach (var y in range<slice<global::go.math.big_package.Word>>(nats(words, size).Invoke)) {
                foreach (var (_, m) in ms) {
                    foreach (var (_, a) in @as) {
                        var wantZ = new slice<global::go.math.big_package.Word>(size);
                        global::go.math.big_package.Word wantC = @ref(wantZ, x, y, m, a);

                        foreach (var (_, inplace) in new bool[]{false, true}.slice()) {
                            @string nameΔ1 = name;
                            if (inplace) {
                                nameΔ1 = "in-place "u8 + nameΔ1;
                            }
                            setSlice(xx, 1, x);
                            setSlice(yy, 2, y);
                            var zzΔ1 = zz;
                            if (inplace){
                                zzΔ1 = xx;
                            } else {
                                foreach (var i in range(zzΔ1)) {
                                    zzΔ1[i] = 0x9876;
                                }
                            }
                            setSlice(zzΔ1, 3, default!);
                            global::go.math.big_package.Word c = fn(zzΔ1.slice(1, 1 + size), xx.slice(1, 1 + size), yy.slice(1, 1 + size), m, a);
                            if (!slices.Equal<slice<global::go.math.big_package.Word>, global::go.math.big_package.Word>(zzΔ1.slice(1, 1 + size), wantZ) || c != wantC) {
                                Ꮡt.Errorf("%s(%#x, %#x, %#x, %#x) = %#x, %#x, want %#x, %#x"u8, nameΔ1, x, y, m, a, zzΔ1.slice(1, 1 + size), c, wantZ, wantC);
                            }
                            if (!inplace) {
                                checkSlice(Ꮡt, nameΔ1, "x"u8, xx, 1, x);
                            }
                            checkSlice(Ꮡt, nameΔ1, "y"u8, yy, 2, y);
                            checkSlice(Ꮡt, nameΔ1, "z"u8, zzΔ1, 3, default!);
                            if (Ꮡt.Failed()) {
                                Ꮡt.FailNow();
                            }
                        }
                    }
                }
            }
        }
    }
}

internal static UntypedInt magic0 => 0x123450;
internal static UntypedInt magic1 => 0x543210;

// setSlice sets x[1:len(x)-1] to orig, leaving magic values in x[0] and x[len(x)-1]
// so that we can tell if routines accidentally write before or after the data.
internal static void setSlice(slice<global::go.math.big_package.Word> x, global::go.math.big_package.Word id, slice<global::go.math.big_package.Word> orig) {
    x[0] = (nuint)magic0 + id;
    copy(x.slice(1, len(x) - 1), orig);
    x[len(x) - 1] = (nuint)magic1 + id;
}

// checkSlice checks that the magic values left by setSlices are still there.
// If orig != nil, it also checks that the actual data in x is unmodified since setSlice.
internal static void checkSlice(ж<testing.T> Ꮡt, @string name, @string val, slice<global::go.math.big_package.Word> x, global::go.math.big_package.Word id, slice<global::go.math.big_package.Word> orig) {
    if (x[0] != (nuint)magic0 + id) {
        Ꮡt.Errorf("%s smashed %s[-1]"u8, name, val);
    }
    if (x[len(x) - 1] != (nuint)magic1 + id) {
        Ꮡt.Errorf("%s smashed %s[len(%s)]"u8, name, val, val);
    }
    if (orig != default! && !slices.Equal<slice<global::go.math.big_package.Word>, global::go.math.big_package.Word>(x.slice(1, len(x) - 1), orig)) {
        Ꮡt.Errorf("%s smashed %s: have %d, want %d"u8, name, val, x.slice(1, len(x) - 1), orig);
    }
}

// nats returns a sequence of interesting nats of the given size:
//
//   - all 0
//   - all ^0
//   - all possible combinations of words
//   - ten random values
internal static iter.Seq<slice<global::go.math.big_package.Word>> nats(slice<global::go.math.big_package.Word> words, nint size) {
    var wordsʗ1 = words;
    return (Func<slice<global::go.math.big_package.Word>, bool> yield) => {
        if (size == 0) {
            yield(default!);
            return;
        }
        var w = new slice<global::go.math.big_package.Word>(size);

        // all 0
        foreach (var i in range(w)) {
            w[i] = 0;
        }
        if (!yield(w)) {
            return;
        }

        // all ^0
        foreach (var i in range(w)) {
            w[i] = _M;
        }
        if (!yield(w)) {
            return;
        }

        // all possible combinations of words
        ref var generate = ref heap<Func<nint, bool>>(out var Ꮡgenerate);
        var wʗ1 = w;
        var wordsʗ2 = wordsʗ1;
        Ꮡgenerate.ValueSlot = (nint i) => {
            if (i >= len(wʗ1)) {
                return yield(wʗ1);
            }
            foreach (var (_, vᴛ1) in wordsʗ2) {
                wʗ1[i] = vᴛ1;

                if (!Ꮡgenerate.ValueSlot(i + 1)) {
                    return false;
                }
            }
            return true;
        };
        if (!Ꮡgenerate.ValueSlot(0)) {
            return;
        }

        // ten random values
        foreach (var _ᴛ1 in range(10)) {
            foreach (var i in range(w)) {
                w[i] = (global::go.math.big_package.Word)rnd.Uint();
            }
            if (!yield(w)) {
                return;
            }
        }
    };
}

// Always the same seed for reproducible results.
internal static ж<Δrand.Rand> rnd = Δrand.New(new Δrand.PCGжSource(Δrand.NewPCG(1, 2)));

internal static global::go.math.big_package.Word rndW() {
    return (global::go.math.big_package.Word)rnd.Uint();
}

internal static slice<global::go.math.big_package.Word> rndV(nint n) {
    var v = new slice<global::go.math.big_package.Word>(n);
    foreach (var i in range(v)) {
        v[i] = rndW();
    }
    return v;
}

// Construct a vector comprising the same word, usually '0' or 'maximum uint'
internal static slice<global::go.math.big_package.Word> makeWordVec(global::go.math.big_package.Word e, nint n) {
    var v = new slice<global::go.math.big_package.Word>(n);
    foreach (var i in range(v)) {
        v[i] = e;
    }
    return v;
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

internal static slice<global::go.math.big_package.Word> arglshVUIn = new global::go.math.big_package.Word[]{1, 2, 4, 8, 16, 32, 64, 0, 0, 0}.slice();

internal static slice<global::go.math.big_package.Word> arglshVUr0 = new global::go.math.big_package.Word[]{1, 2, 4, 8, 16, 32, 64}.slice();

internal static slice<global::go.math.big_package.Word> arglshVUr1 = new global::go.math.big_package.Word[]{2, 4, 8, 16, 32, 64, 128}.slice();

internal static slice<global::go.math.big_package.Word> arglshVUrWm1 = new global::go.math.big_package.Word[]{(global::go.math.big_package.Word)((nuint)1 << (int)(_W - 1)), 0, 1, 2, 4, 8, 16}.slice();

internal static slice<argVU> arglshVU = new argVU[]{
    // test cases for lshVU
    new(new global::go.math.big_package.Word[]{1, _M, _M, _M, _M, _M, (global::go.math.big_package.Word)((nuint)3 << (int)(_W - 2)), 0}.slice(), 7, 0, 0, 1, new global::go.math.big_package.Word[]{2, _M - 1, _M, _M, _M, _M, unchecked((nuint)(9223372036854775809UL))}.slice(), 1, "complete overlap of lshVU"u8),
    new(new global::go.math.big_package.Word[]{1, _M, _M, _M, _M, _M, (global::go.math.big_package.Word)((nuint)3 << (int)(_W - 2)), 0, 0, 0, 0}.slice(), 7, 0, 3, 1, new global::go.math.big_package.Word[]{2, _M - 1, _M, _M, _M, _M, unchecked((nuint)(9223372036854775809UL))}.slice(), 1, "partial overlap by half of lshVU"u8),
    new(new global::go.math.big_package.Word[]{1, _M, _M, _M, _M, _M, (global::go.math.big_package.Word)((nuint)3 << (int)(_W - 2)), 0, 0, 0, 0, 0, 0, 0}.slice(), 7, 0, 6, 1, new global::go.math.big_package.Word[]{2, _M - 1, _M, _M, _M, _M, unchecked((nuint)(9223372036854775809UL))}.slice(), 1, "partial overlap by 1 Word of lshVU"u8),
    new(new global::go.math.big_package.Word[]{1, _M, _M, _M, _M, _M, (global::go.math.big_package.Word)((nuint)3 << (int)(_W - 2)), 0, 0, 0, 0, 0, 0, 0, 0}.slice(), 7, 0, 7, 1, new global::go.math.big_package.Word[]{2, _M - 1, _M, _M, _M, _M, unchecked((nuint)(9223372036854775809UL))}.slice(), 1, "no overlap of lshVU"u8),
    // additional test cases with shift values of 1 and (_W-1)
    new(arglshVUIn, 7, 0, 0, 1, arglshVUr1, 0, "complete overlap of lshVU and shift of 1"u8),
    new(arglshVUIn, 7, 0, 0, _W - 1, arglshVUrWm1, 32, "complete overlap of lshVU and shift of _W - 1"u8),
    new(arglshVUIn, 7, 0, 1, 1, arglshVUr1, 0, "partial overlap by 6 Words of lshVU and shift of 1"u8),
    new(arglshVUIn, 7, 0, 1, _W - 1, arglshVUrWm1, 32, "partial overlap by 6 Words of lshVU and shift of _W - 1"u8),
    new(arglshVUIn, 7, 0, 2, 1, arglshVUr1, 0, "partial overlap by 5 Words of lshVU and shift of 1"u8),
    new(arglshVUIn, 7, 0, 2, _W - 1, arglshVUrWm1, 32, "partial overlap by 5 Words of lshVU abd shift of _W - 1"u8),
    new(arglshVUIn, 7, 0, 3, 1, arglshVUr1, 0, "partial overlap by 4 Words of lshVU and shift of 1"u8),
    new(arglshVUIn, 7, 0, 3, _W - 1, arglshVUrWm1, 32, "partial overlap by 4 Words of lshVU and shift of _W - 1"u8)
}.slice();

internal static slice<global::go.math.big_package.Word> argrshVUIn = new global::go.math.big_package.Word[]{0, 0, 0, 1, 2, 4, 8, 16, 32, 64}.slice();

internal static slice<global::go.math.big_package.Word> argrshVUr0 = new global::go.math.big_package.Word[]{1, 2, 4, 8, 16, 32, 64}.slice();

internal static slice<global::go.math.big_package.Word> argrshVUr1 = new global::go.math.big_package.Word[]{0, 1, 2, 4, 8, 16, 32}.slice();

internal static slice<global::go.math.big_package.Word> argrshVUrWm1 = new global::go.math.big_package.Word[]{4, 8, 16, 32, 64, 128, 0}.slice();

internal static slice<argVU> argrshVU = new argVU[]{
    // test cases for rshVU
    new(new global::go.math.big_package.Word[]{0, 3, _M, _M, _M, _M, _M, (global::go.math.big_package.Word)((nuint)1 << (int)(_W - 1))}.slice(), 7, 1, 1, 1, new global::go.math.big_package.Word[]{unchecked((nuint)(9223372036854775809UL)), _M, _M, _M, _M, unchecked((nuint)(/* _M >> 1 */ 9223372036854775807UL)), (global::go.math.big_package.Word)((nuint)1 << (int)(_W - 2))}.slice(), (global::go.math.big_package.Word)((nuint)1 << (int)(_W - 1)), "complete overlap of rshVU"u8),
    new(new global::go.math.big_package.Word[]{0, 0, 0, 0, 3, _M, _M, _M, _M, _M, (global::go.math.big_package.Word)((nuint)1 << (int)(_W - 1))}.slice(), 7, 4, 1, 1, new global::go.math.big_package.Word[]{unchecked((nuint)(9223372036854775809UL)), _M, _M, _M, _M, unchecked((nuint)(/* _M >> 1 */ 9223372036854775807UL)), (global::go.math.big_package.Word)((nuint)1 << (int)(_W - 2))}.slice(), (global::go.math.big_package.Word)((nuint)1 << (int)(_W - 1)), "partial overlap by half of rshVU"u8),
    new(new global::go.math.big_package.Word[]{0, 0, 0, 0, 0, 0, 0, 3, _M, _M, _M, _M, _M, (global::go.math.big_package.Word)((nuint)1 << (int)(_W - 1))}.slice(), 7, 7, 1, 1, new global::go.math.big_package.Word[]{unchecked((nuint)(9223372036854775809UL)), _M, _M, _M, _M, unchecked((nuint)(/* _M >> 1 */ 9223372036854775807UL)), (global::go.math.big_package.Word)((nuint)1 << (int)(_W - 2))}.slice(), (global::go.math.big_package.Word)((nuint)1 << (int)(_W - 1)), "partial overlap by 1 Word of rshVU"u8),
    new(new global::go.math.big_package.Word[]{0, 0, 0, 0, 0, 0, 0, 0, 3, _M, _M, _M, _M, _M, (global::go.math.big_package.Word)((nuint)1 << (int)(_W - 1))}.slice(), 7, 8, 1, 1, new global::go.math.big_package.Word[]{unchecked((nuint)(9223372036854775809UL)), _M, _M, _M, _M, unchecked((nuint)(/* _M >> 1 */ 9223372036854775807UL)), (global::go.math.big_package.Word)((nuint)1 << (int)(_W - 2))}.slice(), (global::go.math.big_package.Word)((nuint)1 << (int)(_W - 1)), "no overlap of rshVU"u8),
    // additional test cases with shift values of 0, 1 and (_W-1)
    new(argrshVUIn, 7, 3, 3, 1, argrshVUr1, (global::go.math.big_package.Word)((nuint)1 << (int)(_W - 1)), "complete overlap of rshVU and shift of 1"u8),
    new(argrshVUIn, 7, 3, 3, _W - 1, argrshVUrWm1, 2, "complete overlap of rshVU and shift of _W - 1"u8),
    new(argrshVUIn, 7, 3, 2, 1, argrshVUr1, (global::go.math.big_package.Word)((nuint)1 << (int)(_W - 1)), "partial overlap by 6 Words of rshVU and shift of 1"u8),
    new(argrshVUIn, 7, 3, 2, _W - 1, argrshVUrWm1, 2, "partial overlap by 6 Words of rshVU and shift of _W - 1"u8),
    new(argrshVUIn, 7, 3, 1, 1, argrshVUr1, (global::go.math.big_package.Word)((nuint)1 << (int)(_W - 1)), "partial overlap by 5 Words of rshVU and shift of 1"u8),
    new(argrshVUIn, 7, 3, 1, _W - 1, argrshVUrWm1, 2, "partial overlap by 5 Words of rshVU and shift of _W - 1"u8),
    new(argrshVUIn, 7, 3, 0, 1, argrshVUr1, (global::go.math.big_package.Word)((nuint)1 << (int)(_W - 1)), "partial overlap by 4 Words of rshVU and shift of 1"u8),
    new(argrshVUIn, 7, 3, 0, _W - 1, argrshVUrWm1, 2, "partial overlap by 4 Words of rshVU and shift of _W - 1"u8)
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
            Ꮡt.Errorf("d := %v, %s (d[%d:%d], d[%d:%d], %d)\n\tgot z[%d] = %#x; want %#x"u8, a.d, a.m, a.zp, a.zp + a.l, a.xp, a.xp + a.l, a.s, i, zi, a.r[i]);
            break;
        }
    }
    if (c != a.c) {
        Ꮡt.Errorf("d := %v, %s (d[%d:%d], d[%d:%d], %d)\n\tgot c = %#x; want %#x"u8, a.d, a.m, a.zp, a.zp + a.l, a.xp, a.xp + a.l, a.s, c, a.c);
    }
}

public static void TestShiftOverlap(ж<testing.T> Ꮡt) {
    foreach (var (_, a) in arglshVU) {
        var arg = a;
        testShiftFunc(Ꮡt, lshVU, arg);
    }

    foreach (var (_, a) in argrshVU) {
        var arg = a;
        testShiftFunc(Ꮡt, rshVU, arg);
    }
}

public static void TestIssue31084(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var stk = getStack();
        var stkʗ1 = stk;
        defer(stkʗ1.free, ref ᒐ);

        // compute 10^n via 5^n << n.
        UntypedInt n = 165;
        var p = ((global::go.math.big_package.Δnat)default!).expNN(stk, new Δnat(new global::go.math.big_package.Word[]{5}.slice()), new Δnat(new global::go.math.big_package.Word[]{n}.slice()), default!, false);
        p = p.lsh(p, n);
        @string got = (@string)p.utoa(10);
        @string want = "1"u8 + strings.Repeat("0"u8, n);
        if (got != want) {
            Ꮡt.Errorf("lsh(%v, %v)\n\tgot  %s\n\twant %s"u8, p, (nint)(n), got, want);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static readonly @string issue42838Value = "159309191113245227702888039776771180559110455519261878607388585338616290151305816094308987472018268594098344692611135542392730712890625"u8;

public static void TestIssue42838(ж<testing.T> Ꮡt) {
    UntypedInt s = 192;
    var (z, _, _, _) = ((global::go.math.big_package.Δnat)default!).scan(new big_test_package.strings_ReaderжByteScanner(strings.NewReader(issue42838Value)), 0, false);
    z = z.lsh(z, s);
    @string got = (@string)z.utoa(10);
    @string want = "1"u8 + strings.Repeat("0"u8, s);
    if (got != want) {
        Ꮡt.Errorf("lsh(%v, %v)\n\tgot  %s\n\twant %s"u8, z, (nint)(s), got, want);
    }
}

// type funVWW is a methodless func type — rendered inline as its base delegate

internal partial struct argVWW {
    internal global::go.math.big_package.Δnat z, x;
    internal global::go.math.big_package.Word y, r;
    internal global::go.math.big_package.Word c;
}

internal static slice<argVWW> prodVWW = new argVWW[]{
    new(),
    new(new Δnat(new global::go.math.big_package.Word[]{0}.slice()), new Δnat(new global::go.math.big_package.Word[]{0}.slice()), 0, 0, 0),
    new(new Δnat(new global::go.math.big_package.Word[]{991}.slice()), new Δnat(new global::go.math.big_package.Word[]{0}.slice()), 0, 991, 0),
    new(new Δnat(new global::go.math.big_package.Word[]{0}.slice()), new Δnat(new global::go.math.big_package.Word[]{_M}.slice()), 0, 0, 0),
    new(new Δnat(new global::go.math.big_package.Word[]{991}.slice()), new Δnat(new global::go.math.big_package.Word[]{_M}.slice()), 0, 991, 0),
    new(new Δnat(new global::go.math.big_package.Word[]{0}.slice()), new Δnat(new global::go.math.big_package.Word[]{0}.slice()), _M, 0, 0),
    new(new Δnat(new global::go.math.big_package.Word[]{991}.slice()), new Δnat(new global::go.math.big_package.Word[]{0}.slice()), _M, 991, 0),
    new(new Δnat(new global::go.math.big_package.Word[]{1}.slice()), new Δnat(new global::go.math.big_package.Word[]{1}.slice()), 1, 0, 0),
    new(new Δnat(new global::go.math.big_package.Word[]{992}.slice()), new Δnat(new global::go.math.big_package.Word[]{1}.slice()), 1, 991, 0),
    new(new Δnat(new global::go.math.big_package.Word[]{22793}.slice()), new Δnat(new global::go.math.big_package.Word[]{991}.slice()), 23, 0, 0),
    new(new Δnat(new global::go.math.big_package.Word[]{22800}.slice()), new Δnat(new global::go.math.big_package.Word[]{991}.slice()), 23, 7, 0),
    new(new Δnat(new global::go.math.big_package.Word[]{0, 0, 0, 22793}.slice()), new Δnat(new global::go.math.big_package.Word[]{0, 0, 0, 991}.slice()), 23, 0, 0),
    new(new Δnat(new global::go.math.big_package.Word[]{7, 0, 0, 22793}.slice()), new Δnat(new global::go.math.big_package.Word[]{0, 0, 0, 991}.slice()), 23, 7, 0),
    new(new Δnat(new global::go.math.big_package.Word[]{0, 0, 0, 0}.slice()), new Δnat(new global::go.math.big_package.Word[]{7893475, 7395495, 798547395, 68943}.slice()), 0, 0, 0),
    new(new Δnat(new global::go.math.big_package.Word[]{991, 0, 0, 0}.slice()), new Δnat(new global::go.math.big_package.Word[]{7893475, 7395495, 798547395, 68943}.slice()), 0, 991, 0),
    new(new Δnat(new global::go.math.big_package.Word[]{0, 0, 0, 0}.slice()), new Δnat(new global::go.math.big_package.Word[]{0, 0, 0, 0}.slice()), 894375984, 0, 0),
    new(new Δnat(new global::go.math.big_package.Word[]{991, 0, 0, 0}.slice()), new Δnat(new global::go.math.big_package.Word[]{0, 0, 0, 0}.slice()), 894375984, 991, 0),
    new(new Δnat(new global::go.math.big_package.Word[]{unchecked((nuint)(18446744073709551614UL))}.slice()), new Δnat(new global::go.math.big_package.Word[]{_M}.slice()), (global::go.math.big_package.Word)((nuint)1 << 1), 0, (nuint)(/* _M >> (_W - 1) */ 1UL)),
    new(new Δnat(new global::go.math.big_package.Word[]{unchecked((nuint)(18446744073709551615UL))}.slice()), new Δnat(new global::go.math.big_package.Word[]{_M}.slice()), (global::go.math.big_package.Word)((nuint)1 << 1), 1, (nuint)(/* _M >> (_W - 1) */ 1UL)),
    new(new Δnat(new global::go.math.big_package.Word[]{unchecked((nuint)(18446744073709551488UL))}.slice()), new Δnat(new global::go.math.big_package.Word[]{_M}.slice()), (global::go.math.big_package.Word)((nuint)1 << 7), 0, (nuint)(/* _M >> (_W - 7) */ 127UL)),
    new(new Δnat(new global::go.math.big_package.Word[]{unchecked((nuint)(18446744073709551552UL))}.slice()), new Δnat(new global::go.math.big_package.Word[]{_M}.slice()), (global::go.math.big_package.Word)((nuint)1 << 7), (global::go.math.big_package.Word)((nuint)1 << 6), (nuint)(/* _M >> (_W - 7) */ 127UL)),
    new(new Δnat(new global::go.math.big_package.Word[]{unchecked((nuint)(18446744073709551488UL)), _M, _M, _M}.slice()), new Δnat(new global::go.math.big_package.Word[]{_M, _M, _M, _M}.slice()), (global::go.math.big_package.Word)((nuint)1 << 7), 0, (nuint)(/* _M >> (_W - 7) */ 127UL)),
    new(new Δnat(new global::go.math.big_package.Word[]{unchecked((nuint)(18446744073709551552UL)), _M, _M, _M}.slice()), new Δnat(new global::go.math.big_package.Word[]{_M, _M, _M, _M}.slice()), (global::go.math.big_package.Word)((nuint)1 << 7), (global::go.math.big_package.Word)((nuint)1 << 6), (nuint)(/* _M >> (_W - 7) */ 127UL))
}.slice();

internal static void testFunVWW(ж<testing.T> Ꮡt, @string msg, Func<slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, global::go.math.big_package.Word, global::go.math.big_package.Word, global::go.math.big_package.Word> f, argVWW a) {
    var z = new global::go.math.big_package.Δnat(len(a.z));
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
    internal global::go.math.big_package.Δnat z;
    internal global::go.math.big_package.Word xn;
    internal global::go.math.big_package.Δnat x;
    internal global::go.math.big_package.Word y;
    internal global::go.math.big_package.Word r;
}

internal static void testFunWVW(ж<testing.T> Ꮡt, @string msg, Func<slice<global::go.math.big_package.Word>, global::go.math.big_package.Word, slice<global::go.math.big_package.Word>, global::go.math.big_package.Word, global::go.math.big_package.Word> f, argWVW a) {
    var z = new global::go.math.big_package.Δnat(len(a.z));
    global::go.math.big_package.Word r = f(z, a.xn, a.x, a.y);
    if (!slices.Equal<global::go.math.big_package.Δnat, global::go.math.big_package.Word>(z, a.z) || r != a.r){
        Ꮡt.Errorf("%s%+v\nhave %v, %v\nwant %v, %v"u8, msg, a, z, r, a.z, a.r);
    } else {
        Ꮡt.Logf("%s%+v\ngood %v, %v"u8, msg, a, z, r);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string mulAddVWWGˢ = "mulAddVWW_g"u8;
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

partial struct mulWWTestsᴛ1 /*dyn*/ {
    internal global::go.math.big_package.Word x, y;
    internal global::go.math.big_package.Word q, r;
}
internal static slice<mulWWTestsᴛ1> mulWWTests = new mulWWTestsᴛ1[]{
    new(_M, _M, _M - 1, 1)
    // 32 bit only: {0xc47dfa8c, 50911, 0x98a4, 0x998587f4},
}.slice();

public static void TestMulWW(ж<testing.T> Ꮡt) {
    foreach (var (i, test) in mulWWTests) {
        var (q, r) = mulWW(test.x, test.y);
        if (q != test.q || r != test.r) {
            Ꮡt.Errorf("#%d got (%x, %x) want (%x, %x)"u8, i, q, r, test.q, test.r);
        }
    }
}

partial struct mulAddWWWTestsᴛ1 /*dyn*/ {
    internal global::go.math.big_package.Word x, y, c;
    internal global::go.math.big_package.Word q, r;
}
internal static slice<mulAddWWWTestsᴛ1> mulAddWWWTests = new mulAddWWWTestsᴛ1[]{
    // TODO(agl): These will only work on 64-bit platforms.
    // {15064310297182388543, 0xe7df04d2d35d5d80, 13537600649892366549, 13644450054494335067, 10832252001440893781},
    // {15064310297182388543, 0xdab2f18048baa68d, 13644450054494335067, 12869334219691522700, 14233854684711418382},
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
    new(unchecked((nuint)(/* _M >> 1 */ 9223372036854775807UL)), 0, _M, unchecked((nuint)(/* _M >> 1 */ 9223372036854775807UL)), unchecked((nuint)(/* _M >> 1 */ 9223372036854775807UL))),
    new(_M - (4611686018427387904L), _M, (global::go.math.big_package.Word)((nuint)3 << (int)(_W - 2)), _M, _M - (4611686018427387904L))
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

// benchSizes are the benchmark word sizes.
internal static slice<nint> benchSizes = new nint[]{1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 16, 32, 64, 100, 1000, 10_000, 100_000}.slice();

// A benchFunc is a function to be benchmarked.
// It takes one output buffer and two input buffers,
// but it does not have to use any of them.
// type benchFunc is a methodless func type — rendered inline as its base delegate

// bench runs benchmarks of fn for a variety of word sizes.
// It adds the given suffix (for example "/impl=go") to the benchmark names it creates,
// after a "/words=N" parameter. Putting words first makes it easier to run
// all benchmarks with a specific word size
// (go test -run=NONE '-bench=V/words=100$')
// even if different benchmarks have different numbers of other parameters.
internal static void bench(ж<testing.B> Ꮡb, @string suffix, Action<slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>> fn) {
    foreach (var (_, n) in benchSizes) {
        if (isRaceBuilder && n > 1000) {
            continue;
        }
        ref var z = ref heap<slice<global::go.math.big_package.Word>>(out var Ꮡz);
        ref var x = ref heap<slice<global::go.math.big_package.Word>>(out var Ꮡx);
        ref var y = ref heap<slice<global::go.math.big_package.Word>>(out var Ꮡy);
        Ꮡb.Run(fmt.Sprintf("words=%d%s"u8, n, suffix), (ж<testing.B> bΔ1) => {
            if (Ꮡz.ValueSlot == default!) {
                Ꮡz.ValueSlot = new slice<global::go.math.big_package.Word>(n);
                Ꮡx.ValueSlot = rndV(n);
                Ꮡy.ValueSlot = rndV(n);
            }
            bΔ1.SetBytes((int64)(n * (nint)_S));
            while (bΔ1.Loop()) {
                fn(Ꮡz.ValueSlot, Ꮡx.ValueSlot, Ꮡy.ValueSlot);
            }
        });
    }
}

// Benchmark basic I/O and arithmetic processing speed,
// to help estimate the upper bounds on other operations.
public static void BenchmarkCopyVV(ж<testing.B> Ꮡb) {
    bench(Ꮡb, ""u8, benchVV(copyVV));
}

internal static global::go.math.big_package.Word copyVV(slice<global::go.math.big_package.Word> z, slice<global::go.math.big_package.Word> x, slice<global::go.math.big_package.Word> y) {
    copy(z, x);
    return 0;
}

// Note: This benchmark consistently runs faster (even up to 2X faster on MB/s)
// with words=10 and words=100 than larger amounts like words=1000 or words=10000.
// The reason appears to that if you run 100-word addition loops repeatedly,
// they are independent calculations, and the processor speculates/pipelines/whatever
// to such a deep level that it can overlap the repeated loops.
// In contrast, if you run 1000-word or 10000-word loops repeatedly,
// the dependency chains are so long that the processor cannot overlap them.
// If we change arithVV to take the starting value of s and pass in the result
// from the previous arithVV, then even the 10-word or 100-loops become
// a single long dependency chain and the 2X disappears. But since we are
// using BenchmarkArithVV for a given word size to estimate the upper bound
// of, say, BenchmarkAddVV for that same word size, we actually want the
// dependency chain-length variation in BenchmarkArithVV too.
// It's just mysterious to see until you understand what is causing it.
public static void BenchmarkArithVV(ж<testing.B> Ꮡb) {
    bench(Ꮡb, ""u8, benchVV(arithVV));
}

internal static global::go.math.big_package.Word arithVV(slice<global::go.math.big_package.Word> z, slice<global::go.math.big_package.Word> x, slice<global::go.math.big_package.Word> y) {
    global::go.math.big_package.Word a = default!;
    global::go.math.big_package.Word b = default!;
    global::go.math.big_package.Word c = default!;
    global::go.math.big_package.Word d = default!;
    global::go.math.big_package.Word e = default!;
    global::go.math.big_package.Word f = default!;
    global::go.math.big_package.Word g = default!;
    global::go.math.big_package.Word h = default!;
    global::go.math.big_package.Word i = default!;
    global::go.math.big_package.Word j = default!;
    if (len(z) >= 8) {
        (a, b, c, d, e, f, g, h, i, j) = (1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
    }
    if (len(z) < 10) {
        // We don't really care about the speed here, but
        // do something so that the small word counts aren't all the same.
        global::go.math.big_package.Word sΔ1 = (global::go.math.big_package.Word)0;
        foreach (var (_, zi) in z) {
            sΔ1 += zi;
        }
        return sΔ1;
    }
    global::go.math.big_package.Word s = (global::go.math.big_package.Word)0;
    foreach (var _ᴛ1 in range(len(z) / 10)) {
        s += a;
        s += b;
        s += c;
        s += d;
        s += e;
        s += f;
        s += g;
        s += h;
        s += i;
        s += j;
    }
    return s;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string implAsmˢ = "/impl=asm"u8;
internal static readonly @string implGoˢ = "/impl=go"u8;

public static void BenchmarkAddVV(ж<testing.B> Ꮡb) {
    bench(Ꮡb, implAsmˢ, benchVV(addVV));
    bench(Ꮡb, implGoˢ, benchVV(addVV_g));
}

public static void BenchmarkSubVV(ж<testing.B> Ꮡb) {
    bench(Ꮡb, implAsmˢ, benchVV(subVV));
    bench(Ꮡb, implGoˢ, benchVV(subVV_g));
}

internal static Action<slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>> benchVV(Func<slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, global::go.math.big_package.Word> fn) {
    return (slice<global::go.math.big_package.Word> z, slice<global::go.math.big_package.Word> x, slice<global::go.math.big_package.Word> y) => {
        fn(z, x, y);
    };
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string dataRandomˢ = "/data=random"u8;
internal static readonly @string dataCarryˢ = "/data=carry"u8;
internal static readonly @string dataShortcutˢ = "/data=shortcut"u8;

public static void BenchmarkAddVW(ж<testing.B> Ꮡb) {
    bench(Ꮡb, dataRandomˢ, benchVW(addVW, 123));
    bench(Ꮡb, dataCarryˢ, benchCarryVW(addVW, ~((global::go.math.big_package.Word)((global::go.math.big_package.Word)0)), 1));
    bench(Ꮡb, dataShortcutˢ, benchShortVW(addVW, 123));
}

public static void BenchmarkSubVW(ж<testing.B> Ꮡb) {
    bench(Ꮡb, dataRandomˢ, benchVW(subVW, 123));
    bench(Ꮡb, dataCarryˢ, benchCarryVW(subVW, 0, 1));
    bench(Ꮡb, dataShortcutˢ, benchShortVW(subVW, 123));
}

internal static Action<slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>> benchVW(Func<slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, global::go.math.big_package.Word, global::go.math.big_package.Word> fn, global::go.math.big_package.Word w) {
    return (slice<global::go.math.big_package.Word> z, slice<global::go.math.big_package.Word> x, slice<global::go.math.big_package.Word> y) => {
        fn(z, x, w);
    };
}

internal static Action<slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>> benchCarryVW(Func<slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, global::go.math.big_package.Word, global::go.math.big_package.Word> fn, global::go.math.big_package.Word xi, global::go.math.big_package.Word w) {
    return (slice<global::go.math.big_package.Word> z, slice<global::go.math.big_package.Word> x, slice<global::go.math.big_package.Word> y) => {
        // Fill x with xi the first time we are called with a given x.
        // Otherwise x is random, so checking the first two elements is good enough.
        // Assume this is the warmup, so we don't need to worry about it taking longer.
        if (x[0] != w || len(x) >= 2 && x[1] != w) {
            foreach (var i in range(x)) {
                x[i] = xi;
            }
        }
        fn(z, x, w);
    };
}

internal static Action<slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>> benchShortVW(Func<slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, global::go.math.big_package.Word, global::go.math.big_package.Word> fn, global::go.math.big_package.Word w) {
    // Note: calling fn with x not z, to benchmark in-place overwriting.
    return (slice<global::go.math.big_package.Word> z, slice<global::go.math.big_package.Word> x, slice<global::go.math.big_package.Word> y) => {
        fn(x, x, w);
    };
}

public static void BenchmarkLshVU(ж<testing.B> Ꮡb) {
    bench(Ꮡb, implAsmˢ, benchVU(lshVU, 3));
    bench(Ꮡb, implGoˢ, benchVU(lshVU_g, 3));
}

public static void BenchmarkRshVU(ж<testing.B> Ꮡb) {
    bench(Ꮡb, implAsmˢ, benchVU(rshVU, 3));
    bench(Ꮡb, implGoˢ, benchVU(rshVU_g, 3));
}

internal static Action<slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>> benchVU(Func<slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, nuint, global::go.math.big_package.Word> fn, nuint s) {
    return (slice<global::go.math.big_package.Word> z, slice<global::go.math.big_package.Word> x, slice<global::go.math.big_package.Word> y) => {
        fn(z, x, s);
    };
}

public static void BenchmarkMulAddVWW(ж<testing.B> Ꮡb) {
    bench(Ꮡb, implAsmˢ, benchVWW(mulAddVWW, 42, 100));
    bench(Ꮡb, implGoˢ, benchVWW(mulAddVWW_g, 42, 100));
}

internal static Action<slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>> benchVWW(Func<slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, global::go.math.big_package.Word, global::go.math.big_package.Word, global::go.math.big_package.Word> fn, global::go.math.big_package.Word w1, global::go.math.big_package.Word w2) {
    return (slice<global::go.math.big_package.Word> z, slice<global::go.math.big_package.Word> x, slice<global::go.math.big_package.Word> y) => {
        fn(z, x, w1, w2);
    };
}

public static void BenchmarkAddMulVVWW(ж<testing.B> Ꮡb) {
    bench(Ꮡb, implAsmˢ, benchVVWW(addMulVVWW, 42, 100));
    bench(Ꮡb, implGoˢ, benchVVWW(addMulVVWW_g, 42, 100));
}

internal static Action<slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>> benchVVWW(Func<slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, global::go.math.big_package.Word, global::go.math.big_package.Word, global::go.math.big_package.Word> fn, global::go.math.big_package.Word w1, global::go.math.big_package.Word w2) {
    return (slice<global::go.math.big_package.Word> z, slice<global::go.math.big_package.Word> x, slice<global::go.math.big_package.Word> y) => {
        fn(z, x, y, w1, w2);
    };
}

public static void BenchmarkDivWVW(ж<testing.B> Ꮡb) {
    bench(Ꮡb, ""u8, benchWVW(divWVW, 100, 200));
}

internal static Action<slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>, slice<global::go.math.big_package.Word>> benchWVW(Func<slice<global::go.math.big_package.Word>, global::go.math.big_package.Word, slice<global::go.math.big_package.Word>, global::go.math.big_package.Word, global::go.math.big_package.Word> fn, global::go.math.big_package.Word w1, global::go.math.big_package.Word w2) {
    return (slice<global::go.math.big_package.Word> z, slice<global::go.math.big_package.Word> x, slice<global::go.math.big_package.Word> y) => {
        fn(z, w1, x, w2);
    };
}

} // end big_internal_test_package
