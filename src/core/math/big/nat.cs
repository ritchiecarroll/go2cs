// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file implements unsigned multi-precision integers (natural
// numbers). They are the building blocks for the implementation
// of signed integers, rationals, and floating-point numbers.
//
// Caution: This implementation relies on the function "alias"
//          which assumes that (nat) slice capacities are never
//          changed (no 3-operand slice expressions). If that
//          changes, alias needs to be updated for correctness.
namespace go.math;

using byteorder = @internal.byteorder_package;
using bits = go.math.bits_package;
using rand = go.math.rand_package;
using sync = sync_package;
using @internal;
using go.math;

partial class big_package {

[GoType("[]Word")] partial struct nat;

internal static nat natOne = new nat(new Word[]{1}.slice());
internal static nat natTwo = new nat(new Word[]{2}.slice());
internal static nat natFive = new nat(new Word[]{5}.slice());
internal static nat natTen = new nat(new Word[]{10}.slice());

internal static @string String(this nat z) {
    return "0x"u8 + ((sstring)z.itoa(false, 16));
}

internal static nat norm(this nat z) {
    nint i = len(z);
    while (i > 0 && z[i - 1] == 0) {
        i--;
    }
    return z.slice(0, i);
}

internal static nat make(this nat z, nint n) {
    if (n <= cap(z)) {
        return z.slice(0, n); // reuse z
    }
    if (n == 1) {
        // Most nats start small and stay that way; don't over-allocate.
        return new nat(1);
    }
    // Choosing a good value for e has significant performance impact
    // because it increases the chance that a value can be reused.
    const nint e = 4; // extra capacity
    return new nat(n, n + e);
}

internal static nat setWord(this nat z, Word x) {
    if (x == 0) {
        return z[..0];
    }
    z = z.make(1);
    z[0] = x;
    return z;
}

internal static nat setUint64(this nat z, uint64 x) {
    // single-word value
    {
        Word w = ((Word)(nuint)x); if ((uint64)(nuint)w == x) {
            return z.setWord(w);
        }
    }
    // 2-word value
    z = z.make(2);
    z[1] = ((Word)(nuint)((x >> (int)(32))));
    z[0] = ((Word)(nuint)x);
    return z;
}

internal static nat set(this nat z, nat x) {
    z = z.make(len(x));
    copy(z, x);
    return z;
}

internal static nat add(this nat z, nat x, nat y) {
    nint m = len(x);
    nint n = len(y);
    switch (ᐧ) {
    case {} when m < n: {
        return z.add(y, x);
    }
    case {} when m is 0: {
        return z[..0];
    }
    case {} when n is 0: {
        return z.set(x);
    }}

    // n == 0 because m >= n; result is 0
    // result is x
    // m > 0
    z = z.make(m + 1);
    Word c = addVV(z.slice(0, n), x, y);
    if (m > n) {
        c = addVW(z.slice(n, m), x.slice(n), c);
    }
    z[m] = c;
    return z.norm();
}

internal static nat sub(this nat z, nat x, nat y) {
    nint m = len(x);
    nint n = len(y);
    switch (ᐧ) {
    case {} when m < n: {
        throw panic("underflow");
        break;
    }
    case {} when m is 0: {
        return z[..0];
    }
    case {} when n is 0: {
        return z.set(x);
    }}

    // n == 0 because m >= n; result is 0
    // result is x
    // m > 0
    z = z.make(m);
    Word c = subVV(z.slice(0, n), x, y);
    if (m > n) {
        c = subVW(z.slice(n), x.slice(n), c);
    }
    if (c != 0) {
        throw panic("underflow");
    }
    return z.norm();
}

internal static nint /*r*/ cmp(this nat x, nat y) {
    nint r = default!;

    nint m = len(x);
    nint n = len(y);
    if (m != n || m == 0) {
        switch (ᐧ) {
        case {} when m < n: {
            r = -1;
            break;
        }
        case {} when m > n: {
            r = 1;
            break;
        }}

        return r;
    }
    nint i = m - 1;
    while (i > 0 && x[i] == y[i]) {
        i--;
    }
    switch (ᐧ) {
    case {} when x[i] < y[i]: {
        r = -1;
        break;
    }
    case {} when x[i] > y[i]: {
        r = 1;
        break;
    }}

    return r;
}

internal static nat mulAddWW(this nat z, nat x, Word y, Word r) {
    nint m = len(x);
    if (m == 0 || y == 0) {
        return z.setWord(r); // result is r
    }
    // m > 0
    z = z.make(m + 1);
    z[m] = mulAddVWW(z.slice(0, m), x, y, r);
    return z.norm();
}

// basicMul multiplies x and y and leaves the result in z.
// The (non-normalized) result is placed in z[0 : len(x) + len(y)].
internal static void basicMul(nat z, nat x, nat y) {
    clear(z.slice(0, len(x) + len(y))); // initialize z
    foreach (var (i, d) in y) {
        if (d != 0) {
            z[len(x) + i] = addMulVVW(z.slice(i, i + len(x)), x, d);
        }
    }
}

// montgomery computes z mod m = x*y*2**(-n*_W) mod m,
// assuming k = -1/m mod 2**_W.
// z is used for storing the result which is returned;
// z must not alias x, y or m.
// See Gueron, "Efficient Software Implementations of Modular Exponentiation".
// https://eprint.iacr.org/2011/239.pdf
// In the terminology of that paper, this is an "Almost Montgomery Multiplication":
// x and y are required to satisfy 0 <= z < 2**(n*_W) and then the result
// z is guaranteed to satisfy 0 <= z < 2**(n*_W), but it may not be < m.
internal static nat montgomery(this nat z, nat x, nat y, nat m, Word k, nint n) {
    // This code assumes x, y, m are all the same length, n.
    // (required by addMulVVW and the for loop).
    // It also assumes that x, y are already reduced mod m,
    // or else the result will not be properly reduced.
    if (len(x) != n || len(y) != n || len(m) != n) {
        throw panic("math/big: mismatched montgomery number lengths");
    }
    z = z.make(n * 2);
    clear(z);
    Word c = default!;
    for (nint i = 0; i < n; i++) {
        Word d = y[i];
        Word c2 = addMulVVW(z.slice(i, n + i), x, d);
        Word t = z[i] * k;
        Word c3 = addMulVVW(z.slice(i, n + i), m, t);
        Word cx = c + c2;
        Word cy = cx + c3;
        z[n + i] = cy;
        if (cx < c2 || cy < c3){
            c = 1;
        } else {
            c = 0;
        }
    }
    if (c != 0){
        subVV(z.slice(0, n), z.slice(n), m);
    } else {
        copy(z.slice(0, n), z.slice(n));
    }
    return z.slice(0, n);
}

// Fast version of z[0:n+n>>1].add(z[0:n+n>>1], x[0:n]) w/o bounds checks.
// Factored out for readability - do not use outside karatsuba.
internal static void karatsubaAdd(nat z, nat x, nint n) {
    {
        Word c = addVV(z.slice(0, n), z, x); if (c != 0) {
            addVW(z.slice(n, n + (n >> (int)(1))), z.slice(n), c);
        }
    }
}

// Like karatsubaAdd, but does subtract.
internal static void karatsubaSub(nat z, nat x, nint n) {
    {
        Word c = subVV(z.slice(0, n), z, x); if (c != 0) {
            subVW(z.slice(n, n + (n >> (int)(1))), z.slice(n), c);
        }
    }
}

// Operands that are shorter than karatsubaThreshold are multiplied using
// "grade school" multiplication; for longer operands the Karatsuba algorithm
// is used.
internal static nint karatsubaThreshold = 40; // computed by calibrate_test.go

// karatsuba multiplies x and y and leaves the result in z.
// Both x and y must have the same length n and n must be a
// power of 2. The result vector z must have len(z) >= 6*n.
// The (non-normalized) result is placed in z[0 : 2*n].
internal static void karatsuba(nat z, nat x, nat y) {
    nint n = len(y);
    // Switch to basic multiplication if numbers are odd or small.
    // (n is always even if karatsubaThreshold is even, but be
    // conservative)
    if ((nint)(n & 1) != 0 || n < karatsubaThreshold || n < 2) {
        basicMul(z, x, y);
        return;
    }
    // n&1 == 0 && n >= karatsubaThreshold && n >= 2
    // Karatsuba multiplication is based on the observation that
    // for two numbers x and y with:
    //
    //   x = x1*b + x0
    //   y = y1*b + y0
    //
    // the product x*y can be obtained with 3 products z2, z1, z0
    // instead of 4:
    //
    //   x*y = x1*y1*b*b + (x1*y0 + x0*y1)*b + x0*y0
    //       =    z2*b*b +              z1*b +    z0
    //
    // with:
    //
    //   xd = x1 - x0
    //   yd = y0 - y1
    //
    //   z1 =      xd*yd                    + z2 + z0
    //      = (x1-x0)*(y0 - y1)             + z2 + z0
    //      = x1*y0 - x1*y1 - x0*y0 + x0*y1 + z2 + z0
    //      = x1*y0 -    z2 -    z0 + x0*y1 + z2 + z0
    //      = x1*y0                 + x0*y1
    // split x, y into "digits"
    nint n2 = (n >> (int)(1)); // n2 >= 1
    var (x1, x0) = (x.slice(n2), x.slice(0, n2)); // x = x1*b + y0
    var (y1, y0) = (y.slice(n2), y.slice(0, n2)); // y = y1*b + y0
    // z is used for the result and temporary storage:
    //
    //   6*n     5*n     4*n     3*n     2*n     1*n     0*n
    // z = [z2 copy|z0 copy| xd*yd | yd:xd | x1*y1 | x0*y0 ]
    //
    // For each recursive call of karatsuba, an unused slice of
    // z is passed in that has (at least) half the length of the
    // caller's z.
    // compute z0 and z2 with the result "in place" in z
    karatsuba(z, x0, y0); // z0 = x0*y0
    karatsuba(z.slice(n), x1, y1); // z2 = x1*y1
    // compute xd (or the negative value if underflow occurs)
    nint s = 1; // sign of product xd*yd
    var xd = z.slice(2 * n, 2 * n + n2);
    if (subVV(xd, x1, x0) != 0) {
        // x1-x0
        s = -s;
        subVV(xd, x0, x1); // x0-x1
    }
    // compute yd (or the negative value if underflow occurs)
    var yd = z.slice(2 * n + n2, 3 * n);
    if (subVV(yd, y0, y1) != 0) {
        // y0-y1
        s = -s;
        subVV(yd, y1, y0); // y1-y0
    }
    // p = (x1-x0)*(y0-y1) == x1*y0 - x1*y1 - x0*y0 + x0*y1 for s > 0
    // p = (x0-x1)*(y0-y1) == x0*y0 - x0*y1 - x1*y0 + x1*y1 for s < 0
    var p = z.slice(n * 3);
    karatsuba(p, xd, yd);
    // save original z2:z0
    // (ok to use upper half of z since we're done recurring)
    var r = z.slice(n * 4);
    copy(r, z.slice(0, n * 2));
    // add up all partial products
    //
    //   2*n     n     0
    // z = [ z2  | z0  ]
    //   +    [ z0  ]
    //   +    [ z2  ]
    //   +    [  p  ]
    //
    karatsubaAdd(z.slice(n2), r, n);
    karatsubaAdd(z.slice(n2), r.slice(n), n);
    if (s > 0){
        karatsubaAdd(z.slice(n2), p, n);
    } else {
        karatsubaSub(z.slice(n2), p, n);
    }
}

// alias reports whether x and y share the same base array.
//
// Note: alias assumes that the capacity of underlying arrays
// is never changed for nat values; i.e. that there are
// no 3-operand slice expressions in this code (or worse,
// reflect-based operations to the same effect).
internal static bool alias(nat x, nat y) {
    return cap(x) > 0 && cap(y) > 0 && Ꮡ(x.slice(0, cap(x)), cap(x) - 1) == Ꮡ(y.slice(0, cap(y)), cap(y) - 1);
}

// addAt implements z += x<<(_W*i); z must be long enough.
// (we don't use nat.add because we need z to stay the same
// slice, and we don't need to normalize z after each addition)
internal static void addAt(nat z, nat x, nint i) {
    {
        nint n = len(x); if (n > 0) {
            {
                Word c = addVV(z.slice(i, i + n), z.slice(i), x); if (c != 0) {
                    nint j = i + n;
                    if (j < len(z)) {
                        addVW(z.slice(j), z.slice(j), c);
                    }
                }
            }
        }
    }
}

// karatsubaLen computes an approximation to the maximum k <= n such that
// k = p<<i for a number p <= threshold and an i >= 0. Thus, the
// result is the largest number that can be divided repeatedly by 2 before
// becoming about the value of threshold.
internal static nint karatsubaLen(nint n, nint threshold) {
    nuint i = (nuint)0;
    while (n > threshold) {
        n >>= (int)(1);
        i++;
    }
    return n.Lsh(i);
}

internal static nat mul(this nat z, nat x, nat y) {
    nint m = len(x);
    nint n = len(y);
    switch (ᐧ) {
    case {} when m < n: {
        return z.mul(y, x);
    }
    case {} when m == 0 || n == 0: {
        return z[..0];
    }
    case {} when n is 1: {
        return z.mulAddWW(x, y[0], 0);
    }}

    // m >= n > 1
    // determine if z can be reused
    if (alias(z, x) || alias(z, y)) {
        z = default!; // z is an alias for x or y - cannot reuse
    }
    // use basic multiplication if the numbers are small
    if (n < karatsubaThreshold) {
        z = z.make(m + n);
        basicMul(z, x, y);
        return z.norm();
    }
    // m >= n && n >= karatsubaThreshold && n >= 2
    // determine Karatsuba length k such that
    //
    //   x = xh*b + x0  (0 <= x0 < b)
    //   y = yh*b + y0  (0 <= y0 < b)
    //   b = 1<<(_W*k)  ("base" of digits xi, yi)
    //
    nint k = karatsubaLen(n, karatsubaThreshold);
    // k <= n
    // multiply x0 and y0 via Karatsuba
    var x0 = x.slice(0, k); // x0 is not normalized
    var y0 = y.slice(0, k); // y0 is not normalized
    z = z.make(max(6 * k, m + n)); // enough space for karatsuba of x0*y0 and full result of x*y
    karatsuba(z, x0, y0);
    z = z.slice(0, m + n); // z has final length but may be incomplete
    clear(z.slice(2 * k)); // upper portion of z is garbage (and 2*k <= m+n since k <= n <= m)
    // If xh != 0 or yh != 0, add the missing terms to z. For
    //
    //   xh = xi*b^i + ... + x2*b^2 + x1*b (0 <= xi < b)
    //   yh =                         y1*b (0 <= y1 < b)
    //
    // the missing terms are
    //
    //   x0*y1*b and xi*y0*b^i, xi*y1*b^(i+1) for i > 0
    //
    // since all the yi for i > 1 are 0 by choice of k: If any of them
    // were > 0, then yh >= b^2 and thus y >= b^2. Then k' = k*2 would
    // be a larger valid threshold contradicting the assumption about k.
    //
    if (k < n || m != n) {
        var tp = getNat(3 * k);
        var t = tp.ValueSlot;
        // add x0*y1*b
        var x0Δ1 = x0.norm();
        var y1 = y.slice(k); // y1 is normalized because y is
        t = t.mul(x0Δ1, y1); // update t so we don't lose t's underlying array
        addAt(z, t, k);
        // add xi*y0<<i, xi*y1*b<<(i+k)
        var y0Δ1 = y0.norm();
        for (nint i = k; i < len(x); i += k) {
            var xi = x.slice(i);
            if (len(xi) > k) {
                xi = xi.slice(0, k);
            }
            xi = xi.norm();
            t = t.mul(xi, y0Δ1);
            addAt(z, t, i);
            t = t.mul(xi, y1);
            addAt(z, t, i + k);
        }
        putNat(tp);
    }
    return z.norm();
}

// basicSqr sets z = x*x and is asymptotically faster than basicMul
// by about a factor of 2, but slower for small arguments due to overhead.
// Requirements: len(x) > 0, len(z) == 2*len(x)
// The (non-normalized) result is placed in z.
internal static void basicSqr(nat z, nat x) {
    nint n = len(x);
    var tp = getNat(2 * n);
    var t = tp.ValueSlot; // temporary variable to hold the products
    clear(t);
    (z[1], z[0]) = mulWW(x[0], x[0]); // the initial square
    for (nint i = 1; i < n; i++) {
        Word d = x[i];
        // z collects the squares x[i] * x[i]
        (z[2 * i + 1], z[2 * i]) = mulWW(d, d);
        // t collects the products x[i] * x[j] where j < i
        t[2 * i] = addMulVVW(t.slice(i, 2 * i), x.slice(0, i), d);
    }
    t[2 * n - 1] = shlVU(t.slice(1, 2 * n - 1), t.slice(1, 2 * n - 1), 1); // double the j < i products
    addVV(z, z, t); // combine the result
    putNat(tp);
}

// karatsubaSqr squares x and leaves the result in z.
// len(x) must be a power of 2 and len(z) >= 6*len(x).
// The (non-normalized) result is placed in z[0 : 2*len(x)].
//
// The algorithm and the layout of z are the same as for karatsuba.
internal static void karatsubaSqr(nat z, nat x) {
    nint n = len(x);
    if ((nint)(n & 1) != 0 || n < karatsubaSqrThreshold || n < 2) {
        basicSqr(z.slice(0, 2 * n), x);
        return;
    }
    nint n2 = (n >> (int)(1));
    var (x1, x0) = (x.slice(n2), x.slice(0, n2));
    karatsubaSqr(z, x0);
    karatsubaSqr(z.slice(n), x1);
    // s = sign(xd*yd) == -1 for xd != 0; s == 1 for xd == 0
    var xd = z.slice(2 * n, 2 * n + n2);
    if (subVV(xd, x1, x0) != 0) {
        subVV(xd, x0, x1);
    }
    var p = z.slice(n * 3);
    karatsubaSqr(p, xd);
    var r = z.slice(n * 4);
    copy(r, z.slice(0, n * 2));
    karatsubaAdd(z.slice(n2), r, n);
    karatsubaAdd(z.slice(n2), r.slice(n), n);
    karatsubaSub(z.slice(n2), p, n); // s == -1 for p != 0; s == 1 for p == 0
}

// Operands that are shorter than basicSqrThreshold are squared using
// "grade school" multiplication; for operands longer than karatsubaSqrThreshold
// we use the Karatsuba algorithm optimized for x == y.
internal static nint basicSqrThreshold = 20; // computed by calibrate_test.go

internal static nint karatsubaSqrThreshold = 260; // computed by calibrate_test.go

// z = x*x
internal static nat sqr(this nat z, nat x) {
    nint n = len(x);
    switch (ᐧ) {
    case {} when n is 0: {
        return z[..0];
    }
    case {} when n is 1: {
        Word d = x[0];
        z = z.make(2);
        (z[1], z[0]) = mulWW(d, d);
        return z.norm();
    }}

    if (alias(z, x)) {
        z = default!; // z is an alias for x - cannot reuse
    }
    if (n < basicSqrThreshold) {
        z = z.make(2 * n);
        basicMul(z, x, x);
        return z.norm();
    }
    if (n < karatsubaSqrThreshold) {
        z = z.make(2 * n);
        basicSqr(z, x);
        return z.norm();
    }
    // Use Karatsuba multiplication optimized for x == y.
    // The algorithm and layout of z are the same as for mul.
    // z = (x1*b + x0)^2 = x1^2*b^2 + 2*x1*x0*b + x0^2
    nint k = karatsubaLen(n, karatsubaSqrThreshold);
    var x0 = x.slice(0, k);
    z = z.make(max(6 * k, 2 * n));
    karatsubaSqr(z, x0); // z = x0^2
    z = z.slice(0, 2 * n);
    clear(z.slice(2 * k));
    if (k < n) {
        var tp = getNat(2 * k);
        var t = tp.ValueSlot;
        var x0Δ1 = x0.norm();
        var x1 = x.slice(k);
        t = t.mul(x0Δ1, x1);
        addAt(z, t, k);
        addAt(z, t, k); // z = 2*x1*x0*b + x0^2
        t = t.sqr(x1);
        addAt(z, t, 2 * k); // z = x1^2*b^2 + 2*x1*x0*b + x0^2
        putNat(tp);
    }
    return z.norm();
}

// mulRange computes the product of all the unsigned integers in the
// range [a, b] inclusively. If a > b (empty range), the result is 1.
internal static nat mulRange(this nat z, uint64 a, uint64 b) {
    switch (ᐧ) {
    case {} when a is 0: {
        return z.setUint64(0);
    }
    case {} when a > b: {
        return z.setUint64(1);
    }
    case {} when a == b: {
        return z.setUint64(a);
    }
    case {} when a + 1 == b: {
        return z.mul(((nat)default!).setUint64(a), // cut long ranges short (optimization)
 ((nat)default!).setUint64(b));
    }}

    var m = a + (b - a) / 2; // avoid overflow
    return z.mul(((nat)default!).mulRange(a, m), ((nat)default!).mulRange(m + 1, b));
}

// getNat returns a *nat of len n. The contents may not be zero.
// The pool holds *nat to avoid allocation when converting to interface{}.
internal static ж<nat> getNat(nint n) {
    ж<nat> z = default!;
    {
        var v = ᏑnatPool.Get(); if (v != default!) {
            z = v._<ж<nat>>();
        }
    }
    if (z == nil) {
        z = @new<nat>();
    }
    z.ValueSlot = (~z).make(n);
    if (n > 0) {
        (z.ValueSlot)[0] = 0xfedcb; // break code expecting zero
    }
    return z;
}

internal static void putNat(ж<nat> Ꮡx) {
    ᏑnatPool.Put(Ꮡx.OrTypedNil());
}

internal static ж<sync.Pool> ᏑnatPool = new StandardBox<sync.Pool>(default(sync.Pool));
internal static ref sync.Pool natPool => ref ᏑnatPool.Value;

// bitLen returns the length of x in bits.
// Unlike most methods, it works even if x is not normalized.
internal static nint bitLen(this nat x) {
    // This function is used in cryptographic operations. It must not leak
    // anything but the Int's sign and bit size through side-channels. Any
    // changes must be reviewed by a security expert.
    {
        nint i = len(x) - 1; if (i >= 0) {
            // bits.Len uses a lookup table for the low-order bits on some
            // architectures. Neutralize any input-dependent behavior by setting all
            // bits after the first one bit.
            nuint top = (nuint)x[i];
            top |= (nuint)((top >> (int)(1)));
            top |= (nuint)((top >> (int)(2)));
            top |= (nuint)((top >> (int)(4)));
            top |= (nuint)((top >> (int)(8)));
            top |= (nuint)((top >> (int)(16)));
            top |= (nuint)(((top >> (int)(16)) >> (int)(16))); // ">> 32" doesn't compile on 32-bit architectures
            return i * (nint)_W + bits.Len(top);
        }
    }
    return 0;
}

// trailingZeroBits returns the number of consecutive least significant zero
// bits of x.
internal static nuint trailingZeroBits(this nat x) {
    if (len(x) == 0) {
        return 0;
    }
    nuint i = default!;
    while (x[i] == 0) {
        i++;
    }
    // x[i] != 0
    return i * (nuint)_W + (nuint)bits.TrailingZeros((nuint)x[i]);
}

// isPow2 returns i, true when x == 2**i and 0, false otherwise.
internal static (nuint, bool) isPow2(this nat x) {
    nuint i = default!;
    while (x[i] == 0) {
        i++;
    }
    if (i == (nuint)len(x) - 1 && (Word)(x[i] & (x[i] - 1)) == 0) {
        return (i * (nuint)_W + (nuint)bits.TrailingZeros((nuint)x[i]), true);
    }
    return (0, false);
}

internal static bool same(nat x, nat y) {
    return len(x) == len(y) && len(x) > 0 && Ꮡ(x, 0) == Ꮡ(y, 0);
}

// z = x << s
internal static nat shl(this nat z, nat x, nuint s) {
    if (s == 0) {
        if (same(z, x)) {
            return z;
        }
        if (!alias(z, x)) {
            return z.set(x);
        }
    }
    nint m = len(x);
    if (m == 0) {
        return z[..0];
    }
    // m > 0
    nint n = m + (nint)(s / (nuint)_W);
    z = z.make(n + 1);
    z[n] = shlVU(z.slice(n - m, n), x, s % (nuint)_W);
    clear(z.slice(0, n - m));
    return z.norm();
}

// z = x >> s
internal static nat shr(this nat z, nat x, nuint s) {
    if (s == 0) {
        if (same(z, x)) {
            return z;
        }
        if (!alias(z, x)) {
            return z.set(x);
        }
    }
    nint m = len(x);
    nint n = m - (nint)(s / (nuint)_W);
    if (n <= 0) {
        return z[..0];
    }
    // n > 0
    z = z.make(n);
    shrVU(z, x.slice(m - n), s % (nuint)_W);
    return z.norm();
}

internal static nat setBit(this nat z, nat x, nuint i, nuint b) {
    nint j = (nint)(i / (nuint)_W);
    Word m = (((Word)1) << (int)((i % (nuint)_W)));
    nint n = len(x);
    switch (b) {
    case 0: {
        z = z.make(n);
        copy(z, x);
        if (j >= n) {
            // no need to grow
            return z;
        }
        z[j] &= unchecked((Word)~(Word)(m));
        return z.norm();
    }
    case 1: {
        if (j >= n){
            z = z.make(j + 1);
            clear(z.slice(n));
        } else {
            z = z.make(n);
        }
        copy(z, x);
        z[j] |= (Word)(m);
        return z;
    }}

    // no need to normalize
    throw panic("set bit is not 0 or 1");
}

// bit returns the value of the i'th bit, with lsb == bit 0.
internal static nuint bit(this nat x, nuint i) {
    nuint j = i / (nuint)_W;
    if (j >= (nuint)len(x)) {
        return 0;
    }
    // 0 <= j < len(x)
    return (nuint)((Word)((x[j] >> (int)((i % (nuint)_W))) & 1));
}

// sticky returns 1 if there's a 1 bit within the
// i least significant bits, otherwise it returns 0.
internal static nuint sticky(this nat x, nuint i) {
    nuint j = i / (nuint)_W;
    if (j >= (nuint)len(x)) {
        if (len(x) == 0) {
            return 0;
        }
        return 1;
    }
    // 0 <= j < len(x)
    foreach (var (_, xΔ1) in x.slice(0, (nint)(j))) {
        if (xΔ1 != 0) {
            return 1;
        }
    }
    if ((x[j] << (int)(((nuint)_W - i % (nuint)_W))) != 0) {
        return 1;
    }
    return 0;
}

internal static nat and(this nat z, nat x, nat y) {
    nint m = len(x);
    nint n = len(y);
    if (m > n) {
        m = n;
    }
    // m <= n
    z = z.make(m);
    for (nint i = 0; i < m; i++) {
        z[i] = (Word)(x[i] & y[i]);
    }
    return z.norm();
}

// trunc returns z = x mod 2ⁿ.
internal static nat trunc(this nat z, nat x, nuint n) {
    nuint w = (n + (nuint)_W - 1) / (nuint)_W;
    if ((nuint)len(x) < w) {
        return z.set(x);
    }
    z = z.make((nint)w);
    copy(z, x);
    if (n % (nuint)_W != 0) {
        z[len(z) - 1] &= (Word)((Word)((nuint)1 << (int)((n % (nuint)_W))) - 1);
    }
    return z.norm();
}

internal static nat andNot(this nat z, nat x, nat y) {
    nint m = len(x);
    nint n = len(y);
    if (n > m) {
        n = m;
    }
    // m >= n
    z = z.make(m);
    for (nint i = 0; i < n; i++) {
        z[i] = (Word)(x[i] & ~y[i]);
    }
    copy(z.slice(n, m), x.slice(n, m));
    return z.norm();
}

internal static nat or(this nat z, nat x, nat y) {
    nint m = len(x);
    nint n = len(y);
    var s = x;
    if (m < n) {
        (n, m) = (m, n);
        s = y;
    }
    // m >= n
    z = z.make(m);
    for (nint i = 0; i < n; i++) {
        z[i] = (Word)(x[i] | y[i]);
    }
    copy(z.slice(n, m), s.slice(n, m));
    return z.norm();
}

internal static nat xor(this nat z, nat x, nat y) {
    nint m = len(x);
    nint n = len(y);
    var s = x;
    if (m < n) {
        (n, m) = (m, n);
        s = y;
    }
    // m >= n
    z = z.make(m);
    for (nint i = 0; i < n; i++) {
        z[i] = (Word)(x[i] ^ y[i]);
    }
    copy(z.slice(n, m), s.slice(n, m));
    return z.norm();
}

// random creates a random integer in [0..limit), using the space in z if
// possible. n is the bit length of limit.
internal static nat random(this nat z, ж<rand.Rand> Ꮡrand, nat limit, nint n) {
    ref var randΔ1 = ref Ꮡrand.DerefOrNull();

    if (alias(z, limit)) {
        z = default!; // z is an alias for limit - cannot reuse
    }
    z = z.make(len(limit));
    nuint bitLengthOfMSW = (nuint)(n % (nint)_W);
    if (bitLengthOfMSW == 0) {
        bitLengthOfMSW = _W;
    }
    Word mask = ((Word)((nuint)1 << (int)(bitLengthOfMSW))) - 1;
    while (ᐧ) {
        var exprᴛ1 = _W;
        if (exprᴛ1 == 32) {
            foreach (var (i, _) in z) {
                z[i] = ((Word)(nuint)randΔ1.Uint32());
            }
        }
        else if (exprᴛ1 == 64) {
            foreach (var (i, _) in z) {
                z[i] = (Word)(((Word)(nuint)randΔ1.Uint32()) | (((Word)(nuint)randΔ1.Uint32()) << (int)(32)));
            }
        }
        else { /* default: */
            throw panic("unknown word size");
        }

        z[len(limit) - 1] &= (Word)(mask);
        if (z.cmp(limit) < 0) {
            break;
        }
    }
    return z.norm();
}

// If m != 0 (i.e., len(m) != 0), expNN sets z to x**y mod m;
// otherwise it sets z to x**y. The result is the value of z.
internal static nat expNN(this nat z, nat x, nat y, nat m, bool slow) {
    if (alias(z, x) || alias(z, y)) {
        // We cannot allow in-place modification of x or y.
        z = default!;
    }
    // x**y mod 1 == 0
    if (len(m) == 1 && m[0] == 1) {
        return z.setWord(0);
    }
    // m == 0 || m > 1
    // x**0 == 1
    if (len(y) == 0) {
        return z.setWord(1);
    }
    // y > 0
    // 0**y = 0
    if (len(x) == 0) {
        return z.setWord(0);
    }
    // x > 0
    // 1**y = 1
    if (len(x) == 1 && x[0] == 1) {
        return z.setWord(1);
    }
    // x > 1
    // x**1 == x
    if (len(y) == 1 && y[0] == 1) {
        if (len(m) != 0) {
            return z.rem(x, m);
        }
        return z.set(x);
    }
    // y > 1
    if (len(m) != 0) {
        // We likely end up being as long as the modulus.
        z = z.make(len(m));
        // If the exponent is large, we use the Montgomery method for odd values,
        // and a 4-bit, windowed exponentiation for powers of two,
        // and a CRT-decomposed Montgomery method for the remaining values
        // (even values times non-trivial odd values, which decompose into one
        // instance of each of the first two cases).
        if (len(y) > 1 && !slow) {
            if ((Word)(m[0] & 1) == 1) {
                return z.expNNMontgomery(x, y, m);
            }
            {
                var (logM, ok) = m.isPow2(); if (ok) {
                    return z.expNNWindowed(x, y, logM);
                }
            }
            return z.expNNMontgomeryEven(x, y, m);
        }
    }
    z = z.set(x);
    Word v = y[len(y) - 1]; // v > 0 because y is normalized and y > 0
    nuint shift = nlz(v) + 1;
    v <<= (int)(shift);
    nat q = default!;
    UntypedInt mask = /* 1 << (_W - 1) */ 9223372036854775808;
    // We walk through the bits of the exponent one by one. Each time we
    // see a bit, we square, thus doubling the power. If the bit is a one,
    // we also multiply by x, thus adding one to the power.
    nint w = (nint)_W - (nint)shift;
    // zz and r are used to avoid allocating in mul and div as
    // otherwise the arguments would alias.
    nat zz = default!;
    nat r = default!;
    for (nint j = 0; j < w; j++) {
        zz = zz.sqr(z);
        (zz, z) = (z, zz);
        if ((Word)(v & (nuint)mask) != 0) {
            zz = zz.mul(z, x);
            (zz, z) = (z, zz);
        }
        if (len(m) != 0) {
            (zz, r) = zz.div(r, z, m);
            (zz, r, q, z) = (q, z, zz, r);
        }
        v <<= (int)(1);
    }
    for (nint i = len(y) - 2; i >= 0; i--) {
        v = y[i];
        for (nint j = 0; j < _W; j++) {
            zz = zz.sqr(z);
            (zz, z) = (z, zz);
            if ((Word)(v & (nuint)mask) != 0) {
                zz = zz.mul(z, x);
                (zz, z) = (z, zz);
            }
            if (len(m) != 0) {
                (zz, r) = zz.div(r, z, m);
                (zz, r, q, z) = (q, z, zz, r);
            }
            v <<= (int)(1);
        }
    }
    return z.norm();
}

// expNNMontgomeryEven calculates x**y mod m where m = m1 × m2 for m1 = 2ⁿ and m2 odd.
// It uses two recursive calls to expNN for x**y mod m1 and x**y mod m2
// and then uses the Chinese Remainder Theorem to combine the results.
// The recursive call using m1 will use expNNWindowed,
// while the recursive call using m2 will use expNNMontgomery.
// For more details, see Ç. K. Koç, “Montgomery Reduction with Even Modulus”,
// IEE Proceedings: Computers and Digital Techniques, 141(5) 314-316, September 1994.
// http://www.people.vcu.edu/~jwang3/CMSC691/j34monex.pdf
internal static nat expNNMontgomeryEven(this nat z, nat x, nat y, nat m) {
    // Split m = m₁ × m₂ where m₁ = 2ⁿ
    nuint n = m.trailingZeroBits();
    var m1 = ((nat)default!).shl(natOne, n);
    var m2 = ((nat)default!).shr(m, n);
    // We want z = x**y mod m.
    // z₁ = x**y mod m1 = (x**y mod m) mod m1 = z mod m1
    // z₂ = x**y mod m2 = (x**y mod m) mod m2 = z mod m2
    // (We are using the math/big convention for names here,
    // where the computation is z = x**y mod m, so its parts are z1 and z2.
    // The paper is computing x = a**e mod n; it refers to these as x2 and z1.)
    var z1 = ((nat)default!).expNN(x, y, m1, false);
    var z2 = ((nat)default!).expNN(x, y, m2, false);
    // Reconstruct z from z₁, z₂ using CRT, using algorithm from paper,
    // which uses only a single modInverse (and an easy one at that).
    //	p = (z₁ - z₂) × m₂⁻¹ (mod m₁)
    //	z = z₂ + p × m₂
    // The final addition is in range because:
    //	z = z₂ + p × m₂
    //	  ≤ z₂ + (m₁-1) × m₂
    //	  < m₂ + (m₁-1) × m₂
    //	  = m₁ × m₂
    //	  = m.
    z = z.set(z2);
    // Compute (z₁ - z₂) mod m1 [m1 == 2**n] into z1.
    z1 = z1.subMod2N(z1, z2, n);
    // Reuse z2 for p = (z₁ - z₂) [in z1] * m2⁻¹ (mod m₁ [= 2ⁿ]).
    var m2inv = ((nat)default!).modInverse(m2, m1);
    z2 = z2.mul(z1, m2inv);
    z2 = z2.trunc(z2, n);
    // Reuse z1 for p * m2.
    z = z.add(z, z1.mul(z2, m2));
    return z;
}

// expNNWindowed calculates x**y mod m using a fixed, 4-bit window,
// where m = 2**logM.
internal static nat expNNWindowed(this nat z, nat x, nat y, nuint logM) {
    if (len(y) <= 1) {
        throw panic("big: misuse of expNNWindowed");
    }
    if ((Word)(x[0] & 1) == 0) {
        // len(y) > 1, so y  > logM.
        // x is even, so x**y is a multiple of 2**y which is a multiple of 2**logM.
        return z.setWord(0);
    }
    if (logM == 1) {
        return z.setWord(1);
    }
    // zz is used to avoid allocating in mul as otherwise
    // the arguments would alias.
    nint w = (nint)((logM + (nuint)_W - 1) / (nuint)_W);
    var zzp = getNat(w);
    var zz = zzp.ValueSlot;
    UntypedInt n = 4;
    // powers[i] contains x^i.
    array<ж<nat>> powers = new(16); /* (1 << (int)(n)) */
    foreach (var (iΔ1, _) in powers) {
        powers[iΔ1] = getNat(w);
    }
    powers[0].ValueSlot = (~powers[0]).set(natOne);
    powers[1].ValueSlot = (~powers[1]).trunc(x, logM);
    for (nint iΔ2 = 2; iΔ2 < (nint)((1 << (int)(n))); iΔ2 += 2) {
        var (p2, p, p1) = (powers[iΔ2 / 2], powers[iΔ2], powers[iΔ2 + 1]);
        p.ValueSlot = (~p).sqr(p2.ValueSlot);
        p.ValueSlot = (~p).trunc(p.ValueSlot, logM);
        p1.ValueSlot = (~p1).mul(p.ValueSlot, x);
        p1.ValueSlot = (~p1).trunc(p1.ValueSlot, logM);
    }
    // Because phi(2**logM) = 2**(logM-1), x**(2**(logM-1)) = 1,
    // so we can compute x**(y mod 2**(logM-1)) instead of x**y.
    // That is, we can throw away all but the bottom logM-1 bits of y.
    // Instead of allocating a new y, we start reading y at the right word
    // and truncate it appropriately at the start of the loop.
    nint i = len(y) - 1;
    nint mtop = (nint)((logM - 2) / (nuint)_W); // -2 because the top word of N bits is the (N-1)/W'th word.
    Word mmask = ~((Word)((Word)0));
    {
        nuint mbits = (nuint)((logM - 1) & (nuint)(_W - 1)); if (mbits != 0) {
            mmask = ((Word)((nuint)1 << (int)(mbits))) - 1;
        }
    }
    if (i > mtop) {
        i = mtop;
    }
    var advance = false;
    z = z.setWord(1);
    for (; i >= 0; i--) {
        Word yi = y[i];
        if (i == mtop) {
            yi &= (Word)(mmask);
        }
        for (nint j = 0; j < _W; j += n) {
            if (advance) {
                // Account for use of 4 bits in previous iteration.
                // Unrolled loop for significant performance
                // gain. Use go test -bench=".*" in crypto/rsa
                // to check performance before making changes.
                zz = zz.sqr(z);
                (zz, z) = (z, zz);
                z = z.trunc(z, logM);
                zz = zz.sqr(z);
                (zz, z) = (z, zz);
                z = z.trunc(z, logM);
                zz = zz.sqr(z);
                (zz, z) = (z, zz);
                z = z.trunc(z, logM);
                zz = zz.sqr(z);
                (zz, z) = (z, zz);
                z = z.trunc(z, logM);
            }
            zz = zz.mul(z, powers[(yi >> (int)((_W - n)))].ValueSlot);
            (zz, z) = (z, zz);
            z = z.trunc(z, logM);
            yi <<= (int)(n);
            advance = true;
        }
    }
    zzp.ValueSlot = zz;
    putNat(zzp);
    foreach (var (iΔ3, _) in powers) {
        putNat(powers[iΔ3]);
    }
    return z.norm();
}

// expNNMontgomery calculates x**y mod m using a fixed, 4-bit window.
// Uses Montgomery representation.
internal static nat expNNMontgomery(this nat z, nat x, nat y, nat m) {
    nint numWords = len(m);
    // We want the lengths of x and m to be equal.
    // It is OK if x >= m as long as len(x) == len(m).
    if (len(x) > numWords) {
        (_, x) = ((nat)default!).div(default!, x, m);
    }
    // Note: now len(x) <= numWords, not guaranteed ==.
    if (len(x) < numWords) {
        var rr = new nat(numWords);
        copy(rr, x);
        x = rr;
    }
    // Ideally the precomputations would be performed outside, and reused
    // k0 = -m**-1 mod 2**_W. Algorithm from: Dumas, J.G. "On Newton–Raphson
    // Iteration for Multiplicative Inverses Modulo Prime Powers".
    Word k0 = 2 - m[0];
    Word t = m[0] - 1;
    for (nint i = 1; i < _W; i <<= (int)(1)) {
        t *= t;
        k0 *= (t + 1);
    }
    k0 = ((Word)0 - k0);
    // RR = 2**(2*_W*len(m)) mod m
    var RR = ((nat)default!).setWord(1);
    var zz = ((nat)default!).shl(RR, (nuint)(2 * numWords * (nint)_W));
    (_, RR) = ((nat)default!).div(RR, zz, m);
    if (len(RR) < numWords) {
        zz = zz.make(numWords);
        copy(zz, RR);
        RR = zz;
    }
    // one = 1, with equal length to that of m
    var one = new nat(numWords);
    one[0] = 1;
    UntypedInt n = 4;
    // powers[i] contains x^i
    array<nat> powers = new(16); /* (1 << (int)(n)) */
    powers[0] = powers[0].montgomery(one, RR, m, k0, numWords);
    powers[1] = powers[1].montgomery(x, RR, m, k0, numWords);
    for (nint i = 2; i < (nint)((1 << (int)(n))); i++) {
        powers[i] = powers[i].montgomery(powers[i - 1], powers[1], m, k0, numWords);
    }
    // initialize z = 1 (Montgomery 1)
    z = z.make(numWords);
    copy(z, powers[0]);
    zz = zz.make(numWords);
    // same windowed exponent, but with Montgomery multiplications
    for (nint i = len(y) - 1; i >= 0; i--) {
        Word yi = y[i];
        for (nint j = 0; j < _W; j += n) {
            if (i != len(y) - 1 || j != 0) {
                zz = zz.montgomery(z, z, m, k0, numWords);
                z = z.montgomery(zz, zz, m, k0, numWords);
                zz = zz.montgomery(z, z, m, k0, numWords);
                z = z.montgomery(zz, zz, m, k0, numWords);
            }
            zz = zz.montgomery(z, powers[(yi >> (int)((_W - n)))], m, k0, numWords);
            (z, zz) = (zz, z);
            yi <<= (int)(n);
        }
    }
    // convert to regular number
    zz = zz.montgomery(z, one, m, k0, numWords);
    // One last reduction, just in case.
    // See golang.org/issue/13907.
    if (zz.cmp(m) >= 0) {
        // Common case is m has high bit set; in that case,
        // since zz is the same length as m, there can be just
        // one multiple of m to remove. Just subtract.
        // We think that the subtract should be sufficient in general,
        // so do that unconditionally, but double-check,
        // in case our beliefs are wrong.
        // The div is not expected to be reached.
        zz = zz.sub(zz, m);
        if (zz.cmp(m) >= 0) {
            (_, zz) = ((nat)default!).div(default!, zz, m);
        }
    }
    return zz.norm();
}

// bytes writes the value of z into buf using big-endian encoding.
// The value of z is encoded in the slice buf[i:]. If the value of z
// cannot be represented in buf, bytes panics. The number i of unused
// bytes at the beginning of buf is returned as result.
internal static nint /*i*/ bytes(this nat z, slice<byte> buf) {
    nint i = default!;

    // This function is used in cryptographic operations. It must not leak
    // anything but the Int's sign and bit size through side-channels. Any
    // changes must be reviewed by a security expert.
    i = len(buf);
    foreach (var (_, vᴛ1) in z) {
        var d = vᴛ1;

        for (nint j = 0; j < _S; j++) {
            i--;
            if (i >= 0){
                buf[i] = (byte)(nuint)d;
            } else 
            if ((byte)(nuint)d != 0) {
                throw panic("math/big: buffer too small to fit value");
            }
            d >>= (int)(8);
        }
    }
    if (i < 0) {
        i = 0;
    }
    while (i < len(buf) && buf[i] == 0) {
        i++;
    }
    return i;
}

// bigEndianWord returns the contents of buf interpreted as a big-endian encoded Word value.
internal static Word bigEndianWord(slice<byte> buf) {
    if (_W == 64) {
        return ((Word)(nuint)byteorder.BEUint64(buf));
    }
    return ((Word)(nuint)byteorder.BEUint32(buf));
}

// setBytes interprets buf as the bytes of a big-endian unsigned
// integer, sets z to that value, and returns z.
internal static nat setBytes(this nat z, slice<byte> buf) {
    z = z.make((len(buf) + (nint)_S - 1) / (nint)_S);
    nint i = len(buf);
    for (nint k = 0; i >= _S; k++) {
        z[k] = bigEndianWord(buf.slice(i - (nint)_S, i));
        i -= _S;
    }
    if (i > 0) {
        Word d = default!;
        for (nuint s = (nuint)0; i > 0; s += 8) {
            d |= (Word)((((Word)(nuint)buf[i - 1]) << (int)(s)));
            i--;
        }
        z[len(z) - 1] = d;
    }
    return z.norm();
}

// sqrt sets z = ⌊√x⌋
internal static nat sqrt(this nat z, nat x) {
    if (x.cmp(natOne) <= 0) {
        return z.set(x);
    }
    if (alias(z, x)) {
        z = default!;
    }
    // Start with value known to be too large and repeat "z = ⌊(z + ⌊x/z⌋)/2⌋" until it stops getting smaller.
    // See Brent and Zimmermann, Modern Computer Arithmetic, Algorithm 1.13 (SqrtInt).
    // https://members.loria.fr/PZimmermann/mca/pub226.html
    // If x is one less than a perfect square, the sequence oscillates between the correct z and z+1;
    // otherwise it converges to the correct z and stays there.
    nat z1 = default!;
    nat z2 = default!;
    z1 = z;
    z1 = z1.setUint64(1);
    z1 = z1.shl(z1, (nuint)(x.bitLen() + 1) / 2); // must be ≥ √x
    for (nint n = 0; ᐧ ; n++) {
        (z2, _) = z2.div(default!, x, z1);
        z2 = z2.add(z2, z1);
        z2 = z2.shr(z2, 1);
        if (z2.cmp(z1) >= 0) {
            // z1 is answer.
            // Figure out whether z1 or z2 is currently aliased to z by looking at loop count.
            if ((nint)(n & 1) == 0) {
                return z1;
            }
            return z.set(z1);
        }
        (z1, z2) = (z2, z1);
    }
}

// subMod2N returns z = (x - y) mod 2ⁿ.
internal static nat subMod2N(this nat z, nat x, nat y, nuint n) {
    if ((nuint)x.bitLen() > n) {
        if (alias(z, x)){
            // ok to overwrite x in place
            x = x.trunc(x, n);
        } else {
            x = ((nat)default!).trunc(x, n);
        }
    }
    if ((nuint)y.bitLen() > n) {
        if (alias(z, y)){
            // ok to overwrite y in place
            y = y.trunc(y, n);
        } else {
            y = ((nat)default!).trunc(y, n);
        }
    }
    if (x.cmp(y) >= 0) {
        return z.sub(x, y);
    }
    // x - y < 0; x - y mod 2ⁿ = x - y + 2ⁿ = 2ⁿ - (y - x) = 1 + 2ⁿ-1 - (y - x) = 1 + ^(y - x).
    z = z.sub(y, x);
    while ((nuint)len(z) * (nuint)_W < n) {
        z = append(z, (Word)(0));
    }
    foreach (var (i, _) in z) {
        z[i] = ~z[i];
    }
    z = z.trunc(z, n);
    return z.add(z, natOne);
}

} // end big_package
