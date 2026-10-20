// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140;

using constanttime = go.crypto.@internal.constanttime_package;
using sha3 = go.crypto.@internal.fips140.sha3_package;
using errors = errors_package;
using bits = math.bits_package;
using go.crypto.@internal;
using go.crypto.@internal.fips140;
using math;

partial class mldsa_package {

internal static UntypedInt q => 8380417; // 2²³ - 2¹³ + 1
public static UntypedInt R => 4294967296; // 2³²
public static UntypedInt RR => 2365951; // R² mod q, aka R in the Montgomery domain
internal static UntypedInt qNegInv => 4236238847; // -q⁻¹ mod R (q * qNegInv ≡ -1 mod R)
internal static UntypedInt one => 4193792; // R mod q, aka 1 in the Montgomery domain
internal static UntypedInt minusOne => 4186625; // (q - 1) * R mod q, aka -1 in the Montgomery domain

// fieldElement is an element n of ℤ_q in the Montgomery domain, represented as
// an integer x in [0, q) such that x ≡ n * R (mod q) where R = 2³².
public partial struct fieldElement /*num:uint32*/;

internal static error errUnreducedFieldElement = errors.New("mldsa: unreduced field element"u8);

// fieldToMontgomery checks that a value a is < q, and converts it to
// Montgomery form.
internal static (fieldElement, error) fieldToMontgomery(uint32 a) {
    if (a >= q) {
        return (0, errUnreducedFieldElement);
    }
    // a * R² * R⁻¹ ≡ a * R (mod q)
    return (fieldMontgomeryMul((fieldElement)a, RR), default!);
}

// fieldSubToMontgomery converts a difference a - b to Montgomery form.
// a and b must be < q. (This bound can probably be relaxed.)
internal static fieldElement fieldSubToMontgomery(uint32 a, uint32 b) {
    var x = a - b + (uint32)q;
    return fieldMontgomeryMul((fieldElement)x, RR);
}

// fieldFromMontgomery converts a value a in Montgomery form back to
// standard representation.
internal static uint32 fieldFromMontgomery(fieldElement a) {
    // (a * R) * 1 * R⁻¹ ≡ a (mod q)
    return (uint32)fieldMontgomeryReduce((uint64)(uint32)a);
}

// fieldCenteredMod returns r mod± q, the value r reduced to the range
// [−(q−1)/2, (q−1)/2].
internal static int32 fieldCenteredMod(fieldElement r) {
    var x = (int32)fieldFromMontgomery(r);
    // x <= q / 2 ? x : x - q
    return constantTimeSelectLessOrEqual(x, q / 2, x, x - (int32)q);
}

// fieldInfinityNorm returns the infinity norm ||r||∞ of r, or the absolute
// value of r centered around 0.
internal static uint32 fieldInfinityNorm(fieldElement r) {
    var x = (int32)fieldFromMontgomery(r);
    // x <= q / 2 ? x : |x - q|
    // |x - q| = -(x - q) = q - x because x < q => x - q < 0
    return (uint32)constantTimeSelectLessOrEqual(x, q / 2, x, (int32)q - x);
}

// fieldReduceOnce reduces a value a < 2q.
internal static fieldElement fieldReduceOnce(uint32 a) {
    var (x, b) = bits.Sub64((uint64)a, (uint64)q, 0);
    return (fieldElement)(uint32)(x + b * (uint64)q);
}

// fieldAdd returns a + b mod q.
internal static fieldElement fieldAdd(fieldElement a, fieldElement b) {
    var x = (uint32)(a + b);
    return fieldReduceOnce(x);
}

// fieldSub returns a - b mod q.
internal static fieldElement fieldSub(fieldElement a, fieldElement b) {
    var x = (uint32)(a - b + (uint32)q);
    return fieldReduceOnce(x);
}

// fieldMontgomeryMul returns a * b * R⁻¹ mod q.
internal static fieldElement fieldMontgomeryMul(fieldElement a, fieldElement b) {
    var x = (uint64)(uint32)a * (uint64)(uint32)b;
    return fieldMontgomeryReduce(x);
}

// fieldMontgomeryReduce returns x * R⁻¹ mod q for x < q * R.
internal static fieldElement fieldMontgomeryReduce(uint64 x) {
    var t = (uint32)x * (uint32)qNegInv;
    var u = (x + (uint64)t * (uint64)q) >> 32;
    return fieldReduceOnce((uint32)u);
}

// fieldMontgomeryMulSub returns a * (b - c). This operation is fused to save a
// fieldReduceOnce after the subtraction.
internal static fieldElement fieldMontgomeryMulSub(fieldElement a, fieldElement b, fieldElement c) {
    var x = (uint64)(uint32)a * (uint64)(uint32)(b - c + (uint32)q);
    return fieldMontgomeryReduce(x);
}

// fieldMontgomeryAddMul returns a * b + c * d. This operation is fused to save
// a fieldReduceOnce and a fieldReduce.
internal static fieldElement fieldMontgomeryAddMul(fieldElement a, fieldElement b, fieldElement c, fieldElement d) {
    var x = (uint64)(uint32)a * (uint64)(uint32)b;
    x += (uint64)(uint32)c * (uint64)(uint32)d;
    return fieldMontgomeryReduce(x);
}

internal static UntypedInt n => 256;

// ringElement is a polynomial, an element of R_q.
/* [n]fieldElement */
partial struct ringElement /*[256]fieldElement*/;

// polyAdd adds two ringElements or nttElements.
internal static T /*s*/ polyAdd<T>(T a, T b)
    where T : /* ~[256]crypto/internal/fips140/mldsa.fieldElement */ IArray<fieldElement>, new()
{
    T s = GoZero<T>();

    foreach (var (i, _) in s) {
        s[i] = fieldAdd(a[i], b[i]);
    }
    return s;
}

// polySub subtracts two ringElements or nttElements.
internal static T /*s*/ polySub<T>(T a, T b)
    where T : /* ~[256]crypto/internal/fips140/mldsa.fieldElement */ IArray<fieldElement>, new()
{
    T s = GoZero<T>();

    foreach (var (i, _) in s) {
        s[i] = fieldSub(a[i], b[i]);
    }
    return s;
}

// nttElement is an NTT representation, an element of T_q.
/* [n]fieldElement */
partial struct nttElement /*[256]fieldElement*/;

// zetas are the values ζ^BitRev₈(k) mod q for each index k, converted to the
// Montgomery domain.
internal static array<fieldElement> zetas = new fieldElement[]{4193792, 25847, 5771523, 7861508, 237124, 7602457, 7504169, 466468, 1826347, 2353451, 8021166, 6288512, 3119733, 5495562, 3111497, 2680103, 2725464, 1024112, 7300517, 3585928, 7830929, 7260833, 2619752, 6271868, 6262231, 4520680, 6980856, 5102745, 1757237, 8360995, 4010497, 280005, 2706023, 95776, 3077325, 3530437, 6718724, 4788269, 5842901, 3915439, 4519302, 5336701, 3574422, 5512770, 3539968, 8079950, 2348700, 7841118, 6681150, 6736599, 3505694, 4558682, 3507263, 6239768, 6779997, 3699596, 811944, 531354, 954230, 3881043, 3900724, 5823537, 2071892, 5582638, 4450022, 6851714, 4702672, 5339162, 6927966, 3475950, 2176455, 6795196, 7122806, 1939314, 4296819, 7380215, 5190273, 5223087, 4747489, 126922, 3412210, 7396998, 2147896, 2715295, 5412772, 4686924, 7969390, 5903370, 7709315, 7151892, 8357436, 7072248, 7998430, 1349076, 1852771, 6949987, 5037034, 264944, 508951, 3097992, 44288, 7280319, 904516, 3958618, 4656075, 8371839, 1653064, 5130689, 2389356, 8169440, 759969, 7063561, 189548, 4827145, 3159746, 6529015, 5971092, 8202977, 1315589, 1341330, 1285669, 6795489, 7567685, 6940675, 5361315, 4499357, 4751448, 3839961, 2091667, 3407706, 2316500, 3817976, 5037939, 2244091, 5933984, 4817955, 266997, 2434439, 7144689, 3513181, 4860065, 4621053, 7183191, 5187039, 900702, 1859098, 909542, 819034, 495491, 6767243, 8337157, 7857917, 7725090, 5257975, 2031748, 3207046, 4823422, 7855319, 7611795, 4784579, 342297, 286988, 5942594, 4108315, 3437287, 5038140, 1735879, 203044, 2842341, 2691481, 5790267, 1265009, 4055324, 1247620, 2486353, 1595974, 4613401, 1250494, 2635921, 4832145, 5386378, 1869119, 1903435, 7329447, 7047359, 1237275, 5062207, 6950192, 7929317, 1312455, 3306115, 6417775, 7100756, 1917081, 5834105, 7005614, 1500165, 777191, 2235880, 3406031, 7838005, 5548557, 6709241, 6533464, 5796124, 4656147, 594136, 4603424, 6366809, 2432395, 2454455, 8215696, 1957272, 3369112, 185531, 7173032, 5196991, 162844, 1616392, 3014001, 810149, 1652634, 4686184, 6581310, 5341501, 3523897, 3866901, 269760, 2213111, 7404533, 1717735, 472078, 7953734, 1723600, 6577327, 1910376, 6712985, 7276084, 8119771, 4546524, 5441381, 6144432, 7959518, 6094090, 183443, 7403526, 1612842, 4834730, 7826001, 3919660, 8332111, 7018208, 3937738, 1400424, 7534263, 1976782}.array();

// ntt maps a ringElement to its nttElement representation.
//
// It implements NTT, according to FIPS 203, Algorithm 9.
internal static nttElement ntt(ringElement f) {
    f = f.Clone();

    uint8 m = default!;

    for (nint len = 128; len >= 8; len /= 2) {
        for (nint start = 0; start < 256; start += 2 * len) {
            m++;
            var zeta = zetas[m];

            // Bounds check elimination hint.
            var (fΔ1, flen) = (f.slice(start, start + len), f.slice(start + len, start + len + len));
            for (nint j = 0; j < len; j += 2) {
                var t = fieldMontgomeryMul(zeta, flen[j]);
                flen[j] = fieldSub(fΔ1[j], t);
                fΔ1[j] = fieldAdd(fΔ1[j], t);

                // Unroll by 2 for performance.
                t = fieldMontgomeryMul(zeta, flen[j + 1]);
                flen[j + 1] = fieldSub(fΔ1[j + 1], t);
                fΔ1[j + 1] = fieldAdd(fΔ1[j + 1], t);
            }
        }
    }

    // Unroll len = 4, 2, and 1.
    for (nint start = 0; start < 256; start += 8) {
        m++;
        var zeta = zetas[m];

        var t = fieldMontgomeryMul(zeta, f[start + 4]);
        f[start + 4] = fieldSub(f[start], t);
        f[start] = fieldAdd(f[start], t);

        t = fieldMontgomeryMul(zeta, f[start + 5]);
        f[start + 5] = fieldSub(f[start + 1], t);
        f[start + 1] = fieldAdd(f[start + 1], t);

        t = fieldMontgomeryMul(zeta, f[start + 6]);
        f[start + 6] = fieldSub(f[start + 2], t);
        f[start + 2] = fieldAdd(f[start + 2], t);

        t = fieldMontgomeryMul(zeta, f[start + 7]);
        f[start + 7] = fieldSub(f[start + 3], t);
        f[start + 3] = fieldAdd(f[start + 3], t);
    }
    for (nint start = 0; start < 256; start += 4) {
        m++;
        var zeta = zetas[m];

        var t = fieldMontgomeryMul(zeta, f[start + 2]);
        f[start + 2] = fieldSub(f[start], t);
        f[start] = fieldAdd(f[start], t);

        t = fieldMontgomeryMul(zeta, f[start + 3]);
        f[start + 3] = fieldSub(f[start + 1], t);
        f[start + 1] = fieldAdd(f[start + 1], t);
    }
    for (nint start = 0; start < 256; start += 2) {
        m++;
        var zeta = zetas[m];

        var t = fieldMontgomeryMul(zeta, f[start + 1]);
        f[start + 1] = fieldSub(f[start], t);
        f[start] = fieldAdd(f[start], t);
    }

    return (nttElement)(array<fieldElement>)f;
}

// inverseNTT maps a nttElement back to the ringElement it represents.
//
// It implements NTT⁻¹, according to FIPS 203, Algorithm 10.
internal static ringElement inverseNTT(nttElement f) {
    f = f.Clone();

    uint8 m = 255;

    // Unroll len = 1, 2, and 4.
    for (nint start = 0; start < 256; start += 2) {
        var zeta = zetas[m];
        m--;

        var t = f[start];
        f[start] = fieldAdd(t, f[start + 1]);
        f[start + 1] = fieldMontgomeryMulSub(zeta, f[start + 1], t);
    }
    for (nint start = 0; start < 256; start += 4) {
        var zeta = zetas[m];
        m--;

        var t = f[start];
        f[start] = fieldAdd(t, f[start + 2]);
        f[start + 2] = fieldMontgomeryMulSub(zeta, f[start + 2], t);

        t = f[start + 1];
        f[start + 1] = fieldAdd(t, f[start + 3]);
        f[start + 3] = fieldMontgomeryMulSub(zeta, f[start + 3], t);
    }
    for (nint start = 0; start < 256; start += 8) {
        var zeta = zetas[m];
        m--;

        var t = f[start];
        f[start] = fieldAdd(t, f[start + 4]);
        f[start + 4] = fieldMontgomeryMulSub(zeta, f[start + 4], t);

        t = f[start + 1];
        f[start + 1] = fieldAdd(t, f[start + 5]);
        f[start + 5] = fieldMontgomeryMulSub(zeta, f[start + 5], t);

        t = f[start + 2];
        f[start + 2] = fieldAdd(t, f[start + 6]);
        f[start + 6] = fieldMontgomeryMulSub(zeta, f[start + 6], t);

        t = f[start + 3];
        f[start + 3] = fieldAdd(t, f[start + 7]);
        f[start + 7] = fieldMontgomeryMulSub(zeta, f[start + 7], t);
    }

    for (nint len = 8; len < 256; len *= 2) {
        for (nint start = 0; start < 256; start += 2 * len) {
            var zeta = zetas[m];
            m--;

            // Bounds check elimination hint.
            var (fΔ1, flen) = (f.slice(start, start + len), f.slice(start + len, start + len + len));
            for (nint j = 0; j < len; j += 2) {
                var t = fΔ1[j];
                fΔ1[j] = fieldAdd(t, flen[j]);
                // -z * (t - flen[j]) = z * (flen[j] - t)
                flen[j] = fieldMontgomeryMulSub(zeta, flen[j], t);

                // Unroll by 2 for performance.
                t = fΔ1[j + 1];
                fΔ1[j + 1] = fieldAdd(t, flen[j + 1]);
                flen[j + 1] = fieldMontgomeryMulSub(zeta, flen[j + 1], t);
            }
        }
    }

    foreach (var (i, _) in f) {
        f[i] = fieldMontgomeryMul(f[i], 16382); // 16382 = 256⁻¹ * R mod q
    }
    return (ringElement)(array<fieldElement>)f;
}

// nttMul multiplies two nttElements.
internal static nttElement /*p*/ nttMul(nttElement a, nttElement b) {
    nttElement p = default!;
    a = a.Clone();
    b = b.Clone();

    foreach (var (i, _) in p) {
        p[i] = fieldMontgomeryMul(a[i], b[i]);
    }
    return p.Clone();
}

// sampleNTT samples an nttElement uniformly at random from the seed rho and the
// indices s and r. It implements Step 3 of ExpandA, RejNTTPoly, and
// CoeffFromThreeBytes from FIPS 204, passing in ρ, s, and r instead of ρ'.
internal static nttElement sampleNTT(slice<byte> rho, byte s, byte r) {
    var G = sha3.NewShake128();
    G.Write(rho);
    G.Write(new byte[]{s, r}.slice());

    nttElement a = default!;
    nint j = default!;               // index into a
    array<byte> buf = new(168);                 // buffered reads from B, matching the rate of SHAKE-128
    nint off = len(buf); // index into buf, starts in a "buffer fully consumed" state
    while (j < n) {
        if (off >= len(buf)) {
            G.Read(buf[..]);
            off = 0;
        }
        var v = (uint32)buf[off] | (uint32)buf[off + 1] << 8 | (uint32)buf[off + 2] << 16;
        off += 3;
        var (f, err) = fieldToMontgomery(v & 0b01111111_11111111_11111111); // 23 bits
        if (err != default!) {
            continue;
        }
        a[j] = f;
        j++;
    }
    return a.Clone();
}

// sampleBoundedPoly samples a ringElement with coefficients in [−η, η] from the
// seed rho and the index r. It implements RejBoundedPoly and CoeffFromHalfByte
// from FIPS 204, passing in ρ and r separately from ExpandS.
internal static ringElement sampleBoundedPoly(slice<byte> rho, byte r, parameters p) {
    var H = sha3.NewShake256();
    H.Write(rho);
    H.Write(new byte[]{r, 0}.slice()); // IntegerToBytes(r, 2)

    ringElement a = default!;
    nint j = default!;
    array<byte> buf = new(136);                 // buffered reads from H, matching the rate of SHAKE-256
    nint off = len(buf); // index into buf, starts in a "buffer fully consumed" state
    while (ᐧ) {
        if (off >= len(buf)) {
            H.Read(buf[..]);
            off = 0;
        }
        var z0 = (byte)(buf[off] & 0x0F);
        var z1 = (byte)((buf[off] >> 4));
        off++;
        var (coeff, ok) = coeffFromHalfByte(z0, p);
        if (ok) {
            a[j] = coeff;
            j++;
        }
        if (j >= len(a)) {
            break;
        }
        (coeff, ok) = coeffFromHalfByte(z1, p);
        if (ok) {
            a[j] = coeff;
            j++;
        }
        if (j >= len(a)) {
            break;
        }
    }
    return a.Clone();
}

// sampleInBall samples a ringElement with coefficients in {−1, 0, 1}, and τ
// non-zero coefficients. It is not constant-time.
internal static ringElement sampleInBall(slice<byte> rho, parameters p) {
    var H = sha3.NewShake256();
    H.Write(rho);
    var s = new slice<byte>(8);
    H.Read(s);

    ringElement c = default!;
    for (nint i = 256 - p.τ; i < 256; i++) {
        var j = new slice<byte>(1);
        H.Read(j);
        while (j[0] > (byte)i) {
            H.Read(j);
        }
        c[i] = c[j[0]];
        // c[j] = (−1) ^ h[i+τ−256], where h are the bits in s in little-endian.
        // That is, -1⁰ = 1 if the bit is 0, -1¹ = -1 if it is 1.
        nint bitIdx = i + p.τ - 256;
        var bit = (byte)((s[bitIdx / 8].Rsh((int64)(bitIdx % 8))) & 1);
        if (bit == 0){
            c[j[0]] = one;
        } else {
            c[j[0]] = minusOne;
        }
    }

    return c.Clone();
}

// coeffFromHalfByte implements CoeffFromHalfByte from FIPS 204.
//
// It maps a value in [0, 15] to a coefficient in [−η, η]
internal static (fieldElement, bool) coeffFromHalfByte(byte b, parameters p) {
    if (b > 15) {
        throw panic("internal error: half-byte out of range");
    }
    switch (p.η) {
    case 2: {
        // Return z = 2 − (b mod 5), which maps from
        //
        //     b = ( 14, 13, 12, 11, 10,  9,  8,  7,  6,  5,  4,  3,  2,  1,  0 )
        //
        // to
        //
        //   b%5 = (  4,  3,  2,  1,  0,  4,  3,  2,  1,  0,  4,  3,  2,  1,  0 )
        //
        // to
        //
        //     z = ( -2, -1,  0,  1,  2, -2, -1,  0,  1,  2, -2, -1,  0,  1,  2 )
        //
        if (b > 14) {
            return (0, false);
        }
        // Calculate b % 5 with Barrett reduction, to avoid a potentially
        // variable-time division.
        const uint32 barrettMultiplier = 0x3334; // ⌈2¹⁶ / 5⌉
        UntypedInt barrettShift = 16; // log₂(2¹⁶)
        var quotient = ((uint32)b * barrettMultiplier) >> (int)(barrettShift);
        var remainder = (uint32)b - quotient * 5;
        return (fieldSubToMontgomery(2, remainder), true);
    }
    case 4: {
        // Return z = 4 − b, which maps from
        //
        //   b = (  8,  7,  6,  5,  4,  3,  2,  1,  0 )
        //
        // to
        //
        //   z = ( −4, -3, -2, -1,  0,  1,  2,  3,  4 )
        //
        if (b > 8) {
            return (0, false);
        }
        return (fieldSubToMontgomery(4, (uint32)b), true);
    }
    default: {
        throw panic("internal error: unsupported η");
        break;
    }}
}

// power2Round implements Power2Round from FIPS 204.
//
// It separates the bottom d = 13 bits of each 23-bit coefficient, rounding the
// high part based on the low part, and correcting the low part accordingly.
internal static (uint16 hi, fieldElement lo) power2Round(fieldElement r) {
    var rr = fieldFromMontgomery(r);
    // Add 2¹² - 1 to round up r1 by one if r0 > 2¹².
    // r is at most 2²³ - 2¹³ + 1, so rr + (2¹² - 1) won't overflow 23 bits.
    var r1 = rr + ((uint32)1 << 12) - 1;
    r1 >>= (int)(13);
    // r1 <= 2¹⁰ - 1
    // r1 * 2¹³ <= (2¹⁰ - 1) * 2¹³ = 2²³ - 2¹³ < q
    var r0 = fieldSubToMontgomery(rr, r1 << 13);
    return ((uint16)r1, r0);
}

// highBits implements HighBits from FIPS 204.
internal static array<byte> highBits(ringElement r, parameters p) {
    r = r.Clone();

    array<byte> w = new(256); /* n */
    switch (p.γ2) {
    case 32: {
        foreach (var i in range(n)) {
            w[i] = highBits32(fieldFromMontgomery(r[i]));
        }
        break;
    }
    case 88: {
        foreach (var i in range(n)) {
            w[i] = highBits88(fieldFromMontgomery(r[i]));
        }
        break;
    }
    default: {
        throw panic("mldsa: internal error: unsupported γ2");
        break;
    }}
    return w.Clone();
}

// useHint implements UseHint from FIPS 204.
//
// It is not constant-time.
internal static array<byte> useHint(ringElement r, /*[256]*/ array<byte> h, parameters p) {
    r = r.Clone();
    h = h.Clone();

    array<byte> w = new(256); /* n */
    switch (p.γ2) {
    case 32: {
        foreach (var i in range(n)) {
            w[i] = useHint32(r[i], h[i]);
        }
        break;
    }
    case 88: {
        foreach (var i in range(n)) {
            w[i] = useHint88(r[i], h[i]);
        }
        break;
    }
    default: {
        throw panic("mldsa: internal error: unsupported γ2");
        break;
    }}
    return w.Clone();
}

// makeHint implements MakeHint from FIPS 204.
internal static (array<byte> h, nint count1s) makeHint(ringElement ct0, ringElement w, ringElement cs2, parameters p) {
    array<byte> h = new(256);
    nint count1s = default!;
    ct0 = ct0.Clone();
    w = w.Clone();
    cs2 = cs2.Clone();

    switch (p.γ2) {
    case 32: {
        foreach (var i in range(n)) {
            h[i] = makeHint32(ct0[i], w[i], cs2[i]);
            count1s += (nint)h[i];
        }
        break;
    }
    case 88: {
        foreach (var i in range(n)) {
            h[i] = makeHint88(ct0[i], w[i], cs2[i]);
            count1s += (nint)h[i];
        }
        break;
    }
    default: {
        throw panic("mldsa: internal error: unsupported γ2");
        break;
    }}
    return (h.Clone(), count1s);
}

// highBits32 implements HighBits from FIPS 204 for γ2 = (q - 1) / 32.
internal static byte highBits32(uint32 x) {
    // The implementation is based on the reference implementation and on
    // BoringSSL. There are exhaustive tests in TestDecompose that compare it to
    // a straightforward implementation of Decompose from the spec, so for our
    // purposes it only has to work and be constant-time.
    var r1 = (x + 127) >> 7;
    r1 = (r1 * 1025 + ((uint32)1 << 21)) >> 22;
    r1 &= (uint32)(0b1111);
    return (byte)r1;
}

// decompose32 implements Decompose from FIPS 204 for γ2 = (q - 1) / 32.
//
// r1 is in [0, 15].
internal static (byte r1, int32 r0) decompose32(fieldElement r) {
    byte r1 = default!;
    int32 r0 = default!;

    var x = fieldFromMontgomery(r);
    r1 = highBits32(x);

    // r - r1 * (2 * γ2) mod± q
    r0 = (int32)x - (int32)r1 * 2 * (int32)(q - 1) / 32;
    r0 = constantTimeSelectLessOrEqual(q / 2 + 1, r0, r0 - (int32)q, r0);

    return (r1, r0);
}

// useHint32 implements UseHint from FIPS 204 for γ2 = (q - 1) / 32.
internal static byte useHint32(fieldElement r, byte hint) {
    const byte m = 16; // (q − 1) / (2 * γ2)
    var (r1, r0) = decompose32(r);
    if (hint == 1) {
        if (r0 > 0){
            r1 = (byte)((byte)(r1 + 1) % m);
        } else {
            // Underflow is safe, because it operates modulo 256 (since the type
            // is byte), which is a multiple of m.
            r1 = (byte)((byte)(r1 - 1) % m);
        }
    }
    return r1;
}

// makeHint32 implements MakeHint from FIPS 204 for γ2 = (q - 1) / 32.
internal static byte makeHint32(fieldElement ct0, fieldElement w, fieldElement cs2) {
    // v1 = HighBits(r + z) = HighBits(w - cs2 + ct0 - ct0) = HighBits(w - cs2)
    var rPlusZ = fieldSub(w, cs2);
    var v1 = highBits32(fieldFromMontgomery(rPlusZ));
    // r1 = HighBits(r) = HighBits(w - cs2 + ct0)
    var r1 = highBits32(fieldFromMontgomery(fieldAdd(rPlusZ, ct0)));

    return (byte)(constanttime.ByteEq(v1, r1) ^ 1);
}

// highBits88 implements HighBits from FIPS 204 for γ2 = (q - 1) / 88.
internal static byte highBits88(uint32 x) {
    // Like highBits32, this is exhaustively tested in TestDecompose.
    var r1 = (x + 127) >> 7;
    r1 = (r1 * 11275 + ((uint32)1 << 23)) >> 24;
    r1 = constantTimeSelectEqual(r1, 44, 0, r1);
    return (byte)r1;
}

// decompose88 implements Decompose from FIPS 204 for γ2 = (q - 1) / 88.
//
// r1 is in [0, 43].
internal static (byte r1, int32 r0) decompose88(fieldElement r) {
    byte r1 = default!;
    int32 r0 = default!;

    var x = fieldFromMontgomery(r);
    r1 = highBits88(x);

    // r - r1 * (2 * γ2) mod± q
    r0 = (int32)x - (int32)r1 * 2 * (int32)(q - 1) / 88;
    r0 = constantTimeSelectLessOrEqual(q / 2 + 1, r0, r0 - (int32)q, r0);

    return (r1, r0);
}

// useHint88 implements UseHint from FIPS 204 for γ2 = (q - 1) / 88.
internal static byte useHint88(fieldElement r, byte hint) {
    UntypedInt m = 44; // (q − 1) / (2 * γ2)
    var (r1, r0) = decompose88(r);
    if (hint == 1) {
        if (r0 > 0){
            // (r1 + 1) mod m, for r1 in [0, m-1]
            if (r1 == (byte)(m - 1)){
                r1 = 0;
            } else {
                r1++;
            }
        } else {
            // (r1 - 1) % m, for r1 in [0, m-1]
            if (r1 == 0){
                r1 = (byte)(m - 1);
            } else {
                r1--;
            }
        }
    }
    return r1;
}

// makeHint88 implements MakeHint from FIPS 204 for γ2 = (q - 1) / 88.
internal static byte makeHint88(fieldElement ct0, fieldElement w, fieldElement cs2) {
    // Same as makeHint32 above.
    var rPlusZ = fieldSub(w, cs2);
    var v1 = highBits88(fieldFromMontgomery(rPlusZ));
    var r1 = highBits88(fieldFromMontgomery(fieldAdd(rPlusZ, ct0)));
    return (byte)(constanttime.ByteEq(v1, r1) ^ 1);
}

// bitPack implements BitPack(r mod± q, γ₁-1, γ₁), which packs the centered
// coefficients of r into little-endian γ1+1-bit chunks. It appends to buf.
//
// It must only be applied to r with coefficients in [−γ₁+1, γ₁], as
// guaranteed by the rejection conditions in Sign.
internal static slice<byte> bitPack(slice<byte> b, ringElement r, parameters p) {
    r = r.Clone();

    switch (p.γ1) {
    case 17: {
        return bitPack18(b, r);
    }
    case 19: {
        return bitPack20(b, r);
    }
    default: {
        throw panic("mldsa: internal error: unsupported γ1");
        break;
    }}
}

// bitPack18 implements BitPack(r mod± q, 2¹⁷-1, 2¹⁷), which packs the centered
// coefficients of r into little-endian 18-bit chunks. It appends to buf.
//
// It must only be applied to r with coefficients in [−2¹⁷+1, 2¹⁷], as
// guaranteed by the rejection conditions in Sign.
internal static slice<byte> bitPack18(slice<byte> buf, ringElement r) {
    r = r.Clone();

    var (@out, v) = sliceForAppend(buf, 18 * n / 8);
    const int32 b = /* 1 << 17 */ 131072;
    for (nint i = 0; i < n; i += 4) {
        // b - [−2¹⁷+1, 2¹⁷] = [0, 2²⁸-1]
        var w0 = b - fieldCenteredMod(r[i]);
        v[0] = (byte)(w0 << 0);
        v[1] = (byte)(w0 >> 8);
        v[2] = (byte)(w0 >> 16);
        var w1 = b - fieldCenteredMod(r[i + 1]);
        v[2] |= (byte)((byte)(w1 << 2));
        v[3] = (byte)(w1 >> 6);
        v[4] = (byte)(w1 >> 14);
        var w2 = b - fieldCenteredMod(r[i + 2]);
        v[4] |= (byte)((byte)(w2 << 4));
        v[5] = (byte)(w2 >> 4);
        v[6] = (byte)(w2 >> 12);
        var w3 = b - fieldCenteredMod(r[i + 3]);
        v[6] |= (byte)((byte)(w3 << 6));
        v[7] = (byte)(w3 >> 2);
        v[8] = (byte)(w3 >> 10);
        v = v[(int)(4 * 18 / 8)..];
    }
    return @out;
}

// bitPack20 implements BitPack(r mod± q, 2¹⁹-1, 2¹⁹), which packs the centered
// coefficients of r into little-endian 20-bit chunks. It appends to buf.
//
// It must only be applied to r with coefficients in [−2¹⁹+1, 2¹⁹], as
// guaranteed by the rejection conditions in Sign.
internal static slice<byte> bitPack20(slice<byte> buf, ringElement r) {
    r = r.Clone();

    var (@out, v) = sliceForAppend(buf, 20 * n / 8);
    const int32 b = /* 1 << 19 */ 524288;
    for (nint i = 0; i < n; i += 2) {
        // b - [−2¹⁹+1, 2¹⁹] = [0, 2²⁰-1]
        var w0 = b - fieldCenteredMod(r[i]);
        v[0] = (byte)(w0 << 0);
        v[1] = (byte)(w0 >> 8);
        v[2] = (byte)(w0 >> 16);
        var w1 = b - fieldCenteredMod(r[i + 1]);
        v[2] |= (byte)((byte)(w1 << 4));
        v[3] = (byte)(w1 >> 4);
        v[4] = (byte)(w1 >> 12);
        v = v[(int)(2 * 20 / 8)..];
    }
    return @out;
}

// bitUnpack implements BitUnpack(v, 2^γ1-1, 2^γ1), which unpacks each γ1+1 bits
// in little-endian into a coefficient in [-2^γ1+1, 2^γ1].
internal static ringElement bitUnpack(slice<byte> v, parameters p) {
    switch (p.γ1) {
    case 17: {
        return bitUnpack18(v);
    }
    case 19: {
        return bitUnpack20(v);
    }
    default: {
        throw panic("mldsa: internal error: unsupported γ1");
        break;
    }}
}

// bitUnpack18 implements BitUnpack(v, 2¹⁷-1, 2¹⁷), which unpacks each 18 bits
// in little-endian into a coefficient in [-2¹⁷+1, 2¹⁷].
internal static ringElement bitUnpack18(slice<byte> v) {
    if (len(v) != (nint)(18 * n / 8)) {
        throw panic("mldsa: internal error: invalid bitUnpack18 input length");
    }
    const uint32 b = /* 1 << 17 */ 131072;
    const uint32 mask18 = /* 1<<18 - 1 */ 262143;
    ringElement r = default!;
    for (nint i = 0; i < n; i += 4) {
        var w0 = (uint32)v[0] | (uint32)v[1] << 8 | (uint32)v[2] << 16;
        r[i + 0] = fieldSubToMontgomery(b, (uint32)(w0 & mask18));
        var w1 = (uint32)v[2] >> 2 | (uint32)v[3] << 6 | (uint32)v[4] << 14;
        r[i + 1] = fieldSubToMontgomery(b, (uint32)(w1 & mask18));
        var w2 = (uint32)v[4] >> 4 | (uint32)v[5] << 4 | (uint32)v[6] << 12;
        r[i + 2] = fieldSubToMontgomery(b, (uint32)(w2 & mask18));
        var w3 = (uint32)v[6] >> 6 | (uint32)v[7] << 2 | (uint32)v[8] << 10;
        r[i + 3] = fieldSubToMontgomery(b, (uint32)(w3 & mask18));
        v = v[(int)(4 * 18 / 8)..];
    }
    return r.Clone();
}

// bitUnpack20 implements BitUnpack(v, 2¹⁹-1, 2¹⁹), which unpacks each 20 bits
// in little-endian into a coefficient in [-2¹⁹+1, 2¹⁹].
internal static ringElement bitUnpack20(slice<byte> v) {
    if (len(v) != (nint)(20 * n / 8)) {
        throw panic("mldsa: internal error: invalid bitUnpack20 input length");
    }
    const uint32 b = /* 1 << 19 */ 524288;
    const uint32 mask20 = /* 1<<20 - 1 */ 1048575;
    ringElement r = default!;
    for (nint i = 0; i < n; i += 2) {
        var w0 = (uint32)v[0] | (uint32)v[1] << 8 | (uint32)v[2] << 16;
        r[i + 0] = fieldSubToMontgomery(b, (uint32)(w0 & mask20));
        var w1 = (uint32)v[2] >> 4 | (uint32)v[3] << 4 | (uint32)v[4] << 12;
        r[i + 1] = fieldSubToMontgomery(b, (uint32)(w1 & mask20));
        v = v[(int)(2 * 20 / 8)..];
    }
    return r.Clone();
}

// sliceForAppend takes a slice and a requested number of bytes. It returns a
// slice with the contents of the given slice followed by that many bytes and a
// second slice that aliases into it and contains only the extra bytes. If the
// original slice has sufficient capacity then no allocation is performed.
internal static (slice<byte> head, slice<byte> tail) sliceForAppend(slice<byte> @in, nint n) {
    slice<byte> head = default!;
    slice<byte> tail = default!;

    {
        nint total = len(@in) + n; if (cap(@in) >= total){
            head = @in.slice(0, total);
        } else {
            head = new slice<byte>(total);
            copy(head, @in);
        }
    }
    tail = head.slice(len(@in));
    return (head, tail);
}

// constantTimeSelectLessOrEqual returns yes if a <= b, no otherwise, in constant time.
internal static int32 constantTimeSelectLessOrEqual(int32 a, int32 b, int32 yes, int32 no) {
    return (int32)constanttime.Select(constanttime.LessOrEq((nint)a, (nint)b), (nint)yes, (nint)no);
}

// constantTimeSelectEqual returns yes if a == b, no otherwise, in constant time.
internal static uint32 constantTimeSelectEqual(uint32 a, uint32 b, uint32 yes, uint32 no) {
    return (uint32)constanttime.Select(constanttime.Eq((int32)a, (int32)b), (nint)yes, (nint)no);
}

// constantTimeAbs returns the absolute value of x in constant time.
internal static uint32 constantTimeAbs(int32 x) {
    return (uint32)constantTimeSelectLessOrEqual(0, x, x, -x);
}

} // end mldsa_package
