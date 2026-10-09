// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.math;

using fmt = fmt_package;
using math = math_package;
using runtime = runtime_package;
using strings = strings_package;
using testing = testing_package;
using io = io_package;
using static go.math.big_package;

partial class big_internal_test_package {


partial struct cmpTestsᴛ1 /*dyn*/ {
    internal global::go.math.big_package.nat x, y;
    internal nint r;
}
internal static slice<cmpTestsᴛ1> cmpTests = new cmpTestsᴛ1[]{
    new(default!, default!, 0),
    new(default!, ((global::go.math.big_package.nat)default!), 0),
    new(((global::go.math.big_package.nat)default!), default!, 0),
    new(((global::go.math.big_package.nat)default!), ((global::go.math.big_package.nat)default!), 0),
    new(new nat(new global::go.math.big_package.Word[]{0}.slice()), new nat(new global::go.math.big_package.Word[]{0}.slice()), 0),
    new(new nat(new global::go.math.big_package.Word[]{0}.slice()), new nat(new global::go.math.big_package.Word[]{1}.slice()), -1),
    new(new nat(new global::go.math.big_package.Word[]{1}.slice()), new nat(new global::go.math.big_package.Word[]{0}.slice()), 1),
    new(new nat(new global::go.math.big_package.Word[]{1}.slice()), new nat(new global::go.math.big_package.Word[]{1}.slice()), 0),
    new(new nat(new global::go.math.big_package.Word[]{0, _M}.slice()), new nat(new global::go.math.big_package.Word[]{1}.slice()), 1),
    new(new nat(new global::go.math.big_package.Word[]{1}.slice()), new nat(new global::go.math.big_package.Word[]{0, _M}.slice()), -1),
    new(new nat(new global::go.math.big_package.Word[]{1, _M}.slice()), new nat(new global::go.math.big_package.Word[]{0, _M}.slice()), 1),
    new(new nat(new global::go.math.big_package.Word[]{0, _M}.slice()), new nat(new global::go.math.big_package.Word[]{1, _M}.slice()), -1),
    new(new nat(new global::go.math.big_package.Word[]{16, 571956, 8794, 68}.slice()), new nat(new global::go.math.big_package.Word[]{837, 9146, 1, 754489}.slice()), -1),
    new(new nat(new global::go.math.big_package.Word[]{34986, 41, 105, 1957}.slice()), new nat(new global::go.math.big_package.Word[]{56, 7458, 104, 1957}.slice()), 1)
}.slice();

public static void TestCmp(ж<testing.T> Ꮡt) {
    foreach (var (i, a) in cmpTests) {
        nint r = a.x.cmp(a.y);
        if (r != a.r) {
            Ꮡt.Errorf("#%d got r = %v; want %v"u8, i, r, a.r);
        }
    }
}

// type funNN is a methodless func type — rendered inline as its base delegate

internal partial struct argNN {
    internal global::go.math.big_package.nat z, x, y;
}

internal static slice<argNN> sumNN = new argNN[]{
    new(),
    new(new nat(new global::go.math.big_package.Word[]{1}.slice()), default!, new nat(new global::go.math.big_package.Word[]{1}.slice())),
    new(new nat(new global::go.math.big_package.Word[]{1111111110}.slice()), new nat(new global::go.math.big_package.Word[]{123456789}.slice()), new nat(new global::go.math.big_package.Word[]{987654321}.slice())),
    new(new nat(new global::go.math.big_package.Word[]{0, 0, 0, 1}.slice()), default!, new nat(new global::go.math.big_package.Word[]{0, 0, 0, 1}.slice())),
    new(new nat(new global::go.math.big_package.Word[]{0, 0, 0, 1111111110}.slice()), new nat(new global::go.math.big_package.Word[]{0, 0, 0, 123456789}.slice()), new nat(new global::go.math.big_package.Word[]{0, 0, 0, 987654321}.slice())),
    new(new nat(new global::go.math.big_package.Word[]{0, 0, 0, 1}.slice()), new nat(new global::go.math.big_package.Word[]{0, 0, _M}.slice()), new nat(new global::go.math.big_package.Word[]{0, 0, 1}.slice()))
}.slice();

// 3^100 * 3^28 = 3^128
// z = 111....1 (70000 digits)
// x = 10^(99*700) + ... + 10^1400 + 10^700 + 1
// y = 111....1 (700 digits, larger than Karatsuba threshold on 32-bit and 64-bit)
// z = 111....1 (20000 digits)
// x = 10^10000 + 1
// y = 111....1 (10000 digits)
internal static slice<argNN> prodNN;
internal static void initᴛprodNN() { prodNN = new argNN[]{
    new(),
    new(default!, default!, default!),
    new(default!, new nat(new global::go.math.big_package.Word[]{991}.slice()), default!),
    new(new nat(new global::go.math.big_package.Word[]{991}.slice()), new nat(new global::go.math.big_package.Word[]{991}.slice()), new nat(new global::go.math.big_package.Word[]{1}.slice())),
    new(new nat(new global::go.math.big_package.Word[]{991 * 991}.slice()), new nat(new global::go.math.big_package.Word[]{991}.slice()), new nat(new global::go.math.big_package.Word[]{991}.slice())),
    new(new nat(new global::go.math.big_package.Word[]{0, 0, 991 * 991}.slice()), new nat(new global::go.math.big_package.Word[]{0, 991}.slice()), new nat(new global::go.math.big_package.Word[]{0, 991}.slice())),
    new(new nat(new global::go.math.big_package.Word[]{1 * 991, 2 * 991, 3 * 991, 4 * 991}.slice()), new nat(new global::go.math.big_package.Word[]{1, 2, 3, 4}.slice()), new nat(new global::go.math.big_package.Word[]{991}.slice())),
    new(new nat(new global::go.math.big_package.Word[]{4, 11, 20, 30, 20, 11, 4}.slice()), new nat(new global::go.math.big_package.Word[]{1, 2, 3, 4}.slice()), new nat(new global::go.math.big_package.Word[]{4, 3, 2, 1}.slice())),
    new(
        natFromString("11790184577738583171520872861412518665678211592275841109096961"u8),
        natFromString("515377520732011331036461129765621272702107522001"u8),
        natFromString("22876792454961"u8)
    ),
    new(
        natFromString(strings.Repeat("1"u8, 70000)),
        natFromString("1"u8 + strings.Repeat(strings.Repeat("0"u8, 699) + "1"u8, 99)),
        natFromString(strings.Repeat("1"u8, 700))
    ),
    new(
        natFromString(strings.Repeat("1"u8, 20000)),
        natFromString("1"u8 + strings.Repeat("0"u8, 9999) + "1"u8),
        natFromString(strings.Repeat("1"u8, 10000))
    )
}.slice(); }

internal static global::go.math.big_package.nat natFromString(@string s) {
    var (x, _, _, err) = ((global::go.math.big_package.nat)default!).scan(new big_test_package.strings_ReaderжByteScanner(strings.NewReader(s)), 0, false);
    if (err != default!) {
        throw panic(err);
    }
    return x;
}

public static void TestSet(ж<testing.T> Ꮡt) {
    foreach (var (_, a) in sumNN) {
        var z = ((global::go.math.big_package.nat)default!).set(a.z);
        if (z.cmp(a.z) != 0) {
            Ꮡt.Errorf("got z = %v; want %v"u8, z, a.z);
        }
    }
}

internal static void testFunNN(ж<testing.T> Ꮡt, @string msg, Func<global::go.math.big_package.nat, global::go.math.big_package.nat, global::go.math.big_package.nat, global::go.math.big_package.nat> f, argNN a) {
    var z = f(default!, a.x, a.y);
    if (z.cmp(a.z) != 0) {
        Ꮡt.Errorf("%s%+v\n\tgot z = %v; want %v"u8, msg, a, z, a.z);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string addˢ = "add"u8;
internal static readonly @string addSymmetricˢ = "add symmetric"u8;
internal static readonly @string subˢ = "sub"u8;
internal static readonly @string subSymmetricˢ = "sub symmetric"u8;
internal static readonly @string mulˢ = "mul"u8;
internal static readonly @string mulSymmetricˢ = "mul symmetric"u8;

public static void TestFunNN(ж<testing.T> Ꮡt) {
    foreach (var (_, a) in sumNN) {
        var arg = a;
        testFunNN(Ꮡt, addˢ, new Func<global::go.math.big_package.nat, global::go.math.big_package.nat, global::go.math.big_package.nat, global::go.math.big_package.nat>(((Func<global::go.math.big_package.nat, global::go.math.big_package.nat, global::go.math.big_package.nat, global::go.math.big_package.nat>)(global::go.math.big_package.add))), arg);
        arg = new argNN(a.z, a.y, a.x);
        testFunNN(Ꮡt, addSymmetricˢ, new Func<global::go.math.big_package.nat, global::go.math.big_package.nat, global::go.math.big_package.nat, global::go.math.big_package.nat>(((Func<global::go.math.big_package.nat, global::go.math.big_package.nat, global::go.math.big_package.nat, global::go.math.big_package.nat>)(global::go.math.big_package.add))), arg);
        arg = new argNN(a.x, a.z, a.y);
        testFunNN(Ꮡt, subˢ, new Func<global::go.math.big_package.nat, global::go.math.big_package.nat, global::go.math.big_package.nat, global::go.math.big_package.nat>(((Func<global::go.math.big_package.nat, global::go.math.big_package.nat, global::go.math.big_package.nat, global::go.math.big_package.nat>)(global::go.math.big_package.sub))), arg);
        arg = new argNN(a.y, a.z, a.x);
        testFunNN(Ꮡt, subSymmetricˢ, new Func<global::go.math.big_package.nat, global::go.math.big_package.nat, global::go.math.big_package.nat, global::go.math.big_package.nat>(((Func<global::go.math.big_package.nat, global::go.math.big_package.nat, global::go.math.big_package.nat, global::go.math.big_package.nat>)(global::go.math.big_package.sub))), arg);
    }
    foreach (var (_, a) in prodNN) {
        var arg = a;
        testFunNN(Ꮡt, mulˢ, new Func<global::go.math.big_package.nat, global::go.math.big_package.nat, global::go.math.big_package.nat, global::go.math.big_package.nat>(((Func<global::go.math.big_package.nat, global::go.math.big_package.nat, global::go.math.big_package.nat, global::go.math.big_package.nat>)(global::go.math.big_package.mul))), arg);
        arg = new argNN(a.z, a.y, a.x);
        testFunNN(Ꮡt, mulSymmetricˢ, new Func<global::go.math.big_package.nat, global::go.math.big_package.nat, global::go.math.big_package.nat, global::go.math.big_package.nat>(((Func<global::go.math.big_package.nat, global::go.math.big_package.nat, global::go.math.big_package.nat, global::go.math.big_package.nat>)(global::go.math.big_package.mul))), arg);
    }
}

// empty range
// empty range
// 10!
// 20!
// 100!

partial struct mulRangesNᴛ1 /*dyn*/ {
    internal uint64 a, b;
    internal @string prod;
}
internal static slice<mulRangesNᴛ1> mulRangesN = new mulRangesNᴛ1[]{
    new(0, 0, "0"u8),
    new(1, 1, "1"u8),
    new(1, 2, "2"u8),
    new(1, 3, "6"u8),
    new(10, 10, "10"u8),
    new(0, 100, "0"u8),
    new(0, 1000000000, "0"u8),
    new(1, 0, "1"u8),
    new(100, 1, "1"u8),
    new(1, 10, "3628800"u8),
    new(1, 20, "2432902008176640000"u8),
    new(1, 100,
        "933262154439441526816992388562667004907159682643816214685929"u8 + "638952175999932299156089414639761565182862536979208272237582"u8 + "51185210916864000000000000000000000000"u8
    ),
    new(math.MaxUint64 - 0, math.MaxUint64, "18446744073709551615"u8),
    new(math.MaxUint64 - 1, math.MaxUint64, "340282366920938463408034375210639556610"u8),
    new(math.MaxUint64 - 2, math.MaxUint64, "6277101735386680761794095221682035635525021984684230311930"u8),
    new(math.MaxUint64 - 3, math.MaxUint64, "115792089237316195360799967654821100226821973275796746098729803619699194331160"u8)
}.slice();

public static void TestMulRangeN(ж<testing.T> Ꮡt) {
    foreach (var (i, r) in mulRangesN) {
        @string prod = ((@string)((global::go.math.big_package.nat)default!).mulRange(r.a, r.b).utoa(10));
        if (prod != r.prod) {
            Ꮡt.Errorf("#%d: got %s; want %s"u8, i, prod, r.prod);
        }
    }
}

// allocBytes returns the number of bytes allocated by invoking f.
internal static uint64 allocBytes(Action f) {
    ref var stats = ref heap(new runtime.MemStats(), out var Ꮡstats);
    runtime.ReadMemStats(Ꮡstats);
    var t = stats.TotalAlloc;
    f();
    runtime.ReadMemStats(Ꮡstats);
    return stats.TotalAlloc - t;
}

// TestMulUnbalanced tests that multiplying numbers of different lengths
// does not cause deep recursion and in turn allocate too much memory.
// Test case for issue 3807.
public static void TestMulUnbalanced(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        defer(runtime.GOMAXPROCS, runtime.GOMAXPROCS(1), ref ᒐ);
        var x = rndNat(50000);
        var y = rndNat(40);
        var xʗ1 = x;
        var yʗ1 = y;
        var allocSize = allocBytes(() => {
            ((global::go.math.big_package.nat)default!).mul(xʗ1, yʗ1);
        });
        var inputSize = (uint64)(len(x) + len(y)) * (uint64)_S;
        {
            var ratio = allocSize / (uint64)inputSize; if (ratio > 10) {
                Ꮡt.Errorf("multiplication uses too much memory (%d > %d times the size of inputs)"u8, allocSize, ratio);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// rndNat returns a random nat value >= 0 of (usually) n words in length.
// In extremely unlikely cases it may be smaller than n words if the top-
// most words are 0.
internal static global::go.math.big_package.nat rndNat(nint n) {
    return ((global::go.math.big_package.nat)rndV(n)).norm();
}

// rndNat1 is like rndNat but the result is guaranteed to be > 0.
internal static global::go.math.big_package.nat rndNat1(nint n) {
    var x = ((global::go.math.big_package.nat)rndV(n)).norm();
    if (len(x) == 0) {
        x.setWord(1);
    }
    return x;
}

public static void BenchmarkMul(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var mulx = rndNat(10000);
    var muly = rndNat(10000);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        global::go.math.big_package.nat z = default!;
        z.mul(mulx, muly);
    }
}

internal static void benchmarkNatMul(ж<testing.B> Ꮡb, nint nwords) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = rndNat(nwords);
    var y = rndNat(nwords);
    global::go.math.big_package.nat z = default!;
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        z.mul(x, y);
    }
}

internal static slice<nint> mulBenchSizes = new nint[]{10, 100, 1000, 10000, 100000}.slice();

public static void BenchmarkNatMul(ж<testing.B> Ꮡb) {
    foreach (var (_, n) in mulBenchSizes) {
        if (isRaceBuilder && n > 1000) {
            continue;
        }
        Ꮡb.Run(fmt.Sprintf("%d"u8, n), (ж<testing.B> bΔ1) => {
            benchmarkNatMul(bΔ1, n);
        });
    }
}

public static void TestNLZ(ж<testing.T> Ꮡt) {
    global::go.math.big_package.Word x = unchecked((nuint)(9223372036854775808UL));
    for (nint i = 0; i <= _W; i++) {
        if ((nint)nlz(x) != i) {
            Ꮡt.Errorf("failed at %x: got %d want %d"u8, x, nlz(x), i);
        }
        x >>= (int)(1);
    }
}

internal partial struct shiftTest {
    internal global::go.math.big_package.nat @in;
    internal nuint shift;
    internal global::go.math.big_package.nat @out;
}

internal static slice<shiftTest> leftShiftTests;
internal static void initᴛleftShiftTests() { leftShiftTests = new shiftTest[]{
    new(default!, 0, default!),
    new(default!, 1, default!),
    new(natOne, 0, natOne),
    new(natOne, 1, natTwo),
    new(new nat(new global::go.math.big_package.Word[]{(global::go.math.big_package.Word)((nuint)1 << (int)((_W - 1)))}.slice()), 1, new nat(new global::go.math.big_package.Word[]{0}.slice())),
    new(new nat(new global::go.math.big_package.Word[]{(global::go.math.big_package.Word)((nuint)1 << (int)((_W - 1))), 0}.slice()), 1, new nat(new global::go.math.big_package.Word[]{0, 1}.slice()))
}.slice(); }

public static void TestShiftLeft(ж<testing.T> Ꮡt) {
    foreach (var (i, test) in leftShiftTests) {
        global::go.math.big_package.nat z = default!;
        z = z.shl(test.@in, test.shift);
        foreach (var (j, d) in test.@out) {
            if (j >= len(z) || z[j] != d) {
                Ꮡt.Errorf("#%d: got: %v want: %v"u8, i, z, test.@out);
                break;
            }
        }
    }
}

internal static slice<shiftTest> rightShiftTests;
internal static void initᴛrightShiftTests() { rightShiftTests = new shiftTest[]{
    new(default!, 0, default!),
    new(default!, 1, default!),
    new(natOne, 0, natOne),
    new(natOne, 1, default!),
    new(natTwo, 1, natOne),
    new(new nat(new global::go.math.big_package.Word[]{0, 1}.slice()), 1, new nat(new global::go.math.big_package.Word[]{(global::go.math.big_package.Word)((nuint)1 << (int)((_W - 1)))}.slice())),
    new(new nat(new global::go.math.big_package.Word[]{2, 1, 1}.slice()), 1, new nat(new global::go.math.big_package.Word[]{unchecked((nuint)(9223372036854775809UL)), (global::go.math.big_package.Word)((nuint)1 << (int)((_W - 1)))}.slice()))
}.slice(); }

public static void TestShiftRight(ж<testing.T> Ꮡt) {
    foreach (var (i, test) in rightShiftTests) {
        global::go.math.big_package.nat z = default!;
        z = z.shr(test.@in, test.shift);
        foreach (var (j, d) in test.@out) {
            if (j >= len(z) || z[j] != d) {
                Ꮡt.Errorf("#%d: got: %v want: %v"u8, i, z, test.@out);
                break;
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string shlˢ = "Shl"u8;
internal static readonly @string shlSameˢ = "ShlSame"u8;
internal static readonly @string shrˢ = "Shr"u8;
internal static readonly @string shrSameˢ = "ShrSame"u8;

public static void BenchmarkZeroShifts(ж<testing.B> Ꮡb) {
    var x = rndNat(800);
    var xʗ1 = x;
    Ꮡb.Run(shlˢ, (ж<testing.B> bΔ1) => {
        for (nint i = 0; i < (~bΔ1).N; i++) {
            global::go.math.big_package.nat z = default!;
            z.shl(xʗ1, 0);
        }
    });
    var xʗ2 = x;
    Ꮡb.Run(shlSameˢ, (ж<testing.B> bΔ2) => {
        for (nint i = 0; i < (~bΔ2).N; i++) {
            xʗ2.shl(xʗ2, 0);
        }
    });
    var xʗ3 = x;
    Ꮡb.Run(shrˢ, (ж<testing.B> bΔ3) => {
        for (nint i = 0; i < (~bΔ3).N; i++) {
            global::go.math.big_package.nat z = default!;
            z.shr(xʗ3, 0);
        }
    });
    var xʗ4 = x;
    Ꮡb.Run(shrSameˢ, (ж<testing.B> bΔ4) => {
        for (nint i = 0; i < (~bΔ4).N; i++) {
            xʗ4.shr(xʗ4, 0);
        }
    });
}

internal partial struct modWTest {
    internal @string @in;
    internal @string dividend;
    internal @string @out;
}

internal static slice<modWTest> modWTests32 = new modWTest[]{
    new("23492635982634928349238759823742"u8, "252341"u8, "220170"u8)
}.slice();

internal static slice<modWTest> modWTests64 = new modWTest[]{
    new("6527895462947293856291561095690465243862946"u8, "524326975699234"u8, "375066989628668"u8)
}.slice();

internal static void runModWTests(ж<testing.T> Ꮡt, slice<modWTest> tests) {
    foreach (var (i, test) in tests) {
        var (@in, _) = @new<global::go.math.big_package.ΔInt>().SetString(test.@in, 10);
        var (d, _) = @new<global::go.math.big_package.ΔInt>().SetString(test.dividend, 10);
        var (@out, _) = @new<global::go.math.big_package.ΔInt>().SetString(test.@out, 10);
        global::go.math.big_package.Word r = (~@in).abs.modW((~d).abs[0]);
        if (r != (~@out).abs[0]) {
            Ꮡt.Errorf("#%d failed: got %d want %s"u8, i, r, @out.OrTypedNil());
        }
    }
}

public static void TestModW(ж<testing.T> Ꮡt) {
    if (_W >= 32) {
        runModWTests(Ꮡt, modWTests32);
    }
    if (_W >= 64) {
        runModWTests(Ꮡt, modWTests64);
    }
}


partial struct montgomeryTestsᴛ1 /*dyn*/ {
    internal @string x, y, m;
    internal uint64 k0;
    internal @string out32, out64;
}
internal static slice<montgomeryTestsᴛ1> montgomeryTests = new montgomeryTestsᴛ1[]{
    new(
        "0xffffffffffffffffffffffffffffffffffffffffffffffffe"u8,
        "0xffffffffffffffffffffffffffffffffffffffffffffffffe"u8,
        "0xfffffffffffffffffffffffffffffffffffffffffffffffff"u8,
        1,
        "0x1000000000000000000000000000000000000000000"u8,
        "0x10000000000000000000000000000000000"u8
    ),
    new(
        "0x000000000ffffff5"u8,
        "0x000000000ffffff0"u8,
        "0x0000000010000001"u8,
        0xff0000000fffffffUL,
        "0x000000000bfffff4"u8,
        "0x0000000003400001"u8
    ),
    new(
        "0x0000000080000000"u8,
        "0x00000000ffffffff"u8,
        "0x1000000000000001"u8,
        0xfffffffffffffffUL,
        "0x0800000008000001"u8,
        "0x0800000008000001"u8
    ),
    new(
        "0x0000000080000000"u8,
        "0x0000000080000000"u8,
        "0xffffffff00000001"u8,
        0xfffffffeffffffffUL,
        "0xbfffffff40000001"u8,
        "0xbfffffff40000001"u8
    ),
    new(
        "0x0000000080000000"u8,
        "0x0000000080000000"u8,
        "0x00ffffff00000001"u8,
        0xfffffeffffffffUL,
        "0xbfffff40000001"u8,
        "0xbfffff40000001"u8
    ),
    new(
        "0x0000000080000000"u8,
        "0x0000000080000000"u8,
        "0x0000ffff00000001"u8,
        0xfffeffffffffUL,
        "0xbfff40000001"u8,
        "0xbfff40000001"u8
    ),
    new(
        "0x3321ffffffffffffffffffffffffffff00000000000022222623333333332bbbb888c0"u8,
        "0x3321ffffffffffffffffffffffffffff00000000000022222623333333332bbbb888c0"u8,
        "0x33377fffffffffffffffffffffffffffffffffffffffffffff0000000000022222eee1"u8,
        0xdecc8f1249812adfUL,
        "0x04eb0e11d72329dc0915f86784820fc403275bf2f6620a20e0dd344c5cd0875e50deb5"u8,
        "0x0d7144739a7d8e11d72329dc0915f86784820fc403275bf2f61ed96f35dd34dbb3d6a0"u8
    ),
    new(
        "0x10000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000ffffffffffffffffffffffffffffffff00000000000022222223333333333444444444"u8,
        "0x10000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000ffffffffffffffffffffffffffffffff999999999999999aaabbbbbbbbcccccccccccc"u8,
        "0xffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff33377fffffffffffffffffffffffffffffffffffffffffffff0000000000022222eee1"u8,
        0xdecc8f1249812adfUL,
        "0x5c0d52f451aec609b15da8e5e5626c4eaa88723bdeac9d25ca9b961269400410ca208a16af9c2fb07d7a11c7772cba02c22f9711078d51a3797eb18e691295293284d988e349fa6deba46b25a4ecd9f715"u8,
        "0x92fcad4b5c0d52f451aec609b15da8e5e5626c4eaa88723bdeac9d25ca9b961269400410ca208a16af9c2fb07d799c32fe2f3cc5422f9711078d51a3797eb18e691295293284d8f5e69caf6decddfe1df6"u8
    )
}.slice();

public static void TestMontgomery(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var one = NewInt(1);
    var _B = @new<global::go.math.big_package.ΔInt>().Lsh(one, _W);
    foreach (var (i, test) in montgomeryTests) {
        var x = natFromString(test.x);
        var y = natFromString(test.y);
        var m = natFromString(test.m);
        while (len(x) < len(m)) {
            x = append(x, (global::go.math.big_package.Word)(0));
        }
        while (len(y) < len(m)) {
            y = append(y, (global::go.math.big_package.Word)(0));
        }
        if (x.cmp(m) > 0) {
            var (_, r) = ((global::go.math.big_package.nat)default!).div(default!, x, m);
            Ꮡt.Errorf("#%d: x > m (0x%s > 0x%s; use 0x%s)"u8, i, x.utoa(16), m.utoa(16), r.utoa(16));
        }
        if (y.cmp(m) > 0) {
            var (_, r) = ((global::go.math.big_package.nat)default!).div(default!, x, m);
            Ꮡt.Errorf("#%d: y > m (0x%s > 0x%s; use 0x%s)"u8, i, y.utoa(16), m.utoa(16), r.utoa(16));
        }
        global::go.math.big_package.nat @out = default!;
        if (_W == 32){
            @out = natFromString(test.out32);
        } else {
            @out = natFromString(test.out64);
        }
        // t.Logf("#%d: len=%d\n", i, len(m))
        // check output in table
        var xi = Ꮡ(new ΔInt(abs: x));
        var yi = Ꮡ(new ΔInt(abs: y));
        var mi = Ꮡ(new ΔInt(abs: m));
        var p = @new<global::go.math.big_package.ΔInt>().Mod(@new<global::go.math.big_package.ΔInt>().Mul(xi, @new<global::go.math.big_package.ΔInt>().Mul(yi, @new<global::go.math.big_package.ΔInt>().ModInverse(@new<global::go.math.big_package.ΔInt>().Lsh(one, (nuint)len(m) * (nuint)_W), mi))), mi);
        if (@out.cmp((~p).abs.norm()) != 0) {
            Ꮡt.Errorf("#%d: out in table=0x%s, computed=0x%s"u8, i, @out.utoa(16), (~p).abs.norm().utoa(16));
        }
        // check k0 in table
        var k = @new<global::go.math.big_package.ΔInt>().Mod(Ꮡ(new ΔInt(abs: m)), _B);
        k = @new<global::go.math.big_package.ΔInt>().Sub(_B, k);
        k = @new<global::go.math.big_package.ΔInt>().Mod(k, _B);
        global::go.math.big_package.Word k0 = ((global::go.math.big_package.Word)(nuint)@new<global::go.math.big_package.ΔInt>().ModInverse(k, _B).Uint64());
        if (k0 != ((global::go.math.big_package.Word)(nuint)test.k0)) {
            Ꮡt.Errorf("#%d: k0 in table=%#x, computed=%#x\n"u8, i, test.k0, k0);
        }
        // check montgomery with correct k0 produces correct output
        var z = ((global::go.math.big_package.nat)default!).montgomery(x, y, m, k0, len(m));
        z = z.norm();
        if (z.cmp(@out) != 0) {
            Ꮡt.Errorf("#%d: got 0x%s want 0x%s"u8, i, z.utoa(16), @out.utoa(16));
        }
    }
}

internal static slice<expTestsᴛ1> expNNTests = new expTestsᴛ1[]{
    new("0"u8, "0"u8, "0"u8, "1"u8),
    new("0"u8, "0"u8, "1"u8, "0"u8),
    new("1"u8, "1"u8, "1"u8, "0"u8),
    new("2"u8, "1"u8, "1"u8, "0"u8),
    new("2"u8, "2"u8, "1"u8, "0"u8),
    new("10"u8, "100000000000"u8, "1"u8, "0"u8),
    new("0x8000000000000000"u8, "2"u8, ""u8, "0x40000000000000000000000000000000"u8),
    new("0x8000000000000000"u8, "2"u8, "6719"u8, "4944"u8),
    new("0x8000000000000000"u8, "3"u8, "6719"u8, "5447"u8),
    new("0x8000000000000000"u8, "1000"u8, "6719"u8, "1603"u8),
    new("0x8000000000000000"u8, "1000000"u8, "6719"u8, "3199"u8),
    new(
        "2938462938472983472983659726349017249287491026512746239764525612965293865296239471239874193284792387498274256129746192347"u8,
        "298472983472983471903246121093472394872319615612417471234712061"u8,
        "29834729834729834729347290846729561262544958723956495615629569234729836259263598127342374289365912465901365498236492183464"u8,
        "23537740700184054162508175125554701713153216681790245129157191391322321508055833908509185839069455749219131480588829346291"u8
    ),
    new(
        "11521922904531591643048817447554701904414021819823889996244743037378330903763518501116638828335352811871131385129455853417360623007349090150042001944696604737499160174391019030572483602867266711107136838523916077674888297896995042968746762200926853379"u8,
        "426343618817810911523"u8,
        "444747819283133684179"u8,
        "42"u8
    ),
    new("375"u8, "249"u8, "388"u8, "175"u8),
    new("375"u8, "18446744073709551801"u8, "388"u8, "175"u8),
    new("0"u8, "0x40000000000000"u8, "0x200"u8, "0"u8),
    new("0xeffffff900002f00"u8, "0x40000000000000"u8, "0x200"u8, "0"u8),
    new("5"u8, "1435700818"u8, "72"u8, "49"u8),
    new("0xffff"u8, "0x300030003000300030003000300030003000302a3000300030003000300030003000300030003000300030003000300030003030623066307f3030783062303430383064303630343036"u8, "0x300000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000"u8, "0xa3f94c08b0b90e87af637cacc9383f7ea032352b8961fc036a52b659b6c9b33491b335ffd74c927f64ddd62cfca0001"u8)
}.slice();

public static void TestExpNN(ж<testing.T> Ꮡt) {
    foreach (var (i, test) in expNNTests) {
        var x = natFromString(test.x);
        var y = natFromString(test.y);
        var @out = natFromString(test.@out);
        global::go.math.big_package.nat m = default!;
        if (len(test.m) > 0) {
            m = natFromString(test.m);
        }
        var z = ((global::go.math.big_package.nat)default!).expNN(x, y, m, false);
        if (z.cmp(@out) != 0) {
            Ꮡt.Errorf("#%d got %s want %s"u8, i, z.utoa(10), @out.utoa(10));
        }
    }
}

public static void FuzzExpMont(ж<testing.F> Ꮡf) {
    Ꮡf.Fuzz((ж<testing.T> t, nuint x1, nuint x2, nuint x3, nuint y1, nuint y2, nuint y3, nuint m1, nuint m2, nuint m3) => {
        if (m1 == 0 && m2 == 0 && m3 == 0) {
            return;
        }
        var x = @new<global::go.math.big_package.ΔInt>().SetBits(new global::go.math.big_package.Word[]{((global::go.math.big_package.Word)x1), ((global::go.math.big_package.Word)x2), ((global::go.math.big_package.Word)x3)}.slice());
        var y = @new<global::go.math.big_package.ΔInt>().SetBits(new global::go.math.big_package.Word[]{((global::go.math.big_package.Word)y1), ((global::go.math.big_package.Word)y2), ((global::go.math.big_package.Word)y3)}.slice());
        var m = @new<global::go.math.big_package.ΔInt>().SetBits(new global::go.math.big_package.Word[]{((global::go.math.big_package.Word)m1), ((global::go.math.big_package.Word)m2), ((global::go.math.big_package.Word)m3)}.slice());
        var @out = @new<global::go.math.big_package.ΔInt>().Exp(x, y, m);
        var want = @new<global::go.math.big_package.ΔInt>().expSlow(x, y, m);
        if (@out.Cmp(want) != 0) {
            t.Errorf("x = %#x\ny=%#x\nz=%#x\nout=%#x\nwant=%#x\ndc: 16o 16i %X %X %X |p"u8, x.OrTypedNil(), y.OrTypedNil(), m.OrTypedNil(), @out.OrTypedNil(), want.OrTypedNil(), x.OrTypedNil(), y.OrTypedNil(), m.OrTypedNil());
        }
    });
}

public static void BenchmarkExp3Power(ж<testing.B> Ꮡb) {
    UntypedInt x = 3;
    foreach (var (_, y) in new global::go.math.big_package.Word[]{
        0x10, 0x40, 0x100, 0x400, 0x1000, 0x4000, 0x10000, 0x40000, 0x100000, 0x400000
    }.slice()) {
        Ꮡb.Run(fmt.Sprintf("%#x"u8, y), (ж<testing.B> bΔ1) => {
            global::go.math.big_package.nat z = default!;
            for (nint i = 0; i < (~bΔ1).N; i++) {
                z.expWW(x, y);
            }
        });
    }
}

internal static global::go.math.big_package.nat fibo(nint n) {
    switch (n) {
    case 0: {
        return default!;
    }
    case 1: {
        return new nat(new global::go.math.big_package.Word[]{1}.slice());
    }}

    var f0 = fibo(0);
    var f1 = fibo(1);
    global::go.math.big_package.nat f2 = default!;
    for (nint i = 1; i < n; i++) {
        f2 = f2.add(f0, f1);
        (f0, f1, f2) = (f1, f2, f0);
    }
    return f1;
}

internal static slice<@string> fiboNums = new @string[]{
    "0"u8,
    "55"u8,
    "6765"u8,
    "832040"u8,
    "102334155"u8,
    "12586269025"u8,
    "1548008755920"u8,
    "190392490709135"u8,
    "23416728348467685"u8,
    "2880067194370816120"u8,
    "354224848179261915075"u8
}.slice();

public static void TestFibo(ж<testing.T> Ꮡt) {
    foreach (var (i, want) in fiboNums) {
        nint n = i * 10;
        @string got = ((@string)fibo(n).utoa(10));
        if (got != want) {
            Ꮡt.Errorf("fibo(%d) failed: got %s want %s"u8, n, got, want);
        }
    }
}

public static void BenchmarkFibo(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        fibo(1);
        fibo(10);
        fibo(100);
        fibo(1000);
        fibo(10000);
        fibo(100000);
    }
}


partial struct bitTestsᴛ1 /*dyn*/ {
    internal @string x;
    internal nuint i;
    internal nuint want;
}
internal static slice<bitTestsᴛ1> bitTests = new bitTestsᴛ1[]{
    new("0"u8, 0, 0),
    new("0"u8, 1, 0),
    new("0"u8, 1000, 0),
    new("0x1"u8, 0, 1),
    new("0x10"u8, 0, 0),
    new("0x10"u8, 3, 0),
    new("0x10"u8, 4, 1),
    new("0x10"u8, 5, 0),
    new("0x8000000000000000"u8, 62, 0),
    new("0x8000000000000000"u8, 63, 1),
    new("0x8000000000000000"u8, 64, 0),
    new("0x3"u8 + strings.Repeat("0"u8, 32), 127, 0),
    new("0x3"u8 + strings.Repeat("0"u8, 32), 128, 1),
    new("0x3"u8 + strings.Repeat("0"u8, 32), 129, 1),
    new("0x3"u8 + strings.Repeat("0"u8, 32), 130, 0)
}.slice();

public static void TestBit(ж<testing.T> Ꮡt) {
    foreach (var (i, test) in bitTests) {
        var x = natFromString(test.x);
        {
            nuint got = x.bit(test.i); if (got != test.want) {
                Ꮡt.Errorf("#%d: %s.bit(%d) = %v; want %v"u8, i, test.x, test.i, got, test.want);
            }
        }
    }
}

internal static slice<bitTestsᴛ1> stickyTests = new bitTestsᴛ1[]{
    new("0"u8, 0, 0),
    new("0"u8, 1, 0),
    new("0"u8, 1000, 0),
    new("0x1"u8, 0, 0),
    new("0x1"u8, 1, 1),
    new("0x1350"u8, 0, 0),
    new("0x1350"u8, 4, 0),
    new("0x1350"u8, 5, 1),
    new("0x8000000000000000"u8, 63, 0),
    new("0x8000000000000000"u8, 64, 1),
    new("0x1"u8 + strings.Repeat("0"u8, 100), 400, 0),
    new("0x1"u8 + strings.Repeat("0"u8, 100), 401, 1)
}.slice();

public static void TestSticky(ж<testing.T> Ꮡt) {
    foreach (var (i, test) in stickyTests) {
        var x = natFromString(test.x);
        {
            nuint got = x.sticky(test.i); if (got != test.want) {
                Ꮡt.Errorf("#%d: %s.sticky(%d) = %v; want %v"u8, i, test.x, test.i, got, test.want);
            }
        }
        if (test.want == 1) {
            // all subsequent i's should also return 1
            for (nuint d = (nuint)1; d <= 3; d++) {
                {
                    nuint got = x.sticky(test.i + d); if (got != 1) {
                        Ꮡt.Errorf("#%d: %s.sticky(%d) = %v; want %v"u8, i, test.x, test.i + d, got, (nint)(1));
                    }
                }
            }
        }
    }
}

internal static void testSqr(ж<testing.T> Ꮡt, global::go.math.big_package.nat x) {
    var got = new global::go.math.big_package.nat(2 * len(x));
    var want = new global::go.math.big_package.nat(2 * len(x));
    got = got.sqr(x);
    want = want.mul(x, x);
    if (got.cmp(want) != 0) {
        Ꮡt.Errorf("basicSqr(%v), got %v, want %v"u8, x, got, want);
    }
}

public static void TestSqr(ж<testing.T> Ꮡt) {
    foreach (var (_, a) in prodNN) {
        if (a.x != default!) {
            testSqr(Ꮡt, a.x);
        }
        if (a.y != default!) {
            testSqr(Ꮡt, a.y);
        }
        if (a.z != default!) {
            testSqr(Ꮡt, a.z);
        }
    }
}

internal static void benchmarkNatSqr(ж<testing.B> Ꮡb, nint nwords) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = rndNat(nwords);
    global::go.math.big_package.nat z = default!;
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        z.sqr(x);
    }
}

internal static slice<nint> sqrBenchSizes = new nint[]{
    1, 2, 3, 5, 8, 10, 20, 30, 50, 80,
    100, 200, 300, 500, 800,
    1000, 10000, 100000
}.slice();

public static void BenchmarkNatSqr(ж<testing.B> Ꮡb) {
    foreach (var (_, n) in sqrBenchSizes) {
        if (isRaceBuilder && n > 1000) {
            continue;
        }
        Ꮡb.Run(fmt.Sprintf("%d"u8, n), (ж<testing.B> bΔ1) => {
            benchmarkNatSqr(bΔ1, n);
        });
    }
}

// 2^65, 2^66-1, 2^65 - (2^66-1) + 2^67
// 2^66-1, 2^65, 2^65-1

partial struct subMod2NTestsᴛ1 /*dyn*/ {
    internal @string x;
    internal @string y;
    internal nuint n;
    internal @string z;
}
internal static slice<subMod2NTestsᴛ1> subMod2NTests = new subMod2NTestsᴛ1[]{
    new("1"u8, "2"u8, 0, "0"u8),
    new("1"u8, "0"u8, 1, "1"u8),
    new("0"u8, "1"u8, 1, "1"u8),
    new("3"u8, "5"u8, 3, "6"u8),
    new("5"u8, "3"u8, 3, "2"u8),
    new("36893488147419103232"u8, "73786976294838206463"u8, 67, "110680464442257309697"u8),
    new("73786976294838206463"u8, "36893488147419103232"u8, 67, "36893488147419103231"u8)
}.slice();

public static void TestNatSubMod2N(ж<testing.T> Ꮡt) {
    foreach (var (_, mode) in new @string[]{"noalias"u8, "aliasX"u8, "aliasY"u8}.slice()) {
        Ꮡt.Run(mode, (ж<testing.T> tΔ1) => {
            foreach (var (_, tt) in subMod2NTests) {
                var x0 = natFromString(tt.x);
                var y0 = natFromString(tt.y);
                var want = natFromString(tt.z);
                var x = ((global::go.math.big_package.nat)default!).set(x0);
                var y = ((global::go.math.big_package.nat)default!).set(y0);
                global::go.math.big_package.nat z = default!;
                var exprᴛ1 = mode;
                if (exprᴛ1 == "aliasX"u8) {
                    z = x;
                }
                else if (exprᴛ1 == "aliasY"u8) {
                    z = y;
                }

                z = z.subMod2N(x, y, tt.n);
                if (z.cmp(want) != 0) {
                    tΔ1.Fatalf("subMod2N(%d, %d, %d) = %d, want %d"u8, x0, y0, tt.n, z, want);
                }
                if (mode != "aliasX"u8 && x.cmp(x0) != 0) {
                    tΔ1.Fatalf("subMod2N(%d, %d, %d) modified x"u8, x0, y0, tt.n);
                }
                if (mode != "aliasY"u8 && y.cmp(y0) != 0) {
                    tΔ1.Fatalf("subMod2N(%d, %d, %d) modified y"u8, x0, y0, tt.n);
                }
            }
        });
    }
}

public static void BenchmarkNatSetBytes(ж<testing.B> Ꮡb) {
    UntypedInt maxLength = 128;
    var lengths = new nint[]{ // No remainder:

        8, 24, maxLength, // With remainder:

        7, 23, maxLength - 1
    }.slice();
    var n = new global::go.math.big_package.nat(maxLength / _W); // ensure n doesn't need to grow during the test
    var buf = new slice<byte>(maxLength);
    foreach (var (_, l) in lengths) {
        var bufʗ1 = buf;
        var nʗ1 = n;
        Ꮡb.Run(fmt.Sprint(l), (ж<testing.B> bΔ1) => {
            for (nint i = 0; i < (~bΔ1).N; i++) {
                nʗ1.setBytes(bufʗ1.slice(0, l));
            }
        });
    }
}

public static void TestNatDiv(ж<testing.T> Ꮡt) {
    var sizes = new nint[]{
        1, 2, 5, 8, 15, 25, 40, 65, 100,
        200, 500, 800, 1500, 2500, 4000, 6500, 10000
    }.slice();
    foreach (var (_, i) in sizes) {
        foreach (var (_, j) in sizes) {
            var a = rndNat1(i);
            var b = rndNat1(j);
            // the test requires b >= 2
            if (len(b) == 1 && b[0] == 1) {
                b[0] = 2;
            }
            // choose a remainder c < b
            var c = rndNat1(len(b));
            if (len(c) == len(b) && c[len(c) - 1] >= b[len(b) - 1]) {
                c[len(c) - 1] = 0;
                c = c.norm();
            }
            // compute x = a*b+c
            var x = ((global::go.math.big_package.nat)default!).mul(a, b);
            x = x.add(x, c);
            global::go.math.big_package.nat q = default!;
            global::go.math.big_package.nat r = default!;
            (q, r) = q.div(r, x, b);
            if (q.cmp(a) != 0) {
                Ꮡt.Fatalf("wrong quotient: got %s; want %s for %s/%s"u8, q.utoa(10), a.utoa(10), x.utoa(10), b.utoa(10));
            }
            if (r.cmp(c) != 0) {
                Ꮡt.Fatalf("wrong remainder: got %s; want %s for %s/%s"u8, r.utoa(10), c.utoa(10), x.utoa(10), b.utoa(10));
            }
        }
    }
}

// TestIssue37499 triggers the edge case of divBasic where
// the inaccurate estimate of the first word's quotient
// happens at the very beginning of the loop.
public static void TestIssue37499(ж<testing.T> Ꮡt) {
    // Choose u and v such that v is slightly larger than u >> N.
    // This tricks divBasic into choosing 1 as the first word
    // of the quotient. This works in both 32-bit and 64-bit settings.
    var u = natFromString("0x2b6c385a05be027f5c22005b63c42a1165b79ff510e1706b39f8489c1d28e57bb5ba4ef9fd9387a3e344402c0a453381"u8);
    var v = natFromString("0x2b6c385a05be027f5c22005b63c42a1165b79ff510e1706c"u8);
    var q = ((global::go.math.big_package.nat)default!).make(8);
    q.divBasic(u, v);
    q = q.norm();
    {
        @string s = ((@string)q.utoa(16)); if (s != "fffffffffffffffffffffffffffffffffffffffffffffffb"u8) {
            Ꮡt.Fatalf("incorrect quotient: %s"u8, s);
        }
    }
}

// TestIssue42552 triggers an edge case of recursive division
// where the first division loop is never entered, and correcting
// the remainder takes exactly two iterations in the final loop.
public static void TestIssue42552(ж<testing.T> Ꮡt) {
    var u = natFromString("0xc23b166884c3869092a520eceedeced2b00847bd256c9cf3b2c5e2227c15bd5e6ee7ef8a2f49236ad0eedf2c8a3b453cf6e0706f64285c526b372c4b1321245519d430540804a50b7ca8b6f1b34a2ec05cdbc24de7599af112d3e3c8db347e8799fe70f16e43c6566ba3aeb169463a3ecc486172deb2d9b80a3699c776e44fef20036bd946f1b4d054dd88a2c1aeb986199b0b2b7e58c42288824b74934d112fe1fc06e06b4d99fe1c5e725946b23210521e209cd507cce90b5f39a523f27e861f9e232aee50c3f585208b4573dcc0b897b6177f2ba20254fd5c50a033e849dee1b3a93bd2dc44ba8ca836cab2c2ae50e50b126284524fa0187af28628ff0face68d87709200329db1392852c8b8963fbe3d05fb1efe19f0ed5ca9fadc2f96f82187c24bb2512b2e85a66333a7e176605695211e1c8e0b9b9e82813e50654964945b1e1e66a90840396c7d10e23e47f364d2d3f660fa54598e18d1ca2ea4fe4f35a40a11f69f201c80b48eaee3e2e9b0eda63decf92bec08a70f731587d4ed0f218d5929285c8b2ccbc497e20db42de73885191fa453350335990184d8df805072f958d5354debda38f5421effaaafd6cb9b721ace74be0892d77679f62a4a126697cd35797f6858193da4ba1770c06aea2e5c59ec04b8ea26749e61b72ecdde403f3bc7e5e546cd799578cc939fa676dfd5e648576d4a06cbadb028adc2c0b461f145b2321f42e5e0f3b4fb898ecd461df07a6f5154067787bf74b5cc5c03704a1ce47494961931f0263b0aac32505102595957531a2de69dd71aac51f8a49902f81f21283dbe8e21e01e5d82517868826f86acf338d935aa6b4d5a25c8d540389b277dd9d64569d68baf0f71bd03dba45b92a7fc052601d1bd011a2fc6790a23f97c6fa5caeea040ab86841f268d39ce4f7caf01069df78bba098e04366492f0c2ac24f1bf16828752765fa523c9a4d42b71109d123e6be8c7b1ab3ccf8ea03404075fe1a9596f1bba1d267f9a7879ceece514818316c9c0583469d2367831fc42b517ea028a28df7c18d783d16ea2436cee2b15d52db68b5dfdee6b4d26f0905f9b030c911a04d078923a4136afea96eed6874462a482917353264cc9bee298f167ac65a6db4e4eda88044b39cc0b33183843eaa946564a00c3a0ab661f2c915e70bf0bb65bfbb6fa2eea20aed16bf2c1a1d00ec55fb4ff2f76b8e462ea70c19efa579c9ee78194b86708fdae66a9ce6e2cf3d366037798cfb50277ba6d2fd4866361022fd788ab7735b40b8b61d55e32243e06719e53992e9ac16c9c4b6e6933635c3c47c8f7e73e17dd54d0dd8aeba5d76de46894e7b3f9d3ec25ad78ee82297ba69905ea0fa094b8667faa2b8885e2187b3da80268aa1164761d7b0d6de206b676777348152b8ae1d4afed753bc63c739a5ca8ce7afb2b241a226bd9e502baba391b5b13f5054f070b65a9cf3a67063bfaa803ba390732cd03888f664023f888741d04d564e0b5674b0a183ace81452001b3fbb4214c77d42ca75376742c471e58f67307726d56a1032bd236610cbcbcd03d0d7a452900136897dc55bb3ce959d10d4e6a10fb635006bd8c41cd9ded2d3dfdd8f2e229590324a7370cb2124210b2330f4c56155caa09a2564932ceded8d92c79664dcdeb87faad7d3da006cc2ea267ee3df41e9677789cc5a8cc3b83add6491561b3047919e0648b1b2e97d7ad6f6c2aa80cab8e9ae10e1f75b1fdd0246151af709d259a6a0ed0b26bd711024965ecad7c41387de45443defce53f66612948694a6032279131c257119ed876a8e805dfb49576ef5c563574115ee87050d92d191bc761ef51d966918e2ef925639400069e3959d8fe19f36136e947ff430bf74e71da0aa5923b00000000"u8);
    var v = natFromString("0x838332321d443a3d30373d47301d47073847473a383d3030f25b3d3d3e00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000002e00000000000000000041603038331c3d32f5303441e0e0e0e0e0e0e0e0e0e0e0e0e0e0e0e0e0e0e0e0e0e0e0e0e0e0e0e0e0e0e0e0e0e0e0e0e0e0e0e0e0e0e0e0e0e0e0e0e0e0e01c0a5459bfc7b9be9fcbb9d2383840464319434707303030f43a32f53034411c0a5459413820878787878787878787878787878787878787878787878787878787878787878787870630303a3a30334036605b923a6101f83638413943413960204337602043323801526040523241846038414143015238604060328452413841413638523c0240384141364036605b923a6101f83638413943413960204334602043323801526040523241846038414143015238604060328452413841413638523c02403841413638433030f25a8b83838383838383838383838383838383837d838383ffffffffffffffff838383838383838383000000000000000000030000007d26e27c7c8b83838383838383838383838383838383837d838383ffffffffffffffff83838383838383838383838383838383838383838383435960f535073030f3343200000000000000011881301938343030fa398383300000002300000000000000000000f11af4600c845252904141364138383c60406032414443095238010241414303364443434132305b595a15434160b042385341ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff47476043410536613603593a6005411c437405fcfcfcfcfcfcfc0000000000005a3b075815054359000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000"u8);
    var q = ((global::go.math.big_package.nat)default!).make(16);
    q.div(q, u, v);
}

} // end big_internal_test_package
