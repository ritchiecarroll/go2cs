// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file provides Go implementations of elementary multi-precision
// arithmetic operations on word vectors. These have the suffix _g.
// These are needed for platforms without assembly implementations of these routines.
// This file also contains elementary operations that can be implemented
// sufficiently efficiently in Go.
namespace go.math;

using bits = go.math.bits_package;
using go.math;

partial class big_package {

partial struct Word /*num:nuint*/;

internal static UntypedInt _S => /* _W / 8 */ 8; // word size in bytes
internal static UntypedInt _W => /* bits.UintSize */ 64; // word size in bits
internal static readonly GoBigConst _B = /* 1 << _W */    // digit base
    GoBigConst.Parse("18446744073709551616");
internal static UntypedInt _M => /* _B - 1 */ 18446744073709551615; // digit mask

// Many of the loops in this file are of the form
//   for i := 0; i < len(z) && i < len(x) && i < len(y); i++
// i < len(z) is the real condition.
// However, checking i < len(x) && i < len(y) as well is faster than
// having the compiler do a bounds check in the body of the loop;
// remarkably it is even faster than hoisting the bounds check
// out of the loop, by doing something like
//   _, _ = x[len(z)-1], y[len(z)-1]
// There are other ways to hoist the bounds check out of the loop,
// but the compiler's BCE isn't powerful enough for them (yet?).
// See the discussion in CL 164966.
// ----------------------------------------------------------------------------
// Elementary operations on words
//
// These operations are used by the vector operations below.

// z1<<_W + z0 = x*y
internal static (Word z1, Word z0) mulWW(Word x, Word y) {
    var (hi, lo) = bits.Mul((nuint)x, (nuint)y);
    return (((Word)hi), ((Word)lo));
}

// z1<<_W + z0 = x*y + c
internal static (Word z1, Word z0) mulAddWWW_g(Word x, Word y, Word c) {
    var (hi, lo) = bits.Mul((nuint)x, (nuint)y);
    nuint cc = default!;
    (lo, cc) = bits.Add(lo, (nuint)c, 0);
    return (((Word)(hi + cc)), ((Word)lo));
}

// nlz returns the number of leading zeros in x.
// Wraps bits.LeadingZeros call for convenience.
internal static nuint nlz(Word x) {
    return (nuint)bits.LeadingZeros((nuint)x);
}

// The resulting carry c is either 0 or 1.
internal static Word /*c*/ addVV_g(slice<Word> z, slice<Word> x, slice<Word> y) {
    Word c = default!;

    // The comment near the top of this file discusses this for loop condition.
    for (nint i = 0; i < len(z) && i < len(x) && i < len(y); i++) {
        var (zi, cc) = bits.Add((nuint)x[i], (nuint)y[i], (nuint)c);
        z[i] = ((Word)zi);
        c = ((Word)cc);
    }
    return c;
}

// The resulting carry c is either 0 or 1.
internal static Word /*c*/ subVV_g(slice<Word> z, slice<Word> x, slice<Word> y) {
    Word c = default!;

    // The comment near the top of this file discusses this for loop condition.
    for (nint i = 0; i < len(z) && i < len(x) && i < len(y); i++) {
        var (zi, cc) = bits.Sub((nuint)x[i], (nuint)y[i], (nuint)c);
        z[i] = ((Word)zi);
        c = ((Word)cc);
    }
    return c;
}

// The resulting carry c is either 0 or 1.
internal static Word /*c*/ addVW_g(slice<Word> z, slice<Word> x, Word y) {
    Word c = default!;

    c = y;
    // The comment near the top of this file discusses this for loop condition.
    for (nint i = 0; i < len(z) && i < len(x); i++) {
        var (zi, cc) = bits.Add((nuint)x[i], (nuint)c, 0);
        z[i] = ((Word)zi);
        c = ((Word)cc);
    }
    return c;
}

// addVWlarge is addVW, but intended for large z.
// The only difference is that we check on every iteration
// whether we are done with carries,
// and if so, switch to a much faster copy instead.
// This is only a good idea for large z,
// because the overhead of the check and the function call
// outweigh the benefits when z is small.
internal static Word /*c*/ addVWlarge(slice<Word> z, slice<Word> x, Word y) {
    Word c = default!;

    c = y;
    // The comment near the top of this file discusses this for loop condition.
    for (nint i = 0; i < len(z) && i < len(x); i++) {
        if (c == 0) {
            copy(z.slice(i), x.slice(i));
            return c;
        }
        var (zi, cc) = bits.Add((nuint)x[i], (nuint)c, 0);
        z[i] = ((Word)zi);
        c = ((Word)cc);
    }
    return c;
}

internal static Word /*c*/ subVW_g(slice<Word> z, slice<Word> x, Word y) {
    Word c = default!;

    c = y;
    // The comment near the top of this file discusses this for loop condition.
    for (nint i = 0; i < len(z) && i < len(x); i++) {
        var (zi, cc) = bits.Sub((nuint)x[i], (nuint)c, 0);
        z[i] = ((Word)zi);
        c = ((Word)cc);
    }
    return c;
}

// subVWlarge is to subVW as addVWlarge is to addVW.
internal static Word /*c*/ subVWlarge(slice<Word> z, slice<Word> x, Word y) {
    Word c = default!;

    c = y;
    // The comment near the top of this file discusses this for loop condition.
    for (nint i = 0; i < len(z) && i < len(x); i++) {
        if (c == 0) {
            copy(z.slice(i), x.slice(i));
            return c;
        }
        var (zi, cc) = bits.Sub((nuint)x[i], (nuint)c, 0);
        z[i] = ((Word)zi);
        c = ((Word)cc);
    }
    return c;
}

internal static Word /*c*/ shlVU_g(slice<Word> z, slice<Word> x, nuint s) {
    Word c = default!;

    if (s == 0) {
        copy(z, x);
        return c;
    }
    if (len(z) == 0) {
        return c;
    }
    s &= (nuint)(_W - 1); // hint to the compiler that shifts by s don't need guard code
    nuint ŝ = (nuint)_W - s;
    ŝ &= (nuint)(_W - 1); // ditto
    c = (x[len(z) - 1] >> (int)(ŝ));
    for (nint i = len(z) - 1; i > 0; i--) {
        z[i] = (Word)((x[i] << (int)(s)) | (x[i - 1] >> (int)(ŝ)));
    }
    z[0] = (x[0] << (int)(s));
    return c;
}

internal static Word /*c*/ shrVU_g(slice<Word> z, slice<Word> x, nuint s) {
    Word c = default!;

    if (s == 0) {
        copy(z, x);
        return c;
    }
    if (len(z) == 0) {
        return c;
    }
    if (len(x) != len(z)) {
        // This is an invariant guaranteed by the caller.
        throw panic("len(x) != len(z)");
    }
    s &= (nuint)(_W - 1); // hint to the compiler that shifts by s don't need guard code
    nuint ŝ = (nuint)_W - s;
    ŝ &= (nuint)(_W - 1); // ditto
    c = (x[0] << (int)(ŝ));
    for (nint i = 1; i < len(z); i++) {
        z[i - 1] = (Word)((x[i - 1] >> (int)(s)) | (x[i] << (int)(ŝ)));
    }
    z[len(z) - 1] = (x[len(z) - 1] >> (int)(s));
    return c;
}

internal static Word /*c*/ mulAddVWW_g(slice<Word> z, slice<Word> x, Word y, Word r) {
    Word c = default!;

    c = r;
    // The comment near the top of this file discusses this for loop condition.
    for (nint i = 0; i < len(z) && i < len(x); i++) {
        (c, z[i]) = mulAddWWW_g(x[i], y, c);
    }
    return c;
}

internal static Word /*c*/ addMulVVW_g(slice<Word> z, slice<Word> x, Word y) {
    Word c = default!;

    // The comment near the top of this file discusses this for loop condition.
    for (nint i = 0; i < len(z) && i < len(x); i++) {
        var (z1, z0) = mulAddWWW_g(x[i], y, z[i]);
        var (lo, cc) = bits.Add((nuint)z0, (nuint)c, 0);
        (c, z[i]) = (((Word)cc), ((Word)lo));
        c += z1;
    }
    return c;
}

// q = ( x1 << _W + x0 - r)/y. m = floor(( _B^2 - 1 ) / d - _B). Requiring x1<y.
// An approximate reciprocal with a reference to "Improved Division by Invariant Integers
// (IEEE Transactions on Computers, 11 Jun. 2010)"
internal static (Word q, Word r) divWW(Word x1, Word x0, Word y, Word m) {
    nuint s = nlz(y);
    if (s != 0) {
        x1 = (Word)((x1 << (int)(s)) | (x0 >> (int)(((nuint)_W - s))));
        x0 <<= (int)(s);
        y <<= (int)(s);
    }
    nuint d = (nuint)y;
    // We know that
    //   m = ⎣(B^2-1)/d⎦-B
    //   ⎣(B^2-1)/d⎦ = m+B
    //   (B^2-1)/d = m+B+delta1    0 <= delta1 <= (d-1)/d
    //   B^2/d = m+B+delta2        0 <= delta2 <= 1
    // The quotient we're trying to compute is
    //   quotient = ⎣(x1*B+x0)/d⎦
    //            = ⎣(x1*B*(B^2/d)+x0*(B^2/d))/B^2⎦
    //            = ⎣(x1*B*(m+B+delta2)+x0*(m+B+delta2))/B^2⎦
    //            = ⎣(x1*m+x1*B+x0)/B + x0*m/B^2 + delta2*(x1*B+x0)/B^2⎦
    // The latter two terms of this three-term sum are between 0 and 1.
    // So we can compute just the first term, and we will be low by at most 2.
    var (t1, t0) = bits.Mul((nuint)m, (nuint)x1);
    var (_, c) = bits.Add(t0, (nuint)x0, 0);
    (t1, _) = bits.Add(t1, (nuint)x1, c);
    // The quotient is either t1, t1+1, or t1+2.
    // We'll try t1 and adjust if needed.
    nuint qq = t1;
    // compute remainder r=x-d*q.
    var (dq1, dq0) = bits.Mul(d, qq);
    var (r0, b) = bits.Sub((nuint)x0, dq0, 0);
    var (r1, _) = bits.Sub((nuint)x1, dq1, b);
    // The remainder we just computed is bounded above by B+d:
    // r = x1*B + x0 - d*q.
    //   = x1*B + x0 - d*⎣(x1*m+x1*B+x0)/B⎦
    //   = x1*B + x0 - d*((x1*m+x1*B+x0)/B-alpha)                                   0 <= alpha < 1
    //   = x1*B + x0 - x1*d/B*m                         - x1*d - x0*d/B + d*alpha
    //   = x1*B + x0 - x1*d/B*⎣(B^2-1)/d-B⎦             - x1*d - x0*d/B + d*alpha
    //   = x1*B + x0 - x1*d/B*⎣(B^2-1)/d-B⎦             - x1*d - x0*d/B + d*alpha
    //   = x1*B + x0 - x1*d/B*((B^2-1)/d-B-beta)        - x1*d - x0*d/B + d*alpha   0 <= beta < 1
    //   = x1*B + x0 - x1*B + x1/B + x1*d + x1*d/B*beta - x1*d - x0*d/B + d*alpha
    //   =        x0        + x1/B        + x1*d/B*beta        - x0*d/B + d*alpha
    //   = x0*(1-d/B) + x1*(1+d*beta)/B + d*alpha
    //   <  B*(1-d/B) +  d*B/B          + d          because x0<B (and 1-d/B>0), x1<d, 1+d*beta<=B, alpha<1
    //   =  B - d     +  d              + d
    //   = B+d
    // So r1 can only be 0 or 1. If r1 is 1, then we know q was too small.
    // Add 1 to q and subtract d from r. That guarantees that r is <B, so
    // we no longer need to keep track of r1.
    if (r1 != 0) {
        qq++;
        r0 -= d;
    }
    // If the remainder is still too large, increment q one more time.
    if (r0 >= d) {
        qq++;
        r0 -= d;
    }
    return (((Word)qq), ((Word)(r0.Rsh(s))));
}

// reciprocalWord return the reciprocal of the divisor. rec = floor(( _B^2 - 1 ) / u - _B). u = d1 << nlz(d1).
internal static Word reciprocalWord(Word d1) {
    nuint u = (nuint)((d1 << (int)(nlz(d1))));
    nuint x1 = ~u;
    nuint x0 = (nuint)_M;
    var (rec, _) = bits.Div(x1, x0, u); // (_B^2-1)/U-_B = (_B*(_M-C)+_M)/U
    return ((Word)rec);
}

} // end big_package
