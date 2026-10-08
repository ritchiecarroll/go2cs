// Copyright (c) 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140;

using sync = sync_package;

partial class edwards25519_package {

// basepointTable is a set of 32 affineLookupTables, where table i is generated
// from 256i * basepoint. It is precomputed the first time it's used.
internal static ж<array<affineLookupTable>> basepointTable() {
    ᏑbasepointTablePrecomp.of(basepointTablePrecompᴛ1.ᏑinitOnce).Do(() => {
        var p = NewGeneratorPoint();
        for (nint i = 0; i < 32; i++) {
            basepointTablePrecomp.table[i].FromP3(p);
            for (nint j = 0; j < 8; j++) {
                p.Add(p, p);
            }
        }
    });
    return ᏑbasepointTablePrecomp.of(basepointTablePrecompᴛ1.Ꮡtable);
}


partial struct basepointTablePrecompᴛ1 /*dyn*/ {
    internal array<affineLookupTable> table = new(32, () => new());
    internal sync.Once initOnce;
}
internal static ж<basepointTablePrecompᴛ1> ᏑbasepointTablePrecomp = new StandardBox<basepointTablePrecompᴛ1>(new basepointTablePrecompᴛ1());
internal static ref basepointTablePrecompᴛ1 basepointTablePrecomp => ref ᏑbasepointTablePrecomp.Value;

// ScalarBaseMult sets v = x * B, where B is the canonical generator, and
// returns v.
//
// The scalar multiplication is done in constant time.
public static ж<Point> ScalarBaseMult(this ж<Point> Ꮡv, ж<Scalar> Ꮡx) {
    ref var v = ref Ꮡv.DerefOrNull();
    ref var x = ref Ꮡx.DerefOrNull();

    var basepointTableΔ1 = basepointTable();
    // Write x = sum(x_i * 16^i) so  x*B = sum( B*x_i*16^i )
    // as described in the Ed25519 paper
    //
    // Group even and odd coefficients
    // x*B     = x_0*16^0*B + x_2*16^2*B + ... + x_62*16^62*B
    //         + x_1*16^1*B + x_3*16^3*B + ... + x_63*16^63*B
    // x*B     = x_0*16^0*B + x_2*16^2*B + ... + x_62*16^62*B
    //    + 16*( x_1*16^0*B + x_3*16^2*B + ... + x_63*16^62*B)
    //
    // We use a lookup table for each i to get x_i*16^(2*i)*B
    // and do four doublings to multiply by 16.
    var digits = Ꮡx.signedRadix16();
    var multiple = Ꮡ(new affineCached(nil));
    var tmp1 = Ꮡ(new projP1xP1(nil));
    var tmp2 = Ꮡ(new projP2(nil));
    // Accumulate the odd components first
    Ꮡv.Set(NewIdentityPoint());
    for (nint i = 1; i < 64; i += 2) {
        basepointTableΔ1.Value[i / 2].SelectInto(multiple, digits[i]);
        tmp1.AddAffine(Ꮡv, multiple);
        Ꮡv.fromP1xP1(tmp1);
    }
    // Multiply by 16
    tmp2.FromP3(Ꮡv); // tmp2 =    v in P2 coords
    tmp1.Double(tmp2); // tmp1 =  2*v in P1xP1 coords
    tmp2.FromP1xP1(tmp1); // tmp2 =  2*v in P2 coords
    tmp1.Double(tmp2); // tmp1 =  4*v in P1xP1 coords
    tmp2.FromP1xP1(tmp1); // tmp2 =  4*v in P2 coords
    tmp1.Double(tmp2); // tmp1 =  8*v in P1xP1 coords
    tmp2.FromP1xP1(tmp1); // tmp2 =  8*v in P2 coords
    tmp1.Double(tmp2); // tmp1 = 16*v in P1xP1 coords
    Ꮡv.fromP1xP1(tmp1); // now v = 16*(odd components)
    // Accumulate the even components
    for (nint i = 0; i < 64; i += 2) {
        basepointTableΔ1.Value[i / 2].SelectInto(multiple, digits[i]);
        tmp1.AddAffine(Ꮡv, multiple);
        Ꮡv.fromP1xP1(tmp1);
    }
    return Ꮡv;
}

// ScalarMult sets v = x * q, and returns v.
//
// The scalar multiplication is done in constant time.
public static ж<Point> ScalarMult(this ж<Point> Ꮡv, ж<Scalar> Ꮡx, ж<Point> Ꮡq) {
    ref var v = ref Ꮡv.DerefOrNull();
    ref var x = ref Ꮡx.DerefOrNull();

    checkInitialized(Ꮡq);
    projLookupTable table = new();
    table.FromP3(Ꮡq);
    // Write x = sum(x_i * 16^i)
    // so  x*Q = sum( Q*x_i*16^i )
    //         = Q*x_0 + 16*(Q*x_1 + 16*( ... + Q*x_63) ... )
    //           <------compute inside out---------
    //
    // We use the lookup table to get the x_i*Q values
    // and do four doublings to compute 16*Q
    var digits = Ꮡx.signedRadix16();
    // Unwrap first loop iteration to save computing 16*identity
    var multiple = Ꮡ(new projCached(nil));
    var tmp1 = Ꮡ(new projP1xP1(nil));
    var tmp2 = Ꮡ(new projP2(nil));
    table.SelectInto(multiple, digits[63]);
    Ꮡv.Set(NewIdentityPoint());
    tmp1.Add(Ꮡv, multiple); // tmp1 = x_63*Q in P1xP1 coords
    for (nint i = 62; i >= 0; i--) {
        tmp2.FromP1xP1(tmp1); // tmp2 =    (prev) in P2 coords
        tmp1.Double(tmp2); // tmp1 =  2*(prev) in P1xP1 coords
        tmp2.FromP1xP1(tmp1); // tmp2 =  2*(prev) in P2 coords
        tmp1.Double(tmp2); // tmp1 =  4*(prev) in P1xP1 coords
        tmp2.FromP1xP1(tmp1); // tmp2 =  4*(prev) in P2 coords
        tmp1.Double(tmp2); // tmp1 =  8*(prev) in P1xP1 coords
        tmp2.FromP1xP1(tmp1); // tmp2 =  8*(prev) in P2 coords
        tmp1.Double(tmp2); // tmp1 = 16*(prev) in P1xP1 coords
        Ꮡv.fromP1xP1(tmp1); //    v = 16*(prev) in P3 coords
        table.SelectInto(multiple, digits[i]);
        tmp1.Add(Ꮡv, multiple); // tmp1 = x_i*Q + 16*(prev) in P1xP1 coords
    }
    Ꮡv.fromP1xP1(tmp1);
    return Ꮡv;
}

// basepointNafTable is the nafLookupTable8 for the basepoint.
// It is precomputed the first time it's used.
internal static ж<nafLookupTable8> basepointNafTable() {
    ᏑbasepointNafTablePrecomp.of(basepointNafTablePrecompᴛ1.ᏑinitOnce).Do(() => {
        ᏑbasepointNafTablePrecomp.of(basepointNafTablePrecompᴛ1.Ꮡtable).FromP3(NewGeneratorPoint());
    });
    return ᏑbasepointNafTablePrecomp.of(basepointNafTablePrecompᴛ1.Ꮡtable);
}


partial struct basepointNafTablePrecompᴛ1 /*dyn*/ {
    internal nafLookupTable8 table;
    internal sync.Once initOnce;
}
internal static ж<basepointNafTablePrecompᴛ1> ᏑbasepointNafTablePrecomp = new StandardBox<basepointNafTablePrecompᴛ1>(new basepointNafTablePrecompᴛ1());
internal static ref basepointNafTablePrecompᴛ1 basepointNafTablePrecomp => ref ᏑbasepointNafTablePrecomp.Value;

// VarTimeDoubleScalarBaseMult sets v = a * A + b * B, where B is the canonical
// generator, and returns v.
//
// Execution time depends on the inputs.
public static ж<Point> VarTimeDoubleScalarBaseMult(this ж<Point> Ꮡv, ж<Scalar> Ꮡa, ж<Point> ᏑA, ж<Scalar> Ꮡb) {
    ref var v = ref Ꮡv.DerefOrNull();
    ref var a = ref Ꮡa.DerefOrNull();

    checkInitialized(ᏑA);
    // Similarly to the single variable-base approach, we compute
    // digits and use them with a lookup table.  However, because
    // we are allowed to do variable-time operations, we don't
    // need constant-time lookups or constant-time digit
    // computations.
    //
    // So we use a non-adjacent form of some width w instead of
    // radix 16.  This is like a binary representation (one digit
    // for each binary place) but we allow the digits to grow in
    // magnitude up to 2^{w-1} so that the nonzero digits are as
    // sparse as possible.  Intuitively, this "condenses" the
    // "mass" of the scalar onto sparse coefficients (meaning
    // fewer additions).
    var basepointNafTableΔ1 = basepointNafTable();
    nafLookupTable5 aTable = new();
    aTable.FromP3(ᏑA);
    // Because the basepoint is fixed, we can use a wider NAF
    // corresponding to a bigger table.
    var aNaf = Ꮡa.nonAdjacentForm(5);
    var bNaf = Ꮡb.nonAdjacentForm(8);
    // Find the first nonzero coefficient.
    nint i = 255;
    for (nint j = i; j >= 0; j--) {
        if (aNaf[j] != 0 || bNaf[j] != 0) {
            break;
        }
    }
    var multA = Ꮡ(new projCached(nil));
    var multB = Ꮡ(new affineCached(nil));
    var tmp1 = Ꮡ(new projP1xP1(nil));
    var tmp2 = Ꮡ(new projP2(nil));
    tmp2.Zero();
    // Move from high to low bits, doubling the accumulator
    // at each iteration and checking whether there is a nonzero
    // coefficient to look up a multiple of.
    for (; i >= 0; i--) {
        tmp1.Double(tmp2);
        // Only update v if we have a nonzero coeff to add in.
        if (aNaf[i] > 0){
            Ꮡv.fromP1xP1(tmp1);
            aTable.SelectInto(multA, aNaf[i]);
            tmp1.Add(Ꮡv, multA);
        } else 
        if (aNaf[i] < 0) {
            Ꮡv.fromP1xP1(tmp1);
            aTable.SelectInto(multA, (int8)(-aNaf[i]));
            tmp1.Sub(Ꮡv, multA);
        }
        if (bNaf[i] > 0){
            Ꮡv.fromP1xP1(tmp1);
            basepointNafTableΔ1.SelectInto(multB, bNaf[i]);
            tmp1.AddAffine(Ꮡv, multB);
        } else 
        if (bNaf[i] < 0) {
            Ꮡv.fromP1xP1(tmp1);
            basepointNafTableΔ1.SelectInto(multB, (int8)(-bNaf[i]));
            tmp1.SubAffine(Ꮡv, multB);
        }
        tmp2.FromP1xP1(tmp1);
    }
    Ꮡv.fromP2(tmp2);
    return Ꮡv;
}

} // end edwards25519_package
