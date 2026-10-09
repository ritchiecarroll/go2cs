// Copyright (c) 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140.edwards25519;

using bytes = bytes_package;
using rand = go.crypto.rand_package;
using hex = encoding.hex_package;
using io = io_package;
using big = math.big_package;
using bits = math.bits_package;
using mathrand = math.rand_package;
using reflect = reflect_package;
using testing = testing_package;
using quick = go.testing.quick_package;
using encoding;
using go.crypto;
using go.testing;
using math;
using static go.crypto.@internal.fips140.edwards25519.field_package;

partial class field_internal_test_package {

internal static @string String(this global::go.crypto.@internal.fips140.edwards25519.field_package.Element v) {
    return hex.EncodeToString(v.Bytes());
}

// quickCheckConfig returns a quick.Config that scales the max count by the
// given factor if the -short flag is not set.
internal static ж<quick.Config> quickCheckConfig(nint slowScale) {
    var cfg = @new<quick.Config>();
    if (!testing.Short()) {
        cfg.Value.MaxCountScale = (float64)slowScale;
    }
    return cfg;
}

internal static global::go.crypto.@internal.fips140.edwards25519.field_package.Element generateFieldElement(ж<mathrand.Rand> Ꮡrand) {
    ref var rand = ref Ꮡrand.DerefOrNull();

    const uint64 maskLow52Bits = /* (1 << 52) - 1 */ 4503599627370495;
    return new Element(
        (uint64)(rand.Uint64() & maskLow52Bits),
        (uint64)(rand.Uint64() & maskLow52Bits),
        (uint64)(rand.Uint64() & maskLow52Bits),
        (uint64)(rand.Uint64() & maskLow52Bits),
        (uint64)(rand.Uint64() & maskLow52Bits)
    );
}

// weirdLimbs can be combined to generate a range of edge-case field elements.
// 0 and -1 are intentionally more weighted, as they combine well.
internal static slice<uint64> weirdLimbs51 = new uint64[]{
    0, 0, 0, 0,
    1,
    19 - 1,
    19,
    0x2aaaaaaaaaaaaUL,
    0x5555555555555UL,
    (uint64)((2251799813685248L) - 20),
    (uint64)((2251799813685248L) - 19),
    (uint64)((2251799813685248L) - 1), (uint64)((2251799813685248L) - 1),
    (uint64)((2251799813685248L) - 1), (uint64)((2251799813685248L) - 1)
}.slice();

internal static slice<uint64> weirdLimbs52 = new uint64[]{
    0, 0, 0, 0, 0, 0,
    1,
    19 - 1,
    19,
    0x2aaaaaaaaaaaaUL,
    0x5555555555555UL,
    (uint64)((2251799813685248L) - 20),
    (uint64)((2251799813685248L) - 19),
    (uint64)((2251799813685248L) - 1), (uint64)((2251799813685248L) - 1),
    (uint64)((2251799813685248L) - 1), (uint64)((2251799813685248L) - 1),
    (uint64)((2251799813685248L) - 1), (uint64)((2251799813685248L) - 1),
    ((uint64)1 << (int)(51)),
    (uint64)((2251799813685248L) + 1),
    (uint64)((4503599627370496L) - 19),
    (uint64)((4503599627370496L) - 1)
}.slice();

internal static global::go.crypto.@internal.fips140.edwards25519.field_package.Element generateWeirdFieldElement(ж<mathrand.Rand> Ꮡrand) {
    ref var rand = ref Ꮡrand.DerefOrNull();

    return new Element(
        weirdLimbs52[rand.Intn(len(weirdLimbs52))],
        weirdLimbs51[rand.Intn(len(weirdLimbs51))],
        weirdLimbs51[rand.Intn(len(weirdLimbs51))],
        weirdLimbs51[rand.Intn(len(weirdLimbs51))],
        weirdLimbs51[rand.Intn(len(weirdLimbs51))]
    );
}

internal static reflectꓸValue Generate(this global::go.crypto.@internal.fips140.edwards25519.field_package.Element _, ж<mathrand.Rand> Ꮡrand, nint size) {
    ref var rand = ref Ꮡrand.DerefOrNull();

    if (rand.Intn(2) == 0) {
        return reflect.ValueOf(generateWeirdFieldElement(Ꮡrand));
    }
    return reflect.ValueOf(generateFieldElement(Ꮡrand));
}

// isInBounds returns whether the element is within the expected bit size bounds
// after a light reduction.
internal static bool isInBounds(ж<global::go.crypto.@internal.fips140.edwards25519.field_package.Element> Ꮡx) {
    ref var x = ref Ꮡx.DerefOrNull();

    return bits.Len64(x.l0) <= 52 && bits.Len64(x.l1) <= 52 && bits.Len64(x.l2) <= 52 && bits.Len64(x.l3) <= 52 && bits.Len64(x.l4) <= 52;
}

public static void TestMultiplyDistributesOverAdd(ж<testing.T> Ꮡt) {
    var multiplyDistributesOverAdd = (global::go.crypto.@internal.fips140.edwards25519.field_package.Element xʗp, global::go.crypto.@internal.fips140.edwards25519.field_package.Element yʗp, global::go.crypto.@internal.fips140.edwards25519.field_package.Element zʗp) => {
        ref var x = ref heap(xʗp, out var Ꮡx);
        ref var y = ref heap(yʗp, out var Ꮡy);
        ref var z = ref heap(zʗp, out var Ꮡz);
        // Compute t1 = (x+y)*z
        var t1 = @new<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>();
        t1.Add(Ꮡx, Ꮡy);
        t1.Multiply(t1, Ꮡz);
        // Compute t2 = x*z + y*z
        var t2 = @new<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>();
        var t3 = @new<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>();
        t2.Multiply(Ꮡx, Ꮡz);
        t3.Multiply(Ꮡy, Ꮡz);
        t2.Add(t2, t3);
        return t1.Equal(t2) == 1 && isInBounds(t1) && isInBounds(t2);
    };
    {
        var err = quick.Check((multiplyDistributesOverAdd).OrTypedNilFunc(), quickCheckConfig(1024)); if (err != default!) {
            Ꮡt.Error(err);
        }
    }
}

public static void TestMul64to128(ж<testing.T> Ꮡt) {
    var a = (uint64)5;
    var b = (uint64)5;
    var r = mul64(a, b);
    if (r.lo != 0x19 || r.hi != 0) {
        Ꮡt.Errorf("lo-range wide mult failed, got %d + %d*(2**64)"u8, r.lo, r.hi);
    }
    a = (uint64)18014398509481983UL; // 2^54 - 1
    b = (uint64)18014398509481983UL; // 2^54 - 1
    r = mul64(a, b);
    if (r.lo != 0xff80000000000001UL || r.hi != 0xfffffffffffUL) {
        Ꮡt.Errorf("hi-range wide mult failed, got %d + %d*(2**64)"u8, r.lo, r.hi);
    }
    a = (uint64)1125899906842661UL;
    b = (uint64)2097155;
    r = mul64(a, b);
    r = addMul64(r, a, b);
    r = addMul64(r, a, b);
    r = addMul64(r, a, b);
    r = addMul64(r, a, b);
    if (r.lo != 16888498990613035UL || r.hi != 640) {
        Ꮡt.Errorf("wrong answer: %d + %d*(2**64)"u8, r.lo, r.hi);
    }
}

// Check some fixed vectors from dalek
internal partial struct TestSetBytesRoundTrip_feRTTest /*dyn*/ {
    internal global::go.crypto.@internal.fips140.edwards25519.field_package.Element fe;
    internal slice<byte> b;
}

public static void TestSetBytesRoundTrip(ж<testing.T> Ꮡt) {
    var f1 = ([GoArrayDims(32)] array<byte> @in, global::go.crypto.@internal.fips140.edwards25519.field_package.Element feʗp) => {
        @in = @in.Clone();
        ref var fe = ref heap(feʗp, out var Ꮡfe);
        Ꮡfe.SetBytes(@in[..]);
        // Mask the most significant bit as it's ignored by SetBytes. (Now
        // instead of earlier so we check the masking in SetBytes is working.)
        @in[len(@in) - 1] &= (byte)(((1 << (int)(7))) - 1);
        return bytes_package.Equal(@in[..], fe.Bytes()) && isInBounds(Ꮡfe);
    };
    {
        var err = quick.Check((f1).OrTypedNilFunc(), nil); if (err != default!) {
            Ꮡt.Errorf("failed bytes->FE->bytes round-trip: %v"u8, err);
        }
    }
    var f2 = (global::go.crypto.@internal.fips140.edwards25519.field_package.Element feʗp, global::go.crypto.@internal.fips140.edwards25519.field_package.Element rʗp) => {
        ref var fe = ref heap(feʗp, out var Ꮡfe);
        ref var r = ref heap(rʗp, out var Ꮡr);
        Ꮡr.SetBytes(fe.Bytes());
        // Intentionally not using Equal not to go through Bytes again.
        // Calling reduce because both Generate and SetBytes can produce
        // non-canonical representations.
        Ꮡfe.reduce();
        Ꮡr.reduce();
        return fe == r;
    };
    {
        var err = quick.Check((f2).OrTypedNilFunc(), nil); if (err != default!) {
            Ꮡt.Errorf("failed FE->bytes->FE round-trip: %v"u8, err);
        }
    }
    slice<TestSetBytesRoundTrip_feRTTest> tests = new TestSetBytesRoundTrip_feRTTest[]{
        new(
            fe: new Element(358744748052810UL, 1691584618240980UL, 977650209285361UL, 1429865912637724UL, 560044844278676UL),
            b: new byte[]{74, 209, 69, 197, 70, 70, 161, 222, 56, 226, 229, 19, 112, 60, 25, 92, 187, 74, 222, 56, 50, 153, 51, 233, 40, 74, 57, 6, 160, 185, 213, 31}.slice()
        ),
        new(
            fe: new Element(84926274344903UL, 473620666599931UL, 365590438845504UL, 1028470286882429UL, 2146499180330972UL),
            b: new byte[]{199, 23, 106, 112, 61, 77, 216, 79, 186, 60, 11, 118, 13, 16, 103, 15, 42, 32, 83, 250, 44, 57, 204, 198, 78, 199, 253, 119, 146, 172, 3, 122}.slice()
        )
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestSetBytesRoundTrip_feRTTest(), out var Ꮡtt);
        tt = vᴛ1;

        var b = tt.fe.Bytes();
        var (fe, _) = @new<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>().SetBytes(tt.b);
        if (!bytes_package.Equal(b, tt.b) || fe.Equal(Ꮡtt.of(TestSetBytesRoundTrip_feRTTest.Ꮡfe)) != 1) {
            Ꮡt.Errorf("Failed fixed roundtrip: %v"u8, tt);
        }
    }
}

internal static slice<byte> swapEndianness(slice<byte> buf) {
    for (nint i = 0; i < len(buf) / 2; i++) {
        (buf[i], buf[len(buf) - i - 1]) = (buf[len(buf) - i - 1], buf[i]);
    }
    return buf;
}

public static void TestBytesBigEquivalence(ж<testing.T> Ꮡt) {
    var f1 = ([GoArrayDims(32)] array<byte> @in, global::go.crypto.@internal.fips140.edwards25519.field_package.Element feʗp, global::go.crypto.@internal.fips140.edwards25519.field_package.Element fe1ʗp) => {
        @in = @in.Clone();
        ref var fe = ref heap(feʗp, out var Ꮡfe);
        ref var fe1 = ref heap(fe1ʗp, out var Ꮡfe1);
        Ꮡfe.SetBytes(@in[..]);
        @in[len(@in) - 1] &= (byte)(((1 << (int)(7))) - 1); // mask the most significant bit
        var b = @new<bigꓸInt>().SetBytes(swapEndianness(@in[..]));
        Ꮡfe1.fromBig(b);
        if (fe != fe1) {
            return false;
        }
        var buf = new slice<byte>(32);
        buf = swapEndianness(fe1.toBig().FillBytes(buf));
        return bytes_package.Equal(fe.Bytes(), buf) && isInBounds(Ꮡfe) && isInBounds(Ꮡfe1);
    };
    {
        var err = quick.Check((f1).OrTypedNilFunc(), nil); if (err != default!) {
            Ꮡt.Error(err);
        }
    }
}

// fromBig sets v = n, and returns v. The bit length of n must not exceed 256.
internal static ж<global::go.crypto.@internal.fips140.edwards25519.field_package.Element> fromBig(this ж<global::go.crypto.@internal.fips140.edwards25519.field_package.Element> Ꮡv, ж<bigꓸInt> Ꮡn) {
    ref var n = ref Ꮡn.DerefOrNull();

    if (n.BitLen() > 32 * 8) {
        throw panic("edwards25519: invalid field element input size");
    }
    var buf = new slice<byte>(0, 32);
    foreach (var (_, vᴛ1) in n.Bits()) {
        var word = vᴛ1;

        for (nint i = 0; i < bits.UintSize; i += 8) {
            if (len(buf) >= cap(buf)) {
                break;
            }
            buf = append(buf, (byte)(nuint)word);
            word >>= (int)(8);
        }
    }
    Ꮡv.SetBytes(buf[..32]);
    return Ꮡv;
}

internal static ж<global::go.crypto.@internal.fips140.edwards25519.field_package.Element> fromDecimal(this ж<global::go.crypto.@internal.fips140.edwards25519.field_package.Element> Ꮡv, @string s) {
    var (n, ok) = @new<bigꓸInt>().SetString(s, 10);
    if (!ok) {
        throw panic("not a valid decimal: " + s);
    }
    return Ꮡv.fromBig(n);
}

// toBig returns v as a big.Int.
internal static ж<bigꓸInt> toBig(this ref global::go.crypto.@internal.fips140.edwards25519.field_package.Element v) {
    var buf = v.Bytes();
    var words = new slice<big.Word>(32 * 8 / bits.UintSize);
    foreach (var (n, _) in words) {
        for (nint i = 0; i < bits.UintSize; i += 8) {
            if (len(buf) == 0) {
                break;
            }
            words[n] |= (big.Word)((((big.Word)(nuint)buf[0]) << (int)(nuint)(((big.Word)(nuint)i))));
            buf = buf[1..];
        }
    }
    return @new<bigꓸInt>().SetBits(words);
}

public static void TestDecimalConstants(ж<testing.T> Ꮡt) {
    @string sqrtM1String = "19681161376707505956807079304988542015446066515923890162744021073123829784752"u8;
    {
        var exp = @new<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>().fromDecimal(sqrtM1String); if (sqrtM1.Equal(exp) != 1) {
            Ꮡt.Errorf("sqrtM1 is %v, expected %v"u8, sqrtM1.OrTypedNil(), exp.OrTypedNil());
        }
    }
}

// d is in the parent package, and we don't want to expose d or fromDecimal.
// dString := "37095705934669439343138083508754565189542113879843219016388785533085940283555"
// if exp := new(Element).fromDecimal(dString); d.Equal(exp) != 1 {
// 	t.Errorf("d is %v, expected %v", d, exp)
// }
public static void TestSetBytesRoundTripEdgeCases(ж<testing.T> Ꮡt) {
}

// TODO: values close to 0, close to 2^255-19, between 2^255-19 and 2^255-1,
// and between 2^255 and 2^256-1. Test both the documented SetBytes
// behavior, and that Bytes reduces them.

// Tests self-consistency between Multiply and Square.
public static void TestConsistency(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new global::go.crypto.@internal.fips140.edwards25519.field_package.Element(), out var Ꮡx);
    ref var x2 = ref heap(new global::go.crypto.@internal.fips140.edwards25519.field_package.Element(), out var Ꮡx2);
    ref var x2sq = ref heap(new global::go.crypto.@internal.fips140.edwards25519.field_package.Element(), out var Ꮡx2sq);
    x = new Element(1, 1, 1, 1, 1);
    Ꮡx2.Multiply(Ꮡx, Ꮡx);
    Ꮡx2sq.Square(Ꮡx);
    if (x2 != x2sq) {
        Ꮡt.Fatalf("all ones failed\nmul: %x\nsqr: %x\n"u8, x2, x2sq);
    }
    array<byte> bytes = new(32);
    var (_, err) = io.ReadFull(rand.Reader, bytes[..]);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    Ꮡx.SetBytes(bytes[..]);
    Ꮡx2.Multiply(Ꮡx, Ꮡx);
    Ꮡx2sq.Square(Ꮡx);
    if (x2 != x2sq) {
        Ꮡt.Fatalf("all ones failed\nmul: %x\nsqr: %x\n"u8, x2, x2sq);
    }
}

public static void TestEqual(ж<testing.T> Ꮡt) {
    ref var x = ref heap<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>(out var Ꮡx);
    x = new Element(1, 1, 1, 1, 1);
    ref var y = ref heap<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>(out var Ꮡy);
    y = new Element(5, 4, 3, 2, 1);
    nint eq = x.Equal(Ꮡx);
    if (eq != 1) {
        Ꮡt.Errorf("wrong about equality"u8);
    }
    eq = x.Equal(Ꮡy);
    if (eq != 0) {
        Ꮡt.Errorf("wrong about inequality"u8);
    }
}

public static void TestInvert(ж<testing.T> Ꮡt) {
    ref var x = ref heap<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>(out var Ꮡx);
    x = new Element(1, 1, 1, 1, 1);
    var one = new Element(1, 0, 0, 0, 0);
    ref var xinv = ref heap(new global::go.crypto.@internal.fips140.edwards25519.field_package.Element(), out var Ꮡxinv);
    ref var r = ref heap(new global::go.crypto.@internal.fips140.edwards25519.field_package.Element(), out var Ꮡr);
    Ꮡxinv.Invert(Ꮡx);
    Ꮡr.Multiply(Ꮡx, Ꮡxinv);
    Ꮡr.reduce();
    if (one != r) {
        Ꮡt.Errorf("inversion identity failed, got: %x"u8, r);
    }
    array<byte> bytes = new(32);
    var (_, err) = io.ReadFull(rand.Reader, bytes[..]);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    Ꮡx.SetBytes(bytes[..]);
    Ꮡxinv.Invert(Ꮡx);
    Ꮡr.Multiply(Ꮡx, Ꮡxinv);
    Ꮡr.reduce();
    if (one != r) {
        Ꮡt.Errorf("random inversion identity failed, got: %x for field element %x"u8, r, x);
    }
    ref var zero = ref heap<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>(out var Ꮡzero);
    zero = new Element(nil);
    Ꮡx.Set(Ꮡzero);
    {
        var xx = Ꮡxinv.Invert(Ꮡx); if (xx != Ꮡxinv){
            Ꮡt.Errorf("inverting zero did not return the receiver"u8);
        } else 
        if (xinv.Equal(Ꮡzero) != 1) {
            Ꮡt.Errorf("inverting zero did not return zero"u8);
        }
    }
}

public static void TestSelectSwap(ж<testing.T> Ꮡt) {
    ref var a = ref heap<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>(out var Ꮡa);
    a = new Element(358744748052810UL, 1691584618240980UL, 977650209285361UL, 1429865912637724UL, 560044844278676UL);
    ref var b = ref heap<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>(out var Ꮡb);
    b = new Element(84926274344903UL, 473620666599931UL, 365590438845504UL, 1028470286882429UL, 2146499180330972UL);
    ref var c = ref heap(new global::go.crypto.@internal.fips140.edwards25519.field_package.Element(), out var Ꮡc);
    ref var d = ref heap(new global::go.crypto.@internal.fips140.edwards25519.field_package.Element(), out var Ꮡd);
    Ꮡc.Select(Ꮡa, Ꮡb, 1);
    Ꮡd.Select(Ꮡa, Ꮡb, 0);
    if (c.Equal(Ꮡa) != 1 || d.Equal(Ꮡb) != 1) {
        Ꮡt.Errorf("Select failed"u8);
    }
    c.Swap(Ꮡd, 0);
    if (c.Equal(Ꮡa) != 1 || d.Equal(Ꮡb) != 1) {
        Ꮡt.Errorf("Swap failed"u8);
    }
    c.Swap(Ꮡd, 1);
    if (c.Equal(Ꮡb) != 1 || d.Equal(Ꮡa) != 1) {
        Ꮡt.Errorf("Swap failed"u8);
    }
}

public static void TestMult32(ж<testing.T> Ꮡt) {
    var mult32EquivalentToMul = (global::go.crypto.@internal.fips140.edwards25519.field_package.Element xʗp, uint32 y) => {
        ref var x = ref heap(xʗp, out var Ꮡx);
        var t1 = @new<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>();
        for (nint i = 0; i < 100; i++) {
            t1.Mult32(Ꮡx, y);
        }
        var ty = @new<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>();
        ty.Value.l0 = (uint64)y;
        var t2 = @new<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>();
        for (nint i = 0; i < 100; i++) {
            t2.Multiply(Ꮡx, ty);
        }
        return t1.Equal(t2) == 1 && isInBounds(t1) && isInBounds(t2);
    };
    {
        var err = quick.Check((mult32EquivalentToMul).OrTypedNilFunc(), quickCheckConfig(1024)); if (err != default!) {
            Ꮡt.Error(err);
        }
    }
}

// From draft-irtf-cfrg-ristretto255-decaf448-00, Appendix A.4.
internal partial struct TestSqrtRatio_test /*dyn*/ {
    internal slice<byte> u, v;
    internal nint wasSquare;
    internal slice<byte> r;
}

public static void TestSqrtRatio(ж<testing.T> Ꮡt) {
// If u is 0, the function is defined to return (0, TRUE), even if v
// is zero. Note that where used in this package, the denominator v
// is never zero.
// 0/1 == 0²
// If u is non-zero and v is zero, defined to return (0, FALSE).
// 2/1 is not square in this field.
// 4/1 == 2²
// 1/4 == (2⁻¹)² == (2^(p-2))² per Euler's theorem
    slice<TestSqrtRatio_test> tests = new TestSqrtRatio_test[]{
        new(
            decodeHex("0000000000000000000000000000000000000000000000000000000000000000"u8),
            decodeHex("0000000000000000000000000000000000000000000000000000000000000000"u8),
            1, decodeHex("0000000000000000000000000000000000000000000000000000000000000000"u8)
        ),
        new(
            decodeHex("0000000000000000000000000000000000000000000000000000000000000000"u8),
            decodeHex("0100000000000000000000000000000000000000000000000000000000000000"u8),
            1, decodeHex("0000000000000000000000000000000000000000000000000000000000000000"u8)
        ),
        new(
            decodeHex("0100000000000000000000000000000000000000000000000000000000000000"u8),
            decodeHex("0000000000000000000000000000000000000000000000000000000000000000"u8),
            0, decodeHex("0000000000000000000000000000000000000000000000000000000000000000"u8)
        ),
        new(
            decodeHex("0200000000000000000000000000000000000000000000000000000000000000"u8),
            decodeHex("0100000000000000000000000000000000000000000000000000000000000000"u8),
            0, decodeHex("3c5ff1b5d8e4113b871bd052f9e7bcd0582804c266ffb2d4f4203eb07fdb7c54"u8)
        ),
        new(
            decodeHex("0400000000000000000000000000000000000000000000000000000000000000"u8),
            decodeHex("0100000000000000000000000000000000000000000000000000000000000000"u8),
            1, decodeHex("0200000000000000000000000000000000000000000000000000000000000000"u8)
        ),
        new(
            decodeHex("0100000000000000000000000000000000000000000000000000000000000000"u8),
            decodeHex("0400000000000000000000000000000000000000000000000000000000000000"u8),
            1, decodeHex("f6ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff3f"u8)
        )
    }.slice();
    foreach (var (i, tt) in tests) {
        var (u, _) = @new<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>().SetBytes(tt.u);
        var (v, _) = @new<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>().SetBytes(tt.v);
        var (want, _) = @new<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>().SetBytes(tt.r);
        var (got, wasSquare) = @new<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>().SqrtRatio(u, v);
        if (got.Equal(want) == 0 || wasSquare != tt.wasSquare) {
            Ꮡt.Errorf("%d: got (%v, %v), want (%v, %v)"u8, i, got.OrTypedNil(), wasSquare, want.OrTypedNil(), tt.wasSquare);
        }
    }
}

public static void TestCarryPropagate(ж<testing.T> Ꮡt) {
    var asmLikeGeneric = ([GoArrayDims(5)] array<uint64> a) => {
        a = a.Clone();
        var t1 = Ꮡ(new Element(a[0], a[1], a[2], a[3], a[4]));
        var t2 = Ꮡ(new Element(a[0], a[1], a[2], a[3], a[4]));
        t1.carryPropagate();
        t2.carryPropagateGeneric();
        if (t1.Value != t2.Value) {
            Ꮡt.Logf("got: %#v,\nexpected: %#v"u8, t1.OrTypedNil(), t2.OrTypedNil());
        }
        return t1.Value == t2.Value && isInBounds(t2);
    };
    {
        var err = quick.Check((asmLikeGeneric).OrTypedNilFunc(), quickCheckConfig(1024)); if (err != default!) {
            Ꮡt.Error(err);
        }
    }
    if (!asmLikeGeneric(new uint64[]{0xffffffffffffffffUL, 0xffffffffffffffffUL, 0xffffffffffffffffUL, 0xffffffffffffffffUL, 0xffffffffffffffffUL}.array())) {
        Ꮡt.Errorf("failed for {0xffffffffffffffff, 0xffffffffffffffff, 0xffffffffffffffff, 0xffffffffffffffff, 0xffffffffffffffff}"u8);
    }
}

public static void TestFeSquare(ж<testing.T> Ꮡt) {
    var asmLikeGeneric = (global::go.crypto.@internal.fips140.edwards25519.field_package.Element a) => {
        ref var t1 = ref heap<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>(out var Ꮡt1);
        t1 = a;
        ref var t2 = ref heap<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>(out var Ꮡt2);
        t2 = a;
        feSquareGeneric(Ꮡt1, ref (Ꮡt1).DerefOrNull());
        feSquare(Ꮡt2, ref (Ꮡt2).DerefOrNull());
        if (t1 != t2) {
            Ꮡt.Logf("got: %#v,\nexpected: %#v"u8, t1, t2);
        }
        return t1 == t2 && isInBounds(Ꮡt2);
    };
    {
        var err = quick.Check((asmLikeGeneric).OrTypedNilFunc(), quickCheckConfig(1024)); if (err != default!) {
            Ꮡt.Error(err);
        }
    }
}

public static void TestFeMul(ж<testing.T> Ꮡt) {
    var asmLikeGeneric = (global::go.crypto.@internal.fips140.edwards25519.field_package.Element a, global::go.crypto.@internal.fips140.edwards25519.field_package.Element b) => {
        ref var a1 = ref heap<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>(out var Ꮡa1);
        a1 = a;
        ref var a2 = ref heap<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>(out var Ꮡa2);
        a2 = a;
        ref var b1 = ref heap<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>(out var Ꮡb1);
        b1 = b;
        ref var b2 = ref heap<global::go.crypto.@internal.fips140.edwards25519.field_package.Element>(out var Ꮡb2);
        b2 = b;
        feMulGeneric(Ꮡa1, ref (Ꮡa1).DerefOrNull(), ref (Ꮡb1).DerefOrNull());
        feMul(Ꮡa2, ref (Ꮡa2).DerefOrNull(), ref (Ꮡb2).DerefOrNull());
        if (a1 != a2 || b1 != b2) {
            Ꮡt.Logf("got: %#v,\nexpected: %#v"u8, a1, a2);
            Ꮡt.Logf("got: %#v,\nexpected: %#v"u8, b1, b2);
        }
        return a1 == a2 && isInBounds(Ꮡa2) && b1 == b2 && isInBounds(Ꮡb2);
    };
    {
        var err = quick.Check((asmLikeGeneric).OrTypedNilFunc(), quickCheckConfig(1024)); if (err != default!) {
            Ꮡt.Error(err);
        }
    }
}

internal static slice<byte> decodeHex(@string s) {
    var (b, err) = hex.DecodeString(s);
    if (err != default!) {
        throw panic(err);
    }
    return b;
}

} // end field_internal_test_package
