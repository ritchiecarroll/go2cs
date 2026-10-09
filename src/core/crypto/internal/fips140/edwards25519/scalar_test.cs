// Copyright (c) 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140;

using bytes = bytes_package;
using hex = encoding.hex_package;
using big = math.big_package;
using mathrand = math.rand_package;
using reflect = reflect_package;
using testing = testing_package;
using quick = go.testing.quick_package;
using encoding;
using go.testing;
using math;
using rand = math.rand_package;
using static go.crypto.@internal.fips140.edwards25519_package;

partial class edwards25519_internal_test_package {

// quickCheckConfig returns a quick.Config that scales the max count by the
// given factor if the -short flag is not set.
internal static ж<quick.Config> quickCheckConfig(nint slowScale) {
    var cfg = @new<quick.Config>();
    if (!testing.Short()) {
        cfg.Value.MaxCountScale = (float64)slowScale;
    }
    return cfg;
}

internal static array<byte> scOneBytes = new byte[]{1}.array(32);

internal static ж<global::go.crypto.@internal.fips140.edwards25519_package.Scalar> scOne;
internal static error _ᴛ1ʗ;
internal static void initᴛscOne() { scOne = @new<global::go.crypto.@internal.fips140.edwards25519_package.Scalar>().SetCanonicalBytes(scOneBytes[..]).Item1; }

internal static ж<global::go.crypto.@internal.fips140.edwards25519_package.Scalar> scMinusOne;
internal static error _ᴛ2ʗ;
internal static void initᴛscMinusOne() { scMinusOne = @new<global::go.crypto.@internal.fips140.edwards25519_package.Scalar>().SetCanonicalBytes(scalarMinusOneBytes[..]).Item1; }

// Generate returns a valid (reduced modulo l) Scalar with a distribution
// weighted towards high, low, and edge values.
internal static reflectꓸValue Generate(this global::go.crypto.@internal.fips140.edwards25519_package.Scalar _, ж<mathrand.Rand> Ꮡrand, nint size) {
    ref var rand = ref Ꮡrand.DerefOrNull();

    ref var s = ref heap(new array<byte>(32), out var Ꮡs);
    nint diceRoll = rand.Intn(100);
    switch (ᐧ) {
    case {} when diceRoll is 0: {
        break;
    }
    case {} when diceRoll is 1: {
        s = scOneBytes.Clone();
        break;
    }
    case {} when diceRoll is 2: {
        s = scalarMinusOneBytes.Clone();
        break;
    }
    case {} when diceRoll is < 5: {
        Ꮡrand.Read(s[..16]);
        s[15] &= (byte)(((1 << (int)(5))) - 1);
        break;
    }
    case {} when diceRoll is < 10: {
        s[31] = (byte)(1 << (int)(4));
        Ꮡrand.Read(s[..16]);
        s[15] &= (byte)(((1 << (int)(4))) - 1);
        break;
    }
    default: {
        Ꮡrand.Read(s[..]);
        s[31] &= (byte)(((1 << (int)(4))) - 1);
        break;
    }}

    // Generate a low scalar in [0, 2^125).
    // Generate a high scalar in [2^252, 2^252 + 2^124).
    // Generate a valid scalar in [0, l) by returning [0, 2^252) which has a
    // negligibly different distribution (the former has a 2^-127.6 chance
    // of being out of the latter range).
    ref var val = ref heap<global::go.crypto.@internal.fips140.edwards25519_package.Scalar>(out var Ꮡval);
    val = new Scalar(nil);
    var ᴛ1 = val.s.Value;
    fiatScalarFromBytes(ref ᴛ1, Ꮡs);
    var ᴛ2 = (global::go.crypto.@internal.fips140.edwards25519_package.fiatScalarNonMontgomeryDomainFieldElement)((val.s).Value);
    fiatScalarToMontgomery(ref val.s, ref ᴛ2);
    return reflect.ValueOf(val);
}

public static void TestScalarGenerate(ж<testing.T> Ꮡt) {
    var f = (global::go.crypto.@internal.fips140.edwards25519_package.Scalar scʗp) => {
        ref var sc = ref heap(scʗp.ΔClone(), out var Ꮡsc);
        return isReduced(Ꮡsc.Bytes());
    };
    {
        var err = quick.Check((f).OrTypedNilFunc(), quickCheckConfig(1024)); if (err != default!) {
            Ꮡt.Errorf("generated unreduced scalar: %v"u8, err);
        }
    }
}

public static void TestScalarSetCanonicalBytes(ж<testing.T> Ꮡt) {
    var f1 = ([GoArrayDims(32)] array<byte> @in, global::go.crypto.@internal.fips140.edwards25519_package.Scalar scʗp) => {
        @in = @in.Clone();
        ref var sc = ref heap(scʗp.ΔClone(), out var Ꮡsc);
        // Mask out top 4 bits to guarantee value falls in [0, l).
        @in[len(@in) - 1] &= (byte)(((1 << (int)(4))) - 1);
        {
            var (_, err) = Ꮡsc.SetCanonicalBytes(@in[..]); if (err != default!) {
                return false;
            }
        }
        var repr = Ꮡsc.Bytes();
        return bytes_package.Equal(@in[..], repr) && isReduced(repr);
    };
    {
        var err = quick.Check((f1).OrTypedNilFunc(), quickCheckConfig(1024)); if (err != default!) {
            Ꮡt.Errorf("failed bytes->scalar->bytes round-trip: %v"u8, err);
        }
    }
    var f2 = (global::go.crypto.@internal.fips140.edwards25519_package.Scalar sc1ʗp, global::go.crypto.@internal.fips140.edwards25519_package.Scalar sc2ʗp) => {
        ref var sc1 = ref heap(sc1ʗp.ΔClone(), out var Ꮡsc1);
        ref var sc2 = ref heap(sc2ʗp.ΔClone(), out var Ꮡsc2);
        {
            var (_, err) = Ꮡsc2.SetCanonicalBytes(Ꮡsc1.Bytes()); if (err != default!) {
                return false;
            }
        }
        return sc1 == sc2;
    };
    {
        var err = quick.Check((f2).OrTypedNilFunc(), quickCheckConfig(1024)); if (err != default!) {
            Ꮡt.Errorf("failed scalar->bytes->scalar round-trip: %v"u8, err);
        }
    }
    var b = scalarMinusOneBytes.Clone();
    b[31] += 1;
    var s = scOne;
    {
        var (@out, err) = s.SetCanonicalBytes(b[..]); if (err == default!){
            Ꮡt.Errorf("SetCanonicalBytes worked on a non-canonical value"u8);
        } else 
        if (s != scOne){
            Ꮡt.Errorf("SetCanonicalBytes modified its receiver"u8);
        } else 
        if (@out != nil) {
            Ꮡt.Errorf("SetCanonicalBytes did not return nil with an error"u8);
        }
    }
}

public static void TestScalarSetUniformBytes(ж<testing.T> Ꮡt) {
    var (mod, _) = @new<bigꓸInt>().SetString("27742317777372353535851937790883648493"u8, 10);
    mod.Add(mod, @new<bigꓸInt>().Lsh(big.NewInt(1), 252));
    var modʗ1 = mod;
    var f = ([GoArrayDims(64)] array<byte> @in, global::go.crypto.@internal.fips140.edwards25519_package.Scalar scʗp) => {
        @in = @in.Clone();
        ref var sc = ref heap(scʗp.ΔClone(), out var Ꮡsc);
        Ꮡsc.SetUniformBytes(@in[..]);
        var repr = Ꮡsc.Bytes();
        if (!isReduced(repr)) {
            return false;
        }
        var scBig = bigIntFromLittleEndianBytes(repr[..]);
        var inBig = bigIntFromLittleEndianBytes(@in[..]);
        return inBig.Mod(inBig, modʗ1).Cmp(scBig) == 0;
    };
    {
        var err = quick.Check((f).OrTypedNilFunc(), quickCheckConfig(1024)); if (err != default!) {
            Ꮡt.Error(err);
        }
    }
}

public static void TestScalarSetBytesWithClamping(ж<testing.T> Ꮡt) {
    // Generated with libsodium.js 1.0.18 crypto_scalarmult_ed25519_base.
    @string random = "633d368491364dc9cd4c1bf891b1d59460face1644813240a313e61f2c88216e"u8;
    var (s, _) = @new<global::go.crypto.@internal.fips140.edwards25519_package.Scalar>().SetBytesWithClamping(decodeHex(random));
    var p = @new<global::go.crypto.@internal.fips140.edwards25519_package.Point>().ScalarBaseMult(s);
    @string want = "1d87a9026fd0126a5736fe1628c95dd419172b5b618457e041c9c861b2494a94"u8;
    {
        @string got = hex.EncodeToString(p.Bytes()); if (got != want) {
            Ꮡt.Errorf("random: got %q, want %q"u8, got, want);
        }
    }
    @string zero = "0000000000000000000000000000000000000000000000000000000000000000"u8;
    (s, _) = @new<global::go.crypto.@internal.fips140.edwards25519_package.Scalar>().SetBytesWithClamping(decodeHex(zero));
    p = @new<global::go.crypto.@internal.fips140.edwards25519_package.Point>().ScalarBaseMult(s);
    want = "693e47972caf527c7883ad1b39822f026f47db2ab0e1919955b8993aa04411d1"u8;
    {
        @string got = hex.EncodeToString(p.Bytes()); if (got != want) {
            Ꮡt.Errorf("zero: got %q, want %q"u8, got, want);
        }
    }
    @string one = "ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff"u8;
    (s, _) = @new<global::go.crypto.@internal.fips140.edwards25519_package.Scalar>().SetBytesWithClamping(decodeHex(one));
    p = @new<global::go.crypto.@internal.fips140.edwards25519_package.Point>().ScalarBaseMult(s);
    want = "12e9a68b73fd5aacdbcaf3e88c46fea6ebedb1aa84eed1842f07f8edab65e3a7"u8;
    {
        @string got = hex.EncodeToString(p.Bytes()); if (got != want) {
            Ꮡt.Errorf("one: got %q, want %q"u8, got, want);
        }
    }
}

internal static ж<bigꓸInt> bigIntFromLittleEndianBytes(slice<byte> b) {
    var bb = new slice<byte>(len(b));
    foreach (var (i, _) in b) {
        bb[i] = b[len(b) - i - 1];
    }
    return @new<bigꓸInt>().SetBytes(bb);
}

public static void TestScalarMultiplyDistributesOverAdd(ж<testing.T> Ꮡt) {
    var multiplyDistributesOverAdd = (global::go.crypto.@internal.fips140.edwards25519_package.Scalar xʗp, global::go.crypto.@internal.fips140.edwards25519_package.Scalar yʗp, global::go.crypto.@internal.fips140.edwards25519_package.Scalar zʗp) => {
        ref var x = ref heap(xʗp.ΔClone(), out var Ꮡx);
        ref var y = ref heap(yʗp.ΔClone(), out var Ꮡy);
        ref var z = ref heap(zʗp.ΔClone(), out var Ꮡz);
        // Compute t1 = (x+y)*z
        ref var t1 = ref heap(new global::go.crypto.@internal.fips140.edwards25519_package.Scalar(), out var Ꮡt1);
        Ꮡt1.Add(Ꮡx, Ꮡy);
        Ꮡt1.Multiply(Ꮡt1, Ꮡz);
        // Compute t2 = x*z + y*z
        ref var t2 = ref heap(new global::go.crypto.@internal.fips140.edwards25519_package.Scalar(), out var Ꮡt2);
        ref var t3 = ref heap(new global::go.crypto.@internal.fips140.edwards25519_package.Scalar(), out var Ꮡt3);
        Ꮡt2.Multiply(Ꮡx, Ꮡz);
        Ꮡt3.Multiply(Ꮡy, Ꮡz);
        Ꮡt2.Add(Ꮡt2, Ꮡt3);
        var (reprT1, reprT2) = (Ꮡt1.Bytes(), Ꮡt2.Bytes());
        return t1 == t2 && isReduced(reprT1) && isReduced(reprT2);
    };
    {
        var err = quick.Check((multiplyDistributesOverAdd).OrTypedNilFunc(), quickCheckConfig(1024)); if (err != default!) {
            Ꮡt.Error(err);
        }
    }
}

public static void TestScalarAddLikeSubNeg(ж<testing.T> Ꮡt) {
    var addLikeSubNeg = (global::go.crypto.@internal.fips140.edwards25519_package.Scalar xʗp, global::go.crypto.@internal.fips140.edwards25519_package.Scalar yʗp) => {
        ref var x = ref heap(xʗp.ΔClone(), out var Ꮡx);
        ref var y = ref heap(yʗp.ΔClone(), out var Ꮡy);
        // Compute t1 = x - y
        ref var t1 = ref heap(new global::go.crypto.@internal.fips140.edwards25519_package.Scalar(), out var Ꮡt1);
        Ꮡt1.Subtract(Ꮡx, Ꮡy);
        // Compute t2 = -y + x
        ref var t2 = ref heap(new global::go.crypto.@internal.fips140.edwards25519_package.Scalar(), out var Ꮡt2);
        Ꮡt2.Negate(Ꮡy);
        Ꮡt2.Add(Ꮡt2, Ꮡx);
        return t1 == t2 && isReduced(Ꮡt1.Bytes());
    };
    {
        var err = quick.Check((addLikeSubNeg).OrTypedNilFunc(), quickCheckConfig(1024)); if (err != default!) {
            Ꮡt.Error(err);
        }
    }
}

public static void TestScalarNonAdjacentForm(ж<testing.T> Ꮡt) {
    var (s, _) = (Ꮡ(new Scalar(nil))).SetCanonicalBytes(new byte[]{
        0x1a, 0x0e, 0x97, 0x8a, 0x90, 0xf6, 0x62, 0x2d,
        0x37, 0x47, 0x02, 0x3f, 0x8a, 0xd8, 0x26, 0x4d,
        0xa7, 0x58, 0xaa, 0x1b, 0x88, 0xe0, 0x40, 0xd1,
        0x58, 0x9e, 0x7b, 0x7f, 0x23, 0x76, 0xef, 0x09
    }.slice());
    var expectedNaf = new int8[]{
        0, 13, 0, 0, 0, 0, 0, 0, 0, 7, 0, 0, 0, 0, 0, 0, (int8)(-9), 0, 0, 0, 0, (int8)(-11), 0, 0, 0, 0, 3, 0, 0, 0, 0, 1,
        0, 0, 0, 0, 9, 0, 0, 0, 0, (int8)(-5), 0, 0, 0, 0, 0, 0, 3, 0, 0, 0, 0, 11, 0, 0, 0, 0, 11, 0, 0, 0, 0, 0,
        (int8)(-9), 0, 0, 0, 0, 0, (int8)(-3), 0, 0, 0, 0, 9, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, (int8)(-1), 0, 0, 0, 0, 0, 9, 0,
        0, 0, 0, (int8)(-15), 0, 0, 0, 0, (int8)(-7), 0, 0, 0, 0, (int8)(-9), 0, 0, 0, 0, 0, 5, 0, 0, 0, 0, 13, 0, 0, 0, 0, 0, (int8)(-3), 0,
        0, 0, 0, (int8)(-11), 0, 0, 0, 0, (int8)(-7), 0, 0, 0, 0, (int8)(-13), 0, 0, 0, 0, 11, 0, 0, 0, 0, (int8)(-9), 0, 0, 0, 0, 0, 1, 0, 0,
        0, 0, 0, (int8)(-15), 0, 0, 0, 0, 1, 0, 0, 0, 0, 7, 0, 0, 0, 0, 0, 0, 0, 0, 5, 0, 0, 0, 0, 0, 13, 0, 0, 0,
        0, 0, 0, 11, 0, 0, 0, 0, 0, 15, 0, 0, 0, 0, 0, (int8)(-9), 0, 0, 0, 0, 0, 0, 0, (int8)(-1), 0, 0, 0, 0, 0, 0, 0, 7,
        0, 0, 0, 0, 0, (int8)(-15), 0, 0, 0, 0, 0, 15, 0, 0, 0, 0, 15, 0, 0, 0, 0, 15, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0
    }.array();
    var sNaf = s.nonAdjacentForm(5);
    for (nint i = 0; i < 256; i++) {
        if (expectedNaf[i] != sNaf[i]) {
            Ꮡt.Errorf("Wrong digit at position %d, got %d, expected %d"u8, i, sNaf[i], expectedNaf[i]);
        }
    }
}

internal partial struct notZeroScalar /*global::go.crypto.@internal.fips140.edwards25519_package.Scalar*/;

internal static reflectꓸValue Generate(this notZeroScalar _, ж<mathrand.Rand> Ꮡrand, nint size) {
    ref var s = ref heap(new global::go.crypto.@internal.fips140.edwards25519_package.Scalar(), out var Ꮡs);
    ref var isNonZero = ref heap(new uint64(), out var ᏑisNonZero);
    while (isNonZero == 0) {
        s = new Scalar(nil).Generate(Ꮡrand, size).Interface()._<Scalar>();
        var ᴛ3 = s.s.Value;
        fiatScalarNonzero(ref isNonZero, ref ᴛ3);
    }
    return reflect.ValueOf(((notZeroScalar)s));
}

public static void TestScalarEqual(ж<testing.T> Ꮡt) {
    if (scOne.Equal(scMinusOne) == 1) {
        Ꮡt.Errorf("scOne.Equal(&scMinusOne) is true"u8);
    }
    if (scMinusOne.Equal(scMinusOne) == 0) {
        Ꮡt.Errorf("scMinusOne.Equal(&scMinusOne) is false"u8);
    }
}

} // end edwards25519_internal_test_package
