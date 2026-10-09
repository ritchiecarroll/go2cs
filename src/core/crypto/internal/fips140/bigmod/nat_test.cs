// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140;

using bufio = bufio_package;
using bytes = bytes_package;
using cryptorand = go.crypto.rand_package;
using hex = encoding.hex_package;
using fmt = fmt_package;
using big = math.big_package;
using bits = math.bits_package;
using rand = math.rand_package;
using os = os_package;
using reflect = reflect_package;
using slices = slices_package;
using strings = strings_package;
using testing = testing_package;
using quick = go.testing.quick_package;
using encoding;
using go.testing;
using io = io_package;
using math;
using static go.crypto.@internal.fips140.bigmod_package;

partial class bigmod_internal_test_package {

// setBig assigns x = n, optionally resizing n to the appropriate size.
//
// The announced length of x is set based on the actual bit size of the input,
// ignoring leading zeroes.
internal static ж<global::go.crypto.@internal.fips140.bigmod_package.ΔNat> setBig(this ж<global::go.crypto.@internal.fips140.bigmod_package.ΔNat> Ꮡx, ж<bigꓸInt> Ꮡn) {
    ref var x = ref Ꮡx.DerefOrNull();
    ref var n = ref Ꮡn.DerefOrNull();

    var limbs = n.Bits();
    Ꮡx.reset(len(limbs));
    foreach (var (i, _) in limbs) {
        x.limbs[i] = (nuint)limbs[i];
    }
    return Ꮡx;
}

internal static ж<bigꓸInt> asBig(this ref global::go.crypto.@internal.fips140.bigmod_package.ΔNat n) {
    var bits = new slice<big.Word>(len(n.limbs));
    foreach (var (i, _) in n.limbs) {
        bits[i] = ((big.Word)n.limbs[i]);
    }
    return @new<bigꓸInt>().SetBits(bits);
}

internal static @string String(this ref global::go.crypto.@internal.fips140.bigmod_package.ΔNat n) {
    slice<@string> limbs = default!;
    foreach (var (i, _) in n.limbs) {
        limbs = append(limbs, fmt.Sprintf("%016X"u8, n.limbs[len(n.limbs) - 1 - i]));
    }
    return "{"u8 + strings.Join(limbs, " "u8) + "}"u8;
}

// Generate generates an even nat. It's used by testing/quick to produce random
// *nat values for quick.Check invocations.
internal static reflectꓸValue Generate(this ref global::go.crypto.@internal.fips140.bigmod_package.ΔNat _, ж<rand.Rand> Ꮡr, nint size) {
    ref var r = ref Ꮡr.DerefOrNull();

    var limbs = new slice<nuint>(size);
    for (nint i = 0; i < size; i++) {
        limbs[i] = (nuint)((nuint)r.Uint64() & (nuint)(unchecked((nuint)(18446744073709551614UL))));
    }
    return reflect.ValueOf(Ꮡ(new ΔNat(limbs)));
}

internal static bool testModAddCommutative(ж<global::go.crypto.@internal.fips140.bigmod_package.ΔNat> Ꮡa, ж<global::go.crypto.@internal.fips140.bigmod_package.ΔNat> Ꮡb) {
    ref var a = ref Ꮡa.DerefOrNull();

    var m = maxModulus((nuint)len(a.limbs));
    var aPlusB = @new<global::go.crypto.@internal.fips140.bigmod_package.ΔNat>().set(Ꮡa);
    aPlusB.Add(Ꮡb, m);
    var bPlusA = @new<global::go.crypto.@internal.fips140.bigmod_package.ΔNat>().set(Ꮡb);
    bPlusA.Add(Ꮡa, m);
    return aPlusB.Equal(bPlusA) == 1;
}

public static void TestModAddCommutative(ж<testing.T> Ꮡt) {
    var err = quick.Check(testModAddCommutative, Ꮡ(new quick.Config(nil)));
    if (err != default!) {
        Ꮡt.Error(err);
    }
}

internal static bool testModSubThenAddIdentity(ж<global::go.crypto.@internal.fips140.bigmod_package.ΔNat> Ꮡa, ж<global::go.crypto.@internal.fips140.bigmod_package.ΔNat> Ꮡb) {
    ref var a = ref Ꮡa.DerefOrNull();

    var m = maxModulus((nuint)len(a.limbs));
    var original = @new<global::go.crypto.@internal.fips140.bigmod_package.ΔNat>().set(Ꮡa);
    Ꮡa.Sub(Ꮡb, m);
    Ꮡa.Add(Ꮡb, m);
    return a.Equal(original) == 1;
}

public static void TestModSubThenAddIdentity(ж<testing.T> Ꮡt) {
    var err = quick.Check(testModSubThenAddIdentity, Ꮡ(new quick.Config(nil)));
    if (err != default!) {
        Ꮡt.Error(err);
    }
}

public static void TestMontgomeryRoundtrip(ж<testing.T> Ꮡt) {
    var err = quick.Check(bool (ж<global::go.crypto.@internal.fips140.bigmod_package.ΔNat> a) => {
        var one = Ꮡ(new ΔNat(new slice<nuint>(len((~a).limbs))));
        one.Value.limbs[0] = 1;
        var aPlusOne = @new<bigꓸInt>().SetBytes(natBytes(a));
        aPlusOne.Add(aPlusOne, big.NewInt(1));
        var (m, _) = NewModulus(aPlusOne.Bytes());
        var monty = @new<global::go.crypto.@internal.fips140.bigmod_package.ΔNat>().set(a);
        monty.montgomeryRepresentation(m);
        var aAgain = @new<global::go.crypto.@internal.fips140.bigmod_package.ΔNat>().set(monty);
        aAgain.montgomeryMul(monty, one, m);
        if (a.Equal(aAgain) != 1) {
            Ꮡt.Errorf("%v != %v"u8, a.OrTypedNil(), aAgain.OrTypedNil());
            return false;
        }
        return true;
    }, Ꮡ(new quick.Config(nil)));
    if (err != default!) {
        Ꮡt.Error(err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object examplesAreOnlyValidIn64ˢ = (@string)"examples are only valid in 64 bit"u8;

internal partial struct TestShiftIn_examples /*dyn*/ {
    internal slice<byte> m, x, expected;
    internal uint64 y;
}

public static void TestShiftIn(ж<testing.T> Ꮡt) {
    if (bits.UintSize != 64) {
        Ꮡt.Skip(examplesAreOnlyValidIn64ˢ);
    }
    var examples = new TestShiftIn_examples[]{new(
        m: new byte[]{13}.slice(),
        x: new byte[]{0}.slice(),
        y: 0xFFFF_FFFF_FFFF_FFFFUL,
        expected: new byte[]{2}.slice()
    ), new(
        m: new byte[]{13}.slice(),
        x: new byte[]{7}.slice(),
        y: 0xFFFF_FFFF_FFFF_FFFFUL,
        expected: new byte[]{10}.slice()
    ), new(
        m: new byte[]{0x06, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0d}.slice(),
        x: new slice<byte>(9),
        y: 0xFFFF_FFFF_FFFF_FFFFUL,
        expected: new byte[]{0x00, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff}.slice()
    ), new(
        m: new byte[]{0x06, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0d}.slice(),
        x: new byte[]{0x00, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff}.slice(),
        y: 0,
        expected: new byte[]{0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x06}.slice()
    )
    }.slice();
    foreach (var (i, tt) in examples) {
        var m = modulusFromBytes(tt.m);
        var got = natFromBytes(tt.x).ExpandFor(m).shiftIn((nuint)tt.y, m);
        {
            var exp = natFromBytes(tt.expected).ExpandFor(m); if (got.Equal(exp) != 1) {
                Ꮡt.Errorf("%d: got %v, expected %v"u8, i, got.OrTypedNil(), exp.OrTypedNil());
            }
        }
    }
}

public static void TestModulusAndNatSizes(ж<testing.T> Ꮡt) {
    // These are 126 bit (2 * _W on 64-bit architectures) values, serialized as
    // 128 bits worth of bytes. If leading zeroes are stripped, they fit in two
    // limbs, if they are not, they fit in three. This can be a problem because
    // modulus strips leading zeroes and nat does not.
    var m = modulusFromBytes(new byte[]{
        0x3f, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
        0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff}.slice());
    var xb = new byte[]{0x3f, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
        0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xfe}.slice();
    natFromBytes(xb).ExpandFor(m); // must not panic for shrinking
    NewNat().SetBytes(xb, m);
}

internal partial struct TestSetBytes_tests /*dyn*/ {
    internal slice<byte> m, b;
    internal bool fail;
}

public static void TestSetBytes(ж<testing.T> Ꮡt) {
    var tests = new TestSetBytes_tests[]{new(
        m: new byte[]{0xff, 0xff}.slice(),
        b: new byte[]{0x00, 0x01}.slice()
    ), new(
        m: new byte[]{0xff, 0xff}.slice(),
        b: new byte[]{0xff, 0xff}.slice(),
        fail: true
    ), new(
        m: new byte[]{0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff}.slice(),
        b: new byte[]{0x00, 0x01}.slice()
    ), new(
        m: new byte[]{0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff}.slice(),
        b: new byte[]{0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xfe}.slice()
    ), new(
        m: new byte[]{0xff, 0xff}.slice(),
        b: new byte[]{0xff, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00}.slice(),
        fail: true
    ), new(
        m: new byte[]{0xff, 0xff}.slice(),
        b: new byte[]{0xff, 0xff, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00}.slice(),
        fail: true
    ), new(
        m: new byte[]{0x7f, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff}.slice(),
        b: new byte[]{0x7f, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xfe}.slice()
    ), new(
        m: new byte[]{0x7f, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff}.slice(),
        b: new byte[]{0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xfe}.slice(),
        fail: true
    ), new(
        m: new byte[]{0x7f, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff}.slice(),
        b: new byte[]{0x7f, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff}.slice(),
        fail: true
    ), new(
        m: new byte[]{0x7f, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff}.slice(),
        b: new byte[]{0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xfe}.slice(),
        fail: true
    ), new(
        m: new byte[]{0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xfd}.slice(),
        b: new byte[]{0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff}.slice(),
        fail: true
    )
    }.slice();
    foreach (var (i, tt) in tests) {
        var m = modulusFromBytes(tt.m);
        var (got, errΔ1) = NewNat().SetBytes(tt.b, m);
        if (errΔ1 != default!) {
            if (!tt.fail) {
                Ꮡt.Errorf("%d: unexpected error: %v"u8, i, errΔ1);
            }
            continue;
        }
        if (tt.fail) {
            Ꮡt.Errorf("%d: unexpected success"u8, i);
            continue;
        }
        {
            var expected = natFromBytes(tt.b).ExpandFor(m); if (got.Equal(expected) != yes) {
                Ꮡt.Errorf("%d: got %v, expected %v"u8, i, got.OrTypedNil(), expected.OrTypedNil());
            }
        }
    }
    var f = (slice<byte> xBytes) => {
        var m = maxModulus((nuint)(len(xBytes) * 8 / (nint)_W + 1));
        var (got, errΔ2) = NewNat().SetBytes(xBytes, m);
        if (errΔ2 != default!) {
            return false;
        }
        return got.Equal(natFromBytes(xBytes).ExpandFor(m)) == yes;
    };
    var err = quick.Check((f).OrTypedNilFunc(), Ꮡ(new quick.Config(nil)));
    if (err != default!) {
        Ꮡt.Error(err);
    }
}

internal partial struct TestExpand_examples /*dyn*/ {
    internal slice<nuint> @in;
    internal nint n;
    internal slice<nuint> @out;
}

public static void TestExpand(ж<testing.T> Ꮡt) {
    var sliced = new nuint[]{1, 2, 3, 4}.slice();
    var examples = new TestExpand_examples[]{new(
        new nuint[]{1, 2}.slice(),
        4,
        new nuint[]{1, 2, 0, 0}.slice()
    ), new(
        sliced[..2],
        4,
        new nuint[]{1, 2, 0, 0}.slice()
    ), new(
        new nuint[]{1, 2}.slice(),
        2,
        new nuint[]{1, 2}.slice()
    )
    }.slice();
    foreach (var (i, tt) in examples) {
        var got = (Ꮡ(new ΔNat(tt.@in))).expand(tt.n);
        if (len((~got).limbs) != len(tt.@out) || got.Equal(Ꮡ(new ΔNat(tt.@out))) != 1) {
            Ꮡt.Errorf("%d: got %v, expected %v"u8, i, got.OrTypedNil(), tt.@out);
        }
    }
}

public static void TestMod(ж<testing.T> Ꮡt) {
    var m = modulusFromBytes(new byte[]{0x06, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0d}.slice());
    var x = natFromBytes(new byte[]{0x40, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01}.slice());
    var @out = @new<global::go.crypto.@internal.fips140.bigmod_package.ΔNat>();
    @out.Mod(x, m);
    var expected = natFromBytes(new byte[]{0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x09}.slice());
    if (@out.Equal(expected) != 1) {
        Ꮡt.Errorf("%+v != %+v"u8, @out.OrTypedNil(), expected.OrTypedNil());
    }
}

public static void TestModSub(ж<testing.T> Ꮡt) {
    var m = modulusFromBytes(new byte[]{13}.slice());
    var x = Ꮡ(new ΔNat(new nuint[]{6}.slice()));
    var y = Ꮡ(new ΔNat(new nuint[]{7}.slice()));
    x.Sub(y, m);
    var expected = Ꮡ(new ΔNat(new nuint[]{12}.slice()));
    if (x.Equal(expected) != 1) {
        Ꮡt.Errorf("%+v != %+v"u8, x.OrTypedNil(), expected.OrTypedNil());
    }
    x.Sub(y, m);
    expected = Ꮡ(new ΔNat(new nuint[]{5}.slice()));
    if (x.Equal(expected) != 1) {
        Ꮡt.Errorf("%+v != %+v"u8, x.OrTypedNil(), expected.OrTypedNil());
    }
}

public static void TestModAdd(ж<testing.T> Ꮡt) {
    var m = modulusFromBytes(new byte[]{13}.slice());
    var x = Ꮡ(new ΔNat(new nuint[]{6}.slice()));
    var y = Ꮡ(new ΔNat(new nuint[]{7}.slice()));
    x.Add(y, m);
    var expected = Ꮡ(new ΔNat(new nuint[]{0}.slice()));
    if (x.Equal(expected) != 1) {
        Ꮡt.Errorf("%+v != %+v"u8, x.OrTypedNil(), expected.OrTypedNil());
    }
    x.Add(y, m);
    expected = Ꮡ(new ΔNat(new nuint[]{7}.slice()));
    if (x.Equal(expected) != 1) {
        Ꮡt.Errorf("%+v != %+v"u8, x.OrTypedNil(), expected.OrTypedNil());
    }
}

public static void TestExp(ж<testing.T> Ꮡt) {
    var m = modulusFromBytes(new byte[]{13}.slice());
    var x = Ꮡ(new ΔNat(new nuint[]{3}.slice()));
    var @out = Ꮡ(new ΔNat(new nuint[]{0}.slice()));
    @out.Exp(x, new byte[]{12}.slice(), m);
    var expected = Ꮡ(new ΔNat(new nuint[]{1}.slice()));
    if (@out.Equal(expected) != 1) {
        Ꮡt.Errorf("%+v != %+v"u8, @out.OrTypedNil(), expected.OrTypedNil());
    }
}

public static void TestExpShort(ж<testing.T> Ꮡt) {
    var m = modulusFromBytes(new byte[]{13}.slice());
    var x = Ꮡ(new ΔNat(new nuint[]{3}.slice()));
    var @out = Ꮡ(new ΔNat(new nuint[]{0}.slice()));
    @out.ExpShortVarTime(x, 12, m);
    var expected = Ꮡ(new ΔNat(new nuint[]{1}.slice()));
    if (@out.Equal(expected) != 1) {
        Ꮡt.Errorf("%+v != %+v"u8, @out.OrTypedNil(), expected.OrTypedNil());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object aBModAB0ˢ = (@string)"a * b mod (a * b) != 0"u8;
internal static readonly object aInvAModB1ˢ = (@string)"a * inv(a) mod b != 1"u8;

// TestMulReductions tests that Mul reduces results equal or slightly greater
// than the modulus. Some Montgomery algorithms don't and need extra care to
// return correct results. See https://go.dev/issue/13907.
public static void TestMulReductions(ж<testing.T> Ꮡt) {
    // Two short but multi-limb primes.
    var (a, _) = @new<bigꓸInt>().SetString("773608962677651230850240281261679752031633236267106044359907"u8, 10);
    var (b, _) = @new<bigꓸInt>().SetString("180692823610368451951102211649591374573781973061758082626801"u8, 10);
    var n = @new<bigꓸInt>().Mul(a, b);
    var (N, _) = NewModulus(n.Bytes());
    var A = NewNat().setBig(a).ExpandFor(N);
    var B = NewNat().setBig(b).ExpandFor(N);
    if (A.Mul(B, N).IsZero() != 1) {
        Ꮡt.Error(aBModAB0ˢ);
    }
    var i = @new<bigꓸInt>().ModInverse(a, b);
    (N, _) = NewModulus(b.Bytes());
    A = NewNat().setBig(a).ExpandFor(N);
    var I = NewNat().setBig(i).ExpandFor(N);
    var one = NewNat().setBig(big.NewInt(1)).ExpandFor(N);
    if (A.Mul(I, N).Equal(one) != 1) {
        Ꮡt.Error(aInvAModB1ˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string smallˢ = "small"u8;

public static void TestMul(ж<testing.T> Ꮡt) {
    Ꮡt.Run(smallˢ, (ж<testing.T> tΔ1) => {
        testMul(tΔ1, 760 / 8);
    });
    Ꮡt.Run("1024"u8, (ж<testing.T> tΔ2) => {
        testMul(tΔ2, 1024 / 8);
    });
    Ꮡt.Run("1536"u8, (ж<testing.T> tΔ3) => {
        testMul(tΔ3, 1536 / 8);
    });
    Ꮡt.Run("2048"u8, (ж<testing.T> tΔ4) => {
        testMul(tΔ4, 2048 / 8);
    });
}

internal static void testMul(ж<testing.T> Ꮡt, nint n) {
    var (a, b, m) = (new slice<byte>(n), new slice<byte>(n), new slice<byte>(n));
    cryptorand.Read(a);
    cryptorand.Read(b);
    cryptorand.Read(m);
    // Pick the highest as the modulus.
    if (bytes.Compare(a, m) > 0) {
        (a, m) = (m, a);
    }
    if (bytes.Compare(b, m) > 0) {
        (b, m) = (m, b);
    }
    var (M, err) = NewModulus(m);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (var A, err) = NewNat().SetBytes(a, M);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (var B, err) = NewNat().SetBytes(b, M);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    A.Mul(B, M);
    var ABytes = A.Bytes(M);
    var mBig = @new<bigꓸInt>().SetBytes(m);
    var aBig = @new<bigꓸInt>().SetBytes(a);
    var bBig = @new<bigꓸInt>().SetBytes(b);
    var nBig = @new<bigꓸInt>().Mul(aBig, bBig);
    nBig.Mod(nBig, mBig);
    var nBigBytes = new slice<byte>(len(ABytes));
    nBig.FillBytes(nBigBytes);
    if (!bytes.Equal(ABytes, nBigBytes)) {
        Ꮡt.Errorf("got %x, want %x"u8, ABytes, nBigBytes);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string isNot1Mod4ˢ = "3 is not -1 mod 4"u8;
internal static readonly @string isZeroˢ = "3 is zero"u8;
internal static readonly @string isOneˢ = "3 is one"u8;
internal static readonly @string isNotOddˢ = "3 is not odd"u8;
internal static readonly @string is1Mod4ˢ = "2 is -1 mod 4"u8;
internal static readonly @string isZeroˢ2 = "2 is zero"u8;
internal static readonly @string isOneˢ2 = "2 is one"u8;
internal static readonly @string isOddˢ = "2 is odd"u8;
internal static readonly @string is1Mod4ˢ2 = "1 is -1 mod 4"u8;
internal static readonly @string isZeroˢ3 = "1 is zero"u8;
internal static readonly @string isNotOneˢ = "1 is not one"u8;
internal static readonly @string isNotOddˢ2 = "1 is not odd"u8;
internal static readonly @string is1Mod4ˢ3 = "0 is -1 mod 4"u8;
internal static readonly @string isNotZeroˢ = "0 is not zero"u8;
internal static readonly @string isOneˢ3 = "0 is one"u8;
internal static readonly @string isOddˢ2 = "0 is odd"u8;
internal static readonly @string isNot1Mod4ˢ2 = "-1 is not -1 mod 4"u8;
internal static readonly @string isZeroˢ4 = "-1 is zero"u8;
internal static readonly @string isOneˢ4 = "-1 is one"u8;
internal static readonly @string mod4IsNotOddˢ = "-1 mod 4 is not odd"u8;

public static void TestIs(ж<testing.T> Ꮡt) {
    void checkYes(global::go.crypto.@internal.fips140.bigmod_package.choice c, @string errΔ1) {
        Ꮡt.Helper();
        if (c != yes) {
            Ꮡt.Error(errΔ1);
        }
    }
    void checkNot(global::go.crypto.@internal.fips140.bigmod_package.choice c, @string errΔ2) {
        Ꮡt.Helper();
        if (c != no) {
            Ꮡt.Error(errΔ2);
        }
    }
    var mFour = modulusFromBytes(new byte[]{4}.slice());
    var (n, err) = NewNat().SetBytes(new byte[]{3}.slice(), mFour);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    checkYes(n.IsMinusOne(mFour), isNot1Mod4ˢ);
    checkNot(n.IsZero(), isZeroˢ);
    checkNot(n.IsOne(), isOneˢ);
    checkYes(n.IsOdd(), isNotOddˢ);
    n.SubOne(mFour);
    checkNot(n.IsMinusOne(mFour), is1Mod4ˢ);
    checkNot(n.IsZero(), isZeroˢ2);
    checkNot(n.IsOne(), isOneˢ2);
    checkNot(n.IsOdd(), isOddˢ);
    n.SubOne(mFour);
    checkNot(n.IsMinusOne(mFour), is1Mod4ˢ2);
    checkNot(n.IsZero(), isZeroˢ3);
    checkYes(n.IsOne(), isNotOneˢ);
    checkYes(n.IsOdd(), isNotOddˢ2);
    n.SubOne(mFour);
    checkNot(n.IsMinusOne(mFour), is1Mod4ˢ3);
    checkYes(n.IsZero(), isNotZeroˢ);
    checkNot(n.IsOne(), isOneˢ3);
    checkNot(n.IsOdd(), isOddˢ2);
    n.SubOne(mFour);
    checkYes(n.IsMinusOne(mFour), isNot1Mod4ˢ2);
    checkNot(n.IsZero(), isZeroˢ4);
    checkNot(n.IsOne(), isOneˢ4);
    checkYes(n.IsOdd(), mod4IsNotOddˢ);
    var mTwoLimbs = maxModulus(2);
    (n, err) = NewNat().SetBytes(new byte[]{0x01}.slice(), mTwoLimbs);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (n.IsOne() != 1) {
        Ꮡt.Errorf("1 is not one"u8);
    }
}

public static void TestTrailingZeroBits(ж<testing.T> Ꮡt) {
    var nb = @new<bigꓸInt>().SetBytes(new byte[]{0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x7e}.slice());
    nb.Lsh(nb, 128);
    nint expected = 129;
    while (expected >= 0) {
        var n = NewNat().setBig(nb);
        if (n.TrailingZeroBitsVarTime() != (nuint)expected) {
            Ꮡt.Errorf("%d != %d"u8, n.TrailingZeroBitsVarTime(), expected);
        }
        nb.Rsh(nb, 1);
        expected--;
    }
}

public static void TestRightShift(ж<testing.T> Ꮡt) {
    var (nb, err) = cryptorand.Int(cryptorand.Reader, @new<bigꓸInt>().Lsh(big.NewInt(1), 1024));
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    foreach (var (_, shift) in new nuint[]{1, 32, 64, 128, 1024 - 128, 1024 - 64, 1024 - 32, 1024 - 1}.slice()) {
        var nbʗ1 = nb;
        void testShift(ж<testing.T> tΔ1, nuint shiftΔ1) {
            var n = NewNat().setBig(nbʗ1);
            nint oldLen = len((~n).limbs);
            n.ShiftRightVarTime(shiftΔ1);
            if (len((~n).limbs) != oldLen) {
                tΔ1.Errorf("len(n.limbs) = %d, want %d"u8, len((~n).limbs), oldLen);
            }
            var exp = @new<bigꓸInt>().Rsh(nbʗ1, shiftΔ1);
            if (n.asBig().Cmp(exp) != 0) {
                tΔ1.Errorf("%v != %v"u8, n.asBig().OrTypedNil(), exp.OrTypedNil());
            }
        }
        var testShiftʗ1 = testShift;
        Ꮡt.Run(fmt.Sprint(shift - 1), (ж<testing.T> tΔ2) => {
            testShiftʗ1(tΔ2, shift - 1);
        });
        var testShiftʗ2 = testShift;
        Ꮡt.Run(fmt.Sprint(shift), (ж<testing.T> tΔ3) => {
            testShiftʗ2(tΔ3, shift);
        });
        var testShiftʗ3 = testShift;
        Ꮡt.Run(fmt.Sprint(shift + 1), (ж<testing.T> tΔ4) => {
            testShiftʗ3(tΔ4, shift + 1);
        });
    }
}

internal static slice<byte> natBytes(ж<global::go.crypto.@internal.fips140.bigmod_package.ΔNat> Ꮡn) {
    ref var n = ref Ꮡn.DerefOrNull();

    return n.Bytes(maxModulus((nuint)len(n.limbs)));
}

internal static ж<global::go.crypto.@internal.fips140.bigmod_package.ΔNat> natFromBytes(slice<byte> b) {
    // Must not use Nat.SetBytes as it's used in TestSetBytes.
    var bb = @new<bigꓸInt>().SetBytes(b);
    return NewNat().setBig(bb);
}

internal static ж<global::go.crypto.@internal.fips140.bigmod_package.Modulus> modulusFromBytes(slice<byte> b) {
    var bb = @new<bigꓸInt>().SetBytes(b);
    var (m, _) = NewModulus(bb.Bytes());
    return m;
}

// maxModulus returns the biggest modulus that can fit in n limbs.
internal static ж<global::go.crypto.@internal.fips140.bigmod_package.Modulus> maxModulus(nuint n) {
    var b = big.NewInt(1);
    b.Lsh(b, n * (nuint)_W);
    b.Sub(b, big.NewInt(1));
    var (m, _) = NewModulus(b.Bytes());
    return m;
}

internal static ж<global::go.crypto.@internal.fips140.bigmod_package.Modulus> makeBenchmarkModulus() {
    return maxModulus(32);
}

internal static ж<global::go.crypto.@internal.fips140.bigmod_package.ΔNat> makeBenchmarkValue() {
    var x = new slice<nuint>(32);
    for (nint i = 0; i < 32; i++) {
        x[i]--;
    }
    return Ꮡ(new ΔNat(limbs: x));
}

internal static slice<byte> makeBenchmarkExponent() {
    var e = new slice<byte>(256);
    for (nint i = 0; i < 32; i++) {
        e[i] = 0xFF;
    }
    return e;
}

public static void BenchmarkModAdd(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = makeBenchmarkValue();
    var y = makeBenchmarkValue();
    var m = makeBenchmarkModulus();
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        x.Add(y, m);
    }
}

public static void BenchmarkModSub(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = makeBenchmarkValue();
    var y = makeBenchmarkValue();
    var m = makeBenchmarkModulus();
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        x.Sub(y, m);
    }
}

public static void BenchmarkMontgomeryRepr(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = makeBenchmarkValue();
    var m = makeBenchmarkModulus();
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        x.montgomeryRepresentation(m);
    }
}

public static void BenchmarkMontgomeryMul(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = makeBenchmarkValue();
    var y = makeBenchmarkValue();
    var @out = makeBenchmarkValue();
    var m = makeBenchmarkModulus();
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        @out.montgomeryMul(x, y, m);
    }
}

public static void BenchmarkModMul(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = makeBenchmarkValue();
    var y = makeBenchmarkValue();
    var m = makeBenchmarkModulus();
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        x.Mul(y, m);
    }
}

public static void BenchmarkExpBig(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var @out = @new<bigꓸInt>();
    var exponentBytes = makeBenchmarkExponent();
    var x = @new<bigꓸInt>().SetBytes(exponentBytes);
    var e = @new<bigꓸInt>().SetBytes(exponentBytes);
    var n = @new<bigꓸInt>().SetBytes(exponentBytes);
    var one = @new<bigꓸInt>().SetUint64(1);
    n.Add(n, one);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        @out.Exp(x, e, n);
    }
}

public static void BenchmarkExp(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = makeBenchmarkValue();
    var e = makeBenchmarkExponent();
    var @out = makeBenchmarkValue();
    var m = makeBenchmarkModulus();
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        @out.Exp(x, e, m);
    }
}

public static void TestNewModulus(ж<testing.T> Ꮡt) {
    @string expected = modulusMustBe1ˢ;
    var (_, err) = NewModulus(new byte[]{}.slice());
    if (err == default! || err.Error() != expected) {
        Ꮡt.Errorf("NewModulus(0) got %q, want %q"u8, err, expected);
    }
    (_, err) = NewModulus(new byte[]{0}.slice());
    if (err == default! || err.Error() != expected) {
        Ꮡt.Errorf("NewModulus(0) got %q, want %q"u8, err, expected);
    }
    (_, err) = NewModulus(new byte[]{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}.slice());
    if (err == default! || err.Error() != expected) {
        Ꮡt.Errorf("NewModulus(0) got %q, want %q"u8, err, expected);
    }
    (_, err) = NewModulus(new byte[]{1}.slice());
    if (err == default! || err.Error() != expected) {
        Ꮡt.Errorf("NewModulus(1) got %q, want %q"u8, err, expected);
    }
    (_, err) = NewModulus(new byte[]{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1}.slice());
    if (err == default! || err.Error() != expected) {
        Ꮡt.Errorf("NewModulus(1) got %q, want %q"u8, err, expected);
    }
}

internal static slice<nuint> makeTestValue(nint nbits) {
    nint n = nbits / (nint)_W;
    var x = new slice<nuint>(n);
    foreach (var i in range(n)) {
        x[i]--;
    }
    return x;
}

internal partial struct TestAddMulVVWSized_tests /*dyn*/ {
    internal nint n;
    internal Func<ж<nuint>, ж<nuint>, nuint, nuint> f;
}

public static void TestAddMulVVWSized(ж<testing.T> Ꮡt) {
    // Sized addMulVVW have architecture-specific implementations on
    // a number of architectures. Test that they match the generic
    // implementation.
    var tests = new TestAddMulVVWSized_tests[]{
        new(1024, addMulVVW1024),
        new(1536, addMulVVW1536),
        new(2048, addMulVVW2048)
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var test = ref heap(new TestAddMulVVWSized_tests(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        Ꮡt.Run(fmt.Sprint(test.n), (ж<testing.T> tΔ1) => {
            var x = makeTestValue(testʗ1.n);
            var z = makeTestValue(testʗ1.n);
            var z2 = slices.Clone<slice<nuint>, nuint>(z);
            nuint y = default!;
            y--;
            nuint c = addMulVVW(z, x, y);
            nuint c2 = testʗ1.f(Ꮡ(z2, 0), Ꮡ(x, 0), y);
            if (!slices.Equal<slice<nuint>, nuint>(z, z2) || c != c2) {
                tΔ1.Errorf("%016X, %016X != %016X, %016X"u8, z, c, z2, c2);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataModInvTestsTxtˢ = "testdata/mod_inv_tests.txt"u8;
internal static readonly object modulus1ˢ = (@string)"modulus <= 1"u8;
internal static readonly object notInvertibleˢ = (@string)"not invertible"u8;

public static void TestInverse(ж<testing.T> Ꮡt) {
    var (f, err) = os.Open(testdataModInvTestsTxtˢ);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    @string ModInv = default!;
    @string A = default!;
    @string M = default!;
    nint lineNum = default!;
    var scanner = bufio.NewScanner(new bigmod_internal_test_package.os_FileжReader(f));
    while (scanner.Scan()) {
        lineNum++;
        @string line = scanner.Text();
        if (len(line) == 0 || line[0] == (rune)'#') {
            continue;
        }
        var (k, v, _) = strings.Cut(line, " = "u8);
        var exprᴛ1 = k;
        if (exprᴛ1 == "ModInv"u8) {
            ModInv = v;
        }
        else if (exprᴛ1 == "A"u8) {
            A = v;
        }
        else if (exprᴛ1 == "M"u8) {
            M = v;
            Ꮡt.Run(fmt.Sprintf("line %d"u8, lineNum), (ж<testing.T> tΔ2) => {
                var (m, errΔ2) = NewModulus(decodeHex(tΔ2, M));
                if (errΔ2 != default!) {
                    tΔ2.Skip(modulus1ˢ);
                }
                (var a, errΔ2) = NewNat().SetBytes(decodeHex(tΔ2, A), m);
                if (errΔ2 != default!) {
                    tΔ2.Fatal(errΔ2);
                }
                var (got, ok) = NewNat().InverseVarTime(a, m);
                if (!ok) {
                    tΔ2.Fatal(notInvertibleˢ);
                }
                (var exp, errΔ2) = NewNat().SetBytes(decodeHex(tΔ2, ModInv), m);
                if (errΔ2 != default!) {
                    tΔ2.Fatal(errΔ2);
                }
                if (got.Equal(exp) != 1) {
                    tΔ2.Errorf("%v != %v"u8, got.OrTypedNil(), exp.OrTypedNil());
                }
            });
        }
        else { /* default: */
            Ꮡt.Fatalf("unknown key %q on line %d"u8, k, lineNum);
        }

    }
    {
        var errΔ3 = scanner.Err(); if (errΔ3 != default!) {
            Ꮡt.Fatal(errΔ3);
        }
    }
}

internal static slice<byte> decodeHex(ж<testing.T> Ꮡt, @string s) {
    Ꮡt.Helper();
    if (len(s) % 2 != 0) {
        s = "0"u8 + s;
    }
    var (b, err) = hex.DecodeString(s);
    if (err != default!) {
        Ꮡt.Fatalf("failed to decode hex %q: %v"u8, s, err);
    }
    return b;
}

} // end bigmod_internal_test_package
