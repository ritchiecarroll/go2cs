// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Multiplication.
namespace go.math;

partial class big_package {

// Operands that are shorter than karatsubaThreshold are multiplied using
// "grade school" multiplication; for longer operands the Karatsuba algorithm
// is used.
internal static ж<nint> ᏑkaratsubaThreshold = new StaticBox<nint>(40);
internal static ref nint karatsubaThreshold => ref ᏑkaratsubaThreshold.Value; // see calibrate_test.go

// mul sets z = x*y, using stk for temporary storage.
// The caller may pass stk == nil to request that mul obtain and release one itself.
internal static Δnat mul(this Δnat z, ж<stack> Ꮡstk, Δnat x, Δnat y) {
    GoFrame ᒐ = default;
    try {
        ref var stk = ref Ꮡstk.DerefOrNull();

        nint m = len(x);
        nint n = len(y);

        switch (ᐧ) {
        case {} when m < n: {
            return z.mul(Ꮡstk, y, x);
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
        z = z.make(m + n);

        // use basic multiplication if the numbers are small
        if (n < karatsubaThreshold) {
            basicMul(z, x, y);
            return z.norm();
        }

        if (Ꮡstk == nil) {
            Ꮡstk = getStack(); stk = ref Ꮡstk.DerefOrNull();
            defer(Ꮡstk.free, ref ᒐ);
        }

        // Let x = x1:x0 where x0 is the same length as y.
        // Compute z = x0*y and then add in x1*y in sections
        // if needed.
        karatsuba(Ꮡstk, z.slice(0, 2 * n), x.slice(0, n), y);

        if (n < m) {
            clear(z.slice(2 * n));
            defer(Ꮡstk.restore, Ꮡstk.Value.save(), ref ᒐ);
            var t = stk.nat(2 * n);
            for (nint i = n; i < m; i += n) {
                t = t.mul(Ꮡstk, x.slice(i, min(i + n, len(x))), y);
                addTo(z.slice(i), t);
            }
        }

        return z.norm();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// Operands that are shorter than basicSqrThreshold are squared using
// "grade school" multiplication; for operands longer than karatsubaSqrThreshold
// we use the Karatsuba algorithm optimized for x == y.
internal static ж<nint> ᏑbasicSqrThreshold = new StaticBox<nint>(12);
internal static ref nint basicSqrThreshold => ref ᏑbasicSqrThreshold.Value; // see calibrate_test.go

internal static ж<nint> ᏑkaratsubaSqrThreshold = new StaticBox<nint>(80);
internal static ref nint karatsubaSqrThreshold => ref ᏑkaratsubaSqrThreshold.Value; // see calibrate_test.go

// sqr sets z = x*x, using stk for temporary storage.
// The caller may pass stk == nil to request that sqr obtain and release one itself.
internal static Δnat sqr(this Δnat z, ж<stack> Ꮡstk, Δnat x) {
    GoFrame ᒐ = default;
    try {
        ref var stk = ref Ꮡstk.DerefOrNull();

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
        z = z.make(2 * n);

        if (n < basicSqrThreshold && n < karatsubaSqrThreshold) {
            basicMul(z, x, x);
            return z.norm();
        }

        if (Ꮡstk == nil) {
            Ꮡstk = getStack(); stk = ref Ꮡstk.DerefOrNull();
            defer(Ꮡstk.free, ref ᒐ);
        }

        if (n < karatsubaSqrThreshold) {
            basicSqr(Ꮡstk, z, x);
            return z.norm();
        }

        karatsubaSqr(Ꮡstk, z, x);
        return z.norm();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// basicSqr sets z = x*x and is asymptotically faster than basicMul
// by about a factor of 2, but slower for small arguments due to overhead.
// Requirements: len(x) > 0, len(z) == 2*len(x)
// The (non-normalized) result is placed in z.
internal static void basicSqr(ж<stack> Ꮡstk, Δnat z, Δnat x) {
    GoFrame ᒐ = default;
    try {
        ref var stk = ref Ꮡstk.DerefOrNull();

        nint n = len(x);
        if (n < basicSqrThreshold) {
            basicMul(z, x, x);
            return;
        }

        defer(Ꮡstk.restore, Ꮡstk.Value.save(), ref ᒐ);
        var t = stk.nat(2 * n);
        clear(t);
        (z[1], z[0]) = mulWW(x[0], x[0]); // the initial square
        for (nint i = 1; i < n; i++) {
            Word d = x[i];
            // z collects the squares x[i] * x[i]
            (z[2 * i + 1], z[2 * i]) = mulWW(d, d);
            // t collects the products x[i] * x[j] where j < i
            t[2 * i] = addMulVVWW(t.slice(i, 2 * i), t.slice(i, 2 * i), x.slice(0, i), d, 0);
        }
        t[2 * n - 1] = lshVU(t.slice(1, 2 * n - 1), t.slice(1, 2 * n - 1), 1); // double the j < i products
        addVV(z, z, t); // combine the result
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// mulAddWW returns z = x*y + r.
internal static Δnat mulAddWW(this Δnat z, Δnat x, Word y, Word r) {
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
internal static void basicMul(Δnat z, Δnat x, Δnat y) {
    clear(z.slice(0, len(x) + len(y))); // initialize z
    foreach (var (i, d) in y) {
        if (d != 0) {
            z[len(x) + i] = addMulVVWW(z.slice(i, i + len(x)), z.slice(i, i + len(x)), x, d, 0);
        }
    }
}

// karatsuba multiplies x and y,
// writing the (non-normalized) result to z.
// x and y must have the same length n,
// and z must have length twice that.
internal static void karatsuba(ж<stack> Ꮡstk, Δnat z, Δnat x, Δnat y) {
    GoFrame ᒐ = default;
    try {
        ref var stk = ref Ꮡstk.DerefOrNull();

        nint n = len(y);
        if (len(x) != n || len(z) != 2 * n) {
            throw panic("bad karatsuba length");
        }

        // Fall back to basic algorithm if small enough.
        if (n < karatsubaThreshold || n < 2) {
            basicMul(z, x, y);
            return;
        }

        // Let the notation x1:x0 denote the nat (x1<<N)+x0 for some N,
        // and similarly z2:z1:z0 = (z2<<2N)+(z1<<N)+z0.
        //
        // (Note that z0, z1, z2 might be ≥ 2**N, in which case the high
        // bits of, say, z0 are being added to the low bits of z1 in this notation.)
        //
        // Karatsuba multiplication is based on the observation that
        //
        //	x1:x0 * y1:y0 = x1*y1:(x0*y1+y0*x1):x0*y0
        //	              = x1*y1:((x0-x1)*(y1-y0)+x1*y1+x0*y0):x0*y0
        //
        // The second form uses only three half-width multiplications
        // instead of the four that the straightforward first form does.
        //
        // We call the three pieces z0, z1, z2:
        //
        //	z0 = x0*y0
        //	z2 = x1*y1
        //	z1 = (x0-x1)*(y1-y0) + z0 + z2

        nint n2 = (n + 1) / 2;
        var (x0, x1) = (Ꮡ(new ΔInt(abs: x.slice(0, n2).norm())), Ꮡ(new ΔInt(abs: x.slice(n2).norm())));
        var (y0, y1) = (Ꮡ(new ΔInt(abs: y.slice(0, n2).norm())), Ꮡ(new ΔInt(abs: y.slice(n2).norm())));
        var z0 = Ꮡ(new ΔInt(abs: z.slice(0, 2 * n2)));
        var z2 = Ꮡ(new ΔInt(abs: z.slice(2 * n2)));

        // Allocate temporary storage for z1; repurpose z0 to hold tx and ty.
        defer(Ꮡstk.restore, Ꮡstk.Value.save(), ref ᒐ);
        var z1 = Ꮡ(new ΔInt(abs: stk.nat(2 * n2 + 1)));
        var tx = Ꮡ(new ΔInt(abs: z.slice(0, n2)));
        var ty = Ꮡ(new ΔInt(abs: z.slice(n2, 2 * n2)));

        tx.Sub(x0, x1);
        ty.Sub(y1, y0);
        z1.mul(Ꮡstk, tx, ty);

        clear(z);
        z0.mul(Ꮡstk, x0, y0);
        z2.mul(Ꮡstk, x1, y1);
        z1.Add(z1, z0);
        z1.Add(z1, z2);
        addTo(z.slice(n2), (~z1).abs);

        // Debug mode: double-check answer and print trace on failure.
        const bool debug = false;
        if (debug) {
            var zz = new Δnat(len(z));
            basicMul(zz, x, y);
            if (z.cmp(zz) != 0) {
                // All the temps were aliased to z and gone. Recompute.
                z0 = @new<ΔInt>();
                z0.mul(Ꮡstk, x0, y0);
                tx = @new<ΔInt>().Sub(x1, x0);
                ty = @new<ΔInt>().Sub(y0, y1);
                z2 = @new<ΔInt>();
                z2.mul(Ꮡstk, x1, y1);
                print((@string)"karatsuba wrong\n"u8);
                trace("x "u8, Ꮡ(new ΔInt(abs: x)));
                trace("y "u8, Ꮡ(new ΔInt(abs: y)));
                trace("z "u8, Ꮡ(new ΔInt(abs: z)));
                trace("zz"u8, Ꮡ(new ΔInt(abs: zz)));
                trace("x0"u8, x0);
                trace("x1"u8, x1);
                trace("y0"u8, y0);
                trace("y1"u8, y1);
                trace("tx"u8, tx);
                trace("ty"u8, ty);
                trace("z0"u8, z0);
                trace("z1"u8, z1);
                trace("z2"u8, z2);
                throw panic("karatsuba");
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// karatsubaSqr squares x,
// writing the (non-normalized) result to z.
// z must have length 2*len(x).
// It is analogous to [karatsuba] but can run faster
// knowing both multiplicands are the same value.
internal static void karatsubaSqr(ж<stack> Ꮡstk, Δnat z, Δnat x) {
    GoFrame ᒐ = default;
    try {
        ref var stk = ref Ꮡstk.DerefOrNull();

        nint n = len(x);
        if (len(z) != 2 * n) {
            throw panic("bad karatsubaSqr length");
        }

        if (n < karatsubaSqrThreshold || n < 2) {
            basicSqr(Ꮡstk, z, x);
            return;
        }

        // Recall that for karatsuba we want to compute:
        //
        //	x1:x0 * y1:y0 = x1y1:(x0y1+y0x1):x0y0
        //                = x1y1:((x0-x1)*(y1-y0)+x1y1+x0y0):x0y0
        //	              = z2:z1:z0
        // where:
        //
        //	z0 = x0y0
        //	z2 = x1y1
        //	z1 = (x0-x1)*(y1-y0) + z0 + z2
        //
        // When x = y, these simplify to:
        //
        //	z0 = x0²
        //	z2 = x1²
        //	z1 = z0 + z2 - (x0-x1)²

        nint n2 = (n + 1) / 2;
        var (x0, x1) = (Ꮡ(new ΔInt(abs: x.slice(0, n2).norm())), Ꮡ(new ΔInt(abs: x.slice(n2).norm())));
        var z0 = Ꮡ(new ΔInt(abs: z.slice(0, 2 * n2)));
        var z2 = Ꮡ(new ΔInt(abs: z.slice(2 * n2)));

        // Allocate temporary storage for z1; repurpose z0 to hold tx.
        defer(Ꮡstk.restore, Ꮡstk.Value.save(), ref ᒐ);
        var z1 = Ꮡ(new ΔInt(abs: stk.nat(2 * n2 + 1)));
        var tx = Ꮡ(new ΔInt(abs: z.slice(0, n2)));

        tx.Sub(x0, x1);
        z1.Value.abs = (~z1).abs.sqr(Ꮡstk, (~tx).abs);
        z1.Value.neg = true;

        clear(z);
        z0.Value.abs = (~z0).abs.sqr(Ꮡstk, (~x0).abs);
        z2.Value.abs = (~z2).abs.sqr(Ꮡstk, (~x1).abs);
        z1.Add(z1, z0);
        z1.Add(z1, z2);
        addTo(z.slice(n2), (~z1).abs);

        // Debug mode: double-check answer and print trace on failure.
        const bool debug = false;
        if (debug) {
            var zz = new Δnat(len(z));
            basicSqr(Ꮡstk, zz, x);
            if (z.cmp(zz) != 0) {
                // All the temps were aliased to z and gone. Recompute.
                tx = @new<ΔInt>().Sub(x0, x1);
                z0 = @new<ΔInt>().Mul(x0, x0);
                z2 = @new<ΔInt>().Mul(x1, x1);
                z1 = @new<ΔInt>().Mul(tx, tx);
                z1.Neg(z1);
                z1.Add(z1, z0);
                z1.Add(z1, z2);
                print((@string)"karatsubaSqr wrong\n"u8);
                trace("x "u8, Ꮡ(new ΔInt(abs: x)));
                trace("z "u8, Ꮡ(new ΔInt(abs: z)));
                trace("zz"u8, Ꮡ(new ΔInt(abs: zz)));
                trace("x0"u8, x0);
                trace("x1"u8, x1);
                trace("z0"u8, z0);
                trace("z1"u8, z1);
                trace("z2"u8, z2);
                throw panic("karatsubaSqr");
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// ifmt returns the debug formatting of the Int x: 0xHEX.
internal static @string ifmt(ж<ΔInt> Ꮡx) {
    @string neg = ""u8;
    @string s = Ꮡx.Text(16);
    @string t = ""u8;
    if (s == ""u8) { // happens for denormalized zero
        s = "0x0"u8;
    }
    if (s[0] == (rune)'-') {
        (neg, s) = ("-", s[1..]);
    }

    // Add _ between words.
    const nint D = /* _W / 4 */ 16; // digits per chunk
    while (len(s) > D) {
        (s, t) = (s.slice(0, len(s) - D), s.slice(len(s) - D) + "_" + t);
    }
    return neg + s + t;
}

// trace prints a single debug value.
internal static void trace(@string name, ж<ΔInt> Ꮡx) {
    print(name, (@string)"="u8, ifmt(Ꮡx), (@string)"\n"u8);
}

} // end big_package
