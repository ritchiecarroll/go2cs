// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file implements the Bits type used for testing Float operations
// via an independent (albeit slower) representations for floating-point
// numbers.
namespace go.math;

using fmt = fmt_package;
using slices = slices_package;
using testing = testing_package;
using static go.math.big_package;

partial class big_internal_test_package {

public partial struct ΔBits /*[]nint*/;

internal static ΔBits add(this ΔBits x, ΔBits y) {
    return appendꓸꓸꓸ(x, y);
}

internal static ΔBits mul(this ΔBits x, ΔBits y) {
    ΔBits p = default!;
    foreach (var (_, xΔ1) in x) {
        foreach (var (_, yΔ1) in y) {
            p = append(p, xΔ1 + yΔ1);
        }
    }
    return p;
}

internal partial struct TestMulBits_type /*dyn*/ {
    internal ΔBits x, y, want;
}

public static void TestMulBits(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in new TestMulBits_type[]{
        new(default!, default!, default!),
        new(new ΔBits(new nint[]{}.slice()), new ΔBits(new nint[]{}.slice()), default!),
        new(new ΔBits(new nint[]{0}.slice()), new ΔBits(new nint[]{0}.slice()), new ΔBits(new nint[]{0}.slice())),
        new(new ΔBits(new nint[]{0}.slice()), new ΔBits(new nint[]{1}.slice()), new ΔBits(new nint[]{1}.slice())),
        new(new ΔBits(new nint[]{1}.slice()), new ΔBits(new nint[]{1, 2, 3}.slice()), new ΔBits(new nint[]{2, 3, 4}.slice())),
        new(new ΔBits(new nint[]{-1}.slice()), new ΔBits(new nint[]{1}.slice()), new ΔBits(new nint[]{0}.slice())),
        new(new ΔBits(new nint[]{-10, -1, 0, 1, 10}.slice()), new ΔBits(new nint[]{1, 2, 3}.slice()), new ΔBits(new nint[]{-9, -8, -7, 0, 1, 2, 1, 2, 3, 2, 3, 4, 11, 12, 13}.slice()))
    }.slice()) {
        @string got = fmt.Sprintf("%v"u8, test.x.mul(test.y));
        @string want = fmt.Sprintf("%v"u8, test.want);
        if (got != want) {
            Ꮡt.Errorf("%v * %v = %s; want %s"u8, test.x, test.y, got, want);
        }
    }
}

// norm returns the normalized bits for x: It removes multiple equal entries
// by treating them as an addition (e.g., Bits{5, 5} => Bits{6}), and it sorts
// the result list for reproducible results.
internal static ΔBits norm(this ΔBits x) {
    var m = new map<nint, bool>();
    foreach (var (_, vᴛ1) in x) {
        var b = vᴛ1;

        while (m[b]) {
            m[b] = false;
            b++;
        }
        m[b] = true;
    }
    ΔBits z = default!;
    foreach (var (b, set) in m) {
        if (set) {
            z = append(z, b);
        }
    }
    slices.Sort<slice<nint>, nint>(((slice<nint>)z));
    return z;
}

internal partial struct TestNormBits_type /*dyn*/ {
    internal ΔBits x, want;
}

public static void TestNormBits(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in new TestNormBits_type[]{
        new(default!, default!),
        new(new ΔBits(new nint[]{}.slice()), new ΔBits(new nint[]{}.slice())),
        new(new ΔBits(new nint[]{0}.slice()), new ΔBits(new nint[]{0}.slice())),
        new(new ΔBits(new nint[]{0, 0}.slice()), new ΔBits(new nint[]{1}.slice())),
        new(new ΔBits(new nint[]{3, 1, 1}.slice()), new ΔBits(new nint[]{2, 3}.slice())),
        new(new ΔBits(new nint[]{10, 9, 8, 7, 6, 6}.slice()), new ΔBits(new nint[]{11}.slice()))
    }.slice()) {
        @string got = fmt.Sprintf("%v"u8, test.x.norm());
        @string want = fmt.Sprintf("%v"u8, test.want);
        if (got != want) {
            Ꮡt.Errorf("normBits(%v) = %s; want %s"u8, test.x, got, want);
        }
    }
}

// round returns the Float value corresponding to x after rounding x
// to prec bits according to mode.
internal static ж<global::go.math.big_package.Float> round(this ΔBits x, nuint prec, global::go.math.big_package.RoundingMode mode) {
    x = x.norm();
    // determine range
    nint min = default!;
    nint max = default!;
    foreach (var (i, b) in x) {
        if (i == 0 || b < min) {
            min = b;
        }
        if (i == 0 || b > max) {
            max = b;
        }
    }
    nuint prec0 = (nuint)(max + 1 - min);
    if (prec >= prec0) {
        return x.ΔFloat();
    }
    // prec < prec0
    // determine bit 0, rounding, and sticky bit, and result bits z
    nuint bit0 = default!;
    nuint rbit = default!;
    nuint sbit = default!;
    ΔBits z = default!;
    nint r = max - (nint)prec;
    foreach (var (_, b) in x) {
        switch (ᐧ) {
        case {} when b == r: {
            rbit = 1;
            break;
        }
        case {} when b < r: {
            sbit = 1;
            break;
        }
        default: {
            if (b == r + 1) {
                // b > r
                bit0 = 1;
            }
            z = append(z, b);
            break;
        }}

    }
    // round
    var f = z.ΔFloat(); // rounded to zero
    if (mode == ToNearestAway) {
        throw panic("not yet implemented");
    }
    if (mode == ToNearestEven && rbit == 1 && (sbit == 1 || sbit == 0 && bit0 != 0) || mode == AwayFromZero) {
        // round away from zero
        f.SetMode(ToZero).SetPrec(prec);
        f.Add(f, new ΔBits(new nint[]{(nint)r + 1}.slice()).ΔFloat());
    }
    return f;
}

// Float returns the *Float z of the smallest possible precision such that
// z = sum(2**bits[i]), with i = range bits. If multiple bits[i] are equal,
// they are added: Bits{0, 1, 0}.Float() == 2**0 + 2**1 + 2**0 = 4.
public static ж<global::go.math.big_package.Float> ΔFloat(this ΔBits bits) {
    // handle 0
    if (len(bits) == 0) {
        return @new<global::go.math.big_package.Float>();
    }
    // len(bits) > 0
    // determine lsb exponent
    nint min = default!;
    foreach (var (i, b) in bits) {
        if (i == 0 || b < min) {
            min = b;
        }
    }
    // create bit pattern
    var x = NewInt(0);
    foreach (var (_, b) in bits) {
        nint badj = b - min;
        // propagate carry if necessary
        while (x.Bit(badj) != 0) {
            x.SetBit(x, badj, 0);
            badj++;
        }
        x.SetBit(x, badj, 1);
    }
    // create corresponding float
    var z = @new<global::go.math.big_package.Float>().SetInt(x); // normalized
    {
        var e = (int64)(~z).exp + (int64)min; if (MinExp <= e && e <= MaxExp){
            z.Value.exp = (int32)e;
        } else {
            // this should never happen for our test cases
            throw panic("exponent out of range");
        }
    }
    return z;
}

internal partial struct TestFromBits_type /*dyn*/ {
    internal ΔBits bits;
    internal @string want;
}

public static void TestFromBits(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in new TestFromBits_type[]{ // all different bit numbers

        new(default!, "0"u8),
        new(new ΔBits(new nint[]{0}.slice()), "0x.8p+1"u8),
        new(new ΔBits(new nint[]{1}.slice()), "0x.8p+2"u8),
        new(new ΔBits(new nint[]{-1}.slice()), "0x.8p+0"u8),
        new(new ΔBits(new nint[]{63}.slice()), "0x.8p+64"u8),
        new(new ΔBits(new nint[]{33, -30}.slice()), "0x.8000000000000001p+34"u8),
        new(new ΔBits(new nint[]{255, 0}.slice()), "0x.8000000000000000000000000000000000000000000000000000000000000001p+256"u8), // multiple equal bit numbers

        new(new ΔBits(new nint[]{0, 0}.slice()), "0x.8p+2"u8),
        new(new ΔBits(new nint[]{0, 0, 0, 0}.slice()), "0x.8p+3"u8),
        new(new ΔBits(new nint[]{0, 1, 0}.slice()), "0x.8p+3"u8),
        new(appendꓸꓸꓸ(new ΔBits(new nint[]{2, 1, 0}.slice()), /* 7 */
 new ΔBits(new nint[]{3, 1}.slice())), /* 10 */
 "0x.88p+5"u8 /* 17 */
)
    }.slice()) {
        var f = test.bits.ΔFloat();
        {
            @string got = f.Text((rune)'p', 0); if (got != test.want) {
                Ꮡt.Errorf("setBits(%v) = %s; want %s"u8, test.bits, got, test.want);
            }
        }
    }
}

} // end big_internal_test_package
